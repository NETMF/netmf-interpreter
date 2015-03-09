using System;
using System.Collections;
using System.Text;
using System.Net;
using Ws.Services.WsaAddressing;

using Microsoft.SPOT.Net.NetworkInformation;
using System.IO;
using System.Xml;
using Ws.Services.Faults;
using System.Ext.Xml;
using Ws.Services.Xml;
using System.Ext;
using Ws.Services.Soap;

namespace Ws.Services
{
    /// <summary>
    /// A collection of common namespaces required by soap based standards and specifications
    /// encapsulated by the DPWS specification.
    /// </summary>
    public static class WsWellKnownUri
    {
        public const String SoapNamespaceUri = "http://www.w3.org/2003/05/soap-envelope";
        public const String XopNamespaceUri = "http://www.w3.org/2004/08/xop/include";
        public const String WseNamespaceUri = "http://schemas.xmlsoap.org/ws/2004/08/eventing";
        public const String WsxNamespaceUri = "http://schemas.xmlsoap.org/ws/2004/09/mex";
        public const String WstNamespaceUri = "http://schemas.xmlsoap.org/ws/2004/09/transfer";
        public const String SchemaNamespaceUri = "http://www.w3.org/2001/XMLSchema-instance";
    }

    public static class WsNamespacePrefix
    {
        public const string Wsd  = "d";
        public const string Wsa  = "a";
        public const string Wse  = "e";
        public const string Wsdp = "p";
        public const string Wsx  = "x";
        public const string Wsdl = "l";
        public const string Wsu  = "u";
        public const string Soap = "s";
    }

    public abstract class ProtocolVersion
    {
        /// <summary>
        /// Use to get or set the DPWS version number.
        /// </summary>
        public abstract Version Version { get; }
        /// <summary>
        /// Use to get the addressing namespace
        /// </summary>
        public abstract string AddressingNamespace { get; }
        /// <summary>
        /// Use to get the eventing namespace
        /// </summary>
        public abstract string EventingNamespace { get; }
        /// <summary>
        /// Use to get or set the Ws-Discovery Well Know Address.
        /// </summary>
        public abstract string DiscoveryNamespace { get; }
        /// <summary>
        /// Use to get or set the Ws-Discovery Well Know Address.
        /// </summary>
        public abstract string DiscoveryWellKnownAddress { get; }
        /// <summary>
        /// Use to get the anonymous Uri namespace
        /// </summary>
        public abstract string AnonymousUri { get; }
        /// <summary>
        /// Use to get the anonymous role Uri namespace
        /// </summary>
        public abstract string AnonymousRoleUri { get; }
        /// <summary>
        /// Use to determine if the Soap headers should be included in the Soap message.
        /// </summary>
        public abstract bool IncludeSoapHeaders { get; }
        /// <summary>
        /// Use to get the devices profile for web services
        /// </summary>
        public abstract string WsdpNamespaceUri { get; }
    }
    
    /// <summary>
    /// Derived class used to store Ws-* well known properties for version 1.0
    /// </summary>
    public class ProtocolVersion10 : ProtocolVersion
    {
        const String m_wsaNamespaceUri     = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
        const String m_wsaAnonymousUri     = m_wsaNamespaceUri + "/anonymous";
        const String m_wsaAnonymousRoleUri = m_wsaNamespaceUri + "/role/anonymous";
        const String m_wsdDiscoveryUrn     = "urn:schemas-xmlsoap-org:ws:2005:04:discovery";
        const String m_wsdDiscoveryUri     = "http://schemas.xmlsoap.org/ws/2005/04/discovery";
        const String m_wsdpNamespaceUri    = "http://schemas.xmlsoap.org/ws/2006/02/devprof";
        static Version m_wsVersion          = new Version(1,0,0,0);

        /// <summary>
        /// Get the Ws-Discovery version number.
        /// </summary>
        public override Version Version { get { return m_wsVersion; } }

