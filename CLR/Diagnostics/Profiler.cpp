////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Diagnostics.h"

#if defined(TINYCLR_PROFILE_NEW)

CLR_PRF_Profiler g_CLR_PRF_Profiler;

HRESULT CLR_PRF_Profiler::CreateInstance()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_HEADER();

    g_CLR_PRF_Profiler.m_packetSeqId = 0;
    g_CLR_PRF_Profiler.m_stream = NULL;
    g_CLR_PRF_Profiler.m_lastTimestamp = (CLR_UINT32)((CLR_UINT64)(Time_GetMachineTime() + ((1ull << CLR_PRF_CMDS::Bits::TimestampShift) - 1))
        >> CLR_PRF_CMDS::Bits::TimestampShift);
    g_CLR_PRF_Profiler.m_currentAssembly = 0;
    g_CLR_PRF_Profiler.m_currentThreadPID = 0;
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_MemoryStream::CreateInstance(g_CLR_PRF_Profiler.m_stream, NULL, 0));
    
    TINYCLR_NOCLEANUP();
}

HRESULT CLR_PRF_Profiler::DeleteInstance()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    return g_CLR_PRF_Profiler.Profiler_Cleanup();
}

HRESULT CLR_PRF_Profiler::Profiler_Cleanup()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_RT_HeapBlock_MemoryStream::DeleteInstance(m_stream);
    return S_OK;
}

void CLR_PRF_Profiler::SendMemoryLayout()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    //Send Memory Layout
    m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_Memory_Layout, CLR_PRF_CMDS::Bits::CommandHeader );
    PackAndWriteBits( 0 );
    PackAndWriteBits(s_CLR_RT_Heap.m_size);
    Stream_Send();
}

HRESULT CLR_PRF_Profiler::DumpHeap()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_HEADER();

    CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

    CLR_UINT32 heapSize = 0;

    if(CLR_EE_PRF_IS_NOT(Enabled)) { TINYCLR_SET_AND_LEAVE(S_OK); }

    {
        //Send HeapDump Begin Marker
        m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_HeapDump_Start, CLR_PRF_CMDS::Bits::CommandHeader );
        TINYCLR_CHECK_HRESULT(Stream_Send());
    }

    DumpRoots();

    TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc,g_CLR_RT_ExecutionEngine.m_heap)
    {
        CLR_RT_HeapBlock_Node* ptr;
        CLR_UINT32 size;
        CLR_RT_HeapBlock_Node* end = hc->m_payloadEnd;

        for(ptr = hc->m_payloadStart, size = ptr->DataSize(); ptr < end; ptr+= size, size = ptr->DataSize())
        {
            if(ptr->DataType() != DATATYPE_FREEBLOCK && ptr->DataType() != DATATYPE_CACHEDBLOCK)
            {
                heapSize += ptr->DataSize();
            }
            DumpObject(ptr);
            //Don't let the stream get too big.
            TINYCLR_CHECK_HRESULT(Stream_Send());
        }
    }
    TINYCLR_FOREACH_NODE_END();

    {
        //Send HeapDump End Marker
        m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_HeapDump_Stop, CLR_PRF_CMDS::Bits::CommandHeader );
        PackAndWriteBits( heapSize );
        TINYCLR_CHECK_HRESULT(Stream_Send());
    }

    TINYCLR_CLEANUP();

    //Flush out all data we've collected. Stopping the device without informing the program is a bad idea.
    Stream_Flush();

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_PRF_Profiler::DumpRoots()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_HEADER();

    //Root proto:
    //8bits: CLR_PRF_CMDS::c_Profiling_HeapDump_Root
    //32bits: Address of Root
    //3bits: Source of Root: Finalizer, AppDomain, Assembly, Thread, Stack?
    //Only when source is Stack??: 32bits: CLR_RT_MethodDef_Index

    //Iterate through all the finalizers
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Finalizer,fin,g_CLR_RT_ExecutionEngine.m_finalizersPending)
    {
        _ASSERTE(fin->m_object);
        _ASSERTE(fin->m_object->DataType() != DATATYPE_FREEBLOCK);
        _ASSERTE(fin->m_object->DataType() != DATATYPE_CACHEDBLOCK);

        DumpRoot(fin->m_object, CLR_PRF_CMDS::RootTypes::Root_Finalizer, 0, NULL);
    }
    TINYCLR_FOREACH_NODE_END();

    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Finalizer,fin,g_CLR_RT_ExecutionEngine.m_finalizersAlive)
    {
        _ASSERTE(fin->m_object);
        _ASSERTE(fin->m_object->DataType() != DATATYPE_FREEBLOCK);
        _ASSERTE(fin->m_object->DataType() != DATATYPE_CACHEDBLOCK);

        DumpRoot(fin->m_object, CLR_PRF_CMDS::RootTypes::Root_Finalizer, 0, NULL);
    }
    TINYCLR_FOREACH_NODE_END();

