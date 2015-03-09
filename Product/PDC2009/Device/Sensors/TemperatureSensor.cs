using System;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;


using Microsoft.SPOT.Samples.XBeeRadio;



namespace Microsoft.SPOT.Samples.Sensors
{

    public class TemperatureSensor : IDisposable
    {
        XBee _radio;
        private SpiThermostat _sensor;
        private Timer _TemperatureTimer;

        //--//

        public TemperatureSensor(XBee radio)
        {
            _sensor = new SpiThermostat();

            _radio = radio;
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start(int samplingTime)
        {
            _TemperatureTimer = new Timer(TimerCallback, null, 10, samplingTime);
        }

        public void Stop()
        {
            _TemperatureTimer.Dispose();
        }

        public int GetTemperature()
        {
            //read 3 and see if we have consistent readings - there are occasional outliers
            int temperature;
            float temp1, temp2, temp3;
            // Read the value from the sensor

            do
            {
                temp1 = _sensor.ReadTemperature();
                temp2 = _sensor.ReadTemperature();
                temp3 = _sensor.ReadTemperature();
            } while (System.Math.Abs((int)(temp1 - temp2)) >= 1 || System.Math.Abs((int)(temp2 - temp3)) >= 1);

            temperature = (int)(((temp1 + temp2 + temp3) / 3) * 1.8) + 32; // Average and convert to Farenheit

            return temperature;
        }

        private void TimerCallback(object o)
        {
            try
            {

                //turn on the Chip Select
                _radio.Powered = true;

                int temperature = GetTemperature();

                _radio.Send( new byte[] { (byte)temperature } );

                Debug.Print(temperature.ToString()); 
            }
            finally
            {
                //turn  of the power to the radio
                _radio.Powered = false;
            }
        }
    }
}