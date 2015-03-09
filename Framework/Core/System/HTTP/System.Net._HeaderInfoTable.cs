////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{

    using System.Collections;
    using System.Globalization;

    /// <summary>
    /// Internal class with utilities to validate HTTP headers.
    /// </summary>
    internal class HeaderInfoTable
    {

        private static HeaderInfo[] HeaderTable;

        private static HeaderParser SingleParser = new HeaderParser(ParseSingleValue);
        private static HeaderParser MultiParser = new HeaderParser(ParseMultiValue);

        private static string[] ParseSingleValue(string value)
        {
            return new string[1] { value };
        }

        /// <summary>
        /// Parses single HTTP header and separates values delimited by comma.
        /// Like "Content-Type: text, HTML". The value string "text, HTML" will se parsed into 2 strings.
        /// </summary>
        /// <param name="value">Value string with possible multivalue</param>
        /// <returns>Array of strings with single value in each. </returns>
        private static string[] ParseMultiValue(string value)
        {
            ArrayList tempCollection = new ArrayList();

            bool inquote = false;
            int chIndex = 0;
            char[] vp = new char[value.Length];
            string singleValue;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\"')
                {
                    inquote = !inquote;
                }
                else if ((value[i] == ',') && !inquote)
                {
                    singleValue = new String(vp, 0, chIndex);
                    tempCollection.Add(singleValue.Trim());
                    chIndex = 0;
                    continue;
                }

                vp[chIndex++] = value[i];
            }

            //
            // Now add the last of the header values to the stringtable.
            //

            if (chIndex != 0)
            {
                singleValue = new String(vp, 0, chIndex);
                tempCollection.Add(singleValue.Trim());
            }

            return (string[])tempCollection.ToArray(typeof(string));
        }

        /// <summary>
        /// Header info for non-standard headers.
        /// </summary>
        private static HeaderInfo UnknownHeaderInfo =
            new HeaderInfo(String.Empty, false, false, SingleParser);

        private static bool m_Initialized = Initialize();

        /// <summary>
        /// Initialize table with infomation for HTTP WEB headers.
        /// </summary>
        /// <returns></returns>
        private static bool Initialize()
        {

            HeaderTable = new HeaderInfo[] {
                new HeaderInfo(HttpKnownHeaderNames.Age, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Allow, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Accept, true, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Authorization, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptRanges, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptCharset, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptEncoding, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptLanguage, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Cookie, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Connection, true, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentMD5, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentType, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.CacheControl, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentRange, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLength, true, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentEncoding, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLanguage, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLocation, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Date, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.ETag, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Expect, true, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Expires, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.From, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Host, true, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfMatch, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.IfRange, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfNoneMatch, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.IfModifiedSince, true, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfUnmodifiedSince, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Location, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.LastModified, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.MaxForwards, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Pragma, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyAuthenticate, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyAuthorization, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyConnection, true, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Range, true, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Referer, true, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.RetryAfter, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Server, false, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.SetCookie, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.SetCookie2, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.TE, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Trailer, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.TransferEncoding, true , true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Upgrade, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.UserAgent, true, false, SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Via, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Vary, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Warning, false, true, MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.WWWAuthenticate, false, true, SingleParser)
            };

            return true;
        }

        /// <summary>
        /// Return HTTP header information from specified name of HTTP header.
        /// </summary>
        /// <param name="name">Name for HTTP header </param>
        /// <returns>HTTP header information</returns>
        internal HeaderInfo this[string name]
        {
            get
            {   // Return headerInfo with the same name
                string lowerCaseName = name.ToLower();
                for (int i = 0; i < HeaderTable.Length; i++)
                {
                    if (HeaderTable[i].HeaderName.ToLower() == lowerCaseName)
                    {
                        return HeaderTable[i];
                    }
                }

                // Return unknownInfo, instead of NULL
                return UnknownHeaderInfo;
            }
        }

    } // class HeaderInfoTable
} // namespace System.Net


