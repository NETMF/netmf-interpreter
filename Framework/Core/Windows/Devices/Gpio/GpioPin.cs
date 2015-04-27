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
            m_pinNumber = pinNumber;
            SetDriveModeInternal(GpioPinDriveMode.Input);
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
                        UpdateInterruptHandler();
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
                        UpdateInterruptHandler();
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

        // FUTURE: This should be of type TimeSpan.
        public bool DebounceTimeout
        {
            get
            {
                lock (m_syncLock)
                {
                    if (m_disposed)
                    {
                        throw new ObjectDisposedException();
                    }

                    return m_debounceTimeout;
                }
            }

            set
            {
                lock (m_syncLock)
                {
                    if (m_disposed)
                    {
                        throw new ObjectDisposedException();
                    }

                    if (value != m_debounceTimeout)
                    {
                        m_debounceTimeout = value;
                        m_inputPort.GlitchFilter = value;
                    }
                }
            }
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
            if (!IsDriveModeSupported(driveMode))
            {
                throw new ArgumentException("Drive mode not supported", "driveMode");
            }

            lock (m_syncLock)
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                // Short-circuit if the drive mode is unchanged.
                if (driveMode == m_driveMode)
                {
                    return;
                }

                SetDriveModeInternal(driveMode);
            }
        }

        public void Write(GpioPinValue value)
        {
            lock (m_syncLock)
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                m_lastOutputValue = (value != GpioPinValue.Low);
                if (m_outputPort != null)
                {
                    m_outputPort.Write(m_lastOutputValue);
                }
            }
        }

        public GpioPinValue Read()
        {
            lock (m_syncLock)
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                return m_inputPort.Read() ? GpioPinValue.High : GpioPinValue.Low;
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

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_inputPort != null)
                {
                    m_inputPort.Dispose();
                }

                if (m_outputPort != null)
                {
                    m_outputPort.Dispose();
                }
            }
        }

        private void OnInterrupt(uint port, uint state, DateTime time)
        {
            var callbacks = m_callbacks;
            if (callbacks != null)
            {
                // TODO: Validate that this state is translated correctly.
                GpioPinEdge edge = (state == 1) ? GpioPinEdge.RisingEdge : GpioPinEdge.FallingEdge;
                callbacks(this, new GpioPinValueChangedEventArgs(edge));
            }
        }

        private void UpdateInterruptHandler()
        {
            if (m_inputPort == null)
            {
                m_registeredHandler = false;
            }
            else
            {
                if ((m_callbacks == null) && m_registeredHandler)
                {
                    m_inputPort.OnInterrupt -= OnInterrupt;
                }
                else if ((m_callbacks != null) && !m_registeredHandler)
                {
                    m_inputPort.OnInterrupt += OnInterrupt;
                }
            }
        }

        private void SetDriveModeInternal(GpioPinDriveMode driveMode)
        {
            if (driveMode == GpioPinDriveMode.Output)
            {
                if (m_inputPort != null)
                {
                    m_inputPort.Dispose();
                    m_inputPort = null;

                    UpdateInterruptHandler();
                }

                m_outputPort = new OutputPort((Cpu.Pin)m_pinNumber, m_lastOutputValue);
            }
            else
            {
                if (m_outputPort != null)
                {
                    m_outputPort.Dispose();
                    m_outputPort = null;
                }

                Port.ResistorMode resistorMode = Port.ResistorMode.Disabled;
                if (driveMode == GpioPinDriveMode.InputWithPullUp)
                {
                    m_inputPort.Resistor = Port.ResistorMode.PullUp;
                }
                else if (driveMode == GpioPinDriveMode.InputWithPullDown)
                {
                    m_inputPort.Resistor = Port.ResistorMode.PullDown;
                }

                if (m_inputPort == null)
                {
                    m_inputPort = new InterruptPort(
                        (Cpu.Pin)m_pinNumber,
                        m_debounceTimeout,
                        resistorMode,
                        Port.InterruptMode.InterruptEdgeBoth);
                    UpdateInterruptHandler();
                }
                else
                {
                    m_inputPort.Resistor = resistorMode;
                }
            }

            m_driveMode = driveMode;
        }

        // Private fields

        private object m_syncLock = new object();
        private bool m_disposed = false;
        private int m_pinNumber = -1;

        private GpioPinDriveMode m_driveMode = GpioPinDriveMode.Input;
        private bool m_debounceTimeout = false;
        private bool m_lastOutputValue = false;
        private bool m_registeredHandler = false;

        private InterruptPort m_inputPort = null;
        private OutputPort m_outputPort = null;
        private GpioPinValueChangedEventHandler m_callbacks = null;
    }
}
