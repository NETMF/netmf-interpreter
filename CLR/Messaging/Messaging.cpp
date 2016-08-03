////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\core\Core.h"
#include <TinyCLR_Messaging.h>
#include <TinyCLR_Endian.h>

#include <crypto.h>

////////////////////////////////////////////////////////////////////////////////////////////////////
#if 0
#define TRACE0( msg, ...) debug_printf( msg ) 
#define TRACE( msg, ...) debug_printf( msg, __VA_ARGS__ ) 
#else
#define TRACE0(msg,...)
#define TRACE(msg,...)
#endif

// the Arm 3.0 compiler drags in a bunch of ABI methods (for initialization) if struct arrays are not initialized
CLR_UINT32     g_scratchMessaging[ sizeof(CLR_Messaging) * NUM_MESSAGING / sizeof(UINT32) + 1 ];
CLR_Messaging *g_CLR_Messaging;

////////////////////////////////////////////////////////////////////////////////////////////////////

//--//

static const CLR_Messaging_CommandHandlerLookup c_Messaging_Lookup_Request[] =
{
    {  CLR_Messaging::Messaging_Query, CLR_Messaging_Commands::c_Messaging_Query },
    {  CLR_Messaging::Messaging_Send,  CLR_Messaging_Commands::c_Messaging_Send  },
    {  CLR_Messaging::Messaging_Reply, CLR_Messaging_Commands::c_Messaging_Reply },
};

static const CLR_Messaging_CommandHandlerLookup c_Messaging_Lookup_Reply[] =
{
    {  CLR_Messaging::Messaging_Query, CLR_Messaging_Commands::c_Messaging_Query },
    {  CLR_Messaging::Messaging_Send,  CLR_Messaging_Commands::c_Messaging_Send  },
    {  CLR_Messaging::Messaging_Reply, CLR_Messaging_Commands::c_Messaging_Reply },
};


//--//

bool CLR_Messaging::AllocateAndQueueMessage( CLR_UINT32 cmd, UINT32 length, UINT8* data, CLR_RT_HeapBlock_EndPoint::Port port, CLR_RT_HeapBlock_EndPoint::Address addr, CLR_UINT32 found )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_RT_HeapBlock_EndPoint::Message* rpc;
    CLR_RT_HeapBlock_EndPoint*          ep;
    
    if((ep = CLR_RT_HeapBlock_EndPoint::FindEndPoint( port )) == NULL) return false;

    {
        CLR_RT_ProtectFromGC gc( *ep );

        if((rpc = (CLR_RT_HeapBlock_EndPoint::Message*)CLR_RT_Memory::Allocate( sizeof(*rpc) + length, CLR_RT_HeapBlock::HB_CompactOnFailure )) == NULL) return false;
    
        rpc->ClearData();
        
        rpc->m_cmd       = cmd;;
        rpc->m_addr      = addr;
        rpc->m_length    = length;
        rpc->m_found     = found;
        
        if(data) memcpy( rpc->m_data, data, length );

        ep->m_messages.LinkAtBack( rpc );

        g_CLR_RT_ExecutionEngine.SignalEvents( CLR_RT_ExecutionEngine::c_Event_EndPoint );
    }

    return true;
}


bool CLR_Messaging::Messaging_Query( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_Messaging*                           messaging = (CLR_Messaging*)owner;
    CLR_Messaging_Commands::Messaging_Query*       cmd = (CLR_Messaging_Commands::Messaging_Query*)msg->m_payload;
    CLR_Messaging_Commands::Messaging_Query::Reply res;
    CLR_RT_HeapBlock_EndPoint*                      ep = CLR_RT_HeapBlock_EndPoint::FindEndPoint( cmd->m_addr.m_to );

    res.m_found = (ep != NULL);
    res.m_addr  = cmd->m_addr;

    messaging->ReplyToCommand( msg, true, false, &res, sizeof(res) );

    return true;
}

bool CLR_Messaging::Messaging_Query__Reply( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_Messaging *messaging = (CLR_Messaging*)owner;
    
    CLR_Messaging_Commands::Messaging_Query::Reply* cmd = (CLR_Messaging_Commands::Messaging_Query::Reply*)msg->m_payload;
    
    messaging->AllocateAndQueueMessage( CLR_Messaging_Commands::c_Messaging_Query, 0, NULL, cmd->m_addr.m_from, cmd->m_addr, cmd->m_found );
        
    return true;
}

