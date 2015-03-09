using System;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Debug = System.Diagnostics.Debug;
using Microsoft.SPOT.Debugger.WireProtocol;
using BreakpointDef = Microsoft.SPOT.Debugger.WireProtocol.Commands.Debugging_Execution_BreakpointDef;

namespace Microsoft.SPOT.Debugger
{
    //Superclass for all tinyclr breakpoints
    public abstract class CorDebugBreakpointBase
    {
        static ushort s_idNull = 0;
        static ushort s_idNext = 1;

        private CorDebugProcess m_process;
        private CorDebugAppDomain m_appDomain;
        private bool m_fActive;

        protected readonly BreakpointDef m_breakpointDef;

        protected CorDebugBreakpointBase( CorDebugProcess process )
        {                        
            m_breakpointDef = new BreakpointDef();
            m_breakpointDef.m_id = s_idNext++;
            m_breakpointDef.m_pid = BreakpointDef.c_PID_ANY;

            m_appDomain = null;
            m_process = process;

            Debug.Assert(s_idNext != s_idNull);
        }

        protected CorDebugBreakpointBase(CorDebugAppDomain appDomain) : this(appDomain.Process)
        {
            m_appDomain = appDomain;
        }

        public CorDebugAppDomain AppDomain
        {                        
            [System.Diagnostics.DebuggerHidden]
            get { return m_appDomain; }
        }

        public virtual bool IsMatch( BreakpointDef breakpointDef )
        {
            return breakpointDef.m_id == this.m_breakpointDef.m_id;
        }

        public ushort Kind
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_breakpointDef.m_flags; }

