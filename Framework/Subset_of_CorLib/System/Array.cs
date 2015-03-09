////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Runtime.CompilerServices;

namespace System
{
    [Serializable]
    public abstract class Array : ICloneable, IList
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Array CreateInstance(Type elementType, int length);

        public static void Copy(Array sourceArray, Array destinationArray, int length)
        {
            Copy(sourceArray, 0, destinationArray, 0, length);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Clear(Array array, int index, int length);

        public Object GetValue(int index)
        {
            return ((IList)this)[index];
        }

        public extern int Length
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        int ICollection.Count
        {
            get
            {
                return this.Length;
            }
        }

        public Object SyncRoot
        {
            get { return this; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize
        {
            get { return true; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        extern Object IList.this[int index]
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        int IList.Add(Object value)
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(Object value)
        {
            return Array.IndexOf(this, value) >= 0;
        }

        void IList.Clear()
        {
            Array.Clear(this, 0, this.Length);
        }

        int IList.IndexOf(Object value)
        {
            return Array.IndexOf(this, value);
        }

        void IList.Insert(int index, Object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(Object value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public Object Clone()
        {
            int length = this.Length;
            Array destArray = Array.CreateInstance(this.GetType().GetElementType(), length);
            Array.Copy(this, destArray, length);

            return destArray;
        }

        public static int BinarySearch(Array array, Object value, IComparer comparer)
        {
            return BinarySearch(array, 0, array.Length, value, comparer);
        }

        public static int BinarySearch(Array array, int index, int length, Object value, IComparer comparer)
        {
            int lo = index;
            int hi = index + length - 1;
            while (lo <= hi)
            {
                int i = (lo + hi) >> 1;

                int c;
                if (comparer == null)
                {
                    try
                    {
                        var elementComparer = array.GetValue(i) as IComparable;
                        c = elementComparer.CompareTo(value);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("Failed to compare two elements in the array", e);
                    }
                }
                else
                {
                    c = comparer.Compare(array.GetValue(i), value);
                }

                if (c == 0) return i;
                if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        public void CopyTo(Array array, int index)
        {
            Array.Copy(this, 0, array, index, this.Length);
        }

        public IEnumerator GetEnumerator()
        {
            return new SZArrayEnumerator(this);
        }

        public static int IndexOf(Array array, Object value)
        {
            return IndexOf(array, value, 0, array.Length);
        }

        public static int IndexOf(Array array, Object value, int startIndex)
        {
            return IndexOf(array, value, startIndex, array.Length - startIndex);
        }

        public static int IndexOf(Array array, Object value, int startIndex, int count)
        {
            // Try calling a quick native method to handle primitive types.
            int retVal;

            if (TrySZIndexOf(array, startIndex, count, value, out retVal))
            {
                return retVal;
            }

            int endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++)
            {
                Object obj = array.GetValue(i);

                if (Object.Equals(obj, value)) return i;
            }

            return -1;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool TrySZIndexOf(Array sourceArray, int sourceIndex, int count, Object value, out int retVal);

        // This is the underlying Enumerator for all of our array-based data structures (Array, ArrayList, Stack, and Queue)
        // It supports enumerating over an array, a part of an array, and also will wrap around when the endIndex
        // specified is larger than the size of the array (to support Queue's internal circular array)
        internal class SZArrayEnumerator : IEnumerator
        {
            private Array _array;
            private int _index;
            private int _endIndex;
            private int _startIndex;
            private int _arrayLength;

            internal SZArrayEnumerator(Array array)
            {
                _array = array;
                _arrayLength = _array.Length;
                _endIndex = _arrayLength;
                _startIndex = 0;
                _index = -1;
            }

            // By specifying the startIndex and endIndex, the enumerator will enumerate
            // only a subset of the array. Note that startIndex is inclusive, while
            // endIndex is NOT inclusive.
            // For example, if array is of size 5,
            // new SZArrayEnumerator(array, 0, 3) will enumerate through
            // array[0], array[1], array[2]
            //
            // This also supports an array acting as a circular data structure.
            // For example, if array is of size 5,
            // new SZArrayEnumerator(array, 4, 7) will enumerate through
            // array[4], array[0], array[1]
            internal SZArrayEnumerator(Array array, int startIndex, int endIndex)
            {
                _array = array;
                _arrayLength = _array.Length;
                _endIndex = endIndex;
                _startIndex = startIndex;
                _index = _startIndex - 1;
            }

            public bool MoveNext()
            {
                if (_index < _endIndex)
                {
                    _index++;
                    return (_index < _endIndex);
                }

                return false;
            }

            public Object Current
            {
                get
                {
                    return _array.GetValue(_index % _arrayLength);
                }
            }

            public void Reset()
            {
                _index = _startIndex - 1;
            }
        }
    }
}


