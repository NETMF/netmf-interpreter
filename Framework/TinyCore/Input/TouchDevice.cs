////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The TouchDevice class represents the stylus/touch device to the
    ///     members of a context.
    /// </summary>
    public sealed class TouchDevice : InputDevice
    {
        internal TouchDevice(InputManager inputManager)
        {
            _inputManager = inputManager;

            _inputManager.InputDeviceEvents[(int)InputManager.InputDeviceType.Touch].PostProcessInput += new ProcessInputEventHandler(PostProcessInput);
        }

        public override UIElement Target
        {
            get
            {
                return _focus;
            }
        }

        public override InputManager.InputDeviceType DeviceType
        {
            get
            {
                return InputManager.InputDeviceType.Touch;
            }
        }

        public void SetTarget(UIElement target)
        {
            _focus = target;
        }

        private void PostProcessInput(object sender, ProcessInputEventArgs e)
        {
            if (!e.StagingItem.Input.Handled)
            {
                RoutedEvent routedEvent = e.StagingItem.Input.RoutedEvent;
                if (routedEvent == InputManager.InputReportEvent)
                {
                    InputReportEventArgs input = e.StagingItem.Input as InputReportEventArgs;
                    if (input != null)
                    {
                        RawTouchInputReport report = input.Report as RawTouchInputReport;
                        if (report != null)
                        {
                            TouchEventArgs args = new TouchEventArgs(
                            this,
                            report.Timestamp,
                            report.Touches);

                            UIElement target = report.Target;
                            if (report.EventMessage == (byte)TouchMessages.Down)
                            {
                                args.RoutedEvent = TouchEvents.TouchDownEvent;
                            }
                            else if (report.EventMessage == (byte)TouchMessages.Up)
                            {
                                args.RoutedEvent = TouchEvents.TouchUpEvent;
                            }
                            else if (report.EventMessage == (byte)TouchMessages.Move)
                            {
                                args.RoutedEvent = TouchEvents.TouchMoveEvent;
                            }
                            else
                                throw new Exception("Unknown touch event.");

                            args.Source = (target == null ? _focus : target);
                            e.PushInput(args, e.StagingItem);
                        }
                    }
                }
            }
        }

        private InputManager _inputManager;
        private UIElement _focus;
    }
}


