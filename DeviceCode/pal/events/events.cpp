////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

#if defined(HAL_TIMEWARP)

int   s_timewarp_armingState = 0;
INT64 s_timewarp_lastButton  = TIMEWARP_DISABLE;
INT64 s_timewarp_compensate  = 0;

#endif

volatile UINT32 SystemEvents = 0;

set_Event_Callback g_Event_Callback     = NULL;
void*              g_Event_Callback_Arg = NULL;

HAL_COMPLETION g_Events_BoolTimerCompletion;

BOOL Events_Initialize()
{
    NATIVE_PROFILE_PAL_EVENTS();
    g_Events_BoolTimerCompletion.Initialize();

    InterlockedExchange( (LONG*)&SystemEvents, 0 );
    return TRUE;
}

BOOL Events_Uninitialize()
{
    NATIVE_PROFILE_PAL_EVENTS();
    g_Events_BoolTimerCompletion.Abort();
    return TRUE;
}

void Events_Set( UINT32 Events )
{
    NATIVE_PROFILE_PAL_EVENTS();
    InterlockedOr( (LONG*)&SystemEvents, Events );
    if( g_Event_Callback != NULL )
        g_Event_Callback( g_Event_Callback_Arg );
}

UINT32 Events_Get( UINT32 EventsOfInterest )
{
    NATIVE_PROFILE_PAL_EVENTS();

    UINT32 mask = ~EventsOfInterest;
    
    // capture current event flags state and clear the requested flags atomically
    UINT32 Events = InterlockedAnd( (LONG*)&SystemEvents, mask );

    // give the caller notice of just the events they asked for ( and were cleared already )
    return Events & EventsOfInterest;
}

void Events_Clear( UINT32 Events )
{
    NATIVE_PROFILE_PAL_EVENTS();

    InterlockedAnd( (LONG*)&SystemEvents, ~Events );
}

UINT32 Events_MaskedRead( UINT32 EventsOfInterest )
{
    NATIVE_PROFILE_PAL_EVENTS();
    return (SystemEvents & EventsOfInterest);
}

UINT32 Events_WaitForEvents( UINT32 sleepLevel, UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds )
{
    NATIVE_PROFILE_PAL_EVENTS();
    // do NOT call this routine with interrupts disabled,
    // as we can die here, since flags are only set in ISRs
    ASSERT_IRQ_MUST_BE_ON();

    // schedule an interrupt for this far in the future
    // timeout is in milliseconds, convert to Sleep Counts

    UINT64 CountsRemaining = CPU_MillisecondsToTicks( Timeout_Milliseconds );

#if defined(HAL_PROFILE_ENABLED)
    Events_WaitForEvents_Calls++;
#endif

    {
        GLOBAL_LOCK(irq);

        // then check to make sure the events haven't happened on the way in
        // we must do this before we sleep!

        UINT64 Expire           = HAL_Time_CurrentTicks() + CountsRemaining;
        BOOL   RunContinuations = TRUE;

        while(true)
        {
            UINT32 Events = Events_MaskedRead( WakeupSystemEvents );
            if(Events) 
                return Events;

            if(Expire <= HAL_Time_CurrentTicks())
                return 0;

            // first check and possibly run any continuations
            // but only if we have slept after stalling
            if(RunContinuations && !SystemState_QueryNoLock( SYSTEM_STATE_NO_CONTINUATIONS ))
            {
                // restore interrupts before running a continuation
                irq.Release();

                // if we stall on time, don't check again until after we sleep
                RunContinuations = HAL_CONTINUATION::Dequeue_And_Execute();

                irq.Acquire();
            }
            else
            {
                // try stalled continuations again after sleeping
                RunContinuations = TRUE;

                //lcd_printf("\fSleep=%6lld   ", CountsRemaining);
                //lcd_printf(  "Events=%08x", Events_MaskedRead(0xffffffff));

#if defined(HAL_TIMEWARP)
                if(s_timewarp_lastButton < HAL_Time_TicksToTime( HAL_Time_CurrentTicks() ))
                {
                    CountsRemaining = Expire - HAL_Time_CurrentTicks();

                    if(CountsRemaining > 0)
                    {
                        s_timewarp_compensate += (CountsRemaining * 10*1000*1000) / CPU_TicksPerSecond();
                    }
                    return 0;
                }
#endif

                ASSERT_IRQ_MUST_BE_OFF();

                HAL_COMPLETION::WaitForInterrupts( Expire, sleepLevel, WakeupSystemEvents );

                irq.Probe(); // See if we have to serve any pending interrupts.                
            }
        }
    }
}

UINT32 Events_WaitForEventsInternal( UINT32 sleepLevel, UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds )
{
    NATIVE_PROFILE_PAL_EVENTS();
    UINT32 events;

    // for internal HAL calls, we don't allow continuations
    SystemState_Set( SYSTEM_STATE_NO_CONTINUATIONS );

    events = Events_WaitForEvents( sleepLevel, WakeupSystemEvents, Timeout_Milliseconds );    // no indirection, we already have one here

    SystemState_Clear( SYSTEM_STATE_NO_CONTINUATIONS );   // nestable

    return events;
}

static void local_Events_SetBoolTimer_Callback( void* arg )
{
    NATIVE_PROFILE_PAL_EVENTS();
    BOOL* TimerCompleteFlag = (BOOL*)arg;

    *TimerCompleteFlag = TRUE;
}

void Events_SetBoolTimer( BOOL* TimerCompleteFlag, UINT32 MillisecondsFromNow )
{
    NATIVE_PROFILE_PAL_EVENTS();
    // we assume only 1 can be active, abort previous just in case
    g_Events_BoolTimerCompletion.Abort();

    if(TimerCompleteFlag)
    {
        g_Events_BoolTimerCompletion.InitializeForISR( local_Events_SetBoolTimer_Callback, TimerCompleteFlag );

        g_Events_BoolTimerCompletion.EnqueueDelta( MillisecondsFromNow * 1000 );
    }
}

void Events_SetCallback( set_Event_Callback pfn, void* arg )
{
    NATIVE_PROFILE_PAL_EVENTS();
    g_Event_Callback     = pfn;
    g_Event_Callback_Arg = arg;
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
