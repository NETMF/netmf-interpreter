namespace Windows.Devices.I2c
{
    /// <summary>
    /// Describes the bus speeds that are available for connecting to an inter-integrated circuit (I²C) device. The bus
    /// speed is the frequency at which to clock the I²C bus when accessing the device.
    /// </summary>
    public enum I2cBusSpeed
    {
        /// <summary>
        /// The standard speed of 100 kilohertz (kHz). This speed is the default.
        /// </summary>
        StandardMode = 0,

        /// <summary>
        /// A fast speed of 400 kHz.
        /// </summary>
        FastMode,
    }

    /// <summary>
    /// Describes the modes in which you can connect to an inter-integrated circuit (I²C) bus address. These modes
    /// determine whether other connections to the I²C bus address can be opened while you are connected to the I²C bus
    /// address.
    /// </summary>
    public enum I2cSharingMode
    {
        /// <summary>
        /// Connects to the I²C bus address exclusively, so that no other connection to the I²C bus address can be made
        /// while you remain connected. This mode is the default mode.
        /// </summary>
        Exclusive = 0,

        /// <summary>
        /// Connects to the I²C bus address in shared mode, so that other connections to the I²C bus address can be made
        /// while you remain connected.
        /// <para>You can perform all operations on shared connections, but use such connections with care. When
        ///     multiple client apps change the global state of the I²C device, race conditions can result.</para>
        /// <para>An example use case for using a shared connection is a sensor that obtains readings without changing
        ///     the state of the device.</para>
        /// </summary>
        Shared,
    }

    /// <summary>
    /// Describes whether the data transfers that the ReadPartial, WritePartial, or WriteReadPartial method performed
    /// succeeded, or provides the reason that the transfers did not succeed.
    /// </summary>
    public enum I2cTransferStatus
    {
        /// <summary>
        /// The data was entirely transferred. For WriteReadPartial, the data for both the write and the read operations
        /// was entirely transferred.
        /// <para>For this status code, the value of the I2cTransferResult.BytesTransferred member that the method
        ///     returns is the same as the size of the buffer you specified when you called the method, or is equal to
        ///     the sum of the sizes of two buffers that you specified for WriteReadPartial.</para>
        /// </summary>
        FullTransfer = 0,

        /// <summary>
        /// The I²C device negatively acknowledged the data transfer before all of the data was transferred.
        /// <para>For this status code, the value of the I2cTransferResult.BytesTransferred member that the method
        ///     returns is the number of bytes actually transferred. For WriteReadPartial, the value is the sum of the
        ///     number of bytes that the operation wrote and the number of bytes that the operation read.</para>
        /// </summary>
        PartialTransfer,

        /// <summary>
        /// The bus address was not acknowledged.
        /// <para>For this status code, the value of the I2cTransferResult.BytesTransferred member that the method
        ///     returns of the method is 0.</para>
        /// </summary>
        SlaveAddressNotAcknowledged,
    }
}
