////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

__weak BOOL PWM_Initialize(PWM_CHANNEL channel)
{
    return TRUE;
}

__weak BOOL PWM_Uninitialize(PWM_CHANNEL channel)
{
    return TRUE;
}

__weak BOOL PWM_ApplyConfiguration(PWM_CHANNEL channel, GPIO_PIN pin, UINT32& period, UINT32& duration, PWM_SCALE_FACTOR& scale, BOOL invert)
{
    return TRUE;
}

__weak BOOL PWM_Start(PWM_CHANNEL channel, GPIO_PIN pin)
{
    return TRUE;
}

__weak void PWM_Stop(PWM_CHANNEL channel, GPIO_PIN pin)
{
}

__weak BOOL PWM_Start(PWM_CHANNEL* channel, GPIO_PIN* pin, UINT32 count)
{
    return TRUE;
}

__weak void PWM_Stop(PWM_CHANNEL* channel, GPIO_PIN* pin, UINT32 count)
{
}

__weak UINT32 PWM_PWMChannels() 
{
    return 0;
}

__weak GPIO_PIN PWM_GetPinForChannel( PWM_CHANNEL channel )
{
    return GPIO_PIN_NONE;
}
