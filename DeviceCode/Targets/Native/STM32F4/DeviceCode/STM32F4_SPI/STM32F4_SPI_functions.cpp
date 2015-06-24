////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** SPI Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

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

typedef  SPI_TypeDef* ptr_SPI_TypeDef;

// IO addresses
static const ptr_SPI_TypeDef g_STM32_Spi_Port[] = {SPI1, SPI2, SPI3, SPI4, SPI5, SPI6};

// Pins
static const BYTE g_STM32F4_Spi_Sclk_Pins[] = STM32F4_SPI_SCLK_PINS;
static const BYTE g_STM32F4_Spi_Miso_Pins[] = STM32F4_SPI_MISO_PINS;
static const BYTE g_STM32F4_Spi_Mosi_Pins[] = STM32F4_SPI_MOSI_PINS;

#define STM32F4_SPI_MODS ARRAYSIZE_CONST_EXPR(g_STM32F4_Spi_Sclk_Pins) // number of modules


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
    if(!CPU_SPI_Xaction_Start( Configuration ))
    {
        return FALSE;
    }

    SPI_XACTION_16 Transaction;
    Transaction.Read16          = Read16;
    Transaction.ReadCount       = ReadCount;
    Transaction.ReadStartOffset = ReadStartOffset;
    Transaction.Write16         = Write16;
    Transaction.WriteCount      = WriteCount;
    Transaction.SPI_mod         = Configuration.SPI_mod;
    if(!CPU_SPI_Xaction_nWrite16_nRead16( Transaction ))
    {
        return FALSE;
    }

    return CPU_SPI_Xaction_Stop( Configuration );
}

BOOL CPU_SPI_nWrite8_nRead8( const SPI_CONFIGURATION& Configuration, UINT8* Write8, INT32 WriteCount, UINT8* Read8, INT32 ReadCount, INT32 ReadStartOffset )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    if(!CPU_SPI_Xaction_Start( Configuration ))
    {
        return FALSE;
    }

    SPI_XACTION_8 Transaction;
    Transaction.Read8           = Read8;
    Transaction.ReadCount       = ReadCount;
    Transaction.ReadStartOffset = ReadStartOffset;
    Transaction.Write8          = Write8;
    Transaction.WriteCount      = WriteCount;
    Transaction.SPI_mod         = Configuration.SPI_mod;
    if(!CPU_SPI_Xaction_nWrite8_nRead8( Transaction ))
    {
        return FALSE;
    }

    return CPU_SPI_Xaction_Stop( Configuration );
}

