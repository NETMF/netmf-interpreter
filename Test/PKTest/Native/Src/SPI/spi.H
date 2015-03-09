////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\Log\Log.h"

#ifndef _SPI_NMT_
#define _SPI_NMT_ 1

//-----------------------------------------------------------------------------
//
// Support EEPROM ST Micro M95040
//
// define these in application
//
//
//
#define EEPROM_WREN_INST   6
#define EEPROM_WRIT_INST   2
#define EEPROM_READ_INST   3
#define EEPROM_RDSR_INST   5
//
//-----------------------------------------------------------------------------
//
//  Client Device Support
//  must assign chip select and module
//
#define EEPROM_CS   MC9328MXL_GPIO::c_Port_B_18
#define SPI_MODULE  MC9328MXL_SPI::c_SPI1

typedef
struct _SPI  : Log
{
    UINT32     PortsCount;
    GPIO_PIN   SClock;
    GPIO_PIN   MISO;
    GPIO_PIN   MOSI;
    UINT32     ClockRate;
     
    SPI_CONFIGURATION SPI_Config;

    BOOL SPITest_Init(UINT32,UINT32); 
    BOOL SPITest_Read(); 
    BOOL SPITest_Write(); 

    BOOL SPITest(NMT_STREAM Stream)
    {
        BOOL result=false;

	Log::Initialize(Stream);
	Log::BeginTest("SPI");

        result = SPITest_Init(EEPROM_CS,SPI_MODULE);

        if (result)
        {
            SPITest_Write();
            result = SPITest_Read();
        }
	Log::EndTest(result);
        return result;
    };

    void Set_SPI_Configuration(UINT32 ClockRate,
		               UINT32 CS_Active,
			       UINT32 CS_Hold,
			       UINT32 CS_Setup,
			       GPIO_PIN pin,
			       BOOL     MD_16,
			       BOOL     MSK_Idle,
			       BOOL     MSK_Sample,
			       UINT32   SPI_mod)
    {
        SPI_Config.Clock_RateKHz      = 100;
        SPI_Config.CS_Active          = 0;
        SPI_Config.CS_Hold_uSecs      = CS_Hold;
        SPI_Config.CS_Setup_uSecs     = CS_Setup;
        SPI_Config.DeviceCS           = pin;
        SPI_Config.MD_16bits          = MD_16; 
        SPI_Config.MSK_SampleEdge     = MSK_Sample;
        SPI_Config.SPI_mod            = SPI_mod;
    }

} SPI; 



#endif


