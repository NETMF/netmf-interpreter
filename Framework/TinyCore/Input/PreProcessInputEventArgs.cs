
using System;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     Allows the handler to cancel the processing of an input event.
    /// </summary>
    /// <remarks>
    ///     An instance of this class is passed to the handlers of the
    ///     following events:
    ///     <list>
    ///         <item>
    ///             <see cref="InputManager.PreProcessInput"/>
    ///         </item>
    ///     </list>
    /// </remarks>
    public sealed class PreProcessInputEventArgs : ProcessInputEventArgs
    {
        // Only we can make these.  Note that we cache and reuse instances.
        internal PreProcessInputEventArgs(StagingAreaInputItem input)
            : base(input)
        {
            _canceled = false;
        }

        /// <summary>
        ///     Cancels the processing of the input event.
        /// </summary>
        public void Cancel()
        {
            _canceled = true;
        }

        /// <summary>
        ///     Whether or not the input event processing was canceled.
        /// </summary>
        public bool Canceled { get { return _canceled; } }

        internal bool _canceled;
    }
}


