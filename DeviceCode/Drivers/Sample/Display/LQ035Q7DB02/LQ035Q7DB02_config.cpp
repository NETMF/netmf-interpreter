////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\Display\LQ035Q7DB02\LQ035Q7DB02.h>

//--//

#define LQ035Q7DB02_SCREEN_WIDTH            240
#define LQ035Q7DB02_SCREEN_HEIGHT           320
#define LQ035Q7DB02_BITS_PER_PIXEL          16
#define LQ035Q7DB02_ORIENTATION             0

#define LQ035Q7DB02_ENABLE_TFT              TRUE
#define LQ035Q7DB02_ENABLE_COLOR            TRUE
#define LQ035Q7DB02_PIXEL_POLARITY          TRUE
#define LQ035Q7DB02_FIRST_LINE_POLARITY     FALSE
#define LQ035Q7DB02_LINE_PULSE_POLARITY     FALSE
#define LQ035Q7DB02_SHIFT_CLK_POLARITY      TRUE
#define LQ035Q7DB02_OUTPUT_ENABLE_POLARITY  TRUE
#define LQ035Q7DB02_CLK_IDLE_ENABLE         TRUE
#define LQ035Q7DB02_CLK_SELECT_ENABLE       TRUE

#define LQ035Q7DB02_PIXELCLOCKDIVIDER       (16 - 1)    // Div by 19 (96/19 = 5.05 Mhz)
#define LQ035Q7DB02_BUS_WIDTH               16
#define LQ035Q7DB02_HWIDTH                  1
#define LQ035Q7DB02_HWAIT1                  0
#define LQ035Q7DB02_HWAIT2                  61
#define LQ035Q7DB02_VWIDTH                  2
#define LQ035Q7DB02_VWAIT1                  1
#define LQ035Q7DB02_VWAIT2                  6

#define LQ035Q7DB02_ENABLE_PIN              GPIO_PIN_NONE

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_LQ035Q7DB02_Config"
#endif

LQ035Q7DB02_CONFIG g_LQ035Q7DB02_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;
    { LQ035Q7DB02_ENABLE_PIN, TRUE },       //GPIO_PIN Enablepin
    {
        LQ035Q7DB02_SCREEN_WIDTH,           // UINT32 Width;
        LQ035Q7DB02_SCREEN_HEIGHT,          // UINT32 Height;
        LQ035Q7DB02_ENABLE_TFT,             // BOOL EnableTFT;
        LQ035Q7DB02_ENABLE_COLOR,           // BOOL EnableColor;
        LQ035Q7DB02_PIXEL_POLARITY,         // BOOL PixelPolarity;           (TRUE == high)
        LQ035Q7DB02_FIRST_LINE_POLARITY,    // BOOL FirstLineMarkerPolarity; (FALSE == low)
        LQ035Q7DB02_LINE_PULSE_POLARITY,    // BOOL LinePulsePolarity;
        LQ035Q7DB02_SHIFT_CLK_POLARITY,     // BOOL ShiftClockPolarity;
        LQ035Q7DB02_OUTPUT_ENABLE_POLARITY, // BOOL OutputEnablePolarity;
        LQ035Q7DB02_CLK_IDLE_ENABLE,        // BOOL ClockIdleEnable;
        LQ035Q7DB02_CLK_SELECT_ENABLE,      // BOOL ClockSelectEnable;

        LQ035Q7DB02_PIXELCLOCKDIVIDER, // UINT32 PixelClockDivider;
        LQ035Q7DB02_BUS_WIDTH,         // UINT32 BusWidth;
        LQ035Q7DB02_BITS_PER_PIXEL,    // UINT32 BitsPerPixel;
        LQ035Q7DB02_ORIENTATION,       // UINT8 Orientation;

        LQ035Q7DB02_HWIDTH,  // UINT32 HorizontalSyncPulseWidth;
        LQ035Q7DB02_HWAIT1,  // UINT32 HorizontalSyncWait1;
        LQ035Q7DB02_HWAIT2,  // UINT32 HorizontalSyncWait2;
        LQ035Q7DB02_VWIDTH,  // UINT32 VerticalSyncPulseWidth;
        LQ035Q7DB02_VWAIT1,  // UINT32 VerticalSyncWait1;
        LQ035Q7DB02_VWAIT2,  // UINT32 VerticalSyncWait2;
    }
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

