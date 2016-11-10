////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for CMSIS: Copyright (c) Oberon microsystems, Inc.
//
//  *** Serial Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if (!defined(USART_PORTS) || !defined(USART_PORTS_TX_RX)) 
#error "No USART ports defined! Please go to platform_selector.h (in solution folder) and uncomment the define that matches the USART ports available in the device datasheet"
#endif

// IO addresses
static const USART_TypeDef* g_Uart_Ports[] = USART_PORTS;
static const GPIO_TypeDef* g_Uart_Ports_Tx_Rx[] = USART_PORTS_TX_RX;

// Pins
static BYTE g_Uart_RxD_Pins[ARRAYSIZE(g_Uart_Ports)];
static BYTE g_Uart_TxD_Pins[ARRAYSIZE(g_Uart_Ports)];
#ifdef USART_PORTS_FLOW_CONTROL
    static const GPIO_TypeDef* g_Uart_Ports_Flow_Control[] = USART_PORTS_FLOW_CONTROL;
    static BYTE g_Uart_CTS_Pins[ARRAYSIZE(g_Uart_Ports)];
    static BYTE g_Uart_RTS_Pins[ARRAYSIZE(g_Uart_Ports)];
#endif

static UART_HandleTypeDef * uartHandlers[ARRAYSIZE(g_Uart_Ports)];

#define CHECK_ARRAY_SIZE(condition) ((void)sizeof(char[(-1) + 1*!!(condition)]))

void DummyToPerformArrayCheck()
{
    // g_Uart_Ports and g_Uart_Ports_Tx_Rx (and g_Uart_Ports_Flow_Control, if defined) arrays size must match
    // Please go to platform_selector.h (in solution folder) and check related arrays 
    CHECK_ARRAY_SIZE(sizeof(g_Uart_Ports) == sizeof(g_Uart_Ports_Tx_Rx));
    #ifdef USART_PORTS_FLOW_CONTROL
        CHECK_ARRAY_SIZE(sizeof(g_Uart_Ports) == sizeof(g_Uart_Ports_Flow_Control));
    #endif
}

/**
  * @brief Get USART/UART Index 
  *        This function goes throw g_Uart_Ports to find uart index
  * @param uart: UART
  * @retval uart index in g_Uart_Ports array
  */
int GetUsartIndex(USART_TypeDef* uart)
{
    for(int i = 0; i < ARRAYSIZE(g_Uart_Ports); i++)
    {
        if(g_Uart_Ports[i] == uart)
        {
            return i;
        }
    }
    // need a default
    return 0;
}

void USART_Handle_RX_IRQ (int comPortNum, USART_TypeDef* uart)
{
    INTERRUPT_START;

    char c = (char)(uart->DR); // read RX data
    USART_AddCharToRxBuffer(comPortNum, c);
    Events_Set(SYSTEM_EVENT_FLAG_COM_IN);

    INTERRUPT_END;
}

void USART_Handle_TX_IRQ (int comPortNum, USART_TypeDef* uart)
{
    INTERRUPT_START;

    char c;
    if (USART_RemoveCharFromTxBuffer(comPortNum, c)) {
        uart->DR = c;  // write TX data
    } else {
        uart->CR1 &= ~USART_CR1_TXEIE; // TX int disable
    }
    Events_Set(SYSTEM_EVENT_FLAG_COM_OUT);

    INTERRUPT_END;
}

