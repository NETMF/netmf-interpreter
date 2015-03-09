using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Dpws.Device.Services;
using Ws.Services;
using Ws.Services.Faults;
using Ws.Services.Soap;
using Ws.Services.Utilities;
using Ws.Services.WsaAddressing;

using System.Ext;
using System.IO;
using System.Xml;
using Ws.Services.Xml;
using System.Ext.Xml;
using Microsoft.SPOT;
using Ws.Services.Encoding;

namespace Dpws.Device.Services
{
    /// <summary>
    /// This class implements a set of base methods and properties used by the stack to dispatch method
    /// calls and discover hosted service endpoints, operations and events. A Device developer must derive
    /// their hosted service class(es) from this base class.
    /// </summary>
    /// <remarks>
    /// A default namespace and perfix are provided. Set the EndpointAddress property to change default values.
    /// </remarks>
    public class DpwsHostedService : IWsServiceEndpoint
    {
        // Fields
        private object              m_threadLock;
        private string              m_serviceID;
        private string              m_endpointAddress;
        private WsWsaEndpointRefs   m_endpointRefs;
        private WsServiceOperations m_serviceOperations;
        private DpwsWseEventSources m_eventSources;
        private string              m_serviceTypeName;
        private WsXmlNamespace      m_serviceNamespace;
        private static int          m_prefixCounter;
        private bool                m_blockingCall;

        //--//

        protected readonly ProtocolVersion m_version;

        //--//

        /// <summary>
        /// Creates and instance of the HostedService class.
        /// </summary>
        public DpwsHostedService(ProtocolVersion v)
        {
            m_threadLock        = new object();
            m_endpointRefs      = new WsWsaEndpointRefs();
            m_serviceOperations = new WsServiceOperations();
            m_eventSources      = new DpwsWseEventSources();
            m_prefixCounter     = 0;
            m_blockingCall      = true;
            m_version           = v;
        }
        
        /// <summary>
        /// Adds eventing services operations to the Hosted Service
        /// This should only should be called for DPWS hosted services that implement events sources
        /// </summary>
        public void AddEventServices()
        {
            this.ServiceOperations.Add(new WsServiceOperation(m_version.EventingNamespace, "Subscribe"));
            this.ServiceOperations.Add(new WsServiceOperation(m_version.EventingNamespace, "Unsubscribe"));
            this.ServiceOperations.Add(new WsServiceOperation(m_version.EventingNamespace, "GetStatus"));
            this.ServiceOperations.Add(new WsServiceOperation(m_version.EventingNamespace, "Renew"));            
        }

        /// <summary>
        /// Use to Get or Set a collection of endpoint references specific to a device implementation.
        /// </summary>
        /// <remarks>
        /// This property is used by a devices internal services like Discovery and Eventing to ease in the
        /// processing of service endpoint information.
        /// </remarks>
        public WsWsaEndpointRefs EndpointRefs { get { return m_endpointRefs; } }

        /// <summary>
        /// Use to Get or Set a collection used to store Hosted Services operations parameters like Action and namespace.
        /// </summary>
        public WsServiceOperations ServiceOperations { get { return m_serviceOperations; } }

        /// <summary>
        /// Use to Get or Set a collection used to store Event Source parameters like event source name and namespace.
        /// </summary>
        /// <remarks>
        /// An Event Source is used to expose event source endpoints implemented by a hosted service. A DPWS client
        /// application can subscribe to events exposed as event sources. Each hosted service event capable
        /// of firing a message to a client must store an event source in this collection. A device developer creates
        /// and Event Source object for an event and used the Add method of this object to add the event source
        /// to the hosted services event sources collectino. See DpwsWseEventSources for detials on the Add method.
        /// Each event source entry contains a collection of event sinks that have an active subsrciption to an event.
        /// Event Sinks contain information about a clients endpoint.
        /// </remarks>
        public DpwsWseEventSources EventSources { get { return m_eventSources; } set { m_eventSources = value; } }

        /// <summary>
        /// Use to Get or Set a string representing the name of a hosted service.
        /// </summary>
        /// <remarks>
        /// This name represents the type(object) name of this hosted service. This name is used by the
        /// device stacks discovery services to provide a type name presented
        /// in a ProbeMatch or Metadata Get response.
        /// </remarks>
        public string ServiceTypeName
        {
            get
            {
                // Set default service type name
                if (m_serviceTypeName == null)
                {
                    m_serviceTypeName = GetType().Name;
                }

                return m_serviceTypeName;
            }

            set
            {
                m_serviceTypeName = value;
            }
        }

