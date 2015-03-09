////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace Microsoft.SPOT.Emulator.Battery
{
    internal class BatteryDriver : HalDriver<IBatteryDriver>, IBatteryDriver
    {
        BatteryCell _battery;

        private BatteryCell GetBattery()
        {
            if (_battery == null)
            {
                _battery = this.Emulator.Battery ?? new BatteryCellNull();
            }

            if (_battery is BatteryCellNull)
            {
                BatteryCellNull nullBattery = (BatteryCellNull)_battery;

                if (nullBattery.DisplayWarning || Emulator.Verbose)
                {
                    Trace.WriteLine( "Warning: System attempts to access the Battery when none is configured." );
                    nullBattery.TurnOffWarning();
                }
            }

            return _battery;
        }

        #region IBatteryDriver Members

        bool IBatteryDriver.Initialize()
        {
            return true;
        }

        bool IBatteryDriver.Uninitialize()
        {
            return true;
        }

        bool IBatteryDriver.Voltage(out int millivolts)
        {
            millivolts = GetBattery().DeviceVoltage;
            return true;
        }

        bool IBatteryDriver.Temperature(out int degreesCelcius_x10)
        {
            degreesCelcius_x10 = GetBattery().DeviceTemperature;
            return true;
        }

        bool IBatteryDriver.StateOfCharge(out byte stateOfCharge)
        {
            stateOfCharge = GetBattery().DeviceStateOfCharge;

            return true;
        }

        BatteryConfiguration IBatteryDriver.Configuration
        {
            get { return GetBattery().DeviceConfiguration; }
        }

        #endregion
    }

    public class BatteryCell : EmulatorComponent
    {
        int _voltage;
        int _temperature;
        byte _stateOfCharge;
        BatteryConfiguration _configuration;

        public BatteryCell()
        {
            _voltage = 0;
            _temperature = 0;
            _stateOfCharge = 0;

            _configuration.BatteryLifeMin = 10;
            _configuration.BatteryLifeLow = 20;
            _configuration.BatteryLifeMed = 30;
            _configuration.BatteryLifeMax = 98;

            _configuration.BatteryLifeFullMin = 60;
            _configuration.BatteryLifeHysteresis = 6;

            _configuration.TimeoutCharging = 30;
            _configuration.TimeoutCharged = 10;
            _configuration.TimeoutCharger = 5;
            _configuration.TimeoutBacklight = 5;
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            return (ec is BatteryCell);
        }

        internal int DeviceVoltage
        {
            get { return _voltage; }
        }

        internal int DeviceTemperature
        {
            get { return _temperature; }
        }

        internal byte DeviceStateOfCharge
        {
            get { return _stateOfCharge; }
        }

        internal BatteryConfiguration DeviceConfiguration
        {
            get { return _configuration; }
        }

        public int Voltage
        {
            get { return _voltage; }
            set { _voltage = value; }
        }

        public int Temperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }

        public byte StateOfCharge
        {
            get { return _stateOfCharge; }
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _stateOfCharge = value;
                }
                else
                {
                    throw new ArgumentException( "Error: State of charge can only be an integer between 0 and 100." );
                }
            }
        }

        public byte BatteryLifeMin
        {
            get { return _configuration.BatteryLifeMin; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.BatteryLifeMin = value;
            }
        }

        public byte BatteryLifeLow
        {
            get { return _configuration.BatteryLifeLow; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.BatteryLifeLow = value;
            }
        }

        public byte BatteryLifeMed
        {
            get { return _configuration.BatteryLifeMed; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.BatteryLifeMed = value;
            }
        }

        public byte BatteryLifeMax
        {
            get { return _configuration.BatteryLifeMax; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.BatteryLifeMax = value;
            }
        }

        public byte BatteryLifeFullMin
        {
            get { return _configuration.BatteryLifeFullMin; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.BatteryLifeFullMin = value;
            }
        }

        public byte BatteryLifeHysteresis
        {
            get { return _configuration.BatteryLifeHysteresis; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.BatteryLifeHysteresis = value;
            }
        }

        public byte TimeoutCharging
        {
            get { return _configuration.TimeoutCharging; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.TimeoutCharging = value;
            }
        }

        public byte TimeoutCharged
        {
            get { return _configuration.TimeoutCharged; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.TimeoutCharged = value;
            }
        }

        public ushort TimeoutCharger
        {
            get { return _configuration.TimeoutCharger; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.TimeoutCharger = value;
            }
        }

        public ushort TimeoutBacklight
        {
            get { return _configuration.TimeoutBacklight; }
            set
            {
                ThrowIfNotConfigurable();
                _configuration.TimeoutBacklight = value;
            }
        }
    }

    internal sealed class BatteryCellNull : BatteryCell
    {
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
