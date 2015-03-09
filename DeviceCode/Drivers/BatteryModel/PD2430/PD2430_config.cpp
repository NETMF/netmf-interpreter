////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
// some curve fits of this battery discharge vs. voltage/temp

// X is range as (MilliVolts - MinVoltage) / VoltageToX, and below MinVoltage, 0% state of charge
// the upper limit is then X=Max
// this allows the coefficients to fit into the poly evaluator

const STATE_OF_CHARGE_SINGLE_CURVE_FIT g_PD2430_DATA[] =
{
    // -10C
    // y =  -1.620225907181290000E-14x6 
    //      +5.302689240836550000E-11x5 
    //      -2.477542248614790000E-08x4 
    //      +4.087872997604600000E-06x3 
    //      -2.304139127851100000E-04x2 
    //      +6.383284752871530000E-03x 
    //      -2.350309843041030000E-02
    {
        -100,                       // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3102,                       // UINT16  MinVoltage;
        199,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
            -1693576725331970LL,
              459964141350912LL,
              -16603072188160LL,
                 294562292940LL,
                  -1785257336LL,
                      3820990LL,
                        -1168LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 0C
    // y =  +4.940261308989030000E-13x6 
    //      -2.456761176805140000E-10x5 
    //      +3.634600659243450000E-08x4 
    //      -7.782121413946190000E-07x3 
    //      -1.291095317341730000E-04x2 
    //      +6.243316100267290000E-03x 
    //      -3.920463739314070000E-02
    {
        0,                          // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3091,                       // UINT16  MinVoltage;
        210,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
            -2824991845679100LL,
              449878337003520LL,
               -9303322224129LL,
                 -56076094560LL,
                   2619005787LL,
                    -17702830LL,
                        35598LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 10C
    // y =  +8.441347745234730000E-13x6 
    //      -4.750787703364970000E-10x5 
    //      +9.235709310870250000E-08x4 
    //      -6.974657246028300000E-06x3 
    //      +1.721367034122070000E-04x2 
    //      +5.571416600105290000E-04x 
    //      -1.554921953265880000E-02
    {
        100,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3102,                       // UINT16  MinVoltage;
        211,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
            -1120439348690950LL,
               40146287558656LL,
               12403756693504LL,
                -502577020388LL,
                   6655029921LL,
                    -34233034LL,
                        60826LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 20C
    // y =  +9.387529582168810000E-13x6 
    //      -5.654366746956490000E-10x5 
    //      +1.214261294733500000E-07x4 
    //      -1.096462312855100000E-05x3 
    //      +4.007004384725120000E-04x2 
    //      -4.082458726543340000E-03x 
    //      -3.434695468058640000E-04
    {
        200,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3091,                       // UINT16  MinVoltage;
        215,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
              -24749589168128LL,
             -294172153593856LL,
               28873509526272LL,
                -790084362177LL,
                   8749674743LL,
                    -40744007LL,
                        67644LL,
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

    ARRAYSIZE(g_PD2430_DATA),
    g_PD2430_DATA,
};


