////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for CMSIS: Copyright (c) Eclo Solutions Lda.
//
//  *** System Timer Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

static UINT64 g_nextEvent;   // tick time of next event to be scheduled
TIM_HandleTypeDef       masterTimerHandle;
TIM_HandleTypeDef       slaveTimerHandle;

//--//

/*
NETMF time base requires a resolution of 100ns (1MHz).
That requires a timer with 48 bits resolution, which doesn't exist in the supported Cortex M platforms.
The solution is to synchronize 2 timers:
    - MASTER with 32 bits resolution (lower bits)
    - SLAVE with 16 bits resolution (upper bits)
    (16+32=48, check!)

In previous NETMF HAL implementations the developer could choose which timers would be synchronized to implement the HAL timer.
For the current HAL implementation (CMSIS based) we choose to go with a fixed configuration, for simplicity.

TIM2 will be used as "master" timer synchronized with TIM3 "slave".  
      
*/
BOOL HAL_Time_Initialize()
{
    g_nextEvent = 0xFFFFFFFFFFFF; // never
    
    TIM_MasterConfigTypeDef sMasterConfig;
    TIM_SlaveConfigTypeDef  sSlaveConfig;
    RCC_ClkInitTypeDef      clkconfig;
    uint32_t                uwTimclock, uwAPB1Prescaler = 0U;
    uint32_t                uwPrescalerValue = 0U;
    uint32_t                pFLatency;

    // master (32 bit) timer is Timer 2
    masterTimerHandle.Instance = TIM2;
    // slave (16 bit) timer is Timer 3
    slaveTimerHandle.Instance = TIM3;
    
    // enable master clock
    __TIM2_CLK_ENABLE();

    // Configure the master timer IRQ priority one level above the HAL tick
    HAL_NVIC_SetPriority(TIM2_IRQn, TICK_INT_PRIORITY + 1, 0U);
    // enable master timer global Interrupt
    HAL_NVIC_EnableIRQ(TIM2_IRQn);
        
    // enable slave clock
    __TIM3_CLK_ENABLE();

    /* Get clock configuration */
    HAL_RCC_GetClockConfig(&clkconfig, &pFLatency);

    /* Get APB1 prescaler */
    uwAPB1Prescaler = clkconfig.APB1CLKDivider;

    /* Compute TIM clock */
    if (uwAPB1Prescaler == RCC_HCLK_DIV1) 
    {
        uwTimclock = HAL_RCC_GetPCLK1Freq();
    }
    else
    {
        uwTimclock = 2*HAL_RCC_GetPCLK1Freq();
    }

    /* Compute the prescaler value to have the timer counter clock equal to 1MHz */
    uwPrescalerValue = (uint32_t) ((uwTimclock / 1000000U) - 1U);

    /* master timer initialization as follows:
    Period = [(TIM2CLK/1000) - 1]. to have a (1/1000) s time base.
    Prescaler = (uwTimclock/1000000 - 1) to have a 1MHz counter clock.
    ClockDivision = 0
    Counter direction = Up
    */
    masterTimerHandle.Init.Period = 0xFFFFFFFF;
    masterTimerHandle.Init.Prescaler = uwPrescalerValue;
    masterTimerHandle.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
    masterTimerHandle.Init.CounterMode = TIM_COUNTERMODE_UP;
    
    if(HAL_TIM_Base_Init(&masterTimerHandle) != HAL_OK)
    {
        // something went wrong
        return FALSE;
    }
        
    // master configration for the master timer
    sMasterConfig.MasterOutputTrigger = TIM_TRGO_UPDATE;
    sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_ENABLE;
    if(HAL_TIMEx_MasterConfigSynchronization(&masterTimerHandle, &sMasterConfig) != HAL_OK)
    {
        /* Configuration Error */
        // FIXME
        //Error_Handler();
    }


    // slave timer
    slaveTimerHandle.Init.Period = 0xFFFF;
    slaveTimerHandle.Init.Prescaler = 0;
    slaveTimerHandle.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
    slaveTimerHandle.Init.CounterMode = TIM_COUNTERMODE_UP;
    
    if(HAL_TIM_Base_Init(&slaveTimerHandle) != HAL_OK)
    {
        // something went wrong
        return FALSE;
    }
        
    // slave configuration for the slave timer
    sSlaveConfig.SlaveMode = TIM_SLAVEMODE_GATED;
    sSlaveConfig.InputTrigger = TIM_TS_ITR1;
    if(HAL_TIM_SlaveConfigSynchronization(&slaveTimerHandle, &sSlaveConfig) != HAL_OK)
    {
        /* Configuration Error */
        // FIXME
        //Error_Handler();
    }
    
    // Starts the 32 bits timer Input Capture measurement in interrupt mode for channel 1

    if(HAL_TIM_Base_Start(&slaveTimerHandle) == HAL_OK)
    {
        // Starts the 32 bits timer Input Capture measurement in interrupt mode for channel 1
        if(HAL_TIM_IC_Start_IT(&masterTimerHandle, TIM_CHANNEL_1) == HAL_OK)
        {
            return TRUE;
        }
    }
            
    return FALSE;
}

