using System;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Hardware;

namespace System.IO.Ports
{
    public class SerialPort : Stream
    {
        internal class Configuration
        {
            internal readonly int PortIndex;
            internal int Speed;
            internal Parity Parity;
            internal int DataBits;
            internal StopBits StopBits;
            internal Handshake Handshake;
            internal int ReadTimeout;
            internal int WriteTimeout;

            internal Configuration(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            {
                PortIndex = System.IO.Ports.SerialPortName.ConvertNameToIndex(portName);
                Speed = baudRate;
                Parity = parity;
                DataBits = dataBits;
                StopBits = stopBits;
                Handshake = Handshake.None;
                ReadTimeout = Timeout.Infinite;
                WriteTimeout = Timeout.Infinite;
            }
        }

        // -- //

        private Configuration m_config;

        private bool m_fDisposed;
        private bool m_fOpened;
        private string m_portName;
        private NativeEventDispatcher m_evtErrorEvent = null;
        private NativeEventDispatcher m_evtDataEvent = null;
        private SerialErrorReceivedEventHandler m_callbacksErrorEvent = null;
        private SerialDataReceivedEventHandler m_callbacksDataEvent = null;
        // -- //
        public SerialPort(string portName)
            : this(portName, (int)Ports.BaudRate.Baudrate9600, Parity.None, 8, StopBits.One)
        {
        }

        public SerialPort(string portName, int baudRate)
            : this(portName, baudRate, Parity.None, 8, StopBits.One)
        {
        }

        public SerialPort(string portName, int baudRate, Parity parity)
            : this(portName, baudRate, parity, 8, StopBits.One)
        {
        }

        public SerialPort(string portName, int baudRate, Parity parity, int dataBits)
            : this(portName, baudRate, parity, dataBits, StopBits.One)
        {
        }

        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            m_portName = portName;
            m_config = new Configuration(portName, baudRate, parity, dataBits, stopBits);
            m_fDisposed = false;
            m_fOpened = false;
            m_evtErrorEvent = new NativeEventDispatcher("SerialPortErrorEvent", (ulong)m_config.PortIndex);
            m_evtDataEvent = new NativeEventDispatcher("SerialPortDataEvent", (ulong)m_config.PortIndex);
        }

