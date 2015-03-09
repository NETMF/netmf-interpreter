using System;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;

namespace Microsoft.SPOT.Drivers.Bluetooth
{
    /// <summary>
    /// A managed Bluetooth driver for the Promi-ESD Bluetooth chip.
    /// See chip documentation at: \\spo\dotNetMF\Wireless\Bluetooth\Hardware\IC\Promi-ESD-02
    ///
    /// For the time being all functions operate in a synchronous fashion.
    /// </summary>
    public class PromiSDBTDriver : BTDriver, IDisposable, IPowerState
    {
        /// <summary>
        /// Struct that represents an SD chip's response to an AT command.
        /// </summary>
        internal struct SDResponse
        {
            internal int Type;
            internal byte[] Data;
        };

        /// <summary>
        /// The interface between the SD chip and the driver; controls IO to and from the
        /// SD and implements a buffer/parser that is optimized for search, parsing,
        /// and storage of Promi-SD responses and Bluetooth data.
        /// </summary>
        /// <remarks>
        /// Note that when in ONLINE mode, the controller assumes the stream of
        /// incoming data may be interrupted and ended by a "\r\nDISCONNECT\r\n" message.
        /// When not in ONLINE mode (STANDBY), the controller assumes that the stream of
        /// responses coming from the SD will be delimited by "\r\n\r\n" and that each
        /// will be of a format described in Appendix A of the Promi SD Manual.
        /// - example SD responses: "\r\nOK\r\n", "\r\nBD_ADDR,Name,Mode,Status,Auth,Encryp\r\n"
        /// </remarks>
        class SDIOController
        {
            internal bool OpTimedOut;       // True if the last operation timed out

            //--//

            // A map of command codes to commands as byte arrays
            private static object[] s_Cmds = new object[] {
                // AT Commands
                new byte[] { 65, 84, 13 },                                            // AT\r
                new byte[] { 65, 84, 90, 13 },                                        // ATZ\r
                new byte[] { 65, 84, 43, 66, 84, 73, 78, 81, 63, 13 },                // AT+BTINQ?\r
                new byte[] { 65, 84, 68, 32 },                                        // ATD <BD_ADDR>\r
                new byte[] { 65, 84, 68, 13 },                                        // ATD\r
                new byte[] { 65, 84, 43, 66, 84, 83, 67, 65, 78, 44 },                // AT+BTSCAN,
                new byte[] { 65, 84, 43, 66, 84, 67, 65, 78, 67, 69, 76, 13 },        // AT+BTCANCEL\r
                new byte[] { 43, 43, 43, 13 },                                        // +++\r
                new byte[] { 65, 84, 79, 13 },                                        // ATO\r
                new byte[] { 65, 84, 72, 13 },                                        // ATH\r
                new byte[] { 65, 84, 43, 66, 84, 76, 65, 83, 84, 63, 13 },            // AT+BTLAST?\r
                new byte[] { 65, 84, 43, 66, 84, 78, 65, 77, 69, 61 },                // AT+BTNAME="
                new byte[] { 65, 84, 43, 66, 84, 75, 69, 89, 61 },                    // AT+BTKEY="
                new byte[] { 65, 84, 43, 66, 84, 73, 78, 70, 79, 63, 13 },            // AT+BTINFO?\r
                new byte[] { 65, 84, 43, 66, 84, 76, 80, 77, 44 },                    // AT+BTLPM,
                new byte[] { 65, 84, 43, 85, 65, 82, 84, 67, 79, 78, 70, 73, 71, 44 },// AT+UARTCONFIG,

                // AT command responses
                new byte[] { 13, 10 },                                                // \r\n
                new byte[] { 13, 10, 79, 75, 13, 10 },                                // \r\nOK\r\n
                new byte[] { 13, 10, 69, 82, 82, 79, 82, 13, 10 },                    // \r\nERROR\r\n
                new byte[] { 13, 10, 67, 79, 78, 78, 69, 67, 84 },                    // \r\nCONNECT\r\n
                new byte[] { 13, 10, 68, 73, 83, 67, 79, 78, 78, 69, 67, 84, 13, 10 } // \r\nDISCONNECT\r\n
            };

            private const int c_DefaultReadTimeout = 50;   // Default timeout for a read from the SerialStream
            private const int c_DefaultBuffLen = 256;   // Default length of read buffer
            private const int c_maxResponsePerCmd = 12;   // Max responses per command, see Promi manual

            //--//

            private SerialStream m_stream;                  // The SerialStream for communicating with the SD
            private ArrayList m_responseQueue;           // A queue to hold responses read from the SD

            private byte[] m_readBuffer;              // The buffer to read SD responses into
            private int m_bytesInRead;             // The number of bytes currently in the read buffer

            /// <summary>
            /// Creates a new SDIOController for a Promi SD Bluetooth chip attached
            /// to the specified serial port.
            /// </summary>
            /// <param name="com">
            /// COM number of the serial port
            /// </param>
            /// <param name="baud">
            /// Baud rate for the serial port
            /// </param>
            internal SDIOController(SerialPort port)
            {
                m_stream = new SerialStream(port);

                m_responseQueue = new ArrayList();
                m_readBuffer = new byte[c_DefaultBuffLen];
                m_bytesInRead = 0;

                OpTimedOut = false;
            }

            /// <summary>
            /// Writes an AT command to the SD chip and if requested by the user, checks
            /// whether a response received within the timeout is equal to "\r\nOK\r\n".
            /// </summary>
            /// <param name="check">
            /// Whether or not to check for "\r\nOK\r\n"
            /// </param>
            /// <param name="timeout">
            /// The timeout in ms
            /// </param>
            /// <param name="c">
            /// Index into s_Cmds, the specified result response
            /// </param>
            /// <param name="bytes">
            /// An array of byte and byte[] to send to write after the command
            /// </param>
            /// <returns>
            /// True if write (and check) succeeds
            /// </returns>
            /// <remarks>
            /// Note that when check is set that it is assumed that the written command
            /// will generate a response of either "\r\nOK\r\n" or "\r\nERROR\r\n" immediately.
            /// </remarks>
            internal bool WriteCmd(bool check, int timeout, int c, params object[] bytes)
            {
                ClearAll();

                m_stream.Write((byte[])s_Cmds[c]);

                if (null != bytes)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (typeof(byte[]) == bytes[i].GetType())
                        {
                            m_stream.Write((byte[])bytes[i]);
                        }
                        else
                        {
                            m_stream.Write(new byte[] { (byte)bytes[i] });
                        }
                    }
                }

                OpTimedOut = false;

