////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_WeakDelegate::Combine___STATIC__mscorlibSystemDelegate__mscorlibSystemDelegate__mscorlibSystemDelegate( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_Delegate_List::Combine( stack.PushValue(), stack.Arg0(), stack.Arg1(), true ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_WeakDelegate::Remove___STATIC__mscorlibSystemDelegate__mscorlibSystemDelegate__mscorlibSystemDelegate( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_Delegate_List::Remove( stack.PushValue(), stack.Arg0(), stack.Arg1() ));

    TINYCLR_NOCLEANUP();
}
