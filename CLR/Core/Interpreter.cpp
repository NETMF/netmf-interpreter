////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_TRACE_EXCEPTIONS) && defined(_WIN32)

struct BackTrackExecution
{
    CLR_RT_Assembly*       m_assm;
    CLR_RT_MethodDef_Index m_call;
    CLR_PMETADATA          m_IPstart;
    CLR_PMETADATA          m_IP;
    int                    m_pid;
    int                    m_depth;
};

static BackTrackExecution s_track[ 512 ];
static int                s_trackPos;

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_TRACE_STACK)

bool g_CLR_RT_fBadStack;

#define RESETSTACK()               { g_CLR_RT_fBadStack = false;                                          }
#define CHECKSTACK(stack,evalPos)  { g_CLR_RT_fBadStack =        stack->m_evalStackPos <=  evalPos      ; }
#define UPDATESTACK(stack,evalPos) { g_CLR_RT_fBadStack = false; stack->m_evalStackPos  = &evalPos[ + 1]; }


#else

#define RESETSTACK()
#define CHECKSTACK(stack,evalPos)
#define UPDATESTACK(stack,evalPos) { stack->m_evalStackPos  = &evalPos[ +1 ]; }

#endif

//--//
#define EMPTYSTACK(stack,evalPos)           { evalPos               = &stack->m_evalStack   [ -1 ];                                            RESETSTACK(); }
#define READCACHE(stack,evalPos,ip,fDirty)  { evalPos               = &stack->m_evalStackPos[ -1 ]; ip          = stack->m_IP; fDirty = true ; RESETSTACK(); }
#define WRITEBACK(stack,evalPos,ip,fDirty)  { stack->m_evalStackPos = &evalPos              [ +1 ]; stack->m_IP = ip         ; fDirty = false; RESETSTACK(); }

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_RT_HeapBlock::InitObject()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock*            obj = this;
    CLR_DataType                 dt  = obj->DataType();
    const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ dt ];

    if(dtl.m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType)
    {
        obj->NumericByRef().u8 = 0;
        return false;
    }

    if(dt == DATATYPE_VALUETYPE && obj->IsBoxed() == false)
    {
        CLR_UINT32 num = obj->DataSize();

        while(--num)
        {
            (++obj)->InitObject();
        }

        return false;
    }

    if(dt == DATATYPE_OBJECT || dt == DATATYPE_BYREF)
    {
        CLR_RT_HeapBlock* ptr = obj->Dereference(); if(!ptr) return false;

        if(ptr->InitObject() == false) return false;

        obj->SetObjectReference( NULL );
    }

    return true;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_HeapBlock::Convert_Internal( CLR_DataType et )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_DataType dt                     = DataType();
    const CLR_RT_DataTypeLookup& dtlSrc = c_CLR_RT_DataTypeLookup[ dt ];
    const CLR_RT_DataTypeLookup& dtlDst = c_CLR_RT_DataTypeLookup[ et ];
    int          scaleIn;
    int          scaleOut;

    //
    // Extend to maximum precision.
    //
    switch(dt)
    {
    case DATATYPE_BOOLEAN :
    case DATATYPE_I1      :
    case DATATYPE_U1      :

    case DATATYPE_CHAR    :
    case DATATYPE_I2      :
    case DATATYPE_U2      :

    case DATATYPE_I4      :
    case DATATYPE_U4      :
        {
            CLR_UINT64 res   = (CLR_UINT64)NumericByRef().u4;

            if((dtlSrc.m_flags & CLR_RT_DataTypeLookup::c_Signed) && (dtlDst.m_flags & CLR_RT_DataTypeLookup::c_Signed))
            {
                CLR_UINT32 shift = 64 - dtlSrc.m_sizeInBits;

                res <<= shift;
                res = (CLR_UINT64)((CLR_INT64 )res >> shift);
            }

            NumericByRef().u8 = res;
        }
        //
        // Fall-through!!!
        //
    case DATATYPE_I8:
    case DATATYPE_U8:
        scaleIn = 0;
        break;

    case DATATYPE_R4: scaleIn = 1; break;
    case DATATYPE_R8: scaleIn = 2; break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    //--//

    switch(et)
    {
    case DATATYPE_R4: scaleOut = 1; break;
    case DATATYPE_R8: scaleOut = 2; break;
    default         : scaleOut = 0; break;
    }

    //--//

    if(scaleOut != scaleIn)
    {
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)


        double val = 0;

        if((dtlSrc.m_flags & CLR_RT_DataTypeLookup::c_Signed) == 0) scaleIn  = -1;
        if((dtlDst.m_flags & CLR_RT_DataTypeLookup::c_Signed) == 0) scaleOut = -1;

        switch(scaleIn)
        {
        case -1: val = (double)((CLR_UINT64_TEMP_CAST)NumericByRef().u8); break;
        case  0: val = (double)((CLR_INT64_TEMP_CAST) NumericByRef().s8); break;
        case  1: val = (double)                       NumericByRef().r4; break;
        case  2: val =                                NumericByRef().r8; break;
        }

        switch(scaleOut)
        {
        // Direct casting of negative double to CLR_UINT64 is returns zero for RVDS 3.1 compiler. 
        // Double cast looks as most portable way. 
        case -1: NumericByRef().u8 = (CLR_UINT64)(CLR_INT64 )val; break;
        case  0: NumericByRef().s8 = (CLR_INT64 )val; break;
        case  1: NumericByRef().r4 = (float     )val; break;
        case  2: NumericByRef().r8 =             val; break;
        }
#else
        CLR_INT64 val = 0;
        CLR_INT64 orig = 0;

        if((dtlSrc.m_flags & CLR_RT_DataTypeLookup::c_Signed) == 0) scaleIn  = -1;
        if((dtlDst.m_flags & CLR_RT_DataTypeLookup::c_Signed) == 0) scaleOut = -1;

        switch(scaleIn)
        {
        case -1: orig = ((CLR_INT64)((CLR_UINT64_TEMP_CAST)NumericByRef().u8)); val = orig <<  CLR_RT_HeapBlock::HB_DoubleShift;                                    break;
        case  0: orig = ((CLR_INT64)((CLR_INT64_TEMP_CAST) NumericByRef().s8)); val = orig <<  CLR_RT_HeapBlock::HB_DoubleShift;                                    break;
        case  1: orig = ((CLR_INT64)((CLR_INT32)           NumericByRef().r4)); val = orig << (CLR_RT_HeapBlock::HB_DoubleShift - CLR_RT_HeapBlock::HB_FloatShift); break;
        case  2: orig = ((CLR_INT64)                       NumericByRef().r8 ); val = orig;                                                                         break;
        }

        switch(scaleOut)
        {
        // Direct casting of negative double to CLR_UINT64 is returns zero for RVDS 3.1 compiler. 
        // Double cast looks as most portable way. 
        case -1: NumericByRef().u8 = (CLR_UINT64)(CLR_INT64 )(val >>  CLR_RT_HeapBlock::HB_DoubleShift);                                    break;
        case  0: NumericByRef().s8 = (CLR_INT64 )            (val >>  CLR_RT_HeapBlock::HB_DoubleShift);                                    break;
        case  1: NumericByRef().r4 = (CLR_INT64 )            (val >> (CLR_RT_HeapBlock::HB_DoubleShift - CLR_RT_HeapBlock::HB_FloatShift)); break;
        case  2: NumericByRef().r8 =                          val;                                                                          break;
        }

        if(scaleIn < 1 && scaleOut > 0)
        {
            switch(scaleOut)
            {
                case 1:
                    {
                        CLR_INT32 r4 = (CLR_INT32)NumericByRef().r4;
                        
                        if((orig != (((CLR_INT64)r4) >> CLR_RT_HeapBlock::HB_FloatShift)) ||
                            (orig > 0 && r4 < 0))
                        {
                            NumericByRef().r4 = orig < 0 ? 0x80000000 : 0x7FFFFFFF;
                            
                            //
                            // Uncomment to produce an overflow exception for emulated floating points
                            //
                            // TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
                        }
                    }
                    break;
                case 2:
                    {
                        CLR_INT64 r8 = (CLR_INT64)NumericByRef().r8;
                            
                        if((orig != (r8 >> CLR_RT_HeapBlock::HB_DoubleShift)) ||
                            (orig > 0 && r8 < 0))
                        {
                            NumericByRef().r8 = orig < 0 ? ULONGLONGCONSTANT(0x8000000000000000) : ULONGLONGCONSTANT(0x7FFFFFFFFFFFFFFF);
                            
                            //
                            // Uncomment to produce an overflow exception for emulated floating points
                            //
                            // TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
                        }
                    }
                    break;
            }
        }
        else if(scaleIn == 2 && scaleOut == 1)
        {
            CLR_INT32 r4 = (CLR_INT64)NumericByRef().r4;
            
            if((orig != (((CLR_UINT64)r4) << (CLR_RT_HeapBlock::HB_DoubleShift - CLR_RT_HeapBlock::HB_FloatShift))) ||
                (orig > 0 && r4 < 0))
            {
                NumericByRef().r4 = orig < 0 ? 0x80000000 : 0x7FFFFFFF;
                
                //
                // Uncomment to produce an overflow exception for emulated floating points
                //
                //TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
            }
        }
#endif
        
    }

    //--//

    //
    // This takes care of truncations.
    //
    ChangeDataType( et );
    Promote       (    );

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

CLR_RT_Thread::UnwindStack* CLR_RT_Thread::PushEH()
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_nestedExceptionsPos < ARRAYSIZE(m_nestedExceptions))
    {
        memset(&m_nestedExceptions[ m_nestedExceptionsPos ], 0, sizeof(UnwindStack));
        return &m_nestedExceptions[ m_nestedExceptionsPos++ ];
    }
    else
    {
#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "WARNING: TOO MANY NESTED EXCEPTIONS!!\r\n" );
#endif
        // clip the oldest exception in the nest
        memmove(&m_nestedExceptions[0], &m_nestedExceptions[1], sizeof(m_nestedExceptions) - sizeof(m_nestedExceptions[0]));
        return &m_nestedExceptions[ m_nestedExceptionsPos-1 ];
    }
}

void CLR_RT_Thread::PopEH_Inner( CLR_RT_StackFrame* stack, CLR_PMETADATA ip )
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // We could be jumping outside of a nested exception handler.
    //
    if(m_nestedExceptionsPos > 0)
    {
        //
        // Different stack, don't pop.
        //
        if(m_nestedExceptions[ m_nestedExceptionsPos-1 ].m_stack != stack) return;
    
        //
        // No longer check for same stack since nested exceptions will have different
        // stacks
        //
        while(m_nestedExceptionsPos > 0)
        {
            UnwindStack& us = m_nestedExceptions[ m_nestedExceptionsPos-1 ];

            //
            // The new target is within the previous handler, don't pop.
            //
            if(ip && (us.m_currentBlockStart <= ip && ip < us.m_currentBlockEnd)) break;

#ifndef TINYCLR_NO_IL_INLINE
            if(stack->m_inlineFrame) break;
#endif

            m_nestedExceptionsPos--;
        }
    }
}

bool CLR_RT_Thread::FindEhBlock( CLR_RT_StackFrame* stack, CLR_PMETADATA from, CLR_PMETADATA to, CLR_RT_ExceptionHandler& eh, bool onlyFinallys)
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_Assembly*         assm     = stack->m_call.m_assm;
    CLR_RT_ExceptionHandler* ptrEhExt = NULL;
    const CLR_RECORD_EH*     ptrEh    = NULL;
    CLR_UINT32               numEh    = 0;

    //FROM is always non-NULL and indicates the current IP
    _ASSERTE(from);
    //onlyFinallys is false when we're searching for a handler for an exception, and to should be NULL
    _ASSERTE(FIMPLIES(!onlyFinallys, to==NULL));
    //onlyFinallys is true in Phase2, endfinally, leave, etc. to is NULL when we want to leave outside of the current stack frame,
    //or non-NULL and pointing to an IL instruction that we are going to when finally's, if any, are processed.

#if defined(TINYCLR_TRACE_EXCEPTIONS)
    if(s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Annoying)
    {
        if(!onlyFinallys || s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Obnoxious)
        {
            CLR_Debug::Printf( "Unwinding at " ); CLR_RT_DUMP::METHOD( stack->m_call ); CLR_Debug::Printf( " [IP: %04x - %d]\r\n", (size_t)stack->m_IP, (stack->m_IP - stack->m_IPstart) );
        }
    }
