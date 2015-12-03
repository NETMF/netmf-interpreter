////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "graphics.h"

int Graphics_GetSize( int width, int height )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    return Graphics_Driver::GetSize( width, height );
}

int Graphics_GetWidthInWords( int width )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    return Graphics_Driver::GetWidthInWords( width );
}

void Graphics_Clear( const PAL_GFX_Bitmap& bitmap )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::Clear( bitmap );
}

UINT32 Graphics_GetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    return Graphics_Driver::GetPixel( bitmap, x, y );
}

void Graphics_SetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y, UINT32 color )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::SetPixel( bitmap, x , y, color );
}

void Graphics_DrawLine( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::DrawLine( bitmap, pen, x0, y0, x1, y1 );
}

void Graphics_DrawLineRaw( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::DrawLineRaw( bitmap, pen, x0, y0, x1, y1 );
}
void Graphics_DrawRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::DrawRectangle( bitmap, pen, brush, rectangle );
}

void Graphics_DrawRoundedRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle,
                                   int radiusX, int radiusY )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::DrawRoundedRectangle( bitmap, pen, brush, rectangle, radiusX, radiusY );
}

void Graphics_DrawEllipse( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, int x, int y, int radiusX, int radiusY )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::DrawEllipse( bitmap, pen, brush, x, y, radiusX, radiusY );
}

void Graphics_DrawImage( const PAL_GFX_Bitmap& bitmapDst, const GFX_Rect& dst, const PAL_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::DrawImage( bitmapDst, dst, bitmapSrc, src, opacity );
}

void Graphics_RotateImage( int angle, const PAL_GFX_Bitmap& bitmapDst, const GFX_Rect& dst, const PAL_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::RotateImage( angle, bitmapDst, dst, bitmapSrc, src, opacity );
}

void Graphics_SetPixelsHelper( const PAL_GFX_Bitmap& bitmap, const GFX_Rect& rect, UINT32 config, GFX_SetPixelsCallback callback, void* param )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Graphics_Driver::SetPixelsHelper( bitmap, rect, config, callback, param );
}

//--//

// DivHelper is a support class used internally of Graphics_Driver to assist calculating gradient fills
struct DivHelper
{
    // DivHelper uses the midpoint technique to perform integer division using only addition and few multiplications.
    // This is useful when we need to calculate y = a*x / b + c over a range of x values. All we need is a starting
    // point where the value of y is known.
    // Usage:
    // For example, we have f(x) = d*x / e + f, where we know f(n) = m
    // We construct the DivHelper struct by
    //          DivHelper helper(d, e, m)
    // By calling helper.Next(), we will get the result of f(n), f(n+1), f(n+2) ... etc.
    //
    // Note that if b is 0, Next() will always return initY

    int a;
    int b;
    int D;
    int y;
    int stride;
    int DIncStride;
    int DIncStridePlus1;
    bool first;

    // DivHelper(int a, int b, int initY)
    // where a and b are the constant in the formula y = a*x / b + c
    //       initY is the initial value of Y
    DivHelper(int _a, int _b, int initY)
    {
        a = _a;
        b = _b;

        if (b != 0) stride = a / b;

        DIncStride = 2 * (a - b * stride);
        DIncStridePlus1 = 2 * (a - b * (stride + 1));

        Reset(initY);
    }

    // int Next()
    // return the next y value.
    int Next()
    {
        if (first)
        {
            if (b == 0) return y; // if b == 0, first will always be true

            first = false;
            return y;
        }

        if (D > 0)
        {
            D += DIncStride;
            y += stride;
        }
        else
        {
            D += DIncStridePlus1;
            y += stride + 1;
        }
        return y;
    }

    // void Reset(int initY)
    // set a new initY value.
    void Reset(int initY)
    {
        y = initY;
        D = 2 * a - b * (2 * stride + 1);
        first = true;
    }
};

//--//

UINT32 Graphics_Driver::GetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    return ConvertNativeToColor( GetPixelNative( bitmap, x, y ) );
}

void Graphics_Driver::SetPixel( const PAL_GFX_Bitmap& bitmap, int x, int y, UINT32 color )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    SetPixelNative( bitmap, x, y, ConvertColorToNative( color ) );
}

void Graphics_Driver::DrawLine( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    GFX_Pen nativePen = ConvertPenToNative( pen );
    DrawLineNative( bitmap, nativePen, x0, y0, x1, y1 );
}

void Graphics_Driver::DrawRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    GFX_Pen nativePen = ConvertPenToNative( pen );
    GFX_Brush nativeBrush = ConvertBrushToNative( brush );
    DrawRectangleNative( bitmap, nativePen, nativeBrush, rectangle );
}

void Graphics_Driver::DrawRoundedRectangle( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, const GFX_Rect& rectangle,
                                           int radiusX, int radiusY )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    GFX_Pen nativePen = ConvertPenToNative( pen );
    GFX_Brush nativeBrush = ConvertBrushToNative( brush );
    DrawRoundedRectangleNative( bitmap, nativePen, nativeBrush, rectangle, radiusX, radiusY );
}

void Graphics_Driver::DrawEllipse( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, const GFX_Brush& brush, int x, int y, int radiusX, int radiusY )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    GFX_Pen nativePen = ConvertPenToNative( pen );
    GFX_Brush nativeBrush = ConvertBrushToNative( brush );
    DrawEllipseNative( bitmap, nativePen, nativeBrush, x, y, radiusX, radiusY );
}

int Graphics_Driver::GetSize( int width, int height )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    if (width < 32768 && height < 32768)
    {
        // if width and height are both less than 32768 (0x8000), the maximum size would be less than
        // 0x7FFF * 0x7FFF * 4 = 0xFFFC0004, which would fit in UINT32. So no need to do the 64-bit
        // math.
        UINT32 size = GetWidthInWords( width ) * height * sizeof(UINT32);

        if (size > c_MaxSize) return -1;

        return size;
    }
    else
    {
        UINT64 size = (UINT64)GetWidthInWords( width ) * height * sizeof(UINT32);

        if (size > (UINT64)c_MaxSize) return -1;

        return (int)size;
    }
}

int Graphics_Driver::GetWidthInWords( int width )
{
    return (width * 16 + 31) / 32;
}

void Graphics_Driver::Clear( const PAL_GFX_Bitmap& bitmap )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    memset( bitmap.data, 0, GetSize( bitmap.width, bitmap.height ) );
}

