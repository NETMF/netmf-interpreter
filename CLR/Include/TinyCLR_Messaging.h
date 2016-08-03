////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_MESSAGING_H_
#define _TINYCLR_MESSAGING_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyCLR_Types.h>

#include <WireProtocol.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if NUM_MESSAGING > 1
    #define TINYCLR_FOREACH_MESSAGING(ptr)                                 \
            for(int iMessageT = 0; iMessageT < NUM_MESSAGING; iMessageT++) \
            {                                                              \
                CLR_Messaging& ptr = g_CLR_Messaging[ iMessageT ];

#define TINYCLR_FOREACH_MESSAGING_NO_TEMP()                                \
            for(int iMessageT = 0; iMessageT < NUM_MESSAGING; iMessageT++) \
            {                       
#else
    #define TINYCLR_FOREACH_MESSAGING(ptr)                                 \
            {                                                              \
                CLR_Messaging& ptr = g_CLR_Messaging[ 0 ];            
    
    #define TINYCLR_FOREACH_MESSAGING_NO_TEMP()                            \
            {                                                                 
#endif

#define TINYCLR_FOREACH_MESSAGING_END() \
        }

////////////////////////////////////////////////////////////////////////////////////////////////////

typedef bool (*CLR_Messaging_CommandHandler)( WP_Message* msg, void* owner );

struct CLR_Messaging_CommandHandlerLookup
{
    CLR_Messaging_CommandHandler hnd;
    UINT32                       cmd;
};

struct CLR_Messaging_CommandHandlerLookups
{
    const CLR_Messaging_CommandHandlerLookup* table;
    void*                                     owner;
    CLR_UINT32                                size;
};

////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_Messaging_Commands
{
    static const UINT32 c_Messaging_Query                = 0x00020090; // Checks the presence of an EndPoint.
    static const UINT32 c_Messaging_Send                 = 0x00020091; // Sends a message to an EndPoint.
    static const UINT32 c_Messaging_Reply                = 0x00020092; // Response from an EndPoint.

    struct Messaging_Query
    {
        CLR_RT_HeapBlock_EndPoint::Address m_addr;

        struct Reply
        {
            CLR_UINT32                         m_found;
            CLR_RT_HeapBlock_EndPoint::Address m_addr;
        };
    };

    struct Messaging_Send
    {
        CLR_RT_HeapBlock_EndPoint::Address m_addr;
        UINT8                              m_data[ 1 ];

        struct Reply
        {
            CLR_UINT32                         m_found;
            CLR_RT_HeapBlock_EndPoint::Address m_addr;
        };
    };

    struct Messaging_Reply
    {
        CLR_RT_HeapBlock_EndPoint::Address m_addr;
        UINT8                              m_data[ 1 ];

        struct Reply
        {
            CLR_UINT32                         m_found;
            CLR_RT_HeapBlock_EndPoint::Address m_addr;
        };
    };

};


struct CLR_Messaging
{
    struct CachedMessage : public CLR_RT_HeapBlock_Node
    {
        static const CLR_UINT32 c_Processed = 0x00000001;

        CLR_UINT32 m_flags;

        CLR_UINT32 m_size;
        CLR_INT64  m_lastSeen;
        WP_Message m_message;
    };

    //--//

    CLR_Messaging_CommandHandlerLookups m_Lookup_Requests[ 2 ];
    CLR_Messaging_CommandHandlerLookups m_Lookup_Replies[ 2 ];

    static const CLR_UINT32 c_MaxCacheSize = 5 * 1024;
    
    WP_Controller        m_controller;

    CLR_RT_DblLinkedList m_cacheSubordinate;
    CLR_RT_DblLinkedList m_cacheMaster;
    CLR_UINT32           m_cacheTotalSize;

    COM_HANDLE           m_port;

    //--//

    static HRESULT CreateInstance();

    void Initialize(COM_HANDLE port, const CLR_Messaging_CommandHandlerLookup* requestLookup, const CLR_UINT32 requestLookupCount, const CLR_Messaging_CommandHandlerLookup* replyLookup, const CLR_UINT32 replyLookupCount, void* owner );
    void Cleanup();

    static HRESULT DeleteInstance();

    void ProcessCommands();
    void PurgeCache     ();

    bool        SendEvent     ( UINT32 cmd, UINT32 payloadSize, UINT8* payload, UINT32 flags );
    static void BroadcastEvent( UINT32 cmd, UINT32 payloadSize, UINT8* payload, UINT32 flags );

    void ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical, void* ptr, int size );
    void ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical                      );

    static bool Phy_ReceiveBytes   ( void* state, UINT8*& ptr, UINT32 & size );
    static bool Phy_TransmitMessage( void* state, const WP_Message* msg      );

    static bool App_ProcessHeader ( void* state,  WP_Message* msg );
    static bool App_ProcessPayload( void* state,  WP_Message* msg );
    static bool App_Release       ( void* state,  WP_Message* msg );

    bool IsDebuggerInitialized() { return m_fDebuggerInitialized; }
    void InitializeDebugger() { m_fDebuggerInitialized = (DebuggerPort_Initialize( m_port ) == TRUE); }

private:

    bool m_fInitialized;
    bool m_fDebuggerInitialized;

    bool AllocateAndQueueMessage( CLR_UINT32 cmd, UINT32 length, UINT8* data, CLR_RT_HeapBlock_EndPoint::Port port, CLR_RT_HeapBlock_EndPoint::Address addr, CLR_UINT32 found );

    bool ProcessHeader ( WP_Message* msg );
    bool ProcessPayload( WP_Message* msg );


    void PurgeCache( CLR_RT_DblLinkedList& lst, CLR_INT64 oldest );

    bool TransmitMessage( const WP_Message* msg, bool fQueue );

public:  
    static bool Messaging_Query               ( WP_Message* msg, void* owner );
    static bool Messaging_Query__Reply        ( WP_Message* msg, void* owner );
    static bool Messaging_Send                ( WP_Message* msg, void* owner );
    static bool Messaging_Send__Reply         ( WP_Message* msg, void* owner );
    static bool Messaging_Reply               ( WP_Message* msg, void* owner );
    static bool Messaging_Reply__Reply        ( WP_Message* msg, void* owner );
#if defined(NETMF_TARGET_BIG_ENDIAN)
public:     
    static void    SwapDebuggingValue ( UINT8* &msg, UINT32 size                          );
    static void    SwapEndian         ( WP_Message* msg, void* ptr, int size, bool fReply );
    static UINT32  SwapEndianPattern  ( UINT8* &buffer, UINT32 size, UINT32 count=1       );
#endif
};

//--//

extern CLR_UINT32        g_scratchMessaging[];
extern CLR_Messaging    *g_CLR_Messaging;

//--//

#endif // _TINYCLR_MESSAGING_H_
