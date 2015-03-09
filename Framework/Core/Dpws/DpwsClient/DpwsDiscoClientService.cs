using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using Ws.Services;
using Ws.Services.Faults;
using Ws.Services.Mtom;
using Ws.Services.Soap;
using Ws.Services.Utilities;
using Ws.Services.WsaAddressing;
using Ws.Services.Transport;
using Ws.Services.Xml;

using System.Ext;
using Microsoft.SPOT;

namespace Dpws.Client.Discovery
{
    /// <summary>
    /// A delegate used by the discovery client Hello event handler.
    /// </summary>
    /// <param name="obj">The object that raised the event.</param>
    /// <param name="helloEventArgs">
    /// An object that contains the endpoint address, types, xaddr and metadataversion of a hello request.
    /// </param>
    public delegate void HelloEventHandler(object obj, DpwsServiceDescription helloEventArgs);

    /// <summary>
    /// A delegate used by the discovery client Bye event handler.
    /// </summary>
    /// <param name="obj">The object that raised the event.</param>
    /// <param name="byeEventArgs">An object that contains the endpoint address and xaddr of a bye request.</param>
    public delegate void ByeEventHandler(object obj, DpwsServiceDescription byeEventArgs);

    // This hosted service class provides DPWS compliant client Hello discovery services.
    internal class DpwsDiscoClientService : IWsServiceEndpoint
    {
        private object                   m_threadLock;
        private WsServiceOperations      m_discoCallbacks;
        private DpwsClient               m_client;
        private readonly ProtocolVersion m_version;
        private WsMessageCheck           m_messageCheck;

        //--//

        public DpwsDiscoClientService(DpwsClient client, ProtocolVersion version)
        {
            m_client = client;
            m_threadLock = new object();
            m_version = version;

            m_messageCheck = new WsMessageCheck();

            // Add discovery Hello ServiceOperations
            m_discoCallbacks = new WsServiceOperations();
            m_discoCallbacks.Add(new WsServiceOperation(m_version.DiscoveryNamespace, "Hello"));
            m_discoCallbacks.Add(new WsServiceOperation(m_version.DiscoveryNamespace, "Bye"));
        }

        /// <summary>
        /// Property that determines if ProcessRequest call blocks.
        /// </summary>
        bool IWsServiceEndpoint.BlockingCall { get { return true; } set { } }

        /// <summary>
        /// Use to Get or Set a collection used to store Mtom bodyparts.
        /// </summary>
        /// <remarks>
        /// If the transport services receive an Mtom request. This collection contains the body parts parsed
        /// from the Mtom message. If a client needs to send an Mtom request they can populate this collection
        /// and set the ResponseType property to WsResponseType.Mtom to tell the transport services to parse this
        /// collectionand return and Mtom formatted response message.
        /// </remarks>
        public WsMtomBodyParts BodyParts { get { return null; } set { } }

        /// <summary>
        /// Property containing a collection of exposed methods implemented by this target service.
        /// </summary>
        /// <remarks>Use this colleciotn to add or remove event operations (event handlers) for this client.</remarks>
        public WsServiceOperations ServiceOperations { get { return m_discoCallbacks; } }

        /// <summary>
        /// A urn:uuid used by the transport services to locate this client. This property represents the endpoint
        /// reference address of a client. This stack supports urn:uuid endpoint addresses only.
        /// </summary>
        /// <remarks>
        /// This property contains a unique endpoint reference address. The message dispatcher uses this address
        /// to route messages to methods implemented in a client at this address. The combined base transport address
        /// (http://ip_address:port/) and this property equal the Header.To property of messages directed to this client.
        /// </remarks>
        public string EndpointAddress { get { return m_client.DiscoveryClient.DiscoVersion.DiscoveryWellKnownAddress; } set { } }

        /// <summary>
        /// A wrapper method that implements the ProcessRequest interface used to control blocking of request.
        /// The BlockingCall property is used by the stack to control single instance execution. Hello and Bye
        /// are not blocked. Event messages from a device are blocked. This means it is up to a client developer
        /// to insure long thread safety.
        /// </summary>
        /// <param name="header">A WsWsaHeader containing the header proerties of a request.</param>
        /// <param name="envelope">A WsXmlDocument containing the entire envelope of the message in a node tree.</param>
        /// <returns>A byte array containing a soap response to the request.</returns>
        WsMessage IWsServiceEndpoint.ProcessRequest(WsMessage request)
        {
            //We always block
            WsWsaHeader header = request.Header;
            
            if (header.MessageID != null &&
                m_messageCheck.IsDuplicate(header.MessageID, header.From != null ? header.From.Address.AbsoluteUri : ""))
            {
                return null;
            }            

            if(header.Action.IndexOf(m_version.DiscoveryNamespace) == 0)
            {
                String action = header.Action.Substring(m_version.DiscoveryNamespace.Length + 1);

                switch (action)
                {
                    case "Hello":
                        return Hello(request.Header, request.Reader);
                    case "Bye":
                        return Bye(request.Header, request.Reader);
                }
            }
            
            System.Ext.Console.Write("Client discovery callback method for action " + header.Action + " was not found");
            return null;
        }

        /// <summary>
        /// Hello stub calls the client hello handler
        /// </summary>
        /// <param name="header">A WsWsaHeader object containing the details soap header information.</param>
        /// <param name="envelope">A WsXmlDocument object containing the the entire soap message.</param>
        /// <returns>A byte array containig a soap response mesage.</returns>
        public WsMessage Hello(WsWsaHeader header, XmlReader reader)
        {
            try
            {
                if (reader.IsStartElement("Hello", m_version.DiscoveryNamespace))
                {
                    m_client.RaiseHelloEvent(new DpwsServiceDescription(reader, DpwsServiceDescription.ServiceDescriptionType.Hello, m_version));
                }
                else
                {
                    throw new XmlException();
                }
            }
            catch (Exception e)
            {
                System.Ext.Console.Write("Hello request processing failed.");
                System.Ext.Console.Write(e.Message);
                System.Ext.Console.Write(e.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// Bye stub calls the client Bye handler
        /// </summary>
        /// <param name="header">A WsWsaHeader object containing the details soap header information.</param>
        /// <param name="envelope">A WsXmlDocument object containing the the entire soap message.</param>
        /// <returns>A byte array containig a soap response mesage.</returns>
        public WsMessage Bye(WsWsaHeader header, XmlReader reader)
        {
            try
            {
                if (reader.IsStartElement("Bye", m_version.DiscoveryNamespace))
                {
                    m_client.RaiseByeEvent(new DpwsServiceDescription(reader, DpwsServiceDescription.ServiceDescriptionType.Bye, m_version));
                }
                else
                {
                    throw new XmlException();
                }
            }
            catch (Exception e)
            {
                System.Ext.Console.Write("Bye request processing failed.");
                System.Ext.Console.Write(e.Message);
                System.Ext.Console.Write(e.StackTrace);
            }

            return null;
        }
    }
}


