////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

using System;
using System.Collections; // ArrayList
using System.Threading;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    /// <summary>
    /// This interface defines callbacks from the routing table to the network layer to handle certain events
    /// </summary>
    public interface IRoutingTableCallbacks
    {
        /// <summary>
        /// trigger route request from local node
        /// </summary>
        /// <param name="target">target node</param>
        void RouteRequest(UInt16 target);

        /// <summary>
        /// trigger data transmission from local node
        /// </summary>
        /// <param name="targetAddr">target node</param>
        /// <param name="nextHopAddr">next hop address</param>
        /// <param name="sdu">data to send</param>
        /// <param name="sduHandle">handle to use in confirmation message</param>
        /// <param name="handler">confirmation handler</param>
        /// <param name="isControlMsg">true is message is control message, so no additional header will be added</param>
        void DataRequest(
            UInt16 targetAddr,
            UInt16 nextHopAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler,
            bool isControlMsg);

        /// <summary>
        /// discard queued data at local node
        /// </summary>
        /// <param name="target">target node of message</param>
        /// <param name="sduHandle">sdu handle of message</param>
        /// <param name="handler">confirmation handler provided for message</param>
        /// <param name="isControlMsg">true if message is control message</param>
        void DataRequestTimeout(
            UInt16 target,
            Byte sduHandle,
            DataConfirmHandler handler,
            bool isControlMsg);
    }

    /// <summary>
    /// This class implements the DYMO routing table.
    /// In addition to the pure routing, the table is used to:
    /// - queue sdu for nodes while waiting for route lookup
    /// </summary>
    public class RoutingTable : IDisposable
    {
        private const int cInfiniteTimeout = int.MaxValue; // infinite timeout (signed: WaitOne expects signed int)
        private const int cRouteTimeout = 10 * 60 * 1000; // 10 minutes
        private const int cRouteRequestWaitTime = 1000; // 1 sec
        private const int cRouteRequestRetryCnt = 1; // one retry, i.e. two total attempts
        private const UInt16 cInvalidAddr = Routing.cInvalidShortAddr;
        private const int cMaxPendingMsg = 32; // max nr of pending messages per entry

        /// <summary>
        /// This structure represents an entry in the routing table.
        /// Each entry is uniquely identified by the targetAddr address.
        /// The targetAddr address can be either a short addr, an extended address or both.
        /// No two entries can have the same targetAddr addresses.
        /// When searching for a route to a device, an entry is immediatly created to store the retry counter and queued messages.
        ///
        /// </summary>
        private class Entry // ref-type
        {
            public UInt16 targetAddr; // must be valid
            public UInt16 nextHopAddr; // may be cInvalidAddr
            public byte seqNo; // DYMO seqNo, initially set to 0
            public byte hopCount; // distance to targetAddr, 1 for direct neighbor
            public byte minLqi; // worst lqi on path
            public bool routePending; // route request is pending
            public bool routeInUse; // if true, route will be refreshed instead of deleted on timeout
            public bool routeHasError; // error occured sending to next hop
            public int timeOutSet; // delete timeout in ms from now. if routePending, timeout refers to pending route request
            public int timeOutLeft; // set by timerThread. ms left before timeout hits. To set a new timer, set timeOutSet to nonzero
            // when queuing sdus while requesting a route, they are kept here
            public int requestRetryCnt; // if>0, timeout will not delete entry but repeat route request
            public ArrayList pendingMessages; // queued messages to be sent as soon as a route exists

            public bool broadcastSeqNoValid;
            public byte broadcastSeqNo;
            public UInt32 broadcastWindow; // bitmap for 32 entries before broadcastSeqNo, bit 0 corresponds to broadcastSeqNo-1
            public bool broadcastActive; // if true, routing entry will not be deleted

            public Entry()
            {
                pendingMessages = new ArrayList();
            }
        }

        // container for messages to be sent as soon as a route exists
        private struct PendingMessage
        {
            public Frame sdu;
            public Byte sduHandle;
            public DataConfirmHandler handler;
            public bool isControlMsg;
        }

        private object _lock; // to lock routing table from concurrent access
        private IRoutingTableCallbacks _routing; // callback to network layer
        private ArrayList _table; // actual routing table
        private Thread _timerThread;          // A thread that handles timeouts in the routing table
        private AutoResetEvent _timerEvent;   // An event to signal a changed timer
        private bool _timerThreadExit;        // A flag that tells the receive thread to exit

        public RoutingTable(IRoutingTableCallbacks routing)
        {
            _lock = new object();
            _routing = routing;
            _table = new ArrayList();
            _timerThread = new Thread(TimerThread);
#if !MICROFRAMEWORK
            _timerThread.IsBackground = true;
#endif
            _timerEvent = new AutoResetEvent(false);
            _timerThreadExit = false;
            _timerThread.Start();
        }

        public virtual void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _timerThreadExit = true;
            if (_timerThread != null)
            {
                _timerEvent.Set();
                _timerThread.Join();
                _timerThread = null;
            }
        }

        /// <summary>
        /// Updates timeouts in routing table. Deletes entries with expired deletion timer.
        /// After changing some timer externally, set _timerEvent
        /// </summary>
        private void TimerThread()
        {
            int nextTimeout = int.MaxValue;
            while (true)
            {
                DateTime start = DateTime.Now;
                if (nextTimeout == cInfiniteTimeout)
                    _timerEvent.WaitOne();
                else
                    _timerEvent.WaitOne((int)nextTimeout, false);

                if (_timerThreadExit)
                    break;

                TimeSpan tsElapsed = DateTime.Now - start;
                int timeElapsed = tsElapsed.Milliseconds + 1000 * (tsElapsed.Seconds + 60 * tsElapsed.Minutes);
                // max wait time is in minutes, less than an hour

                lock (_lock)
                {
                    // each entry gets checked periodically after cRouteTimeout:
                    // if neither "routeInUse" nor "broadcastActive" is set, entry is deleted
                    // else: flags are reset and timer gets restarted
                    // if "routeInUse" is true, a route request is performed to refresh the route
                    // else: the next hop is set to invalid
                    nextTimeout = cInfiniteTimeout;
                    int i = 0;
                    while (i < _table.Count)
                    {
                        Entry entry = _table[i] as Entry;
                        if (entry.timeOutSet > 0)
                        {
                            entry.timeOutLeft = entry.timeOutSet;
                            entry.timeOutSet = 0; // timer is running;
                        }
                        else
                        {
                            if (entry.timeOutLeft <= timeElapsed)
                            { // timeout
                                bool routeInUse = entry.routeInUse;
                                bool broadcastActive = entry.broadcastActive;
                                entry.routeInUse = false;
                                entry.broadcastActive = false;

                                if (routeInUse && !entry.routePending)
                                {
                                    Trace.Print("Refreshing route to 0x" + HexConverter.ConvertUintToHex(entry.targetAddr, 4));
                                    entry.routePending = true;
                                    entry.requestRetryCnt = cRouteRequestRetryCnt + 1;
                                }

                                if (entry.routePending)
                                {
                                    if (entry.requestRetryCnt > 0)
                                    {
                                        // retry route request
                                        Trace.Print("Sending route request for 0x" + HexConverter.ConvertUintToHex(entry.targetAddr, 4) + " for timeout");
                                        entry.requestRetryCnt--;
                                        entry.timeOutLeft = cRouteRequestWaitTime;
                                        _routing.RouteRequest(entry.targetAddr);
                                    }
                                    else
                                    {
                                        entry.routePending = false; // give up
                                        ClearPendingMessages(entry);
                                    }
                                }
                                else
                                {
                                    if (routeInUse || broadcastActive)
                                    {
                                        // keep entry
                                        Trace.Print("Invalidating route to 0x" + HexConverter.ConvertUintToHex(entry.targetAddr, 4) + " for timeout");
                                        entry.nextHopAddr = cInvalidAddr;
                                        entry.timeOutLeft = cRouteTimeout;
                                    }
                                    else
                                    {
                                        // delete entry
                                        Trace.Print("Removing route for 0x" + HexConverter.ConvertUintToHex(entry.targetAddr, 4) + " for timeout");
                                        RemoveEntry(i);
                                        continue; // keep i
                                    }
                                }
                            }
                            else
                            {
                                // update timer
                                entry.timeOutLeft -= timeElapsed;
                            }
                        }

                        // update timeout
                        if (entry.timeOutLeft < nextTimeout)
                            nextTimeout = entry.timeOutLeft;
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Add a new entry to the routing table and return index of new entry.
        /// Caller should lock the table.
        /// </summary>
        /// <returns>Index of the new entry</returns>
        private Entry AddEntry(UInt16 targetAddr)
        {
            Entry entry = new Entry();
            // set default values
            entry.targetAddr = targetAddr;
            entry.nextHopAddr = cInvalidAddr;
            entry.timeOutSet = cInfiniteTimeout;

            _table.Add(entry);
            return entry;
        }

        private void ClearPendingMessages(Entry entry)
        {
            while (entry.pendingMessages.Count > 0)
            {
                PendingMessage pm = (PendingMessage)entry.pendingMessages[0];
                entry.pendingMessages.RemoveAt(0);
                _routing.DataRequestTimeout(entry.targetAddr, pm.sduHandle, pm.handler, pm.isControlMsg);
                Frame.Release(ref pm.sdu);
            }
        }

        /// <summary>
        /// Remove specified entry in routing table.
        /// Caller should lock the table.
        /// </summary>
        /// <param name="index"></param>
        private void RemoveEntry(int index)
        {
            if (index < 0 || index >= _table.Count)
                return;

            // cleanup pending messages
            Entry entry = _table[index] as Entry;
            ClearPendingMessages(entry);

            // update routes using this entry as next hop
            int count = _table.Count;
            for (int i = 0; i < count; i++)
            {
                Entry item = _table[i] as Entry;
                if (entry.targetAddr == item.nextHopAddr)
                    item.nextHopAddr = cInvalidAddr;
            }

            // remove entry
            _table.RemoveAt(index);
        }

        /// <summary>
        /// Find routing table entry for given targetAddr.
        /// Caller should lock the table.
        /// </summary>
        /// <returns>Index of entry, -1 when not found</returns>
        private int FindTargetAddr(UInt16 targetAddr)
        {
            int count = _table.Count;
            for (int i = 0; i < count; i++)
            {
                Entry entry = _table[i] as Entry;
                if (entry.targetAddr == targetAddr)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Calculates route cost
        /// </summary>
        /// <param name="minLqi">minimum lqi on route</param>
        /// <param name="hopCount">number of hops</param>
        /// <returns></returns>
        private int CalcRouteCost(byte minLqi, byte hopCount)
        {
            int res = (256 - minLqi); // 1..255
            return res * hopCount;
        }

        /// <summary>
        /// Update routing table using given information.
        /// </summary>
        /// <returns>True if new route is superior to old route</returns>
        public bool UpdateRoute(
            UInt16 targetAddr,
            UInt16 nextHopAddr,
            byte seqNo,
            byte hopCount,
            byte minLqi)
        {
            lock (_lock)
            {
                int i = FindTargetAddr(targetAddr);

                Entry entry;
                if (i == -1) // not found, so let's add this entry
                {
                    entry = AddEntry(targetAddr);
                    entry.nextHopAddr = nextHopAddr;
                    entry.seqNo = seqNo;
                    entry.hopCount = hopCount;
                    entry.minLqi = minLqi;
                    entry.timeOutSet = cRouteTimeout;
                    _timerEvent.Set();
                    return true;
                }

                // route was found.
                entry = _table[i] as Entry;

                if (entry.routePending == false && entry.nextHopAddr != cInvalidAddr) // seqNo is valid
                {
                    // some heuristic to balance hop count vs. link quality
                    // the metric has to ensure loop freedom, i.e. be strict monoton
                    int costNew = CalcRouteCost(minLqi, hopCount);
                    int costOld = CalcRouteCost(entry.minLqi, entry.hopCount);

                    // see DYMO 5.2.1 for case differentiation
                    int seqOffset = (SByte)(seqNo - entry.seqNo);
                    if (seqOffset < 0)
                    {
                        // handle reset of seqNo
                        if (seqNo < Routing.cSeqNoWraparound)
                        {
                            entry.seqNo = seqNo;
                            seqOffset = 1; // accept new route
                        }
                        else
                        {
                            return false; // new route is stale
                        }
                    }

                    if (seqOffset == 0 &&
                        costNew >= costOld)
                        return false; // new route is inferior
                }

                // update table
                // - update
                entry.nextHopAddr = nextHopAddr;
                entry.routeHasError = false;
                entry.seqNo = seqNo;
                entry.hopCount = hopCount;
                entry.minLqi = minLqi;

                if (entry.routePending)
                {
                    entry.routePending = false;

                    // transmit all queued messages
                    while (entry.pendingMessages.Count > 0)
                    {
                        PendingMessage pm = (PendingMessage)entry.pendingMessages[0];
                        entry.pendingMessages.RemoveAt(0);
                        _routing.DataRequest(entry.targetAddr, entry.nextHopAddr,
                            ref pm.sdu, pm.sduHandle, pm.handler, pm.isControlMsg);
                    }
                }

                // set/update route timeout. the greater the hopCount, the shorter the timeout.
                // this ensures that outer nodes of a path refresh the path first, implicityly refreshing
                // the route for all intermediate nodes
                entry.timeOutSet = cRouteTimeout -
                    hopCount * (cRouteRequestRetryCnt + 1) * cRouteRequestWaitTime;
                _timerEvent.Set();

                return true;
            }
        }

        /// <summary>
        /// Check/update seqNo from brodcast message.
        /// </summary>
        /// <param name="targetAddrShort"></param>
        /// <param name="seqNo"></param>
        /// <returns>True if message is new</returns>
        public bool CheckBroadcastSeqNo(
            UInt16 orgAddrShort,
            byte seqNo)
        {
            lock (_lock)
            {
                Entry entry;
                int i = FindTargetAddr(orgAddrShort);
                if (i == -1)
                {
                    entry = AddEntry(orgAddrShort);
                }
                else
                {
                    entry = _table[i] as Entry;
                }

                // check is seqNo is known at all
                if (!entry.broadcastSeqNoValid)
                {
                    entry.broadcastSeqNoValid = true;
                    entry.broadcastSeqNo = seqNo;
                    entry.broadcastWindow = 0;
                    return (entry.broadcastActive = true);
                }

                // see spec for cases
                if (seqNo < Routing.cSeqNoWraparound && entry.broadcastSeqNo > Routing.cSeqNoWraparound)
                {
                    // reset case
                    entry.broadcastSeqNo = seqNo;
                    entry.broadcastWindow = 0;
                    return (entry.broadcastActive = true); // new value
                }

                int seqOffset = (SByte)(seqNo - entry.broadcastSeqNo);
                if (seqOffset == 0)
                    return false; // known value
                if (seqOffset < 0)
                {
                    if (seqOffset >= -32)
                    { // check from bitmap
                        // seqOffset is -32..-1 and points into bitmap
                        UInt32 toggle = (UInt32)(1 << (0 - 1 - seqOffset));
                        if ((entry.broadcastWindow & toggle) > 0)
                            return false;
                        entry.broadcastWindow |= toggle;
                        return (entry.broadcastActive = true);
                    }

                    return false; // old value
                }
                else
                {
                    entry.broadcastWindow = (entry.broadcastWindow << 1) | 1; // add current value
                    seqOffset--;
                    entry.broadcastWindow <<= seqOffset;
                    entry.broadcastSeqNo = seqNo;
                    return (entry.broadcastActive = true); // new value
                }
            }
        }

        /// <summary>
        /// Invalidates all routes using given next hop
        /// </summary>
        /// <param name="nextHopAddr"></param>
        public void HandleLinkFailure(UInt16 nextHopAddr)
        {
            if (nextHopAddr == cInvalidAddr)
                return;

            lock (_lock)
            {
                int n = _table.Count;
                for (int i = 0; i < n; i++)
                {
                    Entry entry = _table[i] as Entry;
                    if (entry.nextHopAddr == nextHopAddr)
                    {
                        entry.routeHasError = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles route error.
        /// </summary>
        /// <param name="tgtAddrShort"></param>
        /// <param name="nextHopShortAddr"></param>
        /// <returns>True if active route is affected</returns>
        public bool HandleRouteError(
            UInt16 targetAddr,
            UInt16 nextHopAddr,
            bool fatalError)
        {
            lock (_lock)
            {
                int i = FindTargetAddr(targetAddr);
                if (i != -1)
                {
                    Entry entry = _table[i] as Entry;
                    if (entry.nextHopAddr == nextHopAddr)
                    {
                        if (fatalError)
                        { // invalidate route only if error is fatal
                            entry.nextHopAddr = cInvalidAddr;
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Get next hop from routing table
        /// </summary>
        /// <param name="targetAddr">target node</param>
        /// <param name="nextHopAddr">next hop towards target node</param>
        /// <param name="routeHasError">indicates that selected route has had transmission errors</param>
        /// <returns>true on success</returns>
        public bool GetRoute(
            UInt16 targetAddr,
            out UInt16 nextHopAddr,
            out bool routeHasError)
        {
            lock (_lock)
            {
                int i = FindTargetAddr(targetAddr);
                if (i != -1)
                {
                    Entry entry = _table[i] as Entry;
                    if (entry.nextHopAddr != cInvalidAddr)
                    {
                        nextHopAddr = entry.nextHopAddr;
                        routeHasError = entry.routeHasError;
                        entry.routeHasError = false; // report only once
                        entry.routeInUse = true;
                        return true;
                    }
                }

                nextHopAddr = cInvalidAddr;
                routeHasError = false;
                return false;
            }
        }

        /// <summary>
        /// Get next hop from routing table
        /// </summary>
        /// <param name="targetAddr">target node</param>
        /// <param name="nextHopAddr">next hop towards target node</param>
        /// <param name="routeHasError">indicates that selected route has had transmission errors</param>
        /// <returns>true on success</returns>
        public bool GetRoute(
            UInt16 targetAddr,
            out UInt16 nextHopAddr)
        {
            lock (_lock)
            {
                int i = FindTargetAddr(targetAddr);
                if (i != -1)
                {
                    Entry entry = _table[i] as Entry;
                    if (entry.nextHopAddr != cInvalidAddr)
                    {
                        nextHopAddr = entry.nextHopAddr;
                        entry.routeInUse = true;
                        return true;
                    }
                }

                nextHopAddr = cInvalidAddr;
                return false;
            }
        }

        /// <summary>
        /// To send sdu from local node
        /// </summary>
        /// <param name="tgtAddrExt"></param>
        /// <param name="sduHandle"></param>
        /// <param name="sdu"></param>
        /// <param name="handler"></param>
        public void DataRequest(
            UInt16 targetAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler,
            bool isControlMsg)
        {
            lock (_lock)
            {
                int i = FindTargetAddr(targetAddr);
                Entry entry;
                if (i == -1)
                {
                    // create new entry
                    entry = AddEntry(targetAddr);
                }
                else
                {
                    entry = _table[i] as Entry;
                }

                bool doRouteRequest = false;

                if (entry.nextHopAddr == cInvalidAddr || entry.routeHasError)
                { // no route to targetAddr
                    // queue this message
                    if (entry.pendingMessages.Count == cMaxPendingMsg)
                    { // delete oldest waiting msg
                        PendingMessage pmOld = (PendingMessage)entry.pendingMessages[0];
                        entry.pendingMessages.RemoveAt(0);
                        _routing.DataRequestTimeout(targetAddr, pmOld.sduHandle, pmOld.handler, pmOld.isControlMsg);
                        Frame.Release(ref pmOld.sdu);
                    }

                    PendingMessage pm = new PendingMessage();
                    pm.sdu = sdu;
                    sdu = null;
                    pm.sduHandle = sduHandle;
                    pm.handler = handler;
                    pm.isControlMsg = isControlMsg;
                    entry.pendingMessages.Add(pm);

                    doRouteRequest = true;
                }
                else
                {
                    entry.routeInUse = true;
                    _routing.DataRequest(targetAddr, entry.nextHopAddr, ref sdu, sduHandle, handler, isControlMsg);

                    if (entry.routeHasError)
                    {
                        doRouteRequest = true;
                        entry.routeHasError = false;
                    }
                }

                if (doRouteRequest)
                {
                    // initiate route request
                    if (!entry.routePending)
                    {
                        // we haven't asked for a route yet, start now
                        entry.routePending = true;
                        entry.requestRetryCnt = cRouteRequestRetryCnt;
                        entry.timeOutSet = cRouteRequestWaitTime;
                        _timerEvent.Set();
                        Trace.Print("Sending route request for 0x" + HexConverter.ConvertUintToHex(entry.targetAddr, 4) + " for data");
                        _routing.RouteRequest(targetAddr);
                    } // else: wait for ongoing request
                }
            }
        }
    }
}