UINT32 Graphics_Driver::GetPixelNative( const PAL_GFX_Bitmap& bitmap, int x, int y )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    if(IsPixelVisible( bitmap.clipping, x, y ))
    {
        UINT32  mask, shift;
        UINT32* data = ComputePosition( bitmap, x, y, mask, shift );

        return (*data & mask) >> shift;
    }
    else
    {
        return 0;
    }
}

void Graphics_Driver::SetPixelNative( const PAL_GFX_Bitmap& bitmap, int x, int y, UINT32 color )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    if(IsPixelVisible( bitmap.clipping, x, y ))
    {
        UINT32  mask, shift;
        UINT32* data = ComputePosition( bitmap, x, y, mask, shift );

        *data &= ~mask;
        *data |= color << shift;
    }
}

void Graphics_Driver::SetThickPixel( const PAL_GFX_Bitmap& bitmap, int x, int y, GFX_Pen& pen)
{
    int thickness = pen.thickness;
    UINT32 color = pen.color;

    // r^2 = x^2 + y ^2

    SetPixelNative(bitmap, x, y, color);

    if(thickness > 1)
    {
        int dx = 0;
        int dy = thickness;

        int rr = thickness * thickness + (thickness / 2);

        while(dx <= dy)
        {
            for(int x2 = x - dx; x2 < x + dx; x2++)
            {
                SetPixelNative(bitmap, x2, y + dy, color);                        
                SetPixelNative(bitmap, x2, y - dy, color);                        
            }

            for(int x2 = x - dy; x2 < x + dy; x2++)
            {
                SetPixelNative(bitmap, x2, y + dx, color);                        
                SetPixelNative(bitmap, x2, y - dx, color);                        
            }
            
            if(dx * dx + dy * dy > rr)
            {
                dy--;
            }

            dx++;
        }
    }
}

void Graphics_Driver::DrawLineRaw( const PAL_GFX_Bitmap& bitmap, const GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    GFX_Pen nativePen = Graphics_Driver::ConvertPenToNative( pen );
    nativePen.color = LCD_ConvertColor(nativePen.color);

    // TODO: thickness
    // thickness value is ignore -- we always assume it's 1 pixel, for now
    DrawBresLineNative( bitmap, x0, y0, x1, y1, nativePen );
}

void Graphics_Driver::DrawLineNative( const PAL_GFX_Bitmap& bitmap, GFX_Pen& pen, int x0, int y0, int x1, int y1 )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    // TODO: thickness
    // thickness value is ignore -- we always assume it's 1 pixel, for now
    DrawBresLineNative( bitmap, x0, y0, x1, y1, pen );
}

