using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using Ws.Services.Soap;
using Ws.Services.Mtom;
using Ws.Services.Transport;

namespace Ws.Services.Binding
{        
    /// <summary>
    /// Abtracts the configuration for the HTTP transport
    /// </summary>
    public class HttpTransportBindingConfig
    {
        /// <summary>
        /// Creates an instance of the configuration for the HTTP transport
        /// </summary>
        /// <param name="serviceUrn">The service Urn.</param>
        /// <param name="port">The servce port.</param>
        public HttpTransportBindingConfig( string serviceUrn, int port )
            : this(serviceUrn, port, false)
        {
        }

        /// <summary>
        /// Creates an instance of the configuration for the HTTP transport
        /// </summary>
        /// <param name="serviceUrn">The service Urn.</param>
        /// <param name="port">The servce port.</param>
        /// <param name="persistConnection">Persists the connection for client contexts.</param>
        public HttpTransportBindingConfig( string serviceUrn, int port, bool persistConnection )
        {
            this.ServiceUrn           = new Uri(serviceUrn);
            this.PersistentConnection = persistConnection;

            string transport = "http://" + WsNetworkServices.GetLocalIPV4Address() + ":" + port + "/";
            if(serviceUrn.IndexOf("urn:uuid:") == 0)
            {
                this.EndpointAddress = new Uri(transport + serviceUrn.Substring(9), UriKind.Absolute); // strip off urn:uuid:
            }
            else
            {
                this.EndpointAddress = new Uri(transport + serviceUrn, UriKind.Absolute);
            }
        }

        /// <summary>
        /// Creates an instance of the configuration for the HTTP transport
        /// </summary>
        /// <param name="remoteEndpoint">The remote Endpoint for this service.</param>
        public HttpTransportBindingConfig( Uri remoteEndpoint )
            : this(remoteEndpoint, false)
        {
        }

        /// <summary>
        /// Creates an instance of the configuration for the HTTP transport
        /// </summary>
        /// <param name="remoteEndpoint">The remote Endpoint for this service.</param>
        /// <param name="persistConnection">Persists the connection for client contexts.</param>
        public HttpTransportBindingConfig( Uri remoteEndpoint, bool persistConnection )
        {
            this.ServiceUrn           = null;
            this.EndpointAddress      = remoteEndpoint;
            this.PersistentConnection = persistConnection;
        }

        /// <summary>
        /// Retrieves the Urn for this service
        /// </summary>
        public readonly Uri ServiceUrn;
        /// <summary>
        /// Retrieves the endpoint for this service
        /// </summary>
        public readonly Uri EndpointAddress;
        /// <summary>
        /// Keeps the connection alive until the channel is closed (for client contexts only)
        /// </summary>
        public readonly bool PersistentConnection;
    }
    
    /// <summary>
    /// Abtracts the BindingElement for the HTTP transport
    /// </summary>
    public class HttpTransportBindingElement : TransportBindingElement
    {
        HttpListener m_httpListener;
        bool         m_persistConn;             

        private const int ReadPayload = 0x800;
        private static int MaxReadPayload = 0x20000;

        /// <summary>
        /// Creates an instance of the BindingElement for the HTTP transport
        /// </summary>
        /// <param name="cfg"></param>
        public HttpTransportBindingElement( HttpTransportBindingConfig cfg )
        {
            m_endpointUri= cfg.EndpointAddress;
            m_transportUri = cfg.EndpointAddress;
            m_serviceUrn = cfg.ServiceUrn;
            m_persistConn = cfg.PersistentConnection;
            
        }

        /// <summary>
        /// Sets the configuration for the HTTP transport binding 
        /// </summary>
        /// <param name="cfg">The configuration for this binding.</param>
        protected override void OnSetBindingConfiguration(object cfg)
        {
            HttpTransportBindingConfig config = cfg as HttpTransportBindingConfig;

            if(config != null)
            {
                m_endpointUri= config.EndpointAddress;
                m_transportUri = config.EndpointAddress;
                m_serviceUrn = config.ServiceUrn;
                m_persistConn = config.PersistentConnection;
            }
        }

        /// <summary>
        /// Opens the stream for the HTTP tansport binding 
        /// </summary>
        /// <param name="stream">The stream for this binding.</param>
        /// <param name="ctx">The context associated with the stream for this binding.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnOpen(ref Stream stream, BindingContext ctx)
        {
            if(ctx is ServerBindingContext)
            {
                m_httpListener = new HttpListener("http", m_endpointUri.Port);
                m_httpListener.Start();
            }

            return ChainResult.Handled;
        }

