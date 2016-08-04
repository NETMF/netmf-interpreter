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
 * @file    STM32F4_ETH_lwip.cpp
 * @brief   Interface between the LWIP module and the STM32F4 ethernet driver. 
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/
  
//--------------------------------------------------------------------------------------------
// System and local includes
//--------------------------------------------------------------------------------------------
 
#include <tinyhal.h>
#include "lwip/tcpip.h"
#include "STM32F4_ETH_lwip.h"
#include "STM32F4_ETH_driver.h"
#include "STM32F4_ETH_rx_desc.h"
#include "STM32F4_ETH_tx_desc.h"
#include "STM32F4_ETH_gpio.h"

//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------

#define LED_TX_BLOCKING             0
#define LED_PB15                    (1 * 16 + 15)       // PB15
#define TX_TIMEOUT                  0x000000A0U         // ~30us

//--------------------------------------------------------------------------------------------
// Local declarations
//--------------------------------------------------------------------------------------------

static BOOL s_isDriverOpened;                       // Driver opened flag

//--------------------------------------------------------------------------------------------
// Local functions declarations
//--------------------------------------------------------------------------------------------

static uint32_t processFrame(Netif_t *const pNetif);
static uint32_t copyFrameToPbuf(Pbuf_t *pbuf);
static Pbuf_t* copyRxDescToPbuf(Pbuf_t *pbuf, const uint8_t *const pBuffer);
static void forwardFrameToLwip(Netif_t *const pNetif, Pbuf_t *pbuf);
static uint32_t releaseRxDescUntilNextFrame();
static uint32_t releaseFrame();
static BOOL waitForTxDescriptor(uint32_t timeout);
static BOOL copyFrameFromPbuf(const Pbuf_t *pbuf);
static void copyPbufToTxDesc(uint8_t *const pBuffer, const Pbuf_t *pbuf);
static void sendFrame();
static BOOL initEthernet(Netif_t *const pNetif);

//--------------------------------------------------------------------------------------------
// External functions declarations
//--------------------------------------------------------------------------------------------

extern void lwip_interrupt_continuation(void);

//--------------------------------------------------------------------------------------------
// Functions definitions
//--------------------------------------------------------------------------------------------
/**
 * Open the ethernet driver.
 * @param pNetif the lwip network interface.
 * @return Error status.
 *   @retval TRUE if open successful.
 *   @retval FALSE otherwise.
 */
BOOL STM32F4_ETH_LWIP_open(Netif_t *const pNetif)
{ 
    // Init interrupt handler
    eth_initReceiveIntHandler(lwip_interrupt_continuation);

    // Configure MAC DMA
    if ( !initEthernet(pNetif) )
    {
        #ifdef _DEBUG_TRACE
        hal_printf("Ethernet driver cannot be opened\r\n");
        #endif
        
        // Set flag as closed and abort
        s_isDriverOpened = FALSE;
        return FALSE;
    }
  
    // Initialize TX and RX descriptors
    eth_initTxDescriptor();
    eth_initRxDescriptors();
    
    // Enable MAC and DMA transmission and reception
    eth_enableTxRx();
  
    // Set flag as opened
    s_isDriverOpened = TRUE;
    
    #ifdef _DEBUG_TRACE
    hal_printf("Ethernet driver opened\r\n");
    #endif
    
    return TRUE;
}

//--------------------------------------------------------------------------------------------
/**
 * Close the ethernet driver.
 * @param disableClocks whether the ethernet clocks must be disabled.
 *  @arg TRUE if the clocks must be disabled.
 *  @arg FALSE otherwise.
 */
void STM32F4_ETH_LWIP_close(const BOOL disableClocks)
{
    // Set flag as closed
    s_isDriverOpened = FALSE;
    
    // Disable MAC and DMA reception and transmission
    eth_disableTxRx();
    
    if (disableClocks) 
    {
        // Disable ethernet clocks
        eth_disableClocks();
    }
    
    #ifdef _DEBUG_TRACE
    hal_printf("Ethernet driver closed\r\n");
    #endif
}

//--------------------------------------------------------------------------------------------
/**
 * Transfer an ethernet frame from the network to the lwip stack. There might be several frames 
 * received for one RX interrupt.
 * @param pNetif the lwip network interface.
 */
