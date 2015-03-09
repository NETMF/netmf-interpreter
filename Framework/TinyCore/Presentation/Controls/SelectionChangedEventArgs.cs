using System;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class SelectionChangedEventArgs : EventArgs
    {
        public readonly int PreviousSelectedIndex;
        public readonly int SelectedIndex;

        public SelectionChangedEventArgs(int previousIndex, int newIndex)
        {
            PreviousSelectedIndex = previousIndex;
            SelectedIndex = newIndex;
        }
    }
}


