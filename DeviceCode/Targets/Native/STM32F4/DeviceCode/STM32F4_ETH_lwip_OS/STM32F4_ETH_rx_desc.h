/*
 * Copyright 2011 CSA Engineering AG
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/*********************************************************************************************
 * @file    STM32F4_ETH_rx_desc.h
 * @brief   RX descriptors handler.
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/

#ifndef STM32F4_ETH_RX_DESC_H
#define STM32F4_ETH_RX_DESC_H

#ifdef __cplusplus
extern "C" {
#endif

//--------------------------------------------------------------------------------------------
// System and local includes
//--------------------------------------------------------------------------------------------

#include <tinyhal.h>

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------

#define DEBUG_RX_DESC                   0

#define N_RX_DESC                       8U              // Number of chained RX descriptors
#define RX_BUFFER_LENGTH                256U            // RX buffer length

//--------------------------------------------------------------------------------------------
// Functions prototypes
//--------------------------------------------------------------------------------------------

void eth_initRxDescriptors();
uint8_t* eth_getRxDescBuffer();
void eth_pointToNextRxDesc();
BOOL eth_isRxDescOwnedByDma();
void eth_setRxDescOwnedByDma();
BOOL eth_isFirstDescriptor();
BOOL eth_isLastDescriptor();
uint16_t eth_checkFrameAndGetLength();

#if DEBUG_RX_DESC
void eth_displayRxDescStatus();
#endif

//--------------------------------------------------------------------------------------------
#ifdef __cplusplus
}
#endif

#endif  // STM32F4_ETH_RX_DESC_H
