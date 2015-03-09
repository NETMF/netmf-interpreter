////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Platform.Test;
using System.Threading;
using Media.Properties;

namespace Microsoft.SPOT.Platform.Tests
{
    public class DrawingContextTests : Master_Media, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for DrawingContext tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawLineTest1()
        {
            MFTestResults testResult = MFTestResults.Pass;

            Log.Comment("Drawing 10 diff. Lines and verifying pixel color on the center");
            _pen = new Pen(Colors.Green);
            Random random = new Random();

            for (int i = 0; i < 10; i++)
            {
                x0 = random.Next(midX);
                y0 = random.Next(midY);
                x1 = random.Next(_width);
                y1 = random.Next(_height);
                if (ClearingPanel() != MFTestResults.Pass)
                {
                    return MFTestResults.Fail;
                }
                _drawLine = true;
                Log.Comment("Drawing Line b/n (" + x0.ToString() + ", " + y0.ToString() + ") and (" +
                    x1.ToString() + ", " + y1.ToString() + ") and Verifying");
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(InvalidatePanel), null);
                autoEvent.WaitOne();
                
                testResult = VerifyingPixelColor(new Point[] { new Point(x0 + ((x1 - x0) / 2), y0 + ((y1 - y0) / 2)) }, Colors.Green);
                _drawLine = false;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawRectangleTest2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            xDimension = midX - 1;
            yDimension = midY - 1;
            r = midX;
            s = midY;
            _pen = new Pen(Colors.Green);
            _brush = new SolidColorBrush(Colors.Blue);
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _drawRectangle = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Log.Comment("Initializing 40 Random Points that are inside the rectangle");
            Point[] chkPoints = GetRandomPoints_InRectangle(40, xDimension, yDimension, r + xDimension / 2, s + yDimension / 2);
            Log.Comment("Verifying the pixel colors inside the rectangle");
            testResult = VerifyingPixelColor(chkPoints, Colors.Blue);
            _drawRectangle = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawRectangle_ArgumentExceptionTest3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }

