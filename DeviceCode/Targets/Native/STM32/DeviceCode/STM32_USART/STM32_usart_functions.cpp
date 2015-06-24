////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  *** Serial Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\stm32f10x.h"


static USART_TypeDef* g_STM32_Uart[] = {USART1, USART2, USART3}; // IO addresses


void STM32_USART_Handle_RX_IRQ (int ComPortNum, USART_TypeDef* uart)
{
    INTERRUPT_START;

    char c = (char)(uart->DR); // read RX data
    USART_AddCharToRxBuffer(ComPortNum, c);
    Events_Set(SYSTEM_EVENT_FLAG_COM_IN);

    INTERRUPT_END;
}

void STM32_USART_Handle_TX_IRQ (int ComPortNum, USART_TypeDef* uart)
{
    INTERRUPT_START;

    char c;
    if (USART_RemoveCharFromTxBuffer(ComPortNum, c)) {
        uart->DR = c;  // write TX data
    } else {
        uart->CR1 &= ~USART_CR1_TXEIE; // TX int disable
    }
    Events_Set(SYSTEM_EVENT_FLAG_COM_OUT);

    INTERRUPT_END;
}

void STM32_USART_Interrupt0 (void* param)
{
    UINT16 sr = USART1->SR;
    if (sr & USART_SR_RXNE) STM32_USART_Handle_RX_IRQ(0, USART1);
    if (sr & USART_SR_TXE)  STM32_USART_Handle_TX_IRQ(0, USART1);
}

void STM32_USART_Interrupt1 (void* param)
{
    UINT16 sr = USART2->SR;
    if (sr & USART_SR_RXNE) STM32_USART_Handle_RX_IRQ(1, USART2);
    if (sr & USART_SR_TXE)  STM32_USART_Handle_TX_IRQ(1, USART2);
}

void STM32_USART_Interrupt2 (void* param)
{
    UINT16 sr = USART3->SR;
    if (sr & USART_SR_RXNE) STM32_USART_Handle_RX_IRQ(2, USART3);
    if (sr & USART_SR_TXE)  STM32_USART_Handle_TX_IRQ(2, USART3);
}


BOOL CPU_USART_Initialize( int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue )
{
    if (ComPortNum >= TOTAL_USART_PORT) return FALSE;
    if (Parity >= USART_PARITY_MARK) return FALSE;
    
    GLOBAL_LOCK(irq);
    
    USART_TypeDef* uart = g_STM32_Uart[ComPortNum];
    UINT32 clk;
    
    // enable UART clock
    if (ComPortNum) { // COM2/3 on APB1
        RCC->APB1ENR |= RCC_APB1ENR_USART2EN >> 1 << ComPortNum;
        clk = SYSTEM_APB1_CLOCK_HZ;
    } else { // COM1 on APB2
        RCC->APB2ENR |= RCC_APB2ENR_USART1EN;
        clk = SYSTEM_APB2_CLOCK_HZ;
    }
    
    //  baudrate
    UINT16 div = (UINT16)((clk + (BaudRate >> 1)) / BaudRate); // rounded
    uart->BRR = div;
    
    // control
    UINT16 ctrl = USART_CR1_TE | USART_CR1_RE;
    if (Parity) {
        ctrl |= USART_CR1_PCE;
        DataBits++;
    }
    if (Parity == USART_PARITY_ODD) ctrl |= USART_CR1_PS;
    if (DataBits == 9) ctrl |= USART_CR1_M;
    else if (DataBits != 8) return false;
    uart->CR1 = ctrl;
    
    if (StopBits == USART_STOP_BITS_ONE) StopBits = 0;
    uart->CR2 = (UINT16)(StopBits << 12);
    
    ctrl = 0;
    if (FlowValue & USART_FLOW_HW_OUT_EN) ctrl |= USART_CR3_CTSE;
    if (FlowValue & USART_FLOW_HW_IN_EN)  ctrl |= USART_CR3_RTSE;
    uart->CR3 = ctrl;

    GPIO_PIN rxPin, txPin, ctsPin, rtsPin;
    CPU_USART_GetPins(ComPortNum, rxPin, txPin, ctsPin, rtsPin);
    CPU_GPIO_DisablePin(rxPin, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY); // floating input
    CPU_GPIO_DisablePin(txPin, RESISTOR_DISABLED, 1, GPIO_ALT_MODE_1);  // alternate output
    if (FlowValue & USART_FLOW_HW_OUT_EN)
        CPU_GPIO_DisablePin(ctsPin, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY); // floating input
    if (FlowValue & USART_FLOW_HW_IN_EN)
        CPU_GPIO_DisablePin(rtsPin, RESISTOR_DISABLED, 1, GPIO_ALT_MODE_1);  // alternate output

    CPU_USART_ProtectPins(ComPortNum, FALSE);
    
    if (ComPortNum == 0) {
        CPU_INTC_ActivateInterrupt(USART1_IRQn, STM32_USART_Interrupt0, 0);
    } else if (ComPortNum == 1) {
        CPU_INTC_ActivateInterrupt(USART2_IRQn, STM32_USART_Interrupt1, 0);
    } else if (ComPortNum == 2) {
        CPU_INTC_ActivateInterrupt(USART3_IRQn, STM32_USART_Interrupt2, 0);
    }

    uart->CR1 |= USART_CR1_UE; // start uart

    return TRUE;
}

