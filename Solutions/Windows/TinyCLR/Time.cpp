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
#include "Win32TimerQueue.h"

UINT64 FileTimeToUint64( const FILETIME ft )
{
    ULARGE_INTEGER x;
    x.LowPart = ft.dwLowDateTime;
    x.HighPart = ft.dwHighDateTime;
    return x.QuadPart;
}

// "Ticks" are a platform specific unit of time.
// Typically it is the clock period of some highspeed
// counter used to track time. For Win32 we just use
// the system time converted to FILETIME
UINT64 HAL_Time_CurrentTicks()
{
    FILETIME ft = { 0 };
    GetSystemTimePreciseAsFileTime( &ft );
    return FileTimeToUint64(ft);
}

INT64 HAL_Time_CurrentTime()
{
    return HAL_Time_TicksToTime( HAL_Time_CurrentTicks() );
}

// Convert platform specific Ticks into canonical
// form (100ns intervals since 1 JAN 1601 00:00:00
// Which conveniently is also a Win32 FILETIME.
INT64 HAL_Time_TicksToTime( UINT64 Ticks )
{
    _ASSERTE(Ticks < 0x8000000000000000);
    
    return Ticks;
}

// no drift/calibration scaling needed
void HAL_Time_GetDriftParameters  ( INT32* a, INT32* b, INT64* c )
{
    *a = 1;
    *b = 1;
    *c = 0;
}

void TimerCallback()
{
    GLOBAL_LOCK(irq);
    HAL_COMPLETION::DequeueAndExec( );
}

void HAL_Time_SetCompare( UINT64 CompareValue )
{
    static std::unique_ptr<Microsoft::Win32::Timer> pCompletionsTimer;
    
    // convert to milliseconds for OS timer
    auto compareMs = CompareValue / ( CPU_TicksPerSecond() * 1000 );
    ASSERT( compareMs < UINT32_MAX);
    if( compareMs == 0 )
        TimerCallback( );
    else
        pCompletionsTimer = std::make_unique<Microsoft::Win32::Timer>( (UINT32)compareMs, TimerCallback );
}