        /// <summary>
        /// Closes the stream for the HTTP transport binding
        /// </summary>
        /// <param name="stream">The stream for this binding.</param>
        /// <param name="ctx">The context associated with the stream for this binding.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnClose( Stream stream, BindingContext ctx )
        {
            if (ctx is ServerBindingContext)
            {
                if (ctx.ContextObject != null && ctx.ContextObject is HttpListenerContext)
                {
                    try
                    {
                        ((HttpListenerContext)ctx.ContextObject).Close(ctx.CloseTimeout.Seconds);
                    }
                    catch
                    {
                    }
                    ctx.ContextObject = null;
                }

                if (m_httpListener != null)
                {
                    try
                    {
                        m_httpListener.Close();
                    }
                    catch
                    {
                    }
                    m_httpListener = null;
                }
            }
            else
            {
                if(ctx.ContextObject != null && ctx.ContextObject is IDisposable)
                {
                    try
                    {
                        ((IDisposable)ctx.ContextObject).Dispose();
                    }
                    catch
                    {
                    }
                    ctx.ContextObject = null;
                }
            }

            return ChainResult.Handled;
        }

        /// <summary>
        /// Processes a message
        /// </summary>
        /// <param name="stream">The message being processed.</param>
        /// <param name="ctx">The context associated with the message.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnProcessInputMessage(ref WsMessage msg, BindingContext ctx)
        {
            byte[] soapResponse = null;
            WebHeaderCollection headers = null;

            if(ctx is ClientBindingContext)
            {                
                HttpWebRequest request = ctx.ContextObject as HttpWebRequest;
                
                if(request == null)
                {
                    msg = null;
                    return ChainResult.Abort;
                }

                HttpWebResponse resp = request.GetResponse() as HttpWebResponse;
               
                if (resp == null)
                {
                    throw new WebException("", WebExceptionStatus.ReceiveFailure); // No response was received on the HTTP channel
                }
                
                try                    
                {
                    headers = (System.Net.WebHeaderCollection)resp.Headers;

                    if (resp.ProtocolVersion != HttpVersion.Version11)
                    {
                        throw new IOException(); // Invalid http version in response line.
                    }
            
                    if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.Accepted)
                    {
                        if (resp.ContentType.IndexOf("application/soap+xml") == -1)
                        {
                            throw new IOException(); // Bad status code in response
                        }
                    }
            
                    if (resp.ContentLength > 0)
                    {
                        
                        // Return the soap response.
                        soapResponse        = new byte[(int)resp.ContentLength];
                        Stream respStream   = resp.GetResponseStream();

                        respStream.ReadTimeout = (int)(ctx.ReceiveTimeout.Ticks / TimeSpan.TicksPerMillisecond);
            
                        // Now need to read all data. We read in the loop until resp.ContentLength or zero bytes read.
                        // Zero bytes read means there was error on server and it did not send all data.
                        int respLength = (int)resp.ContentLength;
                        for (int totalBytesRead = 0; totalBytesRead < respLength; )
                        {
                            int bytesRead = respStream.Read(soapResponse, totalBytesRead, (int)resp.ContentLength - totalBytesRead);
                            // If nothing is read - means server closed connection or timeout. In this case no retry.
                            if (bytesRead == 0)
                            {
                                break;
                            }
            
                            // Adds number of bytes read on this iteration.
                            totalBytesRead += bytesRead;
                        }

                        headers = resp.Headers;
                    }
                    //
                    // ContentLenght == 0 is OK
                    //
                    else if(resp.ContentLength < 0)
                    {
                        throw new ProtocolViolationException(); // Invalid http header, content lenght < 0
                    }
                }
                finally
                {
                    resp.Dispose();
                    request.Dispose();
                    ctx.ContextObject = null;
                }
            }
            else // server waits for messages
            {
                HttpListenerContext listenerContext;

                try
                {
                    if(m_persistConn && ctx.ContextObject != null)
                    {
                        listenerContext = (HttpListenerContext)ctx.ContextObject;

                        listenerContext.Reset();
                    }
                    else
                    {
                        if (m_httpListener == null)
                        {
                            msg = null;
                            return ChainResult.Abort;
                        }

                        listenerContext = m_httpListener.GetContext();
                    }

                    if (listenerContext == null)
                    {
                        msg = null;
                        return ChainResult.Abort;
                    }

                    ctx.ContextObject = listenerContext;

                    // The context returned by m_httpListener.GetContext(); can be null in case the service was stopped.
                    HttpListenerRequest listenerRequest = listenerContext.Request;

                    HttpListenerResponse listenerResponse = listenerContext.Response;

                    listenerRequest.InputStream.ReadTimeout = (int)(ctx.ReceiveTimeout.Ticks / TimeSpan.TicksPerMillisecond);
                    listenerResponse.OutputStream.WriteTimeout = (int)(ctx.SendTimeout.Ticks / TimeSpan.TicksPerMillisecond);

                    headers = (System.Net.WebHeaderCollection)listenerRequest.Headers;

                    System.Ext.Console.Write("Request From: " + listenerRequest.RemoteEndPoint.ToString());

                    // Checks and process headers important for DPWS
                    if (!ProcessKnownHeaders(listenerContext))
                    {
                        msg = null;
                        return ChainResult.Abort;
                    }

                    soapResponse = null;

                    int messageLength = (int)listenerRequest.ContentLength64;
                    if (messageLength > 0)
                    {
                        // If there is content length for the message, we read it complete.
                        soapResponse = new byte[messageLength];

                        for (int offset = 0; offset < messageLength; )
                        {
                            int noRead = listenerRequest.InputStream.Read(soapResponse, offset, messageLength - offset);
                            if (noRead == 0)
                            {
                                throw new IOException("Http server got only " + offset + " bytes. Expected to read " + messageLength + " bytes.");
                            }

                            offset += noRead;
                        }
                    }
                    else
                    {
                        // In this case the message is chunk encoded, but m_httpRequest.InputStream actually does processing.
                        // So we we read until zero bytes are read.
                        bool readComplete = false;
                        int bufferSize = ReadPayload;
                        soapResponse = new byte[bufferSize];
                        int offset = 0;
                        while (!readComplete)
                        {
                            while (offset < ReadPayload)
                            {
                                int noRead = listenerRequest.InputStream.Read(soapResponse, offset, messageLength - offset);
                                // If we read zero bytes - means this is end of message. This is how InputStream.Read for chunked encoded data.
                                if (noRead == 0)
                                {
                                    readComplete = true;
                                    break;
                                }

                                offset += noRead;
                            }

                            // If read was not complete - increase the buffer.
                            if (!readComplete)
                            {
                                bufferSize += ReadPayload;
                                byte[] newMessageBuf = new byte[bufferSize];
                                Array.Copy(soapResponse, newMessageBuf, offset);
                                soapResponse = newMessageBuf;
                            }
                        }
                    }                
                }
                catch
                {
                    ctx.ContextObject = null;
                    throw;
                }
            }

