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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Net;
using System.Net.Sockets;
using _DBG = Microsoft.SPOT.Debugger;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]
[assembly: InternalsVisibleTo("MFDeploy, PublicKey=0024000004800000940000000602000000240000525341310004000001000100B72BE28059C8C300C866887A820EAB7FD9E8F7D41179917EA660A5AC8FA46AC492358A9E4A64591F7C279D77E56844ADDD3762F2F539D493A01631B82AE1A255110E0143856C079976A3396CC30D1E81EA2748F04D198BB273BD721C7FF461A514182C2775B7D8658B529DB2BD11319AB024FAABD7272B3C2F6196184EB666B3")]

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    public class MFDeploy : IDisposable
    {
        private bool m_tryConnect = true;
        private bool m_disposed = false;
        private _DBG.UsbDeviceDiscovery m_usbDiscovery;
        private List<MFPortDefinition> m_deviceList = new List<MFPortDefinition>();

        /// <summary>
        /// An event that fires when the list of devices changes.
        /// </summary>
        public event EventHandler<EventArgs> OnDeviceListUpdate;

        private IPAddress m_DiscoveryMulticastAddress     = IPAddress.Parse("234.102.98.44");
        private IPAddress m_DiscoveryMulticastAddressRecv = IPAddress.Parse("234.102.98.45");
        private int       m_DiscoveryMulticastPort        = 26001;
        private string    m_DiscoveryMulticastToken       = "DOTNETMF";
        private int       m_DiscoveryMulticastTimeout     = 3000;
        private int       m_DiscoveryTTL                  = 1;

        public IPAddress DiscoveryMulticastAddress     { get { return m_DiscoveryMulticastAddress;     } set { m_DiscoveryMulticastAddress     = value; } }
        public IPAddress DiscoveryMulticastAddressRecv { get { return m_DiscoveryMulticastAddressRecv; } set { m_DiscoveryMulticastAddressRecv = value; } }
        public int       DiscoveryMulticastPort        { get { return m_DiscoveryMulticastPort;        } set { m_DiscoveryMulticastPort        = value; } }
        public string    DiscoveryMulticastToken       { get { return m_DiscoveryMulticastToken;       } set { m_DiscoveryMulticastToken       = value; } }
        public int       DiscoveryMulticastTimeout     { get { return m_DiscoveryMulticastTimeout;     } set { m_DiscoveryMulticastTimeout     = value; } }
        public int       DiscoveryTTL                  { get { return m_DiscoveryTTL;                  } set { m_DiscoveryTTL                  = value; } }

        public bool EnablePermiscuousWinUsb { get; set; }


        /// <summary>
        /// Enumerates the available ports for .Net Micro Framework devices.
        /// Also, USART or COM ports will be listed even if there is no device attached.
        /// </summary>
        /// <returns>List of MFPortDefinition objects which represent transports for possible MF devices</returns>
        public ReadOnlyCollection<MFPortDefinition> EnumPorts(params TransportType[] types)
        {
            if (types == null || types.Length == 0)
            {
                types = new TransportType[] { TransportType.Serial, TransportType.TCPIP, TransportType.USB };
            }

            m_deviceList.Clear();

            foreach (TransportType type in types)
            {
                ArrayList list = new ArrayList();

                if (type == TransportType.TCPIP)
                {
                    list = new ArrayList(_DBG.PortDefinition_Tcp.EnumeratePorts(m_DiscoveryMulticastAddress, m_DiscoveryMulticastAddressRecv, m_DiscoveryMulticastPort, m_DiscoveryMulticastToken, m_DiscoveryMulticastTimeout, m_DiscoveryTTL));
                }
                else
                {
                    switch (type)
                    {
                        case TransportType.USB:
                            list = _DBG.PortDefinition.Enumerate( EnablePermiscuousWinUsb ? _DBG.PortFilter.LegacyPermiscuousWinUsb : _DBG.PortFilter.Usb);
                            break;
                        case TransportType.Serial:
                            list = _DBG.PortDefinition.Enumerate(_DBG.PortFilter.Serial);
                            break;
                        default:
                            throw new ArgumentException();
                    }
                }

                foreach (_DBG.PortDefinition pd in list)
                {
                    switch (type)
                    {
                        case TransportType.Serial:
                            m_deviceList.Add(new MFSerialPort(pd.DisplayName, pd.Port));
                            break;
                        case TransportType.USB:
                            m_deviceList.Add(new MFUsbPort(pd.DisplayName));
                            break;
                        case TransportType.TCPIP:
                            _DBG.PortDefinition_Tcp prt = pd as _DBG.PortDefinition_Tcp;
                            string[] split = prt.Port.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (split.Length > 0)
                            {
                                m_deviceList.Add(new MFTcpIpPort(split[0], prt.MacAddress));
                            }
                            break;
                    }
                }
            }
            return new ReadOnlyCollection<MFPortDefinition>(m_deviceList);
        }

        /// <summary>
        /// This method attempts to connect to a device on the given port.  Connect throws a MFDeviceNoResponseException 
        /// exception in the case that the connection could not be made.
        /// </summary>
        /// <param name="portDefinition">A Port definition for which to attempt the connection to an .Net Micro Framework device.</param>
        /// <param name="tinyBooterPortDef">Devices can use a second port for the bootloader (TinyBooter).  
        /// This parameter identifies the tinyBooter port.
        /// </param>
        /// <returns>A connected .Net Micro Framework device</returns>
        public MFDevice Connect(MFPortDefinition portDefinition, MFPortDefinition tinyBooterPortDef)
        {
            return InitializePorts(portDefinition, tinyBooterPortDef);
        }
        /// <summary>
        /// This method attempts to connect to a device on the given port. The Connect method throws a 
        /// MFDeviceNoResponseException exception in the case that the connection could not be made.
        /// </summary>
        /// <param name="portDefinition">A port definition for which to attempt the connection to an .Net Micro Framework device</param>
        /// <returns>A connected .Net Micro Framework device</returns>
        public MFDevice Connect(MFPortDefinition portDefinition)
        {
            return InitializePorts(portDefinition, null);
        }
        /// <summary>
        /// Gets an list of all the .Net Micro Framework device ports that are available.  This can include 
        /// ports for which no device is connected.
        /// </summary>
        public IList DeviceList
        {
            get
            {
                EnumPorts();
                ArrayList list = new ArrayList();
                list.AddRange(m_deviceList);
                return list;
            }
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public MFDeploy()
        {
            try
            {
                // Default to disabling legacy WinUSB enumeration so that only
                // WinUSB devices that explicitly advertise the NETMF Debug
                // Interface GUID are enumerated. (Eliminates false positives
                // of legacy mode that shows any WinUSB device that responds
                // to a query for a string with the display name Id (5) )
                EnablePermiscuousWinUsb = false;
                m_usbDiscovery = new Microsoft.SPOT.Debugger.UsbDeviceDiscovery();
                m_usbDiscovery.OnDeviceChanged += new Microsoft.SPOT.Debugger.UsbDeviceDiscovery.DeviceChangedEventHandler(OnDeviceListChanged);
            }
            catch //WMI may not be supported on the OS
            { 
            }
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    if (m_usbDiscovery != null)
                    {
                        m_usbDiscovery.OnDeviceChanged -= new Microsoft.SPOT.Debugger.UsbDeviceDiscovery.DeviceChangedEventHandler(OnDeviceListChanged);
                        m_usbDiscovery.Dispose();
                        m_usbDiscovery = null;
                    }
                }
                m_disposed = true;
            }
        }

        public void Dispose()
        {

            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~MFDeploy()
        {
            Dispose(false);
        }

        internal bool OpenWithoutConnect
        {
            set
            {
                m_tryConnect = !value;
            }
        }

        private MFDevice InitializePorts(MFPortDefinition portDefinitionMain, MFPortDefinition portDefinitionTinyBooter)
        {
            MFDevice device = null;
            MFPortDefinition[] portDefs = new MFPortDefinition[2] { portDefinitionMain, portDefinitionTinyBooter };
            _DBG.PortDefinition[] pds = new Microsoft.SPOT.Debugger.PortDefinition[2];

            for (int i = 0;i < portDefs.Length;i++)
            {
                MFPortDefinition portDefinition = portDefs[i];
                if (portDefinition == null) continue;

                if (portDefinition.Transport == TransportType.TCPIP)
                {
                    System.Net.IPAddress[] addr = System.Net.Dns.GetHostAddresses(portDefinition.Port);

                    pds[i] = new Microsoft.SPOT.Debugger.PortDefinition_Tcp(addr[0]);
                }
                else
                {
                    ArrayList list = _DBG.PortDefinition.Enumerate(_DBG.PortFilter.Usb, _DBG.PortFilter.Serial);
                    foreach (_DBG.PortDefinition pd in list)
                    {
                        if (portDefinition.Port.Length > 0)
                        {
                            if (string.Equals(portDefinition.Port, pd.UniqueId.ToString()))
                            {
                                pds[i] = pd;
                                break;
                            }
                        }
                        if (string.Equals(portDefinition.Name, pd.DisplayName))
                        {
                            pds[i] = pd;
                            break;
                        }
                    }
                }
            }

            if (pds[0] == null && pds[1] != null)
            {
                pds[0] = pds[1];
                pds[1] = null;
            }

            if (pds[0] != null || pds[1] != null)
            {
                device = new MFDevice(pds[0], pds[1]);

                if (!device.Connect(2000, m_tryConnect))
                {
                    throw new MFDeviceNoResponseException();
                }
            }
            else
            {
                throw new MFDeviceUnknownDeviceException();
            }

            return device;
        }

        private void OnDeviceListChanged(_DBG.UsbDeviceDiscovery.DeviceChanged devChange)
        {
            if (OnDeviceListUpdate != null)
            {
                OnDeviceListUpdate(null, null);
            }
        }
    }
}
