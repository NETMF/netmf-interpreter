////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Net.NetworkInformation
{
    public enum NetworkInterfaceType
    {
        Unknown = 1,
        Ethernet = 6,
        Wireless80211 = 71,
    }

    public class NetworkInterface
    {
        //set update flags...
        private const int UPDATE_FLAGS_DNS = 0x1;
        private const int UPDATE_FLAGS_DHCP = 0x2;
        private const int UPDATE_FLAGS_DHCP_RENEW = 0x4;
        private const int UPDATE_FLAGS_DHCP_RELEASE = 0x8;
        private const int UPDATE_FLAGS_MAC = 0x10;

        private const uint FLAGS_DHCP = 0x1;
        private const uint FLAGS_DYNAMIC_DNS = 0x2;

        [FieldNoReflection]
        private readonly int _interfaceIndex;

        private uint _flags;
        private uint _ipAddress;
        private uint _gatewayAddress;
        private uint _subnetMask;
        private uint _dnsAddress1;
        private uint _dnsAddress2;
        private NetworkInterfaceType _networkInterfaceType;
        private byte[] _macAddress;

        protected NetworkInterface(int interfaceIndex)
        {
            this._interfaceIndex = interfaceIndex;
            _networkInterfaceType = NetworkInterfaceType.Unknown;
        }

        public static NetworkInterface[] GetAllNetworkInterfaces()
        {
            int count = GetNetworkInterfaceCount();
            NetworkInterface[] ifaces = new NetworkInterface[count];

            for (uint i = 0; i < count; i++)
            {
                ifaces[i] = GetNetworkInterface(i);
            }

            return ifaces;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static int GetNetworkInterfaceCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static NetworkInterface GetNetworkInterface(uint interfaceIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void InitializeNetworkInterfaceSettings();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void UpdateConfiguration(int updateType);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern uint IPAddressFromString(string ipAddress);

        private string IPAddressToString(uint ipAddress)
        {
            if(SystemInfo.IsBigEndian)
            {
                return string.Concat(
                                ((ipAddress >> 24) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 16) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 8) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 0) & 0xFF).ToString()
                                );
            }
            else
            {
                return string.Concat(
                                ((ipAddress >> 0) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 8) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 16) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 24) & 0xFF).ToString()
                                );
             }
        }

        public void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress)
        {
            try
            {
                _ipAddress = IPAddressFromString(ipAddress);
                _subnetMask = IPAddressFromString(subnetMask);
                _gatewayAddress = IPAddressFromString(gatewayAddress);
                _flags &= ~FLAGS_DHCP;

                UpdateConfiguration(UPDATE_FLAGS_DHCP);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public void EnableDhcp()
        {
            try
            {
                _flags |= FLAGS_DHCP;
                UpdateConfiguration(UPDATE_FLAGS_DHCP);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public void EnableStaticDns(string[] dnsAddresses)
        {
            if (dnsAddresses == null || dnsAddresses.Length == 0 || dnsAddresses.Length > 2)
            {
                throw new ArgumentException();
            }

            uint[] addresses = new uint[2];

            int iAddress = 0;
            for (int i = 0; i < dnsAddresses.Length; i++)
            {
                uint address = IPAddressFromString(dnsAddresses[i]);

                addresses[iAddress] = address;

                if (address != 0)
                {
                    iAddress++;
                }
            }

            try
            {
                _dnsAddress1 = addresses[0];
                _dnsAddress2 = addresses[1];

                _flags &= ~FLAGS_DYNAMIC_DNS;

                UpdateConfiguration(UPDATE_FLAGS_DNS);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public void EnableDynamicDns()
        {
            try
            {
                _flags |= FLAGS_DYNAMIC_DNS;

                UpdateConfiguration(UPDATE_FLAGS_DNS);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public string IPAddress
        {
            get { return IPAddressToString(_ipAddress); }
        }

        public string GatewayAddress
        {
            get { return IPAddressToString(_gatewayAddress); }
        }

        public string SubnetMask
        {
            get { return IPAddressToString(_subnetMask); }
        }

        public bool IsDhcpEnabled
        {
            get { return (_flags & FLAGS_DHCP) != 0; }
        }

        public bool IsDynamicDnsEnabled
        {
            get
            {
                return (_flags & FLAGS_DYNAMIC_DNS) != 0;
            }
        }

        public string[] DnsAddresses
        {
            get
            {
                ArrayList list = new ArrayList();

                if (_dnsAddress1 != 0)
                {
                    list.Add(IPAddressToString(_dnsAddress1));
                }

                if (_dnsAddress2 != 0)
                {
                    list.Add(IPAddressToString(_dnsAddress2));
                }

                return (string[])list.ToArray(typeof(string));
            }
        }

        private void ReloadSettings()
        {
            Thread.Sleep(100);
            InitializeNetworkInterfaceSettings();
        }

        public void ReleaseDhcpLease()
        {
            try
            {
                UpdateConfiguration(UPDATE_FLAGS_DHCP_RELEASE);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public void RenewDhcpLease()
        {
            try
            {
                UpdateConfiguration(UPDATE_FLAGS_DHCP_RELEASE | UPDATE_FLAGS_DHCP_RENEW);
            }
            finally
            {
                ReloadSettings();
            }
        }

        public byte[] PhysicalAddress
        {
            get { return _macAddress; }
            set
            {
                try
                {
                    _macAddress = value;
                    UpdateConfiguration(UPDATE_FLAGS_MAC);
                }
                finally
                {
                    ReloadSettings();
                }
            }
        }

        public NetworkInterfaceType NetworkInterfaceType
        {
            get { return _networkInterfaceType; }
        }
    }
}