                if (check)
                {
                    int okLen = ((byte[])s_Cmds[c_ATR_OK]).Length;

                    int numRead = m_stream.Read(m_readBuffer, 0, okLen, timeout);

                    if (okLen == numRead)
                    {
                        if (c_CR == m_readBuffer[4])
                        {   // Was "\r\nOK\r\n" so return true
                            return true;
                        }
                        else
                        {   // Was "\r\nERROR\r\n"
                            ClearAll();
                            return false;
                        }
                    }

                    OpTimedOut = true;
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Waits for a result response (OK, ERROR, CONNECT, DISCONNECT) for
            /// up to the given timeout.
            /// </summary>
            /// <param name="timeout">
            /// The timeout in ms
            /// </param>
            /// <returns>
            /// The result response code if result response is found, -1 o/w
            /// </returns>
            /// <remarks>
            /// This function is intended to be called after a single AT command, it
            /// expects that no other commands are issued while it executes and
            /// that m_readBuffer was cleared prior to issuing the AT command.
            /// </remarks>
            internal int WaitForResultResponse(int timeout)
            {
                int response = WaitForResultResponseHelper(timeout, 0);

                OpTimedOut = (0 < response) ? false : true;

                return response;
            }

            /// <summary>
            /// Gets the next response from the response queue.
            /// </summary>
            /// <returns>
            /// The next response, or null if there is none
            /// </returns>
            internal byte[] ReadResponse()
            {
                if (0 == m_responseQueue.Count) return null;

                byte[] response = ((SDResponse)m_responseQueue[0]).Data;

                m_responseQueue.RemoveAt(0);

                return response;

            }

            /// <summary>
            /// An online read; reads up to len bytes from the SD into the given buffer
            /// and stops if the given timeout expires.  In the case of a disconnect,
            /// reads up to len chars and returns them, if that's not possible then
            /// returns up to the "\r\nDISCONNECT\r\n" message and indicates disconnect
            /// by returning -1.  Sets buffer to empty if no data could be read.
            /// </summary>
            /// <param name="buf">
            /// The buffer to read into
            /// </param>
            /// <param name="to">
            /// The max amount of time to read
            /// </param>
            /// <returns>
            /// The number of bytes read, or -1 on disconnect
            /// </returns>
            internal int Read(ref byte[] buf, int to)
            {
                // Check for a disconnect
                if (0 < m_responseQueue.Count) return -1;

                // Take bytes from the read buffer if there are any
                int bytesRead = 0;
                int bytesToRead = buf.Length;

                if (0 < m_bytesInRead)
                {
                    bytesRead = (m_bytesInRead >= bytesToRead) ? bytesToRead : m_bytesInRead;

                    Array.Copy(m_readBuffer, buf, bytesRead);

                    m_bytesInRead -= bytesRead;
                    bytesToRead -= bytesRead;

                    if (0 < m_bytesInRead) Array.Copy(m_readBuffer, bytesRead, m_readBuffer, 0, m_bytesInRead);
                }

                // Read from SerialStream if there is more to read
                if (0 < bytesToRead) bytesRead += m_stream.Read(buf, bytesRead, bytesToRead, to);

                // Check for "\r\nDISCONNECT\r\n" in newly read bytes
                byte[] bytes_CR = new byte[] { c_CR };
                byte[] disconnect = ((byte[])s_Cmds[c_ATR_DISCONNECT]);

                if (disconnect.Length > m_bytesInRead)
                {
                    // Search for the beginning of "\r\nDISCONNECT\r\n" in buf
                    int prefixIndex = (0 < bytesRead - disconnect.Length) ? bytesRead - disconnect.Length : 0;
                    prefixIndex = BytesIndexOf(buf, prefixIndex, bytesRead, bytes_CR);

                    if (0 <= prefixIndex)
                    {
                        // Read more if not enough bytes to check for "\r\nDISCONNECT\r\n"
                        int endLen = bytesRead - prefixIndex + m_bytesInRead;

                        if (endLen < disconnect.Length)
                        {
                            int num = m_stream.Read(m_readBuffer, m_bytesInRead, disconnect.Length - endLen, c_DefaultReadTimeout);

                            m_bytesInRead += num;

                            if (disconnect.Length - endLen != num) return bytesRead;
                        }

                        // Check if last read bytes are equal to "\r\nDISCONNECT\r\n"
                        if (BytesEqual(buf, prefixIndex, disconnect, 0, bytesRead - prefixIndex)
                            && BytesEqual(m_readBuffer, 0, disconnect, bytesRead - prefixIndex, disconnect.Length - (bytesRead - prefixIndex))
                           )
                        {
                            // Then remove the "\r\nDISCONNECT\r\n" prefix from buf
                            bytesRead -= bytesRead - prefixIndex;
                            bytesRead = (bytesRead == 0) ? -1 : bytesRead;

                            SDResponse dResponse = new SDResponse();
                            dResponse.Type = c_ATR_DISCONNECT;
                            dResponse.Data = null;

                            m_responseQueue.Add(dResponse);
                        }
                    }
                }

                return bytesRead;
            }

            /// <summary>
            /// Writes the given data to the SD.
            /// </summary>
            /// <param name="buffer">
            /// The data to write
            /// </param>
            /// <returns>
            /// The number of bytes written, or -1 on disconnect
            /// </returns>
            internal int Write(byte[] buf)
            {
                // Check for disconnect
                if (0 < m_responseQueue.Count) return -1;

                byte[] disconnect = (byte[])s_Cmds[c_ATR_DISCONNECT];

                if (0 <= BytesIndexOf(m_readBuffer, 0, m_bytesInRead, disconnect)) return -1;

                // Try reading in a "\r\nDISCONNECT\r\n" (so we can return -1 if found)
                byte[] tempBuf = new byte[disconnect.Length];
                int bytesRead = m_stream.Read(tempBuf, 0, tempBuf.Length, c_DefaultReadTimeout);

                if (0 < bytesRead)
                {
                    if (m_readBuffer.Length < m_bytesInRead + bytesRead)
                    {   // Shift buffer left bytesRead bytes (we lose data here but assume it's a rare case)
                        int shftAmt = m_bytesInRead + bytesRead - m_readBuffer.Length;
                        Array.Copy(m_readBuffer, shftAmt, m_readBuffer, 0, m_readBuffer.Length - shftAmt);
                        m_bytesInRead -= shftAmt;
                    }

                    Array.Copy(tempBuf, 0, m_readBuffer, m_bytesInRead, bytesRead);
                    m_bytesInRead += bytesRead;

                    if (0 <= BytesIndexOf(m_readBuffer, 0, m_bytesInRead, disconnect)) return -1;
                }

                // Now do the write
                m_stream.Write(buf);

                return buf.Length;
            }

            /// <summary>
            /// Clears the read buffer and its associated state.
            /// </summary>
            internal void Clear()
            {
                m_responseQueue.Clear();

                m_bytesInRead = 0;
            }

            /// <summary>
            /// Clears the read buffer, its associated state, and the serial port read buffer.
            /// </summary>
            internal void ClearAll()
            {
                Clear();
                m_stream.Clear(m_readBuffer);
            }

            //--//
            /////////////////////////////////////////////////////////////////////////////////
            /// Private Helper functions

            /// <summary>
            /// A recursive helper function that does the work for WaitForResultResponse.
            /// </summary>
            /// <param name="timeout">
            /// The timeout in ms
            /// </param>
            /// <param name="nextResponse">
            /// The start index of the last checked response in the read buffer
            /// </param>
            /// <returns>
            /// The response code for the result or -1 on timeout
            /// </returns>
            private int WaitForResultResponseHelper(int timeout, int nextResponse)
            {
                int resultResponse = -1;

                // If 1 or more responses have been found
                for (; nextResponse < m_responseQueue.Count; nextResponse++)
                {
                    if (0 < ((SDResponse)m_responseQueue[nextResponse]).Type)
                    {
                        resultResponse = ((SDResponse)m_responseQueue[nextResponse]).Type;

                        m_responseQueue.RemoveAt(nextResponse);

                        return resultResponse;
                    }
                }

                // If timeout expired
                if (0 > timeout)
                {
                    OpTimedOut = true;
                    return resultResponse;
                }

                // If there is still time left
                DateTime start = DateTime.Now;

                ReadAndParse();

                return WaitForResultResponseHelper(timeout - TotalMilliseconds(DateTime.Now - start), nextResponse);
            }

            /// <summary>
            /// A read in STANDBY mode.  Reads data from the SD via SerialStream into the read
            /// buffer and parses it as one or more SD responses.
            /// </summary>
            private void ReadAndParse()
            {
                int bytes = m_stream.Read(m_readBuffer, m_bytesInRead, m_readBuffer.Length - m_bytesInRead, c_DefaultReadTimeout);

                m_bytesInRead += bytes;

                if (0 < bytes) Parse();
            }

            /// <summary>
            /// Parses data read from the SD in STANDBY mode and inserts it into the queue
            /// of responses.
            /// </summary>
            /// <remarks>
            /// This function takes advantage of the fact that a result response
            /// from the SD for an AT command has a unique exact length among all other
            /// possible AT command responses.
            /// </remarks>
            private void Parse()
            {
                byte[] delim = (byte[])s_Cmds[c_ATR_DELIMETER];

                // Extract responses and put them in the response queue
                bool foundResponse = false;
                int startIndex = 0;
                int closeIndex = 0;

                while (0 <= (startIndex = BytesIndexOf(m_readBuffer, startIndex, m_bytesInRead, delim)))
                {
                    // Find closing delimeter
                    closeIndex = BytesIndexOf(m_readBuffer, startIndex + 1, m_bytesInRead, delim);

                    if (-1 == closeIndex) break;

                    foundResponse = true;
                    SDResponse response = new SDResponse();

                    // Check for result response
                    int len = closeIndex + delim.Length - startIndex;
                    response.Data = null;

                    if (((byte[])s_Cmds[c_ATR_OK]).Length == len)
                    {
                        response.Type = c_ATR_OK;
                    }
                    else if (((byte[])s_Cmds[c_ATR_ERROR]).Length == len)
                    {
                        response.Type = c_ATR_ERROR;
                    }
                    else if (((byte[])s_Cmds[c_ATR_DISCONNECT]).Length == len)
                    {
                        response.Type = c_ATR_DISCONNECT;
                    }
                    else if (BytesEqual(m_readBuffer, startIndex, (byte[])s_Cmds[c_ATR_CONNECT], 0, ((byte[])s_Cmds[c_ATR_CONNECT]).Length))
                    {
                        response.Type = c_ATR_CONNECT;

                        SDResponse addrResponse = new SDResponse();

                        addrResponse.Type = -1;
                        addrResponse.Data = new byte[len - ((byte[])s_Cmds[c_ATR_CONNECT]).Length];

                        Array.Copy(m_readBuffer, ((byte[])s_Cmds[c_ATR_CONNECT]).Length, addrResponse.Data, 0, addrResponse.Data.Length);

                        m_responseQueue.Add(addrResponse);
                    }
                    else
                    {
                        response.Type = -1;
                        response.Data = new byte[len];

                        Array.Copy(m_readBuffer, startIndex, response.Data, 0, len);
                    }

                    m_responseQueue.Add(response);

                    startIndex = closeIndex + 1;
                }

                if (foundResponse)
                {   // Move all remaining bytes to the front of the read buffer
                    int lastIndex = (0 > startIndex) ? closeIndex + delim.Length : startIndex;

                    m_bytesInRead = m_bytesInRead - lastIndex;

                    if (0 < m_bytesInRead)
                    {
                        Array.Copy(m_readBuffer, lastIndex, m_readBuffer, 0, m_bytesInRead);
                    }
                }
            }

            /// <summary>
            /// Returns a timespan's value in milliseconds.
            /// </summary>
            /// <param name="ts">
            /// The given timespan
            /// </param>
            /// <returns>
            /// The timepsan's value in milliseconds
            /// </returns>
            private int TotalMilliseconds(TimeSpan ts)
            {
                return (int)((double)ts.Ticks / (double)System.TimeSpan.TicksPerMillisecond);
            }

        } // class SDIOController

