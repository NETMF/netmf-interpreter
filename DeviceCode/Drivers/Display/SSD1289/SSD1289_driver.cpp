//-----------------------------------------------------------------------------
// Software that is described herein is for illustrative purposes only
// which provides customers with programming information regarding the
// products. This software is supplied "AS IS" without any warranties.
// Freescale Semiconductors assumes no responsibility or liability for the
// use of the software, conveys no license or title under any patent,
// copyright, or mask work right to the product. Freescale Semiconductors
// reserves the right to make changes in the software without
// notification. Freescale Semiconductors also make no representation or
// warranty that such application will be suitable for the specified
// use without further testing or modification.
//-----------------------------------------------------------------------------

#include <tinyhal.h>
#include "SSD1289.h"

//////////////////////////////////////////////////////////////////////////////

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

// MUNEEB PASCAL
/*
UINT32 pascal[] = { 
#include "pascal2.h" 
};
*/

//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_SSD1289_Driver"
#endif

SSD1289_Driver g_SSD1289_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif
void Cpu_Delay100US(UINT16 us100)
{
	UINT16 i;
    UINT16 x = 0;
	while((us100--))
	{
	  for(i=0; i < (MCU_BUS_CYCLES_100US/10); i++) 
		{// 10 cycles delay
		  /*__asm{ NOP };
		  __asm{ NOP };
		  __asm{ NOP };*/
		  ++x;
          ++x;
		}
	}
}

void Delay_ms_Common(UINT16 period)   //delay routine (milliseconds)
{
    while (period != 0) 
	{      
      Cpu_Delay100US(10);
      period--;
    }
}


void SSD1289_Driver::SendCmdWord(UINT16 cmd)
{
    RESET(LCD_DC);                   //Switch to data mode
    (void)SendDataWord(cmd);
    SET(LCD_DC);                     
    return ;
}
void SSD1289_Driver::SendDataWord(UINT16 value)
{
	// wait write buffer not full flag        
	while (!(LCD_SPI_SR & SPI_SR_TFFF_MASK)){}; 
	// Assert CS0, Use config 0
	LCD_SPI_PUSHR = SPI_PUSHR_PCS(1 << (LCD_SPI_PCS_ID)) | SPI_PUSHR_CTAS(0) | SPI_PUSHR_TXDATA((UINT16)value);

	while (!(LCD_SPI_SR & SPI_SR_TCF_MASK)){};// while shift-out complete
	LCD_SPI_SR = SPI_SR_TCF_MASK;           // clear flag
	return;
}
void SSD1289_Driver::SendCommandData(const UINT16 data[], UINT16 count)
{
  int i;   
  for (i=0; i<count; i+=2)
  {
	(void)SendCmdWord((UINT16)data[i]);
	(void)SendDataWord((UINT16)data[i+1]);       
  }
}

BOOL SSD1289_Driver::SetWindow(UINT16 x1, UINT16 y1, UINT16 x2, UINT16 y2)
{
	UINT16 x1_x2;
	UINT16 Addr1, Addr2;

	switch (g_SSD1289_Driver.lcd_orientation)
	{
		default:
			// Invalid! Fall through to portrait mode
		case PORTRAIT:
			Addr1 = x1; 
			Addr2 = y1;
			x1_x2 = (UINT16)((x2<<8) + x1);   // pack X-Values into one word            
			break;		
		case PORTRAIT180:
			Addr1 = (UINT16)(SCREEN_SIZE_SHORTER_SIDE - 1 - x1); 
			Addr2 = (UINT16)(SCREEN_SIZE_LONGER_SIDE - 1 - y1);
			x1_x2 = (UINT16)((Addr1<<8) + (SCREEN_SIZE_SHORTER_SIDE - 1 - x2));    // pack X-Values into one word
			y1 = (UINT16)(SCREEN_SIZE_LONGER_SIDE - 1 - y2);
			y2 = Addr2;
			break;		
		case LANDSCAPE:
			Addr1 = (UINT16)(SCREEN_SIZE_SHORTER_SIDE - 1 - y1); 
			Addr2 = x1;
			x1_x2 = (UINT16)((Addr1<<8) + (SCREEN_SIZE_SHORTER_SIDE - 1 - y2));    // pack X-Values into one word
			y1 = x1;
			y2 = x2;
			break;		
		case LANDSCAPE180:
			Addr1 = y1; 
			Addr2 = (UINT16)(SCREEN_SIZE_LONGER_SIDE - 1 - x1);    // pack X-Values into one word
			x1_x2 = (UINT16)((y2<<8) + y1);
			y1 = (UINT16)(SCREEN_SIZE_LONGER_SIDE - 1 - x2);
			y2 = Addr2;
			break;
	}

	//Set Window
	(void)SendCmdWord(0x0044); 
	(void)SendDataWord((UINT16)x1_x2);
	(void)SendCmdWord(0x0045);
	(void)SendDataWord((UINT16)y1);
	(void)SendCmdWord(0x0046);
	(void)SendDataWord((UINT16)y2);
	// Set Start Address counter
	(void)SendCmdWord(0x004e);
	(void)SendDataWord((UINT16)Addr1);
	(void)SendCmdWord(0x004f);
	(void)SendDataWord((UINT16)Addr2);
	(void)SendCmdWord(0x0022);

	return TRUE; 
}

