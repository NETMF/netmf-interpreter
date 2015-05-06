using System;
using System.Runtime.CompilerServices;

namespace Windows.Devices.Gpio
{
    public sealed class GpioController
    {
        // Public properties

        extern public int PinCount
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
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
            return new GpioPin(pinNumber);
        }

        public bool TryOpenPin(int pinNumber, GpioSharingMode sharingMode, out GpioPin pin, out GpioOpenStatus openStatus)
        {
            pin = null;

            try
            {
                pin = OpenPin(pinNumber, sharingMode);
            }
            catch
            {
                // FUTURE: Catch only targeted exceptions.
                openStatus = GpioOpenStatus.PinUnavailable;
                return false;
            }

            openStatus = GpioOpenStatus.PinOpened;
            return true;
        }

        // Private fields

        private static GpioController s_instance = new GpioController();
    }
}
