/*
 This C++ NTP client is derived from teh C# NTP client by Valer BOCAN <vbocan@dataman.ro>
 Check out the complete managed implementation at Framework\Core\Drivers\TimeService 
 The managed implementation can be used in place of the native one
 The native implementation is simply an adaptation of the C# code to C++ and teh .NET MF 
 native layer
 */
     
#include "ntp.h"


//--//

SNTPClient::OutstandingQuery SNTPClient::s_queries[MAX_OUTSTANDING_QUERIES];
BOOL SNTPClient::s_queries_initialized = FALSE;
INT32 SNTPClient::s_currentQuerySlot = -1;

void SNTPClient::InitializeOutstandingQueries() 
{ 
    memset(s_queries, 0, MAX_OUTSTANDING_QUERIES * sizeof(OutstandingQuery)); 
}

SNTPClient::OutstandingQuery* SNTPClient::GetQuery(UINT32 server, SOCK_SOCKET s) 
{ 
    // grab a slot
    SNTPClient::OutstandingQuery* query = &s_queries[++s_currentQuerySlot % MAX_OUTSTANDING_QUERIES];    
    // check if it is a free slot
    if(!query->IsActive())
    {        
        query->Initialize(server, s);
        return query;
    }
    // dispose old aged slot
    else if(query->IsActive() && query->IsOld())
    {
        query->Dispose();
        query->Initialize(server, s);
        return query;
    }
    // no slot available
    return NULL;
}

SNTPClient::OutstandingQuery* SNTPClient::FindOutstandingConnection(UINT32 ipPrimary, UINT32 ipAlternate)
{
    for(UINT32 i = 0; i < MAX_OUTSTANDING_QUERIES; ++i)
    {
        SNTPClient::OutstandingQuery* query = &s_queries[i];
        if(query->IsActive() && query->VerifyIpAddress(ipPrimary, ipAlternate)) {
            return query;
        }
    }
    return NULL;
}

//--//

SNTPClient::SNTPClient(UINT32 host)
{
    if(s_queries_initialized == FALSE)
    {
        InitializeOutstandingQueries();
        s_queries_initialized = TRUE;
    }
    
    m_ipAddressPrimary = host;
    m_ipAddressAlternate =  0;
}

SNTPClient::SNTPClient(UINT32* host, UINT32 serverNum)
{
    if(s_queries_initialized == FALSE)
    {
        InitializeOutstandingQueries();
        s_queries_initialized = TRUE;
    }
    
    m_ipAddressPrimary = host[0];
    m_ipAddressAlternate =  host[1];
}

