using System;
using System.Net;
using System.IO;
using System.Collections;
using Ws.Services.Soap;
using Ws.Services.WsaAddressing;
using Ws.Services.Faults;
using System.Ext.Xml;
using System.Xml;
using Microsoft.SPOT;
using Ws.Services.Serialization;
using Ws.Services.Mtom;
using Ws.Services.Binding;

namespace Ws.Services.Encoding
{  
    /// <summary>
    /// Abtracts the BindingElement for the encoding of a text message 
    /// </summary>
    public class TextMessageEncodingBindingElement : MessageEncodingBindingElement
    {        
        /// <summary>
        /// Processes a message
        /// </summary>
        /// <param name="stream">The message being processed.</param>
        /// <param name="ctx">The context associated with the message.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnProcessInputMessage(ref WsMessage msg, BindingContext ctx)
        {
            if(msg == null) return ChainResult.Abort;
            
            ArrayList props = ctx.BindingProperties;
            if (props != null)
            {
                int len = props.Count;

                for (int j = 0; j < len; j++)
                {
                    BindingProperty prop = (BindingProperty)props[j];

                    if (prop.Name == HttpKnownHeaderNames.ContentType)
                    {
                        string strContentType = ((string)prop.Value).ToLower();

                        if (strContentType.IndexOf("multipart/related;") == 0)
                        {
                            // Create the mtom header class
                            msg.MtomPropeties = new WsMtomParams();

                            // Parse Mtom Content-Type parameters
                            string[] fields = strContentType.Substring(18).Split(';');
                            int fieldsLen = fields.Length;
                            for (int i = 0; i < fieldsLen; ++i)
                            {
                                string type = fields[i];
                                int idx = type.IndexOf('=');

                                if(idx != -1)
                                {
                                    string param = type.Substring(0, idx).Trim();
                                    string value = type.Substring(idx + 1).Trim('\"');
                                    switch (param.ToUpper())
                                    {
                                        case "BOUNDARY":
                                            if (param.Length > 72)
                                                throw new ArgumentException("Mime boundary element length exceeded.", "boundary");
                                            msg.MtomPropeties.boundary = value;
                                            break;
                                        case "TYPE":
                                            msg.MtomPropeties.type = value;
                                            break;
                                        case "START":
                                            msg.MtomPropeties.start = value;
                                            break;
                                        case "START-INFO":
                                            msg.MtomPropeties.startInfo = value;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }

                            // Check required Mtom fields
                            if (msg.MtomPropeties.boundary == null || msg.MtomPropeties.type == null || msg.MtomPropeties.start == null)
                            {
                                throw new WsFaultException(msg.Header, WsFaultType.WseInvalidMessage);
                            }

                            WsMtom mtom = new WsMtom((byte[])msg.Body);

                            msg.Body = mtom.ParseMessage(msg.MtomPropeties.boundary);
                            msg.BodyParts = mtom.BodyParts;
                        }
                        else if (strContentType.IndexOf("application/soap+xml") != 0)
                        {
                            throw new WsFaultException(msg.Header, WsFaultType.WseInvalidMessage);
                        }
                    }
                }
            }

            if (msg.Body == null) return ChainResult.Continue;

            MemoryStream requestStream = new MemoryStream((byte[])msg.Body);

            XmlReader reader = XmlReader.Create(requestStream);
            WsWsaHeader hdr = new WsWsaHeader();

            reader.ReadStartElement("Envelope", WsWellKnownUri.SoapNamespaceUri);

            reader.MoveToContent();
            if(ctx.Version.IncludeSoapHeaders || reader.LocalName == "Header")
            {
                hdr.ParseHeader(reader, ctx.Version);
            }
            
            msg.Header = hdr;

            reader.ReadStartElement("Body", WsWellKnownUri.SoapNamespaceUri);


            if(msg.Deserializer != null)
            {
                msg.Body = ((DataContractSerializer)msg.Deserializer).ReadObject(reader);
                reader.Dispose();
            }
            else
            {
                msg.Reader = reader;
            }

            return ChainResult.Continue;
        }

        /// <summary>
        /// Processes a message 
        /// </summary>
        /// <param name="msg">The message being processed.</param>
        /// <param name="ctx">The context associated with the message.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnProcessOutputMessage( ref WsMessage msg, BindingContext ctx )
        {
            byte[] data = null;

            // if the body is a byte[] then it is already serialized (UDP stuff)
            if (msg == null || (msg.Body != null && msg.Body is byte[])) return ChainResult.Continue;

            // Build response message
            using (XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsSoapMessageWriter smw = new WsSoapMessageWriter(ctx.Version);

                // Write start message up to body element content
                smw.WriteSoapMessageStart(xmlWriter, msg, ctx.Version.IncludeSoapHeaders);

                if (msg.Body != null && msg.Serializer != null)
                {
                    DataContractSerializer ser = (DataContractSerializer)msg.Serializer;
                    // Serialize the body element
                    ser.WriteObject(xmlWriter, msg.Body);

                    if(ser.BodyParts != null && ser.BodyParts.Count > 0)
                    {
                        msg.BodyParts = ser.BodyParts;
                    }
                }

                // Write end message
                smw.WriteSoapMessageEnd(xmlWriter);

                data = xmlWriter.ToArray();
            }

            WsMtomBodyParts bodyParts = msg.BodyParts;

            if (bodyParts != null)
            {
                DataContractSerializer reqDcs = (DataContractSerializer)msg.Serializer;

                bodyParts.Start = "<soap@soap>";
                bodyParts.AddStart(reqDcs.CreateNewBodyPart(data, bodyParts.Start));
                if (reqDcs.BodyParts.Count > 0)
                {
                    bodyParts.Add(reqDcs.BodyParts[0]);
                }

                WsMtomParams mtomParams = new WsMtomParams();
                if (bodyParts.Boundary == null)
                    bodyParts.Boundary = Guid.NewGuid().ToString() + '-' + Guid.NewGuid().ToString().Substring(0, 33);
                mtomParams.start = bodyParts.Start;
                mtomParams.boundary = bodyParts.Boundary;
                msg.MtomPropeties = mtomParams;
                WsMtom mtom = new WsMtom();
                data = mtom.CreateMessage(bodyParts);
            }

            msg.Body = data;

            return ChainResult.Continue;
        }
    }
}

