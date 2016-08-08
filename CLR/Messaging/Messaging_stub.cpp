////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\core\Core.h"
#include <TinyCLR_Messaging.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

CLR_Messaging *g_CLR_Messaging;

CLR_UINT32     g_scratchMessaging[sizeof(CLR_Messaging) * NUM_MESSAGING / sizeof(UINT32) + 1];

HRESULT CLR_Messaging::CreateInstance()
{
    NATIVE_PROFILE_CLR_MESSAGING();
    g_CLR_Messaging = (CLR_Messaging*)&g_scratchMessaging[0];
    TINYCLR_SYSTEM_STUB_RETURN();
}

HRESULT CLR_Messaging::DeleteInstance()
{
    NATIVE_PROFILE_CLR_MESSAGING();
    TINYCLR_SYSTEM_STUB_RETURN();
}

bool CLR_Messaging::SendEvent( UINT32 cmd, UINT32 payloadSize, UINT8* payload, UINT32 flags )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    return true;
}

void CLR_Messaging::PurgeCache()
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

void CLR_Messaging::PurgeCache( CLR_RT_DblLinkedList& lst, CLR_INT64 oldest )
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

void CLR_Messaging::ProcessCommands()
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

void CLR_Messaging::Initialize(COM_HANDLE port, const CLR_Messaging_CommandHandlerLookup* requestLookup, const CLR_UINT32 requestLookupCount, const CLR_Messaging_CommandHandlerLookup* replyLookup, const CLR_UINT32 replyLookupCount, void* owner )
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

void CLR_Messaging::ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical )
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

void CLR_Messaging::ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical, void* ptr, int size )
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

void CLR_Messaging::Cleanup()
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

#if defined(NETMF_TARGET_BIG_ENDIAN)

UINT32 CLR_Messaging::SwapEndianPattern( UINT8* &buffer, UINT32 size, UINT32 count )
{
    NATIVE_PROFILE_CLR_MESSAGING();
    return 0;
}

void CLR_Messaging::SwapDebuggingValue( UINT8* &payload, UINT32 payloadSize )
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

void CLR_Messaging::SwapEndian( WP_Message* msg, void* ptr, int size, bool fReply )
{
    NATIVE_PROFILE_CLR_MESSAGING();
}

#endif

