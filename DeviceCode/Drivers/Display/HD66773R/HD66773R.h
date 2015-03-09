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
 
#include "tinyhal.h"

#ifndef _DRIVERS_DISPLAY_HD66773R_H_
#define _DRIVERS_DISPLAY_HD66773R_H_ 1


// LCD Size Defines
#define PHYSICAL_XMAX 132
#define PHYSICAL_YMAX 176

#define DISP_XMIN   (0) 
#define DISP_XMAX   (128)  
#define DISP_YMIN   (0) 
#define DISP_YMAX   (160)  

#define LCD_ONELINE_OFFSET  (DISP_XMAX *2)

// Colors
#define RED     (0xF800)
#define GREEN   (0x07E0)
#define BLUE    (0x001F)
#define WHITE   (0xFFFF)
#define BLACK   (0x0000)
#define YELLOW  (0xFFE0)
#define CYAN    (0x07FF)
#define MAGENTA (0xF81F)



// HD6673R Register Definitions
#define OSCILLATION_REG         (0x0000)
#define DRVR_OUTPUT_CTRL_REG    (0x0001)
#define LCD_DRV_CTRL_REG        (0x0002)
#define PWR1_CTRL_REG           (0x0003)
#define PWR2_CTRL_REG           (0x0004)
#define ENTRY_MODE_REG          (0x0005)
#define COMPARE_REG             (0x0006)
#define DISP_CTRL_REG           (0x0007)

#define FRAME_CYC_CTRL_REG      (0x000B)
#define PWR3_CTRL_REG           (0x000C)
#define PWR4_CTRL_REG           (0x000D)
#define PWR5_CTRL_REG           (0x000E)
#define GATE_SCAN_POS_REG       (0x000F)

#define V_SCROLL_CTRL_REG       (0x0011)

#define DISPLAY1_DRV_POS_REG    (0x0014)
#define DISPLAY2_DRV_POS_REG    (0x0015)
#define H_RAM_ADDR_POS_REG      (0x0016)
#define V_RAM_ADDR_POS_REG      (0x0017)

#define RAM_WR_DATA_MASK_REG    (0x0020)
#define RAM_ADDR_SET_REG        (0x0021)
#define RAM_WR_DATA_REG         (0x0022)
#define RAM_RD_DATA_REG         (0x0022)

#define G_CTRL1_REG             (0x0030)
#define G_CTRL2_REG             (0x0031)
#define G_CTRL3_REG             (0x0032)
#define G_CTRL4_REG             (0x0033)
#define G_CTRL5_REG             (0x0034)
#define G_CTRL6_REG             (0x0035)
#define G_CTRL7_REG             (0x0036)
#define G_CTRL8_REG             (0x0037)
#define G_CTRL9_REG             (0x003A)
#define G_CTRL10_REG            (0x003B)



struct HD66773R_SERIALBUS_CONFIG
{
    GPIO_FLAG EnablePin;  // HD66773R_LCD_ON_PORT, 1 );
    GPIO_FLAG ResetPin;   // HD66773R_LRST_PORT  , 0 );
    GPIO_FLAG LoadPin;    // HD66773R_LOAD_PORT  , 1 );
    GPIO_FLAG SclPin;     // HD66773R_SCL_PORT   , 0 );
    GPIO_FLAG SdaPin;     // HD66773R_SDA_PORT   , 0 );

    UINT32 StartupTime;   // #define HD66773R_TIME_POWER_STABLE     10000
    UINT32 SdaSetupTime;  // #define HD66773R_TIME_SDA_SETUP        2          // Time after presenting data until rising clock edge.
    UINT32 SdaHoldTime;   // #define HD66773R_TIME_SDA_HOLD         2          // Time after rising clock edge to data change.
    UINT32 CmdholdTime;   // #define HD66773R_TIME_MIN_CMD_HOLD_OFF 2          // Minimum spacing between commands on serial bus.
    UINT32 LoadSetupTime; // #define HD66773R_TIME_LOAD_SETUP       2          // Time between asserting LOAD and first bit.
    UINT32 LoadHoldTime;  // #define HD66773R_TIME_LOAD_HOLD        2
};


struct HD66773R_Driver
{
    UINT32 m_cursor;

    static UINT16 VRAM_Buff[(DISP_YMAX* DISP_XMAX )] ;

    static BOOL Initialize();
    static BOOL Uninitialize();
    static void PowerSave( BOOL On );
    static void Clear();
    static void BitBltEx( int x, int y, int width, int height, UINT32 data[] );
    static void BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta );
    static void WriteChar( unsigned char c, int row, int col );
    static void WriteFormattedChar( unsigned char c );
    static UINT16* GetScreenBuffer();
    static void Data_Trans (UINT16 *data);  

private:
    static void WriteRawByte( UINT8 data );
    static void WriteCmdByte( UINT8 addr, UINT8 data );
    static UINT32 PixelsPerWord();
    static UINT32 TextRows();
    static UINT32 TextColumns();
    static UINT32 WidthInWords();
    static UINT32 SizeInWords();
    static UINT32 SizeInBytes();

    static void Delay_MS (UINT16 time);
    static void Reg_Write (UINT16 reg_addr, UINT16 data);
    static UINT16 Reg_Read (UINT16 reg_addr);
    static void Set_Display_On( void );
    static void Set_Display_Off (void);
    static INT32 Full_Write (UINT16 *lcd_top_addr, UINT16 color);

    
};

extern HD66773R_Driver g_HD66773R_Driver;
extern DISPLAY_CONTROLLER_CONFIG g_HD66773R_Config;

#endif  // _DRIVERS_DISPLAY_HD66773R_H_
