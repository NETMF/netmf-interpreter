////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_BATTERY_MEASUREMENT_DECL_H_
#define _DRIVERS_BATTERY_MEASUREMENT_H_ 1

struct STATE_OF_CHARGE_SINGLE_CURVE_FIT
{
    INT16   Temperature;
    INT8    NumCoefficients;
    INT8    CoefficientsScalerBits;
    UINT16  MinVoltage;
    UINT16  MaxX;
    UINT16  VoltageToX;
    INT64   Coefficients[10];
};

struct STATE_OF_CHARGE_CURVE_FIT
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    INT32                                   NumCurves;
    const STATE_OF_CHARGE_SINGLE_CURVE_FIT* SingleCurve;

    //--//

    static LPCSTR GetDriverName() { return "STATE_OF_CHARGE_CURVE"; }
};

//--//

struct BATTERY_COMMON_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    UINT8  Battery_Life_Min;
    UINT8  Battery_Life_Low;
    UINT8  Battery_Life_Med;
    UINT8  Battery_Life_Max;
    //
    UINT8  Battery_Life_FullMin;
    UINT8  Battery_Life_Hysteresis;
    //
    UINT8  Battery_Timeout_Charging;  // Maximum time to spend at a certain charge level before displaying the "Charged" popup.
    UINT8  Battery_Timeout_Charged;   // Delay after the battery reaches MAX before displaying the "Charged" popup.
    UINT16 Battery_Timeout_Charger;   // Minimum time to ignore glitches on the Charger status.
    UINT16 Battery_Timeout_Backlight; // Time to leave backlight on when placed on the charger.
};

//--//

BOOL Battery_Initialize  ();
BOOL Battery_Uninitialize();

BOOL Battery_Voltage    ( INT32& Millivolts         );
BOOL Battery_Temperature( INT32& DegreesCelcius_x10 );

void Battery_VoltageFilter_Reset();

INT32 Battery_MostRecent_DegreesCelcius_x10();
INT32 Battery_MostRecent_Millivolts();

BOOL Battery_StateOfCharge( UINT8& StateOfCharge );

BATTERY_COMMON_CONFIG* Battery_Configuration();

//--//

extern INT32 Thermistor_ResistanceToTemperature( UINT32 R_Thermistor, UINT32 R_Nominal, INT32 T_Nominal_x100, INT32 B_VAL );
extern UINT32 Battery_CurveFitter( const STATE_OF_CHARGE_SINGLE_CURVE_FIT& SingleCurve, INT32 Voltage );
extern INT64 poly_eval( INT32 Power, const INT64* Coefficients, INT64 X_eval );
extern BOOL Battery_StateOfCharge( UINT8& StateOfCharge );
extern void Battery_StateOfCharge( INT32 Voltage, INT32 DegreesCelcius_x10, UINT8& StateOfCharge );

//--//

extern STATE_OF_CHARGE_CURVE_FIT g_BATTERY_MEASUREMENT_CurveFit;

#endif // _DRIVERS_BATTERY_MEASUREMENT_DECL_H_

