////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_SubThread::CreateInstance( CLR_RT_Thread* th, CLR_RT_StackFrame* stack, int priority, CLR_RT_SubThread*& sthRef )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_SubThread* sth = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_SubThread,DATATYPE_SUBTHREAD); CHECK_ALLOCATION(sth);

    sth->m_owningThread      = th;                              // CLR_RT_Thread*     m_owningThread;
    sth->m_owningStackFrame  = stack;                           // CLR_RT_StackFrame* m_owningStackFrame;
    sth->m_lockRequestsCount = 0;                               // CLR_UINT32         m_lockRequestsCount;
                                                                //
    sth->m_priority          = priority;                        // int                m_priority;
    
    sth->m_timeConstraint    = TIMEOUT_INFINITE;                // CLR_INT64          m_timeConstraint;
    sth->m_status            = 0;                               // CLR_UINT32         m_status;

    th->m_subThreads.LinkAtBack( sth );

    TINYCLR_CLEANUP();

    sthRef = sth;

    TINYCLR_CLEANUP_END();
}

void CLR_RT_SubThread::DestroyInstance( CLR_RT_Thread* th, CLR_RT_SubThread* sthBase, int flags )
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // You cannot destroy a subthread without destroying all the children subthreads.
    //
    while(true)
    {
        CLR_RT_SubThread* sth = th->CurrentSubThread(); if(sth->Prev() == NULL) break;

        //
        // Release all the frames for this subthread.
        //
        while(true)
        {
            CLR_RT_StackFrame* stack = th->CurrentFrame(); if(stack->Prev() == NULL) break;

            if(stack == sth->m_owningStackFrame) break;

            stack->Pop();
        }

        //
        // Release all the locks for this subthread.
        //
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Lock,lock,th->m_locks)
        {
            lock->DestroyOwner( sth );
        }
        TINYCLR_FOREACH_NODE_END();

        //
        // Release all the lock requests.
        //
        g_CLR_RT_ExecutionEngine.DeleteLockRequests( NULL, sth );

        if(sth == sthBase && (flags & CLR_RT_SubThread::MODE_IncludeSelf) == 0) break;

        g_CLR_RT_EventCache.Append_Node( sth );

        if(sth == sthBase) break;
    }
}

bool CLR_RT_SubThread::ChangeLockRequestCount( int diff )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_Thread* th = m_owningThread;

    this->m_lockRequestsCount += diff;
    th  ->m_lockRequestsCount += diff;

    if(th->m_lockRequestsCount == 0)
    {
        th->Restart( false );

        return true;
    }
    else
    {
        th->m_status = CLR_RT_Thread::TH_S_Waiting;

        return false;
    }
}

void CLR_RT_Thread::BringExecCounterToDate( int iGlobalExecutionCounter, int iDebitForEachRun )

