////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_GPIO_BUTTON_DECL_H_
#define _DRIVERS_GPIO_BUTTON_DECL_H_ 1

//--//


/*
    BUTTON_B0    - Backlight
    BUTTON_B1    - Undefined
    BUTTON_B2    - Up
    BUTTON_B3    - Undefined
    BUTTON_B4    - Select
    BUTTON_B5    - Down
*/

enum BUTTON_BITIDX
{
    BUTTON_B0_BITIDX = 0,
    BUTTON_B1_BITIDX,
    BUTTON_B2_BITIDX,
    BUTTON_B3_BITIDX,
    BUTTON_B4_BITIDX,
    BUTTON_B5_BITIDX,
};

#define BUTTON_NONE 0x00000000
#define BUTTON_B0   (1 << BUTTON_B0_BITIDX)
#define BUTTON_B1   (1 << BUTTON_B1_BITIDX)
#define BUTTON_B2   (1 << BUTTON_B2_BITIDX)
#define BUTTON_B3   (1 << BUTTON_B3_BITIDX)
#define BUTTON_B4   (1 << BUTTON_B4_BITIDX)
#define BUTTON_B5   (1 << BUTTON_B5_BITIDX)

#define ALL_BUTTONS (BUTTON_B0 | BUTTON_B1 | BUTTON_B2 | BUTTON_B3 | BUTTON_B4 | BUTTON_B5)


struct GPIO_HW_TO_HAL_MAPPING
{
    GPIO_PIN m_HW;
    UINT32   m_HAL;
};

struct GPIO_BUTTON_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    static const int c_COUNT = 6;

    GPIO_HW_TO_HAL_MAPPING Mapping[c_COUNT];

    //--//

    static LPCSTR GetDriverName() { return "GPIO_BUTTON"; }
};

//--//

BOOL   Buttons_Initialize         (                                                 );
BOOL   Buttons_Uninitialize       (                                                 );
BOOL   Buttons_RegisterStateChange( UINT32  ButtonsPressed, UINT32  ButtonsReleased );
BOOL   Buttons_GetNextStateChange ( UINT32& ButtonsPressed, UINT32& ButtonsReleased );
UINT32 Buttons_CurrentState       (                                                 );
UINT32 Buttons_HW_To_Hal_Button   ( UINT32 HW_Buttons                               );
UINT32 Buttons_CurrentHWState     (                                                 );

//--//

extern GPIO_BUTTON_CONFIG g_GPIO_BUTTON_Config;

#endif // _DRIVERS_GPIO_BUTTON_DECL_H_
