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
    public class PortBooter
    {
        public delegate void ProgressEventHandler( SRecordFile.Block bl, int offset, bool fLast );

        public class Report
        {
            public enum State
            {
                Banner    ,
                EntryPoint,
                ACK       ,
                NACK      ,
                CRC       ,
                Noise     ,
            }

            public State  type;
            public uint   address;
            public string line;
        }

        private Engine                     m_eng;

        private event ProgressEventHandler m_eventProgress;

        private ArrayList                  m_reports = new ArrayList();
        private AutoResetEvent             m_ready   = new AutoResetEvent( false );

        private byte[]                     m_buffer  = new byte[1024];
        private int                        m_pos     = 0;
        private byte                       m_lastEOL = 0;
                              
        private Regex                      m_re_Banner1    = new Regex( "PortBooter v[0-9]+\\.[0-9]+", RegexOptions.None       );
        private Regex                      m_re_Banner2    = new Regex( "Waiting .* for hex upload" , RegexOptions.IgnoreCase );
        private Regex                      m_re_EntryPoint = new Regex( "X start ([0-9a-fA-F]*)"    , RegexOptions.IgnoreCase );
        private Regex                      m_re_ACK        = new Regex( "X ack ([0-9a-fA-F]*)"      , RegexOptions.IgnoreCase );
        private Regex                      m_re_NACK       = new Regex( "X nack ([0-9a-fA-F]*)"     , RegexOptions.IgnoreCase );
        private Regex                      m_re_CRC        = new Regex( "X crc ([0-9a-fA-F]*)"      , RegexOptions.IgnoreCase );

        public PortBooter( Engine eng )
        {
            m_eng = eng;
        }

        public void Dispose()
        {
            Stop();
        }

#if PortBooter_RAWDUMP
        Stream m_stream;
#endif

        public void Start()
        {
#if PortBooter_RAWDUMP
            m_stream = new FileStream( @"RawDump.bin", FileMode.Create );
#endif

            m_eng.WaitForPort();

            m_eng.ConfigureXonXoff( true );
            m_eng.OnNoise += new NoiseEventHandler( this.Process );
        }

        public void Stop()
        {
            m_eng.OnNoise -= new NoiseEventHandler( this.Process );
            m_eng.ConfigureXonXoff( false );

#if PortBooter_RAWDUMP
            if(m_stream != null)
            {
                m_stream.Flush();
                m_stream.Close();
            }
#endif
        }

        public event ProgressEventHandler OnProgress
        {
            add
            {
                m_eventProgress += value;
            }

            remove
            {
                m_eventProgress -= value;
            }
        }

        private void Process( byte[] buf, int offset, int count )
        {
#if PortBooter_RAWDUMP
            if(m_stream != null)
            {
                m_stream.Write( buf, offset, count );
                m_stream.Flush();
            }
#endif

            while(count-- > 0)
            {
                byte c = buf[offset++];

                m_buffer[m_pos] = c;

                if(c == '\n' || c == '\r')
                {
                    if(m_pos == 0)
                    {
                        if(m_lastEOL == '\r' && c == '\n')
                        {
                            m_lastEOL = c;
                            continue;
                        }
                    }

                    m_lastEOL = c;

                    string line = Encoding.UTF8.GetString( m_buffer, 0, m_pos );
                    Report r    = null;

                    if(m_re_Banner1.IsMatch( line ) || m_re_Banner2.IsMatch( line ))
                    {
                        r = new Report();

                        r.type = Report.State.Banner;
                    }
                    else if(m_re_EntryPoint.IsMatch( line ))
                    {
                        GroupCollection group = m_re_EntryPoint.Match( line ).Groups;

                        r = new Report();

                        r.type    = Report.State.EntryPoint;
                        r.address = UInt32.Parse( group[1].Value, System.Globalization.NumberStyles.HexNumber );
                    }
                    else if(m_re_ACK.IsMatch( line ))
                    {
                        GroupCollection group = m_re_ACK.Match( line ).Groups;

                        r = new Report();

                        r.type    = Report.State.ACK;
                        r.address = UInt32.Parse( group[1].Value, System.Globalization.NumberStyles.HexNumber );
                    }
                    else if(m_re_NACK.IsMatch( line ))
                    {
                        GroupCollection group = m_re_NACK.Match( line ).Groups;

                        r = new Report();

                        r.type    = Report.State.NACK;
                        r.address = UInt32.Parse( group[1].Value, System.Globalization.NumberStyles.HexNumber );
                    }
                    else if(m_re_CRC.IsMatch( line ))
                    {
                        GroupCollection group = m_re_CRC.Match( line ).Groups;

                        r = new Report();

                        r.type    = Report.State.CRC;
                        r.address = UInt32.Parse( group[1].Value, System.Globalization.NumberStyles.HexNumber );
                    }
                    else
                    {
                        r = new Report();

                        r.type = Report.State.Noise;
                        r.line = line;
                    }

                    if(r != null)
                    {
                        lock(m_reports)
                        {
                            m_reports.Add( r );

                            m_ready.Set();
                        }
                    }

                    m_pos = 0;
                }
                else if(++m_pos == m_buffer.Length)
                {
                    m_pos = 0;
                }
            }
        }

        public bool WaitBanner( int tries, int timeout )
        {
            while(tries > 0)
            {
                Report r = GetReport( timeout );

                if(r == null)
                {
                    m_eng.InjectMessage( "Synching...\r\n" );
                }
                else if(r.type == Report.State.Noise)
                {
                    m_eng.InjectMessage( "{0}", r.line );
                }
                else if(r.type == Report.State.Banner)
                {
                    for(int i=0; i<10; i++)
                    {
                        SendString( "ZENFLASH\r\n" );
                        Thread.Sleep( 20 );
                    }

                    return true;
                }
            }

            return false;
        }

        public bool Program( ArrayList blocks )
        {
            ProgressEventHandler eventProgress = m_eventProgress;

            const int maxpipeline  = 4;
            const int pipelineSize = 128;

            int[] outstanding = new int[maxpipeline];

            for(int i=0; i<maxpipeline; i++)
            {
                outstanding[i] = -1;
            }

            foreach(SRecordFile.Block bl in blocks)
            {
                byte[] buf      = bl.data.ToArray();
                uint   address  = bl.address;
                int    offset   = 0;
                int    count    = buf.Length;
                int    pipeline = maxpipeline;

                while(offset < count)
                {
                    int pending = 0;

                    for(int i=0; i<pipeline; i++)
                    {
                        if(outstanding[i] == -1)
                        {
                            int pos = offset + i * pipelineSize;
                            int len = System.Math.Min( count - pos, pipelineSize );

                            if(len <= 0) break;

                            WriteMemory( (uint)(address + pos), buf, pos, len );

                            outstanding[i] = i;
                        }

                        pending++;
                    }

                    if(pending == 0) break;

                    Report r = GetReport( 5000 );

                    if(r == null)
                    {
                        for(int i=0; i<pipeline; i++)
                        {
                            outstanding[i] = -1;
                        }
                    }
                    else if(r.type == Report.State.Noise)
                    {
                        m_eng.InjectMessage( "{0}", r.line );
                    }
                    else if(r.type == Report.State.CRC)
                    {
                        int restart = 0;

                        for(int i=0; i<pipeline; i++)
                        {
                            if(r.address == address + offset + i * pipelineSize)
                            {
                                restart = i;
                                break;
                            }
                        }

                        int pos = outstanding[restart];

                        for(int i=restart; i<pipeline; i++)
                        {
                            outstanding[i] = -1;
                        }

                        //
                        // Throttle back pipelining in case of errors.
                        //
                        if(pipeline > pos+1 && pipeline > 1)
                        {
                            pipeline--;
                        }
                    }
                    else if(r.address == address + offset)
                    {
                        if(r.type == Report.State.ACK)
                        {
                            offset += pipelineSize;

                            for(int i=1; i<pipeline; i++)
                            {
                                outstanding[i-1] = outstanding[i];
                            }

                            outstanding[pipeline-1] = -1;

                            if(eventProgress != null) eventProgress( bl, System.Math.Min( count, offset ), false );
                        }
                        else if(r.type == Report.State.NACK)
                        {
                        }
                    }
                }

                if(eventProgress != null) eventProgress( bl, count, true );
            }

            return true;
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

        public void SendString( string s )
        {
            SendBuffer( Encoding.UTF8.GetBytes( s ) );
        }

        public void Execute( uint address )
        {
            WriteMemory( address, null, 0, 0 );
        }

        public void WriteMemory( uint address, byte[] data, int offset, int count )
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter( stream, Encoding.Unicode );

            uint crc = 0;

            crc -=         address;
            crc -= (ushort)count;

            for(int i=0; i<count; i++)
            {
                crc -= data[offset+i];
            }

            writer.Write( (byte  )'X'         );
            writer.Write( (uint  )address     );
            writer.Write( (ushort)count       );
            writer.Write( (uint  )crc         );

            if(count > 0)
            {
                writer.Write( data, offset, count );
            }

            writer.Flush();

            SendBuffer( stream.ToArray() );
        }

        void SendBuffer( byte[] buf )
        {
            m_eng.SendRawBuffer( buf );
        }
    }
}
