////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Globalization
namespace System.Globalization
{
    using System;
    using System.Threading;
    using ArrayList = System.Collections.ArrayList;
    using System.Runtime.CompilerServices;
    /*
     Customized format patterns:
     P.S. Format in the table below is the internal number format used to display the pattern.

     Patterns   Format      Description                           Example
     =========  ==========  ===================================== ========
        "h"     "0"         hour (12-hour clock)w/o leading zero  3
        "hh"    "00"        hour (12-hour clock)with leading zero 03
        "hh*"   "00"        hour (12-hour clock)with leading zero 03

        "H"     "0"         hour (24-hour clock)w/o leading zero  8
        "HH"    "00"        hour (24-hour clock)with leading zero 08
        "HH*"   "00"        hour (24-hour clock)                  08

        "m"     "0"         minute w/o leading zero
        "mm"    "00"        minute with leading zero
        "mm*"   "00"        minute with leading zero

        "s"     "0"         second w/o leading zero
        "ss"    "00"        second with leading zero
        "ss*"   "00"        second with leading zero

        "f"     "0"         second fraction (1 digit)
        "ff"    "00"        second fraction (2 digit)
        "fff"   "000"       second fraction (3 digit)
        "ffff"  "0000"      second fraction (4 digit)
        "fffff" "00000"         second fraction (5 digit)
        "ffffff"    "000000"    second fraction (6 digit)
        "fffffff"   "0000000"   second fraction (7 digit)

        "F"     "0"         second fraction (up to 1 digit)
        "FF"    "00"        second fraction (up to 2 digit)
        "FFF"   "000"       second fraction (up to 3 digit)
        "FFFF"  "0000"      second fraction (up to 4 digit)
        "FFFFF" "00000"         second fraction (up to 5 digit)
        "FFFFFF"    "000000"    second fraction (up to 6 digit)
        "FFFFFFF"   "0000000"   second fraction (up to 7 digit)

        "t"                 first character of AM/PM designator   A
        "tt"                AM/PM designator                      AM
        "tt*"               AM/PM designator                      PM

        "d"     "0"         day w/o leading zero                  1
        "dd"    "00"        day with leading zero                 01
        "ddd"               short weekday name (abbreviation)     Mon
        "dddd"              full weekday name                     Monday
        "dddd*"             full weekday name                     Monday

        "M"     "0"         month w/o leading zero                2
        "MM"    "00"        month with leading zero               02
        "MMM"               short month name (abbreviation)       Feb
        "MMMM"              full month name                       Febuary
        "MMMM*"             full month name                       Febuary

        "y"     "0"         two digit year (year % 100) w/o leading zero           0
        "yy"    "00"        two digit year (year % 100) with leading zero          00
        "yyy"   "D3"        year                                  2000
        "yyyy"  "D4"        year                                  2000
        "yyyyy" "D5"        year                                  2000
        ...

        "z"     "+0;-0"     timezone offset w/o leading zero      -8
        "zz"    "+00;-00"   timezone offset with leading zero     -08
        "zzz"   "+00;-00" for hour offset, "00" for minute offset   full timezone offset   -08:00
        "zzz*"  "+00;-00" for hour offset, "00" for minute offset   full timezone offset   -08:00

        "K"    -Local       "zzz", e.g. -08:00
               -Utc         "'Z'", representing UTC
               -Unspecified ""

        "g*"                the current era name                  A.D.

        ":"                 time separator                        :
        "/"                 date separator                        /
        "'"                 quoted string                         'ABC' will insert ABC into the formatted string.
        '"'                 quoted string                         "ABC" will insert ABC into the formatted string.
        "%"                 used to quote a single pattern characters      E.g.The format character "%y" is to print two digit year.
        "\"                 escaped character                     E.g. '\d' insert the character 'd' into the format string.
        other characters    insert the character into the format string.

    Pre-defined format characters:
        (U) to indicate Universal time is used.
        (G) to indicate Gregorian calendar is used.

        Format              Description                             Real format                             Example
        =========           =================================       ======================                  =======================
        "d"                 short date                              culture-specific                        10/31/1999
        "D"                 long data                               culture-specific                        Sunday, October 31, 1999
        "f"                 full date (long date + short time)      culture-specific                        Sunday, October 31, 1999 2:00 AM
        "F"                 full date (long date + long time)       culture-specific                        Sunday, October 31, 1999 2:00:00 AM
        "g"                 general date (short date + short time)  culture-specific                        10/31/1999 2:00 AM
        "G"                 general date (short date + long time)   culture-specific                        10/31/1999 2:00:00 AM
        "m"/"M"             Month/Day date                          culture-specific                        October 31
(G)     "o"/"O"             Round Trip XML                          "yyyy-MM-ddTHH:mm:ss.fffffffK"          1999-10-31 02:00:00.0000000Z
(G)     "r"/"R"             RFC 1123 date,                          "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"   Sun, 31 Oct 1999 10:00:00 GMT
(G)     "s"                 Sortable format, based on ISO 8601.     "yyyy-MM-dd'T'HH:mm:ss"                 1999-10-31T02:00:00
                                                                    ('T' for local time)
        "t"                 short time                              culture-specific                        2:00 AM
        "T"                 long time                               culture-specific                        2:00:00 AM
(G)     "u"                 Universal time with sortable format,    "yyyy'-'MM'-'dd HH':'mm':'ss'Z'"        1999-10-31 10:00:00Z
                            based on ISO 8601.
(U)     "U"                 Universal time with full                culture-specific                        Sunday, October 31, 1999 10:00:00 AM
                            (long date + long time) format
                            "y"/"Y"             Year/Month day                          culture-specific                        October, 1999

    */

