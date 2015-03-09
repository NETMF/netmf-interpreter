//
// cc.h
//
// Contains compiler-specific definitions and typedefs for lwIP on ADSP-BF535.
// (Actually, only the PACK_* definitions are compiler-specific, all the rest
//  should be in sys_arch.h, I think.)
//
#ifndef _ARCH_CC_H_
#define _ARCH_CC_H_

// lwIP requires memset
//#include <string.h>

// Define platform endianness
#undef BYTE_ORDER
#if defined(BIG_ENDIAN)
#define BYTE_ORDER LWIP_BIG_ENDIAN
#else
#define BYTE_ORDER LWIP_LITTLE_ENDIAN
#endif

// Define basic types used in lwIP
typedef unsigned   char    u8_t;
typedef signed     char    s8_t;
typedef unsigned   short   u16_t;
typedef signed     short   s16_t;
typedef unsigned   long    u32_t;
typedef signed     long    s32_t;

// addresses are 32-bits long
typedef u32_t mem_ptr_t;

#define U16_F "u"
#define S16_F "d"
#define X16_F "x"
#define U32_F "lu"
#define S32_F "ld"
#define X32_F "lx"

// Compiler hints for packing structures
// (Note: this packs the struct layouts but doesn't modify compiler
//        access to the members - we're on our own for avoiding
//        alignment exceptions.)
#define PACK_STRUCT_FIELD(x) x
#define PACK_STRUCT_STRUCT
#define PACK_STRUCT_BEGIN _Pragma("pack(1)")
#define PACK_STRUCT_END   _Pragma("pack()")

// prototypes for printf(), fflush() and abort()
//#include <stdio.h>
//#include <stdlib.h>

// supply a version of (non-ANSI) isascii()
//#define isascii(i) ((int)(i) > 0 && (int)(i) < 128)

#if !defined(BUILD_RTM)

extern void debug_printf( const char *format, ... );
extern void lcd_printf( const char* format, ... );

// Plaform specific diagnostic output
//#ifdef LWIP_DEBUG
//#define LWIP_PLATFORM_DIAG(x)	\
//  do {printf x;} while(0)
//#else
#define LWIP_PLATFORM_DIAG(x) debug_printf x

//#endif

#ifndef LWIP_NOASSERT
#define LWIP_PLATFORM_ASSERT(x) \
  do {debug_printf("LWIP Assertion \"%s\" failed at line %d in %s\n", \
             x, __LINE__, __FILE__); } while(0)
#else
#define LWIP_PLATFORM_ASSERT(x)
#endif

#else

#define LWIP_PLATFORM_DIAG(x)
#define LWIP_PLATFORM_ASSERT(x)

#endif  // !defined(BUILD_RTM)

#endif // _ARCH_CC_H_
