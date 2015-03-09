using System;
using System.IO;
using System.Xml;
using System.Collections;
using Ws.Services.Mtom;
using Ws.Services.WsaAddressing;
using Ws.Services.Xml;
using Microsoft.SPOT;
using System.Runtime.CompilerServices;

using System.Ext;
using System.Ext.Xml;
using Ws.Services.Soap;

namespace Ws.Services.Xml
{
    /// <summary>
    ///  This is a very limitied implementaiton of XmlElement.
    /// </summary>
    /// <remarks>Included for platform compatibility.</remarks>
    public class WsXmlElement : WsXmlNode { }
}

namespace Ws.Services.Serialization
{

    /// <summary>
    /// Class is used when procesing any attributes.
    /// </summary>
    /// <remarks>An instance of this class is created any time a normal attribute is read
    /// and stored in a collection. If AnyAttribute is specified in an object schema, the list
    /// of TypeAttributes is used to eliminate normal attributes form the any attribute collection.
    ///</remarks>
    internal class TypeAttribute
    {
        public string LocalName;
        public string Namespace;

        public TypeAttribute(string localName, string Ns)
        {
            LocalName = localName;
            Namespace = Ns;
        }
    }

    /// <summary>
    /// Base class used to serialize a DataContract type to an xml stream and deserialize an xml stream
    /// into a DataContract type.
    /// </summary>
    public abstract class DataContractSerializer
    {
        protected string _rootName;
        protected string _rootNamespace;
        protected string _localNamespace;
        protected bool _NamespaceIsDefined;
        protected bool _CompressByteArrays;
        protected string _prefix;
        private ArrayList _attributesFound;

        public WsMtomBodyParts BodyParts;

        /// <summary>
        /// Creates an instance of a DataContractSerilizer used to serialize and deserialize xml to
        /// and from a type.
        /// </summary>
        /// <param name="rootName">A string containing the name of the root element of a type.</param>
        /// <param name="rootNamespace">A string containing the namespace of the root element of a type.</param>
        protected DataContractSerializer(string rootName, string rootNamespace) :
            this(rootName, rootNamespace, rootNamespace)
        {
        }

        /// <summary>
        /// Creates an instance of a DataContractSerilizer used to serialize and deserialize xml to
        /// and from a type.
        /// </summary>
        /// <param name="rootName">A string containing the name of the root element of a type.</param>
        /// <param name="rootNamespace">A string containing the namespace of the root element of a type.</param>
        /// <param name="localNamespace">A string containing the namespace of the child element of a type.</param>
        protected DataContractSerializer(string rootName, string rootNamespace, string localNamespace)
        {
            _attributesFound = new ArrayList();
            BodyParts = new WsMtomBodyParts();
            _rootName = rootName;
            _rootNamespace = rootNamespace;
            _localNamespace = localNamespace;
            _CompressByteArrays = true;
        }

        /// <summary>
        /// Reads the XML stream with an XmlReader and returns the deserialized object
        /// </summary>
        /// <param name="reader">An XmlReader positioned on the root element of the data contract type.</param>
        /// <returns></returns>
        public abstract object ReadObject(XmlReader reader);

        /// <summary>
        /// Writes the object data (starting XML element, content, and closing element)
        /// to an XML document or stream with an XmlWriter
        /// </summary>
        /// <param name="writer">An XmlWriter used to write the object.</param>
        /// <param name="graph">An object containing the data to serialize.</param>
        public abstract void WriteObject(XmlWriter writer, object graph);

        /// <summary>
        /// Builds an WsMtomBodyPart object.
        /// </summary>
        /// <param name="content">A byte array containing the binary body part content.</param>
        /// <param name="contentID">A string containing a unique content identifier.</param>
        /// <returns>An complete WsMtomBodyPart.</returns>
        public WsMtomBodyPart CreateNewBodyPart(byte[] content, string contentID)
        {
            WsMtomBodyPart bodyPart = new WsMtomBodyPart();
            bodyPart.Content = new MemoryStream(content);
            bodyPart.ContentID = contentID;
            bodyPart.ContentTransferEncoding = "binary";
            bodyPart.ContentType = "application/xop+xml;charset=UTF-8;type=\"application/soap+xml\"";
            return bodyPart;
        }

