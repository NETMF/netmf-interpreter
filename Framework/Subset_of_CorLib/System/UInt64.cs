////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Globalization;

    /**
     * Wrapper for unsigned 64 bit integers.
     */
    [Serializable, CLSCompliant(false)]
    public struct UInt64
    {
        private ulong m_value;

        public const ulong MaxValue = (ulong)0xffffffffffffffffL;
        public const ulong MinValue = 0x0;

        public override String ToString()
        {
            return Number.Format(m_value, true, "G", NumberFormatInfo.CurrentInfo);
        }

        public String ToString(String format)
        {
            return Number.Format(m_value, true, format, NumberFormatInfo.CurrentInfo);
        }

        [CLSCompliant(false)]
        public static ulong Parse(String s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            return Convert.ToUInt64(s);
        }

    }
}


