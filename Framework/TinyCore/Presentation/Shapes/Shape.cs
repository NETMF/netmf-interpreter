////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Shapes
{
    public abstract class Shape : UIElement
    {
        public Brush Fill
        {
            get
            {
                if (_fill == null)
                {
                    _fill = new SolidColorBrush(Colors.Black);
                    _fill.Opacity = Bitmap.OpacityTransparent;
                }

                return _fill;
            }

            set
            {
                _fill = value;
                Invalidate();
            }
        }

        public Pen Stroke
        {
            get
            {
                if (_stroke == null)
                {
                    _stroke = new Pen(Colors.White, 0);
                }

                return _stroke;
            }

            set
            {
                _stroke = value;
                Invalidate();
            }
        }

        private Brush _fill;
        private Pen _stroke;
    }
}


