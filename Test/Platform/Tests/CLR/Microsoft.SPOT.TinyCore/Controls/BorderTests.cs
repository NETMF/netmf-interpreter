////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class BorderTests : Master_Controls, IMFTestInterface
    {
        private Random _random;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Border Tests");
            _random = new Random();
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults Border_BorderThicknessTest1()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _uniformThickness = true;
            _borderBrush = new SolidColorBrush(Colors.Green);
            _background = new SolidColorBrush(Colors.Blue);
            _thickness = _random.Next(System.Math.Min(_wd / 2, _ht / 2) - 2) + 1;
            UpdateWindow(); 
            Log.Comment("Gettig Border thickness and verifying");
            if ((l2 != _thickness) || (t2 != _thickness) || (r2 != _thickness) || (b2 != _thickness))
            {
                Log.Comment("Failure in Getting Border thickness");
                Log.Comment("Expected l2 = t2 = r2 = b2 = '" + _thickness + "' but got l2 = '" +
                    l2 + "' t2 = '" + t2 + "' r2 = '" + r2 + "' b2 = '" + b2 + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Initializing random points outside, on and inside of Border");
            Log.Comment("and Verifying the pixel colors");

            if (TestBorder(0, 0, xStart, yStart, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors outside Border");
                testResult = MFTestResults.Fail;
            }
            if (TestBorder(xStart, yStart, _thickness, _thickness, _borderBrush.Color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors on the Border");
                testResult = MFTestResults.Fail;
            }
            if (TestBorder(xStart + _thickness, yStart + _thickness, _wd - (2 * _thickness),
                _ht - (2 * _thickness), _background.Color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors inside Border");
                testResult = MFTestResults.Fail;
            }
            _uniformThickness = false;

            return testResult;
        }

        public MFTestResults Border_BorderThickness_BoundaryTest2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _uniformThickness = true;
            _borderBrush = new SolidColorBrush(Colors.Green);
            _background = new SolidColorBrush(Colors.Blue);
            Log.Comment("Varying the Border thickness b/n boundaries and Verifying");
            int[] tkness = new int[] { 0, int.MaxValue };
            for (int i = 0; i < tkness.Length; i++)
            {
                Log.Comment("Setting Border Thickness = '" + tkness[i] + "'");
                _thickness = tkness[i];
                UpdateWindow(); 
                Log.Comment("Gettig Border thickness and verifying");
                if ((l2 != _thickness) || (t2 != _thickness) || (r2 != _thickness) || (b2 != _thickness))
                {
                    Log.Comment("Failure in Getting Border thickness");
                    Log.Comment("Expected l2 = t2 = r2 = b2 = '" + _thickness + "' but got l2 = '" +
                        l2 + "' t2 = '" + t2 + "' r2 = '" + r2 + "' b2 = '" + b2 + "'");
                    testResult = MFTestResults.Fail;
                }
                if (TestBorder(0, 0, xStart, yStart, Color.White) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying pixel colors outside Border");
                    testResult = MFTestResults.Fail;
                }
                if (_thickness >= _wd)
                {
                    _thickness = 0;
                    _background.Color = _borderBrush.Color;
                }
                if (TestBorder(xStart, yStart, _wd, _ht, _background.Color) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying pixel colors inside Border");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        public MFTestResults Border_NonUniform_BorderThicknessTest3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _uniformThickness = false;
            _borderBrush = new SolidColorBrush(Colors.Red);
            _background = new SolidColorBrush(Colors.Blue);
            Log.Comment("Setting the 4 border edges to different thickness");
            l1 = _random.Next((_wd - 2) / 2) + 1;
            t1 = _random.Next((_ht - 2) / 2) + 1;
            r1 = _random.Next((_wd - 2) / 2) + 1;
            b1 = _random.Next((_ht - 2) / 2) + 1;
            UpdateWindow(); 
            if (TestBorder(0, 0, xStart, yStart, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors outside Border");
                testResult = MFTestResults.Fail;
            }
            if (TestBorder(xStart, yStart, l1, t1, _borderBrush.Color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors on the Border");
                testResult = MFTestResults.Fail;
            }
            if (TestBorder(xStart + l1, yStart + t1, _wd - (l1 + r1),
                _ht - (t1 + b1), _background.Color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors inside Border");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        public MFTestResults Border_NonUniform_BorderThickness_BoundaryTest4()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _uniformThickness = false;
            _borderBrush = new SolidColorBrush(Colors.Red);
            _background = new SolidColorBrush(Colors.Blue);
            Log.Comment("Varying the Border thickness b/n boundaries and Verifying");
            int[] tkness = new int[] { 0, int.MaxValue };
            for (int i = 0; i < tkness.Length; i++)
            {
                Log.Comment("Setting Border Thickness = '" + tkness[i] + "'");
                l1 = tkness[i];
                t1 = tkness[i];
                r1 = tkness[i];
                b1 = tkness[i];
                UpdateWindow(); 
                Log.Comment("Gettig Border thickness and verifying");
                if ((l2 != l1) || (t2 != t1) || (r2 != r1) || (b2 != b1))
                {
                    Log.Comment("Failure in Getting Border thickness");
                    Log.Comment("Expected l2 = '" + l1 + "' but got'" + l2 + "'");
                    Log.Comment("Expected t2 = '" + t1 + "' but got'" + t2 + "'");
                    Log.Comment("Expected r2 = '" + r1 + "' but got'" + r2 + "'");
                    Log.Comment("Expected b2 = '" + b1 + "' but got'" + b2 + "'");
                    testResult = MFTestResults.Fail;
                }
                if (TestBorder(0, 0, xStart, yStart, Color.White) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying pixel colors outside Border");
                    testResult = MFTestResults.Fail;
                }
                if ((l1 >= _wd) || (r1 >= _wd) || (t1 >= _ht) || (b1 >= _ht))
                {
                    _thickness = 0;
                    _background.Color = _borderBrush.Color;
                }
                if (TestBorder(xStart, yStart, _wd, _ht, _background.Color) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying pixel colors inside Border");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        public MFTestResults Border_BorderBrush_NullTest5()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _uniformThickness = true;
            _borderBrush = null;
            _background = new SolidColorBrush(Colors.Blue);
            _thickness = _random.Next(System.Math.Min(_wd / 2, _ht / 2) - 2) + 1;
            UpdateWindow(); 
            Log.Comment("Gettig Border thickness and verifying");
            if ((l2 != _thickness) || (t2 != _thickness) || (r2 != _thickness) || (b2 != _thickness))
            {
                Log.Comment("Failure in Getting Border thickness");
                Log.Comment("Expected l2 = t2 = r2 = b2 = '" + _thickness + "' but got l2 = '" +
                    l2 + "' t2 = '" + t2 + "' r2 = '" + r2 + "' b2 = '" + b2 + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Initializing random points outside, on and inside of Border");
            Log.Comment("and Verifying the pixel colors");

            if (TestBorder(0, 0, xStart + _thickness, yStart + _thickness, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors outside and on the Border");
                testResult = MFTestResults.Fail;
            }
            if (TestBorder(xStart + _thickness, yStart + _thickness, _wd - (2 * _thickness),
                _ht - (2 * _thickness), _background.Color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors inside Border");
                testResult = MFTestResults.Fail;
            }
            _uniformThickness = false;

            return testResult;
        }

        public MFTestResults Border_Background_NullTest6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Setting Border.Background to null");
            _uniformThickness = true;
            _borderBrush = new SolidColorBrush(Colors.Green);
            _background = null;
            _thickness = _random.Next(System.Math.Min(_wd / 2, _ht / 2) - 2) + 1;

            UpdateWindow();

            Log.Comment("Gettig Border thickness and verifying");
            if ((l2 != _thickness) || (t2 != _thickness) || (r2 != _thickness) || (b2 != _thickness))
            {
                Log.Comment("Failure in Getting Border thickness");
                Log.Comment("Expected l2 = t2 = r2 = b2 = '" + _thickness + "' but got l2 = '" +
                    l2 + "' t2 = '" + t2 + "' r2 = '" + r2 + "' b2 = '" + b2 + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Initializing random points outside, on and inside of Border");
            Log.Comment("and Verifying the pixel colors");


            if (TestBorder(0, 0, xStart, yStart, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors outside Border");
                testResult = MFTestResults.Fail;
            }
            if (TestBorder(xStart, yStart, _thickness, _thickness, _borderBrush.Color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors on the Border");
                testResult = MFTestResults.Fail;
            }
            if (TestBorder(xStart + _thickness, yStart + _thickness, _wd - (2 * _thickness),
                _ht - (2 * _thickness), _borderBrush.Color) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying pixel colors on the Border.Backgroud");
                testResult = MFTestResults.Fail;
            }
            _uniformThickness = false;

            return testResult;
        }

        public MFTestResults Border_UniformBorderThickness_ArgumentExceptionTest7()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _uniformThickness = true;
            _borderBrush = new SolidColorBrush(Colors.Red);
            _background = new SolidColorBrush(Colors.Blue);
            Log.Comment("Setting Border thickness to negative and verifying ArgumentException");
            _argumentException = false;
            _thickness = -_random.Next(System.Math.Min(_wd / 2, _ht / 2) - 2) + 1;
            
            UpdateWindow();

            m_evtException.WaitOne(1000, true);

            if (!_argumentException)
            {
                Log.Comment("Failure to throw exception for setting border thickness to a negative value");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        public MFTestResults Border_NonUniformBorderThickness_ArgumentExceptionTest8()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _borderBrush = new SolidColorBrush(Colors.Red);
            _background = new SolidColorBrush(Colors.Blue);
            
            Log.Comment("Setting the 4 border edges to different values and verifying ArgumentException is thrown");

            _uniformThickness = false;
            _argumentException = false;
            l1 = -_random.Next(_wd / 2);
            t1 = -_random.Next(_ht / 2);
            r1 = -_random.Next(_wd / 2);
            b1 = -_random.Next(_ht / 2);

            UpdateWindow();
            
            m_evtException.WaitOne(1000, true);

            if (!_argumentException)
            {
                Log.Comment("Failure to throw exception for setting border thicknesses to a negative value");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        private MFTestResults TestBorder(int sX, int sY, int w, int h, Color c)
        {
            int x, y;
            Point[] chkPoint = new Point[10];
            for (int i = 0; i < chkPoint.Length; i++)
            {
                x = _random.Next(w);
                y = _random.Next(h);
                chkPoint[i] = new Point(sX + x, sY + y);
            }
            return VerifyingPixelColor(chkPoint, c);
        }

        Border _border = null;
        SolidColorBrush _borderBrush = null;
        SolidColorBrush _background = null;
        bool _uniformThickness = false;
        static int _thickness, _wd = (3 * _width) / 4, _ht = (3 * _height) / 4;
        int xStart = (_width - _wd) / 2, yStart = (_height - _ht) / 2;
        int l1, t1, r1, b1, l2, t2, r2, b2;
        AutoResetEvent m_evtException = new AutoResetEvent(false);
        object Border_UpdateWindow(object obj)
        {
            _border = new Border();
            try
            {
                if (_uniformThickness)
                {
                    _border.SetBorderThickness(_thickness);
                }
                else
                {
                    _border.SetBorderThickness(l1, t1, r1, b1);
                }
            }
            catch (ArgumentException ex)
            {
                Log.Comment("Caught " + ex.Message + " when setting BorderThickness");
                _argumentException = true;
                m_evtException.Set();
            }
            _border.Width = _wd;
            _border.Height = _ht;
            _border.GetBorderThickness(out l2, out t2, out r2, out b2);
            _border.BorderBrush = _borderBrush;
            _border.Background = _background;
            mainWindow.Child = _border;
            return null;
        }

        void UpdateWindow()
        {
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(Border_UpdateWindow), null);

            _autoEvent.WaitOne();

            Thread.Sleep(5);
        }
    }
}