BOOL ComputeUSARTPins()
{
    // fill arrays with user USART ports definitions
    for(int i = 0; i < ARRAYSIZE(g_Uart_Ports); i++)
    {
        if(g_Uart_Ports[i] == USART1)
        {
            if(g_Uart_Ports_Tx_Rx[i] == GPIOA)
            {
                g_Uart_RxD_Pins[i] = 10; // PA10
                g_Uart_TxD_Pins[i] = 9;  // PA9
            }
            else if(g_Uart_Ports_Tx_Rx[i] == GPIOB)
            {
                g_Uart_RxD_Pins[i] = 23;  // PB7
                g_Uart_TxD_Pins[i] = 22;  // PB6
            }
            else
            {
                // invalid port selection
                return FALSE;
            }
                
            #ifdef USART_PORTS_FLOW_CONTROL
                if(g_Uart_Ports_Flow_Control[i] != GPIO_NO_PORT && g_Uart_Ports_Flow_Control[i] == GPIOA)
                {
                    g_Uart_CTS_Pins[i] = 11; // PA11
                    g_Uart_RTS_Pins[i] = 12; // PA12
                }
                else if(g_Uart_Ports_Flow_Control[i] != GPIO_NO_PORT)
                {
                    // invalid port selection
                    return FALSE;
                } 
            #endif
        }
        #ifdef USART2
        else if(g_Uart_Ports[i] == USART2)
        {
            if(g_Uart_Ports_Tx_Rx[i] == GPIOA)
            {
                g_Uart_RxD_Pins[i] = 3;  // PA3
                g_Uart_TxD_Pins[i] = 2;  // PA2
            }
            else if(g_Uart_Ports_Tx_Rx[i] == GPIOD)
            {
                g_Uart_RxD_Pins[i] = 54;  // PD6
                g_Uart_TxD_Pins[i] = 53;  // PD5
            }
            else
            {
                // invalid port selection
                return FALSE;
            }
                
            #ifdef USART_PORTS_FLOW_CONTROL
                if(g_Uart_Ports_Flow_Control[i] != GPIO_NO_PORT)
                {
                    if(g_Uart_Ports_Flow_Control[i] == GPIOA)
                    {
                        g_Uart_CTS_Pins[i] = 1; // PA1
                        g_Uart_RTS_Pins[i] = 0; // PA0
                    }
                    else if(g_Uart_Ports_Flow_Control[i] == GPIOA)
                    {
                        g_Uart_CTS_Pins[i] = 52; // PD4
                        g_Uart_RTS_Pins[i] = 51; // PD3
                    }
                    else
                    {
                        // invalid port selection
                        return FALSE;
                    }
                }
            #endif
        }
        #endif
        #ifdef USART3
        else if(g_Uart_Ports[i] == USART3)
        {
            if(g_Uart_Ports_Tx_Rx[i] == GPIOB)
            {
                g_Uart_RxD_Pins[i] = 27;  // PB11
                g_Uart_TxD_Pins[i] = 26;  // PB10
            }
            else if(g_Uart_Ports_Tx_Rx[i] == GPIOC)
            {
                g_Uart_RxD_Pins[i] = 43;  // PC11
                g_Uart_TxD_Pins[i] = 42;  // PC10
            }
            else if(g_Uart_Ports_Tx_Rx[i] == GPIOD)
            {
                g_Uart_RxD_Pins[i] = 57;  // PD9
                g_Uart_TxD_Pins[i] = 56;  // PD8
            }
            else
                // shouldn't get here!
                return FALSE;
                
            #ifdef USART_PORTS_FLOW_CONTROL
                if(g_Uart_Ports_Flow_Control[i] != GPIO_NO_PORT)
                {
                    if(g_Uart_Ports_Flow_Control[i] == GPIOB)
                    {
                        g_Uart_CTS_Pins[i] = 30; // PB14
                        g_Uart_RTS_Pins[i] = 29; // PB13
                    }
                    else if(g_Uart_Ports_Flow_Control[i] == GPIOD)
                    {
                        g_Uart_CTS_Pins[i] = 60; // PD12
                        g_Uart_RTS_Pins[i] = 59; // PD11
                    }
                    else
                    {
                        // invalid port selection
                        return FALSE;
                    }
                }
            #endif
        }
        #endif
        #ifdef UART4
        else if(g_Uart_Ports[i] == UART4)
        {
            if(g_Uart_Ports_Tx_Rx[i] == GPIOA)
            {
                g_Uart_RxD_Pins[i] = 1;  // PA1
                g_Uart_TxD_Pins[i] = 0;  // PA0
            }
            else if(g_Uart_Ports_Tx_Rx[i] == GPIOC)
            {
                g_Uart_RxD_Pins[i] = 43;  // PC11
                g_Uart_TxD_Pins[i] = 42;  // PC10
            }
            else
            {
                // invalid port selection
                return FALSE;
            }
        }
        #endif
        #ifdef USART6
        else if(g_Uart_Ports[i] == USART6)
        {
            if(g_Uart_Ports_Tx_Rx[i] == GPIOC)
            {
                g_Uart_RxD_Pins[i] = 39;  // PC7
                g_Uart_TxD_Pins[i] = 38;  // PC6
            }
            else if(g_Uart_Ports_Tx_Rx[i] == GPIOG)
            {
                g_Uart_RxD_Pins[i] = 105;  // PG9
                g_Uart_TxD_Pins[i] = 110;  // PG14
            }
            else
            {
                // invalid port selection
                return FALSE;
            }
            #ifdef USART_PORTS_FLOW_CONTROL
                if(g_Uart_Ports_Flow_Control[i] != GPIO_NO_PORT && g_Uart_Ports_Flow_Control[i] == GPIOG)
                {
                        g_Uart_CTS_Pins[i] = 109; // PG13
                        g_Uart_RTS_Pins[i] = 108; // PG12
                }
                else if(g_Uart_Ports_Flow_Control[i] != GPIO_NO_PORT)
                {
                    // invalid port selection
                    return FALSE;
                }
            #endif
        }
        #endif
        #ifdef UART7
        else if(g_Uart_Ports[i] == UART7)
        {
            if(g_Uart_Ports_Tx_Rx[i] == GPIOE)
            {
                g_Uart_RxD_Pins[i] = 71;  // PE7
                g_Uart_TxD_Pins[i] = 72;  // PE8
            }
            else if(g_Uart_Ports_Tx_Rx[i] == GPIOF)
            {
                g_Uart_RxD_Pins[i] = 86;  // PF6
                g_Uart_TxD_Pins[i] = 87;  // PF7
            }
            else
            {
                // invalid port selection
                return FALSE;
            }
        }
        #endif
        #ifdef UART8
        else if(g_Uart_Ports[i] == UART8)
        {
            if(g_Uart_Ports_Tx_Rx[i] == GPIOE)
            {
                g_Uart_RxD_Pins[i] = 64;  // PE0
                g_Uart_TxD_Pins[i] = 65;  // PE1
            }
            else
            {
                // invalid port selection
                return FALSE;
            }
        }
        #endif
    }
}

