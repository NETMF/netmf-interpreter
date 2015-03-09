////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//#define USE_PERFORMANCE_COUNTER

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Emulator.Time
{
    internal class TimeDriver : HalDriver<ITimeDriver>, ITimeDriver
    {
        internal class HalCompletion : TimingServices.Completion
        {
            IntPtr _ptr;
            TimeDriver _timeDriver;
            
            public HalCompletion(Emulator emulator, IntPtr ptr, TimeDriver timeDriver)
                : base(emulator)
            {
                _ptr = ptr;
                _timeDriver = timeDriver;
            }

            private void ExecuteCallback(Emulator.ExecuteWithInterruptsDisabledCallback callback)
            {
                this.Emulator.ExecuteWithInterruptsDisabled(
                    delegate
                    {
                        HalCompletion completion;

                        if (_timeDriver.RemoveContinuation(_ptr, out completion))
                        {
                            if (completion == this)
                            {
                                //Only can do the callback if it is still in the continuation queue
                                //This prevents a callback from a continuation that has been aborted
                                //It's possible that the continuation gets aborted and then re-enqueued as well
                                //So we verify that this is the instance that is enqueued.

                                callback();
                            }
                        }
                    }
                );
            }

            protected internal override void OnContinuation()
            {
                ExecuteCallback(
                    delegate
                    {
                        this.Emulator.EmulatorNative.ExecuteContinuation(_ptr);
                    }
                );
            }

            protected internal override void OnCompletion()
            {
                ExecuteCallback(
                    delegate
                    {
                        this.Emulator.EmulatorNative.ExecuteCompletion(_ptr);
                    }
                );
            }
        }

        private Dictionary<IntPtr, HalCompletion> _completionLookup;

        public TimeDriver()            
        {
            _completionLookup = new Dictionary<IntPtr, HalCompletion>();
        }

        private TimingServices TimingServices
        {
            [DebuggerHidden]
            get { return this.Emulator.TimingServices; }
        }

        private HalCompletion CreateContinuation(IntPtr Continuation)
        {
            HalCompletion halCompletion = null;

            lock (_completionLookup)
            {
                halCompletion = new HalCompletion(this.Emulator, Continuation, this);
                _completionLookup[Continuation] = halCompletion;
            }

            return halCompletion;
        }

        private bool RemoveContinuation(IntPtr Continuation, out HalCompletion halCompletion)
        {
            bool fRemoved = false;

            lock (_completionLookup)
            {
                if (_completionLookup.TryGetValue(Continuation, out halCompletion))
                {                    
                    _completionLookup.Remove(Continuation);
                    fRemoved = true;
                }                
            }

            return fRemoved;
        }

        private void RemoveContinuation(IntPtr Continuation)
        {
            HalCompletion halCompletion;

            RemoveContinuation(Continuation, out halCompletion);
        }

        #region ITimeDriver Members

        bool ITimeDriver.Initialize()
        {
            return true;
        }

        bool ITimeDriver.Uninitialize()
        {
            return true;
        }

        ulong ITimeDriver.CurrentTicks()
        {
            return this.TimingServices.CurrentTicks;
        }

        long ITimeDriver.TicksToTime(ulong Ticks)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// Our origin is at 1601/01/01:00:00:00.000
        /// While desktop CLR's origin is at 0001/01/01:00:00:00.000.
        /// There are 504911232000000000 ticks between them which we are subtracting.
        /// See DeviceCode\PAL\time_decl.h for explanation of why we are taking
        /// year 1601 as origin for our HAL, PAL, and CLR.
        static Int64 ticksAtOrigin = 504911232000000000;
        long ITimeDriver.CurrentTime()
        {            
            return DateTime.UtcNow.Ticks - ticksAtOrigin;
        }

        void ITimeDriver.SetCompare(ulong CompareValue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void ITimeDriver.Sleep_MicroSeconds(uint uSec)
        {
            long ticks = ((ITimeDriver)this).MicrosecondsToTicks(uSec);
            // ensure we sleep always at least the required time (+1)
            Thread.Sleep((int)((ticks / ((ITimeDriver)this).TicksPerSecond) * 1000 ) + 1);
        }

        void ITimeDriver.Sleep_MicroSecondsInterruptsEnabled(uint uSec)            
        {
            long ticks = ((ITimeDriver)this).MicrosecondsToTicks(uSec);
            // do it in chunks
            ticks /= 10;
            int count = 10;
            do
            {
                long msec = (ticks * 1000) / ((ITimeDriver)this).TicksPerSecond;
                Thread.Sleep((int)msec);
            } while (--count >= 0);
            // ensure we sleep always at least the required time (minium is 1 msec)
            Thread.Sleep(1);
        }
 
        long ITimeDriver.MicrosecondsToTicks(long usec)
        {
            return usec * 10;
        }

        void ITimeDriver.EnqueueCompletion(IntPtr Completion, uint uSecFromNow)
        {
            lock (_completionLookup)
            {
                if(!_completionLookup.ContainsKey( Completion ))
                {
                    HalCompletion halCompletion = CreateContinuation( Completion );

                    this.TimingServices.EnqueueCompletion( halCompletion, uSecFromNow );
                }
            }
        }

        void ITimeDriver.AbortCompletion(IntPtr Completion)
        {
            lock (this.Emulator.IsrLock)
            {
                HalCompletion halCompletion;

                if (RemoveContinuation(Completion, out halCompletion))
                {
                    this.TimingServices.AbortCompletion(halCompletion);
                }
            }
        }

        void ITimeDriver.EnqueueContinuation(IntPtr Continuation)
        {
            lock (_completionLookup)
            {
                if(!_completionLookup.ContainsKey( Continuation ))
                {
                    HalCompletion halCompletion = CreateContinuation(Continuation);

                    halCompletion.EnqueueContinuation();
                }
                else
                {
                    _completionLookup[Continuation].EnqueueContinuation();
                }
            }
        }

        void ITimeDriver.AbortContinuation(IntPtr Continuation)
        {
            lock (this.Emulator.IsrLock)
            {
                HalCompletion halCompletion;

                if (RemoveContinuation(Continuation, out halCompletion))
                {
                    this.TimingServices.AbortContinuation(halCompletion);
                }
            }        
        }

        bool ITimeDriver.DequeueAndExecuteContinuation()
        {
            return this.TimingServices.DequeueAndExecuteContinuation();            
        }

        bool ITimeDriver.IsExecutionPaused
        {
            set 
            {
                if (value)
                    this.TimingServices.SuspendTime();
                else
                    this.TimingServices.ResumeTime();
            }
        }

        uint ITimeDriver.SystemClock
        {
            get { return this.TimingServices.SystemClockFrequency; }
        }

        uint ITimeDriver.TicksPerSecond
        {
            get
            {
                return 10000000;
            }
        }

        bool ITimeDriver.IsLinked(IntPtr Continuation)
        {
            return _completionLookup.ContainsKey(Continuation);
        }

        #endregion
    }

    public class TimingServices : EmulatorComponent
    {
#if USE_PERFORMANCE_COUNTER
        [DllImport("Kernel32.dll")]
        static extern bool QueryPerformanceCounter(out long value);
        [DllImport("Kernel32.dll")]
        static extern bool QueryPerformanceFrequency(out long value);
        private double _performanceCounterToTicks;
        
#endif

        public delegate void OnExecutionPaused();
        public delegate void OnExecutionResumed();

        private const int c_WaitHandleShutdown = 0;
        private const int c_WaitHandleCompletion = 1;
        private const int c_WaitHandles = 2;

        WaitHandle[] _waitHandles;

        private  long _ticksAdjustment; //how time is manipulated
        private ulong _ticksSuspended;  //when execution is suspended, this holds the performance counter data
        private ulong _ticksCurrent;    //CurrentTime, only valid if time is suspended.

        private List<Completion> _completions;
        private List<Continuation> _continuations;
        private AutoResetEvent _areCompletions;
        private Thread _threadCompletion;
        
        private event OnExecutionPaused _evtPaused;
        private event OnExecutionResumed _evtResumed;

        private uint _clockFrequency;

        public abstract class Continuation
        {
            Emulator _emulator;
            TimingServices _timingServices;

            public Continuation(Emulator emulator)
            {
                _emulator = emulator;
                _timingServices = emulator.TimingServices;
            }
            
            protected TimingServices TimingServices
            {
                [DebuggerHidden]
                get { return _timingServices; }
            }

            protected Emulator Emulator
            {
                [DebuggerHidden]
                get { return _emulator; }
            }

            public void EnqueueContinuation()
            {
                this.TimingServices.EnqueueContinuation(this);
                this.Emulator.ManagedHal.Events.Set( 0 /*this will force the global event to be signalled*/);
            }

            public void AbortContinuation()
            {
                this.TimingServices.AbortContinuation(this);
            }

            protected internal abstract void OnContinuation();
        }

        public abstract class Completion : Continuation
        {
            internal ulong _ticks;

            public Completion(Emulator emulator)
                : base(emulator)
            {
            }

            public void EnqueueCompletion(uint uSecFromNow)
            {
                this.TimingServices.EnqueueCompletion(this, uSecFromNow);
            }

            public void AbortCompletion()
            {                
                this.TimingServices.AbortCompletion(this);
            }

            protected internal override void OnContinuation()
            {
                throw new NotImplementedException();
            }

            protected internal virtual void OnCompletion()
            {
                this.Emulator.ManagedHal.Events.Set( (uint)Microsoft.SPOT.Emulator.Events.SystemEvents.SYSTEM_TIMER );
                
                EnqueueContinuation();
            }
        }

        public class Timer : Completion, IDisposable
        {
            private readonly TimerCallback _callback;
            private readonly object _state;
            private ulong _ticksperiod;
            private const ulong _timeoutInfinite = unchecked((ulong)Timeout.Infinite);

            public Timer(Emulator emulator, TimerCallback callback, object state, int dueTime, int period)
                : base(emulator)
            {
                _callback = callback;
                _state = state;
                Change(dueTime, period);
            }

            public Timer(Emulator emulator, TimerCallback callback, object state, long dueTime, long period)
                : base(emulator)
            {
                _callback = callback;
                _state = state;
                Change(dueTime, period);
            }

            public Timer(Emulator emulator, TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
                : base(emulator)
            {
                _callback = callback;
                _state = state;
                Change(dueTime, period);
            }

            public Timer(Emulator emulator, TimerCallback callback, object state, uint dueTime, uint period)
                : base(emulator)
            {
                _callback = callback;
                _state = state;
                Change(dueTime, period);
            }

            public bool Change(int dueTime, int period)
            {
                return Change((ulong)dueTime, (ulong)period, false);
            }

            public bool Change(long dueTime, long period)
            {
                return Change((ulong)dueTime, (ulong)period, false);
            }

            public bool Change(uint dueTime, uint period)
            {
                return Change((ulong)dueTime, (ulong)period, false);
            }

            public bool Change(TimeSpan dueTime, TimeSpan period)
            {
                return Change((ulong)dueTime.Ticks, (ulong)period.Ticks, true);
            }

            private bool Change(ulong dueTime, ulong period, bool inTicks)
            {
                if (_callback == null)
                    throw new Exception("_callback cannot be null");
                
                ulong ticksPeriod, ticksDueTime;

                if (inTicks)
                {
                    ticksPeriod = period;
                    ticksDueTime = dueTime;
                }
                else
                {
                    ticksPeriod = _timeoutInfinite;
                    ticksDueTime = _timeoutInfinite;

                    if (period != _timeoutInfinite)
                    {
                        ticksPeriod = period * TimeSpan.TicksPerMillisecond;
                    }

                    if (dueTime != _timeoutInfinite)
                    {
                        ticksDueTime = dueTime * TimeSpan.TicksPerMillisecond;
                    }
                }

                _ticksperiod = ticksPeriod;

                this.Enqueue(ticksDueTime);

                return true;
            }

            protected internal override void OnContinuation()
            {
                try
                {
                    _callback(_state);
                }
                catch
                {
                }

                ulong ticksOld = _ticks;

                ulong ticksCurrent = TimingServices.CurrentTicks;
                Debug.Assert(ticksCurrent >= _ticks);
                ulong ticksFromNow = _ticksperiod;

                if (ticksCurrent - _ticks <= _ticksperiod)
                {
                    //Let's take into account delays in firing of the timer, and try to 
                    //keep the next firing on schedule.  If we miss by too much (more than one period), we'll just
                    //schedule _ticksPeriod from now, and try again next time.
                    ticksFromNow -= ticksCurrent - _ticks;
                }
                
                this.Enqueue(ticksFromNow);
            }

            private void Enqueue(ulong ticksFromNow)
            {
                if (ticksFromNow == _timeoutInfinite)
                {
                    this.AbortCompletion();                    
                }
                else
                {
                    this.EnqueueCompletion((uint)ticksFromNow / 10);
                }
            }

            #region IDisposable Members

            void IDisposable.Dispose()
            {
                this.AbortCompletion();          

                Change(Timeout.Infinite, Timeout.Infinite);                      
            }

            #endregion
        }

        public TimingServices()
        {
            _clockFrequency = 27000000;
        }

        public override void SetupComponent()
        {
#if USE_PERFORMANCE_COUNTER
            long performanceFrequency;
            
            QueryPerformanceFrequency(out performanceFrequency);

            _performanceCounterToTicks = (double)TimeSpan.TicksPerSecond / (double)performanceFrequency;
#endif
            _completions = new List<Completion>();
            _continuations = new List<Continuation>();
            _areCompletions = new AutoResetEvent( false );
            _threadCompletion = new Thread( new ThreadStart( CompletionThreadProc ) );

            _waitHandles = new WaitHandle[c_WaitHandles];
            _waitHandles[c_WaitHandleShutdown] = this.Emulator.ShutdownHandle;
            _waitHandles[c_WaitHandleCompletion] = _areCompletions;
        }

        public override void InitializeComponent()
        {
            SuspendTime();

            _threadCompletion.Start();
        }

        public override void UninitializeComponent()
        {
            base.UninitializeComponent();

            _areCompletions.Set();
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            return (ec is TimingServices);
        }

        private bool IsTimeSuspended
        {
            get { return _ticksSuspended != 0; }
        }

        public event OnExecutionPaused ExecutionPaused
        {
            add { _evtPaused += value;}
            remove { _evtPaused -= value; }
        }

        public event OnExecutionResumed ExecutionResumed
        {
            add { _evtResumed += value; }
            remove { _evtResumed -= value; }
        }

        public uint SystemClockFrequency
        {
            get { return _clockFrequency; }
            set
            {
                ThrowIfNotConfigurable();
                _clockFrequency = value;
            }
        }

        private ulong CurrentTicksInternal
        {
            get
            {
                ulong ret;

#if USE_PERFORMANCE_COUNTER
                long val;

                QueryPerformanceCounter(out val);

                ret = (ulong)(val * _performanceCounterToTicks);
#else
                ret = this.Emulator.EmulatorNative.GetCurrentTicks();
#endif
                return ret;
            }
        }

        public ulong CurrentTicks
        {
            get
            {
                ulong ret = _ticksCurrent;

                if (!this.IsTimeSuspended)
                {
                    ulong val = this.CurrentTicksInternal;

                    ret = (ulong)((long)val - _ticksAdjustment);
                }

                return ret;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void AdjustTime(long adjustTicks)
        {
            _ticksAdjustment += adjustTicks;
            _areCompletions.Set();
            this.Emulator.ManagedHal.Events.Set(0 /*this will force the global event to be signalled*/);
        }

        internal void SuspendTime()
        {
            if (!this.IsTimeSuspended)
            {
                if (_evtPaused != null)
                    _evtPaused();

                //set _ticksCurrent first
                Debug.Assert(!this.IsTimeSuspended);
                _ticksCurrent = this.CurrentTicks;
                //Then suspend time
                _ticksSuspended = this.CurrentTicksInternal;
                Debug.Assert(this.IsTimeSuspended);
            }
        }

        internal void ResumeTime()
        {
            if (this.IsTimeSuspended)
            {
                if (_evtResumed != null)
                    _evtResumed();

                ulong ticks = this.CurrentTicksInternal - _ticksSuspended;

                AdjustTime((long)ticks);

                _ticksSuspended = 0;
                _areCompletions.Set();
            }
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void EnqueueCompletion(Completion completion, uint uSecFromNow)
        {
            int i;
            lock(this)
            {
                AbortCompletion(completion);

                completion._ticks = this.CurrentTicks + uSecFromNow * 10;

                bool fInserted = false;

                for (i = 0; i < _completions.Count; i++)
                {
                    Completion c = _completions[i];

                    if (completion._ticks < c._ticks)
                    {
                        _completions.Insert(i, completion);
                        fInserted = true;
                        break;
                    }
                }

                if (!fInserted)
                {
                    _completions.Add(completion);
                }

                if(i == 0)
                {
                    _areCompletions.Set();
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void AbortCompletion(Completion completion)
        {
            //remove it from the continuation queue
            AbortContinuation(completion);

            lock(this)
            {
                _completions.Remove(completion);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void EnqueueContinuation(Continuation continuation)
        {
            AbortContinuation(continuation);

            lock(this)
            {
                _continuations.Add(continuation);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void AbortContinuation(Continuation continuation)
        {
            lock(this)
            {
                _continuations.Remove(continuation);
            }
        }

        private void CompletionThreadProc()
        {
            Utility.SetCurrentThreadAffinity();

            while (true)
            {
                long wait;
                                    
                Completion completion = null;

                lock (this)
                {                    
                    if (_completions.Count > 0)
                    {
                        completion = _completions[0];

                        wait = (long)(completion._ticks - this.CurrentTicks);
                        
                        if (wait <= 0)
                        {
                            _completions.RemoveAt(0);
                        }
                        else
                        {
                            completion = null;
                            
                            //turn wait from ticks to msec
                            wait = wait / TimeSpan.TicksPerMillisecond;

                            if (this.IsTimeSuspended)
                            {
                                //No need to wake up until time resumes.
                                wait = Timeout.Infinite;
                            }
                        }
                    }
                    else
                    {
                        wait = Timeout.Infinite;
                    }
                }

                if (completion != null)
                {
                    //Is this ok outside the lock?                      
                    //Emulator completions can be called back

                    completion.OnCompletion();
                }
                else
                {
                    if (WaitHandle.WaitAny(_waitHandles, (int)wait, false) == c_WaitHandleShutdown)
                    {
                        break;
                    }
                }
            }
        }

        internal bool DequeueAndExecuteContinuation()
        {
            Continuation continuation = null;
            bool fRet = false;

            lock (this)
            {
                if (_continuations.Count > 0)
                {
                    continuation = _continuations[0];
                    _continuations.RemoveAt(0);
                }
            }

            if (continuation != null)
            {
                continuation.OnContinuation();
                fRet = true;
            }

            return fRet;
        }
    }
}
