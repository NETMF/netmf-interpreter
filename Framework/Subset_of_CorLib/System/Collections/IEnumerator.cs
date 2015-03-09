////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Collections
{
    using System;

    /**
     * Base interface for all enumerators, providing a simple approach
     * to iterating over a collection.
     */
    public interface IEnumerator
    {
        // Interfaces are not serializable
        /**
         * Advances the enumerator to the next element of the enumeration and
         * returns a boolean indicating whether an element is available. Upon
         * creation, an enumerator is conceptually positioned before the first
         * element of the enumeration, and the first call to <code>MoveNext</code>
         * brings the first element of the enumeration into view.
         *
         * @return true if the enumerator was succesfully advanced to the next
         * element, false if the enumeration has been completed.
         * @exception InvalidOperationException Thrown if the underlying set of objects
         * has been modified since this enumerator was created.
         */
        bool MoveNext();

        /**
         * Returns the current element of the enumeration. The returned value is
         * undefined before the first call to <code>MoveNext</code> and following a
         * call to <code>MoveNext</code> that returned false. Multiple calls to
         * <code>GetCurrent</code> with no intervening calls to <code>MoveNext</code>
         * will return the same object.
         *
         * @return The current element of the enumeration.
         * @exception InvalidOperationException Thrown if the underlying set of objects
         * has been modified since this enumerator was created, or if the Enumerator
         * is positioned before or after the valid range.
         */
        Object Current
        {
            get;
        }

        /**
         * Resets the enumerator to the beginning of the enumeration, starting over.
         * The preferred behavior for Reset is to return the exact same enumeration.
         * This means if you modify the underlying collection then call Reset, your
         * IEnumerator will be invalid, just as it would have been if you had called
         * MoveNext or Current.
         *
         * @exception InvalidOperationException Thrown if the underlying set of
         * objects has been modified since this enumerator was created.
         */
        void Reset();
    }
}


