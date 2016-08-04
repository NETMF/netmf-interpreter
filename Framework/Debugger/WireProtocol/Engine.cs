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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection.Emit;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Win32;
using Microsoft.SPOT.Debugger.WireProtocol;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

using WinUsb;

namespace Microsoft.SPOT.Debugger
{
    public delegate void NoiseEventHandler( byte[ ] buf, int offset, int count );
    public delegate void MessageEventHandler( IncomingMessage msg, string text );
    public delegate void CommandEventHandler( IncomingMessage msg, bool fReply );
    public delegate void ConsoleOutputEventHandler( string text );

    [Serializable]
    public class ThreadStatus
    {
        public const uint STATUS_Ready = Commands.Debugging_Thread_Stack.Reply.TH_S_Ready;
        public const uint STATUS_Waiting = Commands.Debugging_Thread_Stack.Reply.TH_S_Waiting;
        public const uint STATUS_Terminated = Commands.Debugging_Thread_Stack.Reply.TH_S_Terminated;

        public const uint FLAGS_Suspended = Commands.Debugging_Thread_Stack.Reply.TH_F_Suspended;

        public uint m_pid;
        public uint m_flags;
        public uint m_status;
        public string[ ] m_calls;
    }

    public enum PortFilter
    {
        Serial,
        Usb,
        Emulator,
        TcpIp,
        LegacyPermiscuousWinUsb
    }

    public enum PublicKeyIndex
    {
        FirmwareKey = 0,
        DeploymentKey = 1
    };


    public class ProcessExitException : Exception
    {
    }

    internal class State
    {
        public enum Value
        {
            NotStarted,
            Starting,
            Started,
            Stopping,
            Resume,
            Stopped,
            Disposing,
            Disposed
        }

        private Value m_value;
        private object m_syncObject;

        public State( object syncObject )
        {
            m_value = Value.NotStarted;
            m_syncObject = syncObject;
        }

        public Value GetValue( )
        {
            return m_value;
        }

        public bool SetValue( Value value )
        {
            return SetValue( value, false );
        }

        public bool SetValue( Value value, bool fThrow )
        {
            lock( m_syncObject )
            {
                if( m_value == Value.Stopping && value == Value.Resume )
                {
                    m_value = Value.Started;
                    return true;
                }
                else if( m_value < value )
                {
                    m_value = value;
                    return true;
                }
                else
                {
                    if( fThrow )
                    {
                        throw new ApplicationException( string.Format( "Cannot set State to {0}", value ) );
                    }

                    return false;
                }
            }
        }

        public bool IsRunning
        {
            get
            {
                Value val = m_value;

                return val == Value.Starting || val == Value.Started;
            }
        }

        public object SyncObject
        {
            get { return m_syncObject; }
        }
    }

    public enum ConnectionSource
    {
        Unknown,
        TinyBooter,
        TinyCLR,
        MicroBooter,
    };

    internal class RebootTime
    {
        public const int c_RECONNECT_RETRIES_DEFAULT = 5;
        public const int c_RECONNECT_HARD_TIMEOUT_DEFAULT_MS = 1000;         // one second
        public const int c_RECONNECT_SOFT_TIMEOUT_DEFAULT_MS = 1000;         // one second

        public const int c_MIN_RECONNECT_RETRIES = 1;
        public const int c_MAX_RECONNECT_RETRIES = 1000;
        public const int c_MIN_TIMEOUT_MS = 1 * 50;      // fifty milliseconds
        public const int c_MAX_TIMEOUT_MS = 60 * 1000;   // sixty seconds

        int m_retriesCount;
        int m_waitHardMs;
        int m_waitSoftMs;

        public RebootTime( )
        {
            m_waitSoftMs = c_RECONNECT_SOFT_TIMEOUT_DEFAULT_MS;
            m_waitHardMs = c_RECONNECT_HARD_TIMEOUT_DEFAULT_MS;

            bool fOverride = false;
            string timingKey = @"\NonVersionSpecific\Timing\AnyDevice";

            RegistryAccess.GetBoolValue( timingKey, "override", out fOverride, false );

            if( RegistryAccess.GetIntValue( timingKey, "retries", out m_retriesCount, c_RECONNECT_RETRIES_DEFAULT ) )
            {
                if( !fOverride )
                {
                    if( m_retriesCount < c_MIN_RECONNECT_RETRIES )
                        m_retriesCount = c_MIN_RECONNECT_RETRIES;

                    if( m_retriesCount > c_MAX_RECONNECT_RETRIES )
                        m_retriesCount = c_MAX_RECONNECT_RETRIES;
                }
            }

            if( RegistryAccess.GetIntValue( timingKey, "timeout", out m_waitHardMs, c_RECONNECT_HARD_TIMEOUT_DEFAULT_MS ) )
            {
                if( !fOverride )
                {
                    if( m_waitHardMs < c_MIN_TIMEOUT_MS )
                        m_waitHardMs = c_MIN_TIMEOUT_MS;

                    if( m_waitHardMs > c_MAX_TIMEOUT_MS )
                        m_waitHardMs = c_MAX_TIMEOUT_MS;
                }
                m_waitSoftMs = m_waitHardMs;
            }
        }

        public int Retries
        {
            get
            {
                return m_retriesCount;
            }
        }

        public int WaitMs( bool fSoftReboot )
        {
            return ( fSoftReboot ? m_waitSoftMs : m_waitHardMs );
        }

    }

    internal class Request
    {
        internal Engine m_parent;
        internal OutgoingMessage m_outMsg;
        internal IncomingMessage m_res;
        internal int m_retries;
        internal TimeSpan m_waitRetryTimeout;
        internal TimeSpan m_totalWaitTimeout;
        internal CommandEventHandler m_callback;
        internal ManualResetEvent m_event;
        internal Timer m_timer;

        internal Request( Engine parent, OutgoingMessage outMsg, int retries, int timeout, CommandEventHandler callback )
        {
            if( retries < 0 )
            {
                throw new ArgumentException( "Value cannot be negative", "retries" );
            }

            if( timeout < 1 || timeout > 60 * 60 * 1000 )
            {
                throw new ArgumentException( String.Format( "Value out of bounds: {0}", timeout ), "timeout" );
            }

            m_parent = parent;
            m_outMsg = outMsg;
            m_retries = retries;
            m_waitRetryTimeout = new TimeSpan( timeout * TimeSpan.TicksPerMillisecond );
            m_totalWaitTimeout = new TimeSpan( ( retries == 0 ? 1 : 2 * retries ) * timeout * TimeSpan.TicksPerMillisecond );
            m_callback = callback;

            if( callback == null )
                m_event = new ManualResetEvent( false );
        }

        internal void SendAsync( )
        {
            m_outMsg.Send( );
        }

        internal bool MatchesReply( IncomingMessage res )
        {
            Packet headerReq = m_outMsg.Header;
            Packet headerRes = res.Header;

            if( headerReq.m_cmd == headerRes.m_cmd &&
               headerReq.m_seq == headerRes.m_seqReply )
            {
                return true;
            }

            return false;
        }

        internal IncomingMessage Wait( )
        {
            if( m_event == null )
                return m_res;

            var waitStartTime = DateTime.UtcNow;
            var requestTimedOut = !m_event.WaitOne( m_waitRetryTimeout, false );

            // Wait for m_waitRetryTimeout milliseconds, if we did not get a signal by then
            // attempt sending the request again, and then wait more.
            while( requestTimedOut )
            {
                var deltaT = DateTime.UtcNow - waitStartTime;
                if( deltaT >= m_totalWaitTimeout )
                    break;

                if( m_retries <= 0 )
                    break;

                if( m_outMsg.Send( ) )
                    m_retries--;
                    
                requestTimedOut = !m_event.WaitOne( m_waitRetryTimeout, false );
            }

            if( requestTimedOut )
                m_parent.CancelRequest( this );

            if( m_res == null && m_parent.ThrowOnCommunicationFailure )
            {
                //do we want a separate exception for aborted requests?
                throw new IOException( "Request failed" );
            }

            return m_res;
        }

        internal void Signal( IncomingMessage res )
        {
            lock( this )
            {
                if( m_timer != null )
                {
                    m_timer.Dispose( );
                    m_timer = null;
                }

                m_res = res;
            }

            Signal( );
        }

        internal void Signal( )
        {
            CommandEventHandler callback;
            IncomingMessage res;

            lock( this )
            {
                callback = m_callback;
                res = m_res;

                if( m_timer != null )
                {
                    m_timer.Dispose( );
                    m_timer = null;
                }

                if( m_event != null )
                {
                    m_event.Set( );
                }
            }

            if( callback != null )
            {
                callback( res, true );
            }
        }

        internal void Retry( object state )
        {
            bool fCancel = false;
            TimeSpan ts = TimeSpan.MinValue;

            lock( this )
            {
                if( m_res != null || m_timer == null )
                    return;

                try
                {
                    while( true )
                    {
                        DateTime now = DateTime.UtcNow;

                        ts = now - m_parent.LastActivity;

                        if( ts < m_waitRetryTimeout )
                        {
                            // There was some activity going on, compensate for that.
                            ts = m_waitRetryTimeout - ts;
                            break;
                        }

                        if( m_retries > 0 )
                        {
                            if( m_outMsg.Send( ) )
                            {
                                m_retries--;
                                ts = m_waitRetryTimeout;
                            }
                            else
                            {
                                // Too many pending requests, retry in a bit.
                                ts = new TimeSpan( 10 * TimeSpan.TicksPerMillisecond );
                            }

                            break;
                        }

                        fCancel = true;
                        break;
                    }
                }
                catch
                {
                    fCancel = true;
                }

                if( !fCancel )
                {
                    m_timer.Change( ( int )ts.TotalMilliseconds, Timeout.Infinite );
                }
            }

            // Call can go out-of-proc, you need to release locks before the call to avoid deadlocks.
            if( fCancel )
            {
                m_parent.CancelRequest( this );
            }
        }
    }

    public class Engine : IControllerHostLocal, IDisposable
    {
        public enum RebootOption
        {
            EnterBootloader,
            RebootClrOnly,
            NormalReboot,
            NoReconnect,
            RebootClrWaitForDebugger,
        };

        private const int RETRIES_DEFAULT = 4;
        private const int TIMEOUT_DEFAULT = 5000;

        PortDefinition m_portDefinition;
        IController m_ctrl;
        bool m_silent;
        bool m_stopDebuggerOnConnect;
        bool m_connected;
        ConnectionSource m_connectionSource;
        bool m_targetIsBigEndian;
        DateTime m_lastNoise = DateTime.Now;

        event NoiseEventHandler m_eventNoise;
        event MessageEventHandler m_eventMessage;
        event CommandEventHandler m_eventCommand;
        event EventHandler m_eventProcessExit;

        /// <summary>
        /// Notification thread is essentially the Tx thread. Other threads pump outgoing data into it, which after potential
        /// processing is sent out to destination synchronously.
        /// </summary>
        Thread m_notificationThread;
        AutoResetEvent m_notifyEvent;
        ArrayList m_notifyQueue;
        FifoBuffer m_notifyNoise;

        AutoResetEvent m_rpcEvent;
        ArrayList m_rpcQueue;
        ArrayList m_rpcEndPoints;

        ManualResetEvent m_evtShutdown;
        ManualResetEvent m_evtPing;
        ArrayList m_requests;
        TypeSysLookup m_typeSysLookup;
        State m_state;
        bool m_fProcessExited;
        CLRCapabilities m_capabilities;
        bool m_fThrowOnCommunicationFailure;
        RebootTime m_RebootTime;

        private class TypeSysLookup
        {
            public enum Type : uint
            {
                Type,
                Method,
                Field
            }

            private Hashtable m_lookup;

            private void EnsureHashtable( )
            {
                lock( this )
                {
                    if( m_lookup == null )
                    {
                        m_lookup = Hashtable.Synchronized( new Hashtable( ) );
                    }
                }
            }

            private ulong KeyFromTypeToken( Type type, uint token )
            {
                return ( ( ulong )type ) << 32 | ( ulong )token;
            }

            public object Lookup( Type type, uint token )
            {
                EnsureHashtable( );

                ulong key = KeyFromTypeToken( type, token );

                return m_lookup[ key ];
            }

            public void Add( Type type, uint token, object val )
            {
                EnsureHashtable( );

                ulong key = KeyFromTypeToken( type, token );

                m_lookup[ key ] = val;
            }
        }

        public Engine( PortDefinition pd )
        {
            InitializeLocal( pd );
        }

        public Engine( )
        {
            Initialize( );
        }

        public bool ThrowOnCommunicationFailure
        {
            get { return m_fThrowOnCommunicationFailure; }
            set { m_fThrowOnCommunicationFailure = value; }
        }

        private void InitializeLocal( PortDefinition pd )
        {
            m_portDefinition = pd;
            m_ctrl = new Controller( Packet.MARKER_PACKET_V1, this );

            Initialize( );
        }

