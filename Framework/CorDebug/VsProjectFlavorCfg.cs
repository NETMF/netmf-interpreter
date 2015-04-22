using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading;
using System.ComponentModel;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Debugger.Interop;
using Debugging_Resolve_Assembly = Microsoft.SPOT.Debugger.WireProtocol.Commands.Debugging_Resolve_Assembly;
using MessageBox = System.Windows.Forms.MessageBox;
using Microsoft.Win32;
using CorDebugInterop;

namespace Microsoft.SPOT.Debugger
{
    /// <summary>
    /// Summary description for VsProjectFlavorCfg.
    /// </summary>
    ///  Break this into separate classes?  Debug/Deploy
    public class VsProjectFlavorCfg 
        : IVsProjectFlavorCfg
        , IVsDebuggableProjectCfg
        , IVsDeployableProjectCfg
    {

        #region CallbackControl

        private class DeployState
        {
            public delegate void EndDeployDelegate();

            public Thread m_threadDeploy;
            public System.Windows.Forms.Control m_deployCallbackControl;
            public IVsOutputWindowPane m_outputWindowPane;
            public bool m_deploySuccess = true;
            public string m_deployOuputMessage;
            public string m_deployTaskItem;
        }

        #endregion

        IVsCfg m_vsCfg;
        VsProject m_project;
        DeployState m_deployState;

        private ConnectionPoint.Connections m_connectionsDeployStatusCallback;

        public readonly ProjectBuildProperty DeployPropertyPort;
        public readonly ProjectBuildProperty DeployPropertyDevice;

        public readonly ProjectBuildPropertyBool GenerateStubsFlag;
        public readonly ProjectBuildProperty GenerateStubsRootName;
        public readonly ProjectBuildProperty GenerateStubsDirectory;

        private IVsBuildPropertyStorage m_IVsBuildPropertyStorage;
        private IVsDebuggableProjectCfg m_innerIVsDebuggableProjectCfg;

        public VsProjectFlavorCfg(VsProject project, IVsCfg baseCfg)
        {
            m_vsCfg = baseCfg;
            m_project = project;

            m_innerIVsDebuggableProjectCfg = m_vsCfg as IVsDebuggableProjectCfg;
            m_IVsBuildPropertyStorage = m_project as IVsBuildPropertyStorage;

            Debug.Assert(m_innerIVsDebuggableProjectCfg != null);

            m_connectionsDeployStatusCallback = new ConnectionPoint.Connections();

            DeployPropertyPort = new ProjectBuildProperty("DeployTransport", m_IVsBuildPropertyStorage, this, _PersistStorageType.PST_USER_FILE, _PersistStorageType.PST_PROJECT_FILE, DebugPort.NameFromPortFilter(PortFilter.Emulator));
            DeployPropertyDevice = new ProjectBuildProperty("DeployDevice", m_IVsBuildPropertyStorage, this, _PersistStorageType.PST_USER_FILE, _PersistStorageType.PST_PROJECT_FILE);

            GenerateStubsFlag = new ProjectBuildPropertyBool("MF_GenerateStubs", m_IVsBuildPropertyStorage, this, _PersistStorageType.PST_USER_FILE);
            GenerateStubsRootName = new ProjectBuildProperty("MF_GenerateStubsRootName", m_IVsBuildPropertyStorage, this, _PersistStorageType.PST_USER_FILE, _PersistStorageType.PST_PROJECT_FILE, "TARGET");
            GenerateStubsDirectory = new ProjectBuildProperty("MF_GenerateStubsDirectory", m_IVsBuildPropertyStorage, this, _PersistStorageType.PST_USER_FILE, _PersistStorageType.PST_PROJECT_FILE, "DIRECTORY");

            try
            {
                ActivateDebugEngine();
            }
            catch (Exception e)
            {
                VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("Unable to register debug engine: {0}", e.Message));
            }
        }