BOOL HAL_Time_Uninitialize()
{
    // disable master timer clock
    __TIM2_CLK_DISABLE();
    // disable interrupt for master timer 
    HAL_NVIC_DisableIRQ(TIM2_IRQn);  
    // disable slave timer clock
    __TIM3_CLK_DISABLE(); 

    return TRUE;
}

#pragma arm section code = "SectionForFlashOperations"
UINT64 __section("SectionForFlashOperations") HAL_Time_CurrentTicks()
{
    UINT64 ticks, lowerTicks; // cascaded timers

    // read counters from timers
    ticks = (UINT64)__HAL_TIM_GET_COUNTER(&slaveTimerHandle);
    lowerTicks = (UINT64)__HAL_TIM_GET_COUNTER(&masterTimerHandle);
    
    ticks = ticks << 32;
    ticks = ticks | lowerTicks;

    return ticks;
}
#pragma arm section code

INT64 HAL_Time_TicksToTime(UINT64 ticks)
{
    return CPU_TicksToTime(ticks);
}

INT64 HAL_Time_CurrentTime()
{
    return CPU_TicksToTime(HAL_Time_CurrentTicks());
}

void HAL_Time_SetCompare(UINT64 compareValue)
{
    GLOBAL_LOCK(irq);
    
    g_nextEvent = compareValue;
    
    masterTimerHandle.Instance->CCR1 = (UINT32)compareValue; // compare to low bits
    
    if (HAL_Time_CurrentTicks() >= compareValue) 
    { 
        // missed event
        // trigger immediate interrupt with event source being capture compare 1
        HAL_TIM_GenerateEvent(&masterTimerHandle, TIM_EVENTSOURCE_CC1);
    }
}

#pragma arm section code = "SectionForFlashOperations"
//
// To calibrate this constant, uncomment #define CALIBRATE_SLEEP_USEC in TinyHAL.c
//
#define STM32F4_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS 3

void __section("SectionForFlashOperations") HAL_Time_Sleep_MicroSeconds(UINT32 uSec)
{
    GLOBAL_LOCK(irq);

    UINT32 current = HAL_Time_CurrentTicks();
    UINT32 maxDiff = CPU_MicrosecondsToTicks(uSec);

    if(maxDiff <= STM32F4_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS)
    {
        maxDiff  = 0;
    } 
    else
    {
        maxDiff -= STM32F4_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS;
    }

    while(((INT32)(HAL_Time_CurrentTicks() - current)) <= maxDiff);    
}

void HAL_Time_Sleep_MicroSeconds_InterruptEnabled(UINT32 uSec)
{
    // iterations must be signed so that negative iterations will result in the minimum delay
    uSec *= (HAL_RCC_GetHCLKFreq() / 1000000);

    // iterations is equal to the number of CPU instruction cycles in the required time minus
    // overhead cycles required to call this subroutine.
    int iterations = (int)uSec - 5;      // Subtract off call & calculation overhead

    CYCLE_DELAY_LOOP(iterations);
}
#pragma arm section code

void HAL_Time_GetDriftParameters(INT32* a, INT32* b, INT64* c)
{
    *a = 1;
    *b = 1;
    *c = 0;
}

UINT32 CPU_SystemClock()
{
    return HAL_RCC_GetHCLKFreq();
}

UINT32 CPU_TicksPerSecond()
{
    // this is always 1MHz because of the configuration of the base timer used for the NETMF HAL
    return 1000000;
}

UINT64 CPU_MillisecondsToTicks(UINT64 ticks)
{
    return ticks;
}

UINT64 CPU_MillisecondsToTicks(UINT32 ticks32)
{
    return (UINT64)ticks32;
}

#pragma arm section code = "SectionForFlashOperations"
UINT64 __section("SectionForFlashOperations") CPU_MicrosecondsToTicks(UINT64 uSec)
{
    return uSec;
}

UINT32 __section("SectionForFlashOperations") CPU_MicrosecondsToTicks(UINT32 uSec)
{
    return uSec;
}
#pragma arm section code

UINT32 CPU_MicrosecondsToSystemClocks(UINT32 uSec)
{
    return uSec * (HAL_RCC_GetHCLKFreq() / 1000000);
}

UINT64 CPU_TicksToTime(UINT64 ticks)
{
    return ticks * 10;
}

UINT64 CPU_TicksToTime(UINT32 ticks32)
{
    return (UINT64)ticks32 * 10;
}

// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void TIM2_IRQHandler(void)
{
    HAL_TIM_IRQHandler(&masterTimerHandle);
}

void HAL_TIM_OC_DelayElapsedCallback(TIM_HandleTypeDef *htim) 
{
    if (htim->Channel == HAL_TIM_ACTIVE_CHANNEL_1) 
    {
        if (HAL_Time_CurrentTicks() >= g_nextEvent) 
        { 
            // handle event
            HAL_COMPLETION::DequeueAndExec(); // this also schedules the next one, if there is one
        }
    }
}