        /// <summary>
        /// Gets the content of an WsMtomBodyPart.
        /// </summary>
        /// <param name="contentID">A string containing a unique content identifier.</param>
        /// <param name="bodyParts">A WsMtomBodyParts collection.</param>
        /// <returns>A byte array containing the body part content.</returns>
        public byte[] GetBodyPartContent(string contentID, WsMtomBodyParts bodyParts)
        {
            WsMtomBodyPart bodyPart = null;

            // XmlReader will convert ':' and '/' to their respective HEX values (%3A, %2F)
            // for attributes values.  So we need to switch them back in order for the compare to work
            // We may want to change the XML parser to handle this in a more generic way
            int idx = contentID.IndexOf("%");
            while(idx != -1)
            {
                if(contentID[idx+1] == '3' && contentID[idx+2] == 'A')
                {
                    contentID = contentID.Substring(0, idx) + ":" + contentID.Substring(idx + 3);
                }
                else if(contentID[idx+1] == '2' && contentID[idx+2] == 'F')
                {
                    contentID = contentID.Substring(0, idx) + "/" + contentID.Substring(idx + 3);
                }
                idx = contentID.IndexOf("%", idx);
            }

            if ((bodyPart = bodyParts["<" + contentID.Substring(4) + ">"]) != null)
                return bodyPart.Content.ToArray();
            return null;
        }

        /// <summary>
        /// Check for the objects root element at the current reader position.
        /// </summary>
        /// <param name="reader">An XmlReader positioned at a start element.</param>
        /// <param name="nillable">True indicates the element can contain the nil attribute in place
        /// of actual content.
        /// </param>
        /// <param name="isRequired">True indicate the elements minoccurs value is > 0 so this element must exist.
        /// </param>
        /// <returns>
        /// True if the expected parent element is read or if isNillable is true and the nil attribute is present.
        ///</returns>
        ///<exception cref="XmlException">
        /// Thrown if nillable is false and the element does not contain content or if IsRequiredis true
        /// and the reader is not positioned on the specified start element.
        ///</exception>
        protected bool IsParentStartElement(XmlReader reader, bool nillable, bool isRequired)
        {
            return ReadElement(reader, _rootName, nillable, isRequired, _rootNamespace);
        }

        /// <summary>
        /// Check for the specified child element at the current reader position.
        /// </summary>
        /// <param name="reader">An XmlReader positioned at a start element.</param>
        /// <param name="typeName">Expected Element name.</param>
        /// <param name="nillable">True indicates the element can contain the nil attribute in place
        /// of actual content.
        /// </param>
        /// <param name="isRequired">True indicate the elements minoccurs value is > 0 so this element must exist.
        /// </param>
        /// <returns>
        /// True if the expected element is read or if isNillable is true and the nil attribute is present.
        ///</returns>
        ///<exception cref="XmlException">
        /// Thrown if nillable is false and the element does not contain content or if IsRequiredis true
        /// and the reader is not positioned on the specified start element.
        ///</exception>
        protected bool IsChildStartElement(XmlReader reader, string typeName, bool nillable, bool isRequired)
        {
            return ReadElement(reader, typeName, nillable, isRequired, _localNamespace);
        }

        /// <summary>
        /// Determines if the xml reader is positioned at the specified element applying XmlSchema
        /// nillable and modified minoccurs rules.
        /// </summary>
        /// <param name="reader">An XmlReader.</param>
        /// <param name="typeName">Expected element name.</param>
        /// <param name="nillable">True indicates the element can contain the nil attribute in place
        /// of actual content.
        /// </param>
        /// <param name="isRequired">True indicate the elements minoccurs value is > 0 so this element must exist.
        /// </param>
        /// <param name="targetNamespace">the namespace of the element to be read.</param>
        /// <returns>
        /// True if the expected element is read or if isNillable is true and the nil attribute is read,
        ///</returns>
        ///<exception cref="XmlException">
        /// Thrown if nillable is false and the element does not contain content or if IsRequiredis true
        /// and the reader is not positioned on the specified start element.
        ///</exception>
        ///<remarks>
        /// Method compares start element name to typeName, if the tag does not match typeName and isRequired is true,
        /// throw exception. If tag does not match typeName and isRequired is false return false.
        /// If tag matches and nillable is true and nil attribute is true return false.
        /// if tag matches and return true.
        ///</remarks>
        private bool ReadElement(XmlReader reader, string typeName, bool nillable, bool isRequired, string targetNamespace)
        {
            bool isReadable;

            // Make sure this is the element we are looking for
            if (reader.IsStartElement(typeName, targetNamespace))
            {
                if (nillable && reader.MoveToAttribute("nil", WsWellKnownUri.SchemaNamespaceUri))
                {
                    isReadable = reader.Value == "true" ? false : true;
                    reader.MoveToElement();
                    reader.Read();
                }
                else
                    isReadable = true;
            }
            else if (isRequired == false)
                isReadable = false;
            else
            {
                throw new XmlException("Parsing Error. Type " + typeName + " must be present.");
            }

            return isReadable;
        }

