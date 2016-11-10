//////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Open Technologies. All rights reserved.
//  Implementation for CMSIS: Copyright (c) Eclo Solutions Lda
//
//  *** CPU Power States for bootloader (with only the minimal possible implementation ***
//
//////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

void HAL_AssertEx()
{
    __BKPT(0);
    while(true) { ; }
}

void HAL_CPU_Sleep(SLEEP_LEVEL level, UINT64 wakeEvents)
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    
    // enter SLEEP mode
    HAL_PWR_EnterSLEEPMode(PWR_MAINREGULATOR_ON, PWR_SLEEPENTRY_WFI); 
}

//--//

BOOL CPU_Initialize()
{
    // nothing to do here...
    // CMSIS HAL_Init() should be called from main and it takes care of everything required to init the CPU and modules 
    return TRUE;
}

void CPU_Reset()
{
    NVIC_SystemReset();
}

void CPU_Sleep(SLEEP_LEVEL level, UINT64 wakeEvents)
{
    HAL_CPU_Sleep(level, wakeEvents);
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

BOOL CPU_IsSoftRebootSupported ()
{
    return FALSE;
}

void CPU_Halt()
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    while(1);    
}
