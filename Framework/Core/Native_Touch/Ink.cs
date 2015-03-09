////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Touch
{
    public class Ink
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void SetInkRegion(uint flags, int x1, int y1, int x2, int y2, int borderWidth, int color, int penWidth, Bitmap bitmap);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void ResetInkRegion();
    }
}


