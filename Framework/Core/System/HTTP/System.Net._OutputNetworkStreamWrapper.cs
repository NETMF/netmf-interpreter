////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System.Net.Sockets;
    using System.IO;

    /// <summary>
    /// The OutputNetworkStreamWrapper is used to re-implement calls to  NetworkStream.Write
    /// On first write HttpListenerResponse needs to send HTTP headers to client.
    /// </summary>
    internal class OutputNetworkStreamWrapper : Stream
    {

        /// <summary>
        /// This is a socket connected to client.
        /// OutputNetworkStreamWrapper owns the socket, not NetworkStream.
        /// If connection is persistent, then the m_Socket is transferred to the list of
        /// </summary>
        internal Socket m_Socket;

        /// <summary>
        /// Actual network or SSL stream connected to the client.
        /// It could be SSL stream, so NetworkStream is not exact type, m_Stream would be derived from NetworkStream
        /// </summary>
        internal NetworkStream m_Stream;

        /// <summary>
        /// Type definition of delegate for sending of HTTP headers.
        /// </summary>
        internal delegate void SendHeadersDelegate();

        /// <summary>
        /// If not null - indicates whether we have sent headers or not.
        /// Calling of delegete sends HTTP headers to client - HttpListenerResponse.SendHeaders()
        /// </summary>
        private SendHeadersDelegate m_headersSend;

        /// <summary>
        /// Just passes parameters to the base.
        /// Socket is not owned by base NetworkStream
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="stream"></param>
        public OutputNetworkStreamWrapper(Socket socket, NetworkStream stream)
        {
            m_Socket = socket;
            m_Stream = stream;
        }

        /// <summary>
        /// Sets the delegate for sending of headers.
        /// </summary>
        internal SendHeadersDelegate HeadersDelegate { set { m_headersSend = value; } }

        /// <summary>
        /// Return true if stream support reading.
        /// </summary>
        public override bool CanRead { get { return false; } }

        /// <summary>
        /// Return true if stream supports seeking
        /// </summary>
        public override bool CanSeek { get { return false; } }

        /// <summary>
        /// Return true if timeout is applicable to the stream
        /// </summary>
        public override bool CanTimeout { get { return m_Stream.CanTimeout; } }

        /// <summary>
        /// Return true if stream support writing. It should be true, as this is output stream.
        /// </summary>
        public override bool CanWrite { get { return true; } }

        /// <summary>
        /// Gets the length of the data available on the stream.
        /// Since this is output stream reading is not allowed and length does not have meaning.
        /// </summary>
        /// <returns>The length of the data available on the stream.</returns>
        public override long Length { get { throw new NotSupportedException(); } }

        /// <summary>
        /// Position is not supported for NetworkStream
        /// </summary>
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

        /// <summary>
        /// Timeout for read operations.
        /// </summary>
        public override int ReadTimeout
        {
            get { return m_Stream.ReadTimeout; }
            set { m_Stream.ReadTimeout = value; }
        }

        /// <summary>
        /// Timeout for write operations.
        /// </summary>
        public override int WriteTimeout
        {
            get { return m_Stream.WriteTimeout; }
            set { m_Stream.WriteTimeout = value; }
        }

        /// <summary>
        /// Closes the stream. Verifies that HTTP response is sent before closing.
        /// </summary>
        public override void Close()
        {
            if (m_headersSend != null)
            {
                // Calls HttpListenerResponse.SendHeaders. HttpListenerResponse.SendHeaders sets m_headersSend to null.
                m_headersSend();
            }

            m_Stream.Close();
            m_Stream = null;
            m_Socket = null;
        }

        /// <summary>
        /// Flushes the stream. Verifies that HTTP response is sent before flushing.
        /// </summary>
        public override void Flush()
        {
            if (m_headersSend != null)
            {
                // Calls HttpListenerResponse.SendHeaders. HttpListenerResponse.SendHeaders sets m_headersSend to null.
                m_headersSend();
            }

            m_Stream.Flush();
        }

        /// <summary>
        /// This putput stream, so read is not supported.
        /// </summary>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This putput stream, so read is not supported.
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Seeking is not suported on network streams
        /// </summary>
        /// <param name="offset">Offset to seek</param>
        /// <param name="origin">Relative origin of the seek</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Setting length is not suported on network streams
        /// </summary>
        /// <param name="value">Length to set</param>
        /// <returns></returns>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes single byte to the stream.
        /// </summary>
        /// <param name="value">Byte value to write.</param>
        public override void WriteByte(byte value)
        {
            m_Stream.WriteByte(value);
        }

        /// <summary>
        /// Re-implements writing of data to network stream.
        /// The only functionality - on first write it sends HTTP headers.
        /// Then calls base
        /// </summary>
        /// <param name="buffer">Buffer with data to write to HTTP client</param>
        /// <param name="offset">Offset at which to use data from buffer</param>
        /// <param name="size">Count of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int size)
        {
            if (m_headersSend != null)
            {
                // Calls HttpListenerResponse.SendHeaders. HttpListenerResponse.SendHeaders sets m_headersSend to null.
                m_headersSend();
            }

            m_Stream.Write(buffer, offset, size);
        }
    }
}


