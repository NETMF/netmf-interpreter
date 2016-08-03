////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "time_driver.h"

const int CummulativeDaysForMonth[13] = {0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};

#define IS_LEAP_YEAR(y)             (((y % 4 == 0) && (y % 100 != 0)) || (y % 400 == 0))
#define NUMBER_OF_LEAP_YEARS(y)     ((((y - 1) / 4) - ((y - 1) / 100) + ((y - 1) / 400)) - BASE_YEAR_LEAPYEAR_ADJUST) /// Number of leap years until base year, not including the target year itself.
#define NUMBER_OF_YEARS(y)          (y - BASE_YEAR)

#define YEARS_TO_DAYS(y)            ((NUMBER_OF_YEARS(y) * DAYS_IN_NORMAL_YEAR) + NUMBER_OF_LEAP_YEARS(y))
#define MONTH_TO_DAYS(y, m)         (CummulativeDaysForMonth[m - 1] + ((IS_LEAP_YEAR(y) && (m > 2)) ? 1 : 0))

#define TIMEUNIT_TO_MINUTES         600000000
#define TIMEUNIT_TO_MILLISECONDS    10000
#define MILLISECONDS_TO_SECONDS     1000
#define SECONDS_TO_MINUTES          60
#define MINUTES_TO_HOUR             60
#define HOURS_TO_DAY                24

/// In absense of any time sync, or built in clock, this will be the UTC time of the system.
/// We can set it to our RTM date. For now I am setting it to 1/1/2009:00:00:00.000
#define INITIAL_TIME               ((UINT64)(YEARS_TO_DAYS(2011) + MONTH_TO_DAYS(2011, 6)) * HOURS_TO_DAY * MINUTES_TO_HOUR * TIMEUNIT_TO_MINUTES)

TimeDriver g_TimeDriver;
BOOL TimeDriver::m_initialized = FALSE;

HRESULT TimeDriver::Initialize()
{
    if (!m_initialized)
    {
        m_Ticks_a = 1;
        m_Ticks_b = 1;
        m_Ticks_c = 0;
        HAL_Time_GetDriftParameters(&m_Ticks_a, &m_Ticks_b, &m_Ticks_c);

        m_timezoneOffset = 0; 

#ifdef PLATFORM_WINDOWS_EMULATOR

        TIME_ZONE_INFORMATION tzi;
        SYSTEMTIME st;
        INT32 dst = 0;

        ::GetSystemTime(&st);
        ::GetTimeZoneInformation(&tzi);

        bool lessThanStandard = 
               st.wMonth < tzi.StandardDate.wMonth && 
               st.wHour  < tzi.StandardDate.wHour &&
               ((st.wDayOfWeek+1) * st.wDay) < ((tzi.StandardDate.wDayOfWeek+1) * tzi.StandardDate.wDay);

        bool greaterEqualDaylight = 
               st.wMonth >= tzi.DaylightDate.wMonth &&
               st.wHour >= tzi.DaylightDate.wHour &&
               ((st.wDayOfWeek+1) * st.wDay) >= ((tzi.DaylightDate.wDayOfWeek+1) * tzi.DaylightDate.wDay);


        if((tzi.StandardDate.wMonth > tzi.DaylightDate.wMonth && lessThanStandard && greaterEqualDaylight) ||
           (tzi.StandardDate.wMonth < tzi.DaylightDate.wMonth && (lessThanStandard || greaterEqualDaylight)))
        {
            dst = tzi.DaylightBias;
        }

        m_timezoneOffset = (INT64)-(tzi.Bias + dst) * TIMEUNIT_TO_MINUTES;

        /// Instead of giving out machine time, our emulator implementation returns UTC time.
        m_utcTime = 0;
        m_Utc_c = 0;
#else
        m_utcTime = INITIAL_TIME;
        m_Utc_c = INITIAL_TIME;
#endif
        m_initialized = TRUE;        
    }

    return S_OK;
}

HRESULT TimeDriver::Uninitialize()
{
    if (m_initialized)
    {
        m_initialized = FALSE;
    }

    return S_OK;
}

INT64   TimeDriver::GetUtcTime()
{
    return GetMachineTime() + m_Utc_c;
}

INT64   TimeDriver::SetUtcTime( INT64 utcTime, bool calibrate )
{
    m_Utc_c = utcTime - GetMachineTime();

    return TimeDriver::GetUtcTime();
}

INT64 TimeDriver::GetLocalTime()
{
    return (GetUtcTime() + m_timezoneOffset);
}

INT32 TimeDriver::GetTimeZoneOffset()
{
    /// Returned value is in minutes.
    return (INT32)(m_timezoneOffset / TIMEUNIT_TO_MINUTES);
}

INT32 TimeDriver::SetTimeZoneOffset( INT32 offset )
{
    offset  = offset % c_MinutesPerDay; /// no more than 24 hrs.    

    m_timezoneOffset = offset;
    m_timezoneOffset *= TIMEUNIT_TO_MINUTES;

    return offset;
}

INT64 TimeDriver::GetTickCount()
{
    return GetMachineTime() / TIMEUNIT_TO_MILLISECONDS;
}

INT64 TimeDriver::GetMachineTime()
{
    INT64 time = HAL_Time_CurrentTime();

    if(m_Ticks_a != 0)
    {
        time = (time * m_Ticks_b + m_Ticks_c) / m_Ticks_a;
    }

    return time;
}

