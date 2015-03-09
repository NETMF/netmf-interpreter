////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include "tinyhal.h"
#include "tinyhal.h"
   
//--//

BOOL Charger_Initialize()
{
    return TRUE;
}

BOOL Charger_Uninitialize()
{
    return TRUE;
}

BOOL Charger_Status( UINT32& Status )
{
#if defined(FIQ_SAMPLING_PROFILER)
    Status = 0;
#else
    Status = CHARGER_STATUS_ON_AC_POWER | CHARGER_STATUS_CHARGE_COMPLETE;
#endif
    return TRUE;
}

BOOL Charger_Shutdown( UINT32 FlagMask )
{
    return TRUE;
}

BOOL Charger_Restart( UINT32 FlagMask )
{
    return TRUE;
}

void Charger_SetTemperature( INT16 DegreesCelcius_x10 )
{
}

