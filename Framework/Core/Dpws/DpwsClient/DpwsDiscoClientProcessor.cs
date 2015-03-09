using System;
using System.Net;
using System.Xml;
using Ws.Services.Soap;
using Ws.Services.Utilities;
using Ws.Services.Discovery;
using Ws.Services.WsaAddressing;
using Ws.Services.Xml;

using System.Ext;
using Ws.Services;
using Microsoft.SPOT;
using Ws.Services.Transport;

namespace Dpws.Client.Discovery
{
    /// <summary>
    /// This class contains methods that process Ws-Discovery messages. Discovery response messages are passed
    /// to these methods where they are parsed into discovery objects used by client implementations.
    /// </summary>
    class DpwsDiscoClientProcessor
    {
        private readonly ProtocolVersion m_version;
        
        //--//

        /// <summary>
        /// Creates an instance of a DpwsDiscoClientProcessor class.
        /// </summary>
        public DpwsDiscoClientProcessor(ProtocolVersion version)
        {
            m_version = version;
        }

        /// <summary>
        /// Parses a ProbeMatches message.
        /// </summary>
        /// <param name="probeResponse">A byte array containing a probe response soap message.</param>
        /// <param name="messageID">
        /// A string containing the message ID of the original request. This is used to make insure this is
        /// a response to the original request.
        /// </param>
        /// <param name="remoteEP">
        /// A IPEndPoint containing the remote address of the service that sent this response.
        /// </param>
        /// <param name="messageCheck">
        /// A WsMessageCheck object ued to test for a duplicate request message.
        /// If null no check is performed as would be the case for a directed probe response.
        /// </param>
        /// <returns>
        /// A DpwsProbeMatch object containing a service endpoint description returned by a service.
        /// </returns>
        /// <exception cref="XmlException">If required tags are missing or invalid namespaces are detected.</exception>
        public DpwsServiceDescriptions ProcessProbeMatch(WsMessage probeResponse, string messageID, IPEndPoint remoteEP, WsMessageCheck messageCheck)
        {
            return ProcessMatch(DpwsServiceDescription.ServiceDescriptionType.ProbeMatch, probeResponse, messageID, remoteEP, messageCheck);
        }

        /// <summary>
        /// Parses a ResolveMatches message.
        /// </summary>
        /// <param name="resolveResponse">A byte array containing a resolve response soap message.</param>
        /// <param name="messageID">
        /// A string containing the message ID of the original request. This is used to make insure this is
        /// a response to the original request.
        /// </param>
        /// <param name="remoteEP">
        /// A IPEndPoint containing the remote address of the service that sent this response.
        /// </param>
        /// <param name="messageCheck">
        /// A WsdMessageCheck object ued to test for a duplicate request message.
        /// If null no check is performed as would be the case for a directed probe response.
        /// </param>
        /// <returns>
        /// A DpwsResolveMatch object containing a service endpoint description returned by a service.
        /// </returns>
        /// <exception cref="XmlException">If required tags are missing or invalid namespaces are detected.</exception>
        public DpwsServiceDescription ProcessResolveMatch(WsMessage resolveResponse, string messageID, IPEndPoint remoteEP, WsMessageCheck messageCheck)
        {
            DpwsServiceDescriptions resolveMatches = ProcessMatch(DpwsServiceDescription.ServiceDescriptionType.ResolveMatch, resolveResponse, messageID, remoteEP, messageCheck);

            return (resolveMatches == null ? null : resolveMatches[0]);
        }

        private DpwsServiceDescriptions ProcessMatch(DpwsServiceDescription.ServiceDescriptionType type, WsMessage response, string messageID, IPEndPoint remoteEP, WsMessageCheck messageCheck)
        {
            Microsoft.SPOT.Debug.Assert(type == DpwsServiceDescription.ServiceDescriptionType.ProbeMatch
                || type == DpwsServiceDescription.ServiceDescriptionType.ResolveMatch);

            // Parse and build header
            WsWsaHeader header = response.Header;
            XmlReader   reader = response.Reader;

            if(header == null || reader == null)
            {
                return null;
            }

            try
            {
                // Make sure this is a probe matches response
                String headerAction = (type == DpwsServiceDescription.ServiceDescriptionType.ProbeMatch) ?
                    m_version.DiscoveryNamespace + "/ProbeMatches" :
                    m_version.DiscoveryNamespace + "/ResolveMatches";

                if (header.Action != headerAction)
                    return null;

                // Make sure this is not a duplicate probe response
                if (messageCheck != null)
                {
                    if (messageCheck.IsDuplicate(header.MessageID, remoteEP.ToString()) == true)
                    {
                        System.Ext.Console.Write("ProbeMatches / ResolveMatches - Duplicate response - " + header.Action + " received");
                        return null;
                    }
                }

                // Make sure the messageID matches the request ID
                if (header.RelatesTo != messageID)
                    return null;

                // Process the probe matches
#if DEBUG
                int depth = reader.Depth;
#endif
                DpwsServiceDescriptions matches = new DpwsServiceDescriptions(reader, type, m_version, remoteEP);
#if DEBUG
                Microsoft.SPOT.Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif

                return matches;
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Parses a GetResponse message.
        /// </summary>
        /// <param name="soapResponse">A byte array containing a response message.</param>
        /// <param name="messageID">The message ID sent in the original request.</param>
        /// <returns>
        /// A DpwsMetadata object containing a details about a service endpoint.
        /// </returns>
        internal DpwsMetadata ProcessGetResponse(byte[] soapResponse, string messageID)
        {
            // Parse and build header
            WsWsaHeader header = new WsWsaHeader();
            XmlReader reader;
            try
            {
                reader = WsSoapMessageParser.ParseSoapMessage(soapResponse, ref header, m_version);
            }
            catch
            {
                return null;
            }

            try
            {
                // Make sure this is a get response
                if (header.Action != WsWellKnownUri.WstNamespaceUri + "/GetResponse")
                    return null;

                // Make sure the messageID matches the request ID
                if (header.RelatesTo != messageID)
                    return null;

#if DEBUG
                int depth = reader.Depth;
#endif
                DpwsMetadata metaData = new DpwsMetadata(reader, m_version);
#if DEBUG
                Microsoft.SPOT.Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif
                return metaData;

            }
            finally
            {
                reader.Close();
            }
        }
    }
}


