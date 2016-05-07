////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <stdafx.h>
#include "HAL_Windows.h"

////////////////////////////////////////////////////////////////////////////////////////////////////
#if defined(PLATFORM_WINDOWS_EMULATOR)
HAL_Configuration_Windows g_HAL_Configuration_Windows;
#endif

HAL_SYSTEM_CONFIG HalSystemConfig =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//

	// COM_HANDLE      DebuggerPorts[MAX_DEBUGGERS];
    {                                               
        DEBUGGER_PORT,
    },

    {
        MESSAGING_PORT,
    },

    DEBUG_TEXT_PORT,
    115200,
    0,  // STDIO = COM2 or COM1

    { 0, 0 },   // { SRAM1_MEMORY_Base, SRAM1_MEMORY_Size },
    { 0, 0 },   // { FLASH_MEMORY_Base, FLASH_MEMORY_Size },
};

static UINT8* s_Memory_Start  = NULL;
static UINT32 s_Memory_Length = 1024 * 1024 * 10;

void HeapLocation( UINT8*& BaseAddress, UINT32& SizeInBytes )
{
    if(!s_Memory_Start)
    {
        s_Memory_Start = (UINT8*)::VirtualAlloc( NULL, s_Memory_Length, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE );

        if(s_Memory_Start)
        {
            memset( s_Memory_Start, 0xEA, s_Memory_Length );
        }

        HalSystemConfig.RAM1.Base = (UINT32)(size_t)s_Memory_Start;
        HalSystemConfig.RAM1.Size = (UINT32)(size_t)s_Memory_Length;
    }

    BaseAddress = s_Memory_Start;
    SizeInBytes = s_Memory_Length;
}

static UINT8* s_CustomHeap_Start  = NULL;

void CustomHeapLocation( UINT8*& BaseAddress, UINT32& SizeInBytes )
{
    if(!s_CustomHeap_Start)
    {
        s_CustomHeap_Start = (UINT8*)::VirtualAlloc( NULL, s_Memory_Length, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE );

        if(s_CustomHeap_Start)
        {
            memset( s_CustomHeap_Start, 0xEA, s_Memory_Length );
        }
    }

    BaseAddress = s_CustomHeap_Start;
    SizeInBytes = s_Memory_Length;
}

//--//

HRESULT HAL_Windows::Memory_Resize( UINT32 size )
{
    TINYCLR_HEADER();

    if(s_Memory_Start)
    {
        ::VirtualFree( s_Memory_Start, 0, MEM_RELEASE );

        s_Memory_Start = NULL;
    }

    if(s_CustomHeap_Start)
    {
        ::VirtualFree( s_CustomHeap_Start, 0, MEM_RELEASE );

        s_CustomHeap_Start = NULL;
    }

    s_Memory_Length = size;

    TINYCLR_NOCLEANUP_NOLABEL();
}