void EnableUSARTGPIOClocks(int comPortNum)
{
    // Enable GPIO TX/RX clock
    if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOA)
    {
        __HAL_RCC_GPIOA_CLK_ENABLE();
    }
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOB)
    {
        __HAL_RCC_GPIOB_CLK_ENABLE();
    }
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOC)
    {
        __HAL_RCC_GPIOC_CLK_ENABLE();
    }
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOD)
    {
        __HAL_RCC_GPIOD_CLK_ENABLE();
    }
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOE)
    {
        __HAL_RCC_GPIOE_CLK_ENABLE();
    }
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOF)
    {
        __HAL_RCC_GPIOF_CLK_ENABLE();
    }
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOG)
    {
        __HAL_RCC_GPIOG_CLK_ENABLE();
    }
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOH)
    {
        __HAL_RCC_GPIOH_CLK_ENABLE();
    }
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOI)
    {
        __HAL_RCC_GPIOI_CLK_ENABLE();
    }
    #ifdef GPIOJ
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOJ)
    {
        __HAL_RCC_GPIOJ_CLK_ENABLE();
    }
    #endif
    #ifdef GPIOK
    else if(g_Uart_Ports_Tx_Rx[comPortNum] == GPIOK)
    {
        __HAL_RCC_GPIOK_CLK_ENABLE();
    }
    #endif
    
    // Enable USART1 clock
    if(g_Uart_Ports[comPortNum] == USART1)
    {
        __HAL_RCC_USART1_CLK_ENABLE();
    }
    #ifdef USART2
    else if(g_Uart_Ports[comPortNum] == USART2)
    {
        __HAL_RCC_USART2_CLK_ENABLE();
    }
    #endif
    #ifdef USART3
    else if(g_Uart_Ports[comPortNum] == USART3)
    {
        __HAL_RCC_USART3_CLK_ENABLE();
    }
    #endif
    #ifdef UART4
    else if(g_Uart_Ports[comPortNum] == UART4)
    {
        __HAL_RCC_UART4_CLK_ENABLE();
    }
    #endif
    #ifdef USART6
    else if(g_Uart_Ports[comPortNum] == USART6)
    {
        __HAL_RCC_USART6_CLK_ENABLE();
    }
    #endif
    #ifdef UART7
    else if(g_Uart_Ports[comPortNum] == UART7)
    {
        __HAL_RCC_UART7_CLK_ENABLE();
    }
    #endif
    #ifdef UART8
    else if(g_Uart_Ports[comPortNum] == UART8)
    {
        __HAL_RCC_UART8_CLK_ENABLE();
    }
    #endif
}

