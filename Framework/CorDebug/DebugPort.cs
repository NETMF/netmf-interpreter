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
using System.Net;
using System.Net.Sockets;

namespace Microsoft.SPOT.Debugger
{
    public class DebugPort : IDebugPort2, IDebugPortEx2, IDebugPortNotify2, IConnectionPointContainer
    {                        
        private Guid m_guid;
        private DebugPortSupplier m_portSupplier;
        private ArrayList m_alProcesses;
        private string m_name;
        private PortFilter m_portFilter;
        private ConnectionPoint m_cpDebugPortEvents2;
        //This can't be shared with other debugPorts, for remote attaching to multiple processes....???
        protected uint m_pidNext;

        public DebugPort(PortFilter portFilter, DebugPortSupplier portSupplier)
        {
            m_name = NameFromPortFilter(portFilter);
            m_portSupplier = portSupplier;
            m_portFilter = portFilter;
            m_cpDebugPortEvents2 = new ConnectionPoint(this, typeof(IDebugPortEvents2).GUID);
            m_alProcesses = ArrayList.Synchronized(new ArrayList(1));
            m_pidNext = 1;
            m_guid = Guid.NewGuid();
        }

        internal CorDebugProcess TryAddProcess(string name)
        {            
            CorDebugProcess process = null;
            PortDefinition portDefinition = null;

            //this is kind of bogus.  How should the attach dialog be organized????

            switch (m_portFilter)
            {
                case PortFilter.TcpIp:
                    for (int retry = 0; retry < 5; retry++)
                    {
                        try
                        {
                            portDefinition = PortDefinition.CreateInstanceForTcp(name);
                            break;
                        }
                        catch (System.Net.Sockets.SocketException)
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                    break;
            }

            if (portDefinition != null)
            {
                process = this.EnsureProcess(portDefinition);
            }

            return process;
        }

        public bool ContainsProcess(CorDebugProcess process)
        {
            return m_alProcesses.Contains(process);
        }

        public void RefreshProcesses()
        {
            PortDefinition[] ports = null;

            switch (m_portFilter)
            {
                case PortFilter.Emulator:
#if USE_CONNECTION_MANAGER
                    ports = this.DebugPortSupplier.Manager.EnumeratePorts(m_portFilter);
#else
                    ports = Emulator.EnumeratePipes();
#endif
                    break;
                case PortFilter.Serial:
                case PortFilter.Usb:
                case PortFilter.TcpIp:
                    ports = GetPersistablePortDefinitions();
                    break;
                default:
                    Debug.Assert(false);
                    throw new ApplicationException();
            }

            ArrayList processes = new ArrayList( m_alProcesses.Count + ports.Length );

            for (int i = ports.Length - 1; i >= 0; i--)
            {
                PortDefinition portDefinition = (PortDefinition)ports[i];
                CorDebugProcess process = EnsureProcess(portDefinition);

                processes.Add(process);
            }

            for(int i = m_alProcesses.Count - 1; i >= 0; i--)
            {
                CorDebugProcess process = (CorDebugProcess)m_alProcesses[i];
                
                if (!processes.Contains(process))
                {
                    RemoveProcess(process);
                }
            }
        }

        public DebugPortSupplier DebugPortSupplier
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_portSupplier; }
        }

        public void AddProcess(CorDebugProcess process)
        {
            if(!m_alProcesses.Contains( process ))
            {
                m_alProcesses.Add( process );
            }
        }

        public CorDebugProcess EnsureProcess(PortDefinition portDefinition)
        {
            CorDebugProcess process = ProcessFromPortDefinition(portDefinition);            

            if (process == null)
            {
                process = new CorDebugProcess(this, portDefinition);

                uint pid;
                if (portDefinition is PortDefinition_Emulator)
                {
                    Debug.Assert(this.IsLocalPort);
                    pid = (uint)((PortDefinition_Emulator)portDefinition).Pid;
                }
                else
                {
                    pid = m_pidNext++;
                }

                process.SetPid(pid);

                AddProcess(process);
            }

            return process;
        }

        private void RemoveProcess(CorDebugProcess process)
        {
            ((ICorDebugProcess)process).Terminate(0);
            m_alProcesses.Remove(process);
        }

        public void RemoveProcess(PortDefinition portDefinition)
        {
            CorDebugProcess process = ProcessFromPortDefinition(portDefinition);

            if (process != null)
            {
                RemoveProcess(process);
            }
        }

