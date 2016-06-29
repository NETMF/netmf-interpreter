////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_StackFrame::Push( CLR_RT_Thread* th, const CLR_RT_MethodDef_Instance& callInst, CLR_INT32 extraBlocks )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_StackFrame*               stack;
    CLR_RT_StackFrame*               caller;
    CLR_RT_Assembly*                 assm;
    const CLR_RECORD_METHODDEF*      md;
    const CLR_RT_MethodDef_Instance* callInstPtr = &callInst;
    CLR_UINT32                       sizeLocals;
    CLR_UINT32                       sizeEvalStack;

#if defined(PLATFORM_WINDOWS_EMULATOR)
    if(s_CLR_RT_fTrace_SimulateSpeed > c_CLR_RT_Trace_None)
    {
        CLR_PROF_Handler::SuspendTime();

        HAL_Windows_FastSleep( g_HAL_Configuration_Windows.TicksPerMethodCall );

        CLR_PROF_Handler::ResumeTime();
    }
#endif

    assm          = callInstPtr->m_assm;
    md            = callInstPtr->m_target;

    sizeLocals    = md->numLocals;
#ifndef TINYCLR_NO_IL_INLINE
    sizeEvalStack = md->lengthEvalStack + CLR_RT_StackFrame::c_OverheadForNewObjOrInteropMethod + 1;
#else
    sizeEvalStack = md->lengthEvalStack + CLR_RT_StackFrame::c_OverheadForNewObjOrInteropMethod;
#endif

    //--//

    caller = th->CurrentFrame();

    //--//

    //
    // Allocate memory for the runtime state.
    //
    {
        CLR_UINT32 memorySize = sizeLocals + sizeEvalStack;

        if(extraBlocks > 0             ) memorySize += extraBlocks;
#ifndef TINYCLR_NO_IL_INLINE
        if(memorySize  < c_MinimumStack)
        {
            sizeEvalStack += c_MinimumStack - memorySize;
            memorySize     = c_MinimumStack;
        }
#else
        if(memorySize  < c_MinimumStack) memorySize  = c_MinimumStack;
#endif

        memorySize += CONVERTFROMSIZETOHEAPBLOCKS(offsetof(CLR_RT_StackFrame,m_extension));

        stack = EVENTCACHE_EXTRACT_NODE_AS_BLOCKS(g_CLR_RT_EventCache,CLR_RT_StackFrame,DATATYPE_STACK_FRAME,0,memorySize); CHECK_ALLOCATION(stack);
    }

    //--//

    {                                                            //
        stack->m_owningSubThread = th->CurrentSubThread();  // CLR_RT_SubThread*         m_owningSubThread;  // EVENT HEAP - NO RELOCATION -
        stack->m_owningThread    = th;                      // CLR_RT_Thread*            m_owningThread;     // EVENT HEAP - NO RELOCATION -
                                                            // CLR_UINT32                m_flags;
                                                            //
        stack->m_call            = *callInstPtr;            // CLR_RT_MethodDef_Instance m_call;
                                                            //
                                                            // CLR_RT_MethodHandler      m_nativeMethod;
                                                            // CLR_PMETADATA             m_IPstart;          // ANY   HEAP - DO RELOCATION -
                                                            // CLR_PMETADATA             m_IP;               // ANY   HEAP - DO RELOCATION -
                                                            //
        stack->m_locals          = stack->m_extension;      // CLR_RT_HeapBlock*         m_locals;           // EVENT HEAP - NO RELOCATION -
        stack->m_evalStack       = stack->m_extension + sizeLocals;                      // CLR_RT_HeapBlock*         m_evalStack;        // EVENT HEAP - NO RELOCATION -
        stack->m_evalStackPos    = stack->m_evalStack;      // CLR_RT_HeapBlock*         m_evalStackPos;     // EVENT HEAP - NO RELOCATION -
        stack->m_evalStackEnd    = stack->m_evalStack + sizeEvalStack;                      // CLR_RT_HeapBlock*         m_evalStackEnd;     // EVENT HEAP - NO RELOCATION -
        stack->m_arguments       = NULL;                    // CLR_RT_HeapBlock*         m_arguments;        // EVENT HEAP - NO RELOCATION -
                                                            //
                                                            // union
                                                            // {
        stack->m_customState     = 0;                       //    CLR_UINT32             m_customState;
                                                            //    void*                  m_customPointer;
                                                            // };
                                                            //
#ifndef TINYCLR_NO_IL_INLINE
        stack->m_inlineFrame     = NULL;
#endif
#if defined(TINYCLR_PROFILE_NEW_CALLS)
        stack->m_callchain.Enter( stack );                  // CLR_PROF_CounterCallChain m_callchain;
#endif
                                                            //
                                                            // CLR_RT_HeapBlock          m_extension[1];
                                                            //
#if defined(ENABLE_NATIVE_PROFILER)
        stack->m_fNativeProfiled = stack->m_owningThread->m_fNativeProfiled;
#endif
        CLR_RT_MethodHandler impl;

#if defined(TINYCLR_APPDOMAINS)        
        stack->m_appDomain = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();
#endif

        if(md->flags & CLR_RECORD_METHODDEF::MD_DelegateInvoke) // Special case for delegate calls.
        {
            stack->m_nativeMethod = (CLR_RT_MethodHandler)CLR_RT_Thread::Execute_DelegateInvoke;

            stack->m_flags   = CLR_RT_StackFrame::c_MethodKind_Native;
            stack->m_IPstart = NULL;
        }
        else if(assm->m_nativeCode && (impl = assm->m_nativeCode[ stack->m_call.Method() ]) != NULL)
        {
            stack->m_nativeMethod = impl;

            stack->m_flags   = CLR_RT_StackFrame::c_MethodKind_Native;
            stack->m_IPstart = NULL;
            stack->m_IP      = NULL;
        }
#if defined(TINYCLR_JITTER)
        else if(assm->m_jittedCode && (impl = assm->m_jittedCode[ stack->m_call.Method() ]) != NULL)
        {
            stack->m_nativeMethod = (CLR_RT_MethodHandler)(size_t)g_thunkTable.m_address__Internal_Initialize;

            stack->m_flags   = CLR_RT_StackFrame::c_MethodKind_Jitted;
            stack->m_IPstart = (CLR_PMETADATA)impl;

            if(md->flags & CLR_RECORD_METHODDEF::MD_HasExceptionHandlers)
            {
                CLR_UINT32 numEh = *(CLR_UINT32*)stack->m_IPstart;

                stack->m_IP = stack->m_IPstart + sizeof(CLR_UINT32) + numEh * sizeof(CLR_RT_ExceptionHandler);
            }
            else
            {
                stack->m_IP = stack->m_IPstart;
            }
        }
#endif
        else
        {
            stack->m_nativeMethod = (CLR_RT_MethodHandler)CLR_RT_Thread::Execute_IL;

            if(md->RVA == CLR_EmptyIndex) TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);

            stack->m_flags   = CLR_RT_StackFrame::c_MethodKind_Interpreted;
            stack->m_IPstart = assm->GetByteCode( md->RVA );
            stack->m_IP      = stack->m_IPstart;
        }

        #if defined(ENABLE_NATIVE_PROFILER)
        if(stack->m_owningThread->m_fNativeProfiled == false && md->flags & CLR_RECORD_METHODDEF::MD_NativeProfiled)
        {
            stack->m_flags |= CLR_RT_StackFrame::c_NativeProfiled;
            stack->m_owningThread->m_fNativeProfiled = true;
        }
        #endif

        //--//

        th->m_stackFrames.LinkAtBack( stack );

