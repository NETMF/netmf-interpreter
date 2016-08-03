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

//--------------------------------------------------------------------------------------------
// Local declarations
//--------------------------------------------------------------------------------------------

static void (*receiveIntHandler)();

//--------------------------------------------------------------------------------------------
// Local functions declarations
//--------------------------------------------------------------------------------------------

static void enableDmaInterrupts();
static void disableDmaInterrupts();
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
    initMACMIIAR();
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
    // Select PHY interface
    RCC->AHB1RSTR |= RCC_AHB1RSTR_ETHMACRST;
    RCC->APB2ENR |= RCC_APB2ENR_SYSCFGEN;
#ifdef STM32F4_ETH_PHY_MII
    SYSCFG->PMC &= ~SYSCFG_PMC_MII_RMII;
#else
    SYSCFG->PMC |= SYSCFG_PMC_MII_RMII;
#endif
    RCC->AHB1RSTR &= ~RCC_AHB1RSTR_ETHMACRST;
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
BOOL eth_macReset()
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
    enableDmaInterrupts();
    
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
    disableDmaInterrupts();
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
    hal_printf("DMA Int:");
    #endif

    // Normal interrupt summary
    if ((ETH->DMASR & ETH_DMASR_NIS) == ETH_DMASR_NIS)
    {
        #if INT_ENABLE_RI
        if ((ETH->DMASR & ETH_DMASR_RS) == ETH_DMASR_RS)
        {
            // Ethernet frame received
            #if defined (_DEBUG) && DEBUG_DMA_INT
            hal_printf(" RS");
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
            hal_printf(" TS");
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
            hal_printf(" TBUS");
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
            hal_printf(" ERS");
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
            hal_printf(" FBES");
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
            hal_printf(" TPSS");
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
            hal_printf(" TJTS");
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
            hal_printf(" ROS");
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
            hal_printf(" TUS");
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
            hal_printf(" RBUS");
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
            hal_printf(" RPSS");
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
            hal_printf(" RWTS");
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
            hal_printf(" ETS");
            #endif

            // Clear interrupt flag
            ETH->DMASR = ETH_DMASR_ETS;
        }
        #endif

        // Clear abnormal interrupt flag
        ETH->DMASR = ETH_DMASR_AIS;
    }

    #if defined (_DEBUG) && DEBUG_DMA_INT
    hal_printf("\r\n");
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
 * Initialize the receive interrupt handler.
 */
void eth_initReceiveIntHandler(pIntHandler receiveHandler)
{
    receiveIntHandler = receiveHandler;
}

//--------------------------------------------------------------------------------------------
/**
 * Enable DMA interrupts.
 */
void enableDmaInterrupts()
{
    // Enable DMA interrupts
    #if INT_ENABLE_NIS
    ETH->DMAIER |= ETH_DMAIER_NISE;         // Normal interrupt summary
    #endif
    #if INT_ENABLE_AIS
    ETH->DMAIER |= ETH_DMAIER_AISE;         // Abnormal interrupt summary
    #endif
    #if INT_ENABLE_ERI
    ETH->DMAIER |= ETH_DMAIER_ERIE;         // Early receive
    #endif
    #if INT_ENABLE_FBEI
    ETH->DMAIER |= ETH_DMAIER_FBEIE;        // Fatal bus error
    #endif
    #if INT_ENABLE_ETI
    ETH->DMAIER |= ETH_DMAIER_ETIE;         // Early transmit interrupt
    #endif
    #if INT_ENABLE_RWTI
    ETH->DMAIER |= ETH_DMAIER_RWTIE;        // Receive watchdog timeout
    #endif
    #if INT_ENABLE_RPSI
    ETH->DMAIER |= ETH_DMAIER_RPSIE;        // Receive process stopped
    #endif
    #if INT_ENABLE_RBUI
    ETH->DMAIER |= ETH_DMAIER_RBUIE;        // Receive buffer unavailable
    #endif
    #if INT_ENABLE_RI
    ETH->DMAIER |= ETH_DMAIER_RIE;          // Received
    #endif
    #if INT_ENABLE_TUI
    ETH->DMAIER |= ETH_DMAIER_TUIE;         // Transmit underflow
    #endif
    #if INT_ENABLE_ROI
    ETH->DMAIER |= ETH_DMAIER_ROIE;         // Receive overflow
    #endif
    #if INT_ENABLE_TJTI
    ETH->DMAIER |= ETH_DMAIER_TJTIE;        // Transmit jabber timeout
    #endif
    #if INT_ENABLE_TBUI
    ETH->DMAIER |= ETH_DMAIER_TBUIE;        // Transmit buffer unavailable
    #endif
    #if INT_ENABLE_TPSI
    ETH->DMAIER |= ETH_DMAIER_TPSIE;        // Transmit process stopped
    #endif
    #if INT_ENABLE_TI
    ETH->DMAIER |= ETH_DMAIER_TIE;          // Transmit
    #endif
}

