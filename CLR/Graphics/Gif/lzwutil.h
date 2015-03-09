////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#ifndef _LZWUTIL_H_
#ifndef _LZWUTIL_H_
#define _LZWUTIL_H_

/*****************************************************************************
	External APIs.  Clients of this library must implement the following
	APIs for debug purposes - they are not compiled in in non-debug versions
	of the code.
******************************************************************* JohnBo **/

#define LZW_ASSERT(c)

/* Error checking (assert) macros associated with this. */
//#define LZW_B8(i) (LZW_ASSERT((i)<256 && (i)>=0),
//	static_cast<unsigned char>(i))
//#define LZW_B16(i) (LZW_ASSERT((i)<65536 && (i)>=0),
//	static_cast<unsigned short>(i))
#define LZW_B8(i) (static_cast<unsigned char>(i))
#define LZW_B16(i) (static_cast<unsigned short>(i))

/*****************************************************************************
	Utility to set a two byte quantity in the standard GIF little endian
	format, likewise for four bytes (GAMMANOW extension.)
******************************************************************* JohnBo **/
inline void GIF16Bit(unsigned char *pb, int i)
	{
	LZW_ASSERT(i >= 0 && i < 65536);
	pb[0] = static_cast<unsigned char>(i);
	pb[1] = static_cast<unsigned char>(i>>8);
	}

/* Inverse operation. */
inline unsigned short GIFU16(const unsigned char *pb)
	{
	return static_cast<unsigned short>(pb[0] + (pb[1] << 8));
	}


inline void GIF32Bit(unsigned char *pb, unsigned int i)
	{
	pb[0] = static_cast<unsigned char>(i);
	pb[1] = static_cast<unsigned char>(i>>8);
	pb[2] = static_cast<unsigned char>(i>>16);
	pb[3] = static_cast<unsigned char>(i>>24);
	}

/* Inverse operation. */
inline unsigned int GIFU32(const unsigned char *pb)
	{
	return pb[0] + (pb[1] << 8) + (pb[2] << 16) + (pb[3] << 24);
	}

#endif
