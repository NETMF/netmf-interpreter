////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Windows_devices.h"

// [MethodImplAttribute( MethodImplOptions.InternalCall )]
// extern internal SpiBusInfo( int busNum );
HRESULT Library_windows_devices_native_Windows_Devices_Spi_SpiBusInfo::_ctor___VOID__I4( CLR_RT_StackFrame& stackFrame )
{
    TINYCLR_HEADER();

    // Arg[0] ==> implicit this pointer
    CLR_RT_HeapBlock* pThis = stackFrame.This();
    FAULT_ON_NULL( pThis );

    { // use scope to bypass 'Warning: transfer of control bypasses initialization of port' (or similar depending on compiler)
        // Arg[1] ==> port
        UINT32 port = stackFrame.ArgN( 1 ).NumericByRef().s4;
    
        pThis[ FIELD__MinClockFrequency_ ].NumericByRef().s4   = CPU_SPI_MinClockFrequency( port );
        pThis[ FIELD__MaxClockFrequency_ ].NumericByRef().s4   = CPU_SPI_MaxClockFrequency( port );
        pThis[ FIELD__ChipSelectLineCount_ ].NumericByRef().s4 = CPU_SPI_ChipSelectLineCount( port );
    }

    TINYCLR_NOCLEANUP();
}
