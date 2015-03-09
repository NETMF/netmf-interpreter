////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "ramtest.h"

//------------------------------ GLOBAL     ------------------------------------
// 

//
//------------------------------ TEST CASES ------------------------------------
//
//     RAM Test validates data & address bus lines, endian and sparse location test. 
//
// 
//  DataBusTest is a walking ones across data bus. Address can be any valid address
//  and Pattern should have the high-order bit set to align with the number of 
//  address lines.   Example: Pattern = 0x8000 for 16 bit IC.
//
BOOL DataBusTest( volatile UINT32* Address, UINT32 Pattern )
{
    UINT32 memory;
    while(Pattern)
    {
        *Address = Pattern;
        memory = *Address; 
        if(memory != Pattern) 
        {
            break;
        }  
        Pattern = Pattern >> 1;   
    }
    return (Pattern == 0);  
} // DataBusTest

UINT32* AddressBusTest( volatile UINT32* base, UINT32 nBytes )
{
    UINT32 addressMask = (nBytes/sizeof(base));
    UINT32 offset      = 0;
    UINT32 startOffset = 4;
    UINT32 endOffset   = addressMask;
    UINT32 testOffset  = 0;

    for(offset=startOffset; offset<endOffset; offset <<= 1)
    { 
        base[offset] = PATTERN_1;
    }
    testOffset = 0;
    base[testOffset] = PATTERN_2;
    for(offset=startOffset; offset<endOffset; offset <<= 1)
    {
        if(base[offset] != PATTERN_1)
        {
            return (UINT32*) &base[offset];
        }
    }
    base[testOffset] = PATTERN_1;
    for(testOffset=1; testOffset<endOffset; testOffset <<= 1)
    {
        base[testOffset] = PATTERN_2;

        if(base[0] != PATTERN_1)
        {
            return (UINT32*) &base[offset];            
        }
        for(offset=startOffset; offset<endOffset; offset <<= 1)
        {
            if(offset == testOffset)
            {
                continue;
            }
            if(base[offset] !=  PATTERN_1)
            {
                return (UINT32*) &base[offset]; 
            }
        }
        base[testOffset] = PATTERN_1;
    }
    return NULL;
} //AddressBusTest

BOOL EndianTest( UINT8 Endianess )
{
    ENDIAN_UNION memoryAggregate;
    BOOL         status           = false;

    memoryAggregate.typeData.s0 = 0x0102;
    memoryAggregate.typeData.s1 = 0x0304;
    memoryAggregate.typeData.s2 = 0x0506;
    memoryAggregate.typeData.s3 = 0x0708;
    memoryAggregate.typeData.l4 = 0x090a0b0c;
    memoryAggregate.typeData.l5 = 0x0d0e0f10;
    memoryAggregate.typeData.d6 = 0x1112131415161718ull;

    switch(Endianess)
    {
    case LE_ENDIAN:
        {
            if((memoryAggregate.memData[0]  == 0x02) &&
               (memoryAggregate.memData[2]  == 0x04) &&
               (memoryAggregate.memData[4]  == 0x06) &&
               (memoryAggregate.memData[6]  == 0x08) &&
               (memoryAggregate.memData[8]  == 0x0c) &&
               (memoryAggregate.memData[12] == 0x10) &&
               (memoryAggregate.memData[16] == 0x18))
            {
                status = true;
            }
            break;
        };
    case BE_ENDIAN:
        {
            if((memoryAggregate.memData[1] == 0x02) &&
               (memoryAggregate.memData[3] == 0x04) &&
               (memoryAggregate.memData[5] == 0x06) &&
               (memoryAggregate.memData[7] == 0x08) &&
               (memoryAggregate.memData[11] == 0x0c) &&
               (memoryAggregate.memData[15] == 0x10) &&
               (memoryAggregate.memData[23] == 0x18))
            {
                status = true;
            }
            break;
        };
    }
    return status;
} //EndianTest

RAM::RAM( UINT32* RAMTestBase, UINT32 RAMTestSize, ENDIAN_TYPE Endianess, UINT32 BusWidth )
{
    m_RAMTestBase = RAMTestBase;
    m_RAMTestSize = RAMTestSize;
    m_Endianess   = Endianess;
    m_BusWidth    = BusWidth;
}
BOOL RAM::Execute( LOG_STREAM Stream )
{
    BOOL status = false;
    Log& log = Log::InitializeLog( Stream, "RAM_Test");

    status = (DataBusTest( (UINT32*)m_RAMTestBase, m_BusWidth ) != NULL);
    status = (AddressBusTest( (UINT32*)m_RAMTestBase, m_RAMTestSize ) == 0) && status;
    status = EndianTest( m_Endianess ) && status;
   
    log.CloseLog  ( status, NULL );
    return status; 
}

//------------------------------ CASE SUPPORT -----------------------------------

