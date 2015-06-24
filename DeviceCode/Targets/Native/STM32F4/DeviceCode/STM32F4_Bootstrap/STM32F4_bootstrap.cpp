////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** Bootstrap ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

#ifndef FLASH
#define FLASH               ((FLASH_TypeDef *) FLASH_R_BASE)
#endif

///////////////////////////////////////////////////////////////////////////////



/* STM32F4 clock configuration */

#if SYSTEM_CRYSTAL_CLOCK_HZ % ONE_MHZ != 0
    #error SYSTEM_CRYSTAL_CLOCK_HZ must be a multiple of 1MHz
#endif
#if SYSTEM_CRYSTAL_CLOCK_HZ != 0
	#define RCC_PLLCFGR_PLLM_BITS (SYSTEM_CRYSTAL_CLOCK_HZ / ONE_MHZ * RCC_PLLCFGR_PLLM_0)
	#define RCC_PLLCFGR_PLLS_BITS (RCC_PLLCFGR_PLLSRC_HSE)
#else // 16MHz internal oscillator
	#define RCC_PLLCFGR_PLLM_BITS (16 * RCC_PLLCFGR_PLLM_0)
	#define RCC_PLLCFGR_PLLS_BITS (RCC_PLLCFGR_PLLSRC_HSI)
#endif

#if (SYSTEM_CLOCK_HZ * 2 >= 192000000)\
 && (SYSTEM_CLOCK_HZ * 2 <= 432000000)\
 && (SYSTEM_CLOCK_HZ * 2 % 48000000 == 0)
    #define RCC_PLLCFGR_PLLN_BITS (SYSTEM_CLOCK_HZ * 2 / ONE_MHZ * RCC_PLLCFGR_PLLN_0)
    #define RCC_PLLCFGR_PLLP_BITS (0)  // P = 2
    #define RCC_PLLCFGR_PLLQ_BITS (SYSTEM_CLOCK_HZ * 2 / 48000000 * RCC_PLLCFGR_PLLQ_0)
#elif (SYSTEM_CLOCK_HZ * 4 >= 192000000)\
   && (SYSTEM_CLOCK_HZ * 4 <= 432000000)\
   && (SYSTEM_CLOCK_HZ * 4 % 48000000 == 0)
    #define RCC_PLLCFGR_PLLN_BITS (SYSTEM_CLOCK_HZ * 4 / ONE_MHZ * RCC_PLLCFGR_PLLN_0)
    #define RCC_PLLCFGR_PLLP_BITS (RCC_PLLCFGR_PLLP_0)  // P = 4
    #define RCC_PLLCFGR_PLLQ_BITS (SYSTEM_CLOCK_HZ * 4 / 48000000 * RCC_PLLCFGR_PLLQ_0)
#elif (SYSTEM_CLOCK_HZ * 6 >= 192000000)\
   && (SYSTEM_CLOCK_HZ * 6 <= 432000000)\
   && (SYSTEM_CLOCK_HZ * 6 % 48000000 == 0)
    #define RCC_PLLCFGR_PLLN_BITS (SYSTEM_CLOCK_HZ * 6 / ONE_MHZ * RCC_PLLCFGR_PLLN_0)
    #define RCC_PLLCFGR_PLLP_BITS (RCC_PLLCFGR_PLLP_1)  // P = 6
    #define RCC_PLLCFGR_PLLQ_BITS (SYSTEM_CLOCK_HZ * 6 / 48000000 * RCC_PLLCFGR_PLLQ_0)
#elif (SYSTEM_CLOCK_HZ * 8 >= 192000000)\
   && (SYSTEM_CLOCK_HZ * 8 <= 432000000)\
   && (SYSTEM_CLOCK_HZ * 8 % 48000000 == 0)
    #define RCC_PLLCFGR_PLLN_BITS (SYSTEM_CLOCK_HZ * 8 / ONE_MHZ * RCC_PLLCFGR_PLLN_0)
    #define RCC_PLLCFGR_PLLP_BITS (RCC_PLLCFGR_PLLP_0 | RCC_PLLCFGR_PLLP_1)  // P = 8
    #define RCC_PLLCFGR_PLLQ_BITS (SYSTEM_CLOCK_HZ * 8 / 48000000 * RCC_PLLCFGR_PLLQ_0)
#else
    #error illegal SYSTEM_CLOCK_HZ frequency
#endif

#define RCC_PLLCFGR_PLL_BITS (RCC_PLLCFGR_PLLM_BITS \
                            | RCC_PLLCFGR_PLLN_BITS \
                            | RCC_PLLCFGR_PLLP_BITS \
                            | RCC_PLLCFGR_PLLQ_BITS \
                            | RCC_PLLCFGR_PLLS_BITS)

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

#if SUPPLY_VOLTAGE_MV < 2100
    #if SYSTEM_CYCLE_CLOCK_HZ <= 20000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_0WS // no wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 40000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_1WS // 1 wait state
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 60000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_2WS // 2 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 80000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_3WS // 3 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 100000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_4WS // 4 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 120000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_5WS // 5 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 140000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_6WS // 6 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 160000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_7WS // 7 wait states
    #else
        #error SYSTEM_CYCLE_CLOCK_HZ must be <= 160MHz at < 2.1V
    #endif
