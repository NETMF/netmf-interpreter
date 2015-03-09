////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\..\core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

 //this method is called by Hardware_Initialize
 //If you want to initialize your own interrupt handling related objects, put them here
 
HRESULT CLR_HW_Hardware::ManagedHardware_Initialize()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    m_interruptData.m_HalQueue.Initialize( (CLR_HW_Hardware::HalInterruptRecord*)&g_scratchInterruptDispatchingStorage, InterruptRecords() );

    m_interruptData.m_applicationQueue.DblLinkedList_Initialize ();

    m_interruptData.m_queuedInterrupts = 0;

    TINYCLR_NOCLEANUP_NOLABEL();

}

HRESULT CLR_HW_Hardware::SpawnDispatcher()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_ApplicationInterrupt* interrupt;
    CLR_RT_HeapBlock_NativeEventDispatcher* ioPort;
    CLR_RT_HeapBlock_NativeEventDispatcher ::InterruptPortInterrupt *interruptData;

    // if reboot is in progress, just bail out
    if(CLR_EE_DBG_IS( RebootPending )) 
    {
        return S_OK;
    }

    interrupt = (CLR_RT_ApplicationInterrupt*)m_interruptData.m_applicationQueue.FirstValidNode();

    if((interrupt == NULL) || !g_CLR_RT_ExecutionEngine.EnsureSystemThread( g_CLR_RT_ExecutionEngine.m_interruptThread, ThreadPriority::System_Highest ))
    {
        return S_OK;
    }

    interrupt->Unlink();

    interruptData = &interrupt->m_interruptPortInterrupt;
    ioPort = interruptData->m_context;

    CLR_RT_ProtectFromGC gc1 ( *ioPort );
                    
    TINYCLR_SET_AND_LEAVE(ioPort->StartDispatch( interrupt, g_CLR_RT_ExecutionEngine.m_interruptThread ));
            
    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        ioPort->ThreadTerminationCallback( interrupt );
    }

    --m_interruptData.m_queuedInterrupts;

    TINYCLR_CLEANUP_END();    
}

HRESULT CLR_HW_Hardware::TransferAllInterruptsToApplicationQueue()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    while(true)
    {
        HalInterruptRecord* rec;

        {
            GLOBAL_LOCK(irq1);

            rec = m_interruptData.m_HalQueue.Peek();
        }

        if(rec == NULL) break;

        CLR_RT_ApplicationInterrupt* queueRec = (CLR_RT_ApplicationInterrupt*)CLR_RT_Memory::Allocate_And_Erase( sizeof(CLR_RT_ApplicationInterrupt), CLR_RT_HeapBlock::HB_CompactOnFailure );  CHECK_ALLOCATION(queueRec);

        queueRec->m_interruptPortInterrupt.m_data1   =                                          rec->m_data1;
        queueRec->m_interruptPortInterrupt.m_data2   =                                          rec->m_data2;
        queueRec->m_interruptPortInterrupt.m_data3   =                                          rec->m_data3;
        queueRec->m_interruptPortInterrupt.m_time    =                                          rec->m_time;
        queueRec->m_interruptPortInterrupt.m_context = (CLR_RT_HeapBlock_NativeEventDispatcher*)rec->m_context;

        m_interruptData.m_applicationQueue.LinkAtBack( queueRec ); ++m_interruptData.m_queuedInterrupts;

        {
            GLOBAL_LOCK(irq2);
            
            m_interruptData.m_HalQueue.Pop();
        }
    }

    if(m_interruptData.m_queuedInterrupts == 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NO_INTERRUPT);
    }

    TINYCLR_CLEANUP();

    if(CLR_E_OUT_OF_MEMORY == hr)
    {
        // if there is no memory left discard all interrupts to avoid getting into a death spiral of OOM exceptions
        {
            GLOBAL_LOCK(irq3);

            while(!m_interruptData.m_HalQueue.IsEmpty())
            {
                m_interruptData.m_HalQueue.Pop();
            }
        }
    }    
    
    TINYCLR_CLEANUP_END();
}

HRESULT CLR_HW_Hardware::ProcessInterrupts()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(TransferAllInterruptsToApplicationQueue());

    SpawnDispatcher();

    TINYCLR_NOCLEANUP();
}
