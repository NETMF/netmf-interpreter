////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    public sealed class Font : MarshalByRefObject
    {
        private object m_font;

        // Must keep in sync with CLR_GFX_Font::c_DefaultKerning
        public const int DefaultKerning = 1024;

        private Font()
        {
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public int CharWidth(char c);
        extern public int Height
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public int AverageWidth
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public int MaxWidth
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public int Ascent
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public int Descent
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public int InternalLeading
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public int ExternalLeading
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public void ComputeExtent(string text, out int width, out int height)
        {
            ComputeExtent(text, out width, out height, DefaultKerning);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void ComputeExtent(string text, out int width, out int height, int kerning);
        public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight)
        {
            ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, 65536, 0, Bitmap.DT_IgnoreHeight | Bitmap.DT_WordWrap);
        }

        public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int availableWidth)
        {
            ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, availableWidth, 0, Bitmap.DT_IgnoreHeight | Bitmap.DT_WordWrap);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int xRelStart, int yRelStart, int availableWidth, int availableHeight, uint dtFlags);
    }
}


