////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
struct I2C_CONFIGURATION; 

__weak BOOL I2C_Internal_Initialize()
{
    return FALSE;
}

__weak BOOL I2C_Internal_Uninitialize()
{
    return FALSE;
}

__weak void I2C_Internal_XActionStart( I2C_HAL_XACTION* xAction, bool repeatedStart )
{
}

__weak void I2C_Internal_XActionStop()
{
}

__weak void I2C_Internal_GetClockRate( UINT32 rateKhz, UINT8& clockRate, UINT8& clockRate2)
{
    return ;
}

__weak void I2C_Internal_GetPins( GPIO_PIN& scl, GPIO_PIN& sda )
{
    scl = GPIO_PIN_NONE;
    sda = GPIO_PIN_NONE;
}
