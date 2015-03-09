////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;
namespace Microsoft.SPOT.Emulator.Battery
{
    [StructLayout( LayoutKind.Sequential )]
    public struct BatteryConfiguration
    {
        public byte BatteryLifeMin;
        public byte BatteryLifeLow;
        public byte BatteryLifeMed;
        public byte BatteryLifeMax;

        public byte BatteryLifeFullMin;
        public byte BatteryLifeHysteresis;

        public byte TimeoutCharging;  // Maximum time to spend at a certain charge level before displaying the "Charged" popup.
        public byte TimeoutCharged;   // Delay after the battery reaches MAX before displaying the "Charged" popup.
        public ushort TimeoutCharger;   // Minimum time to ignore glitches on the Charger status.
        public ushort TimeoutBacklight; // Time to leave backlight on when placed on the charger.
    }

    public interface IBatteryDriver
    {
        bool Initialize();
        bool Uninitialize();
        bool Voltage(out int millivolts);
        bool Temperature(out int degreesCelcius_x10);
        bool StateOfCharge(out byte stateOfCharge);

        BatteryConfiguration Configuration { get; }
    }
}
