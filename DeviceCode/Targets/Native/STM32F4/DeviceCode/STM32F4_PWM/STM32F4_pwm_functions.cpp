////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** PWM Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif


#if SYSTEM_APB1_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ
    #define PWM1_CLK_HZ (SYSTEM_APB1_CLOCK_HZ)
#else
    #define PWM1_CLK_HZ (SYSTEM_APB1_CLOCK_HZ * 2)
#endif
#define PWM1_CLK_MHZ (PWM1_CLK_HZ / ONE_MHZ)

#if SYSTEM_APB2_CLOCK_HZ == SYSTEM_CYCLE_CLOCK_HZ
    #define PWM2_CLK_HZ (SYSTEM_APB2_CLOCK_HZ)
#else
    #define PWM2_CLK_HZ (SYSTEM_APB2_CLOCK_HZ * 2)
#endif
#define PWM2_CLK_MHZ (PWM2_CLK_HZ / ONE_MHZ)

#if PWM2_CLK_MHZ > PWM1_CLK_MHZ
    #define PWM_MAX_CLK_MHZ PWM2_CLK_MHZ
#else
    #define PWM_MAX_CLK_MHZ PWM1_CLK_MHZ
#endif


typedef  TIM_TypeDef* ptr_TIM_TypeDef;

//Timers
static const BYTE g_STM32F4_PWM_Timer[] = STM32F4_PWM_TIMER;
static const BYTE g_STM32F4_PWM_Channel[] = STM32F4_PWM_CHNL;

// Pins
static const BYTE g_STM32F4_PWM_Pins[] = STM32F4_PWM_PINS;
#define STM32F4_PWM_CHANNELS ARRAYSIZE_CONST_EXPR(g_STM32F4_PWM_Pins) // number of channels

// IO addresses
static const ptr_TIM_TypeDef g_STM32F4_PWM_Ports[] =
    {TIM1, TIM2, TIM3, TIM4, TIM5, NULL, NULL, TIM8, TIM9, TIM10, TIM11, TIM12, TIM13, TIM14};
    
// Alternate Function Number
static const BYTE g_STM32F4_PWM_Alt[] = {0x12,0x12,0x22,0x22,0x22,0,0,0x32,0x32,0x32,0x32,0x92,0x92,0x92}; // AF1/2/3/9


//--//

void STM32F4_PWM_UnitializeAll()
{
    for (int c = 0; c < STM32F4_PWM_CHANNELS; c++) {
        PWM_Stop((PWM_CHANNEL)c, PWM_GetPinForChannel((PWM_CHANNEL)c));
        PWM_Uninitialize((PWM_CHANNEL)c);
    }
}

BOOL PWM_Initialize(PWM_CHANNEL channel)
{
    if (channel >= STM32F4_PWM_CHANNELS) return FALSE;
    int timer = g_STM32F4_PWM_Timer[channel];
    int tchnl = g_STM32F4_PWM_Channel[channel];
    ptr_TIM_TypeDef treg = g_STM32F4_PWM_Ports[timer - 1];
    
    // relevant RCC register & bit
    __IO uint32_t* enReg = &RCC->APB1ENR;
    if ((UINT32)treg & 0x10000) enReg = &RCC->APB2ENR;
    int enBit = 1 << (((UINT32)treg >> 10) & 0x1F);

    if (!(*enReg & enBit)) { // not yet initialized
        *enReg |= enBit; // enable timer clock
        treg->CR1 = TIM_CR1_URS | TIM_CR1_ARPE; // double buffered update
        treg->EGR = TIM_EGR_UG; // enforce first update
        if (timer == 1 || timer == 8) {
            treg->BDTR |= TIM_BDTR_MOE; // main output enable (timer 1 & 8 only)
        }
    }
    
    *(__IO uint16_t*)&((uint32_t*)&treg->CCR1)[tchnl] = 0; // reset compare register
    
    // enable PWM channel
    UINT32 mode = TIM_CCMR1_OC1M_1 | TIM_CCMR1_OC1M_2 | TIM_CCMR1_OC1PE; // PWM1 mode, double buffered
    if (tchnl & 1) mode <<= 8; // 1 or 3
    __IO uint16_t* reg = &treg->CCMR1;
    if (tchnl & 2) reg = &treg->CCMR2; // 2 or 3
    *reg |= mode;
    
    // Ensure driver gets unitialized during soft reboot
    HAL_AddSoftRebootHandler(STM32F4_PWM_UnitializeAll);

    return TRUE;
}

