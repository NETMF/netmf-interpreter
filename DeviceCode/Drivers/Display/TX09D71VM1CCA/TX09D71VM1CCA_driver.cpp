////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "TX09D71VM1CCA.h"

//////////////////////////////////////////////////////////////////////////////

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_TD035STEB1_Driver"
#endif

TX09D71VM1CCA_Driver g_TX09D71VM1CCA_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

BOOL TX09D71VM1CCA_Driver::Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    BOOL bRet = TRUE;

    // Enable CS of LCD
    CPU_GPIO_EnableOutputPin( g_TX09D71VM1CCA_Config.LcdConfig.LcdEnable.Pin, !g_TX09D71VM1CCA_Config.LcdConfig.LcdEnable.ActiveState);
    
    // Initialize LCD Controller    
    bRet = LCD_Controller_Initialize(g_TX09D71VM1CCA_Config.ControllerConfig);

    LCD_Controller_Enable(TRUE);
    
    // Clear display content
    Clear();

    return bRet;
}

BOOL TX09D71VM1CCA_Driver::Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // Clear display content
    Clear();

    LCD_Controller_Enable(FALSE);

    //Disable CS 0f LCD
    CPU_GPIO_EnableOutputPin( g_TX09D71VM1CCA_Config.LcdConfig.LcdEnable.Pin, g_TX09D71VM1CCA_Config.LcdConfig.LcdEnable.ActiveState);

    return LCD_Controller_Uninitialize();
}

void TX09D71VM1CCA_Driver::PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return;
}

void TX09D71VM1CCA_Driver::Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    memset( LCD_GetFrameBuffer(), 0, SizeInBytes() );

    //reset the cursor pos to the begining
    g_TX09D71VM1CCA_Driver.m_cursor = 0;
}

void TX09D71VM1CCA_Driver::BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    
    ASSERT((x >= 0) && ((x+width) <= LCD_SCREEN_WIDTH));
    ASSERT((y >= 0) && ((y+height) <= LCD_SCREEN_HEIGHT));

    UINT16* StartOfLine_src = (UINT16*)&data[0];
    UINT16* StartOfLine_dst = (UINT16*)LCD_GetFrameBuffer();

    const UINT32 screenWidth = g_TX09D71VM1CCA_Config.ControllerConfig.Width;
    
    StartOfLine_src += (y * screenWidth) + x;
    StartOfLine_dst += (y * screenWidth) + x;

    UINT16* src;
    UINT16* dst;
    INT32   Xcnt;

    while( height-- )
    {   
        src = StartOfLine_src;
        dst = StartOfLine_dst;
        Xcnt = width;

        while( Xcnt-- )
        {
#ifndef PLATFORM_ARM_SAM9RL64_ANY       
            UINT32 val = *src++;
            
            *dst++ = ((val & 0xf800) >> 11) |
                     ((val & 0x001f) << 10) |
                     ((val & 0x07C0) >>  1);
#else
            *dst++ = *src++;
#endif                   
        }
        
        StartOfLine_src += screenWidth;
        StartOfLine_dst += screenWidth;
    }
}

// Macro for retriving pixel value in 1-bit bitmaps
#define TX09D71VM1CCA_GETBIT(_x,_y,_data,_widthInWords) (((_data[((_x)/32) + (_y)*(_widthInWords)])>>((_x)%32)) & 0x1)

void TX09D71VM1CCA_Driver::BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    
#if LCD_BIT_PIX == 4    && LCD_RGB_CONVERTION == 1
    int ScreenWidth =  g_TX09D71VM1CCA_Config.ControllerConfig.Width;
    int ScreenHeight = g_TX09D71VM1CCA_Config.ControllerConfig.Height;
    UINT32 *lcd_base;
    int x;
    UINT32 src;
    if((width != ScreenWidth ) || (height != ScreenHeight)) return;
    lcd_base = (UINT32 *)LCD_GetFrameBuffer();
    for(x = (ScreenWidth * ScreenHeight) >> 1; x > 0; x--)
    {
        src = *data++;
        *lcd_base++ =  ((src & 0xf800f800) >> 11) |
                       ((src & 0x001f001f) << 10) |
                       ((src & 0x07C007C0)>>1);
    }

#elif LCD_BIT_PIX == 4 && LCD_RGB_CONVERTION == 0
    int ScreenWidth =  g_TX09D71VM1CCA_Config.ControllerConfig.Width;
    int ScreenHeight = g_TX09D71VM1CCA_Config.ControllerConfig.Height;
    UINT32 *lcd_base;
    if((width != ScreenWidth ) || (height != ScreenHeight)) 
        return;
    lcd_base = (UINT32 *)LCD_GetFrameBuffer();
    memcpy((UINT8 *)lcd_base, (UINT8 *)data, (ScreenWidth * ScreenHeight) << 1);
        
#endif
}

