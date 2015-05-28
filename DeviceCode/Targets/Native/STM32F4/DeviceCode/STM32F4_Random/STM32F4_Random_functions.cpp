////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Oberon microsystems, Inc.
//
//  *** STM32F4 Random Number Generator driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

#include "STM32F4_Random_functions.h"
static UINT32 previous_random;


void Random_Start( )
{
    RCC->AHB2ENR |= RCC_AHB2ENR_RNGEN; // enable RNG clock
    RNG->CR = RNG_CR_RNGEN; // start RNG
    previous_random = Random_Generate(); // get first value
}

void Random_Stop()
{
    RNG->CR = 0; // stop RNG
    RCC->AHB2ENR &= ~RCC_AHB2ENR_RNGEN; // disable RNG clock
}

UINT32 Random_Generate()
{
    // repeat until valid data is present
    while (TRUE) {
        UINT32 status = RNG->SR;
        if (status & RNG_SR_CECS) { // seed error
            RNG->SR = 0; // clear error bits
            RNG->CR = 0; // stop RNG
            RNG->CR = RNG_CR_RNGEN; // restart RNG
        } else if (status & RNG_SR_DRDY) { // data available
            UINT32 data = RNG->DR;
            if (data != previous_random) {
                previous_random = data;
                return data;
            }
        }
    }
}
