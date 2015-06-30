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
    // ADC1 pins plus two internally connected channels thus the 0 for 'no pin'
    // Vsense for temperature sensor @ ADC1_IN16
    // Vrefubt for internal voltage reference (1.21V) @ ADC1_IN17
    // to access the internal channels need to include '16' and/or '17' at the STM32F4_AD_CHANNELS array in 'platform_selector.h' 
    #define STM32F4_ADC_PINS {0,1,2,3,4,5,6,7,16,17,32,33,34,35,36,37,0,0} 
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
    int chNum = g_STM32F4_AD_Channel[channel];

    // init this channel if it's listed in the STM32F4_AD_CHANNELS array
    for (int i = 0; i < STM32F4_AD_NUM ; i++) {
        if (g_STM32F4_AD_Channel[i] == chNum) {
            // valid channel
            if (!(RCC->APB2ENR & RCC_APB2ENR_ADCxEN)) { // not yet initialized
                RCC->APB2ENR |= RCC_APB2ENR_ADCxEN; // enable AD clock
                ADC->CCR = 0; // ADCCLK = PB2CLK / 2;
                ADCx->SQR1 = 0; // 1 conversion
                ADCx->CR1 = 0;
                ADCx->CR2 = ADC_CR2_ADON; // AD on
                ADCx->SMPR1 = 0x01249249 * STM32F4_AD_SAMPLE_TIME;
                ADCx->SMPR2 = 0x09249249 * STM32F4_AD_SAMPLE_TIME;
            }
            
            // set pin as analog input if channel is not one of the internally connected
            if(chNum <= 15) {
                CPU_GPIO_DisablePin(AD_GetPinForChannel(channel), RESISTOR_DISABLED, 0, GPIO_ALT_MODE_1);
                return TRUE;        
            }
        }
    }
	
    // channel not available
    return FALSE;
}

void AD_Uninitialize( ANALOG_CHANNEL channel )
{
    int chNum = g_STM32F4_AD_Channel[channel];

    // free GPIO pin if this channel is listed in the STM32F4_AD_CHANNELS array 
    // and if it's not one of the internally connected ones as these channels don't take any GPIO pins
    if(chNum <= 15) {
        CPU_GPIO_DisablePin(AD_GetPinForChannel(channel), RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY);
    }
}

INT32 AD_Read( ANALOG_CHANNEL channel )
{
    int chNum = g_STM32F4_AD_Channel[channel];
  
    // check if this channel is listed in the STM32F4_AD_CHANNELS array
    for (int i = 0; i < STM32F4_AD_NUM ; i++) {
        if (g_STM32F4_AD_Channel[i] == chNum ) {
            // valid channel
            int x = ADCx->DR; // clear EOC flag

            ADCx->SQR3 = chNum; // select channel
        
            // need to enable internal reference at ADC->CCR register to work with internally connected channels 
            if(chNum == 16 || chNum == 17) {
                ADC->CCR |= ADC_CCR_TSVREFE; // Enable internal reference to work with temperature sensor and VREFINT channels
            }
    
            ADCx->CR2 |= ADC_CR2_SWSTART; // start AD
            while (!(ADCx->SR & ADC_SR_EOC)); // wait for completion
    
            // disable internally reference
            if(chNum == 16 || chNum == 17) {
                ADC->CCR &= ~ADC_CCR_TSVREFE; 
            }
    
            return ADCx->DR; // read result
        }
    }

    // channel not available
    return 0;
}

UINT32 AD_ADChannels()
{
    return STM32F4_AD_NUM;
}

GPIO_PIN AD_GetPinForChannel( ANALOG_CHANNEL channel )
{
    // return GPIO pin
    // for internally connected channels this is GPIO_PIN_NONE as these don't take any GPIO pins
    int chNum = g_STM32F4_AD_Channel[channel];

    for (int i = 0; i < STM32F4_AD_NUM ; i++) {
        if (g_STM32F4_AD_Channel[i] == chNum) {
            return (GPIO_PIN)g_STM32F4_AD_Pins[chNum];
        }
    }

    // channel not available
    return GPIO_PIN_NONE;
}

BOOL AD_GetAvailablePrecisionsForChannel( ANALOG_CHANNEL channel, INT32* precisions, UINT32& size )
{
    int chNum = g_STM32F4_AD_Channel[channel];

    // check if this channel is listed in the STM32F4_AD_CHANNELS array
    for (int i = 0; i < STM32F4_AD_NUM ; i++) {
        if (g_STM32F4_AD_Channel[i] == chNum) {
            precisions[0] = 12;
            size = 1;
            return TRUE;
        }
    }

    // channel not available
    size = 0;
    return FALSE;
}
