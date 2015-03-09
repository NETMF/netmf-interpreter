////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
//--//

#if !defined(DRIVER_PAL_BUTTON_MAPPING)
#define DRIVER_PAL_BUTTON_MAPPING  \
    {GPIO_PIN_NONE,BUTTON_NONE}, \
    {GPIO_PIN_NONE,BUTTON_NONE}, \
    {GPIO_PIN_NONE,BUTTON_NONE}, \
    {GPIO_PIN_NONE,BUTTON_NONE}, \
    {GPIO_PIN_NONE,BUTTON_NONE}, \
    {GPIO_PIN_NONE,BUTTON_NONE}, 
#endif


//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_GPIO_BUTTON_Config"
#endif

GPIO_BUTTON_CONFIG g_GPIO_BUTTON_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//
    { // GPIO_HW_TO_HAL_MAPPING Mapping[c_COUNT];
        DRIVER_PAL_BUTTON_MAPPING
    },
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif


