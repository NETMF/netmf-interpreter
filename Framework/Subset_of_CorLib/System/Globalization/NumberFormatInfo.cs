////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Globalization
namespace System.Globalization
{
    using System;
    //
    // Property             Default Description
    // PositiveSign           '+'   Character used to indicate positive values.
    // NegativeSign           '-'   Character used to indicate negative values.
    // NumberDecimalSeparator '.'   The character used as the decimal separator.
    // NumberGroupSeparator   ','   The character used to separate groups of
    //                              digits to the left of the decimal point.
    // NumberDecimalDigits    2     The default number of decimal places.
    // NumberGroupSizes       3     The number of digits in each group to the
    //                              left of the decimal point.
    // NaNSymbol             "NaN"  The string used to represent NaN values.
    // PositiveInfinitySymbol"Infinity" The string used to represent positive
    //                              infinities.
    // NegativeInfinitySymbol"-Infinity" The string used to represent negative
    //                              infinities.
    //
    //
    //
    // Property                  Default  Description
    // CurrencyDecimalSeparator  '.'      The character used as the decimal
    //                                    separator.
    // CurrencyGroupSeparator    ','      The character used to separate groups
    //                                    of digits to the left of the decimal
    //                                    point.
    // CurrencyDecimalDigits     2        The default number of decimal places.
    // CurrencyGroupSizes        3        The number of digits in each group to
    //                                    the left of the decimal point.
    // CurrencyPositivePattern   0        The format of positive values.
    // CurrencyNegativePattern   0        The format of negative values.
    // CurrencySymbol            "$"      String used as local monetary symbol.
    //

    [Serializable]
    sealed public class NumberFormatInfo /*: ICloneable, IFormatProvider*/
    {
        internal int[] numberGroupSizes = null;//new int[] { 3 };
        internal String positiveSign = null;//"+";
        internal String negativeSign = null;//"-";
        internal String numberDecimalSeparator = null;//".";
        internal String numberGroupSeparator = null;//",";
        private CultureInfo m_cultureInfo;
        internal NumberFormatInfo(CultureInfo cultureInfo)
        {
            m_cultureInfo = cultureInfo;
        }

        public int[] NumberGroupSizes
        {
            get
            {
                if (numberGroupSizes == null)
                {
                    String sizesStr = null;

                    m_cultureInfo.EnsureStringResource(ref sizesStr, System.Globalization.Resources.CultureInfo.StringResources.NumberGroupSizes);

                    int sizesLen = sizesStr.Length;
                    numberGroupSizes = new int[sizesLen];

                    int size;
                    for (int i = 0; i < sizesLen; i++)
                    {
                        size = sizesStr[i] - '0';
                        if (size > 9 || size < 0)
                        {
                            numberGroupSizes = null;
                            throw new InvalidOperationException();
                        }

                        numberGroupSizes[i] = size;
                    }
                }

                return ((int[])numberGroupSizes.Clone());
            }
        }

        public static NumberFormatInfo CurrentInfo
        {
            get
            {
                return CultureInfo.CurrentUICulture.NumberFormat;
            }
        }

        public String NegativeSign
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.negativeSign, System.Globalization.Resources.CultureInfo.StringResources.NegativeSign);
            }
        }

        public String NumberDecimalSeparator
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.numberDecimalSeparator, System.Globalization.Resources.CultureInfo.StringResources.NumberDecimalSeparator);
            }
        }

        public String NumberGroupSeparator
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.numberGroupSeparator, System.Globalization.Resources.CultureInfo.StringResources.NumberGroupSeparator);
            }
        }

        public String PositiveSign
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.positiveSign, System.Globalization.Resources.CultureInfo.StringResources.PositiveSign);
            }
        }
    } // NumberFormatInfo
}


