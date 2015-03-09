////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using Microsoft.SPOT;

namespace System.Xml
{

    // <summary>
    // Contains various static functions and methods for parsing and validating:
    //     NCName (not namespace-aware, no colons allowed)
    //     QName (prefix:local-name)
    // </summary>
    internal class ValidateNames
    {
        // Not creatable
        private ValidateNames()
        {
        }

        public enum Flags
        {
            NCNames = 0x1,              // Validate that each non-empty prefix and localName is a valid NCName
            CheckLocalName = 0x2,       // Validate the local-name
            CheckPrefixMapping = 0x4,   // Validate the prefix --> namespace mapping
            All = 0x7,
            AllExceptNCNames = 0x6,
            AllExceptPrefixMapping = 0x3,
        };

        //-----------------------------------------------
        // NCName parsing
        //-----------------------------------------------

        // <summary>
        // Attempts to parse the input string as an NCName (see the XML Namespace spec).
        // Quits parsing when an invalid NCName char is reached or the end of string is reached.
        // Returns the number of valid NCName chars that were parsed.
        // </summary>
        public static unsafe int ParseNCName(string s, int offset)
        {
            int offsetStart = offset;
            XmlCharType xmlCharType = XmlCharType.Instance;
            Debug.Assert(s != null && offset <= s.Length);

            // Quit if the first character is not a valid NCName starting character
            if (offset < s.Length &&
                (s[offset] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[s[offset]] & XmlCharType.fNCStartName) != 0))
            { // xmlCharType.IsStartNCNameChar(s[offset])) {

                // Keep parsing until the end of string or an invalid NCName character is reached
                for (offset++; offset < s.Length; offset++)
                {
                    if (!(s[offset] > XmlCharType.MaxAsciiChar || (xmlCharType.charProperties[s[offset]] & XmlCharType.fNCName) != 0)) // if (!xmlCharType.IsNCNameChar(s[offset]))
                        break;
                }
            }

            return offset - offsetStart;
        }

        // <summary>
        // Calls parseName and throws exception if the resulting name is not a valid NCName.
        // Returns the input string if there is no error.
        // </summary>
        public static string ParseNCNameThrow(string s)
        {
            ParseNCNameInternal(s, true);
            return s;
        }

        // <summary>
        // Calls parseName and returns false or throws exception if the resulting name is not
        // a valid NCName.  Returns the input string if there is no error.
        // </summary>
        private static bool ParseNCNameInternal(string s, bool throwOnError)
        {
            int len = ParseNCName(s, 0);

            if (len == 0 || len != s.Length)
            {
                // If the string is not a valid NCName, then throw or return false
                if (throwOnError) ThrowInvalidName(s, 0, len);
                return false;
            }

            return true;
        }

        //-----------------------------------------------
        // QName parsing
        //-----------------------------------------------

        // <summary>
        // Attempts to parse the input string as a QName (see the XML Namespace spec).
        // Quits parsing when an invalid QName char is reached or the end of string is reached.
        // Returns the number of valid QName chars that were parsed.
        // Sets colonOffset to the offset of a colon character if it exists, or 0 otherwise.
        // </summary>
        public static int ParseQName(string s, int offset, out int colonOffset)
        {
            int len, lenLocal;

            // Assume no colon
            colonOffset = 0;

            // Parse NCName (may be prefix, may be local name)
            len = ParseNCName(s, offset);
            if (len != 0)
            {

                // Non-empty NCName, so look for colon if there are any characters left
                offset += len;
                if (offset < s.Length && s[offset] == ':')
                {

                    // First NCName was prefix, so look for local name part
                    lenLocal = ParseNCName(s, offset + 1);
                    if (lenLocal != 0)
                    {
                        // Local name part found, so increase total QName length (add 1 for colon)
                        colonOffset = offset;
                        len += lenLocal + 1;
                    }
                }
            }

            return len;
        }

        // <summary>
        // Calls parseQName and throws exception if the resulting name is not a valid QName.
        // Returns the prefix and local name parts.
        // </summary>
        public static void ParseQNameThrow(string s, out string prefix, out string localName)
        {
            int colonOffset;
            int len = ParseQName(s, 0, out colonOffset);

            if (len == 0 || len != s.Length)
            {
                // If the string is not a valid QName, then throw
                ThrowInvalidName(s, 0, len);
            }

            if (colonOffset != 0)
            {
                prefix = s.Substring(0, colonOffset);
                localName = s.Substring(colonOffset + 1);
            }
            else
            {
                prefix = "";
                localName = s;
            }
        }

