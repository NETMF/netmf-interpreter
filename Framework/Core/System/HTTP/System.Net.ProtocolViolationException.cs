////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace System.Net
{

    /// <summary>
    /// The exception that is thrown when an error is made while using a network
    /// protocol.
    /// </summary>
    public class ProtocolViolationException : InvalidOperationException
    {

        // constructors
        /// <summary>
        /// Initializes a new instance of the
        /// <itemref>ProtocolViolationException</itemref> class.
        /// </summary>
        public ProtocolViolationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <itemref>ProtocolViolationException</itemref> class with the
        /// specified message.
        /// </summary>
        /// <param name="message">The error message string.</param>
        public ProtocolViolationException(string message)
            : base(message)
        {
        }

    } // class ProtocolViolationException

} // namespace System.Net