            r = midX / 2;
            s = midY / 2;
            _pen = new Pen(Colors.Green);
            _brush = new SolidColorBrush(Colors.Blue);
            int[] dimensions = GetRandomDimensions(midX, midY);
            for (int i = 0; i < dimensions.Length / 2; i++)
            {
                // make at least one dimension negative 
                if (i == 2)
                {
                    xDimension = -dimensions[i];
                    yDimension = -dimensions[2 * i + 1];
                }
                else
                {
                    xDimension = (i % 2) == 0 ? dimensions[i] : -dimensions[i];
                    yDimension = (i % 2) != 0 ? dimensions[2 * i + 1] : -dimensions[2 * i + 1];
                }

                _argumentException = false;
                _drawRectangle = true;
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(InvalidatePanel), null);
                autoEvent.WaitOne();
                if (!_argumentException)
                {
                    Log.Comment("Drawing a Rectangle at (" + r + ", " + s + ") with Width = " +
                        xDimension + " and Height = " + yDimension + " didn't throw ArgumentException");
                    testResult = MFTestResults.Fail;
                }
                _drawRectangle = false;
                _argumentException = false;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawEllipseTest4()
        {
            MFTestResults testResult = MFTestResults.Pass;
            xDimension = midX - 10;
            yDimension = midY - 10;
            r = midX;
            s = midY;
            _pen = new Pen(Colors.Red);
            _brush = new SolidColorBrush(Colors.Green);
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _drawEllipse = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Log.Comment("Checking pixel colors on the ellipse");
            Point[] chkPoints1 = new Point[] { new Point(r+xDimension, s), new Point(r-xDimension, s),
            new Point(r, s+yDimension), new Point(r, s-yDimension)};
            if (VerifyingPixelColor(chkPoints1, Colors.Red) != MFTestResults.Pass)
            {
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Checking pixel colors inside the ellipse");
            int minSquare = 2 * (int)(System.Math.Min(xDimension, yDimension) / 1.5);
            Log.Comment("Initializing 40 Random Points that are inside the rectangle");
            Point[] chkPoints2 = GetRandomPoints_InRectangle(40, minSquare, minSquare, r, s);
            Log.Comment("Verifying the pixel colors inside the rectangle");
            if (VerifyingPixelColor(chkPoints2, Colors.Green) != MFTestResults.Pass)
            {
                testResult = MFTestResults.Fail;
            }
            _drawEllipse = false;
            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawPolygonTest5()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int min3 = min / 3;
            Point[] pts1 = new Point[] {new Point( midX - min3, midY),new Point(midX, midY - min3),
            new Point(midX + min3, midY), new Point(midX, midY + min3)};
            Log.Comment("Initializing Drawing Points");
            pts = new int[] { pts1[0].x, pts1[0].y, pts1[1].x, pts1[1].y, pts1[2].x, pts1[2].y, pts1[3].x, pts1[3].y };
            _pen = new Pen(Colors.Blue);
            _brush = new SolidColorBrush(Colors.Green);
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _drawPolygon = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Log.Comment("Verifying the pixel colors on the polygon");
            if (VerifyingPixelColor(pts1, Colors.Blue) != MFTestResults.Pass)
            {
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Initializing 40 Random Points that are inside Polygon");
            Point[] chkPoints = GetRandomPoints_InRectangle(40, min3, min3, midX, midY);
            if (VerifyingPixelColor(chkPoints, Colors.Green) != MFTestResults.Pass)
            {
                testResult = MFTestResults.Fail;
            }
            _drawPolygon = false;

            return testResult;
        }


        [TestMethod]
        public MFTestResults DrawingContext_DrawPolygonTest6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point[] pts1 = new Point[] {
                new Point(midX         , 5            ),
                new Point(midX + 30    , midY - 40    ),
                new Point(2 * midX - 35, midY - 40    ), 
                new Point(midX + 40    , midY         ),
                new Point(2 * midX - 35, 2 * midY - 35),
                new Point(midX         , midY + 40   ),
                new Point(35           , 2 * midY - 35),
                new Point(midX - 40    , midY         ),
                new Point(35           , midY - 40    ),
                new Point(midX - 30    , midY - 40    ),
                new Point(midX         , 5            ),

            };
            Log.Comment("Initializing Drawing Points");
            pts = new int[pts1.Length * 2];

            for (int j = 0; j < pts1.Length; j++)
            {
                pts[2 * j] = pts1[j].x;
                pts[2 * j + 1] = pts1[j].y;
            }
            _pen = new Pen(Colors.Blue);
            _brush = new SolidColorBrush(Colors.Green);
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _drawPolygon = true;
            autoEvent.Reset();
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();

            Log.Comment("Verifying the pixel colors on the polygon");
            if (VerifyingPixelColor(pts1, Colors.Blue) != MFTestResults.Pass)
            {
                testResult = MFTestResults.Fail;
            }

            _drawPolygon = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawPolygonTest7()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Point[] pts1 = new Point[] {
                new Point(35           , midY - 40    ),
                new Point(midX - 30    , midY - 40    ),
                new Point(midX         , midY + 60    ),
                new Point(midX         , midY + 60    ),
                new Point(midX + 30    , midY - 40    ),
                new Point(2 * midX - 35, midY - 40    ), 
                new Point(midX - 10    , midY         ),
                new Point(2 * midX - 35, 2 * midY - 35),
                new Point(midX         , midY + 40    ),
                new Point(35           , 2 * midY - 35),
                new Point(midX + 10    , midY         ),
            };
            Log.Comment("Initializing Drawing Points");
            pts = new int[pts1.Length * 2];

            for (int j = 0; j < pts1.Length; j++)
            {
                pts[2 * j] = pts1[j].x;
                pts[2 * j + 1] = pts1[j].y;
            }
            _pen = new Pen(Colors.Blue);
            _brush = new SolidColorBrush(Colors.Green);
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _drawPolygon = true;
            autoEvent.Reset();
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();

            Log.Comment("Verifying the pixel colors on the polygon");
            if (VerifyingPixelColor(pts1, Colors.Blue) != MFTestResults.Pass)
            {
                testResult = MFTestResults.Fail;
            }
            _drawPolygon = false;

            return testResult;
        }


        [TestMethod]
        public MFTestResults DrawingContext_ClearTest6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _drawLine = true;
            _drawRectangle = true;
            _drawEllipse = true;
            _drawPolygon = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            _pen = new Pen(Colors.Red);
            _brush = new SolidColorBrush(Colors.Green);
            Log.Comment("Clearing the DrawingContext");
            _drawLine = false;
            _drawRectangle = false;
            _drawEllipse = false;
            _drawPolygon = false;
            _clear = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
            new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Log.Comment("Initializing 40 Random Points that are inside Polygon");
            Point[] chkPoints = GetRandomPoints_InRectangle(40, _width, _height, midX, midY);
            testResult = VerifyingPixelColor(chkPoints, Colors.Black);
            _clear = false;
            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawImage_ArgumentException_Test8()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }

