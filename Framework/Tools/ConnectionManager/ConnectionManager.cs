////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Services;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

using _DBG   = Microsoft.SPOT.Debugger;
using _WP    = Microsoft.SPOT.Debugger.WireProtocol;
using _Trace = System.Diagnostics.Trace;

/*
 * Microsoft.SPOT.ConnectionManager.dll is the assembly that is shared by both the client (CM) and server 
 * (CMH -- Microsoft.SPOT.ConnectionManagerHost.exe) for the ConnectionManager.
 * 
 * This is a brief overview of the data structures involved, with emphasis on which objects live in which 
 * process, and the IPC involved.
 * 
 * The client instantiates a Microsoft.SPOT.ConnectionManager.Manager object.  The Manager object 
 * creates a remote object, ManagerRemote, which lives in CMH process.  The Manager and ManagerRemote objects 
 * contain references to one another. There is a many-to-one relationship in that many Manager objects talk to a 
 * singleton ManagerRemote. Bug 10042, activation has changed very slightly.  The client (hidden in the constructor of 
 * Manager, uses a single-call activation of ManagerFactory to get the ManagerRemote object.  This is done to avoid
 * the case where the ManagerRemote object's lease expires, and, either outstanding live GatewayStub objects, or else
 * the CMH just hasn't shut down yet, and .Net remoting would otherwise create a new ManagerRemote object.  
 * 
 * The ManagerRemote is responsible for two main tasks.  First, the ManagerRemote notifies each Manager when devices are 
 * inserted or removed.  The Manager exposes these as public events. The second task is to connect to devices.
 * 
 * ManagerRemote.Connect establishes a connection between a GatewayProxy (GWP) and a GatewayStub (GWS).  Manager.Connect
 * uses this functionality to create a Debugger.Engine, which can communicate with any SPOT device/emulator.  There is a 
 * one-to-one mapping of GWP and GWS objects.
 * 
 * From the CM side, Manager objects communicate with ManagerRemote objects.  GatewayProxy objects (often hidden in 
 * an Engine) communicate with GatewayStub objects.
 * 
 * From the CMH side, there exists a singleton ManagerRemote.  It contains a collection of Gateway objects.  The Gateway
 * is responsible for the physical connection do the device, and there is a one-to-one mapping between Gateways and devices. 
 * Each Gateway object has a collection of GWS objects, which is the path back to the client. 
 * 
 * Lifetime management.  We have two goals of lifetime management.  The first is the resource management, and common to all 
 * distributed applications.  All communications are explicitly ended in this system.  The Manager ends communication 
 * with the ManagerRemote through Manager.Disconnect.  The GWP ends communication with the GWS by GWS.Stop.  However, we 
 * obviously need to be more robust, when exceptions are thrown, processes die, or other causes of failure.  We rely on the
 * .Net remoting lifetime management here.  There are two sets of IPC, each with bidirectional communication.  .NET tries to
 * make both the server and client robust. In order to prevent the client object from being disconnected, Manager and GWP 
 * derive from ClientRemoteObject which overrides InitializeLifetimeService and prevents the client objects from ever being
 * disconnected.  On the server side, TrackingServices is used help with cleanup functionality.  Once the server's lease 
 * expires (ManagerRemote or GWS) it is then unreachable from the client(s) so cleanup can be done safely.
 * 
 * The second goal of lifetime management is, as long as CMH remains a process and not a service, to allow CMH to shut down 
 * once all clients are no longer using it's services.  CHM queries ManagerRemote.CanShutdown which explicitly checks to see
 * if the ManagerRemote and all of the GWS leases have expired.  Checking the GWS objects just adds an extra level of 
 * redundancy, as the GWS lease will expire, and it will shut itself down in the normal case (assuming that the GWP doesn't 
 * shutdown normally).
 * 
 */