        private void Initialize( )
        {
            m_notifyEvent = new AutoResetEvent( false );
            m_rpcEvent = new AutoResetEvent( false );
            m_evtShutdown = new ManualResetEvent( false );
            m_evtPing = new ManualResetEvent( false );

            m_rpcQueue = ArrayList.Synchronized( new ArrayList( ) );
            m_rpcEndPoints = ArrayList.Synchronized( new ArrayList( ) );
            m_requests = ArrayList.Synchronized( new ArrayList( ) );
            m_notifyQueue = ArrayList.Synchronized( new ArrayList( ) );

            m_notifyNoise = new FifoBuffer( );
            m_typeSysLookup = new TypeSysLookup( );
            m_state = new State( this );
            m_fProcessExited = false;

            //default capabilities, used until clr can be queried.
            m_capabilities = new CLRCapabilities( );

            m_RebootTime = new RebootTime( );
        }

        private Thread CreateThread( ThreadStart ts )
        {
            Thread th = new Thread( ts );

            th.IsBackground = true;
            th.Priority = ThreadPriority.BelowNormal;

            th.Start( );

            return th;
        }

        public CLRCapabilities Capabilities
        {
            get { return m_capabilities; }
        }

        public BinaryFormatter CreateBinaryFormatter( )
        {
            return new BinaryFormatter( Capabilities );
        }

        public Converter CreateConverter( )
        {
            return new Converter( Capabilities );
        }

        public void SetController( IController ctrl )
        {
            if( m_ctrl != null )
            {
                throw new ArgumentException( "Controller already initialized" );
            }

            if( ctrl == null )
            {
                throw new ArgumentNullException( "ctrl" );
            }

            m_ctrl = ctrl;
        }

        public void Start( )
        {
            if( m_ctrl == null )
            {
                throw new ApplicationException( "Controller not initialized" );
            }

            m_state.SetValue( State.Value.Starting, true );

            try
            {
                m_notificationThread = CreateThread( new ThreadStart( NotificationThreadWorker ) );

                m_ctrl.Start( );
            }
            catch
            {
                Stop( );

                throw;
            }

            m_state.SetValue( State.Value.Started, false );
        }

        public void Stop( )
        {
            if( m_state.SetValue( State.Value.Stopping ) )
            {
                m_evtShutdown.Set( );

                CancelAllRequests( );

                m_notificationThread = null;

                if( m_ctrl != null )
                {
                    m_ctrl.Stop( );
                    m_ctrl = null;
                }

                m_state.SetValue( State.Value.Stopped );
            }
        }

        private bool IsRunning
        {
            get
            {
                return !m_fProcessExited && m_state.IsRunning;
            }
        }

        private void CancelAllRequests( )
        {
            ArrayList requests;
            ArrayList endPoints;

            requests = ( ArrayList )m_requests.Clone( );

            foreach( Request req in requests )
            {
                CancelRequest( req );
            }

            endPoints = ( ArrayList )m_rpcEndPoints.Clone( );

            foreach( EndPointRegistration eep in endPoints )
            {
                try
                {
                    RpcDeregisterEndPoint( eep.m_ep );
                }
                catch
                {
                }
            }
        }

        public DateTime LastActivity
        {
            get
            {
                return m_ctrl.LastActivity;
            }
        }

        public DateTime LastNoise
        {
            get
            {
                return m_lastNoise;
            }
        }

        public PortDefinition PortDefinition
        {
            get
            {
                return m_portDefinition;
            }
        }

        public bool Silent
        {
            get
            {
                return m_silent;
            }

            set
            {
                m_silent = value;
            }
        }

        public bool StopDebuggerOnConnect
        {
            get
            {
                return m_stopDebuggerOnConnect;
            }
            set
            {
                m_stopDebuggerOnConnect = value;
            }
        }

        public event NoiseEventHandler OnNoise
        {
            add
            {
                m_eventNoise += value;
            }

            remove
            {
                m_eventNoise -= value;
            }
        }

        public event MessageEventHandler OnMessage
        {
            add
            {
                m_eventMessage += value;
            }

            remove
            {
                m_eventMessage -= value;
            }
        }

        public event CommandEventHandler OnCommand
        {
            add
            {
                m_eventCommand += value;
            }

            remove
            {
                m_eventCommand -= value;
            }
        }

        public event EventHandler OnProcessExit
        {
            add
            {
                m_eventProcessExit += value;
            }

            remove
            {
                m_eventProcessExit -= value;
            }
        }

        public void WaitForPort( )
        {
            if( m_ctrl.IsPortConnected == false )
            {
                InjectMessage( "Port is not connected, waiting...\r\n" );

                while( m_ctrl.IsPortConnected == false )
                    Thread.Yield( );

                InjectMessage( "Port connected, continuing...\r\n" );
            }
        }

        public void ConfigureXonXoff( bool fEnable )
        {
            IControllerLocal local = m_ctrl as IControllerLocal;
            if( local != null && m_portDefinition is PortDefinition_Serial )
            {
                try
                {
                    AsyncSerialStream port = local.OpenPort( ) as AsyncSerialStream;

                    if( port != null )
                    {
                        port.ConfigureXonXoff( fEnable );
                    }
                }
                catch
                {
                }
            }
        }

        public void SpuriousCharacters( byte[ ] buf, int offset, int count )
        {
            m_lastNoise = DateTime.Now;

            m_notifyNoise.Write( buf, offset, count );
        }


        public void ProcessExited( )
        {
            m_fProcessExited = true;

            Stop( );

            EventHandler eventProcessExit = m_eventProcessExit;
            if( eventProcessExit != null )
            {
                eventProcessExit( this, null );
            }
        }

        public bool ProcessMessage( IncomingMessage msg, bool fReply )
        {
            msg.Payload = Commands.ResolveCommandToPayload( msg.Header.m_cmd, fReply, m_capabilities );

            if( fReply == true )
            {
                Request reply = null;

                lock( m_requests.SyncRoot )
                {
                    foreach( Request req in m_requests )
                    {
                        if( req.MatchesReply( msg ) )
                        {
                            m_requests.Remove( req );

                            reply = req;
                            break;
                        }
                    }
                }

                if( reply != null )
                {
                    reply.Signal( msg );
                    return true;
                }
            }
            else
            {
                Packet bp = msg.Header;

                switch( bp.m_cmd )
                {
                case Commands.c_Monitor_Ping:
                    {
                        Commands.Monitor_Ping.Reply cmdReply = new Commands.Monitor_Ping.Reply( );

                        cmdReply.m_source = Commands.Monitor_Ping.c_Ping_Source_Host;
                        cmdReply.m_dbg_flags = ( m_stopDebuggerOnConnect ? Commands.Monitor_Ping.c_Ping_DbgFlag_Stop : 0 );

                        msg.Reply( CreateConverter( ), Flags.c_NonCritical, cmdReply );

                        m_evtPing.Set( );

                        return true;
                    }

                case Commands.c_Monitor_Message:
                    {
                        Commands.Monitor_Message payload = msg.Payload as Commands.Monitor_Message;

                        Debug.Assert( payload != null );

                        if( payload != null )
                        {
                            QueueNotify( m_eventMessage, msg, payload.ToString( ) );
                        }

                        return true;
                    }

                case Commands.c_Debugging_Messaging_Query:
                case Commands.c_Debugging_Messaging_Reply:
                case Commands.c_Debugging_Messaging_Send:
                    {
                        Debug.Assert( msg.Payload != null );

                        if( msg.Payload != null )
                        {
                            QueueRpc( msg );
                        }

                        return true;
                    }
                }
            }

            if( m_eventCommand != null )
            {
                QueueNotify( m_eventCommand, msg, fReply );
                return true;
            }

            return false;
        }

        public Stream OpenConnection( )
        {
            return m_portDefinition.Open( );
        }

        /// <summary>
        /// Notification thread is essentially the Tx thread. Other threads pump outgoing data into it, which after potential
        /// processing is sent out to destination synchronously.
        /// </summary>
        internal void NotificationThreadWorker( )
        {
            byte[ ] buf = new byte[ 256 ];
            WaitHandle[ ] wh = new WaitHandle[ ] { m_evtShutdown, m_notifyEvent, m_notifyNoise.WaitHandle, m_rpcEvent };

            while( WaitHandle.WaitAny( wh ) > 0 )
            {
                int read = 0;
                while( ( read = m_notifyNoise.Available ) > 0 )
                {
                    if( read > buf.Length )
                        read = buf.Length;

                    m_notifyNoise.Read( buf, 0, read );

                    NoiseEventHandler ev = m_eventNoise;
                    if( ev != null )
                        ev( buf, 0, read );
                }

                while( m_notifyQueue.Count > 0 )
                {
                    object[ ] arr = ( object[ ] )m_notifyQueue[ 0 ];
                    m_notifyQueue.RemoveAt( 0 );

                    CommandEventHandler cev = arr[ 0 ] as CommandEventHandler;
                    if( cev != null )
                        cev( ( IncomingMessage )arr[ 1 ], ( bool )arr[ 2 ] );

                    MessageEventHandler mev = arr[ 0 ] as MessageEventHandler;
                    if( mev != null )
                        mev( ( IncomingMessage )arr[ 1 ], ( string )arr[ 2 ] );
                }

                while( m_rpcQueue.Count > 0 )
                {
                    IncomingMessage msg = ( IncomingMessage )m_rpcQueue[ 0 ];
                    m_rpcQueue.RemoveAt( 0 );

                    object payload = msg.Payload;

                    switch( msg.Header.m_cmd )
                    {
                    case Commands.c_Debugging_Messaging_Query:
                        RpcReceiveQuery( msg, ( Commands.Debugging_Messaging_Query )payload );
                        break;
                    case Commands.c_Debugging_Messaging_Send:
                        RpcReceiveSend( msg, ( Commands.Debugging_Messaging_Send )payload );
                        break;
                    case Commands.c_Debugging_Messaging_Reply:
                        RpcReceiveReply( msg, ( Commands.Debugging_Messaging_Reply )payload );
                        break;
                    default:
                        IncomingMessage.ReplyBadPacket( msg.Parent, 0 );
                        break;
                    }
                }
            }
        }

        internal void QueueNotify( params object[ ] arr )
        {
            m_notifyQueue.Add( arr );
            m_notifyEvent.Set( );
        }

        internal void InjectMessage( string format, params object[ ] args )
        {
            QueueNotify( m_eventMessage, null, String.Format( format, args ) );
        }

        #region RPC Support
        // REVIEW: Can this be refactored out of here to a seperate class dedicated to RPC?
        internal class EndPointRegistration
        {
            internal class Request
            {
                public readonly EndPointRegistration Owner;

                public Request( EndPointRegistration owner )
                {
                    Owner = owner;
                }
            }

            internal class OutboundRequest : Request
            {
                private byte[ ] m_data;
                private readonly AutoResetEvent m_wait;
                public readonly uint Seq;
                public readonly uint Type;
                public readonly uint Id;

                public OutboundRequest( EndPointRegistration owner, uint seq, uint type, uint id )
                    : base( owner )
                {
                    Seq = seq;
                    Type = type;
                    Id = id;
                    m_wait = new AutoResetEvent( false );
                }

                public byte[ ] Reply
                {
                    get { return m_data; }

                    set
                    {
                        m_data = value;
                        m_wait.Set( );
                    }
                }
                public WaitHandle WaitHandle
                {
                    get { return m_wait; }
                }
            }

            internal class InboundRequest : Request
            {
                public readonly Message m_msg;

                public InboundRequest( EndPointRegistration owner, Message msg )
                    : base( owner )
                {
                    m_msg = msg;
                }
            }

            internal EndPoint m_ep;
            internal ArrayList m_req_Outbound;

            internal EndPointRegistration( EndPoint ep )
            {
                m_ep = ep;
                m_req_Outbound = ArrayList.Synchronized( new ArrayList( ) );
            }

            internal void Destroy( )
            {
                lock( m_req_Outbound.SyncRoot )
                {
                    foreach( OutboundRequest or in m_req_Outbound )
                    {
                        or.Reply = null;
                    }
                }

                m_req_Outbound.Clear( );
            }
        }

        internal void QueueRpc( IncomingMessage msg )
        {
            m_rpcQueue.Add( msg );
            m_rpcEvent.Set( );
        }

        internal void RpcRegisterEndPoint( EndPoint ep )
        {
            EndPointRegistration eep = RpcFind( ep );
            bool fSuccess = false;

            if( eep == null )
            {
                IControllerRemote remote = m_ctrl as IControllerRemote;

                if( remote != null )
                {
                    fSuccess = remote.RegisterEndpoint( ep.m_type, ep.m_id );
                }
                else
                {
                    fSuccess = true;
                }

                if( fSuccess )
                {
                    lock( m_rpcEndPoints.SyncRoot )
                    {
                        eep = RpcFind( ep );

                        if( eep == null )
                        {
                            m_rpcEndPoints.Add( new EndPointRegistration( ep ) );
                        }
                        else
                        {
                            fSuccess = false;
                        }
                    }
                }
            }

            if( !fSuccess )
            {
                throw new ApplicationException( "Endpoint already registered." );
            }
        }

