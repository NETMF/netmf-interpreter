using System;

namespace System.Xml
{
    /// <summary>
    /// Summary description for Res.
    /// </summary>";
    internal class Res
    {
        //ISSUE (prasank): only the constants being used from the Xml classes are
        //defined here. removed all other constants (there were nearly 500 of them).
        public const int Xml_UserException = 0;
        public const int Xml_DefaultException = 1;
        public const int Xml_InvalidOperation = 2;
        public const int Xml_UnclosedQuote = 3;
        public const int Xml_UnexpectedEOF = 4;
        public const int Xml_UnexpectedEOF1 = 5;
        public const int Xml_UnexpectedEOFInElementContent = 6;
        public const int Xml_BadStartNameChar = 7;
        public const int Xml_BadNameChar = 8;
        public const int Xml_BadDecimalEntity = 9;
        public const int Xml_BadHexEntity = 10;
        public const int Xml_MissingByteOrderMark = 11;
        public const int Xml_UnknownEncoding = 12;
        public const int Xml_InvalidCharInThisEncoding = 13;
        public const int Xml_ErrorPosition = 14;
        public const int Xml_UnexpectedTokenEx = 15;
        public const int Xml_UnexpectedToken = 16;
        public const int Xml_UnexpectedTokens2 = 17;
        public const int Xml_ExpectingWhiteSpace = 18;
        public const int Xml_TagMismatch = 19;
        public const int Xml_UnexpectedEndTag = 20;
        public const int Xml_BadAttributeChar = 21;
        public const int Xml_MissingRoot = 22;
        public const int Xml_MultipleRoots = 23;
        public const int Xml_InvalidRootData = 24;
        public const int Xml_XmlDeclNotFirst = 25;
        public const int Xml_InvalidXmlDecl = 26;
        public const int Xml_InvalidNodeType = 27;
        public const int Xml_InvalidPIName = 28;
        public const int Xml_InvalidXmlSpace = 29;
        public const int Xml_InvalidVersionNumber = 30;
        public const int Xml_DupAttributeName = 31;
        public const int Xml_BadDTDLocation = 32;
        public const int Xml_ElementNotFound = 33;
        public const int Xml_ElementNotFoundNs = 34;
        public const int Xml_InvalidCharacter = 35;
        public const int Xml_InvalidBinHexValue = 36;
        public const int Xml_InvalidBinHexValueOddCount = 37;
        public const int Xml_InvalidTextDecl = 38;
        public const int Xml_InvalidBase64Value = 39;
        public const int Xml_WhitespaceHandling = 40;
        public const int Xml_InvalidResetStateCall = 41;
        public const int Xml_EntityHandling = 42;
        public const int Xml_CDATAEndInText = 43;
        public const int Xml_ConformanceLevel = 44;
        public const int Xml_ReadOnlyProperty = 45;
        public const int Xml_EncodingSwitchAfterResetState = 46;
        public const int Xml_ReadSubtreeNotOnElement = 47;
        public const int Xml_ReadBinaryContentNotSupported = 48;
        public const int Xml_ReadValueChunkNotSupported = 49;
        public const int Xml_InvalidReadContentAs = 50;
        public const int Xml_InvalidReadElementContentAs = 51;
        public const int Xml_MixedReadElementContentAs = 52;
        public const int Xml_MixingReadValueChunkWithBinary = 53;
        public const int Xml_MixingBinaryContentMethods = 54;
        public const int Xml_MixingV1StreamingWithV2Binary = 55;
        public const int Xml_InvalidReadValueChunk = 56;
        public const int Xml_ReadContentAsFormatException = 57;
        public const int Xml_InvalidCommentChars = 58;
        public const int Xml_EmptyName = 59;
        public const int Enc_InvalidByteInEncoding = 60;

