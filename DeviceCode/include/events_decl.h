////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <Power_decl.h>

#ifndef _DRIVERS_EVENTS_DECL_H_
#define _DRIVERS_EVENTS_DECL_H_ 1

//--//

typedef void (*set_Event_Callback)( void* );

//--//

BOOL Events_Initialize  (               );
BOOL Events_Uninitialize(               );
void Events_Set         ( UINT32 Events );

// destructive read system event flags
UINT32 Events_Get( UINT32 EventsOfInterest );
void Events_Clear( UINT32 Events );

// non-destructive read system event flags
UINT32 Events_MaskedRead( UINT32 EventsOfInterest );


// returns 0 for timeout, non-zero are events that have happened and were asked to be waiting on (non-destructive read)
// timeout limit is about 3034 milliseconds currently
// values greater than this are capped to this

// sleep relative time into the future, or until a SystemEvent occurs, which occurs first
// timeout is a non-negative number of 1mSec ticks, or -1 (any negative value) to sleep forever until a SystemEvent occurs

// Events_WaitForEvents(0, n), sleeps for n milliseconds independent of events
// Events_WaitForEvents(0, EVENTS_TIMEOUT_INFINITE) sleeps forever.  Don't do that.
// Events_WaitForEvents(flags, EVENTS_TIMEOUT_INFINITE) waits forever for that event.

#define EVENTS_TIMEOUT_INFINITE 0xFFFFFFFF

UINT32 Events_WaitForEvents        ( UINT32 sleepLevel, UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds );
UINT32 Events_WaitForEventsInternal( UINT32 sleepLevel, UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds );

__inline UINT32 Events_WaitForEvents( UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds )
{
    return Events_WaitForEvents( SLEEP_LEVEL__SLEEP, WakeupSystemEvents, Timeout_Milliseconds );
}

void Events_SetBoolTimer( BOOL* TimerCompleteFlag, UINT32 MillisecondsFromNow );

void Events_SetCallback( set_Event_Callback pfn, void* arg );

void FreeManagedEvent(UINT8 category, UINT8 subCategory, UINT16 data1, UINT32 data2);

//--//

#endif // _DRIVERS_EVENTS_DECL_H_
