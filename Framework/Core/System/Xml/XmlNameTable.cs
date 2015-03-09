////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.CompilerServices;

namespace System.Xml
{
    // Internal XmlNameTable structure to keep track of individual entries
    internal class XmlNameTable_Entry
    {
        public String Str;
        public int HashCode;
        public XmlNameTable_Entry Next;
    }

    /// <summary>
    /// Table of atomized string objects. This provides an efficient means for the XML
    /// parser to use the same string object for all repeated element and attribute
    /// names in an XML document.
    /// </summary>
    public class XmlNameTable
    {
        private XmlNameTable_Entry[] _entries;
        private int _count;
        private int _mask;
        private int _hashCodeRandomizer;

        // temp handle used in native code to prevent otherwise unrooted object from being GC'ed
        private object _tmp;

        /// <summary>
        /// Initializes a new instance of the XmlNameTable class.
        /// </summary>
        public XmlNameTable()
        {
            _mask = 31;
            _entries = new XmlNameTable_Entry[_mask + 1];

            _hashCodeRandomizer = new Random().Next();
        }

        /// <summary>
        /// Gets the atomized string with the specified value.
        /// </summary>
        /// <param name="value">The name to find.</param>
        /// <returns>The atomized string object or null if the string has not already been atomized.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual String Get(String value);

        /// <summary>
        /// Atomizes the specified string and adds it to the XmlNameTable.
        /// </summary>
        /// <param name="key">The string to add.</param>
        /// <returns>The atomized string or the existing string if it already exists in the NameTable.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual String Add(String key);
    }
}


