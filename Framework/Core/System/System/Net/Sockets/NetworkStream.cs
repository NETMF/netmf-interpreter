using System.IO;
using System.Net;
using System.Net.Sockets;

namespace System.Net.Sockets
{
    // Summary:
    //     Provides the underlying stream of data for network access.
    public class NetworkStream : Stream
    {

        // Summary:
        // Internal members

        // Internal Socket object
        internal Socket _socket;

        // Internal property used to store the socket type
        protected int _socketType;

        // Internal endpoint ref used for dgram sockets
        protected EndPoint _remoteEndPoint;

        // Internal flags
        private bool _ownsSocket;
        protected bool _disposed;

        // Summary:
        //     Creates a new instance of the System.Net.Sockets.NetworkStream class for
        //     the specified System.Net.Sockets.Socket.
        //
        // Parameters:
        //   socket:
        //     The System.Net.Sockets.Socket that the System.Net.Sockets.NetworkStream will
        //     use to send and receive data.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     socket is null.
        //
        //   System.IO.IOException:
        //     socket is not connected.-or- The System.Net.Sockets.Socket.SocketType property
        //     of socket is not System.Net.Sockets.SocketType.Stream.-or- socket is in a
        //     nonblocking state.
        public NetworkStream(Socket socket)
            : this(socket, false)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Net.Sockets.NetworkStream class
        //     for the specified System.Net.Sockets.Socket with the specified System.Net.Sockets.Socket
        //     ownership.
        //
        // Parameters:
        //   ownsSocket:
        //     true to indicate that the System.Net.Sockets.NetworkStream will take ownership
        //     of the System.Net.Sockets.Socket; otherwise, false.
        //
        //   socket:
        //     The System.Net.Sockets.Socket that the System.Net.Sockets.NetworkStream will
        //     use to send and receive data.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     socket is not connected.-or- The value of the System.Net.Sockets.Socket.SocketType
        //     property of socket is not System.Net.Sockets.SocketType.Stream.-or- socket
        //     is in a nonblocking state.
        //
        //   System.ArgumentNullException:
        //     socket is null.
        public NetworkStream(Socket socket, bool ownsSocket)
        {
            if (socket == null) throw new ArgumentNullException();
            
            // This should throw a SocketException if not connected
            try
            {
                _remoteEndPoint = socket.RemoteEndPoint;
            }
            catch (Exception e)
            {
                int errCode = (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error);

                throw new IOException(errCode.ToString(), e);
            }
            
            // Set the internal socket
            _socket = socket;

            _socketType = (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Type);

            _ownsSocket = ownsSocket;
        }

        // Summary:
        //     Gets a value that indicates whether the System.Net.Sockets.NetworkStream
        //     supports reading.
        //
        // Returns:
        //     true if data can be read from the stream; otherwise, false. The default value
        //     is true.
        public override bool CanRead { get { return true; } }

        //
        // Summary:
        //     Gets a value that indicates whether the stream supports seeking. This property
        //     is not currently supported.This property always returns false.
        //
        // Returns:
        //     false in all cases to indicate that System.Net.Sockets.NetworkStream cannot
        //     seek a specific location in the stream.
        public override bool CanSeek { get { return false; } }

        //
        // Summary:
        //     Indicates whether timeout properties are usable for System.Net.Sockets.NetworkStream.
        //
        // Returns:
        //     true in all cases.
        public override bool CanTimeout { get { return true; } }

        //
        // Summary:
        //     Gets a value that indicates whether the System.Net.Sockets.NetworkStream
        //     supports writing.
        //
        // Returns:
        //     true if data can be written to the System.Net.Sockets.NetworkStream; otherwise,
        //     false. The default value is true.
        public override bool CanWrite { get { return true; } }

