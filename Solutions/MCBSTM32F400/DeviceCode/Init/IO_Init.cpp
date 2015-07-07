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

#define ARRAY_LENGTH(x)    (sizeof(x)/sizeof(0[x]))

#include <tinyhal.h>
#include "..\..\..\..\DeviceCode\Targets\Native\STM32F4\DeviceCode\stm32f4xx.h"

// define the generic port table, only one generic extensionn port type supported
// and that is the ITM hardware trace port on Channel 0.
extern GenericPortTableEntry const Itm0GenericPort;
extern GenericPortTableEntry const* const g_GenericPorts[ TOTAL_GENERIC_PORTS ] = { &Itm0GenericPort };

extern void STM32F4_GPIO_Pin_Config( GPIO_PIN pin, UINT32 mode, GPIO_RESISTOR resistor, UINT32 alternate );    // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds

void __section("SectionForBootstrapOperations") InitNorFlash()
{
    uint32_t nor_fsmc_bcr1 = 0x00000000;
    uint32_t nor_fsmc_btr1 = 0x00000000;    
    
    /* Enable the FSMC clock */
    RCC->AHB3ENR |= (RCC_AHB3ENR_FSMCEN);
    
    /* Setup NOR flash control register - See page 1531 of the STM32F4XX reference manual */
    nor_fsmc_bcr1 |= FSMC_BCR1_MTYP_1;        /* NOR Memory          */
    nor_fsmc_bcr1 |= FSMC_BCR1_MWID_0;        /* 16-bit data bus     */
    nor_fsmc_bcr1 |= FSMC_BCR1_FACCEN;        /* Flash access enable */
    nor_fsmc_bcr1 |= FSMC_BCR1_WREN;          /* Write enable        */
    nor_fsmc_bcr1 |= FSMC_BCR1_ASYNCWAIT;     /* Async wait enable   */
    
    FSMC_Bank1->BTCR[0] = nor_fsmc_bcr1;
    
    /* Setup NOR flash timing register - See page 1531 of the STM32F4XX reference manual*/
    nor_fsmc_btr1 |= 0x0C << 0;        /* Address setup phase duration    should be >70ns. With the value set to 0x0C and a 168Mhz clock, the duration becomes 71.4nS       */
    nor_fsmc_btr1 |= 0x08 << 8;        /* Data-phase duration should be >45nS. With the value set of 0x08 and a 168Mhz clock, the duration becomes 47.6nS                   */
    nor_fsmc_btr1 |= 0x05 << 16;    /* Bus turnaround duration + 1 HCLK should be >30nS. With the value set to 0x05 and a 168Mhz clock, the duration becomes 35.7nS   */
    
    FSMC_Bank1->BTCR[1] = nor_fsmc_btr1;
    
    /* Enable memory bank 1 for NOR flash */
    FSMC_Bank1->BTCR[0] |= FSMC_BCR1_MBKEN;
}

void __section("SectionForBootstrapOperations") InitSram()
{
    uint32_t sram_fsmc_bcr1 = 0x00000000;
    uint32_t sram_fsmc_btr1 = 0x00000000;
    
    /* Enable the FSMC clock */
    RCC->AHB3ENR |= (RCC_AHB3ENR_FSMCEN);
    
    /* Setup SRAM control register*/
    sram_fsmc_bcr1 |= FSMC_BCR1_MWID_0;      /* 16-bit data bus     */
    sram_fsmc_bcr1 |= FSMC_BCR1_WREN;        /* Write enable        */
    
    FSMC_Bank1->BTCR[4] = sram_fsmc_bcr1;
    
    /* Setup SRAM timing register*/
    sram_fsmc_btr1 |= 0x02 << 0;        /* Address setup phase duration    should be >8nS. With the value set to 0x02 and a 168Mhz clock, the duration becomes 11.9nS       */
    sram_fsmc_btr1 |= 0x01 << 8;        /* Data-phase duration    should be >8nS. With the value set to 0x01 and a 168Mhz clock, the duration becomes 11.9nS                */
    
    FSMC_Bank1->BTCR[5] = sram_fsmc_btr1;
    
    /* Enable memory bank 3 for SRAM*/
    FSMC_Bank1->BTCR[4] |= FSMC_BCR1_MBKEN;
}

