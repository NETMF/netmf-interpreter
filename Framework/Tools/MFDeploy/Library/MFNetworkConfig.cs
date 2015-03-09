//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using _DBG = Microsoft.SPOT.Debugger;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct HAL_NetworkConfiguration : IHAL_CONFIG_BASE
    {
        public const int c_maxMacBufferLength = 64;

        public HAL_CONFIG_BLOCK Header;
        public Int32 NetworkCount;
        public Int32 Enabled;
        public UInt32 flags;
        public UInt32 ipaddr;
        public UInt32 subnetmask;
        public UInt32 gateway;
        public UInt32 dnsServer1;
        public UInt32 dnsServer2;
        public UInt32 networkInterfaceType;
        public UInt32 macAddressLen;
        public fixed byte macAddressBuffer[c_maxMacBufferLength];

        public HAL_CONFIG_BLOCK ConfigHeader
        {
            get { return Header; }
            set { Header = value; }
        }

        public int Size
        {
            get
            {
                int size = 0;

                unsafe
                {
                    size = sizeof(HAL_NetworkConfiguration);
                }

                return size;
            }
        }
    }

    public class MFNetworkConfiguration
    {
        HAL_NetworkConfiguration m_cfg = new HAL_NetworkConfiguration();
        MFConfigHelper           m_cfgHelper;

        const uint c_SOCK_NETWORKCONFIGURATION_FLAGS_DHCP             = 1;
        const uint c_SOCK_NETWORKCONFIGURATION_INTERFACETYPE_ETHERNET = 6;

        const string c_CfgName = "NETWORK";
        const int c_ConfigTypeFlagShift = 16;

        public MFNetworkConfiguration(MFDevice dev)
        {
            m_cfg.networkInterfaceType = c_SOCK_NETWORKCONFIGURATION_INTERFACETYPE_ETHERNET;

            m_cfgHelper = new MFConfigHelper(dev);
        }

        public bool EnableDhcp 
        { 
            get { return (m_cfg.flags & c_SOCK_NETWORKCONFIGURATION_FLAGS_DHCP) != 0; } 
            set 
            {
                uint hiBits = m_cfg.flags & 0xFFFF0000;
                m_cfg.flags = value ? c_SOCK_NETWORKCONFIGURATION_FLAGS_DHCP : 0;
                m_cfg.flags |= hiBits; /// Keep this same.
            } 
        }

        public IPAddress IpAddress
        {
            get
            {
                return new IPAddress(m_cfg.ipaddr);
            }
            set
            {
                if( value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new MFInvalidNetworkAddressException();
                }

                m_cfg.ipaddr = BitConverter.ToUInt32(value.GetAddressBytes(), 0);
            }
        }
        public IPAddress SubNetMask
        {
            get
            {
                return new IPAddress(m_cfg.subnetmask);
            }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new MFInvalidNetworkAddressException();
                }
                
                m_cfg.subnetmask = BitConverter.ToUInt32(value.GetAddressBytes(), 0);
            }            
        }
        public IPAddress Gateway
        {
            get
            {
                return new IPAddress(m_cfg.gateway);
            }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new MFInvalidNetworkAddressException();
                }

                m_cfg.gateway = BitConverter.ToUInt32(value.GetAddressBytes(), 0);
            }
        }
        public IPAddress PrimaryDns
        {
            get
            {
                return new IPAddress(m_cfg.dnsServer1);
            }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new MFInvalidNetworkAddressException();
                }

                m_cfg.dnsServer1 = BitConverter.ToUInt32(value.GetAddressBytes(), 0);
            }
        }
        public IPAddress SecondaryDns
        {
            get
            {
                return new IPAddress(m_cfg.dnsServer2);
            }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new MFInvalidNetworkAddressException();
                }

                m_cfg.dnsServer2 = BitConverter.ToUInt32(value.GetAddressBytes(), 0);
            }
        }
        public byte[] MacAddress
        {
            get
            {
                uint len = m_cfg.macAddressLen;

                if (len > HAL_NetworkConfiguration.c_maxMacBufferLength)
                {
                    len = HAL_NetworkConfiguration.c_maxMacBufferLength;
                }

                byte[] data = new byte[len];

                unsafe
                {
                    fixed (byte* pData = m_cfg.macAddressBuffer)
                    {
                        for (int i = 0; i < len; i++)
                        {
                            data[i] = pData[i];
                        }
                    }
                }
                return data;
            }
            set
            {
                if (value.Length > 64)
                {
                    throw new MFInvalidMacAddressException();
                }

                unsafe
                {
                    fixed (byte* pData = m_cfg.macAddressBuffer)
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            pData[i] = value[i];
                        }
                    }
                }
                m_cfg.macAddressLen = (uint)value.Length;
            }
        }

        public int MaxMacAddressLength { get { return 64; } }

        public enum NetworkConfigType
        {
            Generic = 0,
            Wireless = 1,
        }

        public NetworkConfigType ConfigurationType
        {
            get
            {
                uint type = (m_cfg.flags >> c_ConfigTypeFlagShift) & 0xF;

                return (NetworkConfigType)(type);
            }

            set
            {
                uint type = (uint)value;
                m_cfg.flags |= ((type & 0xF) << c_ConfigTypeFlagShift);
            }
        }

        public void Load()
        {
            byte[] data = m_cfgHelper.FindConfig(c_CfgName);

            if (data != null)
            {
                m_cfg = (HAL_NetworkConfiguration)MFConfigHelper.UnmarshalData(data, typeof(HAL_NetworkConfiguration));
            }
        }

        public void Save()
        {
            m_cfg.NetworkCount = 1;
            m_cfg.Enabled = 1;            
            m_cfgHelper.WriteConfig(c_CfgName, m_cfg);
        }

        internal void SwapConfigBuffer(MFConfigHelper srcConfig)
        {
            m_cfgHelper.SwapAllConfigData(srcConfig);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct HAL_WirelessConfiguration : IHAL_CONFIG_BASE
    {
        public const int c_PassPhraseLength = 64;
        public const int c_NetworkKeyLength = 256;
        public const int c_ReKeyInternalLength = 32;
        public const int c_SSIDLength = 32;

        public HAL_CONFIG_BLOCK Header;
        public Int32 WirelessNetworkCount;
        public Int32 Enabled;
        public UInt32 WirelessFlags;
        public fixed byte PassPhrase[c_PassPhraseLength];
        public Int32 NetworkKeyLength;
        public fixed byte NetworkKey[c_NetworkKeyLength];
        public Int32 ReKeyLength;
        public fixed byte ReKeyInternal[c_ReKeyInternalLength];
        public fixed byte SSID[c_SSIDLength];

        public HAL_CONFIG_BLOCK ConfigHeader
        {
            get { return Header; }
            set { Header = value; }
        }

        public int Size
        {
            get
            {
                int size = 0;

                unsafe
                {
                    size = sizeof(HAL_WirelessConfiguration);
                }

                return size;
            }
        }        
    }

    public class MFWirelessConfiguration
    {
        private HAL_WirelessConfiguration m_cfg = new HAL_WirelessConfiguration();
        internal MFConfigHelper m_cfgHelper;
        const string c_CfgName = "WIRELESS";
        const int c_EncryptionBitShift = 4;
        const int c_RadioBitShift = 8;
        const int c_DataBitShift = 12;

        [Flags]
        public enum RadioTypes : int
        {
            a = 1,
            b = 2,
            g = 4,
            n = 8,
        }

        public MFWirelessConfiguration(MFDevice dev)
        {
            m_cfgHelper = new MFConfigHelper(dev);
        }

        public int Authentication
        {
            get
            {
                return (int)(m_cfg.WirelessFlags & 0xF);
            }

            set
            {
                m_cfg.WirelessFlags &= 0xFFFFFFF0;
                m_cfg.WirelessFlags |= ((uint)value & 0xF);
            }
        }

        public int Encryption
        {
            get
            {
                return (int)((m_cfg.WirelessFlags >> c_EncryptionBitShift) & 0xF);
            }

            set
            {
                m_cfg.WirelessFlags &= 0xFFFFFF0F;
                m_cfg.WirelessFlags |= (((uint)value & 0xF) << c_EncryptionBitShift);
            }
        }

        public int Radio
        {
            get
            {
                return (int)((m_cfg.WirelessFlags >> c_RadioBitShift) & 0xF);
            }

            set
            {
                m_cfg.WirelessFlags &= 0xFFFFF0FF;
                m_cfg.WirelessFlags |= (((uint)value & 0xF) << c_RadioBitShift);
            }
        }

        public bool UseEncryption
        {
            get
            {
                return (((m_cfg.WirelessFlags >> c_DataBitShift) & 0x1) != 0);
            }

            set
            {
                m_cfg.WirelessFlags &= 0xFFFF0FFF;
                if (value)
                    m_cfg.WirelessFlags |= (1 << c_DataBitShift);
                else
                    m_cfg.WirelessFlags &= ~((uint)(1 << c_DataBitShift));
            }
        }

        public string PassPhrase
        {
            get
            {
                string passPhrase = "";
                unsafe
                {
                    bool fEmpty = true;

                    fixed (byte* data = m_cfg.PassPhrase)
                    {
                        if (UseEncryption)
                        {
                            byte[] interimData = new byte[HAL_WirelessConfiguration.c_PassPhraseLength];
                            int i = 0;
                            for (i = 0; i < HAL_WirelessConfiguration.c_PassPhraseLength; i++)
                            {
                                interimData[i] = data[i];

                                if (interimData[i] != 0xFF)
                                {
                                    fEmpty = false;
                                }
                            }

                            if(!fEmpty)
                            {
                                interimData = Decrypt(interimData);

                                for (i = 0; i < HAL_WirelessConfiguration.c_PassPhraseLength; i++)
                                {
                                    if (interimData[i] == 0)
                                        break;
                                }

                                passPhrase = new string(System.Text.UTF8Encoding.UTF8.GetChars(interimData, 0, i));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < HAL_WirelessConfiguration.c_PassPhraseLength; i++)
                            {
                                if (data[i] != 0xFF)
                                {
                                    fEmpty = false;
                                    break;
                                }
                            }

                            if (!fEmpty)
                            {
                                passPhrase = new string((sbyte*)data);
                            }
                        }
                    }
                }

                return passPhrase;
            }

            set
            {
                string passPhrase = value;

                if (passPhrase.Length >= HAL_WirelessConfiguration.c_PassPhraseLength)
                    throw new ArgumentOutOfRangeException();

                foreach (char c in passPhrase)
                {
                    if ((int)c > 255)
                        throw new ArgumentException("Pass phrase cannot have wide characters.");
                }

                unsafe
                {
                    fixed (byte* data = m_cfg.PassPhrase)
                    {
                        byte[] passData = System.Text.UTF8Encoding.UTF8.GetBytes(passPhrase);

                        byte[] interimData = new byte[HAL_WirelessConfiguration.c_PassPhraseLength];

                        int i = Math.Min(HAL_WirelessConfiguration.c_PassPhraseLength - 1, passData.Length);

                        Array.Copy(passData, interimData, i);

                        /// We always need at least one NULL terminator. length < HAL_WirelessConfiguration.c_PassPhraseLength. 
                        for (; i < HAL_WirelessConfiguration.c_PassPhraseLength; i++)
                        {
                            interimData[i] = 0;
                        }

                        if (UseEncryption)
                        {
                            interimData = Encrypt(interimData);
                        }

                        for (i = 0; i < HAL_WirelessConfiguration.c_PassPhraseLength; i++)
                        {
                            data[i] = interimData[i];
                        }
                    }
                }
            }
        }

        public int NetworkKeyLength
        {
            get
            {
                return m_cfg.NetworkKeyLength;
            }
            set
            {
                m_cfg.NetworkKeyLength = value;
            }
        }

        public string NetworkKey
        {
            get
            {
                unsafe
                {
                    fixed (byte* data = m_cfg.NetworkKey)
                    {
                        return ByteToString(data, m_cfg.NetworkKeyLength, UseEncryption);
                    }
                }
            }

            set
            {
                unsafe
                {
                    fixed (byte* data = m_cfg.NetworkKey)
                    {
                        StringToByte(value, data, HAL_WirelessConfiguration.c_NetworkKeyLength, UseEncryption);
                    }
                }
            }
        }

        public int ReKeyLength
        {
            get
            {
                return m_cfg.ReKeyLength;
            }
            set
            {
                m_cfg.ReKeyLength = value;
            }
        }

        public string ReKeyInternal
        {
            get
            {
                unsafe
                {
                    fixed (byte* data = m_cfg.ReKeyInternal)
                    {
                        return ByteToString(data, m_cfg.ReKeyLength, UseEncryption);
                    }
                }
            }

            set
            {
                unsafe
                {
                    fixed (byte* data = m_cfg.ReKeyInternal)
                    {
                        StringToByte(value, data, HAL_WirelessConfiguration.c_ReKeyInternalLength, UseEncryption);
                    }
                }
            }
        }

        private unsafe string ByteCharsToString(byte* chars, int length)
        {
            string retVal = "";
            bool fEmpty = true;

            for (int i = 0; i < length; i++)
            {
                if (chars[i] == 0)
                {
                    length = i;
                }
                if (chars[i] != 0xFF)
                {
                    fEmpty = false;
                }
            }

            if (!fEmpty)
            {
                retVal = new string((sbyte*)chars);
            }

            return retVal;
        }

        private unsafe void StringToByteChars(byte* chars, int charsLen, string data)
        {
            byte[] dataBytes = System.Text.UTF8Encoding.UTF8.GetBytes(data);

            int min = Math.Min(charsLen, dataBytes.Length);
            int i;

            for (i = 0; i < min; i++)
            {
                chars[i] = dataBytes[i];
            }

            for (; i < charsLen; i++)
            {
                chars[i] = 0;
            }
        }

        public string SSID
        {
            get
            {
                unsafe
                {
                    fixed (byte* data = m_cfg.SSID)
                    {
                        return ByteCharsToString(data, HAL_WirelessConfiguration.c_SSIDLength);
                    }
                }
            }

            set
            {
                unsafe
                {
                    fixed (byte* data = m_cfg.SSID)
                    {
                        StringToByteChars(data, HAL_WirelessConfiguration.c_SSIDLength, value);
                    }
                }
            }
        }

        public void Load()
        {
            byte[] data = m_cfgHelper.FindConfig(c_CfgName);

            if (data != null)
            {
                m_cfg = (HAL_WirelessConfiguration)MFConfigHelper.UnmarshalData(data, typeof(HAL_WirelessConfiguration));
            }
        }

        public void Save()
        {
            m_cfg.WirelessNetworkCount = 1;
            m_cfg.Enabled = 1;
            m_cfgHelper.WriteConfig(c_CfgName, m_cfg, false);
        }

        unsafe private string ByteToString(byte* data, int length, bool decrypt)
        {
            string stringForm = "";
            int i = 0;
            bool fEmpty = true;

            if (decrypt)
            {
                byte[] interimData = new byte[length];
                for (i = 0; i < length; i++)
                {
                    interimData[i] = data[i];

                    if (interimData[i] != 0xFF)
                    {
                        fEmpty = false;
                    }
                }

                interimData = Decrypt(interimData);

                for (i = 0; i < length; i++)
                {
                    stringForm += string.Format("{0:x02}", interimData[i]);
                }
            }
            else
            {
                for (i = 0; i < length; i++)
                {
                    if (data[i] != 0xFF)
                    {
                        fEmpty = false;
                    }
                    stringForm += string.Format("{0:x02}", data[i]);
                }
            }

            if (fEmpty)
            {
                return "";
            }

            return stringForm;
        }

        private byte[] Encrypt(byte[] data)
        {
            byte[] deploymentKey = m_cfgHelper.DeploymentPublicKey;
            byte[] iv = new byte[16] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
            byte[] cypher = new byte[data.Length];
            dotNetMFCrypto.CryptoWrapper.Crypto_Encrypt(deploymentKey, iv, iv.Length, data, data.Length, cypher, cypher.Length);

            return cypher;
        }

        private byte[] Decrypt(byte[] cypher)
        {
            byte[] deploymentKey = m_cfgHelper.DeploymentPublicKey;
            byte[] iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
            byte[] data = new byte[cypher.Length];
            dotNetMFCrypto.CryptoWrapper.Crypto_Decrypt(deploymentKey, iv, iv.Length, cypher, cypher.Length, data, data.Length);

            return data;
        }

        unsafe private void StringToByte(string stringForm, byte* data, int length, bool encrypt)
        {
            stringForm = stringForm.Replace(" ", "");
            int i = 0;

            // make sure we have a full byte at the end, otherwise append 0
            if (1 == (stringForm.Length & 1))
            {
                stringForm += "0";
            }

            int count = Math.Min(stringForm.Length/2, length);
            byte[] interimData = new byte[length];

            for (i = 0; i < count; i++)
            {
                interimData[i] = byte.Parse(stringForm.Substring(2*i, 2), NumberStyles.HexNumber);
            }

            for (; i < length; i++)
            {
                interimData[i] = 0;
            }

            if (encrypt)
            {
                interimData = Encrypt(interimData);
            }

            for (i = 0; i < length; i++)
            {
                data[i] = interimData[i];
            }
        }
    }
}
