////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.SPOT.Hardware
{
    public static class Battery
    {
        public sealed class ChargerModel
        {
            public readonly int Charge_Min;
            public readonly int Charge_Low;
            public readonly int Charge_Medium;
            public readonly int Charge_Full;
            public readonly int Charge_FullMin;
            public readonly int Charge_Hysteresis;

            public readonly TimeSpan Timeout_Charging;  // How long to wait to say "Charged" in case the charger doesn't signal us.
            public readonly TimeSpan Timeout_Charged;   // How long to wait to say "Charged" in case the charger doesn't signal us at 98%.
            public readonly TimeSpan Timeout_Charger;   // How long to persist state after the watch is taken off the charger.
            public readonly TimeSpan Timeout_Backlight; // Lenght of the backlight pulse when the watch is put on the charger.

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            extern internal ChargerModel();
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public int ReadVoltage();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public int ReadTemperature();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool OnCharger();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool IsFullyCharged();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public int StateOfCharge();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool WaitForEvent(int timeout);
        static public ChargerModel GetChargerModel()
        {
            return new ChargerModel();
        }
    }
}


