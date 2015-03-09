////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



#ifndef _SPOT_TIME_NATIVE_H_
#define _SPOT_TIME_NATIVE_H_

#include <TinyCLR_Interop.h>

struct Library_spot_Time_native_Microsoft_SPOT_Time_SystemTimeChangedEventArgs
{
    static const int FIELD__EventTime = 1;


    //--//

};


struct Library_spot_Time_native_Microsoft_SPOT_Time_TimeService
{
    static const int FIELD_STATIC__SystemTimeChanged = 0;
    static const int FIELD_STATIC__TimeSyncFailed    = 1;


    TINYCLR_NATIVE_DECLARE(get_Settings___STATIC__MicrosoftSPOTTimeTimeServiceSettings);
    TINYCLR_NATIVE_DECLARE(set_Settings___STATIC__VOID__MicrosoftSPOTTimeTimeServiceSettings);
    TINYCLR_NATIVE_DECLARE(Start___STATIC__VOID);
    TINYCLR_NATIVE_DECLARE(Stop___STATIC__VOID);
    TINYCLR_NATIVE_DECLARE(get_LastSyncStatus___STATIC__MicrosoftSPOTTimeTimeServiceStatus);
    TINYCLR_NATIVE_DECLARE(Update___STATIC__MicrosoftSPOTTimeTimeServiceStatus__U4__U4);
    TINYCLR_NATIVE_DECLARE(SetUtcTime___STATIC__VOID__I8);
    TINYCLR_NATIVE_DECLARE(SetTimeZoneOffset___STATIC__VOID__I4);    

    //--//

};

struct Library_spot_Time_native_Microsoft_SPOT_Time_TimeServiceSettings
{
    static const int FIELD__PrimaryServerIP     = 1;
    static const int FIELD__AlternateServerIP   = 2;
    static const int FIELD__RefreshTime         = 3;
    static const int FIELD__Tolerance           = 4;
    static const int FIELD__ForceSyncAtWakeUp   = 5;
    static const int FIELD__AutoDayLightSavings = 6;


    //--//

};

struct Library_spot_Time_native_Microsoft_SPOT_Time_TimeServiceStatus
{
    static const int FIELD__Flags            = 1;
    static const int FIELD__SyncSourceServer = 2;
    static const int FIELD__SyncTimeOffset   = 3;
    static const int FIELD__TimeUTC          = 4;


    //--//

};

struct Library_spot_Time_native_Microsoft_SPOT_Time_TimeSyncFailedEventArgs
{
    static const int FIELD__EventTime = 1;
    static const int FIELD__ErrorCode = 2;


    //--//

};


struct Library_spot_Time_native_System_Environment
{
    TINYCLR_NATIVE_DECLARE(get_TickCount___STATIC__I4);

    //--//

};

struct Library_spot_Time_native_Microsoft_SPOT_Time_TimeService__TimeServiceEvent
{
    static const int FIELD__EventType = 3;
    static const int FIELD__EventTime = 4;
    static const int FIELD__Status    = 5;


    //--//

};



extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Time;

#endif  //_SPOT_TIME_NATIVE_H_