void ConfigureUSARTGPIOPin(int comPortNum)
{
    GPIO_InitTypeDef  GPIO_InitStruct;
    
    // USART/UART alternate function
    if(g_Uart_Ports[comPortNum] == USART1)
    {
        GPIO_InitStruct.Alternate = GPIO_AF7_USART1;
    }
    #ifdef USART2
    else if(g_Uart_Ports[comPortNum] == USART2)
    {
        GPIO_InitStruct.Alternate = GPIO_AF7_USART2;
    }
    #endif
    #ifdef USART3
    else if(g_Uart_Ports[comPortNum] == USART3)
    {
        GPIO_InitStruct.Alternate = GPIO_AF7_USART3;
    }
    #endif
    #ifdef UART4
    else if(g_Uart_Ports[comPortNum] == UART4)
    {
        GPIO_InitStruct.Alternate = GPIO_AF8_UART4;
    }
    #endif
    #ifdef USART6
    else if(g_Uart_Ports[comPortNum] == USART6)
    {
        GPIO_InitStruct.Alternate = GPIO_AF8_USART6;
    }
    #endif
    #ifdef UART7
    else if(g_Uart_Ports[comPortNum] == UART7)
    {
        GPIO_InitStruct.Alternate = GPIO_AF8_UART7;
    }
    #endif
    #ifdef UART8
    else if(g_Uart_Ports[comPortNum] == UART8)
    {
        GPIO_InitStruct.Alternate = GPIO_AF8_UART8;
    }
    #endif

    // UART TX GPIO pin configuration
    GPIO_InitStruct.Pin       = g_Uart_TxD_Pins[comPortNum];
    GPIO_InitStruct.Mode      = GPIO_MODE_AF_PP;
    GPIO_InitStruct.Pull      = GPIO_NOPULL;
    GPIO_InitStruct.Speed     = GPIO_SPEED_FAST;

    HAL_GPIO_Init((GPIO_TypeDef*)g_Uart_Ports_Tx_Rx[comPortNum], &GPIO_InitStruct);

    // UART RX GPIO pin configuration
    GPIO_InitStruct.Pin = g_Uart_RxD_Pins[comPortNum];

    HAL_GPIO_Init((GPIO_TypeDef*)g_Uart_Ports_Tx_Rx[comPortNum], &GPIO_InitStruct);
}

void SetUSARTPriority(int comPortNum)
{
    // USART/UART alternate function
    if(g_Uart_Ports[comPortNum] == USART1)
    {
        HAL_NVIC_SetPriority(USART1_IRQn, USART1_PRIORITY, 1);
    }
    #ifdef USART2
    else if(g_Uart_Ports[comPortNum] == USART2)
    {
        HAL_NVIC_SetPriority(USART2_IRQn, USART2_PRIORITY, 1);
    }
    #endif
    #ifdef USART3
    else if(g_Uart_Ports[comPortNum] == USART3)
    {
        HAL_NVIC_SetPriority(USART3_IRQn, USART3_PRIORITY, 1);
    }
    #endif
    #ifdef UART4
    else if(g_Uart_Ports[comPortNum] == UART4)
    {
        HAL_NVIC_SetPriority(UART4_IRQn, USART4_PRIORITY, 1);
    }
    #endif
    #ifdef USART6
    else if(g_Uart_Ports[comPortNum] == USART6)
    {
        HAL_NVIC_SetPriority(USART6_IRQn, USART6_PRIORITY, 1);
    }
    #endif
    #ifdef UART7
    else if(g_Uart_Ports[comPortNum] == UART7)
    {
        HAL_NVIC_SetPriority(UART7_IRQn, USART7_PRIORITY, 1);
    }
    #endif
    #ifdef UART8
    else if(g_Uart_Ports[comPortNum] == UART8)
    {
        HAL_NVIC_SetPriority(UART8_IRQn, USART8_PRIORITY, 1);
    }
    #endif
}

void EnableUSARTInt(int comPortNum)
{
    if(g_Uart_Ports[comPortNum] == USART1)
    {
        HAL_NVIC_EnableIRQ(USART1_IRQn);
    }
    #ifdef USART2
    else if(g_Uart_Ports[comPortNum] == USART2)
    {
        HAL_NVIC_EnableIRQ(USART2_IRQn);
    }
    #endif
    #ifdef USART3
    else if(g_Uart_Ports[comPortNum] == USART3)
    {
        HAL_NVIC_EnableIRQ(USART3_IRQn);
    }
    #endif
    #ifdef UART4
    else if(g_Uart_Ports[comPortNum] == UART4)
    {
        HAL_NVIC_EnableIRQ(UART4_IRQn);
    }
    #endif
    #ifdef USART6
    else if(g_Uart_Ports[comPortNum] == USART6)
    {
        HAL_NVIC_EnableIRQ(USART6_IRQn);
    }
    #endif
    #ifdef UART7
    else if(g_Uart_Ports[comPortNum] == UART7)
    {
        HAL_NVIC_EnableIRQ(UART7_IRQn);
    }
    #endif
    #ifdef UART8
    else if(g_Uart_Ports[comPortNum] == UART8)
    {
        HAL_NVIC_EnableIRQ(UART8_IRQn);
    }
    #endif
}

