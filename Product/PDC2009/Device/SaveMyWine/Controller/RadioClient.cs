//#define RADIO_SIMULATION

//--//

using System;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Hardware;

//using ChipworkX.Hardware;

using Microsoft.SPOT.Samples.XBeeRadio;



namespace Microsoft.SPOT.Samples.SaveMyWine
{
    
    //--//
    public class RadioClient : IDisposable
    {
        private static RadioClient _radio;

        //--//
        
        private static readonly byte[] c_GetTemperature_Command = new byte[] { (byte)'T', (byte)'E', (byte)'M', (byte)'P', (byte)13 };
        
        private static readonly byte[] c_Self_Address   = new byte[] { (byte)'1', (byte)'0' };
        private static readonly byte[] c_Target_Address = new byte[] { (byte)'1', (byte)'1' };

        //--//


#if RADIO_SIMULATION
        private XBeeSimulation _XBeeport;
        private byte[] _temperatureCurve = { (byte)50, (byte)51, (byte)52, (byte)53, (byte)54, (byte)55, (byte)56, (byte)58, (byte)59, (byte)60 };
        private byte[] _humidityCurve = { (byte)60, (byte)61, (byte)62, (byte)63, (byte)64, (byte)65, (byte)66, (byte)68, (byte)69, (byte)70 };
        private int _temperatureIndex = 0;
        private int _humidityIndex = 0;
#else
        private XBee _XBeeport;
#endif
        private Thread _worker;
        private AutoResetEvent _pollingEvent;
        private bool _running;
        private bool _polling;
        private WineController _controller;

        //--//

        const int c_PollingTime = 60 * 1000; // 1 minute in milliseconds

        //--//

        public static RadioClient GetRadio(WineController controller)
        {
            lock (typeof(RadioClient))
            {
                if(_radio == null)
                {
                    _radio = new RadioClient(controller, false);
                }
            }

            return _radio;
        }

        protected RadioClient(WineController controller, bool polling)
        {
            try
            {
                _polling = polling;
                _controller = controller;

#if RADIO_SIMULATION
                _XBeeport = new XBeeSimulation("COM2", 9600, c_Self_Address, c_Target_Address, true, Cpu.Pin.GPIO_NONE);
#else
                _XBeeport = new XBee("COM2", 9600, c_Self_Address, c_Target_Address, true, Cpu.Pin.GPIO_NONE);
#endif
                
                if (polling)
                {
                    _pollingEvent = new AutoResetEvent(false);

                    _running = true;
                    _worker = new Thread(new ThreadStart(ThreadProcedure));
                    _worker.Start();
                }
                else
                {
                    _XBeeport.NotifyReading += TemperatureReading;
                }

            }
            catch
            {
                if(_XBeeport != null)
                {
                    _XBeeport.Dispose();
                }

                throw;
            }
        }

        ~RadioClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Update()
        {
            _XBeeport.Send( c_GetTemperature_Command );
        }

        private void Dispose(bool disposing)
        {
            if (_polling)
            {
                _running = false;
                _pollingEvent.Set();
            }

            _XBeeport.Dispose();

            if (_polling) _worker.Join();
        }

        private void ThreadProcedure()
        {
            while (_running)
            {
                double temperature = GetTemperature( _XBeeport.Receive() );

                // Create the timer that will check the current temperature
                _controller.UpdateSensorData(temperature, -1);

                _pollingEvent.WaitOne(c_PollingTime, false);
            }
        }

        private double GetTemperature(byte[] buffer)
        {
            double temperature = -1;

#if RADIO_SIMULATION
            temperature = _temperatureCurve[_temperatureIndex];

            if (++_temperatureIndex >= _temperatureCurve.Length) _temperatureIndex = 0;
#else
            if (buffer != null && buffer.Length >= 1)
            {
                temperature = (double)buffer[0];
            }
#endif
            return temperature;
        }

        private double GetHumidity(byte[] buffer)
        {
            double humidity = -1;

#if RADIO_SIMULATION
            humidity = _humidityCurve[_humidityIndex];

            if (++_humidityIndex >= _humidityCurve.Length) _humidityIndex = 0;     
#else
            if (buffer != null && buffer.Length >= 2)
            {
                humidity = (double)buffer[1];
            }
#endif
            return humidity;
        }


#if RADIO_SIMULATION
        private void TemperatureReading(XBeeSimulation radio)
        {
            byte[] data = _XBeeport.Receive();

            double temperature = GetTemperature(data);
            double humidity = GetHumidity(data);


            _controller.UpdateSensorData(temperature, humidity);
        }
#else
        private void TemperatureReading( XBee radio )
        {
            byte[] data = _XBeeport.Receive();

            double temperature = GetTemperature(data);
            double humidity = GetHumidity(data);

            _controller.UpdateSensorData(temperature, humidity);
        }
#endif
    }
}