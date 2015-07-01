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
#include "STM32F4_ETH_driver.h"

//#define ADVERTISE_10BASET_ONLY 1

//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------
// 
#define INVALID_PHY_ADDRESS             0xFF


static uint32_t g_phyAddress =INVALID_PHY_ADDRESS;
static uint32_t g_foundPhyAddress =FALSE;

static void findPhyAddr()
{
    uint32_t phyAddress = 0;
    uint16_t value;
    uint32_t rc = 0xFF;
    uint32_t cnt = 31;          // 5 bit address
    uint16_t ret;
    do
    {
        ret = eth_readPhyRegister(phyAddress, PHY_IDENTIFIER_REGISTER_1, &value );
        if ((value == PHY_KENDIN_OUI_ID1) || (value == PHY_ST802RT1X_OUI_ID1))
        {
            rc = phyAddress;
            hal_printf( "Valid PHY Found: %x (%x)\r\n", rc, value);
            g_foundPhyAddress = TRUE;
            g_phyAddress = phyAddress;
            
            break;
        }
        phyAddress = (phyAddress + 1) & 0x1F;

    }while(--cnt);

}

static BOOL readRegister( const uint32_t miiAddress, uint16_t *const miiData)
{

    if (g_phyAddress == INVALID_PHY_ADDRESS) 
        findPhyAddr();
    
    return eth_readPhyRegister(g_phyAddress, miiAddress, miiData);
    
}

