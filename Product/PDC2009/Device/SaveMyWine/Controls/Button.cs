using System;
using System.Runtime.CompilerServices;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Samples.SaveMyWine
{
    // UIButton Class - soft buttons
    public class UIButton : UIElement
    {

        // Autosizing constructor
        public UIButton(string caption, Font font)
        {
            _caption = caption;
            _font = font;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Bottom;

            int textWidth;
            int textHeight;
            _font.ComputeExtent(_caption, out textWidth, out textHeight);

            _width = textWidth + _textMarginX * 2;
            _height = textHeight + _textMarginY * 2;
        }

        //image button constructor
        public UIButton(Bitmap imageoff, Bitmap imageon, int width, int height)
        {
            _imageoff = imageoff;
            _imageon = imageon;
            _width = width;
            _height = height;
        }

        // Manual sizing constructor
        public UIButton(string caption, Font font, int width, int height)
        {
            _width = width;
            _height = height;
            _caption = caption;
            _font = font;
        }

        //Change the caption
        public void ButtonCaption(string caption)
        {
            _caption = caption;
            Invalidate();
        }


        // Override OnRender to do our own drawing
        public override void OnRender(DrawingContext dc)
        {
            int x;
            int y;

            SolidColorBrush brush;
            Pen pen;
            Color color;
            Pen shade1;
            Pen shade2;


            // Check the pressed state and draw accordingly
            if (_pressed)
            {
                brush = _pressedBackgroundBrush;
                pen = _pressedBorderPen;
                color = _pressedForeColor;
                shade1 = _darkShade;
                shade2 = _lightShade;
            }
            else
            {
                brush = _normalBackgroundBrush;
                pen = _borderPen;
                color = _foreColor;
                shade1 = _lightShade;
                shade2 = _darkShade;
            }

            GetLayoutOffset(out x, out y);

            if (_caption != "")
            {
                // Draw the base rectangle of the button
                dc.DrawRectangle(brush, pen, 1, 1, _width - 1, _height - 1);

                // Draw the caption
                string caption = _caption;
                dc.DrawText(ref caption, _font, color, 0, _textMarginY, _width, _height, _alignment, _trimming);
            }
            else
            {
                if (_pressed)
                    dc.DrawImage(_imageon,1,1);
                else
                    dc.DrawImage(_imageoff, 1, 1);
            }

            // Shade the outline of the rectangle for classic button look
            dc.DrawLine(shade1, 1, 1, _width - 1, 1);
            dc.DrawLine(shade1, 1, 1, 1, _height - 1);
            dc.DrawLine(shade2, _width - 1, 1, _width - 1, _height - 1);
            dc.DrawLine(shade2, 1, _height - 1, _width - 1, _height - 1);
        }

        public event EventHandler Click;

        // Handle the stylus down event
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        protected override void OnTouchDown(TouchEventArgs e)
        {
            if (IsEnabled)
            {
                if (!_pressed)
                {
                    // Flag for drawing state
                    _pressed = true;

                    // Trigger redraw
                    Invalidate();
                }
            }
        }


        // Handle the stylus up event
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        protected override void OnTouchUp(TouchEventArgs e)
        {
            if (IsEnabled)
            {
                if (_pressed)
                {
                    // Flag for drawing state
                    _pressed = false;

                    // Trigger redraw
                    Invalidate();

                    // Fire a click event
                    EventArgs args = new EventArgs();
                    OnClick(args);
                }
            }
        }

        // Our click event
        protected virtual void OnClick(EventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            desiredWidth = (availableWidth > _width) ? _width : availableWidth;
            desiredHeight = (availableHeight > _height) ? _height : availableHeight;
        }

        private SolidColorBrush _normalBackgroundBrush = new SolidColorBrush(ColorUtility.ColorFromRGB(140, 0, 100));
        private SolidColorBrush _pressedBackgroundBrush = new SolidColorBrush(ColorUtility.ColorFromRGB(100, 0, 60));

        private Pen _borderPen = new Pen(ColorUtility.ColorFromRGB(128, 128, 128));

        private Pen _pressedBorderPen = new Pen(ColorUtility.ColorFromRGB(128, 128, 128));

        private Pen _lightShade = new Pen(ColorUtility.ColorFromRGB(216, 216, 216));
        private Pen _darkShade = new Pen(ColorUtility.ColorFromRGB(64, 64, 64));

        int _width;
        int _height;
        string _caption = "";
        Font _font = null;
        Color _foreColor = ColorUtility.ColorFromRGB(0, 0, 0);
        Color _pressedForeColor = ColorUtility.ColorFromRGB(255, 255, 255);
        private TextTrimming _trimming = TextTrimming.WordEllipsis;
        private TextAlignment _alignment = TextAlignment.Center;
        protected int _textMarginX = 16;
        protected int _textMarginY = 8;
        protected bool _pressed = false;
        private Bitmap _imageon = null;
        private Bitmap _imageoff = null;
    }
 

}
