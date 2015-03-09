////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Media;
using System.Threading;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ScrollViewerTests : Master_Controls, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for ScrollViewer tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults ScrollViewer_ExtentWidth_ExtentHeightTest1()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Random random = new Random();
            int[] xtValue = new int[] { 0, random.Next(100), int.MaxValue };

            Log.Comment("Setting diff. Values of Width and Height to ScrollViewer's child and");
            Log.Comment("verifying the ExtentWidth and ExtentHeight");

            for (int i = 0; i < xtValue.Length; i++)
            {
                _canHeight = xtValue[i];
                _canWidth = xtValue[i];

                UpdateWindow();
                
                scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(GetSVExtentDimensions), null);

                _autoEvent.WaitOne();

                if ((_getExtentWidth != _canWidth) || (_getExtentHeight != _canHeight))
                {
                    Log.Comment("Expected ScrollViewer ExtentWidth '" +
                        _canWidth + "' but got '" + _getExtentWidth);
                    Log.Comment("Expected ScrollViewer ExtentHeight '" +
                        _canHeight + "' but got '" + _getExtentHeight);
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_HorizontalOffset_Negative_Test2()
        {
            Log.Comment("Setting the HorizontalOffset to zero and -ve values and");
            Log.Comment("verifying the scrolled content is offSetted by zero (not offsetted)");

            return NegativeOffsetTest(ref _hOffset, ref _getHOffset);
        }

        void ConvertPts(ref int x, ref int y)
        {
            if (_rectW < _scvWidth)
            {
                if ((((~_width) & _scvWidth & 1) == 1) || ((~_scvWidth) & _rectW & 1) == 1) x--;
            }
            else
            {
                if (((~_width) & _rectW & 1) == 1) x--;
            }
            if (_rectH < _scvHeight)
            {
                if ((((~_height) & _scvHeight & 1) == 1) || ((~_scvHeight) & _rectH & 1) == 1) x--;
            }
            else
            {
                if (((~_height) & _rectH & 1) == 1) x--;
            }
        }

        [TestMethod]
        public MFTestResults ScrollViewer_HorizontalOffset_Positive_Test3()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            _canWidth = _width;
            _canHeight = _height;
            scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                 new DispatcherOperationCallback(GetSVExtentDimensions), null);
            Log.Comment("Setting the HorizontalOffset +ve values and verifying");
            int x = _midX - _rectW / 2, _wd = _rect.Width;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            int visibleScrollLen = (_scvWidth - _rectW) / 2;

            const int c_scrollValue = 4;

            for (int i = 0; i < (_getExtentWidth - _scvWidth) + 100; i += c_scrollValue)
            {
                Log.Comment("Horizontally offsetting the scrolled content by " + i);
                _hOffset = i;
                UpdateWindow();
                Log.Comment("Initializing Points inside scrolled content and verifying");

                if (i <= (_getExtentWidth - _scvWidth))
                {
                    if (i > visibleScrollLen)
                    {
                        _wd = _rectW - (i - visibleScrollLen);
                        x = (_width - _scvWidth) / 2;
                    }
                    else if (i > 0)
                    {
                        x -= c_scrollValue;
                    }
                }
                else
                {
                    _wd = _rectW - (_getExtentWidth - _scvWidth);
                }

                Point[] chkPoints = GetRandomPoints_InRectangle(10, _wd, _rect.Height, x, y);
                if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
                {
                    Log.Comment("Failure : The scrollviewed content has not been offsetted by " + i);
                    testResult = MFTestResults.Fail;
                }
                if (_getHOffset != i)
                {
                    Log.Comment("ScrollViewer.HorizontalOffset expected '" + i + "' but got '" + _getHOffset + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_VerticalOffset_Negative_Test4()
        {
            Log.Comment("Setting the VerticalOffset to zero and -ve values and");
            Log.Comment("verifying the scrolled content is offSetted by zero (not offsetted)");

            return NegativeOffsetTest(ref _vOffset, ref _getVOffset);
        }

        [TestMethod]
        public MFTestResults ScrollViewer_VerticalOffset_Positive_Test5()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            _canWidth = _width;
            _canHeight = _height;
            scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                 new DispatcherOperationCallback(GetSVExtentDimensions), null);
            Log.Comment("Setting the HorizontalOffset +ve values and verifying");
            
            int y = _midY - _rectH / 2;
            int x = _midX - _rectW / 2;
            int ht = _rectH;

            ConvertPts(ref x, ref y);

            int visibleScrollLen = (_scvHeight - _rectH) / 2;

            const int c_scrollValue = 4;

            for (int i = 0; i < (_getExtentHeight - _scvHeight) + 100; i += c_scrollValue)
            {
                Log.Comment("Vertically offsetting the scrolled content by " + i);
                _vOffset = i;
                UpdateWindow();
                Log.Comment("Initializing Points inside scrolled content and verifying");

                if (i <= (_getExtentHeight - _scvHeight))
                {
                    if (i > visibleScrollLen)
                    {
                        ht = _rectH - (i - visibleScrollLen);
                        y = (_height - _scvHeight) / 2;
                    }
                    else if (i > 0)
                    {
                        y -= c_scrollValue;
                    }
                }
                else
                {
                    ht = _rectH - (_getExtentHeight - _scvHeight);
                }
                Point[] chkPoints = GetRandomPoints_InRectangle(10, _rect.Width, ht, x, y);
                if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
                {
                    Log.Comment("Failure : The scrollviewed content has not been offsetted by " + i);
                    testResult = MFTestResults.Fail;
                }
                if (_getVOffset != i)
                {
                    Log.Comment("ScrollViewer.VerticalOffset expected '" + i + "' but got '" + _getVOffset + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_LineHeightTest6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            _hOffset = 0;
            _vOffset = 0;
            _lineUp = false;
            _lineDown = true;
            int tempRectH = _rectH;
            _lineWidth = 0;
            int[] lHeights = new int[] { 0, ((_scvHeight - _rectH) / 4), 
                   ((_scvHeight - _rectH) / 2), int.MaxValue };

            int maxScrollDown = _canHeight - _scvHeight;
            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;
            int tmpY = y;

            ConvertPts(ref x, ref y);

            for (int i = 0; i < lHeights.Length; i++)
            {
                if (CleaningWindow() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Line Height = '" + lHeights[i] + "', doing ScrollViewer.LineDown and verifying");
                _lineHeight = lHeights[i];
                UpdateWindow();
                if (lHeights[i] > maxScrollDown)
                {
                    tempRectH = _rectH - (maxScrollDown - cTop);
                    tmpY = (_height - _scvHeight) / 2;
                }
                else
                {
                    tmpY = y - lHeights[i];
                }
                Point[] chkPoints = GetRandomPoints_InRectangle(10, _rectW, tempRectH, x, tmpY);
                if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
                {
                    testResult = MFTestResults.Fail;
                    Log.Comment("Failure verifying pixel color after scrolling");
                }

                if (_getLineHeight != lHeights[i])
                {
                    Log.Comment("Expected ScrollViewer.LineHeight to be '" +
                        lHeights[i] + "' but got " + _getLineHeight);
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_LineWidthTest7()
        {
            MFTestResults testResult = MFTestResults.Pass;
            _hOffset = 0;
            _vOffset = 0;
            _lineLeft = false;
            _lineRight = true;
            int tempRectW = _rectW;
            _lineHeight = 0;
            int[] lWidths = new int[] { 0, ((_scvWidth - _rectW) / 4), 
                   ((_scvWidth - _rectW) / 2), int.MaxValue };

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;
            int tmpX = x;

            ConvertPts(ref x, ref y);

            int maxScrollRight = _canWidth - _scvWidth;
            for (int i = 0; i < lWidths.Length; i++)
            {
                if (CleaningWindow() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("LineWidth = '" + lWidths[i] + "', doing ScrollViewer.LineRight and verifying");
                _lineWidth = lWidths[i];
                UpdateWindow();
                if (lWidths[i] > maxScrollRight)
                {
                    tempRectW = _rectW - (maxScrollRight - cLeft);
                    tmpX = (_width - _scvWidth) / 2;
                }
                else
                {
                    tmpX = x - lWidths[i];
                }
                Point[] chkPoints = GetRandomPoints_InRectangle(10, tempRectW, _rectH, tmpX, y);
                if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
                {
                    testResult = MFTestResults.Fail;
                    Log.Comment("Failure verifying pixel color after scrolling");
                }

                if (_getLineWidth != lWidths[i])
                {
                    Log.Comment("Expected ScrollViewer.LineWidth to be '" +
                        lWidths[i] + "' but got " + _getLineWidth);
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_LineDownTest8()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (VerifyBeforeScrolling() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _hOffset = 0;
            _vOffset = 0;
            _lineUp = false;
            _lineDown = true;
            _lineWidth = 0;
            _lineHeight = (_scvHeight - _rectH) / 2;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            Log.Comment("Scrolling Down and Verifying");
            UpdateWindow();
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y - _lineHeight);
            if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Scrolling several times and verifying the vertical offset");
            int[] countArr = new int[] { 3, 7, (_canHeight - _scvHeight) + 1 };
            for (int i = 0; i < countArr.Length; i++)
            {
                scrollCount = countArr[i];
                UpdateWindow();
                if (_getVOffset != (countArr[i] * _lineHeight))
                {
                    Log.Comment("After scrolling down " + countArr[i] + ", expected vertical offset '" +
                        (countArr[i] * _lineHeight) + "' but got '" + _getVOffset + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_LineUpTest9()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (VerifyBeforeScrolling() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _hOffset = 0;
            _vOffset = 0;
            _lineUp = true;
            _lineDown = true;
            _lineWidth = 0;
            _lineHeight = (_scvHeight - _rectH) / 2;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            Log.Comment("Scrolling Down, Scrolling Up and verifying");
            UpdateWindow();
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Scrolling several times Down-Up and verifying the vertical offset is zero");
            int[] countArr = new int[] { 3, 7, (_canHeight - _scvHeight) + 1 };
            for (int i = 0; i < countArr.Length; i++)
            {
                scrollCount = countArr[i];
                UpdateWindow();
                if (_getVOffset != 0)
                {
                    Log.Comment("After scrolling down " + countArr[i] +
                        ", expected vertical offset '0' but got '" + _getVOffset + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_LineRightTest10()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (VerifyBeforeScrolling() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _hOffset = 0;
            _vOffset = 0;
            _lineLeft = false;
            _lineRight = true;
            _lineWidth = (_scvWidth - _rectW) / 2;
            _lineHeight = 0;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            Log.Comment("Scrolling Right and Verifying");
            UpdateWindow();
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x - _lineWidth, y);
            if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Scrolling several times and verifying the Horizontal offset");
            int[] countArr = new int[] { 3, 7, (_canWidth - _scvWidth) + 1 };
            for (int i = 0; i < countArr.Length; i++)
            {
                scrollCount = countArr[i];
                UpdateWindow();
                if (_getHOffset != (countArr[i] * _lineWidth))
                {
                    Log.Comment("After scrolling right " + countArr[i] + ", expected horizontal offset '" +
                        (countArr[i] * _lineWidth) + "' but got '" + _getHOffset + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_LineLeftTest11()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (VerifyBeforeScrolling() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _hOffset = 0;
            _vOffset = 0;
            _lineRight = true;
            _lineLeft = true;
            _lineWidth = (_scvWidth - _rectW) / 2;
            _lineHeight = 0;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            Log.Comment("Scrolling Right, Scrolling Left and verifying");
            UpdateWindow();
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Scrolling several times Right - Left and verifying the Horizontal offset is zero");
            int[] countArr = new int[] { 3, 7, (_canWidth - _scvWidth) + 1 };
            for (int i = 0; i < countArr.Length; i++)
            {
                scrollCount = countArr[i];
                UpdateWindow();
                if (_getHOffset != 0)
                {
                    Log.Comment("After scrolling Right - Left " + countArr[i] +
                        ", expected horizontal offset '0' but got '" + _getHOffset + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_PageDownTest12()
        {
            MFTestResults testResult = MFTestResults.Pass;
            _canWidth = _scvWidth;
            _canHeight = _scvHeight * 2;
            _rectW = _scvWidth;
            _rectH = _scvHeight;
            cTop = 0;
            cLeft = 0;
            _style = ScrollingStyle.PageByPage;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            if (VerifyBeforeScrolling() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Doing PageDown and verifying");
            scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(PageDown), null);
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            Thread.Sleep(c_wait);

            if (VerifyingPixelColor(chkPoints, _scvBground) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_PageUpTest13()
        {
            MFTestResults testResult = MFTestResults.Pass;
            _canWidth = _scvWidth;
            _canHeight = _scvHeight * 2;
            _rectW = _scvWidth;
            _rectH = _scvHeight;
            cTop = 0;
            cLeft = 0;
            _style = ScrollingStyle.PageByPage;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            if (VerifyBeforeScrolling() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Inorder to verify PageUp, 1st Does a Page-Down and PageUp");
            Log.Comment("Doing PageDown and verifying");
            scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(PageDown), null);
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            Thread.Sleep(c_wait);

            if (VerifyingPixelColor(chkPoints, _scvBground) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Doing PageUp and verifying");
            scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(PageUp), null);
            Thread.Sleep(c_wait);
            if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_PageRightTest14()
        {
            MFTestResults testResult = MFTestResults.Pass;
            _canWidth = _scvWidth * 2;
            _canHeight = _scvHeight;
            _rectW = _scvWidth;
            _rectH = _scvHeight;
            cTop = 0;
            cLeft = 0;
            _style = ScrollingStyle.PageByPage;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            if (VerifyBeforeScrolling() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Doing PageRight and verifying");
            scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(PageRight), null);
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            Thread.Sleep(c_wait);

            if (VerifyingPixelColor(chkPoints, _scvBground) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ScrollViewer_PageLeftTest15()
        {
            MFTestResults testResult = MFTestResults.Pass;
            _canWidth = _scvWidth * 2;
            _canHeight = _scvHeight;
            _rectW = _scvWidth;
            _rectH = _scvHeight;
            cTop = 0;
            cLeft = 0;
            _style = ScrollingStyle.PageByPage;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);

            if (VerifyBeforeScrolling() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Inorder to verify PageLeft, 1st does a PageRight then PageLeft");
            Log.Comment("Doing PageRight and verifying");
            scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(PageRight), null);
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            Thread.Sleep(c_wait);

            if (VerifyingPixelColor(chkPoints, _scvBground) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Doing PageLeft and verifying");
            scv.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(PageLeft), null);
            Thread.Sleep(c_wait);

            if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color after scrolling");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        object PageDown(object obj)
        {
            scv.PageDown();
            return null;
        }

        object PageUp(object obj)
        {
            scv.PageUp();
            return null;
        }

        object PageRight(object obj)
        {
            scv.PageRight();
            return null;
        }

        object PageLeft(object obj)
        {
            scv.PageLeft();
            return null;
        }

        ScrollViewer scv = null;
        Rectangle _rect = null;
        Color _color = Colors.Blue, _scvBground = (Color)0x00ffff;
        static int _scvWidth = (3 * _width) / 4, _scvHeight = (3 * _height) / 4;
        static int _canWidth = _width, _canHeight = _height;
        static int _rectW = (_width / 2), _rectH = (_height / 2);
        int cTop = (_scvHeight - _rectH) / 2, cLeft = (_scvWidth - _rectW) / 2;
        int _getExtentWidth, _getExtentHeight;
        int _lineWidth, _lineHeight, _getLineHeight, _getLineWidth;
        int _hOffset, _vOffset, _getHOffset, _getVOffset;
        int scrollCount = 1;
        bool _lineUp = false, _lineDown = false, _lineLeft = false, _lineRight = false;
        ScrollingStyle _style = ScrollingStyle.LineByLine;

        object ScrollViewer_UpdateWindow(object obj)
        {
            scv = new ScrollViewer();
            scv.Width = _scvWidth;
            scv.Height = _scvHeight;
            scv.Background = new SolidColorBrush(_scvBground);
            scv.LineHeight = _lineHeight;
            scv.LineWidth = _lineWidth;
            scv.HorizontalOffset = _hOffset;
            scv.VerticalOffset = _vOffset;
            scv.ScrollingStyle = _style;
            scv.VerticalAlignment = VerticalAlignment.Center;
            scv.HorizontalAlignment = HorizontalAlignment.Center;

            Canvas _canvas = new Canvas();
            _canvas.Width = _canWidth;
            _canvas.Height = _canHeight;
            _canvas.VerticalAlignment = VerticalAlignment.Top;
            _canvas.HorizontalAlignment = HorizontalAlignment.Left;

            //Adding child elt to Canvas           
            _rect = new Rectangle();
            _rect.Width = _rectW;
            _rect.Height = _rectH;
            _rect.Fill = new SolidColorBrush(_color);
            _canvas.Children.Add(_rect);
            Canvas.SetTop(_rect, cTop);
            Canvas.SetLeft(_rect, cLeft);

            //Adding the StackPanel to ScrollViewer
            scv.Child = _canvas;
            if (_lineDown)
            {
                for (int i = 0; i < scrollCount; i++)
                {
                    scv.LineDown();
                }
            }
            if (_lineUp)
            {
                for (int i = 0; i < scrollCount; i++)
                {
                    scv.LineUp();
                }
            }
            if (_lineRight)
            {
                for (int i = 0; i < scrollCount; i++)
                {
                    scv.LineRight();
                }
            }
            if (_lineLeft)
            {
                for (int i = 0; i < scrollCount; i++)
                {
                    scv.LineLeft();
                }
            }

            //Adding the ScrollViewer to the Window           
            mainWindow.Child = scv;

            _getHOffset = scv.HorizontalOffset;
            _getVOffset = scv.VerticalOffset;
            _getLineHeight = scv.LineHeight;
            _getLineWidth = scv.LineWidth;

            return null;
        }

        object GetSVExtentDimensions(object obj)
        {
            _getExtentHeight = scv.ExtentHeight;
            _getExtentWidth = scv.ExtentWidth;

            _autoEvent.Set();

            return null;
        }

        private MFTestResults NegativeOffsetTest(ref int offset, ref int getOffset)
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults tResult = MFTestResults.Pass;
            _hOffset = 0;
            _vOffset = 0;
            _canWidth = _width;
            _canHeight = _height;
            int[] offSet = new int[] { int.MinValue, -new Random().Next(100), 0 };

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);
            
            for (int i = 0; i < offSet.Length; i++)
            {
                Log.Comment("Offsetting the scrolled content by " + offSet[i]);
                offset = offSet[i];
                UpdateWindow();
                Log.Comment("Initializing Points inside scrolled content and verifying");
                Point[] chkPoints = GetRandomPoints_InRectangle(20, _rect.Width, _rect.Height, x, y);
                if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
                {
                    Log.Comment("Failure : The scrollviewed content has been offsetted");
                    tResult = MFTestResults.Fail;
                }
                if (getOffset != 0)
                {
                    Log.Comment("Expected offset '0' but got '" + _getHOffset + "'");
                    tResult = MFTestResults.Fail;
                }
            }

            return tResult;
        }

        private MFTestResults VerifyBeforeScrolling()
        {
            MFTestResults tResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            scrollCount = 1;
            _lineDown = false;
            _lineUp = false;
            _lineRight = false;
            _lineLeft = false;

            int x = _midX - _rectW / 2;
            int y = _midY - _rectH / 2;

            ConvertPts(ref x, ref y);
            
            Log.Comment("Drawing and Verifying pixel colors before scrolling");
            UpdateWindow();
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, y);
            if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel color before scrolling");
                tResult = MFTestResults.Fail;
            }

            return tResult;
        }

        void UpdateWindow()
        {
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(ScrollViewer_UpdateWindow), null);

            _autoEvent.WaitOne();

            Thread.Sleep(5);
        }
    }
}
