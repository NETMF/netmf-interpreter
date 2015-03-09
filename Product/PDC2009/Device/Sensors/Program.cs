using System;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using Microsoft.SPOT.Samples.XBeeRadio;

//--//

namespace Microsoft.SPOT.Samples.Sensors

{
    public class Program
    {
        private static byte[] c_Read_Sensors_Message = new byte[] { (byte)'T', (byte)'E', (byte)'M', (byte)'P', (byte)13 };

        private static readonly byte[] c_Self_Address   = new byte[] { (byte)'1', (byte)'1' };
        private static readonly byte[] c_Target_Address = new byte[] { (byte)'1', (byte)'0' };

        //--//

        static AutoResetEvent _exit = new AutoResetEvent( false );

        //--//

        public static void Main()
        {
            SensorAndRadioClient sensorClient = null; 
            SensorAndRadioServer radioServer = null;

            XBee radio = new XBee(Serial.COM2, 9600, c_Self_Address, c_Target_Address, false, (Cpu.Pin)36);
            
            try
            {

                sensorClient = new SensorAndRadioClient( radio );
                radioServer = new SensorAndRadioServer( radio, c_Read_Sensors_Message, sensorClient );

                sensorClient.Start( 5000 ); // 10 secs
                radioServer.Start();

                _exit.WaitOne();
            }
            finally
            {
                radioServer.Dispose();
                sensorClient.Dispose();
            }
        }
        
        public static void Exit()
        {
            _exit.Set();
        }
    }
}
