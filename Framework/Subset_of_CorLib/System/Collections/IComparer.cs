////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Collections
{

    using System;
    /**
     * The IComparer interface implements a method that compares two objects. It is
     * used in conjunction with the <i>Sort</i> and <i>BinarySearch</i> methods on
     * the <i>Array</i> and <i>List</i> classes.
     *
     * @see System.Array
     * @see System.Collections.List
     *
     * @author Anders Hejlsberg
     * @version 1.00 8/13/98
     */
    // Interfaces are not serializable
    public interface IComparer
    {
        /**
         * Compares two objects. An implementation of this method must return a
         * value less than zero if x is less than y, zero if x is equal to y, or a
         * value greater than zero if x is greater than y.
         *
         * @param x The first object to compare.
         * @param y The second object to compare.
         * @return A value less than zero if x < y, zero if x = y, or a value
         * greater than zero if x > y.
         */
        int Compare(Object x, Object y);
    }
}


