////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_GRAPHICS_DECL_H_
#define _DRIVERS_GRAPHICS_DECL_H_ 1

struct GFX_Rect
{
    int left;
    int top;
    int right;
    int bottom;

    int Width () const { return right  - left + 1; }
    int Height() const { return bottom - top + 1;  }
};

struct PAL_GFX_Bitmap
{
    int width;
    int height;
    UINT32* data;

    GFX_Rect clipping;

    UINT32 transparentColor;

    static const UINT32 c_InvalidColor = 0xFF000000;

    static const UINT16 c_OpacityTransparent = 0;
    static const UINT16 c_OpacityOpaque      = 256;

    static const UINT32 c_SetPixels_None        = 0x00000000;
    static const UINT32 c_SetPixels_First       = 0x00000001;
    static const UINT32 c_SetPixels_NewRow      = 0x00000002;
    static const UINT32 c_SetPixels_PixelHidden = 0x00000004;

    static const UINT32 c_SetPixelsConfig_NoClip       = 0x00000000;
    static const UINT32 c_SetPixelsConfig_Clip         = 0x00000001;
    static const UINT32 c_SetPixelsConfig_NoClipChecks = 0x00000002;
    static const UINT32 c_SetPixelsConfig_ReverseY     = 0x00000004;

    static void ResetClipping( PAL_GFX_Bitmap& bitmap )
    {
        bitmap.clipping.left   = 0;
        bitmap.clipping.top    = 0;
        bitmap.clipping.right  = bitmap.width;
        bitmap.clipping.bottom = bitmap.height;
    }
};

// GFX_Brush is a structure to capture all information needed to perform
// a fill. 
// Note:
// To create a solid brush, make gradientStartColor == gradientEndColor
// To create a empty (no fill) brush, make opacity = c_ColorTransparent
// To create a linear gradient brush, fill in the start and end information
struct GFX_Brush
{
    UINT32 gradientStartColor;
    int    gradientStartX;
    int    gradientStartY;
    UINT32 gradientEndColor;
    int    gradientEndX;
    int    gradientEndY;

    UINT16 opacity;
};

struct GFX_Pen
{
    UINT32 color;
    int thickness;
};

//--//

// The PAL Graphics API uses the 24bit BGR color space, the one that's used for TinyCore and
// CLR. It is the responsibility of whoever is implementing the PAL to deal with color conversion
// as neither CLR or TinyCore understands any color space other than this default one.
// For opacity, the valid value are from 0 (c_OpacityTransparent) to 256 (c_OpacityOpaque).

// Returns the size needed for a native bitmap in the given width and height.
// If the width and height provided exceeds the parameters of the given system,
// return -1.
int Graphics_GetSize( int width, int height );

// Returns the row stride as the number of 32-bit words for a bitmap of the given width.
int Graphics_GetWidthInWords( int width );

// Clears the given bitmap.
void Graphics_Clear( const PAL_GFX_Bitmap& bitmap );

// Gets the pixel value at (x, y) for the given bitmap in 24bit BGR color space.
UINT32 Graphics_GetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y );
// Sets the pixel value at (x, y) to the given color
void   Graphics_SetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y, UINT32 color );

// Draws a line from (x0, y0) to (x1, y1) using the pen (which specified the thickness and the color)
void Graphics_DrawLine( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 );

// Draws a line from (x0, y0) to (x1, y1) using the pen (which specified the thickness and the native -- lcd specific -- color)
void Graphics_DrawLineRaw( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 );

// Draws a rectangle using the specified pen, brush.
void Graphics_DrawRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle );
// Draws a rounded rectangle using the specified pen, brush, and x / y radius.
void Graphics_DrawRoundedRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle, 
                                   int radiusX, int radiusY );
// Draws an ellipse using the specified pen and brush. To draw a circle, make radiusX == radiusY
void Graphics_DrawEllipse( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, int x, int y, int radiusX, int radiusY );
// Draws the source bitmap over the destination bitmap. The src and dst rect determines the area of the bitmap to draw from and draw to. 
// Note that if the width and height from the src rect does not match the width and height of the dst rect, the src image will be stretched
// to fit into the area.
void Graphics_DrawImage( const PAL_GFX_Bitmap& bitmapDst, const GFX_Rect& dst, const PAL_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity );

void Graphics_RotateImage( int angle, const PAL_GFX_Bitmap& bitmapDst, const GFX_Rect& dst, const PAL_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity );

// This is the callback signature for Graphics_SetPixelsHelper(). This callback will be called once for each pixel in the area specified.
// The parameters are as follow:
// 1) int -- the x coordinates of the current pixel
// 2) int -- the y coordinates of the current pixel
// 3) UINT32 -- flags that communicate the status of the SetPixels operation, more specifically --
//      c_SetPixels_First           - This is the first time the callback is called, can be used to
//                                    conditionally perform any initialization work
//      c_SetPixels_NewRow          - This is the first pixel of a new row (i.e. new y value)
//      c_SetPixels_PixelHidden     - This pixel is outside of the clipping area. Note that if
//                                    c_SetPixelsConfig_NoClipChecks is set, this bit will never be
//                                    set.
// 4) UINT16& -- the return value for opacity. Note that you can set this to c_OpacityTransparent if
//               you want to leave the pixel unchanged.
// 5) void* -- this is the custom parameter that was passed into Graphics_SetPixelsHelper(), and it is
//             persisted between each callback call
// Return Value -- The color of the pixel
typedef UINT32 (*GFX_SetPixelsCallback) ( int, int, UINT32, UINT16&, void* );

// Graphics_SetPixelsHelper is designed to allow the caller to fill an area of a bitmap without incurring unnecessary cost
// of calculating the x, y position for each pixel.
// The parameters are as follow:
// 1) bitmap -- the target bitmap
// 2) rect -- the area on the bitmap to iterate through
// 3) config -- configuration for how the callbacks will be made, more specifically:
//      c_SetPixelsConfig_NoClip        - No clipping will be done, each pixel in the rect area specified will received 
//                                        a callback once in order.
//      c_SetPixelsConfig_Clip          - Clipping will be done against bitmap.clipping, and each pixel in the visible area 
//                                        will receive a callback once in order.
//      c_SetPixelsConfig_NoClipChecks  - By default, when c_SetPixelsConfig_NoClip is set, each pixel is passed through
//                                        a series of test to determine if the pixel is visible prior to the callback, and
//                                        the result is reflected in the c_SetPixels_PixelHidden flag. If those check are
//                                        unneccessary, this configuration option will save some time.
//      c_SetPixelsConfig_ReverseY      - The default order of the callback is increasing x, increasing y (top to bottom,
//                                        and within each row, left to right). Setting c_SetPixelsConfig_ReverseY will change
//                                        the order to be increasing x, decreasing y (bottom to top, and within each row, 
//                                        left to right).
// 4) param -- a custom pointer that's passed into each callback.
void Graphics_SetPixelsHelper( const PAL_GFX_Bitmap& bitmap, const GFX_Rect& rect, UINT32 config, GFX_SetPixelsCallback callback, void* param );


#endif // _DRIVERS_GRAPHICS_DECL_H_

