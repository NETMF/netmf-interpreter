namespace Windows.Devices.Spi
{
    /// <summary>
    /// Represents the settings for the connection with a device.
    /// </summary>
    public sealed class SpiConnectionSettings
    {
        private int m_chipSelectionLine;
        private int m_dataBitLength; // TODO: Default value?
        private int m_clockFrequency; // TODO: Default value?
        private SpiMode m_mode = SpiMode.Mode0;
        private SpiSharingMode m_sharingMode = SpiSharingMode.Exclusive;

        /// <summary>
        /// Initializes new instance of SpiConnectionSettings.
        /// </summary>
        /// <param name="chipSelectionLine">The chip select line on which the connection will be made.</param>
        public SpiConnectionSettings(int chipSelectionLine)
        {
            m_chipSelectionLine = chipSelectionLine;
        }

        /// <summary>
        /// Construct a copy of an SpiConnectionSettings object.
        /// </summary>
        /// <param name="source">Source object to copy from.</param>
        internal SpiConnectionSettings(SpiConnectionSettings source)
        {
            m_chipSelectionLine = source.m_chipSelectionLine;
            m_dataBitLength = source.m_dataBitLength;
            m_clockFrequency = source.m_clockFrequency;
            m_mode = source.m_mode;
            m_sharingMode = source.m_sharingMode;
        }

        /// <summary>
        /// Gets or sets the chip select line for the connection to the SPI device.
        /// </summary>
        /// <value>The chip select line.</value>
        public int ChipSelectionLine
        {
            get
            {
                return m_chipSelectionLine;
            }

            set
            {
                m_chipSelectionLine = value;
            }
        }

        /// <summary>
        /// Gets or sets the SpiMode for this connection.
        /// </summary>
        /// <value>The communication mode.</value>
        public SpiMode Mode
        {
            get
            {
                return m_mode;
            }

            set
            {
                m_mode = value;
            }
        }

        /// <summary>
        /// Gets or sets the bit length for data on this connection.
        /// </summary>
        /// <value>The data bit length.</value>
        public int DataBitLength
        {
            get
            {
                return m_dataBitLength;
            }

            set
            {
                m_dataBitLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the clock frequency for the connection.
        /// </summary>
        /// <value>Value of the clock frequency in Hz.</value>
        public int ClockFrequency
        {
            get
            {
                return m_clockFrequency;
            }

            set
            {
                m_clockFrequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the sharing mode for the SPI connection.
        /// </summary>
        /// <value>The sharing mode.</value>
        public SpiSharingMode SharingMode
        {
            get
            {
                return m_sharingMode;
            }

            set
            {
                m_sharingMode = value;
            }
        }
    }
}
