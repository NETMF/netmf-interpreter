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
 * @file    STM32F4_ETH_phy.cpp
 * @brief   PHY (TERIDIAN 78Q2123) driver. 
 * @author  CSA Engineering AG, Switzerland, www.csa.ch, info@csa.ch
 * @date    March 2012
 ********************************************************************************************/
  
//--------------------------------------------------------------------------------------------
// System and local includes
//--------------------------------------------------------------------------------------------
 
#include <tinyhal.h>

#include "STM32F4_ETH_phy.h"

#define ADVERTISE_10BASET_ONLY 1

//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------

#define PHY_CONTROL_REGISTER            0U              // Control register
#define PHY_RESET                       0x8000U         // Reset
#define PHY_ANEGEN                      0x1000U         // Auto-negotiation enable 
#define PHY_ANEG_RESTART                0x0200U 
#define PHY_PWRDN                       0x0800U         // Power-down
#define PHY_STATUS_REGISTER             1U              // Status register
#define PHY_LINK                        0x0004U         // Link status
#define PHY_ANEGC                       0x0020U         // Auto-negotiation complete
#define PHY_RMII_ANEG_ADVERT_REGISTER   04U             // Auto negotiation advertisement register
#define PHY_RMII_XCNTL_REGISTER         16U
#define PHY_RMII_RXCFG_IIS_REGISTER     17U             // Receive Configuration and Interrupt Status Register
#define PHY_DIAGNOSTIC_REGISTER         18U             // Diagnostic register
#define PHY_ANEGF                       0x1000U         // Auto-negotiation fail indication
#define PHY_LINK_TIMEOUT                0x0003FFFFU     // PHY link timeout
#define PHY_RESET_DELAY                 0x000FFFFFU     // PHY reset delay
#define PHY_AUTO_NEGOTIATION_TIMEOUT    0x00100000U     // Auto negotiation timeout
#define PHY_RMII_RXCFG_IIS_MODEMASK     (3<<8)          // Speed bit (bit 9 = speed; bit 8 == Duplex )
#define PHY_IDENTIFIER_REGISTER_1       2U              // Identifier register 1

#define PHY_RMII_ANEG_ADVERT_100BASE_T4  (1<<9)
#define PHY_RMII_ANEG_ADVERT_100BASE_TXF (1<<8)
#define PHY_RMII_ANEG_ADVERT_100BASE_TX  (1<<7)
#define PHY_RMII_ANEG_ADVERT_10BASE_TF   (1<<6)
#define PHY_RMII_ANEG_ADVERT_10BASE_T    (1<<5)

#define PHY_RMII_ANEG_ADVERT_SPEED_MASK ( PHY_RMII_ANEG_ADVERT_100BASE_T4 | PHY_RMII_ANEG_ADVERT_100BASE_TXF | PHY_RMII_ANEG_ADVERT_100BASE_TX | PHY_RMII_ANEG_ADVERT_10BASE_TF | PHY_RMII_ANEG_ADVERT_10BASE_T)

#define PHY_RESPONSE_TIMEOUT            0x0003FFFFU     // PHY response timeout
#define PHY_TERIDIAN_OUI                0x000EU       // Teridian unique identifier
#define PHY_ST802RT1X_OUI               0x80E1U // ST802RT1x PHY unique identifier
#if STM32F4_ETH_PHY_RMII
#define PHY_OUI PHY_ST802RT1X_OUI
#else
#define PHY_TERIRIAN_OUI
#endif

//--------------------------------------------------------------------------------------------
// Local declarations
//--------------------------------------------------------------------------------------------

static BOOL (*readPhyRegister)(const uint32_t miiAddress, uint16_t *const miiData);
static BOOL (*writePhyRegister)(const uint32_t miiAddress, const uint16_t miiData);

//--------------------------------------------------------------------------------------------
// Functions definitions
//--------------------------------------------------------------------------------------------
/**
 * Save the function to read the PHY.
 * @param pRead pointer to the read PHY callback function.
 */
void initReadPhyCallback(pRead readCallback)
{
    readPhyRegister = readCallback;
}

//--------------------------------------------------------------------------------------------
/**
 * Save the function to write to the PHY.
 * @param pWrite pointer to the write PHY callback function.
 */
void initWritePhyCallback(pWrite writeCallback)
{
    writePhyRegister = writeCallback;
}

//--------------------------------------------------------------------------------------------
/**
 * Reset the PHY.
 * @return Error status.
 *   @retval TRUE if PHY reset successful.
 *   @retval FALSE otherwise.
 */
BOOL eth_phyReset()
{
    volatile uint32_t nWait = 0U;
    
    // Check callback
    if (!writePhyRegister)
    {
        return FALSE;
    }
    
    // Perform reset
    if (!writePhyRegister(PHY_CONTROL_REGISTER, PHY_RESET))
    {
        return FALSE;
    }
    
    // Wait for completion
    while (nWait < PHY_RESET_DELAY)
    {
        nWait++;
    }

#if STM32F4_ETH_PHY_RMII
    // enable RMII
    return writePhyRegister(PHY_RMII_XCNTL_REGISTER, 0x1000);
#else
    return TRUE;
#endif
    
}

//--------------------------------------------------------------------------------------------
/**
 * Test whether a valid PHY link is established.
 * @param isCallBlocking indicates whether the function should block (until a link is detected 
 *        or a timeout is elapsed)
 * @return The PHY link status.
 *   @retval TRUE if valid link detected.
 *   @retval FALSE otherwise.
 */
