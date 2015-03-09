////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

using System;
using System.Threading;
#if MICROFRAMEWORK
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy;
#endif

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4
{
    /// <summary>
    /// Convert unsigned integer into hexadecimal value.
    /// </summary>
    public sealed class HexConverter
    {                          
        /// <summary>
        /// Convert unsigned integer into hexadecimal value.
        /// </summary>
        /// <param name="val"> uint value</param>
        /// <param name="precision">maximum size of the string of HEX digit encoding the uint value</param>
        /// <returns></returns>
        public static string ConvertUintToHex(uint val, uint precision)
        {
#if MICROFRAMEWORK
            if (precision > 8) // 32-bit value !
                precision = 8;
            if (precision == 0)
                precision = 1;
            char[] chars = new char[8];
            uint n = 0; // number of chars
            while (val > 0)
            {
                n++;
                byte b = (byte)(val % 16);
                if (b < 10)
                    chars[8 - n] = (char)('0' + b);
                else
                    chars[8 - n] = (char)('A' + (b - 10));
                val = val >> 4;
            }

            while (n < precision)
            {
                n++;
                chars[8 - n] = '0';
            }

            string mystring = new string(chars, (int)(8 - n), (int)n);
            return mystring;
#else
            return val.ToString("X" + precision.ToString());
#endif
        }

        /// <summary>
        /// Convert unsigned 64-bit integer into hexadecimal value.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ConvertUint64ToHex(UInt64 val, uint precision)
        {
#if MICROFRAMEWORK
            if (precision > 16) // 64-bit value !
                precision = 16;
            if (precision == 0)
                precision = 1;
            char[] chars = new char[16];
            uint n = 0; // number of chars
            while (val > 0)
            {
                n++;
                byte b = (byte)(val % 16);
                if (b < 10)
                    chars[16 - n] = (char)('0' + b);
                else
                    chars[16 - n] = (char)('A' + (b - 10));
                val = val >> 4;
            }

            while (n < precision)
            {
                n++;
                chars[16 - n] = '0';
            }

            string mystring = new string(chars, (int)(16 - n), (int)n);
            return mystring;
#else
            return val.ToString("X" + precision.ToString());
#endif

        }
    }

    public sealed class NumberParser
    {

        /// <summary>
        /// Convert a string into an uint.
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <param name="result">parsed value</param>
        /// <returns>true in case of success, false otherwise</returns>
        public static bool TryParseUint32(String s, out UInt32 result)
        {
#if MICROFRAMEWORK
            if (s == null)
                throw new ArgumentNullException("s");

            result = 0;
            int length = s.Length;
            if ((length > 9) || (length == 0))
                return false;
            UInt64 val = 0;
            char[] chars = s.ToCharArray();
            for (int i = 0; i < length; i++)
            {
                if ((chars[i] < '0') || (chars[i] > '9'))
                    return false;
                val *= 10;
                val += (uint)(chars[i] - '0');
                if (val > (UInt64)int.MaxValue)
                    return false;
            }

            result = (uint)val;
            return true;
#else
            return UInt32.TryParse(s, out result);
#endif
        }

        /// <summary>
        /// Convert a string into an int.
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <param name="result">parsed value</param>
        /// <returns>true in case of success, false otherwise</returns>
        public static bool TryParseInt32(String s, out Int32 result)
        {
#if MICROFRAMEWORK
            if (s == null)
                throw new ArgumentNullException("s");

            result = 0;
            int length = s.Length;
            if ((length > 11) || (length == 0))
                return false;
            Int64 val = 0;
            char[] chars = s.ToCharArray();
            int startIndex = 0;
            if (chars[0] == '-')
            {
                if (chars.Length == 1)
                    return false;
                if ((chars[1] < '0') || (chars[1] > '9'))
                    return false;
                val = (Int64)(chars[1] - '0');
                val *= (Int64)(-1);
                startIndex = 2;
            }

            for (int i = startIndex; i < length; i++)
            {
                if ((chars[i] < '0') || (chars[i] > '9'))
                    return false;
                val *= 10;
                if (val >= 0)
                    val += (uint)(chars[i] - '0');
                else
                    val -= (uint)(chars[i] - '0');
                if ((val > (Int64)Int32.MaxValue) || (val < (Int64)Int32.MinValue))
                    return false;

            }

            result = (Int32)val;
            return true;
#else
            return Int32.TryParse(s, out result);
#endif
        }

        /// <summary>
        /// Convert a string into an int64.
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <param name="result">parsed value</param>
        /// <returns>true in case of success, false otherwise</returns>
        public static bool TryParseInt64(String s, out Int64 result)
        {
#if MICROFRAMEWORK
            if (s == null)
                throw new ArgumentNullException("s");

            result = 0;
            int length = s.Length;
            if ((length > 21) || (length == 0))
                return false;
            Int64 val = 0;
            char[] chars = s.ToCharArray();
            int startIndex = 0;
            if (chars[0] == '-')
            {
                if (chars.Length == 1)
                    return false;
                if ((chars[1] < '0') || (chars[1] > '9'))
                    return false;
                val = (Int64)(chars[1] - '0');
                val *= (Int64)(-1);
                startIndex = 2;
            }

            Int64 newVal;
            for (int i = startIndex; i < length; i++)
            {
                if ((chars[i] < '0') || (chars[i] > '9'))
                    return false;
                newVal = 10 * val;
                if (val >= 0)
                    newVal += (uint)(chars[i] - '0');
                else
                    newVal -= (uint)(chars[i] - '0');
                if (((val >= 0) && (newVal < val)) || (val < 0) && (newVal > val))
                    return false;
                val = newVal;
            }

            result = val;
            return true;
#else
            return Int64.TryParse(s, out result);
#endif
        }

        /// <summary>
        /// Convert a string into a bool.
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <param name="result">parsed value</param>
        /// <returns>true in case of success, false otherwise</returns>
        public static bool TryParseBool(String s, out bool result)
        {
#if MICROFRAMEWORK
            if (s == null)
                throw new ArgumentNullException("s");

            result = false;
            int length = s.Length;
            s = s.ToLower();
            if ((s == "true") || (s == "1"))
            {
                result = true;
                return true;
            }
            else if ((s == "false") || (s == "0"))
            {
                result = false;
                return true;
            }

            return false;

#else
            return bool.TryParse(s, out result);
#endif

        }

        /// <summary>
        /// Convert a string into a byte.
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <param name="result">parsed value</param>
        /// <returns>true in case of success, false otherwise</returns>
        public static bool TryParseByte(String s, out byte result)
        {
#if MICROFRAMEWORK
            if (s == null)
                throw new ArgumentNullException("s");

            result = 0;
            int length = s.Length;
            if ((length > 3) || (length == 0))
                return false;
            UInt32 val = 0;
            char[] chars = s.ToCharArray();
            for (int i = 0; i < length; i++)
            {
                if ((chars[i] < '0') || (chars[i] > '9'))
                    return false;
                val *= 10;
                val += (uint)(chars[i] - '0');
                if (val > (UInt32)byte.MaxValue)
                    return false;
            }

            result = (byte)val;
            return true;
#else
            return byte.TryParse(s, out result);
#endif
        }
    }

    public sealed class Random
    {
        private static System.Random random = new System.Random();

        /// <summary>
        /// returns random value in range [0..max-1]
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandom(int max)
        {
            return random.Next(max);
        }
    }

    public sealed class Trace
    {
        private static void ToString(int offset, int digits, int value, char[] c)
        {
            offset += digits;
            while (value > 0 && offset > 0)
            {
                offset--;
                c[offset] += (char)(value % 10);
                value /= 10;
            }
        }

        public static void Print(string s)
        {
            DateTime dt = DateTime.Now;
            int minutes = dt.Minute + 60 * dt.Hour;
#if MICROFRAMEWORK
            char[] c = { '0', '0', '0', '0', '.', '0', '0', '.', '0', '0', '0', ' ' };
            ToString(0, 4, minutes, c);
            ToString(5, 2, dt.Second, c);
            ToString(8, 3, dt.Millisecond, c);
            string line = new string(c);
            Microsoft.SPOT.Debug.Print(line + s);
#else
            string line = minutes.ToString("0000") + "." +
                dt.Second.ToString("00") + "." +
                dt.Millisecond.ToString("000") + " " + s;
            System.Diagnostics.Debug.Print(line);
#endif
        }
    }
}


