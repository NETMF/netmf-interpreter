////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Management;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Debugger
{
    [Serializable]
    public class PortDefinition_Tcp : PortDefinition
    {
        public const int WellKnownPort = 26000;

        protected IPEndPoint m_ipEndPoint;

        protected string m_macAddress = "";

        private static List<PortDefinition> m_portCache = new List<PortDefinition>();
        private static DateTime m_portCacheTime = DateTime.MinValue;

        internal unsafe struct SOCK_discoveryinfo 
        {
            internal uint       ipaddr;
            internal uint       macAddressLen;
            internal fixed byte macAddressBuffer[64];    
        };

        public string MacAddress
        {
            get { return m_macAddress; }
                
        }
        

        public PortDefinition_Tcp(IPEndPoint ipEndPoint, string macAddress)
            : base(ipEndPoint.Address.ToString(), ipEndPoint.ToString())
        {
            if(!string.IsNullOrEmpty(macAddress))
            {
                m_displayName += " - (" + macAddress + ")";
            }
            m_ipEndPoint = ipEndPoint;
            m_macAddress = macAddress;
        }

        public PortDefinition_Tcp(IPEndPoint ipEndPoint)
            : this(ipEndPoint, "")
        {
            m_ipEndPoint = ipEndPoint;
        }

        public PortDefinition_Tcp(IPAddress address)
            : this(new IPEndPoint(address, WellKnownPort), "")
        {
        }

        public PortDefinition_Tcp(IPAddress address, string macAddress)
            : this(new IPEndPoint(address, WellKnownPort), macAddress)
        {
        }

        public override object UniqueId
        {
            get
            {
                return m_ipEndPoint.ToString();
            }
        }

        public static PortDefinition[] EnumeratePorts()
        {
            return EnumeratePorts(true);
        }

        public static PortDefinition[] EnumeratePorts(bool forceRefresh)
        {
            return EnumeratePorts(System.Net.IPAddress.Parse("234.102.98.44"), System.Net.IPAddress.Parse("234.102.98.45"), 26001, "DOTNETMF", 3000, 1, forceRefresh);
        }

        public static PortDefinition[] EnumeratePorts(
            System.Net.IPAddress DiscoveryMulticastAddress    ,
            System.Net.IPAddress DiscoveryMulticastAddressRecv,
            int       DiscoveryMulticastPort       ,
            string    DiscoveryMulticastToken      ,
            int       DiscoveryMulticastTimeout    ,
            int       DiscoveryTTL                 
        )
        {
            return EnumeratePorts(
                DiscoveryMulticastAddress, 
                DiscoveryMulticastAddressRecv, 
                DiscoveryMulticastPort, 
                DiscoveryMulticastToken, 
                DiscoveryMulticastTimeout, 
                DiscoveryTTL, 
                true);
        }

        public static PortDefinition[] EnumeratePorts(
            System.Net.IPAddress DiscoveryMulticastAddress    ,
            System.Net.IPAddress DiscoveryMulticastAddressRecv,
            int       DiscoveryMulticastPort       ,
            string    DiscoveryMulticastToken      ,
            int       DiscoveryMulticastTimeout    ,
            int       DiscoveryTTL                 ,
            bool      ForceRefresh
        )
        {
            PortDefinition_Tcp []ports = null;
            Dictionary<string, string> addresses = new Dictionary<string, string>();

            if (!ForceRefresh && m_portCache.Count > 0 && 
                (DateTime.Now - m_portCacheTime) < TimeSpan.FromSeconds(30))
            {
                return m_portCache.ToArray();
            }

            m_portCache.Clear();

            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        int cnt = 0;
                        int total = 0;
                        byte[] data = new byte[1024];
                        Socket sock = null;
                        Socket recv = null;

                        System.Net.IPEndPoint endPoint    = new System.Net.IPEndPoint(ip, 0);
                        System.Net.EndPoint   epRemote    = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 26001);
                        System.Net.IPEndPoint epRecv      = new System.Net.IPEndPoint(ip, DiscoveryMulticastPort);
                        System.Net.IPEndPoint epMulticast = new System.Net.IPEndPoint(DiscoveryMulticastAddress, DiscoveryMulticastPort);

                        try
                        {
                            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                            recv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                            recv.Bind(epRecv);
                            recv.ReceiveTimeout = DiscoveryMulticastTimeout;
                            sock.Bind(endPoint);

                            recv.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(DiscoveryMulticastAddressRecv, ip));
                            sock.MulticastLoopback = false;
                            sock.Ttl = (short)DiscoveryTTL;
                            sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 64);

                            // send ping
                            sock.SendTo(System.Text.Encoding.ASCII.GetBytes(DiscoveryMulticastToken), SocketFlags.None, epMulticast);

                            while (0 < (cnt = recv.ReceiveFrom(data, total, data.Length - total, SocketFlags.None, ref epRemote)))
                            {
                                addresses[((IPEndPoint)epRemote).Address.ToString()] = "";
                                total += cnt;
                                recv.ReceiveTimeout = DiscoveryMulticastTimeout / 2;
                            }

                            recv.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(DiscoveryMulticastAddressRecv));

                        }
                        // SocketException occurs in RecieveFrom if there is no data.
                        catch (SocketException)
                        {
                        }
                        finally
                        {
                            if (recv != null)
                            {
                                recv.Close();
                                recv = null;
                            }
                            if (sock != null)
                            {
                                sock.Close();
                                sock = null;
                            }
                        }

                        // use this if we need to get the MAC address of the device
                        SOCK_discoveryinfo disc = new SOCK_discoveryinfo();
                        disc.ipaddr = 0;
                        disc.macAddressLen = 0;
                        int idx = 0;
                        int c_DiscSize = Marshal.SizeOf(disc);
                        while (total >= c_DiscSize)
                        {
                            byte[] discData = new byte[c_DiscSize];
                            Array.Copy(data, idx, discData, 0, c_DiscSize);
                            GCHandle gch = GCHandle.Alloc(discData, GCHandleType.Pinned);
                            disc = (SOCK_discoveryinfo)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(SOCK_discoveryinfo));
                            gch.Free();

                            // previously we only displayed the IP address for the device, which doesn't
                            // really tell you which device you are talking to.  The MAC address should be unique.
                            // therefore we will display the MAC address in the device display name to help distinguish
                            // the devices.  
                            if (disc.macAddressLen <= 64 && disc.macAddressLen > 0)
                            {
                                IPAddress ipResp = new IPAddress((long)disc.ipaddr);

                                // only append the MAC if it matches one of the IP address we got responses from
                                if (addresses.ContainsKey(ipResp.ToString()))
                                {
                                    string strMac = "";
                                    for (int mi = 0; mi < disc.macAddressLen - 1; mi++)
                                    {
                                        unsafe
                                        {
                                            strMac += string.Format("{0:x02}-", disc.macAddressBuffer[mi]);
                                        }
                                    }
                                    unsafe
                                    {
                                        strMac += string.Format("{0:x02}", disc.macAddressBuffer[disc.macAddressLen - 1]);
                                    }

                                    addresses[ipResp.ToString()] = strMac;
                                }
                            }
                            total -= c_DiscSize;
                            idx += c_DiscSize;
                        }
                    }
                }
            }
            catch( Exception e2)
            {
                System.Diagnostics.Debug.Print(e2.ToString());
            }

            ports = new PortDefinition_Tcp[addresses.Count];
            int i = 0;

            foreach(string key in addresses.Keys)
            {
                ports[i++] = new PortDefinition_Tcp(IPAddress.Parse(key), addresses[key]);
            }

            m_portCache.AddRange(ports);
            m_portCacheTime = DateTime.Now;

            return ports;            
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public override Stream CreateStream()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.NoDelay = true;
            socket.LingerState = new LingerOption(false, 0);

            IAsyncResult asyncResult = socket.BeginConnect(m_ipEndPoint, null, null);

            if (asyncResult.AsyncWaitHandle.WaitOne(1000, false))
            {
                socket.EndConnect(asyncResult);
            }
            else
            {
                socket.Close();
                throw new IOException("Connect failed");
            }

            AsyncNetworkStream stream = new AsyncNetworkStream(socket, true);

            return stream;
        }    
    }
}
