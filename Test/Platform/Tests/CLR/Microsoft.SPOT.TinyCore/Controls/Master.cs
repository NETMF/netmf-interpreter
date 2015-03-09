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
using Controls.Properties;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_Controls
    {
        public static void Main()
        {
            Master_Controls controls = new Master_Controls();
            Thread appThread = new Thread(new ThreadStart(controls.ApplicationThread));
            appThread.Start();

            //wait until UI Window is created
            _autoEvent.WaitOne();

            Thread.Sleep(500);

            string[] args = {"BorderTests", "CanvasTests","StackPanelTests", "TextTests", "PanelTests",  
                             "ImageTests", "TextFlowTests", "ScrollViewerTests", "ListBoxTests"};
            
            MFTestRunner runner = new MFTestRunner(args);

            Log.Comment("Aborting the Application Thread");
            try
            {
                controls.app.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(controls.ShutDownApp), null);
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

        Application app = null;
        void ApplicationThread()
        {
            app = new Application();

            mainWindow = CreateWindow();

            //autoEvent.Set();
            // Start the application
            app.Run(mainWindow);
        }

        private MyWindow CreateWindow()
        {
            // Create a window object and set its size to the
            // size of the display.
            mainWindow = new MyWindow();
            mainWindow.Height = _height;
            mainWindow.Width = _width;

            Text t = new Text(_font, "Control Tests !");
            t.VerticalAlignment = VerticalAlignment.Center;
            t.HorizontalAlignment = HorizontalAlignment.Center;

            mainWindow.Child = t;
            // Set the window visibility to visible.
            mainWindow.Visibility = Visibility.Visible;

            return mainWindow;
        }
        object ShutDownApp(object arg)
        {
            app.Shutdown();
            return null;
        }
        #endregion NewWindowApplication


        #region Variables
        /// <summary>
        /// Variables all Derived classes use
        /// </summary>
        protected static MyWindow mainWindow = null;
        protected static Font _font = Resources.GetFont(Resources.FontResources.small);
        protected const int c_wait = 200;
        protected static AutoResetEvent _autoEvent = new AutoResetEvent(false);
        protected static readonly int _width = SystemMetrics.ScreenWidth;
        protected static readonly int _height = SystemMetrics.ScreenHeight;
        protected static readonly int _midX = _width  / 2;
        protected static readonly int _midY = _height / 2;
        protected static readonly int _min = System.Math.Min(_width, _height);
        protected static readonly int _max = System.Math.Max(_width, _height);

        protected static bool _argumentException = false;
        #endregion


        #region CleanWindow
        /// <summary>
        /// Cleans the MainWindow,  
        /// Verifies the Window is clean
        /// </summary>
        /// <returns></returns>
        protected MFTestResults CleaningWindow()
        {
            Reset();

            Log.Comment("Cleaning The Window and Verifying");
            try
            {
                Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                                        new DispatcherOperationCallback(CleanWindow), null);
                Thread.Sleep(c_wait);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message + " when cleaning the Window");
                return MFTestResults.Fail;
            }
            Log.Comment("Initializing 100 random points and verifying mainWindow is clean");
            Point[] chkPoints = new Point[100];
            Random random = new Random();
            for (int i = 0; i < chkPoints.Length; i++)
            {
                chkPoints[i] = new Point(random.Next(_width), random.Next(_height));
            }

            return VerifyingPixelColor(chkPoints, Colors.White);
        }
        protected object CleanWindow(object arg)
        {
            //Setting an empty Text as a Child of Window
            //and setting the window background to White
            mainWindow.Child = new Text();
            mainWindow.Background = new SolidColorBrush(Color.White);
            return null;
        }

        protected virtual void Reset()
        {
        }

        #endregion ClearPanel


        #region PixelColorVerification
        /// <summary>
        /// Gets the pixel color and verifies 
        /// </summary>      
        /// <returns></returns>
        protected MFTestResults VerifyingPixelColor(Point[] pts, Color refColor)
        {
            MFTestResults tResult = MFTestResults.Pass;
            for (int i = 0; i < pts.Length; i++)
            {
                Color c = Master_Controls.mainWindow._pBitmap.GetPixel(pts[i].x, pts[i].y);
                if (c != refColor)
                {
                    Debug.Assert(pts[i].x < Master_Controls.mainWindow._pBitmap.Width);
                    Debug.Assert(pts[i].y < Master_Controls.mainWindow._pBitmap.Height);

                    Thread.Sleep(1000);

                    c = Master_Controls.mainWindow._pBitmap.GetPixel(pts[i].x, pts[i].y);

                    if(refColor != c)
                    {
                        Master_Controls.mainWindow._pBitmap.DrawEllipse(Colors.Purple, 1, pts[0].x, pts[0].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);
                        Master_Controls.mainWindow._pBitmap.DrawEllipse(Colors.Purple, 1, pts[1].x, pts[1].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);
                        Master_Controls.mainWindow._pBitmap.DrawEllipse(Colors.Purple, 1, pts[2].x, pts[2].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);
                        Master_Controls.mainWindow._pBitmap.DrawEllipse(Colors.Purple, 1, pts[3].x, pts[3].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);
                 
                        Master_Controls.mainWindow._pBitmap.DrawEllipse(Colors.Red, 1, pts[i].x, pts[i].y, 3, 3, Color.White, 0, 0, Colors.White, 0, 0, Bitmap.OpacityTransparent);
                        Master_Controls.mainWindow._pBitmap.Flush();

                        Log.Comment("Failure : Expected color '" + refColor.ToString() + "' but got '" +
                            c.ToString() + "' at (" + pts[i].x.ToString() + ", " + pts[i].y.ToString() + ")");
                        return MFTestResults.Fail;
                    }
                }
            }
            return tResult;
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

        #region MyWindowClass
        /// <summary>
        /// MyWindow extends Window class and
        /// Overrides OnRender to get the DrawingContext's Bitmap
        /// </summary>
        public class MyWindow : Window
        {
            public Bitmap _pBitmap;
            public override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);
                _pBitmap = dc.Bitmap;
                _autoEvent.Set();
            }

        }
        #endregion MyWindowClass


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
