////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Platform.Test;
using Controls.Properties;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ImageTests : Master_Controls, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Image tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults Image_DefaultConstructorTest1()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Creating new Image(), setting Image.Bitmap and Verifying");
            _default = true;
            _bitmap = Resources.GetBitmap(Resources.BitmapResources.Yellow);
            UpdateWindow();
            Point[] chkPoints = GetRandomPoints_InRectangle(20,
                _bitmap.Width, _bitmap.Height, 0, 0);
            //Yellow Color BGR = 0x00ffff
            Color _yellow = (Color)0x00ffff;

            return VerifyingPixelColor(chkPoints, _yellow);
        }

        [TestMethod]
        public MFTestResults Image_NonDefaultConstructorTest2()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Creating new Image(Bitmap), and Verifying");
            _default = false;
            _bitmap = Resources.GetBitmap(Resources.BitmapResources.Green);
            UpdateWindow();
            Point[] chkPoints = GetRandomPoints_InRectangle(20,
                _bitmap.Width, _bitmap.Height, 0, 0);

            return VerifyingPixelColor(chkPoints, Colors.Green);
        }

        [TestMethod]
        public MFTestResults Image_GetBitmapTest3()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Creating new Image(Bitmap), getting the Bitmap and Verifying");
            _default = false;
            _bitmap = Resources.GetBitmap(Resources.BitmapResources.Yellow);
            UpdateWindow();
            if (_getBitmap != _bitmap)
            {
                Log.Comment("Failure : Image.Bitmap returned diff. from what's set" +
                    " in Image constructor new Image(Bitamp)");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        Bitmap _bitmap = null, _getBitmap;
        Image _image = null;
        bool _default = false;

        object Image_UpdateWindow(object obj)
        {
            if (_default)
            {
                _image = new Image();
                _image.Bitmap = _bitmap;
            }
            else
            {
                _image = new Image(_bitmap);
            }
            _getBitmap = _image.Bitmap;
            mainWindow.Child = _image;
            return null;
        }

        void UpdateWindow()
        {
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                 new DispatcherOperationCallback(Image_UpdateWindow), null);

            _autoEvent.WaitOne();
          
            Thread.Sleep(5);
        }
    }
}
