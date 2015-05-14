////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "windows_devices.h"

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioController::get_PinCount___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    stack.SetResult_I4( ::CPU_GPIO_GetPinCount() );

    TINYCLR_NOCLEANUP();
}
