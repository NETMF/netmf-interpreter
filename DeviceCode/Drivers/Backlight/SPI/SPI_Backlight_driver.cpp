////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "spi_backlight.h"

//--//

HAL_COMPLETION SPI_Backlight_Completion;

//--//

BOOL SPI_BACKLIGHT_Driver::Initialize( SPI_BACKLIGHT_CONFIG* Config )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    return TRUE;
}


void SPI_BACKLIGHT_Driver::Uninitialize( SPI_BACKLIGHT_CONFIG* Config )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    return;
}

void SPI_BACKLIGHT_Driver::Set( SPI_BACKLIGHT_CONFIG* Config, BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    ASSERT(Config);

    Config->On = On;
    UINT16* writedata;

    if (On)
    {
        writedata = &(Config->BacklightOnWriteData);
    }
    else
    {
        writedata = &(Config->BacklightOffWriteData);
    }
    for (int i = 0; i<= Config->CmdRepeatTimes;i++)
    {
        CPU_SPI_nWrite16_nRead16(Config->SPI_Config, writedata, 1, NULL, 0, 0);
    }

}


void SPI_BACKLIGHT_Driver::RefCount( SPI_BACKLIGHT_CONFIG* Config, BOOL Add )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    return;
}

//--//

