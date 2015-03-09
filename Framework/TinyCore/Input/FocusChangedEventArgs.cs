////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The FocusChangedEventArgs class contains information about focus states
    /// </summary>
    public class FocusChangedEventArgs : InputEventArgs
    {
        /// <summary>
        ///     Constructs an instance of the FocusChangedEventArgs class.
        /// </summary>
        /// <param name="button">
        ///     The logical button device associated with this event.
        /// </param>
        /// <param name="timestamp">
        ///     The time when the input occured.
        /// </param>
        /// <param name="oldFocus">
        ///     The element that previously had focus.
        /// </param>
        /// <param name="newFocus">
        ///     The element that now has focus.
        /// </param>
        public FocusChangedEventArgs(ButtonDevice buttonDevice, DateTime timestamp, UIElement oldFocus, UIElement newFocus)
            : base(buttonDevice, timestamp)
        {
            OldFocus = oldFocus;
            NewFocus = newFocus;
        }

        /// <summary>
        ///     The element that previously had focus.
        /// </summary>
        public readonly UIElement OldFocus;

        /// <summary>
        ///     The element that now has focus.
        /// </summary>
        public readonly UIElement NewFocus;
    }
}


