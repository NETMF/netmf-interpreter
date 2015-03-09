////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Makes a request to a Uniform Resource Identifier (URI). This is an
    /// abstract class.
    /// </summary>
    /// <remarks>
    /// This is the base class of all Web resource/protocol objects.  This class
    /// provides common methods, data and proprties for making the top-level
    /// request.
    /// </remarks>
    public abstract class WebRequest : MarshalByRefObject, IDisposable
    {
        internal const int DefaultTimeout = 100000; // default timeout is 100 seconds
        // (ASP .NET is 90 seconds)

        // Lock to syncronize update of s_PrefixList
        private static object g_listLock = new object();
        // List of WebRequestPrefixElement that keeps prefix ( string ) and
        // IWebRequestCreate
        private static ArrayList s_PrefixList = new ArrayList();

        private static IWebProxy s_defaultProxy = null;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="System.Net.WebRequest"/> class.
        /// </summary>
        protected WebRequest()
        {

        }

        ~WebRequest()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
        

        /// <summary>
        /// When overridden in a descendant class, gets or sets the protocol
        /// method to use in this request.
        /// </summary>
        /// <remarks>
        /// This property gets or sets the verb to this request, such as GET or
        /// POST for HTTP.
        /// </remarks>
        /// <value>The protocol method to use in this request.</value>
        public virtual string Method
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// When overridden in a descendant class, gets the URI of the Internet
        /// resource associated with the request.
        /// </summary>
        /// <remarks>
        /// This property is read-only, since the Uri can be specified only on
        /// creation.
        /// </remarks>
        /// <value>A <itemref>Uri</itemref> representing the resource associated
        /// with the request.
        /// </value>
        public virtual Uri RequestUri
        {                            // read-only
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// When overridden in a descendant class, gets or sets the name of the
        /// connection group for the request.
        /// </summary>
        /// <remarks>
        /// This property serves as a way of grouping connections.
        /// </remarks>
        /// <value>The name of the connection group for the request.</value>
        public virtual string ConnectionGroupName
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// When overridden in a descendant class, gets or sets the collection
        /// of header name/value pairs associated with the request.
        /// </summary>
        /// <value>A <itemref>WebHeaderCollection</itemref> containing the
        /// header name/value pairs associated with this request.</value>
        public virtual WebHeaderCollection Headers
        {
            // read-only
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// When overridden in a descendant class, gets or sets the content
        /// length of the request data being sent.
        /// </summary>
        /// <remarks>
        /// The content length is the length of the message with the verb.
        /// It is useful only with verbs that actually support a message, such
        /// as POST; it is not used for the GET verb.
        /// </remarks>
        /// <value>The number of bytes of request data being sent.</value>
        public virtual long ContentLength
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// When overridden in a descendant class, gets or sets the content type
        /// of the request data being sent.
        /// </summary>
        /// <remarks>
        /// The content length is the length of the message with the verb.
        /// It is useful only with verbs that actually support a message, such
        /// as POST; it is not used for the GET verb.
        /// </remarks>
        /// <value>The content type of the request data.</value>
        public virtual string ContentType
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the length of time, in milliseconds, before the request
        /// times out.
        /// </summary>
        /// <value>The length of time, in milliseconds, until the request times
        /// out, or the value Timeout.Infinite to indicate that the request does
        /// not time out. The default value is defined by the descendant
        /// class.</value>
        public virtual int Timeout
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the global HTTP proxy.
        /// The DefaultWebProxy property determines the default proxy that all WebRequest instances use if the request 
        /// supports proxies and no proxy is set explicitly using the Proxy property. Proxies are currently supported 
        /// by HttpWebRequest.
        /// </summary>
        public static IWebProxy DefaultWebProxy 
        {
            get
            {
                return s_defaultProxy;
            }

            set
            {
                s_defaultProxy = value;
            }
        }

        /// <summary>
        /// When overridden in a descendant class, gets or sets the network 
        /// proxy to use to access this Internet resource.
        /// </summary>
        /// <value>The <itemref>IWebProxy</itemref> to use to access the
        /// Internet resource.</value>
        public virtual IWebProxy Proxy
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// When overridden in a descendant class, returns a
        /// <itemref>Stream</itemref> for writing data to the Internet resource.
        /// </summary>
        /// <returns>A <itemref>Stream</itemref> for writing data to the
        /// Internet resource.</returns>
        public virtual Stream GetRequestStream()
        {
            // DataStream may need to be extended to URLDataStream or somesuch.
            // We might need to be able to get the data available. This should
            // be a method of the stream, not of the net classes. Also, we need
            // to know whether the stream is seekable. Only streams via cache
            // and via socket with Content-Length are seekable.

            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a descendant class, returns a response to an
        /// Internet request.
        /// </summary>
        /// <returns>A <itemref>WebResponse</itemref> containing the response to
        /// the Internet request.</returns>
        public virtual WebResponse GetResponse()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Initializes a new <itemref>WebRequest</itemref> instance for the
        /// specified URI scheme, such as http://, https://, or file://.
        /// </summary>
        /// <param name="requestUriString">The URI that identifies the Internet
        /// resource.</param>
        /// <return>Newly created <itemref>WebRequest</itemref>.
        /// A WebRequest descendant for the specific URI scheme.
        /// </return>
        /// <remarks>
        /// This is the main creation routine. The specified Uri is looked up
        /// in the prefix match table, and the appropriate handler is invoked to
        /// create the object.
        /// </remarks>
        public static WebRequest Create(string requestUriString)
        {
            return CreateInternal(new Uri(requestUriString));
        }

        /// <summary>
        /// Creates a <itemref>WebRequest</itemref>.
        /// </summary>
        /// <param name="requestUri">A <see cref="System.Uri"/> containing the
        /// URI of the requested resource.</param>
        /// <return>A <itemref>WebRequest</itemref> descendant for the specified
        /// URI scheme.</return>
        /// <remarks>
        /// This is the main creation routine. The specified Uri is looked up
        /// in the prefix match table, and the appropriate handler is invoked to
        /// create the object.
        /// </remarks>
        public static WebRequest Create(Uri requestUri)
        {
            return CreateInternal(requestUri);
        }

        /// <summary>
        /// Registers a <itemref>WebRequest</itemref> descendant for the
        /// specified URI.
        /// </summary>
        /// <param name="prefix">The complete URI or URI prefix that the
        /// <itemref>WebRequest</itemref> descendant services.</param>
        /// <param name="creator">The create method that the
        /// <itemref>WebRequest</itemref> calls to create the
        /// <itemref>WebRequest</itemref> descendant.</param>
        /// <returns><itemref>true</itemref>.</returns>
        public static bool RegisterPrefix(string prefix,
                                  IWebRequestCreate creator)
        {
            if (prefix == null || creator == null) { throw new ArgumentNullException(); }

            // Changes prefix to lower becuase it is case insensitive.
            prefix = prefix.ToLower();
            lock (g_listLock)
            {
                // Iterate over list of prefixes and checks if this one is
                // already present.
                for (int i = 0; i < s_PrefixList.Count; i++)
                {
                    if (((WebRequestPrefixElement)s_PrefixList[i]).Prefix == prefix)
                    {
                        return false;
                    }
                }

                // This is a new prefix, add it.
                s_PrefixList.Add(new WebRequestPrefixElement(prefix, creator));
            }

            return true;
        }

        private static int ComparePrefixString(string Url, string prefix, int prefixLen)
        {
            for (int i = 0; i < prefixLen; i++)
            {
                if (Url[i] != prefix[i])
                {
                    return Url[i] < prefix[i] ? -1 : 1;
                }
            }

            // Actually the URL starts by prefix.
            return 0;
        }

        private static WebRequest CreateInternal(Uri requestUri)
        {
            if (requestUri == null) { throw new ArgumentNullException(); }

            // Makes LookupUri lowercase since we need case-insensitive compare
            // with prefix
            string lookupUri = requestUri.AbsoluteUri.ToLower();
            int lookupUriLent = lookupUri.Length;

            // Walk down the list of prefixes.
            int prefixListCount = s_PrefixList.Count;
            for (int i = 0; i < prefixListCount; i++)
            {
                WebRequestPrefixElement Current = (WebRequestPrefixElement)s_PrefixList[i];

                // See if this prefix is short enough.
                int prefixLen = Current.Prefix.Length;
                if (lookupUriLent >= prefixLen)
                {
                    // It is. See if these match.
                    if (ComparePrefixString(lookupUri, Current.Prefix, prefixLen) == 0)
                    {
                        return Current.Creator.Create(requestUri);
                    }
                }
            }

            throw new NotSupportedException();
        }
    } // class WebRequest

} // namespace System.Net


