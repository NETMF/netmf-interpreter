////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "TD035STEB1.h"

//////////////////////////////////////////////////////////////////////////////

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_TD035STEB1_Driver"
#endif

TD035STEB1_Driver g_TD035STEB1_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

BOOL TD035STEB1_Driver::Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    BOOL bRet = TRUE;

    // The power pins are pulled down (disabled), but it makes sense
    // to at least enable them here
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.LcdEnable.Pin,       !g_TD035STEB1_Config.LcdConfig.LcdEnable.ActiveState       );        // Turn off 5 volt supply to LCD
    CYCLE_DELAY_LOOP(1);                                        // Need to delay about 60 nS - this should be more than sufficient
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.VeeEnable.Pin,       !g_TD035STEB1_Config.LcdConfig.VeeEnable.ActiveState       );        // Turn off +12 and -6.5 volt supplies to LCD
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.BacklightEnable.Pin, !g_TD035STEB1_Config.LcdConfig.BacklightEnable.ActiveState );        // Turn off backlight
    
    bRet = LCD_Controller_Initialize(g_TD035STEB1_Config.ControllerConfig);

    LCD_Controller_Enable(TRUE);
    
    // Kick on the LCD itself
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.LcdEnable.Pin      , g_TD035STEB1_Config.LcdConfig.LcdEnable.ActiveState       );        // Turn on 5 volt supply to LCD
    CYCLE_DELAY_LOOP(1);                                        // Need to delay about 60 nS - this should be more than sufficient
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.VeeEnable.Pin      , g_TD035STEB1_Config.LcdConfig.LcdEnable.ActiveState       );        // Turn on +12 and -6.5 volt supplies to LCD
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.BacklightEnable.Pin, g_TD035STEB1_Config.LcdConfig.BacklightEnable.ActiveState );        // Turn on backlight

    Clear();

    return bRet;
}

BOOL TD035STEB1_Driver::Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // Clear display content
    Clear();

    LCD_Controller_Enable(FALSE);
    
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.BacklightEnable.Pin, !g_TD035STEB1_Config.LcdConfig.BacklightEnable.ActiveState );        // Turn off backlight
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.LcdEnable.Pin      , !g_TD035STEB1_Config.LcdConfig.LcdEnable.ActiveState       );        // Turn off 5 volt supply to LCD
    CYCLE_DELAY_LOOP(1);                                        // Need to delay about 60 nS - this should be more than sufficient
    CPU_GPIO_EnableOutputPin( g_TD035STEB1_Config.LcdConfig.VeeEnable.Pin      , !g_TD035STEB1_Config.LcdConfig.VeeEnable.ActiveState       );        // Turn off +12 and -6.5 volt supplies to LCD

    return LCD_Controller_Uninitialize();
}

void TD035STEB1_Driver::PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return;
}

void TD035STEB1_Driver::Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    memset( LCD_GetFrameBuffer(), 0, SizeInBytes() );

    //reset the cursor pos to the begining
    g_TD035STEB1_Driver.m_cursor = 0;
}

void TD035STEB1_Driver::BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();

    ASSERT((x >= 0) && ((x+width)  <= g_TD035STEB1_Config.ControllerConfig.Width));
    ASSERT((y >= 0) && ((y+height) <= g_TD035STEB1_Config.ControllerConfig.Height));
    ASSERT(g_TD035STEB1_Config.ControllerConfig.Width % 4 == 0);

    const int c_scanlines = 4;

#define INIT_ADDR_SRC(i) srcAddr[i] = srcAddr[i-1] + g_TD035STEB1_Config.ControllerConfig.Width;
#define INIT_ADDR_DST(i) dstAddr[i] = dstAddr[i-1] + 1;
#define INIT_ADDR(    i) INIT_ADDR_SRC(i); INIT_ADDR_DST(i);

