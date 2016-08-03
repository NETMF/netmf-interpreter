////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

using namespace Microsoft::SPOT::Emulator;

void ClrSetLcdDimensions( INT32 width, INT32 height, INT32 bitsPerPixel )
{
    g_HAL_Configuration_Windows.LCD_Width = width;
    g_HAL_Configuration_Windows.LCD_Height = height;
    g_HAL_Configuration_Windows.LCD_BitsPerPixel = bitsPerPixel;
}

bool ClrIsDebuggerStopped()
{
    return CLR_EE_DBG_IS( Stopped );
}


/// NOTE: 06/18/2008-munirula:
/// There's no true framebuffer for LCD in emulator. Unfortunately DirectDrawing for perf in Touch based inking
/// requires framebuffer access, so it can respond to user activties right away, without requiring round trip into
/// managed layer. Making emulator LCD true Framebuffer based device would require non trivial code change, possibly
/// causing issues in other dependencies, hence this shortcut of copy on write framebuffer, that works as a shadow
/// buffer of real Bitmap object in managed LCD object. Drawing to LCD would essentially cause a copy of the
/// drawing in this buffer, while the other way around is not true, for that you will need to explicitly BitBlt
/// the buffer into the LCD.
///
static UINT32* g_FrameBuffer = NULL;
static UINT32 g_FrameBufferSize = 0;
static bool   g_LcdInitialized = false;

BOOL LCD_Initialize()
{
    if(!g_LcdInitialized)
    {
        g_FrameBufferSize = LCD_GetWidth() * LCD_GetHeight() * (LCD_GetBitsPerPixel() / 8); // in bytes
        g_FrameBuffer =  new UINT32[g_FrameBufferSize/sizeof(UINT32)];

        if(g_FrameBuffer == NULL) return FALSE;

        g_LcdInitialized = true;

        return EmulatorNative::GetILcdDriver()->Initialize();
    }

    return TRUE;
}

BOOL LCD_Uninitialize()
{
    if(g_LcdInitialized)
    {
        if (g_FrameBuffer != NULL)
        {
            delete [] g_FrameBuffer;
        }

        g_LcdInitialized = false;

        return EmulatorNative::GetILcdDriver()->Uninitialize();    
    }

    return TRUE;
}

void LCD_Clear()
{
    EmulatorNative::GetILcdDriver()->Clear();    
}

UINT32* LCD_GetFrameBuffer()
{
    return g_FrameBuffer;
}

void LCD_BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{    
    EmulatorNative::GetILcdDriver()->BitBlt( width, height, widthInWords, (IntPtr)data, INT_TO_BOOL(fUseDelta) );    
}

void LCD_BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    /// ASSUMPTION: "data" is of size g_FrameBufferSize. There's no direct way of
    /// knowing size of data[] here.
    memcpy(g_FrameBuffer, data, g_FrameBufferSize);

    EmulatorNative::GetILcdDriver()->BitBltEx( x, y, width, height, (IntPtr)data );    
}

INT32 LCD_GetWidth()
{
    return g_HAL_Configuration_Windows.LCD_Width;
}

INT32 LCD_GetHeight()
{
    return g_HAL_Configuration_Windows.LCD_Height;
}

INT32 LCD_GetBitsPerPixel()
{
    return g_HAL_Configuration_Windows.LCD_BitsPerPixel;   
}

INT32 LCD_GetOrientation()
{
    return g_HAL_Configuration_Windows.LCD_Orientation;
}

UINT32 LCD_ConvertColor(UINT32 color)
{
    return color;
}

