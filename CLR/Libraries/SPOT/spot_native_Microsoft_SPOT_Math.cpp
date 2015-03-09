////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"

static HRESULT ComputeSinCos( CLR_RT_StackFrame& stack, bool fSin )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    int index = (stack.Arg0().NumericByRef().s4 % 360) / 6; if(index < 0) index += 60;

    const CLR_RADIAN& rec = c_CLR_radians[ index ];

    stack.SetResult_I4( (int)(fSin ? rec.sin : rec.cos) );

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

HRESULT Library_spot_native_Microsoft_SPOT_Math::Cos___STATIC__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(ComputeSinCos( stack, false ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Math::Sin___STATIC__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(ComputeSinCos( stack, true ));

    TINYCLR_NOCLEANUP();
}

