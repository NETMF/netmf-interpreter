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
 * @file    STM32F4_ETH_phy.h
 * @brief   PHY (TERIDIAN 78Q2123) driver. 
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/
  
#ifndef STM32F4_ETH_PHY_H
#define STM32F4_ETH_PHY_H

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

enum EthMode
{
    ETHMODE_FAIL = 0xFFFF,
    ETHMODE_10MBPS_HDPX = 0x0000,
    ETHMODE_10MBPS_FDPX = 0x0100,
    ETHMODE_100MBPS_HDPX = 0x0200,
    ETHMODE_100MBPS_FDPX = 0x0300,
};

//--------------------------------------------------------------------------------------------
// Typedefs and enums
//--------------------------------------------------------------------------------------------

typedef BOOL (*pRead)(const uint32_t, uint16_t *const);
typedef BOOL (*pWrite)(const uint32_t, const uint16_t);

//--------------------------------------------------------------------------------------------
// Functions prototypes
//--------------------------------------------------------------------------------------------

void initReadPhyCallback(pRead readCallback);
void initWritePhyCallback(pWrite writeCallback);

BOOL eth_phyReset();
BOOL eth_isPhyLinkValid(BOOL isCallBlocking);
EthMode eth_enableAutoNegotiation();
BOOL eth_powerUpPhy(BOOL isPowerUp);
BOOL eth_isPhyResponding(void);

//--------------------------------------------------------------------------------------------
#ifdef __cplusplus
}
#endif

#endif  // STM32F4_ETH_PHY_H
