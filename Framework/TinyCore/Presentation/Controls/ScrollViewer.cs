using System;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class ScrollViewer : ContentControl
    {
        public ScrollViewer()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        public event ScrollChangedEventHandler ScrollChanged
        {
            add
            {
                VerifyAccess();

                _scrollChanged += value;
            }

            remove
            {
                VerifyAccess();

                _scrollChanged -= value;
            }
        }

        public int HorizontalOffset
        {
            get
            {
                return _horizontalOffset;
            }

            set
            {
                VerifyAccess();

                if (value < 0)
                {
                    value = 0;
                }
                else if ((_flags & Flags.NeverArranged) == 0 && value > _scrollableWidth)
                {
                    value = _scrollableWidth;
                }

                if (_horizontalOffset != value)
                {
                    _horizontalOffset = value;
                    InvalidateArrange();
                }
            }
        }

        public int VerticalOffset
        {
            get
            {
                return _verticalOffset;
            }

            set
            {
                VerifyAccess();

                if (value < 0)
                {
                    value = 0;
                }
                else if ((_flags & Flags.NeverArranged) == 0 && value > _scrollableHeight)
                {
                    value = _scrollableHeight;
                }

                if (_verticalOffset != value)
                {
                    _verticalOffset = value;
                    InvalidateArrange();
                }
            }
        }

        public int ExtentHeight
        {
            get
            {
                return _extentHeight;
            }
        }

        public int ExtentWidth
        {
            get
            {
                return _extentWidth;
            }
        }

        public int LineWidth
        {
            get
            {
                return _lineWidth;
            }

            set
            {
                VerifyAccess();

                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException("LineWidth");
                }

                _lineWidth = value;
            }
        }

        public int LineHeight
        {
            get
            {
                return _lineHeight;
            }

            set
            {
                VerifyAccess();

                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException("LineHeight");
                }

                _lineHeight = value;
            }
        }

        public ScrollingStyle ScrollingStyle
        {
            get
            {
                return _scrollingStyle;
            }

            set
            {
                VerifyAccess();

                if (value < ScrollingStyle.First || value > ScrollingStyle.Last)
                {
                    throw new ArgumentOutOfRangeException("ScrollingStyle", "Invalid Enum");
                }

                _scrollingStyle = value;
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            UIElement child = this.Child;
            if (child != null && child.Visibility != Visibility.Collapsed)
            {
                child.Measure((HorizontalAlignment == HorizontalAlignment.Stretch) ? Media.Constants.MaxExtent : availableWidth, (VerticalAlignment == VerticalAlignment.Stretch) ? Media.Constants.MaxExtent : availableHeight);
                child.GetDesiredSize(out desiredWidth, out desiredHeight);
                _extentHeight = child._unclippedHeight;
                _extentWidth = child._unclippedWidth;
            }
            else
            {
                desiredWidth = desiredHeight = 0;
                _extentHeight = _extentWidth = 0;
            }
        }

        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight)
        {
            UIElement child = this.Child;
            if (child != null)
            {
                // Clip scroll-offset if necessary
                //
                _scrollableWidth = System.Math.Max(0, ExtentWidth - arrangeWidth);
                _scrollableHeight = System.Math.Max(0, ExtentHeight - arrangeHeight);
                _horizontalOffset = System.Math.Min(_horizontalOffset, _scrollableWidth);
                _verticalOffset = System.Math.Min(_verticalOffset, _scrollableHeight);

                Debug.Assert(_horizontalOffset >= 0);
                Debug.Assert(_verticalOffset >= 0);

                child.Arrange(-_horizontalOffset,
                               -_verticalOffset,
                               System.Math.Max(arrangeWidth, ExtentWidth),
                               System.Math.Max(arrangeHeight, ExtentHeight));
            }
            else
            {
                _horizontalOffset = _verticalOffset = 0;
            }

            InvalidateScrollInfo();
        }

        public void LineDown()
        {
            VerticalOffset += _lineHeight;
        }

        public void LineLeft()
        {
            HorizontalOffset -= _lineWidth;
        }

        public void LineRight()
        {
            HorizontalOffset += _lineWidth;
        }

        public void LineUp()
        {
            VerticalOffset -= _lineHeight;
        }

        public void PageDown()
        {
            VerticalOffset += ActualHeight;
        }

        public void PageLeft()
        {
            HorizontalOffset -= ActualWidth;
        }

        public void PageRight()
        {
            HorizontalOffset += ActualWidth;
        }

        public void PageUp()
        {
            VerticalOffset -= ActualHeight;
        }

        private void InvalidateScrollInfo()
        {
            if (_scrollChanged != null)
            {
                int deltaX = _horizontalOffset - _previousHorizontalOffset;
                int deltaY = _verticalOffset - _previousVerticalOffset;
                _scrollChanged(this, new ScrollChangedEventArgs(_horizontalOffset, _verticalOffset, deltaX, deltaY));
            }

            _previousHorizontalOffset = _horizontalOffset;
            _previousVerticalOffset = _verticalOffset;
        }

        protected override void OnButtonDown(Microsoft.SPOT.Input.ButtonEventArgs e)
        {
            switch (e.Button)
            {
                case Microsoft.SPOT.Hardware.Button.VK_UP:
                    if (_scrollingStyle == ScrollingStyle.LineByLine) LineUp(); else PageUp();
                    break;
                case Microsoft.SPOT.Hardware.Button.VK_DOWN:
                    if (_scrollingStyle == ScrollingStyle.LineByLine) LineDown(); else PageDown();
                    break;
                case Microsoft.SPOT.Hardware.Button.VK_LEFT:
                    if (_scrollingStyle == ScrollingStyle.LineByLine) LineLeft(); else PageLeft();
                    break;
                case Microsoft.SPOT.Hardware.Button.VK_RIGHT:
                    if (_scrollingStyle == ScrollingStyle.LineByLine) LineRight(); else PageRight();
                    break;
                default:
                    return;
            }

            if (_previousHorizontalOffset != _horizontalOffset || _previousVerticalOffset != _verticalOffset)
            {
                e.Handled = true;
            }
        }

        private int _previousHorizontalOffset;
        private int _previousVerticalOffset;
        private int _horizontalOffset;
        private int _verticalOffset;
        private int _extentWidth;
        private int _extentHeight;
        private int _scrollableWidth;
        private int _scrollableHeight;

        private int _lineHeight = 1;
        private int _lineWidth = 1;

        private ScrollingStyle _scrollingStyle = ScrollingStyle.LineByLine;

        private ScrollChangedEventHandler _scrollChanged;
    }
}


