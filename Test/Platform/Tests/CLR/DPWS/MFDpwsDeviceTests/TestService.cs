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

using Convert = System.Ext.Convert;
using System.Ext;
using System.Ext.IO;
using System.Ext.Xml;

namespace Interop.TestService
{
    class TestService : DpwsHostedService
    {
        public TestService(string guid)
        {
            // Add ServiceNamespace. Set ServiceID and ServiceTypeName
            ServiceNamespace = new WsXmlNamespace("sim", 
                "http://schemas.example.org/SimpleService");
            ServiceID = "urn:uuid:" + guid;
            ServiceTypeName = "TestService";

            // Add additional namesapces if needed
            // example: Namespaces.Add("someprefix", "http://some/Namespace");

            // Add service operations
            ServiceOperations.Add(new WsServiceOperation("sim", 
                "http://schemas.example.org/SimpleService", "OneWay"));
            ServiceOperations.Add(new WsServiceOperation("sim", 
                "http://schemas.example.org/SimpleService", "TwoWayRequest"));

            // Add event sources
            DpwsWseEventSource SimpleEvent = new DpwsWseEventSource("sim", 
                "http://schemas.example.org/EventingService", "SimpleEvent");
            EventSources.Add(SimpleEvent);
            this.AddEventServices();
        }

        public byte[] OneWay(WsWsaHeader header, WsXmlDocument envelope)
        {
            WsXmlNode tempNode;
            WsFault fault = new WsFault();

            if ((tempNode = envelope.SelectSingleNode("Body/OneWay/Param", false)) == null)
                return fault.RaiseFault(header, WsExceptionFaultType.XmlException, 
                    "Body/OneWay is missing.");
            int number = Convert.ToInt32(tempNode.Value);
            Debug.Print("");
            Debug.Print("Integer = " + tempNode.Value);
            Debug.Print("");
            byte[] response = new byte[0];
            return response;
        }

        public byte[] TwoWayRequest(WsWsaHeader header, WsXmlDocument envelope)
        {
            WsXmlNode tempNode;
            WsFault fault = new WsFault();

            if ((tempNode = envelope.SelectSingleNode("Body/TwoWayRequest/X", false)) == null)
                return fault.RaiseFault(header, WsExceptionFaultType.XmlException, 
                    "Body/TwoWay X value is missing.");
            int X = Convert.ToInt32(tempNode.Value);
            if ((tempNode = envelope.SelectSingleNode("Body/TwoWayRequest/Y", false)) == null)
                return fault.RaiseFault(header, WsExceptionFaultType.XmlException, 
                    "Body/TwoWay Y value is missing.");
            int Y = Convert.ToInt32(tempNode.Value);
            Debug.Print("");
            Debug.Print("X = " + X.ToString() + " Y = " + Y.ToString());
            Debug.Print(X.ToString() + " + " + Y.ToString() + " = " + ((int)(X + Y)).ToString());
            Debug.Print("");

            return TwoWayResponse(header, X + Y);
        }

        public byte[] TwoWayResponse(WsWsaHeader header, int sum)
        {
            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            // Write processing instructions and root element
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("soap", "Envelope", 
                "http://www.w3.org/2003/05/soap-envelope");

            // Write namespaces
            xmlWriter.WriteAttributeString("xmlns", "wsdp", null, 
                Device.Namespaces.GetNamespace("wsdp"));
            xmlWriter.WriteAttributeString("xmlns", "wsd", null, 
                Device.Namespaces.GetNamespace("wsd"));
            xmlWriter.WriteAttributeString("xmlns", "wsa", null, 
                Device.Namespaces.GetNamespace("wsa"));
            xmlWriter.WriteAttributeString("xmlns", "sim", null, 
                TypeNamespaces.GetNamespace("sim"));

            // Write header
            xmlWriter.WriteStartElement("soap", "Header", null);
            xmlWriter.WriteStartElement("wsa", "To", null);
            xmlWriter.WriteString(header.From);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("wsa", "Action", null);
            xmlWriter.WriteString("http://schemas.example.org/SimpleService/TwoWayResponse");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("wsa", "RelatesTo", null);
            xmlWriter.WriteString(header.MessageID);
            xmlWriter.WriteEndElement(); // End RelatesTo
            xmlWriter.WriteStartElement("wsa", "MessageID", null);
            xmlWriter.WriteString("urn:uuid:" + Device.GetUuid());
            xmlWriter.WriteEndElement(); // End MessageID
            xmlWriter.WriteEndElement(); // End Header

            // write body
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
    }
}
