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


BOOL RTC_Initialize()
{
    PWR->CR |= PWR_CR_DBP; // enable RTC access
    RCC->BDCR |= RCC_BDCR_RTCSEL_0 | RCC_BDCR_LSEON | RCC_BDCR_RTCEN; // RTC & LSE on
    
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
    if (!(RTC->ISR & RTC_ISR_INITS)) { // RTC not set up
        return 0;
    }
    
    while (!(RTC->ISR & RTC_ISR_RSF)); // wait for shadow register ready
    
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
}

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