#if defined(TINYCLR_APPDOMAINS)
    //Iterate through all the appdomains
    TINYCLR_FOREACH_NODE(CLR_RT_AppDomain,appDomain,g_CLR_RT_ExecutionEngine.m_appDomains)
    {
        DumpRoot(appDomain, CLR_PRF_CMDS::RootTypes::Root_AppDomain, 0, NULL);
    }
    TINYCLR_FOREACH_NODE_END();
#endif

    //Iterate through all the assemblies.
    TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
    {
        DumpRoot( pASSM, CLR_PRF_CMDS::RootTypes::Root_Assembly, 0, NULL );
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    { //Iterate through all threads.
        CLR_RT_DblLinkedList* threadLists[ 2 ] = { &g_CLR_RT_ExecutionEngine.m_threadsReady, &g_CLR_RT_ExecutionEngine.m_threadsWaiting };
        for(int list = 0; list < 2; list++)
        {
            TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,*threadLists[ list ])
            {
                DumpRoot( th, CLR_PRF_CMDS::RootTypes::Root_Thread, 0, NULL );
            }
            TINYCLR_FOREACH_NODE_END();
        }
    }

    TINYCLR_CHECK_HRESULT(Stream_Send());

    TINYCLR_NOCLEANUP();
}

void CLR_PRF_Profiler::DumpRoot( CLR_RT_HeapBlock* root, CLR_UINT32 type, CLR_UINT32 flags, CLR_RT_MethodDef_Index* source )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_HeapDump_Root, CLR_PRF_CMDS::Bits::CommandHeader );
    DumpPointer( root );
    m_stream->WriteBits( type, CLR_PRF_CMDS::Bits::RootTypes );

    _ASSERTE(!flags);
    if(type == CLR_PRF_CMDS::RootTypes::Root_Stack)
    {
        PackAndWriteBits(*source);
    } else {
        _ASSERTE(source == NULL);
    }
}