#define INIT_LINE_SRC(i) src[i]  = srcAddr[i];
#define INIT_LINE_DST(i) dst[i]  = dstAddr[i];
#define INIT_LINE(    i) INIT_LINE_SRC(i); INIT_LINE_DST(i);

#define COPY_PIXEL(    i) *(dst[i]) = *(src[i]);
#define INCR_PIXEL_SRC(i)   src[i] += 1; 
#define INCR_PIXEL_DST(i)   dst[i] -= g_TD035STEB1_Config.ControllerConfig.Width;
#define INCR_PIXEL(    i)   COPY_PIXEL(i); INCR_PIXEL_SRC(i); INCR_PIXEL_DST(i);

#define INCR_LINE_SRC(i, scanlines) srcAddr[i] += (g_TD035STEB1_Config.ControllerConfig.Width * scanlines);
#define INCR_LINE_DST(i, scanlines) dstAddr[i] += (1 * scanlines);
#define INCR_LINE(    i, scanlines)     INCR_LINE_SRC(i, scanlines); INCR_LINE_DST(i, scanlines);

    UINT16* dstAddr[c_scanlines];
    UINT16* srcAddr[c_scanlines];

    dstAddr[0] = (UINT16*)LCD_GetFrameBuffer() + ((g_TD035STEB1_Config.ControllerConfig.Height - 1 - x) * g_TD035STEB1_Config.ControllerConfig.Width) + y;
    srcAddr[0] = (UINT16*)data + (y * g_TD035STEB1_Config.ControllerConfig.Width) + x;

    INIT_ADDR(1);
    INIT_ADDR(2);
    INIT_ADDR(3);
    
    UINT16* src[c_scanlines];
    UINT16* dst[c_scanlines];

    for(int i = height / c_scanlines; i > 0; i--)
    {
        INIT_LINE(0);
        INIT_LINE(1);
        INIT_LINE(2);
        INIT_LINE(3);

        for(int j = width; j > 0; j--)
        {
            INCR_PIXEL(0);
            INCR_PIXEL(1);
            INCR_PIXEL(2);
            INCR_PIXEL(3);
        }
        
        INCR_LINE(0, c_scanlines);
        INCR_LINE(1, c_scanlines);
        INCR_LINE(2, c_scanlines);
        INCR_LINE(3, c_scanlines);
    } 
    
    for(int i = height % c_scanlines; i > 0; i--)
    {
        INIT_LINE(0);
        for(int j = width; j > 0; j--)        
        {
            INCR_PIXEL(0);
        }
        
        INCR_LINE(0, 1);
    }

#undef INIT_ADDR_SRC
#undef INIT_ADDR_DST
#undef INIT_ADDR

#undef INIT_LINE_SRC
#undef INIT_LINE_DST
#undef INIT_LINE

#undef COPY_PIXEL
#undef INCR_PIXEL_SRC
#undef INCR_PIXEL_DST
#undef INCR_PIXEL

#undef INCR_LINE_SRC
#undef INCR_LINE_DST
#undef INCR_LINE
}

// Macro for retriving pixel value in 1-bit bitmaps
#define TD035STEB1_GETBIT(_x,_y,_data,_widthInWords) (((_data[((_x)/32) + (_y)*(_widthInWords)])>>((_x)%32)) & 0x1)

void TD035STEB1_Driver::BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    _ASSERTE(width        == LCD_GetWidth());
    _ASSERTE(height       == LCD_GetHeight());
    _ASSERTE(widthInWords == width / PixelsPerWord());
    
    BitBltEx( 0, 0, width, height, data );
}