//--//

bool CLR_Messaging::Messaging_Send( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_Messaging*                           messaging = (CLR_Messaging*)owner;
    CLR_Messaging_Commands::Messaging_Send*       cmd  = (CLR_Messaging_Commands::Messaging_Send*)msg->m_payload;
    CLR_Messaging_Commands::Messaging_Send::Reply res;
    CLR_UINT32                                    len;
    bool                                          fRes;

    len = msg->m_header.m_size - sizeof(cmd->m_addr);
        
    fRes = messaging->AllocateAndQueueMessage( CLR_Messaging_Commands::c_Messaging_Send, len, cmd->m_data, cmd->m_addr.m_to, cmd->m_addr, false );

    res.m_found = true;
    res.m_addr  = cmd->m_addr;

    messaging->ReplyToCommand( msg, fRes, false, &res, sizeof(res) );

    return true;
}

bool CLR_Messaging::Messaging_Send__Reply( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    //
    // retransmission support is currently not implemented
    //

    return true;
}

//--//

bool CLR_Messaging::Messaging_Reply( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_Messaging*                            messaging = (CLR_Messaging*)owner;
    CLR_Messaging_Commands::Messaging_Reply*       cmd  = (CLR_Messaging_Commands::Messaging_Reply*)msg->m_payload;
    CLR_Messaging_Commands::Messaging_Reply::Reply res;
    bool                                           fRes;
    CLR_UINT32                                     len;

    len = msg->m_header.m_size - sizeof(cmd->m_addr);
    fRes = messaging->AllocateAndQueueMessage( CLR_Messaging_Commands::c_Messaging_Reply, len, cmd->m_data, cmd->m_addr.m_from, cmd->m_addr, false );

    res.m_found = true;
    res.m_addr  = cmd->m_addr;

    messaging->ReplyToCommand( msg, fRes, false, &res, sizeof(res) );

    return true;
}

bool CLR_Messaging::Messaging_Reply__Reply( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    //
    // retransmission support  is currently not implemented
    //

    return true;
}


////////////////////////////////////////////////////////////////////////////////////////////////////


bool CLR_Messaging::Phy_ReceiveBytes( void* state, UINT8*& ptr, UINT32 & size )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_Messaging* messaging = (CLR_Messaging*)state;

    if(size)
    {
        int read = DebuggerPort_Read( messaging->m_port, (char*)ptr, size ); if(read <= 0) return false;

        ptr  += read;
        size -= read;
    }

    return true;
}

bool CLR_Messaging::Phy_TransmitMessage( void* state, const WP_Message* msg )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_Messaging* messaging = (CLR_Messaging*)state;

    return messaging->TransmitMessage( msg, true );
}

//--//

bool CLR_Messaging::App_ProcessHeader( void* state, WP_Message*  msg )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_Messaging* pThis = (CLR_Messaging*)state;

    ::Watchdog_ResetCounter();

    if( !pThis->ProcessHeader( msg ) )
    {
        TRACE0("ProcessHeader() indicated invalid header!\n");
        return false;
    }

    if(msg->m_header.m_size)
    {
        void* ptr = CLR_RT_Memory::Allocate( msg->m_header.m_size, CLR_RT_HeapBlock::HB_CompactOnFailure );

        if(ptr == NULL)
        {
            TRACE0("Failed to allocate 0x%08X bytes for message payload!\n");
            return false;
        }

        msg->m_payload = (UINT8*)ptr;
    }

    return true;
}

bool CLR_Messaging::App_ProcessPayload( void* state, WP_Message* msg )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_Messaging* pThis = (CLR_Messaging*)state;

    ::Watchdog_ResetCounter();

    if(pThis->ProcessPayload( msg ) == false)
    {
        return false;
    }

    return true;
}

bool CLR_Messaging::App_Release( void* state, WP_Message* msg )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    if(msg->m_payload != NULL)
    {
        CLR_RT_Memory::Release( msg->m_payload );

        msg->m_payload = NULL;
    }

    return true;
}

//--//

const WP_PhysicalLayer c_Messaging_phy =
{
    &CLR_Messaging::Phy_ReceiveBytes   , 
    &CLR_Messaging::Phy_TransmitMessage, 
};

