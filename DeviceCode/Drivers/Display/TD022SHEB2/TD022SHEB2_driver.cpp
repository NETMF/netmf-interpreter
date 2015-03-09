////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "TD022SHEB2.h"

//////////////////////////////////////////////////////////////////////////////

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_TD022SHEB2_Driver"
#endif

TD022SHEB2_Driver g_TD022SHEB2_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

BOOL TD022SHEB2_Driver::Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    BOOL bRet = TRUE;
    
    //reset the cursor pos to the begining
    g_TD022SHEB2_Driver.m_cursor = 0;

    CPU_GPIO_EnableOutputPin( g_TD022SHEB2_Config.SBConfig.ResetPin.Pin , !g_TD022SHEB2_Config.SBConfig.ResetPin.ActiveState  );
    CPU_GPIO_EnableOutputPin( g_TD022SHEB2_Config.SBConfig.LoadPin.Pin  ,  g_TD022SHEB2_Config.SBConfig.LoadPin.ActiveState   );
    CPU_GPIO_EnableOutputPin( g_TD022SHEB2_Config.SBConfig.SclPin.Pin   , !g_TD022SHEB2_Config.SBConfig.SclPin.ActiveState    );
    CPU_GPIO_EnableOutputPin( g_TD022SHEB2_Config.SBConfig.SdaPin.Pin   , !g_TD022SHEB2_Config.SBConfig.SdaPin.ActiveState    );
    CPU_GPIO_EnableOutputPin( g_TD022SHEB2_Config.SBConfig.EnablePin.Pin, !g_TD022SHEB2_Config.SBConfig.EnablePin.ActiveState );

    CPU_GPIO_SetPinState( g_TD022SHEB2_Config.BacklightPin.Pin, g_TD022SHEB2_Config.BacklightPin.ActiveState );

    HAL_Time_Sleep_MicroSeconds_InterruptEnabled( g_TD022SHEB2_Config.SBConfig.StartupTime );

    CPU_GPIO_SetPinState( g_TD022SHEB2_Config.SBConfig.ResetPin.Pin, g_TD022SHEB2_Config.SBConfig.ResetPin.ActiveState );

    // MODE_SEL1: 00100110
    //
    // [7:6] = 00   --> 176x220 pixels.
    // [5:4] = 10   --> normal horizonal scan / reverse vertical scan
    // [3] = 0      --> negative sync polarity
    // [2] = 1      --> HS/VS mode
    // [1] = 1      --> 16 bit data mode
    // [0] = 0      --> don't care
    //
    // NOTE: you can freely play with bits [5:4] to flip the image around horizontal
    // and vertical axes, to suit your needs--doesn't affect video buffer layout.
    //
    WriteCmdByte( 0x01, 0x26 );
    
    // MODE_SEL2: 10000000
    //
    // [7] = 1      --> full color mode
    // [6:5] = 00   --> moving mode
    // [4] = 0      --> normal display
    // [3] = 0      --> line inversion
    // [2] = 0      --> out of range data is white
    // [1:0] = 00   --> don't care.
    //
    WriteCmdByte( 0x02, 0x80 );
    
    bRet = LCD_Controller_Initialize(g_TD022SHEB2_Config.ControllerConfig);

    LCD_Controller_Enable(TRUE);
    
    Clear();

    return bRet;
}

BOOL TD022SHEB2_Driver::Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    Clear();

    LCD_Controller_Enable(FALSE);

    BOOL ret = LCD_Controller_Uninitialize();

    CPU_GPIO_SetPinState( g_TD022SHEB2_Config.SBConfig.EnablePin.Pin, !g_TD022SHEB2_Config.SBConfig.EnablePin.ActiveState );
    CPU_GPIO_SetPinState( g_TD022SHEB2_Config.SBConfig.ResetPin.Pin , !g_TD022SHEB2_Config.SBConfig.ResetPin.ActiveState  );
    CPU_GPIO_SetPinState( g_TD022SHEB2_Config.SBConfig.LoadPin.Pin  ,  g_TD022SHEB2_Config.SBConfig.LoadPin.ActiveState   );

    CPU_GPIO_DisablePin( g_TD022SHEB2_Config.SBConfig.SdaPin.Pin, RESISTOR_PULLUP, 0, GPIO_ALT_PRIMARY );

    return ret;
}

void TD022SHEB2_Driver::Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    memset( LCD_GetFrameBuffer(), 0, SizeInBytes() );

    //reset the cursor pos to the begining
    g_TD022SHEB2_Driver.m_cursor = 0;
}

void TD022SHEB2_Driver::BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();

    ASSERT((x >= 0) && ((x+width) <= g_TD022SHEB2_Config.ControllerConfig.Width));
    ASSERT((y >= 0) && ((y+height) <= g_TD022SHEB2_Config.ControllerConfig.Height));
    ASSERT(g_TD022SHEB2_Config.ControllerConfig.Width % 4 == 0);

    const int c_scanlines = 4;

