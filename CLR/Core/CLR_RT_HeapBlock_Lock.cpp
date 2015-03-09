////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_HeapBlock_Lock::CreateInstance( CLR_RT_HeapBlock_Lock*& lock, CLR_RT_Thread* th, CLR_RT_HeapBlock& resource )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    lock = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_HeapBlock_Lock,DATATYPE_LOCK_HEAD); CHECK_ALLOCATION(lock);

    lock->m_owningThread = th;                   // CLR_RT_Thread*          m_owningThread;
                                                 //
    lock->m_resource.Assign( resource );         // CLR_RT_HeapBlock        m_resource;
                                                 //
    lock->m_owners  .DblLinkedList_Initialize(); // CLR_RT_DblLinkedList    m_owners;
    lock->m_requests.DblLinkedList_Initialize(); // CLR_RT_DblLinkedList    m_requests;

#if defined(TINYCLR_APPDOMAINS)
    lock->m_appDomain = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();
#endif

    //--//

    if(resource.DataType() == DATATYPE_OBJECT)
    {
        CLR_RT_HeapBlock* ptr = resource.Dereference();

        if(ptr)
        {
            switch(ptr->DataType())
            {
                case DATATYPE_VALUETYPE:
                case DATATYPE_CLASS    :
                    ptr->SetObjectLock( lock );
                    break;
            }
        }
    }

    th->m_locks.LinkAtBack( lock );

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_Lock::DestroyOwner( CLR_RT_SubThread* sth )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Lock::Owner,owner,m_owners)
    {
        if(owner->m_owningSubThread == sth)
        {
            g_CLR_RT_EventCache.Append_Node( owner );
        }
    }
    TINYCLR_FOREACH_NODE_END();

    if(m_owners.IsEmpty())
    {
        ChangeOwner();
    }
}

void CLR_RT_HeapBlock_Lock::ChangeOwner()
{
    NATIVE_PROFILE_CLR_CORE();
    m_owners.DblLinkedList_PushToCache();

    while(true)
    {
        CLR_RT_HeapBlock_LockRequest* req = (CLR_RT_HeapBlock_LockRequest*)m_requests.ExtractFirstNode(); if(!req) break;

        CLR_RT_SubThread* sth = req->m_subthreadWaiting;
        CLR_RT_Thread*    th  = sth->m_owningThread;

        g_CLR_RT_EventCache.Append_Node( req );

        sth->ChangeLockRequestCount( -1 );

        th->m_locks.LinkAtBack( this );

        m_owningThread = th;

        CLR_RT_HeapBlock_Lock::IncrementOwnership( this, sth, TIMEOUT_INFINITE, false );

        //
        // If the new owner was waiting on something, update the flags.
        //
        {
            CLR_RT_StackFrame* stack = th->CurrentFrame();

            if(stack->m_flags & CLR_RT_StackFrame::c_PendingSynchronizeGlobally)
            {
                stack->m_flags &= ~CLR_RT_StackFrame::c_PendingSynchronizeGlobally;
                stack->m_flags |=  CLR_RT_StackFrame::c_SynchronizedGlobally;
            }

            if(stack->m_flags & CLR_RT_StackFrame::c_PendingSynchronize)
            {
                stack->m_flags &= ~CLR_RT_StackFrame::c_PendingSynchronize;
                stack->m_flags |=  CLR_RT_StackFrame::c_Synchronized;
            }
        }

        return;
    }

    //
    // None is listening for this object, unlock it.
    //
    if(m_resource.DataType() == DATATYPE_OBJECT)
    {
        CLR_RT_HeapBlock* ptr = m_resource.Dereference();

        if(ptr)
        {
            switch(ptr->DataType())
            {
                case DATATYPE_VALUETYPE:
                case DATATYPE_CLASS    :
                    ptr->SetObjectLock( NULL );
                    break;
            }
        }
    }

    g_CLR_RT_EventCache.Append_Node( this );
}

HRESULT CLR_RT_HeapBlock_Lock::IncrementOwnership( CLR_RT_HeapBlock_Lock* lock, CLR_RT_SubThread* sth, const CLR_INT64& timeExpire, bool fForce )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Thread* th      = sth ->m_owningThread;
    CLR_RT_Thread* thOwner = lock->m_owningThread;

    if(thOwner == th)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Lock::Owner,owner,lock->m_owners)
        {
            if(owner->m_owningSubThread == sth)
            {
                owner->m_recursion++;
                TINYCLR_SET_AND_LEAVE(S_OK);
            }
        }
        TINYCLR_FOREACH_NODE_END();

        {
            CLR_RT_HeapBlock_Lock::Owner* owner = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,Owner,DATATYPE_LOCK_OWNER_HEAD); CHECK_ALLOCATION(owner);

            owner->m_owningSubThread = sth;
            owner->m_recursion       = 1;

            lock->m_owners.LinkAtFront( owner );
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }

    //
    // Create a request and stop the calling thread.
    //
    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_LockRequest::CreateInstance( lock, sth, timeExpire, fForce ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Lock::DecrementOwnership( CLR_RT_HeapBlock_Lock* lock, CLR_RT_SubThread* sth )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Thread* th = sth->m_owningThread;

    if(lock && lock->m_owningThread == th)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Lock::Owner,owner,lock->m_owners)
        {
            if(owner->m_owningSubThread == sth)
            {
                if(--owner->m_recursion == 0)
                {
                    g_CLR_RT_EventCache.Append_Node( owner );
                }

                //--//

                if(lock->m_owners.IsEmpty())
                {
                    lock->ChangeOwner();
                }

                TINYCLR_SET_AND_LEAVE(S_OK);
            }
        }
        TINYCLR_FOREACH_NODE_END();
    }

    TINYCLR_SET_AND_LEAVE(CLR_E_LOCK_SYNCHRONIZATION_EXCEPTION);

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_Lock::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    m_resource.Relocate__HeapBlock();
}

void CLR_RT_HeapBlock_Lock::Relocate_Owner()
{
    NATIVE_PROFILE_CLR_CORE();
}
