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
#include <tinyhal.h>
#include "OsHal.h"

#ifdef __CC_ARM
// we include this to error at link time if any of the C semihosted support is imported
#pragma import(__use_no_semihosting_swi)
#endif

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

void BootstrapCode_GPIO();

#if PLATFORM_ARM_OS_PORT
static osThreadId ClrThreadId;

osThreadId GetClrThreadId()
{
    return ClrThreadId;
}
#endif

int main(void)
{
#if PLATFORM_ARM_OS_PORT
    ClrThreadId = osThreadGetId();
#endif
    BootstrapCode_GPIO();
    HAL_Time_Initialize();

    HAL_Initialize();

#if !defined(BUILD_RTM) 
    DEBUG_TRACE4( STREAM_LCD, ".NetMF v%d.%d.%d.%d\r\n", VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION);
    DEBUG_TRACE3(TRACE_ALWAYS, "%s, Build Date:\r\n\t%s %s\r\n", HalName, __DATE__, __TIME__);
#if defined(__GNUC__)
    DEBUG_TRACE1(TRACE_ALWAYS, "GNU Compiler version %d\r\n", __GNUC__);
#else
    DEBUG_TRACE1(TRACE_ALWAYS, "ARM Compiler version %d\r\n", __ARMCC_VERSION);
#endif

    UINT8* BaseAddress;
    UINT32 SizeInBytes;

    HeapLocation( BaseAddress,    SizeInBytes );
    memset      ( BaseAddress, 0, SizeInBytes );

    debug_printf("\f");

    debug_printf("%-15s\r\n", HalName);
    debug_printf("%-15s\r\n", "Build Date:");
    debug_printf("  %-13s\r\n", __DATE__);
    debug_printf("  %-13s\r\n", __TIME__);

#endif  // !defined(BUILD_RTM)

    /***********************************************************************************/

    {
#if defined(FIQ_SAMPLING_PROFILER)
        FIQ_Profiler_Init();
#endif
    }

    // the runtime is by default using a watchdog 
   
    Watchdog_GetSetTimeout ( WATCHDOG_TIMEOUT , TRUE );
    Watchdog_GetSetBehavior( WATCHDOG_BEHAVIOR, TRUE );
    Watchdog_GetSetEnabled ( WATCHDOG_ENABLE, TRUE );

 
    // HAL initialization completed.  Interrupts are enabled.  Jump to the Application routine
    ApplicationEntryPoint();

    debug_printf("main exited!!???.  Halting CPU\r\n");

#if defined(BUILD_RTM)
    CPU_Reset();
#else
    CPU_Halt();
#endif
    return -1;
}
