////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Platform.Tests
{
    public class LineTests : Master_Shapes, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Line Tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults LineTest_Stroke1()
        {
            ///<summary>
            ///Make sure the panel is cleared
            ///Create a Line and set the Stroke with a unique color and
            ///verify Stroke by checking the pixel color along the line 
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("This is a BASIC test, all tests depend on this as ");
            Log.Comment("it verifies The pixel color at drawing poins are as set");
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Setting the Pen color to Green");
            _pen.Color = Colors.Green;

            Log.Comment("Initializing points of pixel color verification");
            Point[] chkPoint = new Point[10];
            int xStep = midX / 10, yStep = midY / 10;
            int temp1 = midX - (midX / 2) + (xStep / 2), temp2 = midY - (midY / 2) + (yStep / 2);
            for (int i = 0; i < 10; i++)
            {
                chkPoint[i] = new Point(temp1, temp2);
                temp1 += xStep;
                temp2 += yStep;
            }

            try
            {
                Log.Comment("Invoking the Dispatcher on a panel and verifying ");
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawLine), new Point(midX, midY));
                autoEvent.WaitOne(); Thread.Sleep( 100 );
                
                testResult = VerifyingPixelColor(chkPoint, Colors.Green);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_DefaultConstructor2()
        {
            ///<summary>
            ///Create new Line() - default constructor
            ///verify the horizontal and vertical distances equal to 0 (zero).
            ///Also verify the pixel color at the center is as Set
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            _defConstructor = true;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Setting the Pen color to Red");
            _pen.Color = Colors.Red;
            try
            {
                Log.Comment("Inovking the Dispatcher on a panel");
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawLine), null);
                autoEvent.WaitOne(); Thread.Sleep( 100 );
                
                if ((_line.ActualWidth != 1) || (_line.ActualHeight != 1))
                {
                    Log.Comment("Expecting horizontal distance '1' got " + _line.ActualWidth.ToString());
                    Log.Comment("Expecting vertical distance '1' got " + _line.ActualHeight.ToString());
                    return MFTestResults.Fail;
                }

                Log.Comment("Verifying Pixel Colors along the line");
                int x = midX - (0 == (_width & 1) ? 1 : 0);
                int y = midY - (0 == (_height & 1) ? 1 : 0);
                testResult = VerifyingPixelColor(new Point[] { new Point(x, y) }, Colors.Red);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_DefaultConstructor3()
        {
            ///<summary>
            ///Create new Line() - default constructor
            ///Set the width and height to that of the LCD
            ///verify the line is drawn by getting the pixels along the line. 
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            _defConstructor = false;
            w = _width;
            h = _height;
            Point[] chkPoints = new Point[] { new Point(0, 0), 
                new Point(midX- 1, midY - 1), new Point(_width - 1, _height - 1) };

            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Setting the Pen color to Red");
            _pen.Color = Colors.Red;
            try
            {
                Log.Comment("Inovking the Dispatcher on a panel");
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawLine), null);
                autoEvent.WaitOne(); Thread.Sleep( 100 );
                
                if ((_line.ActualWidth != _width) || (_line.ActualHeight != _height))
                {
                    Log.Comment("Expecting horizontal distance '" + max +
                        "' got " + _line.ActualWidth.ToString());
                    Log.Comment("Expecting vertical distance '" + min +
                        "' got " + _line.ActualHeight.ToString());
                    return MFTestResults.Fail;
                }

                Log.Comment("Verifying Pixel Colors along the line");
                testResult = VerifyingPixelColor(chkPoints, Colors.Red);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_TwoArgumentConstructor4()
        {
            ///<summary>
            ///Create new Line(0, 0) with Blue color
            ///verify the pixel color at the center is blue
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            int x = midX - (0 == (_width & 1) ? 1 : 0);
            int y = midY - (0 == (_height & 1) ? 1 : 0);

            Point[] chkPoints = new Point[] { new Point(x, y) };
            try
            {
                Log.Comment("Drawing new Line(0, 0) and Verifying");
                testResult = LineTest(new Point(0, 0), chkPoints, Colors.Blue);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_TwoArgumentConstructor5()
        {
            ///<summary>
            ///Create new Line(LCDWidth, LCDHeight) with Green Color
            ///verify the pixels along the line are Green
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Initializing different points for verifying pixel color");
            Point[] chkPoints = new Point[10];
            int xStep = _width / 10, yStep = _height / 10;
            int temp1 = (xStep / 2), temp2 = (yStep / 2);
            for (int i = 0; i < 10; i++)
            {
                chkPoints[i] = new Point(temp1, temp2);
                temp1 += xStep;
                temp2 += yStep;
            }
            try
            {
                Log.Comment("Drawing new Line(LCDWidth, LCDHeight) and Verifying");
                testResult = LineTest(new Point(_width - 1, _height - 1), chkPoints, Colors.Green);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_TwoArgumentConstructor6()
        {
            ///<summary>
            ///Create new Line(w, h) with Blue Color, Random w & h
            ///verify the pixels along the line are Blue
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Random random = new Random();
            int w = random.Next(_width) + 1;
            int h = random.Next(_height) + 1;

            Log.Comment("Initializing different points for verifying pixel color");
            Point[] chkPoints = new Point[] { new Point(midX - 1, midY - 1) };
            try
            {
                Log.Comment("Inovking the Dispatcher on a panel and creating lines");
                testResult = LineTest(new Point(w, h), chkPoints, Colors.Blue);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_ArgumentException7()
        {
            Point[] pts = new Point[] { new Point(-5, -70), new Point(-10, (_width / 2)),
                new Point((_width / 2), -100)};
            Log.Comment("setting the Exception counter to zero");
            eCounter = 0;
            try
            {
                Log.Comment("Creating lines with -ve horizontal or vertical distance");
                for (int i = 0; i < pts.Length; i++)
                {
                    if (ClearingPanel() != MFTestResults.Pass)
                    {
                        return MFTestResults.Fail;
                    }
                    Log.Comment("Drawing new Line (" + pts[i].x.ToString() + ", " +
                        pts[i].y.ToString() + ") and Verifying");
                    Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                         new DispatcherOperationCallback(DrawLine), pts[i]);
                    autoEvent.WaitOne(); Thread.Sleep( 100 );
                    
                }
                Log.Comment("Preparing to set the Width and Height of Default Constructor");
                _defConstructor = false;
                Log.Comment("Creating new Line() and setting -ve Width or Height");
                for (int i = 0; i < pts.Length; i++)
                {
                    w = pts[i].x;
                    h = pts[i].y;
                    if (ClearingPanel() != MFTestResults.Pass)
                    {
                        return MFTestResults.Fail;
                    }
                    Log.Comment("Drawing new Line () and setting Width = " + w + " and Height = " + h);
                    Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                        new DispatcherOperationCallback(DrawLine), null);
                    autoEvent.WaitOne(); Thread.Sleep( 100 );
                    
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }
            Log.Comment("Verifying ArgumentException is thrown in all cases");
            return eCounter == (pts.Length * 2) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults LineTest_BoundaryTest8()
        {
            ///<summary>
            ///Create new Line(w, h) - where w, h can go beyond screen limits        
            ///verify the actual width or height is the Max. LCD width or height
            ///</summary>
            ///
            _defConstructor = true;
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Initializing line distances that go beyond screen Limits");
            Point[] pts = new Point[] { 
                new Point((_width / 2), 0), 
                new Point((_width + 100), (_height + 7)),
                new Point(_width, (_height / 2)), 
                new Point( _width + 100, _height + 400)};

            Point[] renderedPts = new Point[] {
                new Point((_width / 2) + 1, 1),
                new Point(_width, _height), 
                new Point(_width, (_height / 2)+1),
                new Point(_width, _height)};

            int x = _width / 2 - (0 == (_width & 1) ? 1 : 0);
            int y = _height / 2 - (0 == (_height & 1) ? 1 : 0);

            Point[] chkPoint = new Point[] { new Point(x, y) };
            try
            {
                Log.Comment("Inovking the Dispatcher on a panel and creating lines beyond screen boundaries");
                for (int i = 0; i < pts.Length; i++)
                {
                    if (ClearingPanel() != MFTestResults.Pass)
                    {
                        return MFTestResults.Fail;
                    }
                    Log.Comment("Setting the Pen color");
                    _pen.Color = colors[i];
                    Log.Comment("Drawing new Line (" + pts[i].x.ToString() + ", " +
                        pts[i].y.ToString() + ") and Verifying");
                    Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                         new DispatcherOperationCallback(DrawLine), pts[i]);
                    autoEvent.WaitOne(); Thread.Sleep( 100 );
                    
                    if ((_line.ActualWidth != renderedPts[i].x) || (_line.ActualHeight != renderedPts[i].y))
                    {
                        Log.Comment("Expecting Actual width '" + renderedPts[i].x.ToString() +
                            "' got '" + _line.ActualWidth.ToString() + "' ");
                        Log.Comment("Expecting Actual height '" + renderedPts[i].y.ToString() +
                           "' got '" + _line.ActualHeight.ToString() + "' ");
                        return MFTestResults.Fail;
                    }
                    Log.Comment("Verifying pixel color at the center, all lines pass through the center");
                    testResult = VerifyingPixelColor(chkPoint, colors[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_DirectionBottomToTop9()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point[] chkPoints = new Point[] { new Point(0, _height - 1), new Point(_width - 1, 0) };
            _bottomToTop = true;
            try
            {
                Log.Comment("Inovking the Dispatcher on a panel and creating lines");
                testResult = LineTest(new Point(_width - 1, _height - 1), chkPoints, Colors.Green);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_DirectionTopToBottom10()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point[] chkPoints = new Point[] { new Point(0, 0), new Point(_width - 1, _height - 1) };
            _bottomToTop = false;
            try
            {
                Log.Comment("Inovking the Dispatcher on a panel and creating lines");
                testResult = LineTest(new Point(_width - 1, _height - 1), chkPoints, Colors.Red);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults LineTest_LineThickness11()
        {
            Log.Comment("This currently fails, Change in Thickness of a Pen used to draw a line");
            Log.Comment("doesn't affect the thickness of the line, see 22234 for details");
            MFTestResults testResult = MFTestResults.Pass;
            Point[] chkPoints = new Point[thickness / 2];
            for (int i = 0; i < chkPoints.Length; i++)
            {
                chkPoints[i] = new Point(midX + i, midY - 1);
            }
            try
            {
                Log.Comment("Changing the Pen Color and Thickness");
                _pen = new Pen(Colors.Blue, thickness);
                Log.Comment("Invoking the Dispatcher on a panel and Drawing a Horizontal Line");
                testResult = LineTest(new Point(_width - 1, thickness), chkPoints, Colors.Blue);                
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        Line _line = null;
        bool _bottomToTop = false;
        object DrawLine(object arg)
        {
            Point pt = arg as Point;
            if (pt == null)
            {
                _line = new Line();
                if (!_defConstructor)
                {
                    try
                    {
                        _line.Width = w;
                        _line.Height = h;
                    }
                    catch (Exception e)
                    {
                        Log.Comment("Caught : " + e.Message +
                            " when setting line Width = " + w + " and Height =" + h);
                        eCounter++;
                    }
                }
            }
            else if ((pt.x < 0) || (pt.y < 0))
            {
                try
                {
                    _line = new Line(pt.x, pt.y);
                }
                catch (Exception e)
                {
                    Log.Comment("Caught : " + e.Message + " when creating new Line(" + pt.x + ", " + pt.y + ")");
                    eCounter++;
                }
            }
            else
            {
                _line = new Line(pt.x, pt.y);
                if (_bottomToTop)
                {
                    _line.Direction = Direction.BottomToTop;
                }
                else
                {
                    _line.Direction = Direction.TopToBottom;
                }
            }
            _line.Stroke = _pen;
            Master_Shapes._panel.Children.Add(_line);

            return null;
        }

        private MFTestResults LineTest(Point pt, Point[] chkPts, Color c)
        {
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Setting the Pen color");
            _pen.Color = c;
            Log.Comment("Drawing new Line (" + pt.x.ToString() + ", " + pt.y.ToString() + ") and Verifying");
            
            Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(DrawLine), pt);

            autoEvent.WaitOne(); Thread.Sleep( 100 );

            if ((_line.ActualWidth != pt.x + 1) || (_line.ActualHeight != pt.y + 1))
            {
                Log.Comment("Expecting horizontal distance '" + pt.x.ToString() +
                    "' got '" + _line.ActualWidth.ToString() + "' ");
                Log.Comment("Expecting vertical distance '" + pt.y.ToString() +
                    "' got '" + _line.ActualHeight.ToString() + "' ");
                return MFTestResults.Fail;
            }
            Log.Comment("Verifying pixel color at verification points");
            return VerifyingPixelColor(chkPts, c);
        }
    }
}