BOOL CPU_SPI_Xaction_Start( const SPI_CONFIGURATION& Configuration )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    if (Configuration.SPI_mod >= STM32F4_SPI_MODS)
        return FALSE;
    
    switch (Configuration.SPI_mod)
    {
    case 0:
        RCC->APB2ENR |= RCC_APB2ENR_SPI1EN;
        break; // enable SPI1 clock

    case 1:
        RCC->APB1ENR |= RCC_APB1ENR_SPI2EN;
        break; // enable SPI2 clock

    case 2:
        RCC->APB1ENR |= RCC_APB1ENR_SPI3EN;
        break; // enable SPI3 clock

    case 3:
        RCC->APB2ENR |= RCC_APB2ENR_SPI4EN;
        break; // enable SPI4 clock

    case 4:
        RCC->APB2ENR |= RCC_APB2ENR_SPI5EN;
        break; // enable SPI5 clock

    case 5:
        RCC->APB2ENR |= RCC_APB2ENR_SPI6EN;
        break; // enable SPI6 clock
    }
    
    ptr_SPI_TypeDef spi = g_STM32_Spi_Port[Configuration.SPI_mod];
    
    // set mode bits
    UINT32 cr1 = SPI_CR1_SSM | SPI_CR1_SSI | SPI_CR1_MSTR | SPI_CR1_SPE;
    if (Configuration.MD_16bits)
    {
        cr1 |= SPI_CR1_DFF;
    }

    if (Configuration.MSK_IDLE)
    {
        cr1 |= SPI_CR1_CPOL | SPI_CR1_CPHA;
    }

    if (!Configuration.MSK_SampleEdge)
    {
        cr1 ^= SPI_CR1_CPHA; // toggle phase
    }
    
    // set clock prescaler
    UINT32 clock = SYSTEM_APB2_CLOCK_HZ / 2000; // SPI1 on APB2
    if (Configuration.SPI_mod > 0 && Configuration.SPI_mod < 3)
    {
        clock = SYSTEM_APB1_CLOCK_HZ / 2000; // SPI2/3 on APB1
    }
    
    if (clock > Configuration.Clock_RateKHz << 3)
    {
        clock >>= 4;
        cr1 |= SPI_CR1_BR_2;
    }

    if (clock > Configuration.Clock_RateKHz << 1)
    {
        clock >>= 2;
        cr1 |= SPI_CR1_BR_1;
    }

    if (clock > Configuration.Clock_RateKHz)
    {
        cr1 |= SPI_CR1_BR_0;
    }
    spi->CR1 = cr1;
    
    // I/O setup
    GPIO_PIN msk, miso, mosi;
    CPU_SPI_GetPins(Configuration.SPI_mod, msk, miso, mosi);
    UINT32 alternate = 0x252; // AF5, speed = 2 (50MHz)
    if (Configuration.SPI_mod == 2 && mosi != 54)
    {
        alternate = 0x262; // SPI3 on AF6
    }

    CPU_GPIO_DisablePin( msk,  RESISTOR_DISABLED, 1, (GPIO_ALT_MODE)alternate);
    CPU_GPIO_DisablePin( miso, RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)alternate);
    CPU_GPIO_DisablePin( mosi, RESISTOR_DISABLED, 1, (GPIO_ALT_MODE)alternate);
    
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
    
    ptr_SPI_TypeDef spi = g_STM32_Spi_Port[Configuration.SPI_mod];
    while (spi->SR & SPI_SR_BSY); // wait for completion
    spi->CR1 = 0; // disable SPI

    if(Configuration.CS_Hold_uSecs)
    {
        HAL_Time_Sleep_MicroSeconds_InterruptEnabled( Configuration.CS_Hold_uSecs );
    }

    CPU_GPIO_SetPinState( Configuration.DeviceCS, !Configuration.CS_Active );
    GPIO_RESISTOR res = RESISTOR_PULLDOWN;
    if (Configuration.MSK_IDLE)
    {
        res = RESISTOR_PULLUP;
    }

    GPIO_PIN msk, miso, mosi;
    CPU_SPI_GetPins(Configuration.SPI_mod, msk, miso, mosi);
    CPU_GPIO_EnableInputPin( msk,  FALSE, NULL, GPIO_INT_NONE, res );
    CPU_GPIO_EnableInputPin( miso, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLDOWN );
    CPU_GPIO_EnableInputPin( mosi, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLDOWN );
    
    switch (Configuration.SPI_mod)
    {
    case 0:
        RCC->APB2ENR &= ~RCC_APB2ENR_SPI1EN;
        break; // disable SPI1 clock

    case 1: 
        RCC->APB1ENR &= ~RCC_APB1ENR_SPI2EN;
        break; // disable SPI2 clock

    case 2:
        RCC->APB1ENR &= ~RCC_APB1ENR_SPI3EN;
        break; // disable SPI3 clock

    case 3:
        RCC->APB2ENR &= ~RCC_APB2ENR_SPI4EN;
        break; // disable SPI4 clock

    case 4:
        RCC->APB2ENR &= ~RCC_APB2ENR_SPI5EN;
        break; // disable SPI5 clock

    case 5:
        RCC->APB2ENR &= ~RCC_APB2ENR_SPI6EN;
        break; // disable SPI6 clock
    }

    return TRUE;
}

BOOL CPU_SPI_Xaction_nWrite16_nRead16( SPI_XACTION_16& Transaction )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    
    ptr_SPI_TypeDef spi = g_STM32_Spi_Port[Transaction.SPI_mod];
    
    UINT16* outBuf = Transaction.Write16;
    UINT16* inBuf  = Transaction.Read16;
    INT32 outLen = Transaction.WriteCount;
    INT32 num, ii, i = 0;
    
    if (Transaction.ReadCount)
    { // write & read
        num = Transaction.ReadCount + Transaction.ReadStartOffset;
        ii = -Transaction.ReadStartOffset;
    }
    else
    { // write only
        num = outLen;
        ii = 0x80000000; // disable write to inBuf
    }

    UINT16 out = outBuf[0];
    UINT16 in;
    spi->DR = out; // write first word
    while (++i < num)
    {
        if (i < outLen)
        {
            out = outBuf[i]; // get new output data
        }

        while (!(spi->SR & SPI_SR_RXNE))
        { /* wait for Rx buffer full */ }

        in = spi->DR; // read input
        spi->DR = out; // start output
        if (ii >= 0)
            inBuf[ii] = in; // save input data
        ii++;
    }
    while (!(spi->SR & SPI_SR_RXNE))
    { /* wait for Rx buffer full */ }

    in = spi->DR; // read last input
    if (ii >= 0)
        inBuf[ii] = in; // save last input

    return TRUE;
}

