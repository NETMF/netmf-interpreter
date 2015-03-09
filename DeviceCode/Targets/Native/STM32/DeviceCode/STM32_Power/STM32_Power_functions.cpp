////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  *** CPU Power States ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\stm32f10x.h"



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

void CPU_Sleep( SLEEP_LEVEL level, UINT64 wakeEvents )
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    
    switch(level) {
        
    case SLEEP_LEVEL__DEEP_SLEEP: // stop
        SCB->SCR |= SCB_SCR_SLEEPDEEP;
        PWR->CR |= PWR_CR_CWUF | PWR_CR_LPDS;   // low power deepsleep
        __WFI(); // stop clocks and wait for external interrupt
        RCC->CR |= RCC_CR_PLLON | RCC_CR_HSEON; // HSE & PLL on
        SCB->SCR &= ~SCB_SCR_SLEEPDEEP;         // reset deepsleep
        while(!(RCC->CR & RCC_CR_PLLRDY));
        RCC->CFGR |= RCC_CFGR_SW_PLL;           // sysclk = pll out
        RCC->CR &= ~RCC_CR_HSION;               // HSI off
        return;
    case SLEEP_LEVEL__OFF: // standby
        SCB->SCR |= SCB_SCR_SLEEPDEEP;
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

__asm void HAL_AssertEx()
{
    BKPT     #0
L1  B        L1
    BX       lr
}

//--//

