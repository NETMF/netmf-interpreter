////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

namespace Microsoft.SPOT.Emulator
{
    public abstract class EmulatorComponent
    {
        private Emulator _emulator;
        private String _componentId;
        private List<EmulatorComponent> _linkedComponents;
        private EmulatorComponent _linkedBy;
        private bool _createdByConfigurationFile;

        public EmulatorComponent()
        {
            _componentId = GetType().ToString() + " (" + Guid.NewGuid().ToString() + ")";
            _linkedComponents = new List<EmulatorComponent>();
            _linkedBy = null;
            _createdByConfigurationFile = false;
        }

        [Conditional("DEBUG")]
        internal void VerifyAccess()
        {
            Debug.Assert(!this.InvokeRequired, "This api can not be called from non-clr thread");
        }

        public virtual void Configure(XmlReader reader)
        {
            _createdByConfigurationFile = true;
            _emulator.ConfigurationEngine.ConfigureEmulatorComponent(this, reader);
        }

        public virtual void SetupComponent()
        {
        }

        public virtual void InitializeComponent()
        {
        }

        public virtual void UninitializeComponent()
        {
        }

        public virtual bool IsReplaceableBy( EmulatorComponent ec )
        {
            // by default, if they have the same component base type (for example, if they're both SerialDriver) then they're replaceable.
            return false;
        }

        public String ComponentId
        {
            [DebuggerHidden]
            get
            {
                return _componentId;
            }
            set
            {
                ThrowIfNotConfigurable();
                _componentId = value;
            }
        }

        public Emulator Emulator
        {
            [DebuggerHidden]
            get { return _emulator; }
        }

        public void SetEmulator( Emulator emulator )
        {
            _emulator = emulator;
        }

        internal EmulatorComponent Clone()
        {
            return (EmulatorComponent)(this.MemberwiseClone());
        }

        internal List<EmulatorComponent> LinkedComponents
        {
            get { return _linkedComponents; }
        }

        internal EmulatorComponent LinkedBy
        {
            get { return _linkedBy; }
            set
            {
                ThrowIfNotSetup();
                _linkedBy = value;
            }
        }

        internal bool CreatedByConfigurationFile
        {
            get { return _createdByConfigurationFile; }
        }

        protected void ThrowIfNotConfigurable()
        {
            if (_emulator != null)
            {
                if (_emulator.State > Emulator.EmulatorState.Configuration)
                {
                    throw new Exception( "This operation is not allowed while the emulator is in the " + _emulator.State + " state." );
                }
            }
        }

        protected void ThrowIfNotSetup()
        {
            if (_emulator != null)
            {
                if (_emulator.State > Emulator.EmulatorState.Setup)
                {
                    throw new Exception( "This operation is not allowed while the emulator is in the " + _emulator.State + " state." );
                }
            }
        }

        public bool InvokeRequired
        {
            get 
            {
                Emulator emulator = this.Emulator;

                return emulator != null && this.Emulator.IsInvokeRequired;
            }
        }

        internal class AsyncContinuation : Time.TimingServices.Continuation, IAsyncResult
        {
            ManualResetEvent _evtCompleted;
            object _result;
            bool _isCompleted;
            Delegate _method;
            object[] _args;
            Exception _exception;
            bool _completedSynchrously;

            internal AsyncContinuation(Emulator emulator, Delegate method, params object[] args)
                : base(emulator)
            {
                _evtCompleted = new ManualResetEvent(false);
                _method = method;
                _args = args;                
            }

            internal object Result
            {
                get { return _result; }
            }

            internal Exception Exception
            {
                get { return _exception; }
            }

            #region IAsyncResult

            bool IAsyncResult.CompletedSynchronously
            {
                get { return _completedSynchrously; }
            }

            object IAsyncResult.AsyncState
            {
                get { return null; }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { return _evtCompleted; }
            }

            bool IAsyncResult.IsCompleted 
            {
                get { return _isCompleted; }
            }

            #endregion

            protected internal override void OnContinuation()
            {
                try
                {
                    _result = _method.DynamicInvoke(_args);
                }
                catch (Exception e)
                {
                    _exception = e;
                }

                _isCompleted = true;
                _evtCompleted.Set();
            }

            internal void ExecuteSynchonously()
            {
                _completedSynchrously = true;
                OnContinuation();                
            }
        }

        internal delegate void InvokeCallback();

        internal void BeginInvoke(InvokeCallback callback)
        {
            BeginInvoke((Delegate)callback);
        }
       
        public object Invoke(Delegate method)
        {
            return Invoke(method, new object[0]);
        }

        public object Invoke(Delegate method, params object[] args)
        {
            IAsyncResult asyncResult = BeginInvoke(method, args);

            return EndInvoke(asyncResult);
        }

        public IAsyncResult BeginInvoke(Delegate method)
        {
            return BeginInvoke(method, new object[0]);
        }

        public IAsyncResult BeginInvoke(Delegate method, params object[] args)
        {
            AsyncContinuation asyncContinuation = new AsyncContinuation(this.Emulator, method, args);

            if (this.InvokeRequired)
            {
                asyncContinuation.EnqueueContinuation();
            }
            else
            {
                asyncContinuation.ExecuteSynchonously();
            }

            return asyncContinuation;
        }

        public object EndInvoke(IAsyncResult asyncResult)
        {
            asyncResult.AsyncWaitHandle.WaitOne();

            AsyncContinuation asyncContinuation = asyncResult as AsyncContinuation;
            Exception exception = asyncContinuation.Exception;

            if (exception != null)
            {
                throw exception;
            }

            return asyncContinuation.Result;
        }
    }
}