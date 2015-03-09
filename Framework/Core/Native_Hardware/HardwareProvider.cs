using System;
using System.Collections;
using System.Threading;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware
{
    public class HardwareProvider
    {
        private static HardwareProvider s_hwProvider = null;

        //--//

        public static void Register(HardwareProvider provider)
        {
            s_hwProvider = provider;

        }

        //--//

        public static HardwareProvider HwProvider
        {
            get
            {
                if (s_hwProvider == null)
                {
                    s_hwProvider = new HardwareProvider();
                }

                return s_hwProvider;
            }
        }

        //--//

        public virtual void GetSerialPins(string comPort, out Cpu.Pin rxPin, out Cpu.Pin txPin, out Cpu.Pin ctsPin, out Cpu.Pin rtsPin)
        {
            int comIdx = System.IO.Ports.SerialPortName.ConvertNameToIndex(comPort);

            rxPin = Cpu.Pin.GPIO_NONE;
            txPin = Cpu.Pin.GPIO_NONE;
            ctsPin = Cpu.Pin.GPIO_NONE;
            rtsPin = Cpu.Pin.GPIO_NONE;

            NativeGetSerialPins(comIdx, ref rxPin, ref txPin, ref ctsPin, ref rtsPin);
        }

        public virtual int GetSerialPortsCount()
        {

            return NativeGetSerialPortsCount();
        }

        public virtual bool SupportsNonStandardBaudRate(int com)
        {
            return NativeSupportsNonStandardBaudRate(com);
        }

        public virtual void GetBaudRateBoundary(int com, out uint MaxBaudRate, out uint MinBaudRate)
        {
            NativeGetBaudRateBoundary(com, out MaxBaudRate, out MinBaudRate);
        }

        public virtual bool IsSupportedBaudRate(int com, ref uint baudrateHz)
        {
            return NativeIsSupportedBaudRate(com, ref baudrateHz);
        }

        public virtual void GetSupportBaudRates(int com, out System.IO.Ports.BaudRate[] StdBaudRate, out int size)
        {
            uint rBaudrate = 0;
            uint[] baudrateSet = new uint[]  { 75,
                                               150,
                                               300,
                                               600,
                                               1200,
                                               2400,
                                               4800,
                                               9600,
                                               19200,
                                               38400,
                                               57600,
                                               115200,
                                               230400,
                                              };

            StdBaudRate = new System.IO.Ports.BaudRate[13] {    System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                                System.IO.Ports.BaudRate.BaudrateNONE,
                                                            };

            size = 0;
            for (int i = 0; i < baudrateSet.Length; i++)
            {
                rBaudrate = baudrateSet[i];
                if (IsSupportedBaudRate(com, ref rBaudrate))
                {
                    StdBaudRate[size] = (System.IO.Ports.BaudRate)rBaudrate;
                    size++;
                }
            }

        }

        //--//
        public virtual void GetSpiPins(SPI.SPI_module spi_mod, out Cpu.Pin msk, out Cpu.Pin miso, out Cpu.Pin mosi)
        {
            msk = Cpu.Pin.GPIO_NONE;
            miso = Cpu.Pin.GPIO_NONE;
            mosi = Cpu.Pin.GPIO_NONE;

            NativeGetSpiPins(spi_mod, out msk, out miso, out mosi);
        }

        public virtual int GetSpiPortsCount()
        {

            return NativeGetSpiPortsCount();
        }

        //--//
        public virtual void GetI2CPins(out Cpu.Pin scl, out Cpu.Pin sda)
        {
            scl = Cpu.Pin.GPIO_NONE;
            sda = Cpu.Pin.GPIO_NONE;

            NativeGetI2CPins(out scl, out sda);
        }


        public virtual int GetPWMChannelsCount()
        {
            return NativeGetPWMChannelsCount();
        }

        public virtual Cpu.Pin GetPwmPinForChannel(Cpu.PWMChannel channel)
        {
            return NativeGetPWMPinForChannel(channel);
        }

        //--//

        public virtual int GetAnalogChannelsCount()
        {
            return NativeGetAnalogChannelsCount();
        }

        public virtual Cpu.Pin GetAnalogPinForChannel(Cpu.AnalogChannel channel)
        {
            return NativeGetAnalogPinForChannel(channel);
        }

        public virtual int[] GetAvailablePrecisionInBitsForChannel(Cpu.AnalogChannel channel)
        {
            return NativeGetAvailablePrecisionInBitsForChannel(channel);
        }

        //--//

        public virtual int GetAnalogOutputChannelsCount()
        {
            return NativeGetAnalogOutputChannelsCount();
        }

        public virtual Cpu.Pin GetAnalogOutputPinForChannel(Cpu.AnalogOutputChannel channel)
        {
            return NativeGetAnalogOutputPinForChannel(channel);
        }

        public virtual int[] GetAvailableAnalogOutputPrecisionInBitsForChannel(Cpu.AnalogOutputChannel channel)
        {
            return NativeGetAvailableAnalogOutputPrecisionInBitsForChannel(channel);
        }

        //--//
        
        public virtual int GetPinsCount()
        {
            return NativeGetPinsCount();
        }

        public virtual void GetPinsMap(out Cpu.PinUsage[] pins, out int PinCount)
        {

            PinCount = GetPinsCount();

            pins = new Cpu.PinUsage[PinCount];

            NativeGetPinsMap(pins);
        }

        public virtual Cpu.PinUsage GetPinsUsage(Cpu.Pin pin)
        {
            return NativeGetPinUsage(pin);
        }

        public virtual Cpu.PinValidResistorMode GetSupportedResistorModes(Cpu.Pin pin)
        {
            return NativeGetSupportedResistorModes(pin);
        }

        public virtual Cpu.PinValidInterruptMode GetSupportedInterruptModes(Cpu.Pin pin)
        {
            return NativeGetSupportedInterruptModes(pin);
        }

        //--//

        public virtual Cpu.Pin GetButtonPins(Button iButton)
        {
            return NativeGetButtonPins(iButton);
        }

        //--//
        public virtual void GetLCDMetrics(out int width, out int height, out int bitsPerPixel, out int orientationDeg)
        {
            NativeGetLCDMetrics(out height, out width, out bitsPerPixel, out orientationDeg);
        }

        //---//

        //---// native calls

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeGetSerialPins(int com, ref Cpu.Pin rxPin, ref Cpu.Pin txPin, ref Cpu.Pin ctsPin, ref Cpu.Pin rtsPin);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeGetSerialPortsCount();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private bool NativeSupportsNonStandardBaudRate(int com);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeGetBaudRateBoundary(int com, out uint MaxBaudRate, out uint MinBaudRate);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private bool NativeIsSupportedBaudRate(int com, ref uint baudrateHz);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeGetSpiPins(SPI.SPI_module spi_mod, out Cpu.Pin msk, out Cpu.Pin miso, out Cpu.Pin mosi);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeGetSpiPortsCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeGetI2CPins(out Cpu.Pin scl, out Cpu.Pin sda);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeGetPinsCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeGetPinsMap(Cpu.PinUsage[] pins);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private Cpu.PinUsage NativeGetPinUsage(Cpu.Pin pin);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private Cpu.PinValidResistorMode NativeGetSupportedResistorModes(Cpu.Pin pin);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private Cpu.PinValidInterruptMode NativeGetSupportedInterruptModes(Cpu.Pin pin);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private Cpu.Pin NativeGetButtonPins(Button iButton);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeGetLCDMetrics(out int height, out int width, out int bitPerPixel, out int orientationDeg);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeGetPWMChannelsCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private Cpu.Pin NativeGetPWMPinForChannel(Cpu.PWMChannel channel);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeGetAnalogChannelsCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private Cpu.Pin NativeGetAnalogPinForChannel(Cpu.AnalogChannel channel);
        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int[] NativeGetAvailablePrecisionInBitsForChannel(Cpu.AnalogChannel channel);
        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeGetAnalogOutputChannelsCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private Cpu.Pin NativeGetAnalogOutputPinForChannel(Cpu.AnalogOutputChannel channel);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int[] NativeGetAvailableAnalogOutputPrecisionInBitsForChannel(Cpu.AnalogOutputChannel channel);
    }
}


