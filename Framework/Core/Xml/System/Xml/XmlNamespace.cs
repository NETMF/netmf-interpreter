using System;
using System.Collections;
using System.Xml;
using System.Text;
using Microsoft.SPOT;

namespace System.Xml
{

    // Summary:
    //     Basic namespace object.
    internal class XmlNamespace
    {
        // Fields
        public readonly string Prefix;
        public readonly string NamespaceURI;

        public XmlNamespace(string prefix, string namespaceURI)
        {
            Prefix = prefix;
            NamespaceURI = namespaceURI;
        }
    }

    internal class XmlNamespaces
    {
        // Fields
        private ArrayList m_namespaceList;

        public XmlNamespaces()
        {
            m_namespaceList = new ArrayList();
        }

        public XmlNamespace this[String prefix]
        {
            get
            {
                XmlNamespace ns;
                int count = m_namespaceList.Count;
                for (int i = 0; i < count; i++)
                {
                    ns = (XmlNamespace)m_namespaceList[i];
                    if (ns.Prefix == prefix)
                    {
                        return ns;
                    }
                }

                return null;
            }
        }

        //
        // Summary:
        //     Adds a namespace to the end of the XmlNamespace list. If the NamespcaeURI is already in the list
        //     it will not be added.
        //
        // Parameters:
        //   prefix:
        //     The prefix of the XmlNamespace.
        //
        //   NamespaceURI:
        //     The NamespaceURI of the XmlNamespace
        //
        // Returns:
        //     The XmlNamespace list index at which the value has been added.
        //
        public int Add(string prefix, string namespaceURI)
        {
            if (NewNamespaceExists(prefix, namespaceURI))
                return -1;
            return m_namespaceList.Add(new XmlNamespace(prefix, namespaceURI));
        }

        //
        // Summary:
        //     Determines whether a duplicate namespace already exists
        //     in the XmlNamespace list.
        //
        // Parameters:
        //   prefix:
        //     The new namspace prefix.
        //
        //   namespaceURI:
        //     The new namespaceURI.
        //
        // Returns:
        //     true if a matching namespace is found in the XmlNamespace list; otherwise, false.
        //
        // Exceptions:
        //     ArgumentNullException:
        //         If the prefix or namespaceURI is null.
        //
        //     XMLException:
        //         If the prefix is alread in the list.
        private bool NewNamespaceExists(string prefix, string namespaceURI)
        {
            Debug.Assert(prefix != null && namespaceURI != null);

            XmlNamespace ns;
            int count = m_namespaceList.Count;
            for (int i = 0; i < count; i++)
            {
                ns = (XmlNamespace)m_namespaceList[i];
                if (ns.Prefix == prefix ||
                    ns.NamespaceURI == namespaceURI)
                {
                    return true;
                }
            }

            return false;
        }
    }
}


