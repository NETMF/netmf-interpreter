////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

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
    return 0;
}

GPIO_PIN PWM_GetPinForChannel( PWM_CHANNEL channel )
{
    return GPIO_PIN_NONE;
}