#define INIT_ADDR_SRC(i) srcAddr[i] = srcAddr[i-1] + g_TD022SHEB2_Config.ControllerConfig.Width;
#define INIT_ADDR_DST(i) dstAddr[i] = dstAddr[i-1] + 1;
#define INIT_ADDR(    i) INIT_ADDR_SRC(i); INIT_ADDR_DST(i);

#define INIT_LINE_SRC(i) src[i]  = srcAddr[i];
#define INIT_LINE_DST(i) dst[i]  = dstAddr[i];
#define INIT_LINE(    i) INIT_LINE_SRC(i); INIT_LINE_DST(i);

#define COPY_PIXEL(    i) *(dst[i]) = *(src[i]);
#define INCR_PIXEL_SRC(i)   src[i] += 1; 
#define INCR_PIXEL_DST(i)   dst[i] -= g_TD022SHEB2_Config.ControllerConfig.Width;
#define INCR_PIXEL(    i)   COPY_PIXEL(i); INCR_PIXEL_SRC(i); INCR_PIXEL_DST(i);

#define INCR_LINE_SRC(i, scanlines) srcAddr[i] += (g_TD022SHEB2_Config.ControllerConfig.Width * scanlines);
#define INCR_LINE_DST(i, scanlines) dstAddr[i] += (1 * scanlines);
#define INCR_LINE(    i, scanlines)     INCR_LINE_SRC(i, scanlines); INCR_LINE_DST(i, scanlines);

    UINT16* dstAddr[c_scanlines];
    UINT16* srcAddr[c_scanlines];

    dstAddr[0] = (UINT16*)LCD_GetFrameBuffer() + ((g_TD022SHEB2_Config.ControllerConfig.Height - 1 - x) * g_TD022SHEB2_Config.ControllerConfig.Width) + y;
    srcAddr[0] = (UINT16*)data + (y * g_TD022SHEB2_Config.ControllerConfig.Width) + x;

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
#define TD022SHEB2_GETBIT(_x,_y,_data,_widthInWords) (((_data[((_x)/32) + (_y)*(_widthInWords)])>>((_x)%32)) & 0x1)

void TD022SHEB2_Driver::BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    _ASSERTE(width        == LCD_GetWidth());
    _ASSERTE(height       == LCD_GetHeight());
    _ASSERTE(widthInWords == width / PixelsPerWord());
    
    BitBltEx( 0, 0, width, height, data );
}

void TD022SHEB2_Driver::WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // convert to LCD pixel coordinates
    row *= Font_Height();
    col *= Font_Width();

    if(row > (g_TD022SHEB2_Config.ControllerConfig.Height - Font_Height())) return;
    if(col > (g_TD022SHEB2_Config.ControllerConfig.Width  - Font_Width() )) return;

    const UINT8* font = Font_GetGlyph( c );

    UINT32* ScreenBuffer = LCD_GetFrameBuffer();

    for(int y = 0; y < Font_Height(); y++)
    {
        for(int x = 0; x < Font_Width(); x+=2)
        {
            UINT32 val = 0;
            // the font data is mirrored
            if(TD022SHEB2_GETBIT( Font_Width() -  x   , y, font, 1 )) val |= 0x07e0;
            if(TD022SHEB2_GETBIT( Font_Width() - (x+1), y, font, 1 )) val |= 0x07e00000;
            
            ScreenBuffer[(row+y)*WidthInWords() + (col+x)/2] = val;
        }
    }
}

void TD022SHEB2_Driver::WriteFormattedChar( unsigned char c )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    if(c < 32)
    {
        switch(c)
        {
        case '\b':                      /* backspace, clear previous char and move cursor back */
            if((g_TD022SHEB2_Driver.m_cursor % TextColumns()) > 0)
            {
                g_TD022SHEB2_Driver.m_cursor--;
                WriteChar( ' ', g_TD022SHEB2_Driver.m_cursor / TextColumns(), g_TD022SHEB2_Driver.m_cursor % TextColumns() );
            }
            break;

        case '\f':                      /* formfeed, clear screen and home cursor */
            //Clear();
            g_TD022SHEB2_Driver.m_cursor = 0;
            break;

        case '\n':                      /* newline */
            g_TD022SHEB2_Driver.m_cursor += TextColumns();
            g_TD022SHEB2_Driver.m_cursor -= (g_TD022SHEB2_Driver.m_cursor % TextColumns());
            break;

        case '\r':                      /* carriage return */
            g_TD022SHEB2_Driver.m_cursor -= (g_TD022SHEB2_Driver.m_cursor % TextColumns());
            break;

        case '\t':                      /* horizontal tab */
            g_TD022SHEB2_Driver.m_cursor += (Font_TabWidth() - ((g_TD022SHEB2_Driver.m_cursor % TextColumns()) % Font_TabWidth()));
            // deal with line wrap scenario
            if((g_TD022SHEB2_Driver.m_cursor % TextColumns()) < Font_TabWidth())
            {
                // bring the cursor to start of line
                g_TD022SHEB2_Driver.m_cursor -= (g_TD022SHEB2_Driver.m_cursor % TextColumns());
            }
            break;

        case '\v':                      /* vertical tab */
            g_TD022SHEB2_Driver.m_cursor += TextColumns();
            break;

        default:
            DEBUG_TRACE2(TRACE_ALWAYS, "Unrecognized control character in LCD_WriteChar: %2u (0x%02x)\r\n", (unsigned int) c, (unsigned int) c);
            break;
        }
    }
    else
    {
        WriteChar( c, g_TD022SHEB2_Driver.m_cursor / TextColumns(), g_TD022SHEB2_Driver.m_cursor % TextColumns() );
        g_TD022SHEB2_Driver.m_cursor++;
    }

    if(g_TD022SHEB2_Driver.m_cursor >= (TextColumns() * TextRows()))
    {
        g_TD022SHEB2_Driver.m_cursor = 0;
    }
}

