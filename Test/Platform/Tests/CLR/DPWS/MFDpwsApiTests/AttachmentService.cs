////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using Microsoft.SPOT;
using Ws.Services;
using Ws.Services.Faults;
using Ws.Services.Mtom;
using Ws.Services.Soap;
using Ws.Services.WsaAddressing;
using Ws.Services.Xml;
using Dpws.Device.Services;
using System.Ext;
using System.IO;
using System.Ext.Xml;
using Microsoft.SPOT.Platform.Test;

namespace Interop.SimpleService
{

    // AttachmentService - Receives MTOM attachments
    // Messages supported:
    //      OneWayAttachment: accepts an MTOM binary attachment, does not return a response
    //      TwoWayAttachmentRequest: accepts an MTOM binary attachment, responding with an
    //          MTOM attachment generated in TwoWayAttachmentResp

    class AttachmentService : DpwsHostedService
    {
        private const String AttNamespace = "http://schemas.example.org/AttachmentService";

        // Constructor sets service properties and defines operations
        public AttachmentService(ProtocolVersion version) : base(version)
        {
            // Add ServiceNamespace. Set ServiceID and ServiceTypeName
            ServiceNamespace = new WsXmlNamespace("att", AttNamespace);
            ServiceID = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b92";
            ServiceTypeName = "AttachmentService";

            // Add service operations
            ServiceOperations.Add(new WsServiceOperation(AttNamespace, "OneWayAttachment"));
            ServiceOperations.Add(new WsServiceOperation(AttNamespace, "TwoWayAttachmentRequest"));
        }

        public AttachmentService() : 
            this(new ProtocolVersion10())
        {
        }

        // OneWayAttachment: receive an MTOM binary attachment
        public WsMessage OneWayAttachment(WsMessage request)
        {
            WsWsaHeader header = request.Header;
            XmlReader reader = request.Reader;

            // Make sure we have MTOM body parts
            if (request.BodyParts == null)
                throw new WsFaultException(header, WsFaultType.XmlException, "MTOM message has no body parts.");

            // Find paramaters
            reader.ReadStartElement("OneWayAttachment", AttNamespace);
            reader.ReadStartElement("Param", AttNamespace);

            // Look for Include parameter
            if (reader.IsStartElement("Include", WsWellKnownUri.XopNamespaceUri) == false)
                throw new WsFaultException(header, WsFaultType.XmlException, "Body/OneWayAttachment/Param/Include is missing.");

            // Look for attachment body part
            reader.MoveToFirstAttribute();
            string cid = "<" + reader.Value.Substring(4) + ">";
            ////Log.Comment("");
            ////Log.Comment("OneWayAttachment received. href = " + reader.Value);
            WsMtomBodyPart bodyPart;
            if ((bodyPart = request.BodyParts[cid]) == null)
                throw new WsFaultException(header, WsFaultType.XmlException, "Required body part \"" + reader.Value + "\" in missing.");

            // Print information about the attachment
            ////Log.Comment("Attachment size = " + bodyPart.Content.Length);
            ////Log.Comment("Content-ID = " + bodyPart.ContentID);
            ////Log.Comment("Content-Type = " + bodyPart.ContentType);
            ////Log.Comment("Content-Transfer-Encoding = " + bodyPart.ContentTransferEncoding);
            ////Log.Comment("");

            return null;     // empty response
        }

        // TwoWayAttachmentRequest: receive an MTOM binary attachment and send a binary attachement in response
        public WsMessage TwoWayAttachmentRequest(WsMessage request)
        {
            WsWsaHeader header = request.Header;
            XmlReader reader = request.Reader;

            // Make sure we have MTOM body parts
            if (request.BodyParts == null)
                throw new WsFaultException(header, WsFaultType.XmlException, "MTOM message has no body parts.");

            // Find paramaters
            reader.ReadStartElement("TwoWayAttachmentRequest", AttNamespace);
            reader.ReadStartElement("Param", AttNamespace);

            // Look for Include parameter
            if (reader.IsStartElement("Include", WsWellKnownUri.XopNamespaceUri) == false)
                throw new WsFaultException(header, WsFaultType.XmlException, "Body/TwoWayAttachmentRequest/Param/Include is missing.");

            // Look for attachment body part
            reader.MoveToFirstAttribute();
            string cid = "<" + reader.Value.Substring(4) + ">";
            ////Log.Comment("");
            ////Log.Comment("TwoWayAttachment received. href = " + reader.Value);
            WsMtomBodyPart bodyPart;
            if ((bodyPart = request.BodyParts[cid]) == null)
                throw new WsFaultException(header, WsFaultType.XmlException, "Required body part \"" + reader.Value + "\" in missing.");

            // Print information about the attachment
            ////Log.Comment("Attachment size = " + bodyPart.Content.Length);
            ////Log.Comment("Content-ID = " + bodyPart.ContentID);
            ////Log.Comment("Content-Type = " + bodyPart.ContentType);
            ////Log.Comment("Content-Transfer-Encoding = " + bodyPart.ContentTransferEncoding);
            ////Log.Comment("");

            // Return response
            return TwoWayAttachmentResp(header);
        }

