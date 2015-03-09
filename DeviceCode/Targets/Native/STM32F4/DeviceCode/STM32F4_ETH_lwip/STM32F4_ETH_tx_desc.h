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
 * @file    STM32F4_ETH_tx_desc.h
 * @brief   TX descriptors handler.
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/

#ifndef STM32F4_ETH_TX_DESC_H
#define STM32F4_ETH_TX_DESC_H

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

#define DEBUG_TX_DESC                   0

#define N_TX_DESC                       3U              // Number of chained TX descriptors
#define TX_BUFFER_LENGTH                1522U           // TX buffer length

//--------------------------------------------------------------------------------------------
// Functions prototypes
//--------------------------------------------------------------------------------------------

void eth_initTxDescriptor();
uint8_t* eth_getTxDescBuffer();
void eth_pointToNextTxDesc();
void eth_setFrameLength(uint32_t length);
BOOL eth_isTxDescOwnedByDma();
void eth_setTxDescOwnedByDma();

#if DEBUG_TX_DESC
void eth_displayTxDescStatus();
#endif

//--------------------------------------------------------------------------------------------
#ifdef __cplusplus
}
#endif

#endif  // STM32F4_ETH_TX_DESC_H