            r = midX / 2;
            s = midY / 2;
            x0 = 80;
            y0 = 5;
            bmp1 = Resources.GetBitmap(Resources.BitmapResources.Tom_Jerry);
            Log.Comment("Drawing a Cropped Image on a DrawingContext");
            int[] dimensions = GetRandomDimensions(midX, midY);
            for (int i = 0; i < dimensions.Length / 2; i++)
            {
                // at least one dimension shoudl be negative
                if(i == 2)
                {
                    xDimension = -dimensions[i];
                    yDimension = -dimensions[2 * i + 1];
                }
                else{
                    xDimension = (i % 2 == 0) ? dimensions[i]         : -dimensions[i];
                    yDimension = (i % 2 != 0) ? dimensions[2 * i + 1] : -dimensions[2 * i + 1];
                }

                _argumentException = false;
                _drawCroppedImage = true;
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(InvalidatePanel), null);

                autoEvent.WaitOne();

                if (!_argumentException)
                {
                    Log.Comment("Drawing a cropped image of Width " + xDimension +
                        " and Height " + yDimension + " didn't throw ArgumentException");
                    testResult = MFTestResults.Fail;
                }
                _drawCroppedImage = false;
                _argumentException = false;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawImage_NullReeferenceException_Test9()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Drawing an Image with null Bitmap and verifying for NullReferenceException");
            r = midX / 2;
            s = midY / 2;
            x0 = 80;
            y0 = 5;
            bmp1 = null;
            Log.Comment("Drawing dc.DrawImage(null, x, y) Image on a DrawingContext");
            _nullReferenceException = false;
            _drawImage = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
    new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            if (!_nullReferenceException)
            {
                Log.Comment("dc.DrawImage(null, x, y) didn't throw NullReferenceException");
                testResult = MFTestResults.Fail;
            }
            _drawImage = false;
            _nullReferenceException = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawCroppedImage_NullReeferenceException_Test10()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Drawing an Image with null Bitmap and verifying for NullReferenceException");
            r = midX / 2;
            s = midY / 2;
            x0 = 80;
            y0 = 5;
            bmp1 = null;
            Log.Comment("dc.DrawImage(null, x, y, sourceX, sourceY, sWidth, sHeight) Image on a DrawingContext");
            _nullReferenceException = false;
            _drawCroppedImage = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
  new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            Thread.Sleep(500);
            if (!_nullReferenceException)
            {
                Log.Comment("dc.DrawImage(null, x, y, sourceX, sourceY, sWidth, sHeight) didn't throw NullReferenceException");
                testResult = MFTestResults.Fail;
            }
            _drawCroppedImage = false;
            _nullReferenceException = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawImageTest11()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            int x, y;
            r = 7;
            s = 7;
            bmp1 = Resources.GetBitmap(Resources.BitmapResources.Yellow_flower);
            Log.Comment("Drawing an Image on a DrawingContext");
            _drawImage = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
           new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Log.Comment("Getting the pixel colors on the panel and verifying");

            Random random = new Random();

