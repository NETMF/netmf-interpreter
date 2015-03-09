////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware_serial.h"

HRESULT Library_spot_hardware_serial_native_System_IO_Ports_SerialPort::InternalOpen___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis;
    CLR_RT_HeapBlock* config;
    CLR_UINT32        portId;
    CLR_UINT32        speed;
    CLR_UINT32        parity;
    CLR_UINT32        dataBits;
    CLR_UINT32        stopBits;
    CLR_UINT32        flowValue;

    pThis = stack.This();  FAULT_ON_NULL(pThis);

    // check if the object was disposed
    if(pThis[ FIELD__m_fDisposed ].NumericByRef().s1 != 0) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    config = pThis[ FIELD__m_config ].Dereference(); FAULT_ON_NULL(config);

    portId    = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__PortIndex ].NumericByRef().u4;
    speed     = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__Speed ]    .NumericByRef().u4;
    parity    = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__Parity ]   .NumericByRef().u4;
    dataBits  = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__DataBits ] .NumericByRef().u4;
    stopBits  = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__StopBits ] .NumericByRef().u4;
    flowValue = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__Handshake ].NumericByRef().u4;

    ::USART_Flush( portId );
    
    ::USART_Uninitialize( portId );

    if(FALSE == ::USART_Initialize( portId, speed, parity, dataBits, stopBits, flowValue ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_serial_native_System_IO_Ports_SerialPort::InternalClose___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis;
    CLR_RT_HeapBlock* config;
    CLR_UINT32        portId;

    pThis = stack.This();  FAULT_ON_NULL(pThis);

    // check if the object was disposed
    if(pThis[ FIELD__m_fDisposed ].NumericByRef().s1 != 0) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    config = pThis[ FIELD__m_config ].Dereference(); FAULT_ON_NULL(config);

    portId    = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__PortIndex ].NumericByRef().u4;

    ::USART_Flush( portId );
    
    if(FALSE == ::USART_Uninitialize( portId ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    // wake up waiting potential threads
    g_CLR_RT_ExecutionEngine.SignalEvents( CLR_RT_ExecutionEngine::c_Event_SerialPort );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_serial_native_System_IO_Ports_SerialPort::Read___I4__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* readBuffer;
    CLR_RT_HeapBlock*       pThis;
    CLR_RT_HeapBlock*       config;
    CLR_UINT8*              ptr;
    CLR_INT32               offset;
    CLR_INT32               count;
    CLR_INT32               totLength;
    CLR_INT32               totRead;
    CLR_RT_HeapBlock*       timeout;
    CLR_INT64*              timeoutTicks;
    CLR_INT32               port;
    bool                    fRes;

    pThis = stack.This();  FAULT_ON_NULL(pThis);

    // check if the object was disposed
    if(pThis[ FIELD__m_fDisposed ].NumericByRef().s1 != 0) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }
    
    config = pThis[ FIELD__m_config ].Dereference(); FAULT_ON_NULL(config);

    readBuffer = stack.Arg1().DereferenceArray();  FAULT_ON_NULL(readBuffer);
    offset     = stack.Arg2().NumericByRef().s4;
    count      = stack.Arg3().NumericByRef().s4;
    totLength  = readBuffer->m_numOfElements;
    timeout    = &config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__ReadTimeout ];
    port       = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__PortIndex ].NumericByRef().s4;

    //
    // Bound checking.
    //
    if(offset < 0 || offset > totLength)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    if(count == -1)
    {
        count = totLength - offset;
    }
    else
    {
        if(count < 0 || (offset+count) > totLength)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
        }
    }

    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( *timeout, timeoutTicks ));

    //
    // Push "totRead" onto the eval stack.
    //
    if(stack.m_customState == 1)
    {
        stack.PushValueI4( 0 );
        
        stack.m_customState = 2;
    }

    //--//

    totRead = stack.m_evalStack[ 1 ].NumericByRef().s4;

    ptr    = readBuffer->GetFirstElement();
    ptr   += offset + totRead;
    count -= totRead;

    fRes = true;

    while(fRes && count > 0)
    {
        int read = ::USART_Read( port, (char*)ptr, count );

        if(read == 0)
        {
            stack.m_evalStack[ 1 ].NumericByRef().s4 = totRead;

            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_SerialPort, fRes ));
        }
        else if(read < 0)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
        }
        else
        {
            ptr     += read;
            totRead += read;
            count   -= read;

            break;
        }
    }

    stack.PopValue();       // totRead
    stack.PopValue();       // Timeout

    stack.SetResult_I4( totRead );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_serial_native_System_IO_Ports_SerialPort::Write___VOID__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis;
    CLR_RT_HeapBlock*       config;
    CLR_RT_HeapBlock_Array* writeBuffer;
    CLR_UINT8*              ptr;
    CLR_INT32               offset;
    CLR_INT32               count;
    CLR_INT32               totLength;
    CLR_INT32               totWrite;
    CLR_INT32               port;
    CLR_RT_HeapBlock*       timeout;
    CLR_INT64*              timeoutTicks;
    bool                    fRes;

    pThis = stack.This();  FAULT_ON_NULL(pThis);
    
    // check if the object was disposed
    if(pThis[ FIELD__m_fDisposed ].NumericByRef().s1 != 0) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }
    
    config = pThis[ FIELD__m_config ].Dereference(); FAULT_ON_NULL(config);

    writeBuffer = stack.Arg1().DereferenceArray();  FAULT_ON_NULL(writeBuffer);
    offset      = stack.Arg2().NumericByRef().s4;
    count       = stack.Arg3().NumericByRef().s4;
    totLength   = writeBuffer->m_numOfElements;
    timeout     = &config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__WriteTimeout ];
    port        = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__PortIndex ].NumericByRef().s4;

    //
    // Bound checking.
    //
    if(offset < 0 || offset > totLength)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    if(count == -1)
    {
        count = totLength - offset;
    }
    else
    {
        if(count < 0 || (offset+count) > totLength)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
        }
    }

    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( *timeout, timeoutTicks ));

    //
    // Push "totWrite" onto the eval stack.
    //
    if(stack.m_customState == 1)
    {
        stack.PushValueI4( 0 );
        
        stack.m_customState = 2;
    }

    totWrite = stack.m_evalStack[ 1 ].NumericByRef().s4;

    ptr    = writeBuffer->GetFirstElement();
    ptr   += offset + totWrite;
    count -= totWrite;

    fRes = true;

    while(fRes && count > 0)
    {
        int write = ::USART_Write( port, (char*)ptr, count );

        if(write == 0)
        {
            stack.m_evalStack[ 1 ].NumericByRef().s4 = totWrite;

            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeoutTicks, CLR_RT_ExecutionEngine::c_Event_SerialPort, fRes ));
        }
        else if(write < 0)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
        }
        else
        {
            ptr      += write;
            totWrite += write;
            count    -= write;
        }
    }

    stack.PopValue();       // totRead
    stack.PopValue();       // Timeout
    
    stack.SetResult_I4( totWrite );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_serial_native_System_IO_Ports_SerialPort::InternalDispose___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock* pThis;
    CLR_RT_HeapBlock* config;
    CLR_UINT32        portId;
    
    pThis  = stack.This();                           FAULT_ON_NULL(pThis);
    config = pThis[ FIELD__m_config ].Dereference(); FAULT_ON_NULL(config);

    portId = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__PortIndex ].NumericByRef().u4;

    ::USART_Flush       ( portId );
    ::USART_Uninitialize( portId );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_serial_native_System_IO_Ports_SerialPort::Flush___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock* pThis;
    CLR_RT_HeapBlock* config;
    CLR_UINT32        portId;

    pThis = stack.This();  FAULT_ON_NULL(pThis);   

    // check if the object was disposed
    if(pThis[ FIELD__m_fDisposed ].NumericByRef().s1 != 0) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    config = pThis[ FIELD__m_config ].Dereference(); FAULT_ON_NULL(config);

    portId = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__PortIndex ].NumericByRef().u4;

    ::USART_Flush( portId );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_serial_native_System_IO_Ports_SerialPort::BytesInBuffer___I4__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis;
    BOOL              fInput;
    CLR_RT_HeapBlock* config;
    CLR_UINT32        portId;
    CLR_INT32         numBytes;

    pThis  = stack.This();  FAULT_ON_NULL(pThis);
    
    // check if the object was disposed
    if(pThis[ FIELD__m_fDisposed ].NumericByRef().s1 != 0) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    config = pThis[ FIELD__m_config ].Dereference(); FAULT_ON_NULL(config);
    fInput = stack.Arg1().NumericByRef().u1 == 0 ? FALSE : TRUE;

    portId = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__PortIndex ].NumericByRef().u4;

    numBytes = USART_BytesInBuffer( portId, fInput );

    if(numBytes < 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    
    stack.SetResult_I4( numBytes );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_serial_native_System_IO_Ports_SerialPort::DiscardBuffer___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis;
    BOOL              fInput;
    CLR_RT_HeapBlock* config;
    CLR_UINT32        portId;

    pThis = stack.This();  FAULT_ON_NULL(pThis);
    
    // check if the object was disposed
    if(pThis[ FIELD__m_fDisposed ].NumericByRef().s1 != 0) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    config = pThis[ FIELD__m_config ].Dereference(); FAULT_ON_NULL(config);
    fInput = stack.Arg1().NumericByRef().u1 == 0 ? FALSE : TRUE;

    portId = config[ Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration::FIELD__PortIndex ].NumericByRef().u4;

    USART_DiscardBuffer( portId, fInput );

    TINYCLR_NOCLEANUP();
}