void TX09D71VM1CCA_Driver::WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // convert to LCD pixel coordinates
    row *= Font_Height();
    col *= Font_Width();

    if(row > (g_TX09D71VM1CCA_Config.ControllerConfig.Height - Font_Height())) return;
    if(col > (g_TX09D71VM1CCA_Config.ControllerConfig.Width  - Font_Width() )) return;

    const UINT8* font = Font_GetGlyph( c );

    UINT32* ScreenBuffer = LCD_GetFrameBuffer();

    for(int y = 0; y < Font_Height(); y++)
    {
        for(int x = 0; x < Font_Width(); x+=2)
        {
            UINT32 val = 0;
            // the font data is mirrored
#ifndef PLATFORM_ARM_SAM9RL64_ANY           
            if(TX09D71VM1CCA_GETBIT( Font_Width() -  x   , y, font, 1 )) val |= 0x7fff;
            if(TX09D71VM1CCA_GETBIT( Font_Width() - (x+1), y, font, 1 )) val |= 0x7fff0000;
#else
            if(TX09D71VM1CCA_GETBIT( Font_Width() -  x   , y, font, 1 )) val |= 0xffff;
            if(TX09D71VM1CCA_GETBIT( Font_Width() - (x+1), y, font, 1 )) val |= 0xffff0000;
#endif            


            
            ScreenBuffer[(row+y)*WidthInWords() + (col+x)/2] = val;
        }
    }
}

void TX09D71VM1CCA_Driver::WriteFormattedChar( unsigned char c )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    if(c < 32)
    {
        switch(c)
        {
        case '\b':                      /* backspace, clear previous char and move cursor back */
            if((g_TX09D71VM1CCA_Driver.m_cursor % TextColumns()) > 0)
            {
                g_TX09D71VM1CCA_Driver.m_cursor--;
                WriteChar( ' ', g_TX09D71VM1CCA_Driver.m_cursor / TextColumns(), g_TX09D71VM1CCA_Driver.m_cursor % TextColumns() );
            }
            break;

        case '\f':                      /* formfeed, clear screen and home cursor */
            //Clear();
            g_TX09D71VM1CCA_Driver.m_cursor = 0;
            break;

        case '\n':                      /* newline */
            g_TX09D71VM1CCA_Driver.m_cursor += TextColumns();
            g_TX09D71VM1CCA_Driver.m_cursor -= (g_TX09D71VM1CCA_Driver.m_cursor % TextColumns());
            break;

        case '\r':                      /* carriage return */
            g_TX09D71VM1CCA_Driver.m_cursor -= (g_TX09D71VM1CCA_Driver.m_cursor % TextColumns());
            break;

        case '\t':                      /* horizontal tab */
            g_TX09D71VM1CCA_Driver.m_cursor += (Font_TabWidth() - ((g_TX09D71VM1CCA_Driver.m_cursor % TextColumns()) % Font_TabWidth()));
            // deal with line wrap scenario
            if((g_TX09D71VM1CCA_Driver.m_cursor % TextColumns()) < Font_TabWidth())
            {
                // bring the cursor to start of line
                g_TX09D71VM1CCA_Driver.m_cursor -= (g_TX09D71VM1CCA_Driver.m_cursor % TextColumns());
            }
            break;

        case '\v':                      /* vertical tab */
            g_TX09D71VM1CCA_Driver.m_cursor += TextColumns();
            break;

        default:
            DEBUG_TRACE2(TRACE_ALWAYS, "Unrecognized control character in LCD_WriteChar: %2u (0x%02x)\r\n", (unsigned int) c, (unsigned int) c);
            break;
        }
    }
    else
    {
        WriteChar( c, g_TX09D71VM1CCA_Driver.m_cursor / TextColumns(), g_TX09D71VM1CCA_Driver.m_cursor % TextColumns() );
        g_TX09D71VM1CCA_Driver.m_cursor++;
    }

    if(g_TX09D71VM1CCA_Driver.m_cursor >= (TextColumns() * TextRows()))
    {
        g_TX09D71VM1CCA_Driver.m_cursor = 0;
        Clear();
    }
}

UINT32 TX09D71VM1CCA_Driver::PixelsPerWord()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((8*sizeof(UINT32)) / g_TX09D71VM1CCA_Config.ControllerConfig.BitsPerPixel);
}
UINT32 TX09D71VM1CCA_Driver::TextRows()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_TX09D71VM1CCA_Config.ControllerConfig.Height / Font_Height());
}
UINT32 TX09D71VM1CCA_Driver::TextColumns()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_TX09D71VM1CCA_Config.ControllerConfig.Width / Font_Width());
}
UINT32 TX09D71VM1CCA_Driver::WidthInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((g_TX09D71VM1CCA_Config.ControllerConfig.Width + (PixelsPerWord() - 1)) / PixelsPerWord());
}
UINT32 TX09D71VM1CCA_Driver::SizeInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (WidthInWords() * g_TX09D71VM1CCA_Config.ControllerConfig.Height);
}
UINT32 TX09D71VM1CCA_Driver::SizeInBytes()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (SizeInWords() * sizeof(UINT32));
}

