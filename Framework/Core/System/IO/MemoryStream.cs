using System;

namespace System.IO
{
    public class MemoryStream : Stream
    {
        private byte[] _buffer;    // Either allocated internally or externally.
        private int _origin;       // For user-provided arrays, start at this origin
        private int _position;     // read/write head.
        private int _length;       // Number of bytes within the memory stream
        private int _capacity;     // length of usable portion of buffer for stream
        private bool _expandable;  // User-provided buffers aren't expandable.
        private bool _isOpen;      // Is this stream open or closed?

        private const int MemStreamMaxLength = 0xFFFF;

        public MemoryStream()
        {
            _buffer = new byte[256];
            _capacity = 256;
            _expandable = true;
            _origin = 0;      // Must be 0 for byte[]'s created by MemoryStream
            _isOpen = true;
        }

        public MemoryStream(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            _buffer = buffer;
            _length = _capacity = buffer.Length;
            _expandable = false;
            _origin = 0;
            _isOpen = true;
        }

        public override bool CanRead
        {
            get { return _isOpen; }
        }

        public override bool CanSeek
        {
            get { return _isOpen; }
        }

        public override bool CanWrite
        {
            get { return _isOpen; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isOpen = false;
            }
        }

        // returns a bool saying whether we allocated a new array.
        private bool EnsureCapacity(int value)
        {
            if (value > _capacity)
            {
                int newCapacity = value;
                if (newCapacity < 256)
                    newCapacity = 256;
                if (newCapacity < _capacity * 2)
                    newCapacity = _capacity * 2;

                if (!_expandable && newCapacity > _capacity) throw new NotSupportedException();
                if (newCapacity > 0)
                {
                    byte[] newBuffer = new byte[newCapacity];
                    if (_length > 0) Array.Copy(_buffer, 0, newBuffer, 0, _length);
                    _buffer = newBuffer;
                }
                else
                {
                    _buffer = null;
                }

                _capacity = newCapacity;

                return true;
            }

            return false;
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get
            {
                if (!_isOpen) throw new ObjectDisposedException();
                return _length - _origin;
            }
        }

        public override long Position
        {
            get
            {
                if (!_isOpen) throw new ObjectDisposedException();
                return _position - _origin;
            }

            set
            {
                if (!_isOpen) throw new ObjectDisposedException();
                if (value < 0 || value > MemStreamMaxLength)
                    throw new ArgumentOutOfRangeException(/*"value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
                _position = _origin + (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (buffer == null)
                throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
            if (buffer.Length - offset < count)
                throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidOffLen")*/);

            int n = _length - _position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;

            Array.Copy(_buffer, _position, buffer, offset, n);
            _position += n;
            return n;
        }

        public override int ReadByte()
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (_position >= _length) return -1;
            return _buffer[_position++];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (offset > MemStreamMaxLength)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength")*/);
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    _position = _origin + (int)offset;
                    break;

                case SeekOrigin.Current:
                    if (offset + _position < _origin)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    _position += (int)offset;
                    break;

                case SeekOrigin.End:
                    if (_length + offset < _origin)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    _position = _length + (int)offset;
                    break;

                default:
                    throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidSeekOrigin")*/);
            }

            return _position;
        }

        /*
         * Sets the length of the stream to a given value.  The new
         * value must be nonnegative and less than the space remaining in
         * the array, <var>Int32.MaxValue</var> - <var>origin</var>
         * Origin is 0 in all cases other than a MemoryStream created on
         * top of an existing array and a specific starting offset was passed
         * into the MemoryStream constructor.  The upper bounds prevents any
         * situations where a stream may be created on top of an array then
         * the stream is made longer than the maximum possible length of the
         * array (<var>Int32.MaxValue</var>).
         *
         * @exception ArgumentException Thrown if value is negative or is
         * greater than Int32.MaxValue - the origin
         * @exception NotSupportedException Thrown if the stream is readonly.
         */
        public override void SetLength(long value)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (value > MemStreamMaxLength || value < 0)
                throw new ArgumentOutOfRangeException(/*"value", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength")*/);

            int newLength = _origin + (int)value;
            bool allocatedNewArray = EnsureCapacity(newLength);
            if (!allocatedNewArray && newLength > _length)
                Array.Clear(_buffer, _length, newLength - _length);
            _length = newLength;
            if (_position > newLength) _position = newLength;
        }

        public virtual byte[] ToArray()
        {
            byte[] copy = new byte[_length - _origin];
            Array.Copy(_buffer, _origin, copy, 0, _length - _origin);
            return copy;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (buffer == null)
                throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
            if (buffer.Length - offset < count)
                throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidOffLen")*/);

            int i = _position + count;
            // Check for overflow

            if (i > _length)
            {
                if (i > _capacity) EnsureCapacity(i);
                _length = i;
            }

            Array.Copy(buffer, offset, _buffer, _position, count);
            _position = i;
            return;
        }

        public override void WriteByte(byte value)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (_position >= _capacity)
            {
                EnsureCapacity(_position + 1);
            }

            _buffer[_position++] = value;

            if (_position > _length)
            {
                _length = _position;
            }
        }

        /*
         * Writes this MemoryStream to another stream.
         * @param stream Stream to write into
         * @exception ArgumentNullException if stream is null.
         */
        public virtual void WriteTo(Stream stream)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (stream == null)
                throw new ArgumentNullException(/*"stream", Environment.GetResourceString("ArgumentNull_Stream")*/);
            stream.Write(_buffer, _origin, _length - _origin);
        }
    }
}


