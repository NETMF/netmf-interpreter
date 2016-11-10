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

//--//

// indexed port configuration access
#define Port(port) ((GPIO_TypeDef *) (GPIOA_BASE + (port << 10)))

#define AD_SAMPLE_TIME 2   // sample time = 28 cycles
#define ADC_CHANNELS {ADC_CHANNEL_0,ADC_CHANNEL_1,ADC_CHANNEL_2,ADC_CHANNEL_3,ADC_CHANNEL_4,ADC_CHANNEL_5,ADC_CHANNEL_6,ADC_CHANNEL_7,ADC_CHANNEL_8,ADC_CHANNEL_9,ADC_CHANNEL_10,ADC_CHANNEL_11,ADC_CHANNEL_12,ADC_CHANNEL_13,ADC_CHANNEL_14,ADC_CHANNEL_15,ADC_CHANNEL_VREFINT,ADC_CHANNEL_VBAT}

#if defined (STM32F401xB) || defined(STM32F401xC) || defined(STM32F401xD) || defined (STM32F401xE) || defined (STM32F410Cx) || defined (STM32F410Rx) || defined (STM32F410Tx) || defined (STM32F411xC) || defined (STM32F411xE)
// these have 1 × 12-bit A/D converter
    #if USER_ADC == 1
        #define ADCx ADC1
        #define RCC_APB2ENR_ADCxEN RCC_APB2ENR_ADC1EN
        // ADC1 pins plus two internally connected channels thus the 0 for 'no pin'
        // Vsense for temperature sensor @ ADC1_IN16
        // Vrefubt for internal voltage reference (1.21V) @ ADC1_IN17
        // to access the internal channels need to include '16' and/or '17' at the AD_CHANNELS array in 'platform_selector.h'
        // PA0,PA1,PA2,PA3,PA4,PA5,PA6,PA7,PB0,PB1,PC0,PC1,PC2,PC3,PC4,PC5,INTERNAL_PIN,INTERNAL_PIN 
        #define ADC_PINS {0,1,2,3,4,5,6,7,16,17,32,33,34,35,36,37,0,0} 
    #else
        #error "wrong USER_ADC value (only 1 is valid). Please go to platform_selector.h (in solution folder) and re-define USER_ADC"
    #endif
    
#endif // defined (STM32F401xC) || defined(STM32F401xD) || defined (STM32F401xE) || defined (STM32F410Cx) || defined (STM32F410Rx) || defined (STM32F411xC) || defined (STM32F411xE) || defined (STM32F410Tx)

#if defined (STM32F405xx) || defined (STM32F407xx) || defined (STM32F415xx) || defined (STM32F417xx) || defined (STM32F427xx) || defined (STM32F429xx) || defined (STM32F437xx) || defined (STM32F439xx) || defined (STM32F446xx) || defined (STM32F469xx) || defined (STM32F479xx)
// these have 3 × 12-bit A/D converter
    #if USER_ADC == 1
        #define ADCx ADC1
        #define RCC_APB2ENR_ADCxEN RCC_APB2ENR_ADC1EN
        // ADC1 pins plus two internally connected channels thus the 0 for 'no pin'
        // Vsense for temperature sensor @ ADC1_IN16
        // Vrefubt for internal voltage reference (1.21V) @ ADC1_IN17
        // to access the internal channels need to include '16' and/or '17' at the AD_CHANNELS array in 'platform_selector.h' 
        // PA0,PA1,PA2,PA3,PA4,PA5,PA6,PA7,PB0,PB1,PC0,PC1,PC2,PC3,PC4,PC5,INTERNAL_PIN,INTERNAL_PIN
        #define ADC_PINS {0,1,2,3,4,5,6,7,16,17,32,33,34,35,36,37,0,0} 
    #elif (USER_ADC == 3 && defined(ADC3))
        #define ADCx ADC3
        #define RCC_APB2ENR_ADCxEN RCC_APB2ENR_ADC3EN
        // PA0,PA1,PA2,PA3,PF6,PF7,PF8,PF9,PF10,PF3,PC0,PC1,PC2,PC3,PF4,PF5 
        #define ADC_PINS {0,1,2,3,86,87,88,89,90,83,32,33,34,35,84,85} // ADC3 pins
    #else
        #error "wrong USER_ADC value (1 or 3). Please go to platform_selector.h (in solution folder) and re-define USER_ADC"
    #endif