const WP_ApplicationLayer c_Messaging_app =
{
    &CLR_Messaging::App_ProcessHeader ,
    &CLR_Messaging::App_ProcessPayload,
    &CLR_Messaging::App_Release       , 
};

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_Messaging::CreateInstance()
{
    NATIVE_PROFILE_CLR_MESSAGING();
    TINYCLR_HEADER();

    g_CLR_Messaging = (CLR_Messaging*)&g_scratchMessaging[ 0 ];

    CLR_RT_Memory::ZeroFill( g_CLR_Messaging, sizeof(CLR_Messaging) * NUM_MESSAGING );

    int iMsg = 0;
    
    TINYCLR_FOREACH_MESSAGING_NO_TEMP()
    {
        g_CLR_Messaging[ iMsg ].Initialize(
            HalSystemConfig.MessagingPorts[ iMsg ], 
            NULL, 
            0, 
            NULL, 
            0, 
            &g_CLR_Messaging[ iMsg ]
            );
        iMsg++;
    }
    TINYCLR_FOREACH_MESSAGING_END();

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

void CLR_Messaging::Initialize(
    COM_HANDLE                                port, 
    const CLR_Messaging_CommandHandlerLookup* requestLookup, 
    const CLR_UINT32                          requestLookupCount, 
    const CLR_Messaging_CommandHandlerLookup* replyLookup, 
    const CLR_UINT32                          replyLookupCount, 
    void*                                     owner 
    )
{
    // If the debugger and Messaging share the same port (Legacy) then we will not initialze the Messaging port (because the debugger will take care of it)
    if((port == HalSystemConfig.MessagingPorts[ 0 ]) && 
       (port == HalSystemConfig.DebuggerPorts[ 0 ] ) &&
        requestLookup == NULL) // messaging is null so don't initialize the port
    {
        return;
    }

    if(m_fInitialized) return;

    m_Lookup_Requests[ 0 ].table = c_Messaging_Lookup_Request;
    m_Lookup_Requests[ 0 ].owner = this;
    m_Lookup_Requests[ 0 ].size  = ARRAYSIZE(c_Messaging_Lookup_Request);

    m_Lookup_Replies[ 0 ].table = c_Messaging_Lookup_Reply;
    m_Lookup_Replies[ 0 ].owner = this;
    m_Lookup_Replies[ 0 ].size  = ARRAYSIZE(c_Messaging_Lookup_Reply);

    m_cacheSubordinate.DblLinkedList_Initialize();
    m_cacheMaster     .DblLinkedList_Initialize();

    m_controller.Initialize( MARKER_DEBUGGER_V1, &c_Messaging_phy, &c_Messaging_app, this );

    m_cacheTotalSize = 0;
    m_port           = port;    

    m_Lookup_Requests[ 1 ].table = requestLookup;
    m_Lookup_Requests[ 1 ].owner = owner;
    m_Lookup_Requests[ 1 ].size  = requestLookupCount;

    m_Lookup_Replies[ 1 ].table = replyLookup;
    m_Lookup_Replies[ 1 ].owner = owner;
    m_Lookup_Replies[ 1 ].size  = replyLookupCount;

    m_fDebuggerInitialized = (DebuggerPort_Initialize( port ) != FALSE);

    m_fInitialized = true;

}

HRESULT CLR_Messaging::DeleteInstance()
{
    NATIVE_PROFILE_CLR_MESSAGING();
    TINYCLR_HEADER();

    TINYCLR_FOREACH_MESSAGING(msg)
    {
        msg.Cleanup();
    }
    TINYCLR_FOREACH_MESSAGING_END();

    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_Messaging::Cleanup()
{
    NATIVE_PROFILE_CLR_MESSAGING();

    if(!m_fInitialized) return;

    // Some devices cannot reset the USB controller so we need to allow them to skip uninitialization
    // of the debug transport
    if(!g_fDoNotUninitializeDebuggerPort)
    {
        DebuggerPort_Uninitialize( m_port );
    }
    
    m_cacheSubordinate.DblLinkedList_Release();
    m_cacheMaster     .DblLinkedList_Release();

    m_cacheTotalSize = 0;

    m_fDebuggerInitialized = false;

    m_fInitialized = false;
}

//--//

void CLR_Messaging::ProcessCommands()
{
    NATIVE_PROFILE_CLR_MESSAGING();
    if(m_fInitialized)
    {
        m_controller.AdvanceState();
    }
}

bool CLR_Messaging::ProcessHeader( WP_Message* msg )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    TRACE("MSG: 0x%08X\n", msg->m_header.m_cmd );
    return true;
}

bool CLR_Messaging::ProcessPayload( WP_Message* msg )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    if(msg->m_header.m_flags & WP_Flags::c_NACK)
    {
        //
        // Bad packet...
        //
        return true;
    }

    //--//
#if defined(NETMF_TARGET_BIG_ENDIAN)
    SwapEndian( msg, msg->m_payload, msg->m_header.m_size, false );
#endif
    const CLR_Messaging_CommandHandlerLookups* tables;
    int tableCount = 0;

    if(msg->m_header.m_flags & WP_Flags::c_Reply)
    {
        //
        // Only process replies once!
        //
        TINYCLR_FOREACH_NODE(CachedMessage,cache,m_cacheMaster)
        {
            WP_Packet& req = cache->m_message.m_header;
            WP_Packet& res = msg->            m_header;

            if(req.m_cmd == res.m_cmd && req.m_seq == res.m_seqReply)
            {
                m_cacheMaster.LinkAtFront( cache );

                cache->m_lastSeen = Time_GetMachineTime();

                if(cache->m_flags & CachedMessage::c_Processed)
                {
                    return true;
                }

                cache->m_flags |= CachedMessage::c_Processed;
                break;
            }
        }
        TINYCLR_FOREACH_NODE_END();

        tables     = m_Lookup_Replies;
        tableCount = ARRAYSIZE(m_Lookup_Replies);
    }
    else
    {
        TINYCLR_FOREACH_NODE(CachedMessage,cache,m_cacheSubordinate)
        {
            WP_Packet& req = msg->            m_header;
            WP_Packet& res = cache->m_message.m_header;

            if(req.m_cmd == res.m_cmd && req.m_seq == res.m_seqReply)
            {
                m_cacheSubordinate.LinkAtFront( cache );

                cache->m_lastSeen = Time_GetMachineTime();

                TransmitMessage( &cache->m_message, false );
                return true;
            }
        }
        TINYCLR_FOREACH_NODE_END();

        tables     = m_Lookup_Requests;
        tableCount = ARRAYSIZE(m_Lookup_Requests);
    }

    while(tableCount-- > 0)
    {
        size_t                                    num = tables->size;
        const CLR_Messaging_CommandHandlerLookup* cmd = tables->table;

        while(num-- > 0 && cmd != NULL)
        {
            if(cmd->cmd == msg->m_header.m_cmd)
            {
                ReplyToCommand( msg, (*(cmd->hnd))( msg, tables->owner ), false );
                return true;
            }

            cmd++;
        }
        tables++;
    }

    ReplyToCommand( msg, false, false );
    
    return true;
}

