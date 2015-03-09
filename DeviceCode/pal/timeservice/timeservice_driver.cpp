////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "timeservice_driver.h"

TimeServiceDriver g_TimeServiceDriver;

BOOL TimeServiceDriver::m_initialized = FALSE;

HRESULT TimeServiceDriver::Initialize()
{
    if(!m_initialized)
    {
        m_settings.RefreshTime = c_DefaultRefreshTime;
        
        m_lastSyncStatus.CurrentTimeUTC = 0;
        m_lastSyncStatus.Flags          = TimeService_Status_Flags_Failed;
        m_lastSyncStatus.SyncOffset     = 0;
        m_lastSyncStatus.ServerIP       = 0;
        
        m_TimeServiceCompletion.InitializeForUserMode(TimeServiceCompletionRoutine, NULL);      

        m_initialized = TRUE;
    }

    return S_OK;
}

HRESULT TimeServiceDriver::Uninitialize()
{
    if(m_initialized)
    {
        Stop();
        
       m_initialized = FALSE; 
    }
    return S_OK;
}

void TimeServiceDriver::TimeServiceCompletionRoutine(void *arg)
{
    g_TimeServiceDriver.Completion(arg);
}

void TimeServiceDriver::Completion(void *arg)
{
    HRESULT hr = S_OK;
    TimeService_Status status;
    
    status.Flags = TimeService_Status_Flags_Success;

    hr = Update(m_settings.Tolerance, &status);    

    if (m_TimeServiceCompletion.IsLinked()) 
    {
        m_TimeServiceCompletion.Abort();
    }

    if (hr == CLR_E_RESCHEDULE)
    {
        m_TimeServiceCompletion.EnqueueDelta(c_AsyncRescheduleTime);
    }
    else
    {
        m_TimeServiceCompletion.EnqueueDelta(m_settings.RefreshTime * c_SecToTimeUnits);
    }
}

HRESULT TimeServiceDriver::Start()
{    
    m_lastSyncStatus.CurrentTimeUTC = 0;
    m_lastSyncStatus.Flags          = TimeService_Status_Flags_Failed;
    m_lastSyncStatus.SyncOffset     = 0;
    m_lastSyncStatus.ServerIP       = 0;
    
    /// Sync up is rather immediate.
    if (!m_TimeServiceCompletion.IsLinked()) m_TimeServiceCompletion.EnqueueDelta(0);

    return S_OK;
}

HRESULT TimeServiceDriver::Stop()
{
    if (m_TimeServiceCompletion.IsLinked()) m_TimeServiceCompletion.Abort();

    return S_OK;
}

void TimeServiceDriver::ToIPFragments(UINT32 ip, UINT8* fragments)
{
    fragments[0] = (ip >> 24) & 0xFF;
    fragments[1] = (ip >> 16) & 0xFF;
    fragments[2] = (ip >> 8) & 0xFF;
    fragments[3] = ip & 0xFF;
}

UINT32 TimeServiceDriver::FromIPFragments(UINT8* fragments)
{
    UINT32 ip = 0;
    ip |= fragments[0];
    ip <<= 8;
    ip |= fragments[1];
    ip <<= 8;
    ip |= fragments[2];
    ip <<= 8;
    ip |= fragments[3];
    
    return ip;
}

HRESULT TimeServiceDriver::Update(UINT8* serverIPs, INT32 serverNum, UINT32 tolerance, TimeService_Status* status)
{
    SYSTEMTIME systemTime;

    INT32 res = HAL_TIMESERVICE_GetTimeFromSNTPServerList(serverIPs, serverNum, &systemTime);

    /// HAL_TIMESERVICE_GetTimeFromSNTPServer needs more time to complete, so try again.
    if (res == HAL_TIMESERVICE_WANT_READ_WRITE)
    {
        return CLR_E_RESCHEDULE;
    }

    if (res == HAL_TIMESERVICE_SUCCESS)
    {
        INT64 time = Time_FromSystemTime(&systemTime);

        time = Time_SetUtcTime(time, true);

        status->Flags = TimeService_Status_Flags_Success;
        status->ServerIP = FromIPFragments(serverIPs);
        status->SyncOffset = 0;
        status->CurrentTimeUTC = time; 

        m_lastSyncStatus = *status;

        PostManagedEvent(EVENT_TIMESERVICE, EVENT_TIMESERVICE_SYSTEMTIMECHANGED, 0, 0);

        return S_OK;
    }
    
    status->Flags = TimeService_Status_Flags_Failed;
    status->ServerIP = FromIPFragments(serverIPs);
    status->SyncOffset = 0;
    status->CurrentTimeUTC = 0; /// Which is invalid time.

    PostManagedEvent(EVENT_TIMESERVICE, EVENT_TIMESERVICE_TIMESYNCFAILED, 0, (UINT32)res);
        
    return res == HAL_TIMESERVICE_TIME_OUT ? CLR_E_TIMEOUT : CLR_E_FAIL;
}

HRESULT TimeServiceDriver::Update(UINT32 serverIP, UINT32 tolerance, TimeService_Status* status)
{
    UINT8 sntpServerIP[4];    

    /// Use m_settings value if no valid server ip is given.
    if (serverIP == 0)
        return Update(tolerance, status);

    ToIPFragments(serverIP, sntpServerIP);

    return Update(sntpServerIP, 1, tolerance, status);
}

HRESULT TimeServiceDriver::Update(UINT32 tolerance, TimeService_Status* status)
{
    UINT8 sntpServerIP[8];

    ToIPFragments( m_settings.PrimaryServerIP  , &sntpServerIP[0] );
    ToIPFragments( m_settings.AlternateServerIP, &sntpServerIP[4] );

    return Update(sntpServerIP, 2, tolerance, status);
}

HRESULT TimeServiceDriver::GetLastSyncStatus(TimeService_Status* status)
{
    *status = m_lastSyncStatus;
    return S_OK;
}

HRESULT TimeServiceDriver::LoadSettings(TimeService_Settings* settings)
{
    *settings = m_settings;
    return S_OK;
}

HRESULT TimeServiceDriver::SaveSettings(TimeService_Settings* settings)
{
    m_settings = *settings;
    return S_OK;
}