        /// <summary>
        /// An enum containing Wildcard schema component namespace validation types
        /// </summary>
        public enum WildcardValidationType
        {
            Any,
            Local,
            Other
        }

        /// <summary>
        /// Starting at the current reader position, builds an arrray of
        /// XmlElement objects up to the end tag of the enclosing element or until namespace
        /// validation rules are violated.
        /// </summary>
        /// <param name="reader">An XmlReader positioned at a start element.</param>
        /// <param name="isRequired">True if minoccurs > 0 for an element.</param>
        /// <returns>An array of XmlElements.</returns>
        /// <remarks>ProcessContent validation is not supported.</remarks>
        protected WsXmlNode[] ReadAnyElement(XmlReader reader, bool isRequired)
        {
            if (isRequired && !reader.IsStartElement())
                throw new XmlException("Parsing Error. Any element is required.");

            ArrayList nodeList = new ArrayList();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                WsXmlNode tempNode = new WsXmlNode(reader);
                nodeList.Add(tempNode);
            }

            return (WsXmlNode[])nodeList.ToArray(typeof(WsXmlNode));
        }

        /// <summary>
        /// Builds an Array of XmlAttribute objects containing any attribute that is not required
        /// ad passes a specified Wildcard namespace validation rules
        /// </summary>
        /// <param name="reader">An XmlReader positioned in a document.</param>
        /// <returns>An array of XmlElements.</returns>
        /// <remarks>ProcessContent validation is not supported.</remarks>
        protected WsXmlAttribute[] ReadAnyAttribute(XmlReader reader)
        {
            ArrayList attribList = new ArrayList();
            
            if(reader.MoveToFirstAttribute())
            {
                while (true)
                {
                    if (reader.Name.IndexOf("xmlns") != 0)
                    {
                        bool found = false;
                        for (int i = 0; i < _attributesFound.Count; i++)
                        {
                            if ((String)(_attributesFound[i]) == reader.Name)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            WsXmlAttribute attrib = new WsXmlAttribute();
                            attrib.Prefix = reader.Prefix;
                            attrib.LocalName = reader.LocalName;
                            attrib.NamespaceURI = reader.NamespaceURI;
                            attrib.Value = reader.Value;
                            attribList.Add(attrib);
                        }
                    }

                    if(!reader.MoveToNextAttribute()) break;
                }
            }

            return (WsXmlAttribute[])attribList.ToArray(typeof(WsXmlAttribute));
        }