//--//

void CLR_Messaging::PurgeCache()
{
    NATIVE_PROFILE_CLR_MESSAGING();
    CLR_INT64 oldest = Time_GetMachineTime() - TIME_CONVERSION__TO_SECONDS * 3;

    PurgeCache( m_cacheMaster     , oldest );
    PurgeCache( m_cacheSubordinate, oldest );
}

void CLR_Messaging::PurgeCache( CLR_RT_DblLinkedList& lst, CLR_INT64 oldest )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    TINYCLR_FOREACH_NODE_BACKWARD(CachedMessage,cache,lst)
    {
        if(cache->m_lastSeen < oldest || m_cacheTotalSize > c_MaxCacheSize)
        {
            cache->Unlink(); m_cacheTotalSize -= cache->m_size;

            CLR_RT_Memory::Release( cache );
        }
    }
    TINYCLR_FOREACH_NODE_BACKWARD_END();
}

bool CLR_Messaging::TransmitMessage( const WP_Message* msg, bool fQueue )
{
    NATIVE_PROFILE_CLR_MESSAGING();

    UINT32 payloadSize;
    UINT32 flags;

#if !defined(NETMF_TARGET_BIG_ENDIAN)
    payloadSize = msg->m_header.m_size;
    flags       = msg->m_header.m_flags;
#else
    payloadSize = ::SwapEndian( msg->m_header.m_size  );
    flags       = ::SwapEndian( msg->m_header.m_flags );
#endif

    if(DebuggerPort_Write( m_port, (char*)&msg->m_header, sizeof(msg->m_header) ) != sizeof(msg->m_header)) return false;

    if(msg->m_header.m_size && msg->m_payload)
    {
        if(DebuggerPort_Write( m_port, (char*)msg->m_payload, payloadSize ) != payloadSize) return false;
    }
    DebuggerPort_Flush( m_port );

    if(fQueue && (flags & WP_Flags::c_NoCaching) == 0)
    {
        CLR_RT_DblLinkedList* lst;

        if(flags & WP_Flags::c_Reply)
        {
            lst = &m_cacheSubordinate;
        }
        else
        {
            if(flags & WP_Flags::c_NonCritical)
            {
                //
                // Don't cache non-critical requests.
                //
                lst = NULL;
            }
            else
            {
                lst = &m_cacheMaster;
            }
        }

        if(lst)
        {
            CLR_UINT32 len = sizeof(CachedMessage) + payloadSize;

            CachedMessage* cache = (CachedMessage*)CLR_RT_Memory::Allocate_And_Erase( len, CLR_RT_HeapBlock::HB_CompactOnFailure );
            if(cache)
            {
                m_cacheTotalSize  += len;

                cache->m_size      = len;
                cache->m_lastSeen  = Time_GetMachineTime();
                cache->m_message   = *msg;

                if(payloadSize && msg->m_payload)
                {
                    cache->m_message.m_payload = (UINT8*)&cache[ 1 ];

                    memcpy( cache->m_message.m_payload, msg->m_payload, payloadSize );
                }
                else
                {
                    cache->m_message.m_header.m_size = 0;
                    cache->m_message.m_payload       = NULL;
                }

                lst->LinkAtFront( cache );
            }
        }
    }

    return true;
}

