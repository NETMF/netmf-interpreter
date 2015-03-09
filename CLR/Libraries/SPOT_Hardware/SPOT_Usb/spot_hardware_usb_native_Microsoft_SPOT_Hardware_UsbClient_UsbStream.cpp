////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware_usb.h"

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::nativeOpen___I4__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32         streamIndex, controllerIndex, writeEndpoint, readEndpoint;
    CLR_RT_HeapBlock* pThis;

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_controllerIndex ].NumericByRef().s4;
    writeEndpoint   = stack.Arg1().NumericByRef().s4;
    readEndpoint    = stack.Arg2().NumericByRef().s4;

    for( streamIndex = 0; streamIndex < USB_MAX_QUEUES; streamIndex++ )
    {
        CLR_INT32 stream = (controllerIndex << USB_CONTROLLER_SHIFT) + streamIndex;
        if( USB_OpenStream( stream, writeEndpoint, readEndpoint ) )  break;
    }
    if( streamIndex >= USB_MAX_QUEUES )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    stack.SetResult_I4( streamIndex );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::nativeClose___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32         controllerIndex, streamIndex, stream;
    CLR_RT_HeapBlock* pThis;

    pThis = stack.This();  FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_controllerIndex ].NumericByRef().s4;
    streamIndex     = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_streamIndex     ].NumericByRef().s4;

    stream = (controllerIndex << USB_CONTROLLER_SHIFT) + streamIndex;
    if( !USB_CloseStream( stream ) )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::nativeRead___I4__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32               retVal = 0;
    CLR_INT32               controllerIndex, streamIndex, stream;
    CLR_UINT32              offset, count;
    CLR_RT_HeapBlock*       pThis;
    CLR_RT_HeapBlock_Array* array;

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_controllerIndex ].NumericByRef().s4;
    streamIndex     = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_streamIndex     ].NumericByRef().s4;
    array           = stack.Arg1().DereferenceArray(); FAULT_ON_NULL(array);
    offset          = stack.Arg2().NumericByRef().u4;
    count           = stack.Arg3().NumericByRef().u4;
    
    stream = (controllerIndex << USB_CONTROLLER_SHIFT) + streamIndex;
    if( array->m_numOfElements < (offset + count) )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    else
    {
        retVal = USB_Read( stream, (char*)array->GetFirstElement()+offset, count );
    }
    stack.SetResult_I4( retVal );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::nativeWrite___I4__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32               retVal = 0;
    CLR_INT32               controllerIndex, streamIndex, stream;
    CLR_UINT32              offset, count;
    CLR_RT_HeapBlock*       pThis;
    CLR_RT_HeapBlock_Array* array;

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_controllerIndex ].NumericByRef().s4;
    streamIndex     = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_streamIndex     ].NumericByRef().s4;
    array           = stack.Arg1().DereferenceArray(); FAULT_ON_NULL( array );
    offset          = stack.Arg2().NumericByRef().u4;
    count           = stack.Arg3().NumericByRef().u4;

    stream = (controllerIndex << USB_CONTROLLER_SHIFT) + streamIndex;
    if( array->m_numOfElements < (offset + count) )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    else
    {
        retVal = USB_Write( stream, (char*)array->GetFirstElement()+offset, count );
    }
    stack.SetResult_I4( retVal );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::nativeFlush___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32         controllerIndex, streamIndex, stream;
    CLR_RT_HeapBlock* pThis;

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_controllerIndex ].NumericByRef().s4;
    streamIndex     = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbStream::FIELD__m_streamIndex     ].NumericByRef().s4;

    stream = (controllerIndex << USB_CONTROLLER_SHIFT) + streamIndex;
    USB_Flush( stream );

    TINYCLR_NOCLEANUP();
}

