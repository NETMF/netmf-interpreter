////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Presentation.Media
{
    public static class ColorUtility
    {
        public static Color ColorFromRGB(byte r, byte g, byte b)
        {
            return (Color)((b << 16) | (g << 8) | r);
        }

        public static byte GetRValue(Color color)
        {
            return (byte)((uint)color & 0xff);
        }

        public static byte GetGValue(Color color)
        {
            return (byte)(((uint)color >> 8) & 0xff);
        }

        public static byte GetBValue(Color color)
        {
            return (byte)(((uint)color >> 16) & 0xff);
        }
    }

    public enum Color : uint
    {
        Black = 0x00000000,
        White = 0x00ffffff,
    }
}


