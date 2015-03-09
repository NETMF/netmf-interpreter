using System;
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;




namespace Microsoft.SPOT.Samples.Sensors
{
    public class I2CTransactionException : Exception
    {
    }
    public class ConversionException : I2CTransactionException
    {
    }

    /// <summary>
    /// This class provides the interface to the AD7993 ADC converter
    /// from Analog Devices
    /// </summary>
    public class AD7993_ADC : IDisposable
    {
        public enum Address : ushort
        {
            Address_Variation_0_AS_Ground = 0x0021,
            Address_Variation_0_AS_VDD = 0x0022,
            Address_Variation_1_AS_Ground = 0x0023,
            Address_Variation_1_AS_VDD = 0x0024,
            Address_Variation_1_AS_Float = 0x0020,
        }

        public enum ClockRateKhz
        {
            Rate_100Khz = 100,
            RAte_400Khz = 400,
        }

        public enum Channel : byte
        {
            Channel_1 = 0x01,
            Channel_2 = 0x02,
            Channel_3 = 0x03,
            Channel_4 = 0x04,
        }

        //--//

        static readonly bool c_ConversionStartActiveState = false;

        I2CDevice m_bus;
        OutputPort m_conversionStart;
        AD7993_ADC_Internal m_device;

        //--//

        #region constructors/destructors
        public AD7993_ADC( ushort address, ClockRateKhz i2cBusBlockRate)
            : this(address, i2cBusBlockRate, Channel.Channel_1, Cpu.Pin.GPIO_NONE)
        {
        }

        public AD7993_ADC( ushort address, ClockRateKhz i2cBusBlockRate, Channel channel)
            : this(address, i2cBusBlockRate, Channel.Channel_1, Cpu.Pin.GPIO_NONE)
        {
        }

        public AD7993_ADC(ushort address, ClockRateKhz i2cBusBlockRate, Channel channel, Cpu.Pin conversionStartPin)
        {
            m_bus = new I2CDevice(new I2CDevice.Configuration(address, (int)i2cBusBlockRate));

            if (conversionStartPin != Cpu.Pin.GPIO_NONE)
            {
                m_conversionStart = new OutputPort(conversionStartPin, c_ConversionStartActiveState);
            }

            m_device = new AD7993_ADC_Internal(channel, m_bus);
        }

        public void Dispose()
        {
            m_conversionStart.Dispose();
            m_bus.Dispose();
        }

        #endregion

        #region public interface

        [MethodImpl(MethodImplOptions.Synchronized)]
        public float Acquire()
        {
            m_conversionStart.Write(!c_ConversionStartActiveState);
            Thread.Sleep(1); // plenty of time
            m_conversionStart.Write(c_ConversionStartActiveState);

            float conversionValue = m_device.Acquire(m_bus);

            return conversionValue;
        }


        #endregion

        #region internal implementation

        private class AD7993_ADC_Internal
        {
            enum RegisterAddress : byte
            {
                ConversionResult = 0x00,
                AlertStatus = 0x01,
                Configuration = 0x02,
                CycleTimer = 0x03,
                DataLow_Channel_1 = 0x04,
                DataHigh_Channel_1 = 0x05,
                Hysteresis_Channel_1 = 0x06,
                DataLow_Channel_2 = 0x07,
                DataHigh_Channel_2 = 0x08,
                Hysteresis_Channel_2 = 0x09,
                DataLow_Channel_3 = 0x0A,
                DataHigh_Channel_3 = 0x0B,
                Hysteresis_Channel_3 = 0x0C,
                DataLow_Channel_4 = 0x0D,
                DataHigh_Channel_4 = 0x0E,
                Hysteresis_Channel_4 = 0x0F,
            }

            //--//

            static readonly byte c_Configuration_Filter_No_Alert = 0x08;

            //--//

            Channel _activeChannel;
            byte[] _data;

            //--//

            public AD7993_ADC_Internal(Channel channel, I2CDevice bus)
            {
                _data = new byte[] { (byte)0x00, (byte)0x00 };
                
                // configure the control register
                Write(new byte[] { (byte)RegisterAddress.Configuration, (byte)((byte)((byte)channel << 4) | c_Configuration_Filter_No_Alert) }, bus);
                Read(_data, bus);

                _activeChannel = channel;
            }

            public int Acquire(I2CDevice bus)
            {
                Write(new byte[] { (byte)RegisterAddress.ConversionResult }, bus);
                Read(_data, bus);

                int result = ((_data[0] & 0x07) << 8) + _data[1];

                return result;
            }

            //--//

            private void Write(byte[] data, I2CDevice bus)
            {
                I2CDevice.I2CWriteTransaction xaction = bus.CreateWriteTransaction(data);

                int count = bus.Execute(new I2CDevice.I2CTransaction[] { xaction }, 1000);

                if (count != data.Length)
                {
                    throw new I2CTransactionException();
                }
            }
            private void Read(byte[] data, I2CDevice bus)
            {
                I2CDevice.I2CReadTransaction xaction = bus.CreateReadTransaction(data);

                int count = bus.Execute(new I2CDevice.I2CTransaction[] { xaction }, 1000);

                if (count != data.Length)
                {
                    throw new I2CTransactionException();
                }
            }
        }



        #endregion
    }
}