using System;
using Microsoft.SPOT.Hardware;

namespace Windows.Devices.Gpio
{
    public sealed class GpioController
    {
        // Public properties

        public int PinCount
        {
            get
            {
                return HardwareProvider.HwProvider.GetPinsCount();
            }
        }

        // Public methods

        public static GpioController GetDefault()
        {
            return s_instance;
        }

        public GpioPin OpenPin(int pinNumber)
        {
            return OpenPin(pinNumber, GpioSharingMode.Exclusive);
        }

        public GpioPin OpenPin(int pinNumber, GpioSharingMode sharingMode)
        {
            GpioPin pin;
            GpioOpenStatus openStatus;
            if (!TryOpenPin(pinNumber, sharingMode, out pin, out openStatus))
            {
                // TODO: Is this the right exception?
                throw new InvalidOperationException("Could not open pin " + pinNumber);
            }

            return pin;
        }

        public bool TryOpenPin(int pinNumber, GpioSharingMode sharingMode, out GpioPin pin, out GpioOpenStatus openStatus)
        {
            if (sharingMode != GpioSharingMode.Exclusive)
            {
                throw new ArgumentException("Shared mode not available in .NET Micro Framework.", "sharingMode");
            }

            // TODO: Check whether the pin is available.
            pin = new GpioPin(pinNumber);
            openStatus = GpioOpenStatus.PinOpened;
            return true;
        }

        // Private fields

        private static GpioController s_instance = new GpioController();
    }
}