        //--//
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Codes abbreviating AT commands for Promi-ESD-02 Bluetooth chip.  See Appendix A of User's Manual

        private const int c_ATC_AT = 0;        // AT\r
        private const int c_ATC_ATZ = 1;        // ATZ\r
        private const int c_ATC_BTINQ = 2;        // AT+BTINQ?\r
        private const int c_ATC_ATD_ADDR = 3;        // ATD <BD_ADDR>\r
        private const int c_ATC_ATD = 4;        // ATD\r
        private const int c_ATC_BTSCAN = 5;        // AT+BTSCAN,
        private const int c_ATC_BTCANCEL = 6;        // AT+BTCANCEL\r
        private const int c_ATC_GOTO_STANDBY = 7;        // +++\r
        private const int c_ATC_GOTO_ONLINE = 8;        // ATO\r
        private const int c_ATC_ATH = 9;        // ATH\r
        private const int c_ATC_BTLAST = 10;        // AT+BTLAST?\r
        private const int c_ATC_BTNAME = 11;        // AT+BTNAME="
        private const int c_ATC_BTKEY = 12;        // AT+BTKEY="
        private const int c_ATC_BTINFO = 13;        // AT+BTINFO?\r
        private const int c_ATC_BTLPM = 14;        // AT+BTLPM,
        private const int c_ATC_UARTCONFIG = 15;        // AT+UARTCONFIG,
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // Codes abbreviating Promi-ESD-02 chip responses to AT commands.  See Appendix A of User's Manual

        private const int c_ATR_DELIMETER = 16;        // \r\n
        private const int c_ATR_OK = 17;        // \r\nOK\r\n
        private const int c_ATR_ERROR = 18;        // \r\nERROR\r\n
        private const int c_ATR_CONNECT = 19;        // \r\nCONNECT\r\n
        private const int c_ATR_DISCONNECT = 20;        // \r\nDISCONNECT\r\n

