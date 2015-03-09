////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
    /// <summary>
    /// Specifies a set of features to support on the XmlReader object created
    /// by the Create method.
    /// </summary>
    public class XmlReaderSettings
    {
        internal const uint NativeIgnoreWhitespace = 0x01;
        internal const uint NativeIgnoreProcessingInstructions = 0x02;
        internal const uint NativeIgnoreComments = 0x04;

        internal uint GetSettings()
        {
            uint settings = 0;
            if (IgnoreWhitespace) settings |= NativeIgnoreWhitespace;
            if (IgnoreProcessingInstructions) settings |= NativeIgnoreProcessingInstructions;
            if (IgnoreComments) settings |= NativeIgnoreComments;

            return settings;
        }

        /// <summary>
        /// Gets or sets the XmlNameTable used for atomized string comparisons.
        /// </summary>
        public XmlNameTable NameTable;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore insignificant
        /// white space.
        /// </summary>
        public bool IgnoreWhitespace;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore processing
        /// instructions.
        /// </summary>
        public bool IgnoreProcessingInstructions;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore comments.
        /// </summary>
        public bool IgnoreComments;

        /// <summary>
        /// Initializes a new instance of the XmlReaderSettings class.
        /// </summary>
        public XmlReaderSettings()
        {
            NameTable = null;
            IgnoreWhitespace = false;
            IgnoreProcessingInstructions = false;
            IgnoreComments = false;
        }
    }
}


