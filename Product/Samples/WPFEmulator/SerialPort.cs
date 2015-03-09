////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO.Ports;
using Microsoft.SPOT.Emulator.Com;
using System.Collections;
using System.IO;

namespace Microsoft.SPOT.Emulator.Sample
{
    

    public class PhysicalSerialPortStream : Stream
    {
        SerialPort _port;

        public PhysicalSerialPortStream(SerialPort port)
        {
            _port = port;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _port.Write(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _port.Read(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            _port.Dispose();
            base.Dispose(disposing);
        }

        public override void SetLength(long value)
        {
            _port.BaseStream.SetLength(value);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _port.BaseStream.Seek(offset, origin);
        }

        public override long Position
        {
            get
            {
                return _port.BaseStream.Position;
            }
            set
            {
                _port.BaseStream.Position = value;
            }
        }

        public override long Length
        {
            get { return _port.BaseStream.Length; }
        }

        public override void Flush()
        {
            _port.BaseStream.Flush();
        }

        public override bool CanWrite
        {
            get { return _port.BaseStream.CanWrite; }
        }
        public override bool CanSeek
        {
            get { return _port.BaseStream.CanSeek; }
        }
        public override bool CanRead
        {
            get { return _port.BaseStream.CanRead; }
        }
    }

    /// <summary>
    /// PhysicalSerialPort is an serial port emulator component that maps an emulator
    /// serial port to a physical serial port on the host PC
    /// </summary>
    public class PhysicalSerialPort : ComPortToStream, ISerialPortToStream
    {
        SerialPort _port;
        String _physicalPortName;
        int _portIndex;
        int _baudRate;
        OnEmuSerialPortEvtHandler _evtHandler;

        /// <summary>
        /// The port name of the physical serial port, such as "COM1" or "COM2"
        /// </summary>
        public String PhysicalPortName
        {
            get
            {
                return _physicalPortName;
            }
            set
            {
                ThrowIfNotConfigurable();

                _physicalPortName = value;
            }
        }

        /// <summary>
        /// The baud rate for the physical serial port, defaults to 38400.
        /// </summary>
        public int BaudRate
        {
            get
            {
                return _baudRate;
            }
            set
            {
                ThrowIfNotConfigurable();
                _baudRate = value;
            }
        }

        /// <summary>
        /// Default constructor for PhysicalSerialPort.
        /// </summary>
        public PhysicalSerialPort()
        {
            _physicalPortName = null;
            _baudRate = 38400;
        }

        public override void SetupComponent()
        {
            base.SetupComponent();
        }

        bool ISerialPortToStream.Initialize(int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue)
        {
            // From MSDN:  The best practice for any application is to wait for some amount of time after calling the Close 
            // method  before attempting to call the Open method, as the port may not be closed instantly.

            for(int retries = 5; retries != 0; retries--)
            {
                try
                {
                    if(_port == null)
                    {
                        _port = new SerialPort(_physicalPortName, BaudRate, (Parity)Parity, DataBits );
                        switch(FlowValue)
                        {
                            case 0x18:
                                _port.Handshake = Handshake.XOnXOff;
                                break;
                                
                            case 0x06:
                                _port.Handshake = Handshake.RequestToSend;
                                break;
                                
                            default:
                                _port.Handshake = Handshake.None;
                                break;
                        }
                    }

                    _portIndex = int.Parse(_physicalPortName.Substring(3))-1;
                    _port.DataReceived += new SerialDataReceivedEventHandler(_port_DataReceived);
                    _port.ErrorReceived += new SerialErrorReceivedEventHandler(_port_ErrorReceived);
                    _port.Open();
                    Stream = new PhysicalSerialPortStream(_port);

                    return true;
                }
                catch(Exception)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }

            return false;
        }

        bool ISerialPortToStream.SetDataEventHandler(OnEmuSerialPortEvtHandler handler)
        {
            _evtHandler = handler;
            return true;
        }

        enum SerialPortEventErrors
        {
            USART_EVENT_TYPE_ERROR = 1,
            USART_EVENT_DATA_CHARS = 5,
        }

        void _port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if(_evtHandler != null)
            {
                _evtHandler(_portIndex, (uint)SerialPortEventErrors.USART_EVENT_TYPE_ERROR);
            }
        }

        void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(_evtHandler != null)
            {
                _evtHandler(_portIndex, (uint)SerialPortEventErrors.USART_EVENT_DATA_CHARS);
            }
        }


        bool ISerialPortToStream.Uninitialize()
        {
            if(_port != null)
            {
                Stream = null;
                _port.Close();
                _port.Dispose();
                _port = null;
            }

            return true;
        }
        
    }

    /// <summary>
    /// A dummy wrapper class to ComPortToMemoryStream for the sample emulator, so it can
    /// be identified and hook up to the drop down menu (send text dialog box).
    /// </summary>
    public class EmulatorSerialPort : ComPortToMemoryStream, ISerialPortToStream
    {
        bool ISerialPortToStream.Initialize(int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue)
        {
            return true;
        }

        bool ISerialPortToStream.Uninitialize()
        {
            return true;
        }

        bool ISerialPortToStream.SetDataEventHandler(OnEmuSerialPortEvtHandler handler)
        {
            return true;
        }
    }
}
