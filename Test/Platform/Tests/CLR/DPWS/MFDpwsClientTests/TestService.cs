using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Threading;
using Dpws.Device;
using Dpws.Device.Services;
using Ws.Services;
using Ws.Services.Faults;
using Ws.Services.Utilities;
using Ws.Services.WsaAddressing;
using Ws.Services.Xml;

using Convert = System.Convert;
using System.Ext;
using System.Ext.Xml;
using System.Xml;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    class TestService : DpwsHostedService
    {
        private const String SimpleServiceNamespaceUri = "http://schemas.example.org/SimpleService";

        public TestService(string guid, ProtocolVersion version) : base(version)
        {
            // Add ServiceNamespace. Set ServiceID and ServiceTypeName
            ServiceNamespace = new WsXmlNamespace("sim", "http://schemas.example.org/SimpleService");
            ServiceID = "urn:uuid:" + guid;
            ServiceTypeName = "TestService";

            // Add additional namesapces if needed
            // example: Namespaces.Add("someprefix", "http://some/Namespace");

            // Add service operations
            ServiceOperations.Add(new WsServiceOperation("http://schemas.example.org/SimpleService", "OneWay"));
            ServiceOperations.Add(new WsServiceOperation("http://schemas.example.org/SimpleService", "TwoWayRequest"));

            // Add event sources
            DpwsWseEventSource SimpleEvent = new DpwsWseEventSource("sim", "http://schemas.example.org/EventingService", "SimpleEvent");
            EventSources.Add(SimpleEvent);
            this.AddEventServices();
        }

        public TestService(string guid) : 
            this(guid, new ProtocolVersion10())
        {
        }

        public byte[] OneWay(WsWsaHeader header, XmlReader reader)
        {
             try
            {
                // Find beginning of request
                reader.ReadStartElement("OneWay", SimpleServiceNamespaceUri);

                // Find the integer value
                int number = Convert.ToInt32(reader.ReadElementString("Param", SimpleServiceNamespaceUri));

                Log.Comment("");
                Log.Comment("Integer = " + number.ToString());
                Log.Comment("");

                return new byte[0];     // Empty response
            }
            catch (Exception e)
            {
                // Something went wrong
                throw new WsFaultException(header, WsFaultType.XmlException, e.ToString());
            }
        }

        // TwoWayRequest: Accept two integer values, return their sum
        public byte[] TwoWayRequest(WsWsaHeader header, XmlReader reader)
        {
            try
            {
                // Find beginning of request
                reader.ReadStartElement("TwoWayRequest", SimpleServiceNamespaceUri);

                // Find the values to be added
                int X = Convert.ToInt32(reader.ReadElementString("X", SimpleServiceNamespaceUri));
                int Y = Convert.ToInt32(reader.ReadElementString("Y", SimpleServiceNamespaceUri));

                Log.Comment("");
                Log.Comment("X = " + X.ToString() + " Y = " + Y.ToString());
                Log.Comment(X.ToString() + " + " + Y.ToString() + " = " + ((int)(X + Y)).ToString());
                Log.Comment("");

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
        public byte[] TwoWayResponse(WsWsaHeader header, int sum)
        {
            ProtocolVersion v = new ProtocolVersion10();

            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            // Write processing instructions and root element
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("soap", "Envelope", "http://www.w3.org/2003/05/soap-envelope");

            // Write namespaces
            xmlWriter.WriteAttributeString("xmlns", "wsdp", null, v.WsdpNamespaceUri);
            xmlWriter.WriteAttributeString("xmlns", "wsd", null, v.DiscoveryNamespace);
            xmlWriter.WriteAttributeString("xmlns", "wsa", null, v.AddressingNamespace);
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
            byte[] soapBuffer = soapStream.ToArray();
            xmlWriter.Close();

            return soapBuffer;
        }
        //static public byte[] TwoWayRequest(int X, int Y, string endpointAddress)
        //{
        //    MemoryStream soapStream = new MemoryStream();
        //    XmlWriter xmlWriter = XmlWriter.Create(soapStream);

        //    // Write processing instructions and root element
        //    xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
        //    xmlWriter.WriteStartElement("soap", "Envelope", "http://www.w3.org/2003/05/soap-envelope");

        //    // Write namespaces
        //    xmlWriter.WriteAttributeString("xmlns", "wsdp", null, Device.Namespaces.GetNamespace("wsdp"));
        //    xmlWriter.WriteAttributeString("xmlns", "wsd", null, Device.Namespaces.GetNamespace("wsd"));
        //    xmlWriter.WriteAttributeString("xmlns", "wsa", null, Device.Namespaces.GetNamespace("wsa"));
        //    xmlWriter.WriteAttributeString("xmlns", "smpl", null, "http://schemas.example.org/SimpleService");

        //    // Write header
        //    xmlWriter.WriteStartElement("soap", "Header", null);
        //    xmlWriter.WriteStartElement("wsa", "To", null);
        //    xmlWriter.WriteString(endpointAddress);
        //    xmlWriter.WriteEndElement(); // End To
        //    xmlWriter.WriteStartElement("wsa", "Action", null);
        //    xmlWriter.WriteString("http://schemas.example.org/SimpleService/TwoWayRequest");
        //    xmlWriter.WriteEndElement(); // End Action
        //    xmlWriter.WriteStartElement("wsa", "From", null);
        //    xmlWriter.WriteStartElement("wsa", "Address", null);
        //    xmlWriter.WriteString(endpointAddress);
        //    xmlWriter.WriteEndElement(); // End Address
        //    xmlWriter.WriteEndElement(); // End From
        //    xmlWriter.WriteStartElement("wsa", "MessageID", null);
        //    xmlWriter.WriteString("urn:uuid:" + Guid.NewGuid());
        //    xmlWriter.WriteEndElement(); // End MessageID
        //    xmlWriter.WriteEndElement(); // End Header

        //    // write body
        //    xmlWriter.WriteStartElement("soap", "Body", null);
        //    xmlWriter.WriteStartElement("smpl", "TwoWayRequest", null);
        //    xmlWriter.WriteStartElement("smpl", "X", null);
        //    xmlWriter.WriteString(X.ToString());
        //    xmlWriter.WriteEndElement(); // End X
        //    xmlWriter.WriteStartElement("smpl", "Y", null);
        //    xmlWriter.WriteString(X.ToString());
        //    xmlWriter.WriteEndElement(); // End Y
        //    xmlWriter.WriteEndElement(); // End TwoWayRequest
        //    xmlWriter.WriteEndElement(); // End Body

        //    xmlWriter.WriteEndElement();

        //    // Create return buffer and close writer
        //    xmlWriter.Flush();
        //    byte[] soapBuffer = soapStream.ToArray();
        //    xmlWriter.Close();

        //    return soapBuffer;
        //}

    }

}
