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
 * @file    STM32F4_ETH_gpio.cpp
 * @brief   Ethernet GPIO driver.
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/

//--------------------------------------------------------------------------------------------
// System and local includes
//--------------------------------------------------------------------------------------------

#include <tinyhal.h>

#include "STM32F4_ETH_gpio.h"

//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------

#define ETH_PA0                     0x00U
#define ETH_PA1                     0x01U
#define ETH_PA2                     0x02U
#define ETH_PA3                     0x03U
#define ETH_PA7                     0x07U

#define ETH_PB0                     0x10U
#define ETH_PB1                     0x11U
#define ETH_PB8                     0x18U
#define ETH_PB10                    0x1AU
#define ETH_PB11                    0x1BU
#define ETH_PB12                    0x1CU
#define ETH_PB13                    0x1DU

#define ETH_PC1                     0x21U
#define ETH_PC2                     0x22U
#define ETH_PC3                     0x23U
#define ETH_PC4                     0x24U
#define ETH_PC5                     0x25U

#define ETH_PD0                     0x30U
#define ETH_PE0                     0x40U
#define ETH_PF0                     0x50U
#define ETH_PG0                     0x60U
#define ETH_PG11                    0x6BU
#define ETH_PG13                    0x6DU
#define ETH_PG14                    0x6EU

#define ETH_AF11                    0x2B2U // speed = 2 (50MHz), af = 11 (Ethernet)

//--------------------------------------------------------------------------------------------
// Functions definitions
//--------------------------------------------------------------------------------------------
/**
 * Configure the ethernet GPIOs and enable their clocks.
 */
void eth_initEthGpio() 
{
    // Configure ethernet GPIOs
    GPIO_PIN Eth_Mdio = ETH_PA2;
    GPIO_PIN Eth_Mdc = ETH_PC1;
    CPU_GPIO_DisablePin(Eth_Mdio, RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11);
    CPU_GPIO_DisablePin(Eth_Mdc, RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11);

    // Configure PHY GPIOs
#if STM32F4_ETH_PHY_MII
    GPIO_PIN Eth_Mii_Crs = ETH_PA0;
    GPIO_PIN Eth_Mii_Col = ETH_PA3;
    GPIO_PIN Eth_Mii_Rxd2 = ETH_PB0;
    GPIO_PIN Eth_Mii_Rxd3 = ETH_PB1;
    GPIO_PIN Eth_Mii_Txd3 = ETH_PB8;
    GPIO_PIN Eth_Mii_Rxer = ETH_PB10;
    GPIO_PIN Eth_Mii_Txen = ETH_PB11;
    GPIO_PIN Eth_Mii_Txd0 = ETH_PB12;
    GPIO_PIN Eth_Mii_Txd1 = ETH_PB13;
    GPIO_PIN Eth_Mii_Rxclk = ETH_PA1;
    GPIO_PIN Eth_Mii_Rxdv = ETH_PA7;
    GPIO_PIN Eth_Mii_Txd2 = ETH_PC2;
    GPIO_PIN Eth_Mii_Txclk = ETH_PC3;
    GPIO_PIN Eth_Mii_Rxd0 = ETH_PC4;
    GPIO_PIN Eth_Mii_Rxd1 = ETH_PC5;
    
    CPU_GPIO_DisablePin( Eth_Mii_Crs  , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Col  , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Rxd2 , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Rxd3 , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Txd3 , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Rxer , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Txen , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Txd0 , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Txd1 , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Rxclk, RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Rxdv , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Txd2 , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Txclk, RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Rxd0 , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Mii_Rxd1 , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
#elif STM32F4_ETH_PHY_RMII
    GPIO_PIN Eth_Rmii_TxD0     = ETH_PG13;
    GPIO_PIN Eth_Rmii_TxD1     = ETH_PG14;
    GPIO_PIN Eth_Rmii_Tx_En    = ETH_PG11;
    GPIO_PIN Eth_Rmii_RxD0     = ETH_PC4;
    GPIO_PIN Eth_Rmii_Rxd1     = ETH_PC5;
    GPIO_PIN Eth_Rmii_Ref_Clk  = ETH_PA1;
    GPIO_PIN Ethr_Rmii_Crs_Div = ETH_PA7;

    CPU_GPIO_DisablePin( Eth_Rmii_TxD0     , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Rmii_TxD1     , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Rmii_Tx_En    , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Rmii_RxD0     , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Rmii_Rxd1     , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Eth_Rmii_Ref_Clk  , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
    CPU_GPIO_DisablePin( Ethr_Rmii_Crs_Div , RESISTOR_DISABLED, 0, (GPIO_ALT_MODE)ETH_AF11 );
#else
#error No supported PHY mode specified - supported PHY modes: STM32F4_ETH_PHY_MII, STM32F4_ETH_PHY_RMII
#endif
}

//--------------------------------------------------------------------------------------------