        //--//

        private const byte c_CR = (byte)'\r';
        private const byte c_COMMA = (byte)',';
        private const byte c_QUOTE = (byte)'\"';

        private const byte c_Scan_INQUIRY = (byte)'1';
        private const byte c_Scan_PAGE = (byte)'2';
        private const byte c_Scan_INQUIRY_AND_PAGE = (byte)'3';
        private const byte c_Scan_PAGE_ADDRESS = (byte)'4';

        //--//

        private const int c_OneSecondTimeout = 1000;

        //--//

        private string m_com;                     // The COM port number
        private BaudRate m_baud;                    // The baud rate for the COM port
        private SerialPort m_port;                    // SerialPort connected to the SD
        private SDIOController m_ioCtrl;                  // IO buffer for the SD
        private OutputPort m_power;                   // Voltage regulator: False -> off, True -> on

        private string m_lastConnectedAddr;       // Address of device last connected to

        private PowerState m_powerState;              // The current power state of the SD
        private bool m_disposed;                // Whether or not the driver is disposed

        //--//

        /// <summary>
        /// Creates a new instance of PromiSDBTDriver by initializing
        /// the Serial connection and checking if the SD is present.
        /// </summary>
        /// <param name="comNum">
        /// Number of the COM port to use
        /// </param>
        /// <param name="baudRate">
        /// Baud rate of the COM port to use
        /// </param>
        /// <param name="powerPin">
        /// GPIO to turn the bluetooth power off
        /// </param>
        /// <remarks>
        /// GPIO pin for the N6D should be GPIO22_VTU_TIO3A, if there is no GPIO for power control
        /// then use GPIO_NONE.
        /// </remarks>
        public PromiSDBTDriver(string com, BaudRate baud, Cpu.Pin powerPin)
        {
            bool success = false;

            m_com = com;
            m_baud = baud;

            try
            {
                if (!CreateSerialConnection(m_com, m_baud)) throw new SystemException("Error booting SD");

                m_power = (powerPin == Cpu.Pin.GPIO_NONE) ? null : new OutputPort(powerPin, false);
                m_powerState = PowerState.Off;
                m_state = BTDeviceState.OFF;

                PowerState state = PowerState.On;
                if (!SetPowerState(ref state) || !CheckDevicePresent()) throw new SystemException("Error booting SD");

                success = true;

                UpdateDeviceStatus();

                m_disposed = false;
            }
            finally
            {
                if (!success)
                {
                    if (null != m_power)
                    {
                        m_power.Dispose();
                        m_power = null;
                    }

                    m_disposed = true;
                }
            }
        }

        /// <summary>
        /// Disposes the SD by tearing down any current connection and disposing the SerialPort.
        /// </summary>
        public override void Dispose()
        {
            if (!m_disposed)
            {
                Disconnect();
                SwitchPower(false);

                m_port.Dispose();

                if (null != m_power)
                {
                    m_power.Dispose();
                    m_power = null;
                }

                m_ioCtrl = null;

                ForceState(BTDeviceState.UNINITIALIZED);

                m_disposed = true;
            }
        }

        /// <summary>
        /// Turns power to chip on or off.
        /// </summary>
        /// <param name="on">
        /// Turns chip on if true, off if false
        /// </param>
        /// <returns>
        /// True if the power switch succeeded, false o/w
        /// </returns>
        public override bool SwitchPower(bool on)
        {
            // Handle the Stamp case (chip is always on)
            if (null == m_power)
            {
                if (on)
                {   // Un-Park the chip
                    m_ioCtrl.WriteCmd(true,
                                       c_OneSecondTimeout,
                                       c_ATC_BTLPM,
                                       new object[] { (byte)'0', c_CR });
                }
                else
                {   // Park the chip
                    m_ioCtrl.WriteCmd(true,
                                       c_OneSecondTimeout,
                                       c_ATC_BTLPM,
                                       new object[] { (byte)'1', c_CR });
                }

                return true;
            }

            if (on)
            {
                // Turn power off if it's already on
                if (m_power.Read()) SwitchPower(false);

                m_power.Write(true);
                m_ioCtrl.WaitForResultResponse(c_OneSecondTimeout);

                // If we can't read the "\r\nOK\r\n" at the standard baudrate, then try
                // reading at baud rates from 4800 up to 230400; once the right baudrate is
                // found, reconfigure the baudrate to the power policy's requested rate
                if (m_ioCtrl.OpTimedOut)
                {
                    BaudRate[] baud = new BaudRate[]
                                                 {
                                                    BaudRate.Baudrate4800,
                                                    BaudRate.Baudrate9600,
                                                    BaudRate.Baudrate19200,
                                                    BaudRate.Baudrate38400,
                                                    BaudRate.Baudrate57600,
                                                    BaudRate.Baudrate115200,
                                                    BaudRate.Baudrate230400
                                                 };

                    bool success = false;
                    for (int i = 0; i < baud.Length; i++)
                    {
                        SwitchPower(false);

                        bool serialCreated = false;
                        try
                        {
                            serialCreated = CreateSerialConnection(m_com, baud[i]);
                        }
                        catch
                        {
                        }

                        if (!serialCreated) return false;

                        m_power.Write(true);
                        m_ioCtrl.WaitForResultResponse(5 * c_OneSecondTimeout);

                        if (!m_ioCtrl.OpTimedOut)
                        {
                            // Now reconfigure baud rate to the user-requested rate
                            if (!m_ioCtrl.WriteCmd(true,
                                                   c_OneSecondTimeout,
                                                   c_ATC_UARTCONFIG,
                                                   new object[] { ConvertToBytes(m_baud + ""), c_COMMA, (byte)'N', c_COMMA, (byte)'1', c_CR }))
                            {
                                return false;
                            }

                            serialCreated = false;
                            try
                            {
                                serialCreated = CreateSerialConnection(m_com, m_baud);
                            }
                            catch
                            {
                            }

                            if (!serialCreated) return false;

                            // Now reset chip
                            SwitchPower(false);
                            SwitchPower(true);

                            success = true;
                            break;
                        }
                    }

                    if (!success) return false;
                }

                m_devicePresent = true;

                ForceState(BTDeviceState.STANDBY);
            }
            else
            {
                if (m_power.Read())
                {
                    m_power.Write(false);
                    Thread.Sleep(80);         // What is the proper timeout here?
                }

                ForceState(BTDeviceState.OFF);
            }

            return true;
        }