#if defined(TINYCLR_PROFILE_NEW_CALLS)
        g_CLR_PRF_Profiler.RecordFunctionCall( th, callInst );
#endif
    }

    if(md->numLocals)
    {        
        g_CLR_RT_ExecutionEngine.InitializeLocals( stack->m_locals, assm, md );
    }

    {
        CLR_UINT32 flags = md->flags & (md->MD_Synchronized | md->MD_GloballySynchronized);

        if(flags)
        {
            if(flags & md->MD_Synchronized        ) stack->m_flags |= c_NeedToSynchronize;
            if(flags & md->MD_GloballySynchronized) stack->m_flags |= c_NeedToSynchronizeGlobally;
        }
    }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    stack->m_depth = stack->Caller()->Prev() ? stack->Caller()->m_depth + 1 : 0;

    if(g_CLR_RT_ExecutionEngine.m_breakpointsNum)
    {
        if(stack->m_call.DebuggingInfo().HasBreakpoint())
        {
            stack->m_flags |= CLR_RT_StackFrame::c_HasBreakpoint;
        }

        if(stack->m_owningThread->m_fHasJMCStepper || (stack->m_flags & c_HasBreakpoint) || (caller->Prev() != NULL && (caller->m_flags & c_HasBreakpoint)))
        {
            g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Push( stack, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_STEP_CALL );
        }
    }
#endif

    //--//

#if defined(TINYCLR_JITTER)
    if(s_CLR_RT_fJitter_Enabled && (stack->m_flags & CLR_RT_StackFrame::c_MethodKind_Mask) == CLR_RT_StackFrame::c_MethodKind_Interpreted)
    {
        CLR_RT_ExecutionEngine::ExecutionConstraint_Suspend();

        g_CLR_RT_ExecutionEngine.Compile( stack->m_call, CLR_RT_ExecutionEngine::c_Compile_ARM );

        CLR_RT_ExecutionEngine::ExecutionConstraint_Resume();

        if(assm->m_jittedCode)
        {
            CLR_PMETADATA ipStart = (CLR_PMETADATA)assm->m_jittedCode[ stack->m_call.Method() ];

            if(ipStart != NULL)
            {
                stack->m_nativeMethod = (CLR_RT_MethodHandler)(size_t)g_thunkTable.m_address__Internal_Initialize;

                stack->m_IPstart = ipStart;

                if(md->flags & CLR_RECORD_METHODDEF::MD_HasExceptionHandlers)
                {
                    CLR_UINT32 numEh = *(CLR_UINT32*)ipStart;

                    stack->m_IP = ipStart + sizeof(CLR_UINT32) + numEh * sizeof(CLR_RT_ExceptionHandler);
                }
                else
                {
                    stack->m_IP = ipStart;
                }

                stack->m_flags &= ~CLR_RT_StackFrame::c_MethodKind_Mask;
                stack->m_flags |=  CLR_RT_StackFrame::c_MethodKind_Jitted;
            }
        }
    }
