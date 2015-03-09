using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;

namespace Microsoft.SPOT.SerialPortTestApp
{
    public class SerialPortTest
    {
        static int c_RecordSize = 50;
        static int c_RecordHeaderSize = 16;
        static int c_BufferSize = 2 * c_RecordSize + c_RecordHeaderSize;

        // The number of records to send/receive for each test
        static int c_NumRecords = 100;

        // Set the timeout in mS for send and receive
        static int c_ReceiveTimeout = 50;
        static int c_TransmitTimeout = 30;

        // Set this to perform the test...
        //static Hardware.SerialPort.Handshake c_Handshake = Hardware.Handshake.XOnXOff;
        //static Hardware.SerialPort.Handshake c_Handshake = Hardware.SerialPort.Handshake.NoFlowControl;
        //static Hardware.SerialPort.Handshake c_Handshake = Hardware.Handshake.RequestToSend;

        UInt32 dwPseudoRandom = 1;      // Should never be 0

        public static void Main()
        {
            Start(Handshake.XOnXOff);
            Start(Handshake.RequestToSend);
            Start(Handshake.None);

            Debug.Print("ALL TESTS COMPLETE!!!!!");
        }

        public static void Start(Handshake handshake)
        {
            int nWritten;
            // Note: zeros must be sent first to help the receiver sync after the noise generated
            // by the port initialization - don't send the important stuff right away...!
            byte[] start = { 0 };
            if (handshake == Handshake.None)
            {
                Debug.Print("***** Testing No Flow Control *****");
                start = new byte[] { 0, 0, 0, 0, 0, (byte)'X', (byte)'2', (byte)'3', (byte)'\r', (byte)'\n', 0 };
            }
            else if (handshake == Handshake.XOnXOff)
            {
                Debug.Print("***** Testing SW Flow Control *****");
                start = new byte[] { 0, 0, 0, 0, 0, (byte)'X', (byte)'2', (byte)'4', (byte)'\r', (byte)'\n', 0 };
            }
            else if (handshake == Handshake.RequestToSend)
            {
                Debug.Print("***** Testing HW Flow Control *****");
                start = new byte[] { 0, 0, 0, 0, 0, (byte)'X', (byte)'2', (byte)'5', (byte)'\r', (byte)'\n', 0 };
            }

            SerialPortTest app = new SerialPortTest();
            byte[] Record = new byte[c_BufferSize];
            UInt32 uiRecordNumber = 0x1323;		        // Starting record number (pulled from a hat)

            SerialPort hPort = new SerialPort("COM1", (int)BaudRate.Baudrate115200);
            
            hPort.Handshake = handshake;

            hPort.ReadTimeout = 500;

            hPort.Open();

            hPort.Flush();

            while (0 < hPort.Read(Record, 0, Record.Length)) ;
            // Note:
            // When a hardware reset is issued to some targets prior to a deploy, the original deployed
            // code begins to execute for a second or two before Visual Studio is able to gain control,
            // halt the old deployed code and deploy the current program.
            // The following two second delay is to prevent this app from issuing the start record prior
            // to VS gaining control.  Otherwise, the PC side app will get the signal to start before
            // the target side app has even deployed.  This will result in the loss of the first three
            // or four data records and the test will fail.
            Thread.Sleep(2000);

            // Send out a simple signal to show the PC side test software that we're ready to go
            hPort.Write(start, 0, start.Length);

            // Test COM port handshake by flooding the port with testable sequential records
            // and verifying that the records arrive correctly.  The handshake is verified
            // by causing 5 second pauses during reading which should translate to five
            // second pauses in the transmit as well - unless the driver buffer is obviating
            // the need for handshake - in which case, the buffer should be diminished or the
            // volume of traffic should increase until handshake must be employed.
            for (int i = 0; i < c_NumRecords; i++)
            {
                if (!app.ReceiveRecord(hPort, Record))				// If there were problems
                {
                    Debug.Print("There were problems with test #1, record #" + toString(i + 1) + " of " + toString(c_NumRecords));
                    // return;		// We may as well quit now
                }
                //Debug.Print(AsString(Record));
                //Debug.Print(new string(System.Text.UTF8Encoding.UTF8.GetChars(Record)));
                if (!app.CheckRecord(uiRecordNumber, Record))	// If there were problems
                {
                    Debug.Print("There were problems with test #1, record #" + toString(i + 1) + " of " + toString(c_NumRecords));
                    //return;		// We may as well quit now
                }
                uiRecordNumber++;		// Next record
            }
            for (int i = 0; i < c_NumRecords; i++)
            {
                app.CreateRecord(uiRecordNumber, Record);

                //Debug.Print("\nSending Record: " + uiRecordNumber);
                //Debug.Print(new string(System.Text.UTF8Encoding.UTF8.GetChars(Record)));
                if(!app.SendRecord(hPort, Record))      // If there were problems
                {
                    Debug.Print("There were problems with test #2, record #" + toString(i + 1) + " of " + toString(c_NumRecords));
                    return;		// We may as well quit now
                }
                uiRecordNumber++;
            }
            // This is just like the previous test - except that a gap of five seconds
            // will be inserted before receiving the rest of the records which will
            // hopefully hold off transmission due to a functioning handshake.
            for (int i = 0; i < 5; i++)
            {
                if (!app.ReceiveRecord(hPort, Record))				// If there were problems
                {
                    Debug.Print("There were problems with test #3, record #" + toString(i + 1) + " of 5");
                    return;		// We may as well quit now
                }
                if (!app.CheckRecord(uiRecordNumber, Record))	// If there were problems
                {
                    return;		// We may as well quit now
                }
                uiRecordNumber++;		// Next record
            }
            Thread.Sleep(5000);     // Do not allow reception of characters for five seconds (simulate erasing FLASH or some such)
            for (int i = 0; i < (c_NumRecords - 5); i++)
            {
                if (!app.ReceiveRecord(hPort, Record))				// If there were problems
                {
                    Debug.Print("There were problems with test #4, record #" + toString(i + 1) + " of " + toString(c_NumRecords - 5));
                    return;		// We may as well quit now
                }
                if (!app.CheckRecord(uiRecordNumber, Record))	// If there were problems
                {
                    byte[] CRecord = new byte[c_BufferSize];

                    app.CreateRecord(uiRecordNumber, CRecord);
                    Debug.Print("got: " + AsString(Record));
                    Debug.Print("exp: " + AsString(CRecord));
                    Debug.Print("There were problems with test #4, record #" + toString(i + 1) + " of " + toString(c_NumRecords - 5));
                    return;		// We may as well quit now
                }
                uiRecordNumber++;		// Next record
            }
            // During this test, the receiving unit will hopefully hold off which will
            // cause this transmission to also (hopefully) report the same holdoff
            for (int i = 0; i < c_NumRecords; i++)
            {
                app.CreateRecord(uiRecordNumber, Record);
                if (!app.SendRecord(hPort, Record))      // If there were problems
                {
                    Debug.Print("There were problems with test #5, record #" + toString(i + 1) + " of " + toString(c_NumRecords));
                    return;		// We may as well quit now
                }
                uiRecordNumber++;
            }

            Debug.Print("All records apparently sent and received as expected");

            hPort.Dispose();
            GC.WaitForPendingFinalizers();
        }

