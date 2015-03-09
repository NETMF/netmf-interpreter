////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "A025DL02.h"

//////////////////////////////////////////////////////////////////////////////

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_A025DL02_Driver"
#endif

A025DL02_Driver g_A025DL02_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

#define A025DL02_I2C_ADDR_DATA(x,y) (UINT16)(((A025DL02_Driver::x) << 8) | (A025DL02_Driver::y))

BOOL A025DL02_Driver::Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    BOOL bRet = TRUE;

    // setup the LCD by SPI cmds
    {
        UINT16 write[] = 
        { 
            A025DL02_I2C_ADDR_DATA(c_CMD_REG5, c_LCD_START      ), 
            A025DL02_I2C_ADDR_DATA(c_CMD_REG4, c_REG4_SETUP_DATA), 
            A025DL02_I2C_ADDR_DATA(c_CMD_REG8, c_BL_DRV_DATA    ),
            A025DL02_I2C_ADDR_DATA(c_CMD_REG5, c_LCD_NORMAL     )
        };

        UINT16 read[ARRAYSIZE_CONST_EXPR(write)];
        
        SPI_XACTION_16 xAction = 
        {
            write,
            ARRAYSIZE_CONST_EXPR(write),
            read,
            ARRAYSIZE_CONST_EXPR(read),
            0,
            g_A025DL02_SPI_Config.SPI_mod
        };

        CPU_SPI_Xaction_Start(g_A025DL02_SPI_Config);
        CPU_SPI_Xaction_nWrite16_nRead16(xAction);
        CPU_SPI_Xaction_Stop (g_A025DL02_SPI_Config);
    }
    
    bRet = LCD_Controller_Initialize(g_LcdController_Config);

    LCD_Controller_Enable(TRUE);
    
    // Clear display content
    Clear();
    
    return bRet;
}

BOOL A025DL02_Driver::Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    
    // Clear display content
    Clear();

    LCD_Controller_Enable(FALSE);
    
    UINT16 write[] = 
    { 
        A025DL02_I2C_ADDR_DATA(c_CMD_REG5, c_LCD_STANDBY), 
    };

    UINT16 read[ARRAYSIZE_CONST_EXPR(write)];
    
    SPI_XACTION_16 xAction = 
    {
        write,
        ARRAYSIZE_CONST_EXPR(write),
        read,
        ARRAYSIZE_CONST_EXPR(read),
        0,
        g_A025DL02_SPI_Config.SPI_mod
    };

    CPU_SPI_Xaction_Start(g_A025DL02_SPI_Config);
    CPU_SPI_Xaction_nWrite16_nRead16(xAction);
    CPU_SPI_Xaction_Stop (g_A025DL02_SPI_Config);

    return LCD_Controller_Uninitialize();
}

void A025DL02_Driver::PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    UINT16 write[2];
    UINT16 read[ARRAYSIZE_CONST_EXPR(write)];

    if(On)
    {
        write[0] = A025DL02_I2C_ADDR_DATA(c_CMD_REG5, c_LCD_POWERSAVE);
        write[1] = A025DL02_I2C_ADDR_DATA(c_CMD_REG5, c_LCD_POWERSAVE);
    }
    else
    {
        write[0] = A025DL02_I2C_ADDR_DATA(c_CMD_REG5, c_LCD_NORMAL);
        write[1] = A025DL02_I2C_ADDR_DATA(c_CMD_REG5, c_LCD_NORMAL);
    }

    SPI_XACTION_16 xAction = 
    {
        write,
        ARRAYSIZE_CONST_EXPR(write),
        read,
        ARRAYSIZE_CONST_EXPR(read),
        0,
        g_A025DL02_SPI_Config.SPI_mod
    };

    CPU_SPI_Xaction_Start(g_A025DL02_SPI_Config);
    CPU_SPI_Xaction_nWrite16_nRead16(xAction);
    CPU_SPI_Xaction_Stop (g_A025DL02_SPI_Config);

    return;
}

void A025DL02_Driver::Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    memset( LCD_GetFrameBuffer(), 0, SizeInBytes() );

    //reset the cursor pos to the begining
    g_A025DL02_Driver.m_cursor = 0;
}

void A025DL02_Driver::BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();

    ASSERT((x >= 0) && ((x+width) <= LCD_SCREEN_WIDTH));
    ASSERT((y >= 0) && ((y+height) <= LCD_SCREEN_HEIGHT));

    UINT16 * StartOfLine_src = (UINT16 *)&data[0];
    UINT16 * StartOfLine_dst = (UINT16 *)LCD_GetFrameBuffer();

    StartOfLine_src += (y * g_LcdController_Config.Width) + x;
    StartOfLine_dst += (y * g_LcdController_Config.Width) + x;

    while( height-- )
    {
        UINT16 * src;
        UINT16 * dst;
        int      Xcnt;
        
        src = StartOfLine_src;
        dst = StartOfLine_dst;
        Xcnt = width;

        while( Xcnt-- )
        {
            *dst++ = *src++;
        }
        
        StartOfLine_src += g_LcdController_Config.Width;
        StartOfLine_dst += g_LcdController_Config.Width;
    }
}


