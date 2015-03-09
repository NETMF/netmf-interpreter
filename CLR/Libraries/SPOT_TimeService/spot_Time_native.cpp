////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_Time_native.h"


static const CLR_RT_MethodHandler method_lookup[] =
{
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::get_Settings___STATIC__MicrosoftSPOTTimeTimeServiceSettings,
    Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::set_Settings___STATIC__VOID__MicrosoftSPOTTimeTimeServiceSettings,
    Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::Start___STATIC__VOID,
    Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::Stop___STATIC__VOID,
    Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::get_LastSyncStatus___STATIC__MicrosoftSPOTTimeTimeServiceStatus,
    NULL,
    NULL,
    Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::Update___STATIC__MicrosoftSPOTTimeTimeServiceStatus__U4__U4,
    Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::SetUtcTime___STATIC__VOID__I8,
    Library_spot_Time_native_Microsoft_SPOT_Time_TimeService::SetTimeZoneOffset___STATIC__VOID__I4,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    Library_spot_Time_native_System_Environment::get_TickCount___STATIC__I4,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Time =
{
    "Microsoft.SPOT.Time", 
    0xF1145827,
    method_lookup
};

