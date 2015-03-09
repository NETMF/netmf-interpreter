using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT
{
    /// <summary>
    ///     Provides data for the various property changed events.
    /// </summary>
    public class PropertyChangedEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the PropertyChangedEventArgs class.
        /// </summary>
        /// <param name="property">
        ///     The property whose value changed.
        /// </param>
        /// <param name="oldValue">
        ///     The value of the property before the change.
        /// </param>
        /// <param name="newValue">
        ///     The value of the property after the change.
        /// </param>
        public PropertyChangedEventArgs(string property, object oldValue, object newValue)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        ///     The property whose value changed.
        /// </summary>
        public readonly string Property;

        /// <summary>
        ///     The value of the property before the change.
        /// </summary>
        public readonly object OldValue;

        /// <summary>
        ///     The value of the property after the change.
        /// </summary>
        public readonly object NewValue;
    }
}


