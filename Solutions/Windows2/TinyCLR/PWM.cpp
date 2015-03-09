////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

//--//

BOOL PWM_Initialize(PWM_CHANNEL channel)
{
    return TRUE;
}

BOOL PWM_Uninitialize(PWM_CHANNEL channel)
{
    return TRUE;
}

BOOL PWM_ApplyConfiguration(PWM_CHANNEL channel, GPIO_PIN pin, UINT32& period, UINT32& duration, PWM_SCALE_FACTOR& scale, BOOL invert)
{
    return TRUE;
}

BOOL PWM_Start(PWM_CHANNEL channel, GPIO_PIN pin)
{
    return TRUE;
}

void PWM_Stop(PWM_CHANNEL channel, GPIO_PIN pin)
{
}

BOOL PWM_Start(PWM_CHANNEL* channel, GPIO_PIN* pin, UINT32 count)
{
    return TRUE;
}

void PWM_Stop(PWM_CHANNEL* channel, GPIO_PIN* pin, UINT32 count)
{
}

UINT32 PWM_PWMChannels() 
{
    return 2;
}

GPIO_PIN PWM_GetPinForChannel( PWM_CHANNEL channel )
{   
    if(channel == PWM_CHANNEL_0)
    {
        return (GPIO_PIN)256;
    }
    else if(channel == PWM_CHANNEL_1)
    {
        return (GPIO_PIN)257;
    }
    else if(channel == PWM_CHANNEL_2)
    {
        return (GPIO_PIN)258;
    }
    else if(channel == PWM_CHANNEL_3)
    {
        return (GPIO_PIN)259;
    }
    else if(channel == PWM_CHANNEL_4)
    {
        return (GPIO_PIN)260;
    }
    else if(channel == PWM_CHANNEL_5)
    {
        return (GPIO_PIN)261;
    }
    else if(channel == PWM_CHANNEL_6)
    {
        return (GPIO_PIN)262;
    }
    else if(channel == PWM_CHANNEL_7)
    {
        return (GPIO_PIN)263;
    }
    return (GPIO_PIN)GPIO_PIN_NONE;
}

