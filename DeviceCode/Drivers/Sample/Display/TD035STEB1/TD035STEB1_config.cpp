////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\Display\TD035STEB1\TD035STEB1.h>

//////////////////////////////////////////////////////////////////////////////

#define TD035STEB1_SCREEN_WIDTH            240
#define TD035STEB1_SCREEN_HEIGHT           320
#define TD035STEB1_BITS_PER_PIXEL          16
#define TD035STEB1_ORIENTATION             0

#define TD035STEB1_VLCD_ENABLE           GPIO_PIN_NONE  /* 1 = 5V on. First on, First off */
#define TD035STEB1_VEE_ENABLE            GPIO_PIN_NONE	/* 1 = VEE & VDD on. Second on, second off */
#define TD035STEB1_BKLT_ENABLE           GPIO_PIN_NONE	/* 1 = backlight on */

#define TD035STEB1_TIME_POWER_STABLE     10000
#define TD035STEB1_TIME_SDA_SETUP        2          // Time after presenting data until rising clock edge.
#define TD035STEB1_TIME_SDA_HOLD         2          // Time after rising clock edge to data change.
#define TD035STEB1_TIME_MIN_CMD_HOLD_OFF 2          // Minimum spacing between commands on serial bus.
#define TD035STEB1_TIME_LOAD_SETUP       2          // Time between asserting LOAD and first bit.
#define TD035STEB1_TIME_LOAD_HOLD        2          // Time after last bit and unasserting LOAD.


#define TD035STEB1_ENABLE_TFT              TRUE
#define TD035STEB1_ENABLE_COLOR            TRUE
#define TD035STEB1_PIXEL_POLARITY          TRUE
#define TD035STEB1_FIRST_LINE_POLARITY     FALSE
#define TD035STEB1_LINE_PULSE_POLARITY     FALSE
#define TD035STEB1_SHIFT_CLK_POLARITY      FALSE
#define TD035STEB1_OUTPUT_ENABLE_POLARITY  TRUE
#define TD035STEB1_CLK_IDLE_ENABLE         TRUE
#define TD035STEB1_CLK_SELECT_ENABLE       TRUE

#define TD035STEB1_PIXELCLOCKDIVIDER     (15 - 1)    // Div by 15 (96/15 = 6.4 Mhz)
#define TD035STEB1_BUS_WIDTH             16
        // The values entered below were copied from the uMon code
        // The values below could not be calculated from the TD035STEB1 spec.  How they arrived at these
        // numbers, I have no idea.
#define TD035STEB1_HWIDTH                62
#define TD035STEB1_HWAIT1                18
#define TD035STEB1_HWAIT2                57
#define TD035STEB1_VWIDTH                1
#define TD035STEB1_VWAIT1                0
#define TD035STEB1_VWAIT2                3

//////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_TD035STEB1_Config"
#endif

TD035STEB1_CONFIG g_TD035STEB1_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;
    {
        { TD035STEB1_VLCD_ENABLE, TRUE }, // GPIO_FLAG LcdEnable;
        { TD035STEB1_VEE_ENABLE , TRUE }, // GPIO_FLAG VeeEnable;
        { TD035STEB1_BKLT_ENABLE, TRUE }, // GPIO_FLAG BacklightEnable;
    },
    {
        TD035STEB1_SCREEN_WIDTH,           // UINT32 Width;
        TD035STEB1_SCREEN_HEIGHT,          // UINT32 Height;
        TD035STEB1_ENABLE_TFT,             // BOOL EnableTFT;
        TD035STEB1_ENABLE_COLOR,           // BOOL EnableColor;
        TD035STEB1_PIXEL_POLARITY,         // BOOL PixelPolarity;           (TRUE == high)
        TD035STEB1_FIRST_LINE_POLARITY,    // BOOL FirstLineMarkerPolarity; (FALSE == low)
        TD035STEB1_LINE_PULSE_POLARITY,    // BOOL LinePulsePolarity;
        TD035STEB1_SHIFT_CLK_POLARITY,     // BOOL ShiftClockPolarity;
        TD035STEB1_OUTPUT_ENABLE_POLARITY, // BOOL OutputEnablePolarity;
        TD035STEB1_CLK_IDLE_ENABLE,        // BOOL ClockIdleEnable;
        TD035STEB1_CLK_SELECT_ENABLE,      // BOOL ClockSelectEnable;

        TD035STEB1_PIXELCLOCKDIVIDER, // UINT32 PixelClockDivider;
        TD035STEB1_BUS_WIDTH,         // UINT32 BusWidth;
        TD035STEB1_BITS_PER_PIXEL,    // UINT32 BitsPerPixel;
        TD035STEB1_ORIENTATION,       // UINT8 Orientation;

        TD035STEB1_HWIDTH,  // UINT32 HorizontalSyncPulseWidth;
        TD035STEB1_HWAIT1,  // UINT32 HorizontalSyncWait1;
        TD035STEB1_HWAIT2,  // UINT32 HorizontalSyncWait2;
        TD035STEB1_VWIDTH,  // UINT32 VerticalSyncPulseWidth;
        TD035STEB1_VWAIT1,  // UINT32 VerticalSyncWait1;
        TD035STEB1_VWAIT2,  // UINT32 VerticalSyncWait2;
    }
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

