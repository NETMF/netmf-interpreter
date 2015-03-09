////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT
{
    /// <summary>
    ///     An enumeration describing the status of a DispatcherOperation.
    /// </summary>
    ///
    public enum DispatcherOperationStatus
    {
        /// <summary>
        ///     The operation is still pending.
        /// </summary>
        Pending,

        /// <summary>
        ///     The operation has been aborted.
        /// </summary>
        Aborted,

        /// <summary>
        ///     The operation has been completed.
        /// </summary>
        Completed,

        /// <summary>
        ///     The operation has started executing, but has not completed yet.
        /// </summary>
        Executing
    }
}


