////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Platform.Test;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ListBoxTests : Master_Controls, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for ListBox tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults ListBox_ExtentHeightTest1()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Adding ListBoxItems to ListBox and verifying the ExtentHeight");
            uniformW = false;
            _rectW = _midX;
            _rectH = _midY / 2;
            UpdateWindow();
            Log.Comment("The ExtentHeight is the sum of the heights of the ListBoxItems");
            lbx.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(GetExtentDimensions_GetOffsets), null);
            if (_getExtentHeight != (_colors.Length * _rectH))
            {
                Log.Comment("Expected ExtentHeight '" + (_colors.Length * _rectH) +
                    "' but got '" + _getExtentHeight + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ListBox_ExtentWidthTest2()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Adding ListBoxItems to ListBox and verifying the ExtentWidth");
            uniformW = false;
            UpdateWindow();
            Log.Comment("The ExtentWidth is the width of the widest ListBoxItem");
            lbx.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(GetExtentDimensions_GetOffsets), null);
            if (_getExtentWidth != maxW)
            {
                Log.Comment("Expected ExtentWidth '" + maxW +
                    "' but got '" + _getExtentWidth + "'");
                testResult = MFTestResults.Fail;
            }
            _rectW = _width + 5;
            UpdateWindow();
            Log.Comment("ListBoxItems have width larger than SystemMetrics width");
            lbx.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(GetExtentDimensions_GetOffsets), null);
            if (_getExtentWidth != _width)
            {
                Log.Comment("Expected ExtentWidth '" + _width +
                    "' but got '" + _getExtentWidth + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ListBox_VerticalOffsetTest3()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Adding ListBoxItems to ListBox, setting Vertical Offset and verifying");
            uniformW = false;
            _rectW = _midX;
            _rectH = _midY / 2;
            int maxOffset = (_colors.Length * _rectH) - _height;
            int[] offset = new int[] { 0, new Random().Next(_rectH), _rectH, maxOffset, maxOffset + 10 };
            for (int i = 0; i < offset.Length; i++)
            {
                _vOffset = offset[i];
                Log.Comment("Vertical Offset = " + _vOffset);
                UpdateWindow();
                lbx.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
             new DispatcherOperationCallback(GetExtentDimensions_GetOffsets), null);
                int temp = offset[i];
                if (temp > maxOffset)
                {
                    temp = maxOffset;
                }
                if (_getVOffset != temp)
                {
                    Log.Comment("Expected Vertical Offset '" + temp + "' but got " + _getVOffset + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ListBox_ItemsTest5()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            uniformW = false;
            if (lbxItemColl.Count != _colors.Length)
            {
                Log.Comment("Expected Items size '" + _colors.Length + "' but got '" + lbxItemColl.Count + "'");
                testResult = MFTestResults.Fail;
            }
            for (int i = 0; i < lbxItemColl.Count; i++)
            {
                if (!lbxItemColl.Contains(lbis[i]))
                {
                    Log.Comment("The ListBox.Items do not contain the " + i + " ListBoxItem");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ListBox_ScrollIntoViewTest6()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            uniformW = true;
            _vOffset = 0;
            _rectW = _midX;
            _rectH = _midY / 2;

            int x = 0;
            int y = _rectH * 3;

            UpdateWindow();
            Log.Comment("Verifying pixel colors before scrolling the item into view");
            Thread.Sleep(c_wait);
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            if (VerifyingPixelColor(chkPoints, _colors[3]) != MFTestResults.Pass)
            {
                Log.Comment("Failure : items not positioned right before doing ListBox.ScrollIntoView");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Doing ListBox.ScrollIntoView last element and verifying");
            lbx.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(ScrollIntoViewLastElt), null);
            Thread.Sleep(c_wait);

            y = _height - _rectH;
            chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            if (VerifyingPixelColor(chkPoints, _colors[_colors.Length - 1]) != MFTestResults.Pass)
            {
                Log.Comment("Failure : last item not scrolled to view on display after ListBox.ScrollIntoView");
                testResult = MFTestResults.Fail;
            }
            if (_getVOffset != ((_colors.Length - 4) * _rectH) - (_height % _rectH))
            {
                Log.Comment("Expected vertical offset '" + ((_colors.Length - 4) * _rectH - (_height % _rectH)) +
                    "' but got '" + _getVOffset + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ListBox_SelectedIndexTest7()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            index = 3;
            uniformW = true;
            _vOffset = 0;
            _rectW = _midX;
            _rectH = _midY / 2;
            UpdateWindow();
            if (_getSelectedIndex != index)
            {
                Log.Comment("Expected Selected Index '" + index +
                    "' but got '" + _getSelectedIndex + "'");
                testResult = MFTestResults.Fail;
            }
            if (_getSelectedItem != lbis[index])
            {
                Log.Comment("Failure getting selected Item");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        Color[] _colors = new Color[] { Colors.Blue, Colors.Red, (Color)0x00ffff,
            Colors.Black, Colors.Green, Colors.Gray };
        ListBox lbx = null;
        ListBoxItemCollection lbxItemColl = null;
        ListBoxItem[] lbis = null;
        ListBoxItem _getSelectedItem = null;

        int _rectW = _midX, _rectH = _midY, maxW = 0;
        int _getExtentHeight, _getExtentWidth;
        int _vOffset, _hOffset = 0, _getVOffset, _getHOffset;
        int index, _getSelectedIndex;
        bool uniformW = false;

        object ListBox_UpdateWindow(object obj)
        {
            maxW = 0;
            lbx = new ListBox();
            lbis = new ListBoxItem[_colors.Length];
            Random random = new Random();
            for (int i = 0; i < _colors.Length; i++)
            {
                lbis[i] = new ListBoxItem();
                Rectangle rect = new Rectangle();
                int wd = _rectW + random.Next(100);
                if (wd > maxW)
                {
                    maxW = wd;
                }
                if (uniformW)
                {
                    rect.Width = _rectW;
                }
                else
                {
                    rect.Width = wd;
                }
                rect.Height = _rectH;
                rect.Fill = new SolidColorBrush(_colors[i]);
                lbis[i].Child = rect;
                lbx.Items.Add(lbis[i]);
            }
            lbx.VerticalOffset = _vOffset;
            lbx.HorizontalOffset = _hOffset;
            lbx.SelectedIndex = index;

            lbxItemColl = lbx.Items;
            _getSelectedIndex = lbx.SelectedIndex;
            _getSelectedItem = lbx.SelectedItem;

            mainWindow.Child = lbx;

            return null;
        }

        void UpdateWindow()
        {
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(ListBox_UpdateWindow), null);

            _autoEvent.WaitOne();

            Thread.Sleep(5);
        }

        object GetExtentDimensions_GetOffsets(object obj)
        {
            _getExtentHeight = lbx.ExtentHeight;
            _getExtentWidth = lbx.ExtentWidth;
            _getVOffset = lbx.VerticalOffset;
            _getHOffset = lbx.HorizontalOffset;
            return null;
        }

        object ScrollIntoViewLastElt(object obj)
        {
            lbx.ScrollIntoView(lbis[_colors.Length - 1]);
            return null;
        }
    }
}
