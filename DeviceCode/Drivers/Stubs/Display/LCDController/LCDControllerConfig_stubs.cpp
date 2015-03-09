////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//////////////////////////////////////////////////////////////////////////////

#define STUB_SCREEN_WIDTH            320
#define STUB_SCREEN_HEIGHT           240
#define STUB_BITS_PER_PIXEL          16
#define STUB_ORIENTATION             0

#define STUB_ENABLE_TFT              TRUE
#define STUB_ENABLE_COLOR            TRUE
#define STUB_PIXEL_POLARITY          TRUE
#define STUB_FIRST_LINE_POLARITY     FALSE
#define STUB_LINE_PULSE_POLARITY     FALSE
#define STUB_SHIFT_CLK_POLARITY      TRUE
#define STUB_OUTPUT_ENABLE_POLARITY  TRUE
#define STUB_CLK_IDLE_ENABLE         TRUE
#define STUB_CLK_SELECT_ENABLE       TRUE

#define STUB_PIXELCLOCKDIVIDER       (19 - 1)    // Div by 19 (96/19 = 5.05 Mhz)
#define STUB_BUS_WIDTH               16
#define STUB_HWIDTH                  0
#define STUB_HWAIT1                  8
#define STUB_HWAIT2                  57
#define STUB_VWIDTH                  1
#define STUB_VWAIT1                  2
#define STUB_VWAIT2                  20


//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_LcdController_Config"
#endif

DISPLAY_CONTROLLER_CONFIG g_LcdController_Config =
{
        STUB_SCREEN_WIDTH,           // UINT32 Width;
        STUB_SCREEN_HEIGHT,          // UINT32 Height;
        STUB_ENABLE_TFT,             // BOOL EnableTFT;
        STUB_ENABLE_COLOR,           // BOOL EnableColor;
        STUB_PIXEL_POLARITY,         // BOOL PixelPolarity;           (TRUE == high)
        STUB_FIRST_LINE_POLARITY,    // BOOL FirstLineMarkerPolarity; (FALSE == low)
        STUB_LINE_PULSE_POLARITY,    // BOOL LinePulsePolarity;
        STUB_SHIFT_CLK_POLARITY,     // BOOL ShiftClockPolarity;
        STUB_OUTPUT_ENABLE_POLARITY, // BOOL OutputEnablePolarity;
        STUB_CLK_IDLE_ENABLE,        // BOOL ClockIdleEnable;
        STUB_CLK_SELECT_ENABLE,      // BOOL ClockSelectEnable;

        STUB_PIXELCLOCKDIVIDER, // UINT32 PixelClockDivider;
        STUB_BUS_WIDTH,         // UINT32 BusWidth;
        STUB_BITS_PER_PIXEL,    // UINT32 BitsPerPixel;
        STUB_ORIENTATION,       // UINT8 Orientation;
                                    
        STUB_HWIDTH,  // UINT32 HorizontalSyncPulseWidth;
        STUB_HWAIT1,  // UINT32 HorizontalSyncWait1;
        STUB_HWAIT2,  // UINT32 HorizontalSyncWait2;
        STUB_VWIDTH,  // UINT32 VerticalSyncPulseWidth;
        STUB_VWAIT1,  // UINT32 VerticalSyncWait1;
        STUB_VWAIT2,  // UINT32 VerticalSyncWait2;
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

