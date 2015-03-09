using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Threading;
using Dpws.Device;
using Dpws.Device.Services;
using Ws.Services.Faults;
using Ws.Services.Transport.HTTP;
using Ws.Services.Utilities;
using Ws.Services.WsaAddressing;

using System.Ext;
using System.Ext.Xml;
using Ws.Services;
using Ws.Services.Xml;
using Microsoft.SPOT;
using Ws.Services.Soap;
using Ws.Services.Binding;

namespace Dpws.Device.Services
{
    /// <summary>
    /// Use to raise a hosted service event to active event sinks.
    /// </summary>
    /// <remarks>
    /// The device stack uses this class to manage event subscriptions. A device developer uses this class to
    /// send an event message to clients that have an active event subscription with an event source.
    /// </remarks>
    public class DpwsWseSubscriptionMgr
    {
        private readonly ProtocolVersion m_version;
        private bool m_persistEventConnections;
        private Hashtable m_evtChannelLookup;

        private class EventChannel
        {
            internal EventChannel(IRequestChannel chan)
            {
                RefCount = 1;
                Channel = chan;
            }
            
            internal int RefCount;
            internal IRequestChannel Channel;
        }
        
        /// <summary>
        /// Creates an instance of the Subscription manager class.
        /// </summary>
        /// <remarks>
        /// The device manages an instance of this class on behalf of the device. A device developer
        /// must use the static Device.SubScriptionMgr.FireEvent method to fire events from an event
        /// source.
        /// </remarks>
        public DpwsWseSubscriptionMgr(Binding binding, ProtocolVersion version)
        {
            m_version = version;
            m_persistEventConnections = false;
            m_evtChannelLookup = new Hashtable();
        }

        /// <summary>
        /// Persists the TCP connection for registered event handlers.
        /// </summary>
        public bool PersistEventConnections
        {
            set
            {
                m_persistEventConnections = value;
            }
        }

