////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_EBIU_DECL_H_
#define _DRIVERS_EBIU_DECL_H_ 1

//--//

struct CPU_MEMORY_CONFIG
{
    UINT8   ChipSelect;
    UINT8   ReadOnly;
    UINT32  WaitStates;
    UINT32  ReleaseCounts;
    UINT32  BitWidth;
    UINT32  BaseAddress;
    UINT32  SizeInBytes;
    UINT8   XREADYEnable;           // 0,1
    UINT8   ByteSignalsForRead;     // 0,1
    UINT8   ExternalBufferEnable;   // 0,1
};

//--//

void CPU_EBIU_ConfigMemoryBlock( const CPU_MEMORY_CONFIG& CPUMemoryConfig                );
BOOL CPU_EBIU_Memory_ReadOnly  ( const CPU_MEMORY_CONFIG& CPUMemoryConfig, BOOL ReadOnly );

//--//

#endif // _DRIVERS_EBIU_DECL_H_

