////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

class TimeServiceDriver
{ 
    static const UINT32 c_SecToTimeUnits        = 1000000;
    static const UINT32 c_AsyncRescheduleTime   = 100000; 
    static const UINT32 c_DefaultRefreshTime    = 5 * 10 * 1000; // 5 minutes 
    
protected:
    static BOOL             m_initialized;
    TimeService_Settings    m_settings;
    TimeService_Status      m_lastSyncStatus;
    HAL_COMPLETION          m_TimeServiceCompletion;

public:
    HRESULT Initialize();
    HRESULT Uninitialize();
    HRESULT Start();
    HRESULT Stop();
    HRESULT Update(UINT32 tolerance, TimeService_Status* status);
    HRESULT Update(UINT32 serverIP, UINT32 tolerance, TimeService_Status* status);
    HRESULT Update(UINT8* serverIPs, INT32 serverNum, UINT32 tolerance, TimeService_Status* status);
    HRESULT GetLastSyncStatus(TimeService_Status* status);
    HRESULT LoadSettings(TimeService_Settings* settings);
    HRESULT SaveSettings(TimeService_Settings* settings);

protected:
    void ToIPFragments(UINT32 ip, UINT8* fragments);
    UINT32 FromIPFragments(UINT8* fragments);
    static void TimeServiceCompletionRoutine(void *arg);
    void Completion(void *arg);
};

extern TimeServiceDriver g_TimeServiceDriver;

