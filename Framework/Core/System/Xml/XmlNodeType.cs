////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
    /// <summary>
    /// Specifies the type of node.
    /// </summary>
    public enum XmlNodeType
    {
        /// <summary>
        /// This is returned by the XmlReader if a Read method has not been called.
        /// </summary>
        None = 0,

        /// <summary>
        /// An element (for example, &lt;item&gt; ).
        /// </summary>
        Element = 1,

        /// <summary>
        /// An attribute (for example, id='123' ).
        /// </summary>
        Attribute = 2,

        /// <summary>
        /// The text content of a node.
        /// </summary>
        Text = 3,

        /// <summary>
        /// A CDATA section (for example, &lt;![CDATA[my escaped text]]&gt; ).
        /// </summary>
        CDATA = 4,

        /// <summary>
        /// A processing instruction (for example, &lt;?pi test?&gt; ).
        /// </summary>
        ProcessingInstruction = 5,

        /// <summary>
        /// A comment (for example, &lt;!-- my comment --&gt; ).
        /// </summary>
        Comment = 6,

        /// <summary>
        /// White space between markup.
        /// </summary>
        Whitespace = 7,

        /// <summary>
        /// White space between markup in a mixed content model or white space within
        /// the xml:space="preserve" scope.
        /// </summary>
        SignificantWhitespace = 8,

        /// <summary>
        /// An end element tag (for example, &lt;/item&gt; ).
        /// </summary>
        EndElement = 9,

        /// <summary>
        /// The XML declaration (for example, &lt;?xml version='1.0'?&gt; ).
        /// </summary>
        XmlDeclaration = 10,
    }
}


