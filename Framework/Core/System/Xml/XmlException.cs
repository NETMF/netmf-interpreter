////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
    /// <summary>
    /// Returns information about the last XML related exception.
    /// </summary>
    public class XmlException : Exception
    {
        /// <summary>
        /// Error codes that specifiy the types of XML exceptions. This is used
        /// in place of an error message to conserve memory footprint.
        /// </summary>
        public enum XmlExceptionErrorCode : int
        {
            Others = unchecked((int)0xD2000000),                    // XML_E_ERROR
            UnexpectedEOF = unchecked((int)0xD3000000),             // XML_E_UNEXPECTED_EOF
            BadNameChar = unchecked((int)0xD4000000),               // XML_E_BAD_NAME_CHAR
            UnknownEncoding = unchecked((int)0xD5000000),           // XML_E_UNKNOWN_ENCODING
            UnexpectedToken = unchecked((int)0xD6000000),           // XML_E_UNEXPECTED_TOKEN
            TagMismatch = unchecked((int)0xD7000000),               // XML_E_TAG_MISMATCH
            UnexpectedEndTag = unchecked((int)0xD8000000),          // XML_E_UNEXPECTED_END_TAG
            BadAttributeChar = unchecked((int)0xD9000000),          // XML_E_BAD_ATTRIBUTE_CHAR
            MultipleRoots = unchecked((int)0xDA000000),             // XML_E_MULTIPLE_ROOTS
            InvalidRootData = unchecked((int)0xDB000000),           // XML_E_INVALID_ROOT_DATA
            XmlDeclNotFirst = unchecked((int)0xDC000000),           // XML_E_XML_DECL_NOT_FIRST
            InvalidXmlDecl = unchecked((int)0xDD000000),            // XML_E_INVALID_XML_DECL
            InvalidXmlSpace = unchecked((int)0xDE000000),           // XML_E_INVALID_XML_SPACE
            DupAttributeName = unchecked((int)0xDF000000),          // XML_E_DUP_ATTRIBUTE_NAME
            InvalidCharacter = unchecked((int)0xE0000000),          // XML_E_INVALID_CHARACTER
            CDATAEndInText = unchecked((int)0xE1000000),            // XML_E_CDATA_END_IN_TEXT
            InvalidCommentChars = unchecked((int)0xE2000000),       // XML_E_INVALID_COMMENT_CHARS
            LimitExceeded = unchecked((int)0xE3000000),             // XML_E_LIMIT_EXCEEDED
            BadOrUnsupportedEntity = unchecked((int)0xE4000000),    // XML_E_BAD_OR_UNSUPPORTED_ENTITY
            UndeclaredNamespace = unchecked((int)0xE5000000),       // XML_E_UNDECLARED_NAMESPACE
            InvalidXmlPrefixMapping = unchecked((int)0xE6000000),   // XML_E_INVALID_XML_PREFIX_MAPPING
            NamespaceDeclXmlXmlns = unchecked((int)0xE7000000),     // XML_E_NAMESPACE_DECL_XML_XMLNS
            InvalidPIName = unchecked((int)0xE8000000),             // XML_E_INVALID_PI_NAME
            DTDIsProhibited = unchecked((int)0xE9000000),           // XML_E_DTD_IS_PROHIBITED
            EmptyName = unchecked((int)0xEA000000),                 // XML_E_EMPTY_NAME

            InvalidNodeType = unchecked((int)0xEB000000),           // XML_E_INVALID_NODE_TYPE
            ElementNotFound = unchecked((int)0xEC000000),           // XML_E_ELEMENT_NOT_FOUND
        }

        internal enum XmlExceptionErrorCodeInternal : int
        {
            ReturnToManagedCode = unchecked((int)0xD0000000),       // XML_S_RETURN_TO_MANAGED_CODE
            NeedMoreData = unchecked((int)0xD1000000),              // XML_E_NEED_MORE_DATA
        }

        /// <summary>
        /// Initializes a new instance of the XmlException class.
        /// </summary>
        public XmlException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlException class with a specified error message.
        /// </summary>
        /// <param name="message">The error description.</param>
        public XmlException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlException class with a specified error message
        /// and an error code.
        /// </summary>
        /// <param name="errorCode">The error code indicating the type of error.</param>
        public XmlException(XmlExceptionErrorCode errorCode)
            : base()
        {
            m_HResult = (int)errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the XmlException class.
        /// </summary>
        /// <param name="message">The error description.</param>
        /// <param name="innerException">The Exception that threw the XmlException, if any. This value can be null.</param>
        public XmlException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Returns the error code indicating the type of error of this XmlException.
        /// </summary>
        public XmlExceptionErrorCode ErrorCode
        {
            get { return (XmlExceptionErrorCode)m_HResult; }
        }
    };
}


