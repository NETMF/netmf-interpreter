////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System.Net.Sockets;
    using System.Collections;
    using System.Threading;
    using Microsoft.SPOT.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Provides a simple, programmatically controlled HTTP protocol listener.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// This class enables using a socket to receive data that uses the HTTP
    /// protocol.
    /// </remarks>
    public class HttpListener
    {
        /// <summary>
        /// Indicates whether the listener is waiting on an http or https
        /// connection.
        /// </summary>
        bool m_IsHttpsConnection;

        /// <summary>
        /// The certificate to send during https authentication.
        /// </summary>
        X509Certificate m_httpsCert;

        /// <summary>
        /// This value is the number of connections that can be ready but are
        /// not retrieved through the Accept call.
        /// </summary>
        /// <remarks>
        /// This value is passed to the <b>Listen</b> method of the socket.
        /// </remarks>
        private const int MaxCountOfPendingConnections = 10;

        /// <summary>
        /// The time we keep connection idle with HTTP 1.1
        /// This is one minute.
        /// </summary>
        internal const int DefaultKeepAliveMilliseconds = 60000;

        /// <summary>
        /// Server socket for incoming connections.
        /// </summary>
        private Socket m_listener;

        /// <summary>
        /// The MAXIMUM length, in kilobytes (1024 bytes), of the request
        /// headers.
        /// </summary>
        internal int m_maxResponseHeadersLen;

        /// <summary>
        /// Event that indicates arrival of new event from client.
        /// </summary>
        private AutoResetEvent m_RequestArrived;

        /// <summary>
        /// The queue of connected networks streams with pending client data.
        /// </summary>
        Queue m_InputStreamsQueue;

        /// <summary>
        /// Port number for the server socket.
        /// </summary>
        private int m_Port;

        /// <summary>
        /// Indicates whether the listener is started and is currently accepting
        /// connections.
        /// </summary>
        private bool m_ServiceRunning;

        /// <summary>
        /// Indicates whether the listener has been closed
        /// </summary>
        private bool m_Closed;

        /// <summary>
        /// Array of connected client sockets.
        /// </summary>
        private ArrayList m_ClientStreams;

        /// <summary>
        /// Http Thread for accepting new connections.
        /// </summary>
        private Thread m_thAccept;

        /// <summary>
        /// Creates an HTTP or HTTPS listener on the standard ports.
        /// </summary>
        /// <param name="prefix">Prefix ( http or https ) to start listen</param>
        /// <remarks>In the desktop version of .NET, the constructor for this
        /// class has no arguments.</remarks>
        public HttpListener(string prefix)
        {
            InitListener(prefix, -1);
        }

        /// <summary>
        /// Creates an HTTP or HTTPS listener on the specified port.
        /// </summary>
        /// <param name="prefix">The prefix for the service, either "http" or
        /// "https".</param>
        /// <param name="port">The port to start listening on.  If -1, the
        /// default port is used (port 80 for http, or port 443 for https).
        /// </param>
        /// <remarks>In the desktop version of .NET, the constructor for this
        /// class has no arguments.</remarks>
        public HttpListener(string prefix, int port)
        {
            InitListener(prefix, port);
        }

        /// <summary>
        /// Initializes the listener.
        /// </summary>
        /// <param name="prefix">The prefix for the service, either "http" or
        /// "https".</param>
        /// <param name="port">The port to start listening on.  If -1, the
        /// default port is used (port 80 for http, or port 443 for https).
        /// </param>
        private void InitListener(string prefix, int port)
        {
            switch (prefix.ToLower())
            {
                case "http":
                    {
                        m_IsHttpsConnection = false;
                        m_Port = Uri.HttpDefaultPort;
                        break;
                    }
                case "https":
                    {
                        m_IsHttpsConnection = true;
                        m_Port = Uri.HttpsDefaultPort;
                        break;
                    }
                default: throw new ArgumentException("Prefix should be http or https");
            }

            if (port != -1)
            {
                m_Port = port;
            }

            // Default members initialization
            m_maxResponseHeadersLen = 4;
            m_RequestArrived = new AutoResetEvent(false);
            m_InputStreamsQueue = new Queue();
            m_ClientStreams = new ArrayList();
        }

        /// <summary>
        /// Adds a new output stream to the list of connected streams.
        /// </summary>
        /// <remarks>This is an internal function, not visible to the user.
        /// </remarks>
        /// <param name="clientStream">The stream to add.</param>
        internal void AddClientStream(OutputNetworkStreamWrapper clientStream)
        {
            lock(m_ClientStreams)
            {
                m_ClientStreams.Add(clientStream);
            }
        }

        /// <summary>
        /// Removes the specified output stream from the list of connected
        /// streams.
        /// </summary>
        /// <param name="clientStream">The stream to remove.</param>
        internal void RemoveClientStream(OutputNetworkStreamWrapper clientStream)
        {
            lock(m_ClientStreams)
            {
                for (int i = 0; i < m_ClientStreams.Count; i++)
                {
                    if (clientStream == m_ClientStreams[i])
                    {
                        m_ClientStreams.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Packages together an HttpListener and a socket.
        /// </summary>
        /// <remarks>This class is used to package together an HttpListener and a socket.
        /// We need to start new thread and pass 2 parameters - instance of listener and socket.
        /// For that purpose we create class that keeps references to both listerner and socket and
        /// start thread using member function of this class as delegate.
        /// Internal class not visible to user.</remarks>
        private class HttpListernerAndStream
        {
            internal HttpListernerAndStream(HttpListener listener, OutputNetworkStreamWrapper outputStream)
            {
                m_listener = listener;
                m_outStream = outputStream;
            }

            internal HttpListener m_listener;
            internal OutputNetworkStreamWrapper m_outStream;

            // Forwards to waiting function of the listener.
            internal void AddToWaitingConnections()
            {
                m_listener.WaitingConnectionThreadFunc(m_outStream);
            }
        }

        internal void AddToWaitingConnections(OutputNetworkStreamWrapper outputStream)
        {
            // Create a thread that blocks onsocket.Poll - basically waits for new data from client.
            HttpListernerAndStream listAndSock = new HttpListernerAndStream(this, outputStream);

            // Creates new thread to wait on data
            Thread thWaitData = new Thread(listAndSock.AddToWaitingConnections);
            thWaitData.Start();
        }

        /// <summary>
        /// Waits for new data from the client.
        /// </summary>
        private void WaitingConnectionThreadFunc(OutputNetworkStreamWrapper outputStream)
        {
            try
            {
                // This is a blocking call waiting for more data. 
                outputStream.m_Socket.Poll(DefaultKeepAliveMilliseconds * 1000, SelectMode.SelectRead);

                if (outputStream.m_Socket.Available > 0)
                {

                    // Add this connected stream to the list.
                    lock (m_InputStreamsQueue)
                    {
                        m_InputStreamsQueue.Enqueue(outputStream);
                    }

                    // Set event that client stream or exception is added to the queue.
                    m_RequestArrived.Set();
                }
                else // If no data available - means connection was close on other side or timed out.
                {
                    outputStream.Dispose();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Shuts down the <itemref>HttpListener</itemref> object immediately,
        /// discarding all currently queued requests.
        /// </summary>
        /// <remarks>This method disposes of all resources held by this
        /// listener.  Any pending requests are unable to complete.  To shut
        /// down the <itemref>HttpListener</itemref> object after processing
        /// currently queued requests, use the
        /// <see cref='System.Net.HttpListener.Close'/> method.
        /// <para>
        /// After calling this method, you will receive an
        /// <see cref='ObjectDisposedException'/> if you attempt to use this
        /// <itemref>HttpListener</itemref>.
        /// </para>
        /// </remarks>
        public void Abort()
        {
            lock (this)
            {
                // First we shut down the service.
                Close();
                
                // Now we need to go through list of all client sockets and close all of them.
                // This will cause exceptions on read/write operations on these sockets.
                foreach (OutputNetworkStreamWrapper netStream in m_ClientStreams)
                {
                    netStream.Close();
                }
                m_ClientStreams.Clear();
            }

            if (m_thAccept != null)
            {
                m_thAccept.Join();
            }
        }

        /// <summary>
        /// Waits for new connections from the client.
        /// </summary>
        /// <remarks>On new connections, this method enques the new input
        /// network stream and sets an event that a new connection is available.
        /// </remarks>
        private void AcceptThreadFunc()
        {
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            // If there was no exception up to this point, means we succeded to start listening.
            m_ServiceRunning = true;
            int retry = 0;

            // The Start function is waiting on this event. We set it to indicate that
            // thread that waits for connections is already started.
            m_RequestArrived.Set();

            // The value of m_serviceStarted normally is changed from other thread by calling Stop.
            while (m_ServiceRunning)
            {
                Socket clientSock;
                // Need to create NetworkStream or SSL stream depending on protocol used.
                NetworkStream netStream = null;

                try
                {
                    // It is important that multithread access to m_listener.Accept(); is not locked.
                    // If it was locked - then Close or Stop would be blocked potnetially for ever while waiting for connection.
                    // This is a blocking call waiting for connection.
                    clientSock = m_listener.Accept();

                    retry = 0;
                    try
                    {
                        // set NoDelay to increase HTTP(s) response times
                        clientSock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                    }
                    catch
                    {

                    }
                }
                catch (SocketException)
                {
                    if (retry > 5)
                    {
                        // If request to stop listener flag is set or locking call is interupted return
                        // On exception we stop the service and record the exception.
                        if (m_ServiceRunning && !m_Closed)
                        {
                            Stop();
                        }

                        // Set event to unblock thread waiting for accept.
                        m_RequestArrived.Set();

                        break;
                    }

                    retry++;
                    continue;
                }
                catch
                {
                    // If request to stop listener flag is set or locking call is interupted return
                    // On exception we stop the service and record the exception.
                    if (m_ServiceRunning && !m_Closed)
                    {
                        Stop();
                    }

                    // Set event to unblock thread waiting for accept.
                    m_RequestArrived.Set();

                    break;
                }

                try
                {
                    if (!m_IsHttpsConnection)
                    {
                        // This is case of normal HTTP. Create network stream.
                        netStream = new NetworkStream(clientSock, true);
                    }
                    else
                    {
                        // This is the case of https.
                        // Once connection estiblished need to create secure stream and authenticate server.
                        netStream = new SslStream(clientSock);

                        SslProtocols[] sslProtocols = new SslProtocols[] { SslProtocols.Default };

                        // Throws exception if fails.
                        ((SslStream)netStream).AuthenticateAsServer(m_httpsCert, SslVerification.NoVerification, sslProtocols);

                        netStream.ReadTimeout = 10000;
                    }
                }
                catch(SocketException)
                {
                    if (netStream != null)
                    {
                        netStream.Dispose();
                    }
                    else
                    {
                        clientSock.Close();
                    }

                    m_RequestArrived.Set();

                    // try again 
                    continue;
                }

                // Add this connected stream to the list.
                lock (m_InputStreamsQueue)
                {
                    m_InputStreamsQueue.Enqueue(new OutputNetworkStreamWrapper(clientSock, netStream));
                }

                // Set event that client stream or exception is added to the queue.
                m_RequestArrived.Set();
            }
        }

        /// <summary>
        /// Allows this instance to receive incoming requests.
        /// </summary>
        /// <remarks>This method must be called before you call the
        /// <see cref="System.Net.HttpListener.GetContext"/> method.   If
        /// the service was already started, the call has no effect.  After you
        /// have started an <itemref>HttpListener</itemref> object, you can use
        /// the <see cref='System.Net.HttpListener.Stop'/> method to stop it.
        /// </remarks>
        public void Start()
        {
            lock (this)
            {
                if (m_Closed) throw new ObjectDisposedException();
                
                // If service was already started, the call has no effect.
                if (m_ServiceRunning)
                {
                    return;
                }
                
                m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // set NoDelay to increase HTTP(s) response times
                    m_listener.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                }
                catch {}

                try
                {
                    // Start server socket to accept incoming connections.
                    m_listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                catch {}

                IPAddress addr;

                if(Microsoft.SPOT.Hardware.SystemInfo.IsEmulator)
                {
                    addr = IPAddress.Any;
                }
                else
                {
                    addr = IPAddress.GetDefaultLocalAddress();
                }

                IPEndPoint endPoint = new IPEndPoint(addr, m_Port);
                m_listener.Bind(endPoint);

                // Starts to listen to maximum of 10 connections.
                m_listener.Listen(MaxCountOfPendingConnections);

                // Create a thread that blocks on m_listener.Accept() - basically waits for connection from client.
                m_thAccept = new Thread(AcceptThreadFunc);
                m_thAccept.Start();

                // Waits for thread that calls Accept to start.
                m_RequestArrived.WaitOne();
            }
        }

        /// <summary>
        /// Shuts down the <itemref>HttpListener</itemref> after processing all
        /// currently queued requests.
        /// </summary>
        /// <remarks>After calling this method, you can no longer use the
        /// <itemref>HttpListener</itemref> object.  To temporarily pause an
        /// <itemref>HttpListener</itemref> object, use the
        /// <see cref='System.Net.HttpListener.Stop'/> method.</remarks>
        public void Close()
        {
            lock(this)
            {
                // close does not throw
                try
                {
                    Stop();
                }
                catch
                {
                }
                
                m_Closed = true;
            }
        }

        /// <summary>
        /// Causes this instance to stop receiving incoming requests.
        /// </summary>
        /// <remarks>If this instance is already stopped, calling this method
        /// has no effect.
        /// <para>
        /// After you have stopped an <itemref>HttpListener</itemref> object,
        /// you can use the <see cref='System.Net.HttpListener.Start'/> method
        /// to restart it.
        /// </para>
        /// </remarks>
        public void Stop()
        {   
            // Need to lock access to object, because Stop can be called from a
            // different thread.
            lock (this)
            {
                if (m_Closed) throw new ObjectDisposedException();
            
                m_ServiceRunning = false;
                
                // We close the server socket that listen for incoming connection.
                // Connections that already accepted are processed.
                // Connections that has been in queue for server socket, but not accepted, are lost.
                if(m_listener != null)
                {
                    m_listener.Close();
                    m_listener = null;

                    m_RequestArrived.Set();
                }
            }
        }

        /// <summary>
        /// Waits for an incoming request and returns when one is received.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Net.HttpListenerContext"/> object that
        /// represents a client request.
        /// </returns>
        /// <exception cref="SocketException">A socket call failed. Check the
        /// exception's ErrorCode property to determine the cause of the exception.</exception>
        /// <exception cref="InvalidOperationException">This object has not been started or is
        /// currently stopped or The HttpListener does not have any Uniform Resource Identifier
        /// (URI) prefixes to respond to.</exception>
        /// <exception cref="ObjectDisposedException">This object is closed.</exception>
        /// <example>This example shows how to call the
        /// <itemref>GetContext</itemref> method.
        /// <code>
        /// HttpListener myListener = new HttpListener("http", -1);
        /// myListener.Start();
        /// while (true)
        /// {
        ///     HttpListenerResponse response = null;
        ///     try
        ///     {
        ///         Debug.Print("Waiting for requests");
        ///         HttpListenerContext context = myListener.GetContext();
        /// </code>
        /// </example>
        public HttpListenerContext GetContext()
        {
            // Protects access for simulteneous call for GetContext and Close or Stop.
            lock (this)
            {
                if (m_Closed) throw new ObjectDisposedException();
            
                if (!m_ServiceRunning) throw new InvalidOperationException();
            }

            // Try to get context until service is running.
            while (m_ServiceRunning)
            {
                // Before waiting for event we need to look for pending connections.
                lock (m_InputStreamsQueue)
                {
                    if (m_InputStreamsQueue.Count > 0)
                    {
                        OutputNetworkStreamWrapper outputStreamWrap = m_InputStreamsQueue.Dequeue() as OutputNetworkStreamWrapper;
                        if (outputStreamWrap != null)
                        {
                            return new HttpListenerContext(outputStreamWrap, this);
                        }
                    }
                }

                // Waits for new connection to arrive on new or existing socket.
                m_RequestArrived.WaitOne();
            }

            return null;
        }

        /// <summary>
        /// Gets whether the <itemref>HttpListener</itemref> service was started
        /// and is waiting for client connections.
        /// </summary>
        /// <value><itemref>true</itemref> if the
        /// <itemref>HttpListener</itemref> was started; otherwise,
        /// <itemref>false</itemref>.</value>
        public bool IsListening
        {
             get { return m_ServiceRunning; }
        }

        /// <summary>
        /// Gets or sets the maximum allowed length of the response headers, in
        /// KB.
        /// </summary>
        /// <value>The length, in kilobytes (1024 bytes), of the response
        /// headers.</value>
        /// <remarks>
        /// The length of the response header includes the response status line
        /// and any extra control characters that are received as part of the
        /// HTTP protocol.  A value of -1 means no limit is imposed on the
        /// response headers; a value of 0 means that all requests fail.  If
        /// this property is not explicitly set, it defaults to 4 (KB).
        /// </remarks>
        public int MaximumResponseHeadersLength
        {
            get { return m_maxResponseHeadersLen; }
            set
            {
                if (value <= 0 && value != -1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                m_maxResponseHeadersLen = value;
            }
        }

        /// <summary>
        /// The certificate used if <b>HttpListener</b> implements an https
        /// server.
        /// </summary>
        public X509Certificate HttpsCert
        {
            get { return m_httpsCert; }
            set { m_httpsCert = value; }
        }
    }
}


