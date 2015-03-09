using System;
using System.Runtime.CompilerServices;
using System.IO.Ports;
using Microsoft.SPOT;
using System.Threading;

using Microsoft.SPOT.Hardware;


namespace Microsoft.SPOT.Samples.XBeeRadio
{
    public delegate void TemperatureReadingCallback( XBee radio );

    //--//

    public class XBee : IDisposable
    {
        public class RadioException : Exception
        {
        }

        //--//

        static readonly int c_powerOnTime = 100; // ms

        //--//

        static readonly byte[] c_InitMessage = new byte[] { (byte)'+', (byte)'+', (byte)'+' };
        static readonly byte[] c_BaudRate = new byte[] { (byte)'A', (byte)'T', (byte)'B', (byte)'D', (byte)13 };
        static readonly byte[] c_GetIDMessage = new byte[] { (byte)'A', (byte)'T', (byte)'I', (byte)'D', (byte)13 };
        static readonly byte[] c_EndMessage = new byte[] { (byte)'A', (byte)'T', (byte)'C', (byte)'N', (byte)13 };

        //--//

        bool _verbose = true;

        //--//

        SerialPort _serial;
        OutputPort _power;
        TemperatureReadingCallback _subscribers;

        //--//

        public XBee(string port, int baud, byte[] self, byte[] target, bool coordinator, Cpu.Pin powerPin )
        {
            int bytesRead;
            int bytesSent;
            byte[] readBuffer = new byte[32];


            if (self.Length != 2 || target.Length != 2)
            {
                throw new ArgumentException();
            }

            _serial = new SerialPort( port, baud );

            if (powerPin != Cpu.Pin.GPIO_NONE) _power = new OutputPort(powerPin, false);
            Powered = true;

            
            _serial.Open();

            //Send out an 'END' message to terminate any previous command sessions left open
            bytesSent = _serial.Write(c_EndMessage, 0, c_EndMessage.Length);
            Thread.Sleep(1000);
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);

            Thread.Sleep(1000);
            bytesSent = _serial.Write(c_InitMessage, 0, c_InitMessage.Length);
            Thread.Sleep(3000);
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);

            // we will take special care of checking that the init message actually went through 
            byte[] initMessage = new byte[ bytesRead ];
            Array.Copy( readBuffer, initMessage, bytesRead );

            string message = new string( System.Text.UTF8Encoding.UTF8.GetChars( initMessage ) );

            if (message.IndexOf(new string(new char[] { (char)79, (char)75, (char)13 } ), 0, message.Length) == -1)
            {
                throw new RadioException();
            }

            //setup the XBee 
            XBeeRestore();
            XBeeConfigure(self, target, coordinator);

            //while (!XBeeNodeDetect())
            //{
            //   if(_verbose)
            //            Debug.Print("No Node found");
            //    Thread.Sleep(1000);
            //}
            //Debug.Print("Node Detected");


            Thread.Sleep(1000);
            bytesSent = _serial.Write(c_EndMessage, 0, c_EndMessage.Length);


            Thread.Sleep(1000);
            Debug.Print("Return from End Message");
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
            for (int i = 0; i < bytesRead; i++)
                {
                    Debug.Print(readBuffer[i].ToString());
                }


            Powered = false;

