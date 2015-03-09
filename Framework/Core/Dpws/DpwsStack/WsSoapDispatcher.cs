using System;
using System.Collections;
using Microsoft.SPOT;
using Ws.Services.Mtom;
using Ws.Services.WsaAddressing;
using Ws.Services.Serialization;
using Ws.Services.Xml;
using System.Xml;

namespace Ws.Services.Soap
{
    [Flags]
    public enum WsPrefix
    {
        None = 0x00,
        Wsdp = 0x02,
        Wse = 0x04,
        Wsx = 0x08,
        Wsd = 0x10
    }
    
    public class WsAppSequence
    {
        public String InstanceId;
        public String SequenceId;
        public String MessageNumber;
    
        public WsAppSequence(String instanceId, String sequenceId, String messageNumber)
        {
            this.InstanceId = instanceId;
            this.SequenceId = sequenceId;
            this.MessageNumber = messageNumber;
        }
    }

    
    /// <summary>
    /// Class used to pass soap or mtom messages from a transport service to the message processor.
    /// </summary>
    public class WsMessage
    {
        public readonly WsPrefix        Prefixes;
        public readonly WsAppSequence   AppSequence;
        public readonly WsXmlNamespaces Namespaces;

        WsMtomParams    m_mtomParams;
        WsMtomBodyParts m_bodyParts;
        object          m_serializer;
        object          m_deserializer;
        XmlReader       m_xmlReader;
        string          m_methodName;

        static WsMessage s_OneWayResponse;

        public static WsMessage CreateOneWayResponse()
        {
            if (s_OneWayResponse == null)
            {
                WsWsaHeader header = new WsWsaHeader(null, null, null, null, null, null);

                header.IsOneWayResponse = true;

                s_OneWayResponse = new WsMessage(header, null, WsPrefix.None);
            }

            return s_OneWayResponse;
        }
        
        
        public WsMessage( WsWsaHeader header, object body, WsPrefix prefixes )
        {
            this.Header   = header;
            this.Body     = body;
            this.Prefixes = prefixes;
        }

        public WsMessage( WsWsaHeader header, object body, WsPrefix prefixes, WsXmlNamespaces namespaces, WsAppSequence appSequence)
        {
            this.Header      = header;
            this.Body        = body;
            this.Prefixes    = prefixes;
            this.Namespaces  = namespaces;
            this.AppSequence = appSequence;
        }

        
        internal WsMtomParams    MtomPropeties { get { return m_mtomParams;   } set { m_mtomParams   = value; } }
        public   WsMtomBodyParts BodyParts     { get { return m_bodyParts;    } set { m_bodyParts    = value; } }
        public   object          Serializer    { get { return m_serializer;   } set { m_serializer   = value; } }
        public   object          Deserializer  { get { return m_deserializer; } set { m_deserializer = value; } }
        public   XmlReader       Reader        { get { return m_xmlReader;    } set { m_xmlReader    = value; } }
        public   string          Method        { get { return m_methodName;   } set { m_methodName   = value; } }


        public WsWsaHeader Header;

        /// <summary>
        /// Use to get a byte array containing the request transport message.
        /// </summary>
        public object Body;
    }
}


