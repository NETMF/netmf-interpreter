using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Ext.Xml;

namespace Ws.Services.Xml
{

    /// <summary>
    /// Class used to store an elements attibute details.
    /// </summary>
    public class WsXmlAttribute
    {
        /// <summary>
        /// Creates an instance of a XmlAttribute class.
        /// </summary>
        public WsXmlAttribute()
        {
        }

        /// <summary>
        /// Property contains the local name of an XmlAttribute.
        /// </summary>
        public string LocalName = null;

        /// <summary>
        /// Property contains the XmlAttributes namespace uri.
        /// </summary>
        public string NamespaceURI = null;

        /// <summary>
        /// Property contains the value of an XmlAttribute.
        /// </summary>
        public string Value = null;

        /// <summary>
        /// Property contains the namespace prefix for an XmlAttribute.
        /// </summary>
        public string Prefix = null;
    }

    /// <summary>
    /// Class used to store a collection of XmlAttribute objects.
    /// </summary>
    /// <remarks>
    /// Each XmlNode object contains an instance of this class. It is used to store Xml attributes
    /// associated with a node. This collecitn is thread safe.
    /// </remarks>
    public class WsXmlAttributeCollection
    {
        // Fields
        private ArrayList m_attribList;

        /// <summary>
        /// Creates an instance of an XmlAttributes collection.
        /// </summary>
        public WsXmlAttributeCollection()
        {
            m_attribList = new ArrayList();
        }

