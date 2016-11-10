//////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Open Technologies. All rights reserved.
//  Implementation for CMSIS: Copyright (c) Eclo Solutions Lda
//
//  *** CPU Power States ***
//
//////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

extern "C" void SystemClock_Config(void);

void HAL_AssertEx()
{
    __BKPT(0);
    while(true) { ; }
}

/* 
    Cortex-M core features 3 low power modes:
        SLEEP mode: Cortex-M core stopped, peripherals kept running.
        STOP mode: all clocks are stopped, regulator running, regulator in low power mode
        STANDBY mode: 1.2V domain powered off
      
    The NETMF SLEEP_LEVEL enum equivalence is:
        SLEEP_LEVEL__SELECTIVE_OFF => Cortex-M SLEEP mode
        SLEEP_LEVEL__SLEEP         => Cortex-M SLEEP mode
        SLEEP_LEVEL__DEEP_SLEEP    => Cortex-M STOP mode
        SLEEP_LEVEL__OFF           => Cortex-M STANDBY mode
       
*/
void HAL_CPU_Sleep(SLEEP_LEVEL level, UINT64 wakeEvents)
{
    NATIVE_PROFILE_HAL_PROCESSOR_POWER();
    
    switch(level) 
    {
        /////////////////////////////
        case SLEEP_LEVEL__DEEP_SLEEP:
            // Cortex-M STOP mode
            
            // power down flash before entering STOP mode
            HAL_PWREx_EnableFlashPowerDown();
             
            #ifdef FAST_WAKEUP_FROM_STOP             
                // enter STOP mode with main power regulator enabled
                HAL_PWR_EnterSTOPMode(PWR_MAINREGULATOR_ON, PWR_STOPENTRY_WFI);
            #else
                // enter STOP mode with low power regulator enabled
                HAL_PWR_EnterSTOPMode(PWR_LOWPOWERREGULATOR_ON, PWR_STOPENTRY_WFI);
            #endif

            // need to reconfigure system clock after wake-up from STOP mode
            SystemClock_Config();
                                    
            // power up flash after resuming from Stop mode
            HAL_PWREx_DisableFlashPowerDown();
            
            // done here
            break;
                        
        //////////////////////
        case SLEEP_LEVEL__OFF:
            // Cortex-M STANDBY mode
            
            // First disable all used wakeup sources
            // Disable wake-up pin
            HAL_PWR_DisableWakeUpPin(PWR_WAKEUP_PIN1);

            // Clear PWR wake-up flag
            __HAL_PWR_CLEAR_FLAG(PWR_FLAG_WU);

            // Enable wake-up pin (WKUP) 
            HAL_PWR_EnableWakeUpPin(PWR_WAKEUP_PIN1);
       
            // actually enter STANDBY mode
            HAL_PWR_EnterSTANDBYMode();

            // never reachs here, but we need to keep the compiler happy...
            break;
         
        ///////////////////////////////               
        default:
            // all other modes revert to Cortex-M SLEEP mode

            // enter SLEEP mode
            HAL_PWR_EnterSLEEPMode(PWR_MAINREGULATOR_ON, PWR_SLEEPENTRY_WFI); 

            // done here
            break;                         
    }    
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