        /// <summary>
        /// Takes a PowerState as a suggestion and tries to put the Promi chip
        /// in that state (or in as close a state as possible to that state).
        /// </summary>
        /// <param name="ps">
        /// The suggested PowerState.  Before returning, this variable is assigned
        /// the PowerState that the Promi chip was actually set to.
        /// </param>
        /// <returns>
        /// True if the Promi chip was put in the requested PowerState or the closest
        /// state possible, false if changing the PowerState of the chip was not possible.
        /// </returns>
        /// <remarks>
        /// The Promi SD only supports the Park low-power mode, so Hold and Sniff
        /// are most closely matched by Park.
        /// </remarks>
        public override bool SetPowerState(ref PowerState ps)
        {
            if (m_powerState == ps) return true;

            if (PowerState.Park == m_powerState && (PowerState.Hold == ps || PowerState.Sniff == ps))
            {
                ps = PowerState.Park;
                return true;
            }

            if (CheckState(BTDeviceState.PENDING))
            {
                return false;
            }

            if (ChangeState(BTDeviceState.ONLINE, BTDeviceState.PENDING))
            {   // Then enter or leave Park mode
                try
                {
                    // Make application responsible for explicitly disconnecting before switching power off
                    if (PowerState.Off == ps) throw new InvalidOperationException();

                    // Otherwise temporarily goto standby (try 3 times to go to standby because SD is unreliable)
                    bool inStandby = false;
                    for (int i = 0; i < 3; i++)
                    {
                        Thread.Sleep(50);
                        if (m_ioCtrl.WriteCmd(true, c_OneSecondTimeout, c_ATC_GOTO_STANDBY, null))
                        {
                            inStandby = true;
                            break;
                        }
                    }

                    if (inStandby)
                    {
                        if (PowerState.On == ps)
                        {   // Then un-Park the chip
                            if (m_ioCtrl.WriteCmd(true,
                                                  c_OneSecondTimeout,
                                                  c_ATC_BTLPM,
                                                  new object[] { (byte)'0', c_CR }))
                            {
                                m_powerState = PowerState.On;
                            }
                        }
                        else
                        {   // Then Park the chip
                            if (m_ioCtrl.WriteCmd(true,
                                                  c_OneSecondTimeout,
                                                  c_ATC_BTLPM,
                                                  new object[] { (byte)'1', c_CR }))
                            {
                                m_powerState = PowerState.Park;
                            }
                        }

                        // Now go back online
                        m_ioCtrl.WriteCmd(false, c_OneSecondTimeout, c_ATC_GOTO_ONLINE, null);
                    }
                }
                finally
                {
                    ForceState(BTDeviceState.ONLINE);
                }
            }
            else if (ChangeState(BTDeviceState.OFF, BTDeviceState.PENDING))
            {   // Then switch chip On and possibly Park it
                BTDeviceState finalState = BTDeviceState.OFF;
                try
                {
                    if (SwitchPower(true))
                    {
                        finalState = BTDeviceState.STANDBY;
                        m_powerState = PowerState.On;

                        if (PowerState.On != ps)
                        {
                            if (m_ioCtrl.WriteCmd(true,
                                                  c_OneSecondTimeout,
                                                  c_ATC_BTLPM,
                                                  new object[] { (byte)'1', c_CR }))
                            {
                                m_powerState = PowerState.Park;
                            }
                        }
                    }
                }
                finally
                {
                    ForceState(finalState);
                }
            }
            else if (ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
            {   // Then switch chip Off, or enter/leave Park mode
                BTDeviceState finalState = BTDeviceState.STANDBY;
                try
                {
                    if (PowerState.Off == ps)
                    {
                        if (SwitchPower(false))
                        {
                            finalState = BTDeviceState.OFF;
                            m_powerState = PowerState.Off;
                        }
                    }
                    else if (PowerState.On == ps)
                    {
                        if (m_ioCtrl.WriteCmd(true,
                                              c_OneSecondTimeout,
                                              c_ATC_BTLPM,
                                              new object[] { (byte)'0', c_CR }))
                        {
                            m_powerState = PowerState.On;
                        }
                    }
                    else
                    {
                        if (m_ioCtrl.WriteCmd(true,
                                              c_OneSecondTimeout,
                                              c_ATC_BTLPM,
                                              new object[] { (byte)'1', c_CR }))
                        {
                            m_powerState = PowerState.Park;
                        }
                    }
                }
                finally
                {
                    ForceState(finalState);
                }
            }
            else
            {   // Throw an exception if the driver has been disposed
                throw new InvalidOperationException();
            }

            ps = m_powerState;

            return true;
        }

        /// <summary>
        /// Checks that the SD is present.
        /// </summary>
        /// <returns>
        /// True if the device is present, false o/w
        /// </returns>
        public override bool CheckDevicePresent()
        {
            if (!ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING)
                && !ChangeState(BTDeviceState.ONLINE, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            BTDeviceState finalState = BTDeviceState.STANDBY;

            try
            {
                // Try to ping the SD chip 3 times and on the third failure declare it disconnected
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(50);
                    m_ioCtrl.WriteCmd(false, c_OneSecondTimeout, c_ATC_GOTO_STANDBY, null);
                    int result = m_ioCtrl.WaitForResultResponse(c_OneSecondTimeout);
                    if (!m_ioCtrl.OpTimedOut)
                    {
                        if (c_ATR_OK == result)
                        {   // Then we were online, so go back online
                            m_ioCtrl.WriteCmd(false, c_OneSecondTimeout, c_ATC_GOTO_ONLINE, null);
                            finalState = BTDeviceState.ONLINE;
                        }
                        else
                        {
                            finalState = BTDeviceState.STANDBY;
                        }
                        break;
                    }
                }
            }
            finally
            {
                m_devicePresent = !m_ioCtrl.OpTimedOut;

                ForceState(finalState);
            }

            return m_devicePresent;
        }

        /// <summary>
        /// Sets a friendly device name for the SD.
        /// </summary>
        /// <param name="name">
        /// The friendly name
        /// </param>
        /// <returns>
        /// True if the operations succeed, false o/w
        /// </returns>
        public override bool SetFriendlyDeviceName(string name)
        {
            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            bool bRet = false;

            try
            {
                bRet = m_ioCtrl.WriteCmd(true,
                                          c_OneSecondTimeout,
                                          c_ATC_BTNAME,
                                          new object[] { c_QUOTE, ConvertToBytes(name), c_QUOTE, c_CR });
            }
            finally
            {
                ForceState(BTDeviceState.STANDBY);
            }

            return bRet;
        }

        /// <summary>
        /// Sets the pass key for the SD.
        /// </summary>
        /// <param name="key">
        /// The key
        /// </param>
        /// <returns>
        /// True if the operations succeed, false o/w
        /// </returns>
        /// <remarks>
        /// The pass key can be up to 16 alphanumeric characters in length
        /// </remarks>
        public override bool SetPassKey(string key)
        {
            if (null == key || 16 < key.Length || 0 == key.Length) return false;

            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            bool bRet = false;

            try
            {
                bRet = m_ioCtrl.WriteCmd(true,
                                          c_OneSecondTimeout,
                                          c_ATC_BTKEY,
                                          new object[] { ConvertToBytes(key), c_CR });
            }
            finally
            {
                ForceState(BTDeviceState.STANDBY);
            }

            return bRet;
        }

        /// <summary>
        /// Retrieves the device configuration information including address, name,
        /// authentication on/off, and encryption on/off.  Results are stored in
        /// public member variables.
        /// </summary>
        public override void UpdateDeviceStatus()
        {
            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            try
            {
                DeviceInfo = new BTDeviceInfo();

                m_ioCtrl.WriteCmd(false, c_OneSecondTimeout, c_ATC_BTINFO, null);

                if (c_ATR_OK == m_ioCtrl.WaitForResultResponse(c_OneSecondTimeout))
                {
                    byte[] cfg = m_ioCtrl.ReadResponse();
                    string configStr = ConvertToString(cfg, cfg.Length);

                    string[] configStrs = configStr.Trim().Split(new char[] { ',' });

                    DeviceInfo.Address = configStrs[0];
                    DeviceInfo.Name = configStrs[1];

                    Authentication = ('1' == configStrs[4][0]) ? true : false;
                    Encryption = ('1' == configStrs[5][0]) ? true : false;
                }
            }
            finally
            {
                ForceState(BTDeviceState.STANDBY);
            }
        }

        /// <summary>
        /// Sets park (low power) mode on/off.
        /// </summary>
        /// <param name="park">
        /// The value to set park mode to (true = on)
        /// </param>
        /// <returns>
        /// True if the operation(s) succeed(s), false o/w
        /// </returns>
        public override bool SetParkMode(bool park)
        {
            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            bool bRet = false;

            try
            {
                byte[] parkBytes = new byte[2];

                parkBytes[0] = (park) ? (byte)'1' : (byte)'0';
                parkBytes[1] = c_CR;

                bRet = m_ioCtrl.WriteCmd(true, c_OneSecondTimeout, c_ATC_BTLPM, new object[] { parkBytes });
            }
            finally
            {
                ForceState(BTDeviceState.STANDBY);
            }

            return bRet;
        }

        /// <summary>
        /// Does a soft-reset of the SD.
        /// </summary>
        /// <returns>
        /// True if the operation(s) succeed(s), false o/w
        /// </returns>
        public override bool ResetDevice()
        {
            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            bool bRet = false;

            try
            {
                bRet = m_ioCtrl.WriteCmd(true, c_OneSecondTimeout, c_ATC_ATZ, null);
            }
            finally
            {
                ForceState(BTDeviceState.STANDBY);
            }

            return bRet;
        }

        /// <summary>
        /// Puts the SD in INQUIRY (discovery) mode for 30 seconds and returns
        /// up to 10 devices seen during that time.
        /// </summary>
        /// <returns>
        /// A list of up to 10 seen devices, or null if error or no devices seen
        /// </returns>
        /// <remarks>
        /// Power consumption during INQUIRY mode is 48 mA
        /// </remarks>
        public override BTDeviceInfo[] Inquiry()
        {
            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            try
            {
                m_ioCtrl.WriteCmd(false, c_OneSecondTimeout, c_ATC_BTINQ, null);

                if (c_ATR_OK == m_ioCtrl.WaitForResultResponse(32 * 1000))
                {
                    ArrayList devList = new ArrayList();
                    byte[] info = null;

                    while (null != (info = m_ioCtrl.ReadResponse()))
                    {
                        string[] devInfo = ConvertToString(info, info.Length).Trim().Split(new char[] { ',' });

                        BTDeviceInfo dev = new BTDeviceInfo();
                        dev.Address = devInfo[0];
                        dev.Name = devInfo[1];

                        devList.Add(dev);
                    }

                    ForceState(BTDeviceState.STANDBY);

                    if (0 == devList.Count) return null;

                    BTDeviceInfo[] devArray = new BTDeviceInfo[devList.Count];

                    for (int i = 0; i < devList.Count; i++) devArray[i] = (BTDeviceInfo)devList[i];

                    return devArray;
                }
            }
            finally
            {
                ForceState(BTDeviceState.STANDBY);
            }

            return null;
        }

        /// <summary>
        /// Puts the SD in Page (connect) mode, trying to connect to the device with the
        /// given address for up to the specified timeout.
        /// </summary>
        /// <param name="addr">
        /// The address of the device to connect to
        /// </param>
        /// <param name="timeout">
        /// The timeout, or lte for no timeout
        /// </param>
        /// <returns>
        /// True if a connection is established, false o/w
        /// </returns>
        /// <remarks>
        /// Power consumption during PAGE mode is 48 mA.
        /// </remarks>
        public override bool PageAddress(string addr, int timeout)
        {
            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            bool goStandbyOnExit = true;
            try
            {
                if (!m_ioCtrl.WriteCmd(true,
                                       c_OneSecondTimeout,
                                       c_ATC_ATD_ADDR,
                                       new object[] { ConvertToBytes(addr), c_CR }))
                {
                    return false;
                }

                if (c_ATR_CONNECT != m_ioCtrl.WaitForResultResponse(timeout))
                {
                    if (m_ioCtrl.OpTimedOut && !m_ioCtrl.WriteCmd(true, c_OneSecondTimeout, c_ATC_BTCANCEL, null))
                    {
                        if (m_ioCtrl.OpTimedOut)
                        {
                            m_lastConnectedAddr = addr;

                            ForceState(BTDeviceState.ONLINE);
                            Disconnect();
                        }
                    }

                    return false;
                }

                goStandbyOnExit = false;
            }
            finally
            {
                if (!goStandbyOnExit)
                {
                    byte[] response = m_ioCtrl.ReadResponse();
                    m_lastConnectedAddr = ConvertToString(response, response.Length);

                    ForceState(BTDeviceState.ONLINE);
                }
                else
                {
                    ForceState(BTDeviceState.STANDBY);
                }
            }

            return true;
        }

        /// <summary>
        /// Put the SD in INQUIRY SCAN (respond to inquiries) mode for
        /// up to the specified timeout.
        /// </summary>
        /// <param name="timeout">
        /// The specified timeout in milliseconds. A timeout value lte 0 specifies
        /// that the SD should continue to scan until a connection is made.
        /// Promi-ESD default maximum is 5 min.
        /// </param>
        /// <returns>
        /// True if the operation(s) succeed(s), false o/w
        /// </returns>
        /// <remarks>
        /// Power consumption for INQUIRY SCAN mode is 19 mA.
        /// </remarks>
        public override bool InquiryScan(int timeout)
        {
            return DoScan(c_Scan_INQUIRY, null, timeout);
        }

        /// <summary>
        /// Put the SD in PAGE SCAN (accept connections) mode for up to the
        /// specified timeout.
        /// </summary>
        /// <param name="timeout">
        /// The specified timeout in milliseconds. A timeout value lte 0 specifies
        /// that the SD should continue to scan until a connection is made.
        /// Promi-ESD default maximum is 5 min.
        /// </param>
        /// <returns>
        /// True if the scan results in a connection, false o/w
        /// </returns>
        /// <remarks>
        /// Power consumption for PAGE SCAN mode is 19 mA.
        /// </remarks>
        public override bool PageScan(int timeout)
        {
            return DoScan(c_Scan_PAGE, null, timeout);
        }

        /// <summary>
        /// Put the SD in PAGE SCAN (accept connections) mode, scanning for the specified
        /// address, and for specified timeout.
        /// </summary>
        /// <param name="addr">
        /// The address
        /// </param>
        /// <param name="timeout">
        /// The timeout
        /// </param>
        /// <returns>
        /// True if the scan results in a connection, false o/w
        /// </returns>
        public override bool PageScanForAddress(string addr, int timeout)
        {
            return DoScan(c_Scan_PAGE_ADDRESS, addr, timeout);
        }

        /// <summary>
        /// Put the SD in alternating INQUIRY SCAN (respond to inquiries) and
        /// PAGE SCAN (accept connections) mode for up to the specified timeout.
        /// </summary>
        /// <param name="timeout">
        /// The specified timeout in milliseconds. A timeout value lte 0 specifies
        /// that the SD should continue to scan until a connection is made.
        /// Promi-ESD default maximum is 5 min.
        /// </param>
        /// <returns>
        /// True if the scan results in a connection, false o/w
        /// </returns>
        /// <remarks>
        /// Power consumption for alternating INQUIRY SCAN and PAGE SCAN mode is 19 mA.
        /// </remarks>
        public override bool InquiryAndPageScans(int timeout)
        {
            return DoScan(c_Scan_INQUIRY_AND_PAGE, null, timeout);
        }

        /// <summary>
        /// Returns the address of the last Bluetooth device to which the SD was connected.
        /// </summary>
        /// <returns>
        /// The address, or null if no such device.
        /// </returns>
        public override string GetLastConnectedAddress()
        {
            if (null != m_lastConnectedAddr) return m_lastConnectedAddr;

            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            try
            {
                m_ioCtrl.WriteCmd(false, c_OneSecondTimeout, c_ATC_BTLAST, null);

                byte[] addr = null;

                if (c_ATR_OK == m_ioCtrl.WaitForResultResponse(c_OneSecondTimeout)
                    && (null != (addr = m_ioCtrl.ReadResponse()))
                   )
                {
                    m_lastConnectedAddr = ConvertToString(addr, addr.Length).Trim();
                }
            }
            finally
            {
                ForceState(BTDeviceState.STANDBY);
            }

            return m_lastConnectedAddr;
        }

        /// <summary>
        /// Releases the SD from it's current connection if there is one.
        /// </summary>
        /// <returns>
        /// True if the function exits in disconnected state, false o/w
        /// </returns>
        public override bool Disconnect()
        {
            if (!m_devicePresent) return true;

            bool gotoStandbyOnExit = false;

            try
            {
                // Try to go to STANDBY mode up to 3 times
                bool inStandby = false;
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(50);
                    if (m_ioCtrl.WriteCmd(true, c_OneSecondTimeout, c_ATC_GOTO_STANDBY, null) || !m_ioCtrl.OpTimedOut)
                    {
                        inStandby = true;
                        break;
                    }
                }

                gotoStandbyOnExit = true;

                Thread.Sleep(50);
                if (!inStandby
                   || (m_ioCtrl.WriteCmd(true, c_OneSecondTimeout, c_ATC_ATH, null)
                       && c_ATR_DISCONNECT != m_ioCtrl.WaitForResultResponse(c_OneSecondTimeout)))
                {
                    // SD has been switched off or physically removed, so assume disconnect
                    // Note that if the device comes back online later we must check it's state
                    // and disconnect (i.e. +++, ATH) in case it is actually still connected
                    m_devicePresent = false;
                }
            }
            finally
            {
                if (gotoStandbyOnExit) ForceState(BTDeviceState.STANDBY);
            }

            return true;
        }

