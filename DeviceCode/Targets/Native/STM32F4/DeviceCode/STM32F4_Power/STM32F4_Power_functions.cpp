//////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Open Technologies. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** CPU Power States ***
//
//////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "STM32F4_Power_functions.h"

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif


static void (*g_STM32F4_stopHandler)();
static void (*g_STM32F4_restartHandler)();


void STM32F4_SetPowerHandlers(void (*stop)(), void (*restart)())
{
    g_STM32F4_stopHandler = stop;
    g_STM32F4_restartHandler = restart;
}


BOOL CPU_Initialize()
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    CPU_INTC_Initialize();
    return TRUE;
}

void CPU_ChangePowerLevel(POWER_LEVEL level)
{
    switch(level)
    {
        case POWER_LEVEL__MID_POWER:
            break;

        case POWER_LEVEL__LOW_POWER:
            break;

        case POWER_LEVEL__HIGH_POWER:
        default:
            break;
    }
}

void HAL_CPU_Sleep( SLEEP_LEVEL level, UINT64 wakeEvents )
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    
    switch(level) {
        
    case SLEEP_LEVEL__DEEP_SLEEP: // stop
        // stop peripherals if needed
        if (g_STM32F4_stopHandler != NULL)
            g_STM32F4_stopHandler();
        SCB->SCR |= SCB_SCR_SLEEPDEEP_Msk;
        PWR->CR |= PWR_CR_CWUF | PWR_CR_FPDS | PWR_CR_LPDS; // low power deepsleep
        __WFI(); // stop clocks and wait for external interrupt
#if SYSTEM_CRYSTAL_CLOCK_HZ != 0
        RCC->CR |= RCC_CR_HSEON;             // HSE on
#endif
        SCB->SCR &= ~SCB_SCR_SLEEPDEEP_Msk;  // reset deepsleep
        while(!(RCC->CR & RCC_CR_HSERDY));
        RCC->CR |= RCC_CR_PLLON;             // pll on
        while(!(RCC->CR & RCC_CR_PLLRDY));
        RCC->CFGR |= RCC_CFGR_SW_PLL;        // sysclk = pll out
#if SYSTEM_CRYSTAL_CLOCK_HZ != 0
		RCC->CR &= ~RCC_CR_HSION;            // HSI off
#endif
        // restart peripherals if needed
        if (g_STM32F4_restartHandler != NULL)
            g_STM32F4_restartHandler();
        return;
    case SLEEP_LEVEL__OFF: // standby
        // stop peripherals if needed
        if (g_STM32F4_stopHandler != NULL)
            g_STM32F4_stopHandler();
        SCB->SCR |= SCB_SCR_SLEEPDEEP_Msk;
        PWR->CR |= PWR_CR_CWUF | PWR_CR_PDDS; // power down deepsleep
        __WFI(); // soft power off, never returns
        return;            
    default: // sleep
        PWR->CR |= PWR_CR_CWUF;
        __WFI(); // sleep and wait for interrupt
        return;
    }
}

void CPU_Halt()  // unrecoverable error
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    while(1);
}

void CPU_Reset()
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    SCB->AIRCR = (0x5FA << SCB_AIRCR_VECTKEY_Pos)  // unlock key
               | (1 << SCB_AIRCR_SYSRESETREQ_Pos); // reset request
     while(1); // wait for reset
}

BOOL CPU_IsSoftRebootSupported ()
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    return TRUE;
}

void HAL_AssertEx()
{
    __BKPT(0);
    while(true) { /*nop*/ }
}

