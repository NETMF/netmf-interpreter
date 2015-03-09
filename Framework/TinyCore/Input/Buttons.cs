////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{

    // REFACTOR -- this could potentially be integrated with ButtonDevice, though
    // this can give us a nice friendly class name (maybe ButtonPad or something instead),
    // so developer code looks real nice and all.
    // However, all these static wrappers are a buncha unneeded indirection if you're
    // looking perf-wise.

    /// <summary>
    ///     The Button class represents the button device to the
    ///     members of a context.
    /// </summary>
    /// <remarks>
    ///     The static members of this class simply delegate to the primary
    ///     button device of the calling thread's input manager.
    /// </remarks>
    public sealed class Buttons
    {
        /// <summary>
        ///     PreviewButtonDown
        /// </summary>
        public static readonly RoutedEvent PreviewButtonDownEvent = new RoutedEvent("PreviewButtonDown", RoutingStrategy.Tunnel, typeof(ButtonEventHandler));

        /// <summary>
        ///     PreviewButtonUp
        /// </summary>
        public static readonly RoutedEvent PreviewButtonUpEvent = new RoutedEvent("PreviewButtonUp", RoutingStrategy.Tunnel, typeof(ButtonEventHandler));

        /// <summary>
        ///     ButtonDown
        /// </summary>
        public static readonly RoutedEvent ButtonDownEvent = new RoutedEvent("ButtonDown", RoutingStrategy.Bubble, typeof(ButtonEventHandler));

        /// <summary>
        ///     ButtonUp
        /// </summary>
        public static readonly RoutedEvent ButtonUpEvent = new RoutedEvent("ButtonUp", RoutingStrategy.Bubble, typeof(ButtonEventHandler));

        /// <summary>
        ///     GotFocus
        /// </summary>
        public static readonly RoutedEvent GotFocusEvent = new RoutedEvent("GotFocus", RoutingStrategy.Bubble, typeof(FocusChangedEventHandler));

        /// <summary>
        ///     LostFocus
        /// </summary>
        public static readonly RoutedEvent LostFocusEvent = new RoutedEvent("LostFocus", RoutingStrategy.Bubble, typeof(FocusChangedEventHandler));

        /// <summary>
        ///     Returns the element that the button is focused on.
        /// </summary>
        public static UIElement FocusedElement
        {
            get
            {
                return PrimaryDevice.Target;
            }
        }

        /// <summary>
        ///     Focuses the button on a particular element.
        /// </summary>
        /// <param name="element">
        ///     The element to focus the button on.
        /// </param>
        public static UIElement Focus(UIElement element)
        {
            return PrimaryDevice.Focus(element);
        }

        /// <summary>
        ///     Returns whether or not the specified button is down.
        /// </summary>
        public static bool IsButtonDown(Button button)
        {
            return PrimaryDevice.IsButtonDown(button);
        }

        /// <summary>
        ///     Returns whether or not the specified button is up.
        /// </summary>
        public static bool IsButtonUp(Button button)
        {
            return PrimaryDevice.IsButtonUp(button);
        }

        /// <summary>
        ///     Returns whether or not the specified button is held.
        /// </summary>
        public static bool IsButtonHeld(Button button)
        {
            return PrimaryDevice.IsButtonHeld(button);
        }

        /// <summary>
        ///     Returns the state of the specified button.
        /// </summary>
        public static ButtonState GetButtonState(Button button)
        {
            return PrimaryDevice.GetButtonState(button);
        }

        /// <summary>
        ///     The primary button device.
        /// </summary>
        public static ButtonDevice PrimaryDevice
        {
            get
            {
                return InputManager.CurrentInputManager._buttonDevice;
            }
        }
    }
}