void CLR_PRF_Profiler::DumpObject( CLR_RT_HeapBlock* ptr )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    //Object Proto:
    // Free blocked and cached blocks are considered free memory and are not dumped.
    // All other types:
    // 8 bits - Profiling_HeapDump_Object
    // 32 bit pointer
    // 16 bit size
    //  8 bit - DataType() const
    // 32 bits are TypeDef info >>>> iff DataType == CLASSTYPE || DataType == VALUETYPE || DataType == SZARRAY <<<<
    // 16 bits are Array Level info >>>> iff DataType == SZARRAY <<<<
    // 1 bit - Reference Follows
    //    0 - No more references. End of Packet
    //    1 - 32-bit pointer to reference follows. Repeat.

    CLR_DataType dt = ptr->DataType();
    _ASSERTE(dt < DATATYPE_FIRST_INVALID);
    _ASSERTE(sizeof(CLR_RT_HeapBlock) == 12);    //HeapBlockObjectPacket in ProfilerPackets.cs assumes sizeof(CLR_RT_HeapBlock) == 12

    if(dt != DATATYPE_FREEBLOCK && dt != DATATYPE_CACHEDBLOCK)
    {
        m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_HeapDump_Object, CLR_PRF_CMDS::Bits::CommandHeader );
        DumpPointer( ptr );
        PackAndWriteBits( ptr->DataSize() );
        m_stream->WriteBits( (CLR_UINT32)dt,CLR_PRF_CMDS::Bits::DataType );
            
        switch(dt)
        {
            case DATATYPE_BOOLEAN:
            case DATATYPE_I1:
            case DATATYPE_U1:
            case DATATYPE_CHAR:
            case DATATYPE_I2:
            case DATATYPE_U2:
            case DATATYPE_I4:
            case DATATYPE_U4:
            case DATATYPE_R4:
            case DATATYPE_I8:
            case DATATYPE_U8:
            case DATATYPE_R8:
            case DATATYPE_DATETIME:
            case DATATYPE_TIMESPAN:
            case DATATYPE_STRING:
                //Boxed primitives.. All are non-pointer types -- no references.
            case DATATYPE_REFLECTION:
                //Has a reference to an assembly, but it's unlikely to be the only thing keeping the assembly alive.
            case DATATYPE_BINARY_BLOB_HEAD:
                //Unknown/unmanaged data-type. No references.
            case DATATYPE_SUBTHREAD:
                //Owned by a DATATYPE_THREAD; Don't dump back-reference.
            case DATATYPE_MEMORY_STREAM_DATA:
                //No references at all -- usually, and when not, its a pointer to an externally shown character buffer used for deserialization.
            case DATATYPE_IO_PORT:
                //No references for normal GPIO; there is a structure for interrupts, but that doesn't seem to be stored on the managed heap.
            case DATATYPE_TIMER_HEAD:
            case DATATYPE_LOCK_OWNER_HEAD:
            case DATATYPE_LOCK_REQUEST_HEAD:
            case DATATYPE_SERIALIZER_DUPLICATE:
            case DATATYPE_SERIALIZER_STATE:
                //No unique forward-looking references.
                break;

            case DATATYPE_OBJECT:
            case DATATYPE_BYREF:
            case DATATYPE_ARRAY_BYREF:
#if defined(TINYCLR_APPDOMAINS)
            case DATATYPE_TRANSPARENT_PROXY:
#endif
                {
                    DumpSingleReference( ptr );
                    break;
                }

            case DATATYPE_CLASS:
            case DATATYPE_VALUETYPE:
                {
                    CLR_RT_TypeDef_Index idx = ptr->ObjectCls();
                    _ASSERTE(TINYCLR_INDEX_IS_VALID(idx));
                    PackAndWriteBits( idx );
                    DumpSingleReference( ptr->ObjectLock() );
                    DumpListOfReferences( ptr + 1, ptr->DataSize() - 1 ); //All items in list should have DataSize() == 1 therefore ptr->DataSize() - 1 == number of items in list.
                    break;
                }

            case DATATYPE_SZARRAY:
                {
                    //Special case needed to dump out array data type and levels.
                    CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)ptr;
                    
                    PackAndWriteBits( array->ReflectionDataConst().m_data.m_type );
                    PackAndWriteBits( array->ReflectionDataConst().m_levels );
                    
                    if(array->m_fReference)
                    {
                        DumpListOfReferences( (CLR_RT_HeapBlock*)array->GetFirstElement(), array->m_numOfElements );
                    }
                    break;
                }

            case DATATYPE_ASSEMBLY:
                {
                    CLR_RT_Assembly* assembly = (CLR_RT_Assembly*)ptr;
                    DumpSingleReference( assembly->m_pFile );
#if !defined(TINYCLR_APPDOMAINS)
                    DumpListOfReferences( assembly->m_pStaticFields, assembly->m_iStaticFields );
#endif
                    break;
                }

            case DATATYPE_WEAKCLASS:
                {
                    CLR_RT_HeapBlock_WeakReference* wr = (CLR_RT_HeapBlock_WeakReference*)ptr;
                    DumpSingleReference( wr->m_targetDirect );
                    break;
                }

            case DATATYPE_DELEGATE_HEAD:
                {
                    CLR_RT_HeapBlock_Delegate* dlg = (CLR_RT_HeapBlock_Delegate*)ptr;
                    DumpSingleReference( &dlg->m_object );
                    break;
                }

            case DATATYPE_DELEGATELIST_HEAD:
                {
                    CLR_RT_HeapBlock_Delegate_List* dlgList = (CLR_RT_HeapBlock_Delegate_List*)ptr;
                    DumpListOfReferences( dlgList->GetDelegates(), dlgList->m_length );
                    break;
                }

            case DATATYPE_THREAD:
                {
                    CLR_RT_Thread* th = (CLR_RT_Thread*)ptr;
                    
                    DumpSingleReference( th->m_dlg );
                    DumpSingleReference( th->m_currentException.Dereference() );

                    for(int i=0; i<th->m_nestedExceptionsPos; i++)
                    {
                        CLR_RT_HeapBlock* except = th->m_nestedExceptions[ i ].m_exception;
                        _ASSERTE(!except || except->DataType() == DATATYPE_CLASS || except->DataType() == DATATYPE_OBJECT);
                        DumpSingleReference( except );
                    }

                    DumpListOfReferences( th->m_locks);
                    DumpSingleReference ( th->m_waitForObject );
                    DumpListOfReferences( th->m_stackFrames   );
                    DumpListOfReferences( th->m_subThreads    );
                    break;
                }

            case DATATYPE_STACK_FRAME:
                {
                    CLR_RT_StackFrame* stack = (CLR_RT_StackFrame*)ptr;
                    DumpListOfReferences( stack->m_arguments, stack->m_call.m_target->numArgs );
                    DumpListOfReferences( stack->m_locals   , stack->m_call.m_target->numLocals );
                    DumpListOfReferences( stack->m_evalStack, stack->TopValuePosition() );
                    break;
                }

            case DATATYPE_OBJECT_TO_EVENT:
                {
                    CLR_RT_ObjectToEvent_Source* otes = (CLR_RT_ObjectToEvent_Source*)ptr;
                    DumpSingleReference( otes->m_eventPtr );  //The managed object should reference this obj, which references the event.
                    break;
                }

            case DATATYPE_LOCK_HEAD:
                {
                    //Object points to Lock Head, Thread points to Lock Head, Lock Head points to list of lock owners and requests
                    CLR_RT_HeapBlock_Lock* lock = (CLR_RT_HeapBlock_Lock*)ptr;
                    DumpListOfReferences( lock->m_owners   );
                    DumpListOfReferences( lock->m_requests );
                    break;
                }

            case DATATYPE_ENDPOINT_HEAD:
                {
                    CLR_RT_HeapBlock_EndPoint* ep = (CLR_RT_HeapBlock_EndPoint*)ptr;
                    DumpListOfReferences( ep->m_messages );
                    break;
                }

            case DATATYPE_WAIT_FOR_OBJECT_HEAD:
                {
                    CLR_RT_HeapBlock_WaitForObject* wfo = (CLR_RT_HeapBlock_WaitForObject*)ptr;
                    DumpListOfReferences(wfo->GetWaitForObjects(), wfo->m_cObjects);
                    break;
                }

            case DATATYPE_FINALIZER_HEAD:
                {
                    CLR_RT_HeapBlock_Finalizer* f = (CLR_RT_HeapBlock_Finalizer*)ptr;
                    DumpSingleReference( f->m_object );
                    break;
                }

            case DATATYPE_MEMORY_STREAM_HEAD:
                {
                    CLR_RT_HeapBlock_MemoryStream* ms = (CLR_RT_HeapBlock_MemoryStream*)ptr;
                    DumpListOfReferences( ms->m_buffers );
                    break;
                }

            case DATATYPE_SERIALIZER_HEAD:
                {
                    CLR_RT_BinaryFormatter* bf = (CLR_RT_BinaryFormatter*)ptr;
                    DumpSingleReference ( bf->m_stream     );
                    DumpListOfReferences( bf->m_duplicates );
                    DumpListOfReferences( bf->m_states     );
                    break;
                }

            case DATATYPE_I2C_XACTION:
                {
                    CLR_RT_HeapBlock_I2CXAction* ixa = (CLR_RT_HeapBlock_I2CXAction*)ptr;
                    size_t index;

                    //These are unmanaged pointers; we need to get the pointer to the BinaryBlob via
                    if(ixa->m_HalXAction)      { DumpSingleReference( CLR_RT_HeapBlock_BinaryBlob::GetBlob( ixa->m_HalXAction      ) ); }
                    if(ixa->m_dataBuffers)     { DumpSingleReference( CLR_RT_HeapBlock_BinaryBlob::GetBlob( ixa->m_dataBuffers     ) ); }
                    if(ixa->m_HalXActionUnits) { DumpSingleReference( CLR_RT_HeapBlock_BinaryBlob::GetBlob( ixa->m_HalXActionUnits ) ); }
                    for(index = 0; index < ixa->m_xActionUnits; ++index)
                    {            
                        DumpSingleReference( CLR_RT_HeapBlock_BinaryBlob::GetBlob( ixa->m_HalXActionUnits[ index ] ) );
                    }
                    break;
                }
                

