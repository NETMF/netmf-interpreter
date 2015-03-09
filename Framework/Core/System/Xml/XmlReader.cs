////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.SPOT;

namespace System.Xml
{
    // Internal structure to keep track of node information
    internal class XmlReader_XmlNode
    {
        public String Name;
        public String NamespaceURI;
        public String Prefix;
        public String LocalName;
        public uint Flags; // Number of namespace declaration, has default ns declaration ... etc

        public XmlReader_XmlNode()
        {
            Name = String.Empty;
            Prefix = String.Empty;
            LocalName = String.Empty;
            NamespaceURI = String.Empty;
            Flags = 0;
        }
    }

    // Internal structure to keep track of attributes
    internal class XmlReader_XmlAttribute
    {
        public String Name;
        public String NamespaceURI;
        public String Prefix;
        public String LocalName;
        public String Value;
    }

    // Internal structure for namespace lookup
    internal class XmlReader_NamespaceEntry
    {
        public String Prefix;
        public String NamespaceURI;
    }

    /// <summary>
    /// Represents a reader that provides fast, non-cached forward only stream access to XML data.
    /// </summary>
    public class XmlReader : IDisposable
    {
        // data to be parsed, these are updated prior to calling ReadInternal()
        // and will be updated by ReadInternal to reflect use.
        private byte[] _buffer;
        private int _offset;
        private int _length;

        private const int BufferSize = 1024;

        // Information to keep track of the XML document, these are updated and maintain
        // by from the native code, and should only be read (not modified) from the managed code
        private Stack _xmlNodes; // Stack of XmlNode
        private Stack _namespaces; // Stack of XmlReader_NamespaceEntry
        private Stack _xmlSpaces; // Stack of XmlSpace
        private Stack _xmlLangs; // Stack of String
        private XmlNodeType _nodeType;
        private String _value;
        private bool _isEmptyElement;
        private ArrayList _attributes; // List of XmlAttributes
        private XmlNameTable _nameTable;
        private Object _state; // Handle to native structure
        private bool _isDone;

        private int _currentAttribute;
        private Stream _stream;
        private ReadState _readState;

        static readonly String Xml = "xml";
        static readonly String Xmlns = "xmlns";
        static readonly String XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        static readonly String XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void Initialize(uint settings);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern int ReadInternal(uint options);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool StringRefEquals(String str1, String str2);

        //--//

        /// <summary>
        /// Gets the type of the current node.
        /// </summary>
        public virtual XmlNodeType NodeType
        {
            get
            {
                return (_currentAttribute >= 0) ? XmlNodeType.Attribute : _nodeType;
            }
        }

