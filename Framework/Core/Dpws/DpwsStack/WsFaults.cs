using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using Ws.Services.WsaAddressing;

using System.Ext;
using System.Ext.Xml;
using Ws.Services.Utilities;
using Ws.Services.Soap;

namespace Ws.Services.Faults
{

    public class WsFaultException : Exception
    {
        String _message;
        String _detail;

        public WsFaultException(WsWsaHeader header, WsFaultType faultType)
            : this(header, faultType, null, null)
        {
        }

        public WsFaultException(WsWsaHeader header, WsFaultType faultType, String message)
            : this(header, faultType, message, null)
        {
        }

        public WsFaultException(WsWsaHeader header, WsFaultType faultType, String message, String detail)
        {
            this.Header = header;
            this.FaultType = faultType;
            _message = message;
            _detail = detail;
        }

        public readonly WsWsaHeader Header;

        public readonly WsFaultType FaultType;

        public override string Message
        {
            get
            {
                return _message == null ? base.Message : _message;
            }
        }

        public string Detail
        {
            get
            {
                return _detail == null ? "" : _detail;
            }
        }
        
    }

    /// <summary>
    /// Enumeration used to represent fault types.
    /// </summary>
    public enum WsFaultType
    {
        // Exception Fault Types

        /// <summary>
        /// Fault sent to indicate a general purpose exception has been thrown.
        /// </summary>
        Exception,
        /// <summary>
        /// Fault sent to indicate an ArgumentException has been thrown.
        /// </summary>
        ArgumentException,
        /// <summary>
        /// Fault sent to indicate an ArgumentNullException has been thrown.
        /// </summary>
        ArgumentNullException,
        /// <summary>
        /// Fault sent to indicate an InvalidOperationException has been thrown.
        /// </summary>
        InvalidOperationException,
        /// <summary>
        /// Fault sent to indicate an XmlException has been thrown.
        /// </summary>
        XmlException,

        // Ws-Addressing Fault Types

        /// <summary>
        /// Fault sent when the message information header cannot be processed.
        /// </summary>
        WsaInvalidMessageInformationHeader,
        /// <summary>
        /// Fault sent when the a required message information header is missing.
        /// </summary>
        WsaMessageInformationHeaderRequired,
        /// <summary>
        /// Fault sent when the endpoint specified in a message information header cannot be found.
        /// </summary>
        WsaDestinationUnreachable,
        /// <summary>
        /// Fault sent when the action property is not supported at the specified endpoint.
        /// </summary>
        WsaActionNotSupported,
        /// <summary>
        /// Fault sent when the endpoint is unable to process the message at this time.
        /// </summary>
        WsaEndpointUnavailable,

        // Ws-Eventing Fault Types

        /// <summary>
        /// Fault sent when a Subscribe request specifies an unsupported delivery mode for an event source.
        /// </summary>
        WseDeliverModeRequestedUnavailable,
        /// <summary>
        /// Fault sent when a Subscribe request contains an expiration value of 0.
        /// </summary>
        WseInvalidExpirationTime,
        /// <summary>
        /// Fault when a Subscrube request contains an unsupported expiration type.
        /// </summary>
        WseUnsupportedExpirationType,
        /// <summary>
        /// Fault sent when a Subscribe request contains a filter and the event source does not support filtering.
        /// </summary>
        WseFilteringNotSupported,
        /// <summary>
        /// Fault sent when a Subscribe request contains an unsupported filter dialect.
        /// </summary>
        WseFilteringRequestedUnavailable,
        /// <summary>
        /// Fault sent when an event source is unable to process a subscribe request for local reasons.
        /// </summary>
        WseEventSourceUnableToProcess,
        /// <summary>
        /// Fault sent when an event source is unable to renew an event subscription.
        /// </summary>
        WseUnableToRenew,
        /// <summary>
        /// Fault sent when an event subscription request has an invalid or unsupported message format.
        /// </summary>
        WseInvalidMessage,
    }

    internal static class WsFault
    {
        internal static WsMessage GenerateFaultResponse(WsFaultException e, ProtocolVersion version)
        {
            return GenerateFaultResponse(e.Header, e.FaultType, e.Message, version);
        }

