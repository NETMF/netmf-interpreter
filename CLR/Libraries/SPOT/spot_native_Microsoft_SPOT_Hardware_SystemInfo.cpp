////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_Hardware_SystemInfo::GetSystemVersion___STATIC__VOID__BYREF_I4__BYREF_I4__BYREF_I4__BYREF_I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock hbMajor;
        CLR_RT_HeapBlock hbMinor;
        CLR_RT_HeapBlock hbBuild;
        CLR_RT_HeapBlock hbRevision;

        MfReleaseInfo releaseInfo;

        Solution_GetReleaseInfo( releaseInfo );

        hbMajor.SetInteger( releaseInfo.version.usMajor );
        TINYCLR_CHECK_HRESULT(hbMajor.StoreToReference( stack.Arg0(), 0 ));

        hbMinor.SetInteger( releaseInfo.version.usMinor );
        TINYCLR_CHECK_HRESULT(hbMinor.StoreToReference( stack.Arg1(), 0 ));

        hbBuild.SetInteger( releaseInfo.version.usBuild );
        TINYCLR_CHECK_HRESULT(hbBuild.StoreToReference( stack.Arg2(), 0 ));

        hbRevision.SetInteger( releaseInfo.version.usRevision );
        TINYCLR_CHECK_HRESULT(hbRevision.StoreToReference( stack.Arg3(), 0 ));
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_SystemInfo::get_OEMString___STATIC__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    {
        MfReleaseInfo releaseInfo;

        Solution_GetReleaseInfo( releaseInfo );

        TINYCLR_SET_AND_LEAVE(stack.SetResult_String( (LPCSTR)releaseInfo.infoString ));
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_SystemInfo::get_IsBigEndian___STATIC__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    {
        volatile CLR_UINT16 val = 0x0100;
        
        stack.SetResult_Boolean((*(volatile CLR_UINT8*)&val) == 1);
    }
    TINYCLR_NOCLEANUP_NOLABEL();
}

