////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using Controls.Properties;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_Presentation
    {
        public static void Main()
        {
            Master_Presentation presentation = new Master_Presentation();
            Thread appThread = new Thread(new ThreadStart(presentation.ApplicationThread));
            appThread.Start();

            //wait until UI Window is created
            autoEvent.WaitOne();
            string[] args = { "UIElementTests" };
            MFTestRunner runner = new MFTestRunner(args);

            Log.Comment("Aborting the Application Thread");
            try
            {
                presentation.app.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(presentation.ShutDownApp), null);
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

        protected Application app = null;
        void ApplicationThread()
        {
            app = new Application();

            mainWindow = CreateWindow();

            autoEvent.Set();
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

            Text t = new Text(_font, "Presentaion Tests !");
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

        protected const int wait = 20;
        protected static AutoResetEvent autoEvent = new AutoResetEvent(false);
        protected static int _width = SystemMetrics.ScreenWidth;
        protected static int _height = SystemMetrics.ScreenHeight;
        protected static int midX = _width / 2;
        protected static int midY = _height / 2;
        protected static int min = System.Math.Min(_width, _height);
        protected static int max = System.Math.Max(_width, _height);
     
        #endregion


        #region CleanWindow
        /// <summary>
        /// Cleans the MainWindow,  
        /// Verifies the Window is clean
        /// </summary>
        /// <returns></returns>
        protected MFTestResults CleaningWindow()
        {
            Log.Comment("Cleaning The Window and Verifying");
            try
            {
                Master_Presentation.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                                        new DispatcherOperationCallback(CleanWindow), null);
                Thread.Sleep(wait);
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
            mainWindow.Invalidate();
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
                Color c = Master_Presentation.mainWindow._pBitmap.GetPixel(pts[i].x, pts[i].y);
                if (c != _color)
                {
                    Thread.Sleep(1000);

                    c = Master_Presentation.mainWindow._pBitmap.GetPixel(pts[i].x, pts[i].y);

                    if(_color != c)
                    {
                        Master_Presentation.mainWindow._pBitmap.DrawEllipse(c == Colors.Red ? Colors.Green : Colors.Red, 1, pts[i].x, pts[i].y, 3, 3, (Color)0, 0, 0, (Color)0, 0, 0, Bitmap.OpacityTransparent);
                        Master_Presentation.mainWindow._pBitmap.Flush();

                        Log.Comment("Failure : Expected color '" + _color.ToString() + "' but got '" +
                            c.ToString() + "' at (" + pts[i].x.ToString() + ", " + pts[i].y.ToString() + ")");
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
                Master_Presentation.autoEvent.Set();
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

        #region GetRandomPointsOutsideRectangle
        /// <summary>
        /// Gets Random Points outside a Rectangle 
        /// and within the Window
        /// </summary>
        /// 
        protected Point[] GetRandomPoints_OutofRectangle(int size, int w, int h, int p, int q)
        {
            // p and q are the (x, y) center of the rectangle
            int x, y;
            Point[] chkPoints = new Point[size];
            Random random = new Random();
            for (int i = 0; i < chkPoints.Length; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        x = random.Next(p - (w / 2));
                        y = random.Next(_height);
                        chkPoints[i] = new Point(x, y);
                        break;
                    case 1:
                        x = random.Next(_width - (p + (w / 2))) + p + w / 2;
                        y = random.Next(_height);
                        chkPoints[i] = new Point(x, y);
                        break;
                    case 2:
                        x = random.Next(_width);
                        y = random.Next(q - (h / 2));
                        chkPoints[i] = new Point(x, y);
                        break;
                    case 3:
                        x = random.Next(_width);
                        y = random.Next(_height - (q + (h / 2))) + q + h / 2;
                        chkPoints[i] = new Point(x, y);
                        break;
                }
            }

            return chkPoints;
        }
        #endregion GetRandomPointsOutsideRectangle
    }
}
