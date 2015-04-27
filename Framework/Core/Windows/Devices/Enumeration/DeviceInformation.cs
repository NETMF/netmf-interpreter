using System;

namespace Windows.Devices.Enumeration
{
    public sealed class DeviceInformation
    {
        // Construction and destruction

        internal DeviceInformation(string id, bool isDefault)
        {
            m_id = id;
            m_isDefault = isDefault;
        }

        // Public properties

        public string Id
        {
            get
            {
                return m_id;
            }
        }

        public bool IsDefault
        {
            get
            {
                return m_isDefault;
            }
        }

        // Public methods

        // FUTURE: This should return DeviceInformationCollection
        public static DeviceInformation[] FindAll()
        {
            return FindAll("");
        }

        public static DeviceInformation[] FindAll(string aqsFilter)
        {
            if (aqsFilter.Equals(""))
            {
                var i2cDevices = FindAllI2c();
                var spiDevices = FindAllSpi();

                var allDevices = new DeviceInformation[i2cDevices.Length + spiDevices.Length];
                i2cDevices.CopyTo(allDevices, 0);
                spiDevices.CopyTo(allDevices, i2cDevices.Length);
                return allDevices;
            }
            else if (aqsFilter.Equals(s_I2cPrefix))
            {
                return FindAllI2c();
            }
            else if (aqsFilter.Equals(s_SpiPrefix))
            {
                return FindAllSpi();
            }

            // Custom filter; just pass it through as the ID.
            // TODO: Validate that this bus exists.
            var devices = new DeviceInformation[1];
            devices[0] = new DeviceInformation(aqsFilter, false);
            return devices;
        }

        // Private methods

        private static DeviceInformation[] FindAllI2c()
        {
            // TODO: Determine whether this bus exists.
            var devices = new DeviceInformation[1];
            devices[0] = new DeviceInformation("I2C0", true);
            return devices;
        }

        private static DeviceInformation[] FindAllSpi()
        {
            // TODO: Find all valid SPI buses.
            return new DeviceInformation[0];
        }

        // Internal fields

        // We don't support full AQS in the Micro Framework. Instead, we use a pre-set list of
        // hard-coded strings. These strings should be considered opaque, so developers should use
        // the GetDeviceSelector helpers to ensure future compatibility.
        internal static string s_I2cPrefix = "I2C";
        internal static string s_SpiPrefix = "SPI";

        // Private fields

        private string m_id;
        private bool m_isDefault;
    }
}