        bool SendRecord(SerialPort hPort, byte[] record)
        {
            int recordSize = c_BufferSize - 1;
            int bytesSent = 0;
            int nSpins = 0;			// Number of timeouts

            while (bytesSent < recordSize)
            {
                int nBytes;

                hPort.Write(record, bytesSent, recordSize - bytesSent);
                bytesSent += nBytes;
            }
            return true;
        }

        bool ReceiveRecord(SerialPort hPort, byte[] record)
        {
            int bytesReceived = 0;
            int bytesExpected = c_BufferSize - 1;
            int nSpins = 0;

            while (bytesReceived < bytesExpected)
            {
                int nBytes;

                if (bytesExpected <= bytesReceived)		// If the buffer is full, but no end of record received
                {
                    Debug.Print("Received record size is too large - dumping record");
                    bytesReceived = 0;
                    return false;
                }

                nBytes = hPort.Read(record, bytesReceived, bytesExpected - bytesReceived);

                if (nBytes == 0)		// If nothing received in RECEIVE_TIMEOUT mS
                {
                    nSpins++;
                    if (nSpins > (20000 / c_ReceiveTimeout))		// If nothing received in about 20 seconds, something is wrong
                    {
                        Debug.Print("Waited more than 20 seconds for input - giving up!");
                        return false;
                    }
                }
                else
                {
                    if (nSpins != 0)
                    {
                        Debug.Print("Gap in receive of about " + toString(nSpins * c_ReceiveTimeout) + "mS");
                        nSpins = 0;
                    }
                    if (bytesReceived == 0)		// If start of record not yet found
                    {
                        while (nBytes > 0)
                        {
                            if (record[bytesReceived] == (byte)':')		// If start of record found
                            {
                                if (bytesReceived == 0)		// If also first character received, wow, great!
                                {
                                    bytesReceived = nBytes;
                                    break;
                                }

                                Array.Copy(record, bytesReceived, record, 0, nBytes);

                                bytesReceived = nBytes;

                                if (bytesReceived == bytesExpected && record[bytesReceived] == '\r')		// If end of record
                                {
                                    record[bytesReceived] = 0;			// Terminate to form a string
                                    return true;
                                }

                                break;
                            }
                            nBytes--;
                            bytesReceived++;
                        }
                        if (nBytes <= 0)		// If start of record not found
                        {
                            nBytes = 0;			// Dump everything received so far
                            bytesReceived = 0;
                        }
                    }
                    else
                    {
                        while (nBytes != 0)
                        {
                            if (bytesReceived == bytesExpected && record[bytesReceived] == '\r')		// If end of record
                            {
                                record[bytesReceived] = 0;			// Terminate to form a string
                                return true;
                            }
                            bytesReceived++;
                            nBytes--;
                        }
                    }
                }
            }
            if (bytesReceived == bytesExpected && record[bytesReceived] == 0)
                return true;
            return false;
        }

