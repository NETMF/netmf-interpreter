using System;
using System.Collections;
using System.Text;
using Dpws.Device.Services;
using Ws.Services.WsaAddressing;
using Ws.Services.Binding;

namespace Dpws.Device.Services
{
    // Enumeration representing the possible states of an Event sink.
    internal enum DpwsWseEventStatus
    {
        Expired = 1,
        Active = 2,
    }

    /// <summary>
    /// Use this class to store information that identifies a specific hosted service event source.
    /// </summary>
    /// <remarks>
    /// You must create an DpwsWseEventSource object for each event in a hosted service you want
    /// to expose to a DPWS client. The event source object must then be added to the hosted service
    /// event source collection to register it with the device services. When a client subscribes
    /// to an event the subscription manager uses this class to validate and create an event subscription
    /// for the event source. The properties of this class represent the event type in an event
    /// subscription request.
    /// </remarks>
    public class DpwsWseEventSource
    {
        private string m_name;
        private string m_prefix;
        private string m_namespaceURI;
        private string m_Operation;
        private DpwsWseEventSinks m_eventSinks;

        /// <summary>
        /// Creates an instance of the event source class.
        /// </summary>
        /// <param name="prefix">The event source namespace prefix.</param>
        /// <param name="namespaceURI">The event source namespace Uri.</param>
        /// <param name="name">The event source type name.</param>
        public DpwsWseEventSource(string prefix, string namespaceURI, string name)
        {
            m_name = name;
            m_prefix = prefix;
            m_namespaceURI = namespaceURI[namespaceURI.Length - 1] == '/' ? namespaceURI.Substring(0, namespaceURI.Length - 1) : namespaceURI;
            m_Operation = m_namespaceURI + "/" + m_name;
            m_eventSinks = new DpwsWseEventSinks();
        }

        /// <summary>
        /// Use to Get the name of the event source.
        /// </summary>
        public string Name { get { return m_name; } set { m_name = value; } }

        /// <summary>
        /// Use to Get the event source namespace prefix.
        /// </summary>
        public string Prefix { get { return m_prefix; } set { m_prefix = value; } }

        /// <summary>
        /// Use to Get the event source namespace Uri.
        /// </summary>
        public string NamespaceURI { get { return m_namespaceURI; } set { m_namespaceURI = value; } }

        /// <summary>
        /// Use to get the complete event source namespace qualified name.
        /// </summary>
        public string Operation { get { return m_Operation; } set { m_Operation = value; } }

        // A colletion used to store active event sinks. An event sink contains endpoint information
        // for each DPWS that has an active subscription to a specific event. The stacks subscription
        // manager uses this informaiton when dispatching events to all clients that have an active subscription
        // to this event source.
        internal DpwsWseEventSinks EventSinks { get { return m_eventSinks; } set { m_eventSinks = value; } }
    }

    /// <summary>
    /// A collection of a hosted services event sources.
    /// </summary>
    /// <remarks>
    /// A device developer uses this collection to manage event sources in their there hosted service.
    /// </remarks>
    public class DpwsWseEventSources
    {
        private object    m_threadLock;
        private ArrayList m_sourceList;

        /// <summary>
        /// Creates an instance of the event sources collection.
        /// </summary>
        public DpwsWseEventSources()
        {
            m_threadLock = new object();
            m_sourceList = new ArrayList();            
        }

        //
        // Summary:
        //     Gets the number of eventsource actually contained in the eventsource list.
        //
        // Returns:
        //     The number of event sources actually contained in eventsource list.
        /// <summary>
        /// Gets the number of event sources actually contained in the eventsource list.
        /// </summary>
        public int Count
        {
            get
            {
                return m_sourceList.Count;
            }
        }

        /// <summary>
        /// Gets or sets the event source at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the event source to get or set.
        /// </param>
        /// <returns>
        /// The event source at the specified index.
        /// </returns>
        public DpwsWseEventSource this[int index]
        {
            get
            {
                return (DpwsWseEventSource)m_sourceList[index];
            }
        }

