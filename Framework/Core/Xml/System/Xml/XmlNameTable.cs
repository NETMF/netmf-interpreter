////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{

    // <devdoc>
    //    <para> Table of atomized string objects. This provides an
    //       efficient means for the XML parser to use the same string object for all
    //       repeated element and attribute names in an XML document. This class is
    //    <see langword='abstract'/>
    //    .</para>
    // </devdoc>
    public abstract class XmlNameTable
    {
        // <devdoc>
        //    <para>Gets the atomized String object containing the same
        //       chars as the specified range of chars in the given char array.</para>
        // </devdoc>
        public abstract String Get(char[] array, int offset, int length);

        // <devdoc>
        //    <para>
        //       Gets the atomized String object containing the same
        //       value as the specified string.
        //    </para>
        // </devdoc>
        public abstract String Get(String array);

        // <devdoc>
        //    <para>Creates a new atom for the characters at the specified range
        //       of chararacters in the specified string.</para>
        // </devdoc>
        public abstract String Add(char[] array, int offset, int length);

        // <devdoc>
        //    <para>
        //       Creates a new atom for the specified string.
        //    </para>
        // </devdoc>
        public abstract String Add(String array);
    }
}


