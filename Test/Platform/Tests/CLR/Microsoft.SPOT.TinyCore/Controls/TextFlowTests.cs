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
    public class TextFlowTests : Master_Controls, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for TextFlow tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults TextFlow_TextRunTest1()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Adds a TextRun into a TextFlow, and Verifies the TextRun exists");
            _txtRun = true;
            tr1 = new TextRun("TextRun", _font, _color);
            UpdateWindow();
            if (!trc.Contains(tr1))
            {
                Log.Comment("TextFlow.TextRuns doesn't contain tr1");
                testResult = MFTestResults.Fail;
            }
            _txtRun = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults TextFlow_LineCountTest2()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("This test gives unpredictable result, skipping it for the moment");
            MFTestResults testResult = MFTestResults.Skip;
            count = 10;
            int _maxFontWidth = _font.AverageWidth;
            int _minCharsinaLine = _width / _maxFontWidth;
            int len = (11 * _minCharsinaLine) / 10;
            _longStr = MFUtilities.GetRandomString(len);
            _lineCount = true;
            UpdateWindow();
            txtFlow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(SetTopLine), null);
            if (txtFlow.LineCount != count + 2)
            {
                Log.Comment("Expectd Line count " + count + 2 + " but got " + txtFlow.LineCount);
                //testResult = MFTestResults.Fail;
            }
            _lineCount = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults TextFlow_ScrollingStyleTest3()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            _lineCount = true;
            ScrollingStyle[] _sStyles = new ScrollingStyle[] { ScrollingStyle.First,
                ScrollingStyle.Last, ScrollingStyle.LineByLine, ScrollingStyle.PageByPage};
            for (int i = 0; i < _sStyles.Length; i++)
            {
                _style = _sStyles[i];
                UpdateWindow();
                if (_getStyle != _sStyles[i])
                {
                    Log.Comment("Expected ScrollingStyle '" + _sStyles[i] +
                        "' but got '" + _getStyle + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults TextFlow_TopLineTest4()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            _lineCount = true;
            count = 16;
            _topLine = new Random().Next(count);
            UpdateWindow();
            txtFlow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
               new DispatcherOperationCallback(SetTopLine), null);
            if (txtFlow.TopLine != _topLine)
            {
                Log.Comment("Expected TopLine '" + _topLine + "' but got " + txtFlow.TopLine + "' ");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        TextFlow txtFlow = null;
        TextRun tr1 = null;
        TextRunCollection trc = null;
        ScrollingStyle _style, _getStyle;
        Color _color = Colors.Red;
        string _longStr;
        int count, _topLine;
        bool _txtRun = false, _lineCount = false;
        object TextFlow_UpdateWindow(object obj)
        {
            txtFlow = new TextFlow();
            if (_txtRun)
            {
                txtFlow.TextRuns.Add(tr1);
                txtFlow.TextRuns.Add("1st Text", _font, _color);
                txtFlow.TextRuns.Add(tr1);
            }

            if (_lineCount)
            {
                txtFlow.TextRuns.Add(_longStr, _font, _color);
                txtFlow.TextRuns.Add(TextRun.EndOfLine);
                for (int i = 0; i < count; i++)
                {
                    txtFlow.TextRuns.Add("Text " + i, _font, _color);
                    txtFlow.TextRuns.Add(TextRun.EndOfLine);
                }
            }

            trc = txtFlow.TextRuns;
            txtFlow.ScrollingStyle = _style;
            _getStyle = txtFlow.ScrollingStyle;
            mainWindow.Child = txtFlow;

            return obj;
        }

        object SetTopLine(object obj)
        {
            txtFlow.TopLine = _topLine;
            return null;
        }

        void UpdateWindow()
        {
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(TextFlow_UpdateWindow), null);

            _autoEvent.WaitOne();
        }
    }
}
