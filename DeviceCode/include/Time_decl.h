////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_TIME_DECL_H_
#define _DRIVERS_TIME_DECL_H_ 1

#define TIME INT64

/// Our time origin is 1/1/1601 00:00:00.000.000.  In Gregorian Calendar Jan 1, 1601 was also a Monday.
#define BASE_YEAR                   1601
#define BASE_YEAR_LEAPYEAR_ADJUST   388    
#define DAYS_IN_NORMAL_YEAR         365
#define BASE_YEAR_DAYOFWEEK_SHIFT   1 

#define TIME_CONVERSION__TO_MILLISECONDS    10000
#define TIME_CONVERSION__TO_SECONDS         10000000
#define TIME_CONVERSION__TICKUNITS         10000
#define TIME_CONVERSION__ONESECOND         1
#define TIME_CONVERSION__ONEMINUTE         60
#define TIME_CONVERSION__ONEHOUR           3600
#define TIME_CONVERSION__ONEDAY            86400

#define TIMEOUT_ZERO      LONGLONGCONSTANT(0x0000000000000000)
#define TIMEOUT_INFINITE  LONGLONGCONSTANT(0x7FFFFFFFFFFFFFFF)

#define TIME_ZONE_OFFSET    ((INT64)Time_GetTimeZoneOffset() * 600000000)



// -- //

/// NOTES: Why origin is at 1/1/1601.
/// Current civil calendar is named as Gregorian calendar after Pope Gregory XIII as he made adjustments
/// in 1582 (read more at wiki http://en.wikipedia.org/wiki/Gregorian_calendar). Rules governing
/// leap years were changed from then. Also in that year month October was 21 days instead of usual 31.
/// This poses a problem on calculating date/time difference, leap years etc before 1582 using simple math.
/// For example 1500 was a leap year using old method while it is not using new. But in reality, as part of the
/// history it was leap year. Default CLR origin 1/1/01 gives wrong date time from years before 1582. For example
/// dates like 10/6/1582 does exist in history (see wiki), while CLR managed date/time will not throw an exception
/// if you are to create that date. To stay safe side 1/1/1601 is taken as origin, as was done for Windows.

/// <summary>
/// Initializes PAL Time drivers, must be called before any of the Time_* PAL
/// methods could be used.
/// </summary>
HRESULT    Time_Initialize  (                     );

/// <summary>
/// Releases all the resource used by Time driver. Once uninitialized is called PAL Time_*
/// APIs should not be used anymore, results may be undefined or unpredictable.
/// </summary>
HRESULT    Time_Uninitialize(                     );

/// <summary>
/// UTC time according to this system. 
/// </summary>
/// <returns>Returns current UTC time in 100ns elapsed since 1/1/1601:00:00:00.000 UTC.</returns>
INT64       Time_GetUtcTime();

/// <summary>
/// Set UTC time of the system. This will be effective immediately.
/// </summary>
/// <param name="UtcTime">In 100ns since 1/1/1601:00:00:00.000</param>
/// <param name="calibrate">If "true" this value will be used for drift calculation.</param>
INT64       Time_SetUtcTime( INT64 UtcTime, bool calibrate ); 

/// <summary>
/// Local time according to the Time subsystem.
/// </summary>
/// <returns>Local time in 100ns elapsed since 1/1/1601:00:00:00.000 local time.</returns>
INT64       Time_GetLocalTime();

/// <notes> 
/// There is no Time_SetLocalTime, instead call Time_SetUtcTime and Time_SetTimeZoneOffset.
/// </notes>


/// <summary>
/// Offset from GMT.
/// </summary>
/// <returns>In minutes, for example Pacific Time would be GMT-8 = -480.</returns>
INT32 Time_GetTimeZoneOffset();

/// <summary>
/// Offset from GMT.
/// </summary>
/// <returns>In minutes, for example Pacific Time would be GMT-8 = -480.</returns>
INT32 Time_SetTimeZoneOffset(INT32 offset);

/// <summary>
/// Retrieves time since device was booted. 
/// </summary>
/// <returns>Time in 100ns.</returns>
INT64 Time_GetMachineTime();