#endif

    if(caller->Prev() != NULL && caller->m_nativeMethod == stack->m_nativeMethod)
    {
        if(stack->m_flags & CLR_RT_StackFrame::c_ProcessSynchronize)
        {
            stack->m_flags |= CLR_RT_StackFrame::c_CallerIsCompatibleForRet;
        }
        else
        {
            stack->m_flags |= CLR_RT_StackFrame::c_CallerIsCompatibleForCall | CLR_RT_StackFrame::c_CallerIsCompatibleForRet;
        }        
    }

    //
    // If the arguments are in the caller's stack frame (var == 0), let's link the two.
    //
    if(extraBlocks < 0)
    {
#if defined(_WIN32) || (defined(PLATFORM_WINCE) && defined(_DEBUG))
        if(caller->m_evalStackPos > caller->m_evalStackEnd)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_STACK_OVERFLOW);
        }
#endif

        //
        // Everything is set up correctly, pop the operands.
        //
        stack->m_arguments = &caller->m_evalStackPos[ -md->numArgs ];

        caller->m_evalStackPos = stack->m_arguments;

#if defined(_WIN32) || (defined(PLATFORM_WINCE) && defined(_DEBUG))
        if(stack->m_arguments < caller->m_evalStack)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_STACK_UNDERFLOW);
        }
#endif
    }
    else
    {
        stack->m_arguments = stack->m_evalStackEnd;
    }

    TINYCLR_CLEANUP();


    TINYCLR_CLEANUP_END();
}

#ifndef TINYCLR_NO_IL_INLINE
bool CLR_RT_StackFrame::PushInline( CLR_PMETADATA& ip, CLR_RT_Assembly*& assm, CLR_RT_HeapBlock*& evalPos, CLR_RT_MethodDef_Instance& calleeInst, CLR_RT_HeapBlock* pThis)
{
    const CLR_RECORD_METHODDEF* md =  calleeInst.m_target;

    if( (m_inlineFrame != NULL) ||                                                                                      // We can only support one inline at a time per stack call
        (m_evalStackEnd - evalPos) <= (md->numArgs + md->numLocals + md->lengthEvalStack + 2) ||                       // We must have enough space on the current stack for the inline method
        (m_nativeMethod != (CLR_RT_MethodHandler)CLR_RT_Thread::Execute_IL) ||                                          // We only support IL inlining
        (md->flags & ~CLR_RECORD_METHODDEF::MD_HasExceptionHandlers) >= CLR_RECORD_METHODDEF::MD_Constructor ||         // Do not try to inline constructors, etc because they require special processing
        (0 != (md->flags & CLR_RECORD_METHODDEF::MD_Static)) ||                                                         // Static methods also requires special processing
        (calleeInst.m_assm->m_nativeCode != NULL && (calleeInst.m_assm->m_nativeCode[ calleeInst.Method() ] != NULL)) || // Make sure the callee is not an internal method
        (md->RVA == CLR_EmptyIndex) ||                                                                                   // Make sure we have a valid IP address for the method
        !g_CLR_RT_EventCache.GetInlineFrameBuffer(&m_inlineFrame))                                                       // Make sure we have an extra slot in the inline cache
    {
        return false;
    }
    
    CLR_PMETADATA ipTmp = calleeInst.m_assm->GetByteCode( md->RVA );

#if defined(PLATFORM_WINDOWS_EMULATOR)
        if(s_CLR_RT_fTrace_SimulateSpeed > c_CLR_RT_Trace_None)
        {
            CLR_PROF_Handler::SuspendTime();
    
            HAL_Windows_FastSleep( g_HAL_Configuration_Windows.TicksPerMethodCall );
    
            CLR_PROF_Handler::ResumeTime();
        }
#endif

    // make backup
    m_inlineFrame->m_frame.m_IP        = ip;
    m_inlineFrame->m_frame.m_IPStart   = m_IPstart;
    m_inlineFrame->m_frame.m_locals    = m_locals;
    m_inlineFrame->m_frame.m_args      = m_arguments;
    m_inlineFrame->m_frame.m_call      = m_call;
    m_inlineFrame->m_frame.m_evalStack = m_evalStack;
    m_inlineFrame->m_frame.m_evalPos   = pThis;

    // increment the evalPos pointer so that we don't corrupt the real stack
    evalPos++;
    assm           = calleeInst.m_assm;
    ip             = ipTmp;
    
    m_arguments    = pThis;     
    m_locals       = &m_evalStackEnd[-md->numLocals];
    m_call         = calleeInst;
    m_evalStackEnd = m_locals;
    m_evalStack    = evalPos;
    m_evalStackPos = evalPos + 1;
    m_IPstart      = ip;
    m_IP           = ip;

    if(md->numLocals)
    {        
        g_CLR_RT_ExecutionEngine.InitializeLocals( m_locals, calleeInst.m_assm, md );
    }

    m_flags |= CLR_RT_StackFrame::c_MethodKind_Inlined;

    if(md->retVal != DATATYPE_VOID)
    {
        m_flags |= CLR_RT_StackFrame::c_InlineMethodHasReturnValue;
    }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    m_depth++;

    if(g_CLR_RT_ExecutionEngine.m_breakpointsNum)
    {
        if(m_call.DebuggingInfo().HasBreakpoint())
        {
            m_flags |= CLR_RT_StackFrame::c_HasBreakpoint;
        }

        if(m_owningThread->m_fHasJMCStepper || (m_flags & CLR_RT_StackFrame::c_HasBreakpoint))
        {
            g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Push( this, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_STEP_CALL );
        }
    }
#endif

    return true;
}

