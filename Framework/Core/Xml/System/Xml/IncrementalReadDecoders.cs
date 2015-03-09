////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.SPOT;

namespace System.Xml
{

    //
    //  IncrementalReadDecoder abstract class
    //
    internal abstract class IncrementalReadDecoder
    {
        internal abstract int DecodedCount { get; }
        internal abstract bool IsFull { get; }
        internal abstract void SetNextOutputBuffer(Array array, int offset, int len);
        internal abstract int Decode(char[] chars, int startPos, int len);
        internal abstract int Decode(string str, int startPos, int len);
        internal abstract void Reset();
    }

    //
    //  Dummy IncrementalReadDecoder
    //
    internal class IncrementalReadDummyDecoder : IncrementalReadDecoder
    {
        internal override int DecodedCount { get { return -1; } }
        internal override bool IsFull { get { return false; } }
        internal override void SetNextOutputBuffer(Array array, int offset, int len) { }
        internal override int Decode(char[] chars, int startPos, int len) { return len; }
        internal override int Decode(string str, int startPos, int len) { return len; }
        internal override void Reset() { }
    }

    //
    //  IncrementalReadDecoder for ReadChars
    //
    internal class IncrementalReadCharsDecoder : IncrementalReadDecoder
    {
        char[] buffer;
        int startIndex;
        int curIndex;
        int endIndex;

        internal IncrementalReadCharsDecoder()
        {
        }

        internal override int DecodedCount
        {
            get
            {
                return curIndex - startIndex;
            }
        }

        internal override bool IsFull
        {
            get
            {
                return curIndex == endIndex;
            }
        }

        internal override int Decode(char[] chars, int startPos, int len)
        {
            Debug.Assert(len > 0);

            int copyCount = endIndex - curIndex;
            if (copyCount > len)
            {
                copyCount = len;
            }

            Array.Copy(chars, startPos, buffer, curIndex, copyCount);
            curIndex += copyCount;

            return copyCount;
        }

        internal override int Decode(string str, int startPos, int len)
        {
            Debug.Assert(len > 0);

            int copyCount = endIndex - curIndex;
            if (copyCount > len)
            {
                copyCount = len;
            }

            for (int i = 0; i < copyCount; i++)
            {
                buffer[curIndex + i] = str[startPos + i];
            }

            curIndex += copyCount;

            return copyCount;
        }

        internal override void Reset()
        {
        }

        internal override void SetNextOutputBuffer(Array buffer, int index, int count)
        {
            Debug.Assert((buffer as char[]) != null);
            this.buffer = (char[])buffer;
            this.startIndex = index;
            this.curIndex = index;
            this.endIndex = index + count;
        }
    }

}


