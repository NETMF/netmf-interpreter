using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Microsoft.SPOT.Debugger;
using WireProtocol = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.SPOT.Debugger
{
    public class CorDebugFrame : ICorDebugFrame, ICorDebugILFrame, ICorDebugNativeFrame, ICorDebugILFrame2//
    {
        const uint STACK_BEGIN = uint.MaxValue;
        const uint IP_NOT_INITIALIZED = uint.MaxValue;
        public const uint DEPTH_CLR_INVALID = uint.MaxValue;

        CorDebugChain m_chain;
        CorDebugFunction m_function;
        uint m_IP;
        uint m_depthTinyCLR;
        internal uint m_depthCLR;
        WireProtocol.Commands.Debugging_Thread_Stack.Reply.Call m_call;
#if ALL_VALUES        
        CorDebugValue[] m_valueLocals;
        CorDebugValue[] m_valueArguments;
        CorDebugValue[] m_valueEvalStack;
#endif

        public CorDebugFrame(CorDebugChain chain, WireProtocol.Commands.Debugging_Thread_Stack.Reply.Call call, uint depth)
        {
            m_chain = chain;
            m_depthTinyCLR = depth;
            m_call  = call;
            m_IP = IP_NOT_INITIALIZED;
        }

        public ICorDebugFrame ICorDebugFrame
        {
            get { return (ICorDebugFrame)this; }
        }

        public ICorDebugILFrame ICorDebugILFrame
        {
            get { return (ICorDebugILFrame)this; }
        }

        public CorDebugFrame Clone ()
        {
            return (CorDebugFrame)MemberwiseClone ();
        }

        public CorDebugProcess Process
        {
            [System.Diagnostics.DebuggerHidden]
            get { return this.Chain.Thread.Process; }
        }

        public CorDebugAppDomain AppDomain
        {
            get { return this.Function.AppDomain; }
        }

        public Engine Engine
        {
            [System.Diagnostics.DebuggerHidden]
            get { return this.Process.Engine; }
        }
        
        public CorDebugChain Chain
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_chain; }
        }
        
        public CorDebugThread Thread
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_chain.Thread; }
        }
        
        public uint DepthCLR
        {
            [System.Diagnostics.DebuggerHidden]
            get {return this.m_depthCLR;}
        }
      
        public uint DepthTinyCLR
        {
            [System.Diagnostics.DebuggerHidden]
            get {return this.m_depthTinyCLR;}
        }

        public static uint AppDomainIdFromCall( Engine engine, WireProtocol.Commands.Debugging_Thread_Stack.Reply.Call call )
        {
            uint appDomainId = CorDebugAppDomain.c_AppDomainId_ForNoAppDomainSupport;

            if(engine.Capabilities.AppDomains)
            {
                WireProtocol.Commands.Debugging_Thread_Stack.Reply.CallEx callEx = call as WireProtocol.Commands.Debugging_Thread_Stack.Reply.CallEx;

                appDomainId = callEx.m_appDomainID;
            }

            return appDomainId;
        }

        public virtual CorDebugFunction Function
        {
            get
            {
                if (m_function == null)
                {
                    uint appDomainId = AppDomainIdFromCall( this.Engine, m_call );

                    CorDebugAppDomain appDomain = Process.GetAppDomainFromId( appDomainId );
                    CorDebugAssembly assembly = appDomain.AssemblyFromIndex( m_call.m_md );;
                    
                    uint tkMethod = TinyCLR_TypeSystem.TinyCLRTokenFromMethodIndex (m_call.m_md);
                                 
                    m_function = assembly.GetFunctionFromTokenTinyCLR(tkMethod);
                }

                return m_function;
            }
        }

        public uint IP
        {
            get
            {
                if (m_IP == IP_NOT_INITIALIZED)
                {
                    m_IP = Function.HasSymbols ? Function.GetILCLRFromILTinyCLR (m_call.m_IP) : m_call.m_IP;
                }

                return m_IP;
            }
        }

        public uint IP_TinyCLR
        {
            [System.Diagnostics.DebuggerHidden]
            get {return m_call.m_IP;}
        }

        private CorDebugValue GetStackFrameValue (uint dwIndex, Engine.StackValueKind kind)
        {
            return CorDebugValue.CreateValue (this.Engine.GetStackFrameValue (m_chain.Thread.ID, m_depthTinyCLR, kind, dwIndex), this.AppDomain);
        }

        public uint Flags
        {
            get
            {
                WireProtocol.Commands.Debugging_Thread_Stack.Reply.CallEx callEx = m_call as WireProtocol.Commands.Debugging_Thread_Stack.Reply.CallEx;
                return (callEx==null)? 0 : callEx.m_flags;
            }
        }