void DisableUSARTInt(int comPortNum)
{
    if(g_Uart_Ports[comPortNum] == USART1)
    {
        HAL_NVIC_DisableIRQ(USART1_IRQn);
    }
    #ifdef USART2
    else if(g_Uart_Ports[comPortNum] == USART2)
    {
        HAL_NVIC_DisableIRQ(USART2_IRQn);
    }
    #endif
    #ifdef USART3
    else if(g_Uart_Ports[comPortNum] == USART3)
    {
        HAL_NVIC_DisableIRQ(USART3_IRQn);
    }
    #endif
    #ifdef UART4
    else if(g_Uart_Ports[comPortNum] == UART4)
    {
        HAL_NVIC_DisableIRQ(UART4_IRQn);
    }
    #endif
    #ifdef USART6
    else if(g_Uart_Ports[comPortNum] == USART6)
    {
        HAL_NVIC_DisableIRQ(USART6_IRQn);
    }
    #endif
    #ifdef UART7
    else if(g_Uart_Ports[comPortNum] == UART7)
    {
        HAL_NVIC_DisableIRQ(UART7_IRQn);
    }
    #endif
    #ifdef UART8
    else if(g_Uart_Ports[comPortNum] == UART8)
    {
        HAL_NVIC_DisableIRQ(UART8_IRQn);
    }
    #endif
}

uint32_t GetUSARTInterruptEnableState(int comPortNum)
{
    if(g_Uart_Ports[comPortNum] == USART1)
    {
        return HAL_NVIC_GetActive(USART1_IRQn);
    }
    #ifdef USART2
    else if(g_Uart_Ports[comPortNum] == USART2)
    {
        return HAL_NVIC_GetActive(USART2_IRQn);
    }
    #endif
    #ifdef USART3
    else if(g_Uart_Ports[comPortNum] == USART3)
    {
        return HAL_NVIC_GetActive(USART3_IRQn);
    }
    #endif
    #ifdef UART4
    else if(g_Uart_Ports[comPortNum] == UART4)
    {
        return HAL_NVIC_GetActive(UART4_IRQn);
    }
    #endif
    #ifdef USART6
    else if(g_Uart_Ports[comPortNum] == USART6)
    {
        return HAL_NVIC_GetActive(USART6_IRQn);
    }
    #endif
    #ifdef UART7
    else if(g_Uart_Ports[comPortNum] == UART7)
    {
        return HAL_NVIC_GetActive(UART7_IRQn);
    }
    #endif
    #ifdef UART8
    else if(g_Uart_Ports[comPortNum] == UART8)
    {
        return HAL_NVIC_GetActive(UART8_IRQn);
    }
    #endif
    
    // shouldn't reach here
    return -1;
}

/**
  * @brief UART MSP Initialization 
  *        This function configures the hardware resources used:
  *           - Compute peripherals pin arrays (only once)
  *           - Peripheral's clock enable
  *           - Peripheral's GPIO Configuration  
  * @param huart: UART handle pointer
  * @retval None
  */
void HAL_UART_MspInit(UART_HandleTypeDef *huart)
{
    // get usart index
    int index = GetUsartIndex(huart->Instance);

    if(g_Uart_RxD_Pins[0] == 0)
    {
        //  need to fill USART pin arrays
        ComputeUSARTPins();
    }
    
    // Enable GPIO TX/RX clock and USART clock
    EnableUSARTGPIOClocks(index);
    // UART TX/RX GPIO pin configuration
    ConfigureUSARTGPIOPin(index);
    // Set USARTx priority
    SetUSARTPriority(index);
}

/**
  * @brief UART MSP De-Initialization 
  *        This function frees the hardware resources used in this example:
  *          - Disable the Peripheral's clock
  *          - Revert GPIO configuration to their default state
  * @param huart: UART handle pointer
  * @retval None
  */