        public PortDefinition[] GetPersistablePortDefinitions()
        {
            PortDefinition[] ports = null;

            switch (m_portFilter)
            {
                case PortFilter.Emulator:
                    Debug.Assert(false);
                    break;
                case PortFilter.Serial:
                    ports = AsyncSerialStream.EnumeratePorts();
                    break;
                case PortFilter.Usb:
                    {
                        PortDefinition []portUSB;
                        PortDefinition []portWinUSB;
                        
                        portUSB    = AsyncUsbStream.EnumeratePorts();
                        portWinUSB = WinUsb_AsyncUsbStream.EnumeratePorts(EnablePermiscuousWinUSB);

                        int lenUSB    = portUSB    != null ? portUSB.Length    : 0;
                        int lenWinUSB = portWinUSB != null ? portWinUSB.Length : 0;

                        ports = new PortDefinition[lenUSB + lenWinUSB];

                        if(lenUSB > 0)
                        {
                            Array.Copy(portUSB, ports, lenUSB);
                        }
                        if(lenWinUSB > 0)
                        {
                            Array.Copy(portWinUSB, 0, ports, lenUSB, lenWinUSB);
                        }
                    }
                    break;
                case PortFilter.TcpIp:
                    ports = PortDefinition_Tcp.EnumeratePorts(false);
                    break;         
                default:
                    Debug.Assert(false);
                    throw new ApplicationException();
            }

            return ports;
        }

        public bool AreProcessIdEqual(AD_PROCESS_ID pid1, AD_PROCESS_ID pid2)
        {
            if (pid1.ProcessIdType != pid2.ProcessIdType)
                return false;

            switch ((AD_PROCESS_ID_TYPE) pid1.ProcessIdType)
            {
                case AD_PROCESS_ID_TYPE.AD_PROCESS_ID_SYSTEM:
                    return pid1.dwProcessId == pid2.dwProcessId;
                case AD_PROCESS_ID_TYPE.AD_PROCESS_ID_GUID:
                    return Guid.Equals(pid1.guidProcessId, pid2.guidProcessId);
                default:
                    return false;
            }
        }

        public Guid PortId
        {
            get { return m_guid; }
        }

        public PortFilter PortFilter
        {
            get { return m_portFilter; }
        }

        public bool IsLocalPort
        {
            get { return m_portFilter == PortFilter.Emulator; }
        }

