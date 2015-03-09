////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
/* STDIO stubs                                                              */
//--//

#if !defined(BUILD_RTM)


void hal_fprintf_SetLoggingCallback( LOGGING_CALLBACK fpn )
{
    NATIVE_PROFILE_PAL_CRT();

}

#endif

//--//

#if !defined(PLATFORM_EMULATED_FLOATINGPOINT)

int hal_snprintf_float( char* buffer, size_t len, const char* format, float f )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}

int hal_snprintf_double( char* buffer, size_t len, const char* format, double d )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}

#else
int hal_snprintf_float( char* buffer, size_t len, const char* format, INT32 f )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}

int hal_snprintf_double( char* buffer, size_t len, const char* format, INT64& d )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}

#endif


int hal_printf( const char* format, ... )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}

int hal_vprintf( const char* format, va_list arg )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}


int hal_fprintf( COM_HANDLE stream, const char* format, ... )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}

int hal_vfprintf( COM_HANDLE stream, const char* format, va_list arg )
{
    NATIVE_PROFILE_PAL_CRT();

    return 0;
}

int hal_snprintf( char* buffer, size_t len, const char* format, ... )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}


int hal_vsnprintf( char* buffer, size_t len, const char* format, va_list arg )
{
    NATIVE_PROFILE_PAL_CRT();
    return 0;
}

#if !defined(PLATFORM_BLACKFIN) && !defined(PLATFORM_SH)
int hal_strcpy_s ( char* strDst, size_t sizeInBytes, const char* strSrc )
{
    NATIVE_PROFILE_PAL_CRT();
#undef strcpy

    size_t len;
    if(strDst == NULL || strSrc == NULL || sizeInBytes == 0) {_ASSERTE(FALSE); return 1;}
    
    len = hal_strlen_s(strSrc);
    if(sizeInBytes < len + 1) {_ASSERTE(FALSE); return 1;}

    strcpy( strDst, strSrc );
    return 0;

#define strcpy DoNotUse_*strcpy []
}

int hal_strncpy_s ( char* strDst, size_t sizeInBytes, const char* strSrc, size_t count )
{
    NATIVE_PROFILE_PAL_CRT();
#undef strncpy
    if(strDst == NULL || strSrc == NULL || sizeInBytes == 0) {_ASSERTE(FALSE); return 1;}
    
    if (sizeInBytes < count + 1)
    {
        _ASSERTE(FALSE);
        strDst[0] = 0;
        return 1;
    }

    strDst[count] = 0;
    strncpy( strDst, strSrc, count );
    return 0;

#define strncpy DoNotUse_*strncpy []
}

size_t hal_strlen_s (const char * str)
{
    NATIVE_PROFILE_PAL_CRT();

    const char *eos = str;
    while( *eos++ ) ;
    return( eos - str - 1 );
}

int hal_strncmp_s ( const char* str1, const char* str2, size_t num )
{
    NATIVE_PROFILE_PAL_CRT();
#undef strncmp
    if(str1 == NULL || str2 == NULL) {_ASSERTE(FALSE); return 1;}
    
    return strncmp( str1, str2, num );

#define strncmp DoNotUse_*strncmp []
}

#endif //!defined(PLATFORM_BLACKFIN) && !defined(PLATFORM_SH)


// Compares 2 ASCII strings case insensitive. Does not take locale into account.
int hal_stricmp( const char * dst, const char * src )
{
    int f = 0, l = 0;

    do
    {
        if ( ((f = (unsigned char)(*(dst++))) >= 'A') && (f <= 'Z') )
        {
            f -= 'A' - 'a';
        }
        if ( ((l = (unsigned char)(*(src++))) >= 'A') && (l <= 'Z') )
        {
            l -= 'A' - 'a';
        }
    }
    while ( f && (f == l) );

    return(f - l);
}
