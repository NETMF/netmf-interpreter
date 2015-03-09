////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System
namespace System
{

    using System;
    /**
     * The <i>IComparable</i> interface is implemented by classes that support an
     * <i>ordering</i> of instances of the class. The ordering represented by
     * <i>IComparable</i> can be used to sort arrays and collections of objects
     * that implement the interface.
     *
     * @see System.Array#Sort
     * @see System.Array#BinarySearch
     * @see System.List#Sort
     * @see System.List#BinarySearch
     * @see System.SortedList
     */
    public interface IComparable
    {
        // Interface does not need to be marked with the serializable attribute
        /**
         * Compares this object to another object, returning an integer that
         * indicates the relationship. An implementation of this method must return
         * a value less than zero if <i>this</i> is less than <i>object</i>, zero
         * if <i>this</i> is equal to <i>object</i>, or a value greater than zero
         * if <i>this</i> is greater than <i>object</i>.
         *
         * @param object The object to compare with this object.
         * @return A value less than zero if <i>this</i> < <i>object</i>, zero if
         * <i>this</i> = <i>object</i>, or a value greater than zero if <i>this</i>
         * > <i>object</i>.
         */
        int CompareTo(Object obj);
    }

}


