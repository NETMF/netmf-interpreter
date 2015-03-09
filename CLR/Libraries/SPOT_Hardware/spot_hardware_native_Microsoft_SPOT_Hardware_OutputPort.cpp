////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_OutputPort::Write___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    bool state;
    
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);     
   
    if(pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    state = stack.Arg1().NumericByRef().u1 != 0;

    // Test if flag says it is output port
    if (pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_flags ].NumericByRefConst().s4 & GPIO_PortParams::c_Output)
    {   // Writes value to the port.
        ::CPU_GPIO_SetPinState( pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_portId ].NumericByRefConst().u4, state );
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_OutputPort::get_InitialState___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);     

    stack.SetResult( pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_initialState ].NumericByRefConst().s4, DATATYPE_U4 );

    TINYCLR_NOCLEANUP();
}