        /// <summary>
        /// Gets the name of the current node, including the namespace prefix.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (_currentAttribute >= 0)
                {
                    return ((XmlReader_XmlAttribute)_attributes[_currentAttribute]).Name;
                }
                else
                {
                    return ((XmlReader_XmlNode)_xmlNodes.Peek()).Name;
                }
            }
        }

        /// <summary>
        /// Gets the name of the current node without the namespace prefix.
        /// </summary>
        public virtual string LocalName
        {
            get
            {
                if (_currentAttribute >= 0)
                {
                    return ((XmlReader_XmlAttribute)_attributes[_currentAttribute]).LocalName;
                }
                else
                {
                    return ((XmlReader_XmlNode)_xmlNodes.Peek()).LocalName;
                }
            }
        }

        /// <summary>
        /// Gets the namespace URI (as defined in the W3C Namespace specification) of
        /// the node on which the reader is positioned.
        /// </summary>
        public virtual string NamespaceURI
        {
            get
            {
                if (_currentAttribute >= 0)
                {
                    return ((XmlReader_XmlAttribute)_attributes[_currentAttribute]).NamespaceURI;
                }
                else
                {
                    return ((XmlReader_XmlNode)_xmlNodes.Peek()).NamespaceURI;
                }
            }
        }

        /// <summary>
        /// Gets the namespace prefix associated with the current node.
        /// </summary>
        public virtual string Prefix
        {
            get
            {
                if (_currentAttribute >= 0)
                {
                    return ((XmlReader_XmlAttribute)_attributes[_currentAttribute]).Prefix;
                }
                else
                {
                    return ((XmlReader_XmlNode)_xmlNodes.Peek()).Prefix;
                }
            }
        }

        private const uint HasValueBitmap = 0x1fc; // 001 1111 1100
        // 0 None,
        // 0 Element,
        // 1 Attribute,
        // 1 Text,
        // 1 CDATA,
        // 1 ProcessingInstruction,
        // 1 Comment,
        // 1 Whitespace,
        // 1 SignificantWhitespace,
        // 0 EndElement,
        // 0 XmlDeclaration

        /// <summary>
        /// Gets a value indicating whether the current node can have a Value other
        /// than String.Empty.
        /// </summary>
        public virtual bool HasValue
        {
            get
            {
                return 0 != (HasValueBitmap & (1 << (int)this.NodeType));
            }
        }

        /// <summary>
        /// Gets the text value of the current node.
        /// </summary>
        public virtual string Value
        {
            get
            {
                if (_currentAttribute >= 0)
                {
                    return ((XmlReader_XmlAttribute)_attributes[_currentAttribute]).Value;
                }
                else
                {
                    return _value;
                }
            }
        }

        /// <summary>
        /// Gets the depth of the current node in the XML element stack.
        /// </summary>
        public virtual int Depth
        {
            get
            {
                if (_currentAttribute >= 0)
                {
                    return _xmlNodes.Count;
                }
                else
                {
                    return _xmlNodes.Count - 1;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current node is an empty element
        /// (for example, &lt;MyElement/&gt;).
        /// </summary>
        public virtual bool IsEmptyElement
        {
            get
            {
                return (_currentAttribute == -1) ? _isEmptyElement : false;
            }
        }

        /// <summary>
        /// Gets the current xml:space scope.
        /// </summary>
        public virtual XmlSpace XmlSpace
        {
            get
            {
                return (XmlSpace)(int)_xmlSpaces.Peek();
            }
        }

        /// <summary>
        /// Gets the current xml:lang scope.
        /// </summary>
        public virtual string XmlLang
        {
            get
            {
                return (String)_xmlLangs.Peek();
            }
        }

        /// <summary>
        /// Gets the number of attributes on the current node.
        /// </summary>
        public virtual int AttributeCount
        {
            get
            {
                return _attributes.Count;
            }
        }

        /// <summary>
        /// Gets the value of the attribute with the specified name.
        /// </summary>
        /// <param name="name">The qualified name of the attribute.</param>
        /// <returns>The value of the specified attribute. If the attribute is not found, null is returned.</returns>
        public virtual string GetAttribute(string name)
        {
            int index = FindAttribute(name);

            if (index >= 0) return ((XmlReader_XmlAttribute)_attributes[index]).Value;

            return null;
        }

        private int FindAttribute(String name)
        {
            name = _nameTable.Get(name);

            if (name != null)
            {
                int count = _attributes.Count;

                for (int i = 0; i < count; i++)
                {
                    if (Object.ReferenceEquals(name, ((XmlReader_XmlAttribute)_attributes[i]).Name))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private int FindAttribute(String localName, String namespaceURI)
        {
            localName = _nameTable.Get(localName);
            namespaceURI = _nameTable.Get(namespaceURI);

            if (localName != null && namespaceURI != null)
            {
                int count = _attributes.Count;
                XmlReader_XmlAttribute attr;

                for (int i = 0; i < count; i++)
                {
                    attr = (XmlReader_XmlAttribute)_attributes[i];

                    if (Object.ReferenceEquals(localName, attr.LocalName) &&
                        Object.ReferenceEquals(namespaceURI, attr.NamespaceURI))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the value of the attribute with the specified local name and namespace URI.
        /// </summary>
        /// <param name="name">The local name of the attribute.</param>
        /// <param name="namespaceURI">The namespace URI of the attribute.</param>
        /// <returns>The value of the specified attribute. If the attribute is not found, null is returned.</returns>
        public virtual string GetAttribute(string localName, string namespaceURI)
        {
            int index = FindAttribute(localName, namespaceURI);

            if (index >= 0) return ((XmlReader_XmlAttribute)_attributes[index]).Value;

            return null;
        }

        /// <summary>
        /// Gets the value of the attribute with the specified index.
        /// </summary>
        /// <param name="i">The index of the attribute. The index is zero-based.</param>
        /// <returns>The value of the specified attribute.</returns>
        public virtual string GetAttribute(int i)
        {
            // this will throw ArgumentOutOfRangeException for us if i is out of range
            return ((XmlReader_XmlAttribute)_attributes[i]).Value;
        }

        /// <summary>
        /// Moves to the attribute with the specified name.
        /// </summary>
        /// <param name="name">The qualified name of the attribute.</param>
        /// <returns>true if the attribute is found; otherwise, false.</returns>
        public virtual bool MoveToAttribute(string name)
        {
            int index = FindAttribute(name);

            if (index >= 0)
            {
                _currentAttribute = index;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves to the attribute with the specified local name and namespace URI.
        /// </summary>
        /// <param name="localName">The local name of the attribute.</param>
        /// <param name="namespaceURI">The namespace URI of the attribute.</param>
        /// <returns>true if the attribute is found; otherwise, false.</returns>
        public virtual bool MoveToAttribute(string localName, string namespaceURI)
        {
            int index = FindAttribute(localName, namespaceURI);

            if (index >= 0)
            {
                _currentAttribute = index;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves to the attribute with the specified index.
        /// </summary>
        /// <param name="i">The index of the attribute.</param>
        public virtual void MoveToAttribute(int i)
        {
            if (i < 0 || i >= _attributes.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            _currentAttribute = i;
        }

        /// <summary>
        /// Moves to the first attribute.
        /// </summary>
        /// <returns>true if an attribute exists (the reader moves to the first attribute); otherwise, false.</returns>
        public virtual bool MoveToFirstAttribute()
        {
            if (_attributes.Count > 0)
            {
                _currentAttribute = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves to the next attribute.
        /// </summary>
        /// <returns>true if there is a next attribute; false if there are no more attributes.</returns>
        public virtual bool MoveToNextAttribute()
        {
            // This will work even when we're currently not on an attribute, i.e. _current = -1
            if (_currentAttribute + 1 < _attributes.Count)
            {
                _currentAttribute++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves to the element that contains the current attribute node.
        /// </summary>
        /// <returns>true if the reader is positioned on an attribute; false if the reader is not positioned on an attribute.</returns>
        public virtual bool MoveToElement()
        {
            if (_currentAttribute >= 0)
            {
                _currentAttribute = -1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads the next node from the stream.
        /// </summary>
        /// <returns>true if the next node was read successfully; false if there are no more nodes to read.</returns>
        public virtual bool Read()
        {
            if (_readState == ReadState.Initial)
            {
                _readState = ReadState.Interactive;
            }

            if (_readState == ReadState.Interactive)
            {
                if (_currentAttribute >= 0)
                {
                    if (MoveToNextAttribute())
                    {
                        return true;
                    }

                    _currentAttribute = -1;
                }

                if (_length == 0)
                {
                    _offset = 0;
                    _length = _stream.Read(_buffer, 0, BufferSize);

                    if (_length == 0)
                    {
                        if (_isDone)
                        {
                            CleanUp(ReadState.EndOfFile);
                            return false;
                        }
                        else
                        {
                            _readState = ReadState.Error;
                            throw new XmlException(XmlException.XmlExceptionErrorCode.UnexpectedEOF);
                        }
                    }
                }

                while (true)
                {
                    int hresult = ReadInternal(0);

                    if (hresult == (int)XmlException.XmlExceptionErrorCodeInternal.ReturnToManagedCode)
                    {
                        return true;
                    }
                    else if (hresult == (int)XmlException.XmlExceptionErrorCodeInternal.NeedMoreData)
                    {
                        int bufferEnd = _offset + _length;

                        int bytesRead = _stream.Read(_buffer, bufferEnd, BufferSize - bufferEnd);

                        if (bytesRead == 0)
                        {
                            // complete the read (since it was expecting data)
                            ReadInternal(0);
                            
                            if (_isDone)
                            {
                                CleanUp(ReadState.EndOfFile);
                                return false;
                            }
                            else
                            {
                                _readState = ReadState.Error;
                                throw new XmlException(XmlException.XmlExceptionErrorCode.UnexpectedEOF);
                            }
                        }

                        _length += bytesRead;
                    }
                    else
                    {
                        _readState = ReadState.Error;
                        throw new XmlException((XmlException.XmlExceptionErrorCode)hresult);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether the reader is positioned at the end of the stream.
        /// </summary>
        public virtual bool EOF
        {
            get
            {
                return _readState == ReadState.EndOfFile;
            }
        }

        /// <summary>
        /// Closes the stream, changes the ReadState to Closed, and sets all the properties back to zero/empty string.
        /// </summary>
        public virtual void Close()
        {
            CleanUp(ReadState.Closed);
            _stream.Close();
        }

        private void CleanUp(ReadState readState)
        {
            _xmlSpaces.Clear();
            _xmlLangs.Clear();
            _xmlNodes.Clear();
            _attributes.Clear();
            _namespaces.Clear();
            _value = String.Empty;
            _isEmptyElement = false;
            _nodeType = XmlNodeType.None;
            _currentAttribute = -1;
            _readState = readState;

            // Place dummy values in these so their related properties won't throw exceptions
            _xmlSpaces.Push((int)XmlSpace.None);
            _xmlLangs.Push(String.Empty);
            _xmlNodes.Push(new XmlReader_XmlNode());
        }

        /// <summary>
        /// Gets the state of the reader.
        /// </summary>
        public virtual ReadState ReadState
        {
            get
            {
                return _readState;
            }
        }

        /// <summary>
        /// Skips the children of the current node.
        /// </summary>
        public virtual void Skip()
        {
            if (_readState != ReadState.Interactive)
                return;

            if (_currentAttribute >= 0)
            {
                _currentAttribute = -1;
            }

            if (_nodeType == XmlNodeType.Element && !_isEmptyElement)
            {
                // note that this "depth" is actually this.Depth - 1, but since we're only doing
                // relative comparison, there's no need to do the extra + 1
                int depth = _xmlNodes.Count;

                while (Read() && depth < _xmlNodes.Count)
                {
                    // Nothing, just read on
                }

                // consume end tag
                if (_nodeType == XmlNodeType.EndElement)
                    Read();
            }
            else
            {
                Read();
            }
        }

        /// <summary>
        /// Gets the XmlNameTable associated with the XmlReader.
        /// </summary>
        public virtual XmlNameTable NameTable
        {
            get
            {
                return _nameTable;
            }
        }

        /// <summary>
        /// Resolves a namespace prefix in the current element's scope.
        /// </summary>
        /// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass an empty string.</param>
        /// <returns>The namespace URI to which the prefix maps or null if no matching prefix is found.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual string LookupNamespace(string prefix);

        private const uint IsTextualNodeBitmap = 0x0cc; // 000 1100 1100
        // 0 None,
        // 0 Element,
        // 0 Attribute,
        // 1 Text,
        // 1 CDATA,
        // 0 ProcessingInstruction,
        // 0 Comment,
        // 1 Whitespace,
        // 1 SignificantWhitespace,
        // 0 EndElement,
        // 0 XmlDeclaration

        /// <summary>
        /// Reads the contents of an element or a text node as a string.
        /// </summary>
        /// <returns>The contents of the element or text node.</returns>
        public virtual string ReadString()
        {
            if (_readState != ReadState.Interactive)
            {
                return "";
            }

            // Move to Element
            if (_currentAttribute >= 0)
            {
                _currentAttribute = -1;
            }

            if (_nodeType == XmlNodeType.Element)
            {
                if (_isEmptyElement)
                {
                    return "";
                }
                else if (!Read())
                {
                    throw new InvalidOperationException();
                }

                if (_nodeType == XmlNodeType.EndElement)
                {
                    return "";
                }
            }

            string result = "";

            while ((IsTextualNodeBitmap & (1 << (int)_nodeType)) != 0)
            {
                result += _value;
                if (!Read())
                {
                    throw new InvalidOperationException();
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether the current node is a content (non-white space text, CDATA,
        /// Element, or EndElement) node. If the node is not a content node, the reader
        /// skips ahead to the next content node or end of file. It skips over nodes of
        /// the following type: ProcessingInstruction, Comment, Whitespace, or
        /// SignificantWhitespace. If the current node is an attribute node, this method
        /// moves the reader back to the element that owns the attribute.
        /// </summary>
        /// <returns>The NodeType of the current node found by the method or XmlNodeType.
        /// None if the reader has reached the end of the input stream.</returns>
        public virtual XmlNodeType MoveToContent()
        {
            // If we're at an attribute, move to element
            if (_currentAttribute >= 0)
            {
                _currentAttribute = -1;
                return _nodeType;
            }

            do
            {
                switch (_nodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.Text:
                    case XmlNodeType.EndElement:
                    case XmlNodeType.CDATA:
                        return _nodeType;
                }

            } while (Read());

            // Read return false, means we're at the EOF
            return XmlNodeType.None;
        }

        /// <summary>
        /// Checks that the current node is an element and advances the reader to the next node.
        /// </summary>
        public virtual void ReadStartElement()
        {
            if (MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
            }

            Read();
        }

        /// <summary>
        /// Checks that the current content node is an element with the given Name and
        /// advances the reader to the next node.
        /// </summary>
        /// <param name="name">The qualified name of the element.</param>
        public virtual void ReadStartElement(string name)
        {
            if (MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
            }

            if (StringRefEquals(((XmlReader_XmlNode)_xmlNodes.Peek()).Name, name))
            {
                Read();
            }
            else
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.ElementNotFound);
            }
        }

        /// <summary>
        /// Checks that the current content node is an element with the given LocalName and
        /// NamespaceURI and advances the reader to the next node.
        /// </summary>
        /// <param name="localname">The local name of the element.</param>
        /// <param name="ns">The namespace URI of the element.</param>
        public virtual void ReadStartElement(string localname, string ns)
        {
            if (MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
            }

            XmlReader_XmlNode node = (XmlReader_XmlNode)_xmlNodes.Peek();

            if (StringRefEquals(node.LocalName, localname) &&
                StringRefEquals(node.NamespaceURI, ns))
            {
                Read();
            }
            else
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.ElementNotFound);
            }
        }

        /// <summary>
        /// Reads a text-only element.
        /// </summary>
        /// <returns>The text contained in the element that was read. An empty string
        /// if the element is empty (&lt;item&gt;&lt;/item&gt; or &lt;item/&gt;).</returns>
        public virtual string ReadElementString()
        {
            string result = "";

            if (MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
            }

            if (!_isEmptyElement)
            {
                result = ReadString();
                if (_nodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
                }
            }

            Read(); // read the EndElement or the empty StartElement

            return result;
        }

        /// <summary>
        /// Checks that the Name property of the element found matches the given string
        /// before reading a text-only element.
        /// </summary>
        /// <param name="name">The qualified name of the element.</param>
        /// <returns>The text contained in the element that was read. An empty string
        /// if the element is empty (&lt;item&gt;&lt;/item&gt; or &lt;item/&gt;).</returns>
        public virtual string ReadElementString(string name)
        {
            string result = "";

            if (MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
            }

            if (!StringRefEquals(((XmlReader_XmlNode)_xmlNodes.Peek()).Name, name))
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.ElementNotFound);
            }

            if (!_isEmptyElement)
            {
                result = ReadString();
                if (_nodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
                }
            }

            Read(); // read the EndElement or the empty StartElement

            return result;
        }

        /// <summary>
        /// Checks that the LocalName and NamespaceURI properties of the element found
        /// matches the given strings before reading a text-only element.
        /// </summary>
        /// <param name="localname">The local name of the element.</param>
        /// <param name="ns">The namespace URI of the element.</param>
        /// <returns>The text contained in the element that was read. An empty string
        /// if the element is empty (&lt;item&gt;&lt;/item&gt; or &lt;item/&gt;).</returns>
        public virtual string ReadElementString(string localname, string ns)
        {
            string result = "";

            if (MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
            }

            XmlReader_XmlNode node = (XmlReader_XmlNode)_xmlNodes.Peek();

            if (!StringRefEquals(node.LocalName, localname) ||
                !StringRefEquals(node.NamespaceURI, ns))
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.ElementNotFound);
            }

            if (!_isEmptyElement)
            {
                result = ReadString();
                if (_nodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
                }
            }

            Read(); // read the EndElement or the empty StartElement

            return result;
        }

        /// <summary>
        /// Checks that the current content node is an end tag and advances the reader
        /// to the next node.
        /// </summary>
        public virtual void ReadEndElement()
        {
            if (MoveToContent() != XmlNodeType.EndElement)
            {
                throw new XmlException(XmlException.XmlExceptionErrorCode.InvalidNodeType);
            }

            Read();
        }

        /// <summary>
        /// Calls MoveToContent and tests if the current content node is a start tag or
        /// empty element tag.
        /// </summary>
        /// <returns>true if MoveToContent finds a start tag or empty element tag;
        /// false if a node type other than XmlNodeType.Element was found.</returns>
        public virtual bool IsStartElement()
        {
            return MoveToContent() == XmlNodeType.Element;
        }

        /// <summary>
        /// Calls MoveToContent and tests if the current content node is a start tag or
        /// empty element tag and if the Name property of the element found matches the
        /// given argument.
        /// </summary>
        /// <param name="name">The qualified name of the element.</param>
        /// <returns>true if the resulting node is an element and the Name property
        /// matches the specified string. false if a node type other than
        /// XmlNodeType.Element was found or if the element Name property does not
        /// match the specified string.</returns>
        public virtual bool IsStartElement(string name)
        {
            return (MoveToContent() == XmlNodeType.Element) &&
                StringRefEquals(((XmlReader_XmlNode)_xmlNodes.Peek()).Name, name);
        }

        /// <summary>
        /// Calls MoveToContent and tests if the current content node is a start tag or
        /// empty element tag and if the LocalName and NamespaceURI properties of the
        /// element found match the given strings.
        /// </summary>
        /// <param name="localname">The local name of the element.</param>
        /// <param name="ns">The namespace URI of the element.</param>
        /// <returns>true if the resulting node is an element. false if a node type other
        /// than XmlNodeType.Element was found or if the LocalName and NamespaceURI
        /// properties of the element do not match the specified strings.</returns>
        public virtual bool IsStartElement(string localname, string ns)
        {
            if (MoveToContent() == XmlNodeType.Element)
            {
                XmlReader_XmlNode node = (XmlReader_XmlNode)_xmlNodes.Peek();

                return StringRefEquals(node.LocalName, localname) &&
                    StringRefEquals(node.NamespaceURI, ns);
            }

            return false;
        }

        /// <summary>
        /// Reads until an element with the specified qualified name is found.
        /// </summary>
        /// <param name="name">The qualified name of the element.</param>
        /// <returns>true if a matching element is found; otherwise false and the
        /// XmlReader is in an end of file state.</returns>
        public virtual bool ReadToFollowing(string name)
        {
            if (name == null || name.Length == 0)
            {
                throw new ArgumentException();
            }

            // atomize name
            name = _nameTable.Add(name);

            // Move back to element, so we don't have to deal with _nodeType != NodeType
            if (_currentAttribute >= 0)
            {
                _currentAttribute = -1;
            }

            // find following element with that name
            while (Read())
            {
                if (_nodeType == XmlNodeType.Element &&
                    Object.ReferenceEquals(((XmlReader_XmlNode)_xmlNodes.Peek()).Name, name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads until an element with the specified local name and namespace URI
        /// is found.
        /// </summary>
        /// <param name="localname">The local name of the element.</param>
        /// <param name="namespaceURI">The namespace URI of the element.</param>
        /// <returns>true if a matching element is found; otherwise false and the
        /// XmlReader is in an end of file state.</returns>
        public virtual bool ReadToFollowing(string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0 || namespaceURI == null)
            {
                throw new ArgumentException();
            }

            // atomize local name and namespace
            localName = NameTable.Add(localName);
            namespaceURI = NameTable.Add(namespaceURI);

            // Move back to element, so we don't have to deal with _nodeType != NodeType
            if (_currentAttribute >= 0)
            {
                _currentAttribute = -1;
            }

            XmlReader_XmlNode node;

            // find following element with that name
            while (Read())
            {
                if (_nodeType == XmlNodeType.Element)
                {
                    node = (XmlReader_XmlNode)_xmlNodes.Peek();

                    if (Object.ReferenceEquals(node.LocalName, localName) &&
                        Object.ReferenceEquals(node.NamespaceURI, namespaceURI))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Advances the XmlReader to the next descendant element with the specified
        /// qualified name.
        /// </summary>
        /// <param name="name">The qualified name of the element.</param>
        /// <returns>true if a matching descendant element is found; otherwise false.
        /// If a matching child element is not found, the XmlReader is positioned on
        /// the end tag (NodeType is XmlNodeType.EndElement) of the element.
        /// If the XmlReader is not positioned on an element when ReadToDescendant
        /// was called, this method returns false and the position of the XmlReader
        /// is not changed.</returns>
        public virtual bool ReadToDescendant(string name)
        {
            if (name == null || name.Length == 0)
            {
                throw new ArgumentException();
            }

            // Check for Attribute node now so we don't have to worry about it anymore
            if (_currentAttribute >= 0)
            {
                return false;
            }

            // save the element or root depth (note that this "depth" is actually this.Depth - 1,
            // but since we're only doing relative comparison, there's no need to do the extra + 1)
            int parentDepth = _xmlNodes.Count;

            if (_nodeType != XmlNodeType.Element)
            {
                // adjust the depth if we are on root node
                if (_readState == ReadState.Initial)
                {
                    parentDepth--;
                }
                else
                {
                    return false;
                }
            }
            else if (_isEmptyElement)
            {
                return false;
            }

            // atomize name
            name = _nameTable.Add(name);

            // find the descendant
            while (Read() && _xmlNodes.Count > parentDepth)
            {
                if (_nodeType == XmlNodeType.Element &&
                    Object.ReferenceEquals(name, ((XmlReader_XmlNode)_xmlNodes.Peek()).Name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Advances the XmlReader to the next descendant element with the specified
        /// local name and namespace URI.
        /// </summary>
        /// <param name="localname">The local name of the element.</param>
        /// <param name="namespaceURI">The namespace URI of the element.</param>
        /// <returns>true if a matching descendant element is found; otherwise false.
        /// If a matching child element is not found, the XmlReader is positioned on
        /// the end tag (NodeType is XmlNodeType.EndElement) of the element.
        /// If the XmlReader is not positioned on an element when ReadToDescendant
        /// was called, this method returns false and the position of the XmlReader
        /// is not changed.</returns>
        public virtual bool ReadToDescendant(string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0 || namespaceURI == null)
            {
                throw new ArgumentException();
            }

            // Check for Attribute node now so we don't have to worry about it anymore
            if (_currentAttribute >= 0)
            {
                return false;
            }

            // save the element or root depth (note that this "depth" is actually this.Depth - 1,
            // but since we're only doing relative comparison, there's no need to do the extra + 1)
            int parentDepth = _xmlNodes.Count;

            if (_nodeType != XmlNodeType.Element)
            {
                // adjust the depth if we are on root node
                if (_readState == ReadState.Initial)
                {
                    parentDepth--;
                }
                else
                {
                    return false;
                }
            }
            else if (_isEmptyElement)
            {
                return false;
            }

            // atomize local name and namespace
            localName = _nameTable.Add(localName);
            namespaceURI = _nameTable.Add(namespaceURI);

            XmlReader_XmlNode node;

            // find the descendant
            while (Read() && _xmlNodes.Count > parentDepth)
            {
                if (_nodeType == XmlNodeType.Element)
                {
                    node = (XmlReader_XmlNode)_xmlNodes.Peek();

                    if (Object.ReferenceEquals(localName, node.LocalName) &&
                        Object.ReferenceEquals(namespaceURI, node.NamespaceURI))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Advances the XmlReader to the next sibling element with the specified
        /// qualified name.
        /// </summary>
        /// <param name="name">The qualified name of the element.</param>
        /// <returns>true if a matching sibling element is found; otherwise false.
        /// If a matching sibling element is not found, the XmlReader is positioned
        /// on the end tag (NodeType is XmlNodeType.EndElement) of the parent
        /// element.</returns>
        public virtual bool ReadToNextSibling(string name)
        {
            if (name == null || name.Length == 0)
            {
                throw new ArgumentException();
            }

            // atomize name
            name = NameTable.Add(name);

            // find the next sibling
            do
            {
                Skip(); // Skip the subtree, note that skip will call MoveToElement(), making _nodeType == NodeType
                if (_nodeType == XmlNodeType.Element &&
                    Object.ReferenceEquals(name, ((XmlReader_XmlNode)_xmlNodes.Peek()).Name))
                {
                    return true;
                }

            } while ((_nodeType != XmlNodeType.EndElement) && (_readState != ReadState.EndOfFile));

            return false;
        }

        /// <summary>
        /// Advances the XmlReader to the next sibling element with the specified
        /// local name and namespace URI.
        /// </summary>
        /// <param name="localname">The local name of the element.</param>
        /// <param name="namespaceURI">The namespace URI of the element.</param>
        /// <returns>true if a matching sibling element is found; otherwise false.
        /// If a matching sibling element is not found, the XmlReader is positioned
        /// on the end tag (NodeType is XmlNodeType.EndElement) of the parent
        /// element.</returns>
        public virtual bool ReadToNextSibling(string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0 || namespaceURI == null)
            {
                throw new ArgumentException();
            }

            // atomize local name and namespace
            localName = _nameTable.Add(localName);
            namespaceURI = _nameTable.Add(namespaceURI);

            // find the next sibling
            XmlReader_XmlNode node;
            do
            {
                Skip(); // Skip the subtree, note that skip will call MoveToElement(), making _nodeType == NodeType
                if (_nodeType == XmlNodeType.Element)
                {
                    node = (XmlReader_XmlNode)_xmlNodes.Peek();
                    if (Object.ReferenceEquals(LocalName, node.LocalName) &&
                        Object.ReferenceEquals(namespaceURI, node.NamespaceURI))
                    {
                        return true;
                    }
                }

            } while ((_nodeType != XmlNodeType.EndElement) && (_readState != ReadState.EndOfFile));

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether the current node has any attributes.
        /// </summary>
        public virtual bool HasAttributes
        {
            get
            {
                return _attributes.Count > 0;
            }
        }

        //
        // IDisposable interface
        //
        public void Dispose()
        {
            if (ReadState != ReadState.Closed)
            {
                Close();
            }
        }

        private XmlReader() { }

        /// <summary>
        /// Creates a new XmlReader instance using the specified stream.
        /// </summary>
        /// <param name="input">The stream containing the XML data.</param>
        /// <returns>An XmlReader object used to read the data contained in the stream.</returns>
        public static XmlReader Create(Stream input)
        {
            return Create(input, null);
        }

        /// <summary>
        /// Creates a new XmlReader instance with the specified stream and
        /// XmlReaderSettings object.
        /// </summary>
        /// <param name="input">The stream containing the XML data.</param>
        /// <param name="settings">The XmlReaderSettings object used to configure the new
        /// XmlReader instance. This value can be null.</param>
        /// <returns>An XmlReader object to read the XML data.</returns>
        public static XmlReader Create(Stream input, XmlReaderSettings settings)
        {
            if (input == null) throw new ArgumentNullException();
            if (settings == null) settings = new XmlReaderSettings();

            XmlReader reader = new XmlReader();

            reader._xmlSpaces = new Stack();
            reader._xmlLangs = new Stack();
            reader._xmlNodes = new Stack();
            reader._attributes = new ArrayList();
            reader._namespaces = new Stack();
            reader._value = String.Empty;
            reader._nodeType = XmlNodeType.None;
            reader._isEmptyElement = false;
            reader._isDone = false;

            reader._buffer = new byte[BufferSize];
            reader._offset = 0;
            reader._length = 0;

            reader._stream = input;
            reader._readState = ReadState.Initial;

            reader._nameTable = (settings.NameTable == null) ? new XmlNameTable() : settings.NameTable;

            reader._xmlNodes.Push(new XmlReader_XmlNode());
            reader._xmlSpaces.Push((int)XmlSpace.None);
            reader._xmlLangs.Push(String.Empty);

            XmlReader_NamespaceEntry nsEntry;

            nsEntry = new XmlReader_NamespaceEntry();
            nsEntry.Prefix = String.Empty;
            nsEntry.NamespaceURI = String.Empty;
            reader._namespaces.Push(nsEntry);

            nsEntry = new XmlReader_NamespaceEntry();
            nsEntry.Prefix = reader._nameTable.Add(Xml);
            nsEntry.NamespaceURI = reader._nameTable.Add(XmlNamespace);
            reader._namespaces.Push(nsEntry);

            nsEntry = new XmlReader_NamespaceEntry();
            nsEntry.Prefix = reader._nameTable.Add(Xmlns);
            nsEntry.NamespaceURI = reader._nameTable.Add(XmlnsNamespace);
            reader._namespaces.Push(nsEntry);

            reader._currentAttribute = -1;

            reader.Initialize(settings.GetSettings());

            return reader;
        }
    }
}


