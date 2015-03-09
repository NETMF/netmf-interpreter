////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h" 

//--//

#ifndef _DRIVERS_BACKLIGHT_SPI_H_
#define _DRIVERS_BACKLIGHT_SPI_H_ 1

//--//

struct SPI_BACKLIGHT_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    SPI_CONFIGURATION   SPI_Config;

    UINT16              BacklightOffWriteData;   // data to be send under the one CS active, for 16 bit operation
    UINT16              BacklightOnWriteData;    // data to be send under the one CS active for 16 bit operation

    UINT32              CmdRepeatTimes;            // number of times command to be repeated send

    BOOL                On;
    //--//

    static LPCSTR GetDriverName() { return "SPI_BACKLIGHT"; }
};

extern SPI_BACKLIGHT_CONFIG g_SPI_BackLight_Config;

//--//

struct SPI_BACKLIGHT_Driver
{
    static BOOL Initialize  ( SPI_BACKLIGHT_CONFIG* Config           );
    static void Uninitialize( SPI_BACKLIGHT_CONFIG* Config           );
    static void Set         ( SPI_BACKLIGHT_CONFIG* Config, BOOL On  );
    static void RefCount    ( SPI_BACKLIGHT_CONFIG* Config, BOOL Add );
};

#endif  // _DRIVERS_BACKLIGHT_SPI_H_
