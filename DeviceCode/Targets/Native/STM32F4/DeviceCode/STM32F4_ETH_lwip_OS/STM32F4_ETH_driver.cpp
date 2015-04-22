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
 * @file    STM32F4_ETH_driver.cpp
 * @brief   STM32F4 ethernet driver. 
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/
  
//--------------------------------------------------------------------------------------------
// System and local includes
//--------------------------------------------------------------------------------------------
 
#include <tinyhal.h>

#include "STM32F4_ETH_driver.h"
#include "STM32F4_ETH_PHY.h"

//--------------------------------------------------------------------------------------------
// Normal interrupt summary
#if INT_ENABLE_NIS
#define ETH_DMAIER_NISE_MASK         ETH_DMAIER_NISE
#else
#define ETH_DMAIER_NISE_MASK         0
#endif

// Abnormal interrupt summary
#if INT_ENABLE_AIS
#define ETH_DMAIER_AISE_MASK         ETH_DMAIER_AISE         
#else
#define ETH_DMAIER_AISE_MASK         0
#endif

// Early receive
#if INT_ENABLE_ERI
#define  ETH_DMAIER_ERIE_MASK       ETH_DMAIER_ERIE
#else
#define  ETH_DMAIER_ERIE_MASK       0
#endif

// Fatal bus error
#if INT_ENABLE_FBEI
#define  ETH_DMAIER_FBEIE_MASK      ETH_DMAIER_FBEIE
#else
#define  ETH_DMAIER_FBEIE_MASK      0
#endif

// Early transmit interrupt

#if INT_ENABLE_ETI
#define     ETH_DMAIER_ETIE_MASK    ETH_DMAIER_ETIE
#else
#define     ETH_DMAIER_ETIE_MASK    0
#endif

// Receive watchdog timeout
#if INT_ENABLE_RWTI
#define     ETH_DMAIER_RWTIE_MASK   ETH_DMAIER_RWTIE        
#else
#define     ETH_DMAIER_RWTIE_MASK   0        
#endif

// Receive process stopped
#if INT_ENABLE_RPSI
#define     ETH_DMAIER_RPSIE_MASK   ETH_DMAIER_RPSIE        
#else
#define     ETH_DMAIER_RPSIE_MASK   0        
#endif

// Receive buffer unavailable
#if INT_ENABLE_RBUI
#define     ETH_DMAIER_RBUIE_MASK   ETH_DMAIER_RBUIE        
#else
#define     ETH_DMAIER_RBUIE_MASK   0
#endif

// Received

#if INT_ENABLE_RI
#define     ETH_DMAIER_RIE_MASK     ETH_DMAIER_RIE
#else
#define     ETH_DMAIER_RIE_MASK     0
#endif

// Transmit underflow
#if INT_ENABLE_TUI
#define     ETH_DMAIER_TUIE_MASK    ETH_DMAIER_TUIE         
#else
#define     ETH_DMAIER_TUIE_MASK    0
#endif

// Receive overflow
#if INT_ENABLE_ROI
#define     ETH_DMAIER_ROIE_MASK    ETH_DMAIER_ROIE         
#else
#define     ETH_DMAIER_ROIE_MASK    0
#endif

// Transmit jabber timeout
#if INT_ENABLE_TJTI
#define     ETH_DMAIER_TJTIE_MASK   ETH_DMAIER_TJTIE        
#else
#define     ETH_DMAIER_TJTIE_MASK   0
#endif

// Transmit buffer unavailable
#if INT_ENABLE_TBUI
#define     ETH_DMAIER_TBUIE_MASK   ETH_DMAIER_TBUIE        
#else
#define     ETH_DMAIER_TBUIE_MASK   0
#endif

// Transmit process stopped
#if INT_ENABLE_TPSI
#define     ETH_DMAIER_TPSIE_MASK   ETH_DMAIER_TPSIE        
#else
#define     ETH_DMAIER_TPSIE_MASK   0
#endif

// Transmit
#if INT_ENABLE_TI
#define     ETH_DMAIER_TIE_MASK     ETH_DMAIER_TIE          
#else
#define     ETH_DMAIER_TIE_MASK     0
#endif

