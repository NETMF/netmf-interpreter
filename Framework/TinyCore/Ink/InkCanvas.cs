////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Controls;

namespace Microsoft.SPOT.Ink
{
    public class DrawingAttributes
    {
        public Color Color = Colors.Black;
    }

    /// <summary>
    /// Note: InkCanvas control is not movable at runtime. This requires complex logic, with
    /// no customer scenario at this moment.
    /// </summary>

    public class InkCanvas : UIElement
    {
        public InkCanvas(int left, int top, int width, int height)
            : this(left, top, width, height, 1)
        {
        }

        public InkCanvas(int left, int top, int width, int height, int borderWidth)
        {
            Init(left, top, width, height, borderWidth);
            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
        }

        ~InkCanvas()
        {
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            int x, y, w, h;

            GetLayoutOffset(out x, out y);
            GetRenderSize(out w, out h);

            x += _left;
            y += _top;

            TouchCapture.Capture(this);

            Microsoft.SPOT.Touch.Ink.SetInkRegion(0, x, y, x + w, y + h, _borderWidth, (int)_defaultDrawingAttributes.Color, 1, _bitmap);
        }

        protected override void OnTouchUp(TouchEventArgs e)
        {
            Microsoft.SPOT.Touch.Ink.ResetInkRegion();
            Invalidate();
        }

        public override void OnRender(Microsoft.SPOT.Presentation.Media.DrawingContext dc)
        {
            if (_bitmap != null)
            {
                dc.DrawImage(_bitmap, 0, 0);
            }
        }

        public void Clear()
        {
            _bitmap.DrawRectangle(Colors.Black, _borderWidth, 0, 0, _width, _height, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityOpaque);
            Invalidate();
        }

        protected virtual void Init(int left, int top, int width, int height, int borderWidth)
        {
            _width = width;
            _height = height;
            _left = left;
            _top = top;
            _borderWidth = borderWidth;

            int x1 = _left;
            int y1 = _top;

            _bitmap = new Bitmap(_width, _height);
            _bitmap.DrawRectangle(Colors.Black, _borderWidth, 0, 0, _width, _height, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityOpaque);

            if ((x1 < 0) || ((x1 + _width) > SystemMetrics.ScreenWidth) ||
                (y1 < 0) || ((y1 + _height) > SystemMetrics.ScreenHeight))
            {
                throw new ArgumentException("screenlimit");
            }
        }

        public DrawingAttributes DefaultDrawingAttributes
        {
            get
            {
                return _defaultDrawingAttributes;
            }

            set
            {
                _defaultDrawingAttributes = value;
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            desiredWidth = (availableWidth > _width) ? _width : availableWidth;
            desiredHeight = (availableHeight > _height) ? _height : availableHeight;
        }

        //--//

        protected DrawingAttributes _defaultDrawingAttributes = new DrawingAttributes();
        protected Bitmap _bitmap = null;

        //--//

        private int _borderWidth;

        private int _width;
        private int _height;
        private int _top;
        private int _left;
    }
}


