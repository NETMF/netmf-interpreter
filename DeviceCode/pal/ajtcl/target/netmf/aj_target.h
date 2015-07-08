#ifndef _AJ_TARGET_H
#define _AJ_TARGET_H
/**
 * @file
 */
/******************************************************************************
 * Copyright AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/

#include <stdint.h>
#include <stddef.h>
#include <stdlib.h>
#include <string.h>

#undef  NDEBUG
//#define AJ_DEBUG_RESTRICT AJ_DEBUG_ALL

// *** //
// This is for using the UDP as the transport, however, the latest
// v15.04 AJ code, the UDP is very broken that there are so many syntax error
// from target\linux to aj_ardp.c, there are still syntax error. 
// wait for AJ to fix it first before proceed to test it.

//#define AJ_ARDP
#ifdef AJ_ARDP
// including the tinyhal.h here will creat problem for other AJ files that using string functions...
// to be resolved
// #include <tinyhal.h>
// #define htons(x)  SOCK_htons(x)
// #define htonl(x)  SOCK_htonl(x)
// #define ntohl(x)  SOCK_htonl(x)
// #define ntohs(x)  SOCK_htons(x)
#endif 
// ***** //

#ifndef TRUE
#define TRUE (1)
#endif

#ifndef FALSE
#define FALSE (0)
#endif

#ifndef max
#define max(x, y) ((x) > (y) ? (x) : (y))
#endif

#ifndef min
#define min(x, y) ((x) < (y) ? (x) : (y))
#endif

#define WORD_ALIGN(x) ((x & 0x3) ? ((x >> 2) + 1) << 2 : x)

#if __BYTE_ORDER == __LITTLE_ENDIAN
#define HOST_IS_LITTLE_ENDIAN  TRUE
#define HOST_IS_BIG_ENDIAN     FALSE
#else
#define HOST_IS_LITTLE_ENDIAN  FALSE
#define HOST_IS_BIG_ENDIAN     TRUE
#endif

#if defined(BUILD_RTM) || !defined(AJ_PRINTF)
#define AJ_Printf(fmat, ...) \
    do { /**/ } while (0)
#else
#include <TinyCLR_Runtime.h>


#define AJ_Printf(fmat, ...) \
    do { debug_printf(fmat, ## __VA_ARGS__); } while (0)

#if 0
#define AJ_Printf(fmat, ...) \
    do { \
        va_list args; \
        va_start(args, fmat); \
        int nBuf; \
        char szBuffer[512]; \
        nBuf = _vsnprintf((char*) szBuffer, 511, fmat, args); \
        ::OutputDebugStringA(szBuffer); \
        va_end(args); \
    } while (0) 

#endif    
#endif


#define AJ_CreateNewGUID AJ_RandBytes

#define AJ_GetDebugTime(x) _AJ_GetDebugTime(x)

#ifndef NDEBUG
extern uint8_t dbgCONFIGUREME;
extern uint8_t dbgINIT;
extern uint8_t dbgNET;
extern uint8_t dbgTARGET_CRYPTO;
extern uint8_t dbgTARGET_NVRAM;
extern uint8_t dbgTARGET_SERIAL;
extern uint8_t dbgTARGET_TIMER;
extern uint8_t dbgTARGET_UTIL;
#endif

#define AJ_ASSERT(x) /* assert(x) */

/*
 * AJ_Reboot() is a NOOP on this platform
 */
#define AJ_Reboot()

#define AJ_EXPORT

#if defined(__arm)
 int hal_snprintf( char* buffer, size_t len, const char* format, ... );
#endif
/**
/*
 * Main method allows argc, argv
 */
//#define MAIN_ALLOWS_ARGS

#endif
