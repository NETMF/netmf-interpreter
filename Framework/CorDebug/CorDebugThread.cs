using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Microsoft.SPOT.Debugger;
using WireProtocol = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.SPOT.Debugger
{
    public class CorDebugThread : ICorDebugThread, ICorDebugThread2 /* -- this is needed for v2 exception handling -- InterceptCurrentException */
    {
        CorDebugProcess m_process;
        CorDebugChain m_chain;
        uint m_id;
        bool m_fSuspended;
        bool m_fSuspendedSav; //for function eval, need to remember if this thread is suspended before suspending for function eval..      
        CorDebugValue m_currentException;
        CorDebugEval m_eval;
        bool m_fSuspendThreadEvents;
        CorDebugAppDomain m_initialAppDomain;
        bool m_fExited;

        //Doubly-linked list of virtual threads.  The head of the list is the real thread
        //All other threads (at this point), should be the cause of a function eval
        CorDebugThread m_threadPrevious;
        CorDebugThread m_threadNext;

        public CorDebugThread(CorDebugProcess process, uint id, CorDebugEval eval)
        {
            m_process = process;
            m_id = id;
            m_fSuspended = false;
            m_eval = eval;
        }

        public CorDebugEval CurrentEval
        {
            get {return m_eval;}         
        }

        public bool Exited
        {
            get { return m_fExited; }
            set { if(value) m_fExited = value; }
        }

        public bool SuspendThreadEvents
        {
            get { return m_fSuspendThreadEvents; }
            set { m_fSuspendThreadEvents = value; }
        }

        public void AttachVirtualThread(CorDebugThread thread)
        {
            CorDebugThread threadLast = this.GetLastCorDebugThread();

            threadLast.m_threadNext = thread;
            thread.m_threadPrevious = threadLast;

            m_process.AddThread(thread);
            Debug.Assert(Process.IsExecutionPaused);

            threadLast.m_fSuspendedSav = threadLast.m_fSuspended;
            threadLast.IsSuspended = true;
        }

        public bool RemoveVirtualThread(CorDebugThread thread)
        {
            //can only remove last thread
            CorDebugThread threadLast = this.GetLastCorDebugThread();

            Debug.Assert(threadLast.IsVirtualThread && !this.IsVirtualThread);
            if (threadLast != thread)
                return false;

            CorDebugThread threadNextToLast = threadLast.m_threadPrevious;

            threadNextToLast.m_threadNext = null;
            threadNextToLast.IsSuspended = threadNextToLast.m_fSuspendedSav;

            threadLast.m_threadPrevious = null;

            //Thread will be removed from process.m_alThreads when the ThreadTerminated breakpoint is hit
            return true;
        }

        public Engine Engine
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_process.Engine; }
        }

        public CorDebugProcess Process
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_process; }
        }

        public CorDebugAppDomain AppDomain
        {
            get
            {
                CorDebugAppDomain appDomain = m_initialAppDomain;

                if(!m_fExited)
                {

                    CorDebugThread thread = GetLastCorDebugThread();

                    CorDebugFrame frame = thread.Chain.ActiveFrame;

                    appDomain = frame.AppDomain;
                }

                return appDomain;
            }
        }

        public uint ID
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_id; }
        }

        public void StoppedOnException()
        {
            m_currentException = CorDebugValue.CreateValue(Engine.GetThreadException(m_id), this.AppDomain);
        }

        //This is the only thread that cpde knows about
        public CorDebugThread GetRealCorDebugThread()
        {
            CorDebugThread thread;

            for (thread = this; thread.m_threadPrevious != null; thread = thread.m_threadPrevious) ;

            return thread;
        }

        public CorDebugThread GetLastCorDebugThread()
        {
            CorDebugThread thread;

            for (thread = this; thread.m_threadNext != null; thread = thread.m_threadNext) ;

            return thread;
        }

        public CorDebugThread PreviousThread
        {
            get { return m_threadPrevious; }
        }

        public CorDebugThread NextThread
        {
            get { return m_threadNext; }
        }

        public bool IsVirtualThread
        {
            get { return m_eval != null; }
        }

        public bool IsLogicalThreadSuspended
        {
            get
            {
                return GetLastCorDebugThread().IsSuspended;
            }
        }

        public bool IsSuspended
        {
            get { return m_fSuspended; }
            set
            {
                bool fSuspend = value;

                if (fSuspend && !IsSuspended)
                {
                    this.Engine.SuspendThread(ID);
                }
                else if (!fSuspend && IsSuspended)
                {
                    this.Engine.ResumeThread(ID);
                }

                m_fSuspended = fSuspend;
            }
        }
        
        public CorDebugChain Chain
        {
            get
            {
                if (m_chain == null)
                {
                    WireProtocol.Commands.Debugging_Thread_Stack.Reply ts = this.Engine.GetThreadStack(m_id);

                    if (ts != null)
                    {
                        m_fSuspended = (ts.m_flags & WireProtocol.Commands.Debugging_Thread_Stack.Reply.TH_F_Suspended) != 0;
                        m_chain = new CorDebugChain(this, ts.m_data);

                        if(m_initialAppDomain == null)
                        {
                            CorDebugFrame initialFrame = m_chain.GetFrameFromDepthTinyCLR( 0 );
                            m_initialAppDomain = initialFrame.AppDomain;
                        }
                    }
                }
                return m_chain;
            }
        }

        public void RefreshChain()
        {
            if (m_chain != null)
            {
                m_chain.RefreshFrames();
            }
        }

        public void ResumingExecution()
        {
            if (IsSuspended)
            {
                RefreshChain();
            }
            else
            {
                m_chain = null;
                m_currentException = null;
            }
        }

        #region ICorDebugThread Members

        int ICorDebugThread.GetObject(out ICorDebugValue ppObject)
        {
            Debug.Assert(!IsVirtualThread);

            RuntimeValue rv = Engine.GetThread(m_id);

            if (rv != null)
            {
                ppObject = CorDebugValue.CreateValue(rv, this.AppDomain);
            }
            else
            {
                ppObject = null;
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.GetDebugState(out CorDebugThreadState pState)
        {
            Debug.Assert(!IsVirtualThread);

            pState = IsLogicalThreadSuspended ? CorDebugThreadState.THREAD_SUSPEND : CorDebugThreadState.THREAD_RUN;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.CreateEval(out ICorDebugEval ppEval)
        {
            Debug.Assert(!IsVirtualThread);

            ppEval = new CorDebugEval(this);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.GetHandle(out uint phThreadHandle)
        {
            Debug.Assert(!IsVirtualThread);

            //CorDebugThread.GetHandle is not implemented
            phThreadHandle = 0;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.SetDebugState(CorDebugThreadState state)
        {
            Debug.Assert(!IsVirtualThread);

            //This isnt' quite right, there is a discrepancy between CorDebugThreadState and CorDebugUserState
            //where the TinyCLR only has one thread state to differentiate between running, waiting, and stopped            
            this.IsSuspended = (state != CorDebugThreadState.THREAD_RUN);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.GetProcess(out ICorDebugProcess ppProcess)
        {
            Debug.Assert(!IsVirtualThread);
            ppProcess = m_process;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.EnumerateChains(out ICorDebugChainEnum ppChains)
        {
            Debug.Assert(!IsVirtualThread);

            ArrayList chains = new ArrayList();

            for (CorDebugThread thread = this.GetLastCorDebugThread(); thread != null; thread = thread.m_threadPrevious)
            {
                CorDebugChain chain = thread.Chain;

                if (chain != null)
                {
                    chains.Add(chain);
                }
            }

            ppChains = new CorDebugEnum(chains, typeof(ICorDebugChain), typeof(ICorDebugChainEnum));

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.GetUserState(out CorDebugUserState pState)
        {
            Debug.Assert(!IsVirtualThread);

            // CorDebugThread.GetUserState is not implemented           
            pState = 0;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.GetRegisterSet(out ICorDebugRegisterSet ppRegisters)
        {
            Debug.Assert(!IsVirtualThread);

            // CorDebugThread.GetRegisterSet is not implemented
            ppRegisters = null;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugThread.GetActiveFrame(out ICorDebugFrame ppFrame)
        {
            Debug.Assert(!IsVirtualThread);
            ((ICorDebugChain)this.GetLastCorDebugThread().Chain).GetActiveFrame(out ppFrame);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.GetActiveChain(out ICorDebugChain ppChain)
        {
            Debug.Assert(!IsVirtualThread);
            ppChain = this.GetLastCorDebugThread().Chain;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.ClearCurrentException()
        {
            Debug.Assert(!IsVirtualThread);
            Debug.Assert(false, "API for unmanaged code only?");

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.GetID(out uint pdwThreadId)
        {
            Debug.Assert(!IsVirtualThread);
            pdwThreadId = m_id;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.CreateStepper(out ICorDebugStepper ppStepper)
        {
            Debug.Assert(!IsVirtualThread);
            ICorDebugFrame frame;

            ((ICorDebugThread)this).GetActiveFrame(out frame);
            ppStepper = new CorDebugStepper((CorDebugFrame)frame);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugThread.GetCurrentException(out ICorDebugValue ppExceptionObject)
        {
            ppExceptionObject = this.GetLastCorDebugThread().m_currentException;

            return Utility.COM_HResults.BOOL_TO_HRESULT_FALSE( ppExceptionObject != null );
        }

        int ICorDebugThread.GetAppDomain(out ICorDebugAppDomain ppAppDomain)
        {
            ppAppDomain = ((CorDebugThread)this).AppDomain;

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region ICorDebugThread2 Members

        int ICorDebugThread2.GetActiveFunctions(uint cFunctions, out uint pcFunctions, COR_ACTIVE_FUNCTION[] pFunctions)
        {
            pcFunctions = 0;

            return Utility.COM_HResults.E_NOTIMPL;            
        }

        int ICorDebugThread2.GetConnectionID( out uint pdwConnectionId )
        {
            pdwConnectionId = 0;

            return Utility.COM_HResults.E_NOTIMPL;            
        }

        int ICorDebugThread2.GetTaskID( out ulong pTaskId )
        {
            pTaskId = 0;

            return Utility.COM_HResults.E_NOTIMPL;            
        }

        int ICorDebugThread2.GetVolatileOSThreadID( out uint pdwTid )
        {
            pdwTid = m_id;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugThread2.InterceptCurrentException( ICorDebugFrame pFrame )
        {
            CorDebugFrame frame = (CorDebugFrame)pFrame;

            this.Engine.UnwindThread(this.ID, frame.DepthTinyCLR);

            return Utility.COM_HResults.S_OK;
        }

        #endregion

    }
}