        //ISSUE (prasank): the error messages corresponding to the above error codes
        //are defined inline here. need to move this to a localizable resource.
        static string[] Xml_Errors =
        {
                "{0}",
                "An XML error has occurred.",
                "The operation is not valid due to the current state of the object.",
                "There is an unclosed literal string.",
                "An unexpected end of file while parsing {0} has occurred.",
                "An unexpected end of file has occurred.",
                "An unexpected end of file has occurred. Following elements are not closed: {0}",
                "A name cannot begin with the '{0}' character, hexadecimal value {1}.",
                "The '{0}' character, hexadecimal value {1}, cannot be included in a name.",
                "This is an invalid syntax for a decimal numeric entity reference.",
                "This is an invalid syntax for a hexadecimal numeric entity reference.",
                "There is no Unicode byte order mark. Cannot switch to Unicode.",
                "The system does not support '{0}' encoding.",
                "There is an invalid character in the given encoding.",
                "Line {0}, position {1}.",
                "'{0}' is an unexpected token. The expected token is '{1}'.",
                "'{0}' is an unexpected token. The expected token is '{1}'.",
                "'{0}' is an unexpected token. The expected token is '{1}' or '{2}'.",
                "'{0}' is an unexpected token. Expecting white space.",
                "The '{0}' start tag on line {1} does not match the end tag of '{2}'.",
                "There is an unexpected end tag.",
                "'{0}', hexadecimal value {1}, is an invalid attribute character.",
                "The root element is missing.",
                "There are multiple root elements.",
                "The data at the root level is invalid.",
                "This is an unexpected XML declaration. The XML declaration must be the first node in the document and no white space characters are allowed to appear before it.",
                "The syntax for an XML declaration is invalid.",
                "'{0}' is an invalid XmlNodeType.",
                "'{0}' is an invalid name for processing instructions.",
                "'{0}' is an invalid xml:space value.",
                "The version number '{0}' is invalid.",
                "'{0}' is a duplicate attribute name.",
                "Unexpected DTD declaration.",
                "The element '{0}' was not found.",
                "The element '{0}' with namespace name '{1}' was not found.",
                "'{0}', hexadecimal value {1}, is an invalid character.",
                "'{0}' is not a valid BinHex text sequence.",
                "'{0}' is not a valid BinHex text sequence. The sequence must contain an even number of characters.",
                "This is an invalid text declaration.",
                "'{0}' is not a valid Base64 text sequence.",
                "Expected WhitespaceHandling.None or WhitespaceHandling.All or WhitespaceHandling.Significant.",
                "Cannot call ResetState when parsing an XML fragment.",
                "Expected EntityHandling.ExpandEntities or EntityHandling.ExpandCharEntities.",
                "']]>' is not allowed in character data.",
                "Expected ConformanceLevel Document, Fragment or None.",
                "The '{0}' property is read only and cannot be set.",
                "'{0}' is an invalid value for 'encoding' attribute. The encoding cannot be switched after a call to ResetState.",
                "ReadSubtree() can be called only if the reader is on an element node.",
                "{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.",
                "ReadValueChunk method is not supported on this XmlReader. Use CanReadValueChunk property to find out if an XmlReader implements it.",
                "{0} method is not supported on node type {1}. If you want to read typed content of an element use ReadElementContentAs method.",
                "{0} method is not supported on node type {1}.",
                "ReadElementContentAs() methods cannot be called on an element that has child elements.",
                "ReadValueChunk calls can not be mixed with ReadContentAsBase64 or ReadContentAsBinHex.",
                "ReadContentAsBase64 and ReadContentAsBinHex method calls cannot be mixed with calls to ReadElementContentAsBase64 and ReadElementContentAsBinHex.",
                "ReadContentAsBase64 and ReadContentAsBinHex method calls cannot be mixed with calls to ReadChars, ReadBase64 and ReadBinHex.",
                "ReadValueAsChunk method is not supported on node type {0}.",
                "The content cannot be converted to the type {0}.",
                "An XML comment cannot have '--' inside, and '-' cannot be the last character.",
                "The empty string '' is not a valid name.",
                "Invalid byte was found at index {0}."
        };

        public static string GetString(int res)
        {
            return Xml_Errors[res];
        }

        public static string GetString(int res, object arg)
        {
            return Format(Xml_Errors[res], arg);
        }

        public static string GetString(int res, object[] args)
        {
            return Format(Xml_Errors[res], args);
        }

        public static string GetString(int res, string arg)
        {
            return Format(Xml_Errors[res], arg);
        }

        public static string GetString(int res, string arg1, string arg2)
        {
            return Format(Xml_Errors[res], new string[] { arg1, arg2 });
        }

        private static string Format(string format, object arg0)
        {
            return Format(format, 0, arg0);
        }

        private static string Format(string format, object[] args)
        {
            string result = format;

            for (int i = 0; i < args.Length; ++i)
                result = Format(result, i, args[i]);

            return result;
        }

        //ISSUE (prasank): there is no equivalent of string.Format available so reinventing the wheel.
        //need to find out if there is a better way to do this.
        private static string Format(string format, int argIndex, object argValue)
        {
            string argSpecifier = "{" + argIndex + "}";
            int index = format.IndexOf(argSpecifier);

            if (index == -1) //argSpecifier not found, so nothing to replace
                return format;

            string result = format.Substring(0, index);

            if (argValue == null)
                result += "null";
            else
                result += argValue.ToString();

            result += format.Substring(index + argSpecifier.Length);

            return result;
        }
    }
}


