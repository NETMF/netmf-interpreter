////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Serialization.h"


HRESULT Library_spot_native_Microsoft_SPOT_Reflection::Serialize___STATIC__SZARRAY_U1__OBJECT__mscorlibSystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pObj = &stack.Arg1();
    CLR_UINT32 flags = 0;

    // unbox reflection types
    if(pObj->DataType() == DATATYPE_OBJECT)
    {
        pObj = pObj->Dereference();
        
        if(pObj && pObj->DataType() == DATATYPE_REFLECTION)
        {
            flags = CLR_RT_BinaryFormatter::c_Flags_Marshal;
        }
    }

    TINYCLR_SET_AND_LEAVE(CLR_RT_BinaryFormatter::Serialize( stack.PushValue(), stack.Arg0(), &stack.Arg1(), flags ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::Deserialize___STATIC__OBJECT__SZARRAY_U1__mscorlibSystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_HEADER();

    CLR_UINT32 hash = 0;
    CLR_RT_HeapBlock* pObj = &stack.Arg1();
    CLR_UINT32 flags = 0;

    // unbox reflection types
    if(pObj->DataType() == DATATYPE_OBJECT)
    {
        pObj = pObj->Dereference();
        
        if(pObj && pObj->DataType() == DATATYPE_REFLECTION)
        {
            flags = CLR_RT_BinaryFormatter::c_Flags_Marshal;
        }
    }

    TINYCLR_SET_AND_LEAVE(CLR_RT_BinaryFormatter::Deserialize( stack.PushValue(), stack.Arg0(), &stack.Arg1(), &hash, flags ));

    TINYCLR_CLEANUP();

    if(hr == CLR_E_WRONG_TYPE && hash != 0)
    {
        CLR_RT_HeapBlock& res = stack.m_owningThread->m_currentException;

        if((Library_corlib_native_System_Exception::CreateInstance( res, g_CLR_RT_WellKnownTypes.m_UnknownTypeException, CLR_E_UNKNOWN_TYPE, &stack )) == S_OK)
        {
            CLR_RT_ReflectionDef_Index idx;

            idx.InitializeFromHash( hash );

            res.Dereference()[ Library_spot_native_Microsoft_SPOT_UnknownTypeException::FIELD__m_type ].SetReflection( idx );
        }

        hr = CLR_E_PROCESS_EXCEPTION;        
    }

    TINYCLR_CLEANUP_END();
}
