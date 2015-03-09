////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//


BOOL           SOC_OffChargerReading;
HAL_COMPLETION SOC_OffChargerReadADC;
UINT8          Last_StateOfCharge;
BOOL           SOC_OffChargerCompletionInitialized = FALSE;

void Battery_StateOfCharge( INT32 Voltage, INT32 DegreesCelcius_x10, UINT8& StateOfCharge )
{
    STATE_OF_CHARGE_CURVE_FIT* CurveFit = &g_BATTERY_MEASUREMENT_CurveFit;

    ASSERT(CurveFit->SingleCurve   );
    ASSERT(CurveFit->NumCurves >= 2);

    const STATE_OF_CHARGE_SINGLE_CURVE_FIT* Table = CurveFit->SingleCurve;

    //
    // Assume table temperatures are always increasing
    //
    // We walk the table short two positions ("i > 2"), so Table[0] and Table[1] are always valid.
    //
    for(int i = CurveFit->NumCurves; i > 2; i--, Table++)
    {
        if(Table[1].Temperature > DegreesCelcius_x10)
        {
            break;
        }
    }

    //--//

    INT32 SOC_Left  = Battery_CurveFitter( Table[0], Voltage );
    INT32 SOC_Right = Battery_CurveFitter( Table[1], Voltage );

    INT32 Temp_Left  = Table[0].Temperature;
    INT32 Temp_Right = Table[1].Temperature;

    StateOfCharge = SOC_Left + (((SOC_Right - SOC_Left) * (DegreesCelcius_x10 - Temp_Left)) / (Temp_Right - Temp_Left));

    //printf( "Lp:%4d Rp:%4d Ls:%3d Rs:%3d s:%3d\r\n", Temp_Left, Temp_Right, SOC_Left, SOC_Right, *StateOfCharge );

    Last_StateOfCharge = StateOfCharge;

    //printf( "SOC=%3d\r\n", Last_StateOfCharge );
}

BOOL Battery_StateOfCharge_Direct( UINT8& StateOfCharge )
{
    INT32 Voltage;
    INT32 DegreesCelcius_x10;

    if(Battery_Voltage    ( Voltage            ) == FALSE) return FALSE;
    if(Battery_Temperature( DegreesCelcius_x10 ) == FALSE) return FALSE;

    Battery_StateOfCharge( Voltage, DegreesCelcius_x10, StateOfCharge );

    return TRUE;
}

static void Battery_StateOfCharge_Delay( void* arg )
{
    UINT8 StateOfCharge;

    Battery_StateOfCharge_Direct( StateOfCharge );

    Charger_Restart( CHARGER_SHUTDOWN_ADC );

    SOC_OffChargerReading = FALSE;
}

BOOL Battery_StateOfCharge( UINT8& StateOfCharge )
{
    if(SOC_OffChargerReading == FALSE) // No delay reading pending.
    {
        UINT32 Status;

        Charger_Status( Status );

        if((Status & CHARGER_STATUS_ON_AC_POWER) == 0) // Not on AC power, we can read the ADC immediately.
        {
            return Battery_StateOfCharge_Direct( StateOfCharge );
        }

        // On AC power, so we need to turn off the charger and do a delayed reading.
        Charger_Shutdown( CHARGER_SHUTDOWN_ADC );

        if(Last_StateOfCharge == 0) // First invocation, we cannot delay the reading, we need a value now.
        {
            Events_WaitForEvents( 0, 120 );

            Battery_StateOfCharge_Delay( 0 );
        }
        else
        {
            SOC_OffChargerReading = TRUE;

            if( !SOC_OffChargerCompletionInitialized )
            {
                SOC_OffChargerCompletionInitialized = TRUE;
                SOC_OffChargerReadADC.InitializeForUserMode( Battery_StateOfCharge_Delay );
            }

            SOC_OffChargerReadADC.EnqueueDelta( 120000 );
        }
    }

    StateOfCharge = Last_StateOfCharge;

    return TRUE;
}

//--//

UINT32 Battery_CurveFitter( const STATE_OF_CHARGE_SINGLE_CURVE_FIT& SingleCurve, INT32 Voltage )
{
    INT64 X;
    INT64 Y;

    // Adjust to usable range of battery voltage, approximate 0-110 for X
    X = (Voltage - SingleCurve.MinVoltage) / SingleCurve.VoltageToX;

    //printf("X:%3lld\r\n", X);

    if(X > 0)
    {
        if(X < SingleCurve.MaxX)
        {
            Y = poly_eval( SingleCurve.NumCoefficients, SingleCurve.Coefficients, X );

            //printf( "Y:%3lld\r\n", Y );

            Y *= 100;   // scale to percent

            // remove scaling from table coefficients
            Y >>= (SingleCurve.CoefficientsScalerBits - 1);
            Y  += 1; // for rounding
            Y >>= 1; // for rounding

            if(Y > 100)
            {
                //ASSERT(0);
                Y = 100;
            }
            else if(Y < 0)
            {
                //ASSERT(0);
                Y = 0;
            }
        }
        else
        {
            // full scale at and above 1050+3100=4150mV
            Y = 100;
        }
    }
    else
    {
        Y = 0;
    }

    //DEBUG_TRACE2( 0, "X,Y=%lld,%lld\r\n", X, Y );

    return Y;
}

INT64 poly_eval( INT32 Power, const INT64* Coefficients, INT64 X_eval )
{
    INT64 X = 1;
    INT64 Y = 0;

    while(Power-- > 0)
    {
        INT64 C = *Coefficients++;

        Y += (C * X);

        //DEBUG_TRACE3( 0, "0x%016llx 0x%016llx 0x%016llx\r\n", C, X, Y );

        // tail, next power
        X *= X_eval;
    }

    return Y;
}

//--//

