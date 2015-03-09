////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{
    /// <summary>
    /// This class stores tasks in a fixed size queue. An event is raised when an item gets added.
    /// </summary>
    public class NotifyingQueue : IDisposable
    {
        private readonly int _maxSize;
        private AutoResetEvent _queueChangeEvent; // the event to signal if an item was added
        private object _lock; // lock to protect local variables
        private Object[] _items; // the fixed size array
        private int _head; // queue head: next item to write
        private int _tail; // queue tail: next item to read
        private int _size; // current nr of items in queue, 0.._maxSize
        private Type _type; // type of the elements stored in the queue

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queueChangeEvent">Event that is set when something was added to the queue.</param>
        public NotifyingQueue(Type type, int queueSize, AutoResetEvent queueChangeEvent)
        {
            if (queueChangeEvent == null)
                throw new ArgumentNullException("queueChangeEvent");

            _maxSize = queueSize;
            _type = type;
            _queueChangeEvent = queueChangeEvent;
            _lock = new object();
            _items = new Object[_maxSize];
            _head = 0;
            _tail = 0;
            _size = 0;
        }

        /// <summary>
        /// Allocate free item. Caller has to hold lock.
        /// </summary>
        /// <returns>-1 on error</returns>
        private int Alloc()
        {
            if (_size == _maxSize)
                return -1;
            int res = _head;
            _head = (_head + 1) % _maxSize;
            _size++;
            _queueChangeEvent.Set();
            return res;
        }

        /// <summary>
        /// Number of elements in the queue
        /// </summary>
        public int Count { get { return _size; } }

        /// <summary>
        /// Removes and returns oldest item from queue.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>True on success</returns>
        public bool Dequeue(out Object item)
        {

            lock (_lock)
            {
                if (_size == 0)
                {
                    item = null;
                    return false;
                }

                item = _items[_tail];
                _items[_tail] = null;
                _tail = (_tail + 1) % _maxSize;
                _size--;
                return true;
            }
        }

        /// <summary>
        /// Returns oldest item from queue.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>True on success</returns>
        public bool Peek(out Object item)
        {

            lock (_lock)
            {
                if (_size == 0)
                {
                    item = null;
                    return false;
                }

                item = _items[_tail];
                return true;
            }
        }

        /// <summary>
        /// Add task to queue
        /// </summary>
        /// <returns>True on success</returns>
        public bool Add(Object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (item.GetType() != _type)
                throw new ArgumentException("Bad type", "item");

            lock (_lock)
            {
                int i = Alloc();
                if (i == -1)
                    return false;
                _items[i] = item;
                return true;
            }
        }

        /// <summary>
        /// Dispose all elements in the queue
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
            lock (_lock)
            {
                Object item;
                while (Dequeue(out item))
                {
                    IDisposable disposableItem = item as IDisposable;
                    if (disposableItem != null)
                    {
                        disposableItem.Dispose();
                    }
                }
            }
        }

    }
}