void Graphics_Driver::DrawRectangleNative( const PAL_GFX_Bitmap& bitmap, GFX_Pen& pen, GFX_Brush& brush, const GFX_Rect& rectangle )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    if (rectangle.Width() <= 0 || rectangle.Height() <= 0) return;

    int x, y;

    x  = rectangle.left;
    y  = rectangle.top;

    int outset = pen.thickness / 2;
    int inset  = pen.thickness - outset;

    int outsetX      = x - outset;
    int outsetY      = y - outset;
    int outsetWidth  = rectangle.Width() + 2 * outset;
    int outsetHeight = rectangle.Height() + 2 * outset;

    int insetX      = x + inset;
    int insetY      = y + inset;
    int insetWidth  = rectangle.Width() - 2 * inset;
    int insetHeight = rectangle.Height() - 2 * inset;

    int stride = GetWidthInWords( bitmap.width ) * 2;

    const UINT16 opacity = brush.opacity;

    int xSrc = 0, ySrc = 0;
    // If the outset rect is completely outside of the drawing region, we can safely return (inset rect is always inside the outset rect)
    if (ClipToVisible( bitmap, outsetX, outsetY, outsetWidth, outsetHeight, NULL, xSrc, ySrc ) == false) return;

    // If the inset rectangle is completely outside of the drawing region, the insetWidth and insetHeight would be set to 0
    // (We have to keep going because the outset rect is at least partly visible)
    ClipToVisible( bitmap, insetX , insetY , insetWidth , insetHeight , NULL, xSrc, ySrc );

    // Outline
    if (pen.thickness > 0)
    {
        UINT16* curRow = ((UINT16*)bitmap.data) + outsetY * stride + outsetX;
        UINT16* curPixel;
        for (int row = outsetY; row < outsetY + outsetHeight; row++, curRow += stride)
        {
            curPixel = curRow;
            if (row >= insetY && row < insetY + insetHeight)
            {
                for (int col = outsetX; col < insetX; col++, curPixel++)
                {
                    *curPixel = pen.color;
                }

                curPixel += insetWidth;
                for (int col = insetX + insetWidth; col < outsetX + outsetWidth; col++, curPixel++)
                {
                    *curPixel = pen.color;
                }
            }
            else
            {
                for (int col = outsetX; col < outsetX + outsetWidth; col++, curPixel++)
                {
                    *curPixel = pen.color;
                }
            }
        }
    }

    // Fills (Gradient / Translucent / Solid)
    if (opacity != 0)
    {
        int gradientDeltaY = brush.gradientEndY - brush.gradientStartY;
        int gradientDeltaX = brush.gradientEndX - brush.gradientStartX;

        if (brush.gradientStartColor == brush.gradientEndColor || (gradientDeltaX == 0 && gradientDeltaY == 0))
        {
            // Solid fill (including Translucent fill)
            UINT32 fillColor = brush.gradientStartColor;

            if (insetWidth > 0 && insetHeight > 0)
            {
                UINT16* curRow   = ((UINT16*)bitmap.data) + insetY * stride + insetX;
                UINT16* curPixel = curRow;

                if (opacity == PAL_GFX_Bitmap::c_OpacityOpaque)
                {
                    for (int row = 0; row < insetHeight; row++, curRow += stride)
                    {
                        curPixel = curRow;
                        for (int col = 0; col < insetWidth; col++, curPixel++)
                        {
                            *curPixel = fillColor;
                        }
                    }
                }
                else // if (opacity != PAL_GFX_Bitmap::c_OpacityOpaque)
                {
                    UINT16 lastPixel = *curPixel;
                    UINT16 interpolated = NativeColorInterpolate( fillColor, lastPixel, opacity );
                    for (int row = 0; row < insetHeight; row++, curRow += stride)
                    {
                        curPixel = curRow;
                        for (int col = 0; col < insetWidth; col++, curPixel++)
                        {
                            if (*curPixel != lastPixel)
                            {
                                lastPixel = *curPixel;
                                interpolated = NativeColorInterpolate( fillColor, lastPixel, opacity );
                            }
                            *curPixel = interpolated;
                        }
                    }
                }
            }
        }
        else
        {
            // Gradient Fill
            if (gradientDeltaX < 0 || ((gradientDeltaX == 0) && gradientDeltaY < 0))
            {
                // Make sure the gradient always go from left to right (x is always increasing)
                // And if it's vertical (no change in x), from top to bottom.
                UINT32 temp;

                temp = brush.gradientEndX;
                brush.gradientEndX = brush.gradientStartX;
                brush.gradientStartX = temp;

                temp = brush.gradientEndY;
                brush.gradientEndY = brush.gradientStartY;
                brush.gradientStartY = temp;

                temp = brush.gradientEndColor;
                brush.gradientEndColor = brush.gradientStartColor;
                brush.gradientStartColor = temp;

                gradientDeltaX *= -1; // so gradientDeltaX is always >= 0
            }

            if (gradientDeltaY < 0) gradientDeltaY *= -1;

            const int LIMIT = 1 << 12;

            UINT16* curRow = ((UINT16*)bitmap.data) + insetY * stride + insetX;

            UINT32 scalar = (gradientDeltaY * gradientDeltaY * LIMIT) / (gradientDeltaY * gradientDeltaY + gradientDeltaX * gradientDeltaX);

            UINT16 colorCache[257];
            bool   colorCacheValid[257];

            for (int i = 0; i < 257; i++)
            {
                colorCacheValid[i] = false;
            }

            int scaleGradientTopLeft;
            int scaleGradientTopRight;
            int scaleGradientBottomLeft;
            int gradientTopLeftX;
            int gradientTopLeftY;

            if (brush.gradientEndY > brush.gradientStartY)
            {
                scaleGradientTopLeft    = 0;
                scaleGradientTopRight   = LIMIT - scalar;
                scaleGradientBottomLeft = scalar;
                gradientTopLeftX        = brush.gradientStartX - insetX;
                gradientTopLeftY        = brush.gradientStartY - insetY;
            }
            else
            {
                scaleGradientTopLeft    = scalar;
                scaleGradientTopRight   = LIMIT;
                scaleGradientBottomLeft = 0;
                gradientTopLeftX        = brush.gradientStartX - insetX;
                gradientTopLeftY        = brush.gradientEndY   - insetY;
            }

            int diffX = 0;
            int diffY = 0;

            if (gradientDeltaX != 0) diffX = (scaleGradientTopRight   - scaleGradientTopLeft) * gradientTopLeftX / gradientDeltaX;
            if (gradientDeltaY != 0) diffY = (scaleGradientBottomLeft - scaleGradientTopLeft) * gradientTopLeftY / gradientDeltaY;

            int scaleLeft = scaleGradientTopLeft - diffX - diffY;

            DivHelper widthDivHelper(scaleGradientTopRight - scaleGradientTopLeft, gradientDeltaX, 0);
            DivHelper heightDivHelper(scaleGradientBottomLeft - scaleGradientTopLeft, gradientDeltaY, scaleLeft);

            for (int j = 0; j < insetHeight; j++, curRow += stride)
            {
                widthDivHelper.Reset(heightDivHelper.Next());

                UINT16* curPixel = curRow;
                for (int i = 0; i < insetWidth; i++, curPixel++)
                {
                    int scale = widthDivHelper.Next();

                    if (scale < 0)
                    {
                        *curPixel = (opacity != PAL_GFX_Bitmap::c_OpacityOpaque) ? NativeColorInterpolate( brush.gradientStartColor, *curPixel, opacity ) : brush.gradientStartColor;
                    }
                    else if (scale > LIMIT)
                    {
                        *curPixel = (opacity != PAL_GFX_Bitmap::c_OpacityOpaque) ? NativeColorInterpolate( brush.gradientEndColor, *curPixel, opacity ) : brush.gradientEndColor;
                    }
                    else
                    {
                        int scale256 = scale * 256 / LIMIT;

                        if (colorCacheValid[scale256] == false)
                        {
                            colorCache[scale256] = NativeColorInterpolate(brush.gradientEndColor, brush.gradientStartColor, scale256 );
                            colorCacheValid[scale256] = true;
                        }

                        if (opacity != PAL_GFX_Bitmap::c_OpacityOpaque)
                        {
                            *curPixel = NativeColorInterpolate( colorCache[scale256], *curPixel, opacity );
                        }
                        else
                        {
                            *curPixel = colorCache[scale256];
                        }
                    }
                }
            }
        }
    } //(opacity != 0)
}

struct Draw4PointsRoundedRectParams
{
    int x1;
    int y1;
    int x2;
    int y2;

    GFX_Pen* pen;
    GFX_Brush* brush;

    int lastFillOffsetY;
};

void Graphics_Driver::DrawRoundedRectangleNative( const PAL_GFX_Bitmap& bitmap, GFX_Pen& pen, GFX_Brush& brush, const GFX_Rect& rectangle,
                                                 int radiusX, int radiusY )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    if (rectangle.Width() <= 0 || rectangle.Height() <= 0 || radiusX < 0 || radiusY < 0) return;

    int x, y, x2, y2;

    x  = rectangle.left;
    y  = rectangle.top;
    x2 = rectangle.right;
    y2 = rectangle.bottom;

    if (radiusX > 0 && radiusY > 0)
    {
        // Set the coordinates to reflect the centers of 4 quarter circles
        Draw4PointsRoundedRectParams params;

        params.x1 =  x + radiusX;
        params.y1 =  y + radiusY;
        params.x2 = x2 - radiusX;
        params.y2 = y2 - radiusY;

        // if the corners ended up overlapping, then don't do anything
        if (params.x2 < params.x1 || params.y2 < params.y1) return;

        params.pen = &pen;
        params.brush = NULL;

        EllipseAlgorithm( bitmap, radiusX, radiusY, &params, &Draw4PointsRoundedRect );

        DrawBresLineNative( bitmap, params.x1,        y , params.x2,        y , pen );
        DrawBresLineNative( bitmap,        x , params.y1,        x , params.y2, pen );
        DrawBresLineNative( bitmap,        x2, params.y1,        x2, params.y2, pen );
        DrawBresLineNative( bitmap, params.x1,        y2, params.x2,        y2, pen );

        // TODO - fill rounded rectangle
    }
}