        internal void RpcDeregisterEndPoint( EndPoint ep )
        {
            EndPointRegistration eep = RpcFind( ep );

            if( eep != null )
            {
                m_rpcEndPoints.Remove( eep );

                eep.Destroy( );

                IControllerRemote remote = m_ctrl as IControllerRemote;
                if( remote != null )
                {
                    remote.DeregisterEndpoint( ep.m_type, ep.m_id );
                }
            }
        }

        private EndPointRegistration RpcFind( EndPoint ep )
        {
            return RpcFind( ep.m_type, ep.m_id, false );
        }

        private EndPointRegistration RpcFind( uint type, uint id, bool fOnlyServer )
        {
            lock( m_rpcEndPoints.SyncRoot )
            {
                foreach( EndPointRegistration eep in m_rpcEndPoints )
                {
                    EndPoint ep = eep.m_ep;

                    if( ep.m_type == type && ep.m_id == id )
                    {
                        if( !fOnlyServer || ep.IsRpcServer )
                        {
                            return eep;
                        }
                    }
                }
            }
            return null;
        }

        private void RpcReceiveQuery( IncomingMessage msg, Commands.Debugging_Messaging_Query query )
        {
            Commands.Debugging_Messaging_Address addr = query.m_addr;
            EndPointRegistration eep = RpcFind( addr.m_to_Type, addr.m_to_Id, true );

            Commands.Debugging_Messaging_Query.Reply res = new Commands.Debugging_Messaging_Query.Reply( );

            res.m_found = ( eep != null ) ? 1u : 0u;
            res.m_addr = addr;

            msg.Reply( CreateConverter( ), Flags.c_NonCritical, res );
        }

        internal bool RpcCheck( Commands.Debugging_Messaging_Address addr )
        {
            Commands.Debugging_Messaging_Query cmd = new Commands.Debugging_Messaging_Query( );

            cmd.m_addr = addr;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Messaging_Query, 0, cmd );
            if( reply != null )
            {
                Commands.Debugging_Messaging_Query.Reply res = reply.Payload as Commands.Debugging_Messaging_Query.Reply;

                if( res != null && res.m_found != 0 )
                {
                    return true;
                }
            }

            return false;
        }

        internal byte[ ] RpcSend( Commands.Debugging_Messaging_Address addr, int timeout, byte[ ] data )
        {
            EndPointRegistration.OutboundRequest or = null;
            byte[ ] res = null;

            try
            {
                or = RpcSend_Setup( addr, data );
                if( or != null )
                {
                    or.WaitHandle.WaitOne( timeout, false );

                    res = or.Reply;
                }
            }
            finally
            {
                if( or != null )
                {
                    or.Owner.m_req_Outbound.Remove( or );
                }
            }

            return res;
        }

        private EndPointRegistration.OutboundRequest RpcSend_Setup( Commands.Debugging_Messaging_Address addr, byte[ ] data )
        {
            EndPointRegistration eep = RpcFind( addr.m_from_Type, addr.m_from_Id, false );
            EndPointRegistration.OutboundRequest or = null;

            if( eep != null )
            {
                bool fSuccess = false;

                or = new EndPointRegistration.OutboundRequest( eep, addr.m_seq, addr.m_to_Type, addr.m_to_Id );

                eep.m_req_Outbound.Add( or );

                Commands.Debugging_Messaging_Send cmd = new Commands.Debugging_Messaging_Send( );

                cmd.m_addr = addr;
                cmd.m_data = data;

                IncomingMessage reply = SyncMessage( Commands.c_Debugging_Messaging_Send, 0, cmd );
                if( reply != null )
                {
                    Commands.Debugging_Messaging_Send.Reply res = reply.Payload as Commands.Debugging_Messaging_Send.Reply;

                    if( res != null && res.m_found != 0 )
                    {
                        fSuccess = true;
                    }
                }

                if( !IsRunning )
                {
                    fSuccess = false;
                }

                if( !fSuccess )
                {
                    eep.m_req_Outbound.Remove( or );

                    or = null;
                }
            }

            return or;
        }

        private void RpcReceiveSend( IncomingMessage msg, Commands.Debugging_Messaging_Send send )
        {
            Commands.Debugging_Messaging_Address addr = send.m_addr;
            EndPointRegistration eep;

            eep = RpcFind( addr.m_to_Type, addr.m_to_Id, true );

            Commands.Debugging_Messaging_Send.Reply res = new Commands.Debugging_Messaging_Send.Reply( );

            res.m_found = ( eep != null ) ? 1u : 0u;
            res.m_addr = addr;

            msg.Reply( CreateConverter( ), Flags.c_NonCritical, res );

            if( eep != null )
            {
                Message msgNew = new Message( eep.m_ep, addr, send.m_data );

                EndPointRegistration.InboundRequest ir = new EndPointRegistration.InboundRequest( eep, msgNew );

                ThreadPool.QueueUserWorkItem( new WaitCallback( RpcReceiveSendDispatch ), ir );
            }
        }

        private void RpcReceiveSendDispatch( object obj )
        {
            EndPointRegistration.InboundRequest ir = ( EndPointRegistration.InboundRequest )obj;

            if( IsRunning )
            {
                ir.Owner.m_ep.DispatchMessage( ir.m_msg );
            }
        }

        internal bool RpcReply( Commands.Debugging_Messaging_Address addr, byte[ ] data )
        {
            Commands.Debugging_Messaging_Reply cmd = new Commands.Debugging_Messaging_Reply( );

            cmd.m_addr = addr;
            cmd.m_data = data;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Messaging_Reply, 0, cmd );
            if( reply != null )
            {
                Commands.Debugging_Messaging_Reply.Reply res = new Commands.Debugging_Messaging_Reply.Reply( );

                if( res != null && res.m_found != 0 )
                {
                    return true;
                }
            }