BOOL eth_isPhyLinkValid(BOOL isCallBlocking)
{
    volatile uint32_t nWait = 0U;
    uint16_t status = 0U;

    // Check callback
    if (!readPhyRegister)
    {
        return FALSE;
    }
    
    // Read status register until a valid link is detected or a timeout is elapsed
    do 
    {
        readPhyRegister(PHY_STATUS_REGISTER, &status);
        nWait++;
    } 
    while ( isCallBlocking &&
            ((status & PHY_LINK) != PHY_LINK) &&
            (nWait++ < PHY_LINK_TIMEOUT) );
    
    // Check the link
    return (status & PHY_LINK) == PHY_LINK;
}

 
//--------------------------------------------------------------------------------------------
/**
 * Enable auto negotiation and wait for completion.
 * @return Error status.
 *   @retval TRUE if auto negotiation successful.
 *   @retval FALSE otherwise.
 */
EthMode eth_enableAutoNegotiation()
{
    volatile uint32_t nWait = 0U;
    uint16_t status = 0U;

#if STM32F4_ETH_PHY_MII
    uint16_t diagnostic = 0U;
#endif

    // Check callback
    if (!writePhyRegister || !readPhyRegister)
    {
        return ETHMODE_FAIL;
    }

    eth_isPhyResponding();

#if ADVERTISE_10BASET_ONLY
    // For now, hard code to 10MB as auto negotiate to 100MB doesn't seem to work... (packets received OK, but unable to transmit valid packets)
    // most devices don't use 100MB at this point so this is OK for basic testing we are doing - for a full 
    // production port this will need to be resolved...
    if (!readPhyRegister(PHY_RMII_ANEG_ADVERT_REGISTER, &status))
        return ETHMODE_FAIL;
    
    status &= ~PHY_RMII_ANEG_ADVERT_SPEED_MASK;
    status |= PHY_RMII_ANEG_ADVERT_10BASE_T | PHY_RMII_ANEG_ADVERT_10BASE_TF;
    if (!writePhyRegister(PHY_RMII_ANEG_ADVERT_REGISTER, status))
        return ETHMODE_FAIL;
#endif

    if (!readPhyRegister(PHY_CONTROL_REGISTER, &status))
        return ETHMODE_FAIL;

    status = (status & ~(PHY_ANEGEN | PHY_ANEG_RESTART)) | PHY_ANEGEN | PHY_ANEG_RESTART;
    if (!writePhyRegister(PHY_CONTROL_REGISTER, status))
        return ETHMODE_FAIL;
    
    // Wait for completion
    do
    {
        readPhyRegister(PHY_STATUS_REGISTER, &status);
        nWait++;
    }
    while ( ((status & PHY_ANEGC) != PHY_ANEGC) &&
            (nWait++ < PHY_AUTO_NEGOTIATION_TIMEOUT) );
    
    // Check auto negotiation completed
    if ((status & PHY_ANEGC) != PHY_ANEGC)
        return ETHMODE_FAIL;

#if STM32F4_ETH_PHY_MII
    // Check common technology found
    readPhyRegister(PHY_DIAGNOSTIC_REGISTER, &diagnostic);
    if ((diagnostic & PHY_ANEGF) == PHY_ANEGF)
        return ETHMODE_FAIL;

    // TODO: find specs on the MII PHY to determine negotiated speed
    return ETHMODE_FAIL;
#else
    readPhyRegister(PHY_RMII_RXCFG_IIS_REGISTER, &status);
    return (EthMode)(status & PHY_RMII_RXCFG_IIS_MODEMASK);
#endif
    
}

//--------------------------------------------------------------------------------------------
/**
 * Power up the PHY.
 * @param isPowerUp the power state.
 *  @arg TRUE if the PHY must be powered up.
 *  @arg FALSE if the PHY must be powered down.
 * @return Error status.
 *   @retval TRUE if PHY powered up succesfully.
 *   @retval FALSE otherwise.
 */
BOOL eth_powerUpPhy(BOOL isPowerUp)
{
    uint16_t ctrlReg;

    // Check callback
    if (!writePhyRegister || !readPhyRegister)
    {
        return FALSE;
    }
    
    // Read the current value of the PHY control register
    if (!readPhyRegister(PHY_CONTROL_REGISTER, &ctrlReg))
    {
        return FALSE;
    }
    
    // Update the PWRDN bit
    if (isPowerUp)
    {
        // Power up the PHY
        ctrlReg &= ~PHY_PWRDN;
    }
    else
    {
        // Power down the PHY
        ctrlReg |= PHY_PWRDN;
    }
    
    // Write the updated PHY control register
    if (!writePhyRegister(PHY_CONTROL_REGISTER, ctrlReg))
    {
        return FALSE;
    }
    
    return TRUE;
}

//--------------------------------------------------------------------------------------------
/**
 * Test whether the PHY is responding.
 * @return The PHY status.
 *   @retval TRUE if the PHY responded.
 *   @retval FALSE otherwise.
 */
BOOL eth_isPhyResponding(void)
{
    volatile uint32_t nWait = 0U;
    uint16_t status = 0U;

    // Check callback
    if (!readPhyRegister)
    {
        return FALSE;
    }
    
    // Read identifier register until OUI can be read or a timeout is elapsed
    do 
    {
        readPhyRegister(PHY_IDENTIFIER_REGISTER_1, &status);
        nWait++;
    } 
    while ( (status == PHY_OUI) &&
            (nWait++ < PHY_RESPONSE_TIMEOUT) );
    
    // Check the link
    return status == PHY_OUI;
}

//--------------------------------------------------------------------------------------------
