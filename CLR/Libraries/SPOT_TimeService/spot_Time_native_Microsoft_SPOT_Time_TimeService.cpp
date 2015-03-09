
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_Time_native.h"
#include "spot_time.h"

typedef Library_spot_Time_native_Microsoft_SPOT_Time_TimeServiceSettings ManagedSettings;
typedef Library_spot_Time_native_Microsoft_SPOT_Time_TimeServiceStatus   ManagedStatus;

HRESULT Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::get_Settings___STATIC__MicrosoftSPOTTimeTimeServiceSettings( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    TimeService_Settings    settings;
    CLR_RT_HeapBlock&       top = stack.PushValueAndClear();
    CLR_RT_HeapBlock*       managedSettings = NULL;

    TINYCLR_CHECK_HRESULT(TimeService_LoadSettings(&settings));    

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( top, g_CLR_RT_WellKnownTypes.m_TimeServiceSettings ));
    managedSettings = top.Dereference();

    managedSettings[ ManagedSettings::FIELD__PrimaryServerIP     ].SetInteger( settings.PrimaryServerIP );
    managedSettings[ ManagedSettings::FIELD__AlternateServerIP   ].SetInteger( settings.AlternateServerIP );
    managedSettings[ ManagedSettings::FIELD__RefreshTime         ].SetInteger( settings.RefreshTime );
    managedSettings[ ManagedSettings::FIELD__Tolerance           ].SetInteger( settings.Tolerance );
    managedSettings[ ManagedSettings::FIELD__ForceSyncAtWakeUp   ].SetBoolean( 0 != (settings.Flags & TimeService_Settings_Flags_ForceSyncAtWakeUp) );
    managedSettings[ ManagedSettings::FIELD__AutoDayLightSavings ].SetBoolean( 0 != (settings.Flags & TimeService_Settings_Flags_AutoDST) );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::set_Settings___STATIC__VOID__MicrosoftSPOTTimeTimeServiceSettings( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    TimeService_Settings    settings;
    CLR_RT_HeapBlock* managedSettings = stack.Arg0().Dereference();  FAULT_ON_NULL(managedSettings);    
    
    settings.Flags = 0;
    settings.PrimaryServerIP   = managedSettings[ ManagedSettings::FIELD__PrimaryServerIP     ].NumericByRef().u4;
    settings.AlternateServerIP = managedSettings[ ManagedSettings::FIELD__AlternateServerIP   ].NumericByRef().u4;
    settings.RefreshTime       = managedSettings[ ManagedSettings::FIELD__RefreshTime         ].NumericByRef().u4;
    settings.Tolerance         = managedSettings[ ManagedSettings::FIELD__Tolerance           ].NumericByRef().u4;
    settings.Flags            |=(managedSettings[ ManagedSettings::FIELD__ForceSyncAtWakeUp   ].NumericByRef().s1 == 0) ? 0 : TimeService_Settings_Flags_ForceSyncAtWakeUp;
    settings.Flags            |=(managedSettings[ ManagedSettings::FIELD__AutoDayLightSavings ].NumericByRef().s1 == 0) ? 0 : TimeService_Settings_Flags_AutoDST;

    TINYCLR_SET_AND_LEAVE(TimeService_SaveSettings( &settings ));    

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::Start___STATIC__VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 
    
    TINYCLR_SET_AND_LEAVE(TimeService_Start());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::Stop___STATIC__VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    TINYCLR_SET_AND_LEAVE(TimeService_Stop());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::Update___STATIC__MicrosoftSPOTTimeTimeServiceStatus__U4__U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_RT_HeapBlock* managedStatus = NULL;  

    UINT32 serverIP  = stack.Arg0().NumericByRef().u4;
    UINT32 tolerance = stack.Arg1().NumericByRef().u4;    

    TimeService_Status status;

    hr = TimeService_Update( serverIP, tolerance, &status );

    if(hr == CLR_E_RESCHEDULE) 
    {
        TINYCLR_LEAVE();
    }
    else 
    {
        CLR_RT_HeapBlock& top = stack.PushValueAndClear();

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( top, g_CLR_RT_WellKnownTypes.m_TimeServiceStatus ));
        
        managedStatus = top.Dereference();

        managedStatus[ ManagedStatus::FIELD__Flags            ].SetInteger( status.Flags );
        managedStatus[ ManagedStatus::FIELD__SyncSourceServer ].SetInteger( status.ServerIP );
        managedStatus[ ManagedStatus::FIELD__SyncTimeOffset   ].SetInteger( status.SyncOffset );
        managedStatus[ ManagedStatus::FIELD__TimeUTC          ].SetInteger( status.CurrentTimeUTC );
    }

    TINYCLR_CLEANUP();

    // we are not throwing any exception, we are instead communicating the result through the TimeServiceStatus object we return
    // if the operation did not completed, we need to reschedule this call though
    switch(hr)
    {
        case CLR_E_RESCHEDULE:
            break;
        case CLR_E_TIMEOUT:
        case CLR_E_FAIL:
        default:
            // swallow error
            hr = S_OK; 
            break;
    }

    TINYCLR_CLEANUP_END();
}

HRESULT Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::SetUtcTime___STATIC__VOID__I8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    INT64 utc = stack.Arg0().NumericByRef().s8;
    
    INT64 utcBefore = Time_GetUtcTime();
    INT64 utcNow = Time_SetUtcTime( utc, false );

    // correct the uptime
    if(utcNow > utcBefore) 
    {
        g_CLR_RT_ExecutionEngine.m_startTime += (utcNow - utcBefore);
    }
    else
    {
        g_CLR_RT_ExecutionEngine.m_startTime -= (utcBefore - utcNow);
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::SetTimeZoneOffset___STATIC__VOID__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    Time_SetTimeZoneOffset( stack.Arg0().NumericByRef().u4 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::get_LastSyncStatus___STATIC__MicrosoftSPOTTimeTimeServiceStatus( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    TimeService_Status status;
    CLR_RT_HeapBlock&       top = stack.PushValueAndClear();
    CLR_RT_HeapBlock*       managedStatus = NULL;      

    TINYCLR_CHECK_HRESULT(TimeService_GetLastSyncStatus( &status ));

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( top, g_CLR_RT_WellKnownTypes.m_TimeServiceStatus ));
    managedStatus = top.Dereference();

    managedStatus[ ManagedStatus::FIELD__Flags            ].SetInteger( status.Flags );
    managedStatus[ ManagedStatus::FIELD__SyncSourceServer ].SetInteger( status.ServerIP );
    managedStatus[ ManagedStatus::FIELD__SyncTimeOffset   ].SetInteger( status.SyncOffset );
    managedStatus[ ManagedStatus::FIELD__TimeUTC          ].SetInteger( status.CurrentTimeUTC );

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_Time_native_System_Environment::get_TickCount___STATIC__I4 ( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    stack.SetResult_I4( (CLR_INT32)Time_GetTickCount() );

    TINYCLR_NOCLEANUP_NOLABEL();
}
