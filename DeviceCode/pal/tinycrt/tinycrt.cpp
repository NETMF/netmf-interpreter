////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
/* ADS Specific functions to avoid the semihosting environment              */
//--//

#if defined(__arm)

extern "C"
{
#if !defined(PLATFORM_ARM_OS_PORT)

void __rt_div0()
{
    NATIVE_PROFILE_PAL_CRT();
#if defined(BUILD_RTM)
    // failure, reset immediately
    CPU_Reset();
#else
    lcd_printf("\fERROR:\r\n__rt_div0\r\n");
    debug_printf("ERROR: __rt_div0\r\n");

    HARD_BREAKPOINT();
#endif
}

void __rt_exit (int /*returncode*/) {}



void __rt_raise( int sig, int type )
{
    NATIVE_PROFILE_PAL_CRT();
#if defined(BUILD_RTM)
    // failure, reset immediately
    CPU_Reset();
#else
    lcd_printf("\fERROR:\r\n__rt_raise(%d, %d)\r\n", sig, type);
    debug_printf("ERROR: __rt_raise(%d, %d)\r\n", sig, type);

    HARD_BREAKPOINT();
#endif
}
#endif //!defined(PLATFORM_ARM_OS_PORT)

}
#endif



//--//
/* STDIO stubs                                                              */
//--//

#if !defined(BUILD_RTM)

static LOGGING_CALLBACK LoggingCallback;

void hal_fprintf_SetLoggingCallback( LOGGING_CALLBACK fpn )
{
    NATIVE_PROFILE_PAL_CRT();
    LoggingCallback = fpn;
}

#endif

//--//

int hal_printf( const char* format, ... )
{
    NATIVE_PROFILE_PAL_CRT();
    va_list arg_ptr;

    va_start(arg_ptr, format);

    int chars = hal_vprintf( format, arg_ptr );

    va_end( arg_ptr );

    return chars;
}

int hal_vprintf( const char* format, va_list arg )
{
    NATIVE_PROFILE_PAL_CRT();
    return hal_vfprintf( HalSystemConfig.stdio, format, arg );
}


int hal_fprintf( COM_HANDLE stream, const char* format, ... )
{
    NATIVE_PROFILE_PAL_CRT();
    va_list arg_ptr;
    int     chars;

    va_start( arg_ptr, format );

    chars = hal_vfprintf( stream, format, arg_ptr );

    va_end( arg_ptr );

    return chars;
}

int hal_vfprintf( COM_HANDLE stream, const char* format, va_list arg )
{
    NATIVE_PROFILE_PAL_CRT();
    char buffer[512];
    int chars = 0;

    chars = hal_vsnprintf( buffer, sizeof(buffer), format, arg );

    switch(ExtractTransport(stream))
    {
    default:
        DebuggerPort_Write( stream, buffer, chars, 0 ); // skip null terminator
        break;

#if !defined(BUILD_RTM)
    case FLASH_WRITE_TRANSPORT:
        if(LoggingCallback)
        {
            buffer[chars] = 0;

            LoggingCallback( buffer );
        }
        break;
#endif

    case LCD_TRANSPORT:
        {
            for(int i = 0; i < chars; i++)
            {
                LCD_WriteFormattedChar( buffer[i] );
            }
        }
        break;
    }

    return chars;
}

#if !defined(PLATFORM_EMULATED_FLOATINGPOINT)

int hal_snprintf_float( char* buffer, size_t len, const char* format, float f )
{
    NATIVE_PROFILE_PAL_CRT();

    // GCC vsnprintf corrupts memory with floating point values 
#if defined(__GNUC__)
    INT64   i = (INT64)f;
    int pow=0;

    if((float)0x7FFFFFFFFFFFFFFFll < f || (float)(-0x7FFFFFFFFFFFFFFFll) > f)
    {
        while(f >= 10.0 || f <= -10.0)
        {
            f = f / 10.0;
            pow++;
        }
        
        float dec = f - (float)(INT64)f;
        if(dec < 0.0) dec = -dec;
        dec *= 1000000000ull;
        
        return hal_snprintf( buffer, len, "%d.%llue+%d", (int)f, (UINT64)dec, pow);
    }
    else if(f != 0.0 && (INT64)f == 0)
    {
        char zeros[32];

        while(f < 1.0 && f > -1.0)
        {
            f = f * 10.0;
            pow--;
        }

        float dec = f - (float)(INT64)f;
        if(dec < 0.0) dec = -dec;

        //count the number of leading zeros
        double dec2 = dec;
        int num_zeros = 0;
        while(dec2 < 0.1 && dec2 > -0.1 && dec2 != 0 && num_zeros < ARRAYSIZE(zeros)-1)
        {
            dec2 *= 10;
            zeros[num_zeros++] = '0';
        }

        //create a string containing the leading zeros
        zeros[num_zeros] = '\0';
        
        dec *= 1000000000ull;
        
        return hal_snprintf( buffer, len, "%d.%s%llue%d", (int)f, zeros, (UINT64)dec, pow);
    }
    else
    {
        INT64 i = (INT64)f;

        f = f - (double)(INT64)f;
        if(f < 0) f = -f;
        f *= 1000000000ull;

        return hal_snprintf( buffer, len, "%lld.%09llu", i, (UINT64)f);
    }
#else

    return hal_snprintf( buffer, len, format, f );

#endif
}