// Macro for retriving pixel value in 1-bit bitmaps
#define A025DL02_GETBIT(_x,_y,_data,_widthInWords) (((_data[((_x)/32) + (_y)*(_widthInWords)])>>((_x)%32)) & 0x1)

void A025DL02_Driver::BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    _ASSERTE(width        == g_LcdController_Config.Width);
    _ASSERTE(height       == g_LcdController_Config.Height);
    _ASSERTE(widthInWords == width / PixelsPerWord());
    

    BitBltEx( 0, 0, width, height, data );
}

void A025DL02_Driver::WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    // convert to LCD pixel coordinates
    row *= Font_Height();
    col *= Font_Width();

    if(row > (g_LcdController_Config.Height - Font_Height())) return;
    if(col > (g_LcdController_Config.Width  - Font_Width() )) return;

    const UINT8* font = Font_GetGlyph( c );

    UINT32* ScreenBuffer = LCD_GetFrameBuffer();

    for(int y = 0; y < Font_Height(); y++)
    {
        for(int x = 0; x < Font_Width(); x+=2)
        {
            UINT32 val = 0;
            // the font data is mirrored
            if(A025DL02_GETBIT( Font_Width() -  x   , y, font, 1 )) val |= 0x07e0;
            if(A025DL02_GETBIT( Font_Width() - (x+1), y, font, 1 )) val |= 0x07e00000;
            
            ScreenBuffer[(row+y) * WidthInWords() + (col+x)/2] = val;
        }
    }
}

void A025DL02_Driver::WriteFormattedChar( unsigned char c )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    if(c < 32)
    {
        switch(c)
        {
        case '\b':                      /* backspace, clear previous char and move cursor back */
            if((g_A025DL02_Driver.m_cursor % TextColumns()) > 0)
            {
                g_A025DL02_Driver.m_cursor--;
                WriteChar( ' ', g_A025DL02_Driver.m_cursor / TextColumns(), g_A025DL02_Driver.m_cursor % TextColumns() );
            }
            break;

        case '\f':                      /* formfeed, clear screen and home cursor */
            //Clear();
            g_A025DL02_Driver.m_cursor = 0;
            break;

        case '\n':                      /* newline */
            g_A025DL02_Driver.m_cursor += TextColumns();
            g_A025DL02_Driver.m_cursor -= (g_A025DL02_Driver.m_cursor % TextColumns());
            break;

        case '\r':                      /* carriage return */
            g_A025DL02_Driver.m_cursor -= (g_A025DL02_Driver.m_cursor % TextColumns());
            break;

        case '\t':                      /* horizontal tab */
            g_A025DL02_Driver.m_cursor += (Font_TabWidth() - ((g_A025DL02_Driver.m_cursor % TextColumns()) % Font_TabWidth()));
            // deal with line wrap scenario
            if((g_A025DL02_Driver.m_cursor % TextColumns()) < Font_TabWidth())
            {
                // bring the cursor to start of line
                g_A025DL02_Driver.m_cursor -= (g_A025DL02_Driver.m_cursor % TextColumns());
            }
            break;

        case '\v':                      /* vertical tab */
            g_A025DL02_Driver.m_cursor += TextColumns();
            break;

        default:
            DEBUG_TRACE2(TRACE_ALWAYS, "Unrecognized control character in LCD_WriteChar: %2u (0x%02x)\r\n", (unsigned int) c, (unsigned int) c);
            break;
        }
    }
    else
    {
        WriteChar( c, g_A025DL02_Driver.m_cursor / TextColumns(), g_A025DL02_Driver.m_cursor % TextColumns() );
        g_A025DL02_Driver.m_cursor++;
    }

    if(g_A025DL02_Driver.m_cursor >= (TextColumns() * TextRows()))
    {
        g_A025DL02_Driver.m_cursor = 0;
    }
}

UINT32 A025DL02_Driver::PixelsPerWord()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((8*sizeof(UINT32)) / g_LcdController_Config.BitsPerPixel);
}
UINT32 A025DL02_Driver::TextRows()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_LcdController_Config.Height / Font_Height());
}
UINT32 A025DL02_Driver::TextColumns()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_LcdController_Config.Width / Font_Width());
}
UINT32 A025DL02_Driver::WidthInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((g_LcdController_Config.Width + (PixelsPerWord() - 1)) / PixelsPerWord());
}
UINT32 A025DL02_Driver::SizeInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (WidthInWords() * g_LcdController_Config.Height);
}
UINT32 A025DL02_Driver::SizeInBytes()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (SizeInWords() * sizeof(UINT32));
}

