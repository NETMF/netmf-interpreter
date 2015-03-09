using System;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Xml;
using Ws.Services;
using Ws.Services.WsaAddressing;
using Ws.Services.Faults;
using Ws.Services.Mtom;
using Ws.Services.Soap;
using Ws.Services.Transport;
using Ws.Services.Utilities;
using _Bind=Ws.Services.Binding;

using System.Ext;
using Microsoft.SPOT;
using System.Runtime.CompilerServices;

namespace Ws.Services.Transport.HTTP
{
    /// <summary>
    /// An Http Service host listens for and processes request made to it's service endpoints.
    /// </summary>
    internal class WsHttpServiceHost
    {
        // Fields
        private bool                   m_isStarted;
        private Thread                 m_thread;
        private WsServiceEndpoints     m_serviceEndpoints;
        private WsThreadManager        m_threadManager;
        private _Bind.Binding          m_binding;
        private _Bind.IReplyChannel    m_replyChannel;
        private _Bind.ServerBindingContext m_ctx;

        private static int m_maxReadPayload = 0x20000;

        /// <summary>
        /// Creates a http service host.
        /// </summary>
        /// <param name="port">An integer containing the port number this host will listen on.</param>
        /// <param name="serviceEndpoints">A collection of service endpoints this transport service can dispatch to.</param>
        public WsHttpServiceHost(_Bind.Binding binding, WsServiceEndpoints serviceEndpoints)
        {
            m_threadManager    = new WsThreadManager(5, "Http");            
            m_binding          = binding;
            m_serviceEndpoints = serviceEndpoints;
            m_isStarted        = false;
        }

        /// <summary>
        /// Use to get or set the maximum number of processing threads for Udp request. Default is 5.
        /// </summary>
        public int MaxThreadCount { get { return m_threadManager.MaxThreadCount; } set { m_threadManager.MaxThreadCount = value; } }


        /// <summary>
        /// Use to get or set the maximum number of incoming messages to hold for processing.  Default is 20.
        /// </summary>
        public int MaxRequestQueue 
        { 
            get { return WsHttpMessageProcessor.s_maxReqQueueSize; } 
            set 
            { 
                if(value <= 0) throw new ArgumentException();

                WsHttpMessageProcessor.s_maxReqQueueSize = value;
            }
        }
                
        
        /// <summary>
        /// Property containing the maximum message size this transport service will accept.
        /// </summary>
        public static int MaxReadPayload { get { return m_maxReadPayload; } set { m_maxReadPayload = value; } }

        /// <summary>
        /// Use to start the Http Server listening for request.
        /// </summary>
        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        public void Start(_Bind.ServerBindingContext ctx)
        {
            if (m_isStarted) throw new InvalidOperationException();

            m_isStarted = true;

            m_ctx = ctx;

            m_replyChannel = m_binding.CreateServerChannel(ctx);
            m_replyChannel.Open();

            m_thread = new Thread(new ThreadStart(this.Listen));
            m_thread.Start();
        }

        /// <summary>
        /// Use to stop the Http service.
        /// </summary>
        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        public void Stop()
        {
            if(!m_isStarted) throw new InvalidOperationException();

            m_isStarted = false;

            WsHttpMessageProcessor.StopProcessing();

            m_replyChannel.Close();
            m_thread.Join();
        }

        /// <summary>
        /// Collection property containing service endpoints for this service host.
        /// </summary>
        public WsServiceEndpoints ServiceEndpoints { get { return m_serviceEndpoints; } set { m_serviceEndpoints = value; } }

        /// <summary>
        /// HttpServer Socket Listener
        /// </summary>
        public void Listen()
        {
            // Create listener and start listening
            while (m_isStarted)
            {
                try
                {
                    _Bind.RequestContext context = m_replyChannel.ReceiveRequest();

                    // The context returned by m_httpListener.GetContext(); can be null in case the service was stopped.
                    if (context != null)
                    {
                        WsHttpMessageProcessor processor = new WsHttpMessageProcessor(m_serviceEndpoints);

                        // Try to get a processing thread and process the request
                        m_threadManager.StartNewThread(processor, context);
                        m_ctx.ContextObject = context.m_context.ContextObject;
                    }
                }
                catch
                {
                    m_ctx.ContextObject = null;
                }
            }
        }
    }

    sealed class WsHttpMessageProcessor : IWsTransportMessageProcessor
    {
        // Fields
        private WsServiceEndpoints    m_serviceEndpoints;
        private static Queue          s_requests     = new Queue();
        private static AutoResetEvent s_requestEvent = new AutoResetEvent(false);
        private static int            s_threadCnt    = 0;
        internal static int           s_maxReqQueueSize = 20;
        private static bool           s_exit         = false;


        /// <summary>
        /// HttpProcess()
        ///     Summary:
        ///         Main Http processor class.
        /// </summary>
        /// <param name="serviceEndpoints">A collection of service endpoints.</param>
        /// <param name="s">
        /// Socket s
        /// </param>
        public WsHttpMessageProcessor(WsServiceEndpoints serviceEndpoints)
        {
            m_serviceEndpoints = serviceEndpoints;
        }

