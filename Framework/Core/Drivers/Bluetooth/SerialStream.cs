using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;

namespace Microsoft.SPOT.Drivers.Bluetooth
{
    /// <summary>
    /// A wrapper for the class that implements the interface to the serial port.
    /// </summary>
    class SerialStream
    {
        private const int c_DefaultSerialTimeout = 10;

        //--//

        private SerialPort m_port;

        //--//

        /// <summary>
        /// Creates a new SerialStream to wrap the specified SerialPort.
        /// </summary>
        /// <param name="port">
        /// The serial port
        /// </param>
        internal SerialStream(SerialPort port)
        {
            m_port = port;
            m_port.ReadTimeout = c_DefaultSerialTimeout;
        }

        /// <summary>
        /// Reads from the serial port into a buffer until num bytes have been read or
        /// until the given timeout expires.
        /// </summary>
        /// <param name="buf">
        /// The buffer to read into
        /// </param>
        /// <param name="offset">
        /// The offset in the buffer at which to start reading into
        /// </param>
        /// <param name="num">
        /// The number of bytes to read
        /// </param>
        /// <param name="to">
        /// The timeout
        /// </param>
        /// <returns>
        /// The number of bytes read
        /// </returns>
        internal int Read(byte[] buf, int offset, int num, int to)
        {
            DateTime start = DateTime.Now;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, to);

            int numRead = offset;
            int numToRead = num;

            do
            {
                int len = m_port.Read(buf, numRead, numToRead);

                numToRead -= len;
                numRead += len;

            } while (0 < numToRead && DateTime.Now - start < ts);

            return numRead - offset;
        }

        /// <summary>
        /// Writes the given buffer to the serial port.
        /// </summary>
        /// <param name="buf">
        /// The buffer to write to the serial port
        /// </param>
        internal void Write(byte[] buf)
        {
            m_port.Write(buf, 0, buf.Length);
            m_port.Flush();
        }

        /// <summary>
        /// Clears the serial port's read buffer.
        /// </summary>
        internal void Clear(byte[] buf)
        {
            while (m_port.Read(buf, 0, buf.Length) > 0) ;
        }

    } // end class SerialStream
} // end namespace


