////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for the MCBSTM32F400 board (STM32F4): Copyright (c) Oberon microsystems, Inc.
//
//  *** MCBSTM32F400 Board specific IO Port Initialization ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\..\..\..\DeviceCode\Targets\Native\STM32F4\DeviceCode\stm32f4xx.h"

void __section(SectionForBootstrapOperations) BootstrapCode_GPIO()
{
    /* Enable GPIO clocks */  
    RCC->AHB1ENR |= RCC_AHB1ENR_GPIOAEN | RCC_AHB1ENR_GPIOBEN | RCC_AHB1ENR_GPIOCEN
                  | RCC_AHB1ENR_GPIODEN | RCC_AHB1ENR_GPIOEEN | RCC_AHB1ENR_GPIOFEN
                  | RCC_AHB1ENR_GPIOGEN | RCC_AHB1ENR_GPIOHEN | RCC_AHB1ENR_GPIOIEN;

    CPU_GPIO_EnableOutputPin(LED1, FALSE);
    CPU_GPIO_EnableOutputPin(LED2, FALSE); 
    CPU_GPIO_EnableOutputPin(LED3, FALSE); 
    CPU_GPIO_EnableOutputPin(LED4, FALSE); 
    CPU_GPIO_EnableOutputPin(LED5, FALSE);
    CPU_GPIO_EnableOutputPin(LED6, FALSE);
    CPU_GPIO_EnableOutputPin(LED7, FALSE);
    CPU_GPIO_EnableOutputPin(LED8, FALSE);
}
