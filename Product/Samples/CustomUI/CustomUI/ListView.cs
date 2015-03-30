using System;
using System.Collections;
using System.IO;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

namespace CustomUI
{
    /// <summary>
    /// Holds a sub-item for a list view.
    /// </summary>
    public class ListViewSubItem
    {
        public string Text;
    }

    /// <summary>
    /// Defines the list view item.
    /// </summary>
    public class ListViewItem
    {
        // Initialize the array of sub-items.
        public ArrayList SubItems = new ArrayList();

        /// <summary>
        /// Adds a sub-item to the list view item.
        /// </summary>
        /// <param name="itemText">The text of the sub-item.</param>
        public void AddSubItem(string itemText)
        {
            ListViewSubItem listViewSubItem = new ListViewSubItem();
            listViewSubItem.Text = itemText;

            SubItems.Add(listViewSubItem);
        }
    }

    /// <summary>
    /// Holds information about a column in the list view.
    /// </summary>
    public class ListViewColumn
    {
        public string Name;
        public int Width;
    }

    /// <summary>
    /// Defines the list view object.
    /// </summary>
    class ListView : UIElement
    {
        public ListViewItem SelectedItem = null;

        public ArrayList Columns = new ArrayList();
        public ArrayList Items = new ArrayList();

        // Initialize the members.
        protected Brush _emptyBrush = new SolidColorBrush(Color.White);
        protected Brush _grayBrush = new SolidColorBrush(ColorUtility.ColorFromRGB(160, 160, 160));
        protected Brush _lightBrush = new SolidColorBrush(ColorUtility.ColorFromRGB(216, 216, 216));
        protected Brush _cyanBrush = new SolidColorBrush(ColorUtility.ColorFromRGB(192, 192, 255));
        protected Brush _lightCyanBrush = new SolidColorBrush(ColorUtility.ColorFromRGB(224, 224, 255));
        protected Pen _borderPen = new Pen(Color.Black, 1);
        protected Pen _cyanPen = new Pen(ColorUtility.ColorFromRGB(192, 192, 255), 1);
        protected Pen _darkCyanPen = new Pen(ColorUtility.ColorFromRGB(128, 128, 192), 1);
        protected Pen _emptyPen = new Pen(ColorUtility.ColorFromRGB(255, 255, 255), 1);
        protected Pen _grayPen = new Pen(ColorUtility.ColorFromRGB(192, 192, 192), 1);
        protected Font _font;

        protected int _columnHeaderHeight = 20;
        protected int _itemHeight = 20;
        protected int _scrollbarDimension = 10;

        protected int _verticalSliderY = 0;
        protected int _verticalSliderHeight = 0;

        protected int _horizontalSliderX = 0;
        protected int _horizontalSliderWidth = 0;

        protected int _verticalScrollViewHeight = 0;
        protected int _horizontalScrollViewWidth = 0;

        protected int _totalViewHeight = 0;
        protected int _totalViewWidth = 0;

        protected int _horizontalScroll = 0;
        protected int _verticalScroll = 0;

        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Constructs a ListView, of the specified width and height.
        /// </summary>
        /// <param name="width">The width of the ListView.</param>
        /// <param name="height">The height of the ListView.</param>
        public ListView(int width, int height)
        {
            Width = width;
            Height = height;
            _font = Resources.GetFont(Resources.FontResources.small);

            _verticalScrollViewHeight = Height - _scrollbarDimension - _columnHeaderHeight;
            _horizontalScrollViewWidth = Width;
        }

        /// <summary>
        /// Renders the list view.
        /// </summary>
        /// <param name="dc">The drawing context of the list view.</param>
        public override void OnRender(DrawingContext dc)
        {
            // Draw a border around the list view.
            dc.DrawRectangle(_emptyBrush, _borderPen, 0, 0, Width, Height);

            RenderColumns(dc);
            RenderItems(dc);
            RenderVerticalScrollBar(dc, 0, _columnHeaderHeight);
            RenderHorizontalScrollBar(dc);
        }

        /// <summary>
        /// Empties the list view of all items.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Adds a column to the list view.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <param name="width">The width of the column.</param>
        public void AddColumn(string name, int width)
        {
            ListViewColumn column = new ListViewColumn();
            column.Name = name;
            column.Width = width;

            Columns.Add(column);
        }

