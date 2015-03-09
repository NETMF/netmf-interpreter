using System;
using System.Collections;
using System.Text;
using System.Xml;
using Microsoft.SPOT;
using Ws.Services.Faults;
using System.Ext;
using Ws.Services.Xml;

namespace Ws.Services.WsaAddressing
{

    /// <summary>
    /// Class used to store and access Ws-Addressing endpoint references.
    /// </summary>
    public class WsWsaEndpointRef
    {
        /// <summary>
        /// Use to Set the address property of a WsaEndpointRef.
        /// </summary>
        public readonly Uri Address;

        /// <summary>
        /// Use to Get or Set a property containing a collection of WsaRefProperties.
        /// </summary>
        public readonly WsXmlNodeList RefProperties;

        /// <summary>
        /// Use to get or Set a property containing a collection of WsaRefParameters.
        /// </summary>
        public readonly WsXmlNodeList RefParameters;

        /// <summary>
        /// Creates an instance of a WsaEndpointRef cless.
        /// </summary>
        public WsWsaEndpointRef(Uri address)
        {
            Address = address;
            RefProperties = new WsXmlNodeList();
            RefParameters = new WsXmlNodeList();
        }

        public WsWsaEndpointRef(XmlReader reader, string addressing)
        {
            reader.ReadStartElement();

            Address = new Uri(reader.ReadElementString("Address", addressing));

            if (reader.IsStartElement("ReferenceProperties", addressing))
            {
                reader.Read();
#if DEBUG
                int depth = reader.Depth;
#endif
                RefProperties = new WsXmlNodeList(reader);
#if DEBUG
                Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif
                reader.ReadEndElement();
            }

            if (reader.IsStartElement("ReferenceParameters", addressing))
            {
                reader.Read();
#if DEBUG
                int depth = reader.Depth;
#endif
                RefParameters = new WsXmlNodeList(reader);
#if DEBUG
                Debug.Assert(XmlReaderHelper.HasReadCompleteNode(depth, reader));
#endif
                reader.ReadEndElement();
            }

            XmlReaderHelper.SkipAllSiblings(reader); // PortType, ServiceName, xs:any

            reader.ReadEndElement();
        }

    }

    /// <summary>
    /// Collection of WsaEndpointRef objects.
    /// </summary>
    public class WsWsaEndpointRefs
    {
        // Fields
        private object    m_threadLock;
        private ArrayList m_EndpointReferences;

        /// <summary>
        /// Creates an instance of a WsaEndpointRef collection.
        /// </summary>
        /// <remarks>This collection is thread safe.</remarks>
        public WsWsaEndpointRefs()
        {
            m_threadLock         = new object();
            m_EndpointReferences = new ArrayList();            
        }

        /// <summary>
        /// Gets the number of WsaEndpointRef objects contained in collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_EndpointReferences.Count;
            }
        }

        /// <summary>
        /// Gets or sets the WsaEndpointRef object at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the WsaEndpointRef object to get or set.</param>
        /// <returns>The WsaEndpointRef object at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index is less than zero.-or- index is equal to or greater than collection count.
        /// </exception>
        public WsWsaEndpointRef this[int index]
        {
            get
            {
                return (WsWsaEndpointRef)m_EndpointReferences[index];
            }
        }

        public WsWsaEndpointRef this[Uri address]
        {
            get
            {
                lock (m_threadLock)
                {
                    int count = m_EndpointReferences.Count;
                    WsWsaEndpointRef endpointRef;
                    for (int i = 0; i < count; i++)
                    {
                        endpointRef = (WsWsaEndpointRef)m_EndpointReferences[i];
                        if (endpointRef.Address == address)
                        {
                            return endpointRef;
                        }
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Adds a WsaEndpointRef object to the end of the collection.
        /// </summary>
        /// <param name="value">The WsaEndpointRef object to be added to the end of the collection.
        /// The value can be null.
        /// </param>
        /// <returns>The index at which the WsaEndpointRef object has been added.</returns>
        public int Add(WsWsaEndpointRef value)
        {
            lock (m_threadLock)
            {
                return m_EndpointReferences.Add(value);
            }
        }

        /// <summary>
        /// Removes all WsaEndpointRef objects from the collection.
        /// </summary>
        public void Clear()
        {
            lock (m_threadLock)
            {
                m_EndpointReferences.Clear();
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific WsaEndpointRef object from the colleciton.
        /// </summary>
        /// <param name="endpointReference">
        /// The WsaEndpointRef object to remove from the collection. The value can be null.</param>
        public void Remove(WsWsaEndpointRef endpointReference)
        {
            lock (m_threadLock)
            {
                m_EndpointReferences.Remove(endpointReference);
            }
        }
    }
}


