using System;
using System.Collections;

namespace Windows.Devices.Enumeration
{
    public sealed class DeviceInformation
    {
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
            // We don't support full AQS in the Micro Framework. Instead, we use a pre-set list of
            // hard-coded strings. These strings should be considered opaque, so developers should use
            // the GetDeviceSelector helpers to ensure future compatibility.
            ArrayList foundDevices = new ArrayList();

            // Find all I2C buses which contain the given prefix.
            var i2cBusNames = I2c.I2cDevice.GetValidBusNames();
            for (int i = 0; i < i2cBusNames.Length; ++i)
            {
                if (i2cBusNames[i].IndexOf(aqsFilter) == 0)
                {
                    // TODO: Issue #102: Determine whether this bus exists.
                    foundDevices.Add(new DeviceInformation(i2cBusNames[i], i == 0));
                }
            }

            // Find all SPI buses which contain the given prefix.
            var spiBusNames = Spi.SpiDevice.GetValidBusNames();
            for (int i = 0; i < spiBusNames.Length; ++i)
            {
                if (spiBusNames[i].IndexOf(aqsFilter) == 0)
                {
                    // TODO: Issue #102: Determine whether this bus exists.
                    foundDevices.Add(new DeviceInformation(spiBusNames[i], i == 0));
                }
            }

            var allDevices = new DeviceInformation[foundDevices.Count];
            foundDevices.CopyTo(allDevices);
            return allDevices;
        }
    }
}
