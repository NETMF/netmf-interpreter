////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "LQ035Q7DB02.h"

//--//

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_LQ035Q7DB02_Driver"
#endif

LQ035Q7DB02_Driver g_LQ035Q7DB02_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

BOOL LQ035Q7DB02_Driver::Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    BOOL bRet = TRUE;
    
    bRet = LCD_Controller_Initialize(g_LQ035Q7DB02_Config.ControllerConfig);

    LCD_Controller_Enable(TRUE);
    
        // Turn on LCD (PC8)
    CPU_GPIO_EnableOutputPin( g_LQ035Q7DB02_Config.EnablePin.Pin, g_LQ035Q7DB02_Config.EnablePin.ActiveState );

    Clear();

    return bRet;
}

BOOL LQ035Q7DB02_Driver::Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    
    Clear();

    LCD_Controller_Enable(FALSE);

    // Turn off LCD (PC8)
    CPU_GPIO_EnableOutputPin( g_LQ035Q7DB02_Config.EnablePin.Pin, !g_LQ035Q7DB02_Config.EnablePin.ActiveState );

    return LCD_Controller_Uninitialize();
}

void LQ035Q7DB02_Driver::PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return;
}

void LQ035Q7DB02_Driver::Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    memset( LCD_GetFrameBuffer(), 0, SizeInBytes() );

    g_LQ035Q7DB02_Driver.m_cursor = 0;
}

// Macro for retriving pixel value in 1-bit bitmaps
#define LQ035Q7DB02_GETBIT(_x,_y,_data,_widthInWords) (((_data[((_x)/32) + (_y)*(_widthInWords)])>>((_x)%32)) & 0x1)
#define LCD_BITBLT_COLOR_0  0x0000
#define LCD_BITBLT_COLOR_1  0x9E79

void LQ035Q7DB02_Driver::BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    ASSERT(width  <= g_LQ035Q7DB02_Config.ControllerConfig.Width );
    ASSERT(height <= g_LQ035Q7DB02_Config.ControllerConfig.Height);

    UINT32* ScreenBuffer = LCD_GetFrameBuffer();

    for(int y = 0; y < height; y++)
    {
        for(int x = 0; x < width; x+=2)
        {
            UINT32 val = 0;
            if(LQ035Q7DB02_GETBIT(x  ,y,data,widthInWords)) val |=          LCD_BITBLT_COLOR_1       ;
            else                                            val |=          LCD_BITBLT_COLOR_0       ;
            if(LQ035Q7DB02_GETBIT(x+1,y,data,widthInWords)) val |= ((UINT32)LCD_BITBLT_COLOR_1) << 16;
            else                                            val |= ((UINT32)LCD_BITBLT_COLOR_0) << 16;

            ScreenBuffer[y*WidthInWords() + x/2] = val;
        }
    }
}

void LQ035Q7DB02_Driver::BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // Make sure the region being redrawn is completely inside the screen
    ASSERT((x >= 0) && ((x+width)  <= g_LQ035Q7DB02_Config.ControllerConfig.Width));
    ASSERT((y >= 0) && ((y+height) <= g_LQ035Q7DB02_Config.ControllerConfig.Height));

    // Adjust x and width so they start out word-aligned
    UINT32  leftAdjustment = x % PixelsPerWord();
    x     -= leftAdjustment;
    width += leftAdjustment;


    // Set the starting addresses and size based on the clipping region
    UINT32  firstPixelAdj = (y * WidthInWords()) + (x / PixelsPerWord());
    UINT32* destAddr      = LCD_GetFrameBuffer() + firstPixelAdj;
    UINT32* srcAddr       = data                                  + firstPixelAdj;
    UINT32  widthInBytes  = (width + (PixelsPerWord() - 1)) / PixelsPerWord() * 4;

    // Copy away
    for(int i = 0; i < height; i++)
    {
        memcpy(destAddr, srcAddr, widthInBytes);
        destAddr += WidthInWords();
        srcAddr  += WidthInWords();
    }
}

