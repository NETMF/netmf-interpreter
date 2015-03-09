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
    /// This interface defines callbacks from the neighbor table to the network layer to handle certain events
    /// </summary>
    public interface INeighborTableCallbacks
    {
        /// <summary>
        /// send beacon message
        /// </summary>
        /// <param name="sdu">beacon encoded into frame</param>
        void SendBeacon(ref Frame sdu);
    }

    /// <summary>
    /// This class is used to keep track of neighbouring nodes.
    /// Nodes are periodically broadcasting beacons to maintain this table.
    /// It is crucial to know which radio link operate bidirectional. Thus, each node broadcasts its own neighbors.
    /// </summary>
    public class NeighborTable : IDisposable
    {
        const int cMaxSize = Message.NeighborhoodDiscovery.cMaxNeighbours;
        const int cMaxMissCount = 2; // remove neighbor after x missed beacon frames (0==immediately)
        const int cBeaconPeriod = 10000; // ms, 10 secs

        private class Entry
        {
            public UInt16 addrShort; // short addr of this neighbour
            public byte lqiRx; // the lqi when receiving frames from neighbour directly
            public byte lqiTx; // the lqi when sending frames to our neighbour (reported from neighbour)
            public bool lqiTxValid; // guess
            public byte missCounter; // nr of missing beacon frames
        }

        private INeighborTableCallbacks _host;
        private int _macHeader;
        private int _macTailer;

        private object _lock;
        private ArrayList _neighbours; // type Entry
        private UInt16 _localAddr;
        private Thread _timerThread;          // A thread that handles timeouts in the routing table
        private AutoResetEvent _timerEvent;   // An event to signal a changed timer

        public NeighborTable(INeighborTableCallbacks host, int macHeader, int macTailer)
        {
            _host = host;
            _macHeader = macHeader;
            _macTailer = macTailer;

            _lock = new object();
            _neighbours = new ArrayList();
            _timerThread = null;
            _timerEvent = new AutoResetEvent(false);
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
            Stop(true);
        }

        /// <summary>
        /// start periodic sending of beacons
        /// </summary>
        /// <param name="localAddr">local short addr</param>
        public void Start(UInt16 localAddr)
        {
            lock (_lock)
            {
                _localAddr = localAddr;
                if (_timerThread == null)
                {
                    _timerThread = new Thread(TimerThread);
#if !MICROFRAMEWORK
                    _timerThread.IsBackground = true;
#endif
                    _timerThread.Start();
                }
            }
        }

        /// <summary>
        /// stop periodic sending of beacons
        /// </summary>
        /// <param name="clearData"></param>
        public void Stop(bool clearData)
        {
            Thread timerThread;
            lock (_lock)
            {
                timerThread = _timerThread;
                _timerThread = null;
            }

            if (timerThread != null)
            {
                _timerEvent.Set();
                timerThread.Join();
            }

            lock (_lock)
            {
                if (clearData)
                    _neighbours.Clear();
            }
        }

        /// <summary>
        /// process received beacon
        /// </summary>
        /// <param name="senderAddr">address of sender of beacon</param>
        /// <param name="sdu">encoded beacon</param>
        /// <param name="lqi">LQI of beacon message</param>
        public void ReceiveBeacon(UInt16 senderAddr, ref Frame sdu, byte lqi)
        {
            if (sdu == null)
                return;
            Message.NeighborhoodDiscovery msg = new Message.NeighborhoodDiscovery();
            if (msg.ReadFromFrame(sdu))
            {
                lock (_lock)
                {
                    // check if we know this node
                    Entry e = null;
                    int count = _neighbours.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Entry _e = _neighbours[i] as Entry;
                        if (_e != null && _e.addrShort == senderAddr)
                        {
                            e = _e;
                            break;
                        }
                    }

                    // create/update our entry
                    if (e == null)
                    {
                        e = new Entry();
                        e.addrShort = senderAddr;
                        if (_neighbours.Count < cMaxSize)
                            _neighbours.Add(e);
                    }

                    e.lqiRx = lqi;
                    e.lqiTx = 0;
                    e.lqiTxValid = false;
                    e.missCounter = 0;

                    if (_neighbours.Count > cMaxSize)
                    {
                        // remove worst entry
                        byte lqiWorst = 0xff;
                        int iWorst = 0;
                        count = _neighbours.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Entry eWorst = _neighbours[i] as Entry;
                            if (eWorst != null && eWorst.lqiRx < lqiWorst)
                            {
                                lqiWorst = eWorst.lqiRx;
                                iWorst = i;
                            }
                        }

                        _neighbours.RemoveAt(iWorst);
                    }

                    // check if this node has information on us
                    if (msg.Neighbours != null)
                    {
                        int length = msg.Neighbours.Length;
                        for (int i = 0; i < length; i++)
                        {
                            if (msg.Neighbours[i].Address == _localAddr)
                            {
                                e.lqiTxValid = true;
                                e.lqiTx = msg.Neighbours[i].Lqi;
                                break;
                            }
                        }
                    }
                }
            }

            Frame.Release(ref sdu);
        }

        /// <summary>
        /// Update LQI information of neighbor node
        /// </summary>
        /// <param name="senderAddr">the neighbor node</param>
        /// <param name="lqi">the received LQI</param>
        public void UpdateLqi(UInt16 senderAddr, byte lqi)
        {
            lock (_lock)
            {
                // check if we know this node
                int count = _neighbours.Count;
                for (int i = 0; i < count; i++)
                {
                    Entry e = _neighbours[i] as Entry;
                    if (e != null && e.addrShort == senderAddr)
                    {
                        e.lqiRx = lqi;
                        return;
                    }
                }
            }
        }

        private void SendBeacon()
        {
            if (_host == null)
                return;
            Message.NeighborhoodDiscovery msg = new Message.NeighborhoodDiscovery();
            lock (_lock)
            {
                int count = _neighbours.Count;
                msg.Neighbours = new Message.NeighborhoodDiscovery.Neighbour[count];
                for (int i = 0; i < count; i++)
                {
                    Entry e = (Entry)_neighbours[i];
                    msg.Neighbours[i].Address = e.addrShort;
                    msg.Neighbours[i].Lqi = e.lqiRx;
                }
            }

            Frame frame = Frame.GetFrame(_macHeader, msg.Length() + _macTailer);
            if (frame == null)
                return;
            if (msg.WriteToFrame(frame))
                _host.SendBeacon(ref frame);
            Frame.Release(ref frame);
        }

        private void TimerThread()
        {
            for (; ; )
            {
                SendBeacon();
                if (_timerEvent.WaitOne(cBeaconPeriod, false))
                    return;

                lock (_lock)
                {
                    int i = 0;
                    while (i < _neighbours.Count)
                    {
                        Entry entry = (Entry)_neighbours[i];
                        if (entry.missCounter > cMaxMissCount)
                        {
                            _neighbours.RemoveAt(i);
                            continue;
                        }

                        entry.missCounter++;
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Test if another node is a neighbor according to the neighbor table
        /// </summary>
        /// <param name="addrShort">address of node to check</param>
        /// <returns>True if node is neighbor</returns>
        public bool IsNeighbor(UInt16 addrShort)
        {
            byte dontcare;
            return IsNeighbor(addrShort, out dontcare);
        }

        /// <summary>
        /// Test if another node is a neighbor according to the neighbor table
        /// </summary>
        /// <param name="addrShort">address of node to check</param>
        /// <param name="lqi">observed bi-directional LQI towards neighbor</param>
        /// <returns>True if node is neighbor</returns>
        public bool IsNeighbor(UInt16 addrShort, out byte lqi)
        {
            lqi = 0;
            lock (_lock)
            {
                int count = _neighbours.Count;
                for (int i = 0; i < count; i++)
                {
                    Entry e = (Entry)_neighbours[i];
                    if (e.addrShort == addrShort)
                    {
                        if (e.lqiTxValid)
                        {
                            if (e.lqiTx < e.lqiRx)
                                lqi = e.lqiTx;
                            else
                                lqi = e.lqiRx;
                            return true;
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get set of current observed neighbors
        /// </summary>
        /// <returns>set of neighbors</returns>
        public Neighbor[] GetNeighbors()
        {
            lock (_lock)
            {
                int count = _neighbours.Count;
                Neighbor[] res = new Neighbor[count];
                for (int i = 0; i < count; i++)
                {
                    Entry e = (Entry)_neighbours[i];
                    res[i].shortAdr = e.addrShort;
                    res[i].lqi = e.lqiRx;
                }

                return res;
            }
        }
    }
}


