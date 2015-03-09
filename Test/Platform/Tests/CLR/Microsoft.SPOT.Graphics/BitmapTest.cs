////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Platform.Tests
{
    public class BitmapTest : IMFTestInterface
    {
        public int width;
        public int height;
        Bitmap window = null;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            // Add your functionality here. 

            int bpp, orientation;

            try
            {
                Microsoft.SPOT.Hardware.HardwareProvider hwProvider = Microsoft.SPOT.Hardware.HardwareProvider.HwProvider;
                hwProvider.GetLCDMetrics(out width, out height, out bpp, out orientation);
                window = new Bitmap(width, height);
            }
            catch (NotSupportedException e)
            {
                Log.Comment("If we get a not supported exception that means its not supported so skip the tests. " + e.ToString());
                Log.Comment("Bitmaps are not supported for this platform so skip the tests.");
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            Debug.GC(true);
            System.Threading.Thread.Sleep(500);
        }

        [TestMethod]
        public MFTestResults Bitmap_GetBitmap()
        {
            MFTestResults result = MFTestResults.Fail;
            ArrayList bitmaps = new ArrayList();

            Log.Comment("Allocate a bitmap and then get it.");

            try
            {
                using (Bitmap window = new Bitmap(UntitledBmp.Untitled, Bitmap.BitmapImageType.Bmp))
                {

                    Log.Comment("Verify what we put in is what we get out.");
                    byte[] bitmap = window.GetBitmap();

                    byte[] convertedOriginal = ConvertOriginal(UntitledBmp.Untitled);

                    Log.Comment("Don't include the header from the original file which has an offset of: xx");
                    Log.Comment("Verify that the length of the resultant bitmap is the same as the input bitmap");
                    for (int i = 0; i < bitmap.Length; ++i)
                    {
                        if (bitmap[i] != convertedOriginal[i])
                        {
                            Log.Comment("The lengths are the same but the input image is different from the resultant image at position: " + i.ToString());
                            return result;
                        }
                    }
                    result = MFTestResults.Pass;
                }
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception: " + e.ToString());
                result = MFTestResults.Fail;
            }

            return result;
        }

        private byte[] ConvertOriginal(byte[] originalImage)
        {
            const int HeaderSize = 54;

            using (Bitmap window = new Bitmap(originalImage, Bitmap.BitmapImageType.Bmp))
            {

                int width = window.Width;
                int height = window.Height;
                int rowStride = 3 * width;

                byte[] convertedOriginal = new byte[width * height * 4];

                int addPaddingOffset = 0;
                int removeNullsCount = 0;
                for (int y = 0; y < height; ++y)
                {
                    int rowId = y * rowStride;
                    for (int x = 0; x < rowStride; ++x)
                    {
                        //add the least significant byte as padding.
                        if ((x != 0) && (x + rowId) % 3 == 0)
                        {
                            ++addPaddingOffset;
                            convertedOriginal[x + rowId + addPaddingOffset] = 0x00;
                        }

                        convertedOriginal[x + rowId + addPaddingOffset] = originalImage[HeaderSize + x + rowId + removeNullsCount];

                        //remove the double nulls at the ends of the rowstrides.
                        if (x + 1 == rowStride)
                        {
                            removeNullsCount += 3;
                            //Add a null at the end of the rowstride
                            ++addPaddingOffset;
                            convertedOriginal[x + rowId + addPaddingOffset] = 0x00;

                        }
                    }
                }
                return convertedOriginal;
            }
        }

        internal class ScreenSize
        {
            public ScreenSize(int w, int h)
            {
                this.width = w;
                this.height = h;
            }

            public int width;
            public int height;
        }

        private static void Scale9ImageManaged(Bitmap bmpDest, int xDst, int yDst, int widthDst, int heightDst, Bitmap bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity)
        {
            if (widthDst < leftBorder || heightDst < topBorder)
                return;
            int widthSrc = bitmap.Width;
            int heightSrc = bitmap.Height;
            int centerWidthSrc = widthSrc - (leftBorder + rightBorder);
            int centerHeightSrc = heightSrc - (topBorder + bottomBorder);
            int centerWidthDst = widthDst - (leftBorder + rightBorder);
            int centerHeightDst = heightDst - (topBorder + bottomBorder);

            //top-left
            //if(widthDst >= leftBorder && heightDst >= topBorder)
            bmpDest.StretchImage(xDst, yDst, leftBorder, topBorder, bitmap, 0, 0, leftBorder, topBorder, opacity);

            //top-right
            if (widthDst > leftBorder /*&& heightDst >= topBorder*/)
                bmpDest.StretchImage(xDst + widthDst - rightBorder, yDst, rightBorder, topBorder, bitmap,
                                     widthSrc - rightBorder, 0, rightBorder, topBorder, opacity);
            //bottom-left
            if (/*widthDst >= leftBorder && */heightDst > topBorder)
                bmpDest.StretchImage(xDst, yDst + heightDst - bottomBorder, leftBorder, bottomBorder, bitmap,
                                     0, heightSrc - bottomBorder, leftBorder, bottomBorder, opacity);
            //bottom-right
            if (widthDst > leftBorder && heightDst > topBorder)
                bmpDest.StretchImage(xDst + widthDst - rightBorder, yDst + heightDst - bottomBorder, rightBorder, bottomBorder, bitmap,
                                     widthSrc - rightBorder, heightSrc - bottomBorder, rightBorder, bottomBorder, opacity);

            //left
            if (/*widthDst >= leftBorder &&*/ centerHeightDst > 0)
                bmpDest.StretchImage(xDst, yDst + topBorder, leftBorder, centerHeightDst, bitmap,
                                     0, topBorder, leftBorder, centerHeightSrc, opacity);

            //top
            if (centerWidthDst > 0 /*&& heightDst >= topBorder*/)
                bmpDest.StretchImage(xDst + leftBorder, yDst, centerWidthDst, topBorder, bitmap,
                                     leftBorder, 0, centerWidthSrc, topBorder, opacity);

            //right
            if (widthDst > leftBorder && centerHeightDst > 0)
                bmpDest.StretchImage(xDst + widthDst - rightBorder, yDst + topBorder, rightBorder, centerHeightDst, bitmap,
                                     widthSrc - rightBorder, topBorder, rightBorder, centerHeightSrc, opacity);

            //bottom
            if (centerWidthDst > 0 && heightDst > topBorder)
                bmpDest.StretchImage(xDst + leftBorder, yDst + heightDst - bottomBorder, centerWidthDst, bottomBorder, bitmap,
                                     leftBorder, heightSrc - bottomBorder, centerWidthSrc, bottomBorder, opacity);

            //center
            if (centerWidthDst > 0 && centerHeightDst > 0)
                bmpDest.StretchImage(xDst + leftBorder, yDst + topBorder, centerWidthDst, centerHeightDst, bitmap,
                                     leftBorder, topBorder, centerWidthSrc, centerHeightSrc, opacity);

        }

        [TestMethod]
        public MFTestResults Scale9ImageTest()
        {
            Bitmap screen = new Bitmap(width, height);

            Bitmap btn = new Bitmap(WaterFallJpg.WaterFall, Bitmap.BitmapImageType.Jpeg);

            btn.MakeTransparent(ColorUtility.ColorFromRGB(255, 0, 255));

            screen.DrawRectangle(Color.White, 0, 0, 0, width, height, 0, 0, Color.White, 0, 0, 0, 0, 0, 256);

            screen.Scale9Image(30, 30, width / 3, height / 3, btn, 6, 6, 6, 6, 256);
            screen.Scale9Image(width / 2, height / 2, width / 3, 30, btn, 6, 6, 6, 6, 256);
            screen.Scale9Image(30, height / 2, 30, height / 3, btn, 6, 6, 6, 6, 256);
            screen.Scale9Image(width / 2, 30, 30, 30, btn, 6, 6, 6, 6, 256);

            screen.Flush();

            System.Threading.Thread.Sleep(500);

            screen.Dispose();
            btn.Dispose();

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults StretchImage()
        {
            Bitmap screen = new Bitmap(width, height);

            Bitmap btn = new Bitmap(WaterFallJpg.WaterFall, Bitmap.BitmapImageType.Jpeg);
            
            screen.StretchImage(10, 10, 15, 75,
                                    btn,
                                    btn.Width / 2, 0, btn.Width / 2, btn.Height / 2,
                                    256);

            screen.Flush();

            screen.StretchImage(20, 20, 30, 150,
                                    btn,
                                    btn.Width / 2, 0, btn.Width / 2, btn.Height / 2,
                                    256);
            screen.Flush();

            screen.StretchImage(40, 40, 60, 250,
                                    btn,
                                    btn.Width / 2, 0, btn.Width / 2, btn.Height / 2,
                                    256);
            screen.Flush();

            System.Threading.Thread.Sleep(500);

            screen.Dispose();
            btn.Dispose();

            return MFTestResults.Pass;
        }
        
        [TestMethod]
        public MFTestResults TileImage()
        {
            Bitmap screen = new Bitmap(width, height);

            Bitmap btn = new Bitmap(WaterFallJpg.WaterFall, Bitmap.BitmapImageType.Jpeg);
            
            screen.DrawRectangle(Color.White, 0, 0, 0, width, height, 0, 0, Color.White, 0, 0, 0, 0, 0, 256);

            screen.TileImage( 5, 10, btn, width - 50, height / 2, 256);

            screen.Flush();

            System.Threading.Thread.Sleep(500);

            screen.Dispose();
            btn.Dispose();

            return MFTestResults.Pass;
        }
     
        [TestMethod]
        public MFTestResults LargeBitmapTest()
        {

            MFTestResults result = MFTestResults.Fail;

            ScreenSize[] pairs = new ScreenSize[] { 
                                                    new ScreenSize( 160, 120 ), // QQVGA
                                                    new ScreenSize( 240, 160 ), // HQVGA
                                                    new ScreenSize( 320, 240 ), // QVGA
                                                    new ScreenSize( 480, 320 ), // HVGA
                                                    new ScreenSize( 640, 240 ), // HVGA
                                                    new ScreenSize( 864, 480 ), // FWVGA
                                                    new ScreenSize( 640, 480 ), // VGA 
                                                    new ScreenSize( 800, 480 ), // WVGA
                                                    new ScreenSize( 848, 480 ), // WVGA
                                                    new ScreenSize( 854, 480 ), // WVGA
                                                    //new ScreenSize( 1024, 768 ), // XVGA
                                                    //new ScreenSize( 1152, 864 ), // XVGA+
                                                    //new ScreenSize( 1280, 1024 ), // SXGA
                                                    //new ScreenSize( 1400, 1050 ), // SXGA+
                                                    //new ScreenSize( 1600, 1200 ), // UXGA
                                                  };
            Bitmap bmp = null;
            try
            {

                foreach (ScreenSize p in pairs)
                {
                    try
                    {
                        // the CLR's limit of allocation in the managed heap is sizeof(CLR_RT_HeapBlock) * (2^16) = 768432 bytes. 
                        // To that one needs to subtract the size of an heap cluster (sizeof(CLR_RT_heapCluster)) and 
                        // round to the number of heap blocks in the remaining amount, since the CLR allocated in heap blocks units. 
                        // Thus some allocation (e.g. 800x600) must go in the unmanaged SimpleHeap if supported.

                        bmp = new Bitmap(p.width, p.height);
                    }
                    catch (OutOfMemoryException)
                    {
                        //SimpleHeap has no legitimate API to distinguish between no implementation
                        //or implemented but out-of-memory condition occurred. If OOM is thrown, assume
                        //it is because no SimpleHeap was provided.
                        // On the emulator though, we know it is implemented
                        if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
                        {
                            return MFTestResults.Fail;
                        }
                    }

                    bmp.DrawLine(Color.White, 5, 0, 0, bmp.Width, bmp.Height);
                    bmp.DrawEllipse(Color.White, 4, 50, 50, 40, 11, Color.White, 0, 0, Color.Black, bmp.Width, bmp.Height, 5);

                    window.StretchImage(0, 0, bmp, width, height, 0xFF);
                    window.Flush();

                    bmp.Dispose(); bmp = null; // this is particlarly needed for the allocation in the non-managed heap   
                }

                result = MFTestResults.Pass;
            }
            finally
            {
                if (bmp != null)
                {
                    bmp.Dispose();
                }
                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            return result;
        }


        [TestMethod]
        public MFTestResults BitmapStressTest_RotateImage()
        {
            MFTestResults result = MFTestResults.Fail;
            
            Log.Comment("Rotate bitmaps");
            
            Bitmap bmpSrc = null;
            Bitmap bmpDst = null;
            
            try
            {
                bmpDst = new Bitmap(width, height);

                bmpSrc = new Bitmap(WaterFallJpg.WaterFall, Bitmap.BitmapImageType.Jpeg);
                bmpDst.StretchImage(0, 0, bmpSrc, bmpSrc.Width, bmpSrc.Height, 0x0100);
                bmpDst.Flush();

                System.Threading.Thread.Sleep(200);

                
                for (int i = 1; i < 4; ++i )
                {
                    bmpDst.RotateImage(90 * i , 0, 0, bmpSrc, 0, 0, bmpSrc.Width, bmpSrc.Height, 0xFFFF);
                    bmpDst.Flush();

                    System.Threading.Thread.Sleep(200);
                }

                result = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Exception( "Caught exception: " + e.Message);
            }
            finally
            {
                if(bmpSrc != null)
                {
                    bmpSrc.Dispose();
                }
                if(bmpDst != null)
                {
                    bmpDst.Dispose();
                }
                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            return result;
        }


        [TestMethod]
        public MFTestResults BitmapStressTest_DrawImage()
        {
            MFTestResults result = MFTestResults.Fail;
            ArrayList bitmaps = new ArrayList();

            Log.Comment("Allocate as many bitmaps as possible");

            try
            {
                while (true)
                {
                    Bitmap bmp = new Bitmap(width, height);
                    bmp.DrawLine(Color.White, 5, 0, 0, bmp.Width, bmp.Height);
                    bmp.DrawEllipse(Color.White, 4, 50, 50, 40, 11, Color.White, 0, 0, Color.Black, bmp.Width, bmp.Height, 5);

                    bitmaps.Add(bmp);

                    window.DrawImage(0, 0, bmp, 0, 0, width, height, 0xFF);
                    window.Flush();
                }
            }
            catch (OutOfMemoryException e)
            {
                Log.Comment("Correctly threw exception: " + e.ToString());
                result = MFTestResults.Pass;
            }
            finally
            {
                foreach (Bitmap b in bitmaps)
                {
                    b.Dispose();
                }
                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            Log.Comment("Total Bitmap count = " + bitmaps.Count.ToString());

            return result;
        }

        [TestMethod]
        public MFTestResults BitmapStressTest_StretchImage()
        {
            MFTestResults result = MFTestResults.Fail;
            ArrayList bitmaps = new ArrayList();

            Log.Comment("Allocate as many bitmaps as possible");

            try
            {
                while (true)
                {
                    Bitmap bmp = new Bitmap(width, height);
                    bmp.DrawLine(Color.White, 5, 0, 0, bmp.Width, bmp.Height);
                    bmp.DrawEllipse(Color.White, 4, 50, 50, 40, 11, Color.White, 0, 0, Color.Black, bmp.Width, bmp.Height, 5);

                    bitmaps.Add(bmp);

                    window.StretchImage(0, 0, bmp, width, height, 0xFF);
                    window.Flush();
                }
            }
            catch (OutOfMemoryException e)
            {
                Log.Comment("Correctly threw exception: " + e.ToString());
                result = MFTestResults.Pass;
            }
            finally
            {
                foreach (Bitmap b in bitmaps)
                {
                    b.Dispose();
                }
                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            Log.Comment("Total Bitmap count = " + bitmaps.Count.ToString());

            return result;
        }

        [TestMethod]
        public MFTestResults BitmapSuperStressTest_StretchImage()
        {
            MFTestResults result = MFTestResults.Pass;
            ArrayList bitmaps = new ArrayList();
            Random random = new Random();

            Log.Comment("Allocate as many bitmaps as possible");
            for (int i = 0; i < 10; ++i)
            {
                try
                {
                    while (true)
                    {
                        Bitmap bmp = new Bitmap(width, height);
                        bmp.DrawLine(Color.White, 5, 0, 0, bmp.Width, bmp.Height);
                        bmp.DrawEllipse(Color.White, 4, 50, 50, 40, 11, Color.White, 0, 0, Color.Black, bmp.Width, bmp.Height, 5);

                        bitmaps.Add(bmp);

                        window.StretchImage(0, 0, bmp, width, height, 0xFF);
                        window.Flush();
                    }
                }
                catch (OutOfMemoryException)
                {
                    while (bitmaps.Count > 2)
                        bitmaps.RemoveAt(random.Next(bitmaps.Count));
                }
                catch (Exception e)
                {
                    Log.Comment("Incorrect exception caught: " + e.ToString());
                    result = MFTestResults.Fail;
                }
                finally
                {
                    foreach (Bitmap b in bitmaps)
                    {
                        b.Dispose();
                    }
                }

                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            Log.Comment("Total Bitmap count = " + bitmaps.Count.ToString());

            return result;
        }

        [TestMethod]
        public MFTestResults GifStressTest_DrawImage()
        {
            MFTestResults result = MFTestResults.Fail;
            ArrayList bitmaps = new ArrayList();

            Log.Comment("Allocate as many gifs as possible");

            try
            {
                while (true)
                {

                    Bitmap bmp = new Bitmap(PandaGif.Panda, Bitmap.BitmapImageType.Gif);

                    bitmaps.Add(bmp);

                    window.DrawImage(0, 0, bmp, 0, 0, width, height, 0xFF);
                    window.Flush();
                }
            }
            catch (OutOfMemoryException e)
            {
                Log.Comment("Correctly threw exception: " + e.ToString());
                result = MFTestResults.Pass;
            }
            finally
            {
                foreach (Bitmap b in bitmaps)
                {
                    b.Dispose();
                }

                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            Log.Comment("Total Gif count = " + bitmaps.Count.ToString());

            return result;
        }

        [TestMethod]
        public MFTestResults GifStressTest_StretchImage()
        {
            MFTestResults result = MFTestResults.Pass;
            ArrayList bitmaps = new ArrayList();
            Random random = new Random();

            Log.Comment("Allocate as many gifs as possible");

            try
            {
                for (int i = 0; i < 10; ++i)
                {
                    try
                    {
                        while (true)
                        {
                            Bitmap bmp = new Bitmap(PandaGif.Panda, Bitmap.BitmapImageType.Gif);

                            bitmaps.Add(bmp);

                            window.StretchImage(0, 0, bmp, width, height, 0xFF);
                            window.Flush();
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        int totalBmpsToRemove = 10;
                        while (bitmaps.Count > 20 && totalBmpsToRemove > 0)
                        {
                            int idx = random.Next(bitmaps.Count);
                            ((Bitmap)bitmaps[idx]).Dispose();
                            bitmaps.RemoveAt(idx);
                            totalBmpsToRemove--;
                        }

                        Debug.GC(true);
                        System.Threading.Thread.Sleep(500);
                    }
                    catch (Exception e)
                    {
                        Log.Comment("Incorrect exception caught: " + e.ToString());
                        result = MFTestResults.Fail;
                    }
                }
            }
            finally
            {
                foreach (Bitmap b in bitmaps)
                {
                    b.Dispose();
                }
                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            Log.Comment("Total Gif count = " + bitmaps.Count.ToString());

            return result;
        }

        [TestMethod]
        public MFTestResults JpgStressTest_DrawImage()
        {
            MFTestResults result = MFTestResults.Fail;
            ArrayList bitmaps = new ArrayList();

            Log.Comment("Allocate as many jpgs as possible");

            try
            {
                while (true)
                {
                    Bitmap bmp = new Bitmap(WaterFallJpg.WaterFall, Bitmap.BitmapImageType.Jpeg);

                    bitmaps.Add(bmp);

                    window.DrawImage(0, 0, bmp, 0, 0, width, height, 0xFF);
                    window.Flush();
                }
            }
            catch (OutOfMemoryException e)
            {
                Log.Comment("Correctly threw exception: " + e.ToString());
                result = MFTestResults.Pass;
            }
            finally
            {
                foreach (Bitmap b in bitmaps)
                {
                    b.Dispose();
                }
                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            Log.Comment("Total Jpeg count = " + bitmaps.Count.ToString());

            return result;
        }

        [TestMethod]
        public MFTestResults JpgStressTest_StretchImage()
        {
            MFTestResults result = MFTestResults.Fail;
            ArrayList bitmaps = new ArrayList();

            Log.Comment("Allocate as many jpgs as possible");

            try
            {
                while (true)
                {
                    Bitmap bmp = new Bitmap(WaterFallJpg.WaterFall, Bitmap.BitmapImageType.Jpeg);

                    bitmaps.Add(bmp);

                    window.StretchImage(0, 0, bmp, width, height, 0xFF);
                    window.Flush();
                }
            }
            catch (OutOfMemoryException e)
            {
                Log.Comment("Correctly threw exception: " + e.ToString());
                result = MFTestResults.Pass;
            }
            finally
            {
                foreach (Bitmap b in bitmaps)
                {
                    b.Dispose();
                }
                Debug.GC(true);
                System.Threading.Thread.Sleep(500);
            }

            Log.Comment("Total Jpeg count = " + bitmaps.Count.ToString());

            return result;
        }

        [TestMethod]
        public MFTestResults DisposedBitmapTest()
        {
            MFTestResults result = MFTestResults.Fail;

            Log.Comment("*** Space Left: " + Debug.GC(true) + " ***");

            Bitmap bmp = new Bitmap(width, height);
            bmp.Dispose();

            try
            {
                int w = bmp.Width;
            }
            catch (ObjectDisposedException)
            {
                result = MFTestResults.Pass;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Bitmap_FlushSmallBitmapTest()
        {
            Log.Comment("Creating Bitmap with Width & Height that of LCD.");
            Log.Comment("Flushing part of Bitmap");
            Log.Comment("Bitmap.Flush() doesn't change the content of Bitmap itself");
            Log.Comment("Need to verify LCD framebuffer, verification NYI");
            Bitmap bmp = null;
            try
            {
                bmp = new Bitmap(width, height);
                Color _redColor = (Color)255;
                //Documentation says opacity = 0xff is completely opaque but not, it's 256 bug 22349
                window.DrawRectangle(_redColor, 1, 0, 0, width, height, 0, 0, _redColor, 0, 0, _redColor, width, height, 256);
                int x0 = width / 4, y0 = height / 4, wd = width / 2, ht = height / 2;
                window.Flush(x0, y0, wd, ht);
                //int x, y;
                //for (int i = 0; i < window.Width; i++)
                //{
                //    x = i;
                //    y = (i * height) / width;
                //    Color _gotColor = bmp.GetPixel(x, y);
                //    if ((x < x0 && y < y0) && (_gotColor != Color.Black))
                //    {
                //        Log.Comment("Failure outside flushed area : expected color '" + Color.Black +
                //            "' but got '" + _gotColor + "' at '(" + x + ", " + y + ")'");
                //    }
                //    if (((x > x0 && x < (3 * x0)) && (y > y0 && y < (3 * y0)))
                //        && (_gotColor != _redColor))
                //    {
                //        Log.Comment("Failure inside flushed area : expected color '" + _redColor +
                //            "' but got '" + _gotColor + "' at '(" + x + ", " + y + ")'");
                //    }
                //    if ((x > (3 * x0) && y > (3 * y0)) && (_gotColor != Color.Black))
                //    {
                //        Log.Comment("Failure outside flushed area : expected color '" + Color.Black +
                //            "' but got '" + _gotColor + "' at '(" + x + ", " + y + ")'");
                //    }
                //}
            }
            finally
            {
                window.Dispose();
            }

            return MFTestResults.Skip;
        }
    }
}