            set
            {
                m_breakpointDef.m_flags = value;
                Dirty();
            }
        }

        protected CorDebugProcess Process
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_process; }
        }

        public bool Active
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_fActive; }

            set
            {
                if (m_fActive != value)
                {
                    m_fActive = value;
                    m_process.RegisterBreakpoint(this, m_fActive);
                    Dirty();
                }
            }
        }

        public void Dirty()
        {
            m_process.DirtyBreakpoints();
        }

        public virtual void Hit(BreakpointDef breakpointDef) 
        { 
        }

        public virtual bool ShouldBreak(BreakpointDef breakpointDef) 
        { 
            return true; 
        }

        public virtual bool Equals(CorDebugBreakpointBase breakpoint)
        {
            return this.Equals( (object)breakpoint );
        }

        public BreakpointDef Debugging_Execution_BreakpointDef
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_breakpointDef; }
        }
    }

    public class CLREventsBreakpoint : CorDebugBreakpointBase
    {
        public CLREventsBreakpoint(CorDebugProcess process) : base (process)
        {
            this.Kind = BreakpointDef.c_EXCEPTION_THROWN |
                        BreakpointDef.c_EXCEPTION_CAUGHT |
#if NO_THREAD_CREATED_EVENTS                                                
                        BreakpointDef.c_EVAL_COMPLETE    |
#else 
                        BreakpointDef.c_THREAD_CREATED |
                        BreakpointDef.c_THREAD_TERMINATED |
#endif
                        BreakpointDef.c_ASSEMBLIES_LOADED | 
                        BreakpointDef.c_BREAK;

            this.Active = true;
        }

        public override void Hit(BreakpointDef breakpointDef)
        {
#if NO_THREAD_CREATED_EVENTS
            if ((breakpointDef.m_flags & BreakpointDef.c_EVAL_COMPLETE) != 0)
                EvalComplete(breakpointDef);
#else
            if ((breakpointDef.m_flags & BreakpointDef.c_THREAD_CREATED) != 0)
                ThreadCreated(breakpointDef);
            else if ((breakpointDef.m_flags & BreakpointDef.c_THREAD_TERMINATED) != 0)
                ThreadTerminated(breakpointDef);
#endif
            else if ((breakpointDef.m_flags & BreakpointDef.c_EXCEPTION_THROWN) != 0)
                ExceptionThrown(breakpointDef);
            else if ((breakpointDef.m_flags & BreakpointDef.c_EXCEPTION_CAUGHT) != 0)
                ExceptionCaught(breakpointDef);
            else if ((breakpointDef.m_flags & BreakpointDef.c_ASSEMBLIES_LOADED) != 0)
                AssembliesLoaded(breakpointDef);
            else if ((breakpointDef.m_flags & BreakpointDef.c_BREAK) != 0)
                Break(breakpointDef);
            else
                Debug.Assert(false, "unknown CLREvent breakpoint");
        }
        
#if NO_THREAD_CREATED_EVENTS                          
        private void EvalComplete(BreakpointDef breakpointDef)
        {
            CorDebugThread thread = Process.GetThread(breakpointDef.m_pid);

            //This currently gets called after BreakpointHit updates the list of threads.
            //This should nop as long as func-eval happens on separate threads.

            Debug.Assert(thread == null);
            
            Process.RemoveThread(thread);
        }
#else
        private void ThreadTerminated(BreakpointDef breakpointDef)
        {
            CorDebugThread thread = Process.GetThread(breakpointDef.m_pid);
                                 
            // Thread could be NULL if this function is called as result of Thread.Abort in 
            // managed application and application does not catch expeption.
            // ThreadTerminated is called after thread exits in managed application.
            if ( thread != null )
            {
                Process.RemoveThread( thread );
            }
        }
              
        private void ThreadCreated(BreakpointDef breakpointDef)
        {
            CorDebugThread thread = this.Process.GetThread(breakpointDef.m_pid);

            Debug.Assert(thread == null || thread.IsVirtualThread);
            if (thread == null)
            {
                thread = new CorDebugThread(this.Process, breakpointDef.m_pid, null);
                this.Process.AddThread(thread);
            }
        }
#endif

        public override bool ShouldBreak(Commands.Debugging_Execution_BreakpointDef breakpointDef)
        {
            if ((breakpointDef.m_flags & BreakpointDef.c_EXCEPTION_CAUGHT) != 0)
            {                            
                //This if statement remains for compatibility with TinyCLR pre exception filtering support.
                if((breakpointDef.m_flags & BreakpointDef.c_EXCEPTION_UNWIND) == 0)
                    return false;
            }

            return true;
        }

        private void ExceptionThrown(BreakpointDef breakpointDef)
        {
            CorDebugThread thread = this.Process.GetThread(breakpointDef.m_pid);

            thread.StoppedOnException();

            CorDebugFrame frame = thread.Chain.GetFrameFromDepthTinyCLR(breakpointDef.m_depth);
            bool fIsEval        = thread.IsVirtualThread;
            bool fUnhandled     = (breakpointDef.m_depthExceptionHandler == BreakpointDef.c_DEPTH_UNCAUGHT);

            if (this.Process.Engine.Capabilities.ExceptionFilters)
            {
                switch (breakpointDef.m_depthExceptionHandler)
                {
                    case BreakpointDef.c_DEPTH_EXCEPTION_FIRST_CHANCE:
                        Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackException(thread, frame, breakpointDef.m_IP, CorDebugExceptionCallbackType.DEBUG_EXCEPTION_FIRST_CHANCE));
                        break;
                    case BreakpointDef.c_DEPTH_EXCEPTION_USERS_CHANCE:
                        Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackException(thread, frame, breakpointDef.m_IP, CorDebugExceptionCallbackType.DEBUG_EXCEPTION_USER_FIRST_CHANCE));
                        break;
                    case BreakpointDef.c_DEPTH_EXCEPTION_HANDLER_FOUND:
                        Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackException(thread, frame, breakpointDef.m_IP, CorDebugExceptionCallbackType.DEBUG_EXCEPTION_CATCH_HANDLER_FOUND));
                        break;
                }
            }
            else
            {
                Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackException(thread, frame, breakpointDef.m_IP, CorDebugExceptionCallbackType.DEBUG_EXCEPTION_FIRST_CHANCE));

                uint depthMin = (fUnhandled) ? 0 : breakpointDef.m_depthExceptionHandler;

                for (uint depth = breakpointDef.m_depth; depth >= depthMin; depth--)
                {
                    frame = thread.Chain.GetFrameFromDepthTinyCLR(depth);

                    if (frame != null && frame.Function.HasSymbols && frame.Function.PdbxMethod.IsJMC)
                    {
                        Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackException(thread, frame, frame.IP_TinyCLR, CorDebugExceptionCallbackType.DEBUG_EXCEPTION_USER_FIRST_CHANCE));
                        break;
                    }

                    if (depth == 0)
                    {
                        break;
                    }
                }

                if(!fUnhandled)
                {
                    frame = thread.Chain.GetFrameFromDepthTinyCLR(breakpointDef.m_depthExceptionHandler);
                    Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackException(thread, frame, breakpointDef.m_IP, CorDebugExceptionCallbackType.DEBUG_EXCEPTION_CATCH_HANDLER_FOUND));
                }
            }

            if (fUnhandled)
            {
                //eval threads are virtual, and although the physical thread has an unhandled exception, the 
                //virtual thread does not.  The eval thread will get killed, but the rest of the thread chain will
                //survive, no need to confuse cpde by throwing an unhandled exception
                if (fIsEval)
                {
                    CorDebugEval eval = thread.CurrentEval;
                    eval.StoppedOnUnhandledException();

                    Debug.Assert(thread.IsVirtualThread);

                    CorDebugThread threadT = thread.GetRealCorDebugThread();
                    CorDebugFrame frameT = threadT.Chain.ActiveFrame;

                    //fake event to let debugging of unhandled exception occur.  
                    //Frame probably needs to be an InternalStubFrame???
                    frame = thread.Chain.GetFrameFromDepthCLR(thread.Chain.NumFrames - 1);
#if DEBUG
                    CorDebugInternalFrame internalFrame = frame as CorDebugInternalFrame;
                    Debug.Assert(internalFrame != null && internalFrame.FrameType == CorDebugInternalFrameType.STUBFRAME_FUNC_EVAL);
                    
#endif
                    Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackException(thread, frame, 0, CorDebugExceptionCallbackType.DEBUG_EXCEPTION_CATCH_HANDLER_FOUND));
                }
                else
                {
                    Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackException(thread, null, uint.MaxValue, CorDebugExceptionCallbackType.DEBUG_EXCEPTION_UNHANDLED));
                }
            }
        }

        private void ExceptionCaught(BreakpointDef breakpointDef)
        {
            CorDebugThread thread = this.Process.GetThread(breakpointDef.m_pid);

            CorDebugFrame frame = thread.Chain.GetFrameFromDepthTinyCLR(breakpointDef.m_depth);

            Debug.Assert((breakpointDef.m_flags & BreakpointDef.c_EXCEPTION_UNWIND) != 0);

            Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackExceptionUnwind(thread, frame, CorDebugExceptionUnwindCallbackType.DEBUG_EXCEPTION_INTERCEPTED));
        }

        private void AssembliesLoaded(BreakpointDef breakpointDef)
        {
            this.Process.UpdateAssemblies();
        }

        private void Break(BreakpointDef breakpointDef)
        {
            CorDebugThread thread = Process.GetThread(breakpointDef.m_pid);

            Process.EnqueueEvent( new ManagedCallbacks.ManagedCallbackBreak( thread ) );
        }
    }
}
