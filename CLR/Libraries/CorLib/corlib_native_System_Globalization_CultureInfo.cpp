////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Globalization_CultureInfo::get_CurrentUICultureInternal___STATIC__SystemGlobalizationCultureInfo( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.PushValue().SetObjectReference( g_CLR_RT_ExecutionEngine.m_currentUICulture );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Globalization_CultureInfo::set_CurrentUICultureInternal___STATIC__VOID__SystemGlobalizationCultureInfo( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    g_CLR_RT_ExecutionEngine.m_currentUICulture = stack.Arg0().Dereference();        

    TINYCLR_NOCLEANUP_NOLABEL();
}
