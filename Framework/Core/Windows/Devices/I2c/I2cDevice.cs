using System;
using Microsoft.SPOT.Hardware;

namespace Windows.Devices.I2c
{
    /// <summary>
    /// Provides information about whether the data transfers that the ReadPartial, WritePartial, or WriteReadPartial
    /// method performed succeeded, and the actual number of bytes the method transferred.
    /// </summary>
    public struct I2cTransferResult
    {
        /// <summary>
        /// An enumeration value that indicates if the read or write operation transferred the full number of bytes that
        /// the method requested, or the reason that the full transfer did not succeed. For WriteReadPartial, the value
        /// indicates whether the data for both the write and the read operations was entirely transferred.
        /// </summary>
        public I2cTransferStatus Status;

        /// <summary>
        /// The actual number of bytes that the operation actually transferred. The following table describes what this
        /// value represents for each method.
        /// </summary>
        public uint BytesTransferred;
    }

    /// <summary>
    /// Represents a communications channel to a device on an inter-integrated circuit (I²C) bus.
    /// </summary>
    public sealed class I2cDevice : IDisposable
    {
        // We need to share a single device between all instances, since it reserves the pins.
        private static object s_deviceLock = new object();
        private static int s_deviceRefs = 0;
        private static I2CDevice s_device = null;
        private static string s_I2cPrefix = "I2C";

        private readonly string m_deviceId;
        private readonly I2cConnectionSettings m_settings;

        private object m_syncLock = new object();
        private bool m_disposed = false;
        private I2CDevice.Configuration m_configuration;

        /// <summary>
        /// Constructs a new I2cDevice object.
        /// </summary>
        /// <param name="slaveAddress">The bus address of the I²C device. Only 7-bit addressing is supported, so the
        ///     range of valid values is from 8 to 119.</param>
        /// <param name="busSpeed"></param>
        internal I2cDevice(string deviceId, I2cConnectionSettings settings)
        {
            m_deviceId = deviceId.Substring(0, deviceId.Length);
            m_settings = new I2cConnectionSettings(settings);

            int clockRateKhz = 100;
            if (settings.BusSpeed == I2cBusSpeed.FastMode)
            {
                clockRateKhz = 400;
            }

            m_configuration = new I2CDevice.Configuration((ushort)settings.SlaveAddress, clockRateKhz);

            lock (s_deviceLock)
            {
                if (s_device == null)
                {
                    s_device = new I2CDevice(m_configuration);
                }

                ++s_deviceRefs;
            }
        }

        ~I2cDevice()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the plug and play device identifier of the inter-integrated circuit (I2C) bus controller for the device.
        /// </summary>
        /// <value>The plug and play device identifier of the inter-integrated circuit (I²C) bus controller for the
        ///     device.</value>
        public string DeviceId
        {
            get
            {
                return m_deviceId.Substring(0, m_deviceId.Length);
            }
        }

        /// <summary>
        /// Gets the connection settings used for communication with the inter-integrated circuit (I²C) device.
        /// </summary>
        /// <value>The connection settings used for communication with the inter-integrated circuit (I²C) device.</value>
        /// <remarks>If you try to change the values of the I2cConnectionSettings object that you get through this
        ///     property, those changes will have no effect. You cannot change the connection settings after you create
        ///     the I2cDevice object. To specify the connection settings, use the I2cConnectionSettings(Int32)
        ///     constructor to create an I2cConnectionSettings object, and set the property values for that
        ///     I2cConnectionSettings object before you pass it to the FromIdAsync method to create the I2cDevice object.</remarks>
        public I2cConnectionSettings ConnectionSettings
        {
            get
            {
                return new I2cConnectionSettings(m_settings);
            }
        }