        /// <summary>
        /// Use this method to raise an event.
        /// </summary>
        /// <param name="hostedService">The hosted service that contains the event source.</param>
        /// <param name="eventSource">The event source that define the event.</param>
        /// <param name="eventMessage">The event message buffer.</param>
        /// <remarks>
        /// A device developer is responsible for building the event message buffer sent
        /// to clients that have an active event subscription with an event source. The subscription manager
        /// uses the hosted service parameter to access various properties of the service. The event source
        /// parameter is used to access the event sinks collection of the event source. Note: This method
        /// requires special provisions in order to properly build event message headers for each event sink.
        /// In order to send an event to a listening client the soap.header.To property must be changed for
        /// each listening client. In the future custom attribute support will solve this problem. For now
        /// however a search and replace mechanism is used to modifiy the header.To property. When a device
        /// developer builds the event message buffer they must use the search string WSDNOTIFYTOADDRESS for
        /// the To header property.
        /// </remarks>
        public void FireEvent(DpwsHostedService hostedService, DpwsWseEventSource eventSource, WsMessage msgEvt)
        {
            // Find the specified event source
            if (eventSource == null)
                throw new ArgumentNullException();
            
            DpwsWseEventSinks eventSinks = eventSource.EventSinks;

            // if there are event sources display message
            int count = eventSinks.Count;

            if (count > 0)
            {
                System.Ext.Console.Write("");
                System.Ext.Console.Write("Firing " + eventSource.Name);
                System.Ext.Console.Write("");
            }

            // Loop through event sinks and send the event message
            for (int i = count-1; i >= 0; i--)
            {
                DpwsWseEventSink eventSink = eventSinks[i];

                // Try to send event. If attempt fails delete the subscription/eventSink
                try
                {
                    msgEvt.Header = new WsWsaHeader(msgEvt.Header.Action, msgEvt.Header.RelatesTo, eventSink.NotifyTo.Address.AbsoluteUri,
                        null, null, eventSink.NotifyTo.RefParameters);

                    if(m_persistEventConnections)
                    {
                        eventSink.RequestChannel.RequestOneWay(msgEvt);
                    }
                    else
                    {
                        eventSink.RequestChannel.Open();
                        eventSink.RequestChannel.RequestOneWay(msgEvt);
                        eventSink.RequestChannel.Close();
                    }
                }
                catch (Exception e)
                {
                    System.Ext.Console.Write("");
                    System.Ext.Console.Write("FireEvent failed. Deleting EventSink! NotifyToAddress = " + eventSink.NotifyTo.Address + " Exception: " + e.Message);
                    System.Ext.Console.Write("");

                    if (!(e is WebException))
                    {
                        try
                        {
                            // Send oneway subscription end message
                            SendSubscriptionEnd(eventSink, "DeliveryFailure", hostedService.ServiceID);
                        }
                        catch { }
                    }

                    // Remove event sink from event source list
                    if(m_persistEventConnections)
                    {
                        eventSink.RequestChannel.Close();
                    }
                    eventSinks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Used by the subscription manager to send a subscription end message to a listening client.
        /// </summary>
        /// <param name="eventSink">An event sink object containing the client endpoint information.</param>
        /// <param name="subEndType">A string that specifies the reason fot the subscription end.</param>
        /// <param name="subMangerID">A id used by the client to identify this subcription.</param>
        /// <remarks>If an error occures when sending an event message this method is called to tell
        /// the client this subscription has been expired.
        /// </remarks>
        private void SendSubscriptionEnd(DpwsWseEventSink eventSink, string subEndType, string subMangerID)
        {
            // if we weren't given an EndTo, don't send
            if (eventSink.EndTo == null)
            {
                return;
            }

            if(m_persistEventConnections)
            {
                eventSink.RequestChannel.Request(SubscriptionEndResponse(eventSink, subEndType, subMangerID));
            }
            else
            {
                eventSink.RequestChannel.Open();
                eventSink.RequestChannel.Request(SubscriptionEndResponse(eventSink, subEndType, subMangerID));
                eventSink.RequestChannel.Close();
            }
        }

        /// <summary>
        /// This method build a subscription end message.
        /// </summary>
        /// <param name="eventSink">An event sink containing client endpoint information.</param>
        /// <param name="shutdownMessage">A string containing reason why the subscription is ending.</param>
        /// <param name="subMangerID">An id sent by the client that they use to reference a subscription.</param>
        /// <returns></returns>
        private WsMessage SubscriptionEndResponse(DpwsWseEventSink eventSink, string shutdownMessage, string subMangerID)
        {
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsWsaHeader responseHeader = new WsWsaHeader(
                    WsWellKnownUri.WseNamespaceUri + "/SubscriptionEnd",    // Action
                    null,                                                   // RelatesTo
                    eventSink.EndTo.Address.AbsoluteUri,                    // To
                    null, null, eventSink.EndTo.RefProperties);             // ReplyTo, From, Any

                WsMessage msg = new WsMessage(responseHeader, null, WsPrefix.Wse, null, null);

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                smw.WriteSoapMessageStart(xmlWriter, msg);

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "SubscriptionEnd", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "SubscriptionManager", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                xmlWriter.WriteString("http://" + Device.IPV4Address + ":" + Device.Port + "/" + subMangerID);
                xmlWriter.WriteEndElement(); // End Address
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "ReferenceParameters", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "Identifier", null);
                xmlWriter.WriteString(eventSink.ID);
                xmlWriter.WriteEndElement(); // End Identifier
                xmlWriter.WriteEndElement(); // End ReferenceParameters
                xmlWriter.WriteEndElement(); // End SubscriptionManager
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "Code", null);
                xmlWriter.WriteString(WsNamespacePrefix.Wse + ":" + shutdownMessage);
                xmlWriter.WriteEndElement(); // End Code
                xmlWriter.WriteEndElement(); // End SubscriptionEnd

                smw.WriteSoapMessageEnd(xmlWriter);

                msg.Body = xmlWriter.ToArray();

                // Return stream buffer
                return msg;
            }
        }

        /// <summary>
        /// Service endpoints are stored as urn:uuid's. This method is used to convert the header.To address into
        /// urn:uuid format if it is not already.
        /// </summary>
        /// <param name="toAddress">A string containing the header.To address.</param>
        /// <returns>A string containing a urn:uuid parsed from the header.To field.</returns>
        internal string FixToAddress(string toAddress)
        {
            // Make sure that the To address is a urn:uuid, use uri for parsing (cases could be complex)
            string endpointAddress = null;
            if (toAddress.IndexOf("urn") == 0 || toAddress.IndexOf("uuid") == 0 || toAddress.IndexOf("http") == 0)
            {
                // Convert to address to Uri
                Uri toUri = new Uri(toAddress);

                // Convert the to address to a urn:uuid if it is an Http endpoint
                if (toUri.Scheme == "urn")
                    endpointAddress = toUri.AbsoluteUri;
                else if (toUri.Scheme == "uuid")
                {
                    endpointAddress = "urn:" + toUri.AbsoluteUri;
                }
                else if (toUri.Scheme == "http")
                {
                    endpointAddress = "urn:uuid:" + toUri.AbsolutePath;
                }
                else
                    endpointAddress = toAddress;
            }
            else
                endpointAddress = "urn:uuid:" + toAddress;
            return endpointAddress;
        }

