////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"
#include <LargeBuffer_decl.h>

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer::CreateBufferHelper( CLR_RT_HeapBlock& hbBytes, size_t size )
{
    TINYCLR_HEADER();
    
    CLR_RT_ReflectionDef_Index reflex;

    hbBytes.SetObjectReference(NULL);

    reflex.m_kind        = REFLECTION_TYPE;
    reflex.m_levels      = 1;
    reflex.m_data.m_type = g_CLR_RT_WellKnownTypes.m_UInt8;

    CLR_RT_HeapBlock_Array* pData   = (CLR_RT_HeapBlock_Array*)SimpleHeap_Allocate(size + sizeof(CLR_RT_HeapBlock_Array)); CHECK_ALLOCATION(pData);

    CLR_RT_Memory::ZeroFill(pData, size + sizeof(CLR_RT_HeapBlock_Array));

    pData->ReflectionData() = reflex;
    pData->m_fReference    = 0;
    pData->m_numOfElements = size;
    pData->m_sizeOfElement = 1;
    pData->m_typeOfElement = DATATYPE_U1;

    pData->SetDataId(CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_SZARRAY,CLR_RT_HeapBlock::HB_Unmovable, size)); 

    hbBytes.SetObjectReference(pData);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer::InternalCreateBuffer___VOID__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This(); 
    int               size  = stack.Arg1().NumericByRef().s4;

    TINYCLR_CHECK_HRESULT(CreateBufferHelper( pThis[FIELD__m_bytes], size ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer::InternalDestroyBuffer___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis   = stack.This();
    CLR_RT_HeapBlock& hbBytes = pThis[FIELD__m_bytes];

    CLR_RT_HeapBlock_Array* hbRef = hbBytes.DereferenceArray(); FAULT_ON_NULL(hbRef);

    SimpleHeap_Release(hbRef);

    hbBytes.SetObjectReference( NULL );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBufferMarshaller::MarshalBuffer___VOID__MicrosoftSPOTHardwareLargeBuffer( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_INT32 size;
    CLR_UINT16 bufferId;
    CLR_UINT8* pBytes;

    CLR_RT_HeapBlock_Array* array = NULL;

    CLR_RT_HeapBlock* pThis = stack.This();
    CLR_RT_HeapBlock* pLB   = stack.Arg1().Dereference();  FAULT_ON_NULL(pLB);

    array = pLB[Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer::FIELD__m_bytes].DereferenceArray(); FAULT_ON_NULL(array);

    pBytes   = array->GetFirstElement();
    size     = array->m_numOfElements;    
    bufferId = pThis[FIELD__m_id].NumericByRef().u2;

    LargeBuffer_ManagedToNative( bufferId, pBytes, size );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBufferMarshaller::UnMarshalBuffer___VOID__BYREF_MicrosoftSPOTHardwareLargeBuffer( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    UINT8*    pData = NULL;
    CLR_INT32 size  = 0;

    CLR_RT_HeapBlock_Array* array = NULL;
    
    CLR_RT_HeapBlock* pThis    = (CLR_RT_HeapBlock*)stack.This();
    CLR_UINT16        bufferId = pThis[FIELD__m_id].NumericByRef().u2;     
    CLR_RT_HeapBlock* pArgRef  = stack.Arg1().Dereference();  
    CLR_RT_HeapBlock* pLB      = pArgRef->Dereference(); 

    array = pLB[Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer::FIELD__m_bytes].DereferenceArray();  FAULT_ON_NULL(array);

    size = LargeBuffer_GetNativeBufferSize(bufferId);

    if(size == 0) TINYCLR_SET_AND_LEAVE(CLR_E_NO_INTERRUPT);

    if(array->m_numOfElements != size)
    {
        SimpleHeap_Release(array);       

        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer::CreateBufferHelper( pLB[Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer::FIELD__m_bytes], size ));

        array = pLB[Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer::FIELD__m_bytes].DereferenceArray(); // no need to check since we just created it
    }

    pData = array->GetFirstElement();

    LargeBuffer_NativeToManaged( bufferId, pData, size );

    TINYCLR_NOCLEANUP();
}

