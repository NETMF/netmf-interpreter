using System;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Test
{
    /// <summary>
    /// A string utility class.
    /// </summary>
    public class MFUtilities
    {


        #region RandomString

        private static Random s_random = new Random();

        /// <summary>
        /// Array of special characters that are allowed in File names
        /// </summary>
        public static readonly char[] SafeFileChars = new char[] { '!', '#', '$', '%', '\'', 
            '(', ')', '+', '-', '.', '@', '[', ']', '_', '`', '{', '}', '~' };

        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <returns>random string</returns>
        public static string GetRandomString()
        {
            return GetRandomString(-1, RandomType.None);
        }

        /// <summary>
        /// Generates a random string of the specified length.
        /// </summary>
        /// <param name="length">An integer specifying the length of the random string.</param>
        /// <returns>random string</returns>
        public static string GetRandomString(int length)
        {
            return GetRandomString(length, RandomType.None);
        }

        /// <summary>
        /// Generates a safe random string. 
        /// A safe string consists of only upper case and lower case english alphabets and numbers.
        /// </summary>
        /// <returns>random string</returns>
        public static string GetRandomSafeString()
        {
            return GetRandomString(-1, RandomType.String);
        }

        /// <summary>
        /// Generates a random safe string of the specified length.
        /// A safe string consists of only upper case and lower case english alphabets and numbers.
        /// </summary>
        /// <param name="length">An integer specifying the length of the random string.</param>
        /// <returns>random string</returns>
        public static string GetRandomSafeString(int length)
        {
            return GetRandomString(length, RandomType.String);
        }

        /// <summary>
        /// Generates a random high byte string of random length.
        /// </summary>
        /// <returns>random string</returns>
        public static string GetRandomHighByteString()
        {
            return GetRandomString(-1, RandomType.HighByte);
        }

        /// <summary>
        /// Generates a random high byte string of the specified length.
        /// </summary>
        /// <param name="length">An integer specifying the length of the random string.</param>
        /// <returns>random string</returns>
        public static string GetRandomHighByteString(int length)
        {
            return GetRandomString(length, RandomType.HighByte);
        }

        /// <summary>
        /// Generates a randome safe filename string.
        /// A safe string consists of only upper case, lower case, numbers, and SafeFileChars
        /// </summary>
        /// <returns>random string</returns>
        public static string GetRandomFileName()
        {
            return GetRandomString(-1, RandomType.File);
        }

        /// <summary>
        /// Generates a randome safe filename string of the specified length.
        /// A safe string consists of only upper case, lower case, numbers, and SafeFileChars
        /// </summary>
        /// <param name="length">An integer specifying the length of the random string</param>
        /// <returns>random string</returns>
        public static string GetRandomFileName(int length)
        {
            return GetRandomString(length, RandomType.File);
        }

        /// <summary>
        /// Used to specified RandomString type
        /// </summary>
        private enum RandomType : int
        {
            String = 0,   // only alpha numeric
            File = 1,     // alpha/numeric/SafeFileChars
            None = 2,     // ascii 32 -126
            HighByte = 3 // Beyond ascii 128 (into the abyss)
        }

        /// <summary>
        /// Used to generate random string of specified length and type
        /// </summary>
        /// <param name="length">An integer specifying the length on the random string</param>
        /// <param name="safe">A RandomType enum specifying random string type</param>
        /// <returns>random string</returns>
        private static string GetRandomString(int length, RandomType rnd)
        {
            // Negative numbers indicate a random string length of 10-20 is desired.
            if (length < 0)
                length = 10 + s_random.Next(11);

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                switch (rnd)
                {
                    case RandomType.String:
                        switch (s_random.Next(3))
                        {
                            case 0:
                                // Get a random char between ascii 65 and 90 (upper case alphabets).
                                chars[i] = (char)(65 + s_random.Next(26));
                                break;
                            case 1:
                                // Get a random char between ascii 97 and 122 (lower case alphabets).
                                chars[i] = (char)(97 + s_random.Next(26));
                                break;
                            case 2:
                                // Get a random number 0 - 9
                                chars[i] = (char)('0' + s_random.Next(10));
                                break;
                        }
                        break;
                    case RandomType.File:
                        // 10% use SafeFileChars
                        if (s_random.Next(10) == 0)
                        {
                            // Get a random char from SafeFileChars
                            chars[i] = SafeFileChars[s_random.Next(SafeFileChars.Length)];
                        }
                        else
                        {
                            goto case 0;  // value - RandomType.String
                        }
                        break;
                    case RandomType.None:
                        // Get a random char from ascii 32 - 126
                        chars[i] = (char)(32 + s_random.Next(95));
                        break;
                    case RandomType.HighByte:
                        // Get a random char from ascii 128 - 65535
                        chars[i] = (char)(128 + s_random.Next(65407));                 
                        break;
                }
            }
            return new string(chars);
        }

        #endregion RandomString

        #region RandomByte
        /// <summary>
        /// A utility method to generate a random byte.
        /// </summary>
        /// <returns></returns>
        public static byte GetRandomByte()
        {
            return (byte)s_random.Next();
        }

        /// <summary>
        /// A utility to a generate random byte array of specified length.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] GetRandomBytes(int length)
        {
            byte[] byteArr = new byte[length];

            s_random.NextBytes(byteArr);

            return byteArr;
        }
        #endregion RandomByte

        #region ConvertToHex
        /// <summary>
        /// A utility to Convert uint to Hex string
        /// </summary>
        /// <returns></returns>       
        public static string UintToHex(uint word)
        {
            string ret = new string("0x".ToCharArray());

            for (int shift = 12; shift >= 0; shift -= 4)
            {
                ret += ByteToChar((byte)(word >> shift));
            }
            return ret;
        }
        /// <summary>
        /// A utility to Convert byte to Hex string
        /// </summary>
        /// <returns></returns>    
        public static string ByteToHex(byte data)
        {
            string ret = new string("0x".ToCharArray());

            for (int shift = 4; shift >= 0; shift -= 4)
            {
                ret += ByteToChar((byte)(data >> shift));
            }
            return ret;
        }
        /// <summary>
        /// A utility to Convert byte to char
        /// </summary>
        /// <returns></returns>    
        private static char ByteToChar(byte nybble)
        {
            nybble &= 15;
            if (nybble < 10)
            {
                return (char)(nybble + (byte)'0');
            }
            else
            {
                return (char)((nybble - 10) + (byte)'A');
            }
        }
        #endregion ConvertToHex
    }
}