void LQ035Q7DB02_Driver::WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // convert to LCD pixel coordinates
    row *= Font_Height();
    col *= Font_Width();

    if(row > (g_LQ035Q7DB02_Config.ControllerConfig.Height - Font_Height())) return;
    if(col > (g_LQ035Q7DB02_Config.ControllerConfig.Width  - Font_Width() )) return;

    const UINT8* font = Font_GetGlyph( c );

    UINT32* ScreenBuffer = LCD_GetFrameBuffer();

    for(int y = 0; y < Font_Height(); y++)
    {
        for(int x = 0; x < Font_Width(); x+=2)
        {
            UINT32 val = 0;
            // the font data is mirrored
            if(LQ035Q7DB02_GETBIT(Font_Width() -  x   ,y,font,1)) val |= 0x07e0;
            if(LQ035Q7DB02_GETBIT(Font_Width() - (x+1),y,font,1)) val |= 0x07e00000;
            
            ScreenBuffer[(row+y)*WidthInWords() + (col+x)/2] = val;
        }
    }
}

void LQ035Q7DB02_Driver::WriteFormattedChar( unsigned char c )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    if(c < 32)
    {
        switch(c)
        {
        case '\b':                      /* backspace, clear previous char and move cursor back */
            if((g_LQ035Q7DB02_Driver.m_cursor % TextColumns()) > 0)
            {
                g_LQ035Q7DB02_Driver.m_cursor--;
                LCD_WriteChar( ' ', g_LQ035Q7DB02_Driver.m_cursor / TextColumns(), g_LQ035Q7DB02_Driver.m_cursor % TextColumns() );
            }
            break;

        case '\f':                      /* formfeed, clear screen and home cursor */
            //LCD_Clear();
            g_LQ035Q7DB02_Driver.m_cursor = 0;
            break;

        case '\n':                      /* newline */
            g_LQ035Q7DB02_Driver.m_cursor += TextColumns();
            g_LQ035Q7DB02_Driver.m_cursor -= (g_LQ035Q7DB02_Driver.m_cursor % TextColumns());
            break;

        case '\r':                      /* carriage return */
            g_LQ035Q7DB02_Driver.m_cursor -= (g_LQ035Q7DB02_Driver.m_cursor % TextColumns());
            break;

        case '\t':                      /* horizontal tab */
            g_LQ035Q7DB02_Driver.m_cursor += (Font_TabWidth() - ((g_LQ035Q7DB02_Driver.m_cursor % TextColumns()) % Font_TabWidth()));
            // deal with line wrap scenario
            if((g_LQ035Q7DB02_Driver.m_cursor % TextColumns()) < Font_TabWidth())
            {
                // bring the cursor to start of line
                g_LQ035Q7DB02_Driver.m_cursor -= (g_LQ035Q7DB02_Driver.m_cursor % TextColumns());
            }
            break;

        case '\v':                      /* vertical tab */
            g_LQ035Q7DB02_Driver.m_cursor += TextColumns();
            break;

        default:
            DEBUG_TRACE2(TRACE_ALWAYS, "Unrecognized control character in LCD_WriteChar: %2u (0x%02x)\r\n", (unsigned int) c, (unsigned int) c);
            break;
        }
    }
    else
    {
        LCD_WriteChar( c, g_LQ035Q7DB02_Driver.m_cursor / TextColumns(), g_LQ035Q7DB02_Driver.m_cursor % TextColumns() );
        g_LQ035Q7DB02_Driver.m_cursor++;
    }

    if(g_LQ035Q7DB02_Driver.m_cursor >= (TextColumns() * TextRows()))
    {
        g_LQ035Q7DB02_Driver.m_cursor = 0;
    }
}

UINT32 LQ035Q7DB02_Driver::PixelsPerWord()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((8*sizeof(UINT32)) / g_LQ035Q7DB02_Config.ControllerConfig.BitsPerPixel);
}
UINT32 LQ035Q7DB02_Driver::TextRows()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_LQ035Q7DB02_Config.ControllerConfig.Height / Font_Height());
}
UINT32 LQ035Q7DB02_Driver::TextColumns()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_LQ035Q7DB02_Config.ControllerConfig.Width / Font_Width());
}
UINT32 LQ035Q7DB02_Driver::WidthInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((g_LQ035Q7DB02_Config.ControllerConfig.Width + (PixelsPerWord() - 1)) / PixelsPerWord());
}
UINT32 LQ035Q7DB02_Driver::SizeInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (WidthInWords() * g_LQ035Q7DB02_Config.ControllerConfig.Height);
}
UINT32 LQ035Q7DB02_Driver::SizeInBytes()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (SizeInWords() * sizeof(UINT32));
}

