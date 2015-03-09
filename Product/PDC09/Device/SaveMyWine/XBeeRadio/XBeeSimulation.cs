using System;
using System.Runtime.CompilerServices;
using System.IO.Ports;
using Microsoft.SPOT;
using System.Threading;

using Microsoft.SPOT.Hardware;


namespace Microsoft.SPOT.Samples.XBeeRadio
{
    public delegate void TemperatureReadingSimulationCallback(XBeeSimulation radio);

    //--//

    public class XBeeSimulation : IDisposable
    {
        public class RadioException : Exception
        {
            public RadioException()
            {
            }
        }

        //--//

        const int c_StartupTime = 1 * 1000; // 2 seconds 
        const int c_PowerOnTime = 100;
        
        //--//

        static readonly byte[] InitMessage = new byte[] { (byte)'+', (byte)'+', (byte)'+' };
        static readonly byte[] GetIDMessage = new byte[] { (byte)'A', (byte)'T', (byte)'I', (byte)'D', (byte)13 };
        static readonly byte[] EndMessage = new byte[] { (byte)'A', (byte)'T', (byte)'C', (byte)'N', (byte)13 };

        bool _powered;

        Timer _dataReceivedTimer;
        TemperatureReadingSimulationCallback _subscribers;

        //--//

        public XBeeSimulation(string port, int baud, byte[] self, byte[] target, bool coordinator, Cpu.Pin powerPin)
        {
            byte[] readBuffer = new byte[32];

            if (self.Length != 2 || target.Length != 2)
            {
                throw new ArgumentException();
            }

            Powered = true;

            Thread.Sleep(c_StartupTime);

            _dataReceivedTimer = new Timer(FakeDataReivedTimerCallback, null, 5000, 2000); // every 2 secs
            
            Powered = false;
        }

        public void Dispose()
        {
        }

        public bool Powered
        {
            get
            {
                return _powered;
            }
            set
            {
                lock (this)
                {
                    // we power on the radio all the times a user requests it...
                    if (value == true)
                    {
                        // do not power up twice
                        if (_powered == false)
                        {
                            _powered = true;

                            Thread.Sleep(c_PowerOnTime); // give it a little time to wake up
                        }
                    }
                    //.. but we power it off only if no handler is registered for notifications
                    else
                    {
                        if (_subscribers == null)
                        {
                            _powered = false;
                        }
                    }
                }
            }
        }

        public event TemperatureReadingSimulationCallback NotifyReading
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                TemperatureReadingSimulationCallback callbacksOld = _subscribers;
                TemperatureReadingSimulationCallback callbacksNew = (TemperatureReadingSimulationCallback)Delegate.Combine(callbacksOld, value);

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
                            callbacksNew = new TemperatureReadingSimulationCallback(MultiCastCase);
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
                TemperatureReadingSimulationCallback callbacksOld = _subscribers;
                TemperatureReadingSimulationCallback callbacksNew = (TemperatureReadingSimulationCallback)Delegate.Remove(callbacksOld, value);

                try
                {
                    _subscribers = (TemperatureReadingSimulationCallback)callbacksNew;

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

                _dataReceivedTimer.Change(10, 5000);
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
            return null;
        }

        public int BytesToRead
        {
            get
            {
                return 5;
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

        private void FakeDataReivedTimerCallback(object state)
        {
            DataAvailable(null, null);
        }

        private void MultiCastCase(XBeeSimulation radio)
        {
            TemperatureReadingSimulationCallback callbacks = _subscribers;

            if (callbacks != null)
            {
                callbacks(this);
            }
        }
    }
}
