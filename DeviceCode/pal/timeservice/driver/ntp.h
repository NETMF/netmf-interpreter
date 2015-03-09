/*
 This C++ NTP client is derived from teh C# NTP client by Valer BOCAN <vbocan@dataman.ro>
 Check out the complete managed implementation at Framework\Core\Drivers\TimeService 
 The managed implementation can be used in place of the native one
 The native implementation is simply an adaptation of the C# code to C++ and teh .NET MF 
 native layer
 */
     
#include <tinyhal.h>

//--//

#ifndef _DRIVERS_TIMESERVICE_NTP_DECL_H_
#define _DRIVERS_TIMESERVICE_NTP_DECL_H_ 1

//--//

typedef TIME _DateTime;

// Leap indicator field values
enum _LeapIndicator
{
    NoWarning,        // 0 - No warning
    LastMinute61,     // 1 - Last minute has 61 seconds
    LastMinute59,     // 2 - Last minute has 59 seconds
    Alarm             // 3 - Alarm condition (clock not synchronized)
};

//Mode field values
enum _Mode
{
    SymmetricActive,  // 1 - Symmetric active
    SymmetricPassive, // 2 - Symmetric pasive
    Client,           // 3 - Client
    Server,           // 4 - Server
    Broadcast,        // 5 - Broadcast
    UnknownMode       // 0, 6, 7 - Reserved
};

// Stratum field values
enum _Stratum
{
    Unspecified,       // 0 - unspecified or unavailable
    PrimaryReference,  // 1 - primary reference (e.g. radio-clock)
    SecondaryReference,// 2-15 - secondary reference (via NTP or SNTP)
    Reserved           // 16-255 - reserved
};

/// <summary>
/// SNTPClient is a C# class designed to connect to time servers on the Internet and
/// fetch the current date and time. Optionally, it may update the time of the local system.
/// The implementation of the protocol is based on the RFC 2030.
/// 
/// Public class members:
///
/// LeapIndicator - Warns of an impending leap second to be inserted/deleted in the last
/// minute of the current day. (See the _LeapIndicator enum)
/// 
/// VersionNumber - Version number of the protocol (3 or 4).
/// 
/// Mode - Returns mode. (See the _Mode enum)
/// 
/// Stratum - Stratum of the clock. (See the _Stratum enum)
/// 
/// PollInterval - Maximum interval between successive messages
/// 
/// Precision - Precision of the clock
/// 
/// RootDelay - Round trip time to the primary reference source.
/// 
/// RootDispersion - Nominal error relative to the primary reference source.
/// 
/// ReferenceID - Reference identifier (either a 4 character string or an IP address).
/// 
/// ReferenceTimestamp - The time at which the clock was last set or corrected.
/// 
/// OriginateTimestamp - The time at which the request departed the client for the server.
/// 
/// ReceiveTimestamp - The time at which the request arrived at the server.
/// 
/// Transmit Timestamp - The time at which the reply departed the server for client.
/// 
/// RoundTripDelay - The time between the departure of request and arrival of reply.
/// 
/// LocalClockOffset - The offset of the local clock relative to the primary reference
/// source.
/// 
/// Initialize - Sets up data structure and prepares for connection.
/// 
/// Connect - Connects to the time server and populates the data structure.
///    It can also update the system time.
/// 
/// IsResponseValid - Returns true if received data is valid and if comes from
/// a NTP-compliant time server.
/// 
/// ToString - Returns a string representation of the object.
/// 
/// -----------------------------------------------------------------------------
/// Structure of the standard NTP header (as described in RFC 2030)
///                       1                   2                   3
///   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |LI | VN  |Mode |    Stratum    |     Poll      |   Precision   |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                          Root Delay                           |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                       Root Dispersion                         |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                     Reference Identifier                      |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                                                               |
///  |                   Reference Timestamp (64)                    |
///  |                                                               |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                                                               |
///  |                   Originate Timestamp (64)                    |
///  |                                                               |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                                                               |
///  |                    Receive Timestamp (64)                     |
///  |                                                               |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                                                               |
///  |                    Transmit Timestamp (64)                    |
///  |                                                               |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                 Key Identifier (optional) (32)                |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
///  |                                                               |
///  |                                                               |
///  |                 Message Digest (optional) (128)               |
///  |                                                               |
///  |                                                               |
///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// 
/// -----------------------------------------------------------------------------
/// 
/// SNTP Timestamp Format (as described in RFC 2030)
///                         1                   2                   3
///     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                           Seconds                             |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                  Seconds Fraction (0-padded)                  |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// 
/// </summary>

class SNTPClient
{
private:

