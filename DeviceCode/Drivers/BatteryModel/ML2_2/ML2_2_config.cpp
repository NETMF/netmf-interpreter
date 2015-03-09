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

const STATE_OF_CHARGE_SINGLE_CURVE_FIT g_ML2_2_DATA[] =
{
    // -20C (temp code 4)
    // y = +3.228018008701300000E-13x6
    //     -8.468460459616540000E-11x5
    //     -5.285055716848510000E-09x4
    //     +2.593171286341400000E-06x3
    //     -1.491846038019770000E-04x2
    //     +6.846342278763020000E-03x
    //     -1.162911808921760000E-02
    {
        -200,                       // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3101,                       // UINT16  MinVoltage;
        148,                        // UINT16  MaxX;
        4,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             -837966270291966LL,
              493330952567808LL,
              -10749883617472LL,
                 186857683822LL,
                   -380828400LL,
                     -6102169LL,
                        23260LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // -10C (temp code 6)
    // y = +5.265831082000790000E-13x6
    //     -3.032794977271000000E-10x5
    //     +6.036440610606220000E-08x4
    //     -4.988599520028280000E-06x3
    //     +1.892991296195130000E-04x2
    //     -4.235845494804380000E-05x
    //     +7.192602702389190000E-03
    {
        -100,                       // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3104,                       // UINT16  MinVoltage;
        185,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
              518281645604864LL,
               -3052248350720LL,
               13640439833856LL,
                -359466479032LL,
                   4349713869LL,
                    -21853591LL,
                        37944LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 0C (temp code 8)
    // y = +2.402626777518820000E-13x6
    //     -1.770806299226150000E-10x5
    //     +4.516794614934160000E-08x4
    //     -4.913819713070480000E-06x3
    //     +2.472209541970470000E-04x2
    //     -2.814036480742740000E-03x
    //     +1.327680238432550000E-02
    {
        0,                          // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3100,                       // UINT16  MinVoltage;
        202,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
              956694436331521LL,
             -202772698337280LL,
               17814147155200LL,
                -354078026060LL,
                   3254693527LL,
                    -12760005LL,
                        17312LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 20C (temp code 2)
    // y = +1.979178431115580000E-13x6
    //     -1.606332455796700000E-10x5
    //     +4.546125689092180000E-08x4
    //     -5.618922975680900000E-06x3
    //     +3.299071788873680000E-04x2
    //     -6.404656143672580000E-03x
    //     +2.894454058241540000E-02
    {
        200,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3121,                       // UINT16  MinVoltage;
        208,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             2085673954902020LL,
             -461504112353280LL,
               23772317566464LL,
                -404886070712LL,
                   3275828793LL,
                    -11574846LL,
                        14261LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 45C (temp code 12)
    // y = -4.529983649241380000E-14x6
    //     -2.085145183822410000E-12x5
    //     +7.826204324358490000E-09x4
    //     -1.622510165444350000E-06x3
    //     +1.406750366044210000E-04x2
    //     -3.252793356750770000E-03x
    //     +1.894375182382650000E-02

    {
        450,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3107,                       // UINT16  MinVoltage;
        212,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1365041178476550LL,
             -234388463190016LL,
               10136704678912LL,
                -116914178824LL,
                    563937454LL,
                      -150251LL,
                        -3265LL,
                            0LL,
                            0LL,
                            0LL,

        }
    },
    // 60C (temp code 14)
    // y = -7.285517686112140000E-14x6
    //     +1.442117441224430000E-11x5
    //     +4.213793876328700000E-09x4
    //     -1.279266538034560000E-06x3
    //     +1.275957827786560000E-04x2
    //     -3.023948610234580000E-03x
    //     +1.928295519519450000E-02
    {
        600,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3117,                       // UINT16  MinVoltage;
        209,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1389483357306880LL,
             -217898461347840LL,
                9194245116416LL,
                 -92180868864LL,
                    303635848LL,
                      1039155LL,
                        -5250LL,
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

    ARRAYSIZE(g_ML2_2_DATA),
    g_ML2_2_DATA,
};



