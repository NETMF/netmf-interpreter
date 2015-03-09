////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Collections
{
    using System;

    /**
     * Base interface for all collections, defining enumerators, size, and
     * synchronization methods.
     */
    public interface ICollection : IEnumerable
    {
        // Interfaces are not serialable
        /**
         * CopyTo copies a collection into an Array, starting at a particular
         * index into the array.
         *
         * @param array array to copy collection into
         * @param index Index into <var>array</var>.
         * @exception ArgumentNullException if <var>array</var> is null.
         */
        void CopyTo(Array array, int index);

        /**
         * Number of items in the collections.
         */
        int Count
        { get; }

        Object SyncRoot
        { get; }
        bool IsSynchronized
        { get; }
    }
}


