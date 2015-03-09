using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
namespace Microsoft.SPOT.Platform.Test
{
    class TestUtilities
    {
        public static long DottedDecimalToIp(byte a1, byte a2, byte a3, byte a4)
        {
            return (long)((ulong)a4 << 24 | (ulong)a3 << 16 | (ulong)a2 << 8 | (ulong)a1);
        }
        public static void PrintSslStreamProperties(SslStream sslStream)
        {
            Console.WriteLine("IsAuthenticated: {0}", sslStream.IsAuthenticated);
            Console.WriteLine("IsMutuallyAuthenticated: {0}", sslStream.IsMutuallyAuthenticated);
            Console.WriteLine("IsEncrypted: {0}", sslStream.IsEncrypted);
            Console.WriteLine("IsSigned: {0}", sslStream.IsSigned);
            Console.WriteLine("IsServer: {0}", sslStream.IsServer);
            Console.WriteLine("SslProtocol: {0}", sslStream.SslProtocol);
        }

        public static readonly char[] SafeFileChars = new char[] { '!', '#', '$', '%', '\'', '(', ')', '+', '-', '.', '@', '[', ']', '_', '`', '{', '}', '~' };

        private enum SafeType : int
        {
            String = 0,  // only alpha numeric
            File = 1,    // alpha/numeric/SafeFileChars
            None = 2     // ascii 32 -126
        }

        public static string GetRandomSafeString(int length)
        {
            return GetRandomString(length, SafeType.String);
        }

        private static string GetRandomString(int length, SafeType safe)
        {
            // randomize generator
            System.Random r = new Random (1);

            // Negative numbers indicate a random string length of 10-20 is desired.
            if (length < 0)
                length = 10 + r.Next(11);

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                switch (safe)
                {
                    case SafeType.String:
                        switch (r.Next(3))
                        {
                            case 0:
                                // Get a random char between ascii 65 and 90 (upper case alphabets).
                                chars[i] = (char)(65 + r.Next(26));
                                break;
                            case 1:
                                // Get a random char between ascii 97 and 122 (lower case alphabets).
                                chars[i] = (char)(97 + r.Next(26));
                                break;
                            case 2:
                                // Get a random number 0 - 9
                                chars[i] = (char)('0' + r.Next(10));
                                break;
                        }
                        break;
                    case SafeType.File:
                        // 10% use SafeFileChars
                        if (r.Next(10) == 0)
                        {
                            // Get a random char from SafeFileChars
                            chars[i] = SafeFileChars[r.Next(SafeFileChars.Length)];
                        }
                        else
                        {
                            goto case 0;  // value - SafeType.String
                        }
                        break;
                    case SafeType.None:
                        // Get a random char from ascii 32 - 126
                        chars[i] = (char)(32 + r.Next(95));
                        break;
                }
            }
            return new string(chars);
        }


    }

    public enum MFTestResults
    {
        Skip = 0,
        Pass = 1,
        Fail = 2,
        KnownFailure = 3,
    }

}
