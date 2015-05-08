using System;
using System.Runtime.CompilerServices;

namespace Windows.Devices.Gpio
{
    // FUTURE: This should be "EventHandler<GpioPinValueChangedEventArgs>"
    public delegate void GpioPinValueChangedEventHandler(
        Object sender,
        GpioPinValueChangedEventArgs e);

    public sealed class GpioPin : IDisposable
    {
        // Construction and destruction

        internal GpioPin(int pinNumber)
        {
            if (m_lastOutputValue == GpioPinValue.Low) { } // Silence an unused variable warning.

            InitNative(pinNumber);
            s_eventListener.AddPin(pinNumber, this);
        }

        ~GpioPin()
        {
            Dispose(false);
        }

        // Public events

        public event GpioPinValueChangedEventHandler ValueChanged
        {
            add
            {
                lock (m_syncLock)
                {
                    if (m_disposed)
                    {
                        throw new ObjectDisposedException();
                    }

                    var callbacksOld = m_callbacks;
                    var callbacksNew = (GpioPinValueChangedEventHandler)Delegate.Combine(callbacksOld, value);

                    try
                    {
                        m_callbacks = callbacksNew;
                        SetDriveModeInternal(m_driveMode);
                    }
                    catch
                    {
                        m_callbacks = callbacksOld;
                        throw;
                    }
                }
            }

            remove
            {
                lock (m_syncLock)
                {
                    if (m_disposed)
                    {
                        throw new ObjectDisposedException();
                    }

                    var callbacksOld = m_callbacks;
                    var callbacksNew = (GpioPinValueChangedEventHandler)Delegate.Remove(callbacksOld, value);

                    try
                    {
                        m_callbacks = callbacksNew;
                        SetDriveModeInternal(m_driveMode);
                    }
                    catch
                    {
                        m_callbacks = callbacksOld;
                        throw;
                    }
                }
            }
        }

        // Public properties

        extern public TimeSpan DebounceTimeout
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        public int PinNumber
        {
            get
            {
                lock (m_syncLock)
                {
                    if (m_disposed)
                    {
                        throw new ObjectDisposedException();
                    }

                    return m_pinNumber;
                }
            }
        }

        public GpioSharingMode SharingMode
        {
            get
            {
                return GpioSharingMode.Exclusive;
            }
        }

        // Public methods

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public GpioPinValue Read();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Write(GpioPinValue value);

        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            switch (driveMode)
            {
            case GpioPinDriveMode.Input:
            case GpioPinDriveMode.Output:
            case GpioPinDriveMode.InputPullUp:
            case GpioPinDriveMode.InputPullDown:
                return true;
            }

            return false;
        }

        public GpioPinDriveMode GetDriveMode()
        {
            lock (m_syncLock)
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                return m_driveMode;
            }
        }

        public void SetDriveMode(GpioPinDriveMode driveMode)
        {
            lock (m_syncLock)
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                if (driveMode != m_driveMode)
                {
                    SetDriveModeInternal(driveMode);
                    m_driveMode = driveMode;
                }
            }
        }

        public void Dispose()
        {
            lock (m_syncLock)
            {
                if (!m_disposed)
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                    m_disposed = true;
                }
            }
        }

        internal void OnPinChangedInternal(GpioPinEdge edge)
        {
            GpioPinValueChangedEventHandler callbacks = null;

            lock (m_syncLock)
            {
                if (!m_disposed)
                {
                    callbacks = m_callbacks;
                }
            }

            if (callbacks != null)
            {
                callbacks(this, new GpioPinValueChangedEventArgs(edge));
            }
        }

        // Private methods

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void InitNative(int pinNumber);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void DisposeNative();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void SetDriveModeInternal(GpioPinDriveMode driveMode);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeNative();
                s_eventListener.RemovePin(m_pinNumber);
            }
        }

        // Private fields

        private static GpioPinEventListener s_eventListener = new GpioPinEventListener();

        private object m_syncLock = new object();
        private bool m_disposed = false;
        private int m_pinNumber = -1;

        private GpioPinDriveMode m_driveMode = GpioPinDriveMode.Input;
        private GpioPinValue m_lastOutputValue = GpioPinValue.Low;
        private GpioPinValueChangedEventHandler m_callbacks = null;
    }
}
