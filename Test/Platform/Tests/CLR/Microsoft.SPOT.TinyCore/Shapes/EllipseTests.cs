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
    public class EllipseTests : Master_Shapes, IMFTestInterface
    {
        Pen _ellipsePen = null;
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Ellipse Tests");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults EllipseTest_DefaultConstructor1()
        {
            ///<summary>
            ///Create new Ellipse() - default constructor
            ///Render the ellipse and 
            ///verify the rendered Width and Height are that of LCD width and LCD Height
            ///</summary>
            ///

            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _defConstructor = true;
            try
            {
                Log.Comment("Invoking the Dispatcher on the panel");
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(DrawEllipse), null);

                autoEvent.WaitOne(); Thread.Sleep( 100 );

                // the desktop would return 1 and 1 for width and height, we do differently
                if ((_ellipse.ActualWidth != 1) || (_ellipse.ActualHeight != 1))
                {
                    Log.Comment("Expecting rendered width '1' got " + _ellipse.ActualWidth.ToString());
                    Log.Comment("Expecting rendered height '1' got " + _ellipse.ActualHeight.ToString());
                    return MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }
            Point[] chkPoints = new Point[] { new Point(_width / 2 - 1, midY - _ellipse.ActualHeight / 2),             
                new Point(midX - _ellipse.ActualWidth/2, _height/2 -1)};
            Log.Comment("Verifying Pixel Colors on the ellipse");

            return VerifyingPixelColor(chkPoints, Colors.White);
        }

        [TestMethod]
        public MFTestResults EllipseTest_DefaultConstructor2()
        {
            ///<summary>
            ///Create new Ellipse() - default constructor
            ///Set the width and height to some +ve values
            ///verify the Ellipse is drawn by getting the pixels along the line. 
            ///</summary>
            ///
            _defConstructor = false;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            w = min / 2 + 1;
            h = min / 2 + 1;
            try
            {
                Log.Comment("Invoking the Dispatcher on the panel");
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(DrawEllipse), null);                 

                autoEvent.WaitOne(); Thread.Sleep( 100 );
                
                if ((_ellipse.ActualWidth != w) || (_ellipse.ActualHeight != h))
                {
                    Log.Comment("Expecting rendered width '" + w.ToString() + "' got " + _ellipse.ActualWidth.ToString());
                    Log.Comment("Expecting rendered height '" + h.ToString() + "' got " + _ellipse.ActualHeight.ToString());
                    return MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            int x = _width / 2 - (0 == (_width & 1) ? 1 : 0);
            int y = _height / 2 - (0 == (_height & 1) ? 1 : 0);

            Point[] chkPoints = new Point[] { 
                new Point(x, y - w / 2),
                new Point(x, y + w / 2),
                new Point(x - w / 2, y), 
                new Point(x + w / 2, y) };
            Log.Comment("Verifying Pixel Colors on the ellipse");

            return VerifyingPixelColor(chkPoints, Colors.Red);
        }

        [TestMethod]
        public MFTestResults EllipseTest_TwoArgumentConstructor3()
        {
            ///<summary>
            ///Create new Ellipse(0, 0)
            ///verify the width, and height equal 1
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                testResult = EllipseTest(new Point(0, 0), Colors.White);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults EllipseTest_TwoArgumentConstructor4()
        {
            ///<summary>
            ///Create new Ellipse(x, y), where
            ///1) x = xRadMax, y = yRadMax
            ///2) x = w, y = h where w and h are Random numbers b/n 0 and RadMax
            ///3) x = y, Circle
            ///4) x = y/2
            ///verify the Ellipse is drawn by getting the pixel colors of points on the ellipse 
            ///</summary>
            ///

            MFTestResults testResult = MFTestResults.Pass;
            Random random = new Random();

            int w = random.Next(_width / 2) + 1;
            int h = random.Next(_height / 2) + 1;

            Point[] pts = new Point[] {
                new Point((_width / 2) - 1, (_height / 2) - 1),
                new Point(w, h),
                new Point(w-1, h-1),
                new Point((min / 2) - 1, (min / 2) - 1), 
                new Point((min / 4) - 1,(min / 2) - 1 )};
            try
            {
                for (int i = 0; i < pts.Length; i++)
                {
                    if (EllipseTest(pts[i], Colors.Green) != MFTestResults.Pass)
                    {
                        testResult = MFTestResults.Fail;
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
        public MFTestResults EllipseTest_ArgumentException5()
        {
            ///<summary>
            ///Create new Ellipse() - default constructor
            ///Set the width and height to some -ve values
            ///verify ArgumentException is thrown
            ///</summary>
            ///

            _defConstructor = false;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("setting the Exception counter to zero");
            eCounter = 0;
            Random random = new Random();
            Point[] pts = new Point[]{
                new Point(-(random.Next(_width) + 1), random.Next(_width) + 1),
                new Point(random.Next(_width)+1     , -(random.Next(_width)+1)),
                new Point(-(random.Next(_width)+1)  , -(random.Next(_width)+1))};

            try
            {
                Log.Comment("Invoking the Dispatcher on the panel");
                for (int i = 0; i < pts.Length; i++)
                {
                    w = pts[i].x;
                    h = pts[i].y;
                    Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(DrawEllipse), null);
                    
                    autoEvent.WaitOne(); Thread.Sleep( 100 );
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return eCounter == pts.Length ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults EllipseTest_ArgumentException6()
        {
            Point[] pts = new Point[] { new Point(-5, -70), new Point(-10, (_width / 2)),
                new Point((_width / 2), -100)};
            Log.Comment("setting the Exception counter to zero");
            eCounter = 0;

            try
            {
                Log.Comment("Creating Ellipses with -ve xRad or -ve yRad");
                for (int i = 0; i < pts.Length; i++)
                {
                    if (ClearingPanel() != MFTestResults.Pass)
                    {
                        return MFTestResults.Fail;
                    }
                    Log.Comment("Drawing new Ellipse (" + pts[i].x.ToString() + ", " + pts[i].y.ToString() + ") and Verifying");

                    Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(DrawEllipse), pts[i]);
                    
                    autoEvent.WaitOne(); Thread.Sleep( 100 );
                }
                Log.Comment("Preparing to set the Width and Height of Default Constructor");
                _defConstructor = false;
                Log.Comment("Creating new Ellipse() and setting -ve Width or Height");
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
                        new DispatcherOperationCallback(DrawEllipse), null);
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
        public MFTestResults EllipseTest_BoundaryTest7()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Initializing ellipses radiuses that go beyond screen Limits");

            int div4 = _width / 4 - (0 == (_width & 1) ? 1 : 0);
            int div2 = _width / 2 - (0 == (_width & 1) ? 1 : 0);

            int hdiv4 = _height / 4 - (0 == (_height & 1) ? 1 : 0);
            int hdiv2 = _height / 2 - (0 == (_height & 1) ? 1 : 0);

            Point[] pts = new Point[] {    
                new Point(div4         , _height + 7 ),
                new Point(_width + 100 , hdiv4       ),            
                new Point(div2   - 1   , _height+700 ),
                new Point(_width + 1000, hdiv2       )};

            Point[] bdry = new Point[] {
                new Point(div4 * 2 + 1, _height),
                new Point(_width      , hdiv4 * 2 + 1), 
                new Point(div2 * 2 -1 , _height),
                new Point(_width      , hdiv2 * 2 + 1)};

            Point[] chkPoints = null;
            try
            {
                Log.Comment("Drawing ellipses that go out beyond LCD boundary and verifying");
                for (int i = 0; i < pts.Length; i++)
                {
                    if (ClearingPanel() != MFTestResults.Pass)
                    {
                        return MFTestResults.Fail;
                    }
                    Log.Comment("Setting the Pen color");
                    _pen.Color = Colors.Red;
                    Log.Comment("Drawing new Ellipse(" + pts[i].x.ToString() + ", " +
                        pts[i].y.ToString() + ") and verifying");
                    Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                         new DispatcherOperationCallback(DrawEllipse), pts[i]);
                    autoEvent.WaitOne(); Thread.Sleep( 100 );
                    
                    if ((_ellipse.ActualWidth != bdry[i].x) || (_ellipse.ActualHeight != bdry[i].y))
                    {
                        Log.Comment("Expecting horizontal distance '" + bdry[i].x.ToString() + "' got '" +
                            _ellipse.ActualWidth.ToString() + "'");
                        Log.Comment("Expecting vertical distance '" + bdry[i].y.ToString() + "' got '" +
                            _ellipse.ActualHeight.ToString() + "'");
                        return MFTestResults.Fail;
                    }

                    int x = midX - (0 == (_width & 1) ? 1 : 0);
                    int y = midY - (0 == (_height & 1) ? 1 : 0);
                    
                    if (i % 2 == 0)
                    {
                        Log.Comment("Vertical Ellipse");
                        chkPoints = new Point[] { 
                            new Point(x - (_ellipse.ActualWidth/2), y), 
                            new Point(x + (_ellipse.ActualWidth/2), y) };
                    }
                    else
                    {
                        Log.Comment("Horizontal Ellipse");

                        chkPoints = new Point[] {  
                            new Point(x, y - (_ellipse.ActualHeight / 2)),
                            new Point(x, y + (_ellipse.ActualHeight / 2))};
                    }

                    Log.Comment("Verifying Pixel Colors on the ellipse");
                    testResult = VerifyingPixelColor(chkPoints, Colors.Red);
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
        public MFTestResults EllipseTest_EllipseThickness8()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int xRad = min / 3, yRad = min / 3;
            Point pt = new Point(xRad, yRad);
            Point l, t, r, b;
            Color lc, tc, rc, bc;

            try
            {
                if (ClearingPanel() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Setting the Pen color and thickness");

                ushort ellipseBorderThickness = 6;
                _ellipsePen = new Pen(Colors.Blue, ellipseBorderThickness);
                Log.Comment("Invoking the Dispatcher on a panel");
                Log.Comment("Drawing Ellipse with xRadius = " + pt.x.ToString() + " and yRadius = " + pt.y.ToString());
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawEllipse), new Point(pt.x, pt.y));
                autoEvent.WaitOne(); Thread.Sleep( 100 );
                

                _ellipsePen = null;

                int x1 = 0 , x2 = 0, y1 = 0, y2 = 0;
                int i = 0;

                for (i = 0; i < midX; i++)
                {
                    l = new Point(midX - i, midY);
                    lc = _panel._pBitmap.GetPixel(l.x, l.y);
                    if ((x1 == 0) && (lc == Colors.Blue))
                    {
                        x1 = midX - i;
                    }

                    l = new Point(midX + i, midY);
                    lc = _panel._pBitmap.GetPixel(l.x, l.y);
                    if ((x2 == 0) && (lc == Colors.Blue))
                    {
                        x2 = midX + i;
                    }
                }

                for (i = 0; i < midY; i++)
                {
                    l = new Point(midX, midY - i);
                    lc = _panel._pBitmap.GetPixel(l.x, l.y);
                    if ((y1 == 0) && (lc == Colors.Blue))
                    {
                        y1 = midY - i;
                    }

                    l = new Point(midX, midY + i);
                    lc = _panel._pBitmap.GetPixel(l.x, l.y);
                    if ((y2 == 0) && (lc == Colors.Blue))
                    {
                        y2 = midY + i;
                    }
                }

                Log.Comment("Veifying The pixel colors at points on the Ellipse");

                if ((x1 != 0) && (y1 != 0) && (x2 != 0) && (y2 != 0))
                {
                    for (i = 0; i < ellipseBorderThickness; i++)
                    {
                        l = new Point(x1 - i, midY);
                        t = new Point(midX, y1 - i);
                        r = new Point(x2 + i, midY);
                        b = new Point(midX, y2 + i);
                        lc = _panel._pBitmap.GetPixel(l.x, l.y);
                        tc = _panel._pBitmap.GetPixel(t.x, t.y);
                        rc = _panel._pBitmap.GetPixel(r.x, r.y);
                        bc = _panel._pBitmap.GetPixel(b.x, b.y);

                        if ((lc != Colors.Blue) || (tc != Colors.Blue) || (rc != Colors.Blue) || (bc != Colors.Blue))
                        {
                            Log.Comment("Expected Color : " + Colors.Blue.ToString() + " but got ");
                            Log.Comment(lc.ToString() + " at Left (" + l.x.ToString() + ", " + l.y.ToString() + ")");
                            Log.Comment(tc.ToString() + " at Top (" + t.x.ToString() + ", " + t.y.ToString() + ")");
                            Log.Comment(rc.ToString() + " at Right (" + r.x.ToString() + ", " + r.y.ToString() + ")");
                            Log.Comment(bc.ToString() + " at Bootom (" + b.x.ToString() + ", " + b.y.ToString() + ")");
                            testResult = MFTestResults.Fail;
                        }
                    }
                }
                else
                {
                    testResult = MFTestResults.Fail;
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
        public MFTestResults EllipseTest_EllipseFill9()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int xRad = min / 3;
            Point pt = new Point(xRad, xRad);

            try
            {
                if (ClearingPanel() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Setting the Pen and Brush");
                _pen = new Pen(Colors.Red, 1);
                _brush = new SolidColorBrush(Colors.Green);
                Log.Comment("Invoking the Dispatcher on a panel");
                Log.Comment("Drawing a Circle(Ellipse)with radius = " + pt.x.ToString() + " and yRadius = " + pt.y.ToString());
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawEllipse), new Point(pt.x, pt.y));
                autoEvent.WaitOne(); Thread.Sleep( 100 );
                
                Log.Comment("Initializing 40 random points inside the largest square inside the circle");
                int s = (int)(2 * xRad / sqrt2);

                int x = midX - s / 2;
                int y = midY - s / 2;

                Point[] chkPoints = GetRandomPoints_InRectangle(40, s-2, s-2, x+1, y+1);
                Log.Comment("Veifying The pixel colors inside the Circle");
                if (VerifyingPixelColor(chkPoints, Colors.Green) != MFTestResults.Pass)
                {
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        Ellipse _ellipse = null;
        object DrawEllipse(object arg)
        {
            Point pt = arg as Point;
            if (pt == null)
            {
                _ellipse = new Ellipse(0, 0);
                if (!_defConstructor)
                {
                    try
                    {
                        _ellipse.Width = w;
                        _ellipse.Height = h;
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Comment("Caught : " + ex.Message + " when setting w = '" + w + "' and h = '" + h + "'");
                        eCounter++;
                    }
                }
            }
            else if ((pt.x < 0) || (pt.y < 0))
            {
                try
                {
                    _ellipse = new Ellipse(pt.x, pt.y);
                }
                catch (ArgumentException e)
                {
                    Log.Comment("Caught : " + e.Message + " when new Ellipse(" + pt.x + ", " + pt.y + ")");
                    eCounter++;
                }
            }
            else
            {
                _ellipse = new Ellipse(pt.x, pt.y);
            }

            if (_ellipsePen == null)
            {
                _ellipse.Stroke = new Pen(_pen.Color, 1);
            }
            else
            {
                _ellipse.Stroke = _ellipsePen;
            }

            _ellipse.Fill = _brush;
            Master_Shapes._panel.Children.Add(_ellipse);
            return null;
        }

        private void ConvertPts(ref int x, ref int y)
        {
            if (1 == ((~_width ) & _ellipse.Width & 1)) x--;
            if (1 == ((~_height) & _ellipse.Width & 1)) y--;
        }

        private MFTestResults EllipseTest(Point pt, Color c)
        {
            if (ClearingPanel() != MFTestResults.Pass)
            {
                Log.Comment("The Panel is not Clear");
                return MFTestResults.Fail;
            }
            Log.Comment("Setting the Pen color");
            _pen.Color = c;
            Log.Comment("Drawing new Ellipse(" + pt.x.ToString() + ", " +
               pt.y.ToString() + ") and verifying");
            Log.Comment("Invoking the Dispatcher on the panel");
            Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                 new DispatcherOperationCallback(DrawEllipse), pt);
            autoEvent.WaitOne(); Thread.Sleep( 100 );
            
            if ((_ellipse.ActualWidth != (pt.x * 2) + 1) || (_ellipse.ActualHeight != (pt.y * 2) + 1))
            {
                Log.Comment("Expecting horizontal distance '" + pt.x.ToString() + "' got '" +
                    _ellipse.ActualWidth.ToString() + "' ");
                Log.Comment("Expecting vertical distance '" + pt.y.ToString() + "' got '" +
                   _ellipse.ActualHeight.ToString() + "' ");
                return MFTestResults.Fail;
            }

            int x = _width / 2;
            int y = _height / 2;

            ConvertPts(ref x, ref y);

            Point[] chkPoints = new Point[] { 
                new Point(x, y - (pt.y)),
                new Point(x, y + (pt.y)),
                new Point(x - (pt.x), y), 
                new Point(x + (pt.x), y) };

            Log.Comment("Verifying Pixel Colors");
            return VerifyingPixelColor(chkPoints, c);
        }
    }
}
