using System;
using System.Collections;
using System.Text;
using Ws.Services.Soap;
using Ws.Services.Transport.HTTP;
using Ws.Services.Utilities;

using System.Ext;
using System.Ext.Xml;
using System.IO;
using System.Xml;
using Ws.Services;
using Ws.Services.WsaAddressing;
using Microsoft.SPOT;

namespace Dpws.Client.Discovery
{
    /// <summary>
    /// Used to get a Dpws services metadata.
    /// </summary>
    public class DpwsMexClient
    {
        private readonly ProtocolVersion m_version;

        //--//

        /// <summary>
        /// Creates an instance of a DpwsMexClient class.
        /// </summary>
        public DpwsMexClient(ProtocolVersion version)
        {
            m_version = version;
        }

        /// <summary>
        /// Use to request metadata from a devices hosted service endpoint.
        /// </summary>
        /// <param name="serviceAddress">
        /// A string containing the transport address of a service endpoint. For Dpws the address represents
        /// a devices transport address.
        /// For example: http://192.168.0.1:8084/3cb0d1ba-cc3a-46ce-b416-212ac2419b20
        /// </param>
        /// <returns>
        /// A collection of DpwsMetadata objects containing endpoint details about services hosted by a device.
        /// </returns>
        public DpwsMetadata Get(string serviceAddress)
        {
            // Convert the address string to a Uri
            Uri serviceUri = null;
            try
            {
                serviceUri = new Uri(serviceAddress);
                if (serviceUri.Scheme != "http")
                {
                    System.Ext.Console.Write("");
                    System.Ext.Console.Write("Invalid serviceAddress. Must be a Uri. Http Uri schemes only.");
                    System.Ext.Console.Write("");
                    return null;
                }
            }
            catch (Exception e)
            {
                System.Ext.Console.Write("");
                System.Ext.Console.Write(e.Message);
                System.Ext.Console.Write("");
                return null;
            }

            // Performance debugging
            DebugTiming timeDebuger = new DebugTiming();
            long startTime = timeDebuger.ResetStartTime("");

            // Build Get Request
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsWsaHeader header = new WsWsaHeader(
                    WsWellKnownUri.WstNamespaceUri + "/Get",            // Action
                    null,                                               // RelatesTo
                    "urn:uuid:" + serviceUri.AbsolutePath.Substring(1), // To
                    m_version.AnonymousUri,                             // ReplyTo
                    null, null);                                        // From, Any

                WsMessage msg = new WsMessage(header, null, WsPrefix.None, null, null);

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                String messageID = smw.WriteSoapMessageStart(xmlWriter, msg);

                // Performance debuging
                timeDebuger.PrintElapsedTime("*****Write Header Took");

                // Performance debuging
                timeDebuger.PrintElapsedTime("*****Write Body Took");

                smw.WriteSoapMessageEnd(xmlWriter);

                // Performance debuging
                timeDebuger.PrintTotalTime(startTime, "***Get Message Build Took");

                // Create an Http client and send Get request
                WsHttpClient httpClient = new WsHttpClient(m_version);

                System.Ext.Console.Write("");
                System.Ext.Console.Write("Sending Get to: " + serviceAddress);

                msg.Body = xmlWriter.ToArray();

                WsMessage getResponse = httpClient.SendRequest(msg, new Uri(serviceAddress));

                // If a get response is received process it and return DpwsMetadata object
                DpwsMetadata metadata = null;
                if (getResponse == null || getResponse.Body == null)
                {
                    return null;
                }
                else
                {
                    DpwsDiscoClientProcessor soapProcessor = new DpwsDiscoClientProcessor(m_version);
                    try
                    {
                        System.Ext.Console.Write(getResponse.Body as byte[]);
                        System.Ext.Console.Write("");

                        metadata = soapProcessor.ProcessGetResponse((byte[])getResponse.Body, messageID);
                    }
                    catch (Exception e)
                    {
                        System.Ext.Console.Write("");
                        System.Ext.Console.Write("Get response parser threw an exception. " + e.Message);
                        return null;
                    }
                }

                return metadata;
            }
        }
    }

    /// <summary>
    /// Class used to store a devices ThisModel information.
    /// </summary>
    public class DpwsThisModel
    {
        /// <summary>
        /// Name of the manufacturer of a device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
        public readonly String Manufacturer;

        /// <summary>
        /// Url to a Web site of the manufacturer of a device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_URI_SIZE octets.</remarks>
        public readonly String ManufacturerUrl;

        /// <summary>
        /// User friendly name for a model of device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
        public readonly String ModelName;

        /// <summary>
        /// Model number for a model of device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
        public readonly String ModelNumber;

        /// <summary>
        /// URL to a Web site for a model of device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_URI_SIZE octets.</remarks>
        public readonly String ModelUrl;

        /// <summary>
        /// URL to an Html page for a device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_URI_SIZE octets.</remarks>
        public readonly String PresentationUrl;

        internal DpwsThisModel(XmlReader reader, ProtocolVersion version)
        {
            reader.ReadStartElement("ThisModel", version.WsdpNamespaceUri);

            this.Manufacturer = reader.ReadElementString("Manufacturer", version.WsdpNamespaceUri);
            while (reader.IsStartElement("Manufacturer", version.WsdpNamespaceUri))
            {
                reader.Skip(); // ignore the other localized name for now until we have a better loc. story
            }

            if (reader.IsStartElement("ManufacturerUrl", version.WsdpNamespaceUri))
            {
                this.ManufacturerUrl = reader.ReadElementString();
            }

            this.ModelName = reader.ReadElementString("ModelName", version.WsdpNamespaceUri);
            while (reader.IsStartElement("ModelName", version.WsdpNamespaceUri))
            {
                reader.Skip(); // ignore the other localized name for now until we have a better loc. story
            }

            if (reader.IsStartElement("ModelNumber", version.WsdpNamespaceUri))
            {
                this.ModelNumber = reader.ReadElementString();
            }

            if (reader.IsStartElement("ModelUrl", version.WsdpNamespaceUri))
            {
                this.ModelUrl = reader.ReadElementString();
            }

            if (reader.IsStartElement("PresentationUrl", version.WsdpNamespaceUri))
            {
                this.PresentationUrl = reader.ReadElementString();
            }

            XmlReaderHelper.SkipAllSiblings(reader); // xs:any

            reader.ReadEndElement(); // ThisModel
        }
    }

    /// <summary>
    /// A class used to store a devices ThisDevice metadata.
    /// </summary>
    public class DpwsThisDevice
    {
        /// <summary>
        /// User-friendly name for a device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
        public readonly String FriendlyName;

        /// <summary>
        /// Firmware version for a device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
        public readonly String FirmwareVersion;

        /// <summary>
        /// Manufacturer assigned serial number for a device.
        /// </summary>
        /// <remarks>Must have fewer than MAX_FIELD_SIZE characters.</remarks>
        public readonly String SerialNumber;

        internal DpwsThisDevice(XmlReader reader, ProtocolVersion version)
        {
            reader.ReadStartElement("ThisDevice", version.WsdpNamespaceUri);

            this.FriendlyName = reader.ReadElementString("FriendlyName", version.WsdpNamespaceUri);
            while (reader.IsStartElement("FriendlyName", version.WsdpNamespaceUri))
            {
                reader.Skip(); // ignore the other localized name for now until we have a better loc. story
            }

            if (reader.IsStartElement("FirmwareVersion", version.WsdpNamespaceUri))
            {
                this.FirmwareVersion = reader.ReadElementString();
            }

            if (reader.IsStartElement("SerialNumber", version.WsdpNamespaceUri))
            {
                this.SerialNumber = reader.ReadElementString();
            }

            XmlReaderHelper.SkipAllSiblings(reader); // xs:any

            reader.ReadEndElement(); // ThisDevice
        }
    }

    /// <summary>
    /// A class used to store Relationship metadata.
    /// </summary>
    public class DpwsRelationship
    {
        private DpwsMexService _host;

        /// <summary>
        /// Used to store a metadata Relationship service Host object.
        /// </summary>
        public DpwsMexService Host { get { return _host; } }

        /// <summary>
        /// A collection used to store metadata Relationship Hosted Service objects.
        /// </summary>
        public readonly DpwsMexServices HostedServices;

        public DpwsRelationship(XmlReader reader, ProtocolVersion version)
        {
            this.HostedServices = new DpwsMexServices();
            Append(reader, version);
        }

        internal void Append(XmlReader reader, ProtocolVersion version)
        {
            if (reader.IsStartElement("Relationship", version.WsdpNamespaceUri) == false)
            {
                throw new XmlException();
            }

            reader.MoveToAttribute("Type");
            if (reader.Value != version.WsdpNamespaceUri + "/host")
            {
                throw new XmlException(); // Incorrect Type Attribute
            }

            reader.MoveToElement();
            reader.Read(); // Relationship - Start Element

            if (reader.IsStartElement("Host", version.WsdpNamespaceUri))
            {
                if (_host != null)
                {
                    throw new XmlException(); // An host has already been defined
                }

                _host = new DpwsMexService(reader, version);
            }

            while (reader.IsStartElement("Hosted", version.WsdpNamespaceUri))
            {
                this.HostedServices.Add(new DpwsMexService(reader, version));
            }

            if (this.Host == null && this.HostedServices.Count == 0)
            {
                throw new XmlException();
            }

            XmlReaderHelper.SkipAllSiblings(reader); // xs:any

            reader.ReadEndElement(); // Relationship
        }
    }

    /// <summary>
    /// Base class used to store metadata service information about a host or Hosted service.
    /// </summary>
    public class DpwsMexService
    {
        /// <summary>
        /// A collection of service types.
        /// </summary>
        public readonly DpwsServiceTypes ServiceTypes;
        /// <summary>
        /// A Uri containing a services transport address.
        /// </summary>
        public readonly WsWsaEndpointRefs EndpointRefs;
        /// <summary>
        /// A string containing a services ID.
        /// </summary>
        public readonly String ServiceID;

        public DpwsMexService(XmlReader reader, ProtocolVersion version)
        {
            reader.ReadStartElement();

            this.EndpointRefs = new WsWsaEndpointRefs();

            while (reader.IsStartElement("EndpointReference", version.AddressingNamespace))
            {
                this.EndpointRefs.Add(new WsWsaEndpointRef(reader, version.AddressingNamespace));
            }

            if (EndpointRefs.Count == 0)
            {
                throw new XmlException(); // must have at least one EndpointReference
            }

            if (reader.IsStartElement("Types", version.WsdpNamespaceUri))
            {
                this.ServiceTypes = new DpwsServiceTypes(reader);
            }

            this.ServiceID = reader.ReadElementString("ServiceId", version.WsdpNamespaceUri);

            XmlReaderHelper.SkipAllSiblings(reader); // xs:any

            reader.ReadEndElement();
        }
    }

    /// <summary>
    /// A collection of DpwsMexServices metadata elements.
    /// </summary>
    /// <remarks>
    /// This class is used by DpwsMetadata to store a collection of DpwsMexServices objects.
    /// DpwsMexServices objects contain details about a hosted service endpoint.
    /// </remarks>
    public class DpwsMexServices
    {
        private object    m_threadLock;
        private ArrayList m_services;

        /// <summary>
        /// Creates an instance of a DpwsMexServices class.
        /// </summary>
        internal DpwsMexServices()
        {
            m_threadLock = new object();
            m_services   = new ArrayList();
        }

        /// <summary>
        /// Use to Get the number of DpwsMexServices elements actually contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_services.Count;
            }
        }

        /// <summary>
        /// Use to Get or set the DpwsMexServices element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the DpwsMexServices element to get or set.
        /// </param>
        /// <returns>
        /// An instance of a DpwsMexServices element.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index is less than zero.-or- index is equal to or greater than the collection count.
        /// </exception>
        public DpwsMexService this[int index]
        {
            get
            {
                return (DpwsMexService)m_services[index];
            }
        }

        public DpwsMexService this[Uri address]
        {
            get
            {
                lock (m_threadLock)
                {
                    DpwsMexService service;
                    int count = m_services.Count;
                    for (int i = 0; i < count; i++)
                    {
                        service = (DpwsMexService)m_services[i];
                        if (service.EndpointRefs[address] != null)
                        {
                            return service;
                        }
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Adds a DpwsMexServices to the end of the collection.
        /// </summary>
        /// <param name="value">
        /// The DpwsMexServices element to be added to the end of the collection.
        /// The value can be null.
        /// </param>
        /// <returns>
        /// The collection index at which the DpwsMexServices has been added.
        /// </returns>
        public int Add(DpwsMexService value)
        {
            lock (m_threadLock)
            {
                return m_services.Add(value);
            }
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            lock (m_threadLock)
            {
                m_services.Clear();
            }
        }
    }

    /// <summary>
    /// Used to store service endpoint metadata details aquired from a Get metadata response.
    /// </summary>
    public class DpwsMetadata
    {
        /// <summary>
        /// Creates an instance of a DpwsMetadata class.
        /// </summary>
        public DpwsMetadata(XmlReader reader, ProtocolVersion version)
        {
            reader.ReadStartElement("Metadata", WsWellKnownUri.WsxNamespaceUri);

            while (reader.IsStartElement("MetadataSection", WsWellKnownUri.WsxNamespaceUri))
            {
                reader.MoveToAttribute("Dialect");
                String dialect = reader.Value;
                reader.MoveToElement();
                reader.Read(); // MetadataSection

                if (dialect == version.WsdpNamespaceUri + "/ThisModel")
                {
#if DEBUG
                    int depth = reader.Depth;
#endif
                    this.ThisModel = new DpwsThisModel(reader, version);
#if DEBUG
                    Microsoft.SPOT.Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif
                }
                else if (dialect == version.WsdpNamespaceUri + "/ThisDevice")
                {
#if DEBUG
                    int depth = reader.Depth;
#endif
                    this.ThisDevice = new DpwsThisDevice(reader, version);
#if DEBUG
                    Microsoft.SPOT.Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif
                }
                else if (dialect == version.WsdpNamespaceUri + "/Relationship")
                {
#if DEBUG
                    int depth = reader.Depth;
#endif
                    if (this.Relationship == null)
                    {
                        this.Relationship = new DpwsRelationship(reader, version);
                    }
                    else
                    {
                        this.Relationship.Append(reader, version);
                    }

#if DEBUG
                    Microsoft.SPOT.Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif
                }
                else // known dialect
                {
                    reader.Skip();
                }

                reader.ReadEndElement(); // MetadataSection
            }

            if (this.ThisModel == null || this.ThisDevice == null || this.Relationship == null)
            {
                // Metadata must include ThisModel (R2038, R2012), ThisDevice(R2039, R2014),
                // at least one Relationship(R2040, R2029)
                throw new XmlException();
            }

            XmlReaderHelper.SkipAllSiblings(reader); // xs:any

            reader.ReadEndElement(); // Metadata
        }

        /// <summary>
        /// Use to get an object containing model specific information for the device this metadata object contains.
        /// </summary>
        public readonly DpwsThisModel ThisModel;

        /// <summary>
        /// Use to get an object containing device specific information for the device this metadata object contains.
        /// </summary>
        public readonly DpwsThisDevice ThisDevice;

        public readonly DpwsRelationship Relationship;
    }
}