void __section("SectionForBootstrapOperations") BootstrapCode_GPIO()
{
    /* GPIO pins connected to NOR Flash and SRAM on the MCBSTM32F400 board */
    const uint8_t PortD_PinList[] = {0, 1, 4, 5, 6, 7, 8, 9, 10, 11, 12 ,13, 14, 15};
#ifdef DEBUG
    // PE2,3,4,5 are used for TRACECLK and TRACEDATA0-3 so don't enable them as address pins in debug builds
    // This limits external FLASH and SRAM to 1MB addressable space each.
    const uint8_t PortE_PinList[] = {0, 1, /*2, 3, 4, 5,*/ 7, 8, 9, 10, 11, 12, 13, 14, 15};
#else
    const uint8_t PortE_PinList[] = {0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15};
#endif
    const uint8_t PortF_PinList[] = {0, 1, 2, 3, 4, 5, 12, 13, 14, 15};
    const uint8_t PortG_PinList[] = {0, 1, 2, 3, 4, 5, 10};
    
    const uint32_t pinConfig = 0x3C2;    // Speed 100Mhz, AF12 FSMC, Alternate Mode
    const uint32_t pinMode = pinConfig & 0xF;
    const GPIO_ALT_MODE alternateMode = (GPIO_ALT_MODE) pinConfig;
    const GPIO_RESISTOR resistorConfig = RESISTOR_PULLUP;
    
    uint32_t i;

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
    
    /*Initialize SRAM and NOR GPIOs */
    for(i = 0; i < ARRAY_LENGTH(PortD_PinList); i++)    /* Port D */
    {
        CPU_GPIO_ReservePin( PORT_PIN(GPIO_PORTD, PortD_PinList[i]), TRUE);
        CPU_GPIO_DisablePin( PORT_PIN(GPIO_PORTD, PortD_PinList[i]),  resistorConfig, 0, alternateMode);
        STM32F4_GPIO_Pin_Config( PORT_PIN(GPIO_PORTD, PortD_PinList[i]), pinMode, resistorConfig, pinConfig ); // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
    }
    
    for(i = 0; i < ARRAY_LENGTH(PortE_PinList); i++)    /* Port E */
    {
        CPU_GPIO_ReservePin( PORT_PIN(GPIO_PORTE, PortE_PinList[i]), TRUE);
        CPU_GPIO_DisablePin( PORT_PIN(GPIO_PORTE, PortE_PinList[i]),  resistorConfig, 0, alternateMode);
        STM32F4_GPIO_Pin_Config( PORT_PIN(GPIO_PORTE, PortE_PinList[i]), pinMode, resistorConfig, pinConfig ); // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
    }
    
    for(i = 0; i < ARRAY_LENGTH(PortF_PinList); i++)    /* Port F */
    {
        CPU_GPIO_ReservePin( PORT_PIN(GPIO_PORTF, PortF_PinList[i]), TRUE);
        CPU_GPIO_DisablePin( PORT_PIN(GPIO_PORTF, PortF_PinList[i]),  resistorConfig, 0, alternateMode);
        STM32F4_GPIO_Pin_Config( PORT_PIN(GPIO_PORTF, PortF_PinList[i]), pinMode, resistorConfig, pinConfig ); // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
    }
    
    for(i = 0; i < ARRAY_LENGTH(PortG_PinList); i++)    /* Port G */
    {
        CPU_GPIO_ReservePin( PORT_PIN(GPIO_PORTG, PortG_PinList[i]), TRUE);
        CPU_GPIO_DisablePin( PORT_PIN(GPIO_PORTG, PortG_PinList[i]),  resistorConfig, 0, alternateMode);
        STM32F4_GPIO_Pin_Config( PORT_PIN(GPIO_PORTG, PortG_PinList[i]), pinMode, resistorConfig, pinConfig ); // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
    }
    
    /* Initialize NOR and SRAM */
    InitNorFlash();
    InitSram();
}
