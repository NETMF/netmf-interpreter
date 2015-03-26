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

extern volatile SLEEP_LEVEL SleepLevel = SLEEP_LEVEL__AWAKE;
extern volatile UINT64 WakeEvents = 0;

// used with debugger and HW trace to track sleep mode transitions
volatile bool InSleep = false;

extern void HAL_CPU_Sleep( SLEEP_LEVEL level, UINT64 wakeEvents );
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

#endif