        // TwoWayAttachmentResp: Construct response message for TwoWayAttachmentRequest
        public WsMessage TwoWayAttachmentResp(WsWsaHeader header)
        {
            // Create xmlWriter object
            MemoryStream soapStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(soapStream);

            // Write processing instructions and root element
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("soap", "Envelope", "http://www.w3.org/2003/05/soap-envelope");

            // Write namespaces
            ProtocolVersion10 ver = new ProtocolVersion10();
            xmlWriter.WriteAttributeString("xmlns", "wsa", null, ver.AddressingNamespace);
            xmlWriter.WriteAttributeString("xmlns", "xop", null, WsWellKnownUri.XopNamespaceUri);
            xmlWriter.WriteAttributeString("xmlns", "att", null, "http://schemas.example.org/AttachmentService");

            // Write header
            xmlWriter.WriteStartElement("soap", "Header", null);
            xmlWriter.WriteStartElement("wsa", "To", null);
            xmlWriter.WriteString("http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous");
            xmlWriter.WriteEndElement(); // End To
            xmlWriter.WriteStartElement("wsa", "Action", null);
            xmlWriter.WriteString("http://schemas.example.org/AttachmentService/TwoWayAttachmentResponse");
            xmlWriter.WriteEndElement(); // End Action
            xmlWriter.WriteStartElement("wsa", "RelatesTo", null);
            xmlWriter.WriteString(header.MessageID);
            xmlWriter.WriteEndElement(); // End RelatesTo
            xmlWriter.WriteStartElement("wsa", "MessageID", null);
            xmlWriter.WriteString("urn:uuid:" + Guid.NewGuid().ToString());
            xmlWriter.WriteEndElement(); // End To
            xmlWriter.WriteEndElement(); // End Header

            // write body
            xmlWriter.WriteStartElement("soap", "Body", null);
            xmlWriter.WriteStartElement("att", "TwoWayAttachmentResponse", null);
            xmlWriter.WriteStartElement("att", "Param", null);
            xmlWriter.WriteStartElement("xop", "Include", null);
            xmlWriter.WriteAttributeString("href", "cid:0@body");
            xmlWriter.WriteString("");
            xmlWriter.WriteEndElement(); // End Include
            xmlWriter.WriteEndElement(); // End Param
            xmlWriter.WriteEndElement(); // End TwoWayAttachmentResponse
            xmlWriter.WriteEndElement(); // End Body

            xmlWriter.WriteEndElement(); // End Envelope

            // Create return buffer and close writer
            xmlWriter.Flush();
            xmlWriter.Close();

            WsMessage msg = new WsMessage(null, soapStream.ToArray(), WsPrefix.Wse);

            // Clear the hosted service's MTOM body parts collection and build return body parts collection
            // Create and store soap body part (As per spec soap envelope must be first body part)
            WsMtomBodyPart bodyPart = new WsMtomBodyPart();

            // Create and store attachment body part
            bodyPart = new WsMtomBodyPart();
            HelpIcon testIcon = new HelpIcon();
            bodyPart.Content = testIcon.Data;
            bodyPart.ContentID = "<0@body>";
            bodyPart.ContentTransferEncoding = "binary";
            bodyPart.ContentType = "application/octet-stream";
            msg.BodyParts.Add(bodyPart);

            // Set the response type so the HTTP processor knows we are sending MTOM
            //MessageType = WsMessageType.Mtom;

            // We are returning the actual response in the hosted service's body parts, so return null
            return msg;
        }

    }
}
