using System;

namespace Microsoft.SPOT.Emulator.I2c
{
    public interface II2cDriver
    {
        bool Initialize();
        bool Uninitialize();
        bool Enqueue( I2cXAction xAction );
        void Cancel( IntPtr xAction, bool signal );
        void GetPins( out uint scl, out uint sda);
        
    }

    [Flags]
    public enum I2cStatus : byte
    {
        Idle = 0x01,
        Scheduled = 0x02,
        Processing = 0x04,
        Completed = 0x08,
        Aborted = 0x10,
        Cancelled = 0x20
    }

    public class I2cXAction
    {
        I2cXActionUnit[] _xActionUnits;
        uint _clockRate;
        byte _address;
        I2cStatus _status;

        public I2cXAction( I2cXActionUnit[] xActionUnits, byte address, uint clockRate, I2cStatus status )
        {
            _xActionUnits = xActionUnits;
            _address = address;
            _clockRate = clockRate;
            _status = status;
        }

        public byte Address
        {
            get { return _address; }
        }

        public uint ClockRate
        {
            get { return _clockRate; }
        }

        public I2cStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public I2cXActionUnit[] XActionUnits
        {
            get { return _xActionUnits; }
        }
    }

    public class I2cXActionUnit
    {
        byte[] _data;
        bool _isRead;

        public I2cXActionUnit( bool isRead, byte[] data )
        {
            _isRead = isRead;
            _data = data;
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public bool IsRead
        {
            get { return _isRead; }
        }
    }
}
