////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** System Timer Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif


#if STM32F4_32B_TIMER == 2
    #define TIM_32 TIM2
    #define RCC_APB1ENR_TIM_32_EN RCC_APB1ENR_TIM2EN
    #define DBGMCU_APB1_FZ_DBG_TIM_32_STOP DBGMCU_APB1_FZ_DBG_TIM2_STOP
    #define TIM_32_IRQn TIM2_IRQn
    #if STM32F4_16B_TIMER == 3
        #define TIM_16 TIM3
        #define RCC_APB1ENR_TIM_16_EN RCC_APB1ENR_TIM3EN
        #define TIM_SMCR_TS_BITS (TIM_SMCR_TS_0 * 1) // TS = 001
    #elif STM32F4_16B_TIMER == 4
        #define TIM_16 TIM4
        #define RCC_APB1ENR_TIM_16_EN RCC_APB1ENR_TIM4EN
        #define TIM_SMCR_TS_BITS (TIM_SMCR_TS_0 * 1) // TS = 001
    #elif STM32F4_16B_TIMER == 8
        #define TIM_16 TIM8
        #define RCC_APB2ENR_TIM_16_EN RCC_APB2ENR_TIM8EN
        #define TIM_SMCR_TS_BITS (TIM_SMCR_TS_0 * 1) // TS = 001
    #elif STM32F4_16B_TIMER == 9
        #define TIM_16 TIM9
        #define RCC_APB2ENR_TIM_16_EN RCC_APB2ENR_TIM9EN
        #define TIM_SMCR_TS_BITS (TIM_SMCR_TS_0 * 0) // TS = 000
    #else
        #error wrong 16 bit timer (3, 4, 8 or 9)
    #endif
#elif STM32F4_32B_TIMER == 5
    #define TIM_32 TIM5
    #define RCC_APB1ENR_TIM_32_EN RCC_APB1ENR_TIM5EN
    #define DBGMCU_APB1_FZ_DBG_TIM_32_STOP DBGMCU_APB1_FZ_DBG_TIM5_STOP
    #define TIM_32_IRQn TIM5_IRQn
    #if STM32F4_16B_TIMER == 1
        #define TIM_16 TIM1
        #define RCC_APB2ENR_TIM_16_EN RCC_APB2ENR_TIM1EN
        #define TIM_SMCR_TS_BITS (TIM_SMCR_TS_0 * 0) // TS = 000
    #elif STM32F4_16B_TIMER == 3
        #define TIM_16 TIM3
        #define RCC_APB1ENR_TIM_16_EN RCC_APB1ENR_TIM3EN
        #define TIM_SMCR_TS_BITS (TIM_SMCR_TS_0 * 2) // TS = 010
    #elif STM32F4_16B_TIMER == 8
        #define TIM_16 TIM8
        #define RCC_APB2ENR_TIM_16_EN RCC_APB2ENR_TIM8EN
        #define TIM_SMCR_TS_BITS (TIM_SMCR_TS_0 * 3) // TS = 011
    #elif STM32F4_16B_TIMER == 12
        #define TIM_16 TIM12
        #define RCC_APB1ENR_TIM_16_EN RCC_APB1ENR_TIM12EN
        #define TIM_SMCR_TS_BITS (TIM_SMCR_TS_0 * 1) // TS = 001
    #else
        #error wrong 16 bit timer (1, 3, 8 or 12)
    #endif
#else
    #error wrong 32 bit timer (2 or 5)
#endif 

// 32 bit timer on APB1
#if SYSTEM_APB1_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ
#define TIM_CLK_HZ SYSTEM_APB1_CLOCK_HZ
#else
#define TIM_CLK_HZ (SYSTEM_APB1_CLOCK_HZ * 2)
#endif


static UINT64 g_nextEvent;   // tick time of next event to be scheduled


UINT32 CPU_SystemClock()
{
    return SYSTEM_CLOCK_HZ;
}

UINT32 CPU_TicksPerSecond()
{
    return SLOW_CLOCKS_PER_SECOND;
}

//--//

UINT64 CPU_TicksToTime( UINT64 Ticks )
{
    Ticks *= (TEN_MHZ               /SLOW_CLOCKS_TEN_MHZ_GCD);
    Ticks /= (SLOW_CLOCKS_PER_SECOND/SLOW_CLOCKS_TEN_MHZ_GCD);

    return Ticks;
}

UINT64 CPU_TicksToTime( UINT32 Ticks32 )
{
    UINT64 Ticks;

    Ticks  = (UINT64)Ticks32 * (TEN_MHZ               /SLOW_CLOCKS_TEN_MHZ_GCD);
    Ticks /=                   (SLOW_CLOCKS_PER_SECOND/SLOW_CLOCKS_TEN_MHZ_GCD);

    return Ticks;
}

