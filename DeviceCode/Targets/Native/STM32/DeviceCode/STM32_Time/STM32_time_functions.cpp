////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  *** System Timer Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\stm32f10x.h"


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


void Timer_Interrupt (void* param) // timer2 compare event
{
    INTERRUPT_START

    TIM2->SR = ~TIM_SR_CC1IF; // reset interrupt flag
    
    if (HAL_Time_CurrentTicks() >= g_nextEvent) { // handle event
       HAL_COMPLETION::DequeueAndExec(); // this also schedules the next one, if there is one
    }

    INTERRUPT_END
}


#pragma arm section code = "SectionForFlashOperations"

UINT64 __section("SectionForFlashOperations") HAL_Time_CurrentTicks()
{
    UINT32 t2, t3, t4; // cascaded timers
    do {
        t3 = TIM3->CNT;
        t4 = TIM4->CNT;
        t2 = TIM2->CNT;
    } while (t3 != TIM3->CNT); // asure consistent values
    return t2 | t3 << 16 | (UINT64)t4 << 32;
}

#pragma arm section code

void HAL_Time_SetCompare( UINT64 CompareValue )
{
    GLOBAL_LOCK(irq);
    g_nextEvent = CompareValue;
    TIM2->CCR1 = (UINT16)CompareValue; // compare to low bits
    
    if (HAL_Time_CurrentTicks() >= CompareValue) { // missed event
        // trigger immediate interrupt
        TIM2->EGR = TIM_EGR_CC1G; // trigger compare1 event
    }

}

#if SYSTEM_APB1_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ
#define TIM2CLK_HZ SYSTEM_APB1_CLOCK_HZ
#else
#define TIM2CLK_HZ (SYSTEM_APB1_CLOCK_HZ * 2)
#endif

BOOL HAL_Time_Initialize()
{
    g_nextEvent = 0xFFFFFFFFFFFF; // never
    
    // enable timer 2-4 clocks
    RCC->APB1ENR |= RCC_APB1ENR_TIM2EN | RCC_APB1ENR_TIM3EN | RCC_APB1ENR_TIM4EN;
    
    // timer2
    TIM2->CR1 = TIM_CR1_URS;
    TIM2->CR2 = TIM_CR2_MMS_1; // master mode selection = update event
    TIM2->SMCR = 0; // TS = 000, SMS = 000, internal clock
    TIM2->DIER = TIM_DIER_CC1IE; // enable compare1 interrupt
    TIM2->CCMR1 = 0; // compare, no outputs
    TIM2->CCMR2 = 0; // compare, no outputs
    TIM2->PSC = (TIM2CLK_HZ / SLOW_CLOCKS_PER_SECOND) - 1; // prescaler to 1MHz
    TIM2->CCR1 = 0;
    TIM2->ARR = 0xFFFF;
    TIM2->EGR = TIM_EGR_UG; // enforce first update
    
    // timer3
    TIM3->CR1 = TIM_CR1_URS;
    TIM3->CR2 = TIM_CR2_MMS_1; // master mode selection = update event
    TIM3->SMCR = TIM_SMCR_TS_0 // TS = 001, clock = timer2 update
                 | TIM_SMCR_SMS_0 | TIM_SMCR_SMS_1 | TIM_SMCR_SMS_2; // SMS = 111
    TIM3->DIER = 0; // no interrupt
    TIM3->PSC = 0;  // no prescaler
    TIM3->ARR = 0xFFFF;
    TIM3->EGR = TIM_EGR_UG; // enforce first update
    
    // timer4
    TIM4->CR1 = TIM_CR1_URS;
    TIM4->CR2 = 0;
    TIM4->SMCR = TIM_SMCR_TS_1 // TS = 010, clock = timer3 update
                 | TIM_SMCR_SMS_0 | TIM_SMCR_SMS_1 | TIM_SMCR_SMS_2; // SMS = 111
    TIM4->DIER = 0; // no interrupt
    TIM4->PSC = 0;  // no prescaler
    TIM4->ARR = 0xFFFF;
    TIM4->EGR = TIM_EGR_UG; // enforce first update
    
    TIM4->CR1 |= TIM_CR1_CEN; // enable timers
    TIM3->CR1 |= TIM_CR1_CEN;
    TIM2->CR1 |= TIM_CR1_CEN;
    
    return CPU_INTC_ActivateInterrupt(TIM2_IRQn, Timer_Interrupt, 0);
}

BOOL HAL_Time_Uninitialize()
{
    CPU_INTC_DeactivateInterrupt(TIM2_IRQn);
    
    TIM4->CR1 &= ~TIM_CR1_CEN; // disable timers
    TIM3->CR1 &= ~TIM_CR1_CEN;
    TIM2->CR1 &= ~TIM_CR1_CEN;
    
    // disable timer 2-4 clocks
    RCC->APB1ENR &= ~(RCC_APB1ENR_TIM2EN | RCC_APB1ENR_TIM3EN | RCC_APB1ENR_TIM4EN);
    
    return TRUE;
}


#pragma arm section code = "SectionForFlashOperations"

//
// To calibrate this constant, uncomment #define CALIBRATE_SLEEP_USEC in TinyHAL.c
//
#define STM32_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS 3

void __section("SectionForFlashOperations") HAL_Time_Sleep_MicroSeconds( UINT32 uSec )
{
    GLOBAL_LOCK(irq);

    UINT32 current   = HAL_Time_CurrentTicks();
    UINT32 maxDiff = CPU_MicrosecondsToTicks( uSec );

    if(maxDiff <= STM32_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS) maxDiff  = 0; 
    else maxDiff -= STM32_SLEEP_USEC_FIXED_OVERHEAD_CLOCKS;

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
