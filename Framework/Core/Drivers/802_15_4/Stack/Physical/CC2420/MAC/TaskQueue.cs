////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac
{
    /// <summary>
    /// This class stores tasks in a fixed size queue. An event is raised when an item gets added.
    /// There are separate size limits for distinct item types (cmd and data).
    /// </summary>
    class TaskQueue
    {
        private readonly int _maxSize; // total queue size
        private readonly int _maxSizeCmd; // max cmd items to queue
        private readonly int _maxSizeData; // max data items to queue
        private AutoResetEvent _queueChangeEvent; // the event to signal if an item was added
        private object _lock; // lock to protect local variables
        private Task[] _tasks; // the fixed size array
        private int _head; // queue head: next item to write
        private int _tail; // queue tail: next item to read
        private int _size; // current nr of items in queue
        private int _sizeCmd; // current nr of cmd items in queue
        private int _sizeData; // current nr of data items in queue

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queueChangeEvent">Event that is set when something was added to the queue.</param>
        public TaskQueue(int queueSizeCmd, int queueSizeData, AutoResetEvent queueChangeEvent)
        {
            _maxSize = queueSizeCmd + queueSizeData;
            _maxSizeCmd = queueSizeCmd;
            _maxSizeData = queueSizeData;
            _queueChangeEvent = queueChangeEvent;
            _lock = new object();
            _tasks = new Task[_maxSize];
            _head = 0;
            _tail = 0;
            _sizeCmd = 0;
            _sizeData = 0;
        }

        /// <summary>
        /// Allocate free item. Caller has to hold lock.
        /// </summary>
        /// <returns>-1 on error</returns>
        private int Alloc(TaskType type)
        {
            bool isData = (type == TaskType.DataRequest);
            if ((isData && _sizeData == _maxSizeData) ||
                (!isData && _sizeCmd == _maxSizeCmd))
                return -1;

            int res = _head;
            _head = (_head + 1) % _maxSize;

            _size++;
            if (isData)
                _sizeData++;
            else
                _sizeCmd++;

            _queueChangeEvent.Set();
            return res;
        }

        /// <summary>
        /// Removes and returns oldest item from queue.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>True on success</returns>
        public bool Dequeue(out Task task)
        {
            lock (_lock)
            {
                if (_size == 0)
                {
                    task = null;
                    return false;
                }

                task = _tasks[_tail];
                _tasks[_tail] = null;
                _tail = (_tail + 1) % _maxSize;

                _size--;
                if (task.taskType == TaskType.DataRequest)
                    _sizeData--;
                else
                    _sizeCmd--;

                return true;
            }
        }

        /// <summary>
        /// Add task to queue
        /// </summary>
        /// <returns>True on success</returns>
        public bool Add(Task task)
        {
            if (task == null)
                return false;
            lock (_lock)
            {
                int i = Alloc(task.taskType);
                if (i == -1)
                {
                    Trace.Print("TaskQueue is full");
                    return false;
                }

                _tasks[i] = task;
                return true;
            }
        }
    }
}


