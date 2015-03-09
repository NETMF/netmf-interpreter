////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class Control : UIElement
    {
        public Brush Background
        {
            get
            {
                VerifyAccess();

                return _background;
            }

            set
            {
                VerifyAccess();

                _background = value;
                Invalidate();
            }
        }

        public Font Font
        {
            get
            {
                return _font;
            }

            set
            {
                VerifyAccess();

                _font = value;
                InvalidateMeasure();
            }
        }

        public Brush Foreground
        {
            get
            {
                VerifyAccess();

                return _foreground;
            }

            set
            {
                VerifyAccess();

                _foreground = value;
                Invalidate();
            }
        }

        public override void OnRender(DrawingContext dc)
        {
            if (_background != null)
            {
                dc.DrawRectangle(_background, null, 0, 0, _renderWidth, _renderHeight);
            }
        }

        protected internal Brush _background = null;
        protected internal Brush _foreground = new SolidColorBrush(Colors.Black);
        protected internal Font _font;
    }
}