#endif

    if(stack->m_call.m_target->flags & CLR_RECORD_METHODDEF::MD_HasExceptionHandlers)
    {
        switch(stack->m_flags & CLR_RT_StackFrame::c_MethodKind_Mask)
        {
        case CLR_RT_StackFrame::c_MethodKind_Interpreted:
            {
                CLR_OFFSET ipMethod_Start;
                CLR_OFFSET ipMethod_End;

                if(assm->FindMethodBoundaries( stack->m_call.Method(), ipMethod_Start, ipMethod_End ))
                {
                    const CLR_RECORD_EH* ptrEh2;
                    CLR_UINT32           numEh2;

                    CLR_RECORD_EH::ExtractEhFromByteCode( stack->m_IPstart + (ipMethod_End - ipMethod_Start), ptrEh2, numEh2 );

                    ptrEh = ptrEh2; // This allows the compiler to leave ptrEh and numEh in registers...
                    numEh = numEh2; // This allows the compiler to leave ptrEh and numEh in registers...
                }
                else
                {
                    return false;
                }

                ptrEhExt = NULL;
            }
            break;

        case CLR_RT_StackFrame::c_MethodKind_Native:
            return false;

#if defined(TINYCLR_JITTER)
        case CLR_RT_StackFrame::c_MethodKind_Jitted:
            {
                numEh    = *(CLR_UINT32*)             stack->m_IPstart;
                ptrEhExt = (CLR_RT_ExceptionHandler*)(stack->m_IPstart + sizeof(CLR_UINT32));
                ptrEh    = NULL;
            }
            break;
#endif
        }

        //
        // We're certain there's at least one EH.
        //
        while(numEh--)
        {
            if(ptrEh)
            {
                eh.ConvertFromEH( stack->m_call, stack->m_IPstart, ptrEh++ );

                ptrEhExt = &eh;
            }

#if defined(TINYCLR_TRACE_EXCEPTIONS)
            if(s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Annoying)
            {
                if(to == NULL || s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Obnoxious)
                {
                    CLR_Debug::Printf( "Checking EH: %04X-%04X => %04X\r\n", (size_t)ptrEhExt->m_tryStart, (size_t)ptrEhExt->m_tryEnd, ptrEhExt->m_handlerStart - stack->m_IPstart );
                }
            }
#endif

            if(ptrEhExt->IsFilter() && ptrEhExt->m_userFilterStart <= from && from < ptrEhExt->m_handlerStart)
            {
                //The IP was in the middle of this EH block's filter.
                //Therefore, reset the IP to inside the original try block so we can try the next handler in this sequence:
                // Try
                //     ...            //2. set IP to here
                // Catch When False
                // Catch When False   //1. from was in this filter
                // Catch ...          //3. This is the next EH block, and will be found correctly.
                // ...
                // End Try
                from = ptrEhExt->m_tryStart;
            }
            else if(from >= ptrEhExt->m_tryStart && from < ptrEhExt->m_tryEnd)
            {
                if(onlyFinallys)
                {
                    //
                    // Only execute the finally if this is an exception or we leave the block.
                    //
                    if(ptrEhExt->IsFinally() && (!to || (to < ptrEhExt->m_tryStart || to >= ptrEhExt->m_tryEnd)))
                    {
#if defined(TINYCLR_TRACE_EXCEPTIONS)
                        if(s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Obnoxious)
                        {
                            CLR_Debug::Printf( "Found match for a 'finally'\r\n" );
                        }
#endif

                        eh = *ptrEhExt;
                        return true;
                    }
                }
                else
                {
                    if(ptrEhExt->IsCatchAll())
                    {
#if defined(TINYCLR_TRACE_EXCEPTIONS)
                        if(s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Annoying)
                        {
                            CLR_Debug::Printf( "Found match for a 'catch all'\r\n" );
                        }
#endif

                        eh = *ptrEhExt;
                        return true; // Catch all...
                    }
                    else
                    {
                        if(ptrEhExt->IsFilter() || CLR_RT_ExecutionEngine::IsInstanceOf( m_currentException, ptrEhExt->m_typeFilter ))
                        {
#if defined(TINYCLR_TRACE_EXCEPTIONS)
                            if(s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Annoying)
                            {
                                if (ptrEhExt->IsFilter())
                                {
                                    CLR_Debug::Printf( "Trying a 'filter'\r\n" );
                                }
                                else
                                {
                                    CLR_Debug::Printf( "Found match for a 'catch'\r\n" );
                                }
                            }
#endif

                            eh = *ptrEhExt;
                            return true;
                        }
                    }
                }
            }

            ptrEhExt++;
        }
    }
    
#if defined(TINYCLR_TRACE_EXCEPTIONS)
        if(s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Annoying)
        {
            if(to == NULL || s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Obnoxious)
            {
                CLR_Debug::Printf( "No match\r\n" );
            }
        }
#endif

    return false;
}

//--//

HRESULT CLR_RT_Thread::Execute()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain* appDomainSav = g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( this->CurrentAppDomain() );
#endif
 
    CLR_RT_Thread* currentThreadSav = g_CLR_RT_ExecutionEngine.m_currentThread;

    g_CLR_RT_ExecutionEngine.m_currentThread = this;

    if(m_currentException.Dereference() != NULL)
    {
        hr = CLR_E_PROCESS_EXCEPTION;
    }
    else
    {
        hr = S_OK;
    }

    m_timeQuantumExpired = FALSE;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    _ASSERTE(!CLR_EE_DBG_IS( Stopped ));
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    ::Events_SetBoolTimer( (BOOL*)&m_timeQuantumExpired, CLR_RT_Thread::c_TimeQuantum_Milliseconds );

    while(m_timeQuantumExpired == FALSE && !CLR_EE_DBG_IS( Stopped ))
    {
        CLR_RT_StackFrame* stack;

        if(SUCCEEDED(hr))
        {
            hr = Execute_Inner(); if(SUCCEEDED(hr)) TINYCLR_LEAVE();
        }

        switch(hr)
        {
        case CLR_E_THREAD_WAITING:
        case CLR_E_RESCHEDULE:
            TINYCLR_LEAVE();

        case CLR_E_PROCESS_EXCEPTION:
            CLR_RT_DUMP::POST_PROCESS_EXCEPTION( m_currentException );
            break;

        default: // Allocate a new exception.
            stack = CurrentFrame();
            if(stack->Prev() != NULL)
            {           
#if defined(TINYCLR_TRACE_INSTRUCTIONS) && defined(PLATFORM_WINDOWS_EMULATOR)
                for(int i = 0; i < ARRAYSIZE(s_track); i++)
                {
                    BackTrackExecution& track = s_track[ (s_trackPos+i) % ARRAYSIZE(s_track) ];

                    CLR_Debug::Printf( " %3d ", track.m_depth );

                    CLR_RT_MethodDef_Instance inst; inst.InitializeFromIndex( track.m_call );

                    //track.m_assm->DumpOpcodeDirect( inst, track.m_IP, track.m_IPstart, track.m_pid );
                }

                CLR_Debug::Printf( "\r\n" );
#endif
                                
                (void)Library_corlib_native_System_Exception::CreateInstance( m_currentException, hr, stack );
            }

            hr = CLR_E_PROCESS_EXCEPTION;
            break;
        }

        //
        // Process exception.
        //

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(CLR_EE_DBG_IS( Stopped )) break;
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        
        TINYCLR_CHECK_HRESULT(ProcessException());

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(CLR_EE_DBG_IS( Stopped )) break;
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

        if(m_currentException.Dereference() != NULL) { break; }
    }

    TINYCLR_SET_AND_LEAVE(CLR_S_QUANTUM_EXPIRED);

    TINYCLR_CLEANUP();

#if defined(TINYCLR_APPDOMAINS)
    g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );
