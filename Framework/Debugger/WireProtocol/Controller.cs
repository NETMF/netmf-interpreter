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
using System.Reflection;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

using DEBUG=System.Diagnostics.Debug;

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    public class Controller : IControllerLocal
    {

        internal byte[] marker_Debugger = Encoding.UTF8.GetBytes( Packet.MARKER_DEBUGGER_V1 );
        internal byte[] marker_Packet   = Encoding.UTF8.GetBytes( Packet.MARKER_PACKET_V1   );

        private string               m_marker;
        private IControllerHostLocal m_app;
        
        private Stream               m_port;
        private int                  m_lastOutboundMessage;
        private DateTime             m_lastActivity = DateTime.UtcNow;

        private int                  m_nextEndpointId;
                                   
        private FifoBuffer           m_inboundData;
                                   
        private Thread               m_inboundDataThread;        
                                   
        private Thread               m_stateMachineThread;

        private bool                 m_fProcessExit;        

        private ManualResetEvent     m_evtShutdown;
        
        private State                m_state;        
        private CLRCapabilities      m_capabilities;
        private WaitHandle[]         m_waitHandlesRead;

        public Controller( string marker, IControllerHostLocal app )
        {
            m_marker = marker;
            m_app    = app;

            Random random = new Random();

            m_lastOutboundMessage = random.Next( 65536 );
            m_nextEndpointId      = random.Next( int.MaxValue );

            m_state = new State( this );
            
            //default capabilities
            m_capabilities = new CLRCapabilities();
        }

        internal Converter CreateConverter()
        {
            return new Converter( m_capabilities );
        }

        private Thread CreateThread( ThreadStart ts )
        {
            Thread th = new Thread( ts );            
            th.IsBackground = true;
            th.Start();

            return th;
        }

        public DateTime LastActivity
        {
            get
            {
                return m_lastActivity;
            }
        }

        public bool IsPortConnected
        {
            [MethodImplAttribute( MethodImplOptions.Synchronized )]
            get
            {
                return (m_port != null);
            }
        }

        public Packet NewPacket()
        {
            if(!m_state.IsRunning)
                throw new ArgumentException( "Controller not started, cannot create message" );

            Packet bp = new Packet();

            SetSignature( bp, m_marker );

            bp.m_seq = (ushort)Interlocked.Increment( ref m_lastOutboundMessage );

            return bp;
        }

        public bool QueueOutput(MessageRaw raw)
        {
            SendRawBuffer(raw.m_header);
            if (raw.m_payload != null)
                SendRawBuffer(raw.m_payload);

            return true;
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        public void ClosePort()
        {
            if( m_port == null )
                return;

            try
            {
                m_port.Dispose( );
            }
            finally
            {
                m_port = null;
            }
        }

        public void Start()
        {
            m_state.SetValue( State.Value.Starting, true );

            m_inboundData  = new FifoBuffer();
            
            m_evtShutdown = new ManualResetEvent( false );

            m_waitHandlesRead = new WaitHandle[] { m_evtShutdown, m_inboundData.WaitHandle };

            m_inboundDataThread  = CreateThread( new ThreadStart( this.ReceiveInput   ) );
            m_stateMachineThread = CreateThread( new ThreadStart( this.Process        ) );

            m_state.SetValue( State.Value.Started, false );
        }

        public void StopProcessing()
        {
            m_state.SetValue( State.Value.Stopping, false );

            m_evtShutdown.Set();

            if (m_inboundDataThread != null)
            {
                m_inboundDataThread.Join();
                m_inboundDataThread = null;
            }
            if (m_stateMachineThread != null)
            {
                m_stateMachineThread.Join();
                m_stateMachineThread = null;
            }
        }

        public void ResumeProcessing()
        {
            m_evtShutdown.Reset();
            m_state.SetValue(State.Value.Resume, false);
            if (m_inboundDataThread == null)
            {
                m_inboundDataThread = CreateThread(new ThreadStart(this.ReceiveInput));
            }
            if (m_stateMachineThread == null)
            {
                m_stateMachineThread = CreateThread(new ThreadStart(this.Process));
            }
        }

        public void Stop()
        {
            if (m_evtShutdown != null)
            {
                m_evtShutdown.Set();
            }

            if (m_state.SetValue(State.Value.Stopping, false))
            {
                ((IController)this).StopProcessing();

                ((IController)this).ClosePort();

                m_state.SetValue( State.Value.Stopped, false );
            }
        }

        public uint GetUniqueEndpointId()
        {
            int id = Interlocked.Increment( ref m_nextEndpointId );

            return (uint)id;
        }

        public CLRCapabilities Capabilities
        {
            get { return m_capabilities ; }
            set { m_capabilities = value; }            
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public Stream OpenPort()
        {
            if(m_port == null)
            {
                m_port = App.OpenConnection();
            }

            return m_port;
        }

        internal IControllerHostLocal App
        {
            get
            {
                return m_app;
            }
        }

        internal int Read( byte[] buf, int offset, int count )
        {
            //wait on inbound data, or on exit....
            int countRequested = count;
            
            while (count > 0 && WaitHandle.WaitAny(m_waitHandlesRead) != 0)
            {                
                System.Diagnostics.Debug.Assert(m_inboundData.Available > 0);

                int cBytesRead = m_inboundData.Read( buf, offset, count );

                offset += cBytesRead;
                count  -= cBytesRead;
            }

            return countRequested - count;
        }

        internal void SetSignature( Packet bp, string sig )
        {
            byte[] buf = Encoding.UTF8.GetBytes( sig );

            Array.Copy( buf, 0, bp.m_signature, 0, buf.Length );
        }
      
        private void ProcessExit()
        {
            bool fExit = false;

            lock(this)
            {
                if(!m_fProcessExit)
                {
                    m_fProcessExit = true;

                    fExit = true;
                }
            }

            if(fExit)
            {
                App.ProcessExited();
            }
        }

        private void Process()
        {
            MessageReassembler msg = new MessageReassembler( this );

            while(m_state.IsRunning)
            {
                try
                {
                    msg.Process();
                }
                catch(ThreadAbortException)
                {
                    Stop();
                    break;
                }
                catch
                {
                    ClosePort();
                    throw;
                }
            }
        }

        private void ReceiveInput()
        {
            byte[] buf = new byte[128];

            int invalidOperationRetry = 5;

            while(m_state.IsRunning)
            {
                try
                {
                    Stream stream = OpenPort();

                    IStreamAvailableCharacters streamAvail = stream as IStreamAvailableCharacters;
                    int avail = 0;

                    if (streamAvail != null)
                    {
                        avail = streamAvail.AvailableCharacters;

                        if (avail == 0)
                        {
                            Thread.Yield();
                            continue;
                        }
                    }

                    if (avail == 0)
                        avail = 1;

                    if (avail > buf.Length)
                        buf = new byte[avail];

                    int read = stream.Read(buf, 0, avail);

                    if (read > 0)
                    {
                        m_lastActivity = DateTime.UtcNow;

                        m_inboundData.Write(buf, 0, read);
                    }
                    else if (read == 0)
                    {
                        Thread.Yield();
                    }
                }
                catch (ProcessExitException)
                {
                    ProcessExit();

                    ClosePort();

                    return;
                }
                catch (InvalidOperationException)
                {
                    if(invalidOperationRetry <= 0)
                    {
                        ProcessExit();

                        ClosePort();

                        return;
                    }

                    invalidOperationRetry--;

                    ClosePort();

                    Thread.Yield();
                }
                catch (IOException)
                {
                    ClosePort();
                    Thread.Yield( );
                }
                catch
                {
                    ClosePort();
                    throw;
                }
            }
        }

        public void SendRawBuffer( byte[] buf )
        {
            try
            {
                Stream stream = OpenPort();
                
                stream.Write(buf, 0, buf.Length);
                stream.Flush();
            }
            catch (ProcessExitException)
            {
                ProcessExit();
            }
            catch
            {
                ClosePort();
                throw;
            }
        }
    }
}
