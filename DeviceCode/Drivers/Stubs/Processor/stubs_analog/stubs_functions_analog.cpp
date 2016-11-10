////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

__weak BOOL AD_Initialize( ANALOG_CHANNEL channel, INT32 precisionInBits )
{
    return FALSE;
}

__weak void AD_Uninitialize( ANALOG_CHANNEL channel )
{
}

__weak INT32 AD_Read( ANALOG_CHANNEL channel )
{
    return 0;
}

__weak UINT32 AD_ADChannels()
{
    return 0;
}

__weak GPIO_PIN AD_GetPinForChannel( ANALOG_CHANNEL channel )
{
    return GPIO_PIN_NONE;
}

__weak BOOL AD_GetAvailablePrecisionsForChannel( ANALOG_CHANNEL channel, INT32* precisions, UINT32& size )
{
    size = 0; return FALSE;
}
