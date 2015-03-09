////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  *** PWM Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\stm32f10x.h"


#define STM32_PWM_CHANNELS 4 // number of channels
#define STM32_PWM_FIRST_PIN 0 // channel 0 pin (A0)


#if SYSTEM_APB1_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ
#define TIM5CLK_HZ SYSTEM_APB1_CLOCK_HZ
#else
#define TIM5CLK_HZ (SYSTEM_APB1_CLOCK_HZ * 2)
#endif
#define TIM5CLK_MHZ (TIM5CLK_HZ / ONE_MHZ)


//--//

BOOL PWM_Initialize(PWM_CHANNEL channel)
{
    if (channel >= STM32_PWM_CHANNELS) return FALSE;

    if (!(RCC->APB1ENR & RCC_APB1ENR_TIM5EN)) { // not yet initialized
        RCC->APB1ENR |= RCC_APB1ENR_TIM5EN; // enable timer clock
        TIM5->CR1 = TIM_CR1_URS | TIM_CR1_ARPE; // double buffered update
        TIM5->EGR = TIM_EGR_UG; // enforce first update
    }
    
    *(__IO uint16_t*)&((uint32_t*)&TIM5->CCR1)[channel] = 0; // reset compare register
    
    // enable PWM channel
    UINT32 mode = TIM_CCMR1_OC1M_1 | TIM_CCMR1_OC1M_2 | TIM_CCMR1_OC1PE; // PWM1 mode, double buffered
    if (channel & 1) mode <<= 8; // 1 or 3
    __IO uint16_t* reg = &TIM5->CCMR1;
    if (channel & 2) reg = &TIM5->CCMR2; // 2 or 3
    *reg |= mode;

    return TRUE;
}

BOOL PWM_Uninitialize(PWM_CHANNEL channel)
{
    UINT32 mask = 0xFF; // disable PWM channel
    if (channel & 1) mask = 0xFF00; // 1 or 3
    __IO uint16_t* reg = &TIM5->CCMR1;
    if (channel & 2) reg = &TIM5->CCMR2; // 2 or 3
    *reg &= ~mask;
    
    if ((TIM5->CCMR1 | TIM5->CCMR2) == 0) { // no channel active
        RCC->APB1ENR &= ~RCC_APB1ENR_TIM5EN; // disable timer clock
    }
    
    return TRUE;
}

BOOL PWM_ApplyConfiguration(PWM_CHANNEL channel, GPIO_PIN pin, UINT32& period, UINT32& duration, PWM_SCALE_FACTOR& scale, BOOL invert)
{
    UINT32 p = period, d = duration, s = scale;
    if (d > p) d = p;
    
    // set pre, p, & d such that:
    // pre * p = PWM_CLK * period / scale
    // pre * d = PWM_CLK * duration / scale
    
    UINT32 pre = TIM5CLK_HZ / s; // prescaler
    if (pre == 0) { // s > PWM_CLK
        UINT32 sm = s / ONE_MHZ; // scale in MHz
        if (p > 0xFFFFFFFF / TIM5CLK_MHZ) { // avoid overflow
            pre = TIM5CLK_MHZ;
            p /= sm;
            d /= sm;
        } else {
            pre = 1;
            p = p * TIM5CLK_MHZ / sm;
            d = d * TIM5CLK_MHZ / sm;
        }
    } else {
        while (pre > 0x10000) { // prescaler too large
            if (p >= 0x8000) return FALSE;
            pre >>= 1;
            p <<= 1;
            d <<= 1;
        }
    }
    while (p >= 0x10000) { // period too large
        if (pre > 0x8000) return FALSE;
        pre <<= 1;
        p >>= 1;
        d >>= 1;
    }
    TIM5->PSC = pre - 1;
    TIM5->ARR = p - 1;
    *(__IO uint16_t*)&((uint32_t*)&TIM5->CCR1)[channel] = d;
    UINT32 invBit = TIM_CCER_CC1P << (4 * channel);
    if (invert) {
        TIM5->CCER |= invBit;
    } else {
        TIM5->CCER &= ~invBit;
    }
    return TRUE;
}

BOOL PWM_Start(PWM_CHANNEL channel, GPIO_PIN pin)
{
    CPU_GPIO_DisablePin( pin, RESISTOR_DISABLED, 1, GPIO_ALT_MODE_1 );
    UINT16 enBit = TIM_CCER_CC1E << (4 * channel);
    TIM5->CCER |= enBit; // enable output
    UINT16 cr1 = TIM5->CR1;
    if ((cr1 & TIM_CR1_CEN) == 0) { // timer stopped
        TIM5->EGR = TIM_EGR_UG; // enforce register update
        TIM5->CR1 = cr1 | TIM_CR1_CEN; // start timer
    }
    return TRUE;
}

void PWM_Stop(PWM_CHANNEL channel, GPIO_PIN pin)
{
    UINT16 ccer = TIM5->CCER;
    ccer &= ~(TIM_CCER_CC1E << (4 * channel));
    TIM5->CCER = ccer; // disable output
    CPU_GPIO_DisablePin( pin, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY );
    if ((ccer & (TIM_CCER_CC1E | TIM_CCER_CC2E | TIM_CCER_CC3E | TIM_CCER_CC4E)) == 0) { // idle
        TIM5->CR1 &= ~TIM_CR1_CEN; // stop timer
    }
}

BOOL PWM_Start(PWM_CHANNEL* channel, GPIO_PIN* pin, UINT32 count)
{
    for (int i = 0; i < count; i++) {
        if (!PWM_Start(channel[i], pin[i])) return FALSE;
    }
    return TRUE;
}

void PWM_Stop(PWM_CHANNEL* channel, GPIO_PIN* pin, UINT32 count)
{
    for (int i = 0; i < count; i++) {
        PWM_Stop(channel[i], pin[i]);
    }
}

UINT32 PWM_PWMChannels() 
{
    return STM32_PWM_CHANNELS;
}

GPIO_PIN PWM_GetPinForChannel( PWM_CHANNEL channel )
{
    return STM32_PWM_FIRST_PIN + channel;
}
