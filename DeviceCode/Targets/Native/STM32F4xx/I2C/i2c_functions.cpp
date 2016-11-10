////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for CMSIS: Copyright (c) Eclo Solutions Lda. Based in original code from Oberon microsystems, Inc.
//
//  *** I2C Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h> 
//////////////////////////////////////
// to simplify, becasue NETMF only use a single I2C1 port
//////////////////////////////////////
// #if I2C_PORT == 2
//     #define I2Cx                   I2C2 
//     #define I2Cx_EV_IRQn           I2C2_EV_IRQn
//     #define I2Cx_ER_IRQn           I2C2_ER_IRQn
//     #define RCC_APB1ENR_I2CxEN     RCC_APB1ENR_I2C2EN
//     #define RCC_APB1RSTR_I2CxRST   RCC_APB1RSTR_I2C2RST
//     #if !defined(I2C_SCL_PIN)
//         #define I2C_SCL_PIN  26 // PB10
//     #endif
//     #if !defined(I2C_SDA_PIN)
//         #define I2C_SDA_PIN  27 // PB11
//     #endif
// #elif I2C_PORT == 3
//     #define I2Cx                   I2C3 
//     #define I2Cx_EV_IRQn           I2C3_EV_IRQn
//     #define I2Cx_ER_IRQn           I2C3_ER_IRQn
//     #define RCC_APB1ENR_I2CxEN     RCC_APB1ENR_I2C3EN
//     #define RCC_APB1RSTR_I2CxRST   RCC_APB1RSTR_I2C3RST
//     #if !defined(I2C_SCL_PIN)
//         #define I2C_SCL_PIN  8 // PA8
//     #endif
//     #if !defined(I2C_SDA_PIN)
//         #define I2C_SDA_PIN  41 // PC9
//     #endif
// #else // use I2C1 by default
    #define I2Cx                   I2C1 
    #define I2Cx_EV_IRQn           I2C1_EV_IRQn
    #define I2Cx_ER_IRQn           I2C1_ER_IRQn
    #define RCC_APB1ENR_I2CxEN     RCC_APB1ENR_I2C1EN
    #define RCC_APB1RSTR_I2CxRST   RCC_APB1RSTR_I2C1RST
    #if !defined(I2C_SCL_PIN)
        #define I2C_SCL_PIN  22 // PB6
    #endif
    #if !defined(I2C_SDA_PIN)
        #define I2C_SDA_PIN  23 // PB7
    #endif
// #endif

/* I2C handler declaration */
I2C_HandleTypeDef I2cHandle;

static I2C_HAL_XACTION* currentI2CXAction;
static I2C_HAL_XACTION_UNIT* currentI2CUnit;

// prototypes
void Start_Master_Transmit(I2C_HAL_XACTION_UNIT* unit, uint16_t I2CAddress);
void Start_Master_Receive(I2C_HAL_XACTION_UNIT* unit, uint16_t I2CAddress);
void I2C_Tx_Rx_Completed();

// RX buffer pointer
uint8_t * rxBuffer = NULL;

/**
  * @brief  This function handles I2C event interrupt request.  
  * @param  None
  * @retval None
  * @Note   This function is redefined in "main.h" and related to I2C data transmission     
  */
void I2C1_EV_IRQHandler(void)
{
    HAL_I2C_EV_IRQHandler(&I2cHandle);
}

/**
  * @brief  This function handles I2C error interrupt request.
  * @param  None
  * @retval None
  * @Note   This function is redefined in "main.h" and related to I2C error
  */
void I2C1_ER_IRQHandler(void)
{
    HAL_I2C_ER_IRQHandler(&I2cHandle);
}

/**
  * @brief  I2C error callbacks.
  * @param  hi2c Pointer to a I2C_HandleTypeDef structure that contains
  *         the configuration information for I2C module
  * @retval None
  */
void HAL_I2C_ErrorCallback(I2C_HandleTypeDef *hi2c)
{
    // free rx buffer 
    if(rxBuffer != NULL)
    {
        free(rxBuffer);
        rxBuffer = NULL;
    }
    
    I2C_HAL_XACTION* xAction = currentI2CXAction;
    xAction->Signal(I2C_HAL_XACTION::c_Status_Aborted); // calls XActionStop()
}

