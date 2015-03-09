////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//#define BUFFER_BUILDER_TRACING

using System.IO;
using System.Text;
using Microsoft.SPOT;

namespace System.Xml
{
    //
    //  Buffer Builder
    //
    // BufferBuilder is a replacement for StringBuilder for cases when large strings can occur.
    // StringBuilder stores the string that is being built in one large chunk of memory. If it needs more memory,
    // it allocates a new chunk of double size and copies the data into it. This results in bad perf and
    // memory constumption in case the string is very large (>85kB). Large objects are allocated on Large Object
    // Heap and are not freed by GC as fast as smaller objects.
    //
    // BufferBuilder uses a StringBuilder as long as the stored string is smaller that 64kB. If the final string
    // should be bigger that that, it stores the data in a list of char[] arrays. A StringBuilder object still needs to be
    // used in order to create the final string in ToString methods, but this is ok since at that point
    // we already know the resulting string length and we can initialize the StringBuilder with the correct
    // capacity.
    //
    // The BufferBuilder is designed for reusing. The Clear method will clear the state of the builder.
    // The next string built by BufferBuilder will reuse the string builder and the buffer chunks allocated
    // in the previous uses. (The string builder it not reused when it was last used to create a string >64kB because
    // setting Length=0 on the string builder makes it allocate the big string again.)
    // When the buffer chunks are not in use, they are stored as WeakReferences so they can be freed by GC
    // in case memory-pressure situation happens.

#if BUFFER_BUILDER_TRACING
    public class BufferBuilder {
#else
    internal class BufferBuilder
    {
#endif
        //
        // Private types
        //
        private struct Buffer
        {
            internal char[] buffer;
            internal WeakReference recycledBuffer;
        }

        //
        // Fields
        //
        Buffer[] buffers;
        int buffersCount;
        char[] lastBuffer;
        int lastBufferIndex;
        int length;

#if BUFFER_BUILDER_TRACING
//
// Tracing
//
        public static TextWriter s_TraceOutput = null;
        static int minLength = int.MaxValue;
        static int maxLength;
        static int totalLength;
        static int toStringCount;
        static int totalAppendCount;
#endif

        //
        // Constants
        //
#if DEBUG
        // make it easier to catch buffer-related bugs on debug builds
        const int BufferSize = 4 * 1024;
#else
        const int BufferSize = 16*1024;
#endif
        const int InitialBufferArrayLength = 4;
        const int MaxStringBuilderLength = BufferSize;
        const int DefaultSBCapacity = 16;

        //
        // Constructor
        //
        public BufferBuilder()
        {
#if BUFFER_BUILDER_TRACING
#if DESKTOP
            if ( s_TraceOutput != null ) {
                s_TraceOutput.WriteLine( "----------------------------\r\nnew BufferBuilder()\r\n----------------------------" );
            }

#endif
#endif
        }

        //
        // Properties
        //
        public int Length
        {
            get
            {
                return length;
            }

            set
            {
#if BUFFER_BUILDER_TRACING
#if DESKTOP
                if ( s_TraceOutput != null ) {
                    s_TraceOutput.WriteLine( "BufferBuilder.Length = " + value );
                }

#endif
#endif

                if (value < 0 || value > length)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value == 0)
                {
                    Clear();
                }
                else
                {
                    SetLength(value);
                }
            }
        }

        //
        // Public methods
        //

        public void Append(char value)
        {
#if BUFFER_BUILDER_TRACING
#if DESKTOP
            if ( s_TraceOutput != null ) {
                s_TraceOutput.WriteLine( "BufferBuilder.Append\tLength = 1\tchar '" + value.ToString() + "'" );
                totalAppendCount++;
            }

#endif
#endif
            if (lastBuffer == null)
            {
                CreateBuffers();
            }

            if (lastBufferIndex == lastBuffer.Length)
            {
                AddBuffer();
            }

            lastBuffer[lastBufferIndex++] = value;

            length++;
        }

        public void Append(char[] value)
        {
            Append(value, 0, value.Length);
        }

        public void Append(char[] value, int start, int count)
        {
#if BUFFER_BUILDER_TRACING
#if DESKTOP
            if ( s_TraceOutput != null ) {
                s_TraceOutput.WriteLine( "BufferBuilder.Append\tLength = " + count + "\t char array \"" + new string( value, start, count ) + "\"" );
                totalAppendCount++;
            }

#endif
#endif
            if (value == null)
            {
                if (start == 0 && count == 0)
                {
                    return;
                }

                throw new ArgumentNullException("value");
            }

            if (count == 0)
            {
                return;
            }

            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start");
            }