#endif
    
    g_CLR_RT_ExecutionEngine.m_currentThread = currentThreadSav;

    ::Events_SetBoolTimer( NULL, 0 );

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_Thread::Execute_Inner()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    while(m_timeQuantumExpired == FALSE && !CLR_EE_DBG_IS( Stopped ))
    {
        CLR_RT_StackFrame *stack = CurrentFrame();

        #if defined(ENABLE_NATIVE_PROFILER)
        if(stack->m_owningThread->m_fNativeProfiled == true)
        {
            Native_Profiler_Start();
        }
        #endif

        if(stack->Prev() == NULL)
        {
            m_status = CLR_RT_Thread::TH_S_Terminated;

            TINYCLR_SET_AND_LEAVE(CLR_S_THREAD_EXITED); // End of Thread.
        }

        if(stack->m_flags & CLR_RT_StackFrame::c_ProcessSynchronize)
        {
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            if(stack->m_flags & CLR_RT_StackFrame::c_InvalidIP)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

            //
            // Thread cannot run if a lock request is still pending...
            //
            if(stack->m_flags & (CLR_RT_StackFrame::c_PendingSynchronizeGlobally | CLR_RT_StackFrame::c_PendingSynchronize))
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_THREAD_WAITING);
            }

            if(stack->m_flags & CLR_RT_StackFrame::c_NeedToSynchronizeGlobally)
            {
                stack->m_flags &= ~CLR_RT_StackFrame::c_NeedToSynchronizeGlobally;

                if(FAILED(hr = stack->HandleSynchronized( true, true )))
                {
                    if(hr == CLR_E_THREAD_WAITING)
                    {
                        stack->m_flags |= CLR_RT_StackFrame::c_PendingSynchronizeGlobally;
                    }

                    TINYCLR_LEAVE();
                }

                stack->m_flags |= CLR_RT_StackFrame::c_SynchronizedGlobally;
            }

            if(stack->m_flags & CLR_RT_StackFrame::c_NeedToSynchronize)
            {
                stack->m_flags &= ~CLR_RT_StackFrame::c_NeedToSynchronize;

                if(FAILED(hr = stack->HandleSynchronized( true, false )))
                {
                    if(hr == CLR_E_THREAD_WAITING)
                    {
                        stack->m_flags |= CLR_RT_StackFrame::c_PendingSynchronize;
                    }

                    TINYCLR_LEAVE();
                }

                stack->m_flags |= CLR_RT_StackFrame::c_Synchronized;
            }
        }

        {
#if defined(TINYCLR_PROFILE_NEW_CALLS)
            CLR_PROF_HANDLER_CALLCHAIN( pm2, stack->m_callchain );
#endif

            CLR_UINT32 methodKind = (stack->m_flags & CLR_RT_StackFrame::c_MethodKind_Mask);

            switch(methodKind)
            {
            case CLR_RT_StackFrame::c_MethodKind_Native:
                CLR_RT_ExecutionEngine::ExecutionConstraint_Suspend();
                break;

            case CLR_RT_StackFrame::c_MethodKind_Interpreted:
                //_ASSERTE((stack->m_flags & CLR_RT_StackFrame::c_InvalidIP) == 0);
                stack->m_flags |= CLR_RT_StackFrame::c_ExecutingIL;
                break;
            }

            // perform systematic GC and compaction under memory pressure
            // (g_CLR_RT_GarbageCollector.m_freeBytes refers to last time GC was run)
            if(g_CLR_RT_GarbageCollector.m_freeBytes < g_CLR_RT_GarbageCollector.c_memoryThreshold2)
            {
                stack->m_flags |= CLR_RT_StackFrame::c_CompactAndRestartOnOutOfMemory;
            }
            
            while(true)
            {
#if defined(TINYCLR_JITTER_ARMEMULATION)
                if(methodKind == CLR_RT_StackFrame::c_MethodKind_Jitted)
                {
                    if(s_CLR_RT_fJitter_Trace_Execution >= c_CLR_RT_Trace_Info) CLR_RT_ExecutionEngine::ExecutionConstraint_Suspend();

                    hr = g_CLR_RT_ExecutionEngine.Emulate( stack );

                    if(s_CLR_RT_fJitter_Trace_Execution >= c_CLR_RT_Trace_Info) CLR_RT_ExecutionEngine::ExecutionConstraint_Resume();
                }
                else
#endif
                {
                    #if defined(ENABLE_NATIVE_PROFILER)
                    if(stack->m_flags & CLR_RT_StackFrame::c_NativeProfiled)
                    {
                        Native_Profiler_Start();
                    }
                    else if(stack->m_fNativeProfiled == false)
                    {
                        stack->m_owningThread->m_fNativeProfiled = false;
                        Native_Profiler_Stop();
                    }
                    #endif
                    
                    hr = stack->m_nativeMethod( *stack );
                }

                // check for exception injected by native code
                if(m_currentException.Dereference() != NULL)
                {
                    hr = CLR_E_PROCESS_EXCEPTION;
                }
                                
                //The inner loop may push or pop more stack frames 
                stack = CurrentFrame();

                if(stack->Prev() == NULL)
                {          
                    m_status = CLR_RT_Thread::TH_S_Terminated;

                    TINYCLR_SET_AND_LEAVE(CLR_S_THREAD_EXITED); // End of Thread.
                }

                if(hr == CLR_E_OUT_OF_MEMORY && (stack->m_flags & CLR_RT_StackFrame::c_CompactAndRestartOnOutOfMemory))
                {
                    stack->m_flags &= ~CLR_RT_StackFrame::c_CompactAndRestartOnOutOfMemory;

                    g_CLR_RT_ExecutionEngine.PerformHeapCompaction();
                }
                else
                {
                    break;
                }
            }

            switch(methodKind)
            {
            case CLR_RT_StackFrame::c_MethodKind_Native:
                CLR_RT_ExecutionEngine::ExecutionConstraint_Resume();
                break;
            }

            if(FAILED(hr))
            {
                //
                // CLR_E_RESTART_EXECUTION is used to inject calls to methods from native code.
                //
                if(hr != CLR_E_RESTART_EXECUTION) TINYCLR_LEAVE();
            }
            else
            {
                stack->m_flags &= ~CLR_RT_StackFrame::c_ExecutingIL;

                if(hr == S_OK)
                {
                    CurrentFrame()->Pop();
                }
            }
        }
    }

    TINYCLR_SET_AND_LEAVE(CLR_S_QUANTUM_EXPIRED);

    TINYCLR_CLEANUP();

    #if defined(ENABLE_NATIVE_PROFILER)
    Native_Profiler_Stop();
    #endif

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_Thread::Execute_DelegateInvoke( CLR_RT_StackFrame* stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const CLR_RECORD_METHODDEF* md;
    CLR_RT_HeapBlock_Delegate*  dlg;
    CLR_RT_HeapBlock*           array;
    CLR_RT_HeapBlock*           ptr;
    CLR_UINT32                  num;

    ptr = &stack->m_arguments[ 0 ]; if(ptr->DataType() != DATATYPE_OBJECT) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    dlg = ptr->DereferenceDelegate(); FAULT_ON_NULL(dlg);

    md = stack->m_call.m_target;

    switch(dlg->DataType())
    {
    case DATATYPE_DELEGATE_HEAD:
        array = ptr;
        num   = 1;
        break;

    case DATATYPE_DELEGATELIST_HEAD:
        {
            CLR_RT_HeapBlock_Delegate_List* list = ptr->DereferenceDelegateList();

            array = list->GetDelegates();
            num   = list->m_length;
        }
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    while(true)
    {
        if(stack->m_customState >= num) // We have called all the delegates, let's return.
        {
            TINYCLR_SET_AND_LEAVE(S_OK);
        }

        dlg = array[ stack->m_customState++ ].DereferenceDelegate();

        if(dlg == NULL || dlg->DataType() != DATATYPE_DELEGATE_HEAD) continue;

        break;
    }
        
    //--//

    stack->ResetStack();

    {
        CLR_RT_ProtectFromGC      gc( *dlg );
        CLR_RT_MethodDef_Instance inst; inst.InitializeFromIndex( dlg->DelegateFtn() );
        bool                      fStaticMethod = (inst.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) != 0;

        TINYCLR_CHECK_HRESULT(stack->MakeCall(inst, fStaticMethod ? NULL : &dlg->m_object, &stack->m_arguments[ 1 ], md->numArgs - 1 ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_Thread::Execute_IL( CLR_RT_StackFrame* stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Thread*     th   = stack->m_owningThread;
    CLR_RT_Assembly*   assm = stack->m_call.m_assm;
    CLR_RT_HeapBlock*  evalPos;
    CLR_PMETADATA      ip;
    bool               fCondition;
    bool               fDirty = false;

    READCACHE(stack,evalPos,ip,fDirty);

    while(true)
    {
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(th->m_timeQuantumExpired == TRUE)
        {
            TINYCLR_SET_AND_LEAVE( CLR_S_QUANTUM_EXPIRED );
        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if defined(TINYCLR_TRACE_EXCEPTIONS) && defined(_WIN32)
        if(s_CLR_RT_fTrace_Exceptions >= c_CLR_RT_Trace_Annoying)
        {
            CLR_PROF_HANDLER_SUSPEND_TIME();

            BackTrackExecution& track = s_track[ s_trackPos++ ]; s_trackPos %= ARRAYSIZE(s_track);
            int                 depth = 0;

            TINYCLR_FOREACH_NODE_BACKWARD__DIRECT(CLR_RT_StackFrame,tmp,stack)
            {
                depth++;
            }
            TINYCLR_FOREACH_NODE_BACKWARD_END();

            track.m_assm    = assm;
            track.m_call    = stack->m_call;
            track.m_IP      = ip;
            track.m_IPstart = stack->m_IPstart;
            track.m_pid     = stack->m_owningThread->m_pid;
            track.m_depth   = depth;

            CLR_PROF_HANDLER_RESUME_TIME();
        }
#endif

        assm->DumpOpcode( stack, ip );

        CLR_OPCODE op = CLR_OPCODE(*ip++);

Execute_RestartDecoding:

#if defined(TINYCLR_OPCODE_STACKCHANGES)
        if(op != CEE_PREFIX1)
        {
            TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyStackOK( *stack, &evalPos[ 1 ], c_CLR_RT_OpcodeLookup[ op ].StackChanges() ));
        }
#endif

        {
            ////////////////////////
            //
            //
#if defined(PLATFORM_WINDOWS_EMULATOR)
            if(s_CLR_RT_fTrace_SimulateSpeed > c_CLR_RT_Trace_None)
            {
                CLR_PROF_Handler::SuspendTime();

                HAL_Windows_FastSleep( g_HAL_Configuration_Windows.TicksPerOpcode );                    

                CLR_PROF_Handler::ResumeTime();
            }
#endif
            //
            //
            ////////////////////////

            //--//


            switch(op)
            {
#define OPDEF(name,string,pop,push,oprType,opcType,l,s1,s2,ctrl) case name:
            OPDEF(CEE_PREFIX1,                    "prefix1",          Pop0,               Push0,       InlineNone,         IInternal,   1,  0xFF,    0xFE,    META)
            {
                op = CLR_OPCODE(*ip++ + 256);
                goto Execute_RestartDecoding;
            }

            OPDEF(CEE_BREAK,                      "break",            Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x01,    BREAK)
                break;

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDARG_0,                    "ldarg.0",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x02,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_arguments[ 0 ] );

                goto Execute_LoadAndPromote;
            }

            OPDEF(CEE_LDARG_1,                    "ldarg.1",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x03,    NEXT)
            OPDEF(CEE_LDARG_2,                    "ldarg.2",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x04,    NEXT)
            OPDEF(CEE_LDARG_3,                    "ldarg.3",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x05,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_arguments[ op - CEE_LDARG_0 ] );

                goto Execute_LoadAndPromote;
            }

            OPDEF(CEE_LDARG_S,                    "ldarg.s",          Pop0,               Push1,       ShortInlineVar,     IMacro,      1,  0xFF,    0x0E,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT8(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_arguments[ arg ] );

                goto Execute_LoadAndPromote;
            }

            OPDEF(CEE_LDARG,                      "ldarg",            Pop0,               Push1,       InlineVar,          IPrimitive,  2,  0xFE,    0x09,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT16(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_arguments[ arg ] );

                goto Execute_LoadAndPromote;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDLOC_0,                    "ldloc.0",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x06,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_locals[ 0 ] );

                goto Execute_LoadAndPromote;
            }

            OPDEF(CEE_LDLOC_1,                    "ldloc.1",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x07,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_locals[ 1 ] );

                goto Execute_LoadAndPromote;
            }

            OPDEF(CEE_LDLOC_2,                    "ldloc.2",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x08,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_locals[ 2 ] );

                goto Execute_LoadAndPromote;
            }

            OPDEF(CEE_LDLOC_3,                    "ldloc.3",          Pop0,               Push1,       InlineNone,         IMacro,      1,  0xFF,    0x09,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_locals[ 3 ] );

                goto Execute_LoadAndPromote;
            }

            OPDEF(CEE_LDLOC_S,                    "ldloc.s",          Pop0,               Push1,       ShortInlineVar,     IMacro,      1,  0xFF,    0x11,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT8(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_locals[ arg ] );

                goto Execute_LoadAndPromote;
            }

            OPDEF(CEE_LDLOC,                      "ldloc",            Pop0,               Push1,       InlineVar,          IPrimitive,  2,  0xFE,    0x0C,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT16(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( stack->m_locals[ arg ] );

                goto Execute_LoadAndPromote;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDLOCA_S,                   "ldloca.s",         Pop0,               PushI,       ShortInlineVar,     IMacro,      1,  0xFF,    0x12,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT8(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetReference( stack->m_locals[ arg ] );
                break;
            }

            OPDEF(CEE_LDLOCA,                     "ldloca",           Pop0,               PushI,       InlineVar,          IPrimitive,  2,  0xFE,    0x0D,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT16(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetReference( stack->m_locals[ arg ] );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDARGA_S,                   "ldarga.s",         Pop0,               PushI,       ShortInlineVar,     IMacro,      1,  0xFF,    0x0F,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT8(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetReference( stack->m_arguments[ arg ] );
                break;
            }

            OPDEF(CEE_LDARGA,                     "ldarga",           Pop0,               PushI,       InlineVar,          IPrimitive,  2,  0xFE,    0x0A,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT16(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetReference( stack->m_arguments[ arg ] );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_STLOC_0,                    "stloc.0",          Pop1,               Push0,       InlineNone,         IMacro,      1,  0xFF,    0x0A,    NEXT)
            OPDEF(CEE_STLOC_1,                    "stloc.1",          Pop1,               Push0,       InlineNone,         IMacro,      1,  0xFF,    0x0B,    NEXT)
            OPDEF(CEE_STLOC_2,                    "stloc.2",          Pop1,               Push0,       InlineNone,         IMacro,      1,  0xFF,    0x0C,    NEXT)
            OPDEF(CEE_STLOC_3,                    "stloc.3",          Pop1,               Push0,       InlineNone,         IMacro,      1,  0xFF,    0x0D,    NEXT)
            // Stack: ... ... <value> -> ...
            {
                stack->m_locals[ op - CEE_STLOC_0 ].AssignPreserveTypeCheckPinned( evalPos[ 0 ] );
                                         
                evalPos--; CHECKSTACK(stack,evalPos);
                break;
            }

            OPDEF(CEE_STLOC_S,                    "stloc.s",          Pop1,               Push0,       ShortInlineVar,     IMacro,      1,  0xFF,    0x13,    NEXT)
            // Stack: ... ... <value> -> ...
            {
                FETCH_ARG_UINT8(arg,ip);

                stack->m_locals[ arg ].AssignPreserveTypeCheckPinned( evalPos[ 0 ] );

                evalPos--; CHECKSTACK(stack,evalPos);
                break;
            }

            OPDEF(CEE_STLOC,                      "stloc",            Pop1,               Push0,       InlineVar,          IPrimitive,  2,  0xFE,    0x0E,    NEXT)
            // Stack: ... ... <value> -> ...
            {
                FETCH_ARG_UINT16(arg,ip);

                stack->m_locals[ arg ].AssignPreserveTypeCheckPinned( evalPos[ 0 ] );

                evalPos--; CHECKSTACK(stack,evalPos);
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_STARG_S,                    "starg.s",          Pop1,               Push0,       ShortInlineVar,     IMacro,      1,  0xFF,    0x10,    NEXT)
            // Stack: ... ... <value> -> ...
            {
                FETCH_ARG_UINT8(arg,ip);

                stack->m_arguments[ arg ].AssignAndPreserveType( evalPos[ 0 ] );

                evalPos--; CHECKSTACK(stack,evalPos);
                break;
            }

            OPDEF(CEE_STARG,                      "starg",            Pop1,               Push0,       InlineVar,          IPrimitive,  2,  0xFE,    0x0B,    NEXT)
            // Stack: ... ... <value> -> ...
            {
                FETCH_ARG_UINT16(arg,ip);

                stack->m_arguments[ arg ].AssignAndPreserveType( evalPos[ 0 ] );

                evalPos--; CHECKSTACK(stack,evalPos);
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDNULL,                     "ldnull",           Pop0,               PushRef,     InlineNone,         IPrimitive,  1,  0xFF,    0x14,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetObjectReference( NULL );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDC_I4_M1,                  "ldc.i4.m1",        Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x15,    NEXT)
            OPDEF(CEE_LDC_I4_0,                   "ldc.i4.0",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x16,    NEXT)
            OPDEF(CEE_LDC_I4_1,                   "ldc.i4.1",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x17,    NEXT)
            OPDEF(CEE_LDC_I4_2,                   "ldc.i4.2",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x18,    NEXT)
            OPDEF(CEE_LDC_I4_3,                   "ldc.i4.3",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x19,    NEXT)
            OPDEF(CEE_LDC_I4_4,                   "ldc.i4.4",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1A,    NEXT)
            OPDEF(CEE_LDC_I4_5,                   "ldc.i4.5",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1B,    NEXT)
            OPDEF(CEE_LDC_I4_6,                   "ldc.i4.6",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1C,    NEXT)
            OPDEF(CEE_LDC_I4_7,                   "ldc.i4.7",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1D,    NEXT)
            OPDEF(CEE_LDC_I4_8,                   "ldc.i4.8",         Pop0,               PushI,       InlineNone,         IMacro,      1,  0xFF,    0x1E,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( (CLR_INT32)op - (CLR_INT32)CEE_LDC_I4_0 );
                break;
            }

            OPDEF(CEE_LDC_I4_S,                   "ldc.i4.s",         Pop0,               PushI,       ShortInlineI,       IMacro,      1,  0xFF,    0x1F,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_INT8(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( (CLR_INT32)arg );
                break;
            }

            OPDEF(CEE_LDC_I4,                     "ldc.i4",           Pop0,               PushI,       InlineI,            IPrimitive,  1,  0xFF,    0x20,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_INT32(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( (CLR_INT32)arg );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDC_I8,                     "ldc.i8",           Pop0,               PushI8,      InlineI8,           IPrimitive,  1,  0xFF,    0x21,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_INT64(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( (CLR_INT64)arg );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDC_R4,                     "ldc.r4",           Pop0,               PushR4,      ShortInlineR,       IPrimitive,  1,  0xFF,    0x22,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT32(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
                evalPos[ 0 ].SetFloatFromBits( arg );
#else
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].SetFloatIEEE754( arg ));
#endif
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDC_R8,                     "ldc.r8",           Pop0,               PushR8,      InlineR,            IPrimitive,  1,  0xFF,    0x23,    NEXT)
            // Stack: ... ... -> <value> ...
            {
                FETCH_ARG_UINT64(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
                evalPos[ 0 ].SetDoubleFromBits( arg );
#else
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].SetDoubleIEEE754( arg ));
#endif
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_DUP,                        "dup",              Pop1,               Push1+Push1, InlineNone,         IPrimitive,  1,  0xFF,    0x25,    NEXT)
            // Stack: ... ... <value> -> <value> <value> ...
            {
                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( evalPos[ -1 ] );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_POP,                        "pop",              Pop1,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x26,    NEXT)
            // Stack: ... ... <value> -> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);
                break;
            }

            //----------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BR_S,                       "br.s",             Pop0,               Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2B,    BRANCH)
            OPDEF(CEE_BR,                         "br",               Pop0,               Push0,       InlineBrTarget,     IPrimitive,  1,  0xFF,    0x38,    BRANCH)
            {
                fCondition = true;
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BRFALSE_S,                  "brfalse.s",        PopI,               Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2C,    COND_BRANCH)
            OPDEF(CEE_BRFALSE,                    "brfalse",          PopI,               Push0,       InlineBrTarget,     IPrimitive,  1,  0xFF,    0x39,    COND_BRANCH)
            // Stack: ... ... <value> -> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                fCondition = (evalPos[ 1 ].IsZero() == true);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BRTRUE_S,                   "brtrue.s",         PopI,               Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2D,    COND_BRANCH)
            OPDEF(CEE_BRTRUE,                     "brtrue",           PopI,               Push0,       InlineBrTarget,     IPrimitive,  1,  0xFF,    0x3A,    COND_BRANCH)
            // Stack: ... ... <value> -> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                fCondition = (evalPos[ 1 ].IsZero() == false);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BEQ_S,                      "beq.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2E,    COND_BRANCH)
            OPDEF(CEE_BEQ,                        "beq",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3B,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Signed_Values( evalPos[ 1 ], evalPos[ 2 ] ) == 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BGE_S,                      "bge.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x2F,    COND_BRANCH)
            OPDEF(CEE_BGE,                        "bge",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3C,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Signed_Values( evalPos[ 1 ], evalPos[ 2 ] ) >= 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BGT_S,                      "bgt.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x30,    COND_BRANCH)
            OPDEF(CEE_BGT,                        "bgt",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3D,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Signed_Values( evalPos[ 1 ], evalPos[ 2 ] ) > 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BLE_S,                      "ble.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x31,    COND_BRANCH)
            OPDEF(CEE_BLE,                        "ble",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3E,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Signed_Values( evalPos[ 1 ], evalPos[ 2 ] ) <= 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BLT_S,                      "blt.s",            Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x32,    COND_BRANCH)
            OPDEF(CEE_BLT,                        "blt",              Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x3F,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Signed_Values( evalPos[ 1 ], evalPos[ 2 ] ) < 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BNE_UN_S,                   "bne.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x33,    COND_BRANCH)
            OPDEF(CEE_BNE_UN,                     "bne.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x40,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Unsigned_Values( evalPos[ 1 ], evalPos[ 2 ] ) != 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BGE_UN_S,                   "bge.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x34,    COND_BRANCH)
            OPDEF(CEE_BGE_UN,                     "bge.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x41,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Unsigned_Values( evalPos[ 1 ], evalPos[ 2 ] ) >= 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BGT_UN_S,                   "bgt.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x35,    COND_BRANCH)
            OPDEF(CEE_BGT_UN,                     "bgt.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x42,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Unsigned_Values( evalPos[ 1 ], evalPos[ 2 ] ) > 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BLE_UN_S,                   "ble.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x36,    COND_BRANCH)
            OPDEF(CEE_BLE_UN,                     "ble.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x43,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Unsigned_Values( evalPos[ 1 ], evalPos[ 2 ] ) <= 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BLT_UN_S,                   "blt.un.s",         Pop1+Pop1,          Push0,       ShortInlineBrTarget,IMacro,      1,  0xFF,    0x37,    COND_BRANCH)
            OPDEF(CEE_BLT_UN,                     "blt.un",           Pop1+Pop1,          Push0,       InlineBrTarget,     IMacro,      1,  0xFF,    0x44,    COND_BRANCH)
            // Stack: ... ... <value1> <value2> -> ...
            {
                evalPos -= 2; CHECKSTACK(stack,evalPos);

                fCondition = (CLR_RT_HeapBlock::Compare_Unsigned_Values( evalPos[ 1 ], evalPos[ 2 ] ) < 0);
                goto Execute_BR;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_SWITCH,                     "switch",           PopI,               Push0,       InlineSwitch,       IPrimitive,  1,  0xFF,    0x45,    COND_BRANCH)
            // Stack: ... ... <value> -> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                FETCH_ARG_UINT8(arg,ip);

                CLR_UINT32 numCases     = arg;
                CLR_UINT32 caseSelected = evalPos[ 1 ].NumericByRef().u4;

                if(caseSelected < numCases)
                {
                    CLR_PMETADATA ipsub = ip + (CLR_INT32)caseSelected * sizeof(CLR_INT16);

                    FETCH_ARG_INT16(offset,ipsub);

                    ip += offset;
                }

                ip += (CLR_INT32)numCases * sizeof(CLR_INT16);
                break;
            }

            //----------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDIND_I1,                   "ldind.i1",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x46,    NEXT)
            OPDEF(CEE_LDIND_U1,                   "ldind.u1",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x47,    NEXT)
            OPDEF(CEE_LDIND_I2,                   "ldind.i2",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x48,    NEXT)
            OPDEF(CEE_LDIND_U2,                   "ldind.u2",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x49,    NEXT)
            OPDEF(CEE_LDIND_I4,                   "ldind.i4",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x4A,    NEXT)
            OPDEF(CEE_LDIND_U4,                   "ldind.u4",         PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x4B,    NEXT)
            OPDEF(CEE_LDIND_I8,                   "ldind.i8",         PopI,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x4C,    NEXT)
            OPDEF(CEE_LDIND_I,                    "ldind.i",          PopI,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x4D,    NEXT)
            OPDEF(CEE_LDIND_R4,                   "ldind.r4",         PopI,               PushR4,      InlineNone,         IPrimitive,  1,  0xFF,    0x4E,    NEXT)
            OPDEF(CEE_LDIND_R8,                   "ldind.r8",         PopI,               PushR8,      InlineNone,         IPrimitive,  1,  0xFF,    0x4F,    NEXT)
            OPDEF(CEE_LDIND_REF,                  "ldind.ref",        PopI,               PushRef,     InlineNone,         IPrimitive,  1,  0xFF,    0x50,    NEXT)
            // Stack: ... ... <address> -> <value> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].LoadFromReference( evalPos[ 0 ] ));

                goto Execute_LoadAndPromote;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_STIND_REF,                  "stind.ref",        PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x51,    NEXT)
            OPDEF(CEE_STIND_I1,                   "stind.i1",         PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x52,    NEXT)
            OPDEF(CEE_STIND_I2,                   "stind.i2",         PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x53,    NEXT)
            OPDEF(CEE_STIND_I4,                   "stind.i4",         PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x54,    NEXT)
            OPDEF(CEE_STIND_I8,                   "stind.i8",         PopI+PopI8,         Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x55,    NEXT)
            OPDEF(CEE_STIND_R4,                   "stind.r4",         PopI+PopR4,         Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x56,    NEXT)
            OPDEF(CEE_STIND_R8,                   "stind.r8",         PopI+PopR8,         Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x57,    NEXT)
            OPDEF(CEE_STIND_I,                    "stind.i",          PopI+PopI,          Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xDF,    NEXT)
            // Stack: ... ... <address> <value> -> ...
            {
                int size = 0;

                evalPos -= 2; CHECKSTACK(stack,evalPos);

                switch(op)
                {
                case CEE_STIND_I  : size = 4; break;
                case CEE_STIND_I1 : size = 1; break;
                case CEE_STIND_I2 : size = 2; break;
                case CEE_STIND_I4 : size = 4; break;
                case CEE_STIND_I8 : size = 8; break;
                case CEE_STIND_R4 : size = 4; break;
                case CEE_STIND_R8 : size = 8; break;
                case CEE_STIND_REF: size = 0; break;
                }

                evalPos[ 2 ].Promote();

                TINYCLR_CHECK_HRESULT(evalPos[ 2 ].StoreToReference( evalPos[ 1 ], size ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_ADD,                        "add",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x58,    NEXT)
            OPDEF(CEE_ADD_OVF,                    "add.ovf",          Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xD6,    NEXT)
            OPDEF(CEE_ADD_OVF_UN,                 "add.ovf.un",       Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xD7,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericAdd( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_SUB,                        "sub",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x59,    NEXT)
            OPDEF(CEE_SUB_OVF,                    "sub.ovf",          Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xDA,    NEXT)
            OPDEF(CEE_SUB_OVF_UN,                 "sub.ovf.un",       Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xDB,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericSub( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_MUL,                        "mul",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5A,    NEXT)
            OPDEF(CEE_MUL_OVF,                    "mul.ovf",          Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xD8,    NEXT)
            OPDEF(CEE_MUL_OVF_UN,                 "mul.ovf.un",       Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0xD9,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericMul( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_DIV,                        "div",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5B,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericDiv( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_DIV_UN,                     "div.un",           Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5C,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericDivUn( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_REM,                        "rem",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5D,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericRem( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_REM_UN,                     "rem.un",           Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5E,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericRemUn( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_AND,                        "and",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x5F,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].NumericByRef().u8 &= evalPos[ 1 ].NumericByRef().u8;
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_OR,                         "or",               Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x60,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].NumericByRef().u8 |= evalPos[ 1 ].NumericByRef().u8;
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_XOR,                        "xor",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x61,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].NumericByRef().u8 ^= evalPos[ 1 ].NumericByRef().u8;
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_SHL,                        "shl",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x62,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericShl( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_SHR,                        "shr",              Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x63,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericShr( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_SHR_UN,                     "shr.un",           Pop1+Pop1,          Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x64,    NEXT)
            // Stack: ... ... <value1> <value2> -> <valueR> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericShrUn( evalPos[ 1 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_NEG,                        "neg",              Pop1,               Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x65,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].NumericNeg());
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_NOT,                        "not",              Pop1,               Push1,       InlineNone,         IPrimitive,  1,  0xFF,    0x66,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                evalPos[ 0 ].NumericByRef().u8 = ~evalPos[ 0 ].NumericByRef().u8;
                break;
            }

            //----------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_I1,                    "conv.i1",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x67,    NEXT)
            OPDEF(CEE_CONV_OVF_I1,                "conv.ovf.i1",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB3,    NEXT)
            OPDEF(CEE_CONV_OVF_I1_UN,             "conv.ovf.i1.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x82,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_I1, op != CEE_CONV_I1, op == CEE_CONV_OVF_I1_UN ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_I2,                    "conv.i2",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x68,    NEXT)
            OPDEF(CEE_CONV_OVF_I2,                "conv.ovf.i2",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB5,    NEXT)
            OPDEF(CEE_CONV_OVF_I2_UN,             "conv.ovf.i2.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x83,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_I2, op != CEE_CONV_I2, op == CEE_CONV_OVF_I2_UN ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_I4,                    "conv.i4",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x69,    NEXT)
            OPDEF(CEE_CONV_OVF_I4,                "conv.ovf.i4",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB7,    NEXT)
            OPDEF(CEE_CONV_OVF_I4_UN,             "conv.ovf.i4.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x84,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_I4, op != CEE_CONV_I4, op == CEE_CONV_OVF_I4_UN ));
                break;
            }

            OPDEF(CEE_CONV_I,                     "conv.i",           Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD3,    NEXT)
            OPDEF(CEE_CONV_OVF_I,                 "conv.ovf.i",       Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD4,    NEXT)
            OPDEF(CEE_CONV_OVF_I_UN,              "conv.ovf.i.un",    Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x8A,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                if ( evalPos[ 0 ].DataType() == DATATYPE_BYREF || evalPos[ 0 ].DataType() == DATATYPE_ARRAY_BYREF )
                {
                    break; 
                }
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_I4, op != CEE_CONV_I, op == CEE_CONV_OVF_I_UN ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_I8,                    "conv.i8",          Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x6A,    NEXT)
            OPDEF(CEE_CONV_OVF_I8,                "conv.ovf.i8",      Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0xB9,    NEXT)
            OPDEF(CEE_CONV_OVF_I8_UN,             "conv.ovf.i8.un",   Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x85,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_I8, op != CEE_CONV_I8, op == CEE_CONV_OVF_I8_UN ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_R4,                    "conv.r4",          Pop1,               PushR4,      InlineNone,         IPrimitive,  1,  0xFF,    0x6B,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_R4, false, false ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//
            
            OPDEF(CEE_CONV_R_UN,                  "conv.r.un",        Pop1,               PushR8,      InlineNone,         IPrimitive,  1,  0xFF,    0x76,    NEXT)
            OPDEF(CEE_CONV_R8,                    "conv.r8",          Pop1,               PushR8,      InlineNone,         IPrimitive,  1,  0xFF,    0x6C,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_R8, false, op == CEE_CONV_R_UN ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_U1,                    "conv.u1",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD2,    NEXT)
            OPDEF(CEE_CONV_OVF_U1,                "conv.ovf.u1",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB4,    NEXT)
            OPDEF(CEE_CONV_OVF_U1_UN,             "conv.ovf.u1.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x86,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_U1, op != CEE_CONV_U1, op == CEE_CONV_OVF_U1_UN ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_U2,                    "conv.u2",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD1,    NEXT)
            OPDEF(CEE_CONV_OVF_U2,                "conv.ovf.u2",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB6,    NEXT)
            OPDEF(CEE_CONV_OVF_U2_UN,             "conv.ovf.u2.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x87,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_U2, op != CEE_CONV_U2, op == CEE_CONV_OVF_U2_UN ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_U4,                    "conv.u4",          Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x6D,    NEXT)
            OPDEF(CEE_CONV_OVF_U4,                "conv.ovf.u4",      Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xB8,    NEXT)
            OPDEF(CEE_CONV_OVF_U4_UN,             "conv.ovf.u4.un",   Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x88,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_U4, op != CEE_CONV_U4, op == CEE_CONV_OVF_U4_UN ));
                break;
            }

            OPDEF(CEE_CONV_U,                     "conv.u",           Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xE0,    NEXT)
            OPDEF(CEE_CONV_OVF_U,                 "conv.ovf.u",       Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0xD5,    NEXT)
            OPDEF(CEE_CONV_OVF_U_UN,              "conv.ovf.u.un",    Pop1,               PushI,       InlineNone,         IPrimitive,  1,  0xFF,    0x8B,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                if ( evalPos[ 0 ].DataType() == DATATYPE_BYREF || evalPos[ 0 ].DataType() == DATATYPE_ARRAY_BYREF )
                {
                    break; 
                }
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_U4, op != CEE_CONV_U, op == CEE_CONV_OVF_U_UN ));
               break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONV_U8,                    "conv.u8",          Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x6E,    NEXT)
            OPDEF(CEE_CONV_OVF_U8,                "conv.ovf.u8",      Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0xBA,    NEXT)
            OPDEF(CEE_CONV_OVF_U8_UN,             "conv.ovf.u8.un",   Pop1,               PushI8,      InlineNone,         IPrimitive,  1,  0xFF,    0x89,    NEXT)
            // Stack: ... ... <value> -> <valueR> ...
            {
                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].Convert( DATATYPE_U8, op != CEE_CONV_U8, op == CEE_CONV_OVF_U8_UN ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------//
            OPDEF(CEE_CALL,                       "call",             VarPop,             VarPush,     InlineMethod,       IPrimitive,  1,  0xFF,    0x28,    CALL)
            OPDEF(CEE_CALLVIRT,                   "callvirt",         VarPop,             VarPush,     InlineMethod,       IObjModel,   1,  0xFF,    0x6F,    CALL)

            {
                FETCH_ARG_COMPRESSED_METHODTOKEN(arg,ip);

                CLR_RT_MethodDef_Instance calleeInst; if(calleeInst.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                CLR_RT_TypeDef_Index      cls;
                CLR_RT_HeapBlock*         pThis;
#if defined(TINYCLR_APPDOMAINS)                
                bool                      fAppDomainTransition = false;
#endif

                pThis = &evalPos[1-calleeInst.m_target->numArgs]; // Point to the first arg, 'this' if an instance method
               
                if(calleeInst.m_target->flags & CLR_RECORD_METHODDEF::MD_DelegateInvoke)
                {
                    CLR_RT_HeapBlock_Delegate* dlg = pThis->DereferenceDelegate(); FAULT_ON_NULL(dlg);

                    if(dlg->DataType() == DATATYPE_DELEGATE_HEAD)
                    {
                        calleeInst.InitializeFromIndex( dlg->DelegateFtn() );

                        if((calleeInst.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) == 0)
                        {
                            pThis->Assign( dlg->m_object );

#if defined(TINYCLR_APPDOMAINS)
                            fAppDomainTransition = pThis[ 0 ].IsTransparentProxy();
#endif
                        }
                        else
                        {                            
                            memmove( &pThis[ 0 ], &pThis[ 1 ], calleeInst.m_target->numArgs * sizeof(CLR_RT_HeapBlock) );

                            evalPos--;
                        }
                    }
                    else
                    {
                        //
                        // The lookup for multicast delegates is done at a later stage...
                        //
                    }
                }
                else //Non delegate
                {                                        
                    CLR_RT_MethodDef_Index calleeReal;
                    
                    if((calleeInst.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) == 0)
                    {                        
                        //Instance method, pThis[ 0 ] is valid
           
                        if(op == CEE_CALL && pThis[ 0 ].Dereference() == NULL)
                        {                                                    
                            //CALL on a null instance is allowed, and should not throw a NullReferenceException on the call
                            //although a NullReferenceException is likely to be thrown soon thereafter if the call tries to access
                            //any member variables.       
                        }
                        else
                        {
                            TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( pThis[ 0 ], cls ));

                            //This test is for performance reasons.  c# emits a callvirt on all instance methods to make sure that 
                            //a NullReferenceException is thrown if 'this' is NULL.  However, if the instance method isn't virtual
                            //we don't need to do the more expensive virtual method lookup.
                            if(op == CEE_CALLVIRT && (calleeInst.m_target->flags & (CLR_RECORD_METHODDEF::MD_Abstract | CLR_RECORD_METHODDEF::MD_Virtual)))
                            {
                                if(g_CLR_RT_EventCache.FindVirtualMethod( cls, calleeInst, calleeReal ) == false)
                                {
                                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                                }

                                calleeInst.InitializeFromIndex( calleeReal );
                            }

#if defined(TINYCLR_APPDOMAINS)
                            fAppDomainTransition = pThis[ 0 ].IsTransparentProxy();
#endif
                        }
                    }
                }
                               
#if defined(TINYCLR_APPDOMAINS)
                if(fAppDomainTransition)
                {
                    WRITEBACK(stack,evalPos,ip,fDirty);

                    _ASSERTE(FIMPLIES(pThis->DataType() == DATATYPE_OBJECT, pThis->Dereference() != NULL));
                    TINYCLR_CHECK_HRESULT(CLR_RT_StackFrame::PushAppDomainTransition( th, calleeInst, &pThis[ 0 ], &pThis[ 1 ]));
                }
                else
#endif //TINYCLR_APPDOMAINS
                {
#ifndef TINYCLR_NO_IL_INLINE
                    if(stack->PushInline(ip, assm, evalPos, calleeInst, pThis))
                    {
                        fDirty = true;
                        break;
                    }
#endif

                    WRITEBACK(stack,evalPos,ip,fDirty);
                    TINYCLR_CHECK_HRESULT(CLR_RT_StackFrame::Push( th, calleeInst, -1 ));
                }
                                
                goto Execute_Restart;
            }
            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_RET,                        "ret",              VarPop,             Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x2A,    RETURN)
            {
#ifndef TINYCLR_NO_IL_INLINE
                if(stack->m_inlineFrame)
                {
                    stack->m_evalStackPos = evalPos;
                    
                    stack->PopInline();
                    
                    ip      = stack->m_IP;
                    assm    = stack->m_call.m_assm;
                    evalPos = stack->m_evalStackPos-1;
                    fDirty  = true;
                    
                    break;
                }
#endif

                WRITEBACK(stack,evalPos,ip,fDirty);

                //
                // Same kind of handler, no need to pop back out, just restart execution in place.
                //
                if(stack->m_flags & CLR_RT_StackFrame::c_CallerIsCompatibleForRet)
                {
                    CLR_RT_StackFrame* stackNext = stack->Caller();

                    stack->Pop();

                    stack = stackNext;
                    goto Execute_Reload;
                }
                else
                {
                    stack->Pop();

                    TINYCLR_SET_AND_LEAVE(CLR_S_RESTART_EXECUTION);
                }
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CPOBJ,                      "cpobj",            PopI+PopI,          Push0,       InlineType,         IObjModel,   1,  0xFF,    0x70,    NEXT)
            OPDEF(CEE_STOBJ,                      "stobj",            PopI+Pop1,          Push0,       InlineType,         IPrimitive,  1,  0xFF,    0x81,    NEXT)
            // Stack: ... ... <dstValObj> <srcValObj> -> ...
            {
                ip += 2; // Skip argument, not used...

                evalPos -= 2; CHECKSTACK(stack,evalPos);

                //
                // Reassign will make sure these are objects of the same type.
                //
                TINYCLR_CHECK_HRESULT(evalPos[ 1 ].Reassign( evalPos[ 2 ] ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDOBJ,                      "ldobj",            PopI,               Push1,       InlineType,         IObjModel,   1,  0xFF,    0x71,    NEXT)
            // Stack: ... ... <srcValObj> -> <valObj> ...
            {
                FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

                CLR_RT_TypeDef_Instance type;
                CLR_RT_TypeDef_Index    cls;

                if(type.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( evalPos[ 0 ], cls ));

                // Check this is an object of the requested type.
                if(type.m_data != cls.m_data)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }

                UPDATESTACK(stack,evalPos);

                {
                    //
                    // Save the pointer to the object to load/copy and protect it from GC.
                    //
                    CLR_RT_HeapBlock     safeSource; safeSource.Assign( evalPos[ 0 ] );
                    CLR_RT_ProtectFromGC gc( safeSource );

                    TINYCLR_CHECK_HRESULT(evalPos[ 0 ].LoadFromReference( safeSource ));

                }
                
                goto Execute_LoadAndPromote;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDSTR,                      "ldstr",            Pop0,               PushRef,     InlineString,       IObjModel,   1,  0xFF,    0x72,    NEXT)
            {
                FETCH_ARG_COMPRESSED_STRINGTOKEN(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                UPDATESTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( evalPos[ 0 ], arg, assm ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_NEWOBJ,                     "newobj",           VarPop,             PushRef,     InlineMethod,       IObjModel,   1,  0xFF,    0x73,    CALL)
            // Stack: ... <arg1> <arg2> ... <argN> -> ...
            {
                FETCH_ARG_COMPRESSED_METHODTOKEN(arg,ip);

                CLR_RT_MethodDef_Instance calleeInst; if(calleeInst.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                CLR_RT_TypeDef_Instance   cls;
                CLR_RT_HeapBlock*         top;
                CLR_INT32                 changes;

                cls.InitializeFromMethod( calleeInst ); // This is the class to create!

                evalPos++;

                WRITEBACK(stack,evalPos,ip,fDirty);

                if(cls.m_target->IsDelegate())
                {
                    //
                    // Special case for delegates. LDFTN or LDVIRTFTN have already created the delegate object, just check that...
                    //
                    changes = -calleeInst.m_target->numArgs;
                    TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyStackOK( *stack, stack->m_evalStackPos, changes )); // Check to see if we have enough parameters.
                    stack->m_evalStackPos += changes;

                    top = stack->m_evalStackPos++; // Push back the result.

                    if(top[ 1 ].DataType() != DATATYPE_OBJECT)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                    }

                    CLR_RT_HeapBlock_Delegate* dlg = top[ 1 ].DereferenceDelegate();

                    if(dlg == NULL)
                    {
                        TINYCLR_CHECK_HRESULT(CLR_E_NULL_REFERENCE);
                    }

                    dlg->m_cls = cls;

                    CLR_RT_MethodDef_Instance dlgInst;

                    if(dlgInst.InitializeFromIndex( dlg->DelegateFtn() ) == false)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                    }

                    if((dlgInst.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) == 0)
                    {
                        dlg->m_object.Assign( top[ 0 ] );
                    }

                    top->SetObjectReference( dlg );
                }
                else
                {
                    changes = calleeInst.m_target->numArgs;
                    TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyStackOK( *stack, stack->m_evalStackPos, -changes )); // Check to see if we have enough parameters.
                    top = stack->m_evalStackPos;

                    //
                    // We have to insert the 'this' pointer as argument 0, that means moving all the arguments up one slot...
                    //
                    top--;
                    while(--changes > 0)
                    {
                        top[ 0 ].Assign( top[ -1 ] ); top--;
                    }
                    top->SetObjectReference( NULL );

                    // Stack: ... <null> <arg1> <arg2> ... <argN> -> ...
                    //            ^
                    //            Top points here.

                    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObject( top[ 0 ], cls ));

                    //
                    // This is to flag the fact that we need to copy back the 'this' pointer into our stack.
                    //
                    // See CLR_RT_StackFrame::Pop()
                    //
                    stack->m_flags |= CLR_RT_StackFrame::c_ExecutingConstructor;

                    //
                    // Ok, creating a ValueType then calls its constructor.
                    // But the constructor will try to load the 'this' pointer and since it's a value type, it will be cloned.
                    // For the length of the constructor, change the type from an object pointer to a reference.
                    //
                    // See CLR_RT_StackFrame::Pop()
                    //
                    if((cls.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_ValueType)
                    {
                        if(top[ 0 ].DataType() == DATATYPE_OBJECT)
                        {
                            top[ 0 ].ChangeDataType( DATATYPE_BYREF );
                        }
                        else
                        {
                            //
                            // This is to support the optimization on DateTime and TimeSpan:
                            //
                            // These are passed as built-ins. But we need to pass them as a reference,
                            // so push everything down and undo the "ExecutingConstructor" trick.
                            //
                            top = stack->m_evalStackPos++;

                            changes = calleeInst.m_target->numArgs;
                            while(--changes > 0)
                            {
                                top[ 0 ].Assign( top[ -1 ] ); top--;
                            }
                            top[ 0 ].SetReference( top[ -1 ] );

                            stack->m_flags &= ~CLR_RT_StackFrame::c_ExecutingConstructor;
                        }
                    }
                    
                    if(FAILED(hr = CLR_RT_StackFrame::Push( th, calleeInst, -1 )))
                    {   
                        if(hr == CLR_E_NOT_SUPPORTED)
                        {
                            // no matter what, we are no longer executing a ctor
                            stack->m_flags &= ~CLR_RT_StackFrame::c_ExecutingConstructor;  
                        }

                        TINYCLR_LEAVE();
                    }

                    goto Execute_Restart;
                }

                READCACHE(stack,evalPos,ip,fDirty);
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CASTCLASS,                  "castclass",        PopRef,             PushRef,     InlineType,         IObjModel,   1,  0xFF,    0x74,    NEXT)
            OPDEF(CEE_ISINST,                     "isinst",           PopRef,             PushI,       InlineType,         IObjModel,   1,  0xFF,    0x75,    NEXT)
            {
                FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

                TINYCLR_CHECK_HRESULT(CLR_RT_ExecutionEngine::CastToType( evalPos[ 0 ], arg, assm, (op == CEE_ISINST) ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_THROW,                      "throw",            PopRef,             Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x7A,    THROW)
            {
                th->m_currentException.Assign( evalPos[ 0 ] ); 
                
                EMPTYSTACK(stack, evalPos);

                Library_corlib_native_System_Exception::SetStackTrace( th->m_currentException, stack );

                TINYCLR_CHECK_HRESULT(CLR_E_PROCESS_EXCEPTION);
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDFLD,                      "ldfld",            PopRef,             Push1,       InlineField,        IObjModel,   1,  0xFF,    0x7B,    NEXT)
            // Stack: ... <obj> -> <value> ...
            {
                FETCH_ARG_COMPRESSED_FIELDTOKEN(arg,ip);

                CLR_RT_FieldDef_Instance fieldInst; if(fieldInst.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                CLR_RT_HeapBlock*        obj = &evalPos[ 0 ];
                CLR_DataType             dt  = obj->DataType();

                TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractObjectAndDataType(obj, dt));

                switch(dt)
                {
                    case DATATYPE_CLASS:
                    case DATATYPE_VALUETYPE:
                        evalPos[ 0 ].Assign( obj[ fieldInst.CrossReference().m_offset ] );
                        goto Execute_LoadAndPromote;
                    case DATATYPE_DATETIME:
                    case DATATYPE_TIMESPAN:
                        evalPos[ 0 ].SetInteger( (CLR_INT64)obj->NumericByRefConst().s8 );
                        break;
#if defined(TINYCLR_APPDOMAINS)
                    case DATATYPE_TRANSPARENT_PROXY:
                        {
                            CLR_RT_HeapBlock val;

                            TINYCLR_CHECK_HRESULT(obj->TransparentProxyValidate());

                            UPDATESTACK(stack,evalPos);

                            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->MarshalObject( obj->TransparentProxyDereference()[ fieldInst.CrossReference().m_offset ], val, obj->TransparentProxyAppDomain() ));
                            
                            evalPos[ 0 ].Assign( val );
                        }
                        goto Execute_LoadAndPromote;
#endif
                    default:
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                        break;

                }
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDFLDA,                     "ldflda",           PopRef,             PushI,       InlineField,        IObjModel,   1,  0xFF,    0x7C,    NEXT)
            // Stack: ... <obj> -> <address>...
            {
                FETCH_ARG_COMPRESSED_FIELDTOKEN(arg,ip);

                CLR_RT_FieldDef_Instance fieldInst; if(fieldInst.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                CLR_RT_HeapBlock*        obj = &evalPos[ 0 ];
                CLR_DataType             dt  = obj->DataType();

                TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractObjectAndDataType(obj, dt));

#if defined(TINYCLR_APPDOMAINS)
                _ASSERTE(dt != DATATYPE_TRANSPARENT_PROXY);
#endif
                if(dt == DATATYPE_CLASS || dt == DATATYPE_VALUETYPE)
                {
                    evalPos[ 0 ].SetReference( obj[ fieldInst.CrossReference().m_offset ] );
                }
                else if(dt == DATATYPE_DATETIME || dt == DATATYPE_TIMESPAN) // Special case.
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE); // NOT SUPPORTED.
                }
                else
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }

                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_STFLD,                      "stfld",            PopRef+Pop1,        Push0,       InlineField,        IObjModel,   1,  0xFF,    0x7D,    NEXT)
            // Stack: ... ... <obj> <value> -> ...
            {
                FETCH_ARG_COMPRESSED_FIELDTOKEN(arg,ip);

                evalPos -= 2; CHECKSTACK(stack,evalPos);

                CLR_RT_FieldDef_Instance fieldInst; if(fieldInst.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                CLR_RT_HeapBlock*        obj = &evalPos[ 1 ];
                CLR_DataType             dt  = obj->DataType();

                TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractObjectAndDataType(obj, dt));

                switch(dt)
                {
                    case DATATYPE_CLASS:
                    case DATATYPE_VALUETYPE:
                        obj[ fieldInst.CrossReference().m_offset ].AssignAndPreserveType( evalPos[ 2 ] );
                        break;
                    case DATATYPE_DATETIME: // Special case.
                    case DATATYPE_TIMESPAN: // Special case.
                        obj->NumericByRef().s8 = evalPos[ 2 ].NumericByRefConst().s8;
                        break;

#if defined(TINYCLR_APPDOMAINS)
                    case DATATYPE_TRANSPARENT_PROXY:
                        {
                            CLR_RT_HeapBlock val;

                            UPDATESTACK(stack,evalPos);

                            TINYCLR_CHECK_HRESULT(obj->TransparentProxyValidate());
                            TINYCLR_CHECK_HRESULT(obj->TransparentProxyAppDomain()->MarshalObject( evalPos[ 2 ], val ));

                            obj->TransparentProxyDereference()[ fieldInst.CrossReference().m_offset ].AssignAndPreserveType( val );
                        }
                        break;
#endif

                    default:
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                        break;
                }

                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDSFLD,                     "ldsfld",           Pop0,               Push1,       InlineField,        IObjModel,   1,  0xFF,    0x7E,    NEXT)
            // Stack: ... -> <value> ...
            {
                FETCH_ARG_COMPRESSED_FIELDTOKEN(arg,ip);

                CLR_RT_FieldDef_Instance field; if(field.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                CLR_RT_HeapBlock* ptr = CLR_RT_ExecutionEngine::AccessStaticField( field ); if(ptr == NULL) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].Assign( *ptr );

                goto Execute_LoadAndPromote;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDSFLDA,                    "ldsflda",          Pop0,               PushI,       InlineField,        IObjModel,   1,  0xFF,    0x7F,    NEXT)
            // Stack: ... -> <address> ...
            {
                FETCH_ARG_COMPRESSED_FIELDTOKEN(arg,ip);

                CLR_RT_FieldDef_Instance field; if(field.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                CLR_RT_HeapBlock* ptr = CLR_RT_ExecutionEngine::AccessStaticField( field ); if(ptr == NULL) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetReference( *ptr );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_STSFLD,                     "stsfld",           Pop1,               Push0,       InlineField,        IObjModel,   1,  0xFF,    0x80,    NEXT)
            // Stack: ... ... <value> -> ...
            {
                FETCH_ARG_COMPRESSED_FIELDTOKEN(arg,ip);

                CLR_RT_FieldDef_Instance field; if(field.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                CLR_RT_HeapBlock* ptr = CLR_RT_ExecutionEngine::AccessStaticField( field ); if(ptr == NULL) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                evalPos--; CHECKSTACK(stack,evalPos);

                ptr->AssignAndPreserveType( evalPos[ 1 ] );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_BOX,                        "box",              Pop1,               PushRef,     InlineType,         IPrimitive,  1,  0xFF,    0x8C,    NEXT)
            OPDEF(CEE_UNBOX,                      "unbox",            PopRef,             PushI,       InlineType,         IPrimitive,  1,  0xFF,    0x79,    NEXT)
            {
                FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

                CLR_RT_TypeDef_Instance typeInst; if(typeInst.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                UPDATESTACK(stack,evalPos);

                if(op == CEE_BOX)
                {
                    TINYCLR_CHECK_HRESULT(evalPos[ 0 ].PerformBoxing( typeInst ));
                }
                else
                {
                    TINYCLR_CHECK_HRESULT(evalPos[ 0 ].PerformUnboxing( typeInst ));
                }
                break;
            }
            
            //----------------------------------------------------------------------------------------------------------//
            
            OPDEF(CEE_UNBOX_ANY,                  "unbox.any",        PopRef,             Push1,       InlineType,         IObjModel,   1,  0xFF,    0xA5,    NEXT)
            {
                //Stack: ... <value> -> ..., value or obj
                FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

                //When applied to the boxed form of a value type, the unbox.any instruction 
                //extracts the value contained within obj (of type O).  (It is equivalent to unbox followed by ldobj.)  
                //When applied to a reference type, the unbox.any instruction has the same effect as castclass typeTok. 

                CLR_RT_TypeDef_Instance typeInst; if(typeInst.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                UPDATESTACK(stack,evalPos);

                if(((typeInst.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_ValueType) ||
                   ((typeInst.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_Enum     ))
                {
                    //"unbox"
                    TINYCLR_CHECK_HRESULT(evalPos[ 0 ].PerformUnboxing( typeInst ));

                    //"ldobj"
                    {
                        CLR_RT_HeapBlock     safeSource; safeSource.Assign( evalPos[ 0 ] );
                        CLR_RT_ProtectFromGC gc( safeSource );

                        TINYCLR_CHECK_HRESULT(evalPos[ 0 ].LoadFromReference( safeSource ));
                        
                        goto Execute_LoadAndPromote;
                    }
                }
                else
                {
                    //"castclass"
                    TINYCLR_CHECK_HRESULT(CLR_RT_ExecutionEngine::CastToType( evalPos[ 0 ], arg, assm, false ));
                }

                break;
            }
            
            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_NEWARR,                     "newarr",           PopI,               PushRef,     InlineType,         IObjModel,   1,  0xFF,    0x8D,    NEXT)
            {
                
                FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

                CLR_UINT32 size = evalPos[ 0 ].NumericByRef().u4;

                UPDATESTACK(stack,evalPos);

                stack->m_flags &= ~CLR_RT_StackFrame::c_CompactAndRestartOnOutOfMemory; // we do not need this in this case

                for(int pass=0; pass<2; pass++)
                {
                    hr = CLR_RT_HeapBlock_Array::CreateInstance( evalPos[ 0 ], size, assm, arg ); if(SUCCEEDED(hr)) break;

                    // if we have an out of memory exception, perform a compaction and try again.
                    if (hr == CLR_E_OUT_OF_MEMORY && pass == 0)
                    {
                        WRITEBACK(stack, evalPos, ip, fDirty);
                        g_CLR_RT_ExecutionEngine.PerformHeapCompaction();
                        READCACHE(stack, evalPos, ip, fDirty);
                    }
                    else
                    {
                        TINYCLR_SET_AND_LEAVE(hr);
                    }
                }
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDLEN,                      "ldlen",            PopRef,             PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x8E,    NEXT)
            // Stack: ... <obj> -> ...
            {
                TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyArrayReference( evalPos[ 0 ] ));

                CLR_RT_HeapBlock_Array* array = evalPos[ 0 ].DereferenceArray();

                evalPos[ 0 ].SetInteger( array->m_numOfElements );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDELEMA,                    "ldelema",          PopRef+PopI,        PushI,       InlineType,         IObjModel,   1,  0xFF,    0x8F,    NEXT)
            // Stack: ... <obj> <index> -> <address> ...
            {
                ip += 2; // Skip argument, not used...

                evalPos--; CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].InitializeArrayReference( evalPos[ 0 ], evalPos[ 1 ].NumericByRef().s4 ));

                evalPos[ 0 ].FixArrayReferenceForValueTypes();
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDELEM_I1,                  "ldelem.i1",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x90,    NEXT)
            OPDEF(CEE_LDELEM_U1,                  "ldelem.u1",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x91,    NEXT)
            OPDEF(CEE_LDELEM_I2,                  "ldelem.i2",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x92,    NEXT)
            OPDEF(CEE_LDELEM_U2,                  "ldelem.u2",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x93,    NEXT)
            OPDEF(CEE_LDELEM_I4,                  "ldelem.i4",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x94,    NEXT)
            OPDEF(CEE_LDELEM_U4,                  "ldelem.u4",        PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x95,    NEXT)
            OPDEF(CEE_LDELEM_I8,                  "ldelem.i8",        PopRef+PopI,        PushI8,      InlineNone,         IObjModel,   1,  0xFF,    0x96,    NEXT)
            OPDEF(CEE_LDELEM_I,                   "ldelem.i",         PopRef+PopI,        PushI,       InlineNone,         IObjModel,   1,  0xFF,    0x97,    NEXT)
            OPDEF(CEE_LDELEM_R4,                  "ldelem.r4",        PopRef+PopI,        PushR4,      InlineNone,         IObjModel,   1,  0xFF,    0x98,    NEXT)
            OPDEF(CEE_LDELEM_R8,                  "ldelem.r8",        PopRef+PopI,        PushR8,      InlineNone,         IObjModel,   1,  0xFF,    0x99,    NEXT)
            OPDEF(CEE_LDELEM_REF,                 "ldelem.ref",       PopRef+PopI,        PushRef,     InlineNone,         IObjModel,   1,  0xFF,    0x9A,    NEXT)
            // Stack: ... <obj> <index> -> <value> ...
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                //
                // To load an element from an array, we first initialize a temporary reference to the element and then dereference it.
                //
                CLR_RT_HeapBlock ref; TINYCLR_CHECK_HRESULT(ref.InitializeArrayReference( evalPos[ 0 ], evalPos[ 1 ].NumericByRef().s4 ));

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].LoadFromReference( ref ));

                goto Execute_LoadAndPromote;
            }
            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDELEM,                     "ldelem",           PopRef+PopI,        Push1,       InlineType,         IObjModel,   1,  0xFF,    0xA3,    NEXT)
            // Stack: ... <obj> <index> -> <value>
            {
                // treat this like ldelema + ldobj
                // read the type token argument from the instruction and advance the instruction pointer accordingly
                // LDELEMA doesn't need the type token, but the ldobj portion does
                FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

                //<LDELEMA>
                // move stack pointer back so that evalpos[0] is the array
                evalPos--;
                CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 0 ].InitializeArrayReference( evalPos[ 0 ], evalPos[ 1 ].NumericByRef().s4 ));

                evalPos[ 0 ].FixArrayReferenceForValueTypes();
                //</LDELEMA>
                // <LDOBJ>
                CLR_RT_TypeDef_Instance type;
                CLR_RT_TypeDef_Index cls;

                if( !type.ResolveToken( arg, assm ) )
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( evalPos[ 0 ], cls ));

                // Check this is an object of the requested type.
                if( type.m_data != cls.m_data )
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                UPDATESTACK(stack,evalPos);
                {
                    // Save the pointer to the object to load/copy and protect it from GC.
                    CLR_RT_HeapBlock safeSource;
                    safeSource.Assign( evalPos[ 0 ] );
                    CLR_RT_ProtectFromGC gc( safeSource );

                    TINYCLR_CHECK_HRESULT(evalPos[ 0 ].LoadFromReference( safeSource ));
                }
                
                goto Execute_LoadAndPromote;
                //<LDOBJ>
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_STELEM_I,                   "stelem.i",         PopRef+PopI+PopI,   Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9B,    NEXT)
            OPDEF(CEE_STELEM_I1,                  "stelem.i1",        PopRef+PopI+PopI,   Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9C,    NEXT)
            OPDEF(CEE_STELEM_I2,                  "stelem.i2",        PopRef+PopI+PopI,   Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9D,    NEXT)
            OPDEF(CEE_STELEM_I4,                  "stelem.i4",        PopRef+PopI+PopI,   Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9E,    NEXT)
            OPDEF(CEE_STELEM_I8,                  "stelem.i8",        PopRef+PopI+PopI8,  Push0,       InlineNone,         IObjModel,   1,  0xFF,    0x9F,    NEXT)
            OPDEF(CEE_STELEM_R4,                  "stelem.r4",        PopRef+PopI+PopR4,  Push0,       InlineNone,         IObjModel,   1,  0xFF,    0xA0,    NEXT)
            OPDEF(CEE_STELEM_R8,                  "stelem.r8",        PopRef+PopI+PopR8,  Push0,       InlineNone,         IObjModel,   1,  0xFF,    0xA1,    NEXT)
            OPDEF(CEE_STELEM_REF,                 "stelem.ref",       PopRef+PopI+PopRef, Push0,       InlineNone,         IObjModel,   1,  0xFF,    0xA2,    NEXT)
            // Stack: ... ... <obj> <index> <value> -> ...
            {
                evalPos -= 3; CHECKSTACK(stack,evalPos);

                //
                // To load an element from an array, we first initialize a temporary reference to the element and then dereference it.
                //
                CLR_RT_HeapBlock ref; TINYCLR_CHECK_HRESULT(ref.InitializeArrayReference( evalPos[ 1 ], evalPos[ 2 ].NumericByRef().s4 ));
                int              size = 0;

                switch(op)
                {
                case CEE_STELEM_I  : size = 4; break;
                case CEE_STELEM_I1 : size = 1; break;
                case CEE_STELEM_I2 : size = 2; break;
                case CEE_STELEM_I4 : size = 4; break;
                case CEE_STELEM_I8 : size = 8; break;
                case CEE_STELEM_R4 : size = 4; break;
                case CEE_STELEM_R8 : size = 8; break;
                case CEE_STELEM_REF: size = 0; break;
                }

                evalPos[ 3 ].Promote();

                TINYCLR_CHECK_HRESULT(evalPos[ 3 ].StoreToReference( ref, size ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_STELEM,                     "stelem",           PopRef+PopI+Pop1,   Push0,       InlineType,         IObjModel,   1,  0xFF,    0xA4,    NEXT)
            // Stack: ... ... <obj> <index> <value> -> ...
            {
                // Treat STELEM like ldelema + stobj
                ip += 2; // Skip type argument, not used...

                evalPos -= 3; // "pop" args from evaluation stack
                CHECKSTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(evalPos[ 1 ].InitializeArrayReference( evalPos[ 1 ], evalPos[ 2 ].NumericByRef().s4 ));
                evalPos[ 1 ].FixArrayReferenceForValueTypes();

                // Reassign will make sure these are objects of the same type.
                TINYCLR_CHECK_HRESULT(evalPos[ 1 ].Reassign( evalPos[ 3 ] ));

                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDTOKEN,                    "ldtoken",          Pop0,               PushI,       InlineTok,          IPrimitive,  1,  0xFF,    0xD0,    NEXT)
            {
                FETCH_ARG_UINT32(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetObjectReference( NULL );

                switch(CLR_TypeFromTk( arg ))
                {
                case TBL_TypeSpec:
                    {
                        CLR_RT_TypeSpec_Instance sig; if(sig.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                        evalPos[ 0 ].SetReflection( sig );
                    }
                    break;

                case TBL_TypeRef:
                case TBL_TypeDef:
                    {
                        CLR_RT_TypeDef_Instance cls; if(cls.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                        evalPos[ 0 ].SetReflection( cls );
                    }
                    break;

                case TBL_FieldRef:
                case TBL_FieldDef:
                    {
                        CLR_RT_FieldDef_Instance field; if(field.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                        evalPos[ 0 ].SetReflection( field );
                    }
                    break;

                case TBL_MethodRef:
                case TBL_MethodDef:
                    {
                        CLR_RT_MethodDef_Instance method; if(method.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                        evalPos[ 0 ].SetReflection( method );
                    }
                    break;

                default:
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                    break;
                }
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_ENDFINALLY,                 "endfinally",       Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0xDC,    RETURN)
            {                                                
                EMPTYSTACK(stack, evalPos);
                WRITEBACK(stack,evalPos,ip,fDirty);
                
                TINYCLR_CHECK_HRESULT(th->ProcessException_EndFinally());

                _ASSERTE(th->m_currentException.Dereference() == NULL);
                stack = th->CurrentFrame();
                goto Execute_Reload;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LEAVE,                      "leave",            Pop0,               Push0,       InlineBrTarget,     IPrimitive,  1,  0xFF,    0xDD,    BRANCH)
            OPDEF(CEE_LEAVE_S,                    "leave.s",          Pop0,               Push0,       ShortInlineBrTarget,IPrimitive,  1,  0xFF,    0xDE,    BRANCH)
            {
                CLR_INT32 arg;

                EMPTYSTACK(stack, evalPos);
                
                if(op == CEE_LEAVE)
                {
                    TINYCLR_READ_UNALIGNED_INT16( arg, ip );
                }
                else
                {
                    TINYCLR_READ_UNALIGNED_INT8( arg, ip );
                }

                {
                    CLR_PMETADATA           ipLeave = ip + arg;
                    CLR_RT_ExceptionHandler eh;

                    th->PopEH( stack, ipLeave );

                    if(th->FindEhBlock( stack, ip-1, ipLeave, eh, true ))
                    {
                        UnwindStack* us = th->PushEH();
                        _ASSERTE(us);

                        us->m_stack             = stack;
                        us->m_exception         = NULL;
                        us->m_ip                = ipLeave;
                        us->m_currentBlockStart = eh.m_handlerStart;
                        us->m_currentBlockEnd   = eh.m_handlerEnd;
                        us->m_flags             = UnwindStack::p_4_NormalCleanup;

                        ip = eh.m_handlerStart;
                        break;
                    }

                    ip = ipLeave;

                    if(th->m_flags & CLR_RT_Thread::TH_F_Aborted)
                    {                                
                        _ASSERTE(th->m_currentException.Dereference() == NULL);

                        (void)Library_corlib_native_System_Exception::CreateInstance( th->m_currentException, g_CLR_RT_WellKnownTypes.m_ThreadAbortException, S_OK, stack );
                        
                        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
                    }                
                }
                
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CEQ,                        "ceq",              Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x01,    NEXT)
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( CLR_RT_HeapBlock::Compare_Signed_Values( evalPos[ 0 ], evalPos[ 1 ] ) == 0 ? 1 : 0 );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CGT,                        "cgt",              Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x02,    NEXT)
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( CLR_RT_HeapBlock::Compare_Signed_Values( evalPos[ 0 ], evalPos[ 1 ] ) > 0 ? 1 : 0 );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CGT_UN,                     "cgt.un",           Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x03,    NEXT)
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( CLR_RT_HeapBlock::Compare_Unsigned_Values( evalPos[ 0 ], evalPos[ 1 ] ) > 0 ? 1 : 0 );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CLT,                        "clt",              Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x04,    NEXT)
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( CLR_RT_HeapBlock::Compare_Signed_Values( evalPos[ 0 ], evalPos[ 1 ] ) < 0 ? 1 : 0 );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CLT_UN,                     "clt.un",           Pop1+Pop1,          PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x05,    NEXT)
            {
                evalPos--; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( CLR_RT_HeapBlock::Compare_Unsigned_Values( evalPos[ 0 ], evalPos[ 1 ] ) < 0 ? 1 : 0 );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDFTN,                      "ldftn",            Pop0,               PushI,       InlineMethod,       IPrimitive,  2,  0xFE,    0x06,    NEXT)
            {
                FETCH_ARG_COMPRESSED_METHODTOKEN(arg,ip);

                evalPos++; CHECKSTACK(stack,evalPos);

                CLR_RT_MethodDef_Instance method; if(method.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

                UPDATESTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Delegate::CreateInstance( evalPos[ 0 ], method, stack ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_LDVIRTFTN,                  "ldvirtftn",        PopRef,             PushI,       InlineMethod,       IPrimitive,  2,  0xFE,    0x07,    NEXT)
            {
                FETCH_ARG_COMPRESSED_METHODTOKEN(arg,ip);

                CLR_RT_MethodDef_Instance callee; if(callee.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                CLR_RT_TypeDef_Index      cls;

                TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( evalPos[ 0 ], cls ));

                CLR_RT_MethodDef_Index calleeReal;

                if(g_CLR_RT_EventCache.FindVirtualMethod( cls, callee, calleeReal ) == false)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }

                UPDATESTACK(stack,evalPos);

                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Delegate::CreateInstance( evalPos[ 0 ], calleeReal, stack ));
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_INITOBJ,                    "initobj",          PopI,               Push0,       InlineType,         IObjModel,   2,  0xFE,    0x15,    NEXT)
            {
                ip += 2; // Skip argument, not used...

                evalPos->InitObject(); evalPos--; CHECKSTACK(stack,evalPos);
 
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_RETHROW,                    "rethrow",          Pop0,               Push0,       InlineNone,         IObjModel,   2,  0xFE,    0x1A,    THROW)
            {
                if(th->m_nestedExceptionsPos == 0)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_STACK_UNDERFLOW);
                }
                else
                {
                    UnwindStack& us = th->m_nestedExceptions[ --th->m_nestedExceptionsPos ];

                    if(us.m_exception)
                    {
                        th->m_currentException.SetObjectReference( us.m_exception );

                        TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
                    }
                    else
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_STACK_UNDERFLOW);
                    }
                }
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_SIZEOF,                     "sizeof",           Pop0,               PushI,       InlineType,         IPrimitive,  2,  0xFE,    0x1C,    NEXT)
            {
                FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

                CLR_RT_TypeDef_Instance clsInst; if(clsInst.ResolveToken( arg, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                CLR_INT32               len;

                if(clsInst.m_target->dataType)
                {
                    len = sizeof(CLR_RT_HeapBlock);
                }
                else
                {
                    len = (CLR_RT_HeapBlock::HB_Object_Fields_Offset + clsInst.CrossReference().m_totalFields) * sizeof(CLR_RT_HeapBlock);
                }

                evalPos++; CHECKSTACK(stack,evalPos);

                evalPos[ 0 ].SetInteger( len );
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_CONSTRAINED,                "constrained.",     Pop0,               Push0,       InlineType,         IPrefix,     2,  0xFE,    0x16,    META)
            {
                FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

                //nop
                break;
            }

            //----------------------------------------------------------------------------------------------------------//

            OPDEF(CEE_ENDFILTER,                  "endfilter",        PopI,               Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x11,    RETURN)
            {

                CHECKSTACK(stack,evalPos);
                WRITEBACK(stack,evalPos,ip,fDirty);
                
                TINYCLR_CHECK_HRESULT(th->ProcessException_EndFilter());
                
                stack = th->CurrentFrame();
                goto Execute_Reload;
            }

            //----------------------------------------------------------------------------------------------------------//

            //////////////////////////////////////////////////////////////////////////////////////////
            //
            // These opcodes do nothing...
            //
            OPDEF(CEE_NOP,                        "nop",              Pop0,               Push0,       InlineNone,         IPrimitive,  1,  0xFF,    0x00,    NEXT)
            OPDEF(CEE_UNALIGNED,                  "unaligned.",       Pop0,               Push0,       ShortInlineI,       IPrefix,     2,  0xFE,    0x12,    META)
            OPDEF(CEE_VOLATILE,                   "volatile.",        Pop0,               Push0,       InlineNone,         IPrefix,     2,  0xFE,    0x13,    META)
            OPDEF(CEE_TAILCALL,                   "tail.",            Pop0,               Push0,       InlineNone,         IPrefix,     2,  0xFE,    0x14,    META)
                break;

            //////////////////////////////////////////////////////////////////////////////////////////
            //
            // Unsupported opcodes...
            //
            OPDEF(CEE_ARGLIST,                    "arglist",          Pop0,               PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x00,    NEXT)
            OPDEF(CEE_CPBLK,                      "cpblk",            PopI+PopI+PopI,     Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x17,    NEXT)
            OPDEF(CEE_JMP,                        "jmp",              Pop0,               Push0,       InlineMethod,       IPrimitive,  1,  0xFF,    0x27,    CALL)
            OPDEF(CEE_INITBLK,                    "initblk",          PopI+PopI+PopI,     Push0,       InlineNone,         IPrimitive,  2,  0xFE,    0x18,    NEXT)
            OPDEF(CEE_CALLI,                      "calli",            VarPop,             VarPush,     InlineSig,          IPrimitive,  1,  0xFF,    0x29,    CALL)
            OPDEF(CEE_CKFINITE,                   "ckfinite",         Pop1,               PushR8,      InlineNone,         IPrimitive,  1,  0xFF,    0xC3,    NEXT)
            OPDEF(CEE_LOCALLOC,                   "localloc",         PopI,               PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x0F,    NEXT)
            OPDEF(CEE_MKREFANY,                   "mkrefany",         PopI,               Push1,       InlineType,         IPrimitive,  1,  0xFF,    0xC6,    NEXT)
            OPDEF(CEE_REFANYTYPE,                 "refanytype",       Pop1,               PushI,       InlineNone,         IPrimitive,  2,  0xFE,    0x1D,    NEXT)
            OPDEF(CEE_REFANYVAL,                  "refanyval",        Pop1,               PushI,       InlineType,         IPrimitive,  1,  0xFF,    0xC2,    NEXT)
            OPDEF(CEE_READONLY,                   "readonly.",        Pop0,               Push0,       InlineNone,         IPrefix,     2,  0xFE,    0x1E,    META)

            TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyUnsupportedInstruction( op ));
                break;

            //////////////////////////////////////////////////////////////////////////////////////////

            default:
                TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyUnknownInstruction( op ));
                break;
#undef OPDEF
            }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            if(stack->m_flags & CLR_RT_StackFrame::c_HasBreakpoint)
            {
                g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Step( stack, ip );
            }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

            continue;

            //--//

    Execute_LoadAndPromote:

            {
                CLR_RT_HeapBlock& res = evalPos[ 0 ];

                switch(res.DataType())
                {
                case DATATYPE_VOID    :                                                                                               break;

                case DATATYPE_BOOLEAN : res.ChangeDataType( DATATYPE_I4 ); res.NumericByRef().u4 = (CLR_UINT32)res.NumericByRef().u1; break;
                case DATATYPE_I1      : res.ChangeDataType( DATATYPE_I4 ); res.NumericByRef().s4 = (CLR_INT32 )res.NumericByRef().s1; break;
                case DATATYPE_U1      : res.ChangeDataType( DATATYPE_I4 ); res.NumericByRef().u4 = (CLR_UINT32)res.NumericByRef().u1; break;

                case DATATYPE_CHAR    : res.ChangeDataType( DATATYPE_I4 ); res.NumericByRef().u4 = (CLR_UINT32)res.NumericByRef().u2; break;
                case DATATYPE_I2      : res.ChangeDataType( DATATYPE_I4 ); res.NumericByRef().s4 = (CLR_INT32 )res.NumericByRef().s2; break;
                case DATATYPE_U2      : res.ChangeDataType( DATATYPE_I4 ); res.NumericByRef().u4 = (CLR_UINT32)res.NumericByRef().u2; break;

                case DATATYPE_I4      :                                                                                               break;
                case DATATYPE_U4      :                                                                                               break;
                case DATATYPE_R4      :                                                                                               break;

                case DATATYPE_I8      :                                                                                               break;
                case DATATYPE_U8      :                                                                                               break;
                case DATATYPE_R8      :                                                                                               break;
                case DATATYPE_DATETIME:                                                                                               break;
                case DATATYPE_TIMESPAN:                                                                                               break;
                case DATATYPE_STRING  :                                                                                               break;

                case DATATYPE_OBJECT  :
                    if(evalPos[ 0 ].IsAValueType())
                    {
                        UPDATESTACK(stack,evalPos);

                        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.CloneObject( evalPos[ 0 ], evalPos[ 0 ] ));
                    }
                    break;
                }
            }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            if(stack->m_flags & CLR_RT_StackFrame::c_HasBreakpoint)
            {
                g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Step( stack, ip );
            }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            continue;

            //--//

    Execute_BR:
            {
                CLR_INT32 offset;

                if(c_CLR_RT_OpcodeLookup[ op ].m_opParam == CLR_OpcodeParam_ShortBrTarget)
                {
                    TINYCLR_READ_UNALIGNED_INT8( offset, ip );
                }
                else
                {
                    TINYCLR_READ_UNALIGNED_INT16( offset, ip );
                }

                if(fCondition) ip += offset;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
                if(stack->m_flags & CLR_RT_StackFrame::c_HasBreakpoint)
                {
                    g_CLR_RT_ExecutionEngine.Breakpoint_StackFrame_Step( stack, ip );
                }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)                

                if(th->m_timeQuantumExpired)
                {
                    TINYCLR_SET_AND_LEAVE( CLR_S_QUANTUM_EXPIRED );
                }

                continue;
            }

            //--//

    Execute_Restart:
            {
                CLR_RT_StackFrame* stackNext = th->CurrentFrame();

                #if defined(ENABLE_NATIVE_PROFILER)
                if(stackNext->m_flags & CLR_RT_StackFrame::c_NativeProfiled)
                {
                    stackNext->m_owningThread->m_fNativeProfiled = true;
                    Native_Profiler_Start();
                }
                else if(stackNext->m_fNativeProfiled == false)
                {
                    stackNext->m_owningThread->m_fNativeProfiled = false;
                    Native_Profiler_Stop();
                }
                #endif

                if((stackNext->m_flags & CLR_RT_StackFrame::c_CallerIsCompatibleForCall) == 0)
                {
                    TINYCLR_SET_AND_LEAVE( CLR_S_RESTART_EXECUTION );
                }

                //
                // Same kind of handler, no need to pop back out, just restart execution in place.
                //
                stack->m_flags &= ~CLR_RT_StackFrame::c_ExecutingIL;

                stack = stackNext;
                goto Execute_Reload;
            }

    Execute_Reload:
            {
                stack->m_flags |= CLR_RT_StackFrame::c_ExecutingIL;

                assm = stack->m_call.m_assm;

                READCACHE(stack,evalPos,ip,fDirty);
                continue;
            }
        }
    }


    TINYCLR_CLEANUP();

    if(fDirty)
    {
        WRITEBACK(stack,evalPos,ip,fDirty);
    }

    TINYCLR_CLEANUP_END();
}

