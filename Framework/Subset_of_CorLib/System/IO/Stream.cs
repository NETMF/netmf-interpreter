////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using System.Runtime.InteropServices;

namespace System.IO
{
    [Serializable()]
    public abstract class Stream : MarshalByRefObject, IDisposable
    {
        public abstract bool CanRead
        {
            get;
        }

        // If CanSeek is false, Position, Seek, Length, and SetLength should throw.
        public abstract bool CanSeek
        {
            get;
        }

        public virtual bool CanTimeout
        {
            get
            {
                return false;
            }
        }

        public abstract bool CanWrite
        {
            get;
        }

        public abstract long Length
        {
            get;
        }

        public abstract long Position
        {
            get;
            set;
        }

        public virtual int ReadTimeout
        {
            get
            {
                throw new InvalidOperationException();
            }

            set
            {
                throw new InvalidOperationException();
            }
        }

        public virtual int WriteTimeout
        {
            get
            {
                throw new InvalidOperationException();
            }

            set
            {
                throw new InvalidOperationException();
            }
        }

        // Stream used to require that all cleanup logic went into Close(),
        // which was thought up before we invented IDisposable.  However, we
        // need to follow the IDisposable pattern so that users can write
        // sensible subclasses without needing to inspect all their base
        // classes, and without worrying about version brittleness, from a
        // base class switching to the Dispose pattern.  We're moving
        // Stream to the Dispose(bool) pattern - that's where all subclasses
        // should put their cleanup starting in V2.
        public virtual void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch
            {
            }
        }

        ~Stream()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract void Flush();
        public abstract long Seek(long offset, SeekOrigin origin);

        public abstract void SetLength(long value);

        public abstract int Read(/*[In, Out]*/ byte[] buffer, int offset, int count);

        // Reads one byte from the stream by calling Read(byte[], int, int).
        // Will return an unsigned byte cast to an int or -1 on end of stream.
        // This implementation does not perform well because it allocates a new
        // byte[] each time you call it, and should be overridden by any
        // subclass that maintains an internal buffer.  Then, it can help perf
        // significantly for people who are reading one byte at a time.
        public virtual int ReadByte()
        {
            byte[] oneByteArray = new byte[1];
            int r = Read(oneByteArray, 0, 1);
            if (r == 0)
                return -1;
            return oneByteArray[0];
        }

        public abstract void Write(byte[] buffer, int offset, int count);

        // Writes one byte from the stream by calling Write(byte[], int, int).
        // This implementation does not perform well because it allocates a new
        // byte[] each time you call it, and should be overridden by any
        // subclass that maintains an internal buffer.  Then, it can help perf
        // significantly for people who are writing one byte at a time.
        public virtual void WriteByte(byte value)
        {
            byte[] oneByteArray = new byte[1];
            oneByteArray[0] = value;
            Write(oneByteArray, 0, 1);
        }
    }
}


