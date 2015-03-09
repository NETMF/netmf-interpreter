using System;
using System.Xml;
using Microsoft.SPOT;
using Ws.Services;
using Ws.Services.Faults;
using Ws.Services.WsaAddressing;
using Ws.Services.Xml;
using Dpws.Device.Services;
using System.IO;
using System.Ext.Xml;
using System.Ext;
using Microsoft.SPOT.Platform.Test;
using Ws.Services.Soap;

namespace Interop.SimpleService
{

    // SimpleService - Sample DPWS service
    // Messages supported:
    //      OneWay: accepts an integer, does not return a response
    //      TwoWayRequest: accepts two integers, returns their sum
    // Events supported:
    //      SimpleEvent (implementation from EventingService)

    class SimpleService : DpwsHostedService
    {
        private const String SimpleServiceNamespaceUri = "http://schemas.example.org/SimpleService";

        // Constructor sets service properties and defines operations and adds event sources
        public SimpleService(ProtocolVersion version) : base(version)
        {
            // Add ServiceNamespace. Set ServiceID and ServiceTypeName
            ServiceNamespace = new WsXmlNamespace("sim", "http://schemas.example.org/SimpleService");
            ServiceID = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b90";
            ServiceTypeName = "SimpleService";

            // Add service operations
            ServiceOperations.Add(new WsServiceOperation("http://schemas.example.org/SimpleService", "OneWay"));
            ServiceOperations.Add(new WsServiceOperation("http://schemas.example.org/SimpleService", "TwoWayRequest"));

            // Add event sources
            DpwsWseEventSource SimpleEvent = new DpwsWseEventSource("sim", "http://schemas.example.org/EventingService", "SimpleEvent");
            EventSources.Add(SimpleEvent);
            this.AddEventServices();
        }

        public SimpleService() : 
            this(new ProtocolVersion10())
        {
        }

        // OneWay: Accept an integer value
        public WsMessage OneWay(WsMessage request)
        {
            try
            {
                WsWsaHeader header = request.Header;
                XmlReader reader = request.Reader;

                // Find beginning of request
                reader.ReadStartElement("OneWay", SimpleServiceNamespaceUri);

                // Find the integer value
                int number = Convert.ToInt32(reader.ReadElementString("Param", SimpleServiceNamespaceUri));
                
                //Log.Comment("");
                //Log.Comment("Integer = " + number.ToString());
                //Log.Comment("");

                return null;     // Empty response
            }
            catch (Exception e)
            {
                // Something went wrong
                throw new WsFaultException(request.Header, WsFaultType.XmlException, e.ToString());
            }
        }

        // TwoWayRequest: Accept two integer values, return their sum
        public WsMessage TwoWayRequest(WsMessage request)
        {
            WsWsaHeader header = request.Header;
            XmlReader reader = request.Reader;

            try
            {
                // Find beginning of request
                reader.ReadStartElement("TwoWayRequest", SimpleServiceNamespaceUri);

                // Find the values to be added
                int X = Convert.ToInt32(reader.ReadElementString("X", SimpleServiceNamespaceUri));
                int Y = Convert.ToInt32(reader.ReadElementString("Y", SimpleServiceNamespaceUri));

                //Log.Comment("");
                //Log.Comment("X = " + X.ToString() + " Y = " + Y.ToString());
                //Log.Comment(X.ToString() + " + " + Y.ToString() + " = " + ((int)(X + Y)).ToString());
                //Log.Comment("");

                // Return the response
                return TwoWayResponse(header, X + Y);
            }
            catch (Exception e)
            {
                // Something went wrong 
                throw new WsFaultException(header, WsFaultType.XmlException, e.ToString());
            }
        }

        // TwoWayResponse: Build the response for a TwoWayRequest
        public WsMessage TwoWayResponse(WsWsaHeader header, int sum)
        {
            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            // Write processing instructions and root element
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("soap", "Envelope", "http://www.w3.org/2003/05/soap-envelope");

            // Write namespaces
            xmlWriter.WriteAttributeString("xmlns", "wsdp", null, this.m_version.WsdpNamespaceUri);
            xmlWriter.WriteAttributeString("xmlns", "wsd", null, this.m_version.DiscoveryNamespace);
            xmlWriter.WriteAttributeString("xmlns", "wsa", null, this.m_version.AddressingNamespace);
            xmlWriter.WriteAttributeString("xmlns", "sim", null, SimpleServiceNamespaceUri);

            // Write header
            xmlWriter.WriteStartElement("soap", "Header", null);
            xmlWriter.WriteStartElement("wsa", "To", null);
            xmlWriter.WriteString(header.From.Address.AbsoluteUri);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("wsa", "Action", null);
            xmlWriter.WriteString("http://schemas.example.org/SimpleService/TwoWayResponse");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("wsa", "RelatesTo", null);
            xmlWriter.WriteString(header.MessageID);
            xmlWriter.WriteEndElement(); // End RelatesTo
            xmlWriter.WriteStartElement("wsa", "MessageID", null);
            xmlWriter.WriteString("urn:uuid:" + Guid.NewGuid().ToString());
            xmlWriter.WriteEndElement(); // End MessageID
            xmlWriter.WriteEndElement(); // End Header

            // Write body
            xmlWriter.WriteStartElement("soap", "Body", null);
            xmlWriter.WriteStartElement("sim", "TwoWayResponse", null);
            xmlWriter.WriteStartElement("sim", "Sum", null);
            xmlWriter.WriteString(sum.ToString());
            xmlWriter.WriteEndElement(); // End Sun
            xmlWriter.WriteEndElement(); // End TwoWayResponse
            xmlWriter.WriteEndElement(); // End Body

            xmlWriter.WriteEndElement();

            // Create return buffer and close writer
            xmlWriter.Flush();
            WsMessage msg = new WsMessage(null, soapStream.ToArray(), WsPrefix.Wsdp);
            xmlWriter.Close();

            return msg;
        }

    }

}
