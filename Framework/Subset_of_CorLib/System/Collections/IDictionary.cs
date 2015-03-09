////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace System.Collections
{
    public interface IDictionary : ICollection
    {
        bool IsReadOnly { get; }
        bool IsFixedSize { get; }
        ICollection Keys { get; }
        ICollection Values { get; }

        /// <summary>
        /// The Item property provides methods to read and edit entries in the Dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[object key]{ get; set; }

        /// <summary>
        /// Adds a key/value pair to the dictionary.  The exact position in the dictionary is
        /// implementation-dependent. A hashtable will always hash the same key to the same position.
        /// This position may be different if a different hash function is used.
        /// </summary>
        /// <param name="key"></param>
        void Add(object key, object value);

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns whether the dictionary contains a particular item.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if found otherwise false</returns>
        bool Contains(object key);

        /// <summary>
        /// Removes an item from the dictionary.
        /// </summary>
        /// <param name="key"></param>
        void Remove(object key);
    }
}
