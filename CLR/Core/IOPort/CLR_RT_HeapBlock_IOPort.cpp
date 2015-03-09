////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

CLR_RT_DblLinkedList CLR_RT_HeapBlock_NativeEventDispatcher::m_ioPorts; 

void CLR_RT_HeapBlock_NativeEventDispatcher::HandlerMethod_Initialize()
{
    NATIVE_PROFILE_CLR_IOPORT();
    CLR_RT_HeapBlock_NativeEventDispatcher::m_ioPorts.DblLinkedList_Initialize();
}

void CLR_RT_HeapBlock_NativeEventDispatcher::HandlerMethod_RecoverFromGC()
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_NativeEventDispatcher,ioPort,CLR_RT_HeapBlock_NativeEventDispatcher::m_ioPorts)
    {
        ioPort->RecoverFromGC();
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_HeapBlock_NativeEventDispatcher::HandlerMethod_CleanUp()
{
    NATIVE_PROFILE_CLR_IOPORT();
    CLR_RT_HeapBlock_NativeEventDispatcher* ioPort;

    while(NULL != (ioPort = (CLR_RT_HeapBlock_NativeEventDispatcher*)CLR_RT_HeapBlock_NativeEventDispatcher::m_ioPorts.FirstValidNode()))
    {
        if(ioPort->m_DriverMethods != NULL)
        {
            ioPort->m_DriverMethods->m_CleanupProc(ioPort);
        }
        ioPort->DetachAll();
        ioPort->ReleaseWhenDeadEx();
    }
}

HRESULT CLR_RT_HeapBlock_NativeEventDispatcher::CreateInstance( CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock& portRef )
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_NativeEventDispatcher* port = NULL;
    
    port = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_HeapBlock_NativeEventDispatcher,DATATYPE_IO_PORT); CHECK_ALLOCATION(port);
    
    {
    
        CLR_RT_ProtectFromGC gc( *port );
    
        port->Initialize();

        m_ioPorts.LinkAtBack( port );
    
        TINYCLR_CHECK_HRESULT(CLR_RT_ObjectToEvent_Source::CreateInstance( port, owner, portRef ));
    
    } 

    // Set pointer to driver custom data to NULL. It initialized later by users of CLR_RT_HeapBlock_NativeEventDispatcher
    port->m_pDrvCustomData = NULL;
    // Set pointers to drivers methods to NULL.
    port->m_DriverMethods  = NULL;

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(port)         port->ReleaseWhenDead();
    }

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_HeapBlock_NativeEventDispatcher::ExtractInstance( CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_NativeEventDispatcher*& port )
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_HEADER();

    CLR_RT_ObjectToEvent_Source* src = CLR_RT_ObjectToEvent_Source::ExtractInstance( ref ); FAULT_ON_NULL(src);

    port = (CLR_RT_HeapBlock_NativeEventDispatcher*)src->m_eventPtr;

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_NativeEventDispatcher::RecoverFromGC()
{
    NATIVE_PROFILE_CLR_IOPORT();
    CheckAll();

    ReleaseWhenDeadEx();
}

bool CLR_RT_HeapBlock_NativeEventDispatcher::ReleaseWhenDeadEx()
{
    NATIVE_PROFILE_CLR_IOPORT();
    if(!IsReadyForRelease()) return false;

    //remove any queued interrupts for this port
    TINYCLR_FOREACH_NODE(CLR_RT_ApplicationInterrupt,interrupt,g_CLR_HW_Hardware.m_interruptData.m_applicationQueue)
    {
        if(this == interrupt->m_interruptPortInterrupt.m_context)
        {
            interrupt->Unlink();

            --g_CLR_HW_Hardware.m_interruptData.m_queuedInterrupts;

            ThreadTerminationCallback( interrupt );
        }
    }
    TINYCLR_FOREACH_NODE_END();

    return ReleaseWhenDead();
}

/************************************************************************************************************
**  Iterates over HAL queue and remove records that point to this instance of CLR_RT_HeapBlock_NativeEventDispatcher.
**  This operation should be done from Dispose as part of destroying the CLR_RT_HeapBlock_NativeEventDispatcher
**
*************************************************************************************************************/

void CLR_RT_HeapBlock_NativeEventDispatcher::RemoveFromHALQueue()

{   // Since we are going to analyze and update the queue we need to disable interrupts.
    // Interrupt service routines add records to this queue.
    GLOBAL_LOCK(irq);
    CLR_UINT32 elemCount = g_CLR_HW_Hardware.m_interruptData.m_HalQueue.NumberOfElements();
    
    // For all elements in the queue
    for ( CLR_UINT32 curElem = 0; curElem < elemCount; curElem++ )
    {   // Retrieve the element ( actually remove it from the queue )
        CLR_HW_Hardware::HalInterruptRecord* testRec = g_CLR_HW_Hardware.m_interruptData.m_HalQueue.Pop();
        
        // Check if context of this record points to the instance of CLR_RT_HeapBlock_NativeEventDispatcher
        // If the "context" is the same as "this", then we skip the "Push" and record is removed.
        if ( testRec->m_context != this )
        { // If it is different from this instance of CLR_RT_HeapBlock_NativeEventDispatcher, thin push it back
          CLR_HW_Hardware::HalInterruptRecord* newRec = g_CLR_HW_Hardware.m_interruptData.m_HalQueue.Push();
          newRec->AssignFrom( *testRec );
        }
    }

}


