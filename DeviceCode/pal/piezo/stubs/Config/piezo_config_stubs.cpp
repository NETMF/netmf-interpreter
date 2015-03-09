////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <PAL\Piezo\Piezo.h>

//--//

PIEZO_CONFIG g_PiezoStub_Config =
{
    { TRUE },          // HAL_DRIVER_CONFIG_HEADER::Enable

    {   // PWM_CONFIG[2]
        { 
            GPIO_PIN_NONE, // PWM_Output pin
            FALSE,         // PWM Disabled state
            0,             // VTU_Channel
            0              // Prescaler
        },

        { 
            GPIO_PIN_NONE, // PWM_Output pin
            FALSE,         // PWM Disabled state
            0,             // VTU_Channel
            0              // Prescaler
        }
    }
};


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_pPiezo_Config"
#endif

PIEZO_CONFIG* const g_pPIEZO_Config = &g_PiezoStub_Config;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata
#endif
