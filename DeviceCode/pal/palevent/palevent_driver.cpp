////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "palevent_driver.h"

PalEventDriver g_palEventDriver;

BOOL PalEventDriver::s_initialized = FALSE;

HRESULT PalEventDriver::Initialize()
{
    if (!g_palEventDriver.s_initialized)
    {
        g_palEventDriver.m_listenerList.Initialize();
        PalEventDriver::s_initialized = TRUE;
    }

    return S_OK;
}

HRESULT PalEventDriver::Uninitialize()
{
    if (PalEventDriver::s_initialized)
    {
        PalEventDriver::s_initialized = FALSE;

        /// Remove all nodes.
        g_palEventDriver.m_listenerList.Initialize();
    }

    return S_OK;
}

HRESULT PalEventDriver::PostEvent(unsigned int e, unsigned int param)
{
    PalEventListener *listener = g_palEventDriver.m_listenerList.FirstNode();
        
    while(listener->Next() != NULL)
    {
        if (listener->m_eventMask & e)
        {
            listener->m_palEventListener(e, param);
        }

        listener = listener->Next();
    }

    return S_OK;
}

HRESULT PalEventDriver::EnlistListener(PalEventListener* listener)
{
    listener->Initialize();
    g_palEventDriver.m_listenerList.LinkAtBack(listener);

    return S_OK;
}

