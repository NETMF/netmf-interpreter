////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPI.h"

//--//

SPI::SPI( GPIO_PIN pin, UINT32 module, ISPI& g_spi_device )
{
    m_chip_select    = pin;
    m_spi_module     = module;
    this->Initialize = g_spi_device.Initialize;
    this->Read       = g_spi_device.Read;
    this->Write      = g_spi_device.Write;
    this->Validate   = g_spi_device.Validate;
}

BOOL SPI::Execute( LOG_STREAM Stream )
{
    Log& log = Log::InitializeLog( Stream, "SPI_EEPROM" );  
    
    if((GPIO_PIN_NONE == m_chip_select) || !Initialize( m_chip_select, m_spi_module ))
    {
        log.CloseLog( FALSE, "init failed" );
    }
    else
    {
        Write();

        Read(); 
    
        if(!Validate())
        {
            log.CloseLog( FALSE, "mis-match " );           
        }
        else
        {
            log.CloseLog( TRUE, NULL );           
        }
    }
    return TRUE;
};
    
    

