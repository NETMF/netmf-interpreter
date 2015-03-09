////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** AD Conversion ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

//--//


#define STM32F4_AD_SAMPLE_TIME 2   // sample time = 28 cycles

#if STM32F4_ADC == 1
    #define ADCx ADC1
    #define RCC_APB2ENR_ADCxEN RCC_APB2ENR_ADC1EN
    #define STM32F4_ADC_PINS {0,1,2,3,4,5,6,7,16,17,32,33,34,35,36,37} // ADC1 pins
#elif STM32F4_ADC == 3
    #define ADCx ADC3
    #define RCC_APB2ENR_ADCxEN RCC_APB2ENR_ADC3EN
    #define STM32F4_ADC_PINS {0,1,2,3,86,87,88,89,90,83,32,33,34,35,84,85} // ADC3 pins
#else
    #error wrong STM32F4_ADC value (1 or 3)
#endif

// Channels
static const BYTE g_STM32F4_AD_Channel[] = STM32F4_AD_CHANNELS;
static const BYTE g_STM32F4_AD_Pins[] = STM32F4_ADC_PINS;
#define STM32F4_AD_NUM ARRAYSIZE_CONST_EXPR(g_STM32F4_AD_Channel)  // number of channels


//--//

BOOL AD_Initialize( ANALOG_CHANNEL channel, INT32 precisionInBits )
{
    if (!(RCC->APB2ENR & RCC_APB2ENR_ADCxEN)) { // not yet initialized
        RCC->APB2ENR |= RCC_APB2ENR_ADCxEN; // enable AD clock
        ADC->CCR = 0; // ADCCLK = PB2CLK / 2;
        ADCx->SQR1 = 0; // 1 conversion
        ADCx->CR1 = 0;
        ADCx->CR2 = ADC_CR2_ADON; // AD on
        ADCx->SMPR1 = 0x01249249 * STM32F4_AD_SAMPLE_TIME;
        ADCx->SMPR2 = 0x09249249 * STM32F4_AD_SAMPLE_TIME;
    }
    // set pin as analog input
    CPU_GPIO_DisablePin(AD_GetPinForChannel(channel), RESISTOR_DISABLED, 0, GPIO_ALT_MODE_1);
    return TRUE;
}

void AD_Uninitialize( ANALOG_CHANNEL channel )
{
    // free pin
    CPU_GPIO_DisablePin(AD_GetPinForChannel(channel), RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY);
}

INT32 AD_Read( ANALOG_CHANNEL channel )
{
    if ((UINT32)channel >= STM32F4_AD_NUM) return 0;
    int x = ADCx->DR; // clear EOC flag
    ADCx->SQR3 = g_STM32F4_AD_Channel[channel]; // select channel
    ADCx->CR2 |= ADC_CR2_SWSTART; // start AD
    while (!(ADCx->SR & ADC_SR_EOC)); // wait for completion
    return ADCx->DR; // read result
}

UINT32 AD_ADChannels()
{
    return STM32F4_AD_NUM;
}

GPIO_PIN AD_GetPinForChannel( ANALOG_CHANNEL channel )
{
    if ((UINT32)channel >= STM32F4_AD_NUM) return GPIO_PIN_NONE;
    int chNum = g_STM32F4_AD_Channel[channel];
    return (GPIO_PIN)g_STM32F4_AD_Pins[chNum];
}

BOOL AD_GetAvailablePrecisionsForChannel( ANALOG_CHANNEL channel, INT32* precisions, UINT32& size )
{
    size = 0;
    if (precisions == NULL || (UINT32)channel >= STM32F4_AD_NUM) return FALSE;
    precisions[0] = 12;
    size = 1;
    return TRUE;
}
