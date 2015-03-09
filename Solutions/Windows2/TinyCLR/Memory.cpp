////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

using namespace Microsoft::SPOT::Emulator;

////////////////////////////////////////////////////////////////////////////////////////////////////

void HeapLocation( UINT8*& BaseAddress, UINT32& SizeInBytes )
{
    IntPtr address;
    UINT32 size;

    EmulatorNative::GetIMemoryDriver()->HeapLocation( address, size );
            
    BaseAddress = (UINT8*)address.ToPointer();
    SizeInBytes = size;

    HalSystemConfig.RAM1.Base = (UINT32)(size_t)BaseAddress;
    HalSystemConfig.RAM1.Size =                 SizeInBytes;
}

void CustomHeapLocation( UINT8*& BaseAddress, UINT32& SizeInBytes )
{
    IntPtr address;
    UINT32 size;

    EmulatorNative::GetIMemoryDriver()->CustomHeapLocation( address, size );
            
    BaseAddress = (UINT8*)address.ToPointer();
    SizeInBytes = size;
}
