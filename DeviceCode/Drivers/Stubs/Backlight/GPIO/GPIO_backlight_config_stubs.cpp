////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <Drivers\Backlight\GPIO\gpio_backlight.h>

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_BackLight_Config"
#endif

OUTPUT_GPIO_CONFIG g_BackLight_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//
    { 
        GPIO_PIN_NONE, // Output GPIO pin
        FALSE,      // Active Stae
    },

    0,              // Refcount
    TRUE,           // State (default on)
};


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif
