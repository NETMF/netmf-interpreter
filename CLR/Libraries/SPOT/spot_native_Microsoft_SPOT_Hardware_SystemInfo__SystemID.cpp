////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_Hardware_SystemInfo__SystemID::get_OEM___STATIC__U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.SetResult( OEM_Model_SKU.OEM, DATATYPE_U1 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_SystemInfo__SystemID::get_Model___STATIC__U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.SetResult( OEM_Model_SKU.Model, DATATYPE_U1 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_SystemInfo__SystemID::get_SKU___STATIC__U2( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.SetResult( OEM_Model_SKU.SKU, DATATYPE_U2 );

    TINYCLR_NOCLEANUP_NOLABEL();
}