{   // Normally the condition is false. It becames true if thread was out of execution for some time.
    // The value of ThreadPriority::System_Highest + 1) is 33. 
    // 33 for ThreadPriority::Highest gives up to 16 cycles to catch up.
    // 33 for ThreadPriority::Lowest we provide only 1 cycle to catch up.
    // If thread was sleeping for some time we forefeet the time it was sleeping and not updating execution counter. 
    if ( m_executionCounter - iGlobalExecutionCounter > (int)((1 << ThreadPriority::System_Highest) + 1) )
    {
       m_executionCounter = iGlobalExecutionCounter + (int)((1 << ThreadPriority::System_Highest) + 1); 
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_RT_Thread::IsFinalizerThread()
{
    NATIVE_PROFILE_CLR_CORE();
    return g_CLR_RT_ExecutionEngine.m_finalizerThread == this;    
}

bool CLR_RT_Thread::CanThreadBeReused()
{
    NATIVE_PROFILE_CLR_CORE();
    return (m_flags & CLR_RT_Thread::TH_F_System) && 
        (m_status == CLR_RT_Thread::TH_S_Terminated || m_status == CLR_RT_Thread::TH_S_Unstarted );
}

HRESULT CLR_RT_Thread::PushThreadProcDelegate( CLR_RT_HeapBlock_Delegate* pDelegate )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Instance inst;

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain*         appDomainSav = g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( pDelegate->m_appDomain );
#endif

    if(pDelegate == NULL || pDelegate->DataType() != DATATYPE_DELEGATE_HEAD || inst.InitializeFromIndex( pDelegate->DelegateFtn() ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

#if defined(TINYCLR_APPDOMAINS)

    if(!pDelegate->m_appDomain->IsLoaded())
    {        
        if(!IsFinalizerThread())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_APPDOMAIN_EXITED);
        }

        m_flags |= CLR_RT_Thread::TH_F_ContainsDoomedAppDomain;
    }
#endif

    this->m_dlg     = pDelegate;
    this->m_status  = TH_S_Ready;

    TINYCLR_CHECK_HRESULT(CLR_RT_StackFrame::Push( this, inst, inst.m_target->numArgs ));

    if((inst.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) == 0)
    {
        CLR_RT_StackFrame* stackTop = this->CurrentFrame();

        stackTop->m_arguments[ 0 ].Assign( pDelegate->m_object );
    }    

   g_CLR_RT_ExecutionEngine.PutInProperList( this );

   TINYCLR_CLEANUP();
   
#if defined(TINYCLR_APPDOMAINS)
    g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );
#endif
    
   TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_Thread::CreateInstance( int pid, int priority, CLR_RT_Thread*& th, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_SubThread*         sth;   

    //--//

    th = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_Thread,DATATYPE_THREAD); CHECK_ALLOCATION(th);

    {
        CLR_RT_ProtectFromGC gc( (void**)&th, CLR_RT_Thread::ProtectFromGCCallback );

        th->Initialize();

        th->m_pid                            = pid;                             // int                        m_pid;
        th->m_status                         = TH_S_Unstarted;                 // CLR_UINT32                 m_status;
        th->m_flags                          = flags;                           // CLR_UINT32                 m_flags;
        th->m_executionCounter               = 0;                               // int                        m_executionCounter;
        th->m_timeQuantumExpired             = FALSE;                           // BOOL                       m_timeQuantumExpired;
                                                                                //
        th->m_dlg                            = NULL;                            // CLR_RT_HeapBlock_Delegate* m_dlg;
        th->m_currentException               .SetObjectReference( NULL );       // CLR_RT_HeapBlock           m_currentException;
                                                                                // UnwindStack                m_nestedExceptions[c_MaxStackUnwindDepth];
        th->m_nestedExceptionsPos            = 0;                               // int                        m_nestedExceptionsPos;

                                                                                //
                                                                                // //--//
                                                                                //
        th->m_terminationCallback            = NULL;                            // ThreadTerminationCallback  m_terminationCallback;
        th->m_terminationParameter           = NULL;                            // void*                      m_terminationParameter;
                                                                                //
        th->m_waitForEvents                  = 0;                               // CLR_UINT32                 m_waitForEvents;
        th->m_waitForEvents_Timeout          = TIMEOUT_INFINITE;                // CLR_INT64                  m_waitForEvents_Timeout;
        th->m_waitForEvents_IdleTimeWorkItem = TIMEOUT_ZERO;                    // CLR_INT64                  m_waitForEvents_IdleTimeWorkItem;
                                                                                //
        th->m_locks                          .DblLinkedList_Initialize();       // CLR_RT_DblLinkedList       m_locks;
        th->m_lockRequestsCount              = 0;                               // CLR_UINT32                 m_lockRequestsCount;
        th->m_waitForObject                  = NULL;
                                                                                //
        th->m_stackFrames                    .DblLinkedList_Initialize();       // CLR_RT_DblLinkedList       m_stackFrames;
                                                                                //
        th->m_subThreads                     .DblLinkedList_Initialize();       // CLR_RT_DblLinkedList       m_subThreads;
                                                                                //
        #if defined(ENABLE_NATIVE_PROFILER)
        th->m_fNativeProfiled                = false;
        #endif

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        th->m_scratchPad                     = -1;                              // int                        m_scratchPad;
        th->m_fHasJMCStepper                 = false;                           // bool                       m_fHasJMCStepper

        // For normal threads created in CLR m_realThread  points to the thread object.
        // If debugger creates managed thread for function evaluation, then m_realThread  points to the thread that has focus in debugger
        th->m_realThread                     = th;                              // CLR_RT_Thread*             m_realThread
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

        //--//

        TINYCLR_CHECK_HRESULT(CLR_RT_SubThread::CreateInstance( th, NULL, priority, sth ));

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(g_CLR_RT_ExecutionEngine.m_breakpointsNum)
        {
            //
            // This needs to happen before the Push
            //
            g_CLR_RT_ExecutionEngine.Breakpoint_Thread_Created( th );
        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)   
    }
        
    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_Thread::CreateInstance( int pid, CLR_RT_HeapBlock_Delegate* pDelegate, int priority, CLR_RT_Thread*& th, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(CreateInstance( pid, priority, th, flags ));

    if(pDelegate)
    {
        TINYCLR_CHECK_HRESULT(th->PushThreadProcDelegate( pDelegate ));
    }

    TINYCLR_NOCLEANUP();
}

void CLR_RT_Thread::DestroyInstance()
{
    NATIVE_PROFILE_CLR_CORE();
    DetachAll();

    Passivate();

    //Prevent ReleaseWhenDeadEx from keeping the thread alive
    if(m_flags & CLR_RT_Thread::TH_F_System)
    {
        m_flags &= ~CLR_RT_Thread::TH_F_System;
        OnThreadTerminated();
    }

    ReleaseWhenDeadEx();
}

bool CLR_RT_Thread::ReleaseWhenDeadEx()
{
    NATIVE_PROFILE_CLR_CORE();
    //maybe separate for shutdown....
    //These threads live forever?!!?
    if(m_flags & CLR_RT_Thread::TH_F_System) return false;

    if(!IsReadyForRelease()) return false;

    if(this == g_CLR_RT_ExecutionEngine.m_finalizerThread) g_CLR_RT_ExecutionEngine.m_finalizerThread = NULL;
    if(this == g_CLR_RT_ExecutionEngine.m_interruptThread) g_CLR_RT_ExecutionEngine.m_interruptThread = NULL;
    if(this == g_CLR_RT_ExecutionEngine.m_timerThread    ) g_CLR_RT_ExecutionEngine.m_timerThread     = NULL;
    if(this == g_CLR_RT_ExecutionEngine.m_cctorThread    ) g_CLR_RT_ExecutionEngine.m_cctorThread     = NULL;
    
    return ReleaseWhenDead();
}

//--//

void CLR_RT_Thread::ProtectFromGCCallback( void* state )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_Thread* th = (CLR_RT_Thread*)state;
    
    g_CLR_RT_GarbageCollector.Thread_Mark( th );
}

//--//