        /// <summary>
        /// Use to get the Ws-Discovery Well Know Address.
        /// </summary>
        public override string AddressingNamespace       { get { return m_wsaNamespaceUri; } }
        public override string EventingNamespace         { get { return WsWellKnownUri.WseNamespaceUri; } }
        public override string DiscoveryNamespace        { get { return m_wsdDiscoveryUri; } }
        public override string DiscoveryWellKnownAddress { get { return m_wsdDiscoveryUrn; } }
        public override string AnonymousUri              { get { return m_wsaAnonymousRoleUri; } } // DPWS 1.0 requires AnonymousRole for To: in Resolve/Probe matches
        public override string AnonymousRoleUri          { get { return m_wsaAnonymousRoleUri; } }
        public override string WsdpNamespaceUri          { get { return m_wsdpNamespaceUri; } }
        public override bool   IncludeSoapHeaders        { get { return true; } }
    }

    /// <summary>
    /// Derived class used to store Ws-* well known properties for version 1.1
    /// </summary>
    public class ProtocolVersion11 : ProtocolVersion
    {
        const String m_wsaNamespaceUri     = "http://www.w3.org/2005/08/addressing";
        const String m_wsaAnonymousUri     = m_wsaNamespaceUri + "/anonymous";
        const String m_wsaAnonymousRoleUri = m_wsaNamespaceUri + "/role/anonymous";
        const String m_wsdDiscoveryUrn     = "urn:docs-oasis-open-org:ws-dd:ns:discovery:2009:01";
        const String m_wsdDiscoveryUri     = "http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01";
        const String m_wsdpNamespaceUri    = "http://docs.oasis-open.org/ws-dd/ns/dpws/2009/01";
        static Version m_wsVersion         = new Version(1,1,0,0);

        /// <summary>
        /// Get the Ws-Discovery version number.
        /// </summary>
        public override Version Version { get { return m_wsVersion; } }

        /// <summary>
        /// Use to get the Ws-Discovery Well Know Address.
        /// </summary>
        public override string AddressingNamespace       { get { return m_wsaNamespaceUri; } }
        public override string EventingNamespace         { get { return WsWellKnownUri.WseNamespaceUri; } }
        public override string DiscoveryNamespace        { get { return m_wsdDiscoveryUri; } }
        public override string DiscoveryWellKnownAddress { get { return m_wsdDiscoveryUrn; } }
        public override string AnonymousUri              { get { return m_wsaAnonymousUri; } }
        public override string AnonymousRoleUri          { get { return m_wsaAnonymousRoleUri; } }
        public override string WsdpNamespaceUri          { get { return m_wsdpNamespaceUri; } }
        public override bool   IncludeSoapHeaders        { get { return true; } }
    }

    //--//

    internal static class XmlReaderHelper
    {
        public static void SkipAllSiblings(XmlReader reader)
        {
            reader.MoveToContent();

            Microsoft.SPOT.Debug.Assert(reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement);

            // We don't care about the rest, skip it
            if (reader.NodeType == XmlNodeType.Element)
            {
                int targetDepth = reader.Depth - 1;
                while (reader.Read() && reader.Depth > targetDepth) ;

                Microsoft.SPOT.Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
            }
        }

#if DEBUG
        public static bool HasReadCompleteNode(int oldDepth, XmlReader reader)
        {
            reader.MoveToContent();

            return (reader.NodeType == XmlNodeType.Element && reader.Depth == oldDepth) ||
                   (reader.NodeType == XmlNodeType.EndElement && reader.Depth == oldDepth - 1);
        }

#endif
    }

    internal static class WsSoapMessageParser
    {
        public static XmlReader ParseSoapMessage(byte[] soapMessage, ref WsWsaHeader header, ProtocolVersion version)
        {
            MemoryStream requestStream = new MemoryStream(soapMessage);
            XmlReader reader = XmlReader.Create(requestStream);
            header = new WsWsaHeader();

            try
            {
                reader.ReadStartElement("Envelope", WsWellKnownUri.SoapNamespaceUri);
#if DEBUG
                int depth = reader.Depth;
#endif
                header.ParseHeader(reader, version);
#if DEBUG
                Microsoft.SPOT.Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif
                reader.ReadStartElement("Body", WsWellKnownUri.SoapNamespaceUri);

            }
            catch (XmlException e)
            {
                reader.Close();
                throw new WsFaultException(header, WsFaultType.XmlException, e.ToString());
            }

            return reader;
        }
    }