            if (count < 0 || start + count > value.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (lastBuffer == null)
            {
                CreateBuffers();
            }

            int copyCount = 0;
            int src = start;

            while (count > 0)
            {
                if (lastBufferIndex >= lastBuffer.Length)
                {
                    AddBuffer();
                }

                copyCount = count;
                int free = lastBuffer.Length - lastBufferIndex;
                if (free < copyCount)
                {
                    copyCount = free;
                }

                Array.Copy(value, src, lastBuffer, lastBufferIndex, copyCount);

                src += copyCount;
                lastBufferIndex += copyCount;
                length += copyCount;
                count -= copyCount;
            }

        }

        public void Append(string value)
        {
            Append(value, 0, value.Length);
        }

        public void Append(string value, int start, int count)
        {
#if BUFFER_BUILDER_TRACING
#if DESKTOP
            if ( s_TraceOutput != null ) {
                s_TraceOutput.WriteLine( "BufferBuilder.Append\tLength = " + count + "\t string fragment \"" + value.Substring( start, count ) + "\"" );
                totalAppendCount++;
            }

#endif
#endif
            if (value == null)
            {
                if (start == 0 && count == 0)
                {
                    return;
                }

                throw new ArgumentNullException("value");
            }

            if (count == 0)
            {
                return;
            }

            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start");
            }

            if (count < 0 || start + count > value.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (lastBuffer == null)
            {
                CreateBuffers();
            }

            int copyCount = 0;
            int src = start;

            while (count > 0)
            {
                if (lastBufferIndex >= lastBuffer.Length)
                {
                    AddBuffer();
                }

                copyCount = count;
                int free = lastBuffer.Length - lastBufferIndex;
                if (free < copyCount)
                {
                    copyCount = free;
                }

                for (int i = 0; i < copyCount; i++)
                {
                    lastBuffer[lastBufferIndex++] = value[src++];
                }

                length += copyCount;
                count -= copyCount;
            }

        }

        public void Clear()
        {
            if (lastBuffer != null)
            {
                ClearBuffers();
            }

            length = 0;
        }

        internal void ClearBuffers()
        {
            if (buffers != null)
            {
                // recycle all but the first the buffer
                for (int i = 0; i < buffersCount; i++)
                {
                    Recycle(buffers[i]);
                }

                lastBuffer = null;
            }
            else
            {
                // just one buffer allocated with no buffers array -> no recycling
            }

            lastBufferIndex = 0;
            buffersCount = 0;
        }

        public override string ToString()
        {
            string returnString;
            if (buffersCount == 0 || (buffersCount == 1 && lastBufferIndex == 0))
            {
                returnString = "";
            }
            else if (buffersCount == 1)
            {
                returnString = new string(buffers[0].buffer, 0, length);
            }
            else
            {
                returnString = "";
                int charsLeft = length;
                for (int i = 0; i < buffersCount - 1; i++)
                {
                    char[] buf = buffers[i].buffer;
                    int len = (i == buffersCount - 1) ? lastBufferIndex + 1 : buf.Length;

                    returnString = returnString + new string(buf, 0, len);
                }
            }

            return returnString;
        }

        public string ToString(int startIndex, int len)
        {
#if BUFFER_BUILDER_TRACING
#if DESKTOP
            if ( s_TraceOutput != null ) {
                s_TraceOutput.WriteLine( "BufferBuilder.ToString( " + startIndex + ", " + len + " )" );
            }

#endif
#endif
            if (startIndex < 0 || startIndex >= length)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            if (len < 0 || startIndex + len > length)
            {
                throw new ArgumentOutOfRangeException("len");
            }

            if ((buffersCount == 0) || (buffersCount == 1 && lastBufferIndex == 0))
            {
                return "";
            }
            else if (buffersCount == 1)
            {
                return new string(buffers[0].buffer, startIndex, len);
            }
            else
            {

                string returnString = "";
                int i;
                for (i = 0; i < buffersCount; i++)
                {
                    if (startIndex < buffers[i].buffer.Length)
                    {
                        break;
                    }

                    startIndex -= buffers[i].buffer.Length;
                }

                if (i < buffersCount)
                {
                    int charsLeft = len;
                    for (; i < buffersCount && charsLeft > 0; i++)
                    {
                        char[] buf = buffers[i].buffer;
                        int copyCount = (buf.Length < charsLeft) ? buf.Length : charsLeft;
                        returnString = returnString + new string(buf, startIndex, copyCount);
                        startIndex = 0;
                        charsLeft -= copyCount;
                    }
                }

                return returnString;
            }
        }

        //
        // Private implementation methods
        //
        private void CreateBuffers()
        {
            Debug.Assert(lastBuffer == null);
            if (buffers == null)
            {
                lastBuffer = new char[BufferSize];
                buffers = new Buffer[InitialBufferArrayLength];
                buffers[0].buffer = lastBuffer;
                buffersCount = 1;
            }
            else
            {
                AddBuffer();
            }
        }

