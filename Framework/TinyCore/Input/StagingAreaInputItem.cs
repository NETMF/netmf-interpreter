////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     This class encapsulates an input event while it is being
    ///     processed by the input manager.
    /// </summary>
    /// <remarks>
    ///     This class just provides the dictionary-based storage for
    ///     all of the listeners of the various input manager events.
    /// </remarks>
    public class StagingAreaInputItem
    {
        internal StagingAreaInputItem(InputEventArgs input, StagingAreaInputItem promote)
        {
            Input = input;

            if (promote != null && promote._table != null)
            {
                // REFACTOR -- need a hashtable!

                _table = (Hashtable)promote._table.Clone();
            }
        }

        /// <summary>
        ///     Returns the input event.
        /// </summary>
        public readonly InputEventArgs Input;

        /// <summary>
        ///     Provides storage for arbitrary data needed during the
        ///     processing of this input event.
        /// </summary>
        /// <param name="key">
        ///     An arbitrary key for the data.  This cannot be null.
        /// </param>
        /// <returns>
        ///     The data previously set for this key, or null.
        /// </returns>
        public object GetData(object key)
        {
            if (_table == null)
            {
                return null;
            }
            else
            {
                return _table[key];
            }
        }

        /// <summary>
        ///     Provides storage for arbitrary data needed during the
        ///     processing of this input event.
        /// </summary>
        /// <param name="key">
        ///     An arbitrary key for the data.  This cannot be null.
        /// </param>
        /// <param name="value">
        ///     The data to set for this key.  This can be null.
        /// </param>
        public void SetData(object key, object value)
        {
            if (_table == null)
            {
                _table = new Hashtable();
            }

            _table[key] = value;
        }

        Hashtable _table;
    }
}


