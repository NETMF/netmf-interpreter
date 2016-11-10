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

///////////////////////////////////////////////////////////////////////////////

#define DA_CHANNELS 2       // number of channels

#if (!defined(USE_DAC_CHANNEL_1) && !defined(USE_DAC_CHANNEL_2) && !defined(USE_BOTH_DAC_CHANNEL)) 
#error "No DAC defines! Please go to platform_selector.h (in solution folder) and uncomment one and only one DAC define"
#endif
#if ((defined(USE_DAC_CHANNEL_2) && !defined(DAC_CHANNEL_2)) || (defined(USE_BOTH_DAC_CHANNEL) && !defined(DAC_CHANNEL_2))) 
#error "DAC channel 2 doesn't exist! Please go to platform_selector.h (in solution folder) and comment defines USE_DAC_CHANNEL_2 and USE_BOTH_DAC_CHANNEL"
#endif
#if ((defined(USE_DAC_CHANNEL_1) && defined(USE_DAC_CHANNEL_2)) || (defined(USE_DAC_CHANNEL_1) && defined(USE_BOTH_DAC_CHANNEL)) || (defined(USE_DAC_CHANNEL_2) && defined(USE_BOTH_DAC_CHANNEL))) 
#error "There can be only one define! Please go to platform_selector.h (in solution folder) and comment DAC defines less one"
#endif
//--//

DAC_HandleTypeDef DacHandle;

/**
  * @brief DAC MSP De-Initialization 
  *        This function frees the hardware resources used in this example:
  *          - Disable the Peripheral's clock
  *          - Revert GPIO to their default state
  * @param hadc: DAC handle pointer
  * @retval None
  */
void HAL_DAC_MspInit(DAC_HandleTypeDef* hdac)
{
    GPIO_InitTypeDef GPIO_InitStruct;

    /*##-1- Enable peripherals and GPIO Clocks #################################*/
    /* DAC Periph clock enable */
    __HAL_RCC_DAC_CLK_ENABLE();
    /* Enable GPIO clock ****************************************/
    __HAL_RCC_GPIOA_CLK_ENABLE();

    /*##-2- Configure peripheral GPIO ##########################################*/ 
    /* DAC Channels GPIO pin configuration */
#ifdef USE_BOTH_DAC_CHANNEL
    GPIO_InitStruct.Mode = GPIO_MODE_ANALOG;
    GPIO_InitStruct.Pull = GPIO_NOPULL;
    // channel 1 in pin PA4
    GPIO_InitStruct.Pin = GPIO_PIN_4;
    HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);
    // channel 2 in pin PA5
    GPIO_InitStruct.Pin = GPIO_PIN_5;
    HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);
#else
    #if USE_DAC_CHANNEL_2
        // channel 2 in pin PA5
        GPIO_InitStruct.Pin = GPIO_PIN_5;
    #else
        // channel 1 in pin PA4
        GPIO_InitStruct.Pin = GPIO_PIN_4;
    #endif
        GPIO_InitStruct.Mode = GPIO_MODE_ANALOG;
        GPIO_InitStruct.Pull = GPIO_NOPULL;
        HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);
#endif
}
  
/**
  * @brief  DeInitializes the DAC MSP.
  * @param  hdac: pointer to a DAC_HandleTypeDef structure that contains
  *         the configuration information for the specified DAC.  
  * @retval None
  */
void HAL_DAC_MspDeInit(DAC_HandleTypeDef* hdac)
{
    /*##-1- Reset peripherals ##################################################*/
    __HAL_RCC_DAC_FORCE_RESET();
    __HAL_RCC_DAC_RELEASE_RESET();

    /*##-2- Disable peripherals and GPIO Clocks ################################*/
    /* De-initialize the DAC Channels GPIO pin */
#ifdef USE_BOTH_DAC_CHANNEL
    HAL_GPIO_DeInit(GPIOA, GPIO_PIN_4);
    HAL_GPIO_DeInit(GPIOA, GPIO_PIN_5);
#elif USE_DAC_CHANNEL_2
    HAL_GPIO_DeInit(GPIOA, GPIO_PIN_5);
#else
    HAL_GPIO_DeInit(GPIOA, GPIO_PIN_4);
#endif
}