BOOL PWM_Uninitialize(PWM_CHANNEL channel)
{
    int timer = g_STM32F4_PWM_Timer[channel];
    int tchnl = g_STM32F4_PWM_Channel[channel];
    ptr_TIM_TypeDef treg = g_STM32F4_PWM_Ports[timer - 1];
    
    UINT32 mask = 0xFF; // disable PWM channel
    if (tchnl & 1) mask = 0xFF00; // 1 or 3
    __IO uint16_t* reg = &treg->CCMR1;
    if (tchnl & 2) reg = &treg->CCMR2; // 2 or 3
    *reg &= ~mask;
    
    if ((treg->CCMR1 | treg->CCMR2) == 0) { // no channel active
        __IO uint32_t* enReg = &RCC->APB1ENR;
        if ((UINT32)treg & 0x10000) enReg = &RCC->APB2ENR;
        int enBit = 1 << (((UINT32)treg >> 10) & 0x1F);
        *enReg &= ~enBit; // disable timer clock
    }
    
    return TRUE;
}

BOOL PWM_ApplyConfiguration(PWM_CHANNEL channel, GPIO_PIN pin, UINT32& period, UINT32& duration, PWM_SCALE_FACTOR& scale, BOOL invert)
{
    int timer = g_STM32F4_PWM_Timer[channel];
    int tchnl = g_STM32F4_PWM_Channel[channel];
    ptr_TIM_TypeDef treg = g_STM32F4_PWM_Ports[timer - 1];
    
    UINT32 p = period, d = duration, s = scale;
    if (d > p) d = p;
    
    // set pre, p, & d such that:
    // pre * p = PWM_CLK * period / scale
    // pre * d = PWM_CLK * duration / scale
    
    UINT32 clk = PWM1_CLK_HZ;
    if ((UINT32)treg & 0x10000) clk = PWM2_CLK_HZ; // APB2
    
    UINT32 pre = clk / s; // prescaler
    if (pre == 0) { // s > PWM_CLK
        UINT32 sm = s / ONE_MHZ; // scale in MHz
        clk = PWM1_CLK_MHZ;      // clock in MHz
        if ((UINT32)treg & 0x10000) clk = PWM2_CLK_MHZ; // APB2
        if (p > 0xFFFFFFFF / PWM_MAX_CLK_MHZ) { // avoid overflow
            pre = clk;
            p /= sm;
            d /= sm;
        } else {
            pre = 1;
            p = p * clk / sm;
            d = d * clk / sm;
        }
    } else {
        while (pre > 0x10000) { // prescaler too large
            if (p >= 0x80000000) return FALSE;
            pre >>= 1;
            p <<= 1;
            d <<= 1;
        }
    }
    if (timer != 2 && timer != 5) { // 16 bit timer
        while (p >= 0x10000) { // period too large
            if (pre > 0x8000) return FALSE;
            pre <<= 1;
            p >>= 1;
            d >>= 1;
        }
    }
    treg->PSC = pre - 1;
    treg->ARR = p - 1;
    *(__IO uint16_t*)&((uint32_t*)&treg->CCR1)[tchnl] = d;
    UINT32 invBit = TIM_CCER_CC1P << (4 * tchnl);
    if (invert) {
        treg->CCER |= invBit;
    } else {
        treg->CCER &= ~invBit;
    }
    return TRUE;
}

BOOL PWM_Start(PWM_CHANNEL channel, GPIO_PIN pin)
{
    int timer = g_STM32F4_PWM_Timer[channel];
    int tchnl = g_STM32F4_PWM_Channel[channel];
    ptr_TIM_TypeDef treg = g_STM32F4_PWM_Ports[timer - 1];
    
    CPU_GPIO_DisablePin( pin, RESISTOR_DISABLED, 1, (GPIO_ALT_MODE)g_STM32F4_PWM_Alt[timer - 1] );
    UINT16 enBit = TIM_CCER_CC1E << (4 * tchnl);
    treg->CCER |= enBit; // enable output
    UINT16 cr1 = treg->CR1;
    if ((cr1 & TIM_CR1_CEN) == 0) { // timer stopped
        treg->EGR = TIM_EGR_UG; // enforce register update
        treg->CR1 = cr1 | TIM_CR1_CEN; // start timer
    }
    return TRUE;
}

void PWM_Stop(PWM_CHANNEL channel, GPIO_PIN pin)
{
    int timer = g_STM32F4_PWM_Timer[channel];
    int tchnl = g_STM32F4_PWM_Channel[channel];
    ptr_TIM_TypeDef treg = g_STM32F4_PWM_Ports[timer - 1];
    
    UINT16 ccer = treg->CCER;
    ccer &= ~(TIM_CCER_CC1E << (4 * tchnl));
    treg->CCER = ccer; // disable output
    CPU_GPIO_DisablePin( pin, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY );
    if ((ccer & (TIM_CCER_CC1E | TIM_CCER_CC2E | TIM_CCER_CC3E | TIM_CCER_CC4E)) == 0) { // idle
        treg->CR1 &= ~TIM_CR1_CEN; // stop timer
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
    return STM32F4_PWM_CHANNELS;
}

GPIO_PIN PWM_GetPinForChannel( PWM_CHANNEL channel )
{
    if ((UINT32)channel >= STM32F4_PWM_CHANNELS) return GPIO_PIN_NONE;
    return g_STM32F4_PWM_Pins[channel];
}
