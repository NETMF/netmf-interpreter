namespace Windows.Devices.Gpio
{
    public enum GpioSharingMode
    {
        Exclusive = 0, // No other connections may be opened to this pin.
        SharedReadOnly,
    }

    public enum GpioOpenStatus
    {
        PinOpened = 0,
        PinUnavailable,
        SharingViolation,
    }

    public enum GpioPinDriveMode
    {
        Input = 0,
        Output,
        InputWithPullUp,
        InputWithPullDown,
        OutputStrongLow,
        OutputStrongLowPullUp,
        OutputStrongHigh,
        OutputStrongHighPullDown,
    }

    public enum GpioPinValue
    {
        Low = 0,
        High,
    }

    public enum GpioPinEdge
    {
        RisingEdge = 0,
        FallingEdge,
    }
}
