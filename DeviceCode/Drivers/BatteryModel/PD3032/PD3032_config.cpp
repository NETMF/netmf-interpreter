////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
// some curve fits of this battery discharge vs. voltage/temp

// X is range as (MilliVolts - MinVoltage) / VoltageToX, and below MinVoltage, 0% state of charge
// the upper limit is then X=Max
// this allows the coefficients to fit into the poly evaluator

const STATE_OF_CHARGE_SINGLE_CURVE_FIT g_PD3032_DATA[] =
{
    // -10C
    // y = +8.986950530979070000E-13x6
    //     -5.431904580589090000E-10x5
    //     +1.176866198425790000E-07x4
    //     -1.093049630612740000E-05x3
    //     +4.389056324978210000E-04x2
    //     -5.908306986555090000E-03x
    //     +1.442404987642480000E-02
    {
        -100,                       // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        55,                         // INT8    CoefficientsScalerBits;
        3089,                       // UINT16  MinVoltage;
        212,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
              519681165189121LL,
             -212869193144320LL,
               15813241943744LL,
                -393812632730LL,
                   4240107338LL,
                    -19570499LL,
                        32378LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 0C
    // y = +1.158834284064330000E-12x6
    //     -7.230676382772080000E-10x5
    //     +1.640161139868610000E-07x4
    //     -1.631688112996300000E-05x3
    //     +7.107354033948350000E-04x2
    //     -1.104999429986720000E-02x
    //     +3.523820403074750000E-02

    {
        0,                          // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        54,                         // INT8    CoefficientsScalerBits;
        3091,                       // UINT16  MinVoltage;
        213,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
              634795050168320LL,
             -199059000845312LL,
               12803470791552LL,
                -293938799107LL,
                   2954651639LL,
                    -13025629LL,
                        20875LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 10C
    // y = +1.058348972587790000E-12x6
    //     -6.832656928920240000E-10x5
    //     +1.611950232400030000E-07x4
    //     -1.681818630555610000E-05x3
    //     +7.750146182328170000E-04x2
    //     -1.293333781802630000E-02x
    //     +4.109396047624610000E-02

    {
        100,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        54,                         // INT8    CoefficientsScalerBits;
        3083,                       // UINT16  MinVoltage;
        217,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
              740282980351999LL,
             -232986301511680LL,
               13961422183520LL,
                -302969510315LL,
                   2903831386LL,
                    -12308621LL,
                        19065LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 20C
    // y = +7.913457983294310000E-13x6
    //     -5.104418870140750000E-10x5
    //     +1.192674192415120000E-07x4
    //     -1.214484692491080000E-05x3
    //     +5.406881519842700000E-04x2
    //     -8.501402668656510000E-03x
    //     +2.507717515970850000E-02

    {
        200,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        54,                         // INT8    CoefficientsScalerBits;
        3097,                       // UINT16  MinVoltage;
        217,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
              451750226819071LL,
             -153147655562752LL,
                9740171839200LL,
                -218782112342LL,
                   2148530819LL,
                     -9195304LL,
                        14255LL,
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

    ARRAYSIZE(g_PD3032_DATA),
    g_PD3032_DATA,
};