        /// <summary>
        /// Gets the number of elements actually contained in the XmlAttributes collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_attribList.Count;
            }
        }

        /// <summary>
        /// Gets the XmlAttribute at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the XmlAttribute to get or set.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Index is less than zero.-or- index is equal to or greater than XmlAttributes count.
        /// </exception>
        public WsXmlAttribute this[int i]
        {
            get
            {
                return (WsXmlAttribute)m_attribList[i];
            }
        }

        public WsXmlAttribute this[String localName, String namespaceURI]
        {
            get
            {
                int count = m_attribList.Count;
                WsXmlAttribute cur;
                for (int i = 0; i < count; i++)
                {
                    cur = (WsXmlAttribute)m_attribList[i];
                    if (cur.LocalName == localName && cur.NamespaceURI == namespaceURI)
                    {
                        return cur;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Adds an XmlAttribute to the end of the XmlAttributes collection.
        /// </summary>
        /// <param name="value">
        /// The XmlAttribute to be added to the end of the XmlAttributes collection. The value can be null.
        /// </param>
        /// <returns>The XmlAttribute index at which the value has been added.</returns>
        public WsXmlAttribute Append(WsXmlAttribute node)
        {
            m_attribList.Add(node);
            return node;
        }

        public void RemoveAll()
        {
            m_attribList.Clear();
        }
    }

    /// <summary>
    /// Class used to store a collection of XmlNode objects.
    /// </summary>
    /// <remarks>The XmlDocument uses this collection.</remarks>
    public class WsXmlNodeList
    {
        // Fields
        private ArrayList m_nodeList;

        /// <summary>
        /// Creates and instance of an XmlNodes collection.
        /// </summary>
        public WsXmlNodeList()
        {
            m_nodeList = new ArrayList();
        }

        /// <summary>
        /// Gets the number of XmlNodes actually contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_nodeList.Count;
            }
        }

        /// <summary>
        /// Gets or sets the XmlNode at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the XmlNode to get or set.</param>
        /// <returns>The XmlNode at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Index is less than zero.-or- index is equal to or greater than collection count.
        /// </exception>
        public WsXmlNode this[int i]
        {
            get
            {
                return (WsXmlNode)m_nodeList[i];
            }
        }

        /// <summary>
        /// Adds an XmlNode to the end of the collection.
        /// </summary>
        /// <param name="value">The XmlNode to be added to the end of the collection. The value can be null.</param>
        /// <returns>The XmlNodes index at which the value has been added.</returns>
        public int Add(WsXmlNode value)
        {
            return m_nodeList.Add(value);
        }

        internal WsXmlNode GetNode(String localName, String namespaceUri)
        {
            int count = m_nodeList.Count;
            WsXmlNode cur;
            for (int i = 0; i < count; i++)
            {
                cur = (WsXmlNode)m_nodeList[i];
                if (cur.LocalName == localName && cur.NamespaceURI == namespaceUri)
                {
                    return cur;
                }
            }

            return null;
        }

        internal String GetNodeValue(String localName, String namespaceUri)
        {
            WsXmlNode node = GetNode(localName, namespaceUri);

            if (node != null)
            {
                return node.Value;
            }

            return null;
        }

        internal WsXmlNodeList GetNodes(String localName, String namespaceUri)
        {
            WsXmlNodeList list = new WsXmlNodeList();
            int count = m_nodeList.Count;
            WsXmlNode cur;
            for (int i = 0; i < count; i++)
            {
                cur = (WsXmlNode)m_nodeList[i];
                if (cur.LocalName == localName && cur.NamespaceURI == namespaceUri)
                {
                    list.Add(cur);
                }
            }

            return list;
        }

        internal WsXmlNodeList(XmlReader reader)
        {
            m_nodeList = new ArrayList();

            reader.MoveToContent();

            int targetDepth = reader.Depth - 1;

            do
            {
                m_nodeList.Add(new WsXmlNode(reader));
                reader.MoveToContent();
            }

            while (reader.Depth > targetDepth);
        }

        internal void WriteTo(XmlWriter writer)
        {
            int count = m_nodeList.Count;
            for (int i = 0; i < count; i++)
            {
                ((WsXmlNode)m_nodeList[i]).WriteTo(writer);
            }
        }
    }

    /// <summary>
    /// Class used to store XmlNode properties and child collections.
    /// </summary>
    public class WsXmlNode
    {
        /// <summary>
        /// Creates an instance of an XmlNode.
        /// </summary>
        public WsXmlNode()
        {
            this.Attributes = new WsXmlAttributeCollection();
            this.ChildNodes = new WsXmlNodeList();
        }

        public WsXmlNode(String prefix, String localName, String namespaceUri, String value)
            : this()
        {
            this.Prefix = prefix;
            this.LocalName = localName;
            this.NamespaceURI = namespaceUri;
            this.Value = value;
        }

        internal WsXmlNode(XmlReader reader)
            : this()
        {
            if (reader.IsStartElement() == false)
            {
                throw new XmlException();
            }

            WsXmlNode currentElement = this;
            WsXmlNode parentElement = currentElement;
            int previousDepth = reader.Depth;
            int originalDepth = previousDepth;

            PopulateNode(this, null, reader);
            reader.Read();

            // Walk the document
            while (true)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        // We are decending the branch
                        while (reader.Depth < previousDepth)
                        {
                            parentElement = parentElement.ParentNode;
                            --previousDepth;
                        }

                        // We are accending the branch
                        if (reader.Depth > previousDepth)
                        {
                            parentElement = currentElement;
                            ++previousDepth;
                        }

                        currentElement = new WsXmlNode();
                        parentElement.AppendChild(currentElement);

                        PopulateNode(currentElement, parentElement, reader);
                        break;
                    case XmlNodeType.Text:
                        currentElement.Value = reader.Value;
                        break;
                }

                if (reader.Depth <= originalDepth)
                    break;

                // Read more xml
                reader.Read();
            }

            if (reader.Depth == originalDepth && reader.NodeType == XmlNodeType.EndElement)
            {
                reader.Read();
            }
        }

        private void PopulateNode(WsXmlNode node, WsXmlNode parent, XmlReader reader)
        {
            node.LocalName = reader.LocalName;
            node.NamespaceURI = reader.NamespaceURI;
            node.Prefix = reader.Prefix;
            node.ParentNode = parent;

            // Read attributes if they exist
            if (reader.HasAttributes)
            {
                reader.MoveToFirstAttribute();
                do
                {
                    WsXmlAttribute attribute = new WsXmlAttribute();
                    attribute.Value = reader.Value;
                    attribute.LocalName = reader.LocalName;
                    if (reader.NamespaceURI != String.Empty)
                    {
                        attribute.NamespaceURI = reader.NamespaceURI;
                    }

                    attribute.Prefix = reader.Prefix;
                    node.Attributes.Append(attribute);
                }

                while (reader.MoveToNextAttribute());

                reader.MoveToElement();
            }
        }

        internal WsXmlNode ParentNode;

        /// <summary>
        /// Property contains an XmlNodes namespace uri.
        /// </summary>
        public string NamespaceURI;

        /// <summary>
        /// Property contains an XmlNodes namespace prefix.
        /// </summary>
        public string Prefix;

        /// <summary>
        /// Property contains an XmlNodes local name.
        /// </summary>
        public string LocalName;

        /// <summary>
        /// Property contains an XmlNodes value.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                if (ChildNodes.Count > 0)
                {
                    throw new XmlException();
                }

                _value = value;
            }
        }

        private String _value;
        /// <summary>
        /// Property contains a collection of XmlNode attributes.
        /// </summary>
        public readonly WsXmlAttributeCollection Attributes;

        /// <summary>
        /// Property contains a collection of child XmlNodes.
        /// </summary>
        public readonly WsXmlNodeList ChildNodes;

        /// <summary>
        /// Use to append an XmlNode to the ChildNodes collection.
        /// </summary>
        /// <param name="newNode"></param>
        /// <returns></returns>
        public WsXmlNode AppendChild(WsXmlNode newNode)
        {
            this.ChildNodes.Add(newNode);
            return newNode;
        }

        internal void WriteTo(XmlWriter w)
        {
            w.WriteStartElement(Prefix, LocalName, NamespaceURI);

            if (this.Attributes.Count > 0)
            {
                int count = this.Attributes.Count;
                for (int i = 0; i < count; i += 1)
                {
                    WsXmlAttribute attr = this.Attributes[i];
                    if (attr.Prefix != "xmlns" || attr.LocalName != Prefix) // so we don't redefine prefix
                    {
                        w.WriteAttributeString(attr.Prefix, attr.LocalName, attr.NamespaceURI, attr.Value);
                    }
                }
            }

            int childNodesCount = this.ChildNodes.Count;

            if (childNodesCount > 0)
            {
                for (int i = 0; i < childNodesCount; i++)
                {
                    this.ChildNodes[i].WriteTo(w);
                }
            }
            else if (this.Value != null)
            {
                w.WriteString(this.Value);
            }

            w.WriteEndElement();
        }
    }
}


