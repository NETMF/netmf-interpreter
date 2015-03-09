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
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;
using System.Runtime.Serialization;

namespace Microsoft.SPOT.Debugger
{
    public class AbortHandler
    {
        public class Report
        {
            public const int c_Noise      = 0x00000000;
            public const int c_ReadMemory = 0x00000001;
            public const int c_Register   = 0x00000002;
            public const int c_Layout     = 0x00000003;
            public const int c_Error      = 0x00000004;

            public int      type;
            public string   line;
            public string[] values;

            public Report( int type, string line, Regex regex )
            {
                this.type = type;
                this.line = line;

                if(regex != null)
                {
                    GroupCollection group = regex.Match( line ).Groups;

                    this.values = new string[group.Count];

                    for(int i=0; i<group.Count; i++)
                    {
                        this.values[i] = group[i].Value;
                    }
                }
            }
        }

        private Engine         m_eng;
        private bool           m_deviceRunning;

        private ArrayList      m_reports = new ArrayList();
        private AutoResetEvent m_ready   = new AutoResetEvent( false );

        private byte[]         m_buffer = new byte[1024];
        private int            m_pos    = 0;
                              
        private Regex          m_re_ReadMemory = new Regex( "^\\[0x([0-9a-fA-F]*)\\](.*)"    , RegexOptions.IgnoreCase );
        private Regex          m_re_Register   = new Regex( "^([^=]+)=([0-9a-fA-F]*)"        , RegexOptions.IgnoreCase );
        private Regex          m_re_Layout     = new Regex( "^(rb|rs|fb|fs)=0x([0-9a-fA-F]*)", RegexOptions.IgnoreCase );
        private Regex          m_re_Error      = new Regex( "^ERROR: (.*)"                   , RegexOptions.IgnoreCase );

        public AbortHandler( Engine eng, bool deviceRunning )
        {
            m_eng           = eng;
            m_deviceRunning = deviceRunning;
        }

        public void Dispose()
        {
            Stop();
        }

#if ABORTHANDLER_RAWDUMP
        Stream m_stream;
#endif

        public void Start()
        {
#if ABORTHANDLER_RAWDUMP
            m_stream = new FileStream( @"RawDump.bin", FileMode.Create );
#endif

            m_eng.WaitForPort();

            m_eng.OnNoise += new NoiseEventHandler( this.Process );
        }

        public void Stop()
        {
            m_eng.OnNoise -= new NoiseEventHandler( this.Process );

#if ABORTHANDLER_RAWDUMP
            if(m_stream != null)
            {
                m_stream.Flush();
                m_stream.Close();
            }
#endif
        }

        private void Process( byte[] buf, int offset, int count )
        {
#if ABORTHANDLER_RAWDUMP
            if(m_stream != null)
            {
                m_stream.Write( buf, offset, count );
                m_stream.Flush();
            }
#endif

            while(count-- > 0)
            {
                byte c = buf[offset++];


                if(c == '\r') continue;

                if(c == '\n')
                {
                    string line  = Encoding.UTF8.GetString( m_buffer, 0, m_pos );
                    Regex  regex = null;
                    int    rep;

                    if(m_re_ReadMemory.IsMatch( line ))
                    {
                        rep   = Report.c_ReadMemory;
                        regex = m_re_ReadMemory;
                    }
                    else if(m_re_Layout.IsMatch( line ))
                    {
                        rep   = Report.c_Layout;
                        regex = m_re_Layout;
                    }
                    else if(m_re_Register.IsMatch( line ))
                    {
                        rep   = Report.c_Register;
                        regex = m_re_Register;
                    }
                    else if(m_re_Error.IsMatch( line ))
                    {
                        rep   = Report.c_Error;
                        regex = m_re_Error;
                    }
                    else
                    {
                        rep = Report.c_Noise;
                    }

                    lock(m_reports)
                    {
                        m_reports.Add( new Report( rep, line, regex ) );

                        m_ready.Set();
                    }

                    m_pos = 0;
                }
                else if(m_pos >= m_buffer.Length)
                {
                    m_pos = 0;
                }
                else
                {
                    m_buffer[m_pos++] = c;
                }
            }
        }

        public Report GetReport( int timeout )
        {
            while(true)
            {
                lock(m_reports)
                {
                    if(m_reports.Count > 0)
                    {
                        Report r = (Report)m_reports[0]; m_reports.RemoveAt( 0 );

                        return r;
                    }
                }

                if(timeout <= 0) return null;

                DateTime start = DateTime.Now;

                m_ready.WaitOne( timeout, false );

                TimeSpan diff = DateTime.Now - start;

                timeout -= (int)diff.TotalMilliseconds;
            }
        }

