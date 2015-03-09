////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Platform.Test;
using Media.Properties;

namespace Microsoft.SPOT.Platform.Tests
{
    public class BrushTests : Master_Media, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Brush Tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        public MFTestResults SolidColorBrush_Test1()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point pt = new Point(3 * _width / 4, 3 * _height / 4);
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                _brush = new SolidColorBrush(Colors.Blue);
                Log.Comment("Drawing Rectangle : w = " + pt.x.ToString() + ", h = " + pt.y.ToString());
                Log.Comment("and filling with SolidColorBrush");
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), pt);
                autoEvent.WaitOne();
                
                Log.Comment("Initializing 40 Random Points that are inside the rectangle");
                Point[] chkPoints = GetRandomPoints_InRectangle(40, pt.x, pt.y, midX, midY);
                Log.Comment("Verifying the pixel colors inside the rectangle");
                testResult = VerifyingPixelColor(chkPoints, Colors.Blue);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        public MFTestResults ImageBrush_Test2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                Bitmap bmp = Resources.GetBitmap(Resources.BitmapResources.Yellow_flower);
                _brush = new ImageBrush(bmp);
                Log.Comment("Filling a Rectangle with an ImageBrush and verifying");
                testResult = ImageBrushTest(bmp);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        public MFTestResults ImageBrush_BitmapSourceTest3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                Bitmap flower = Resources.GetBitmap(Resources.BitmapResources.Yellow_flower);
                Log.Comment("Filling Rectangle with an ImageBrush");
                _brush = new ImageBrush(flower);
                if (ImageBrushTest(flower) != MFTestResults.Pass)
                {
                    Log.Comment("Failure filling the Rectangle with ImageBrush");
                    testResult = MFTestResults.Fail;
                }
                Bitmap cartoon = Resources.GetBitmap(Resources.BitmapResources.Tom_Jerry);
                Log.Comment("Changing the ImageBrush BitmapSource and verifying");
                ((ImageBrush)_brush).BitmapSource = cartoon;
                if (ImageBrushTest(cartoon) != MFTestResults.Pass)
                {
                    Log.Comment("Failure on changing the ImageBrush BitmapSource and verifying");
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

        public MFTestResults ImageBrush_StretchTest4()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                Bitmap smallFlower = Resources.GetBitmap(Resources.BitmapResources.Yellow_flower_small);
                int w = 4 * smallFlower.Width, h = 4 * smallFlower.Height;
                _pen = new Pen(Colors.Black, 1);
                _brush = new ImageBrush(smallFlower);
                ((ImageBrush)_brush).Stretch = Stretch.None;
                Log.Comment("Drawing Rectangle : w = " + w.ToString() + ", h = " + h.ToString());
                Log.Comment("filling the Rectangle with ImageBrush not stretched");
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), new Point(w, h));
                autoEvent.WaitOne();
                
                Log.Comment("Initializing 40 Random Points inside Rectangle but not on the image");
                Point[] chkPoints = GetRandomPoints_InRectangle(40, (_rectangle.ActualWidth / 2) - 1,
                    (_rectangle.ActualHeight / 2) - 1, midX, midY);
                Log.Comment("Verifying pixel color around the center are white");
                if (VerifyingPixelColor(chkPoints, Colors.White) != MFTestResults.Pass)
                {
                    testResult = MFTestResults.Fail;
                }
                ((ImageBrush)_brush).Stretch = Stretch.Fill;
                Log.Comment("Drawing Rectangle : w = " + w.ToString() + ", h = " + h.ToString());
                Log.Comment("filling the Rectangle with ImageBrush Stretched and Verifying");
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), new Point(w, h));
                autoEvent.WaitOne();
                
                for (int i = 0; i < chkPoints.Length; i++)
                {
                    if (_panel._pBitmap.GetPixel(chkPoints[i].x, chkPoints[i].y) == Colors.White)
                    {
                        testResult = MFTestResults.Fail;
                        Log.Comment("Failure : Not expected to see White color at (" + chkPoints[i].x.ToString() +
                            ", " + chkPoints[i].y.ToString() + ") after ImageBrush stretched");
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

        public MFTestResults LinearGradientBrush_DiagonalUpTest5()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point pt = new Point(3 * min / 4, 3 * min / 4);
            int x1 = 0, y1 = 0, x2 = pt.x-1, y2 = pt.y-1;
            Color startColor = Colors.Red, endColor = (Color)0x00ffff;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                _pen = null;
                _brush = null;
                Log.Comment("Drawing Rectangle, w = " + pt.x.ToString() + " h = " + pt.y.ToString());
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), pt);
                autoEvent.WaitOne();
                
                if (LinearGradientBrushTest(startColor, endColor, 0, 0, pt.x, pt.y) != MFTestResults.Pass)
                {
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Verifying start corner is " + startColor.ToString());
                _rectangle.PointToScreen(ref x1, ref y1);
                _rectangle.PointToScreen(ref x2, ref y2);
                Color c1 = _panel._pBitmap.GetPixel(x1, y1);
                if (!ColorExtensions.ColorEquals(c1, startColor))
                {
                    _panel._pBitmap.DrawEllipse(Colors.Red, 1, x1, y1, 3, 3, (Color)0, 0, 0, (Color)0, 0, 0, Bitmap.OpacityTransparent);
                    _panel._pBitmap.Flush();

                    Log.Comment("Failure : Start corner expected color " +
                        startColor.ToString() + " got " + c1.ToString());
                    testResult = MFTestResults.Fail;
                }
                Color c2 = _panel._pBitmap.GetPixel(x2, y2);
                Log.Comment("Verifying end corner is " + endColor.ToString());
                if (!ColorExtensions.ColorEquals(c2, endColor))
                {
                    _panel._pBitmap.DrawEllipse(Colors.Red, 1, x2, y2, 2, 2, (Color)0, 0, 0, (Color)0, 0, 0, Bitmap.OpacityTransparent);
                    _panel._pBitmap.Flush();

                    Log.Comment("Failure : End corner expected color " +
                        endColor.ToString() + " got " + c2.ToString());
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

        public MFTestResults LinearGradientBrush_DiagonalDownTest6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Initializing starting color, ending color and gradient path");
            // Black BGR = 0X0, DarkGreen BGR = 0x006400 
            Color startColor = (Color)0, endColor = (Color)0x006400;
            //int x1 = w, y1 = 0, x2 = 0, y2 = w;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {

                if (LinearGradientBrushTest(startColor, endColor, LinearGradientBrush.RelativeBoundingBoxSize, 0, 0, LinearGradientBrush.RelativeBoundingBoxSize) != MFTestResults.Pass)
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

        public MFTestResults LinearGradientBrush_HorizontalTest7()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Initializing starting color, ending color and gradient path");
            //int x1 = w / 2, y1 = 0, x2 = w / 2, y2 = w;
            //DarkBlue BGR = 0x8B0000, DarkRed BGR = 0x00008B          
            Color startColor = (Color)0x00008B, endColor = (Color)0x8B0000;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {

                if (LinearGradientBrushTest(startColor, endColor, LinearGradientBrush.RelativeBoundingBoxSize / 2, 0, LinearGradientBrush.RelativeBoundingBoxSize / 2, LinearGradientBrush.RelativeBoundingBoxSize) != MFTestResults.Pass)
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

        public MFTestResults LinearGradientBrush_VerticalTest8()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Initializing starting color, ending color and gradient path");
            //int x1 = 0, y1 = w / 2, x2 = w, y2 = w / 2;
            Color startColor = (Color)0x0000ff, endColor = Colors.Blue;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                testResult = LinearGradientBrushTest(startColor, endColor, 0, LinearGradientBrush.RelativeBoundingBoxSize / 2, LinearGradientBrush.RelativeBoundingBoxSize, LinearGradientBrush.RelativeBoundingBoxSize / 2);
            }
            catch (Exception ex)
            {
                Log.Comment("Caught : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        public MFTestResults Brush_OpacityTest9()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point pt1 = new Point(3 * min / 4, 3 * min / 4);
            Point pt2 = new Point(3 * _width / 4, 3 * _height / 4);
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                //Red - BGR 0x0000ff
                _brush = new SolidColorBrush(Colors.Red);
                Log.Comment("Drawing Square with s = " + pt1.x.ToString() + " and filling with Red color");
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), pt1);
                autoEvent.WaitOne();
                

                //Yellow - BGR 0x00ffff   
                _brush = new SolidColorBrush((Color)0x00ffff);
                _brush.Opacity = 0;
                Log.Comment("Drawing Rectangle, w = " + pt2.x.ToString() + " h = " + pt2.y.ToString());
                Log.Comment("and filling with Yellow color");
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), pt2);
                autoEvent.WaitOne();
                
                Log.Comment("Initializing 40 Random Points that are inside the rectangle");
                Point[] chkPoints = GetRandomPoints_InRectangle(40, pt1.x, pt1.y, midX, midY);
                if (VerifyingPixelColor(chkPoints, Colors.Red) != MFTestResults.Pass)
                {
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Varying the Opacity from 0 to 256 and verifying the color at the center");
                Color c1 = _panel._pBitmap.GetPixel(midX, midY), c2;
                //try i <= 256, i += 16, i.e Opacity = 256
                for (int i = 0; i <= (int)Bitmap.OpacityOpaque; i += 16)
                {
                    _brush.Opacity = (ushort)i;
                    
                    Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                   new DispatcherOperationCallback(DrawRectangle), pt2);
                    autoEvent.WaitOne();
                    
                    c2 = _panel._pBitmap.GetPixel(midX, midY);
                    if (c1 > c2)
                    {
                        testResult = MFTestResults.Fail;
                    }
                }
                chkPoints = GetRandomPoints_InRectangle(40, pt1.x, pt1.y, midX, midY);
                Log.Comment("Verifying the pixel colors in the inside square");
                if (VerifyingPixelColor(chkPoints, (Color)0x00ffff) != MFTestResults.Pass)
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

        public MFTestResults Brush_DerviedBrushTest10()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point pt = new Point(3 * _width / 4, 3 * _height / 4);
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            try
            {
                _brush = new SolidColorBrush(Colors.Red);
                Log.Comment("Drawing Rectangle : w = " + pt.x.ToString() + ", h = " + pt.y.ToString());
                Log.Comment("and filling with Red SolidColorBrush");
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(DrawRectangle), pt);
                autoEvent.WaitOne();
                
                Log.Comment("Initializing 40 Random Points that are inside the rectangle");
                Point[] chkPoints = GetRandomPoints_InRectangle(40, pt.x, pt.y, midX, midY);
                Log.Comment("Verifying the pixel colors inside the rectangle are Red");
                if (VerifyingPixelColor(chkPoints, Colors.Red) != MFTestResults.Pass)
                {
                    Log.Comment("Failure : Rectangle not filled with SolidColorBrush");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Changing the Brush to a new BlueBrush, drawing and verifying");
                _brush = new BlueBrush();
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(DrawRectangle), pt);
                autoEvent.WaitOne();
                
                Log.Comment("Verifying the pixel colors inside the rectangle are Blue");
                if (VerifyingPixelColor(chkPoints, Colors.Blue) != MFTestResults.Pass)
                {
                    Log.Comment("Failure : Rectangle not filled with BlueBrush");
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

        Rectangle _rectangle = null;
        int w = 3 * min / 4;

        private object DrawRectangle(object arg)
        {
            Point pt = arg as Point;
            _rectangle = new Rectangle();
            try
            {
                _rectangle.Width = pt.x;
                _rectangle.Height = pt.y;
                _rectangle.HorizontalAlignment = HorizontalAlignment.Center;
                _rectangle.VerticalAlignment = VerticalAlignment.Center;
            }
            catch (Exception e)
            {
                Log.Comment("Caught : " + e.Message + " when setting Width = " +
                    pt.x + " and Height = " + pt.y);
            }
            _rectangle.Stroke = _pen;
            _rectangle.Fill = _brush;
            Master_Media._panel.Children.Add(_rectangle);

            return null;
        }

        private MFTestResults LinearGradientBrushTest(Color start, Color end, int x1, int y1, int x2, int y2)
        {
            MFTestResults tResult = MFTestResults.Pass;
            Point pt = new Point(w, w);
            int p1x = x1 * w / LinearGradientBrush.RelativeBoundingBoxSize,
                p1y = y1 * w / LinearGradientBrush.RelativeBoundingBoxSize,
                p2x = x2 * w / LinearGradientBrush.RelativeBoundingBoxSize,
                p2y = y2 * w / LinearGradientBrush.RelativeBoundingBoxSize;
            _brush = new LinearGradientBrush(start, end, x1, y1, x2, y2);
            Log.Comment("Drawing : Rectangle, w = " + pt.x.ToString() + " h = " + pt.y.ToString());
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(DrawRectangle), pt);
            autoEvent.WaitOne();
            
            Log.Comment("Verifying the color changes along the gradient");
            _rectangle.PointToScreen(ref p1x, ref p1y);
            _rectangle.PointToScreen(ref p2x, ref p2y);
            int temp1 = p1x + 1, temp2 = p1y + 1;
            Color c1 = _panel._pBitmap.GetPixel(temp1, temp2), c2;

            //try dividing by 10
            int stepX = (p2x - p1x) / 5, stepY = (p2y - p1y) / 5;
            int iMax = temp1 + p2x - p1x;
            for (temp1 = temp1 + stepX, temp2 = temp2 + stepY; temp1 < iMax - 2; temp1 += stepX, temp2 += stepY)
            {
                c2 = _panel._pBitmap.GetPixel(temp1, temp2);
                if (c2 <= c1)
                {
                    Log.Comment("Expected color value increases along gradient but got color " +
                        c2.ToString() + " after color " + c1.ToString() + " at (" + temp1.ToString() + ", " + temp2.ToString() + ")");
                    tResult = MFTestResults.Fail;
                }
                c1 = _panel._pBitmap.GetPixel(temp1, temp2);
            }
            return tResult;
        }

        private MFTestResults ImageBrushTest(Bitmap bmp)
        {
            MFTestResults tResult = MFTestResults.Pass;
            int x, y, w = bmp.Width, h = bmp.Height;
            Log.Comment("Drawing Rectangle with Bitmap's w = " + w.ToString() + " and h = " + h.ToString());
            Log.Comment("filling the Rectangle with ImageBrush ");
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(DrawRectangle), new Point(bmp.Width, bmp.Height));
            autoEvent.WaitOne();

            Log.Comment("Getting a random pixel colors on the image and verifying");

            Random random = new Random();

            for (int i = 0; i < 10; i++)
            {
                x = random.Next(w - 2) + 1;
                y = random.Next(h - 2) + 1;

                Color c = bmp.GetPixel(x, y);

                int x1 = x, y1 = y;
                _rectangle.PointToScreen(ref x1, ref y1);
                if (VerifyingPixelColor(new Point[] { new Point(x1, y1) }, c) != MFTestResults.Pass)
                {
                    tResult = MFTestResults.Fail;
                    break;
                }
            }
            
            return tResult;
        }
             
        public class BlueBrush : Brush
        {         
            protected override void RenderRectangle(Bitmap bmp, Pen outline, int x, int y, int width, int height)
            {
                bmp.DrawRectangle(Colors.Blue, 1, x, y, width, height, 0, 0, Colors.Blue, 0, 0, Colors.Blue, 0, 0, Opacity);
            }
        }
    }    
}
