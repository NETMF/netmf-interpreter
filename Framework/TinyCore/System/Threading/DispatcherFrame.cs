////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT
{
    /// <summary>
    ///     Representation of Dispatcher frame.
    /// </summary>
    public class DispatcherFrame
    {
        /// <summary>
        ///     Constructs a new instance of the DispatcherFrame class.
        /// </summary>
        public DispatcherFrame()
            : this(true)
        {
        }

        /// <summary>
        ///     Constructs a new instance of the DispatcherFrame class.
        /// </summary>
        /// <param name="exitWhenRequested">
        ///     Indicates whether or not this frame will exit when all frames
        ///     are requested to exit.
        ///     <p/>
        ///     Dispatcher frames typically break down into two categories:
        ///     1) Long running, general purpose frames, that exit only when
        ///        told to.  These frames should exit when requested.
        ///     2) Short running, very specific frames that exit themselves
        ///        when an important criteria is met.  These frames may
        ///        consider not exiting when requested in favor of waiting
        ///        for their important criteria to be met.  These frames
        ///        should have a timeout associated with them.
        /// </param>
        public DispatcherFrame(bool exitWhenRequested)
        {
            _exitWhenRequested = exitWhenRequested;
            _continue = true;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        ///     Indicates that this dispatcher frame should exit.
        /// </summary>
        public bool Continue
        {
            get
            {
                // First check if this frame wants to continue.
                bool shouldContinue = _continue;
                if (shouldContinue)
                {
                    // This frame wants to continue, so next check if it will
                    // respect the "exit requests" from the dispatcher.
                    // and if the dispatcher wants to exit.
                    if (_exitWhenRequested && _dispatcher._hasShutdownStarted)
                    {
                        shouldContinue = false;
                    }
                }

                return shouldContinue;
            }

            set
            {
                _continue = value;

                _dispatcher.QueryContinueFrame();
            }
        }

        private bool _exitWhenRequested;
        private bool _continue;
        private Dispatcher _dispatcher;
    }
}