        /// <summary>
        /// Global eventing Subscribe stub.
        /// </summary>
        /// <param name="header">Header object.</param>
        /// <param name="reader">An XmlReader positioned at the begining of the Subscribe request body element.</param>
        /// <param name="serviceEndpoints">A Collection of serviceEndpoints used to determine what services contain the event source specified in the filter.</param>
        /// <returns>Byte array containing a Subscribe response.</returns>
        internal WsMessage Subscribe(WsWsaHeader header, XmlReader reader, WsServiceEndpoints serviceEndpoints)
        {
            WsMessage msg = null;

            // Parse Subscribe Request
            /////////////////////////////
            DpwsWseEventSink eventSink = new DpwsWseEventSink();
            try
            {
                reader.ReadStartElement("Subscribe", WsWellKnownUri.WseNamespaceUri);

                if (reader.IsStartElement("EndTo", WsWellKnownUri.WseNamespaceUri))
                {
                    eventSink.EndTo = new WsWsaEndpointRef(reader, m_version.AddressingNamespace);
                }

                reader.ReadStartElement("Delivery", WsWellKnownUri.WseNamespaceUri);
                if (reader.IsStartElement("NotifyTo", WsWellKnownUri.WseNamespaceUri))
                {
                    eventSink.NotifyTo = new WsWsaEndpointRef(reader, m_version.AddressingNamespace);
                }
                else
                {
                    throw new WsFaultException(header, WsFaultType.WseDeliverModeRequestedUnavailable);
                }

                reader.ReadEndElement();

                if (reader.IsStartElement("Expires", WsWellKnownUri.WseNamespaceUri))
                {
                    long expires = new WsDuration(reader.ReadElementString()).DurationInSeconds;

                    if (expires > 0)
                    {
                        eventSink.Expires = expires;
                    }
                    else
                    {
                        throw new WsFaultException(header, WsFaultType.WseInvalidExpirationTime);
                    }
                }
                else
                {
                    // Never Expires
                    eventSink.Expires = -1;
                }

                if (reader.IsStartElement("Filter", WsWellKnownUri.WseNamespaceUri))
                {
                    if (reader.MoveToAttribute("Dialect") == false || reader.Value != m_version.WsdpNamespaceUri + "/Action")
                    {
                        throw new WsFaultException(header, WsFaultType.WseFilteringRequestedUnavailable);
                    }

                    reader.MoveToElement();

                    String filters = reader.ReadElementString();

                    if (filters != String.Empty)
                    {
                        eventSink.Filters = filters.Split(' ');
                    }
                }

                XmlReaderHelper.SkipAllSiblings(reader);

                reader.ReadEndElement(); // Subscribe
            }
            catch (XmlException e)
            {
                throw new WsFaultException(header, WsFaultType.WseInvalidMessage, e.ToString());
            }

            // Parse urn:uuid from the To address
            string endpointAddress = FixToAddress(header.To);

            // Build a temporary collection of device services that match the specified endpoint address.
            WsServiceEndpoints matchingServices = new WsServiceEndpoints();
            for (int i = 0; i < serviceEndpoints.Count; ++i)
            {
                if (serviceEndpoints[i].EndpointAddress == endpointAddress)
                    matchingServices.Add(serviceEndpoints[i]);
            }

            // For each service with a matching endpoint and event sources add an event sink to the
            // event source collection
            for (int i = 0; i < matchingServices.Count; ++i)
            {
                DpwsWseEventSources eventSources = ((DpwsHostedService)matchingServices[i]).EventSources;

                // Set the EventSinkID
                eventSink.ID = "urn:uuid:" + Guid.NewGuid().ToString();

                // If subscribing to all event sources
                if (eventSink.Filters == null)
                {
                    int count = eventSources.Count;
                    for (int ii = 0; i < count; i++)
                    {
                        DpwsWseEventSource eventSource = eventSources[ii];
                        eventSink.StartTime = DateTime.Now.Ticks;
                        Uri key =  eventSink.NotifyTo.Address;

                        if(m_evtChannelLookup.Contains(key))
                        {
                            EventChannel ec = (EventChannel)m_evtChannelLookup[key];
                            eventSink.RequestChannel = ec.Channel;
                            Interlocked.Increment(ref ec.RefCount);
                        }
                        else
                        {
                            WS2007HttpBinding binding = new WS2007HttpBinding(new HttpTransportBindingConfig(eventSink.NotifyTo.Address, m_persistEventConnections));
                            eventSink.RequestChannel = binding.CreateClientChannel(new ClientBindingContext(m_version));

                            if(m_persistEventConnections)
                            {
                                eventSink.RequestChannel.Open();
                            }

                            m_evtChannelLookup[key] = new EventChannel(eventSink.RequestChannel);
                        }
                        eventSource.EventSinks.Add(eventSink);
                    }
                }
                else
                {
                    // If subscribing to a specific event based on an event filter.
                    DpwsWseEventSource eventSource;
                    string[] filterList = eventSink.Filters;
                    int length = filterList.Length;
                    for (int ii = 0; i < length; i++)
                    {
                        if ((eventSource = eventSources[filterList[ii]]) != null)
                        {
                            eventSink.StartTime = DateTime.Now.Ticks;
                            Uri key =  eventSink.NotifyTo.Address;

                            if(m_evtChannelLookup.Contains(key))
                            {
                                EventChannel ec = (EventChannel)m_evtChannelLookup[key];
                                eventSink.RequestChannel = ec.Channel;
                                Interlocked.Increment(ref ec.RefCount);
                            }
                            else
                            {
                                WS2007HttpBinding binding = new WS2007HttpBinding(new HttpTransportBindingConfig(eventSink.NotifyTo.Address, m_persistEventConnections));
                                eventSink.RequestChannel = binding.CreateClientChannel(new ClientBindingContext(m_version));

                                if(m_persistEventConnections)
                                {
                                    eventSink.RequestChannel.Open();
                                }

                                m_evtChannelLookup[key] = new EventChannel(eventSink.RequestChannel);
                            }
                            eventSource.EventSinks.Add(eventSink);
                        }
                        else
                        {
                            throw new Exception("Event source " + filterList[ii] + " was not found.");
                        }
                    }
                }
            }

            // Generate Response
            //////////////////////////
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsWsaHeader responseHeader = new WsWsaHeader(
                    WsWellKnownUri.WseNamespaceUri + "/SubscribeResponse",  // Action
                    header.MessageID,                                       // RelatesTo
                    header.ReplyTo.Address.AbsoluteUri,                     // To
                    null, null, null);                                      // ReplyTo, From, Any

                msg = new WsMessage(responseHeader, null, WsPrefix.Wse, null, 
                    new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID));

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                smw.WriteSoapMessageStart(xmlWriter, msg);

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "SubscribeResponse", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "SubscriptionManager", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "Address", null);
                // Create a uri. Use the path (by default will be a uuid) for the sub manager endpoint
                Uri subMgrUri = new Uri(((DpwsHostedService)matchingServices[0]).EndpointAddress);
                xmlWriter.WriteString("http://" + Device.IPV4Address + ":" + Device.Port + "/" + subMgrUri.AbsolutePath);
                xmlWriter.WriteEndElement(); // End Address
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wsa, "ReferenceParameters", null);
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "Identifier", null);
                xmlWriter.WriteString(eventSink.ID);
                xmlWriter.WriteEndElement(); // End Identifier
                xmlWriter.WriteEndElement(); // End ReferenceParameters
                xmlWriter.WriteEndElement(); // End SubscriptionManager
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "Expires", null);
                xmlWriter.WriteString(new WsDuration(eventSink.Expires).DurationString);
                xmlWriter.WriteEndElement(); // End Expires
                xmlWriter.WriteEndElement(); // End SubscribeResponse

                smw.WriteSoapMessageEnd(xmlWriter);

                // Return stream buffer
                msg.Body = xmlWriter.ToArray();
            }

            return msg;
        }

        /// <summary>
        /// Eventing UnSubscribe stub.
        /// </summary>
        /// <param name="header">Header object.</param>
        /// <param name="reader">An XmlReader positioned at the begining of the Unsubscribe request body element.</param>
        /// <param name="serviceEndpoints">A Collection of serviceEndpoints used to determine what services contain the specified event.</param>
        /// <returns>Byte array containing an UnSubscribe response.</returns>
        /// <remarks>This method is used by the stack framework. Do not use this method.</remarks>
        public WsMessage Unsubscribe(WsWsaHeader header, XmlReader reader, WsServiceEndpoints serviceEndpoints)
        {
            // Parse Unsubscribe Request
            ///////////////////////////////
            // there's no info in Unsubscribe that we actually need, just get the identifier from header
            String eventSinkID = header.Any.GetNodeValue("Identifier", WsWellKnownUri.WseNamespaceUri);

            bool eventSourceFound = false;
            if (eventSinkID != null)
            {

                // Parse urn:uuid from the To address
                string endpointAddress = FixToAddress(header.To);

                // Iterate the list of hosted services at the specified endpoint and unsubscribe from each event source
                // that matches the eventSinkID

                DpwsHostedService serv = (DpwsHostedService)Device.HostedServices[endpointAddress];

                if(serv != null)
                {
                    DpwsWseEventSources eventSources = serv.EventSources;
                        
                    // Delete Subscription
                    // Look for matching event in hosted services event sources
                    DpwsWseEventSource eventSource;
                    DpwsWseEventSinks eventSinks;
                    DpwsWseEventSink eventSink;
                    int eventSourcesCount = eventSources.Count;
                    int eventSinksCount;
                    for (int i = 0; i < eventSourcesCount; i++)
                    {
                        eventSource = eventSources[i];
                        eventSinks = eventSource.EventSinks;
                        eventSinksCount = eventSinks.Count;

                        eventSink = eventSinks[eventSinkID];
                        
                        if (eventSink != null)
                        {
                            Uri key = eventSink.NotifyTo.Address;

                            eventSourceFound = true;

                            EventChannel ec = (EventChannel)m_evtChannelLookup[key];

                            if(m_persistEventConnections && 0 == Interlocked.Decrement(ref ec.RefCount))
                            {
                                eventSink.RequestChannel.Close();
                            }
                            eventSource.EventSinks.Remove(eventSink);
                        }
                    }
                }

                if (eventSourceFound)
                {
                    // Generate Response
                    using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
                    {
                        WsWsaHeader responseHeader = new WsWsaHeader(
                            WsWellKnownUri.WseNamespaceUri + "/UnsubscribeResponse",// Action
                            header.MessageID,                                       // RelatesTo
                            header.ReplyTo.Address.AbsoluteUri,                     // To
                            null, null, null);

                        WsMessage msg = new WsMessage( responseHeader, null, WsPrefix.Wse, null, 
                            new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID));

                        WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                        smw.WriteSoapMessageStart(xmlWriter, msg);
                        smw.WriteSoapMessageEnd(xmlWriter);

                        // Return stream buffer
                        msg.Body = xmlWriter.ToArray();

                        return msg;
                    }
                }
            }

            // Something went wrong
            throw new WsFaultException(header, WsFaultType.WseEventSourceUnableToProcess);
        }

        /// <summary>
        /// Event renew stub.
        /// </summary>
        /// <param name="header">Header object.</param>
        /// <param name="reader">An XmlReader positioned at the begining of the Renew request body element.</param>
        /// <param name="serviceEndpoints">A Collection of serviceEndpoints used to determine what services contain the specified event.</param>
        /// <returns>Byte array containing an Renew response.</returns>
        /// <remarks>This method is used by the stack framework. Do not use this method.</remarks>
        public WsMessage Renew(WsWsaHeader header, XmlReader reader, WsServiceEndpoints serviceEndpoints)
        {
            long newExpiration = 0;
            String eventSinkId = String.Empty;

            // Parse Renew request
            ////////////////////////////
            try
            {
                reader.ReadStartElement("Renew", WsWellKnownUri.WseNamespaceUri);

                if (reader.IsStartElement("Expires", WsWellKnownUri.WseNamespaceUri))
                {
                    newExpiration = new WsDuration(reader.ReadElementString()).DurationInSeconds;

                    if (newExpiration <= 0)
                    {
                        throw new WsFaultException(header, WsFaultType.WseInvalidExpirationTime);
                    }
                }
                else
                {
                    // Never Expires
                    newExpiration = -1;
                }

                eventSinkId = header.Any.GetNodeValue("Identifier", WsWellKnownUri.WseNamespaceUri);

                if (eventSinkId == null)
                {
                    throw new XmlException();
                }
            }
            catch (XmlException e)
            {
                throw new WsFaultException(header, WsFaultType.WseInvalidMessage, e.ToString());
            }

            // Parse urn:uuid from the To address
            string endpointAddress = FixToAddress(header.To);

            // Iterate the list of hosted services at the specified endpoint and renew each subscription
            // with and event source that matches the eventSinkID
            DpwsWseEventSink eventSink;
            bool eventSinkFound = false;

            DpwsHostedService endpoint = (DpwsHostedService)serviceEndpoints[endpointAddress];

            if(endpoint != null)
            {
                if ((eventSink = GetEventSink(endpoint.EventSources, eventSinkId)) != null)
                {
                    eventSinkFound = true;

                    // Update event sink expires time
                    eventSink.Expires = newExpiration;
                }
            }

            // Generate Response
            if (eventSinkFound)
                return GetStatusResponse(header, newExpiration); // It's just like the GetStatus Response
            throw new WsFaultException(header, WsFaultType.WseEventSourceUnableToProcess, "Subscription was not found. ID=" + eventSinkId);
        }

        /// <summary>
        /// Event GetStatus stub.
        /// </summary>
        /// <param name="header">Header object.</param>
        /// <param name="reader">An XmlReader positioned at the begining of the GetStatus request body element.</param>
        /// <param name="serviceEndpoints">A Collection of serviceEndpoints used to determine what services contain the specified event.</param>
        /// <returns>Byte array containing an GetStatus response.</returns>
        /// <remarks>This method is used by the stack framework. Do not use this method.</remarks>
        public WsMessage GetStatus(WsWsaHeader header, XmlReader reader, WsServiceEndpoints serviceEndpoints)
        {
            // Parse GetStatus Request
            ///////////////////////////////
            // there's no info in GetStatus that we actually need, just get the identifier from header
            String eventSinkID = header.Any.GetNodeValue("Identifier", WsWellKnownUri.WseNamespaceUri);

            // Iterate the list of hosted services at the specified endpoint and get the status of the first
            // subscription matching the eventSink. Not pretty but eventing and shared service endpoints don't
            // fit together
            if (eventSinkID != null)
            {
                // Parse urn:uuid from the To address
                string endpointAddress = FixToAddress(header.To);

                DpwsHostedService endpoint = (DpwsHostedService)serviceEndpoints[endpointAddress];
                if (endpoint != null)
                {
                    DpwsWseEventSink eventSink;
                    if ((eventSink = GetEventSink(endpoint.EventSources, eventSinkID)) != null)
                    {
                        long timeRemaining = DateTime.Now.Ticks - (eventSink.StartTime + eventSink.Expires);
                        timeRemaining = timeRemaining < 0 ? 0 : timeRemaining;

                        return GetStatusResponse(header, timeRemaining);
                    }
                }
            }

            // Something went wrong
            throw new WsFaultException(header, WsFaultType.WseEventSourceUnableToProcess);
        }

        /// <summary>
        /// Iterates a collection of event sources an looks for an event sink by ID.
        /// </summary>
        /// <param name="eventSources">A collection of event sources.</param>
        /// <param name="eventSinkID">An event sink ID.</param>
        /// <returns>An event sink object if found otherwise null.</returns>
        private DpwsWseEventSink GetEventSink(DpwsWseEventSources eventSources, string eventSinkID)
        {
            DpwsWseEventSink eventSink;
            // Look for matching event in hosted services event sources
            int count = eventSources.Count;
            for (int i = 0; i < count; i++)
            {
                if ((eventSink = eventSources[i].EventSinks[eventSinkID]) != null)
                {
                    return eventSink;
                }
            }

            return null;
        }

        private WsMessage GetStatusResponse(WsWsaHeader header, long newDuration)
        {
            using(XmlMemoryWriter xmlWriter = XmlMemoryWriter.Create())
            {
                WsWsaHeader responseHeader = new WsWsaHeader(
                    WsWellKnownUri.WseNamespaceUri + "/RenewResponse",      // Action
                    header.MessageID,                                       // RelatesTo
                    header.ReplyTo.Address.AbsoluteUri,                     // To
                    null, null, null);                                      // ReplyTo, From, Any

                WsMessage msg = new WsMessage(responseHeader, null, WsPrefix.Wse, null, 
                    new WsAppSequence(Device.AppSequence, Device.SequenceID, Device.MessageID));

                WsSoapMessageWriter smw = new WsSoapMessageWriter(m_version);
                smw.WriteSoapMessageStart(xmlWriter, msg);

                // write body
                xmlWriter.WriteStartElement(WsNamespacePrefix.Wse, "Expires", null);
                xmlWriter.WriteString(new WsDuration(newDuration).DurationString);
                xmlWriter.WriteEndElement(); // End Expires

                smw.WriteSoapMessageEnd(xmlWriter);

                msg.Body = xmlWriter.ToArray();

                return msg;
            }
        }
    }
}