            if(headers != null)
            {
                string [] keys = headers.AllKeys;
                int       len  = keys.Length;

                ArrayList props = ctx.BindingProperties;
                
                for(int i=0; i<len; i++)
                {
                    string key = keys[i];
                    
                    if(!WebHeaderCollection.IsRestricted(key))
                    {
                        props.Add( new BindingProperty( "header", key, headers[key] ) );
                    }
                }
            }

            System.Ext.Console.Write(soapResponse);
         

            msg.Body = soapResponse;

            return ChainResult.Continue;
        }

        /// <summary>
        /// Verifies values of specific headers.
        /// </summary>
        /// <returns>True if parsing is successful</returns>
        private bool ProcessKnownHeaders(HttpListenerContext listenerContext)
        {
            HttpListenerRequest listenerRequest = listenerContext.Request;
            
            HttpStatusCode errorCode = 0;
            string errorName = "";
            // The HTTP methid should be GET or POST
            if (listenerRequest.HttpMethod != "POST" && listenerRequest.HttpMethod != "GET")
            {
                errorCode = HttpStatusCode.NotImplemented;
            }

            // HTTP version should be 1.1
            if (listenerRequest.ProtocolVersion != HttpVersion.Version11)
            {
                errorCode = HttpStatusCode.BadRequest;
            }

            if (listenerRequest.ContentLength64 > MaxReadPayload)
            {
                errorCode = HttpStatusCode.Forbidden; // 403
                errorName = HttpKnownHeaderNames.ContentLength;
            }

            WebHeaderCollection webHeaders = (System.Net.WebHeaderCollection)listenerRequest.Headers;

            string strMimeVersion = webHeaders[HttpKnownHeaderNames.MimeVersion];
            if (strMimeVersion != null)
            {
                if (strMimeVersion != "1.0")
                {
                    errorCode = HttpStatusCode.NotFound;
                    errorName = HttpKnownHeaderNames.MimeVersion;
                }
            }

            if (errorCode != 0)
            {
                SendError((int)errorCode, errorName, listenerContext);
                return false;
            }

            return true;
        }

