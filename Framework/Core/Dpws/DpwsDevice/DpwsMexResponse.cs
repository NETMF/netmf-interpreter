using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using Dpws.Device;
using Dpws.Device.Services;
using Ws.Services.WsaAddressing;

using System.Ext;
using System.Ext.Xml;
using Ws.Services.Xml;
using Ws.Services;
using Ws.Services.Soap;
using Ws.Services.Discovery;

namespace Dpws.Device.Mex
{
    internal class DpwsWsxMetdataResponse
    {
        private ProtocolVersion m_version;

        //--//

        public DpwsWsxMetdataResponse(ProtocolVersion version)
        {
            m_version = version;
        }

        public WsMessage GetResponse(WsMessage req)
        {
            XmlReader   reader = req.Reader;
            WsWsaHeader header = req.Header;

            // Build ProbeMatch
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                // Write service type namespaces
                WsXmlNamespaces additionalPrefixes = new WsXmlNamespaces();
                if (Device.Host != null)
                {
                    additionalPrefixes = new WsXmlNamespaces();
                    additionalPrefixes.Add(Device.Host.ServiceNamespace);
                }

                WsServiceEndpoints hostedServices = Device.HostedServices;
                int count = hostedServices.Count;
                for (int i = 0; i < count; i++)
                {
                    DpwsHostedService hostedService = (DpwsHostedService)hostedServices[i];

                    // Don't return Mex Service namespace
                    if (hostedService.ServiceTypeName == "Internal")
                        continue;

                    additionalPrefixes.Add(hostedService.ServiceNamespace);
                }

                WsWsaHeader responseHeader = new WsWsaHeader(
                    WsWellKnownUri.WstNamespaceUri + "/GetResponse",                        // Action
                    header.MessageID,                                                       // RelatesTo
                    header.ReplyTo.Address.AbsoluteUri,                                     // To
                    null, null, null);

                WsMessage resp = new WsMessage(responseHeader, null, WsPrefix.Wsx | WsPrefix.Wsdp, additionalPrefixes,
                    new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID));

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                smw.WriteSoapMessageStart(xmlWriter, resp);

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsx, "Metadata", null);