void Graphics_Driver::Draw4PointsRoundedRect( const PAL_GFX_Bitmap& bitmap, int offsetX, int offsetY, void* params )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Draw4PointsRoundedRectParams* p = (Draw4PointsRoundedRectParams*) params;

    if (p->pen != NULL)
    {
        if (p->pen->thickness < 2)
        {
            /// For thickness == 1 use the faster path.
            UINT32 color = p->pen->color;      

            SetPixelNative( bitmap, p->x2 + offsetX, p->y2 + offsetY, color );
            SetPixelNative( bitmap, p->x1 - offsetX, p->y2 + offsetY, color );
            SetPixelNative( bitmap, p->x2 + offsetX, p->y1 - offsetY, color );
            SetPixelNative( bitmap, p->x1 - offsetX, p->y1 - offsetY, color );
        }
        else
        {
            SetThickPixel( bitmap, p->x2 + offsetX, p->y2 + offsetY, *p->pen );
            SetThickPixel( bitmap, p->x1 - offsetX, p->y2 + offsetY, *p->pen );
            SetThickPixel( bitmap, p->x2 + offsetX, p->y1 - offsetY, *p->pen );
            SetThickPixel( bitmap, p->x1 - offsetX, p->y1 - offsetY, *p->pen );
        }
    }
}

struct Draw4PointsEllipseParams
{
    int centerX;
    int centerY;

    GFX_Pen* pen;

    GFX_Brush* brush;
    int lastFillOffsetY;

};

void Graphics_Driver::DrawEllipseNative( const PAL_GFX_Bitmap& bitmap, GFX_Pen& pen, GFX_Brush& brush, int x, int y, int radiusX, int radiusY )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    
    Draw4PointsEllipseParams params;

    params.centerX = x;
    params.centerY = y;

    /// If Fill is expected, then do fill part first before drawing outline.
    if (brush.opacity != PAL_GFX_Bitmap::c_OpacityTransparent)
    {
        params.pen   = NULL;
        params.brush   = &brush;
        params.lastFillOffsetY = -1;
        EllipseAlgorithm( bitmap, radiusX, radiusY, &params, &Draw4PointsEllipse );
    }
    
    params.pen   = &pen;
    params.brush   = NULL;

    EllipseAlgorithm( bitmap, radiusX, radiusY, &params, &Draw4PointsEllipse );
}

void Graphics_Driver::Draw4PointsEllipse( const PAL_GFX_Bitmap& bitmap, int offsetX, int offsetY, void* params )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    Draw4PointsEllipseParams* p = (Draw4PointsEllipseParams*) params;
    int centerX = p->centerX;
    int centerY = p->centerY;
    
    if (p->pen != NULL)
    {
        if (p->pen->thickness < 2)
        {
            /// For thickness == 1 use the faster path.
            UINT32 color = p->pen->color;      

            SetPixelNative( bitmap, centerX + offsetX, centerY + offsetY, color );
            SetPixelNative( bitmap, centerX - offsetX, centerY + offsetY, color );
            SetPixelNative( bitmap, centerX + offsetX, centerY - offsetY, color );
            SetPixelNative( bitmap, centerX - offsetX, centerY - offsetY, color );
        }
        else
        {
            SetThickPixel( bitmap, centerX + offsetX, centerY + offsetY, *p->pen );
            SetThickPixel( bitmap, centerX - offsetX, centerY + offsetY, *p->pen );
            SetThickPixel( bitmap, centerX + offsetX, centerY - offsetY, *p->pen );
            SetThickPixel( bitmap, centerX - offsetX, centerY - offsetY, *p->pen );            
        }
    }

    if (p->brush != NULL)
    {
        int opacity = p->brush->opacity;

        /// Fill only if this is not a NULL_BRUSH and we have not painted this scan line already.
        if ((opacity != PAL_GFX_Bitmap::c_OpacityTransparent) && (p->lastFillOffsetY != offsetY))
        {
            int gradientDeltaY = p->brush->gradientEndY - p->brush->gradientStartY;
            int gradientDeltaX = p->brush->gradientEndX - p->brush->gradientStartX;

            int startX = centerX - offsetX + 1;
            int endX = centerX + offsetX;
            int startY[2] = {centerY - offsetY, centerY + offsetY};
            int stride = GetWidthInWords( bitmap.width ) * 2;

            p->lastFillOffsetY = offsetY;

            if (p->brush->gradientStartColor == p->brush->gradientEndColor || (gradientDeltaX == 0 && gradientDeltaY == 0))
            {
                // Solid fill (including Translucent fill)
                UINT32 fillColor = p->brush->gradientStartColor;

                for(int r = 0; r < 2; r++)
                {
                    if (opacity == PAL_GFX_Bitmap::c_OpacityOpaque)
                    {                    
                        for (int col = startX; col < endX; col++)
                        {
                            SetPixelNative( bitmap, col, startY[r], fillColor );                            
                        }                    
                    }
                    else // if (opacity != PAL_GFX_Bitmap::c_OpacityOpaque)
                    {
                        UINT16 lastPixel = 0;
                        UINT16 interpolated = 0;
                        for (int col = startX; col < endX; col++)
                        {                 
                            lastPixel = GetPixelNative(bitmap, col, startY[r]);
                            interpolated = NativeColorInterpolate( fillColor, lastPixel, opacity );
                            SetPixelNative( bitmap, col, startY[r], interpolated );
                        }
                    }

                    /// Do only once when y offset shift is zero.
                    if (offsetY == 0)
                        break;
                }
            }       
        }    
    }
}

struct PAL_RADIAN
{
    float cos;
    float sin;
};

