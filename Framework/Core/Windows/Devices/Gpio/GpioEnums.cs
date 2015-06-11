namespace Windows.Devices.Gpio
{
    /// <summary>
    /// Describes the modes in which you can open a general-purpose I/O (GPIO) pin. These modes determine whether other
    /// connections to the GPIO pin can be opened while you have the pin open.
    /// </summary>
    public enum GpioSharingMode
    {
        /// <summary>
        /// Opens the GPIO pin exclusively, so that no other connection to the pin can be opened.
        /// </summary>
        Exclusive = 0,

        /// <summary>
        /// Opens the GPIO pin as shared, so that other connections in SharedReadOnly mode to the pin can be opened.
        /// </summary>
        SharedReadOnly,
    }

    /// <summary>
    /// Describes the possible results of opening a pin with the GpioPin.TryOpenPin method.
    /// </summary>
    public enum GpioOpenStatus
    {
        /// <summary>
        /// The GPIO pin was successfully opened.
        /// </summary>
        PinOpened = 0,

        /// <summary>
        /// The pin is reserved by the system and is not available to apps that run in user mode.
        /// </summary>
        PinUnavailable,

        /// <summary>
        /// The pin is currently open in an incompatible sharing mode.
        /// </summary>
        SharingViolation,
    }

    /// <summary>
    /// Describes whether a general-purpose I/O (GPIO) pin is configured as an input or an output, and how values are
    /// driven onto the pin.
    /// </summary>
    public enum GpioPinDriveMode
    {
        /// <summary>
        /// Configures the GPIO pin in floating mode, with high impedance.
        /// <para>If you call the GpioPin.Read method for this pin, the method returns the current state of the pin as
        ///     driven externally.</para>
        /// <para>If you call the GpioPin.Write method, the method sets the latched output value for the pin. The pin
        ///     takes on this latched output value when the pin is changed to an output.</para>
        /// </summary>
        Input = 0,

        /// <summary>
        /// Configures the GPIO pin in strong drive mode, with low impedance.
        /// <para>If you call the GpioPin.Write method for this pin with a value of GpioPinValue.High, the method
        ///     produces a low-impedance high value for the pin. If you call the GpioPin.Write method for this pin with
        ///     a value of GpioPinValue.Low, the method produces a low-impedance low value for the pin.</para>
        /// <para>If you call the GpioPin.Read method for this pin, the method returns the value previously written to
        ///     the pin.</para>
        /// </summary>
        Output,

        /// <summary>
        /// Configures the GPIO pin in pull-up mode, with high impedance.
        /// <para>If you call the GpioPin.Read method for this pin, the method returns the current state of the pin as
        ///     driven externally.</para>
        /// <para>If you call the GpioPin.Write method, the method sets the latched output value for the pin. The pin
        ///     takes on this latched output value when the pin is changed to an output.</para>
        /// </summary>
        InputPullUp,

        /// <summary>
        /// Configures the GPIO pin in pull-down mode, with high impedance.
        /// <para>If you call the GpioPin.Read method for this pin, the method returns the current state of the pin as
        ///     driven externally.</para>
        /// <para>If you call the GpioPin.Write method, the method sets the latched output value for the pin. The pin
        ///     takes on this latched output value when the pin is changed to an output.</para>
        /// </summary>
        InputPullDown,

        /// <summary>
        /// Not supported.
        /// </summary>
        OutputOpenDrain,

        /// <summary>
        /// Not supported.
        /// </summary>
        OutputOpenDrainPullUp,

        /// <summary>
        /// Not supported.
        /// </summary>
        OutputOpenSource,

        /// <summary>
        /// Not supported.
        /// </summary>
        OutputOpenSourcePullDown,
    }

    /// <summary>
    /// Describes the possible values for a general-purpose I/O (GPIO) pin.
    /// </summary>
    public enum GpioPinValue
    {
        /// <summary>
        /// The value of the GPIO pin is low.
        /// </summary>
        Low = 0,

        /// <summary>
        /// The value of the GPIO pin is high.
        /// </summary>
        High,
    }

    /// <summary>
    /// Describes the possible types of change that can occur to the value of the general-purpose I/O (GPIO) pin for the
    /// GpioPin.ValueChanged event.
    /// </summary>
    public enum GpioPinEdge
    {
        /// <summary>
        /// The value of the GPIO pin changed from high to low.
        /// </summary>
        FallingEdge = 0,

        /// <summary>
        /// The value of the GPIO pin changed from low to high.
        /// </summary>
        RisingEdge,
    }
}