void STM32F4_ETH_LWIP_recv(Netif_t *const pNetif)
{
    uint32_t iRxDesc = 0;
         
    // Scan all RX descriptors owned by the CPU
    while ( !eth_isRxDescOwnedByDma() && (iRxDesc < N_RX_DESC) )
    { 
        // Process current frame
        iRxDesc += processFrame(pNetif);
    }
    
    // Resume DMA reception
    eth_resumeDmaReception();
}

//--------------------------------------------------------------------------------------------
/**
 * Process the frame starting in the current descriptor. The frame might span over several
 * descriptors.
 * @param pNetif the lwip network interface.
 * @return the number of RX descriptors processed.
 */
static uint32_t processFrame(Netif_t *const pNetif)
{
    Pbuf_t *pbuf = NULL;
    uint32_t nRxDesc = 0;

    // Check frame integrity and get its length
    uint16_t frameLength = eth_checkFrameAndGetLength();
    if (frameLength == 0)
    {
        #ifdef _DEBUG_TRACE
        hal_printf("%s: erroneous frame\r\n", __func__);
        #endif
    
        return releaseRxDescUntilNextFrame();
    }

    // Allocate pbuf
    pbuf = pbuf_alloc(PBUF_RAW, frameLength, PBUF_POOL);
    if (!pbuf)
    {
        #ifdef _DEBUG_TRACE
        hal_printf("%s: pbuf_alloc failed, discard current frame\r\n", __func__);
        #endif
        
        return releaseFrame();
    }
    
    // Copy the frame from the RX descriptors to the pbuf
    nRxDesc = copyFrameToPbuf(pbuf);
    
    // Transmit the frame to lwip
    forwardFrameToLwip(pNetif, pbuf);
    
    return nRxDesc;
}

//--------------------------------------------------------------------------------------------
/**
 * Copy the frame starting in the current descriptor to a chain of pbuf. The frame might span
 * over several descriptors.
 * @param pbuf a pointer to the first pbuf of the chain.
 * @return the number of RX descriptors processed.
 */
static uint32_t copyFrameToPbuf(Pbuf_t *pbuf)
{
    uint32_t nRxDesc = 0;
    BOOL isLastDescProcessed = FALSE;
    
    while (!eth_isRxDescOwnedByDma() && !isLastDescProcessed)
    {
        #if DEBUG_RX_DESC
        hal_printf("----- Before processing RX -----\r\n");
        eth_displayRxDescStatus();
        #endif
    
        // Copy the current descriptor
        uint8_t* pBuffer = eth_getRxDescBuffer();
        pbuf = copyRxDescToPbuf(pbuf, pBuffer);
    
        if (eth_isLastDescriptor())
        {
            isLastDescProcessed = TRUE;
        }
    
        // Release the descriptor and point to the next one
        eth_setRxDescOwnedByDma();
        eth_pointToNextRxDesc();
        nRxDesc++;
    
        #if DEBUG_RX_DESC
        hal_printf("----- After processing RX -----\r\n");
        eth_displayRxDescStatus();
        #endif
    }
    return nRxDesc;
}

//--------------------------------------------------------------------------------------------
/**
 * Copy data from an RX descriptor to a chain of pbuf.
 * @param pbuf a pointer to the first pbuf of the chain.
 * @param pBuffer the buffer of an RX descriptor.
 * @return The pbuf after copy.
 */
static Pbuf_t* copyRxDescToPbuf(Pbuf_t *pbuf, const uint8_t *const pBuffer)
{
    uint32_t offset = 0;
    while (pbuf && (offset < RX_BUFFER_LENGTH))
    {
        memcpy(pbuf->payload, (void*)(pBuffer + offset), pbuf->len);
        offset += pbuf->len;
        pbuf = pbuf->next;
    }

    return pbuf;
}

//--------------------------------------------------------------------------------------------
/**
 * Forward a frame contained in a pbuf to the lwip stack.
 * @param pNetif the lwip network interface.
 * @param pbuf the pbuf containing the frame.
 */
static void forwardFrameToLwip(Netif_t *const pNetif, Pbuf_t* pbuf)
{
#if LWIP_NETIF_API
    // post the input packet to the stack
    tcpip_input( pbuf, pNetif );
#else
    // Transmit the frame to the lwip stack
    err_t err = pNetif->input(pbuf, pNetif);
    if (err != ERR_OK)
    {
        pbuf_free(pbuf);
        pbuf = NULL;
    }
#endif
}

