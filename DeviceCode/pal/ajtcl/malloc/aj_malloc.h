/**
 * @file  A pool based memory allocator designed for embedded systemms.
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

#include "aj_target.h"


/*
 * Indicates that allocations can be borrowed from the the next larger pool if the best-fit pool is
 * exhausted.
 */
#define AJ_POOL_BORROW   1

typedef struct _Aj_HeapConfig {
    const uint16_t size;     /* Size of the pool entries in bytes */
    const uint16_t entries;  /* Number of entries in this pool */
    const uint8_t borrow;    /* Indicates if pool can borrow from then next larger pool */
} AJ_HeapConfig;

/*
 * This should be 4 or 8
 */
#define AJ_HEAP_POOL_ROUNDING  4

/**
 * Example of a heap pool description. Note that the pool sizes must be in ascending order of size
 * and should be rounded according to AJ_HEAP_POOL_ROUNDING.

   @code

   static const AJ_HeapConfig memPools[] = {
    { 32,   1, AJ_POOL_BORROW },
    { 96,   4, },
    { 192,  1, }
   };

   @endcode

 */


/**
 * Compute the required size of the heap for the given pool list.
 *
 * @param heapConfig Description of the pools to require.
 * @param numPools   The number of different sized memory pools, maximum is 255.
 *
 * @return  Returns the total heap size required.
 */
size_t AJ_PoolRequired(const AJ_HeapConfig* heapConfig, uint8_t numPools);

/**
 * Initialize the heap.
 *
 * @param heap       Pointer to the heap storage
 * @param heapSz     Size of the heap for sanity checking
 * @param heapConfig Description of the pools to allocate. This structure must remain valid for the
 *                   lifetime of the heap
 * @param numPools   The number of different sized memory pools, maximum is 255.
 *
 * @return - AJ_OK if the heap was allocated and pools were initialized
 *         - AJ_ERR_RESOURCES of the heap is not big enough to allocate the requested pools.
 */
AJ_Status AJ_PoolInit(void* heap, size_t heapSz, const AJ_HeapConfig* heapConfig, uint8_t numPools);

/**
 * Terminate the heap
 */
void AJ_PoolTerminate(void* heap);

/**
 * Indicates if the heap has been initialized
 */
uint8_t AJ_PoolIsInitialized();

/**
 * Allocate memory
 *
 * @param sz  The size of the memory block to allocate
 *
 * @return A pointer to the allocated memory block or NULL if the request could not be satisfied.
 */
void* AJ_PoolAlloc(size_t sz);

/**
 * Free a memory block returning it to the pool from which it was allocated.
 *
 * @param mem   Pointer to the memory block to free, can be NULL
 */
void AJ_PoolFree(void* mem);

/**
 * Reallocates a memory block with a larger or smaller size. If the current block is large enough to
 * satisfy the request that block is simply returned, otherwise a new larger block is allocated, the
 * contents of the old block are copied over and the old block is freed.
 *
 * @param mem   Pointer to the memory block to reallocate, can be NULL which case this is equivalent
 *              to calling AJ_PoolAlloc.
 * @param newSz The size of the new memory block
 *
 * @return A pointer to the allocated memory block or NULL if the request could not be satisfied.
 */
void* AJ_PoolRealloc(void* mem, size_t newSz);

#ifndef NDEBUG
void AJ_PoolDump(void);
#else
#define AJ_PoolDump()
#endif
