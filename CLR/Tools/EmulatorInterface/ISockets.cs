////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.Sockets
{
    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct TimeVal
    {
        public int tv_sec;
        public int tv_usec;
    };

    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct NetworkAdapterConfiguration
    {
        public const uint FLAGS_DHCP          = 0x00000001;
        public const uint FLAGS_DYNAMIC_DNS   = 0x00000002;

        public const uint UPDATE_DNS          = 0x00000001;
        public const uint UPDATE_DHCP         = 0x00000002;
        public const uint UPDATE_DHCP_RENEW   = 0x00000004;
        public const uint UPDATE_DHCP_RELEASE = 0x00000008;

        public uint flags;
        public uint ipaddr;
        public uint subnetmask;
        public uint gateway;
        public uint dnsServer1;
        public uint dnsServer2;
        public uint networkInterfaceType;
        public uint networkAddressLen;
        public fixed byte networkAddressBuf[64];
    }

    public interface ISocketsDriver
    {
        bool Initialize();
        bool Uninitialize();

        int socket(int family, int type, int protocol);
        int bind(int socket, IntPtr address, int addressLen);
        int connect(int socket, IntPtr address, int addressLen);
        int send(int socket, IntPtr buf, int len, int flags);
        int recv(int socket, IntPtr buf, int len, int flags);
        int close(int socket);
        int listen(int socket, int backlog);
        int accept(int socket, IntPtr address, ref int addressLen);
        int shutdown(int socket, int how);
        int getaddrinfo(IntPtr nodename, IntPtr servname, IntPtr hints, out IntPtr res);
        void freeaddrinfo(IntPtr ai);
        int ioctl(int socket, int cmd, ref int data);
        int getlasterror();
        int select(int nfds, IntPtr readfds, IntPtr writefds, IntPtr exceptfdx, ref TimeVal timeout);
        int setsockopt(int socket, int level, int optname, IntPtr optval, int optlen);
        int getsockopt(int socket, int level, int optname, IntPtr optval, ref int optlen);
        int getpeername(int socket, IntPtr name, ref int namelen);
        int getsockname(int socket, IntPtr name, ref int namelen);
        int sendto(int socket, IntPtr buf, int len, int flags, IntPtr to, ref int tolen);
        int recvfrom(int socket, IntPtr buf, int len, int flags, IntPtr from, ref int fromlen);

        uint NetworkAdapterCount { get; }
        int LoadAdapterConfiguration(uint interfaceIndex, ref NetworkAdapterConfiguration config);
        int UpdateAdapterConfiguration(uint interfaceIndex, uint updateFlags, ref NetworkAdapterConfiguration config);
    }
}
