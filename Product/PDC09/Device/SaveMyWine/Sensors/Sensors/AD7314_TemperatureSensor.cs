using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;


namespace Microsoft.SPOT.Samples.Sensors
{
    class AD7314_TemperatureSensor : IDisposable
    {
        /// <summary>
        /// Keep this private member around as the SPI object
        /// </summary>
        private SPI _myThermostat;
        private byte[] _bout;
        private byte[] _bin;

        /// <summary>
        /// Standard public constructor
        /// </summary>
        public AD7314_TemperatureSensor()
        {
            // Get a new SPI object that is connected to the temperature sensor
            _myThermostat = new SPI(new SPI.Configuration((Cpu.Pin)43, true, 0, 0, false, false, 4000, SPI.SPI_module.SPI1));

            // Create the output and input arrays of bytes
            _bout = new byte[2];
            _bin = new byte[2];

            // Set the output bytes both to 0
            // This sensor doesn't need any commands. All this method does is read
            // the input data, so the output is simply for SPI clocking
            _bout[0] = 0;
            _bout[1] = 0;
        }

        public void Dispose()
        {
            _myThermostat.Dispose();
        }

        /// <summary>
        /// This method reads the temperature from the sensor
        /// </summary>
        /// <returns>Returns a float representation of the temperature</returns>
        public float ReadTemperature()
        {
            // Write the clocking info, then read the input data
            lock (typeof(SPI))
            {
                _myThermostat.WriteRead(_bout, _bin);
            }

            // Convert the 2 byte value according to the AD7314 data sheet

            // Put the raw bytes into an int
            // Mask the upper bit, which is a sign indicator (+/-)
            int s = (((_bin[0] << 8) + _bin[1]) & 0xBFFF);

            // Convert to a float
            float f = s;

            // Shift right
            f /= 128;

            // Detect negative
            if ((_bin[0] & 0x40) == 0x40)
                f = -f;

            // Return the temperature
            return f;
        }
    }
}
