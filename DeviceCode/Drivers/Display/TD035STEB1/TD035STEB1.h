////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

#ifndef _DRIVERS_DISPLAY_TD035STEB1_H_
#define _DRIVERS_DISPLAY_TD035STEB1_H_ 1

//////////////////////////////////////////////////////////////////////////////
struct TD035STEB1_LCD_CONFIG
{
    GPIO_FLAG LcdEnable;
    GPIO_FLAG VeeEnable;
    GPIO_FLAG BacklightEnable;
};

struct TD035STEB1_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    TD035STEB1_LCD_CONFIG LcdConfig;

    DISPLAY_CONTROLLER_CONFIG ControllerConfig;

    static LPCSTR GetDriverName() { return "TD035STEB1"; }
};

extern TD035STEB1_CONFIG g_TD035STEB1_Config;

//////////////////////////////////////////////////////////////////////////////

struct TD035STEB1_Driver
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

extern TD035STEB1_Driver g_TD035STEB1_Driver;


#endif  // _DRIVERS_DISPLAY_TD035STEB1_H_