HRESULT CLR_RT_Thread::Suspend()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if((m_flags & CLR_RT_Thread::TH_F_Suspended) == 0 && m_status != CLR_RT_Thread::TH_S_Terminated)
    {
        m_flags |= CLR_RT_Thread::TH_F_Suspended;

        g_CLR_RT_ExecutionEngine.PutInProperList( this );
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_Thread::Resume()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if((m_flags & CLR_RT_Thread::TH_F_Suspended) != 0 && m_status != CLR_RT_Thread::TH_S_Terminated)
    {
        m_flags &= ~CLR_RT_Thread::TH_F_Suspended;

        g_CLR_RT_ExecutionEngine.PutInProperList( this );
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_Thread::Terminate()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    m_status = CLR_RT_Thread::TH_S_Terminated;

    //An exception is needed to ensure that StackFrame::Pop does not copy uninitialized data
    //to callers evaluation stacks.  This would likely be harmless, as the entire thread is about to be killed
    //However, this is simply a safeguard to prevent possible problems if it ever happens that
    //between the start and end of killing the thread, a GC gets run.
    (void)Library_corlib_native_System_Exception::CreateInstance( m_currentException, g_CLR_RT_WellKnownTypes.m_ThreadAbortException, S_OK, CurrentFrame() );

    g_CLR_RT_ExecutionEngine.PutInProperList( this );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_Thread::Abort()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    //
    // Only abort a non-terminated thread...
    //
    if(m_status != CLR_RT_Thread::TH_S_Terminated)
    {
        (void)Library_corlib_native_System_Exception::CreateInstance( m_currentException, g_CLR_RT_WellKnownTypes.m_ThreadAbortException, S_OK, CurrentFrame() );

        m_flags |= CLR_RT_Thread::TH_F_Aborted;

        Restart( true );
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_RT_Thread::Restart( bool fDeleteEvent )
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // Wake up and queue.
    //
    m_status = CLR_RT_Thread::TH_S_Ready;

    g_CLR_RT_ExecutionEngine.PutInProperList( this );

    if(fDeleteEvent)
    {
        m_waitForEvents         = 0;
        m_waitForEvents_Timeout = TIMEOUT_INFINITE;
    }
}

void CLR_RT_Thread::OnThreadTerminated()
{
    NATIVE_PROFILE_CLR_CORE();
    SignalAll();

    //
    // Release all the subthreads.
    //
    CLR_RT_SubThread::DestroyInstance( this, NULL, 0 );
    
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    if(g_CLR_RT_ExecutionEngine.m_breakpointsNum)
    {                       
        g_CLR_RT_ExecutionEngine.Breakpoint_Thread_Terminated( this );
    }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
}

void CLR_RT_Thread::Passivate()
{
    NATIVE_PROFILE_CLR_CORE();
    m_flags  &= ~(CLR_RT_Thread::TH_F_Suspended | CLR_RT_Thread::TH_F_ContainsDoomedAppDomain | CLR_RT_Thread::TH_F_Aborted);

    g_CLR_RT_ExecutionEngine.m_threadsZombie.LinkAtFront( this );

    m_waitForEvents         = 0;
    m_waitForEvents_Timeout = TIMEOUT_INFINITE;

    //--//

    if(m_waitForObject != NULL)
    {
        g_CLR_RT_EventCache.Append_Node( m_waitForObject );
        m_waitForObject = NULL;
    }

    //--//
        
     if((m_flags & CLR_RT_Thread::TH_F_System) == 0)
     {
         m_status  =  CLR_RT_Thread::TH_S_Terminated;
         OnThreadTerminated();
     }
     else
     {
         m_status = CLR_RT_Thread::TH_S_Unstarted;
     }

     m_currentException.SetObjectReference( NULL ); // Reset exception flag.

    //
    // If the thread is associated with a timer, advance the state of the timer.
    //
    if(m_terminationCallback)
    {
        ThreadTerminationCallback terminationCallback = m_terminationCallback; m_terminationCallback = NULL;

        terminationCallback( m_terminationParameter );
    }
        
    if(m_status == CLR_RT_Thread::TH_S_Terminated || m_status == CLR_RT_Thread::TH_S_Unstarted)
    {        
        //This is used by Static constructor thread.
        m_dlg = NULL;
    }

    ReleaseWhenDeadEx();
}

bool CLR_RT_Thread::CouldBeActivated()
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_waitForEvents_Timeout != TIMEOUT_INFINITE) return true;
    if(m_waitForEvents                            ) return true;

    return false;
}

void CLR_RT_Thread::RecoverFromGC()
{
    NATIVE_PROFILE_CLR_CORE();
    CheckAll();
}

void CLR_RT_Thread::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_dlg );

    m_currentException.Relocate__HeapBlock();

    for(int i=0; i<m_nestedExceptionsPos; i++)
    {
        UnwindStack& us = m_nestedExceptions[ i ];

        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&us.m_stack             );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&us.m_handlerStack      );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&us.m_exception         );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&us.m_ip                );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&us.m_currentBlockStart );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&us.m_currentBlockEnd   );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&us.m_handlerBlockStart );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&us.m_handlerBlockEnd   );
    }
}

//--//

#if defined(TINYCLR_TRACE_CALLS)

void CLR_RT_Thread::DumpStack()
{
    NATIVE_PROFILE_CLR_CORE();
    LPCSTR szStatus;

    switch(m_status)
    {
    case TH_S_Ready     : szStatus = "Ready"     ; break;
    case TH_S_Waiting   : szStatus = "Waiting"   ; break;
    case TH_S_Terminated: szStatus = "Terminated"; break;
    default             : szStatus = ""          ; break;
    }

    CLR_Debug::Printf( "Thread: %d %d %s %s\r\n", m_pid, GetThreadPriority(), szStatus, (m_flags & CLR_RT_Thread::TH_F_Suspended) ? "Suspended" : "" );

    TINYCLR_FOREACH_NODE_BACKWARD(CLR_RT_StackFrame,stack,m_stackFrames)
    {
        CLR_Debug::Printf( "    " ); CLR_RT_DUMP::METHOD( stack->m_call ); CLR_Debug::Printf( " [IP: %04x]\r\n", (stack->m_IP - stack->m_IPstart) );
    }
    TINYCLR_FOREACH_NODE_BACKWARD_END();
}

