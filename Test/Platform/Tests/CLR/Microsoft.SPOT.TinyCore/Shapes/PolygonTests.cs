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
    public class PolygonTests : Master_Shapes, IMFTestInterface
    {
        Pen _polygonPen = null;
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Polygon Tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults PolygonTest_DefaultConstructor1()
        {
            Log.Comment("Initalizes Polygon with a  default constructor");

            _brush = new SolidColorBrush(Colors.Green);

            return PolygonConstructorTest();
        }

        [TestMethod]
        public MFTestResults PolygonTest_NonDefaultConstructor2()
        {
            Log.Comment("Initalizes Polygon with a non - default constructor");

            _brush = new SolidColorBrush(Colors.Green);

            _default = false;
            return PolygonConstructorTest();
        }

        [TestMethod]
        public MFTestResults PolygonTest_ArgumentException3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            _default = true;

            eCounter = 0;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            int[] pts = null;
            try
            {
                Log.Comment("Invoking the Dispatcher on a panel and verifying ");
                Log.Comment("Drawing a polygon with int[] = null ");
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawPolygon), pts);
                autoEvent.WaitOne(); Thread.Sleep( 1000 );
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }
            if (eCounter != 1)
            {
                Log.Comment("Expected ArgumentException when setting _poygon.Points = null");
                testResult = MFTestResults.Fail;
            }


            Log.Comment("Creating int[] with length zero");
            pts = new int[] { };
            eCounter = 0;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                Log.Comment("Invoking the Dispatcher on a panel and verifying ");
                Log.Comment("Drawing a polygon with int[] = null ");
                Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawPolygon), pts);
                autoEvent.WaitOne(); Thread.Sleep( 1000 );
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }
            if (eCounter != 1)
            {
                Log.Comment("Expected ArgumentException when setting _poygon.Points = int[] { } (zero length)");
                testResult = MFTestResults.Fail;
            }

            eCounter = 0;


            return testResult;
        }

        [TestMethod]
        public MFTestResults PolygonTest_PolygonThickness4()
        {
            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                if (ClearingPanel() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Setting the Pen color and thickness");
                ushort polyThickness = 6;
                _polygonPen = new Pen(Colors.Blue, polyThickness);

                Log.Comment("Draw a Regular Octagon");
                DrawRegularOctagon();

                autoEvent.WaitOne(); Thread.Sleep( 2000 );

                int len = min / 6 + (int)(((double)min / 3) / sqrt2);
                int k = 0;
                Point[] checkPts = new Point[2 * polyThickness];
                Log.Comment("Initializing Point to verify the pixel colors");
                for (int i = 0; i < polyThickness / 2; i++)
                {
                    checkPts[k++] = new Point(midX - len + i, midY);
                    checkPts[k++] = new Point(midX, midY - len + i);
                    checkPts[k++] = new Point(midX + len - i, midY);
                    checkPts[k++] = new Point(midX, midY + len - i);
                }
                Log.Comment("Veifying The pixel colors through the Rectangle thickness");
                if (VerifyingPixelColor(checkPts, Colors.Blue) != MFTestResults.Pass)
                {
                    testResult = MFTestResults.Fail;
                }

                _polygonPen = null;
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults PolygonTest_PolygonFill5()
        {
            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                if (ClearingPanel() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Setting the Pen and Brush");
                _pen = new Pen(Colors.Red, 1);
                _brush = new SolidColorBrush(Colors.Green);

                Log.Comment("Draw a Regular Octagon");
                DrawRegularOctagon();

                autoEvent.WaitOne(); Thread.Sleep( 1000 );

                Log.Comment("Initializing 40 Random Points inside the Horizontal rectangle in the Octagon");
                int len = min / 6 + (int)(((double)min / 3) / sqrt2);
                int w1 = 2 * (len-1), h1 = (min -1) / 3;
                Point[] chkPoints1 = GetRandomPoints_InRectangle(40, w1, h1, midX - w1 / 2, midY - h1 / 2);
                Log.Comment("Veifying The pixel colors inside the Horizontal rectangle in the Octagon");
                MFTestResults tRes1 = VerifyingPixelColor(chkPoints1, Colors.Green);
                Log.Comment("Initializing 40 Random Points inside the Vertical Rectangle in the Octagon");
                int w2 = h1, h2 = w1;
                Point[] chkPoints2 = GetRandomPoints_InRectangle(40, w2, h2, midX - w2 / 2, midY - h2 / 2);
                Log.Comment("Veifying The pixel colors inside the Vertical rectangle in the Octagon");
                MFTestResults tRes2 = VerifyingPixelColor(chkPoints1, Colors.Green);
                if ((tRes1 != MFTestResults.Pass) && (tRes2 != MFTestResults.Pass))
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

        Polygon _polygon = null;
        bool _default = true;
        object DrawPolygon(object arg)
        {
            int[] pts = arg as int[];
            if (_default)
            {
                _polygon = new Polygon();
                try
                {
                    _polygon.Points = pts;
                }
                catch (ArgumentException ex)
                {
                    Log.Comment("Caught : " + ex.Message + " for Points length '" + (pts != null ? pts.Length.ToString() : "(null)") + "'");
                    eCounter++;
                }
            }
            else
            {
                _polygon = new Polygon(pts);
            }

            if (_polygonPen == null)
            {
                _polygon.Stroke = new Pen(_pen.Color, 1);
            }
            else
            {
                _polygon.Stroke = _polygonPen;
            }

            _polygon.Fill = _brush;

            Master_Shapes._panel.Children.Add(_polygon);
            return null;
        }

        private void DrawRegularOctagon()
        {
            int s = min / 3, s2 = s / 2;
            double h = (double)s / sqrt2;
            int len = s2 + (int)h;

            Log.Comment("Initializing Points for drawing a Regular Octagon");
            int[] pts = new int[] {
                        midX - s2 , midY - len,
                        midX + s2 , midY - len,
                        midX + len, midY - s2, 
                        midX + len, midY + s2,
                        midX + s2 , midY + len, 
                        midX - s2 , midY + len, 
                        midX - len, midY + s2, 
                        midX - len, midY - s2 };

            Log.Comment("Drawing a Regular Octagon with side length s = " + s.ToString());
            Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(DrawPolygon), pts);
        }

        private MFTestResults PolygonConstructorTest()
        {
            MFTestResults tResult = MFTestResults.Pass;
            Random random = new Random();

            for (int i = 2; i <= 10; i++)
            {
                if (ClearingPanel() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Initializing " + i.ToString() + " Points");
                int[] pts = new int[i * 2];
                Point[] chkPoints = new Point[i];
                
                for (int j = 0; j < pts.Length; j+=2)
                {
                    pts[j    ] = random.Next(_width);
                    pts[j + 1] = random.Next(_height);

                    chkPoints[j/2] = new Point(pts[j], pts[j+1]);
                }
                try
                {
                    Log.Comment("Invoking the Dispatcher on a panel and verifying ");
                    Log.Comment("Drawing a polygon with " + i.ToString() + " sides");

                    autoEvent.Reset();

                    Master_Shapes._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(DrawPolygon), pts);

                    autoEvent.WaitOne(); Thread.Sleep( i * 600 );
                    
                    if (_polygon.Points != pts)
                    {
                        Log.Comment("Failure in Getting the points");
                        return MFTestResults.Fail;
                    }
                    Log.Comment("Verifying the pixel colors at drawing Points");
                    tResult = VerifyingPixelColor(chkPoints, Colors.Red);
                }
                catch (Exception ex)
                {
                    Log.Comment("Caught : " + ex.Message);
                    return MFTestResults.Fail;
                }
            }
            return tResult;
        }
    }
}
