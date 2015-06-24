////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Net.Sockets;
    using System.Collections;

    /// <summary>
    /// Represents a response to a request being handled by an
    /// <see cref="System.Net.HttpListener"/> object.
    /// </summary>
    public sealed class HttpListenerResponse : IDisposable
    {
        /// <summary>
        /// A flag that indicates whether the response was already sent.
        /// </summary>
        private bool m_WasResponseSent = false;

        /// <summary>
        /// A flag that indicates that the response was closed.
        /// Writing to client is not allowed after response is closed.
        /// </summary>
        private bool m_IsResponseClosed = false;

        /// <summary>
        /// The length of the content of the response.
        /// </summary>
        long m_ContentLength = -1;

        /// <summary>
        /// The response headers from the HTTP client.
        /// </summary>
        private WebHeaderCollection m_httpResponseHeaders = new WebHeaderCollection(true);

        /// <summary>
        /// The HTTP version for the response.
        /// </summary>
        private Version m_version = new Version(1, 1);

        /// <summary>
        /// Indicates whether the server requests a persistent connection.
        /// Persistent connection is used if KeepAlive is <itemref>true</itemref>
        /// in both the request and the response.
        /// </summary>
        private bool m_KeepAlive = false;

        /// <summary>
        /// Encoding for this response's OutputStream.
        /// </summary>
        private Encoding m_Encoding = Encoding.UTF8;

        /// <summary>
        /// Keeps content type for the response, set by user application.
        /// </summary>
        private string m_contentType;

        /// <summary>
        /// Response status code.
        /// </summary>
        private int m_ResponseStatusCode = (int)HttpStatusCode.OK;

        /// <summary>
        /// Array of connected client streams
        /// </summary>
        private HttpListener m_Listener;

        /// <summary>
        /// Member with network stream connected to client.
        /// After call to Close() the stream is closed, no further writing allowed.
        /// </summary>
        private OutputNetworkStreamWrapper m_clientStream;

        /// <summary>
        /// The value of the HTTP Location header in this response.
        /// </summary>
        private string m_redirectLocation;

        /// <summary>
        /// Response uses chunked transfer encoding.
        /// </summary>
        private bool m_sendChunked = false;

        /// <summary>
        /// text description of the HTTP status code returned to the client.
        /// </summary>
        private string m_statusDescription;

        /// <summary>
        /// Throws InvalidOperationException is HTTP response was sent.
        /// Called before setting of properties.
        /// </summary>
        private void ThrowIfResponseSent()
        {
            if (m_WasResponseSent)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// HttpListenerResponse is created by HttpListenerContext
        /// </summary>
        /// <param name="clientStream">Network stream to the client</param>
        /// <param name="httpListener">TBD</param>
        internal HttpListenerResponse(OutputNetworkStreamWrapper clientStream, HttpListener httpListener)
        {
            // Sets the delegate, so SendHeaders will be called on first write.
            clientStream.HeadersDelegate = new OutputNetworkStreamWrapper.SendHeadersDelegate(SendHeaders);
            // Saves network stream as member.
            m_clientStream = clientStream;
            // Saves list of client streams. m_clientStream is removed from clientStreamsList during Close().
            m_Listener = httpListener;
        }

        /// <summary>
        /// Updates the HTTP WEB header collection to prepare it for request.
        /// For each property set it adds member to m_httpResponseHeaders.
        /// m_httpResponseHeaders is serializes to string and sent to client.
        /// </summary>
        private void PrepareHeaders()
        {
            // Adds content length if it was present.
            if (m_ContentLength != -1)
            {
                m_httpResponseHeaders.ChangeInternal(HttpKnownHeaderNames.ContentLength, m_ContentLength.ToString());
            }

            // Since we do not support persistent connection, send close always.
            string connection = m_KeepAlive ? "Keep-Alive" : "Close";
            m_httpResponseHeaders.ChangeInternal(HttpKnownHeaderNames.Connection, connection);

            // Adds content type if user set it:
            if (m_contentType != null)
            {
                m_httpResponseHeaders.AddWithoutValidate(HttpKnownHeaderNames.ContentType, m_contentType);
            }

            if (m_redirectLocation != null)
            {
                m_httpResponseHeaders.AddWithoutValidate(HttpKnownHeaderNames.Location, m_redirectLocation);
                m_ResponseStatusCode = (int)HttpStatusCode.Redirect;
            }
        }

        /// <summary>
        /// Composes HTTP response line based on
        /// </summary>
        /// <returns></returns>
        private string ComposeHTTPResponse()
        {
            // Starts with HTTP
            string resp = "HTTP/";
            // Adds version of HTTP
            resp += m_version.ToString();
            // Add status code.
            resp += " " + m_ResponseStatusCode;
            // Adds description
            if (m_statusDescription == null)
            {
                resp += " " + GetStatusDescription(m_ResponseStatusCode);
            }
            else // User provided description is present.
            {
                resp += " " + m_statusDescription;
            }

            // Add line termindation.
            resp += "\r\n";
            return resp;
        }

        /// <summary>
        /// Sends HTTP status and headers to client.
        /// </summary>
        private void SendHeaders()
        {
            // As first step we disable the callback to SendHeaders, so m_clientStream.Write would not call
            // SendHeaders() again.
            m_clientStream.HeadersDelegate = null;

            // Creates encoder, generates headers and sends the data.
            Encoding encoder = Encoding.UTF8;

            byte[] statusLine = encoder.GetBytes(ComposeHTTPResponse());
            m_clientStream.Write(statusLine, 0, statusLine.Length);

            // Prepares/Updates WEB header collection.
            PrepareHeaders();

            // Serialise WEB header collection to byte array.
            byte[] pHeaders = m_httpResponseHeaders.ToByteArray();

            // Sends the headers
            m_clientStream.Write(pHeaders, 0, pHeaders.Length);

            m_WasResponseSent = true;
        }

        /// <summary>
        /// Gets or sets the HTTP status code to be returned to the client.
        /// </summary>
        /// <value>An <itemref>Int32</itemref> value that specifies the
        /// <see cref="System.Net.HttpStatusCode"/> for the requested resource.
        /// The default is <itemref>OK</itemref>, indicating that the server
        /// successfully processed the client's request and included the
        /// requested resource in the response body.</value>
        public int StatusCode
        {
            get { return m_ResponseStatusCode; }
            set
            {
                ThrowIfResponseSent();
                m_ResponseStatusCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes in the body data included in the
        /// response.
        /// </summary>
        /// <value>The value of the response's <itemref>Content-Length</itemref>
        /// header.</value>
        public long ContentLength64
        {
            get { return m_ContentLength; }

            set
            {
                ThrowIfResponseSent();
                m_ContentLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the collection of header name/value pairs that is
        /// returned by the server.
        /// </summary>
        /// <value>A <itemref>WebHeaderCollection</itemref> instance that
        /// contains all the explicitly set HTTP headers to be included in the
        /// response.</value>
        public WebHeaderCollection Headers
        {
            get { return m_httpResponseHeaders; }
            set
            {
                ThrowIfResponseSent();
                m_httpResponseHeaders = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the server requests a persistent connection.
        /// </summary>
        /// <value><itemref>true</itemref> if the server requests a persistent
        /// connection; otherwise, <itemref>false</itemref>.  The default is
        /// <itemref>true</itemref>.</value>
        public bool KeepAlive
        {
            get { return m_KeepAlive; }

            set { m_KeepAlive = value; }
        }

        /// <summary>
        /// Gets a <itemref>Stream</itemref> object to which a response can be
        /// written.
        /// </summary>
        /// <value>A <itemref>Stream</itemref> object to which a response can be
        /// written.</value>
        /// <remarks>
        /// The first write to the output stream sends a response to the client.
        /// </remarks>
        public Stream OutputStream
        {
            get
            {
                if (m_IsResponseClosed)
                {
                    throw new ObjectDisposedException("Response has been sent");
                }

                return m_clientStream;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP version that is used for the response.
        /// </summary>
        /// <value>A <itemref>Version</itemref> object indicating the version of
        /// HTTP used when responding to the client.  This property is obsolete.
        /// </value>
        public Version ProtocolVersion
        {
            get { return m_version; }
            set
            {
                ThrowIfResponseSent();
                m_version = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the HTTP <itemref>Location</itemref>
        /// header in this response.
        /// </summary>
        /// <value>A <itemref>String</itemref> that contains the absolute URL to
        /// be sent to the client in the <itemref>Location</itemref> header.
        /// </value>
        public string RedirectLocation
        {
            get { return m_redirectLocation; }
            set
            {
                ThrowIfResponseSent();
                m_redirectLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the response uses chunked transfer encoding.
        /// </summary>
        /// <value><itemref>true</itemref> if the response is set to use chunked
        /// transfer encoding; otherwise, <itemref>false</itemref>.  The default
        /// is <itemref>false</itemref>.</value>
        public bool SendChunked
        {
            get { return m_sendChunked; }
            set
            {
                ThrowIfResponseSent();
                m_sendChunked = value;
            }
        }

        /// <summary>
        /// Gets or sets the encoding for this response's
        /// <itemref>OutputStream</itemref>.
        /// </summary>
        /// <value>An <itemref>Encoding</itemref> object suitable for use with
        /// the data in the
        /// <see cref="System.Net.HttpListenerResponse.OutputStream"/> property,
        /// or <itemref>null</itemref> reference if no encoding is specified.
        /// </value>
        /// <remarks>
        /// Only UTF8 encoding is supported.
        /// </remarks>
        public Encoding ContentEncoding
        {
            get { return m_Encoding; }
            set
            {
                ThrowIfResponseSent();
                m_Encoding = value;
            }
        }

        /// <summary>
        /// Gets or sets the MIME type of the returned content.
        /// </summary>
        /// <value>A <itemref>String</itemref> instance that contains the text
        /// of the response's <itemref>Content-Type</itemref> header.</value>
        public string ContentType
        {
            get { return m_contentType; }
            set
            {
                ThrowIfResponseSent();
                m_contentType = value;
            }
        }

        /// <summary>
        /// Gets or sets a text description of the HTTP status code that is
        /// returned to the client.
        /// </summary>
        /// <value>The text description of the HTTP status code returned to the
        /// client.</value>
        public string StatusDescription
        {
            get { return m_statusDescription; }
            set
            {
                ThrowIfResponseSent();
                m_statusDescription = value;
            }
        }

        public void Detach()
        {
            if (!m_IsResponseClosed)
            {
                if (!m_WasResponseSent)
                {
                    SendHeaders();
                }

                m_IsResponseClosed = true;
            }
        }

        /// <summary>
        /// Sends the response to the client and releases the resources held by
        /// this HttpListenerResponse instance.
        /// </summary>
        /// <remarks>
        /// This method flushes data to the client and closes the network
        /// connection.
        /// </remarks>
        public void Close()
        {
            if (!m_IsResponseClosed)
            {
                try
                {
                    if (!m_WasResponseSent)
                    {
                        SendHeaders();
                    }
                }
                finally
                {
                    // Removes from the list of streams and closes the socket.
                    ((IDisposable)this).Dispose();
                }
            }
        }

        /// <summary>
        /// Closes the socket and sends the response if it was not done earlier
        /// and the socket is present.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (!m_IsResponseClosed)
            {
                try
                {
                    // Iterates over list of client connections and remove its stream from it.
                    m_Listener.RemoveClientStream(m_clientStream);

                    m_clientStream.Flush();

                    // If KeepAlive is true,
                    if (m_KeepAlive)
                    {   // Then socket is tramsferred to the list of waiting for new data.
                        m_Listener.AddToWaitingConnections(m_clientStream);
                    }
                    else  // If not KeepAlive then close
                    {
                        m_clientStream.Dispose();
                    }
                }
                catch{}

                m_IsResponseClosed = true;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called to close the socket if necessary.
        /// </summary>
        ~HttpListenerResponse()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Return default Description based in response status code.
        /// </summary>
        /// <param name="code">HTTP status code</param>
        /// <returns>
        /// Default string with description.
        /// </returns>
        internal static string GetStatusDescription(int code)
        {
            switch (code)
            {
                case 100: return "Continue";
                case 101: return "Switching Protocols";
                case 102: return "Processing";
                case 200: return "OK";
                case 201: return "Created";
                case 202: return "Accepted";
                case 203: return "Non-Authoritative Information";
                case 204: return "No Content";
                case 205: return "Reset Content";
                case 206: return "Partial Content";
                case 207: return "Multi-Status";
                case 300: return "Multiple Choices";
                case 301: return "Moved Permanently";
                case 302: return "Found";
                case 303: return "See Other";
                case 304: return "Not Modified";
                case 305: return "Use Proxy";
                case 307: return "Temporary Redirect";
                case 400: return "Bad Request";
                case 401: return "Unauthorized";
                case 402: return "Payment Required";
                case 403: return "Forbidden";
                case 404: return "Not Found";
                case 405: return "Method Not Allowed";
                case 406: return "Not Acceptable";
                case 407: return "Proxy Authentication Required";
                case 408: return "Request Timeout";
                case 409: return "Conflict";
                case 410: return "Gone";
                case 411: return "Length Required";
                case 412: return "Precondition Failed";
                case 413: return "Request Entity Too Large";
                case 414: return "Request-Uri Too Long";
                case 415: return "Unsupported Media Type";
                case 416: return "Requested Range Not Satisfiable";
                case 417: return "Expectation Failed";
                case 422: return "Unprocessable Entity";
                case 423: return "Locked";
                case 424: return "Failed Dependency";
                case 500: return "Internal Server Error";
                case 501: return "Not Implemented";
                case 502: return "Bad Gateway";
                case 503: return "Service Unavailable";
                case 504: return "Gateway Timeout";
                case 505: return "Http Version Not Supported";
                case 507: return "Insufficient Storage";
            }

            return "";
        }

    }
}


