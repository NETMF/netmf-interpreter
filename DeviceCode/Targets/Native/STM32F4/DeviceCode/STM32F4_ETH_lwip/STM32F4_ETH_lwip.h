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
 * @file    STM32F4_ETH_lwip.h
 * @brief   Interface between the LWIP module and the STM32F4 ethernet driver. 
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/
  
#ifndef STM32F4_ETH_LWIP_H
#define STM32F4_ETH_LWIP_H

#ifdef __cplusplus
extern "C" {
#endif

//--------------------------------------------------------------------------------------------
// System and local includes
//--------------------------------------------------------------------------------------------

#include <tinyhal.h>
#include <lwip\pbuf.h>
#include <lwip\netif.h>

//--------------------------------------------------------------------------------------------
// Typedefs and enums
//--------------------------------------------------------------------------------------------

typedef struct pbuf Pbuf_t;
typedef struct netif Netif_t;

//--------------------------------------------------------------------------------------------
// Functions prototypes
//--------------------------------------------------------------------------------------------

BOOL STM32F4_ETH_LWIP_open(Netif_t *const pNetif);
void STM32F4_ETH_LWIP_close(const BOOL disableClocks);
void STM32F4_ETH_LWIP_recv(Netif_t *const pNetif);
err_t STM32F4_ETH_LWIP_xmit(Netif_t *const pNetif, Pbuf_t *const pBuf);
void STM32F4_ETH_LWIP_interrupt(Netif_t *const pNetif);

//--------------------------------------------------------------------------------------------
#ifdef __cplusplus
}
#endif

#endif  // STM32F4_ETH_LWIP_H