        internal static WsMessage GenerateFaultResponse(WsWsaHeader header, WsFaultType faultType, String details, ProtocolVersion version)
        {
            String code = String.Empty;
            String subcode = String.Empty;
            String reason = String.Empty;
            WsPrefix extraNS = WsPrefix.Wse;

            string faultAddress = null;

            if(header.FaultTo != null)
            {
                faultAddress = header.FaultTo.Address.OriginalString;
            }

            if (faultAddress == null)
            {
                faultAddress = version.AnonymousUri;
            }

            switch (faultType)
            {
                case WsFaultType.ArgumentException:
                    code =  WsNamespacePrefix.Soap + ":Receiver";
                    subcode = "ArgumentException";
                    reason = "One of the arguments provided to a method is not valid.";
                    break;
                case WsFaultType.ArgumentNullException:
                    code = WsNamespacePrefix.Soap + ":Receiver";
                    subcode = "ArgumentNullException";
                    reason = "A null reference was passed to a method that does not accept it as a valid argument.";
                    break;
                case WsFaultType.Exception:
                    code = WsNamespacePrefix.Soap + ":Receiver";
                    subcode = "Exception";
                    reason = "Errors occured during application execution.";
                    break;
                case WsFaultType.InvalidOperationException:
                    code = WsNamespacePrefix.Soap + ":Receiver";
                    subcode = "InvalidOperationException";
                    reason = "A method call is invalid for the object's current state.";
                    break;
                case WsFaultType.XmlException:
                    code = WsNamespacePrefix.Soap + ":Receiver";
                    subcode = "XmlException";
                    reason = "Syntax errors found during parsing.";
                    break;

                case WsFaultType.WsaInvalidMessageInformationHeader:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wsa + ":InvalidMessageInformationHeader";
                    reason = "A message information header is not valid and cannot be processed.";
                    break;
                case WsFaultType.WsaMessageInformationHeaderRequired:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wsa + ":MessageInformationHeaderRequired";
                    reason = "A required message Information header, To, MessageID, or Action, is not present";
                    break;
                case WsFaultType.WsaDestinationUnreachable:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wsa + ":DestinationUnreachable";
                    reason = "No route can be determined to reach the destination role defined by the WS=Addressing To.";
                    break;
                case WsFaultType.WsaActionNotSupported:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wsa + ":ActionNotSupported";
                    reason = "The [action] cannot be processed at the receiver.";
                    break;
                case WsFaultType.WsaEndpointUnavailable:
                    code = WsNamespacePrefix.Soap + ":Receiver";
                    subcode = WsNamespacePrefix.Wsa + ":EndpointUnavailable";
                    reason = "The endpoint is unable to process the message at this time.";
                    break;

                case WsFaultType.WseDeliverModeRequestedUnavailable:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wse + ":DeliverModeRequestedUnavailable";
                    reason = "The request delivery mode is not supported.";
                    extraNS = WsPrefix.Wse;
                    break;
                case WsFaultType.WseInvalidExpirationTime:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wse + ":InvalidExpirationTime";
                    reason = "The expiration time requested is invalid.";
                    extraNS = WsPrefix.Wse;
                    break;
                case WsFaultType.WseUnsupportedExpirationType:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wse + ":UnsupportedExpirationType";
                    reason = "Only expiration durations are supported.";
                    extraNS = WsPrefix.Wse;
                    break;
                case WsFaultType.WseFilteringNotSupported:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wse + ":FilteringNotSupported";
                    reason = "Filtering is not supported.";
                    extraNS = WsPrefix.Wse;
                    break;
                case WsFaultType.WseFilteringRequestedUnavailable:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wse + ":FilteringRequestedUnavailable";
                    reason = "The requested filter dialect is not supported.";
                    extraNS = WsPrefix.Wse;
                    break;
                case WsFaultType.WseEventSourceUnableToProcess:
                    code = WsNamespacePrefix.Soap + ":Receiver";
                    subcode = WsNamespacePrefix.Wse + ":EventSourceUnableToProcess";
                    reason = "No explaination yet.";
                    extraNS = WsPrefix.Wse;
                    break;
                case WsFaultType.WseUnableToRenew:
                    code = WsNamespacePrefix.Soap + ":Receiver";
                    subcode = WsNamespacePrefix.Wse + ":UnableToRenew";
                    reason = "No explaination yet.";
                    extraNS = WsPrefix.Wse;
                    break;
                case WsFaultType.WseInvalidMessage:
                    code = WsNamespacePrefix.Soap + ":Sender";
                    subcode = WsNamespacePrefix.Wse + ":InvalidMessage";
                    reason = "Message is not valid and cannot be processed.";
                    extraNS = WsPrefix.Wse;
                    break;
            }

            // Create the XmlWriter
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                // Generate the fault Header
                WsWsaHeader faultHeader = new WsWsaHeader(
                    version.AddressingNamespace + "/fault", // Action
                    header.MessageID,                       // RelatesTo
                    faultAddress,                           // To
                    null, null, null);                      // ReplyTo, From, Any

                faultHeader.IsFaultMessage = true;

                WsMessage msg = new WsMessage(faultHeader, null, extraNS, null, null);

                WsSoapMessageWriter smw = new WsSoapMessageWriter(version);
                smw.WriteSoapMessageStart(xmlWriter, msg);

                // Generate fault Body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Soap, "Fault", null);

                xmlWriter.WriteStartElement(WsNamespacePrefix.Soap, "Code", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Soap, "Value", null);
                xmlWriter.WriteString(code);
                xmlWriter.WriteEndElement(); // End Value
                xmlWriter.WriteStartElement(WsNamespacePrefix.Soap, "Subcode", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Soap, "Value", null);
                xmlWriter.WriteString(subcode);
                xmlWriter.WriteEndElement(); // End Value
                xmlWriter.WriteEndElement(); // End Subcode
                xmlWriter.WriteEndElement(); // End Code

                xmlWriter.WriteStartElement(WsNamespacePrefix.Soap, "Reason", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Soap, "Text", null);
                xmlWriter.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", "en-US");
                xmlWriter.WriteString(reason);
                xmlWriter.WriteEndElement(); // End Text
                xmlWriter.WriteEndElement(); // End Reason

                xmlWriter.WriteStartElement(WsNamespacePrefix.Soap , "Detail", null);
                xmlWriter.WriteString(details);
                xmlWriter.WriteEndElement(); // End Detail

                xmlWriter.WriteEndElement(); // End Fault

                smw.WriteSoapMessageEnd(xmlWriter);

                msg.Body = xmlWriter.ToArray();

                // Flush and close writer. Return stream buffer
                return msg;
            }
        }