#define DMA_MASK   (ETH_DMAIER_NISE_MASK | ETH_DMAIER_AISE_MASK | ETH_DMAIER_ERIE_MASK | ETH_DMAIER_FBEIE_MASK |\
                    ETH_DMAIER_ETIE_MASK | ETH_DMAIER_RWTIE_MASK | ETH_DMAIER_RPSIE_MASK | ETH_DMAIER_RBUIE_MASK |\
                    ETH_DMAIER_RIE_MASK  | ETH_DMAIER_TUIE_MASK | ETH_DMAIER_ROIE_MASK | ETH_DMAIER_TJTIE_MASK  |\
                    ETH_DMAIER_TBUIE_MASK | ETH_DMAIER_TPSIE_MASK | ETH_DMAIER_TIE_MASK )


//--------------------------------------------------------------------------------------------
// Local declarations
//--------------------------------------------------------------------------------------------

static void (*receiveIntHandler)();

//--------------------------------------------------------------------------------------------
// Local functions declarations
//--------------------------------------------------------------------------------------------

static void flushTxFifo();
static void initMACCR();
static void initMACFFR();
static void initMACMIIAR();
static void initMACFCR();
static void initMACIMR();
static void initDMABMR();
static void initDMAOMR();

//--------------------------------------------------------------------------------------------
// Functions definitions
//--------------------------------------------------------------------------------------------
/**
 * Initialize the DMA and MAC modules.
 * @return Error status.
 *   @retval TRUE if initialization successful.
 *   @retval FALSE otherwise.
 */
