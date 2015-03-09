////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "timedevents.h"

//------------------------------ GLOBAL     ------------------------------------

UINT32 callBackCount;

// 
//
//------------------------------ TEST CASES ------------------------------------
//
//     event test confirms two events post in the expected order. 
//     Implementation - queue event to global completion list with a 2 ms timeout...
//     and issue a Wait for some event (50 ms). If the wait-for-event fires first,
//     the 50 ms has passed instead of the completion list fire and the result is failure.
//


void CompletionCallback(void * context)
{
    callBackCount++;
    Events_Set(SYSTEM_EVENT_FLAG_UNUSED_0x80000000);
}

BOOL TimedEvents::Execute( LOG_STREAM Stream )
{
    UINT32 count    = 100;
    UINT32 signaled = FALSE;
    
    HAL_COMPLETION compObj;

    Log& log = Log::InitializeLog( Stream, "TimedEvent" );

    callBackCount = 0;

    compObj.InitializeForISR( CompletionCallback, NULL );

    while(count--)
    {
        compObj.EnqueueDelta( 150 );

        signaled = Events_WaitForEvents( SYSTEM_EVENT_FLAG_UNUSED_0x80000000, 50 );

        if(signaled != SYSTEM_EVENT_FLAG_UNUSED_0x80000000)
        {
            log.CloseLog( FALSE, "Wrong order" );

            return FALSE; 
        }
    }

    log.SetState( callBackCount );
    log.LogState  ( "%2d callbacks" );
    log.CloseLog  ( TRUE, NULL );

    return TRUE; 
}


//------------------------------ CASE SUPPORT -----------------------------------

