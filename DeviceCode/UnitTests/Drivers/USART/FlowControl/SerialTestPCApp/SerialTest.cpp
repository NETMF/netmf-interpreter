// SerialTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#define RECORD_SIZE    50
#define RECORD_HEADER  16
#define BUFFER_SIZE    2*RECORD_SIZE + RECORD_HEADER

// The number of records to send/receive for each test
#define NUM_RECORDS 100

#define USART_FLOW_HW_IN_EN   0x01
#define USART_FLOW_HW_OUT_EN  0x02
#define USART_FLOW_SW_IN_EN   0x04
#define USART_FLOW_SW_OUT_EN  0x08

// Set the timeout in mS for send and receive
#define RECEIVE_TIMEOUT 10
#define TRANSMIT_TIMEOUT 20

// Set this to perform the test...
UINT32 HANDSHAKE = 0;
//#define HANDSHAKE 0
//#define HANDSHAKE (USART_FLOW_HW_IN_EN | USART_FLOW_HW_OUT_EN)
//#define HANDSHAKE (USART_FLOW_SW_IN_EN | USART_FLOW_SW_OUT_EN)

PWCHAR portName = L"COM2:";			// Standard serial port
UINT32 portBaud = 115200;
//PWCHAR portName = L"COM3:";		// USB based serial port for FFUART on Mote2 (sometimes)

HANDLE SetupSerialPort(PWCHAR portName);
void CreateRecord(DWORD n, UINT8 *record);
BOOL CheckRecord(DWORD n, UINT8 *record);
void SeedRandom(DWORD dwSeed);
UINT8 GetRandom();
void AddChar(UINT8 n, UINT8 *&record);
int ReadChar(UINT8 *&record);
BOOL SendRecord(HANDLE hPort, UINT8 *record);
BOOL ReceiveRecord(HANDLE hPort, UINT8 *record);
void WaitForStart( HANDLE hPort );


DWORD dwPseudoRandom;

