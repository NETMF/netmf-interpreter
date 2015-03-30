////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

//--//

BOOL AD_Initialize( ANALOG_CHANNEL channel, INT32 precisionInBits )
{
    return TRUE;
}

void AD_Uninitialize( ANALOG_CHANNEL channel )
{
}

INT32 AD_Read( ANALOG_CHANNEL channel )
{
    return 0;
}

UINT32 AD_ADChannels()
{
    return 8;
}

GPIO_PIN AD_GetPinForChannel( ANALOG_CHANNEL channel )
{   
    if(channel == ANALOG_CHANNEL_0)
    {
        return (GPIO_PIN)264;
    }
    else if(channel == ANALOG_CHANNEL_1)
    {
        return (GPIO_PIN)265;
    }
    else if(channel == ANALOG_CHANNEL_2)
    {
        return (GPIO_PIN)266;
    }
    else if(channel == ANALOG_CHANNEL_3)
    {
        return (GPIO_PIN)267;
    }
    else if(channel == ANALOG_CHANNEL_4)
    {
        return (GPIO_PIN)268;
    }
    else if(channel == ANALOG_CHANNEL_5)
    {
        return (GPIO_PIN)269;
    }
    else if(channel == ANALOG_CHANNEL_6)
    {
        return (GPIO_PIN)270;
    }
    else if(channel == ANALOG_CHANNEL_7)
    {
        return (GPIO_PIN)271;
    }
    return (GPIO_PIN)GPIO_PIN_NONE;
}


BOOL AD_GetAvailablePrecisionsForChannel( ANALOG_CHANNEL channel, INT32* precisions, UINT32& size )
{
    if(precisions == NULL || size == 0)
    {
        return FALSE;
    }
    
    // let's simulate 3 channels
    INT32 precisionStart = 8;
    INT32 precisionIncr = 4;
    UINT32 channelIdx = 0;
    for( ; channelIdx < 3 && channelIdx < size; ++channelIdx)
    {
        precisions[channelIdx] = precisionStart + channelIdx * precisionIncr;
    }

    size = channelIdx;

    return TRUE;
}

//--//
//--//
//--//

BOOL DA_Initialize( DA_CHANNEL channel, INT32 precisionInBits )
{
    return TRUE;
}

void DA_Uninitialize( DA_CHANNEL channel )
{
}

void DA_Write( DA_CHANNEL channel, INT32 level )
{
}

UINT32 DA_DAChannels( )
{
    return 8;
}

GPIO_PIN DA_GetPinForChannel( DA_CHANNEL channel )
{
    if(channel == DA_CHANNEL_0)
    {
        return (GPIO_PIN)272;
    }
    else if(channel == DA_CHANNEL_1)
    {
        return (GPIO_PIN)273;
    }
    else if(channel == DA_CHANNEL_2)
    {
        return (GPIO_PIN)274;
    }
    else if(channel == DA_CHANNEL_3)
    {
        return (GPIO_PIN)275;
    }
    else if(channel == DA_CHANNEL_4)
    {
        return (GPIO_PIN)276;
    }
    else if(channel == DA_CHANNEL_5)
    {
        return (GPIO_PIN)277;
    }
    else if(channel == DA_CHANNEL_6)
    {
        return (GPIO_PIN)278;
    }
    else if(channel == DA_CHANNEL_7)
    {
        return (GPIO_PIN)279;
    }
    return (GPIO_PIN)GPIO_PIN_NONE;
}

BOOL DA_GetAvailablePrecisionsForChannel( DA_CHANNEL channel, INT32* precisions, UINT32& size )
{
    if(precisions == NULL || size == 0)
    {
        return FALSE;
    }
    
    // let's simulate 3 channels
    INT32 precisionStart = 8;
    INT32 precisionIncr = 4;
    UINT32 channelIdx = 0;
    for( ; channelIdx < 3 && channelIdx < size; ++channelIdx)
    {
        precisions[channelIdx] = precisionStart + channelIdx * precisionIncr;
    }

    size = channelIdx;

    return TRUE;
}