static const PAL_RADIAN c_PAL_radians[] =
{
    { 1.0f, 0.0f },
    { 0.999847695156391f, 0.017452406437284f },
    { 0.999390827019096f, 0.034899496702501f },
    { 0.998629534754574f, 0.052335956242944f },
    { 0.997564050259824f, 0.069756473744125f },
    { 0.996194698091746f, 0.087155742747658f },
    { 0.994521895368273f, 0.104528463267653f },
    { 0.992546151641322f, 0.121869343405147f },
    { 0.990268068741570f, 0.139173100960065f },
    { 0.987688340595138f, 0.156434465040231f },
    { 0.984807753012208f, 0.173648177666930f },
    { 0.981627183447664f, 0.190808995376545f },
    { 0.978147600733806f, 0.207911690817759f },
    { 0.974370064785235f, 0.224951054343865f },
    { 0.970295726275996f, 0.241921895599668f },
    { 0.965925826289068f, 0.258819045102521f },
    { 0.961261695938319f, 0.275637355816999f },
    { 0.956304755963035f, 0.292371704722737f },
    { 0.951056516295154f, 0.309016994374947f },
    { 0.945518575599317f, 0.325568154457157f },
    { 0.939692620785908f, 0.342020143325669f },
    { 0.933580426497202f, 0.358367949545300f },
    { 0.927183854566787f, 0.374606593415912f },
    { 0.920504853452440f, 0.390731128489274f },
    { 0.913545457642601f, 0.406736643075800f },
    { 0.906307787036650f, 0.422618261740699f },
    { 0.898794046299167f, 0.438371146789077f },
    { 0.891006524188368f, 0.453990499739547f },
    { 0.882947592858927f, 0.469471562785891f },
    { 0.874619707139396f, 0.484809620246337f },
    { 0.866025403784439f, 0.500000000000000f },
    { 0.857167300702112f, 0.515038074910054f },
    { 0.848048096156426f, 0.529919264233205f },
    { 0.838670567945424f, 0.544639035015027f },
    { 0.829037572555042f, 0.559192903470747f },
    { 0.819152044288992f, 0.573576436351046f },
    { 0.809016994374947f, 0.587785252292473f },
    { 0.798635510047293f, 0.601815023152048f },
    { 0.788010753606722f, 0.615661475325658f },
    { 0.777145961456971f, 0.629320391049837f },
    { 0.766044443118978f, 0.642787609686539f },
    { 0.754709580222772f, 0.656059028990507f },
    { 0.743144825477394f, 0.669130606358858f },
    { 0.731353701619171f, 0.681998360062498f },
    { 0.719339800338651f, 0.694658370458997f },
    { 0.707106781186548f, 0.707106781186547f },
    { 0.694658370458997f, 0.719339800338651f },
    { 0.681998360062498f, 0.731353701619170f },
    { 0.669130606358858f, 0.743144825477394f },
    { 0.656059028990507f, 0.754709580222772f },
    { 0.642787609686539f, 0.766044443118978f },
    { 0.629320391049838f, 0.777145961456971f },
    { 0.615661475325658f, 0.788010753606722f },
    { 0.601815023152048f, 0.798635510047293f },
    { 0.587785252292473f, 0.809016994374947f },
    { 0.573576436351046f, 0.819152044288992f },
    { 0.559192903470747f, 0.829037572555042f },
    { 0.544639035015027f, 0.838670567945424f },
    { 0.529919264233205f, 0.848048096156426f },
    { 0.515038074910054f, 0.857167300702112f },
    { 0.500000000000000f, 0.866025403784439f },
    { 0.484809620246337f, 0.874619707139396f },
    { 0.469471562785891f, 0.882947592858927f },
    { 0.453990499739547f, 0.891006524188368f },
    { 0.438371146789077f, 0.898794046299167f },
    { 0.422618261740699f, 0.906307787036650f },
    { 0.406736643075800f, 0.913545457642601f },
    { 0.390731128489274f, 0.920504853452440f },
    { 0.374606593415912f, 0.927183854566787f },
    { 0.358367949545300f, 0.933580426497202f },
    { 0.342020143325669f, 0.939692620785908f },
    { 0.325568154457157f, 0.945518575599317f },
    { 0.309016994374947f, 0.951056516295154f },
    { 0.292371704722737f, 0.956304755963035f },
    { 0.275637355816999f, 0.961261695938319f },
    { 0.258819045102521f, 0.965925826289068f },
    { 0.241921895599668f, 0.970295726275996f },
    { 0.224951054343865f, 0.974370064785235f },
    { 0.207911690817759f, 0.978147600733806f },
    { 0.190808995376545f, 0.981627183447664f },
    { 0.173648177666930f, 0.984807753012208f },
    { 0.156434465040231f, 0.987688340595138f },
    { 0.139173100960066f, 0.990268068741570f },
    { 0.121869343405147f, 0.992546151641322f },
    { 0.104528463267653f, 0.994521895368273f },
    { 0.087155742747658f, 0.996194698091746f },
    { 0.069756473744125f, 0.997564050259824f },
    { 0.052335956242944f, 0.998629534754574f },
    { 0.034899496702501f, 0.999390827019096f },
    { 0.017452406437283f, 0.999847695156391f },
};

void Graphics_Driver::RotateImage( INT16 degree, const PAL_GFX_Bitmap& dst, const GFX_Rect& dstRect, const PAL_GFX_Bitmap& src, const GFX_Rect& srcRect, UINT16 opacity )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    float  cos, sin;
    int     yMin, yMax, xMin, xMax;
    int     index;

    degree %= 360; if(degree < 0) degree = 360 + degree;

    //If it's just a translation, do the BitBlt instead
    if(degree == 0)
    {
        DrawImage( dst, dstRect, src, srcRect, opacity );
        return;
    }

    index = (degree % 90);

    cos = (c_PAL_radians[index].cos);
    sin = (c_PAL_radians[index].sin);

    if(degree >= 270) 
    {
        float tmp = cos;
        cos = sin;
        sin = -tmp;
    }
    else if(degree >= 180)
    {
        cos = -cos;
        sin = -sin;
    }
    else if(degree >= 90)
    {
        float tmp = cos;
        cos = -sin;
        sin = tmp;
    }

    float xCenterSrc = (srcRect.right + srcRect.left  ) / 2.0f;
    float yCenterSrc = (srcRect.top   + srcRect.bottom) / 2.0f;
    float xCenterDst = (dstRect.right + dstRect.left  ) / 2.0f;
    float yCenterDst = (dstRect.top   + dstRect.bottom) / 2.0f;
        

    xMin = dstRect.left;
    xMax = dstRect.right;

    yMin = dstRect.top;
    yMax = dstRect.bottom;


    for(int y = yMin; y <= yMax; y++)
    {
        float dy = (y - yCenterDst);
        
        float cosY = cos * dy;
        float sinY = sin * dy;
        
        for(int x = xMin; x <= xMax; x++)
        {
            float dx = (x - xCenterDst);

            int xSrc = (int)(xCenterSrc + ((dx * cos) + sinY) + 0.5f); 
            int ySrc = (int)(yCenterSrc + (cosY - (dx * sin)) + 0.5f); 
            
            if(xSrc <= srcRect.right && srcRect.left <= xSrc && ySrc <= srcRect.bottom && srcRect.top <= ySrc && ySrc > 0 && xSrc > 0)
            {
                UINT32  maskDst, shiftDst;
                UINT32* pbyteDst = ComputePosition( dst, x, y, maskDst, shiftDst );

                UINT32  mask, shift;
                UINT32* pbyteSrc = ComputePosition( src, xSrc, ySrc, mask, shift );

                *pbyteDst &= ~maskDst;
                *pbyteDst |= ((*pbyteSrc & mask) >> shift) << shiftDst;
            }            
        }
    }
}

