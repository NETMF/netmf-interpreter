using System;
using System.Text;
using System.IO;
using Microsoft.SPOT;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MFWsStack")]

namespace System.IO
{
    public class StreamWriter : TextWriter
    {
        private Stream m_stream;
        private bool m_disposed;
        private byte[] m_buffer;

        private int m_curBufPos;

        private const string c_NewLine = "\r\n";
        private const int c_BufferSize = 0xFFF;

        //--//

        public StreamWriter(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException();
            }

            m_stream = stream;
            m_buffer = new byte[c_BufferSize];
            m_curBufPos = 0;
            m_disposed = false;
        }

        public StreamWriter(String path)
            : this(path, false)
        {
        }

        public StreamWriter(String path, bool append)
            : this(new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read))
        {
        }

        public override void Close()
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (m_stream != null)
            {
                if (disposing)
                {
                    try
                    {
                        if (m_stream.CanWrite)
                        {
                            Flush();
                        }
                    }
                    catch { }

                    try
                    {
                        m_stream.Close();
                    }
                    catch {}
                }

                m_stream = null;
                m_buffer = null;
                m_curBufPos = 0;
            }

            m_disposed = true;
        }

        public override void Flush()
        {
            if (m_disposed) throw new ObjectDisposedException();

            if (m_curBufPos > 0)
            {
                try
                {
                    m_stream.Write(m_buffer, 0, m_curBufPos);
                }
                catch (Exception e)
                {
                    throw new IOException("StreamWriter Flush. ", e);
                }

                m_curBufPos = 0;
            }
        }

        public override void Write(char value)
        {
            byte[] buffer = this.Encoding.GetBytes(value.ToString());

            WriteBytes(buffer, 0, buffer.Length);
        }

        public override void WriteLine()
        {
            byte[] tempBuf = this.Encoding.GetBytes(c_NewLine);
            WriteBytes(tempBuf, 0, tempBuf.Length);
            return;
        }

        public override void WriteLine(string value)
        {
            byte[] tempBuf = this.Encoding.GetBytes(value + c_NewLine);
            WriteBytes(tempBuf, 0, tempBuf.Length);
            return;
        }

        public virtual Stream BaseStream
        {
            get
            {
                return m_stream;
            }
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        //--//

        internal void WriteBytes(byte[] buffer, int index, int count)
        {
            if (m_disposed) throw new ObjectDisposedException();

            // If this write will overrun the buffer flush the current buffer to stream and
            // write remaining bytes directly to stream.
            if (m_curBufPos + count >= c_BufferSize)
            {
                // Flush the current buffer to the stream and write new bytes
                // directly to stream.
                try
                {
                    m_stream.Write(m_buffer, 0, m_curBufPos);
                    m_curBufPos = 0;

                    m_stream.Write(buffer, index, count);
                    return;
                }
                catch (Exception e)
                {
                    throw new IOException("StreamWriter WriteBytes. ", e);
                }
            }

            // Else add bytes to the internal buffer
            Array.Copy(buffer, index, m_buffer, m_curBufPos, count);

            m_curBufPos += count;

            return;
        }
    }
}


