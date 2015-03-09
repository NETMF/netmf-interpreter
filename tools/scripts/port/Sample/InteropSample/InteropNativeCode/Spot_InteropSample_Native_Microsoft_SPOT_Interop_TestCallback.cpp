////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include <TinyCLR_Interop.h>
#include "Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestCallback.h"

using namespace Microsoft::SPOT::Interop;

static bool g_TestInterruptEnalbed = false;
static CLR_RT_HeapBlock_NativeEventDispatcher *g_Context = NULL;
static UINT64 g_UserData = 0;

static HRESULT InitializeTestDriver( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT64 userData )
{
   g_Context  = pContext;
   g_UserData = userData;
   return S_OK;
}


static HRESULT EnableDisableTestDriver( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, bool fEnable )
{
   g_TestInterruptEnalbed = fEnable;
   return S_OK;
}


static HRESULT CleanupIestDriver( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )
{
    g_Context = NULL;
    g_UserData = 0;
    CleanupNativeEventsFromHALQueue( pContext );
    return S_OK;
}

static void ISR_TestProc( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )

{
    GLOBAL_LOCK(irq);
    SaveNativeEventToHALQueue( pContext, UINT32(g_UserData >> 16), UINT32(g_UserData & 0xFFFFFFFF) );
}

static const CLR_RT_DriverInterruptMethods g_InteropSampleDriverMethods = 

{ InitializeTestDriver,
  EnableDisableTestDriver,
  CleanupIestDriver
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_InteropSample_DriverProcs =
{
    "InteropSample_TestDriver", 
    DRIVER_INTERRUPT_METHODS_CHECKSUM,
    &g_InteropSampleDriverMethods
};

void TestCallback::GenerateInterrupt( HRESULT &hr )
{
    if ( g_Context == NULL )
    {
        // Generates exception if context not set.
        hr = CLR_E_DRIVER_NOT_REGISTERED;
        return; 
    }
    
    ISR_TestProc( g_Context );
}
