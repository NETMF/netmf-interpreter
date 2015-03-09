////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.TouchPanel
{
    public interface ITouchPanelDriver
    {
        bool TouchPanel_Disable();
        bool TouchPanel_Enable();
        void TouchPanel_GetPoint(
            ref int tipState,
            ref int source,
            ref int unCalX,
            ref int unCalY);

        /// Managed touch driver specific roucmethods.
        void TouchPanelSetPoint(
            int tipState,
            int source,
            int unCalX,
            int unCalY);
    }
}
