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

/* FreeRTOS includes. */
#if (__FREE_RTOS)

#include "FreeRTOS.h"
#include "core_cm4.h"

// this was copied from stm32f4xx_hal_cortex.h
#define NVIC_PRIORITYGROUP_4         ((uint32_t)0x00000003) /*!< 4 bits for pre-emption priority 
                                                                 0 bits for subpriority */
#endif

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

#if (__FREE_RTOS)

extern uint32_t SystemCoreClock = SYSTEM_CLOCK_HZ;

static void CLRThread(void const *argument);
static void ToggleLEDsThread(void const *argument);

// CLR thread
static void CLRThread(void const *argument) 
{
	(void) argument;

    //CPU_GPIO_EnableOutputPin(LED3, TRUE);

    ClrThreadId = osThreadGetId();

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

    // CAN'T touch heap now!!
    // UINT8* BaseAddress;
    // UINT32 SizeInBytes;
    //HeapLocation( BaseAddress,    SizeInBytes );
    //memset      ( BaseAddress, 0, SizeInBytes );

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

}

// dummy thread for testing
static void ToggleLEDsThread(void const *argument) 
{
	(void) argument;

 	for (;;) 
    {
		/* Toggle LED each 500ms */
		CPU_GPIO_EnableOutputPin(LED6, TRUE);

		osDelay(500);
		CPU_GPIO_EnableOutputPin(LED6, FALSE);

		osDelay(500);
	}
}

int main()
{
    // need to set NVIC priority otherwise FreeRTOS won't run
    // see http://www.freertos.org/RTOS-Cortex-M3-M4.html (Preempt Priority and Subpriority -> Relevance when using the RTOS)
    // Set Interrupt Group Priority
    NVIC_SetPriorityGrouping(NVIC_PRIORITYGROUP_4);
    
    // Create LED blink thread
	osThreadDef(LEDThread, ToggleLEDsThread, osPriorityNormal, 0, configMINIMAL_STACK_SIZE);
	osThreadCreate(osThread(LEDThread), NULL);
	
    // Create CLR Thread
    // TBD check stack size it's too big right now
	osThreadDef(CLRThread, CLRThread, osPriorityNormal, 0, configMINIMAL_STACK_SIZE * 10);
	osThreadCreate(osThread(CLRThread), NULL);
	
    // Start scheduler
	osKernelStart();
    
    // should never reach here
    for( ;; );
}

#else

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

#endif
