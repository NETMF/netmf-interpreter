////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\Display\TX09D71VM1CCA\TX09D71VM1CCA.h>

//////////////////////////////////////////////////////////////////////////////

#define TX09D71VM1CCA_VLCD_ENABLE                       AT91_GPIO_Driver::PA12

#define TX09D71VM1CCA_SCREEN_WIDTH              240
#define TX09D71VM1CCA_SCREEN_HEIGHT             320
#define TX09D71VM1CCA_ENABLE_TFT                    TRUE
#define TX09D71VM1CCA_ENABLE_COLOR              TRUE
#define TX09D71VM1CCA_PIXEL_POLARITY                FALSE
#define TX09D71VM1CCA_FIRST_LINE_POLARITY       FALSE
#define TX09D71VM1CCA_LINE_PULSE_POLARITY       FALSE
#define TX09D71VM1CCA_SHIFT_CLK_POLARITY            FALSE
#define TX09D71VM1CCA_OUTPUT_ENABLE_POLARITY    FALSE
#define TX09D71VM1CCA_CLK_IDLE_ENABLE           TRUE
#define TX09D71VM1CCA_CLK_SELECT_ENABLE         TRUE

#define TX09D71VM1CCA_PIXELCLOCKDIVIDER     9
#define TX09D71VM1CCA_BUS_WIDTH             16
#define TX09D71VM1CCA_BITS_PER_PIXEL        16
#define TX09D71VM1CCA_ORIENTATION           0
        // The values entered below were copied from the uMon code
        // The values below could not be calculated from the TX09D71VM1CCA spec.  How they arrived at these
        // numbers, I have no idea.
#define TX09D71VM1CCA_HWIDTH                5
#define TX09D71VM1CCA_HWAIT1                1
#define TX09D71VM1CCA_HWAIT2                33
#define TX09D71VM1CCA_VWIDTH                1
#define TX09D71VM1CCA_VWAIT1                1
#define TX09D71VM1CCA_VWAIT2                0

//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_TX09D71VM1CCA_Config"
#endif

TX09D71VM1CCA_CONFIG g_TX09D71VM1CCA_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;
    {
        { TX09D71VM1CCA_VLCD_ENABLE, TRUE }, // GPIO_FLAG LcdEnable;
    },
    {
        TX09D71VM1CCA_SCREEN_WIDTH,           // UINT32 Width;
        TX09D71VM1CCA_SCREEN_HEIGHT,          // UINT32 Height;
        TX09D71VM1CCA_ENABLE_TFT,             // BOOL EnableTFT;
        TX09D71VM1CCA_ENABLE_COLOR,           // BOOL EnableColor;
        TX09D71VM1CCA_PIXEL_POLARITY,         // BOOL PixelPolarity;           (TRUE == high)
        TX09D71VM1CCA_FIRST_LINE_POLARITY,    // BOOL FirstLineMarkerPolarity; (FALSE == low)
        TX09D71VM1CCA_LINE_PULSE_POLARITY,    // BOOL LinePulsePolarity;
        TX09D71VM1CCA_SHIFT_CLK_POLARITY,     // BOOL ShiftClockPolarity;
        TX09D71VM1CCA_OUTPUT_ENABLE_POLARITY, // BOOL OutputEnablePolarity;
        TX09D71VM1CCA_CLK_IDLE_ENABLE,        // BOOL ClockIdleEnable;
        TX09D71VM1CCA_CLK_SELECT_ENABLE,      // BOOL ClockSelectEnable;

        TX09D71VM1CCA_PIXELCLOCKDIVIDER, // UINT32 PixelClockDivider;
        TX09D71VM1CCA_BUS_WIDTH,         // UINT32 BusWidth;
        TX09D71VM1CCA_BITS_PER_PIXEL,    // UINT32 BitsPerPixel;
        TX09D71VM1CCA_ORIENTATION,       // UINT8 Orientation;

        TX09D71VM1CCA_HWIDTH,  // UINT32 HorizontalSyncPulseWidth;
        TX09D71VM1CCA_HWAIT1,  // UINT32 HorizontalSyncWait1;
        TX09D71VM1CCA_HWAIT2,  // UINT32 HorizontalSyncWait2;
        TX09D71VM1CCA_VWIDTH,  // UINT32 VerticalSyncPulseWidth;
        TX09D71VM1CCA_VWAIT1,  // UINT32 VerticalSyncWait1;
        TX09D71VM1CCA_VWAIT2,  // UINT32 VerticalSyncWait2;
    }
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

