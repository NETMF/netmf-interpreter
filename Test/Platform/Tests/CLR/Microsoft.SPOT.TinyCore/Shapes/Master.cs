////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_Shapes
    {
        public static void Main()
        {

            Thread appThread = new Thread(new ThreadStart(new Master_Shapes().ApplicationThread));
            appThread.Start();

            //wait until UI Window is created
            autoEvent.WaitOne(); Thread.Sleep( 100 );

            string[] args = {"LineTests", "EllipseTests", 
                            "RectangleTests", "PolygonTests"};
            
            MFTestRunner runner = new MFTestRunner(args);

            Log.Comment("Aborting the Application Thread");
            try
            {
                appThread.Abort();
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message + " when aborting the application thread");
            }
        }

        #region NewWindowApplication
        /// <summary>
        /// Creates a New Window Application
        /// </summary>
        void ApplicationThread()
        {
            Application app = new Application();

            Window mainWindow = CreateWindow();

            // Start the application
            app.Run(mainWindow);
        }

        public Window CreateWindow()
        {
            // Create a window object and set its size to the
            // size of the display.
            mainWindow = new Window();
            _panel = new MyPanel();
            mainWindow.Height = SystemMetrics.ScreenHeight;
            mainWindow.Width = SystemMetrics.ScreenWidth;

            Text t = new Text(Resources.GetFont(Resources.FontResources.small), "Shape Tests !");
            t.VerticalAlignment = VerticalAlignment.Center;
            t.HorizontalAlignment = HorizontalAlignment.Center;
            _panel.Children.Add(t);
            mainWindow.Child = _panel;
            // Set the window visibility to visible.
            mainWindow.Visibility = Visibility.Visible;

            return mainWindow;
        }
        #endregion NewWindowApplication


        #region Variables
        /// <summary>
        /// Variables all Derived classes use
        /// </summary>
        protected static Window mainWindow = null;
        protected static MyPanel _panel = null;
        protected Pen _pen = new Pen(Colors.Red, 10);
        protected Brush _brush = null;
        protected Color[] colors = new Color[10]{Colors.Green, Colors.Blue, Colors.Gray, Colors.Red, Colors.Blue,
            Colors.Green, Colors.Gray, Colors.Red, Colors.Red, Colors.Red};

        protected bool _defConstructor = false;
        protected int w, h, eCounter;
        protected const int thickness = 16; // constatnt thickness for thickness tests
        protected double sqrt2 = 1.4142135623730950488016887242097; // We don't have System.Math.Sqrt(2.0)
        protected static AutoResetEvent autoEvent = new AutoResetEvent(false);
        protected static int _width = SystemMetrics.ScreenWidth;
        protected static int _height = SystemMetrics.ScreenHeight;
        protected static int midX = _width / 2;
        protected static int midY = _height / 2;
        protected static int min = System.Math.Min(_width, _height);
        protected static int max = System.Math.Max(_width, _height);       
        #endregion


        #region ClearPanel       
        /// <summary>
        /// Clears the Drawing Panel and 
        /// Verifies the Panel is clean
        /// </summary>
        /// <returns></returns>
        protected MFTestResults ClearingPanel()
        {
            Log.Comment("Clearing The Panel and Verifying");
            try
            {
                Master_Shapes.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                                        new DispatcherOperationCallback(ClearPanel), null);
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message + " when clearing the Panel");
                return MFTestResults.Fail;
            }
            Log.Comment("Initializing 100 random points and verifying the panel is clear");
            Point[] chkPoints = new Point[100];
            Random random = new Random();
            for (int i = 0; i < chkPoints.Length; i++)
            {
                chkPoints[i] = new Point(random.Next(_width), random.Next(_height));
            }

            return VerifyingPixelColor(chkPoints, Colors.White);
        }
        protected object ClearPanel(object arg)       
        {
            Master_Shapes._panel.Children.Clear();
            return null;
        }
        #endregion ClearPanel


        #region PixelColorVerification
        /// <summary>
        /// Gets the pixel color and verifies 
        /// </summary>      
        /// <returns></returns>
        protected MFTestResults VerifyingPixelColor(Point[] pts, Color _color)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                Color c = Master_Shapes._panel._pBitmap.GetPixel(pts[i].x, pts[i].y);

                if (c != _color)
                {
                    Thread.Sleep(1000);

                    c = Master_Shapes._panel._pBitmap.GetPixel(pts[i].x, pts[i].y);

                    if (c != _color)
                    {
                        Master_Shapes._panel._pBitmap.DrawEllipse(Colors.Purple, 1, pts[0].x, pts[0].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);
                        Master_Shapes._panel._pBitmap.DrawEllipse(Colors.Purple, 1, pts[1].x, pts[1].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);
                        Master_Shapes._panel._pBitmap.DrawEllipse(Colors.Purple, 1, pts[2].x, pts[2].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);
                        Master_Shapes._panel._pBitmap.DrawEllipse(Colors.Purple, 1, pts[3].x, pts[3].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);

                        Master_Shapes._panel._pBitmap.DrawEllipse(Colors.Red, 1, pts[i].x, pts[i].y, 3, 3, (Color)0, 0, 0, (Color)0, 0, 0, Bitmap.OpacityTransparent);
                        Master_Shapes._panel._pBitmap.Flush();

                        Log.Comment("Failure : Expected color '" + _color.ToString() + "' but got '" + c.ToString() + "' at (" + pts[i].x.ToString() + ", " + pts[i].y.ToString() + ")");
                        return MFTestResults.Fail;
                    }
                }
            }

            return MFTestResults.Pass;
        }
        #endregion PixelColorVerification


        #region PointClass
        /// <summary>
        /// Point class to hold a point coordinates
        /// </summary>
        protected class Point
        {
            public int x;
            public int y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
        #endregion PointClass


        #region MyPanelClass
        /// <summary>
        /// MyPanel extends Panel class and
        /// Overrides OnRender to get the DrawingContext's Bitmap
        /// </summary>
        protected class MyPanel : Panel
        {
            public Bitmap _pBitmap;
            public override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);
                _pBitmap = dc.Bitmap;
                Master_Shapes.autoEvent.Set();

            }
        }
        #endregion MyPanelClass


        #region GetRandomPointsInsideRectangle
        /// <summary>
        /// Gets Random Points inside a Rectangle 
        /// distributed across the four quadrants of a rectangle
        /// </summary>
        /// 
        protected Point[] GetRandomPoints_InRectangle(int size, int w, int h, int xStart, int yStart)
        {
            // p and q are the (x, y) center of the rectangle
            int x, y;
            Point[] chkPoints = new Point[size];
            Random random = new Random();

            chkPoints[0] = new Point(xStart      , yStart      );
            chkPoints[1] = new Point(xStart + w-1, yStart      );
            chkPoints[2] = new Point(xStart + w-1, yStart + h-1);
            chkPoints[3] = new Point(xStart      , yStart + h-1); 

            for (int i = 4; i < chkPoints.Length; i++)
            {
                x = random.Next(w);
                y = random.Next(h);

                chkPoints[i] = new Point(xStart + x, yStart + y);
            }
            return chkPoints;
        }
        #endregion GetRandomPointsInsideRectangle

    }
}
