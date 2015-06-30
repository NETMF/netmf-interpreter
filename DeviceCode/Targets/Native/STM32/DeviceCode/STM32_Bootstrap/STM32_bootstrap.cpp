////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  *** Bootstrap ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\stm32f10x.h"

///////////////////////////////////////////////////////////////////////////////


/* STM32 clock configuration */

#if SYSTEM_CLOCK_HZ % SYSTEM_CRYSTAL_CLOCK_HZ != 0
#error SYSTEM_CLOCK_HZ must be a multiple of SYSTEM_CRYSTAL_CLOCK_HZ
#endif
#define RCC_CFGR_PLLMULL_FACT (SYSTEM_CLOCK_HZ / SYSTEM_CRYSTAL_CLOCK_HZ)
#if RCC_CFGR_PLLMULL_FACT < 2 || RCC_CFGR_PLLMULL_FACT > 16
#error SYSTEM_CLOCK_HZ must be between SYSTEM_CRYSTAL_CLOCK_HZ * 2 and SYSTEM_CRYSTAL_CLOCK_HZ * 16
#endif
#define RCC_CFGR_PLLMULL_BITS ((RCC_CFGR_PLLMULL_FACT - 2) * RCC_CFGR_PLLMULL3)

#if SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 1
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV1
#elif SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 2
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV2
#elif SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 4
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV4
#elif SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 8
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV8
#elif SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 16
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV16
#elif SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 64
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV64
#elif SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 128
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV128
#elif SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 256
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV256
#elif SYSTEM_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ * 512
#define RCC_CFGR_HPRE_DIV_BITS RCC_CFGR_HPRE_DIV512
#else
#error SYSTEM_CLOCK_HZ must be SYSTEM_CYCLE_CLOCK_HZ * 1, 2, 4, 8, .. 256, or 512
#endif

#if SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB1_CLOCK_HZ * 1
#define RCC_CFGR_PPRE1_DIV_BITS RCC_CFGR_PPRE1_DIV1
#elif SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB1_CLOCK_HZ * 2
#define RCC_CFGR_PPRE1_DIV_BITS RCC_CFGR_PPRE1_DIV2
#elif SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB1_CLOCK_HZ * 4
#define RCC_CFGR_PPRE1_DIV_BITS RCC_CFGR_PPRE1_DIV4
#elif SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB1_CLOCK_HZ * 8
#define RCC_CFGR_PPRE1_DIV_BITS RCC_CFGR_PPRE1_DIV8
#elif SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB1_CLOCK_HZ * 16
#define RCC_CFGR_PPRE1_DIV_BITS RCC_CFGR_PPRE1_DIV16
#else
#error SYSTEM_CYCLE_CLOCK_HZ must be SYSTEM_APB1_CLOCK_HZ * 1, 2, 4, 8, or 16
#endif

#if SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB2_CLOCK_HZ * 1
#define RCC_CFGR_PPRE2_DIV_BITS RCC_CFGR_PPRE2_DIV1
#elif SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB2_CLOCK_HZ * 2
#define RCC_CFGR_PPRE2_DIV_BITS RCC_CFGR_PPRE2_DIV2
#elif SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB2_CLOCK_HZ * 4
#define RCC_CFGR_PPRE2_DIV_BITS RCC_CFGR_PPRE2_DIV4
#elif SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB2_CLOCK_HZ * 8
#define RCC_CFGR_PPRE2_DIV_BITS RCC_CFGR_PPRE2_DIV8
#elif SYSTEM_CYCLE_CLOCK_HZ == SYSTEM_APB2_CLOCK_HZ * 16
#define RCC_CFGR_PPRE2_DIV_BITS RCC_CFGR_PPRE2_DIV16
#else
#error SYSTEM_CYCLE_CLOCK_HZ must be SYSTEM_APB2_CLOCK_HZ * 1, 2, 4, 8, or 16
#endif

