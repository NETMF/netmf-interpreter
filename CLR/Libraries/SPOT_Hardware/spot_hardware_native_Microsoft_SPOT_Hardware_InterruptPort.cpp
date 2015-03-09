////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

static HRESULT Microsoft_SPOT_Hardware_InterruptPort_EnableInterrupt( CLR_RT_HeapBlock* pThis, CLR_RT_HeapBlock_NativeEventDispatcher* pIOPort )
{    
    TINYCLR_HEADER();
    {
        CLR_INT32 &flags              = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_flags              ].NumericByRef().s4;
        CLR_UINT32  portId            = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_portId             ].NumericByRef().u4;
        CLR_INT32  glitchFilterEnable = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_glitchFilterEnable ].NumericByRef().s4;
        
        GPIO_RESISTOR  resistorMode  = (GPIO_RESISTOR)pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_resistorMode      ].NumericByRef().s4;
        GPIO_INT_EDGE  interruptMode = (GPIO_INT_EDGE)pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_interruptMode     ].NumericByRef().s4;
        
        
        if(flags & GPIO_PortParams::c_Disposed)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_PIN_DEAD);
        }

        if(flags & GPIO_PortParams::c_InterruptDisabled)
        {   
            flags &= ~GPIO_PortParams::c_InterruptDisabled;

            if(
                !::CPU_GPIO_EnableInputPin2(
                                             portId,
                                             glitchFilterEnable,
                                             Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::IsrProcedure,
                                             pIOPort,
                                             interruptMode,
                                             resistorMode
                                           )
              )
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
            }
        }
    }
    TINYCLR_NOCLEANUP();
}

static HRESULT Microsoft_SPOT_Hardware_InterruptPort_DisableInterrupt( CLR_RT_HeapBlock* pThis, CLR_RT_HeapBlock_NativeEventDispatcher* pIOPort )

{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    {
        CLR_INT32 &flags              = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_flags             ].NumericByRef().s4;
        CLR_UINT32  portId            = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_portId            ].NumericByRef().u4;
        
        GPIO_RESISTOR  resistorMode   =(GPIO_RESISTOR)pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_resistorMode      ].NumericByRef().s4;
        
        if(flags & GPIO_PortParams::c_Disposed)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_PIN_DEAD);
        }

        if(( flags & GPIO_PortParams::c_InterruptDisabled ) == 0)
        {
            flags |= GPIO_PortParams::c_InterruptDisabled;

            if(
                !::CPU_GPIO_EnableInputPin2(
                                             portId,
                                             false, 
                                             NULL,
                                             0,
                                             GPIO_INT_NONE,
                                             resistorMode
                                           )
              )
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
            }
        }
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InterruptPort::EnableInterrupt___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_NativeEventDispatcher* port;

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
    
    if(pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, port ));
        
    TINYCLR_SET_AND_LEAVE(Microsoft_SPOT_Hardware_InterruptPort_EnableInterrupt( pThis, port ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InterruptPort::DisableInterrupt___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_NativeEventDispatcher* port;
    
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    if(pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, port ));
    
    
    TINYCLR_SET_AND_LEAVE(Microsoft_SPOT_Hardware_InterruptPort_DisableInterrupt( pThis, port ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InterruptPort::ClearInterrupt___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_NativeEventDispatcher* port;
    GPIO_INT_EDGE                           interruptMode;

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    if(pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }
    
    TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, port ));
    
    interruptMode = (GPIO_INT_EDGE)pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_interruptMode ].NumericByRef().s4;

    switch(interruptMode)
    {
        case GPIO_INT_LEVEL_HIGH :
        case GPIO_INT_LEVEL_LOW  :
            TINYCLR_SET_AND_LEAVE(Microsoft_SPOT_Hardware_InterruptPort_EnableInterrupt( pThis, port ));
            break;
        default:
            // NOP
            break;
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InterruptPort::get_Interrupt___MicrosoftSPOTHardwarePortInterruptMode( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);     
    
    stack.SetResult( pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_interruptMode ].NumericByRefConst().s4, DATATYPE_U4 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_InterruptPort::set_Interrupt___VOID__MicrosoftSPOTHardwarePortInterruptMode( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_NativeEventDispatcher* port;
    GPIO_INT_EDGE                           interrupt;

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    if(pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, port ));
    
    interrupt = (GPIO_INT_EDGE)stack.Arg1().NumericByRef().u4;

    pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_interruptMode ].NumericByRef().s4 = interrupt;

    // Changes hardware state to new interrupt mode for input port. The last false means input port.
    TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::ChangeState( pThis, port, false ));
    
    TINYCLR_NOCLEANUP();
}

