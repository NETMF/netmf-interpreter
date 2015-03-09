////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\Display\TD022SHEB2\TD022SHEB2.h>

//////////////////////////////////////////////////////////////////////////////

#define TD022SHEB2_SCREEN_WIDTH            176
#define TD022SHEB2_SCREEN_HEIGHT           220
#define TD022SHEB2_BITS_PER_PIXEL          16
#define TD022SHEB2_ORIENTATION             0


#define TD022SHEB2_LCD_ON_PORT           GPIO_PIN_NONE
#define TD022SHEB2_LRST_PORT             GPIO_PIN_NONE
#define TD022SHEB2_BKL_ON_PORT           GPIO_PIN_NONE
#define TD022SHEB2_LOAD_PORT             GPIO_PIN_NONE
#define TD022SHEB2_SCL_PORT              GPIO_PIN_NONE
#define TD022SHEB2_SDA_PORT              GPIO_PIN_NONE

#define TD022SHEB2_TIME_POWER_STABLE     10000
#define TD022SHEB2_TIME_SDA_SETUP        2          // Time after presenting data until rising clock edge.
#define TD022SHEB2_TIME_SDA_HOLD         2          // Time after rising clock edge to data change.
#define TD022SHEB2_TIME_MIN_CMD_HOLD_OFF 2          // Minimum spacing between commands on serial bus.
#define TD022SHEB2_TIME_LOAD_SETUP       2          // Time between asserting LOAD and first bit.
#define TD022SHEB2_TIME_LOAD_HOLD        2          // Time after last bit and unasserting LOAD.

#define TD022SHEB2_ENABLE_TFT              TRUE
#define TD022SHEB2_ENABLE_COLOR            TRUE
#define TD022SHEB2_PIXEL_POLARITY          TRUE
#define TD022SHEB2_FIRST_LINE_POLARITY     FALSE
#define TD022SHEB2_LINE_PULSE_POLARITY     FALSE
#define TD022SHEB2_SHIFT_CLK_POLARITY      FALSE
#define TD022SHEB2_OUTPUT_ENABLE_POLARITY  TRUE
#define TD022SHEB2_CLK_IDLE_ENABLE         TRUE
#define TD022SHEB2_CLK_SELECT_ENABLE       TRUE

#define TD022SHEB2_PIXELCLOCKDIVIDER       (29 - 1)    // Div by 19 (96/19 = 5.05 Mhz)
#define TD022SHEB2_BUS_WIDTH               16
#define TD022SHEB2_HWIDTH                  7
#define TD022SHEB2_HWAIT1                  43
#define TD022SHEB2_HWAIT2                  9
#define TD022SHEB2_VWIDTH                  2
#define TD022SHEB2_VWAIT1                  3
#define TD022SHEB2_VWAIT2                  5

//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_TD022SHEB2_Config"
#endif

TD022SHEB2_CONFIG g_TD022SHEB2_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;
    { TD022SHEB2_BKL_ON_PORT, TRUE },
    {
        { TD022SHEB2_LCD_ON_PORT, TRUE },
        { TD022SHEB2_LRST_PORT  , TRUE },
        { TD022SHEB2_LOAD_PORT  , TRUE },
        { TD022SHEB2_SCL_PORT   , TRUE },
        { TD022SHEB2_SDA_PORT   , TRUE },
            
        TD022SHEB2_TIME_POWER_STABLE     ,
        TD022SHEB2_TIME_SDA_SETUP        ,
        TD022SHEB2_TIME_SDA_HOLD         ,
        TD022SHEB2_TIME_MIN_CMD_HOLD_OFF ,
        TD022SHEB2_TIME_LOAD_SETUP       ,
        TD022SHEB2_TIME_LOAD_HOLD        ,         
    },
    {
        TD022SHEB2_SCREEN_WIDTH,           // UINT32 Width;
        TD022SHEB2_SCREEN_HEIGHT,          // UINT32 Height;
        TD022SHEB2_ENABLE_TFT,             // BOOL EnableTFT;
        TD022SHEB2_ENABLE_COLOR,           // BOOL EnableColor;
        TD022SHEB2_PIXEL_POLARITY,         // BOOL PixelPolarity;           (TRUE == high)
        TD022SHEB2_FIRST_LINE_POLARITY,    // BOOL FirstLineMarkerPolarity; (FALSE == low)
        TD022SHEB2_LINE_PULSE_POLARITY,    // BOOL LinePulsePolarity;
        TD022SHEB2_SHIFT_CLK_POLARITY,     // BOOL ShiftClockPolarity;
        TD022SHEB2_OUTPUT_ENABLE_POLARITY, // BOOL OutputEnablePolarity;
        TD022SHEB2_CLK_IDLE_ENABLE,        // BOOL ClockIdleEnable;
        TD022SHEB2_CLK_SELECT_ENABLE,      // BOOL ClockSelectEnable;

        TD022SHEB2_PIXELCLOCKDIVIDER, // UINT32 PixelClockDivider;
        TD022SHEB2_BUS_WIDTH,         // UINT32 BusWidth;
        TD022SHEB2_BITS_PER_PIXEL,    // UINT32 BitsPerPixel;
        TD022SHEB2_ORIENTATION,       // UINT8 Orientation;

        TD022SHEB2_HWIDTH,  // UINT32 HorizontalSyncPulseWidth;
        TD022SHEB2_HWAIT1,  // UINT32 HorizontalSyncWait1;
        TD022SHEB2_HWAIT2,  // UINT32 HorizontalSyncWait2;
        TD022SHEB2_VWIDTH,  // UINT32 VerticalSyncPulseWidth;
        TD022SHEB2_VWAIT1,  // UINT32 VerticalSyncWait1;
        TD022SHEB2_VWAIT2,  // UINT32 VerticalSyncWait2;
    }
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

