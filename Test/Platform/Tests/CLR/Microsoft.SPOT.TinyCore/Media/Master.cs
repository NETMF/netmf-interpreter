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
using Media.Properties;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_Media
    {
        public static void Main()
        {
            Master_Media shapes = new Master_Media();
            
            Thread appThread = new Thread(new ThreadStart(shapes.ApplicationThread));
            appThread.Start();

            //wait until UI Window is created
            autoEvent.WaitOne();
            string[] args = { "BrushTests", "ColorTests", "DrawingContextTests" };
            MFTestRunner runner = new MFTestRunner(args);

            Log.Comment("Aborting the Application Thread");
            try
            {
                shapes.app.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(shapes.ShutDownApp), null);
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

            Text t = new Text(Resources.GetFont(Resources.FontResources.NinaB), "Media Tests !");
            t.VerticalAlignment = VerticalAlignment.Center;
            t.HorizontalAlignment = HorizontalAlignment.Center;
            _panel.Children.Add(t);
            mainWindow.Child = _panel;
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
        protected static Window mainWindow = null;
        protected static MyPanel _panel = null;
        protected static Pen _pen = null;
        protected static Brush _brush = null;
        protected static Color _color;
        protected static Font _font = Resources.GetFont(Resources.FontResources.NinaB);
        protected static AutoResetEvent autoEvent = new AutoResetEvent(false);
        protected static int _width = SystemMetrics.ScreenWidth;
        protected static int _height = SystemMetrics.ScreenHeight;
        protected static int midX = _width / 2;
        protected static int midY = _height / 2;
        protected static int min = System.Math.Min(_width, _height);
        protected static int max = System.Math.Max(_width, _height);
        protected static int r, s, x0, y0, x1, y1, wd, ht, xDimension, yDimension;
        protected static int[] pts = null;
        protected static bool _drawLine = false;
        protected static bool _drawRectangle = false;
        protected static bool _drawEllipse = false;
        protected static bool _drawPolygon = false;
        protected static bool _clear = false;
        protected static bool _setPixel = false;
        protected static bool _drawImage = false;
        protected static bool _drawCroppedImage = false;
        protected static bool _translate = false;
        protected static bool _blendImage = false;
        protected static bool _pushClippingRectangle = false;
        protected static bool _popClippingRectangle = false;
        protected static bool _getClippingRectangle = false;
        protected static bool _drawText = false;
        protected static bool _textFits = true;
        protected static bool _argumentException = false;
        protected static bool _nullReferenceException = false;
        protected static string _str = "";
        protected static TextAlignment _alignment;
        protected static TextTrimming _trimming;
        protected static ushort _opacity = 0;
        protected static Bitmap bmp1 = null;
        protected static Bitmap bmp2 = null;
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
                
                Master_Media.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                                        new DispatcherOperationCallback(ClearPanel), null);
                
                autoEvent.WaitOne();
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
            Master_Media._panel.Children.Clear();
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
            MFTestResults tResult = MFTestResults.Pass;
            for (int i = 0; i < pts.Length; i++)
            {
                if ((pts[i].x >= Master_Media._panel._pBitmap.Width) ||
                   (pts[i].y >= Master_Media._panel._pBitmap.Height))
                {
                    continue;
                }

                Color c = Master_Media._panel._pBitmap.GetPixel(pts[i].x, pts[i].y);
                if (c != _color)
                {
                    // this method could fail because of a race condition between renderign and this thread
                    // wait a bit
                    Thread.Sleep(2000);

                    // get the color again
                    c = Master_Media._panel._pBitmap.GetPixel(pts[i].x, pts[i].y);
                    if (c != _color)
                    {
                        Master_Media._panel._pBitmap.DrawEllipse(Colors.Red, 1, pts[i].x, pts[i].y, 3, 3, Colors.White, 0, 0, Color.White, 3, 3, Bitmap.OpacityOpaque);
                        Master_Media._panel._pBitmap.Flush();

                        Log.Comment("Failure : Expected color '" + _color.ToString() + "' but got '" +
                            c.ToString() + "' at (" + pts[i].x.ToString() + ", " + pts[i].y.ToString() + ")");
                        tResult = MFTestResults.Fail;
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
                if (_pushClippingRectangle)
                {
                    try
                    {
                        dc.PushClippingRectangle(x0, y0, xDimension, yDimension);
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Comment("Caught " + ex.Message + " when Pushing a clipping rectangle at (" + x0 + ", " + y0 + ")" +
                            " and Width = " + xDimension + " Height = " + yDimension);
                        _argumentException = true;
                    }
                }
                if (_popClippingRectangle)
                {
                    dc.PopClippingRectangle();
                }
                if (_getClippingRectangle)
                {
                    dc.GetClippingRectangle(out x1, out y1, out wd, out ht);
                }
                if (_drawLine)
                {
                    dc.DrawLine(_pen, x0, y0, x1, y1);
                }
                if (_drawRectangle)
                {
                    try
                    {
                        dc.DrawRectangle(_brush, _pen, r, s, xDimension, yDimension);
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Comment("Caught " + ex.Message + " when drawing a Rectangle at (" + r + ", " + s + ")" +
                            " Width = " + xDimension + " Height = " + yDimension);
                        _argumentException = true;
                    }
                }
                if (_drawEllipse)
                {
                    dc.DrawEllipse(_brush, _pen, r, s, xDimension, yDimension);
                }
                if (_drawPolygon)
                {
                    dc.DrawPolygon(_brush, _pen, pts);
                }
                if (_clear)
                {
                    dc.Clear();
                }
                if (_setPixel)
                {
                    dc.SetPixel(_color, r, s);
                }
                if (_drawImage)
                {
                    try
                    {
                        dc.DrawImage(bmp1, r, s);
                    }
                    catch (NullReferenceException ex)
                    {
                        Log.Comment("Caught " + ex.Message + " when drawing a null Image");
                        _nullReferenceException = true;
                    }
                }
                if (_drawCroppedImage)
                {
                    try
                    {
                        dc.DrawImage(bmp1, r, s, x0, y0, xDimension, yDimension);
                    }                   
                    catch (ArgumentException ex)
                    {
                        Log.Comment("Caught " + ex.Message + " when drawing an Image at (" + r + ", " + s + ")" +
                            "from a source Image at(" + x0 + ", " + y0 + ") Width = " + xDimension + " Height = " + yDimension);
                        _argumentException = true;
                    }
                    catch (NullReferenceException ex)
                    {
                        Log.Comment("Caught " + ex.Message + " when drawing a null Image");
                        _nullReferenceException = true;
                    }
                }
                if (_translate)
                {
                    dc.Translate(r, s);
                }
                if (_blendImage)
                {
                    dc.BlendImage(bmp2, x0, y0, x1, y1, xDimension, yDimension, _opacity);
                }
                if (_drawText)
                {
                    _textFits = dc.DrawText(ref _str, _font, _color, r, s, xDimension, yDimension, _alignment, _trimming);
                }
                base.OnRender(dc);
                _pBitmap = dc.Bitmap;

                Master_Media.autoEvent.Set();
            }
        }

        #endregion MyPanelClass


        #region GetRandomPointsInsideRectangle
        /// <summary>
        /// Gets Random Points inside a Rectangle 
        /// distributed across the four quadrants of a rectangle
        /// </summary>
        /// 
        protected Point[] GetRandomPoints_InRectangle(int size, int w, int h, int p, int q)
        {
            // p and q are the (x, y) center of the rectangle
            int x, y;
            Point[] chkPoints = new Point[size];
            Random random = new Random();

            for (int i = 0; i < chkPoints.Length; i++)
            {
                x = random.Next(w - 2) / 2;
                y = random.Next(h - 2) / 2;
                switch (i % 4)
                {
                    case 0:
                        chkPoints[i] = new Point(p + x, q - y);
                        break;
                    case 1:
                        chkPoints[i] = new Point(p + x, q + y);
                        break;
                    case 2:
                        chkPoints[i] = new Point(p - x, q + y);
                        break;
                    case 3:
                        chkPoints[i] = new Point(p - x, q - y);
                        break;
                }
            }
            return chkPoints;
        }
        #endregion GetRandomPointsInsideRectangle

    }

    #region Color Comparison
    public static class ColorExtensions
    {
        public static bool ColorEquals(Color c1, Color c2)
        {
            //Ignore 3LSB Red & Blue bits, and 2 LSB green bits
            return ((int)c1 & 0xF8FCF8) == ((int)c2 & 0xF8FCF8);
        }
    }
    #endregion
}
