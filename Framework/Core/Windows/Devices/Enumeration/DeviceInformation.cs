using System;

namespace Windows.Devices.Enumeration
{
    public sealed class DeviceInformation
    {
        // We don't support full AQS in the Micro Framework. Instead, we use a pre-set list of
        // hard-coded strings. These strings should be considered opaque, so developers should use
        // the GetDeviceSelector helpers to ensure future compatibility.
        internal static string s_I2cPrefix = "I2C";
        internal static string s_SpiPrefix = "SPI";

        private string m_id;
        private bool m_isDefault;

        /// <summary>
        /// Constructs a new DeviceInformation object.
        /// </summary>
        /// <param name="id">Unique identifier describing this device.</param>
        /// <param name="isDefault">Whether this is the default device for a given device class.</param>
        internal DeviceInformation(string id, bool isDefault)
        {
            m_id = id;
            m_isDefault = isDefault;
        }

        /// <summary>
        /// A string representing the identity of the device.
        /// </summary>
        /// <value>A string representing the identity of the device.</value>
        /// <remarks>This ID can be used to activate device functionality using the <c>CreateFromIdAsync</c> methods on
        ///     classes that implement device functionality.
        ///     <para>The DeviceInformation object that the Id property identifies is actually a device interface. For
        ///         simplicity in this documentation, the DeviceInformation object is called a device, and the
        ///         identifier in its Id property is called a DeviceInformation ID.</para></remarks>
        public string Id
        {
            get
            {
                return m_id;
            }
        }

        /// <summary>
        /// Indicates whether this device is the default device for the class.
        /// </summary>
        /// <value>Indicates whether this device is the default device for the class.</value>
        public bool IsDefault
        {
            get
            {
                return m_isDefault;
            }
        }

        /// <summary>
        /// Enumerates all DeviceInformation objects.
        /// </summary>
        /// <returns>List of all available DeviceInformation objects.</returns>
        public static DeviceInformation[] FindAll()
        {
            // FUTURE: This should return DeviceInformationCollection
            return FindAll(string.Empty);
        }

        /// <summary>
        /// Enumerates DeviceInformation objects matching the specified Advanced Query Syntax (AQS) string.
        /// </summary>
        /// <param name="aqsFilter"></param>
        /// <returns>List of available DeviceInformation objects matching the given criteria..</returns>
        public static DeviceInformation[] FindAll(string aqsFilter)
        {
            if (aqsFilter.Equals(string.Empty))
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
            // TODO: Issue #102: Validate that this bus exists.
            var devices = new DeviceInformation[1];
            devices[0] = new DeviceInformation(aqsFilter, false);
            return devices;
        }

        /// <summary>
        /// Find all currently available I2C buses.
        /// </summary>
        /// <returns>List of DeviceInformation objects describing I2C objects.</returns>
        private static DeviceInformation[] FindAllI2c()
        {
            // TODO: Issue #102: Determine whether this bus exists.
            var devices = new DeviceInformation[1];
            devices[0] = new DeviceInformation("I2C0", true);
            return devices;
        }

        /// <summary>
        /// Find all currently available SPI buses.
        /// </summary>
        /// <returns>List of DeviceInformation objects describing SPI objects.</returns>
        private static DeviceInformation[] FindAllSpi()
        {
            // FUTURE: Find all valid SPI buses.
            return new DeviceInformation[0];
        }
    }
}
