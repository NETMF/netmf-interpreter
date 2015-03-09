using System;
using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Microsoft.SPOT.Debugger;

namespace Microsoft.NetMicroFramework.Tools.MFProfilerTool
{
    [System.ComponentModel.DefaultEvent("SelectedValueChanged"),
     System.ComponentModel.Designer(typeof(MFDeviceSelectorComboBoxDesigner)),
     System.ComponentModel.Description( ".NET MF Transport Type Selector Control" )]
    public partial class MFDeviceSelectorComboBox : System.Windows.Forms.ComboBox //, ISupportInitialize
    {
        private const string c_DesignText = ".NET Micro Device List";
        private const string c_None = "<None>";
        private IPAddress DiscoveryMulticastAddress = IPAddress.Parse("234.102.98.44");
        private IPAddress DiscoveryMulticastAddressRecv = IPAddress.Parse("234.102.98.45");
        private const int DiscoveryMulticastPort = 26001;
        private const string DiscoveryMulticastToken = "DOTNETMF";

        private PortFilter[] m_pf;

        public MFDeviceSelectorComboBox() : base()
        {
            base.DisplayMember = "DisplayName";
            base.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            base.FormattingEnabled = true;

            m_pf = new PortFilter[] { PortFilter.Serial };

            this.DropDown += new EventHandler(MFDeviceSelectorComboBox_DropDown);            
        }

        void MFDeviceSelectorComboBox_DropDown(object sender, EventArgs e)
        {
            OnDeviceListUpdate();
        }

        protected override void OnCreateControl()
        {
            //this.Site is null in the constructor; wait for the OnCreateControl method to determine Design/Run-time mode.
            if (this.DesignMode)
            {
                Text = c_DesignText;
            }
        }

        [Browsable(false)]
        public PortDefinition PortDefinition
        {
            get
            {
                if (base.DesignMode) return null;

                PortDefinition port = base.SelectedItem as PortDefinition;
               
                return port;
            }
        }

        [Browsable(false)]
        public PortFilter[] Filter
        {
            set {
                m_pf = value;
                if (!this.DesignMode)
                {
                    UpdateList();
                }
            }
            get { return m_pf; }
        }

        public void UpdateList()
        {
            ArrayList list = PortDefinition.Enumerate(m_pf);

            #region Code that should probably be in PortDefinition.Enumerate
            {
                bool enumTcpIpPorts = false;
                foreach (PortFilter pf in m_pf)
                {
                    if (pf == PortFilter.TcpIp)
                    {
                        enumTcpIpPorts = true;
                        break;
                    }
                }
                if (enumTcpIpPorts)
                {
                    list.AddRange(EnumerateTcpIpPorts());
                }
            }
            #endregion

            #region Code that could go in PortDefinition.Enumerate if we add a LauncableEmulator port definition. (Or move PersistableEmulator into a non-VS class?
            {
                bool listLaunchableEmulators = false;
                foreach (PortFilter pf in m_pf)
                {
                    if (pf == PortFilter.Emulator)
                    {
                        listLaunchableEmulators = true;
                        break;
                    }
                }

                if (listLaunchableEmulators == true)
                {
                    PlatformInfo pi = new PlatformInfo(null);
                    PlatformInfo.Emulator[] emus = pi.Emulators;

                    foreach (PlatformInfo.Emulator emu in emus)
                    {
                        list.Add(new PortDefinition_Emulator("Launch '" + emu.name + "'", emu.persistableName, 0));
                    }
                }
            }
            #endregion

            object si = SelectedItem;
            base.DataSource = null;
            if (list.Count > 0)
            {
                base.DataSource = list;
                if (list[0] is PortDefinition)
                {
                    base.DisplayMember = "DisplayName";
                }
                if (si != null)
                {
                    SelectedItem = si;
                }
                else
                {
                    base.SelectedIndex = 0;
                }

            }
            else
            {
                base.Text = "<None>";
            }
            base.Update();
        }

        public void SelectLaunchedEmulator(PortDefinition_Emulator port)
        {
            ArrayList al = (ArrayList)DataSource as ArrayList;
            if (al == null)
            {
                al = new ArrayList();
            }

            al.Add(port);
            DataSource = null;  //Set to null to force list refresh
            DataSource = al;
            DisplayMember = "DisplayName";
            SelectedItem = port; //Select newly-launched emulator.
        }

        #region Code that should probably be in Microsoft.SPOT.Debugger

        /* Why isn't this in Microsoft.SPOT.Debugger like all the other port enumeration methods? */
        internal ArrayList EnumerateTcpIpPorts()
        {
            ArrayList list = new ArrayList();

            int cnt = 0;
            int total = 0;
            byte[] data = new byte[1024];
            Socket sock = null;
            Socket recv = null;

            try
            {
                System.Collections.Generic.List<IPAddress> addresses = new System.Collections.Generic.List<IPAddress>();
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPEndPoint epMulticast = new IPEndPoint(DiscoveryMulticastAddress, DiscoveryMulticastPort);
                IPAddress addr = null;

                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        addr = ip;
                        break;
                    }
                }

                IPEndPoint endPoint = new IPEndPoint(addr, 0);
                System.Net.EndPoint epRemote = new IPEndPoint(IPAddress.Any, DiscoveryMulticastPort);

                // send ping
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sock.Bind(endPoint);
                sock.MulticastLoopback = false;
                sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 64);
                sock.SendTo(System.Text.Encoding.ASCII.GetBytes(DiscoveryMulticastToken), SocketFlags.None, epMulticast);

                try
                {
                    recv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    recv.Bind(epRemote);
                    recv.ReceiveTimeout = 1000;
                    System.Net.Sockets.MulticastOption mco = new MulticastOption(DiscoveryMulticastAddressRecv);

                    recv.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mco);

                    while (0 < (cnt = recv.ReceiveFrom(data, total, data.Length - total, SocketFlags.None, ref epRemote)))
                    {
                        addresses.Add(((IPEndPoint)epRemote).Address);
                        total += cnt;
                        recv.ReceiveTimeout = 200;
                    }
                }
                catch
                {
                }

                foreach (IPAddress ip1 in addresses)
                {
                    list.Add(new PortDefinition_Tcp(ip1));
                }
            }
            catch (Exception e2)
            {
                System.Diagnostics.Debug.Print(e2.ToString());
            }
            finally
            {
                if (recv != null)
                {
                    recv.Shutdown(SocketShutdown.Both);
                    recv.Close();
                }
                if (sock != null)
                {
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }

            return list;
        }

        #endregion

        private void OnDeviceListUpdate()
        {
            base.Invoke((MethodInvoker)delegate
            {
                UpdateList();
            });
        }
    }

    public class MFDeviceSelectorComboBoxDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            base.AutoResizeHandles = true;
        }

        protected override void PostFilterProperties(IDictionary properties)
        {
            properties.Remove("AutoCustomSource");
            properties.Remove("AutoCompleteMode");
            properties.Remove("AutoCompleteSource");
            properties.Remove("BindingContext");
            properties.Remove("DataBindings");
            properties.Remove("DataSource");
            properties.Remove("DisplayMember");
            properties.Remove("DropDownStyle");
            properties.Remove("Items");
            properties.Remove("Filter");
            properties.Remove("Text");
            properties.Remove("ValueMember");
        }

        public override SelectionRules SelectionRules
        {
            get
            {
                SelectionRules selectionRules = base.SelectionRules;
                return (selectionRules & ~(SelectionRules.BottomSizeable | SelectionRules.TopSizeable));
            }
        }
    }
}

