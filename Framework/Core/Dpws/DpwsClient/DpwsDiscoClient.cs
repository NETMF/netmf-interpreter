using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Ws.Services.Transport;
using Ws.Services.Transport.HTTP;
using Ws.Services.Utilities;
using Ws.Services.Soap;
using Ws.Services.Xml;
using Ws.Services.WsaAddressing;
using Ws.Services.Discovery;


using System.Ext;
using System.IO;
using System.Ext.Xml;
using System.Xml;
using Ws.Services;
using Microsoft.SPOT;
using Ws.Services.Binding;

namespace Dpws.Client.Discovery
{
    /// <summary>
    /// A class used to process client Probe and Resolve request.
    /// </summary>
    public class DpwsDiscoveryClient
    {
        private int        m_discoResponsePort;
        private int        m_receiveTimeout;
        private Random     m_random;
        private DpwsClient m_parent;

        private const int MulticastUdpRepeat = 4;
        private const int UdpUpperDelay      = 3000;
        private const int UdpMinDelay        = 50;
        private const int UdpMaxDelay        = 250;

        private readonly ProtocolVersion m_version;

        /// <summary>
        /// Creates and instance of the DpwsDiscoveryClient class.
        /// </summary>
        internal DpwsDiscoveryClient(DpwsClient parent, ProtocolVersion v)
        {
            // Discovery Port defined by http://www.iana.org/assignments/port-numbers as Web Services for Devices
            m_discoResponsePort = 5357;
            m_receiveTimeout    = 5000;
            m_version = v;
            m_parent = parent;
            m_random = new Random();
        }

        /// <summary>
        /// Use to get or set the UDP discovery port that this client listens for discovery responses on.
        /// </summary>
        public int DiscoveryResponsePort { get { return m_discoResponsePort; } set { m_discoResponsePort = value; } }

        /// <summary>
        /// Use to get or set the time in milliseconds that a probe or resolve listener should wait for probe matches.
        /// </summary>
        public int ReceiveTimeout { get { return m_receiveTimeout; } set { m_receiveTimeout = value; } }

        /// <summary>
        /// Use to set the Discovery version
        /// </summary>
        public ProtocolVersion DiscoVersion
        {
            get
            {
                return m_version;
            }
        }

