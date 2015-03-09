////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Globalization;

    /**
     * A place holder class for signed bytes.
     * @author Jay Roxe (jroxe)
     * @version
     */
    [Serializable, CLSCompliant(false)]
    public struct SByte
    {
        private sbyte m_value;

        /**
         * The maximum value that a <code>Byte</code> may represent: 127.
         */
        public const sbyte MaxValue = (sbyte)0x7F;

        /**
         * The minimum value that a <code>Byte</code> may represent: -128.
         */
        public const sbyte MinValue = unchecked((sbyte)0x80);

        public override String ToString()
        {
            return Number.Format(m_value, true, "G", NumberFormatInfo.CurrentInfo);
        }

        public String ToString(String format)
        {
            return Number.Format(m_value, true, format, NumberFormatInfo.CurrentInfo);
        }

        [CLSCompliant(false)]
        public static sbyte Parse(String s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            return Convert.ToSByte(s);
        }

    }
}


