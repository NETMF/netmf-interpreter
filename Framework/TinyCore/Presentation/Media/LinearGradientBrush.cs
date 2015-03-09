////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Presentation.Media
{
    public sealed class LinearGradientBrush : Brush
    {
        public Color StartColor;
        public Color EndColor;

        public BrushMappingMode MappingMode = BrushMappingMode.RelativeToBoundingBox;

        public int StartX, StartY;
        public int EndX, EndY;

        public const int RelativeBoundingBoxSize = 1000;

        public LinearGradientBrush(Color startColor, Color endColor)
            : this(startColor, endColor, 0, 0, RelativeBoundingBoxSize, RelativeBoundingBoxSize)
        { }

        public LinearGradientBrush(Color startColor, Color endColor, int startX, int startY, int endX, int endY)
        {
            StartColor = startColor;
            EndColor = endColor;
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
        }

        protected internal override void RenderRectangle(Bitmap bmp, Pen pen, int x, int y, int width, int height)
        {
            Color outlineColor = (pen != null) ? pen.Color : (Color)0x0;
            ushort outlineThickness = (pen != null) ? pen.Thickness : (ushort)0;

            int x1, y1;
            int x2, y2;

            switch (MappingMode)
            {
                case BrushMappingMode.RelativeToBoundingBox:
                    x1 = x + (int)((long)(width - 1) * StartX / RelativeBoundingBoxSize);
                    y1 = y + (int)((long)(height - 1) * StartY / RelativeBoundingBoxSize);
                    x2 = x + (int)((long)(width - 1) * EndX / RelativeBoundingBoxSize);
                    y2 = y + (int)((long)(height - 1) * EndY / RelativeBoundingBoxSize);
                    break;
                default: //case BrushMappingMode.Absolute:
                    x1 = StartX;
                    y1 = StartY;
                    x2 = EndX;
                    y2 = EndY;
                    break;
            }

            bmp.DrawRectangle(outlineColor, outlineThickness, x, y, width, height, 0, 0,
                                          StartColor, x1, y1, EndColor, x2, y2, Opacity);
        }
    }
}


