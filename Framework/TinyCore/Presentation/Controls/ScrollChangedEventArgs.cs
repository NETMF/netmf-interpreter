using System;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class ScrollChangedEventArgs : EventArgs
    {
        public readonly int HorizontalChange;
        public readonly int HorizontalOffset;

        public readonly int VerticalChange;
        public readonly int VerticalOffset;

        public ScrollChangedEventArgs(int offsetX, int offsetY, int offsetChangeX, int offsetChangeY)
        {
            HorizontalOffset = offsetX;
            HorizontalChange = offsetChangeX;

            VerticalOffset = offsetY;
            VerticalChange = offsetChangeY;
        }
    }
}


