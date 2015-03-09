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

#include "tinyhal.h"
#include "..\..\..\..\Targets\Native\K60\DeviceCode\Platform\MK60N512MD100.h"
//--//


#ifndef _DRIVERS_DISPLAY_SSD1289_H_
#define _DRIVERS_DISPLAY_SSD1289_H_ 1
// LCD Size Defines
#define LCD_HW_HEIGHT 96//240
#define LCD_HW_WIDTH  96//320


#define SET(x)       (x##_PORT) |= (1 << (x))
#define RESET(x)     (x##_PORT) &= ~(1 << (x))

#define OUTPUT(x)    (x##_DDR) |= (1 << (x)) ; (x##_PCR) = PORT_PCR_MUX(1);
#define INPUT(x)     (x##_DDR) &= ~(1 << (x)); (x##_PCR) = PORT_PCR_MUX(1);
  

#define LCD_COLOR_RGB(R,G,B) ((UINT16)((((R) / 8) << 11) + (((G) / 4) << 5) + ((B) / 8)))
//Colors definition

#define LCD_COLOR_DARK_BLUE     LCD_COLOR_RGB(0, 0, 152)  
#define LCD_COLOR_BRIGHT_BLUE   LCD_COLOR_RGB(222, 219, 255)
#define LCD_COLOR_BLUE          LCD_COLOR_RGB(0, 0, 255)  
#define LCD_COLOR_CYAN          LCD_COLOR_RGB(0, 255, 255)
#define LCD_COLOR_BRIGHT_YELLOW LCD_COLOR_RGB(255, 220, 120)
#define LCD_COLOR_YELLOW        LCD_COLOR_RGB(255, 255, 0) 
#define LCD_COLOR_ORANGE        LCD_COLOR_RGB(255, 152, 96)
#define LCD_COLOR_BRIGHT_RED    LCD_COLOR_RGB(255, 28, 24)  
#define LCD_COLOR_RED           LCD_COLOR_RGB(255, 0, 0)  
#define LCD_COLOR_DARK_RED      LCD_COLOR_RGB(152, 0, 0)  
#define LCD_COLOR_MAGENTA       LCD_COLOR_RGB(255, 0, 255)  
#define LCD_COLOR_BRIGHT_GREEN  LCD_COLOR_RGB(152, 255, 152)
#define LCD_COLOR_GREEN         LCD_COLOR_RGB(0, 255, 0)  
#define LCD_COLOR_DARK_GREEN    LCD_COLOR_RGB(0, 128, 0)  
#define LCD_COLOR_BRIGHT_GREY   LCD_COLOR_RGB(48, 48, 48)  
#define LCD_COLOR_LIGHT_GREY    LCD_COLOR_RGB(120, 120, 120)
#define LCD_COLOR_GREY          LCD_COLOR_RGB(24, 24, 24)   
#define LCD_COLOR_WHITE         LCD_COLOR_RGB(255, 255, 255)
#define LCD_COLOR_BLACK         LCD_COLOR_RGB(0, 0, 0)

#define LCD_COLOR_BKG			LCD_COLOR_BLACK
#define LCD_COLOR_TEXT			LCD_COLOR_GREEN

#define MCU_BUS_CYCLES_100US (MCU_BUS_CLOCK / 10000) 
#define MCU_BUS_CLOCK    96000000L 

#define LCD_ORIENTATION	   				  LANDSCAPE
#define SCREEN_SIZE_LONGER_SIDE  96//320  // The maximum resolution of longer longer side of physical LCD
#define SCREEN_SIZE_SHORTER_SIDE  96//240  // The maximum resolution of  shorter longer side of physical LCD

#define LCD_SPI_MISO_PCR   PORTD_PCR14

