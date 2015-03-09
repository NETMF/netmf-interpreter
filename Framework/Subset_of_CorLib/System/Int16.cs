////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System;
    using System.Runtime.CompilerServices;
    using System.Globalization;

    [Serializable]
    public struct Int16
    {
        internal short m_value;

        public const short MaxValue = (short)0x7FFF;
        public const short MinValue = unchecked((short)0x8000);

        public override String ToString()
        {
            return Number.Format(m_value, true, "G", NumberFormatInfo.CurrentInfo);
        }

        public String ToString(String format)
        {
            return Number.Format(m_value, true, format, NumberFormatInfo.CurrentInfo);
        }

        public static short Parse(String s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            return Convert.ToInt16(s);
        }

    }
}


