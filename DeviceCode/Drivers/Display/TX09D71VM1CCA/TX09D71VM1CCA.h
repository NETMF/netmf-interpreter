////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

#ifndef _DRIVERS_DISPLAY_TX09D71VM1CCA_H_
#define _DRIVERS_DISPLAY_TX09D71VM1CCA_H_ 1

//////////////////////////////////////////////////////////////////////////////
struct TX09D71VM1CCA_LCD_CONFIG
{
    GPIO_FLAG LcdEnable;
};

struct TX09D71VM1CCA_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    TX09D71VM1CCA_LCD_CONFIG LcdConfig;

    DISPLAY_CONTROLLER_CONFIG ControllerConfig;

    static LPCSTR GetDriverName() { return "TX09D71VM1CCA"; }
};

extern TX09D71VM1CCA_CONFIG g_TX09D71VM1CCA_Config;

//////////////////////////////////////////////////////////////////////////////

struct TX09D71VM1CCA_Driver
{
    //--//

    UINT32 m_cursor;

    //--//
    
    static BOOL Initialize();

    static BOOL Uninitialize();

    static void PowerSave( BOOL On );

    static void Clear();

    static void BitBltEx( int x, int y, int width, int height, UINT32 data[] );

    static void BitBlt( int width, int height, int widthInWords, UINT32 data[], BOOL fUseDelta );

    static void WriteChar         ( unsigned char c, int row, int col );
    
    static void WriteFormattedChar( unsigned char c                   );

private:
    static UINT32 PixelsPerWord();
    static UINT32 TextRows();
    static UINT32 TextColumns();
    static UINT32 WidthInWords();
    static UINT32 SizeInWords();
    static UINT32 SizeInBytes();
};

extern TX09D71VM1CCA_Driver g_TX09D71VM1CCA_Driver;


#endif  // _DRIVERS_DISPLAY_TX09D71VM1CCA_H_
