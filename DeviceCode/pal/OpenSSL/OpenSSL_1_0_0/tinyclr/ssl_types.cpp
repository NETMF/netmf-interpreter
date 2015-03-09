
#include "e_os.h"
#ifdef OPENSSL_SYS_WINDOWS
#include <time.h>
#else
#include <tinyhal.h>
#endif

static struct tm g_timestruct;

int tinyclr_ssl_stat( const char *path, struct stat *buffer )
{
    return 0;
}

int tinyclr_ssl_toupper(int c)
{   
    int retval = c;
    if(('a' <= c) && (c <= 'z'))            
        retval = 'A' + (c - 'a');
    return retval;
} 
int tinyclr_ssl_tolower(int c)
{   
    int retval = c;
    if(('A' <= c) && (c <= 'Z'))            
        retval = 'a' + (c - 'A');

    return retval;
}
int tinyclr_strcmp ( const char * str1, const char * str2 )
{
    int n = TINYCLR_SSL_STRLEN(str1);
    while (*str1 && *str2 && n)
        {
        int res = *str1 - *str2;
        if (res) return res < 0 ? -1 : 1;
        str1++;
        str2++;
        n--;
        }
    if (n == 0)
        return 0;
    if (*str1)
        return 1;
    if (*str2)
        return -1;
    return 0;
}

size_t tinyclr_ssl_fwrite ( const void * ptr, size_t size, size_t count, void * stream )
{
    return 0;
};

int tinyclr_ssl_gettimeofday(void *tp, void *tzp)
{
    time_t localtime = Time_GetLocalTime(); //100nanoseconds 
    ((struct TINYCLR_SSL_TIMEVAL*)tp)->tv_sec  = (long)localtime / 10000000; //for seconds 
    ((struct TINYCLR_SSL_TIMEVAL*)tp)->tv_usec = (long)(localtime % 10000000) / 1000; //for microseconds
    return 0;
};

int tinyclr_ssl_chmod(const char *filename, int pmode )
{
    return 0; // for now
};

struct servent *tinyclr_ssl_getservbyname(const char *name, const char *proto)
{
    // TODO - temp workaround
#if SOCKETS_MAX_COUNT!=1
#if !defined(TCPIP_LWIP) && !defined(TCPIP_LWIP_OS)
    return getservbyname(name, proto);
#else
    debug_printf("tinyclr_ssl_getservbyname(%s,%s) stubbed for LWIP!\r\n",name,proto);
    return NULL;
#endif
#else
    return NULL;
#endif
}
struct hostent *tinyclr_ssl_gethostbyname(const char *name)
{
#if SOCKETS_MAX_COUNT!=1
#if !defined(TCPIP_LWIP) && !defined(TCPIP_LWIP_OS)
    return gethostbyname(name);
#else
    debug_printf("tinyclr_ssl_gethostbyname(%s) stubbed for LWIP!\r\n",name);
    // undefined symbol, how is that possible ? return lwip_gethostbyname(name);
    return NULL;
#endif
#else
    return NULL;
#endif
}
struct hostent *tinyclr_ssl_gethostbyaddr(const char *addr, int length, int type)
{
#if SOCKETS_MAX_COUNT!=1
#if !defined(TCPIP_LWIP) && !defined(TCPIP_LWIP_OS)
    return gethostbyaddr(addr, length, type);
#else
    debug_printf("tinyclr_ssl_gethostbyaddr(%s,%d,%d) stubbed for LWIP!\r\n",addr,length,type);
    return NULL;
#endif
#else
    return NULL;
#endif
}

pid_t tinyclr_ssl_getpid()
{
    return 0;
}

void tinyclr_qsort ( void * base, size_t num, size_t size, 
    int ( * comparator ) ( const void *, const void * ) )
{
    if(num <= 1) return;

    // TODO:     

    return;
}

int hal_fprintf_ssl(OPENSSL_TYPE__FILE* x, const char* format, ... )
{
    va_list arg_ptr;
    int     chars;

    va_start( arg_ptr, format );

    chars = hal_vprintf( format, arg_ptr );

    va_end( arg_ptr );

    return chars;
}