void CLR_RT_StackFrame::PopInline()
{

    CLR_RT_HeapBlock& src = m_evalStackPos[0];

    RestoreFromInlineStack();
            
    if(m_flags & CLR_RT_StackFrame::c_InlineMethodHasReturnValue)
    {
        if(m_owningThread->m_currentException.Dereference() == NULL)
        {
            CLR_RT_HeapBlock& dst = PushValueAndAssign( src );
            
            dst.Promote();       
        }
    }

    g_CLR_RT_EventCache.FreeInlineBuffer(m_inlineFrame);
    m_inlineFrame = NULL;
    m_flags &= ~(CLR_RT_StackFrame::c_MethodKind_Inlined | CLR_RT_StackFrame::c_InlineMethodHasReturnValue);

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    if(m_owningThread->m_fHasJMCStepper || (m_flags & CLR_RT_StackFrame::c_HasBreakpoint))
    {
        g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Pop( this, false );
    }
    m_depth--;
#endif    
}

void CLR_RT_StackFrame::RestoreFromInlineStack() 
{ 
    m_arguments     = m_inlineFrame->m_frame.m_args;
    m_locals        = m_inlineFrame->m_frame.m_locals;
    m_evalStackEnd += m_call.m_target->numLocals;
    m_call          = m_inlineFrame->m_frame.m_call;
    m_IP            = m_inlineFrame->m_frame.m_IP;
    m_IPstart       = m_inlineFrame->m_frame.m_IPStart;
    m_evalStack     = m_inlineFrame->m_frame.m_evalStack;
    m_evalStackPos  = m_inlineFrame->m_frame.m_evalPos;
}

void CLR_RT_StackFrame::RestoreStack(CLR_RT_InlineFrame& frame)
{
    m_arguments     = frame.m_args;
    m_locals        = frame.m_locals;
    m_call          = frame.m_call;
    m_IP            = frame.m_IP;
    m_IPstart       = frame.m_IPStart;
    m_evalStack     = frame.m_evalStack;
    m_evalStackPos  = frame.m_evalPos;
    m_evalStackEnd -= m_call.m_target->numLocals;    
}

void CLR_RT_StackFrame::SaveStack(CLR_RT_InlineFrame& frame)
{
    frame.m_args      = m_arguments;
    frame.m_locals    = m_locals;
    frame.m_call      = m_call;
    frame.m_IP        = m_IP;
    frame.m_IPStart   = m_IPstart;
    frame.m_evalPos   = m_evalStackPos;
    frame.m_evalStack = m_evalStack;
}
#endif

