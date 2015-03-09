using System;
using System.Collections;
using System.Xml;
using System.Text;

namespace Ws.Services.Xml
{
    /// <summary>
    /// Class used to store xml namespace details.
    /// </summary>
    public class WsXmlNamespace
    {
        /// <summary>
        /// Creates an instance of a XmlNamespace class with a preset namespace prefix and namespace uri.
        /// </summary>
        /// <param name="prefix">A namespace prefix.</param>
        /// <param name="namespaceURI">A namespace URI.</param>
        public WsXmlNamespace(string prefix, string namespaceURI)
        {
            this.Prefix = prefix;
            this.NamespaceURI = namespaceURI;
        }

        /// <summary>
        /// Property conatins a namespace prefix.
        /// </summary>
        public readonly String Prefix;

        /// <summary>
        /// Property contains a namaspace uri.
        /// </summary>
        public readonly String NamespaceURI;

    }

    /// <summary>
    /// Class used to store a collection of xml namespace objects.
    /// </summary>
    /// <remarks>
    /// The stack uses this class to store base namespaces. Implementations use this class to store additional
    /// implementation specific namespaces. This collection is thread safe.
    /// </remarks>
    internal class WsXmlNamespaces
    {
        // Fields
        private ArrayList m_namespaceList;

        /// <summary>
        /// Creates an instance of a XmlNamespaces collection.
        /// </summary>
        public WsXmlNamespaces()
        {
            m_namespaceList = new ArrayList();
        }

        /// <summary>
        /// Gets the number of XmlNamspace objects actually contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_namespaceList.Count;
            }
        }

        /// <summary>
        /// Gets or sets the XmlNamespace object at the specified index. If the NamespcaeURI is already in the list
        /// it will not be added.
        /// </summary>
        /// <param name="index">The zero-based index of the XmmlNamespace object to get or set.</param>
        /// <returns>The XmlNamespace object at the specified index.</returns>
        public WsXmlNamespace this[int index]
        {
            get
            {
                return (WsXmlNamespace)m_namespaceList[index];
            }
        }

        /// <summary>
        /// Adds an XmlNamespace object to the end of the collection. If the NamespcaeURI is already in the list
        /// it will not be added.
        /// </summary>
        /// <param name="value">The XmlNamespace to be added to the end of the collection. The value can be null.</param>
        /// <returns>The index at which the XmlNamespace has been added.</returns>
        public int Add(WsXmlNamespace value)
        {
            if (Exists(value.Prefix, value.NamespaceURI))
                return -1;
            return m_namespaceList.Add(value);
        }

        /// <summary>
        /// Determines whether a duplicate namespace already exists in the collection.
        /// </summary>
        /// <param name="prefix">A namespace prefix.</param>
        /// <param name="namespaceURI">A namespace uri.</param>
        /// <returns>True if a matching namespace is found in the collection; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">If the prefix parameter is null.</exception>
        /// <exception cref="XmlException">If the prefix is already in the list.</exception>
        private bool Exists(string prefix, string namespaceURI)
        {
            WsXmlNamespace xmlNamespace;
            int count = m_namespaceList.Count;
            for (int i = 0; i < count; i++)
            {
                xmlNamespace = (WsXmlNamespace)m_namespaceList[i];
                if (xmlNamespace.Prefix == prefix || xmlNamespace.NamespaceURI == namespaceURI)
                {
                    return true;
                }
            }
            return false;
        }

        public string LookupPrefix(string namespaceUri)
        {
            WsXmlNamespace xmlNamespace;
            int count = m_namespaceList.Count;
            for (int i = 0; i < count; i++)
            {
                xmlNamespace = (WsXmlNamespace)m_namespaceList[i];
                if (xmlNamespace.NamespaceURI == namespaceUri)
                {
                    return xmlNamespace.Prefix;
                }
            }

            return null;
        }
    }
}