                // Write ThisModel metadata section
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsx, "MetadataSection", null);
                xmlWriter.WriteAttributeString("Dialect", m_version.WsdpNamespaceUri + "/ThisModel");

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ThisModel", null);
                if (Device.ThisModel.Manufacturer != null && Device.ThisModel.Manufacturer != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "Manufacturer", null);
                    xmlWriter.WriteString(Device.ThisModel.Manufacturer);
                    xmlWriter.WriteEndElement(); // End Manufacturer
                }

                if (Device.ThisModel.ManufacturerUrl != null && Device.ThisModel.ManufacturerUrl != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ManufacturerUrl", null);
                    xmlWriter.WriteString(Device.ThisModel.ManufacturerUrl);
                    xmlWriter.WriteEndElement(); // End ManufacturerUrl
                }

                if (Device.ThisModel.ModelName != null && Device.ThisModel.ModelName != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ModelName", null);
                    xmlWriter.WriteString(Device.ThisModel.ModelName);
                    xmlWriter.WriteEndElement(); // End ModelName
                }

                if (Device.ThisModel.ModelNumber != null && Device.ThisModel.ModelNumber != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ModelNumber", null);
                    xmlWriter.WriteString(Device.ThisModel.ModelNumber);
                    xmlWriter.WriteEndElement(); // End ModelNumber
                }

                if (Device.ThisModel.ModelUrl != null && Device.ThisModel.ModelUrl != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ModelUrl", null);
                    xmlWriter.WriteString(Device.ThisModel.ModelUrl);
                    xmlWriter.WriteEndElement(); // End ModelUrl
                }

                if (Device.ThisModel.PresentationUrl != null && Device.ThisModel.PresentationUrl != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "PresentationUrl", null);
                    xmlWriter.WriteString(Device.ThisModel.PresentationUrl);
                    xmlWriter.WriteEndElement(); // End PresentationUrl
                }

                if (Device.ThisModel.Any != null)
                {
                    Device.ThisModel.Any.WriteTo(xmlWriter);
                }

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ModelName", null);
                xmlWriter.WriteString(Device.ThisModel.ModelName);
                xmlWriter.WriteEndElement(); // End ModelName

                xmlWriter.WriteEndElement(); // End ThisModel
                xmlWriter.WriteEndElement(); // End MetadataSection

                // Write ThisDevice metadata section
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsx, "MetadataSection", null);
                xmlWriter.WriteAttributeString("Dialect", m_version.WsdpNamespaceUri + "/ThisDevice");

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ThisDevice", null);
                if (Device.ThisDevice.FriendlyName != null && Device.ThisDevice.FriendlyName != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "FriendlyName", null);
                    xmlWriter.WriteString(Device.ThisDevice.FriendlyName);
                    xmlWriter.WriteEndElement(); // End FriendlyName
                }

                if (Device.ThisDevice.FirmwareVersion != null && Device.ThisDevice.FirmwareVersion != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "FirmwareVersion", null);
                    xmlWriter.WriteString(Device.ThisDevice.FirmwareVersion);
                    xmlWriter.WriteEndElement(); // End FirmwareVersion
                }

                if (Device.ThisDevice.SerialNumber != null && Device.ThisDevice.SerialNumber != "")
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "SerialNumber", null);
                    xmlWriter.WriteString(Device.ThisDevice.SerialNumber);
                    xmlWriter.WriteEndElement(); // End SerialNumber
                }

                if (Device.ThisDevice.Any != null)
                {
                    Device.ThisDevice.Any.WriteTo(xmlWriter);
                }

                xmlWriter.WriteEndElement(); // End ThisDevice
                xmlWriter.WriteEndElement(); // End MetadataSection

                // Write next MetadataSection
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsx, "MetadataSection", null);
                xmlWriter.WriteAttributeString("Dialect", m_version.WsdpNamespaceUri + "/Relationship");

                // Write Relationship Elements
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "Relationship", null);
                xmlWriter.WriteAttributeString("Type", m_version.WsdpNamespaceUri + "/host");

                // List used to maintain service endpoints that have been processed. Because the DPWS spec allows
                // for multiple service types at a single endpoint address, we must make sure we only create
                // a relationship once for all of the types at a service endpoint.
                ArrayList processedEndpointList = new ArrayList();

                // If a Host type exist add it
                if (Device.Host != null)
                {
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "Host", null);
                    WsWsaEndpointRef endpointReference;
                    endpointReference = (WsWsaEndpointRef)Device.Host.EndpointRefs[0];
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "EndpointReference", null);
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                    xmlWriter.WriteString(endpointReference.Address.AbsoluteUri);
                    xmlWriter.WriteEndElement(); // End Address
                    xmlWriter.WriteEndElement(); // End EndpointReference

                    // Build list of all service types that share this endpoint address
                    /*
                    string serviceTypes = null;
                    if ((serviceTypes = BuildServiceTypesList(Device.Host, processedEndpointList)) == null)
                        serviceTypes = Device.Host.ServiceNamespace.Prefix + ":" + Device.Host.ServiceTypeName;
                    else
                        serviceTypes = serviceTypes + " " + Device.Host.ServiceNamespace.Prefix + ":" + Device.Host.ServiceTypeName;
                    */
                    
                    // Write service types
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "Types", null);
                    xmlWriter.WriteString(Device.Host.ServiceNamespace.Prefix + ":" + Device.Host.ServiceTypeName);
                    xmlWriter.WriteEndElement(); // End Types

                    // Write Service ID
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ServiceId", null);
                    xmlWriter.WriteString(Device.Host.ServiceID);
                    xmlWriter.WriteEndElement(); // End ServiceID

                    xmlWriter.WriteEndElement(); // End Hosted

                    // Update processed endpoint list
                    processedEndpointList.Add(Device.Host.EndpointAddress);
                }

                // Add hosted services types
                int serviceCount = hostedServices.Count;
                DpwsHostedService currentService;
                for (int i = 0; i < serviceCount; i++)
                {
                    currentService = (DpwsHostedService)hostedServices[i];

                    // Don't return Mex Service type
                    if (currentService.ServiceTypeName == "Internal")
                        continue;

                    // Build list of all service types that share this endpoint address
                    string serviceTypes = null;
                    if ((serviceTypes = BuildServiceTypesList(currentService, processedEndpointList)) == null)
                        continue;

                    // Write hosted start element
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "Hosted", null);
                    
                    // Write n number of endpoint references
                    int epRefCount = currentService.EndpointRefs.Count;
                    for (int j = 0; j < epRefCount; j++)
                    {
                        xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "EndpointReference", null);
                        xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                        xmlWriter.WriteString(currentService.EndpointRefs[j].Address.AbsoluteUri);
                        xmlWriter.WriteEndElement(); // End Address
                        xmlWriter.WriteEndElement(); // End EndpointReference
                    }

                    // Write service types
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "Types", null);
                    xmlWriter.WriteString(currentService.ServiceNamespace.Prefix + ":" + currentService.ServiceTypeName);
                    xmlWriter.WriteEndElement(); // End Types

                    // Write Service ID
                    xmlWriter.WriteStartElement(WsNamespacePrefix.Wsdp, "ServiceId", null);
                    xmlWriter.WriteString(currentService.ServiceID);
                    xmlWriter.WriteEndElement(); // End ServiceID

                    xmlWriter.WriteEndElement(); // End Hosted

                    // Update processed endpoint list
                    processedEndpointList.Add(currentService.EndpointAddress);
                }

                xmlWriter.WriteEndElement(); // End Relastionship
                xmlWriter.WriteEndElement(); // End MetadataSection

                xmlWriter.WriteEndElement(); // End Metadata

                smw.WriteSoapMessageEnd(xmlWriter);

                resp.Body = xmlWriter.ToArray();

                return resp;
            }
        }

        // Builds Service Types list. This hack is required because service spec writers determined that
        // more than one service type can live at a single endpoint address. Why you would want to break
        // the object model and allow this feature is unknown so for now we must hack.
        private string BuildServiceTypesList(DpwsHostedService service, ArrayList processedEndpointList)
        {
            if (processedEndpointList.Contains(service.EndpointAddress))
                return null;
            else
                processedEndpointList.Add(service.EndpointAddress);

            int serviceCount = Device.HostedServices.Count;
            string serviceTypes = null;
            for (int i = 0; i < serviceCount; ++i)
            {
                if (((DpwsHostedService)Device.HostedServices[i]).ServiceTypeName == "Internal")
                    continue;

                if (service.EndpointAddress == Device.HostedServices[i].EndpointAddress)
                    serviceTypes = serviceTypes +
                        ((serviceTypes == null) ? "" : " ") +
                        ((DpwsHostedService)Device.HostedServices[i]).ServiceNamespace.Prefix + ":" +
                        ((DpwsHostedService)Device.HostedServices[i]).ServiceTypeName;
            }

            return serviceTypes;
        }
    }
}