void HAL_UART_MspDeInit(UART_HandleTypeDef *huart)
{
    // get usart index
    int index = GetUsartIndex(huart->Instance);
    /*##-1- Reset peripherals ##################################################*/
    if(huart->Instance == USART1)
    {
        __HAL_RCC_USART1_FORCE_RESET();
        __HAL_RCC_USART1_RELEASE_RESET();
    }
    #ifdef USART2
    else if(huart->Instance == USART2)
    {
        __HAL_RCC_USART2_FORCE_RESET();
        __HAL_RCC_USART2_RELEASE_RESET();
    }
    #endif
    #ifdef USART3
    else if(huart->Instance == USART3)
    {
        __HAL_RCC_USART3_FORCE_RESET();
        __HAL_RCC_USART3_RELEASE_RESET();
    }
    #endif
    #ifdef UART4
    else if(huart->Instance == UART4)
    {
        __HAL_RCC_UART4_FORCE_RESET();
        __HAL_RCC_UART4_RELEASE_RESET();
    }
    #endif
    #ifdef USART6
    else if(huart->Instance == USART6)
    {
        __HAL_RCC_USART6_FORCE_RESET();
        __HAL_RCC_USART6_RELEASE_RESET();
    }
    #endif
    #ifdef UART7
    else if(huart->Instance == UART7)
    {
        __HAL_RCC_UART7_FORCE_RESET();
        __HAL_RCC_UART7_RELEASE_RESET();
    }
    #endif
    #ifdef UART8
    else if(huart->Instance == UART8)
    {
        __HAL_RCC_UART8_FORCE_RESET();
        __HAL_RCC_UART8_RELEASE_RESET();
    }
    #endif

    /*##-2- Disable peripherals and GPIO Clocks ################################*/
    /* Configure UART Tx as alternate function */
    HAL_GPIO_DeInit((GPIO_TypeDef*)g_Uart_Ports_Tx_Rx[index], g_Uart_TxD_Pins[index]);
    /* Configure UART Rx as alternate function */
    HAL_GPIO_DeInit((GPIO_TypeDef*)g_Uart_Ports_Tx_Rx[index], g_Uart_RxD_Pins[index]);
}

BOOL CPU_USART_Initialize(int comPortNum, int baudRate, int parity, int dataBits, int stopBits, int flowValue)
{
    UART_HandleTypeDef UartHandle;


    uartHandlers[comPortNum];
    
    if (comPortNum >= TOTAL_USART_PORT)
        return FALSE;

    if (parity >= USART_PARITY_MARK)
        return FALSE;
    
    GLOBAL_LOCK(irq);
    
    UartHandle.Instance          = (USART_TypeDef*)g_Uart_Ports[comPortNum];
    UartHandle.Init.BaudRate     = baudRate;
    // UART Number of Stop Bits
    if (stopBits == USART_STOP_BITS_TWO)
    {
        UartHandle.Init.StopBits = USART_CR2_STOP_1;    
    }
    else
    {
        UartHandle.Init.StopBits = 0;
    }
    // UART Parity
    if(parity == UART_PARITY_ODD)
    {
        UartHandle.Init.Parity   = USART_CR1_PCE | USART_CR1_PS;
    }
    else if(parity == UART_PARITY_EVEN)
    {
        UartHandle.Init.Parity   = USART_CR1_PCE;
        dataBits++;
    }
    else
    {
        UartHandle.Init.Parity = 0;
    }
    // UART Word Length
    if (dataBits == 9)
        UartHandle.Init.WordLength = USART_CR1_M;
    else
    {
        if (dataBits != 8)
            return FALSE;
        UartHandle.Init.WordLength   = 0;
    }
    // UART Hardware Flow Control
    if (flowValue & USART_FLOW_NONE)
    {
        UartHandle.Init.HwFlowCtl |= 0;
    }
    if (flowValue & USART_FLOW_HW_OUT_EN)
    {
        UartHandle.Init.HwFlowCtl |= USART_CR3_CTSE;
    }
    if (flowValue & USART_FLOW_HW_IN_EN)
    {
        UartHandle.Init.HwFlowCtl |= USART_CR3_RTSE;
    }   

    UartHandle.Init.Mode         = UART_MODE_TX_RX;
    UartHandle.Init.OverSampling = UART_OVERSAMPLING_16;
      
    // // control
    // UINT16 ctrl = USART_CR1_TE | USART_CR1_RE;
    // if (Parity) {
    //     ctrl |= USART_CR1_PCE;
    //     DataBits++;
    // }

    // if (Parity == USART_PARITY_ODD)
    //     ctrl |= USART_CR1_PS;

    // if (DataBits == 9)
    //     ctrl |= USART_CR1_M;
    // else
    // {
    //     if (DataBits != 8)
    //         return FALSE;
    // }
    // uart->CR1 = ctrl;
    
    // if (StopBits == USART_STOP_BITS_ONE)
    //     StopBits = 0;

    // uart->CR2 = (UINT16)(StopBits << 12);
    
    // ctrl = 0;
    // if (FlowValue & USART_FLOW_HW_OUT_EN)
    //     ctrl |= USART_CR3_CTSE;

    // if (FlowValue & USART_FLOW_HW_IN_EN)
    //     ctrl |= USART_CR3_RTSE;

    // uart->CR3 = ctrl;

    // GPIO_PIN rxPin, txPin, ctsPin, rtsPin;
    // CPU_USART_GetPins(comPortNum, rxPin, txPin, ctsPin, rtsPin);
    // UINT32 alternate = 0x72; // AF7 = USART1-3
    // if (comPortNum >= 3)
    //     alternate = 0x82; // AF8 = UART4-8

    // CPU_GPIO_DisablePin(rxPin, RESISTOR_PULLUP, 0, (GPIO_ALT_MODE)alternate);
    // CPU_GPIO_DisablePin(txPin, RESISTOR_DISABLED, 1, (GPIO_ALT_MODE)alternate);
    // if (FlowValue & USART_FLOW_HW_OUT_EN)
    // {
    //     if (ctsPin == GPIO_PIN_NONE)
    //         return FALSE;

    //     CPU_GPIO_DisablePin(ctsPin, RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)alternate);
    // }

    // if (FlowValue & USART_FLOW_HW_IN_EN)
    // {
    //     if (rtsPin == GPIO_PIN_NONE)
    //         return FALSE;

    //     CPU_GPIO_DisablePin(rtsPin, RESISTOR_DISABLED, 1, (GPIO_ALT_MODE)alternate);
    // }
    
    // CPU_USART_ProtectPins(comPortNum, FALSE);
    
    if(HAL_UART_Init(&UartHandle) != HAL_OK)
    {
        return FALSE;
    }

    // Enable the NVIC for UART
    EnableUSARTInt(comPortNum);

    return TRUE;
}

