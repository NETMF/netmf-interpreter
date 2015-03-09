////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_CACHE_DECL_H_
#define _DRIVERS_CACHE_DECL_H_ 1

//--//

// if the implementation does not want to handle caching details, 
// e.g. when running on a higher level OS such as Windows or WINCE, 
// then return the same address
size_t CPU_GetCachableAddress  ( size_t address );
size_t CPU_GetUncachableAddress( size_t address );

template <typename T> void CPU_InvalidateAddress( T* address );

void CPU_FlushCaches      ();
void CPU_DrainWriteBuffers();
void CPU_InvalidateCaches ();
void CPU_EnableCaches     ();
void CPU_DisableCaches    ();

//--//

template <typename T> FORCEINLINE T* CPU_GetCachableAddress( T* address )
{
    return (T*)CPU_GetCachableAddress( (size_t)address );
}

template <typename T> FORCEINLINE T* CPU_GetUncachableAddress( T* address )
{
    return (T*)CPU_GetUncachableAddress( (size_t)address );
}

//--//

#endif // _DRIVERS_CACHE_DECL_H_

