////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using Microsoft.SPOT.Hardware;

using Configuration = Microsoft.SPOT.Hardware.Spi.Configuration;
using Hardware = Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Hardware
{
    public class Spi
    {
        public enum SpiModule: int
        {
            Spi1 = 0,
            Spi2 = 1,
            Spi3 = 2,
            Spi4 = 3,
        }

        public class Configuration
        {
            public readonly Cpu.Pin ChipSelect_Port;
            public readonly bool ChipSelect_ActiveState;
            public readonly uint ChipSelect_SetupTime;
            public readonly uint ChipSelect_HoldTime;
            public readonly bool Clock_IdleState;
            public readonly bool Clock_Edge;
            public readonly uint Clock_RateKHz;
            public readonly SpiModule SpiModule;
            public Configuration(
                                  Cpu.Pin ChipSelect_Port,
                                  bool ChipSelect_ActiveState,
                                  uint ChipSelect_SetupTime,
                                  uint ChipSelect_HoldTime,
                                  bool Clock_IdleState,
                                  bool Clock_Edge,
                                  uint Clock_RateKHz,
                                  SpiModule SpiModule 
                                )
            {
                this.ChipSelect_Port = ChipSelect_Port;
                this.ChipSelect_ActiveState = ChipSelect_ActiveState;
                this.ChipSelect_SetupTime = ChipSelect_SetupTime;
                this.ChipSelect_HoldTime = ChipSelect_HoldTime;
                this.Clock_IdleState = Clock_IdleState;
                this.Clock_Edge = Clock_Edge;
                this.Clock_RateKHz = Clock_RateKHz;
                this.SpiModule = SpiModule; 
            }
        }
    }
}

namespace Microsoft.SPOT.Emulator.Spi
{
    internal class SpiDriver : HalDriver<ISpiDriver>, ISpiDriver
    {
        SpiDevice _spiDeviceCurrent;

        private Configuration SpiConfigurationToConfiguration(SpiConfiguration configuration)
        {
            Configuration cfg = new Configuration(
                (Cpu.Pin)configuration.DeviceCS,
                configuration.CS_Active,
                configuration.CS_Setup_uSecs,
                configuration.CS_Hold_uSecs,
                configuration.MSK_IDLE,
                configuration.MSK_SampleEdge,
                configuration.Clock_RateKHz,
                (Hardware.Spi.SpiModule)configuration.SPI_mod);

            return cfg;
        }

        private bool IsInTransaction
        {
            get { return _spiDeviceCurrent != null; }
        }

        private bool StartTransaction(Configuration configuration)
        {            
            if (IsInTransaction)
            {
                Debug.Assert(false, "already in a SPI transaction" );
                Trace.WriteLine( "Error: Attempt to start a transaction when another one is in place." );
                return false;
            }

            if (configuration.ChipSelect_Port != Cpu.Pin.GPIO_NONE)
            {
                this.Emulator.ManagedHal.Gpio.SetPinState((uint)configuration.ChipSelect_Port, configuration.ChipSelect_ActiveState);
            }

            _spiDeviceCurrent = this.Emulator.SpiBus.GetActiveDevice();

            return true;
        }

        private bool EndTransaction(Configuration configuration)
        {
            SpiDevice spiDevice = this.Emulator.SpiBus.GetActiveDevice();

            if (configuration.ChipSelect_Port != Cpu.Pin.GPIO_NONE)
            {
                this.Emulator.ManagedHal.Gpio.SetPinState((uint)configuration.ChipSelect_Port, !configuration.ChipSelect_ActiveState);
            }

            if (!IsInTransaction)
            {
                Debug.Assert( false, "not in a SPI transaction" );
                Trace.WriteLine( "Error: Attempt to end a transaction when none is in place." );
                return false;
            }
            if (_spiDeviceCurrent != spiDevice)
            {
                Debug.Assert( false, "wrong SPI device" );
                Trace.WriteLine( "Error: Attempt to end a transaction, but the SPI device does not match." );
                return false;
            }

            _spiDeviceCurrent = null;

            return true;
        }

        #region ISpi Members

        bool ISpiDriver.Initialize()
        {
            return true;
        }

        void ISpiDriver.Uninitialize()
        {
        }