BOOL CPU_USART_Uninitialize(int comPortNum)
{
    GLOBAL_LOCK(irq);
    
    ((USART_TypeDef*)g_Uart_Ports[comPortNum])->CR1 = 0; // stop uart

    // Disable the NVIC for UART
    DisableUSARTInt(comPortNum);

    return TRUE;
}

BOOL CPU_USART_TxBufferEmpty(int comPortNum)
{
    if (g_Uart_Ports[comPortNum]->SR & USART_SR_TXE)
        return TRUE;

    return FALSE;
}

BOOL CPU_USART_TxShiftRegisterEmpty(int comPortNum)
{
    if (g_Uart_Ports[comPortNum]->SR & USART_SR_TC)
        return TRUE;

    return FALSE;
}

void CPU_USART_WriteCharToTxBuffer(int comPortNum, UINT8 c)
{   
#ifdef DEBUG
    ASSERT(CPU_USART_TxBufferEmpty(comPortNum));
#endif
    ((USART_TypeDef*)g_Uart_Ports[comPortNum])->DR = c;
}

void CPU_USART_TxBufferEmptyInterruptEnable(int comPortNum, BOOL enable)
{
    USART_TypeDef* uart = (USART_TypeDef*)g_Uart_Ports[comPortNum];
    if (enable) 
    {
        uart->CR1 |= USART_CR1_TXEIE;  // tx int enable
    } 
    else
    {
        uart->CR1 &= ~USART_CR1_TXEIE; // tx int disable
    }
}

BOOL CPU_USART_TxBufferEmptyInterruptState(int comPortNum)
{
    if (g_Uart_Ports[comPortNum]->CR1 & USART_CR1_TXEIE)
        return TRUE;

    return FALSE;
}

void CPU_USART_RxBufferFullInterruptEnable(int comPortNum, BOOL enable)
{
    USART_TypeDef* uart = (USART_TypeDef*)g_Uart_Ports[comPortNum];
    if (enable) 
    {
        uart->CR1 |= USART_CR1_RXNEIE;  // rx int enable
    } 
    else 
    {
        uart->CR1 &= ~USART_CR1_RXNEIE; // rx int disable
    }
}

BOOL CPU_USART_RxBufferFullInterruptState(int comPortNum)
{
    if (g_Uart_Ports[comPortNum]->CR1 & USART_CR1_RXNEIE)
        return TRUE;

    return FALSE;
}