        /// <summary>
        /// Use to Get or Set the namespace for a HostedService.
        /// </summary>
        /// <remarks>
        /// It is strongly recommeded that a device developer sets this namespace however if a namespace
        /// is not set a default namespace is generated. The default namespace will have a prefix of S(x) and
        /// the namespace URI will be generated using the name of derived hosted service class.
        /// </remarks>
        public WsXmlNamespace ServiceNamespace
        {
            get
            {
                if (m_serviceNamespace == null)
                {
                    // Set default service namespace
                    // Special Namespace process required because of lack of Types.Namespace support on Microframework
                    string tempNS = GetType().FullName;
                    char[] ns = new char[tempNS.Length];
                    int index = tempNS.LastIndexOf('.');
                    for (int i = 0; i < index; ++i)
                        ns[i] = tempNS[i] == '.' ? '/' : tempNS[i];
                    tempNS = new string(ns);

                    m_serviceNamespace = new WsXmlNamespace("S" + (++m_prefixCounter).ToString(), "http://" + tempNS);
                }

                return m_serviceNamespace;
            }

            set
            {
                if (value != null && m_serviceNamespace == null)
                {
                    m_serviceNamespace = value;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        /// <summary>
        /// Use to Get or Set a unique identifier for the hosted services.
        /// </summary>
        /// <remarks>
        /// This is the unique identifier for a hosted service which must be persisted across
        /// re-initialization and must be shared across multiple hosted elements.
        /// A hosted service ID must be unique within a device. This value will be compared directly
        /// as a case sensitive string. This stack supports urn:uuid:GUID values for service id only.
        /// A device developer must set this property. This property is sent is a Get metadata response
        /// and used by a DPWS client to identify a specific service within a device.
        /// </remarks>
        public string ServiceID
        {
            get { return m_serviceID; }
            set
            {
                // Knock out some abstraction and force this to be a urn:uuid
                if (!WsUtilities.ValidateUrnUuid(value))
                    throw new ArgumentException();

                m_serviceID = value;

                // If address is not yet set default to ServiceID for convenience
                if (m_endpointAddress == null)
                    EndpointAddress = m_serviceID;
            }
        }

        /// <summary>
        /// The unique endpoint address used by the HTTP transport to access this hosted service.
        /// </summary>
        /// <remarks>
        /// This property makes up the unique portion of a hosted service endpoint address. The stack automatically
        /// gererates http transport endpoint addresses using this property. The generated endpoint address
        /// will be used by DPWS clients to call this hosted service. In a directed soap request the http
        /// transport that incorporates this address will be sent in the header.To property. The transport
        /// dispatcher uses the http transport address to dispatch a request to the intended hosted service.
        /// By default this property is set to the ServiceID. It is highly recommended you do not set this
        /// value manually. By default the stack generates a unique http transport address using the following
        /// format: HTTP://(device ip address):(deviceport):/(SericeAddress). If you set this property the
        /// stack will use your address in place of the (ServiceAddress) portion. If you set it make sure
        /// you know what you intend to do or your request will not make it to the service. When this property
        /// is set an endpoint reference is created and added to the endpoint references collection for this
        /// service. See AddEndpoints for more information.
        /// </remarks>
        public string EndpointAddress
        {
            get
            {
                return m_endpointAddress;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                if(!Uri.IsWellFormedUriString(value, UriKind.Absolute))
                {
                    throw new ArgumentException();
                }

                m_endpointAddress = value;
                AddEndpoints(m_endpointAddress);
            }
        }

        /// <summary>
        /// Adds an endpoint reference to the hosted services collection.
        /// This endpoint refernce will be used by the devices discovery services to expose the endpoint
        /// of this service.
        /// </summary>
        /// <param name="serviceAddress">A string containing the urn:uuid that uniquely defines this service.</param>
        private void AddEndpoints(string serviceAddress)
        {
            EndpointRefs.Clear();

            // Create IPV4 endpoint
            if (Device.IPV4Address != null)
            {
                EndpointRefs.Add(new WsWsaEndpointRef(new Uri("http://" + Device.IPV4Address + ":" + Device.Port + "/" + serviceAddress.Substring(9))));
            }
        }

        /// <summary>
        /// Property that determins if the ProcessRequest call blocks.
        /// </summary>
        bool IWsServiceEndpoint.BlockingCall { get { return m_blockingCall; } set { m_blockingCall = value; } }

        /// <summary>
        /// A wrapper method that implements the ProcessRequest interface used to control blocking of request.
        /// Standard stack services are static and do not require blocking. Hosted service are blocked.
        /// </summary>
        /// <param name="header">A WsWsaHeader containing the header proerties of a request.</param>
        /// <param name="envelope">A WsXmlDocument containing the entire envelope of the message in a node tree.</param>
        /// <returns>A byte array containing a soap response to the request.</returns>
        WsMessage IWsServiceEndpoint.ProcessRequest(WsMessage request)
        {
            // This allow static services like Discovery to run unblocked
            if (m_blockingCall == true)
            {
                lock (m_threadLock)
                {
                    return ProcessRequest(request);
                }
            }
            else
            {
                return ProcessRequest(request);
            }
        }
        
        /// <summary>
        /// Implements the IWsServiceEndpoint.ProcessRequest method.
        /// Gets a service operation based on the action property sent in the soap header and calls
        /// invoke on the service method.
        /// </summary>
        /// <param name="header">A WsWsaHeader containing the header proerties of a request.</param>
        /// <param name="envelope">A WsXmlDocument containing the entire envelope of the message in a node tree.</param>
        /// <returns>A byte array containing a soap response to the request.</returns>
        WsMessage ProcessRequest(WsMessage request)
        {
            WsWsaHeader header = request.Header;
            String      action = header.Action;

            // Display the action name.
            System.Ext.Console.Write("Action: " + action);

            // Look for a valid service Action
            WsServiceOperation operation = m_serviceOperations[action];
            if (operation != null)
            {
                System.Reflection.MethodInfo methodInfo = this.GetType().GetMethod(operation.MethodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                if (methodInfo == null)
                {
                    System.Ext.Console.Write("");
                    System.Ext.Console.Write("Operation method for action " + header.Action + " was not found");
                    System.Ext.Console.Write("");
                    
                    throw new WsFaultException(header, WsFaultType.WsaDestinationUnreachable);
                }

                return (WsMessage)methodInfo.Invoke(this, new object[] { request });
            }

            // If a valid Action is not found, fault
            throw new WsFaultException(header, WsFaultType.WsaDestinationUnreachable, "To: " + header.To + " Action: " + action);
        }

        /// <summary>
        /// Eventing Subscribe stub.
        /// </summary>
        /// <param name="header">Header object.</param>
        /// <param name="reader">An XmlReader positioned at the begining of the Subscribe request body element.</param>
        /// <returns>Byte array containing a Subscribe response.</returns>
        /// <remarks>This method is used by the stack framework. Do not use this method.</remarks>
        public WsMessage Subscribe(WsMessage msg)
        {
            // Call SubscriptionManagers global eventing method
            return Device.SubscriptionManager.Subscribe(msg.Header, msg.Reader, Device.HostedServices);
        }

        /// <summary>
        /// Eventing UnSubscribe stub.
        /// </summary>
        /// <param name="header">Header object.</param>
        /// <param name="reader">An XmlReader positioned at the begining of the Unsubscribe request body element.</param>
        /// <returns>Byte array containing an UnSubscribe response.</returns>
        /// <remarks>This method is used by the stack framework. Do not use this method.</remarks>
        public WsMessage Unsubscribe(WsMessage msg)
        {
            // Call SubscriptionManagers global eventing method
            return Device.SubscriptionManager.Unsubscribe(msg.Header, msg.Reader, Device.HostedServices);
        }

        /// <summary>
        /// Event renew stub.
        /// </summary>
        /// <param name="header">Header object.</param>
        /// <param name="reader">An XmlReader positioned at the begining of the Renew request body element.</param>
        /// <returns>Byte array containing an Renew response.</returns>
        /// <remarks>This method is used by the stack framework. Do not use this method.</remarks>
        public WsMessage Renew(WsMessage msg)
        {
            // Call SubscriptionManagers global eventing method
            return Device.SubscriptionManager.Renew(msg.Header, msg.Reader, Device.HostedServices);
        }

        /// <summary>
        /// Event GetStatus stub.
        /// </summary>
        /// <param name="header">Header object.</param>
        /// <param name="reader">An XmlReader positioned at the begining of the GetStatus request body element.</param>
        /// <returns>Byte array containing an GetStatus response.</returns>
        /// <remarks>This method is used by the stack framework. Do not use this method.</remarks>
        public WsMessage GetStatus(WsMessage msg)
        {
            // Call SubscriptionManagers global eventing method
            return Device.SubscriptionManager.GetStatus(msg.Header, msg.Reader, Device.HostedServices);
        }

        private WsMessage GetStatusResponse(WsWsaHeader header, long newDuration)
        {
            using (XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsWsaHeader responseHeader = new WsWsaHeader(
                    WsWellKnownUri.WseNamespaceUri + "/RenewResponse",      // Action
                    header.MessageID,                                       // RelatesTo
                    header.ReplyTo.Address.AbsoluteUri,                     // To
                    null, null, null);                                      // ReplyTo, From, Any

                WsMessage msg = new WsMessage(responseHeader, null, WsPrefix.Wse,
                    null, new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID)); // AppSequence

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "Expires", null);
                xmlWriter.WriteString(new WsDuration(newDuration).DurationString);
                xmlWriter.WriteEndElement(); // End Expires

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                smw.WriteSoapMessageEnd(xmlWriter);

                msg.Body = xmlWriter.ToArray();

                return msg;
            }
        }
    }
}