BOOL DA_Initialize(DA_CHANNEL channel, INT32 precisionInBits)
{
    DAC_ChannelConfTypeDef sConfig;
    
    if (precisionInBits != 12) return FALSE;

    // Configure the DAC peripheral
    DacHandle.Instance = DAC;
    // Initialize the DAC peripheral
    if(HAL_DAC_Init(&DacHandle) != HAL_OK)
    {
        // Initialization Error
        return FALSE;
    }

    // Configure DAC channel
    sConfig.DAC_Trigger = DAC_TRIGGER_NONE;
    sConfig.DAC_OutputBuffer = DAC_OUTPUTBUFFER_DISABLE;

#ifdef USE_BOTH_DAC_CHANNEL
    if (channel) 
    {
        // enable channel 2
        if(HAL_DAC_ConfigChannel(&DacHandle, &sConfig, DAC_CHANNEL_2) != HAL_OK)
        {
            // Channel configuration Error
            return FALSE;
        }
        // Enable DAC Channel
        if (HAL_DAC_Start(&DacHandle, DAC_CHANNEL_2) != HAL_OK)
        {
            // Start Error
            return FALSE;
        }
    } 
    else 
    {
        // enable channel 1
        if(HAL_DAC_ConfigChannel(&DacHandle, &sConfig, DAC_CHANNEL_1) != HAL_OK)
        {
            // Channel configuration Error
            return FALSE;
        }
        // Enable DAC Channel
        if (HAL_DAC_Start(&DacHandle, DAC_CHANNEL_1) != HAL_OK)
        {
            // Start Error
            return FALSE;
        }
    }
#else
    #if USE_DAC_CHANNEL_2
        // enable channel 2
        if(HAL_DAC_ConfigChannel(&DacHandle, &sConfig, DAC_CHANNEL_2) != HAL_OK)
        {
            // Channel configuration Error
            return FALSE;
        }
        // Enable DAC Channel
        if (HAL_DAC_Start(&DacHandle, DAC_CHANNEL_2) != HAL_OK)
        {
            // Start Error
            return FALSE;
        }
    #else
        // enable channel 1
        if(HAL_DAC_ConfigChannel(&DacHandle, &sConfig, DAC_CHANNEL_1) != HAL_OK)
        {
            // Channel configuration Error
            return FALSE;
        }
        // Enable DAC Channel
        if (HAL_DAC_Start(&DacHandle, DAC_CHANNEL_1) != HAL_OK)
        {
            // Start Error
            return FALSE;
        }
    #endif
#endif

    return TRUE;
}

void DA_Uninitialize(DA_CHANNEL channel)
{
    HAL_DAC_DeInit(&DacHandle);
}

// level is a 12 bit value
void DA_Write(DA_CHANNEL channel, INT32 level)
{
    // set value
#ifdef USE_BOTH_DAC_CHANNEL
    if (channel) 
    {
        HAL_DAC_SetValue(&DacHandle, DAC_CHANNEL_2, DAC_ALIGN_12B_R, level);
    } 
    else 
    {
        HAL_DAC_SetValue(&DacHandle, DAC_CHANNEL_1, DAC_ALIGN_12B_R, level);
    }
#else
    #if USE_DAC_CHANNEL_2
    HAL_DAC_SetValue(&DacHandle, DAC_CHANNEL_2, DAC_ALIGN_12B_R, level);
    #else
    HAL_DAC_SetValue(&DacHandle, DAC_CHANNEL_1, DAC_ALIGN_12B_R, level);
    #endif
#endif
}

UINT32 DA_DAChannels()
{
#ifdef DAC_CHANNEL_2
    return 2;
#else
    return 1;
#endif
}

GPIO_PIN DA_GetPinForChannel(DA_CHANNEL channel)
{
#ifdef USE_BOTH_DAC_CHANNEL
    if (channel) 
    {
        // DAC_CHANNEL_2 on PA5
        return GPIO_PIN_5;
    } 
    else 
    {
        // DAC_CHANNEL_2 on PA4
        return GPIO_PIN_4;
    }
#elif USE_DAC_CHANNEL_2
    return GPIO_PIN_5;
#else
    return GPIO_PIN_4;
#endif 
}

BOOL DA_GetAvailablePrecisionsForChannel(DA_CHANNEL channel, INT32* precisions, UINT32& size)
{
    // FIXME - complete code with hardware availabel precisions
    // Considering all channels with same precision
    precisions[0] = 12;
    size = 1;
    return TRUE;
}
