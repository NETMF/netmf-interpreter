using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using Microsoft.SPOT.Samples.XBeeRadio;

//--//


namespace Microsoft.SPOT.Samples.Sensors
{
    class CHS_UGS_HumiditySensor : IDisposable
    {   
        private AD7993_ADC _adc;

        /// <summary>
        /// Standard public constructor
        /// </summary>
        public CHS_UGS_HumiditySensor()
        {
            // Get a new SPI object that is connected to the temperature sensor
            _adc = new AD7993_ADC( (ushort)AD7993_ADC.Address.Address_Variation_0_AS_Ground, AD7993_ADC.ClockRateKhz.Rate_100Khz, AD7993_ADC.Channel.Channel_1, (Cpu.Pin)14);
        }
        
        public void Dispose()
        {
            _adc.Dispose();
        }

        /// <summary>
        /// This method reads the temperature from the sensor
        /// </summary>
        /// <returns>Returns a float representation of the temperature</returns>
        public float ReadHumidity()
        {
            // Write the clocking info, then read the input data
            float raw = _adc.Acquire();

            // we will return a percentage (i.e. 44 for 44%)    
            return raw / 10;    
        }
    }
}
