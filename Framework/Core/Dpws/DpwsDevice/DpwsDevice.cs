using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Threading;
using Dpws.Device.Discovery;
using Dpws.Device.Mex;
using Dpws.Device.Services;
using Ws.Services;
using Ws.Services.Binding;
using Ws.Services.Transport;
using Ws.Services.Transport.HTTP;
using Ws.Services.Transport.UDP;
using Ws.Services.Utilities;

using System.Runtime.CompilerServices;

using Microsoft.SPOT.Net.NetworkInformation;
using System.Ext;
using Ws.Services.Xml;
using Ws.Services.WsaAddressing;
using Microsoft.SPOT;
using Ws.Services.Discovery;

namespace Dpws.Device
{

    /// <summary>
    /// Creates a device used to manage global device information, host and hosted services.
    /// </summary>
    /// <remarks>
    /// The Device class provides the base functionality of a device. It is the primary control class
    /// for a device and is the container and manager of hosted services provided by a device. This class exposes
    /// static methods and properties used to manage device information, start and stop internal services
    /// and manage hosted services.
    ///
    /// A device developer uses this class to set device specific information common to all hosted services
    /// like ThisModel, ThisDevice and endpoint informaton like the device address and device id. When
    /// developing hosted services this class provides access to information required by the service
    /// when building response messages. The device class also manages a collection of hosted services
    /// that implement the functionality of a device.
    ///
    /// Hosted services are created by device developers. These services are added to the collection
    /// of hosted services maintained by the device class. When the device services, start an instance
    /// of each hosted service in the hosted services collection is instantiated. When a device request is
    /// received the hosted services collection contains information used by the transport services
    /// to route request to a specific service.
    ///
    /// The device class also manages a set of internal hosted services that provide the boiler plate
    /// functionality of DPWS Device Profile for Web Services) like Discovery, Mex and the Event Subscription
    /// manager. As mentioned these are boiler plate services and are therefore not exposed to a device
    /// developer.
    ///
    /// The device class also manages a collection of namespaces common to all DPWS compliant devices.
    ///
    /// If a device requires Host functionality outlined in the DPWS specification a hosted service can be
    /// added to the device to provide this functionality. Special provisions are made because a devices
    /// Host service has the same endpoint as the device.
    /// </remarks>
    public static class Device
    {
        // Fields
        /// <summary>
        /// This class stores model specific information about a device.
        /// </summary>
        public static class ThisModel
        {
            /// <summary>
            /// Name of the manufacturer of this device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
            public static string Manufacturer = string.Empty;

            /// <summary>
            /// Url to a Web site of the manufacturer of this device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_URI_SIZE octets.</remarks>
            public static string ManufacturerUrl = string.Empty;

            /// <summary>
            /// User friendly name for this model of device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
            public static string ModelName;

            /// <summary>
            /// Model number for this model of device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
            public static string ModelNumber;

            /// <summary>
            /// URL to a Web site for this model of device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_URI_SIZE octets.</remarks>
            public static string ModelUrl;

            /// <summary>
            /// URL to an Html page for this device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_URI_SIZE octets.</remarks>
            public static string PresentationUrl;

            public static WsXmlNodeList Any;
        }

        /// <summary>
        /// This class stores device specific information about a device.
        /// </summary>
        public static class ThisDevice
        {
            /// <summary>
            /// User-friendly name for this device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
            public static string FriendlyName;

            /// <summary>
            /// Firmware version for this device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
            public static string FirmwareVersion;

            /// <summary>
            /// Manufacturer assigned serial number for this device.
            /// </summary>
            /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
            public static string SerialNumber;

            public static WsXmlNodeList Any;
        }

        private static DpwsWseEventSinkQMgr m_eventQManager;
        private static int                  m_MetadataVersion;
        private static int                  m_AppSequence;
        private static int                  m_MessageID;
        private static string               m_address;
        private static WsServiceEndpoints   m_hostedServices;
        private static WsHttpServiceHost    m_httpServiceHost;
        private static int                  m_appMaxDelay;
        private static bool                 m_isStarted;
        private static bool                 m_supressAdhoc;
        private static Binding              m_binding;

        static Device()
        {
            m_MetadataVersion   = 1;
            m_AppSequence       = 1;
            m_MessageID         = 0;
            m_hostedServices    = new WsServiceEndpoints();
            m_appMaxDelay       = 500;
        }
        
        /// <summary>
        /// This class is an internal hosted service that provides DPWS compliant WS-Discovery Probe and Resolve
        /// service endpoints.
        /// </summary>
        internal static DpwsDeviceDiscoService m_discoveryService;
        internal static DpwsDiscoGreeting      m_discoGreeting;