//MUNEEB-------------------------------
typedef struct GPIO_MemMap {
  UINT32 PDOR;                                   /*!< Port Data Output Register n, offset: 0x0 */
  UINT32 PSOR;                                   /*!< Port Set Output Register n, offset: 0x4 */
  UINT32 PCOR;                                   /*!< Port Clear Output Register n, offset: 0x8 */
  UINT32 PTOR;                                   /*!< Port Toggle Output Register n, offset: 0xC */
  UINT32 PDIR;                                   /*!< Port Data Input Register n, offset: 0x10 */
  UINT32 POER;                                   /*!< Port Output Enable Register n, offset: 0x14 */
} volatile *GPIO_MemMapPtr;
#define GPIO_PDOR_REG(base) ((base)->PDOR)

/* GPIO - Peripheral instance base addresses */
/*! Peripheral PTA base pointer */
#define PTA_BASE_PTR                             ((GPIO_MemMapPtr)0x400FF000u)
/*! Peripheral PTB base pointer */
#define PTB_BASE_PTR                             ((GPIO_MemMapPtr)0x400FF040u)
/*! Peripheral PTC base pointer */
#define PTC_BASE_PTR                             ((GPIO_MemMapPtr)0x400FF080u)
/*! Peripheral PTD base pointer */
#define PTD_BASE_PTR                             ((GPIO_MemMapPtr)0x400FF0C0u)
/*! Peripheral PTE base pointer */
#define PTE_BASE_PTR                             ((GPIO_MemMapPtr)0x400FF100u)

/* PORT - Peripheral instance base addresses */
/*! Peripheral PORTA base pointer */
#define PORTA_BASE_PTR                           ((PORT_MemMapPtr)0x40049000u)
/*! Peripheral PORTB base pointer */
#define PORTB_BASE_PTR                           ((PORT_MemMapPtr)0x4004A000u)
/*! Peripheral PORTC base pointer */
#define PORTC_BASE_PTR                           ((PORT_MemMapPtr)0x4004B000u)
/*! Peripheral PORTD base pointer */
#define PORTD_BASE_PTR                           ((PORT_MemMapPtr)0x4004C000u)
/*! Peripheral PORTE base pointer */
#define PORTE_BASE_PTR                           ((PORT_MemMapPtr)0x4004D000u)

/* PTB */
#define GPIOB_PDOR                               GPIO_PDOR_REG(PTB_BASE_PTR)
#define GPIOB_PSOR                               GPIO_PSOR_REG(PTB_BASE_PTR)
#define GPIOB_PCOR                               GPIO_PCOR_REG(PTB_BASE_PTR)
#define GPIOB_PTOR                               GPIO_PTOR_REG(PTB_BASE_PTR)
#define GPIOB_PDIR                               GPIO_PDIR_REG(PTB_BASE_PTR)
#define GPIOB_POER                               GPIO_POER_REG(PTB_BASE_PTR)

#define PORTB_PCR17                              PORT_PCR_REG(PORTB_BASE_PTR,17)


#define GPIOA_PDOR      	GPIO_PDOR_REG(PTA_BASE_PTR)

typedef struct PORT_MemMap {
  UINT32 PCR[32];                                /*!< Pin Control Register n, array offset: 0x0, array step: 0x4 */
  UINT32 GPCLR;                                  /*!< Global Pin Control Low Register, offset: 0x80 */
  UINT32 GPCHR;                                  /*!< Global Pin Control High Register, offset: 0x84 */
  UINT8 RESERVED_0[24];
  UINT32 ISFR;                                   /*!< Interrupt Status Flag Register, offset: 0xA0 */
  UINT8 RESERVED_1[28];
  UINT32 DFER;                                   /*!< Digital Filter Enable Register, offset: 0xC0 */
  UINT32 DFCR;                                   /*!< Digital Filter Clock Register, offset: 0xC4 */
  UINT32 DFWR;                                   /*!< Digital Filter Width Register, offset: 0xC8 */
} volatile *PORT_MemMapPtr;
#define PORTD_BASE_PTR                           ((PORT_MemMapPtr)0x4004C000u)
#define PORT_PCR_REG(base,index)                 ((base)->PCR[index])
#define PORTD_PCR14                              PORT_PCR_REG(PORTD_BASE_PTR,14)
#define PORTD_PCR13                              PORT_PCR_REG(PORTD_BASE_PTR,13)
#define PORTD_PCR12                              PORT_PCR_REG(PORTD_BASE_PTR,12)
#define PORTD_PCR15                              PORT_PCR_REG(PORTD_BASE_PTR,15)


