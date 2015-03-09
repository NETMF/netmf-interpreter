////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Platform.Test;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class PanelTests : Master_Controls, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Panel tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults Panel_ChildrenAddTest1()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Adding 3 child elts on to a Panel and verifying");

            MFTestResults testResult = MFTestResults.Pass;
            _color = Colors.Green;
            
            if ((MFTestResults)Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(ChildrenAddTest), null) != MFTestResults.Pass)
            {
                Log.Comment("Failure in Verifying Children Added to a Panel");
                testResult = MFTestResults.Fail;
            }

            _autoEvent.WaitOne();

            Log.Comment("Verifying the child elts are rendered on a Window");
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, m_x, m_y);
            if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Not all child elts are rendered on the Window");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Panel_ChildrenRemoveTest2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("This test is dependent on Panel_ChildrenAdd Test");
            if (Panel_ChildrenAddTest1() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _remove = true;
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(ChildrenAddTest), null);

            _autoEvent.WaitOne();
            
            Log.Comment("Verifying removed child is not rendered on Window");
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH, m_x, m_y);
            if (VerifyingPixelColor(chkPoints, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure : The child elt removed is  rendered on the Window");
                testResult = MFTestResults.Fail;
            }
            _remove = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Panel_ChildrenClearTest3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("This test is dependent on Panel_ChildrenAdd Test");
            if (Panel_ChildrenAddTest1() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _clear = true;
            if ((MFTestResults)Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(ChildrenAddTest), null) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Panel.Children.Count should be 0 after clearing the panel");
                testResult = MFTestResults.Fail;
            }
            _autoEvent.WaitOne();

            Log.Comment("Verifying nothing is rendered after clearing the Panel");
            Point[] chkPoints = GetRandomPoints_InRectangle(40, _width, _height, 0, 0);
            if (VerifyingPixelColor(chkPoints, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Not all child elts are cleared from the Panel");
                testResult = MFTestResults.Fail;
            }
            _clear = false;

            return testResult;
        }

        Color _color;
        readonly int _rectW = _midX, _rectH = _midY;
        bool _remove = false, _clear = false;
        readonly int m_x = _midX / 2;
        readonly int m_y = _midY / 2;


        object ChildrenAddTest(object obj)
        {
            MFTestResults tResult = MFTestResults.Pass;

            Panel _panel = new Panel();
            Text txt1 = new Text(_font, "1st Text");
            txt1.HorizontalAlignment = HorizontalAlignment.Right;
            _panel.Children.Add(txt1);
            _panel.Children.Add(new Text(_font, "2nd Text"));
            Rectangle _rect = new Rectangle();
            _rect.Width = _rectW;
            _rect.Height = _rectH;
            _rect.Fill = new SolidColorBrush(_color);
            _panel.Children.Add(_rect);

            mainWindow.Child = _panel;

            if (_panel.Children[0] != txt1)
            {
                Log.Comment("Failure : _panel.Children[0] != txt1 ");
                tResult = MFTestResults.Fail;
            }
            if ((_panel.Children[1] as Text).TextContent != "2nd Text")
            {
                Log.Comment("Expected (_panel.Children[1] as Text).TextContent" +
                    " '2nd Text' but got '" + (_panel.Children[1] as Text).TextContent + "'");
                tResult = MFTestResults.Fail;
            }
            if (_panel.Children[2] != _rect)
            {
                Log.Comment("Failure : _panel.Children[2] != _rect ");
                tResult = MFTestResults.Fail;
            }
            if (_remove)
            {
                _panel.Children.Remove(_rect);
            }
            if (_clear)
            {
                _panel.Children.Clear();
                if (_panel.Children.Count != 0)
                {
                    Log.Comment("After Clearing the panel expected children count = '0' but got '" +
                        _panel.Children.Count + "'");
                    tResult = MFTestResults.Fail;
                }
            }
            return tResult;
        }
    }
}