BOOL SSD1289_Driver::Initialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
        
    //reset the cursor pos to the begining
    g_SSD1289_Driver.m_cursor = 0;
	g_SSD1289_Driver.lcd_orientation = LANDSCAPE;
    const UINT16 init_data[] = 
    {
        0x0000,  0x0001,        
        0x0003,  0xAEAC, 0x000C,  0x0007, 0x000D,  0x000F, 0x000E,  0x2900, 0x001E,  0x00B3,        
        0x0001,  0x2B3F, 0x0002,  0x0600, 0x0010,  0x0000, 0x0011,  0x60B0,        
        0x0005,  0x0000, 0x0006,  0x0000, 0x0016,  0xEF1C, 0x0017,  0x0003, 0x0007,  0x0233,
        0x000B,  0x5312, 0x000F,  0x0000,        
        0x0041,  0x0000, 0x0042,  0x0000, 0x0048,  0x0000, 0x0049,  0x013F, 0x0044,  0xEF00, 
        0x0045,  0x0000, 0x0046,  0x013F, 0x004A,  0x0000, 0x004B,  0x0000,        
        0x0030,  0x0707, 0x0031,  0x0704, 0x0032,  0x0204, 0x0033,  0x0201, 0x0034,  0x0203,
        0x0035,  0x0204, 0x0036,  0x0204, 0x0037,  0x0502, 0x003A,  0x0302, 0x003B,  0x0500,
        0x0023 , 0x0000, 0x0024 , 0x0000
    };
	
	/* Hardware Init */
    
	LCD_SPI_MISO_PCR = (PORT_PCR_MUX(2)|PORT_PCR_DSE_MASK);
    LCD_SPI_MOSI_PCR = (PORT_PCR_MUX(2)|PORT_PCR_DSE_MASK);
	LCD_SPI_CLK_PCR  = (PORT_PCR_MUX(2)|PORT_PCR_DSE_MASK);
	LCD_SPI_CS_PCR   = (PORT_PCR_MUX(2)|PORT_PCR_DSE_MASK);

    //  MUNEEB GOOD
    SIM->SCGC3 |= SIM_SCGC3_SPI2_MASK;
	
    RESET(LCD_DC);
    OUTPUT(LCD_DC);
         
    // Enable and clear SPI
    LCD_SPI_MCR &= (~ SPI_MCR_MDIS_MASK);    
    LCD_SPI_MCR = SPI_MCR_HALT_MASK | SPI_MCR_CLR_TXF_MASK | SPI_MCR_CLR_RXF_MASK;  
    // 15+1 = 16-bit transfers, Fclk = Fsys/4
    LCD_SPI_CTAR0 = SPI_CTAR_FMSZ(15) | SPI_CTAR_BR(0);
    // tweak off the SPI frequency to maximum 25Mb/s, standard 12Mb/s	
    LCD_SPI_CTAR0 |= SPI_CTAR_DBR_MASK;
    // Set CS0-7 inactive high 
    LCD_SPI_MCR |= (SPI_MCR_PCSIS(1 << (LCD_SPI_PCS_ID))|SPI_MCR_MSTR_MASK);    
    // Disable all IRQs
    LCD_SPI_RSER = 0;
    // clear Flag
    LCD_SPI_SR = SPI_SR_TFFF_MASK;
    LCD_SPI_SR = SPI_SR_TCF_MASK;    
    // Enable SPI
    LCD_SPI_MCR &= (~ SPI_MCR_HALT_MASK);   	
	
	Delay_ms_Common(300);     
    SendCommandData(&init_data[0], 2);    Delay_ms_Common(15); 
    SendCommandData(&init_data[2], 10);   Delay_ms_Common(15); 
    SendCommandData(&init_data[12], 8);   Delay_ms_Common(150);
    SendCommandData(&init_data[20], 14);  Delay_ms_Common(20);
    SendCommandData(&init_data[34], 18);  Delay_ms_Common(20);    
    SendCommandData(&init_data[52], 24);  Delay_ms_Common(31);
	
  	UINT16 LCD_EntryMode = 0;	
	switch (g_SSD1289_Driver.lcd_orientation)
	{
		default: // Invalid! Fall through to portrait mode
		case PORTRAIT:
			LCD_EntryMode = 0x60b0; break;  // &B110000010110000
		case PORTRAIT180:
			LCD_EntryMode = 0x6080; break;  // &B110000010000000
		case LANDSCAPE:
			LCD_EntryMode = 0x60a8; break;  // &B110000010101000
		case LANDSCAPE180:
			LCD_EntryMode = 0x6098; break;  // &B110000010011000
	}
	(void)SendCmdWord(0x0011);
	(void)SendDataWord(LCD_EntryMode);
	(void)SetWindow(0,0,1,1);

    // MUNEEB DRAW SOMETHING
    Clear();
    Delay_ms_Common(1000);
    WriteChar('c',5,5);
    Delay_ms_Common(1000);
    
    return TRUE;
}

