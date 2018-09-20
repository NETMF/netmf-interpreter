////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for the MCBSTM32F400 board (STM32F4): Copyright (c) Oberon microsystems, Inc.
//
//  *** STM32F429IDISCOVERY Board specific IO Port Initialization ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\..\..\..\DeviceCode\Targets\Native\STM32F4\DeviceCode\stm32f4xx.h"

// Define the generic port table, only one generic extensionn port type supported
// and that is the ITM hardware trace port on Channel 0.
extern GenericPortTableEntry const Itm0GenericPort;
extern GenericPortTableEntry const* const g_GenericPorts[TOTAL_GENERIC_PORTS] = { &Itm0GenericPort };

// Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
extern void STM32F4_GPIO_Pin_Config(GPIO_PIN pin, UINT32 mode, GPIO_RESISTOR resistor, UINT32 alternate);


/////////////////////////////////////////////////////////////////////////////
// IS42S16400J 64Mb (1M x 16 x 4 bits)

void __section("SectionForBootstrapOperations") InitSram()
{
    // FMC - 429, vs. FMSC - 40x only

    RCC->AHB3ENR |= RCC_AHB3ENR_FMCEN;

//#if defined (STM32F427_437xx) || defined (STM32F429_439xx)
//#define  RCC_AHB3ENR_FMCEN                  ((uint32_t)0x00000001)
//#endif /* STM32F427_437xx ||  STM32F429_439xx */

    
    /* Setup SRAM control register*/
    //sram_fsmc_bcr1 |= FSMC_BCR1_MWID_0;      /* 16-bit data bus     */
    //sram_fsmc_bcr1 |= FSMC_BCR1_WREN;        /* Write enable        */
    //
    //FSMC_Bank1->BTCR[4] = sram_fsmc_bcr1;
    //
    ///* Setup SRAM timing register*/
    //sram_fsmc_btr1 |= 0x02 << 0;        /* Address setup phase duration    should be >8nS. With the value set to 0x02 and a 168Mhz clock, the duration becomes 11.9nS       */
    //sram_fsmc_btr1 |= 0x01 << 8;        /* Data-phase duration    should be >8nS. With the value set to 0x01 and a 168Mhz clock, the duration becomes 11.9nS                */
    //
    //FSMC_Bank1->BTCR[5] = sram_fsmc_btr1;
    //
    ///* Enable memory bank 3 for SRAM*/
    //FSMC_Bank1->BTCR[4] |= FSMC_BCR1_MBKEN;
}

void __section("SectionForBootstrapOperations") BootstrapCode_GPIO()
{
    // GPIO pins connected to SRAM
    const BYTE pins[] = {
      // PB5, 6
      PORT_PIN(GPIO_PORTB,  5), PORT_PIN(GPIO_PORTB, 6),
      // PC0
      PORT_PIN(GPIO_PORTC,  0),
      // PD0, 1, 8-10, 14, 15
      PORT_PIN(GPIO_PORTD,  0), PORT_PIN(GPIO_PORTD,  1), PORT_PIN(GPIO_PORTD,  8), PORT_PIN(GPIO_PORTD,  9),
      PORT_PIN(GPIO_PORTD, 10), PORT_PIN(GPIO_PORTD, 14), PORT_PIN(GPIO_PORTD, 15),
      // PE0, 1, 7-15
      PORT_PIN(GPIO_PORTE,  0), PORT_PIN(GPIO_PORTE,  1), PORT_PIN(GPIO_PORTE,  7), PORT_PIN(GPIO_PORTE,  8),
      PORT_PIN(GPIO_PORTE,  9), PORT_PIN(GPIO_PORTE, 10), PORT_PIN(GPIO_PORTE, 11), PORT_PIN(GPIO_PORTE, 12),
      PORT_PIN(GPIO_PORTE, 13), PORT_PIN(GPIO_PORTE, 14), PORT_PIN(GPIO_PORTE, 15),
      // PF0-5, 11-15
      PORT_PIN(GPIO_PORTF,  0), PORT_PIN(GPIO_PORTF,  1), PORT_PIN(GPIO_PORTF,  2), PORT_PIN(GPIO_PORTF,  3),
      PORT_PIN(GPIO_PORTF,  4), PORT_PIN(GPIO_PORTF,  5), PORT_PIN(GPIO_PORTF, 11), PORT_PIN(GPIO_PORTF, 12),
      PORT_PIN(GPIO_PORTF, 13), PORT_PIN(GPIO_PORTF, 14), PORT_PIN(GPIO_PORTF, 15),
      // PG0, 1, 4, 5, 8, 15
      PORT_PIN(GPIO_PORTG,  0), PORT_PIN(GPIO_PORTG,  1), PORT_PIN(GPIO_PORTG,  4), PORT_PIN(GPIO_PORTG,  5),
      PORT_PIN(GPIO_PORTG,  8), PORT_PIN(GPIO_PORTG, 15)
    };

    // Enable GPIO clocks for ports A - G
    RCC->AHB1ENR |= RCC_AHB1ENR_GPIOAEN | RCC_AHB1ENR_GPIOBEN | RCC_AHB1ENR_GPIOCEN
                  | RCC_AHB1ENR_GPIODEN | RCC_AHB1ENR_GPIOEEN | RCC_AHB1ENR_GPIOFEN
                  | RCC_AHB1ENR_GPIOGEN;

    // Initialize SRAM pins
    for(int i = 0; i < ARRAYSIZE(pins); i++)
    {
      const uint32_t pinConfig = 0x3C2;    // Speed 100Mhz, AF12 FSMC, Alternate Mode
      const uint32_t pinMode = pinConfig & 0xF;
      const GPIO_ALT_MODE alternateMode = (GPIO_ALT_MODE)pinConfig;
      const GPIO_RESISTOR resistorConfig = RESISTOR_PULLUP;

      // TODO: Why RESISTOR_PULLUP? Why not disabled?

      CPU_GPIO_ReservePin(pins[i], TRUE);
      CPU_GPIO_DisablePin(pins[i], resistorConfig, 0, alternateMode);
      STM32F4_GPIO_Pin_Config(pins[i], pinMode, resistorConfig, pinConfig); // Workaround, since CPU_GPIO_DisablePin() does not correctly initialize pin speeds
    }

    InitSram();

    // TODO: Restore at the end of bootloader?
    CPU_GPIO_EnableOutputPin(LED3, FALSE);
    CPU_GPIO_EnableOutputPin(LED4, FALSE);
}
