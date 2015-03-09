////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("System.Net.Security")]

namespace Microsoft.SPOT.Net.Security
{
    [FlagsAttribute]
    public enum SslProtocols
    {
        None = 0x00,
        //SSLv2   = 0x04,  // NO longer supported (obsolete and insecure)
        SSLv3 = 0x08,
        TLSv1 = 0x10,
        Default = SSLv3 | TLSv1,
    }

    public enum SslVerification
    {
        NoVerification = 1,
        VerifyPeer = 2,
        CertificateRequired = 4,
        VerifyClientOnce = 8,
    }

    internal static class SslNative
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureServerInit(int sslProtocols, int sslCertVerify, X509Certificate certificate, X509Certificate[] ca);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureClientInit(int sslProtocols, int sslCertVerify, X509Certificate certificate, X509Certificate[] ca);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void UpdateCertificates(int contextHandle, X509Certificate certificate, X509Certificate[] ca);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void SecureAccept(int contextHandle, object socket);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void SecureConnect(int contextHandle, string targetHost, object socket);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureRead(object socket, byte[] buffer, int offset, int size, int timeout_ms);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureWrite(object socket, byte[] buffer, int offset, int size, int timeout_ms);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureCloseSocket(object socket);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int ExitSecureContext(int contextHandle);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int DataAvailable(object socket);
    }
}


