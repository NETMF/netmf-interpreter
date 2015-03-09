////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  *** AD Conversion ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\stm32f10x.h"

///////////////////////////////////////////////////////////////////////////////

#define STM32_AD_CHANNELS 6       // number of channels
#define STM32_AD_FIRST_PIN 32     // channel 0 pin (C0)
#define STM32_AD_FIRST_CHANNEL 10 // channel 0 hw channel
#define STM32_AD_SAMPLE_TIME 3    // sample time = 28.5 cycles

//--//

BOOL AD_Initialize( ANALOG_CHANNEL channel, INT32 precisionInBits )
{
    if (!(RCC->APB2ENR & RCC_APB2ENR_ADC1EN)) { // not yet initialized
        RCC->APB2ENR |= RCC_APB2ENR_ADC1EN; // enable AD clock
        ADC1->SQR1 = 0; // 1 conversion
        ADC1->CR1 = 0;
        ADC1->CR2 = ADC_CR2_ADON; // AD on
        ADC1->SMPR1 = 0x00249249 * STM32_AD_SAMPLE_TIME;
        ADC1->SMPR2 = 0x09249249 * STM32_AD_SAMPLE_TIME;
    }
    // set pin as analog input
    CPU_GPIO_DisablePin(STM32_AD_FIRST_PIN + channel, RESISTOR_DISABLED, 0, GPIO_ALT_MODE_1);
    return TRUE;
}

void AD_Uninitialize( ANALOG_CHANNEL channel )
{
    // free pin
    CPU_GPIO_DisablePin(STM32_AD_FIRST_PIN + channel, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY);
}

INT32 AD_Read( ANALOG_CHANNEL channel )
{
    int x = ADC1->DR; // clear EOC flag
    ADC1->SQR3 = STM32_AD_FIRST_CHANNEL + channel; // select channel
    ADC1->CR2 |= ADC_CR2_ADON; // start AD
    while (!(ADC1->SR & ADC_SR_EOC)); // wait for completion
    return ADC1->DR; // read result
}

UINT32 AD_ADChannels()
{
    return STM32_AD_CHANNELS;
}

GPIO_PIN AD_GetPinForChannel( ANALOG_CHANNEL channel )
{
    if ((UINT32)channel >= STM32_AD_CHANNELS) return GPIO_PIN_NONE;
    return STM32_AD_FIRST_PIN + channel;
}

BOOL AD_GetAvailablePrecisionsForChannel( ANALOG_CHANNEL channel, INT32* precisions, UINT32& size )
{
    size = 0;
    if (precisions == NULL || (UINT32)channel >= STM32_AD_CHANNELS) return FALSE;
    precisions[0] = 12;
    size = 1;
    return TRUE;
}
