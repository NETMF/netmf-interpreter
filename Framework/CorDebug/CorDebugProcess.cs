using System;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Microsoft.SPOT.Debugger;
using WireProtocol = Microsoft.SPOT.Debugger.WireProtocol;
using BreakpointDef = Microsoft.SPOT.Debugger.WireProtocol.Commands.Debugging_Execution_BreakpointDef;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.Win32.SafeHandles;
using System.IO.Pipes;
using Microsoft.VisualStudio;
using System.Text.RegularExpressions;

namespace Microsoft.SPOT.Debugger
{
    public class CorDebugProcess : ICorDebugController,
                                   ICorDebugProcess,
                                   ICorDebugProcess2,
                                   IDebugProcess2,
                                   IDebugProcessEx2,
                                   IDisposable
    {
        CorDebug m_corDebug;
        Queue m_events;
        ArrayList m_threads;
        ArrayList m_assemblies;
        ArrayList m_appDomains;
        CorDebugAppDomain m_appDomainCurrent;
        string[] m_assemblyPaths;
        ArrayList m_breakpoints;
        Engine m_engine;
        bool m_fExecutionPaused;
        AutoResetEvent m_eventDispatch;
        ManualResetEvent m_eventExecutionPaused;
        ManualResetEvent m_eventProcessExited;
        EventWaitHandle[] m_eventsStopped;
        uint m_pid;
        bool m_fUpdateBreakpoints;
        int m_cStopped;
        DebugPort m_port;
        PortDefinition m_portDefinition;
        Guid m_guidProcessId;
        bool m_fLaunched;       //whether the attach was from a launch (vs. an attach)
        bool m_fTerminating;
        bool m_fDetaching;
        Utility.Kernel32.CreateThreadCallback m_dummyThreadDelegate;
        AutoResetEvent m_dummyThreadEvent;
        int m_cEvalThreads;
        Hashtable m_tdBuiltin;
        ScratchPadArea m_scratchPad;
        Process m_win32process;
        ulong m_fakeAssemblyAddressNext;
        object m_syncTerminatingObject;
        Thread m_threadDispatch;

        const ulong c_fakeAddressStart = 0x100000000;

        internal const string c_DeployDeviceName = "/CorDebug_DeployDeviceName:";

        public CorDebugProcess(DebugPort port, PortDefinition portDefinition)
        {
            m_port = port;
            m_portDefinition = portDefinition;
            m_guidProcessId = Guid.NewGuid();
            m_syncTerminatingObject = new object();
        }

        public void Dispose() 
        {
          Dispose(true);
          GC.SuppressFinalize(this); 
        }

        bool m_fDisposed = false;

        protected virtual void Dispose(bool disposing) 
        {
            if (!m_fDisposed)
            {
                if (disposing)
                {
                    // free managed stuff
                }

                // free "unmanaged" stuff
                StopDebugging(); 
                m_fDisposed = true;
            }
        }

        ~CorDebugProcess()
        {
           Dispose (false);
        }


        public PortDefinition PortDefinition
        {
            get { return m_portDefinition; }
            set { m_portDefinition = value; }
        }

        public CorDebug CorDebug
        {
            get { return m_corDebug; }
        }

        public bool IsAttachedToEngine
        {
            get { return m_engine != null; }
        }

        public bool IsLocalWin32Process
        {
            get { return m_port.IsLocalPort; }
        }

        public void SetPid(uint pid)
        {
            if (m_pid != 0)
                throw new ArgumentException("PID already set");

            m_pid = pid;
        }

        public ScratchPadArea ScratchPad
        {
            get { return m_scratchPad; }
        }

        public bool IsDebugging
        {
            get { return m_corDebug != null; }
        }

        public void SetCurrentAppDomain( CorDebugAppDomain appDomain )
        {
            if(appDomain != m_appDomainCurrent)
            {
                if(appDomain != null && this.Engine.Capabilities.AppDomains)
                {
                    this.Engine.SetCurrentAppDomain( appDomain.Id );
                }
            }

            m_appDomainCurrent = appDomain;
        }

        private void OnProcessExit(object sender, EventArgs args)
        {
            uint errorCode = 0;
            try
            {
                Process process = sender as Process;

                if (process != null && m_win32process != null)
                {
                    errorCode = (uint)process.ExitCode;
                }
            }
            catch
            {
            }

            this.ICorDebugProcess.Terminate(errorCode);
        }

        private void Init(CorDebug corDebug, bool fLaunch)
        {
            try
            {
                if (IsDebugging)
                    throw new Exception("CorDebugProcess is already in debugging mode before Init() has run");

                m_corDebug = corDebug;
                m_fLaunched = fLaunch;

                m_events = Queue.Synchronized(new Queue());
                m_threads = new ArrayList();
                m_assemblies = new ArrayList();
                m_appDomains = new ArrayList();
                m_breakpoints = ArrayList.Synchronized(new ArrayList());
                m_fExecutionPaused = true;
                m_eventDispatch = new AutoResetEvent(false);
                m_eventExecutionPaused = new ManualResetEvent(true);
                m_eventProcessExited = new ManualResetEvent(false);
                m_eventsStopped = new EventWaitHandle[] { m_eventExecutionPaused, m_eventProcessExited };
                m_fUpdateBreakpoints = false;
                m_cStopped = 0;
                m_fTerminating = false;
                m_cEvalThreads = 0;
                m_tdBuiltin = null;
                m_scratchPad = new ScratchPadArea(this);
                m_fakeAssemblyAddressNext = c_fakeAddressStart;
                m_threadDispatch = null;

                if (this.IsLocalWin32Process)
                {
                    m_win32process = Process.GetProcessById((int)m_pid);
                    m_win32process.EnableRaisingEvents = true;
                    m_win32process.Exited += new EventHandler(OnProcessExit);
                }

                m_corDebug.RegisterProcess(this);
            }
            catch (Exception)
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.DeploymentErrorDeviceErrors);
                throw;
            }
        }

        private void UnInit()
        {
            if (m_assemblies != null)
            {
                foreach (CorDebugAssembly assembly in m_assemblies)
                {
                    ((IDisposable)assembly).Dispose();
                }

                m_assemblies = null;
            }

            if(m_win32process != null)
            {
                m_win32process.EnableRaisingEvents = false;

                // If the process hasn't already exited, we'll wait another 2 sec and kill it.
                for (int i = 1; !m_win32process.HasExited && i <= 2; i++)
                {
                    if (i == 1)
                    {
                        Thread.Yield();
                    }
                    else
                    {
                        try
                        { 
                            m_win32process.Kill();
                        }
                        catch(Win32Exception)
                        {
                        }
                        catch( NotSupportedException )
                        {
                        }
                        catch( InvalidOperationException )
                        {
                        }
                    }
                }

                m_win32process.Dispose();
                m_win32process = null;
            }

            if (m_port != null)
            {
                m_port.RemoveProcess(this.m_portDefinition);
                m_port = null;
            }

            m_appDomains = null;
            m_events = null;
            m_threads = null;
            m_assemblyPaths = null;
            m_breakpoints = null;
            m_fExecutionPaused = false;
            m_eventDispatch = null;
            m_fUpdateBreakpoints = false;
            m_cStopped = 0;
            m_fLaunched = false;
            m_fTerminating = false;
            m_fDetaching = false;
            m_dummyThreadEvent = null;
            m_cEvalThreads = 0;
            m_tdBuiltin = null;
            m_scratchPad = null;

            m_threadDispatch = null;

            if (m_corDebug != null)
            {
                m_corDebug.UnregisterProcess(this);
                m_corDebug = null;
            }
        }

        public void DirtyBreakpoints()
        {
            m_fUpdateBreakpoints = true;

            if (!IsExecutionPaused)
            {
                UpdateBreakpoints();
            }
        }

        public void DetachFromEngine()
        {
            lock (this)
            {
                if (m_engine != null)
                {
                    m_engine.OnMessage -= new MessageEventHandler(OnMessage);
                    m_engine.OnCommand -= new CommandEventHandler(OnCommand);
                    m_engine.OnNoise -= new NoiseEventHandler(OnNoise);
                    m_engine.OnProcessExit -= new EventHandler(OnProcessExit);

                    try
                    {
                        m_engine.Stop();
                        m_engine.Dispose();
                    }
                    catch
                    {
                        // Depending on when we get called, stopping the engine 
                        // throws anything from NullReferenceException, ArgumentNullException, IOException, etc.
                    }

                    m_engine = null;
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        private void EnsureProcessIsInInitializedState()
        {
            if (!IsDeviceInInitializeState())
            {
                bool fSucceeded = false;
                VsPackage.MessageCentre.StartProgressMsg(DiagnosticStrings.Rebooting);

                AttachToEngine();

                m_engine.RebootDevice(Engine.RebootOption.RebootClrWaitForDebugger);

                if(m_engine.PortDefinition is PortDefinition_Tcp)
                {
                    DetachFromEngine();
                }

                for(int retries=0; retries<5; retries++)
                {
                    if (AttachToEngine() != null)
                    {
                        if(m_engine.ConnectionSource == ConnectionSource.TinyCLR)
                        {
                            if(IsDeviceInInitializeState())
                            {
                                fSucceeded = true;
                                break;
                            }

                            m_engine.RebootDevice(Engine.RebootOption.RebootClrWaitForDebugger);

                            Thread.Yield();
                        }
                        else if(m_engine.ConnectionSource == ConnectionSource.TinyBooter)
                        {
                            m_engine.ExecuteMemory(0); // tell tinybooter to enter CLR
                            Thread.Yield();
                        }
                        else
                        {
                            Thread.Yield();
                        }
                    }
                }
                
                if (!ShuttingDown && !fSucceeded)
                {
                    VsPackage.MessageCentre.StopProgressMsg();
                    throw new Exception(DiagnosticStrings.CouldNotReconnect);
                }
            }
            VsPackage.MessageCentre.StopProgressMsg(DiagnosticStrings.TargetInitializeSuccess);
        }

        public Engine AttachToEngine()
        {
            int c_maxRetries     = 30;
            int c_retrySleepTime = 300;

            for(int retry=0; retry<c_maxRetries; retry++)
            {
                if(ShuttingDown) break;
                
                try
                {
                    lock(this)
                    {
                        if (m_engine == null)
                        {
#if USE_CONNECTION_MANAGER
                            m_engine = m_port.DebugPortSupplier.Manager.Connect(m_portDefinition);
#else
                            m_engine = new Engine(m_portDefinition);
#endif
                            m_engine.StopDebuggerOnConnect = true;
                            m_engine.Start();

                            m_engine.OnMessage += new MessageEventHandler(OnMessage);
                            m_engine.OnCommand += new CommandEventHandler(OnCommand);
                            m_engine.OnNoise   += new NoiseEventHandler(OnNoise);
                            m_engine.OnProcessExit += new EventHandler(OnProcessExit);
                        }
                        else
                        {
                            m_engine.ThrowOnCommunicationFailure = false;
                            m_engine.StopDebuggerOnConnect = true;                        
                        }
                    }
                    if( m_engine.TryToConnect( 3, c_retrySleepTime, true, ConnectionSource.Unknown ) )
                    {
                        if(m_engine.ConnectionSource == ConnectionSource.TinyBooter)
                        {
                            m_engine.ExecuteMemory(0);
                            Thread.Yield();
                        }
                        m_engine.ThrowOnCommunicationFailure = true;
                        m_engine.SetExecutionMode( WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_SourceLevelDebugging, 0 );
                        break;
                    }

                    // only detach-reattach after 10 retries (10 seconds)
                    if((retry % 10) == 9) 
                        DetachFromEngine();

                    Thread.Yield();
                }
                catch
                {
                    DetachFromEngine();

                    if(!ShuttingDown)
                    {
                        Thread.Yield();
                    }
                }
            }

            if(m_engine != null && !m_engine.IsConnected)
            {
                DetachFromEngine();
            }

            return m_engine;
        }

        public void EnqueueEvent(ManagedCallbacks.ManagedCallback cb)
        {
            m_events.Enqueue(cb);
            m_eventDispatch.Set();
        }

        public void EnqueueEvents(List<ManagedCallbacks.ManagedCallback> callbacks)
        {
            for (int i = 0; i < callbacks.Count; i++)
            {
                m_events.Enqueue(callbacks[i]);
            }

            m_eventDispatch.Set();
        }

        private bool FlushEvent()
        {
            bool fContinue = false;
            ManagedCallbacks.ManagedCallback mc = null;

            lock(this)
            {
                if(m_cStopped == 0 && AnyQueuedEvents)
                {
                    Interlocked.Increment( ref m_cStopped );

                    mc = (ManagedCallbacks.ManagedCallback)m_events.Dequeue();
                }
            }

            if(mc != null)
            {
                DebugAssert(this.ShuttingDown || this.IsExecutionPaused || mc is ManagedCallbacks.ManagedCallbackDebugMessage);

                mc.Dispatch( m_corDebug.ManagedCallback );
                fContinue = true;
            }

            return fContinue;
        }

        private void FlushEvents()
        {
            while (FlushEvent())
            {
            }
        }

        private bool ShuttingDown
        {
            get
            {
                DebugAssert(!(m_fTerminating && m_fDetaching));

                return m_fTerminating || m_fDetaching;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void UpdateExecutionIfNecessary()
        {
            DebugAssert(m_cStopped >= 0);

            if(m_cStopped > 0)
            {
                PauseExecution();
            }
            else if(!AnyQueuedEvents)
            {
                if(IsExecutionPaused)
                {
                    /*
                     * cStopped count is not maintained on the TinyCLR.
                     * There is a race condition where we try to resume execution
                     * while the TinyCLR is telling us that a breakpoint is hit.  Specifically,
                     * the following can occur.
                     *
                     * 1.  CorDebug tells TinyCLR to resume
                     * 2.  TinyCLR hits a breakpoint
                     * 3.  CorDebug tells TinyCLR to stop
                     * 4.  CorDebug tells TinyCLR to resume
                     * 5.  CorDebug is notified that breakpoint is hit.
                     *
                     * Draining breakpoints is necessary here to avoid the race condition
                     * that step 4 should not be allowed to occur until all breakpoints are drained.
                     * This amounts to breakpoints being skipped.
                     *
                     * Note the asymmetry here.  CorDebug can at any time tell TinyCLR to stop, and be sure that execution
                     * is stopped.  But CorDebug should not tell TinyCLR to resume execution if there are any outstanding
                     * breakpoints available, regardless of whether CorDebug has been notified yet.
                     */
                    DrainBreakpoints();

                    if(!AnyQueuedEvents)
                    {
                        ResumeExecution();
                    }
                }
            }

            DebugAssert(m_cStopped >= 0);
        }

        private void DispatchEvents()
        {
            VsPackage.MessageCentre.InternalErrorMsg(false, DiagnosticStrings.DispatchEvents);

            try
            {
                while (IsAttachedToEngine && !ShuttingDown)
                {
                    m_eventDispatch.WaitOne();
                    FlushEvents();

                    UpdateExecutionIfNecessary();
                }
            }
            catch (Exception e)
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.DispatchEventsFailed);

                DebugAssert(!(IsAttachedToEngine && !ShuttingDown), "Event dispatch failed:" + e.Message);
                this.ICorDebugProcess.Terminate(0);
            }
        }

        private void EnqueueStartupEventsAndWait()
        {
            DebugAssert(m_pid != 0);    //cpde will fail
            DebugAssert(m_portDefinition != null);

            EnqueueEvent(new ManagedCallbacks.ManagedCallbackProcess(this, ManagedCallbacks.ManagedCallbackProcess.EventType.CreateProcess));

            CorDebugAppDomain appDomain = new CorDebugAppDomain(this, 1);
            m_appDomains.Add(appDomain);

            EnqueueEvent(new ManagedCallbacks.ManagedCallbackAppDomain(appDomain, ManagedCallbacks.ManagedCallbackAppDomain.EventType.CreateAppDomain));

            if (m_fLaunched)
            {
                //the sdm tries to resumeProcess through the DE, but cpde doesn't propagate that message to our
                //Debug Port.  As a result, we have to create a dummy thread, just so that cpde can resume it.
                //We wait for initialization to be complete, by getting signaled by the dummy thread.
                VsPackage.MessageCentre.InternalErrorMsg(false, DiagnosticStrings.WaitingForWakeup);
                WaitDummyThread();
            }

            //Dispatch the CreateProcess/CreateAppDomain events, so that VS will hopefully shut down nicely from now on
            FlushEvents();

            DebugAssert(m_cStopped == 0);
            DebugAssert(!AnyQueuedEvents);
        }

        private uint GetDeviceState()
        {
            uint executionMode = 0;

            try
            {
                if(m_engine.SetExecutionMode(0, 0, out executionMode))
                {
                    return (executionMode & WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_State_Mask);
                }
            }
            catch
            {
            }

            return WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_State_Mask; // error condition
        }

        internal bool IsDeviceInExitedState()
        {
            return GetDeviceState() == WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_State_ProgramExited;
        }

        internal bool IsDeviceInInitializeState()
        {
            return GetDeviceState() == WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_State_Initialize;
        }


        private void StartClr()
        {
            VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.RestartingClr));
            try
            {
                EnqueueStartupEventsAndWait();
                
                VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.AttachingToDevice));

                if (AttachToEngine() == null)
                {
                    VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.DeploymentErrorReconnect));
                    
                    throw new Exception(DiagnosticStrings.DebugEngineAttachmentFailure);
                }

                CorDebugBreakpointBase breakpoint = new CLREventsBreakpoint(this);

                if (m_fLaunched)
                {
                    if (m_port.IsLocalPort)
                    {
                        //Emulator currently behaves differently from device.  The emulator
                        //doesn't give you a chance to catch assembly loaded events.  So if we launch an
                        //emulator, fire the assembly load events now.
                        UpdateAssemblies();
                    }
                    else
                    {
                        //This will reboot the device if start debugging was done without a deployment
    
                        VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.WaitingInitialize));

                        EnsureProcessIsInInitializedState();
                    }
                }
                else
                {
                    this.IsExecutionPaused = false;
                    PauseExecution();

                    UpdateAssemblies();
                    UpdateThreadList();

                    if (m_threads.Count == 0)
                    {
                        //Check to see if the device has exited
                        if (IsDeviceInExitedState())
                        {
                            VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.DebuggingTargetNotFound);
                            throw new ProcessExitException();
                        }
                    }
                }
            }
            catch(Exception)
            {
                VsPackage.MessageCentre.DeploymentMsg(DiagnosticStrings.InitializationFailed);
                throw;
            }
        }

        private void UpdateThreadList()
        {
        /*
          This is a bit of a hack (or performance improvement, if you prefer)
          The TinyCLR creates threads with wild abandon, but ICorDebug specifies that
          thread creation/destruction events should stop the CLR, and provide callbacks
          This can slow down debugging anything that makes heavy use of threads
          For example...managed drivers, timers, etc...
          So we are faking the events just in time, in a couple of cases --
          when a real breakpoint gets hit, when execution is stopped via BreakAll, etc..
         */

            VsPackage.MessageCentre.InternalErrorMsg(DiagnosticStrings.RunningThreadsInformation);

            uint[] threads = m_engine.GetThreadList();

            if (threads != null)
            {
                ArrayList threadsDeleted = (ArrayList)m_threads.Clone();

                //Find new threads to create
                foreach (uint pid in threads)
                {
                    CorDebugThread thread = this.GetThread(pid);

                    if (thread == null)
                    {
                        this.AddThread(new CorDebugThread(this, pid, null));
                    }
                    else
                    {
                        threadsDeleted.Remove(thread);
                    }
                }

                //Find old threads to delete
                foreach (CorDebugThread thread in threadsDeleted)
                {
                    this.RemoveThread(thread);
                }
            }
        }

        public void StartDebugging(CorDebug corDebug, bool fLaunch)
        {
            try
            {
                if (m_pid == 0)
                    throw new Exception(DiagnosticStrings.BogusCorDebugProcess);

                this.Init(corDebug, fLaunch);

                m_threadDispatch = new Thread(delegate()
                {
                    try
                    {
                        VsPackage.MessageCentre.StartProgressMsg(DiagnosticStrings.DebuggingStarting);
                        this.StartClr();
                        this.DispatchEvents();
                    }
                    catch (Exception ex)
                    {
                        VsPackage.MessageCentre.StopProgressMsg();
                        this.ICorDebugProcess.Terminate(0);
                        VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.DebuggerThreadTerminated, ex.Message));
                    }
                    finally
                    {
                        DebugAssert(ShuttingDown);

                        m_eventProcessExited.Set();

                        if (m_fTerminating)
                        {
                            ManagedCallbacks.ManagedCallbackProcess mc = new ManagedCallbacks.ManagedCallbackProcess(this, ManagedCallbacks.ManagedCallbackProcess.EventType.ExitProcess);

                            mc.Dispatch(m_corDebug.ManagedCallback);
                        }

                        StopDebugging();
                    }
                });
                m_threadDispatch.Start();
            }
            catch (Exception)
            {
                this.ICorDebugProcess.Terminate(0);
                throw;
            }
        }

        public Engine Engine
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_engine; }
        }

        public AD_PROCESS_ID PhysicalProcessId
        {
            get
            {
                AD_PROCESS_ID id = new AD_PROCESS_ID();

                id.ProcessIdType = (uint) AD_PROCESS_ID_TYPE.AD_PROCESS_ID_SYSTEM;
                id.dwProcessId = m_pid;
                return id;
            }
        }

        public bool IsInEval
        {
            get { return m_cEvalThreads > 0; }
        }

        public class ScratchPadArea
        {
            private CorDebugProcess m_process;
            private WeakReference[] m_scratchPad;

            public ScratchPadArea(CorDebugProcess process)
            {
                m_process = process;
            }

            private void ReallocScratchPad(int size)
            {
                DebugAssert(m_scratchPad != null && m_scratchPad.Length < size);
                m_process.Engine.ResizeScratchPad(size);

                //Refresh scratch pad values
                for (int iScratchPad = 0; iScratchPad < m_scratchPad.Length; iScratchPad++)
                {
                    WeakReference wr = m_scratchPad[iScratchPad];

                    if (wr != null)
                    {
                        CorDebugValue val = (CorDebugValue) wr.Target;

                        if (val != null)
                        {
                            RuntimeValue rtv = m_process.Engine.GetScratchPadValue(iScratchPad);

                            val.RuntimeValue = rtv;
                        }
                    }
                }

                WeakReference[] scratchPadT = m_scratchPad;
                m_scratchPad = new WeakReference[size];
                scratchPadT.CopyTo(m_scratchPad, 0);
            }

            public int ReserveScratchBlockHelper()
            {
                if (m_scratchPad == null)
                    m_scratchPad = new WeakReference[0];

                for (int i = 0; i < m_scratchPad.Length; i++)
                {
                    WeakReference wr = (WeakReference) m_scratchPad[i];

                    if (wr == null || wr.Target == null)
                        return i;
                }

                return -1;
            }

            public int ReserveScratchBlock()
            {
                int index = ReserveScratchBlockHelper();

                if (index < 0 && m_scratchPad.Length > 0)
                {
                    GC.Collect();
                    index = ReserveScratchBlockHelper();
                }

                if (index < 0)
                {
                    index = m_scratchPad.Length;
                    ReallocScratchPad(m_scratchPad.Length + 50);
                }

                return index;
            }

            public void Clear()
            {
                if (m_scratchPad != null && m_scratchPad.Length > 0)
                {
                    m_process.Engine.ResizeScratchPad(0);
                    m_scratchPad = null;
                }
            }

            public CorDebugValue GetValue(int index, CorDebugAppDomain appDomain)
            {
                WeakReference wr = m_scratchPad[index];

                if (wr == null)
                {
                    wr = new WeakReference(null);
                    m_scratchPad[index] = wr;
                }

                CorDebugValue val = (CorDebugValue) wr.Target;

                if (val == null)
                {
                    val = CorDebugValue.CreateValue(m_process.Engine.GetScratchPadValue(index), appDomain);
                    wr.Target = val;
                }

                return val;
            }
        }

        public CorDebugThread GetThread(uint id)
        {
            foreach (CorDebugThread thread in m_threads)
            {
                if (id == thread.ID)
                    return thread;
            }

            return null;
        }

        public void AddThread(CorDebugThread thread)
        {
            DebugAssert(!m_threads.Contains(thread));

            m_threads.Add(thread);
            if (thread.IsVirtualThread)
            {
                if (m_cEvalThreads == 0)
                {
                    Engine.SetExecutionMode(WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_NoCompaction | WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_PauseTimers, 0);
                }

                m_cEvalThreads++;
            }
            else
            {
                EnqueueEvent(new ManagedCallbacks.ManagedCallbackThread(thread, ManagedCallbacks.ManagedCallbackThread.EventType.CreateThread));
            }
        }

        public void RemoveThread(CorDebugThread thread)
        {
            if (m_threads.Contains(thread))
            {
                thread.Exited = true;

                m_threads.Remove(thread);
                if (thread.IsVirtualThread)
                {
                    CorDebugEval eval = thread.CurrentEval;
                    eval.EndEval(CorDebugEval.EvalResult.Complete, false);

                    m_cEvalThreads--;
                    if (m_cEvalThreads == 0)
                    {
                        Engine.SetExecutionMode(0, WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_NoCompaction | WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_PauseTimers);
                    }
                }
                else
                {
                    EnqueueEvent(new ManagedCallbacks.ManagedCallbackThread(thread, ManagedCallbacks.ManagedCallbackThread.EventType.ExitThread));
                }
            }
        }

        public bool IsExecutionPaused
        {
            get { return m_fExecutionPaused; }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                m_fExecutionPaused = value;

                if (m_fExecutionPaused)
                    m_eventExecutionPaused.Set();
                else
                    m_eventExecutionPaused.Reset();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PauseExecution()
        {
            if (!IsExecutionPaused)
            {
                if (m_engine.PauseExecution())
                {
#if NO_THREAD_CREATED_EVENTS
                    UpdateThreadList();
#endif
                    IsExecutionPaused = true;
                }
                else
                {
                    DebugAssert(m_fTerminating);
                }

                m_eventExecutionPaused.Set();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ResumeExecution(bool fForce)
        {
            if( (fForce || IsExecutionPaused) && this.IsDebugging)
            {
                foreach (CorDebugThread thread in m_threads)
                {
                    thread.ResumingExecution();
                }

                if (m_cEvalThreads == 0)
                {
                    ScratchPad.Clear();
                }

                UpdateBreakpoints();
                Engine.ResumeExecution();

                SetCurrentAppDomain( null );

                IsExecutionPaused = false;
            }
        }

        private void ResumeExecution()
        {
            ResumeExecution(false);
        }

        public CorDebugAssembly AssemblyFromIdx(uint idx)
        {
            return CorDebugAssembly.AssemblyFromIdx( idx, m_assemblies );
        }

        public CorDebugAssembly AssemblyFromIndex(uint index)
        {
            return CorDebugAssembly.AssemblyFromIndex( index, m_assemblies );
        }

        public void RegisterBreakpoint(object o, bool fRegister)
        {
            if (fRegister)
            {
                m_breakpoints.Add(o);
            }
            else
            {
                m_breakpoints.Remove(o);
            }

            DirtyBreakpoints();
        }

        internal ArrayList GetBreakpoints( Type t, CorDebugAppDomain appDomain )
        {
            ArrayList al = new ArrayList( m_breakpoints.Count );

            foreach(CorDebugBreakpointBase breakpoint in m_breakpoints)
            {
                if(t.IsAssignableFrom( breakpoint.GetType() ))
                {
                    if(appDomain == null || appDomain == breakpoint.AppDomain)
                    {
                        al.Add( breakpoint );
                    }
                }
            }

            return al;
        }

        private CorDebugBreakpointBase[] FindBreakpoints( BreakpointDef breakpointDef )
        {
            //perhaps need to cheat for CorDebugEval.Breakpoint for uncaught exceptions...
            ArrayList breakpoints = new ArrayList( 1 );
            if( IsDebugging )
            {
                foreach( CorDebugBreakpointBase breakpoint in m_breakpoints )
                {
                    if( breakpoint.IsMatch( breakpointDef ) )
                    {
                        breakpoints.Add( breakpoint );
                    }
                }
            }

            return (CorDebugBreakpointBase[])breakpoints.ToArray( typeof( CorDebugBreakpointBase ) );
        }

        private bool BreakpointHit(BreakpointDef breakpointDef)
        {
#if NO_THREAD_CREATED_EVENTS
            UpdateThreadList();
#endif
            CorDebugBreakpointBase[] breakpoints = FindBreakpoints(breakpointDef);
            bool fStopExecution = false;

            for(int iBreakpoint = 0; iBreakpoint < breakpoints.Length; iBreakpoint++)
            {
                CorDebugBreakpointBase breakpoint = breakpoints[iBreakpoint];

                if(breakpoint.ShouldBreak(breakpointDef))
                {
                    fStopExecution = true;
                    break;
                }
            }

            if(fStopExecution)
            {
                for(int iBreakpoint = 0; iBreakpoint < breakpoints.Length; iBreakpoint++)
                {
                    CorDebugBreakpointBase breakpoint = breakpoints[iBreakpoint];
                    breakpoint.Hit( breakpointDef );
                }
            }

            return fStopExecution;
        }

        public void UpdateBreakpoints()
        {
            if(!IsAttachedToEngine || !m_fUpdateBreakpoints || ShuttingDown)
                return;

            //Function breakpoints are set for each AppDomain.
            //No need to send all duplicates to the TinyCLR
            ArrayList al = new ArrayList( m_breakpoints.Count );
            for(int i = 0; i < m_breakpoints.Count; i++)
            {
                CorDebugBreakpointBase breakpoint1 = ((CorDebugBreakpointBase)m_breakpoints[i]);

                bool fDuplicate = false;

                for(int j = 0; j < i; j++)
                {
                    CorDebugBreakpointBase breakpoint2 = ((CorDebugBreakpointBase)m_breakpoints[j]);

                    if(breakpoint1.Equals( breakpoint2 ))
                    {
                        fDuplicate = true;
                        break;
                    }
                }

                if(!fDuplicate)
                {
                    al.Add( breakpoint1.Debugging_Execution_BreakpointDef );
                }
            }

            BreakpointDef[] breakpointDefs = (BreakpointDef[])al.ToArray( typeof( BreakpointDef ) );

            Engine.SetBreakpoints( breakpointDefs );
            m_fUpdateBreakpoints = false;
        }

        private void StoreAssemblyPaths(string[] args)
        {
            DebugAssert(m_assemblyPaths == null);

            ArrayList al = new ArrayList(args.Length);

            for (int iArg = 0; iArg < args.Length; iArg++)
            {
                string arg = args[iArg];
                if (arg == "-load")
                {
                    al.Add(args[++iArg]);
                }
                else if (arg.StartsWith("/load:"))
                {
                    al.Add(arg.Substring("/load:".Length));
                }
            }

            this.AssemblyPaths = (string[])al.ToArray(typeof(string));
        }

        public string[] AssemblyPaths
        {
            get { return m_assemblyPaths; }
            set
            {
                if (m_assemblyPaths != null)
                    throw new ArgumentException("already stored assembly paths");

                m_assemblyPaths = value;
            }
        }

        /*
            DummyThreadStart is an incredibly big hack.  It is exclusively a workaround for a cdpe shortcoming.  cpde requires
            a valid Win32 thread handle returned from CreateProcess.  DummyThread is created for that reason.  Also note
            that cpde is not expecting to receive events until ResumeThread is called.  So DummyThread gets suspended,
            and when it gets resumed by cpde, it then can start sending events.
        */
        private void DummyThreadStart(IntPtr ptr)
        {
            m_dummyThreadEvent.Set();
        }

        private void CreateDummyThread(out IntPtr threadHandle, out uint threadId)
        {
            m_dummyThreadDelegate = new Utility.Kernel32.CreateThreadCallback(DummyThreadStart);
            m_dummyThreadEvent = new AutoResetEvent(false);

            threadHandle = Utility.Kernel32.CreateThread(IntPtr.Zero, 0, m_dummyThreadDelegate, IntPtr.Zero, Utility.Kernel32.CREATE_SUSPENDED, out threadId);
        }

        private void WaitDummyThread()
        {
            if (m_dummyThreadEvent == null)
                throw new Exception("The device debuggee proxy thread is not initialized");
            m_dummyThreadEvent.WaitOne();
            m_dummyThreadEvent = null;
            m_dummyThreadDelegate = null;
        }

        private void CreateEmulatorProcess(
            DebugPort   port,
            string      lpApplicationName,
            string      lpCommandLine,
            IntPtr      lpProcessAttributes,
            IntPtr      lpThreadAttributes,
            int         bInheritHandles,
            uint        dwCreationFlags,
            System.IntPtr lpEnvironment,
            string      lpCurrentDirectory,
            ref _STARTUPINFO lpStartupInfo,
            ref _PROCESS_INFORMATION lpProcessInformation,
            uint        debuggingFlags
            )
        {
            VsPackage.MessageCentre.DeploymentMsg(String.Format(DiagnosticStrings.EmulatorCommandLine, lpCommandLine));

            try
            {
                Process emuProcess = new Process();
                emuProcess.StartInfo.FileName = lpApplicationName;
                emuProcess.StartInfo.Arguments = lpCommandLine.Substring(lpApplicationName.Length+2);
                emuProcess.StartInfo.UseShellExecute = false;

                emuProcess.StartInfo.RedirectStandardOutput = true;
                emuProcess.StartInfo.RedirectStandardError = true;
                emuProcess.StartInfo.RedirectStandardInput = false;

                // Set our event handler to asynchronously read the emulator's outputs.
                emuProcess.OutputDataReceived += new DataReceivedEventHandler(VsPackage.MessageCentre.OutputMsgHandler);
                emuProcess.ErrorDataReceived += new DataReceivedEventHandler(VsPackage.MessageCentre.ErrorMsgHandler);

                emuProcess.StartInfo.WorkingDirectory = lpCurrentDirectory;

                // Start the process.
                if(!emuProcess.Start())
                    throw new Exception("Process.Start() returned false.");

                // Start the asynchronous reads of the emulator's output streams
                emuProcess.BeginOutputReadLine();
                emuProcess.BeginErrorReadLine();

                this.PortDefinition = new PortDefinition_Emulator("Emulator", emuProcess.Id);
                this.SetPid((uint)emuProcess.Id);

                port.AddProcess(this);

                const int DUPLICATE_SAME_ACCESS = 0x00000002;
                Utility.Kernel32.DuplicateHandle(Utility.Kernel32.GetCurrentProcess(), emuProcess.Handle,
                                                 Utility.Kernel32.GetCurrentProcess(), out lpProcessInformation.hProcess,
                                                 0, false, DUPLICATE_SAME_ACCESS);
                
                lpProcessInformation.dwProcessId = (uint)emuProcess.Id;
                CreateDummyThread(out lpProcessInformation.hThread, out lpProcessInformation.dwThreadId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Could not create emulator process."),ex);
            }
        }

        private void CreateDeviceProcess(DebugPort port, string lpApplicationName, string lpCommandLine, System.IntPtr lpProcessAttributes, System.IntPtr lpThreadAttributes, int bInheritHandles, uint dwCreationFlags, System.IntPtr lpEnvironment, string lpCurrentDirectory, ref _STARTUPINFO lpStartupInfo, ref _PROCESS_INFORMATION lpProcessInformation, uint debuggingFlags)
        {
            bool fDidDeploy = true;     //What if we did a launch but no deploy to a device...

            if (!fDidDeploy)
            {
                this.Engine.RebootDevice(Engine.RebootOption.RebootClrWaitForDebugger);
            }

            DebugAssert(m_port == port);

            lpProcessInformation.dwProcessId = m_pid;

            CreateDummyThread(out lpProcessInformation.hThread, out lpProcessInformation.dwThreadId);
        }

        private void InternalCreateProcess(
            DebugPort   port,
            string      lpApplicationName,
            string      lpCommandLine,
            IntPtr      lpProcessAttributes,
            IntPtr      lpThreadAttributes,
            int         bInheritHandles,
            uint        dwCreationFlags,
            System.IntPtr lpEnvironment,
            string      lpCurrentDirectory,
            ref _STARTUPINFO lpStartupInfo,
            ref _PROCESS_INFORMATION lpProcessInformation,
            uint        debuggingFlags
            )
        {
            if (port.IsLocalPort)
                this.CreateEmulatorProcess(port, lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, ref lpStartupInfo, ref lpProcessInformation, debuggingFlags);
            else
                this.CreateDeviceProcess(port, lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, ref lpStartupInfo, ref lpProcessInformation, debuggingFlags);
        }

        public static CorDebugProcess CreateProcessEx(IDebugPort2 pPort, string lpApplicationName, string lpCommandLine, System.IntPtr lpProcessAttributes, System.IntPtr lpThreadAttributes, int bInheritHandles, uint dwCreationFlags, System.IntPtr lpEnvironment, string lpCurrentDirectory, ref _STARTUPINFO lpStartupInfo, ref _PROCESS_INFORMATION lpProcessInformation, uint debuggingFlags)
        {
            DebugPort port = pPort as DebugPort;
            if (port == null)
                throw new Exception("IDebugPort2 object passed to Micro Framework package by Visual Studio is not a valid device port");

            CommandLineBuilder cb = new CommandLineBuilder(lpCommandLine);
            string[] args = cb.Arguments;
            string deployDeviceName = args[args.Length-1];

            //Extract deployDeviceName
            if (!deployDeviceName.StartsWith(CorDebugProcess.c_DeployDeviceName))
                throw new Exception(String.Format("\"{0}\" does not appear to be a valid Micro Framework device name", CorDebugProcess.c_DeployDeviceName));

            deployDeviceName = deployDeviceName.Substring(CorDebugProcess.c_DeployDeviceName.Length);
            cb.RemoveArguments(args.Length - 1, 1);

            lpCommandLine = cb.ToString();

            CorDebugProcess process = port.IsLocalPort
                ? new CorDebugProcess(port, null)
                : port.GetDeviceProcess(deployDeviceName, 60);

            if (process == null)
                throw new Exception("CorDebugProcess.CreateProcessEx() could not create or find the device process");

            process.StoreAssemblyPaths(args);
            process.InternalCreateProcess(port, lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, ref lpStartupInfo, ref lpProcessInformation, debuggingFlags);

            return process;
        }

        internal ulong FakeLoadAssemblyIntoMemory( CorDebugAssembly assembly )
        {
            ulong address = m_fakeAssemblyAddressNext;

            uint size;

            assembly.ICorDebugModule.GetSize(out size);

            m_fakeAssemblyAddressNext += size;

            return address;
        }

        private void LoadAssemblies()
        {
            VsPackage.MessageCentre.DebugMsg(DiagnosticStrings.LoadAssemblies);

            WireProtocol.Commands.Debugging_Resolve_Assembly[] assemblies = Engine.ResolveAllAssemblies();
            string[] assemblyPathsT = new string[1];
            Pdbx.PdbxFile.Resolver resolver = new Pdbx.PdbxFile.Resolver();

            if (!m_fLaunched || !this.IsLocalWin32Process)
            {
                //Find mscorlib
                WireProtocol.Commands.Debugging_Resolve_Assembly a = null;
                WireProtocol.Commands.Debugging_Resolve_Assembly.Reply reply = null;

                for (int i = 0; i < assemblies.Length; i++)
                {
                    a = assemblies[i];
                    reply = a.m_reply;

                    if (reply.Name == "mscorlib")
                        break;
                }

                DebugAssert(reply.Name == "mscorlib");

                //Add assemblyDirectories
                string asyVersion = "v4.3";
                if(AssemblyPaths != null)
                {
                    Regex versionExpr = new Regex(@"\\(v[\d]+\.[\d]+)\\");
                    for(int i=0; i<AssemblyPaths.Length; i++)
                    {
                        Match m = versionExpr.Match(AssemblyPaths[i]);

                        if (m.Success)
                        {
                            asyVersion = m.Groups[1].Value;
                            break;
                        }
                    }
                }

                PlatformInfo platformInfo = new PlatformInfo(asyVersion); // by not spcifying any runtime information, we will look for the most suitable version
                resolver.AssemblyDirectories = platformInfo.AssemblyFolders;
            }

            for(int i = 0; i < assemblies.Length; i++)
            {
                WireProtocol.Commands.Debugging_Resolve_Assembly a = assemblies[i];

                CorDebugAssembly assembly = this.AssemblyFromIdx( a.m_idx );

                if(assembly == null)
                {
                    WireProtocol.Commands.Debugging_Resolve_Assembly.Reply reply = a.m_reply;

                    if (!string.IsNullOrEmpty(reply.Path))
                    {
                        assemblyPathsT[0] = reply.Path;

                        resolver.AssemblyPaths = assemblyPathsT;
                    }
                    else
                    {
                        resolver.AssemblyPaths = m_assemblyPaths;
                    }

                    Pdbx.PdbxFile pdbxFile = resolver.Resolve(reply.Name, reply.m_version, Engine.IsTargetBigEndian); //Pdbx.PdbxFile.Open(reply.Name, reply.m_version, assemblyPaths);

                    assembly = new CorDebugAssembly(this, reply.Name, pdbxFile, TinyCLR_TypeSystem.IdxAssemblyFromIndex(a.m_idx));

                    m_assemblies.Add( assembly );
                }
            }
        }

        public CorDebugAppDomain GetAppDomainFromId( uint id )
        {
            foreach(CorDebugAppDomain appDomain in m_appDomains)
            {
                if(appDomain.Id == id)
                {
                    return appDomain;
                }
            }

            return null;
        }

        private void CreateAppDomainFromId(uint id)
        {
            CorDebugAppDomain appDomain = new CorDebugAppDomain(this, id);

            m_appDomains.Add(appDomain);
            EnqueueEvent(new ManagedCallbacks.ManagedCallbackAppDomain(appDomain, ManagedCallbacks.ManagedCallbackAppDomain.EventType.CreateAppDomain));
        }

        private void RemoveAppDomain(CorDebugAppDomain appDomain)
        {
            EnqueueEvent(new ManagedCallbacks.ManagedCallbackAppDomain(appDomain, ManagedCallbacks.ManagedCallbackAppDomain.EventType.ExitAppDomain));
            m_appDomains.Remove(appDomain);
        }

        public void UpdateAssemblies()
        {
            VsPackage.MessageCentre.InternalErrorMsg(DiagnosticStrings.LoadedAssembliesInformation);
            lock(m_appDomains)
            {
                LoadAssemblies();

                uint[] appDomains = new uint[] { CorDebugAppDomain.c_AppDomainId_ForNoAppDomainSupport };

                if(Engine.Capabilities.AppDomains)
                {
                    appDomains = Engine.GetAppDomains().m_data;
                }

                //Search for new appDomains
                foreach(uint id in appDomains)
                {
                    CorDebugAppDomain appDomain = GetAppDomainFromId( id );

                    if(appDomain == null)
                    {
                        CreateAppDomainFromId(id);
                    }
                }

                for(int iAppDomain = 0; iAppDomain < m_appDomains.Count; iAppDomain++)
                {
                    CorDebugAppDomain appDomain = (CorDebugAppDomain)m_appDomains[iAppDomain];
                    appDomain.UpdateAssemblies();
                }

                //Search for dead appDomains
                for(int iAppDomain = m_appDomains.Count - 1; iAppDomain >= 0; iAppDomain--)
                {
                    CorDebugAppDomain appDomain = (CorDebugAppDomain)m_appDomains[iAppDomain];

                    if(Array.IndexOf( appDomains, appDomain.Id ) < 0)
                    {
                        RemoveAppDomain(appDomain);
                    }
                }
            }
        }

        private void OnDetach()
        {
#if DEBUG
            //cpde should remove all breakpoints (except for our one internal breakpoint)
            DebugAssert(m_breakpoints.Count == 1);
            DebugAssert(m_breakpoints[0] is CLREventsBreakpoint);
            DebugAssert(m_cEvalThreads == 0);

            foreach (CorDebugThread thread in m_threads)
            {
                //cpde should abort all func-eval
                DebugAssert(!thread.IsVirtualThread);
                //cpde should resume all threads
                DebugAssert(!thread.IsSuspended);
            }
#endif

            //We need to kill the
            m_breakpoints.Clear();
            DirtyBreakpoints();
            ResumeExecution();

            uint iCurrent;

            m_engine.SetExecutionMode(0, WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_SourceLevelDebugging, out iCurrent);

            DebugAssert((iCurrent & WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_PauseTimers  ) == 0);
            DebugAssert((iCurrent & WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_NoCompaction ) == 0);
            DebugAssert((iCurrent & WireProtocol.Commands.Debugging_Execution_ChangeConditions.c_Stopped      ) == 0);
        }

        private void OnTerminate()
        {
            // only kill the win32 process, leave the device alone in the debugger loop 
            if (this.IsLocalWin32Process)
            {
                DebugAssert(m_fLaunched);

                Process process = m_win32process;

                if (process != null && !process.HasExited)
                {
                    process.CloseMainWindow();
                }
            }
        }

        private void StopDebugging()
        {
            DebugAssert(ShuttingDown);

            /*
             * this is called when debugging stops, either via terminate or detach.
             */

            try
            {
                if (m_engine != null) //perhaps we never attached.
                {
                    lock(this)
                    {
                        m_engine.ThrowOnCommunicationFailure = false;

                        if (IsDebugging)
                        {
                            //Should terminating a device simply be a detach?
                            if (m_fDetaching || !this.IsLocalWin32Process)
                            {
                                OnDetach();
                            }
                            else
                            {
                                OnTerminate();
                            }
                        }

                        DetachFromEngine();
                    }
                }
            }
            catch (Exception ex)
            {
                VsPackage.MessageCentre.InternalErrorMsg(false, "Exception while terminating CorDebugProcess: " + ex.Message);
            }
            finally
            {
                UnInit();
            }
        }

        public void OnMessage(WireProtocol.IncomingMessage msg, string text)
        {
            if (m_threads != null && m_threads.Count > 0)
            {
                if(m_appDomains != null && m_appDomains.Count > 0)
                {
                  EnqueueEvent( new ManagedCallbacks.ManagedCallbackDebugMessage( (CorDebugThread)m_threads[0], (CorDebugAppDomain)m_appDomains[0], "TinyCLR_Message", text, LoggingLevelEnum.LStatusLevel0 ) );
                }
            }
            else
            {
                VsPackage.MessageCentre.DebugMsg(text);
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DrainBreakpoints()
        {
            bool fStopExecution = false;
            bool fAnyBreakpoints = false;
            bool fLastBreakpoint = true;
            BreakpointDef bp;

            if (Engine == null) return;

            do
            {
                bp = Engine.GetBreakpointStatus();

                if(bp == null || bp.m_flags == 0)
                {
                    break;
                }

                fLastBreakpoint = (bp.m_flags & BreakpointDef.c_LAST_BREAKPOINT) != 0;

                //clear c_LAST_BREAKPOINT flags because so derived breakpoint classes don't need to care
                bp.m_flags = (ushort)(bp.m_flags & ~BreakpointDef.c_LAST_BREAKPOINT);

                bool fStopExecutionT = BreakpointHit( bp );

                fStopExecution = fStopExecution || fStopExecutionT;
                fAnyBreakpoints = true;
            } while (!fLastBreakpoint);

            if (fAnyBreakpoints)
            {
                if (!fStopExecution)
                {
                    DebugAssert(!this.IsExecutionPaused);
                    //Force execution to resume, even though IsExecutionPaused state is not set
                    //hitting a breakpoint requires the TinyCLR to be stopped.  Setting IsExecutionPaused = true
                    //just to force resumeExecution to succeed is wrong, and will result in a race condition
                    //where Stop is waiting for a synchronous stop, will succeed while execution is about to be resumed.
                    ResumeExecution(true);
                }
                else
                {
                    IsExecutionPaused = true;
                }
            }
        }

        public void SuspendCommands(bool fSuspend)
        {
            if (fSuspend)
            {
                bool lockTaken = false;
                Monitor.Enter(this, ref lockTaken);
                if (lockTaken)
                {
                    Interlocked.Increment(ref m_cStopped); //also don't dispatch events
                }
            }
            else
            {
                Interlocked.Decrement(ref m_cStopped);
                m_eventDispatch.Set();                 //just in case there events needing dispatch.
                Monitor.Exit(this);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnCommand(WireProtocol.IncomingMessage msg, bool fReply)
        {
            switch(msg.Header.m_cmd)
            {
                case WireProtocol.Commands.c_Debugging_Execution_BreakpointHit:
                    DrainBreakpoints();
                    break;
                case WireProtocol.Commands.c_Monitor_ProgramExit:
                    this.ICorDebugProcess.Terminate(0);
                    break;
                case WireProtocol.Commands.c_Monitor_Ping:
                case WireProtocol.Commands.c_Debugging_Button_Report:
                case WireProtocol.Commands.c_Debugging_Lcd_NewFrame:
                case WireProtocol.Commands.c_Debugging_Lcd_NewFrameData:
                case WireProtocol.Commands.c_Debugging_Value_GetStack:
                    //nop
                    break;
                default:
                    VsPackage.MessageCentre.InternalErrorMsg(false, "Unexpected command=" + msg.Header.m_cmd );
                    break;
            }
        }

        public void OnNoise(byte[] buf, int offset, int count)
        {
            if(buf != null && (offset + count) <= buf.Length)
            {
                VsPackage.MessageCentre.InternalErrorMsg( System.Text.UTF8Encoding.UTF8.GetString(buf, offset, count) );
            }
        }

        private ArrayList GetAllNonVirtualThreads()
        {
            ArrayList al = new ArrayList(m_threads.Count);

            foreach (CorDebugThread thread in m_threads)
            {
                if (!thread.IsVirtualThread)
                    al.Add(thread);
            }

            return al;
        }

        public class BuiltinType
        {
            private CorDebugAssembly m_assembly;
            private uint m_tkCLR;
            private CorDebugClass m_class;

            public BuiltinType( CorDebugAssembly assembly, uint tkCLR, CorDebugClass cls )
            {
                this.m_assembly = assembly;
                this.m_tkCLR = tkCLR;
                this.m_class = cls;
            }

            public CorDebugAssembly GetAssembly( CorDebugAppDomain appDomain )
            {
                return appDomain.AssemblyFromIdx( m_assembly.Idx );
            }

            public CorDebugClass GetClass( CorDebugAppDomain appDomain )
            {
                CorDebugAssembly assembly = GetAssembly( appDomain );

                return assembly.GetClassFromTokenCLR( this.m_tkCLR );
            }

            public uint TokenCLR
            {
                get { return m_tkCLR; }
            }
        }

        private void AddBuiltInType(object o, CorDebugAssembly assm, string type)
        {
            uint tkCLR = MetaData.Helper.ClassTokenFromName(assm.MetaDataImport, type);
            CorDebugClass c = assm.GetClassFromTokenCLR(tkCLR);

            BuiltinType builtInType = new BuiltinType( assm, tkCLR, c );

            m_tdBuiltin[o] = builtInType;
        }

        public BuiltinType ResolveBuiltInType(object o)
        {
            DebugAssert(o is CorElementType || o is ReflectionDefinition.Kind || o is Debugger.RuntimeDataType);

            CorDebugAssembly assmCorLib = null;

            if (m_tdBuiltin == null)
            {
                m_tdBuiltin = new Hashtable();

                foreach (CorDebugAssembly assm in m_assemblies)
                {
                    if (assm.Name == "mscorlib")
                    {
                        assmCorLib = assm;
                        break;
                    }
                }

                DebugAssert(assmCorLib != null);

                AddBuiltInType(CorElementType.ELEMENT_TYPE_BOOLEAN, assmCorLib, "System.Boolean");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_CHAR,    assmCorLib, "System.Char");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_I1, assmCorLib, "System.SByte");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_U1, assmCorLib, "System.Byte");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_I2, assmCorLib, "System.Int16");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_U2, assmCorLib, "System.UInt16");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_I4, assmCorLib, "System.Int32");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_U4, assmCorLib, "System.UInt32");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_I8, assmCorLib, "System.Int64");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_U8, assmCorLib, "System.UInt64");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_R4, assmCorLib, "System.Single");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_R8, assmCorLib, "System.Double");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_CLASS, assmCorLib, "System.Object"); //???
                AddBuiltInType(CorElementType.ELEMENT_TYPE_OBJECT, assmCorLib, "System.Object");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_SZARRAY, assmCorLib, "System.Array");
                AddBuiltInType(CorElementType.ELEMENT_TYPE_ARRAY, assmCorLib, "System.Array");

                AddBuiltInType(Microsoft.SPOT.Debugger.ReflectionDefinition.Kind.REFLECTION_TYPE, assmCorLib, "System.RuntimeType");
                AddBuiltInType(Microsoft.SPOT.Debugger.ReflectionDefinition.Kind.REFLECTION_TYPE_DELAYED, assmCorLib, "System.RuntimeType");
                AddBuiltInType(Microsoft.SPOT.Debugger.ReflectionDefinition.Kind.REFLECTION_ASSEMBLY, assmCorLib, "System.Reflection.Assembly");
                AddBuiltInType(Microsoft.SPOT.Debugger.ReflectionDefinition.Kind.REFLECTION_FIELD, assmCorLib, "System.Reflection.RuntimeFieldInfo");
                AddBuiltInType(Microsoft.SPOT.Debugger.ReflectionDefinition.Kind.REFLECTION_METHOD, assmCorLib, "System.Reflection.RuntimeMethodInfo");
                AddBuiltInType(Microsoft.SPOT.Debugger.ReflectionDefinition.Kind.REFLECTION_CONSTRUCTOR, assmCorLib, "System.Reflection.RuntimeConstructorInfo");

                AddBuiltInType( Microsoft.SPOT.Debugger.RuntimeDataType.DATATYPE_TRANSPARENT_PROXY, assmCorLib, "System.Runtime.Remoting.Proxies.__TransparentProxy" );
            }

            return (BuiltinType)m_tdBuiltin[o];
        }

        private bool AnyQueuedEvents
        {
            get { return m_events.Count > 0; }
        }

        private ICorDebugProcess ICorDebugProcess
        {
            get {return (ICorDebugProcess)this;}
        }

        private ICorDebugController ICorDebugController
        {
            get { return (ICorDebugController)this; }
        }

        #region ICorDebugController Members

        int ICorDebugController.Stop(uint dwTimeout)
        {
            Interlocked.Increment(ref m_cStopped);
            m_eventDispatch.Set();

            DebugAssert(IsExecutionPaused || Thread.CurrentThread != m_threadDispatch );
            EventWaitHandle.WaitAny(m_eventsStopped);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.Continue(int fIsOutOfBand)
        {
            Interlocked.Decrement(ref m_cStopped);
            m_eventDispatch.Set();

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.IsRunning(out int pbRunning)
        {
            pbRunning = Utility.Boolean.BoolToInt(!IsExecutionPaused);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.HasQueuedCallbacks(ICorDebugThread pThread, out int pbQueued)
        {
            CorDebugThread thread = pThread as CorDebugThread;
            bool fQueued = false;

            if (thread == null || !thread.SuspendThreadEvents)
            {
                lock (m_events.SyncRoot)
                {
                    foreach (ManagedCallbacks.ManagedCallback mc in m_events)
                    {
                        ManagedCallbacks.ManagedCallbackThread mct = mc as ManagedCallbacks.ManagedCallbackThread;

                        if (thread == null)
                        {
                            //any non-thread events? should
                            //any thread events that aren't visible to cpde
                            fQueued = (mct == null || !mct.Thread.SuspendThreadEvents);
                        }
                        else
                        {
                            //only visible, thread events, matching the requested thread.
                            fQueued = (mct != null && mct.Thread == thread && !mct.Thread.SuspendThreadEvents);
                        }

                        if (fQueued)
                        {
                            break;
                        }
                    }
                }
            }

            pbQueued = Utility.Boolean.BoolToInt(fQueued);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.EnumerateThreads(out ICorDebugThreadEnum ppThreads)
        {
            ppThreads = new CorDebugEnum(GetAllNonVirtualThreads(), typeof(ICorDebugThread), typeof(ICorDebugThreadEnum));

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.SetAllThreadsDebugState(CorDebugThreadState state, ICorDebugThread pExceptThisThread)
        {
            // could/should really make this one call to engine...or else send them all at once..
            CorDebugThread threadExcept = (CorDebugThread)pExceptThisThread;

            foreach (CorDebugThread thread in GetAllNonVirtualThreads())
            {
                if (thread != threadExcept)
                {
                    DebugAssert(!thread.GetLastCorDebugThread().IsVirtualThread);
                    ((ICorDebugThread)thread).SetDebugState(state);
                }
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.Detach()
        {
            lock (m_syncTerminatingObject)
            {
                if (!ShuttingDown)
                {
                    m_fDetaching = true;
                    m_eventDispatch.Set();
                }
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.Terminate(uint exitCode)
        {
            lock (m_syncTerminatingObject)
            {
                if (!ShuttingDown)
                {
                    m_fTerminating = true;

                    if (IsDebugging)
                    {
                        m_eventDispatch.Set();
                    }
                    else
                    {
                        StopDebugging();
                    }
                }
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.CanCommitChanges(uint cSnapshots, ref ICorDebugEditAndContinueSnapshot pSnapshots, out ICorDebugErrorInfoEnum pError)
        {
            // CorDebugProcess.CanCommitChanges is not implemented
            pError = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugController.CommitChanges(uint cSnapshots, ref ICorDebugEditAndContinueSnapshot pSnapshots, out ICorDebugErrorInfoEnum pError)
        {
            // CorDebugProcess.CommitChanges is not implemented
            pError = null;

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region ICorDebugProcess Members

        int ICorDebugProcess.Stop(uint dwTimeout)
        {
            return this.ICorDebugController.Stop(dwTimeout);
        }

        int ICorDebugProcess.Continue(int fIsOutOfBand)
        {
            return this.ICorDebugController.Continue(fIsOutOfBand);
        }

        int ICorDebugProcess.IsRunning(out int pbRunning)
        {
            return this.ICorDebugController.IsRunning(out pbRunning);
        }

        int ICorDebugProcess.HasQueuedCallbacks(ICorDebugThread pThread, out int pbQueued)
        {
            return this.ICorDebugController.HasQueuedCallbacks(pThread, out pbQueued);
        }

        int ICorDebugProcess.EnumerateThreads(out ICorDebugThreadEnum ppThreads)
        {
            return this.ICorDebugController.EnumerateThreads(out ppThreads);
        }

        int ICorDebugProcess.SetAllThreadsDebugState(CorDebugThreadState state, ICorDebugThread pExceptThisThread)
        {
            return this.ICorDebugController.SetAllThreadsDebugState(state, pExceptThisThread);
        }

        int ICorDebugProcess.Detach()
        {
            return this.ICorDebugController.Detach();
        }

        int ICorDebugProcess.Terminate(uint exitCode)
        {
            return this.ICorDebugController.Terminate(exitCode);
        }

        int ICorDebugProcess.CanCommitChanges(uint cSnapshots, ref ICorDebugEditAndContinueSnapshot pSnapshots, out ICorDebugErrorInfoEnum pError)
        {
            return this.ICorDebugController.CanCommitChanges(cSnapshots, ref pSnapshots, out pError);
        }

        int ICorDebugProcess.CommitChanges(uint cSnapshots, ref ICorDebugEditAndContinueSnapshot pSnapshots, out ICorDebugErrorInfoEnum pError)
        {
            return this.ICorDebugController.CommitChanges(cSnapshots, ref pSnapshots, out pError);
        }

        int ICorDebugProcess.GetID(out uint pdwProcessId)
        {
            pdwProcessId = m_pid;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.GetHandle(out uint phProcessHandle)
        {
            // CorDebugProcess.GetHandle is not implemented
            phProcessHandle = 0;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.GetThread(uint dwThreadId, out ICorDebugThread ppThread)
        {
            ppThread = GetThread(dwThreadId);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.EnumerateObjects(out ICorDebugObjectEnum ppObjects)
        {
            // CorDebugProcess.EnumerateObjects is not implemented
            ppObjects = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.IsTransitionStub(ulong address, out int pbTransitionStub)
        {
            pbTransitionStub = Utility.Boolean.FALSE;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.IsOSSuspended(uint threadID, out int pbSuspended)
        {
            // CorDebugProcess.IsOSSuspended is not implemented
            pbSuspended = Utility.Boolean.FALSE;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.GetThreadContext(uint threadID, uint contextSize, IntPtr context)
        {
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugProcess.SetThreadContext(uint threadID, uint contextSize, IntPtr context)
        {
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugProcess.ReadMemory(ulong address, uint size, byte[] buffer, out uint read)
        {
            read = 0;
            byte[] bufRead;

            if (address + size == 0x100000000)
            {
            }
            else if (address >= c_fakeAddressStart)
            {
                //find the assembly loaded at this addres (all fake, of course)
                for (int iAssembly = 0; iAssembly < m_assemblies.Count; iAssembly++)
                {
                    CorDebugAssembly assembly = (CorDebugAssembly)m_assemblies[iAssembly];

                    ulong baseAddress;
                    uint assemblySize;
                    assembly.ICorDebugModule.GetBaseAddress(out baseAddress);
                    assembly.ICorDebugModule.GetSize(out assemblySize);

                    if (address >= baseAddress && address < baseAddress + assemblySize)
                    {
                        DebugAssert(address + size <= baseAddress + assemblySize);

                        assembly.ReadMemory(address - baseAddress, size, buffer, out read);
                    }
                }
            }
            else
            {
                if (this.Engine.ReadMemory((uint)address, size, out bufRead))
                {
                    DebugAssert(bufRead.Length == size);

                    bufRead.CopyTo(buffer, 0);
                    read = size;
                }
            }

            return Utility.COM_HResults.BOOL_TO_HRESULT_FAIL(read > 0);
        }

        int ICorDebugProcess.WriteMemory(ulong address, uint size, byte[] buffer, out uint written)
        {
            written = 0;

            if (address < c_fakeAddressStart)
            {
                if (this.Engine.WriteMemory((uint)address, buffer))
                {
                    written = size;
                }
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.ClearCurrentException(uint threadID)
        {
            CorDebugThread thread = GetThread(threadID);
            if (thread != null)
                ((ICorDebugThread)thread).ClearCurrentException();

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.EnableLogMessages(int fOnOff)
        {
            // CorDebugProcess.EnableLogMessages is not implemented
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.ModifyLogSwitch(string pLogSwitchName, int lLevel)
        {
            // Need to adjust Interop assembly CorDebugInterop, to enable this function
            // CorDebugProcess.ModifyLogSwitch is not implemented
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.EnumerateAppDomains(out ICorDebugAppDomainEnum ppAppDomains)
        {
            ppAppDomains = new CorDebugEnum(m_appDomains, typeof(ICorDebugAppDomain), typeof(ICorDebugAppDomainEnum));

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.GetObject(out ICorDebugValue ppObject)
        {
            // CorDebugProcess.GetObject is not implemented
            ppObject = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.ThreadForFiberCookie(uint fiberCookie, out ICorDebugThread ppThread)
        {
            // CorDebugProcess.ThreadForFiberCookie is not implemented
            ppThread = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess.GetHelperThreadID(out uint pThreadID)
        {
            // CorDebugProcess.GetHelperThreadID is not implemented
            pThreadID = 0;

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region ICorDebugProcess2 Members

        int ICorDebugProcess2.GetVersion( out _COR_VERSION version )
        {
            version = new _COR_VERSION();
            version.dwMajor = 1;    //This is needed to handle v1 exceptions.

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugProcess2.GetThreadForTaskID( ulong taskid, out ICorDebugThread2 ppThread )
        {
            ppThread = null;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugProcess2.SetUnmanagedBreakpoint( ulong address, uint bufsize, byte[] buffer, out uint bufLen )
        {
            bufLen = 0;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugProcess2.GetDesiredNGENCompilerFlags( out uint pdwFlags )
        {
            pdwFlags = 0;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugProcess2.SetDesiredNGENCompilerFlags( uint pdwFlags )
        {
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugProcess2.ClearUnmanagedBreakpoint( ulong address )
        {
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugProcess2.GetReferenceValueFromGCHandle(UIntPtr handle, out ICorDebugReferenceValue pOutValue)
        {
            pOutValue = null;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        #endregion

        #region IDebugProcess2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.Detach()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.CauseBreak()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.GetAttachedSessionName(out string pbstrSessionName)
        {
            pbstrSessionName = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.GetProcessId(out Guid pguidProcessId)
        {
            pguidProcessId = m_guidProcessId;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            // DebugProcess.EnumThreads is not implemented
            //If we need to implement this, must return a copy without any VirtualThreads
            ppEnum = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.Terminate()
        {
            return this.ICorDebugProcess.Terminate(0);
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.GetServer(out IDebugCoreServer2 ppServer)
        {
            ppServer = m_port.DebugPortSupplier.CoreServer;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            ArrayList appDomains = m_appDomains;

            if(!this.IsAttachedToEngine)
            {
                //need to fake this in order to get the Attach Dialog to work.
                DebugAssert( appDomains == null );
                appDomains = new ArrayList();
                appDomains.Add( new CorDebugAppDomain( this, 1 ) );
            }

            ppEnum = new CorDebugEnum( appDomains, typeof( IDebugProgram2 ), typeof( IEnumDebugPrograms2 ) );
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            pProcessId[0] = PhysicalProcessId;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.GetInfo(uint Fields, PROCESS_INFO[] pProcessInfo)
        {
            Process process = null;
            PROCESS_INFO pi = new PROCESS_INFO();
            pi.Fields = Fields;

            if (this.m_port == null)
            {
                return Utility.COM_HResults.E_FAIL;
            }

            if (this.m_port.IsLocalPort)
            {
                try
                {
                    process = Process.GetProcessById((int)this.m_pid);
                }
                catch // Emulator has exited!
                {
                    return Utility.COM_HResults.E_FAIL;
                }
            }

            if ((Fields & (uint) PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME) != 0)
            {
                pi.bstrAttachedSessionName = null;
            }

            if ((Fields & (uint) PROCESS_INFO_FIELDS.PIF_BASE_NAME) != 0)
            {
                //We aren't connected to the CLR yet.  This information (it appears in the Processes window)
                //is probably better than nothing.
                if (process != null)
                {
                    pi.bstrBaseName = process.ProcessName;
                }
                else
                {
                    pi.bstrBaseName = m_portDefinition.DisplayName;
                }
            }

            if ((Fields & (uint) PROCESS_INFO_FIELDS.PIF_CREATION_TIME) != 0)
            {
                pi.CreationTime.dwHighDateTime = 0;
                pi.CreationTime.dwLowDateTime = 0;
            }

            if ((Fields & (uint) PROCESS_INFO_FIELDS.PIF_FILE_NAME) != 0)
            {
                if (process != null)
                {
                    pi.bstrFileName = process.ProcessName;
                }
                else
                {
                    pi.bstrFileName = m_portDefinition.DisplayName;
                }
            }

            if ((Fields & (uint) PROCESS_INFO_FIELDS.PIF_FLAGS) != 0)
            {
                if(m_fExecutionPaused)
                    pi.Flags = (uint) PROCESS_INFO_FLAGS.PIFLAG_PROCESS_STOPPED;
                else
                    pi.Flags = (uint) PROCESS_INFO_FLAGS.PIFLAG_PROCESS_RUNNING;
            }

            if ((Fields & (uint) PROCESS_INFO_FIELDS.PIF_PROCESS_ID) != 0)
            {
                pi.ProcessId = PhysicalProcessId;
            }

            if ((Fields & (uint) PROCESS_INFO_FIELDS.PIF_SESSION_ID) != 0)
            {
                //Can/should we enable debugging across sessions?  Can we access other user's
                //debug pipe?
                pi.dwSessionId = 0;
            }

            if ((Fields & (uint) PROCESS_INFO_FIELDS.PIF_TITLE) != 0)
            {
                if (process != null)
                {
                    pi.bstrTitle = process.MainWindowTitle;
                }
                else
                {
                    pi.bstrTitle = "<Unknown>";
                }
            }

            if (process != null)
            {
                process.Dispose();
                process = null;
            }

            pProcessInfo[0] = pi;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.CanDetach()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.GetPort(out IDebugPort2 ppPort)
        {
            ppPort = m_port;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcess2.GetName(uint gnType, out string pbstrName)
        {
            // DebugProcess.GetName is not implemented
            pbstrName = "Micro Framework application";
            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IDebugProcessEx2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcessEx2.Detach(IDebugSession2 pSession)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcessEx2.Attach(IDebugSession2 pSession)
        {
            int hr = Utility.COM_HResults.S_OK;

            //check if the process is still alive.  For emulator, if the process is still alive.
            if (!m_port.ContainsProcess(this))
            {
                hr = Utility.COM_HResults.E_PROCESS_DESTROYED;
            }

            return hr;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProcessEx2.AddImplicitProgramNodes(ref Guid guidLaunchingEngine, Guid[] rgguidSpecificEngines, uint celtSpecificEngines)
        {
            return Utility.COM_HResults.S_OK;
        }

        #endregion


        private static void DebugAssert(bool condition, string message, string detailedMessage)
        {
            VsPackage.MessageCentre.InternalErrorMsg(condition, String.Format("message: {0}\r\nDetailed Message: {1}", message, detailedMessage));
        }

        private static void DebugAssert(bool condition, string message)
        {
            VsPackage.MessageCentre.InternalErrorMsg(condition, message);
        }

        private static void DebugAssert(bool condition)
        {
            DebugAssert(condition, null);
        }
    }
}