//--------------------------------------------------------------------------------------------
/**
 * Release descriptors until the next frame.
 * @return the number of RX descriptors released.
 */
static uint32_t releaseRxDescUntilNextFrame()
{
    uint32_t nRxDesc = 0;

    while (!eth_isRxDescOwnedByDma() && !eth_isFirstDescriptor())
    {
        // Give back the descriptor to the DMA and point to next one
        eth_setRxDescOwnedByDma();
        eth_pointToNextRxDesc();
        nRxDesc++;
    }
    return nRxDesc;
}

//--------------------------------------------------------------------------------------------
/**
 * Release the descriptors containing a frame.
 * @return the number of RX descriptors released.
 */
static uint32_t releaseFrame()
{
    uint32_t nRxDesc = 0;
    BOOL isLastDescProcessed = FALSE;
    
    while (!eth_isRxDescOwnedByDma() && !isLastDescProcessed)
    {
        if (eth_isLastDescriptor())
        {
            isLastDescProcessed = TRUE;
        }
    
        // Give back the descriptor to the DMA and point to next one
        eth_setRxDescOwnedByDma();
        eth_pointToNextRxDesc();
        nRxDesc++;
    }
    return nRxDesc;
}

//--------------------------------------------------------------------------------------------
/**
 * Transfer an ethernet frame from the lwip stack to the network.
 * @param pNetif the lwip network interface (not used but needed so that signature matches lwip).
 * @param pBuf the pbuf containing the ethernet frame.
 * @return Error status (always ERR_OK)
 */
err_t STM32F4_ETH_LWIP_xmit(Netif_t *const pNetif, Pbuf_t *const pbuf)
{
    // Check whether driver is opened. Prevents blocking when booting without ethernet cable
    if (!s_isDriverOpened)
    {
        return ERR_OK;
    }
  
    // Check pbuf
    if (!pbuf)
    {
        return ERR_OK;
    }
  
    // Block until the last packet has been sent
    if (!waitForTxDescriptor(TX_TIMEOUT))
    {
        return ERR_OK;
    }
  
    // Copy frame to TX descriptors
    if (!copyFrameFromPbuf(pbuf))
    {
        return ERR_OK;
    }
    
    // Send frame
    sendFrame();
  
    // Point to next descriptor
    eth_pointToNextTxDesc();
  
    return ERR_OK;
}

//--------------------------------------------------------------------------------------------
/**
 * Block until the TX descriptor is available or a timeout elapsed.
 * @param timeout the timeout.
 * @return TX descriptor availability.
 *   @retval TRUE if the TX descriptor is available.
 *   @retval FALSE if the timeout elapsed.
 */
static BOOL waitForTxDescriptor(uint32_t timeout)
{
    volatile uint32_t nWait = 0U;

    #if LED_TX_BLOCKING
    CPU_GPIO_EnableOutputPin(LED_PB15, TRUE);
    #endif
    
    while (eth_isTxDescOwnedByDma() && (nWait < timeout) )
    {
        nWait++;
    }
    
    #if LED_TX_BLOCKING
    CPU_GPIO_EnableOutputPin(LED_PB15, FALSE);
    #endif
    
    if (nWait == timeout)
    {
        //#ifdef _DEBUG
        //hal_printf("%s: TX descriptor owned by DMA\r\n", __func__);
        //#endif
    
        return FALSE;
    }
    
    return TRUE;
}

//--------------------------------------------------------------------------------------------
/**
 * Copy the frame contained in a chain of pbuf to the current TX descriptor.
 * @param pbuf the pbuf containing the frame.
 * @return Error status.
 *   @retval TRUE if copy successful.
 *   @retval FALSE otherwise.
 */
static BOOL copyFrameFromPbuf(const Pbuf_t *pbuf)
{
    uint8_t *txBuffer;
    uint32_t frameLength = pbuf->tot_len;

    // Check frame length
    if (frameLength > TX_BUFFER_LENGTH)
    {
        #ifdef _DEBUG_TRACE
        hal_printf("%s: Frame larger than TX buffer (%d)\r\n", __func__, frameLength);
        #endif

        return FALSE;
    }

    // Copy frame
    txBuffer = eth_getTxDescBuffer();
    copyPbufToTxDesc(txBuffer, pbuf);

    // Set length in descriptor
    eth_setFrameLength(frameLength);

    return TRUE;
}