        bool ISpiDriver.nWrite16_nRead16(ref SpiConfiguration Configuration, IntPtr Write16, int WriteCount, IntPtr Read16, int ReadCount, int ReadStartOffset)
        {
            Configuration configuration = SpiConfigurationToConfiguration(Configuration);

            bool res = false;

            try
            {
                if(StartTransaction(configuration))
                {

                    SpiXaction xaction = new SpiXaction(Write16, WriteCount, Read16, ReadCount, ReadStartOffset);

                    res = ((ISpiDriver)this).Xaction_nWrite16_nRead16(ref xaction);
                }
            }
            finally
            {
                res &= EndTransaction(configuration);
            }

            return res;
        }

        bool ISpiDriver.nWrite8_nRead8(ref SpiConfiguration Configuration, IntPtr Write8, int WriteCount, IntPtr Read8, int ReadCount, int ReadStartOffset)
        {
            Configuration configuration = SpiConfigurationToConfiguration(Configuration);

            bool res = false;

            try
            {
                if(StartTransaction(configuration))
                {
                    SpiXaction xaction = new SpiXaction(Write8, WriteCount, Read8, ReadCount, ReadStartOffset);

                    res = ((ISpiDriver)this).Xaction_nWrite8_nRead8(ref xaction);
                 }
            }
            finally
            {
                res &= EndTransaction(configuration);
            }

            return res;
        }

        bool ISpiDriver.Xaction_Start(ref SpiConfiguration Configuration)
        {
            Configuration configuration = SpiConfigurationToConfiguration(Configuration);

            return StartTransaction(configuration);
        }

        bool ISpiDriver.Xaction_Stop(ref SpiConfiguration Configuration)
        {
            Configuration configuration = SpiConfigurationToConfiguration(Configuration);

            return EndTransaction(configuration);
        }

        bool ISpiDriver.Xaction_nWrite16_nRead16(ref SpiXaction Transaction)
        {
            if (Transaction.WriteCount <= 0)
            {
                Debug.Assert(false, "Transaction.WriteCount is 0.");
                Trace.WriteLine("Error: The write buffer in a SPI transaction cannot be empty.");
                return false;
            }
            if (Transaction.WritePtr == IntPtr.Zero)
            {
                Debug.Assert(false, "Transaction.write buffer is null.");
                Trace.WriteLine("Error: The write buffer in a SPI transaction cannot be null.");
                return false;
            }

            if ((Transaction.ReadCount >0) && (Transaction.ReadPtr == IntPtr.Zero))
            {
                Debug.Assert(false, "Transaction.read buffer is null.");
                Trace.WriteLine("Error: The read buffer in a SPI transaction cannot be null.");
                return false;              
            }

            ushort[] writeData;
                

            int length = Transaction.ReadCount + Transaction.ReadStartOffset;

            if (length < Transaction.WriteCount )
            {
                length = Transaction.WriteCount;
            }
            
            writeData = new ushort[length];


            // marshal the pointer to an array
            for (int i = 0; i < Transaction.WriteCount; i++)
            {
                writeData[i] = (ushort)Marshal.ReadInt16(Transaction.WritePtr, i * 2);
            }

            // repeat the last byte if neccessary to fill up writeData
            for (int i = Transaction.WriteCount; i < writeData.Length; i++)
            {
                writeData[i] = writeData[Transaction.WriteCount - 1];
            }

            ushort[] readData = _spiDeviceCurrent.DeviceWrite(writeData);

            if (Transaction.ReadPtr != IntPtr.Zero)
            {
                Utility.MarshalUshort(Transaction.ReadPtr, Transaction.ReadCount, Transaction.ReadStartOffset, readData);
            }

            return true;
        }

