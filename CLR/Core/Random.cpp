////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////


void CLR_RT_Random::Initialize()
{
    CLR_INT64  st  = HAL_Time_CurrentTime();
    CLR_INT32* ptr = (CLR_INT32*)&st;

    m_next = ptr[ 0 ] ^ ptr[ 1 ];
}

void CLR_RT_Random::Initialize( int seed )
{
    m_next = seed;
}

int CLR_RT_Random::Next()
{
    //
    // Portable Random Number Generator: page 278-279 "Numerical Recipes in C".
    //
    static const int IA   = 16807;
    static const int IM   = 2147483647;
    static const int IQ   = 127773;
    static const int IR   = 2836;
    static const int MASK = 123459876;

    int v = m_next;
    int k;

    v = v  ^ MASK;
    k = v  / IQ;
    v = IA * (v - k * IQ) - IR * k; if(v < 0) v += IM;
    v = v  ^ MASK;

    m_next = v;

    return v;
}

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)

double CLR_RT_Random::NextDouble()
{
    // Next() will return value between 0 - 0x7FFFFFFF (inclusive)
    return ((double)Next()) / ((double)0x7FFFFFFF);
}
#else

CLR_INT64 CLR_RT_Random::NextDouble()
{
    // Next() will return value between 0 - 0x7FFFFFFF (inclusive)

    return  ((CLR_INT64 )Next() ) / ((CLR_INT64)0x7FFFFFFF >> CLR_RT_HeapBlock::HB_DoubleShift);
}

#endif

void CLR_RT_Random::NextBytes( BYTE* buffer, UINT32 count )
{
    UINT32 i = count / 3;

    while(i-- != 0)
    {
        int rand = Next();

        // We take only the bottom 3 bytes, because the top bit is always 0
        buffer[ 0 ] = (BYTE)rand; rand >>= 8;
        buffer[ 1 ] = (BYTE)rand; rand >>= 8;
        buffer[ 2 ] = (BYTE)rand;

        buffer += 3;
    }

    i = count % 3;

    // fill the remaining bytes
    if(i > 0)
    {
        int rand = Next();
        
        while(i-- != 0)
        {
            buffer[ 0 ] = (BYTE)rand; rand >>= 8;

            buffer++;
        }
    }
}