BOOL CPU_USART_TxHandshakeEnabledState(int comPortNum)
{
    // The state of the CTS input only matters if Flow Control is enabled
#ifdef USART_PORTS_FLOW_CONTROL
    if( (UINT32)comPortNum < ARRAYSIZE_CONST_EXPR(g_Uart_CTS_Pins)
     && g_Uart_Ports[comPortNum]->CR3 & USART_CR3_CTSE
     )
    {
        return !CPU_GPIO_GetPinState(g_Uart_CTS_Pins[comPortNum]); // CTS active
    }
#endif

    return TRUE; // If this handshake input is not being used, it is assumed to be good
}

void CPU_USART_ProtectPins(int comPortNum, BOOL on)  // idempotent
{
    if (on)
    {
        CPU_USART_RxBufferFullInterruptEnable(comPortNum, FALSE);
        CPU_USART_TxBufferEmptyInterruptEnable(comPortNum, FALSE);
    }
    else
    {
        CPU_USART_TxBufferEmptyInterruptEnable(comPortNum, TRUE);
        CPU_USART_RxBufferFullInterruptEnable(comPortNum, TRUE);
    }
}

UINT32 CPU_USART_PortsCount()
{
    return TOTAL_USART_PORT;
}

void CPU_USART_GetPins(int comPortNum, GPIO_PIN& rxPin, GPIO_PIN& txPin,GPIO_PIN& ctsPin, GPIO_PIN& rtsPin)
{
    rxPin = txPin = ctsPin = rtsPin = GPIO_PIN_NONE;
    if ((UINT32)comPortNum >= ARRAYSIZE_CONST_EXPR(g_Uart_RxD_Pins))
        return;

    rxPin = g_Uart_RxD_Pins[comPortNum];
    txPin = g_Uart_TxD_Pins[comPortNum];

#if defined(UART_CTS_PINS) && defined(UART_RTS_PINS)
    if ((UINT32)comPortNum >= ARRAYSIZE_CONST_EXPR(g_Uart_CTS_Pins))
        return; // no CTS/RTS

    ctsPin = g_Uart_CTS_Pins[comPortNum];
    rtsPin = g_Uart_RTS_Pins[comPortNum];
#endif
}

void CPU_USART_GetBaudrateBoundary(int comPortNum, UINT32 & maxBaudrateHz, UINT32 & minBaudrateHz)
{
    UINT32 clk = SYSTEM_APB2_CLOCK_HZ;
    if (comPortNum && comPortNum != 5)
        clk = SYSTEM_APB1_CLOCK_HZ;

    maxBaudrateHz = clk >> 4;
    minBaudrateHz = clk >> 16;
}

BOOL CPU_USART_SupportNonStandardBaudRate(int comPortNum)
{
    return TRUE;
}

BOOL CPU_USART_IsBaudrateSupported(int comPortNum, UINT32& baudrateHz)
{
    UINT32 max = SYSTEM_APB2_CLOCK_HZ >> 4;
    if (comPortNum && comPortNum != 5)
        max = SYSTEM_APB1_CLOCK_HZ >> 4;

    if (baudrateHz <= max)
        return TRUE;

    baudrateHz = max;
    return FALSE;
}

void GlobalUsartHandler(USART_TypeDef* uart)
{
    UINT16 sr = uart->SR;
    if (sr & USART_SR_RXNE) USART_Handle_RX_IRQ(GetUsartIndex(uart), uart);
    if (sr & USART_SR_TXE)  USART_Handle_TX_IRQ(GetUsartIndex(uart), uart);
}

#ifdef USART1
// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void USART1_IRQHandler(void)
{
    GlobalUsartHandler(USART1);
}
#endif
#ifdef USART2
// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void USART2_IRQHandler(void)
{
    GlobalUsartHandler(USART2);
}
#endif
#ifdef USART3
// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void USART3_IRQHandler(void)
{
    GlobalUsartHandler(USART3);
}
#endif
#ifdef UART4
// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void UART4_IRQHandler(void)
{
    GlobalUsartHandler(UART4);
}
#endif
#ifdef USART6
// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void USART6_IRQHandler(void)
{
    GlobalUsartHandler(USART6);
}
#endif
#ifdef UART7
// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void UART7_IRQHandler(void)
{
    GlobalUsartHandler(UART7);
}
#endif
#ifdef UART8
// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void UART8_IRQHandler(void)
{
    GlobalUsartHandler(UART8);
}
#endif

