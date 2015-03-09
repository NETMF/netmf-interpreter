using System;
using System.Xml;
using Microsoft.SPOT;
using Ws.Services;
using Ws.Services.WsaAddressing;
using Dpws.Client;
using Dpws.Client.Discovery;
//using Dpws.Client.Transport;
using System.Ext;
using System.IO;
using System.Ext.Xml;
using System.Threading;
using Microsoft.SPOT.Platform.Test;
using Ws.Services.Binding;
using Ws.Services.Transport.HTTP;
using Ws.Services.Soap;

namespace Interop.SimpleService
{
    /// <summary>
    /// Sample device client class.
    /// </summary>
    /// <remarks>
    /// This class, derived from DpwsClient, contains Hello and Bye message event handlers. When a Hello event is received,
    /// the handler sends a probe request with the service types filter set to DPWS interop type sim:SimpleService, accepting
    /// up to 10 probe matches. For each probe match received, a resolve request is sent to the indicated endpoint address.
    /// For each resolve match received, a Get a request is sent to each Xaddr in the resolve match message. For each 
    /// successful Get, a TwoWayRequest (integer add request) is made to the service.
    /// </remarks>
    class SimpleServiceClient : DpwsClient
    {
        public bool m_receivedHelloEvent = false;
        public bool m_receivedByeEvent = false;
        public bool m_getMex = false;
        public bool m_twoWay = false;

        public AutoResetEvent arHello;
        public AutoResetEvent arBye;

        private const string c_LocalUrn = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b90";
        private const string c_ServiceUrn = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51";

        public SimpleServiceClient()
            : base(new WS2007HttpBinding(new HttpTransportBindingConfig(c_LocalUrn, 8085)), new ProtocolVersion10())
        {
            arHello = new AutoResetEvent(false);
            arBye = new AutoResetEvent(false);
            this.IgnoreRequestFromThisIP = false;
            //EndpointAddress = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b90";
            this.HelloEvent += new HelloEventHandler(SimpleServiceClient_HelloEvent);
            this.ByeEvent += new ByeEventHandler(SimpleServiceClient_ByeEvent);
        }

        /// <summary>
        /// Bye event handler.
        /// </summary>
        /// <param name="obj">A reference to the object calling the event handler.</param>
        /// <param name="byeEventArgs">A DpwsServiceDescription object containing the Bye parameters.</param>
        /// <remarks>This handler simply displays the Bye message.</remarks>
        void SimpleServiceClient_ByeEvent(object obj, DpwsServiceDescription byeEventArgs)
        {
            Log.Comment("Received Bye Event");

            if (m_receivedByeEvent) return;

            if (byeEventArgs.Endpoint.Address.AbsoluteUri == c_ServiceUrn)
            {
                m_receivedByeEvent = true;

                arBye.Set();
            }

            //Log.Comment("SimpleService received a bye request.");
            ////Log.Comment("Endpoint address: " + byeEventArgs.Endpoint.Address.AbsoluteUri);
            ////Log.Comment("Xaddrs: " + byeEventArgs.XAddrs);
        }

