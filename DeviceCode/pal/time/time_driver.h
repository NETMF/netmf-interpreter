////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

class TimeDriver
{
    static const INT32 c_MinutesPerDay = 1440;
    static const INT32 c_ToMinutes     = 60 * TEN_MHZ;

protected:
    static BOOL     m_initialized;
    INT64           m_utcTime;
    INT64           m_timezoneOffset; /// This is not in minutes, instead in 100ns.

    /// Ticks to UTC time params. Offset only.    
    INT64           m_Utc_c;

    /// Machine Ticks to Real Ticks params.
    /// ax = by + c; a, b, c are integers. x = adjusted ticks, y = machine ticks.
    INT32           m_Ticks_a;
    INT32           m_Ticks_b;
    INT64           m_Ticks_c;
   
public:
    HRESULT     Initialize();
    HRESULT     Uninitialize();

    INT64       GetUtcTime();
    INT64       SetUtcTime( INT64 UtcTime, bool calibrate ); 
    INT64       GetLocalTime();
    INT32       GetTimeZoneOffset();
    INT32       SetTimeZoneOffset(INT32 offset);
    INT64       GetTickCount();
    INT64       GetMachineTime();

    BOOL        ToSystemTime(INT64 time, SYSTEMTIME* systemTime);
    INT64       FromSystemTime(const SYSTEMTIME* systemTime);

    HRESULT     AccDaysInMonth(INT32 year, INT32 month, INT32* days);
    HRESULT     DaysInMonth(INT32 year, INT32 month, INT32* days);

    BOOL       TimeSpanToString( const INT64& ticks, LPSTR& buf, size_t& len );
    LPCSTR     TimeSpanToString( const INT64& ticks );
    BOOL       DateTimeToString( const INT64& time, LPSTR& buf, size_t& len );
    LPCSTR     DateTimeToString( const INT64& time);    

    BOOL       SafeSprintfV( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, va_list arg );
    BOOL       SafeSprintf( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, ... );

};

extern TimeDriver g_TimeDriver;