    //This class contains only static members and does not require the serializable attribute.
    internal static
    class DateTimeFormat
    {

        internal const int MaxSecondsFractionDigits = 3;
        ////////////////////////////////////////////////////////////////////////////
        //
        // Format the positive integer value to a string and perfix with assigned
        // length of leading zero.
        //
        // Parameters:
        //  value: The value to format
        //  len: The maximum length for leading zero.
        //  If the digits of the value is greater than len, no leading zero is added.
        //  (If len > 2, we'll treat it as 2)
        //
        // Notes:
        //  The function can format to Int32.MaxValue.
        //
        ////////////////////////////////////////////////////////////////////////////
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern String FormatDigits(int value, int len);
        static int ParseRepeatPattern(String format, int pos, char patternChar)
        {
            int len = format.Length;
            int index = pos + 1;
            while ((index < len) && (format[index] == patternChar))
            {
                index++;
            }

            return (index - pos);
        }

        //
        // The pos should point to a quote character. This method will
        // get the string encloed by the quote character.
        //
        internal static String ParseQuoteString(String format, int pos, out int count)
        {
            //
            // NOTE : pos will be the index of the quote character in the 'format' string.
            //
            String result = String.Empty;
            int formatLen = format.Length;
            int beginPos = pos;
            char quoteChar = format[pos++]; // Get the character used to quote the following string.

            bool foundQuote = false;
            while (pos < formatLen)
            {
                char ch = format[pos++];
                if (ch == quoteChar)
                {
                    foundQuote = true;
                    break;
                }
                else if (ch == '\\')
                {
                    // The following are used to support escaped character.
                    // Escaped character is also supported in the quoted string.
                    // Therefore, someone can use a format like "'minute:' mm\"" to display:
                    //  minute: 45"
                    // because the second double quote is escaped.
                    if (pos < formatLen)
                    {
                        result += format[pos++];
                    }
                    else
                    {
                        //
                        // This means that '\' is at the end of the formatting string.
                        //
                        throw new ArgumentException("Format_InvalidString");
                        //throw new FormatException( Environment.GetResourceString( "Format_InvalidString" ) );
                    }
                }
                else
                {
                    result += ch;
                }
            }

            if (!foundQuote)
            {
                // Here we can't find the matching quote.
                throw new ArgumentException("Format_BadQuote");
            }

            //
            // Return the character count including the begin/end quote characters and enclosed string.
            //
            count = pos - beginPos;

            return result;
        }

        //
        // Get the next character at the index of 'pos' in the 'format' string.
        // Return value of -1 means 'pos' is already at the end of the 'format' string.
        // Otherwise, return value is the int value of the next character.
        //
        private static int ParseNextChar(String format, int pos)
        {
            if (pos >= format.Length - 1)
            {
                return (-1);
            }

            return ((int)format[pos + 1]);
        }

