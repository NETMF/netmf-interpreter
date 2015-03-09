////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InputPort::get_Resistor___MicrosoftSPOTHardwarePortResistorMode( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
    stack.SetResult_I4( pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_resistorMode ].NumericByRefConst().s4 );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InputPort::set_Resistor___VOID__MicrosoftSPOTHardwarePortResistorMode( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
        GPIO_RESISTOR resistorMode = (GPIO_RESISTOR)stack.Arg1().NumericByRef().u4;

        CLR_RT_HeapBlock_NativeEventDispatcher* port = NULL;
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, port ));
        
        // Change data member in managed object. 
        pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_resistorMode ].NumericByRef().s4 = resistorMode;

        // Changes hardware state to new resistor mode for input port. The last false means input port.
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::ChangeState( pThis, port, false ));
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InputPort::get_GlitchFilter___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
    stack.SetResult_I4( pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_glitchFilterEnable ].NumericByRefConst().s4 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InputPort::set_GlitchFilter___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock_NativeEventDispatcher* port;
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, port ));
        
        CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
        
        bool glitchFilter = stack.Arg1().NumericByRef().u1 != 0;;
        
        // Change data member in managed object. 
        pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_glitchFilterEnable ].NumericByRef().s4 = glitchFilter;
        
        // Changes hardware state to new GlitchFilter mode for input port. The last false means input port.
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::ChangeState( pThis, port, false ));
    }
    TINYCLR_NOCLEANUP();
}

