////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////
// There is a duplication of this PAL graphics implementation at 
//      DeviceCode\Targets\os\WindowsNative\Graphics.cpp
// Until we figure out a way to compile PAL library for the emulator, these two places need
// to be kept consistent!!
////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_PAL_GRAPHICS_H_
#define _DRIVERS_PAL_GRAPHICS_H_ 1

//--//

#include "Tinyhal.h"

struct Graphics_Driver
{
public:
    static int GetSize( int width, int height );
    static int GetWidthInWords( int width );

    static void Clear( const PAL_GFX_Bitmap& bitmap );

    static UINT32 GetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y );
    static void SetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y, UINT32 color );

    static void DrawLine( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 );
    static void DrawLineRaw( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 );
    static void DrawRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle );
    static void DrawRoundedRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle, 
        int radiusX, int radiusY );
    static void DrawEllipse( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, int x, int y, int radiusX, int radiusY );

    static void DrawImage( const PAL_GFX_Bitmap& bitmapDst, const GFX_Rect& dst, const PAL_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity );

    static void SetPixelsHelper( const PAL_GFX_Bitmap& bitmap, const GFX_Rect& rect, UINT32 config, GFX_SetPixelsCallback callback, void* param );

    static void RotateImage( INT16 degree, const PAL_GFX_Bitmap& dst, const GFX_Rect& dstRect, const PAL_GFX_Bitmap& src, const GFX_Rect& srcRect, UINT16 opacity );
    
   

    //--//

private:

    static const UINT32 c_MaxSize = 2097152; // 2MB

    static UINT32 GetPixelNative( const PAL_GFX_Bitmap& bitmap, int x, int y );
    static void SetPixelNative( const PAL_GFX_Bitmap& bitmap, int x, int y, UINT32 color );
    static void SetThickPixel( const PAL_GFX_Bitmap& bitmap, int x, int y, GFX_Pen& pen);

    static void DrawLineNative( const PAL_GFX_Bitmap& bitmap, GFX_Pen& pen, int x0, int y0, int x1, int y1 );
    static void DrawRectangleNative( const PAL_GFX_Bitmap& bitmap, GFX_Pen& pen, GFX_Brush& brush, const GFX_Rect& rectangle );
    static void DrawRoundedRectangleNative( const PAL_GFX_Bitmap& bitmap, GFX_Pen& pen, GFX_Brush& brush, const GFX_Rect& rectangle, 
        int radiusX, int radiusY );
    static void DrawEllipseNative( const PAL_GFX_Bitmap& bitmap, GFX_Pen& pen, GFX_Brush& brush, int x, int y, int radiusX, int radiusY );

    static UINT32* ComputePosition( const PAL_GFX_Bitmap& bitmap, int xPos, int yPos, UINT32& mask , UINT32& shift );

    static BOOL IsPixelVisible( const GFX_Rect& clipping, int x, int y );
    static BOOL ClipToVisible( const PAL_GFX_Bitmap& target, int& x, int& y, int& width, int& height, const PAL_GFX_Bitmap* pSrc, int& xSrc, int& ySrc );

    static void DrawBresLineNative( const PAL_GFX_Bitmap& bitmap, int x0, int y0, int x1, int y1, GFX_Pen& pen );

    static UINT32 NativeColorInterpolate( UINT32 colorTo, UINT32 colorFrom, UINT16 scalar );


    __inline static BYTE NativeColorRValue( UINT32 color ) { return (color & 0xF800) >> 11;   }
    __inline static BYTE NativeColorGValue( UINT32 color ) { return (color & 0x07E0) >>  5;   }
    __inline static BYTE NativeColorBValue( UINT32 color ) { return  color & 0x1F;            }
    __inline static UINT32 NativeColorFromRGB( BYTE r, BYTE g, BYTE b ) 
    { 
        ASSERT( (b <= 0x1F) && (g <= 0x3F) && (r <= 0x1F)); 
        return (r << 11) | (g << 5) | b; 
    }

    __inline static BYTE   ColorRValue ( UINT32 color           ) { return  color & 0x0000FF;          }
    __inline static BYTE   ColorGValue ( UINT32 color           ) { return (color & 0x00FF00) >>  8;   }
    __inline static BYTE   ColorBValue ( UINT32 color           ) { return (color & 0xFF0000) >> 16;   }
    __inline static UINT32 ColorFromRGB( BYTE r, BYTE g, BYTE b ) { return (b << 16) | (g << 8) | r; }


    __inline static UINT32 ConvertNativeToColor( UINT32 nativeColor )
    {
        int r = NativeColorRValue( nativeColor ) << 3; if ((r & 0x8) != 0) r |= 0x7;   // Copy LSB
        int g = NativeColorGValue( nativeColor ) << 2; if ((g & 0x4) != 0) g |= 0x3;   // Copy LSB
        int b = NativeColorBValue( nativeColor ) << 3; if ((b & 0x8) != 0) b |= 0x7;   // Copy LSB
        return ColorFromRGB( r, g, b );
    }
    __inline static UINT32 ConvertColorToNative( UINT32 color )
    {
        return NativeColorFromRGB( ColorRValue(color) >> 3, ColorGValue(color) >> 2, ColorBValue(color) >> 3 );
    }

    __inline static GFX_Pen ConvertPenToNative( const GFX_Pen& pen )
    {
        GFX_Pen nativePen;

        nativePen.thickness = pen.thickness;
        nativePen.color = ConvertColorToNative( pen.color );

        return nativePen;
    }

    __inline static GFX_Brush ConvertBrushToNative( const GFX_Brush& brush )
    {
        GFX_Brush nativeBrush;

        nativeBrush.gradientStartX = brush.gradientStartX;
        nativeBrush.gradientStartY = brush.gradientStartY;
        nativeBrush.gradientStartColor = ConvertColorToNative( brush.gradientStartColor );
        nativeBrush.gradientEndX = brush.gradientEndX;
        nativeBrush.gradientEndY = brush.gradientEndY;
        nativeBrush.gradientEndColor = ConvertColorToNative( brush.gradientEndColor );
        nativeBrush.opacity = brush.opacity;

        return nativeBrush;
    }

    //--//

    typedef void (*EllipseCallback) ( const PAL_GFX_Bitmap&, int, int, void* );

    static void EllipseAlgorithm( const PAL_GFX_Bitmap& bitmap, int radiusX, int radiusY, void* params, EllipseCallback ellipseCallback );

    static void Draw4PointsEllipse    ( const PAL_GFX_Bitmap& bitmap, int offsetX, int offsetY, void* params );
    static void Draw4PointsRoundedRect( const PAL_GFX_Bitmap& bitmap, int offsetX, int offsetY, void* params );

};

#endif  // _DRIVERS_PAL_GRAPHICS_H_
