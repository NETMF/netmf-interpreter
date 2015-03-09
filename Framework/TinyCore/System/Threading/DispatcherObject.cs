////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT
{
    /// <summary>
    ///     A DispatcherObject is an object associated with a
    ///     <see cref="Dispatcher"/>.  A DispatcherObject instance should
    ///     only be access by the dispatcher's thread.
    /// </summary>
    /// <remarks>
    ///     Subclasses of <see cref="DispatcherObject"/> should enforce thread
    ///     safety by calling <see cref="VerifyAccess"/> on all their public
    ///     methods to ensure the calling thread is the appropriate thread.
    ///     <para/>
    ///     DispatcherObject cannot be independently instantiated; that is,
    ///     all constructors are protected.
    /// </remarks>
    public abstract class DispatcherObject
    {

        /// <summary>
        ///     Checks that the calling thread has access to this object.
        /// </summary>
        /// <remarks>
        ///     Only the dispatcher thread may access DispatcherObjects.
        ///     <p/>
        ///     This method is public so that any thread can probe to
        ///     see if it has access to the DispatcherObject.
        /// </remarks>
        /// <returns>
        ///     True if the calling thread has access to this object.
        /// </returns>
        public bool CheckAccess()
        {
            bool accessAllowed = true;

            // Note: a DispatcherObject that is not associated with a
            // dispatcher is considered to be free-threaded.
            if (Dispatcher != null)
            {
                accessAllowed = Dispatcher.CheckAccess();
            }

            return accessAllowed;
        }

        /// <summary>
        ///     Verifies that the calling thread has access to this object.
        /// </summary>
        /// <remarks>
        ///     Only the dispatcher thread may access DispatcherObjects.
        ///     <p/>
        ///     This method is public so that derived classes can probe to
        ///     see if the calling thread has access to itself.
        ///
        ///     This is only verified in debug builds.
        /// </remarks>
        public void VerifyAccess()
        {
            // Note: a DispatcherObject that is not associated with a
            // dispatcher is considered to be free-threaded.
            if (Dispatcher != null)
            {
                Dispatcher.VerifyAccess();
            }
        }

        /// <summary>
        ///     Instantiate this object associated with the current Dispatcher.
        /// </summary>
        protected DispatcherObject()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        ///     Instantiate this object associated with the current Dispatcher.
        /// </summary>
        /// <param name="canBeUnbound">
        ///     Whether or not the object can be detached from any Dispatcher.
        /// </param>
        internal DispatcherObject(bool canBeUnbound)
        {
            if (canBeUnbound)
            {
                // DispatcherObjects that can be unbound do not force
                // the creation of a dispatcher.
                Dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            }
            else
            {
                Dispatcher = Dispatcher.CurrentDispatcher;
            }
        }

        /// <summary>
        ///     The <see cref="Dispatcher"/> that this
        ///     <see cref="DispatcherObject"/> is associated with.
        /// </summary>
        public readonly Dispatcher Dispatcher;
    }
}


