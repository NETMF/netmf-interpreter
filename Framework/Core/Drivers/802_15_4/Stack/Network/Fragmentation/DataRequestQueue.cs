////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Collections;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{

    /// <summary>
    /// Internal class used to store item to send in  a queue.
    /// </summary>
    public class DataRequestItem : IDisposable
    {

        ushort _targetShortAddress;
        Frame _sdu;
        byte _sduHandle;
        DataConfirmHandler _handler;

        public ushort TargetShortAddr { get { return _targetShortAddress; } }
        public Frame Sdu { get { return _sdu; } }
        public byte SduHandle { get { return _sduHandle; } }
        public DataConfirmHandler Handler { get { return _handler; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetShortAddr">destination short address</param>
        /// <param name="sdu">data to send</param>
        /// <param name="sduHandle">handle associated to sdu</param>
        /// <param name="handler">handler used to notify when message transmission is finished</param>
        public DataRequestItem(ushort targetShortAddr, ref Frame sdu, byte sduHandle, DataConfirmHandler handler)
        {
            _targetShortAddress = targetShortAddr;
            _sdu = sdu;
            sdu = null; // take ownership of the frame
            _sduHandle = sduHandle;
            _handler = handler;
        }

        #region IDisposable Members

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
            if (_sdu != null)
            {
                Frame.Release(ref _sdu);
            }
        }

        #endregion
    }

    /// <summary>
    /// Notifying Queue with an additional attribute: targetAddress.
    /// </summary>
    internal class DataRequestQueue : NotifyingQueue
    {
        readonly UInt16 _targetAddress;
        internal UInt16 TargetAddress { get { return _targetAddress; } }

        internal DataRequestQueue(int queueSize, AutoResetEvent queueChangeEvent, UInt16 targetAddress)
            : base(typeof(DataRequestItem), queueSize, queueChangeEvent)
        {
            _targetAddress = targetAddress;
        }
    }

    /// <summary>
    /// Set of DataREquestQueue, each ahving a different target address
    /// </summary>
    public class DataRequestQueueSet : IDisposable
    {
        private int _queueSize;
        private AutoResetEvent _queueChangedEvent;

        public DataRequestQueueSet(int queueSize, AutoResetEvent queueChangedEvent)
        {
            if (queueChangedEvent == null)
                throw new ArgumentNullException("queueChangedEvent");
            _queueSize = queueSize;
            _queueChangedEvent = queueChangedEvent;
            _dataRequestQueues = new ArrayList();
        }

        private ArrayList _dataRequestQueues;

        /// <summary>
        /// Get request queue associated to targetAddress
        /// </summary>
        /// <param name="targetAddress"></param>
        /// <returns>new DataRequestQueue if not yet present, existing queue otherwise</returns>
        private DataRequestQueue GetDataRequestQueue(UInt16 targetAddress)
        {
            lock (_dataRequestQueues)
            {
                int dataREquestQueuesCount = _dataRequestQueues.Count;
                for (int i = 0; i < dataREquestQueuesCount; i++)
                {
                    DataRequestQueue queue = (DataRequestQueue)_dataRequestQueues[i];
                    if (queue.TargetAddress == targetAddress) { return queue; }
                }

                // did not find a corresponding queue. Create a new one.
                DataRequestQueue newQueue = new DataRequestQueue(_queueSize, _queueChangedEvent, targetAddress);
                _dataRequestQueues.Add((Object)newQueue);
                return newQueue;
            }
        }

        /// <summary>
        /// Get the number of data request currently pending for the provided destination
        /// </summary>
        /// <param name="targetAddress">destination</param>
        /// <returns>number of pending data request</returns>
        public int GetCount(ushort targetAddress)
        {
            return GetDataRequestQueue(targetAddress).Count;
        }

        /// <summary>
        /// Removes and returns oldest item from queue.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>True on success</returns>
        public bool Dequeue(ushort targetAddress, out DataRequestItem item)
        {
            DataRequestQueue queue = GetDataRequestQueue(targetAddress);
            Object o;
            item = null;
            if (queue.Dequeue(out o))
            {
                if (!(o is DataRequestItem))
                    throw new ArgumentException("Bad object type dequeued");
                item = o as DataRequestItem;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the oldest item from the queue (but leaves the item in the queue)
        /// </summary>
        /// <param name="targetAddress"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Peek(UInt16 targetAddress, out DataRequestItem item)
        {
            DataRequestQueue queue = GetDataRequestQueue(targetAddress);
            Object o;
            item = null;
            if (queue.Peek(out o))
            {
                if (!(o is DataRequestItem))
                    throw new ArgumentException("Bad object type peeked");
                item = o as DataRequestItem;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add item to the appropriate queue
        /// </summary>
        /// <returns>True on success</returns>
        public bool Add(DataRequestItem item)
        {
            DataRequestQueue queue = GetDataRequestQueue(item.TargetShortAddr);
            return queue.Add(item);
        }

        /// <summary>
        /// Disposes all queues.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Close()
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            int dataREquestQueuesCount = _dataRequestQueues.Count;
            for (int i = 0; i < dataREquestQueuesCount; i++)
            {
                DataRequestQueue queue = (DataRequestQueue)_dataRequestQueues[i];
                queue.Dispose();
            }

            _dataRequestQueues.Clear();
        }
    }
}


