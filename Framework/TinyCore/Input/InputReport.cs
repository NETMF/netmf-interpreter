////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The InputReport is an abstract base class for all input that is
    ///     reported to the InputManager.
    /// </summary>
    /// <remarks>
    ///     It is important to note that the InputReport class only contains
    ///     blittable types.  This is required so that the report can be
    ///     marshalled across application domains.
    /// </remarks>
    public abstract class InputReport
    {
        /// <summary>
        ///     Constructs an instance of the InputReport class.
        /// </summary>
        /// <param name="inputSource">
        ///     The type of input that is being reported.
        /// </param>
        /// <param name="timestamp">
        ///     The time when the input occured.
        /// </param>
        protected InputReport(PresentationSource inputSource, DateTime timestamp)
        {
            /* We need to allow a null presentation source for now, since the root input system doesn't have a presentationsource.
                        if (inputSource == null)
                            throw new ArgumentNullException("inputSource");
            */

            InputSource = inputSource;
            Timestamp = timestamp;
        }

        /// <summary>
        ///     Read-only access to the type of input source that reported input.
        /// </summary>
        public readonly PresentationSource InputSource;

        /// <summary>
        ///     Read-only access to the time when the input occured.
        /// </summary>
        public readonly DateTime Timestamp;
    }

    /// <summary>
    ///     report arguments
    /// </summary>
    public class InputReportArgs
    {
        public InputReportArgs(object dev, object report)
        {
            Device = (InputDevice)dev;
            Report = (InputReport)report;
        }

        public readonly InputDevice Device;
        public readonly InputReport Report;
    }

}