    class OutstandingQueryGuard;
    
    class OutstandingQuery
    {

        friend class OutstandingQueryGuard;
        //--//
        #define MAX_OUTSTANDING_QUERIES  5
        #define MAX_QUERY_AGE  100000000 // 10 seconds 
        //--//
        TIME m_server;
        SOCK_SOCKET m_socket;
        TIME m_timeStamp;
        BOOL m_active;
    public:
        //--//
        OutstandingQuery() : m_server(0), m_socket(SOCK_SOCKET_ERROR), m_timeStamp(0x0ull), m_active(FALSE) 
        {
        }
        void Initialize(UINT32 server, SOCK_SOCKET s) 
        { 
            m_server = server; 
            m_socket = s; 
            m_timeStamp = Time_GetUtcTime(); 
            m_active = TRUE; 
        }
        BOOL VerifyIpAddress(UINT32 ipPrimary, UINT32 ipAlternate)
        {
            if(ipPrimary == m_server || ipAlternate == m_server)
            {
                return TRUE;
            }
            return FALSE;
        }
        BOOL IsActive() 
        {
            return m_active;
        }
        BOOL IsOld()
        {
            INT64 diff = Time_GetUtcTime() - m_timeStamp;
        
            return (diff > MAX_QUERY_AGE) || (diff < 0);
        }
        SOCK_SOCKET GetSocket() 
        { 
            return m_socket; 
        }
        void Dispose() 
        { 
            m_active = FALSE; 
            SOCK_close(m_socket); 
        }
    };
        
    static INT32 s_currentQuerySlot;
    static OutstandingQuery s_queries[MAX_OUTSTANDING_QUERIES];
    static BOOL s_queries_initialized;

    static void InitializeOutstandingQueries();
    static OutstandingQuery* GetQuery(UINT32 server, SOCK_SOCKET s);
    static OutstandingQuery* FindOutstandingConnection(UINT32 ipPrimary, UINT32 ipAlternate);
        
    //--//
    
    // SNTP Data Structure Length
    static const BYTE c_SNTPDataLength = 48;

    // Offset constants for timestamps in the data structure
    static const BYTE offReferenceID = 12;
    static const BYTE offReferenceTimestamp = 16;
    static const BYTE offOriginateTimestamp = 24;
    static const BYTE offReceiveTimestamp = 32;
    static const BYTE offTransmitTimestamp = 40;

    //--//
    
    // SNTP Data Structure (as described in RFC 2030)
    BYTE SNTPData[c_SNTPDataLength];

    // The IP Address of the time server we are connecting to
    UINT32 m_ipAddressPrimary;
    UINT32 m_ipAddressAlternate;
    
public:
    static const UINT32 TIMEUNIT_TO_MILLISECONDS = 10000;
    
    // Destination Timestamp (T4)
    _DateTime DestinationTimestamp;

    SNTPClient(UINT32 host);

    SNTPClient(UINT32* host, UINT32 serverNum);

    // Connect to the time server and update system time
    INT32 Connect();
    
    // Leap Indicator
    _LeapIndicator getLeapIndicator();

    // Version Number
    BYTE getVersionNumber();

    // Mode
    _Mode getMode();

    // Stratum
    _Stratum getStratum();

    // Poll Interval (in seconds)
    UINT32 getPollInterval();

    // Precision (in seconds)
    double getPrecision();

    // Root Delay (in milliseconds)
    double getRootDelay();

    // Root Dispersion (in milliseconds)
    double getRootDispersion();

    
    // Reference Timestamp
    _DateTime getReferenceTimestamp();

    // Originate Timestamp (T1)
    _DateTime getOriginateTimestamp();

    // Receive Timestamp (T2)
    _DateTime getReceiveTimestamp();

    // Transmit Timestamp (T3)
    _DateTime getTransmitTimestamp();
    
    void setTransmitTimestamp(_DateTime value);

    // Round trip delay (in milliseconds)
    TIME getRoundTripDelay();

    // Local clock offset (in milliseconds)
    TIME getLocalClockOffset();

    // Check if the response from server is valid
    bool IsResponseValid();

private:
    // Compute date, given the number of milliseconds since January 1, 1900
    _DateTime ComputeDate(UINT64 milliseconds);

    // Compute the number of milliseconds, given the offset of a 8-BYTE array
    UINT64 GetMilliSeconds(BYTE offset);
    
    // Compute the 8-BYTE array, given the date
    void SetDate(BYTE offset, _DateTime date);

    // Initialize the NTPClient data
    void Initialize();
};

//--//

#endif // _DRIVERS_TIMESERVICE_NTP_DECL_H_

//--//
