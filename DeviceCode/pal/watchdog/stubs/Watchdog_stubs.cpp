////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

__weak BOOL Watchdog_GetSetEnabled( BOOL enabled, BOOL fSet )
{
    return FALSE;
}

__weak UINT32 Watchdog_GetSetTimeout( INT32 timeout_ms , BOOL fSet )
{
    return 0;
}

__weak Watchdog_Behavior Watchdog_GetSetBehavior( Watchdog_Behavior behavior, BOOL fSet )
{
    return Watchdog_Behavior__None;
}

__weak BOOL Watchdog_LastOccurence( INT64& time, INT64& timeout, UINT32& assembly, UINT32& method, BOOL fSet )
{
    return FALSE;
}


__weak void EmulatorHook__Watchdog_Callback()
{
}
