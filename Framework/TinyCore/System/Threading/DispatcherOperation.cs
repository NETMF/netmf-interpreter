////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT
{
    /// <summary>
    ///     DispatcherOperation represents a delegate that has been
    ///     posted to the Dispatcher queue.
    /// </summary>
    public sealed class DispatcherOperation
    {

        internal DispatcherOperation(
            Dispatcher dispatcher,
            DispatcherOperationCallback method,
            object args)
        {
            _dispatcher = dispatcher;
            _method = method;
            _args = args;
        }

        /// <summary>
        ///     Returns the Dispatcher that this operation was posted to.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get
            {
                return _dispatcher;
            }
        }

        /// <summary>
        ///     The status of this operation.
        /// </summary>
        public DispatcherOperationStatus Status
        {
            get
            {
                return _status;
            }

            internal set
            {
                _status = value;
            }
        }

        /// <summary>
        ///     Waits for this operation to complete.
        /// </summary>
        /// <returns>
        ///     The status of the operation.  To obtain the return value
        ///     of the invoked delegate, use the the Result property.
        /// </returns>
        public DispatcherOperationStatus Wait()
        {
            return Wait(new TimeSpan(-TimeSpan.TicksPerMillisecond)); /// Negative one (-1) milliseconds to prevent the timer from starting. See documentation.
        }

        /// <summary>
        ///     Waits for this operation to complete.
        /// </summary>
        /// <param name="timeout">
        ///     The maximum amount of time to wait.
        /// </param>
        /// <returns>
        ///     The status of the operation.  To obtain the return value
        ///     of the invoked delegate, use the the Result property.
        /// </returns>
        public DispatcherOperationStatus Wait(TimeSpan timeout)
        {
            if ((_status == DispatcherOperationStatus.Pending || _status == DispatcherOperationStatus.Executing) &&
                timeout.Ticks != 0)
            {
                if (_dispatcher.Thread == Thread.CurrentThread)
                {
                    if (_status == DispatcherOperationStatus.Executing)
                    {
                        // We are the dispatching thread, and the current operation state is
                        // executing, which means that the operation is in the middle of
                        // executing (on this thread) and is trying to wait for the execution
                        // to complete.  Unfortunately, the thread will now deadlock, so
                        // we throw an exception instead.
                        throw new InvalidOperationException();
                    }

                    // We are the dispatching thread for this operation, so
                    // we can't block.  We will push a frame instead.
                    DispatcherOperationFrame frame = new DispatcherOperationFrame(this, timeout);
                    Dispatcher.PushFrame(frame);
                }
                else
                {
                    // We are some external thread, so we can just block.  Of
                    // course this means that the Dispatcher (queue)for this
                    // thread (if any) is now blocked.
                    // Because we have a single dispatcher per app domain, this thread
                    // must be from another app domain.  We will enforce semantics on
                    // dispatching between app domains so we don't lock up the system.

                    DispatcherOperationEvent wait = new DispatcherOperationEvent(this, timeout);
                    wait.WaitOne();
                }
            }

            return _status;
        }

        /// <summary>
        ///     Aborts this operation.
        /// </summary>
        /// <returns>
        ///     False if the operation could not be aborted (because the
        ///     operation was already in  progress)
        /// </returns>
        public bool Abort()
        {
            bool removed = _dispatcher.Abort(this);

            if (removed)
            {
                _status = DispatcherOperationStatus.Aborted;

                // Raise the Aborted so anyone who is waiting will wake up.
                EventHandler e = Aborted;
                if (e != null)
                {
                    e(this, EventArgs.Empty);
                }
            }

            return removed;
        }

        /// <summary>
        ///     Returns the result of the operation if it has completed.
        /// </summary>
        public object Result
        {
            get
            {
                return _result;
            }
        }

        /// <summary>
        ///     An event that is raised when the operation is aborted.
        /// </summary>
        public event EventHandler Aborted;

        /// <summary>
        ///     An event that is raised when the operation completes.
        /// </summary>
        public event EventHandler Completed;

        internal void OnCompleted()
        {
            EventHandler e = Completed;
            if (e != null)
            {
                e(this, EventArgs.Empty);
            }
        }

        private class DispatcherOperationFrame : DispatcherFrame ,IDisposable 
        {
            // Note: we pass "exitWhenRequested=false" to the base
            // DispatcherFrame construsctor because we do not want to exit
            // this frame if the dispatcher is shutting down. This is
            // because we may need to invoke operations during the shutdown process.
            public DispatcherOperationFrame(DispatcherOperation op, TimeSpan timeout)
                : base(false)
            {
                _operation = op;

                // We will exit this frame once the operation is completed or aborted.
                _operation.Aborted += new EventHandler(OnCompletedOrAborted);
                _operation.Completed += new EventHandler(OnCompletedOrAborted);

                // We will exit the frame if the operation is not completed within
                // the requested timeout.
                if (timeout.Ticks > 0)
                {
                    _waitTimer = new Timer(new TimerCallback(OnTimeout),
                                           null,
                                           timeout,
                                           new TimeSpan(-TimeSpan.TicksPerMillisecond)); /// Negative one (-1) milliseconds to disable periodic signaling.
                }

                // Some other thread could have aborted the operation while we were
                // setting up the handlers.  We check the state again and mark the
                // frame as "should not continue" if this happened.
                if (_operation._status != DispatcherOperationStatus.Pending)
                {
                    Exit();
                }
            }

            private void OnCompletedOrAborted(object sender, EventArgs e)
            {
                Exit();
            }

            private void OnTimeout(object arg)
            {
                Exit();
            }

            private void Exit()
            {
                Continue = false;

                if (_waitTimer != null)
                {
                    _waitTimer.Dispose();
                }

                _operation.Aborted -= new EventHandler(OnCompletedOrAborted);
                _operation.Completed -= new EventHandler(OnCompletedOrAborted);
            }

            private DispatcherOperation _operation;
            private Timer _waitTimer;

            public virtual void Close()
            {
                Dispose();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                _waitTimer.Dispose();
            }

        }

        private class DispatcherOperationEvent : IDisposable
        {
            public DispatcherOperationEvent(DispatcherOperation op, TimeSpan timeout)
            {
                _operation = op;
                _timeout = timeout;
                _event = new AutoResetEvent(false);

                // We will set our event once the operation is completed or aborted.
                _operation.Aborted += new EventHandler(OnCompletedOrAborted);
                _operation.Completed += new EventHandler(OnCompletedOrAborted);

                // Since some other thread is dispatching this operation, it could
                // have been dispatched while we were setting up the handlers.
                // We check the state again and set the event ourselves if this
                // happened.
                if (_operation._status != DispatcherOperationStatus.Pending && _operation._status != DispatcherOperationStatus.Executing)
                {
                    _event.Set();
                }
            }

            private void OnCompletedOrAborted(object sender, EventArgs e)
            {
                _event.Set();
            }

            public void WaitOne()
            {
                _waitTimer = new Timer(new TimerCallback(OnTimeout), null, _timeout, new TimeSpan(-TimeSpan.TicksPerMillisecond));
                _event.WaitOne();
                _waitTimer.Dispose();

                // Cleanup the events.
                _operation.Aborted -= new EventHandler(OnCompletedOrAborted);
                _operation.Completed -= new EventHandler(OnCompletedOrAborted);
            }

            private void OnTimeout(object arg)
            {
                _event.Set();
            }

            private DispatcherOperation _operation;
            private TimeSpan _timeout;
            private AutoResetEvent _event;
            private Timer _waitTimer;

            public virtual void Close()
            {
                Dispose();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                _waitTimer.Dispose();
            }
        }

        private Dispatcher _dispatcher;
        internal DispatcherOperationCallback _method;
        internal object _args;
        internal object _result;
        internal DispatcherOperationStatus _status;     
    }
}