INT32 SNTPClient::Connect()
{
    SOCK_SOCKET timeSocket = SOCK_SOCKET_ERROR;
    INT32 sockErr = 0;
    UINT32 usedServer = 0;

    // look up outstdanding queries for this set of servers
    OutstandingQuery* query = FindOutstandingConnection(m_ipAddressPrimary, m_ipAddressAlternate);
    
    if(query != NULL)
    {
        //
        // signal failed queries
        //
        if(query->IsOld()) 
        {
            query->Dispose();
            query = NULL;
        }
        else
        {
            //
            // resume old connection
            //
            
            timeSocket = query->GetSocket();
        }
    }

    if(timeSocket == SOCK_SOCKET_ERROR)
    {
        //
        // new connection
        //
        
        timeSocket = SOCK_socket(SOCK_AF_INET, SOCK_SOCK_DGRAM, SOCK_IPPROTO_UDP);

        if(timeSocket == SOCK_SOCKET_ERROR)
        {
            sockErr = SOCK_getlasterror();
            return (sockErr == 0 ? HAL_TIMESERVICE_ERROR : sockErr);
        }

        SOCK_sockaddr addr;
        SOCK_sockaddr_in* dst = (SOCK_sockaddr_in*)&addr;

        memset(dst, 0, sizeof(SOCK_sockaddr_in));
        
        dst->sin_family           = SOCK_AF_INET;
        dst->sin_port             = SOCK_htons(123);
        dst->sin_addr.S_un.S_addr = SOCK_htonl(m_ipAddressPrimary);

        usedServer = m_ipAddressPrimary;

        if(SOCK_connect(timeSocket, &addr, sizeof(addr)) == SOCK_SOCKET_ERROR && SOCK_getlasterror() != SOCK_EWOULDBLOCK) 
        {
            if(m_ipAddressAlternate != 0) 
            {
                usedServer = m_ipAddressAlternate;
        
                dst->sin_addr.S_un.S_addr = SOCK_htonl(m_ipAddressAlternate);
                if(SOCK_connect(timeSocket, &addr, sizeof(addr)) == SOCK_SOCKET_ERROR && SOCK_getlasterror() != SOCK_EWOULDBLOCK)
                {
                    sockErr = SOCK_getlasterror();
                    SOCK_close(timeSocket);
                    return (sockErr == 0 ? HAL_TIMESERVICE_ERROR : sockErr);
                }
            }
            else 
            {
                sockErr = SOCK_getlasterror();
                SOCK_close(timeSocket);
                return (sockErr == 0 ? HAL_TIMESERVICE_ERROR : sockErr);
            }
        }

        Initialize();

        int sent = SOCK_send(timeSocket, (char*)SNTPData, sizeof(SNTPData), 0);

        if(sent != sizeof(SNTPData))
        {
            sockErr = SOCK_getlasterror();
            SOCK_close(timeSocket);
            return (sockErr == 0 ? HAL_TIMESERVICE_ERROR : sockErr);
        }
    }

    // retry 10 times every time we stop by
    INT32 retry = 10;
    INT32 bytesToRead = c_SNTPDataLength;
    char* buf = (char*)SNTPData;
    while(retry-- > 0)
    {
        int read = SOCK_recv(timeSocket, buf, bytesToRead, 0);

        if(read < 0 && (sockErr = SOCK_getlasterror()) != SOCK_EWOULDBLOCK)
        {
            SOCK_close(timeSocket);

            return (sockErr == 0 ? HAL_TIMESERVICE_ERROR : sockErr);
        }
        else if(read > 0) 
        {
            bytesToRead -= read;
            if(bytesToRead <= 0) 
            {
                break;
            }
            buf += read;

            // incase we start receiving data towards the end 
            // of the retry limit.
            retry++;
        }
    }

    // if we could not finish reading, then cache and retry later
    // if we read a part of answer, then declare failure
    // in the future we could try and cope with this problem
    if(bytesToRead == c_SNTPDataLength)
    {
        //
        // if this is a new connection, get a slot
        //
        if(query == NULL) {
            query = GetQuery(usedServer, timeSocket);
        }

        return HAL_TIMESERVICE_WANT_READ_WRITE;
    }
    else if(bytesToRead > 0 && bytesToRead < c_SNTPDataLength)
    {
        if(query != NULL)
            query->Dispose();
        
        return HAL_TIMESERVICE_WANT_READ_WRITE;
    }
    else 
    {
        if(query != NULL)
        {
            query->Dispose();
            query = NULL;
        }
        else 
        {
            if( timeSocket != SOCK_SOCKET_ERROR )
            {
                SOCK_close(timeSocket);
                timeSocket = SOCK_SOCKET_ERROR;
            }
        }
    }
    
    DestinationTimestamp = Time_GetUtcTime();
     
    if( !IsResponseValid() )
    {
        if(query != NULL)
        {
            query->Dispose();
            query = NULL;
        }
        else 
        {
            if( timeSocket != SOCK_SOCKET_ERROR )
            {
                SOCK_close(timeSocket);
                timeSocket = SOCK_SOCKET_ERROR;
            }
        }

        return HAL_TIMESERVICE_ERROR;
    }

    return HAL_TIMESERVICE_SUCCESS;
}

_LeapIndicator SNTPClient::getLeapIndicator() {
    // Isolate the two most significant bits
    BYTE val = (BYTE)(SNTPData[0] >> 6);
    switch (val)
    {
        case 0: return NoWarning;
        case 1: return LastMinute61;
        case 2: return LastMinute59;
        case 3: 
        default:
            return Alarm;
    }
}

// Version Number
BYTE SNTPClient::getVersionNumber()
{
    // Isolate bits 3 - 5
    BYTE val = (BYTE)((SNTPData[0] & 0x38) >> 3);
    return val;
}

// Mode
_Mode SNTPClient::getMode()
{
    // Isolate bits 0 - 3
    BYTE val = (BYTE)(SNTPData[0] & 0x7);
    switch (val)
    {
        case 0: 
        case 6: 
        case 7: 
        default:
            return UnknownMode;
        case 1:
            return SymmetricActive;
        case 2:
            return SymmetricPassive;
        case 3:
            return Client;
        case 4:
            return Server;
        case 5:
            return Broadcast;
    }
}

