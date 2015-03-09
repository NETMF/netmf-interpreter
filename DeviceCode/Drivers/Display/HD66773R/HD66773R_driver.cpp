////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (C) Microsoft Corporation. All rights reserved. Use of this sample source code is subject to 
// the terms of the Microsoft license agreement under which you licensed this sample source code. 
// 
// THIS SAMPLE CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
 *    Copyright (C) Renesas Technology America,Ltd. 2009  All rights reserved.
 */
 
#include <tinyhal.h>
#include "HD66773R.h"

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS                       0x00000001
#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

HD66773R_Driver g_HD66773R_Driver;


UINT16 HD66773R_Driver::VRAM_Buff[(DISP_YMAX* DISP_XMAX )] = { 0 };


void HD66773R_Driver::Delay_MS (UINT16 time)
{
    UINT32 i;
   
    i = 31250;          // 8 ns/instruction * 4 instructions * loop_count = 1ms = 1000000 ns
    i *= time;
    while (i--);        /* loop count */
}

void HD66773R_Driver::Reg_Write (UINT16 reg_addr, UINT16 data)
{

    // memory mapping register is platform dependant
    volatile UINT16 * indexReg = (UINT16 *)PF_INDEX_REG;
    volatile UINT16 * dataReg  = (UINT16 *)PF_DATA_REG;
    

    *indexReg = reg_addr;   /* send the register address to the index register, to set the index register value */
    *dataReg = data;        /* send the data to write to the indexed register */
}

UINT16 HD66773R_Driver::Reg_Read (UINT16 reg_addr)
{
    volatile UINT16 * indexReg = (UINT16 *)PF_INDEX_REG;
    volatile UINT16 * dataReg  = (UINT16 *)PF_DATA_REG;


    *indexReg = reg_addr;   /* send the register address to the index register, to set the index register value */
    return (*dataReg);              /* receive the data from the indexed register */
}


void HD66773R_Driver::Set_Display_On( void )
{
    Reg_Write(DISP_CTRL_REG, 0x0001);
    Delay_MS (100);                             /* 3Frame   */
    Reg_Write(DISP_CTRL_REG, 0x0021);
    Reg_Write(DISP_CTRL_REG, 0x0023);
    Delay_MS (100);                             /* 3Frame   */  
    Reg_Write(DISP_CTRL_REG, 0x0037);      /***normaly white ***/
}

void HD66773R_Driver::Set_Display_Off (void)
{
    Reg_Write(DISP_CTRL_REG, 0x0032);      /* GON = 1, DTE = 1, D1-0 = 10 */
    Delay_MS (100);                             /* 3Frame */
    Reg_Write(DISP_CTRL_REG, 0x0022);      /* GON = 1, DTE = 0, D1-0 = 10 */
    Delay_MS (100);                             /* 3Frame */
    Reg_Write(DISP_CTRL_REG, 0x0000);      /* GON = 0, DTE = 0, D1-0 = 00 */

#if 0 // JJ: this was the original code, I added more code to follow the manual */
    Reg_Write (PWR1_CTRL_REG, 0x0000);
#else
    Reg_Write(PWR1_CTRL_REG, 0x0600);      /* BT2-0 = 110 */
    Delay_MS (100);                             /* wait 100ms */    
    Reg_Write(PWR4_CTRL_REG, 0x0000);      /* PON = 0 */
    Reg_Write(PWR5_CTRL_REG, 0x0000);      /* VCOMG = 0 */
    Reg_Write(PWR1_CTRL_REG, 0x0000);      /* BT2-0 = 000, AP2-0 000 */
#endif
}

void HD66773R_Driver::Data_Trans (UINT16 *data)
{
    INT16 p_x = 0;
    INT16 p_y = 0;

    volatile UINT16 * indexReg = (UINT16 *)PF_INDEX_REG;
    volatile UINT16 * dataReg  = (UINT16 *)PF_DATA_REG;
    
    Reg_Write (RAM_ADDR_SET_REG, 0x0000);   
    *indexReg = RAM_WR_DATA_REG;

    for (p_y = 0; p_y < DISP_YMAX; p_y++)
    {
        for (p_x = 0; p_x < DISP_XMAX; p_x++)
        {
            *dataReg = *data++;
        }
        for (; p_x < PHYSICAL_XMAX; p_x++)
        {
            *dataReg = 0xffff;
        }
    }

    //fill the out of display window pixels
    for (p_y=DISP_YMAX;p_y <PHYSICAL_YMAX; p_y++)
    {
        for (p_x=0; p_x < PHYSICAL_XMAX; p_x++)
        {
            *dataReg = 0xffff;
        }
    }

}


INT32 HD66773R_Driver::Full_Write (UINT16 *lcd_top_addr, UINT16 color)
{
    INT16 p_x, p_y;

    for (p_y = 0; p_y < DISP_YMAX; p_y++)
    {
        for (p_x = 0; p_x < DISP_XMAX; p_x++)
        {
            *lcd_top_addr++ = color;
        }
    }

    return (0);
}


