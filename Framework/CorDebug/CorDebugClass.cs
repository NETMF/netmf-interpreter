using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using CorDebugInterop;

namespace Microsoft.SPOT.Debugger
{
    public class CorDebugClass : ICorDebugClass, ICorDebugClass2
    {
        CorDebugAssembly m_assembly;
        Pdbx.Class m_pdbxClass;
        uint m_tkSymbolless;

        public CorDebugClass(CorDebugAssembly assembly, Pdbx.Class cls)
        {
            m_assembly = assembly;
            m_pdbxClass = cls;
        }
        
        public CorDebugClass (CorDebugAssembly assembly, uint tkSymbolless) : this(assembly, null)
        {
            m_tkSymbolless = tkSymbolless;
        }

        public ICorDebugClass ICorDebugClass
        {
            get { return (ICorDebugClass)this; }
        }

        public ICorDebugClass2 ICorDebugClass2
        {
            get { return (ICorDebugClass2)this; }
        }

        public CorDebugAssembly Assembly
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_assembly; }
        }

        public bool IsEnum
        {
            get 
            {
                if(HasSymbols)
                    return MetaData.Helper.ClassIsEnum( this.Assembly.MetaDataImport, m_pdbxClass.Token.CLR );
                else
                    return false;
            }
        }

        public Engine Engine
        {
            [System.Diagnostics.DebuggerHidden]
            get { return this.Process.Engine; }
        }

        public CorDebugProcess Process
        {
            [System.Diagnostics.DebuggerHidden]
            get { return this.Assembly.Process; }
        }

        public CorDebugAppDomain AppDomain
        {
            [System.Diagnostics.DebuggerHidden]
            get { return this.Assembly.AppDomain; }
        }

        public Pdbx.Class PdbxClass
        {
            [System.Diagnostics.DebuggerHidden]
            get {return m_pdbxClass;}
        }

        public bool HasSymbols
        {
            get { return m_pdbxClass != null; }
        }

        public uint TypeDef_Index
        {
            get
            {
                uint tk = HasSymbols ? m_pdbxClass.Token.TinyCLR : m_tkSymbolless;

                return TinyCLR_TypeSystem.ClassMemberIndexFromTinyCLRToken (tk, this.Assembly);                
            }
        }

        #region ICorDebugClass Members

        int ICorDebugClass. GetModule (out ICorDebugModule pModule)
        {
            pModule = m_assembly;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugClass. GetToken (out uint pTypeDef)
        {
            pTypeDef = HasSymbols ? m_pdbxClass.Token.CLR : m_tkSymbolless;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugClass. GetStaticFieldValue (uint fieldDef, ICorDebugFrame pFrame, out ICorDebugValue ppValue)
        {
            //Cache, and invalidate when necessary???
            uint fd = TinyCLR_TypeSystem.ClassMemberIndexFromCLRToken(fieldDef, this.Assembly);
            this.Process.SetCurrentAppDomain( this.AppDomain );
            RuntimeValue rtv = this.Engine.GetStaticFieldValue( fd );
            ppValue = CorDebugValue.CreateValue(rtv, this.AppDomain);

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region ICorDebugClass2 Members

        int ICorDebugClass2.GetParameterizedType( CorElementType elementType, uint nTypeArgs, ICorDebugType []ppTypeArgs, out ICorDebugType ppType )
        {
            // CorDebugClass.GetParameterizedType is not implemented
            ppType = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugClass2. SetJMCStatus (int bIsJustMyCode)
        {
            bool fJMC = Utility.Boolean.IntToBool(bIsJustMyCode);

            Debug.Assert(Utility.FImplies(fJMC, this.HasSymbols));

            int hres = fJMC ? Utility.COM_HResults.E_FAIL : Utility.COM_HResults.S_OK;

            if (this.HasSymbols)
            {
                if (this.Engine.Info_SetJMC(fJMC, ReflectionDefinition.Kind.REFLECTION_TYPE, this.TypeDef_Index))
                {
                    if(!m_assembly.IsFrameworkAssembly)
                    {
                        //now update the debugger JMC state...
                        foreach (Pdbx.Method m in this.m_pdbxClass.Methods)
                        {                    
                            m.IsJMC = fJMC;                    
                        }
                    }

                    hres = Utility.COM_HResults.S_OK;
                }
            }

            return hres;
        }

#endregion
    }
}