typedef struct SPI_MemMap {
  UINT32 MCR;                                    /*!< DSPI Module Configuration Register, offset: 0x0 */
  UINT32 HCR;                                    /*!< DSPI Hardware Configuration Register, offset: 0x4 */
  UINT32 TCR;                                    /*!< DSPI Transfer Count Register, offset: 0x8 */
  union {                                          /* offset: 0xC */
    UINT32 CTAR[2];                                /*!< DSPI Clock and Transfer Attributes Register (In Master Mode), array offset: 0xC, array step: 0x4 */
    UINT32 CTAR_SLAVE[1];                          /*!< DSPI Clock and Transfer Attributes Register (In Slave Mode), array offset: 0xC, array step: 0x4 */
  };
  UINT8 RESERVED_0[24];
  UINT32 SR;                                     /*!< DSPI Status Register, offset: 0x2C */
  UINT32 RSER;                                   /*!< DSPI DMA/Interrupt Request Select and Enable Register, offset: 0x30 */
  union {                                          /* offset: 0x34 */
    UINT32 PUSHR;                                  /*!< DSPI PUSH TX FIFO Register In Master Mode, offset: 0x34 */
    UINT32 PUSHR_SLAVE;                            /*!< DSPI PUSH TX FIFO Register In Slave Mode, offset: 0x34 */
  };
  UINT32 POPR;                                   /*!< DSPI POP RX FIFO Register, offset: 0x38 */
  UINT32 TXFR0;                                  /*!< DSPI Transmit FIFO Registers, offset: 0x3C */
  UINT32 TXFR1;                                  /*!< DSPI Transmit FIFO Registers, offset: 0x40 */
  UINT32 TXFR2;                                  /*!< DSPI Transmit FIFO Registers, offset: 0x44 */
  UINT32 TXFR3;                                  /*!< DSPI Transmit FIFO Registers, offset: 0x48 */
  UINT8 RESERVED_1[48];
  UINT32 RXFR0;                                  /*!< DSPI Receive FIFO Registers, offset: 0x7C */
  UINT32 RXFR1;                                  /*!< DSPI Receive FIFO Registers, offset: 0x80 */
  UINT32 RXFR2;                                  /*!< DSPI Receive FIFO Registers, offset: 0x84 */
  UINT32 RXFR3;                                  /*!< DSPI Receive FIFO Registers, offset: 0x88 */
} volatile *SPI_MemMapPtr;
#define SPI_SR_REG(base)                         ((base)->SR)
#define SPI2_BASE_PTR                            ((SPI_MemMapPtr)0x400AC000u)
#define SPI2_SR                                  SPI_SR_REG(SPI2_BASE_PTR)
#define SPI_PUSHR_REG(base)                      ((base)->PUSHR)
#define SPI2_PUSHR                               SPI_PUSHR_REG(SPI2_BASE_PTR)
#define GPIO_POER_REG(base)                      ((base)->POER)
#define GPIOA_POER                               GPIO_POER_REG(PTA_BASE_PTR)

#define SPI_MCR_REG(base)                        ((base)->MCR)
#define SPI2_MCR                                 SPI_MCR_REG(SPI2_BASE_PTR)
#define SPI_CTAR_REG(base,index2)                ((base)->CTAR[index2])
#define SPI2_CTAR0                               SPI_CTAR_REG(SPI2_BASE_PTR,0)
#define SPI_RSER_REG(base)                       ((base)->RSER)
#define SPI2_RSER                                SPI_RSER_REG(SPI2_BASE_PTR)
#define PORTA_BASE_PTR                           ((PORT_MemMapPtr)0x40049000u)
#define PORTA_PCR26                              PORT_PCR_REG(PORTA_BASE_PTR,26)

#define PORT_PCR_DSE_MASK						 0x40u