UINT16* HD66773R_Driver::GetScreenBuffer()
{
    return &(VRAM_Buff[0]);    
}


//--//

BOOL HD66773R_Driver::Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    BOOL bRet = TRUE;
    

    //reset the cursor pos to the begining
    g_HD66773R_Driver.m_cursor = 0;


//    Power_Setting();
    Delay_MS (1);
    Reg_Write (OSCILLATION_REG, 0x0001);
    Delay_MS (10);
    
    Reg_Write (PWR1_CTRL_REG, 0x0000);
    Reg_Write (PWR2_CTRL_REG, 0x0000);  
    Reg_Write (PWR3_CTRL_REG, 0x0000);
    Reg_Write (PWR4_CTRL_REG, 0x0F00);  /* Pon = 0 */
    Reg_Write (PWR5_CTRL_REG, 0x0E0F);  /* VCOMG=0 */
    Reg_Write (PWR1_CTRL_REG, 0x0004);  /* SLP="0",STB="0" */
    Delay_MS (40);
    Reg_Write (PWR4_CTRL_REG, 0x0F13);  /* Pon=1 */
    Delay_MS (40);
    Reg_Write (PWR5_CTRL_REG, 0x2E0F);  /* VCOMG=1 */
    Delay_MS (100);

//    Set_Display_Control();
    Reg_Write (DRVR_OUTPUT_CTRL_REG,    0x0515);
    Reg_Write (LCD_DRV_CTRL_REG,        0x0500);
   
    Reg_Write (ENTRY_MODE_REG,          0x1030); 
    Reg_Write (COMPARE_REG,             0x0000);

    Reg_Write (DISP_CTRL_REG,           0x0000);  // GON="0",DTE="0",D1-0="00" /

    Reg_Write (FRAME_CYC_CTRL_REG,      0x0000);
    Reg_Write (GATE_SCAN_POS_REG,       0x0001);  
    Reg_Write (V_SCROLL_CTRL_REG,       0x0000);  
    Reg_Write (DISPLAY1_DRV_POS_REG,    0xA000);   
    Reg_Write( DISPLAY2_DRV_POS_REG,    0xAFAF);

    Reg_Write (H_RAM_ADDR_POS_REG,      0x8300);
    Reg_Write (V_RAM_ADDR_POS_REG,      0xAF00);

    Reg_Write (RAM_WR_DATA_MASK_REG,    0x0000);
    Reg_Write (RAM_ADDR_SET_REG,        0x0000);


//    Set_Gamma();    
    Reg_Write (G_CTRL1_REG,     0x0100);
    Reg_Write (G_CTRL2_REG,     0x0707);
    Reg_Write (G_CTRL3_REG,     0x0102);
    Reg_Write (G_CTRL4_REG,     0x0502);
    Reg_Write (G_CTRL5_REG,     0x0506);
    Reg_Write (G_CTRL6_REG,     0x0000);
    Reg_Write (G_CTRL7_REG,     0x0706);
    Reg_Write (G_CTRL8_REG,     0x0205);
    Reg_Write (G_CTRL9_REG,     0x0000);
    Reg_Write (G_CTRL10_REG,    0x000F);
    
    volatile UINT16 dummyData;
    dummyData = Reg_Read (RAM_WR_DATA_REG);


//    DisplayConfig();
    //Clear();
    Full_Write ((UINT16*)GetScreenBuffer(), WHITE);
    Data_Trans ((UINT16*)GetScreenBuffer());

    Set_Display_On();    

    return bRet;
}

BOOL HD66773R_Driver::Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    Clear();

    Set_Display_Off();

    Delay_MS (1);
    Reg_Write (OSCILLATION_REG, 0x0001);
    Delay_MS (10);
    
    Reg_Write (PWR1_CTRL_REG, 0x0000);
    Reg_Write (PWR2_CTRL_REG, 0x0000);  
    Reg_Write (PWR3_CTRL_REG, 0x0000);
    Reg_Write (PWR4_CTRL_REG, 0x0F00);  /* Pon = 0 */
    Reg_Write (PWR5_CTRL_REG, 0x0E0F);  /* VCOMG=0 */
    Reg_Write (PWR1_CTRL_REG, 0x0003);  /* SLP="1",STB="1" */

    return TRUE;
}

void HD66773R_Driver::Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();


    // have to verified that its memset takes UINT32* or not.
    memset( GetScreenBuffer(), 0, SizeInBytes() );

    //reset the cursor pos to the begining
    g_HD66773R_Driver.m_cursor = 0;
}

void HD66773R_Driver::PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return;
}

// Macro for retriving pixel value in 1-bit bitmaps
//#define HD66773R_GETBIT(_x,_y,_data,_widthInWords) (((_data[((_x)/32) + (_y)*(_widthInWords)])>>((_x)%32)) & 0x1)
#define HD66773R_GETBIT(_x,_y,_data,_widthInWords) ((_data[_y] >> _x)& 0x1)