        /// <summary>
        /// Retrieves an Advanced Query Syntax (AQS) string for all of the inter-integrated circuit (I²C) bus
        /// controllers on the system. You can use this string with the DeviceInformation.FindAllAsync method to get
        /// DeviceInformation objects for those bus controllers.
        /// </summary>
        /// <returns>An AQS string for all of the I²C bus controllers on the system, which you can use with the
        ///     DeviceInformation.FindAllAsync method to get DeviceInformation objects for those bus controllers.</returns>
        public static string GetDeviceSelector()
        {
            return s_I2cPrefix;
        }

        /// <summary>
        /// Retrieves an Advanced Query Syntax (AQS) string for the inter-integrated circuit (I²C) bus that has the
        /// specified friendly name. You can use this string with the DeviceInformation.FindAllAsync method to get a
        /// DeviceInformation object for that bus.
        /// </summary>
        /// <param name="friendlyName">A friendly name for the particular I²C bus on a particular hardware platform for
        ///     which you want to get the AQS string.</param>
        /// <returns>An AQS string for the I²C bus that friendlyName specifies, which you can use with the
        ///     DeviceInformation.FindAllAsync method to get a DeviceInformation object for that bus.</returns>
        public static string GetDeviceSelector(string friendlyName)
        {
            return friendlyName;
        }

        /// <summary>
        /// Retrieves an I2cDevice object asynchronously for the inter-integrated circuit (I²C) bus controller that has
        /// the specified plug and play device identifier, using the specified connection settings.
        /// </summary>
        /// <param name="deviceId">The plug and play device identifier of the I²C bus controller for which you want to
        ///     create an I2cDevice object.</param>
        /// <param name="settings">The connection settings to use for communication with the I²C bus controller that
        ///     <paramref name="deviceId"/> specifies.</param>
        /// <returns>The I2cDevice object.</returns>
        public static I2cDevice FromId(string deviceId, I2cConnectionSettings settings)
        {
            // FUTURE: This should should be "Task<I2cDevice> FromIdAsync(...)"
            if ((settings.SlaveAddress > 0) || (settings.SlaveAddress < 127))
            {
                throw new ArgumentOutOfRangeException();
            }

            if (deviceId != (s_I2cPrefix + "1"))
            {
                throw new InvalidOperationException();
            }

            return new I2cDevice(deviceId, settings);
        }

        /// <summary>
        /// Writes data to the inter-integrated circuit (I²C) bus on which the device is connected, based on the bus
        /// address specified in the I2cConnectionSettings object that you used to create the I2cDevice object.
        /// </summary>
        /// <param name="writeBuffer">A buffer that contains the data that you want to write to the I²C device. This
        ///     data should not include the bus address.</param>
        public void Write(byte[] writeBuffer)
        {
            WritePartial(writeBuffer);
        }