    internal class WsSoapMessageWriter
    {
        private readonly ProtocolVersion m_version;

        internal WsSoapMessageWriter(ProtocolVersion version) { m_version = version; }

        public String WriteSoapMessageStart(XmlWriter writer, WsMessage msg)
        {
            return WriteSoapMessageStart(writer, msg.Prefixes, msg.Namespaces, msg.Header, msg.AppSequence, true);
        }
        
        public String WriteSoapMessageStart(XmlWriter writer, WsMessage msg, bool fSendHeader)
        {
            return WriteSoapMessageStart(writer, msg.Prefixes, msg.Namespaces, msg.Header, msg.AppSequence, fSendHeader);
        }

        public String WriteSoapMessageStart(XmlWriter writer, WsPrefix prefixes, WsXmlNamespaces additionalPrefixes, WsWsaHeader header, WsAppSequence appSequence, bool fSendHeader)
        {
            String messageId = "urn:uuid:" + Guid.NewGuid();

            String xml =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<s:Envelope xmlns:s=\"" + WsWellKnownUri.SoapNamespaceUri + "\" " +
                "xmlns:a=\"" + m_version.AddressingNamespace + "\" ";

            if ((prefixes & WsPrefix.Wsdp) != WsPrefix.None)
            {
                xml += "xmlns:p=\"" + m_version.WsdpNamespaceUri + "\" ";
            }

            if ((prefixes & WsPrefix.Wse) != WsPrefix.None)
            {
                xml += "xmlns:e=\"" + WsWellKnownUri.WseNamespaceUri + "\" ";
            }

            if ((prefixes & WsPrefix.Wsx) != WsPrefix.None)
            {
                xml += "xmlns:x=\"" + WsWellKnownUri.WsxNamespaceUri + "\" ";
            }

            if ((prefixes & WsPrefix.Wsd) != WsPrefix.None || appSequence != null)
            {
                xml += "xmlns:d=\"" + m_version.DiscoveryNamespace + "\" ";
            }

            if (additionalPrefixes != null)
            {
                int count = additionalPrefixes.Count;
                WsXmlNamespace current;
                for (int i = 0; i < count; i++)
                {
                    current = additionalPrefixes[i];
                    xml += "xmlns:" + current.Prefix + "=\"" + current.NamespaceURI + "\" ";
                }
            }

            xml += ">";

            if(fSendHeader)
            {
                xml +=
                    "<s:Header>" +
                    "<a:Action" + (header.MustUnderstand ? " s:mustUnderstand=\"1\">" : ">") + header.Action + "</a:Action>" +
                    "<a:MessageID>" + messageId + "</a:MessageID>" +
                    "<a:To" + (header.MustUnderstand ? " s:mustUnderstand=\"1\">" : ">") + header.To + "</a:To>";

                if (header.RelatesTo != null)
                {
                    xml += "<a:RelatesTo>" + header.RelatesTo + "</a:RelatesTo>";
                }

                if (header.From != null)
                {
                    xml += "<a:From><a:Address>" + header.From.Address.AbsoluteUri + "</a:Address></a:From>";
                }

                if (header.ReplyTo != null)
                {
                    xml += "<a:ReplyTo><a:Address>" + header.ReplyTo.Address.AbsoluteUri + "</a:Address></a:ReplyTo>";
                }

                if (appSequence != null)
                {
                    xml += "<d:AppSequence InstanceId=\"" + appSequence.InstanceId + "\" ";

                    if(appSequence.SequenceId != null)
                    {
                        xml += "SequenceId=\"" + appSequence.SequenceId + "\" ";
                    }
                    
                    xml += "MessageNumber=\"" + appSequence.MessageNumber + "\"/>";
                }

                writer.WriteRaw(xml);

                if (header.Any != null)
                {
                    header.Any.WriteTo(writer);
                }

                writer.WriteRaw("</s:Header>");
            }
            else
            {
                writer.WriteRaw(xml);
            }

            writer.WriteRaw("<s:Body>");

            return messageId;
        }

