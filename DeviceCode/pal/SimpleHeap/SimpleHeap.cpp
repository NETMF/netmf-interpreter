////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if defined(_DEBUG)
//#define SIMPLEHEAP_VERBOSE 1
#define SIMPLEHEAD_DEBUG
#endif

//#define SIMPLEHEAD_DEBUG
//#define SIMPLEHEAP_VERBOSE 1

#define SIMPLE_HEAP_GUARDWORD 1

struct BlockHeader
{
    struct BlockHeader* next;
    struct BlockHeader* prev;
    UINT32              length;
    UINT32              signature;
#if defined(SIMPLE_HEAP_GUARDWORD)
    UINT32              head_guard;
#endif
};

static const UINT32 c_Free = 0xDEADBEEF;
static const UINT32 c_Busy = 0xDEADBE5E;
#if defined(SIMPLE_HEAP_GUARDWORD)
static const UINT32 c_Guard = 0xbab1f00d;
#endif

#if defined(SIMPLEHEAP_VERBOSE)
#if defined(PATCH_BUILD)
const UINT32 g_heapmem_used = 0;
#else
UINT32 g_heapmem_used = 0;
#endif  // defined(PATCH_BUILD)
#endif

#if defined(PATCH_BUILD)
static struct BlockHeader * const g_s_FirstCluster = NULL;
#else
static struct BlockHeader *g_s_FirstCluster;
#endif  // defined(PATCH_BUILD)

//--//

void SimpleHeap_Initialize( void* buffer, UINT32 length )
{
    struct BlockHeader **s_FirstCluster = (struct BlockHeader **) &g_s_FirstCluster;

    *s_FirstCluster = (struct BlockHeader *) buffer;

    (*s_FirstCluster)->length     = length;
    (*s_FirstCluster)->next       = NULL;
    (*s_FirstCluster)->prev       = NULL;
    (*s_FirstCluster)->signature  = c_Free;
#if defined(SIMPLE_HEAP_GUARDWORD)
    (*s_FirstCluster)->head_guard = c_Guard;
#endif
}

//--//

BOOL SimpleHeap_IsAllocated( void *ptr )
{
    if(ptr)
    {
        struct BlockHeader* blk = (struct BlockHeader*)ptr; blk--;

        if(blk->signature == c_Busy)
        {
            return TRUE;
        }
        else if(blk->signature == c_Free)
        {
            return FALSE;
        }
        else
        {
            // corrupt pointer to memory
            hal_printf( "SimpleHeap: Bad Ptr: 0x%08x\r\n", (size_t)ptr );
            ASSERT(0);
            return FALSE;
        }
    }

    return FALSE;
}

//--//

void SimpleHeap_Release( void* ptr )
{
#if defined(SIMPLEHEAP_VERBOSE)
    UINT32 *p_heapmem_used = (UINT32 *) &g_heapmem_used;
#endif  // defined(SIMPLEHEAP_VERBOSE)

#if defined(SIMPLEHEAP_VERBOSE)
    //printf("SimpleHeap_Release(0x%08x)\r\n", (UINT32) ptr);
#endif

    if(ptr)
    {
        struct BlockHeader* next;
        struct BlockHeader* prev;
        struct BlockHeader* blk = (struct BlockHeader*)ptr; blk--;

#if defined(SIMPLEHEAD_DEBUG)
        if(blk->signature != c_Busy)
        {
            if(blk->signature != c_Free)
            {
                hal_printf( "    Memory(%08x): CORRUPTION: %08x %08x\r\n", (UINT32) ptr, blk->signature, blk->length );
            }
            else
            {
                hal_printf( "    Memory(%08x): RELEASE TWICE: %08x\r\n", (UINT32) ptr, blk->length );
            }
            HARD_BREAKPOINT();
        }
#endif

#if defined(SIMPLE_HEAP_GUARDWORD)
        if(blk->head_guard != c_Guard)
        {
            hal_printf( "SimpleHeap_Release: Memory block head corruption: %08x %08x\r\n", blk->head_guard, blk->length );
            HARD_BREAKPOINT();
        }

        if(*(UINT32*)((UINT32)&blk[0] + blk->length - sizeof( c_Guard )) != c_Guard)
        {
            hal_printf( "SimpleHeap_Release: Memory block tail corruption: %08x %08x\r\n", *(UINT32 *)((UINT32)&blk[0] + blk->length - sizeof( c_Guard )), blk->length );
            HARD_BREAKPOINT();
        }
#endif

        //--//

        blk->signature = c_Free;

#if defined(SIMPLEHEAP_VERBOSE)
        *p_heapmem_used -= blk->length;

        hal_printf("F:%4d:%08x T:%6d\r\n", blk->length, (UINT32) ptr, *p_heapmem_used);
#endif
        //
        // First merge with the following block, if free.
        //
        next = blk->next;
        if(next && next->signature == c_Free)
        {
            struct BlockHeader* nextnext;

            blk->length += next->length;

            nextnext = next->next;

            blk->next = nextnext;

            if(nextnext) nextnext->prev = blk;
        }

        //
        // Then merge with the preceding block, if free.
        //
        prev = blk->prev;
        if(prev && prev->signature == c_Free)
        {
            prev->length += blk->length;

            next = blk->next;

            prev->next = next;

            if(next) next->prev = prev;
        }
    }
    else
    {
        // releasing a null pointer is OK
        //ASSERT(0);
    }
}

