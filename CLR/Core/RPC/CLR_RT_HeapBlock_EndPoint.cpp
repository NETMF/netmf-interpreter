////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////


CLR_RT_DblLinkedList CLR_RT_HeapBlock_EndPoint::m_endPoints; 

void CLR_RT_HeapBlock_EndPoint::HandlerMethod_Initialize()
{
    CLR_RT_HeapBlock_EndPoint::m_endPoints.DblLinkedList_Initialize();
}

void CLR_RT_HeapBlock_EndPoint::HandlerMethod_RecoverFromGC()
{
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_EndPoint,endPoint,CLR_RT_HeapBlock_EndPoint::m_endPoints)
    {
        endPoint->RecoverFromGC();
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_HeapBlock_EndPoint::HandlerMethod_CleanUp()
{
}

CLR_RT_HeapBlock_EndPoint* CLR_RT_HeapBlock_EndPoint::FindEndPoint( const CLR_RT_HeapBlock_EndPoint::Port& port )
{
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_EndPoint,endPoint,CLR_RT_HeapBlock_EndPoint::m_endPoints)
    {
        if((endPoint->m_addr.m_type == port.m_type) && (endPoint->m_addr.m_id == port.m_id)) return endPoint; //eliminate the need for another func. call; member variables are public
    }
    TINYCLR_FOREACH_NODE_END();

    return NULL;
}

bool CLR_RT_HeapBlock_EndPoint::Port::Compare( const CLR_RT_HeapBlock_EndPoint::Port& port )
{
    return m_type == port.m_type && m_id == port.m_id;
}

//--//

HRESULT CLR_RT_HeapBlock_EndPoint::CreateInstance( const CLR_RT_HeapBlock_EndPoint::Port& port, CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock& epRef )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_EndPoint* endPoint = NULL;

    //
    // Create a request and stop the calling thread.
    //
    endPoint = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_HeapBlock_EndPoint,DATATYPE_ENDPOINT_HEAD); CHECK_ALLOCATION(endPoint);

    {
        CLR_RT_ProtectFromGC gc( *endPoint );

        endPoint->Initialize();

        endPoint->m_seq  = 0;
        endPoint->m_addr = port;

        endPoint->m_messages.DblLinkedList_Initialize();

        CLR_RT_HeapBlock_EndPoint::m_endPoints.LinkAtBack( endPoint );

        TINYCLR_SET_AND_LEAVE(CLR_RT_ObjectToEvent_Source::CreateInstance( endPoint, owner, epRef ));
    }

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(endPoint) endPoint->ReleaseWhenDeadEx();
    }

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_HeapBlock_EndPoint::ExtractInstance( CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_EndPoint*& endPoint )
{
    TINYCLR_HEADER();

    CLR_RT_ObjectToEvent_Source* src = CLR_RT_ObjectToEvent_Source::ExtractInstance( ref ); FAULT_ON_NULL(src);

    endPoint = (CLR_RT_HeapBlock_EndPoint*)src->m_eventPtr;

    TINYCLR_NOCLEANUP();
}

bool CLR_RT_HeapBlock_EndPoint::ReleaseWhenDeadEx()
{
    if(!IsReadyForRelease()) return false;

    m_messages.DblLinkedList_Release();

    return ReleaseWhenDead();
}

//--//

void CLR_RT_HeapBlock_EndPoint::RecoverFromGC()
{
    CheckAll();

    ReleaseWhenDeadEx();
}

//--//

CLR_RT_HeapBlock_EndPoint::Message* CLR_RT_HeapBlock_EndPoint::FindMessage( CLR_UINT32 cmd, const CLR_UINT32* seq )
{
    TINYCLR_FOREACH_NODE(Message,msg,m_messages)
    {
        if(msg->m_cmd == cmd)
        {
            if(!seq                     ) return msg;
            if(msg->m_addr.m_seq == *seq) return msg;
        }
    }
    TINYCLR_FOREACH_NODE_END();

    return NULL;
}
