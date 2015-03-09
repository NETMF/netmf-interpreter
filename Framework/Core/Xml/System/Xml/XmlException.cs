////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
    using System;
    using System.IO;

    using System.Text;
    using System.Globalization;
    using System.Threading;

    using Microsoft.SPOT;

    // <devdoc>
    //    <para>Returns detailed information about the last parse error, including the error
    //       number, line number, character position, and a text description.</para>
    // </devdoc>
    public class XmlException : Exception
    {
        int res;
        int lineNumber;
        int linePosition;

        string sourceUri;

        // message != null for V1 exceptions deserialized in Whidbey
        // message == null for V2 or higher exceptions; the exception message is stored on the base class (Exception._message)
        string message = null;
        //provided to meet the ECMA standards
        public XmlException()
            : this(null)
        {
        }

        //provided to meet the ECMA standards
        public XmlException(String message)
            : this(message, ((Exception)null), 0, 0)
        {
#if DEBUG
            Debug.Assert(message == null || message.Length < 4 || message.Substring(0, 4) != "Xml_", "Do not pass a resource here!");
#endif
        }

        //provided to meet ECMA standards
        public XmlException(String message, Exception innerException)
            : this(message, innerException, 0, 0)
        {
        }

        //provided to meet ECMA standards
        public XmlException(String message, Exception innerException, int lineNumber, int linePosition) :
            this(message, innerException, lineNumber, linePosition, null)
        {
        }

        internal XmlException(String message, Exception innerException, int lineNumber, int linePosition, string sourceUri) :
            this((message == null ? Res.Xml_DefaultException : Res.Xml_UserException), new string[] { message }, innerException, lineNumber, linePosition, sourceUri)
        {
        }

        internal XmlException(int res, string[] args) :
            this(res, args, null, 0, 0, null) { }

        internal XmlException(int res, string[] args, string sourceUri) :
            this(res, args, null, 0, 0, sourceUri) { }

        internal XmlException(int res, string arg) :
            this(res, new string[] { arg }, null, 0, 0, null) { }

        internal XmlException(int res, string arg, string sourceUri) :
            this(res, new string[] { arg }, null, 0, 0, sourceUri) { }

        internal XmlException(int res, String arg, IXmlLineInfo lineInfo) :
            this(res, new string[] { arg }, lineInfo, null) { }

        internal XmlException(int res, String arg, Exception innerException, IXmlLineInfo lineInfo) :
            this(res, new string[] { arg }, innerException, (lineInfo == null ? 0 : lineInfo.LineNumber), (lineInfo == null ? 0 : lineInfo.LinePosition), null) { }

        internal XmlException(int res, String arg, IXmlLineInfo lineInfo, string sourceUri) :
            this(res, new string[] { arg }, lineInfo, sourceUri) { }

        internal XmlException(int res, string[] args, IXmlLineInfo lineInfo) :
            this(res, args, lineInfo, null) { }

        internal XmlException(int res, string[] args, IXmlLineInfo lineInfo, string sourceUri) :
            this(res, args, null, (lineInfo == null ? 0 : lineInfo.LineNumber), (lineInfo == null ? 0 : lineInfo.LinePosition), sourceUri)
        {
        }

        internal XmlException(int res, int lineNumber, int linePosition) :
            this(res, (string[])null, null, lineNumber, linePosition) { }

        internal XmlException(int res, string arg, int lineNumber, int linePosition) :
            this(res, new string[] { arg }, null, lineNumber, linePosition, null) { }

        internal XmlException(int res, string arg, int lineNumber, int linePosition, string sourceUri) :
            this(res, new string[] { arg }, null, lineNumber, linePosition, sourceUri) { }

        internal XmlException(int res, string[] args, int lineNumber, int linePosition) :
            this(res, args, null, lineNumber, linePosition, null) { }

        internal XmlException(int res, string[] args, int lineNumber, int linePosition, string sourceUri) :
            this(res, args, null, lineNumber, linePosition, sourceUri) { }

        internal XmlException(int res, string[] args, Exception innerException, int lineNumber, int linePosition) :
            this(res, args, innerException, lineNumber, linePosition, null) { }

        internal XmlException(int res, string[] args, Exception innerException, int lineNumber, int linePosition, string sourceUri) :
            base(CreateMessage(res, args, lineNumber, linePosition), innerException)
        {
            this.res = res;
            this.sourceUri = sourceUri;
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
        }

        private static string CreateMessage(int res, string[] args, int lineNumber, int linePosition)
        {
            string message = Res.GetString(res, args);

            if (lineNumber != 0)
            {
                string[] msg = new string[2];
                msg[0] = lineNumber.ToString();
                msg[1] = linePosition.ToString();
                message += " " + Res.GetString(Res.Xml_ErrorPosition, msg);
            }

            return message;
        }

        internal static string[] BuildCharExceptionStr(char ch)
        {
            string[] aStringList = new string[2];
            if ((int)ch == 0)
            {
                aStringList[0] = ".";
            }
            else
            {
                //                aStringList[0] = ch.ToString(CultureInfo.InvariantCulture);
                aStringList[0] = ch.ToString();
            }

            aStringList[1] = Utility.ToHexDigits((uint)(int)ch);
            return aStringList;
        }

        public int LineNumber
        {
            get { return this.lineNumber; }
        }

        public int LinePosition
        {
            get { return this.linePosition; }
        }

        public string SourceUri
        {
            get { return this.sourceUri; }
        }

        public override string Message
        {
            get
            {
                return (message == null) ? base.Message : message;
            }
        }

        internal int ResId
        {
            get
            {
                return res;
            }
        }

    };

    class XmlExceptionHelper
    {
        internal static ArgumentException CreateInvalidNameArgumentException(string name, string argumentName)
        {
            return (name == null) ? new ArgumentNullException(argumentName) : new ArgumentException(Res.GetString(Res.Xml_EmptyName), argumentName);
        }
    }

} // namespace System.Xml