        /// <summary>
        /// This class is an internal hosted service that provides DPWS compliant WS-Transfer (Metadata Exchange) services.
        /// </summary>
        internal static DpwsDeviceMexService m_discoMexService;

        /// <summary>
        /// Property used to determine if device services are running.
        /// </summary>
        public static bool Started { get { return m_isStarted; } }

        /// <summary>
        /// Collection property used to add hosted services to this device. Hosted services implement device
        /// specific services and derive from DpwsHostedService.
        /// </summary>
        public static WsServiceEndpoints HostedServices { get { return m_hostedServices; } }

        /// <summary>
        /// Property useed to set the maximum number of threads used to process Http request. Default value is 5.
        /// Use this as a tuning parameter and be conservative.
        /// </summary>
        public static int MaxServiceThreads
        {
            get
            {
                return m_httpServiceHost != null ? m_httpServiceHost.MaxThreadCount : 0;
            }

            set
            {
                if (m_httpServiceHost == null)
                    throw new NullReferenceException();
                m_httpServiceHost.MaxThreadCount = value;
            }
        }

        /// <summary>
        /// Property useed to set the maximum number of threads used to process Udp (Discovery) request. Default value is 5.
        /// Use this as a tuning parameter and be conservative.
        /// </summary>
        public static int MaxDiscoveryThreads
        {
            get
            {
                return WsUdpServiceHost.Instance.MaxThreadCount;
            }

            set
            {
                WsUdpServiceHost.Instance.MaxThreadCount = value;
            }
        }

        /// <summary>
        /// Use this static property if the device requires a Host relationship, create a hosted service that
        /// implements device specific host funcitonality and assign it to the Host property.
        /// The Host, hosted service will have the same endpoint address as the device.
        /// </summary>
        public static DpwsHostedService Host;

        /// <summary>
        /// Field used to access an event subscription manager. The subscription manager provides event
        /// subscription functionality required to fire events from a hosted service.
        /// </summary>
        public static DpwsWseSubscriptionMgr SubscriptionManager;

        /// <summary>
        /// Property containing the maximum wait time for a probe match message response.
        /// </summary>
        /// <remarks>
        /// The default value is 500 ms. as per Dpws specification. Set this value to 0 if implementing
        /// a discovery proxy. See Ws-Discovery specification for details.
        /// </remarks>
        public static int ProbeMatchDelay { get { return m_appMaxDelay; } set { m_appMaxDelay = value; } }

        /// <summary>
        /// Set this property to true to ignore disco request from the same ip address as the device.
        /// </summary>
        public static bool IgnoreLocalClientRequest
        {
            get
            {
                return WsUdpServiceHost.Instance.IgnoreRequestFromThisIP;
            }

            set
            {
                WsUdpServiceHost.Instance.IgnoreRequestFromThisIP = value;
            }
        }

        /// <summary>
        /// Initializes the device. This method builds the colletion of internal device services, sets
        /// the devices transport address and adds DPWS required namespaces to the devices namespace collection.
        /// </summary>
        public static void Initialize(Binding binding, ProtocolVersion v)
        {
            m_binding = binding;

            string addr = binding.Transport.EndpointAddress.AbsoluteUri;
            int index = addr.LastIndexOf('/');
            m_address = "urn:uuid:" + addr.Substring(index+1);

            SubscriptionManager = new DpwsWseSubscriptionMgr(binding, v);

            if(v != null) 
            {            
                // Add disco services to udp service endpoints collection
                m_discoveryService = new DpwsDeviceDiscoService(v);
                m_discoGreeting = new DpwsDiscoGreeting(v);
                // Create a new udp service host and add the discovery endpoints
                WsUdpServiceHost.Instance.AddServiceEndpoint(m_discoveryService);
            }

            // Add metadata get service endpoint
            m_discoMexService = new DpwsDeviceMexService(v);
            m_hostedServices.DiscoMexService = m_discoMexService;

            // Add direct probe service endpoint
            if(m_discoveryService != null) m_hostedServices.Add(m_discoveryService);

            // Create a new http service host and add hosted services endpoints
            m_httpServiceHost = new WsHttpServiceHost(m_binding, m_hostedServices);

            System.Ext.Console.Write("IP Address: " + WsNetworkServices.GetLocalIPV4Address());
        }

        /// <summary>
        /// Property contining the IPV4 address of this device.
        /// </summary>
        public static string IPV4Address { get { return WsNetworkServices.GetLocalIPV4Address(); } }

        /// <summary>
        /// Property containing the http listener port number.
        /// </summary>
        public static int Port
        {
            get 
            { 
                if(m_binding == null) throw new Exception(); // Not initialized
                
                return m_binding.Transport.EndpointPort; 
            }
        }