#elif SUPPLY_VOLTAGE_MV < 2400
    #if SYSTEM_CYCLE_CLOCK_HZ <= 22000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_0WS // no wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 44000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_1WS // 1 wait state
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 66000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_2WS // 2 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 88000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_3WS // 3 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 110000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_4WS // 4 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 1328000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_5WS // 5 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 154000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_6WS // 6 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 176000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_7WS // 7 wait states
    #else
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_8WS // 8 wait states
    #endif
#elif SUPPLY_VOLTAGE_MV < 2700
    #if SYSTEM_CYCLE_CLOCK_HZ <= 24000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_0WS // no wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 48000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_1WS // 1 wait state
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 72000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_2WS // 2 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 96000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_3WS // 3 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 120000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_4WS // 4 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 144000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_5WS // 5 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 168000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_6WS // 6 wait states
    #else
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_7WS // 7 wait states
    #endif
#else
    #if SYSTEM_CYCLE_CLOCK_HZ <= 30000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_0WS // no wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 60000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_1WS // 1 wait state
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 90000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_2WS // 2 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 120000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_3WS // 3 wait states
    #elif SYSTEM_CYCLE_CLOCK_HZ <= 150000000
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_4WS // 4 wait states
    #else
        #define FLASH_ACR_LATENCY_BITS FLASH_ACR_LATENCY_5WS // 5 wait states
    #endif
#endif


#pragma arm section code = "SectionForBootstrapOperations"


/* IO initialization implemented in solution DeviceCode\Init */
void BootstrapCode_GPIO();


extern "C"
{

void __section("SectionForBootstrapOperations") STM32F4_BootstrapCode()
{
#ifdef STM32F4XX
    // enable FPU coprocessors (CP10, CP11)
    SCB->CPACR |= 0x3 << 2 * 10 | 0x3 << 2 * 11; // full access
#endif
    
    // allow unaligned memory access and do not enforce 8 byte stack alignment
    SCB->CCR &= ~(SCB_CCR_UNALIGN_TRP_Msk | SCB_CCR_STKALIGN_Msk);

    // for clock configuration the cpu has to run on the internal 16MHz oscillator
    RCC->CR |= RCC_CR_HSION;
    while(!(RCC->CR & RCC_CR_HSIRDY));
    
    RCC->CFGR = RCC_CFGR_SW_HSI;         // sysclk = AHB = APB1 = APB2 = HSI (16MHz)
    RCC->CR &= ~(RCC_CR_PLLON | RCC_CR_PLLI2SON); // pll off
    
#if SYSTEM_CRYSTAL_CLOCK_HZ != 0
	// turn HSE on
    RCC->CR |= RCC_CR_HSEON;
    while(!(RCC->CR & RCC_CR_HSERDY));
#endif
    
    // Set flash access time and enable caches & prefetch buffer
#ifdef STM32F4XX
    // The prefetch buffer must not be enabled on rev A devices.
    // Rev A cannot be read from revision field (another rev A error!).
    // The wrong device field (411=F2) must be used instead!
    if ((DBGMCU->IDCODE & 0xFF) == 0x11) {
        FLASH->ACR = FLASH_ACR_ICEN | FLASH_ACR_DCEN | FLASH_ACR_LATENCY_BITS;
    } else {
        FLASH->ACR = FLASH_ACR_PRFTEN | FLASH_ACR_ICEN | FLASH_ACR_DCEN | FLASH_ACR_LATENCY_BITS;
    }
#else
    FLASH->ACR = FLASH_ACR_PRFTEN | FLASH_ACR_ICEN | FLASH_ACR_DCEN | FLASH_ACR_LATENCY_BITS;
#endif

    // setup PLL
    RCC->PLLCFGR = RCC_PLLCFGR_PLL_BITS; // pll multipliers
    RCC->CR |= RCC_CR_PLLON;             // pll on
    while(!(RCC->CR & RCC_CR_PLLRDY));
        
    // final clock setup
    RCC->CFGR = RCC_CFGR_SW_PLL          // sysclk = pll out (SYSTEM_CLOCK_HZ)
              | RCC_CFGR_HPRE_DIV_BITS   // AHB clock
              | RCC_CFGR_PPRE1_DIV_BITS  // APB1 clock
              | RCC_CFGR_PPRE2_DIV_BITS; // APB2 clock
            
    // minimal peripheral clocks
#ifdef STM32F4XX
    RCC->AHB1ENR = RCC_AHB1ENR_CCMDATARAMEN; // 64k RAM (CCM)
#else
    RCC->AHB1ENR = 0;
#endif
    RCC->AHB2ENR = 0;
    RCC->AHB3ENR = 0;
    RCC->APB1ENR = RCC_APB1ENR_PWREN;    // PWR clock used for sleep;
    RCC->APB2ENR = RCC_APB2ENR_SYSCFGEN; // SYSCFG clock used for IO;
    
    // stop HSI clock
#if SYSTEM_CRYSTAL_CLOCK_HZ != 0
	RCC->CR &= ~RCC_CR_HSION;
#endif

    // remove Flash remap to Boot area to avoid problems with Monitor_Execute
    SYSCFG->MEMRMP = 1; // map System memory to Boot area
    
#ifdef STM32F4_Enable_RTC
    STM32F4_RTC_Initialize(); // enable RTC
#endif

}

__section("SectionForBootstrapOperations") void BootstrapCode()
{
    STM32F4_BootstrapCode();
    
    PrepareImageRegions();
    
    BootstrapCode_GPIO();

}

__section("SectionForBootstrapOperations") void BootstrapCodeMinimal()
{
    STM32F4_BootstrapCode();
    
    BootstrapCode_GPIO();
}

}

