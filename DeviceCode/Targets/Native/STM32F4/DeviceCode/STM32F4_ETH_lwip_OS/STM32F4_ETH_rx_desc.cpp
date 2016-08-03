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
 * @file    STM32F4_ETH_rx_desc.cpp
 * @brief   RX descriptors handler.
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/

//--------------------------------------------------------------------------------------------
// System and local includes
//--------------------------------------------------------------------------------------------

#include <tinyhal.h>

#include "STM32F4_ETH_rx_desc.h"
#include "STM32F4_ETH_driver.h"

//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------

#define RDES0_OWNED_BY_DMA              0x80000000U     // Own bit
#define RDES0_FL_POSITION               16U             // Frame length bit position
#define RDES0_FL_MASK                   0x3FFF0000U     // Frame length bit mask
#define RDES0_FS                        0x00000200U     // First descriptor
#define RDES0_LS                        0x00000100U     // Last descriptor
#define RDES1_RCH                       0x00004000U     // Second address chained
#define RDES1_RBS1_POSITION             0U              // Receive buffer 1 size bit position
#define RDES1_RBS1_MASK                 0x00001FFFU     // Receive buffer 1 size bit mask
#define RDES1_RBS2_POSITION             16U             // Receive buffer 2 size bit position
#define RDES1_RBS2_MASK                 0x1FFF0000U     // Receive buffer 2 size bit mask

#define BUFFER_ADDRESS_NULL             0x00000000U     // Address for null buffer pointers
#define FRAME_CRC_LENGTH                4U              // CRC length

//--------------------------------------------------------------------------------------------
// Typedefs and enums
//--------------------------------------------------------------------------------------------

// RX descriptor. Enhanced descriptor mode not enabled.
typedef struct
{
    volatile uint32_t rdes0;                    // Own, status
    uint32_t rdes1;                             // Buffer 1 byte count
    uint32_t rdes2;                             // Buffer 1 address
    uint32_t rdes3;                             // Next descriptor address
} RxDesc_t;

//--------------------------------------------------------------------------------------------
// Local declarations
//--------------------------------------------------------------------------------------------

static RxDesc_t s_rxDescriptor[N_RX_DESC];      // RX descriptors
static RxDesc_t *s_pRxDesc;                     // Pointer on the current RX descriptor
static uint8_t s_rxBuffer[N_RX_DESC][RX_BUFFER_LENGTH]; // RX buffers

void ZeroRxDesc()
{
    s_pRxDesc = NULL;
    memset(s_rxDescriptor, 0, sizeof(s_rxDescriptor));
    memset(s_rxBuffer, 0, sizeof(s_rxBuffer));
}

//--------------------------------------------------------------------------------------------
// Local functions declarations
//--------------------------------------------------------------------------------------------

static uint16_t getCurrentDescriptorLength();

//--------------------------------------------------------------------------------------------
// Functions definitions
//--------------------------------------------------------------------------------------------
/**
 * Initialize the RX descriptors as a chain structure.
 */
void eth_initRxDescriptors()
{
    uint32_t i;

    // Initialize the descriptor pointer to the first RX descriptor
    s_pRxDesc = &s_rxDescriptor[0];

    // Initialize each descriptor
    for (i = 0; i < N_RX_DESC; i++)
    {
        // Owned by DMA
        s_rxDescriptor[i].rdes0 = RDES0_OWNED_BY_DMA;

        // Chained structure
        s_rxDescriptor[i].rdes1 |= RDES1_RCH;

        // Buffer size
        s_rxDescriptor[i].rdes1 |= RX_BUFFER_LENGTH;

        // Buffer address
        s_rxDescriptor[i].rdes2 = (uint32_t)s_rxBuffer[i];

        // Assign next descriptor address
        if (i < (N_RX_DESC - 1))
        {
            s_rxDescriptor[i].rdes3 = (uint32_t)&s_rxDescriptor[i + 1];
        }
        else
        {
            s_rxDescriptor[i].rdes3 = (uint32_t)&s_rxDescriptor[0];
        }
    }

    #if DEBUG_RX_DESC
    hal_printf("----- After init RX desc -----\r\n");
    eth_displayRxDescStatus();
    #endif
    
    // Initialize receive descriptor list address register
    eth_initRxDescList((uint32_t)&s_rxDescriptor[0]);
}

//--------------------------------------------------------------------------------------------
/**
 * Get the buffer of the current RX descriptor.
 * @return The buffer.
 */
