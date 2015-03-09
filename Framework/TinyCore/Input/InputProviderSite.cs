////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Input
{

    /// <summary>
    ///     The object which input providers use to report input to the input manager.
    /// </summary>
    public class InputProviderSite : IDisposable
    {
        internal InputProviderSite(InputManager inputManager, object inputProvider)
        {
            if (inputManager == null)
            {
                throw new ArgumentNullException("inputManager");
            }

            _inputManager = inputManager;
            _inputProvider = inputProvider;
        }

        /// <summary>
        ///     Returns the input manager that this site is attached to.
        /// </summary>
        public InputManager InputManager
        {
            get
            {
                return _inputManager;
            }
        }

        /// <summary>
        ///     Unregisters this input provider.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void  Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                if (_inputManager != null && _inputProvider != null)
                {
                    _inputManager.UnregisterInputProvider(_inputProvider);
                }

                _inputManager = null;
                _inputProvider = null;
            }

        }

        /// <summary>
        /// Returns true if we are disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
        }

        /// <summary>
        ///     Reports input to the input manager.
        /// </summary>
        /// <returns>
        ///     Whether or not any event generated as a consequence of this
        ///     event was handled.
        /// </returns>
        // do we really need this?  Make the "providers" call InputManager.ProcessInput themselves.
        // we currently need to map back to providers for other reasons.
        public bool ReportInput(InputDevice device, InputReport inputReport)
        {
            if (_isDisposed)
            {
                throw new InvalidOperationException();
            }

            bool handled = false;

            InputReportEventArgs input = new InputReportEventArgs(device, inputReport);
            input.RoutedEvent = InputManager.PreviewInputReportEvent;

            if (_inputManager != null)
            {
                handled = _inputManager.ProcessInput(input);
            }

            return handled;
        }

        private bool _isDisposed;

        private InputManager _inputManager;

        private object _inputProvider;
    }
}