#endif // defined (STM32F405xx) || defined (STM32F407xx) || defined (STM32F415xx) || defined (STM32F417xx) || defined (STM32F427xx) || defined (STM32F429xx) || defined (STM32F437xx) || defined (STM32F439xx) || defined (STM32F446xx) || defined (STM32F469xx) || defined (STM32F479xx)


// Channels
#if !defined (AD_CHANNELS)
    #error "AD_CHANNELS not defined. Please go to platform_selector.h (in solution folder) and define AD_CHANNELS"
#endif
static const BYTE g_AD_Channel[] = AD_CHANNELS;
CT_ASSERT( sizeof( g_AD_Channel ) > 0 ) // at least one channel must be set, if you get error here, please go to platform_selector.h (in solution folder) and re-define AD_CHANNELS
CT_ASSERT( sizeof( g_AD_Channel ) <= 8 ) // maximum of 8 channels can be set, if you get error here, please go to platform_selector.h (in solution folder) and re-define AD_CHANNELS

static const uint32_t g_ADC_Channel[] = ADC_CHANNELS;
static const BYTE g_AD_Pins[] = ADC_PINS;
#define AD_NUM ARRAYSIZE_CONST_EXPR(g_AD_Channel)  // number of channels
// GPIO PINs
static const GPIO_PIN g_GPIO_PINS[] = GPIO_PINS;

// ADC handlers array declaration
ADC_HandleTypeDef AdcHandlers[AD_NUM];

//--//

/* 
 * 
 */
void GetPortFromEncodedPIN(GPIO_TypeDef* port, GPIO_PIN pin)
{
    port = Port( pin >> 4 );
}

/**
  * @brief ADC MSP Initialization 
  *        This function configures the hardware resources used in this example: 
  *           - Peripheral's clock enable
  *           - Peripheral's GPIO Configuration  
  * @param huart: UART handle pointer
  * @retval None
  */
void HAL_ADC_MspInit(ADC_HandleTypeDef* hadc)
{
    GPIO_InitTypeDef GPIO_InitStruct;
    GPIO_TypeDef* port;// pointer to the actual port registers
    
    /*##-1- Enable peripherals and GPIO Clocks #################################*/
    /*##-2- Configure peripheral GPIO ##########################################*/

    /* ADC Periph clock enable */
    __HAL_RCC_ADC1_CLK_ENABLE();

    /* ADC Channel GPIO pin configuration */
    GPIO_InitStruct.Mode = GPIO_MODE_ANALOG;
    GPIO_InitStruct.Pull = GPIO_NOPULL;
    
    for(int i = 0; i < ARRAYSIZE(g_AD_Channel); i++)
    {
        if(g_AD_Channel[i] < 16) // internal channels 16 and 17 doesn't have port
        {
            GetPortFromEncodedPIN(port, g_AD_Pins[g_AD_Channel[i]]);
            GPIO_InitStruct.Pin = g_GPIO_PINS[g_AD_Channel[i]];
            if(port == GPIOA)
            {
                /* Enable GPIO clock ****************************************/
                __HAL_RCC_GPIOA_CLK_ENABLE();
                /* ADC Channel GPIO pin configuration */
                HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);
            }
            else if(port == GPIOB)
            {
                /* Enable GPIO clock ****************************************/
                __HAL_RCC_GPIOB_CLK_ENABLE();
                /* ADC Channel GPIO pin configuration */
                HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
            }
            else if(port == GPIOC)
            {
                /* Enable GPIO clock ****************************************/
                __HAL_RCC_GPIOC_CLK_ENABLE();
                /* ADC Channel GPIO pin configuration */
                HAL_GPIO_Init(GPIOC, &GPIO_InitStruct);
            }
            else if(port == GPIOF)
            {
                /* Enable GPIO clock ****************************************/
                __HAL_RCC_GPIOF_CLK_ENABLE();
                /* ADC Channel GPIO pin configuration */
                HAL_GPIO_Init(GPIOF, &GPIO_InitStruct);
            }
        }
    }
}
  
