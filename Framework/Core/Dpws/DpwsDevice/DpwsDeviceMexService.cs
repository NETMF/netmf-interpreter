using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using Dpws.Device;
using Dpws.Device.Services;
using Ws.Services;
using Ws.Services.WsaAddressing;
using Ws.Services.Xml;
using Ws.Services.Soap;
using Ws.Services.Discovery;
using Ws.Services.Faults;

namespace Dpws.Device.Mex
{

    // This is a Hosted Service class used by the Device to provide DPWS compliant metadata services.
    internal class DpwsDeviceMexService : DpwsHostedService
    {
        private DpwsWsxMetdataResponse m_response;
        
        
        // Creates an instance of the metadata services class.
        public DpwsDeviceMexService(ProtocolVersion v) : base(v)
        {
            Init();

            m_response = new DpwsWsxMetdataResponse(v);
        }

        // Add MEX namespaces, set the MEX address = to the device address, and adds the
        // Mex service to the internal device hosted service collection.
        private void Init()
        {

            // Set the service type name to internal to hide from discovery services
            ServiceTypeName = "Internal";

            // Set service namespace
            ServiceNamespace = new WsXmlNamespace(WsNamespacePrefix.Wsx, WsWellKnownUri.WsxNamespaceUri);

            // Set the endpoint address
            EndpointAddress = Device.EndpointAddress;

            // Add Discovery service operations
            ServiceOperations.Add(new WsServiceOperation(WsWellKnownUri.WstNamespaceUri, "Get"));
        }

        // MetadataExchange GetResponse service stub
        public WsMessage Get(WsMessage msg)
        {
            WsWsaHeader header = msg.Header;

            // DPWS 1.1 spec: R0031:  A SERVICE MUST NOT generate a wsa:InvalidAddressingHeader SOAP Fault 
            // [WS-Addressing SOAP Binding] if the [address] of the [reply endpoint] of an HTTP Request 
            // Message SOAP ENVELOPE is "http://www.w3.org/2005/08/addressing/anonymous".
            if (header.ReplyTo != null && header.ReplyTo.Address.AbsoluteUri != m_version.AnonymousUri && 
                header.ReplyTo.Address.AbsoluteUri != m_version.AnonymousRoleUri)
            {
                throw new WsFaultException(header, WsFaultType.WsaInvalidMessageInformationHeader, "R0031");
            }

            return m_response.GetResponse(msg);
        }

    }

}


