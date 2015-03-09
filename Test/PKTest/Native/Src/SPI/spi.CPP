////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPI.h"

//------------------------------ GLOBAL     ------------------------------------




UINT8   InBuffer8[8];
UINT8   OutBuffer8[8];
UINT16  InBuffer16[8];
UINT16  OutBuffer16[8];

#define BUFFER_SIZE 128
char Buffer[BUFFER_SIZE];
//

//------------------------------ TEST CASES ------------------------------------
//
//
// Note: Detailed log out is muted if not initialized
//

BOOL SPI::SPITest_Init(UINT32 spi_module,GPIO_PIN ChipSelect)
{

    SPI::Set_SPI_Configuration(100,0,0,0,(GPIO_PIN)ChipSelect, false, false, true, 0);
    
    if (!CPU_SPI_Initialize())
    {
        Comment("Unable to initialize SPI");
        return false;
    }

    SPI::PortsCount = CPU_SPI_PortsCount();
    CPU_SPI_GetPins(spi_module, SPI::SClock, SPI::MISO, SPI::MOSI);

    /*
    
    hal_snprintf(Buffer,BUFFER_SIZE,
                "Ports:%d\tClk:%d\tMISO:%d\tMOSI:%d\r\n", 
                SPI::PortsCount,
                SPI::SClock,
                SPI::MISO,
                SPI::MOSI);

    Comment(Buffer);
    */
    return true;
}


BOOL SPI::SPITest_Read()
{

    OutBuffer8[0] = EEPROM_READ_INST;
    OutBuffer8[1] = 32;
    OutBuffer8[2] = 0;
    InBuffer8[0]  = 0;
    InBuffer8[1]  = 0;
    InBuffer8[2]  = 0;
    InBuffer8[3]  = 0;
    InBuffer8[4]  = 0;
    InBuffer8[5]  = 0;
    InBuffer8[6]  = 0;
    InBuffer8[7]  = 0;

    CPU_SPI_nWrite8_nRead8(SPI::SPI_Config,OutBuffer8,1,InBuffer8,3,3);

    /*
    hal_snprintf(Buffer,BUFFER_SIZE,
                 "SPI-In:%d\t%d\t%d",
                 InBuffer8[0],
                 InBuffer8[1],
                 InBuffer8[2]);

    Comment(Buffer);
    */
    
    return true;
}

BOOL SPI::SPITest_Write()
{
    OutBuffer8[0] = EEPROM_WREN_INST;

    CPU_SPI_nWrite8_nRead8(SPI::SPI_Config,OutBuffer8,1,InBuffer8,0,0);

    Events_WaitForEvents (0, 100);

    OutBuffer8[0] = EEPROM_WRIT_INST;
    OutBuffer8[1] = 32;
    OutBuffer8[2] = 0;
    OutBuffer8[3] = 13;
    OutBuffer8[4] = 21;
    OutBuffer8[5] = 34;

    CPU_SPI_nWrite8_nRead8(SPI::SPI_Config,OutBuffer8,1,InBuffer8,0,0);

    OutBuffer8[0] = 0;
    OutBuffer8[1] = 0;
    OutBuffer8[2] = 0;
    OutBuffer8[3] = 0;
    OutBuffer8[4] = 0;
    OutBuffer8[5] = 0;

    return true;
}