        /// <summary>
        /// Hello event handler.
        /// </summary>
        /// <param name="obj">A reference to the object calling the event handler.</param>
        /// <param name="helloEventArgs">A DpwsServiceDescription object containing the Hello requesst parameters.</param>
        [GloballySynchronized()]
        void SimpleServiceClient_HelloEvent(object obj, DpwsServiceDescription helloEventArgs)
        {
            m_receivedHelloEvent = true;
            DpwsMetadata m_mexDetails = null;

            try
            {
                // Print Hello information
                //Log.Comment("");
                //Log.Comment("SimpleService received a hello request.");
                ////Log.Comment("Endpoint address: " + helloEventArgs.Endpoint.Address.AbsoluteUri);
                string types = "";
                int count = helloEventArgs.ServiceTypes.Count;
                for (int i = 0; i < count; i++)
                {
                    DpwsServiceType type = helloEventArgs.ServiceTypes[i];
                    types += "NamespaceUri: " + type.NamespaceUri + " " + "TypeName: " + type.TypeName + "\n";
                }
                ////Log.Comment("Types: " + types);

                if (helloEventArgs.XAddrs != null)
                {
                    foreach (String xaddr in helloEventArgs.XAddrs)
                    {
                        ////Log.Comment("Xaddrs: " + xaddr);
                    }
                }

                ////Log.Comment("Metadata version: " + helloEventArgs.MetadataVersion);
                ////Log.Comment("");

                // Probe for services. For each valid probe match, print service information.
                DpwsServiceType searchType = new DpwsServiceType("SimpleService", "http://schemas.example.org/SimpleService");
                DpwsServiceTypes searchTypes = new DpwsServiceTypes();

                searchTypes.Add(searchType);

                // Accept the first 10 found within the first 10 seconds
                DpwsServiceDescriptions probeMatches = this.DiscoveryClient.Probe(searchTypes, 1, 10000);

                // retry
                if (probeMatches.Count == 0)
                {
                    probeMatches = this.DiscoveryClient.Probe(searchTypes, 1, 10000);
                }

                if (probeMatches != null)
                {
                    // Print the probe match information
                    ////Log.Comment("**********************");
                    ////Log.Comment("ProbeMatches received: " + probeMatches.Count);
                    int probeMatchCount = probeMatches.Count;
                    for (int i = 0; i < probeMatchCount; i++)
                    {
                        DpwsServiceDescription probeMatch = probeMatches[i];
                        ////Log.Comment("");
                        ////Log.Comment("ProbeMatch:");
                        ////Log.Comment("  Endpoint Address = " + probeMatch.Endpoint.Address.AbsoluteUri);
                        ////Log.Comment("  Types:");

                        PrintServiceTypes(probeMatch.ServiceTypes);

                        //////Log.Comment("  Xaddrs:");
                        //foreach (string xaddr in probeMatch.XAddrs)
                        //    //Log.Comment("    TransportAddress = " + xaddr);
                        ////Log.Comment("  Metadata Version = " + probeMatch.MetadataVersion);

                        // Resolve this endpoint
                        DpwsServiceDescription resolveMatch = this.DiscoveryClient.Resolve(probeMatch.Endpoint.Address.AbsoluteUri, 10000);
                        if (resolveMatch != null)
                        {
                            // Print resolve match information
                            ////Log.Comment("");
                            ////Log.Comment("ResolveMatch:");
                            ////Log.Comment("  Endpoint Address = " + resolveMatch.Endpoint.Address.AbsoluteUri);
                            ////Log.Comment("  Types:");

                            PrintServiceTypes(resolveMatch.ServiceTypes);

                            ////Log.Comment("  Xaddrs:");
                            //foreach (string xaddr in resolveMatch.XAddrs)
                            //    //Log.Comment("    TransportAddress = " + xaddr);

                            //Log.Comment("  Metadata Version = " + resolveMatch.MetadataVersion);

                            // For each Xaddr, get metadata
                            foreach (string xaddr in resolveMatch.XAddrs)
                            {
                                // Call Get
                                m_mexDetails = this.MexClient.Get(xaddr);

                                if (m_mexDetails == null)
                                {

                                    ////Log.Comment("Get failed. Address: " + xaddr);
                                }
                                else
                                {
                                    m_getMex = true;

                                    // Display metadata information
                                    ////Log.Comment("  Metadata:");
                                    ////Log.Comment("    ThisModel:");
                                    ////Log.Comment("      Manufacturer: " + m_mexDetails.ThisModel.Manufacturer);
                                    ////Log.Comment("      ManufacturerUrl: " + m_mexDetails.ThisModel.ManufacturerUrl);
                                    ////Log.Comment("      ModelName: " + m_mexDetails.ThisModel.ModelName);
                                    ////Log.Comment("      ModelNumber: " + m_mexDetails.ThisModel.ModelNumber);
                                    ////Log.Comment("      ModelUrl: " + m_mexDetails.ThisModel.ModelUrl);
                                    ////Log.Comment("      PresentationUrl: " + m_mexDetails.ThisModel.PresentationUrl);
                                    ////Log.Comment("    ThisDevice:");
                                    ////Log.Comment("      FirmwareVersion: " + m_mexDetails.ThisDevice.FirmwareVersion);
                                    ////Log.Comment("      FriendlyName: " + m_mexDetails.ThisDevice.FriendlyName);
                                    ////Log.Comment("      SerialNumber: " + m_mexDetails.ThisDevice.SerialNumber);
                                    if (m_mexDetails.Relationship.Host != null)
                                    {
                                        ////Log.Comment("    Host:");
                                        ////Log.Comment("      Service ID: " + m_mexDetails.Relationship.Host.ServiceID);
                                        ////Log.Comment("      Address: " + m_mexDetails.Relationship.Host.EndpointRefs[0].Address.AbsoluteUri);
                                        ////Log.Comment("      Types:");

                                        PrintServiceTypes(m_mexDetails.Relationship.Host.ServiceTypes);
                                    }
                                    if (m_mexDetails.Relationship.HostedServices != null)
                                    {
                                        ////Log.Comment("    HostedServices:");
                                        int hostedServiceCount = m_mexDetails.Relationship.HostedServices.Count;
                                        for (int j = 0; j < hostedServiceCount; j++)
                                        {
                                            DpwsMexService hostedService = m_mexDetails.Relationship.HostedServices[j];

                                            ////Log.Comment("      Service ID: " + hostedService.ServiceID);
                                            ////Log.Comment("      Address: " + hostedService.EndpointRefs[0].Address.AbsoluteUri);
                                            ////Log.Comment("      Types:");

                                            PrintServiceTypes(hostedService.ServiceTypes);
                                        }
                                    }
                                }
                            }

                            if (m_mexDetails != null)
                            {
                                // Look for the SimpleService in the HostedServices collection, create an HTTP
                                // client, call TwoWayRequest on the service endpoint, and display the result.
                                int hostedServiceCount = m_mexDetails.Relationship.HostedServices.Count;
                                for (int j = 0; j < hostedServiceCount; j++)
                                {
                                    DpwsMexService hostedService = m_mexDetails.Relationship.HostedServices[j];

                                    if (hostedService.ServiceTypes["SimpleService"] != null)    // found a SimpleService endpoint
                                    {
                                        string endpointAddress = hostedService.EndpointRefs[0].Address.AbsoluteUri;
                                        ////Log.Comment("");
                                        ////Log.Comment("Calling TwoWayRequest at endpoint: " + endpointAddress);

                                        // Create HttpClient and send request
                                        int x = 150;
                                        int y = 180;

                                        byte[] TwoWayRequest = Build2wayRequest(x, y, endpointAddress);

                                        
                                        //Log.Comment(new String(System.Text.Encoding.UTF8.GetChars(TwoWayRequest)));
                                        //DpwsHttpClient httpClient = new DpwsHttpClient();
                                        //DpwsSoapResponse response = httpClient.SendRequest(TwoWayRequest, endpointAddress, false, false);

                                        WsHttpClient httpClient = new WsHttpClient(m_version);
                                        WsMessage response = httpClient.SendRequest(new WsMessage(null, TwoWayRequest, WsPrefix.Wsdp), new Uri(endpointAddress));
                                        
                                        if (response != null)       // got a response
                                        {
                                            try
                                            {
                                                int sum = Parse2WayResponse(response.Header, response.Reader);
                                                //Log.Comment("");
                                                //Log.Comment("Received sum of " + sum.ToString() + " from " + endpointAddress);
                                                if ((x + y) == sum)
                                                    m_twoWay = true;
                                            }
                                            finally
                                            {
                                                response.Reader.Close();
                                            }
                                        }
                                    }
                                }
                            }
                            //Log.Comment("");
                        }
                    }
                }
            }
            finally
            {
            }

            if (m_mexDetails != null)
            {
                arHello.Set();
            }
        }

