using System;
using Microsoft.SPOT.Hardware;

namespace Windows.Devices.I2c
{
    public struct I2cTransferResult
    {
        public I2cTransferStatus Status;
        public uint BytesTransferred;
    }

    public sealed class I2cDevice : IDisposable
    {
        // Construction and destruction

        internal I2cDevice(ushort slaveAddress, I2cBusSpeed busSpeed)
        {
            int clockRateKhz = 100;
            if (busSpeed == I2cBusSpeed.FastMode)
            {
                clockRateKhz = 400;
            }

            m_configuration = new I2CDevice.Configuration(slaveAddress, clockRateKhz);

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

        // Public methods

        public static string GetDeviceSelector()
        {
            return Enumeration.DeviceInformation.s_I2cPrefix;
        }

        public static string GetDeviceSelector(string friendlyName)
        {
            return friendlyName;
        }

        // FUTURE: This should be "Task<I2cDevice> FromIdAsync(...)"
        public static I2cDevice FromId(string deviceId, I2cConnectionSettings settings)
        {
            if (settings.AddressingMode != I2cAddressingMode.SevenBit)
            {
                throw new ArgumentException("Ten-bit addressing mode not supported.", "settings");
            }

            if (settings.SharingMode != I2cSharingMode.Exclusive)
            {
                throw new ArgumentException("Shared mode not supported on single-process devices.", "settings");
            }

            if ((settings.SlaveAddress > ushort.MaxValue) || (settings.SlaveAddress < ushort.MinValue))
            {
                throw new ArgumentOutOfRangeException("Slave address is out of range.", "settings");
            }

            if (deviceId != "I2C0")
            {
                // TODO: Is this the right exception?
                //throw new InvalidOperationException();
            }

            // TODO: Should we protect against creating two devices at the same bus and address?
            return new I2cDevice((ushort)settings.SlaveAddress, settings.BusSpeed);
        }

        public void Write(byte[] writeBuffer)
        {
            WritePartial(writeBuffer);
        }

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

        public void Read(byte[] readBuffer)
        {
            ReadPartial(readBuffer);
        }

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

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            WriteReadPartial(writeBuffer, readBuffer);
        }

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

        // Private methods

        private I2cTransferResult ExecuteTransactions(I2CDevice.I2CTransaction[] transactions)
        {
            uint bytesRequested = 0;
            foreach (var transaction in transactions)
            {
                bytesRequested += (uint)transaction.Buffer.Length;
            }

            I2cTransferResult result;

            lock (s_deviceLock)
            {
                // TODO: How long should our transaction timeout be? 15ms is the max in UWP, but too short for us.
                s_device.Config = m_configuration;
                result.BytesTransferred = (uint)s_device.Execute(transactions, 1000);
            }

            // TODO: We need a more reliable way to report SlaveAddressNotAcknowledged.
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

        // Private fields

        // We need to share a single device between all instances, since it reserves the pins.
        private static object s_deviceLock = new object();
        private static int s_deviceRefs = 0;
        private static I2CDevice s_device = null;

        private object m_syncLock = new object();
        private bool m_disposed = false;
        private I2CDevice.Configuration m_configuration;
    }
}
