////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace System
{
    //We don't want to implement this whole class, but VB needs an external function to convert any integer type to a Char.
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
    public static class Convert
    {
        [CLSCompliant(false)]
        public static char ToChar(ushort value)
        {
            return (char)value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(string value)
        {
            return (sbyte)ToInt64(value, true, SByte.MinValue, SByte.MaxValue);
        }

        public static byte ToByte(string value)
        {
            return (byte)ToInt64(value, false, Byte.MinValue, Byte.MaxValue);
        }

        public static short ToInt16(string value)
        {
            return (short)ToInt64(value, true, Int16.MinValue, Int16.MaxValue);
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(string value)
        {
            return (ushort)ToInt64(value, false, UInt16.MinValue, UInt16.MaxValue);;
        }

        public static int ToInt32(string value)
        {
            return (int)ToInt64(value, true, Int32.MinValue, Int32.MaxValue);
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(string value)
        {
            return (uint)ToInt64(value, false, UInt32.MinValue, UInt32.MaxValue);
        }

        public static long ToInt64(string value)
        {
            return ToInt64(value, true, Int64.MinValue, Int64.MaxValue);
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(string value)
        {
            return (ulong)ToInt64(value, false, 0, 0);
        }

        //--//

        public static int ToInt32(string hexNumber, int fromBase)
        {
            if (hexNumber == null)
                return 0;

            if (fromBase != 16)
                throw new ArgumentException();

            int result = 0;
            int digit;

            char[] hexDigit = hexNumber.Trim(' ').ToUpper().ToCharArray();

            // Trim hex sentinal if present 
            int len = hexDigit.Length;
            int i   = (len >= 2 && hexDigit[0] == '0' && hexDigit[1] == 'X') ? 2 : 0;

            // 8 hex chars == 4 bytes == sizeof(Int32)
            if ((len - i) > 8) throw new ArgumentException();

            // Convert hex to integer
            for (; i < len; i++)
            {
                char c = hexDigit[i];

                switch (c)
                {
                    case '0':
                        digit = 0;
                        break;
                    case '1':
                        digit = 1;
                        break;
                    case '2':
                        digit = 2;
                        break;
                    case '3':
                        digit = 3;
                        break;
                    case '4':
                        digit = 4;
                        break;
                    case '5':
                        digit = 5;
                        break;
                    case '6':
                        digit = 6;
                        break;
                    case '7':
                        digit = 7;
                        break;
                    case '8':
                        digit = 8;
                        break;
                    case '9':
                        digit = 9;
                        break;
                    case 'A':
                        digit = 10;
                        break;
                    case 'B':
                        digit = 11;
                        break;
                    case 'C':
                        digit = 12;
                        break;
                    case 'D':
                        digit = 13;
                        break;
                    case 'E':
                        digit = 14;
                        break;
                    case 'F':
                        digit = 15;
                        break;
                    default:
                        throw new ArgumentException();
                }

                result <<= 4;
                result += digit;
            }

            return result;
        }

        public static double ToDouble(string s)
        {
            if (s == null)
                return 0;

            s = s.Trim(' ').ToLower();

            if(s.Length == 0) return 0;

            int decimalpoint = s.IndexOf('.');
            int exp          = s.IndexOf('e');
            
            if (exp != -1 && decimalpoint > exp)
                throw new Exception();

            char [] chars           = s.ToCharArray();
            int     len             = chars.Length;
            double  power           = 0;
            double  rightDecimal    = 0;
            int     decLeadingZeros = 0;
            double  leftDecimal     = 0;
            int     leftDecLen      = 0;
            bool    isNeg           = chars[0] == '-';

            // convert the exponential portion to a number            
            if (exp != -1 && exp + 1 < len - 1)
            {
                int tmp;
                power = GetDoubleNumber(chars, exp + 1, len - (exp + 1), out tmp);
            }

            // convert the decimal portion to a number
            if (decimalpoint != -1)
            {
                double number;
                int decLen;

                if (exp == -1)
                {
                    decLen = len - (decimalpoint + 1);
                }
                else
                {
                    decLen = (exp - (decimalpoint + 1));
                }

                number = GetDoubleNumber(chars, decimalpoint + 1, decLen, out decLeadingZeros);

                rightDecimal = number * System.Math.Pow(10, -decLen);
            }

            // convert the integer portion to a number
            if (decimalpoint != 0)
            {
                int leadingZeros;
                
                     if (decimalpoint == -1 && exp == -1) leftDecLen = len;
                else if (decimalpoint != -1)              leftDecLen = decimalpoint;
                else                                      leftDecLen = exp;

                leftDecimal = GetDoubleNumber(chars, 0, leftDecLen, out leadingZeros);
                // subtract leading zeros from integer length
                leftDecLen -= leadingZeros;

                if (chars[0] == '-' || chars[0] == '+') leftDecLen--;
            }

            double value = 0;
            if (leftDecimal < 0)
            {
                value = -leftDecimal + rightDecimal;
                value = -value;
            }
            else
            {
                value = leftDecimal + rightDecimal;
            }

            // lets normalize the integer portion first
            while(leftDecLen > 1)
            {
                switch(leftDecLen)
                {
                    case 2:
                        value      /= 10.0;
                        power      += 1;
                        leftDecLen -= 1;
                        break;
                    case 3:
                        value      /= 100.0;
                        power      += 2;
                        leftDecLen -= 2;
                        break;                    
                    case 4:
                        value      /= 1000.0;
                        power      += 3;
                        leftDecLen -= 3;
                        break;
                    default:
                        value      /= 10000.0;
                        power      += 4;
                        leftDecLen -= 4;
                        break;
                }
            }

            // now normalize the decimal portion
            if (value != 0.0 && value < 1.0 && value > -1.0)
            {
                // for normalization we want x.xxx instead of 0.xxx
                decLeadingZeros++;

                while(decLeadingZeros > 0)
                {
                    switch (decLeadingZeros)
                    {
                        case 1:
                            value           *= 10.0;
                            power           -= 1;
                            decLeadingZeros -= 1;
                            break;
                        case 2:
                            value           *= 100.0;
                            power           -= 2;
                            decLeadingZeros -= 2;
                            break;
                        case 3:
                            value           *= 1000.0;
                            power           -= 3;
                            decLeadingZeros -= 3;
                            break;
                        default:
                            value           *= 10000.0;
                            power           -= 4;
                            decLeadingZeros -= 4;
                            break;
                    }
                }
            }

            // special case for epsilon (the System.Math.Pow native method will return zero for -324)
            if (power == -324)
            {
                value = value * System.Math.Pow(10, power + 1);
                value /= 10.0;
            }
            else
            {
                value = value * System.Math.Pow(10, power);
            }

            if (value == double.PositiveInfinity || value == double.NegativeInfinity)
            {
                throw new Exception();
            }

            if(isNeg && value > 0)
            {
                value = -value;
            }

            return value;
        }

        //--//

        private static long ToInt64(string value, bool signed, long min, long max)
        {
            if (value == null)
                return 0;

            value = value.Trim(' ');

            char[] num    = value.ToCharArray();
            int    len    = num.Length;
            ulong  result = 0;
            int    index  = 0;
            bool   isNeg  = false;

            // check the sign
            if (num[0] == '-')
            {
                isNeg = true;
                index = 1;
            }
            else if (num[0] == '+')
            {
                index = 1;
            }
            
            for (int i = index; i < len; i++)
            {
                ulong digit;
                char c = num[i];

                // switch statement is faster than subtracting '0'
                switch(c)
                {
                    case '0':
                        digit = 0;
                        break;
                    case '1':
                        digit = 1;
                        break;
                    case '2':
                        digit = 2;
                        break;
                    case '3':
                        digit = 3;
                        break;
                    case '4':
                        digit = 4;
                        break;
                    case '5':
                        digit = 5;
                        break;
                    case '6':
                        digit = 6;
                        break;
                    case '7':
                        digit = 7;
                        break;
                    case '8':
                        digit = 8;
                        break;
                    case '9':
                        digit = 9;
                        break;
                    default:
                        throw new Exception();
                }

                // check for overflow - any number greater than this number will cause an overflow
                // when multiplied by 10
                if(( signed && result > 0x0CCCCCCCCCCCCCCC) || 
                   (!signed && result > 0x1999999999999999))
                {
                    throw new Exception();
                }

                result *= 10;
                result += digit;
            }

            if (isNeg && !signed && result != 0) throw new Exception();

            long res;

            if (isNeg)
            {
                res = -(long)result;

                // if the result is not negative, we had an overflow
                if(res > 0) throw new Exception();
            }
            else
            {
                res = (long)result;

                // if the result is negative and we are not converting a
                // UInt64, we had an overflow
                if(max != 0 && res < 0) throw new Exception();
            }

            // final check for max/min
            if (max != 0 && (res < min || res > max)) throw new Exception();

            return res;
        }

        private static double GetDoubleNumber(char[] chars, int start, int length, out int numLeadingZeros)
        {
            double number = 0;
            bool   isNeg  = false;
            int    end    = start + length;

            numLeadingZeros = 0;

            if(chars[start] == '-')
            {
                isNeg      = true;
                start++;
            }
            else if(chars[start] == '+')
            {
                start++;
            }

            for (int i = start; i < end; i++)
            {
                int  digit;
                char c = chars[i];

                // switch statement is faster than subtracting '0'                
                switch(c)
                {
                    case '0':
                        // update the number of leading zeros (used for normalizing)
                        if((numLeadingZeros + start) == i)
                        {
                            numLeadingZeros++;
                        }
                        digit = 0;
                        break;
                    case '1':
                        digit = 1;
                        break;
                    case '2':
                        digit = 2;
                        break;
                    case '3':
                        digit = 3;
                        break;
                    case '4':
                        digit = 4;
                        break;
                    case '5':
                        digit = 5;
                        break;
                    case '6':
                        digit = 6;
                        break;
                    case '7':
                        digit = 7;
                        break;
                    case '8':
                        digit = 8;
                        break;
                    case '9':
                        digit = 9;
                        break;
                    default:
                        throw new Exception();
                }

                number *= 10;
                number += digit;
            }

            return isNeg ? -number : number;
        }

        /// <summary>
        /// Conversion array from 6 bit of value into base64 encoded character.
        /// </summary>
        static char[] s_rgchBase64EncodingDefault = new char[]
        {
           'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', /* 12 */
           'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', /* 24 */
           'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', /* 36 */
           'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', /* 48 */
           'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', /* 60 */
           '8', '9', '!', '*'            /* 64 */
        };

        static char[] s_rgchBase64EncodingRFC4648 = new char[]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', /* 12 */
            'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', /* 24 */
            'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', /* 36 */
            'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', /* 48 */
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', /* 60 */
            '8', '9', '+', '/'            /* 64 */
        };

        static char[] s_rgchBase64Encoding = s_rgchBase64EncodingDefault;

        static byte[] s_rgbBase64Decode = new byte[]
        {
            // Note we also accept ! and + interchangably.
            // Note we also accept * and / interchangably.
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, /*   0 -   7 */
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, /*   8 -  15 */
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, /*  16 -  23 */
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, /*  24 -  31 */
            0x00, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, /*  32 -  39 */
            0x00, 0x00, 0x3f, 0x3e, 0x00, 0x00, 0x00, 0x3f, /*  40 -  47 */
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, /*  48 -  55 */
            0x3c, 0x3d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, /*  56 -  63 */
            0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, /*  64 -  71 */
            0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, /*  72 -  79 */
            0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, /*  80 -  87 */
            0x17, 0x18, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, /*  88 -  95 */
            0x00, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, /*  96 - 103 */
            0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, /* 104 - 111 */
            0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, /* 112 - 119 */
            0x31, 0x32, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00  /* 120 - 127 */
        };

        private const int CCH_B64_IN_QUARTET = 4;
        private const int CB_B64_OUT_TRIO = 3;

        static private int GetBase64EncodedLength(int binaryLen)
        {
            return (((binaryLen / 3) + (((binaryLen % 3) != 0) ? 1 : 0)) * 4);

        }

        public static bool UseRFC4648Encoding
        {
            get { return s_rgchBase64Encoding == s_rgchBase64EncodingRFC4648; }
            set { s_rgchBase64Encoding = (value ? s_rgchBase64EncodingRFC4648 : s_rgchBase64EncodingDefault); }
        }

        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent String representation encoded with base 64 digits.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers. </param>
        /// <returns>The String representation, in base 64, of the contents of inArray.</returns>
        public static string ToBase64String(byte[] inArray)
        {
            return ToBase64String(inArray, 0, inArray.Length);
        }

        public static string ToBase64String(byte[] inArray, int offset, int length)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException();
            }

            if(length == 0) return "";

            if(offset + length > inArray.Length) throw new ArgumentOutOfRangeException();

            // Create array of characters with appropriate length.
            int inArrayLen = length;
            int outArrayLen = GetBase64EncodedLength(inArrayLen);
            char[] outArray = new char[outArrayLen];

            /* encoding starts from end of string */

            /*
            ** Convert the input buffer bytes through the encoding table and
            ** out into the output buffer.
            */
            int iInputEnd = offset + (outArrayLen / CCH_B64_IN_QUARTET - 1) * CB_B64_OUT_TRIO;
            int iInput = offset, iOutput = 0;
            byte uc0 = 0, uc1 = 0, uc2 = 0;
            // Loop is for all trios except of last one.
            for (; iInput < iInputEnd; iInput += CB_B64_OUT_TRIO, iOutput += CCH_B64_IN_QUARTET)
            {
                uc0 = inArray[iInput];
                uc1 = inArray[iInput + 1];
                uc2 = inArray[iInput + 2];
                // Writes data to output character array.
                outArray[iOutput] = s_rgchBase64Encoding[uc0 >> 2];
                outArray[iOutput + 1] = s_rgchBase64Encoding[((uc0 << 4) & 0x30) | ((uc1 >> 4) & 0xf)];
                outArray[iOutput + 2] = s_rgchBase64Encoding[((uc1 << 2) & 0x3c) | ((uc2 >> 6) & 0x3)];
                outArray[iOutput + 3] = s_rgchBase64Encoding[uc2 & 0x3f];
            }

            // Now we process the last trio of bytes. This trio might be incomplete and thus require special handling.
            // This code could be incorporated into main "for" loop, but hte code would be slower becuase of extra 2 "if"
            uc0 = inArray[iInput];
            uc1 = ((iInput + 1) < (offset + inArrayLen)) ? inArray[iInput + 1] : (byte)0;
            uc2 = ((iInput + 2) < (offset + inArrayLen)) ? inArray[iInput + 2] : (byte)0;
            // Writes data to output character array.
            outArray[iOutput] = s_rgchBase64Encoding[uc0 >> 2];
            outArray[iOutput + 1] = s_rgchBase64Encoding[((uc0 << 4) & 0x30) | ((uc1 >> 4) & 0xf)];
            outArray[iOutput + 2] = s_rgchBase64Encoding[((uc1 << 2) & 0x3c) | ((uc2 >> 6) & 0x3)];
            outArray[iOutput + 3] = s_rgchBase64Encoding[uc2 & 0x3f];

            switch (inArrayLen % CB_B64_OUT_TRIO)
            {
                /*
                ** One byte out of three, add padding and fall through
                */
                case 1:
                    outArray[outArrayLen - 2] = '=';
                    goto case 2;
                /*
                ** Two bytes out of three, add padding.
                */
                case 2:
                    outArray[outArrayLen - 1] = '=';
                    break;
            }

            // Creates string out of character array and return it.
            return new string(outArray);
        }

        /// <summary>
        /// Converts the specified String, which encodes binary data as base 64 digits, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="inString">Base64 encoded string to convert</param>
        /// <returns>An array of 8-bit unsigned integers equivalent to s.</returns>
        /// <remarks>s is composed of base 64 digits, white space characters, and trailing padding characters.
        /// The base 64 digits in ascending order from zero are the uppercase characters 'A' to 'Z',
        /// lowercase characters 'a' to 'z', numerals '0' to '9', and the symbols '+' and '/'.
        /// An arbitrary number of white space characters can appear in s because all white space characters are ignored.
        /// The valueless character, '=', is used for trailing padding. The end of s can consist of zero, one, or two padding characters.
        /// </remarks>
        public static byte[] FromBase64String(string inString)
        {
            if (inString == null)
            {
                throw new ArgumentNullException();
            }

            char []chArray = inString.ToCharArray();
            
            return FromBase64CharArray(chArray, 0, chArray.Length);
        }

        public static byte[] FromBase64CharArray(char[] inString, int offset, int length)
        {
            if(length == 0) return new byte[0];

            // Checks that length of string is multiple of 4
            int inLength = length;
            if (inLength % CCH_B64_IN_QUARTET != 0)
            {
                throw new ArgumentException("Encoded string length should be multiple of 4");
            }

            // Maximum buffer size needed.
            int outCurPos = (((inLength + (CCH_B64_IN_QUARTET - 1)) / CCH_B64_IN_QUARTET) * CB_B64_OUT_TRIO);
            if (inString[offset + inLength - 1] == '=')
            {   // If the last was "=" - it means last byte was padded/
                --outCurPos;
                // If one more '=' - two bytes were actually padded.
                if (inString[offset + inLength - 2] == '=')
                {
                    --outCurPos;
                }
            }

            // Output array.
            byte[] retArray = new byte[outCurPos];
            // Array of 4 bytes - temporary.
            byte[] rgbOutput = new byte[CCH_B64_IN_QUARTET];
            // Loops over each 4 bytes quartet.
            for (int inCurPos = offset + inLength;
                 inCurPos > offset;
                 inCurPos -= CCH_B64_IN_QUARTET)
            {
                int ibDest = 0;
                for (; ibDest < CB_B64_OUT_TRIO + 1; ibDest++)
                {
                    int ichGet = inCurPos + ibDest - CCH_B64_IN_QUARTET;
                    // Equal sign can be only at the end and maximum of 2
                    if (inString[ichGet] == '=')
                    {
                        if (ibDest < 2 || inCurPos != (offset + inLength))
                        {
                            throw new ArgumentException("Invalid base64 encoded string");
                        }
                        break;
                    }

                    // Applies decoding table to the character.
                    rgbOutput[ibDest] = s_rgbBase64Decode[inString[ichGet]];
                }

                // Convert 4 bytes in rgbOutput, each having 6 bits into three bytes in final data.
                switch (ibDest)
                {
                    default:
                        retArray[--outCurPos] = (byte)(((rgbOutput[2] & 0x03) << 6) | rgbOutput[3]);
                        goto case 3;

                    case 3:
                        retArray[--outCurPos] = (byte)(((rgbOutput[1] & 0x0F) << 4) | (((rgbOutput[2]) & 0x3C) >> 2));
                        goto case 2;

                    case 2:
                        retArray[--outCurPos] = (byte)(((rgbOutput[0]) << 2) | (((rgbOutput[1]) & 0x30) >> 4));
                        break;
                }
            }

            return retArray;
        }
    }
}


