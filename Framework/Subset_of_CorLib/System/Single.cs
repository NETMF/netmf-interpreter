////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System.Globalization;
    using System;
    using System.Runtime.CompilerServices;

    [Serializable()]
    public struct Single
    {
        internal float m_value;

        //
        // Public constants
        //
        public const float MinValue = (float)-3.40282346638528859e+38;
        public const float Epsilon = (float)1.4e-45;
        public const float MaxValue = (float)3.40282346638528859e+38;

        public override String ToString()
        {
            return Number.Format(m_value, false, "G", NumberFormatInfo.CurrentInfo);
        }

        public String ToString(String format)
        {
            return Number.Format(m_value, false, format, NumberFormatInfo.CurrentInfo);
        }

    }
}


