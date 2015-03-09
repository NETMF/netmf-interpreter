////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// This is the class that we use to create HTTP and requests.
    /// Used to register prefix "http" with WEB Request class.
    /// </summary>
    internal class HttpRequestCreator : IWebRequestCreate
    {
        internal HttpRequestCreator()
        {
        }

        /// <summary>
        /// Creates an HttpWebRequest. We register
        /// for HTTP and HTTPS URLs, and this method is called when a request
        /// needs to be created for one of those.
        /// </summary>
        /// <param name="Url">Url for request being created.</param>
        /// <returns>The newly created HttpWebRequest.</returns>
        public WebRequest Create(Uri Url)
        {

            return new HttpWebRequest(Url);
        }

    } // class HttpRequestCreator

    /// <summary>
    /// Provides an HTTP-specific implementation of the <see cref="System.Net.WebRequest"/> class.
    /// </summary>
    /// <remarks>This class does the main work of the request: it collects the header information
    /// from the user, exposes the Stream for outgoing entity data, and processes the incoming
    /// request.</remarks>
    public class HttpWebRequest : WebRequest
    {

        /// <summary>
        /// Array list of connected streams.
        /// This is static list, keeps all "stay live" sockets.
        /// </summary>
        internal static ArrayList m_ConnectedStreams;

        /// <summary>
        /// Timer that checks on open connections and closes them if they are
        /// idle for a long time.
        /// </summary>
        static Timer m_DropOldConnectionsTimer;

        /// <summary>
        /// If a response was created then Dispose on the Request will not dispose the underlying stream.
        /// </summary>
        private bool m_responseCreated;

        /// <summary>
        /// Timer callback. Called periodically and closes all connections that
        /// are idle for long time.
        /// </summary>
        /// <param name="unused">Unused</param>
        static private void CheckPersistentConnections(object unused)
        {
            int count = m_ConnectedStreams.Count;
            // The fastest way to exit out - if there are no sockets in the list - exit out.
            if (count > 0)
            {
                DateTime curTime = DateTime.Now;
                lock (m_ConnectedStreams)
                {
                    count = m_ConnectedStreams.Count;

                    for (int i = count-1; i >= 0; i--)
                    {
                        InputNetworkStreamWrapper streamWrapper = (InputNetworkStreamWrapper)m_ConnectedStreams[i];

                        TimeSpan timePassed = streamWrapper.m_lastUsed - curTime;

                        // If the socket is old, then close and remove from the list.
                        if (timePassed.Milliseconds > HttpListener.DefaultKeepAliveMilliseconds)
                        {
                            m_ConnectedStreams.RemoveAt(i);
                            
                            // Closes the socket to release resources.
                            streamWrapper.Dispose();
                        }
                    }

                    // turn off the timer if there are no active streams
                    if(m_ConnectedStreams.Count > 0)
                    {
                        m_DropOldConnectionsTimer.Change( HttpListener.DefaultKeepAliveMilliseconds, System.Threading.Timeout.Infinite );
                    }
                }
            }
        }

        /// <summary>
        /// Registers <itemref>HttpRequestCreator</itemref> as the creator for the "http" prefix.
        /// </summary>
        static HttpWebRequest()
        {
            // Creates instance of HttpRequestCreator. HttpRequestCreator creates HttpWebRequest
            HttpRequestCreator Creator = new HttpRequestCreator();
            // Register prefix. HttpWebRequest handles both http and https
            RegisterPrefix("http:", Creator);
            RegisterPrefix("https:", Creator);
            if (m_ConnectedStreams == null)
            {
                // Creates new list for connected sockets.
                m_ConnectedStreams = new ArrayList();
                m_DropOldConnectionsTimer = new Timer(CheckPersistentConnections, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
        }

        /// <summary>
        /// Closes a response stream, if present.
        /// </summary>
        /// <param name="disposing">Not used.</param>
        protected override void Dispose(bool disposing)
        {
            if(m_requestStream != null)
            {
                if(!m_responseCreated)
                {
                    RemoveStreamFromPool(m_requestStream);

                    m_requestStream.Dispose();
                }
            }

            base.Dispose(disposing);
        }
        

        /// <summary>
        /// The length in KB of the default maximum for response headers
        /// received.
        /// </summary>
        /// <remarks>
        /// The default configuration file sets this value to 4 kilobytes.
        /// </remarks>
        static int defaultMaxResponseHeadersLen = 4;

        /// <summary>
        ///  Default delay on the Stream.Read and Stream.Write operations
        /// </summary>
        private const int DefaultReadWriteTimeout = 5 * 60 * 1000; // 5 minutes

        /// <summary>
        ///  maximum length of the line in reponse line 
        /// </summary>
        internal const int maxHTTPLineLength = 4000;

        /// <summary>
        ///  Delegate that can be called on Continue Response
        /// </summary>
        private HttpContinueDelegate m_continueDelegate;

        /// <summary>
        /// HTTP verb.
        /// </summary>
        private string m_method;

        /// <summary>
        /// The Headers for the HTTP request.
        /// </summary>
        private WebHeaderCollection m_httpRequestHeaders;

        /// <summary>
        /// Controls how writes are handled.
        /// </summary>
        private HttpWriteMode m_httpWriteMode;

        /// <summary>
        /// The URI that we do the request for.
        /// </summary>
        private Uri m_originalUrl;

        /// <summary>
        /// Content length of the request message on POST.
        /// </summary>
        private long m_contentLength;

        /// <summary>
        /// The HTTP version for this request.
        /// </summary>
        private Version m_version;

        /// <summary>
        /// Timeout for Read And Write on the Stream that we return through
        /// GetResponse().GetResponseStream() and GetRequestStream()
        /// </summary>
        private int m_readWriteTimeout;

        /// <summary>
        /// Proxy to use for connection.
        /// </summary>
        private IWebProxy m_proxy;

        /// <summary>
        /// Whether to use persistent connections.
        /// </summary>
        private bool m_keepAlive;

        /// <summary>
        /// An array of certificates used to verify servers that support https.
        /// </summary>
        /// <remarks>
        /// The client application sets these certificates to the
        /// <b>HttpWebRequest</b>.  When the server certificate is received, it
        /// is validated with certificates in this array.
        /// </remarks>
        X509Certificate[] m_caCerts;

        /// <summary>
        /// The number of people using the connection.  Must reference-count this
        /// stuff.  Except reference counting is apparently insufficient.  I'm going to flag each section
        /// that uses the parser with a constant, and twiddle the flags for
        /// adding and removing connections.
        /// </summary>
        internal const int k_noConnection = 0x0;
        private const int k_parserFlag = 0x1;
        private const int k_writeStreamFlag = 0x2;
        private const int k_readStreamFlag = 0x4;
        private const int k_abortFlag = 0x8;
        internal int m_connectionUsers = 0;

        /// <summary>
        /// Static instance of decoder to convert received bytes from network
        /// stream into text of the response line and WEB headers.
        /// </summary>
        static private Decoder UTF8decoder = System.Text.Encoding.UTF8.GetDecoder();

        /// <summary>
        /// Invalid characters that cannot be found in a valid method-verb.
        /// </summary>
        private static readonly char[] k_invalidMethodChars =
        new char[]{' ',
            '\r',
            '\t',
            '\n'};

        /// <summary>
        /// The maximum length, in kilobytes (1024 bytes), of the response
        /// headers.
        /// </summary>
        private int m_maxResponseHeadersLen = defaultMaxResponseHeadersLen;

        /// <summary>
        /// The response from the server.
        /// </summary>
        private HttpStatusCode m_responseStatus = (HttpStatusCode)0;

        /// <summary>
        /// true if we have a response, or a transport error while constructing the response
        /// </summary>
        private bool m_responseComplete = false;

        /// <summary>
        /// This is non-null if there was an error.  If this is true, then there is no valid HttpWebResponse.
        /// </summary>
        private WebException m_errorResponse = null;

        /// <summary>
        /// Buffer size for reading from the server
        /// </summary>
        private const int k_readBlockLength = 2048;

        /// <summary>
        /// This is the maximum amount of data which can be buffered at any time
        /// and have a failed match.  In other words, if we receive this much
        /// data, and can't parse it in any useful way, assume an error.
        /// </summary>
        private const int k_maximumBufferSize = 8192;

        /// <summary>
        /// True if the request has been started, false otherwise.  Disables
        /// setting of many header properties.
        /// </summary>
        private bool m_requestSent;

        /// <summary>
        /// This is the request stream, if it has been created.
        /// </summary>
        private InputNetworkStreamWrapper m_requestStream;

        /// <summary>
        /// Whether or not data should be buffered when sent.
        /// Data is always buffered though (given redirects and stuff).
        /// </summary>
        private bool m_allowWriteStreamBuffering;

        /// <summary>
        /// The timeout value for this request.
        /// </summary>
        private int m_timeout;

        /// <summary>
        /// Keep NetworkCredential if user have send user name and password.
        /// </summary>
        private NetworkCredential m_NetworkCredentials;

        /// <summary>
        /// Gets or sets the timeout value in milliseconds for the
        /// <see cref="System.Net.HttpWebRequest.GetResponse"/> and
        /// <see cref="System.Net.HttpWebRequest.GetRequestStream"/> methods.
        /// </summary>
        /// <value>The number of milliseconds to wait before the request times
        /// out.  The default is 100,000 milliseconds (100 seconds).</value>
        /// <remarks>
        /// Overrides the <see cref="System.Net.WebRequest.Timeout"/> property
        /// of <itemref>WebRequest</itemref>.</remarks>
        public override int Timeout
        {
            get
            {
                return m_timeout;
            }

            set
            {
                if (value < 0 && value != System.Threading.Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                m_timeout = value;
            }
        }

        /// <summary>
        /// Set or Get NetworkCredential if user have send user name and password.
        /// </summary>
        public NetworkCredential Credentials
        {
            get { return m_NetworkCredentials; }
            set { m_NetworkCredentials = value; }
        }

        /// <summary>
        /// Gets or sets the array of certificates used to authenticate https
        /// servers.  These certificates are used only for https connections;
        /// http connections do not require them.
        /// </summary>
        public X509Certificate[] HttpsAuthentCerts
        {
            get { return m_caCerts; }
            set { m_caCerts = value; }
        }

        /// <summary>
        /// Gets or sets a timeout in milliseconds when writing to or reading
        /// from a stream.
        /// </summary>
        /// <value>The number of milliseconds before the writing or reading
        /// times out.  The default value is 300,000 milliseconds (5 minutes).
        /// </value>
        /// <remarks>This property is used to control the timeout when calling
        /// <see cref="System.IO.Stream.Read"/> and <see cref="System.IO.Stream.Write"/>.
        /// This property affects <itemref>Stream</itemref>s returned from
        /// GetResponse().<see cref="System.Net.WebResponse.GetResponseStream"/>()
        /// and
        /// GetResponse().<see cref="System.Net.HttpWebRequest.GetRequestStream"/>().
        /// </remarks>
        public int ReadWriteTimeout
        {
            get
            {
                return m_readWriteTimeout;
            }

            set
            {
                // we can't change timeouts after the request has been sent
                if (m_requestSent)
                    throw new InvalidOperationException("Cannot change timeout after request submitted ");
                if (value <= 0 && value != System.Threading.Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                m_readWriteTimeout = value;
            }
        }

        /// <summary>
        /// The HTTP status code returned by the server.
        /// </summary>
        internal HttpStatusCode ResponseStatusCode
        {
            get
            {
                return m_responseStatus;
            }
        }

        /// <summary>
        /// Return if error is present in response.
        /// </summary>
        /// <returns>true if error happened, false otherwise</returns>
        internal bool hasError()
        {
            return m_errorResponse != null;
        }

        /// <summary>
        /// Gets the original Uniform Resource Identifier (URI) of the request.
        /// </summary>
        /// <remarks>
        /// The URI object was created by the constructor and is always
        /// non-null.  The URI object will always be the base URI, because
        /// automatic re-directs aren't supported.
        /// </remarks>
        /// <value>A Uri that contains the URI of the Internet resource passed
        /// to the WebRequest.<see cref="System.Net.WebRequest.Create(Uri)"/> method.
        /// </value>
        public override Uri RequestUri
        {
            get
            {
                return m_originalUrl;
            }
        }

        /// <summary>
        /// Gets the URI for this request.
        /// </summary>
        /// <value>A <itemref>Uri</itemref> that identifies the Internet
        /// resource that actually responds to the request.  The default is the
        /// URI used by the
        /// WebRequest.<see cref="System.Net.WebRequest.Create(Uri)"/> method to
        /// initialize the request.
        /// </value>
        /// <remarks>
        /// This value is always the same as the
        /// <see cref="System.Net.HttpWebRequest.RequestUri"/>
        /// property, because automatic re-direction isn't supported.
        /// </remarks>
        public Uri Address
        {
            get
            {
                return m_originalUrl;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to buffer the data sent
        /// to the Internet resource.
        /// </summary>
        /// <value><itemref>true</itemref> to enable buffering of the data sent
        /// to the Internet resource; <b>false</b> to disable buffering.  The
        /// default is <b>true</b>.</value>
        public bool AllowWriteStreamBuffering
        {

            get
            {
                return m_allowWriteStreamBuffering;
            }

            set
            {
                m_allowWriteStreamBuffering = value;
            }
        }

        /// <summary>
        /// Gets or sets the <b>Content-Length</b> of the request entity body.
        /// </summary>
        /// <value>The number of bytes of data to send to the Internet resource.
        /// The default is -1, which indicates the property has not been set and
        /// that there is no request data to send.</value>
        /// <remarks>
        /// Getting this property returns the last value set, or -1 if no value
        /// has been set.  Setting it sets the content length, and the
        /// application must write that much data to the stream.  This property
        /// interacts with
        /// <b>HttpWebRequest</b>.<see cref="System.Net.HttpWebRequest.SendChunked"/>.
        /// </remarks>
        public override long ContentLength
        {
            get
            {
                return m_contentLength;
            }

            set
            {
                //no race.  Don't need interlocked
                if (true == m_requestSent)
                    throw new InvalidOperationException();

                if (value < 0)
                    throw new ArgumentOutOfRangeException("Content length cannot be negative: " + value);

                m_contentLength = value;
                //if a content length is set, then we cannot send chunked data.
                m_httpWriteMode = HttpWriteMode.Write;
            }
        }

        /// <summary>
        /// Gets or sets the delegate used to signal on Continue callback.
        /// </summary>
        /// <value>A delegate that implements the callback method that executes
        /// when an HTTP Continue response is returned from the Internet
        /// resource.  The default value is <b>null</b>.</value>
        /// <remarks>
        /// This property gets or sets the delegate method called when an HTTP
        /// 100-continue response is received from the Internet resource.
        /// </remarks>
        public HttpContinueDelegate ContinueDelegate
        {
            get { return m_continueDelegate; }
            set { m_continueDelegate = value; }
        }

        /// <summary>
        /// Gets  a value that indicates whether the request should follow
        /// redirection responses.  This value is always
        /// <itemref>false</itemref>, because Autodirect isn't supported.
        /// </summary>
        /// <value>This value is always <itemref>false</itemref>, because
        /// Autodirect isn't supported.</value>
        public bool AllowAutoRedirect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the maximum number of automatic redirections.  This value is
        /// always zero, because auto-redirection isn't supported.
        /// </summary>
        /// <value>This value is always zero, because auto-redirection isn't
        /// supported.</value>
        public int MaximumAutomaticRedirections
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP method of this request.
        /// </summary>
        /// <value>The request method to use to contact the Internet resource.
        /// The default value is GET.</value>
        /// <remarks>
        /// This method represents the initial origin verb, which is unchanged
        /// and unaffected by redirects.
        /// </remarks>
        public override string Method
        {
            get
            {
                return m_method;
            }

            set
            {
                if (ValidationHelper.IsBlankString(value))
                {
                    throw new ArgumentException("Blank Method Set: " + value);
                }

                if (value.IndexOfAny(k_invalidMethodChars) != -1)
                {
                    throw new ArgumentException("Invalid Method Set: " + value);
                }

                m_method = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to use a persistent connection, if available.
        /// </summary>
        /// <value><b>true</b> if the request to the Internet resource should
        /// contain a <b>Connection</b> HTTP header with the value Keep-alive;
        /// otherwise, <b>false</b>.  The default is <b>true</b>.</value>
        public bool KeepAlive
        {
            get
            {
                return m_keepAlive;
            }

            set
            {
                m_keepAlive = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed length of the response headers.
        /// </summary>
        /// <value>The length, in kilobytes (1024 bytes), of the response
        /// headers.</value>
        /// <remarks>
        /// The length of the response header includes the response status line
        /// and any extra control characters that are received as part of HTTP
        /// protocol.  A value of -1 means no limit is imposed on the response
        /// headers; a value of 0 means that all requests fail.  If this
        /// property is not explicitly set, it defaults to the value of the
        /// <see cref="System.Net.HttpWebRequest.DefaultMaximumResponseHeadersLength"/>
        /// property.
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
        /// Gets or sets the default maximum allowed length of the response
        /// headers.
        /// </summary>
        /// <value>The default maximum allowed length of the response headers.
        /// </value>
        /// <remarks>
        /// On creation of an <itemref>HttpWebRequest</itemref> instance, this
        /// value is used for the
        /// <see cref="System.Net.HttpWebRequest.MaximumResponseHeadersLength"/>
        /// property.
        /// </remarks>
        public static int DefaultMaximumResponseHeadersLength
        {
            get { return defaultMaxResponseHeadersLen; }
            set
            {
                if (value <= 0 && value != -1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                defaultMaxResponseHeadersLen = value;
            }
        }

        /// <summary>
        /// A collection of HTTP headers stored as name/value pairs.
        /// </summary>
        /// <value>A <b>WebHeaderCollection</b> that contains the name/value
        /// pairs that make up the headers for the HTTP request.</value>
        /// <remarks>
        /// The following header values are set through properties on the
        /// <itemref>HttpWebRequest</itemref> class: Accept, Connection,
        /// Content-Length, Content-Type, Expect, Range, Referer,
        /// Transfer-Encoding, and User-Agent.  Trying to set these header
        /// values by using
        /// <b>WebHeaderCollection.<see cref="System.Net.WebHeaderCollection.Add(string, string)"/>()</b>
        /// will raise an exception.  Date and Host are set internally.
        /// </remarks>
        public override WebHeaderCollection Headers
        {
            get
            {
                return m_httpRequestHeaders;
            }

            set
            {
                // we can't change headers after they've already been sent
                if (m_requestSent)
                    throw new InvalidOperationException("Cannot change headers after request submitted");

                WebHeaderCollection webHeaders = value;
                WebHeaderCollection newWebHeaders =
                    new WebHeaderCollection(true);

                // Copy And Validate -
                // Handle the case where their object tries to change
                // name, value pairs after they call set, so therefore,
                // we need to clone their headers.
                for (int i = 0; i < webHeaders.AllKeys.Length; i++)
                {
                    newWebHeaders.Add(webHeaders.AllKeys[i], webHeaders[webHeaders.AllKeys[i]]);
                }

                m_httpRequestHeaders = newWebHeaders;
            }
        }

        /// <summary>
        /// Gets or sets the proxy for the request.
        /// </summary>
        /// <value>The <see cref="System.Net.IWebProxy"/> object to use to proxy
        /// the request.  <b>null</b> indicates that no proxy will be used.</value>
        public override IWebProxy Proxy
        {
            get { return m_proxy; }
            set
            {
                if (m_requestSent)
                    throw new InvalidOperationException("Cannot change proxy after request submitted");
                if (value == null)
                    throw new ArgumentNullException();

                m_proxy = value;
            }
        }

        /// <summary>
        /// Gets or sets the state of chunk transfer send mode.
        /// </summary>
        /// <value><b>true</b> to send data to the Internet resource in
        /// segments; otherwise, <b>false</b>.  The default value is
        /// <b>false</b>.</value>
        /// <remarks>
        /// If <itemref>true</itemref>, bits are uploaded and written using the
        /// <b>Chunked</b> property of <b>HttpWriteMode</b>.
        /// </remarks>
        public bool SendChunked
        {
            get { return m_httpWriteMode == HttpWriteMode.Chunked; }
            set
            {
                //no race.  Don't need interlocked
                if (true == m_requestSent)
                {
                    throw new InvalidOperationException("Cannot set \"chunked\" after request submitted");
                }

                if (value)
                {
                    m_httpWriteMode = HttpWriteMode.Chunked;
                }
                else
                {
                    if (m_contentLength >= 0)
                    {
                        m_httpWriteMode = HttpWriteMode.Write;
                    }
                    else
                    {
                        m_httpWriteMode = HttpWriteMode.None;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the HTTP protocol version for this request.
        /// </summary>
        /// <value>The HTTP version to use for the request.  The default is
        /// <see cref="System.Net.HttpVersion.Version11"/>.</value>
        public Version ProtocolVersion
        {
            get
            {
                return m_version;
            }

            set
            {
                if (!value.Equals(HttpVersion.Version10) &&
                    !value.Equals(HttpVersion.Version11))
                {
                    throw new ArgumentException("Invalid HTTP Verion: " + value);
                }

                m_version = new Version(value.Major, value.Minor);
            }
        }

        /// <summary>
        /// Private method for removing duplicate code which removes and adds
        /// headers that are marked private.
        /// </summary>
        /// <param name="HeaderName">The name of the HTTP header.</param>
        /// <param name="value">The value of the HTTP header.</param>
        private void SetSpecialHeaders(String HeaderName, String value)
        {
            value = WebHeaderCollection.CheckBadChars(value, true);

            m_httpRequestHeaders.RemoveInternal(HeaderName);
            if (value != null && value.Length != 0)
            {
                m_httpRequestHeaders.AddInternal(HeaderName, value);
            }
        }

        /// <summary>
        /// Gets or sets the type of the entity body (the value of the content
        /// type).
        /// </summary>
        /// <value>The value of the <b>Content-type</b> HTTP header.  The
        /// default value is <b>null</b>.</value>
        /// <remarks>
        /// Setting to <b>null</b> clears the content-type.
        /// </remarks>
        public override String ContentType
        {
            get
            {
                return m_httpRequestHeaders[HttpKnownHeaderNames.ContentType];
            }

            set
            {
                SetSpecialHeaders(HttpKnownHeaderNames.ContentType, value);
            }
        }

        /// <summary>
        /// Gets or sets the <b>TransferEncoding</b> HTTP header.
        /// </summary>
        /// <value>The value of the <b>Transfer-encoding</b> HTTP header.  The
        /// default value is <b>null</b>.</value>
        /// <remarks>
        /// <b>null</b> clears the transfer encoding except for the
        /// <b>Chunked</b> setting.
        /// </remarks>
        public String TransferEncoding
        {
            get
            {
                return m_httpRequestHeaders[HttpKnownHeaderNames.TransferEncoding];
            }

            set
            {
                bool fChunked;

                // on blank string, remove current header
                if (ValidationHelper.IsBlankString(value))
                {
                    // if the value is blank, then remove the header
                    m_httpRequestHeaders.RemoveInternal(HttpKnownHeaderNames.TransferEncoding);

                    return;
                }

                // if not, check if the user is trying to set chunked:
                string newValue = value.ToLower();

                fChunked = (newValue.IndexOf("chunked") != -1);

                // prevent them from adding chunked, or from adding an Encoding
                // without turing on chunked, the reason is due to the HTTP
                // Spec which prevents additional encoding types from being
                // used without chunked
                if (fChunked)
                {
                    throw new ArgumentException("Cannot add \"Encoding\" and set \"chunked\"");
                }
                else if (m_httpWriteMode != HttpWriteMode.Chunked)
                {
                    throw new InvalidOperationException("Need HttpWriteMode.Chunked to be current mode");
                }
                else
                {
                    m_httpRequestHeaders.CheckUpdate(HttpKnownHeaderNames.TransferEncoding, value);
                }
            }

        }

        /// <summary>
        /// Gets or sets the value of the <b>Accept</b> HTTP header.
        /// </summary>
        /// <value>The value of the <b>Accept</b> HTTP header.  The default
        /// value is <b>null</b>.</value>
        public String Accept
        {
            get { return m_httpRequestHeaders[HttpKnownHeaderNames.Accept]; }
            set { SetSpecialHeaders(HttpKnownHeaderNames.Accept, value); }
        }

        /// <summary>
        /// Gets or sets the value of the <b>Referer</b> HTTP header.
        /// </summary>
        /// <value>The value of the <b>Referer</b> HTTP header.  The default
        /// value is <b>null</b>.</value>
        /// <remarks>This header value is misspelled intentionally.</remarks>
        public String Referer
        {
            get { return m_httpRequestHeaders[HttpKnownHeaderNames.Referer]; }
            set
            {
                SetSpecialHeaders(HttpKnownHeaderNames.Referer, value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the <b>User-Agent</b> HTTP header.
        /// </summary>
        /// <value>The value of the <b>User-agent</b> HTTP header.  The default
        /// value is <b>null</b>.</value>
        public String UserAgent
        {
            get { return m_httpRequestHeaders[HttpKnownHeaderNames.UserAgent]; }
            set
            {
                SetSpecialHeaders(HttpKnownHeaderNames.UserAgent, value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the <b>Expect</b> HTTP header.
        /// </summary>
        /// <value>The contents of the <b>Expect</b> HTTP header.  The default
        /// value is <b>null</b>.</value>
        /// <remarks>When setting this property, <b>null</b> clears the
        /// <b>Expect</b> (except for the 100-continue value).</remarks>
        public String Expect
        {
            get { return m_httpRequestHeaders[HttpKnownHeaderNames.Expect]; }
            set
            {
                // on blank string, remove current header
                if (ValidationHelper.IsBlankString(value))
                {
                    m_httpRequestHeaders.RemoveInternal(HttpKnownHeaderNames.Expect);
                    return;
                }

                m_httpRequestHeaders.CheckUpdate(HttpKnownHeaderNames.Expect, value);
            }
        }

        /// <summary>
        /// Gets the <itemref>IfModifiedSince</itemref> value of
        /// <itemref>HttpKnownHeaderNames</itemref>.
        /// </summary>
        /// <value>A <see cref="System.DateTime"/> that contains the contents of
        /// the <b>If-Modified-Since</b> HTTP header.  The default value is the
        /// current date and time.</value>
        /// <remarks>
        /// The setter for this property isn't supported, because a function
        /// that formats the time isn't implemented.
        /// <para>
        /// <b>null</b> clears the
        /// <itemref>IfModifiedSince</itemref> header.</para>
        /// </remarks>
        public DateTime IfModifiedSince
        {
            get
            {
                string ifmodHeaderValue = m_httpRequestHeaders[HttpKnownHeaderNames.IfModifiedSince];

                if (ifmodHeaderValue == null)
                {
                    return DateTime.Now;
                }

                return HttpProtocolUtils.string2date(ifmodHeaderValue);
            }

            // Set is not supported at this moment.  It is needed for server.
            //set
            //{
            //    SetSpecialHeaders(HttpKnownHeaderNames.IfModifiedSince,
            //                      HttpProtocolUtils.date2string(value));
            //}
        }

        /// <summary>
        /// Constructs an instance of the HTTP Protocol class and initalizes it
        /// to the basic header state.
        /// </summary>
        /// <param name="Url">The Url object for which we're creating.</param>
        internal HttpWebRequest(Uri Url)
        {
            m_requestSent = false;
            m_originalUrl = Url;
            SendChunked = false;
            m_keepAlive = true;
            m_httpRequestHeaders = new WebHeaderCollection(true);
            m_httpWriteMode = HttpWriteMode.None;

            m_contentLength = -1;
            m_version = HttpVersion.Version11;

            m_allowWriteStreamBuffering = false;

            Method = "GET";

            m_timeout = WebRequest.DefaultTimeout;
            m_readWriteTimeout = DefaultReadWriteTimeout;

            // set the default proxy initially (this can be overriden by the Proxy property)
            m_proxy = WebRequest.DefaultWebProxy;

            m_responseCreated = false;
        }

        public void Reset()
        {
            m_requestSent = false;
            m_responseCreated = false;
            m_contentLength = -1;
            m_httpWriteMode = HttpWriteMode.None;
            m_httpRequestHeaders = new WebHeaderCollection(true);
        }

        /// <summary>
        /// Gets whether a response has been received from an Internet resource.
        /// </summary>
        /// <value><b>true</b> if a response has been received; otherwise,
        /// <b>false</b>.</value>
        public bool HaveResponse
        {
            get { return (m_responseComplete); }
        }

        /// <summary>
        /// Adds a byte range header to the request for a specified range.
        /// </summary>
        /// <param name="from">The start of the range.</param>
        /// <param name="to">The end of the range.</param>
        public void AddRange(int from, int to)
        {
            AddRange("bytes", from, to);
        }

        /// <summary>
        /// Adds a range header to a request for a specific range from the
        /// beginning or end of the requested data.
        /// </summary>
        /// <param name="range">Start of the range.  The end of the range is the
        /// end of the existing data.</param>
        public void AddRange(int range)
        {
            AddRange("bytes", range);
        }

        /// <summary>
        /// Adds a range header to a request for a specified range.
        /// </summary>
        /// <param name="rangeSpecifier">The description of the range, such as
        /// "bytes".</param>
        /// <param name="from">The start of the range.</param>
        /// <param name="to">The end of the range.</param>
        /// <remarks>
        /// <paramref name="rangeSpecifier"/> would normally be
        /// specified as "bytes", since this is the only range specifier
        /// recognized by most HTTP servers.  Setting
        /// <paramref name="rangeSpecifier"/> to some other string allows
        /// support for custom range specifiers other than bytes.  The
        /// byte-range specifier is defined in RFC 2616 by the IETF.</remarks>
        public void AddRange(string rangeSpecifier, int from, int to)
        {

            // Do some range checking before assembling the header
            if ((from < 0) || (to < 0) || (from > to))
            {
                throw new ArgumentOutOfRangeException();
            }

            // Add it
            if (!AddRange(rangeSpecifier, from.ToString(), to.ToString()))
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Adds a range header to a request for a specific range from the
        /// beginning or end of the requested data.
        /// </summary>
        /// <param name="rangeSpecifier">The description of the range, such as
        /// "bytes".</param>
        /// <param name="range">The range value.</param>
        public void AddRange(string rangeSpecifier, int range)
        {
            if (!AddRange(rangeSpecifier, range.ToString(), (range >= 0) ? "" : null))
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Adds or extends a range header.
        /// </summary>
        /// <param name="rangeSpecifier">Range specifier</param>
        /// <param name="from">Start of range</param>
        /// <param name="to">End of range</param>
        /// <returns>TBD</returns>
        /// <remarks>
        /// Various range types can be specified via
        /// <paramref name="rangeSpecifier"/>, but only one type of Range
        /// request will be made; for example, a byte-range request, or a
        /// row-range request.  Range types cannot be mixed.
        /// </remarks>
        private bool AddRange(string rangeSpecifier, string from, string to)
        {
            // Checks for NULL rangeSpecifier
            if (rangeSpecifier == null)
            {
                throw new ArgumentNullException();
            }

            // Checks for Valid characters in rangeSpecifier
            if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
            {
                throw new ArgumentException();
            }

            // Add range specifier or appends to existing one
            string curRange = m_httpRequestHeaders[HttpKnownHeaderNames.Range];

            if ((curRange == null) || (curRange.Length == 0))
            {
                curRange = rangeSpecifier + "=";
            }
            else
            {
                if (String.Compare(curRange.Substring(0, curRange.IndexOf('=')), rangeSpecifier) != 0)
                {
                    return false;
                }

                curRange = string.Empty;
            }

            curRange += from.ToString();
            if (to != null)
            {
                curRange += "-" + to;
            }

            m_httpRequestHeaders.SetAddVerified(HttpKnownHeaderNames.Range, curRange);
            return true;
        }

        /// <summary>
        /// This function is called first in GetRequestStream() and throws exception
        /// if conditions are not correct.
        /// </summary>
        private void ValidateGetRequestStream()
        {
            // TransferEncoding is set to a value and SendChunked is false.
            if (TransferEncoding != null && SendChunked == false)
            {
                throw new InvalidOperationException();
            }

            // ProtocolViolationException The Method property is GET or HEAD.
            if (m_method == "GET" || m_method == "HEAD")
            {
                throw new ProtocolViolationException("HTTP Method is incorrect: " + Method);
            }

            // Condition for exception - KeepAlive is true, AllowWriteStreamBuffering is false,
            // ContentLength is -1, SendChunked is false.
            if (m_method == "PUT" || m_method == "POST")
            {
                if (ContentLength == -1 && SendChunked == false)
                {
                    throw new ProtocolViolationException("Content lenght must be present for this request");
                }
            }
        }

        /// <summary>
        /// Updates the HTTP WEB header collection to prepare it for request.
        /// </summary>
        private void PrepareHeaders()
        {
            // Depending on protocol version we update HTTP headers collection.
            if (!(m_version.Equals(HttpVersion.Version10)))
            {
                if (m_httpWriteMode == HttpWriteMode.Write)
                {
                    m_httpRequestHeaders.ChangeInternal(HttpKnownHeaderNames.ContentLength, m_contentLength.ToString());
                }
                else if (m_httpWriteMode == HttpWriteMode.Chunked)
                {
                    m_httpRequestHeaders.AddInternal(HttpKnownHeaderNames.TransferEncoding, "chunked");
                }

                // Set keepAlive header, we always send it, do not rely in defaults.
                // Basically we send "Connection:Close" or "Connection:Keep-Alive"
                string connectionValue;
                if (m_keepAlive)
                {
                    connectionValue = "Keep-Alive";
                }
                else
                {
                    connectionValue = "Close";
                }

                m_httpRequestHeaders.ChangeInternal(HttpKnownHeaderNames.Connection, connectionValue);
            }

            //1.0 path
            else
            {
                //1.0 doesn't support chunking
                SendChunked = false;

                //1.0 doesn't support keep alive
                m_keepAlive = false;

                if (m_httpWriteMode == HttpWriteMode.Write)
                {
                    m_httpRequestHeaders.ChangeInternal(HttpKnownHeaderNames.ContentLength, m_contentLength.ToString());
                }
            }

            m_httpRequestHeaders.ChangeInternal(HttpKnownHeaderNames.Host, ConnectHostAndPort());
            // Adds user name and password for basic Http authentication.
            if (m_NetworkCredentials != null && m_NetworkCredentials.AuthenticationType == AuthenticationType.Basic)
            {   // If credentials are supplied, we need to add header like "Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ=="
                // where QWxhZGRpbjpvcGVuIHNlc2FtZQ== is b64 encoded user name and password orginating as username:password.
                string userInfo = "";
                if (m_NetworkCredentials.UserName != null)
                {
                    userInfo += m_NetworkCredentials.UserName;
                }

                userInfo += ":";
                if (m_NetworkCredentials.Password != null)
                {
                    userInfo += m_NetworkCredentials.Password;
                }

                // Encode user info.
                byte[] buffer = Encoding.UTF8.GetBytes(userInfo);
                string userNameAndPassEncoded = buffer != null ? Convert.ToBase64String(buffer) : "";
                string authValue = "Basic " + userNameAndPassEncoded;
                m_httpRequestHeaders.ChangeInternal(HttpKnownHeaderNames.Authorization, authValue);
            }

            m_requestSent = true;
        }

        /// <summary>
        /// Return string with remote Host and Port if port is not default.
        /// Need update for HTTPS.
        /// </summary>
        /// <returns>String with host Url and port corresponding to target Uri.</returns>
        internal string ConnectHostAndPort()
        {
            string retStr = m_originalUrl.Host;
            if (m_originalUrl.Port != 80)
            {
                retStr += ":" + m_originalUrl.Port;
            }

            return retStr;
        }

        /// <summary>
        /// Removes the given stream from the connection pool
        /// </summary>
        internal static void RemoveStreamFromPool(InputNetworkStreamWrapper stream)
        {
            lock (m_ConnectedStreams)
            {
                if (m_ConnectedStreams.Contains(stream))
                {
                    m_ConnectedStreams.Remove(stream);
                }
            }
        }

        /// <summary>
        /// Returns network stream connected to server. It could be a proxy or a
        /// real server Uri.
        /// </summary>
        /// <param name="proxyServer">Uri that describes the proxy server.</param>
        /// <param name="targetServer">Uri that describes the target (real) server.</param>
        /// <returns>Nerwork stream connected to server.</returns>
        private InputNetworkStreamWrapper EstablishConnection(Uri proxyServer, Uri targetServer)
        {
            InputNetworkStreamWrapper retStream = null;

            // Create a socket and set reuse true.
            // But before creating new socket we look in the list of existing sockets. If socket for this host already
            // exist - use it. No need to create new socket.
            string remoteServer = targetServer.Host + ":" + targetServer.Port;
            lock (m_ConnectedStreams)
            {
                ArrayList removeStreamList = new ArrayList();

                for (int i = 0; i < m_ConnectedStreams.Count; i++)
                {
                    InputNetworkStreamWrapper inputStream = (InputNetworkStreamWrapper)m_ConnectedStreams[i];

                    if (inputStream.m_rmAddrAndPort == remoteServer && !inputStream.m_InUse)
                    {
                        // Re-use the connected socket.
                        // But first we need to know that socket is not closed.
                        try
                        {
                            // If socket is closed ( from this or other side ) the call throws exception.
                            if (inputStream.m_Socket.Poll(1, SelectMode.SelectWrite))
                            {
                                // No exception, good we can condtinue and re-use connected stream.

                                // Control flow returning here means persistent connection actually works. 
                                inputStream.m_InUse = true;
                                inputStream.m_lastUsed = DateTime.Now;

                                retStream = inputStream;
                                break;
                            }
                            else
                            {
                                removeStreamList.Add(inputStream);
                            }

                        }
                        catch (Exception)
                        {
                            removeStreamList.Add(inputStream);
                        }

                    }
                }

                for (int i = 0; i < removeStreamList.Count; i++)
                {
                    InputNetworkStreamWrapper removeStream = (InputNetworkStreamWrapper)removeStreamList[i];

                    // Means socket was closed. Remove it from the list.
                    m_ConnectedStreams.Remove(removeStream);

                    removeStream.Dispose();
                }
            }

            if (retStream == null)
            {
                // Existing connection did not worked. Need to establish new one.
                IPAddress address = null;
                UriHostNameType hostNameType = proxyServer.HostNameType;
                if (hostNameType == UriHostNameType.IPv4)
                {
                    address = IPAddress.Parse(proxyServer.Host);
                }
                else if (hostNameType == UriHostNameType.Dns)
                {
                    IPHostEntry hostEntry = null;

                    try
                    {
                        hostEntry = Dns.GetHostEntry(proxyServer.Host);
                    }
                    catch(SocketException se)
                    {
                        throw new WebException("host not available", se, WebExceptionStatus.ConnectFailure, null);
                    }

                    int addressListSize = hostEntry.AddressList.Length;
                    for (int i = 0; i < addressListSize; i++)
                    {
                        if ((address = hostEntry.AddressList[i]) != null)
                        {
                            break;
                        }
                    }

                    if (address == null)
                    {
                        throw new WebException("Unable to resolve Dns entry to valid IPv4 Address", WebExceptionStatus.NameResolutionFailure);
                    }
                }
                else
                {
                    throw new WebException("Only IPv4 or Dns host names allowed.");
                }

                // If socket was not found in waiting connections, then we create new one.
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                catch{}
                try
                {
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                }
                catch{}

                // Connect to remote endpoint
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(address, proxyServer.Port);
                    socket.Connect((EndPoint)remoteEP);
                }
                catch (SocketException e)
                {
                    throw new WebException("connection failed", e, WebExceptionStatus.ConnectFailure, null);
                }

                bool isHttps = m_originalUrl.Scheme == "https";

                // We have connected socket. Create request stream
                retStream = new InputNetworkStreamWrapper(new NetworkStream(socket), socket, !isHttps, proxyServer.Host + ":" + proxyServer.Port);

                // For https proxy works differenly from http.
                if (isHttps)
                {
                    // If proxy is set, then for https we need to send "CONNECT" command to proxy.
                    // Once this command is send, the socket from proxy works as if it is the socket to the destination server.
                    if (proxyServer != targetServer)
                    {
                        String request = "CONNECT " + remoteServer + " HTTP/" + ProtocolVersion + "\r\n\r\n";
                        Byte[] bytesToSend = Encoding.UTF8.GetBytes(request);
                        retStream.Write(bytesToSend, 0, bytesToSend.Length);

                        // Now proxy should respond with the connected status. If it is successul, then we are good to go.
                        CoreResponseData respData = ParseHTTPResponse(retStream, m_keepAlive);
                        if (respData.m_statusCode != (int)HttpStatusCode.OK)
                        {
                            throw new WebException("Proxy returned " + respData.m_statusCode, WebExceptionStatus.ConnectFailure);
                        }
                    }

                    // Once connection estiblished need to create secure stream and authenticate server.
                    SslStream sslStream = new SslStream(retStream.m_Socket);

                    // Throws exception is fails.
                    sslStream.AuthenticateAsClient(m_originalUrl.Host, null, m_caCerts, SslVerification.CertificateRequired, SslProtocols.Default);

                    // Changes the stream to SSL stream.
                    retStream.m_Stream = sslStream;

                    // Changes the address. Originally socket was connected to proxy, now as if it connected to m_originalUrl.Host on m_originalUrl.Port
                    retStream.m_rmAddrAndPort = m_originalUrl.Host + ":" + m_originalUrl.Port;
                }

                lock (m_ConnectedStreams)
                {
                    m_ConnectedStreams.Add(retStream);

                    // if the current stream list is empty then start the timer that drops unused connections.
                    if (m_ConnectedStreams.Count == 1)
                    {
                        m_DropOldConnectionsTimer.Change(HttpListener.DefaultKeepAliveMilliseconds, System.Threading.Timeout.Infinite);
                    }
                }
            }

            return retStream;
        }

        /// <summary>
        /// Submits request to the WEB server.
        /// </summary>
        private void SubmitRequest()
        {
            // We have connected socket. Create request stream
            // If proxy is set - connect to proxy server.

            if(m_requestStream == null)
            {
                if (m_proxy == null)
                {   // Direct connection to target server.
                    m_requestStream = EstablishConnection(m_originalUrl, m_originalUrl);
                }
                else // Connection through proxy. We create network stream connected to proxy
                {
                    Uri proxyUri = m_proxy.GetProxy(m_originalUrl);

                    if (m_originalUrl.Scheme == "https")
                    {
                        // For HTTPs we still need to know the target name to decide on persistent connection.
                        m_requestStream = EstablishConnection(proxyUri, m_originalUrl);
                    }
                    else
                    {
                        // For normal HTTP all requests go to proxy
                        m_requestStream = EstablishConnection(proxyUri, proxyUri);
                    }
                }
            }

            // We have connected stream. Set the timeout from HttpWebRequest
            m_requestStream.WriteTimeout = m_readWriteTimeout;
            m_requestStream.ReadTimeout = m_readWriteTimeout;

            // Now we need to write headers. First we update headers.
            PrepareHeaders();

            // Now send request string and headers.
            byte[] dataToSend = GetHTTPRequestData();

#if DEBUG   // In debug mode print the request. It helps a lot to troubleshoot the issues.
            int byteUsed, charUsed;
            bool completed = false;
            char[] charBuf = new char[dataToSend.Length];
            UTF8decoder.Convert(dataToSend, 0, dataToSend.Length, charBuf, 0, charBuf.Length, true, out byteUsed, out charUsed, out completed);
            string strSend = new string(charBuf);
            Debug.Print(strSend);
#endif
            // Writes this data to the network stream.
            m_requestStream.Write(dataToSend, 0, dataToSend.Length);
            m_requestSent = true;
        }


        /// <summary>
        /// Reads and parses HTTP response from server.
        /// After return of function HTTP response is read.
        /// </summary>
        /// <param name="inStream">Network stream connected to server.</param>
        /// <param name="defaultKeepAlive">TBD</param>
        /// <returns>CoreResponseData that describes server response.</returns>
        private CoreResponseData ParseHTTPResponse(InputNetworkStreamWrapper inStream, bool defaultKeepAlive)
        {
            // CoreResponseData keeps all the information of the response.
            CoreResponseData ret = new CoreResponseData();
            // maximumHeadersLength is maximum total length of http header. Basically this is amount
            // of memory used for headers.
            int headersLength = m_maxResponseHeadersLen == -1 ? 0x7FFFFFFF : m_maxResponseHeadersLen * 1024;

            ret.m_shouldClose = !defaultKeepAlive;
            // Parse the request line.
            string line = inStream.Read_HTTP_Line(maxHTTPLineLength).Trim();

            // Cutoff white spaces
            int currentOffset = 0;
            for (; currentOffset < line.Length && ' ' != line[currentOffset]; ++currentOffset) ;

            // find HTTP version, read http/1.x
            string httpVersionString = line.Substring(0, currentOffset).ToLower();
            if (httpVersionString.Equals("http/1.1"))
            {
                ret.m_version = HttpVersion.Version11;
            }
            else if (httpVersionString.Equals("http/1.0"))
            {
                ret.m_version = HttpVersion.Version10;
            }
            else
            {
                ret.m_status = WebExceptionStatus.ServerProtocolViolation;
                ret.m_exceptionMessage = "Unknown http version: " + httpVersionString;
                return ret;
            }

            //advance to the status code
            for (; currentOffset < line.Length && ' ' == line[currentOffset]; ++currentOffset) ;

            // Read the status code
            int codeStart = currentOffset;
            for (; currentOffset < line.Length && ' ' != line[currentOffset]; ++currentOffset) ;
            int statusCode = -1;
            try
            {
                string statusCodeStr =
                    line.Substring(codeStart,
                                    currentOffset - codeStart);
                statusCode = Convert.ToInt32(statusCodeStr);
            }
            catch (Exception e)
            {
                ret.m_status = WebExceptionStatus.ServerProtocolViolation;
                ret.m_exceptionMessage = "Missing status code in HTTP reply";
                ret.m_innerException = e;
                return ret;
            }

            // If we get here - status code should be read.
            ret.m_statusCode = statusCode;

            // Advance to the status message.  The message is optional
            for (; currentOffset < line.Length && ' ' != line[currentOffset]; ++currentOffset) ;
            ret.m_statusDescription = line.Substring(currentOffset);

            ret.m_headers = new WebHeaderCollection(true);
            ret.m_chunked = false;
            ret.m_contentLength = -1;

            while ((line = inStream.Read_HTTP_Header(maxHTTPLineLength)).Length > 0)
            {
                // line.Length is used for the header. Substruct it.
                headersLength -= line.Length;
                // If total length used for header is exceeded, we break
                if (headersLength < 0)
                {
                    ret.m_status = WebExceptionStatus.ServerProtocolViolation;
                    ret.m_exceptionMessage = "Headers size exceed limit";
                    return ret;
                }

                // Now parse the header.
                int sepIdx = line.IndexOf(':');
                if (sepIdx == -1)
                {
                    ret.m_status = WebExceptionStatus.ServerProtocolViolation;
                    ret.m_exceptionMessage = "Illegal header format: " + line;
                    return ret;
                }

                string headerName = line.Substring(0, sepIdx);
                string headerValue = line.Substring(sepIdx + 1).TrimStart(null);
                string matchableHeaderName = headerName.ToLower();

                ret.m_headers.AddInternal(headerName, headerValue);
                if (matchableHeaderName.Equals("content-length"))
                {
                    try
                    {
                        ret.m_contentLength = Convert.ToInt32(headerValue);

                        // set the response stream length for the input stream, so that an EOF will be read
                        // if the caller tries to read base the response content length
                        inStream.m_BytesLeftInResponse = ret.m_contentLength;
                    }
                    catch (Exception e)
                    {
                        ret.m_status =
                            WebExceptionStatus.ServerProtocolViolation;
                        ret.m_exceptionMessage = "Content length NAN: " + headerValue;
                        ret.m_innerException = e;
                        return ret;
                    }
                }
                else if (matchableHeaderName.Equals("transfer-encoding"))
                {
                    if (headerValue.ToLower().IndexOf("chunked") != -1)
                    {
                        ret.m_chunked = true;
                    }
                }
                else if (matchableHeaderName.Equals("connection"))
                {
                    if (headerValue.ToLower().IndexOf(HttpKnownHeaderValues.close) != -1)
                    {
                        ret.m_shouldClose = true;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Event handler for the web request timeout.  This handler will be invoked if the response takes longer than the value
        /// indicated by the property Timeout.
        /// </summary>
        /// <param name="arg"></param>
        private void OnRequestTimeout(object arg)
        {
            if(m_requestStream != null && m_requestStream.m_Socket != null)
            {
                try
                {
                    // Close the socket to kill the operation
                    m_requestStream.m_Socket.Close();
                }
                catch
                {
                }
                finally
                {
                    m_requestStream.m_InUse = false;
                }
            }
        }

        /// <summary>
        /// Returns a response from an Internet resource.  Overrides the 
        /// <itemref>WebRequest</itemref>.<see cref="System.Net.WebRequest.GetResponse"/> 
        /// method.
        /// </summary>
        /// <returns>The response from the Internet resource.</returns>
        public override WebResponse GetResponse()
        {
            HttpWebResponse response = null;

            try
            {
                // If response was not sent, Submit the request. 
                if (!m_requestSent)
                {
                    SubmitRequest();
                }

                CoreResponseData respData = null;

                // reset the total response bytes for the new request.
                m_requestStream.m_BytesLeftInResponse = -1;

                // create the request timeout timer.  This will kill the operation if it takes longer than specified by the Timeout property.  
                // The underlying socket will be closed to end the web request
                using (Timer tmr = new Timer(new TimerCallback(OnRequestTimeout), null, m_timeout, System.Threading.Timeout.Infinite))
                {
                    // Processes response from server. Request stream should already be there.

                    respData = ParseHTTPResponse(m_requestStream, m_keepAlive);

                    if (respData.m_statusCode == (int)HttpStatusCode.Continue)
                    {
                        if (m_continueDelegate != null)
                        {
                            m_continueDelegate(respData.m_statusCode, respData.m_headers);
                        }
                        else
                        {
                            respData = ParseHTTPResponse(m_requestStream, m_keepAlive);
                        }
                    }
                }
                
                response = new HttpWebResponse(m_method, m_originalUrl, respData, this);
                
                // Now we look if response has chunked encoding. If it is chunked, we need to set flag in m_requestStream we return.
                if (respData.m_chunked)
                {
                    m_requestStream.m_EnableChunkedDecoding = true;
                }

                // Currently the request and response are the same network streams, but we optimize later.
                response.SetResponseStream(m_requestStream);

                m_responseStatus = response.StatusCode;

                m_responseCreated = true;
            }
            catch(SocketException se)
            {
                if (m_requestStream != null)
                {
                    m_requestStream.m_InUse = false;

                    if (m_requestStream.m_Socket != null)
                    {
                        this.m_requestStream.m_Socket.Close();
                    }
                }

                throw new WebException("", se);
            }
            catch(Exception e)
            {
                throw new WebException("", e);
            }


            return response;
        }

        /// <summary>
        /// Submits a request with HTTP headers to the server, and returns a
        /// <b>Stream</b> object to use to write request data.
        /// </summary>
        /// <returns>A <b>Stream</b> to use to write request data.</returns>
        /// <remarks>Used for POST of PUT requests.</remarks>
        public override Stream GetRequestStream()
        {
            // Validates the call to GetRequestStream. Throws exception on errors.
            ValidateGetRequestStream();

            // Submits the request.
            try
            {
                SubmitRequest();
            }
            catch
            {
                if(m_requestStream != null)
                {
                    RemoveStreamFromPool(m_requestStream);
                    m_requestStream.Dispose();
                }
                throw;
            }
            
            // Return the stream
            return m_requestStream.CloneStream();
        }

        /// <summary>
        /// Constucts WEB exception if error is detected during parsing.
        /// </summary>
        /// <param name="inner">Inner exception network exception</param>
        /// <param name="resp">Partially constructed HttpWebResponse</param>
        /// <returns>WebException instance</returns>
        private static WebException protocolError(Exception inner,
                                                   HttpWebResponse resp)
        {
            HttpStatusCode statusCode = resp.StatusCode;
            int sr = (int)statusCode;
            string message = "(" + ((int)statusCode) + ")";
            string description;
            description = resp.StatusDescription;
            if (description != null && description.Length > 0)
                message += " " + description;
            message = "Server returned error" + message;
            return new WebException(message, inner,
                                     WebExceptionStatus.ProtocolError, resp);
        }

        internal bool m_sentHeaders = false;

        private bool hasEntityData()
        {
            if (m_httpWriteMode != HttpWriteMode.None)
                return true;
            else
                return false;
        }

        private bool canWrite()
        {
            return !KnownVerbs.GetHttpVerbType(Method).m_ContentBodyNotAllowed;
        }

        /// <summary>
        /// Retrieves HTTP request as bytes array.  Used to create a request
        /// message.
        /// </summary>
        /// <returns>Byte array with HTTP request. This data is sent through network.</returns>
        private byte[] GetHTTPRequestData()
        {
            //step 1 - compute the length of the headers.

            string statusLine;

            // Connect verbs require CONNECT host:port
            if (Method.ToUpper().Equals("CONNECT"))
            {
                statusLine = "CONNECT " + Address.Host + ":" + Address.Port + " HTTP/" + ProtocolVersion + "\r\n";
            }
            else if (m_proxy != null && m_originalUrl.Scheme != "https")
            {
                statusLine = Method + " " + Address.AbsoluteUri + " HTTP/" + ProtocolVersion + "\r\n";
            }
            else
            {
                statusLine = Method + " " + Address.AbsolutePath + " HTTP/" + ProtocolVersion + "\r\n";    // .PathAndQuery
            }

            //most intrinsic headers are stored in the webheaders class
            //content length is not.
            int headersLength = statusLine.Length;

            //extra header lengths.  Includes extension headers.
            headersLength += Headers.byteLength();

            byte[] headerBytes = new byte[headersLength];

            int currentOffset = 0;
            //store the request line
            currentOffset += copyString(statusLine, headerBytes,
                                         currentOffset);

            //now for the general headers
            currentOffset += Headers.copyTo(headerBytes, currentOffset);

            return headerBytes;
        }

        /// <summary>
        /// Converts array to string by casting bytes to chars.
        /// </summary>
        /// <param name="data">Array with byte data.</param>
        /// <param name="offset">Offset to start convertion.</param>
        /// <param name="count">Count of bytes to convert to string.</param>
        /// <returns>String converted from byte array.</returns>
        private static string toEAscii(byte[] data, int offset, int count)
        {
            char[] output = new char[count];
            for (int i = 0; i < count; ++i)
            {
                output[i] = (char)data[offset + i];
            }

            return new string(output, 0, count);
        }

        /// <summary>
        /// Convert string to array of bytes
        /// </summary>
        /// <param name="data">string to convert</param>
        /// <returns>array of bytes converted from string</returns>
        internal static byte[] fromEAscii(string data)
        {
            byte[] ret = new byte[data.Length];
            for (int i = 0; i < data.Length; ++i)
            {
                ret[i] = (byte)data[i];
            }

            return ret;
        }

        /// <summary>
        /// This function returns true if the response code from the server
        /// (i.e. 304) MUST NOT have any entity data.  I will artificially set
        /// the content length in the stream to zero, so that reading...
        /// </summary>
        /// <param name="responseCode">HTTP response code</param>
        /// <returns><b>true</b> if the specified response code is one of the
        /// defined values; otherwise, <b>false</b>.</returns>
        private bool setContentLengthToZero(HttpStatusCode responseCode)
        {
            switch (responseCode)
            {
                case HttpStatusCode.SwitchingProtocols:
                case HttpStatusCode.ResetContent:
                case HttpStatusCode.NotModified:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.UseProxy:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Copies a string into an array of bytes.
        /// </summary>
        /// <param name="src">A String to copy.</param>
        /// <param name="bytes">Output array.</param>
        /// <param name="offset">Offset to start placing data in array.</param>
        /// <returns>Count of bytes copied</returns>
        private static int copyString(String src, byte[] bytes, int offset)
        {
            int i;
            for (i = 0; i < src.Length; ++i)
                bytes[offset + i] = (byte)src[i];
            return i;
        }

    }; // class HttpWebRequest

} // namespace System.Net