//--//

UINT64 CPU_MillisecondsToTicks( UINT64 Ticks )
{
    Ticks *= (SLOW_CLOCKS_PER_SECOND/SLOW_CLOCKS_MILLISECOND_GCD);
    Ticks /= (1000                  /SLOW_CLOCKS_MILLISECOND_GCD);

    return Ticks;
}

UINT64 CPU_MillisecondsToTicks( UINT32 Ticks32 )
{
    UINT64 Ticks;

    Ticks  = (UINT64)Ticks32 * (SLOW_CLOCKS_PER_SECOND/SLOW_CLOCKS_MILLISECOND_GCD);
    Ticks /=                   (1000                  /SLOW_CLOCKS_MILLISECOND_GCD);

    return Ticks;
}

//--//

#pragma arm section code = "SectionForFlashOperations"

UINT64 __section("SectionForFlashOperations") CPU_MicrosecondsToTicks( UINT64 uSec )
{
#if ONE_MHZ <= SLOW_CLOCKS_PER_SECOND
    return uSec * (SLOW_CLOCKS_PER_SECOND / ONE_MHZ);
#else
    return uSec / (ONE_MHZ / SLOW_CLOCKS_PER_SECOND);
#endif
}

UINT32 __section("SectionForFlashOperations") CPU_MicrosecondsToTicks( UINT32 uSec )
{
#if ONE_MHZ <= SLOW_CLOCKS_PER_SECOND
    return uSec * (SLOW_CLOCKS_PER_SECOND / ONE_MHZ);
#else
    return uSec / (ONE_MHZ / SLOW_CLOCKS_PER_SECOND);
#endif
}

#pragma arm section code

//--//

UINT32 CPU_MicrosecondsToSystemClocks( UINT32 uSec )
{
    uSec *= (SYSTEM_CLOCK_HZ/CLOCK_COMMON_FACTOR);
    uSec /= (ONE_MHZ        /CLOCK_COMMON_FACTOR);

    return uSec;
}

int CPU_MicrosecondsToSystemClocks( int uSec )
{
    uSec *= (SYSTEM_CLOCK_HZ/CLOCK_COMMON_FACTOR);
    uSec /= (ONE_MHZ        /CLOCK_COMMON_FACTOR);

    return uSec;
}

//--//

int CPU_SystemClocksToMicroseconds( int Ticks )
{
    Ticks *= (ONE_MHZ        /CLOCK_COMMON_FACTOR);
    Ticks /= (SYSTEM_CLOCK_HZ/CLOCK_COMMON_FACTOR);

    return Ticks;
}

//--//


void Timer_Interrupt (void* param) // 32 bit timer compare event
{
    INTERRUPT_START

    TIM_32->SR = ~TIM_SR_CC1IF; // reset interrupt flag
    
    if (HAL_Time_CurrentTicks() >= g_nextEvent) { // handle event
       HAL_COMPLETION::DequeueAndExec(); // this also schedules the next one, if there is one
    }

    INTERRUPT_END
}


#pragma arm section code = "SectionForFlashOperations"

UINT64 __section("SectionForFlashOperations") HAL_Time_CurrentTicks()
{
    UINT32 t2, t3; // cascaded timers
    do {
        t3 = TIM_16->CNT;
        t2 = TIM_32->CNT;
    } while (t3 != TIM_16->CNT); // asure consistent values
    return t2 | (UINT64)t3 << 32;
}

#pragma arm section code

void HAL_Time_SetCompare( UINT64 CompareValue )
{
    GLOBAL_LOCK(irq);
    g_nextEvent = CompareValue;
    TIM_32->CCR1 = (UINT32)CompareValue; // compare to low bits
    
    if (HAL_Time_CurrentTicks() >= CompareValue) { // missed event
        // trigger immediate interrupt
        TIM_32->EGR = TIM_EGR_CC1G; // trigger compare1 event
    }

}