        /// <summary>
        /// Method used to Start the stack transport services.
        /// </summary>
        /// <remarks>
        /// The Start Method initialises device specific services and calls the stack services to start
        /// the Http and UDP transport services. It also creates and starts an instance of the
        /// Event Queue manager service that manages event subscription expirations.
        /// </remarks>
        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        public static void Start(ServerBindingContext ctx)
        {
            if(m_isStarted) throw new InvalidOperationException();

            m_isStarted = true;

            // Start Http service host
            m_httpServiceHost.Start(ctx);
            System.Ext.Console.Write("Http service host started...");

            // Start Udp service host
            WsUdpServiceHost.Instance.Start(ctx);
            System.Ext.Console.Write("Udp service host started...");

            // Send hello greeting
            if (m_discoGreeting != null) m_discoGreeting.SendGreetingMessage(true);

            // Start the event subscription manager
            m_eventQManager = new DpwsWseEventSinkQMgr();
            m_eventQManager.Start();
            System.Ext.Console.Write("Event subscription manager started...");
        }

        /// <summary>
        /// Static method used to stop stack transport services.
        /// </summary>
        /// <remarks>
        /// This method stops the underlying stack transport services and the devices event queue manager.
        /// </remarks>
        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        public static void Stop()
        {
            if(!m_isStarted) throw new InvalidOperationException();

            // Send bye
            if (m_discoGreeting != null) m_discoGreeting.SendGreetingMessage(false);

            WsUdpServiceHost.Instance.Stop();
            System.Ext.Console.Write("Udp service host stopped...");

            m_eventQManager.Stop();
            System.Ext.Console.Write("Event subscription manager stopped...");
            m_httpServiceHost.Stop();
            System.Ext.Console.Write("Http service host stopped...");

            m_isStarted = false;

            // TODO: Use extended weak references to assure an incremental value even after boot
            m_AppSequence++;
        }

        /// <summary>
        /// Static property used to Get or Set a devices DPWS compliant Metadata version.
        /// </summary>
        /// <remarks>
        /// If a device changes any of it's relationship metadata, it Must increment the Metadata Version exposed
        /// in Hello, ProbeMatch and ResolveMatch soap messages. The device metadata relationship is considered to
        /// be static per metadata version.
        /// </remarks>
        public static int MetadataVersion
        {
            get
            {
                return m_MetadataVersion;
            }

            set
            {
                m_MetadataVersion = value;
            }
        }

        /// <summary>
        /// Static property containing a Dpws compliant endpoint address.
        /// </summary>
        /// <remarks>
        /// This stack supports urn:uuid:(guid) format addresses only.
        /// </remarks>
        public static string EndpointAddress
        {
            get
            {
                return m_address;
            }
        }

        /// <summary>
        /// Static property used to Get a devices DPWS compliant transport address.
        /// </summary>
        /// <remarks>
        /// The transport address format supportd by this stack is:
        /// http://(device ip address):(device port)/(device.address - urn:uuid prefix)
        /// </remarks>
        internal static string TransportAddress
        {
            get
            {
                return m_binding.Transport.EndpointAddress.AbsoluteUri;
            }
        }

        /// <summary>
        /// Static property used to Get the required WS-Discovery App Sequence value.
        /// </summary>
        /// <remarks>
        /// This is a soap header value and is required by WS-Discovery to facilitate ordering of Hello and
        /// Bye messages. This stack uses the App Sequence request and response messages. In addition to the
        /// Ws-Discovery Hello/Bye requirements this stack includes the AppSequence element in all WS-Discovery
        /// messages.
        /// </remarks>
        internal static String AppSequence
        {
            get
            {
                return m_AppSequence.ToString();
            }
        }

        /// <summary>
        /// Static property used to Get or Set the DPWS compliant MessageID value.
        /// </summary>
        /// <remarks>
        /// This is a soap header value and is required in DPWS compliant request and response messages.
        /// This stack uses this property to match a response with a request.
        /// This property is thread safe.
        /// </remarks>
        internal static String MessageID
        {
            get
            {
                return Interlocked.Increment(ref m_MessageID).ToString();
            }
        }

        internal static String SequenceID = null; //"urn:uuid:c883e4a8-9af4-4bf4-aaaf-06394151d6c0";

        /// <summary>
        /// Static property used to turn adhoc discovery on or off. Adhoc disco is optional with a Discovery Proxy.
        /// </summary>
        public static bool SupressAdhoc { get { return m_supressAdhoc; } set { m_supressAdhoc = value; } }

        /// <summary>
        /// Use to set the Discovery version
        /// </summary>
        public static ProtocolVersion DiscoveryVersion
        {
            get
            {
                return  (m_discoveryService != null) ? m_discoveryService.Version : null;
            }
        }
    
    }
}

