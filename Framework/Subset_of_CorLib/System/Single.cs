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
            // Number.Format method is responsible for returning the correct string representation of the value; however, it does not work properly for special values.
            // Fixing the issue in Number.Format requires a significant amount of modification in both native and managed code.
            // In order to avoid that (at lease for now), we use the help of Double class to identify special values and use Number.Format for the others.
            string str = ((Double)m_value).ToString();
            switch (str)
            {
                case "Infinity":
                case "-Infinity":
                case "NaN":
                    return str;
                default:
                    return Number.Format(m_value, false, "G", NumberFormatInfo.CurrentInfo);
            }
        }

        public String ToString(String format)
        {
            string str = ((Double)m_value).ToString();
            switch (str)
            {
                case "Infinity":
                case "-Infinity":
                case "NaN":
                    return str;
                default:
                    return Number.Format(m_value, false, format, NumberFormatInfo.CurrentInfo);
            }
        }
    }
}


