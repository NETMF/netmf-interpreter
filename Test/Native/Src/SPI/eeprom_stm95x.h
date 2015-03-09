////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <CPU_SPI_decl.h>
#include "..\Log\Log.h"

#ifndef _EEPROM_TEST_
#define _EEPROM_TEST_ 1

//------------------------------------------------------------------------------
// SPI Device Support
//
// EEPROM ST Micro M95040
//

#define EEPROM_WREN_INST   6
#define EEPROM_WRIT_INST   2
#define EEPROM_READ_INST   3
#define EEPROM_RDSR_INST   5

#define SUBORDINATE_DATA_LENGTH    4

struct EEPROM_STM95x
{
    static EEPROM_STM95x s_spi_device;
    UINT8                toSubordinateData  [ SUBORDINATE_DATA_LENGTH  ];    
    UINT8                fromSubordinateData[ SUBORDINATE_DATA_LENGTH  ];   

    SPI_CONFIGURATION    m_SPI_Config; 
    BOOL  static         Initialize     ( GPIO_PIN pin,   UINT32 spiModule ); 
    void  static         Read           ( ); 
    void  static         Write          ( );
    BOOL  static         Validate       ( );
};

#endif