#if defined(TINYCLR_APPDOMAINS)
HRESULT CLR_RT_StackFrame::PopAppDomainTransition()
{            
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    bool fException = false;
    CLR_RT_HeapBlock exception;
    CLR_RT_StackFrame* caller = this->Caller();
    
    exception.SetObjectReference( NULL );
            
    if(m_flags & CLR_RT_StackFrame::c_AppDomainInjectException)
    {    
        //this is the last frame on the thread in a doomed AppDomain
        //Convert the current ThreadAbortException into an AppDomainUnloaded exception

        _ASSERTE(m_owningThread->m_flags & CLR_RT_Thread::TH_F_Aborted);
        _ASSERTE(m_owningThread->m_flags & CLR_RT_Thread::TH_F_ContainsDoomedAppDomain);
        _ASSERTE(m_owningThread->m_currentException.Dereference() != NULL);                        
        _ASSERTE(m_owningThread->m_currentException.Dereference()->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_ThreadAbortException.m_data);                        
        _ASSERTE(!m_appDomain->IsLoaded());

        m_owningThread->m_flags &= ~(CLR_RT_Thread::TH_F_Aborted | CLR_RT_Thread::TH_F_ContainsDoomedAppDomain);
                                                                
        hr = CLR_E_APPDOMAIN_EXITED;
    }
    else if(m_owningThread->m_currentException.Dereference() == NULL)
    {                                    
        _ASSERTE((m_flags & CLR_RT_StackFrame::c_AppDomainInjectException) == 0);

        //Normal return.  No exception is in flight

        if(m_flags & CLR_RT_StackFrame::c_AppDomainMethodInvoke)
        {                        
            //  For dynamic invoke, 
            //  we do not marshal byRef parameters back to the calling AppDomain
            //  The caller is a native method (MethodBase::Invoke), and does not have the args on it's eval stack.
        }
        else
        {            
            int cArgs = m_call.m_target->numArgs;

            //First marshal the ref parameters                            
            TINYCLR_CHECK_HRESULT(caller->m_appDomain->MarshalParameters( &caller->m_evalStackPos[ -cArgs ], m_arguments, cArgs, true ));        
                
            //Now, pop the caller's arguments off the eval stack                
            caller->m_evalStackPos -= cArgs;
        }

        // Now, push the return, if any.
        if(m_call.m_target->retVal != DATATYPE_VOID)
        {
            CLR_RT_HeapBlock& dst = caller->PushValueAndClear();
            CLR_RT_HeapBlock& src = this  ->TopValue ();          
            
            TINYCLR_CHECK_HRESULT(caller->m_appDomain->MarshalObject( src, dst ));
                                                
            dst.Promote();                    
        }
    }                
    else //Exception
    {        
        //Normal exceptions must be marshaled to the caller's AppDomain
        TINYCLR_CHECK_HRESULT(caller->m_appDomain->MarshalObject( m_owningThread->m_currentException, exception ));            
        fException = true;        
    }
    
    TINYCLR_CLEANUP();

    if(FAILED(hr) || fException)
    {
        if(FAILED(hr))
        {
            (void)Library_corlib_native_System_Exception::CreateInstance( exception, hr, caller );
        }

        m_owningThread->m_currentException.Assign( exception );
    }

    (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( caller->m_appDomain );   

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_StackFrame::PushAppDomainTransition( CLR_RT_Thread* th, const CLR_RT_MethodDef_Instance& callInst, CLR_RT_HeapBlock* pThis, CLR_RT_HeapBlock* pArgs )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_StackFrame* frame = NULL;
    int cArgs = callInst.m_target->numArgs;
    CLR_RT_HeapBlock* proxy;

    _ASSERTE(pThis->IsTransparentProxy());

    proxy = pThis->Dereference();
    
    TINYCLR_CHECK_HRESULT(proxy->TransparentProxyValidate());
    
    TINYCLR_CHECK_HRESULT(Push( th, callInst, cArgs ));
    
    frame  = th->CurrentFrame();    
    
    frame->m_appDomain  = proxy->TransparentProxyAppDomain();
    frame->m_flags     |= CLR_RT_StackFrame::c_AppDomainTransition;
    frame->m_flags     &= ~CLR_RT_StackFrame::c_CallerIsCompatibleForRet;   

    //Marshal the arguments from the caller (on the eval stack) to the callee, unitialized heapblocks that
    //are set up by the extra blocks in CLR_RT_StackFrame::Push
    TINYCLR_CHECK_HRESULT(frame->m_appDomain->MarshalObject    ( *pThis,  frame->m_arguments[ 0 ]                 ));
    TINYCLR_CHECK_HRESULT(frame->m_appDomain->MarshalParameters(  pArgs, &frame->m_arguments[ 1 ], cArgs-1, false ));

    (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( frame->m_appDomain );

    TINYCLR_NOCLEANUP();    
}

#endif //TINYCLR_APPDOMAINS

HRESULT CLR_RT_StackFrame::MakeCall( CLR_RT_MethodDef_Instance md, CLR_RT_HeapBlock* obj, CLR_RT_HeapBlock* args, int nArgs )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    const CLR_RECORD_METHODDEF* mdR        = md.m_target;
    bool                        fStatic    =(mdR->flags & CLR_RECORD_METHODDEF::MD_Static) != 0;
    int                         numArgs    = mdR->numArgs;
    int                         argsOffset = 0;
    CLR_RT_StackFrame*          stackSub;    
    CLR_RT_HeapBlock            tmp; tmp.SetObjectReference( NULL );
    CLR_RT_ProtectFromGC        gc(tmp);

    if(mdR->flags & CLR_RECORD_METHODDEF::MD_Constructor)
    {                    
        CLR_RT_TypeDef_Instance owner; owner.InitializeFromMethod( md );

        _ASSERTE(obj == NULL);
            
        _SIDE_ASSERTE(owner.InitializeFromMethod( md ));

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObject( tmp, owner ));
        
        obj = &tmp;
                
        //
        // Make a copy of the object pointer.
        //
        PushValueAndAssign( tmp );
    }

    if(!fStatic)
    {
        FAULT_ON_NULL(obj);
        numArgs--;
        argsOffset = 1;
    }

    if(numArgs != nArgs) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);    
           
    //
    // In case the invoked method is abstract or virtual, resolve it to the correct method implementation.
    //
    if(mdR->flags & (CLR_RECORD_METHODDEF::MD_Abstract | CLR_RECORD_METHODDEF::MD_Virtual))
    {
        CLR_RT_TypeDef_Index   cls;
        CLR_RT_MethodDef_Index mdReal;

        _ASSERTE(obj);
        _ASSERTE(!fStatic);

        TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( *obj, cls ));

        if(g_CLR_RT_EventCache.FindVirtualMethod( cls, md, mdReal ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }

        md.InitializeFromIndex( mdReal );

        mdR = md.m_target;
    }

#if defined(TINYCLR_APPDOMAINS)
                
    if(!fStatic && obj->IsTransparentProxy())
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_StackFrame::PushAppDomainTransition( m_owningThread, md, obj, args ));

        stackSub = m_owningThread->CurrentFrame();
        
        stackSub->m_flags |= CLR_RT_StackFrame::c_AppDomainMethodInvoke;
    }
    else
