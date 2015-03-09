////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public abstract class ContentControl : Control
    {
        public UIElement Child
        {
            get
            {
                if (LogicalChildren.Count > 0)
                {
                    return _logicalChildren[0];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                VerifyAccess();

                LogicalChildren.Clear();
                LogicalChildren.Add(value);
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            UIElement child = this.Child;
            if (child != null)
            {
                child.Measure(availableWidth, availableHeight);
                child.GetDesiredSize(out desiredWidth, out desiredHeight);
            }
            else
            {
                desiredWidth = desiredHeight = 0;
            }
        }
    }
}