#define RCC_CFGR_ADC_MAX 14000000
#if SYSTEM_APB2_CLOCK_HZ <= RCC_CFGR_ADC_MAX * 2
#define RCC_CFGR_ADC_BITS RCC_CFGR_ADCPRE_DIV2
#elif SYSTEM_APB2_CLOCK_HZ <= RCC_CFGR_ADC_MAX * 4
#define RCC_CFGR_ADC_BITS RCC_CFGR_ADCPRE_DIV4
#elif SYSTEM_APB2_CLOCK_HZ <= RCC_CFGR_ADC_MAX * 6
#define RCC_CFGR_ADC_BITS RCC_CFGR_ADCPRE_DIV6
#else
#define RCC_CFGR_ADC_BITS RCC_CFGR_ADCPRE_DIV8
#endif

#if SYSTEM_CLOCK_HZ <= 24000000
#define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_0 // no wait states
#define RCC_CFGR_USBPRE_BIT RCC_CFGR_USBPRE        // USB clk = sysclk
#elif SYSTEM_CLOCK_HZ <= 48000000
#define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_1 // one wait state
#define RCC_CFGR_USBPRE_BIT RCC_CFGR_USBPRE        // USB clk = sysclk
#else
#define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_2 // two wait states
#define RCC_CFGR_USBPRE_BIT 0                      // USB clk = sysclk * 2/3
#endif


#pragma arm section code = "SectionForBootstrapOperations"


/* IO initialization implemented in solution DeviceCode\Init */
void BootstrapCode_GPIO();


extern "C"
{
void __section("SectionForBootstrapOperations") STM32_BootstrapCode()
{
    // assure interupts off
    __disable_irq();
    
    // allow unaligned memory access and do not enforce 8 byte stack alignment
    SCB->CCR &= ~(SCB_CCR_UNALIGN_TRP | SCB_CCR_STKALIGN);

    // for clock configuration the cpu has to run on the internal 8MHz oscillator
    RCC->CR |= RCC_CR_HSION;
    while(!(RCC->CR & RCC_CR_HSIRDY));
    
    RCC->CFGR = RCC_CFGR_PLLMULL_BITS // pll multiplier
              | RCC_CFGR_PLLSRC_HSE   // pll in = HSE
              | RCC_CFGR_SW_HSI;      // sysclk = AHB = APB1 = APB2 = HSI (8MHz)
    
    // turn HSE & PLL on
    RCC->CR |= RCC_CR_PLLON | RCC_CR_HSEON;
    
    // set flash access time & enable prefetch buffer
    FLASH->ACR = FLASH_ACR_LATENCY_BITS | FLASH_ACR_PRFTBE;

    // wait for PPL to lock
    while(!(RCC->CR & RCC_CR_PLLRDY));
        
    // final clock setup
    RCC->CFGR = RCC_CFGR_PLLMULL_BITS   // pll multiplier
              | RCC_CFGR_PLLSRC_HSE     // pll in = HSE
              | RCC_CFGR_SW_PLL         // sysclk = pll out (SYSTEM_CLOCK_HZ)
              | RCC_CFGR_USBPRE_BIT     // USB clock
              | RCC_CFGR_HPRE_DIV_BITS  // AHB clock
              | RCC_CFGR_PPRE1_DIV_BITS // APB1 clock
              | RCC_CFGR_PPRE2_DIV_BITS // APB2 clock
              | RCC_CFGR_ADC_BITS;      // ADC clock (max 14MHz)
            
    // minimal peripheral clocks
    RCC->AHBENR  = RCC_AHBENR_SRAMEN | RCC_AHBENR_FLITFEN;
    RCC->APB2ENR = RCC_APB2ENR_AFIOEN;
    RCC->APB1ENR = RCC_APB1ENR_PWREN;
    
    // stop HSI clock
    RCC->CR &= ~RCC_CR_HSION;
}


void __section("SectionForBootstrapOperations") BootstrapCode()
{
    STM32_BootstrapCode();
    
    BootstrapCode_GPIO();

    PrepareImageRegions();
}

void __section("SectionForBootstrapOperations") BootstrapCodeMinimal()
{
    STM32_BootstrapCode();
    
    BootstrapCode_GPIO();
}

}