#if defined(TINYCLR_APPDOMAINS)
        case DATATYPE_APPDOMAIN_HEAD:
            {
                CLR_RT_AppDomain* appDomain = (CLR_RT_AppDomain*)ptr;
                DumpListOfReferences( appDomain->m_appDomainAssemblies );
                DumpSingleReference ( appDomain->m_globalLock );
                DumpSingleReference ( appDomain->m_strName );
                DumpSingleReference ( appDomain->m_outOfMemoryException );
                break;
            }

        case DATATYPE_APPDOMAIN_ASSEMBLY:
            {
                CLR_RT_AppDomainAssembly* appDomainAssembly = (CLR_RT_AppDomainAssembly*)ptr;
                DumpListOfReferences( appDomainAssembly->m_pStaticFields, appDomainAssembly->m_assembly->m_iStaticFields );
                break;
            }
#endif

        default:
                _ASSERTE(false);
                break;
        }
        DumpEndOfRefsList();
    } //if(dt != DATATYPE_FREEBLOCK && dt != DATATYPE_CACHEDBLOCK)
}

CLR_RT_HeapBlock* CLR_PRF_Profiler::FindReferencedObject(CLR_RT_HeapBlock* ref)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    while(ref)
    {
        CLR_DataType dt = ref->DataType();
        switch(dt)
        {
        case DATATYPE_BYREF:
        case DATATYPE_OBJECT:
            ref = ref->Dereference();
            break;
#if defined(TINYCLR_APPDOMAINS)
        case DATATYPE_TRANSPARENT_PROXY:
            ref = ref->TransparentProxyDereference();
            break;
#endif
        case DATATYPE_ARRAY_BYREF:
            ref = ref->Array();
        default:
            return ref;
        }
    }
    return NULL;
}

