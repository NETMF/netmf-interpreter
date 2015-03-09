using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using CorDebugInterop;

namespace Microsoft.SPOT.Debugger
{    
    public class CorDebugFunction : ICorDebugFunction , ICorDebugFunction2
    {
        CorDebugClass    m_class;
        Pdbx.Method      m_pdbxMethod;
        CorDebugCode     m_codeNative;
        CorDebugCode     m_codeIL;
        uint             m_tkSymbolless;

        public CorDebugFunction(CorDebugClass cls, Pdbx.Method method)
        {
            m_class = cls;
            m_pdbxMethod = method;            
        }

        public CorDebugFunction (CorDebugClass cls, uint tkSymbolless) : this (cls, null)
        {
            m_tkSymbolless = tkSymbolless;
        }

        public ICorDebugFunction ICorDebugFunction
        {
            get { return (ICorDebugFunction)this; }
        }

        public ICorDebugFunction2 ICorDebugFunction2
        {
            get { return (ICorDebugFunction2)this; }
        }

        public CorDebugClass Class
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_class; }
        }

        public CorDebugAppDomain AppDomain
        {                       
            [System.Diagnostics.DebuggerHidden]
            get { return this.Class.AppDomain; }
        }
                
        public CorDebugProcess Process
        {                       
            [System.Diagnostics.DebuggerHidden]
            get { return this.Class.Process; }
        }

        public CorDebugAssembly Assembly
        {                       
            [System.Diagnostics.DebuggerHidden]
            get { return this.Class.Assembly; }
        }

        private Engine Engine
        {
            [System.Diagnostics.DebuggerHidden]
            get { return this.Class.Engine; }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private CorDebugCode GetCode(ref CorDebugCode code)
        {
            if (code == null)
                code = new CorDebugCode(this);
            return code;
        }

        public bool HasSymbols
        {
            get { return m_pdbxMethod != null; }
        }

        public uint MethodDef_Index
        {
            get 
            {
                uint tk = HasSymbols ? m_pdbxMethod.Token.TinyCLR : m_tkSymbolless;

                return TinyCLR_TypeSystem.ClassMemberIndexFromTinyCLRToken (tk, this.m_class.Assembly);
            }
        }

        public Pdbx.Method PdbxMethod
        {
            [System.Diagnostics.DebuggerHidden]
            get {return m_pdbxMethod;}
        }

        public bool IsInternal
        {
            get {return MetaData.Helper.MethodIsInternal (this.Class.Assembly.MetaDataImport, this.m_pdbxMethod.Token.CLR); }
        }

        public bool IsInstance
        {
            get { return MetaData.Helper.MethodIsInstance(this.Class.Assembly.MetaDataImport, this.m_pdbxMethod.Token.CLR); }
        }

        public bool IsVirtual
        {
            get { return MetaData.Helper.MethodIsVirtual(this.Class.Assembly.MetaDataImport, this.m_pdbxMethod.Token.CLR); }
        }

        public uint NumArg
        {
            get {return MetaData.Helper.MethodGetNumArg (this.Class.Assembly.MetaDataImport, this.m_pdbxMethod.Token.CLR);  }
        }

        public uint GetILCLRFromILTinyCLR(uint ilTinyCLR)
        {
            uint ilCLR;
            
            //Special case for CatchHandlerFound and AppDomain transitions; possibly used elsewhere.
            if (ilTinyCLR == uint.MaxValue) return uint.MaxValue;

            ilCLR = ILComparer.Map(false, m_pdbxMethod.ILMap, ilTinyCLR);
            Debug.Assert(ilTinyCLR <= ilCLR);

            return ilCLR;
        }

        public uint GetILTinyCLRFromILCLR(uint ilCLR)
        {
            //Special case for when CPDE wants to step to the end of the function?
            if (ilCLR == uint.MaxValue) return uint.MaxValue;

            uint ilTinyCLR = ILComparer.Map(true, m_pdbxMethod.ILMap, ilCLR);

            Debug.Assert(ilTinyCLR <= ilCLR);

            return ilTinyCLR;
        }

        private class ILComparer : IComparer
        {
            bool m_fCLR;

            private ILComparer(bool fCLR)
            {
                m_fCLR = fCLR;
            }

            private static uint GetIL(bool fCLR, Pdbx.IL il)
            {
                return fCLR ? il.CLR : il.TinyCLR;
            }

            private uint GetIL(Pdbx.IL il)
            {
                return GetIL(m_fCLR, il);
            }

            private static void SetIL(bool fCLR, Pdbx.IL il, uint offset)
            {
                if (fCLR)
                    il.CLR = offset;
                else
                    il.TinyCLR = offset;
            }

            private void SetIL(Pdbx.IL il, uint offset)
            {
                SetIL(m_fCLR, il, offset);
            }

            public int Compare(object o1, object o2)
            {
                return GetIL(o1 as Pdbx.IL).CompareTo(GetIL(o2 as Pdbx.IL));
            }

            public static uint Map(bool fCLR, Pdbx.IL [] ilMap, uint offset)
            {
                ILComparer ilComparer = new ILComparer(fCLR);
                Pdbx.IL il = new Pdbx.IL();
                ilComparer.SetIL(il, offset);
                int i = Array.BinarySearch(ilMap, il, ilComparer);
                uint ret = 0;

                if (i >= 0)
                {
                    //Exact match
                    ret = GetIL(!fCLR, ilMap[i]);
                }
                else
                {

                    i = ~i;

                    if (i == 0)
                    {
                        //Before the IL diverges
                        ret = offset;
                    }
                    else
                    {
                        //Somewhere in between
                        i--;

                        il = ilMap[i];
                        ret = offset - GetIL(fCLR, il) + GetIL(!fCLR, il);
                    }
                }

                Debug.Assert(ret >= 0);
                return ret;
            }
        }

        #region ICorDebugFunction Members

        int ICorDebugFunction.GetLocalVarSigToken( out uint pmdSig )
        {
            pmdSig = 0;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugFunction.CreateBreakpoint( out ICorDebugFunctionBreakpoint ppBreakpoint )
        {
            ppBreakpoint = new CorDebugFunctionBreakpoint( this, 0 );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFunction.GetILCode( out ICorDebugCode ppCode )
        {
            ppCode = GetCode( ref m_codeIL );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFunction.GetModule( out ICorDebugModule ppModule )
        {
            m_class.ICorDebugClass.GetModule( out ppModule );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFunction.GetNativeCode( out ICorDebugCode ppCode )
        {
            ppCode = GetCode( ref m_codeNative );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFunction.GetToken( out uint pMethodDef )
        {
            pMethodDef = HasSymbols ? m_pdbxMethod.Token.CLR : m_tkSymbolless;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFunction.GetClass( out ICorDebugClass ppClass )
        {
            ppClass = m_class;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFunction.GetCurrentVersionNumber( out uint pnCurrentVersion )
        {
            pnCurrentVersion = 0;

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region ICorDebugFunction2 Members

        int ICorDebugFunction2.SetJMCStatus( int bIsJustMyCode )
        {
            bool fJMC = Utility.Boolean.IntToBool( bIsJustMyCode );

            Debug.Assert( Utility.FImplies( fJMC, this.HasSymbols ) );

            int hres = fJMC ? Utility.COM_HResults.E_FAIL : Utility.COM_HResults.S_OK;

            if(this.HasSymbols)
            {
                if(fJMC != this.m_pdbxMethod.IsJMC && m_pdbxMethod.CanSetJMC)
                {
                    if (this.Engine.Info_SetJMC(fJMC, ReflectionDefinition.Kind.REFLECTION_METHOD, this.MethodDef_Index))
                    {
                        if( !this.Assembly.IsFrameworkAssembly)
                        {
                            //now update the debugger JMC state...
                            this.m_pdbxMethod.IsJMC = fJMC;
                        }

                        hres = Utility.COM_HResults.S_OK;
                    }
                }
            }

            return hres;
        }

        int ICorDebugFunction2.GetJMCStatus( out int pbIsJustMyCode )
        {
            pbIsJustMyCode = Utility.Boolean.BoolToInt( this.HasSymbols ? this.m_pdbxMethod.IsJMC : false );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFunction2.GetVersionNumber( out uint pnVersion )
        {
            // CorDebugFunction.GetVersionNumber is not implemented
            pnVersion = 1;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugFunction2.EnumerateNativeCode( out ICorDebugCodeEnum ppCodeEnum )
        {
            ppCodeEnum = null;

            return Utility.COM_HResults.S_OK;
        }

        #endregion        
    }
}
