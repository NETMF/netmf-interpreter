namespace Windows.Devices.Spi
{
    /// <summary>
    /// Defines the SPI communication mode. The communication mode defines the clock edge on which the master out line
    /// toggles, the master in line samples, and the signal clock's signal steady level (named SCLK). Each mode is
    /// defined with a pair of parameters called clock polarity (CPOL) and clock phase (CPHA).
    /// </summary>
    public enum SpiMode
    {
        /// <summary>
        /// CPOL = 0, CPHA = 0.
        /// </summary>
        Mode0 = 0,

        /// <summary>
        /// CPOL = 0, CPHA = 1.
        /// </summary>
        Mode1,

        /// <summary>
        /// CPOL = 1, CPHA = 0.
        /// </summary>
        Mode2,

        /// <summary>
        /// CPOL = 1, CPHA = 1.
        /// </summary>
        Mode3,
    }

    /// <summary>
    /// Defines the sharing mode for the SPI bus.
    /// </summary>
    public enum SpiSharingMode
    {
        /// <summary>
        /// SPI bus segment is not shared.
        /// </summary>
        Exclusive = 0,

        /// <summary>
        /// SPI bus is shared.
        /// </summary>
        Shared,
    }
}