//--//

bool CLR_Messaging::SendEvent( UINT32 cmd, UINT32 payloadSize, UINT8* payload, UINT32 flags )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    return m_controller.SendProtocolMessage( cmd, flags, payloadSize, payload );
}

void CLR_Messaging::BroadcastEvent( UINT32 cmd, UINT32 payloadSize, UINT8* payload, UINT32 flags )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    TINYCLR_FOREACH_MESSAGING(msg)
    {
        msg.m_controller.SendProtocolMessage( cmd, flags, payloadSize, payload );        
    }
    TINYCLR_FOREACH_MESSAGING_END();
}

//--//

void CLR_Messaging::ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical, void* ptr, int size )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    WP_Message msgReply;
    UINT32     flags = 0;

    //
    // Make sure we reply only once!
    //
    if(msg->m_header.m_flags & WP_Flags::c_NonCritical) return;
    msg->m_header.m_flags |= WP_Flags::c_NonCritical;

    //
    // No caching in the request, no caching in the reply...
    //
    if(msg->m_header.m_flags & WP_Flags::c_NoCaching) flags |= WP_Flags::c_NoCaching;

    if(fSuccess  ) flags |= WP_Flags::c_ACK;
    else           flags |= WP_Flags::c_NACK;
    if(!fCritical) flags |= WP_Flags::c_NonCritical;

    if(fSuccess == false)
    {
        ptr  = NULL;
        size = 0;
    }


    msgReply.Initialize( &m_controller );
#if defined(NETMF_TARGET_BIG_ENDIAN)
    SwapEndian( msg, ptr, size, true );
#endif

    msgReply.PrepareReply( msg->m_header, flags, size, (UINT8*)ptr );

    m_controller.SendProtocolMessage( msgReply );
}

void CLR_Messaging::ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    ReplyToCommand( msg, fSuccess, fCritical, NULL, 0 );
}

