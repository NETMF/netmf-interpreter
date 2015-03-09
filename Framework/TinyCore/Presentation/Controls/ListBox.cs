////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class ListBox : ContentControl
    {
        public ListBox()
        {
            _panel = new StackPanel();
            _scrollViewer = new ScrollViewer();
            _scrollViewer.Child = _panel;
            this.LogicalChildren.Add(_scrollViewer);
        }

        public ListBoxItemCollection Items
        {
            get
            {
                VerifyAccess();

                if (_items == null)
                {
                    _items = new ListBoxItemCollection(this, _panel.Children);
                }

                return _items;
            }
        }

        public event SelectionChangedEventHandler SelectionChanged
        {
            add
            {
                VerifyAccess();
                _selectionChanged += value;
            }

            remove
            {
                VerifyAccess();
                _selectionChanged -= value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                VerifyAccess();

                if (_selectedIndex != value)
                {
                    if (value < -1)
                    {
                        throw new ArgumentOutOfRangeException("SelectedIndex");
                    }

                    ListBoxItem item = (_items != null && value >= 0 && value < _items.Count) ? _items[value] : null;

                    if (item != null && !item.IsSelectable)
                    {
                        throw new InvalidOperationException("Item is not selectable");
                    }

                    ListBoxItem previousItem = SelectedItem;
                    if (previousItem != null)
                    {
                        previousItem.OnIsSelectedChanged(false);
                    }

                    SelectionChangedEventArgs args = new SelectionChangedEventArgs(_selectedIndex, value);
                    _selectedIndex = value;

                    if (item != null)
                    {
                        item.OnIsSelectedChanged(true);
                    }

                    if (_selectionChanged != null)
                    {
                        _selectionChanged(this, args);
                    }
                }
            }
        }

        public ListBoxItem SelectedItem
        {
            get
            {
                if (_items != null && _selectedIndex >= 0 && _selectedIndex < _items.Count)
                {
                    return _items[_selectedIndex];
                }

                return null;
            }

            set
            {
                VerifyAccess();

                int index = Items.IndexOf(value);
                if (index != -1)
                {
                    SelectedIndex = index;
                }
            }
        }

        public void ScrollIntoView(ListBoxItem item)
        {
            VerifyAccess();

            if (!Items.Contains(item)) return;

            int panelX, panelY;
            _panel.GetLayoutOffset(out panelX, out panelY);

            int x, y;
            item.GetLayoutOffset(out x, out y);

            int top = y + panelY;
            int bottom = top + item._renderHeight;

            // Make sure bottom of item is in view
            //
            if (bottom > _scrollViewer._renderHeight)
            {
                _scrollViewer.VerticalOffset -= (_scrollViewer._renderHeight - bottom);
            }

            // Make sure top of item is in view
            //
            if (top < 0)
            {
                _scrollViewer.VerticalOffset += top;
            }
        }

        protected override void OnButtonDown(Microsoft.SPOT.Input.ButtonEventArgs e)
        {
            if (e.Button == Button.VK_DOWN && _selectedIndex < Items.Count - 1)
            {
                int newIndex = _selectedIndex + 1;
                while (newIndex < Items.Count && !Items[newIndex].IsSelectable) newIndex++;

                if (newIndex < Items.Count)
                {
                    SelectedIndex = newIndex;
                    ScrollIntoView(SelectedItem);
                    e.Handled = true;
                }
            }
            else if (e.Button == Button.VK_UP && _selectedIndex > 0)
            {
                int newIndex = _selectedIndex - 1;
                while (newIndex >= 0 && !Items[newIndex].IsSelectable) newIndex--;

                if (newIndex >= 0)
                {
                    SelectedIndex = newIndex;
                    ScrollIntoView(SelectedItem);
                    e.Handled = true;
                }
            }
        }

        //
        // Scrolling events re-exposed from the ScrollViewer
        //

        /// <summary>
        /// Event handler if the scroll changes.
        /// </summary>
        public event ScrollChangedEventHandler ScrollChanged
        {
            add { _scrollViewer.ScrollChanged += value; }
            remove { _scrollViewer.ScrollChanged -= value; }
        }

        /// <summary>
        /// Horizontal offset of the scroll.
        /// </summary>
        public int HorizontalOffset
        {
            get
            {
                return _scrollViewer.HorizontalOffset;
            }

            set
            {
                _scrollViewer.HorizontalOffset = value;
            }
        }

        /// <summary>
        /// Vertical offset of the scroll.
        /// </summary>
        public int VerticalOffset
        {
            get
            {
                return _scrollViewer.VerticalOffset;
            }

            set
            {
                _scrollViewer.VerticalOffset = value;
            }
        }

        /// <summary>
        /// Extent height of the scroll area.
        /// </summary>
        public int ExtentHeight
        {
            get
            {
                return _scrollViewer.ExtentHeight;
            }
        }

        /// <summary>
        /// Extent width of the scroll area.
        /// </summary>
        public int ExtentWidth
        {
            get
            {
                return _scrollViewer.ExtentWidth;
            }
        }

        /// <summary>
        /// The scrolling style.
        /// </summary>
        public ScrollingStyle ScrollingStyle
        {
            get
            {
                return _scrollViewer.ScrollingStyle;
            }

            set
            {
                _scrollViewer.ScrollingStyle = value;
            }
        }

        internal ScrollViewer _scrollViewer;
        internal StackPanel _panel;
        private int _selectedIndex = -1;
        private SelectionChangedEventHandler _selectionChanged;

        private ListBoxItemCollection _items;
    }
}


