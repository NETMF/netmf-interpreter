////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

////////////////////////////////////////////////////////////////////////////////////////////////////
#define TRACE_ERRORS 1
#define TRACE_HEADERS 2
#define TRACE_STATE 4
#define TRACE_NODATA 8

#define TRACE_MASK (TRACE_ERRORS)

#if TRACE_MASK != 0
#define TRACE0( f, msg ) if((f) & TRACE_MASK ) debug_printf( msg ) 
#define TRACE( f, msg, ...) if((f) & TRACE_MASK ) debug_printf( msg, __VA_ARGS__ ) 
#else
#define TRACE0( f, msg,...)
#define TRACE( f, msg,...)
#endif

void WP_Message::Initialize( WP_Controller* parent )
{
    m_parent  = parent;                          
                                                 
    memset( &m_header, 0, sizeof(m_header) );    
    m_payload = NULL;                            
                                                 
                                                 
                                                 
    m_pos     = NULL;                            
    m_size    = 0;                               
    m_payloadTicks = 0;                         
    m_rxState = ReceiveState::Idle;             
}

void WP_Message::PrepareReception()
{
    m_rxState = ReceiveState::Initialize;
}

void WP_Message::PrepareRequest( UINT32 cmd, UINT32 flags, UINT32 payloadSize, UINT8* payload )
{
    memcpy( m_header.m_signature, m_parent->m_szMarker ? m_parent->m_szMarker : MARKER_PACKET_V1, sizeof(m_header.m_signature) );

    m_header.m_crcData   = SUPPORT_ComputeCRC( payload, payloadSize, 0 );

    m_header.m_cmd       = cmd;
    m_header.m_seq       = m_parent->m_lastOutboundMessage++;
    m_header.m_seqReply  = 0;
    m_header.m_flags     = flags;
    m_header.m_size      = payloadSize;
    m_payload            = payload;

    //
    // The CRC for the header is computed setting the CRC field to zero and then running the CRC algorithm.
    //
    m_header.m_crcHeader = 0;
#if defined(BIG_ENDIAN)
    SwapEndian();
#endif
    m_header.m_crcHeader = SUPPORT_ComputeCRC( (UINT8*)&m_header, sizeof(m_header), 0 );
#if defined(BIG_ENDIAN)
    m_header.m_crcHeader = ::SwapEndian( m_header.m_crcHeader );
#endif

}

void WP_Message::PrepareReply( const WP_Packet& req, UINT32 flags, UINT32 payloadSize, UINT8* payload )
{
    memcpy( m_header.m_signature, m_parent->m_szMarker ? m_parent->m_szMarker : MARKER_PACKET_V1, sizeof(m_header.m_signature) );

    m_header.m_crcData   = SUPPORT_ComputeCRC( payload, payloadSize, 0 );

    m_header.m_cmd       = req.m_cmd;
    m_header.m_seq       = m_parent->m_lastOutboundMessage++;
    m_header.m_seqReply  = req.m_seq;
    m_header.m_flags     = flags | WP_Flags::c_Reply;
    m_header.m_size      = payloadSize;
    m_payload            = payload;

    //
    // The CRC for the header is computed setting the CRC field to zero and then running the CRC algorithm.
    //
    m_header.m_crcHeader = 0;
#if defined(BIG_ENDIAN)
    SwapEndian();
#endif
    m_header.m_crcHeader = SUPPORT_ComputeCRC( (UINT8*)&m_header, sizeof(m_header), 0 );
#if defined(BIG_ENDIAN)
    m_header.m_crcHeader = ::SwapEndian( m_header.m_crcHeader );
#endif
}

void WP_Message::SetPayload( UINT8* payload )
{
    m_payload = payload;
}

void WP_Message::Release()
{
    if(m_payload)
    {
        m_parent->m_app->Release( m_parent->m_state, this );

        m_payload = NULL;
    }
}


