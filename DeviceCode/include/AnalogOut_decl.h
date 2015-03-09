////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  D/A Implementation: Copyright (c) Oberon microsystems, Inc.
//
//  *** Analog Output ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_ANALOGOUT_DECL_H_
#define _DRIVERS_ANALOGOUT_DECL_H_ 1

//--//

//
// Keep in sync with values in cpu.cs
//
enum DA_CHANNEL
{
    DA_CHANNEL_NONE = -1,
    DA_CHANNEL_0    =  0,
    DA_CHANNEL_1    =  1,
    DA_CHANNEL_2    =  2,
    DA_CHANNEL_3    =  3,
    DA_CHANNEL_4    =  4,
    DA_CHANNEL_5    =  5,
    DA_CHANNEL_6    =  6,
    DA_CHANNEL_7    =  7,
};

//--//

BOOL DA_Initialize                      ( DA_CHANNEL channel, INT32 precisionInBits );
void DA_Uninitialize                    ( DA_CHANNEL channel );
void DA_Write                           ( DA_CHANNEL channel, INT32 level );
UINT32 DA_DAChannels                    ( );
GPIO_PIN DA_GetPinForChannel            ( DA_CHANNEL channel );
BOOL DA_GetAvailablePrecisionsForChannel( DA_CHANNEL channel, INT32* precisions, UINT32& size );


//--//

#endif // _DRIVERS_ANALOGOUT_DECL_H_