        /// <summary>
        /// Adds an item to the list view, given a full list view item.
        /// </summary>
        /// <param name="listViewItem">The item to add to the list view.
        /// </param>
        public void AddItem(ListViewItem listViewItem)
        {
            Items.Add(listViewItem);
        }

        public void RemoveItem(ListViewItem listViewItem)
        {
            if (listViewItem == SelectedItem)
            {
                SelectedItem = null;
            }

            Items.Remove(listViewItem);
        }

        /// <summary>
        /// Renders the column titles of the list view.
        /// </summary>
        /// <param name="dc"></param>
        protected virtual void RenderColumns(DrawingContext dc)
        {
            // Draw the header background.
            dc.DrawRectangle(_cyanBrush, _darkCyanPen, 0, 2, Width, _columnHeaderHeight);

            int columnWidthSum = 5 - _horizontalScroll;
            for (int i = 0; i < Columns.Count; i++)
            {
                // Draw the column text.
                dc.DrawText(((ListViewColumn)Columns[i]).Name, _font, Color.Black, columnWidthSum, 2);

                // Increment by the column width.
                columnWidthSum += ((ListViewColumn)Columns[i]).Width;
            }

            // Store the total column width.
            _totalViewWidth = columnWidthSum;
        }

        /// <summary>
        /// Renders the items in the list view.
        /// </summary>
        /// <param name="dc"></param>
        protected virtual void RenderItems(DrawingContext dc)
        {
            // Create the brushes and pens for drawing the items.
            Brush[] brushes = new Brush[] { _emptyBrush, _lightCyanBrush, _grayBrush };
            Pen[] pens = new Pen[] { _emptyPen, _cyanPen };

            // Set the starting location for the first item.  Offset the
            // location by _horizontalScroll and _verticalScroll, which are the scroll positions.
            int cx = 6 - _horizontalScroll;
            int cy = _columnHeaderHeight + 2 - _verticalScroll;
            int j = 0;

            // Iterate through all the items.
            for (int ii = 0; ii < Items.Count; ii++)
            {
                // Only draw the items that are visible.
                if (cy >= _columnHeaderHeight)
                {
                    // If the item is selected...
                    if (((ListViewItem)Items[ii]) == SelectedItem)
                    {
                        // Draw a rectangle alternately shaded and transparent.
                        dc.DrawRectangle(brushes[2], pens[j & 1], 2, cy, Width - 4, _itemHeight);
                    }
                    else
                    {
                        // Draw a rectangle alternately shaded and transparent.
                        dc.DrawRectangle(brushes[j & 1], pens[j & 1], 2, cy, Width - 4, _itemHeight);
                    }

                    // Set the starting location for the first column.
                    // Offset the location by _horizontalScroll, which is the scroll
                    // position.
                    cx = 4 - _horizontalScroll;
                    int i = 0;
                    int newcx = 0;

                    // Iterate through the sub-items, which are columns.
                    for (int e = 0; e < ((ListViewItem)Items[ii]).SubItems.Count; e++)
                    {
                        // Calculate the x position of the next column.
                        newcx = cx + ((ListViewColumn)Columns[i]).Width;

                        // Draw only the items that will appear on the
                        // screen.
                        if (newcx > 0)
                        {
                            string text = ((ListViewSubItem)((ListViewItem)Items[ii]).SubItems[e]).Text;
                            dc.DrawText(text, _font, Color.Black, cx, cy + 2);
                        }

                        // Move to the next column.
                        cx = newcx;
                        i++;

                        // If the location is past the right side of the
                        // screen, don't show any more columns.
                        if (cx > Width)
                        {
                            break;
                        }
                    }
                }

                // Increment to the next item.
                cy += _itemHeight;
                j++;

                // If the location is past the bottom of the screen, don't show any more items.
                if (cy > Height)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Renders the vertical scroll bar.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void RenderVerticalScrollBar(DrawingContext dc, int x, int y)
        {
            try
            {
                // Calculate and store the total view height.
                _totalViewHeight = Items.Count * _itemHeight;

                // If there is no height, don't go any further.
                if (_totalViewHeight == 0)
                {
                    return;
                }

                // Calculate and store the vertical scroll information.
                _verticalSliderHeight = (_verticalScrollViewHeight * _verticalScrollViewHeight) / _totalViewHeight;
                _verticalSliderY = (_verticalScroll * _verticalScrollViewHeight) / _totalViewHeight;

                // Draw the vertical scrollbar background.
                int sx = x + Width - _scrollbarDimension;
                dc.DrawRectangle(_lightBrush, _grayPen, sx, y, _scrollbarDimension, _verticalScrollViewHeight);

                // Draw the scrollbar thumb.
                if (_verticalSliderHeight < Height)
                {
                    dc.DrawRectangle(_grayBrush,
                                     _emptyPen,
                                     sx + 1,
                                     y + 1 + _verticalSliderY,
                                     _scrollbarDimension - 2,
                                     _verticalSliderHeight);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }

        /// <summary>
        /// Renders the horizontal scroll bar.
        /// </summary>
        /// <param name="dc">The drawing context.</param>
        protected void RenderHorizontalScrollBar(DrawingContext dc)
        {
            // If there is no width, don't go any further.
            if (_totalViewWidth == 0)
            {
                return;
            }

            // Calculate and store the horizontal scroll information.
            _horizontalSliderWidth = (_horizontalScrollViewWidth * _horizontalScrollViewWidth) / _totalViewWidth;
            if (_horizontalSliderWidth > Width)
            {
                _horizontalSliderWidth = Width;
            }

            _horizontalSliderX = (_horizontalScroll * _horizontalScrollViewWidth) / _totalViewWidth;

            // Draw the horizontal scrollbar background.
            int sy = Height - _scrollbarDimension;
            dc.DrawRectangle(_lightBrush, _grayPen, 0, sy, _horizontalScrollViewWidth, _scrollbarDimension);

            // Draw the scrollbar thumb.
            dc.DrawRectangle(_grayBrush,
                             _emptyPen,
                             _horizontalSliderX,
                             sy + 1,
                             _horizontalSliderWidth,
                             _scrollbarDimension - 2);
        }

        /// <summary>
        /// Overrides the default measurements and sets them to the desired
        /// width and height.
        /// </summary>
        /// <param name="availableWidth"></param>
        /// <param name="availableHeight"></param>
        /// <param name="desiredWidth"></param>
        /// <param name="desiredHeight"></param>
        protected override void MeasureOverride(
            int availableWidth,
            int availableHeight,
            out int desiredWidth,
            out int desiredHeight)
        {
            desiredWidth = (availableWidth > Width) ? Width : availableWidth;
            desiredHeight = (availableHeight > Height) ? Height : availableHeight;
        }

        /// <summary>
        /// Handles the OnTouchDown event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTouchDown(TouchEventArgs e)
        {
            int x = 0;
            int y = 0;

            e.GetPosition(this, 0, out x, out y);

            try
            {
                // Figure out which section of the screen was clicked.
                if (y <= _columnHeaderHeight)
                {
                    // Ignore clicks on column headers.
                }
                else if (y >= (Height - _scrollbarDimension))
                {
                    // The horizontal scrollbar was clicked.
                    OnHorizontalScrollStylusDown(x);
                }
                else if (x >= (Width - _scrollbarDimension))
                {
                    // The vertical scrollbar was clicked.
                    OnVerticalScrollStylusDown(y);
                }
                else
                {
                    // Main section; an item was clicked.

                    int previousIndex = Items.IndexOf(SelectedItem);

                    // Calculate which item was clicked.
                    int index = ((y - _columnHeaderHeight) + _verticalScroll) / _itemHeight;

                    // No item is selected, so select this one.
                    if ((index >= 0) && (index < Items.Count))
                    {
                        SelectedItem = (ListViewItem)Items[index];
                    }
                    else
                    {
                        SelectedItem = null;
                        index = -1;
                    }

                    // Refresh the list view.
                    Invalidate();

                    if (SelectionChanged != null)
                    {
                        SelectionChanged(this, new SelectionChangedEventArgs(previousIndex, index));
                    }
                }
            }
            catch (IOException ex)
            {
                Debug.Print(ex.ToString());
            }
        }

        /// <summary>
        /// Handles the OnTouchMove event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTouchMove(TouchEventArgs e)
        {
            int x = 0;
            int y = 0;

            e.GetPosition(this, 0, out x, out y);

            // If a slider was tapped...
            if (_moveVerticalSlider)
            {
                // The vertical slider was clicked.

                // Calculate the difference between the old position and the
                // new position.
                int diff = y - _verticalSliderMoveY;

                // Calculate the view difference.
                int realDiff = (diff * _totalViewHeight) / _verticalScrollViewHeight;

                // Set the new position of the slider.
                _verticalSliderMoveY = y;

                // Validate the new position and bound-align it.
                if (_verticalSliderMoveY < _verticalSliderY + _columnHeaderHeight)
                {
                    _verticalSliderMoveY = _verticalSliderY + _columnHeaderHeight;
                }

                // Update the view scroll position.
                ScrollVertical(realDiff);
            }
            else if (_moveHorizontalSlider)
            {
                // The horizontal slider was clicked.

                // Calculate the difference between the old position and the
                // new position.
                int diff = x - _horizontalSliderMoveX;

                // Validate and bound-align the difference.
                if ((x >= _horizontalSliderMoveX) &&
                    (_horizontalSliderX + _horizontalSliderWidth + diff >= Width + _scrollbarDimension))
                {
                    diff -= (Width + _scrollbarDimension) - (_horizontalSliderX + _horizontalSliderWidth + diff);
                }

                // Validate the new position.
                if ((_horizontalSliderX + _horizontalSliderWidth + diff < Width + _scrollbarDimension) ||
                    (x < _horizontalSliderMoveX))
                {
                    // Calculate the view difference.
                    int realDiff = (diff * _totalViewWidth) / _horizontalScrollViewWidth;

                    // Set the new position of the slider.
                    _horizontalSliderMoveX = x;

                    // Update the view scroll position.
                    ScrollHorizontal(realDiff);
                }
            }
        }

        /// <summary>
        /// The method handles the OnTouchUp event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTouchUp(TouchEventArgs e)
        {
            // Reset the slider flags.
            _moveVerticalSlider = false;
            _moveHorizontalSlider = false;
        }

        // Declarations of the slider flags.
        protected bool _moveVerticalSlider = false;
        protected int _verticalSliderMoveY = 0;
        protected bool _moveHorizontalSlider = false;
        protected int _horizontalSliderMoveX = 0;

        /// <summary>
        /// Handles the OnVerticalScrollStylusDown event.
        /// </summary>
        /// <param name="y"></param>
        protected virtual void OnVerticalScrollStylusDown(int y)
        {
            if (y < _verticalSliderY)
            {
                // Page up.
                ScrollVertical(-_itemHeight * 5);
            }
            else if (y > (_verticalSliderY + _verticalSliderHeight))
            {
                // Page down.
                ScrollVertical(_itemHeight * 5);
            }
            else
            {
                // On the slider.
                _verticalSliderMoveY = y;
                _moveVerticalSlider = true;
            }
        }

        /// <summary>
        /// Handles the OnHorizontalScrollStylusDown event.
        /// </summary>
        /// <param name="x"></param>
        protected virtual void OnHorizontalScrollStylusDown(int x)
        {
            if (x < _horizontalSliderX)
            {
                // Page left.
                ScrollHorizontal(-20);
            }
            else if (x > (_horizontalSliderX + _horizontalSliderWidth))
            {
                // Page right.
                ScrollHorizontal(20);
            }
            else
            {
                // The thumb was clicked.
                _horizontalSliderMoveX = x;
                _moveHorizontalSlider = true;
            }
        }

        /// <summary>
        /// Updates the vertical value of the scroll view.
        /// </summary>
        /// <param name="delta"></param>
        protected virtual void ScrollVertical(int delta)
        {
            // Update the member
            _verticalScroll += delta;

            // Validate and bounds align
            if (_verticalScroll < 0)
            {
                _verticalScroll = 0;
            }

            if (_verticalScroll > _totalViewHeight)
            {
                _verticalScroll = _totalViewHeight;
            }

            // Refresh the view
            Invalidate();
        }

        /// <summary>
        /// Updates the horizontal value of the scroll view.
        /// </summary>
        /// <param name="delta"></param>
        protected virtual void ScrollHorizontal(int delta)
        {
            // Update the member.
            _horizontalScroll += delta;

            // Validate and bounds-align.
            if (_horizontalScroll < 0)
            {
                _horizontalScroll = 0;
            }

            if (_horizontalScroll > _totalViewWidth)
            {
                _horizontalScroll = _totalViewWidth;
            }

            // Refresh the view.
            Invalidate();
        }
    }
}
