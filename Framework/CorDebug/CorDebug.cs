using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Microsoft.Win32;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.SPOT.Debugger
{
    // The Guid {D9BC7C21-9AD2-49a2-B33A-5AB2145E6C56} needs to match CorDebugCLSID defined in WiXRegistryInclude.wxs
    [ComVisible(true), Guid("D9BC7C21-9AD2-49a2-B33A-5AB2145E6C56")]
    public class CorDebug : ICorDebug, IDebugRemoteCorDebug
    {
        // The Guid {009e15ee-439a-4ce2-b078-08136a4712b1} needs to match EngineGuid defined in WiXRegistryInclude.wxs
        public static Guid s_guidDebugEngine = new Guid("009e15ee-439a-4ce2-b078-08136a4712b1");

        private ArrayList m_processes;
        private ICorDebugManagedCallback m_callback;

        public CorDebug()
        {
            m_processes = new ArrayList(1);
        }

        public ICorDebugManagedCallback ManagedCallback
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_callback; }
        }

        public void RegisterProcess(CorDebugProcess process)
        {
            m_processes.Add(process);
        }

        public void UnregisterProcess(CorDebugProcess process)
        {
            m_processes.Remove(process);
        }

        #region ICorDebug Members

        int ICorDebug.Terminate()
        {
            // CorDebug.Terminate is not implemented
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebug.GetProcess(uint dwProcessId, out ICorDebugProcess ppProcess)
        {
            ppProcess = null;

            foreach (CorDebugProcess process in m_processes)
            {
                uint id = process.PhysicalProcessId.dwProcessId;

                if (dwProcessId == id)
                {
                    ppProcess = process;
                    break;
                }
            }

            return Utility.COM_HResults.BOOL_TO_HRESULT_FAIL( ppProcess != null ); /*better failure?*/
        }

        int ICorDebug.SetManagedHandler( ICorDebugManagedCallback pCallback )
        {
            m_callback = pCallback;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebug.EnumerateProcesses( out ICorDebugProcessEnum ppProcess )
        {
            ppProcess = new CorDebugEnum(m_processes, typeof(ICorDebugProcess), typeof(ICorDebugProcessEnum));
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebug.SetUnmanagedHandler( ICorDebugUnmanagedCallback pCallback )
        {
            // CorDebug.SetUnmanagedHandler is not implemented

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebug.DebugActiveProcess( uint id, int win32Attach, out ICorDebugProcess ppProcess )
        {
            ppProcess = null;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebug.CreateProcess( string lpApplicationName, string lpCommandLine, _SECURITY_ATTRIBUTES lpProcessAttributes, _SECURITY_ATTRIBUTES lpThreadAttributes, int bInheritHandles, uint dwCreationFlags, System.IntPtr lpEnvironment, string lpCurrentDirectory, _STARTUPINFO lpStartupInfo, _PROCESS_INFORMATION lpProcessInformation, CorDebugCreateProcessFlags debuggingFlags, out ICorDebugProcess ppProcess )
        {
            ppProcess = null;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebug.CanLaunchOrAttach( uint dwProcessId, int win32DebuggingEnabled )
        {
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebug.Initialize()
        {
            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IDebugRemoteCorDebug Members

        int IDebugRemoteCorDebug.CreateProcessEx( Microsoft.VisualStudio.Debugger.Interop.IDebugPort2 pPort, string lpApplicationName, string lpCommandLine, System.IntPtr lpProcessAttributes, System.IntPtr lpThreadAttributes, int bInheritHandles, uint dwCreationFlags, System.IntPtr lpEnvironment, string lpCurrentDirectory, ref CorDebugInterop._STARTUPINFO lpStartupInfo, ref CorDebugInterop._PROCESS_INFORMATION lpProcessInformation, uint debuggingFlags, out object ppProcess )
        {
            ppProcess = null;

            try
            {
                // CreateProcessEx() is guaranteed to return a valid process object, or throw an exception
                CorDebugProcess process = CorDebugProcess.CreateProcessEx(pPort, lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, ref lpStartupInfo, ref lpProcessInformation, debuggingFlags);

                // StartDebugging() will either get a connected device into a debuggable state and start the dispatch thread, or throw.
                process.StartDebugging(this, true);
                ppProcess = process;

                return Utility.COM_HResults.S_OK;
            }
            catch (ProcessExitException)
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.InitializeProcessFailedProcessDied);
                return Utility.COM_HResults.S_FALSE;
            }
            catch (Exception ex)
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.InitializeProcessFailed);
                VsPackage.MessageCentre.InternalErrorMsg(false, ex.Message);
                return Utility.COM_HResults.S_FALSE;
            }
        }

        int IDebugRemoteCorDebug.DebugActiveProcessEx( IDebugPort2 pPort, uint id, int win32Attach, out object ppProcess )
        {
            ppProcess = null;
            try
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.Attach);
                AD_PROCESS_ID pid = new AD_PROCESS_ID();

                pid.ProcessIdType = (uint) AD_PROCESS_ID_TYPE.AD_PROCESS_ID_SYSTEM;

                pid.dwProcessId = id;

                IDebugProcess2 iDebugProcess;
                pPort.GetProcess(pid, out iDebugProcess);

                CorDebugProcess process = (CorDebugProcess) iDebugProcess;

                // StartDebugging() will either get a connected device into a debuggable state and start the dispatch thread, or throw.
                process.StartDebugging(this, false);
                ppProcess = process;

                return Utility.COM_HResults.S_OK;
            }
            catch (ProcessExitException)
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.AttachFailedProcessDied);
                return Utility.COM_HResults.S_FALSE;
            }
            catch (Exception ex)
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.AttachFailed);
                VsPackage.MessageCentre.InternalErrorMsg(false, ex.Message);
                return Utility.COM_HResults.S_FALSE;
            }
       }

       #endregion
    }
}