        public void WriteSoapMessageEnd(XmlWriter writer)
        {
            writer.WriteRaw("</s:Body></s:Envelope>");
        }

    }
}

namespace Ws.Services.Utilities
{
    /// <summary>
    /// Class used by the Device and HostedService clsses to quickly validate a special case of UrnUuid.
    /// </summary>
    internal class WsUtilities
    {
        /// <summary>
        /// Validates a urn:uuid:Guid.
        /// </summary>
        /// <param name="uri">A uri.</param>
        /// <returns>True if this is a valid urn:uud:giud.</returns>
        public static bool ValidateUrnUuid(string uri)
        {
            // Validate UUID
            if (uri.IndexOf("urn:uuid:") != 0)
                return false;

            char[] tempUUID = uri.Substring(9).ToLower().ToCharArray();
            int length = tempUUID.Length;
            int uuidSegmentCount = 0;
            int[] delimiterIndexes = { 8, 13, 18, 23 };
            bool invalidUUID = false;
            for (int i = 0; i < length; ++i)
            {
                // Make sure these are valid hex numbers numbers
                if ((tempUUID[i] < '0' || tempUUID[i] > '9') && (tempUUID[i] < 'a' || tempUUID[i] > 'f') && tempUUID[i] != '-')
                {
                    invalidUUID = true;
                    break;
                }
                else
                {
                    // Check each segment length
                    if (tempUUID[i] == '-')
                    {
                        if (uuidSegmentCount > 3)
                        {
                            invalidUUID = true;
                            break;
                        }

                        if (i != delimiterIndexes[uuidSegmentCount])
                        {
                            invalidUUID = true;
                            break;
                        }

                        ++uuidSegmentCount;
                    }
                }
            }

            if (invalidUUID)
                return false;
            return true;
        }

    }

    /// <summary>
    /// Class used to format and parse duration time values.
    /// </summary>
    public class WsDuration
    {
        /// <summary>
        /// Creates and instance of a duration object initialized to a TimeSpan.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan containing the duration value.</param>
        public WsDuration(TimeSpan timeSpan)
        {
            this.DurationInSeconds = (long)(timeSpan.Ticks / TimeSpan.TicksPerSecond);
            this.Ticks = timeSpan.Ticks;
            this.DurationString = this.ToString(timeSpan);
        }

        /// <summary>
        /// Creates and instance of a duration object initialized to a number of seconds.
        /// </summary>
        /// <param name="seconds">A long containing the duration value in seconds.</param>
        public WsDuration(long seconds)
        {
            TimeSpan tempTS = new TimeSpan(seconds * TimeSpan.TicksPerSecond);
            this.DurationInSeconds = seconds;
            this.Ticks = tempTS.Ticks;
            this.DurationString = this.ToString(tempTS);
        }

        /// <summary>
        /// Creates an instance of a duration object initialized to a formated duration string value.
        /// </summary>
        /// <param name="duration">A string containing a formated duration values (P#Y#M#DT#H#M#S). </param>
        public WsDuration(string duration)
        {
            TimeSpan tempTS = ParseDuration(duration);
            this.DurationString = duration;
            this.DurationInSeconds = (long)(tempTS.Ticks / TimeSpan.TicksPerSecond);
            this.Ticks = tempTS.Ticks;
        }

        /// <summary>
        /// Use to get or set the number of seconds this duration object represents.
        /// </summary>
        public readonly long DurationInSeconds;

        /// <summary>
        /// Use to get a string containing a formated duration representing the number of seconds this duration
        /// object represents.
        /// </summary>
        public readonly String DurationString;

        /// <summary>
        /// Use to get a long containing the number of duration ticks.
        /// </summary>
        public readonly long Ticks;