void Graphics_Driver::DrawImage( const PAL_GFX_Bitmap& bitmapDst, const GFX_Rect& dst, const PAL_GFX_Bitmap& bitmapSrc, const GFX_Rect& src, UINT16 opacity )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    if (opacity == PAL_GFX_Bitmap::c_OpacityTransparent) return;    // No-op for opacity == 0

    if(dst.Width() == src.Width() && dst.Height() == src.Height())
    {
        int xDst = dst.left;
        int yDst = dst.top;
        int nWidth = dst.Width();
        int nHeight = dst.Height();
        int xSrc = src.left;
        int ySrc = src.top;

        if(ClipToVisible( bitmapDst, xDst, yDst, nWidth, nHeight, &bitmapSrc, xSrc, ySrc ) == false) return;

        int strideSrc = GetWidthInWords( bitmapSrc.width ) * 2;
        int strideDst = GetWidthInWords( bitmapDst.width ) * 2;

        UINT16* ps = (UINT16*)bitmapSrc.data + (strideSrc * ySrc) + xSrc;
        UINT16* pd = (UINT16*)bitmapDst.data + (strideDst * yDst) + xDst;

        static const int Transparent = 0x1;
        static const int Translucent = 0x2;

        static const int NotTranslucent_NotTransparent = 0x0;
        static const int NotTranslucent_Transparent    = Transparent;
        static const int Translucent_NotTransparent    = Translucent;
        static const int Translucent_Transparent       = Translucent | Transparent;

        int type = 0x0;
        UINT32 nativeTransparentColor = 0;
        if (bitmapSrc.transparentColor != PAL_GFX_Bitmap::c_InvalidColor)
        {
            type |= Transparent;
            nativeTransparentColor = ConvertColorToNative( bitmapSrc.transparentColor );
        }
        if (opacity != PAL_GFX_Bitmap::c_OpacityOpaque)
        {
            type |= Translucent;
        }

        if (type == NotTranslucent_NotTransparent)
        {
            while (nHeight-- > 0)
            {
                memcpy( pd, ps, (size_t)(nWidth * 2) );
                pd += strideDst;
                ps += strideSrc;
            }
        }
        else
        {
            while (nHeight-- > 0)
            {
                // We're translucent or transparent or both
                for (int col = 0; col < nWidth; col++, ps++, pd++)
                {
                    UINT16 sourceColor = *ps;

                    switch (type)
                    {
                    case NotTranslucent_Transparent:
                        if (sourceColor != nativeTransparentColor)
                        {
                            *pd = sourceColor;
                        }
                        break;

                    case Translucent_NotTransparent:
                        *pd = NativeColorInterpolate( sourceColor, *pd, opacity );
                        break;

                    case Translucent_Transparent:
                        if (sourceColor != nativeTransparentColor)
                        {
                            *pd = NativeColorInterpolate( sourceColor, *pd, opacity );
                        }
                        break;
                    }
                }
                pd += strideDst - nWidth;
                ps += strideSrc - nWidth;
            }
        }
    }
    else // StretchImage
    {
        int sourceWidth  = src.Width();
        int sourceHeight = src.Height();
        int dstWidth     = dst.Width();
        int dstHeight    = dst.Height();
        int xDst         = dst.left;
        int yDst         = dst.top;

        int xSrc = src.left; 
        int ySrc = src.top; 


        if (sourceWidth <= 0 || sourceHeight <= 0) return;

        int strideSrc = GetWidthInWords( bitmapSrc.width ) * 2;
        int strideDst = GetWidthInWords( bitmapDst.width ) * 2;

        UINT16* ps = (UINT16*)bitmapSrc.data + (strideSrc * ySrc) + xSrc;
        UINT16* rowstart = ps;

        // Clip left
        //
        int croppedWidth = dstWidth;
        int xOffset = xDst;
        int xSourceOffset = 0;
        int xNumeratorStart = 0;
        if (xOffset < bitmapDst.clipping.left)
        {
            int deltaX = bitmapDst.clipping.left - xOffset;
            croppedWidth -= deltaX;
            xNumeratorStart += sourceWidth * deltaX;
            xSourceOffset = xNumeratorStart / dstWidth;
            xNumeratorStart %= dstWidth;
            xOffset = bitmapDst.clipping.left;
        }

        // Clip right
        //
        if (xOffset + croppedWidth > bitmapDst.clipping.right)
        {
            croppedWidth = bitmapDst.clipping.right - xOffset;
        }

        // Clip top
        //
        int croppedHeight = dstHeight;
        int yOffset = yDst;
        int ySourceOffset = 0;
        int yNumerator = 0;
        if (yOffset < bitmapDst.clipping.top)
        {
            int deltaY = bitmapDst.clipping.top - yOffset;
            croppedHeight -= deltaY;
            yNumerator += sourceHeight * deltaY;
            ySourceOffset = yNumerator / dstHeight;
            yNumerator %= dstHeight;
            yOffset = bitmapDst.clipping.top;
            rowstart += strideSrc * ySourceOffset;
        }

        // Clip bottom
        //
        if (yOffset + croppedHeight > bitmapDst.clipping.bottom)
        {
            croppedHeight = bitmapDst.clipping.bottom - yOffset;
        }

        if (croppedWidth <= 0 || croppedHeight <= 0) return;

        UINT16* pd = (UINT16*)bitmapDst.data + (strideDst * yOffset) + xOffset;


        for(int y=0; y < croppedHeight; y++, pd += strideDst)
        {
            int xNumerator = xNumeratorStart;

            ps = rowstart + xSourceOffset;

            UINT16 color = *ps;
            UINT32 nativeTransparentColor = ConvertColorToNative( bitmapSrc.transparentColor );
            bool noTransparent = bitmapSrc.transparentColor == PAL_GFX_Bitmap::c_InvalidColor;
            bool transparent = (noTransparent == false) && (color == nativeTransparentColor);

            for(int x=0; x < croppedWidth; x++, pd++)
            {
                if (!transparent)
                {
                    *pd = (opacity == PAL_GFX_Bitmap::c_OpacityOpaque)
                        ? color
                        : NativeColorInterpolate( color, *pd, opacity );
                }

                xNumerator += sourceWidth;
                while (xNumerator >= dstWidth)
                {
                    xNumerator -= dstWidth;
                    color = *(++ps);
                    transparent = (noTransparent == false) && (color == nativeTransparentColor);
                }
            }
            pd -= croppedWidth;

            yNumerator += sourceHeight;
            while (yNumerator >= dstHeight)
            {
                yNumerator -= dstHeight;
                rowstart += strideSrc;
            }
        }
    }
}