//--------------------------------------------------------------------------------------------
/**
 * Disable DMA interrupts.
 */
void disableDmaInterrupts()
{
    // Disable DMA interrupts
    #if INT_ENABLE_NIS
    ETH->DMAIER &= ~ETH_DMAIER_NISE;        // Normal interrupt summary
    #endif
    #if INT_ENABLE_AIS
    ETH->DMAIER &= ~ETH_DMAIER_AISE;        // Abnormal interrupt summary
    #endif
    #if INT_ENABLE_ERI
    ETH->DMAIER &= ~ETH_DMAIER_ERIE;        // Early receive
    #endif
    #if INT_ENABLE_FBEI
    ETH->DMAIER &= ~ETH_DMAIER_FBEIE;       // Fatal bus error
    #endif
    #if INT_ENABLE_ETI
    ETH->DMAIER &= ~ETH_DMAIER_ETIE;        // Early transmit interrupt
    #endif
    #if INT_ENABLE_RWTI
    ETH->DMAIER &= ~ETH_DMAIER_RWTIE;       // Receive watchdog timeout
    #endif
    #if INT_ENABLE_RPSI
    ETH->DMAIER &= ~ETH_DMAIER_RPSIE;       // Receive process stopped
    #endif
    #if INT_ENABLE_RBUI
    ETH->DMAIER &= ~ETH_DMAIER_RBUIE;       // Receive buffer unavailable
    #endif
    #if INT_ENABLE_RI
    ETH->DMAIER &= ~ETH_DMAIER_RIE;         // Received
    #endif
    #if INT_ENABLE_TUI
    ETH->DMAIER &= ~ETH_DMAIER_TUIE;        // Transmit underflow
    #endif
    #if INT_ENABLE_ROI
    ETH->DMAIER &= ~ETH_DMAIER_ROIE;        // Receive overflow
    #endif
    #if INT_ENABLE_TJTI
    ETH->DMAIER &= ~ETH_DMAIER_TJTIE;       // Transmit jabber timeout
    #endif
    #if INT_ENABLE_TBUI
    ETH->DMAIER &= ~ETH_DMAIER_TBUIE;       // Transmit buffer unavailable
    #endif
    #if INT_ENABLE_TPSI
    ETH->DMAIER &= ~ETH_DMAIER_TPSIE;       // Transmit process stopped
    #endif
    #if INT_ENABLE_TI
    ETH->DMAIER &= ~ETH_DMAIER_TIE;         // Transmit
    #endif
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
    ETH->MACCR &= ~ETH_MACCR_WD;                // Watchdog enabled
    ETH->MACCR &= ~ETH_MACCR_JD;                // Jabber enabled
    ETH->MACCR &= ~ETH_MACCR_IFG;               // Interframe gap:
    ETH->MACCR |=  ETH_MACCR_IFG_96Bit;         //   96 bit times
    ETH->MACCR &= ~ETH_MACCR_CSD;               // Carrier sense enabled
    
    ETH->MACCR |=  ETH_MACCR_FES;               // 100 Mbit/s fast ethernet mode
    ETH->MACCR |=  ETH_MACCR_DM;                // Full-duplex mode

    ETH->MACCR &= ~ETH_MACCR_ROD;               // Receive own enabled
    ETH->MACCR &= ~ETH_MACCR_LM;                // Loopback mode disabled
    ETH->MACCR &= ~ETH_MACCR_IPCO;              // Checksum offload disabled
    ETH->MACCR |=  ETH_MACCR_RD;                // Retry disabled
    ETH->MACCR &= ~ETH_MACCR_APCS;              // Automatic pad/CRC stripping disabled
    ETH->MACCR &= ~ETH_MACCR_BL;                // Back-off limit:
    ETH->MACCR |=  ETH_MACCR_BL_10;             //   min(n, 10)
    ETH->MACCR &= ~ETH_MACCR_DC;                // Defferal check disabled
    ETH->MACCR &= ~ETH_MACCR_TE;                // Transmitter disabled
    ETH->MACCR &= ~ETH_MACCR_RE;                // Received disabled
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACFFR, the ethernet MAC frame filter register.
 */
static void initMACFFR()
{
    ETH->MACFFR &= ~ETH_MACFFR_RA;              // Receive all disabled
    ETH->MACFFR &= ~ETH_MACFFR_HPF;             // Hash or perfect filter disabled
    ETH->MACFFR &= ~ETH_MACFFR_SAF;             // Source address filter disabled
    ETH->MACFFR &= ~ETH_MACFFR_SAIF;            // Source address inverse filtering disabled
    ETH->MACFFR &= ~ETH_MACFFR_PCF;             // Pass control frames:
    ETH->MACFFR |=  ETH_MACFFR_PCF_BlockAll;    //   prevents all control frames
    ETH->MACFFR &= ~ETH_MACFFR_BFD;             // Broadcast frames enabled
    ETH->MACFFR &= ~ETH_MACFFR_PAM;             // Pass all multicast disabled
    ETH->MACFFR &= ~ETH_MACFFR_DAIF;            // Destination address inverse filtering disabled
    ETH->MACFFR &= ~ETH_MACFFR_HM;              // Hash multicast disabled
    ETH->MACFFR &= ~ETH_MACFFR_HU;              // Hash unicast disabled
    ETH->MACFFR &= ~ETH_MACFFR_PM;              // Promiscuous mode disabled
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACMIIAR, the ethernet MAC MII adress register.
 */
static void initMACMIIAR()
{ 
    // Clear the clock range
    ETH->MACMIIAR &= ~ETH_MACMIIAR_CR;
    
    // Select the clock range HCLK/62
    ETH->MACMIIAR |= ETH_MACMIIAR_CR_Div62;
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACFCR, the ethernet MAC flow control register.
 */
static void initMACFCR()
{
    ETH->MACFCR &= ~ETH_MACFCR_PT;              // Pause time: 0
    ETH->MACFCR |=  ETH_MACFCR_ZQPD;            // Zero-quanta pause disabled
    ETH->MACFCR &= ~ETH_MACFCR_PLT;             // Pause low threshold:
    ETH->MACFCR |=  ETH_MACFCR_PLT_Minus4;      //   pause time minus 4 slot times
    ETH->MACFCR &= ~ETH_MACFCR_UPFD;            // Unicast pause frame detect disabled
    ETH->MACFCR &= ~ETH_MACFCR_RFCE;            // Receive flow control disabled
    ETH->MACFCR &= ~ETH_MACFCR_TFCE;            // Transmit flow control disabled
    ETH->MACFCR &= ~ETH_MACFCR_FCBBPA;          // Flow control busy/back pressure activate disabled
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_MACIMR, the ethernet MAC interrupt mask register.
 */
static void initMACIMR()
{
    ETH->MACIMR |= ETH_MACIMR_TSTIM;            // Time stamp trigger interrupt disabled
    ETH->MACIMR |= ETH_MACIMR_PMTIM;            // PMT interrupt disabled
}
 
//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_DMABMR, the ethernet DMA bus mode register.
 */
static void initDMABMR()
{
    ETH->DMABMR |=  ETH_DMABMR_AAB;             // Address-aligned beat enabled
    ETH->DMABMR &= ~ETH_DMABMR_FPM;             // 4xPBL mode disabled
    ETH->DMABMR |=  ETH_DMABMR_USP;             // Use separeate PBL enabled
    ETH->DMABMR &= ~ETH_DMABMR_RDP;             // Rx DMA PBL:
    ETH->DMABMR |=  ETH_DMABMR_RDP_32Beat;      //   32
    ETH->DMABMR |=  ETH_DMABMR_FB;              // Fixed burst enabled
    ETH->DMABMR &= ~ETH_DMABMR_RTPR;            // Rx Tx priority ratio:
    ETH->DMABMR |=  ETH_DMABMR_RTPR_1_1;        //   1:1
    ETH->DMABMR &= ~ETH_DMABMR_PBL;             // Programmable burst length:
    ETH->DMABMR |=  ETH_DMABMR_PBL_32Beat;      //   32
    ETH->DMABMR &= ~ETH_DMABMR_EDE;             // Enhanced descriptor format disabled
    ETH->DMABMR &= ~ETH_DMABMR_DSL;             // Descriptor skip length: contiguous
    ETH->DMABMR &= ~ETH_DMABMR_DA;              // DMA Arbitration: round robin
}

//--------------------------------------------------------------------------------------------
/**
 * Initialize ETH_DMAOMR, the ethernet DMA operation mode register.
 */
static void initDMAOMR()
{
    ETH->DMAOMR &= ~ETH_DMAOMR_DTCEFD;          // Dropping of TCP/IP checksum error frames enabled
    ETH->DMAOMR |=  ETH_DMAOMR_RSF;             // Receive store and forwared enabled
    ETH->DMAOMR &= ~ETH_DMAOMR_DFRF;            // Flushing of received frames enabled
    ETH->DMAOMR &= ~ETH_DMAOMR_TSF;             // Transmit store and forwared disabled       
    ETH->DMAOMR &= ~ETH_DMAOMR_TTC;             // Transmit threshold control:
    ETH->DMAOMR |=  ETH_DMAOMR_TTC_16Bytes;     //   16
    ETH->DMAOMR &= ~ETH_DMAOMR_FEF;             // Forward error frames disabled
    ETH->DMAOMR &= ~ETH_DMAOMR_FUGF;            // Forward undersized good frames disabled
    ETH->DMAOMR &= ~ETH_DMAOMR_RTC;             // Receive threshold control:
    ETH->DMAOMR |=  ETH_DMAOMR_RTC_64Bytes;     //   no matter because RSF enabled
    ETH->DMAOMR &= ~ETH_DMAOMR_OSF;             // Operate on second frame disabled (otherwise 
                                                // frames might be sent twice since there is only 
                                                // one TX descriptor)
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
   
    value = ETH->MACMIIAR & ETH_MACMIIAR_CR; // Clear all bits except the clock rate
    ETH->MACMIIAR  = value
                   | (phyAddress << MACMIIAR_PA_POSITION)
                   | (miiAddress << MACMIIAR_MR_POSITION)
                   | ETH_MACMIIAR_MB;
   
    // Wait for completion
    for( nWait = 0; nWait < MII_BUSY_TIMEOUT; ++nWait )
    {
        if( !(ETH->MACMIIAR & ETH_MACMIIAR_MB) )
        {
            // Data Ready , Read data
            *pMiiData = ETH->MACMIIDR;
            return TRUE;
        }
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

