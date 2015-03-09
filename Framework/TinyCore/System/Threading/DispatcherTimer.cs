////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT
{
    /// <summary>
    ///     A timer that is integrated into the Dispatcher queues, and will
    ///     be processed after a given amount of time
    /// </summary>
    public class DispatcherTimer : IDisposable
    {
        /// <summary>
        ///     Creates a timer that uses the current thread's Dispatcher to
        ///     process the timer event
        /// </summary>
        public DispatcherTimer()
            : this(Dispatcher.CurrentDispatcher)
        {
        }

        /// <summary>
        ///     Creates a timer that uses the specified Dispatcher to
        ///     process the timer event.
        /// </summary>
        /// <param name="dispatcher">
        ///     The dispatcher to use to process the timer.
        /// </param>
        public DispatcherTimer(Dispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            _dispatcher = dispatcher;
            
            _timer = new Timer(new TimerCallback(this.Callback), null, Timeout.Infinite, Timeout.Infinite);

        }

        /// <summary>
        ///     Gets the dispatcher this timer is associated with.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get
            {
                return _dispatcher;
            }
        }

        /// <summary>
        ///     Gets or sets whether the timer is running.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            set
            {
                lock (_instanceLock)
                {
                    if (!value && _isEnabled)
                    {
                        Stop();
                    }
                    else if (value && !_isEnabled)
                    {
                        Start();
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the time between timer ticks.
        /// </summary>
        public TimeSpan Interval
        {
            get
            {
                return new TimeSpan(_interval * TimeSpan.TicksPerMillisecond);
            }

            set
            {
                bool updateTimer = false;

                long ticks = value.Ticks;

                if (ticks < 0)
                    throw new ArgumentOutOfRangeException("value", "too small");

                if (ticks > Int32.MaxValue * TimeSpan.TicksPerMillisecond)
                    throw new ArgumentOutOfRangeException("value", "too large");

                lock (_instanceLock)
                {
                    _interval = (int)(ticks / TimeSpan.TicksPerMillisecond);

                    if (_isEnabled)
                    {
                        updateTimer = true;
                    }
                }

                if (updateTimer)
                {
                    _timer.Change(_interval, _interval);
                }
            }
        }

        /// <summary>
        ///     Starts the timer.
        /// </summary>
        public void Start()
        {
            lock (_instanceLock)
            {
                if (!_isEnabled)
                {
                    _isEnabled = true;

                    _timer.Change(_interval, _interval);
                }
            }
        }

        /// <summary>
        ///     Stops the timer.
        /// </summary>
        public void Stop()
        {
            lock (_instanceLock)
            {
                if (_isEnabled)
                {
                    _isEnabled = false;
                    
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        ///     Occurs when the specified timer interval has elapsed and the
        ///     timer is enabled.
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        ///     Any data that the caller wants to pass along with the timer.
        /// </summary>
        public object Tag
        {
            get
            {
                return _tag;
            }

            set
            {
                _tag = value;
            }
        }

        private void Callback(object state)
        {
            // BeginInvoke a new operation.
            _dispatcher.BeginInvoke(
                new DispatcherOperationCallback(FireTick),
                null);
        }

        private object FireTick(object unused)
        {
            EventHandler e = Tick;
            if (e != null)
            {
                e(this, EventArgs.Empty);
            }

            return null;
        }

        private object _instanceLock = new Object();
        private Dispatcher _dispatcher;
        private int _interval;
        private object _tag;
        private bool _isEnabled;
        private Timer _timer;

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
            _timer.Dispose();
        }
    }
}


