////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Net;
using System.Globalization;

namespace System.Net
{

    /// <summary>
    /// Contains HTTP proxy settings for the <see cref="System.Net.WebRequest"/>
    /// class.
    /// </summary>
    public class WebProxy : IWebProxy
    {

        // true means DO NOT use proxy on local connections.
        // false means use proxy for local network connections.

        private bool _BypassOnLocal;
        private Uri _ProxyAddress;  // Uri of proxy itself

        /// <summary>
        /// Initializes an empty instance of the WebProxy class.
        /// </summary>
        /// <remarks>
        /// The URI of the proxy can be set later, using the
        /// <see cref="System.Net.WebProxy.Address"/> property.
        /// </remarks>
        public WebProxy()
            : this((Uri)null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <itemref>WebProxy</itemref> class
        /// from the specified <see cref="System.Uri"/> instance.
        /// </summary>
        /// <param name="Address">A <itemref>Uri</itemref> instance that
        /// contains the address of the proxy server.</param>
        public WebProxy(Uri Address)
            : this(Address, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <itemref>WebProxy</itemref> class
        /// with the specified <see cref="System.Uri"/> instance and bypass
        /// setting.
        /// </summary>
        /// <param name="Address">A <itemref>Uri</itemref> instance that
        /// contains the address of the proxy server.</param>
        /// <param name="BypassOnLocal">Indicates whether to bypass the WebProxy
        /// on local network addresses.</param>
        public WebProxy(Uri Address, bool BypassOnLocal)
        {
            _ProxyAddress = Address;
            _BypassOnLocal = BypassOnLocal;
        }

        /// <summary>
        /// Initializes a new instance of the <itemref>WebProxy</itemref> class
        /// with the specified host and port number.
        /// </summary>
        /// <param name="Host">The name of the proxy host, such as: contoso</param>
        /// <param name="Port">The port number on the host to use, such as:
        /// 80</param>
        /// <remarks>
        /// The <itemref>WebProxy</itemref> instance is initialized with the
        /// <see cref="System.Net.WebProxy.Address"/> property set
        /// to a <see cref="System.Uri"/> instance of the form: http://Host:Port
        /// </remarks>
        public WebProxy(string Host, int Port)
            : this(new Uri("http://" + Host + ":" + Port.ToString()), false) { }

        /// <summary>
        /// Initializes a new instance of the <itemref>WebProxy</itemref> class
        /// with the specified URI.
        /// </summary>
        /// <param name="Address">The URI address of the proxy server.</param>
        /// <remarks>
        /// The <itemref>WebProxy</itemref> instance is initialized with the
        /// <see cref="System.Net.WebProxy.Address"/> property set to a
        /// <see cref="System.Uri"/> instance containing the
        /// <itemref>Address</itemref> string.
        /// <para>
        /// For the new instance of the <itemref>WebProxy</itemref> class,
        /// "Bypass on local addresses" is set to <itemref>false</itemref>.
        /// </para>
        /// </remarks>
        public WebProxy(string Address)
            : this(CreateProxyUri(Address), false) { }

        /// <summary>
        /// Initializes a new instance of the <itemref>WebProxy</itemref> class
        /// with the specified URI and bypass setting.
        /// </summary>
        /// <param name="Address">The URI of the proxy server.</param>
        /// <param name="BypassOnLocal">Indicates whether to bypass the proxy
        /// when accessing local addresses.</param>
        public WebProxy(string Address, bool BypassOnLocal)
            : this(CreateProxyUri(Address), BypassOnLocal) { }

        /// <summary>
        /// Gets or sets the address of the proxy server.
        /// </summary>
        /// <value>A <see cref="System.Uri"/> instance that contains the address
        /// of the proxy server.</value>
        public Uri Address
        {
            get
            {
                return _ProxyAddress;
            }

            set
            {
                _ProxyAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to bypass the proxy server for local
        /// addresses.</summary>
        /// <value><itemref>true</itemref> to bypass the proxy server for local
        /// addresses; otherwise, <itemref>false</itemref>.</value>
        public bool BypassProxyOnLocal
        {
            get
            {
                return _BypassOnLocal;
            }

            set
            {
                _BypassOnLocal = value;
            }
        }

        /// <summary>
        /// Returns the proxied URI for a request.
        /// </summary>
        /// <param name="destination">The <itemref>Uri</itemref> instance of the
        /// requested Internet resource.</param>
        /// <returns>The <itemref>Uri</itemref> instance of the Internet
        /// resource, if the resource is on the bypass list; otherwise, the
        /// <itemref>Uri</itemref> instance of the proxy.
        /// </returns>
        public Uri GetProxy(Uri destination)
        {
            if (IsBypassed(destination))
            {
                return destination;
            }

            Uri proxy = _ProxyAddress;
            if (proxy != null)
            {
                return proxy;
            }

            return destination;
        }

        /// <summary>
        /// Maps a string to a Uri.
        /// </summary>
        /// <param name="Address">The Url for creation of the Uri.</param>
        /// <returns>The new Uri corresponding to the Url.</returns>
        private static Uri CreateProxyUri(string Address)
        {
            if (Address == null)
            {
                return null;
            }

            // Original code was IndexOf("://", StringComparison.Ordinal),
            // changed to IndexOf("://") we only support ASCII in .NET MF.
            if (Address.IndexOf("://") == -1)
            {
                Address = "http://" + Address;
            }

            return new Uri(Address);
        }

        /// <summary>
        /// Checks whether the supplied Uri represents a local address.
        /// </summary>
        /// <param name="host">The Uri to check.</param>
        /// <returns><itemref>true</itemref>if the address is local; otherwise,
        /// <itemref>false</itemref>.</returns>
        private bool IsLocal(Uri host)
        {
            string hostString = host.Host;
            int dot = hostString.IndexOf('.');

            if (dot == -1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Indicates whether to use the proxy server for the specified host.
        /// </summary>
        /// <param name="host">The <itemref>Uri</itemref> instance of the host
        /// to check for proxy use.</param>
        /// <returns><itemref>true</itemref> if the proxy server should not be
        /// used for the host; otherwise, <itemref>false</itemref>.</returns>
        public bool IsBypassed(Uri host)
        {

            if (host.IsLoopback)
            {
                return true; // bypass localhost from using a proxy.
            }

            if ((_ProxyAddress == null) ||
                (_BypassOnLocal && IsLocal(host)))
            {
                return true; // bypass when non .'s and no proxy on local
            }
            else
            {
                return false;
            }
        }

    }; // class WebProxy

} // namespace System.Net


