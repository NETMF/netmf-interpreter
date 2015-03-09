////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Defines status codes for the <see cref="System.Net.WebException"/>
    /// class.
    /// </summary>
    public enum WebExceptionStatus
    {
        /// <summary>No error was encountered.</summary>
        Success = 0,
        /// <summary>The name resolver service could not resolve the host name.
        /// </summary>
        NameResolutionFailure = 1,
        /// <summary>The remote service point could not be contacted at the
        /// transport level.</summary>
        ConnectFailure = 2,
        /// <summary>A complete response was not received from the remote
        /// server.</summary>
        ReceiveFailure = 3,
        /// <summary>A complete request could not be sent to the remote
        /// server.</summary>
        SendFailure = 4,
        /// <summary>The request was a piplined request and the connection was
        /// closed before the response was received.</summary>
        PipelineFailure = 5,
        /// <summary>The request was canceled or an unclassifiable error
        /// occurred.  This is the default value for
        /// <see cref="System.Net.WebException.Status"/>.</summary>
        RequestCanceled = 6,
        /// <summary>The response received from the server was complete but
        /// indicated a protocol-level error.  For example, an HTTP protocol
        /// error such as 401 Access Denied would use this status.</summary>
        ProtocolError = 7,
        /// <summary>The connection was prematurely closed.</summary>
        ConnectionClosed = 8,
        /// <summary>A server certificate could not be validated.</summary>
        TrustFailure = 9,
        /// <summary>An error occurred while establishing a connection using
        /// SSL.</summary>
        SecureChannelFailure = 10,
        /// <summary>The server response was not a valid HTTP
        /// response.</summary>
        ServerProtocolViolation = 11,
        /// <summary>The connection for a request that specifies the Keep-alive
        /// header was closed unexpectedly.</summary>
        KeepAliveFailure = 12,
        /// <summary>An internal asynchronous request is pending.</summary>
        Pending = 13,
        /// <summary>No response was received during the time-out period for a
        /// request.</summary>
        Timeout = 14,
        /// <summary>The name resolver service could not resolve the proxy host
        /// name.</summary>
        ProxyNameResolutionFailure = 15
    } // enum WebStatus

} // namespace System.Net


