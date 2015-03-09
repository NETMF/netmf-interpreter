////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class StackPanel : Panel
    {
        public StackPanel()
            : this(Orientation.Vertical)
        {
        }

        public StackPanel(Orientation orientation)
        {
            this.Orientation = orientation;
        }

        public Orientation Orientation
        {
            get
            {
                return _orientation;
            }

            set
            {
                VerifyAccess();

                _orientation = value;
                InvalidateMeasure();
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            desiredWidth = 0;
            desiredHeight = 0;

            bool fHorizontal = (Orientation == Orientation.Horizontal);

            //  Iterate through children.
            //
            int nChildren = Children.Count;
            for (int i = 0; i < nChildren; i++)
            {
                UIElement child = Children[i];

                if (child.Visibility != Visibility.Collapsed)
                {
                    // Measure the child.
                    // - according to Avalon specs, the stack panel should not constrain
                    //   a child's measure in the direction of the stack
                    //
                    if (fHorizontal)
                    {
                        child.Measure(Media.Constants.MaxExtent, availableHeight);
                    }
                    else
                    {
                        child.Measure(availableWidth, Media.Constants.MaxExtent);
                    }

                    // Accumulate child size.
                    //
                    int childDesiredWidth, childDesiredHeight;
                    child.GetDesiredSize(out childDesiredWidth, out childDesiredHeight);

                    if (fHorizontal)
                    {
                        desiredWidth += childDesiredWidth;
                        desiredHeight = System.Math.Max(desiredHeight, childDesiredHeight);
                    }
                    else
                    {
                        desiredWidth = System.Math.Max(desiredWidth, childDesiredWidth);
                        desiredHeight += childDesiredHeight;
                    }
                }
            }
        }

        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight)
        {
            bool fHorizontal = (Orientation == Orientation.Horizontal);
            int previousChildSize = 0;
            int childPosition = 0;

            // Arrange and Position Children.
            //
            int nChildren = Children.Count;
            for (int i = 0; i < nChildren; ++i)
            {
                UIElement child = Children[i];
                if (child.Visibility != Visibility.Collapsed)
                {
                    childPosition += previousChildSize;
                    int childDesiredWidth, childDesiredHeight;
                    child.GetDesiredSize(out childDesiredWidth, out childDesiredHeight);

                    if (fHorizontal)
                    {
                        previousChildSize = childDesiredWidth;
                        child.Arrange(childPosition, 0, previousChildSize, System.Math.Max(arrangeHeight, childDesiredHeight));
                    }
                    else
                    {
                        previousChildSize = childDesiredHeight;
                        child.Arrange(0, childPosition, System.Math.Max(arrangeWidth, childDesiredWidth), previousChildSize);
                    }
                }
            }
        }

        private Orientation _orientation;
    }
}