        private void AddBuffer()
        {
            Debug.Assert(buffers != null);

            // check the buffers array it its big enough
            if (buffersCount + 1 == buffers.Length)
            {
                Buffer[] newBuffers = new Buffer[buffers.Length * 2];
                Array.Copy(buffers, 0, newBuffers, 0, buffers.Length);
                buffers = newBuffers;
            }

            // use the recycled buffer if we have one
            char[] newBuffer;
            if (buffers[buffersCount].recycledBuffer != null)
            {
                newBuffer = (char[])buffers[buffersCount].recycledBuffer.Target;
                if (newBuffer != null)
                {
                    buffers[buffersCount].recycledBuffer.Target = null;
                    goto End;
                }
            }

            newBuffer = new char[BufferSize];

        End:
            // add the buffer to the list
            lastBuffer = newBuffer;
            buffers[buffersCount++].buffer = newBuffer;
            lastBufferIndex = 0;
        }

        private void Recycle(Buffer buf)
        {
            // recycled buffers are kept as WeakReferences
            if (buf.recycledBuffer == null)
            {
                buf.recycledBuffer = new WeakReference(buf.buffer);
            }
            else
            {
                buf.recycledBuffer.Target = buf.buffer;
            }

            buf.buffer = null;
        }

        private void SetLength(int newLength)
        {
            Debug.Assert(newLength <= length);

            if (newLength == length)
            {
                return;
            }

            int newLastIndex = newLength;
            int i;
            for (i = 0; i < buffersCount; i++)
            {
                if (newLastIndex < buffers[i].buffer.Length)
                {
                    break;
                }

                newLastIndex -= buffers[i].buffer.Length;
            }

            if (i < buffersCount)
            {
                lastBuffer = buffers[i].buffer;
                lastBufferIndex = newLastIndex;
                i++;
                int newBuffersCount = i;
                for (; i < buffersCount; i++)
                {
                    Recycle(buffers[i]);
                }

                buffersCount = newBuffersCount;
            }

            length = newLength;
        }

        /***************
                internal static unsafe void wstrcpy( char *dmem, char *smem, int charCount ) {
                    if ( charCount > 0 ) {
                        if ( ( ( (int)dmem ^ (int)smem ) & 3 ) == 0 ) {
                            while ( ( (int) dmem & 3 ) != 0 && charCount > 0) {
                                dmem[0] = smem[0];
                                dmem += 1;
                                smem += 1;
                                charCount -= 1;
                            }

                            if ( charCount >= 8 ) {
                                charCount -= 8;
                                do {
                                    ((uint*)dmem)[0] = ((uint*)smem)[0];
                                    ((uint*)dmem)[1] = ((uint*)smem)[1];
                                    ((uint*)dmem)[2] = ((uint*)smem)[2];
                                    ((uint*)dmem)[3] = ((uint*)smem)[3];
                                    dmem += 8;
                                    smem += 8;
                                    charCount -= 8;
                                } while ( charCount >= 0 );
                            }

                            if ( ( charCount & 4 ) != 0 ) {
                                ((uint*)dmem)[0] = ((uint*)smem)[0];
                                ((uint*)dmem)[1] = ((uint*)smem)[1];
                                dmem += 4;
                                smem += 4;
                            }

                            if ( ( charCount & 2 ) != 0) {
                                ((uint*)dmem)[0] = ((uint*)smem)[0];
                                dmem += 2;
                                smem += 2;
                            }
                        }
                        else {
                            if ( charCount >= 8 ) {
                                charCount -= 8;
                                do {
                                    dmem[0] = smem[0];
                                    dmem[1] = smem[1];
                                    dmem[2] = smem[2];
                                    dmem[3] = smem[3];
                                    dmem[4] = smem[4];
                                    dmem[5] = smem[5];
                                    dmem[6] = smem[6];
                                    dmem[7] = smem[7];
                                    dmem += 8;
                                    smem += 8;
                                    charCount -= 8;
                                }

                                while ( charCount >= 0 );
                            }

                            if ( ( charCount & 4) != 0 ) {
                                dmem[0] = smem[0];
                                dmem[1] = smem[1];
                                dmem[2] = smem[2];
                                dmem[3] = smem[3];
                                dmem += 4;
                                smem += 4;
                            }

                            if ( ( charCount & 2 ) != 0 ) {
                                dmem[0] = smem[0];
                                dmem[1] = smem[1];
                                dmem += 2;
                                smem += 2;
                            }
                        }

                        if ( ( charCount & 1 ) != 0 ) {
                            dmem[0] = smem[0];
                        }
                    }
                }

        *******************/

#if BUFFER_BUILDER_TRACING
        public static int ToStringCount {
            get {
                return toStringCount;
            }
        }

        public static double AvgAppendCount {
            get {
                return toStringCount == 0 ? 0 : (double)totalAppendCount / toStringCount;
            }
        }

        public static int AvgLength {
            get {
                return toStringCount == 0 ? 0 : totalLength / toStringCount;
            }
        }

        public static int MaxLength {
            get {
                return maxLength;
    }
}

        public static int MinLength {
            get {
                return minLength;
            }
        }

#endif
    }
}