void eth_initDmaMacRegisters()
{
    // Init DMA and MAC registers
    initMACCR();
    initMACFFR();
    initMACFCR();
    initDMAOMR();
    initDMABMR();
    initMACIMR();
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize the 6-byte MAC address of the ethernet interface.
 * @param pAddress a pointer to the MAC address.
 */
void eth_initMacAddress(const uint8_t *const pAddress)
{
    // Initialize ETH_MACA0HR, the ethernet MAC address 0 high register
    ETH->MACA0HR &= ((uint32_t)pAddress[5] << 8U) |
                    ((uint32_t)pAddress[4]);
    
    // Initialize ETH_MACA0LR, the ethernet MAC address 0 low register
    ETH->MACA0LR =  ((uint32_t)pAddress[3] << 24U) |
                    ((uint32_t)pAddress[2] << 16U) |
                    ((uint32_t)pAddress[1] << 8U)  |
                    ((uint32_t)pAddress[0]);
 }

//--------------------------------------------------------------------------------------------
/**
 * Select the PHY interface.
 */ 
void eth_selectMii()
{
   // enable system controller
    RCC->APB2ENR |= RCC_APB2ENR_SYSCFGEN;

    //reset mac
    RCC->AHB1RSTR |= RCC_AHB1RSTR_ETHMACRST;

    // Select PHY interface

#ifdef STM32F4_ETH_PHY_MII
    SYSCFG->PMC &= ~SYSCFG_PMC_MII_RMII;
#else
    SYSCFG->PMC |= SYSCFG_PMC_MII_RMII;
#endif
    RCC->AHB1RSTR &= ~RCC_AHB1RSTR_ETHMACRST;

    // init MII clock    
    initMACMIIAR();

}

//--------------------------------------------------------------------------------------------
/**
 * Enable the ethernet clocks.
 */ 
void eth_enableClocks()
{
    // Enable ethernet clocks
    RCC->AHB1ENR |= ( RCC_AHB1ENR_ETHMACRXEN
                    | RCC_AHB1ENR_ETHMACTXEN
                    | RCC_AHB1ENR_ETHMACEN
                    | RCC_AHB1ENR_ETHMACPTPEN
                    );
}

//--------------------------------------------------------------------------------------------
/**
 * Disable the ethernet clocks.
 */ 
void eth_disableClocks()
{
    // Disable ethernet clocks
    RCC->AHB1ENR &= ~( RCC_AHB1ENR_ETHMACRXEN
                     | RCC_AHB1ENR_ETHMACTXEN
                     | RCC_AHB1ENR_ETHMACEN
                     | RCC_AHB1ENR_ETHMACPTPEN
                     );
}

//--------------------------------------------------------------------------------------------
/**
 * Reset the MAC module. Wait until the reset operation completes or until a timeout elapses.
 * @return Error status.
 *   @retval TRUE if reset successful.
 *   @retval FALSE if timeout elapsed.
 */
BOOL eth_macDMAReset()
{
    volatile uint32_t nWait = 0U;
    BOOL retval = TRUE;
    
    // Software reset
    ETH->DMABMR |= ETH_DMABMR_SR;
    
    // Wait for completion
    while ( ((ETH->DMABMR & ETH_DMABMR_SR) == ETH_DMABMR_SR) && 
            (nWait < MAC_RESET_TIMEOUT) )
    {
        nWait++;
    }
    
    // Check whether timeout elapsed
    if (nWait == MAC_RESET_TIMEOUT)
    {
        retval = FALSE;
    }
    return retval;
}

//--------------------------------------------------------------------------------------------
/**
 * Enable ethernet communication.
 */
void eth_enableTxRx()
{
    // Enable DMA interrupts
    ETH->DMAIER |= ( DMA_MASK );
    
    // Start DMA and MAC reception and transmission
    flushTxFifo();
    ETH->DMAOMR |= ETH_DMAOMR_SR | ETH_DMAOMR_ST;
    ETH->MACCR |= ETH_MACCR_RE | ETH_MACCR_TE;
}

//--------------------------------------------------------------------------------------------
/**
 * Disable ethernet communication.
 */
void eth_disableTxRx()
{
    // Stop MAC and DMA reception and transmission
    ETH->MACCR &= ~(ETH_MACCR_RE | ETH_MACCR_TE);
    ETH->DMAOMR &= ~(ETH_DMAOMR_SR | ETH_DMAOMR_ST);
    
    // Disable DMA interrupts
    ETH->DMAIER &= (~DMA_MASK );
}
 
//--------------------------------------------------------------------------------------------
/**
 * Resume DMA transmission.
 */
void eth_resumeDmaTransmission()
{
    if ((ETH->DMASR & ETH_DMASR_TPS) == ETH_DMASR_TPS_Suspended)
    {
        // Resume DMA transmission
        ETH->DMATPDR = ETH_DMATPDR_TPD;
    }
}

//--------------------------------------------------------------------------------------------
/**
 * Resume DMA reception.
 */
void eth_resumeDmaReception()
{
    if ((ETH->DMASR & ETH_DMASR_RPS) == ETH_DMASR_RPS_Suspended)
    {
        // Resume DMA reception
        ETH->DMARPDR = ETH_DMARPDR_RPD;
    }
}

//--------------------------------------------------------------------------------------------
/**
 * Ethernet interrupt handler
 */
void eth_dmaInterruptHandler()
{
    #if defined (_DEBUG) && DEBUG_DMA_INT
    debug_printf("DMA Int:");
    #endif

    // Normal interrupt summary
    if ((ETH->DMASR & ETH_DMASR_NIS) == ETH_DMASR_NIS)
    {
        #if INT_ENABLE_RI
        if ((ETH->DMASR & ETH_DMASR_RS) == ETH_DMASR_RS)
        {
            // Ethernet frame received
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" RS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_RS;

            // Process receive interrupt
            if (receiveIntHandler)
            {
                receiveIntHandler();
            }
        }
        #endif
        #if INT_ENABLE_TI
        if ((ETH->DMASR & ETH_DMASR_TS) == ETH_DMASR_TS)
        {
            // Ethernet frame sent
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" TS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_TS;
        }
        #endif
        #if INT_ENABLE_TBUI
        if ((ETH->DMASR & ETH_DMASR_TBUS) == ETH_DMASR_TBUS)
        {
            // Transmit buffer unavailable
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" TBUS");
            #endif

            // Clear interrupt flag, transmition is resumed after descriptors have been prepared
            ETH->DMASR = ETH_DMASR_TBUS;
        }
        #endif
        #if INT_ENABLE_ERI
        if ((ETH->DMASR & ETH_DMASR_ERS) == ETH_DMASR_ERS)
        {
            // Early receive
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" ERS");
            #endif

            // Clear interrupt flag. Also cleared automatically by RI
            ETH->DMASR = ETH_DMASR_ERS;
        }
        #endif

        // Clear normal interrupt flag
        ETH->DMASR = ETH_DMASR_NIS;
    }

    // Abnormal interrupt summary
    if ((ETH->DMASR & ETH_DMASR_AIS) == ETH_DMASR_AIS)
    {
        #if INT_ENABLE_FBEI
        if ((ETH->DMASR & ETH_DMASR_FBES) == ETH_DMASR_FBES)
        {
            // Fatal bus error
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" FBES");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_FBES;
        }
        #endif
        #if INT_ENABLE_TPSI
        if ((ETH->DMASR & ETH_DMASR_TPSS) == ETH_DMASR_TPSS)
        {
            // Transmit process stopped
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" TPSS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_TPSS;
        }
        #endif
        #if INT_ENABLE_TJTI
        if ((ETH->DMASR & ETH_DMASR_TJTS) == ETH_DMASR_TJTS)
        {
            // Transmit jabber timeout
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" TJTS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_TJTS;
        }
        #endif
        #if INT_ENABLE_ROI
        if ((ETH->DMASR & ETH_DMASR_ROS) == ETH_DMASR_ROS)
        {
            // Receive overflow
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" ROS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_ROS;
        }
        #endif
        #if INT_ENABLE_TUI
        if ((ETH->DMASR & ETH_DMASR_TUS) == ETH_DMASR_TUS)
        {
            // Transmit underflow
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" TUS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_TUS;
        }
        #endif
        #if INT_ENABLE_RBUI
        if ((ETH->DMASR & ETH_DMASR_RBUS) == ETH_DMASR_RBUS)
        {
            // Receive buffer unavailable
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" RBUS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_RBUS;
        }
        #endif
        #if INT_ENABLE_RPSI
        if ((ETH->DMASR & ETH_DMASR_RPSS) == ETH_DMASR_RPSS)
        {
            // Receive process stopped
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" RPSS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_RPSS;
        }
        #endif
        #if INT_ENABLE_RWTI
        if ((ETH->DMASR & ETH_DMASR_RWTS) == ETH_DMASR_RWTS)
        {
            // Receive watchdog timeout
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" RWTS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_RWTS;
        }
        #endif
        #if INT_ENABLE_ETI
        if ((ETH->DMASR & ETH_DMASR_ETS) == ETH_DMASR_ETS)
        {
            // Early transmit interrupt
            #if defined (_DEBUG) && DEBUG_DMA_INT
            debug_printf(" ETS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_ETS;
        }
        #endif

        // Clear abnormal interrupt flag
        ETH->DMASR = ETH_DMASR_AIS;
    }

    #if defined (_DEBUG) && DEBUG_DMA_INT
    debug_printf("\r\n");
    #endif
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize receive descriptor list address register.
 * @param txAddress the address of the RX descriptor list.
 */
