////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for MCBSTM32E board (STM32): Copyright (c) Oberon microsystems, Inc.
//
//  *** Board-specific IO Port Initialization ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\..\..\..\DeviceCode\Targets\Native\STM32\DeviceCode\stm32f10x.h"


/* Pin Configuration Register Values
    0x0  Analog Input
    0x1  Output 10Mhz
    0x2  Output  2Mhz
    0x3  Output 50Mhz
    0x4  Floating Input (default)
    0x5  Open-Drain 10Mhz
    0x6  Open-Drain  2Mhz
    0x7  Open-Drain 50Mhz
    0x8  Input with pull up/down
    0x9  Alternate Output 10Mhz
    0xA  Alternate Output  2Mhz
    0xB  Alternate Output 50Mhz
    0xC     -
    0xD  Alternate Open-Drain 10Mhz
    0xE  Alternate Open-Drain  2Mhz
    0xF  Alternate Open-Drain 50Mhz
*/


void __section(SectionForBootstrapOperations) STM3210E_InitSRam() {
/*!< FSMC Bank1 NOR/SRAM3 is used for the STM3210E-EVAL, if another Bank is 
  required, then adjust the Register Addresses */

    /* Enable FSMC clock */
    RCC->AHBENR |= RCC_AHBENR_FSMCEN;
  
    /* Enable GPIOD, GPIOE, GPIOF and GPIOG clocks */  
    RCC->APB2ENR |= RCC_APB2ENR_IOPDEN | RCC_APB2ENR_IOPEEN
                  | RCC_APB2ENR_IOPFEN | RCC_APB2ENR_IOPGEN;
  
    // Configure Ax, Dx, NOE, NWE, NE3, NBLx pins
  
    // pins:      15------8                 7------0
    GPIOD->CRH = 0xBBBBBBBB; GPIOD->CRL = 0x44BB44BB;
    GPIOE->CRH = 0xBBBBBBBB; GPIOE->CRL = 0xBBBBBBBB;  
    GPIOF->CRH = 0xBBBB4444; GPIOF->CRL = 0x44BBBBBB;  
    GPIOG->CRH = 0x444B4B44; GPIOG->CRL = 0x44BBBBBB;  // also enable NE4 (LCD)
   
    // Enable FSMC Bank1_SRAM 3 (mode A, 16 bit)
  
    FSMC_Bank1->BTCR[4] = FSMC_BCR1_EXTMOD | FSMC_BCR1_WREN | FSMC_BCR1_MWID_0 | FSMC_BCR1_MBKEN;
    FSMC_Bank1->BTCR[5] = 0x00000100; // addset = 0+1, dataset = 1+1
    FSMC_Bank1E->BWTR[4] = 0x00000100; // addset = 0+1, dataset = 1+1
#ifdef STM32F10X_XL 
    AFIO->MAPR2 |= AFIO_MAPR2_FSMC_NADV_REMAP; // disconnect NADV
#endif

}


void __section(SectionForBootstrapOperations) BootstrapCode_GPIO() {

    /* Enable GPIOA, GPIOB, and GPIOC clocks */  
    RCC->APB2ENR |= RCC_APB2ENR_IOPAEN | RCC_APB2ENR_IOPBEN | RCC_APB2ENR_IOPCEN;

    // pins:      15------8                 7------0
    GPIOB->ODR = 0; // PB: LEDs off
    GPIOA->CRH = 0x44444442; GPIOA->CRL = 0x44444444; // PA8: backlight enable
    GPIOB->CRH = 0x24222222; GPIOB->CRL = 0x44444444; // PB8-PB13,PB15: LED
    GPIOC->CRH = 0x44444444; GPIOC->CRL = 0x44404444; // PC4: analog input

    STM3210E_InitSRam(); // initialize FSMC & ports D-G
}
