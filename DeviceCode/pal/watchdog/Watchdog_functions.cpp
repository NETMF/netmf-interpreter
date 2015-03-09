////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "Watchdog_driver.h"

//--//

BOOL Watchdog_GetSetEnabled( BOOL enabled, BOOL fSet )
{
    return Watchdog_Driver::GetSetEnabled( enabled, fSet );
}

UINT32 Watchdog_GetSetTimeout( INT32 timeout_ms , BOOL fSet )
{
    return Watchdog_Driver::GetSetTimeout( timeout_ms , fSet );
}

Watchdog_Behavior Watchdog_GetSetBehavior( Watchdog_Behavior behavior, BOOL fSet )
{
    return Watchdog_Driver::GetSetBehavior( behavior, fSet );
}

BOOL Watchdog_LastOccurence( INT64& time, INT64& timeout, UINT32& assembly, UINT32& method, BOOL fSet )
{
    GLOBAL_LOCK(irq);

    Watchdog_Driver::WatchdogEvent last; 

    if(fSet)
    {
        last.Time     = time;
        last.Timeout  = timeout;
        last.Assembly = assembly;
        last.Method   = method;
    }
    
    BOOL fRes = Watchdog_Driver::LastOccurence( last, fSet );

    if(!fSet && fRes)
    {
        time     = last.Time;
        timeout  = last.Timeout;
        assembly = last.Assembly;
        method   = last.Method;
    }

    return fRes;
}