#if ALL_VALUES
        private CorDebugValue[] EnsureValues(ref CorDebugValue[] values, uint cInfo, Engine.StackValueKind kind)
        {
            if (values == null)
            {
                values = CorDebugValue.CreateValues(this.Engine.GetStackFrameValueAll(m_chain.Thread.ID, this.DepthTinyCLR, cInfo, kind), this.Process);
            }

            return values;
        }

        
        private CorDebugValue[] Locals
        {
            get
            {
                return EnsureValues(ref m_valueLocals, m_function.PdbxMethod.NumLocal, Debugger.Engine.StackValueKind.Local);
            }
        }

        private CorDebugValue[] Arguments
        {
            get
            {
                return EnsureValues(ref m_valueArguments, m_function.PdbxMethod.NumArg, Debugger.Engine.StackValueKind.Argument);
            }
        }

        private CorDebugValue[] Evals
        {
            get
            {
                return EnsureValues(ref m_valueEvalStack, m_function.PdbxMethod.MaxStack, Debugger.Engine.StackValueKind.EvalStack);
            }
        }
#endif 
       
        public static void GetStackRange(CorDebugThread thread, uint depthCLR, out ulong start, out ulong end)
        {
            for (CorDebugThread threadT = thread.GetRealCorDebugThread (); threadT != thread; threadT = threadT.NextThread)
            {
                Debug.Assert (threadT.IsSuspended);
                depthCLR += threadT.Chain.NumFrames;
            }

            start = depthCLR;
            end = start;
        }

        #region ICorDebugFrame Members

        int ICorDebugFrame.GetChain (out ICorDebugChain ppChain)
        {
            ppChain = (ICorDebugChain)m_chain;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFrame.GetCaller (out ICorDebugFrame ppFrame)
        {
            ppFrame = (ICorDebugFrame)m_chain.GetFrameFromDepthCLR (this.m_depthCLR - 1);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFrame.GetFunctionToken (out uint pToken)
        {
            this.Function.ICorDebugFunction.GetToken (out pToken);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFrame.GetCallee (out ICorDebugFrame ppFrame)
        {
            ppFrame = (ICorDebugFrame)m_chain.GetFrameFromDepthCLR (this.m_depthCLR + 1);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFrame.GetCode (out ICorDebugCode ppCode)
        {
            ppCode = new CorDebugCode(Function);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFrame.GetFunction (out ICorDebugFunction ppFunction)
        {
            ppFunction = this.Function;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFrame.CreateStepper (out ICorDebugStepper ppStepper)
        {
            ppStepper = new CorDebugStepper(this);

            return Utility.COM_HResults.S_OK;
        }
       
        int ICorDebugFrame.GetStackRange (out ulong pStart, out ulong pEnd)
        {
            GetStackRange(this.Thread, m_depthCLR, out pStart, out pEnd);

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region ICorDebugILFrame Members

        int ICorDebugILFrame.GetChain( out ICorDebugChain ppChain )
        {
            return ((ICorDebugFrame)this).GetChain( out ppChain );            
        }

        int ICorDebugILFrame.GetCode( out ICorDebugCode ppCode )
        {
            return ((ICorDebugFrame)this).GetCode( out ppCode );            
        }

        int ICorDebugILFrame.GetFunction( out ICorDebugFunction ppFunction )
        {
            return ((ICorDebugFrame)this).GetFunction( out ppFunction );            
        }

        int ICorDebugILFrame.GetFunctionToken( out uint pToken )
        {
            return ((ICorDebugFrame)this).GetFunctionToken( out pToken );            
        }

        int ICorDebugILFrame.GetStackRange( out ulong pStart, out ulong pEnd )
        {
            return ((ICorDebugFrame)this).GetStackRange( out pStart, out pEnd );            
        }

        int ICorDebugILFrame.GetCaller( out ICorDebugFrame ppFrame )
        {
            return ((ICorDebugFrame)this).GetCaller( out ppFrame );            
        }

        int ICorDebugILFrame.GetCallee( out ICorDebugFrame ppFrame )
        {
            return ((ICorDebugFrame)this).GetCallee( out ppFrame );            
        }

        int ICorDebugILFrame.CreateStepper( out ICorDebugStepper ppStepper )
        {
            return ((ICorDebugFrame)this).CreateStepper( out ppStepper );            
        }

        int ICorDebugILFrame.GetIP( out uint pnOffset, out CorDebugMappingResult pMappingResult )
        {
            pnOffset = IP;
            pMappingResult = CorDebugMappingResult.MAPPING_EXACT;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugILFrame.SetIP( uint nOffset )
        {
            uint ip = this.Function.GetILTinyCLRFromILCLR( nOffset );
            if(Engine.SetIPOfStackFrame( this.Thread.ID, m_depthTinyCLR, ip, 0/*compute eval depth*/))
            {
                m_call.m_IP = ip;
                this.m_IP = nOffset;
            }

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugILFrame.EnumerateLocalVariables( out ICorDebugValueEnum ppValueEnum )
        {
#if ALL_VALUES
            ppValueEnum = new CorDebugEnum(this.Locals, typeof(ICorDebugValue), typeof(ICorDebugValueEnum));

            return Utility.COM_HResults.S_OK;
#else
            ppValueEnum = null;

            return Utility.COM_HResults.E_NOTIMPL;
#endif           
        }

        int ICorDebugILFrame.GetLocalVariable( uint dwIndex, out ICorDebugValue ppValue )
        {
            //Does getting all locals at once provide any savings???
#if ALL_VALUES
            ppValue = (CorDebugValue)this.Locals[(int)dwIndex];
#else
            ppValue = GetStackFrameValue( dwIndex, Engine.StackValueKind.Local );
#endif

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugILFrame.EnumerateArguments( out ICorDebugValueEnum ppValueEnum )
        {
#if ALL_VALUES
            ppValueEnum = new CorDebugEnum(this.Locals, typeof(ICorDebugValue), typeof(ICorDebugValueEnum));

            return Utility.COM_HResults.S_OK;     
#else
            ppValueEnum = null;

            return Utility.COM_HResults.E_NOTIMPL;
#endif                   
        }

        int ICorDebugILFrame.GetArgument( uint dwIndex, out ICorDebugValue ppValue )
        {
#if ALL_VALUES
            ppValue = (CorDebugValue)this.Arguments[(int)dwIndex];
#else
            ppValue = GetStackFrameValue( dwIndex, Engine.StackValueKind.Argument );
#endif

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugILFrame.GetStackDepth( out uint pDepth )
        {
            pDepth = m_depthCLR;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugILFrame.GetStackValue( uint dwIndex, out ICorDebugValue ppValue )
        {
            ppValue = GetStackFrameValue( dwIndex, Engine.StackValueKind.EvalStack );
            Debug.Assert( false, "Not tested" ); 

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugILFrame.CanSetIP( uint nOffset )
        {
            //IF WE DON"T ENSURE THAT THE IP is VALID....we are hosed
            //Not in an Exception block, at zero eval stack....etc....

            return Utility.COM_HResults.S_OK;            
        }

        #endregion

        
        #region ICorDebugILFrame2 Members

        int ICorDebugILFrame2.RemapFunction([In]uint newILOffset)
        {
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugILFrame2.EnumerateTypeParameters(out ICorDebugTypeEnum ppTyParEnum)
        {
            ppTyParEnum = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        #endregion

        //ICorDebugNative is needed for CPDE to back the IP up to the beginning
        //of a sequence point when the user intercepts an exception.
        #region ICorDebugNativeFrame Members

        int ICorDebugNativeFrame.GetChain(out ICorDebugChain ppChain)
        {
            return ((ICorDebugFrame)this).GetChain(out ppChain);
        }

        int ICorDebugNativeFrame.GetCode(out ICorDebugCode ppCode)
        {
            return ((ICorDebugFrame)this).GetCode(out ppCode);
        }

        int ICorDebugNativeFrame.GetFunction(out ICorDebugFunction ppFunction)
        {
            return ((ICorDebugFrame)this).GetFunction(out ppFunction);
        }

        int ICorDebugNativeFrame.GetFunctionToken(out uint pToken)
        {
            return ((ICorDebugFrame)this).GetFunctionToken(out pToken);
        }

        int ICorDebugNativeFrame.GetStackRange(out ulong pStart, out ulong pEnd)
        {
            return ((ICorDebugFrame)this).GetStackRange(out pStart, out pEnd);
        }

        int ICorDebugNativeFrame.GetCaller(out ICorDebugFrame ppFrame)
        {
            return ((ICorDebugFrame)this).GetCaller(out ppFrame);
        }

        int ICorDebugNativeFrame.GetCallee(out ICorDebugFrame ppFrame)
        {
            return ((ICorDebugFrame)this).GetCallee(out ppFrame);
        }

        int ICorDebugNativeFrame.CreateStepper(out ICorDebugStepper ppStepper)
        {
            return ((ICorDebugFrame)this).CreateStepper(out ppStepper);
        }

        int ICorDebugNativeFrame.GetIP(out uint pnOffset)
        {
            CorDebugMappingResult ignorable;
            return ((ICorDebugILFrame)this).GetIP(out pnOffset, out ignorable);
        }

        int ICorDebugNativeFrame.SetIP(uint nOffset)
        {
            return ((ICorDebugILFrame)this).SetIP(nOffset);
        }

        int ICorDebugNativeFrame.GetRegisterSet(out ICorDebugRegisterSet ppRegisters)
        {
            Debug.Assert(false);
            ppRegisters = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugNativeFrame.GetLocalRegisterValue(CorDebugRegister reg, uint cbSigBlob, IntPtr pvSigBlob, out ICorDebugValue ppValue)
        {
            Debug.Assert(false);
            ppValue = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugNativeFrame.GetLocalDoubleRegisterValue(CorDebugRegister highWordReg, CorDebugRegister lowWordReg, uint cbSigBlob, IntPtr pvSigBlob, out ICorDebugValue ppValue)
        {
            Debug.Assert(false);
            ppValue = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugNativeFrame.GetLocalMemoryValue(ulong address, uint cbSigBlob, IntPtr pvSigBlob, out ICorDebugValue ppValue)
        {
            Debug.Assert(false);
            ppValue = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugNativeFrame.GetLocalRegisterMemoryValue(CorDebugRegister highWordReg, ulong lowWordAddress, uint cbSigBlob, IntPtr pvSigBlob, out ICorDebugValue ppValue)
        {
            Debug.Assert(false);
            ppValue = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugNativeFrame.GetLocalMemoryRegisterValue(ulong highWordAddress, CorDebugRegister lowWordRegister, uint cbSigBlob, IntPtr pvSigBlob, out ICorDebugValue ppValue)
        {
            Debug.Assert(false);
            ppValue = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugNativeFrame.CanSetIP(uint nOffset)
        {
            return ((ICorDebugILFrame)this).CanSetIP(nOffset);
        }

        #endregion
    }

    public class CorDebugInternalFrame : CorDebugFrame, ICorDebugInternalFrame
    {
        CorDebugInternalFrameType m_type;

        public CorDebugInternalFrame( CorDebugChain chain, CorDebugInternalFrameType type )
            : base( chain, null, CorDebugFrame.DEPTH_CLR_INVALID )
        {
            m_type = type;
        }

        public CorDebugInternalFrameType FrameType
        {
            get { return m_type; }
        }

        public override CorDebugFunction Function
        {
            get { return null; }
        }

        #region ICorDebugInternalFrame Members

        int ICorDebugInternalFrame.GetChain( out ICorDebugChain ppChain )
        {
            return this.ICorDebugFrame.GetChain( out ppChain );
        }

        int ICorDebugInternalFrame.GetCode( out ICorDebugCode ppCode )
        {
            return this.ICorDebugFrame.GetCode( out ppCode );
        }

        int ICorDebugInternalFrame.GetFunction( out ICorDebugFunction ppFunction )
        {
            return this.ICorDebugFrame.GetFunction( out ppFunction );
        }

        int ICorDebugInternalFrame.GetFunctionToken( out uint pToken )
        {
            return this.ICorDebugFrame.GetFunctionToken( out pToken );
        }

        int ICorDebugInternalFrame.GetStackRange( out ulong pStart, out ulong pEnd )
        {
            return this.ICorDebugFrame.GetStackRange( out pStart, out pEnd );
        }

        int ICorDebugInternalFrame.GetCaller( out ICorDebugFrame ppFrame )
        {
            return this.ICorDebugFrame.GetCaller( out ppFrame );
        }

        int ICorDebugInternalFrame.GetCallee( out ICorDebugFrame ppFrame )
        {
            return this.ICorDebugFrame.GetCallee( out ppFrame );
        }

        int ICorDebugInternalFrame.CreateStepper( out ICorDebugStepper ppStepper )
        {
            return this.ICorDebugFrame.CreateStepper( out ppStepper );
        }

        int ICorDebugInternalFrame.GetFrameType( out CorDebugInternalFrameType pType )
        {
            pType = m_type;

            return Utility.COM_HResults.S_OK;
        }

        #endregion
    }
}
