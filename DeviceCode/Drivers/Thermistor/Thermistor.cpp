////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

#ifndef ZEROC_KELVINS_X100
#define ZEROC_KELVINS_X100                          27315
#endif


INT32 Thermistor_ResistanceToTemperature( UINT32 R_Thermistor, UINT32 R_Nominal, INT32 T_Nominal_x100, INT32 B_VAL )
{
    ////////////////////////////////////////////////////////////////////////////////////
    //
    // here we use the form:  Rt = Rn * e^(B * (1/T - 1/Tn))
    //
    // which inverts (with a little algebra) to:
    //
    // T = (Tn * log2(e) * B) / ( (Tn * log2(Rt)) - (Tn * log2(Rn)) + (log2(e) * B) )
    //
    // if we replace log2 with our log2_x256, then we work nicely in integer math
    // across the whole usable range.  See attached excel... oops, not this decade.
    //
    // T = (Tn * log2_x256(e) * B) / ( (Tn * log2_x256(Rt)) - (Tn * log2_x256(Rn)) + (log2_x256(e) * B) )
    //
    // T is in Kelvins, 0 Celcius = 273.15 Kevlins
    //
    ////////////////////////////////////////////////////////////////////////////////////
    //
    // log2_x256(e) * 1024 = 378193.84879879642345896009557656
    const INT32 log2_e_x256_x1024 = 378194;

    //--//

    INT32 Temperature_x10;
    INT64 Numerator;
    INT64 Denominator;
    INT64 DenominatorN;

    // (Tn * log2_x256(e) * B) * 1024 * 100 * 100
    Numerator  = T_Nominal_x100;
    Numerator *= log2_e_x256_x1024;
    Numerator *= B_VAL;
    Numerator *= 100;   // extra 100 for overscaling result
    // numerator has a total extra scaling of x1024 and x100, and x100 for 10x final temp scaling of 10x

    // + (Tn * log2_x256(Rt)) * 1024 * 100
    Denominator   = T_Nominal_x100;
    Denominator  *= ilog64_x256( (UINT64)R_Thermistor );
    Denominator  *= 1024;

    // - (Tn * log2_x256(Rn)) * 1024 * 100
    DenominatorN  = T_Nominal_x100;
    DenominatorN *= ilog64_x256( (UINT64)R_Nominal );
    DenominatorN *= 1024;
    Denominator  -= DenominatorN;

    // + (log2_x256(e) * B)   * 1024 * 100
    DenominatorN  = log2_e_x256_x1024;
    DenominatorN *= B_VAL;
    DenominatorN *= 100;
    Denominator  += DenominatorN;

    Temperature_x10  = Numerator / Denominator;
    Temperature_x10 -= ZEROC_KELVINS_X100;
    Temperature_x10 /= 10;  // remove final extra scaling

    return Temperature_x10;
}

