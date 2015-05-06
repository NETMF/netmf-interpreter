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
 * @file    STM32F4_ETH_driver.h
 * @brief   STM32F4 ethernet driver. 
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/
  
#ifndef STM32F4_ETH_DRIVER_H
#define STM32F4_ETH_DRIVER_H

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

#include "STM32F4_ETH_phy.h"


//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------

// Debug switches
#define DEBUG_DMA_INT               0

// PHY
#define PHY_ADDRESS                     0x01U           // default PHY address

// MACMIIAR
#define MACMIIAR_PA_POSITION            11U             // PHY address bit position
#define MACMIIAR_MR_POSITION            6U              // MII register bit position
#define MII_BUSY_TIMEOUT                0x00050000U     // PHY read and write register timeout

// Timeouts
#define MAC_RESET_TIMEOUT               0x00050000U     // MAC reset timeout
#define FLUSH_TX_FIFO_TIMEOUT           0x00050000U     // Flush TX FIFO timeout

// Enable normal DMA interrupts
#define INT_ENABLE_NIS        1                         // Normal interrupt summary
#define INT_ENABLE_TI         1                         // Transmit
#define INT_ENABLE_TBUI       0                         // Transmit buffer unavailable
#define INT_ENABLE_RI         1                         // Receive
#define INT_ENABLE_ERI        0                         // Early receive
// Enable abnormal DMA interrupts
#define INT_ENABLE_AIS        1                         // Abnormal interrupt summary
#define INT_ENABLE_TPSI       1                         // Transmit process stopped
#define INT_ENABLE_TJTI       1                         // Transmit jabber timeout
#define INT_ENABLE_ROI        1                         // Receive overflow
#define INT_ENABLE_TUI        1                         // Transmit underflow
#define INT_ENABLE_RBUI       1                         // Receive buffer unavailable
#define INT_ENABLE_RPSI       1                         // Receive process stopped
#define INT_ENABLE_RWTI       1                         // Receive watchdog timeout
#define INT_ENABLE_ETI        0                         // Early transmit
#define INT_ENABLE_FBEI       1                         // Fatal bus error

    
//--------------------------------------------------------------------------------------------
// Typedefs and enums
//--------------------------------------------------------------------------------------------

typedef void (*pIntHandler)();

//--------------------------------------------------------------------------------------------
// Functions prototypes
//--------------------------------------------------------------------------------------------

// DMA and MAC functions
void eth_initDmaMacRegisters();
void eth_initMacAddress(const uint8_t *const pAddress);
BOOL eth_macReset();
void eth_selectMii();
void eth_enableClocks();
void eth_disableClocks();
void eth_enableTxRx();
void eth_disableTxRx();
void eth_resumeDmaTransmission();
void eth_resumeDmaReception();
void eth_dmaInterruptHandler();

// Interrupt handler
void eth_initReceiveIntHandler(pIntHandler receiveHandler);

// For PHY driver to read/write PHY registers
BOOL eth_readPhyRegister(uint32_t phyAddress, const uint32_t miiAddress, uint16_t *const pMiiData);
BOOL eth_writePhyRegister(uint32_t phyAddress, const uint32_t miiAddress, const uint16_t miiData);

// Descriptors
void eth_initTxDescList(uint32_t txAddress);
void eth_initRxDescList(uint32_t rxAddress);

//--------------------------------------------------------------------------------------------
#ifdef __cplusplus
}
#endif

#endif  // STM32F4_ETH_DRIVER_H
