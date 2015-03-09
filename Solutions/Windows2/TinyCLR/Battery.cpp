////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

//--//
static BATTERY_COMMON_CONFIG BatteryConfig;

using namespace Microsoft::SPOT::Emulator;

BOOL Battery_Initialize()
{
    return EmulatorNative::GetIBatteryDriver()->Initialize();        
}

BOOL Battery_Uninitialize()
{
    return EmulatorNative::GetIBatteryDriver()->Uninitialize();
}

BOOL Battery_Voltage( INT32& Millivolts )
{
    return EmulatorNative::GetIBatteryDriver()->Voltage( Millivolts );    
}

BOOL Battery_Temperature( INT32& DegreesCelcius_x10 )
{
    return EmulatorNative::GetIBatteryDriver()->Temperature( DegreesCelcius_x10 );
}

void Battery_VoltageFilter_Reset()
{
    _ASSERTE(FALSE);
}

BOOL Battery_StateOfCharge( UINT8& StateOfCharge )
{
    return EmulatorNative::GetIBatteryDriver()->StateOfCharge( StateOfCharge ); 
}

BATTERY_COMMON_CONFIG* Battery_Configuration()
{
    BatteryConfiguration managedConfig = EmulatorNative::GetIBatteryDriver()->Configuration;

    BatteryConfig.Battery_Life_Min = managedConfig.BatteryLifeMin;
    BatteryConfig.Battery_Life_Low = managedConfig.BatteryLifeLow;
    BatteryConfig.Battery_Life_Med = managedConfig.BatteryLifeMed;
    BatteryConfig.Battery_Life_Max = managedConfig.BatteryLifeMax;
    BatteryConfig.Battery_Life_FullMin = managedConfig.BatteryLifeFullMin;
    BatteryConfig.Battery_Life_Hysteresis = managedConfig.BatteryLifeHysteresis;

    BatteryConfig.Battery_Timeout_Charging = managedConfig.TimeoutCharging;
    BatteryConfig.Battery_Timeout_Charged = managedConfig.TimeoutCharged;
    BatteryConfig.Battery_Timeout_Charger = managedConfig.TimeoutCharger;
    BatteryConfig.Battery_Timeout_Backlight = managedConfig.TimeoutBacklight;

    return &BatteryConfig;
}
