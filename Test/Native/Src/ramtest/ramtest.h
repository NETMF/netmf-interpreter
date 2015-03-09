////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\Log\Log.h"

//--//

#ifndef _ramtest_
#define _ramtest_ 1

//
enum ENDIAN_TYPE {LE_ENDIAN, BE_ENDIAN};
//
// PATTERN_1 and PATTERN_2 are arbitrary bit-wise complements
//
#define  PATTERN_1    0xF0E1C387
#define  PATTERN_2    0x0F1E3C78
//

union ENDIAN_UNION
{
    struct 
    {
        UINT16 s0;
        UINT16 s1;
        UINT16 s2;
        UINT16 s3;
        UINT32 l4;
        UINT32 l5;
        UINT64 d6;
    } typeData;
    UINT8 memData[24];
};

class RAM
{
    ENDIAN_TYPE   m_Endianess;
    UINT32*       m_RAMTestBase;
    UINT32        m_RAMTestSize;
    UINT32        m_BusWidth;
public:
                  RAM( UINT32* RAMTestBase, UINT32 RAMTestSize, ENDIAN_TYPE Endianess, UINT32 BusWidth );
    BOOL          Execute( LOG_STREAM Stream );
    
}; 

//--//

#endif