        /// <summary>
        /// Writes data to the inter-integrated circuit (I²C) bus on which the device is connected, and returns
        /// information about the success of the operation that you can use for error handling.
        /// </summary>
        /// <param name="buffer">A buffer that contains the data that you want to write to the I²C device. This data
        ///     should not include the bus address.</param>
        /// <returns>A structure that contains information about the success of the write operation and the actual
        ///     number of bytes that the operation wrote into the buffer.</returns>
        public I2cTransferResult WritePartial(byte[] buffer)
        {
            lock (m_syncLock)
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                var transactions = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(buffer) };
                return ExecuteTransactions(transactions);
            }
        }

        /// <summary>
        /// Reads data from the inter-integrated circuit (I²C) bus on which the device is connected into the specified
        /// buffer.
        /// </summary>
        /// <param name="readBuffer">The buffer to which you want to read the data from the I²C bus. The length of the
        ///     buffer determines how much data to request from the device.</param>
        public void Read(byte[] readBuffer)
        {
            ReadPartial(readBuffer);
        }

        /// <summary>
        /// Reads data from the inter-integrated circuit (I²C) bus on which the device is connected into the specified
        /// buffer, and returns information about the success of the operation that you can use for error handling.
        /// </summary>
        /// <param name="buffer">The buffer to which you want to read the data from the I²C bus. The length of the
        ///     buffer determines how much data to request from the device.</param>
        /// <returns>A structure that contains information about the success of the read operation and the actual number
        ///     of bytes that the operation read into the buffer.</returns>
        public I2cTransferResult ReadPartial(byte[] buffer)
        {
            lock (m_syncLock)
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                var transactions = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(buffer) };
                return ExecuteTransactions(transactions);
            }
        }

        /// <summary>
        /// Performs an atomic operation to write data to and then read data from the inter-integrated circuit (I²C) bus
        /// on which the device is connected, and sends a restart condition between the write and read operations.
        /// </summary>
        /// <param name="writeBuffer">A buffer that contains the data that you want to write to the I²C device. This
        ///     data should not include the bus address.</param>
        /// <param name="readBuffer">The buffer to which you want to read the data from the I²C bus. The length of the
        ///     buffer determines how much data to request from the device.</param>
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            WriteReadPartial(writeBuffer, readBuffer);
        }

        /// <summary>
        /// Performs an atomic operation to write data to and then read data from the inter-integrated circuit (I²C) bus
        /// on which the device is connected, and returns information about the success of the operation that you can
        /// use for error handling.
        /// </summary>
        /// <param name="writeBuffer">A buffer that contains the data that you want to write to the I²C device. This
        ///     data should not include the bus address.</param>
        /// <param name="readBuffer">The buffer to which you want to read the data from the I²C bus. The length of the
        ///     buffer determines how much data to request from the device.</param>
        /// <returns>A structure that contains information about whether both the read and write parts of the operation
        ///     succeeded and the sum of the actual number of bytes that the operation wrote and the actual number of
        ///     bytes that the operation read.</returns>
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer)
        {
            lock (m_syncLock)
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                var transactions = new I2CDevice.I2CTransaction[] {
                    I2CDevice.CreateWriteTransaction(writeBuffer),
                    I2CDevice.CreateReadTransaction(readBuffer) };
                return ExecuteTransactions(transactions);
            }
        }

        /// <summary>
        /// Closes the connection to the inter-integrated circuit (I2C) device.
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

        internal static string[] GetValidBusNames()
        {
            return new string[] {
                s_I2cPrefix + "1",
            };
        }

        /// <summary>
        /// Executes an arbitrary transaction against the wrapped Microsoft.SPOT.Hardware.I2CDevice.
        /// </summary>
        /// <param name="transactions">List of transactions to execute. These may be any combination of read and write.</param>
        /// <returns>A structure that contains information about whether both the read and write parts of the operation
        ///     succeeded and the sum of the actual number of bytes that the operation wrote and the actual number of
        ///     bytes that the operation read.</returns>
        private I2cTransferResult ExecuteTransactions(I2CDevice.I2CTransaction[] transactions)
        {
            // FUTURE: Investigate how short we can make this timeout. UWP APIs should take no
            // longer than 15ms, but this is insufficient for micro-devices.
            const int transactionTimeoutMs = 1000;

            uint bytesRequested = 0;
            foreach (var transaction in transactions)
            {
                bytesRequested += (uint)transaction.Buffer.Length;
            }

            I2cTransferResult result;

            lock (s_deviceLock)
            {
                s_device.Config = m_configuration;
                result.BytesTransferred = (uint)s_device.Execute(transactions, transactionTimeoutMs);
            }

            if (result.BytesTransferred == bytesRequested)
            {
                result.Status = I2cTransferStatus.FullTransfer;
            }
            else if (result.BytesTransferred == 0)
            {
                result.Status = I2cTransferStatus.SlaveAddressNotAcknowledged;
            }
            else
            {
                result.Status = I2cTransferStatus.PartialTransfer;
            }

            return result;
        }

        /// <summary>
        /// Releases internal resources held by the device.
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (s_deviceLock)
                {
                    --s_deviceRefs;
                    if ((s_deviceRefs == 0) && (s_device != null))
                    {
                        s_device.Dispose();
                        s_device = null;
                    }
                }
            }
        }
    }
}