        /// <summary>
        /// Adds a request object to be processed
        /// </summary>
        public int AddRequest(object request)
        {
            lock(s_requests)
            {
                if(s_requests.Count >= s_maxReqQueueSize)
                {
                    s_requests.Dequeue();
                }
                s_requests.Enqueue(request);
            }
    
            s_requestEvent.Set();
            
            return s_requests.Count;
        }

        /// <summary>
        /// Stop all threads so that the server can shut down.
        /// </summary>
        public static void StopProcessing()
        {
            s_exit = true;
            lock(s_requests)
            {
                s_requests.Clear();
            }
            while(s_threadCnt > 0)
            {
                s_requestEvent.Set();
                Thread.Sleep(0);
            }
        }
    
        /// <summary>
        /// Http servers message processor. This method reads a message from a socket and calls downstream
        /// processes that parse the request, dispatch to a method and returns a response.
        /// </summary>
        /// <remarks>The parameters should always be set to null. See IWsTransportMessageProcessor for details.</remarks>
        public void ProcessRequest()
        {
            _Bind.RequestContext request;
        
            Interlocked.Increment(ref s_threadCnt);
        
            while(true)
            {
                if (s_exit || (s_requests.Count == 0 && !s_requestEvent.WaitOne(1000, false))) break;
        
                lock (s_requests)
                {
                    if (s_requests.Count == 0)
                    {
                        continue;
                    }
        
                    request = (_Bind.RequestContext)s_requests.Dequeue();
                }
                
                // Process the message
                if (request.Message != null)
                {
                    WsMessage response = ProcessRequestMessage(request);
                    
                    // null response indicates error which requires a response.  IsOneWay, on the other hand,
                    // should not send a response (for events and oneway messages).
                    if(response == null || response.Header == null || !response.Header.IsOneWayResponse)
                    {
                        request.Reply(response);
                    }
                }
            }
            
            Interlocked.Decrement(ref s_threadCnt);
        }
    
        /// <summary>
        /// Gets the current number of threads currently processing the given message transport
        /// </summary>
        public int ThreadCount
        {
            get { return s_threadCnt; }
        }

        /// <summary>
        /// Parses a transport message and builds a header object and envelope document then calls processRequest
        /// on a service endpoint.
        /// </summary>
        /// <param name="soapRequest">WsRequestMessage object containing a raw soap message or mtom soap request.</param>
        /// <returns>WsResponseMessage object containing the soap response returned from a service endpoint.</returns>
        private WsMessage ProcessRequestMessage(_Bind.RequestContext context)
        {
            // Now check for implementation specific service endpoints.
            IWsServiceEndpoint serviceEndpoint = null;
            string             endpointAddress;
            WsMessage          soapRequest = context.Message;
            WsWsaHeader        header      = soapRequest.Header;

            // If this is Uri convert it
            if (header.To.IndexOf("urn") == 0 || header.To.IndexOf("http") == 0)
            {

                // Convert to address to Uri
                Uri toUri;
                try
                {
                    toUri = new Uri(header.To);
                }
                catch
                {
                    System.Ext.Console.Write("Unsupported Header.To Uri format: " + header.To);
                    return WsFault.GenerateFaultResponse(header, WsFaultType.ArgumentException, "Unsupported Header.To Uri format", context.Version);
                }

                // Convert the to address to a Urn:uuid if it is an Http endpoint
                if (toUri.Scheme == "urn")
                    endpointAddress = toUri.AbsoluteUri;
                else if (toUri.Scheme == "http")
                {
                    endpointAddress = "urn:uuid:" + toUri.AbsolutePath.Substring(1);
                }
                else
                    endpointAddress = header.To;
            }
            else
                endpointAddress = "urn:uuid:" + header.To;

            // Look for a service at the requested endpoint that contains an operation matching the Action            
            IWsServiceEndpoint ep = m_serviceEndpoints[endpointAddress];

            if(ep != null)
            {
                if (ep.ServiceOperations[header.Action] != null)
                {
                    serviceEndpoint = ep;
                }
            }

            if(serviceEndpoint == null)
            {
                IWsServiceEndpoint mex = m_serviceEndpoints.DiscoMexService; // mex endpoint

                // If either the MEX endpoint or any of the services endpoints match
                // the requested endpoint then see if the requested action is for the discovery
                // service.
                if (mex != null && 
                   (mex.EndpointAddress == endpointAddress || ep != null) &&
                    mex.ServiceOperations[header.Action] != null)
                {
                    serviceEndpoint = mex;
                }
            }
            
            // If a matching service endpoint is found call operation
            if (serviceEndpoint != null)
            {
                // Process the request
                WsMessage response;
                try
                {
                    response = serviceEndpoint.ProcessRequest(soapRequest);
                }
                catch (WsFaultException e)
                {
                    return WsFault.GenerateFaultResponse(e, context.Version);
                }
                catch (Exception e)
                {
                    return WsFault.GenerateFaultResponse(header, WsFaultType.Exception, e.ToString(), context.Version);
                }

                return response;
            }

            // Unreachable endpoint requested. Generate fault response
            return WsFault.GenerateFaultResponse(header, WsFaultType.WsaDestinationUnreachable, "Unknown service endpoint", context.Version);
        }

    }
}