//--//
#if defined(NETMF_TARGET_BIG_ENDIAN)
UINT32 CLR_Messaging::SwapEndianPattern( UINT8* &buffer, UINT32 size, UINT32 count )
{
    UINT32 consumed=0;
    
    if (1==size)
    {
        // Do no swapping, just increment pointer
        buffer += (size*count);
        consumed = size*count;
    }
    else
    {
        while (count--)
        {
            switch ( size )
            {
                case 1      :
                    
                    break;
                case 2      :
                    {
                        UINT16 *p16 = (UINT16 *)buffer;
                        *p16 = ::SwapEndian( *p16 );
                    }
                    break;
                case 4      :
                    {
                        UINT32 *p32 = (UINT32 *)buffer;
                        *p32 = ::SwapEndian( *p32 );
                    }
                    break;
                case 8      :
                    {
                        UINT64 *p64 = (UINT64 *)buffer;
                        *p64 = ::SwapEndian( *p64 );
                    }
                    break;
            }
            buffer += size;
            consumed += size;
        }
    }
    return consumed;
}
void CLR_Messaging::SwapDebuggingValue( UINT8* &payload, UINT32 payloadSize )
{
    UINT32 count = payloadSize / sizeof(CLR_DBG_Commands::Debugging_Value);
    while (count--)
    {
        SwapEndianPattern( payload, sizeof(UINT32), 4);
        SwapEndianPattern( payload, 1, 128 );
        SwapEndianPattern( payload, sizeof(UINT32), 2);
        SwapEndianPattern( payload, sizeof(UINT32), 1);
        SwapEndianPattern( payload, sizeof(UINT32), 2);
        SwapEndianPattern( payload, sizeof(UINT32), 1);
        SwapEndianPattern( payload, sizeof(UINT32), 2);
    }
}