        static uint debugEngineCmdUICookie = 0;
        private void ActivateDebugEngine()
        {
            // The debug engine will not work unless we enable a CmdUIContext using the engine's GUID.
            if (debugEngineCmdUICookie == 0)
            {
                IVsMonitorSelection monitorSelection = ServiceProvider.GlobalProvider.GetService( typeof( SVsShellMonitorSelection ) ) as IVsMonitorSelection;
                if (monitorSelection == null)
                {
                    throw new InvalidOperationException(String.Format("Missing service {0}!", typeof(IVsMonitorSelection).FullName));
                }
                Guid guidDebugEngine = CorDebug.s_guidDebugEngine;

                int hr = monitorSelection.GetCmdUIContextCookie(ref guidDebugEngine, out debugEngineCmdUICookie);
                if (ErrorHandler.Succeeded(hr))
                {
                    ErrorHandler.ThrowOnFailure(monitorSelection.SetCmdUIContext(debugEngineCmdUICookie, 1));
                }
                else
                {
                    // GetCmdUIContextCookie is expected to fail if the IDE has been launched
                    // in command line mode. Verify that it's unexpected before throwing.
                    IVsShell vsShell = ServiceProvider.GlobalProvider.GetService( typeof( SVsShell ) ) as IVsShell;
                    if (vsShell != null)
                    {
                        object inCmdLineMode;
                        ErrorHandler.ThrowOnFailure(vsShell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out inCmdLineMode));
                        if (inCmdLineMode is bool)
                        {
                            if ((bool)inCmdLineMode)
                            {
                                hr = VSConstants.S_OK;
                            }
                        }
                    }
                    // Reset hr to S_OK to avoid throwing here if the failure was expected.
                    ErrorHandler.ThrowOnFailure(hr);
                }
            }
        }

        private string DeployTransportName
        {
            get
            {
                string portName = this.DeployPropertyPort.Value;

                if (portName == null)
                {
                    portName = DebugPort.NameFromPortFilter(PortFilter.Emulator);
                }

                return portName;
            }
        }

        public Debugger.PlatformInfo PlatformInfo
        {
            get {
                string frameworkVersion;
                m_IVsBuildPropertyStorage.GetPropertyValue(MSBuildProperties.TargetFrameworkVersion, this.GetName(false), (uint)_PersistStorageType.PST_PROJECT_FILE, out frameworkVersion);
                return new PlatformInfo(frameworkVersion);
            }
        }

        private string DeployDeviceName
        {
            get
            {
                return this.DeployPropertyDevice.Value;
            }
        }

        public string GetName(bool fIncludePlatform)
        {
            string name;

            get_CanonicalName(out name);

            if (!fIncludePlatform)
            {
                int index = name.IndexOf('|');
                if (index >= 0)
                {
                    name = name.Substring(0, index);
                }
            }

            return name;
        }

        private VsPackage Package
        {
            get { return this.m_project.Package; }
        }

        private void GetEmulatorConfigHelper(ref string emulatorValue, string propName, _PersistStorageType storage)
        {
            string val;

            this.m_IVsBuildPropertyStorage.GetPropertyValue(propName, this.GetName(false), (uint)storage, out val);

            if (!string.IsNullOrEmpty(val))
            {
                emulatorValue = val;
            }
        }

        private void GetEmulatorConfigHelper(ref bool emulatorValue, string propName, _PersistStorageType storage)
        {
            string val = null;

            GetEmulatorConfigHelper(ref val, propName, storage);

            if (!string.IsNullOrEmpty(val))
            {
                bool.TryParse(val, out emulatorValue);
            }
        }

        private Debugger.PlatformInfo.Emulator GetEmulatorConfig()
        {
            PlatformInfo.Emulator emulator = this.PlatformInfo.FindEmulator(this.DeployDeviceName);

            if (emulator == null)
            {
                PlatformInfo.Emulator[] emulators = this.PlatformInfo.Emulators;

                if (emulators.Length > 0)
                {
                    //Pick a default
                    emulator = emulators[0];
                }
            }

            if (emulator != null)
            {
                emulator = emulator.Clone();
            }
            else
            {
                emulator = new PlatformInfo.Emulator();
            }

            //Allow for overrides from the project file
            GetEmulatorConfigHelper(ref emulator.application, "EmulatorExe", _PersistStorageType.PST_PROJECT_FILE);
            GetEmulatorConfigHelper(ref emulator.additionalOptions, "EmulatorArguments", _PersistStorageType.PST_PROJECT_FILE);
            GetEmulatorConfigHelper(ref emulator.config, "EmulatorConfig", _PersistStorageType.PST_PROJECT_FILE);
            GetEmulatorConfigHelper(ref emulator.legacyCommandLine, "EmulatorArgumentsLegacy", _PersistStorageType.PST_PROJECT_FILE);

            return emulator;
        }

        private void GetCommandLineForLaunch(bool fNoDebug, Debugger.PlatformInfo.Emulator emulatorConfig, Debugger.CommandLineBuilder cb, bool fIsTargetBigEndian)
        {
            if (emulatorConfig != null)
            {
                if (!string.IsNullOrEmpty(emulatorConfig.config))
                {
                    cb.AddArguments("/config:" + emulatorConfig.config);
                }
            }

            if (!fNoDebug)
            {
                cb.AddArguments("/waitfordebugger");
            }

            foreach (string assembly in m_project.GetDependencies(true, true, fIsTargetBigEndian))
            {
                cb.AddArguments( "/load:" + assembly );
            }

            string args = this.m_project.ProjectProperties.ActiveConfigurationSettings.StartArguments;
            args = args.Trim();
            if (args.Length > 0)
            {
                cb.AddArguments( "/commandlinearguments:" + args );
            }
        }

        private void GetCommandLineForLaunchLegacy(bool fNoDebug, Debugger.PlatformInfo.Emulator emulatorConfig, Debugger.CommandLineBuilder cb, bool fIsTargetBigEndian)
        {

#if DEBUG   // TRACE has been removed from release builds
                cb.AddArguments( "-Trace_MemoryStats", Utility.Boolean.BoolToInt( true ) );
#endif

                if (!fNoDebug)
                    cb.AddArguments( "-PipeName", "" );

#if DEBUG
                cb.AddArguments( "-Force_Compaction", Utility.Boolean.BoolToInt( true ) );
#endif

                string deviceCfg = Environment.GetEnvironmentVariable( "CLRROOT" );
                if (deviceCfg != null)
                {
                    deviceCfg = Path.Combine( deviceCfg, "test.devicecfg" );
                    if (File.Exists( deviceCfg ))
                        cb.AddArguments( "-devicecfg", deviceCfg );
                }


                foreach (string assembly in m_project.GetDependencies(true, true, fIsTargetBigEndian))
                {
                    cb.AddArguments( "-load", assembly );
                }

                string args = this.m_project.ProjectProperties.ActiveConfigurationSettings.StartArguments;
                args = args.Trim();
                if (args.Length > 0)
                {
                    cb.AddArguments( "-commandLineArgs", args );
                }

                cb.AddArguments( "-resolve" );
                cb.AddArguments( "-execute" );

        }

        private string GetCommandLineForLaunch(bool fNoDebug, Debugger.PlatformInfo.Emulator emulatorConfig, bool fIsTargetBigEndian)
        {
            CommandLineBuilder cb = new CommandLineBuilder();

            if (emulatorConfig != null && !string.IsNullOrEmpty(emulatorConfig.additionalOptions))
            {
                CommandLineBuilder cbT = new CommandLineBuilder(emulatorConfig.additionalOptions);
                cb.AddArguments(cbT.Arguments);
            }

            if (emulatorConfig != null && emulatorConfig.legacyCommandLine)
            {
                GetCommandLineForLaunchLegacy(fNoDebug, emulatorConfig, cb, fIsTargetBigEndian);
            }
            else
            {
                GetCommandLineForLaunch(fNoDebug, emulatorConfig, cb, fIsTargetBigEndian);
            }

            string commandLine = cb.ToString();
            commandLine = Environment.ExpandEnvironmentVariables(commandLine);

            return commandLine;
        }

        #region IVsProjectFlavorCfg Members

        public int Close()
        {
            return Utility.COM_HResults.S_OK;
        }

        public int get_CfgType(ref Guid iidCfg, out System.IntPtr ppCfg)
        {
            Type[] types = { typeof(IVsDebuggableProjectCfg), typeof(IVsDeployableProjectCfg) /*, typeof(IVsPublishableProjectCfg)*/ };

            foreach (Type type in types)
            {
                if (iidCfg == type.GUID)
                {
                    ppCfg = Marshal.GetComInterfaceForObject(this, type);
                    Debug.Assert(ppCfg != IntPtr.Zero);
                    return Utility.COM_HResults.S_OK;
                }
            }

            ppCfg = IntPtr.Zero;
            return Utility.COM_HResults.E_NOINTERFACE;
        }

        #endregion

        #region IVsDebuggableProjectCfg Members

        public int OpenOutput(string szOutputCanonicalName, out IVsOutput ppIVsOutput)
        {
            return m_innerIVsDebuggableProjectCfg.OpenOutput( szOutputCanonicalName, out ppIVsOutput );
        }

        public int get_DisplayName(out string pbstrDisplayName)
        {
            return m_innerIVsDebuggableProjectCfg.get_DisplayName( out pbstrDisplayName );
        }

        public int get_BuildableProjectCfg(out IVsBuildableProjectCfg ppIVsBuildableProjectCfg)
        {
            return m_innerIVsDebuggableProjectCfg.get_BuildableProjectCfg( out ppIVsBuildableProjectCfg );
        }

        public int get_ProjectCfgProvider(out IVsProjectCfgProvider ppIVsProjectCfgProvider)
        {
            return m_innerIVsDebuggableProjectCfg.get_ProjectCfgProvider( out ppIVsProjectCfgProvider );
        }

        public int get_IsReleaseOnly(out int pfIsReleaseOnly)
        {
            return m_innerIVsDebuggableProjectCfg.get_IsReleaseOnly( out pfIsReleaseOnly );
        }

        public int get_IsPackaged(out int pfIsPackaged)
        {
            return m_innerIVsDebuggableProjectCfg.get_IsPackaged( out pfIsPackaged );
        }

        public int get_RootURL(out string pbstrRootURL)
        {
            return m_innerIVsDebuggableProjectCfg.get_RootURL( out pbstrRootURL );
        }

        public int get_Platform(out Guid pguidPlatform)
        {
            return m_innerIVsDebuggableProjectCfg.get_Platform( out pguidPlatform );
        }

        public int get_UpdateSequenceNumber(Microsoft.VisualStudio.OLE.Interop.ULARGE_INTEGER[] puliUSN)
        {
            return m_innerIVsDebuggableProjectCfg.get_UpdateSequenceNumber( puliUSN );
        }

        public int get_TargetCodePage(out uint puiTargetCodePage)
        {
            return m_innerIVsDebuggableProjectCfg.get_TargetCodePage( out puiTargetCodePage );
        }

        public int EnumOutputs(out IVsEnumOutputs ppIVsEnumOutputs)
        {
            return m_innerIVsDebuggableProjectCfg.EnumOutputs( out ppIVsEnumOutputs );
        }

        public int get_IsDebugOnly(out int pfIsDebugOnly)
        {
            return m_innerIVsDebuggableProjectCfg.get_IsDebugOnly( out pfIsDebugOnly );
        }

        public int QueryDebugLaunch(uint grfLaunch, out int pfCanLaunch)
        {
            return m_innerIVsDebuggableProjectCfg.QueryDebugLaunch( grfLaunch, out pfCanLaunch );
        }

        public int get_IsSpecifyingOutputSupported(out int pfIsSpecifyingOutputSupported)
        {
            return m_innerIVsDebuggableProjectCfg.get_IsSpecifyingOutputSupported( out pfIsSpecifyingOutputSupported );
        }

        public int get_CanonicalName(out string pbstrCanonicalName)
        {
            return m_innerIVsDebuggableProjectCfg.get_CanonicalName( out pbstrCanonicalName );
        }

        public unsafe int DebugLaunch(uint grfLaunch)
        {
            try
            {
                if (!this.m_project.CanLaunch)
                {
                    Utility.ShowMessageBox("The project must have either an output type of 'Console Application', or an output type of 'Class Library' and the start action set to a valid .NET MicroFramework application");
                    return Utility.COM_HResults.E_FAIL;
                }

                //Consider Launching ourselves (at least for device), and then calling Attach.
                //This would get rid of the ugly dummy thread hack in CorDebugProcess.
                //However, we would need to jump through some other hoops, like resuming the process,
                //perhaps setting up the entry point breakpoint ourselves, ..?

                VsDebugTargetInfo2 vsDebugTargetInfo = new VsDebugTargetInfo2();
                DebugPort port = GetDebugPort();
                Process processWin32 = null;
                bool fNoDebug = (__VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug & (__VSDBGLAUNCHFLAGS)grfLaunch) != 0;
                int hRes = Utility.COM_HResults.S_OK;
                PlatformInfo.Emulator emulatorConfig = GetEmulatorConfig();

                if (port == null)
                {
                    throw new Exception("Cannot find port to deploy to");
                }

                string commandLine = GetCommandLineForLaunch(fNoDebug, emulatorConfig, IsTargetBigEndian() );
                string exe = port.IsLocalPort ? emulatorConfig.application : typeof(CorDebugProcess).Assembly.Location;

                if (!fNoDebug)
                {
                    commandLine = string.Format("{0} \"{1}{2}\"", commandLine, CorDebugProcess.c_DeployDeviceName, this.DeployDeviceName);
                }

                //use emulator args even though this may be a device launch.  This is needed to store
                //paths to assemblies.
                vsDebugTargetInfo.bstrArg = commandLine;
                vsDebugTargetInfo.bstrCurDir = (string)m_project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir);
                vsDebugTargetInfo.bstrEnv = null;
                vsDebugTargetInfo.bstrExe = exe;
                vsDebugTargetInfo.bstrOptions = null;
                vsDebugTargetInfo.bstrPortName = DeployTransportName;
                vsDebugTargetInfo.bstrRemoteMachine = null;
                vsDebugTargetInfo.cbSize = (uint)Marshal.SizeOf(vsDebugTargetInfo);
                vsDebugTargetInfo.dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
                vsDebugTargetInfo.dwDebugEngineCount = 0;
                vsDebugTargetInfo.dwProcessId = 0;
                vsDebugTargetInfo.dwReserved = 0;
                vsDebugTargetInfo.fSendToOutputWindow = 0;
                vsDebugTargetInfo.guidLaunchDebugEngine = CorDebug.s_guidDebugEngine;
                vsDebugTargetInfo.guidPortSupplier = DebugPortSupplier.s_guidPortSupplier;
                vsDebugTargetInfo.guidProcessLanguage = Guid.Empty;
                vsDebugTargetInfo.hStdError = 0;
                vsDebugTargetInfo.hStdInput = 0;
                vsDebugTargetInfo.hStdOutput = 0;
                vsDebugTargetInfo.LaunchFlags = grfLaunch;
                vsDebugTargetInfo.pDebugEngines = IntPtr.Zero;
                vsDebugTargetInfo.pUnknown = null;

                if (fNoDebug)
                {
                    if (port.IsLocalPort)
                    {
                        processWin32 = new Process();

                        processWin32.StartInfo = new ProcessStartInfo();
                        processWin32.StartInfo.FileName = vsDebugTargetInfo.bstrExe;
                        processWin32.StartInfo.Arguments = vsDebugTargetInfo.bstrArg;
                        processWin32.StartInfo.WorkingDirectory = vsDebugTargetInfo.bstrCurDir;
                        processWin32.StartInfo.UseShellExecute = false;
                        processWin32.Start();
                    }
                    else
                    {
                        RebootAndResumeExecution();
                    }
                }
                else
                {
                    byte* bpDebugTargetInfo = stackalloc byte[(int)vsDebugTargetInfo.cbSize];
                    IntPtr ipDebugTargetInfo = (IntPtr)bpDebugTargetInfo;
                    try
                    {
                        IVsDebugger2 iVsDebugger = ( IVsDebugger2 )ServiceProvider.GlobalProvider.GetService( typeof( IVsDebugger ) );
                        Marshal.StructureToPtr(vsDebugTargetInfo, ipDebugTargetInfo, false);
                        hRes = iVsDebugger.LaunchDebugTargets2(1, ipDebugTargetInfo);
                    }
                    finally
                    {
                        if (ipDebugTargetInfo != null)
                            Marshal.DestroyStructure(ipDebugTargetInfo, vsDebugTargetInfo.GetType());
                    }
                }

                return hRes;
            }
            catch (Exception ex)
            {
                Utility.ShowMessageBox(String.Format("An exception occurred while attempting to launch the debugger: {0}", ex.Message));
                return Utility.COM_HResults.E_FAIL;
            }
        }

        #endregion

        #region IVsDeployableProjectCfg Members

        int IVsDeployableProjectCfg.Rollback(uint dwReserved)
        {
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int IVsDeployableProjectCfg.AdviseDeployStatusCallback(IVsDeployStatusCallback pIVsDeployStatusCallback, out uint pdwCookie)
        {
            m_connectionsDeployStatusCallback.Advise(pIVsDeployStatusCallback, out pdwCookie);
            return Utility.COM_HResults.S_OK;
        }

        int IVsDeployableProjectCfg.QueryStatusDeploy(out int pfDeployDone)
        {
            // VsProjectFlavorCfg.QueryStatusDeploy is not implemented
            pfDeployDone = Utility.Boolean.FALSE;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int IVsDeployableProjectCfg.Commit(uint dwReserved)
        {
            // VsProjectFlavorCfg.Commit is not implemented
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int IVsDeployableProjectCfg.UnadviseDeployStatusCallback(uint dwCookie)
        {
            m_connectionsDeployStatusCallback.Unadvise(dwCookie);
            return Utility.COM_HResults.S_OK;
        }

        int IVsDeployableProjectCfg.StopDeploy(int fSync)
        {
            if (m_deployState != null)
            {
                Thread thread = m_deployState.m_threadDeploy;

                if (thread != null)
                {
                    thread.Abort();

                    if (Utility.Boolean.IntToBool(fSync))
                    {
                        thread.Join();
                    }
                }
            }

            return Utility.COM_HResults.S_OK;
        }

        int IVsDeployableProjectCfg.StartDeploy(IVsOutputWindowPane pIVsOutputWindowPane, uint dwOptions)
        {
            VsPackage.MessageCentre.ClearDeploymentMsgs();
            VsPackage.MessageCentre.StartProgressMsg(DiagnosticStrings.StartDeploy);
            VsPackage.MessageCentre.DeploymentMsg(string.Format(DiagnosticStrings.LookingUpDevice, this.DeployTransportName));

            int hr;

            m_deployState = new DeployState();
            m_deployState.m_outputWindowPane = pIVsOutputWindowPane;

            foreach (IVsDeployStatusCallback dsc in m_connectionsDeployStatusCallback)
            {
                int iContinue = Utility.Boolean.TRUE;
                dsc.OnStartDeploy(ref iContinue);
                m_deployState.m_deploySuccess = m_deployState.m_deploySuccess && (iContinue == Utility.Boolean.TRUE);
            }

            DebugPort port = GetDebugPort();

            if (port == null)
                SetDeployFailure(string.Format("Invalid device transport setting '{0}', could not find a debug port", this.DeployTransportName));
            else
                VsPackage.MessageCentre.DeploymentMsg(string.Format( DiagnosticStrings.FoundDevicePort, port.Name, port.PortId, port.PortFilter));

            hr = m_deployState.m_deploySuccess ? Utility.COM_HResults.S_OK : Utility.COM_HResults.E_FAIL;

            if (m_deployState.m_deploySuccess && !port.IsLocalPort)
            {
                m_deployState.m_deployCallbackControl = new System.Windows.Forms.Control();
                m_deployState.m_deployCallbackControl.CreateControl();
                m_deployState.m_threadDeploy = new Thread(delegate()
                    {
                        try
                        {
                            Deploy(port);
                        }
                        catch (Exception ex)
                        {
                            SetDeployFailure("An error has occurred: please check your hardware.",
                                "An error has occurred: please check your hardware.\n"
                                + ex.Message + "\n"
                                + "Source: " + ex.Source + "\n"
                                + "Stack : \n"
                                + ex.StackTrace + "\n"
                                );
                        }

                        m_deployState.m_deployCallbackControl.Invoke(new DeployState.EndDeployDelegate(OnEndDeploy));
                    });
                m_deployState.m_threadDeploy.Start();
            }
            else
            {
                OnEndDeploy();
            }

            return hr;
        }

        int IVsDeployableProjectCfg.WaitDeploy(uint dwMilliseconds, int fTickWhenMessageQNotEmpty)
        {
            // VsProjectFlavorCfg.WaitDeploy is not implemented
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int IVsDeployableProjectCfg.QueryStartDeploy(uint dwOptions, int[] pfSupported, int[] pfReady)
        {
            Debug.Assert(pfSupported == null || pfSupported.Length > 0);
            Debug.Assert(pfReady == null || pfReady.Length > 0);
            Debug.Assert(dwOptions == 0);

            if (pfSupported != null)
                pfSupported[0] = Utility.Boolean.TRUE;

            if (pfReady != null)
                pfReady[0] = Utility.Boolean.TRUE;

            DebugPort port = GetDebugPort();
            if(port != null && !port.IsLocalPort)
            {
                // run this prior to deploy to assure we get the assembly list before
                // deploying, because build tasks are not allowed during deployment.
                m_project.GetDependencies(true, true , false);
            }

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        private void OnEndDeploy()
        {
            if (m_deployState == null)
            {
                VsPackage.MessageCentre.StopProgressMsg();
            }
            else
            {
                int iSuccessful = Utility.Boolean.BoolToInt(m_deployState.m_deploySuccess);

                foreach (IVsDeployStatusCallback dsc in m_connectionsDeployStatusCallback)
                {
                    dsc.OnEndDeploy(iSuccessful);
                }

                if (!m_deployState.m_deploySuccess)
                {
                    m_deployState.m_outputWindowPane.OutputTaskItemString(m_deployState.m_deployOuputMessage, VSTASKPRIORITY.TP_HIGH, VSTASKCATEGORY.CAT_BUILDCOMPILE, null, 0, null, 0, m_deployState.m_deployTaskItem);
                    m_deployState.m_outputWindowPane.FlushToTaskList();
                }

                if (m_deployState.m_deployCallbackControl != null)
                {
                    m_deployState.m_deployCallbackControl.Dispose();
                }

                m_deployState = null;

                VsPackage.MessageCentre.StopProgressMsg(iSuccessful != 0 ? DiagnosticStrings.DeploySucceeded : DiagnosticStrings.DeployFailed);
            }
        }

        internal DebugPort GetDebugPort()
        {
            string deployPortName = this.DeployTransportName;

            DebugPort port = new DebugPortSupplier().FindPort(deployPortName);

            if(port == null)
                VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.InvalidPort, deployPortName));
            return port;
        }

        internal void EnsureDeviceProcess()
        {
            DebugPort port = GetDebugPort();

            if (!port.IsLocalPort)
            {
                CorDebugProcess process = port.GetDeviceProcess(this.DeployDeviceName);
            }
        }

        internal CorDebugProcess GetDeviceProcess()
        {
            DebugPort port = GetDebugPort();
            Debug.Assert(!port.IsLocalPort);

            return port.GetDeviceProcess(this.DeployDeviceName);
        }

        private void SetDeployFailure(string taskMessage, string outputMessage)
        {
            if (string.IsNullOrEmpty(taskMessage))
                taskMessage = "An error has occurred.  Please check your hardware";
            if (string.IsNullOrEmpty(outputMessage))
                outputMessage = taskMessage;

            Debug.Assert(m_deployState != null);
            m_deployState.m_deploySuccess = false;
            m_deployState.m_deployTaskItem = taskMessage;
            m_deployState.m_deployOuputMessage = outputMessage;
        }

        private void SetDeployFailure(string message)
        {
            SetDeployFailure(message, message);
        }

        private void SetDeployFailure()
        {
            SetDeployFailure(null, null);
        }

        private string DeployDeviceDescription
        {
            get
            {
                return string.Format("{0}:{1}", this.DeployTransportName, this.DeployDeviceName);
            }
        }

        private bool IsTargetBigEndian()
        {
            bool fBE;

            DebugPort port = GetDebugPort();

            if(port == null) return false;

            if (!port.IsLocalPort)
            {
                CorDebugProcess process = GetDeviceProcess();
                bool detach = !process.IsAttachedToEngine;
                Engine engine = process.AttachToEngine();
                fBE = engine.IsTargetBigEndian;
                if(detach)
                {
                    process.DetachFromEngine();
                }
            }
            else
            {
                // emulator is little endian
                fBE = false;
            }
            return fBE;
        }

        private void Deploy(DebugPort port)
        {
            Debug.Assert(port != null && !port.IsLocalPort);

            VsPackage.MessageCentre.InternalErrorMsg(DiagnosticStrings.StartDeployAssemblies);

            CorDebugProcess process = null;
            bool fDeviceFound = false;

            VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.StartingDeviceDeployment);
            
            for(int retries = 0; retries < 60; retries++)
            {
                VsPackage.MessageCentre.DeploymentMsg( String.Format( DiagnosticStrings.Iteration, port.Name, DeployDeviceName, retries ) );
                
                try
                {   
                    process = GetDeviceProcess();

                    if (process != null)
                    {
                        switch (port.PortFilter)
                        {
                            case PortFilter.Serial:
                            case PortFilter.Usb:
                            case PortFilter.TcpIp:
                                //test the connection.  This doesn't tell you if the connection will fail during the AttachToEngine,
                                //but it's a pretty good guess.
                                VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.OpeningPort, process.PortDefinition.Port));
                                fDeviceFound = process.PortDefinition.TryToOpen();
                                if(!fDeviceFound)
                                {
                                    VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.PortNotFound, process.PortDefinition.Port));
                                }
                                break;
                            default:
                                SetDeployFailure(string.Format("Device {0} has an unsupported/unexpected port type", this.DeployDeviceDescription));
                                return;
                        }

                        if(fDeviceFound) break;
                    }
                }
                catch(IOException)
                {
                }

                Thread.Sleep(500);                
            }

            if (!fDeviceFound || process == null)
            {
                SetDeployFailure(string.Format("Device not found or cannot be opened - {0}", this.DeployDeviceDescription));
                return;
            }
            
            VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.AttachingEngine);

            Engine engine = process.AttachToEngine();

            if (engine == null)
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.CannotAttachEngine);
                SetDeployFailure(string.Format(DiagnosticStrings.UnableToCommunicate, this.DeployDeviceDescription));
                return;
            }
            
            VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.EngineAttached);

            try
            {
                
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.QueryingDeviceAssemblies);
                
                ArrayList assemblies = new ArrayList();
                Hashtable systemAssemblies = new Hashtable();
                //Ensure Assemblies are loaded
                if(process.IsDeviceInInitializeState())
                {
                    engine.ResumeExecution();
                    Thread.Sleep(200);                    

                    while (process.IsDeviceInInitializeState())
                    {
                        VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.WaitingInitialize);
                    
                        //need to break out of this or timeout or something?
                        Thread.Sleep(200);
                    }
                }
                
                WireProtocol.Commands.Debugging_Resolve_Assembly[] assms = engine.ResolveAllAssemblies();
                
                // find out which are the system assemblies
                // we will insert each system assemblies in a hash table where the value will be the assembly version
                foreach (Debugging_Resolve_Assembly resolvedAssembly in assms)
                {
                    VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.FoundAssembly, resolvedAssembly.m_reply.Name, resolvedAssembly.m_reply.m_version.ToString()));
                    if ((resolvedAssembly.m_reply.m_flags & Debugging_Resolve_Assembly.Reply.c_Deployed) == 0)
                    {
                        systemAssemblies[resolvedAssembly.m_reply.Name.ToLower()] = resolvedAssembly.m_reply.m_version;
                    }
                }

                string[] pes = m_project.GetDependencies(true, true, engine.IsTargetBigEndian);
                string[] dlls = m_project.GetDependencies(true, false, engine.IsTargetBigEndian);

                Debug.Assert( pes.Length == dlls.Length );

                // now we will re-deploy all system assemblies
                for(int i = 0; i < pes.Length; ++i)
                {
                    string assemblyPath = pes[i];
                    string dllPath = dlls[i];
                    
                    //is this a system assembly?
                    string fileName = Path.ChangeExtension(Path.GetFileName(assemblyPath), null).ToLower();
                    bool fDeployNewVersion = true;

                    if (systemAssemblies.ContainsKey(fileName))
                    {
                        // get the version of the assembly on the device 
                        Debugging_Resolve_Assembly.Version deviceVer = (WireProtocol.Commands.Debugging_Resolve_Assembly.Version)systemAssemblies[fileName];
                        
                        // get the version of the assembly of the project
                        // We need to load the bytes for the assembly because other Load methods can override the path
                        // with gac or recently used paths.  This is the only way we control the exact assembly that is loaded.
                        byte[] asmData = null;

                        using(FileStream sr = new FileStream(dllPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            asmData = new byte[sr.Length];
                            sr.Read(asmData, 0, (int)sr.Length);
                        }

                        System.Reflection.Assembly assm = Assembly.Load(asmData); Version deployVer = assm.GetName().Version;

                        // compare versions strictly, and deploy whatever assembly does not match the version on the device
                        if (VsProject.TargetFrameworkExactMatch(deviceVer, deployVer))
                        {
                            fDeployNewVersion = false;
                        }
                        else
                        {
                            //////////////////////////////////////////////// 
                            //            !!! SPECIAL CASE !!!            //
                            //                                            //
                            // MSCORLIB cannot be deployed more than once //
                            ////////////////////////////////////////////////

                            if (assm.GetName().Name.ToLower().Contains("mscorlib"))
                            {
                                string message = string.Format("Cannot deploy the base assembly '{9}', or any of his satellite assemblies, to device - {0} twice. Assembly '{9}' on the device has version {1}.{2}.{3}.{4}, while the program is trying to deploy version {5}.{6}.{7}.{8} ", this.DeployDeviceDescription, deviceVer.iMajorVersion, deviceVer.iMinorVersion, deviceVer.iBuildNumber, deviceVer.iRevisionNumber, deployVer.Major, deployVer.Minor, deployVer.Build, deployVer.Revision, assm.GetName().Name);
                                VsPackage.MessageCentre.DeploymentMsg( message );
                                SetDeployFailure(message);
                                return;
                            }
                        }
                    }
                    // append the assembly whose version does not match, or that still is not on the device, to the blob to deploy
                    if (fDeployNewVersion)
                    {
                        using (FileStream fs = File.Open(assemblyPath, FileMode.Open, FileAccess.Read))
                        {
                            VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.AddingPEtoBundle, assemblyPath));
                            long length = (fs.Length + 3) / 4 * 4;
                            byte[] buf = new byte[length];

                            fs.Read(buf, 0, (int)fs.Length);
                            assemblies.Add(buf);
                        }
                    }
                }
                
                VsPackage.MessageCentre.DeploymentMsg("Attempting deployment...");

                if (!engine.Deployment_Execute(assemblies, false, VsPackage.MessageCentre.DeploymentMsg))
                {
                    VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.DeployFailed);
                    SetDeployFailure();
                    return;
                }
            }
            finally
            {
                process.DetachFromEngine();
            }
        }

        private void RebootAndResumeExecution()
        {
            CorDebugProcess process = GetDeviceProcess();

            Engine engine = process.Engine;

            Debug.Assert(engine == null);

            engine = process.AttachToEngine();

            try
            {
                if (!process.IsDeviceInInitializeState())
                {
                    engine.RebootDevice(Engine.RebootOption.RebootClrOnly);
                }
            }
            finally
            {
                process.DetachFromEngine();
            }
        }
    }

    public class ProjectBuildProperty
    {
        private IVsBuildPropertyStorage m_storage;
        private VsProjectFlavorCfg m_cfg;
        private string m_propName;
        private string m_val;
        private _PersistStorageType m_type;
        private bool m_fPropertyLoaded; //This lets the ProjectBuildProperty get loaded before the project file is finished loading
        private string m_valDefault;
        private _PersistStorageType m_typeDefault;

        public const _PersistStorageType PersistStorageTypeNone = (_PersistStorageType)0;

        public ProjectBuildProperty(string propName, IVsBuildPropertyStorage storage, VsProjectFlavorCfg cfg, _PersistStorageType type, _PersistStorageType typeDefault, string valDefault)
        {
            m_propName = propName;
            m_storage = storage;
            m_cfg = cfg;
            m_type = type;
            m_fPropertyLoaded = false;
            m_valDefault = valDefault;
            m_typeDefault = typeDefault;
        }

        public ProjectBuildProperty(string propName, IVsBuildPropertyStorage storage, VsProjectFlavorCfg cfg, _PersistStorageType type, _PersistStorageType typeDefault)
            : this(propName, storage, cfg, type, typeDefault, string.Empty)
        {
        }

        public ProjectBuildProperty(string propName, IVsBuildPropertyStorage storage, VsProjectFlavorCfg cfg, _PersistStorageType type)
            : this(propName, storage, cfg, type, PersistStorageTypeNone, string.Empty)
        {
        }

        protected string GetConfigName(bool fIncludePlatform)
        {
            return m_cfg.GetName(fIncludePlatform);
        }

        private string GetPropertyValueHelper(_PersistStorageType type)
        {
            string val = null;

            if (m_storage == null)
                return null;

            try
            {
                m_storage.GetPropertyValue(m_propName, this.GetConfigName(false), (uint)type, out val);
            }
            catch (COMException e)
            {
                if (e.ErrorCode != Utility.COM_HResults.ERR_XML_ATTRIBUTE_NOT_FOUND)
                {
                    throw;
                }
            }

            return val;
        }

        public string Value
        {
            get
            {
                if (!m_fPropertyLoaded)
                {
                    //get initial property

                    m_val = GetPropertyValueHelper(m_type);

                    if (string.IsNullOrEmpty(m_val))
                    {
                        if (m_typeDefault != ProjectBuildProperty.PersistStorageTypeNone)
                        {
                            m_val = GetPropertyValueHelper(m_typeDefault);
                        }
                    }

                    if (string.IsNullOrEmpty(m_val))
                    {
                        m_val = m_valDefault;
                    }

                    m_fPropertyLoaded = true;
                }

                return m_val;
            }

            set
            {
                if (m_val != value)
                {
                    if (value == null)
                    {
                        m_storage.RemoveProperty(m_propName, this.GetConfigName(true), (uint) m_type);
                    }
                    else
                    {
                        m_storage.SetPropertyValue(m_propName, this.GetConfigName(true), (uint) m_type, value);
                    }

                    m_val = value;
                }
            }
        }

        protected bool IsNull
        {
            get { return m_val == null; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            ProjectBuildProperty prop = (ProjectBuildProperty) obj;
            return this.m_propName == prop.m_propName && this.m_val == prop.m_val;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class ProjectBuildPropertyBool : ProjectBuildProperty
    {
        private bool m_defaultValue = false;

        public ProjectBuildPropertyBool(string propName, IVsBuildPropertyStorage storage, VsProjectFlavorCfg cfg, _PersistStorageType type) : base (propName, storage, cfg, type)
        {
        }

        public new bool Value
        {
            get
            {
                bool val;
                
                if(!bool.TryParse(base.Value, out val))
                {
                    val = m_defaultValue;
                }

                return val;
            }

            set { base.Value = value.ToString(); }
        }
    }
}
