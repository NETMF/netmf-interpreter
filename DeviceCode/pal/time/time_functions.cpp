////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "Time_driver.h"

HRESULT Time_Initialize()
{
    return g_TimeDriver.Initialize();
}

HRESULT Time_Uninitialize()
{
    return g_TimeDriver.Uninitialize();
}

INT64 Time_GetUtcTime()
{
    return g_TimeDriver.GetUtcTime();
}

INT64 Time_SetUtcTime( INT64 UtcTime, bool calibrate )
{
    return g_TimeDriver.SetUtcTime( UtcTime, calibrate );
}

INT64 Time_GetLocalTime()
{
    return g_TimeDriver.GetLocalTime();
}

INT32 Time_GetTimeZoneOffset()
{
    return g_TimeDriver.GetTimeZoneOffset();
}

INT32 Time_SetTimeZoneOffset(INT32 offset)
{
    return g_TimeDriver.SetTimeZoneOffset(offset);
}

INT64 Time_GetTickCount()
{
    return g_TimeDriver.GetTickCount();
}

INT64 Time_GetMachineTime()
{
    return g_TimeDriver.GetMachineTime();
}

BOOL Time_ToSystemTime(INT64 time, SYSTEMTIME* systemTime)
{
    return g_TimeDriver.ToSystemTime(time, systemTime);
}

INT64 Time_FromSystemTime(const SYSTEMTIME* systemTime)
{
    return g_TimeDriver.FromSystemTime(systemTime);
}

HRESULT Time_DaysInMonth(INT32 year, INT32 month, INT32* days)
{
    return g_TimeDriver.DaysInMonth(year, month, days);
}

HRESULT Time_AccDaysInMonth(INT32 year, INT32 month, INT32* days)
{
    return g_TimeDriver.AccDaysInMonth(year, month, days);
}

BOOL Utility_SafeSprintfV( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, va_list arg )
{
    return g_TimeDriver.SafeSprintfV(szBuffer, iBuffer, format, arg);
}

BOOL Utility_SafeSprintf( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, ... )
{
    va_list arg;
    BOOL   fRes;

    va_start( arg, format );

    fRes = g_TimeDriver.SafeSprintfV(szBuffer, iBuffer, format, arg);

    va_end( arg );

    return fRes;
}

BOOL Time_TimeSpanToStringEx( const INT64& ticks, LPSTR& buf, size_t& len )
{
    return g_TimeDriver.TimeSpanToString(ticks, buf, len);
}

LPCSTR Time_TimeSpanToString( const INT64& ticks )
{
    return g_TimeDriver.TimeSpanToString(ticks);
}

BOOL Time_DateTimeToStringEx( const INT64& time, LPSTR& buf, size_t& len )
{   
    return g_TimeDriver.DateTimeToString(time, buf, len);
}

LPCSTR Time_DateTimeToString( const INT64& time)
{
    return g_TimeDriver.DateTimeToString(time);
}

LPCSTR Time_CurrentDateTimeToString()
{
    return g_TimeDriver.DateTimeToString(Time_GetLocalTime());
}



