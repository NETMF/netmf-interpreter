////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The ButtonEventArgs class contains information about button states.
    /// </summary>
    /// <ExternalAPI/>
    public class ButtonEventArgs : InputEventArgs
    {
        /// <summary>
        ///     Constructs an instance of the ButtonEventArgs class.
        /// </summary>
        /// <param name="buttonDevice">
        ///     The button device associated with this event.
        /// </param>
        /// <param name="timestamp">
        ///     The time when the input occured. (machine time)
        /// </param>
        /// <param name="button">
        ///     The button referenced by the event.
        /// </param>
        public ButtonEventArgs(ButtonDevice buttonDevice, PresentationSource inputSource, DateTime timestamp, Button button)
            : base(buttonDevice, timestamp)
        {
            InputSource = inputSource;
            Button = button;
        }

        /// <summary>
        ///     The Button referenced by the event.
        /// </summary>
        public readonly Button Button;

        /// <summary>
        ///     The state of the button referenced by the event.
        /// </summary>
        public ButtonState ButtonState
        {
            get { return ((ButtonDevice)this.Device).GetButtonState(Button); }
        }

        /// <summary>
        /// The source for this button
        /// </summary>
        public readonly PresentationSource InputSource;

        /// <summary>
        ///     Whether the button pressed is a repeated button or not.
        /// </summary>
        public bool IsRepeat
        {
            get { return _isRepeat; }
        }

        internal bool _isRepeat;
    }
}