//--//

UINT32* Graphics_Driver::ComputePosition( const PAL_GFX_Bitmap& bitmap, int x, int y, UINT32& mask , UINT32& shift )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

#if !defined(BIG_ENDIAN)
    shift = (x % 2) * 16;
    mask = 0x0000FFFF << shift;
#else
    shift = ((x+1) % 2) * 16;
    mask = 0x0000FFFF << shift;
   
#endif

    return &(bitmap.data[y * GetWidthInWords( bitmap.width ) + (x / 2)]);
}

BOOL Graphics_Driver::IsPixelVisible( const GFX_Rect& clipping, int x, int y )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    return (x >= clipping.left && x < clipping.right ) &&
        (y >= clipping.top  && y < clipping.bottom)  ;
}

BOOL Graphics_Driver::ClipToVisible( const PAL_GFX_Bitmap& target, int& x, int& y, int& width, int& height, const PAL_GFX_Bitmap* pSrc, int& xSrc, int& ySrc )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    if(pSrc != NULL)
    {
        if(xSrc < 0 || ySrc < 0) return false;

        int widthSrc  = pSrc->width  - xSrc;
        int heightSrc = pSrc->height - ySrc;

        if(width  == -1) width  = pSrc->width;
        if(height == -1) height = pSrc->height;

        if(width  > widthSrc ) width  = widthSrc;
        if(height > heightSrc) height = heightSrc;
    }

    int xEnd = x + width;
    int yEnd = y + height;

    //
    // Totally outside the clipping region?
    //
    if(xEnd <  target.clipping.left   ||
        x    >= target.clipping.right  ||
        yEnd <  target.clipping.top    ||
        y    >= target.clipping.bottom  )
    {
        width = 0;
        height = 0;
        return false;
    }

    //--//

    if(x < target.clipping.left)
    {
        int nDelta = target.clipping.left - x;

        x     += nDelta;
        xSrc  += nDelta;
        width -= nDelta;
    }

    if(xEnd > target.clipping.right)
    {
        width = target.clipping.right - x;
    }

    //--//

    if(y < target.clipping.top)
    {
        int nDelta = target.clipping.top - y;

        y      += nDelta;
        ySrc   += nDelta;
        height -= nDelta;
    }

    if(yEnd > target.clipping.bottom)
    {
        height = target.clipping.bottom - y;
    }

    if(width <= 0 || height <= 0) return false;

    return true;
}

UINT32 Graphics_Driver::NativeColorInterpolate( UINT32 colorTo, UINT32 colorFrom, UINT16 scalar )
{
    // truncate to maximum acceptable value to implement a tolerant behaviour
    if(scalar > PAL_GFX_Bitmap::c_OpacityOpaque) scalar = PAL_GFX_Bitmap::c_OpacityOpaque;

    if (scalar == PAL_GFX_Bitmap::c_OpacityOpaque     ) return colorTo;
    if (scalar == PAL_GFX_Bitmap::c_OpacityTransparent) return colorFrom;

    UINT32 rT = NativeColorRValue( colorTo ) * scalar;
    UINT32 gT = NativeColorGValue( colorTo ) * scalar;
    UINT32 bT = NativeColorBValue( colorTo ) * scalar;

    UINT16 invertScalar = PAL_GFX_Bitmap::c_OpacityOpaque - scalar;

    UINT32 rF = NativeColorRValue( colorFrom ) * invertScalar;
    UINT32 gF = NativeColorGValue( colorFrom ) * invertScalar;
    UINT32 bF = NativeColorBValue( colorFrom ) * invertScalar;

    ASSERT(((rT + rF) >> 8) <= 0x1F);
    ASSERT(((gT + gF) >> 8) <= 0x3F);
    ASSERT(((bT + bF) >> 8) <= 0x1F);

    return NativeColorFromRGB( (rT + rF) >> 8, (gT + gF) >> 8, (bT + bF) >> 8 );
}

void Graphics_Driver::DrawBresLineNative( const PAL_GFX_Bitmap& bitmap, int x0, int y0, int x1, int y1, GFX_Pen& pen )
{
    int         dPr;
    int         dPru;
    int         P;
    int         dx;
    int         dy;
    int         incrementX;
    int         incrementY;
    int         thickness = pen.thickness;
    UINT32      color = pen.color;

    //RWOLFF - this can be very slow if outside clipping bounds
    bool        fValidX = x0 >= bitmap.clipping.left && x0 < bitmap.clipping.right;
    bool        fValidY = y0 >= bitmap.clipping.top  && y0 < bitmap.clipping.bottom;
    UINT32  mask, shift;
    UINT32* data;

    dx = abs( x1 - x0 );    // store the change in X and Y of the line endpoints
    dy = abs( y1 - y0 );

    incrementX = (x0 > x1) ? -1 : 1;
    incrementY = (y0 > y1) ? -1 : 1;


    /// Pixels are rectangular rather than circular lets adjust approximate
    /// width for lines that are not horizontal or vertical.
    if (thickness > 2)
    {
        int m1 = dx;
        int m2 = dy;
        int r = 0;
        int t = thickness;
        if (dy > dx)
        {
            m1 = dy;
            m2 = dx;
        }

        if (m2 > 0)
        {
            r = m1 / m2;

            if (r < 2)
            {
                thickness = (t * 2) / 3;
            }
            else if (r < 4)
            {
                thickness = (t * 4) / 5;
            }
        }
    }        

    if(dx >= dy)
    {
        dPr  =        dy << 1;  // amount to increment decision if right is chosen (always)
        dPru = dPr - (dx << 1); // amount to increment decision if up is chosen
        P    = dPr -  dx;       // decision variable start value

        for(; dx>=0; dx--)
        {
            if(fValidX && fValidY)
            {
                if (thickness > 1)
                {
                    SetThickPixel(bitmap, x0, y0, pen);
                }
                else
                {
                    data = ComputePosition( bitmap, x0, y0, mask, shift );

                    *data &= ~mask;
                    *data |= color << shift;
                }
            }

            if(P > 0)
            {
                x0 += incrementX;
                y0 += incrementY;
                P  += dPru;

                fValidY = y0 >= bitmap.clipping.top && y0 < bitmap.clipping.bottom;
            }
            else
            {
                x0 += incrementX;
                P  += dPr;
            }

            fValidX = x0 >= bitmap.clipping.left && x0 < bitmap.clipping.right;
        }
    }
    else
    {
        dPr  =        dx << 1;  // amount to increment decision if right is chosen (always)
        dPru = dPr - (dy << 1); // amount to increment decision if up is chosen
        P    = dPr -  dy;       // decision variable start value

        for(; dy>=0; dy--)
        {
            if(fValidX && fValidY)
            {
                if (thickness > 1)
                {
                    SetThickPixel(bitmap, x0, y0, pen);
                }
                else
                {
                    data = ComputePosition( bitmap, x0, y0, mask, shift );

                    *data &= ~mask;
                    *data |= color << shift;
                }
            }

            if(P > 0)
            {
                x0 += incrementX;
                y0 += incrementY;
                P  += dPru;

                fValidX = x0 >= bitmap.clipping.left && x0 < bitmap.clipping.right;
            }
            else
            {
                y0 += incrementY;
                P  += dPr;
            }

            fValidY = y0 >= bitmap.clipping.top  && y0 < bitmap.clipping.bottom;
        }
    }
}

