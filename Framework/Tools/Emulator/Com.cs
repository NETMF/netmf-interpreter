////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

using Microsoft.SPOT.Emulator.Events;
using System.Collections;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Emulator.Com
{
    //keep in sync with hal\include\tinyhal.h
    public enum TransportType : byte
    {
        Invalid = 0,
        Usart = 1,
        Debug = 4,
        Messaging = 7,
    }

    public struct ComPortHandle
    {
        TransportType _transportType;
        int _portNumber;

        public ComPortHandle( TransportType transportType, int portNumber )
        {
            if (Enum.IsDefined( typeof( TransportType ), transportType ) == false ||
                transportType == TransportType.Invalid)
            {
                throw new ArgumentException( "Invalid Transport Type." );
            }

            if (portNumber <= 0)
            {
                throw new ArgumentException( "Invalid port number: " + portNumber );
            }

            _transportType = transportType;
            _portNumber = portNumber;
        }

        public static ComPortHandle ConvertFromNative( int nativeHandle )
        {
            return new ComPortHandle( (TransportType)((nativeHandle >> 8) & 0xFF), (int)(nativeHandle & 0xFF) );
        }

        public TransportType TransportType
        {
            get { return _transportType; }
        }

        public int PortNumber
        {
            get { return _portNumber; }
        }

        public static ComPortHandle Parse( String value )
        {
            String[] transportTypeStrings = Enum.GetNames( typeof( TransportType ) );

            TransportType transportType = TransportType.Invalid;
            int portNumberIndex = 0;
            int portNumber;

            foreach (String type in transportTypeStrings)
            {
                if (value.StartsWith( type, StringComparison.CurrentCultureIgnoreCase ))
                {
                    transportType = (TransportType)Enum.Parse( typeof( TransportType ), type, true );
                    portNumberIndex = type.Length;
                    break;
                }
            }

            if (transportType == TransportType.Invalid ||
                Int32.TryParse( value.Substring( portNumberIndex ), out portNumber ) == false)
            {
                throw new FormatException( "Unrecognized Com port handle: " + value );
            }

            try
            {
                return new ComPortHandle( transportType, portNumber );
            }
            catch (ArgumentException)
            {
                throw new FormatException( "Unrecognized Com port handle: " + value );
            }
        }

        public static bool operator ==(ComPortHandle a, ComPortHandle b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals( a, b ))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((Object)a) == null || ((Object)b) == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (a.TransportType == b.TransportType) && (a.PortNumber == b.PortNumber);
        }

        public static bool operator !=( ComPortHandle a, ComPortHandle b )
        {
            return !(a == b);
        }

        public override bool Equals( Object o )
        {
            if (o is ComPortHandle)
            {
                return (this == (ComPortHandle)o);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (((int)_transportType) << 8) + ((int)_portNumber);
        }

        public override String ToString()
        {
            return _transportType.ToString() + _portNumber.ToString();
        }

        public static readonly ComPortHandle DebugPortRpc = new ComPortHandle( TransportType.Messaging, 1 ); 
        public static readonly ComPortHandle DebugPort    = new ComPortHandle( TransportType.Debug,     1 );
    }

    internal class ComDriver : HalDriver<IComDriver>, IComDriver
    {
        private ComPort GetComPort( int nativeHandle )
        {
            return this.Emulator.ComPortCollection.DeviceGet( ComPortHandle.ConvertFromNative( nativeHandle ) );
        }

        #region IComDriver

        bool IComDriver.Initialize( int ComHandle )
        {
            ComPort port = GetComPort( ComHandle );
            bool retVal = true;

            if(port is ISerialPortToStream)
            {
                retVal = ((ISerialPortToStream)port).Initialize( 115200, 0, 8, 1, 0 );
            }
            
            return port.DeviceInitialize() && retVal;
        }

        bool IComDriver.Uninitialize( int ComHandle )
        {
            ComPort port = GetComPort( ComHandle );
            bool retVal = true;

            if(port is ISerialPortToStream)
            {
                retVal = ((ISerialPortToStream)port).Uninitialize();
            }

            return port.DeviceUninitialize() && retVal;
        }

        int IComDriver.Write( int ComHandle, IntPtr Data, uint size )
        {
            byte[] buffer = Utility.MarshalBytes(Data, (int)size);

            return GetComPort( ComHandle ).DeviceWrite( buffer );
        }

        int IComDriver.Read( int ComHandle, IntPtr Data, uint size )
        {
            byte[] buffer = new byte[size];

            int bytesRead = GetComPort( ComHandle ).DeviceRead( buffer );

            if (bytesRead > 0)
            {
                Marshal.Copy(buffer, 0, Data, bytesRead);

                return bytesRead;
            }
            
            return 0;
        }

        bool IComDriver.Flush( int ComHandle )
        {
            return GetComPort( ComHandle ).DeviceFlush();
        }
        
        #endregion
    }

    public class ComPortCollection : EmulatorComponentCollection
    {
        protected Dictionary<ComPortHandle, ComPort> _ports;
        private List<ComPort> _validPorts;

        public ComPortCollection() 
            : base(typeof(ComPort))
        {
            _ports = new Dictionary<ComPortHandle, ComPort>();
            _validPorts = new List<ComPort>();
        }

        internal ComPort DeviceGet( ComPortHandle portHandle )
        {
            ComPort port;

            if (_ports.TryGetValue( portHandle, out port ) == false)
            {
                port = new ComPortNull( portHandle );
                _ports.Add( portHandle, port );
            }

            if (port is ComPortNull && portHandle.TransportType != TransportType.Debug)
            {
                ComPortNull nullPort = (ComPortNull)port;

                if (nullPort.DisplayWarning || Emulator.Verbose)
                {
                    Trace.WriteLine( "Warning: System attempts to access COM Port " + portHandle + " that was not configured." );
                    nullPort.TurnOffWarning();
                }
            }

            return port;
        }

        public ComPort this[ComPortHandle portHandle]
        {
            get
            {
                if (Exists( portHandle ) == false)
                {
                    throw new ArgumentException( "ComPort " + portHandle + " does not exist." );
                }

                return _ports[portHandle];
            }
        }

        internal List<ComPort> GetAll(TransportType type)
        {
            return _validPorts.FindAll(delegate(ComPort p) { return (p.ComPortHandle.TransportType == type); });
        }

        internal override void RegisterInternal(EmulatorComponent ec)
        {
            VerifyAccess();

            ComPort comPort = ec as ComPort;

            if (comPort == null)
            {
                throw new Exception( "Attempt to register a non ComPort with ComPortCollection." );
            }

            ComPortHandle handle = comPort.ComPortHandle;

            Debug.Assert( !(comPort is ComPortNull) );

            if (Exists( handle ))
                throw new Exception( "Duplicate " + handle + " ComPorts found." );

            _ports[handle] = comPort;
            _validPorts.Add(comPort);

            base.RegisterInternal(ec);
        }

        internal override void UnregisterInternal(EmulatorComponent ec)
        {
            VerifyAccess();

            ComPort comPort = ec as ComPort;

            if (comPort != null)
            {
                if (_ports[comPort.ComPortHandle] == comPort)
                {
                    _ports.Remove(comPort.ComPortHandle);
                    _validPorts.Remove(comPort);

                    base.UnregisterInternal(ec);
                }
            }
        }

        private bool Exists( ComPortHandle handle )
        {
            return _ports.ContainsKey( handle ) && ((_ports[handle] is ComPortNull) == false);
        }

        public override IEnumerator GetEnumerator()
        {
            return _validPorts.GetEnumerator();
        }

        public override int Count
        {
            get { return _validPorts.Count; }
        }

        public override void CopyTo(Array array, int index)
        {
            ComPort[] comPortArray = array as ComPort[];
            if (comPortArray == null)
            {
                throw new ArgumentException("Cannot cast array into ComPort[]");
            }
            _validPorts.CopyTo(comPortArray, index);
        }
    }

    public abstract class ComPort : EmulatorComponent
    {
        internal Events.SystemEvents _systemEventRead = Events.SystemEvents.NONE;        
        private ComPortHandle _handle;
        protected bool _supportNonStandardBaudRate;
        protected uint _maxBaudrateHz;
        protected uint _minBaudrateHz;
        protected Cpu.Pin _rxPin = Cpu.Pin.GPIO_NONE;
        protected Cpu.Pin _txPin = Cpu.Pin.GPIO_NONE;
        protected Cpu.Pin _ctsPin = Cpu.Pin.GPIO_NONE;
        protected Cpu.Pin _rtsPin = Cpu.Pin.GPIO_NONE;
        
        internal virtual bool DeviceInitialize()
        {
            return true;
        }

        internal virtual bool DeviceUninitialize()
        {
            return true;
        }

        internal abstract int DeviceWrite(byte[] Data);
        internal abstract int DeviceRead(byte[] Data);
        internal abstract bool DeviceFlush();

        public ComPortHandle ComPortHandle
        {
            get { return _handle; }
            set 
            {
                ThrowIfNotConfigurable();
                _handle = value;

                switch(_handle.TransportType)
                {
                    case TransportType.Usart:
                        _systemEventRead = Events.SystemEvents.COM_IN;
                        break;
                    case TransportType.Debug:
                        _systemEventRead = Events.SystemEvents.DEBUGGER_ACTIVITY;
                        break;
                    default:
                        _systemEventRead = Events.SystemEvents.NONE;
                        break;
                }
            }
        }

        public override void SetupComponent()
        {
            if (_handle.TransportType == TransportType.Invalid)
            {
                throw new Exception( "Invalid ComPort component: the ComPortHandle needs to be set." );
            }
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            if (ec is ComPort)
            {
                return (((ComPort)ec).ComPortHandle == this.ComPortHandle);
            }
            else
            {
                return false;
            }
        }

        public bool SupportNonStandardBaudRate
        {
            get { return _supportNonStandardBaudRate; }
            set { ThrowIfNotConfigurable(); _supportNonStandardBaudRate = value; }
        }

        public uint MaxBaudrateHz
        {
            get { return _maxBaudrateHz; }
            set { ThrowIfNotConfigurable(); _maxBaudrateHz = value; }
        }

        public uint MinBaudrateHz
        {
            get { return _minBaudrateHz; }
            set { ThrowIfNotConfigurable(); _minBaudrateHz = value; }
        }

        public Cpu.Pin RxPin
        {
            get { return _rxPin; }
            set { ThrowIfNotConfigurable(); _rxPin = value; }
        }

        public Cpu.Pin TxPin
        {
            get { return _txPin; }
            set { ThrowIfNotConfigurable(); _txPin = value; }
        }

        public Cpu.Pin RtsPin
        {
            get { return _rtsPin; }
            set { ThrowIfNotConfigurable(); _rtsPin = value; }
        }

        public Cpu.Pin CtsPin
        {
            get { return _ctsPin; }
            set { ThrowIfNotConfigurable(); _ctsPin = value; }
        }
    }

    internal sealed class ComPortNull : ComPort
    {
        internal ComPortNull( ComPortHandle handle )
        {
            this.ComPortHandle = handle;
        }

        internal override bool DeviceInitialize()
        {   
            return false;
        }

        internal override bool DeviceUninitialize()
        {
            return true;
        }

        internal override int DeviceWrite( byte[] Data )
        {
            return Data.Length;
        }

        internal override int DeviceRead( byte[] Data )
        {
            return 0;
        }

        internal override bool DeviceFlush()
        {
            return true;
        }

        private bool _displayWarning = true;

        internal bool DisplayWarning
        {
            get { return _displayWarning; }
        }

        internal void TurnOffWarning()
        {
            _displayWarning = false;
        }
    }

    public abstract class ComPortToStream : ComPort
    {
        protected Stream _stream;
        bool _isInitialized;
        byte[] _buffer;
        IAsyncResult _asyncResult;
        bool _isReadFailure = false;

        internal FifoBuffer _fifoToDevice;  //From emulator to device

        public ComPortToStream()
        {
            _fifoToDevice = new FifoBuffer();
            _buffer = new byte[512];
        }

        public ComPortToStream(Stream stream)
            : this()
        {
            _stream = stream;
        }

        public virtual Stream Stream
        {
            get { return _stream; }
            set { _stream = value; }
        }

        protected virtual void InitializeProtected()
        {
        }

        protected virtual void UninitializeProtected()
        {
            if (_stream != null)
            {
                _stream.Flush();
            }
        }

        protected void StartRead()
        {
            try
            {
                _asyncResult = _stream.BeginRead(_buffer, 0, 1 , new AsyncCallback(OnReadComplete), null);
            }
            catch
            {
                _isReadFailure = true;
                OnRead();
            }
        }
        
        protected virtual void OnRead()
        {
            this.Emulator.SetSystemEvents(_systemEventRead);
        }

        private void OnReadComplete(IAsyncResult ar)
        {
            if (!_isInitialized || _stream == null)
            {
                OnRead();
                return;
            }

            try
            {
                int bytesRead = _stream.EndRead(ar);

                int totalBytesRead = 0;

                if (bytesRead == 0)
                {
                    _isReadFailure = true;
                    OnRead();
                    return;
                }

                while (bytesRead > 0)
                {
                    totalBytesRead += bytesRead;

                    _fifoToDevice.Write(_buffer, 0, bytesRead);

                    bytesRead = Math.Min(this.AvailableBytes, _buffer.Length);

                    if (bytesRead > 0)
                    {
                        bytesRead = _stream.Read(_buffer, 0, bytesRead);
                    }
                }

                if (totalBytesRead > 0)
                {
                    OnRead();
                }

                _asyncResult = null;
                StartRead();
            }
            catch
            {
                _isReadFailure = true;
                OnRead();
            }
        }

        public virtual int AvailableBytes
        {
            get
            {
                return 0;
            }
        }

        #region ComPort abstract Members

        internal override bool DeviceInitialize()
        {
            base.DeviceInitialize();

            if (!_isInitialized)
            {
                _fifoToDevice = new FifoBuffer();

                InitializeProtected();

                _isInitialized = true;

                StartRead();
            }

            return true;
        }

        internal override bool DeviceUninitialize()
        {
            if (_isInitialized)
            {
                _isInitialized = false;

                UninitializeProtected();
            }

            base.DeviceUninitialize();

            return true;
        }

        internal override int DeviceWrite( byte[] Data )
        {
            if (!_isInitialized) return -1;

            try
            {
                _stream.Write(Data, 0, Data.Length);
            }
            catch
            {
                return -1;
            }

            return Data.Length;
        }

        internal override int DeviceRead( byte[] Data )
        {
            if (!_isInitialized) return -1;

            if(_isReadFailure)
            {
                // retry reading, and let the user determine when to close
                _isReadFailure = false;
                _asyncResult = null;
                StartRead();

                return -1;
            }

            int bytesRead = _fifoToDevice.Read(Data, 0, Data.Length);

            return bytesRead;
        }

        internal override bool DeviceFlush()
        {
            if (!_isInitialized) return false;

            if (_stream != null)
                _stream.Flush();

            return true;
        }

        #endregion
    }


    public class ComPortToAsyncStream : ComPortToStream
    {
        public ComPortToAsyncStream()
        {
        }

        public ComPortToAsyncStream(Stream stream)
            : base(stream)
        {
        }

        public override int AvailableBytes
        {
            get
            {
                GenericAsyncStream stream = _stream as GenericAsyncStream;

                int cBytes = 0;

                if (stream != null)
                {
                    cBytes = stream.AvailableCharacters;
                }

                return cBytes;                
            }
        }

        protected override void UninitializeProtected()
        {
            base.UninitializeProtected();

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }
    }

    public class ComPortToNamedPipe : ComPortToAsyncStream
    {
        string _fileName;
        bool _isInitialized = false;

        public ComPortToNamedPipe()
        {
        }

        public ComPortToNamedPipe(string fileName)
        {
            _fileName = fileName;
        }

        public ComPortToNamedPipe(string fileName, Stream stream) : base(stream)
        {
            _fileName = fileName;
        }

        internal override bool DeviceInitialize()
        {
            ///
            /// Only initialize NamedPipe once!!!
            ///
            if(!_isInitialized)
            {
                _isInitialized = base.DeviceInitialize();
            }

            return _isInitialized;
        }

        internal override bool DeviceUninitialize()
        {
            ///
            /// Never uninitialize (this method is called during soft reboots and re-initializing the NamedPipe will kill the emulator)
            ///
            return true;
        }

        protected override void InitializeProtected()
        {
            if (_stream == null)
            {
                _stream = new AsyncFileStream(_fileName, FileShare.ReadWrite);
            }
        }

        public virtual string Filename
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
    }

    public class ComPortToMemoryStream : ComPortToStream
    {
        FifoBuffer _fifoIn;  //From emulator to device
        FifoBuffer _fifoOut; //From device to emulator

        ComPortStream _streamIn;  //Stream from SerialPortToStream to device
        ComPortStream _streamOut; //Stream from emulator model to SerialPortToStream

        public ComPortToMemoryStream()
            : base(null)
        {
            _fifoIn = new FifoBuffer();
            _fifoOut = new FifoBuffer();

            _streamIn = new ComPortStream(_fifoIn, _fifoOut);
            _streamOut = new ComPortStream(_fifoOut, _fifoIn);

            _stream = _streamIn;
        }

        public override int AvailableBytes
        {
            get { return ((ComPortStream)_stream).AvailableBytes; }
        }

        public Stream StreamOut
        {
            get { return _streamOut; }
        }

        private class ComPortStream : Stream
        {
            FifoBuffer _fifoRead;
            FifoBuffer _fifoWrite;
            AsyncResultRead _asyncResultRead;

            public class AsyncResultRead : IAsyncResult
            {
                bool _completed;
                ComPortStream _stream;
                ManualResetEvent _mre;
                int _bytesRead;

                byte[] _buffer;
                int _offset;
                int _count;
                AsyncCallback _callback;
                object _state;

                public AsyncResultRead(ComPortStream stream, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    _stream = stream;
                    _buffer = buffer;
                    _offset = offset;
                    _count = count;
                    _callback = callback;
                    _state = state;

                    _mre = new ManualResetEvent(false);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(ReadCallback));
                }

                void ReadCallback(object state)
                {
                    FifoBuffer fifoBuffer = _stream._fifoRead;

                    fifoBuffer.WaitHandle.WaitOne();

                    int bytesRead = Math.Min(fifoBuffer.Available, _count);
                    _bytesRead = fifoBuffer.Read(_buffer, _offset, bytesRead);

                    _completed = true;
                    _mre.Set();

                    if (_callback != null)
                        _callback(this);
                }

                internal int EndRead()
                {
                    _mre.WaitOne();

                    return _bytesRead;
                }

                #region IAsyncResult Members

                object IAsyncResult.AsyncState
                {
                    get { return _state; }
                }

                System.Threading.WaitHandle IAsyncResult.AsyncWaitHandle
                {
                    get { return _mre; }
                }

                bool IAsyncResult.CompletedSynchronously
                {
                    get { return false; }
                }

                bool IAsyncResult.IsCompleted
                {
                    get { return _completed; }
                }

                #endregion
            }

            public ComPortStream(FifoBuffer fifoRead, FifoBuffer fifoWrite)
            {
                _fifoRead = fifoRead;
                _fifoWrite = fifoWrite;
            }

            public int AvailableBytes
            {
                get { return _fifoRead.Available; }
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
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

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _fifoRead.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _fifoWrite.Write(buffer, offset, count);
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                if (_asyncResultRead != null)
                    throw new Exception("Only one outstanding read supported");

                _asyncResultRead = new AsyncResultRead(this, buffer, offset, count, callback, state);

                return _asyncResultRead;
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                AsyncResultRead ar = asyncResult as AsyncResultRead;

                int bytesRead = ar.EndRead();

                _asyncResultRead = null;

                return bytesRead;
            }

            protected override void Dispose(bool disposing)
            {
                //Set the _asyncResult to avoid blocking.
                base.Dispose(disposing);
            }
        }
    }

    public delegate void OnEmuSerialPortEvtHandler(int port, uint evt);

    public interface ISerialPortToStream
    {
        bool Initialize(int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue);

        bool Uninitialize();

        bool SetDataEventHandler( OnEmuSerialPortEvtHandler handler );
    }
}