BOOL SSD1289_Driver::Uninitialize()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    Clear();
    return TRUE;
}

void SSD1289_Driver::PowerSave( BOOL On )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return;
} //done

void SSD1289_Driver::Clear()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();    
    SetWindow(0, 0, LCD_HW_WIDTH-1, LCD_HW_HEIGHT-1);
	for(int i=0;i<LCD_HW_WIDTH*LCD_HW_HEIGHT;i++) 
	{
	  SendDataWord(LCD_COLOR_BKG);
	}
    //reset the cursor pos to the begining
    g_SSD1289_Driver.m_cursor = 0;  
} //done


void SSD1289_Driver::BitBltEx( int x, int y, int width, int height, UINT32 data[] )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();

    ASSERT((x >= 0) && ((x+width) <= LCD_SCREEN_WIDTH));
    ASSERT((y >= 0) && ((y+height) <= LCD_SCREEN_HEIGHT));

    UINT16 * StartOfLine_src = (UINT16*)&data[0];
    SetWindow(x, y, (x+width-1),(y+height-1));
	 
    UINT16 offset = (y * g_SSD1289_Config.Width) + x;
    StartOfLine_src += offset;     
	 	 
    while( height-- )
    {
        UINT16 * src;    
        int      Xcnt;        
        src = StartOfLine_src;     
        Xcnt = width;
        while( Xcnt-- )
        {             	
	    	SendDataWord(*src++);			
        }        
        StartOfLine_src += g_SSD1289_Config.Width;         
    }         
}

// Macro for retriving pixel value in 1-bit bitmaps

void SSD1289_Driver::BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta )
{
     NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
     _ASSERTE(width        == LCD_GetWidth());
     _ASSERTE(height       == LCD_GetHeight());
     _ASSERTE(widthInWords == width / PixelsPerWord());
    
     BitBltEx( 0, 0, width, height, data );
} //done