        private void HandlePinReservations(bool fReserve)
        {
            int exceptLevel = 0;
            Cpu.Pin rxPin = Cpu.Pin.GPIO_NONE;
            Cpu.Pin txPin = Cpu.Pin.GPIO_NONE;
            Cpu.Pin CTSPin = Cpu.Pin.GPIO_NONE;
            Cpu.Pin RTSPin = Cpu.Pin.GPIO_NONE;

            try
            {
                HardwareProvider hwProvider = HardwareProvider.HwProvider;
                if (hwProvider != null)
                {
                    hwProvider.GetSerialPins(m_portName, out rxPin, out txPin, out CTSPin, out RTSPin);

                    if (rxPin != Cpu.Pin.GPIO_NONE)
                    {
                        Port.ReservePin(rxPin, fReserve);
                        exceptLevel = 1;
                    }

                    if (txPin != Cpu.Pin.GPIO_NONE)
                    {
                        Port.ReservePin(txPin, fReserve);
                        exceptLevel = 2;
                    }

                    if (m_config.Handshake == Handshake.RequestToSend)
                    {
                        if (CTSPin != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(CTSPin, fReserve);
                            exceptLevel = 3;
                        }

                        if (RTSPin != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(RTSPin, fReserve);
                            exceptLevel = 4;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (fReserve)
                {
                    // unreserve the pin in the case of an exception
                    switch (exceptLevel)
                    {
                        case 4:
                            Port.ReservePin(RTSPin, false);
                            // fall through
                            goto case 3;
                        case 3:
                            Port.ReservePin(CTSPin, false);
                            // fall through
                            goto case 2;
                        case 2:
                            Port.ReservePin(txPin, false);
                            // fall through
                            goto case 1;
                        case 1:
                            Port.ReservePin(rxPin, false);
                            break;
                    }
                }

                throw e;
            }

        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void Open()
        {
            if (m_fOpened) throw new InvalidOperationException();
            if (m_fDisposed) throw new ObjectDisposedException();

            InternalOpen();
            HandlePinReservations(true);

            m_fOpened = true;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public override void Close()
        {
            if (!m_fOpened) return;

            try
            {
                HandlePinReservations(false);

                InternalClose();
            }
            finally
            {
                m_fOpened = false;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (!m_fDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_callbacksErrorEvent != null)
                        {
                            m_evtErrorEvent.OnInterrupt -= new NativeEventHandler(ErrorEventHandler);
                            m_callbacksErrorEvent = null;
                            m_evtErrorEvent.Dispose();
                        }

                        if (m_callbacksDataEvent != null)
                        {
                            m_evtDataEvent.OnInterrupt -= new NativeEventHandler(DataEventHandler);
                            m_callbacksDataEvent = null;
                            m_evtDataEvent.Dispose();
                        }
                    }

                    if (m_fOpened)
                    {
                        Close();
                    }
                }
                finally
                {
                    m_fDisposed = true;
                    m_fOpened = false;
                }
            }

            base.Dispose(disposing);
        }

        public void DiscardInBuffer()
        {
            DiscardBuffer(true);
        }

        public void DiscardOutBuffer()
        {
            DiscardBuffer(false);
        }

        //--// Properties

        public string PortName { get { return m_portName; } }
        public bool IsOpen { get { return m_fOpened; } }
        public int BytesToRead { get { return BytesInBuffer(true); } }
        public int BytesToWrite { get { return BytesInBuffer(false); } }
        public int DataBits { get { return m_config.DataBits; } set { if (m_fOpened) throw new InvalidOperationException(); m_config.DataBits = value; } }
        public int BaudRate { get { return m_config.Speed; } set { if (m_fOpened) throw new InvalidOperationException(); m_config.Speed = value; } }
        public Handshake Handshake { get { return m_config.Handshake; } set { if (m_fOpened) throw new InvalidOperationException(); m_config.Handshake = value; } }
        public Parity Parity { get { return m_config.Parity; } set { if (m_fOpened) throw new InvalidOperationException(); m_config.Parity = value; } }
        public StopBits StopBits { get { return m_config.StopBits; } set { if (m_fOpened) throw new InvalidOperationException(); m_config.StopBits = value; } }
        public override int ReadTimeout { get { return m_config.ReadTimeout; } set { if (m_fOpened) throw new InvalidOperationException(); m_config.ReadTimeout = value; } }
        public override int WriteTimeout { get { return m_config.WriteTimeout; } set { if (m_fOpened) throw new InvalidOperationException(); m_config.WriteTimeout = value; } }

        public Stream BaseStream { get { return (Stream)this; } }


        public override bool CanRead
        {
            get { return true; }
        }

        // If CanSeek is false, Position, Seek, Length, and SetLength should throw.
        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanTimeout
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }


        //--// Events

        private void ErrorEventHandler(uint evt, uint data2, DateTime timestamp)
        {
            if (m_callbacksErrorEvent != null)
            {
                m_callbacksErrorEvent(this, new SerialErrorReceivedEventArgs((SerialError)evt));
            }
        }

        private void DataEventHandler(uint evt, uint data2, DateTime timestamp)
        {
            if (m_callbacksDataEvent != null)
            {
                m_callbacksDataEvent(this, new SerialDataReceivedEventArgs((SerialData)evt));
            }
        }

        public event SerialErrorReceivedEventHandler ErrorReceived
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                if (m_fDisposed)
                {
                    throw new ObjectDisposedException();
                }

                SerialErrorReceivedEventHandler callbacksOld = m_callbacksErrorEvent;
                SerialErrorReceivedEventHandler callbacksNew = (SerialErrorReceivedEventHandler)Delegate.Combine(callbacksOld, value);

                try
                {
                    m_callbacksErrorEvent = callbacksNew;

                    if (callbacksOld == null && m_callbacksErrorEvent != null)
                    {
                        m_evtErrorEvent.OnInterrupt += new NativeEventHandler(ErrorEventHandler);
                    }
                }
                catch
                {
                    m_callbacksErrorEvent = callbacksOld;

                    throw;
                }
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove
            {
                if (m_fDisposed)
                {
                    throw new ObjectDisposedException();
                }

                SerialErrorReceivedEventHandler callbacksOld = m_callbacksErrorEvent;
                SerialErrorReceivedEventHandler callbacksNew = (SerialErrorReceivedEventHandler)Delegate.Remove(callbacksOld, value);

                try
                {
                    m_callbacksErrorEvent = callbacksNew;

                    if (m_callbacksErrorEvent == null)
                    {
                        m_evtErrorEvent.OnInterrupt -= new NativeEventHandler(ErrorEventHandler);
                    }
                }
                catch
                {
                    m_callbacksErrorEvent = callbacksOld;

                    throw;
                }
            }
        }

        public event SerialDataReceivedEventHandler DataReceived
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                if (m_fDisposed)
                {
                    throw new ObjectDisposedException();
                }

                SerialDataReceivedEventHandler callbacksOld = m_callbacksDataEvent;
                SerialDataReceivedEventHandler callbacksNew = (SerialDataReceivedEventHandler)Delegate.Combine(callbacksOld, value);

                try
                {
                    m_callbacksDataEvent = callbacksNew;

                    if (callbacksOld == null && m_callbacksDataEvent != null)
                    {
                        m_evtDataEvent.OnInterrupt += new NativeEventHandler(DataEventHandler);
                    }
                }
                catch
                {
                    m_callbacksDataEvent = callbacksOld;

                    throw;
                }
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove
            {
                if (m_fDisposed)
                {
                    throw new ObjectDisposedException();
                }

                SerialDataReceivedEventHandler callbacksOld = m_callbacksDataEvent;
                SerialDataReceivedEventHandler callbacksNew = (SerialDataReceivedEventHandler)Delegate.Remove(callbacksOld, value);

                try
                {
                    m_callbacksDataEvent = callbacksNew;

                    if (m_callbacksDataEvent == null)
                    {
                        m_evtDataEvent.OnInterrupt -= new NativeEventHandler(DataEventHandler);
                    }
                }
                catch
                {
                    m_callbacksDataEvent = callbacksOld;

                    throw;
                }
            }
        }

        //--// Inter-Op Methods

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void InternalOpen();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void InternalClose();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override int Read(byte[] buffer, int offset, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override void Write(byte[] buffer, int offset, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void InternalDispose();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override void Flush();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int BytesInBuffer(bool fInput);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void DiscardBuffer(bool fInput);
    }
}