void CLR_PRF_Profiler::DumpEndOfRefsList()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    SendFalse();
}

void CLR_PRF_Profiler::DumpPointer(void* ptr)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    PackAndWriteBits( (CLR_UINT32)((CLR_UINT8*)ptr - s_CLR_RT_Heap.m_location) );
}

void CLR_PRF_Profiler::DumpSingleReference(CLR_RT_HeapBlock* ptr)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    ptr = FindReferencedObject(ptr);
    if(ptr)
    {
        SendTrue();
        DumpPointer(ptr);
    }
}

void CLR_PRF_Profiler::DumpListOfReferences(CLR_RT_HeapBlock* firstItem, CLR_UINT16 count)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_RT_HeapBlock* ptr;
    for(ptr = firstItem; count > 0; ptr += ptr->DataSize(), count--)
    {
        DumpSingleReference(ptr);
    }
}

void CLR_PRF_Profiler::DumpListOfReferences(CLR_RT_DblLinkedList& list)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Node, ptr, list)
    {
        DumpSingleReference(ptr);
    }
    TINYCLR_FOREACH_NODE_END();
}

//--//

void CLR_PRF_Profiler::Timestamp()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    //Send Profiling Timestamp
    CLR_UINT32 time = (CLR_UINT32)((Time_GetMachineTime() + ((CLR_UINT64)((1ull << CLR_PRF_CMDS::Bits::TimestampShift) - 1)))
        >> CLR_PRF_CMDS::Bits::TimestampShift);
    if(time > m_lastTimestamp)
    {
        m_stream->WriteBits(CLR_PRF_CMDS::c_Profiling_Timestamp, CLR_PRF_CMDS::Bits::CommandHeader);
        PackAndWriteBits(time - m_lastTimestamp);
        Stream_Send();
        m_lastTimestamp = time;
    }
}