        static void AddChar(byte n, byte[] record, int i)
        {
	        byte c;

            c = (byte)((n >> 4) & 0x0F);
	        record[i++] = (byte)(c > 9 ? c - 10 + 'A' : c + '0');
	        c = (byte)(n & 0x0F);
	        record[i++] = (byte)(c > 9 ? c - 10 + 'A' : c + '0');
        }

        static UInt16 ReadChar(byte[] record, int i)
        {
	        byte nextChar;
	        UInt16 retChar = 256;

	        nextChar = record[i++];
	        if( nextChar >= '0' && nextChar <= '9' )
		        retChar = (UInt16)(16 * (nextChar - '0'));
	        else if( nextChar >= 'A' && nextChar <= 'F' )
		        retChar = (UInt16)(16 * (nextChar - 'A') + 160);
	        else
		        return 256;
	        nextChar = record[i++];
	        if( nextChar >= '0' && nextChar <= '9' )
		        return (UInt16)(retChar + (nextChar - '0'));
	        else if( nextChar >= 'A' && nextChar <= 'F' )
		        return (UInt16)(retChar + (nextChar - 'A') + 10);
	        else
		        return 256;
        }

        void SeedRandom(UInt32 dwSeed)
        {
            // Don't allow a zero seed
            dwPseudoRandom = dwSeed != 0 ? dwSeed : 0x80000000;
        }

        byte GetRandom()
        {
            //return (byte)'a'; // (byte)dwPseudoRandom++;
            /**/
            for (int i = 0; i < 9; i++)
            {
                if( (dwPseudoRandom & 0x80000000) != 0 )
                {
                    dwPseudoRandom = (dwPseudoRandom << 1) ^ 0x122B9027;
                }
                else
                {
                    dwPseudoRandom <<= 1;
                }
            }
            return (byte)(dwPseudoRandom + (dwPseudoRandom >> 8) + (dwPseudoRandom >> 16) + (dwPseudoRandom >> 24));
            /**/
        }

        static string AsString(byte[] Array)
        {
            string retVal = "";

            for (int i = 0; Array[i] != 0; i++)
            {
                retVal += (char)Array[i];
            }
            return retVal;
        }