static BOOL writeRegister( const uint32_t miiAddress, const uint16_t miiData)
{

    if (g_phyAddress == INVALID_PHY_ADDRESS) 
        findPhyAddr();
    
    return eth_writePhyRegister(g_phyAddress, miiAddress, miiData);

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
    uint32_t nWait = 0U;
    uint16_t status ;

    // Perform reset
    if (!writeRegister(PHY_CONTROL_REGISTER, PHY_CR_RESET))
        return FALSE;
    
    do 
    {
        if( !readRegister(PHY_CONTROL_REGISTER, &status) )
            return FALSE;
        
        nWait++;
    } 
    while ( (status & (PHY_CR_RESET | PHY_CR_PWRDN)) && (nWait <PHY_RESET_DELAY));

    return TRUE;

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
    uint32_t nWait = 0U;
    uint16_t status = 0U;
    BOOL    result = FALSE;
    // Read status register until a valid link is detected or a timeout is elapsed
    do 
    {
        if( !readRegister(PHY_STATUS_REGISTER, &status) || 0xFFFF == status )
            return FALSE;

        if (status & PHY_SR_LINK)
        { 
            result = TRUE;
            break;  // when link is up exit
        }
        nWait++;
    }while ( isCallBlocking && (nWait < PHY_LINK_TIMEOUT) ); 
    
    return result;
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
    uint32_t nWait = 0;
    uint16_t status = 0;
    uint16_t phyId = 0;
    
#if STM32F4_ETH_PHY_MII
    uint16_t diagnostic = 0;
#endif
    // accomodate both ST802RTIX and KSZ8081 PHY chip
    // which have mostly identical register sets.
    if( !readRegister(PHY_IDENTIFIER_REGISTER_1, &phyId) )
        return ETHMODE_FAIL;

    if( phyId == PHY_ST802RT1X_OUI_ID1 )
    {
        // For now, hard code to 10MB as auto negotiate to 100MB doesn't seem to work...
        // (packets received OK, but unable to transmit valid packets)
        // most devices don't use 100MB at this point so this is OK for basic testing
        // we are doing - for a full production port this will need to be resolved...
        // NOTE: rev 1.2 boards use a different PHY where 100MB works
        if (!readRegister(PHY_RMII_ANEG_ADVERT_REGISTER, &status))
            return ETHMODE_FAIL;

        status &= ~PHY_RMII_ANEG_ADVERT_SPEED_MASK;
        status |= PHY_RMII_ANEG_ADVERT_10BASE_T | PHY_RMII_ANEG_ADVERT_10BASE_TF;
        if (!writeRegister(PHY_RMII_ANEG_ADVERT_REGISTER, status))
            return ETHMODE_FAIL;
    }

    if (!readRegister(PHY_CONTROL_REGISTER, &status))
        return ETHMODE_FAIL;

#if (DEBUG_TRACE)    
    hal_printf("PHY CR status 0x%x \n",status);
#endif 

    // Start Auto Negotiation    
    status = (status & ~(PHY_CR_ANEGEN | PHY_CR_ANEG_RESTART)) | PHY_CR_ANEGEN | PHY_CR_ANEG_RESTART;
    if (!writeRegister(PHY_CONTROL_REGISTER, status))
        return ETHMODE_FAIL;
    
    // Wait for completion
    do
    {
        if( !readRegister(PHY_STATUS_REGISTER, &status) )
            status = 0;

        nWait++;
    }
    while ( (!(status & PHY_SR_ANEGC) ) && (nWait < PHY_AUTO_NEGOTIATION_TIMEOUT) );
    
    // Check auto negotiation completed
    if ((status & PHY_SR_ANEGC) != PHY_SR_ANEGC)
    {
        hal_printf("autonegotiate failed. Status: %x\n", status);    
        return ETHMODE_FAIL;

    }
    
#if (DEBUG_TRACE)
    hal_printf("Autonegotiate Complete SR:%x\n", status);
#endif

#if STM32F4_ETH_PHY_MII
    // Check common technology found
    readRegister(PHY_DIAGNOSTIC_REGISTER, &diagnostic);
    if ((diagnostic & PHY_ANEGF) == PHY_ANEGF)
        return ETHMODE_FAIL;

    // TODO: find specs on the MII PHY to determine negotiated speed
    return ETHMODE_FAIL;
#else

    // determine the negotiated speed of PHY chip. 
    if( PHY_ST802RT1X_OUI_ID1 == phyId )
    {
        readRegister(PHY_RMII_RXCFG_IIS_REGISTER, &status);
        return (EthMode)(status & PHY_RMII_RXCFG_IIS_MODEMASK);
    }
    else if( PHY_KENDIN_OUI_ID1 == phyId )
    {
        uint16_t speedStatus = 0U;

        readRegister(PHY_CTRL1_REG, &speedStatus);

        // convert speed settings to common form
        speedStatus =  speedStatus & PHY_CTRL1_MASK;
        status = 0;
        if (speedStatus & PHY_KSZ_100TB)
            status |= ETHMODE_100MPS_BIT;
        
        if (speedStatus & PHY_KSZ_FULLDUPLEX) 
            status |= ETHMODE_FULLDPX_BIT;
        
        return (EthMode)status;
    }
#endif

    return ETHMODE_FAIL;
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

    // Read the current value of the PHY control register
    if (!readRegister(PHY_CONTROL_REGISTER, &ctrlReg))
        return FALSE;
    
    // Update the PWRDN bit
    if (isPowerUp)
    {
        // Power up the PHY
        ctrlReg &= ~PHY_CR_PWRDN;
    }
    else
    {
        // Power down the PHY
        ctrlReg |= PHY_CR_PWRDN;
    }
    
    // Write the updated PHY control register
    if (!writeRegister(PHY_CONTROL_REGISTER, ctrlReg))
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
    // Read identifier register until OUI can be read or a timeout is elapsed
    for( uint32_t nWait = 0U; nWait < PHY_RESPONSE_TIMEOUT; ++nWait )   
    {
        uint16_t status = 0U;

        if( !readRegister( PHY_IDENTIFIER_REGISTER_1, &status ) )
            return FALSE;
        
        if( ( status == PHY_ST802RT1X_OUI ) || ( status == PHY_KENDIN_OUI ) )
            return TRUE;
    }

    return FALSE;
}

//--------------------------------------------------------------------------------------------