struct K60_GPIO
{  
	// Use FAST IO for all ports
    static const UINT32 c_GPIO_Base = 0x400FF000;
	
    //--//

    static const UINT32 c_Pin_None  = 0xFFFFFFFF;
// Controlled by PINSEL0
    static const UINT32 c_PA_00 =   0; 
    static const UINT32 c_PA_01 =   1; 
    static const UINT32 c_PA_02 =   2; 
    static const UINT32 c_PA_03 =   3; 
    static const UINT32 c_PA_04 =   4;                                        
    static const UINT32 c_PA_05 =   5;                                        
    static const UINT32 c_PA_06 =   6;                                      
    static const UINT32 c_PA_07 =   7;                                       
    static const UINT32 c_PA_08 =   8;                                      
    static const UINT32 c_PA_09 =   9;                                     
    static const UINT32 c_PA_10 =  10; 
    static const UINT32 c_PA_11 =  11; 
    static const UINT32 c_PA_12 =  12; 
    static const UINT32 c_PA_13 =  13; 
    static const UINT32 c_PA_14 =  14; 
    static const UINT32 c_PA_15 =  15; 
// Controlled by PINSEL1
    static const UINT32 c_PA_16 =  16; 
    static const UINT32 c_PA_17 =  17; 
    static const UINT32 c_PA_18 =  18; 
    static const UINT32 c_PA_19 =  19; 
    static const UINT32 c_PA_20 =  20; 
    static const UINT32 c_PA_21 =  21; 
    static const UINT32 c_PA_22 =  22; 
    static const UINT32 c_PA_23 =  23; 
    static const UINT32 c_PA_24 =  24; 
    static const UINT32 c_PA_25 =  25; 
    static const UINT32 c_PA_26 =  26; 
    static const UINT32 c_PA_27 =  27; 
    static const UINT32 c_PA_28 =  28; 
    static const UINT32 c_PA_29 =  29; 

// Controlled by PINSEL2
    static const UINT32 c_PB_00 =  30; 
    static const UINT32 c_PB_01 =  31; 
    static const UINT32 c_PB_02 =  32; 
    static const UINT32 c_PB_03 =  33; 
    static const UINT32 c_PB_04 =  34; 
    static const UINT32 c_PB_05 =  35; 
    static const UINT32 c_PB_06 =  36; 
    static const UINT32 c_PB_07 =  37; 
    static const UINT32 c_PB_08 =  38; 
    static const UINT32 c_PB_09 =  39; 
    static const UINT32 c_PB_10 =  40; 
    static const UINT32 c_PB_11 =  41; 
    static const UINT32 c_PB_12 =  42; 
    static const UINT32 c_PB_13 =  43; 
    static const UINT32 c_PB_14 =  44; 
    static const UINT32 c_PB_15 =  45; 
    static const UINT32 c_PB_16 =  46; 
    static const UINT32 c_PB_17 =  47; 
    static const UINT32 c_PB_18 =  48; 
    static const UINT32 c_PB_19 =  49;    
    static const UINT32 c_PB_20 =  50; 
    static const UINT32 c_PB_21 =  51; 
    static const UINT32 c_PB_22 =  52; 
    static const UINT32 c_PB_23 =  53; 
//////////////////////////////////////////////////////////////////
    static const UINT32 c_PC_00 =  54; 
    static const UINT32 c_PC_01 =  55; 
    static const UINT32 c_PC_02 =  56; 
    static const UINT32 c_PC_03 =  57; 
    static const UINT32 c_PC_04 =  58; 
    static const UINT32 c_PC_05 =  59; 
    static const UINT32 c_PC_06 =  60; 
    static const UINT32 c_PC_07 =  61; 
    static const UINT32 c_PC_08 =  62; 
    static const UINT32 c_PC_09 =  63; 
    static const UINT32 c_PC_10 =  64; 
    static const UINT32 c_PC_11 =  65; 
    static const UINT32 c_PC_12 =  66; 
    static const UINT32 c_PC_13 =  67; 
    static const UINT32 c_PC_14 =  68; 
    static const UINT32 c_PC_15 =  69; 
    static const UINT32 c_PC_16 =  70; 
    static const UINT32 c_PC_17 =  71; 
    static const UINT32 c_PC_18 =  72; 
    static const UINT32 c_PC_19 =  73; 
////////////////////////////////////////////////////////////////////    
	static const UINT32 c_PD_00 =  74; 
    static const UINT32 c_PD_01 =  75; 
    static const UINT32 c_PD_02 =  76; 
    static const UINT32 c_PD_03 =  77; 
    static const UINT32 c_PD_04 =  78; 
    static const UINT32 c_PD_05 =  79; 
    static const UINT32 c_PD_06 =  80; 
    static const UINT32 c_PD_07 =  81; 
    static const UINT32 c_PD_08 =  82; 
    static const UINT32 c_PD_09 =  83; 
    static const UINT32 c_PD_10 =  84; 
    static const UINT32 c_PD_11 =  85; 
    static const UINT32 c_PD_12 =  86; 
    static const UINT32 c_PD_13 =  87; 
    static const UINT32 c_PD_14 =  88; 
    static const UINT32 c_PD_15 =  89; 
//////////////////////////////////////////////////////////////
    static const UINT32 c_PE_00 =  90; 
    static const UINT32 c_PE_01 =  91; 
    static const UINT32 c_PE_02 =  92; 
    static const UINT32 c_PE_03 =  93; 
    static const UINT32 c_PE_04 =  94; 
    static const UINT32 c_PE_05 =  95; 
    static const UINT32 c_PE_06 =  96; 
    static const UINT32 c_PE_07 =  97; 
    static const UINT32 c_PE_08 =  98; 
    static const UINT32 c_PE_09 =  99; 
    static const UINT32 c_PE_10 = 100; 
    static const UINT32 c_PE_11 = 101; 
    static const UINT32 c_PE_12 = 102; 
    static const UINT32 c_PE_13 = 103; 
    static const UINT32 c_PE_14 = 104; 
    static const UINT32 c_PE_15 = 105; 
    static const UINT32 c_PE_16 = 106; 
    static const UINT32 c_PE_17 = 107; 
    static const UINT32 c_PE_18 = 108; 
    static const UINT32 c_PE_19 = 109; 
    static const UINT32 c_PE_20 = 110; 
    static const UINT32 c_PE_21 = 111; 
    static const UINT32 c_PE_22 = 112; 
    static const UINT32 c_PE_23 = 113; 
    static const UINT32 c_PE_24 = 114; 
    static const UINT32 c_PE_25 = 115; 
    static const UINT32 c_PE_26 = 116; 
    static const UINT32 c_PE_27 = 117; 
    static const UINT32 c_PE_28 = 118; 
                               
                                                                          
    //--//

					
};

