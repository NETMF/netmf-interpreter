//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Threading;

namespace Microsoft.SPOT.Debugger
{
    public class BitStream
    {
        class Buffer
        {
            const int c_BufferSize = 512;

            internal Buffer m_next = null;
            internal byte[] m_data;
            internal int m_length;
            internal int m_avail;   //Bits available at last position.

            internal Buffer()
            {
                m_data = new byte[c_BufferSize];
                m_length = 0;
            }

            internal Buffer( byte[] data, int pos, int len )
            {
                m_data = new byte[len];
                m_length = len;

                Array.Copy( data, pos, m_data, 0, len );
            }

            internal Buffer(byte[] data, int pos, int len, int bitsInLastPos)
            {
                if (bitsInLastPos < 1 || bitsInLastPos > 8) { throw new ArgumentException("bits"); }
                m_data = new byte[len];
                m_length = len;
                m_avail = bitsInLastPos;
                Array.Copy( data, pos, m_data, 0, len );
            }
        }

        Buffer m_first;
        Buffer m_current;
        Buffer m_last;
        int m_pos;
        int m_avail;

        bool m_blockingRead;
        bool m_streamEnded;

        private object m_lock;

        public BitStream()
        {
            m_first = new Buffer();
            m_last = m_first;
            m_blockingRead = false;
            m_streamEnded = false;
            m_lock = new object();
            Rewind();
        }

        public BitStream(bool blockingRead) : this()
        {
            m_blockingRead = blockingRead;
        }

        public BitStream( byte[] data, int pos, int len ) : this()
        {
            AppendChunk(data, pos, len * 8);
        }

        public void MarkStreamEnd()
        {
            lock (m_lock)
            {
                m_streamEnded = true;
                Monitor.Pulse(m_lock);
            }
        }

        public void AppendChunk(byte[] data, int pos, int bitlen)
        {
            lock (m_lock)
            {
                if (bitlen > 0)
                {
                    int len = bitlen / 8;
                    int bitsInLast = bitlen % 8;
                    if (bitsInLast == 0)
                    {
                        bitsInLast = 8;
                    }
                    else
                    {
                        len++;
                    }

                    Buffer next = new Buffer(data, pos, len, bitsInLast);

                    if (m_last == null)
                    {
                        m_first = m_last = m_current = next;
                        Rewind();
                    }
                    else
                    {
                        m_last.m_next = next;
                        m_last = next;
                    }
                    Monitor.Pulse(m_lock);
                }
            }
        }

        public void Rewind()
        {
            lock (m_lock)
            {
                m_current = m_first;
                m_pos = -1;
                m_avail = 0;
            }
        }

        public byte[] ToArray()
        {
            /*
             * WARNING: Buffer.m_avail is the number of bits written in the last byte of the buffer.
             * This function doesn't account for buffers whose contents don't end on a byte-boundary.
             * Under normal circumstances this isn't an issue because such a situation only occurs by
             * calling AppendChunk where (bitLen % 8 != 0) on a stream before calling ToArray().
             * AppendChunk is currently exclusively used by the profiling stream, which doesn't use ToArray()
             * so currently there is no problem.
             */

            byte[] res = null;

            lock (m_lock)
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    int tot = 0;
                    Buffer ptr = m_first;

                    while (ptr != null)
                    {
                        if (pass == 1) Array.Copy(ptr.m_data, 0, res, tot, ptr.m_length);

                        tot += ptr.m_length;
                        ptr = ptr.m_next;
                    }

                    if (pass == 0)
                    {
                        res = new byte[tot];
                    }
                }
            }

            return res;
        }
      
        public int BitsAvailable
        {
            get
            {
                int val;

                lock (m_lock)
                {
                    Buffer ptr = m_current;
                    val = 8 * (ptr.m_length - m_pos) + m_avail - 8;

                    while (ptr.m_next != null)
                    {
                        ptr = ptr.m_next;

                        val += 8 * (ptr.m_length - 1) + ptr.m_avail;
                    }
                }

                return val;
            }
        }

        public void WriteBits( uint val, int bits )
        {
            if(bits > 32) throw new ArgumentException( "Max number of bits per write is 32" );

            BinaryFormatter.WriteLine( "OUTPUT: {0:X8} {1}", val & (0xFFFFFFFF >> (32 - bits)), bits );

            int pos = bits;

            lock (m_lock)
            {
                while (bits > 0)
                {
                    while (m_avail == 0)
                    {
                        m_pos++;
                        m_avail = 8;

                        if (m_pos >= m_current.m_data.Length)
                        {
                            m_current.m_avail = 8;  //WriteBits will always try to fill the last bits of a buffer.
                            m_current.m_next = new Buffer();

                            m_current = m_current.m_next;
                            m_pos = 0;
                        }

                        if (m_pos >= m_current.m_length)
                        {
                            m_current.m_length = m_pos + 1;
                        }
                    }

                    int insert = System.Math.Min(bits, m_avail);
                    uint mask = ((1U << insert) - 1U);

                    pos -= insert; m_current.m_data[m_pos] |= (byte)(((val >> pos) & mask) << (m_avail - insert));
                    bits -= insert;
                    m_avail -= insert;
                }

                if (m_pos == m_current.m_length - 1)
                {
                    m_current.m_avail = 8 - m_avail;
                }

                Monitor.Pulse(m_lock);
            }
        }

        public uint ReadBits( int bits )
        {
            if(bits > 32) throw new ArgumentException( "Max number of bits per read is 32" );

            uint val = 0;
            int pos = bits;
            int bitsOrig = bits;

            lock(m_lock)
            {
                while(bits > 0)
                {
                    while(m_avail == 0)
                    {
                        m_pos++;

                        while(m_pos >= m_current.m_length)
                        {
                            if (m_current.m_next == null)
                            {
                                if (m_blockingRead && !m_streamEnded)   //Don't wait if stream has ended.
                                {
                                    Monitor.Wait(m_lock);
                                }
                                else
                                {
                                    throw new EndOfStreamException();
                                }
                            }
                            else
                            {
                                m_current = m_current.m_next;
                                m_pos = 0;
                            }
                        }

                        if (m_pos < m_current.m_length - 1)
                        {
                            m_avail = 8;
                        }
                        else
                        {
                            m_avail = m_current.m_avail;
                        }
                    }

                    int insert = System.Math.Min( bits, m_avail );
                    uint mask = ((1U << insert) - 1U);
                    int shift = m_avail - insert;

                    if (m_pos == m_current.m_length - 1)
                    {
                        shift += 8 - m_current.m_avail;
                    }

                    pos -= insert; val |= (((uint)m_current.m_data[m_pos] >> shift) & mask) << pos;
                    bits -= insert;
                    m_avail -= insert;
                }
            }

            BinaryFormatter.WriteLine( "INPUT: {0:X8}, {1}", val, bitsOrig );

            return val;
        }

        public void WriteArray( byte[] data, int pos, int len )
        {
            lock (m_lock)
            {
                while (len-- > 0)
                {
                    WriteBits((uint)data[pos++], 8);
                }
            }
        }

        public void ReadArray( byte[] data, int pos, int len )
        {
            lock (m_lock)
            {
                while (len-- > 0)
                {
                    data[pos++] = (byte)ReadBits(8);
                }
            }
        }
    }
}