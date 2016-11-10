//////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Open Technologies. All rights reserved.
//  Implementation for CMSIS: Copyright (c) Eclo Solutions Lda
//
//  *** Interface for Cortex-M CRC Calculation Unit ***
//
//////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#define CRC_BASE            (AHB1PERIPH_BASE + 0x3000U)
#define CRC                 ((CRC_TypeDef *) CRC_BASE)

typedef struct
{
  CRC_TypeDef                 *Instance;  /*!< Register base address   */
}CRC_HandleTypeDef;


/*
    Interface to Cortex-M CRC calculation unit that implements the equivalent to the software implementation at Support\CRC project.
    CRC-32 (Ethernet) polynomial: 0x4C11DB7.
    buffer: pointer to the region block to be CRCed
    size: lenght of buffer to compute CRC
    crc: previous CRC value to start CRC computing
*/

/* NOTE: is good for F1, L1, F2 and F4 units series, other have diferent configurations and polynomial coefficients */

static UINT32 ReverseCRC32(UINT32 targetCRC)
{
    // nibble lookup table for _REVERSE_ CRC32 polynomial 0x4C11DB7
    static const UINT32 crc32NibbleTable[16] =
    {
        0x00000000, 0xB2B4BCB6, 0x61A864DB, 0xD31CD86D, 0xC350C9B6, 0x71E47500, 0xA2F8AD6D, 0x104C11DB,
        0x82608EDB, 0x30D4326D, 0xE3C8EA00, 0x517C56B6, 0x4130476D, 0xF384FBDB, 0x209823B6, 0x922C9F00
    };
    UINT8 counter = 8;

    while(counter--)
    {
        targetCRC = (targetCRC >> 4) ^ crc32NibbleTable[targetCRC & 0x0F];
    }

    return targetCRC;
}

UINT32 FastCRC32(UINT32 initial_crc, uint8_t data)
{
    // nibble lookup table for CRC32 polynomial 0x4C11DB7
    static const UINT32 crc32NibbleTable[16] = 
    { 
        0x00000000, 0x04C11DB7, 0x09823B6E, 0x0D4326D9, 0x130476DC, 0x17C56B6B, 0x1A864DB2, 0x1E475005,
        0x2608EDB8, 0x22C9F00F, 0x2F8AD6D6, 0x2B4BCB61, 0x350C9B64, 0x31CD86D3, 0x3C8EA00A, 0x384FBDBD 
    };

    initial_crc = crc32NibbleTable[(initial_crc >> 28) ^ (data >> 4)] ^ (initial_crc << 4) ;
    initial_crc = crc32NibbleTable[(initial_crc >> 28) ^ (data & 0xF)] ^ (initial_crc << 4) ;

    return initial_crc;
}

UINT32 SUPPORT_ComputeCRC(const void* buffer, int size, UINT32 initCrc)
{
    CRC_HandleTypeDef hcrc;

    uint32_t index = 0U;
    uint32_t arg1;
    uint32_t size_remainder = 0U;
    volatile UINT32 crc, crc_temp;
    volatile UINT32 targetCRC;
    volatile UINT32 currentCRC;

    // anything to do here?
    if(size == 0)
    {
        return initCrc;
    }

    // init CRC unit
    hcrc.Instance = CRC;

    // get pointer to buffer
    uint8_t* ptr = (uint8_t*)buffer;

    // need to reset CRC peripheral if:
    // - CRC initial value is 0 
    // - the initial CRC is NOT already loaded in the calculation register 
    if(initCrc == 0 || (hcrc.Instance->DR != initCrc))
    {
        // Reset CRC Calculation Unit
        (&hcrc)->Instance->CR |= CRC_CR_RESET;

        // CRC calculation unit is initiated with 0xFFFFFFFF which is not a initial value for our CRC calculation
        // feeding 0xFFFFFFFF to the calculation unit will set the register to 0x00000000
        while(hcrc.Instance->DR != 0x0)
        {
             hcrc.Instance->DR = hcrc.Instance->DR;
        }
    }

    if(initCrc != 0 && hcrc.Instance->DR != initCrc)
    {
        // we have an initial value for CRC calculation and that is not loaded in the CRC calculation register
        // load calculation register with REVERSE initial CRC32 value (because of STM32F4 shift order)
        hcrc.Instance->DR = ReverseCRC32(initCrc);
    }

    // set variable to hold how many bytes remain after processing the buffer in steps of 4 bytes
    size_remainder = size & 3;

    // we'll be reading the buffer in steps of 4 bytes, so the size must be recalculated accordingly
    size = size >> 2;

    // feed data into the CRC calculator
    for(index = 0U; index < size; index++)
    {
        // take the next 4 bytes as if they were a UINT32
        // because the CRC calculation unit expects the bytes in reverse order, reverse the byte order first
        arg1 = __REV(*(uint32_t*)(ptr));

        // feed the bytes to the CRC
        hcrc.Instance->DR = arg1;

        // copy to return value
        crc = (uint32_t)hcrc.Instance->DR;

        // increase pointer by 4 to the next position
        // !! we are reading UINT32 from a UINT8 pointer!!
        ptr +=4;
    }

    // compute CRC for remaining bytes, if any
    while(size_remainder--)
    {
        crc = FastCRC32(crc, *(uint8_t*)(ptr++));
    }

    // Return the CRC computed value
    return crc;
}