//--//

void* SimpleHeap_Allocate( size_t len )
{
    struct BlockHeader **pptr = (struct BlockHeader **) &g_s_FirstCluster;
    struct BlockHeader *ptr = *pptr;
#if defined(SIMPLEHEAP_VERBOSE)
    UINT32 *p_heapmem_used = (UINT32 *) &g_heapmem_used;
#endif  // defined(SIMPLEHEAP_VERBOSE)

#if defined(SIMPLEHEAP_VERBOSE)
    //printf("SimpleHeap_Allocate(%d)\r\n", len);

    //Events_WaitForEventsInternal(0, 30);    // delay 30mSec to simulate a long GC/compact allocation
#endif

    len += sizeof( struct BlockHeader );
#if defined(SIMPLE_HEAP_GUARDWORD)
    len += sizeof( c_Guard );   // leave room for tail guard
#endif
    len  = (len + sizeof(UINT32) - 1) & ~(sizeof(UINT32)-1);

    while(ptr)
    {
        if(ptr->signature == c_Free && ptr->length >= len)
        {
            if(ptr->length <= (len + sizeof(struct BlockHeader)*2))
            {
                //
                // Perfect or almost perfect fit, nothing else to do.
                //

                // note that ptr->length is set to block size, not allocated size,
                // which then gets lost, so we can only check tail guard
                // based on actual size, not requested size, blah...
                break;
            }
            else
            {
                //
                // Split the block in two.
                //
                struct BlockHeader* prev;
                struct BlockHeader* next;

                prev = ptr;
                ptr = (struct BlockHeader*)((UINT8*)ptr + ptr->length - len);
                next = prev->next;

                ptr->next = next;
                ptr->prev = prev;

                         prev->next = ptr;
                if(next) next->prev = ptr;

                ptr ->length  = (int)len;
                prev->length -= (int)len;
                break;
            }
        }

        ptr = ptr->next;
    }

    if(ptr)
    {
        ptr->signature  = c_Busy;
#if defined(SIMPLE_HEAP_GUARDWORD)
        ptr->head_guard = c_Guard;                                          // set the head guard word
        *(UINT32 *)((UINT32)&ptr[0] + ptr->length - sizeof( c_Guard )) = c_Guard;   // set the tail guard word
#endif

#if defined(SIMPLEHEAP_VERBOSE)
        *p_heapmem_used += ptr->length;

        hal_printf("A:%4d:%08x T:%6d\r\n", len, (UINT32) &ptr[1], *p_heapmem_used);
#endif
        return &ptr[1];
    }
    else
    {
        hal_printf( "    Memory: OUTOFMEMORY!! %d\r\n", len );

#if defined(SIMPLEHEAP_VERBOSE)
        hal_printf("SimpleHeap_Allocate(%d)=0x%08x\r\n", len, NULL);
#endif

        return NULL;
    }
}

//--//

void* SimpleHeap_ReAllocate( void* ptr, size_t len )
{
    void* p = SimpleHeap_Allocate( len ); if(!p) return NULL;

     if(ptr)
     {
        struct BlockHeader* blk = (struct BlockHeader*)ptr; blk--;

        memcpy( p, ptr, len > blk->length ? blk->length : len );

        SimpleHeap_Release( ptr );

        ptr = p;
    }
    
    return p;
}

