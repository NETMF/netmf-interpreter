////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _BMP_H_
#define _BMP_H_

#include "..\Graphics.h"

#if defined(PLATFORM_BLACKFIN)
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

#if !defined(_WIN32)
// For non-windows build, we need to define the Bitmap header structures.
// (These are defined in wingdi.h which is indirectly linked in windows build.)

__packed struct __gnu_packed BITMAPFILEHEADER {
    UINT16 bfType;
    UINT32 bfSize;
    UINT16 bfReserved1;
    UINT16 bfReserved2;
    UINT32 bfOffBits;
};

__packed struct __gnu_packed BITMAPINFOHEADER {
    UINT32 biSize;
    INT32  biWidth;
    INT32  biHeight;
    UINT16 biPlanes;
    UINT16 biBitCount;
    UINT32 biCompression;
    UINT32 biSizeImage;
    INT32  biXPelsPerMeter;
    INT32  biYPelsPerMeter;
    UINT32 biClrUsed;
    UINT32 biClrImportant;
};

#define BI_RGB        0L
#define BI_RLE8       1L
#define BI_RLE4       2L
#define BI_BITFIELDS  3L
#define BI_JPEG       4L
#define BI_PNG        5L

typedef UINT32 COLORREF;

#endif //#if !defined(PLATFORM_WINDOWS_EMULATOR)

enum BmpEncodingType
{
    BmpUnknown      = 0,
    Bmp16Bit_565    = 1,
    Bmp24Bit_RGB    = 2,
    Bmp8Bit_Indexed = 3,
    Bmp32Bit_ARGB   = 4,    
};

struct BmpDecoder
{
    int             width;
    int             height;
    BmpEncodingType encodingType;

    HRESULT BmpInitOutput ( const UINT8* src, UINT32 srcSize );
    HRESULT BmpStartOutput( CLR_GFX_Bitmap* bitmap );

    static UINT32 BmpOutputHelper( int x, int y, UINT32 flags, UINT16& opacity, void* param );

private:
    CLR_RT_ByteArrayReader source;
    const UINT8*           palette;
    UINT8                  paletteDepth;
    bool                   isTopDown;
};

#if defined(PLATFORM_BLACKFIN)
#pragma pack()
// __packed is an ARM compiler-only keyword (equivalent of #pragma pack(1))
#endif

#if defined(PLATFORM_SH)
#pragma unpack 
#endif


#endif