        /// <summary>
        /// Sends a directed Probe request and parses ProbeMatches responses.
        /// </summary>
        /// <param name="endpointAddress">
        /// A string containing a Dpws devices transport endpoint address.
        /// For example: http://192.168.0.1:8084/3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <param name="targetServiceAddress">
        /// A string containing the target service address to probe for.
        /// For example: urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <param name="filters">
        /// A DpwsServiceTypes object containing a collection of types a service must support to signal a match.
        /// Null = any type.
        /// </param>
        /// <remarks>
        /// A directed Probe is used to discover services on a network. The DirectedProbe method sends a
        /// HTTP request to the service specified endpointAddress parameter. A service at the endpoint that
        /// implements matching types specified in the filters parameter should respond with a ProbeMatches
        /// message. The ProbeMatches mesgage is returned as the response to the DirectedProbe request.
        /// If a null filter is supplied any Dpws complient service should reply with a ProbeMatches reponse.
        /// This method is used to directly ask for service endpoint information.
        /// </remarks>
        /// <returns>
        /// A collection of ProbeMatches objects.  A ProbeMatch object contains endpoint details used
        /// used to locate the actual service on a network and the types supported by the service.
        /// </returns>
        /// <exception cref="InvalidOperationException">If a fault response is received.</exception>
        public DpwsServiceDescriptions DirectedProbe(string endpointAddress, string targetServiceAddress, DpwsServiceTypes filters)
        {
            // Build the probe request message
            string messageID = null;
            WsMessage probeRequest = BuildProbeRequest(targetServiceAddress, filters, out messageID);

            System.Ext.Console.Write("");
            System.Ext.Console.Write("Sending DirectedProbe:");
            System.Ext.Console.Write(probeRequest.Body as byte[]);
            System.Ext.Console.Write("");

            // Create an http client and send the probe request. Use a WspHttpClient to get an array response.
            WsHttpClient httpClient = new WsHttpClient(m_version);

            WsMessage probeResponse = httpClient.SendRequest(probeRequest, new Uri(endpointAddress));

            // Build probe matches collection
            DpwsServiceDescriptions probeMatches = new DpwsServiceDescriptions();
            DpwsDiscoClientProcessor soapProcessor = new DpwsDiscoClientProcessor(m_version);
            if (probeResponse != null)
            {
                System.Ext.Console.Write("");
                System.Ext.Console.Write("ProbeMatches Response From: " + endpointAddress);
                System.Ext.Console.Write(probeResponse.Body as byte[]);

                try
                {
                    DpwsServiceDescriptions tempMatches = soapProcessor.ProcessProbeMatch(probeResponse, messageID, null, null);
                    if (tempMatches != null)
                    {
                        int count = tempMatches.Count;
                        for (int i = 0; i < count; i++)
                        {
                            probeMatches.Add(tempMatches[i]);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Ext.Console.Write("");
                    System.Ext.Console.Write(e.Message);
                    System.Ext.Console.Write("");
                }
            }

            if (probeMatches.Count == 0)
                return null;
            else
                return probeMatches;
        }

        /// <summary>
        /// Sends a directed Resolve request and parses ResolveMatch response.
        /// </summary>
        /// <param name="endpointAddress">
        /// A string containing a Dpws devices transport endpoint address.
        /// For example: http://192.168.0.1:8084/3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <param name="serviceAddress">
        /// A string containing the address of a service that will handle the resolve request.
        /// For example: urn:uuid:2bcdd1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <param name="targetServiceAddress">
        /// A string containing the address of a service that can process the resolve request.
        /// For example: urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <remarks>
        /// A Resolve is used to resolve the transport address of a know service. The request contains a service
        /// address aquired from configuration or a previous Resolve or Metadata Get request.
        /// The DirectedResolve method sends a Http request to the specified endpoint address.
        /// If the endpoint contains a device with this address receives the request, it must send a unicast ResolveMatches
        /// response back to the client that made the request.  
        /// </remarks>
        /// <returns>
        /// A collection of ResolveMatches objects. A ResolveMatch object contains endpoint details used
        /// used to locate the actual service on a network and the types supported by the service. 
        /// </returns>
        public DpwsServiceDescription DirectedResolve(string endpointAddress, string serviceAddress, string targetServiceAddress)
        {
            String messageID = "";
            WsMessage resolveRequest = BuildResolveRequest(targetServiceAddress, serviceAddress, ref messageID);

            if (resolveRequest == null)
                return null;

            System.Ext.Console.Write("");
            System.Ext.Console.Write("Sending Resolve:");
            System.Ext.Console.Write(resolveRequest.Body as byte[]);

            // Create an http client and send the resolve request. Use a WspHttpClient to get an array response.
            WsHttpClient httpClient = new WsHttpClient(m_version);

            WsMessage resolveResponse = httpClient.SendRequest(resolveRequest, new Uri(endpointAddress));

            DpwsDiscoClientProcessor soapProcessor = new DpwsDiscoClientProcessor(m_version);
            DpwsServiceDescription resolveMatch = soapProcessor.ProcessResolveMatch(resolveResponse, messageID, null, null);
            return resolveMatch;
        }

        /// <summary>
        /// Sends a Probe request and parses ProbeMatches responses.
        /// </summary>
        /// <param name="filters">
        /// A DpwsServiceTypes object containing a collection of types a service must support to signal a match.
        /// Null = any type.
        /// </param>
        /// <remarks>
        /// A Probe is used to discover services on a network. The Probe method sends a UDP request to the
        /// Dpws multicast address, 239.255.255.250:3702. Any service that implements types specified in the
        /// filters parameter should respond with a ProbeMatches message. The ProbeMatches mesgage is unicast
        /// back to the that client that made the request. If a null filter is supplied any Dpws complient
        /// service should reply with a ProbeMatches reponse. Probe waits DpwsDiscoveryClient.ReceiveTimout
        /// for probe matches.
        /// </remarks>
        /// <returns>
        /// A collection of ProbeMatches objects.  A ProbeMatch object contains endpoint details used
        /// used to locate the actual service on a network and the types supported by the service.
        /// </returns>
        public DpwsServiceDescriptions Probe(DpwsServiceTypes filters)
        {
            return Probe(filters, 5, m_receiveTimeout);
        }

        /// <summary>
        /// Sends a Probe request and parses ProbeMatches responses.
        /// </summary>
        /// <param name="filters">
        /// A DpwsServiceTypes object containing a collection of types a service must support to signal a match.
        /// Null = any type.
        /// </param>
        /// <param name="maxProbeMatches">
        /// An integer representing the maximum number of matches to reveive within the timout period. Pass 0 to receive
        /// as many matches as possible before the timeout expires.
        /// </param>
        /// <param name="timeout">
        /// An integer specifying a request timeout in milliseconds. Pass -1 to wait ReceiveTimeout.
        /// </param>
        /// <remarks>
        /// A Probe is used to discover services on a network. The Probe method sends a UDP request to the
        /// Dpws multicast address, 239.255.255.250:3702. Any service that implements types specified in the
        /// filters parameter should respond with a ProbeMatches message. The ProbeMatches mesgage is unicast
        /// back to the that client that made the request. If a null filter is supplied any Dpws complient
        /// service should reply with a ProbeMatches reponse. Probe waits DpwsDiceoveryCleint.ReceiveTimout
        /// for probe matches.
        /// </remarks>
        /// <returns>
        /// A collection of ProbeMatches objects.  A ProbeMatch object contains endpoint details used
        /// used to locate the actual service on a network and the types supported by the service.
        /// </returns>
        public DpwsServiceDescriptions Probe(DpwsServiceTypes filters, int maxProbeMatches, int timeout)
        {
            // Build the probe request message
            WsMessageCheck messageCheck = new WsMessageCheck();
            string messageID = null;
            WsMessage probeRequest = BuildProbeRequest(DiscoVersion.DiscoveryWellKnownAddress, filters, out messageID);
            DpwsServiceDescriptions probeMatches = new DpwsServiceDescriptions();

            System.Ext.Console.Write("");
            System.Ext.Console.Write("Sending Probe:");
            System.Ext.Console.Write(probeRequest.Body as byte[]);
            System.Ext.Console.Write("");

            // Create a new UdpClient
            using(Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                // Very important - Set default multicast interface for the underlying socket

                uint       ipLocal = WsNetworkServices.GetLocalIPV4AddressValue();
                IPAddress  localIP = IPAddress.Parse(WsNetworkServices.GetLocalIPV4Address());
                IPEndPoint localEP = new IPEndPoint(localIP, 0);
                
                udpClient.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)ipLocal);
                udpClient.ReceiveTimeout = timeout;

                udpClient.Bind(localEP);

                // Random back off implemented as per soap over udp specification
                // for unreliable multicast message exchange
                Thread th = SendWithBackoff((byte[])probeRequest.Body, udpClient);

                // Create probe matches collection and set expiration loop timer
                byte[] probeResponse = new byte[c_MaxUdpPacketSize];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int responseLength;

                DpwsDiscoClientProcessor soapProcessor = new DpwsDiscoClientProcessor(m_version);

                while (true)
                {
                    // Since MF sockets does not have an IOControl method catch 10054 to get around the problem
                    // with Upd and ICMP.
                    try
                    {
                        // Wait for response
                        responseLength = udpClient.ReceiveFrom(probeResponse, c_MaxUdpPacketSize, SocketFlags.None, ref remoteEP);
                    }
                    catch (SocketException se)
                    {
                        if ((SocketError)se.ErrorCode == SocketError.ConnectionReset)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        // Timeout
                        if ((SocketError)se.ErrorCode == SocketError.TimedOut)
                            break;

                        throw se;
                    }

                    // If we received process probe match
                    if (responseLength > 0)
                    {
                        System.Ext.Console.Write("");
                        System.Ext.Console.Write("ProbeMatches Response From: " + ((IPEndPoint)remoteEP).Address.ToString());
                        System.Ext.Console.Write(probeResponse);

                        // Process the response
                        try
                        {
                            WsWsaHeader header = new WsWsaHeader();
                            
                            XmlReader reader = WsSoapMessageParser.ParseSoapMessage(probeResponse, ref header, m_version);
                            
                            WsMessage msg = new WsMessage(header, null, WsPrefix.Wsdp);

                            msg.Reader = reader;

                            DpwsServiceDescriptions tempMatches = soapProcessor.ProcessProbeMatch(msg, messageID, (IPEndPoint)remoteEP, messageCheck);
                            if (tempMatches != null)
                            {
                                int count = maxProbeMatches < tempMatches.Count ? maxProbeMatches : tempMatches.Count;

                                for (int i = 0; i < count; i++)
                                {
                                    probeMatches.Add(tempMatches[i]);
                                }
                                maxProbeMatches -= count;
                            }
                        }
                        catch (Exception e)
                        {
                            System.Ext.Console.Write("");
                            System.Ext.Console.Write(e.Message);
                            System.Ext.Console.Write("");
                        }

                        // If maxProbeRequest is set check count
                        if (maxProbeMatches <= 0)
                        {
                            break;
                        }
                        
                    }
                }

                th.Join();
            }

            // Display results
            if (probeMatches.Count == 0)
                System.Ext.Console.Write("Probe timed out.");
            else
                System.Ext.Console.Write("Received " + probeMatches.Count + " probeMatches matches.");

            return probeMatches;
        }