uint8_t* eth_getRxDescBuffer()
{
    if (!s_pRxDesc)
    {
        return NULL;
    }

    return (uint8_t*)s_pRxDesc->rdes2;
}

//--------------------------------------------------------------------------------------------
/**
 * Move the current RX descriptor pointer to the next descriptor in the chain.
 */
void eth_pointToNextRxDesc()
{
    if (!s_pRxDesc)
    {
        return;
    }

    // Go to next descriptor
    s_pRxDesc = (RxDesc_t*)s_pRxDesc->rdes3;
}

//--------------------------------------------------------------------------------------------
/**
 * Test whether the current RX descriptor is owned by the DMA.
 * @return Ownership of the RX descriptor.
 *   @retval TRUE if owned by DMA.
 *   @retval FALSE if owned by CPU.
 */
BOOL eth_isRxDescOwnedByDma()
{
    if (!s_pRxDesc)
    {
        return FALSE;
    }

    return (s_pRxDesc->rdes0 & RDES0_OWNED_BY_DMA) == RDES0_OWNED_BY_DMA;
}

//--------------------------------------------------------------------------------------------
/**
 * Check the first descriptor status of the current descriptor.
 * @return First descriptor status.
 *   @retval TRUE if the current descriptor is the first descriptor of the frame.
 *   @retval FALSE otherwise.
 */
BOOL eth_isFirstDescriptor()
{
    if (!s_pRxDesc)
    {
        return FALSE;
    }
    
    return (s_pRxDesc->rdes0 & RDES0_FS) == RDES0_FS;
}

//--------------------------------------------------------------------------------------------
/**
 * Check the last descriptor status of the current descriptor.
 * @return Last descriptor status.
 *   @retval TRUE if the current descriptor is the last descriptor of the frame.
 *   @retval FALSE otherwise.
 */
BOOL eth_isLastDescriptor()
{
    if (!s_pRxDesc)
    {
        return FALSE;
    }

    return (s_pRxDesc->rdes0 & RDES0_LS) == RDES0_LS;
}

//--------------------------------------------------------------------------------------------
/**
 * Give ownership of the current RX descriptor to the DMA.
 */
void eth_setRxDescOwnedByDma()
{
    if (!s_pRxDesc)
    {
        return;
    }

    // Set own bit and clear other status.
    s_pRxDesc->rdes0 = RDES0_OWNED_BY_DMA;
}

//--------------------------------------------------------------------------------------------
/**
 * Check the integrity of the descriptors containing a frame, and compute its length.
 * @return The length of the frame without CRC, or 0 if the frame is erroneous.
 */
uint16_t eth_checkFrameAndGetLength()
{
    uint16_t length = 0;
    RxDesc_t *pSavedRxDesc = s_pRxDesc;

    if (!s_pRxDesc)
    {
        return 0;
    }
    
    if (eth_isRxDescOwnedByDma() || !eth_isFirstDescriptor())
    {
        s_pRxDesc = pSavedRxDesc;
        return 0;
    }

    while (!eth_isRxDescOwnedByDma() && !eth_isLastDescriptor())
    {
        eth_pointToNextRxDesc();
        if (s_pRxDesc->rdes2 == BUFFER_ADDRESS_NULL)
        {
            s_pRxDesc = pSavedRxDesc;
            return 0;
        }
    }

    if (eth_isRxDescOwnedByDma() || !eth_isLastDescriptor())
    {
        s_pRxDesc = pSavedRxDesc;
        return 0;
    }

    length = getCurrentDescriptorLength();
    s_pRxDesc = pSavedRxDesc;
    return length;
}

//--------------------------------------------------------------------------------------------
/**
 * Display the status of the RX descriptors.
 */
#if DEBUG_RX_DESC
void eth_displayRxDescStatus()
{
    if (!s_pRxDesc)
    {
        return;
    }

    for (uint32_t i = 0; i < N_RX_DESC; i++)
    {
        hal_printf("%d: 0x%08x %s\r\n", i, s_rxDescriptor[i].rdes0, 
                     s_pRxDesc == &s_rxDescriptor[i] ? "<===" : "");
    }
}
#endif

//--------------------------------------------------------------------------------------------
/**
 * Get the frame length status of the current descriptor.
 * @return the frame length of the current descriptor
 */
static uint16_t getCurrentDescriptorLength()
{
    if (!s_pRxDesc)
    {
        return 0;
    }

    return (uint16_t)((s_pRxDesc->rdes0 & RDES0_FL_MASK) >> RDES0_FL_POSITION)
            - FRAME_CRC_LENGTH;
}

//--------------------------------------------------------------------------------------------
