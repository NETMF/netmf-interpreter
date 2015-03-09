////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The RawTouchInputReport class encapsulates the raw input
    ///     provided from a multitouch source.
    /// </summary>
    /// <remarks>
    ///     It is important to note that the InputReport class only contains
    ///     blittable types.  This is required so that the report can be
    ///     marshalled across application domains.
    /// </remarks>
    public class RawTouchInputReport : InputReport
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
        public RawTouchInputReport(PresentationSource inputSource, DateTime timestamp, byte eventMessage, TouchInput[] touches)
            : base(inputSource, timestamp)
        {
            EventMessage = eventMessage;
            Touches = touches;
        }

        public RawTouchInputReport(PresentationSource inputSource,
                    DateTime timestamp, byte eventMessage, TouchInput[] touches, UIElement destTarget)
            : base(inputSource, timestamp)
        {
            EventMessage = eventMessage;
            Touches = touches;
            Target = destTarget;
        }

        public readonly UIElement Target;
        public readonly byte EventMessage;
        public readonly TouchInput[] Touches;
    }

    public enum RawTouchActions
    {
        TouchDown = 0x01,
        TouchUp = 0x02,
        Activate = 0x04,
        Deactivate = 0x08,
        TouchMove = 0x10,
    }
}