        private void SendError(int errorCode, string message, HttpListenerContext ctx)
        {
            try
            {
                HttpListenerResponse listenerResponse = ctx.Response;
                
                listenerResponse.StatusCode = errorCode;
                listenerResponse.StatusDescription = message;
                listenerResponse.Close();
                System.Ext.Console.Write("Http error response: " + "HTTP/1.1 " + errorCode + " " + message);
            }
            catch (Exception e)
            {
                System.Ext.Console.Write("Http error response failed send: " + e.Message);
            }
        }
        
        /// <summary>
        /// Processes a message 
        /// </summary>
        /// <param name="msg">The message being processed.</param>
        /// <param name="ctx">The context associated with the message.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnProcessOutputMessage( ref WsMessage msg, BindingContext ctx )
        {
            ArrayList props = ctx.BindingProperties;

            byte[] message = null;
            string contentType = "text/plain";

            if (msg != null)
            {
                message = msg.Body as byte[];

                if (msg.BodyParts != null)
                {
                    contentType = "Multipart/Related;boundary=" +
                        msg.MtomPropeties.boundary +
                        ";type=\"application/xop+xml\";start=\"" +
                        msg.MtomPropeties.start +
                        "\";start-info=\"application/soap+xml\"";

                    ctx.BindingProperties.Add(new BindingProperty("header", HttpKnownHeaderNames.Server, "Microsoft-MF HTTP 1.0"));
                    ctx.BindingProperties.Add(new BindingProperty("header", HttpKnownHeaderNames.MimeVersion, "1.0"));
                    ctx.BindingProperties.Add(new BindingProperty("header", HttpKnownHeaderNames.Date, DateTime.Now.ToString()));
                }
                else
                {
                    contentType = "application/soap+xml; charset=utf-8";
                }
            }

            if(ctx is ClientBindingContext)
            {
                if (message == null) return ChainResult.Abort;

                HttpWebRequest request;

                try
                {
                    if(!m_persistConn || ctx.ContextObject == null)
                    {
                        request = HttpWebRequest.Create(new Uri(m_transportUri.AbsoluteUri)) as HttpWebRequest;
                        request.Timeout          = (int)(ctx.OpenTimeout.Ticks / TimeSpan.TicksPerMillisecond);
                        request.ReadWriteTimeout = (int)(ctx.ReceiveTimeout.Ticks / TimeSpan.TicksPerMillisecond);

                        ctx.ContextObject = request;
                    }
                    else
                    {
                        request = (HttpWebRequest)ctx.ContextObject;

                        request.Reset();
                    }
                    

                    // Post method
                    request.Method = "POST";

                    WebHeaderCollection headers = request.Headers;

                    request.ContentType = contentType;
                    request.UserAgent   = "MFWsAPI";

                    request.Headers.Add(HttpKnownHeaderNames.CacheControl, "no-cache");
                    request.Headers.Add(HttpKnownHeaderNames.Pragma      , "no-cache");

                    if (props != null)
                    {
                        int len = props.Count;
                        for (int i = 0; i < len; i++)
                        {
                            BindingProperty prop = (BindingProperty)props[i];
                            string container = prop.Container;

                            if (container == "header")
                            {
                                headers.Add(prop.Name, (string)prop.Value);
                            }
                            else if (container == null || container == "")
                            {
                                string name = prop.Name;

                                if (name == HttpKnownHeaderNames.ContentType)
                                {
                                    request.ContentType = (string)prop.Value;
                                }
                                else if (name == HttpKnownHeaderNames.UserAgent)
                                {
                                    request.UserAgent = (string)prop.Value;
                                }
                            }
                        }
                    }

                    if (message != null)
                    {
                        System.Ext.Console.Write("Http message sent: ");
                        System.Ext.Console.Write(message);

                        request.ContentLength = message.Length;

                        using (Stream stream = request.GetRequestStream())
                        {
                            // Write soap message
                            stream.Write(message, 0, message.Length);

                            // Flush the stream and force a write
                            stream.Flush();
                        }
                    }
                }
                catch
                {
                    ctx.ContextObject = null;

                    throw;
                }
            }
            else
            {
                HttpListenerContext listenerContext = ctx.ContextObject as HttpListenerContext;

                if(listenerContext == null) return ChainResult.Abort;
                
                HttpListenerResponse listenerResponse = listenerContext.Response;

                if (listenerResponse == null || listenerResponse.OutputStream == null)
                {
                    ctx.ContextObject = null;
                    return ChainResult.Abort;
                }

                try
                {
                    StreamWriter streamWriter = new StreamWriter(listenerResponse.OutputStream);

                    // Write Header, if message is null write accepted
                    if (message == null || (msg != null && msg.Header != null && msg.Header.IsFaultMessage))
                        listenerResponse.StatusCode = 202;
                    else
                        listenerResponse.StatusCode = 200;

                    // Check to see it the hosted service is sending mtom
                    WebHeaderCollection headers = listenerResponse.Headers;

                    listenerResponse.ContentType = contentType;

                    bool isChunked = false;

                    if (props != null)
                    {
                        int len = props.Count;
                        for (int i = 0; i < len; i++)
                        {
                            BindingProperty prop = (BindingProperty)props[i];
                            string container = prop.Container;
                            string name = prop.Name;
                            string value = (string)prop.Value;

                            if (container == "header")
                            {
                                if (!isChunked && name == HttpKnownHeaderNames.TransferEncoding && value.ToLower() == "chunked")
                                {
                                    isChunked = true;
                                }

                                headers.Add(name, (string)prop.Value);
                            }
                            else if (container == null || container == "")
                            {
                                if (name == HttpKnownHeaderNames.ContentType)
                                {
                                    listenerResponse.ContentType = (string)prop.Value;
                                    System.Ext.Console.Write(HttpKnownHeaderNames.ContentType + ": " + listenerResponse.ContentType);
                                }
                            }
                        }
                    }

                    // If chunked encoding is enabled write chunked message else write Content-Length
                    if (isChunked)
                    {
                        // Chunk message
                        int bufferIndex = 0;
                        int chunkSize = 0;
                        int defaultChunkSize = 0xff;
#if DEBUG
                    byte[] displayBuffer = new byte[defaultChunkSize];
#endif
                        while (bufferIndex < message.Length)
                        {

                            // Calculate chunk size and write to stream
                            chunkSize = message.Length - bufferIndex < defaultChunkSize ? message.Length - bufferIndex : defaultChunkSize;
                            streamWriter.WriteLine(chunkSize.ToString("{0:X}"));
                            System.Ext.Console.Write(chunkSize.ToString("{0:X}"));

                            // Write chunk
                            streamWriter.WriteBytes(message, bufferIndex, chunkSize);
                            streamWriter.WriteLine();
#if DEBUG
                            Array.Copy(message, bufferIndex, displayBuffer, 0, chunkSize);
                            System.Ext.Console.Write(displayBuffer, bufferIndex, chunkSize);
#endif

                            // Adjust buffer index
                            bufferIndex = bufferIndex + chunkSize;
                        }

                        // Write 0 length and blank line
                        streamWriter.WriteLine("0");
                        streamWriter.WriteLine();
                        System.Ext.Console.Write("0");
                        System.Ext.Console.Write("");

                    }
                    else
                    {
                        if (message == null)
                        {
                            listenerResponse.ContentLength64 = 0;
                        }
                        else
                        {
                            listenerResponse.ContentLength64 = message.Length;
                        }

                        System.Ext.Console.Write("Content Length: " + listenerResponse.ContentLength64);

                        // If an empty message is returned (i.e. oneway request response) don't send
                        if (message != null && message.Length > 0)
                        {
                            System.Ext.Console.Write(message);

                            // Write soap message
                            streamWriter.WriteBytes(message, 0, message.Length);
                        }
                    }

                    // Flush the stream and return
                    streamWriter.Flush();

                }
                catch
                {
                    return ChainResult.Abort;
                }
                finally
                {
                    if (m_persistConn)
                    {
                        listenerResponse.Detach();
                    }
                    else
                    {
                        listenerContext.Close( ctx.CloseTimeout.Seconds );
                        ctx.ContextObject = null;
                    }
                }
            }
            return ChainResult.Handled;
        }
    }
}