            return false;
        }

        private void RpcReceiveReply( IncomingMessage msg, Commands.Debugging_Messaging_Reply reply )
        {
            Commands.Debugging_Messaging_Address addr = reply.m_addr;
            EndPointRegistration eep;

            eep = RpcFind( addr.m_from_Type, addr.m_from_Id, false );

            Commands.Debugging_Messaging_Reply.Reply res = new Commands.Debugging_Messaging_Reply.Reply( );

            res.m_found = ( eep != null ) ? 1u : 0u;
            res.m_addr = addr;

            msg.Reply( CreateConverter( ), Flags.c_NonCritical, res );

            if( eep != null )
            {
                lock( eep.m_req_Outbound.SyncRoot )
                {
                    foreach( EndPointRegistration.OutboundRequest or in eep.m_req_Outbound )
                    {
                        if( or.Seq == addr.m_seq && or.Type == addr.m_to_Type && or.Id == addr.m_to_Id )
                        {
                            or.Reply = reply.m_data;

                            break;
                        }
                    }
                }
            }
        }

        internal uint RpcGetUniqueEndpointId( )
        {
            return m_ctrl.GetUniqueEndpointId( );
        }
        #endregion

        internal Request AsyncRequest( OutgoingMessage msg, int retries, int timeout )
        {
            Request req = new Request( this, msg, retries, timeout, null );

            lock( m_state.SyncObject )
            {

                //Checking whether IsRunning and adding the request to m_requests
                //needs to be atomic to avoid adding a request after the Engine
                //has been stopped.

                if( !IsRunning )
                {
                    throw new ApplicationException( "Engine is not running or process has exited." );
                }

                m_requests.Add( req );

                req.SendAsync( );
            }

            return req;
        }

        /// <summary>
        /// Global lock object for synchornizing message request. This ensures there is only one
        /// outstanding request at any point of time. 
        /// </summary>
        internal object m_ReqSyncLock = new object( );

        internal IncomingMessage SyncRequest( OutgoingMessage msg, int retries, int timeout )
        {
            // Lock on m_ReqSyncLock object, so only one thread is active inside the block.
            lock( m_ReqSyncLock )
            {
                Request req = AsyncRequest( msg, retries, timeout );

                return req != null ? req.Wait( ) : null;
            }
        }

        internal void CancelRequest( Request req )
        {
            m_requests.Remove( req );

            req.Signal( );
        }

        private OutgoingMessage CreateMessage( uint cmd, uint flags, object payload )
        {
            return new OutgoingMessage( m_ctrl, CreateConverter( ), cmd, flags, payload );
        }

        private Request AsyncMessage( uint cmd, uint flags, object payload, int retries, int timeout )
        {
            OutgoingMessage msg = CreateMessage( cmd, flags, payload );

            return AsyncRequest( msg, retries, timeout );
        }

        private IncomingMessage SyncMessage( uint cmd, uint flags, object payload, int retries, int timeout )
        {
            // Lock on m_ReqSyncLock object, so only one thread is active inside the block.
            lock( m_ReqSyncLock )
            {
                Request req = AsyncMessage( cmd, flags, payload, retries, timeout );

                return req.Wait( );
            }
        }

        private IncomingMessage SyncMessage( uint cmd, uint flags, object payload )
        {
            return SyncMessage( cmd, flags, payload, RETRIES_DEFAULT, TIMEOUT_DEFAULT );
        }

        private IncomingMessage[ ] SyncMessages( OutgoingMessage[ ] messages, int retries, int timeout )
        {
            int cMessage = messages.Length;
            IncomingMessage[ ] replies = new IncomingMessage[ cMessage ];
            Request[ ] requests = new Request[ cMessage ];

            for( int iMessage = 0; iMessage < cMessage; iMessage++ )
            {
                replies[ iMessage ] = SyncRequest( messages[ iMessage ], retries, timeout );
            }

            return replies;
        }

        private IncomingMessage[ ] SyncMessages( OutgoingMessage[ ] messages )
        {
            return SyncMessages( messages, 2, 1000 );
        }

        public bool IsConnected
        {
            get
            {
                return m_connected;
            }
        }

        public ConnectionSource ConnectionSource
        {
            get
            {
                if( !m_connected )
                {
                    TryToConnect( 3, 500, true, ConnectionSource.Unknown );
                }

                return m_connected ? m_connectionSource : ConnectionSource.Unknown;
            }
        }

        public bool IsConnectedToTinyCLR
        {
            get { return ConnectionSource == ConnectionSource.TinyCLR; }
        }

        public bool IsTargetBigEndian
        {
            get { return m_targetIsBigEndian; }
        }

        public bool TryToConnect( int retries, int wait )
        {
            return TryToConnect( retries, wait, false, ConnectionSource.Unknown );
        }

        public bool TryToConnect( int retries, int wait, bool force, ConnectionSource connectionSource )
        {
            if( force || m_connected == false )
            {
                Commands.Monitor_Ping cmd = new Commands.Monitor_Ping( );

                cmd.m_source = Commands.Monitor_Ping.c_Ping_Source_Host;
                cmd.m_dbg_flags = ( m_stopDebuggerOnConnect ? Commands.Monitor_Ping.c_Ping_DbgFlag_Stop : 0 );

                IncomingMessage msg = SyncMessage( Commands.c_Monitor_Ping, Flags.c_NoCaching, cmd, retries, wait );

                if( msg == null )
                {
                    m_connected = false;
                    return false;
                }

                Commands.Monitor_Ping.Reply reply = msg.Payload as Commands.Monitor_Ping.Reply;

                if( reply != null )
                {
                    m_targetIsBigEndian = ( reply.m_dbg_flags & Commands.Monitor_Ping.c_Ping_DbgFlag_BigEndian ).Equals( Commands.Monitor_Ping.c_Ping_DbgFlag_BigEndian );
                }
                m_connected = true;

                m_connectionSource = ( reply == null || reply.m_source == Commands.Monitor_Ping.c_Ping_Source_TinyCLR ) ? ConnectionSource.TinyCLR : ConnectionSource.TinyBooter;

                if( m_silent )
                {
                    SetExecutionMode( Commands.Debugging_Execution_ChangeConditions.c_fDebugger_Quiet, 0 );
                }

                // resume execution for older clients, since server tools no longer do this.
                if( !m_stopDebuggerOnConnect && ( msg != null && msg.Payload == null ) )
                {
                    ResumeExecution( );
                }
            }

            if( ( force || m_capabilities.IsUnknown ) && m_connectionSource == ConnectionSource.TinyCLR )
            {
                m_capabilities = DiscoverCLRCapabilities( );
                m_ctrl.Capabilities = m_capabilities;
            }

            if( connectionSource != ConnectionSource.Unknown && connectionSource != m_connectionSource )
            {
                m_connected = false;
                return false;
            }

            return true;
        }

        public Commands.Monitor_Ping.Reply GetConnectionSource( )
        {
            IncomingMessage reply = SyncMessage( Commands.c_Monitor_Ping, 0, null, 2, 1000 );
            if( reply != null )
            {
                return reply.Payload as Commands.Monitor_Ping.Reply;
            }
            return null;
        }

        public Commands.Monitor_OemInfo.Reply GetMonitorOemInfo( )
        {
            IncomingMessage reply = SyncMessage( Commands.c_Monitor_OemInfo, 0, null, 2, 1000 );
            if( reply != null )
            {
                return reply.Payload as Commands.Monitor_OemInfo.Reply;
            }
            return null;
        }

        public Commands.Monitor_FlashSectorMap.Reply GetFlashSectorMap( )
        {
            IncomingMessage reply = SyncMessage( Commands.c_Monitor_FlashSectorMap, 0, null, 1, 4000 );
            if( reply != null )
            {
                return reply.Payload as Commands.Monitor_FlashSectorMap.Reply;
            }
            return null;
        }

        public bool UpdateSignatureKey( PublicKeyIndex keyIndex, byte[ ] oldPublicKeySignature, byte[ ] newPublicKey, byte[ ] reserveData )
        {
            Commands.Monitor_SignatureKeyUpdate keyUpdate = new Commands.Monitor_SignatureKeyUpdate( );

            // key must be 260 bytes
            if( keyUpdate.m_newPublicKey.Length != newPublicKey.Length )
                return false;

            if( !keyUpdate.PrepareForSend( ( uint )keyIndex, oldPublicKeySignature, newPublicKey, reserveData ) )
                return false;

            IncomingMessage reply = SyncMessage( Commands.c_Monitor_SignatureKeyUpdate, 0, keyUpdate );

            return IncomingMessage.IsPositiveAcknowledge( reply );
        }


        public void SendRawBuffer( byte[ ] buf )
        {
            m_ctrl.SendRawBuffer( buf );
        }

        private bool ReadMemory( uint address, uint length, byte[ ] buf, uint offset )
        {
            while( length > 0 )
            {
                Commands.Monitor_ReadMemory cmd = new Commands.Monitor_ReadMemory( );

                cmd.m_address = address;
                cmd.m_length = length;

                IncomingMessage reply = SyncMessage( Commands.c_Monitor_ReadMemory, 0, cmd );
                if( reply == null )
                    return false;

                Commands.Monitor_ReadMemory.Reply cmdReply = reply.Payload as Commands.Monitor_ReadMemory.Reply;
                if( cmdReply == null || cmdReply.m_data == null )
                    return false;

                uint actualLength = Math.Min( ( uint )cmdReply.m_data.Length, length );

                Array.Copy( cmdReply.m_data, 0, buf, ( int )offset, ( int )actualLength );

                address += actualLength;
                length -= actualLength;
                offset += actualLength;
            }
            return true;
        }

        public bool ReadMemory( uint address, uint length, out byte[ ] buf )
        {
            buf = new byte[ length ];

            return ReadMemory( address, length, buf, 0 );
        }

        public bool WriteMemory( uint address, byte[ ] buf, int offset, int length )
        {
            int count = length;
            int pos = offset;

            while( count > 0 )
            {
                Commands.Monitor_WriteMemory cmd = new Commands.Monitor_WriteMemory( );
                int len = Math.Min( 1024, count );

                cmd.PrepareForSend( address, buf, pos, len );

                DebuggerEventSource.Log.EngineWriteMemory( address, len );
                IncomingMessage reply = SyncMessage( Commands.c_Monitor_WriteMemory, 0, cmd );

                if( !IncomingMessage.IsPositiveAcknowledge( reply ) )
                    return false;

                address += ( uint )len;
                count -= len;
                pos += len;
            }

            return true;
        }

        public bool WriteMemory( uint address, byte[ ] buf )
        {
            return WriteMemory( address, buf, 0, buf.Length );
        }

        public bool CheckSignature( byte[ ] signature, uint keyIndex )
        {
            Commands.Monitor_Signature cmd = new Commands.Monitor_Signature( );

            cmd.PrepareForSend( signature, keyIndex );

            IncomingMessage reply = SyncMessage( Commands.c_Monitor_CheckSignature, 0, cmd, 0, 600000 );

            return IncomingMessage.IsPositiveAcknowledge( reply );
        }

        public bool EraseMemory( uint address, uint length )
        {
            DebuggerEventSource.Log.EngineEraseMemory( address, length );
            var cmd = new Commands.Monitor_EraseMemory
                    {
                        m_address = address,
                        m_length = length
                    };

            // Magic number multiplier here is somewhat arbitrary. Assuming a max 250ms per 1KB block erase time.
            // (Given most chip erase times are measured in uSecs that's pretty generous 8^) )
            // The idea is to extend the timeout based on the actual length of the area being erased
            var timeout = ( int )( length / 1024 ) * 250;
            IncomingMessage reply = SyncMessage( Commands.c_Monitor_EraseMemory, 0, cmd, 2, timeout );

            return IncomingMessage.IsPositiveAcknowledge( reply );
        }

        public bool ExecuteMemory( uint address )
        {
            Commands.Monitor_Execute cmd = new Commands.Monitor_Execute( );

            cmd.m_address = address;

            IncomingMessage reply = SyncMessage( Commands.c_Monitor_Execute, 0, cmd );

            return IncomingMessage.IsPositiveAcknowledge( reply );
        }

        public void RebootDevice( )
        {
            RebootDevice( RebootOption.NormalReboot );
        }

        public void RebootDevice( RebootOption option )
        {
            Commands.Monitor_Reboot cmd = new Commands.Monitor_Reboot( );

            bool fThrowOnCommunicationFailureSav = m_fThrowOnCommunicationFailure;

            m_fThrowOnCommunicationFailure = false;

            switch( option )
            {
            case RebootOption.EnterBootloader:
                cmd.m_flags = Commands.Monitor_Reboot.c_EnterBootloader;
                break;
            case RebootOption.RebootClrOnly:
                cmd.m_flags = Capabilities.SoftReboot ? Commands.Monitor_Reboot.c_ClrRebootOnly : Commands.Monitor_Reboot.c_NormalReboot;
                break;
            case RebootOption.RebootClrWaitForDebugger:
                cmd.m_flags = Capabilities.SoftReboot ? Commands.Monitor_Reboot.c_ClrWaitForDbg : Commands.Monitor_Reboot.c_NormalReboot;
                break;
            default:
                cmd.m_flags = Commands.Monitor_Reboot.c_NormalReboot;
                break;
            }

            try
            {
                m_evtPing.Reset( );

                SyncMessage( Commands.c_Monitor_Reboot, 0, cmd );

                if( option != RebootOption.NoReconnect )
                {
                    int timeout = 1000;

                    if( m_portDefinition is PortDefinition_Tcp )
                    {
                        timeout = 2000;
                    }

                    Thread.Sleep( timeout );
                }
            }
            finally
            {
                m_fThrowOnCommunicationFailure = fThrowOnCommunicationFailureSav;
            }

        }


        public bool TryToReconnect( bool fSoftReboot )
        {
            if( !TryToConnect( m_RebootTime.Retries, m_RebootTime.WaitMs( fSoftReboot ), true, ConnectionSource.Unknown ) )
            {
                if( m_fThrowOnCommunicationFailure )
                {
                    throw new ApplicationException( "Could not reconnect to TinyCLR" );
                }
                return false;
            }

            return true;
        }


        public Commands.Monitor_MemoryMap.Range[ ] MemoryMap( )
        {
            Commands.Monitor_MemoryMap cmd = new Commands.Monitor_MemoryMap( );

            IncomingMessage reply = SyncMessage( Commands.c_Monitor_MemoryMap, 0, cmd );

            if( reply != null )
            {
                Commands.Monitor_MemoryMap.Reply cmdReply = reply.Payload as Commands.Monitor_MemoryMap.Reply;

                if( cmdReply != null )
                {
                    return cmdReply.m_map;
                }
            }

            return null;
        }

        public Commands.Monitor_DeploymentMap.Reply DeploymentMap( )
        {
            Commands.Monitor_DeploymentMap cmd = new Commands.Monitor_DeploymentMap( );

            IncomingMessage reply = SyncMessage( Commands.c_Monitor_DeploymentMap, 0, cmd, 2, 10000 );

            if( reply != null )
            {
                Commands.Monitor_DeploymentMap.Reply cmdReply = reply.Payload as Commands.Monitor_DeploymentMap.Reply;

                return cmdReply;
            }

            return null;
        }
        public bool GetExecutionBasePtr( out uint ee )
        {
            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Execution_BasePtr, 0, null );
            if( reply != null )
            {
                Commands.Debugging_Execution_BasePtr.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_BasePtr.Reply;

                if( cmdReply != null )
                {
                    ee = cmdReply.m_EE;
                    return true;
                }
            }

            ee = 0;
            return false;
        }

        public bool SetExecutionMode( uint iSet, uint iReset, out uint iCurrent )
        {
            Commands.Debugging_Execution_ChangeConditions cmd = new Commands.Debugging_Execution_ChangeConditions( );

            cmd.m_set = iSet;
            cmd.m_reset = iReset;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Execution_ChangeConditions, Flags.c_NoCaching, cmd );
            if( reply != null )
            {
                Commands.Debugging_Execution_ChangeConditions.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_ChangeConditions.Reply;

                if( cmdReply != null )
                {
                    iCurrent = cmdReply.m_current;
                }
                else
                {
                    iCurrent = 0;
                }

                return true;
            }

            iCurrent = 0;
            return false;
        }

        public bool SetExecutionMode( uint iSet, uint iReset )
        {
            uint iCurrent;

            return SetExecutionMode( iSet, iReset, out iCurrent );
        }

        public bool PauseExecution( )
        {
            return SetExecutionMode( Commands.Debugging_Execution_ChangeConditions.c_Stopped, 0 );
        }

        public bool ResumeExecution( )
        {
            return SetExecutionMode( 0, Commands.Debugging_Execution_ChangeConditions.c_Stopped );
        }

        public bool SetCurrentAppDomain( uint id )
        {
            Commands.Debugging_Execution_SetCurrentAppDomain cmd = new Commands.Debugging_Execution_SetCurrentAppDomain( );

            cmd.m_id = id;

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Execution_SetCurrentAppDomain, 0, cmd ) );
        }

        public bool SetBreakpoints( Commands.Debugging_Execution_BreakpointDef[ ] breakpoints )
        {
            Commands.Debugging_Execution_Breakpoints cmd = new Commands.Debugging_Execution_Breakpoints( );

            cmd.m_data = breakpoints;

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Execution_Breakpoints, 0, cmd ) );
        }

        public Commands.Debugging_Execution_BreakpointDef GetBreakpointStatus( )
        {
            Commands.Debugging_Execution_BreakpointStatus cmd = new Commands.Debugging_Execution_BreakpointStatus( );

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Execution_BreakpointStatus, 0, cmd );
            if( reply != null )
            {
                Commands.Debugging_Execution_BreakpointStatus.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_BreakpointStatus.Reply;

                if( cmdReply != null )
                    return cmdReply.m_lastHit;
            }

            return null;
        }

        public bool SetSecurityKey( byte[ ] key )
        {
            Commands.Debugging_Execution_SecurityKey cmd = new Commands.Debugging_Execution_SecurityKey( );

            cmd.m_key = key;

            return SyncMessage( Commands.c_Debugging_Execution_SecurityKey, 0, cmd ) != null;
        }

        public bool UnlockDevice( byte[ ] blob )
        {
            Commands.Debugging_Execution_Unlock cmd = new Commands.Debugging_Execution_Unlock( );

            Array.Copy( blob, 0, cmd.m_command, 0, 128 );
            Array.Copy( blob, 128, cmd.m_hash, 0, 128 );

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Execution_Unlock, 0, cmd ) );
        }

        public bool AllocateMemory( uint size, out uint address )
        {
            Commands.Debugging_Execution_Allocate cmd = new Commands.Debugging_Execution_Allocate( );

            cmd.m_size = size;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Execution_Allocate, 0, cmd );
            if( reply != null )
            {
                Commands.Debugging_Execution_Allocate.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_Allocate.Reply;

                if( cmdReply != null )
                {
                    address = cmdReply.m_address;
                    return true;
                }
            }

            address = 0;
            return false;
        }

        public IAsyncResult UpgradeConnectionToSsl_Begin( X509Certificate2 cert, bool fRequireClientCert )
        {
            AsyncNetworkStream ans = ( ( IControllerLocal )m_ctrl ).OpenPort( ) as AsyncNetworkStream;

            if( ans == null )
                return null;

            m_ctrl.StopProcessing( );

            IAsyncResult iar = ans.BeginUpgradeToSSL( cert, fRequireClientCert );

            return iar;
        }

        public bool UpgradeConnectionToSSL_End( IAsyncResult iar )
        {
            AsyncNetworkStream ans = ( ( IControllerLocal )m_ctrl ).OpenPort( ) as AsyncNetworkStream;

            if( ans == null )
                return false;

            bool result = ans.EndUpgradeToSSL( iar );

            m_ctrl.ResumeProcessing( );

            return result;
        }

        public bool IsUsingSsl
        {
            get
            {
                if( !IsConnected )
                    return false;

                AsyncNetworkStream ans = ( ( IControllerLocal )m_ctrl ).OpenPort( ) as AsyncNetworkStream;

                if( ans == null )
                    return false;

                return ans.IsUsingSsl;
            }
        }

        public bool CanUpgradeToSsl( )
        {
            Commands.Debugging_UpgradeToSsl cmd = new Commands.Debugging_UpgradeToSsl( );

            cmd.m_flags = 0;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_UpgradeToSsl, Flags.c_NoCaching, cmd, 2, 5000 );

            if( reply != null )
            {
                Commands.Debugging_UpgradeToSsl.Reply cmdReply = reply.Payload as Commands.Debugging_UpgradeToSsl.Reply;

                if( cmdReply != null )
                {
                    return cmdReply.m_success != 0;
                }
            }

            return false;

        }

        Dictionary<int, uint[ ]> m_updateMissingPktTbl = new Dictionary<int, uint[ ]>( );

        public bool StartUpdate(
            string provider,
            ushort versionMajor,
            ushort versionMinor,
            uint updateId,
            uint updateType,
            uint updateSubType,
            uint updateSize,
            uint packetSize,
            uint installAddress,
            ref int updateHandle )
        {
            Commands.Debugging_MFUpdate_Start cmd = new Commands.Debugging_MFUpdate_Start( );

            byte[ ] name = UTF8Encoding.UTF8.GetBytes( provider );

            Array.Copy( name, cmd.m_updateProvider, Math.Min( name.Length, cmd.m_updateProvider.Length ) );
            cmd.m_updateId = updateId;
            cmd.m_updateVerMajor = versionMajor;
            cmd.m_updateVerMinor = versionMinor;
            cmd.m_updateType = updateType;
            cmd.m_updateSubType = updateSubType;
            cmd.m_updateSize = updateSize;
            cmd.m_updatePacketSize = packetSize;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_MFUpdate_Start, Flags.c_NoCaching, cmd, 2, 5000 );

            if( reply != null )
            {
                Commands.Debugging_MFUpdate_Start.Reply cmdReply = reply.Payload as Commands.Debugging_MFUpdate_Start.Reply;

                if( cmdReply != null )
                {
                    updateHandle = cmdReply.m_updateHandle;
                    return ( -1 != updateHandle );
                }
            }

            updateHandle = -1;
            return false;
        }

        public bool UpdateAuthCommand( int updateHandle, uint authCommand, byte[ ] commandArgs, ref byte[ ] response )
        {
            Commands.Debugging_MFUpdate_AuthCommand cmd = new Commands.Debugging_MFUpdate_AuthCommand( );

            if( commandArgs == null )
                commandArgs = new byte[ 0 ];

            cmd.m_updateHandle = updateHandle;
            cmd.m_authCommand = authCommand;
            cmd.m_authArgs = commandArgs;
            cmd.m_authArgsSize = ( uint )commandArgs.Length;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_MFUpdate_AuthCmd, Flags.c_NoCaching, cmd );

            if( reply != null )
            {
                Commands.Debugging_MFUpdate_AuthCommand.Reply cmdReply = reply.Payload as Commands.Debugging_MFUpdate_AuthCommand.Reply;

                if( cmdReply != null && cmdReply.m_success != 0 )
                {
                    if( cmdReply.m_responseSize > 0 )
                    {
                        Array.Copy( cmdReply.m_response, response, Math.Min( response.Length, cmdReply.m_responseSize ) );
                    }
                    return true;
                }
            }

            return false;
        }

        public bool UpdateAuthenticate( int updateHandle, byte[ ] authenticationData )
        {
            Commands.Debugging_MFUpdate_Authenticate cmd = new Commands.Debugging_MFUpdate_Authenticate( );

            cmd.m_updateHandle = updateHandle;
            cmd.PrepareForSend( authenticationData );

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_MFUpdate_Authenticate, Flags.c_NoCaching, cmd );

            if( reply != null )
            {
                Commands.Debugging_MFUpdate_Authenticate.Reply cmdReply = reply.Payload as Commands.Debugging_MFUpdate_Authenticate.Reply;

                if( cmdReply != null && cmdReply.m_success != 0 )
                {
                    return true;
                }
            }

            return false;
        }

        private bool UpdateGetMissingPackets( int updateHandle )
        {
            Commands.Debugging_MFUpdate_GetMissingPkts cmd = new Commands.Debugging_MFUpdate_GetMissingPkts( );

            cmd.m_updateHandle = updateHandle;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_MFUpdate_GetMissingPkts, Flags.c_NoCaching, cmd );

            if( reply != null )
            {
                Commands.Debugging_MFUpdate_GetMissingPkts.Reply cmdReply = reply.Payload as Commands.Debugging_MFUpdate_GetMissingPkts.Reply;

                if( cmdReply != null && cmdReply.m_success != 0 )
                {
                    if( cmdReply.m_missingPktCount > 0 )
                    {
                        m_updateMissingPktTbl[ updateHandle ] = cmdReply.m_missingPkts;
                    }
                    else
                    {
                        m_updateMissingPktTbl[ updateHandle ] = new uint[ 0 ];
                    }
                    return true;
                }
            }

            return false;
        }

        public bool AddPacket( int updateHandle, uint packetIndex, byte[ ] packetData, uint packetValidation )
        {
            if( !m_updateMissingPktTbl.ContainsKey( updateHandle ) )
            {
                UpdateGetMissingPackets( updateHandle );
            }

            if( m_updateMissingPktTbl.ContainsKey( updateHandle ) && m_updateMissingPktTbl[ updateHandle ].Length > 0 )
            {
                uint[ ] pktBits = m_updateMissingPktTbl[ updateHandle ];
                uint div = packetIndex >> 5;

                if( pktBits.Length > div )
                {
                    if( 0 == ( pktBits[ div ] & ( 1u << ( int )( packetIndex % 32 ) ) ) )
                    {
                        return true;
                    }
                }
            }

            Commands.Debugging_MFUpdate_AddPacket cmd = new Commands.Debugging_MFUpdate_AddPacket( );

            cmd.m_updateHandle = updateHandle;
            cmd.m_packetIndex = packetIndex;
            cmd.m_packetValidation = packetValidation;
            cmd.PrepareForSend( packetData );

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_MFUpdate_AddPacket, Flags.c_NoCaching, cmd );
            if( reply != null )
            {
                Commands.Debugging_MFUpdate_AddPacket.Reply cmdReply = reply.Payload as Commands.Debugging_MFUpdate_AddPacket.Reply;

                if( cmdReply != null )
                {
                    return cmdReply.m_success != 0;
                }
            }

            return false;
        }

        public bool InstallUpdate( int updateHandle, byte[ ] validationData )
        {
            if( m_updateMissingPktTbl.ContainsKey( updateHandle ) )
            {
                m_updateMissingPktTbl.Remove( updateHandle );
            }

            Commands.Debugging_MFUpdate_Install cmd = new Commands.Debugging_MFUpdate_Install( );

            cmd.m_updateHandle = updateHandle;

            cmd.PrepareForSend( validationData );

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_MFUpdate_Install, Flags.c_NoCaching, cmd );
            if( reply != null )
            {
                Commands.Debugging_MFUpdate_Install.Reply cmdReply = reply.Payload as Commands.Debugging_MFUpdate_Install.Reply;

                if( cmdReply != null )
                {
                    return cmdReply.m_success != 0;
                }
            }

            return false;
        }


        public uint CreateThread( uint methodIndex, int scratchPadLocation )
        {
            return CreateThread( methodIndex, scratchPadLocation, 0 );
        }


        public uint CreateThread( uint methodIndex, int scratchPadLocation, uint pid )
        {
            if( Capabilities.ThreadCreateEx )
            {
                Commands.Debugging_Thread_CreateEx cmd = new Commands.Debugging_Thread_CreateEx( );

                cmd.m_md = methodIndex;
                cmd.m_scratchPad = scratchPadLocation;
                cmd.m_pid = pid;

                IncomingMessage reply = SyncMessage( Commands.c_Debugging_Thread_CreateEx, 0, cmd );
                if( reply != null )
                {
                    Commands.Debugging_Thread_CreateEx.Reply cmdReply = reply.Payload as Commands.Debugging_Thread_CreateEx.Reply;

                    return cmdReply.m_pid;
                }
            }

            return 0;
        }

        public uint[ ] GetThreadList( )
        {
            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Thread_List, 0, null );
            if( reply != null )
            {
                Commands.Debugging_Thread_List.Reply cmdReply = reply.Payload as Commands.Debugging_Thread_List.Reply;

                if( cmdReply != null )
                {
                    return cmdReply.m_pids;
                }
            }

            return null;
        }

        public Commands.Debugging_Thread_Stack.Reply GetThreadStack( uint pid )
        {
            Commands.Debugging_Thread_Stack cmd = new Commands.Debugging_Thread_Stack( );

            cmd.m_pid = pid;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Thread_Stack, 0, cmd );
            if( reply != null )
            {
                return reply.Payload as Commands.Debugging_Thread_Stack.Reply;
            }

            return null;
        }

        public bool KillThread( uint pid )
        {
            Commands.Debugging_Thread_Kill cmd = new Commands.Debugging_Thread_Kill( );

            cmd.m_pid = pid;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Thread_Kill, 0, cmd );
            if( reply != null )
            {
                Commands.Debugging_Thread_Kill.Reply cmdReply = reply.Payload as Commands.Debugging_Thread_Kill.Reply;

                return cmdReply.m_result != 0;
            }

            return false;
        }

        public bool SuspendThread( uint pid )
        {
            Commands.Debugging_Thread_Suspend cmd = new Commands.Debugging_Thread_Suspend( );

            cmd.m_pid = pid;

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Thread_Suspend, 0, cmd ) );
        }

        public bool ResumeThread( uint pid )
        {
            Commands.Debugging_Thread_Resume cmd = new Commands.Debugging_Thread_Resume( );

            cmd.m_pid = pid;

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Thread_Resume, 0, cmd ) );
        }

        public RuntimeValue GetThreadException( uint pid )
        {
            Commands.Debugging_Thread_GetException cmd = new Commands.Debugging_Thread_GetException( );

            cmd.m_pid = pid;

            return GetRuntimeValue( Commands.c_Debugging_Thread_GetException, cmd );
        }

        public RuntimeValue GetThread( uint pid )
        {
            Commands.Debugging_Thread_Get cmd = new Commands.Debugging_Thread_Get( );

            cmd.m_pid = pid;

            return GetRuntimeValue( Commands.c_Debugging_Thread_Get, cmd );
        }

        public bool UnwindThread( uint pid, uint depth )
        {
            Commands.Debugging_Thread_Unwind cmd = new Commands.Debugging_Thread_Unwind( );

            cmd.m_pid = pid;
            cmd.m_depth = depth;

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Thread_Unwind, 0, cmd ) );
        }

        public bool SetIPOfStackFrame( uint pid, uint depth, uint IP, uint depthOfEvalStack )
        {
            Commands.Debugging_Stack_SetIP cmd = new Commands.Debugging_Stack_SetIP( );

            cmd.m_pid = pid;
            cmd.m_depth = depth;

            cmd.m_IP = IP;
            cmd.m_depthOfEvalStack = depthOfEvalStack;

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Stack_SetIP, 0, cmd ) );
        }

        public Commands.Debugging_Stack_Info.Reply GetStackInfo( uint pid, uint depth )
        {
            Commands.Debugging_Stack_Info cmd = new Commands.Debugging_Stack_Info( );

            cmd.m_pid = pid;
            cmd.m_depth = depth;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Stack_Info, 0, cmd );
            if( reply != null )
            {
                return reply.Payload as Commands.Debugging_Stack_Info.Reply;
            }

            return null;
        }

        //--//

        public Commands.Debugging_TypeSys_AppDomains.Reply GetAppDomains( )
        {
            if( !Capabilities.AppDomains )
                return null;

            Commands.Debugging_TypeSys_AppDomains cmd = new Commands.Debugging_TypeSys_AppDomains( );

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_TypeSys_AppDomains, 0, cmd );
            if( reply != null )
            {
                return reply.Payload as Commands.Debugging_TypeSys_AppDomains.Reply;
            }

            return null;
        }

        public Commands.Debugging_TypeSys_Assemblies.Reply GetAssemblies( )
        {
            Commands.Debugging_TypeSys_Assemblies cmd = new Commands.Debugging_TypeSys_Assemblies( );

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_TypeSys_Assemblies, 0, cmd );
            if( reply != null )
            {
                return reply.Payload as Commands.Debugging_TypeSys_Assemblies.Reply;
            }

            return null;
        }

        public Commands.Debugging_Resolve_Assembly[ ] ResolveAllAssemblies( )
        {
            Commands.Debugging_TypeSys_Assemblies.Reply assemblies = GetAssemblies( );
            Commands.Debugging_Resolve_Assembly[ ] resolveAssemblies = null;

            if( assemblies == null || assemblies.m_data == null )
            {
                resolveAssemblies = new Commands.Debugging_Resolve_Assembly[ 0 ];
            }
            else
            {
                int cAssembly = assemblies.m_data.Length;
                OutgoingMessage[ ] requests = new OutgoingMessage[ cAssembly ];
                int iAssembly;

                for( iAssembly = 0; iAssembly < cAssembly; iAssembly++ )
                {
                    Commands.Debugging_Resolve_Assembly cmd = new Commands.Debugging_Resolve_Assembly( );

                    cmd.m_idx = assemblies.m_data[ iAssembly ];

                    requests[ iAssembly ] = CreateMessage( Commands.c_Debugging_Resolve_Assembly, 0, cmd );
                }

                IncomingMessage[ ] replies = SyncMessages( requests );

                resolveAssemblies = new Commands.Debugging_Resolve_Assembly[ cAssembly ];

                for( iAssembly = 0; iAssembly < cAssembly; iAssembly++ )
                {
                    resolveAssemblies[ iAssembly ] = requests[ iAssembly ].Payload as Commands.Debugging_Resolve_Assembly;
                    resolveAssemblies[ iAssembly ].m_reply = replies[ iAssembly ].Payload as Commands.Debugging_Resolve_Assembly.Reply;
                }
            }

            return resolveAssemblies;
        }

        public Commands.Debugging_Resolve_Assembly.Reply ResolveAssembly( uint idx )
        {
            Commands.Debugging_Resolve_Assembly cmd = new Commands.Debugging_Resolve_Assembly( );

            cmd.m_idx = idx;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Resolve_Assembly, 0, cmd );
            if( reply != null )
            {
                return reply.Payload as Commands.Debugging_Resolve_Assembly.Reply;
            }

            return null;
        }

        public enum StackValueKind
        {
            Local = 0,
            Argument = 1,
            EvalStack = 2,
        }

        public bool GetStackFrameInfo( uint pid, uint depth, out uint numOfArguments, out uint numOfLocals, out uint depthOfEvalStack )
        {
            numOfArguments = 0;
            numOfLocals = 0;
            depthOfEvalStack = 0;

            Commands.Debugging_Stack_Info.Reply reply = GetStackInfo( pid, depth );

            if( reply == null )
                return false;

            numOfArguments = reply.m_numOfArguments;
            numOfLocals = reply.m_numOfLocals;
            depthOfEvalStack = reply.m_depthOfEvalStack;

            return true;
        }

        private RuntimeValue GetRuntimeValue( uint msg, object cmd )
        {
            IncomingMessage reply = SyncMessage( msg, 0, cmd );
            if( reply != null && reply.Payload != null )
            {
                Commands.Debugging_Value_Reply cmdReply = reply.Payload as Commands.Debugging_Value_Reply;

                return RuntimeValue.Convert( this, cmdReply.m_values );
            }

            return null;
        }

        internal RuntimeValue GetFieldValue( RuntimeValue val, uint offset, uint fd )
        {
            Commands.Debugging_Value_GetField cmd = new Commands.Debugging_Value_GetField( );

            cmd.m_heapblock = ( val == null ? 0 : val.m_handle.m_referenceID );
            cmd.m_offset = offset;
            cmd.m_fd = fd;

            return GetRuntimeValue( Commands.c_Debugging_Value_GetField, cmd );
        }

        public RuntimeValue GetStaticFieldValue( uint fd )
        {
            return GetFieldValue( null, 0, fd );
        }

        internal RuntimeValue AssignRuntimeValue( uint heapblockSrc, uint heapblockDst )
        {
            Commands.Debugging_Value_Assign cmd = new Commands.Debugging_Value_Assign( );

            cmd.m_heapblockSrc = heapblockSrc;
            cmd.m_heapblockDst = heapblockDst;

            return GetRuntimeValue( Commands.c_Debugging_Value_Assign, cmd );
        }

        internal bool SetBlock( uint heapblock, uint dt, byte[ ] data )
        {
            Commands.Debugging_Value_SetBlock setBlock = new Commands.Debugging_Value_SetBlock( );

            setBlock.m_heapblock = heapblock;
            setBlock.m_dt = dt;

            data.CopyTo( setBlock.m_value, 0 );

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Value_SetBlock, 0, setBlock ) );
        }

        private OutgoingMessage CreateMessage_GetValue_Stack( uint pid, uint depth, StackValueKind kind, uint index )
        {
            Commands.Debugging_Value_GetStack cmd = new Commands.Debugging_Value_GetStack( );

            cmd.m_pid = pid;
            cmd.m_depth = depth;
            cmd.m_kind = ( uint )kind;
            cmd.m_index = index;

            return CreateMessage( Commands.c_Debugging_Value_GetStack, 0, cmd );
        }

        public bool ResizeScratchPad( int size )
        {
            Commands.Debugging_Value_ResizeScratchPad cmd = new Commands.Debugging_Value_ResizeScratchPad( );

            cmd.m_size = size;

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Value_ResizeScratchPad, 0, cmd ) );
        }

        public RuntimeValue GetStackFrameValue( uint pid, uint depth, StackValueKind kind, uint index )
        {
            OutgoingMessage cmd = CreateMessage_GetValue_Stack( pid, depth, kind, index );

            IncomingMessage reply = SyncRequest( cmd, 10, 200 );
            if( reply != null )
            {
                Commands.Debugging_Value_Reply cmdReply = reply.Payload as Commands.Debugging_Value_Reply;

                return RuntimeValue.Convert( this, cmdReply.m_values );
            }

            return null;
        }

        public RuntimeValue[ ] GetStackFrameValueAll( uint pid, uint depth, uint cValues, StackValueKind kind )
        {
            OutgoingMessage[ ] cmds = new OutgoingMessage[ cValues ];
            RuntimeValue[ ] vals = null;
            uint i;

            for( i = 0; i < cValues; i++ )
            {
                cmds[ i ] = CreateMessage_GetValue_Stack( pid, depth, kind, i );
            }

            IncomingMessage[ ] replies = SyncMessages( cmds );
            if( replies != null )
            {
                vals = new RuntimeValue[ cValues ];

                for( i = 0; i < cValues; i++ )
                {
                    Commands.Debugging_Value_Reply reply = replies[ i ].Payload as Commands.Debugging_Value_Reply;
                    if( reply != null )
                    {
                        vals[ i ] = RuntimeValue.Convert( this, reply.m_values );
                    }
                }
            }

            return vals;
        }

        public RuntimeValue GetArrayElement( uint arrayReferenceId, uint index )
        {
            Commands.Debugging_Value_GetArray cmd = new Commands.Debugging_Value_GetArray( );

            cmd.m_heapblock = arrayReferenceId;
            cmd.m_index = index;

            RuntimeValue rtv = GetRuntimeValue( Commands.c_Debugging_Value_GetArray, cmd );

            if( rtv != null )
            {
                rtv.m_handle.m_arrayref_referenceID = arrayReferenceId;
                rtv.m_handle.m_arrayref_index = index;
            }

            return rtv;
        }

        internal bool SetArrayElement( uint heapblock, uint index, byte[ ] data )
        {
            Commands.Debugging_Value_SetArray cmd = new Commands.Debugging_Value_SetArray( );

            cmd.m_heapblock = heapblock;
            cmd.m_index = index;

            data.CopyTo( cmd.m_value, 0 );

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Value_SetArray, 0, cmd ) );
        }

        public RuntimeValue GetScratchPadValue( int index )
        {
            Commands.Debugging_Value_GetScratchPad cmd = new Commands.Debugging_Value_GetScratchPad( );

            cmd.m_index = index;

            return GetRuntimeValue( Commands.c_Debugging_Value_GetScratchPad, cmd );
        }

        public RuntimeValue AllocateObject( int scratchPadLocation, uint td )
        {
            Commands.Debugging_Value_AllocateObject cmd = new Commands.Debugging_Value_AllocateObject( );

            cmd.m_index = scratchPadLocation;
            cmd.m_td = td;

            return GetRuntimeValue( Commands.c_Debugging_Value_AllocateObject, cmd );
        }

        public RuntimeValue AllocateString( int scratchPadLocation, string val )
        {
            Commands.Debugging_Value_AllocateString cmd = new Commands.Debugging_Value_AllocateString( );

            cmd.m_index = scratchPadLocation;
            cmd.m_size = ( uint )Encoding.UTF8.GetByteCount( val );

            RuntimeValue rtv = GetRuntimeValue( Commands.c_Debugging_Value_AllocateString, cmd );

            if( rtv != null )
            {
                rtv.SetStringValue( val );
            }

            return rtv;
        }

        public RuntimeValue AllocateArray( int scratchPadLocation, uint td, int depth, int numOfElements )
        {
            Commands.Debugging_Value_AllocateArray cmd = new Commands.Debugging_Value_AllocateArray( );

            cmd.m_index = scratchPadLocation;
            cmd.m_td = td;
            cmd.m_depth = ( uint )depth;
            cmd.m_numOfElements = ( uint )numOfElements;

            return GetRuntimeValue( Commands.c_Debugging_Value_AllocateArray, cmd );
        }

        public Commands.Debugging_Resolve_Type.Result ResolveType( uint td )
        {
            Commands.Debugging_Resolve_Type.Result result = ( Commands.Debugging_Resolve_Type.Result )m_typeSysLookup.Lookup( TypeSysLookup.Type.Type, td );

            if( result == null )
            {
                Commands.Debugging_Resolve_Type cmd = new Commands.Debugging_Resolve_Type( );

                cmd.m_td = td;

                IncomingMessage reply = SyncMessage( Commands.c_Debugging_Resolve_Type, 0, cmd );
                if( reply != null )
                {
                    Commands.Debugging_Resolve_Type.Reply cmdReply = reply.Payload as Commands.Debugging_Resolve_Type.Reply;

                    if( cmdReply != null )
                    {
                        result = new Commands.Debugging_Resolve_Type.Result( );

                        result.m_name = Commands.GetZeroTerminatedString( cmdReply.m_type, false );

                        m_typeSysLookup.Add( TypeSysLookup.Type.Type, td, result );
                    }
                }
            }

            return result;
        }

        public Commands.Debugging_Resolve_Method.Result ResolveMethod( uint md )
        {
            Commands.Debugging_Resolve_Method.Result result = ( Commands.Debugging_Resolve_Method.Result )m_typeSysLookup.Lookup( TypeSysLookup.Type.Method, md );
            ;

            if( result == null )
            {
                Commands.Debugging_Resolve_Method cmd = new Commands.Debugging_Resolve_Method( );

                cmd.m_md = md;

                IncomingMessage reply = SyncMessage( Commands.c_Debugging_Resolve_Method, 0, cmd );
                if( reply != null )
                {
                    Commands.Debugging_Resolve_Method.Reply cmdReply = reply.Payload as Commands.Debugging_Resolve_Method.Reply;

                    if( cmdReply != null )
                    {
                        result = new Commands.Debugging_Resolve_Method.Result( );

                        result.m_name = Commands.GetZeroTerminatedString( cmdReply.m_method, false );
                        result.m_td = cmdReply.m_td;

                        m_typeSysLookup.Add( TypeSysLookup.Type.Method, md, result );
                    }
                }
            }

            return result;
        }

        public Commands.Debugging_Resolve_Field.Result ResolveField( uint fd )
        {
            Commands.Debugging_Resolve_Field.Result result = ( Commands.Debugging_Resolve_Field.Result )m_typeSysLookup.Lookup( TypeSysLookup.Type.Field, fd );
            ;

            if( result == null )
            {
                Commands.Debugging_Resolve_Field cmd = new Commands.Debugging_Resolve_Field( );

                cmd.m_fd = fd;

                IncomingMessage reply = SyncMessage( Commands.c_Debugging_Resolve_Field, 0, cmd );
                if( reply != null )
                {
                    Commands.Debugging_Resolve_Field.Reply cmdReply = reply.Payload as Commands.Debugging_Resolve_Field.Reply;

                    if( cmdReply != null )
                    {
                        result = new Commands.Debugging_Resolve_Field.Result( );

                        result.m_name = Commands.GetZeroTerminatedString( cmdReply.m_name, false );
                        result.m_offset = cmdReply.m_offset;
                        result.m_td = cmdReply.m_td;

                        m_typeSysLookup.Add( TypeSysLookup.Type.Field, fd, result );
                    }
                }
            }

            return result;
        }

        public Commands.Debugging_Resolve_AppDomain.Reply ResolveAppDomain( uint appDomainID )
        {
            if( !Capabilities.AppDomains )
                return null;

            Commands.Debugging_Resolve_AppDomain cmd = new Commands.Debugging_Resolve_AppDomain( );

            cmd.m_id = appDomainID;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Resolve_AppDomain, 0, cmd );
            if( reply != null )
            {
                return reply.Payload as Commands.Debugging_Resolve_AppDomain.Reply;
            }

            return null;
        }

        public string GetTypeName( uint td )
        {
            Commands.Debugging_Resolve_Type.Result resolvedType = ResolveType( td );

            return ( resolvedType != null ) ? resolvedType.m_name : null;
        }

        public string GetMethodName( uint md, bool fIncludeType )
        {
            Commands.Debugging_Resolve_Method.Result resolvedMethod = ResolveMethod( md );
            string name = null;

            if( resolvedMethod != null )
            {
                if( fIncludeType )
                {
                    name = string.Format( "{0}::{1}", GetTypeName( resolvedMethod.m_td ), resolvedMethod.m_name );
                }
                else
                {
                    name = resolvedMethod.m_name;
                }
            }

            return name;
        }

        public string GetFieldName( uint fd, out uint td, out uint offset )
        {
            Commands.Debugging_Resolve_Field.Result resolvedField = ResolveField( fd );

            if( resolvedField != null )
            {
                td = resolvedField.m_td;
                offset = resolvedField.m_offset;

                return resolvedField.m_name;
            }

            td = 0;
            offset = 0;

            return null;
        }

        public uint GetVirtualMethod( uint md, RuntimeValue obj )
        {
            Commands.Debugging_Resolve_VirtualMethod cmd = new Commands.Debugging_Resolve_VirtualMethod( );

            cmd.m_md = md;
            cmd.m_obj = obj.ReferenceId;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Resolve_VirtualMethod, 0, cmd );
            if( reply != null )
            {
                Commands.Debugging_Resolve_VirtualMethod.Reply cmdReply = reply.Payload as Commands.Debugging_Resolve_VirtualMethod.Reply;

                if( cmdReply != null )
                {
                    return cmdReply.m_md;
                }
            }

            return 0;
        }

        public bool GetFrameBuffer( out ushort widthInWords, out ushort heightInPixels, out uint[ ] buf )
        {
            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Lcd_GetFrame, 0, null );
            if( reply != null )
            {
                Commands.Debugging_Lcd_GetFrame.Reply cmdReply = reply.Payload as Commands.Debugging_Lcd_GetFrame.Reply;

                if( cmdReply != null )
                {
                    widthInWords = cmdReply.m_header.m_widthInWords;
                    heightInPixels = cmdReply.m_header.m_heightInPixels;
                    buf = cmdReply.m_data;
                    return true;
                }
            }

            widthInWords = 0;
            heightInPixels = 0;
            buf = null;
            return false;
        }

        private void Adjust1bppOrientation( uint[ ] buf )
        {
            //CLR_GFX_Bitmap::AdjustBitOrientation
            //The TinyCLR treats 1bpp bitmaps reversed from Windows
            //And most likely every other 1bpp format as well
            byte[ ] reverseTable = new byte[ ]
            {
                0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0,
                0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
                0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8,
                0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
                0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4,
                0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
                0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC,
                0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
                0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2,
                0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
                0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA,
                0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
                0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6,
                0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
                0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE,
                0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
                0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1,
                0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
                0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9,
                0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
                0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5,
                0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
                0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED,
                0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
                0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3,
                0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
                0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB,
                0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
                0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7,
                0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
                0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF,
                0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F,0xFF,
                };

            unsafe
            {
                fixed( uint* pbuf = buf )
                {
                    byte* ptr = ( byte* )pbuf;

                    for( int i = buf.Length * 4; i > 0; i-- )
                    {
                        *ptr = reverseTable[ *ptr ];
                        ptr++;
                    }
                }
            }
        }

        public Bitmap GetFrameBuffer( )
        {
            ushort widthInWords;
            ushort heightInPixels;
            uint[ ] buf;

            Bitmap bmp = null;


            PixelFormat pixelFormat = PixelFormat.DontCare;

            if( GetFrameBuffer( out widthInWords, out heightInPixels, out buf ) )
            {
                CLRCapabilities.LCDCapabilities lcdCaps = Capabilities.LCD;

                int pixelsPerWord = 32 / ( int )lcdCaps.BitsPerPixel;

                Debug.Assert( heightInPixels == lcdCaps.Height );
                Debug.Assert( widthInWords == ( lcdCaps.Width + pixelsPerWord - 1 ) / pixelsPerWord );

                Color[ ] colors = null;

                switch( lcdCaps.BitsPerPixel )
                {
                case 1:
                    pixelFormat = PixelFormat.Format1bppIndexed;
                    colors = new Color[ ] { Color.White, Color.Black };
                    Adjust1bppOrientation( buf );
                    break;
                case 4:
                case 8:
                    //Not tested
                    int cColors = 1 << ( int )lcdCaps.BitsPerPixel;

                    pixelFormat = ( lcdCaps.BitsPerPixel == 4 ) ? PixelFormat.Format4bppIndexed : PixelFormat.Format8bppIndexed;

                    colors = new Color[ cColors ];

                    for( int i = 0; i < cColors; i++ )
                    {
                        int intensity = 256 / cColors * i;
                        colors[ i ] = Color.FromArgb( intensity, intensity, intensity );
                    }

                    break;
                case 16:
                    pixelFormat = PixelFormat.Format16bppRgb565;
                    break;
                default:
                    Debug.Assert( false );
                    return null;
                }

                BitmapData bitmapData = null;

                try
                {
                    bmp = new Bitmap( ( int )lcdCaps.Width, ( int )lcdCaps.Height, pixelFormat );
                    Rectangle rect = new Rectangle( 0, 0, ( int )lcdCaps.Width, ( int )lcdCaps.Height );

                    if( colors != null )
                    {
                        ColorPalette palette = bmp.Palette;
                        colors.CopyTo( palette.Entries, 0 );
                        bmp.Palette = palette;
                    }

                    bitmapData = bmp.LockBits( rect, ImageLockMode.WriteOnly, pixelFormat );
                    IntPtr data = bitmapData.Scan0;

                    unsafe
                    {
                        fixed( uint* pbuf = buf )
                        {
                            uint* src = ( uint* )pbuf;
                            uint* dst = ( uint* )data.ToPointer( );

                            for( int i = buf.Length; i > 0; i-- )
                            {
                                *dst = *src;
                                dst++;
                                src++;
                            }

                        }
                    }
                }

                finally
                {
                    if( bitmapData != null )
                    {
                        bmp.UnlockBits( bitmapData );
                    }
                }
            }

            return bmp;
        }

        public void InjectButtons( uint pressed, uint released )
        {
            Commands.Debugging_Button_Inject cmd = new Commands.Debugging_Button_Inject( );

            cmd.m_pressed = pressed;
            cmd.m_released = released;

            SyncMessage( Commands.c_Debugging_Button_Inject, 0, cmd );
        }

        public ArrayList GetThreads( )
        {
            ArrayList threads = new ArrayList( );
            uint[ ] pids = GetThreadList( );

            if( pids != null )
            {
                for( int i = 0; i < pids.Length; i++ )
                {
                    Commands.Debugging_Thread_Stack.Reply reply = GetThreadStack( pids[ i ] );

                    if( reply != null )
                    {
                        int depth = reply.m_data.Length;
                        ThreadStatus ts = new ThreadStatus( );

                        ts.m_pid = pids[ i ];
                        ts.m_status = reply.m_status;
                        ts.m_flags = reply.m_flags;
                        ts.m_calls = new string[ depth ];

                        for( int j = 0; j < depth; j++ )
                        {
                            ts.m_calls[ depth - 1 - j ] = String.Format( "{0} [IP:{1:X4}]", GetMethodName( reply.m_data[ j ].m_md, true ), reply.m_data[ j ].m_IP );
                        }

                        threads.Add( ts );
                    }
                }

                return threads;
            }

            return null;
        }

        public bool Deployment_GetStatus( out uint entrypoint, out uint storageStart, out uint storageLength )
        {
            Commands.Debugging_Deployment_Status.Reply status = Deployment_GetStatus( );

            if( status != null )
            {
                entrypoint = status.m_entryPoint;
                storageStart = status.m_storageStart;
                storageLength = status.m_storageLength;

                return true;
            }
            else
            {
                entrypoint = 0;
                storageStart = 0;
                storageLength = 0;

                return false;
            }
        }

        public Commands.Debugging_Deployment_Status.Reply Deployment_GetStatus( )
        {
            Commands.Debugging_Deployment_Status cmd = new Commands.Debugging_Deployment_Status( );
            Commands.Debugging_Deployment_Status.Reply cmdReply = null;

            IncomingMessage reply = SyncMessage( Commands.c_Debugging_Deployment_Status, Flags.c_NoCaching, cmd, 2, 10000 );
            if( reply != null )
            {
                cmdReply = reply.Payload as Commands.Debugging_Deployment_Status.Reply;
            }

            return cmdReply;
        }

        public bool Info_SetJMC( bool fJMC, ReflectionDefinition.Kind kind, uint index )
        {
            Commands.Debugging_Info_SetJMC cmd = new Commands.Debugging_Info_SetJMC( );

            cmd.m_fIsJMC = ( uint )( fJMC ? 1 : 0 );
            cmd.m_kind = ( uint )kind;
            cmd.m_raw = index;

            return IncomingMessage.IsPositiveAcknowledge( SyncMessage( Commands.c_Debugging_Info_SetJMC, 0, cmd ) );
        }

        private bool Deployment_Execute_Incremental( ArrayList assemblies, MessageHandler mh )
        {
            Commands.Debugging_Deployment_Status.ReplyEx status = Deployment_GetStatus( ) as Commands.Debugging_Deployment_Status.ReplyEx;

            if( status == null )
                return false;

            Commands.Debugging_Deployment_Status.FlashSector[ ] sectors = status.m_data;

            int iAssembly = 0;

            //The amount of bytes that the deployment will take
            uint deployLength = 0;

            //Compute size of assemblies to deploy
            for( iAssembly = 0; iAssembly < assemblies.Count; iAssembly++ )
            {
                byte[ ] assembly = ( byte[ ] )assemblies[ iAssembly ];
                deployLength += ( uint )assembly.Length;
            }

            if( deployLength > status.m_storageLength )
            {
                if( mh != null )
                    mh( string.Format( "Deployment storage (size: {0} bytes) was not large enough to fit deployment assemblies (size: {1} bytes)", status.m_storageLength, deployLength ) );
                
                return false;
            }

            //Compute maximum sector size
            uint maxSectorSize = 0;

            for( int iSector = 0; iSector < sectors.Length; iSector++ )
            {
                maxSectorSize = Math.Max( maxSectorSize, sectors[ iSector ].m_length );
            }

            //pre-allocate sector data, and a buffer to hold an empty sector's data
            byte[ ] sectorData = new byte[ maxSectorSize ];
            byte[ ] sectorDataErased = new byte[ maxSectorSize ];

            Debug.Assert( status.m_eraseWord == 0 || status.m_eraseWord == 0xffffffff );

            byte bErase = ( status.m_eraseWord == 0 ) ? ( byte )0 : ( byte )0xff;
            if( bErase != 0 )
            {
                //Fill in data for what an empty sector looks like
                for( int i = 0; i < maxSectorSize; i++ )
                {
                    sectorDataErased[ i ] = bErase;
                }
            }

            uint bytesDeployed = 0;

            //The assembly we are using
            iAssembly = 0;
            //byte index into the assembly remaining to deploy
            uint iAssemblyIndex = 0;
            //deploy each sector, one at a time
            for( int iSector = 0; iSector < sectors.Length; iSector++ )
            {
                Commands.Debugging_Deployment_Status.FlashSector sector = sectors[ iSector ];

                uint cBytesLeftInSector = sector.m_length;
                //byte index into the sector that we are deploying to.
                uint iSectorIndex = 0;

                //fill sector with deployment data
                while( cBytesLeftInSector > 0 && iAssembly < assemblies.Count )
                {
                    byte[ ] assembly = ( byte[ ] )assemblies[ iAssembly ];

                    uint cBytesLeftInAssembly = ( uint )assembly.Length - iAssemblyIndex;

                    //number of bytes from current assembly to deploy in this sector
                    uint cBytes = Math.Min( cBytesLeftInSector, cBytesLeftInAssembly );

                    Array.Copy( assembly, iAssemblyIndex, sectorData, iSectorIndex, cBytes );

                    cBytesLeftInSector -= cBytes;
                    iAssemblyIndex += cBytes;
                    iSectorIndex += cBytes;

                    //Is assembly finished?
                    if( iAssemblyIndex == assembly.Length )
                    {
                        //Next assembly
                        iAssembly++;
                        iAssemblyIndex = 0;

                        //If there is enough room to waste the remainder of this sector, do so
                        //to allow for incremental deployment, if this assembly changes for next deployment
                        if( deployLength + cBytesLeftInSector <= status.m_storageLength )
                        {
                            deployLength += cBytesLeftInSector;
                            break;
                        }
                    }
                }

                uint crc = Commands.Debugging_Deployment_Status.c_CRC_Erased_Sentinel;

                if( iSectorIndex > 0 )
                {
                    //Fill in the rest with erased value
                    Array.Copy( sectorDataErased, iSectorIndex, sectorData, iSectorIndex, cBytesLeftInSector );

                    crc = CRC.ComputeCRC( sectorData, 0, ( int )sector.m_length, 0 );
                }

                //Has the data changed from what is in this sector
                if( sector.m_crc != crc )
                {
                    //Is the data not erased
                    if( sector.m_crc != Commands.Debugging_Deployment_Status.c_CRC_Erased_Sentinel )
                    {
                        if( !EraseMemory( sector.m_start, sector.m_length ) )
                        {
                            if( mh != null )
                                mh( string.Format( "FAILED to erase device memory @0x{0:X8} with Length=0x{1:X8}", sector.m_start, sector.m_length ) );

                            return false;
                        }

#if DEBUG
                        Commands.Debugging_Deployment_Status.ReplyEx statusT = Deployment_GetStatus( ) as Commands.Debugging_Deployment_Status.ReplyEx;
                        Debug.Assert( statusT != null );
                        Debug.Assert( statusT.m_data[ iSector ].m_crc == Commands.Debugging_Deployment_Status.c_CRC_Erased_Sentinel );
#endif
                    }

                    //Is there anything to deploy
                    if( iSectorIndex > 0 )
                    {
                        bytesDeployed += iSectorIndex;

                        if( !WriteMemory( sector.m_start, sectorData, 0, ( int )iSectorIndex ) )
                        {
                            if( mh != null )
                                mh( string.Format( "FAILED to write device memory @0x{0:X8} with Length={1:X8}", sector.m_start, ( int )iSectorIndex ) );

                            return false;
                        }
#if DEBUG
                        Commands.Debugging_Deployment_Status.ReplyEx statusT = Deployment_GetStatus( ) as Commands.Debugging_Deployment_Status.ReplyEx;
                        Debug.Assert( statusT != null );
                        Debug.Assert( statusT.m_data[ iSector ].m_crc == crc );
                        //Assert the data we are deploying is not sentinel value
                        Debug.Assert( crc != Commands.Debugging_Deployment_Status.c_CRC_Erased_Sentinel );
#endif
                    }
                }
            }

            if( mh != null )
            {
                if( bytesDeployed == 0 )
                {
                    mh( "All assemblies on the device are up to date.  No assembly deployment was necessary." );
                }
                else
                {
                    mh( string.Format( "Deploying assemblies for a total size of {0} bytes", bytesDeployed ) );
                }
            }

            return true;
        }

        private bool Deployment_Execute_Full( ArrayList assemblies, MessageHandler mh )
        {
            uint entrypoint;
            uint storageStart;
            uint storageLength;
            uint deployLength;
            byte[ ] closeHeader = new byte[ 8 ];

            if( !Deployment_GetStatus( out entrypoint, out storageStart, out storageLength ) )
                return false;

            if( storageLength == 0 )
                return false;

            deployLength = ( uint )closeHeader.Length;

            foreach( byte[ ] assembly in assemblies )
            {
                deployLength += ( uint )assembly.Length;
            }

            if( mh != null )
                mh( string.Format( "Deploying assemblies for a total size of {0} bytes", deployLength ) );

            if( deployLength > storageLength )
                return false;

            if( !EraseMemory( storageStart, deployLength ) )
                return false;

            foreach( byte[ ] assembly in assemblies )
            {
                //
                // Only word-aligned assemblies are allowed.
                //
                if( assembly.Length % 4 != 0 )
                    return false;

                if( !WriteMemory( storageStart, assembly ) )
                    return false;

                storageStart += ( uint )assembly.Length;
            }

            if( !WriteMemory( storageStart, closeHeader ) )
                return false;

            return true;
        }

        public delegate void MessageHandler( String msg );


        public bool Deployment_Execute( ArrayList assemblies )
        {
            return Deployment_Execute( assemblies, true, null );
        }

        public bool Deployment_Execute( ArrayList assemblies, bool fRebootAfterDeploy, MessageHandler mh )
        {
            bool fDeployedOK = false;

            if( !PauseExecution( ) )
                return false;

            if( Capabilities.IncrementalDeployment )
            {
                if( mh != null )
                    mh( "Incrementally deploying assemblies to device" );
                fDeployedOK = Deployment_Execute_Incremental( assemblies, mh );
            }
            else
            {
                if( mh != null )
                    mh( "Deploying assemblies to device" );
                fDeployedOK = Deployment_Execute_Full( assemblies, mh );
            }

            if( !fDeployedOK )
            {
                if( mh != null )
                    mh( "Assemblies not successfully deployed to device." );
            }
            else
            {
                if( mh != null )
                    mh( "Assemblies successfully deployed to device." );
                if( fRebootAfterDeploy )
                {
                    if( mh != null )
                        mh( "Rebooting device..." );
                    RebootDevice( RebootOption.RebootClrOnly );
                }
            }

            return fDeployedOK;
        }

        public bool SetProfilingMode( uint iSet, uint iReset, out uint iCurrent )
        {
            Commands.Profiling_Command cmd = new Commands.Profiling_Command( );
            cmd.m_command = Commands.Profiling_Command.c_Command_ChangeConditions;
            cmd.m_parm1 = iSet;
            cmd.m_parm2 = iReset;

            IncomingMessage reply = SyncMessage( Commands.c_Profiling_Command, 0, cmd );
            if( reply != null )
            {
                Commands.Profiling_Command.Reply cmdReply = reply.Payload as Commands.Profiling_Command.Reply;

                if( cmdReply != null )
                {
                    iCurrent = cmdReply.m_raw;
                }
                else
                {
                    iCurrent = 0;
                }

                return true;
            }

            iCurrent = 0;
            return false;
        }

        public bool SetProfilingMode( uint iSet, uint iReset )
        {
            uint iCurrent;

            return SetProfilingMode( iSet, iReset, out iCurrent );
        }

        public bool FlushProfilingStream( )
        {
            Commands.Profiling_Command cmd = new Commands.Profiling_Command( );
            cmd.m_command = Commands.Profiling_Command.c_Command_FlushStream;
            SyncMessage( Commands.c_Profiling_Command, 0, cmd );
            return true;
        }

        private IncomingMessage DiscoverCLRCapability( uint caps )
        {
            Commands.Debugging_Execution_QueryCLRCapabilities cmd = new Commands.Debugging_Execution_QueryCLRCapabilities( );

            cmd.m_caps = caps;

            return SyncMessage( Commands.c_Debugging_Execution_QueryCLRCapabilities, 0, cmd );
        }

        private uint DiscoverCLRCapabilityUint( uint caps )
        {
            uint ret = 0;

            IncomingMessage reply = DiscoverCLRCapability( caps );

            if( reply != null )
            {
                Commands.Debugging_Execution_QueryCLRCapabilities.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_QueryCLRCapabilities.Reply;

                if( cmdReply != null && cmdReply.m_data != null && cmdReply.m_data.Length == 4 )
                {
                    object obj = ( object )ret;

                    new Converter( ).Deserialize( obj, cmdReply.m_data );

                    ret = ( uint )obj;
                }
            }

            return ret;
        }

        private CLRCapabilities.Capability DiscoverCLRCapabilityFlags( )
        {
            return ( CLRCapabilities.Capability )DiscoverCLRCapabilityUint( Commands.Debugging_Execution_QueryCLRCapabilities.c_CapabilityFlags );
        }

        private CLRCapabilities.SoftwareVersionProperties DiscoverSoftwareVersionProperties( )
        {
            IncomingMessage reply = DiscoverCLRCapability( Commands.Debugging_Execution_QueryCLRCapabilities.c_CapabilitySoftwareVersion );

            Commands.Debugging_Execution_QueryCLRCapabilities.SoftwareVersion ver = new Commands.Debugging_Execution_QueryCLRCapabilities.SoftwareVersion( );

            CLRCapabilities.SoftwareVersionProperties verCaps = new CLRCapabilities.SoftwareVersionProperties( );

            if( reply != null )
            {
                Commands.Debugging_Execution_QueryCLRCapabilities.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_QueryCLRCapabilities.Reply;

                if( cmdReply != null && cmdReply.m_data != null )
                {
                    new Converter( ).Deserialize( ver, cmdReply.m_data );

                    verCaps = new CLRCapabilities.SoftwareVersionProperties( ver.m_buildDate, ver.m_compilerVersion );
                }
            }

            return verCaps;
        }

        private CLRCapabilities.LCDCapabilities DiscoverCLRCapabilityLCD( )
        {
            IncomingMessage reply = DiscoverCLRCapability( Commands.Debugging_Execution_QueryCLRCapabilities.c_CapabilityLCD );

            Commands.Debugging_Execution_QueryCLRCapabilities.LCD lcd = new Commands.Debugging_Execution_QueryCLRCapabilities.LCD( );

            CLRCapabilities.LCDCapabilities lcdCaps = new CLRCapabilities.LCDCapabilities( );

            if( reply != null )
            {
                Commands.Debugging_Execution_QueryCLRCapabilities.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_QueryCLRCapabilities.Reply;

                if( cmdReply != null && cmdReply.m_data != null )
                {
                    new Converter( ).Deserialize( lcd, cmdReply.m_data );

                    lcdCaps = new CLRCapabilities.LCDCapabilities( lcd.m_width, lcd.m_height, lcd.m_bpp );
                }
            }

            return lcdCaps;
        }

        private CLRCapabilities.HalSystemInfoProperties DiscoverHalSystemInfoProperties( )
        {
            IncomingMessage reply = DiscoverCLRCapability( Commands.Debugging_Execution_QueryCLRCapabilities.c_CapabilityHalSystemInfo );

            Commands.Debugging_Execution_QueryCLRCapabilities.HalSystemInfo halSystemInfo = new Commands.Debugging_Execution_QueryCLRCapabilities.HalSystemInfo( );

            CLRCapabilities.HalSystemInfoProperties halProps = new CLRCapabilities.HalSystemInfoProperties( );

            if( reply != null )
            {
                Commands.Debugging_Execution_QueryCLRCapabilities.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_QueryCLRCapabilities.Reply;

                if( cmdReply != null && cmdReply.m_data != null )
                {
                    new Converter( ).Deserialize( halSystemInfo, cmdReply.m_data );

                    halProps = new CLRCapabilities.HalSystemInfoProperties(
                                    halSystemInfo.m_releaseInfo.Version, halSystemInfo.m_releaseInfo.Info,
                                    halSystemInfo.m_OemModelInfo.OEM, halSystemInfo.m_OemModelInfo.Model, halSystemInfo.m_OemModelInfo.SKU,
                                    halSystemInfo.m_OemSerialNumbers.module_serial_number, halSystemInfo.m_OemSerialNumbers.system_serial_number
                                    );
                }
            }

            return halProps;
        }

        private CLRCapabilities.ClrInfoProperties DiscoverClrInfoProperties( )
        {
            IncomingMessage reply = DiscoverCLRCapability( Commands.Debugging_Execution_QueryCLRCapabilities.c_CapabilityClrInfo );

            Commands.Debugging_Execution_QueryCLRCapabilities.ClrInfo clrInfo = new Commands.Debugging_Execution_QueryCLRCapabilities.ClrInfo( );

            CLRCapabilities.ClrInfoProperties clrInfoProps = new CLRCapabilities.ClrInfoProperties( );

            if( reply != null )
            {
                Commands.Debugging_Execution_QueryCLRCapabilities.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_QueryCLRCapabilities.Reply;

                if( cmdReply != null && cmdReply.m_data != null )
                {
                    new Converter( ).Deserialize( clrInfo, cmdReply.m_data );

                    clrInfoProps = new CLRCapabilities.ClrInfoProperties( clrInfo.m_clrReleaseInfo.Version, clrInfo.m_clrReleaseInfo.Info, clrInfo.m_TargetFrameworkVersion.Version );
                }
            }

            return clrInfoProps;
        }

        private CLRCapabilities.SolutionInfoProperties DiscoverSolutionInfoProperties( )
        {
            IncomingMessage reply = DiscoverCLRCapability( Commands.Debugging_Execution_QueryCLRCapabilities.c_CapabilitySolutionReleaseInfo );

            ReleaseInfo solutionInfo = new ReleaseInfo( );

            CLRCapabilities.SolutionInfoProperties solInfProps = new CLRCapabilities.SolutionInfoProperties( );

            if( reply != null )
            {
                Commands.Debugging_Execution_QueryCLRCapabilities.Reply cmdReply = reply.Payload as Commands.Debugging_Execution_QueryCLRCapabilities.Reply;

                if( cmdReply != null && cmdReply.m_data != null )
                {
                    new Converter( ).Deserialize( solutionInfo, cmdReply.m_data );

                    solInfProps = new CLRCapabilities.SolutionInfoProperties( solutionInfo.Version, solutionInfo.Info );
                }
            }

            return solInfProps;
        }

        private CLRCapabilities DiscoverCLRCapabilities( )
        {
            return new CLRCapabilities(
                            DiscoverCLRCapabilityFlags( ),
                            DiscoverCLRCapabilityLCD( ),
                            DiscoverSoftwareVersionProperties( ),
                            DiscoverHalSystemInfoProperties( ),
                            DiscoverClrInfoProperties( ),
                            DiscoverSolutionInfoProperties( )
                            );
        }

        ~Engine( )
        {
            Dispose( false );
        }

        public void Dispose( )
        {
            Dispose( true );

            GC.SuppressFinalize( this );
        }

        private void Dispose( bool fDisposing )
        {
            try
            {
                Stop( );

                if( m_state.SetValue( State.Value.Disposing ) )
                {
                    IDisposable disposable = m_ctrl as IDisposable;

                    if( disposable != null )
                    {
                        disposable.Dispose( );
                    }

                    m_state.SetValue( State.Value.Disposed );
                }
            }
            catch
            {
            }
        }
    }
}

