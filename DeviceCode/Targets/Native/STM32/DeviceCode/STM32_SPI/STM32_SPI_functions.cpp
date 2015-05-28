////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  *** SPI Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\stm32f10x.h"

// pins
#define SPI1_SCLK_Pin (0 + 5)   // PA5
#define SPI1_MISO_Pin (0 + 6)   // PA6
#define SPI1_MOSI_Pin (0 + 7)   // PA7
#define SPI2_SCLK_Pin (16 + 13) // PB13
#define SPI2_MISO_Pin (16 + 14) // PB14
#define SPI2_MOSI_Pin (16 + 15) // PB15
#define SPI3_SCLK_Pin (16 + 3)  // PB3
#define SPI3_MISO_Pin (16 + 4)  // PB4
#define SPI3_MOSI_Pin (16 + 5)  // PB5

/*
struct SPI_CONFIGURATION
{
    GPIO_PIN       DeviceCS;
    BOOL           CS_Active;             // False = LOW active,      TRUE = HIGH active
    BOOL           MSK_IDLE;              // False = LOW during idle, TRUE = HIGH during idle
    BOOL           MSK_SampleEdge;        // False = sample falling edge, TRUE = samples on rising
    BOOL           MD_16bits;
    UINT32         Clock_RateKHz;
    UINT32         CS_Setup_uSecs;
    UINT32         CS_Hold_uSecs;
    UINT32         SPI_mod;
    GPIO_FLAG      BusyPin;
};
*/


static SPI_TypeDef* g_STM32_Spi[] = {SPI1, SPI2, SPI3}; // IO addresses


BOOL CPU_SPI_Initialize()
{
    return TRUE;
}

void CPU_SPI_Uninitialize()
{
}

BOOL CPU_SPI_nWrite16_nRead16( const SPI_CONFIGURATION& Configuration, UINT16* Write16, INT32 WriteCount, UINT16* Read16, INT32 ReadCount, INT32 ReadStartOffset )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    if(!CPU_SPI_Xaction_Start( Configuration )) return FALSE;
    SPI_XACTION_16 Transaction;
    Transaction.Read16          = Read16;
    Transaction.ReadCount       = ReadCount;
    Transaction.ReadStartOffset = ReadStartOffset;
    Transaction.Write16         = Write16;
    Transaction.WriteCount      = WriteCount;
    Transaction.SPI_mod         = Configuration.SPI_mod;
    if(!CPU_SPI_Xaction_nWrite16_nRead16( Transaction )) return FALSE;
    return CPU_SPI_Xaction_Stop( Configuration );
}

BOOL CPU_SPI_nWrite8_nRead8( const SPI_CONFIGURATION& Configuration, UINT8* Write8, INT32 WriteCount, UINT8* Read8, INT32 ReadCount, INT32 ReadStartOffset )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    if(!CPU_SPI_Xaction_Start( Configuration )) return FALSE;
    SPI_XACTION_8 Transaction;
    Transaction.Read8           = Read8;
    Transaction.ReadCount       = ReadCount;
    Transaction.ReadStartOffset = ReadStartOffset;
    Transaction.Write8          = Write8;
    Transaction.WriteCount      = WriteCount;
    Transaction.SPI_mod         = Configuration.SPI_mod;
    if(!CPU_SPI_Xaction_nWrite8_nRead8( Transaction )) return FALSE;
    return CPU_SPI_Xaction_Stop( Configuration );
}

BOOL CPU_SPI_Xaction_Start( const SPI_CONFIGURATION& Configuration )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    // enable SPI
    switch (Configuration.SPI_mod) {
        case 0: RCC->APB2ENR |= RCC_APB2ENR_SPI1EN; break;
        case 1: RCC->APB1ENR |= RCC_APB1ENR_SPI2EN; break;
        case 2: RCC->APB1ENR |= RCC_APB1ENR_SPI3EN; break;
        default: return FALSE;
    }
    
    SPI_TypeDef* spi = g_STM32_Spi[Configuration.SPI_mod];
    
    // set mode bits
    UINT32 cr1 = SPI_CR1_SSM | SPI_CR1_SSI | SPI_CR1_MSTR | SPI_CR1_SPE;
    if (Configuration.MD_16bits) cr1 |= SPI_CR1_DFF;
    if (Configuration.MSK_IDLE) cr1 |= SPI_CR1_CPOL | SPI_CR1_CPHA;
    if (!Configuration.MSK_SampleEdge) cr1 ^= SPI_CR1_CPHA; // toggle phase
    
    // set clock prescaler
    UINT32 clock = SYSTEM_APB2_CLOCK_HZ / 2000; // SPI1 on APB2
    if (Configuration.SPI_mod != 0) clock = SYSTEM_APB1_CLOCK_HZ / 2000; // SPI2/3 on APB1
    
    if (clock > Configuration.Clock_RateKHz << 3) {
        clock >>= 4;
        cr1 |= SPI_CR1_BR_2;
    }
    if (clock > Configuration.Clock_RateKHz << 1) {
        clock >>= 2;
        cr1 |= SPI_CR1_BR_1;
    }
    if (clock > Configuration.Clock_RateKHz) {
        cr1 |= SPI_CR1_BR_0;
    }
    spi->CR1 = cr1;
    
    // I/O setup
    GPIO_PIN msk, miso, mosi;
    CPU_SPI_GetPins(Configuration.SPI_mod, msk, miso, mosi);
    CPU_GPIO_DisablePin( msk,  RESISTOR_DISABLED, 1, GPIO_ALT_MODE_2);
    CPU_GPIO_DisablePin( miso, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY);
    CPU_GPIO_DisablePin( mosi, RESISTOR_DISABLED, 1, GPIO_ALT_MODE_2);
    
    // CS setup
    CPU_GPIO_EnableOutputPin( Configuration.DeviceCS, Configuration.CS_Active );
    if(Configuration.CS_Setup_uSecs)
    {
        HAL_Time_Sleep_MicroSeconds_InterruptEnabled( Configuration.CS_Setup_uSecs );
    }
    
    return TRUE;
}

