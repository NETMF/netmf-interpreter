////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\BatteryCharger\Charger_DualStatus\charger_dualstatus.h>

//--//

#define MAX_SYSTEM_CURRENT_LOAD_MA      80  // in mA

#define CHRG_CURRENT_MA                             200         

#define CHRG_SHDN_OUT_GPIO_PIN                      GPIO_PIN_NONE
#define CHRG_SHDN_OUT_ACTIVE_STATE                  FALSE
#define CHRG_SHDN_OUT_ACTIVE_DRIVEN                 TRUE
#define CHRG_SHDN_OUT_INACTIVE_DRIVEN               FALSE
#define CHRG_SHDN_OUT_ACTIVE_UNDRIVEN_RESISTOR      RESISTOR_DISABLED
#define CHRG_SHDN_OUT_INACTIVE_UNDRIVEN_RESISTOR    RESISTOR_DISABLED
#define CHRG_SHDN_LOW_POWER_STATE                   TRUE
#define CHRG_FAST_H_OUT_GPIO_PIN                    GPIO_PIN_NONE
#define CHRG_ACPR_L_IN_GPIO_PIN                     GPIO_PIN_NONE
#define CHRG_ACPR_L_IN_RESISTOR                     RESISTOR_DISABLED
#define CHRG_ACPR_L_ACTIVE_STATE                    FALSE
#define CHRG_STAT1_L_IN_GPIO_PIN                    GPIO_PIN_NONE
#define CHRG_STAT2_L_IN_GPIO_PIN                    GPIO_PIN_NONE
#define CHRG_STAT1_RESISTOR                         RESISTOR_PULLUP
#define CHRG_STAT2_RESISTOR                         RESISTOR_PULLUP
#define OVER_TEMPERATURE_SHUTDOWN                   450         // hot, hot, hot: (LTC4053 kicks in at 50, so use 45 for testing sw feature)
#define UNDER_TEMPERATURE_SHUTDOWN                  0
#define CHARGER_DUALSTATUS_MAP                      LTC4053_DUALSTATUS_MAP

//--//


/*
    struct DECODED_STATUS {
        BOOL m_charging;
        BOOL m_fault;
        BOOL m_chargeComplete;
    };
*/

/*
    CHARGE STATE                    CHRG    FAULT
    charge in progress              ON      OFF
    Charge done                     OFF     OFF
    Charge suspend (temperature)    ON      ON
    Timer fault ( < 2.48V )         OFF     ON
    (*) OFF means the open-drain output transistor on the STAT1 and STAT2 pins is in an off state.
*/
// STAT1    STAT2
// OFF      OFF
// OFF      ON
// ON       OFF
// ON       ON
#define LTC4053_DUALSTATUS_MAP  \
{                               \
    { FALSE, FALSE, TRUE  },    \
    { FALSE, TRUE , FALSE },    \
    { TRUE , FALSE, FALSE },    \
    { FALSE, TRUE , FALSE },    \
}

/*
        Table 1. Status Pins Summary
    CHARGE STATE                    STAT1   STAT2
    Precharge in progress           ON      ON
    Fast charge in progress         ON      OFF
    Charge done                     OFF     ON
    Charge suspend (temperature)    OFF     OFF
    Timer fault                     OFF     OFF
    Sleep mode                      OFF     OFF
    (*) OFF means the open-drain output transistor on the STAT1 and STAT2 pins is in an off state.
*/
// STAT1    STAT2
// OFF      OFF
// OFF      ON
// ON       OFF
// ON       ON
#define BQ24022_DUALSTATUS_MAP  \
{                               \
    { FALSE, TRUE , FALSE },    \
    { FALSE, FALSE, TRUE  },    \
    { TRUE , FALSE, FALSE },    \
    { TRUE , FALSE, FALSE },    \
}

// one status
#define BQ24200_DUALSTATUS_MAP  \
{                               \
    { FALSE, FALSE, TRUE  },    \
    { FALSE, FALSE, TRUE  },    \
    { TRUE , FALSE, FALSE },    \
    { TRUE , FALSE, FALSE },    \
}

// LM 3658
/*
    Stat1   stat2
    Off     off     power down chargeing suspend
    On      off     pre-qualifing mode, cc/ CV charging
    off     on      charge is completed
    on      on      bad battery or LDO mode
*/

#define LM3658_DUALSTATUS_MAP   \
{                               \
    { FALSE, TRUE,  FALSE },    \
    { FALSE, FALSE, TRUE  },    \
    { TRUE , FALSE, FALSE },    \
    { FALSE, TRUE,  FALSE },    \
}


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_DualStatus_Config"
#endif

CHARGER_DUALSTATUS_CONFIG g_DualStatus_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    { CHRG_SHDN_OUT_GPIO_PIN   , CHRG_SHDN_OUT_ACTIVE_STATE }, // GPIO_FLAG      Shutdown_GPIO_PIN;
    { CHRG_FAST_H_OUT_GPIO_PIN , TRUE  },                      // GPIO_FLAG      FastCharge_GPIO_PIN;
    { CHRG_ACPR_L_IN_GPIO_PIN  , CHRG_ACPR_L_ACTIVE_STATE   }, // GPIO_FLAG      ACPower_GPIO_PIN;
    CHRG_ACPR_L_IN_RESISTOR,                                   // GPIO_RESISTOR  ACPower_Resistor;
    { CHRG_STAT1_L_IN_GPIO_PIN , FALSE },                      // GPIO_FLAG      Stat1_GPIO_PIN;
    { CHRG_STAT2_L_IN_GPIO_PIN , FALSE },                      // GPIO_FLAG      Stat2_GPIO_PIN;
    CHRG_STAT1_RESISTOR,                                       // GPIO_RESISTOR  Stat1_Resistor;
    CHRG_STAT2_RESISTOR,                                       // GPIO_RESISTOR  Stat2_Resistor;
    OVER_TEMPERATURE_SHUTDOWN,                                 // INT16          Over_Temperature_Shutdown;
    UNDER_TEMPERATURE_SHUTDOWN,                                // INT16          Under_Temperature_Shutdown;
    CHRG_SHDN_OUT_ACTIVE_DRIVEN,                               // BOOL           Shutdown_ActiveStateDriven;
    CHRG_SHDN_OUT_INACTIVE_DRIVEN,                             // BOOL           Shutdown_InactiveStateDriven;
    CHRG_SHDN_OUT_ACTIVE_UNDRIVEN_RESISTOR,                    // GPIO_RESISTOR  Shutdown_ActiveUndriven_Resistor;
    CHRG_SHDN_OUT_INACTIVE_UNDRIVEN_RESISTOR,                  // GPIO_RESISTOR  Shutdown_InactiveUndriven_Resistor;
    CHARGER_DUALSTATUS_MAP,                                    // DECODED_STATUS StatusDecodes[4];
    100000,                                                    // UINT32         ACPower_On_Debounce_uSec;
    100000,                                                    // UINT32         Status_Change_Debounce_uSec;
    CHRG_SHDN_LOW_POWER_STATE,                                 // UINT8          Shutdown_LowPowerState;
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

