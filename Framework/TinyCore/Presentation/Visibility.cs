////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Presentation
{
    /// <summary>
    /// Visibility - Enum which describes 3 possible visibility options.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Normally visible.
        /// </summary>
        Visible = 0,

        /// <summary>
        /// Occupies space in the layout, but is not visible (completely transparent).
        /// </summary>
        Hidden,

        /// <summary>
        /// Not visible and does not occupy any space in layout, as if it doesn't exist.
        /// </summary>
        Collapsed
    }
}