/*
struct K60_GPIO_Driver
{

    struct PIN_ISR_DESCRIPTOR
    {
        //HAL_COMPLETION                 m_completion;
        GPIO_INTERRUPT_SERVICE_ROUTINE m_isr;
        void*                             m_param;
        GPIO_PIN                       m_pin;
        GPIO_INT_EDGE             m_intEdge;
        UINT8                            m_flags;
        UINT8                            m_status;
	 	UINT32				   m_lastExecTicks;

        //--//
        
        static const UINT8 c_Flags_Debounce        = 0x01;
		
		// we are not using these
		
        static const UINT8 c_Flags_RequireLowEdge  = 0x02;
        static const UINT8 c_Flags_RequireHighEdge = 0x04;

        static const UINT8 c_Status_AllowLowEdge   = 0x01;
        static const UINT8 c_Status_AllowHighEdge  = 0x02;

        //--//

        static void Fire( void* arg );

        void HandleDebounce( BOOL edge );
    };
*/
static void PintoReg( UINT32 port, GPIO_MemMapPtr &Gp, PORT_MemMapPtr &Po ) 
{
	switch(port)
	{
		case 0: 
			Gp = PTA_BASE_PTR; 
			Po = PORTA_BASE_PTR;
			break;
		case 1:
			Gp = PTB_BASE_PTR;
			Po = PORTB_BASE_PTR;
			break;
		case 2: 
			Gp = PTC_BASE_PTR;
			Po = PORTC_BASE_PTR;
			break;
		case 3: 
			Gp = PTD_BASE_PTR;
			Po = PORTD_BASE_PTR;
			break;
		case 4: 
			Gp = PTE_BASE_PTR;
			Po = PORTE_BASE_PTR;			
			break;
	}
}