void TD022SHEB2_Driver::PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return;
}

// Helper that directly clocks a raw byte to the LCD panel serial bus.
void TD022SHEB2_Driver::WriteRawByte( UINT8 data )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    UINT32 cnt = 8;
    
    do {
        // Set up next data bit.
        if (data & 0x80)
            CPU_GPIO_EnableInputPin( g_TD022SHEB2_Config.SBConfig.SdaPin.Pin, FALSE, NULL, GPIO_INT_NONE, RESISTOR_DISABLED );
        else 
            CPU_GPIO_EnableOutputPin( g_TD022SHEB2_Config.SBConfig.SdaPin.Pin, !g_TD022SHEB2_Config.SBConfig.SdaPin.ActiveState );
        
        // Data setup time.
        HAL_Time_Sleep_MicroSeconds_InterruptEnabled( g_TD022SHEB2_Config.SBConfig.SdaSetupTime );
        
        // Clock leading edge.
        CPU_GPIO_SetPinState( g_TD022SHEB2_Config.SBConfig.SclPin.Pin, g_TD022SHEB2_Config.SBConfig.SclPin.ActiveState );
        
        // Data hold time.
        HAL_Time_Sleep_MicroSeconds_InterruptEnabled( g_TD022SHEB2_Config.SBConfig.SdaHoldTime );
        
        // CLock trailing edge.
        CPU_GPIO_SetPinState( g_TD022SHEB2_Config.SBConfig.SclPin.Pin, !g_TD022SHEB2_Config.SBConfig.SclPin.ActiveState );

        // Position next data bit.       
        data <<= 1;
    } while (--cnt);
}

// Write a data byte to a given register of the LCD panel.
void TD022SHEB2_Driver::WriteCmdByte( UINT8 addr, UINT8 data )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // Add write flag (0) to LSB of register address.
    // NOTE: LCD datasheet is WRONG!! It ways write flag is '1', but it's actually '0'.
    addr <<= 1;
    
    // Start transaction by enabling load.
    CPU_GPIO_SetPinState( g_TD022SHEB2_Config.SBConfig.LoadPin.Pin, !g_TD022SHEB2_Config.SBConfig.LoadPin.ActiveState );
    HAL_Time_Sleep_MicroSeconds_InterruptEnabled( g_TD022SHEB2_Config.SBConfig.LoadSetupTime );
    
    // Write address and direction flag
    WriteRawByte( addr );
    
    // Write data byte.
    WriteRawByte( data );
    
    // Stop transaction by disabling load.
    HAL_Time_Sleep_MicroSeconds_InterruptEnabled( g_TD022SHEB2_Config.SBConfig.LoadHoldTime );
    CPU_GPIO_SetPinState( g_TD022SHEB2_Config.SBConfig.LoadPin.Pin, g_TD022SHEB2_Config.SBConfig.LoadPin.ActiveState );

    // Holdoff next possible command.
    HAL_Time_Sleep_MicroSeconds_InterruptEnabled( g_TD022SHEB2_Config.SBConfig.CmdholdTime );
}

UINT32 TD022SHEB2_Driver::PixelsPerWord()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((8*sizeof(UINT32)) / g_TD022SHEB2_Config.ControllerConfig.BitsPerPixel);
}
UINT32 TD022SHEB2_Driver::TextRows()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_TD022SHEB2_Config.ControllerConfig.Height / Font_Height());
}
UINT32 TD022SHEB2_Driver::TextColumns()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_TD022SHEB2_Config.ControllerConfig.Width / Font_Width());
}
UINT32 TD022SHEB2_Driver::WidthInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((g_TD022SHEB2_Config.ControllerConfig.Width + (PixelsPerWord() - 1)) / PixelsPerWord());
}
UINT32 TD022SHEB2_Driver::SizeInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (WidthInWords() * g_TD022SHEB2_Config.ControllerConfig.Height);
}
UINT32 TD022SHEB2_Driver::SizeInBytes()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (SizeInWords() * sizeof(UINT32));
}

