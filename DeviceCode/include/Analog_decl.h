////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_ANALOG_DECL_H_
#define _DRIVERS_ANALOG_DECL_H_ 1

//--//

//
// Keep in sync with values in cpu.cs
//
enum ANALOG_CHANNEL
{
    ANALOG_CHANNEL_NONE = -1,
    ANALOG_CHANNEL_0    =  0,
    ANALOG_CHANNEL_1    =  1,
    ANALOG_CHANNEL_2    =  2,
    ANALOG_CHANNEL_3    =  3,
    ANALOG_CHANNEL_4    =  4,
    ANALOG_CHANNEL_5    =  5,
    ANALOG_CHANNEL_6    =  6,
    ANALOG_CHANNEL_7    =  7,
};

//--//

// a valua of - 1 for the precision in bits indicates maximum available precision
BOOL     AD_Initialize                      ( ANALOG_CHANNEL channel, INT32 precisionInBits );
void     AD_Uninitialize                    ( ANALOG_CHANNEL channel );
INT32    AD_Read                            ( ANALOG_CHANNEL channel );
UINT32   AD_ADChannels                      ( );
GPIO_PIN AD_GetPinForChannel                ( ANALOG_CHANNEL channel );
BOOL     AD_GetAvailablePrecisionsForChannel( ANALOG_CHANNEL channel, INT32* precisions, UINT32& size );


//--//

#endif // _DRIVERS_ANALOG_DECL_H_