namespace Microsoft.SPOT.ConnectionManager
{
    /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="DeviceNotification"]/*' />
    public delegate void DeviceNotification( _DBG.PortDefinition pd );
        
    public class ChannelRegistrationHelper
    {
        public const string c_ChannelName = "ipc";

        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern bool ProcessIdToSessionId(int dwProcessId, out int pSessionId);            

        public static void RegisterChannel( string port )
        {
            IDictionary props = new Hashtable();

            props[ "portName" ] = port;            
            props[ "name"     ] = ChannelRegistrationHelper.c_ChannelName;

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            IChannel channel = new IpcChannel( props, null, provider );
            ChannelServices.RegisterChannel( channel, true );
        }

        public static int CurrentSessionId
        {
            get
            {
                int sessionId;
                int pid        = Process.GetCurrentProcess().Id;                

                ProcessIdToSessionId( pid, out sessionId );

                return sessionId;
            }
        }
    }

    public class ManagerFactory : MarshalByRefObject
    {        
        private const string c_Port        = "SPOTConnectionManager";      
        private const string c_ServiceName = "ManagerFactory.rem";

        static ManagerRemote s_managerRemote;


        public ManagerRemote ManagerRemote
        {
            get
            {
                lock(typeof(ManagerFactory))
                {
                    if(s_managerRemote == null)
                    {
                        s_managerRemote = new ManagerRemote();
                    }
                    
                    return s_managerRemote;
                }
            }
        }

        static public string ServiceName
        {
            get { return c_ServiceName; }
        }

        static public string Port
        {
            get 
            { 
                return c_Port + ChannelRegistrationHelper.CurrentSessionId.ToString();
            }
        }
    }

    /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="ManagerRemote"]/*' />
    public class ManagerRemote : ServerRemoteObject, ITrackingHandler
    {
        private class Client
        {
            public Manager m_manager;
            public bool    m_fNotify;

            public Client(Manager mgr)
            {
                m_manager = mgr;
            }
        }

        private List<Client>             m_clients           = new List<Client>();
        
        private Dictionary<object, _DBG.PortDefinition> m_devices = new Dictionary<object, Microsoft.SPOT.Debugger.PortDefinition>();
        private Dictionary<object, Gateway> m_gateways = new Dictionary<object, Gateway>();
        private  _DBG.UsbDeviceDiscovery m_usbDiscovery;
        private  _DBG.EmulatorDiscovery  m_emulatorDiscovery;        
        private int                      m_clientsInterestedInEvents;
        
        //--//

        public ManagerRemote()
        {
            ManagerRemote.LogLine( "New ManagerRemote" );            
            TrackingServices.RegisterTrackingHandler( this );
        }
              
        ~ManagerRemote()
        {            
            ManagerRemote.LogLine( "Delete ManagerRemote" );
            TrackingServices.UnregisterTrackingHandler( this );
        }
 
        //--//
        
        public override void OnDisconnect()
        {
            List<Client> clients = this.GetClientsCopy();
            
            foreach(Client client in clients)
            {
                RemoveClient( client.m_manager );
            }
        }


        //--//

        public bool CanShutdown
        {
            get
            {        
                if(!this.IsLeaseExpired) return false;
                
                foreach(Gateway gw in this.GetGatewaysCopy())
                {
                    if(!gw.CanShutdown) return false; 
                }
                
                return true;                
            }
        }

        public ArrayList EnumeratePorts( params _DBG.PortFilter[] args )
        {
            //Refresh m_devices list if out of date, or if events are turned off
            RefreshDeviceList();

            lock(m_devices)
            {
                ArrayList lst = new ArrayList();

                foreach(_DBG.PortFilter pf in args)
                {
                    foreach(_DBG.PortDefinition pd in m_devices.Values)
                    {
                        _DBG.PortDefinition pd2;

                        switch(pf)
                        {
                            case _DBG.PortFilter.Emulator: pd2 = pd as _DBG.PortDefinition_Emulator; break;
                            case _DBG.PortFilter.Serial  : pd2 = pd as _DBG.PortDefinition_Serial  ; break;
                            case _DBG.PortFilter.Usb     : pd2 = pd as _DBG.PortDefinition_Usb     ; break;
                            default                      : pd2 = null                              ; break;
                        }

                        if(pd2 != null && lst.Contains( pd2 ) == false)
                        {
                            lst.Add( pd2 );
                        }
                    }
                }

                return lst;
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void Register( Manager mgr )
        {            
            AddClient( mgr );         
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void Unregister( Manager mgr )
        {
            UnregisterForEvents( mgr );
            RemoveClient       ( mgr );
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void RegisterForEvents( Manager mgr )
        {
            Client client = FindClient( mgr );

            if (client != null && client.m_fNotify == false)
            {
                client.m_fNotify = true;

                if (m_clientsInterestedInEvents++ == 0)
                {
                    StartEvents();
                    RefreshDeviceList();
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void UnregisterForEvents( Manager mgr )
        {
            Client client = FindClient(mgr);

            if (client != null && client.m_fNotify == true)
            {
                client.m_fNotify = false;

                if (--m_clientsInterestedInEvents == 0)
                {
                    StopEvents();
                }                
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public GatewayStub Connect(_DBG.PortDefinition pd, GatewayProxy gwp)
        {
            lock (m_gateways)
            {
                object key = pd.UniqueId;
                Gateway gw = null;

                if (m_gateways.ContainsKey(key))
                {
                    gw = m_gateways[key];
                }
                else
                {
                    gw = new Gateway(this, pd);

                    ((_WP.IController)gw).Start();

                    m_gateways[key] = gw;
                }

                return new GatewayStub(gw, gwp);
            }
        }

        public void Disconnect(Gateway gw)
        {
            lock (m_gateways)
            {
                object key = gw.PortDefinition.UniqueId;

                if (gw == m_gateways[key])
                {
                    m_gateways.Remove(key);
                }
            }
        }

        //--//

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void StartEvents()
        {
            ManagerRemote.LogLine( "StartEvents" );

            if(m_usbDiscovery == null)
            {
                m_usbDiscovery                  = new _DBG.UsbDeviceDiscovery();
                m_usbDiscovery.OnDeviceChanged += new _DBG.UsbDeviceDiscovery.DeviceChangedEventHandler( OnDeviceChanged );
            }

            if(m_emulatorDiscovery == null)
            {
                m_emulatorDiscovery                    = new _DBG.EmulatorDiscovery();
                m_emulatorDiscovery.OnEmulatorChanged += new _DBG.EmulatorDiscovery.EmulatorChangedEventHandler( OnEmulatorChanged ); 
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void StopEvents()
        {
            ManagerRemote.LogLine( "StopEvents" );

            if(m_usbDiscovery != null)
            {
                m_usbDiscovery.Dispose();
                m_usbDiscovery = null;
            }

            if(m_emulatorDiscovery != null)
            {
                m_emulatorDiscovery.Dispose();
                m_emulatorDiscovery = null;
            }
        }

        private Client FindClient(Manager mgr, out int iClient)
        {
            lock (m_clients)
            {
                for (iClient = 0; iClient < m_clients.Count; iClient++)
                {
                    Client client = m_clients[iClient];

                    if ((object)client.m_manager == (object)mgr) return client;
                }
            }

            return null;
        }

        private Client FindClient( Manager mgr )
        {
            int iClient;

            return FindClient( mgr, out iClient );
        }

        private void AddClient(Manager mgr)
        {
            lock(m_clients)
            {
                if(FindClient( mgr ) == null)
                {
                    m_clients.Add( new Client(mgr) );
                }
            }
        }

        private List<Client> GetClientsCopy()
        {
            lock(m_clients)
            {
                return new List<Client>( m_clients ) ;
            }
        }

        private ArrayList GetGatewaysCopy()
        {
            lock(m_gateways)
            {
                return new ArrayList(m_gateways.Values);
            }
        }

        private void RemoveClient(Manager mgr)
        {
            lock(m_clients)
            {
                int iClient;
                Client client = FindClient( mgr, out iClient );

                if (client != null)
                {                    
                    m_clients.RemoveAt(iClient);
                }
            }
        }

        //--//

        private void OnEmulatorChanged()
        {
            MonitorEmulators();
        }

        private void OnDeviceChanged( _DBG.UsbDeviceDiscovery.DeviceChanged change )
        {
            MonitorUSB();
        }

        //--//

        private void MonitorEmulators()
        {
            Monitor( _DBG.PortDefinition.Enumerate( _DBG.PortFilter.Emulator ), typeof(_DBG.PortDefinition_Emulator) );
        }

        private void MonitorUSB()
        {
            Monitor( _DBG.PortDefinition.Enumerate( _DBG.PortFilter.Usb ), typeof(_DBG.PortDefinition_Usb) );
        }

        private void Monitor( ArrayList lst, Type t )
        {
            bool      fNotify = false;
            ArrayList lstAdd  = new ArrayList();
            ArrayList lstDel  = new ArrayList();

            lock(m_devices)
            {
                Hashtable ht = new Hashtable();

                foreach(_DBG.PortDefinition pd in lst)
                {
                    object key = pd.UniqueId;

                    if(m_devices.ContainsKey( key ) == false)
                    {
                        m_devices[ key ] = pd;

                        lstAdd.Add( pd );
                        fNotify = true;
                    }

                    ht[ key ] = pd;
                }

                foreach(_DBG.PortDefinition pd in m_devices.Values)
                {
                    if(t.IsInstanceOfType( pd ))
                    {
                        if(ht.Contains( pd.UniqueId ) == false)
                        {
                            lstDel.Add( pd );
                            fNotify = true;
                        }
                    }
                }

                foreach(_DBG.PortDefinition pd in lstDel)
                {
                    m_devices.Remove( pd.UniqueId );
                }
            }

            if(fNotify)
            {
                List<Client> clients = GetClientsCopy();

                foreach(Client client in clients)
                {
                    try
                    {
                        if (client.m_fNotify)
                        {
                            client.m_manager.NotifyChange(lstAdd, lstDel);
                        }
                    }
                    catch
                    {                        
                    }
                }                
            }
        }

        private void RefreshDeviceList()
        {
            MonitorEmulators();
            MonitorUSB();
        }

        //--//

        [System.Diagnostics.ConditionalAttribute("TRACE")]
        static public void InitializeLog()
        {
            const string LogFileName = @"%TEMP%\SPOT_connection_manager.txt";

            StreamWriter outputWriter = File.AppendText( Environment.ExpandEnvironmentVariables( LogFileName ) );

            _Trace.Listeners.Add( new TextWriterTraceListener( outputWriter ) );

            _Trace.WriteLine( "" );
            _Trace.WriteLine( "" );

            ManagerRemote.LogLine( "InitializeLog" );
        }

        [System.Diagnostics.ConditionalAttribute("TRACE")]
        static public void LogLine( string line )
        {
            _Trace.WriteLine( String.Format( "{0}: {1}", DateTime.Now, line ) );
            _Trace.Flush();
        }

        static internal byte[] Serialize( params object[] args )
        {
            MemoryStream                                                   stream = new MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf     = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            bf.Serialize( stream, args );

            return stream.ToArray();
        }

        static internal object[] Deserialize( byte[] args )
        {
            MemoryStream                                                   stream = new MemoryStream( args );
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf     = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            return (object[])bf.Deserialize( stream );
        }
        #region ITrackingHandler Members

        void ITrackingHandler.DisconnectedObject(object obj)
        {
            ServerRemoteObject sro = obj as ServerRemoteObject;

            if(sro != null)
            {
                sro.OnDisconnect();
            }
        }

        void ITrackingHandler.UnmarshaledObject(object obj, ObjRef or)
        {            
        }

        void ITrackingHandler.MarshaledObject(object obj, ObjRef or)
        {            
        }

        #endregion
    }

    /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="Manager"]/*' />
    public class Manager : ClientRemoteObject, IDisposable
    {
        ManagerRemote            m_mgr;
        event DeviceNotification m_insertion;
        event DeviceNotification m_removal;
        
        //--//

#if DEBUG
        static Manager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler( ResolveAssembly );
        }

        static Assembly ResolveAssembly( object sender, ResolveEventArgs args )
        {
            foreach (Assembly assm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if(assm.FullName == args.Name) return assm;                
            }

            return null;
        }
#endif

// ENABLING THIS FOR RELEASE BUILDS FOR NOW SO THAT DEBUGGING WILL WORK FOR RELEASE BUILDS 
// THIS SHOULD BE REVISITED FOR FUTURE RELEAsES
        private bool NoConnectionManagerHost
        {
            get
            {
                using(Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETTinyFramework"))
                {
                    if(key == null) return false;

                    object o = key.GetValue("NoConnectionManagerHost");

                    if(o == null || !(o is int)) return false;

                    return (int)o != 0;
                }
            }
        }
//#endif

        public Manager( ManagerFactory factory )
        {            
            RegisterChannel();
            Initialize( factory );
        }

        /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="Manager.Manager"]/*' />
        public Manager() 
        {

            ManagerFactory factory = null;

// ENABLING THIS FOR RELEASE BUILDS FOR NOW SO THAT DEBUGGING WILL WORK FOR RELEASE BUILDS 
// THIS SHOULD BE REVISITED FOR FUTURE RELEAsES
//#if DEBUG
            if (NoConnectionManagerHost)
            {
                factory = new ManagerFactory();

                Initialize( factory );

                return;
            }
//#endif                

            RegisterChannel();                         
            
            //
            // Launch the ConnectionManagerHost if possible
            //
            bool fLaunch = true;
            
            factory = (ManagerFactory)Activator.GetObject( typeof(ManagerFactory), string.Format( "{0}://{1}/{2}", ChannelRegistrationHelper.c_ChannelName, ManagerFactory.Port, ManagerFactory.ServiceName ) );                    
                
            for (int iRetries = 0; iRetries < 20; iRetries++)
            {
                if(Initialize( factory ))
                {
                    break;
                }

                if(fLaunch)
                {
                    string file = null;

                    using(Win32.RegistryKey key = Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETTinyFramework"))
                    {
                        file = (string)key.GetValue("ConnectionManager");
                    }
                
                    System.Diagnostics.Process process = new Process();
                    process.StartInfo = new ProcessStartInfo(file);
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();

                    fLaunch = false;
                }

                Thread.Sleep(100);                  
            }     
            
            if(m_mgr == null)
            {
                throw new ApplicationException("Could not create ManagerRemote");
            }
        }

        private void RegisterChannel()
        {
            //
            // Create a back-channel for the event callbacks from the service.
            //           
            
            if(ChannelServices.GetChannel(ChannelRegistrationHelper.c_ChannelName) == null)
            {
                string port = "backchannel_" + Guid.NewGuid().ToString( "N" );

                ChannelRegistrationHelper.RegisterChannel( port );
            }
        }

        private bool Initialize( ManagerFactory factory )
        {            
            ManagerRemote mgr = null;

            try
            {
                mgr = factory.ManagerRemote;
                mgr.Register( this );
                RegisterRemoteObject( mgr );
            }
            catch
            {
                return false;
            }

            m_mgr = mgr;

            return true;
        }
       
        /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="Manager.~Manager"]/*' />
        ~Manager()
        {
            Dispose( false );
        }

        void IDisposable.Dispose()
        {
            Dispose( true );    
        
            GC.SuppressFinalize( this );
        }

        private void Dispose(bool fDisposing)
        {
            try
            {
                ManagerRemote mgr = m_mgr;
                m_mgr = null; 

                if(mgr != null)
                {
                    /*
                     * Not sure if we care about supporting this case, but this check allows us to 
                     * support registering for events, releasing all references to this object, but
                     * still makes sure that the ManagerRemote object does not get disconnected
                     * and will still notify device events
                     */
                    if(m_insertion == null && m_removal == null)
                    {
                        mgr.Unregister( this );                    
                        UnregisterRemoteObject();                        
                    }                    
                }                
            }
            catch
            {
            }            
        }

        
        /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="Manager.NotifyChange"]/*' />
        public void NotifyChange( ArrayList lstAdd, ArrayList lstDel)
        {
            DeviceNotification insertion;
            DeviceNotification removal;

            lock(this)
            {
                insertion = m_insertion;
                removal   = m_removal;
            }

            try
            {
                if(removal != null)
                {
                    foreach(_DBG.PortDefinition pd in lstDel)
                    {
                        removal( pd );
                    }
                }

                if(insertion != null)
                {
                    foreach(_DBG.PortDefinition pd in lstAdd)
                    {
                        insertion( pd );
                    }
                }
            }
            catch
            {
            }
        }

        //--//

        /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="Manager.EnumeratePorts"]/*' />
        public ArrayList EnumeratePorts( params _DBG.PortFilter[] args )
        {
            return m_mgr.EnumeratePorts( args );
        }

        /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="Manager.DeviceInsertion"]/*' />
        public event DeviceNotification DeviceInsertion
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                if (m_insertion == null)
                {
                    m_mgr.RegisterForEvents(this);
                }

                m_insertion += value;                                
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove
            {
                m_insertion -= value;

                if (m_insertion == null)
                {
                    m_mgr.UnregisterForEvents(this);
                }
            }
        }

        /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="Manager.DeviceRemoval"]/*' />
        public event DeviceNotification DeviceRemoval
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                m_removal += value;
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove
            {
                m_removal -= value;
            }
        }

        /// <include file='doc\ConnectionManager.uex' path='docs/doc[@for="Manager.Connect"]/*' />
        public _DBG.Engine Connect( _DBG.PortDefinition pd )
        {
            _DBG.Engine eng = new _DBG.Engine(); 

            GatewayProxy gwp = new GatewayProxy();
            GatewayStub gws = m_mgr.Connect( pd, gwp );                        
            
            gwp.SetOwner( eng );

            eng.SetController( gwp );
            
            return eng;            
        }                
    }


    public abstract class ClientRemoteObject : MarshalByRefObject
    {
        protected ClientSponsor m_sponsor;
        private ServerRemoteObject m_mbr;
        
        /* 
         * ClientRemoteObject lives on the client, and is used for callbacks from ManagerRemote
         * As long as the object is alive, it never needs to unregister from the remoting infrastructure          
         */
        public override object InitializeLifetimeService()
        {
            return null;
        }
 
        //--//

        protected void RegisterRemoteObject(ServerRemoteObject mbr)
        {
            UnregisterRemoteObject();

            m_mbr     = mbr;
            m_sponsor = new ClientSponsor();
            m_sponsor.Register( mbr );            
        }

        protected void UnregisterRemoteObject()
        {
            try
            {
                if(m_sponsor != null)
                {
                    m_sponsor.Unregister( m_mbr );            
                }                
            }
            catch
            {
            }
            
            m_sponsor = null;
            m_mbr     = null;
        }
    }

    public abstract class ServerRemoteObject : MarshalByRefObject
    {

#if DEBUG
        public override object InitializeLifetimeService()
        {
            ILease lease = base.InitializeLifetimeService() as ILease;

            if(lease.CurrentState == LeaseState.Initial)
            {
                lease.InitialLeaseTime   = TimeSpan.FromMinutes( 1 );
                lease.RenewOnCallTime    = TimeSpan.FromMinutes( 1 );
                lease.SponsorshipTimeout = TimeSpan.FromMinutes( 1 );
            }

            return lease;
        }

#endif

        public bool IsLeaseExpired
        {
            get
            {
                ILease lease = this.GetLifetimeService() as ILease;

                return (lease == null || lease.CurrentState == LeaseState.Expired);
            }
        }

        public virtual void OnDisconnect()
        {
        }
    }
}