/**
  * @brief  Master Tx Transfer completed callbacks.
  * @param  hi2c Pointer to a I2C_HandleTypeDef structure that contains
  *         the configuration information for I2C module
  * @retval None
  */
void HAL_I2C_MasterTxCpltCallback(I2C_HandleTypeDef *hi2c)
{
    I2C_Tx_Rx_Completed();
}

/**
  * @brief  Master Rx Transfer completed callbacks.
  * @param  hi2c Pointer to a I2C_HandleTypeDef structure that contains
  *         the configuration information for I2C module
  * @retval None
  */
void HAL_I2C_MasterRxCpltCallback(I2C_HandleTypeDef *hi2c)
{
    for(int i = 0; i < hi2c->XferSize; i++)
    {
         // save next data byte into queue
        *(currentI2CUnit->m_dataQueue.Push()) = rxBuffer[i];
    }
    // adjust counters
    currentI2CUnit->m_bytesTransferred = hi2c->XferSize;
    currentI2CUnit->m_bytesToTransfer -= hi2c->XferSize;

    // free rx buffer
    free(rxBuffer);
    rxBuffer = NULL;

    I2C_Tx_Rx_Completed();
}

void I2C_Tx_Rx_Completed()
{
    I2C_HAL_XACTION* xAction = currentI2CXAction;
    
    // all received or all sent
    if (!xAction->ProcessingLastUnit()) 
    {
        // get current unit 
        currentI2CUnit = xAction->m_xActionUnits[xAction->m_current++];

        // start next unit
        // what is the direction of the current I2C unit?
        if (currentI2CUnit->IsReadXActionUnit()) 
        { 
            // read transaction
            // get address from xAction        
            Start_Master_Receive(currentI2CUnit, xAction->m_address << 1);
        }
        else
        {
            // write transaction
            // get address from xAction
            Start_Master_Transmit(currentI2CUnit, xAction->m_address << 1);
        }
    } 
    else 
    {
        xAction->Signal(I2C_HAL_XACTION::c_Status_Completed); // calls XActionStop()
    }
}

void HAL_I2C_MspInit(I2C_HandleTypeDef *hi2c)
{
    GPIO_InitTypeDef  GPIO_InitStruct;

    /*##-1- Enable peripherals and GPIO Clocks #################################*/
    /* Enable GPIO TX/RX clock */
    __HAL_RCC_GPIOB_CLK_ENABLE();
    /* Enable I2C1 clock */
    __HAL_RCC_I2C1_CLK_ENABLE(); 

    /*##-2- Configure peripheral GPIO ##########################################*/  
    /* I2C TX GPIO pin configuration  */
    GPIO_InitStruct.Pin       = I2C_SCL_PIN;
    GPIO_InitStruct.Mode      = GPIO_MODE_AF_OD;
    GPIO_InitStruct.Pull      = GPIO_PULLUP;
    GPIO_InitStruct.Speed     = GPIO_SPEED_FAST;
    GPIO_InitStruct.Alternate = GPIO_AF4_I2C1;

    HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

    /* I2C RX GPIO pin configuration  */
    GPIO_InitStruct.Pin = I2C_SDA_PIN;
    GPIO_InitStruct.Alternate = GPIO_AF4_I2C1;

    HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

    /*##-3- Configure the NVIC for I2C #########################################*/   
    /* NVIC priority for I2C1 */
    HAL_NVIC_SetPriority(I2C1_ER_IRQn, 0, 1);
    HAL_NVIC_SetPriority(I2C1_EV_IRQn, 0, 2);
}

void HAL_I2C_MspDeInit(I2C_HandleTypeDef *hi2c)
{
    /*##-1- Reset peripherals ##################################################*/
    __HAL_RCC_I2C1_FORCE_RESET();
    __HAL_RCC_I2C1_RELEASE_RESET();

    /*##-2- Disable peripherals and GPIO Clocks ################################*/
    /* Configure I2C Tx as alternate function  */
    HAL_GPIO_DeInit(GPIOB, I2C_SCL_PIN);
    /* Configure I2C Rx as alternate function  */
    HAL_GPIO_DeInit(GPIOB, I2C_SDA_PIN);

    /*##-3- Disable the NVIC for I2C ###########################################*/
    HAL_NVIC_DisableIRQ(I2C1_ER_IRQn);
    HAL_NVIC_DisableIRQ(I2C1_EV_IRQn);
}