        //
        //  FormatCustomized
        //
        //  Actions: Format the DateTime instance using the specified format.
        //
        private static String FormatCustomized(DateTime dateTime, String format, DateTimeFormatInfo dtfi)
        {
            String result = String.Empty;
            int i = 0;
            int tokenLen = 1, hour12;
            int formatLen = format.Length;

            while (i < formatLen)
            {
                char ch = format[i];
                int nextChar;
                bool doneParsingCh = true;
                String tempResult = String.Empty;

                switch (ch)
                {
                    case ':':
                        tempResult = dtfi.TimeSeparator;
                        tokenLen = 1;
                        break;
                    case '/':
                        tempResult = dtfi.DateSeparator;
                        tokenLen = 1;
                        break;
                    case '\'':
                    case '\"':
                        tempResult = ParseQuoteString(format, i, out tokenLen);
                        break;
                    case '%':
                        // Optional format character.
                        // For example, format string "%d" will print day of month
                        // without leading zero.  Most of the cases, "%" can be ignored.
                        nextChar = ParseNextChar(format, i);
                        // nextChar will be -1 if we already reach the end of the format string.
                        // Besides, we will not allow "%%" appear in the pattern.
                        if (nextChar >= 0 && nextChar != (int)'%')
                        {
                            tempResult = FormatCustomized(dateTime, ((char)nextChar).ToString(), dtfi);
                            tokenLen = 2;
                        }
                        else
                        {
                            //
                            // This means that '%' is at the end of the format string or
                            // "%%" appears in the format string.
                            //
                            throw new ArgumentException("Format_InvalidString");
                        }
                        break;
                    case '\\':
                        // Escaped character.  Can be used to insert character into the format string.
                        // For exmple, "\d" will insert the character 'd' into the string.
                        //
                        // NOTENOTE : we can remove this format character if we enforce the enforced quote
                        // character rule.
                        // That is, we ask everyone to use single quote or double quote to insert characters,
                        // then we can remove this character.
                        //
                        nextChar = ParseNextChar(format, i);
                        if (nextChar >= 0)
                        {
                            tempResult = ((char)nextChar).ToString();
                            tokenLen = 2;
                        }
                        else
                        {
                            //
                            // This means that '\' is at the end of the formatting string.
                            //
                            throw new ArgumentException("Format_InvalidString");
                        }
                        break;
                    default:
                        doneParsingCh = false;
                        break;
                }

                if (!doneParsingCh)
                {
                    tokenLen = ParseRepeatPattern(format, i, ch);
                    switch (ch)
                    {
                        case 'h':
                            hour12 = dateTime.Hour % 12;
                            if (hour12 == 0)
                            {
                                hour12 = 12;
                            }

                            tempResult = FormatDigits(hour12, tokenLen);
                            break;
                        case 'H':
                            tempResult = FormatDigits(dateTime.Hour, tokenLen);
                            break;
                        case 'm':
                            tempResult = FormatDigits(dateTime.Minute, tokenLen);
                            break;
                        case 's':
                            tempResult = FormatDigits(dateTime.Second, tokenLen);
                            break;
                        case 'f':
                            if (tokenLen <= MaxSecondsFractionDigits)
                            {
                                int precision = 3;
                                int fraction = dateTime.Millisecond;

                                // Note: Need to add special case when tokenLen > precision to begin with
                                // if we're to change MaxSecondsFractionDigits to be more than 3

                                while (tokenLen < precision)
                                {
                                    fraction /= 10;
                                    precision--;
                                }

                                tempResult = FormatDigits(fraction, tokenLen);
                            }
                            else
                            {
                                throw new ArgumentException("Format_InvalidString");
                            }
                            break;
                        case 't':
                            if (tokenLen == 1)
                            {
                                if (dateTime.Hour < 12)
                                {
                                    if (dtfi.AMDesignator.Length >= 1)
                                    {
                                        tempResult = dtfi.AMDesignator[0].ToString();
                                    }
                                }
                                else
                                {
                                    if (dtfi.PMDesignator.Length >= 1)
                                    {
                                        tempResult = dtfi.PMDesignator[0].ToString();
                                    }
                                }

                            }
                            else
                            {
                                tempResult = (dateTime.Hour < 12 ? dtfi.AMDesignator : dtfi.PMDesignator);
                            }
                            break;
                        case 'd':
                            //
                            // tokenLen == 1 : Day of month as digits with no leading zero.
                            // tokenLen == 2 : Day of month as digits with leading zero for single-digit months.
                            // tokenLen == 3 : Day of week as a three-leter abbreviation.
                            // tokenLen >= 4 : Day of week as its full name.
                            //
                            if (tokenLen <= 2)
                            {
                                tempResult = FormatDigits(dateTime.Day, tokenLen);
                            }
                            else
                            {
                                int dayOfWeek = (int)dateTime.DayOfWeek;

                                if (tokenLen == 3)
                                {
                                    tempResult = dtfi.AbbreviatedDayNames[dayOfWeek];
                                }
                                else
                                {
                                    tempResult = dtfi.DayNames[dayOfWeek];
                                }
                            }
                            break;
                        case 'M':
                            //
                            // tokenLen == 1 : Month as digits with no leading zero.
                            // tokenLen == 2 : Month as digits with leading zero for single-digit months.
                            // tokenLen == 3 : Month as a three-letter abbreviation.
                            // tokenLen >= 4 : Month as its full name.
                            //
                            int month = dateTime.Month;
                            if (tokenLen <= 2)
                            {
                                tempResult = FormatDigits(month, tokenLen);
                            }
                            else
                            {
                                if (tokenLen == 3)
                                {
                                    tempResult = dtfi.AbbreviatedMonthNames[month - 1];
                                }
                                else
                                {
                                    tempResult = dtfi.MonthNames[month - 1];
                                }
                            }
                            break;
                        case 'y':
                            // Notes about OS behavior:
                            // y: Always print (year % 100). No leading zero.
                            // yy: Always print (year % 100) with leading zero.
                            // yyy/yyyy/yyyyy/... : Print year value.  With leading zeros.

                            int year = dateTime.Year;

                            if (tokenLen <= 2)
                            {
                                tempResult = FormatDigits(year % 100, tokenLen);
                            }
                            else
                            {
                                tempResult = year.ToString();

                            }

                            if (tempResult.Length < tokenLen)
                            {
                                tempResult = new string('0', tokenLen - tempResult.Length) + tempResult;
                            }
                            break;

                        default:
                            if (tokenLen == 1)
                            {
                                tempResult = ch.ToString();
                            }
                            else
                            {
                                tempResult = new String(ch, tokenLen);
                            }
                            break;
                    }
                }

                result += tempResult;
                i += tokenLen;
            }

            return result;
        }

