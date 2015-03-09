////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "gpio_backlight.h"

extern OUTPUT_GPIO_CONFIG g_BackLight_Config;

BOOL BackLight_Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    HAL_CONFIG_BLOCK::ApplyConfig( "GPIO_BACKLIGHT", &g_BackLight_Config, sizeof(g_BackLight_Config) );

    return OUTPUT_GPIO_Driver::Initialize( &g_BackLight_Config );
}

void BackLight_Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    OUTPUT_GPIO_Driver::Uninitialize( &g_BackLight_Config );
}

void BackLight_Set( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    OUTPUT_GPIO_Driver::Set( &g_BackLight_Config, On );
}

void BackLight_RefCount( BOOL Add )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    OUTPUT_GPIO_Driver::RefCount( &g_BackLight_Config, Add );
}

void BackLight_Force( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT();
    OUTPUT_GPIO_Driver::Set( &g_BackLight_Config, On );
}

