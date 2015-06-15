/**
 * @file
 */
/******************************************************************************
 * Copyright AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE MALLOC

#include <stdio.h>
#include <assert.h>

#include "aj_target.h"
#include "aj_debug.h"
#include "aj_malloc.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgMALLOC = 0;
#endif

typedef struct {
    void* endOfPool; /* Address of end of this pool */
    void* freeList;  /* Free list for this pool */
#ifndef NDEBUG
    uint16_t use;    /* Number of entries in use */
    uint16_t hwm;    /* High-water mark */
    uint16_t max;    /* Max allocation from this pool */
#endif
} Pool;

typedef struct _MemBlock {
    struct _MemBlock* next;
    uint8_t mem[sizeof(void*)];
} MemBlock;

static const AJ_HeapConfig* heapConfig;
static Pool* heapPools;
static uint8_t numPools;
static uint8_t* heapStart;

size_t AJ_PoolRequired(const AJ_HeapConfig* poolConfig, uint8_t numPools)
{
    size_t heapSz = sizeof(Pool) * numPools;
    uint8_t i;

    for (i = 0; i < numPools; ++i) {
        size_t sz = poolConfig[i].size;
        sz += AJ_HEAP_POOL_ROUNDING - (sz & (AJ_HEAP_POOL_ROUNDING - 1));
        heapSz += sz * poolConfig[i].entries;
    }
    return heapSz;
}

void AJ_PoolTerminate(void* heap)
{
    heapPools = NULL;
}

AJ_Status AJ_PoolInit(void* heap, size_t heapSz, const AJ_HeapConfig* poolConfig, uint8_t num)
{
    uint8_t i;
    uint16_t n;
    uint8_t* heapEnd = (uint8_t*)heap + sizeof(Pool) * num;
    Pool* p = (Pool*)heap;

    heapPools = p;
    heapStart = heapEnd;
    heapConfig = poolConfig;
    numPools = num;

    /*
     * Clear the heap pool info  block
     */
    memset(heapPools, 0, heapStart - (uint8_t*)heap);

    for (i = 0; i < numPools; ++i, ++p) {
        size_t sz = poolConfig[i].size;
        sz += AJ_HEAP_POOL_ROUNDING - (sz & (AJ_HEAP_POOL_ROUNDING - 1));
        /*
         * Add all blocks to the pool free list
         */
        for (n = poolConfig[i].entries; n != 0; --n) {
            MemBlock* block = (MemBlock*)heapEnd;
            if (sz > heapSz) {
                AJ_ErrPrintf(("Heap is too small for the requested pool allocations\n"));
                return AJ_ERR_RESOURCES;
            }
            block->next = (MemBlock*)p->freeList;
            p->freeList = (void*)block;
            heapEnd += sz;
            heapSz -= sz;
        }
        /*
         * Save end of pool pointer for use by AJ_PoolFree
         */
        p->endOfPool = (void*)heapEnd;
#ifndef NDEBUG
        p->use = 0;
        p->hwm = 0;
        p->max = 0;
#endif
    }
    return AJ_OK;
}

uint8_t AJ_PoolIsInitialized()
{
    return heapPools != NULL;
}

void* AJ_PoolAlloc(size_t sz)
{
    Pool* p = heapPools;
    uint8_t i;

    if (!p) {
        AJ_ErrPrintf(("Heap not initialized\n"));
        return NULL;
    }
    /*
     * Find pool that can satisfy the allocation
     */
    for (i = 0; i < numPools; ++i, ++p) {
        if (sz <= heapConfig[i].size) {
            MemBlock* block = (MemBlock*)p->freeList;
            if (!block) {
                /*
                 * Are we allowed to borrowing from next pool?
                 */
                if (heapConfig[i].borrow) {
                    continue;
                }
                break;
            }
            AJ_InfoPrintf(("AJ_PoolAlloc pool[%d] allocated %d\n", heapConfig[i].size, (int)sz));
            p->freeList = block->next;
#ifndef NDEBUG
            ++p->use;
            p->hwm = max(p->use, p->hwm);
            p->max = max(p->max, sz);
#endif
            return (void*)block;
        }
    }
    AJ_ErrPrintf(("AJ_PoolAlloc of %d bytes failed\n", (int)sz));
    AJ_PoolDump();
    return NULL;
}

void AJ_PoolFree(void* mem)
{
    Pool* p = heapPools;
    uint8_t i;

    if (mem) {
        assert((ptrdiff_t)mem >= (ptrdiff_t)heapStart);
        /*
         * Locate the pool from which the released memory was allocated
         */
        for (i = 0; i < numPools; ++i, ++p) {
            if ((ptrdiff_t)mem < (ptrdiff_t)p->endOfPool) {
                MemBlock* block = (MemBlock*)mem;
                block->next = (MemBlock*)p->freeList;
                p->freeList = block;
                AJ_InfoPrintf(("AJ_PoolFree pool[%d]\n", heapConfig[i].size));
#ifndef NDEBUG
                --p->use;
#endif
                break;
            }
        }
        assert(i < numPools);
    }
}

void* AJ_PoolRealloc(void* mem, size_t newSz)
{
    Pool* p = heapPools;
    uint8_t i;

    if (mem) {
        assert((ptrdiff_t)mem >= (ptrdiff_t)heapStart);
        /*
         * Locate the pool from which the released memory was allocated
         */
        for (i = 0; i < numPools; ++i, ++p) {
            if ((ptrdiff_t)mem < (ptrdiff_t)p->endOfPool) {
                size_t oldSz = heapConfig[i].size;
                /*
                 * Don't need to do anything if the same block would be reused
                 */
                if ((newSz <= oldSz) && ((i == 0) || (newSz > heapConfig[i - 1].size))) {
                    AJ_InfoPrintf(("AJ_Realloc pool[%d] %d bytes in place\n", (int)oldSz, (int)newSz));
                } else {
                    MemBlock* block = (MemBlock*)mem;
                    AJ_InfoPrintf(("AJ_Realloc pool[%d] by AJ_Alloc(%d)\n", (int)oldSz, (int)newSz));
                    mem = AJ_PoolAlloc(newSz);
                    if (mem) {
                        memcpy(mem, (void*)block, min(oldSz, newSz));
                        /*
                         * Put old block on the free list
                         */
                        block->next = (MemBlock*)p->freeList;
                        p->freeList = block;
#ifndef NDEBUG
                        --p->use;
#endif
                    }
                }
                return mem;
            }
        }
    } else {
        return AJ_PoolAlloc(newSz);
    }
    AJ_ErrPrintf(("AJ_Realloc of %d bytes failed\n", (int)newSz));
    AJ_PoolDump();
    return NULL;
}

#ifndef NDEBUG
void AJ_PoolDump(void)
{
    uint8_t i;
    size_t memUse = 0;
    size_t memHigh = 0;
    size_t memTotal = 0;
    Pool* p = heapPools;

    AJ_AlwaysPrintf(("======= dump of %d heap pools ======\n", numPools));
    for (i = 0; i < numPools; ++i, ++p) {
        AJ_AlwaysPrintf(("pool[%d] used=%d free=%d high-water=%d max-alloc=%d\n", heapConfig[i].size, p->use, heapConfig[i].entries - p->use, p->hwm, p->max));
        memUse += p->use * heapConfig[i].size;
        memHigh += p->hwm * heapConfig[i].size;
        memTotal += p->hwm * p->max;
    }
    AJ_AlwaysPrintf(("======= heap hwm = %d use = %d waste = %d ======\n", (int)memHigh, (int)memUse, (int)(memHigh - memTotal)));
}
#endif
