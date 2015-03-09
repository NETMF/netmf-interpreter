////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "spi_backlight.h"

//--//

BOOL BackLight_Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    return SPI_BACKLIGHT_Driver::Initialize( &g_SPI_BackLight_Config );
}

void BackLight_Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    SPI_BACKLIGHT_Driver::Uninitialize( &g_SPI_BackLight_Config );
}

void BackLight_Set( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    SPI_BACKLIGHT_Driver::Set( &g_SPI_BackLight_Config, On );
}

void BackLight_RefCount( BOOL Add )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    SPI_BACKLIGHT_Driver::RefCount( &g_SPI_BackLight_Config, Add );
}

void BackLight_Force( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    SPI_BACKLIGHT_Driver::Set( &g_SPI_BackLight_Config, On );
}

