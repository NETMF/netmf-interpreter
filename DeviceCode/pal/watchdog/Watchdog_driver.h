////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

class Watchdog_Driver
{
public:
    struct WatchdogEvent 
    {    
        HAL_DRIVER_CONFIG_HEADER   Header;
     
        //--//
        
        INT64  Time;
        INT64  Timeout;
        UINT32 Assembly;
        UINT32 Method;

        BOOL Load()
        {
            return HAL_CONFIG_BLOCK::ApplyConfig( GetDriverName(), this, sizeof(WatchdogEvent), NULL );
        }
        
        BOOL Commit()
        {
            return HAL_CONFIG_BLOCK::UpdateBlockWithName( GetDriverName(), this, sizeof(WatchdogEvent), TRUE );            
        }

        //--//

        static LPCSTR GetDriverName() { return "WATCHDOG_EVENT"; }
    };

private:
    
    BOOL              m_Enabled;
    UINT32            m_Timeout_ms;
    Watchdog_Behavior m_Behavior;
    WatchdogEvent     m_Event;

public:
    static BOOL              GetSetEnabled ( BOOL fEnable, BOOL fSet );
    static UINT32            GetSetTimeout ( INT32 timeout , BOOL fSet );
    static Watchdog_Behavior GetSetBehavior( Watchdog_Behavior behavior, BOOL fSet );
    static BOOL              LastOccurence ( WatchdogEvent& last, BOOL fSet );

    static void WatchdogCallback( void* context );
};

extern Watchdog_Driver                g_Watchdog_Driver;