// Stratum
_Stratum SNTPClient::getStratum()
{
    BYTE val = (BYTE)SNTPData[1];
    if (val == 0) return Unspecified;
    else
        if (val == 1) return PrimaryReference;
        else
            if (val <= 15) return SecondaryReference;
            else
                return Reserved;
}

// Poll Interval (in seconds)
UINT32 SNTPClient::getPollInterval()
{
    // Thanks to Jim Hollenhorst <hollenho@attbi.com>
    return 2 << (INT8)SNTPData[2];
}

// Precision (in seconds)
double SNTPClient::getPrecision()
{
    // Thanks to Jim Hollenhorst <hollenho@attbi.com>
    return 2 << (INT8)SNTPData[3];
}

// Root Delay (in milliseconds)
double SNTPClient::getRootDelay()
{
    int temp = 0;
    temp = 256 * (256 * (256 * SNTPData[4] + SNTPData[5]) + SNTPData[6]) + SNTPData[7];
    return 1000 * (((double)temp) / 0x10000);
}

// Root Dispersion (in milliseconds)
double SNTPClient::getRootDispersion()
{
    int temp = 0;
    temp = 256 * (256 * (256 * SNTPData[8] + SNTPData[9]) + SNTPData[10]) + SNTPData[11];
    return 1000 * (((double)temp) / 0x10000);
}


// Reference Timestamp
_DateTime SNTPClient::getReferenceTimestamp()
{
    _DateTime time = ComputeDate(GetMilliSeconds(offReferenceTimestamp));
    // Take care of the time zone
    return time + TIME_ZONE_OFFSET;
}

// Originate Timestamp (T1)
_DateTime SNTPClient::getOriginateTimestamp()
{
    return ComputeDate(GetMilliSeconds(offOriginateTimestamp));
}

// Receive Timestamp (T2)
_DateTime SNTPClient::getReceiveTimestamp()
{
    _DateTime time = ComputeDate(GetMilliSeconds(offReceiveTimestamp));
    // Take care of the time zone
    return time + TIME_ZONE_OFFSET;
}

// Transmit Timestamp (T3)
_DateTime SNTPClient::getTransmitTimestamp()
{
    _DateTime time = ComputeDate(GetMilliSeconds(offTransmitTimestamp));
    // Take care of the time zone
    return time + TIME_ZONE_OFFSET;
}

void SNTPClient::setTransmitTimestamp(_DateTime value)
{
    SetDate(offTransmitTimestamp, value);
}

// Round trip delay (in time units)
TIME SNTPClient::getRoundTripDelay()
{
    // Thanks to DNH <dnharris@csrlink.net>
    INT64 span = (DestinationTimestamp - getOriginateTimestamp()) - (getReceiveTimestamp() - getTransmitTimestamp());
    return span ;
}

// Local clock offset (in time unit)
TIME SNTPClient::getLocalClockOffset()
{
    // Thanks to DNH <dnharris@csrlink.net>
    TIME span = (getReceiveTimestamp() - TIME_ZONE_OFFSET - getOriginateTimestamp()) + (getTransmitTimestamp() - TIME_ZONE_OFFSET - DestinationTimestamp);
    return span / 2;
}

// Check if the response from server is valid
bool SNTPClient::IsResponseValid()
{
    if (getMode() != Server)
    {
        return false;
    }
    else
    {
        return true;
    }
}

// Compute date, given the number of milliseconds since January 1, 1900
_DateTime SNTPClient::ComputeDate(UINT64 milliseconds)
{   
    SYSTEMTIME        st;
    st.wYear         = 1900;
    st.wMonth        = 1;
    st.wDayOfWeek    = 1;
    st.wDay          = 1;
    st.wHour         = 0;
    st.wMinute       = 0;
    st.wSecond       = 0;
    st.wMilliseconds = 0;

    _DateTime time = Time_FromSystemTime(&st);
    time += (milliseconds * TIMEUNIT_TO_MILLISECONDS);
    return time;
}

// Compute the number of milliseconds, given the offset of a 8-BYTE array
UINT64 SNTPClient::GetMilliSeconds(BYTE offset)
{
    UINT64 intpart = 0, fractpart = 0;

    for (int i = 0; i <= 3; i++)
    {
        intpart = 256 * intpart + (UINT64)SNTPData[offset + i];
    }
    for (int i = 4; i <= 7; i++)
    {
        fractpart = 256 * fractpart + (UINT64)SNTPData[offset + i];
    }
    UINT64 milliseconds = intpart * 1000ull + ((UINT64)(fractpart * 1000) / 0x100000000ull);
    return milliseconds;
}