#endif

//--//

void CLR_RT_Thread::ProcessException_FilterPseudoFrameCopyVars(CLR_RT_StackFrame* to, CLR_RT_StackFrame* from)
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT8 numArgs = from->m_call.m_target->numArgs;

    if(numArgs)
    {
        memcpy( to->m_arguments, from->m_arguments, sizeof(CLR_RT_HeapBlock) * numArgs );
    }

    if(from->m_call.m_target->numLocals)
    {
        memcpy( to->m_locals, from->m_locals, sizeof(CLR_RT_HeapBlock) * from->m_call.m_target->numLocals );
    }
}

HRESULT CLR_RT_Thread::ProcessException_EndFilter()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_StackFrame*  stack   = CurrentFrame();
    CLR_INT32           choice  = stack->PopValue().NumericByRef().s4;


    UnwindStack& us = m_nestedExceptions[ m_nestedExceptionsPos - 1 ];
    
    ProcessException_FilterPseudoFrameCopyVars(us.m_handlerStack, stack);

    //Clear the stack variable so Pop doesn't remove us from the UnwindStack.
    us.m_stack = NULL;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    //We don't want to send any breakpoints until after we set the IP appropriately
    bool fBreakpointsDisabledSav = CLR_EE_DBG_IS(BreakpointsDisabled);
    CLR_EE_DBG_SET(BreakpointsDisabled);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

      stack->Pop();

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    if(!fBreakpointsDisabledSav)
    {
        CLR_EE_DBG_CLR(BreakpointsDisabled);
    }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    if(choice == 1)
    {
        //The filter signaled that it will handle this exception. Update the phase state
        us.SetPhase(UnwindStack::p_2_RunningFinallys_0);
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        g_CLR_RT_ExecutionEngine.Breakpoint_Exception(us.m_handlerStack, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_EXCEPTION_HANDLER_FOUND, us.m_handlerBlockStart);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    }
    else
    {
        //Store the IP so Phase1/FindEhBlock knows to start looking from the point of the filter we were executing.
        us.m_ip = us.m_currentBlockStart;
    }

    //Signal that this is a continuation of processing for the handler on top of the unwind stack.
    us.m_flags |= UnwindStack::c_ContinueExceptionHandler;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    //We must stop if we sent out a Catch Handler found message.
    if(CLR_EE_DBG_IS( Stopped ))
    {   //If the debugger stopped because of the messages we sent, then we should break out of Execute_IL, drop down,
        //and wait for the debugger to continue.
        m_currentException.SetObjectReference(us.m_exception);
        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
    }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    (void)ProcessException();


    // Swap results around. ProcessException must return a success code in Thread::Execute and set m_currentException to continue processing.
    // Execute_IL must get a FAILED hr in order to break outside of the execution loop.
    if(m_currentException.Dereference() == NULL)
    {
        //Return S_OK because exception handling is complete or handling is in-flight and needs to execute IL to continue.
        TINYCLR_SET_AND_LEAVE(S_OK);
    }
    else
    {
        //Return PROCESS_EXCEPTION to break out of the IL loop to rerun ProcessException and/or abort from an unhandled exception
        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_Thread::ProcessException_EndFinally()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_StackFrame* stack = CurrentFrame();
    UnwindStack&       us    = m_nestedExceptions[ m_nestedExceptionsPos - 1 ];
    
    if(us.m_ip)
    {
        CLR_PMETADATA           ipLeave = us.m_ip;
        CLR_RT_ExceptionHandler eh;

        if(FindEhBlock( stack, stack->m_IP-1, ipLeave, eh, true ))
        {
            us.m_stack             = stack;
            us.m_exception         = NULL;
            us.m_ip                = ipLeave;
            us.m_currentBlockStart = eh.m_handlerStart;
            us.m_currentBlockEnd   = eh.m_handlerEnd;
            //Leave is not valid to leave a finally block, and is the only thing that leaves m_ip set when executing IL.
            //Therefore if we're here then we are not interfering with an unhandled exception and flags can be safely set.
            us.SetPhase( UnwindStack::p_4_NormalCleanup );
            stack->m_IP             = eh.m_handlerStart  ;
        }
        else
        {   //We're truely done with finally's for now. Pop off the handler
            m_nestedExceptionsPos--;
            stack->m_IP = ipLeave;
        }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(stack->m_flags & CLR_RT_StackFrame::c_HasBreakpoint)
        {
            g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Step( stack, stack->m_IP );
        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)        

        TINYCLR_SET_AND_LEAVE(S_OK);
    }
    else if(us.m_exception)
    {
        //This finally block was executed because an exception was thrown inside its protected block.
        //Signal that this is a continuation of an already existing EH process.
        us.m_flags |= UnwindStack::c_ContinueExceptionHandler;

        (void)ProcessException();
        //Similar to EndFilter, we need to swap the return codes around. Thread::Execute needs a success code or the thread will be aborted.
        //ExecuteIL needs a failure code or we'll continue to execute IL when we possibly shouldn't.
        if (m_currentException.Dereference() == NULL)
        {
            //Return S_OK because exception handling is complete or handling is in-flight and needs to execute IL to continue.
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
        else
        {
            //Return PROCESS_EXCEPTION to break out of the IL loop to rerun ProcessException and/or abort from an unhandled exception
            TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
        }
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_Thread::ProcessException_Phase1()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    // Load the UnwindStack entry to process, as created/loaded by ProcessException
    UnwindStack& us = m_nestedExceptions[ m_nestedExceptionsPos - 1 ];

    CLR_RT_ExceptionHandler eh;

    // If we were executing a filter that returned false, there's not much point checking the stack frames above the point of the filter.
    // Try to resume from the frame of the last filter executed.
    CLR_RT_StackFrame*  stack     = us.m_handlerStack;
    
#ifndef TINYCLR_NO_IL_INLINE
    CLR_RT_InlineFrame tmpInline;
    tmpInline.m_IP = NULL;
#endif
    
    // If this is the first pass through _Phase1 then start at the top.
    if(!stack) { stack = CurrentFrame(); }

    //Search for a willing catch handler.
    while(stack->Caller() != NULL)
    {
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(g_CLR_RT_ExecutionEngine.m_breakpointsNum && us.GetPhase() < UnwindStack::p_1_SearchingForHandler_2_SentUsersChance && stack->m_IP)
        {   //We have a debugger attached and we need to send some messages before we start searching.
            //These messages should only get sent when the search reaches managed code. Stack::Push sets m_IP to NULL for native code,
            //so therefore we need IP to be non-NULL

            us.m_handlerStack   = stack;

            if(us.GetPhase() < UnwindStack::p_1_SearchingForHandler_1_SentFirstChance)
            {
                g_CLR_RT_ExecutionEngine.Breakpoint_Exception( stack, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_EXCEPTION_FIRST_CHANCE, NULL );
                us.SetPhase(UnwindStack::p_1_SearchingForHandler_1_SentFirstChance);

                //Break out here, because of synchronization issues (false positives) with JMC checking.
                if(CLR_EE_DBG_IS( Stopped )) { goto ContinueAndExit; }
            }

            //In order to send the User's first chance message, we have to know that we're in JMC
            //Do we have thread synchronization issues here? The debugger is sending out 3 Not My Code messages for a function,
            //presumably the one on the top of the stack, but when we execute this, we're reading true.
            if(stack->m_call.DebuggingInfo().IsJMC())
            {
                g_CLR_RT_ExecutionEngine.Breakpoint_Exception( stack, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_EXCEPTION_USERS_CHANCE, NULL );
                us.SetPhase( UnwindStack::p_1_SearchingForHandler_2_SentUsersChance );
                if(CLR_EE_DBG_IS(Stopped)) { goto ContinueAndExit; }
            }
        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

        if(stack->m_call.m_target->flags & CLR_RECORD_METHODDEF::MD_HasExceptionHandlers)
        {
            CLR_PMETADATA ip;
            if (us.m_ip)
            {
                ip = us.m_ip;       //Use the IP set by endfilter
                us.m_ip = NULL;     //Reset to prevent catch block & PopEH issues via 'leave' or 'endfinally'
            }
            else
            {
                ip = stack->m_IP;   //Normal case: use the IP where the exception was thrown.
            }

            if(ip) // No IP? Either out of memory during allocation of stack frame or native method.
            {
                if(FindEhBlock( stack, ip, NULL, eh, false ))
                {   //There are two cases here:
                    //1. We found a catch block... in this case, we want to break out and go to phase 2.
                    //2. We found a filter... in this case, we want to duplicate the stack and execute the filter.

                    //Store the handler block address and stack frame.
                    //It's needed in Phase2 for when finally's are finished and we execute the catch handler
                    us.m_handlerBlockStart = eh.m_handlerStart;
                    us.m_handlerBlockEnd   = eh.m_handlerEnd;
                    us.m_handlerStack      = stack;

#ifndef TINYCLR_NO_IL_INLINE
                    if(tmpInline.m_IP)
                    {
                        us.m_flags |= UnwindStack::c_MagicCatchForInline;
                    }
#endif

                    if (eh.IsFilter())
                    {
                        CLR_RT_StackFrame*  newStack  = NULL;

                        //Store the IP range that we're currently executing so leave/PopEH doesn't accidentally pop the filter off.
                        us.m_currentBlockStart = eh.m_userFilterStart;
                        us.m_currentBlockEnd   = eh.m_handlerStart;

                        //Create a pseudo-frame at the top of the stack so the filter can call other functions.
                        CLR_UINT8 numArgs = stack->m_call.m_target->numArgs;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
                        //We don't want to send any breakpoints until after we set the IP appropriately
                        bool fBreakpointsDisabledSav = CLR_EE_DBG_IS(BreakpointsDisabled);
                        CLR_EE_DBG_SET(BreakpointsDisabled);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

                        hr = CLR_RT_StackFrame::Push( stack->m_owningThread, stack->m_call, numArgs );

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
                        if(!fBreakpointsDisabledSav)
                        {
                            CLR_EE_DBG_CLR(BreakpointsDisabled);
                        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)                        

                        if(FAILED(hr))
                        {   //We probably ran out of memory. In either case, don't run this handler.
                            //Set the IP so we'll try the next catch block.
                            us.m_ip = us.m_currentBlockStart;
                            continue;
                        }
                        
                        //stack is the original filter stack frame; newStack is the new pseudoframe
                        newStack = CurrentFrame();
                        newStack->m_flags |= CLR_RT_StackFrame::c_PseudoStackFrameForFilter;
                        us.m_stack = newStack;

                        //Copy local variables and arguments so the filter has access to them.
                        if(numArgs)
                        {
                            memcpy( newStack->m_arguments, stack->m_arguments, sizeof(CLR_RT_HeapBlock) * numArgs );
                        }

                        if (stack->m_call.m_target->numLocals)
                        {
                            memcpy( newStack->m_locals, stack->m_locals, sizeof(CLR_RT_HeapBlock) * stack->m_call.m_target->numLocals );
                        }

                        newStack->PushValueAndAssign( m_currentException );

                        //Set the ip to the handler
                        newStack->m_IP = eh.m_userFilterStart;

                        //We are willing to execute IL again so clear the m_currentException flag.
                        m_currentException.SetObjectReference( NULL );

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
                        g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Push( newStack, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_STEP_INTERCEPT );
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

                        //Return a success value to break out of ProcessException and to signal that execution of IL can continue.
                        TINYCLR_SET_AND_LEAVE(S_OK);
                    }
                    else
                    {   //We found a normal Catch or CatchAll block. We are all set to proceed onto the Unwinding phase.
                        //Note that we found a catch handler so we don't look for it again for this exception.
                        us.SetPhase( UnwindStack::p_2_RunningFinallys_0 );

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
                        if(g_CLR_RT_ExecutionEngine.m_breakpointsNum)
                        {
                            g_CLR_RT_ExecutionEngine.Breakpoint_Exception( stack, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_EXCEPTION_HANDLER_FOUND, eh.m_handlerStart );
                            if(CLR_EE_DBG_IS(Stopped)) { goto ContinueAndExit; }
                        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

                        //We want to continue running EH "goo" code so leave m_currentException set and return PROCESS_EXCEPTION
                        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
                    }
                }
            }
        }

        //We didn't find a catch block at this level...
        //Check to see if we trickled up to a pseudoStack frame that we created to execute a handler
        //Both of these shouldn't be set at once because of the two-pass handling mechanism.
        if (stack->m_flags & CLR_RT_StackFrame::c_AppDomainTransition)
        {
            us.m_handlerStack = NULL;
            us.SetPhase(UnwindStack::p_2_RunningFinallys_0);

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            if(g_CLR_RT_ExecutionEngine.m_breakpointsNum)
            {
                //Send the IP offset -1 for a catch handler in the case of an appdomain transition to mimic the desktop.
                g_CLR_RT_ExecutionEngine.Breakpoint_Exception( stack, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_EXCEPTION_HANDLER_FOUND, stack->m_IPstart - 1 );
                if(CLR_EE_DBG_IS(Stopped)) { goto ContinueAndExit; }
            }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

            TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
        }
        if (stack->m_flags & CLR_RT_StackFrame::c_PseudoStackFrameForFilter)
        {
            us.m_handlerStack = NULL;
            us.SetPhase( UnwindStack::p_2_RunningFinallys_0 );
            TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
        }

#ifndef TINYCLR_NO_IL_INLINE
        if(stack->m_inlineFrame != NULL && tmpInline.m_IP == NULL)
        {
            stack->SaveStack(tmpInline);
            stack->RestoreFromInlineStack();
        }
        else
        {
            if(tmpInline.m_IP)  
            {
                stack->RestoreStack(tmpInline);
                tmpInline.m_IP = NULL;
            }
#else
        {
#endif
            
            stack = stack->Caller();
        }
    }

    us.m_handlerStack = NULL;
    us.SetPhase(UnwindStack::p_2_RunningFinallys_0);

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    if(g_CLR_RT_ExecutionEngine.m_breakpointsNum)
    {
        g_CLR_RT_ExecutionEngine.Breakpoint_Exception_Uncaught( this );
        if(CLR_EE_DBG_IS(Stopped)) { goto ContinueAndExit; }
    }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    //We want to continue running EH "goo" code so leave m_currentException set and return PROCESS_EXCEPTION
    TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
ContinueAndExit:
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    //There are multiple cases where we want to break out of this function, send debug messages, and then resume exactly where we were.
    //All of those cases jump to here.
    //However, there are cases where we may be stopped but we don't want to set this flag (i.e. pushing on a filter and completing a stepper)
    //where we do not want to set this flag, so it cannot be in TinyCLR_Cleanup.
    us.m_flags         |= UnwindStack::c_ContinueExceptionHandler;
    TINYCLR_SET_AND_LEAVE(S_OK);

    TINYCLR_CLEANUP();

#ifndef TINYCLR_NO_IL_INLINE
    if(tmpInline.m_IP)
    {
        stack->RestoreStack(tmpInline);
    }    
#endif

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_Thread::ProcessException_Phase2()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    /* 
     * Start running through the stack frames, running all finally handlers and popping them off until
     * we hit our target catch handler, if any.
     */

    UnwindStack& us                 = m_nestedExceptions[ m_nestedExceptionsPos - 1 ];

    CLR_RT_StackFrame* iterStack    = CurrentFrame();
    
    CLR_RT_ExceptionHandler eh;

    //Unwind the stack, running finally's as we go
    while (iterStack->Caller() != NULL)
    {
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if (g_CLR_RT_ExecutionEngine.m_breakpointsNum && iterStack == us.m_handlerStack && (us.m_flags & UnwindStack::c_MagicCatchForInteceptedException) != 0)
        {
            //We've reached the frame we want to "handle" this exception. However, since the handler doesn't really exist, we want to remove the UnwindStack entry.
            m_nestedExceptionsPos--;
            iterStack->ResetStack();

            //We are willing to execute IL again so clear the m_currentException flag.
            m_currentException.SetObjectReference( NULL );

            //CPDE better reset the IP, or there are going to be issues.
            iterStack->m_flags |=  CLR_RT_StackFrame::c_InvalidIP;

            //Send the message to the debugger.
            g_CLR_RT_ExecutionEngine.Breakpoint_Exception_Intercepted( iterStack );

            //Return a success value to break out of ProcessException and to signal that execution of IL can continue.
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
        else
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(iterStack->m_call.m_target->flags & CLR_RECORD_METHODDEF::MD_HasExceptionHandlers)
        {
            if(iterStack->m_IP) // No IP? Either out of memory during allocation of iterStack frame or native method.
            {
                //handlerBlockStart is used to not execute finally's who's protected blocks contain the handler itself.
                //NULL is used when we're not in the handler stack frame to make it work in the case of recursive functions with filtered handlers.
                if(FindEhBlock( iterStack, iterStack->m_IP, (us.m_handlerStack == iterStack)? us.m_handlerBlockStart : NULL, eh, true ))
                {
                    //We have a finally block to process
                    
                    us.m_stack             = iterStack;
                    us.m_ip                = NULL;
                    us.m_currentBlockStart = eh.m_handlerStart;
                    us.m_currentBlockEnd   = eh.m_handlerEnd;
                    us.SetPhase(UnwindStack::p_2_RunningFinallys_0);

                    m_currentException.SetObjectReference( NULL ); // Reset exception flag.

                    iterStack->ResetStack();
                    iterStack->m_IP = eh.m_handlerStart;
                    iterStack->m_flags &=  ~CLR_RT_StackFrame::c_InvalidIP;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
#ifndef TINYCLR_NO_IL_INLINE
                    if(iterStack->m_inlineFrame == NULL)
#endif
                    {
                        g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Pop( iterStack, true );
                    }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

                    TINYCLR_SET_AND_LEAVE(S_OK);
                }

                if (iterStack == us.m_handlerStack)
                {
#ifndef TINYCLR_NO_IL_INLINE
                    if(iterStack->m_inlineFrame == NULL || 0 == (us.m_flags & UnwindStack::c_MagicCatchForInline))
#endif
                    {
                        //We've popped off all stack frames above the target.
                        //Now we should run the exception handler.

                        //Store the range of the block and the stack frame we're executing for PopEH
                        us.m_currentBlockStart = us.m_handlerBlockStart;
                        us.m_currentBlockEnd   = us.m_handlerBlockEnd;
                        us.m_stack             = us.m_handlerStack;
                        us.SetPhase( UnwindStack::p_3_RunningHandler );

                        //Set the IP and push the exception object on the stack.
                        iterStack->m_IP        = us.m_handlerBlockStart;
                        iterStack->m_flags    &= ~CLR_RT_StackFrame::c_InvalidIP;

                        iterStack->ResetStack();
                        iterStack->PushValue().SetObjectReference( us.m_exception );

                        //We are willing to execute IL again so clear the m_currentException flag.
                        m_currentException.SetObjectReference( NULL );

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
#ifndef TINYCLR_NO_IL_INLINE
                        if(iterStack->m_inlineFrame == NULL)
#endif
                        {
                            g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Pop( iterStack, true );
                        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

                        //Return a success value to break out of ProcessException and to signal that execution of IL can continue.
                        TINYCLR_SET_AND_LEAVE(S_OK);
                    }
                }
            }
        }

        //We didn't find a finally block at this level...
        //Check to see if we trickled up to a pseudoiterStack frame that we created to execute a filter handler:
        if (iterStack->m_flags & CLR_RT_StackFrame::c_PseudoStackFrameForFilter)
        {   
            //An exception was thrown while executing a filter block.
            //The CLR should swallow the current exception, perform some filter-cleanup, act as if the filter returned false and
            //continue looking for a handler for the old exception
            m_nestedExceptionsPos--;
           
            UnwindStack& us = m_nestedExceptions[ m_nestedExceptionsPos - 1 ];

            //Since there are no applicable handlers for this IP inside this filter block, and all finally's nested below the
            //filter have executed, we should pop off our pseudoframe and try to find another catch block.

            //Copy the arguments and locals back to the original stack frame.
            ProcessException_FilterPseudoFrameCopyVars( us.m_handlerStack, iterStack );

            //Set IP so we can resume looking for the next filter.
            us.m_ip = us.m_currentBlockStart;

            us.m_stack = NULL;  //Prevent Pop from taking this handler off the stack.

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            //We don't want to send any breakpoints until after we set the IP appropriately
            bool fBreakpointsDisabledSav = CLR_EE_DBG_IS(BreakpointsDisabled);
            CLR_EE_DBG_SET(BreakpointsDisabled);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

            iterStack->Pop();   //No finally's for the current ip in this method, pop to the next.

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            if(!fBreakpointsDisabledSav)
            {
                CLR_EE_DBG_CLR(BreakpointsDisabled);
            }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)            

            m_currentException.SetObjectReference( us.m_exception );    //Drop current exception, use old one.

            //Set the continue flag, and leave with S_OK to loop around and get Phase1 called again via ProcessException
            us.m_flags |= UnwindStack::c_ContinueExceptionHandler;

            //We are not ready to execute IL yet so do NOT clear m_currentException flag.
            //There still remains hope for this thread so return S_OK so ProcessException can get called again via Thread::Execute
            TINYCLR_SET_AND_LEAVE(S_OK);
        }

#if defined(TINYCLR_APPDOMAINS)        
        if(iterStack->m_flags & CLR_RT_StackFrame::c_AppDomainTransition)
        {
            //If we hit an AppDomain transition and haven't handled the exception, then a special case occurs.
            //Exception handling stops at this point and the whole process needs to start over in the caller's AppDomain.
            //We need to pop the handler off the unwind stack, pop the current stack frame, and then proceed to Phase1.
            m_nestedExceptionsPos--;    //Take off the pseudo-handler

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            bool fBreakpointsDisabledSav = CLR_EE_DBG_IS(BreakpointsDisabled);
            CLR_EE_DBG_SET(BreakpointsDisabled);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)            

#ifndef TINYCLR_NO_IL_INLINE
            if(iterStack->m_inlineFrame)
            {
                iterStack->PopInline();
            }
            else
#endif                
            {
                iterStack->Pop();   //No finally's for the current ip in this method, pop to the next.
            }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            if(!fBreakpointsDisabledSav)
            {
                CLR_EE_DBG_CLR(BreakpointsDisabled);
            }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)            

            //We are not ready to execute IL yet so do NOT clear m_currentException flag.
            //There still remains hope for this thread so return S_OK so ProcessException can get called again via Thread::Execute
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
#endif

        us.m_stack = NULL;  //Don't pop off the handler when we pop this stack frame


#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        //We don't want to send any breakpoints until after we set the IP appropriately
        bool fBreakpointsDisabledSav = CLR_EE_DBG_IS(BreakpointsDisabled);
        CLR_EE_DBG_SET(BreakpointsDisabled);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)        


#ifndef TINYCLR_NO_IL_INLINE
        if(iterStack->m_inlineFrame)
        {
            iterStack->PopInline();
        }
        else
#endif            
        {
            iterStack->Pop();   //No finally's for the current ip in this method, pop to the next.
        }
        iterStack = CurrentFrame();

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(!fBreakpointsDisabledSav)
        {
            CLR_EE_DBG_CLR(BreakpointsDisabled);
        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    }

    //If we reached this point, we've unwound the entire thread and have an unhandled exception.
    m_nestedExceptionsPos = 0;      //We have an unhandled exception, we might as well clean the unwind stack.

    //At this point, no hope remains.
    //m_currentException is still set, but we return PROCESS_EXCEPTION signalling that there is no hope for the thread,
    //which causes Thread::Execute to terminate it.
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
#if !defined(BUILD_RTM)
    //special case thread abort exception
    if((this->m_flags & CLR_RT_Thread::TH_F_Aborted) == 0)
    {
        CLR_Debug::Printf(" Uncaught exception \r\n" );
        //Perhaps some stronger notification is needed.  Consider CLR 2.0's fail-fast work
        //We could kill the application, and perhaps even store relevant data to dump to the user
        //when they connect to the PC.  Save the state so the debug API for the uncaught exception could be
        //retrieved?
    }
#endif //!BUILD_RTM
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_Thread::ProcessException()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    CLR_RT_StackFrame* stack = CurrentFrame();
    UnwindStack* us = NULL;

    // If the exception was thrown in the middle of an IL instruction,
    // back up the pointer to point to the executing instruction, not the next one.
    // Not an assert because the exception can be thrown by a native method.
    if(stack->m_flags & CLR_RT_StackFrame::c_ExecutingIL)
    {
        stack->m_IP--;
        stack->m_flags &= ~CLR_RT_StackFrame::c_ExecutingIL;
        stack->m_flags |=  CLR_RT_StackFrame::c_InvalidIP;
    }

    // If we are supposed to continue with the old exception handler, then pull the state from it rather than create a new exception.
    if (m_nestedExceptionsPos)
    {
        us = &m_nestedExceptions[ m_nestedExceptionsPos - 1 ];
        if (us->m_flags & UnwindStack::c_ContinueExceptionHandler)
        {
            //Reset the flag so we don't get false positives from new exceptions
            us->m_flags &= ~UnwindStack::c_ContinueExceptionHandler;
            m_currentException.SetObjectReference( us->m_exception );
        }
        else
        {
            //Signal that we need a new handler pushed on the stack.
            us = NULL;
        }
    }

    if (us == NULL)
    {
        //Push a new handler on the unwind stack that will last until its handler gets executed or
        //something out-of-band forces the stack frame to get popped.
        us = PushEH();

        //decent failure case is currently not implemented
        //a. Clearing the unwind stack and throwing a new exception is just asking for undefined behavior if this exception is caught
        //and the IP in stack frames somewhere below are in a finally due to an exception that hasn't yet ran a catch block and
        //endfinally gets executed. Execution would continue, thinking that nothing was wrong... a guranteed way to create hard-to-find bugs.

        //b. We could treat it as an unhandled exception, which would terminate the thread. It would be annoying, but it wouldn't lead to
        //unexpected code execution leading to potentially more exceptions.

        //c. We could forcibly pop stack frames until there is room on the unwind stack and then throw a stack overflow exception.
        //It gives a program a chance to recover, especially for 'always-on' type devices that are inconvenient for the user to perform a cold-boot.
        //At any restartable point in the program, there could be a try { } catch-all { } block inside a loop, causing the app to immediately restart
        //after an error that would normally terminate the thread.

        //A similar setup would be needed for leave, involving goto Execute_Restart to compensate for possible stack frame changes. Perhaps it could be
        //implemented directly in PushEh to reduce common code.

        us->m_exception = m_currentException.Dereference();
        us->m_stack     = stack;
        us->m_flags     = UnwindStack::p_1_SearchingForHandler_0;
    }
    
    if (us->GetPhase() <= UnwindStack::p_1_SearchingForHandler_2_SentUsersChance)
    {
        TINYCLR_EXIT_ON_SUCCESS(ProcessException_Phase1()); // Leave if we're executing a filter.
    }

    TINYCLR_SET_AND_LEAVE(ProcessException_Phase2()); //Leave if we're executing a finally or the catch block, or have an unhandled exception

    TINYCLR_NOCLEANUP();
}
