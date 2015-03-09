////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_TIMESERVICE_DECL_H_
#define _DRIVERS_TIMESERVICE_DECL_H_ 1

#define EVENT_TIMESERVICE_SYSTEMTIMECHANGED 1
#define EVENT_TIMESERVICE_TIMESYNCFAILED    2

/// <summary>
/// TimeService settings flags that are configuarble via API.
/// </summary>
enum TimeService_Settings_Flags
{
    TimeService_Settings_Flags_ForceSyncAtWakeUp = 0x1,
    TimeService_Settings_Flags_AutoDST           = 0x2,
};

enum TimeService_Status_Flags
{
    TimeService_Status_Flags_Success             = 0x0,
    TimeService_Status_Flags_Failed              = 0x1,
};

/// <summary>
/// TimeService settings (configuarble via API).
/// </summary>
struct TimeService_Settings
{
    UINT32 Flags; /// Flag values from TimeService_Settings_Flags
    UINT32 PrimaryServerIP; /// Main Server IP address.
    UINT32 AlternateServerIP; /// Alaternate Server IP address.
    UINT32 RefreshTime; /// Default refresh period in seconds.
    UINT32 Tolerance;   /// Amount of deviation from that is acceptable.
};

/// <summary>
/// Status structure for Time Service, usually returned from its APIs.
/// </summary>
struct TimeService_Status
{
    UINT32 Flags;           /// Status flags.
    UINT32 ServerIP;        /// Server IP that is involved in this status.
    UINT32 SyncOffset;      ///
    INT64  CurrentTimeUTC;  /// Latest know UTC time value. This saves an additional call to Time_GetUtcTime().
};

/// <summary>
/// Initializes time service, must be called before using any TimeService APIs.
/// </summary>
HRESULT TimeService_Initialize();

/// <summary>
/// Releases all resources used by TimeService, shuts down scheduled refresh.
/// After Uninitialized TimeService APIs may not function or return undefined results.
/// </summary>
HRESULT TimeService_UnInitialize();


/// <summary>
/// Starts scheduled time synchronization service. For periodic refresh it uses previously set refreshtime.
/// Refresh time is updateable dynamically, and effective immediately.
/// </summary>
HRESULT TimeService_Start();

/// <summary>
/// Stops periodic time synchronization service. Timeservice APIs are still available, may return stale data
/// unless manual sync is performed.
/// </summary>
HRESULT TimeService_Stop();

/// <summary>
/// Manual update of system time value from a given server. This can be called orthogonally along with
/// scheduled time service.
/// </summary>
HRESULT TimeService_Update(UINT32 serverIP, UINT32 tolerance, TimeService_Status* status);

/// <summary>
/// Returns latest sync status, it may be scheduled or forced sync, whichever occured last. This
/// can be verified from the TimeService_Status.Flags field. Optionally this will also
/// return the latest UTC time.
/// </summary>
HRESULT TimeService_GetLastSyncStatus(TimeService_Status* status);

/// <summary>
/// Loads the existing TimeService_Settings information.
/// </summary>
HRESULT TimeService_LoadSettings(TimeService_Settings* settings);

/// <summary>
/// Saves existing TimeService_Settings information, effective immediately.
/// </summary>
HRESULT TimeService_SaveSettings(TimeService_Settings* settings);


/// We redefine the error code to map 1-o-1 onto EBS stack error codes 
#define HAL_TIMESERVICE_TIME_OUT            -3 // SNTP_TIME_OUT
#define HAL_TIMESERVICE_WANT_READ_WRITE     -2 // SNTP_WANT_READ_WRITE
#define HAL_TIMESERVICE_ERROR               -1 // SNTP_ERROR
#define HAL_TIMESERVICE_SUCCESS              0

INT32 HAL_TIMESERVICE_GetTimeFromSNTPServer(UINT8* serverIP, SYSTEMTIME* systemTime);
INT32 HAL_TIMESERVICE_GetTimeFromSNTPServerList(UINT8* serverIP, INT32 serverNum, SYSTEMTIME* systemTime);
// -- //

#endif // _DRIVERS_TIMESERVICE_DECL_H_