void SSD1289_Driver::WriteChar( unsigned char c, int row, int col )
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();    

    // MUNEEB - Font_Width refers to font 8x8 or 8x15...so limiting it to 8x8.  tinyclr.proj defines that we are using 8x8
	//int width = Font_Width();
	//int height = Font_Height();
	int width = 8;
    int height = 8;
    
	// convert to LCD pixel coordinates
    row *= height;
    col *= width;

    if(row > (g_SSD1289_Config.Height - height)) return;
    if(col > (g_SSD1289_Config.Width  - width )) return;

    const UINT8* font = Font_GetGlyph( c );	
    // MUNEEB - SEE NOTE ABOVE
	//UINT16 data[height*width];
	UINT16 data[8*8];
	int i=0;
	for(int y = 0; y < height; y++)
    {
        for(int x = 0; x < width; x++)
        {
            UINT16 val = LCD_COLOR_BKG;             
            // the font data is mirrored
			if ((font[y]&(1<<(width-x-1))) != 0) val |= LCD_COLOR_TEXT;
            data[i]=val;
			i++;
        }	   
    }	
	SetWindow(col, row, col+width-1, row+height-1);
	for(i=0;i<width*height;i++) 
	{
	  SendDataWord(data[i]);
	}	 
} //done

 void SSD1289_Driver::WriteFormattedChar( unsigned char c)
{
     NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
     if(c < 32)
     {
         switch(c)
         {
         case '\b':                      /* backspace, clear previous char and move cursor back */
             if((g_SSD1289_Driver.m_cursor % TextColumns()) > 0)
             {
                 g_SSD1289_Driver.m_cursor--;
                 WriteChar( ' ', g_SSD1289_Driver.m_cursor / TextColumns(), g_SSD1289_Driver.m_cursor % TextColumns() );
             }
             break;

         case '\f':                      /* formfeed, clear screen and home cursor */
             //Clear();
             g_SSD1289_Driver.m_cursor = 0;
             break;

         case '\n':                      /* newline */
             g_SSD1289_Driver.m_cursor += TextColumns();
             g_SSD1289_Driver.m_cursor -= (g_SSD1289_Driver.m_cursor % TextColumns());
             break;

         case '\r':                      /* carriage return */
             g_SSD1289_Driver.m_cursor -= (g_SSD1289_Driver.m_cursor % TextColumns());
             break;

         case '\t':                      /* horizontal tab */
             g_SSD1289_Driver.m_cursor += (Font_TabWidth() - ((g_SSD1289_Driver.m_cursor % TextColumns()) % Font_TabWidth()));
             // deal with line wrap scenario
             if((g_SSD1289_Driver.m_cursor % TextColumns()) < Font_TabWidth())
             {
                 // bring the cursor to start of line
                 g_SSD1289_Driver.m_cursor -= (g_SSD1289_Driver.m_cursor % TextColumns());
             }
             break;

         case '\v':                      /* vertical tab */
             g_SSD1289_Driver.m_cursor += TextColumns();
             break;

         default:
             DEBUG_TRACE2(TRACE_ALWAYS, "Unrecognized control character in LCD_WriteChar: %2u (0x%02x)\r\n", (unsigned int) c, (unsigned int) c);
             break;
         }
     }
     else
     {
         WriteChar( c, g_SSD1289_Driver.m_cursor / TextColumns(), g_SSD1289_Driver.m_cursor % TextColumns() );
         g_SSD1289_Driver.m_cursor++;
     }

     if(g_SSD1289_Driver.m_cursor >= (TextColumns() * TextRows()))
     {
         g_SSD1289_Driver.m_cursor = 0;
     }
} //done

UINT32 SSD1289_Driver::PixelsPerWord()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return ((8*sizeof(UINT32)) / g_SSD1289_Config.BitsPerPixel);
} //done
UINT32 SSD1289_Driver::TextRows()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_SSD1289_Config.Height / Font_Height());
} //done
UINT32 SSD1289_Driver::TextColumns()
{
    NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
    return (g_SSD1289_Config.Width / Font_Width());
} //done
UINT32 SSD1289_Driver::WidthInWords()
{
     NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
     return ((g_SSD1289_Config.Width + (PixelsPerWord() - 1)) / PixelsPerWord());
} //done
UINT32 SSD1289_Driver::SizeInWords()
{
     NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
     return (WidthInWords() * g_SSD1289_Config.Height);
} //done
UINT32 SSD1289_Driver::SizeInBytes()
{
     NATIVE_PROFILE_HAL_DRIVERS_DISPLAY();
     return (SizeInWords() * sizeof(UINT32));
} //done

