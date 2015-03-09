////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The RawButtonInputReport class encapsulates the raw input
    ///     provided from a keyboard.
    /// </summary>
    /// <remarks>
    ///     It is important to note that the InputReport class only contains
    ///     blittable types.  This is required so that the report can be
    ///     marshalled across application domains.
    /// </remarks>
    public class RawButtonInputReport : InputReport
    {
        /// <summary>
        ///     Constructs an instance of the RawKeyboardInputReport class.
        /// </summary>
        /// <param name="inputSource">
        ///     source of the input
        /// </param>
        /// <param name="timestamp">
        ///     The time when the input occured.
        /// </param>
        public RawButtonInputReport(PresentationSource inputSource, DateTime timestamp, Button button, RawButtonActions actions)
            : base(inputSource, timestamp)
        {
            Button = button;
            Actions = actions;
        }

        /// <summary>
        /// Read-only access to the button reported.
        /// </summary>
        public readonly Button Button;

        /// <summary>
        /// Read-only access to the action reported.
        /// </summary>
        public readonly RawButtonActions Actions;
    }

    // REFACTOR -- this goes in a separate CS file.
    public enum RawButtonActions
    {
        ButtonDown = 1,
        ButtonUp = 2,
        Activate = 4,
        Deactivate = 8,
    }
}


