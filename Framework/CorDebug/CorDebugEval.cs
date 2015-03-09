using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using CorDebugInterop;
using BreakpointDef = Microsoft.SPOT.Debugger.WireProtocol.Commands.Debugging_Execution_BreakpointDef;

namespace Microsoft.SPOT.Debugger
{    
    public class CorDebugEval : ICorDebugEval, ICorDebugEval2
    {
        const int SCRATCH_PAD_INDEX_NOT_INITIALIZED = -1;

        bool m_fActive;
        CorDebugThread m_threadReal;
        CorDebugThread m_threadVirtual;
        CorDebugAppDomain m_appDomain;
        int m_iScratchPad;
        CorDebugValue m_resultValue;
        EvalResult m_resultType;
        bool m_fException;

        public enum EvalResult
        {
            NotFinished,
            Complete,
            Abort,
            Exception,
        }
        
        public CorDebugEval (CorDebugThread thread) 
        {
            m_appDomain = thread.Chain.ActiveFrame.AppDomain;
            m_threadReal = thread;
            m_resultType = EvalResult.NotFinished;
            ResetScratchPadLocation ();            
        }

        public CorDebugThread ThreadVirtual
        {
            get { return m_threadVirtual; }
        }

        public CorDebugThread ThreadReal
        {
            get { return m_threadReal; }
        }

        public Engine Engine
        {
            get { return Process.Engine; }
        }

        public CorDebugProcess Process
        {
            get { return m_threadReal.Process; }
        }

        public CorDebugAppDomain AppDomain
        {
            get { return m_appDomain; }
        }

        private CorDebugValue GetResultValue ()
        {
            if (m_resultValue == null)
            {
                m_resultValue = Process.ScratchPad.GetValue (m_iScratchPad, m_appDomain);
            }

            return m_resultValue;
        }

        private int GetScratchPadLocation ()
        {
            if (m_iScratchPad == SCRATCH_PAD_INDEX_NOT_INITIALIZED)
            {
                m_iScratchPad = Process.ScratchPad.ReserveScratchBlock ();
            }

            return m_iScratchPad;
        }

        private void ResetScratchPadLocation()
        {
            m_iScratchPad = SCRATCH_PAD_INDEX_NOT_INITIALIZED;
            m_resultValue = null;            
        }

        public void StoppedOnUnhandledException()
        {
            /*
             * store the fact that this eval ended with an exception
             * the exception will be stored in the scratch pad by TinyCLR
             * but the event to cpde should wait to be queued until the eval
             * thread completes.  At that time, the information is lost as to
             * the fact that the result was an exception (rather than the function returning
             * an object of type exception. Hence, this flag.
            */
            m_fException = true;
        }