        public override int ReadTimeout
        {
            get { return _socket.ReceiveTimeout; }
            set
            {   
                if (value == 0 || value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                _socket.ReceiveTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get { return _socket.SendTimeout; }
            set
            {
                if (value == 0 || value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                _socket.SendTimeout = value;
            }
        }

        //
        // Summary:
        //     Gets the length of the data available on the stream.
        //
        // Returns:
        //     The length of the data available on the stream.
        //
        // Exceptions:
        //     InvalidOperationException - when socket is disposed.
        public override long Length
        {
            get
            {
                if (_disposed == true) throw new ObjectDisposedException();                
                if (_socket.m_Handle == -1) throw new IOException();

                return _socket.Available;
            }
        }

        //
        // Summary:
        //     Gets or sets the current position in the stream. This property is not currently
        //     supported and always throws a System.NotSupportedException.
        //
        // Returns:
        //     The current position in the stream.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     Any use of this property.
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public virtual bool DataAvailable
        {
            get
            {
                if (_disposed == true) throw new ObjectDisposedException();     
                if (_socket.m_Handle == -1) throw new IOException();

                return (_socket.Available > 0);
            }
        }

        //
        // Summary:
        //     Closes the System.Net.Sockets.NetworkStream after waiting the specified time
        //     to allow data to be sent.
        //
        // Parameters:
        //   timeout:
        //     A 32-bit signed integer that specifies how long to wait to send any remaining
        //     data before closing.
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     timeout is less than -1.
        public void Close(int timeout)
        {
            if (timeout < -1)
                throw new ArgumentOutOfRangeException();

            System.Threading.Thread.Sleep(timeout);

            Close();
        }

        //
        // Summary:
        //     Releases the unmanaged resources used by the System.Net.Sockets.NetworkStream
        //     and optionally releases the managed resources.
        //
        // Parameters:
        //   disposing:
        //     true to release both managed and unmanaged resources; false to release only
        //     unmanaged resources.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (_ownsSocket == true)
                            _socket.Close();
                    }
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        //
        // Summary:
        //     Flushes data from the stream. This method is reserved for future use.
        public override void Flush()
        {
        }

        //
        // Summary:
        //     Reads data from the System.Net.Sockets.NetworkStream.
        //
        // Parameters:
        //   offset:
        //     The location in buffer to begin storing the data to.
        //
        //   size:
        //     The number of bytes to read from the System.Net.Sockets.NetworkStream.
        //
        //   buffer:
        //     An array of type System.Byte that is the location in memory to store data
        //     read from the System.Net.Sockets.NetworkStream.
        //
        // Returns:
        //     The number of bytes read from the System.Net.Sockets.NetworkStream.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     The underlying System.Net.Sockets.Socket is closed.
        //
        //   System.ArgumentNullException:
        //     buffer is null.
        //
        //   System.ObjectDisposedException:
        //     The System.Net.Sockets.NetworkStream is closed.-or- There is a failure reading
        //     from the network.
        //
        //   System.ArgumentOutOfRangeException:
        //     offset is less than 0.-or- offset is greater than the length of buffer.-or-
        //     size is less than 0.-or- size is greater than the length of buffer minus
        //     the value of the offset parameter. -or-An error occurred when accessing the
        //     socket. See the Remarks section for more information.
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException();            
            if (_socket.m_Handle == -1) throw new IOException();
            if (buffer == null) throw new ArgumentNullException();
            if (offset < 0 || offset > buffer.Length) throw new ArgumentOutOfRangeException();
            if (count < 0 || count > buffer.Length - offset) throw new ArgumentOutOfRangeException();

            int available = _socket.Available;

            // we will need to read using thr timeout specified
            // if there is data available we can return with that data only
            // the underlying socket infrastructure will handle the timeout
            if (count > available && available > 0)
            {
                count = available;
            }

            if (_socketType == (int)SocketType.Stream)
            {
                return _socket.Receive(buffer, offset, count, SocketFlags.None);
            }
            else if (_socketType == (int)SocketType.Dgram)
            {
                return _socket.ReceiveFrom(buffer, offset, count, SocketFlags.None, ref _remoteEndPoint);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        //
        // Summary:
        //     Sets the current position of the stream to the given value. This method is
        //     not currently supported and always throws a System.NotSupportedException.
        //
        // Parameters:
        //   offset:
        //     This parameter is not used.
        //
        //   origin:
        //     This parameter is not used.
        //
        // Returns:
        //     The position in the stream.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     Any use of this property.
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        //
        // Summary:
        //     Sets the length of the stream. This method always throws a System.NotSupportedException.
        //
        // Parameters:
        //   value:
        //     This parameter is not used.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     Any use of this property.
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        //
        // Summary:
        //     Writes data to the System.Net.Sockets.NetworkStream.
        //
        // Parameters:
        //   offset:
        //     The location in buffer from which to start writing data.
        //
        //   size:
        //     The number of bytes to write to the System.Net.Sockets.NetworkStream.
        //
        //   buffer:
        //     An array of type System.Byte that contains the data to write to the System.Net.Sockets.NetworkStream.
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     offset is less than 0.-or- offset is greater than the length of buffer.-or-
        //     size is less than 0.-or- size is greater than the length of buffer minus
        //     the value of the offset parameter.
        //
        //   System.ObjectDisposedException:
        //     The System.Net.Sockets.NetworkStream is closed.-or- There was a failure reading
        //     from the network.
        //
        //   System.IO.IOException:
        //     There was a failure while writing to the network. -or-An error occurred when
        //     accessing the socket. See the Remarks section for more information.
        //
        //   System.ArgumentNullException:
        //     buffer is null.
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException();            
            if (_socket.m_Handle == -1) throw new IOException();
            if (buffer == null) throw new ArgumentNullException();
            if (offset < 0 || offset > buffer.Length) throw new ArgumentOutOfRangeException();
            if (count < 0 || count > buffer.Length - offset) throw new ArgumentOutOfRangeException();

            int bytesSent = 0;

            if (_socketType == (int)SocketType.Stream)
            {
                bytesSent = _socket.Send(buffer, offset, count, SocketFlags.None);
            }
            else if (_socketType == (int)SocketType.Dgram)
            {
                bytesSent = _socket.SendTo(buffer, offset, count, SocketFlags.None, _socket.RemoteEndPoint);
            }
            else
            {
                throw new NotSupportedException();
            }

            if (bytesSent != count) throw new IOException();
        }
    }
}

