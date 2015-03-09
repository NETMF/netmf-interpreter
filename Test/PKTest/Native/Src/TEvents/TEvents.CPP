////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "TEvents.h"

//------------------------------ GLOBAL     ------------------------------------


// 
//
//------------------------------ TEST CASES ------------------------------------
//
//  Basic event test confirms two events post in the expected order. 
//     Implementation - queue event to global completion list with a 2 ms timeout...
//     and issue a Wait for some event (50 ms). If the wait-for-event fires first,
//     the 50 ms has passed instead of the completion list fire and the result is failure.

static const UINT32 s_event = SYSTEM_EVENT_FLAG_UNUSED_0x00000100;

void CompletionCallback(void * context)
{
    Events_Set(s_event);
}

BOOL TEvents::TEvents_1()
{
   UINT32         count = 100;
   UINT32         signaled;
   HAL_COMPLETION compObj;


   compObj.InitializeForISR(CompletionCallback, NULL);

    while (count--)
    {
        compObj.EnqueueDelta(1000*2);
        signaled = Events_WaitForEvents(s_event,50);

        if (signaled != s_event)
        {
            return false; 
        }
    }


    return true; 
}


//------------------------------ CASE SUPPORT -----------------------------------