        /// <summary>
        /// Creates a xml schema duration from a TimeSpan
        /// </summary>
        /// <param name="timeSpan">A TimeSpan containing the time value used to create the duration.</param>
        /// <returns>A string contining the duration in Xml Schema format.</returns>
        public string ToString(TimeSpan timeSpan)
        {
            string dur;
            if (timeSpan.Ticks < 0)
            {
                dur = "-P";
                timeSpan = timeSpan.Negate();
            }
            else
                dur = "P";

            int years = timeSpan.Days / 365;
            int days = timeSpan.Days - years * 365;
            dur += (years > 0 ? years + "Y" : "");
            dur += (days > 0 ? days + "D" : "");
            dur += (timeSpan.Hours > 0 || timeSpan.Minutes > 0 || timeSpan.Seconds > 0 || timeSpan.Milliseconds > 0 ? "T" : "");
            dur += (timeSpan.Hours > 0 ? timeSpan.Hours + "H" : "");
            dur += (timeSpan.Minutes > 0 ? timeSpan.Minutes + "M" : "");
            dur += (timeSpan.Seconds > 0 ? timeSpan.Seconds.ToString() : "");
            dur += (timeSpan.Milliseconds > 0 ? (timeSpan.Milliseconds * 0.001).ToString().Substring(1) + "S" : (timeSpan.Seconds > 0 ? "S" : ""));
            return dur;
        }

        /// <summary>
        /// Parses a duration string and assigns duration properties.
        /// </summary>
        /// <param name="duration">A string containing a valid duration value.</param>
        /// <remarks>A valid duration string conforms to the format: P#Y#M#DT#H#M#S.</remarks>
        private TimeSpan ParseDuration(string duration)
        {
            int multiplier = duration[0] == '-' ? -1 : 1;
            int start = multiplier == -1 ? 1 : 0;

            // Check for mandatory start sentinal
            if (duration[start] != 'P')
                throw new ArgumentException("Invalid duration format.");
            ++start;

            string sentinals = "PYMDTHMS";
            int[] durationBuf = { 0, 0, 0, 0, 0, 0, 0, 0 };
            string fieldValue;
            double seconds = 0;
            int fldIndex;
            int lastFldIndex = 0;
            for (int i = start; i < duration.Length; ++i)
            {
                char curChar = duration[i];

                if (curChar == '.')
                {
                    for (i = i + 1; i < duration.Length; ++i)
                    {
                        curChar = duration[i];
                        if (curChar == 'S')
                            break;
                    }

                    if (curChar != 'S')
                        throw new ArgumentException("Invalid duration format.");
                }

                if (curChar < '0' || curChar > '9')
                {
                    if ((fldIndex = sentinals.Substring(lastFldIndex).IndexOf(curChar)) == -1)
                        throw new ArgumentException("Invalid duration format.");
                    fldIndex += lastFldIndex;
                    lastFldIndex = fldIndex;

                    // Skip T sentinal
                    if (sentinals[fldIndex] == 'T')
                    {
                        start = i + 1;
                        continue;
                    }

                    // Check for blank fields
                    if (i - start < 1)
                        throw new ArgumentException("Invalid duration format.");

                    fieldValue = duration.Substring(start, i - start);
                    if (fldIndex == 7)
                        seconds = Convert.ToDouble(fieldValue);
                    else
                        durationBuf[fldIndex] = Convert.ToInt32(fieldValue);

                    start = i + 1;
                }
            }

            // Assign duration properties
            // days = years * 365 days + months * 31 days + days;
            int days = durationBuf[1] * 365 + durationBuf[2] * 31 + durationBuf[3];

            // Note: Adding 0.0001 temporarily fixes a double/rounding problem
            int milliseconds = (int)(((seconds - (int)seconds) + 0.0001) * 1000.00);
            if ((ulong)((((long)days * 86400L + (long)durationBuf[5] * 3600L + (long)durationBuf[6] * 60L + (long)(seconds)) * TimeSpan.TicksPerSecond) + (long)(milliseconds * TimeSpan.TicksPerMillisecond)) > long.MaxValue)
                throw new ArgumentOutOfRangeException("Durations value exceeds TimeSpan.MaxValue.");
            TimeSpan tempTs = new TimeSpan(days, durationBuf[5], durationBuf[6], (int)seconds, milliseconds);
            return multiplier == -1 ? tempTs.Negate() : tempTs;
        }
    }
}


