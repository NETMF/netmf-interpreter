using System;

namespace Windows.Devices.I2c
{
    public sealed class I2cConnectionSettings
    {
        // Construction and destruction

        public I2cConnectionSettings(int slaveAddress)
        {
            m_slaveAddress = slaveAddress;
        }

        // Public properties

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

        public I2cAddressingMode AddressingMode
        {
            get
            {
                return m_addressingMode;
            }

            set
            {
                m_addressingMode = value;
            }
        }

        // Private fields

        private int m_slaveAddress = 0;
        private I2cBusSpeed m_busSpeed = I2cBusSpeed.StandardMode;
        private I2cSharingMode m_sharingMode = I2cSharingMode.Exclusive;
        private I2cAddressingMode m_addressingMode = I2cAddressingMode.SevenBit;
    }
}
