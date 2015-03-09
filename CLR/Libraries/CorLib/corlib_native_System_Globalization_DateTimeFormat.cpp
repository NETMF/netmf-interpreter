////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Globalization_DateTimeFormat::FormatDigits___STATIC__STRING__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pArgs = &(stack.Arg0());

    int value = pArgs[ 0 ].NumericByRef().s4;
    int len   = pArgs[ 1 ].NumericByRef().s4;

    char buffer[ 12 ]; // Enough to accommodate max int

    char fmt [10]; 

    if(len >= 2)
    {
        hal_snprintf( fmt, ARRAYSIZE(fmt), "%%0%dd", len );
    }
    else
    {
        fmt[0] = '%';
        fmt[1] = 'd';
        fmt[2] = '\0';
    }

    hal_snprintf( buffer, ARRAYSIZE(buffer), fmt, value );

    TINYCLR_SET_AND_LEAVE(stack.SetResult_String( buffer ));

    TINYCLR_NOCLEANUP();
}
