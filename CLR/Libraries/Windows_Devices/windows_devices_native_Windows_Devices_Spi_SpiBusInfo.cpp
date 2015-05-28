////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "windows_devices.h"

HRESULT Library_windows_devices_native_Windows_Devices_Spi_SpiBusInfo::get_ChipSelectLineCount___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    SetResult_INT32( stack, static_cast<INT32>(CPU_SPI_PortsCount()) );

    TINYCLR_NOCLEANUP();
}
