////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_hardware_serial.h"


//--// Event callback 

static void PostUsartEvent( void* pContext, unsigned int event )
{
    if(pContext)
    {
        GLOBAL_LOCK(irq);    
        
        SaveNativeEventToHALQueue( (CLR_RT_HeapBlock_NativeEventDispatcher*)pContext, event, 0 );
    }
}

//--//  Serial Port Data Event 

static HRESULT InitializeUsartDataEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT64 userData )
{
    UINT32 port = (UINT32)userData;
    void*  arg  = NULL;

    if(pContext)
    {
        if(!USART_ConnectEventSink( port, USART_EVENT_TYPE_DATA, pContext, NULL, &arg )) return CLR_E_INVALID_PARAMETER;

        pContext->m_pDrvCustomData = arg;

        return S_OK;
    }

    return CLR_E_FAIL;
}

static HRESULT EnableDisableUsartDataEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, bool fEnable )
{
    UINT32 port;

    if(pContext)
    {
        port = (UINT32)(pContext->m_pDrvCustomData);

        if(!USART_ConnectEventSink( port, USART_EVENT_TYPE_DATA, pContext, (fEnable ? PostUsartEvent : NULL), NULL )) return CLR_E_INVALID_PARAMETER;
    }

    return S_OK;
}

static HRESULT CleanupUsartDataEventSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )
{
    UINT32 port;

    if(pContext)
    {
        port = (UINT32)(pContext->m_pDrvCustomData);

        CleanupNativeEventsFromHALQueue( pContext );

        if(!USART_ConnectEventSink( port, USART_EVENT_TYPE_DATA, NULL, NULL, NULL )) return CLR_E_INVALID_PARAMETER;
    }

    return S_OK;
}

static const CLR_RT_DriverInterruptMethods g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_UsartEventSink = 
{ 
    InitializeUsartDataEventSink,
    EnableDisableUsartDataEventSink,
    CleanupUsartDataEventSink
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_UsartEvent =
{
    "SerialPortDataEvent", 
    DRIVER_INTERRUPT_METHODS_CHECKSUM,
    &g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_UsartEventSink
};

//--//  Serial Port Error Event (parity error)

static HRESULT InitializeUsartErrorSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT64 userData )
{
    UINT32 port = (UINT32)userData;
    void *arg   = NULL;

    if(pContext)
    {
        if(!USART_ConnectEventSink( port, USART_EVENT_TYPE_ERROR, pContext, NULL, &arg )) return CLR_E_INVALID_PARAMETER;

        pContext->m_pDrvCustomData = arg;

        return S_OK;
    }

    return CLR_E_FAIL;
}


static HRESULT EnableDisableUsartErrorSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, bool fEnable )
{
    UINT32 port;

    if(pContext)
    {
        port = (UINT32)(pContext->m_pDrvCustomData);

        if(!USART_ConnectEventSink( port, USART_EVENT_TYPE_ERROR, pContext, (fEnable ? PostUsartEvent : NULL), NULL )) return CLR_E_INVALID_PARAMETER;
    }

    return S_OK;
}


static HRESULT CleanupUsartErrorSink( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )
{
    UINT32 port;

    if(pContext)
    {
        port = (UINT32)(pContext->m_pDrvCustomData);

        CleanupNativeEventsFromHALQueue( pContext );

        if(!USART_ConnectEventSink( port, USART_EVENT_TYPE_ERROR, NULL, NULL, NULL )) return CLR_E_INVALID_PARAMETER;
    }

    return S_OK;
}

static const CLR_RT_DriverInterruptMethods g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_UsartErrorSink = 
{ 
    InitializeUsartErrorSink,
    EnableDisableUsartErrorSink,
    CleanupUsartErrorSink
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_UsartError =
{
    "SerialPortErrorEvent", 
    DRIVER_INTERRUPT_METHODS_CHECKSUM,
    &g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_UsartErrorSink
};

