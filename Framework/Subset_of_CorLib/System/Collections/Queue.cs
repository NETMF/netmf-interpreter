////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections
{
    /// <summary>
    /// A circular-array implementation of a queue. Enqueue can be O(n).  Dequeue is O(1).
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable()]
    public class Queue : ICollection, ICloneable
    {
        private Object[] _array;
        private int _head;       // First valid element in the queue
        private int _tail;       // Last valid element in the queue
        private int _size;       // Number of elements.

        // Keep in-sync with c_DefaultCapacity in CLR_RT_HeapBlock_Queue in TinyCLR_Runtime__HeapBlock.h
        private const int _defaultCapacity = 4;

        /// <summary>
        /// Initializes a new instance of the Queue class that is empty, has the default initial
        /// capacity, and uses the default growth factor (2x).
        /// </summary>
        public Queue()
        {
            _array = new Object[_defaultCapacity];
            _head = 0;
            _tail = 0;
            _size = 0;
        }

        /// <summary>
        /// Gets the number of elements contained in the Queue.
        /// </summary>
        public virtual int Count
        {
            get { return _size; }
        }

        /// <summary>
        /// Creates a shallow copy of the Queue.
        /// </summary>
        /// <returns>A shallow copy of the Queue.</returns>
        public virtual Object Clone()
        {
            Queue q = new Queue();

            if (_size > _defaultCapacity)
            {
                // only re-allocate a new array if the size isn't what we need.
                // otherwise, the one allocated in the constructor will be just fine
                q._array = new Object[_size];
            }
            else
            {
                // if size is not the same as capacity, we need to adjust tail accordingly
                q._tail = _size % _defaultCapacity;
            }

            q._size = _size;

            CopyTo(q._array, 0);

            return q;
        }

        /// <summary>
        /// Gets a value indicating whether access to the Queue is synchronized (thread safe).
        /// Always return false.
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the Queue.
        /// </summary>
        public virtual Object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Removes all objects from the Queue.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void Clear();

        /// <summary>
        /// Copies the Queue elements to an existing one-dimensional Array, starting at
        /// the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from Queue.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void CopyTo(Array array, int index);

        /// <summary>
        /// Adds an object to the end of the Queue.
        /// </summary>
        /// <param name="obj">The object to add to the Queue.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void Enqueue(Object obj);

        /// <summary>
        /// Returns an enumerator that iterates through the Queue.
        /// </summary>
        /// <returns>An IEnumerator for the Queue.</returns>
        public virtual IEnumerator GetEnumerator()
        {
            int endIndex = _tail;

            if (_size > 0 && _tail <= _head) endIndex += _array.Length;

            return new Array.SZArrayEnumerator(_array, _head, endIndex);
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the Queue.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual Object Dequeue();

        /// <summary>
        /// Returns the object at the beginning of the Queue without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the Queue.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual Object Peek();

        /// <summary>
        /// Determines whether an element is in the Queue.
        /// </summary>
        /// <param name="obj">The Object to locate in the Queue.</param>
        /// <returns>true if obj is found in the Queue; otherwise, false.</returns>
        public virtual bool Contains(Object obj)
        {
            if (_size == 0)
                return false;

            if (_head < _tail)
            {
                return Array.IndexOf(_array, obj, _head, _size) >= 0;
            }
            else
            {
                return (Array.IndexOf(_array, obj, _head, _array.Length - _head) >= 0) ||
                       (Array.IndexOf(_array, obj, 0, _tail) >= 0);
            }
        }

        /// <summary>
        /// Copies the Queue elements to a new array. The order of the elements in the new
        /// array is the same as the order of the elements from the beginning of the Queue
        /// to its end.
        /// </summary>
        /// <returns>A new array containing elements copied from the Queue.</returns>
        public virtual Object[] ToArray()
        {
            Object[] arr = new Object[_size];

            CopyTo(arr, 0);

            return arr;
        }
    }
}


