////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** STM32F4 RTC driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include <tinyhal.h>
#include "RTC_decl.h"

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

#define RCC_LSE_TIMEOUT_VALUE           ((uint32_t)6000)  // 6000 ms
#define LSI_TIMEOUT_VALUE               ((uint32_t)100)   // 100 ms

BOOL RTC_Initialize()
{
#ifndef STM32F4_RTC_ENABLE

    PWR->CR |= PWR_CR_DBP; // enable RTC access
    RCC->BDCR |= RCC_BDCR_RTCSEL_0 | RCC_BDCR_LSEON | RCC_BDCR_RTCEN; // RTC & LSE on
    
#else
  
    UINT64 ticksStart;
    uint32_t tmpreg1 = 0;

   	// enable power clock
  	RCC->APB1ENR |= RCC_APB1ENR_PWREN;
  	// also on sleep mode
  	RCC->APB1LPENR |= RCC_APB1LPENR_PWRLPEN;
     		
    // enable access to the backup domain allowing access to RTC
    PWR->CR |= PWR_CR_DBP;
    
#if defined(STM32F4_RTC_LSE)
    // enable LSE
    
    if((RCC->BDCR & RCC_BDCR_RTCSEL) != RCC_BDCR_RTCSEL_LSE)
    {
        // Store the content of BDCR register before the reset of Backup Domain
        // and clear RTC clock source
        tmpreg1 = (RCC->BDCR & ~(RCC_BDCR_RTCSEL));
        // RTC Clock selection can be changed only if the Backup Domain is reset
        RCC->BDCR |= RCC_BDCR_BDRST;
        RCC->BDCR &= ~RCC_BDCR_BDRST;
        // Restore the Content of BDCR register
        RCC->BDCR = tmpreg1;
    }
    
    // Set LSEON bit
    RCC->BDCR = RCC_BDCR_LSEON;

    // Get Start Tick
  	ticksStart = HAL_Time_CurrentTicks();

    // wait for external low-speed oscillator valid
    while(!(RCC->BDCR & RCC_BDCR_LSERDY))
    {
        if((HAL_Time_CurrentTicks()-ticksStart) > CPU_MillisecondsToTicks((UINT32)RCC_LSE_TIMEOUT_VALUE))
        {
            // external low-speed oscillator is not working or not fitted
            return FALSE;
        }    
    }
			
		// Select the RTC Clock Source
    RCC->BDCR |= RCC_BDCR_RTCSEL_LSE;   
    
#elif defined(STM32F4_RTC_LSE_BYPASS)
    // enable LSE and bypass oscillator

    // Set LSEBYP and LSEON bits
    RCC->BDCR = RCC_BDCR_LSEBYP | RCC_BDCR_LSEON;

    // Get Start Tick
  	ticksStart = HAL_Time_CurrentTicks();

    // wait for external low-speed oscillator valid
    while(!(RCC->BDCR & RCC_BDCR_LSERDY))
    {
        if((HAL_Time_CurrentTicks()-ticksStart) > CPU_MillisecondsToTicks((UINT32)RCC_LSE_TIMEOUT_VALUE))
        {
            // external low-speed oscillator is not working or not fitted
            return FALSE;
        }    
    }
			
    // Select the RTC Clock Source
    RCC->BDCR |= RCC_BDCR_RTCSEL_LSE;

#elif defined(STM32F4_RTC_LSI)
    // enable LSI
        
    // enable internal low-power oscillator
    RCC->CSR |= (uint32_t)RCC_CSR_LSION

    // Get Start Tick
    ticksStart = HAL_Time_CurrentTicks();
        
    // Wait till LSI is ready
    while(!(RCC->CSR & RCC_CSR_LSIRDY))
    {
        if((HAL_Time_CurrentTicks()-ticksStart) > CPU_MillisecondsToTicks((UINT32)LSI_TIMEOUT_VALUE))
        {
            // external low-speed oscillator is not working
            return FALSE;
        } 
    }
		
    // Select the RTC Clock Source
    RCC->BDCR |= RCC_BDCR_RTCSEL_LSI;
    
#else
// sanity check for RTC enabled but no selection of clock source
#error RTC is enabled in configuration but there is no RTC clock source selected   
#endif
       		
  	// Enable the RTC Clock
    RCC->BDCR |= RCC_BDCR_RTCEN;   
  	
    // Disable the write protection for RTC registers
    RTC->WPR = 0xCA;
    RTC->WPR = 0x53;
   
    // Check if the Initialization mode is set
    if (!(RTC->ISR & RTC_ISR_INITF))
    {
        // Set the Initialization mode
        RTC->ISR |= (uint32_t)RTC_ISR_INIT;
        
        // Wait till RTC is in INIT state and if Time out is reached exit 
        // according to the STM32 manual it's OK to wait without a timeout as it takes from 1 to 2 RTCCLK clock cycles to enter Initialization mode 
        while(!(RTC->ISR & RTC_ISR_INITF));
    }

    // can be read from the shadow registers only if APB clock is at least 7 times the frequency of the RTC
    // to avoid adding another check on this, RTC will always be read directly from registers
    RTC->CR |= RTC_CR_BYPSHAD; 
  
    // Configure the RTC PRER
    RTC->PRER = (uint32_t)0xFF;
    RTC->PRER |= ((uint32_t)0x7F << 16);
  
    // Exit Initialization mode
    RTC->ISR &= ~RTC_ISR_INIT;  
    
    // Enable the write protection for RTC registers
    RTC->WPR = 0xFF; 
	
    // disable access to the backup domain
    PWR->CR &= ~PWR_CR_DBP;
    
#endif
   
    return TRUE;
}

