
using System;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;


using Microsoft.SPOT.Samples.XBeeRadio;

namespace Microsoft.SPOT.Samples.Sensors

{
    public class XBeeRadioSensorsClient
    {
        private XBee _XBeeport;
        private TemperatureSensor _sensor;
        private byte[] _requestMessage;

        //--//
        
        public XBeeRadioSensorsClient( XBee XBeeport, byte[] requestMessage)
        {
            _XBeeport = XBeeport;
            _requestMessage = requestMessage;

            _sensor = new TemperatureSensor(_XBeeport);

            _XBeeport.NotifyReading += new TemperatureReadingCallback(OnRadioActivity);
        }

        public void Start( int timeoutMs )
        {
            _sensor.Start(timeoutMs);
        }

        public void Stop()
        {
            _sensor.Stop();
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
            if (request == null || request.Length != _requestMessage.Length) return;

            for(int i = 0; i < _requestMessage.Length; ++i)
            {
                if(request[i] != _requestMessage[i]) return;
            }

            int temperature = _sensor.GetTemperature();
            _XBeeport.Send( new byte[] { (byte)temperature } );
        }
    }
}