#endif
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_StackFrame::Push( m_owningThread, md, md.m_target->numArgs ));

        stackSub = m_owningThread->CurrentFrame();

        if(!fStatic)
        {
            stackSub->m_arguments[ 0 ].Assign( *obj );
        }

        if(numArgs)
        {
            memcpy( &stackSub->m_arguments[ argsOffset ], args, sizeof(CLR_RT_HeapBlock) * numArgs );
        }
    }

    TINYCLR_CHECK_HRESULT(stackSub->FixCall());

    TINYCLR_SET_AND_LEAVE(CLR_E_RESTART_EXECUTION);

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_StackFrame::FixCall()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const CLR_RECORD_METHODDEF* target  = m_call.m_target;
    CLR_UINT8                   numArgs = target->numArgs;

    //
    // The copy of ValueTypes is delayed as much as possible.
    //
    // If an argument is a ValueType, now it's a good time to clone it.
    //
    if(numArgs)
    {
        CLR_RT_SignatureParser          parser; parser.Initialize_MethodSignature( m_call.m_assm, target );
        CLR_RT_SignatureParser::Element res;
        CLR_RT_HeapBlock*               args = m_arguments;

        if(parser.m_flags & PIMAGE_CEE_CS_CALLCONV_HASTHIS)
        {
            args++;
        }

        //
        // Skip return value.
        //
        TINYCLR_CHECK_HRESULT(parser.Advance( res ));

        for(;parser.Available() > 0;args++)
        {
            TINYCLR_CHECK_HRESULT(parser.Advance( res ));

            if(res.m_levels > 0) continue; // Array, no need to fix.

            if(args->DataType() == DATATYPE_OBJECT)
            {
                CLR_RT_TypeDef_Instance      inst;               inst.InitializeFromIndex( res.m_cls );
                CLR_DataType                 dtT = (CLR_DataType)inst.m_target->dataType;
                const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ dtT ];

                if(dtl.m_flags & (CLR_RT_DataTypeLookup::c_OptimizedValueType | CLR_RT_DataTypeLookup::c_ValueType))
                {
                    CLR_RT_HeapBlock* value = args->FixBoxingReference(); FAULT_ON_NULL(value);

                    if(value->DataType() == dtT)
                    {
                        // It's a boxed primitive/enum type.
                        args->Assign( *value );
                    }
                    else if(args->Dereference()->ObjectCls().m_data == res.m_cls.m_data)
                    {
                        TINYCLR_CHECK_HRESULT(args->PerformUnboxing( inst ));
                    }
                    else
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                    }
                }
            }

            if(res.m_dt == DATATYPE_VALUETYPE && res.m_fByRef == false)
            {
                if(args->IsAReferenceOfThisType( DATATYPE_VALUETYPE ))
                {
                    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.CloneObject( *args, *args ));
                }
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_StackFrame::HandleSynchronized( bool fAcquire, bool fGlobal )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock   refType;
    CLR_RT_HeapBlock*  obj;
    CLR_RT_HeapBlock   ref;
    CLR_RT_HeapBlock** ppGlobalLock;
    CLR_RT_HeapBlock*  pGlobalLock;

    if(fGlobal)
    {
        obj = &ref;

#if defined(TINYCLR_APPDOMAINS)
        //With AppDomains enabled, the global lock is no longer global.  It is only global wrt the AppDomain/
        //Do we need a GlobalGlobalLock? (an attribute on GloballySynchronized (GlobalAcrossAppDomain?)
        ppGlobalLock = &g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->m_globalLock;
#else 
        ppGlobalLock = &g_CLR_RT_ExecutionEngine.m_globalLock;
#endif

        pGlobalLock = *ppGlobalLock;

        if(pGlobalLock)
        {
            obj->SetObjectReference( pGlobalLock );
        }
        else
        {
            //
            // Create an private object to implement global locks.
            //
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *obj, g_CLR_RT_WellKnownTypes.m_Object ));

            *ppGlobalLock = obj->Dereference();
        }
    }
    else if(m_call.m_target->flags & CLR_RECORD_METHODDEF::MD_Static)
    {
        CLR_RT_TypeDef_Index idx;

        idx.Set( m_call.Assembly(), m_call.CrossReference().GetOwner() );

        refType.SetReflection( idx );

        obj = &refType;
    }
    else
    {
        obj = &Arg0();
    }

    if(fAcquire)
    {
        TINYCLR_SET_AND_LEAVE(g_CLR_RT_ExecutionEngine.LockObject( *obj, m_owningSubThread, TIMEOUT_INFINITE, false ));
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(g_CLR_RT_ExecutionEngine.UnlockObject( *obj, m_owningSubThread ));
    }

    TINYCLR_NOCLEANUP();
}

