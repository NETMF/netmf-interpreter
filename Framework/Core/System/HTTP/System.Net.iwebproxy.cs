////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;

    /// <summary>
    /// Provides the base interface for implementing proxy access for the
    /// <see cref="System.Net.WebRequest"/> class.
    /// </summary>
    public interface IWebProxy
    {

        /// <summary>
        /// Returns the URI of a proxy.
        /// </summary>
        /// <param name="destination">The destination URI.</param>
        /// <returns>A <b>Uri</b> instance that contains the URI of the proxy
        /// used to contact <paramref name="destination"/>.</returns>
        Uri GetProxy(Uri destination);

        /// <summary>
        /// Indicates whether the proxy should not be used for the specified
        /// host.
        /// </summary>
        /// <param name="host">The host to check, to determine whether the proxy
        /// is needed to access it.</param>
        /// <returns>Whether the proxy should not be used for the specified
        /// host.</returns>
        bool IsBypassed(Uri host);
    }
}


