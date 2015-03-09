////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
// some curve fits of this battery discharge vs. voltage/temp

// X is range as (MilliVolts - MinVoltage) / VoltageToX, and below MinVoltage, 0% state of charge
// the upper limit is then X=Max
// this allows the coefficients to fit into the poly evaluator

const STATE_OF_CHARGE_SINGLE_CURVE_FIT g_IML200425_2_DATA[] =
{
    // -10C
    // y = - 2.921848049979300000E-02
    //     + 8.782307948443700000E-03x
    //     - 2.333808204045830000E-04x2
    //     + 3.856237912002600000E-06x3
    //     - 2.214094332551320000E-08x4
    //     + 4.117594055691850000E-11x5
    {
        -100,                       // INT16   Temperature;
        6,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3105,                       // UINT16  MinVoltage;
        210,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
            -2105413406259200LL,
              632831980865024LL,
              -16816860412952LL,
                 277871225976LL,
                  -1595423106LL,
                      2967039LL,
                            0LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 0C
    // y = + 2.047522162115460000E-02
    //     - 4.275968405409000000E-03x
    //     + 3.272098642863600000E-04x2
    //     - 6.454859570126370000E-06x3
    //     + 6.073919354325840000E-08x4
    //     - 2.543781333968020000E-10x5
    //     + 3.846397268909590000E-13x6
    {
        0,                          // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3101,                       // UINT16  MinVoltage;
        209,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1475395207413760LL,
             -308115995475968LL,
               23577955565952LL,
                -465121650476LL,
                   4376720150LL,
                    -18329877LL,
                        27716LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 10C

    // y = + 2.411092941110840000E-02
    //     - 4.693360087401290000E-03x
    //     + 2.525245543836260000E-04x2
    //     - 4.017030204306330000E-06x3
    //     + 3.082229605788980000E-08x4
    //     - 1.006704638813020000E-10x5
    //     + 1.071946769614940000E-13x6

    {
        100,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3094,                       // UINT16  MinVoltage;
        209,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1737375563382790LL,
             -338192235851776LL,
               18196311824384LL,
                -289457531700LL,
                   2220980496LL,
                     -7254072LL,
                         7724LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 20C
    // y = + 3.337666242396150000E-02
    //     - 6.122761342908230000E-03x
    //     + 2.778481003815610000E-04x2
    //     - 3.921589108946130000E-06x3
    //     + 2.666577843908270000E-08x4
    //     - 7.656661606751900000E-11x5
    //     + 6.677988717433050000E-14x6

    {
        200,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3095,                       // UINT16  MinVoltage;
        209,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             2405041991286780LL,
             -441191451238400LL,
               20021065621504LL,
                -282580275996LL,
                   1921471837LL,
                     -5517207LL,
                         4811LL,
                            0LL,
                            0LL,
                            0LL,
        }
    },
    // 30C
    // y = + 2.283453822883530000E-02
    //     - 3.790126855165000000E-03x
    //     + 1.648931592157510000E-04x2
    //     - 1.697413976042840000E-06x3
    //     + 5.926739318935780000E-09x4
    //     + 1.259643704311840000E-11x5
    //     - 7.520650690460260000E-14x6

    {
        300,                        // INT16   Temperature;
        7,                          // INT8    NumCoefficients;
        56,                         // INT8    CoefficientsScalerBits;
        3095,                       // UINT16  MinVoltage;
        209,                        // UINT16  MaxX;
        5,                          // UINT16  VoltageToX;
        {                           // INT64   Coefficients[10];
             1645401885736960LL,
             -273107422281728LL,
               11881804326400LL,
                -122311567200LL,
                    427066575LL,
                       907668LL,
                        -5420LL,
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

    ARRAYSIZE(g_IML200425_2_DATA),
    g_IML200425_2_DATA,
};


