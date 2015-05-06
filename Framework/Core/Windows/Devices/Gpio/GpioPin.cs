using System;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Hardware;

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
            // TODO: Remove this. Without it, the compiler complains that this value is unused.
            if (m_lastOutputValue == GpioPinValue.Low)
            {
            }

            Init(pinNumber);
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
            case GpioPinDriveMode.InputWithPullUp:
            case GpioPinDriveMode.InputWithPullDown:
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

        // Private methods

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void Init(int pinNumber);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void Dispose(bool disposing);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void SetDriveModeInternal(GpioPinDriveMode driveMode);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void SetInterruptsEnabled(bool enabled);

        private void OnInterrupt(uint port, uint state, DateTime time)
        {
            var callbacks = m_callbacks;
            if (callbacks != null)
            {
                GpioPinEdge edge = (state == 0) ? GpioPinEdge.FallingEdge : GpioPinEdge.RisingEdge;
                callbacks(this, new GpioPinValueChangedEventArgs(edge));
            }
        }

        // Private fields

        private object m_syncLock = new object();
        private bool m_disposed = false;
        private int m_pinNumber = -1;

        private GpioPinDriveMode m_driveMode = GpioPinDriveMode.Input;
        private GpioPinValue m_lastOutputValue = GpioPinValue.Low;

        private object m_eventDispatcher;
        private GpioPinValueChangedEventHandler m_callbacks = null;
    }
}
