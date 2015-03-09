////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The RawGenericInputReport class encapsulates the raw input
    ///     provided from a keyboard.
    /// </summary>
    /// <remarks>
    ///     It is important to note that the InputReport class only contains
    ///     blittable types.  This is required so that the report can be
    ///     marshalled across application domains.
    /// </remarks>
    public class RawGenericInputReport : InputReport
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
        public RawGenericInputReport(PresentationSource inputSource, GenericEvent genericEvent)
            : base(inputSource, genericEvent.Time)
        {
            InternalEvent = genericEvent;
            Target = null;
        }

        public RawGenericInputReport(PresentationSource inputSource,
                        GenericEvent genericEvent, UIElement destTarget)
            : base(inputSource, genericEvent.Time)
        {
            InternalEvent = genericEvent;
            Target = destTarget;
        }

        public readonly UIElement Target;

        public readonly GenericEvent InternalEvent;
    }
}


