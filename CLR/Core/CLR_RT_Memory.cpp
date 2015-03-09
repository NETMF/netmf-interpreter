////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_TRACE_MALLOC)
static CLR_UINT32 s_TotalAllocated;
#endif

CLR_RT_MemoryRange s_CLR_RT_Heap = { 0, 0 };

static int s_PreHeapInitIndex = 0;

////////////////////////////////////////////////////////////

HAL_DECLARE_CUSTOM_HEAP( CLR_RT_Memory::Allocate, CLR_RT_Memory::Release, CLR_RT_Memory::ReAllocate );

//--//

void CLR_RT_Memory::Reset()
{
    NATIVE_PROFILE_CLR_CORE();
    ::HeapLocation( s_CLR_RT_Heap.m_location, s_CLR_RT_Heap.m_size );

#if defined(TINYCLR_TRACE_MALLOC)
    s_TotalAllocated = 0;
#endif
}

void* CLR_RT_Memory::SubtractFromSystem( size_t len )
{
    NATIVE_PROFILE_CLR_CORE();
    len = ROUNDTOMULTIPLE(len,CLR_UINT32);

    if(len <= s_CLR_RT_Heap.m_size)
    {
        s_CLR_RT_Heap.m_size -= (CLR_UINT32)len;

        return &s_CLR_RT_Heap.m_location[ s_CLR_RT_Heap.m_size ];
    }

    return NULL;
}

//--//

#if defined(TINYCLR_FILL_MEMORY_WITH_DIRTY_PATTERN)

#define DEBUG_POINTER_INCREMENT(ptr,size) ptr = (void*)((char*)ptr + (size))
#define DEBUG_POINTER_DECREMENT(ptr,size) ptr = (void*)((char*)ptr - (size))

const CLR_UINT32 c_extra = sizeof(CLR_RT_HeapBlock) * 2;

#endif


void CLR_RT_Memory::Release( void* ptr )
{
    NATIVE_PROFILE_CLR_CORE();

    // CLR heap not initialized yet, return (this is not an error condition because we allow pre
    if(s_CLR_RT_Heap.m_size == 0)
    {
        return;
    }

    if(ptr)
    {
#if defined(TINYCLR_FILL_MEMORY_WITH_DIRTY_PATTERN)
        DEBUG_POINTER_DECREMENT(ptr,c_extra+sizeof(CLR_UINT32));
#endif

        CLR_RT_HeapBlock_BinaryBlob* pThis = CLR_RT_HeapBlock_BinaryBlob::GetBlob( ptr );

        if(pThis->DataType() != DATATYPE_BINARY_BLOB_HEAD)
        {
            TINYCLR_STOP();
        }
        else
        {
#if defined(TINYCLR_FILL_MEMORY_WITH_DIRTY_PATTERN)
            CLR_UINT32 len = *(CLR_UINT32*)ptr;
            CLR_UINT8* blk;
            CLR_UINT32 pos;

            for(pos=0, blk=(CLR_UINT8*)ptr + sizeof(len); pos<c_extra; pos++, blk++)
            {
                if(*blk != 0xDD)
                {
                    CLR_Debug::Printf( "CLR_RT_Memory::Release: HEAP OVERRUN START %08x(%d) = %08x\r\n", ptr, len, blk );
                    TINYCLR_STOP();
                }
            }

            for(pos=0, blk=(CLR_UINT8*)ptr + len - c_extra; pos<c_extra; pos++, blk++)
            {
                if(*blk != 0xDD)
                {
                    CLR_Debug::Printf( "CLR_RT_Memory::Release: HEAP OVERRUN END %08x(%d) = %08x\r\n", ptr, len, blk );
                    TINYCLR_STOP();
                }
            }
#endif

#if defined(TINYCLR_TRACE_MALLOC)
            s_TotalAllocated -= pThis->DataSize();
            CLR_Debug::Printf( "CLR_RT_Memory::Release : %08x = %3d blocks (tot %4d)\r\n", ptr, pThis->DataSize(), s_TotalAllocated );
#endif

            pThis->Release( true );
        }
    }
}

void* CLR_RT_Memory::Allocate( size_t len, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_CORE();

    if(s_CLR_RT_Heap.m_size == 0)
    {
        UINT8* heapStart = NULL;
        UINT32 heapSize  = 0;

        ::HeapLocation( heapStart, heapSize );

        if(len > heapSize)
        {
            ASSERT(FALSE);
            return NULL;

        }

        // use the current index to prevent heap thrashing before initialization
        heapStart = &heapStart[ s_PreHeapInitIndex ];

        s_PreHeapInitIndex += len;
        
        return heapStart;
    }
    
    flags |= CLR_RT_HeapBlock::HB_Event;

#if defined(TINYCLR_FILL_MEMORY_WITH_DIRTY_PATTERN)
    len += c_extra * 2 + sizeof(CLR_UINT32);
#endif

    CLR_RT_HeapBlock_BinaryBlob* obj = CLR_RT_HeapBlock_BinaryBlob::Allocate( (CLR_UINT32)len, flags );
    if(obj)
    {
        void* res = obj->GetData();

#if defined(TINYCLR_TRACE_MALLOC)
        s_TotalAllocated += obj->DataSize();
        CLR_Debug::Printf( "CLR_RT_Memory::Allocate: %08x = %3d blocks (tot %4d), %d bytes\r\n", res, obj->DataSize(), s_TotalAllocated, len );
#endif

#if defined(TINYCLR_FILL_MEMORY_WITH_DIRTY_PATTERN)
        memset( res, 0xDD, len );

        *(CLR_UINT32*)res = (CLR_UINT32)len;

        DEBUG_POINTER_INCREMENT(res,c_extra+sizeof(CLR_UINT32));
#endif

        return res;
    }
    
    return NULL;
}

void* CLR_RT_Memory::Allocate_And_Erase( size_t len, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_CORE();
    void* ptr = CLR_RT_Memory::Allocate( len, flags );

    if(ptr) ZeroFill( ptr, len );

    return ptr;
}


void* CLR_RT_Memory::ReAllocate( void* ptr, size_t len )
{
    NATIVE_PROFILE_CLR_CORE();
    
    // allocate always as an event but do not run GC on failure
    void* p = CLR_RT_Memory::Allocate( len, CLR_RT_HeapBlock::HB_Event | CLR_RT_HeapBlock::HB_NoGcOnFailedAllocation); if(!p) return NULL;
    
    if(ptr)
    {
        CLR_RT_HeapBlock_BinaryBlob* pThis = CLR_RT_HeapBlock_BinaryBlob::GetBlob( ptr );

        size_t prevLen = pThis->DataSize() * sizeof(CLR_RT_HeapBlock);

        memcpy( p, ptr, len > prevLen ? prevLen : len );

        CLR_RT_Memory::Release( ptr );

        ptr = p;
    }

    return p;    
}

