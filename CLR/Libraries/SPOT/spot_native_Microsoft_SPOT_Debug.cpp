////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_Debug::Print___STATIC__VOID__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    LPCSTR szText0 = stack.Arg0().RecoverString();

    if(!szText0) szText0 = "<null>";

    CLR_Debug::Emit( szText0, -1 );
    CLR_Debug::Emit( "\r\n" , -1 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Debug::GC___STATIC__U4__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

#if defined(TINYCLR_GC_VERBOSE)
    if(s_CLR_RT_fTrace_GC >= c_CLR_RT_Trace_Info)
    {
        CLR_Debug::Printf( "    Memory: Debug.GC.\r\n" );
    }
#endif

    stack.SetResult_I4( g_CLR_RT_ExecutionEngine.PerformGarbageCollection() );

    if(stack.Arg0().NumericByRefConst().u1)
    {
        //
        // Decrement the number of GC, otherwise the outer loop may request another compaction.
        //
        g_CLR_RT_GarbageCollector.m_numberOfGarbageCollections--;

        g_CLR_RT_ExecutionEngine.PerformHeapCompaction();
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Debug::EnableGCMessages___STATIC__VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

#if defined(TINYCLR_TRACE_MEMORY_STATS)
    s_CLR_RT_fTrace_MemoryStats = stack.Arg0().NumericByRefConst().u1 != 0 ? c_CLR_RT_Trace_Info : c_CLR_RT_Trace_None;
#endif

    TINYCLR_NOCLEANUP_NOLABEL();
}
