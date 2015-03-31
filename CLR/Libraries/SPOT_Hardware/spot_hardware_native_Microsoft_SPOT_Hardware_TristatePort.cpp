////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_TristatePort::get_Active___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
    stack.SetResult_Boolean( pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_flags ].NumericByRefConst().s4 
                             & GPIO_PortParams::c_Output ? true : false );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_TristatePort::set_Active___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock_NativeEventDispatcher* port;
    CLR_INT32                               flags;
    bool                                    activate;
    
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
    
    if(pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, port ));
    
    activate = stack.Arg1().NumericByRefConst().u1 != 0;

    flags = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_flags ].NumericByRefConst().s4;

    if( ( activate && ((flags & GPIO_PortParams::c_Output) == 0)) ||
        (!activate &&  (flags & GPIO_PortParams::c_Output)     )    )
    {
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::ChangeState( pThis, port, activate ));
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_TristatePort::get_Resistor___MicrosoftSPOTHardwarePortResistorMode( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    return Library_spot_hardware_native_Microsoft_SPOT_Hardware_InputPort::get_Resistor___MicrosoftSPOTHardwarePortResistorMode( stack );
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_TristatePort::set_Resistor___VOID__MicrosoftSPOTHardwarePortResistorMode( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    return Library_spot_hardware_native_Microsoft_SPOT_Hardware_InputPort::set_Resistor___VOID__MicrosoftSPOTHardwarePortResistorMode( stack ); 
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_TristatePort::get_GlitchFilter___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    return Library_spot_hardware_native_Microsoft_SPOT_Hardware_InputPort::get_GlitchFilter___BOOLEAN( stack );
}

