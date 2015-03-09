////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class RectangleTests : Master_Shapes, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Rectangle Tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults RectangleTest_1()
        {
            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                Log.Comment("Invoking the Dispatcher on a panel and verifying ");
                testResult = RectangleTest(new Point(midX, midY), Colors.Red);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults RectangleTest_2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int stepY = _height / 10;
            int stepX = _width / 10;
            int x = 0, y = 0;

            try
            {
                Log.Comment("Drawing Increasing Dimensions Rectangle");
                Log.Comment("Invoking the Dispatcher on a panel and verifying ");
                for (int i = 0; i < 10; i++)
                {
                    x += stepX;
                    y += stepY;
                    testResult = RectangleTest(new Point(x, y), colors[i]);
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }
            return testResult;
        }

        void ConvertPoints(ref int x, ref int y)
        {
            if (((~_width ) & _rectangle.Width  & 1) == 1) x--;
            if (((~_height) & _rectangle.Height & 1) == 1) y--;
        }

        [TestMethod]
        public MFTestResults RectangleTest_ArgumenException3()
        {
            Point[] pts = new Point[] { new Point(-5, -70), new Point(-(new Random().Next(1000)+1), midX),
                new Point(midX, -100), new Point(0, -100)};
            Log.Comment("setting the Exception counter to zero");
            eCounter = 0;

            try
            {
                Log.Comment("Creating new Rectangle() and setting -ve Width or Height");
                for (int i = 0; i < pts.Length; i++)
                {
                    if (ClearingPanel() != MFTestResults.Pass)
                    {
                        return MFTestResults.Fail;
                    }
                    Log.Comment("Drawing new Rectangle() and setting Width = " + w + " and Height = " + h);
                    Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                        new DispatcherOperationCallback(DrawRectangle), new Point(pts[i].x, pts[i].y));
                    autoEvent.WaitOne(); Thread.Sleep( 100 );
                    
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }
            Log.Comment("Verifying ArgumenException is thrown in all cases");
            return eCounter == pts.Length ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults RectangleTest_RectangleThickness4()
        {
            ushort thickness1 = 26;

            MFTestResults testResult = MFTestResults.Pass;
            int w = midX;
            int h = midY;
            Point pt = new Point(w, h);
            //Point l, t, r, b;
            //Color lc, tc, rc, bc;

            try
            {
                if (ClearingPanel() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Setting the Pen color and thickness");
                _pen = new Pen(Colors.Blue, thickness1);
                Log.Comment("Invoking the Dispatcher on a panel");
                Log.Comment("Drawing Rectangle with w = " + pt.x.ToString() + " and h = " + pt.y.ToString());
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), new Point(pt.x, pt.y));
                autoEvent.WaitOne(); Thread.Sleep( 100 );

                int midW = w / 2;
                int midH = h / 2;

                int x1 = midX - midW;
                int x2 = midX + midW - 1;
                int y1 = midY - midH;
                int y2 = midY + midH - 1;

                ConvertPoints(ref x1, ref y1);

                Log.Comment("Veifying The pixel colors through the Rectangle thickness");
                for (int i = 0; i < thickness1; i++)
                {
                    Point []pts = new Point[]{
                        new Point(x1 + i    , y1 + h / 2),
                        new Point(x1 + w / 2, y1 + i    ),
                        new Point(x2 - i    , y2 - h / 2),
                        new Point(x2 - w / 2, y2 - i    ),
                    };

                    if (MFTestResults.Fail == VerifyingPixelColor(pts, Colors.Blue))
                    {
                        return MFTestResults.Fail;
                    }
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
        public MFTestResults RectangleTest_RectangleFill5()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point pt = new Point(midX, midY);
            try
            {
                Log.Comment("Setting the Pen and Brush");
                _pen = new Pen(Colors.Red, 1);
                _brush = new SolidColorBrush(Colors.Green);
                Log.Comment("Invoking the Dispatcher on a panel and verifying ");
                Log.Comment("Drawing Rectangle with w = " + midX.ToString() + " and h = " + midX.ToString());
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), pt);
                autoEvent.WaitOne(); Thread.Sleep( 100 );
                
                Log.Comment("Initializing 40 Random Points that are inside the rectangle");

                int midW = pt.x / 2;
                int midH = pt.y / 2;

                int x = midX - midW + 1; // + 1 for the border
                int y = midY - midH + 1;

                ConvertPoints(ref x, ref y);

                Point[] chkPoints = GetRandomPoints_InRectangle(40, pt.x-2, pt.y-2, x, y);
                Log.Comment("Verifying the pixel colors inside the rectangle");
                testResult = VerifyingPixelColor(chkPoints, Colors.Green);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        Rectangle _rectangle = null;
        object DrawRectangle(object arg)
        {
            Point pt = arg as Point;
            _rectangle = new Rectangle();
            try
            {
                _rectangle.Width = pt.x;
                _rectangle.Height = pt.y;
            }
            catch (Exception e)
            {
                Log.Comment("Caught : " + e.Message + " when setting Width = " + pt.x + " and Height = " + pt.y);
                eCounter++;
            }
            _rectangle.Stroke = _pen;
            _rectangle.Fill = _brush;
            Master_Shapes._panel.Children.Add(_rectangle);
            return null;
        }

        private MFTestResults RectangleTest(Point pt, Color c)
        {
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Setting the Pen color");
            _pen.Color = c;
            Log.Comment("Drawing Rectangle with w = " + pt.x.ToString() + " and h = " + pt.y.ToString());
            Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(DrawRectangle), new Point(pt.x, pt.y));
            autoEvent.WaitOne(); Thread.Sleep( 100 );
            
            if ((_rectangle.ActualWidth != pt.x) || (_rectangle.ActualHeight != pt.y))
            {
                Log.Comment("Expecting width '" + pt.x.ToString() + "' got '" + _rectangle.ActualWidth.ToString() + "'");
                Log.Comment("Expecting height '" + pt.y.ToString() + "' got '" + _rectangle.ActualWidth.ToString() + "'");
                return MFTestResults.Fail;
            }

            int midW = _rectangle.Width / 2;
            int midH = _rectangle.Height / 2;

            int x1 = midX - midW;
            int x2 = midX + midW - 1;

            int y1 = midY - midH;
            int y2 = midY + midH - 1;

            ConvertPoints(ref x1, ref y1);

            int thick = _pen.Thickness - 1;

            Point[] chkPoints = new Point[] { 
                new Point(x1, midY),
                new Point(x1 + thick, midY),
                new Point(midX, y1),
                new Point(midX, y1 + thick),
                new Point(x2, midY),
                new Point(x2 - thick, midY),
                new Point(midX, y2),
                new Point(midX, y2 - thick + 1),
                };
            Log.Comment("Verifying Pixel Colors on the Rectangle");
            return VerifyingPixelColor(chkPoints, c);
        }
    }
}