        bool ISpiDriver.Xaction_nWrite8_nRead8(ref SpiXaction Transaction)
        {
            if (Transaction.WriteCount <= 0)
            {
                Debug.Assert(false, "Transaction.WriteCount is 0.");
                Trace.WriteLine("Error: The write buffer in a SPI transaction cannot be empty.");
                return false;
            }
            if (Transaction.WritePtr == IntPtr.Zero)
            {
                Debug.Assert(false, "Transaction.write buffer is null.");
                Trace.WriteLine("Error: The write buffer in a SPI transaction cannot be null.");
                return false;
            }

            if ((Transaction.ReadCount >0) && (Transaction.ReadPtr == IntPtr.Zero))
            {
                Debug.Assert(false, "Transaction.read buffer is null.");
                Trace.WriteLine("Error: The read buffer in a SPI transaction cannot be null.");
                return false;              
            }


            byte[] writeData;

            int length = Transaction.ReadCount + Transaction.ReadStartOffset;

            if (length < Transaction.WriteCount )
            {
                length = Transaction.WriteCount;
            }

            writeData = new byte[length];

            
            Marshal.Copy(Transaction.WritePtr, writeData, 0, Transaction.WriteCount);

            // repeat the last byte if neccessary to fill up writeData
            for (int i = Transaction.WriteCount; i < writeData.Length; i++)
            {
                writeData[i] = writeData[Transaction.WriteCount - 1]; 
            }

            byte[] readData = _spiDeviceCurrent.DeviceWrite(writeData);


            if (Transaction.ReadPtr != IntPtr.Zero)
            {
                Utility.MarshalBytes(Transaction.ReadPtr, Transaction.ReadCount, Transaction.ReadStartOffset, readData);
            }

            return true;
        }

        uint ISpiDriver.GetPortsCount()
        {
            uint portCount = Emulator.SpiBus.GetPortsCount();

            return ((portCount > 0) ? portCount : 1);
        }
        
        void ISpiDriver.GetPins( uint spi_mod, out uint msk, out uint  miso, out uint mosi )
        {
            Emulator.SpiBus.GetPins(spi_mod, out msk, out miso, out mosi);
        }


        #endregion
    }

    public class SpiBus : EmulatorComponentCollection
    {
        List<SpiDevice> _devices;
        List<SpiDevice> _validDevices;
        SpiDeviceNull _nullDevice;

        public SpiBus()
            : base( typeof( SpiDevice ) )
        {
            _devices = new List<SpiDevice>();
            _validDevices = new List<SpiDevice>();
            _nullDevice = new SpiDeviceNull();
        }

        internal SpiDevice GetActiveDevice()
        {
            SpiDevice device = null;

            for (int i = 0; i < _devices.Count; i++)
            {
                SpiDevice deviceT = _devices[i];

                if (deviceT.IsActive)
                {
                    if (device != null)
                    {
                        if (_nullDevice.DisplayWarning || Emulator.Verbose)
                        {
                            Trace.WriteLine("Warning: multiple SpiDevices are active on the SpiBus");
                            _nullDevice.TurnOffWarning();
                        }
                    }

                    device = deviceT;
                }
            }

            if (device == null)
            {
                if (_nullDevice.DisplayWarning || Emulator.Verbose)
                {
                    Trace.WriteLine("Warning: no SpiDevice is active on the SpiBus");
                    _nullDevice.TurnOffWarning();
                }

                device = _nullDevice;
            }

            return device;
        }

        internal uint GetPortsCount()
        {
            return (uint)_devices.Count;
        }

        internal void GetPins( uint spi_mod, out uint msk, out uint  miso, out uint mosi )
        {
            for (int i = 0; i < _devices.Count; i++)
            {
                SpiDevice deviceT = _devices[i];
                if (deviceT._spiModule == (Microsoft.SPOT.Hardware.Spi.SpiModule)spi_mod)
                {
                    deviceT.GetPins(out msk, out miso, out mosi);
                    return;
                }
            }

            msk  = (uint) 0xffffffff;
            miso = (uint) 0xffffffff;
            mosi = (uint) 0xffffffff;
        }

        private SpiDevice LookupSpiDeviceFromPin(Cpu.Pin chipSelectPin)
        {
            if (chipSelectPin != Cpu.Pin.GPIO_NONE)
            {
                for (int i = 0; i < _devices.Count; i++)
                {
                    SpiDevice device = _devices[i];

                    if (device.ChipSelectPin == chipSelectPin)
                    {
                        return device;
                    }
                }
            }

            return null;
        }

        public SpiDevice this[Cpu.Pin chipSelectPin]
        {
            get
            {
                if (chipSelectPin == Cpu.Pin.GPIO_NONE)
                {
                    throw new ArgumentException("pin cannot be Cpu.Pin.GPIO_NONE");
                }

                SpiDevice device = LookupSpiDeviceFromPin(chipSelectPin);

                if (device == null)
                {
                    throw new ArgumentException("SpiDevice at pin " + chipSelectPin + " does not exist.");
                }

                return device;
            }
        }

