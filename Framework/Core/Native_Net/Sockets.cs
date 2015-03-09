////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("System")]

namespace Microsoft.SPOT.Net
{
    internal static class SocketNative
    {
        public const int FIONREAD = 0x4004667F;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int socket(int family, int type, int protocol);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void bind(object socket, byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void connect(object socket, byte[] address, bool fThrowOnWouldBlock);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int send(object socket, byte[] buf, int offset, int count, int flags, int timeout_ms);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int recv(object socket, byte[] buf, int offset, int count, int flags, int timeout_ms);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int close(object socket);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void listen(object socket, int backlog);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int accept(object socket);

        //No standard non-blocking api
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void getaddrinfo(string name, out string canonicalName, out byte[][] addresses);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void shutdown(object socket, int how, out int err);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int sendto(object socket, byte[] buf, int offset, int count, int flags, int timeout_ms, byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int recvfrom(object socket, byte[] buf, int offset, int count, int flags, int timeout_ms, ref byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void getpeername(object socket, out byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void getsockname(object socket, out byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void getsockopt(object socket, int level, int optname, byte[] optval);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void setsockopt(object socket, int level, int optname, byte[] optval);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool poll(object socket, int mode, int microSeconds);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void ioctl(object socket, uint cmd, ref uint arg);
    }
}


