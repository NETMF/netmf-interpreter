////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

void Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::IsrProcedure( GPIO_PIN pin, BOOL pinState, void* context )
{
    { 
        ASSERT_IRQ_MUST_BE_OFF();

        // If context parameter is NULL, then there is no way to dispatch data managed thread.
        CLR_RT_HeapBlock_NativeEventDispatcher* port = (CLR_RT_HeapBlock_NativeEventDispatcher*)context;
        if ( port == NULL )
        {    
            return;
        }
        
        // Retrieve managed object correspoinding to instance of CLR_RT_HeapBlock.
        // Managed object keeps all the data associated with port.
        CLR_RT_HeapBlock* pManagedPortObj = NULL;
        port->RecoverManagedObject( pManagedPortObj );
        if ( pManagedPortObj == NULL )
        {
            return;
        }

        CLR_INT32 &flags              = pManagedPortObj[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_flags             ].NumericByRef().s4;
        CLR_UINT32  portID            = pManagedPortObj[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_portId            ].NumericByRefConst().u4;
        GPIO_INT_EDGE  interruptMode  = (GPIO_INT_EDGE)pManagedPortObj[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_interruptMode ].NumericByRefConst().s4;
        GPIO_RESISTOR  resistorMode   = (GPIO_RESISTOR)pManagedPortObj[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_resistorMode  ].NumericByRefConst().s4;

        
        // Discard the call if flags show that interrupt is disabled for this port. 
        if ( flags & GPIO_PortParams::c_InterruptDisabled )
        {
            return;
        }

        // For the level interrupt we disable it once the level was reached.
        // Application needs to enable it back. It is used to protect agains multiple callbacks as result of reaching the level. 
        if ( interruptMode == GPIO_INT_LEVEL_HIGH || interruptMode == GPIO_INT_LEVEL_LOW )
        {   
           
            if ( flags & GPIO_PortParams::c_Disposed )
            {
                return;
            }

            // Disable interrupts for port if not disabled. 
            if(( flags & GPIO_PortParams::c_InterruptDisabled ) == 0)
            {
                // Disable level interrupt.
                flags |= GPIO_PortParams::c_InterruptDisabled;
                ::CPU_GPIO_EnableInputPin2(
                                             portID, // data1 is portId for GPIO case.
                                             false, 
                                             NULL,
                                             0,
                                             GPIO_INT_NONE,
                                             resistorMode
                                          );
            }
        }

        // To calling SaveToHALQueue saves data to 128 slot HAL queue and finally causes dispatch to managed callback. 
        port->SaveToHALQueue( pin, pinState );
    }
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::ChangeState( CLR_RT_HeapBlock* pThis, CLR_RT_HeapBlock_NativeEventDispatcher* pIOPort, bool toOutput )

{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    {
        CLR_INT32 &flags              = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_flags              ].NumericByRef().s4;
        CLR_UINT32  portID            = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_portId             ].NumericByRefConst().u4;
        CLR_INT32  glitchFilterEnable = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_glitchFilterEnable ].NumericByRefConst().s4;
        CLR_INT32  initialState       = pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_initialState       ].NumericByRefConst().s4;
        
        GPIO_RESISTOR  resistorMode  = (GPIO_RESISTOR)pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_resistorMode      ].NumericByRefConst().s4;
        GPIO_INT_EDGE  interruptMode = (GPIO_INT_EDGE)pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_interruptMode     ].NumericByRefConst().s4;
        
        if(toOutput)
        {
            ::CPU_GPIO_EnableOutputPin( portID, initialState );

            flags |= GPIO_PortParams::c_Output;
        }
        else
        {
            if(flags & GPIO_PortParams::c_InterruptDisabled)
            {
                GPIO_INT_EDGE interruptMode = glitchFilterEnable ? (resistorMode == RESISTOR_PULLUP ?  GPIO_INT_EDGE_LOW : GPIO_INT_EDGE_HIGH) : GPIO_INT_NONE;

                if(
                    !::CPU_GPIO_EnableInputPin2(
                                                 portID,
                                                 glitchFilterEnable,
                                                 glitchFilterEnable ? Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::IsrProcedure : NULL,
                                                 0,
                                                 interruptMode,
                                                 resistorMode
                                               )
                  )
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
                }
            }
            else
            {
                if(
                    !::CPU_GPIO_EnableInputPin2(
                                                 portID,
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

            flags &= ~GPIO_PortParams::c_Output;
        }
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::Dispose___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
        CLR_RT_HeapBlock_NativeEventDispatcher* port;
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, port ));
        // Check if HW port was not closed.  - 
        if ( ( pThis[ FIELD__m_flags ].NumericByRefConst().s4 & GPIO_PortParams::c_Disposed ) == 0 )   
        {
            CLR_UINT32 portID = pThis[ FIELD__m_portId ].NumericByRefConst().u4;
            
            // Cleanup the HAL queue from the instance of assiciated CLR_RT_HeapBlock_NativeEventDispatcher 
            port->RemoveFromHALQueue();
            
            // Set flag in managed object that port is closed.
            pThis[ FIELD__m_flags ].NumericByRef().s4 |= GPIO_PortParams::c_Disposed;

            // Set pin to input state so the pin is in "weak" state and does not drain power. 
            ::CPU_GPIO_EnableInputPin2( portID,
                                        false,
                                        NULL,
                                        0,
                                        GPIO_INT_NONE,
                                        RESISTOR_PULLUP
                                      );
            // Releases the pin
            ::CPU_GPIO_ReservePin( portID, FALSE );
            
        }
    }
    TINYCLR_NOCLEANUP();
}

