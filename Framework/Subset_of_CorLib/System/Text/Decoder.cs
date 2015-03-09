////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Text
{
    /*
     * A <i>Decoder</i> is used to decode a sequence of blocks of bytes into a
     * sequence of blocks of characters. Following instantiation of a decoder,
     * sequential blocks of bytes are converted into blocks of characters through
     * calls to the <i>GetChars</i> method. The decoder maintains state between the
     * conversions, allowing it to correctly decode byte sequences that span
     * adjacent blocks.
     *
     * <p>Instances of specific implementations of the <i>Decoder</i> abstract base
     * class are typically obtained through calls to the <i>GetDecoder</i> method
     * of <i>Encoding</i> objects.
     */
    public abstract class Decoder
    {
        public abstract void Convert(byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex, int charCount, bool flush,
            out int bytesUsed, out int charsUsed, out bool completed);
    }
}


