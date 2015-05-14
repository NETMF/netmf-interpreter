using System;

namespace Windows.Devices.I2c
{
    /// <summary>
    /// Represents the connection settings you want to use for an inter-integrated circuit (I²C) device.
    /// </summary>
    public sealed class I2cConnectionSettings
    {
        private int m_slaveAddress = 0;
        private I2cBusSpeed m_busSpeed = I2cBusSpeed.StandardMode;
        private I2cSharingMode m_sharingMode = I2cSharingMode.Exclusive;

        /// <summary>
        /// Creates and initializes a new instance of the I2cConnectionSettings class for inter-integrated
        /// circuit (I2C) device with specified bus address, using the default settings of the standard mode for the bus
        /// speed and exclusive sharing mode.
        /// </summary>
        /// <param name="slaveAddress">Initial address of the device.</param>
        public I2cConnectionSettings(int slaveAddress)
        {
            m_slaveAddress = slaveAddress;
        }

        /// <summary>
        /// Construct a copy of an I2cConnectionSettings object.
        /// </summary>
        /// <param name="source">Source object to copy from.</param>
        internal I2cConnectionSettings(I2cConnectionSettings source)
        {
            m_slaveAddress = source.m_slaveAddress;
            m_busSpeed = source.m_busSpeed;
            m_sharingMode = source.m_sharingMode;
        }

        /// <summary>
        /// Gets or sets the bus address of the inter-integrated circuit (I²C) device.
        /// </summary>
        /// <value>The bus address of the I²C device. Only 7-bit addressing is supported, so the range of valid values
        ///     is from 8 to 119.</value>
        public int SlaveAddress
        {
            get
            {
                return m_slaveAddress;
            }

            set
            {
                m_slaveAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the bus speed to use for connecting to an inter-integrated circuit (I²C) device. The bus speed
        /// is the frequency at which to clock the I²C bus when accessing the device.
        /// </summary>
        /// <value>The bus speed to use for connecting to an I²C device.</value>
        public I2cBusSpeed BusSpeed
        {
            get
            {
                return m_busSpeed;
            }

            set
            {
                m_busSpeed = value;
            }
        }

        /// <summary>
        /// Gets or sets the sharing mode to use to connect to the inter-integrated circuit (I²C) bus address. This mode
        /// determines whether other connections to the I²C bus address can be opened while you are connect to the I²C
        /// bus address.
        /// </summary>
        /// <value>The sharing mode to use to connect to the I²C bus address.</value>
        public I2cSharingMode SharingMode
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