        /// <summary>
        /// Builds a probe request message based on the filters parameter.
        /// </summary>
        /// <param name="serviceAddress">
        /// A string containing the target service address.
        /// For example: urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <param name="filters">
        /// A DpwsServiceTypes object containing a collection of types a service must support to signal a match.
        /// Null = any type.
        /// </param>
        /// <param name="messageID">
        /// A string used to return the messageID assigned to this message.
        /// </param>
        /// <returns>A byte array containing the probe message or null if an error occures.</returns>
        private WsMessage BuildProbeRequest(string serviceAddress, DpwsServiceTypes filters, out String messageID)
        {
            // Performance debugging
            DebugTiming timeDebuger = new DebugTiming();
            long startTime = timeDebuger.ResetStartTime("");
            WsMessage msg = null;

            // Build Probe request
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsWsaHeader header = new WsWsaHeader(
                    m_version.DiscoveryNamespace + "/Probe",      // Action
                    null,                                         // RelatesTo
                    serviceAddress,                               // To
                    (m_version is ProtocolVersion11?  m_version.AnonymousUri : null), null, null);          // ReplyTo, From, Any

                header.MustUnderstand = true;

                // If filters are supplied, write filter namespaces if prefixed. Build filter list for use later
                WsXmlNamespaces namespaces = new WsXmlNamespaces();

                // Prefix hack for now:
                int i = 0;
                string prefix;
                string filterList = "";
                bool spaceFlag = false;
                if (filters != null)
                {
                    int count = filters.Count;
                    for (int j = 0; j < count; j++)
                    {
                        DpwsServiceType serviceType = filters[j];
                        prefix = namespaces.LookupPrefix(serviceType.NamespaceUri);
                        if (prefix == null)
                        {
                            prefix = "dp" + (i++);
                            namespaces.Add(new WsXmlNamespace(prefix, serviceType.NamespaceUri));
                        }

                        filterList = filterList + ((spaceFlag == true) ? " " : "") + prefix + ":" + serviceType.TypeName;
                        spaceFlag = true;
                    }
                }

                msg = new WsMessage(header, null, WsPrefix.Wsd, namespaces, null);

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                messageID = smw.WriteSoapMessageStart(xmlWriter, msg);

                // Performance debuging
                timeDebuger.PrintElapsedTime("*****Write Header Took");

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "Probe", null);