BOOL CPU_USART_Uninitialize( int ComPortNum )
{
    GLOBAL_LOCK(irq);
    
    g_STM32_Uart[ComPortNum]->CR1 = 0; // stop uart

    if (ComPortNum == 0) {
        CPU_INTC_DeactivateInterrupt(USART1_IRQn);
    } else if (ComPortNum == 1) {
        CPU_INTC_DeactivateInterrupt(USART2_IRQn);
    } else if (ComPortNum == 2) {
        CPU_INTC_DeactivateInterrupt(USART3_IRQn);
    }

    CPU_USART_ProtectPins(ComPortNum, TRUE);
    
    // disable UART clock
    if (ComPortNum) { // COM2 or COM3 on APB1
        RCC->APB1ENR &= ~(RCC_APB1ENR_USART2EN >> 1 << ComPortNum);
    } else { // COM1 on APB2
        RCC->APB2ENR &= ~RCC_APB2ENR_USART1EN;
    }

    return TRUE;
}

BOOL CPU_USART_TxBufferEmpty( int ComPortNum )
{
    if (g_STM32_Uart[ComPortNum]->SR & USART_SR_TXE) return TRUE;
    return FALSE;
}

BOOL CPU_USART_TxShiftRegisterEmpty( int ComPortNum )
{
    if (g_STM32_Uart[ComPortNum]->SR & USART_SR_TC) return TRUE;
    return FALSE;
}

void CPU_USART_WriteCharToTxBuffer( int ComPortNum, UINT8 c )
{
#ifdef DEBUG
    ASSERT(CPU_USART_TxBufferEmpty(ComPortNum));
#endif
    g_STM32_Uart[ComPortNum]->DR = c;
}

void CPU_USART_TxBufferEmptyInterruptEnable( int ComPortNum, BOOL Enable )
{
    USART_TypeDef* uart = g_STM32_Uart[ComPortNum];
    if (Enable) {
        uart->CR1 |= USART_CR1_TXEIE;  // tx int enable
    } else {
        uart->CR1 &= ~USART_CR1_TXEIE; // tx int disable
    }
}

BOOL CPU_USART_TxBufferEmptyInterruptState( int ComPortNum )
{
    if (g_STM32_Uart[ComPortNum]->CR1 & USART_CR1_TXEIE) return TRUE;
    return FALSE;
}

void CPU_USART_RxBufferFullInterruptEnable( int ComPortNum, BOOL Enable )
{
    USART_TypeDef* uart = g_STM32_Uart[ComPortNum];
    if (Enable) {
        uart->CR1 |= USART_CR1_RXNEIE;  // rx int enable
    } else {
        uart->CR1 &= ~USART_CR1_RXNEIE; // rx int disable
    }
}

BOOL CPU_USART_RxBufferFullInterruptState( int ComPortNum )
{
    if (g_STM32_Uart[ComPortNum]->CR1 & USART_CR1_RXNEIE) return TRUE;
    return FALSE;
}

BOOL CPU_USART_TxHandshakeEnabledState( int ComPortNum )
{
    // The state of the CTS input only matters if Flow Control is enabled
    if (g_STM32_Uart[ComPortNum]->CR3 & USART_CR3_CTSE)
    {
        GPIO_PIN pin = 0x0B;
        if (ComPortNum == 1) pin = 0x00;
        else if (ComPortNum == 2) pin = 0x1D;
        return !CPU_GPIO_GetPinState(pin); // CTS active
    }
    return TRUE; // If this handshake input is not being used, it is assumed to be good
}

void CPU_USART_ProtectPins( int ComPortNum, BOOL On )  // idempotent
{
    if (On) {
        CPU_USART_RxBufferFullInterruptEnable(ComPortNum, FALSE);
        CPU_USART_TxBufferEmptyInterruptEnable(ComPortNum, FALSE);
    } else {
        CPU_USART_TxBufferEmptyInterruptEnable(ComPortNum, TRUE);
        CPU_USART_RxBufferFullInterruptEnable(ComPortNum, TRUE);
    }
}

UINT32 CPU_USART_PortsCount()
{
    return TOTAL_USART_PORT;
}

void CPU_USART_GetPins( int ComPortNum, GPIO_PIN& rxPin, GPIO_PIN& txPin,GPIO_PIN& ctsPin, GPIO_PIN& rtsPin )
{
    if (ComPortNum == 0) {
        rxPin  = 0x0A; // A10
        txPin  = 0x09; // A9
        ctsPin = 0x0B; // A11
        rtsPin = 0x0C; // A12
    } else if (ComPortNum == 1) {
        rxPin  = 0x03; // A3
        txPin  = 0x02; // A2
        ctsPin = 0x00; // A0
        rtsPin = 0x01; // A1
    } else {
        rxPin  = 0x1B; // B11
        txPin  = 0x1A; // B10
        ctsPin = 0x1D; // B13
        rtsPin = 0x1E; // B14
    }
}

void CPU_USART_GetBaudrateBoundary( int ComPortNum, UINT32 & maxBaudrateHz, UINT32 & minBaudrateHz )
{
    UINT32 clk = SYSTEM_APB2_CLOCK_HZ;
    if (ComPortNum) clk = SYSTEM_APB1_CLOCK_HZ;
    maxBaudrateHz = clk >> 4;
    minBaudrateHz = clk >> 16;
}

BOOL CPU_USART_SupportNonStandardBaudRate( int ComPortNum )
{
    return TRUE;
}

BOOL CPU_USART_IsBaudrateSupported( int ComPortNum, UINT32& BaudrateHz )
{
    UINT32 max = SYSTEM_APB2_CLOCK_HZ >> 4;
    if (ComPortNum) max = SYSTEM_APB1_CLOCK_HZ >> 4;
    if (BaudrateHz <= max) return TRUE;
    BaudrateHz = max;
    return FALSE;
}