        public bool ReadMemory( uint address, byte[] data, int offset, int count )
        {
            if(m_deviceRunning)
            {
                byte[] buf;

                if(m_eng.ReadMemory( address, (uint)count, out buf ))
                {
                    Array.Copy( buf, 0, data, offset, count );

                    return true;
                }

                return false;
            }
            else
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter( stream, Encoding.Unicode );

                writer.Write( (byte)'M'            );
                writer.Write( (byte)0              );
                writer.Write( (byte)0              );
                writer.Write( (byte)0              );
                writer.Write( (uint) address       );
                writer.Write( (uint)(address+count));
                writer.Write( (byte)'\n'           );
                writer.Write( (byte)'\n'           );
                writer.Write( (byte)'\n'           );
                writer.Write( (byte)'\n'           );

                writer.Flush();

                SendBuffer( stream.ToArray() );

                while(count > 0)
                {
                    Report r = GetReport( 100 ); if(r == null) return false;

                    if(r.type == Report.c_ReadMemory)
                    {
                        try
                        {
                            uint   addressReply = UInt32.Parse( r.values[1], System.Globalization.NumberStyles.HexNumber );
                            string dataText     =               r.values[2];

                            if(addressReply == address)
                            {
                                int size = dataText.Length / 2;

                                for(int pos=0; pos<size; pos++)
                                {
                                    data[offset+pos] = Byte.Parse( dataText.Substring( pos*2, 2 ), System.Globalization.NumberStyles.HexNumber );
                                }

                                address += (uint)size;
                                offset  +=       size;
                                count   -=       size;
                            }
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else if(r.type == Report.c_Error)
                    {
                        return false;
                    }
                    else if(r.type == Report.c_Noise)
                    {
                        m_eng.InjectMessage( "{0}\r\n", r.line );
                    }
                }

                return true;
            }
        }

        public bool ReadRegisters( uint[] registers, out uint cpsr, out uint BWA, out uint BWC )
        {
            if(m_deviceRunning)
            {
                cpsr = 0;
                BWA  = 0;
                BWC  = 0;

                return true;
            }
            else
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter( stream, Encoding.Unicode );
                int          count  = 16 + 3;

                writer.Write( (byte)'R'  );
                writer.Write( (byte)'\n' );
                writer.Write( (byte)'\n' );
                writer.Write( (byte)'\n' );
                writer.Write( (byte)'\n' );

                writer.Flush();

                SendBuffer( stream.ToArray() );

                cpsr = 0;
                BWA  = 0;
                BWC  = 0;

                while(count > 0)
                {
                    Report r = GetReport( 100 ); if(r == null) return false;

                    if(r.type == Report.c_Register )
                    {
                        try
                        {
                            string reg  =               r.values[1];
                            uint   data = UInt32.Parse( r.values[2], System.Globalization.NumberStyles.HexNumber );

                            switch(reg)
                            {
                                case "r0"  : registers[ 0] = data; break;
                                case "r1"  : registers[ 1] = data; break;
                                case "r2"  : registers[ 2] = data; break;
                                case "r3"  : registers[ 3] = data; break;
                                case "r4"  : registers[ 4] = data; break;
                                case "r5"  : registers[ 5] = data; break;
                                case "r6"  : registers[ 6] = data; break;
                                case "r7"  : registers[ 7] = data; break;
                                case "r8"  : registers[ 8] = data; break;
                                case "r9"  : registers[ 9] = data; break;
                                case "r10" : registers[10] = data; break;
                                case "r11" : registers[11] = data; break;
                                case "r12" : registers[12] = data; break;
                                case "sp"  : registers[13] = data; break;
                                case "lr"  : registers[14] = data; break;
                                case "pc"  : registers[15] = data; break;
                                case "cpsr": cpsr          = data; break;
                                case "BWA" : BWA           = data; break;
                                case "BWC" : BWC           = data; break;

                                default: return false;
                            }

                            count--;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else if(r.type == Report.c_Error)
                    {
                        return false;
                    }
                    else if(r.type == Report.c_Noise)
                    {
                        m_eng.InjectMessage( "{0}\r\n", r.line );
                    }
                }

                return true;
            }
        }

        public bool ReadLayout( out uint flashBase, out uint flashSize, out uint ramBase, out uint ramSize )
        {
            flashBase = flashSize = ramBase = ramSize = 0;

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter( stream, Encoding.Unicode );
            int          count  = 4;

            writer.Write( (byte)'L'  );
            writer.Write( (byte)'\n' );
            writer.Write( (byte)'\n' );
            writer.Write( (byte)'\n' );
            writer.Write( (byte)'\n' );

            writer.Flush();

            SendBuffer( stream.ToArray() );

            while(count > 0)
            {
                Report r = GetReport( 100 ); if(r == null) return false;

                if(r.type == Report.c_Layout )
                {
                    try
                    {
                        string reg  =               r.values[1];
                        uint   data = UInt32.Parse( r.values[2], System.Globalization.NumberStyles.HexNumber );

                        switch(reg)
                        {
                            case "rb": ramBase   = data; break;
                            case "rs": ramSize   = data; break;
                            case "fb": flashBase = data; break;
                            case "fs": flashSize = data; break;

                            default: return false;
                        }

                        count--;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else if(r.type == Report.c_Error)
                {
                    return false;
                }
                else if(r.type == Report.c_Noise)
                {
                    m_eng.InjectMessage( "{0}\r\n", r.line );
                }
            }

            return true;
        }

        void SendBuffer( byte[] buf )
        {
            m_eng.SendRawBuffer( buf );
        }
    }
}
