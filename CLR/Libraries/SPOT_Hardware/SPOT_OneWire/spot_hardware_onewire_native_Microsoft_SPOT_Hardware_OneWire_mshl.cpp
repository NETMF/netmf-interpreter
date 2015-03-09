////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "spot_hardware_onewire_native.h"
#include "spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire.h"

using namespace Microsoft::SPOT::Hardware;


HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::TouchReset___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 retVal = OneWire::TouchReset( pMngObj,  hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::TouchBit___I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32( stack, 1, param0 ) );

        INT32 retVal = OneWire::TouchBit( pMngObj,  param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::TouchByte___I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32( stack, 1, param0 ) );

        INT32 retVal = OneWire::TouchByte( pMngObj,  param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::WriteByte___I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32( stack, 1, param0 ) );

        INT32 retVal = OneWire::WriteByte( pMngObj,  param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::ReadByte___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 retVal = OneWire::ReadByte( pMngObj,  hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::AcquireEx___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 retVal = OneWire::AcquireEx( pMngObj,  hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::Release___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 retVal = OneWire::Release( pMngObj,  hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::FindFirstDevice___I4__BOOLEAN__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT8 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 1, param0 ) );

        INT8 param1;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 2, param1 ) );

        INT32 retVal = OneWire::First( pMngObj,  param0, param1, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::FindNextDevice___I4__BOOLEAN__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT8 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 1, param0 ) );

        INT8 param1;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 2, param1 ) );

        INT32 retVal = OneWire::Next( pMngObj,  param0, param1, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire::SerialNum___I4__SZARRAY_U1__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        CLR_RT_TypedArray_UINT8 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8_ARRAY( stack, 1, param0 ) );

        INT8 param1;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 2, param1 ) );

        INT32 retVal = OneWire::SerialNum( pMngObj,  param0, param1, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}