BOOL I2C_Internal_Initialize()
{
    // NATIVE_PROFILE_HAL_PROCESSOR_I2C();
    
    // if (!(RCC->APB1ENR & RCC_APB1ENR_I2CxEN)) { // only once
    //     currentI2CXAction = NULL;
    //     currentI2CUnit = NULL;
        
    //     // set pins to AF4 and open drain
    //     CPU_GPIO_DisablePin( I2C_SDA_PIN, RESISTOR_PULLUP, 0, (GPIO_ALT_MODE)0x43 );
    //     CPU_GPIO_DisablePin( I2C_SCL_PIN, RESISTOR_PULLUP, 0, (GPIO_ALT_MODE)0x43 );
        
    //     RCC->APB1ENR |= RCC_APB1ENR_I2CxEN; // enable I2C clock
    //     RCC->APB1RSTR = RCC_APB1RSTR_I2CxRST; // reset I2C peripheral
    //     RCC->APB1RSTR = 0;
        
    //     I2Cx->CR2 = SYSTEM_APB1_CLOCK_HZ / 1000000; // APB1 clock in MHz
    //     I2Cx->CCR = (SYSTEM_APB1_CLOCK_HZ / 1000 / 2 - 1) / 100 + 1; // 100KHz
    //     I2Cx->TRISE = SYSTEM_APB1_CLOCK_HZ / (1000 * 1000) + 1; // 1ns;
    //     I2Cx->OAR1 = 0x4000; // init address register
        
    //     I2Cx->CR1 = I2C_CR1_PE; // enable peripheral
        
    //     CPU_INTC_ActivateInterrupt(I2Cx_EV_IRQn, I2C_EV_Interrupt, 0);
    //     CPU_INTC_ActivateInterrupt(I2Cx_ER_IRQn, I2C_ER_Interrupt, 0);
    // }
    
    return TRUE;
}

BOOL I2C_Internal_Uninitialize()
{
    // NATIVE_PROFILE_HAL_PROCESSOR_I2C();
    
    // CPU_INTC_DeactivateInterrupt(I2Cx_EV_IRQn);
    // CPU_INTC_DeactivateInterrupt(I2Cx_ER_IRQn);
    // I2Cx->CR1 = 0; // disable peripheral
    // RCC->APB1ENR &= ~RCC_APB1ENR_I2CxEN; // disable I2C clock
    
    return TRUE;
}

void I2C_Internal_XActionStart( I2C_HAL_XACTION* xAction, bool repeatedStart )
{
    NATIVE_PROFILE_HAL_PROCESSOR_I2C();

    // I2C handler declaration
    I2C_HandleTypeDef I2cHandle;

    /*##-1- Configure the I2C peripheral ######################################*/
    I2cHandle.Instance             = I2Cx;

    I2cHandle.Init.AddressingMode  = I2C_ADDRESSINGMODE_10BIT;
    I2cHandle.Init.ClockSpeed      = xAction->m_clockRate + (xAction->m_clockRate2 << 8);//400000;
    I2cHandle.Init.DualAddressMode = I2C_DUALADDRESS_DISABLE;
    I2cHandle.Init.DutyCycle       = I2C_DUTYCYCLE_16_9;
    I2cHandle.Init.GeneralCallMode = I2C_GENERALCALL_DISABLE;
    I2cHandle.Init.NoStretchMode   = I2C_NOSTRETCH_DISABLE;
    I2cHandle.Init.OwnAddress1     = xAction->m_address << 1;//I2C_ADDRESS;
    I2cHandle.Init.OwnAddress2     = 0xFE;

    if(HAL_I2C_Init(&I2cHandle) != HAL_OK)
    {
        // Initialization Error
        return;    
    }

    // set vars for latter use
    currentI2CXAction = xAction;
    currentI2CUnit = xAction->m_xActionUnits[xAction->m_current++];

    // enable interrupts
    HAL_NVIC_EnableIRQ(I2C1_ER_IRQn);
    HAL_NVIC_EnableIRQ(I2C1_EV_IRQn);

    // what is the direction of the current I2C unit?
    if (currentI2CUnit->IsReadXActionUnit()) 
    { 
        // read transaction
        // get address from xAction        
        Start_Master_Receive(currentI2CUnit, xAction->m_address << 1);
    }
    else
    {
        // write transaction
        // get address from xAction
        Start_Master_Transmit(currentI2CUnit, xAction->m_address << 1);
     }
}