        public DpwsWseEventSource this[string eventSourceName]
        {
            get
            {
                lock (m_threadLock)
                {
                    int count = m_sourceList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        DpwsWseEventSource sourceType = (DpwsWseEventSource)m_sourceList[i];
                        if (sourceType.Operation == eventSourceName || sourceType.Name == eventSourceName)
                        {
                            return sourceType;
                        }
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Adds an event source to the end of the event source collection.
        /// </summary>
        /// <param name="value">
        /// The event source object to be added to the end of the event sources collection. The value can be null.
        /// </param>
        /// <returns>
        /// The event source index at which the value has been added.
        /// </returns>
        public int Add(DpwsWseEventSource value)
        {
            lock (m_threadLock)
            {
                return m_sourceList.Add(value);
            }
        }
    }

    internal class DpwsWseEventSink
    {
        private string           m_guid;
        private WsWsaEndpointRef m_endTo;
        private WsWsaEndpointRef m_notifyTo;
        private long             m_expires;
        private long             m_startTime;
        private string[]         m_filter;
        private IRequestChannel  m_reqChannel;

        public string           ID              { get { return m_guid;       } set { m_guid       = value; } }
        public WsWsaEndpointRef EndTo           { get { return m_endTo;      } set { m_endTo      = value; } }
        public WsWsaEndpointRef NotifyTo        { get { return m_notifyTo;   } set { m_notifyTo   = value; } }
        public long             Expires         { get { return m_expires;    } set { m_expires    = value; } }
        public string[]         Filters         { get { return m_filter;     } set { m_filter     = value; } }
        public long             StartTime       { get { return m_startTime;  } set { m_startTime  = value; } }
        public IRequestChannel  RequestChannel  { get { return m_reqChannel; } set { m_reqChannel = value; } }
    }

    internal class DpwsWseEventSinks
    {
        private object    m_threadLock;
        private ArrayList m_sinkList;

        public DpwsWseEventSinks()
        {
            m_threadLock = new object();
            m_sinkList = new ArrayList();     
        }

        //
        // Summary:
        //     Gets the number of eventsinks actually contained in the eventsink list.
        //
        // Returns:
        //     The number of event sinks actually contained in eventsink list.
        public int Count
        {
            get
            {
                return m_sinkList.Count;
            }
        }

        // Summary:
        //     Gets or sets the eventsink at the specified index.
        //
        // Parameters:
        //   index:
        //     The zero-based index of the eventsink to get or set.
        //
        // Returns:
        //     The eventsink at the specified index.
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     index is less than zero.-or- index is equal to or greater than eventsinks list count.
        public DpwsWseEventSink this[int index]
        {
            get
            {
                return (DpwsWseEventSink)m_sinkList[index];
            }
        }

        public DpwsWseEventSink this[String eventSinkId]
        {
            get
            {
                lock (m_threadLock)
                {
                    int count = m_sinkList.Count;
                    DpwsWseEventSink eventSink;
                    for (int i = 0; i < count; i++)
                    {
                        eventSink = (DpwsWseEventSink)m_sinkList[i];

                        if (eventSink.ID == eventSinkId)
                        {
                            return eventSink;
                        }
                    }

                    return null;
                }
            }
        }

        //
        // Summary:
        //     Adds an object to the end of the eventsink list.
        //
        // Parameters:
        //   value:
        //     The System.Object to be added to the end of the eventlist.
        //     The value can be null.
        //
        // Returns:
        //     The eventsink index at which the value has been added.
        //
        public int Add(DpwsWseEventSink value)
        {
            lock (m_threadLock)
            {
                // Ignore duplicate event sinks. Delete the current sink and add the new event sink.
                int count = m_sinkList.Count;
                String valueUri = value.NotifyTo.Address.AbsoluteUri.ToLower();
                for (int i = 0; i < count; i++)
                {
                    if (((DpwsWseEventSink)m_sinkList[i]).NotifyTo.Address.AbsoluteUri.ToLower() == valueUri)
                    {
                        m_sinkList.RemoveAt(i);
                        break;
                    }
                }

                return m_sinkList.Add(value);
            }
        }

        //
        // Summary:
        //     Removes all eventsinks from the event sink list list.
        //
        public void Clear()
        {
            lock (m_threadLock)
            {
                m_sinkList.Clear();
            }
        }

        //
        // Summary:
        //     Removes the first occurrence of a specific object from the eventsink list.
        //
        // Parameters:
        //   subscription:
        //     The eventsink object to remove from the eventsink list. The value
        //     can be null.
        //
        public void Remove(DpwsWseEventSink eventSink)
        {
            lock (m_threadLock)
            {
                m_sinkList.Remove(eventSink);
            }
        }

        //
        // Summary:
        //     Removes the item of the specified index from the eventsink list.
        //
        // Parameters:
        //   index:
        //     The index of the eventsink object to remove from the eventsink list.
        //
        public void RemoveAt(int index)
        {
            lock (m_threadLock)
            {
                m_sinkList.RemoveAt(index);
            }
        }
    }

}


