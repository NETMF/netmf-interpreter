////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for the MCBSTM32F400 board (STM32F4): Copyright (c) Oberon microsystems, Inc.
//
//  *** STM32F4DISCOVERY Board specific IO Port Initialization ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define ARRAY_LENGTH(x)    (sizeof(x)/sizeof(0[x]))

#include <tinyhal.h>
#include "..\..\..\..\DeviceCode\Targets\Native\STM32F4\DeviceCode\stm32f4xx.h"

extern void STM32F4_GPIO_Pin_Config( GPIO_PIN pin, UINT32 mode, GPIO_RESISTOR resistor, UINT32 alternate );    // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds

void __section("SectionForBootstrapOperations") BootstrapCode_GPIO()
{
//#ifdef DEBUG
//    // PE2,3,4,5 are used for TRACECLK and TRACEDATA0-3 so don't enable them as address pins in debug builds
//    // This limits external FLASH and SRAM to 1MB addressable space each.
//    const uint8_t PortE_PinList[] = {0, 1, /*2, 3, 4, 5,*/ 7, 8, 9, 10, 11, 12, 13, 14, 15};
//#else
//    const uint8_t PortE_PinList[] = {0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15};
//#endif
//    const uint8_t PortF_PinList[] = {0, 1, 2, 3, 4, 5, 12, 13, 14, 15};
//    const uint8_t PortG_PinList[] = {0, 1, 2, 3, 4, 5, 10};
//    
//    const uint32_t pinConfig = 0x3C2;    // Speed 100Mhz, AF12 FSMC, Alternate Mode
//    const uint32_t pinMode = pinConfig & 0xF;
//    const GPIO_ALT_MODE alternateMode = (GPIO_ALT_MODE) pinConfig;
//    const GPIO_RESISTOR resistorConfig = RESISTOR_PULLUP;
//
//    uint32_t i;

    /* Enable GPIO clocks */  
    RCC->AHB1ENR |= RCC_AHB1ENR_GPIOAEN | RCC_AHB1ENR_GPIOBEN | RCC_AHB1ENR_GPIOCEN
                  | RCC_AHB1ENR_GPIODEN | RCC_AHB1ENR_GPIOEEN | RCC_AHB1ENR_GPIOFEN
                  | RCC_AHB1ENR_GPIOGEN | RCC_AHB1ENR_GPIOHEN | RCC_AHB1ENR_GPIOIEN;

    CPU_GPIO_EnableOutputPin(LED3, FALSE);
    CPU_GPIO_EnableOutputPin(LED4, FALSE);
    CPU_GPIO_EnableOutputPin(LED5, FALSE);
    CPU_GPIO_EnableOutputPin(LED6, FALSE);

    /*Initialize SRAM and NOR GPIOs */

    //for(i = 0; i < ARRAY_LENGTH(PortE_PinList); i++)    /* Port E */
    //{
    //    CPU_GPIO_ReservePin( PORT_PIN(GPIO_PORTE, PortE_PinList[i]), TRUE);
    //    CPU_GPIO_DisablePin( PORT_PIN(GPIO_PORTE, PortE_PinList[i]),  resistorConfig, 0, alternateMode);
    //    STM32F4_GPIO_Pin_Config( PORT_PIN(GPIO_PORTE, PortE_PinList[i]), pinMode, resistorConfig, pinConfig ); // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
    //}
    //
    //for(i = 0; i < ARRAY_LENGTH(PortF_PinList); i++)    /* Port F */
    //{
    //    CPU_GPIO_ReservePin( PORT_PIN(GPIO_PORTF, PortF_PinList[i]), TRUE);
    //    CPU_GPIO_DisablePin( PORT_PIN(GPIO_PORTF, PortF_PinList[i]),  resistorConfig, 0, alternateMode);
    //    STM32F4_GPIO_Pin_Config( PORT_PIN(GPIO_PORTF, PortF_PinList[i]), pinMode, resistorConfig, pinConfig ); // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
    //}
    //
    //for(i = 0; i < ARRAY_LENGTH(PortG_PinList); i++)    /* Port G */
    //{
    //    CPU_GPIO_ReservePin( PORT_PIN(GPIO_PORTG, PortG_PinList[i]), TRUE);
    //    CPU_GPIO_DisablePin( PORT_PIN(GPIO_PORTG, PortG_PinList[i]),  resistorConfig, 0, alternateMode);
    //    STM32F4_GPIO_Pin_Config( PORT_PIN(GPIO_PORTG, PortG_PinList[i]), pinMode, resistorConfig, pinConfig ); // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
    //}
}
