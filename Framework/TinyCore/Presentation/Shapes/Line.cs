////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Presentation.Shapes
{
    public enum Direction
    {
        TopToBottom,
        BottomToTop
    }

    public class Line : Shape
    {
        public Line()
            : this(0, 0)
        {
        }

        public Line(int dx, int dy)
        {
            if( dx < 0 || dy < 0)
            {
                throw new ArgumentException();
            }
            
            this.Width = dx + 1;
            this.Height = dy + 1;
        }

        public Direction Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value;
                Invalidate();
            }
        }

        public override void OnRender(Media.DrawingContext dc)
        {
            int width = this._renderWidth;
            int height = this._renderHeight;

            if (_direction == Direction.TopToBottom)
            {
                dc.DrawLine(Stroke, 0, 0, width - 1, height - 1);
            }
            else
            {
                dc.DrawLine(Stroke, 0, height - 1, width - 1, 0);
            }
        }

        private Direction _direction;
    }
}