BOOL CPU_SPI_Xaction_nWrite8_nRead8( SPI_XACTION_8& Transaction )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    
    ptr_SPI_TypeDef spi = g_STM32_Spi_Port[Transaction.SPI_mod];
    
    UINT8* outBuf = Transaction.Write8;
    UINT8* inBuf  = Transaction.Read8;
    INT32 outLen = Transaction.WriteCount;
    INT32 num, ii, i = 0;
    
    if (Transaction.ReadCount)
    { // write & read
        num = Transaction.ReadCount + Transaction.ReadStartOffset;
        ii = -Transaction.ReadStartOffset;
    }
    else
    { // write only
        num = outLen;
        ii = 0x80000000; // disable write to inBuf
    }

    UINT8 out = outBuf[0];
    UINT16 in;
    spi->DR = out; // write first word
    while (++i < num)
    {
        if (i < outLen)
        {
            out = outBuf[i]; // get new output data
        }

        while (!(spi->SR & SPI_SR_RXNE))
        { /* wait for Rx buffer full */ }

        in = spi->DR; // read input
        spi->DR = out; // start output
        if (ii >= 0)
        {
            inBuf[ii] = (UINT8)in; // save input data
        }
        ii++;
    }
    while (!(spi->SR & SPI_SR_RXNE))
    { /* wait for Rx buffer full */ }

    in = spi->DR; // read last input
    if (ii >= 0)
    {
        inBuf[ii] = (UINT8)in; // save last input
    }

    return TRUE;
}

UINT32 CPU_SPI_PortsCount()
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    return STM32F4_SPI_MODS;
}

void CPU_SPI_GetPins( UINT32 spi_mod, GPIO_PIN& msk, GPIO_PIN& miso, GPIO_PIN& mosi )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SPI();
    msk = miso = mosi = GPIO_PIN_NONE;
    if (spi_mod >= STM32F4_SPI_MODS)
    {
        return;
    }

    msk  = g_STM32F4_Spi_Sclk_Pins[spi_mod];
    miso = g_STM32F4_Spi_Miso_Pins[spi_mod];
    mosi = g_STM32F4_Spi_Mosi_Pins[spi_mod];
}

UINT32 CPU_SPI_MinClockFrequency( UINT32 spi_mod )
{
    // Theoretically this could read the Clock and PLL configurations
    // to determine an actual realistic minimum, however there doesn't
    // seem to be a lot of value in that since the CPU_SPI_Xaction_Start
    // has to determine the applicability of the selected speed at the
    // time a transaction is started anyway.
    return 1;
}

UINT32 CPU_SPI_MaxClockFrequency( UINT32 spi_mod )
{
    // Theoretically this could read the Clock and PLL configurations
    // to determine an actual realistic maximum, however there doesn't
    // seem to be a lot of value in that since the CPU_SPI_Xaction_Start
    // has to determine the applicability of the selected speed at the
    // time a transaction is started anyway.
    // Max supported (e.g. not overclocked) AHB speed / 2
    return 48000000;
}

UINT32 CPU_SPI_ChipSelectLineCount( UINT32 spi_mod )
{
    // This could maintain a map of the actual pins
    // that are available for a particular port.
    // (Not all pins can be mapped to all ports.) 
    // The value of doing that, however, is marginal
    // since the count of possible chip selects doesn't
    // really help in determining which chip select to
    // use so just report the total count of all GPIO
    // pins as possible so that the selected Chip select
    // line coresponds to a GPIO pin number directly
    // without needing any additional translation/mapping.
    return TOTAL_GPIO_PINS;
}