INT64 s_TimeUntil1900 = 0;

static const INT64 MF_TIME_TO_SECONDS = 10000000LL; //// convert from 100nanosecond ticks to seconds
    
// localtime as returned here is seconds elapsed since 1/1/1601 rather
// than that C library standard of seconds elapsed wince 1/1/1970. However
// the following implementation of localtime and gmtime handle this correctly.
time_t tinyclr_time ( time_t * timer )
{
    INT64 tim = Time_GetLocalTime();
    time_t localtime;

    if(s_TimeUntil1900 == 0)
    {
        SYSTEMTIME st;

        memset(&st, 0, sizeof(st));

        st.wMonth = 1;
        st.wDay  = 1;
        st.wYear = 1900;

        s_TimeUntil1900 = Time_FromSystemTime(&st);
    }

    tim -= s_TimeUntil1900;

    tim = (tim / MF_TIME_TO_SECONDS); // convert from 100nano to seconds

    localtime = (time_t)tim;

    if (timer != NULL) *timer = localtime;
    
    return localtime;
}

// timer is milliseconds elapsed since 1/1/1601
struct tm * tinyclr_localtime ( const time_t * timer )
{
    SYSTEMTIME systime;
    INT64 tim = (INT64)(UINT32)(*timer);
    INT64 localtime = tim * MF_TIME_TO_SECONDS + s_TimeUntil1900; // convert from seconds to 100nano seconds
    Time_ToSystemTime(localtime, &systime); 
    g_timestruct.tm_hour = systime.wHour;
    g_timestruct.tm_mday = systime.wDay;
    g_timestruct.tm_min = systime.wMinute;
    g_timestruct.tm_mon = systime.wMonth - 1; // SYSTEMTIME structure is one based and struct tm is zero based
    g_timestruct.tm_sec = systime.wSecond;
    g_timestruct.tm_wday = systime.wDayOfWeek;
    g_timestruct.tm_year = systime.wYear - 1900; // struct tm records years since 1900
    return &g_timestruct;
}

// timer is milliseconds elapsed since 1/1/1601
struct tm * tinyclr_gmtime ( const time_t * timer )
{
    SYSTEMTIME systime;
    INT64 offset = Time_GetTimeZoneOffset() * 60; // convert to seconds

    INT64 tim = (INT64)(UINT32)(*timer);
    INT64 utctime = (tim + offset) * MF_TIME_TO_SECONDS + s_TimeUntil1900; // convert to seconds since 1900
    Time_ToSystemTime(utctime, &systime); 
    g_timestruct.tm_hour = systime.wHour;
    g_timestruct.tm_mday = systime.wDay;
    g_timestruct.tm_min = systime.wMinute;
    g_timestruct.tm_mon = systime.wMonth - 1; // SYSTEMTIME structure is one based and struct tm is zero based
    g_timestruct.tm_sec = systime.wSecond;
    g_timestruct.tm_wday = systime.wDayOfWeek;
    g_timestruct.tm_year = systime.wYear - 1900; // struct tm records years since 1900
    return &g_timestruct;
}

time_t tinyclr_mktime ( struct tm * timeptr )
{
    SYSTEMTIME systime;
    systime.wHour = timeptr->tm_hour;
    systime.wDay = timeptr->tm_mday;
    systime.wMinute = timeptr->tm_min;
    systime.wMonth = timeptr->tm_mon + 1;  // SYSTEMTIME structure is one based and struct tm is zero based
    systime.wSecond = timeptr->tm_sec;
    systime.wDayOfWeek = timeptr->tm_wday;
    systime.wYear = timeptr->tm_year + 1900; // struct tm records years since 1900
    time_t localtime = (time_t)((Time_FromSystemTime(&systime) - s_TimeUntil1900) / MF_TIME_TO_SECONDS); //convert from 100nano to seconds
    return localtime;
}