        // <summary>
        // Parses the input string as a NameTest (see the XPath spec), returning the prefix and
        // local name parts.  Throws an exception if the given string is not a valid NameTest.
        // If the NameTest contains a star, null values for localName (case NCName':*'), or for
        // both localName and prefix (case '*') are returned.
        // </summary>
        public static void ParseNameTestThrow(string s, out string prefix, out string localName)
        {
            int len, lenLocal, offset;

            if (s.Length != 0 && s[0] == '*')
            {
                // '*' as a NameTest
                prefix = localName = null;
                len = 1;
            }
            else
            {
                // Parse NCName (may be prefix, may be local name)
                len = ParseNCName(s, 0);
                if (len != 0)
                {

                    // Non-empty NCName, so look for colon if there are any characters left
                    localName = s.Substring(0, len);
                    if (len < s.Length && s[len] == ':')
                    {

                        // First NCName was prefix, so look for local name part
                        prefix = localName;
                        offset = len + 1;
                        if (offset < s.Length && s[offset] == '*')
                        {
                            // '*' as a local name part, add 2 to len for colon and star
                            localName = null;
                            len += 2;
                        }
                        else
                        {
                            lenLocal = ParseNCName(s, offset);
                            if (lenLocal != 0)
                            {
                                // Local name part found, so increase total NameTest length
                                localName = s.Substring(offset, lenLocal);
                                len += lenLocal + 1;
                            }
                        }
                    }
                    else
                    {
                        prefix = "";
                    }
                }
                else
                {
                    // Make the compiler happy
                    prefix = localName = null;
                }
            }

            if (len == 0 || len != s.Length)
            {
                // If the string is not a valid NameTest, then throw
                ThrowInvalidName(s, 0, len);
            }
        }

        // <summary>
        // Throws an invalid name exception.
        // </summary>
        // <param name="s">String that was parsed.</param>
        // <param name="offsetStartChar">Offset in string where parsing began.</param>
        // <param name="offsetBadChar">Offset in string where parsing failed.</param>
        public static void ThrowInvalidName(string s, int offsetStartChar, int offsetBadChar)
        {
            // If the name is empty, throw an exception
            if (offsetStartChar >= s.Length)
                throw new XmlException(Res.Xml_EmptyName, "");

            Debug.Assert(offsetBadChar < s.Length);

            if (XmlCharType.Instance.IsNCNameChar(s[offsetBadChar]) && !XmlCharType.Instance.IsStartNCNameChar(s[offsetBadChar]))
            {
                // The error character is a valid name character, but is not a valid start name character
                throw new XmlException(Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionStr(s[offsetBadChar]));
            }
            else
            {
                // The error character is an invalid name character
                throw new XmlException(Res.Xml_BadNameChar, XmlException.BuildCharExceptionStr(s[offsetBadChar]));
            }
        }

        public static string GetInvalidNameErrorMessage(string s, int offsetStartChar, int offsetBadChar)
        {
            // If the name is empty, throw an exception
            if (offsetStartChar >= s.Length)
                return Res.GetString(Res.Xml_EmptyName, "");

            Debug.Assert(offsetBadChar < s.Length);

            if (XmlCharType.Instance.IsNCNameChar(s[offsetBadChar]) && !XmlCharType.Instance.IsStartNCNameChar(s[offsetBadChar]))
            {
                // The error character is a valid name character, but is not a valid start name character
                return Res.GetString(Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionStr(s[offsetBadChar]));
            }
            else
            {
                // The error character is an invalid name character
                return Res.GetString(Res.Xml_BadNameChar, XmlException.BuildCharExceptionStr(s[offsetBadChar]));
            }
        }

        // <summary>
        // Creates a colon-delimited qname from prefix and local name parts.
        // </summary>
        private static string CreateName(string prefix, string localName)
        {
            return (prefix.Length != 0) ? prefix + ":" + localName : localName;
        }
    }
}


