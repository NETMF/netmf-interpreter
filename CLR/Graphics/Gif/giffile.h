////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#ifndef _GIFFILE_H_
#ifndef _GIFFILE_H_
#define _GIFFILE_H_

/**************************************************************************\
* 
*
* Module Name:
*
*   giffile.hpp
*
* Abstract:
*
*   Header file with gif file structures.
*
* Revision History:
*
*   6/8/1999 t-aaronl
*       Created it.
*
\**************************************************************************/


#define GIFPLAINTEXTEXTENSIONSIZE 13
#define GIFAPPEXTENSIONHEADERSIZE 11

#if defined(_MSC_VER) || defined(PLATFORM_BLACKFIN) 
#pragma pack(1)
// __packed is an ARM compiler-only keyword (equivalent of #pragma pack(1))
#define __packed 
#endif

#if defined(PLATFORM_SH)
#pragma pack 1
// __packed is an ARM compiler-only keyword (equivalent of #pragma pack(1))
#define __packed
#endif


#if defined(__GNUC__)
#define __packed
#define __gnu_packed __attribute__((packed))
#else


#define __gnu_packed
#endif

struct __gnu_packed GifFileHeader  //13 bytes
{
    BYTE signature[6];
    __packed WORD __gnu_packed LogicScreenWidth;
    __packed WORD __gnu_packed LogicScreenHeight;
    BYTE globalcolortablesize: 3;  //bit fields in reverse significant order
    BYTE sortflag: 1;
    BYTE colorresolution: 3;
    BYTE globalcolortableflag: 1;  // <- most significant
    BYTE backgroundcolor;
    BYTE pixelaspect;
};

__packed struct __gnu_packed GifPaletteEntry
{
    BYTE red;
    BYTE green;
    BYTE blue;
};

__packed struct __gnu_packed GifColorTable  //palette is up to 3*256 BYTEs
{
    GifPaletteEntry colors[256];
};

struct __gnu_packed GifImageDescriptor  //9 bytes
{
  //BYTE imageseparator;  //=0x2C
    __packed WORD __gnu_packed left;
    __packed WORD __gnu_packed top;
    __packed WORD __gnu_packed width;
    __packed WORD __gnu_packed height;
    BYTE localcolortablesize: 3;  //bit fields in reverse significant order
    BYTE reserved: 2;
    BYTE sortflag: 1;
    BYTE interlaceflag: 1;
    BYTE localcolortableflag: 1;  // <- most significant
};

struct __gnu_packed GifGraphicControlExtension  //6 bytes
{
  //BYTE extensionintroducer;  //=0x21
  //BYTE graphiccontrollabel;  //=0xF9
    BYTE blocksize;
    BYTE transparentcolorflag: 1;  //bit fields in reverse significant order
    BYTE userinputflag: 1;
    BYTE disposalmethod: 3;
    BYTE reserved: 3;  // <- most significant

    __packed WORD __gnu_packed delaytime;  //in hundreths of a second
    BYTE transparentcolorindex;
};

#if defined(_MSC_VER) || defined(PLATFORM_BLACKFIN)
#pragma pack()
#endif

#if defined(PLATFORM_SH)
#pragma unpack 
#endif



#endif