                // If filter is supplied add filter types tag else write blank string to probe body, force an empty tag
                if (filterList.Length != 0)
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "Types", null);
                    xmlWriter.WriteString(filterList);
                    xmlWriter.WriteEndElement(); // End Filter
                }
                else
                    xmlWriter.WriteString("");

                xmlWriter.WriteEndElement(); // End Probe

                // Performance debuging
                timeDebuger.PrintElapsedTime("*****Write Body Took");

                smw.WriteSoapMessageEnd(xmlWriter);

                // Performance debuging
                timeDebuger.PrintTotalTime(startTime, "***Probe Message Build Took");

                // return the probe message
                msg.Body = xmlWriter.ToArray();
            }

            return msg;
        }

        /// <summary>
        /// Send a Resolve request to a specific service endpoint.
        /// </summary>
        /// <param name="targetServiceAddress">
        /// A string containing the target service address of a known service. For Dpws this address would
        /// represents a devices address.
        /// For example: urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <remarks>
        /// A Resolve is used to resolve the transport address of a know service. The request contains a service
        /// address aquired from configuration or a previous Probe or Metadata Get request.
        /// The Resolve method sends a UDP request to the the Dpws multicast address, 239.255.255.250:3702.
        /// If a device with this address receives the request, it must send a unicast ResolveMatches
        /// response back to the client that made the request.
        /// </remarks>
        /// <returns>
        /// A collection of ResolveMatches objects. A ResolveMatch object contains endpoint details used
        /// used to locate the actual service on a network and the types supported by the service.
        /// </returns>
        public DpwsServiceDescription Resolve(string targetServiceAddress)
        {
            return Resolve(targetServiceAddress, -1);
        }

        /// <summary>
        /// Send a Resolve request to a specific service endpoint.
        /// </summary>
        /// <param name="serviceAddress">
        /// A string containing the target service address of a known service. For Dpws this address would
        /// represents a devices address.
        /// For example: urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <param name="timeout">
        /// An integer specifying a request timeout in milliseconds. Pass -1 to wait ReceiveTimeout.
        /// </param>
        /// <remarks>
        /// A Resolve is used to resolve the transport address of a know service. The request contains a service
        /// address aquired from configuration or a previous Probe or Metadata Get request.
        /// The Resolve method sends a UDP request to the the Dpws multicast address, 239.255.255.250:3702.
        /// If a device with this address receives the request, it must send a unicast ResolveMatches
        /// response back to the client that made the request.
        /// </remarks>
        /// <returns>
        /// A collection of ResolveMatches objects. A ResolveMatch object contains endpoint details used
        /// used to locate the actual service on a network and the types supported by the service.
        /// </returns>
        public DpwsServiceDescription Resolve(string targetServiceAddress, int timeout)
        {
            String messageID = "";
            WsMessage resolveRequest = BuildResolveRequest(targetServiceAddress, DiscoVersion.DiscoveryWellKnownAddress, ref messageID);

            if (resolveRequest == null)
                return null;

            // Send Resolve and return probe matches if received
            return SendResolveRequest((byte[])resolveRequest.Body, messageID, timeout < 0 ? m_receiveTimeout : timeout);
        }

        /// <summary>
        /// Builds a probe request message based on the filters parameter.
        /// </summary>
        /// <param name="targetServiceAddress">
        /// A string containing the target service address of a known service to resolve.
        /// For example: urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <param name="serviceAddress">
        /// A string containing the address of a service endpoint used to process the resolve request.
        /// For example: urn:uuid:22d0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <param name="messageID">
        /// A string reference used to store retreive the messageID from resolve message generation. The id is
        /// used to verify probe match responses for ad-hoc operation.
        /// </param>
        /// <returns>A byte array containing the resolve message or null if an error occures.</returns>
        private WsMessage BuildResolveRequest(string targetServiceAddress, string serviceAddress, ref String messageID)
        {
            WsMessage msg = null;

            // Build Resolve Request
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsWsaHeader header = new WsWsaHeader(
                    m_version.DiscoveryNamespace + "/Resolve",    // Action
                    null,                                           // RelatesTo
                    serviceAddress,                                 // To
                    null, null, null);                              // ReplyTo, From, Any

                msg = new WsMessage(header, null, WsPrefix.Wsd, null, null);

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                messageID = smw.WriteSoapMessageStart(xmlWriter, msg);


                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "Resolve", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "EndpointReference", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                xmlWriter.WriteString(targetServiceAddress);

                xmlWriter.WriteEndElement(); // End Address
                xmlWriter.WriteEndElement(); // End EndpointReference
                xmlWriter.WriteEndElement(); // End Resolve

                smw.WriteSoapMessageEnd(xmlWriter);

                // return the resolve message
                msg.Body = xmlWriter.ToArray();
            }

            return msg;
        }

        // This is the maximum size of the UDP datagram before it gets broken up
        // Since an incomplete packet would fail in the stack, there's no point to
        // read beyond the packetsize
        private const int c_MaxUdpPacketSize = 5229;

        /// <summary>
        /// Use to send a resolve request to the ws-discovery address and receive a resolve match.
        /// </summary>
        /// <param name="message">A byte array containing a the resolve message.</param>
        /// <param name="messageID">
        /// A string containing the message ID of a resolve request. This ID will be used to validate against
        /// a ResolveMatch received if it don't match, the ResolveMatch is discarded.
        /// </param>
        /// <param name="timeout">
        /// A DateTime value containing the length of time this request will wait for resolve match.
        /// until the timeout value has expired.
        /// </param>
        /// <returns>A resolve match object.</returns>
        private DpwsServiceDescription SendResolveRequest(byte[] message, string messageID, long timeout)
        {
            WsMessageCheck messageCheck = new WsMessageCheck();
            
            DpwsServiceDescription resolveMatch = null;

            System.Ext.Console.Write("");
            System.Ext.Console.Write("Sending Resolve:");
            System.Ext.Console.Write(message);

            // Create a new UdpClient
            using(Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                uint       ipLocal = WsNetworkServices.GetLocalIPV4AddressValue();
                IPAddress  localIP = IPAddress.Parse(WsNetworkServices.GetLocalIPV4Address());
                IPEndPoint localEP = new IPEndPoint(localIP, 0);

                // Very important - Set default multicast interface for the underlying socket
                udpClient.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)ipLocal);
                udpClient.ReceiveTimeout = (int)timeout;

                udpClient.Bind(localEP);

                // Random back off implemented as per soap over udp specification
                // for unreliable multicast message exchange
                Thread th = SendWithBackoff(message, udpClient);

                // Wait for resolve match as long a timeout has not expired
                byte[] resolveResponse = new byte[c_MaxUdpPacketSize];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int responseLength;
                DpwsDiscoClientProcessor soapProcessor = new DpwsDiscoClientProcessor(m_version);
                
                while (true)
                {
                    // Since MF sockets does not have an IOControl method catch 10054 to get around the problem
                    // with Upd and ICMP.
                    try
                    {
                        // Wait for response
                        responseLength = udpClient.ReceiveFrom(resolveResponse, c_MaxUdpPacketSize, SocketFlags.None, ref remoteEP);
                    }
                    catch (SocketException se)
                    {
                        if ((SocketError)se.ErrorCode == SocketError.ConnectionReset)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        if((SocketError)se.ErrorCode == SocketError.TimedOut)
                            break;
                        
                        throw se;
                    }

                    // If we received process resolve match
                    if (responseLength > 0)
                    {
                        System.Ext.Console.Write("ResolveMatches Response From: " + ((IPEndPoint)remoteEP).Address.ToString());
                        System.Ext.Console.Write(resolveResponse);

                        try
                        {
                            WsWsaHeader header = new WsWsaHeader();
                            
                            XmlReader reader = WsSoapMessageParser.ParseSoapMessage(resolveResponse, ref header, m_version);
                            
                            WsMessage msg = new WsMessage(header, null, WsPrefix.Wsdp);

                            msg.Reader = reader;

                            resolveMatch = soapProcessor.ProcessResolveMatch(msg, messageID, (IPEndPoint)remoteEP, messageCheck);
                            if (resolveMatch != null)
                                break;
                        }
                        catch (Exception e)
                        {
                            System.Ext.Console.Write(e.Message);
                        }
                    }
                }
                
                th.Join();
            }

            // Display results
            if (resolveMatch == null)
                System.Ext.Console.Write("Resolve timed out.");

            return resolveMatch;
        }


        private Thread SendWithBackoff(byte[] message, Socket udpClient)
        {
            System.Ext.Console.Write("UDP multicast to: " + WsDiscovery.WsDiscoveryEndPoint.ToString());
            System.Ext.Console.Write(message);
            
            Thread th = new Thread( new ThreadStart( delegate() 
            {
                int backoff = 0;
                int messageLen = message.Length;
                for (int i = 0; i < MulticastUdpRepeat; ++i)
                {
                    if (i == 0)
                    {
                        backoff = m_random.Next(UdpMaxDelay - UdpMinDelay) + UdpMinDelay;
                    }
                    else
                    {
                        backoff = backoff * (i*2);
                        if (backoff > UdpUpperDelay)
                        {
                            backoff = UdpUpperDelay;
                        }
                    }

                    try
                    {
                        udpClient.SendTo(message, messageLen, SocketFlags.None, WsDiscovery.WsDiscoveryEndPoint);
                    }
                    catch
                    {
                        break;
                    }

                    Thread.Sleep(backoff);
                }
            }
            ));

            th.Start();

            Thread.Sleep(0);

            return th;
        }
    }

    /// <summary>
    /// Base class used by DpwsProbeMatch and DpwsResolve match classes.
    /// </summary>
    /// <remarks>
    /// This base class contains target service endpoint properties that identify
    /// a services supported types and endpoint address.
    /// </remarks>
    public class DpwsServiceDescription
    {
        private readonly ProtocolVersion m_version;

        //--//

        internal enum ServiceDescriptionType
        {
            Hello,
            Bye,
            ProbeMatch,
            ResolveMatch
        }

        internal DpwsServiceDescription(XmlReader reader, ServiceDescriptionType type, ProtocolVersion v) :
            this(reader, type, v, null)
        {
        }

        internal DpwsServiceDescription(XmlReader reader, ServiceDescriptionType type, ProtocolVersion v, IPEndPoint remoteEP)
        {
            m_version = v;

            reader.ReadStartElement(); // ProbeMatch / ResolveMatch / Hello / Bye

            // Endpoint Reference
            if (reader.IsStartElement("EndpointReference", m_version.AddressingNamespace) == false)
            {
                throw new XmlException();                
            }
            this.Endpoint = new WsWsaEndpointRef(reader, m_version.AddressingNamespace);

            // Types
            if (reader.IsStartElement("Types", m_version.DiscoveryNamespace))
            {
                this.ServiceTypes = new DpwsServiceTypes(reader);
            }

            // Optional Scopes??
            if (reader.IsStartElement("Scopes", m_version.DiscoveryNamespace))
            {
                reader.Skip();
            }

            // XAddrs
            if (reader.IsStartElement("XAddrs", m_version.DiscoveryNamespace))
            {
                this.XAddrs = reader.ReadElementString().Trim().Split(' ');
                int count = XAddrs.Length;

                for (int i = 0; i < count; i++)
                {
                    // validate all XAddrs for fully qualified paths
                    if (Uri.IsWellFormedUriString(XAddrs[i], UriKind.Absolute) == false)
                    {
                        throw new XmlException();
                    }

                    if(remoteEP != null)
                    {
                        int idx = XAddrs[i].ToLower().IndexOf("localhost");

                        if(idx != -1)
                        {
                            XAddrs[i] = XAddrs[i].Substring(0, idx) + remoteEP.Address.ToString() + XAddrs[i].Substring(idx + 9 /*localhost*/);
                        }
                    }
                }
            }
            else if (type == ServiceDescriptionType.ResolveMatch) // for ResolveMatch, XAddrs is required
            {
                throw new XmlException();
            }

            // MetadataVersion
            if (reader.IsStartElement("MetadataVersion", m_version.DiscoveryNamespace))
            {
                this.MetadataVersion = reader.ReadElementString();
            }
            else if (type != ServiceDescriptionType.Bye) // for Hello, ProbeMatch and ResolveMatch, MetadataVersion is required
            {
                throw new XmlException();
            }

            XmlReaderHelper.SkipAllSiblings(reader); // xs:any
            reader.ReadEndElement(); // ProbeMatch / ResolveMatch / Hello / Bye
        }

        /// <summary>
        /// Creates an instance of a DpwsServiceDescription class initialized with a service endpoint and a list of
        /// supported service types.
        /// </summary>
        /// <param name="endpoint">A WsWsaEndpointRef object containing a Dpws Device servie endpoint.</param>
        /// <param name="serviceTypes">A string array containing a list of service types supporte by a Dpws devie service endpoint.</param>
        public DpwsServiceDescription(WsWsaEndpointRef endpoint, DpwsServiceTypes serviceTypes)
        {
            this.Endpoint = endpoint;
            this.ServiceTypes = serviceTypes;
        }

        /// <summary>
        /// Use to get a WsWsaEndpointRef contining the endpoint of a service that supports
        /// types requested in a probe match.
        /// </summary>
        public readonly WsWsaEndpointRef Endpoint;

        /// <summary>
        /// Use to get the metadata version of the probe match object.
        /// </summary>
        public readonly String MetadataVersion;

        /// <summary>
        /// Use to get a list of types supported by a device.
        /// </summary>
        public readonly DpwsServiceTypes ServiceTypes;

        /// <summary>
        /// Use to get an array containing transport specific endpoint addresses representing a
        /// services physical endpoint address. This is an optional ws-discovery parameter.
        /// </summary>
        public readonly String[] XAddrs;
    }

    /// <summary>
    /// A base collection class used to store DpwsServiceDescription objects.
    /// </summary>
    /// <remarks>
    /// DpwsProbeMatches and DpwsResolveMatches derive from this base collection.
    /// This class is thread safe.
    /// </remarks>
    public class DpwsServiceDescriptions
    {
        private object    m_threadLock;
        private ArrayList m_serviceDescriptions;

        /// <summary>
        /// Creates an instance of a DpwsServiceDescriptions collection.
        /// </summary>
        internal DpwsServiceDescriptions()
        {
            m_threadLock          = new object();
            m_serviceDescriptions = new ArrayList();            
        }

        internal DpwsServiceDescriptions(XmlReader reader, DpwsServiceDescription.ServiceDescriptionType type, ProtocolVersion v, IPEndPoint remoteEP)
        {
            m_threadLock          = new object();
            m_serviceDescriptions = new ArrayList();            

            Microsoft.SPOT.Debug.Assert(type == DpwsServiceDescription.ServiceDescriptionType.ProbeMatch ||
                type == DpwsServiceDescription.ServiceDescriptionType.ResolveMatch);

            String collectionName, itemName;
            if (type == DpwsServiceDescription.ServiceDescriptionType.ProbeMatch)
            {
                collectionName = "ProbeMatches";
                itemName = "ProbeMatch";
            }
            else
            {
                collectionName = "ResolveMatches";
                itemName = "ResolveMatch";
            }

            reader.ReadStartElement(collectionName, v.DiscoveryNamespace);

            while (reader.IsStartElement(itemName, v.DiscoveryNamespace))
            {
#if DEBUG
                int depth = reader.Depth;
#endif
                m_serviceDescriptions.Add(new DpwsServiceDescription(reader, type, v, remoteEP));
#if DEBUG
                Microsoft.SPOT.Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif
            }

            if (type == DpwsServiceDescription.ServiceDescriptionType.ResolveMatch && m_serviceDescriptions.Count > 1)
            {
                // Per schema, there can only be 1 resolve match
                throw new XmlException();
            }

            XmlReaderHelper.SkipAllSiblings(reader); // xs:any
            reader.ReadEndElement(); // collectionName
        }

        /// <summary>
        /// Use to Get the number of DpwsServiceDescription elements actually contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_serviceDescriptions.Count;
            }
        }

        /// <summary>
        /// Use to Get or set the DpwsServiceDescription element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the DpwsServiceDescription element to get or set.
        /// </param>
        /// <returns>
        /// An intance of a DpwsServiceDescription element.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index is less than zero.-or- index is equal to or greater than the collection count.
        /// </exception>
        public DpwsServiceDescription this[int index]
        {
            get
            {
                return (DpwsServiceDescription)m_serviceDescriptions[index];
            }
        }

        public DpwsServiceDescription this[String endpointAddress]
        {
            get
            {
                lock (m_threadLock)
                {
                    int index = IndexOf(endpointAddress);

                    if(index >= 0)
                    {
                        return (DpwsServiceDescription)m_serviceDescriptions[index];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Adds a DpwsServiceDescription to the end of the collection.
        /// </summary>
        /// <param name="value">
        /// The DpwsServiceDescription element to be added to the end of the collection.
        /// The value can be null.
        /// </param>
        /// <returns>
        /// The collection index at which the DpwsServiceDescription has been added.
        /// </returns>
        public int Add(DpwsServiceDescription value)
        {
            lock (m_threadLock)
            {
                return m_serviceDescriptions.Add(value);
            }
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            lock (m_threadLock)
            {
                m_serviceDescriptions.Clear();
            }
        }

        /// <summary>
        /// Determines whether an instance of a specified DpwsServiceDescription is in the collection.
        /// </summary>
        /// <param name="item">
        /// The DpwsServiceDescription to locate in the collection. The value can be null.
        /// </param>
        /// <returns>
        /// True if DpwsServiceDescription is found in the collection; otherwise, false.
        /// </returns>
        public bool Contains(DpwsServiceDescription item)
        {
            lock (m_threadLock)
            {
                return m_serviceDescriptions.Contains(item);
            }
        }

        /// <summary>
        /// Searches for a DpwsServiceDescription by endpoint address and returns the zero-based index
        /// of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="endpointAddress">
        /// The endpoint address of the DpwsServiceDescription to locate in the collection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of DpwsServiceDescription within the entire collection,
        /// if found; otherwise, -1.
        /// </returns>
        public int IndexOf(string endpointAddress)
        {
            lock (m_threadLock)
            {
                int count = m_serviceDescriptions.Count;

                endpointAddress = endpointAddress.ToLower();
                
                for (int i = 0; i < count; i++)
                {
                    if (((DpwsServiceDescription)m_serviceDescriptions[i]).Endpoint.Address.AbsoluteUri.ToLower() == endpointAddress)
                        return i;
                }

                return -1;
            }
        }
    }
}