void Graphics_Driver::EllipseAlgorithm( const PAL_GFX_Bitmap& bitmap, int radiusX, int radiusY, void* params, EllipseCallback ellipseCallback )
{
    NATIVE_PROFILE_PAL_GRAPHICS();
    // TODO: check for some upper bound of the radius so that the caclulation won't overflow the int.

    if (radiusX <= 0 || radiusY <= 0)
        return;

    if (radiusX == radiusY)
    {
        int  x = 0;
        int  y = radiusX;
        int  p = 3 - (radiusX << 1);
        int  yNext = y;

        while(x <= y)
        {
            if(p < 0)
            {
                p += (x << 2) + 6;
            }
            else
            {
                p += ((x - y) << 2) + 10;
                yNext--;
            }

            (*ellipseCallback)( bitmap, x, y, params );
            if (x < y) (*ellipseCallback)( bitmap, y, x, params );

            y = yNext;
            x++;
        }
    }
    else
    {
#define _a radiusX
#define _b radiusY

        int x = 0;
        int y = radiusY;

        const int      aSqr = _a * _a;
        const int      bSqr = _b * _b;
        const int  fourASqr = 4 * aSqr;
        const int  fourBSqr = 4 * bSqr;
        const int eightASqr = 8 * aSqr;
        const int eightBSqr = 8 * bSqr;

        int D = fourBSqr - fourASqr * _b + aSqr;
        int G = fourASqr * (2 * radiusY - 1);

        int deltaE  = fourBSqr + eightBSqr;
        int deltaSE = fourBSqr + eightBSqr + eightASqr * (1 - radiusY);

        (*ellipseCallback)( bitmap, x, y, params );

        while (G > 0)
        {
            if (D < 0) // Select E
            {
                D += deltaE;

                deltaE  += eightBSqr;
                deltaSE += eightBSqr;

                G -= eightBSqr;
            }
            else // Select SE
            {
                D += deltaSE;

                deltaE  += eightBSqr;
                deltaSE += eightBSqr + eightASqr;

                G -= (eightASqr + eightBSqr);

                y--;
            }
            x++;

            (*ellipseCallback)( bitmap, x, y, params );
        }

        D += aSqr * (3 - 4 * y) - bSqr * (3 + 4 * x);

        int deltaS;

        deltaS  = fourASqr * (-2 * y + 3);
        deltaSE = eightBSqr * (x + 1) + deltaS;

        while (y > 0)
        {
            if (D < 0) // Select SE
            {
                D += deltaSE;

                deltaS  += eightASqr;
                deltaSE += eightASqr + eightBSqr;

                x++;
            }
            else // Select S
            {
                D += deltaS;

                deltaS  += eightASqr;
                deltaSE += eightASqr;
            }
            y--;

            (*ellipseCallback)( bitmap, x, y, params );
        }

#undef _a
#undef _b
    }
}

void Graphics_Driver::SetPixelsHelper( const PAL_GFX_Bitmap& bitmap, const GFX_Rect& rect, UINT32 config, GFX_SetPixelsCallback callback, void* param )
{
    NATIVE_PROFILE_PAL_GRAPHICS();

    int x = rect.left;
    int y = rect.top;
    int width = rect.Width();
    int height = rect.Height();
    int xSrc = 0;
    int ySrc = 0;
    bool checkPixelVisible = (!(config & PAL_GFX_Bitmap::c_SetPixelsConfig_Clip)) && (!(config & PAL_GFX_Bitmap::c_SetPixelsConfig_NoClipChecks));

    if (config & PAL_GFX_Bitmap::c_SetPixelsConfig_Clip)
    {
        if (ClipToVisible( bitmap, x, y, width, height, NULL, xSrc, ySrc ) == FALSE) return;
    }

    int stride = GetWidthInWords( bitmap.width ) * 2;
    UINT16* curRow = ((UINT16*)bitmap.data) + y * stride + x;
    UINT16* curPixel;

    int xStart = x;
    int xEnd = x + width;
    UINT32 color;
    UINT32 flag = PAL_GFX_Bitmap::c_SetPixels_First;
    UINT16 opacity;
    int yIncrement = 1;

    if (config & PAL_GFX_Bitmap::c_SetPixelsConfig_ReverseY)
    {
        yIncrement = -1;
        y = y + height - 1;
        curRow += (height - 1) * stride; // go to the last row
        stride *= -1;
    }

    curPixel = curRow;

    for (; height > 0; height--)
    {
        flag |= PAL_GFX_Bitmap::c_SetPixels_NewRow;

        for (x = xStart; x < xEnd; x++)
        {
            opacity = PAL_GFX_Bitmap::c_OpacityOpaque;

            if (checkPixelVisible && !IsPixelVisible( bitmap.clipping, x, y ))
            {
                flag |= PAL_GFX_Bitmap::c_SetPixels_PixelHidden;
                (*callback)( x, y, flag, opacity, param );
            }
            else
            {
                color = (*callback)( x, y, flag, opacity, param );

                if (opacity != PAL_GFX_Bitmap::c_OpacityTransparent)
                {
                    *curPixel = NativeColorInterpolate( ConvertColorToNative( color ), *curPixel, opacity );
                }
            }

            flag = PAL_GFX_Bitmap::c_SetPixels_None;
            curPixel++;
        }

        curRow += stride;
        curPixel = curRow;
        y += yIncrement;
    }
}
