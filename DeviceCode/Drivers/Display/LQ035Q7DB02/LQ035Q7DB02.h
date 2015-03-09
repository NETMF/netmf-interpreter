////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h> 
#include <tinyhal.h> 

#ifndef _DRIVERS_DISPLAY_LQ035Q7DB02_H_
#define _DRIVERS_DISPLAY_LQ035Q7DB02_H_ 1

//--//

struct LQ035Q7DB02_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    GPIO_FLAG EnablePin;

    DISPLAY_CONTROLLER_CONFIG ControllerConfig;

    static LPCSTR GetDriverName() { return "LQ035Q7DB02"; }
};

extern LQ035Q7DB02_CONFIG g_LQ035Q7DB02_Config;

//--//

struct LQ035Q7DB02_Driver
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

extern LQ035Q7DB02_Driver g_LQ035Q7DB02_Driver;


#endif  // _DRIVERS_DISPLAY_LQ035Q7DB02_H_