UINT32 RTC_BcdToBin(UINT32 bcd)
{
    return (bcd >> 4) * 10 + (bcd & 15);
}

UINT32 RTC_BinToBcd(UINT32 bin)
{
    return bin / 10 << 4 | bin % 10;
}

INT64 RTC_GetTime()
{
    if (!(RTC->ISR & RTC_ISR_INITS)) 
    { 
        // RTC not set up
        return 0;
    }
        
#ifndef STM32F4_RTC_ENABLE

    while (!(RTC->ISR & RTC_ISR_RSF)); // wait for shadow register ready
    
#endif   
    
    UINT32 ss = ~RTC->SSR & 255; // sub seconds [s/256]
    UINT32 tr = RTC->TR; // time
    UINT32 dr = RTC->DR; // date
    
    SYSTEMTIME sysTime;
    sysTime.wMilliseconds = ss * 1000 >> 8; // s/256 -> ms
    sysTime.wSecond = RTC_BcdToBin(tr & 0x7F);
    sysTime.wMinute = RTC_BcdToBin(tr >> 8 & 0x7F);
    sysTime.wHour = RTC_BcdToBin(tr >> 16 & 0x3F);
    sysTime.wDay = RTC_BcdToBin(dr & 0x3F);
    sysTime.wMonth = RTC_BcdToBin(dr >> 8 & 0x1F);
    sysTime.wYear = 2000 + RTC_BcdToBin(dr >> 16 & 0xFF);
    return Time_FromSystemTime(&sysTime);
}

void RTC_SetTime( INT64 time )
{
#ifndef STM32F4_RTC_ENABLE

    RTC->WPR = 0xCA; // disable write protection
    RTC->WPR = 0x53;
    RTC->ISR = 0xFFFFFFFF; // enter Init mode
    
    SYSTEMTIME sysTime;
    Time_ToSystemTime(time, &sysTime);
    UINT32 tr = RTC_BinToBcd(sysTime.wSecond)
              | RTC_BinToBcd(sysTime.wMinute) << 8
              | RTC_BinToBcd(sysTime.wHour) << 16;
    UINT32 dr = RTC_BinToBcd(sysTime.wDay)
              | RTC_BinToBcd(sysTime.wMonth) << 8
              | (sysTime.wDayOfWeek ? sysTime.wDayOfWeek : 7) << 13
              | RTC_BinToBcd(sysTime.wYear % 100) << 16;
    
    while (!(RTC->ISR & RTC_ISR_INITF)); // wait for Init mode ready
    
    RTC->CR &= ~(RTC_CR_FMT); // 24h format
    RTC->TR = tr;
    RTC->DR = dr;

#ifdef RTC_CALIBRATION
    #if RTC_CALIBRATION > 488 || RTC_CALIBRATION < -488
        #error illegal RTC_CALIBRATION value (-488..488)
    #endif
    UINT32 cal = -((RTC_CALIBRATION << 20) / 1000000); // ppm -> 1/2^20
    if (cal < 0) cal += RTC_CALR_CALP | 512;
    RTC->CALR = cal;
    }
#endif
    
    RTC->ISR = 0xFFFFFFFF & ~RTC_ISR_INIT; // exit Init mode
    RTC->WPR = 0xFF; // enable write protection

#else

    // enable access to the backup domain allowing access to RTC
    PWR->CR |= PWR_CR_DBP;

    RTC->WPR = 0xCA; // disable write protection
    RTC->WPR = 0x53;
    
    // Set the Initialization mode
    RTC->ISR |= (uint32_t)RTC_ISR_INIT;
        
    SYSTEMTIME sysTime;
    Time_ToSystemTime(time, &sysTime);
    UINT32 tr = RTC_BinToBcd(sysTime.wSecond)
              | RTC_BinToBcd(sysTime.wMinute) << 8
              | RTC_BinToBcd(sysTime.wHour) << 16;
    UINT32 dr = RTC_BinToBcd(sysTime.wDay)
              | RTC_BinToBcd(sysTime.wMonth) << 8
              | (sysTime.wDayOfWeek ? sysTime.wDayOfWeek : 7) << 13
              | RTC_BinToBcd(sysTime.wYear % 100) << 16;
   
    // Wait till RTC is in INIT state and if Time out is reached exit 
    // according to the STM32 manual it's OK to wait without a timeout as it takes from 1 to 2 RTCCLK clock cycles to enter Initialization mode 
    while(!(RTC->ISR & RTC_ISR_INITF));
    
    // 24h format
    RTC->CR &= ~(RTC_CR_FMT);
    // set time register
    RTC->TR = tr;
    // set date register
    RTC->DR = dr;
    
    // Exit Initialization mode
    RTC->ISR &= ~RTC_ISR_INIT;  
    
    // Enable the write protection for RTC registers
    RTC->WPR = 0xFF; 
    	
    // disable access to the backup domain
    PWR->CR &= ~PWR_CR_DBP;
    
#endif    
}

#ifndef STM32F4_RTC_ENABLE

INT32 RTC_GetOffset()
{
    return RTC->BKP0R;
}

void RTC_SetOffset(INT32 offset)
{
    RTC->WPR = 0xCA; // disable write protection
    RTC->WPR = 0x53;
    RTC->BKP0R = offset;
    RTC->WPR = 0xFF; // enable write protection
}

#endif