bool WP_Message::VerifyHeader()
{    
    bool   fRes;
    
#if !defined(BIG_ENDIAN)
    UINT32 crc = m_header.m_crcHeader;
#else
    UINT32 crc = ::SwapEndian( m_header.m_crcHeader );
#endif
    m_header.m_crcHeader = 0;
    UINT32 computedCrc = SUPPORT_ComputeCRC( ( UINT8* )&m_header, sizeof( m_header ), 0 );
    m_header.m_crcHeader = crc;
    fRes = computedCrc == crc;
    if( !fRes )
        TRACE( TRACE_ERRORS, "Header CRC check failed: computed: 0x%08X; got: 0x%08X\n", computedCrc, m_header.m_crcHeader );

    return fRes;
}

bool WP_Message::VerifyPayload()
{
    if( m_payload == NULL && m_header.m_size )
        return false;

    UINT32 computedCrc = SUPPORT_ComputeCRC( m_payload, m_header.m_size, 0 );
    bool fRes = ( computedCrc == m_header.m_crcData );
    if( !fRes )
        TRACE( TRACE_ERRORS, "Payload CRC check failed: computed: 0x%08X; got: 0x%08X\n", computedCrc, m_header.m_crcData );

    return fRes;
}

void WP_Message::ReplyBadPacket( UINT32 flags )
{
    WP_Message msg;

    msg.Initialize( m_parent );

    msg.PrepareRequest( 0, WP_Flags::c_NonCritical | WP_Flags::c_NACK | flags, 0, NULL );
    m_parent->SendProtocolMessage( msg );
}

bool WP_Message::Process()
{
    UINT8* buf = (UINT8*)&m_header;
    int    len;

    while(true)
    {
        switch(m_rxState)
        {
        case ReceiveState::Idle:
            TRACE0( TRACE_STATE, "RxState==IDLE\n");
            return true;

        case ReceiveState::Initialize:
            TRACE0( TRACE_STATE, "RxState==INIT\n");
            Release();

            m_rxState = ReceiveState::WaitingForHeader;
            m_pos     = (UINT8*)&m_header;
            m_size    = sizeof(m_header);
            break;

        case ReceiveState::WaitingForHeader:
            TRACE0( TRACE_STATE, "RxState==WaitForHeader\n");
            if(m_parent->m_phy->ReceiveBytes( m_parent->m_state, m_pos, m_size ) == false)
            {
                TRACE0( TRACE_NODATA, "ReceiveBytes returned false - bailing out\n");
                return true;
            }

            //
            // Synch to the start of a message.
            //
            while(true)
            {
                len = sizeof(m_header) - m_size; if(len <= 0) break;

                size_t lenCmp = __min( len, sizeof(m_header.m_signature) );

                if(memcmp( &m_header, MARKER_DEBUGGER_V1, lenCmp ) == 0) break;
                if(memcmp( &m_header, MARKER_PACKET_V1  , lenCmp ) == 0) break;

                memmove( &buf[ 0 ], &buf[ 1 ], len-1 );

                m_pos--;
                m_size++;
            }

            if(len >= sizeof(m_header.m_signature))
            {
                m_rxState = ReceiveState::ReadingHeader;
            }
            break;

        case ReceiveState::ReadingHeader:
            TRACE0( TRACE_STATE, "RxState==ReadingHeader\n");
            if(m_parent->m_phy->ReceiveBytes( m_parent->m_state, m_pos, m_size ) == false)
            {
                TRACE0( TRACE_NODATA, "ReceiveBytes returned false - bailing out\n");
                return true;
            }

            if(m_size == 0)
            {
               m_rxState = ReceiveState::CompleteHeader;
            }
            break;

        case ReceiveState::CompleteHeader:
            {
                TRACE0( TRACE_STATE, "RxState=CompleteHeader\n");

                bool fBadPacket=true;
                if( VerifyHeader() )
                {
#if defined(BIG_ENDIAN)
                    SwapEndian();
#endif
                    TRACE( TRACE_HEADERS, "RXMSG: 0x%08X, 0x%08X, 0x%08X\n", m_header.m_cmd, m_header.m_flags, m_header.m_size );
                    if ( m_parent->m_app->ProcessHeader( m_parent->m_state, this ) )
                    {
                        fBadPacket = false;
                        if(m_header.m_size)
                        {
                            if(m_payload == NULL) // Bad, no buffer...
                            {
                                m_rxState = ReceiveState::Initialize;
                            }
                            else
                            {
                                m_payloadTicks = HAL_Time_CurrentTicks();
                                m_rxState = ReceiveState::ReadingPayload;
                                m_pos     = (UINT8*)m_payload;
                                m_size    = m_header.m_size;
                            }
                        }
                        else
                        {
                            m_rxState = ReceiveState::CompletePayload;
                        }
                    }
                }
                
                if ( fBadPacket )
                {
                    if((m_header.m_flags & WP_Flags::c_NonCritical) == 0)
                    {
                        ReplyBadPacket( WP_Flags::c_BadHeader );
                    }

                    m_rxState = ReceiveState::Initialize;
                }
            }
            break;

        case ReceiveState::ReadingPayload:
            {
                TRACE0( TRACE_STATE, "RxState=ReadingPayload\n");
                
                UINT64 curTicks = HAL_Time_CurrentTicks();

                // If the time between consecutive payload bytes exceeds the timeout threshold then assume that
                // the rest of the payload is not coming. Reinitialize to synch on the next header. 

                if(HAL_Time_TicksToTime( curTicks - m_payloadTicks ) < (UINT64)c_PayloadTimeout)
                {
                    m_payloadTicks = curTicks;
                
                    if(m_parent->m_phy->ReceiveBytes( m_parent->m_state, m_pos, m_size ) == false)
                    {
                        TRACE0( TRACE_NODATA, "ReceiveBytes returned false - bailing out\n");
                        return true;
                    }

                    if(m_size == 0)
                    {
                        m_rxState = ReceiveState::CompletePayload;
                    }
                }
                else
                {
                    TRACE0( TRACE_ERRORS, "RxError: Payload InterCharacterTimeout exceeded\n");
                    m_rxState = ReceiveState::Initialize;
                }
            }
            break;

        case ReceiveState::CompletePayload:
            TRACE0( TRACE_STATE, "RxState=CompletePayload\n");
            if(VerifyPayload() == true)
            {
                m_parent->m_app->ProcessPayload( m_parent->m_state, this );
            }
            else
            {
                ReplyBadPacket( WP_Flags::c_BadPayload );
            }

            m_rxState = ReceiveState::Initialize;
            break;


        default:
            TRACE0( TRACE_ERRORS, "RxState=UNKNOWN!!\n");
            return false;
        }
    }
}

