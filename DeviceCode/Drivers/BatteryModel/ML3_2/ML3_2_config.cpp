////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
// some curve fits of this battery discharge vs. voltage/temp

// X is range as (MilliVolts - MinVoltage) / VoltageToX, and below MinVoltage, 0% state of charge
// the upper limit is then X=Max
// this allows the coefficients to fit into the poly evaluator

/*
ML2-2
0.2C    Temp    2:20C 4:-20C 6:-10C 8:0C 10:20C 12:45C 14:60C
*/

const STATE_OF_CHARGE_SINGLE_CURVE_FIT g_ML3_2_DATA[] =
{
    // -10C
    // y = +2.7890485959798700E-13x6
    //     -2.0956947363191700E-10x5
    //     +5.4461489906385000E-08x4
    //     -6.1933537611746900E-06x3
    //     +3.3622239230624000E-04x2
    //     -4.7008632542429000E-03x
    //     +2.4562226285070200E-02
    {
        -100,                       // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3102,                       // UINT16  MinVoltage;
        183,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1769894930317310LL,
             -338732896002048LL,
               24227376651264LL,
                -446278171056LL,
                   3924363930LL,
                    -15101073LL,
                        20097LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 0C
    //     +3.1626572057575700E-02
    //     -5.2554356941527700E-03x
    //     +2.4437958340684000E-04x2
    //     -3.1826711255345000E-06x3
    //     +1.9083498115080000E-08x4
    //     -3.7730538359176000E-11x5
    // y = -1.1882681440768000E-14x6
    {
        0,                          // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3103,                       // UINT16  MinVoltage;
        204,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             2278934690136070LL,
             -378694051741696LL,
               17609404812288LL,
                -229335623920LL,
                   1375110960LL,
                     -2718772LL,
                         -857LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 10C
    //     +2.5540646460285600E-02
    //     -4.2251045684906800E-03x
    //     +1.8535649547146700E-04x2
    //     -2.0785142011980900E-06x3
    //     +9.9456818752896600E-09x4
    //     -6.2554708896817800E-12x5
    // y = -4.4980618487687000E-14x6
    {
        100,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3103,                       // UINT16  MinVoltage;
        206,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1840397534101500LL,
             -304450869764096LL,
               13356343102976LL,
                -149772732512LL,
                    716661907LL,
                      -450755LL,
                        -3242LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 20C
    //     +2.9662054174878000E-02
    //     -4.8406669864107200E-03x
    //     +2.1837861616091900E-04x2
    //     -2.8425882370175500E-06x3
    //     +1.8009553960120200E-08x4
    //     -4.5897204626162600E-11x5
    // y = +2.8489493639054700E-14x6
    {
        200,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3108,                       // UINT16  MinVoltage;
        203,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             2137376258064380LL,
             -348806816579584LL,
               15735837669888LL,
                -204830069200LL,
                   1297725128LL,
                     -3307243LL,
                         2052LL,
                            0LL,
                            0LL,
                            0LL,

        }
    },
    // 30C
    //     +2.5581926981431000E-02
    //     -4.0370463752879000E-03x
    //     +1.7598964914356000E-04x2
    //     -1.9385437023400300E-06x3
    //     +8.9218178409358800E-09x4
    //     -3.6011225502485900E-12x5
    // y = -4.4906923325788400E-14x6
    {
        300,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3103,                       // UINT16  MinVoltage;
        204,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1843372109135870LL,
             -290899848822784LL,
               12681390692864LL,
                -139686795128LL,
                    642884728LL,
                      -259489LL,
                        -3236LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
};


STATE_OF_CHARGE_CURVE_FIT g_BATTERY_MEASUREMENT_CurveFit =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    ARRAYSIZE(g_ML3_2_DATA),
    g_ML3_2_DATA,
};