        public string Name
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_name; }
        }
        
        public CorDebugProcess GetDeviceProcess(string deviceName, int eachSecondRetryMaxCount)
        {
            if (string.IsNullOrEmpty(deviceName))
                throw new Exception("DebugPort.GetDeviceProcess() called with no argument");

            VsPackage.MessageCentre.StartProgressMsg(String.Format(DiagnosticStrings.StartDeviceSearch, deviceName, eachSecondRetryMaxCount));

            CorDebugProcess process = this.InternalGetDeviceProcess(deviceName);
            if (process != null)
                return process;

            if (eachSecondRetryMaxCount < 0) eachSecondRetryMaxCount = 0;

            for (int i = 0; i < eachSecondRetryMaxCount && process == null; i++)
            {
                VsPackage.MessageCentre.DeployDot();
                System.Threading.Thread.Sleep(1000);
                process = this.InternalGetDeviceProcess(deviceName);
            }                
            
            VsPackage.MessageCentre.StopProgressMsg(String.Format((process == null) 
                                                                    ? DiagnosticStrings.DeviceFound
                                                                    : DiagnosticStrings.DeviceNotFound,
                                                                  deviceName));
            return process;            
        }
        
        public CorDebugProcess GetDeviceProcess(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                throw new Exception("DebugPort.GetDeviceProcess() called with no argument");
                            
            return this.InternalGetDeviceProcess(deviceName);
        }

        private CorDebugProcess InternalGetDeviceProcess(string deviceName)
        {
            CorDebugProcess process = null;

            RefreshProcesses();
            
            for(int i = 0; i < m_alProcesses.Count; i++)
            {
                CorDebugProcess processT = (CorDebugProcess)m_alProcesses[i];
                PortDefinition pd = processT.PortDefinition;
                if (String.Compare(GetDeviceName(pd), deviceName, true) == 0)
                {
                    process = processT;
                    break;
                }
            }

            if(m_portFilter == PortFilter.TcpIp && process == null)
            {
                process = EnsureProcess(PortDefinition.CreateInstanceForTcp(deviceName));                
            }
                            
            return process;
        }

        public static string NameFromPortFilter(PortFilter portFilter)
        {
            switch (portFilter)
            {
                case PortFilter.Serial:
                    return "Serial";
                case PortFilter.Emulator:
                    return "Emulator";
                case PortFilter.Usb:
                    return "USB";
                case PortFilter.TcpIp:
                    return "TCP/IP";
                default:
                    Debug.Assert(false);
                    return "";
            }
        }

        public string GetDeviceName(PortDefinition pd)
        {
            return pd.PersistName;
        }

        private bool ArePortEntriesEqual(PortDefinition pd1, PortDefinition pd2)
        {
            if (pd1.Port != pd2.Port) 
                return false;
            
            if (pd1.GetType() != pd2.GetType()) 
                return false;
            
            if (pd1 is PortDefinition_Emulator && pd2 is PortDefinition_Emulator)
            {
                if (((PortDefinition_Emulator) pd1).Pid != ((PortDefinition_Emulator) pd2).Pid) 
                    return false;
            }
            
            return true;
        }

        private CorDebugProcess ProcessFromPortDefinition(PortDefinition portDefinition)
        {
            foreach (CorDebugProcess process in m_alProcesses)
            {
                if (ArePortEntriesEqual(portDefinition, process.PortDefinition))
                    return process;
            }

            return null;
        }
        
        private CorDebugProcess GetProcess( AD_PROCESS_ID ProcessId )
        {                        
            AD_PROCESS_ID pid;                   

            foreach (CorDebugProcess process in m_alProcesses)
            {
                pid = process.PhysicalProcessId;

                if (AreProcessIdEqual(ProcessId, pid))
                {
                    return process;
                }
            }

            return null;
        }
                
        private CorDebugProcess GetProcess( IDebugProgramNode2 programNode )
        {
            AD_PROCESS_ID[] pids = new AD_PROCESS_ID[1];                        

            programNode.GetHostPid( pids );
            return GetProcess( pids[0] );    
        }

        private CorDebugAppDomain GetAppDomain( IDebugProgramNode2 programNode )
        {
            uint appDomainId;

            CorDebugProcess process = GetProcess( programNode );

            IDebugCOMPlusProgramNode2 node = (IDebugCOMPlusProgramNode2)programNode;
            node.GetAppDomainId( out appDomainId );

            CorDebugAppDomain appDomain = process.GetAppDomainFromId( appDomainId );

            return appDomain;
        }

        private void SendProgramEvent(IDebugProgramNode2 programNode, enum_EVENTATTRIBUTES attributes, Guid iidEvent)
        {
            CorDebugProcess process = GetProcess( programNode );
            CorDebugAppDomain appDomain = GetAppDomain( programNode );

            IDebugEvent2 evt = new DebugEvent((uint) attributes);
            foreach (IDebugPortEvents2 dpe in m_cpDebugPortEvents2.Sinks)
            {
                dpe.Event(this.DebugPortSupplier.CoreServer, this, (IDebugProcess2)process, (IDebugProgram2) appDomain, evt, ref iidEvent);
            }
        }

        #region IDebugPort2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPort2.GetPortId(out Guid pguidPort)
        {
            pguidPort = PortId;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPort2.GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess)
        {
            ppProcess = ((DebugPort)this).GetProcess( ProcessId );

            return Utility.COM_HResults.BOOL_TO_HRESULT_FAIL( ppProcess != null );            
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPort2.GetPortSupplier(out IDebugPortSupplier2 ppSupplier)
        {
            ppSupplier = m_portSupplier;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPort2.GetPortRequest(out IDebugPortRequest2 ppRequest)
        {
            ppRequest = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPort2.EnumProcesses(out IEnumDebugProcesses2 ppEnum)
        {
            RefreshProcesses();
            ppEnum = new CorDebugEnum(m_alProcesses, typeof(IDebugProcess2), typeof(IEnumDebugProcesses2));
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPort2.GetPortName(out string pbstrName)
        {
            pbstrName = m_name;
            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IDebugPortEx2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPortEx2.GetProgram(IDebugProgramNode2 pProgramNode, out IDebugProgram2 ppProgram)
        {
            CorDebugAppDomain appDomain = GetAppDomain( pProgramNode );
            ppProgram = appDomain;

            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPortEx2.LaunchSuspended(string pszExe, string pszArgs, string pszDir, string bstrEnv, uint hStdInput, uint hStdOutput, uint hStdError, out IDebugProcess2 ppPortProcess)
        {
            ppPortProcess = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPortEx2.TerminateProcess(IDebugProcess2 pPortProcess)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPortEx2.ResumeProcess(IDebugProcess2 pPortProcess)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPortEx2.CanTerminateProcess(IDebugProcess2 pPortProcess)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPortEx2.GetPortProcessId(out uint pdwProcessId)
        {
            pdwProcessId = 0;
            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IDebugPortNotify2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPortNotify2.AddProgramNode(IDebugProgramNode2 pProgramNode)
        {
            SendProgramEvent(pProgramNode, enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS, typeof(IDebugProgramCreateEvent2).GUID);
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugPortNotify2.RemoveProgramNode(IDebugProgramNode2 pProgramNode)
        {
            SendProgramEvent(pProgramNode, enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS, typeof(IDebugProgramDestroyEvent2).GUID);
            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IConnectionPointContainer Members

        void Microsoft.VisualStudio.OLE.Interop.IConnectionPointContainer.FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
        {
            if (riid.Equals(typeof(IDebugPortEvents2).GUID))
            {
                ppCP = m_cpDebugPortEvents2;
            }
            else
            {
                ppCP = null;
                Marshal.ThrowExceptionForHR(Utility.COM_HResults.CONNECT_E_NOCONNECTION);
            }
        }

        void Microsoft.VisualStudio.OLE.Interop.IConnectionPointContainer.EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
        {
            throw new NotImplementedException();
        }

        #endregion

        public bool EnablePermiscuousWinUSB { get; set; }
    }
}
