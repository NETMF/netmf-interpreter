////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>
   
////////////////////////////////////////////////////////////////////////////////////////////////////

BOOL Battery_Initialize()
{
    return TRUE;
}

BOOL Battery_Uninitialize()
{
    return TRUE;
}

BOOL Battery_Voltage( INT32& Millivolts )
{
    Millivolts = 3700;

    return TRUE;
}

extern void Charger_SetTemperature( INT16 DegreesCelcius_x10 );
    
BOOL Battery_Temperature( INT32& DegreesCelcius_x10 )
{
    DegreesCelcius_x10 = 250;

    // everytime we read the temperature (app controlled), we update the charger info too
    // do this for stub, to clear initial overtemp condition
    Charger_SetTemperature( 250 );

    return TRUE;
}

void Battery_VoltageFilter_Reset()
{
}

INT32 Battery_MostRecent_DegreesCelcius_x10()
{
    return 250;
}

INT32 Battery_MostRecent_Millivolts()
{
    return 3700;
}

BOOL Battery_StateOfCharge( UINT8& StateOfCharge )
{
    StateOfCharge = 75;

    return TRUE;
}

void Battery_StateOfCharge( INT32 Voltage, INT32 DegreesCelcius_x10, UINT8& StateOfCharge )
{
    StateOfCharge = 75;
}

BATTERY_COMMON_CONFIG* Battery_Configuration()
{
    return NULL;
}


