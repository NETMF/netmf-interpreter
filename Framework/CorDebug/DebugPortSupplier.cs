using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Microsoft.Win32;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.SPOT.Debugger
{
    // The Guid {A1A9D1C3-1F8E-4c82-BD58-474106DE8A9E} needs to match PortSupplierCLSID defined in WiXRegistryInclude.wxs
    [ComVisible(true), Guid("A1A9D1C3-1F8E-4c82-BD58-474106DE8A9E")]
    public class DebugPortSupplier : IDebugPortSupplier2, IDebugPortSupplierEx2, IDebugPortSupplierDescription2
    {
        // The Guid {499dc3d0-66c1-42a2-a292-6ee253d7f708} needs to match PortSupplierGuid defined in WiXRegistryInclude.wxs
        public static Guid s_guidPortSupplier = new Guid("499dc3d0-66c1-42a2-a292-6ee253d7f708");
        private static DebugPortSupplierPrivate s_portSupplier;        

        private class DebugPortSupplierPrivate : DebugPortSupplier
        {                        
            private DebugPort[] m_ports;
            private IDebugCoreServer2 m_server;

#if USE_CONNECTION_MANAGER
            private ConnectionManager.Manager m_manager;
#endif

            public DebugPortSupplierPrivate() : base(true)
            {
#if USE_CONNECTION_MANAGER
                m_manager = new ConnectionManager.Manager();
#endif
                m_ports = new DebugPort[] {
                                            new DebugPort(PortFilter.Emulator, this),
                                            new DebugPort(PortFilter.Usb, this),
                                            new DebugPort(PortFilter.Serial, this),
                                            new DebugPort(PortFilter.TcpIp, this),
                                            };

            }
            
#if USE_CONNECTION_MANAGER            
            public override ConnectionManager.Manager Manager
            {
                [System.Diagnostics.DebuggerHidden]
                get { return m_manager; }
            }
#endif            
                        
            public override DebugPort FindPort(string name)
            {
                for(int i = 0; i < m_ports.Length; i++)
                {
                    DebugPort port = m_ports[i];

                    if (String.Compare(port.Name, name, true) == 0)
                    {
                        return port;
                    }
                }

                return null;
            }

            public override DebugPort[] Ports
            {
                get {return (DebugPort[])m_ports.Clone();}
            }

            public override IDebugCoreServer2 CoreServer
            {
                [System.Diagnostics.DebuggerHidden]
                get { return m_server; }
            }

            private DebugPort DebugPortFromPortFilter(PortFilter portFilter)
            {
                foreach (DebugPort port in m_ports)
                {
                    if (port.PortFilter == portFilter)
                        return port;
                }

                return null;
            }

            #region IDebugPortSupplier2 Members

            new public int GetPortSupplierId(out Guid pguidPortSupplier)
            {
                pguidPortSupplier = DebugPortSupplier.s_guidPortSupplier;
                return Utility.COM_HResults.S_OK;
            }

            new public int RemovePort(IDebugPort2 pPort)
            {
                return Utility.COM_HResults.E_NOTIMPL;
            }

            new public int CanAddPort()
            {
                return Utility.COM_HResults.S_FALSE;
            }

            new public int GetPortSupplierName(out string pbstrName)
            {
                pbstrName = ".NET Micro Framework Device";
                return Utility.COM_HResults.S_OK;
            }

            new public int EnumPorts(out IEnumDebugPorts2 ppEnum)
            {
                ppEnum = new CorDebugEnum(m_ports, typeof(IDebugPort2), typeof(IEnumDebugPorts2));
                return Utility.COM_HResults.S_OK;
            }

            new public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
            {
                string name;

                pRequest.GetPortName(out name);
                ppPort = FindPort(name);

                if (ppPort == null)
                {
                    DebugPort port = m_ports[(int)PortFilter.TcpIp];
                    //hack, hack.  Abusing the Attach to dialog box to force the NetworkDevice port to 
                    //look for a TinyCLR process
                    if (port.TryAddProcess(name) != null)
                    {
                        ppPort = port;
                    }
                }

                return Utility.COM_HResults.BOOL_TO_HRESULT_FAIL( ppPort != null );                
            }

            new public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
            {
                ppPort = null;
                foreach (DebugPort port in m_ports)
                {
                    if (guidPort.Equals(port.PortId))
                    {
                        ppPort = port;
                        break;
                    }
                }
                return Utility.COM_HResults.S_OK;
            }

            #endregion

            #region IDebugPortSupplierEx2 Members

            new public int SetServer(IDebugCoreServer2 pServer)
            {
                m_server = pServer;
                return Utility.COM_HResults.S_OK;
            }

            #endregion
        }

        private DebugPortSupplier( bool fPrivate )
        {
        }

        public DebugPortSupplier()
        {
            lock (typeof(DebugPortSupplier))
            {
                if(s_portSupplier == null)
                {
                    s_portSupplier = new DebugPortSupplierPrivate();
                }
            }
        }

        public virtual DebugPort[] Ports
        {
            get { return s_portSupplier.Ports; }
        }

        public virtual DebugPort FindPort(string name)
        {
            return s_portSupplier.FindPort(name);
        }

#if USE_CONNECTION_MANAGER
        public virtual ConnectionManager.Manager Manager
        {
            get { return s_portSupplier.Manager; }
        }
#endif

        public virtual IDebugCoreServer2 CoreServer
        {
            get { return s_portSupplier.CoreServer; }
        }

        #region IDebugPortSupplierEx2 Members

        public int SetServer(IDebugCoreServer2 pServer) 
        {
            return s_portSupplier.SetServer(pServer);
        }

        #endregion
        
        #region IDebugPortSupplier2 Members

        public int GetPortSupplierId(out Guid pguidPortSupplier) 
        {
            return s_portSupplier.GetPortSupplierId(out pguidPortSupplier);
        }

        public int RemovePort(IDebugPort2 pPort)
        {
            return s_portSupplier.RemovePort(pPort);
        }

        public int CanAddPort()
        {
            return s_portSupplier.CanAddPort();
        }

        public int GetPortSupplierName(out string pbstrName)
        {
            return s_portSupplier.GetPortSupplierName(out pbstrName);
        }

        public int EnumPorts(out IEnumDebugPorts2 ppEnum)
        {
            return s_portSupplier.EnumPorts(out ppEnum);
        }

        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
        {
            return s_portSupplier.AddPort(pRequest, out ppPort);
        }

        public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
        {
            return s_portSupplier.GetPort(ref guidPort, out ppPort);
        }

        #endregion

        #region IDebugPortSupplierDescription2 Members

        int IDebugPortSupplierDescription2.GetDescription(out uint pdwFlags, out string pbstrText)
        {            
            pdwFlags = (uint)(enum_PORT_SUPPLIER_DESCRIPTION_FLAGS)0;

            pbstrText = "Use this transport to connect to all .NET Micro Framework devices or emulators.";

            return Utility.COM_HResults.S_OK;
        }

        #endregion
    }
}
