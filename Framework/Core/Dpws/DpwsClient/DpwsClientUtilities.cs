using System;
using System.Collections;
using System.Text;
using System.Net;
using Ws.Services.WsaAddressing;
using System.Xml;
using Ws.Services;

namespace Dpws.Client
{
    internal static class WsFault
    {
        internal static void ParseFaultResponseAndThrow(XmlReader reader)
        {
            String code = String.Empty;
            String subcode = String.Empty;
            String reason = String.Empty;
            String details = String.Empty;

            try
            {
                reader.ReadStartElement("Fault", WsWellKnownUri.SoapNamespaceUri);

                reader.ReadStartElement("Code", WsWellKnownUri.SoapNamespaceUri);

                code = reader.ReadElementString("Value", WsWellKnownUri.SoapNamespaceUri);

                if (reader.IsStartElement("Subcode", WsWellKnownUri.SoapNamespaceUri))
                {
                    reader.Read(); //StartElement Subcode
                    subcode = reader.ReadElementString("Value", WsWellKnownUri.SoapNamespaceUri);
                    reader.ReadEndElement(); // Subcode
                }

                reader.ReadEndElement(); // Code

                reader.ReadStartElement("Reason", WsWellKnownUri.SoapNamespaceUri);
                reason = reader.ReadElementString("Text", WsWellKnownUri.SoapNamespaceUri);
                reader.ReadEndElement(); // Reason;

                if (reader.ReadToNextSibling("Details", WsWellKnownUri.SoapNamespaceUri))
                {
                    details = reader.ReadElementString("Details", WsWellKnownUri.SoapNamespaceUri);
                }
            }
            catch
            {
                //We'll swallow all exception, and just make do with what we have
            }

            throw new Exception("A fault is generated.\r\nCode: " + code + "\r\nSubcode: " + subcode +
                "\r\nReason: " + reason + "\r\nDetail: " + details);
        }
    }
}


