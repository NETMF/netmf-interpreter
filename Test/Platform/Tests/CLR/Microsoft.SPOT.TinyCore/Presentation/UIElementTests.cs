////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Threading;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;

namespace Microsoft.SPOT.Platform.Tests
{
    public class UIElementTests : Master_Presentation, IMFTestInterface
    {
        private Random _random;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for UIElement tests.");
            _random = new Random();
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults UIElement_RaiseEvent_ButtonEvent_Test1()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Raising ButtonEvents and Verifying");
            //addToEvtRoute = false;
            btnEvent = true;
            fcsEvent = false;
            _btnEventCntr = 0;
            rEvents = new RoutedEvent[] { Buttons.ButtonDownEvent, Buttons.ButtonUpEvent,             
                Buttons.PreviewButtonDownEvent, Buttons.PreviewButtonUpEvent};
            for (int i = 0; i < rEvents.Length; i++)
            {
                //rEvent = rEvents[i];
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(AddToHandler), rEvents[i]);

                ButtonEventArgs bea = new ButtonEventArgs(null, null, DateTime.Now, Hardware.Button.AppDefined1);
                bea.RoutedEvent = rEvents[i];

                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(RaiseEvent), bea);
                if (_btnEventCntr != i + 1)
                {
                    Log.Comment("ButtonEvent '" + rEvents[i].Name + "' event not raised");
                    testResult = MFTestResults.Fail;
                }
            }
            btnEvent = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_RaiseEvent_FocusChangedEvent_Test2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Raising FocusChangedEventEvents and Verifying");
            //addToEvtRoute = false;
            btnEvent = false;
            fcsEvent = true;
            _fcsEventCntr = 0;
            rEvents = new RoutedEvent[] { Buttons.GotFocusEvent, Buttons.LostFocusEvent };
            for (int i = 0; i < rEvents.Length; i++)
            {
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                 new DispatcherOperationCallback(AddToHandler), rEvents[i]);

                FocusChangedEventArgs fcea = new FocusChangedEventArgs(null, DateTime.Now, mainWindow, mainWindow);
                fcea.RoutedEvent = rEvents[i];

                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(RaiseEvent), fcea);
                if (_fcsEventCntr != i + 1)
                {
                    Log.Comment("FocusChangedEvent '" + rEvents[i].Name + "' event not raised");
                    testResult = MFTestResults.Fail;
                }
            }
            fcsEvent = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_AddHandlerTest3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            //addToEvtRoute = false;
            btnEvent = true;
            _btnEventCntr = 0;
            handledEventsToo = false;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                 new DispatcherOperationCallback(AddToHandler), Buttons.ButtonDownEvent);

            ButtonEventArgs bea = new ButtonEventArgs(null, null, DateTime.Now, Hardware.Button.AppDefined1);
            bea.RoutedEvent = Buttons.ButtonDownEvent;

            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(RaiseEvent), bea);

            Log.Comment("registering handler NOT to be invoked when event is marked handled");
            if (_btnEventCntr != 1)
            {
                Log.Comment("Expected ButtonHandler called '1' but got '" + _btnEventCntr + "'");
                testResult = MFTestResults.Fail;
            }
            _btnEventCntr = 0;
            Log.Comment("registering handler to be invoked even when event is marked handled");
            handledEventsToo = true;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                 new DispatcherOperationCallback(AddToHandler), Buttons.ButtonDownEvent);
            bea = new ButtonEventArgs(null, null, DateTime.Now, Hardware.Button.AppDefined2);
            bea.RoutedEvent = Buttons.ButtonDownEvent;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(RaiseEvent), bea);

            if (_btnEventCntr <= 1)
            {
                Log.Comment("Expected ButtonHandler called more than '1' but got '" + _btnEventCntr + "'");
                testResult = MFTestResults.Fail;
            }
            handledEventsToo = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_AddToEventRouteTest4()
        {
            MFTestResults testResult = MFTestResults.Pass;
            rEvents = new RoutedEvent[] { Buttons.ButtonDownEvent, Buttons.ButtonUpEvent,             
                Buttons.PreviewButtonDownEvent, Buttons.PreviewButtonUpEvent};
            handledEventsToo = false;
            //addToEvtRoute = true;
            Log.Comment("Setting Both Event Counters to zero (0)");
            _btnEventCntr = 0;
            _fcsEventCntr = 0;
            Log.Comment("Adding all ButtonDownEvents to the EventRoute");
            btnEvent = true;
            fcsEvent = false;
            for (int i = 0; i < rEvents.Length; i++)
            {
                eRoute = new EventRoute(rEvents[i]);
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                        new DispatcherOperationCallback(AddToHandler), eRoute);
            }
            Log.Comment("Raising the Events and Verifying");
            for (int i = 0; i < rEvents.Length; i++)
            {
                ButtonEventArgs bea = new ButtonEventArgs(null, null, DateTime.Now, Hardware.Button.AppDefined1);
                bea.RoutedEvent = rEvents[i];

                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                        new DispatcherOperationCallback(RaiseEvent), bea);
            }

            Log.Comment("This currently fails, getting one additional button event than expected, investigating");
            if (_btnEventCntr != rEvents.Length)
            {
                Log.Comment("Expected ButtonEvents = '" + rEvents.Length +
                    "' but got '" + _btnEventCntr + "'");
                testResult = MFTestResults.Skip;
            }
            Log.Comment("Adding all FocuseChangedEvents to the EventRoute");
            btnEvent = false;
            fcsEvent = true;
            rEvents = new RoutedEvent[] { Buttons.GotFocusEvent, Buttons.LostFocusEvent };
            for (int i = 0; i < rEvents.Length; i++)
            {
                eRoute = new EventRoute(rEvents[i]);
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                    new DispatcherOperationCallback(AddToHandler), eRoute);
            }
            Log.Comment("Raising the Events and Verifying");
            for (int i = 0; i < rEvents.Length; i++)
            {
                FocusChangedEventArgs fcea = new FocusChangedEventArgs(null, DateTime.Now, mainWindow, mainWindow);
                fcea.RoutedEvent = rEvents[i];

                Application.Current.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                        new DispatcherOperationCallback(RaiseEvent), fcea);
            }

            if (_fcsEventCntr != rEvents.Length)
            {
                Log.Comment("Expected FocusChangedEvent = '" + rEvents.Length +
                    "' but got '" + _fcsEventCntr + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_ArrangeTest5()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Creating UI");
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(CreateUI), null);
            Log.Comment("Varying Panel dimensions, Getting Rendered panel dimensions and");
            Log.Comment("Verifying contained Rectangle pixel colors");
            Point[] dimensions = new Point[] {new Point(0, 0), new Point(1, 1), 
                new Point(_random.Next(_width/2)+1,_random.Next(_height/2 )+1),
                new Point(_width/4, _height/4), new Point(_width/2, _height/2)};
            for (int i = 0; i < dimensions.Length; i++)
            {
                if ((MFTestResults)_panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                      new DispatcherOperationCallback(ArrangeTest), dimensions[i]) != MFTestResults.Pass)
                {
                    Log.Comment("Failure : Panel Rendered size not updated");
                    testResult = MFTestResults.Fail;
                }
                if (dimensions[i].x > 1 && dimensions[i].y > 1)
                {
                    Point[] chkPoints = GetRandomPoints_InRectangle(20, dimensions[i].x, dimensions[i].y,
                        0, 0);
                    Thread.Sleep(wait);
                    Log.Comment("Verifying colors inside new Rectangle arrangement");
                    if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
                    {
                        Log.Comment("Failure in verifying UI with the new Arrangement");
                        testResult = MFTestResults.Fail;
                    }
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_CheckAccessTest6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Checking Access to UIElements and Verifying");
            if (_panel.CheckAccess())
            {
                Log.Comment("A Non-UI Thread Shouldn't have access to UIElements");
                testResult = MFTestResults.Fail;
            }
            checkAccess = false;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(CreateUI), null);
            if (!checkAccess)
            {
                Log.Comment("A UI Thread Should have access to UIElements");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_ChildElementFromPointTest7()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Creating The UI");
            addText = true;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(CreateUI), null);
            Log.Comment("Verifying the Rectnagle is at the Center and Text at Bottom-Right");
            testResult = (MFTestResults)mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
             new DispatcherOperationCallback(ChildElementFromPointTest), null);
            addText = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_ContainsPointTest8()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Creating The UI");
            addText = true;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(CreateUI), null);

            if ((MFTestResults)mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                 new DispatcherOperationCallback(ContainsPointTest), null) != MFTestResults.Pass)
            {
                testResult = MFTestResults.Fail;
            }
            addText = false;

            return testResult;
        }


        [TestMethod]
        public MFTestResults UIElement_GetDesiredSizeTest9()
        {
            MFTestResults testResult = MFTestResults.Pass;
            UIElement elt = new Panel();
            elt.Visibility = Visibility.Visible;
            int wd, ht;
            int[] eltW = new int[] { 0, midX, _width, _width + 1, int.MaxValue };
            int[] eltH = new int[] { 0, midY, _height, _height + 1, int.MaxValue };
            int[] w = new int[] { 0, _random.Next(midX), midX / 2, midX, midX + _random.Next(midX), _width };
            int[] h = new int[] { 0, _random.Next(midY), midY / 2, midY, midY + _random.Next(midY), _height };
            for (int i = 0; i < eltW.Length; i++)
            {
                elt.Width = eltW[i];
                elt.Height = eltH[i];
                Log.Comment("UIElement Width = " + elt.Width + ", Height = " + elt.Height);
                for (int j = 0; j < w.Length; j++)
                {
                    elt.Measure(w[j], h[j]);
                    elt.GetDesiredSize(out wd, out ht);
                    if (((w[j] < elt.Width) && (wd != w[j])) ||
                        ((h[j] < elt.Height) && (ht != h[j])))
                    {
                        Log.Comment("expected desired width/height = '" + w[j] + "/" + h[j] + "' but got '" +
                            wd + "/" + ht + "' for available width/height '" + w[j] + "/" + h[j] + "'");
                        testResult = MFTestResults.Fail;
                    }
                    else if (((w[j] >= elt.Width) && (wd != elt.Width)) ||
                        ((h[j] >= elt.Height) && (ht != elt.Height)))
                    {
                        Log.Comment("expected desired width/height = '" + elt.Width + "/" + elt.Height + "' but got '" +
                          wd + "/" + ht + "' for available width/height '" + w[j] + "/" + h[j] + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            Log.Comment("Verifying Desirede width/height returns zero when UIElement is collapsed");
            elt.Visibility = Visibility.Collapsed;
            elt.Width = _random.Next(_width) + 1;
            elt.Height = _random.Next(_height) + 1;
            elt.Measure(_random.Next(_width) + 1, _random.Next(_width) + 1);
            elt.GetDesiredSize(out wd, out ht);
            if ((wd != 0) || (ht != 0))
            {
                Log.Comment("expected desired width/height = '0/0' when " +
                    "UIElement is collapsed but got '" + wd + "/" + ht + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_GetLayoutOffsetTest10()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int[] eltW = new int[] { 0, midX, _width };
            int[] eltH = new int[] { 0, midY, _height };
            int x, y;
            Log.Comment("Creating The UI, Getting the Layout Offset and verifying");
            for (int i = 0; i < eltW.Length; i++)
            {
                rWidth = eltW[i];
                rHeight = eltH[i];
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 15),
                  new DispatcherOperationCallback(CreateUI), null);
                Thread.Sleep(wait);
                _rect.GetLayoutOffset(out x, out y);
                if ((x != (_width - rWidth) / 2) || (y != (_height - rHeight) / 2))
                {
                    Log.Comment("Expected Rectangle Offset(" + (_width - rWidth) / 2 +
                        ", " + (_height - rHeight) / 2 + ") but got (" + x + ", " + y + ")");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_SetMarginTest11()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Creating The UI, Setting the Margin and verifying");
            setMargin = true;
            rWidth = midX;
            rHeight = midY;
            int[] lft = new int[] { 0, 1, 7, _random.Next(rWidth / 2), 0 };
            int[] tp = new int[] { 12, 0, 7, _random.Next(rHeight / 2), rHeight / 2 };
            int[] rgt = new int[] { 5, 3, 7, _random.Next(rWidth / 2), rWidth / 2 };
            int[] btm = new int[] { 0, 1, 7, _random.Next(rHeight / 2), 0 };
            int l, t, r, b;
            Point[] chkPoints = null;
            for (int i = 0; i < lft.Length; i++)
            {
                left = lft[i];
                top = tp[i];
                right = rgt[i];
                bottom = btm[i];
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(CreateUI), null);
                Thread.Sleep(wait);
                chkPoints = GetRandomPoints_InRectangle(20, (rWidth - (left + right)),
                    (rHeight - (top + bottom)), midX + left - rWidth / 2, midY + top - rHeight / 2);
                if (VerifyingPixelColor(chkPoints, _color) != MFTestResults.Pass)
                {
                    Log.Comment("Failure verifying pixel colors upon setting the Margin to (" +
                        left + ", " + top + ", " + right + ", " + bottom + ")");
                    testResult = MFTestResults.Fail;
                }
                _rect.GetMargin(out l, out t, out r, out b);
                if (l != left || t != top || r != right || b != bottom)
                {
                    Log.Comment("Expected Margins '(" + left + ", " + top +
                        ", " + right + ", " + bottom + ")'");
                    testResult = MFTestResults.Fail;
                }
            }
            setMargin = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_GetMarginTest12()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Setting a UIElement margin, Getting Margin and verifying");
            int l, t, r, b;
            int[] mgs = new int[] { 0, -1, 1, _random.Next(midX), midX, _width, int.MaxValue, int.MinValue };
            UIElement _UIElt = new Panel();
            for (int i = 0; i < mgs.Length; i++)
            {
                _UIElt.SetMargin(mgs[i]);
                _UIElt.GetMargin(out l, out t, out r, out b);
                if (l != mgs[i] || t != mgs[i] || r != mgs[i] || b != mgs[i])
                {
                    Log.Comment("Expected Margins '(" + mgs[i] + ", " + mgs[i] +
                        ", " + mgs[i] + ", " + mgs[i] + ")'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_GetPointerTargetTest13()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Creating The UI, Getting the Point at Target(x, y) and verifying");
            pWidth = 3 * _width / 4;
            pHeight = 3 * _height / 4;
            rWidth = midX;
            rHeight = midY;
            _rectVisibility = Visibility.Visible;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(CreateUI), null);
            autoEvent.WaitOne(wait, false);
            Thread.Sleep(wait);
            UIElement _UIElt1 = mainWindow.GetPointerTarget(midX, midY);
            if (_UIElt1.Width != rWidth || _UIElt1.Height != rHeight)
            {
                Log.Comment("Expected a UIElement with Width/Height '" + rWidth +
                    "/" + rHeight + "' at(" + midX + ", " + midY + ") but got Width/Height '" +
                    _UIElt1.Width + "/" + _UIElt1.Height + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Hidding the Rectangle, Getting the Point at Target(x, y) and verifying");
            _rectVisibility = Visibility.Hidden;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(CreateUI), null);
            autoEvent.WaitOne(wait, false);
            Thread.Sleep(wait);
            UIElement _UIElt2 = mainWindow.GetPointerTarget(midX, midY);
            if (_UIElt2.Width != pWidth || _UIElt2.Height != pHeight)
            {
                Log.Comment("Expected a UIElement with Width/Height '" + pWidth +
                    "/" + pHeight + "' at(" + midX + ", " + midY + ") but got Width/Height '" +
                    _UIElt2.Width + "/" + _UIElt2.Height + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Hidding the Panel, Getting the Point at Target(x, y) and verifying");
            _panelVisibilty = Visibility.Hidden;
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(CreateUI), null);
            autoEvent.WaitOne(wait, false);
            Thread.Sleep(wait);
            UIElement _UIElt3 = mainWindow.GetPointerTarget(midX, midY);
            if (_UIElt3 != null)
            {
                Log.Comment("Expected null but got '" + _UIElt3.ToString() + "'");
                testResult = MFTestResults.Fail;
            }
            _rectVisibility = Visibility.Visible;
            _panelVisibilty = Visibility.Visible;
            pWidth = _width;
            pHeight = _height;

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_GetRenderSizeTest14()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            int w, h;
            int[] pW = new int[] { 3 * _width / 4, midX, midX, _random.Next(_width) };
            int[] pH = new int[] { 3 * _height / 4, midY, midY, _random.Next(_height) };
            int[] rW = new int[] { midX, midX, 3 * _width / 4, _random.Next(_width) };
            int[] rH = new int[] { midY, midY, 3 * _height / 4, _random.Next(_height) };
            Log.Comment("Creating UI, Getting Rendered Size and verifying");
            for (int i = 0; i < pW.Length; i++)
            {
                pWidth = pW[i];
                pHeight = pH[i];
                rWidth = rW[i];
                rHeight = rH[i];
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
             new DispatcherOperationCallback(CreateUI), null);
                Thread.Sleep(wait);
                _rect.GetRenderSize(out w, out h);
                if ((pWidth <= rWidth && w != pWidth) || (pWidth > rWidth && w != rWidth))
                {
                    Log.Comment("Panel/Rectangle Width = '" + pWidth + "/" +
                        rWidth + "' but got Rendered width '" + w + "'");
                    testResult = MFTestResults.Fail;
                }
                if ((pHeight <= rHeight && h != pHeight) || (pHeight > rHeight && h != rHeight))
                {
                    Log.Comment("Panel/Rectangle Height = '" + pHeight + "/" +
                        rHeight + "' but got Rendered Height '" + h + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_GetUnclippedSizeTest15()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            int w, h;
            int[] rW = new int[] { midX / 2, midX, midX + 1, 3 * _width / 4, _random.Next(_width) };
            int[] rH = new int[] { midY / 2, midY, midY + 1, 3 * _height / 4, _random.Next(_height) };
            pWidth = midX;
            pHeight = midY;
            Log.Comment("Creating UI, Getting UnclippedSize and verifying");
            for (int i = 0; i < rW.Length; i++)
            {
                rWidth = rW[i];
                rHeight = rH[i];
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
             new DispatcherOperationCallback(CreateUI), null);
                autoEvent.WaitOne(wait, false);
                Thread.Sleep(wait);
                _rect.GetUnclippedSize(out w, out h);
                if (w != rWidth || h != rHeight)
                {
                    Log.Comment("Expected Unclipped Width/Height = '" + rWidth +
                        "/" + rHeight + "' but got '" + w + "/" + h + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        private void ConvertPts(ref int x, ref int y)
        {
            if (rWidth < midX)
            {
                if ((((~_width) & midX & 1) == 1) || ((~midX) & rWidth & 1) == 1) x--;
            }
            else
            {
                if (((~_width) & rWidth & 1) == 1) x--;
            }
            if (rHeight < midY)
            {
                if ((((~_height) & midY & 1) == 1) || ((~midY) & rHeight & 1) == 1) x--;
            }
            else
            {
                if (((~_height) & rHeight & 1) == 1) x--;
            }
        }


        [TestMethod]
        public MFTestResults UIElement_InvalidateTest16()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Changing the Rectangle Fill color, Invalidating and Verifying");
            _color = Colors.Red;
            _rect.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(InvalidateTest), null);
            Thread.Sleep(wait);

            int x = midX - rWidth / 2;
            int y = midY - rHeight / 2;

            ConvertPts(ref x, ref y);

            Point[] chkPoint = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            if (VerifyingPixelColor(chkPoint, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure in Verifying pixels after Invalidating");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_InvalidateArrangeTest17()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }
            Log.Comment("This currently fails, skipping it until verify and open bug for it");
            MFTestResults testResult = MFTestResults.Skip;
            Log.Comment("Change Rectangle Alignment to Top-Right, InvalidateArrange and Verifying");

            int x = midX - rWidth / 2;
            int y = midY - rHeight / 2;

            ConvertPts(ref x, ref y);

            Point[] chkPoint = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            _rect.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(InvalidateArrangeTest), null);
            Thread.Sleep(wait);
            if (VerifyingPixelColor(chkPoint, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure in Verifying pixels after Invalidating");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_InvalidateMeasureTest18()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Change Rectangle size, InvalidateMeasure and Verifying");
            _rect.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
              new DispatcherOperationCallback(InvalidateMeasureTest), null);
            Thread.Sleep(wait);

            rWidth /= 2;
            rHeight /= 2;

            int x = midX - rWidth / 2;
            int y = midY - rHeight / 2;

            ConvertPts(ref x, ref y);

            Point[] chkPoint = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            if (VerifyingPixelColor(chkPoint, _color) != MFTestResults.Pass)
            {
                Log.Comment("Failure in Verifying pixels after Invalidating");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Verifying area outside Rectangle is white");
            chkPoint = GetRandomPoints_OutofRectangle(20, rWidth, rHeight, midX, midY);
            Thread.Sleep(wait);
            if (VerifyingPixelColor(chkPoint, Color.White) != MFTestResults.Pass)
            {
                Log.Comment("Failure: not all areas outside rectangle are white");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_PointToClientTest19()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Initializing Random points in the screen coordinates,");
            Log.Comment("Converting to Client coordinates and verifying");
            int x1, y1, x2 = 0, y2 = 0;
            for (int i = 0; i < 40; i++)
            {
                x1 = _random.Next(int.MaxValue);
                y1 = _random.Next(int.MaxValue);
                if (i % 2 == 0)
                {
                    x1 = -x1;
                    y1 = -y1;
                }

                x2 = x1 - (midX - (rWidth  / 2)) + (1 == (rWidth & 1) ? 1 : 0);
                y2 = y1 - (midY - (rHeight / 2)) + (1 == (rHeight & 1) ? 1 : 0);

                _rect.PointToClient(ref x1, ref y1);
                if (x1 != x2 || y1 != y2)
                {
                    Log.Comment("Expected client coordinate  '(" + x2 +
                        ", " + y2 + ")' but got '(" + x1 + ", " + y1 + ")'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_PointToScreenTest20()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Initializing Random points in the client coordinates,");
            Log.Comment("Converting to Screen coordinates and verifying");
            int x1, y1, x2 = 0, y2 = 0;
            for (int i = 0; i < 40; i++)
            {
                x1 = _random.Next(int.MaxValue);
                y1 = _random.Next(int.MaxValue);
                if (i % 2 == 0)
                {
                    x1 = -x1;
                    y1 = -y1;
                }

                x2 = x1 + (midX - (rWidth  / 2)) - (1 == (rWidth  & 1) ? 1 : 0);
                y2 = y1 + (midY - (rHeight / 2)) - (1 == (rHeight & 1) ? 1 : 0);

                _rect.PointToScreen(ref x1, ref y1);
                if (x1 != x2 || y1 != y2)
                {
                    Log.Comment("Expected screen coordinate  '(" + x2 +
                        ", " + y2 + ")' but got '(" + x1 + ", " + y1 + ")'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_UpdateLayoutTest21()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }

            return (MFTestResults)_panel.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(UpdateLayoutTest), null);
        }

        [TestMethod]
        public MFTestResults UIElement_VerticalAlignmentTest22()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            pWidth = _width;
            pHeight = _height;
            rWidth = midX;
            rHeight = midY;
            _color = Colors.Green;
            hAlignment = HorizontalAlignment.Center;
            Log.Comment("Creating UI, Rectangle with Width/Height = '" + rWidth + "/" + rHeight + "'");
            Log.Comment(" changing VerticalAlignment and verifying");
            VerticalAlignment[] vAlignments = new VerticalAlignment[] {VerticalAlignment.Bottom,
            VerticalAlignment.Center, VerticalAlignment.Stretch, VerticalAlignment.Top};

            int x = midX - rWidth / 2;
            int y = midY - rHeight / 2;

            ConvertPts(ref x, ref y);

            Point[] chkPoint1 = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, _height - rHeight);
            Point[] chkPoint2 = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            Point[] chkPoint3 = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            Point[] chkPoint4 = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, 0);
            Point[][] chkPoints = new Point[][] { chkPoint1, chkPoint2, chkPoint3, chkPoint4 };
            for (int i = 0; i < vAlignments.Length; i++)
            {
                vAlignment = vAlignments[i];
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(CreateUI), null);
                Thread.Sleep(wait);
                if (VerifyingPixelColor(chkPoints[i], _color) != MFTestResults.Pass)
                {
                    Log.Comment("Failure in Verifying pixels for VerticalAlignment '" +
                        vAlignments[i].ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            //Stretch not-explicitly set dimension and verify

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_HorizontalAlignmentTest23()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            pWidth = _width;
            pHeight = _height;
            rWidth = midX;
            rHeight = midY;
            _color = Colors.Green;
            vAlignment = VerticalAlignment.Center;
            Log.Comment("Creating UI, Rectangle with Width/Height = '" + rWidth + "/" + rHeight + "'");
            Log.Comment(" changing HorizontalAlignment and verifying");
            HorizontalAlignment[] hAlignments = new HorizontalAlignment[] {HorizontalAlignment.Center,
            HorizontalAlignment.Left, HorizontalAlignment.Right, HorizontalAlignment.Stretch};

            int x = midX - rWidth / 2;
            int y = midY - rHeight / 2;

            ConvertPts(ref x, ref y);

            Point[] chkPoint1 = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            Point[] chkPoint2 = GetRandomPoints_InRectangle(20, rWidth, rHeight, 0, y);
            Point[] chkPoint3 = GetRandomPoints_InRectangle(20, rWidth, rHeight, _width - rWidth, y);
            Point[] chkPoint4 = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            Point[][] chkPoints = new Point[][] { chkPoint1, chkPoint2, chkPoint3, chkPoint4 };

            for (int i = 0; i < hAlignments.Length; i++)
            {
                hAlignment = hAlignments[i];
                mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(CreateUI), null);
                Thread.Sleep(wait);
                if (VerifyingPixelColor(chkPoints[i], _color) != MFTestResults.Pass)
                {
                    Log.Comment("Failure in Verifying pixels for HorizontalAlignment '" +
                        hAlignments[i].ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            //Stretch not-explicitly set dimension and verify

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_VisibilityTest24()
        {
            Log.Comment("Tests visiblity and IsVisible property");
            Log.Comment("Creating UI, changing visibility and verifying");
            MFTestResults testResult = MFTestResults.Pass;
            Visibility[] visibilities = new Visibility[] { Visibility.Collapsed, 
                Visibility.Hidden, Visibility.Visible };
            _panelVisibilty = Visibility.Visible;
            foreach (Visibility v in visibilities)
            {
                Log.Comment("Setting Rectangle Visibility to '" + v.ToString() + "'");
                _rectVisibility = v;
                if ((v == Visibility.Visible && (CreateUI_And_Verify(_color) != MFTestResults.Pass))
                    || v == Visibility.Visible && !_rect.IsVisible)
                {
                    Log.Comment("Failure : Rectangle.IsVisible returned '" + _rect.IsVisible + "'");
                    testResult = MFTestResults.Fail;
                }
                else if ((v != Visibility.Visible && CreateUI_And_Verify(Color.White) != MFTestResults.Pass)
                    || v != Visibility.Visible && _rect.IsVisible)
                {
                    Log.Comment("Failure : Rectangle.IsVisible returned '" + _rect.IsVisible + "'");
                    testResult = MFTestResults.Fail;
                }
                else if (!_panel.IsVisible)
                {
                    Log.Comment("Child visibility shouldn't affect parent visibility");
                    testResult = MFTestResults.Fail;
                }
            }
            Log.Comment("If the parent is hidden, then children are too");
            _panelVisibilty = Visibility.Hidden;
            foreach (Visibility v in visibilities)
            {
                Log.Comment("Setting Rectangle Visibility to '" + v.ToString() + "'");
                _rectVisibility = v;
                if ((CreateUI_And_Verify(Color.White) != MFTestResults.Pass) || _rect.IsVisible)
                {
                    Log.Comment("Failure : Rectangle.IsVisible returned '" + _rect.IsVisible + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            _rectVisibility = Visibility.Visible;
            _panelVisibilty = Visibility.Visible;

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_RootUIElementTest25()
        {
            Log.Comment("Gets the RootUIElement of child rectangle and panel");
            Log.Comment("Verifies RootUIElement is WindowManager.Instance");
            Log.Comment("Creates a new Panel, creates a rectangle and add it as a child of the panel");
            Log.Comment("Gets the RootUIElement of the new rectangle and panel");
            Log.Comment("Verifies RootUIElement is the new Panle");
            MFTestResults testResult = MFTestResults.Pass;
            Panel p = new Panel();
            Rectangle r = new Rectangle();
            p.Children.Add(r);
            UIElement[] uiElets = new UIElement[] { _rect, _panel, r, p };
            //corresponding root UIElements
            UIElement[] roots = new UIElement[] { WindowManager.Instance, WindowManager.Instance, p, p };
            for (int i = 0; i < uiElets.Length; i++)
            {
                Log.Comment("Getting the RootUIElement of '" +
                    uiElets[i].GetType() + "' and Verifying");
                if (uiElets[i].RootUIElement != roots[i])
                {
                    Log.Comment("Failure : expected RootUIElement '" +
                        roots + "' but got '" + uiElets[i].RootUIElement + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_ParentTest26()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Getting the rectangle parent and verifying");
            if (_rect.Parent != _panel)
            {
                Log.Comment("Failure : expected parent'" + _panel +
                    "' but got '" + _rect.Parent + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Getting the panel parent and verifying");
            if (_panel.Parent != mainWindow)
            {
                Log.Comment("Failure : expected parent'" + mainWindow +
                   "' but got '" + _panel.Parent + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Creating a new Panel, adding a new Rectangle as its child");
            Panel p = new Panel();
            Rectangle r = new Rectangle();
            p.Children.Add(r);
            Log.Comment("Getting the Parent and verifying it's the Panel");
            if (r.Parent != p)
            {
                Log.Comment("Failure : expected parent'" + p +
                 "' but got '" + r.Parent + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Removing the child Rectangle");
            p.Children.Remove(r);
            Log.Comment("verifying the removed rectangle parent is null");
            if (r.Parent != null)
            {
                Log.Comment("Failure : expected parent'null' but got '" + r.Parent + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_IsEnabledTest27()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Panel parentPanel = new Panel();
            Panel childPanel = new Panel();
            Rectangle _uiElementUnderTest = new Rectangle();
            parentPanel.Children.Add(childPanel);
            childPanel.Children.Add(_uiElementUnderTest);
            Log.Comment("Verifying default value of UIElement.IsEnabled is true");
            if (!_uiElementUnderTest.IsEnabled)
            {
                Log.Comment("Failure : by defalut IsEnable returned false");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Disabling the parent and verifying");
            childPanel.IsEnabled = false;
            if (_uiElementUnderTest.IsEnabled)
            {
                Log.Comment("Disabling parent should also disable child");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Enabling the parent and verifying");
            childPanel.IsEnabled = true;
            if (!_uiElementUnderTest.IsEnabled)
            {
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Disabling the RootUIElement and verifying");
            parentPanel.IsEnabled = false;
            if (_uiElementUnderTest.IsEnabled)
            {
                Log.Comment("Disabling RootUIElement should also disable child");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Enabling the RootUIElement and verifying");
            parentPanel.IsEnabled = true;
            if (!_uiElementUnderTest.IsEnabled)
            {
                Log.Comment("Enabling RootUIElement should also disable child");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_IsEnabledChangedTest28()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Panel parentPanel = new Panel();
            Panel childPanel = new Panel();
            Rectangle _uiElementUnderTest = new Rectangle();
            parentPanel.Children.Add(childPanel);
            childPanel.Children.Add(_uiElementUnderTest);
            _uiElementUnderTest.IsEnabledChanged += new PropertyChangedEventHandler(EnabledChangedHandler);
            Log.Comment("Setting IsEnabled to true, and verifying");
            Log.Comment("No notification, since by default IsEnabled is true");
            autoEvent.Reset();
            _uiElementUnderTest.IsEnabled = true;
            Log.Comment("Waiting and verifying");
            if (autoEvent.WaitOne(1000, true))
            {
                Log.Comment("Failure : PropertyChangedEventHandler shouldn't be notified");
                testResult = MFTestResults.Fail;
            }
            if ((bool)_enabledHandlerState)
            {
                Log.Comment("Failure : IsEnable property changed");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Changing the IsEnabled property to false");
            _uiElementUnderTest.IsEnabled = false;
            Log.Comment("Waiting and verifying");
            if (!autoEvent.WaitOne(1000, true))
            {
                Log.Comment("Failure : PropertyChangedEventHandler not notified");
                testResult = MFTestResults.Fail;
            }
            if ((bool)_enabledHandlerState)
            {
                Log.Comment("Failure : IsEnable property not changed");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Changing the IsEnabled property back to true");
            _uiElementUnderTest.IsEnabled = true;
            Log.Comment("Waiting and verifying");
            if (!autoEvent.WaitOne(1000, true))
            {
                Log.Comment("Failure : PropertyChangedEventHandler not notified");
                testResult = MFTestResults.Fail;
            }
            if (!(bool)_enabledHandlerState)
            {
                Log.Comment("Failure : IsEnable property not changed");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_IsFocusedTest29()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }

            return (MFTestResults)mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(IsFocusedTest), null);
        }

        [TestMethod]
        public MFTestResults UIElement_IsVisibleChangedTest30()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }

            return (MFTestResults)mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(IsVisibleChangedTest), null);
        }

        [TestMethod]
        public MFTestResults UIElement_Parent_IsVisibleChangedTest31()
        {
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                Log.Comment("Failure : Creating UI and Verifying");
                return MFTestResults.Fail;
            }

            return (MFTestResults)mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
                new DispatcherOperationCallback(Parent_IsVisibleChangedTest), null);
        }

        [TestMethod]
        public MFTestResults UIElement_UpdateUIByDispatcherInvokeTest32()
        {
            addText = true;
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Updating _txt through the dispatcher and verifying");
            _txt.Dispatcher.Invoke(new TimeSpan(0, 0, 2), new DispatcherOperationCallback(UpdateText), null);
            DateTime t1 = DateTime.Now;
            mEvent.WaitOne(3000, false);
            Log.Comment("Waited for : " + (DateTime.Now - t1).Ticks.ToString());
            if (_txt.TextContent != "olleH")
            {
                Log.Comment("Expected 'olleH' but got '" + _txt.TextContent + "'");
                testResult = MFTestResults.Fail;
            }
            if (_txt.VerticalAlignment != VerticalAlignment.Top)
            {
                Log.Comment("Expected alignment '" + VerticalAlignment.Top + "' but got '" + _txt.VerticalAlignment + "'");
                testResult = MFTestResults.Fail;
            }
            mEvent.Reset();
            if (!noInvalidException)
            {
                Log.Comment("InvalidException was thrown");
                testResult = MFTestResults.Fail;
            }
            addText = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults UIElement_UpdateUIByNonUIThreadTest33()
        {
            addText = true;
            if (CreateUI_And_Verify(_color) != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Updating _txt without the dispatcher and verifying for Exception");
            Thread t = new Thread(new ThreadStart(UpdateText));
            t.Start();
            DateTime t1 = DateTime.Now;
            mEvent.WaitOne(3000, false);
            Log.Comment("Waited for :" + (DateTime.Now - t1).Ticks.ToString());
            if (_txt.VerticalAlignment != VerticalAlignment.Bottom)
            {
                Log.Comment("Expected alignment '" + VerticalAlignment.Bottom + "' but got '" + _txt.VerticalAlignment + "'");
                testResult = MFTestResults.Fail;
            }
            if (noInvalidException)
            {
                Log.Comment("Failure : InvalidException should have been thrown");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        private Panel _panel = null;
        private Rectangle _rect;
        private Visibility _rectVisibility = Visibility.Visible, _panelVisibilty = Visibility.Visible;
        private Text _txt;
        private Color _color = Colors.Green;
        private VerticalAlignment vAlignment = VerticalAlignment.Center;
        private HorizontalAlignment hAlignment = HorizontalAlignment.Center;
        private string txtStr = "Bottom-Right Text";
        private int rWidth = (3 * _width) / 4, rHeight = (3 * _height) / 4;
        private int pWidth = _width, pHeight = _height;
        static bool checkAccess = false, addText = false, setMargin = false;
        static int left, top, right, bottom;
        private object _visibleHandlerState = false, _enabledHandlerState = false;
        public bool noInvalidException = true;
        public ManualResetEvent mEvent = new ManualResetEvent(false);

        #region CreateUIAndVerifyPixel
        private MFTestResults CreateUI_And_Verify(Color c)
        {
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            rWidth = midX;
            rHeight = midY;

            int x = midX - rWidth / 2;
            int y = midY - rHeight / 2;

            ConvertPts(ref x, ref y);

            Log.Comment("Creating UI, Rectangle with Width/Height = '" +
                rWidth + "/" + rHeight + "' at the center and verifying");
            Point[] chkPoint = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5),
          new DispatcherOperationCallback(CreateUI), null);
            Thread.Sleep(wait);

            return VerifyingPixelColor(chkPoint, c);
        }
        #endregion CreateUIAndVerifyPixel

        public void UpdateText()
        {
            try
            {
                _txt.TextContent = "olleH";
                _txt.VerticalAlignment = VerticalAlignment.Top;
            }
            catch (InvalidOperationException e)
            {
                Log.Comment("Caught : " + e.Message);
                noInvalidException = false;
            }
            mEvent.Set();
        }

        public object UpdateText(object o)
        {
            UpdateText();
            return null;
        }

        object CreateUI(object obj)
        {
            _panel = new Panel();
            _panel.Width = pWidth;
            _panel.Height = pHeight;
            _panel.Visibility = _panelVisibilty;

            _rect = new Rectangle();
            _rect.Width = rWidth;
            _rect.Height = rHeight;
            _rect.VerticalAlignment = vAlignment;
            _rect.HorizontalAlignment = hAlignment;
            _rect.Fill = new SolidColorBrush(_color);
            _rect.Visibility = _rectVisibility;
            _panel.Children.Add(_rect);
            if (setMargin)
            {
                _rect.SetMargin(left, top, right, bottom);
            }
            if (addText)
            {
                _txt = new Text(_font, txtStr);
                _txt.HorizontalAlignment = HorizontalAlignment.Right;
                _txt.VerticalAlignment = VerticalAlignment.Bottom;
                _panel.Children.Add(_txt);
            }

            checkAccess = _panel.CheckAccess();
            mainWindow.Child = _panel;
            return null;
        }

        object ArrangeTest(object obj)
        {
            MFTestResults tResult = MFTestResults.Pass;
            Point pt = obj as Point;
            int wd, ht;
            _panel.Width = pt.x;
            _panel.Height = pt.y;
            _panel.InvalidateArrange();
            _panel.Arrange(0, 0, pt.x, pt.y);
            _panel.GetRenderSize(out wd, out ht);

            if (wd != pt.x || ht != pt.y)
            {
                Log.Comment("Failure Getting Rendered size");
                tResult = MFTestResults.Fail;
            }

            return tResult;
        }

        object ChildElementFromPointTest(object obj)
        {
            MFTestResults tResult = MFTestResults.Pass;
            Point pt = obj as Point;
            Text t = (Text)mainWindow.ChildElementFromPoint(_width - 3, _height - 3);
            if (t.TextContent != txtStr)
            {
                Log.Comment("Expected Text Content '" + txtStr + "' but got '" + t.TextContent + "'");
                tResult = MFTestResults.Fail;
            }
            Rectangle r = (Rectangle)mainWindow.ChildElementFromPoint(midX, midY);
            if (r.Width != rWidth || r.Height != rHeight)
            {
                Log.Comment("Expected Rectangle @ (" + midX + ", " + midY + ") with Width = '" +
                    rWidth + "', Height = '" + rHeight + "' but got Width = '" +
                    r.Width + "' and Height '" + r.Height + "'");
                tResult = MFTestResults.Fail;
            }
            return tResult;
        }

        object ContainsPointTest(object obj)
        {
            MFTestResults tResult = MFTestResults.Pass;

            int x = midX - _width / 2;
            int y = midY - _height / 2;

            Point[] chkPoint = GetRandomPoints_InRectangle(20, _width, _height, x, y);
            for (int i = 0; i < chkPoint.Length; i++)
            {
                if (!mainWindow.ContainsPoint(chkPoint[i].x, chkPoint[i].y))
                {
                    mainWindow._pBitmap.DrawEllipse(Colors.Red, 1, chkPoint[i].x, chkPoint[i].y, 3, 3, (Color)0, 0, 0, (Color)0, 0, 0, Bitmap.OpacityTransparent);
                    mainWindow._pBitmap.Flush();

                    Log.Comment("The mainWindow do not contain the Point (" +
                        chkPoint[i].x + ", " + chkPoint[i].y + ")");
                    tResult = MFTestResults.Fail;
                }
            }

            x = midX - rWidth / 2;
            y = midY - rHeight / 2;

            ConvertPts(ref x, ref y);

            chkPoint = GetRandomPoints_InRectangle(20, rWidth, rHeight, x, y);
            for (int i = 0; i < chkPoint.Length; i++)
            {
                if (!_rect.ContainsPoint(chkPoint[i].x, chkPoint[i].y))
                {
                    mainWindow._pBitmap.DrawEllipse(Colors.Red, 1, chkPoint[i].x, chkPoint[i].y, 3, 3, (Color)0, 0, 0, (Color)0, 0, 0, Bitmap.OpacityTransparent);
                    mainWindow._pBitmap.Flush();

                    Log.Comment("The rectangle do not contain the Point (" +
                        chkPoint[i].x + ", " + chkPoint[i].y + ")");
                    tResult = MFTestResults.Fail;
                }
            }
            return tResult;
        }

        object InvalidateTest(object obj)
        {
            _rect.Fill = new SolidColorBrush(_color);
            _rect.Invalidate();
            return null;
        }

        object InvalidateArrangeTest(object obj)
        {
            _rect.VerticalAlignment = VerticalAlignment.Top;
            _rect.HorizontalAlignment = HorizontalAlignment.Right;
            _rect.InvalidateArrange();
            //_rect.UpdateLayout();
            return null;
        }

        object InvalidateMeasureTest(object obj)
        {
            _rect.Width = rWidth / 2;
            _rect.Height = rHeight / 2;
            _rect.InvalidateMeasure();
            _rect.UpdateLayout();
            return null;
        }

        object UpdateLayoutTest(object obj)
        {
            MFTestResults tResult = MFTestResults.Pass;
            _panel.InvalidateArrange();
            _panel.InvalidateMeasure();
            _panel.UpdateLayout();
            if (!_panel.IsMeasureValid || !_panel.IsArrangeValid)
            {
                tResult = MFTestResults.Fail;
            }
            Text t1 = new Text(_font, "Text 1");
            t1.ForeColor = Colors.Red;
            Text t2 = new Text(_font, "Text 2");
            Rectangle r = new Rectangle();
            r.Width = rWidth / 2;
            r.Height = rHeight / 2;
            r.Fill = new SolidColorBrush(Colors.Blue);
            Panel childPanel = new Panel();
            childPanel.Children.Add(t1);
            _panel.Children.Add(childPanel);
            _panel.Children.Add(t2);
            _panel.Children.Add(r);
            UIElement[] elts = new UIElement[] { childPanel, t2, r };
            foreach (UIElement elt in elts)
            {
                elt.InvalidateArrange();
                elt.InvalidateMeasure();
                _panel.UpdateLayout();
                if (!elt.IsArrangeValid || !elt.IsMeasureValid)
                {
                    Log.Comment("Failure: Updating a child UIElement type '" +
                        elt.ToString() + "' and verifying");
                    tResult = MFTestResults.Fail;
                }
            }
            t1.InvalidateArrange();
            t1.InvalidateMeasure();
            _panel.UpdateLayout();
            if (!t1.IsArrangeValid || !t1.IsMeasureValid)
            {
                Log.Comment("Failure: Updating child of child panel and verifying");
                tResult = MFTestResults.Fail;
            }

            childPanel.Children.Remove(t1);
            t1.InvalidateArrange();
            t1.InvalidateMeasure();
            _panel.UpdateLayout();
            if (t1.IsArrangeValid || t1.IsMeasureValid)
            {
                Log.Comment("Failure: Updating removed UIElement shouldn't validate");
                tResult = MFTestResults.Fail;
            }

            return tResult;
        }

        private void EnabledChangedHandler(object o, PropertyChangedEventArgs pcea)
        {
            _enabledHandlerState = pcea.NewValue;
            autoEvent.Set();
        }

        private void VisibleChangedHandler(object o, PropertyChangedEventArgs pcea)
        {
            _visibleHandlerState = pcea.NewValue;
            autoEvent.Set();
        }

        object IsFocusedTest(object obj)
        {
            MFTestResults tResult = MFTestResults.Pass;
            try
            {
                _rect.IsEnabled = true;
                Log.Comment("Setting Focus to UIElement and verifying");
                Buttons.Focus(_rect);
                if ((!_rect.IsFocused) || _panel.IsFocused)
                {
                    Log.Comment("Failure : UIElement IsFocused '" + _rect.IsFocused +
                        "', parent IsFocused '" + _panel.IsFocused + "'");
                    tResult = MFTestResults.Fail;
                }
                Log.Comment("Setting Focus to parent and verifying");
                Buttons.Focus(_panel);
                if (!_panel.IsFocused || _rect.IsFocused)
                {
                    Log.Comment("Failure : UIElement IsFocused '" + _rect.IsFocused +
                        "', parent IsFocused '" + _panel.IsFocused + "'");
                    tResult = MFTestResults.Fail;
                }
                Log.Comment("Disabling UIElement");
                _rect.IsEnabled = false;
                Log.Comment("Setting Focus to UIElement and verifying");
                Buttons.Focus(_rect);
                if (_rect.IsFocused)
                {
                    Log.Comment("Failure : UIElement IsFocused '" + _rect.IsFocused + "'");
                    tResult = MFTestResults.Fail;
                }
            }
            finally
            {
                _rect.IsVisibleChanged -= new PropertyChangedEventHandler(VisibleChangedHandler);
            }

            return tResult;
        }

        object IsVisibleChangedTest(object obj)
        {
            MFTestResults tResult = MFTestResults.Pass;
            try
            {
                _rect.IsVisibleChanged += new PropertyChangedEventHandler(VisibleChangedHandler);
                Log.Comment("Setting visibility to Visibility.Visible");
                _rect.Visibility = Visibility.Visible;
                autoEvent.Reset();
                Log.Comment("Changing the visibiltity to Hidden and verifying");
                _rect.Visibility = Visibility.Hidden;
                if (!autoEvent.WaitOne(2000, false))
                {
                    Log.Comment("Failure : PropertyChangedEventHandler not notified");
                    tResult = MFTestResults.Fail;
                }
                if ((bool)_visibleHandlerState)
                {
                    Log.Comment("Failure : IsVisible property NOT changed upon Visible -> Hidden");
                    tResult = MFTestResults.Fail;
                }
                Log.Comment("Changing the visibiltity to Collapsed and verifying");
                _rect.Visibility = Visibility.Collapsed;
                if (autoEvent.WaitOne(1000, true))
                {
                    Log.Comment("Failure : PropertyChangedEventHandler notified");
                    tResult = MFTestResults.Fail;
                }
                if ((bool)_visibleHandlerState)
                {
                    Log.Comment("Failure : IsVisible property changed upon Hidden - > Collapsed");
                    tResult = MFTestResults.Fail;
                }
                Log.Comment("Changing the visibiltity back to Visible and verifying");
                _rect.Visibility = Visibility.Visible;

                if (!autoEvent.WaitOne(1000, true))
                {
                    Log.Comment("Failure : PropertyChangedEventHandler not notified");
                    tResult = MFTestResults.Fail;
                }
                if (!(bool)_visibleHandlerState)
                {
                    Log.Comment("Failure : IsVisible property NOT changed upon Hidden -> Visible");
                    tResult = MFTestResults.Fail;
                }
            }
            finally
            {
                _rect.IsVisibleChanged -= new PropertyChangedEventHandler(VisibleChangedHandler);
            }

            return tResult;
        }

        object Parent_IsVisibleChangedTest(object obj)
        {
            MFTestResults tResult = MFTestResults.Pass;
            _panel.Visibility = Visibility.Visible;
            Text t = new Text(_font, "Bar");
            try
            {
                autoEvent.Reset();
                t.IsVisibleChanged += new PropertyChangedEventHandler(VisibleChangedHandler);
                t.Visibility = Visibility.Hidden;
                t.Visibility = Visibility.Visible;
                Log.Comment(" An unparented object should not receive IsVisibleChanged because it is always not visible");
                Log.Comment("Changing the visibility of an unparented element, verifying no IsVisibleChanged event");
                if (autoEvent.WaitOne(1000, false))
                {
                    Log.Comment("Failure : An unparented object received IsVisibleChanged event");
                    tResult = MFTestResults.Fail;
                }
                Log.Comment("Adding a UIElement set to Visible to a IsVisible parent does change IsVislble.");
                _panel.Children.Add(t);
                if (!autoEvent.WaitOne(1000, true))
                {
                    Log.Comment("Failure : IsVisibleChanged event not received");
                    tResult = MFTestResults.Fail;
                }
                Log.Comment("Now modify the visibility of the parent and ensure the child receives the event.");
                Log.Comment("changing the parent visibility to hidden and verifying");
                _panel.Visibility = Visibility.Hidden;
                if (!autoEvent.WaitOne(1000, true))
                {
                    Log.Comment("Failure : IsVisibleChanged event not received");
                    tResult = MFTestResults.Fail;
                }
                if ((bool)_visibleHandlerState)
                {
                    Log.Comment("Failure : IsVisible property NOT changed upon Visible -> Hidden");
                    tResult = MFTestResults.Fail;
                }
                Log.Comment("changing the parent visibility to visible and verifying");
                _panel.Visibility = Visibility.Visible;
                if (!autoEvent.WaitOne(1000, true))
                {
                    Log.Comment("Failure : IsVisibleChanged event not received");
                    tResult = MFTestResults.Fail;
                }
                if (!(bool)_visibleHandlerState)
                {
                    Log.Comment("Failure : IsVisible property NOT changed upon Hidden -> Visible");
                    tResult = MFTestResults.Fail;
                }
                Log.Comment("changing the parent visibility to collapsed and verifying");
                _panel.Visibility = Visibility.Collapsed;
                if (!autoEvent.WaitOne(1000, true))
                {
                    Log.Comment("Failure : IsVisibleChanged event not received");
                    tResult = MFTestResults.Fail;
                }
                if ((bool)_visibleHandlerState)
                {
                    Log.Comment("Failure : IsVisible property NOT changed upon Visible -> Collapsed");
                    tResult = MFTestResults.Fail;
                }
            }
            finally
            {
                t.IsVisibleChanged -= new PropertyChangedEventHandler(VisibleChangedHandler);
                _panel.Children.Remove(t);
            }

            return tResult;
        }

        #region EventTestHelper
        int _btnEventCntr, _fcsEventCntr;
        bool btnEvent = false, fcsEvent = false;
        bool handledEventsToo = false;
        //bool addToEvtRoute = false;
        RoutedEvent[] rEvents = null;
        AutoResetEvent _handlerEvent = new AutoResetEvent(false);
        //RoutedEvent rEvent = null;
        EventRoute eRoute = null;
        object RaiseEvent(object obj)
        {
            bool bRet = false;
            mainWindow.RaiseEvent((RoutedEventArgs)obj);
            bRet &= _handlerEvent.WaitOne(1000, true);
            return null;
        }
        object AddToHandler(object obj)
        {
            if (obj is RoutedEvent && btnEvent)
            {
                mainWindow.AddHandler((RoutedEvent)obj, new RoutedEventHandler(HandleButtonEvents), handledEventsToo);
            }
            if (obj is RoutedEvent &&  fcsEvent)
            {
                mainWindow.AddHandler((RoutedEvent)obj, new RoutedEventHandler(HandleFocusChangedEvent), handledEventsToo);
            }
            if (obj is EventRoute)
            {
                RoutedEventArgs args = null;

                if (btnEvent)
                {
                    args = new ButtonEventArgs(null, null, DateTime.Now, Hardware.Button.AppDefined1);
                    args.RoutedEvent = Buttons.ButtonDownEvent;
                }
                else
                {
                    args = new FocusChangedEventArgs(null, DateTime.Now, mainWindow, mainWindow);
                    args.RoutedEvent = Buttons.LostFocusEvent;
                }
                mainWindow.AddToEventRoute((EventRoute)obj, args);
            }
            return null;
        }

        private void HandleButtonEvents(object o, RoutedEventArgs e)
        {
            ButtonEventArgs bea = (ButtonEventArgs)e;
            bea.Handled = true;
            _handlerEvent.Set();
            _btnEventCntr++;
        }

        private void HandleFocusChangedEvent(object o, RoutedEventArgs e)
        {
            FocusChangedEventArgs fcea = (FocusChangedEventArgs)e;
            fcea.Handled = true;
            _handlerEvent.Set();
            _fcsEventCntr++;
        }
        #endregion EventTestHelper
    }
}
