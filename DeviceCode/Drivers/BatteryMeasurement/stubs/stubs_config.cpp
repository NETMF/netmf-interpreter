////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>

//--//

#define BATTERY_LIFE_MIN          10
#define BATTERY_LIFE_LOW          20
#define BATTERY_LIFE_MED          30
#define BATTERY_LIFE_MAX          98

#define BATTERY_LIFE_FULLMIN      40
#define BATTERY_LIFE_HYSTERESIS    6

#define BATTERY_TIMEOUT_CHARGER    5
#define BATTERY_TIMEOUT_BACKLIGHT  5

#define ZEROC_KELVINS_X100          27315

#define BATTERY_TIMEOUT_CHARGING   120        
#define BATTERY_TIMEOUT_CHARGED    60         
#define CHRG_CURRENT_MA            200        


//--//

#define THERMISTOR_R_NOMINAL        0
#define THERMISTOR_T_NOMINAL_X100   0
#define THERMISTOR_B_VAL            0


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_STUB_BATTERY_Config"
#endif

BATTERY_COMMON_CONFIG g_STUB_BATTERY_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    BATTERY_LIFE_MIN,           // UINT8  Battery_Life_Min;
    BATTERY_LIFE_LOW,           // UINT8  Battery_Life_Low;
    BATTERY_LIFE_MED,           // UINT8  Battery_Life_Med;
    BATTERY_LIFE_MAX,           // UINT8  Battery_Life_Max;
                                //
    BATTERY_LIFE_FULLMIN,       // UINT8  Battery_Life_FullMin;
    BATTERY_LIFE_HYSTERESIS,    // UINT8  Battery_Life_Hysteresis;
                                //
    BATTERY_TIMEOUT_CHARGING,   // UINT8  Battery_Timeout_Charging;
    BATTERY_TIMEOUT_CHARGED,    // UINT8  Battery_Timeout_Charged;
    BATTERY_TIMEOUT_CHARGER,    // UINT16 Battery_Timeout_Charger;
    BATTERY_TIMEOUT_BACKLIGHT,  // UINT16 Battery_Timeout_Backlight;
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

 
