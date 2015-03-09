////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace System.Net
{

    /// <summary>
    /// Defines the HTTP version numbers that are supported by the
    /// <see cref="System.Net.HttpWebRequest"/> and
    /// <see cref="System.Net.HttpWebResponse"/> classes.
    /// </summary>
    public class HttpVersion
    {

        /// <summary>
        /// Defines a <see cref="System.Version"/> instance for HTTP 1.0.
        /// </summary>
        public static readonly Version Version10 = new Version(1, 0);

        /// <summary>
        /// Defines a <see cref="System.Version"/> instance for HTTP 1.1.
        /// </summary>
        public static readonly Version Version11 = new Version(1, 1);

    } // class HttpVersion

} // namespace System.Net


