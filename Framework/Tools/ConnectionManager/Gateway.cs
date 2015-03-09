////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;

using _DBG = Microsoft.SPOT.Debugger;
using _WP  = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.SPOT.ConnectionManager
{
    /// <include file='doc\Gateway.uex' path='docs/doc[@for="Gateway"]/*' />
    public class Gateway : _WP.IControllerHostLocal, _WP.IController
    {
        _DBG.PortDefinition  m_portDefinition;
        _WP.IControllerLocal m_ctrl;
        bool                 m_stopDebuggerOnBoot;
        ManagerRemote        m_mgrRemote;

        ArrayList            m_stubs;
        Hashtable            m_endpoints;

        //--//

        public Gateway( ManagerRemote mgrRemote, _DBG.PortDefinition pd )
        {
            m_mgrRemote      = mgrRemote;
            m_portDefinition = pd;
            m_ctrl           = new _WP.Controller( _WP.Packet.MARKER_PACKET_V1, this );

            m_stubs          = ArrayList.Synchronized( new ArrayList() );
            m_endpoints      = Hashtable.Synchronized( new Hashtable() );
        }

        //--//

        #region IControllerRemote

        #region IController

        DateTime _WP.IController.LastActivity
        {
            get
            {
                return m_ctrl.LastActivity;
            }
        }

        bool _WP.IController.IsPortConnected
        {
            get
            {
                return m_ctrl.IsPortConnected;
            }
        }

        _WP.Packet _WP.IController.NewPacket()
        {
            return m_ctrl.NewPacket();
        }

        bool _WP.IController.QueueOutput( _WP.MessageRaw raw )
        {
            return m_ctrl.QueueOutput( raw );
        }

        void _WP.IController.SendRawBuffer( byte[] buf )
        {
            m_ctrl.SendRawBuffer( buf );
        }

        void _WP.IController.ClosePort()
        {
            m_ctrl.ClosePort();
        }

        void _WP.IController.Start()
        {
            m_ctrl.Start();
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        void _WP.IController.Stop()
        {
            m_stubs.Clear();

            m_ctrl.Stop();

            if(m_mgrRemote != null)
            {
                m_mgrRemote.Disconnect( this );
                m_mgrRemote = null;
            }
        }

        uint _WP.IController.GetUniqueEndpointId()
        {
            return m_ctrl.GetUniqueEndpointId();
        }

        CLRCapabilities _WP.IController.Capabilities
        {
            get { return m_ctrl.Capabilities ; }
            set { m_ctrl.Capabilities = value; }
        }

        #endregion

        #endregion

        //--//

        internal bool RegisterEndpoint( uint epType, uint epId, GatewayStub stub )
        {
            ulong epKey = EndpointKeyFromTypeId( epType, epId );

            lock(m_endpoints.SyncRoot)
            {
                if(m_endpoints.ContainsKey( epKey ))
                {
                    return false;
                }

                m_endpoints[epKey] = stub;
            }

            return true;
        }

        internal void DeregisterEndpoint( uint epType, uint epId, GatewayStub stub )
        {
            ulong epKey = EndpointKeyFromTypeId( epType, epId );

            GatewayStub gws = (GatewayStub)m_endpoints[epKey];

            if(gws != stub)
            {
                throw new ArgumentException( "Endpoint is not registered" );
            }

            m_endpoints.Remove( epKey );
        }

        public bool CanShutdown
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            get
            {                
                foreach(GatewayStub gws in m_stubs)
                {
                    if(!gws.IsLeaseExpired) return false;
                }

                return true;
            }
        }

        //--//

        public _DBG.PortDefinition PortDefinition
        {
            get
            {
                return m_portDefinition;
            }
        }

        public bool StopDebuggerOnBoot
        {
            get
            {
                return m_stopDebuggerOnBoot;
            }

            set
            {
                m_stopDebuggerOnBoot = value;
            }
        }

        //--//

        private GatewayStub FindGatewayStubForReply( _WP.IncomingMessage msg )
        {
            ArrayList stubs = CreateCopyOfStubs();

            for(int iStub = stubs.Count - 1; iStub >= 0; iStub--)
            {
                GatewayStub stub = (GatewayStub)stubs[iStub];

                if(stub.CanProcessMessageReply( msg ))
                    return stub;
            }

            return null;
        }

        private GatewayStub FindGatewayStubForRpc( _WP.IncomingMessage msg )
        {
            object payload = msg.Payload;
            uint epType    = 0;
            uint epId      = 0;

            _WP.Commands.Debugging_Messaging_Address addr = null;

            switch(msg.Header.m_cmd)
            {
                case _WP.Commands.c_Debugging_Messaging_Query:
                    addr   = ((_WP.Commands.Debugging_Messaging_Query)payload).m_addr;

                    epType = addr.m_to_Type;
                    epId   = addr.m_to_Id;
                    break;
                case _WP.Commands.c_Debugging_Messaging_Reply:
                    addr   = ((_WP.Commands.Debugging_Messaging_Reply)payload).m_addr;

                    epType = addr.m_from_Type;
                    epId   = addr.m_from_Id;
                    break;
                case _WP.Commands.c_Debugging_Messaging_Send:
                    addr   = ((_WP.Commands.Debugging_Messaging_Send)payload).m_addr;

                    epType = addr.m_to_Type;
                    epId   = addr.m_to_Id;
                    break;
                default:
                    return null;
            }

            ulong epKey = EndpointKeyFromTypeId( epType, epId );

            GatewayStub stub = (GatewayStub)m_endpoints[epKey];

            return stub;
        }

        private ArrayList FindGatewayStubsForMessage( _WP.IncomingMessage msg, bool fReply )
        {
            ArrayList stubs  = null;
            GatewayStub stub = null;
            bool fMulticast  = false;

            msg.Payload = _WP.Commands.ResolveCommandToPayload( msg.Header.m_cmd, fReply, ((_WP.IController)this).Capabilities );

            _WP.Packet bp = msg.Header;

            if(fReply)
            {
                stub = FindGatewayStubForReply( msg );
            }
            else
            {
                switch(bp.m_cmd)
                {
                    case _WP.Commands.c_Monitor_Ping:
                        if((msg.Header.m_flags & _WP.Flags.c_NonCritical) == 0)
                        {
                            _WP.Commands.Monitor_Ping.Reply cmdReply = new _WP.Commands.Monitor_Ping.Reply();
                            cmdReply.m_source    = _WP.Commands.Monitor_Ping.c_Ping_Source_Host;
                            cmdReply.m_dbg_flags = (m_stopDebuggerOnBoot? _WP.Commands.Monitor_Ping.c_Ping_DbgFlag_Stop: 0);
                            
                            msg.Reply( null, _WP.Flags.c_NonCritical, cmdReply);
                        }
                        break;

                    case _WP.Commands.c_Monitor_Message:
                    case _WP.Commands.c_Monitor_ProgramExit:
                    case _WP.Commands.c_Debugging_Button_Report:
                    case _WP.Commands.c_Debugging_Execution_BreakpointHit:
                    case _WP.Commands.c_Debugging_Lcd_NewFrame:
                    case _WP.Commands.c_Debugging_Lcd_NewFrameData:
                        fMulticast = true;
                        break;

                    case _WP.Commands.c_Debugging_Messaging_Query:
                    case _WP.Commands.c_Debugging_Messaging_Reply:
                    case _WP.Commands.c_Debugging_Messaging_Send:
                        stub = FindGatewayStubForRpc( msg );
                        break;
                }
            }

            if(fMulticast)
            {
                stubs = CreateCopyOfStubs();
            }
            else
            {
                stubs = new ArrayList();

                if(stub != null)
                {
                    stubs.Add( stub );
                }
            }

            return stubs;
        }

        //--//

        #region IControllerHostLocal

        #region IControllerHost

        void _WP.IControllerHost.SpuriousCharacters( byte[] buf, int offset, int count )
        {
            ArrayList stubs = CreateCopyOfStubs();

            foreach(GatewayStub stub in stubs)
            {
                try
                {
                    stub.SpuriousCharacters( buf, offset, count );
                }
                catch
                {
                }
            }
        }

        void _WP.IControllerHost.ProcessExited()
        {
            ArrayList stubs = CreateCopyOfStubs();

            foreach(GatewayStub stub in stubs)
            {
                try
                {
                    stub.ProcessExited();
                }
                catch
                {
                }
            }
        }

        #endregion

        Stream _WP.IControllerHostLocal.OpenConnection()
        {
            return m_portDefinition.Open();
        }

        bool _WP.IControllerHostLocal.ProcessMessage( _WP.IncomingMessage msg, bool fReply )
        {
            bool fRes = true;

            ArrayList stubs = FindGatewayStubsForMessage( msg, fReply );

            foreach(GatewayStub stub in stubs)
            {
                try
                {
                    if(!stub.ProcessMessage( msg, fReply ))
                    {
                        fRes = false;
                    }
                }
                catch
                {
                }
            }

            return fRes;
        }

        #endregion
        
        //--//

        internal ArrayList CreateCopyOfStubs()
        {
            return (ArrayList)m_stubs.Clone();
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        internal void Start( GatewayStub stub )
        {
            if(m_stubs.Contains( stub ) == false)
            {
                m_stubs.Add( stub );
            }
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        internal void Stop( GatewayStub stub )
        {
            m_stubs.Remove( stub );

            DeregisterEndpoints( stub );

            if(m_stubs.Count == 0)
            {
                ((_WP.IController)this).Stop();
            }
        }

        private ulong EndpointKeyFromTypeId( uint epType, uint epId )
        {
            return (epType << 32) | epId;
        }

        private void EndpointKeyToTypeId( uint epKey, out uint epType, out uint epId)
        {
            epId = (uint)epKey;
            epType = (uint)(epKey >> 32);
        }

        private void DeregisterEndpoints( GatewayStub stub )
        {
            lock(m_endpoints.SyncRoot)
            {
                Hashtable htClone = (Hashtable)m_endpoints.Clone();

                foreach(DictionaryEntry de in htClone)
                {
                    if(de.Value == stub)
                    {
                        m_endpoints.Remove( de.Key );
                    }
                }
            }
        }
    }
    
    /// <include file='doc\Gateway.uex' path='docs/doc[@for="GatewayStub"]/*' />
    public class GatewayStub : ServerRemoteObject, _WP.IControllerRemote
    {
        const int    c_ForcedPurgeLimit     = 100        ;
        const double c_ForcedPurgeThreshold = 10.0 / 60.0; // Seconds

        Gateway                   m_parent;
        _WP.IControllerHostRemote m_owner;
        Hashtable                 m_requests;
        
        //--//
        
        internal GatewayStub( Gateway parent, GatewayProxy owner )
        {
            m_parent    = parent;
            m_owner     = owner;

            m_requests  = Hashtable.Synchronized( new Hashtable() );

            owner.SetParent( this );
        }

        //--//
        
        public override void OnDisconnect()
        {
            ((_WP.IController)this).Stop();
        }

        //--//

        internal void SpuriousCharacters( byte[] buf, int offset, int count )
        {
            m_owner.SpuriousCharacters( buf, offset, count );
        }

        internal bool CanProcessMessageReply( _WP.IncomingMessage msg )
        {
            return m_requests.Contains( msg.Header.m_seqReply );
        }

        internal bool ProcessMessage( _WP.IncomingMessage msg, bool fReply )
        {
            m_requests.Remove( msg.Header.m_seqReply );

            return m_owner.ProcessMessage( msg.Raw.m_header, msg.Raw.m_payload, fReply );
        }

        public void ProcessExited()
        {
            m_owner.ProcessExited();
        }
        
        //--//

        void Purge( object state )
        {
            ManagerRemote.LogLine( "Purge GatewayStub" );

            DateTime threshold = DateTime.Now.AddMinutes( -(double)state );

            lock(m_requests.SyncRoot)
            {
                foreach(object key in new ArrayList( m_requests.Keys ))
                {
                    if((DateTime)m_requests[key] < threshold)
                    {
                        m_requests.Remove( key );
                    }
                }
            }
        }

        ulong EndpointKeyFromTypeId( uint epType, uint epId )
        {
            return (epType << 32) | epId;
        }

        private _WP.IController ParentIController
        {
            get { return (_WP.IController)m_parent; }
        }

        //--//

        #region IControllerRemote

        #region IController

        DateTime _WP.IController.LastActivity
        {
            get
            {
                return this.ParentIController.LastActivity;
            }
        }

        bool _WP.IController.IsPortConnected
        {
            get
            {
                return this.ParentIController.IsPortConnected;
            }
        }

        _WP.Packet _WP.IController.NewPacket()
        {
            return this.ParentIController.NewPacket();
        }

        bool _WP.IController.QueueOutput( _WP.MessageRaw raw )
        {
            _WP.Packet pkt = new _WP.Packet();
            
            new _WP.Converter( this.ParentIController.Capabilities ).Deserialize( pkt, raw.m_header );

            if(m_requests.Count > c_ForcedPurgeLimit) Purge( c_ForcedPurgeThreshold );

            m_requests[pkt.m_seq] = DateTime.Now;

            return this.ParentIController.QueueOutput( raw );
        }

        void _WP.IController.SendRawBuffer( byte[] buf )
        {
            this.ParentIController.SendRawBuffer( buf );
        }

        void _WP.IController.ClosePort()
        {
            this.ParentIController.ClosePort();
        }

        void _WP.IController.Start()
        {
            m_parent.Start( this );
        }

        void _WP.IController.Stop()
        {
            m_parent.Stop( this );

            m_requests.Clear();
        }

        uint _WP.IController.GetUniqueEndpointId()
        {
            return this.ParentIController.GetUniqueEndpointId();
        }

        CLRCapabilities _WP.IController.Capabilities
        {
            get { return this.ParentIController.Capabilities ; }
            set { this.ParentIController.Capabilities = value; }
        }

        #endregion

        bool _WP.IControllerRemote.RegisterEndpoint( uint epType, uint epId )
        {
            return this.m_parent.RegisterEndpoint( epType, epId, this );
        }

        void _WP.IControllerRemote.DeregisterEndpoint( uint epType, uint epId )
        {
            m_parent.DeregisterEndpoint( epType, epId, this );            
        }

        #endregion
    }

    /// <include file='doc\Gateway.uex' path='docs/doc[@for="GatewayProxy"]/*' />
    public class GatewayProxy : ClientRemoteObject, _WP.IControllerRemote, _WP.IControllerHostRemote, IDisposable
    {
        GatewayStub               m_parent;
        _WP.IControllerHostLocal  m_owner;

        //--//

        public void SetParent( GatewayStub parent )
        {
            m_parent = parent;                    
                        
            RegisterRemoteObject( m_parent );
        }

        internal void SetOwner( _WP.IControllerHostLocal owner )
        {
            m_owner = owner; 
        }        

        private _WP.IControllerRemote ParentIControllerRemote
        {
            get { return (_WP.IControllerRemote)m_parent; }
        }

        //--//        

        #region IControllerHostRemote

        #region IControllerHost

        void _WP.IControllerHost.SpuriousCharacters( byte[] buf, int offset, int count )
        {
            m_owner.SpuriousCharacters( buf, offset, count );
        }

        void _WP.IControllerHost.ProcessExited()
        {
            m_owner.ProcessExited();
        }

        #endregion

        bool _WP.IControllerHostRemote.ProcessMessage( byte[] header, byte[] payload, bool fReply )
        {
            _WP.MessageRaw      raw = new _WP.MessageRaw (); raw.m_header = header; raw.m_payload = payload;
            _WP.MessageBase     bs  = new _WP.MessageBase();
            _WP.IncomingMessage msg = new _WP.IncomingMessage( this, raw, bs );

            bs.m_header = new _WP.Packet();

            new _WP.Converter( this.ParentIControllerRemote.Capabilities ).Deserialize( bs.m_header, raw.m_header );

            return m_owner.ProcessMessage( msg, fReply );
        }

        #endregion

        #region IControllerRemote

        #region IController

        DateTime _WP.IController.LastActivity
        {
            get
            {
                return this.ParentIControllerRemote.LastActivity;
            }
        }

        bool _WP.IController.IsPortConnected
        {
            get
            {
                return this.ParentIControllerRemote.IsPortConnected;
            }
        }

        _WP.Packet _WP.IController.NewPacket()
        {
            return this.ParentIControllerRemote.NewPacket();
        }

        bool _WP.IController.QueueOutput( _WP.MessageRaw raw )
        {
            return this.ParentIControllerRemote.QueueOutput( raw );
        }

        void _WP.IController.SendRawBuffer( byte[] buf )
        {
            this.ParentIControllerRemote.SendRawBuffer( buf );
        }

        void _WP.IController.ClosePort()
        {
            this.ParentIControllerRemote.ClosePort();
        }

        void _WP.IController.Start()
        {
            this.ParentIControllerRemote.Start();
        }

        void _WP.IController.Stop()
        {
            this.ParentIControllerRemote.Stop();
        }

        uint _WP.IController.GetUniqueEndpointId()
        {
            return this.ParentIControllerRemote.GetUniqueEndpointId();
        }

        CLRCapabilities _WP.IController.Capabilities
        {
            get { return this.ParentIControllerRemote.Capabilities ; }
            set { this.ParentIControllerRemote.Capabilities = value; }
        }

        #endregion

        bool _WP.IControllerRemote.RegisterEndpoint( uint epType, uint epId  )
        {
            return this.ParentIControllerRemote.RegisterEndpoint( epType, epId );
        }

        void _WP.IControllerRemote.DeregisterEndpoint( uint epType, uint epId )
        {
            this.ParentIControllerRemote.DeregisterEndpoint( epType, epId );
        }

        #endregion

        #region IDisposable

        void IDisposable.Dispose()
        {
            Dispose( true );

            GC.SuppressFinalize( this );
        }

        #endregion

        //--//

        ~GatewayProxy()
        {
            Dispose( false );
        }

        private void Dispose(bool fDisposing)
        {
            try
            {
                ((_WP.IController)this).Stop();
            
                UnregisterRemoteObject();
            }
            catch
            {
            }
        }
    }
}
