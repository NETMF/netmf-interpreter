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
using Ws.Services.Xml;

using System.Ext;

namespace Dpws.Client.Eventing
{
    /// <summary>
    /// Type used to define a set of parameters passed to a SubscriptionEnd event handler.
    /// </summary>
    public class SubscriptionEndEventArgs
    {
        /// <summary>
        /// The endpoint address of the subscription manager that sent the subscription end request.
        /// </summary>
        public readonly WsWsaEndpointRef SubscriptionManager;

        /// <summary>
        /// Client defined identifier.
        /// </summary>
        public readonly String Identifier;

        /// <summary>
        /// A string containing a subscription ID that identifies an event source.
        /// </summary>
        public readonly String SubscriptionID;

        /// <summary>
        /// Create an instance of a SubscriptionEndEvent arguments object.
        /// </summary>
        internal SubscriptionEndEventArgs(WsMessage msg, ProtocolVersion version)
        {
            // Retrieve the Identifier
            WsWsaHeader header = msg.Header;
            XmlReader reader = msg.Reader;


            this.Identifier = header.Any.GetNodeValue("Identifier", version.EventingNamespace);

            reader.ReadStartElement("SubscriptionEnd", version.EventingNamespace);

            if (reader.IsStartElement("SubscriptionManager", version.EventingNamespace))
            {
                this.SubscriptionManager = new WsWsaEndpointRef(reader, version.AddressingNamespace);
            }

            this.SubscriptionID = this.SubscriptionManager.RefParameters.GetNodeValue("Identifier", version.EventingNamespace);

            if (this.SubscriptionID == null || this.Identifier == null) // both need to be there
            {
                throw new XmlException();
            }
        }
    }

    /// <summary>
    /// A delegate used by the eventing clients SubscriptionEnd event handler.
    /// </summary>
    /// <param name="obj">The object that raised the event.</param>
    /// <param name="SubscriptionEndEventArgs">An object that contains subscription end details.</param>
    public delegate void SubscriptionEndEventHandler(object obj, SubscriptionEndEventArgs SubscriptionEndEventArgs);
}


