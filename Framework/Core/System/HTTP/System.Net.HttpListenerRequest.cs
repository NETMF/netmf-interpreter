////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Sockets;

    /// <summary>
    /// Describes an incoming HTTP request to an
    /// <see cref="System.Net.HttpListener"/> object.
    /// </summary>
    /// <remarks>
    /// When a client makes a request to a Uniform Resource Identifier (URI)
    /// handled by an <itemref>HttpListener</itemref> object, the
    /// <itemref>HttpListener</itemref> provides an
    /// <see cref="System.Net.HttpListenerContext"/> object that contains
    /// information about the sender, the request, and the response that is sent
    /// to the client. The <see cref="System.Net.HttpListenerContext.Request"/>
    /// property returns the <itemref>HttpListenerRequest</itemref> object that
    /// describes the request.
    /// <para>
    /// The <itemref>HttpListenerRequest</itemref> object contains information
    /// about the request, such as the request
    /// <see cref="System.Net.HttpListenerRequest.HttpMethod"/> string,
    /// <see cref="System.Net.HttpListenerRequest.UserAgent"/> string, and
    /// request body data (see the
    /// <see cref="System.Net.HttpListenerRequest.InputStream"/>
    /// property).</para>
    /// <para>
    /// To reply to the request, you must get the associated response using the
    /// <see cref="System.Net.HttpListenerContext.Response"/> property.</para>
    /// </remarks>
    public class HttpListenerRequest
    {
        /// <summary>
        /// The original string with the request.  For example,
        /// "GET /Index.htm HTTP/1.1".
        /// </summary>
        private string m_RequestString;

        /// <summary>
        /// The maximum length of the request headers, in KB (1024 bytes).
        /// </summary>
        private int m_maxResponseHeadersLen;

        /// <summary>
        /// The verb of the request parsed from m_RequestString.
        /// </summary>
        private string m_requestVerb;

        /// <summary>
        /// URL of request parsed from m_RequestString.
        /// </summary>
        private string m_rawURL;

        /// <summary>
        /// HTTP version from m_RequestString.
        /// </summary>
        private Version m_requestHttpVer;

        /// <summary>
        ///  Indicates whether the client requests a persistent connection.
        ///  If client did not specify "Connection" header - default is false.
        /// </summary>
        private bool m_KeepAlive = false;

        /// <summary>
        /// The request Headers From HTTP client.
        /// </summary>
        private WebHeaderCollection m_httpRequestHeaders = new WebHeaderCollection(true);

        /// <summary>
        /// Member with network stream connected to client.
        ///
        /// </summary>
        private InputNetworkStreamWrapper m_clientStream;

        /// <summary>
        /// The length of the content in the body of the request, if a body is
        /// present.
        /// </summary>
        long m_contentLength;

        /// <summary>
        /// Keep NetworkCredential if user have send user name and password.
        /// </summary>
        private NetworkCredential m_NetworkCredentials;

        /// <summary>
        /// Constructs a HttpListenerRequest is created by HttpListenerContext.
        /// </summary>
        /// <param name="clientStream">Network stream to the client.</param>
        /// <param name="maxHeaderLen">TBD</param>
        internal HttpListenerRequest(InputNetworkStreamWrapper clientStream, int maxHeaderLen)
        {
            m_clientStream = clientStream;

            // maxHeaderLen is in kilobytes (Desktop designer decided so). If -1 just maximum integer value
            m_maxResponseHeadersLen = maxHeaderLen == -1 ? 0x7FFFFFFF : maxHeaderLen * 1024;
            // If not set, default for content length is -1
            m_contentLength = -1;
        }

        public void Reset()
        {
            m_httpRequestHeaders = new WebHeaderCollection(true);
            m_contentLength = -1;
        }

        /// <summary>
        /// Parses request from client.
        /// Fills
        /// - HTTP Verb.
        /// - HTTP version.
        /// - Content Length.
        /// - Fills generic value name pair in WEB header collection.
        /// </summary>
        internal void ParseHTTPRequest()
        {
            // This is the request line.
            m_RequestString = m_clientStream.Read_HTTP_Line(HttpWebRequest.maxHTTPLineLength).Trim();

            // Split request line into 3 strings - VERB, URL and HTTP version.
            char[] delimiter = { ' ' };
            string[] requestStr = m_RequestString.Split(delimiter);
            // requestStr should consist of 3 parts.
            if (requestStr.Length < 3)
            {
                throw new ProtocolViolationException("Invalid HTTP request String: " + m_RequestString);
            }

            // We have at least 3 strings. Fills the proper fields
            m_requestVerb = requestStr[0];
            m_rawURL = requestStr[1];

            // Process third string. It should be either http/1.1 or http/1.0
            string httpVerLowerCase = requestStr[2].ToLower();
            if (httpVerLowerCase.Equals("http/1.1"))
            {
                m_requestHttpVer = HttpVersion.Version11;
            }
            else if (httpVerLowerCase.Equals("http/1.0"))
            {
                m_requestHttpVer = HttpVersion.Version10;
            }
            else
            {
                throw new ProtocolViolationException("Unsupported HTTP version: " + requestStr[2]);
            }

            // Now it is list of HTTP headers:
            string line;
            int headersLen = m_maxResponseHeadersLen;
            while ((line = m_clientStream.Read_HTTP_Header(HttpWebRequest.maxHTTPLineLength)).Length > 0)
            {
                // line.Length is used for the header. Substruct it.
                headersLen -= line.Length;
                // If total length used for header is exceeded, we break

                if (headersLen < 0)
                {
                    throw new ProtocolViolationException("Http Headers exceeding: " + m_maxResponseHeadersLen);
                }

                int sepIdx = line.IndexOf(':');
                if (sepIdx == -1)
                {
                    throw new ProtocolViolationException("Invalid HTTP Header: " + line);
                }

                string headerName = line.Substring(0, sepIdx).Trim();
                string headerValue = line.Substring(sepIdx + 1).Trim();
                string matchableHeaderName = headerName.ToLower();
                // Adds new header to collection.
                m_httpRequestHeaders.AddInternal(headerName, headerValue);

                // Now we check the value - name pair. For some of them we need to initilize member variables.
                headerName = headerName.ToLower();
                // If it is connection header
                if (headerName == "connection")
                {
                    // If value is "Keep-Alive" ( lower case now ), set m_KeepAlive to true;
                    headerValue = headerValue.ToLower();
                    m_KeepAlive = headerValue == "keep-alive";
                }

                // If user supplied user name and password - parse it and store in m_NetworkCredentials
                if (headerName == "authorization")
                {
                    int sepSpace = headerValue.IndexOf(' ');
                    string authType = headerValue.Substring(0, sepSpace);
                    if (authType.ToLower() == "basic")
                    {
                        string authInfo = headerValue.Substring(sepSpace + 1);
                        // authInfo is base64 encoded username and password.
                        byte[] authInfoDecoded = Convert.FromBase64String(authInfo);
                        char[] authInfoDecChar = System.Text.Encoding.UTF8.GetChars(authInfoDecoded);
                        string strAuthInfo = new string(authInfoDecChar);
                        // The strAuthInfo comes in format username:password. Parse it.
                        int sepColon = strAuthInfo.IndexOf(':');
                        if (sepColon != -1)
                        {
                            m_NetworkCredentials = new NetworkCredential(strAuthInfo.Substring(0, sepColon), strAuthInfo.Substring(sepColon + 1));
                        }
                    }
                }
            }

            // Http headers were processed. Now we search for content length.
            string strContentLen = m_httpRequestHeaders[HttpKnownHeaderNames.ContentLength];
            if (strContentLen != null)
            {
                try
                {
                    m_contentLength = Convert.ToInt32(strContentLen);
                }
                catch (Exception)
                {
                    throw new ProtocolViolationException("Invalid content length in request: " + strContentLen);
                }
            }
        }

        /// <summary>
        /// Gets the HTTP method specified by the client.
        /// </summary>
        /// <value>A <itemref>String</itemref> that contains the method used in
        /// the request.</value>
        public string HttpMethod
        {
            get { return m_requestVerb; }
        }

        /// <summary>
        /// Gets the URL information (without the host and port) requested by
        /// the client.
        /// </summary>
        /// <value>A <itemref>String</itemref> that contains the raw URL for
        /// this request.</value>
        /// <remarks>
        /// This URL information is the URL requested in the first request line.
        /// </remarks>
        public string RawUrl
        {
            get { return m_rawURL; }
        }

        /// <summary>
        /// Gets the MIME types accepted by the client.
        /// </summary>
        /// <value>A <itemref>String</itemref> array that contains the type
        /// names specified in the request's Accept header, or a null reference
        /// if the client request did not include an Accept header.</value>
        public string[] AcceptTypes
        {
            get { return m_httpRequestHeaders.GetValues(HttpKnownHeaderNames.Accept); }
        }

        /// <summary>
        /// Gets the length of the body data included in the request.
        /// </summary>
        /// <remarks>
        /// The Content-Length header expresses the length, in bytes, of the
        /// body data that accompanies the request.
        /// enumeration.
        /// </remarks>
        /// <value>The value from the request's Content-Length header. This
        /// value is -1 if the content length is not known.</value>
        public long ContentLength64
        {
            get { return m_contentLength; }
        }

        /// <summary>
        /// Gets the MIME type of the body data included in the request.
        /// </summary>
        /// <value>A <itemref>String</itemref> that contains the text of the
        /// request's Content-Type header.</value>
        public string ContentType
        {
            get { return m_httpRequestHeaders[HttpKnownHeaderNames.ContentType]; }
        }

        /// <summary>
        /// Gets the collection of header name/value pairs sent in the request.
        /// </summary>
        /// <value>A <itemref>WebHeaderCollection</itemref> that contains the
        /// HTTP headers included in the request.</value>
        public WebHeaderCollection Headers
        {
            get { return m_httpRequestHeaders; }
        }

        /// <summary>
        /// Gets a stream that contains the body data sent by the client.
        /// </summary>
        /// <value>A readable <itemref>Stream</itemref> object that contains the
        /// bytes sent by the client in the body of the request.  This property
        /// returns <itemref>Null</itemref> if no data is sent with the request.
        /// </value>
        public Stream InputStream
        {
            get { return m_clientStream; }
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the client sending this
        /// request is authenticated.
        /// </summary>
        /// <remarks>
        /// Because authentication is not supported, returns
        /// <itemref>false</itemref>.
        /// </remarks>
        /// <value>Because authentication is not supported, returns
        /// <itemref>false</itemref>.</value>
        public bool IsAuthenticated
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> value that indicates whether the
        /// client requests a persistent connection.
        /// </summary>
        /// <remarks>
        /// This property is set during parsing of HTTP header.
        /// </remarks>
        /// <value><itemref>true</itemref> if the connection should be kept
        /// open; otherwise, <itemref>false</itemref>.</value>
        public bool KeepAlive
        {
            get { return m_KeepAlive; }
        }

        /// <summary>
        /// Gets the server IP address and port number to which the request is
        /// directed.  Not currently supported.
        /// </summary>
        /// <value>An <itemref>IPEndPoint</itemref> that represents the IP
        /// address that the request is sent to.
        /// </value>
        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)m_clientStream.m_Socket.LocalEndPoint; }
        }

        /// <summary>
        /// Gets the HTTP version used by the requesting client.
        /// </summary>
        /// <remarks>
        /// The capabilities of different HTTP versions are specified in the
        /// documents available at http://www.rfc-editor.org.
        /// </remarks>
        /// <value>A <itemref>Version</itemref> that identifies the client's
        /// version of HTTP.</value>
        public Version ProtocolVersion
        {
            get { return m_requestHttpVer; }
        }

        /// <summary>
        /// Gets the client IP address and port number from which the request
        /// originated.
        /// </summary>
        /// <value>An <itemref>IPEndPoint</itemref> that represents the IP
        /// address and port number from which the request originated.</value>
        public IPEndPoint RemoteEndPoint
        {
            get { return (IPEndPoint)m_clientStream.m_Socket.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets the Uri object requested by the client.  Not currently
        /// supported.
        /// </summary>
        public Uri Url { get { return new Uri(m_rawURL, UriKind.Relative); } }

        /// <summary>
        /// Gets the user agent presented by the client.
        /// </summary>
        /// <value>A <itemref>String</itemref> object that contains the text of
        /// the request's User-Agent header.</value>
        /// <remarks>
        /// </remarks>
        public string UserAgent
        {
            get { return m_httpRequestHeaders[HttpKnownHeaderNames.UserAgent]; }
        }

        /// <summary>
        /// Gets the server IP address and port number to which the request is
        /// directed.
        /// </summary>
        /// <value>A <itemref>String</itemref> that contains the host address
        /// information.</value>
        public string UserHostAddress
        {
            get { return ((IPEndPoint)m_clientStream.m_Socket.LocalEndPoint).Address.ToString(); }
        }

        /// <summary>
        /// Gets the DNS name and, if provided, the port number specified by the
        /// client.
        /// </summary>
        /// <value>A String value that contains the text of the request's Host header.</value>
        public string UserHostName
        {
            get { return m_httpRequestHeaders[HttpKnownHeaderNames.UserAgent]; }
        }

        /// <summary>
        /// Return NetworkCredential if user have send user name and password.
        /// </summary>
        public NetworkCredential Credentials
        {
            get { return m_NetworkCredentials; }
        }

        /// <summary>
        /// Gets the natural languages that are preferred for the response.
        /// </summary>
        /// <value>A <itemref>String</itemref> array that contains the languages
        /// specified in the request's <itemref>AcceptLanguage</itemref> header,
        /// or <itemref>null</itemref> if the client request did not include an
        /// <itemref>AcceptLanguage</itemref> header.</value>
        public string[] UserLanguages
        {
            get { return m_httpRequestHeaders.GetValues(HttpKnownHeaderNames.AcceptLanguage); }
        }

    }
}


