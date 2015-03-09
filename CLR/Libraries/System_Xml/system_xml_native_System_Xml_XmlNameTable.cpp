////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "system_xml.h"


HRESULT Library_system_xml_native_System_Xml_XmlNameTable::Get___STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_XmlNameTable* pThis;
    CLR_RT_HeapBlock_String*       ret  ;
    LPCSTR                         key  ;

    pThis = (CLR_RT_HeapBlock_XmlNameTable*)stack.This(); FAULT_ON_NULL(pThis);
    ret   = NULL;
    key   = stack.Arg1().RecoverString(); FAULT_ON_NULL(key);

    TINYCLR_CHECK_HRESULT(pThis->Add( key, hal_strlen_s( key ), ret, TRUE ));

    stack.SetResult_Object( ret );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_system_xml_native_System_Xml_XmlNameTable::Add___STRING__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_XmlNameTable* pThis;
    CLR_RT_HeapBlock_String*       str  ;
    LPCSTR                         key  ;
    
    pThis = (CLR_RT_HeapBlock_XmlNameTable*)stack.This(); FAULT_ON_NULL(pThis);
    str   = stack.Arg1().DereferenceString();             FAULT_ON_NULL(str);
    key   = str->StringText();

    TINYCLR_CHECK_HRESULT(pThis->Add( key, hal_strlen_s( key ), str ));

    stack.SetResult_Object( str );

    TINYCLR_NOCLEANUP();
}