int hal_snprintf_double( char* buffer, size_t len, const char* format, double d )
{
    NATIVE_PROFILE_PAL_CRT();

    // GCC vsnprintf corrupts memory with floating point values 
#if defined(__GNUC__)
    int pow=0;

    if((double)0x7FFFFFFFFFFFFFFFll < d || (double)(-0x7FFFFFFFFFFFFFFFll) > d)
    {
        while(d >= 10.0 || d <= -10.0)
        {
            d = d / 10.0;
            pow++;
        }

        double dec = d - (double)(INT64)d;
        if(dec < 0.0) dec = -dec;
        dec *= 100000000000000000ull;
        
        return hal_snprintf( buffer, len, "%d.%llue+%d", (int)d, (UINT64)dec, pow);
    }
    else if(d != 0.0 && (INT64)d == 0)
    {
        char zeros[32];
        
        while(d < 1.0 && d > -1.0)
        {
            d = d * 10.0;
            pow--;
        }

        double dec = d - (double)(INT64)d;
        if(dec < 0.0) dec = -dec;

        //count the number of leading zeros
        double dec2 = dec;
        int num_zeros = 0;
        while(dec2 < 0.1 && dec2 > -0.1 && dec2 != 0 && num_zeros < ARRAYSIZE(zeros)-1)
        {
            dec2 *= 10;
            zeros[num_zeros++] = '0';
        }

        //create a string containing the leading zeros
        zeros[num_zeros] = '\0';

        dec *= 100000000000000000ull;
        
        return hal_snprintf( buffer, len, "%d.%s%llue%d", (int)d, zeros, (UINT64)dec, pow);
    }
    else
    {
        INT64 i = (INT64)d;
        
        d = d - (double)(INT64)d;
        if(d < 0) d = -d;
        d *= 100000000000000000ull;

        return hal_snprintf( buffer, len, "%lld.%017llu", i, (UINT64)d);
    }
#else

    return hal_snprintf( buffer, len, format, d );

#endif    
}


#else

// no floating point, fixed point 

int hal_snprintf_float( char* buffer, size_t len, const char* format, INT32 f )
{
    NATIVE_PROFILE_PAL_CRT();
    UINT32   i ;
    UINT32  dec;

    if ( f < 0 ) 
    {
        // neagive number
        i = (UINT32) -f  ;
        dec = i & (( 1<<HAL_FLOAT_SHIFT) -1 );
        i = (i >>HAL_FLOAT_SHIFT);   

        if (dec !=0) dec = (dec * (UINT32)HAL_FLOAT_PRECISION + (1<< (HAL_FLOAT_SHIFT-1))) >>HAL_FLOAT_SHIFT;  

        return hal_snprintf( buffer, len, "-%d.%03u", i, (UINT32)dec);

    }
    else
    {
        // positive number
        i = (UINT32) f  ;
        dec = f & (( 1<<HAL_FLOAT_SHIFT) -1 );
        i =(UINT32)( i >>HAL_FLOAT_SHIFT); 

        if (dec !=0) dec = (dec * (UINT32)HAL_FLOAT_PRECISION + (1<< (HAL_FLOAT_SHIFT-1))) >>HAL_FLOAT_SHIFT;  

        return hal_snprintf( buffer, len, "%d.%03u", i, (UINT32)dec);
    }

}

