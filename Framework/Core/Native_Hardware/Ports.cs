using System;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.SPOT.Hardware.SerialPort")]

namespace System.IO.Ports
{
    //--// classes

    public class SerialErrorReceivedEventArgs
    {
        SerialError m_error;

        internal SerialErrorReceivedEventArgs(SerialError error)
        {
            m_error = error;
        }

        public SerialError EventType { get { return m_error; } }
    }

    public class SerialDataReceivedEventArgs
    {
        SerialData m_data;

        internal SerialDataReceivedEventArgs(SerialData dataValue)
        {
            m_data = dataValue;
        }

        public SerialData EventType { get { return m_data; } }
    }

    //--// delegates

    public delegate void SerialErrorReceivedEventHandler(object sender, SerialErrorReceivedEventArgs e);

    public delegate void SerialDataReceivedEventHandler(Object sender, SerialDataReceivedEventArgs e);

    //--// Enumerations

    public enum Handshake : int
    {
        None = 0x00,
        XOnXOff = 0x18,
        RequestToSend = 0x06,
    }

    public enum Parity : int
    {
        None = 0,
        Odd = 1,
        Even = 2,
        Mark = 3,
        Space = 4,
    }

    public enum StopBits : int
    {
        None = 0,
        One = 1,
        Two = 2,
        OnePointFive = 3,
    }

    public class Serial
    {
        public const string COM1 = "COM1";
        public const string COM2 = "COM2";
        public const string COM3 = "COM3";
    }

    public enum BaudRate : int
    {
        BaudrateNONE = 0,
        Baudrate75 = 75,
        Baudrate150 = 150,
        Baudrate300 = 300,
        Baudrate600 = 600,
        Baudrate1200 = 1200,
        Baudrate2400 = 2400,
        Baudrate4800 = 4800,
        Baudrate9600 = 9600,
        Baudrate19200 = 19200,
        Baudrate38400 = 38400,
        Baudrate57600 = 57600,
        Baudrate115200 = 115200,
        Baudrate230400 = 230400,
    }

    public enum SerialError
    {
        TXFull,     // The application tried to transmit a character, but the output buffer was full.
        RXOver,     // An input buffer overflow has occurred. There is either no room in the input buffer, or a character was received after the end-of-file (EOF) character.
        Overrun,    // A character-buffer overrun has occurred. The next character is lost.
        RXParity,   // The hardware detected a parity error.
        Frame,      // The hardware detected a framing error.
    }

    public enum SerialData
    {
        Chars, // A character was received and placed in the input buffer.
        Eof,   // The end of file character was received and placed in the input buffer.
    }

    //--//

    internal static class SerialPortName
    {
        internal static int ConvertNameToIndex(string portName)
        {
            int portIndex = 0;

            const string c_InvalidSerialPortName = "Invalid serial port name";

            if (portName == null) throw new ArgumentNullException();

            string ucName = portName.ToUpper();

            if (ucName.Length < 4 || ucName[0] != 'C' || ucName[1] != 'O' || ucName[2] != 'M') throw new ArgumentException(c_InvalidSerialPortName);

            string portNumber = portName.Substring(3, portName.Length - 3);

            for (int i = 0; i < portNumber.Length; i++)
            {
                portIndex *= 10;

                int val = portNumber[i] - '0';

                if (val < 0 || val > 9) throw new ArgumentException(c_InvalidSerialPortName);

                portIndex += val;
            }

            if (portIndex <= 0) throw new ArgumentException(c_InvalidSerialPortName);

            return portIndex - 1; // zero-based index
        }
    }
}


