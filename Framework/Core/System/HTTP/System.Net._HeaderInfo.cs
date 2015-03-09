////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{

    internal delegate string[] HeaderParser(string value);

    /// <summary>
    /// Internal supporting class for validation of HTTP Web Headers.
    /// </summary>
    internal class HeaderInfo
    {

        internal bool IsRestricted;
        internal HeaderParser Parser;

        /// <summary>
        /// Note that the HeaderName field is not always valid, and should not
        /// be used after initialization. In particular, the HeaderInfo returned
        /// for an unknown header will not have the correct header name.
        /// </summary>
        internal string HeaderName;
        internal bool AllowMultiValues;

        internal HeaderInfo(string name, bool restricted, bool multi, HeaderParser p)
        {
            HeaderName = name;
            IsRestricted = restricted;
            Parser = p;
            AllowMultiValues = multi;
        }
    }
} // namespace System.Net