        public void EndEval (EvalResult resultType, bool fSynchronousEval)
        {
            try
            {
                //This is used to avoid deadlock.  Suspend commands synchronizes on this.Process                
                Process.SuspendCommands(true);

                Debug.Assert(Utility.FImplies(fSynchronousEval, !m_fActive));

                if (fSynchronousEval || m_fActive)  //what to do if the eval isn't active anymore??
                {
                    bool fKillThread = false;

                    if (m_threadVirtual != null)
                    {
                        if (m_threadReal.GetLastCorDebugThread() != m_threadVirtual)
                            throw new ArgumentException();

                        m_threadReal.RemoveVirtualThread(m_threadVirtual);
                    }

                    //Stack frames don't appear if they are not refreshed
                    if (fSynchronousEval)
                    {
                        for (CorDebugThread thread = this.m_threadReal; thread != null; thread = thread.NextThread)
                        {
                            thread.RefreshChain();
                        }
                    }

                    if(m_fException)
                    {
                        resultType = EvalResult.Exception;
                    }

                    //Check to see if we are able to EndEval -- is this the last virtual thread?
                    m_fActive = false;
                    m_resultType = resultType;
                    switch (resultType)
                    {
                        case EvalResult.Complete:
                            Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackEval(m_threadReal, this, ManagedCallbacks.ManagedCallbackEval.EventType.EvalComplete));
                            break;

                        case EvalResult.Exception:
                            Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackEval(m_threadReal, this, ManagedCallbacks.ManagedCallbackEval.EventType.EvalException));                             
                            break;

                        case EvalResult.Abort:
                            fKillThread = true;
                            /* WARNING!!!!
                             * If we do not give VS a EvalComplete message within 3 seconds of them calling ICorDebugEval::Abort then VS will attempt a RudeAbort
                             * and will display a scary error message about a serious internal debugger error and ignore all future debugging requests, among other bad things.
                             */
                            Process.EnqueueEvent(new ManagedCallbacks.ManagedCallbackEval(m_threadReal, this, ManagedCallbacks.ManagedCallbackEval.EventType.EvalComplete));
                            break;
                    }

                    if (fKillThread && m_threadVirtual != null)
                    {
                        Engine.KillThread(m_threadVirtual.ID);
                    }

                    if (resultType == EvalResult.Abort)
                    {
                        Process.PauseExecution();
                    }
                }
            }
            finally
            {
                Process.SuspendCommands(false);
            }
        }

        private uint GetTypeDef_Index(CorElementType elementType, ICorDebugClass pElementClass)
        {
            uint tdIndex;

            if (pElementClass != null)
            {
                tdIndex = ((CorDebugClass)pElementClass).TypeDef_Index;
            }
            else
            {
                CorDebugProcess.BuiltinType builtInType = this.Process.ResolveBuiltInType(elementType);
                tdIndex = builtInType.GetClass(this.m_appDomain).TypeDef_Index;
            }

            return tdIndex;
        }
        
        #region ICorDebugEval Members

        int ICorDebugEval.IsActive (out int pbActive)
        {
            pbActive = Utility.Boolean.BoolToInt (m_fActive);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.GetThread( out ICorDebugThread ppThread )
        {
            ppThread = m_threadReal;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.GetResult( out ICorDebugValue ppResult )
        {
            switch (m_resultType)
            {
                case EvalResult.Exception:
                case EvalResult.Complete:
                    ppResult = GetResultValue ();
                    break;
                default:
                    ppResult = null;
                    throw new ArgumentException ();
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.NewArray(CorElementType elementType, ICorDebugClass pElementClass, uint rank, ref uint dims, ref uint lowBounds)
        {            
            if(rank != 1) return Utility.COM_HResults.E_FAIL;
            
            this.Process.SetCurrentAppDomain( this.AppDomain );
            uint tdIndex = GetTypeDef_Index(elementType, pElementClass);
            Engine.AllocateArray(GetScratchPadLocation(), tdIndex, 1, (int)dims);            
            EndEval (EvalResult.Complete, true);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.CallFunction( ICorDebugFunction pFunction, uint nArgs, ICorDebugValue[] ppArgs )
        {
            try
            {
                //CreateThread will cause a thread create event to occur.  This is a virtual thread, so 
                //we need to suspend processing of tinyclr commands until we have created the thread ourselves
                //and the processing of a new virtual thread will be ignored.
                Process.SuspendCommands (true);

                //need to flush the breakpoints in case new breakpoints were waiting until process was resumed.
                Process.UpdateBreakpoints ();   

                Debug.Assert (nArgs == ppArgs.Length);
                Debug.Assert (Process.IsExecutionPaused);

                CorDebugFunction function = (CorDebugFunction)pFunction;

                uint md = function.MethodDef_Index;
                if(function.IsVirtual && function.IsInstance)
                {
                    Debug.Assert(nArgs > 0);
                    md = this.Engine.GetVirtualMethod(function.MethodDef_Index, ((CorDebugValue) ppArgs[0]).RuntimeValue);
                }

                this.Process.SetCurrentAppDomain( this.AppDomain );
                //Send the selected thread ID to the device so calls that use Thread.CurrentThread work as the user expects.
                uint pid = this.Engine.CreateThread (md, GetScratchPadLocation (), m_threadReal.ID);

                if (pid == uint.MaxValue)
                {
                    throw new ArgumentException("TinyCLR cannot call this function.  Possible reasons include: ByRef arguments not supported");
                }

                //If anything below fails, we need to clean up by killing the thread
                if (nArgs > 0)
                {
                    RuntimeValue[] args = this.Engine.GetStackFrameValueAll (pid, 0, function.NumArg, Engine.StackValueKind.Argument);

                    for (int iArg = 0; iArg < nArgs; iArg++)
                    {
                        CorDebugValue valSrc = (CorDebugValue)ppArgs[iArg];
                        CorDebugValue valDst = CorDebugValue.CreateValue (args[iArg], m_appDomain);

                        if (valDst.RuntimeValue.Assign(valSrc.RuntimeValue) == null)
                        {
                            throw new ArgumentException("TinyCLR cannot set argument " + iArg);
                        }
                    }
                }

                m_threadVirtual = new CorDebugThread (this.Process, pid, this);
                m_threadReal.AttachVirtualThread (m_threadVirtual);
                Debug.Assert (!m_fActive);
                m_fActive = true;                

                //It is possible that a hard breakpoint is hit, the first line of the function
                //to evaluate.  If that is the case, than breakpoints need to be drained so the 
                //breakpoint event is fired, to avoid a race condition, where cpde resumes 
                //execution to start the function eval before it gets the breakpoint event
                //This is primarily due to the difference in behavior of the TinyCLR and the desktop.
                //In the desktop, the hard breakpoint will not get hit until execution is resumed.
                //The TinyCLR can hit the breakpoint during the Thread_Create call.
                
                Process.DrainBreakpoints ();                
            }
            finally
            {
                Process.SuspendCommands (false);                
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.NewString( string @string )
        {
            this.Process.SetCurrentAppDomain( this.AppDomain );
            
            //changing strings is dependant on this method working....
            Engine.AllocateString (GetScratchPadLocation (), @string);            
            EndEval (EvalResult.Complete, true);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.NewObjectNoConstructor( ICorDebugClass pClass )
        {
            this.Process.SetCurrentAppDomain( this.AppDomain );
            Engine.AllocateObject (GetScratchPadLocation (), ((CorDebugClass)pClass).TypeDef_Index);            
            EndEval (EvalResult.Complete, true);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.CreateValue( CorElementType elementType, ICorDebugClass pElementClass, out ICorDebugValue ppValue )
        {
            uint tdIndex = GetTypeDef_Index(elementType, pElementClass);
            Debug.Assert(Utility.FImplies(pElementClass != null, elementType == CorElementType.ELEMENT_TYPE_VALUETYPE)); 

            this.Process.SetCurrentAppDomain( this.AppDomain );
            Engine.AllocateObject (GetScratchPadLocation (), tdIndex);
            ppValue = GetResultValue ();
            ResetScratchPadLocation ();

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.NewObject( ICorDebugFunction pConstructor, uint nArgs, ICorDebugValue[] ppArgs )
        {
            Debug.Assert (nArgs == ppArgs.Length);

            CorDebugFunction f = (CorDebugFunction)pConstructor;
            CorDebugClass c = f.Class;

            this.Process.SetCurrentAppDomain( this.AppDomain );
            Engine.AllocateObject (GetScratchPadLocation (), c.TypeDef_Index);
            
            ICorDebugValue[] args = new ICorDebugValue[nArgs + 1];

            args[0] = GetResultValue ();
            ppArgs.CopyTo (args, 1);
            ((ICorDebugEval)this).CallFunction (pConstructor, (uint)args.Length, args);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugEval.Abort ()
        {
            EndEval (EvalResult.Abort, false);

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region ICorDebugEval2 Members

        int ICorDebugEval2.CallParameterizedFunction(ICorDebugFunction pFunction, uint nTypeArgs, ICorDebugType[] ppTypeArgs, uint nArgs, ICorDebugValue[] ppArgs)
        {
            return ((ICorDebugEval)this).CallFunction(pFunction, nArgs, ppArgs);
        }

        int ICorDebugEval2.CreateValueForType(ICorDebugType pType, out ICorDebugValue ppValue)
        {
            CorElementType type;
            ICorDebugClass cls;

            pType.GetType( out type );
            pType.GetClass( out cls );

            return ((ICorDebugEval)this).CreateValue(type, cls, out ppValue);
        }

        int ICorDebugEval2.NewParameterizedObject(ICorDebugFunction pConstructor, uint nTypeArgs, ICorDebugType[] ppTypeArgs, uint nArgs, ICorDebugValue[] ppArgs)
        {
            return ((ICorDebugEval)this).NewObject(pConstructor, nArgs, ppArgs);
        }

        int ICorDebugEval2.NewParameterizedObjectNoConstructor(ICorDebugClass pClass, uint nTypeArgs, ICorDebugType[] ppTypeArgs)
        {
            return ((ICorDebugEval)this).NewObjectNoConstructor(pClass);
        }

        int ICorDebugEval2.NewParameterizedArray(ICorDebugType pElementType, uint rank, ref uint dims, ref uint lowBounds)
        {
            CorElementType type;
            ICorDebugClass cls;

            pElementType.GetType(out type);
            pElementType.GetClass(out cls);

            return ((ICorDebugEval)this).NewArray(type, cls, rank, dims, lowBounds);
        }

        int ICorDebugEval2.NewStringWithLength(string @string, uint uiLength)
        {
            string strVal = @string.Substring(0, (int)uiLength);
            
            return ((ICorDebugEval)this).NewString(strVal);
        }

        int ICorDebugEval2.RudeAbort()
        {
            return ((ICorDebugEval)this).Abort();
        }
        #endregion
    }
}