//--//

#if defined(TINYCLR_PROFILE_NEW_CALLS)
HRESULT CLR_PRF_Profiler::RecordContextSwitch(CLR_RT_Thread* nextThread)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_HEADER();
    _ASSERTE(nextThread);

    CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

    if(CLR_EE_PRF_IS(Calls))
    {
        Timestamp();
        m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_Calls_CtxSwitch, CLR_PRF_CMDS::Bits::CommandHeader );
        PackAndWriteBits( nextThread->m_pid );
        m_currentThreadPID = nextThread->m_pid;
        TINYCLR_CHECK_HRESULT(Stream_Send());
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_PRF_Profiler::RecordFunctionCall(CLR_RT_Thread* th, CLR_RT_MethodDef_Index md)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_HEADER();
    _ASSERTE(th);

    if(CLR_EE_PRF_IS(Calls))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        if(th->m_pid != m_currentThreadPID)
        {
            TINYCLR_CHECK_HRESULT(RecordContextSwitch( th ));
        }
        else
        {
            Timestamp();
        }

        _ASSERTE(th->m_pid == m_currentThreadPID);

        m_stream->WriteBits(CLR_PRF_CMDS::c_Profiling_Calls_Call, CLR_PRF_CMDS::Bits::CommandHeader);

        if(md.Assembly() == m_currentAssembly)
        {
            SendFalse();
        }
        else
        {
            SendTrue();
            m_currentAssembly = md.Assembly();
            PackAndWriteBits(m_currentAssembly);
        }
        PackAndWriteBits(md.Method());
        TINYCLR_CHECK_HRESULT(Stream_Send());
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_PRF_Profiler::RecordFunctionReturn(CLR_RT_Thread* th, CLR_PROF_CounterCallChain& prof)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_HEADER();
    _ASSERTE(th);

    if(CLR_EE_PRF_IS(Calls))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        if(th->m_pid != m_currentThreadPID)
        {
            TINYCLR_CHECK_HRESULT(RecordContextSwitch( th ));
        }
        else
        {
            Timestamp();
        }

        _ASSERTE(th->m_pid == m_currentThreadPID);

        m_stream->WriteBits(CLR_PRF_CMDS::c_Profiling_Calls_Return, CLR_PRF_CMDS::Bits::CommandHeader);
        PackAndWriteBits((CLR_UINT32)(prof.m_time_exclusive >> CLR_PRF_CMDS::Bits::CallTimingShift));
        TINYCLR_CHECK_HRESULT(Stream_Send());
    }

    TINYCLR_NOCLEANUP();
}
#endif

//--//

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)

void CLR_PRF_Profiler::TrackObjectCreation( CLR_RT_HeapBlock* ptr )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    _ASSERTE(ptr);
    if(CLR_EE_PRF_IS(Allocations))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        CLR_UINT8 dt = ptr->DataType();
        if(dt != DATATYPE_STACK_FRAME && dt != DATATYPE_BINARY_BLOB_HEAD)
        {
            Timestamp();
            m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_Allocs_Alloc, CLR_PRF_CMDS::Bits::CommandHeader );
            DumpPointer( ptr );
            PackAndWriteBits( ptr->DataSize() );
            m_stream->WriteBits( (CLR_UINT32)dt, CLR_PRF_CMDS::Bits::DataType );
            if(dt == DATATYPE_CLASS || dt == DATATYPE_VALUETYPE)
            {
                PackAndWriteBits( ptr->ObjectCls() );
            }
            else if(dt == DATATYPE_SZARRAY)
            {
                CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)ptr;
                PackAndWriteBits( array->ReflectionDataConst().m_data.m_type );
                PackAndWriteBits( array->ReflectionDataConst().m_levels );
            }
            Stream_Send();
        }
    }
}

