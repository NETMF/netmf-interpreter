using System;
using System.Runtime.InteropServices;
using CorDebugInterop;
using System.Diagnostics;

namespace Microsoft.SPOT.Debugger
{
    /// <summary>
    /// Summary description for CorDebugType.
    /// </summary>
    public class CorDebugTypeArray : ICorDebugType
    {
        CorDebugValueArray m_ValueArray;
        
        public CorDebugTypeArray( CorDebugValueArray valArray )
        {
            m_ValueArray = valArray; 
        }

        int ICorDebugType.EnumerateTypeParameters (out ICorDebugTypeEnum ppTyParEnum)
        {
            ppTyParEnum = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugType.GetType (out CorElementType ty)
        {
            // This is for arrays. ELEMENT_TYPE_SZARRAY - means single demensional array.
            ty = CorElementType.ELEMENT_TYPE_SZARRAY;
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugType.GetRank (out uint pnRank)
        {
            // ELEMENT_TYPE_SZARRAY - means single demensional array.
            pnRank = 1;
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugType.GetClass (out ICorDebugClass ppClass)
        {
            ppClass = CorDebugValue.ClassFromRuntimeValue(m_ValueArray.RuntimeValue, m_ValueArray.AppDomain);
            return Utility.COM_HResults.S_OK;
        }

        /*
         *  The function ICorDebugType.GetFirstTypeParameter returns the type 
         *  of element in the array.
         *  It control viewing of arrays elements in the watch window of debugger.
         */
        int ICorDebugType.GetFirstTypeParameter (out ICorDebugType value)
        {
            value = new CorDebugGenericType(CorElementType.ELEMENT_TYPE_CLASS, m_ValueArray.RuntimeValue, m_ValueArray.AppDomain);
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugType.GetStaticFieldValue (uint fieldDef, ICorDebugFrame pFrame, out ICorDebugValue ppValue)
        {            
            ppValue = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugType.GetBase (out ICorDebugType pBase)
        {
            pBase = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }
    }

    public class CorDebugGenericType : ICorDebugType
    {
        CorElementType m_elemType;
        public RuntimeValue m_rtv;
        public CorDebugAppDomain m_appDomain;
        private CorDebugAssembly m_assembly;

        // This is used to resolve values into types when we know the appdomain, but not the assembly.
        public CorDebugGenericType(CorElementType elemType, RuntimeValue rtv, CorDebugAppDomain appDomain)
        {
            m_elemType = elemType;
            m_rtv = rtv;
            m_appDomain = appDomain;
        }

        // This constructor is used exclusively for resovling potentially (but never really) generic classes into fully specified types.
        // Generics are not supported by netmf, but we still need to be able to convert classes into fully specified types.
        public CorDebugGenericType(CorElementType elemType, RuntimeValue rtv, CorDebugAssembly assembly)
        {
            m_elemType = elemType;
            m_rtv = rtv;
            m_assembly = assembly;
        }

        int ICorDebugType.EnumerateTypeParameters(out ICorDebugTypeEnum ppTyParEnum)
        {
            ppTyParEnum = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugType.GetType(out CorElementType ty)
        {
            // Return CorElementType element type. 
            ty = m_elemType;
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugType.GetRank(out uint pnRank)
        {
            // Not an array. Thus rank is zero
            pnRank = 0;
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugType.GetClass(out ICorDebugClass ppClass)
        {
            ppClass = CorDebugValue.ClassFromRuntimeValue(m_rtv, this.AppDomain);
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugType.GetFirstTypeParameter(out ICorDebugType value)
        {
            // For non-arrays there is not first parameter.
            value = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugType.GetStaticFieldValue(uint fieldDef, ICorDebugFrame pFrame, out ICorDebugValue ppValue)
        {
            uint fd = TinyCLR_TypeSystem.ClassMemberIndexFromCLRToken(fieldDef, this.Assembly);
            this.Process.SetCurrentAppDomain(this.AppDomain);
            RuntimeValue rtv = this.Engine.GetStaticFieldValue(fd);
            ppValue = CorDebugValue.CreateValue(rtv, this.AppDomain);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugType.GetBase(out ICorDebugType pBase)
        {
            pBase = null;
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public CorDebugAssembly Assembly
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_assembly; }
        }

        public Engine Engine
        {
            [System.Diagnostics.DebuggerHidden]
            get { return this.Process?.Engine; }
        }

        public CorDebugProcess Process
        {
            [System.Diagnostics.DebuggerHidden]
            get { return this.Assembly?.Process; }
        }

        public CorDebugAppDomain AppDomain
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                if (m_appDomain != null)
                    return m_appDomain;
                else
                    return this.Assembly?.AppDomain;
            }
        }

    }
}
