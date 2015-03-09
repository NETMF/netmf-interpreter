
using System;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;


using Microsoft.SPOT.Samples.XBeeRadio;

namespace Microsoft.SPOT.Samples.Sensors

{
    public class SensorAndRadioServer : IDisposable
    {
        private XBee _XBeeport;
        private byte[] _requestMessage;
        SensorAndRadioClient _sensor;

        //--//
        
        public SensorAndRadioServer( XBee XBeeport, byte[] requestMessage, SensorAndRadioClient sensor)
        {
            _XBeeport = XBeeport;
            _sensor = sensor;
            _requestMessage = requestMessage;

        }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            finally
            {
                _XBeeport.Dispose();
            }
        }

        public void Start()
        {
            _XBeeport.NotifyReading += new TemperatureReadingCallback(OnRadioActivity);
        }

        public void Stop()
        {
            _XBeeport.NotifyReading -= new TemperatureReadingCallback(OnRadioActivity);
        }

        //--//
        
        private void OnRadioActivity( XBee radio )
        {
            byte[] buffer = radio.Receive();

            if (buffer != null)
            {
                ProcessRadioActivity(buffer);
            }
        }

        private void ProcessRadioActivity( byte[] request )
        {
            Debug.Print("processing request");

            if (request == null || request.Length != _requestMessage.Length) return;

            for(int i = 0; i < _requestMessage.Length; ++i)
            {
                if(request[i] != _requestMessage[i]) return;
            }

            int temperature = _sensor.GetTemperature();
            int humidity = _sensor.GetHumidity();

            Debug.Print("(request) T: " + temperature.ToString());
            Debug.Print("(request) H: " + humidity.ToString()); 

            _XBeeport.Send( new byte[] { (byte)temperature, (byte)humidity } );

        }
    }
}