int _tmain(int argc, _TCHAR* argv[])
{
	HANDLE hPort;
	UINT8 record[BUFFER_SIZE];
	DWORD dwRecordNumber;

    if(argc >= 2)
    {
        portName = argv[1];
    }
    if(argc >= 3)
    {
        portBaud = _wtoi(argv[2]);
    }

while(TRUE)
{
    dwRecordNumber = 0x1323;		// Starting record number (pulled from a hat)
    hPort = SetupSerialPort(portName);

    printf("%ls: (%d baud) opened OK - Waiting for connection...\n", portName, portBaud);

	if(hPort == INVALID_HANDLE_VALUE)
	{
		printf("Port couldn't be opened.\n");
		return -1;
	}
	WaitForStart( hPort );

	// Test COM port handshake by flooding the port with testable sequential records
	// and verifying that the records arrive correctly.  The handshake is verified
	// by causing 5 second pauses during reading which should translate to five
	// second pauses in the transmit as well - unless the driver buffer is obviating
	// the need for handshake - in which case, the buffer should be dimished or the
	// volume of traffic should increase until handshake must be employed.
	for(int i=0; i < NUM_RECORDS; i++)
	{
		CreateRecord(dwRecordNumber, record);		// Create a large, testable record
		if(!SendRecord(hPort, record))				// If there were problems
		{
			printf("Encountered error on test #1 while outputing record #%d\n", i+1);
			goto BLAH; //return -1;		// We may as well quit now
		}
		dwRecordNumber++;		// Next record
	}
	printf("Successfully completed test #1\n");
	for(int i=0; i < NUM_RECORDS; i++)
	{
		if(!ReceiveRecord(hPort, record))		// If there were problems
		{
			printf("Encountered error on test #2 while inputing record #%d\n", i+1);
			goto BLAH; //return -1;		// We may as well quit now
		}
		if(!CheckRecord(dwRecordNumber, record))	// If there were problems
		{
			printf("Encountered error on test #2 while checking record #%d\n", i+1);
			goto BLAH; //return -1;		// We may as well quit now
		}
        //printf("Good Record: %d\n", dwRecordNumber);
		dwRecordNumber++;
	}
	printf("Successfully completed test #2\n");
	// This is just like the previous test - except the receiver will hold off
	// for 5 seconds requiring the use of handshake (hopefully)
	for(int i=0; i < NUM_RECORDS; i++)
	{
		CreateRecord(dwRecordNumber, record);		// Create a large, testable record
		if(!SendRecord(hPort, record))				// If there were problems
		{
			printf("Encountered error on test #3 while sending record #%d\n", i+1);
			goto BLAH; //return -1;		// We may as well quit now
		}
		dwRecordNumber++;		// Next record
	}
	printf("Successfully completed test #3\n");
	// Send five records before holding off receiving characters for five seconds
	for(int i=0; i < 5; i++)
	{
		if(!ReceiveRecord(hPort, record))		// If there were problems
		{
			printf("Encountered error on test #3 while inputting record #%d\n", i+1);
			goto BLAH; //return -1;		// We may as well quit now
		}
		if(!CheckRecord(dwRecordNumber, record))	// If there were problems
		{
			printf("Encountered error on test #3 while checking record #%d\n", i+1);
			goto BLAH; //return -1;		// We may as well quit now
		}
		dwRecordNumber++;
	}
	printf("Holding off input for five seconds\n");
	Sleep(5000);		// Don't allow communications for five seconds
	// Receive the balance of the records
	for(int i=0; i < (NUM_RECORDS-5); i++)
	{
		if(!ReceiveRecord(hPort, record))		// If there were problems
		{
			printf("Encountered error on test #4 while inputting record #%d\n", i+1);
			goto BLAH; //return -1;		// We may as well quit now
		}
		if(!CheckRecord(dwRecordNumber, record))	// If there were problems
		{
			printf("Encountered error on test #4 while checking record #%d\n", i+1);
			goto BLAH; //return -1;		// We may as well quit now
		}
		dwRecordNumber++;
	}
	printf("All records apparently sent and received as expected\n");

BLAH:

	CloseHandle(hPort);

printf( "Done... Restarting...\n\n");
}

	return 0;
}


BOOL SendRecord(HANDLE hPort, UINT8 *record)
{
	int recordSize = (int)strlen((CHAR*)record);
	int bytesSent = 0;
	int nSpins = 0;			// Number of timeouts

	while(bytesSent < recordSize)
	{
		DWORD nBytes;

		WriteFile(hPort, &record[bytesSent], recordSize - bytesSent, &nBytes, NULL);
		if(nBytes == 0)
		{
			nSpins++;		// Each timeout is about TRANSMIT_TIMEOUT mS
			if(nSpins > (10000 / TRANSMIT_TIMEOUT))		// If more than 10 seconds have passed
			{
				printf("Waited for more than 10 seconds - something's gone wrong\n");
				return FALSE;
			}
		}
		else
		{
			if(nSpins)
			{
				printf("There was a gap of %d mS\n", nSpins * TRANSMIT_TIMEOUT);
				nSpins = 0;
			}
			bytesSent += nBytes;
		}
	}
	return TRUE;
}


