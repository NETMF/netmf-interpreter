using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class Panel : UIElement
    {
        public UIElementCollection Children
        {
            get
            {
                return LogicalChildren;
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            desiredWidth = desiredHeight = 0;
            UIElementCollection children = _logicalChildren;
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    UIElement child = children[i];
                    child.Measure(availableWidth, availableHeight);
                    int childDesiredWidth, childDesiredHeight;
                    child.GetDesiredSize(out childDesiredWidth, out childDesiredHeight);
                    desiredWidth = System.Math.Max(desiredWidth, childDesiredWidth);
                    desiredHeight = System.Math.Max(desiredHeight, childDesiredHeight);
                }
            }
        }
    }
}


