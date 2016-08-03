////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _SUPPORT_WIREPROTOCOL_H_
#define _SUPPORT_WIREPROTOCOL_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(_MSC_VER)
#pragma pack(push, SUPPORT_WIREPROTOCOL_H_, 4)
#endif

#include <TinySupport.h>
#include <tinyhal.h>    //  Needed because of reference to ....  

////////////////////////////////////////////////////////////////////////////////////////////////////

//
// Keep these strings less than 7-characters long!! They are stuffed into an 8-byte structure (\0 terminated).
//
#define MARKER_DEBUGGER_V1 "MSdbgV1" // Used to identify the debugger at boot time.
#define MARKER_PACKET_V1   "MSpktV1" // Used to identify the start of a packet.

//--//

struct WP_Packet;
struct WP_PhysicalLayer;
struct WP_ApplicationLayer;
struct WP_Message;
struct WP_Controller;

//--//

struct WP_Flags
{
    static const UINT16 c_NonCritical = 0x0001; // This doesn't need an acknowledge.
    static const UINT16 c_Reply       = 0x0002; // This is the result of a command.
    static const UINT16 c_BadHeader   = 0x0004;
    static const UINT16 c_BadPayload  = 0x0008;
    static const UINT16 c_Spare0010   = 0x0010;
    static const UINT16 c_Spare0020   = 0x0020;
    static const UINT16 c_Spare0040   = 0x0040;
    static const UINT16 c_Spare0080   = 0x0080;
    static const UINT16 c_Spare0100   = 0x0100;
    static const UINT16 c_Spare0200   = 0x0200;
    static const UINT16 c_Spare0400   = 0x0400;
    static const UINT16 c_Spare0800   = 0x0800;
    static const UINT16 c_Spare1000   = 0x1000;
    static const UINT16 c_NoCaching   = 0x2000;
    static const UINT16 c_NACK        = 0x4000;
    static const UINT16 c_ACK         = 0x8000;
};

struct WP_Packet
{
    UINT8  m_signature[ 8 ];
    UINT32 m_crcHeader;
    UINT32 m_crcData;

    UINT32 m_cmd;
    UINT16 m_seq;
    UINT16 m_seqReply;
    UINT32 m_flags;
    UINT32 m_size;
};

//--//

struct WP_PhysicalLayer
{
    //
    // TransmitMessage has to be fully buffered, in the sense it should accept all the input and return.
    // Blocking behavior has to be hidden in the driver.
    //
    bool (*ReceiveBytes   )( void* state, UINT8*& ptr, UINT32 & size );
    bool (*TransmitMessage)( void* state, const WP_Message* msg      );
};

struct WP_ApplicationLayer
{
    bool (*ProcessHeader )( void* state,  WP_Message* msg );
    bool (*ProcessPayload)( void* state,  WP_Message* msg );
    bool (*Release       )( void* state,  WP_Message* msg );
};

//--//

struct WP_Message
{
    struct ReceiveState
    {
        static const int Idle             = 0;
        static const int Initialize       = 1;
        static const int WaitingForHeader = 2;
        static const int ReadingHeader    = 3;
        static const int CompleteHeader   = 4;
        static const int ReadingPayload   = 5;
        static const int CompletePayload  = 6;
    };

    static const UINT32 c_PayloadTimeout = 60000000; // 6 secs (100 nsecs units)

    //--//

    WP_Controller* m_parent;

    WP_Packet      m_header;
    UINT8*         m_payload;

    //--//

private:
    UINT8*         m_pos;
    UINT32         m_size;
    UINT64         m_payloadTicks;
    int            m_rxState;
    //--//

public:
    void Initialize( WP_Controller* parent );

    //--//

    void PrepareReception(                                                                        );
    void PrepareRequest  ( UINT32           cmd, UINT32 flags, UINT32 payloadSize, UINT8* payload );
    void PrepareReply    ( const WP_Packet& req, UINT32 flags, UINT32 payloadSize, UINT8* payload );
    void SetPayload      (                                                         UINT8* payload );
    void Release         (                                                                        );
#if defined(NETMF_TARGET_BIG_ENDIAN)
    void SwapEndian      (                                                                        );
#endif

    bool Process();

    //--//

private:
    bool VerifyHeader ();
    bool VerifyPayload();

    void ReplyBadPacket( UINT32 flags );
};

struct WP_Controller
{
    LPCSTR                     m_szMarker;
    const WP_PhysicalLayer*    m_phy;
    const WP_ApplicationLayer* m_app;
    void*                      m_state;

    WP_Message                 m_inboundMessage;
    UINT16                     m_lastOutboundMessage;

    //--//

    void Initialize( LPCSTR szMarker, const WP_PhysicalLayer* phy, const WP_ApplicationLayer* app, void* state );

    //--//

    bool AdvanceState();

    bool SendProtocolMessage( const WP_Message& msg );

    bool SendProtocolMessage( UINT32 cmd, UINT32 flags = 0, UINT32 payloadSize = 0, UINT8* payload = NULL );
};

//--//

//
// This structure is never used, its purpose is to generate a compiler error in case the size of any structure changes.
//
struct WP_CompileCheck
{
    char buf1[ sizeof(WP_Packet) == 8 * 4 ? 1 : -1 ];
};

//--//

#if defined(_MSC_VER)
#pragma pack(pop, SUPPORT_WIREPROTOCOL_H_)
#endif

#endif // _SUPPORT_WIREPROTOCOL_H_
