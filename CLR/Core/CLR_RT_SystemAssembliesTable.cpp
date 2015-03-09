////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyCLR_Runtime.h>
#include <corlib_native.h>
#include <SPOT_native.h>
#include "TinyCLR_Interop.h"

static const CLR_RT_NativeAssemblyData *LookUpAssemblyNativeDataByName
(
    const CLR_RT_NativeAssemblyData **pAssembliesNativeData,
    const char *lpszAssemblyName
)
{
    // Just sanity check to avoid crash in strcmp if name is NULL.
    if ( lpszAssemblyName == NULL )
    {
        return NULL;
    }

    // Loops in all entries and looks for the CLR_RT_NativeAssemblyData with name same as lpszAssemblyName
    for ( int i = 0; pAssembliesNativeData[ i ]; i++ )
    {
        if ( pAssembliesNativeData[ i ] != NULL  &&  0 == strcmp( lpszAssemblyName, pAssembliesNativeData[ i ]->m_szAssemblyName ) )
        {
            return pAssembliesNativeData[ i ];
        }
    }
    return NULL;
}


const CLR_RT_NativeAssemblyData *GetAssemblyNativeData( const char *lpszAssemblyName )
{
    extern const CLR_RT_NativeAssemblyData *g_CLR_InteropAssembliesNativeData[];

    // This will return NULL if there is no registered interop assembly of that name
    return LookUpAssemblyNativeDataByName(
            g_CLR_InteropAssembliesNativeData,
            lpszAssemblyName
            );
}

