////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "windows_devices.h"

typedef Library_windows_devices_native_Windows_Devices_Gpio_GpioPin GpioPin;

enum GpioPinDriveMode
{
    GpioPinDriveMode_Input = 0,
    GpioPinDriveMode_Output,
    GpioPinDriveMode_InputWithPullUp,
    GpioPinDriveMode_InputWithPullDown,
    GpioPinDriveMode_OutputStrongLow,
    GpioPinDriveMode_OutputStrongLowPullUp,
    GpioPinDriveMode_OutputStrongHigh,
    GpioPinDriveMode_OutputStrongHighPullDown,
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
    case GpioPinDriveMode_InputWithPullUp:
    case GpioPinDriveMode_OutputStrongLowPullUp:
        return RESISTOR_PULLUP;

    case GpioPinDriveMode_InputWithPullDown:
    case GpioPinDriveMode_OutputStrongHighPullDown:
        return RESISTOR_PULLDOWN;
    }

    return RESISTOR_DISABLED;
}

static CLR_RT_HeapBlock_NativeEventDispatcher* GetEventDispatcher(CLR_RT_HeapBlock* pThis)
{
    CLR_RT_ObjectToEvent_Source* source = CLR_RT_ObjectToEvent_Source::ExtractInstance( pThis[ GpioPin::FIELD__m_eventDispatcher ] );
    if (source == NULL)
    {
        return NULL;
    }

    return (CLR_RT_HeapBlock_NativeEventDispatcher*)source->m_eventPtr;
}

void IsrProcedure( GPIO_PIN pin, BOOL pinState, void* context )
{
    ASSERT_IRQ_MUST_BE_OFF();

    // Get the GpioPin object and bail out early if it was disposed.
    CLR_RT_HeapBlock* pThis = (CLR_RT_HeapBlock*)context;
    if ((pThis == NULL) || (pThis[ GpioPin::FIELD__m_disposed ].NumericByRef().s1 != 0))
    {
        return;
    }

    CLR_RT_HeapBlock_NativeEventDispatcher* dispatcher = GetEventDispatcher(pThis);
    if (dispatcher == NULL)
    {
        return;
    }

    /* TODO: Once we move to level interrupts, we'll need to disable the interrupt each time it fires.
    GPIO_PIN portId = pThis[ GpioPin::FIELD__m_pinNumber ].NumericByRefConst().u4;
    GPIO_RESISTOR resistorMode = GetResistorMode((GpioPinDriveMode)pThis[ GpioPin::FIELD__m_driveMode ].NumericByRefConst().s4);
    ::CPU_GPIO_EnableInputPin2(portId, FALSE, NULL, NULL, GPIO_INT_NONE, resistorMode);
    */

    // To calling SaveToHALQueue saves data to 128 slot HAL queue and finally causes dispatch to managed callback. 
    dispatcher->SaveToHALQueue( pin, pinState );
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::get_DebounceTimeout___mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT64 value;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    value = (CLR_INT64)::CPU_GPIO_GetDebounce() * TIME_CONVERSION__TO_MILLISECONDS;
    stack.SetResult_I8( value );
    stack.TopValue().ChangeDataType( DATATYPE_TIMESPAN );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::set_DebounceTimeout___VOID__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    // TODO: Set debounce individually per pin.
    //CLR_INT64 val = (CLR_INT64_TEMP_CAST) stack.Arg1().NumericByRef().s8 / TIME_CONVERSION__TO_MILLISECONDS;
    //if (!::CPU_GPIO_SetDebounce( val ))
    //{
    //    TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    //}
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

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::Init___VOID__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    GPIO_PIN portId = GPIO_PIN_NONE;
    BOOL pinAllocated = FALSE;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    portId = stack.Arg1().NumericByRef().u4;

    // Ensure the pin exists.
    if (::CPU_GPIO_Attributes( portId ) == 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    // Reserve the pin. If this fails, it's already in use.
    pinAllocated = ::CPU_GPIO_ReservePin( portId, TRUE );
    if (!pinAllocated)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_PIN_UNAVAILABLE);
    }

    pThis[ FIELD__m_pinNumber ].NumericByRef().u4 = portId;

    // Allocate and initialize an event dispatcher instance.
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_NativeEventDispatcher::CreateInstance( *pThis, pThis[ FIELD__m_eventDispatcher ] ));

    // Initialize the default drive mode.
    if (!::CPU_GPIO_EnableInputPin2( portId, false, NULL, NULL, GPIO_INT_NONE, RESISTOR_DISABLED ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    TINYCLR_CLEANUP();
    if (FAILED(hr))
    {
        if (pinAllocated)
        {
            ::CPU_GPIO_ReservePin( portId, FALSE );
        }
    }
    TINYCLR_CLEANUP_END();
}

HRESULT Library_windows_devices_native_Windows_Devices_Gpio_GpioPin::Dispose___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    // If disposing (not finalizing), clean up.
    if (stack.Arg1().NumericByRef().u1)
    {
        GPIO_PIN portId = pThis[ FIELD__m_pinNumber ].NumericByRefConst().u4;

        // Remove our event dispatcher from the HAL queue.
        CLR_RT_HeapBlock_NativeEventDispatcher* dispatcher = GetEventDispatcher(pThis);
        if (dispatcher != NULL)
        {
            dispatcher->RemoveFromHALQueue();
        }

        // Set pin to input state so the pin is in "weak" state and does not drain power. Then release the pin.
        ::CPU_GPIO_EnableInputPin2( portId, false, NULL, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
        ::CPU_GPIO_ReservePin( portId, FALSE );
    }

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
    case GpioPinDriveMode_InputWithPullUp:
    case GpioPinDriveMode_InputWithPullDown:
        {
            // TODO: Find a way to move this to level interrupts.
            BOOL callbacksRegistered = (pThis[ FIELD__m_callbacks ].Dereference() != NULL);
            GPIO_RESISTOR resistorMode = GetResisterMode(driveMode);
            ::CPU_GPIO_EnableInputPin2(
                portId,
                true,
                IsrProcedure,
                pThis,
                callbacksRegistered ? GPIO_INT_EDGE_BOTH : GPIO_INT_NONE,
                resistorMode );
        }
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    TINYCLR_NOCLEANUP();
}
