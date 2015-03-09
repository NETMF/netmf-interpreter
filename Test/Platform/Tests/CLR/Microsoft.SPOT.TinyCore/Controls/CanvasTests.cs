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
    public class CanvasTests : Master_Controls, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Canvas tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults Canvas_DefaultSetTest1()
        {
            ///<summary>
            ///1. Creates a new Canvas with dimensions less than the Window
            ///2. Verifies the Canvas' child is placed at the top-left corner of the Canvas
            ///</summary>
            ///

            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            UpdateWindow();
            Log.Comment("Verifying the Canvas' child elt is by default set at Top - Left");
            Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                _cXStart, _cYStart);
            if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
            {
                Log.Comment("Failure in verifying the default position of a child elt of Canvas is Top-Left");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_SetTopTest2()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Point[] chkPoints;
            Log.Comment("Canavs.SetTop(UIElt, i), for i = 0,1, ... and Verifying");
            _top = true;
            for (int i = 0; i < 7; i++)
            {
                _tp = i;
                Log.Comment("Canvas.SetTop(UIElt, " + i + ") and verifying");
                UpdateWindow();
                chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                    _cXStart, _cYStart + i);
                if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying child elt is Set at " + i + " from Canvas Top");
                    testResult = MFTestResults.Fail;
                }
            }

            Log.Comment("Canavs.SetTop(UIElt, i), for i = -16, ... ,-4, -2 and Verifying");
            for (int i = -16; i < 0; i += 2)
            {
                _tp = i;
                Log.Comment("Canvas.SetTop(UIElt, " + i + ") and verifying");
                UpdateWindow();
                chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH + i,
                    _cXStart, _cYStart);
                if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying child elt is Set at " + i + " from Canvas Top");
                    testResult = MFTestResults.Fail;
                }
            }
            _top = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_SetBottomTest3()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Canavs.SetBottom(UIElt, i), for i = 0,1, ... and Verifying");
            _bottom = true;
            for (int i = 0; i < 7; i++)
            {
                _bt = i;
                Log.Comment("Canvas.SetBottom(UIElt, " + i + ") and verifying");
                UpdateWindow();
                Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                    _cXStart, _cYStart + _cHeight - _rectH - i);
                if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
                {
                    Log.Comment("Failure in verifying child elt is Set at " + i + "from Canvas Bottom");
                    testResult = MFTestResults.Fail;
                }
            }
            Log.Comment("Canavs.SetBottom(UIElt, i), for i = -16, ...-4, -2 and Verifying");
            for (int i = -16; i < 0; i++)
            {
                _bt = i;
                Log.Comment("Canvas.SetBottom(UIElt, " + i + ") and verifying");
                UpdateWindow();
                Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH + i,
                    _cXStart, _cYStart + _cHeight - _rectH - i);
                if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
                {
                    Log.Comment("Failure in verifying child elt is Set at " + i + "from Canvas Bottom");
                    testResult = MFTestResults.Fail;
                }
            }
            _bottom = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_SetLeftTest4()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Canavs.SetLeft(UIElt, i), for i = 0,1, ... and Verifying");
            _left = true;
            for (int i = 0; i < 7; i++)
            {
                _lt = i;
                Log.Comment("Canvas.SetLeft(UIElt, " + i + ") and verifying");
                UpdateWindow();
                Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                    (_cXStart +  i), _cYStart);
                if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying child elt is Set at " + i + " from Canvas Left");
                    testResult = MFTestResults.Fail;
                }
            }
            Log.Comment("Canavs.SetLeft(UIElt, i), for i = -16,...-4,-2 and Verifying");
            for (int i = -16; i < 0; i += 2)
            {
                _lt = i;
                Log.Comment("Canvas.SetLeft(UIElt, " + i + ") and verifying");
                UpdateWindow();
                Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW + i, _rectH,
                    _cXStart, _cYStart);
                if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying child elt is Set at " + i + " from Canvas Left");
                    testResult = MFTestResults.Fail;
                }
            }
            _left = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_SetRightTest5()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Canavs.SetRight(UIElt, i), for i = 0,1, ... and Verifying");
            _right = true;
            for (int i = 0; i < 7; i++)
            {
                _rt = i;
                Log.Comment("Canvas.SetRight(UIElt, " + i + ") and verifying");
                UpdateWindow();
                Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                    _cXStart + _cWidth - _rectW - i, _cYStart);
                if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying child elt is Set at " + i + " from Canvas Right");
                    testResult = MFTestResults.Fail;
                }
            }
            Log.Comment("Canavs.SetRight(UIElt, i), for i = -16, ... -4, -2 and Verifying");
            for (int i = -16; i < 0; i += 2)
            {
                _rt = i;
                Log.Comment("Canvas.SetRight(UIElt, " + i + ") and verifying");
                UpdateWindow();
                Point[] chkPoints = GetRandomPoints_InRectangle(20, _rectW + i, _rectH,
                    _cXStart + _cWidth - _rectW - i, _cYStart);
                if (VerifyingPixelColor(chkPoints, _fillColor) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying child elt is Set at " + i + " from Canvas Right");
                    testResult = MFTestResults.Fail;
                }
            }
            _right = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_GetTopTest6()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Canavs.SetTop(UIElt, i), for any i and Verifying i = Canvas.GetTop(UIElt)");
            _top = true;

            return Canvas_GetTest(ref  gTop, ref _tp, ref  _top);
        }

        [TestMethod]
        public MFTestResults Canvas_GetBottomTest7()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Canavs.SetBottom(UIElt, i), for any i and Verifying i = Canvas.GetBottom(UIElt)");
            _bottom = true;

            return Canvas_GetTest(ref  gBottom, ref _bt, ref  _bottom);
        }

        [TestMethod]
        public MFTestResults Canvas_GetLeftTest8()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Canavs.SetLeft(UIElt, i), for any i and Verifying i = Canvas.GetLeft(UIElt)");
            _left = true;

            return Canvas_GetTest(ref  gLeft, ref _lt, ref  _left);
        }

        [TestMethod]
        public MFTestResults Canvas_GetRightTest9()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Canavs.SetRight(UIElt, i), for any i and Verifying i = Canvas.GetRight(UIElt)");
            _right = true;

            return Canvas_GetTest(ref  gRight, ref _rt, ref  _right);
        }

        [TestMethod]
        public MFTestResults Canvas_SetTop_NullReferenceExceptionTest10()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Canvas.SetTop(null, i), verifying NullReferenceException is thrown");
            if (!NullReferenceExceptionTest(ref _top, ref _tp))
            {
                Log.Comment("NullReferenceException not thrown upon Canvas.SetTop(null, " + _tp + ")");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_SetRight_NullReferenceExceptionTest11()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Canvas.SetRight(null, i), verifying NullReferenceException is thrown");
            if (!NullReferenceExceptionTest(ref _right, ref _rt))
            {
                Log.Comment("NullReferenceException not thrown upon Canvas.SetRight(null, " + _rt + ")");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_SetBottom_NullReferenceExceptionTest12()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Canvas.SetBottom(null, i), verifying NullReferenceException is thrown");
            if (!NullReferenceExceptionTest(ref _bottom, ref _bt))
            {
                Log.Comment("NullReferenceException not thrown upon Canvas.SetBottom(null, " + _bt + ")");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_SetLeft_NullReferenceExceptionTest13()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Canvas.SetLeft(null, i), verifying NullReferenceException is thrown");
            if (!NullReferenceExceptionTest(ref _left, ref _lt))
            {
                Log.Comment("NullReferenceException not thrown upon Canvas.SetLeft(null, " + _lt + ")");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Canvas_SetAllTest14()
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Point[] chkPoints;
            Log.Comment("Creates a Canvas, Sets Children in all positions and verifies all are set");

            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(Canvas_AllSet), null);
            
            _autoEvent.WaitOne();

            Thread.Sleep(5);

            int top    = vLength;
            int bottom = _cHeight - vLength + (1 == ((_midY | _height) & 1) ? 1 : 0);
            int left   = hLength;
            int right = _cWidth - hLength + (1 == ((_midX | _width) & 1) ? 1 : 0);

            Log.Comment("Verifying the 1st elt is set at Top = " + vLength + ", Left = " + hLength);
            chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                left, top);
            if (VerifyingPixelColor(chkPoints, c1) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying the element set at Top = " + vLength + ", Left = " + hLength);
                testResult = MFTestResults.Fail;
            }

            Log.Comment("Verifying the 2nd elt is Set at Top = " + vLength + ", Right =" + hLength);
            chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                right, top);
            if (VerifyingPixelColor(chkPoints, c2) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying the element set at Top = " + vLength + ", Right =" + hLength);
                testResult = MFTestResults.Fail;
            }

            Log.Comment("Verifying the 3rd elt is Set at Bottom = " + vLength + ", Left = " + hLength);
            chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                left, bottom);
            if (VerifyingPixelColor(chkPoints, c3) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying the element set at Bottom = " + vLength + ", Left = " + hLength);
                testResult = MFTestResults.Fail;
            }

            Log.Comment("Verifying the 4th elt is Set at Bottom = " + vLength + ", Right = " + hLength);
            chkPoints = GetRandomPoints_InRectangle(20, _rectW, _rectH,
                right, bottom);
            if (VerifyingPixelColor(chkPoints, c4) != MFTestResults.Pass)
            {
                Log.Comment("Failure verifying the element set at Bottom = " + vLength + ", Right = " + hLength);
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        //Canvas Width & Height
        static readonly int _cWidth  = (3 * _width ) / 4;
        static readonly int _cHeight = (3 * _height) / 4;
        //Calculating Canvas Top-Left coordinates
        static readonly int _cXStart = (_width  - _cWidth ) / 2;
        static readonly int _cYStart = (_height - _cHeight) / 2;
        //variables to store Canvas.GetXxxx(UIElement)
        int gLeft, gTop, gRight, gBottom;
        //Rectangle Width & Height
        readonly int _rectW = _midX / 2;
        readonly int _rectH = _midY / 2;

        int _lt, _tp, _rt, _bt;

        readonly int hLength = _cWidth  - _midX;
        readonly int vLength = _cHeight - _midY;

        bool _left = false, _top = false, _right = false, _bottom = false, _null = false;
        Canvas _canvas = null;
        Rectangle _rect1 = null;
        Color _fillColor = Colors.Green;
        readonly Color c1 = Colors.Blue;
        readonly Color c2 = Colors.Red;
        readonly Color c3 = Colors.Green;
        readonly Color c4 = Colors.Gray;

        protected override void Reset()
        {
            //variables to store Canvas.GetXxxx(UIElement)
            gLeft = 0;
            gTop = 0;
            gRight = 0;
            gBottom = 0;
            //Rectangle Width & Height
            _lt = 0;
            _tp = 0;
            _rt = 0;
            _bt = 0;
            _left = false;
            _top = false;
            _right = false;
            _bottom = false;
            _null = false;
            _canvas = null;
            _rect1 = null;
            _fillColor = Colors.Green;
        }

        object Canvas_UpdateWindow(object obj)
        {
            _canvas = new Canvas();
            //Setting the Width & Height of the Canvas
            //by default the Canvas is at the center both from Horizontal and Vertical
            _canvas.Width = _cWidth;
            _canvas.Height = _cHeight;
            if (!_null)
            {
                _rect1 = new Rectangle();
                _rect1.Width = _rectW;
                _rect1.Height = _rectH;
                _rect1.Fill = new SolidColorBrush(_fillColor);
            }
            if (_left)
            {
                try
                {
                    Canvas.SetLeft(_rect1, _lt);
                    gLeft = Canvas.GetLeft(_rect1);
                }
                catch (Exception ex)
                {
                    Log.Comment("Caught " + ex.Message + " when Canvas.SetLeft(UIElemnt, " + _lt + ")");
                    _argumentException = true;
                }
            }
            else if (_top)
            {
                try
                {
                    Canvas.SetTop(_rect1, _tp);
                    gTop = Canvas.GetTop(_rect1);
                }
                catch (Exception ex)
                {
                    Log.Comment("Caught " + ex.Message + " when Canvas.SetTop(UIElemnt, " + _tp + ")");
                    _argumentException = true;
                }
            }
            else if (_right)
            {
                try
                {
                    Canvas.SetRight(_rect1, _rt);
                    gRight = Canvas.GetRight(_rect1);
                }
                catch (Exception ex)
                {
                    Log.Comment("Caught " + ex.Message + " when Canvas.SetRight(UIElemnt, " + _rt + ")");
                    _argumentException = true;
                }

            }
            else if (_bottom)
            {
                try
                {
                    Canvas.SetBottom(_rect1, _bt);
                    gBottom = Canvas.GetBottom(_rect1);
                }
                catch (Exception ex)
                {
                    Log.Comment("Caught " + ex.Message + " when Canvas.SetBottom(UIElemnt, " + _bt + ")");
                    _argumentException = true;
                }
            }
            if (_rect1 != null)
            {
                _canvas.Children.Add(_rect1);
            }
            mainWindow.Child = _canvas;
            return null;
        }

        object Canvas_AllSet(object obj)
        {
            _canvas = new Canvas();

            Rectangle _rect1 = new Rectangle();
            _rect1.Width = _rectW;
            _rect1.Height = _rectH;
            _rect1.Fill = new SolidColorBrush(c1);
            Canvas.SetLeft(_rect1, hLength);
            Canvas.SetTop(_rect1, vLength);

            Rectangle _rect2 = new Rectangle();
            _rect2.Width = _rectW;
            _rect2.Height = _rectH;
            _rect2.Fill = new SolidColorBrush(c2);
            Canvas.SetRight(_rect2, hLength);
            Canvas.SetTop(_rect2, vLength);

            Rectangle _rect3 = new Rectangle();
            _rect3.Width = _rectW;
            _rect3.Height = _rectH;
            _rect3.Fill = new SolidColorBrush(c3);
            Canvas.SetLeft(_rect3, hLength);
            Canvas.SetBottom(_rect3, vLength);

            Rectangle _rect4 = new Rectangle();
            _rect4.Width = _rectW;
            _rect4.Height = _rectH;
            _rect4.Fill = new SolidColorBrush(c4);
            Canvas.SetRight(_rect4, hLength);
            Canvas.SetBottom(_rect4, vLength);

            _canvas.Children.Add(_rect1);
            _canvas.Children.Add(_rect2);
            _canvas.Children.Add(_rect3);
            _canvas.Children.Add(_rect4);

            mainWindow.Child = _canvas;
            return null;
        }

        private bool NullReferenceExceptionTest(ref bool alignemt, ref int length)
        {
            _argumentException = false;
            _rect1 = null;
            _null = true;
            alignemt = true;
            length = new Random().Next(100);
            UpdateWindow();
            return _argumentException;
        }

        private MFTestResults Canvas_GetTest(ref int _get, ref int _set, ref bool alignment)
        {
            MFTestResults tResult = MFTestResults.Pass;
            Random random = new Random();
            int[] lengths = new int[] {int.MinValue, -random.Next(100),
                random.Next(100), int.MaxValue };
            for (int i = 0; i < lengths.Length; i++)
            {
                _set = lengths[i];
                Log.Comment("Canvas.SetXxxx(UIElt, " + _set + ")");
                UpdateWindow();
                Thread.Sleep(c_wait);
                if (_get != _set)
                {
                    Log.Comment("Failure, Expected Canvas.Xxxx(UIElt) = '" +
                        _set + "' but got '" + _get + "'");
                    tResult = MFTestResults.Fail;
                }
            }
            alignment = false;

            return tResult;
        }

        void UpdateWindow()
        {
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(Canvas_UpdateWindow), null);

            _autoEvent.WaitOne();

            Thread.Sleep(5);
        }
    }
}
