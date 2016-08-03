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
 * @file    STM32F4_ETH_tx_desc.cpp
 * @brief   TX descriptors handler.
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/

//--------------------------------------------------------------------------------------------
// System and local includes
//--------------------------------------------------------------------------------------------

#include <tinyhal.h>

#include "STM32F4_ETH_tx_desc.h"
#include "STM32F4_ETH_driver.h"

//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------

#define TDES0_OWNED_BY_DMA              0x80000000U     // Own bit
#define TDES0_IC                        0x40000000U     // Interrupt on completion
#define TDES0_LS                        0x20000000U     // Last segment
#define TDES0_FS                        0x10000000U     // First segment
#define TDES0_TER                       0x00200000U     // Transmit end of ring
#define TDES0_TCH                       0x00100000U     // Second address chained
#define TDES1_TBS1_POSITION             0U              // Transmit buffer 1 size bit position
#define TDES1_TBS1_MASK                 0x00001FFFU     // Transmit buffer 1 size bit mask
#define TDES1_TBS2_POSITION             16U             // Transmit buffer 2 size bit position
#define TDES1_TBS2_MASK                 0x1FFF0000U     // Transmit buffer 2 size bit mask

#define BUFFER_ADDRESS_NULL             0x00000000U     // Address for null buffer pointers

//--------------------------------------------------------------------------------------------
// Typedefs and enums
//--------------------------------------------------------------------------------------------

// TX descriptor. Enhanced descriptor mode not enabled.
typedef struct
{
    volatile uint32_t tdes0;                    // Own, ctrl, ttss, status
    uint32_t tdes1;                             // Buffer 2 byte count, buffer 1 byte count
    uint32_t tdes2;                             // Buffer 1 address
    uint32_t tdes3;                             // Buffer 2 address
} TxDesc_t;

//--------------------------------------------------------------------------------------------
// Local declarations
//--------------------------------------------------------------------------------------------

static TxDesc_t s_txDescriptor[N_TX_DESC];      // TX descriptors
static TxDesc_t *s_pTxDesc;                     // Pointer on the current TX descriptor
static uint8_t s_txBuffer[N_TX_DESC][TX_BUFFER_LENGTH];

void ZeroTxDesc()
{
    s_pTxDesc = NULL;
    memset(s_txDescriptor, 0, sizeof(s_txDescriptor));
    memset(s_txBuffer, 0, sizeof(s_txBuffer));
}

//--------------------------------------------------------------------------------------------
// Functions definitions
//--------------------------------------------------------------------------------------------
/**
 * Initialize the TX descriptors as a chain structure.
 */
void eth_initTxDescriptor()
{
    uint32_t i;

    // Initialize the descriptor pointer to the first TX descriptor
    s_pTxDesc = &s_txDescriptor[0];

    // Initialize each descriptor
    for (i = 0; i < N_TX_DESC; i++)
    {
        // Owned by CPU
        s_txDescriptor[i].tdes0 &= ~TDES0_OWNED_BY_DMA;

        // Chain structure
        s_txDescriptor[i].tdes0 |= TDES0_TCH;

        // First and last segments always contained
        s_txDescriptor[i].tdes0 |= TDES0_LS;
        s_txDescriptor[i].tdes0 |= TDES0_FS;

        // Set buffer
        s_txDescriptor[i].tdes2 = (uint32_t)s_txBuffer[i];

        // Assign next descriptor address
        if (i < (N_TX_DESC - 1))
        {
            s_txDescriptor[i].tdes3 = (uint32_t)&s_txDescriptor[i + 1];
        }
        else
        {
            s_txDescriptor[i].tdes3 = (uint32_t)&s_txDescriptor[0];
        }

        // Set interrupt on completion
        #ifdef INT_ENABLE_TI
        s_txDescriptor[i].tdes0 |= TDES0_IC;
        #endif
    }

    #if DEBUG_TX_DESC
    hal_printf("----- After init TX desc -----\r\n");
    eth_displayTxDescStatus();
    #endif
    
    // Initialize transmit descriptor list address register
    eth_initTxDescList((uint32_t)&s_txDescriptor[0]);
}

//--------------------------------------------------------------------------------------------
/**
 * Get the buffer of the current TX descriptor.
 * @return The buffer.
 */
uint8_t* eth_getTxDescBuffer()
{
    if (!s_pTxDesc)
    {
        return NULL;
    }

    return (uint8_t*)s_pTxDesc->tdes2;
}

//--------------------------------------------------------------------------------------------
/**
 * Move the current TX descriptor pointer to the next descriptor in the chain.
 */
void eth_pointToNextTxDesc()
{
    if (!s_pTxDesc)
    {
        return;
    }

    // Go to next descriptor
    s_pTxDesc = (TxDesc_t*)s_pTxDesc->tdes3;
}

//--------------------------------------------------------------------------------------------
/**
 * Set the length of the outgoing frame in the TX descriptor.
 * @param length the frame length.
 */
void eth_setFrameLength(uint32_t length)
{
    if (!s_pTxDesc)
    {
        return;
    }

    s_pTxDesc->tdes1 = ((length << TDES1_TBS1_POSITION) & TDES1_TBS1_MASK);
}

//--------------------------------------------------------------------------------------------
/**
 * Test whether the TX descriptor is owned by the DMA.
 * @return Ownership of the TX descriptor.
 *   @retval TRUE if owned by DMA.
 *   @retval FALSE if owned by CPU.
 */
BOOL eth_isTxDescOwnedByDma()
{
    if (!s_pTxDesc)
    {
        return FALSE;
    }

    return (s_pTxDesc->tdes0 & TDES0_OWNED_BY_DMA) == TDES0_OWNED_BY_DMA;
}

//--------------------------------------------------------------------------------------------
/**
 * Give ownership of the TX descriptor to the DMA.
 */
void eth_setTxDescOwnedByDma()
{
    if (!s_pTxDesc)
    {
        return;
    }

    // Give TX descriptor to DMA
    s_pTxDesc->tdes0 |= TDES0_OWNED_BY_DMA;
}

//--------------------------------------------------------------------------------------------
/**
 * Display the status of the TX descriptors.
 */
#if DEBUG_TX_DESC
void eth_displayTxDescStatus()
{
    if (!s_pTxDesc)
    {
        return;
    }

    for (uint32_t i = 0; i < N_TX_DESC; i++)
    {
        hal_printf("tdes0: 0x%08x %s\r\n", s_txDescriptor[i].tdes0,
                     s_pTxDesc == &s_txDescriptor[i] ? "<===" : "");
        hal_printf("tdes1: 0x%08x\r\n", s_txDescriptor[i].tdes1);
    }
}
#endif

//--------------------------------------------------------------------------------------------