int hal_snprintf_double( char* buffer, size_t len, const char* format, INT64& d )
{
    NATIVE_PROFILE_PAL_CRT();

    UINT64   i;
    UINT32  dec; // 32 bit is enough for decimal part

    if ( d < 0 ) 
    {
        // negative number
        i = (UINT64)-d;
        
        i += ((1 << (HAL_DOUBLE_SHIFT-1)) / HAL_DOUBLE_PRECISION); // add broad part of rounding increment before split

        dec = i & (( 1<<HAL_DOUBLE_SHIFT) -1 );
        i = i >> HAL_DOUBLE_SHIFT ;

        if (dec !=0)  dec = (dec * HAL_DOUBLE_PRECISION + ((1 << (HAL_DOUBLE_SHIFT-1)) % HAL_DOUBLE_PRECISION)) >> HAL_DOUBLE_SHIFT;

        return hal_snprintf( buffer, len, "-%lld.%04u", (INT64)i, (UINT32)dec);

    }
    else
    {

        // positive number
        i = (UINT64)d;

        i += ((1 << (HAL_DOUBLE_SHIFT-1)) / HAL_DOUBLE_PRECISION); // add broad part of rounding increment before split

        dec = i & (( 1<<HAL_DOUBLE_SHIFT) -1 );
        i = i >> HAL_DOUBLE_SHIFT;
        
        if (dec !=0)  dec = (dec * HAL_DOUBLE_PRECISION + ((1 << (HAL_DOUBLE_SHIFT-1)) % HAL_DOUBLE_PRECISION)) >> HAL_DOUBLE_SHIFT;

        return hal_snprintf( buffer, len, "%lld.%04u", (INT64)i, (UINT32)dec);
    }
    
}


#endif



int hal_snprintf( char* buffer, size_t len, const char* format, ... )
{
    NATIVE_PROFILE_PAL_CRT();
    va_list arg_ptr;
    int     chars;

    va_start( arg_ptr, format );

    chars = hal_vsnprintf( buffer, len, format, arg_ptr );

    va_end( arg_ptr );

    return chars;
}

#if defined(__GNUC__)

// RealView and GCC signatures for hal_vsnprintf() are different.
// This routine matches the RealView call, which defines va_list as int**
// rather than void* for GNUC.
// RealView calls to hal_vsnprintf() come here, then are converted to
// the gcc call.

#if defined(GCC_OLD_VA_LIST)
int hal_vsnprintf( char* buffer, size_t len, const char* format, int* args )
{
    NATIVE_PROFILE_PAL_CRT();

    hal_vsnprintf( buffer, len, format, (va_list) (args) );        // The GNU & RealView va_list actually differ only by a level of indirection
}
#else
int hal_vsnprintf( char* buffer, size_t len, const char* format, va_list arg )
{
#undef vsnprintf

    return vsnprintf( buffer, len, format, arg );

#define vsnprintf  DoNotUse_*printf []

}
#endif

#elif defined(__RENESAS__)
int hal_vsnprintf( char* buffer, size_t len, const char* format, va_list arg )
{
    NATIVE_PROFILE_PAL_CRT();
    return vsprintf(buffer, format, arg);
}

#elif defined(PLATFORM_BLACKFIN)

int hal_vsnprintf( char* buffer, size_t len, const char* format, va_list arg )
{
#undef vsnprintf

    return vsnprintf( buffer, len, format, arg );

#define vsnprintf  DoNotUse_*printf []

}

#elif defined(__arm)
int hal_vsnprintf( char* buffer, size_t len, const char* format, va_list arg )
{
    NATIVE_PROFILE_PAL_CRT();

#if defined(HAL_REDUCESIZE) || defined(PLATFORM_EMULATED_FLOATINGPOINT)

#undef _vsnprintf

    // _vsnprintf do not support floating point, vs vsnprintf supports floating point

    return _vsnprintf( buffer, len, format, arg );

#define _vsnprintf  DoNotUse_*printf []

#else

#undef vsnprintf

    return vsnprintf( buffer, len, format, arg );

#define vsnprintf  DoNotUse_*printf []

#endif

}
#endif

#if !defined(PLATFORM_BLACKFIN) 
int hal_strcpy_s ( char* strDst, size_t sizeInBytes, const char* strSrc )
{
    NATIVE_PROFILE_PAL_CRT();
#undef strcpy

    size_t len;
    if(strDst == NULL || strSrc == NULL || sizeInBytes == 0) {return 1;}
    
    len = hal_strlen_s(strSrc);
    if(sizeInBytes < len + 1) {return 1;}

    strcpy( strDst, strSrc );
    return 0;

#define strcpy DoNotUse_*strcpy []
}

int hal_strncpy_s ( char* strDst, size_t sizeInBytes, const char* strSrc, size_t count )
{
    NATIVE_PROFILE_PAL_CRT();
#undef strncpy
    if(strDst == NULL || strSrc == NULL || sizeInBytes == 0) {return 1;}
    
    if (sizeInBytes < count + 1)
    {
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
    if(str1 == NULL || str2 == NULL) {return 1;}
    
    return strncmp( str1, str2, num );

#define strncmp DoNotUse_*strncmp []
}

#endif //!defined(PLATFORM_BLACKFIN) 

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
