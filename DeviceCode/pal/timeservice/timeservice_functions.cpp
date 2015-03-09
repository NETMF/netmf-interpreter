////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "TimeService_driver.h"

HRESULT TimeService_Initialize()
{
    return g_TimeServiceDriver.Initialize();
}

HRESULT TimeService_UnInitialize()
{
    return g_TimeServiceDriver.Uninitialize();
}

HRESULT TimeService_Start()
{
    return g_TimeServiceDriver.Start();
}

HRESULT TimeService_Stop()
{
    return g_TimeServiceDriver.Stop();
}

HRESULT TimeService_Update(UINT32 serverIP, UINT32 tolerance, TimeService_Status* status)
{
    return g_TimeServiceDriver.Update(serverIP, tolerance, status);
}

HRESULT TimeService_GetLastSyncStatus(TimeService_Status* status)
{
    return g_TimeServiceDriver.GetLastSyncStatus(status);
}

HRESULT TimeService_LoadSettings(TimeService_Settings* settings)
{
    return g_TimeServiceDriver.LoadSettings(settings);
}

HRESULT TimeService_SaveSettings(TimeService_Settings* settings)
{
    return g_TimeServiceDriver.SaveSettings(settings);
}

