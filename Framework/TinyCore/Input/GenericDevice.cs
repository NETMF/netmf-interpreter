////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    public delegate void GenericEventHandler(object sender, GenericEventArgs e);

    public class GenericEventArgs : InputEventArgs
    {
        public GenericEventArgs(InputDevice inputDevice, GenericEvent genericEvent)
            : base(inputDevice, genericEvent.Time)
        {
            InternalEvent = genericEvent;
        }

        public readonly GenericEvent InternalEvent;
    }

    public sealed class GenericEvents
    {
        // Fields
        public static readonly RoutedEvent GenericStandardEvent = new RoutedEvent("GenericStandardEvent", RoutingStrategy.Tunnel, typeof(GenericEventArgs));
    }

    /// <summary>
    ///     The GenericDevice class represents the Generic device to the
    ///     members of a context.
    /// </summary>
    public sealed class GenericDevice : InputDevice
    {
        internal GenericDevice(InputManager inputManager)
        {
            _inputManager = inputManager;

            _inputManager.InputDeviceEvents[(int)InputManager.InputDeviceType.Generic].PostProcessInput += new ProcessInputEventHandler(PostProcessInput);
        }

        private UIElement _focus = null;

        public override UIElement Target
        {
            get
            {
                VerifyAccess();

                return _focus;
            }
        }

        public void SetTarget(UIElement target)
        {
            _focus = target;
        }

        public override InputManager.InputDeviceType DeviceType
        {
            get
            {
                return InputManager.InputDeviceType.Generic;
            }
        }

        private void PostProcessInput(object sender, ProcessInputEventArgs e)
        {
            InputReportEventArgs input = e.StagingItem.Input as InputReportEventArgs;
            if (input != null && input.RoutedEvent == InputManager.InputReportEvent)
            {
                RawGenericInputReport report = input.Report as RawGenericInputReport;

                if (report != null)
                {
                    if (!e.StagingItem.Input.Handled)
                    {
                        GenericEvent ge = (GenericEvent)report.InternalEvent;
                        GenericEventArgs args = new GenericEventArgs(
                            this,
                            report.InternalEvent);

                        args.RoutedEvent = GenericEvents.GenericStandardEvent;
                        if (report.Target != null)
                        {
                            args.Source = report.Target;
                        }

                        e.PushInput(args, e.StagingItem);
                    }
                }
            }
        }

        private InputManager _inputManager;
    }
}


