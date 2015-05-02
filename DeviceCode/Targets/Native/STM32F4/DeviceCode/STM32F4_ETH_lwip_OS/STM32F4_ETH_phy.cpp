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

#define ADVERTISE_10BASET_ONLY 1

//--------------------------------------------------------------------------------------------
// Constant defines
//--------------------------------------------------------------------------------------------
// 
#define INVALID_PHY_ADDRESS             0xFF


#define DEBUG_TRACE                     1


static uint32_t g_phyAddress =INVALID_PHY_ADDRESS;
static uint32_t g_foundPhyAddress =FALSE;

static void findPhyAddr()
{
    uint32_t phyAddress = 0;
    uint32_t retryMax = 1000;    // At least 1, should not be 0
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
            debug_printf( "Valid PHY Found: %x (%x)\r\n", rc, value);
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
        readRegister(PHY_CONTROL_REGISTER, &status);
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
        readRegister(PHY_STATUS_REGISTER, &status);
        readRegister(PHY_STATUS_REGISTER, &status);

        if (status & PHY_SR_LINK)
        { 
            result = TRUE;
            break;  // when link is up exit
        }
        nWait++;
    } 
    while ( isCallBlocking && (nWait < PHY_LINK_TIMEOUT) );
    
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
     uint32_t nWait = 0U;
     uint16_t status = 0U;

#if STM32F4_ETH_PHY_MII
    uint16_t diagnostic = 0U;
#endif

#if ADVERTISE_10BASET_ONLY
    // For now, hard code to 10MB as auto negotiate to 100MB doesn't seem to work... (packets received OK, but unable to transmit valid packets)
    // most devices don't use 100MB at this point so this is OK for basic testing we are doing - for a full 
    // production port this will need to be resolved...
    if (!readRegister(PHY_RMII_ANEG_ADVERT_REGISTER, &status))
        return ETHMODE_FAIL;
    
    status &= ~PHY_RMII_ANEG_ADVERT_SPEED_MASK;
    status |= PHY_RMII_ANEG_ADVERT_10BASE_T | PHY_RMII_ANEG_ADVERT_10BASE_TF ;
    if (!writeRegister(PHY_RMII_ANEG_ADVERT_REGISTER, status))
        return ETHMODE_FAIL;
#endif

    if (!readRegister(PHY_CONTROL_REGISTER, &status))
        return ETHMODE_FAIL;
#if (DEBUG_TRACE)    
    debug_printf("PHY CR status 0x%x \r\n",status);
#endif 

    // Start Auto Negotiation    
    status = (status & ~(PHY_CR_ANEGEN | PHY_CR_ANEG_RESTART)) | PHY_CR_ANEGEN | PHY_CR_ANEG_RESTART;

    if (!writeRegister(PHY_CONTROL_REGISTER, status))
        return ETHMODE_FAIL;


    // Wait for completion
    do
    {
        uint16_t val;
        val = readRegister(PHY_STATUS_REGISTER, &status);
        val = readRegister(PHY_STATUS_REGISTER, &status);
        nWait ++;
    }
    while ( (!(status & PHY_SR_ANEGC) ) && (nWait < PHY_AUTO_NEGOTIATION_TIMEOUT) );
    
    // Check auto negotiation completed
    if ((status & PHY_SR_ANEGC) != PHY_SR_ANEGC)
    {
        debug_printf("autogen FAIL!!! Status %x\r\n", status);    
        return ETHMODE_FAIL;

     }
#if (DEBUG_TRACE)
    debug_printf("Autogen Complete!!! SR %x\r\n", status);
#endif

#if STM32F4_ETH_PHY_MII
    // Check common technology found
    readRegister(PHY_DIAGNOSTIC_REGISTER, &diagnostic);
    if ((diagnostic & PHY_ANEGF) == PHY_ANEGF)
        return ETHMODE_FAIL;

    // TODO: find specs on the MII PHY to determine negotiated speed
    return ETHMODE_FAIL;
#else

    readRegister(PHY_IDENTIFIER_REGISTER_1, &status);

    // the following is bit hacking to accomodating both ST802RTIX and KSZ8081 PHY chip
    // determine the negotiated speed of PHY chip. 
    if (status == PHY_ST802RT1X_OUI_ID1)
    {
        readRegister(PHY_RMII_RXCFG_IIS_REGISTER, &status);
        // the bit definition of EthMode is same as the ST8021,just shift it the LSB is fine.
        return (EthMode)((status & PHY_RMII_RXCFG_IIS_MODEMASK)>>8);
    }
    else if (status == PHY_KENDIN_OUI_ID1)
    {
        uint16_t speedStatus = 0U;

        readRegister(PHY_CTRL1_REG, &speedStatus);

        // conversion of the speed conversion
        speedStatus =  speedStatus & PHY_CTRL1_MASK;
        status = 0;
#define         PHY_KSZ_100TB               0x2
#define         PHY_KSZ_FULLDUPLEX          0x4
        if (speedStatus & PHY_KSZ_100TB)
                status |= ETHMODE_100MPS_BIT;
        if (speedStatus & PHY_KSZ_FULLDUPLEX) 
                status |= ETHMODE_FULLDPX_BIT;
        return (EthMode)(status );
    }
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
    volatile uint32_t nWait = 0U;
    uint16_t status = 0U;
    BOOL     result = FALSE;

    // Read identifier register until OUI can be read or a timeout is elapsed
    do 
    {
        readRegister(PHY_IDENTIFIER_REGISTER_1, &status);
        if ((status == PHY_ST802RT1X_OUI_ID1) || (status == PHY_KENDIN_OUI_ID1) )
        {
            result = TRUE;
            break;
        }
        nWait++;
    } 
    while ( nWait < PHY_RESPONSE_TIMEOUT);
    
    // Check the link
    return result;
}

//--------------------------------------------------------------------------------------------