void CLR_PRF_Profiler::TrackObjectDeletion( CLR_RT_HeapBlock* ptr )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    _ASSERTE(ptr);

    if(CLR_EE_PRF_IS(Allocations))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        CLR_UINT8 dt = ptr->DataType();
        if(dt != DATATYPE_STACK_FRAME && dt != DATATYPE_CACHEDBLOCK )
        {
            Timestamp();
            m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_Allocs_Delete, CLR_PRF_CMDS::Bits::CommandHeader );
            DumpPointer( ptr );
            Stream_Send();
        }
    }
}

void CLR_PRF_Profiler::TrackObjectRelocation()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    if(CLR_EE_PRF_IS(Allocations))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        CLR_RT_GarbageCollector::RelocationRegion* relocBlocks = g_CLR_RT_GarbageCollector.m_relocBlocks;
        size_t relocCount = g_CLR_RT_GarbageCollector.m_relocCount;

        Timestamp();
        m_stream->WriteBits(CLR_PRF_CMDS::c_Profiling_Allocs_Relloc, CLR_PRF_CMDS::Bits::CommandHeader);
        PackAndWriteBits(( CLR_UINT32)relocCount );

        for(size_t i = 0; i < relocCount; i++)
        {
            DumpPointer( relocBlocks[ i ].m_start);
            DumpPointer( relocBlocks[ i ].m_end );
            PackAndWriteBits( relocBlocks[i].m_offset );
        }
    }
}

void CLR_PRF_Profiler::RecordGarbageCollectionBegin()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    if(CLR_EE_PRF_IS(Allocations))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        Timestamp();
        m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_GarbageCollect_Begin, CLR_PRF_CMDS::Bits::CommandHeader );
        PackAndWriteBits( g_CLR_RT_GarbageCollector.m_freeBytes );
        Stream_Send();
    }
}

void CLR_PRF_Profiler::RecordGarbageCollectionEnd()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    if(CLR_EE_PRF_IS(Allocations))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        Timestamp();
        m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_GarbageCollect_End, CLR_PRF_CMDS::Bits::CommandHeader );
        PackAndWriteBits( g_CLR_RT_GarbageCollector.m_freeBytes );
        Stream_Send();
    }
}

void CLR_PRF_Profiler::RecordHeapCompactionBegin()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    if(CLR_EE_PRF_IS(Allocations))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        Timestamp();
        m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_HeapCompact_Begin, CLR_PRF_CMDS::Bits::CommandHeader );
        PackAndWriteBits( g_CLR_RT_GarbageCollector.m_freeBytes );
        Stream_Send();
    }
}

void CLR_PRF_Profiler::RecordHeapCompactionEnd()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    if(CLR_EE_PRF_IS(Allocations))
    {
        CLR_PROF_HANDLER_CALLCHAIN_VOID(perf);

        Timestamp();
        m_stream->WriteBits( CLR_PRF_CMDS::c_Profiling_HeapCompact_End, CLR_PRF_CMDS::Bits::CommandHeader );
        PackAndWriteBits( g_CLR_RT_GarbageCollector.m_freeBytes );
        Stream_Send();
    }
}

#endif

//--//

void CLR_PRF_Profiler::SendTrue()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    m_stream->WriteBits( (CLR_UINT32)1, 1 );
}

void CLR_PRF_Profiler::SendFalse()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    m_stream->WriteBits( (CLR_UINT32)0, 1 );
}

