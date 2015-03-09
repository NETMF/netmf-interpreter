using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml;
using System.Threading;
using Dpws.Device;
using Dpws.Device.Services;
using Ws.Services;
using Ws.Services.Transport;
using Ws.Services.WsaAddressing;
using Ws.Services.Xml;
using Ws.Services.Discovery;
using Ws.Services.Faults;

using System.Ext;
using System.Ext.Xml;
using Ws.Services.Soap;

namespace Dpws.Device.Discovery
{

    // This hosted service class provides DPWS compliant Ws-Discovery Probe services.
    internal class DpwsDeviceDiscoService : DpwsHostedService
    {
        DpwsDeviceDiscovery m_discovery;

        // defined by http://www.iana.org/assignments/port-numbers as Web Services for Devices
        public const int DiscoveryLocalPort = 5357;

        //--//
        
        public DpwsDeviceDiscoService(ProtocolVersion v) : base(v)
        {
            string wsdNamespace = v.DiscoveryNamespace;

            // Set the service type name to internal to hide from discovery services
            ServiceTypeName = "Internal";

            // Set service namespace
            ServiceNamespace = new WsXmlNamespace(WsNamespacePrefix.Wsd, wsdNamespace);

            // Set endpoint address
            EndpointAddress = v.DiscoveryWellKnownAddress;

            // Add Discovery service operations
            ServiceOperations.Add(new WsServiceOperation(wsdNamespace, "Probe"));
            ServiceOperations.Add(new WsServiceOperation(wsdNamespace, "Resolve"));

            m_discovery = new DpwsDeviceDiscovery(v);
        }

        public ProtocolVersion Version 
        { 
            get
            { 
                return m_discovery.Version; 
            } 
        }

        // ProbeMatch response stub
        public WsMessage Probe(WsMessage msg)
        {
            // If Adhoc disco is turned off return null. Adhoc disco is optional with a Discovery Proxy
            if (Device.SupressAdhoc == true)
                return null;

            XmlReader   reader = msg.Reader;
            WsWsaHeader header = msg.Header;

            DpwsHostedService matchedService = null;

            string wsdNamespace = m_discovery.Version.DiscoveryNamespace;

            reader.ReadStartElement("Probe", wsdNamespace);

            bool match = true;
            bool fMatchAny = false;

            if (reader.IsStartElement("Types", wsdNamespace))
            {
                // use MoveToContent, ReadString, Read instead of ReadElementString because local namespaces are popped
                // from the stack on return.  
                reader.MoveToContent();

                // Look for specified type, send probe match if any instance of any of the listed types are found
                string[] typesList = reader.ReadString().Trim().Split(' ');

                int count = typesList.Length;
                match = false;

                if(count == 0 || ((count == 1) && (typesList[0] == "")))
                {
                    fMatchAny = true;
                }

                if (count > 0 && !fMatchAny)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string type = typesList[i];
                        // Parse type
                        string namespaceUri, prefix, typeName;
                        int namespaceIndex = type.IndexOf(':');
                        if (namespaceIndex == -1)
                        {
                            namespaceUri = "";
                            typeName = type;
                        }
                        else
                        {
                            if (namespaceIndex == type.Length - 1)
                                throw new XmlException();

                            prefix = type.Substring(0, namespaceIndex);
                            namespaceUri = reader.LookupNamespace(prefix);

                            if (namespaceUri == null)
                            {
                                namespaceUri = prefix;
                            }

                            typeName = type.Substring(namespaceIndex + 1);
                        }

                        // Check for the dpws standard type
                        if (namespaceUri == m_version.WsdpNamespaceUri && typeName == "Device")
                        {
                            match = true;
                            break;
                        }

                        // If there is a host check it
                        if (Device.Host != null)
                        {
                            if (Device.Host.ServiceNamespace.NamespaceURI == namespaceUri && Device.Host.ServiceTypeName == typeName)
                            {
                                match = true;
                                break;
                            }
                        }

                        // Check for matching service
                        int servicesCount = Device.HostedServices.Count;
                        DpwsHostedService hostedService;
                        for (i = 0; i < servicesCount; i++)
                        {
                            hostedService = (DpwsHostedService)Device.HostedServices[i];
                            // Skip internal services
                            if (hostedService.ServiceTypeName == "Internal")
                                continue;

                            if (hostedService.ServiceNamespace.NamespaceURI == namespaceUri &&
                                hostedService.ServiceTypeName == typeName)
                            {
                                match = true;
                                matchedService = hostedService;
                                break;
                            }
                        }

                        if(match) break;
                    }
                }
                reader.Read(); // read the EndElement or the empty StartElement
            }