        /// <summary>
        /// Receives up to the specified number of bytes from the remotely connected
        /// device until the timeout expires or until no more bytes can be read.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to put received bytes into
        /// </param>
        /// <param name="timeout">
        /// The timeout in milliseconds
        /// </param>
        /// <returns>
        /// The number of bytes received, or -1 on disconnect
        /// </returns>
        public override int Receive(ref byte[] buffer, int timeout)
        {
            if (!m_devicePresent || !CheckState(BTDeviceState.ONLINE)) return -1;

            int bytesRead = m_ioCtrl.Read(ref buffer, timeout);

            if (0 > bytesRead) ForceState(BTDeviceState.STANDBY); // There was a disconnect

            return bytesRead;
        }

        /// <summary>
        /// Sends data to the remotely connected device.
        /// </summary>
        /// <param name="text">
        /// The data to send
        /// </param>
        /// <returns>
        /// The number of bytes written, or -1 on disconnect
        /// </returns>
        public override int Send(byte[] data)
        {
            if (!m_devicePresent || !CheckState(BTDeviceState.ONLINE))
                return -1;

            int bytesWritten = m_ioCtrl.Write(data);

            if (0 > bytesWritten) ForceState(BTDeviceState.STANDBY);  // There was a disconnect

            return bytesWritten;
        }