void CLR_Messaging::SwapEndian( WP_Message* msg, void* ptr, int size, bool fReply )
{

    UINT8 *payload      = (UINT8*)ptr;
    UINT32 payloadSize  = size ;

    ASSERT(sizeof(int)==sizeof(UINT32));

    // Some commands may have a zero payload if not supproted, protect here.
    if (NULL==ptr) return;

    switch ( msg->m_header.m_cmd )
    {
    case CLR_DBG_Commands::c_Monitor_Ping               :
        SwapEndianPattern( payload, sizeof(UINT32), 2  );
        break;

    case CLR_DBG_Commands::c_Monitor_Message            :
        // string (no NULL termination) - nothing to do
        break;
    case CLR_DBG_Commands::c_Monitor_ReadMemory         :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2 );
        break;
    case CLR_DBG_Commands::c_Monitor_WriteMemory        :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2 );
        break;
    case CLR_DBG_Commands::c_Monitor_CheckMemory        :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:2  );
        break;
    case CLR_DBG_Commands::c_Monitor_EraseMemory        :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2  );
        break;
    case CLR_DBG_Commands::c_Monitor_Execute            :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1  );
        break;
    case CLR_DBG_Commands::c_Monitor_Reboot             :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1 );
        break;
    case CLR_DBG_Commands::c_Monitor_MemoryMap          :
        // do for each range entry
        SwapEndianPattern( payload, sizeof(UINT32), payloadSize/sizeof(UINT32) );
        break;
    case CLR_DBG_Commands::c_Monitor_ProgramExit        :
        // no payload
        break;
    case CLR_DBG_Commands::c_Monitor_CheckSignature     :
        // Monitor_Signature struct
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2 );
        break;
    case CLR_DBG_Commands::c_Monitor_DeploymentMap      :
        if (fReply)
        {
            SwapEndianPattern( payload, sizeof(UINT32), 3 );
        }
        break;
    case CLR_DBG_Commands::c_Monitor_FlashSectorMap     :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?payloadSize/sizeof(UINT32):0 );
        break;
    case CLR_DBG_Commands::c_Monitor_SignatureKeyUpdate :
        if (!fReply)
        {
            SwapEndianPattern( payload, sizeof(UINT32), 1 );
            SwapEndianPattern( payload, 1, 128 );
            SwapEndianPattern( payload, 1, 260 );
            SwapEndianPattern( payload, sizeof(UINT32), 1 );
        }
        break;
    case CLR_DBG_Commands::c_Monitor_OemInfo            :
        // swap the version, leave the rest
        SwapEndianPattern( payload, sizeof(UINT16), fReply?4:0 );
        break;

    case CLR_DBG_Commands::c_Debugging_Execution_BasePtr              :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:0 );
        break;
    case CLR_DBG_Commands::c_Debugging_Execution_ChangeConditions     :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:2 );
        break;
    case CLR_DBG_Commands::c_Debugging_Execution_SecurityKey          :
        // NOP
        break;
    case CLR_DBG_Commands::c_Debugging_Execution_Unlock               :
        // NOP
        break;
    case CLR_DBG_Commands::c_Debugging_Execution_Allocate             :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:1 );
        break;

    case CLR_DBG_Commands::c_Debugging_UpgradeToSsl:
        SwapEndianPattern( payload, sizeof(UINT32), 1 );
        break;

    case CLR_DBG_Commands::c_Debugging_MFUpdate_Start:
        if(!fReply)
        {
            SwapEndianPattern( payload,              1, 64 );
            SwapEndianPattern( payload, sizeof(UINT32),  5 );
            SwapEndianPattern( payload, sizeof(UINT16),  2 );
        }
        else
        {
            SwapEndianPattern( payload, sizeof(UINT32), 1 );
        }
        break;
    case CLR_DBG_Commands::c_Debugging_MFUpdate_AuthCommand:
        SwapEndianPattern( payload, sizeof(UINT32), fReply ? 2 : 3 );
        break;
    case CLR_DBG_Commands::c_Debugging_MFUpdate_Authenticate:
        SwapEndianPattern( payload, sizeof(UINT32), fReply ? 1 : 2 );
        break;
    case CLR_DBG_Commands::c_Debugging_MFUpdate_AddPacket:
        SwapEndianPattern( payload, sizeof(UINT32), fReply?4:1 );
        break;
    case CLR_DBG_Commands::c_Debugging_MFUpdate_GetMissingPkts:
        SwapEndianPattern( payload, sizeof(UINT32), fReply ? 2 : 1 );
        break;
    case CLR_DBG_Commands::c_Debugging_MFUpdate_Install:
        SwapEndianPattern( payload, sizeof(UINT32), fReply?2:1 );
        break;
        
    case CLR_DBG_Commands::c_Debugging_Execution_Breakpoints          :
        if (!fReply)
        {
            payloadSize -= SwapEndianPattern( payload, sizeof(UINT32), 1 );
            while (payloadSize)
            {
                payloadSize -= SwapEndianPattern( payload, sizeof(UINT16), 2 );
                payloadSize -= SwapEndianPattern( payload, sizeof(UINT32), 8 );
            }
        }
        break;
    case CLR_DBG_Commands::c_Debugging_Execution_BreakpointHit        :
        if (!fReply)
        {
            SwapEndianPattern( payload, sizeof(UINT16), 2 );
            SwapEndianPattern( payload, sizeof(UINT32), 8 );
        }
        break;
    case CLR_DBG_Commands::c_Debugging_Execution_BreakpointStatus     :
        if (fReply)
        {
            SwapEndianPattern( payload, sizeof(UINT16), 2 );
            SwapEndianPattern( payload, sizeof(UINT32), 8 );
        }
        break;
    case CLR_DBG_Commands::c_Debugging_Execution_QueryCLRCapabilities :
        {
            if (!fReply)
            {
                SwapEndianPattern( payload, sizeof(UINT32), 1 );
            }
            else
            {
                CLR_UINT32 cmd = ((CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities*)msg->m_payload)->m_cmd;
                // Swap the union according to the cmd
                if (CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags==cmd)             {   SwapEndianPattern( payload, sizeof(UINT32), 1 );                }
                if (CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityLCD==cmd)               {   SwapEndianPattern( payload, sizeof(UINT32), 3 );                }
                if (CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_ClrInfo==cmd)                     
                {   
                    SwapEndianPattern( payload, sizeof(UINT16), 4       ); 
                    SwapEndianPattern( payload, 1, 64-sizeof(MFVersion)   );
                    SwapEndianPattern( payload, sizeof(UINT16), 4       );
                }
                if (CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_SolutionReleaseInfo==cmd)         
                {   
                    SwapEndianPattern( payload, sizeof(UINT16), 4 );                
                }
                if (CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityVersion==cmd)           
                {
                    SwapEndianPattern( payload, 1, 20                   ); 
                    SwapEndianPattern( payload, sizeof(UINT32), 1       ); 
                }                
                if (CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_HalSystemInfo==cmd)               
                {   
                    SwapEndianPattern( payload, sizeof(UINT16), 4       ); 
                    SwapEndianPattern( payload, 1, 64-sizeof(MFVersion)   );
                    SwapEndianPattern( payload, 1, 2                    );
                    SwapEndianPattern( payload, sizeof(UINT16), 1       );
                }
            }
        }
        break;