BOOL TimeDriver::ToSystemTime(TIME time, SYSTEMTIME* systemTime)
{
    int ytd = 0;
    int mtd = 0;
    
    time /= TIMEUNIT_TO_MILLISECONDS;
    systemTime->wMilliseconds = time % MILLISECONDS_TO_SECONDS;
    time /= MILLISECONDS_TO_SECONDS;
    systemTime->wSecond = time % SECONDS_TO_MINUTES;
    time /= SECONDS_TO_MINUTES;
    systemTime->wMinute = time % MINUTES_TO_HOUR;
    time /= MINUTES_TO_HOUR;
    systemTime->wHour = time % HOURS_TO_DAY;
    time /= HOURS_TO_DAY;

    systemTime->wDayOfWeek = (time + BASE_YEAR_DAYOFWEEK_SHIFT) % 7;
    systemTime->wYear = (WORD)(time / DAYS_IN_NORMAL_YEAR + BASE_YEAR);
    ytd = YEARS_TO_DAYS(systemTime->wYear);
    if (ytd > time)
    {
        systemTime->wYear--;
        ytd = YEARS_TO_DAYS(systemTime->wYear);
    }

    time -= ytd;

    systemTime->wMonth = (WORD)(time / 31 + 1);
    mtd = MONTH_TO_DAYS(systemTime->wYear, systemTime->wMonth + 1);

    if (time >= mtd)
    {
        systemTime->wMonth++;        
    }

    mtd = MONTH_TO_DAYS(systemTime->wYear, systemTime->wMonth);

    systemTime->wDay = (WORD)(time - mtd + 1); 

    return TRUE;
}

TIME TimeDriver::FromSystemTime(const SYSTEMTIME* systemTime)
{
    TIME r = YEARS_TO_DAYS(systemTime->wYear) + MONTH_TO_DAYS(systemTime->wYear, systemTime->wMonth) + systemTime->wDay - 1;
    r = (((( (r * HOURS_TO_DAY) + systemTime->wHour) * MINUTES_TO_HOUR + systemTime->wMinute) * SECONDS_TO_MINUTES + systemTime->wSecond ) * MILLISECONDS_TO_SECONDS + systemTime->wMilliseconds) * TIMEUNIT_TO_MILLISECONDS;

    return r;
}

HRESULT TimeDriver::AccDaysInMonth(INT32 year, INT32 month, INT32* days)
{
    *days = MONTH_TO_DAYS(year, month);

    return S_OK;
}

HRESULT TimeDriver::DaysInMonth(INT32 year, INT32 month, INT32* days)
{
    *days = MONTH_TO_DAYS(year, month + 1) - MONTH_TO_DAYS(year, month);

    return S_OK;
}

BOOL TimeDriver::TimeSpanToString( const INT64& ticks, LPSTR& buf, size_t& len )
{
    UINT64 ticksAbs;
    UINT64 rest;

    if(ticks < 0)
    {
        ticksAbs = -ticks;

        Utility_SafeSprintf( buf, len, "-" );
    }
    else
    {
        ticksAbs = ticks;
    }

    rest      = ticksAbs % ( 1000 * TIME_CONVERSION__TICKUNITS);
    ticksAbs  = ticksAbs / ( 1000 * TIME_CONVERSION__TICKUNITS);  // Convert to seconds.

    if(ticksAbs > TIME_CONVERSION__ONEDAY) // More than one day.
    {
        Utility_SafeSprintf( buf, len, "%d.", (INT32)(ticksAbs / TIME_CONVERSION__ONEDAY) ); ticksAbs %= TIME_CONVERSION__ONEDAY;
    }

    SafeSprintf( buf, len, "%02d:", (INT32)(ticksAbs / TIME_CONVERSION__ONEHOUR)  ); ticksAbs %= TIME_CONVERSION__ONEHOUR  ;
    SafeSprintf( buf, len, "%02d:", (INT32)(ticksAbs / TIME_CONVERSION__ONEMINUTE)); ticksAbs %= TIME_CONVERSION__ONEMINUTE;
    SafeSprintf( buf, len, "%02d" , (INT32)(ticksAbs / TIME_CONVERSION__ONESECOND)); ticksAbs %= TIME_CONVERSION__ONESECOND;

    ticksAbs = (UINT32)rest;
    if(ticksAbs)
    {
        SafeSprintf( buf, len, ".%07d", (UINT32)ticksAbs );
    }

    return len != 0;
}

LPCSTR TimeDriver::TimeSpanToString( const INT64& ticks )
{
    static char rgBuffer[128];
    LPSTR  szBuffer = rgBuffer;
    size_t iBuffer  = ARRAYSIZE(rgBuffer);

    TimeSpanToString( ticks, szBuffer, iBuffer );

    return rgBuffer;
}

BOOL TimeDriver::DateTimeToString( const INT64& time, LPSTR& buf, size_t& len )
{
    SYSTEMTIME st;

    ToSystemTime( time, &st );

    return SafeSprintf( buf, len, "%4d/%02d/%02d %02d:%02d:%02d.%03d", st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds );
}

LPCSTR TimeDriver::DateTimeToString( const INT64& time)
{
    static char rgBuffer[128];
    LPSTR  szBuffer =           rgBuffer;
    size_t iBuffer  = ARRAYSIZE(rgBuffer);

    DateTimeToString( time, szBuffer, iBuffer );

    return rgBuffer;

}

BOOL TimeDriver::SafeSprintfV( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, va_list arg )
{
    int  chars = hal_vsnprintf( szBuffer, iBuffer, format, arg );
    BOOL fRes  = (chars >= 0);

    if(fRes == FALSE) chars = 0;

    szBuffer += chars; szBuffer[0] = 0;
    iBuffer  -= chars;

    return fRes;
}

BOOL TimeDriver::SafeSprintf( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, ... )
{
    va_list arg;
    BOOL    fRes;

    va_start( arg, format );

    fRes = Utility_SafeSprintfV( szBuffer, iBuffer, format, arg );

    va_end( arg );

    return fRes;
}