BOOL ReceiveRecord(HANDLE hPort, UINT8 *record)
{
	int bytesReceived = 0;
	int bytesExpected = BUFFER_SIZE - 1;
	int nSpins = 0;
	DWORD ComErrors;

	while(bytesReceived < bytesExpected)
	{
		DWORD nBytes;

		if(bytesExpected <= bytesReceived)		// If the buffer is full, but no end of record received
		{
			printf("Received record size is too large - dumping record\n");
			*record = 0;
			return FALSE;
		}
		if(!ReadFile(hPort, &record[bytesReceived], bytesExpected - bytesReceived, &nBytes, NULL) )
		{
			if( GetLastError() != 0 )
				ClearCommError( hPort, &ComErrors, NULL );
		}

		if(nBytes == 0)		// If nothing received in RECEIVE_TIMEOUT mS
		{
			nSpins++;
			if(nSpins > (20000 / RECEIVE_TIMEOUT))		// If nothing received in about 20 seconds, something is wrong
			{
				printf("Waited more than 20 seconds for input - giving up!\n");
				return FALSE;
			}
		}
		else
		{
			record[bytesReceived+nBytes] = 0;
			//printf("%s",&record[bytesReceived]);
			if(nSpins)
			{
				printf("Gap in receive of about %d mS\n", nSpins * RECEIVE_TIMEOUT);
				nSpins = 0;
			}
			if(bytesReceived == 0)		// If start of record not yet found
			{
				while(nBytes > 0)
				{
					if(record[bytesReceived] == ':')		// If start of record found
					{
						if(bytesReceived == 0)		// If also first character received, wow, great!
                        {
                            bytesReceived = nBytes;
							break;
                        }

						memcpy(record, &record[bytesReceived], nBytes);

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
				if(nBytes <= 0)		// If start of record not found
				{
					nBytes = 0;			// Dump everything received so far
					bytesReceived = 0;
				}
			}
            else
            {
			    while(nBytes != 0)
			    {
                    if (bytesReceived == bytesExpected && record[bytesReceived] == '\r')		// If end of record
				    {
					    record[bytesReceived] = 0;			// Terminate to form a string
					    return TRUE;
				    }
				    bytesReceived++;
				    nBytes--;
			    }
            }
		}
	}
    if (bytesReceived == bytesExpected && record[bytesReceived] == 0)
		return TRUE;
	return FALSE;
}

void SetupSerialPortDCB(HANDLE hPort)
{
    DCB    portDCB;

	if(hPort == INVALID_HANDLE_VALUE)
		return;

	portDCB.DCBlength = sizeof(DCB);

	GetCommState(hPort, &portDCB);

	// Set up serial port parameters
	portDCB.BaudRate        = portBaud;
	portDCB.ByteSize        = 8;
	portDCB.fBinary         = TRUE;
	portDCB.fDsrSensitivity = FALSE;
	portDCB.fDtrControl     = DTR_CONTROL_DISABLE;
	portDCB.fInX            = (HANDSHAKE & USART_FLOW_SW_IN_EN ) ? 1 : 0;
	portDCB.fNull           = FALSE;
	portDCB.fOutX           = (HANDSHAKE & USART_FLOW_SW_OUT_EN) ? 1 : 0;
	portDCB.fOutxCtsFlow    = (HANDSHAKE & USART_FLOW_HW_OUT_EN) ? 1 : 0;
	portDCB.fOutxDsrFlow    = FALSE;
	portDCB.fParity         = TRUE;
	portDCB.fRtsControl     = (HANDSHAKE & USART_FLOW_HW_IN_EN ) ? RTS_CONTROL_HANDSHAKE : RTS_CONTROL_ENABLE;
	portDCB.fTXContinueOnXoff = FALSE;
	portDCB.Parity          = NOPARITY;
	portDCB.StopBits        = ONESTOPBIT;
	portDCB.XoffChar        = 0x13;		// XOFF
	portDCB.XonChar         = 0x11;		// XON

	SetCommState(hPort, &portDCB);
}

HANDLE SetupSerialPort(PWCHAR portName)
{
	HANDLE hPort;
	COMMTIMEOUTS portTimeouts;

	hPort = CreateFile(portName, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
	if(hPort == INVALID_HANDLE_VALUE)
		return hPort;

	// Set up the serial port timeouts
	GetCommTimeouts(hPort, &portTimeouts);

	// RECEIVE_TIMEOUT mS timeout for read and TRANSMIT_TIMEOUT mS timeout for write
	portTimeouts.ReadIntervalTimeout = 0;
	portTimeouts.ReadTotalTimeoutConstant = RECEIVE_TIMEOUT;
	portTimeouts.ReadTotalTimeoutMultiplier = 0;
	portTimeouts.WriteTotalTimeoutConstant = TRANSMIT_TIMEOUT;
	portTimeouts.WriteTotalTimeoutMultiplier = 0;

	SetCommTimeouts(hPort, &portTimeouts);

	SetupSerialPortDCB(hPort);

	PurgeComm( hPort, PURGE_TXCLEAR | PURGE_RXCLEAR );

	return hPort;
}


void AddChar(UINT8 n, UINT8 *&record)
{
	UINT8 c;

	c = (n / 16) & 0x0F;
	*record++ = c > 9 ? c - 10 + 'A' : c + '0';
	c = n & 0x0F;
	*record++   = c > 9 ? c - 10 + 'A' : c + '0';
}

int ReadChar(UINT8 *&record)
{
	UINT8 nextChar;
	int retChar = -1;

	nextChar = *record++;
	if(nextChar >= '0' && nextChar <= '9')
		retChar = 16 * (nextChar - '0');
	else if(nextChar >= 'A' && nextChar <= 'F')
		retChar = 16 * (nextChar - 'A') + 160;
	else
		return -1;
	nextChar = *record++;
	if(nextChar >= '0' && nextChar <= '9')
		return retChar + (nextChar - '0');
	else if(nextChar >= 'A' && nextChar <= 'F')
		return retChar + (nextChar - 'A') + 10;
	else
		return -1;
}

void CreateRecord(DWORD dwStart, UINT8 *record)
{
	UINT8 *ptr = record;
	UINT8  sumcheck = RECORD_SIZE;
	UINT8  nextChar;

	SeedRandom(dwStart);

	*ptr++ = ':';
	AddChar(RECORD_SIZE, ptr);
	nextChar = (CHAR)(dwPseudoRandom >> 24);
	AddChar(nextChar, ptr);
	sumcheck += nextChar;
	nextChar = (CHAR)(dwPseudoRandom >> 16);
	AddChar(nextChar, ptr);
	sumcheck += nextChar;
	nextChar = (CHAR)(dwPseudoRandom >> 8);
	AddChar(nextChar, ptr);
	sumcheck += nextChar;
	nextChar = (CHAR)dwPseudoRandom;
	AddChar(nextChar, ptr);
	sumcheck += nextChar;
	for(int i=0; i < RECORD_SIZE; i++)
	{
		nextChar = GetRandom();
		AddChar(nextChar, ptr);
		sumcheck += nextChar;
	}
	AddChar(sumcheck, ptr);
	*ptr++ = '\r';
	*ptr++ = '\n';
	*ptr++ = '\0';
}

BOOL CheckRecord(DWORD n, UINT8 *record)
{
	int nextChar;
	int recordSize;
	DWORD recordNumber;
	UINT8 sumcheck = 0;

	nextChar = *record++;
	if(nextChar != ':')		// Record always starts with ':'
	{
		printf("Invalid record - starts with 0x%02X instead of 0x%02X\n", nextChar, (int)':');
		return FALSE;
	}
	recordSize = ReadChar(record);		// Get byte count
	if(recordSize == -1)
	{
		printf("Non-hex character found in record size\n");
		return FALSE;
	}
	if(recordSize != RECORD_SIZE)			// If this is an off record size
	{
		if(recordSize > RECORD_SIZE)
		{
			printf("Record size should be %d but is %d which is too large\n", recordSize, RECORD_SIZE);
			return FALSE;
		}
		printf("Record size is off (%d instead of %d) - continuing\n", recordSize, RECORD_SIZE);
	}
	sumcheck += recordSize;
	nextChar = ReadChar(record);
	if(nextChar == -1)
	{
		printf("Record number is non-hex\n");
		return FALSE;
	}
	recordNumber = nextChar << 24;
	sumcheck += nextChar;
	nextChar = ReadChar(record);
	if(nextChar == -1)
	{
		printf("Record number is non-hex\n");
		return FALSE;
	}
	recordNumber |= nextChar << 16;
	sumcheck += nextChar;
	nextChar = ReadChar(record);
	if(nextChar == -1)
	{
		printf("Record number is non-hex\n");
		return FALSE;
	}
	recordNumber |= nextChar << 8;
	sumcheck += nextChar;
	nextChar = ReadChar(record);
	if(nextChar == -1)
	{
		printf("Record number is non-hex\n");
		return FALSE;
	}
	recordNumber |= nextChar;
	sumcheck += nextChar;
	if(recordNumber != n)
	{
		printf("Unexpected record number (expected %X, received %X) - continuing\n", n, recordNumber);
	}
	SeedRandom(recordNumber);
	for(int i=0; i < recordSize; i++)
	{
		UINT8 randomChar;

		nextChar = ReadChar(record);
		if(nextChar == -1)
		{
			printf("Received non-hex data character in byte %d\n", i);
			return FALSE;
		}
		randomChar = GetRandom();
		if(nextChar != randomChar)
		{
			printf("Data in record does not match given record number in byte %d (%02X != %02X)\n", i, nextChar, randomChar);
			return FALSE;
		}
		sumcheck += nextChar;
	}
	nextChar = ReadChar(record);
	if(nextChar != sumcheck)
	{
		printf("Sumchecks do not match (expected %02X, received %02X)\n", sumcheck, nextChar);
		return  FALSE;
	}
	return TRUE;
}


void SeedRandom(DWORD dwSeed)
{
	// Don't allow a zero seed
	dwPseudoRandom = dwSeed ? dwSeed : 0x80000000;
}

UINT8 GetRandom()
{
	for(int i=0; i<9; i++)
	{
		if(dwPseudoRandom & 0x80000000)
		{
			dwPseudoRandom = (dwPseudoRandom << 1) ^ 0x122B9027;
		}
		else
		{
			dwPseudoRandom <<= 1;
		}
	}
	return (CHAR)(dwPseudoRandom + (dwPseudoRandom >> 8) + (dwPseudoRandom >> 16) + (dwPseudoRandom >> 24));
}

void WaitForStart( HANDLE hPort )
{
	int state = 0;
	DWORD nBytes;
	char c;
	int error = 0;

	while( state < 3 )
	{
		if( !ReadFile(hPort, &c, 1, &nBytes, NULL) )
		{
			DWORD Errors;

			error = GetLastError();		// Boards like Digi change baud rates a couple of times which cause framing errors
			if( error != 0 )
			{
				ClearCommError( hPort, &Errors, NULL );		// Which must be cleared or communications breaks down
			}
		}
		if( nBytes )
		{
			switch( state )
			{
			case 0:
				if( c == 'X' )
					state++;
				break;
			case 1:
				if( c == '2' )
					state++;
				else
					state = 0;
				break;
			case 2:
				if( c == '3' )
				{
					printf("Received no HS start from target\n");
                    HANDSHAKE = 0;
					state++;
				}
				else if (c == '4')
				{
					printf("Received SW HS start from target\n");
                    HANDSHAKE = (USART_FLOW_SW_IN_EN | USART_FLOW_SW_OUT_EN);
					state++;
				}
				else if (c == '5')
				{
					printf("Received HW HS start from target\n");
                    HANDSHAKE = (USART_FLOW_HW_IN_EN | USART_FLOW_HW_OUT_EN);
					state++;
				}
				else if (c == '6')
				{
					printf("Apparent error setting HW HS, reverted to SW HS\n");
					state++;
				}
				else
					state = 0;
				break;
			}
		}
	}

    SetupSerialPortDCB(hPort);

	while( nBytes != 0 )
		ReadFile( hPort, &c, 1, &nBytes, NULL );		// Empty any junk from the buffer
}

