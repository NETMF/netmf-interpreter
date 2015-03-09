////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TestSerialPort : SerialPort
    {
        private bool _readAsync = false;
        private bool _asyncReading = false;
        private string _asyncResult = "";
        private int _serialPortCount;

        private void TestSerialPort_Init()
        {
            _serialPortCount = HardwareProvider.HwProvider.GetSerialPortsCount();
            this.ErrorReceived += new SerialErrorReceivedEventHandler(TestSerialPort_ErrorReceived);
        }

        public TestSerialPort( string portName )
            : base(portName)
        {
            this.TestSerialPort_Init();
        }

        public TestSerialPort( string portName, int baudRate )
            : base( portName, baudRate)
        {
            this.TestSerialPort_Init();
        }
        public TestSerialPort(string portName, int baudRate, Parity parity)
            : base( portName, baudRate, parity)
        {
            this.TestSerialPort_Init();
        }
        public TestSerialPort( string portName, int baudRate, Parity parity, int dataBits )
            : base( portName, baudRate, parity, dataBits)
        {
            this.TestSerialPort_Init();
        }
        public TestSerialPort( string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits ) 
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
            this.TestSerialPort_Init();
        }

        void TestSerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Log.Exception("TestSerialPort Error Rcvd event fired.");
            string type = "";
            switch (e.EventType)
            {
                case SerialError.Frame:
                    type = "Frame";
                    break;
                case SerialError.Overrun:
                    type = "Overrun"; 
                    break;
                case SerialError.RXOver:
                    type = "RXOver";
                    break;
                case SerialError.RXParity:
                    type = "RXParity";
                    break;
                case SerialError.TXFull:
                    type = "TXFull";
                    break;
            }
            Log.Comment("Event Type: " + type);
        }
        
        public void VerifyWrite()
        {
            // limit to 512 bytes because of potential 
            // buffer issues with larger strings.
            int length = new Random().Next(512);
            string data = MFUtilities.GetRandomString(length);
            Log.FilteredComment("Using string: '" + data + "'");
            this.VerifyWrite(data);
        }

        public void VerifyWrite(string data)
        {
            if (!this.IsOpen)
                this.Open();
            byte[] buff = Encoding.UTF8.GetBytes(data);
            this.Write(buff, 0, buff.Length);
            this.Flush();
            while (this.BytesToWrite > 0)
            {
                // wait for all data to be buffered
                Log.Comment("bytes to send: " + this.BytesToWrite);
                Thread.Sleep(100);
            }
            // Make a buffer size of the bytes in read buffer
            string readString = "";
            Thread.Sleep(100);
            while (this.BytesToRead > 0)
            {
                byte[] readbuff = new byte[this.BytesToRead];
                this.Read(readbuff, 0, readbuff.Length);
                readString += new string(Encoding.UTF8.GetChars(readbuff));

                Thread.Sleep(100);
            }
            this.Close();
            if (data != readString)
                throw new Exception("VerifyWrite failed: <![CDATA[" + data + " != " + readString + "]]>");
        }

        public void EvalOpenClose(object a, object b)
        {
            this.Eval(a, b);
            if (_serialPortCount > 0)
            {
                this.Open();
                this.Close();
                this.Eval(a, b);
            }
        }

        public void EvalOpenClose(string a, string b)
        {
            this.Eval(a, b);
            if (_serialPortCount > 0)
            {
                this.Open();
                this.Close();
                this.Eval(a, b);
            }
        }
        
        public void Eval(object a, object b)
        {
            if (!a.Equals(b))
            {
                throw new Exception(a + " != " + b);
            }
            else
            {
                Log.Comment(a + " == " + b);
            }
        }

        public void Eval(string a, string b)
        {
            if (a != b)
            {
                Log.Exception(a + " != " + b);
                throw new Exception(a + " != " + b);
            }
            else
            {
                Log.Comment(a + " == " + b);
            }
        }

        public bool IsAsyncReading
        {
            get { return _asyncReading; }
        }

        public string AsyncResult
        {
            get { return _asyncResult; }
        }

        public bool AsyncRead
        {
            get { return _readAsync; }
            set
            {
                _readAsync = value;
                if (_readAsync)
                {
                    _asyncResult = string.Empty;
                    Thread asyncThread = new Thread(new ThreadStart(this.AsyncReader));
                    asyncThread.Priority = ThreadPriority.BelowNormal;
                    asyncThread.Start();
                }
            }
        }

        private void AsyncReader()
        {
            _asyncReading = true;
            try
            {
                while (_readAsync && this.IsOpen)
                {
                    byte[] readbuff = new byte[this.BytesToRead];
                    this.Read(readbuff, 0, readbuff.Length);
                    _asyncResult += new string(Encoding.UTF8.GetChars(readbuff));
                }
            }
            finally
            {
                _asyncReading = false;
            }
        }
    }
}