void TD035STEB1_Driver::WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // convert to LCD pixel coordinates
    row *= Font_Height();
    col *= Font_Width();

    if(row > (g_TD035STEB1_Config.ControllerConfig.Height - Font_Height())) return;
    if(col > (g_TD035STEB1_Config.ControllerConfig.Width  - Font_Width() )) return;

    const UINT8* font = Font_GetGlyph( c );

    UINT32* ScreenBuffer = LCD_GetFrameBuffer();

    for(int y = 0; y < Font_Height(); y++)
    {
        for(int x = 0; x < Font_Width(); x+=2)
        {
            UINT32 val = 0;
            // the font data is mirrored
            if(TD035STEB1_GETBIT( Font_Width() -  x   , y, font, 1 )) val |= 0x07e0;
            if(TD035STEB1_GETBIT( Font_Width() - (x+1), y, font, 1 )) val |= 0x07e00000;
            
            ScreenBuffer[(row+y)*WidthInWords() + (col+x)/2] = val;
        }
    }
}

void TD035STEB1_Driver::WriteFormattedChar( unsigned char c )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    if(c < 32)
    {
        switch(c)
        {
        case '\b':                      /* backspace, clear previous char and move cursor back */
            if((g_TD035STEB1_Driver.m_cursor % TextColumns()) > 0)
            {
                g_TD035STEB1_Driver.m_cursor--;
                WriteChar( ' ', g_TD035STEB1_Driver.m_cursor / TextColumns(), g_TD035STEB1_Driver.m_cursor % TextColumns() );
            }
            break;

        case '\f':                      /* formfeed, clear screen and home cursor */
            //Clear();
            g_TD035STEB1_Driver.m_cursor = 0;
            break;

        case '\n':                      /* newline */
            g_TD035STEB1_Driver.m_cursor += TextColumns();
            g_TD035STEB1_Driver.m_cursor -= (g_TD035STEB1_Driver.m_cursor % TextColumns());
            break;

        case '\r':                      /* carriage return */
            g_TD035STEB1_Driver.m_cursor -= (g_TD035STEB1_Driver.m_cursor % TextColumns());
            break;

        case '\t':                      /* horizontal tab */
            g_TD035STEB1_Driver.m_cursor += (Font_TabWidth() - ((g_TD035STEB1_Driver.m_cursor % TextColumns()) % Font_TabWidth()));
            // deal with line wrap scenario
            if((g_TD035STEB1_Driver.m_cursor % TextColumns()) < Font_TabWidth())
            {
                // bring the cursor to start of line
                g_TD035STEB1_Driver.m_cursor -= (g_TD035STEB1_Driver.m_cursor % TextColumns());
            }
            break;

        case '\v':                      /* vertical tab */
            g_TD035STEB1_Driver.m_cursor += TextColumns();
            break;

        default:
            DEBUG_TRACE2(TRACE_ALWAYS, "Unrecognized control character in LCD_WriteChar: %2u (0x%02x)\r\n", (unsigned int) c, (unsigned int) c);
            break;
        }
    }
    else
    {
        WriteChar( c, g_TD035STEB1_Driver.m_cursor / TextColumns(), g_TD035STEB1_Driver.m_cursor % TextColumns() );
        g_TD035STEB1_Driver.m_cursor++;
    }

    if(g_TD035STEB1_Driver.m_cursor >= (TextColumns() * TextRows()))
    {
        g_TD035STEB1_Driver.m_cursor = 0;
    }
}

UINT32 TD035STEB1_Driver::PixelsPerWord()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((8*sizeof(UINT32)) / g_TD035STEB1_Config.ControllerConfig.BitsPerPixel);
}
UINT32 TD035STEB1_Driver::TextRows()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_TD035STEB1_Config.ControllerConfig.Height / Font_Height());
}
UINT32 TD035STEB1_Driver::TextColumns()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_TD035STEB1_Config.ControllerConfig.Width / Font_Width());
}
UINT32 TD035STEB1_Driver::WidthInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((g_TD035STEB1_Config.ControllerConfig.Width + (PixelsPerWord() - 1)) / PixelsPerWord());
}
UINT32 TD035STEB1_Driver::SizeInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (WidthInWords() * g_TD035STEB1_Config.ControllerConfig.Height);
}
UINT32 TD035STEB1_Driver::SizeInBytes()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (SizeInWords() * sizeof(UINT32));
}