            _serial.DataReceived += new SerialDataReceivedEventHandler(DataAvailable);
        }

        public void Dispose()
        {
            _serial.Dispose();
            _power.Dispose();
        }

        public bool Powered
        {
            get
            {
                return _power != null && _power.Read();
            }
            set
            {
                if (_power != null)
                {
                    lock(this)
                    {
                        // we power on the radio all the times a user requests it...
                        if (value == true)
                        {
                            // do not power up twice
                            if (_power.Read() == false)
                            {
                                _power.Write(true);

                                Thread.Sleep(c_powerOnTime); // give it a little time to wake up
                            }
                        }
                        //.. but we power it off only if no handler is registered for notifications
                        else
                        {
                            if (_subscribers == null)
                            {
                                _power.Write(false);
                            }
                        }
                    }
                }
            }
        }

        public event TemperatureReadingCallback NotifyReading
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                TemperatureReadingCallback callbacksOld = _subscribers;
                TemperatureReadingCallback callbacksNew = (TemperatureReadingCallback)Delegate.Combine(callbacksOld, value);

                try
                {
                    _subscribers = callbacksNew;

                    if (callbacksNew != null)
                    {
                        if (callbacksOld == null)
                        {
                            Powered = true;
                        }

                        if (callbacksNew.Equals(value) == false)
                        {
                            callbacksNew = new TemperatureReadingCallback(MultiCastCase);
                        }
                    }
                }
                catch
                {
                    _subscribers = callbacksOld;

                    if (callbacksOld == null)
                    {
                        Powered = false;
                    }

                    throw;
                }
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove
            {
                TemperatureReadingCallback callbacksOld = _subscribers;
                TemperatureReadingCallback callbacksNew = (TemperatureReadingCallback)Delegate.Remove(callbacksOld, value);

                try
                {
                    _subscribers = (TemperatureReadingCallback)callbacksNew;

                    if (callbacksNew == null && callbacksOld != null)
                    {
                        Powered = false;
                    }
                }
                catch
                {
                    _subscribers = callbacksOld;

                    throw;
                }
            }
        }

        public void Send(byte[] buffer)
        {
            try
            {
                Powered = true;

                _serial.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                lock (this)
                {
                    if (_subscribers == null)
                    {
                        Powered = false;
                    }
                }
            }
        }

        public byte[] Receive()
        {
            const int maxBufferLength = 32;

            byte[] buffer = new byte[maxBufferLength];

            int read = 0;

            while(true)
            {
                int available = _serial.BytesToRead;

                if(read >= maxBufferLength) break;

                if(available <= 0)
                {

                    {
                        int sleep = 10000; while(sleep-- > 0) ;
                    }

                    available = _serial.BytesToRead;

                    if(available <= 0) break;
                }

                int count = _serial.Read(buffer, read, available);

                read += count;
            }

            if(read > 0)
            {
                byte[] result = new byte[read];

                Array.Copy(buffer, result, read );

                return result;
            }

            return null;
        }

        public int BytesToRead
        {
            get
            {
                return _serial.BytesToRead;
            }
        }

        //--//

        private void DataAvailable(object sender, SerialDataReceivedEventArgs e)
        {
            if (_subscribers != null)
            {
                _subscribers(this);
            }
        }

        private bool IsOK(byte[] readBuffer)
        {
            if (readBuffer[0] == 79 && readBuffer[1] == 75 && readBuffer[2] == 13)
                return (true);
            else
                return (false);
        }

        private void XBeeRestore()
        {
            XBeeRestore(false);
        }
        private void XBeeRestore( bool throwOnError )
        {
            int bytesRead;
            int bytesSent;
            byte[] readBuffer = new byte[1000];

            byte[] RestoreMessage = new byte[] { (byte)'A', (byte)'T', (byte)'R', (byte)'E', (byte)13 };

            Thread.Sleep(1000);
            bytesSent = _serial.Write(RestoreMessage, 0, RestoreMessage.Length);
            Thread.Sleep(1000);
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
            if (throwOnError)
            {
                if (bytesRead != 3 || !IsOK(readBuffer))
                    throw new RadioException();
            }

        }

        private bool XBeeNodeDetect()
        {
            int bytesRead;
            int bytesSent;
            byte[] readBuffer = new byte[1000];

            byte[] NodeDetectMessage = new byte[] { (byte)'A', (byte)'T', (byte)'N', (byte)'D', (byte)13 };
            byte[] GetNetworkSettingsMessage = new byte[] { (byte)'A', (byte)'T', (byte)'A', (byte)'I', (byte)13 };

            Thread.Sleep(1000);
            bytesSent = _serial.Write(NodeDetectMessage, 0, NodeDetectMessage.Length);
            Thread.Sleep(1000);
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
            if (_verbose)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    Debug.Print(readBuffer[i].ToString());
                }
            }
            if (bytesRead < 2)
            {
                if (_verbose)
                {
                    bytesSent = _serial.Write(GetNetworkSettingsMessage, 0, GetNetworkSettingsMessage.Length);
                    Debug.Print("NetworkSettings Retrieved ");
                    Thread.Sleep(1000);
                    Debug.Print("NetworkSettings Retrieved ");
                    bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
                    for (int i = 0; i < bytesRead; i++)
                    {
                        Debug.Print(readBuffer[i].ToString());
                    }
                }
                return (false);
            }
            else
                return (true);
        }

        private void XBeeConfigure(byte[] self, byte[] target, bool coordinator)
        {
            XBeeConfigure(self, target, coordinator, false);
        }
        private void XBeeConfigure(byte[] self, byte[] target, bool coordinator, bool throwOnError)
        {
            byte[] SetNodeMessage    = new byte[] { (byte)'A', (byte)'T', (byte)'M', (byte)'Y', self[0]  , self  [1], (byte)13 };
            byte[] SetTargetLMessage = new byte[] { (byte)'A', (byte)'T', (byte)'D', (byte)'L', target[0], target[1], (byte)13 };

            byte[] SetIDMessage = new byte[] { (byte)'A', (byte)'T', (byte)'I', (byte)'D', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)13 };
            byte[] GetNodeMessage = new byte[] { (byte)'A', (byte)'T', (byte)'M', (byte)'Y', (byte)13 };


            //reverse these on the USBizi board

            byte[] SetTargetHMessage = new byte[] { (byte)'A', (byte)'T', (byte)'D', (byte)'H', (byte)'0', (byte)13 };
            byte[] GetTargetHMessage = new byte[] { (byte)'A', (byte)'T', (byte)'D', (byte)'H', (byte)13 };
            byte[] GetTargetLMessage = new byte[] { (byte)'A', (byte)'T', (byte)'D', (byte)'L', (byte)13 };

            byte[] SetCoordinatorMessage = new byte[] { (byte)'A', (byte)'T', (byte)'C', (byte)'E', (byte)1, (byte)13 };

            //byte[] WriteMessage = new byte[] { (byte)'A', (byte)'T', (byte)'W', (byte)'R', (byte)13 }; //not used - radion set up at the start of every session

            int bytesRead;
            int bytesSent;
            byte[] readBuffer = new byte[1000];

            // set this devices as the coordinator
            if (coordinator)
            {
                bytesSent = _serial.Write(SetCoordinatorMessage, 0, SetCoordinatorMessage.Length);
                Thread.Sleep(1000);
                bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead); // Clear the response
                Thread.Sleep(1000);
            }

            //set the network ID to "1234"
            bytesSent = _serial.Write(SetIDMessage, 0, SetIDMessage.Length);
            Thread.Sleep(1000);
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
            if (throwOnError)
            {
                if (bytesRead != 3 || !IsOK(readBuffer))
                    throw new RadioException();
            }
            Thread.Sleep(1000);

            if (_verbose)
            {
                // get the ID back to make sure that it was set 
                bytesSent = _serial.Write(c_GetIDMessage, 0, c_GetIDMessage.Length);
                Debug.Print("ID Retreived : ");
                Thread.Sleep(1000);
                bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
                for (int i = 0; i < bytesRead; i++)
                {
                    Debug.Print(readBuffer[i].ToString());
                }
            }
            //set the Node ID to "10"
            bytesSent = _serial.Write(SetNodeMessage, 0, SetNodeMessage.Length);
            Thread.Sleep(1000);
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
            if (throwOnError)
            {
                if (bytesRead != 3 || !IsOK(readBuffer))
                    throw new RadioException();
            }

            if (_verbose)
            {
                bytesSent = _serial.Write(GetNodeMessage, 0, GetNodeMessage.Length);
                Debug.Print("Node Retrieved : ");
                Thread.Sleep(1000);
                bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
                for (int i = 0; i < bytesRead; i++)
                {
                    Debug.Print(readBuffer[i].ToString());
                }
            }

            // set the target high address to '0'
            bytesSent = _serial.Write(SetTargetHMessage, 0, SetTargetHMessage.Length);
            Thread.Sleep(1000);
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
            if (throwOnError)
            {
                if (bytesRead != 3 || !IsOK(readBuffer))
                    throw new RadioException();
            }

            if (_verbose)
            {
                //get the target node that was set
                bytesSent = _serial.Write(GetTargetHMessage, 0, GetTargetHMessage.Length);
                Debug.Print("Target H Retrieved ");
                Thread.Sleep(1000);
                bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
                for (int i = 0; i < bytesRead; i++)
                {
                    Debug.Print(readBuffer[i].ToString());
                }
            }

            //set target low address to '11'
            bytesSent = _serial.Write(SetTargetLMessage, 0, SetTargetLMessage.Length);
            Thread.Sleep(1000);
            bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
            if (throwOnError)
            {
                if (bytesRead != 3 || !IsOK(readBuffer))
                    throw new RadioException();
            }
            if (_verbose)
            {
                bytesSent = _serial.Write(GetTargetLMessage, 0, GetTargetLMessage.Length);
                Debug.Print("Target L Retrieved ");
                Thread.Sleep(1000);
                bytesRead = _serial.Read(readBuffer, 0, _serial.BytesToRead);
                for (int i = 0; i < bytesRead; i++)
                {
                    Debug.Print(readBuffer[i].ToString());
                }
            }
        }

        private void MultiCastCase(XBee radio)
        {
            TemperatureReadingCallback callbacks = _subscribers;

            if (callbacks != null)
            {
                callbacks( this );
            }
        }
    }
}