void CLR_PRF_Profiler::PackAndWriteBits(CLR_UINT32 value)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    /* In three different scenarios tested, sending nibbles instead of bytes and 2-bit hunks resulted
     * in the most efficiency over the wire. It also involves less code than 2-bit hunks that I expected to win,
     * so that's the method used.
     */
    _ASSERTE(sizeof(CLR_UINT32) == 4);

    const CLR_UINT32 SHIFT_PER_NIBBLE = 2; //2^2 = 4 bits per nibble
    CLR_UINT32 nibbles = 1;
    if(     value & 0xF0000000) { nibbles = 8; }
    else if(value & 0x0F000000) { nibbles = 7; }
    else if(value & 0x00F00000) { nibbles = 6; }
    else if(value & 0x000F0000) { nibbles = 5; }
    else if(value & 0x0000F000) { nibbles = 4; }
    else if(value & 0x00000F00) { nibbles = 3; }
    else if(value & 0x000000F0) { nibbles = 2; }

    m_stream->WriteBits( nibbles - 1, CLR_PRF_CMDS::Bits::NibbleCount );
    m_stream->WriteBits( value, nibbles << SHIFT_PER_NIBBLE );
}

void CLR_PRF_Profiler::PackAndWriteBits(const CLR_RT_TypeDef_Index& typeDef)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    PackAndWriteBits( typeDef.Assembly() );
    PackAndWriteBits( typeDef.Type() );
}

void CLR_PRF_Profiler::PackAndWriteBits(const CLR_RT_MethodDef_Index& methodDef)
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    PackAndWriteBits( methodDef.Assembly() );
    PackAndWriteBits( methodDef.Method() );
}

//--//

HRESULT CLR_PRF_Profiler::Stream_Send()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_HEADER();

    const int BUFFER_THRESHOLD_BYTES = CLR_RT_HeapBlock_MemoryStream::Buffer::c_PayloadSize * 2 / 3;
    const int BUFFER_THRESHOLD_BITS  = BUFFER_THRESHOLD_BYTES << 3;

    if(m_stream->BitsWritten() >= BUFFER_THRESHOLD_BITS)
    {
        TINYCLR_CHECK_HRESULT(Stream_Flush());
    } 
    //else { Stream isn't above threshold; allow to cache. }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_PRF_Profiler::Stream_Flush()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    TINYCLR_HEADER();

    //These need to be paired; If this function is ever used to send multiple stream types, each should get their own packet sequence id.
    const CLR_UINT32 messageType = CLR_DBG_Commands::c_Profiling_Stream;

    CLR_UINT8 buffer[ 2*sizeof(CLR_UINT16) + CLR_RT_HeapBlock_MemoryStream::Buffer::c_PayloadSize ];
    CLR_DBG_Commands::Profiling_Stream* packet = (CLR_DBG_Commands::Profiling_Stream*)buffer;

    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_MemoryStream::Buffer, ptr, m_stream->m_buffers)
    {
        int payloadLength = ptr->m_length;
        if(payloadLength > 0)
        {
            _ASSERTE(sizeof(ptr->m_payload) == CLR_RT_HeapBlock_MemoryStream::Buffer::c_PayloadSize);

            int bitLength = payloadLength * 8;
            if(ptr == m_stream->m_current)
            {
                bitLength -= m_stream->m_avail;
            }

            packet->m_bitLen = bitLength;
            packet->m_seqId = m_packetSeqId++;
            memcpy(&packet[ 1 ], ptr->m_payload, payloadLength);

            int packetLength = sizeof(CLR_DBG_Commands::Profiling_Stream) + payloadLength;
            if(!CLR_EE_DBG_EVENT_SEND(messageType, packetLength, buffer, WP_Flags::c_NonCritical))
            {
                _ASSERTE(FALSE);
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }

        //Don't go past the cursor.
        if(ptr == m_stream->m_current) { break; }
    }
    TINYCLR_FOREACH_NODE_END();

    m_stream->Reset();

    TINYCLR_NOCLEANUP();
}

#endif //#if defined(TINYCLR_PROFILE_NEW)
