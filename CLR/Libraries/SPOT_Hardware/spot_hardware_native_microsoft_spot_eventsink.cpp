////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "SPOT_Hardware.h"

static CLR_RT_HeapBlock_NativeEventDispatcher *g_Context = NULL;

void PostManagedEvent( UINT8 category, UINT8 subCategory, UINT16 data1, UINT32 data2 )
{
    if(g_Context != NULL)
    {
        GLOBAL_LOCK(irq);    

        UINT32 d = ((UINT32)data1 << 16) | (category << 8) | subCategory;

        SaveNativeEventToHALQueue( g_Context, d, data2 );
    }
}

static HRESULT InitializeEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT64 userData )
{
   g_Context  = pContext;

   return S_OK;
}


static HRESULT EnableDisableEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, bool fEnable )
{
   return S_OK;
}


static HRESULT CleanupEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )
{
    g_Context = NULL;

    CleanupNativeEventsFromHALQueue( pContext );

    return S_OK;
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_EventSink::EventConfig___VOID( CLR_RT_StackFrame& stack )
{
    return S_OK;
}

static const CLR_RT_DriverInterruptMethods g_CLR_AssemblyNative_Microsoft_SPOT_EventSink = 
{ 
    InitializeEventSink,
    EnableDisableEventSink,
    CleanupEventSink
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_EventSink_DriverProcs =
{
    "EventSink", 
    DRIVER_INTERRUPT_METHODS_CHECKSUM,
    &g_CLR_AssemblyNative_Microsoft_SPOT_EventSink
};