        /// <summary>
        /// Checks to see if the specified attribute exists at the current XmlReader position.
        /// </summary>
        /// <param name="reader">An XmlReader positioned on an element attribute.</param>
        /// <param name="attributeName">A string containing the name of the attribute to read.</param>
        /// <returns>True if the attribute is found, else false.</returns>
        protected bool IsAttribute(XmlReader reader, string attributeName)
        {
            if (reader.MoveToAttribute(attributeName) == true)
            {
                _attributesFound.Add(reader.Name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes an object to the root element of an xml stream.
        /// </summary>
        /// <param name="writer">An XmlWriter used to write to the stream.</param>
        /// <param name="nillable">Used to enforce nillable schema attibute validation.
        /// If true and the object is null, write the nil attribute instead of a value.
        /// </param>
        /// <param name="isRequired">Used to enforse minoccurs schema attribute validation. If true
        /// the object cannot be null.
        /// </param>
        /// <param name="obj">An object contaiing the value to serilaize.</param>
        /// <returns>
        /// True if nillable is true and obj is null or not null.
        /// True is isRequired is true and obj is not null or isRequired is false and value is null or not null.
        /// Else false.
        /// </returns>
        /// <remarks>
        /// This helper method provides basic nillable and minoccurs schema validation.
        /// Since the method writes te parent or root element the root name and root namespace are used.
        ///</remarks>
        protected bool WriteParentElement(XmlWriter writer, bool nillable, bool isRequired, object obj)
        {
            return WriteElement(writer, _rootName, nillable, isRequired, obj);
        }

        /// <summary>
        /// Writes an object to an element in the xml stream.
        /// </summary>
        /// <param name="writer">An XmlWriter used to write to the stream.</param>
        /// <param name="typeName">A string containing the element name to write.</param>
        /// <param name="nillable">Used to enforce nillable schema attibute validation.
        /// If true and the object is null, write the nil attribute instead of a value.
        /// </param>
        /// <param name="isRequired">Used to enforse minoccurs schema attribute validation. If true
        /// the object cannot be null.
        /// </param>
        /// <param name="obj">An object contaiing the value to serilaize.</param>
        /// <returns>
        /// True if nillable is true and obj is null or not null.
        /// True is isRequired is true and obj is not null or isRequired is false and value is null or not null.
        /// Else false.
        /// </returns>
        /// <remarks>
        /// This helper method provides basic nillable and minoccurs schema validation.
        ///</remarks>
        protected bool WriteChildElement(XmlWriter writer, string typeName, bool nillable, bool isRequired, object obj)
        {
            return WriteElement(writer, typeName, nillable, isRequired, obj);
        }

        // Writes the opening XML element using an XmlWriter
        // isNull determines if the object is null. defaultNameSpace is the default name space
        // of the element.

        /// <summary>
        /// Writes an object to an element in the xml stream.
        /// </summary>
        /// <param name="writer">An XmlWriter used to write to the stream.</param>
        /// <param name="typeName">A string containing the element name to write.</param>
        /// <param name="nillable">Used to enforce nillable schema attibute validation.
        /// If true and the object is null, write the nil attribute instead of a value.
        /// </param>
        /// <param name="isRequired">Used to enforse minoccurs schema attribute validation. If true
        /// the object cannot be null.
        /// </param>
        /// <param name="obj">An object contaiing the value to serilaize.</param>
        /// <returns>
        /// True if nillable is true and obj is null or not null.
        /// True is isRequired is true and obj is not null or isRequired is false and value is null or not null.
        /// Else false.
        /// </returns>
        /// <remarks>
        /// This helper method provides basic nillable and minoccurs schema validation.
        ///</remarks>
        internal bool WriteElement(XmlWriter writer, string typeName, bool nillable, bool isRequired, object obj)
        {
            bool retVal = true;

            // if obj is null check requirements
            if (obj == null)
            {
                // If this element is not nillable throw exception
                if (nillable == false && isRequired == true)
                    throw new XmlException("not nillable");

                // If this element is not nillable but is also not required return false
                if (nillable == false && isRequired == false)
                    return false;

                // If the object is null and IsNillable == true and IsRequired == true
                if (nillable == true && isRequired == true)
                {
                    if (_NamespaceIsDefined)
                        writer.WriteStartElement(_prefix, typeName, null);
                    else
                    {
                        _prefix = writer.WriteStartElement(null, typeName, _localNamespace);
                        _NamespaceIsDefined = true;
                    }

                    writer.WriteAttributeString("nil", WsWellKnownUri.SchemaNamespaceUri, "true");
                    writer.WriteEndElement();
                }

                retVal = false;
            }
            else
            {
                // Value is set so write normal
                // First time through. if the writer does not already contain the namespace
                // a new prefix is created and the new namespace is writen to this element.
                // Remaining elements in this scope are writen without a namespace or prefix.
                if (_NamespaceIsDefined)
                    writer.WriteStartElement(_prefix, typeName, null);
                else
                {
                    _prefix = writer.WriteStartElement(null, typeName, _localNamespace);
                    _NamespaceIsDefined = true;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Writes an array of elements at the current position in the document.
        /// </summary>
        /// <param name="writer">The XmlWriter used to write the elements.</param>
        /// <param name="elements">An array of XmlElements to write.</param>
        protected void WriteAnyElement(XmlWriter writer, WsXmlNode[] elements, bool isRequired)
        {
            if (elements == null || elements.Length == 0)
                if (isRequired)
                    throw new XmlException("Parsing Error. The parameter element is either null or empty.");
                else
                    return;
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].WriteTo(writer);
            }
        }

        /// <summary>
        /// Adds the collection of XmlAttributes to the current element.
        /// </summary>
        /// <param name="writer">The XmlWriter use dto write the attributes.</param>
        /// <param name="attributes">An array of XmlAttributes to write.</param>
        protected void WriteAnyAttribute(XmlWriter writer, WsXmlAttribute[] attributes)
        {
            if (attributes == null)
                return;
            for (int i = 0; i < attributes.Length; i++)
            {
                writer.WriteAttributeString(attributes[i].Prefix, attributes[i].LocalName, attributes[i].NamespaceURI, attributes[i].Value);
            }
        }

        /// <summary>
        /// Wtites an attribute to the current element.
        /// </summary>
        /// <param name="writer">The XmlWriter.</param>
        /// <param name="localName">The local name of the attribute.</param>
        /// <param name="isRequired">True if this attribute is required.</param>
        /// <param name="value">The value of the attribute.</param>
        protected void WriteAttribute(XmlWriter writer, string localName, bool isRequired, object value)
        {
            if (IsInitialized(value))
            {
                string valueString = "";
                if (value is Array)
                {
                    int arrayLen = ((Array)value).Length;
                    for (int i = 0; (i < arrayLen); ++i)
                    {
                        valueString += ((Array)value).GetValue(i).ToString();
                        valueString += arrayLen - i > 1 ? " " : "";
                    }
                }
                else
                    valueString = value.ToString();

                writer.WriteAttributeString(null, localName, null, valueString);
            }
            else if (isRequired == true)
                throw new ArgumentException("Schemas rule violation. Attribute " + localName + " is required but has not be initialized.");
        }

        /// <summary>
        /// Determines if an object has been initialized.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True is the object has been initialized.</returns>
        /// <remarks>This method is required so that we never try to write an attribute defined in the
        /// data contract that has not been initialized.</remarks>
        private bool IsInitialized(object obj)
        {
            if (obj == null)
                return false;
            return true;
        }
    }

    /// <summary>
    /// Class contains methods for converting between common language runtime types and XML Schema
    /// definition language (XSD) types.
    /// </summary>
    public static class WsXmlConvert
    {

        /// <summary>
        /// Converts the System.String in XmlSchema duration format to System.TimeSpan equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A TimeSpan object equivalent.</returns>
        public static TimeSpan ToTimeSpan(string s)
        {
            Ws.Services.Utilities.WsDuration duration = new Ws.Services.Utilities.WsDuration(s);
            return new TimeSpan(duration.Ticks);
        }

        /// <summary>
        /// Converts a System.String containing an Xml Schema dateTime to a System.DateTime equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A DateTime object equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static DateTime ToDateTime(string s)
        {
            try
            {
                //yyyy-MM-ddThh:mm:ss.ssZ
                string[] dateAndTime = s.Split('T');
                if (dateAndTime.Length != 2)
                    throw new Exception("Invalid DateTime Format.");
                string[] dateParts = dateAndTime[0].Split('-');
                if (dateParts.Length != 3)
                    throw new Exception("Invalid Date.");

                uint year, month, day, hour, minute, second, millisecond;

                if (dateParts[0].Length != 4)
                    throw new Exception("Invalid Year.");

                year = ToUInt32(dateParts[0]);

                if (dateParts[1].Length != 2)
                    throw new Exception("Invalid Month.");

                month = ToUInt32(dateParts[1]);
                if (month < 1 && month > 12)
                    throw new Exception("Invalid Month.");

                if (dateParts[2].Length != 2)
                    throw new Exception("Invalid Day.");

                day = ToUInt32(dateParts[2]);

                string[] timeParts = dateAndTime[1].Split(':');

                if (timeParts.Length != 3)
                    throw new Exception("Invalid Time Format.");

                if (timeParts[0].Length != 2)
                    throw new Exception("Invalid Hour.");

                hour = ToUInt32(timeParts[0]);

                if (timeParts[1].Length != 2)
                    throw new Exception("Invalid Minute.");

                minute = ToUInt32(timeParts[1]);

                if (timeParts[2].IndexOf('Z') != timeParts[2].Length - 1)
                    throw new Exception("Invalid Timezone.");

                string[] secAndTimeZone = timeParts[2].Split('.');

                if (secAndTimeZone[0].Length < 2)
                    throw new Exception("Invalid Second.");

                second = ToUInt32(secAndTimeZone[0].Substring(0, 2));

                if (second > 60)
                    throw new Exception("Invalid Second.");

                if (secAndTimeZone.Length == 2)
                {
                    if (secAndTimeZone[1].Length < 2)
                        throw new Exception("Invalid Second.");

                    millisecond = ToUInt32(secAndTimeZone[1].Substring(0, secAndTimeZone[1].Length-1));
                }
                else
                    millisecond = 0;

                //ignore time zone for now.

                DateTime d = new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second, (int)millisecond);

                return d;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Unable to convert \"" + s + "\" to a DateTime object. " + e.Message);
            }
        }

        /// <summary>
        /// Converts the System.String to a System.Boolean equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A bool value equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static bool ToBoolean(string boolean)
        {
            string tempBool = boolean.ToLower();
            if (tempBool == "true" || tempBool == "1")
                return true;
            else if (tempBool == "false" || tempBool == "0")
                return false;
            else
                throw new ArgumentException("Unable to convert \"" + boolean + "\" to a boolean");
        }

        /// <summary>
        /// Converts the System.String to a System.Byte equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A byte value equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static byte ToByte(string s)
        {
            return System.Convert.ToByte(s);
        }

        /// <summary>
        /// Converts the System.String to a System.SByte equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A byte value equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static sbyte ToSByte(string s)
        {
            return System.Convert.ToSByte(s);
        }

        /// <summary>
        /// Converts the System.String to a short equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A short value equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static short ToInt16(string s)
        {
            return System.Convert.ToInt16(s);
        }

        /// <summary>
        /// Converts the System.String to a int equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A int value equivalent.</returns>
        /// <exception cref="Exception">If the string format is invalid.</exception>
        public static int ToInt32(string s)
        {
            return System.Convert.ToInt32(s);
        }

        /// <summary>
        /// Converts the System.String to a long equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A long value equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static long ToInt64(string s)
        {
            return System.Convert.ToInt64(s);
        }

        /// <summary>
        /// Converts the System.String to a ushort equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A ushort value equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static ushort ToUInt16(string s)
        {
            return System.Convert.ToUInt16(s);
        }

        /// <summary>
        /// Converts the System.String to a uint equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A uint value equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static uint ToUInt32(string s)
        {
            return System.Convert.ToUInt32(s);
        }

        /// <summary>
        /// Converts the System.String to a ulong equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A ulong value equivalent.</returns>
        /// <exception cref="ArgumentException">If the string format is invalid.</exception>
        public static ulong ToUInt64(string s)
        {
            return System.Convert.ToUInt64(s);
        }

        // And Sometime later private static decimal ToDecimal(string s)

        /// <summary>
        /// Converts the System.String to a float equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A float value equivalent.</returns>
        /// <exception cref="Exception">If the string format is invalid.</exception>
        public static float ToSingle(string s)
        {
            return (float)System.Convert.ToDouble(s);
        }

        /// <summary>
        /// Converts the System.String to a double equivalent.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A double value equivalent.</returns>
        /// <exception cref="Exception">If the string format is invalid.</exception>
        public static double ToDouble(string s)
        {
            return System.Convert.ToDouble(s);
        }

        /// <summary>
        ///  Converts the System.TimeSpan to a System.String containing and XmlSchema duration.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the TimeSpan in XmlSchema duration format.</returns>
        public static string ToString(TimeSpan value)
        {
            // WsDuration converts second to duration
            Ws.Services.Utilities.WsDuration duration = new Ws.Services.Utilities.WsDuration(value);
            return duration.DurationString;
        }

        /// <summary>
        ///  Converts the System.DateTime to a System.String containing the date time converted to Xml Schema dateTime.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the DateTime converted to xs:dateTime.</returns>
        public static string ToString(DateTime value)
        {
            if (value.Millisecond == 0)
                return value.ToString("yyyy-MM-ddTHH:mm:ssZ");
            else
                return value.ToString("yyyy-MM-ddTHH:mm:ss.ffZ");
        }

        /// <summary>
        ///  Converts the System.Byte to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the Byte.</returns>
        public static string ToString(Byte value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.SByte to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the SByte.</returns>
        public static string ToString(SByte value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.Boolean to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the Boolean.</returns>
        public static string ToString(Boolean value)
        {
            return (value == true) ? "true" : "false";
        }

        /// <summary>
        ///  Converts the System.Int16 to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the Int16.</returns>
        public static string ToString(Int16 value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.Int32 to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the Int32.</returns>
        public static string ToString(Int32 value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.Int64 to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the Int64.</returns>
        public static string ToString(Int64 value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.UInt16 to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the UInt16.</returns>
        public static string ToString(UInt16 value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.UInt32 to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the UInt32.</returns>
        public static string ToString(UInt32 value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.UInt64 to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the UInt64.</returns>
        public static string ToString(UInt64 value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.Double to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the Double.</returns>
        public static string ToString(Double value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the float to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the float.</returns>
        public static string ToString(float value)
        {
            return value.ToString();
        }

        /// <summary>
        ///  Converts the System.Uri to a System.String.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the Uri.</returns>
        public static string ToString(System.Uri value)
        {
            return value.AbsoluteUri;
        }
    }
}

namespace Ws.ServiceModel
{

    /// <summary>
    /// Indicates that an interface or a class defines a service contract in a application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ServiceContractAttribute : Attribute
    {
        private string m_name;
        private string m_namespace;
        private Type m_callbackContract;

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.ServiceContractAttribute class.
        /// </summary>
        public ServiceContractAttribute() { }

        /// <summary>
        /// Gets or sets the name for the <portType> element in Web Services Description Language (WSDL).
        /// </summary>
        public string Name { get { return m_name; } set { m_name = value; } }
        //
        /// <summary>
        /// Gets or sets the namespace of the <portType> element in Web Services Description Language (WSDL).
        /// </summary>
        public string Namespace { get { return m_namespace; } set { m_namespace = value; } }

        /// <summary>
        /// Gets or sets the type of callback contract when the contract has callback operations defined in Web Service Description Language (WSDL).
        /// </summary>
        public Type CallbackContract { get { return m_callbackContract; } set { m_callbackContract = value; } }
    }

    /// <summary>
    /// Indicates that an interface or a class or a operation has a binding policy assertion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class PolicyAssertionAttribute : Attribute
    {
        private string m_name;
        private string m_namespace;
        private string m_policyId;

        /// <summary>
        /// Initializes a new instance of the PolicyAssertionAttribute class.
        /// </summary>
        public PolicyAssertionAttribute() { }

        /// <summary>
        /// Gets or sets the name for the policy assertion defined in Web Services Description Language (WSDL).
        /// </summary>
        public string Name { get { return m_name; } set { m_name = value; } }
        //
        /// <summary>
        /// Gets or sets the namespace of the policy assertion defined in Web Services Description Language (WSDL).
        /// </summary>
        public string Namespace { get { return m_namespace; } set { m_namespace = value; } }

        /// <summary>
        /// Gets or sets the Policy ID attribute defined in Web Service Description Language (WSDL).
        /// </summary>
        public string PolicyID { get { return m_policyId; } set { m_policyId = value; } }
    }

    /// <summary>
    /// Indicates that a method defines an operation that is part of a service contract
    /// in a application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OperationContractAttribute : Attribute
    {
        private string m_action;
        private bool   m_isOneWay;
        private string m_operationName;
        private string m_replyAction;

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.OperationContractAttribute class.
        /// </summary>
        public OperationContractAttribute() { }

        /// <summary>
        ///  Gets or sets the WS-Addressing action of the request message.
        /// </summary>
        public string Action { get { return m_action; } set { m_action = value; } }

        /// <summary>
        /// Gets or sets a value that indicates whether an operation returns a reply message.
        /// </summary>
        public bool IsOneWay { get { return m_isOneWay; } set { m_isOneWay = value; } }

        /// <summary>
        ///  Gets or sets the name of the operation.
        /// </summary>
        public string Name { get { return m_operationName; } set { m_operationName = value; } }

        /// <summary>
        ///  Gets or sets the value of the SOAP action for the reply message of the operation.
        /// </summary>
        public string ReplyAction { get { return m_replyAction; } set { m_replyAction = value; } }
    }

    /// <summary>
    /// Controls the name of the request and response parameter names. Cannot be
    /// used with System.ServiceModel.Channels.Message or message contracts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
    public sealed class MessageParameterAttribute : Attribute
    {
        private string m_name;

        /// <summary>
        ///  Initializes a new instance of the System.ServiceModel.MessageParameterAttribute class.
        /// </summary>
        public MessageParameterAttribute() { }

        /// <summary>
        ///  Obtains or sets the name attribute of the parameter in the XML Schema (XSD).
        /// </summary>
        public string Name { get { return m_name; } set { m_name = value; } }
    }

    /// <summary>
    /// When applied to the member of a type, specifies that the member is part of
    /// a data contract and is serializable by the Ws.Services.Serialization.DataContractSerializer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class DataMemberAttribute : Attribute
    {
        private bool m_emitDefaultValue;
        private bool m_isAttribute;
        private bool m_isRequired;
        private bool m_isNillable;
        private string m_name;
        private int m_order;

        /// <summary>
        /// Initializes a new instance of the System.Runtime.Serialization.DataMemberAttribute class.
        /// </summary>
        public DataMemberAttribute() 
        {
            m_isRequired = true;
            m_isNillable = true;            
            m_order = 0;
        }

        /// <summary>
        /// Gets or sets a value that specifies whether to serialize the default value
        /// for a field or property being serialized.
        /// </summary>
        /// <remarks>
        /// True if the default value for a member should be generated in the serialization
        /// stream; otherwise, false. The default is true.
        ///</remarks>
        public bool EmitDefaultValue { get { return m_emitDefaultValue; } set { m_emitDefaultValue = value; } }

        /// <summary>
        ///  Gets or sets a value that indicates that this member is an attribute.
        /// </summary>
        /// <remarks>True if the member is an attribute; otherwise, false.</remarks>
        public bool IsAttribute { get { return m_isAttribute; } set { m_isAttribute = value; } }

        /// <summary>
        /// Gets or sets a value that instructs the serialization engine that the member
        /// must be present when reading or deserializing.
        /// </summary>
        /// <remarks>True if the member is required; otherwise, false.</remarks>
        public bool IsRequired { get { return m_isRequired; } set { m_isRequired = value; } }

        /// <summary>
        ///  Gets or sets a value that indicates that a members value may be blank
        /// </summary>
        /// <remarks>True if the member can be blank; otherwise, false.</remarks>
        public bool IsNillable { get { return m_isNillable; } set { m_isNillable = value; } }

        /// <summary>
        ///  Gets or sets a data member name.
        /// </summary>
        /// <remarks>Property contains the name of the data member. The default is the name of the target
        /// that the attribute is applied to.
        /// </remarks>
        public string Name { get { return m_name; } set { m_name = value; } }

        /// <summary>
        ///  Gets or sets the order of serialization and deserialization of a member.
        /// </summary>
        public int Order { get { return m_order; } set { m_order = value; } }
    }

    /// <summary>
    /// Specifies that the type defines or implements a data contract and is serializable
    /// by a derived DataContractSerializer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class DataContractAttribute : Attribute
    {
        private string m_name;
        private string m_namespace;

        /// <summary>
        /// Initializes a new instance of the System.Runtime.Serialization.DataContractAttribute class.
        /// </summary>
        public DataContractAttribute() { }

        // Summary:
        //     Gets or sets the name of the data contract for the type.
        //
        // Returns:

        /// <summary>
        ///  Gets or sets the name of the data contract for the type.
        /// </summary>
        /// <remarks>
        /// The local name of a data contract. The default is the name of the class that
        /// the attribute is applied to.
        /// </remarks>
        public string Name { get { return m_name; } set { m_name = value; } }

        /// <summary>
        ///  Gets or sets the namespace for the data contract for the type.
        /// </summary>
        /// <remarks>The namespace of the contract.</remarks>
        public string Namespace { get { return m_namespace; } set { m_namespace = value; } }
    }

    /// <summary>
    /// Specifies that the field is an enumeration member and should be serialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class EnumMemberAttribute : Attribute
    {
        private string m_enumValue = null;

        // Summary:
        //     Initializes a new instance of the System.Runtime.Serialization.EnumMemberAttribute
        //     class.

        /// <summary>
        /// Initializes a new instance of the System.Runtime.Serialization.EnumMemberAttribute class.
        /// </summary>
        public EnumMemberAttribute() { }

        /// <summary>
        /// Gets or sets the value associated with the enumeration member the attribute is applied to.
        /// </summary>
        public string Value { get { return m_enumValue; } set { m_enumValue = value; } }
    }
}