        internal static String GetRealFormat(String format, DateTimeFormatInfo dtfi)
        {
            String realFormat = null;

            switch (format[0])
            {
                case 'd':       // Short Date
                    realFormat = dtfi.ShortDatePattern;
                    break;
                case 'D':       // Long Date
                    realFormat = dtfi.LongDatePattern;
                    break;
                case 'f':       // Full (long date + short time)
                    realFormat = dtfi.LongDatePattern + " " + dtfi.ShortTimePattern;
                    break;
                case 'F':       // Full (long date + long time)
                    realFormat = dtfi.FullDateTimePattern;
                    break;
                case 'g':       // General (short date + short time)
                    realFormat = dtfi.GeneralShortTimePattern;
                    break;
                case 'G':       // General (short date + long time)
                    realFormat = dtfi.GeneralLongTimePattern;
                    break;
                case 'm':
                case 'M':       // Month/Day Date
                    realFormat = dtfi.MonthDayPattern;
                    break;
                case 'r':
                case 'R':       // RFC 1123 Standard
                    realFormat = dtfi.RFC1123Pattern;
                    break;
                case 's':       // Sortable without Time Zone Info
                    realFormat = dtfi.SortableDateTimePattern;
                    break;
                case 't':       // Short Time
                    realFormat = dtfi.ShortTimePattern;
                    break;
                case 'T':       // Long Time
                    realFormat = dtfi.LongTimePattern;
                    break;
                case 'u':       // Universal with Sortable format
                    realFormat = dtfi.UniversalSortableDateTimePattern;
                    break;
                case 'U':       // Universal with Full (long date + long time) format
                    realFormat = dtfi.FullDateTimePattern;
                    break;
                case 'y':
                case 'Y':       // Year/Month Date
                    realFormat = dtfi.YearMonthPattern;
                    break;
                default:
                    throw new ArgumentException("Format_InvalidString");
            }

            return (realFormat);
        }

        internal static String Format(DateTime dateTime, String format, DateTimeFormatInfo dtfi)
        {
            if (format == null || format.Length == 0)
            {
                format = "G";
            }

            if (format.Length == 1)
            {
                format = GetRealFormat(format, dtfi);
            }

            return (FormatCustomized(dateTime, format, dtfi));
        }
    }
}


