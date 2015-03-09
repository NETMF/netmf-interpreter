////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
// some curve fits of this battery discharge vs. voltage/temp

// X is range as (MilliVolts - MinVoltage) / VoltageToX, and below MinVoltage, 0% state of charge
// the upper limit is then X=Max
// this allows the coefficients to fit into the poly evaluator

const STATE_OF_CHARGE_SINGLE_CURVE_FIT g_ML1A_2_CURVEFIT_DATA[] =
{
    // 0C
    // y = +3.277533310408630000E-13x6
    //     -2.307492713496120000E-10x5
    //     +5.869503602118700000E-08x4
    //     -6.705251069161200000E-06x3
    //     +3.689173874192870000E-04x2
    //     -5.973381977753430000E-03x
    //     +2.812116045515720000E-02
    {
        0,                          // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3102,                       // UINT16  MinVoltage;
        202,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             2026343163953150LL,
             -430427533586432LL,
               26583299336192LL,
                -483164259464LL,
                   4229423077LL,
                    -16627238LL,
                        23617LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 10C

    // y = +3.943288201686050000E-14x6
    //     -6.548374638219200000E-11x5
    //     +2.454237054121820000E-08x4
    //     -3.624653436840040000E-06x3
    //     +2.537104163593540000E-04x2
    //     -5.095895506883610000E-03x
    //     +2.629240607893730000E-02

    {
        100,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3116,                       // UINT16  MinVoltage;
        205,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1894567523516410LL,
             -367197969694720LL,
               18281762185216LL,
                -261183805880LL,
                   1768464173LL,
                     -4718602LL,
                         2841LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 20C
    // y = -4.381236841221900000E-14x6
    //     -6.865540135779200000E-12x5
    //     +1.012704964331590000E-08x4
    //     -2.088601063710000000E-06x3
    //     +1.825895213904970000E-04x2
    //     -4.166807303363380000E-03x
    //     +2.371864070664740000E-02

    {
        200,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3105,                       // UINT16  MinVoltage;
        213,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1709108183171070LL,
             -300250109100032LL,
               13156961607936LL,
                -150499567556LL,
                    729730832LL,
                      -494715LL,
                        -3158LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 30C
    // y = -8.076076685592080000E-14x6
    //     +1.766976897847500000E-11x5
    //     +3.902485990608940000E-09x4
    //     -1.329619430223690000E-06x3
    //     +1.373885127335940000E-04x2
    //     -3.090322003913570000E-03x
    //     +1.847653810727930000E-02

    {
        300,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3119,                       // UINT16  MinVoltage;
        207,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1331374882160640LL,
             -222681168404480LL,
                9899885676032LL,
                 -95809177128LL,
                    281203751LL,
                      1273241LL,
                        -5820LL,
                            0LL,
                            0LL,
                            0LL,
        }
    }
};

STATE_OF_CHARGE_CURVE_FIT g_BATTERY_MEASUREMENT_CurveFit =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    ARRAYSIZE(g_ML1A_2_CURVEFIT_DATA),
    g_ML1A_2_CURVEFIT_DATA,
};


