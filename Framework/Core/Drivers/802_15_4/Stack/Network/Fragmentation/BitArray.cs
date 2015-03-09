////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{
    public class BitArray
    {
        private UInt32[] _array;
        private int _size;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">number of elements in the BitArray</param>
        public BitArray(int size, bool initialValue)
        {
            if (size <= 0) { throw new ArgumentOutOfRangeException("size"); }
            _size = size;
            int requiredUInt32ArraySize = (size % 32 == 0) ? (size / 32) : (size / 32) + 1;
            _array = new UInt32[requiredUInt32ArraySize];
            SetAll(initialValue);
        }

        /// <summary>
        /// The number of elements in the bitarray
        /// </summary>
        public int Length { get { return _size; } }

        /// <summary>
        /// Sets the bit at position index to the specified value
        /// </summary>
        /// <param name="index">the position of the bit</param>
        /// <param name="value">value of the bit</param>
        public void Set(int index, bool value)
        {
            if (index >= _size) { throw new ArgumentOutOfRangeException("index"); }
            int indexInArray = index / 32;
            int indexInInteger = index % 32;

            UInt32 flag = (UInt32)1 << indexInInteger;
            if (value)
            {
                _array[indexInArray] |= flag;
            }
            else
            {
                _array[indexInArray] &= (UInt32)(~flag);
            }

        }

        /// <summary>
        /// Clone the BitArray
        /// </summary>
        /// <returns>the clone BitArray</returns>
        public BitArray Clone()
        {
            BitArray clone = new BitArray(_size, false);
            Array.Copy(_array, clone._array, _array.Length);
            return clone;
        }

        /// <summary>
        /// Check whether exactly the same bits are set into both bit arrays.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if all bit values match, false otherwise.</returns>
        public bool HasSameContent(BitArray other)
        {
            if (_size != other._size) { return false; }

            for (int i = 0; i < _array.Length - 1; i++)
            {
                if (_array[i] != other._array[i])
                    return false;
            }

            // last uint32 might not be completed.
            int nbElts = _size % 32;
            UInt32 mask = 0xFFFFFFFF >> (32 - nbElts);
            // set to 0 all the bits which do not correspond to elements
            UInt32 last = mask & _array[_array.Length - 1];
            UInt32 lastOther = mask & other._array[_array.Length - 1];
            return (last == lastOther);
        }

        /// <summary>
        /// Get the value of the bit at position index
        /// </summary>
        /// <param name="index">the position of the bit</param>
        /// <returns>the value of the bit</returns>
        public bool Get(int index)
        {
            if (index >= _size) { throw new ArgumentOutOfRangeException("index"); }
            int indexInArray = index / 32;
            int indexInInteger = index % 32;

            UInt32 flag = (UInt32)1 << indexInInteger;
            return ((_array[indexInArray] & flag) > 0);

        }

        /// <summary>
        /// Sets all bits in the BitArray to the specified value.
        /// </summary>
        /// <param name="value">value of the bits</param>
        public void SetAll(bool value)
        {
            UInt32 val = (value) ? 0xFFFFFFFF : 0x00000000;
            for (int i = 0; i < _array.Length; i++)
            {
                _array[i] = val;
            }
        }

        /// <summary>
        /// Performs the bitwise AND operation on the elements in the current BitArray against
        /// the corresponding elements in the specified BitArray
        /// </summary>
        /// <param name="value">The BitArray with which to perform the bitwise AND operation. </param>
        public void And(BitArray value)
        {
            if (value.Length != this.Length)
                throw new ArgumentException("lenght mismatch.", "value");

            for (int i = 0; i < _array.Length; i++)
            {
                this._array[i] = this._array[i] & value._array[i];
            }
        }

        /// <summary>
        /// Performs the bitwise OR operation on the elements in the current BitArray against
        /// the corresponding elements in the specified BitArray
        /// </summary>
        /// <param name="value">The BitArray with which to perform the bitwise OR operation. </param>
        public void Or(BitArray value)
        {
            if (value.Length != this.Length)
                throw new ArgumentException("lenght mismatch.", "value");

            for (int i = 0; i < _array.Length; i++)
            {
                this._array[i] = this._array[i] | value._array[i];
            }
        }

        /// <summary>
        /// Inverts all the bit values in the current BitArray, so that elements set to true
        /// are changed to false, and elements set to false are changed to true.
        /// </summary>
        public void Not()
        {
            for (int i = 0; i < _array.Length; i++)
            {
                _array[i] = ~_array[i];
            }
        }

        /// <summary>
        /// checks whether all elements are set (value = true) or unset (value = false)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool CheckAllSet(bool value)
        {
            UInt32 val = value ? 0xFFFFFFFF : 0x00000000;

            for (int i = 0; i < _array.Length - 1; i++)
            {
                if (_array[i] != val)
                    return false;
            }

            // last uint32 might not be completed.
            int nbElts = _size % 32;
            UInt32 mask = 0xFFFFFFFF >> (32 - nbElts);
            // set to 0 all the bits which do not correspond to elements
            UInt32 last = mask & _array[_array.Length - 1];
            val &= mask;
            return (val == last);
        }

        /// <summary>
        /// Converts a BitArray into an array of bytes (small endian: first byte includes first elements)
        /// </summary>
        /// <param name="bitArray">the BitArray to be converted</param>
        /// <returns>the arry of bytes encoding the BitArray</returns>
        static public byte[] ToByteArray(BitArray bitArray)
        {
            int byteArrayLength = ((bitArray._size % 8) == 0) ? (bitArray._size / 8) : 1 + (bitArray._size / 8);
            byte[] byteArray = new byte[byteArrayLength];
            int index = 0;
            for (int i = 0; index < byteArrayLength; i++)
            {
                UInt32 val = bitArray._array[i];
                for (int j = 0; j < 4 && index < byteArrayLength; j++, index++)
                {
                    byteArray[index] = (byte)(val & 0x000000FF);
                    val = val >> 8;
                }
            }

            return byteArray;
        }

        /// <summary>
        /// Converts a byte array (small indian: first byte includes first elements) into a BitArray
        /// </summary>
        /// <param name="nbElements">number of elements</param>
        /// <param name="array">encoding of the BitArray</param>
        /// <returns>decoded bitarray</returns>
        static public BitArray FromByteArray(int nbElements, byte[] array)
        {
            if (nbElements > array.Length * 8)
                throw new ArgumentException("nbElements");
            BitArray bitArray = new BitArray(nbElements, false);
            int index = 0;
            int arrayLength = array.Length;
            for (int i = 0; index < arrayLength; i++)
            {
                for (int j = 0; j < 4 && index < array.Length; j++, index++)
                {
                    bitArray._array[i] |= ((UInt32)array[index]) << (j * 8);
                }
            }

            return bitArray;
        }

    }
}


