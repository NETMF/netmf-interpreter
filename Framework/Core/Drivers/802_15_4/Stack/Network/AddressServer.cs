////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

using System;
using System.Collections;
using System.Threading;
#if MICROFRAMEWORK
using Microsoft.SPOT;
#endif

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    /// <summary>
    /// Implements allocation service for short addresses.
    /// </summary>
    public class AddressServer
    {
        private const int cBuckets = 16;
        private const int cFirstAddr = Routing.cCoordinatorShortAddr + 1;
        private const int cLastAddr = 32767; // addr with upper bit set are multicast addr

        [Serializable]
        private struct Mapping
        {
            public UInt64 extAddr;
            public UInt16 shortAddr;
        }

        [Serializable]
        private class Addresses
        {
            public UInt16 nextAddr;
            public ArrayList[] data;
        }

        Addresses _addresses;

        public AddressServer()
        {
            _addresses = new Addresses();
            _addresses.nextAddr = cFirstAddr;
            _addresses.data = new ArrayList[cBuckets];
            for (uint i = 0; i < cBuckets; i++)
            {
                _addresses.data[i] = new ArrayList();
            }
        }

        /// <summary>
        /// allocate a short address
        /// </summary>
        /// <param name="extAddr">the extended address of the target node</param>
        /// <param name="shortAddr">the allocated short address</param>
        /// <returns>true on success</returns>
        public virtual bool AllocateAddress(UInt64 extAddr, out UInt16 shortAddr)
        {
            lock (_addresses)
            {
                int bucket = (int)(extAddr % cBuckets);
                // lookup old addr
                ArrayList list = _addresses.data[bucket];
                Mapping mapping;
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    mapping = (Mapping)list[i];
                    if (mapping.extAddr == extAddr)
                    {
                        shortAddr = mapping.shortAddr;
                        return true;
                    }
                }

                // allocate new addr
                if (_addresses.nextAddr > cLastAddr)
                {
                    shortAddr = 0;
                    return false;
                }

                shortAddr = _addresses.nextAddr++;
                // store addr
                mapping = new Mapping();
                mapping.extAddr = extAddr;
                mapping.shortAddr = shortAddr;
                list.Add(mapping);
                Trace.Print("Allocating short address 0x" + HexConverter.ConvertUintToHex(shortAddr, 4) +
                    " for device 0x" + HexConverter.ConvertUint64ToHex(extAddr, 16));

                return true;
            }
        }

        /// <summary>
        /// Load address mapping from persistent storage
        /// </summary>
        public void Load()
        {
#if MICROFRAMEWORK
            ExtendedWeakReference ewr = ExtendedWeakReference.Recover(typeof(Addresses), 0);
            if (ewr != null)
            {
                Addresses addr = ewr.Target as Addresses;
                if (addr != null)
                    _addresses = addr;
            }

#endif
        }

        /// <summary>
        /// Save addres mapping to persistent storage
        /// </summary>
        public void Save()
        {
#if MICROFRAMEWORK
            ExtendedWeakReference ewr = ExtendedWeakReference.RecoverOrCreate(typeof(Addresses), 0, ExtendedWeakReference.c_SurvivePowerdown);
            if (ewr != null)
            {
                ewr.Priority = (int)ExtendedWeakReference.PriorityLevel.System;
                ewr.Target = _addresses;
                ewr.PushBackIntoRecoverList();
            }

#endif
        }
    }

    /// <summary>
    /// this interface provides the callbacks from the discovery server
    /// </summary>
    public interface IDiscoveryServerCallbacks
    {
        /// <summary>
        /// Handler to indicate change of available nodes
        /// </summary>
        /// <param name="node">the short address of the node that has joined or left the network</param>
        /// <param name="isAvailable">true if the node has joined, false if the node has left the network</param>
        void NodeChanged(
            UInt16 node,
            bool isAvailable);
    }

    /// <summary>
    /// this class implements a combination of the address server and a n
    /// </summary>
    public class AddressAndDiscoveryServer : AddressServer, IDisposable
    {
        private static readonly UInt16 cDefaultTimeInterval = 15; // seconds
        private readonly int cMaxMissedIntervals = 2; // assume node is dead after x missing messages
        // cannot use 0 since there is some jitter between timers at nodes and local timer

        private class TimerItem
        {
            public UInt16 shortAddr;
            public int timeoutCounter;
        }

        IDiscoveryServerCallbacks _callbacks;
        private UInt16 _timeInterval;
        ArrayList _data;
        Timer _timer;

        public AddressAndDiscoveryServer(IDiscoveryServerCallbacks callbacks)
            : this(callbacks, cDefaultTimeInterval)
        {
            // load old address mapping
            base.Load();
        }

        public AddressAndDiscoveryServer(IDiscoveryServerCallbacks callbacks, UInt16 timeInterval)
        {
            _callbacks = callbacks;
            _timeInterval = timeInterval;
            _data = new ArrayList();
            int period = timeInterval * 1000;
            _timer = new Timer(MyTimerCallback, null, period, period);
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
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            _data.Clear();
        }

        /// <summary>
        /// Get time interval for discovery messages
        /// </summary>
        public UInt16 TimeInterval
        {
            get { return _timeInterval; }
        }

        /// <summary>
        /// Handle internal timer
        /// </summary>
        /// <param name="state"></param>
        private void MyTimerCallback(object state)
        {
            // update interval counter for all nodes, detect failed nodes
            lock (_data)
            {
                int i = 0;
                while (i < _data.Count)
                {
                    TimerItem item = _data[i] as TimerItem;
                    item.timeoutCounter++;
                    if (item.timeoutCounter > cMaxMissedIntervals)
                    {
                        _callbacks.NodeChanged(item.shortAddr, false);
                        _data.RemoveAt(i);
                        continue;
                    }

                    i++;
                }
            }
        }

        /// <summary>
        /// allocate a short address and update set of available nodes
        /// </summary>
        /// <param name="extAddr">the extended address of the target node</param>
        /// <param name="shortAddr">the allocated short address</param>
        /// <returns>true on success</returns>
        public override bool AllocateAddress(UInt64 extAddr, out UInt16 shortAddr)
        {
            bool res = base.AllocateAddress(extAddr, out shortAddr);
            if (res)
            {
                // save current address mapping
                base.Save();
                // add node to list
                TimerItem item = null;
                // - check if entry already exists
                lock (_data)
                {
                    int count = _data.Count;
                    for (int i = 0; i < count; i++)
                    {
                        item = _data[i] as TimerItem;
                        if (item.shortAddr == shortAddr)
                        {
                            item.timeoutCounter = 0;
                            break;
                        }

                        item = null;
                    }
                }

                if (item != null)
                {
                    // if an existing node requests an address, it has reset.
                    // inform users by toggling its presence
                    _callbacks.NodeChanged(shortAddr, false);
                }
                else
                {
                    // - create new entry
                    item = new TimerItem();
                    item.shortAddr = shortAddr;
                    item.timeoutCounter = 0;
                    lock (_data)
                    {
                        _data.Add(item);
                    }
                }

                _callbacks.NodeChanged(shortAddr, true);
            }

            return res;
        }

        /// <summary>
        /// Handle discovery message from peer node
        /// </summary>
        /// <param name="node"></param>
        public void HandleDiscoveryReply(UInt16 node)
        {
            // reset timeout counter
            lock (_data)
            {
                int count = _data.Count;
                for (int i = 0; i < count; i++)
                {
                    TimerItem item = _data[i] as TimerItem;
                    if (item.shortAddr == node)
                    {
                        item.timeoutCounter = 0;
                        return;
                    }
                }

                // add node to list
                {
                    TimerItem item = new TimerItem();
                    item.shortAddr = node;
                    item.timeoutCounter = 0;
                    lock (_data)
                    {
                        _data.Add(item);
                    }

                    _callbacks.NodeChanged(node, true);
                }
            }
        }

        /// <summary>
        /// Get current set of available nodes
        /// </summary>
        /// <returns>array of nodes</returns>
        public UInt16[] GetNodes()
        {
            lock (_data)
            {
                int count = _data.Count;
                UInt16[] res = new UInt16[count];
                for (int i = 0; i < count; i++)
                {
                    TimerItem item = _data[i] as TimerItem;
                    res[i] = item.shortAddr;
                }

                return res;
            }
        }
    }
}


