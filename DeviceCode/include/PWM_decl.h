////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_PWM_DECL_H_
#define _DRIVERS_PWM_DECL_H_ 1

//--//

//
// Keep in sync with values in cpu.cs
//
enum PWM_CHANNEL
{
    PWM_CHANNEL_NONE = -1,
    PWM_CHANNEL_0    =  0,
    PWM_CHANNEL_1    =  1,
    PWM_CHANNEL_2    =  2,
    PWM_CHANNEL_3    =  3,
    PWM_CHANNEL_4    =  4,
    PWM_CHANNEL_5    =  5,
    PWM_CHANNEL_6    =  6,
    PWM_CHANNEL_7    =  7,
    PWM_CHANNEL_8    =  8,
    PWM_CHANNEL_9    =  9,
    PWM_CHANNEL_10   = 10,
    PWM_CHANNEL_11   = 11,
    PWM_CHANNEL_12   = 12,
    PWM_CHANNEL_13   = 13,
    PWM_CHANNEL_14   = 14,
    PWM_CHANNEL_15   = 15,
};

enum PWM_SCALE_FACTOR
{
    PWM_MILLISECONDS = 1000,
    PWM_MICROSECONDS = 1000000,
    PWM_NANOSECONDS  = 1000000000,
};

//--//

BOOL     PWM_Initialize        ( PWM_CHANNEL channel );
BOOL     PWM_Uninitialize      ( PWM_CHANNEL channel );
BOOL     PWM_ApplyConfiguration( PWM_CHANNEL channel, GPIO_PIN pin, UINT32& period, UINT32& duration, PWM_SCALE_FACTOR &scale, BOOL invert );
BOOL     PWM_Start             ( PWM_CHANNEL channel, GPIO_PIN pin );
void     PWM_Stop              ( PWM_CHANNEL channel, GPIO_PIN pin );
BOOL     PWM_Start             ( PWM_CHANNEL* channel, GPIO_PIN* pin, UINT32 count );
void     PWM_Stop              ( PWM_CHANNEL* channel, GPIO_PIN* pin, UINT32 count );
UINT32   PWM_PWMChannels       ( );
GPIO_PIN PWM_GetPinForChannel ( PWM_CHANNEL channel );

//--//

#endif // _DRIVERS_PWM_DECL_H_
