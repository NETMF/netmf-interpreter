////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Reflection_ConstructorInfo::Invoke___OBJECT__SZARRAY_OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*           thisRef = stack.ThisRef().Dereference();
    CLR_RT_MethodDef_Instance   md;
    CLR_RT_HeapBlock_Array*     pArray  = stack.Arg1().DereferenceArray();
    CLR_RT_HeapBlock*           args    = NULL;
    int                         numArgs = 0;

    if(md.InitializeFromIndex( thisRef->ReflectionDataConst().m_data.m_method ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_NULL_REFERENCE);

    if(stack.m_customState == 0)
    {
        stack.m_customState = 1;

        if(pArray)
        {
            args = (CLR_RT_HeapBlock*)pArray->GetFirstElement();
            numArgs = pArray->m_numOfElements;
        }

        TINYCLR_CHECK_HRESULT(stack.MakeCall( md, NULL, args, numArgs ));
    }   

    TINYCLR_NOCLEANUP();
}
