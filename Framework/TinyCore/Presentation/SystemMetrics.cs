////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Presentation
{
    public sealed class SystemMetrics
    {
        // Gets the color depth of the screen
        // color depth isn't the whole story, this needs to be improved.
        public static int ScreenColorDepth
        {
            get
            {
                int bpp, orientation, height, width;
                Microsoft.SPOT.Hardware.HardwareProvider hwProvider = Microsoft.SPOT.Hardware.HardwareProvider.HwProvider;

                hwProvider.GetLCDMetrics(out width, out height, out bpp, out orientation);

                return bpp;
            }
        }

        // Gets the width of the screen
        public static int ScreenWidth
        {
            get
            {
                int bpp, orientation, height, width;
                Microsoft.SPOT.Hardware.HardwareProvider hwProvider = Microsoft.SPOT.Hardware.HardwareProvider.HwProvider;

                hwProvider.GetLCDMetrics(out width, out height, out bpp, out orientation);

                return width;
            }
        }

        // Gets the height of the screen
        public static int ScreenHeight
        {
            get
            {
                int bpp, orientation, height, width;
                HardwareProvider hwProvider = HardwareProvider.HwProvider;

                hwProvider.GetLCDMetrics(out width, out height, out bpp, out orientation);

                return height;
            }
        }
    }
}


