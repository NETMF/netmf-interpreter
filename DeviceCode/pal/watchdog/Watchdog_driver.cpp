////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "Watchdog_driver.h"
#include "TinyCLR_Interop.h"

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_Watchdog_Driver"
#endif

Watchdog_Driver g_Watchdog_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

//--//--//--//

BOOL Watchdog_Driver::GetSetEnabled( BOOL fEnable, BOOL fSet )
{
    BOOL current = g_Watchdog_Driver.m_Enabled;
    
    if(fSet && g_Watchdog_Driver.m_Enabled != fEnable)
    {
        if(fEnable)
        {
            ::Watchdog_Enable( g_Watchdog_Driver.m_Timeout_ms, &Watchdog_Driver::WatchdogCallback, &g_Watchdog_Driver );
        }
        else
        {
            ::Watchdog_Disable();
        }
        
        g_Watchdog_Driver.m_Enabled = fEnable;
    }
    
    return current;
}


UINT32 Watchdog_Driver::GetSetTimeout ( INT32 timeout_ms, BOOL fSet )
{
    INT32 current = g_Watchdog_Driver.m_Timeout_ms;
    
    if(fSet && g_Watchdog_Driver.m_Timeout_ms != timeout_ms)
    {
        if(g_Watchdog_Driver.m_Enabled)
        {
            ::Watchdog_Enable( timeout_ms, &Watchdog_Driver::WatchdogCallback, &g_Watchdog_Driver );
        }
        
        g_Watchdog_Driver.m_Timeout_ms = timeout_ms;
    }
    
    return current;
}

Watchdog_Behavior Watchdog_Driver::GetSetBehavior( Watchdog_Behavior behavior, BOOL fSet )
{
    Watchdog_Behavior current = g_Watchdog_Driver.m_Behavior;
    
    if(fSet && g_Watchdog_Driver.m_Behavior != behavior)
    {   
        g_Watchdog_Driver.m_Behavior = behavior;
    }
    
    return current;
}

BOOL Watchdog_Driver::LastOccurence ( WatchdogEvent& last, BOOL fSet )
{
    BOOL fRes;
    
    if(fSet)
    {
        fRes = last.Commit();
    }
    else
    {
        fRes = last.Load();
    }

    return fRes;
}

void EmulatorHook__Watchdog_Callback()
{
    Watchdog_Driver::WatchdogCallback( NULL );
}

void Watchdog_Driver::WatchdogCallback( void* context )
{
    WatchdogEvent last;

    UINT32 assmIdx = 0, methodIdx = 0;

    static int iReentrantState = 0;
    static INT64 lastTime = -1;

    INT64 currentTime = HAL_Time_CurrentTime();

    ///
    ///  If we have repeated watchdogs within the given rety timeout we will attempt to fix the problem
    ///
    if((currentTime - lastTime) < (WATCHDOG_RETRY_TIMEOUT_SECONDS * TEN_MHZ))
    {
        iReentrantState++;
    }
    else
    {
        iReentrantState = 0;
    }

    ///
    /// If we have had WATCHDOG_RETRY_COUNT watchdogs in the the allotted timeout, then we first will
    /// try increasing the watchdog timeout.  Then we wil perform a 
    /// hard reboot.
    ///
    if(iReentrantState == WATCHDOG_RETRY_COUNT) 
    {
        // add 2 second to the timeout
        GetSetTimeout( WATCHDOG_RETRY_COUNT + 2000, TRUE );
    }
    else if(iReentrantState > WATCHDOG_RETRY_COUNT)
    {
#if !defined(JTAG_DEBUGGING)
        // reboot the device to see if that clears the problem.
        Watchdog_ResetCpu();
#endif
    }

    lastTime = currentTime;

    Watchdog_ResetCounter();

    // we will ask the runtime what method was executing
    CLR_RetrieveCurrentMethod( assmIdx, methodIdx );

    last.Header.Enable = TRUE;
    last.Time          = currentTime;
    last.Timeout       = TIME_CONVERSION__TO_MILLISECONDS * g_Watchdog_Driver.m_Timeout_ms;
    last.Assembly      = assmIdx;
    last.Method        = methodIdx;
    
    LastOccurence( last, TRUE );

    switch(g_Watchdog_Driver.m_Behavior)
    {
#if !defined(JTAG_DEBUGGING)
        case Watchdog_Behavior__HardReboot:
            Watchdog_ResetCpu();
            break;
#endif
        case Watchdog_Behavior__SoftReboot:
            CLR_SoftReboot();
            break;
        case Watchdog_Behavior__EnterBooter:
            HAL_EnterBooterMode();
            break;
        case Watchdog_Behavior__DebugBreak_Managed:
            CLR_DebuggerBreak();
            break;
        case Watchdog_Behavior__DebugBreak_Native:
            Watchdog_Disable();
            while(true);
            break;
    }

    Watchdog_ResetCounter();
}

