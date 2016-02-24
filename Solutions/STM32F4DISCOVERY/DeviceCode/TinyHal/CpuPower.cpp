//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <tinyhal.h>
#include <cmsis_os.h>
#include "OsHal.h"

extern void HAL_CPU_Sleep( SLEEP_LEVEL level, UINT64 wakeEvents );

#if (__FREE_RTOS)

// FreeRTOS implementation will be using standard CPU_Sleep for now
// TBD evaluate if we should implement here the vApplicationIdleHook
// TDB check how RTX implementation is using the ClrEventSignal to signal events to the CRL task 
void CPU_Sleep( SLEEP_LEVEL level, UINT64 wakeEvents )
{
    HAL_CPU_Sleep( level, wakeEvents );
}

// void vApplicationIdleHook( void )
// {
// 	/* vApplicationIdleHook() will only be called if configUSE_IDLE_HOOK is set
// 	to 1 in FreeRTOSConfig.h.  It will be called on each iteration of the idle
// 	task.  It is essential that code added to this hook function never attempts
// 	to block in any way (for example, call xQueueReceive() with a block time
// 	specified, or call vTaskDelay()).  If the application makes use of the
// 	vTaskDelete() API function (as this demo application does) then it is also
// 	important that vApplicationIdleHook() is permitted to return to its calling
// 	function, because it is the responsibility of the idle task to clean up
// 	memory allocated by the kernel to any task that has since been deleted. */
// }

#elif (__CMSIS_RTOS)

extern volatile SLEEP_LEVEL SleepLevel = SLEEP_LEVEL__AWAKE;
extern volatile UINT64 WakeEvents = 0;

// used with debugger and HW trace to track sleep mode transitions
volatile bool InSleep = false;

void CPU_Sleep( SLEEP_LEVEL level, UINT64 wakeEvents )
{
    ASSERT_IRQ_MUST_BE_OFF();
    SleepLevel = level;
    WakeEvents = wakeEvents;
    ENABLE_INTERRUPTS();
    {
        InSleep = true;
#ifdef PLATFORM_ARM_OS_PORT
        // if no events are already pending - use OS Signal to wait for
        // one. The OS will atomically clear the signal whenever this
        // thread is rescheduled.
        if( Events_MaskedRead( wakeEvents ) == 0 )
            osSignalWait( ClrEventSignal, osWaitForever );
#else
        HAL_CPU_Sleep( level, wakeEvents );
#endif
        InSleep = false;
    }
    DISABLE_INTERRUPTS();
}

#if PLATFORM_ARM_OS_PORT

void SignalClr()
{
    osSignalSet( ::GetClrThreadId(), ClrEventSignal );
}

extern "C" void os_idle_demon()
{
  /* The idle demon is a system thread, running when no other thread is      */
  /* ready to run.                                                           */
 
  for(;;)
  {
      HAL_CPU_Sleep( SleepLevel, WakeEvents );
      // Kick the CLR to wake up if it was sleeping
      SignalClr();
  }
}
#endif  // PLATFORM_ARM_OS_PORT

#else

#endif
