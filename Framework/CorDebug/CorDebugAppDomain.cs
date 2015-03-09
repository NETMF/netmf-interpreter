using System;
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
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.SPOT.Debugger
{
    public class CorDebugAppDomain : ICorDebugAppDomain, 
                                     ICorDebugAppDomain2,
                                     IDebugProgram2,
                                     IDebugProgramEx2,
                                     IDebugProgramNode2,
                                     IDebugCOMPlusProgramNode2
    {
        public const uint c_AppDomainId_ForNoAppDomainSupport = 1;

        private readonly CorDebugProcess m_process;
        private readonly ArrayList m_assemblies;
        private readonly uint m_id;
        private string m_name;
        private readonly Guid m_guidProgramId;

        public CorDebugAppDomain( CorDebugProcess process, uint id )
        {
            m_process = process;
            m_id = id;
            m_assemblies = new ArrayList();
            m_guidProgramId = Guid.NewGuid();
        }

        public string Name
        {
            get { return m_name; }
        }

        public uint Id
        {
            get { return m_id; }
        }
                
        public CorDebugAssembly AssemblyFromIdx(uint idx)
        {
            return CorDebugAssembly.AssemblyFromIdx( idx, m_assemblies );
        }

        public CorDebugAssembly AssemblyFromIndex(uint index)
        {
            return CorDebugAssembly.AssemblyFromIndex( index, m_assemblies );            
        }

        public bool UpdateAssemblies()
        {
            uint[] assemblies = null;            
            List<ManagedCallbacks.ManagedCallback> callbacks = new System.Collections.Generic.List<ManagedCallbacks.ManagedCallback>();

            if(this.Process.Engine.Capabilities.AppDomains)
            {
                WireProtocol.Commands.Debugging_Resolve_AppDomain.Reply reply = this.Process.Engine.ResolveAppDomain( m_id );

                if(reply != null)
                {
                    m_name = reply.Name;
                    assemblies = reply.m_data;
                }

                if(assemblies == null)
                {
                    //assembly is already unloaded
                    assemblies = new uint[0];
                }
            }
            else
            {
                WireProtocol.Commands.Debugging_Resolve_Assembly[] reply = this.Process.Engine.ResolveAllAssemblies();
                assemblies = new uint[reply.Length];

                for(int iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
                {
                    assemblies[iAssembly] = reply[iAssembly].m_idx;
                }
            }

            //Convert Assembly Index to Idx.
            for(uint iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)            
            {
                assemblies[iAssembly] = TinyCLR_TypeSystem.IdxAssemblyFromIndex( assemblies[iAssembly] );
            }            

            //Unload dead assemblies
            for(int iAssembly = m_assemblies.Count - 1; iAssembly >= 0; iAssembly--)
            {
                CorDebugAssembly assembly = (CorDebugAssembly)m_assemblies[iAssembly];

                if(Array.IndexOf( assemblies, assembly.Idx ) < 0)
                {                 
                    callbacks.Add(new ManagedCallbacks.ManagedCallbackAssembly(assembly, ManagedCallbacks.ManagedCallbackAssembly.EventType.UnloadModule));
                    callbacks.Add(new ManagedCallbacks.ManagedCallbackAssembly(assembly, ManagedCallbacks.ManagedCallbackAssembly.EventType.UnloadAssembly));

                    m_assemblies.RemoveAt( iAssembly );                    
                }
            }

            //Load new assemblies                                    
            for(int i = 0; i < assemblies.Length; i++)
            {
                uint idx = assemblies[i];
                
                CorDebugAssembly assembly = AssemblyFromIdx( idx );

                if(assembly == null)
                {
                    //Get the primary assembly from CorDebugProcess
                    assembly = this.Process.AssemblyFromIdx( idx );
                    
                    Debug.Assert( assembly != null );

                    //create a new CorDebugAssemblyInstance
                    assembly = assembly.CreateAssemblyInstance( this );

                    Debug.Assert(assembly != null);

                    m_assemblies.Add( assembly );

                    //cpde expects mscorlib to be the first assembly it hears about
                    int index = (assembly.Name == "mscorlib") ? 0 : callbacks.Count;

                    callbacks.Insert(index, new ManagedCallbacks.ManagedCallbackAssembly(assembly, ManagedCallbacks.ManagedCallbackAssembly.EventType.LoadAssembly));
                    callbacks.Insert(index+1, new ManagedCallbacks.ManagedCallbackAssembly(assembly, ManagedCallbacks.ManagedCallbackAssembly.EventType.LoadModule));
                }
            }

            this.Process.EnqueueEvents(callbacks);

            return callbacks.Count > 0;            
        }
        
        public CorDebugProcess Process
        {
            [DebuggerHidden]
            get { return m_process; }
        }

        public Engine Engine
        {
            [DebuggerHidden]
            get { return m_process.Engine; }
        }

        public ICorDebugController ICorDebugController
        {
            get { return (ICorDebugController)this; }
        }

        public ICorDebugAppDomain ICorDebugAppDomain
        {
            get { return (ICorDebugAppDomain)this; }
        }

        public ICorDebugAppDomain2 ICorDebugAppDomain2
        {
            get { return (ICorDebugAppDomain2)this; }
        }

        #region ICorDebugController Members

        int ICorDebugController.Stop( uint dwTimeout )
        {
            return ((ICorDebugController)m_process).Stop( dwTimeout );
        }

        int ICorDebugController.Continue( int fIsOutOfBand )
        {
            return ((ICorDebugController)m_process).Continue( fIsOutOfBand );
        }

        int ICorDebugController.IsRunning( out int pbRunning )
        {
            return ((ICorDebugController)m_process).IsRunning(out pbRunning);
        }

        int ICorDebugController.HasQueuedCallbacks( ICorDebugThread pThread, out int pbQueued )
        {
            return ((ICorDebugController)m_process).HasQueuedCallbacks(pThread, out pbQueued);
        }

        int ICorDebugController.EnumerateThreads( out ICorDebugThreadEnum ppThreads )
        {
            return ((ICorDebugController)m_process).EnumerateThreads(out ppThreads);
        }

        int ICorDebugController.SetAllThreadsDebugState( CorDebugThreadState state, ICorDebugThread pExceptThisThread )
        {
            return ((ICorDebugController)m_process).SetAllThreadsDebugState(state, pExceptThisThread);
        }

        int ICorDebugController.Detach()
        {
            return ((ICorDebugController)m_process).Detach();
        }

        int ICorDebugController.Terminate( uint exitCode )
        {
            return ((ICorDebugController)m_process).Terminate(exitCode);
        }

        int ICorDebugController.CanCommitChanges( uint cSnapshots, ref ICorDebugEditAndContinueSnapshot pSnapshots, out ICorDebugErrorInfoEnum pError )
        {
            return ((ICorDebugController)m_process).CanCommitChanges(cSnapshots, ref pSnapshots, out pError);
        }

        int ICorDebugController.CommitChanges( uint cSnapshots, ref ICorDebugEditAndContinueSnapshot pSnapshots, out ICorDebugErrorInfoEnum pError )
        {
            return ((ICorDebugController)m_process).CommitChanges(cSnapshots, ref pSnapshots, out pError);
        }

        #endregion

        #region ICorDebugAppDomain Members

        int ICorDebugAppDomain.Stop( uint dwTimeout )
        {
            return this.ICorDebugController.Stop( dwTimeout );
        }

        int ICorDebugAppDomain.Continue( int fIsOutOfBand )
        {
            return this.ICorDebugController.Continue( fIsOutOfBand );
        }

        int ICorDebugAppDomain.IsRunning( out int pbRunning )
        {
            return this.ICorDebugController.IsRunning( out pbRunning );
        }

        int ICorDebugAppDomain.HasQueuedCallbacks( ICorDebugThread pThread, out int pbQueued )
        {
            return this.ICorDebugController.HasQueuedCallbacks( pThread, out pbQueued );
        }

        int ICorDebugAppDomain.EnumerateThreads( out ICorDebugThreadEnum ppThreads )
        {
            return this.ICorDebugController.EnumerateThreads( out ppThreads );
        }

        int ICorDebugAppDomain.SetAllThreadsDebugState( CorDebugThreadState state, ICorDebugThread pExceptThisThread )
        {
            return this.ICorDebugController.SetAllThreadsDebugState(state, pExceptThisThread);
        }

        int ICorDebugAppDomain.Detach()
        {
            return this.ICorDebugController.Detach();
        }

        int ICorDebugAppDomain.Terminate( uint exitCode )
        {
            return this.ICorDebugController.Terminate( exitCode );
        }

        int ICorDebugAppDomain.CanCommitChanges( uint cSnapshots, ref ICorDebugEditAndContinueSnapshot pSnapshots, out ICorDebugErrorInfoEnum pError )
        {
            return this.ICorDebugController.CanCommitChanges( cSnapshots, ref pSnapshots, out pError );
        }

        int ICorDebugAppDomain.CommitChanges( uint cSnapshots, ref ICorDebugEditAndContinueSnapshot pSnapshots, out ICorDebugErrorInfoEnum pError )
        {
            return this.ICorDebugController.CommitChanges( cSnapshots, ref pSnapshots, out pError );
        }

        int ICorDebugAppDomain.GetProcess( out ICorDebugProcess ppProcess )
        {
            ppProcess = this.Process;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.EnumerateAssemblies( out ICorDebugAssemblyEnum ppAssemblies )
        {
            ppAssemblies = new CorDebugEnum(m_assemblies, typeof(ICorDebugAssembly), typeof(ICorDebugAssemblyEnum));

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.GetModuleFromMetaDataInterface( object pIMetaData, out ICorDebugModule ppModule )
        {
            ppModule = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.EnumerateBreakpoints( out ICorDebugBreakpointEnum ppBreakpoints )
        {            
            ppBreakpoints = new CorDebugEnum(this.Process.GetBreakpoints(typeof(CorDebugBreakpoint), this), typeof(ICorDebugBreakpoint), typeof(ICorDebugBreakpointEnum));

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.EnumerateSteppers( out ICorDebugStepperEnum ppSteppers )
        {            
            ppSteppers = new CorDebugEnum(this.Process.GetBreakpoints(typeof(CorDebugStepper), this), typeof(ICorDebugBreakpoint), typeof(ICorDebugBreakpointEnum));

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.IsAttached( out int pbAttached )
        {
            pbAttached = Utility.Boolean.BoolToInt(this.Process.IsAttachedToEngine);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.GetName( uint cchName, IntPtr pcchName, IntPtr szName )
        {
            Utility.MarshalString( m_name, cchName, pcchName, szName );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.GetObject( out ICorDebugValue ppObject )
        {
            ppObject = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.Attach()
        {
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain.GetID( out uint pId )
        {
            pId = m_id;

            return Utility.COM_HResults.S_OK;
        }
        #endregion
                
        #region ICorDebugAppDomain2 Members

        int ICorDebugAppDomain2.GetArrayOrPointerType( CorElementType elementType, uint nRank, ICorDebugType pTypeArg, out ICorDebugType ppType )
        {
            ppType = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugAppDomain2.GetFunctionPointerType( uint nTypeArgs, ICorDebugType[] ppTypeArgs, out ICorDebugType ppType )
        {
            ppType = null;

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IDebugProgram2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.Detach()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.Attach(IDebugEventCallback2 pCallback)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.CauseBreak()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.GetProgramId(out Guid pguidProgramId)
        {
            pguidProgramId = m_guidProgramId;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            // CorDebugProcess.Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.EnumThreads is not implemented
            //If we need to implement this, must return a copy without any VirtualThreads
            ppEnum = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
        {
            ppEnum = null;
            ppSafety = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            ppMemoryBytes = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.Terminate()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.Continue(IDebugThread2 pThread)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.GetProcess(out IDebugProcess2 ppProcess)
        {
            ppProcess = m_process;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.Step(IDebugThread2 pThread, uint sk, uint Step)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            ppEnum = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            pbstrEngine = "Managed";
            pguidEngine = CorDebug.s_guidDebugEngine;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            ppProperty = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.WriteDump(uint DUMPTYPE, string pszDumpUrl)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.Execute()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.GetENCUpdate(out object ppUpdate)
        {
            ppUpdate = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.CanDetach()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.EnumModules(out IEnumDebugModules2 ppEnum)
        {
            ppEnum = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.GetDisassemblyStream(uint dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
        {
            ppDisassemblyStream = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgram2.GetName(out string pbstrName)
        {
            pbstrName = null;
            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IDebugProgramEx2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramEx2.Attach(IDebugEventCallback2 pCallback, uint dwReason, IDebugSession2 pSession)
        {
            //This needs to return false to allow cpde to do the attach
            return Utility.COM_HResults.S_FALSE;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramEx2.GetProgramNode(out IDebugProgramNode2 ppProgramNode)
        {
            ppProgramNode = this;
            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IDebugProgramNode2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramNode2.GetHostName(uint dwHostNameType, out string pbstrHostName)
        {
            pbstrHostName = null;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramNode2.GetProgramName(out string pbstrProgramName)
        {
            pbstrProgramName = m_name;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            pHostProcessId[0] = m_process.PhysicalProcessId;
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramNode2.DetachDebugger_V7()
        {
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramNode2.GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            pbstrEngine = null;
            pguidEngine = new Guid();
            return Utility.COM_HResults.S_OK;
        }

        int Microsoft.VisualStudio.Debugger.Interop.IDebugProgramNode2.GetHostMachineName_V7(out string pbstrHostMachineName)
        {
            pbstrHostMachineName = null;
            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IDebugCOMPlusProgramNode2 Members

        int Microsoft.VisualStudio.Debugger.Interop.IDebugCOMPlusProgramNode2.GetAppDomainId(out uint pul32Id)
        {
            pul32Id = m_id;
            return Utility.COM_HResults.S_OK;
        }

        #endregion
    }
}