void I2C_Internal_XActionStop()
{
    NATIVE_PROFILE_HAL_PROCESSOR_I2C();
    // disable interrupts
    HAL_NVIC_DisableIRQ(I2C1_ER_IRQn);
    HAL_NVIC_DisableIRQ(I2C1_EV_IRQn);
    // null vars
    currentI2CXAction = NULL;
    currentI2CUnit = NULL;
}

void I2C_Internal_GetClockRate( UINT32 rateKhz, UINT8& clockRate, UINT8& clockRate2)
{
    NATIVE_PROFILE_HAL_PROCESSOR_I2C();
    if (rateKhz > 400) rateKhz = 400; // upper limit
    UINT32 ccr;
    if (rateKhz <= 100) 
    { 
        // slow clock
        ccr = (SYSTEM_APB1_CLOCK_HZ / 1000 / 2 - 1) / rateKhz + 1; // round up
        if (ccr > 0xFFF) ccr = 0xFFF; // max divider
    } 
    else 
    { 
        // fast clock
        ccr = (SYSTEM_APB1_CLOCK_HZ / 1000 / 3 - 1) / rateKhz + 1; // round up
        ccr |= 0x8000; // set fast mode (duty cycle 1:2)
    }
    clockRate = (UINT8)ccr; // low byte
    clockRate2 = (UINT8)(ccr >> 8); // high byte
}

void I2C_Internal_GetPins(GPIO_PIN& scl, GPIO_PIN& sda)
{
    scl = I2C_SCL_PIN;
    sda = I2C_SDA_PIN;
}

void Start_Master_Transmit(I2C_HAL_XACTION_UNIT* unit, uint16_t I2CAddress)
{
    // fill tx buffer
    uint8_t aTxBuffer[unit->m_bytesToTransfer];
    uint8_t counter = unit->m_bytesToTransfer;
    
    for(int i = 0; i < counter; i++)
    {
         // get next data byte from queue
        aTxBuffer[i] = *(unit->m_dataQueue.Pop());
    }
        
    while(HAL_I2C_Master_Transmit_IT(&I2cHandle, (uint16_t)I2CAddress, (uint8_t*)aTxBuffer, unit->m_bytesToTransfer) != HAL_OK)
    {
        /* When Acknowledge failure occurs (Slave don't acknowledge it's address)
            Master restarts communication */
        if (HAL_I2C_GetError(&I2cHandle) != HAL_I2C_ERROR_AF)
        {
            // couldn't start the transmission because of a timeout error 
            // nothing else to do because the caller will handle the timeout error
            
            // done here
            return;
        }
    }
    
    // transmission started correctly
    // adjust counters
    unit->m_bytesTransferred = unit->m_bytesToTransfer;
    unit->m_bytesToTransfer = 0;
}

void Start_Master_Receive(I2C_HAL_XACTION_UNIT* unit, uint16_t I2CAddress)
{
    // allocate rx buffer
    rxBuffer = (uint8_t *) malloc(unit->m_bytesToTransfer); 

    while(HAL_I2C_Master_Receive_IT(&I2cHandle, (uint16_t)I2CAddress, (uint8_t *)rxBuffer, unit->m_bytesToTransfer) != HAL_OK)
    {
        /* When Acknowledge failure occurs (Slave don't acknowledge it's address)
            Master restarts communication */
        if (HAL_I2C_GetError(&I2cHandle) != HAL_I2C_ERROR_AF)
        {
            // couldn't start the receive because of a timeout error 
            // nothing else to do because the caller will handle the timeout error

            // free rx buffer
            free(rxBuffer);
            rxBuffer = NULL;

            // done here
            return;
        }   
    }   
}
