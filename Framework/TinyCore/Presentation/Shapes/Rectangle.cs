////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Presentation.Shapes
{
    public class Rectangle : Shape
    {
        public Rectangle()
        {
            Width = 0;
            Height = 0;
        }

        public Rectangle(int width, int height)
        {
            if( width < 0 || height < 0)                
            {
                throw new ArgumentException();
            }
            
            Width = width;
            Height = height;
        }

        public override void OnRender(Media.DrawingContext dc)
        {
            int offset = Stroke != null ? Stroke.Thickness / 2 : 0;
            
            dc.DrawRectangle(Fill, Stroke, offset, offset, _renderWidth - 2 * offset, _renderHeight - 2 * offset);
        }
    }
}
