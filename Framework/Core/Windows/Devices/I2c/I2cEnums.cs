namespace Windows.Devices.I2c
{
    public enum I2cBusSpeed
    {
        StandardMode = 0,   // 100kHz
        FastMode,           // 400kHz
    }

    public enum I2cSharingMode
    {
        Exclusive = 0,
        Shared,
    }

    public enum I2cTransferStatus
    {
        FullTransfer = 0,
        PartialTransfer,
        SlaveAddressNotAcknowledged,
    }
}
