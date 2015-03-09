////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\Backlight\GPIO\gpio_backlight.h>

//--//

#define BACKLIGHT_H_GPIO_PIN                GPIO_PIN_NONE

//--//


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_BackLight_Config"
#endif

OUTPUT_GPIO_CONFIG g_BackLight_Config =
{
    { TRUE  }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    { BACKLIGHT_H_GPIO_PIN, TRUE }, // GPIO_FLAG Output_GPIO_PIN;
    0,                              // INT32     RefCount;
    0                               // BOOL      On;
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

