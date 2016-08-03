//-----------------------------------------------------------------------------
//
//  <No description>
//
//  Microsoft dotNetMF Project
//  Copyright �2001,2002,2009 Microsoft Corporation
//  One Microsoft Way, Redmond, Washington 98052-6399 U.S.A.
//  All rights reserved.
//  MICROSOFT CONFIDENTIAL
//
//-----------------------------------------------------------------------------

#ifndef _TINYCLR_ENDIAN_H_
#define _TINYCLR_ENDIAN_H_

#include <TinyHAL_types.h>

////////////////////////////////////////////////////////////////////////////////////////////////////
// Endian conversion helpers
//

// UINT8 is created in case of swapendian by mistakes of UINT8.
__inline UINT8 SwapEndian( UINT8 u )
{
	return u;
}

__inline UINT32 SwapEndian( UINT32 u )
{
    return (u>>24) | ((u &0xff0000UL)>>8) | ((u&0xff00UL)<<8) | ((u)<<24);
}

__inline UINT16 SwapEndian( UINT16 u )
{
    return (u>>8)|(u<<8);
}

__inline UINT64 SwapEndian( UINT64 u )
{
    UINT32 t;
    UINT64 h;
    t =  (UINT32)(( u & 0xFFFFFFFF00000000ull) >> 32);
    h =  SwapEndian( t                              );
    t =  (UINT32)( u & 0xFFFFFFFFull                );
    h |= ( ((UINT64)SwapEndian( t ))           << 32);
    return h;
}

// UINT8 is created in case of swapendian by mistakes of INT8.
__inline INT8 SwapEndian( INT8 u )
{
	return u;
}
__inline INT32 SwapEndian( INT32 u )
{
    return (INT32)SwapEndian( (UINT32)u );
}

__inline INT16 SwapEndian( INT16 u )
{
    return (INT16)SwapEndian( (UINT16)u ); 
}

__inline INT64 SwapEndian( INT64 u )
{
    UINT32 t;
    UINT64 h;
    t =  (UINT32) (( u & 0xFFFFFFFF00000000ull) >> 32);
    h =  SwapEndian( t                               );
    t =  (UINT32) ( u & 0xFFFFFFFFull                );
    h |= ( (UINT64)(SwapEndian( t ))           << 32 );
    return (INT64)h;
}

// These macros only swap the Endiannes on BIG Endian machines
#if !defined(NETMF_TARGET_BIG_ENDIAN)
#define SwapEndianIfBE( a ) a
#define SwapEndianIfBEc32( a ) a
#define SwapEndianIfBEc16( a ) a
#define SwapEndianAndAssignIfBE( a, b )
#define SwapEndianAndAssignIfBEc32( a, b )
#define SwapEndianAndAssignIfBEc16( a, b )
#else
#define SwapEndianIfBE( a ) SwapEndian( a )
#define SwapEndianIfBEc32( u ) (((u & 0xff000000UL)>>24) | ((u & 0x00ff0000UL)>>8) | ((u&0x0000ff00UL)<<8) | ((u & 0x000000ffUL)<<24))
#define SwapEndianIfBEc16( u ) (((u&0xff00)>>8)|((u&0x00ff)<<8))
#define SwapEndianAndAssignIfBE( a, b ) a = SwapEndian( b )
#define SwapEndianAndAssignIfBEc32( a, u ) a = SwapEndianIfBEc32(u)
#define SwapEndianAndAssignIfBEc16( a, u ) a = SwapEndianIfBEc16(u)
#endif

//
// Endian conversion helpers
////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // _TINYCLR_ENDIAN_H_