#if 0
    case CLR_DBG_Commands::c_Debugging_Messaging_Query      :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?6:5      );
        break;
    case CLR_DBG_Commands::c_Debugging_Messaging_Send       :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?6:5      );
        break;
    case CLR_DBG_Commands::c_Debugging_Messaging_Reply      :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?6:5      );
        break;
#endif
    case CLR_DBG_Commands::c_Debugging_Execution_SetCurrentAppDomain  :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1      );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_Create                  :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:3      );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_List                    :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?payloadSize/sizeof(UINT32):0 );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_Stack                   :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?payloadSize/sizeof(UINT32):1 );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_Kill                    :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:1      );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_Suspend                 :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1      );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_Resume                  :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1      );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_GetException            :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 1 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_Unwind                  :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2      );
        break;
    case CLR_DBG_Commands::c_Debugging_Thread_CreateEx                :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:3      );
        break;
    case CLR_DBG_Commands::c_Debugging_Stack_Info                     :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2      );
        SwapEndianPattern( payload, sizeof(UINT32), fReply?5:0      );
        break;
    case CLR_DBG_Commands::c_Debugging_Stack_SetIP                    :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:4      );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_ResizeScratchPad         :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1      );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_GetStack                 :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 4 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_GetField                 :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 3 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_GetArray                 :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 2 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_GetBlock                 :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 1 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_GetScratchPad            :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 1 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_SetBlock                 :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2      );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_SetArray                 :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2      );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_AllocateObject           :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 2 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_AllocateString           :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 2 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_AllocateArray            :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 4 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_Value_Assign                   :
        if (!fReply) SwapEndianPattern(  payload, sizeof(UINT32), 2 );
        else         SwapDebuggingValue( payload, payloadSize       );
        break;
    case CLR_DBG_Commands::c_Debugging_TypeSys_Assemblies             :        
        SwapEndianPattern( payload, sizeof(UINT32), fReply?payloadSize/sizeof(UINT32):0 );
        break;
    case CLR_DBG_Commands::c_Debugging_TypeSys_AppDomains             :
        SwapEndianPattern( payload, sizeof(int), fReply?(payloadSize)/sizeof(int):0 );
        break;
    case CLR_DBG_Commands::c_Debugging_Resolve_Assembly               :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:1  );
        SwapEndianPattern( payload, sizeof(UINT16), fReply?4:0  );
        break;
    case CLR_DBG_Commands::c_Debugging_Resolve_Type                   :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1   );
        break;
    case CLR_DBG_Commands::c_Debugging_Resolve_Field                  :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?2:1   );
        break;
    case CLR_DBG_Commands::c_Debugging_Resolve_Method                 :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:1   );
        break;
    case CLR_DBG_Commands::c_Debugging_Resolve_VirtualMethod          :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:2   );
        break;
    case CLR_DBG_Commands::c_Debugging_Resolve_AppDomain              :
        payloadSize -= SwapEndianPattern( payload, sizeof(UINT32), fReply?1:1               );
        payloadSize -= SwapEndianPattern( payload, 1,              fReply?512:0             );
        SwapEndianPattern( payload, sizeof(UINT32), fReply?(payloadSize/sizeof(UINT32)):0   );
        break;
    case CLR_DBG_Commands::c_Debugging_Deployment_Status              :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?payloadSize/sizeof(UINT32):0 );
        break;
    case CLR_DBG_Commands::c_Debugging_Info_SetJMC                    :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:3   );
        break;
    case CLR_DBG_Commands::c_Profiling_Command                        :
        if ( !fReply )
        {
            // FIXME GJS are these structs packed on the wire? 
            UINT32 data[2];
            UINT8* dp = (UINT8*)data;
            
            payloadSize -= SwapEndianPattern( payload, sizeof(UINT8), 1        );
            if ( payloadSize > sizeof(data) ) 
            {
                memcpy(data, payload, payloadSize);
                SwapEndianPattern( dp, sizeof(UINT32), payloadSize/sizeof(UINT32) );
                memcpy(payload, data, payloadSize);
            }
        }
        else
        {
            SwapEndianPattern( payload, sizeof(UINT32), 1   );   
        }

        break;
    case CLR_DBG_Commands::c_Profiling_Stream                         :
        SwapEndianPattern( payload, sizeof(UINT16), fReply?0:2   );
        break;
    }
}
#endif

