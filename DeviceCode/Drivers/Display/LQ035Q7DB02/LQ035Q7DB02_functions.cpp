////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#ifndef _DRIVERS_DISPLAY_LQ035Q7DB02_FUNCTIONS_H_
#ifndef _DRIVERS_DISPLAY_LQ035Q7DB02_FUNCTIONS_H_
#define _DRIVERS_DISPLAY_LQ035Q7DB02_FUNCTIONS_H_ 1
#include "LQ035Q7DB02.h"
//--//

BOOL LCD_Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return LQ035Q7DB02_Driver::Initialize();
}

BOOL LCD_Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return LQ035Q7DB02_Driver::Uninitialize();
}

void LCD_PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    LQ035Q7DB02_Driver::PowerSave( On );
}

void LCD_Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    LQ035Q7DB02_Driver::Clear();
}

void LCD_BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    LQ035Q7DB02_Driver::BitBlt( width, height, widthInWords, data, fUseDelta );
}

void LCD_BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    LQ035Q7DB02_Driver::BitBltEx( x, y, width, height, data );
}

void LCD_WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    LQ035Q7DB02_Driver::WriteChar( c, row, col );
}

void LCD_WriteFormattedChar( unsigned char c )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    LQ035Q7DB02_Driver::WriteFormattedChar( c );
}

INT32 LCD_GetWidth()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_LQ035Q7DB02_Config.ControllerConfig.Width;
}

INT32 LCD_GetHeight()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_LQ035Q7DB02_Config.ControllerConfig.Height;
}

INT32 LCD_GetBitsPerPixel()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_LQ035Q7DB02_Config.ControllerConfig.BitsPerPixel;
}

UINT32 LCD_GetPixelClockDivider()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_LQ035Q7DB02_Config.ControllerConfig.PixelClockDivider;
}

INT32 LCD_GetOrientation()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_LQ035Q7DB02_Config.ControllerConfig.Orientation;
}

UINT32 LCD_ConvertColor(UINT32 color)
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();

    return color;
}

//--//

#endif // _DRIVERS_DISPLAY_LQ035Q7DB02_FUNCTIONS_H_

