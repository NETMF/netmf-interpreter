////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.SPOT.Hardware;


namespace Microsoft.SPOT.Emulator.I2c
{
    internal class I2cDriver : HalDriver<II2cDriver>, II2cDriver
    {
        private I2cDevice GetI2cDevice( byte address )
        {
            return this.Emulator.I2cBus.DeviceGet( address );
        }

        #region II2cDriver Members

        bool II2cDriver.Initialize()
        {
            return true;
        }

        bool II2cDriver.Uninitialize()
        {
            return true;
        }

        bool II2cDriver.Enqueue( I2cXAction xAction )
        {
            bool success = GetI2cDevice( xAction.Address ).DeviceExecute( xAction );

            // We are emulating this synchronously, and the tranaction can only complete successfully or not at all.
            xAction.Status = (success) ? I2cStatus.Completed : I2cStatus.Aborted;

            Emulator.SetSystemEvents( Microsoft.SPOT.Emulator.Events.SystemEvents.I2C_XACTION );

            return success;
        }

        void II2cDriver.Cancel( IntPtr xAction, bool signal )
        {
            //no-op
        }


        void II2cDriver.GetPins( out uint scl, out uint sda)
        {
            Emulator.I2cBus.GetPins(out scl, out sda);
        }

        #endregion
    }

    public class I2cBus : EmulatorComponentCollection
    {
        Dictionary<byte, I2cDevice> _deviceLookup;
        List<I2cDevice> _validDevices;

        public I2cBus()
            : base( typeof( I2cDevice ) )
        {
            _deviceLookup = new Dictionary<byte, I2cDevice>();
            _validDevices = new List<I2cDevice>();
        }

        internal I2cDevice DeviceGet( byte address )
        {
            I2cDevice device;

            if (_deviceLookup.TryGetValue( address, out device ) == false)
            {
                device = new I2cDeviceNull( address );
                _deviceLookup.Add( address, device );
            }

            if (device is I2cDeviceNull)
            {
                I2cDeviceNull nullDevice = (I2cDeviceNull)device;

                if (nullDevice.DisplayWarning || Emulator.Verbose)
                {
                    Trace.WriteLine( "Warning: System attempts to access an I2C device at address " + address + " that was not configured." );
                    nullDevice.TurnOffWarning();
                }
            }

            return device;
        }

        public I2cDevice this[byte address]
        {
            get
            {
                if (Exists( address ) == false)
                {
                    throw new ArgumentException( "I2cDevice at address " + address + " does not exist." );
                }

                return _deviceLookup[address];
            }
        }

        internal void GetPins(out uint scl, out uint sda)
        {
            if (_validDevices.Count > 0)
            {
                scl = (uint)(int)((I2cDevice)_validDevices[0]).Scl;
                sda = (uint)(int)((I2cDevice)_validDevices[0]).Sda;
                return;
            }

            scl = (uint)0xffffffff;
            sda = (uint)0xffffffff;
        }

        internal override void RegisterInternal(EmulatorComponent ec)
        {
            I2cDevice i2cDevice = ec as I2cDevice;

            if ((i2cDevice == null) || (i2cDevice is I2cDeviceNull))
            {
                throw new Exception( "Attempt to register a non I2cDevice with I2cBus." );
            }

            byte address = i2cDevice.Address;

            if (Exists( address ))
                throw new Exception( "I2C device already set up at address " + address + "." );

            _deviceLookup[address] = i2cDevice;
            _validDevices.Add(i2cDevice);

            base.RegisterInternal( ec );
        }

        internal override void UnregisterInternal(EmulatorComponent ec)
        {
            I2cDevice i2cDevice = ec as I2cDevice;

            if (i2cDevice != null)
            {
                Debug.Assert( _deviceLookup[i2cDevice.Address] == i2cDevice );

                _deviceLookup.Remove( i2cDevice.Address );
                _validDevices.Remove(i2cDevice);

                base.UnregisterInternal(ec);
            }
        }

        private bool Exists( byte address )
        {
            return _deviceLookup.ContainsKey( address ) && ((_deviceLookup[address] is I2cDeviceNull) == false);
        }

        public override IEnumerator GetEnumerator()
        {
            return _validDevices.GetEnumerator();
        }

        public override int Count
        {
            get { return _validDevices.Count; }
        }

        public override void CopyTo(Array array, int index)
        {
            I2cDevice[] i2cDeviceArray = array as I2cDevice[];
            if (i2cDeviceArray == null)
            {
                throw new ArgumentException("Cannot cast array into I2cDevice[]");
            }
            _validDevices.CopyTo(i2cDeviceArray, index);
        }
    }

    public abstract class I2cDevice : EmulatorComponent
    {
        protected byte _address;
        protected Cpu.Pin _scl = Cpu.Pin.GPIO_NONE;
        protected Cpu.Pin _sda = Cpu.Pin.GPIO_NONE;

        public I2cDevice()
        {
            _address = 0;
        }

        public override void SetupComponent()
        {
            if (IsValidAddress(_address) == false)
            {
                throw new Exception( "The I2cDevice has an invalid address." );
            }
        }

        public static bool IsValidAddress( byte address )
        {
            //SPOT#15874 -- this is a 7 bit address, from 0x00 - 0x80

            if ((address & 0x80) != 0) // the LSB is the RW bit, not part of the address.
            {
                return false;
            }

            if (address == 0x02) return true; //adderss 0x02 is can be used for differentbus format

            switch (address & 0x78)
            {
                case 0x00: // Top 4 address bits are 0 -> reserve
                case 0x78: // Top 4 address bits are 1 -> reserve 
                    return false; 
                default:
                    return true;
            }
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            I2cDevice i2cDevice = ec as I2cDevice;
            if (i2cDevice != null)
            {
                return i2cDevice.Address == this.Address;
            }
            else
            {
                return false;
            }
        }

        public byte Address
        {
            get { return _address; }
            set
            {
                ThrowIfNotConfigurable();

                if (_address == 0)
                {
                    if (IsValidAddress( value ) == false)
                    {
                        throw new Exception( "Invalid I2C address: " + value );
                    }

                    _address = value;
                }
                else
                {
                    throw new Exception( "Address can only be set once." );
                }
            }
        }

        public Cpu.Pin Scl
        {
            get { return _scl; }
            set { ThrowIfNotConfigurable(); _scl = value; }
        }

        public Cpu.Pin Sda
        {
            get { return _sda; }
            set { ThrowIfNotConfigurable(); _sda = value; }
        }

        internal virtual bool DeviceExecute( I2cXAction xAction )
        {
            try
            {
                DeviceBeginTransaction();

                foreach (I2cXActionUnit unit in xAction.XActionUnits)
                {
                    if (unit.IsRead)
                    {
                        DeviceRead( unit.Data );
                    }
                    else
                    {
                        DeviceWrite( unit.Data );
                    }
                }

                DeviceEndTransaction();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected virtual void DeviceRead( byte[] data )
        {
        }

        protected virtual void DeviceWrite( byte[] data )
        {
        }

        protected virtual void DeviceEndTransaction()
        {
        }

        protected virtual void DeviceBeginTransaction()
        {
        }
    }

    internal sealed class I2cDeviceNull : I2cDevice
    {
        public I2cDeviceNull( byte address )
        {
            _address = address;
        }

        internal override bool DeviceExecute( I2cXAction xAction )
        {
            return false;
        }

        private bool _displayWarning = true;

        internal bool DisplayWarning
        {
            get { return _displayWarning; }
        }

        internal void TurnOffWarning()
        {
            _displayWarning = false;
        }
    }
}
