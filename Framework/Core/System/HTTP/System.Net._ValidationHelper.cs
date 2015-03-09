namespace System.Net
{
    using System;
    using System.Globalization;

    /// <summary>
    ///  Internal support class for Validation related stuff.
    /// </summary>
    internal class ValidationHelper
    {

        public static string[] EmptyArray = new string[0];

        public static string[] MakeEmptyArrayNull(string[] stringArray)
        {
            if (stringArray == null || stringArray.Length == 0)
            {
                return null;
            }
            else
            {
                return stringArray;
            }
        }

        public static string MakeStringNull(string stringValue)
        {
            if (stringValue == null || stringValue.Length == 0)
            {
                return null;
            }
            else
            {
                return stringValue;
            }
        }

        public static string MakeStringEmpty(string stringValue)
        {
            if (stringValue == null || stringValue.Length == 0)
            {
                return String.Empty;
            }
            else
            {
                return stringValue;
            }
        }

        public static bool IsBlankString(string stringValue)
        {
            if (stringValue == null || stringValue.Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static short ValidatePort(int port)
        {

            if ((port < IPEndPoint.MinPort) ||
                (port > IPEndPoint.MaxPort))
            {       // If upper 16b is not zero...
                throw new ArgumentOutOfRangeException("port");  // Then this is not valid port
            }

            return (short)port;
        }

        public static bool ValidateTcpPort(int port)
        {
            // on false, API should throw new ArgumentOutOfRangeException("port");
            return port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort;
        }

        public static void ValidateRange(int actual, int fromAllowed, int toAllowed)
        {
            if (actual > toAllowed || actual < fromAllowed)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        public static string ExceptionMessage(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            if (exception.InnerException == null)
            {
                return exception.Message;
            }

            return exception.Message + " (" + ExceptionMessage(exception.InnerException) + ")";
        }

        internal static readonly char[] InvalidParamChars =
            new char[]{
                '(',
                ')',
                '<',
                '>',
                '@',
                ',',
                ';',
                ':',
                '\\',
                '"',
                '\'',
                '/',
                '[',
                ']',
                '?',
                '=',
                '{',
                '}',
                ' ',
                '\t',
                '\r',
                '\n'};

    }
}


