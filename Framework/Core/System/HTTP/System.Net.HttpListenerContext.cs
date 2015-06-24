////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Collections;

    /// <summary>
    /// Provides access to the request and response objects used by the
    /// <itemref>HttpListener</itemref> class.  This class cannot be inherited.
    /// </summary>
    public class HttpListenerContext
    {
        /// <summary>
        /// A flag that indicates whether an HTTP request was parsed.
        /// </summary>
        /// <remarks>
        /// The HTTP request is parsed upon the first access to the Request to
        /// Response property.  Access to that property might be done from a
        /// different thread than the thread that is used for construction of
        /// the HttpListenerContext.
        /// </remarks>
        bool m_IsHTTPRequestParsed;

        /// <summary>
        /// Member with network stream connected to client.
        /// This stream is used for writing data.
        /// This stream owns the socket.
        /// </summary>
        private OutputNetworkStreamWrapper m_clientOutputStream;

        /// <summary>
        /// Member with network stream connected to client.
        /// This stream is used for Reading data.
        /// This stream does not own the socket.
        /// </summary>
        private InputNetworkStreamWrapper m_clientInputStream;

        /// <summary>
        /// Instance of the request from client.
        /// it is a server side representation of HttpWebRequest.
        /// It is the same data, but instead of composing request we parse it.
        /// </summary>
        private HttpListenerRequest m_ClientRequest;

        /// <summary>
        /// Instance of the response to client.
        ///
        /// </summary>
        private HttpListenerResponse m_ResponseToClient;

        /// <summary>
        /// Internal constructor, used each time client connects.
        /// </summary>
        /// <param name="clientStream">The stream that is connected to the client. A stream is needed, to
        /// provide information about the connected client.
        /// See also the <see cref="System.Net.HttpListenerRequest"/> class.
        /// </param>
        /// <param name="httpListener">TBD</param>
        internal HttpListenerContext(OutputNetworkStreamWrapper clientStream, HttpListener httpListener)
        {
            // Saves the stream.
            m_clientOutputStream = clientStream;

            // Input stream does not own socket.
            m_clientInputStream = new InputNetworkStreamWrapper(clientStream.m_Stream, clientStream.m_Socket, false, null);

            // Constructs request and response classes.
            m_ClientRequest = new HttpListenerRequest(m_clientInputStream, httpListener.m_maxResponseHeadersLen);

            // Closing reponse to client causes removal from clientSocketsList.
            // Thus we need to pass clientSocketsList to client response.
            m_ResponseToClient = new HttpListenerResponse(m_clientOutputStream, httpListener);

            // There is incoming connection HTTP connection. Add new Socket to the list of connected sockets
            // The socket is removed from this array after correponding HttpListenerResponse is closed.
            httpListener.AddClientStream(m_clientOutputStream);

            // Set flag that HTTP request was not parsed yet.
            // It will be parsed on first access to m_ClientRequest or m_ResponseToClient
            m_IsHTTPRequestParsed = false;
        }

        public void Reset()
        {
            m_IsHTTPRequestParsed = false;
            m_ClientRequest.Reset();
        }

        /// <summary>
        /// Gets the <itemref>HttpListenerRequest</itemref> that represents a
        /// client's request for a resource.
        /// </summary>
        /// <value>An <itemref>HttpListenerRequest</itemref> object that
        /// represents the client request.</value>
        public HttpListenerRequest Request
        {
            get
            {
                if (!m_IsHTTPRequestParsed)
                {
                    m_ClientRequest.ParseHTTPRequest();
                    // After request parsed check for "transfer-ecoding" header. If it is chunked, change stream property.
                    // If m_EnableChunkedDecoding is set to true, then readig from stream automatically processing chunks.
                    string chunkedVal = m_ClientRequest.Headers[HttpKnownHeaderNames.TransferEncoding];
                    if (chunkedVal != null && chunkedVal.ToLower() == "chunked")
                    {
                        m_clientInputStream.m_EnableChunkedDecoding = true;
                    }

                    m_IsHTTPRequestParsed = true;
                }

                return m_ClientRequest;
            }
        }

        /// <summary>
        /// Gets the <itemref>HttpListenerResponse</itemref> object that will be
        /// sent to the client in response to the client's request.
        /// </summary>
        /// <value>An <itemref>HttpListenerResponse</itemref> object used to
        /// send a response back to the client.</value>
        public HttpListenerResponse Response
        {
            get
            {
                if (!m_IsHTTPRequestParsed)
                {
                    m_ClientRequest.ParseHTTPRequest();
                    m_IsHTTPRequestParsed = true;
                }

                return m_ResponseToClient;
            }
        }

        public void Close()
        {
            Close(-2);
        }

        /// <summary>
        /// Closes the stream attached to this listener context. 
        /// </summary>
        public void Close(int lingerValue)
        {
            try
            {  
                if (m_clientOutputStream != null)
                {
                    try
                    {
                        if(m_clientOutputStream.m_Socket != null)
                        {
                            m_clientOutputStream.m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerValue);
                        }
                    }
                    catch{}
                }
                
                if (m_ResponseToClient != null)
                {
                    m_ResponseToClient.Close();
                    m_ResponseToClient = null;
                }
                
                // Close the underlying stream
                if (m_clientOutputStream != null)
                {
                    m_clientOutputStream.Dispose();
                    m_clientOutputStream = null;
                }
                
                if (m_clientInputStream != null)
                {
                    m_clientInputStream.Dispose();
                    m_clientInputStream = null;
                }
            }
            catch
            {
            }
        }
    }
}