BOOL CPU_SPI_Xaction_Stop( const SPI_CONFIGURATION& Configuration )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    
    SPI_TypeDef* spi = g_STM32_Spi[Configuration.SPI_mod];
    while (spi->SR & SPI_SR_BSY); // wait for completion
    spi->CR1 = 0; // disable SPI

    if(Configuration.CS_Hold_uSecs)
    {
        HAL_Time_Sleep_MicroSeconds_InterruptEnabled( Configuration.CS_Hold_uSecs );
    }
    CPU_GPIO_SetPinState( Configuration.DeviceCS, !Configuration.CS_Active );
    GPIO_RESISTOR res = RESISTOR_PULLDOWN;
    if (Configuration.MSK_IDLE) res = RESISTOR_PULLUP;
    GPIO_PIN msk, miso, mosi;
    CPU_SPI_GetPins(Configuration.SPI_mod, msk, miso, mosi);
    CPU_GPIO_EnableInputPin( msk,  FALSE, NULL, GPIO_INT_NONE, res );
    CPU_GPIO_EnableInputPin( miso, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLDOWN );
    CPU_GPIO_EnableInputPin( mosi, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLDOWN );
    
    // disable SPI
    switch (Configuration.SPI_mod) {
        case 0: RCC->APB2ENR &= ~RCC_APB2ENR_SPI1EN; break;
        case 1: RCC->APB1ENR &= ~RCC_APB1ENR_SPI2EN; break;
        case 2: RCC->APB1ENR &= ~RCC_APB1ENR_SPI3EN; break;
    }

    return TRUE;
}

BOOL CPU_SPI_Xaction_nWrite16_nRead16( SPI_XACTION_16& Transaction )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    
    SPI_TypeDef* spi = g_STM32_Spi[Transaction.SPI_mod];
    
    UINT16* outBuf = Transaction.Write16;
    UINT16* inBuf  = Transaction.Read16;
    INT32 outLen = Transaction.WriteCount;
    INT32 num, ii, i = 0;
    
    if (Transaction.ReadCount) { // write & read
        num = Transaction.ReadCount + Transaction.ReadStartOffset;
        ii = -Transaction.ReadStartOffset;
    } else { // write only
        num = outLen;
        ii = 0x80000000; // disable write to inBuf
    }

    UINT16 out = outBuf[0];
    UINT16 in;
    spi->DR = out; // write first word
    while (++i < num) {
        if (i < outLen) out = outBuf[i]; // get new output data
        while (!(spi->SR & SPI_SR_RXNE)); // wait for Rx buffer full
        in = spi->DR; // read input
        spi->DR = out; // start output
        if (ii >= 0) inBuf[ii] = in; // save input data
        ii++;
    }
    while (!(spi->SR & SPI_SR_RXNE)); // wait for Rx buffer full
    in = spi->DR; // read last input
    if (ii >= 0) inBuf[ii] = in; // save last input

    return TRUE;
}

BOOL CPU_SPI_Xaction_nWrite8_nRead8( SPI_XACTION_8& Transaction )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    
    SPI_TypeDef* spi = g_STM32_Spi[Transaction.SPI_mod];
    
    UINT8* outBuf = Transaction.Write8;
    UINT8* inBuf  = Transaction.Read8;
    INT32 outLen = Transaction.WriteCount;
    INT32 num, ii, i = 0;
    
    if (Transaction.ReadCount) { // write & read
        num = Transaction.ReadCount + Transaction.ReadStartOffset;
        ii = -Transaction.ReadStartOffset;
    } else { // write only
        num = outLen;
        ii = 0x80000000; // disable write to inBuf
    }

    UINT8 out = outBuf[0];
    UINT16 in;
    spi->DR = out; // write first word
    while (++i < num) {
        if (i < outLen) out = outBuf[i]; // get new output data
        while (!(spi->SR & SPI_SR_RXNE)); // wait for Rx buffer full
        in = spi->DR; // read input
        spi->DR = out; // start output
        if (ii >= 0) inBuf[ii] = (UINT8)in; // save input data
        ii++;
    }
    while (!(spi->SR & SPI_SR_RXNE)); // wait for Rx buffer full
    in = spi->DR; // read last input
    if (ii >= 0) inBuf[ii] = (UINT8)in; // save last input

    return TRUE;
}

UINT32 CPU_SPI_PortsCount()
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    return 3;
}

void CPU_SPI_GetPins( UINT32 spi_mod, GPIO_PIN& msk, GPIO_PIN& miso, GPIO_PIN& mosi )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    if (spi_mod == 0) {
        msk  = SPI1_SCLK_Pin;
        miso = SPI1_MISO_Pin;
        mosi = SPI1_MOSI_Pin;
    } else if (spi_mod == 1) {
        msk  = SPI2_SCLK_Pin;
        miso = SPI2_MISO_Pin;
        mosi = SPI2_MOSI_Pin;
    } else {
        msk  = SPI3_SCLK_Pin;
        miso = SPI3_MISO_Pin;
        mosi = SPI3_MOSI_Pin;
    }
}

