using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;
using Dpws.Device;
using Dpws.Device.Services;
using Ws.Services.Utilities;
using Ws.Services.WsaAddressing;
using Ws.Services.Discovery;

using System.Ext;
using System.Ext.Xml;
using Ws.Services;
using Ws.Services.Faults;
using Ws.Services.Xml;
using Ws.Services.Soap;
using Ws.Services.Transport;

namespace Dpws.Device.Discovery
{
    internal class DpwsDeviceDiscovery
    {
        public readonly ProtocolVersion Version;

        //--//
        
        public DpwsDeviceDiscovery(ProtocolVersion v)
        {
            this.Version = v;
        }
        
        public virtual WsMessage ProbeMatch(WsMessage probe, DpwsHostedService matchedService)
        {
            XmlReader   reader = probe.Reader;
            WsWsaHeader header = probe.Header;

            // Performance debugging
            DebugTiming timeDebuger = new DebugTiming();
            long startTime = timeDebuger.ResetStartTime("");

            // Build ProbeMatch
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                // If a Host exist write the Host namespace
                WsXmlNamespaces additionalPrefixes = null;
                if (Device.Host != null)
                {
                    additionalPrefixes = new WsXmlNamespaces();
                    additionalPrefixes.Add(Device.Host.ServiceNamespace);
                }

                WsWsaHeader matchHeader = new WsWsaHeader(
                    this.Version.DiscoveryNamespace + "/ProbeMatches", // Action
                    header.MessageID,                                  // RelatesTo
                    this.Version.AnonymousUri,                         // To
                    null, null, null);                                 // ReplyTo, From, Any

                WsMessage msg = new WsMessage(matchHeader, null, WsPrefix.Wsd | WsPrefix.Wsdp, additionalPrefixes,
                    new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID));

                WsSoapMessageWriter smw = new WsSoapMessageWriter(this.Version);
                smw.WriteSoapMessageStart(xmlWriter, msg);

                // Performance debuging
                timeDebuger.PrintElapsedTime("*****Write Header Took");

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "ProbeMatches", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "ProbeMatch", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "EndpointReference", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                xmlWriter.WriteString(matchedService == null ? Device.EndpointAddress : matchedService.EndpointAddress);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                // Write hosted service types
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "Types", null);
                WriteDeviceServiceTypes(xmlWriter);
                xmlWriter.WriteEndElement(); // End Types

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "XAddrs", null);

                string transport = Device.TransportAddress;
                if(matchedService != null)
                {
                    int idx = transport.LastIndexOf('/');
                    if(idx != -1)
                    {
                        transport = transport.Substring(0, idx + 1);
                        transport += matchedService.EndpointAddress.Substring(matchedService.EndpointAddress.IndexOf("urn:uuid:") + 9);
                    }
                }

                int idx2 = transport.ToLower().IndexOf("localhost");

                if(idx2 != -1)
                {
                    transport = transport.Substring(0, idx2) + WsNetworkServices.GetLocalIPV4Address() + transport.Substring(idx2 + 9 /*localhost*/);
                }
                
                xmlWriter.WriteString(transport);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "MetadataVersion", null);
                xmlWriter.WriteString(Device.MetadataVersion.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                smw.WriteSoapMessageEnd(xmlWriter);

                msg.Body = xmlWriter.ToArray();

                // Performance debuging
                timeDebuger.PrintTotalTime(startTime, "***ProbeMatch Took");

                // Delay probe match as per Ws-Discovery specification (2.4 Protocol Assignments)
                Thread.Sleep(new Random().Next(Device.ProbeMatchDelay));

                // Return stream buffer
                return msg;
            }
        }

        public virtual WsMessage ResolveMatch(WsMessage message)
        {
            XmlReader   reader = message.Reader;
            WsWsaHeader header = message.Header;
            bool        match  = false;
            string      epAddr = "";

            reader.ReadStartElement("Resolve", this.Version.DiscoveryNamespace);

            
            if(reader.IsStartElement("EndpointReference", this.Version.AddressingNamespace) == false)
            {
                return null;
            }

            WsWsaEndpointRef epRef = new WsWsaEndpointRef(reader, this.Version.AddressingNamespace);

            epAddr = epRef.Address.AbsoluteUri;

            if(Device.EndpointAddress != epAddr)
            {
                // If the destination endpoint is ours send a resolve match else return null
                int servicesCount = Device.HostedServices.Count;
                DpwsHostedService hostedService;
                for (int i = 0; i < servicesCount; i++)
                {
                    hostedService = (DpwsHostedService)Device.HostedServices[i];
                    // Skip internal services
                    if (hostedService.ServiceTypeName == "Internal")
                        continue;
                
                    if (hostedService.EndpointAddress == epAddr)
                    {
                        match = true;
                        break;
                    }
                }

                if (!match)
                    return null;
            }

            // Build ResolveMatch
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                // If a Host exist write the Host namespace
                WsXmlNamespaces additionalPrefixes = null;
                if (Device.Host != null)
                {
                    additionalPrefixes = new WsXmlNamespaces();
                    additionalPrefixes.Add(Device.Host.ServiceNamespace);
                }

                WsWsaHeader matchHeader = new WsWsaHeader(
                    this.Version.DiscoveryNamespace + "/ResolveMatches",  // Action
                    header.MessageID,                                     // RelatesTo
                    this.Version.AnonymousUri,                            // To
                    null, null, null);                                    // ReplyTo, From, Any

                WsMessage msg = new WsMessage(matchHeader, null, WsPrefix.Wsd | WsPrefix.Wsdp, additionalPrefixes, 
                    new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID));

                WsSoapMessageWriter smw = new WsSoapMessageWriter(this.Version);
                smw.WriteSoapMessageStart(xmlWriter, msg);

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "ResolveMatches", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "ResolveMatch", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "EndpointReference", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                xmlWriter.WriteString(epRef.Address.AbsoluteUri);
                xmlWriter.WriteEndElement(); // End Address
                xmlWriter.WriteEndElement(); // End EndpointReference

                // Write hosted service types
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "Types", null);
                WriteDeviceServiceTypes(xmlWriter);
                xmlWriter.WriteEndElement(); // End Types

                string transport = Device.TransportAddress;
                if(match)
                {
                    int idx = transport.LastIndexOf('/');
                    if(idx != -1)
                    {
                        transport = transport.Substring(0, idx + 1);
                        transport += epAddr.Substring(epAddr.IndexOf("urn:uuid:") + 9);
                    }
                }

                int idx2 = transport.ToLower().IndexOf("localhost");

                if(idx2 != -1)
                {
                    transport = transport.Substring(0, idx2) + WsNetworkServices.GetLocalIPV4Address() + transport.Substring(idx2 + 9 /*localhost*/);
                }

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "XAddrs", null);
                xmlWriter.WriteString(transport);
                xmlWriter.WriteEndElement(); // End XAddrs

                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsd, "MetadataVersion", null);
                xmlWriter.WriteString(Device.MetadataVersion.ToString());
                xmlWriter.WriteEndElement(); // End MetadataVersion

                xmlWriter.WriteEndElement(); // End ResolveMatch
                xmlWriter.WriteEndElement(); // End ResolveMatches

                smw.WriteSoapMessageEnd(xmlWriter);

                msg.Body = xmlWriter.ToArray();

                // Return stream buffer
                return msg;
            }
        }

        // Write Service Types list.
        private void WriteDeviceServiceTypes(XmlWriter xmlWriter)
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
        }
    }

}