static const UINT32 c_MaxPins = 119;


static UINT32 PinToPort( GPIO_PIN pin )
{
	ASSERT(pin < c_MaxPins);
if (pin <= 29)
	return 0;
if ((pin <= 53) && (pin >= 30) )
	return 1;
if ((pin <= 73) && (pin >= 54))
	return 2;	
if ((pin <= 89) && (pin >= 74) )
	return 3;	
if ((pin <= 118) && (pin >= 90) )
	return 4;	
}

//END-------------------------------

#define LCD_SPI_MOSI_PCR   PORTD_PCR13
#define LCD_SPI_CLK_PCR    PORTD_PCR12
//#define LCD_SPI_CS_PCR     PORTD_PCR11 // PCS0
#define LCD_SPI_CS_PCR     PORTD_PCR15 // PCS1

#define LCD_SPI_ID 2       // SPI module number
#define LCD_SPI_PCS_ID 1   // Chip Select used by SPI
  
#define LCD_SPI_MCR     SPI2_MCR
#define LCD_SPI_CTAR0   SPI2_CTAR0
#define LCD_SPI_RSER    SPI2_RSER
#define LCD_SPI_SR      SPI2_SR
#define LCD_SPI_PUSHR   SPI2_PUSHR
// #define LCD_DC          26          // PTA_26 
// #define LCD_DC_PORT     GPIOA_PDOR  // PortA Output Data Output 
// #define LCD_DC_DDR      GPIOA_POER  // PortA Output Enable 
// #define LCD_DC_PCR      PORTA_PCR26 // PAD configuration register 
#define LCD_DC          17          // PTA_26 
#define LCD_DC_PORT     GPIOB_PDOR  // PortA Output Data Output 
#define LCD_DC_DDR      GPIOB_POER  // PortA Output Enable 
#define LCD_DC_PCR      PORTB_PCR17 // PAD configuration register


typedef enum
{
	PORTRAIT,
	PORTRAIT180,
	LANDSCAPE,
	LANDSCAPE180
} SCREEN_ORIENTATION;
//////////////////////////////////////////////////////////////////////////////

struct SSD1289_Driver
{
    //--//

    UINT32 m_cursor;
	SCREEN_ORIENTATION lcd_orientation;

    //static UINT16 VRAM_Buff[(DISP_YMAX* DISP_XMAX )] ;
    static BOOL Initialize();
    static BOOL Uninitialize();
    static void PowerSave( BOOL On );   
    static void Clear();
    static void BitBltEx( int x, int y, int width, int height, UINT32 data[] );
    static void BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta );
    static void WriteChar         ( unsigned char c, int row, int col );   
    static void WriteFormattedChar( unsigned char c                   );    
	
	
private:
	static BOOL SetWindow(UINT16 x0, UINT16 y0, UINT16 x1, UINT16 y1);
	static void SendCmdWord(UINT16 cmd);	
	static void SendDataWord(UINT16 data);	
	static void SendCommandData(const UINT16 data[], UINT16 count);
    static UINT32 PixelsPerWord();
    static UINT32 TextRows();
    static UINT32 TextColumns();
    static UINT32 WidthInWords();
    static UINT32 SizeInWords();
    static UINT32 SizeInBytes();
};

extern SSD1289_Driver g_SSD1289_Driver;
extern DISPLAY_CONTROLLER_CONFIG g_SSD1289_Config;
extern SPI_CONFIGURATION g_SSD1289_SPI_Config;
#endif  // _DRIVERS_DISPLAY_SSD1289_H_
