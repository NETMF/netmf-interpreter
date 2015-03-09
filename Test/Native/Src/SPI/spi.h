////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <CPU_SPI_decl.h>
#include "..\Log\Log.h"

#ifndef _SPI_TEST_
#define _SPI_TEST_ 1

#include "eeprom_stm95x.h"

extern struct ISPI g_EEPROM_STM95x;

//--//

struct ISPI
{
    BOOL                 (*Initialize)     ( GPIO_PIN pin,   UINT32 spiModule );
    void                 (*Read)           ( ); 
    void                 (*Write)          ( );   
    BOOL                 (*Validate)       ( );
};

class SPI 
{
public:
    GPIO_PIN             m_chip_select;
    UINT32               m_spi_module;

                         SPI            ( GPIO_PIN ChipSelect, UINT32 SPI_Module, ISPI& g_spi_device );
                         
    BOOL                 Execute        ( LOG_STREAM Stream );
    BOOL                 (*Initialize)  ( GPIO_PIN pin,   UINT32 spiModule );
    void                 (*Write)       ( );        
    void                 (*Read)        ( );      
    BOOL                 (*Validate)    ( );                             
}; 

// -- //

#endif