        internal override void RegisterInternal( EmulatorComponent ec )
        {
            SpiDevice spiDevice = ec as SpiDevice;

            if ((spiDevice == null) || (spiDevice is SpiDeviceNull))
            {
                throw new Exception( "Attempt to register a non SpiDevice with SpiBus." );
            }

            if (LookupSpiDeviceFromPin(spiDevice.ChipSelectPin) != null)
            {
                throw new Exception(string.Format("SPI device already set up on pin {0}", spiDevice.ChipSelectPin));
            }

            _devices.Add(spiDevice);
            _validDevices.Add(spiDevice);

            base.RegisterInternal(ec);
        }

        internal override void UnregisterInternal(EmulatorComponent ec)
        {
            SpiDevice spiDevice = ec as SpiDevice;

            if (spiDevice != null)
            {
                _devices.Remove(spiDevice);
                _validDevices.Remove(spiDevice);

                base.UnregisterInternal(ec);
            }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return _validDevices.GetEnumerator();
        }

        public override int Count
        {
            get { return _validDevices.Count; }
        }

        public override void CopyTo(Array array, int index)
        {
            SpiDevice[] spiDeviceArray = array as SpiDevice[];
            if (spiDeviceArray == null)
            {
                throw new ArgumentException("Cannot cast array into SpiDevice[]");
            }
            _validDevices.CopyTo(spiDeviceArray, index);
        }
    }

    public abstract class SpiDevice : EmulatorComponent
    {
        private Cpu.Pin _chipSelectPin;
        private Gpio.GpioPort _gpioPort;
        protected bool _chipSelectActiveState;
        protected uint _chipSelectSetupTime;
        protected uint _chipSelectHoldTime;
        protected bool _clockIdleState;
        protected bool _clockEdge;
        protected uint _clockRateKHz;
        protected Cpu.Pin _msk = Cpu.Pin.GPIO_NONE;
        protected Cpu.Pin _miso = Cpu.Pin.GPIO_NONE;
        protected Cpu.Pin _mosi = Cpu.Pin.GPIO_NONE;
        internal Microsoft.SPOT.Hardware.Spi.SpiModule _spiModule;

        private bool HasValidChipSelectPin
        {
            get
            {
                return _chipSelectPin != Cpu.Pin.GPIO_NONE;
            }
        }

        public Cpu.Pin ChipSelectPin
        {
            get
            {
                return _chipSelectPin;
            }
            set
            {
                ThrowIfNotConfigurable();

                if (!this.HasValidChipSelectPin)
                {
                    _chipSelectPin = value;
                }
                else
                {
                    throw new Exception( "Pin can only be set once." ); 
                }
            }
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            if (ec is SpiDevice)
            {
                return (((SpiDevice)ec).ChipSelectPin == this.ChipSelectPin) && this.HasValidChipSelectPin;
            }
            else
            {
                return false;
            }
        }

        public override void InitializeComponent()
        {
            if (this.HasValidChipSelectPin)
            {
                if (!this.Emulator.GpioPorts.Exists(_chipSelectPin))
                {
                    Gpio.GpioPort port = new Microsoft.SPOT.Emulator.Gpio.GpioPort();
                    port.Pin = _chipSelectPin;
                    port.ModesAllowed = Microsoft.SPOT.Emulator.Gpio.GpioPortMode.InputOutputPort;
                    port.ModesExpected = Microsoft.SPOT.Emulator.Gpio.GpioPortMode.OutputPort;

                    Trace.WriteLine(string.Format("GPIO pin '{0}' was not configured.  SpiDevice is initializing it now", _chipSelectPin));

                    this.Emulator.RegisterComponent(port);
                    
                    Debug.Assert(port == this.Emulator.GpioPorts[_chipSelectPin]);
                }

                _gpioPort = this.Emulator.GpioPorts[_chipSelectPin];
            }

            base.InitializeComponent();
        }

        public SpiDevice()
            : this( Cpu.Pin.GPIO_NONE )
        {
        }

        public SpiDevice(Configuration configuation)
        {
            SetConfiguration( configuation );
        }

        public SpiDevice(Cpu.Pin chipSelectPin)
        {
            SetConfiguration( new Configuration( chipSelectPin, false, 0, 0, false, false, 13800, Microsoft.SPOT.Hardware.Spi.SpiModule.Spi1) );
        }