// Compute the 8-BYTE array, given the date
void SNTPClient::SetDate(BYTE offset, _DateTime date)
{
    UINT64 intpart = 0, fractpart = 0;
    
    SYSTEMTIME        st;
    st.wYear         = 1900;
    st.wMonth        = 1;
    st.wDayOfWeek    = 1;
    st.wDay          = 1;
    st.wHour         = 0;
    st.wMinute       = 0;
    st.wSecond       = 0;
    st.wMilliseconds = 0;

    _DateTime startOfCentury = Time_FromSystemTime(&st);

    SYSTEMTIME st1;
    Time_ToSystemTime(startOfCentury, &st1);

    // MS-CHANGE
    //ulong milliseconds = (ulong)(date - StartOfCentury).TotalMilliseconds;
    UINT64 milliseconds = (UINT64)((date - startOfCentury) / TIMEUNIT_TO_MILLISECONDS);
    intpart = milliseconds / 1000;
    fractpart = ((milliseconds % 1000) * 0x100000000ULL) / 1000;

    UINT64 temp = intpart;
    for (int i = 3; i >= 0; i--)
    {
        SNTPData[offset + i] = (BYTE)(temp % 256);
        temp = temp / 256;
    }

    temp = fractpart;
    for (int i = 7; i >= 4; i--)
    {
        SNTPData[offset + i] = (BYTE)(temp % 256);
        temp = temp / 256;
    }
}

// Initialize the NTPClient data
void SNTPClient::Initialize()
{
    // Set version number to 4 and Mode to 3 (client)
    SNTPData[0] = 0x1B;
    // Initialize all other fields with 0
    for (int i = 1; i < c_SNTPDataLength; i++)
    {
        SNTPData[i] = 0;
    }
    // Initialize the transmit timestamp
    setTransmitTimestamp(Time_GetUtcTime());
}

//--//
//--//
//--//

/*
// Converts the object to string
public override string ToString()
{
    string str;

    str = "Leap Indicator: ";
    switch (LeapIndicator)
    {
        case _LeapIndicator.NoWarning:
            str += "No warning";
            break;
        case _LeapIndicator.LastMinute61:
            str += "Last minute has 61 seconds";
            break;
        case _LeapIndicator.LastMinute59:
            str += "Last minute has 59 seconds";
            break;
        case _LeapIndicator.Alarm:
            str += "Alarm Condition (clock not synchronized)";
            break;
    }
    str += "\r\nVersion number: " + VersionNumber.ToString() + "\r\n";
    str += "Mode: ";
    switch (Mode)
    {
        case _Mode.Unknown:
            str += "Unknown";
            break;
        case _Mode.SymmetricActive:
            str += "Symmetric Active";
            break;
        case _Mode.SymmetricPassive:
            str += "Symmetric Pasive";
            break;
        case _Mode.Client:
            str += "Client";
            break;
        case _Mode.Server:
            str += "Server";
            break;
        case _Mode.Broadcast:
            str += "Broadcast";
            break;
    }
    str += "\r\nStratum: ";
    switch (Stratum)
    {
        case _Stratum.Unspecified:
        case _Stratum.Reserved:
            str += "Unspecified";
            break;
        case _Stratum.PrimaryReference:
            str += "Primary Reference";
            break;
        case _Stratum.SecondaryReference:
            str += "Secondary Reference";
            break;
    }
    str += "\r\nLocal time: " + TransmitTimestamp.ToString();
    str += "\r\nPrecision: " + Precision.ToString() + " s";
    str += "\r\nPoll Interval: " + PollInterval.ToString() + " s";
    str += "\r\nReference ID: " + ReferenceID.ToString();
    str += "\r\nRoot Delay: " + RootDelay.ToString() + " ms";
    str += "\r\nRoot Dispersion: " + RootDispersion.ToString() + " ms";
    str += "\r\nRound Trip Delay: " + RoundTripDelay.ToString() + " ms";
    str += "\r\nLocal Clock Offset: " + LocalClockOffset.ToString() + " ms";
    str += "\r\n";

    return str;
}
*/

