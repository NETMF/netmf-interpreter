////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <CPU_SPI_decl.h>
#include "spi.h"

//--//

EEPROM_STM95x EEPROM_STM95x::s_spi_device;

//--//

struct ISPI g_EEPROM_STM95x = 
{
    EEPROM_STM95x::Initialize,
    EEPROM_STM95x::Read,
    EEPROM_STM95x::Write,
    EEPROM_STM95x::Validate
};

//--//

BOOL EEPROM_STM95x::Initialize( GPIO_PIN ChipSelect, UINT32 SPI_Module )
{
    s_spi_device.m_SPI_Config.Clock_RateKHz  = 2000;    
    s_spi_device.m_SPI_Config.CS_Active      = FALSE;
    s_spi_device.m_SPI_Config.CS_Hold_uSecs  = 0;
    s_spi_device.m_SPI_Config.CS_Setup_uSecs = 0;
    s_spi_device.m_SPI_Config.DeviceCS       = ChipSelect;
    s_spi_device.m_SPI_Config.MD_16bits      = FALSE;
    s_spi_device.m_SPI_Config.MSK_SampleEdge = TRUE;
    s_spi_device.m_SPI_Config.SPI_mod        = SPI_Module; 
    s_spi_device.m_SPI_Config.BusyPin.Pin    = GPIO_PIN_NONE;

    for(UINT32 i=0; i<SUBORDINATE_DATA_LENGTH; i++)
    {
        s_spi_device.toSubordinateData[i]   = (i+1)*16;
        s_spi_device.fromSubordinateData[i] = 0;
    }
    return TRUE;
};

BOOL EEPROM_STM95x::Validate()
{
    for(UINT32 i=0; i<SUBORDINATE_DATA_LENGTH; i++)
    {
        if(s_spi_device.toSubordinateData[i] != s_spi_device.fromSubordinateData[i])
        {
            return FALSE;
        }
    }
    return TRUE;
};

void EEPROM_STM95x::Write( )
{
    UINT8 toBuffer[8];
    UINT8 fromBuffer[8];

    toBuffer[0] = EEPROM_WREN_INST;
    
    CPU_SPI_nWrite8_nRead8( s_spi_device.m_SPI_Config, toBuffer, 1, fromBuffer, 0, 0 );

    // 
    // must have a new transaction to write
    //

    Events_WaitForEvents ( 0, 20 );
    
    //
    // Prepend instruction and address
    //
 
    toBuffer[0] = EEPROM_WRIT_INST;
    toBuffer[1] = 0;
    toBuffer[2] = 0;

    for(int i=0; i<SUBORDINATE_DATA_LENGTH; i++)
    {
        toBuffer[3+i] = s_spi_device.toSubordinateData[i];
    }

    CPU_SPI_nWrite8_nRead8( s_spi_device.m_SPI_Config, toBuffer,SUBORDINATE_DATA_LENGTH+3 ,fromBuffer, 0, 0 );

    Events_WaitForEvents ( 0, 20 );
};

void EEPROM_STM95x::Read( )
{
    UINT8 toBuffer[3];

    toBuffer[0] = EEPROM_READ_INST;
    toBuffer[1] = 0;
    toBuffer[2] = 0;

    CPU_SPI_nWrite8_nRead8( s_spi_device.m_SPI_Config, toBuffer, 3 ,s_spi_device.fromSubordinateData, SUBORDINATE_DATA_LENGTH, 3 );

    Events_WaitForEvents ( 0, 20 );
};