        internal static void ThrowFaultException(XmlReader reader, WsWsaHeader header)
        {
            WsFaultType type = WsFaultType.Exception;
            string message = "";
            string detail = "";
            string subcode;

            reader.ReadStartElement("Body", WsWellKnownUri.SoapNamespaceUri);
            reader.ReadStartElement("Fault", WsWellKnownUri.SoapNamespaceUri);

            try
            {
                while (true)
                {
                    if (reader.IsStartElement("Code", WsWellKnownUri.SoapNamespaceUri))
                    {
                        reader.ReadStartElement(); //<Code>

                        if (reader.IsStartElement("Value", WsWellKnownUri.SoapNamespaceUri))
                        {
                            message = reader.ReadElementString();
                        }
                        if (reader.IsStartElement("Subcode", WsWellKnownUri.SoapNamespaceUri))
                        {
                            reader.ReadStartElement();
                            subcode = reader.ReadElementString("Value", WsWellKnownUri.SoapNamespaceUri);

                            int idx = subcode.IndexOf(':');

                            if (idx != -1)
                            {
                                subcode = subcode.Substring(idx + 1);
                            }

                            switch (subcode)
                            {
                                case "ArgumentException":
                                    type = WsFaultType.ArgumentException;
                                    break;
                                case "ArgumentNullException":
                                    type = WsFaultType.ArgumentNullException;
                                    break;
                                case "Exception":
                                    type = WsFaultType.Exception;
                                    break;
                                case "InvalidOperationException":
                                    type = WsFaultType.InvalidOperationException;
                                    break;
                                case "XmlException":
                                    type = WsFaultType.XmlException;
                                    break;

                                case "InvalidMessageInformationHeader":
                                    type = WsFaultType.WsaInvalidMessageInformationHeader;
                                    break;
                                case "MessageInformationHeaderRequired":
                                    type = WsFaultType.WsaMessageInformationHeaderRequired;
                                    break;
                                case "DestinationUnreachable":
                                    type = WsFaultType.WsaDestinationUnreachable;
                                    break;
                                case "ActionNotSupported":
                                    type = WsFaultType.WsaActionNotSupported;
                                    break;
                                case "EndpointUnavailable":
                                    type = WsFaultType.WsaEndpointUnavailable;
                                    break;

                                case "DeliverModeRequestedUnavailable":
                                    type = WsFaultType.WseDeliverModeRequestedUnavailable;
                                    break;
                                case "InvalidExpirationTime":
                                    type = WsFaultType.WseInvalidExpirationTime;
                                    break;
                                case "UnsupportedExpirationType":
                                    type = WsFaultType.WseUnsupportedExpirationType;
                                    break;
                                case "FilteringNotSupported":
                                    type = WsFaultType.WseFilteringNotSupported;
                                    break;
                                case "FilteringRequestedUnavailable":
                                    type = WsFaultType.WseFilteringRequestedUnavailable;
                                    break;
                                case "EventSourceUnableToProcess":
                                    type = WsFaultType.WseEventSourceUnableToProcess;
                                    break;
                                case "UnableToRenew":
                                    type = WsFaultType.WseUnableToRenew;
                                    break;
                                case "InvalidMessage":
                                    type = WsFaultType.WseInvalidMessage;
                                    break;
                            }

                            reader.ReadEndElement();
                        }

                        reader.ReadEndElement(); // </Code>
                    }

                    if (reader.IsStartElement("Reason", WsWellKnownUri.SoapNamespaceUri))
                    {
                        reader.ReadStartElement();

                        message += " = " + reader.ReadElementString("Text", WsWellKnownUri.SoapNamespaceUri);

                        reader.ReadEndElement();
                    }

                    if (reader.IsStartElement("Detail", WsWellKnownUri.SoapNamespaceUri))
                    {
                        reader.ReadStartElement();

                        if (reader.IsStartElement())
                        {
                            detail = reader.ReadElementString("string"); 
                        }
                        else
                        {
                            detail = reader.ReadString();
                        }

                        reader.ReadEndElement();
                    }

                    if (!reader.IsStartElement()) break;
                }

                reader.ReadEndElement(); // </Fault>
                reader.ReadEndElement(); // </Body>
            }
            catch
            {
            }

            throw new WsFaultException(header, type, message, detail);
        }
    }
}


