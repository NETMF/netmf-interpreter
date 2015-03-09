////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.Lcd
{
    public interface ILcdDriver
    {
        bool Initialize();
        bool Uninitialize();
        bool RefreshController();
        void Clear();
        IntPtr GetFrameBuffer();
        void BitBltEx ( int x, int y, int width, int height, IntPtr data );
        void BitBlt( int width, int height, int widthInWords, IntPtr data, bool fUseDelta );
        void SetContrast( byte Contrast );
        void PowerSave( bool On );
        void SetOscillatorFrequency( byte OscillatorFrequency );
        void WriteChar ( byte c, int row, int col );
        void WriteFormattedChar( char c );
        int GetWidth       ();
        int GetHeight      ();
        int GetBitsPerPixel();
    }
}
