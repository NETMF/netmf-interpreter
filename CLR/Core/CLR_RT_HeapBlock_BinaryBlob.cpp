////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_HeapBlock_BinaryBlob::CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length, CLR_RT_MarkingHandler mark, CLR_RT_RelocationHandler relocate, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_BinaryBlob* obj = Allocate( length, flags ); CHECK_ALLOCATION(obj);

    reference.SetObjectReference( obj );

    obj->SetBinaryBlobHandlers( mark, relocate );
    obj->m_assembly = NULL;

    TINYCLR_NOCLEANUP();
}

CLR_RT_HeapBlock_BinaryBlob* CLR_RT_HeapBlock_BinaryBlob::Allocate( CLR_UINT32 length, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32 totLength = (CLR_UINT32)( sizeof(CLR_RT_HeapBlock_BinaryBlob) + length );

    CLR_RT_HeapBlock_BinaryBlob* obj;

    if(flags & CLR_RT_HeapBlock::HB_Event)
    {
        obj = EVENTCACHE_EXTRACT_NODE_AS_BYTES(g_CLR_RT_EventCache,CLR_RT_HeapBlock_BinaryBlob,DATATYPE_BINARY_BLOB_HEAD,flags,totLength);
    }
    else
    {
        obj = (CLR_RT_HeapBlock_BinaryBlob*)g_CLR_RT_ExecutionEngine.ExtractHeapBytesForObjects( DATATYPE_BINARY_BLOB_HEAD, flags, totLength );
    }

    if(obj)
    {
        obj->SetBinaryBlobHandlers( NULL, NULL );
    }

    return obj;
}

void CLR_RT_HeapBlock_BinaryBlob::Release( bool fEvent )
{
    NATIVE_PROFILE_CLR_CORE();
    SetBinaryBlobHandlers( NULL, NULL );

    if(fEvent) g_CLR_RT_EventCache.Append_Node( this );
}

void CLR_RT_HeapBlock_BinaryBlob::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_RelocationHandler relocate = BinaryBlobRelocationHandler();

    if(relocate)
    {
        relocate( this );
    }
}
