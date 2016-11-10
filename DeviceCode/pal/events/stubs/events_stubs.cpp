////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

__weak BOOL Events_Initialize()
{
    NATIVE_PROFILE_PAL_EVENTS();
    return TRUE;
}

__weak BOOL Events_Uninitialize()
{
    NATIVE_PROFILE_PAL_EVENTS();
    return TRUE;
}

__weak void Events_Set( UINT32 Events )
{
    NATIVE_PROFILE_PAL_EVENTS();
}


__weak UINT32 Events_Get( UINT32 EventsOfInterest )
{
    NATIVE_PROFILE_PAL_EVENTS();
    return 0;
}


__weak void Events_Clear( UINT32 Events )
{
    NATIVE_PROFILE_PAL_EVENTS();
}


__weak UINT32 Events_MaskedRead( UINT32 EventsOfInterest )
{
    NATIVE_PROFILE_PAL_EVENTS();
    return 0;
}


__weak UINT32 Events_WaitForEvents( UINT32 powerLevel, UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds )
{
    NATIVE_PROFILE_PAL_EVENTS();
    return 0;
}


__weak UINT32 Events_WaitForEventsInternal( UINT32 powerLevel, UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds )
{
    NATIVE_PROFILE_PAL_EVENTS();
    return 0;
}


__weak void local_Events_SetBoolTimer_Callback( void* arg )
{
    NATIVE_PROFILE_PAL_EVENTS();
}

__weak void Events_SetBoolTimer( BOOL* TimerCompleteFlag, UINT32 MillisecondsFromNow )
{
    NATIVE_PROFILE_PAL_EVENTS();
}

__weak void Events_SetCallback( set_Event_Callback pfn, void* arg )
{
    NATIVE_PROFILE_PAL_EVENTS();
}

__weak void FreeManagedEvent(UINT8 category, UINT8 subCategory, UINT16 data1, UINT32 data2)
{
    NATIVE_PROFILE_PAL_EVENTS();
}