//--------------------------------------------------------------------------------------------
/**
 * Copy data from a chain of pbuf to a TX descriptor.
 * @param pBuffer the buffer of a TX descriptor.
 * @param pbuf a pointer to the first pbuf of a chain.
 * @return the number of bytes copied.
 */
static void copyPbufToTxDesc(uint8_t *const pBuffer, const Pbuf_t *pbuf)
{
    uint32_t copiedLength = 0;
    
    while (pbuf && (copiedLength < TX_BUFFER_LENGTH))
    {
        memcpy(pBuffer + copiedLength, pbuf->payload, pbuf->len);
        copiedLength += pbuf->len;
        pbuf = pbuf->next;
    }
}

//--------------------------------------------------------------------------------------------
/**
 * Send the frame contained in the current TX descriptors.
 */
static void sendFrame()
{
    eth_setTxDescOwnedByDma();
    eth_resumeDmaTransmission();
}

//--------------------------------------------------------------------------------------------
/**
 * Ethernet Interrupt handler.
 * @param pNetif the lwip network interface.
 */
void STM32F4_ETH_LWIP_interrupt(Netif_t *const pNetif)
{
    // Disable preempting
    GLOBAL_LOCK(irq);
  
    eth_dmaInterruptHandler();
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize the ethernet module.
 * @return Error status.
 *   @retval TRUE if initialization successful.
 *   @retval FALSE otherwise.
 */
static BOOL initEthernet(Netif_t *const pNetif)
{   
    EthMode mode = ETHMODE_FAIL;

    // Initialize Ethernet GPIOs
    eth_initEthGpio();

    // Select MII interface
    eth_selectMii();
    
    // Enable ethernet clocks
    eth_enableClocks();
   
    // Reset MAC module
    if (!eth_macReset())
    {
        return FALSE;
    }

    // Initialize DMA and MAC registers
    eth_initDmaMacRegisters();

    // Reset PHY
    if (!eth_phyReset())
    {
        return FALSE;
    }
  
    // Check link
    if (!eth_isPhyLinkValid(TRUE))
    {
        return FALSE;
    }
  
    // Enable Auto-Negotiation
    mode = eth_enableAutoNegotiation();
    if (mode == ETHMODE_FAIL)
    {
        return FALSE;
    }    

    LWIP_PLATFORM_DIAG(("AutoNegotiation completed with:"));
    switch (mode)
    {
    case ETHMODE_100MBPS_HDPX:
        ETH->MACCR |= ETH_MACCR_FES;
        ETH->MACCR |= ETH_MACCR_DM; //ETH->MACCR &= ~ETH_MACCR_DM; // HACK: force Full duplex mode as HD won't send - we only need HD for HUB and we only need that for network analyzers at the moment
        LWIP_PLATFORM_DIAG((" 100MB Half Duplex\n"));
        break;

    case ETHMODE_100MBPS_FDPX:
        ETH->MACCR |= ETH_MACCR_FES;
        ETH->MACCR |= ETH_MACCR_DM;
        LWIP_PLATFORM_DIAG((" 100MB Full Duplex\n"));
        break;

    case ETHMODE_10MBPS_HDPX:
        ETH->MACCR &= ~ETH_MACCR_FES;
        ETH->MACCR |= ETH_MACCR_DM; //ETH->MACCR &= ~ETH_MACCR_DM; // HACK: force Full duplex mode as HD won't send - we only need HD for HUB and we only need that for network analyzers at the moment
        LWIP_PLATFORM_DIAG((" 10MB Half Duplex\n"));
        break;

    case ETHMODE_10MBPS_FDPX:
        ETH->MACCR &= ~ETH_MACCR_FES;
        ETH->MACCR |= ETH_MACCR_DM;
        LWIP_PLATFORM_DIAG((" 10MB Full Duplex\n"));
        break;

    default:
        LWIP_PLATFORM_DIAG((" INVALID!\n"));
        return FALSE;
    }

    // Initialize MAC address
    eth_initMacAddress(pNetif->hwaddr);
  
    return TRUE;
}

//--------------------------------------------------------------------------------------------
