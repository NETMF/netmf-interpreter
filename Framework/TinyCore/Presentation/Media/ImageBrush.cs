////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Presentation.Media
{
    public sealed class ImageBrush : Brush
    {
        public Bitmap BitmapSource;
        public Stretch Stretch = Stretch.Fill;

        public ImageBrush(Bitmap bmp)
        {
            BitmapSource = bmp;
        }

        protected internal override void RenderRectangle(Bitmap bmp, Pen pen, int x, int y, int width, int height)
        {
            if (Stretch == Stretch.None)
            {
                bmp.DrawImage(x, y, BitmapSource, 0, 0, BitmapSource.Width, BitmapSource.Height, Opacity);
            }
            else if (width == BitmapSource.Width && height == BitmapSource.Height)
            {
                bmp.DrawImage(x, y, BitmapSource, 0, 0, width, height, Opacity);
            }
            else
            {
                bmp.StretchImage(x, y, BitmapSource, width, height, Opacity);
            }

            if (pen != null && pen.Thickness > 0)
            {
                bmp.DrawRectangle(pen.Color, pen.Thickness, x, y, width, height, 0, 0,
                                      (Color)0, 0, 0, (Color)0, 0, 0, 0);
            }
        }
    }
}