BOOL HAL_Time_Initialize()
{
    g_nextEvent = 0xFFFFFFFFFFFF; // never
    
    // enable timer clocks
#ifdef RCC_APB1ENR_TIM_16_EN
    RCC->APB1ENR |= RCC_APB1ENR_TIM_32_EN | RCC_APB1ENR_TIM_16_EN;
#else
    RCC->APB1ENR |= RCC_APB1ENR_TIM_32_EN;
    RCC->APB2ENR |= RCC_APB2ENR_TIM_16_EN;
#endif

    // configure jtag debug support
    DBGMCU->APB1FZ |= DBGMCU_APB1_FZ_DBG_TIM_32_STOP;
    
    // 32 bit timer (lower bits)
    TIM_32->CR1 = TIM_CR1_URS;
    TIM_32->CR2 = TIM_CR2_MMS_1; // master mode selection = update event
    TIM_32->SMCR = 0; // TS = 000, SMS = 000, internal clock
    TIM_32->DIER = TIM_DIER_CC1IE; // enable compare1 interrupt
    TIM_32->CCMR1 = 0; // compare, no outputs
    TIM_32->CCMR2 = 0; // compare, no outputs
    TIM_32->PSC = (TIM_CLK_HZ / SLOW_CLOCKS_PER_SECOND) - 1; // prescaler to 1MHz
    TIM_32->CCR1 = 0;
    TIM_32->ARR = 0xFFFFFFFF; // 32 bit counter
    TIM_32->EGR = TIM_EGR_UG; // enforce first update

    // 16 bit timer (upper bits)
    TIM_16->CR1 = TIM_CR1_URS;
    TIM_16->CR2 = 0;
    TIM_16->SMCR = TIM_SMCR_TS_BITS // clock = trigger = timer_32 update
                 | TIM_SMCR_SMS_0 | TIM_SMCR_SMS_1 | TIM_SMCR_SMS_2; // SMS = 111
    TIM_16->DIER = 0; // no interrupt
    TIM_16->PSC = 0;  // no prescaler
    TIM_16->ARR = 0xFFFF; // 16 bit counter
    TIM_16->EGR = TIM_EGR_UG; // enforce first update
    
    TIM_16->CR1 |= TIM_CR1_CEN; // enable timers
    TIM_32->CR1 |= TIM_CR1_CEN;
    
    return CPU_INTC_ActivateInterrupt(TIM_32_IRQn, Timer_Interrupt, 0);
}

BOOL HAL_Time_Uninitialize()
{
    CPU_INTC_DeactivateInterrupt(TIM_32_IRQn);
    
    TIM_16->CR1 &= ~TIM_CR1_CEN; // disable timers
    TIM_32->CR1 &= ~TIM_CR1_CEN;
    
    // disable timer clocks
#ifdef RCC_APB1ENR_TIM_16_EN
    RCC->APB1ENR &= ~(RCC_APB1ENR_TIM_32_EN | RCC_APB1ENR_TIM_16_EN);
#else
    RCC->APB1ENR &= ~RCC_APB1ENR_TIM_32_EN;
    RCC->APB2ENR &= ~RCC_APB2ENR_TIM_16_EN;
#endif
    
    return TRUE;
}


#pragma arm section code = "SectionForFlashOperations"

//
// To calibrate this constant, uncomment #define CALIBRATE_SLEEP_USEC in TinyHAL.c
//
#define STM32F4_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS 3

void __section("SectionForFlashOperations") HAL_Time_Sleep_MicroSeconds( UINT32 uSec )
{
    GLOBAL_LOCK(irq);

    UINT32 current   = HAL_Time_CurrentTicks();
    UINT32 maxDiff = CPU_MicrosecondsToTicks( uSec );

    if(maxDiff <= STM32F4_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS) maxDiff  = 0; 
    else maxDiff -= STM32F4_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS;

    while(((INT32)(HAL_Time_CurrentTicks() - current)) <= maxDiff);
}

void HAL_Time_Sleep_MicroSeconds_InterruptEnabled( UINT32 uSec )
{
    // iterations must be signed so that negative iterations will result in the minimum delay

    uSec *= (SYSTEM_CYCLE_CLOCK_HZ / CLOCK_COMMON_FACTOR);
    uSec /= (ONE_MHZ               / CLOCK_COMMON_FACTOR);

    // iterations is equal to the number of CPU instruction cycles in the required time minus
    // overhead cycles required to call this subroutine.
    int iterations = (int)uSec - 5;      // Subtract off call & calculation overhead

    CYCLE_DELAY_LOOP(iterations);
}

#pragma arm section code

INT64 HAL_Time_TicksToTime( UINT64 Ticks )
{
    return CPU_TicksToTime( Ticks );
}

INT64 HAL_Time_CurrentTime()
{
    return CPU_TicksToTime( HAL_Time_CurrentTicks() );
}

void HAL_Time_GetDriftParameters  ( INT32* a, INT32* b, INT64* c )
{
    *a = 1;
    *b = 1;
    *c = 0;
}


//******************** Profiler ********************

UINT64 Time_CurrentTicks()
{
    return HAL_Time_CurrentTicks();
}


