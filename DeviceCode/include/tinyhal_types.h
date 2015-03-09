////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// these are from ADS 1.1 compiler

#ifndef _HAL_TYPES_H_
#define _HAL_TYPES_H_ 1

#include <netmf_errors.h>

#include <stddef.h>

// Note below that the MC9328 processor must use the cacheable address to access Flash because
// of Chip Erratum #12 - the LDM instruction fails under certain circumstances when accessing
// uncached memory.  memcpy() in particular sometimes violates this (when the byte count is
// divisible by 8 but not by 16) when performed on uncached memory.  This is only true for 
// ARM v1.2
#if defined(ARM_V1_2) && (defined(PROCESSOR_MC9328) || defined(PROCESSOR_TEMPLATE))

#include <string.h>

extern size_t CPU_GetCachableAddress( size_t address );

inline void* hal_memcpy( void* dst, const void* src, size_t len )
{
#undef memcpy
   return memcpy(dst, (void*)CPU_GetCachableAddress((size_t)src), len);
#define memcpy(x,y,z) hal_memcpy(x,y,z)
}

#define memcpy(x,y,z) hal_memcpy(x,y,z)

#endif

#if defined(__ADSPBLACKFIN__) || defined (__GNUC__) || defined(_ARC) || defined(__RENESAS__)
#define __int64 long long
#undef NULL
#endif

#if defined(__ADSPBLACKFIN__)
#define PLATFORM_BLACKFIN
#endif

#if defined(__arm) || defined(PLATFORM_BLACKFIN) || defined(__GNUC__) || defined(_ARC) || defined(__RENESAS__)

#undef UNICODE

typedef unsigned int BOOL;

#define TRUE  1
#define FALSE 0

typedef unsigned char      BYTE;
typedef unsigned char*     PBYTE;

typedef unsigned char      UINT8;
typedef signed   char      INT8;

typedef unsigned short int UINT16;
typedef signed   short int INT16;

typedef unsigned int       UINT32;
typedef signed   int       INT32;

typedef unsigned __int64   UINT64;
typedef signed   __int64   INT64;

#define NULL 0

typedef char               CHAR;
typedef char*              LPSTR;
typedef const char*        LPCSTR;
typedef unsigned short     WORD;
typedef unsigned long      DWORD;


#if defined (__RENESAS__)
typedef unsigned short     wchar_t;
#endif

typedef wchar_t            WCHAR;
typedef WCHAR*             LPWSTR;
typedef const WCHAR*       LPCWSTR;

#endif //defined(__arm) || defined(PLATFORM_BLACKFIN) || defined(__GNUC__) || defined(_ARC) || defined(__RENESAS__) 

#define ARRAYSIZE_CONST_EXPR(x) (sizeof(x)/sizeof(x[0]))
#if (!defined(_WIN32) && !defined(WIN32) && !defined(_WIN32_WCE))
#define ARRAYSIZE(x) ARRAYSIZE_CONST_EXPR(x) 
#endif
#define MAXSTRLEN(x) (ARRAYSIZE(x)-1)
#define ROUNDTOMULTIPLE(x,y)           ((x + sizeof(y) - 1) & ~(sizeof(y)-1)) // Only works with powers of 2.
#define CONVERTFROMSIZETOELEMENTS(x,y) ((x + sizeof(y) - 1) /   sizeof(y))
#define CONVERTFROMSIZETOHEAPBLOCKS(x) CONVERTFROMSIZETOELEMENTS(x,CLR_RT_HeapBlock)

//--//

#if !(defined(_WIN32) || defined(WIN32) || defined(_WIN32_WCE)) 
struct SYSTEMTIME
{
    WORD wYear;
    WORD wMonth;
    WORD wDayOfWeek;
    WORD wDay;
    WORD wHour;
    WORD wMinute;
    WORD wSecond;
    WORD wMilliseconds;
};
#endif 

#endif  // _HAL_TYPES_H_