            int w = bmp1.Width - r;
            int h = bmp1.Height - s;

            for (int i = 0; i < 100; i++)
            {
                x = random.Next(w);
                y = random.Next(h);
                Color c1 = bmp1.GetPixel(x, y), c2 = _panel._pBitmap.GetPixel(x + r, y + s);
                if (c1 != c2)
                {
                    Log.Comment("Expected color '" + c1.ToString() + "' but got '"
                        + c2.ToString() + "' at (" + x.ToString() + ", " + y.ToString() + ")");
                    testResult = MFTestResults.Fail;
                }
            }
            _drawImage = false;

            return testResult;
        }


        [TestMethod]
        public MFTestResults DrawingContext_drawCroppedImageTest12()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            int x, y;
            r = midX / 2;
            s = midY / 2;
            x0 = 80;
            y0 = 5;
            xDimension = 100;
            yDimension = 120;
            bmp1 = Resources.GetBitmap(Resources.BitmapResources.Tom_Jerry);
            Log.Comment("Drawing a Cropped Image on a DrawingContext");
            _drawCroppedImage = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
           new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Log.Comment("Getting the pixel colors on the cropped image and verifying");
            Random random = new Random();

            for (int i = 0; i < 20; i++)
            {
                x = random.Next(xDimension);
                y = random.Next(yDimension);
                Color c1 = bmp1.GetPixel(x + x0, y + y0), c2 = _panel._pBitmap.GetPixel(x + r, y + s);
                if (c1 != c2)
                {
                    Log.Comment("Expected color '" + c1.ToString() + "' but got '"
                        + c2.ToString() + "' at (" + x.ToString() + ", " + y.ToString() + ")");
                    testResult = MFTestResults.Fail;
                }
            }
            Log.Comment("Verifying Areas outside cropped image area clean");
            if (Test_PixelsOutsideRectangualrArea(r, s, xDimension, yDimension, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure in Veifying pixel colors outside cropped Image");
                testResult = MFTestResults.Fail;
            }
            _drawCroppedImage = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_BlendImageTest14()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            int x, y;
            r = midX / 4;
            s = midY / 4;
            bmp1 = Resources.GetBitmap(Resources.BitmapResources.Green);
            Log.Comment("Drawing Green Image on a DrawingContext and verifying");
            _drawImage = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
           new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Color c1 = Colors.Green, c2;
            Log.Comment("Verifying pixel colors on the image are Green");
            Random random = new Random();

            int wMin = bmp1.Width - r;
            int hMin = bmp1.Height - s;

            for (int i = 0; i < 100; i++)
            {
                x = random.Next(wMin);
                y = random.Next(hMin);
                c2 = _panel._pBitmap.GetPixel(x + r, y + s);
                if (c2 != c1)
                {
                    Log.Comment("Expected color '" + c1.ToString() + "' but got '"
                        + c2.ToString() + "' at (" + x.ToString() + ", " + y.ToString() + ")");
                    testResult = MFTestResults.Fail;
                }
            }
            bmp2 = Resources.GetBitmap(Resources.BitmapResources.Red);
            x0 = r + midX / 4;
            y0 = s + midY / 4;
            x1 = 0;
            y1 = 0;
            xDimension = bmp2.Width;
            yDimension = bmp2.Height;
            _opacity = 128;
            _blendImage = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
        new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Log.Comment("Veifying Image Areas not blended retain their original color");
            for (int i = 0; i < 10; i++)
            {
                x = r + random.Next(midX / 4);
                y = s + random.Next(midY / 4);
                if (_panel._pBitmap.GetPixel(x, y) != Colors.Green)
                {
                    Log.Comment("Image Area not blended hasn't retained its original color");
                    testResult = MFTestResults.Fail;
                }
            }
            Log.Comment("Verifying Blended Image areas have mixed color");
            Color commonColor = _panel._pBitmap.GetPixel(x0 + 1, y0 + 1);
            int w = bmp1.Width - midX / 4, h = bmp1.Height - midY / 4;
            Point[] chkPoint = GetRandomPoints_InRectangle(20, w, h, x0 + (w / 2), y0 + (h / 2));
            if (VerifyingPixelColor(chkPoint, commonColor) != MFTestResults.Pass)
            {
                testResult = MFTestResults.Fail;
            }
            _drawImage = false;
            _blendImage = false;
            return testResult;
        }
        [TestMethod]
        public MFTestResults DrawingContext_ClippingRectangleTest15()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _pushClippingRectangle = true;
            _drawImage = true;
            bmp1 = Resources.GetBitmap(Resources.BitmapResources.Red);
            r = midX - bmp1.Width / 2;
            s = midY - bmp1.Height / 2;
            xDimension = bmp1.Width / 2;
            yDimension = bmp1.Height / 2;
            x0 = midX - xDimension / 2;
            y0 = midY - yDimension / 2;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
       new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            Log.Comment("Retrieving the Clippping Rectangle and Verifying");
            _getClippingRectangle = true;
            x1 = 0;
            y1 = 0;
            wd = 0;
            ht = 0;
            y0 = midY - yDimension / 2;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
       new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            if ((x1 != x0) || (y1 != y0) || (wd != xDimension) || (ht != yDimension))
            {
                Log.Comment("Failure in getting the clipping Rectangle");
                Log.Comment("Expected (x, y, Width, Height ) : '(" + x0 + ", " + y0 + ", " + xDimension +
                    ", " + yDimension + ")' but got '(" + x1 + ", " + y1
                    + ", " + ", " + wd + ", " + ", " + ht + ")'");
                testResult = MFTestResults.Fail;
            }
            _pushClippingRectangle = false;
            _getClippingRectangle = false;
            Log.Comment("Verifying the Image is drawn ONLY inside the Clipped Rectangle");
            Point[] chkPoint1 = GetRandomPoints_InRectangle(20, xDimension, yDimension, midX, midY);
            if (VerifyingPixelColor(chkPoint1, Colors.Red) != MFTestResults.Pass)
            {
                Log.Comment("Failure in Veifying pixel colors inside Clipped Rectangle");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Verifying Areas outside clipping rectangle are white");
            if (Test_PixelsOutsideRectangualrArea(x0, y0, xDimension, yDimension, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure in Veifying pixel colors outside Clipped Rectangle");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Poping the Clipping Rectangle and Verifying");
            _popClippingRectangle = true;
            
            Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
     new DispatcherOperationCallback(InvalidatePanel), null);
            autoEvent.WaitOne();
            
            chkPoint1 = GetRandomPoints_InRectangle(20, bmp1.Width, bmp1.Height, midX, midY);
            if (VerifyingPixelColor(chkPoint1, Colors.Red) != MFTestResults.Pass)
            {
                Log.Comment("Failure in Veifying pixel colors after poping Clipped Rectangle");
                testResult = MFTestResults.Fail;
            }
            _drawImage = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_ClippingRectangle_ArgumentException_Test16()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }

            x0 = midX / 2;
            y0 = midY / 2;
            int[] dimensions = GetRandomDimensions(midX, midY);
            for (int i = 0; i < dimensions.Length / 2; i++)
            {
                // at least one dimension shoudl be negative
                if (i == 2)
                {
                    xDimension = -dimensions[i];
                    yDimension = -dimensions[2 * i + 1];
                }
                else
                {
                    xDimension = (i % 2 == 0) ? dimensions[i] : -dimensions[i];
                    yDimension = (i % 2 != 0) ? dimensions[2 * i + 1] : -dimensions[2 * i + 1];
                }

                _argumentException = false;
                _pushClippingRectangle = true;
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(InvalidatePanel), null);
                autoEvent.WaitOne();
                if (!_argumentException)
                {
                    Log.Comment("pushing a clipping rectangle Width = " + xDimension + " and Height = " + yDimension + " didn't throw ArgumentException");
                    testResult = MFTestResults.Fail;
                }
                _pushClippingRectangle = false;
                _argumentException = false;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults DrawingContext_DrawTextTest17()
        {
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            string[] strArr2 = new string[] {"wxyz","TUVWXYZ","uvwxyz","=406lffld#'~:)*+.","852314",
"off the surface of the globe then man would only have four years of life left."};
            r = midX / 2;
            s = midY / 2;
            xDimension = 320 / 2;
            yDimension = _font.Height;
            _color = Colors.Blue;
            _alignment = TextAlignment.Left;
            _trimming = TextTrimming.None;

            return DrawText_Test(strArr2);
        }

        [TestMethod]
        public MFTestResults DrawingContext_TextTrimming_CharacterEllipsis_Test18()
        {
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            string[] strArr2 = new string[] {"uvwxyz","RSTUVWXYZ","STuvwxyz","~:)*+.",
                "47852314","d off the surface of the globe then man would only have four years of life left."};
            r = midX / 2;
            s = midY / 2;
            xDimension = 320 / 2;
            yDimension = _font.Height;
            _color = Colors.Blue;
            _alignment = TextAlignment.Left;
            _trimming = TextTrimming.CharacterEllipsis;

            return DrawText_Test(strArr2);
        }

        [TestMethod]
        public MFTestResults DrawingContext_TextTrimming_WordEllipsis_Test19()
        {
            if (ClearingPanel() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            string[] strArr2 = new string[] {"uvwxyz","RSTUVWXYZ","STuvwxyz","=406lffld#'~:)*+.",
                "47852314","disappeared off the surface of the globe then man would only have four years of life left."};
            r = midX / 2;
            s = midY / 2;
            xDimension = 320 / 2;
            yDimension = _font.Height;
            _color = Colors.Blue;
            _alignment = TextAlignment.Left;
            _trimming = TextTrimming.WordEllipsis;

            return DrawText_Test(strArr2);
        }

        string[] strArr = new string[] {"abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "aBCdEFGhijklmNOPQRSTuvwxyz", "Q1!&5%o)-=406lffld#'~:)*+.", "0123456789.0Xyz78547852314", "If the bee disappeared off the surface of " +
            "the globe then man would only have four years of life left."};

        private MFTestResults DrawText_Test(string[] strArr2)
        {
            MFTestResults tResult = MFTestResults.Pass;
            Log.Comment("Drawing the text and verifying");
            for (int i = 0; i < strArr.Length; i++)
            {
                _str = strArr[i];

                _drawText = true;
                
                Master_Media._panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
       new DispatcherOperationCallback(InvalidatePanel), null);
                autoEvent.WaitOne();
                if (_str != strArr2[i])
                {
                    Log.Comment("Expected '" + strArr2[i] + "' but got '" + _str + "'");
                    tResult = MFTestResults.Fail;
                }
                _drawText = false;
            }
            return tResult;
        }

        private MFTestResults Test_PixelsOutsideRectangualrArea(int pX, int pY, int dX, int dY, Color c)
        {
            //(p1, p2) - top left corner of the Rectangle
            // d1, d2 horizontal, vertical dimensions respectively        
            int x, y;
            Point[] chkPoint = new Point[20];
            // Generate random coordinate not inside the rectangle
            Random random = new Random();
            for (int i = 0; i < chkPoint.Length; i++)
            {
                x = random.Next(_width);
                y = random.Next(_height);
                while (((x > pX) && (x < pX + dX)) || ((y > pY) && (y < pY + dY)))
                {
                    x = random.Next(_width);
                    y = random.Next(_height);
                }
                chkPoint[i] = new Point(x, y);
            }
            return VerifyingPixelColor(chkPoint, c);
        }

        private int[] GetRandomDimensions(int maxWidth, int maxHeight)
        {
            Random random = new Random();
            int[] dims = new int[]{random.Next(maxWidth), random.Next(maxHeight),
                random.Next(maxWidth), random.Next(maxHeight),
                random.Next(maxWidth), random.Next(maxHeight)};
            return dims;
        }

        object InvalidatePanel(object obj)
        {
            Master_Media._panel.Invalidate();

            return null;
        }
    }
}