void CLR_RT_StackFrame::Pop()
{
    NATIVE_PROFILE_CLR_CORE();

#if defined(TINYCLR_PROFILE_NEW_CALLS)
    {
        //
        // This passivates any outstanding handler.
        //
        CLR_PROF_HANDLER_CALLCHAIN(pm2,m_callchain);

        m_callchain.Leave();
    }
#endif

#if defined(TINYCLR_PROFILE_NEW_CALLS)
    g_CLR_PRF_Profiler.RecordFunctionReturn( m_owningThread, m_callchain );
#endif

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    if(m_owningThread->m_fHasJMCStepper || (m_flags & c_HasBreakpoint))
    {
        g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Pop( this, false );
    }
#endif
    
    const CLR_UINT32 c_flagsToCheck = CLR_RT_StackFrame::c_CallOnPop | CLR_RT_StackFrame::c_Synchronized | CLR_RT_StackFrame::c_SynchronizedGlobally | CLR_RT_StackFrame::c_NativeProfiled;

    if(m_flags & c_flagsToCheck)
    {
        if(m_flags & CLR_RT_StackFrame::c_CallOnPop)
        {
            m_flags |= CLR_RT_StackFrame::c_CalledOnPop;

            if(m_nativeMethod)
            {
                (void)m_nativeMethod( *this );
            }
        }

        if(m_flags & CLR_RT_StackFrame::c_Synchronized)
        {
            m_flags &= ~CLR_RT_StackFrame::c_Synchronized;

            (void)HandleSynchronized( false, false );
        }

        if(m_flags & CLR_RT_StackFrame::c_SynchronizedGlobally)
        {
            m_flags &= ~CLR_RT_StackFrame::c_SynchronizedGlobally;

            (void)HandleSynchronized( false, true );
        }

        #if defined(ENABLE_NATIVE_PROFILER)
        if(m_flags & CLR_RT_StackFrame::c_NativeProfiled)
        {
            m_owningThread->m_fNativeProfiled = false;
            m_flags &= ~CLR_RT_StackFrame::c_NativeProfiled;
            Native_Profiler_Stop();
        }
        #endif
    }

    CLR_RT_StackFrame* caller = Caller();

    if(caller->Prev() != NULL)
    {
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(caller->m_flags & CLR_RT_StackFrame::c_HasBreakpoint)
        {
            g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Step( caller, caller->m_IP );
        }
#endif

        //
        // Constructors are slightly different, they push the 'this' pointer back into the caller stack.
        //
        // This is to enable the special case for strings, where the object can be recreated by the constructor...
        //
        if(caller->m_flags & CLR_RT_StackFrame::c_ExecutingConstructor)
        {
            CLR_RT_HeapBlock& src = this  ->Arg0              (     );
            CLR_RT_HeapBlock& dst = caller->PushValueAndAssign( src );

            dst.Promote();

            //
            // Undo the special "object -> reference" hack done by CEE_NEWOBJ.
            //
            if(dst.DataType() == DATATYPE_BYREF)
            {
                dst.ChangeDataType( DATATYPE_OBJECT );
            }

            caller->m_flags &= ~CLR_RT_StackFrame::c_ExecutingConstructor;

            _ASSERTE((m_flags & CLR_RT_StackFrame::c_AppDomainTransition) == 0);
        }
        else
        {   //Note that ExecutingConstructor is checked on 'caller', whereas the other two flags are checked on 'this'
            const CLR_UINT32 c_moreFlagsToCheck = CLR_RT_StackFrame::c_PseudoStackFrameForFilter | CLR_RT_StackFrame::c_AppDomainTransition;

            if(m_flags & c_moreFlagsToCheck)
            {
                if(m_flags & CLR_RT_StackFrame::c_PseudoStackFrameForFilter)
                {
                    //Do nothing here. Pushing return values onto stack frames that don't expect them are a bad idea.
                }
#if defined(TINYCLR_APPDOMAINS)
                else if((m_flags & CLR_RT_StackFrame::c_AppDomainTransition) != 0)
                {   
                    (void)PopAppDomainTransition();             
                }
#endif
            }
            else //!c_moreFlagsToCheck
            {
                //
                // Push the return, if any.
                //
                if(m_call.m_target->retVal != DATATYPE_VOID)
                {
                    if(m_owningThread->m_currentException.Dereference() == NULL)
                    {
                        CLR_RT_HeapBlock& src = this  ->TopValue          (     );
                        CLR_RT_HeapBlock& dst = caller->PushValueAndAssign( src );
                        
                        dst.Promote();       
                    }
                }
            }
        }
    }
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    else
    {
        int idx = m_owningThread->m_scratchPad;

        if(idx >= 0)
        {
            CLR_RT_HeapBlock_Array* array = g_CLR_RT_ExecutionEngine.m_scratchPadArray;

            if(array && array->m_numOfElements > (CLR_UINT32)idx)
            {
                CLR_RT_HeapBlock* dst       = (CLR_RT_HeapBlock*)array->GetElement( (CLR_UINT32)idx );
                CLR_RT_HeapBlock* exception = m_owningThread->m_currentException.Dereference();

                dst->SetObjectReference( NULL );

                if(exception != NULL)
                {
                    dst->SetObjectReference( exception );
                }
                else if(m_call.m_target->retVal != DATATYPE_VOID)
                {
                    CLR_RT_SignatureParser sig; sig.Initialize_MethodSignature( this->m_call.m_assm, this->m_call.m_target );
                    CLR_RT_SignatureParser::Element res;
                    CLR_RT_TypeDescriptor           desc;

                    dst->Assign( this->TopValue() );

                    //Perform boxing, if needed.

                    //Box to the return value type
                    _SIDE_ASSERTE(SUCCEEDED(sig.Advance( res )));
                    _SIDE_ASSERTE(SUCCEEDED(desc.InitializeFromType( res.m_cls )));
                    
                   
                    if(c_CLR_RT_DataTypeLookup[ this->DataType() ].m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType
                        || desc.m_handlerCls.m_target->IsEnum() 
                      )                            
                    {
                         if(FAILED(dst->PerformBoxing( desc.m_handlerCls )))
                         {
                            dst->SetObjectReference( NULL );
                         }
                    }
                }
            }
        }
    }
#endif

    //
    // We could be jumping outside of a nested exception handler.
    //

    m_owningThread->PopEH( this, NULL );


    //
    // If this StackFrame owns a SubThread, kill it.
    //
    {
        CLR_RT_SubThread* sth = (CLR_RT_SubThread*)m_owningSubThread->Next();

        if(sth->Next() && sth->m_owningStackFrame == this)
        {
            CLR_RT_SubThread::DestroyInstance( sth->m_owningThread, sth, CLR_RT_SubThread::MODE_IncludeSelf );
        }
    }
    
    g_CLR_RT_EventCache.Append_Node( this );
}

