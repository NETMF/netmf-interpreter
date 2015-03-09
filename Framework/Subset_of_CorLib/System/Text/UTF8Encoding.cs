////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// The worker functions in this file was optimized for performance. If you make changes
// you should use care to consider all of the interesting cases.

// The code of all worker functions in this file is written twice: Once as as a slow loop, and the
// second time as a fast loop. The slow loops handles all special cases, throws exceptions, etc.
// The fast loops attempts to blaze through as fast as possible with optimistic range checks,
// processing multiple characters at a time, and falling back to the slow loop for all special cases.

// This define can be used to turn off the fast loops. Useful for finding whether
// the problem is fastloop-specific.
#define FASTLOOP

namespace System.Text
{
    using System;
    using System.Runtime.CompilerServices;
    // Encodes text into and out of UTF-8.  UTF-8 is a way of writing
    // Unicode characters with variable numbers of bytes per character,
    // optimized for the lower 127 ASCII characters.  It's an efficient way
    // of encoding US English in an internationalizable way.
    //
    // Don't override IsAlwaysNormalized because it is just a Unicode Transformation and could be confused.
    //
    // The UTF-8 byte order mark is simply the Unicode byte order mark
    // (0xFEFF) written in UTF-8 (0xEF 0xBB 0xBF).  The byte order mark is
    // used mostly to distinguish UTF-8 text from other encodings, and doesn't
    // switch the byte orderings.
    public class UTF8Encoding : Encoding
    {
        /*
            bytes   bits    UTF-8 representation
            -----   ----    -----------------------------------
            1        7      0vvvvvvv
            2       11      110vvvvv 10vvvvvv
            3       16      1110vvvv 10vvvvvv 10vvvvvv
            4       21      11110vvv 10vvvvvv 10vvvvvv 10vvvvvv
            -----   ----    -----------------------------------

            Surrogate:
            Real Unicode value = (HighSurrogate - 0xD800) * 0x400 + (LowSurrogate - 0xDC00) + 0x10000
         */
        public UTF8Encoding()
        {
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override byte[] GetBytes(String s);

        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override char[] GetChars(byte[] bytes);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override char[] GetChars(byte[] bytes, int byteIndex, int byteCount);

        public override Decoder GetDecoder()
        {
            return new UTF8Decoder();
        }
    }
}