/// <summary>
/// Retrieves time since device was booted. This is essentially same as Time_GetMachineTime(), except
/// return value is in milliseconds to keep parity with Windows API.
/// </summary>
/// <returns>Time in ms.</returns>
INT64 Time_GetTickCount();

/// <summary>
/// Converts 64bit time value to SystemTime structure. 64bit time is assumed as an offset from 1/1/1601:00:00:00.000 in 100ns.
/// </summary>
/// <returns>True if conversion is successful.</returns>
BOOL       Time_ToSystemTime(TIME time, SYSTEMTIME* systemTime);

/// <summary>
/// Converts SystemTime structure to 64bit time, which is assumed as an offset from 1/1/1601:00:00:00.000 in 100ns.
/// </summary>
/// <returns>Time value.</returns>
TIME       Time_FromSystemTime(const SYSTEMTIME* systemTime);

/// <summary>
/// Retrieves number of days given a month and a year. Calculates for leap years.
/// </summary>
/// <returns>S_OK if successful.</returns>
HRESULT    Time_DaysInMonth(INT32 year, INT32 month, INT32* days);

/// <summary>
/// Retrieves number of days since the beginning of the year given a month and a year. Calculates for leap years.
/// </summary>
/// <returns>S_OK if successful.</returns>
HRESULT    Time_AccDaysInMonth(INT32 year, INT32 month, INT32* days);

/// APIs to convert between types
BOOL       Time_TimeSpanToStringEx( const INT64& ticks, LPSTR& buf, size_t& len );
LPCSTR     Time_TimeSpanToString( const INT64& ticks );
BOOL       Time_DateTimeToStringEx( const INT64& time, LPSTR& buf, size_t& len );
LPCSTR     Time_DateTimeToString( const INT64& time);
LPCSTR     Time_CurrentDateTimeToString();

//--//

/// Debug utilities
BOOL       Utility_SafeSprintfV( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, va_list arg );
BOOL       Utility_SafeSprintf( LPSTR& szBuffer, size_t& iBuffer, LPCSTR format, ... );

//--//

BOOL    HAL_Time_Initialize      (                     );
BOOL    HAL_Time_Uninitialize    (                     );
INT64   HAL_Time_TicksToTime     ( UINT64 Ticks        );
INT64   HAL_Time_CurrentTime     (                     );
void    HAL_Time_SetCompare      ( UINT64 CompareValue );
void    HAL_Time_GetDriftParameters( INT32* a, INT32* b, INT64* c ); /// correct-time = (raw-time * b + c) / a. b is multiplication factor, a is the divisor and c is offset (if any).

extern "C" 
{
    UINT64  HAL_Time_CurrentTicks( );
    UINT64  Time_CurrentTicks    ( ); 
}

// -- //
// the following function are used in flash operations that they have to be located in the RAM. 
// It has been included in the scatterfile_ram_functions.xml

#if defined(__GNUC__)

void HAL_Time_Sleep_MicroSeconds_InRam( UINT32 uSec );
#endif


void    HAL_Time_Sleep_MicroSeconds( UINT32 uSec );
void    HAL_Time_Sleep_MicroSeconds_InterruptEnabled( UINT32 uSec );
// --//
    
    
UINT32  CPU_SystemClock        (             );
UINT32  CPU_TicksPerSecond     (             );
UINT64  CPU_MillisecondsToTicks( UINT64 mSec );
UINT64  CPU_MillisecondsToTicks( UINT32 mSec );

// -- //
// the following function are used in flash operations that they have to be located in the RAM. 
// It has been included in the scatterfile_ram_functions.xml
UINT32  CPU_MicrosecondsToTicks       ( UINT32 uSec    );
UINT64  CPU_MicrosecondsToTicks       ( UINT64 uSec    );
//--//
UINT32  CPU_MicrosecondsToSystemClocks( UINT32 uSec    );
UINT64  CPU_TicksToTime               ( UINT64 Ticks   );
UINT64  CPU_TicksToTime               ( UINT32 Ticks32 );

//--//

#endif // _DRIVERS_TIME_DECL_H_