//--//

void CLR_RT_StackFrame::SetResult( CLR_INT32 val, CLR_DataType dataType )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetInteger( val, dataType );
}

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)

void CLR_RT_StackFrame::SetResult_R4( float val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetFloat( val );
}

void CLR_RT_StackFrame::SetResult_R8( double val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetDouble( val );
}

#else

void CLR_RT_StackFrame::SetResult_R4( CLR_INT32 val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetFloat( val );
}

void CLR_RT_StackFrame::SetResult_R8( CLR_INT64 val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetDouble( val );
}

#endif

void CLR_RT_StackFrame::SetResult_I4( CLR_INT32 val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetInteger( val );
}

void CLR_RT_StackFrame::SetResult_I8( CLR_INT64& val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetInteger( val );
}

void CLR_RT_StackFrame::SetResult_U4( CLR_UINT32  val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetInteger( val );
}

void CLR_RT_StackFrame::SetResult_U8( CLR_UINT64& val )

{
    CLR_RT_HeapBlock& top = PushValue();

    top.SetInteger( val );
}

void CLR_RT_StackFrame::SetResult_Boolean( bool val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetBoolean( val );
}

void CLR_RT_StackFrame::SetResult_Object( CLR_RT_HeapBlock* val )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = PushValue();

    top.SetObjectReference( val );
}

HRESULT CLR_RT_StackFrame::SetResult_String( LPCSTR val )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& top = PushValue();

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_String::CreateInstance( top, val ));

    TINYCLR_NOCLEANUP();
}


void CLR_RT_StackFrame::ConvertResultToBoolean()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = TopValue();

    top.SetBoolean( top.NumericByRef().s4 == 0 );
}

void CLR_RT_StackFrame::NegateResult()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& top = TopValue();

    top.NumericByRef().s4 = top.NumericByRef().s4 ? 0 : 1;
}

//--//

HRESULT CLR_RT_StackFrame::SetupTimeout( CLR_RT_HeapBlock& input, CLR_INT64*& output )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(m_customState == 0)
    {
        CLR_RT_HeapBlock& ref = PushValueAndClear();
        CLR_INT64         timeExpire;

        //
        // Initialize timeout and save it on the stack.
        //
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.InitTimeout( timeExpire, input.NumericByRef().s4 ));

        ref.SetInteger( timeExpire );

        m_customState = 1;
    }

    output = (CLR_INT64*)&m_evalStack[ 0 ].NumericByRef().s8;

    TINYCLR_NOCLEANUP();
}

//--//

void CLR_RT_StackFrame::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();

#ifndef TINYCLR_NO_IL_INLINE
    if(m_inlineFrame)
    {
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_inlineFrame->m_frame.m_call.m_assm   );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_inlineFrame->m_frame.m_call.m_target );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_inlineFrame->m_frame.m_IPStart       );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_inlineFrame->m_frame.m_IP            );

        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_call.m_assm   );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_call.m_target );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_nativeMethod  );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_IPstart       );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_IP            );

        CLR_RT_GarbageCollector::Heap_Relocate( m_inlineFrame->m_frame.m_args     , m_inlineFrame->m_frame.m_call.m_target->numArgs   );
        CLR_RT_GarbageCollector::Heap_Relocate( m_inlineFrame->m_frame.m_locals   , m_inlineFrame->m_frame.m_call.m_target->numLocals );
        CLR_RT_GarbageCollector::Heap_Relocate( m_inlineFrame->m_frame.m_evalStack, (int)(m_evalStackPos - m_inlineFrame->m_frame.m_evalStack) );
        CLR_RT_GarbageCollector::Heap_Relocate( m_locals, m_call.m_target->numLocals );
    }
    else
#endif
    {
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_call.m_assm   );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_call.m_target );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_nativeMethod  );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_IPstart       );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_IP            );

        CLR_RT_GarbageCollector::Heap_Relocate( m_arguments, m_call.m_target->numArgs   );
        CLR_RT_GarbageCollector::Heap_Relocate( m_locals   , m_call.m_target->numLocals );
        CLR_RT_GarbageCollector::Heap_Relocate( m_evalStack, TopValuePosition()         );
    }
}

//--//

HRESULT CLR_RT_StackFrame::NotImplementedStub()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(m_call.m_target->retVal != DATATYPE_VOID)
    {
        SetResult_I4( 0 );
    }

    TINYCLR_SET_AND_LEAVE(CLR_E_NOTIMPL);

    TINYCLR_NOCLEANUP();
}


