////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     Provides the base class for all input devices.
    /// </summary>
    public abstract class InputDevice : DispatcherObject
    {
        /// <summary>
        ///     Constructs an instance of the InputDevice class.
        /// </summary>
        protected InputDevice()
        {
        }

        /// <summary>
        ///     Returns the element that input from this device is sent to.
        /// </summary>
        public abstract UIElement Target { get; }

        public abstract InputManager.InputDeviceType DeviceType { get; }
    }
}