            if (reader.IsStartElement("Scopes", wsdNamespace))
            {
                reader.MoveToContent();

                // Look for specified scope.  We do not support scopes at this point, so
                // any scope will result in no matches
                // TODO: Add support for scopes?
                string[] scopeList = reader.ReadString().Trim().Split(' ');

                int count = scopeList.Length;

                if (count == 1 && scopeList[0] == "")
                {
                    count = 0;
                }

                if (count > 0)
                {
                    match = false;
                    fMatchAny = false;
                }

                reader.Read(); // read the EndElement or the empty StartElement
            }

            // For completeness sake
            // We don't care about the rest...
            XmlReaderHelper.SkipAllSiblings(reader);
            reader.ReadEndElement(); // Probe

            if (match || fMatchAny)
            {
                return m_discovery.ProbeMatch(msg, matchedService);
            }

            return null;
        }

        // ResolveMatch response stub
        public WsMessage Resolve(WsMessage message)
        {
            // If Adhoc disco is turned off return null. Adhoc disco is optional with a Discovery Proxy
            if (Device.SupressAdhoc == true)
                return null;

            return m_discovery.ResolveMatch(message);
        }
    }


    /// <summary>
    /// Discovery client used to send managed Hello and Bye messages
    /// </summary>
    public class ManagedDiscovery
    {
        /// <summary>
        /// Method used to send an http Hello message to specified endpoint. Use for managed discovery.
        /// </summary>
        /// <param name="endpointAddress">A string containing the endpoint address of a listening client.</param>
        public void DirectedHello(string endpointAddress, DpwsDiscoGreeting greeting, ProtocolVersion version)
        {
            WsMessage greetingsMessage = greeting.BuildHelloMessage(endpointAddress, null, null);
            Ws.Services.Transport.HTTP.WsHttpClient httpClient = new Ws.Services.Transport.HTTP.WsHttpClient(version);
            httpClient.SendRequestOneWay(greetingsMessage, new Uri(endpointAddress));
        }

        /// <summary>
        /// Method used to send an http Bye message to specified endpoint. Use for managed discovery.
        /// </summary>
        /// <param name="endpointAddress">A string containing the endpoint address of a listening client.</param>
        public void DirectedBye(string endpointAddress, DpwsDiscoGreeting greeting, ProtocolVersion version)
        {
            WsMessage greetingsMessage = greeting.BuildByeMessage(endpointAddress, null, null);
            Ws.Services.Transport.HTTP.WsHttpClient httpClient = new Ws.Services.Transport.HTTP.WsHttpClient(version);
            httpClient.SendRequestOneWay(greetingsMessage, new Uri(endpointAddress));
        }
    }
    
    /// <summary>
    /// Discovery client used to send Hello and Bye messages
    /// </summary>
    public class DpwsDiscoGreeting
    {
        private const int MulticastUdpRepeat = 4;
        private const int UdpUpperDelay = 500;
        private const int UdpMinDelay = 50;
        private const int UdpMaxDelay = 250;

        //--//

        private ProtocolVersion m_version;

        //--//
        
        public DpwsDiscoGreeting(ProtocolVersion v)
        {
            m_version = v;
        }
        
        /// <summary>
        /// Method used to send Hello and Bye messages
        /// </summary>
        /// <param name="greetingType">An integer representing the type of greeting 0 = Hello, 1 = Bye.</param>
        public void SendGreetingMessage(bool isHello)
        {
            // If Adhoc disco is turned off return. Adhoc disco is optional with a Discovery Proxy
            if (Device.SupressAdhoc == true)
                return;

            WsMessage greetingsMessage = null;
            if (isHello)
                greetingsMessage = BuildHelloMessage(Device.DiscoveryVersion.DiscoveryWellKnownAddress, null, null);
            else
                greetingsMessage = BuildByeMessage(Device.DiscoveryVersion.DiscoveryWellKnownAddress, null, null);
            
            byte[] greeting = (byte[])greetingsMessage.Body;
            
            System.Ext.Console.Write("UDP (" + (isHello ? "Hello" : "Bye") +  ") multicast to: " + WsDiscovery.WsDiscoveryEndPoint.ToString());
            System.Ext.Console.Write(greetingsMessage.Body as byte[]);
            
            // Create a UdpClient used to send Hello and Bye messages
            using(Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                // Very important - Set default multicast interface for the underlying socket
                udpClient.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)WsNetworkServices.GetLocalIPV4AddressValue());

                // Random back off implemented as per soap over udp specification
                // for unreliable multicast message exchange
                Random rand = new Random();
                int backoff = 0;
                for (int i = 0; i < MulticastUdpRepeat; ++i)
                {
                    if (i == 0)
                    {
                        backoff = rand.Next(UdpMaxDelay - UdpMinDelay) + UdpMinDelay;
                    }
                    else
                    {
                        backoff = backoff * 2;
                        backoff = backoff > UdpUpperDelay ? UdpUpperDelay : backoff;
                    }

                    udpClient.SendTo(greeting, greeting.Length, SocketFlags.None, WsDiscovery.WsDiscoveryEndPoint);
                    Thread.Sleep(backoff);
                }
            }            
        }

        internal WsMessage BuildHelloMessage(string endpointAddress, WsWsaHeader header, XmlReader reader)
        {
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsXmlNamespaces additionalPrefixes = null;
                // If a Host exist write the Host namespace
                if (Device.Host != null)
                {
                    additionalPrefixes = new WsXmlNamespaces();
                    additionalPrefixes.Add(Device.Host.ServiceNamespace);
                }

                WsWsaHeader helloHeader = new WsWsaHeader(
                    m_version.DiscoveryNamespace + "/Hello",    // Action
                    null,                                       // RelatesTo
                    endpointAddress,                            // To
                    null, null, null);                          // ReplyTo, From, Any

                WsMessage msg = new WsMessage(helloHeader, null, WsPrefix.Wsd | WsPrefix.Wsdp, additionalPrefixes, 
                    new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID));

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                smw.WriteSoapMessageStart(xmlWriter, msg);

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "Hello", null);

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "EndpointReference", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                xmlWriter.WriteString(Device.EndpointAddress);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                // Write hosted service types
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "Types", null);
                WriteDeviceServiceTypes(xmlWriter, false);
                xmlWriter.WriteEndElement(); // End Types

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "XAddrs", null);
                xmlWriter.WriteString(Device.TransportAddress);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "MetadataVersion", null);
                xmlWriter.WriteString(Device.MetadataVersion.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();

                // Flush and close writer. Return stream buffer
                smw.WriteSoapMessageEnd(xmlWriter);

                msg.Body = xmlWriter.ToArray();

                return msg;
            }
        }

        // Write Service Types list. Optionally add hosted service to the list
        private void WriteDeviceServiceTypes(XmlWriter xmlWriter, bool includeHostedServices)
        {
            // Write the default p:Device type
            xmlWriter.WriteString(WsNamespacePrefix.Wsdp + ":Device");

            // If device has a Host service add the Host service type
            if (Device.Host != null)
            {
                DpwsHostedService hostedService = Device.Host;

                // Add a space delimiter
                xmlWriter.WriteString(" ");

                string serviceType = (string)hostedService.ServiceNamespace.Prefix + ":" + hostedService.ServiceTypeName;
                xmlWriter.WriteString(serviceType);
            }

            // Add hosted service to list if required
            if (includeHostedServices)
                WriteHostedServiceTypes(xmlWriter);

        }

        // Write Hosted Service Types
        private void WriteHostedServiceTypes(XmlWriter xmlWriter)
        {

            // Step through list of hosted services and add types to list
            int count = Device.HostedServices.Count;
            DpwsHostedService hostedService;
            String typesString = string.Empty;
            for (int i = 0; i < count; i++)
            {
                hostedService = (DpwsHostedService)Device.HostedServices[i];

                // Skip internal services
                if (hostedService.ServiceTypeName == "Internal")
                    continue;
                typesString += hostedService.ServiceNamespace.Prefix + ":" + hostedService.ServiceTypeName + " ";
            }

            xmlWriter.WriteString(typesString);
        }

        // Build ws-discovery bye message
        internal WsMessage BuildByeMessage(string endpointAddress, WsWsaHeader header, XmlReader reader)
        {
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsWsaHeader byeHeader = new WsWsaHeader(
                    m_version.DiscoveryNamespace + "/Bye",  // Action
                    null,                                   // RelatesTo
                    endpointAddress,                        // To
                    null, null, null);                      // ReplyTo, From, Any

                WsMessage msg = new WsMessage(byeHeader, null, WsPrefix.Wsd, null, 
                    new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID));

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                smw.WriteSoapMessageStart(xmlWriter, msg);

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "Bye", null);

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "EndpointReference", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                xmlWriter.WriteString(Device.EndpointAddress);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();

                smw.WriteSoapMessageEnd(xmlWriter);

                msg.Body = xmlWriter.ToArray();

                // Return stream buffer
                return msg;
            }
        }
    }
}

