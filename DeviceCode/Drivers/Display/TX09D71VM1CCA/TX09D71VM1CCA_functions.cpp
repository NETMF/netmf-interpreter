////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "TX09D71VM1CCA.h"

//--//

BOOL LCD_Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return TX09D71VM1CCA_Driver::Initialize();
}

BOOL LCD_Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return TX09D71VM1CCA_Driver::Uninitialize();
}

void LCD_PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    TX09D71VM1CCA_Driver::PowerSave( On );
}

void LCD_Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    TX09D71VM1CCA_Driver::Clear();
}

void LCD_BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    TX09D71VM1CCA_Driver::BitBlt( width, height, widthInWords, data, fUseDelta );
}

void LCD_BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    TX09D71VM1CCA_Driver::BitBltEx( x, y, width, height, data );
}

void LCD_WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    TX09D71VM1CCA_Driver::WriteChar( c, row, col );
}

void LCD_WriteFormattedChar( unsigned char c )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    TX09D71VM1CCA_Driver::WriteFormattedChar( c );
}

INT32 LCD_GetWidth()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_TX09D71VM1CCA_Config.ControllerConfig.Width;
}

INT32 LCD_GetHeight()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_TX09D71VM1CCA_Config.ControllerConfig.Height;
}

INT32 LCD_GetBitsPerPixel()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_TX09D71VM1CCA_Config.ControllerConfig.BitsPerPixel;
}

UINT32 LCD_GetPixelClockDivider()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_TX09D71VM1CCA_Config.ControllerConfig.PixelClockDivider;
}

INT32 LCD_GetOrientation()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return g_TX09D71VM1CCA_Config.ControllerConfig.Orientation;
}

UINT32 LCD_ConvertColor(UINT32 color)
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();

#if LCD_RGB_CONVERTION == 1
    return  ((color & 0xf800) >> 11) |
            ((color & 0x001f) << 10) |
            ((color & 0x07C0) >>  1);
#else
	return color;
#endif	
}

