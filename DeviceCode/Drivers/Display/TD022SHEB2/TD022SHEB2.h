////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

#ifndef _DRIVERS_DISPLAY_TD022SHEB2_H_
#define _DRIVERS_DISPLAY_TD022SHEB2_H_ 1

//////////////////////////////////////////////////////////////////////////////

struct TD022SHEB2_SERIALBUS_CONFIG
{
    GPIO_FLAG EnablePin;  // TD022SHEB2_LCD_ON_PORT, 1 );
    GPIO_FLAG ResetPin;   // TD022SHEB2_LRST_PORT  , 0 );
    GPIO_FLAG LoadPin;    // TD022SHEB2_LOAD_PORT  , 1 );
    GPIO_FLAG SclPin;     // TD022SHEB2_SCL_PORT   , 0 );
    GPIO_FLAG SdaPin;     // TD022SHEB2_SDA_PORT   , 0 );

    UINT32 StartupTime;   // #define TD022SHEB2_TIME_POWER_STABLE     10000
    UINT32 SdaSetupTime;  // #define TD022SHEB2_TIME_SDA_SETUP        2          // Time after presenting data until rising clock edge.
    UINT32 SdaHoldTime;   // #define TD022SHEB2_TIME_SDA_HOLD         2          // Time after rising clock edge to data change.
    UINT32 CmdholdTime;   // #define TD022SHEB2_TIME_MIN_CMD_HOLD_OFF 2          // Minimum spacing between commands on serial bus.
    UINT32 LoadSetupTime; // #define TD022SHEB2_TIME_LOAD_SETUP       2          // Time between asserting LOAD and first bit.
    UINT32 LoadHoldTime;  // #define TD022SHEB2_TIME_LOAD_HOLD        2

};

struct TD022SHEB2_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    GPIO_FLAG BacklightPin;

    TD022SHEB2_SERIALBUS_CONFIG SBConfig;

    DISPLAY_CONTROLLER_CONFIG ControllerConfig;

    static LPCSTR GetDriverName() { return "TD022SHEB2"; }
};

extern TD022SHEB2_CONFIG g_TD022SHEB2_Config;

//////////////////////////////////////////////////////////////////////////////

struct TD022SHEB2_Driver
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
    static void WriteRawByte( UINT8 data );

    static void WriteCmdByte( UINT8 addr, UINT8 data );

    static UINT32 PixelsPerWord();
    static UINT32 TextRows();
    static UINT32 TextColumns();
    static UINT32 WidthInWords();
    static UINT32 SizeInWords();
    static UINT32 SizeInBytes();
};

extern TD022SHEB2_Driver g_TD022SHEB2_Driver;


#endif  // _DRIVERS_DISPLAY_TD022SHEB2_H_
