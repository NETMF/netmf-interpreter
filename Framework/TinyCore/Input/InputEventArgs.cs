////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The InputEventArgs class represents a type of RoutedEventArgs that
    ///     are relevant to all input events.
    /// </summary>

    public class InputEventArgs : RoutedEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the InputEventArgs class.
        /// </summary>
        /// <param name="inputDevice">
        ///     The input device to associate with this event.
        /// </param>
        /// <param name="timestamp">
        ///     The time when the input occured.
        /// </param>
        public InputEventArgs(InputDevice inputDevice, DateTime timestamp)
        {
            /* inputDevice parameter being null is valid                */
            /* timestamp parameter is valuetype, need not be checked    */

            _inputDevice = inputDevice;
            Timestamp = timestamp;
        }

        /// <summary>
        ///     Read-only access to the input device that initiated this
        ///     event.
        /// </summary>
        public InputDevice Device
        {
            get { return _inputDevice; }
        }

        /// <summary>
        ///     Read-only access to the input timestamp.
        /// </summary>
        public readonly DateTime Timestamp;

        internal InputDevice _inputDevice;
    }
}


