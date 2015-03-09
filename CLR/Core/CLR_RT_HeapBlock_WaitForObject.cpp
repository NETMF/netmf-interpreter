////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"


////////////////////////////////////////////////////////////////////////////////////////////////////
// CreateInstance - Creates a wait object 
//
// Result - returns S_OK                 - if the target object is in the signaled state - indicating no wait is needed
//        - returns CLR_E_THREAD_WAITING - if the target object is not signaled - indicating that the caller thread is waiting
//                                         for the object to be signaled
//
HRESULT CLR_RT_HeapBlock_WaitForObject::CreateInstance( CLR_RT_Thread* caller, const CLR_INT64& timeExpire, CLR_RT_HeapBlock* objects, CLR_UINT32 cObjects, bool fWaitAll )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    //
    // Create a request and stop the calling thread.
    //
        
    _ASSERTE( sizeof(CLR_RT_HeapBlock_WaitForObject) % 4 == 0 );

    CLR_UINT32 totLength = (CLR_UINT32)(sizeof(CLR_RT_HeapBlock_WaitForObject) + cObjects * sizeof(CLR_RT_HeapBlock));

    CLR_RT_HeapBlock_WaitForObject* wait = EVENTCACHE_EXTRACT_NODE_AS_BYTES(g_CLR_RT_EventCache,CLR_RT_HeapBlock_WaitForObject,DATATYPE_WAIT_FOR_OBJECT_HEAD,0,totLength); CHECK_ALLOCATION(wait);
    
    wait->m_timeExpire = timeExpire; CLR_RT_ExecutionEngine::InvalidateTimerCache();
    wait->m_cObjects   = cObjects;
    wait->m_fWaitAll   = fWaitAll;

    memcpy( wait->GetWaitForObjects(), objects, sizeof(CLR_RT_HeapBlock) * cObjects );

    caller->m_waitForObject        = wait;
    caller->m_status               = CLR_RT_Thread::TH_S_Waiting;
    caller->m_waitForObject_Result = CLR_RT_Thread::TH_WAIT_RESULT_INIT;

    TINYCLR_SET_AND_LEAVE(CLR_E_THREAD_WAITING);

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_WaitForObject::TryWaitForSignal( CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_WaitForObject* wait = th->m_waitForObject;

    if(wait)
    {
        if (CLR_RT_HeapBlock_WaitForObject::TryWaitForSignal( th, wait->GetWaitForObjects(), wait->m_cObjects, wait->m_fWaitAll ))
        {
            th->Restart( true );
        }
    }
}

bool CLR_RT_HeapBlock_WaitForObject::TryWaitForSignal(CLR_RT_Thread* caller, CLR_RT_HeapBlock* objects, CLR_UINT32 cObjects, bool fWaitAll)
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* blk;
    CLR_RT_HeapBlock* obj;
    
    if(fWaitAll)
    {
        //first time through, find out whether or not the requested objects are signaled    
        //second time through, reset the signalled objects    
        for(int phase = 0; phase < 2; phase++)
        {
            blk = objects;
            for(CLR_UINT32 i = 0; i < cObjects; i++)
            {
                obj = blk->Dereference();
                _ASSERTE(obj != NULL);

                if(phase == 0)
                {
                    if(!obj->IsFlagSet( CLR_RT_HeapBlock::HB_Signaled ))
                    {
                        return false;
                    }
                }
                else
                {
                    if(obj->IsFlagSet( CLR_RT_HeapBlock::HB_SignalAutoReset ))
                    {
                        obj->ResetFlags( CLR_RT_HeapBlock::HB_Signaled );
                    } 
                }

                blk++;
            }
        }

        caller->m_waitForObject_Result = CLR_RT_Thread::TH_WAIT_RESULT_HANDLE_ALL;
        return true;
    }
    else 
    {            
        blk = objects;
        for(CLR_UINT32 i = 0; i < cObjects; i++)
        {
            obj = blk->Dereference();
            _ASSERTE(obj != NULL);

            if(obj->IsFlagSet( CLR_RT_HeapBlock::HB_Signaled ))
            {                                  
                if(obj->IsFlagSet( CLR_RT_HeapBlock::HB_SignalAutoReset ))
                {
                    obj->ResetFlags( CLR_RT_HeapBlock::HB_Signaled );
                }

                caller->m_waitForObject_Result = CLR_RT_Thread::TH_WAIT_RESULT_HANDLE_0 + i;
                return true;
            }

            blk++;
        }
    }    
 
    return false;
}

void CLR_RT_HeapBlock_WaitForObject::SignalObject( CLR_RT_HeapBlock& object )
{
    NATIVE_PROFILE_CLR_CORE();
    object.SetFlags( CLR_RT_HeapBlock::HB_Signaled );

    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,g_CLR_RT_ExecutionEngine.m_threadsWaiting)
    {
        CLR_RT_HeapBlock_WaitForObject::TryWaitForSignal( th );

        if(!object.IsFlagSet( CLR_RT_HeapBlock::HB_Signaled ))
        {
            _ASSERTE(object.IsFlagSet( CLR_RT_HeapBlock::HB_SignalAutoReset ));
            //This is an AutoResetEvent.  Since the event got unsignaled, we can break out of
            //the loop early, as this object can only free one thread.
            break;
        }        
    }
    TINYCLR_FOREACH_NODE_END();
}

HRESULT CLR_RT_HeapBlock_WaitForObject::WaitForSignal( CLR_RT_StackFrame& stack, const CLR_INT64& timeExpire, CLR_RT_HeapBlock* objects, CLR_UINT32 cObjects, bool fWaitAll )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(stack.m_customState == 0)
    {        
        CLR_RT_HeapBlock* objectsT = objects;

        stack.m_customState = 1;

        if(cObjects > 64) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

        for(CLR_UINT32 i = 0; i < cObjects; i++)
        {
            _ASSERTE(objects->DataType() == DATATYPE_OBJECT);

            FAULT_ON_NULL(objectsT->Dereference());

            objectsT++;            
        }

        if(!TryWaitForSignal( stack.m_owningThread, objects, cObjects, fWaitAll ))
        {
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_WaitForObject::CreateInstance( stack.m_owningThread, timeExpire, objects, cObjects, fWaitAll ));
        }
    }

    TINYCLR_NOCLEANUP();
}


HRESULT CLR_RT_HeapBlock_WaitForObject::WaitForSignal ( CLR_RT_StackFrame& stack, const CLR_INT64& timeExpire, CLR_RT_HeapBlock& object )    
{
    NATIVE_PROFILE_CLR_CORE();
    return WaitForSignal( stack, timeExpire, &object, 1, false );
}

void CLR_RT_HeapBlock_WaitForObject::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( GetWaitForObjects(), m_cObjects );
}
