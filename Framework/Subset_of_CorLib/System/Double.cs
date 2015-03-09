////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    [Serializable]
    public struct Double
    {
        internal double m_value;

        // Public Constants

        // Summary:
        //     Represents the smallest possible value of a System.Double. This field is
        //     constant.
        public const double MinValue = -1.7976931348623157E+308;
        //
        // Summary:
        //     Represents the largest possible value of a System.Double. This field is constant.
        public const double MaxValue = 1.7976931348623157E+308;
        //
        // Summary:
        //     Represents the smallest positive System.Double value that is greater than
        //     zero. This field is constant.
        // 
        // Note:
        // Real value of Epsilon: 4.9406564584124654e-324 (0x1), but JVC misparses that
        // number, giving 2*Epsilon (0x2).
        public const double Epsilon = 4.9406564584124650E-324;
        //
        // Summary:
        //     Represents negative infinity. This field is constant.
        public const double NegativeInfinity = (double)-1.0 / (double)(0.0);
        //
        // Summary:
        //     Represents positive infinity. This field is constant.
        public const double PositiveInfinity = (double)1.0 / (double)(0.0);
        //        
        // Summary:
        //     Represents a value that is not a number (NaN). This field is constant.
        public const double NaN = 0.0 / 0.0;
        //
        // Summary:
        //     Compares this instance to a specified double-precision floating-point number
        //     and returns an integer that indicates whether the value of this instance
        //     is less than, equal to, or greater than the value of the specified double-precision
        //     floating-point number.
        //
        // Parameters:
        //   value:
        //     A double-precision floating-point number to compare.
        //
        // Returns:
        //     A signed number indicating the relative values of this instance and value.Return
        //     Value Description Less than zero This instance is less than value.-or- This
        //     instance is not a number (System.Double.NaN) and value is a number. Zero
        //     This instance is equal to value.-or- Both this instance and value are not
        //     a number (System.Double.NaN), System.Double.PositiveInfinity, or System.Double.NegativeInfinity.
        //     Greater than zero This instance is greater than value.-or- This instance
        //     is a number and value is not a number (System.Double.NaN).
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int CompareTo(double d, double value);
        //
        // Summary:
        //     Returns a value indicating whether the specified number evaluates to negative
        //     or positive infinity
        //
        // Parameters:
        //   d:
        //     A double-precision floating-point number.
        //
        // Returns:
        //     true if d evaluates to System.Double.PositiveInfinity or System.Double.NegativeInfinity;
        //     otherwise, false.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool IsInfinity(double d);
        //
        // Summary:
        //     Returns a value indicating whether the specified number evaluates to a value
        //     that is not a number (System.Double.NaN).
        //
        // Parameters:
        //   d:
        //     A double-precision floating-point number.
        //
        // Returns:
        //     true if d evaluates to System.Double.NaN; otherwise, false.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool IsNaN(double d);
        //
        // Summary:
        //     Returns a value indicating whether the specified number evaluates to negative
        //     infinity.
        //
        // Parameters:
        //   d:
        //     A double-precision floating-point number.
        //
        // Returns:
        //     true if d evaluates to System.Double.NegativeInfinity; otherwise, false.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool IsNegativeInfinity(double d);
        //
        // Summary:
        //     Returns a value indicating whether the specified number evaluates to positive
        //     infinity.
        //
        // Parameters:
        //   d:
        //     A double-precision floating-point number.
        //
        // Returns:
        //     true if d evaluates to System.Double.PositiveInfinity; otherwise, false.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool IsPositiveInfinity(double d);
        //
        // Summary:
        //     Converts the string representation of a number in a specified style and culture-specific
        //     format to its double-precision floating-point number equivalent.
        //
        // Parameters:
        //   s:
        //     A string that contains a number to convert.
        //
        //   style:
        //     A bitwise combination of enumeration values that indicate the style elements
        //     that can be present in s. A typical value to specify is System.Globalization.NumberStyles.Float
        //     combined with System.Globalization.NumberStyles.AllowThousands.
        //
        //   provider:
        //     An object that supplies culture-specific formatting information about s.
        //
        // Returns:
        //     A double-precision floating-point number that is equivalent to the numeric
        //     value or symbol specified in s.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     s is null.
        //
        //   System.FormatException:
        //     s does not represent a numeric value.
        //
        //   System.ArgumentException:
        //     style is not a System.Globalization.NumberStyles value. -or-style is the
        //     System.Globalization.NumberStyles.AllowHexSpecifier value.
        //
        //   System.OverflowException:
        //     s represents a number that is less than System.Double.MinValue or greater
        //     than System.Double.MaxValue.
        public static double Parse(String s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            return Convert.ToDouble(s);
        }
        //
        // Summary:
        //     Converts the numeric value of this instance to its equivalent string representation.
        //
        // Returns:
        //     The string representation of the value of this instance.        
        public override String ToString()
        {
            if(IsPositiveInfinity(this))
            {
                return "Infinity";
            }
            else if(IsNegativeInfinity(this))
            {
                return "-Infinity";
            }
            else if(IsNaN(this))
            {
                return "NaN";
            }

            return Number.Format(m_value, false, "G", NumberFormatInfo.CurrentInfo);
        }
        //
        // Summary:
        //     Converts the numeric value of this instance to its equivalent string representation,
        //     using the specified format.
        //
        // Parameters:
        //   format:
        //     A numeric format string.
        //
        // Returns:
        //     The string representation of the value of this instance as specified by format.
        //
        // Exceptions:
        //   System.FormatException:
        //     format is invalid.
        public String ToString(String format)
        {
            if (IsPositiveInfinity(this))
            {
                return "Infinity";
            }
            else if (IsNegativeInfinity(this))
            {
                return "-Infinity";
            }
            else if (IsNaN(this))
            {
                return "NaN";
            }

            return Number.Format(m_value, false, format, NumberFormatInfo.CurrentInfo);
        }
        //
        // Summary:
        //     Converts the string representation of a number to its double-precision floating-point
        //     number equivalent. A return value indicates whether the conversion succeeded
        //     or failed.
        //
        // Parameters:
        //   s:
        //     A string containing a number to convert.
        //
        //   result:
        //     When this method returns, contains the double-precision floating-point number
        //     equivalent to the s parameter, if the conversion succeeded, or zero if the
        //     conversion failed. The conversion fails if the s parameter is null, is not
        //     a number in a valid format, or represents a number less than System.Double.MinValue
        //     or greater than System.Double.MaxValue. This parameter is passed uninitialized.
        //
        // Returns:
        //     true if s was converted successfully; otherwise, false.
        public static bool TryParse(string s, out double result)
        {
            result = 0.0;

            if (s == null)
            {
                return false;
            }

            try
            {
                result = Convert.ToDouble(s);
                return true;
            }
            catch
            {
                result = 0.0;
            }
            return false;
        }
        //
        // Summary:
        //     Converts the string representation of a number in a specified style and culture-specific
        //     format to its double-precision floating-point number equivalent. A return
        //     value indicates whether the conversion succeeded or failed.
        //
        // Parameters:
        //   s:
        //     A string containing a number to convert.
        //
        //   style:
        //     A bitwise combination of System.Globalization.NumberStyles values that indicates
        //     the permitted format of s. A typical value to specify is System.Globalization.NumberStyles.Float
        //     combined with System.Globalization.NumberStyles.AllowThousands.
        //
        //   provider:
        //     An System.IFormatProvider that supplies culture-specific formatting information
        //     about s.
        //
        //   result:
        //     When this method returns, contains a double-precision floating-point number
        //     equivalent to the numeric value or symbol contained in s, if the conversion
        //     succeeded, or zero if the conversion failed. The conversion fails if the
        //     s parameter is null, is not in a format compliant with style, represents
        //     a number less than System.SByte.MinValue or greater than System.SByte.MaxValue,
        //     or if style is not a valid combination of System.Globalization.NumberStyles
        //     enumerated constants. This parameter is passed uninitialized.
        //
        // Returns:
        //     true if s was converted successfully; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     style is not a System.Globalization.NumberStyles value. -or-style includes
        //     the System.Globalization.NumberStyles.AllowHexSpecifier value.
//        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out double result);
    }
}