void CLR_RT_HeapBlock_NativeEventDispatcher::SaveToHALQueue( UINT32 data1, UINT32 data2 )
{
    NATIVE_PROFILE_CLR_IOPORT();
    ASSERT_IRQ_MUST_BE_OFF();

    CLR_HW_Hardware::HalInterruptRecord* rec = g_CLR_HW_Hardware.m_interruptData.m_HalQueue.Push();

    if(rec == NULL)
    {
        g_CLR_HW_Hardware.m_interruptData.m_HalQueue.Pop(); // remove the oldest interrupt to make room for the newest
        rec = g_CLR_HW_Hardware.m_interruptData.m_HalQueue.Push();
    }
    
    if(rec)
    {
        rec->m_data1   = data1;
        rec->m_data2   = data2;
        rec->m_context = this;
        rec->m_time    = Time_GetUtcTime();
    }

    ::Events_Set( SYSTEM_EVENT_HW_INTERRUPT );
}

void SaveNativeEventToHALQueue( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT32 data1, UINT32 data2 )
{
    pContext->SaveToHALQueue( data1, data2 );
}

void CleanupNativeEventsFromHALQueue( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )
{
    pContext->RemoveFromHALQueue();
}

HRESULT CLR_RT_HeapBlock_NativeEventDispatcher::StartDispatch( CLR_RT_ApplicationInterrupt* appInterrupt, CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_HEADER();

    CLR_RT_StackFrame* stackTop;
    CLR_RT_HeapBlock*  args;
    CLR_RT_HeapBlock_Delegate* dlg;
    CLR_RT_HeapBlock* port;
    const CLR_UINT64 c_UTCMask = 0x8000000000000000ULL;

    InterruptPortInterrupt& interrupt = appInterrupt->m_interruptPortInterrupt;

    TINYCLR_CHECK_HRESULT(RecoverManagedObject( port ));
    dlg = port[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_threadSpawn ].DereferenceDelegate(); FAULT_ON_NULL(dlg);

    TINYCLR_CHECK_HRESULT(th->PushThreadProcDelegate( dlg ));

    stackTop = th->CurrentFrame();

    args = stackTop->m_arguments;

    if((stackTop->m_call.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) == 0)
    {
        ++args;
    }

    //
    // set values for delegate arguments
    //
    args[0].SetInteger    ( interrupt.m_data1                        );
    args[1].SetInteger    ( interrupt.m_data2                        );
    args[2].SetInteger    ( (CLR_UINT64)interrupt.m_time | c_UTCMask );
    args[2].ChangeDataType( DATATYPE_DATETIME                        );
    

    th->m_terminationCallback  = CLR_RT_HeapBlock_NativeEventDispatcher::ThreadTerminationCallback;
    th->m_terminationParameter = appInterrupt;

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_NativeEventDispatcher::ThreadTerminationCallback( void* arg )
{
    NATIVE_PROFILE_CLR_IOPORT();
    CLR_RT_ApplicationInterrupt* appInterrupt = (CLR_RT_ApplicationInterrupt*)arg;
    CLR_RT_HeapBlock_NativeEventDispatcher::InterruptPortInterrupt& interrupt = appInterrupt->m_interruptPortInterrupt;

    FreeManagedEvent((interrupt.m_data1 >>  8) & 0xff, //category
                     (interrupt.m_data1      ) & 0xff, //subCategory
                      interrupt.m_data1 >> 16        , //data1
                      interrupt.m_data2              );

    interrupt.m_data1 = 0;
    interrupt.m_data2 = 0;

    CLR_RT_Memory::Release( appInterrupt );

    g_CLR_HW_Hardware.SpawnDispatcher();
}

HRESULT CLR_RT_HeapBlock_NativeEventDispatcher::RecoverManagedObject( CLR_RT_HeapBlock*& port )
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_HEADER();

    // recover the managed object
    TINYCLR_FOREACH_NODE(CLR_RT_ObjectToEvent_Source,ref,this->m_references)
    {
        if(ref->m_objectPtr)
        {
            port = ref->m_objectPtr;
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }
    TINYCLR_FOREACH_NODE_END();

    port = NULL;

    TINYCLR_SET_AND_LEAVE(CLR_E_PIN_DEAD);

    TINYCLR_NOCLEANUP();
}
