////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "windows_devices.h"

enum GpioPinDriveMode
{
    GpioPinDriveMode_Input = 0,
    GpioPinDriveMode_Output,
    GpioPinDriveMode_InputPullUp,
    GpioPinDriveMode_InputPullDown,
    GpioPinDriveMode_OutputOpenDrain,
    GpioPinDriveMode_OutputOpenDrainPullUp,
    GpioPinDriveMode_OutputOpenSource,
    GpioPinDriveMode_OutputOpenSourcePullDown,
};

enum GpioPinValue
{
    GpioPinValue_Low = 0,
    GpioPinValue_High,
};

static GPIO_RESISTOR GetResisterMode(GpioPinDriveMode driveMode)
{
    switch (driveMode)
    {
    case GpioPinDriveMode_InputPullUp:
    case GpioPinDriveMode_OutputOpenDrainPullUp:
        return RESISTOR_PULLUP;

    case GpioPinDriveMode_InputPullDown:
    case GpioPinDriveMode_OutputOpenSourcePullDown:
        return RESISTOR_PULLDOWN;
    }

    return RESISTOR_DISABLED;
}

void IsrProcedure( GPIO_PIN pin, BOOL pinState, void* context )
{
    ASSERT_IRQ_MUST_BE_OFF();
    PostManagedEvent( EVENT_GPIO, 0, pin, pinState );
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::get_DebounceTimeout___mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    GPIO_PIN pin;
    CLR_INT64 value;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    pin = (GPIO_PIN)( pThis[ FIELD__m_pinNumber ].NumericByRefConst().u4 );
    value = (CLR_INT64)::CPU_GPIO_GetPinDebounce( pin ) * TIME_CONVERSION__TO_MILLISECONDS;
    stack.SetResult_I8( value );
    stack.TopValue().ChangeDataType( DATATYPE_TIMESPAN );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::set_DebounceTimeout___VOID__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    GPIO_PIN pin;
    CLR_INT64 value;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    value = (CLR_INT64_TEMP_CAST) stack.Arg1().NumericByRef().s8 / TIME_CONVERSION__TO_MILLISECONDS;
    pin = (GPIO_PIN)( pThis[ FIELD__m_pinNumber ].NumericByRefConst().u4 );
    if (!::CPU_GPIO_SetPinDebounce( pin, value ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::Write___VOID__WindowsDevicesGpioGpioPinValue( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    BOOL state;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    pThis[ FIELD__m_lastOutputValue ].NumericByRef().s4 = stack.Arg1().NumericByRef().s4;
    state = (stack.Arg1().NumericByRef().s4 != GpioPinValue_Low);

    // If the current drive mode is set to output, write the value to the pin.
    if (pThis[ FIELD__m_driveMode ].NumericByRefConst().s4 == GpioPinDriveMode_Output)
    {
        ::CPU_GPIO_SetPinState( pThis[ FIELD__m_pinNumber ].NumericByRefConst().u4, state );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::Read___WindowsDevicesGpioGpioPinValue( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    BOOL state;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    state = ::CPU_GPIO_GetPinState( pThis[ FIELD__m_pinNumber ].NumericByRefConst().u4 );
    stack.SetResult_I4( state ? GpioPinValue_High : GpioPinValue_Low );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::InitNative___BOOLEAN__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    GPIO_PIN portId = GPIO_PIN_NONE;
    bool pinAllocated = false;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    portId = stack.Arg1().NumericByRef().u4;

    // Ensure the pin exists.
    if (::CPU_GPIO_Attributes( portId ) == 0)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    // Reserve the pin. If this fails, it's already in use.
    pinAllocated = !!::CPU_GPIO_ReservePin( portId, TRUE );
    if (!pinAllocated)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    // Initialize the default drive mode.
    if (!::CPU_GPIO_EnableInputPin2( portId, false, NULL, NULL, GPIO_INT_NONE, RESISTOR_DISABLED ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    pThis[ FIELD__m_pinNumber ].NumericByRef().u4 = portId;

    TINYCLR_CLEANUP();

    if (FAILED(hr))
    {
        if (pinAllocated)
        {
            ::CPU_GPIO_ReservePin( portId, FALSE );
        }
    }

    stack.SetResult_Boolean( pinAllocated );

    TINYCLR_CLEANUP_END();
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::DisposeNative___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    GPIO_PIN portId;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    portId = pThis[ FIELD__m_pinNumber ].NumericByRefConst().u4;

    // Set pin to input state so the pin is in "weak" state and does not drain power. Then release the pin.
    ::CPU_GPIO_EnableInputPin2( portId, false, NULL, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
    ::CPU_GPIO_ReservePin( portId, FALSE );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::SetDriveModeInternal___VOID__WindowsDevicesGpioGpioPinDriveMode( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    GpioPinDriveMode driveMode;
    GPIO_PIN portId;
    UINT8 attr;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    driveMode = (GpioPinDriveMode)stack.Arg1().NumericByRef().s4;
    portId = pThis[ FIELD__m_pinNumber ].NumericByRefConst().u4;
    attr = ::CPU_GPIO_Attributes( portId );

    switch (driveMode)
    {
    case GpioPinDriveMode_Output:
        if ((attr & GPIO_PortParams::c_Output) != 0)
        {
            BOOL state = (pThis[ FIELD__m_lastOutputValue ].NumericByRef().s4 != GpioPinValue_Low);
            ::CPU_GPIO_EnableOutputPin( portId, state );
        }
        break;

    case GpioPinDriveMode_Input:
    case GpioPinDriveMode_InputPullUp:
    case GpioPinDriveMode_InputPullDown:
        {
            // TODO: Find a way to move this to level interrupts.
            BOOL callbacksRegistered = (pThis[ FIELD__m_callbacks ].Dereference() != NULL);
            ::CPU_GPIO_EnableInputPin2(
                portId,
                true,
                IsrProcedure,
                NULL,
                callbacksRegistered ? GPIO_INT_EDGE_BOTH : GPIO_INT_NONE,
                GetResisterMode(driveMode) );
        }
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    TINYCLR_NOCLEANUP();
}