        ///////////////////////////////////////////////////////////////////////////
        // Private helper methods

        /// <summary>
        /// A helper method to create the serial connection objects (SerialPort) and
        /// SDIOController.
        /// </summary>
        /// <param name="com">
        /// The COM port number
        /// </param>
        /// <param name="baud">
        /// The baudrate for the specified COM port
        /// </param>
        /// <returns>
        /// True if the objects were successfully created, false o/w
        /// </returns>
        private bool CreateSerialConnection(string com, BaudRate baud)
        {
            bool created = false;
            try
            {
                if (null != m_port) m_port.Dispose();
                m_port = null;
                m_port = new SerialPort(com, (int)baud);
                m_port.Open();
                m_ioCtrl = new SDIOController(m_port);
                created = true;
            }
            finally
            {
                if (!created)
                {
                    if (null != m_port)
                    {
                        m_port.Dispose();
                        m_port = null;
                    }

                    if (null != m_ioCtrl) m_ioCtrl = null;
                }
            }

            return created;
        }

        /// <summary>
        /// A helper method to test and set the SD driver state.
        /// </summary>
        /// <param name="curState">
        /// State to test for
        /// </param>
        /// <param name="newState">
        /// State to set to if test succeeds
        /// </param>
        /// <returns>
        /// The result of the test
        /// </returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool ChangeState(BTDeviceState curState, BTDeviceState newState)
        {
            lock (this)
            {
                if (m_state == curState)
                {
                    m_state = newState;

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// A helper method to set the SD driver state in a synchronous way.
        /// </summary>
        /// <param name="state">
        /// The value to set the state to
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ForceState(BTDeviceState state)
        {
            m_state = state;
        }

        /// <summary>
        /// A helper method to check that the SD driver state has a particular value.
        /// </summary>
        /// <param name="state">
        /// The value to check for
        /// </param>
        /// <returns>
        /// True if the provided value matches the state, false o/w
        /// </returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool CheckState(BTDeviceState state)
        {
            return (m_state == state);
        }

        /// <summary>
        /// A helper method for INQUIRY and PAGE SCAN methods.  Takes the type of
        /// scan operation and does that operation with the given timeout.
        /// </summary>
        /// <param name="type">
        /// The type of scan operation
        /// </param>
        /// <param name="addr">
        /// The address to page in case of a PAGE SCAN address
        /// </param>
        /// <param name="timeout">
        /// The timeout in milliseconds, lte 0 for no timeout
        /// </param>
        /// <returns>
        /// True if the scan is an INQUIRY SCAN and the INQUIRY SCAN successfully
        /// initiates and completes, true if the scan includes a PAGE SCAN and
        /// results in a connection, false otherwise.
        /// </returns>
        private bool DoScan(byte type, string addr, int timeout)
        {
            if (!m_devicePresent || !ChangeState(BTDeviceState.STANDBY, BTDeviceState.PENDING))
                throw new InvalidOperationException();

            bool gotoStandbyOnExit = true;

            try
            {
                string timeoutSec = ((timeout <= 0) ? 0 : timeout / 1000).ToString();

                // Start the scan
                if (type != c_Scan_PAGE_ADDRESS)
                {
                    if (!m_ioCtrl.WriteCmd(true,
                                           c_OneSecondTimeout,
                                           c_ATC_BTSCAN,
                                           new object[] { type, c_COMMA, ConvertToBytes(timeoutSec), c_CR }))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!m_ioCtrl.WriteCmd(true,
                                           c_OneSecondTimeout,
                                           c_ATC_BTSCAN,
                                           new object[] { ConvertToBytes(addr), c_COMMA, ConvertToBytes(timeoutSec), c_CR }))
                    {
                        return false;
                    }
                }

                // Wait for scan result
                if (c_Scan_INQUIRY == type)
                {   // Wait for "\r\nERROR\r\n"
                    if ((c_ATR_ERROR != m_ioCtrl.WaitForResultResponse(timeout)) && m_ioCtrl.OpTimedOut)
                    {   // Cancel inquiry if it timed out
                        if (m_ioCtrl.OpTimedOut) m_ioCtrl.WriteCmd(true, c_OneSecondTimeout, c_ATC_BTCANCEL, null);
                    }

                    return true;
                }
                else
                {   // Then wait for "\r\nCONNECT\r\n"
                    if (c_ATR_CONNECT == m_ioCtrl.WaitForResultResponse(timeout + 1 * 1000)) // Wait longer than SD
                    {
                        byte[] response = m_ioCtrl.ReadResponse();
                        m_lastConnectedAddr = ConvertToString(response, response.Length);

                        gotoStandbyOnExit = false;
                        return true;
                    }
                    else
                    {   // If the wait for connect timed out (shouldn't happen) then cancel the scan
                        if (m_ioCtrl.OpTimedOut)
                        {
                            m_ioCtrl.WriteCmd(false, c_OneSecondTimeout, c_ATC_BTCANCEL, null);

                            if (c_ATR_OK == m_ioCtrl.WaitForResultResponse(c_OneSecondTimeout))
                            {
                                return false;
                            }
                            else
                            {
                                byte[] response = m_ioCtrl.ReadResponse();
                                m_lastConnectedAddr = ConvertToString(response, response.Length);

                                gotoStandbyOnExit = false;
                                return true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (gotoStandbyOnExit)
                {
                    ForceState(BTDeviceState.STANDBY);
                }
                else
                {
                    ForceState(BTDeviceState.ONLINE);
                }
            }

            return true;
        }

        /// <summary>
        /// Evaluates whether or not the indicated sections of two given byte arrays are equal in value.
        /// </summary>
        /// <param name="a">
        /// First byte array
        /// </param>
        /// <param name="aStart">
        /// Start index in the first byte array
        /// </param>
        /// <param name="b">
        /// Second byte array
        /// </param>
        /// <param name="bStart">
        /// Start index in the second byte array
        /// </param>
        /// <param name="len">
        /// The length of the array sections to compare
        /// </param>
        /// <returns>
        /// True if the arrays are equal, false o/w
        /// </returns>
        private static bool BytesEqual(byte[] a, int aStart, byte[] b, int bStart, int len)
        {
            if (aStart + len > a.Length || bStart + len > b.Length) return false;

            for (int i = 0, j = aStart, k = bStart; i < len; i++, j++, k++)
            {
                if (a[j] != b[k]) return false;
            }

            return true;
        }

        /// <summary>
        /// Finds the first index of the given sequence in the given array,
        /// beginning and ending at the specified start and end indices.
        /// </summary>
        /// <param name="array">
        /// The array
        /// </param>
        /// <param name="start">
        /// The start index
        /// </param>
        /// <param name="end">
        /// The index after the end
        /// </param>
        /// <param name="sequence">
        /// The sequence to search for
        /// </param>
        /// <returns>
        /// The index of the sequence if found and -1 o/w
        /// </returns>
        private static int BytesIndexOf(byte[] array, int start, int end, byte[] sequence)
        {
            if (start >= array.Length || end > array.Length || start < 0) return -1;

            for (int i = start; i < end; i++)
            {
                if (array[i] == sequence[0])
                {
                    for (int j = i, k = 0; j < end && k < sequence.Length; j++, k++)
                    {
                        if (array[j] != sequence[k])
                        {
                            break;
                        }
                        else
                        {
                            if (k == sequence.Length - 1) return i;
                        }
                    }
                }
            }

            return -1;
        }

        // The following two functions (ConvertToBytes and ConvertToString) should
        // be removed when the Text-to-bytes functions are implemented in the TinyCLR.

        /// <summary>
        /// Converts the given string to an equivalent byte array.
        /// </summary>
        /// <param name="str">
        /// The string to convert
        /// </param>
        /// <returns>
        /// The string converted to a byte array
        /// </returns>
        private byte[] ConvertToBytes(string str)
        {
            byte[] bytes = new byte[str.Length];

            for (int i = 0; i < str.Length; i++) { bytes[i] = (byte)str[i]; }

            return bytes;
        }

        /// <summary>
        /// Converts the specified bytes of the given byte array to an equivalent string.
        /// </summary>
        /// <param name="bytes">
        /// The byte array to convert
        /// </param>
        /// <param name="size">
        /// The number of bytes (starting from index 0) to convert
        /// </param>
        /// <returns>
        /// The byte array converted to a string.
        /// </returns>
        private string ConvertToString(byte[] bytes, int size)
        {
            if (size >= bytes.Length) size = bytes.Length;

            char[] chars = new char[size];

            for (int i = 0; i < size; i++) { chars[i] = (char)bytes[i]; }

            return new string(chars);
        }

    } // class PromiSDBTDriver
}