/**
  * @brief ADC MSP De-Initialization 
  *        This function frees the hardware resources used in this example:
  *          - Disable the Peripheral's clock
  *          - Revert GPIO to their default state
  * @param hadc: ADC handle pointer
  * @retval None
  */
void HAL_ADC_MspDeInit(ADC_HandleTypeDef *hadc)
{
    GPIO_TypeDef* port;// pointer to the actual port registers

    /*##-1- Reset peripherals ##################################################*/
    __HAL_RCC_ADC_FORCE_RESET();
    __HAL_RCC_ADC_RELEASE_RESET();

    /*##-2- Disable peripherals and GPIO Clocks ################################*/
    for(int i = 0; i < ARRAYSIZE(g_AD_Channel); i++)
    {
        if(g_AD_Channel[i] < 16) // internal channels 16 and 17 doesn't have port
        {
            GetPortFromEncodedPIN(port, g_AD_Pins[g_AD_Channel[i]]);
            if(port == GPIOA)
            {
                /* De-initialize the ADC Channel GPIO pin */
                HAL_GPIO_DeInit(GPIOA, g_GPIO_PINS[g_AD_Channel[i]]);
            }
            else if(port == GPIOB)
            {
                /* De-initialize the ADC Channel GPIO pin */
                HAL_GPIO_DeInit(GPIOB, g_GPIO_PINS[g_AD_Channel[i]]);
            }
            else if(port == GPIOC)
            {
                /* De-initialize the ADC Channel GPIO pin */
                HAL_GPIO_DeInit(GPIOC, g_GPIO_PINS[g_AD_Channel[i]]);
            }
            else if(port == GPIOF)
            {
                /* De-initialize the ADC Channel GPIO pin */
                HAL_GPIO_DeInit(GPIOF, g_GPIO_PINS[g_AD_Channel[i]]);
            }
        }
    }
}

BOOL AD_Initialize(ANALOG_CHANNEL channel, INT32 precisionInBits)
{
    ADC_ChannelConfTypeDef sConfig;
    
    // init this channel if it's listed in the AD_CHANNELS array
    // FIXME
    /*##-1- Configure the ADC peripheral #######################################*/
    AdcHandlers[channel].Instance = ADCx;

    AdcHandlers[channel].Init.ClockPrescaler        = ADC_CLOCKPRESCALER_PCLK_DIV4;  /* Asynchronous clock mode, input ADC clock not divided */
    AdcHandlers[channel].Init.Resolution            = ADC_RESOLUTION_12B;            /* 12-bit resolution for converted data */
    AdcHandlers[channel].Init.DataAlign             = ADC_DATAALIGN_RIGHT;           /* Right-alignment for converted data */
    AdcHandlers[channel].Init.ScanConvMode          = DISABLE;                       /* Sequencer disabled (ADC conversion on only 1 channel: channel set on rank 1) */
    AdcHandlers[channel].Init.EOCSelection          = DISABLE;           			/* EOC flag picked-up to indicate conversion end */
    AdcHandlers[channel].Init.ContinuousConvMode    = DISABLE;                       /* Continuous mode disabled to have only 1 conversion at each conversion trig */
    AdcHandlers[channel].Init.NbrOfConversion       = 1;                             /* Parameter discarded because sequencer is disabled */
    AdcHandlers[channel].Init.DiscontinuousConvMode = DISABLE;                       /* Parameter discarded because sequencer is disabled */
    AdcHandlers[channel].Init.NbrOfDiscConversion   = 0;                             /* Parameter discarded because sequencer is disabled */
    AdcHandlers[channel].Init.ExternalTrigConv      = ADC_EXTERNALTRIGCONV_T1_CC1;   /* Software start to trig the 1st conversion manually, without external event */
    AdcHandlers[channel].Init.ExternalTrigConvEdge  = ADC_EXTERNALTRIGCONVEDGE_NONE; /* Parameter discarded because software trigger chosen */
    AdcHandlers[channel].Init.DMAContinuousRequests = DISABLE;                       /* DMA one-shot mode selected (not applied to this example) */
        
    if(HAL_ADC_Init(&AdcHandlers[channel]) != HAL_OK)
    {
        // channel not available
        return FALSE;
    }
    
    /*##-2- Configure ADC regular channel ######################################*/
    /* Note: Considering IT occurring after each number of size of              */
    /*       "uhADCxConvertedValue"  ADC conversions (IT by DMA end             */
    /*       of transfer), select sampling time and ADC clock with sufficient   */
    /*       duration to not create an overhead situation in IRQHandler.        */  
    sConfig.Channel = g_ADC_Channel[g_AD_Channel[channel]];//ADCx_CHANNEL; /* Sampled channel number */
    sConfig.Rank         = 1;                              /* Rank of sampled channel number ADCx_CHANNEL */
    sConfig.SamplingTime = ADC_SAMPLETIME_3CYCLES;         /* Sampling time (number of clock cycles unit) */
    sConfig.Offset = 0;                                    /* Parameter discarded because offset correction is disabled */

    if(HAL_ADC_ConfigChannel(&AdcHandlers[channel], &sConfig) != HAL_OK)
    {
        // channel not available
        return FALSE;
    }
    return TRUE;
}