        void CreateRecord(UInt32 dwStart, byte[] record)
        {
            int i = 0;
            byte sumcheck = (byte)c_RecordSize;
            byte nextChar;

            SeedRandom(dwStart);

            record[i++] = (byte)':';
            AddChar((byte)c_RecordSize, record, i);
            i += 2;
            nextChar = (byte)(dwPseudoRandom >> 24);
            AddChar(nextChar, record, i);
            sumcheck += nextChar;
            i += 2;
            nextChar = (byte)(dwPseudoRandom >> 16);
            AddChar(nextChar, record, i);
            sumcheck += nextChar;
            i += 2;
            nextChar = (byte)(dwPseudoRandom >> 8);
            AddChar(nextChar, record, i);
            sumcheck += nextChar;
            i += 2;
            nextChar = (byte)dwPseudoRandom;
            AddChar(nextChar, record, i);
            sumcheck += nextChar;
            i += 2;
            for (int nada = 0; nada < c_RecordSize; nada++)
            {
                nextChar = GetRandom();
                AddChar(nextChar, record, i);
                sumcheck += nextChar;
                i += 2;
            }
            AddChar(sumcheck, record, i);
            i += 2;
            record[i++] = (byte)'\r';
            record[i++] = (byte)'\n';
            record[i++] = (byte)'\0';
        }

        bool CheckRecord(UInt32 n, byte[] record)
        {
            UInt16 nextChar;
            UInt16 recordSize;
            UInt32 recordNumber;
            byte sumcheck = 0;
            int i = 0;

            nextChar = record[i++];
            if (nextChar != ':')		// Record always starts with ':'
            {
                Debug.Print("Invalid record - starts with 0x" + toString((int)nextChar) + " instead of 0x" + toString((int)':'));
                return false;
            }
            recordSize = ReadChar(record, i);		// Get byte count
            i += 2;
            if (recordSize == 256)
            {
                Debug.Print("Non-hex character found in record size.");
                return false;
            }
            if (recordSize != c_RecordSize)			// If this is an off record size
            {
                if (recordSize > c_RecordSize)
                {
                    Debug.Print("Record size should be " + toString(c_RecordSize) + " but is " + toString(recordSize) + " which is too large");
                    return false;
                }
                Debug.Print("Record size is off (" + toString(recordSize) + " instead of " + toString(c_RecordSize) + ") - continuing");
            }
            sumcheck += (byte)recordSize;
            nextChar = ReadChar(record, i);
            i += 2;
            if (nextChar == 256)
            {
                Debug.Print("Record number is non-hex");
                return false;
            }
            recordNumber = (UInt32)((UInt32)nextChar << 24);
            sumcheck += (byte)nextChar;
            nextChar = ReadChar(record, i);
            i += 2;
            if (nextChar == 256)
            {
                Debug.Print("Record number is non-hex");
                return false;
            }
            recordNumber |= (UInt32)((UInt32)nextChar << 16);
            sumcheck += (byte)nextChar;
            nextChar = ReadChar(record, i);
            i += 2;
            if (nextChar == 256)
            {
                Debug.Print("Record number is non-hex");
                return false;
            }
            recordNumber |= (UInt32)((UInt32)nextChar << 8);
            sumcheck += (byte)nextChar;
            nextChar = ReadChar(record, i);
            i += 2;
            if (nextChar == 256)
            {
                Debug.Print("Record number is non-hex");
                return false;
            }
            recordNumber |= (UInt32)nextChar;
            sumcheck += (byte)nextChar;
            if (recordNumber != n)
            {
                Debug.Print("Unexpected record number (expected " + toString((int)n) + ", received " + toString((int)(recordNumber)) + ") - continuing");
            }
            else
            {
                //Debug.Print("Good Record number " + recordNumber);
            }
            SeedRandom(recordNumber);
            for (int nada = 0; nada < recordSize; nada++)
            {
                UInt16 randomChar;

                nextChar = ReadChar(record, i);
                i += 2;
                if (nextChar == 256)
                {
                    Debug.Print("Received non-hex data character in byte " + toString(nada));
                    return false;
                }
                randomChar = GetRandom();
                if (nextChar != randomChar)
                {
                    Debug.Print("Data in record does not match given record number in byte " + toString(nada) + " (" + toString(nextChar) + " != " + toString(randomChar) + ")");
                    return false;
                }
                sumcheck += (byte)nextChar;
            }
            nextChar = ReadChar(record, i);
            i += 2;
            if (nextChar != sumcheck)
            {
                Debug.Print("Sumchecks do not match (expected " + toString(sumcheck) + ", received " + toString(nextChar) + ")");
                return false;
            }
            return true;
        }

        public static string toString(int n)
        {
            string ret = "";
            char c = (char)((n % 10) + '0');

            n = n / 10;
            if(n != 0)
                ret = toString(n);
            ret += c;
            return ret;
        }

    }
}
