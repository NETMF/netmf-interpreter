////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

HAL_COMPLETION g_Events_BoolTimerCompletion;

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

// mutex, condition variable and flags for CLR's global events state
static std::mutex EventsMutex;
static std::condition_variable EventsConditionVar;
static UINT32 SystemEvents;

BOOL Events_Initialize( )
{
    std::unique_lock<std::mutex> scopeLock( EventsMutex );
    SystemEvents = 0;
    return TRUE;
}

BOOL Events_Uninitialize( )
{
    std::unique_lock<std::mutex> scopeLock( EventsMutex );
    SystemEvents = 0;
    return TRUE;
}

void Events_Set( UINT32 Events )
{
    {
        std::unique_lock<std::mutex> scopeLock( EventsMutex );
        SystemEvents |= Events;
    }
    EventsConditionVar.notify_all( );
}

UINT32 Events_Get( UINT32 EventsOfInterest )
{
    std::unique_lock<std::mutex> scopeLock( EventsMutex );
    auto retVal = SystemEvents & EventsOfInterest;
    SystemEvents &= ~EventsOfInterest;
    return retVal;
}

void Events_Clear( UINT32 Events )
{
    {
        std::unique_lock<std::mutex> scopeLock( EventsMutex );
        SystemEvents &= ~Events;
    }
    EventsConditionVar.notify_all( );
}

UINT32 Events_MaskedRead( UINT32 Events )
{
    return SystemEvents & Events;
}

// block this thread and wake up when at least one of the requested events is set or a timeout occurs...
UINT32 Events_WaitForEvents( UINT32 powerLevel, UINT32 WakeupSystemEvents, UINT32 Timeout_Milliseconds )
{
    std::unique_lock<std::mutex> scopeLock( EventsMutex );
    
    bool timeout = false;
    // check current condition before waiting as Condition var doesn't do that
    if( ( WakeupSystemEvents & SystemEvents ) == 0 )
    {
        timeout = !EventsConditionVar.wait_for( scopeLock
                                              , std::chrono::milliseconds( Timeout_Milliseconds )
                                              , [=]( ){ return ( WakeupSystemEvents & SystemEvents ) != 0; } 
                                              );
    }
    return timeout ? 0 : SystemEvents & WakeupSystemEvents;
}

// No idea what this was for, there are no known callers of this function in the NETMF code
//void Events_SetCallback( set_Event_Callback pfn, void* arg )
//{
//}

// No idea what this was intended for, all known implementations are an empty stub like this one...
// Strong candidate for "refactoring"!
void FreeManagedEvent( UINT8 category, UINT8 subCategory, UINT16 data1, UINT32 data2 )
{
    NATIVE_PROFILE_PAL_EVENTS( );
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
