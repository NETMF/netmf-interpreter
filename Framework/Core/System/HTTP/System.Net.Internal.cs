////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    internal class WebRequestPrefixElement
    {
        public string Prefix;
        public IWebRequestCreate Creator;

        public WebRequestPrefixElement(string P, IWebRequestCreate C)
        {
            Prefix = P;
            Creator = C;
        }

    } // class PrefixListElement

    /// <summary>
    /// Used during parsing to capture all the information contained in the http
    /// status line and headers.
    /// </summary>
    internal class CoreResponseData
    {
        // Basic response information.
        internal int m_statusCode;
        internal string m_statusDescription;
        internal WebHeaderCollection m_headers;
        internal Version m_version;

        // Variables for the end of entity mark.
        internal bool m_chunked;
        internal long m_contentLength;
        internal bool m_shouldClose;

        // The web status.
        internal WebExceptionStatus m_status;
        //here is the error message string associated with the status code.
        //Its used for an exception message.
        internal string m_exceptionMessage;
        //here's the inner exception from the parse (for the WebException)
        internal Exception m_innerException;
    }

    /// <summary>
    /// Contains known HTTP header names.
    /// </summary>
    public class HttpKnownHeaderNames
    {
        /// <summary>The <b>Cache-Control</b> HTTP header.</summary>
        public const string CacheControl = "Cache-Control";
        /// <summary>The <b>Connection</b> HTTP header.</summary>
        public const string Connection = "Connection";
        /// <summary>The <b>Date</b> HTTP header.</summary>
        public const string Date = "Date";
        /// <summary>The <b>Keep-Alive</b> HTTP header.</summary>
        public const string KeepAlive = "Keep-Alive";
        /// <summary>The <b>Pragma</b> HTTP header.</summary>
        public const string Pragma = "Pragma";
        /// <summary>The <b>Proxy-Connection</b> HTTP header.</summary>
        public const string ProxyConnection = "Proxy-Connection";
        /// <summary>The <b>Trailer</b> HTTP header.</summary>
        public const string Trailer = "Trailer";
        /// <summary>The <b>Transfer-Encoding</b> HTTP header.</summary>
        public const string TransferEncoding = "Transfer-Encoding";
        /// <summary>The <b>Upgrade</b> HTTP header.</summary>
        public const string Upgrade = "Upgrade";
        /// <summary>The <b>Via</b> HTTP header.</summary>
        public const string Via = "Via";
        /// <summary>The <b>Warning</b> HTTP header.</summary>
        public const string Warning = "Warning";
        /// <summary>The <b>Content-Length</b> HTTP header.</summary>
        public const string ContentLength = "Content-Length";
        /// <summary>The <b>Content-Type</b> HTTP header.</summary>
        public const string ContentType = "Content-Type";
        /// <summary>The <b>Content-ID</b> HTTP header.</summary>
        public const string ContentID = "Content-ID";
        /// <summary>The <b>Content-Encoding</b> HTTP header.</summary>
        public const string ContentEncoding = "Content-Encoding";
        /// <summary>The <b>Content-Transfer-Encoding</b> HTTP header.</summary>
        public const string ContentTransferEncoding = "Content-Transfer-Encoding";
        /// <summary>The <b>Content-Language</b> HTTP header.</summary>
        public const string ContentLanguage = "Content-Language";
        /// <summary>The <b>Content-Location</b> HTTP header.</summary>
        public const string ContentLocation = "Content-Location";
        /// <summary>The <b>Content-Range</b> HTTP header.</summary>
        public const string ContentRange = "Content-Range";
        /// <summary>The <b>Expires</b> HTTP header.</summary>
        public const string Expires = "Expires";
        /// <summary>The <b>Last-Modified</b> HTTP header.</summary>
        public const string LastModified = "Last-Modified";
        /// <summary>The <b>Age</b> HTTP header.</summary>
        public const string Age = "Age";
        /// <summary>The <b>Location</b> HTTP header.</summary>
        public const string Location = "Location";
        /// <summary>The <b>Proxy-Authenticate</b> HTTP header.</summary>
        public const string ProxyAuthenticate = "Proxy-Authenticate";
        /// <summary>The <b>Retry-After</b> HTTP header.</summary>
        public const string RetryAfter = "Retry-After";
        /// <summary>The <b>Server</b> HTTP header.</summary>
        public const string Server = "Server";
        /// <summary>The <b>Set-Cookie</b> HTTP header.</summary>
        public const string SetCookie = "Set-Cookie";
        /// <summary>The <b>Set-Cookie2</b> HTTP header.</summary>
        public const string SetCookie2 = "Set-Cookie2";
        /// <summary>The <b>Vary</b> HTTP header.</summary>
        public const string Vary = "Vary";
        /// <summary>The <b>WWW-Authenticate</b> HTTP header.</summary>
        public const string WWWAuthenticate = "WWW-Authenticate";
        /// <summary>The <b>Accept</b> HTTP header.</summary>
        public const string Accept = "Accept";
        /// <summary>The <b>Accept-Charset</b> HTTP header.</summary>
        public const string AcceptCharset = "Accept-Charset";
        /// <summary>The <b>Accept-Encoding</b> HTTP header.</summary>
        public const string AcceptEncoding = "Accept-Encoding";
        /// <summary>The <b>Accept-Language</b> HTTP header.</summary>
        public const string AcceptLanguage = "Accept-Language";
        /// <summary>The <b>Authorization</b> HTTP header.</summary>
        public const string Authorization = "Authorization";
        /// <summary>The <b>Cookie</b> HTTP header.</summary>
        public const string Cookie = "Cookie";
        /// <summary>The <b>Cookie2</b> HTTP header.</summary>
        public const string Cookie2 = "Cookie2";
        /// <summary>The <b>Expect</b> HTTP header.</summary>
        public const string Expect = "Expect";
        /// <summary>The <b>From</b> HTTP header.</summary>
        public const string From = "From";
        /// <summary>The <b>Host</b> HTTP header.</summary>
        public const string Host = "Host";
        /// <summary>The <b>If-Match</b> HTTP header.</summary>
        public const string IfMatch = "If-Match";
        /// <summary>The <b>If-Modified-Since</b> HTTP header.</summary>
        public const string IfModifiedSince = "If-Modified-Since";
        /// <summary>The <b>If-None-Match</b> HTTP header.</summary>
        public const string IfNoneMatch = "If-None-Match";
        /// <summary>The <b>If-Range</b> HTTP header.</summary>
        public const string IfRange = "If-Range";
        /// <summary>The <b>If-Unmodified-Since</b> HTTP header.</summary>
        public const string IfUnmodifiedSince = "If-Unmodified-Since";
        /// <summary>The <b>Max-Forwards</b> HTTP header.</summary>
        public const string MaxForwards = "Max-Forwards";
        /// <summary>The <b>Proxy-Authorization</b> HTTP header.</summary>
        public const string ProxyAuthorization = "Proxy-Authorization";
        /// <summary>The <b>Referer</b> HTTP header.</summary>
        public const string Referer = "Referer";
        /// <summary>The <b>Range</b> HTTP header.</summary>
        public const string Range = "Range";
        /// <summary>The <b>User-Agent</b> HTTP header.</summary>
        public const string UserAgent = "User-Agent";
        /// <summary>The <b>Content-MD5</b> HTTP header.</summary>
        public const string ContentMD5 = "Content-MD5";
        /// <summary>The <b>ETag</b> HTTP header.</summary>
        public const string ETag = "ETag";
        /// <summary>The <b>TE</b> HTTP header.</summary>
        public const string TE = "TE";
        /// <summary>The <b>Allow</b> HTTP header.</summary>
        public const string Allow = "Allow";
        /// <summary>The <b>Accept-Ranges</b> HTTP header.</summary>
        public const string AcceptRanges = "Accept-Ranges";
        /// <summary>The <b>MIME-Version</b> HTTP header.</summary>
        public const string MimeVersion  = "MIME-Version";
    }

    /// <summary>
    /// TBD
    /// </summary>
    internal class HttpKnownHeaderValues
    {
        /// <summary>TBD</summary>
        public const string close = "close";
    }

    /*
    File:      httpreq.cs

    Summary:   Basic HTTP Protocol support for HttpWeb request Class.
             Contains the implimention of various HTTP primitives.

    Classes:   HttpWebReques

    Functions:

    ----------------------------------------------------------------------------
    This file is part of the Microsoft COM+ Netclasses.

    Copyright (C) 1998-1999 Microsoft Corporation.  All rights reserved.
    ==========================================================================+*/

    //      - seperate HTTP header names/header data
    //      - improve/check var/func naming
    //      - stress parsering cases
    //      - Chunked transfer needs a better algorithm, to prevent over copying
    //      - keep-alive

    /// <summary>
    /// Represents the method that notifies callers when a continue response is
    /// received by the client.
    /// </summary>
    /// <param name="StatusCode">The numeric value of the HTTP status from the
    /// server.</param>
    /// <param name="httpHeaders">The headers returned with the 100-continue
    /// response from the server.</param>
    public delegate void HttpContinueDelegate(int StatusCode, WebHeaderCollection httpHeaders);

    /// <summary>
    /// Controls the way an entity body is posted.
    /// </summary>
    internal enum HttpWriteMode
    {
        Chunked = 1,
        Write = 2,
        None = 0,
        Prebuffer = 3
    }

    /// <summary>
    /// Known Verbs are verbs that require special handling.
    /// </summary>
    internal class KnownVerbs
    {

        // This is a placeholder for Verb properties.  The following two bools can most likely be
        // combined into a single Enum type.  And the Verb can be incorporated.

        internal struct HttpVerb
        {
            // require content body to be sent
            internal bool m_RequireContentBody;
            // not allowed to send content body
            internal bool m_ContentBodyNotAllowed;
            // special semantics for a connect request
            internal bool m_ConnectRequest;
            // response will not have content body
            internal bool m_ExpectNoContentResponse;

            /*
             * XXX
             * * Wed 10/10/2001
             * This should only be used by KnownVerbs
             * */
            internal string m_name;
            internal HttpVerb(string name, bool RequireContentBody, bool ContentBodyNotAllowed, bool ConnectRequest, bool ExpectNoContentResponse)
            {

                m_name = name;
                m_RequireContentBody = RequireContentBody;
                m_ContentBodyNotAllowed = ContentBodyNotAllowed;
                m_ConnectRequest = ConnectRequest;
                m_ExpectNoContentResponse = ExpectNoContentResponse;
            }
        }

        // Force an an init, before we use them
        private static HttpVerb[] m_knownVerbs;
        static KnownVerbs()
        {
            m_knownVerbs = new HttpVerb[5];
            m_knownVerbs[0] = new HttpVerb("GET", false, true, false, false);
            m_knownVerbs[1] = new HttpVerb("POST", true, false, false, false);
            m_knownVerbs[2] = new HttpVerb("HEAD", false, true, false, true);
            m_knownVerbs[3] = new HttpVerb("PUT", true, false, false, false);
            /*
             * XXX
             * * Mon 02/25/2002
             * I've changed this from the desktop.  There is no entity response
             * in a connect request.  It won't be there, and don't close it.
             * */
            m_knownVerbs[4] = new HttpVerb("CONNECT", false, true, true, true);

            // default Verb
            DefaultVerb = new HttpVerb("", false, false, false, false);
        }

        // default verb, contains default properties for an unidentifable verb.
        private static HttpVerb DefaultVerb;

        internal static HttpVerb GetHttpVerbType(String name)
        {
            for (int i = 0; i < m_knownVerbs.Length; ++i)
            {
                HttpVerb v = m_knownVerbs[i];
                if (0 == string.Compare(v.m_name, name))
                    return v;
            }

            return DefaultVerb;
        }
    }

    /// <summary>
    ///  A collection of utility functions for HTTP usage.
    /// </summary>
    internal class HttpProtocolUtils
    {
        private HttpProtocolUtils()
        {
        }

        /// <summary>
        /// Parse String to DateTime format.
        /// </summary>
        /// <param name="S">String with date.</param>
        /// <returns>DateTime object that represent the same value as in input string.</returns>
        internal static DateTime
        string2date(String S)
        {
            DateTime dtOut;

            if (HttpDateParse.ParseHttpDate(
                                           S,
                                           out dtOut))
            {
                return dtOut;
            }
            else
            {
                throw new Exception("Invalid Date in HTTP header");
            }

        }
    }

} // namespace System.Net


