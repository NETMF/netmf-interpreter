using System;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;


using Microsoft.SPOT.Samples.XBeeRadio;



namespace Microsoft.SPOT.Samples.Sensors
{

    public class SensorAndRadioClient : IDisposable
    {
        private XBee _radio;
        private AD7314_TemperatureSensor _temperatureSensor;
        private CHS_UGS_HumiditySensor _humiditySensor;
        private Timer _TemperatureTimer;

        //--//

        public SensorAndRadioClient(XBee radio)
        {
            _temperatureSensor = new AD7314_TemperatureSensor();
            _humiditySensor = new CHS_UGS_HumiditySensor();

            _radio = radio;
        }

        public void Dispose()
        {
            Stop();
            _temperatureSensor.Dispose();
            _humiditySensor.Dispose();            
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
                temp1 = _temperatureSensor.ReadTemperature();
                temp2 = _temperatureSensor.ReadTemperature();
                temp3 = _temperatureSensor.ReadTemperature();
            } while (System.Math.Abs((int)(temp1 - temp2)) >= 1 || System.Math.Abs((int)(temp2 - temp3)) >= 1);

            temperature = (int)(((temp1 + temp2 + temp3) / 3) * 1.8) + 32; // Average and convert to Farenheit

            return temperature;
        }

        public int GetHumidity()
        {
            float humidity = _humiditySensor.ReadHumidity();

            int h = (int)humidity;

            return h;
        }

        private void TimerCallback(object o)
        {
            try
            {
                //turn on the Chip Select
                _radio.Powered = true;

                int temperature = GetTemperature();
                int humidity = GetHumidity();

                _radio.Send( new byte[] { (byte)temperature, (byte)humidity } );

                Debug.Print("(timer) T: " + temperature.ToString());
                Debug.Print("(timer) H: " + humidity.ToString()); 
            }
            finally
            {
                //turn  of the power to the radio
                _radio.Powered = false;
            }
        }
    }
}