#if defined (BIG_ENDIAN)
void WP_Message::SwapEndian()
{
    m_header.m_crcHeader = ::SwapEndian( m_header.m_crcHeader     );
    m_header.m_crcData   = ::SwapEndian( m_header.m_crcData       );

    m_header.m_cmd       = ::SwapEndian( m_header.m_cmd           );
    m_header.m_seq       = ::SwapEndian( m_header.m_seq           );
    m_header.m_seqReply  = ::SwapEndian( m_header.m_seqReply      );
    m_header.m_flags     = ::SwapEndian( m_header.m_flags         );
    m_header.m_size      = ::SwapEndian( m_header.m_size          );
}
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

void WP_Controller::Initialize( LPCSTR szMarker, const WP_PhysicalLayer* phy, const WP_ApplicationLayer* app, void* state )
{
    m_szMarker            = szMarker; 
    m_phy                 = phy;      
    m_app                 = app;      
    m_state               = state;    
                                                              
                                      
    m_lastOutboundMessage = 0;        

    memset( &m_inboundMessage, 0, sizeof(m_inboundMessage) );
    m_inboundMessage.Initialize( this );

    m_inboundMessage.PrepareReception();
}

bool WP_Controller::AdvanceState()
{
    return m_inboundMessage.Process();
}

bool WP_Controller::SendProtocolMessage( const WP_Message& msg )
{
    TRACE( TRACE_HEADERS, "TXMSG: 0x%08X, 0x%08X, 0x%08X\n", msg.m_header.m_cmd, msg.m_header.m_flags, msg.m_header.m_size );
    return m_phy->TransmitMessage( m_state, &msg );
}

bool WP_Controller::SendProtocolMessage( UINT32 cmd, UINT32 flags, UINT32 payloadSize, UINT8* payload )
{
    WP_Message msg;

    msg.Initialize( this );

    msg.PrepareRequest( cmd, flags, payloadSize, payload );

    return SendProtocolMessage( msg );
}