void HD66773R_Driver::BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    ASSERT(width  <= g_HD66773R_Config.Width );
    ASSERT(height <= g_HD66773R_Config.Height);

    BitBltEx( 0, 0, width, height, data );
}

void HD66773R_Driver::BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();

    ASSERT((x >= 0) && ((x+width) <= LCD_SCREEN_WIDTH));
    ASSERT((y >= 0) && ((y+height) <= LCD_SCREEN_HEIGHT));

    UINT16 * StartOfLine_src = (UINT16 *)&data[0];
    UINT16 * StartOfLine_dst = GetScreenBuffer();

    UINT16 offset = (y * g_HD66773R_Config.Width) + x;

    StartOfLine_src += offset;
    StartOfLine_dst += offset;


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
        
        StartOfLine_src += g_HD66773R_Config.Width;
        StartOfLine_dst += g_HD66773R_Config.Width;
    }
    
    Data_Trans(GetScreenBuffer());
}

void HD66773R_Driver::WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();    
    // convert to LCD pixel coordinates
    
    row *= Font_Height();
    col *= Font_Width();

 
    if(row > (g_HD66773R_Config.Height - Font_Height())) return;
    if(col > (g_HD66773R_Config.Width  - Font_Width() )) return;

    const UINT8* font = Font_GetGlyph( c );

    UINT16* ScreenBuffer = GetScreenBuffer();

    for(int y = 0; y < Font_Height(); y++)
    {
        for(int x = 0; x < Font_Width(); x+=1)
        {
            // the font data is mirrored
            if(HD66773R_GETBIT(Font_Width() - 1 - x  ,y,font,1)) 
                ScreenBuffer[(row+y)*g_HD66773R_Config.Width + (col+x)] = 0x0000;
            else
                ScreenBuffer[(row+y)*g_HD66773R_Config.Width + (col+x)] = 0xFFFF;
        }        
    }    
    Data_Trans(ScreenBuffer);
}

void HD66773R_Driver::WriteFormattedChar( unsigned char c )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    
           
    if(c < 32)
    {        
        switch(c)
        {        
        case '\b':                      /* backspace, clear previous char and move cursor back */
            if((g_HD66773R_Driver.m_cursor % TextColumns()) > 0)
            {
                g_HD66773R_Driver.m_cursor--;
                LCD_WriteChar( ' ', g_HD66773R_Driver.m_cursor / TextColumns(), g_HD66773R_Driver.m_cursor % TextColumns() );
            }
            break;

        case '\f':                      /* formfeed, clear screen and home cursor */
            //LCD_Clear();
            g_HD66773R_Driver.m_cursor = 0;
            break;

        case '\n':                      /* newline */
            g_HD66773R_Driver.m_cursor += TextColumns();
            g_HD66773R_Driver.m_cursor -= (g_HD66773R_Driver.m_cursor % TextColumns());
            break;

        case '\r':                      /* carriage return */
            g_HD66773R_Driver.m_cursor -= (g_HD66773R_Driver.m_cursor % TextColumns());
            break;

        case '\t':                      /* horizontal tab */
            g_HD66773R_Driver.m_cursor += (Font_TabWidth() - ((g_HD66773R_Driver.m_cursor % TextColumns()) % Font_TabWidth()));
            // deal with line wrap scenario
            if((g_HD66773R_Driver.m_cursor % TextColumns()) < Font_TabWidth())
            {
                // bring the cursor to start of line
                g_HD66773R_Driver.m_cursor -= (g_HD66773R_Driver.m_cursor % TextColumns());
            }
            break;

        case '\v':                      /* vertical tab */
            g_HD66773R_Driver.m_cursor += TextColumns();
            break;

        default:
            DEBUG_TRACE2(TRACE_ALWAYS, "Unrecognized control character in LCD_WriteChar: %2u (0x%02x)\r\n", (unsigned int) c, (unsigned int) c);
            break;
        }
    }
    else
    {        
        LCD_WriteChar( c, g_HD66773R_Driver.m_cursor / TextColumns(), g_HD66773R_Driver.m_cursor % TextColumns() );
        g_HD66773R_Driver.m_cursor++;
    }

    if(g_HD66773R_Driver.m_cursor >= (TextColumns() * TextRows()))
    {
        g_HD66773R_Driver.m_cursor = 0;
    }
}

UINT32 HD66773R_Driver::PixelsPerWord()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((8*sizeof(UINT32)) / g_HD66773R_Config.BitsPerPixel);
}

UINT32 HD66773R_Driver::TextRows()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_HD66773R_Config.Height / Font_Height());
}

UINT32 HD66773R_Driver::TextColumns()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();    
    return (g_HD66773R_Config.Width / Font_Width());
}

UINT32 HD66773R_Driver::WidthInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((g_HD66773R_Config.Width + (PixelsPerWord() - 1)) / PixelsPerWord());
}

UINT32 HD66773R_Driver::SizeInWords()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (WidthInWords() * g_HD66773R_Config.Height);
}

UINT32 HD66773R_Driver::SizeInBytes()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (SizeInWords() * sizeof(UINT32));
}
