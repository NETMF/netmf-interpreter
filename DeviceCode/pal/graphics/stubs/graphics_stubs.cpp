////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

int Graphics_GetSize( int width, int height )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    return 0;
}

int Graphics_GetWidthInWords( int width )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    return 0;
}

void Graphics_Clear( const PAL_GFX_Bitmap& bitmap )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

UINT32 Graphics_GetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    return 0;
}

void Graphics_SetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y, UINT32 color )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

void Graphics_DrawLine( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

void Graphics_DrawLineRaw( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

void Graphics_DrawRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

void Graphics_DrawRoundedRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle, 
                                   int radiusX, int radiusY )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

void Graphics_DrawEllipse( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, int x, int y, int radiusX, int radiusY )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

void Graphics_DrawImage( const PAL_GFX_Bitmap& bitmapDst, const GFX_Rect& dst, const PAL_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

void Graphics_RotateImage( int angle, const PAL_GFX_Bitmap& bitmapDst, const GFX_Rect& dst, const PAL_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}

void Graphics_SetPixelsHelper( const PAL_GFX_Bitmap& bitmap, const GFX_Rect& rect, UINT32 config, GFX_SetPixelsCallback callback, void* param )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
}
