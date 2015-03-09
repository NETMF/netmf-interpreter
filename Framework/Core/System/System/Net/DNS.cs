////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System.Text;
    using System.Collections;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Globalization;
    using Microsoft.SPOT.Hardware;

    using NativeSocket = Microsoft.SPOT.Net.SocketNative;

    public static class Dns
    {
        public static IPHostEntry GetHostEntry(string hostNameOrAddress)
        {

            //Do we need to try to pase this as an Address????
            string canonicalName;
            byte[][] addresses;

            NativeSocket.getaddrinfo(hostNameOrAddress, out canonicalName, out addresses);

            int cAddresses = addresses.Length;
            IPAddress[] ipAddresses = new IPAddress[cAddresses];
            IPHostEntry ipHostEntry = new IPHostEntry();

            for (int i = 0; i < cAddresses; i++)
            {
                byte[] address = addresses[i];

                SocketAddress sockAddress = new SocketAddress(address);

                AddressFamily family;

                if(SystemInfo.IsBigEndian)
                {
                    family = (AddressFamily)((address[0] << 8) | address[1]);
                }
                else
                {
                    family = (AddressFamily)((address[1] << 8) | address[0]);
                }
                //port address[2-3]

                if (family == AddressFamily.InterNetwork)
                {
                    //This only works with IPv4 addresses

                    uint ipAddr = (uint)((address[7] << 24) | (address[6] << 16) | (address[5] << 8) | (address[4]));

                    ipAddresses[i] = new IPAddress((long)ipAddr);
                }
            }

            ipHostEntry.hostName = canonicalName;
            ipHostEntry.addressList = ipAddresses;

            return ipHostEntry;
        }
    }
}