        public virtual bool IsActive
        {
            get
            {
                if (_gpioPort == null)
                    return false;

                return _gpioPort.Read() == _chipSelectActiveState;
            }
        }

        public Cpu.Pin Mask
        {
            get { return _msk; }
            set { ThrowIfNotConfigurable(); _msk = value; }
        }

        public Cpu.Pin Miso
        {
            get { return _miso; }
            set { ThrowIfNotConfigurable(); _miso = value; }
        }

        public Cpu.Pin Mosi
        {
            get { return _mosi; }
            set { ThrowIfNotConfigurable(); _mosi = value; }
        }

        internal void GetPins( out uint msk, out uint miso, out uint mosi)
        {
            msk = (uint)Mask;
            miso = (uint)Miso;
            mosi = (uint)Mosi;
        }

        private void SetConfiguration( Configuration configuration )
        {
            _chipSelectPin = configuration.ChipSelect_Port;
            _chipSelectActiveState = configuration.ChipSelect_ActiveState;
            _chipSelectSetupTime = configuration.ChipSelect_SetupTime;
            _chipSelectHoldTime = configuration.ChipSelect_HoldTime;
            _clockIdleState = configuration.Clock_IdleState;
            _clockEdge = configuration.Clock_Edge;
            _clockRateKHz = configuration.Clock_RateKHz;
            _spiModule = configuration.SpiModule;            
        }

        protected virtual ushort[] Write( ushort[] data )
        {
            byte[] writeData8 = new byte[data.Length * 2];

            for (int i = 0; i < data.Length; i++)
            {
                //this is a default implementation, currently choosing little-endian byte order
                //if useful, this could be a property of the SpiDevice

                ushort us = data[i];
                writeData8[i * 2] = (byte)(us & 0xff);
                writeData8[i * 2 + 1] = (byte)(us >> 8);
            }

            byte[] readData8 = DeviceWrite( writeData8 );

            if (readData8.Length != writeData8.Length)
            {
                Debug.Assert( false, "read data length not equal to write data length" );
                Trace.WriteLine( "Error: SpiDevice.DeviceWrite() does not return data of the same length." );
                return new ushort[data.Length];
            }

            ushort[] readData16 = new ushort[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                readData16[i] = (ushort)(readData8[i * 2] | (readData8[i * 2 + 1] << 8));
            }

            return readData16;
        }

        protected virtual byte[] Write( byte[] data )
        {
            Debug.Assert( false, "SpiDevice.Write() is not implemented." );
            Trace.WriteLine( "Warning: An unimplemented method, SpiDevice.Write() is called." );
            return new byte[data.Length];
        }

        #region SpiDevice abstract Members

        internal virtual ushort[] DeviceWrite( ushort[] data )
        {
            return Write( data );
        }

        internal virtual byte[] DeviceWrite( byte[] data )
        {
            return Write( data );
        }

        #endregion

        public bool ChipSelectActiveState
        {
            get { return _chipSelectActiveState; }
            set { _chipSelectActiveState = value; }
        }

        public uint ChipSelectSetupTime
        {
            get { return _chipSelectSetupTime; }
            set { _chipSelectSetupTime = value; }
        }

        public uint ChipSelectHoldTime
        {
            get { return _chipSelectHoldTime; }
            set { _chipSelectHoldTime = value; }
        }

        public bool ClockIdleState
        {
            get { return _clockIdleState; }
            set { _clockIdleState = value; }
        }

        public bool ClockEdge
        {
            get { return _clockEdge; }
            set { _clockEdge = value; }
        }

        public uint ClockRateKHz
        {
            get { return _clockRateKHz; }
            set { _clockRateKHz = value; }
        }


        public Microsoft.SPOT.Hardware.Spi.SpiModule SpiModule
        {
            get { return _spiModule; }
            set { _spiModule = value; }
        }
    }

    internal sealed class SpiDeviceNull : SpiDevice
    {
        public SpiDeviceNull()
        {
        }

        internal override ushort[] DeviceWrite( ushort[] data )
        {
            return new ushort[data.Length];
        }

        internal override byte[] DeviceWrite( byte[] data )
        {
            return new byte[data.Length];
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

#if SAMPLE_CODE_FOR_SPI
    public class Echo : SpiDevice
    {
        protected override byte[] Write(byte[] data)
        {
            return data;
        }

        public Echo(Cpu.Pin pin)
            : base(pin)
        {
        }
    }
#endif
}