void eth_initTxDescList(uint32_t txAddress)
{
    ETH->DMATDLAR = txAddress;
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize transmit descriptor list address register.
 * @param rxAddress the address of the RX descriptor list.
 */
void eth_initRxDescList(uint32_t rxAddress)
{
    ETH->DMARDLAR = rxAddress;
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize PHY.
 */

//--------------------------------------------------------------------------------------------
/**
 * Initialize the receive interrupt handler.
 */
void eth_initReceiveIntHandler(pIntHandler receiveHandler)
{
    receiveIntHandler = receiveHandler;
}


//--------------------------------------------------------------------------------------------
/**
 * Flush the data in the transmit FIFO. Wait until the reset operation completes or  
 * until a timeout elapses.
 */
static void flushTxFifo()
{
    volatile uint32_t nWait = 0U;
    
    // Flush Transmit FIFO
    ETH->DMAOMR |= ETH_DMAOMR_FTF;
    
    // Wait for completion
    while ( ((ETH->DMAOMR & ETH_DMAOMR_FTF) == ETH_DMAOMR_FTF) && 
            (nWait < FLUSH_TX_FIFO_TIMEOUT) )
    {
        nWait++;
    }
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACCR, the ethernet MAC configuration register.
 */
static void initMACCR()
{
    volatile uint32_t value = 0;

    value = ETH->MACCR;

    // Watchdog enabled
    // Jabber enabled
    // Interframe gap:
    // 96 bit times
    // Carrier sense enabled
    // 100 Mbit/s fast ethernet mode
    // Full-duplex mode
    
    // Receive own enabled
    // Loopback mode disabled
    // Checksum offload disabled
    // Retry disabled
    // Automatic pad/CRC stripping disabled
    // Back-off limit:
    //   min(n, 10)
    // Defferal check disabled
    // Transmitter disabled
    // Received disabled
    value = value & (~( ETH_MACCR_WD 
                      | ETH_MACCR_JD 
                      | ETH_MACCR_IFG 
                      | ETH_MACCR_CSD
                      | ETH_MACCR_FES 
                      | ETH_MACCR_DM 
                      | ETH_MACCR_ROD 
                      | ETH_MACCR_LM 
                      | ETH_MACCR_IPCO 
                      | ETH_MACCR_RD 
                      | ETH_MACCR_APCS 
                      | ETH_MACCR_BL
                      | ETH_MACCR_DC 
                      | ETH_MACCR_TE 
                      | ETH_MACCR_RE));

    value = value | ETH_MACCR_IFG_96Bit 
                  | ETH_MACCR_FES 
                  | ETH_MACCR_DM
                  | ETH_MACCR_RD 
                  | ETH_MACCR_BL_10;
    ETH->MACCR = value;
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACFFR, the ethernet MAC frame filter register.
 */
static void initMACFFR()
{
    uint32_t value;

    value = ETH->MACFFR;

    // Receive all disabled
    // Hash or perfect filter disabled
    // Source address filter disabled
    // Source address inverse filtering disabled
    // Pass control frames:
    // prevents all control frames
    // Broadcast frames enabled
    // Pass all multicast disabled
    // Destination address inverse filtering disabled
    // Hash multicast disabled
    // Hash unicast disabled
    // Promiscuous mode disabled
    value = value & (~( ETH_MACFFR_RA 
                      | ETH_MACFFR_HPF 
                      | ETH_MACFFR_SAF 
                      | ETH_MACFFR_SAIF
                      | ETH_MACFFR_PCF  
                      | ETH_MACFFR_BFD 
                      | ETH_MACFFR_PAM
                      | ETH_MACFFR_DAIF 
                      | ETH_MACFFR_HM 
                      | ETH_MACFFR_HU 
                      | ETH_MACFFR_PM ));
    value = value | ETH_MACFFR_PCF_BlockAll ;
    ETH->MACFFR = value;
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACMIIAR, the ethernet MAC MII adress register.
 */
static void initMACMIIAR()
{ 
    volatile uint32_t value;

    value = ETH->MACMIIAR;
    // Select the clock range HCLK/62
    ETH->MACMIIAR = (value & (~ETH_MACMIIAR_CR)) | ETH_MACMIIAR_CR_Div62;
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACFCR, the ethernet MAC flow control register.
 */
static void initMACFCR()
{
    volatile uint32_t value;

    value = ETH->MACFCR;

    // Pause time: 0
    // Zero-quanta pause disabled
    // Pause low threshold:
    // pause time minus 4 slot times
    // Unicast pause frame detect disabled
    // Receive flow control disabled
    // Transmit flow control disabled
    // Flow control busy/back pressure activate disabled
    value = value & (~( ETH_MACFCR_PT 
                      | ETH_MACFCR_ZQPD 
                      | ETH_MACFCR_PLT
                      | ETH_MACFCR_UPFD 
                      | ETH_MACFCR_RFCE 
                      | ETH_MACFCR_TFCE 
                      | ETH_MACFCR_FCBBPA ));

    value = value | ETH_MACFCR_ZQPD | ETH_MACFCR_PLT_Minus4 ;
    ETH->MACFCR = value;
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACIMR, the ethernet MAC interrupt mask register.
 */
static void initMACIMR()
{
    // Time stamp trigger interrupt disabled,  PMT interrupt disabled
    ETH->MACIMR |= ( ETH_MACIMR_TSTIM  | ETH_MACIMR_PMTIM );   
}
 
//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_DMABMR, the ethernet DMA bus mode register.
 */
static void initDMABMR()
{
    volatile uint32_t value;

    value = ETH->DMABMR;
    // Rx DMA PBL:
    //   32
    // Fixed burst enabled
    // Rx Tx priority ratio:
    //   1:1
    // Programmable burst length:
    //   32
    // Enhanced descriptor format disabled
    // Descriptor skip length: contiguous
    // DMA Arbitration: round robin
    value = value & (~( ETH_DMABMR_AAB 
                      | ETH_DMABMR_FPM 
                      | ETH_DMABMR_USP 
                      | ETH_DMABMR_RDP 
                      | ETH_DMABMR_FB 
                      | ETH_DMABMR_RTPR 
                      | ETH_DMABMR_PBL 
                      | ETH_DMABMR_EDE 
                      | ETH_DMABMR_DSL 
                      | ETH_DMABMR_DA ));
    value = value | ETH_DMABMR_AAB 
                  | ETH_DMABMR_USP 
                  | ETH_DMABMR_RDP_32Beat 
                  | ETH_DMABMR_FB
                  | ETH_DMABMR_RTPR_1_1 
                  | ETH_DMABMR_PBL_32Beat;
    ETH->DMABMR = value;
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_DMAOMR, the ethernet DMA operation mode register.
 */
static void initDMAOMR()
{
    volatile uint32_t value;
    
    value = ETH->DMAOMR;

    // Dropping of TCP/IP checksum error frames enabled
    // Receive store and forwared enabled
    // Flushing of received frames enabled
    // Transmit store and forwared disabled       
    // Transmit threshold control:
    //   16
    // Forward error frames disabled
    // Forward undersized good frames disabled
    // Receive threshold control:
    //   no matter because RSF enabled
    // Operate on second frame disabled (otherwise 
    // frames might be sent twice since there is only 
    // one TX descriptor)
    value = value & (~( ETH_DMAOMR_DTCEFD 
                      | ETH_DMAOMR_RSF 
                      | ETH_DMAOMR_DFRF  
                      | ETH_DMAOMR_TSF
                      | ETH_DMAOMR_TTC  
                      | ETH_DMAOMR_FEF 
                      | ETH_DMAOMR_FUGF 
                      | ETH_DMAOMR_RTC 
                      | ETH_DMAOMR_OSF ));

    value = value | ETH_DMAOMR_RSF | ETH_DMAOMR_TTC_16Bytes | ETH_DMAOMR_RTC_64Bytes;
    ETH->DMAOMR = value;
}

//--------------------------------------------------------------------------------------------
/**
 * Read a PHY register through MII.
 * @param phyAddress, the physical phy address of the PHY chip to be read
 * @param miiAddress the address of the register to read.
 * @param pMiiData a pointer to a variable where the read value is copied.
 * @return Error status.
 *   @retval TRUE if read successful.
 *   @retval FALSE otherwise (pMiiData is not modified).
 */
BOOL eth_readPhyRegister(uint32_t phyAddress, const uint32_t miiAddress, uint16_t *const pMiiData)
{
    uint32_t nWait = 0U; 
    uint32_t value = 0;
    // Wait for PHY availability
    while ( ((ETH->MACMIIAR & ETH_MACMIIAR_MB) == ETH_MACMIIAR_MB) &&
            (nWait < MII_BUSY_TIMEOUT) )
    {
        nWait++;
    }
    if (nWait == MII_BUSY_TIMEOUT)
    {
        return FALSE;
    }
   
    value = ETH->MACMIIAR & ETH_MACMIIAR_CR;
    ETH->MACMIIAR  = value | (phyAddress << MACMIIAR_PA_POSITION) | (miiAddress << MACMIIAR_MR_POSITION) | ETH_MACMIIAR_MB;
   
    // Wait for completion
    nWait = 0U;
    while ( nWait < MII_BUSY_TIMEOUT ) 
    {
        if ( !(ETH->MACMIIAR & ETH_MACMIIAR_MB) )
        {
            // Data Ready , Read data
            *pMiiData = ETH->MACMIIDR;
            return TRUE;
        }
        nWait++;
    }
    
    return FALSE;

}

//--------------------------------------------------------------------------------------------
/**
 * Write to a PHY register through MII.
 * @param phyAddress, the physical phy address of the PHY chip to be write
 * @param miiAddress the address of the register to write.
 * @param miiData the value to write.
 * @return Error status.
 *   @retval TRUE if write successful.
 *   @retval FALSE otherwise.
 */
BOOL eth_writePhyRegister(uint32_t phyAddress, const uint32_t miiAddress, const uint16_t miiData)
{
    uint32_t nWait = 0U; 
    uint32_t value = 0U;

  
    // Wait for PHY availability
    while ( (ETH->MACMIIAR & ETH_MACMIIAR_MB) &&(nWait < MII_BUSY_TIMEOUT) )
    {
        nWait++;
    }
    if (nWait == MII_BUSY_TIMEOUT)
    {
        return FALSE;
    }
    // Write MII data first 
    ETH->MACMIIDR = miiData;

    value = ETH->MACMIIAR & ETH_MACMIIAR_CR;
    ETH->MACMIIAR  = value | (phyAddress << MACMIIAR_PA_POSITION) | (miiAddress << MACMIIAR_MR_POSITION) | ETH_MACMIIAR_MW | ETH_MACMIIAR_MB;


    while ( nWait < MII_BUSY_TIMEOUT ) 
    {
        if ( !(ETH->MACMIIAR & ETH_MACMIIAR_MB) )
        {
            return TRUE;
        }
        nWait++;
    }
    
    return FALSE;
}

//--------------------------------------------------------------------------------------------
