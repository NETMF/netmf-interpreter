////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Platform.Test;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class StackPanelTests : Master_Controls, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for StackPanel Tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults StackPanel_DefaultOrientationTest1()
        {
            Log.Comment("Adding Children to a StackPanel and verifying the default Orienttion is Vertical");
            _defaultConstructor = true;
            return VerticalOrientation_Test();
        }

        [TestMethod]
        public MFTestResults StackPanel_DefaultConstructor_HorizontalOrientationTest2()
        {
            Log.Comment("Setting StackPanel Orientation to Horizontal and Verifying");
            _defaultConstructor = true;
            _oHorizontal = true;

            return HorizontalOrientation_Test();
        }

        [TestMethod]
        public MFTestResults StackPanel_DefaultConstructor_VerticalOrientationTest3()
        {
            Log.Comment("Setting StackPanel Orientation to Vertical and Verifying");
            _defaultConstructor = true;
            _oVertical = true;

            return VerticalOrientation_Test();
        }

        [TestMethod]
        public MFTestResults StackPanel_NonDefaultConstructor_HorizontalOrientationTest4()
        {
            Log.Comment("Setting StackPanel Orientation to Horizontal and Verifying");
            _defaultConstructor = false; ;
            _setOrientation = Orientation.Horizontal;

            return HorizontalOrientation_Test();
        }

        [TestMethod]
        public MFTestResults StackPanel_NonDefaultConstructor_VerticalOrientationTest3()
        {
            Log.Comment("Setting StackPanel Orientation to Vertical and Verifying");
            _defaultConstructor = false;
            _setOrientation = Orientation.Vertical;

            return VerticalOrientation_Test();
        }

        StackPanel stp = null;
        static Color[] _colors = new Color[] { Colors.Black, (Color)0x00ffff, Colors.Red, Colors.Blue, Colors.Green };
        int _rectW = _width / _colors.Length, _rectH = _height / _colors.Length;
        Orientation _getOrientation, _setOrientation;
        bool _oVertical = false, _oHorizontal = false, _defaultConstructor = false;
        object StackPanel_UpdateWindow(object obj)
        {
            if (_defaultConstructor)
            {
                stp = new StackPanel();
            }
            else
            {
                stp = new StackPanel(_setOrientation);
            }
            Rectangle[] _rects = new Rectangle[_colors.Length];
            int len = _rects.Length;
            for (int i = 0; i < len; i++)
            {
                _rects[i] = new Rectangle();
                _rects[i].Width = _width / len;
                _rects[i].Height = _height / len;
                _rects[i].Fill = new SolidColorBrush(_colors[i]);
            }
            for (int i = 0; i < _rects.Length; i++)
            {
                stp.Children.Add(_rects[i]);
            }
            if (_oHorizontal)
            {
                stp.Orientation = Orientation.Horizontal;
            }
            if (_oVertical)
            {
                stp.Orientation = Orientation.Vertical;
            }
            _getOrientation = stp.Orientation;
            mainWindow.Child = stp;

            return null;
        }

        private MFTestResults HorizontalOrientation_Test()
        {
            MFTestResults tResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            UpdateWindow();
            Log.Comment("Verifying StackPanel's children are Oriented Horizontally");
            Point[] chkPoints = null;
            int tempX = 0;
            int y = _midY - (_rectH / 2);
            for (int i = 0; i < _colors.Length; i++)
            {
                chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, tempX, y);
                tempX += _rectW;
                if (VerifyingPixelColor(chkPoints, _colors[i]) != MFTestResults.Pass)
                {
                    Log.Comment("Failure Verifying Rectangle '" + i + "' is Oriented Horizontally");
                    tResult = MFTestResults.Fail;
                }
            }
            Log.Comment("Getting the StackPanel's Orientation and Verifying it's Horizontal");
            if (_getOrientation != Orientation.Horizontal)
            {
                Log.Comment("Expected Orientation Horizontal = '" +
                    Orientation.Vertical + "' but got '" + _getOrientation + "'");
                tResult = MFTestResults.Fail;
            }
            _oHorizontal = false;

            return tResult;
        }

        private MFTestResults VerticalOrientation_Test()
        {
            MFTestResults tResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            UpdateWindow();
            Log.Comment("Verifying StackPanel's children are Oriented Vertically");
            Point[] chkPoints = null;
            int tempY = 0;
            int x = _midX - (_rectW / 2);
            for (int i = 0; i < _colors.Length; i++)
            {
                chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, x, tempY);
                tempY += _rectH;
                if (VerifyingPixelColor(chkPoints, _colors[i]) != MFTestResults.Pass)
                {
                    Log.Comment("Failure : Verifying Rectangle '" + i + "' is Oriented Vertically");
                    tResult = MFTestResults.Fail;
                }
            }
            Log.Comment("Getting the StackPanel's Orientation and Verifying It's Vertical");
            if (_getOrientation != Orientation.Vertical)
            {
                Log.Comment("Expected Orientation Vertical = '" +
                    Orientation.Vertical + "' but got '" + _getOrientation + "'");
                tResult = MFTestResults.Fail;
            }
            _oVertical = false;

            return tResult;
        }

        void UpdateWindow()
        {
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(StackPanel_UpdateWindow), null);
            
            _autoEvent.WaitOne();

            Thread.Sleep(5);
        }
    }
}
