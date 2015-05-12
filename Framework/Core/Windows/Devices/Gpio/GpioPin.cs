using System;
using System.Runtime.CompilerServices;

namespace Windows.Devices.Gpio
{
    // FUTURE: This should be "EventHandler<GpioPinValueChangedEventArgs>"
    public delegate void GpioPinValueChangedEventHandler(
        Object sender,
        GpioPinValueChangedEventArgs e);

    /// <summary>
    /// Represents a general-purpose I/O (GPIO) pin.
    /// </summary>
    public sealed class GpioPin : IDisposable
    {
        private static GpioPinEventListener s_eventListener = new GpioPinEventListener();

        private object m_syncLock = new object();
        private bool m_disposed = false;
        private int m_pinNumber = -1;

        private GpioPinDriveMode m_driveMode = GpioPinDriveMode.Input;
        private GpioPinValue m_lastOutputValue = GpioPinValue.Low;
        private GpioPinValueChangedEventHandler m_callbacks = null;

        internal GpioPin()
        {
            if (m_lastOutputValue == GpioPinValue.Low) { } // Silence an unused variable warning.
        }

        ~GpioPin()
        {
            Dispose(false);
        }

        /// <summary>
        /// Occurs when the value of the general-purpose I/O (GPIO) pin changes, either because of an external stimulus
        /// when the pin is configured as an input, or when a value is written to the pin when the pin is configured as
        /// an output.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the debounce timeout for the general-purpose I/O (GPIO) pin, which is an interval during which
        /// changes to the value of the pin are filtered out and do not generate <c>ValueChanged</c> events.
        /// </summary>
        /// <value> The debounce timeout for the GPIO pin, which is an interval during which changes to the value of the
        ///     pin are filtered out and do not generate <c>ValueChanged</c> events. If the length of this interval is
        ///     0, all changes to the value of the pin generate <c>ValueChanged</c> events.</value>
        extern public TimeSpan DebounceTimeout
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        /// <summary>
        /// Gets the pin number of the general-purpose I/O (GPIO) pin.
        /// </summary>
        /// <value>The pin number of the GPIO pin.</value>
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

        /// <summary>
        /// Gets the sharing mode in which the general-purpose I/O (GPIO) pin is open.
        /// </summary>
        /// <value>The sharing mode in which the GPIO pin is open.</value>
        public GpioSharingMode SharingMode
        {
            get
            {
                return GpioSharingMode.Exclusive;
            }
        }

        /// <summary>
        /// Reads the current value of the general-purpose I/O (GPIO) pin.
        /// </summary>
        /// <returns>The current value of the GPIO pin. If the pin is configured as an output, this value is the last
        ///     value written to the pin.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public GpioPinValue Read();

        /// <summary>
        /// Drives the specified value onto the general purpose I/O (GPIO) pin according to the current drive mode for
        /// the pin if the pin is configured as an output, or updates the latched output value for the pin if the pin is
        /// configured as an input.
        /// </summary>
        /// <param name="value">The enumeration value to write to the GPIO pin.
        ///     <para>If the GPIO pin is configured as an output, the method drives the specified value onto the pin
        ///         according to the current drive mode for the pin.</para>
        ///     <para>If the GPIO pin is configured as an input, the method updates the latched output value for the pin.
        ///         The latched output value is driven onto the pin when the configuration for the pin changes to
        ///         output.</para></param>
        /// <remarks>If the pin drive mode is not currently set to output, this will latch <paramref name="value"/>
        ///     and drive the signal the when the mode is set.</remarks>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Write(GpioPinValue value);

        /// <summary>
        /// Gets whether the general-purpose I/O (GPIO) pin supports the specified drive mode.
        /// </summary>
        /// <param name="driveMode">The drive mode to check for support.</param>
        /// <returns>True if the GPIO pin supports the drive mode that driveMode specifies; otherwise false. If you
        ///     specify a drive mode for which this method returns false when you call SetDriveMode, SetDriveMode
        ///     generates an exception.</param>
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

        /// <summary>
        /// Gets the current drive mode for the general-purpose I/O (GPIO) pin. The drive mode specifies whether the pin
        /// is configured as an input or an output, and determines how values are driven onto the pin.
        /// </summary>
        /// <returns>An enumeration value that indicates the current drive mode for the GPIO pin. The drive mode
        ///     specifies whether the pin is configured as an input or an output, and determines how values are driven
        ///     onto the pin.</returns>
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

        /// <summary>
        /// Sets the drive mode of the general-purpose I/O (GPIO) pin. The drive mode specifies whether the pin is
        /// configured as an input or an output, and determines how values are driven onto the pin.
        /// </summary>
        /// <param name="driveMode">An enumeration value that specifies drive mode to use for the GPIO pin. The drive
        ///     mode specifies whether the pin is configured as an input or an output, and determines how values are
        ///     driven onto the pin.</param>
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

        /// <summary>
        /// Closes the general-purpose I/O (GPIO) pin and releases the resources associated with it.
        /// </summary>
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

        /// <summary>
        /// Binds the pin to a given pin number.
        /// </summary>
        /// <param name="pinNumber">Number of the pin to bind this object to.</param>
        /// <returns>True if the pin was found and reserved; otherwise false.</returns>
        /// <remarks>If this method throws or returns false, there is no need to dispose the pin. </remarks>
        internal bool Init(int pinNumber)
        {
            bool foundPin = InitNative(pinNumber);
            if (foundPin)
            {
                s_eventListener.AddPin(pinNumber, this);
            }

            return foundPin;
        }

        /// <summary>
        /// Handles internal events and re-dispatches them to the publicly subsribed delegates.
        /// </summary>
        /// <param name="edge">The state transition for this event.</param>
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

        /// <summary>
        /// Initialize the interop components of the pin.
        /// </summary>
        /// <param name="pinNumber">The pin number to bind this object to.</param>
        /// <returns>True if the pin was found and reserved; otherwise false.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private bool InitNative(int pinNumber);

        /// <summary>
        /// Release the interop components of the pin.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void DisposeNative();

        /// <summary>
        /// Interop method to set the pin drive mode in hardware.
        /// </summary>
        /// <param name="driveMode">Drive mode to set.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void SetDriveModeInternal(GpioPinDriveMode driveMode);

        /// <summary>
        /// Releases internal resources held by the GPIO pin.
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeNative();
                s_eventListener.RemovePin(m_pinNumber);
            }
        }
    }
}
