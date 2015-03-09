////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "system_xml.h"

HRESULT Library_system_xml_native_System_Xml_XmlReader::LookupNamespace___STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock_XmlReader* pThis;
    LPCSTR                      prefixStr;
    CLR_RT_HeapBlock_String*    prefix;
    CLR_RT_HeapBlock_String*    result = NULL;

    pThis  = (CLR_RT_HeapBlock_XmlReader*)stack.This(); FAULT_ON_NULL(pThis);

    prefixStr = stack.Arg1().RecoverString();

    if(prefixStr)
    {
        TINYCLR_CHECK_HRESULT(pThis->GetNameTable()->Add( prefixStr, hal_strlen_s( prefixStr ), prefix, true ));

        if(prefix)
        {
            result = pThis->GetNamespaces()->LookupNamespace( prefix );
        }
    }

    stack.SetResult_Object( result );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_system_xml_native_System_Xml_XmlReader::Initialize___VOID__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock_XmlReader* pThis = (CLR_RT_HeapBlock_XmlReader*)stack.This(); FAULT_ON_NULL(pThis);

    // stack->m_call.m_assm is the System.Xml assembly
    TINYCLR_SET_AND_LEAVE(pThis->Initialize( stack.Arg1().NumericByRef().u4, stack.m_call.m_assm ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_system_xml_native_System_Xml_XmlReader::ReadInternal___I4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_XmlReader* pThis = (CLR_RT_HeapBlock_XmlReader*)stack.This(); FAULT_ON_NULL(pThis);

    if(IS_XML_ERROR_CODE(hr = pThis->Read()))
    {
        stack.SetResult_I4( hr );

        TINYCLR_SET_AND_LEAVE(S_OK);
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(hr);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_system_xml_native_System_Xml_XmlReader::StringRefEquals___STATIC__BOOLEAN__STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    LPCSTR str1 = stack.Arg0().RecoverString();
    LPCSTR str2 = stack.Arg1().RecoverString();

    stack.SetResult_Boolean( (str1 == str2) /* Check reference first */ ||
                             (str1 && str2 && hal_strncmp_s( str1, str2, hal_strlen_s( str1 ) + 1 ) == 0) // else do a real string compare
                             );

    TINYCLR_NOCLEANUP_NOLABEL();
}

