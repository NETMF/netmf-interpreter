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


// The first 8 registers are standard for all PHY chips
// control register
#define PHY_CONTROL_REGISTER                0U              // Control register
#define PHY_CR_RESET                        0x8000U         // Reset
#define PHY_CR_SPEED_SELECT                 0x2000U         
#define PHY_CR_ANEGEN                       0x1000U         // Auto-negotiation enable 
#define PHY_CR_PWRDN                        0x0800U         // Power-down
#define PHY_CR_ISOLATE                      0x0400U
#define PHY_CR_ANEG_RESTART                 0x0200U 
#define PHY_CR_DUPLEX_MODE                  0x0100U

// status register
#define PHY_STATUS_REGISTER                 1U              // Status register
#define PHY_SR_ANEGC                        0x0020U         // Auto-negotiation complete
#define PHY_SR_LINK                         0x0004U         // Link status

// ID register
#define PHY_IDENTIFIER_REGISTER_1           2U              // Identifier register 1

#define PHY_IDENTIFIER_REGISTER_2           3U              // Identifier register 2


#define PHY_RMII_ANEG_ADVERT_REGISTER       04U             // Auto negotiation advertisement register
#define PHY_RMII_ANEG_ADVERT_NEXTPAGE       (1<<15)

#define PHY_RMII_ANEG_ADVERT_100BASE_T4     (1<<9)
#define PHY_RMII_ANEG_ADVERT_100BASE_TXF    (1<<8)
#define PHY_RMII_ANEG_ADVERT_100BASE_TX     (1<<7)
#define PHY_RMII_ANEG_ADVERT_10BASE_TF      (1<<6)
#define PHY_RMII_ANEG_ADVERT_10BASE_T       (1<<5)

#define PHY_RMII_ANEG_ADVERT_SPEED_MASK     ( PHY_RMII_ANEG_ADVERT_100BASE_T4 | PHY_RMII_ANEG_ADVERT_100BASE_TXF | PHY_RMII_ANEG_ADVERT_100BASE_TX | PHY_RMII_ANEG_ADVERT_10BASE_TF | PHY_RMII_ANEG_ADVERT_10BASE_T)

#define PHY_AUTO_NEGOTIATION_TIMEOUT        0x00100000U     // Auto negotiation timeout
#define PHY_ST802RT1X_OUI               0x80E1U         // ST802RT1x PHY unique identifier
#define PHY_ST802RT1X_OUI_ID1           0x0203U         // the value directly read out from ID1

#define PHY_RMII_XCNTL_REGISTER         16U
#define PHY_RMII_RXCFG_IIS_REGISTER     17U             // Receive Configuration and Interrupt Status Register
#define PHY_DIAGNOSTIC_REGISTER         18U             // Diagnostic register
#define PHY_ANEGF                       0x1000U         // Auto-negotiation fail indication
#define PHY_RMII_RXCFG_IIS_MODEMASK     (3<<8)          // Speed bit (bit 9 = speed; bit 8 == Duplex )


#define PHY_LINK_TIMEOUT                0x0004FFFFU     // PHY link timeout
#define PHY_RESET_DELAY                 0x000FFFFFU     // PHY reset delay
#define PHY_RESPONSE_TIMEOUT            0x0004FFFFU     // PHY response timeout
#define PHY_TERIDIAN_OUI                0x000EU         // Teridian unique identifier
// for KSZ8081RNB
#define PHY_KENDIN_OUI                  0x10A1U
#define PHY_KENDIN_OUI_ID1              0x0022U

#define PHY_CTRL1_MASK                  0x07
#define PHY_CTRL1_REG                   0x1E

#define PHY_KSZ_100TB 0x2
#define PHY_KSZ_FULLDUPLEX 0x4
#define ETHMODE_100MPS_BIT 0x0200
#define ETHMODE_FULLDPX_BIT 0x0100
enum EthMode
{
    ETHMODE_FAIL = 0xFFFF,
    ETHMODE_10MBPS_HDPX  = 0,
    ETHMODE_10MBPS_FDPX  = ETHMODE_FULLDPX_BIT,
    ETHMODE_100MBPS_HDPX = ETHMODE_100MPS_BIT,
    ETHMODE_100MBPS_FDPX = ETHMODE_100MPS_BIT | ETHMODE_FULLDPX_BIT,
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
