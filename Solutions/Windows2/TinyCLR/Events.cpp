////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

using namespace Microsoft::SPOT::Emulator;

////////////////////////////////////////////////////////////////////////////////////////////////////

//--//

BOOL Events_Initialize()
{
    return EmulatorNative::GetIEventsDriver()->Initialize();
}

BOOL Events_Uninitialize()
{
    return EmulatorNative::GetIEventsDriver()->Uninitialize();
}

void Events_Set( UINT32 Events )
{
    EmulatorNative::GetIEventsDriver()->Set( Events );
}

UINT32 Events_Get( UINT32 EventsOfInterest )
{
    return EmulatorNative::GetIEventsDriver()->Get( EventsOfInterest ); 
}

void Events_Clear( UINT32 Events )
{
    return EmulatorNative::GetIEventsDriver()->Clear( Events );
}

UINT32 Events_MaskedRead( UINT32 Events )
{
    return EmulatorNative::GetIEventsDriver()->MaskedRead( Events );
}

UINT32 Events_WaitForEvents( UINT32 powerLevel, UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds )
{
    return EmulatorNative::GetIEventsDriver()->WaitForEvents( powerLevel, WakeupSystemEvents, Timeout_Milliseconds ); 
}

void Events_SetBoolTimer( BOOL* TimerCompleteFlag, UINT32 MillisecondsFromNow )
{
    return EmulatorNative::GetIEventsDriver()->SetBoolTimer( (IntPtr)TimerCompleteFlag, MillisecondsFromNow ); 
}

void Events_SetCallback( set_Event_Callback pfn, void* arg )
{
    _ASSERTE(FALSE);
}

void FreeManagedEvent(UINT8 category, UINT8 subCategory, UINT16 data1, UINT32 data2)
{
    NATIVE_PROFILE_PAL_EVENTS();
    /*
    switch(category)
    {
        //case EVENT_GESTURE:
        case EVENT_TOUCH:
            break;
        default:
            break;
    }
    */
}
