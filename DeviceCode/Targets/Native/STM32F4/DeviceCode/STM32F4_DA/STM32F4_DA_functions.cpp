////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** DA Conversion ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

///////////////////////////////////////////////////////////////////////////////

#define STM32F4_DA_CHANNELS 2       // number of channels
#define STM32F4_DA_FIRST_PIN 4      // channel 0 pin (A4)

//--//

BOOL DA_Initialize( DA_CHANNEL channel, INT32 precisionInBits )
{
    if (precisionInBits != 12) return FALSE;
    
    // enable DA clock
    RCC->APB1ENR |= RCC_APB1ENR_DACEN;
    // set pin as analog
    CPU_GPIO_DisablePin(STM32F4_DA_FIRST_PIN + channel, RESISTOR_DISABLED, 0, GPIO_ALT_MODE_1);
    if (channel) {
        DAC->CR |= DAC_CR_EN2; // enable channel 2
    } else {
        DAC->CR |= DAC_CR_EN1; // enable channel 1
    }
    return TRUE;
}

void DA_Uninitialize( DA_CHANNEL channel )
{
    if (channel) {
        DAC->CR &= ~DAC_CR_EN2; // disable channel 2
    } else {
        DAC->CR &= ~DAC_CR_EN1; // disable channel 1
    }
    // free pin
    CPU_GPIO_DisablePin(STM32F4_DA_FIRST_PIN + channel, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY);
    if ((DAC->CR & (DAC_CR_EN1 | DAC_CR_EN2)) == 0) { // all channels off
        // disable DA clock
        RCC->APB1ENR &= ~RCC_APB1ENR_DACEN;
    }
}

// level is a 12 bit value
void DA_Write( DA_CHANNEL channel, INT32 level )
{
    if (channel) {
        DAC->DHR12R2 = level;
    } else {
        DAC->DHR12R1 = level;
    }
}

UINT32 DA_DAChannels()
{
    return STM32F4_DA_CHANNELS;
}

GPIO_PIN DA_GetPinForChannel( DA_CHANNEL channel )
{
    if ((UINT32)channel >= STM32F4_DA_CHANNELS) return GPIO_PIN_NONE;
    return STM32F4_DA_FIRST_PIN + channel;
}

BOOL DA_GetAvailablePrecisionsForChannel( DA_CHANNEL channel, INT32* precisions, UINT32& size )
{
    size = 0;
    if (precisions == NULL || (UINT32)channel >= STM32F4_DA_CHANNELS) return FALSE;
    precisions[0] = 12;
    size = 1;
    return TRUE;
}