        // Helper method to print names and namespaces of service types
        private void PrintServiceTypes(DpwsServiceTypes types)
        {
            int typeCount = types.Count;
            for (int i = 0; i < typeCount; i++)
            {
                DpwsServiceType serviceType = types[i];
                //Log.Comment("    Name = " + serviceType.TypeName);
                //Log.Comment("    Namespace = " + serviceType.NamespaceUri);
            }
        }

        // Helper method to parse the response from a TwoWayRequest message
        private int Parse2WayResponse(WsWsaHeader header, XmlReader reader)
        {
            try
            {
                reader.ReadStartElement("TwoWayResponse", "http://schemas.example.org/SimpleService");
                int Sum = Convert.ToInt32(reader.ReadElementString("Sum", "http://schemas.example.org/SimpleService"));
                return Sum;
            }
            catch
            {
                //Log.Comment("Parse2WayResponse failed.");
                return 0;
            }

        }

        /// <summary>
        /// Builds a TwoWayRequest message.
        /// </summary>
        /// <param name="X">The first integer to add.</param>
        /// <param name="Y">The second integer to add.</param>
        /// <param name="endpointAddress">A string containig the service endpoint address.</param>
        /// <returns>The SOAP request.</returns>
        private byte[] Build2wayRequest(int X, int Y, string endpointAddress)
        {
            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            // Write processing instructions and root element
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("soap", "Envelope", "http://www.w3.org/2003/05/soap-envelope");

            // Write namespaces
            ProtocolVersion10 ver = new ProtocolVersion10();
            xmlWriter.WriteAttributeString("xmlns", "wsa", null, ver.AddressingNamespace);
            xmlWriter.WriteAttributeString("xmlns", "smpl", null, "http://schemas.example.org/SimpleService");

            // Write header
            xmlWriter.WriteStartElement("soap", "Header", null);
            xmlWriter.WriteStartElement("wsa", "To", null);
            xmlWriter.WriteString(endpointAddress);
            xmlWriter.WriteEndElement(); // End To
            xmlWriter.WriteStartElement("wsa", "Action", null);
            xmlWriter.WriteString("http://schemas.example.org/SimpleService/TwoWayRequest");
            xmlWriter.WriteEndElement(); // End Action
            xmlWriter.WriteStartElement("wsa", "From", null);
            xmlWriter.WriteStartElement("wsa", "Address", null);
            xmlWriter.WriteString(EndpointAddress);
            xmlWriter.WriteEndElement(); // End Address
            xmlWriter.WriteEndElement(); // End From
            xmlWriter.WriteStartElement("wsa", "MessageID", null);
            xmlWriter.WriteString("urn:uuid:" + Guid.NewGuid());
            xmlWriter.WriteEndElement(); // End MessageID
            xmlWriter.WriteEndElement(); // End Header

            // Write body
            xmlWriter.WriteStartElement("soap", "Body", null);
            xmlWriter.WriteStartElement("smpl", "TwoWayRequest", null);
            xmlWriter.WriteStartElement("smpl", "X", null);
            xmlWriter.WriteString(X.ToString());
            xmlWriter.WriteEndElement(); // End X
            xmlWriter.WriteStartElement("smpl", "Y", null);
            xmlWriter.WriteString(Y.ToString());
            xmlWriter.WriteEndElement(); // End Y
            xmlWriter.WriteEndElement(); // End TwoWayRequest
            xmlWriter.WriteEndElement(); // End Body

            xmlWriter.WriteEndElement(); // End Document

            // Create return buffer and close xmlWriter
            xmlWriter.Flush();
            byte[] soapBuffer = soapStream.ToArray();
            xmlWriter.Close();

            return soapBuffer;
        }

    }
}
