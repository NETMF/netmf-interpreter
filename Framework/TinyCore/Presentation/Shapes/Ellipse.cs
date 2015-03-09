////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Shapes
{
    public class Ellipse : Shape
    {
        public Ellipse(int xRadius, int yRadius)
        {
            if( xRadius < 0 || yRadius < 0)                
            {
                throw new ArgumentException();
            }
            
            this.Width = xRadius * 2 + 1;
            this.Height = yRadius * 2 + 1;
        }

        public override void OnRender(Media.DrawingContext dc)
        {
            /// Make room for cases when strokes are thick.
            int x = _renderWidth / 2 + Stroke.Thickness - 1;
            int y = _renderHeight / 2 + Stroke.Thickness - 1;
            int w = _renderWidth / 2 - (Stroke.Thickness - 1) * 2;
            int h = _renderHeight / 2 - (Stroke.Thickness - 1) * 2;

            dc.DrawEllipse(Fill, Stroke, x, y, w, h);
        }
    }
}