static HRESULT HWPORT_GPIO_CheckPortId( bool fOutput, CLR_UINT32 portId )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    bool fInputOnly;

    UINT8 attr = ::CPU_GPIO_Attributes( portId );

    if(attr == 0) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    fInputOnly = (GPIO_PortParams::c_Output & attr) == 0;

    if(fInputOnly && fOutput)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    TINYCLR_NOCLEANUP();
}

static HRESULT Microsoft_SPOT_Hardware_Port_Construct
( 
    CLR_RT_HeapBlock*  pThis,
    CLR_UINT32 portId, 
    bool glitchFilterEnable, 
    GPIO_RESISTOR  resistorMode, 
    GPIO_INT_EDGE interruptMode, 
    bool initialState,
    bool fOutput
)
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    GPIO_PortParams params;
    BOOL pinAllocated = FALSE;
    CLR_UINT32 portFlags  = 0;

    params.m_interruptMode          = interruptMode;
    params.m_resistorMode           = resistorMode;
    params.m_glitchFilterEnable     = glitchFilterEnable;
    params.m_initialState           = initialState;
    params.m_initialDirectionOutput = fOutput;

    // First we check if this port ID is valid for the specified operation 
    TINYCLR_CHECK_HRESULT(HWPORT_GPIO_CheckPortId( fOutput, portId ));
    
    // Reserve the pin. If reservation fails - no luck, exit with error CLR_E_PIN_UNAVAILABLE
    pinAllocated = ::CPU_GPIO_ReservePin( portId, TRUE );
    if(pinAllocated == FALSE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_PIN_UNAVAILABLE);
    }

    if(fOutput)
    {
        ::CPU_GPIO_EnableOutputPin( portId, params.m_initialState );
    }
    else
    {

        GPIO_INT_EDGE interruptMode = params.m_glitchFilterEnable ? (params.m_resistorMode == RESISTOR_PULLUP ?  GPIO_INT_EDGE_LOW : GPIO_INT_EDGE_HIGH) : GPIO_INT_NONE;

        if(
            !::CPU_GPIO_EnableInputPin2(
                                    portId,
                                    params.m_glitchFilterEnable,
                                    params.m_glitchFilterEnable ? Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::IsrProcedure : NULL,
                                    0,
                                    interruptMode,
                                    params.m_resistorMode
                                  )
          )
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);    
        }
    }

    // Allocates and initializes instance of CLR_RT_HeapBlock_NativeEventDispatcher
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_NativeEventDispatcher::CreateInstance( *pThis, pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_NativeEventDispatcher ] ));

    portFlags = ::CPU_GPIO_Attributes( portId );
    // ports can initially be both input and output (indicating they support both), 
    // but now that we are explicitly initializing the port to a direction, we need
    // to set the flag accordingly.
    portFlags &= (fOutput ? GPIO_ATTRIBUTE_OUTPUT: GPIO_ATTRIBUTE_INPUT);
    portFlags |= GPIO_PortParams::c_InterruptDisabled;
        
    // Save parameters in managed instance of port variable
    pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_interruptMode      ].NumericByRef().s4 = interruptMode;
    pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_resistorMode       ].NumericByRef().s4 = resistorMode;
    pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_portId             ].NumericByRef().u4 = portId;
    pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_flags              ].NumericByRef().s4 = portFlags;
    pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_glitchFilterEnable ].NumericByRef().s4 = glitchFilterEnable;
    pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::FIELD__m_initialState       ].NumericByRef().s4 = initialState;

    
    // In cleanup section if operation failed we need to release hardware port with "portID"
    TINYCLR_CLEANUP();
    if(FAILED(hr))
    {
        if(pinAllocated) 
        {
            ::CPU_GPIO_EnableInputPin2( portId, false, NULL, 0, GPIO_INT_NONE, RESISTOR_PULLUP );
            ::CPU_GPIO_ReservePin( portId, FALSE );
        }
    }
    TINYCLR_CLEANUP_END();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::_ctor___VOID__MicrosoftSPOTHardwareCpuPin__BOOLEAN__MicrosoftSPOTHardwarePortResistorMode__MicrosoftSPOTHardwarePortInterruptMode( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock*  pThis = stack.This();  FAULT_ON_NULL(pThis);
        CLR_UINT32     portId             =                stack.Arg1().NumericByRef().u4;
        bool           glitchFilterEnable =                stack.Arg2().NumericByRef().u1 != 0;
        GPIO_RESISTOR  resistorMode       = (GPIO_RESISTOR)stack.Arg3().NumericByRef().u4;
        GPIO_INT_EDGE  interruptMode      = (GPIO_INT_EDGE)stack.Arg4().NumericByRef().u4;

        TINYCLR_CHECK_HRESULT(Microsoft_SPOT_Hardware_Port_Construct( pThis, portId, glitchFilterEnable, resistorMode, interruptMode, false, false ));
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::_ctor___VOID__MicrosoftSPOTHardwareCpuPin__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
        CLR_UINT32 portId       = stack.Arg1().NumericByRef().u4;
        bool       initialState = stack.Arg2().NumericByRef().u1 != 0;
    
        TINYCLR_CHECK_HRESULT(Microsoft_SPOT_Hardware_Port_Construct( pThis, portId, false, RESISTOR_DISABLED, GPIO_INT_NONE, initialState, true )); 
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::_ctor___VOID__MicrosoftSPOTHardwareCpuPin__BOOLEAN__BOOLEAN__MicrosoftSPOTHardwarePortResistorMode( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    {
        CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
        CLR_UINT32    portId       =                stack.Arg1().NumericByRef().u4;
        bool          initialState =                stack.Arg2().NumericByRef().u1 != 0;
        bool          glitchFilter =                stack.Arg3().NumericByRef().u1 != 0;
        GPIO_RESISTOR resistorMode = (GPIO_RESISTOR)stack.Arg4().NumericByRef().u4;

        TINYCLR_CHECK_HRESULT(Microsoft_SPOT_Hardware_Port_Construct( pThis, portId, glitchFilter, resistorMode, GPIO_INT_NONE, initialState, false ));
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::Read___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_UINT32 state;
        
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
    
    if(pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }
    
    // Read state from Hardware
    state = ::CPU_GPIO_GetPinState( pThis[ FIELD__m_portId ].NumericByRefConst().u4 );
    
    // Return value to the managed application.
    stack.SetResult_Boolean( state == 0 ? false : true ) ;
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::get_Id___MicrosoftSPOTHardwareCpuPin( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);     
    // Read port ID from the managed object. Application can get it in managed code, but we support backward compatibility. 
    stack.SetResult( pThis[ FIELD__m_portId ].NumericByRefConst().u4, DATATYPE_U4 );
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port::ReservePin___STATIC__BOOLEAN__MicrosoftSPOTHardwareCpuPin__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_UINT32 portId; 
    bool       fReserve;

    portId   = stack.Arg0().NumericByRef().u4;
    fReserve = stack.Arg1().NumericByRef().u1 != 0;

    if(!::CPU_GPIO_ReservePin( portId, fReserve ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }
    
    stack.SetResult_Boolean( true );

    TINYCLR_NOCLEANUP();
}