void AD_Uninitialize(ANALOG_CHANNEL channel)
{
    HAL_ADC_DeInit(&AdcHandlers[channel]);
}

INT32 AD_Read(ANALOG_CHANNEL channel)
{
    int chNum = g_AD_Channel[channel];
  
    /*##-1- Start the conversion process #######################################*/
    if (HAL_ADC_Start(&AdcHandlers[channel]) != HAL_OK)
    {
        /* Start Conversation Error */
        return 0;
    }

    /*##-2- Wait for the end of conversion #####################################*/
    /*  Before starting a new conversion, you need to check the current state of
        the peripheral; if it�s busy you need to wait for the end of current
        conversion before starting a new one.
        For simplicity reasons, this example is just waiting till the end of the
        conversion, but application may perform other tasks while conversion
        operation is ongoing. */
    if (HAL_ADC_PollForConversion(&AdcHandlers[channel], 10) != HAL_OK)
    {
        /* End Of Conversion flag not set on time */
        return 0;
    }

    /* Check if the continuous conversion of regular channel is finished */
    if ((HAL_ADC_GetState(&AdcHandlers[channel]) & HAL_ADC_STATE_EOC_REG) == HAL_ADC_STATE_EOC_REG)
    {
        /*##-3- Get the converted value of regular channel  ########################*/
        return HAL_ADC_GetValue(&AdcHandlers[channel]);
    }

    // channel not available
    return 0;
}

UINT32 AD_ADChannels()
{
    return AD_NUM;
}

GPIO_PIN AD_GetPinForChannel(ANALOG_CHANNEL channel)
{
    // return GPIO pin
    // for internally connected channels this is GPIO_PIN_NONE as these don't take any GPIO pins
    int chNum = g_AD_Channel[channel];

    for (int i = 0; i < AD_NUM ; i++) {
        if (g_AD_Channel[i] == chNum) {
            return (GPIO_PIN)g_AD_Pins[chNum];
        }
    }

    // channel not available
    return GPIO_PIN_NONE;
}

BOOL AD_GetAvailablePrecisionsForChannel(ANALOG_CHANNEL channel, INT32* precisions, UINT32& size)
{
    int chNum = g_AD_Channel[channel];

    // check if this channel is listed in the AD_CHANNELS array
    for (int i = 0; i < AD_NUM ; i++) {
        if (g_AD_Channel[i] == chNum) {
            precisions[0] = 12;
            size = 1;
            return TRUE;
        }
    }

    // channel not available
    size = 0;
    return FALSE;
}
