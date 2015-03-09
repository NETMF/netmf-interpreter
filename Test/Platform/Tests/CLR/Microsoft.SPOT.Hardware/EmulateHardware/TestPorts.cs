using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TestPorts : IMFTestInterface
    {
        private MFTestResults result;
        private int portCount;
        private bool IsEmulator = false;
        private bool IsLoopback = false;
        private const string testString = @"\\//><:;',.Test string !@#$%^&**()123";
        private byte[] sendbuff;
        private byte[] readbuff = new byte[256];
        private SerialPort eventSerialPort = null;
        private int eventCount;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            // Add your functionality here.                
            portCount = HardwareProvider.HwProvider.GetSerialPortsCount();
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)  //The Emulator on Windows Host
                IsEmulator = true;
            Debug.Print("IsEmulator: " + IsEmulator);

            // Test for loopback
            sendbuff = Encoding.UTF8.GetBytes(testString);
            Log.Comment("Port count: " + portCount);
            if (portCount > 0)
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.ReadTimeout = 1000;
                    try
                    {
                        serialPort.VerifyWrite(testString);
                        this.IsLoopback = true;
                    }
                    catch
                    {
                        Log.Comment("No loopback detected.  Functional tests will not run");
                    }
                }
            }
            
            return InitializeResult.ReadyToGo;            
        }         

        [TearDown]
        public void CleanUp()
        {
        }

        #region Constructor tests  -  should run on all platforms

        [TestMethod]
        public MFTestResults SerialPortConstructor_1()
        {
            result = MFTestResults.Pass;
            try
            {
                // Valid cases
                ConstructorSuccessTest(Serial.COM1);
                ConstructorSuccessTest(Serial.COM2);
                ConstructorSuccessTest(Serial.COM3);
                ConstructorSuccessTest("com4");
                ConstructorSuccessTest("CoM5");
                ConstructorSuccessTest("Com10");
                ConstructorSuccessTest("Com100");

                // Negative cases
                ArgumentExceptionTest("");
                ArgumentExceptionTest(null);
                ArgumentExceptionTest("COM0");
                ArgumentExceptionTest("COM" + int.MaxValue + "0");
                ArgumentExceptionTest("COM");
                ArgumentExceptionTest("SON1");
                string longStr = "Com1 long";
                for (int i = 0; i < 10; i++)
                {
                    longStr += longStr;
                }
                ArgumentExceptionTest(longStr);
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }

            return result;
        }

        [TestMethod]
        public MFTestResults SerialPortConstructor_2()
        {
            result = MFTestResults.Pass;
            try
            {
                // Validate all BaudRate values - no casting/bounds tests
                ConstructorSuccessTest(BaudRate.Baudrate4800);
                ConstructorSuccessTest(BaudRate.Baudrate9600);
                ConstructorSuccessTest(BaudRate.Baudrate19200);
                ConstructorSuccessTest(BaudRate.Baudrate38400);
                ConstructorSuccessTest(BaudRate.Baudrate57600);
                ConstructorSuccessTest(BaudRate.Baudrate115200);
                ConstructorSuccessTest(BaudRate.Baudrate230400);
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults SerialPortConstructor_3()
        {
            result = MFTestResults.Pass;
            try
            {
                // Valid cases
                ConstructorSuccessTest(Parity.Even);
                ConstructorSuccessTest(Parity.Mark);
                ConstructorSuccessTest(Parity.None);
                ConstructorSuccessTest(Parity.Odd);
                ConstructorSuccessTest(Parity.Space);
                // No bounds checking so make sure all valid int values are fine.
                ConstructorSuccessTest(-1);
                ConstructorSuccessTest(int.MinValue);
                ConstructorSuccessTest(int.MaxValue);
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }

            return result;
        }

        [TestMethod]
        public MFTestResults SerialPortConstructor_4()
        {
            result = MFTestResults.Pass;
            try
            {
                // Valid cases
                ConstructorSuccessTest(5);
                ConstructorSuccessTest(6);
                ConstructorSuccessTest(7);
                ConstructorSuccessTest(8);
                ConstructorSuccessTest(9);
                // No bounds checking so make sure all valid int values are fine.
                ConstructorSuccessTest(-1);
                ConstructorSuccessTest(int.MinValue);
                ConstructorSuccessTest(int.MaxValue);
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults SerialPortConstructor_5()
        {
            result = MFTestResults.Pass;
            try
            {
                // Valid cases
                ConstructorSuccessTest(StopBits.None);
                ConstructorSuccessTest(StopBits.One);
                ConstructorSuccessTest(StopBits.OnePointFive);
                ConstructorSuccessTest(StopBits.Two);
                // No bounds checking so make sure all valid int values are fine.
                ConstructorSuccessTest(-1);
                ConstructorSuccessTest(int.MinValue);
                ConstructorSuccessTest(int.MaxValue);
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        #endregion - Constructor tests

        #region SerialPort Property tests
        [TestMethod]
        public MFTestResults GetSetBaudRate()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.BaudRate, (int)BaudRate.Baudrate9600);
                    serialPort.BaudRate = (int)BaudRate.Baudrate230400;
                    serialPort.Eval(serialPort.BaudRate, (int)BaudRate.Baudrate230400);
                    serialPort.BaudRate = (int)BaudRate.Baudrate19200;
                    serialPort.EvalOpenClose(serialPort.BaudRate, (int)BaudRate.Baudrate19200);
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetSetDataBits()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.DataBits, 8);
                    serialPort.DataBits = 7;
                    serialPort.EvalOpenClose(serialPort.DataBits, 7);
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetSetParity()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.Parity, Parity.None);
                    serialPort.Parity = Parity.Even;
                    serialPort.Eval(serialPort.Parity, Parity.Even);
                    serialPort.Parity = Parity.Odd;
                    serialPort.EvalOpenClose(serialPort.Parity, Parity.Odd);
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetSetStopBits()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.StopBits, StopBits.One);
                    serialPort.StopBits = StopBits.Two;
                    serialPort.EvalOpenClose(serialPort.StopBits, StopBits.Two);
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetSetHandShake()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.Handshake, Handshake.None);
                    serialPort.Handshake = Handshake.RequestToSend;
                    serialPort.EvalOpenClose(serialPort.Handshake, Handshake.RequestToSend);
                    serialPort.Handshake = Handshake.XOnXOff;
                    serialPort.EvalOpenClose(serialPort.Handshake, Handshake.XOnXOff);
                }
                if (portCount > 0)
                {
                    // Validate we can re-reserve all pins
                    using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                    {
                        serialPort.Handshake = Handshake.RequestToSend;
                        serialPort.EvalOpenClose(serialPort.Handshake, Handshake.RequestToSend);
                        serialPort.Open();
                    }
                }
                // TODO: Write Desktop tests to validate proper function of RTS/XonXoff
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetSetReadTimeout()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.ReadTimeout, Timeout.Infinite);
                    serialPort.ReadTimeout = 100;
                    serialPort.Eval(serialPort.ReadTimeout, 100);
                    serialPort.ReadTimeout = int.MinValue;
                    serialPort.Eval(serialPort.ReadTimeout, int.MinValue);
                    serialPort.ReadTimeout = int.MaxValue;
                    serialPort.EvalOpenClose(serialPort.ReadTimeout, int.MaxValue);
                    serialPort.ReadTimeout = 1000;
                    serialPort.EvalOpenClose(serialPort.ReadTimeout, 1000);
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetSetWriteTimeout()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.WriteTimeout, Timeout.Infinite);
                    serialPort.WriteTimeout = 100;
                    serialPort.Eval(serialPort.WriteTimeout, 100);
                    serialPort.WriteTimeout = int.MinValue;
                    serialPort.Eval(serialPort.WriteTimeout, int.MinValue);
                    serialPort.WriteTimeout = int.MaxValue;
                    serialPort.EvalOpenClose(serialPort.WriteTimeout, int.MaxValue);
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetPortName()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.PortName, Serial.COM1);
                }
                if (portCount > 1)
                {
                    using (TestSerialPort serialPort = new TestSerialPort(Serial.COM2))
                    {
                        serialPort.Eval(serialPort.PortName, Serial.COM2);
                    }
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetBytesToRead()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.BytesToRead, 0);
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults GetBytesToWrite()
        {
            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Eval(serialPort.BytesToWrite, 0);
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }
        #endregion SerialPort Property tests

        #region Functional Tests
        [TestMethod]
        public MFTestResults DataBufferTests()
        {
            if (!IsLoopback)
                return MFTestResults.Skip;

            result = MFTestResults.Pass;
            try
            {
                using (SerialPort serialPort = new SerialPort(Serial.COM1))
                {
                    // set flow control so we can fill both RX/TX buffers
                    serialPort.Handshake = Handshake.RequestToSend;
                    serialPort.WriteTimeout = 1000;
                    serialPort.Open();

                    // buffer data in RX/TX
                    serialPort.Write(sendbuff, 0, sendbuff.Length);

                    Log.Comment("bytes to send: " + serialPort.BytesToWrite);
                    Log.Comment("bytes to read: " + serialPort.BytesToRead);

                    // clear RX buffer
                    serialPort.DiscardInBuffer();
                    if (serialPort.BytesToRead != 0)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception(serialPort.BytesToRead + " bytes still in buffer after DiscardInBuffer!");
                    }

                    // clear TX Buffer
                    serialPort.DiscardOutBuffer();
                    if (serialPort.BytesToWrite != 0)
                    {
                        // BUGBUG: 21224
                        result = MFTestResults.Fail;
                        Log.Exception(serialPort.BytesToWrite + " bytes still in buffer after DiscardOutBuffer!");
                    }
                    serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults LargeSendBuffer()
        {
            if (!IsLoopback)
                return MFTestResults.Skip;

            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Handshake = Handshake.RequestToSend;
                    serialPort.Open();
                    serialPort.AsyncRead = true;
                    string start = MFUtilities.GetRandomSafeString(10000);
                    byte[] buff = Encoding.UTF8.GetBytes(start);
                    serialPort.Write(buff, 0, buff.Length);

                    // wait for data to drain
                    while (serialPort.AsyncResult.Length < start.Length && (serialPort.BytesToWrite > 0 || serialPort.BytesToRead > 0))
                    {
                        Thread.Sleep(100);
                    }
                    if (serialPort.AsyncResult != start)
                    {
                        Log.Exception("Failed: " + serialPort.AsyncResult + " != " + start);
                        result = MFTestResults.Fail;
                    }
                    // wait to make sure AsyncReader is closed;
                    serialPort.AsyncRead = false;
                    while (serialPort.IsAsyncReading)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults VerifyValidChars()
        {
            if (!IsLoopback)
                return MFTestResults.Skip;

            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    serialPort.Open();
                    for (int i = 1; i < 255; i++)
                    {
                        string data = new string((char)i, 10);
                        Log.FilteredComment("i = " + i + " chars: " + data);
                        serialPort.VerifyWrite(data);
                    }
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults AdvancedGetSetTest()
        {
            if (!IsLoopback)
                return MFTestResults.Skip;

            result = MFTestResults.Pass;
            try
            {
                using (TestSerialPort serialPort = new TestSerialPort(Serial.COM1))
                {
                    // Exercise changing rate while open if port is available
                    serialPort.VerifyWrite();
                    Log.Comment("Change baud and resend");
                    serialPort.BaudRate = (int)BaudRate.Baudrate19200;
                    serialPort.VerifyWrite();
                    Log.Comment("Change Parity and resend");
                    serialPort.Parity = Parity.Even;
                    serialPort.VerifyWrite();
                    Log.Comment("Change StopBit and resend");
                    serialPort.StopBits = StopBits.Two;
                    serialPort.VerifyWrite();
                    Log.Comment("Change DataBits and resend");
                    serialPort.DataBits = 7;
                    serialPort.VerifyWrite();
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults DataRcdEvent()
        {
            if (!IsLoopback)
                return MFTestResults.Skip;

            // BUGBUG: 21216
            result = MFTestResults.Fail;
            try
            {
                eventCount = 0;
                eventSerialPort = new SerialPort(Serial.COM1);
                // register before open
                eventSerialPort.DataReceived += new SerialDataReceivedEventHandler(eventserialPort_DataReceived_BeforeOpen);
                eventSerialPort.Open();
                eventSerialPort.DataReceived += new SerialDataReceivedEventHandler(eventSerialPort_DataReceived_AfterOpen);
                eventSerialPort.DataReceived += new SerialDataReceivedEventHandler(eventSerialPort_DataReceived_AfterOpen2);
                eventSerialPort.Write(sendbuff, 0, sendbuff.Length);
                eventSerialPort.Flush();
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(100);
                    if (eventCount >= 3)
                    {
                        result = MFTestResults.Pass;
                        break;
                    }
                }
                Log.Comment(eventCount + " events fired");
                eventSerialPort.Close();
            }
            catch (Exception ex)
            {
                Log.Exception(ex.Message);
            }
            finally
            {
                eventSerialPort.Dispose();
            }
            return result;
        }
        
        private SerialError expectedError;

        [TestMethod]
        public MFTestResults ErrorRcvdEvent()
        {
            if (!IsLoopback || IsEmulator)
                return MFTestResults.Skip;

            result = MFTestResults.Pass;
            try
            {
                eventCount = 0;
                // create a buffer several bytes bigger then internal buffers
                byte[] buffer = Encoding.UTF8.GetBytes(new string('a', 512+40));
                eventSerialPort = new SerialPort(Serial.COM1);
                eventSerialPort.WriteTimeout = 1000;
                eventSerialPort.Handshake = Handshake.None;

                // register events
                eventSerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(eventSerialPort_ErrorReceived_BeforeOpen);
                eventSerialPort.Open();
                eventSerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(eventSerialPort_ErrorReceived_AfterOpen);

                // Test RX overflow (no flow control)
                expectedError = SerialError.RXOver;
                eventSerialPort.Write(buffer, 0, buffer.Length / 2);
                Thread.Sleep(100);
                eventSerialPort.Write(buffer, 0, buffer.Length / 2);
                eventSerialPort.Close();

                Thread.Sleep(500);

                if (eventCount == 0)
                {
                    // BUGBUG: 21222
                    Log.Exception("Expected RXOver events fired, saw " + eventCount + " fired.");
                    result = MFTestResults.Fail;
                }
                eventCount = 0;

                // Test TX overflow (flow control - HW)
                expectedError = SerialError.TXFull;
                eventSerialPort.Open();
                for (int i = 0; i < 2; i++)
                {
                    eventSerialPort.Write(buffer, 0, buffer.Length);
                }
                eventSerialPort.Close();

                Thread.Sleep(500);

                if (eventCount == 0)
                {
                    // BUGBUG: 21222
                    Log.Exception("Expected TXFull events fired, saw " + eventCount + " fired.");
                    result = MFTestResults.Fail;
                }

                // TODO: Need to add PC based tests that allow testing Parity, Overrun, and Frame errors.  This is done manually now.
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception(ex.Message);
            }
            finally
            {
                eventSerialPort.Dispose();
            }
            return result;
        }



        #endregion

        #region Event handlers

        void eventSerialPort_DataReceived_AfterOpen2(object sender, SerialDataReceivedEventArgs e)
        {
            Log.Comment("AfterOpen2 event fired.  " + eventSerialPort.BytesToRead + " bytes availabe to read.  Event Type: " + e.EventType);
            if (eventSerialPort.BytesToRead > 0)
                eventCount++;
        }

        void eventSerialPort_DataReceived_AfterOpen(object sender, SerialDataReceivedEventArgs e)
        {
            Log.Comment("AfterOpen event fired.  " + eventSerialPort.BytesToRead + " bytes availabe to read.  Event Type: " + e.EventType);
            if (eventSerialPort.BytesToRead > 0)
                eventCount++;
        }

        void eventserialPort_DataReceived_BeforeOpen(object sender, SerialDataReceivedEventArgs e)
        {
            Log.Comment("BeforeOpen event fired.  " + eventSerialPort.BytesToRead + " bytes availabe to read.  Event Type: " + e.EventType);
            if (eventSerialPort.BytesToRead > 0)
                eventCount++;
        }

        void eventSerialPort_ErrorReceived_BeforeOpen(object sender, SerialErrorReceivedEventArgs e)
        {
            if (e.EventType == expectedError)
            {
                if (eventCount == 0)
                {
                    Log.Comment("BeforeOpen event fired.  " + eventSerialPort.BytesToRead + " bytes availabe to read.  Event Type: " + e.EventType);
                }
                eventCount++;
            }
            // with loopback you can't get a TxFull without an RX FULL
            else if(!(e.EventType == SerialError.RXOver && expectedError == SerialError.TXFull))
            {
                Log.Exception("Expected EventType " + expectedError + " but received EventType " + e.EventType);
                result = MFTestResults.Fail;
            }
        }

        void eventSerialPort_ErrorReceived_AfterOpen(object sender, SerialErrorReceivedEventArgs e)
        {
            if (e.EventType == expectedError)
            {
                if (eventCount == 0)
                {
                    Log.Comment("AfterOpen event fired.  Event Type: " + e.EventType);
                }
                eventCount++;
            }
            // with loopback you can't get a TxFull without an RX FULL
            else if (!(e.EventType == SerialError.RXOver && expectedError == SerialError.TXFull))
            {
                Log.Exception("Expected EventType " + expectedError + " but received EventType " + e.EventType);
                result = MFTestResults.Fail;
            }
        }

        #endregion event handlers

        #region Private helper functions
        private void ConstructorSuccessTest(string port)
        {
            ConstructorSuccessTest(port, (int)BaudRate.Baudrate9600, Parity.None, 8, StopBits.One);
        }

        private void ConstructorSuccessTest(BaudRate baud)
        {
            ConstructorSuccessTest(Serial.COM1, (int)baud, Parity.None, 8, StopBits.One);
        }

        private void ConstructorSuccessTest(Parity parity)
        {
            ConstructorSuccessTest(Serial.COM1, (int)BaudRate.Baudrate9600, parity, 8, StopBits.One);
        }

        private void ConstructorSuccessTest(int databits)
        {
            ConstructorSuccessTest(Serial.COM1, (int)BaudRate.Baudrate9600, Parity.None, databits, StopBits.One);
        }

        private void ConstructorSuccessTest(StopBits stopbits)
        {
            ConstructorSuccessTest(Serial.COM1, (int)BaudRate.Baudrate9600, Parity.None, 8, stopbits);
        }

        private void ConstructorSuccessTest(string port, int baud, Parity parity, int databits, StopBits stopbits)
        {
            try
            {
                using (SerialPort serial = new SerialPort(port, baud, parity, databits, stopbits)) { }
            }
            catch (ArgumentException)
            {
                Log.Comment("Ignore argument exceptions in constructor");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception message for parity: " + parity + " - message: " + ex.Message);
                result = MFTestResults.Fail;
            }
        }


        private void ArgumentExceptionTest(string port)
        {
            if (port == null)
                ArgumentExceptionTest(port, 9600, Parity.None, 8, StopBits.One, "Exception was thrown: System.ArgumentNullException");
            else
                ArgumentExceptionTest(port, 9600, Parity.None, 8, StopBits.One, "Invalid serial port name");
        }

        private void ArgumentExceptionTest(int parity)
        {
            ArgumentExceptionTest(Serial.COM1, 9600, (Parity)parity, 8, StopBits.None, "parity");
        }

        private void ArgumentExceptionTest(string port, int baud, Parity parity, int databits, StopBits stopbits, string message)
        {
            try
            {
                using (SerialPort serial = new SerialPort(port, baud, parity, databits, stopbits)) { }
                throw new Exception("SerialPort constructor using " + port + " unexpectedly succeeded.  ArgumentException expected");
            }
            catch (ArgumentException ex)
            {
                if (ex.Message != message)
                {
                    Log.Exception("Unexpected exception message for port:" + port + " baud:" + baud + " parity:" + parity + " stopbits:" + stopbits + " - message: " + ex.Message);
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception for port:" + port + " baud:" + baud + " parity:" + parity + " stopbits:" + stopbits + " - message: " + ex.Message);
                result = MFTestResults.Fail;
            }
        }

        private void ValidateSendString(byte[] response)
        {
            string readString = new string(Encoding.UTF8.GetChars(response));
            if (testString != readString)
            {
                result = MFTestResults.Fail;
                Log.Exception( "<![CDATA[" + testString + " != " + readString + "]]>");
            }
        }
        #endregion
    }
}
