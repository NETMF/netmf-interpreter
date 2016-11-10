////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

__weak HRESULT TimeService_Initialize()
{
    return CLR_E_NOTIMPL;
}

__weak HRESULT TimeService_UnInitialize()
{
    return CLR_E_NOTIMPL;
}

__weak HRESULT TimeService_Start()
{
    return CLR_E_NOTIMPL;
}

__weak HRESULT TimeService_Stop()
{
    return CLR_E_NOTIMPL;
}

__weak HRESULT TimeService_Update(UINT32 serverIP, UINT32 tolerance, TimeService_Status* status)
{
    return CLR_E_NOTIMPL;
}

__weak HRESULT TimeService_GetLastSyncStatus(TimeService_Status* status)
{
    return CLR_E_NOTIMPL;
}

__weak HRESULT TimeService_LoadSettings(TimeService_Settings* settings)
{
    return CLR_E_NOTIMPL;
}

__weak HRESULT TimeService_SaveSettings(TimeService_Settings* settings)
{
    return CLR_E_NOTIMPL;
}
