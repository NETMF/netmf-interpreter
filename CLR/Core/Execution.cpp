////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "Core.h" 

////////////////////////////////////////////////////////////////////////////////////////////////////

static const CLR_INT64 c_MaximumTimeToActive = (TIME_CONVERSION__ONEMINUTE * TIME_CONVERSION__TO_SECONDS);


//--//

CLR_RT_ExecutionEngine::ExecutionConstraintCompensation CLR_RT_ExecutionEngine::s_compensation = { 0, 0, 0 };

//--//

HRESULT CLR_RT_ExecutionEngine::CreateInstance()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_CLEAR(g_CLR_RT_ExecutionEngine);

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.ExecutionEngine_Initialize());

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::ExecutionEngine_Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    m_maximumTimeToActive = c_MaximumTimeToActive;  // CLR_INT64                           m_maximumTimeToActive
                                                    // int                                 m_iDebugger_Conditions;
                                                    //
                                                    // CLR_INT64                           m_currentMachineTime;
                                                    // CLR_INT64                           m_currentLocalTime;
    m_lastTimeZoneOffset =  Time_GetTimeZoneOffset();// CLR_INT32                           m_lastTimeZoneOffset;

                                                    // CLR_INT64                           m_currentNextActivityTime;
    m_timerCache    = false;                        // bool                                m_timerCache;
                                                    // CLR_INT64                           m_timerCacheNextTimeout;
                                                    //
    m_heap          .DblLinkedList_Initialize();    // CLR_RT_DblLinkedList                m_heap;
                                                    // CLR_RT_HeapCluster*                 m_lastHcUsed;
    m_heapState     = c_HeapState_Normal;           // int                                 m_heapState;
                                                    //
    m_weakReferences.DblLinkedList_Initialize();    // CLR_RT_DblLinkedList                m_weakReferences;
                                                    //
    m_timers        .DblLinkedList_Initialize();    // CLR_RT_DblLinkedList                m_timers;
    m_raisedEvents  = 0;                            // CLR_UINT32                          m_raisedEvents;
                                                    //
    m_threadsReady  .DblLinkedList_Initialize();    // CLR_RT_DblLinkedList                m_threadsReady;
    m_threadsWaiting.DblLinkedList_Initialize();    // CLR_RT_DblLinkedList                m_threadsWaiting;
    m_threadsZombie .DblLinkedList_Initialize();    // CLR_RT_DblLinkedList                m_threadsZombie;
                                                    // int                                 m_lastPid;
                                                    //
    m_finalizersAlive  .DblLinkedList_Initialize(); // CLR_RT_DblLinkedList                m_finalizersAlive;
    m_finalizersPending.DblLinkedList_Initialize(); // CLR_RT_DblLinkedList                m_finalizersPending;
                                                    // CLR_RT_Thread*                      m_finalizerThread;
                                                    // CLR_RT_Thread*                      m_cctorThread;
                                                    //
#if !defined(TINYCLR_APPDOMAINS)
    m_globalLock           = NULL;                  // CLR_RT_HeapBlock*                   m_globalLock;
    m_outOfMemoryException = NULL;                  // CLR_RT_HeapBlock*                   m_outOfMemoryException;
#endif                                              //

    m_currentUICulture     = NULL;                  // CLR_RT_HeapBlock*                   m_currentUICulture;

#if defined(CLR_COMPONENTIZATION_USE_HANDLER)
    Handler_Initialize();
#else
    CLR_RT_HeapBlock_EndPoint::HandlerMethod_Initialize(); 
    CLR_RT_HeapBlock_NativeEventDispatcher::HandlerMethod_Initialize();
    CLR_RT_HeapBlock_I2CXAction::HandlerMethod_Initialize();
      

#endif

    m_interruptThread     = NULL;                   // CLR_RT_Thread                       m_interruptThread;
                                                    //
#if defined(TINYCLR_JITTER)
                                                    // const FLASH_SECTOR*                 m_jitter_firstSector;
                                                    // int                                 m_jitter_numSectors;
                                                    //
                                                    // FLASH_WORD*                         m_jitter_current;
                                                    // FLASH_WORD*                         m_jitter_end;
#endif
                                                    //
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    m_scratchPadArray = NULL;                       // CLR_RT_HeapBlock_Array*             m_scratchPadArray;
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    

#if defined(TINYCLR_APPDOMAINS)
    m_appDomains.DblLinkedList_Initialize();        // CLR_RT_DblLinkedList                m_appDomains;
    
    m_appDomainCurrent = NULL;                      // CLR_AppDomainCurrent*               m_appDomainCurrent;    
    m_appDomainIdNext  = c_AppDomainId_Invalid + 1; // int                                 m_appDomainIdNext;
#endif

    m_currentThread    = NULL;

    m_GlobalExecutionCounter = 0;

#if !defined(BUILD_RTM)
    m_fShuttingDown = false;                        //bool                                m_fShuttingDown;
#endif

    //--//

    TINYCLR_CHECK_HRESULT(AllocateHeaps());

    g_CLR_RT_TypeSystem.TypeSystem_Initialize();
    g_CLR_RT_EventCache.EventCache_Initialize();

    //--//

    TINYCLR_CHECK_HRESULT(CLR_HW_Hardware::CreateInstance());

    //--//

    TINYCLR_CHECK_HRESULT(CLR_Messaging::CreateInstance());
    
    TINYCLR_CHECK_HRESULT(CLR_DBG_Debugger::CreateInstance());

#if defined(TINYCLR_PROFILE_NEW)
    TINYCLR_CHECK_HRESULT(CLR_PRF_Profiler::CreateInstance());
#endif

#if defined(TINYCLR_APPDOMAINS)
    TINYCLR_CHECK_HRESULT(CLR_RT_AppDomain::CreateInstance( "default", m_appDomainCurrent ));
#endif

    UpdateTime();

    m_startTime = Time_GetUtcTime();

    CLR_RT_HeapBlock_WeakReference::RecoverObjects( m_heap );

    //--//

#if defined(TINYCLR_JITTER)
    {
        int                 numSectors;
        const FLASH_SECTOR* pSectors;

        if(SUCCEEDED(::Flash_EnumerateSectors( numSectors, pSectors )))
        {
            while(numSectors--)
            {
                if((pSectors->Usage & MEMORY_USAGE_MASK) == MEMORY_USAGE_JITTER)
                {
                    FLASH_WORD* start =                                                            pSectors->Start;
                    FLASH_WORD* end   = CLR_RT_Persistence_Manager::Bank::IncrementPointer( start, pSectors->Length );

                    if(::Flash_IsSectorErased( pSectors ) == FALSE)
                    {
                        ::Flash_ChipReadOnly( FALSE    );
                        ::Flash_EraseSector ( pSectors );
                        ::Flash_ChipReadOnly( TRUE     );

                        if(::Flash_IsSectorErased( pSectors ) == FALSE)
                        {
                            break;
                        }
                    }

                    if(m_jitter_current == NULL)
                    {
                        m_jitter_firstSector = pSectors;
                        m_jitter_numSectors  = 1;

                        m_jitter_current     = start;
                        m_jitter_end         = end;
                    }
                    else if(m_jitter_end != start)
                    {
                        //
                        // Only allow contiguous sectors.
                        //
                        break;
                    }
                    else
                    {
                        m_jitter_numSectors++;
                        m_jitter_end = end;
                    }
                }

                pSectors++;
            }
        }
    }
#endif

    //--//

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::AllocateHeaps()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const CLR_UINT32 c_HeapClusterSize = sizeof(CLR_RT_HeapBlock) * CLR_RT_HeapBlock::HB_MaxSize;

    CLR_UINT8* heapFirstFree = s_CLR_RT_Heap.m_location;
    CLR_UINT32 heapFree      = s_CLR_RT_Heap.m_size;
    CLR_INT32  i             = 0;
    CLR_UINT32 blockSize     = 1;

    if(heapFree <= sizeof(CLR_RT_HeapCluster))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_MEMORY);
    }

    while(heapFree > sizeof(CLR_RT_HeapCluster))
    {
        CLR_RT_HeapCluster* hc   = (CLR_RT_HeapCluster*)                                 heapFirstFree;
        CLR_UINT32          size =                      (heapFree < c_HeapClusterSize) ? heapFree : c_HeapClusterSize;

        ///
        /// Speed up heap initialization for devices with very large heaps > 1MB
        /// Exponentially increase the size of a default heap block
        ///
        if(i > 100*1024*1024)
        {
            blockSize = CLR_RT_HeapBlock::HB_MaxSize;
        }
        else if( i > 10*1024*1024)
        {
            blockSize = 10*1024;
        }
        else if(i > 1024*1024)
        {
            blockSize = 1*1024;
        }

        hc->HeapCluster_Initialize( size, blockSize );

        m_heap.LinkAtBack( hc );

        heapFirstFree += size;
        heapFree      -= size;
        i             += size;
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::DeleteInstance()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    g_CLR_RT_ExecutionEngine.ExecutionEngine_Cleanup();

    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_RT_ExecutionEngine::ExecutionEngine_Cleanup()
{
    NATIVE_PROFILE_CLR_CORE();
    m_fShuttingDown = true;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    m_scratchPadArray = NULL;
    m_breakpointsNum = 0;

    CLR_DBG_Debugger::DeleteInstance();
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if defined(TINYCLR_PROFILE_NEW)
    CLR_PRF_Profiler::DeleteInstance();
#endif

    CLR_Messaging::DeleteInstance();

    CLR_HW_Hardware::DeleteInstance();

    m_finalizersAlive  .DblLinkedList_PushToCache();
    m_finalizersPending.DblLinkedList_PushToCache();
    m_finalizerThread = NULL;
    m_cctorThread = NULL;

    m_timerThread = NULL;

    AbortAllThreads  ( m_threadsReady   );
    AbortAllThreads  ( m_threadsWaiting );

    ReleaseAllThreads( m_threadsReady   );
    ReleaseAllThreads( m_threadsWaiting );
    ReleaseAllThreads( m_threadsZombie  );

    g_CLR_RT_TypeSystem.TypeSystem_Cleanup();
    g_CLR_RT_EventCache.EventCache_Cleanup();

#if !defined(TINYCLR_APPDOMAINS)
    m_globalLock = NULL;
#endif

#if defined(CLR_COMPONENTIZATION_USE_HANDLER)
    Handler_CleanUp();
#else
    CLR_RT_HeapBlock_EndPoint::HandlerMethod_CleanUp(); 
    CLR_RT_HeapBlock_NativeEventDispatcher::HandlerMethod_CleanUp();
    CLR_RT_HeapBlock_I2CXAction::HandlerMethod_CleanUp();
#endif

    m_interruptThread = NULL;    

    m_heap.DblLinkedList_Initialize();
}

//--//

HRESULT CLR_RT_ExecutionEngine::StartHardware()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(g_CLR_HW_Hardware.Hardware_Initialize());

    TINYCLR_NOCLEANUP();
}

void CLR_RT_ExecutionEngine::Reboot( bool fHard )
{
    NATIVE_PROFILE_CLR_CORE();
    
    ::Watchdog_GetSetEnabled( FALSE, TRUE );

    g_CLR_RT_Persistence_Manager.Flush();
    g_CLR_RT_Persistence_Manager.m_state = CLR_RT_Persistence_Manager::STATE_FlushNextObject;
    g_CLR_RT_Persistence_Manager.m_pending_object = NULL;
    g_CLR_RT_Persistence_Manager.Flush();

    if(fHard)
    {
        ::CPU_Reset();
    }
    else
    {
        CLR_EE_REBOOT_SET(ClrOnly);
        CLR_EE_DBG_SET(RebootPending);
    }
}

CLR_INT64 CLR_RT_ExecutionEngine::GetUptime()
{
    return Time_GetUtcTime() - g_CLR_RT_ExecutionEngine.m_startTime;
}

void CLR_RT_ExecutionEngine::JoinAllThreadsAndExecuteFinalizer()
{
}

void CLR_RT_ExecutionEngine::LoadDownloadedAssemblies()
{
    NATIVE_PROFILE_CLR_CORE ();
    PerformGarbageCollection();
    PerformHeapCompaction   ();

    //
    // Load any patch or similar!
    //
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_WeakReference,weak,m_weakReferences)
    {
        if((weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_ArrayOfBytes) != 0 && weak->m_targetSerialized)
        {
            CLR_RECORD_ASSEMBLY* header;

            header = (CLR_RECORD_ASSEMBLY*)weak->m_targetSerialized->GetFirstElement();

            if(header->GoodAssembly())
            {
                CLR_RT_Assembly* assm = NULL;

                if(SUCCEEDED(CLR_RT_Assembly::CreateInstance( header, assm )))
                {
                    assm->m_pFile = weak->m_targetSerialized;

                    g_CLR_RT_TypeSystem.Link( assm );
                }
            }
        }
    }
    TINYCLR_FOREACH_NODE_END();

    (void)g_CLR_RT_TypeSystem.ResolveAll();

    TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
    {
        if(pASSM->m_pFile)
        {
            //
            // For those assemblies that failed to load (missing dependency?), clean up.
            //
            if((pASSM->m_flags & CLR_RT_Assembly::c_ResolutionCompleted) == 0)
            {
                pASSM->m_pFile = NULL;

                pASSM->DestroyInstance();
            }
        }
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    g_CLR_RT_TypeSystem.PrepareForExecution();
}

//--//

void CLR_RT_ExecutionEngine::ExecutionConstraint_Suspend()
{
    NATIVE_PROFILE_CLR_CORE();
    s_compensation.Suspend();
}

void CLR_RT_ExecutionEngine::ExecutionConstraint_Resume()
{
    NATIVE_PROFILE_CLR_CORE();
    s_compensation.Resume();
}

//--//

CLR_UINT32 CLR_RT_ExecutionEngine::PerformGarbageCollection()
{
    NATIVE_PROFILE_CLR_CORE();
    m_heapState = c_HeapState_UnderGC;

    CLR_UINT32 freeMem = g_CLR_RT_GarbageCollector.ExecuteGarbageCollection();

    m_heapState = c_HeapState_Normal;

    m_lastHcUsed = NULL;

#if !defined(BUILD_RTM) || defined(_WIN32)
    if(m_fPerformHeapCompaction) CLR_EE_SET( Compaction_Pending );
#endif

    g_CLR_RT_ExecutionEngine.SpawnFinalizer();

    return freeMem;
}

void CLR_RT_ExecutionEngine::PerformHeapCompaction()
{
    NATIVE_PROFILE_CLR_CORE();
    if(CLR_EE_DBG_IS( NoCompaction )) return;

    g_CLR_RT_GarbageCollector.ExecuteCompaction();

    CLR_EE_CLR( Compaction_Pending );

    m_lastHcUsed = NULL;
}

void CLR_RT_ExecutionEngine::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_scratchPadArray );
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if !defined(TINYCLR_APPDOMAINS)
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_globalLock           );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_outOfMemoryException );
#endif

    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_currentUICulture     );

    m_weakReferences.Relocate();

    g_CLR_RT_Persistence_Manager.Relocate();
}

//--//

#if defined(TINYCLR_APPDOMAINS)

void CLR_RT_ExecutionEngine::TryToUnloadAppDomains_Helper_Threads( CLR_RT_DblLinkedList& threads )
{        
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,threads)
    {
        if(th->m_flags & CLR_RT_Thread::TH_F_ContainsDoomedAppDomain)
        {
            TINYCLR_FOREACH_NODE(CLR_RT_StackFrame, stack, th->m_stackFrames)
            {
                stack->m_appDomain->m_fCanBeUnloaded = false;                
            }
            TINYCLR_FOREACH_NODE_END();
        }
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_ExecutionEngine::TryToUnloadAppDomains_Helper_Finalizers( CLR_RT_DblLinkedList& finalizers, bool fAlive )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Finalizer,fin,finalizers)
    {
        if(!fin->m_appDomain->IsLoaded())
        {
            if(fAlive)
            {
                //When an AppDomain is being unloaded, all live finalizers are run, regardless
                //of whether or not they are still reachable.
                m_finalizersPending.LinkAtBack( fin );
            }

            fin->m_appDomain->m_fCanBeUnloaded = false;
        }
    }
    TINYCLR_FOREACH_NODE_END();
}

bool CLR_RT_ExecutionEngine::TryToUnloadAppDomains()
{
    NATIVE_PROFILE_CLR_CORE();
    bool fAnyAppDomainsUnloaded = false;

    TINYCLR_FOREACH_NODE(CLR_RT_AppDomain,appDomain,m_appDomains)
    {
        appDomain->m_fCanBeUnloaded = true;
    }
    TINYCLR_FOREACH_NODE_END();

    TryToUnloadAppDomains_Helper_Finalizers( m_finalizersAlive  , true  );
    TryToUnloadAppDomains_Helper_Finalizers( m_finalizersPending, false );
            
    TryToUnloadAppDomains_Helper_Threads( m_threadsReady   );
    TryToUnloadAppDomains_Helper_Threads( m_threadsWaiting );

    CLR_EE_CLR( UnloadingAppDomain );
    
    TINYCLR_FOREACH_NODE(CLR_RT_AppDomain,appDomain,m_appDomains)
    {
        if(appDomain->m_state == CLR_RT_AppDomain::AppDomainState_Unloading)
        {
            if(appDomain->m_fCanBeUnloaded)
            {                                
                appDomain->m_state = CLR_RT_AppDomain::AppDomainState_Unloaded;
                appDomain->AppDomain_Uninitialize();
                fAnyAppDomainsUnloaded = true;
            }
            else
            {
                CLR_EE_SET(UnloadingAppDomain);
            }
        }
    }
    TINYCLR_FOREACH_NODE_END();

    if(fAnyAppDomainsUnloaded)
    {                      
        SignalEvents( CLR_RT_ExecutionEngine::c_Event_AppDomain );
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        Breakpoint_Assemblies_Loaded();
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    }

    return fAnyAppDomainsUnloaded;
}

#endif

HRESULT CLR_RT_ExecutionEngine::WaitForDebugger()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    while(CLR_EE_DBG_IS(Stopped) && !CLR_EE_DBG_IS(RebootPending) && !CLR_EE_DBG_IS(ExitPending))
    {
        // TODO: Generalize this as a standard HAL API
#if defined(WIN32)
        if(HAL_Windows_IsShutdownPending())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_SHUTTING_DOWN);
        }
#endif

        DebuggerLoop();
    }

#if defined(WIN32)
    TINYCLR_NOCLEANUP();
#else
    TINYCLR_NOCLEANUP_NOLABEL();
#endif 
}

#if defined(WIN32)
HRESULT CLR_RT_ExecutionEngine::CreateEntryPointArgs( CLR_RT_HeapBlock& argsBlk, WCHAR* szCommandLineArgs )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    std::list<std::wstring> args;

    WCHAR* szArgNext = NULL;
    WCHAR* szArg     = szCommandLineArgs;
    WCHAR* sep       = L" ";
    WCHAR* context   = NULL;
    
    szArg = wcstok_s( szArg, sep, &context );
    
    while(szArg != NULL)
    {
        std::wstring arg = szArg;                
        args.insert( args.end(), arg );
        
        szArg = wcstok_s( NULL, sep, &context );
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( argsBlk, (CLR_UINT32)args.size(), g_CLR_RT_WellKnownTypes.m_String ));
    
    CLR_RT_HeapBlock_Array* array = argsBlk.Array();
    CLR_UINT32 iArg = 0;

    for(std::list<std::wstring>::iterator it = args.begin(); it != args.end(); it++, iArg++)
    {
        std::string arg;

        CLR_RT_HeapBlock* blk = (CLR_RT_HeapBlock*)array->GetElement( iArg );
        CLR_RT_UnicodeHelper::ConvertToUTF8( (*it).c_str(), arg );

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( *blk, arg.c_str() ));
    }    
    
    TINYCLR_NOCLEANUP();
}

#endif

HRESULT CLR_RT_ExecutionEngine::Execute( LPWSTR entryPointArgs, int maxContextSwitch )
{            
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock ref;
    CLR_RT_Thread*   thMain = NULL;

        
    if(TINYCLR_INDEX_IS_INVALID(g_CLR_RT_TypeSystem.m_entryPoint))
    {
#if !defined(BUILD_RTM) || defined(WIN32)
        CLR_Debug::Printf( "Cannot find any entrypoint!\r\n" );
#endif
        TINYCLR_SET_AND_LEAVE(CLR_E_ENTRYPOINT_NOT_FOUND);
    }

    TINYCLR_CHECK_HRESULT(WaitForDebugger());

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    CLR_EE_DBG_SET_MASK(State_ProgramRunning,State_Mask);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
 
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Delegate::CreateInstance( ref, g_CLR_RT_TypeSystem.m_entryPoint, NULL ));

    {
        CLR_RT_ProtectFromGC gc( ref );

        TINYCLR_CHECK_HRESULT(NewThread( thMain, ref.DereferenceDelegate(), ThreadPriority::Normal, -1 ));
    }

    {            
        CLR_RT_StackFrame* stack = thMain->CurrentFrame();
        
        if(stack->m_call.m_target->numArgs > 0)
        {                                
            //Main entrypoint takes an optional String[] parameter.
            //Set the arg to NULL, if that's the case.

#if defined(WIN32)
            if(entryPointArgs != NULL)
            {
                TINYCLR_CHECK_HRESULT(CreateEntryPointArgs( stack->m_arguments[ 0 ], entryPointArgs ));
            }
            else
#endif
            {
                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( stack->m_arguments[ 0 ], 0, g_CLR_RT_WellKnownTypes.m_String ));
            }
        }
    }

    //To debug static constructors, the thread should be created after the entrypoint thread.
    TINYCLR_CHECK_HRESULT(WaitForDebugger());
    
    // m_cctorThread is NULL before call and inialized by the SpawnStaticConstructor
    SpawnStaticConstructor( m_cctorThread );


    while(true)
    {
        HRESULT hr2 = ScheduleThreads( maxContextSwitch ); TINYCLR_CHECK_HRESULT(hr2);
        
        if(CLR_EE_DBG_IS( RebootPending ) || CLR_EE_DBG_IS( ExitPending ) || CLR_EE_REBOOT_IS(ClrOnly))
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(CLR_EE_DBG_IS( Stopped ))
        {
            CLR_RT_ExecutionEngine::ExecutionConstraint_Suspend();

            TINYCLR_CHECK_HRESULT(WaitForDebugger());

            CLR_RT_ExecutionEngine::ExecutionConstraint_Resume();
        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

        if(CLR_EE_IS(Compaction_Pending))
        {            
            PerformHeapCompaction();
            _ASSERTE(FIMPLIES(CLR_EE_DBG_IS_NOT(NoCompaction), CLR_EE_IS_NOT(Compaction_Pending)));
        }
                                
        if(hr2 == CLR_S_NO_READY_THREADS)
        {
            WaitForActivity();
        }
        else if(hr2 == CLR_S_QUANTUM_EXPIRED)
        {
#if !defined(BUILD_RTM) || defined(WIN32)
            if(m_fPerformGarbageCollection)
            {
#if defined(TINYCLR_GC_VERBOSE)
                if(s_CLR_RT_fTrace_GC >= c_CLR_RT_Trace_Info)
                {
                    CLR_Debug::Printf( "    Memory: Forcing GC.\r\n" );
                }
#endif
                PerformGarbageCollection();

#if defined(TINYCLR_GC_VERBOSE)
                if(s_CLR_RT_fTrace_Memory > c_CLR_RT_Trace_Info)
                {
                    CLR_UINT32 inUse = g_CLR_RT_GarbageCollector.m_totalBytes - g_CLR_RT_GarbageCollector.m_freeBytes;

                    CLR_Debug::Printf( "    Memory: INUSE: %d\r\n", (int)inUse );
                }
#endif
            }
#endif
        }
        else
        {
            break;           
        }
    }

    TINYCLR_CLEANUP();

#if defined(TINYCLR_PROFILE_NEW)
    /* g_CLR_RT_ExecutionEngine.Cleanup() gets called too late to flush things out on the emulator since it
     * only gets called when the finalizer dealing with the managed half of the emulator runs.
     */
    g_CLR_PRF_Profiler.Stream_Flush();
#endif

    g_CLR_RT_Persistence_Manager.Flush();

#if defined(WIN32)
#if defined(TINYCLR_PROFILE_NEW)
    if(CLR_EE_PRF_IS( Enabled ))
    {
        //Clients do not get all the messages they want if a program happens to end and the emulator terminates before
        //the pipe can be read; furthermore the emulator must be able to persist until all required data is requested.
        CLR_EE_DBG_SET(Stopped);
    }
#endif

    //By skipping the whole CLRStartup routine, the Monitor_Program_Exit message never gets sent to clients.
    CLR_EE_DBG_EVENT_BROADCAST(CLR_DBG_Commands::c_Monitor_ProgramExit,0,NULL,WP_Flags::c_NonCritical);
    WaitForDebugger();
#endif

    TINYCLR_CLEANUP_END();
}

bool CLR_RT_ExecutionEngine::EnsureSystemThread( CLR_RT_Thread*& thread, int priority )
{
    NATIVE_PROFILE_CLR_CORE();

    if(thread == NULL)
    {
        return SUCCEEDED(NewThread( thread, NULL, priority, -1, CLR_RT_Thread::TH_F_System ));
    }
    else
    {
        return thread->CanThreadBeReused();
    }
}

void CLR_RT_ExecutionEngine::SpawnTimer()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Timer,timer,m_timers)
    {
        if(timer->m_flags & CLR_RT_HeapBlock_Timer::c_Triggered)
        {               
            if(EnsureSystemThread( m_timerThread, ThreadPriority::Normal ))
            {
                //only fire one timer at a time

                timer->SpawnTimer( m_timerThread );

                //put at the back of the queue to allow for fairness of the timers.
                m_timers.LinkAtBack( timer );
            }
            break;
        }
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_ExecutionEngine::StaticConstructorTerminationCallback( void* arg )
{
    NATIVE_PROFILE_CLR_CORE();
    g_CLR_RT_ExecutionEngine.SpawnStaticConstructor( g_CLR_RT_ExecutionEngine.m_cctorThread );
}

#if defined(TINYCLR_APPDOMAINS)
bool CLR_RT_ExecutionEngine::SpawnStaticConstructorHelper( CLR_RT_AppDomain* appDomain, CLR_RT_AppDomainAssembly* appDomainAssembly, const CLR_RT_MethodDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_MethodDef_Index idxNext;

    _ASSERTE(m_cctorThread != NULL);
    //_ASSERTE(m_cctorThread->CanThreadBeReused());  

    idxNext.m_data = idx.m_data;

    _ASSERTE(appDomainAssembly != NULL);

    //find next method with static constructor
    if(appDomainAssembly->m_assembly->FindNextStaticConstructor( idxNext ))
    {                                
        CLR_RT_HeapBlock_Delegate* dlg; 
        CLR_RT_HeapBlock     refDlg; refDlg.SetObjectReference( NULL );    
        CLR_RT_ProtectFromGC gc( refDlg );
        
        if(SUCCEEDED(CLR_RT_HeapBlock_Delegate::CreateInstance( refDlg, idxNext, NULL )))
        {
            dlg = refDlg.DereferenceDelegate();
            dlg->m_appDomain = appDomain;

            if(SUCCEEDED(m_cctorThread->PushThreadProcDelegate( dlg )))
            {                                
                m_cctorThread->m_terminationCallback = CLR_RT_ExecutionEngine::StaticConstructorTerminationCallback;

                return true;
            }
        }
    }

    appDomainAssembly->m_flags |= CLR_RT_AppDomainAssembly::c_StaticConstructorsExecuted;
    return false;
}

void CLR_RT_ExecutionEngine::SpawnStaticConstructor( CLR_RT_Thread *&pCctorThread )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Delegate* dlg = NULL;

    if(!EnsureSystemThread(pCctorThread, ThreadPriority::System_Highest ))  
        return;

    dlg = pCctorThread->m_dlg;

    if(dlg != NULL)
    {
        CLR_RT_AppDomainAssembly* appDomainAssembly;        
        CLR_RT_MethodDef_Index idx = dlg->DelegateFtn();
        CLR_RT_MethodDef_Instance inst;

        //Find next static constructor for given idx
        _ASSERTE(TINYCLR_INDEX_IS_VALID(idx));
        _SIDE_ASSERTE(inst.InitializeFromIndex( idx ));

        appDomainAssembly = dlg->m_appDomain->FindAppDomainAssembly( inst.m_assm );
                
        _ASSERTE(appDomainAssembly != NULL);
        _ASSERTE(appDomainAssembly->m_assembly == inst.m_assm );
    
        //This is ok if idx is no longer valid.  SpawnStaticConstructorHelper will call FindNextStaticConstructor
        //which will fail        
        idx.m_data++;

        //This is not the first static constructor run in this appDomain
        if(SpawnStaticConstructorHelper( dlg->m_appDomain, appDomainAssembly, idx ))
            return;
    }

    //first, find the AppDomainAssembly to run. (what about appdomains!!!)
    TINYCLR_FOREACH_NODE(CLR_RT_AppDomain,appDomain,g_CLR_RT_ExecutionEngine.m_appDomains)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_AppDomainAssembly,appDomainAssembly,appDomain->m_appDomainAssemblies)
        {
            CLR_RT_Assembly* assembly = appDomainAssembly->m_assembly;

            //Find an AppDomainAssembly that does not have it's static constructor bit set...
            if((appDomainAssembly->m_flags & CLR_RT_AppDomainAssembly::c_StaticConstructorsExecuted) == 0)
            {                                
                CLR_RT_MethodDef_Index idx; idx.Set( assembly->m_idx, 0 );

#ifdef DEBUG
                
                //Check that all dependent assemblies have had static constructors run.                    
                CLR_RT_AssemblyRef_CrossReference* ar = assembly->m_pCrossReference_AssemblyRef;
                for(int i=0; i<assembly->m_pTablesSize[ TBL_AssemblyRef ]; i++, ar++)
                {
                    CLR_RT_AppDomainAssembly* appDomainAssemblyRef = appDomain->FindAppDomainAssembly(ar->m_target);
                    
                    _ASSERTE(appDomainAssemblyRef != NULL);
                    _ASSERTE(appDomainAssemblyRef->m_flags & CLR_RT_AppDomainAssembly::c_StaticConstructorsExecuted);
                }
#endif
                        

                if(SpawnStaticConstructorHelper( appDomain, appDomainAssembly, idx ))
                    return;
            }
        }
        TINYCLR_FOREACH_NODE_END();
    }
    TINYCLR_FOREACH_NODE_END();


    // No more static constructors needed...
    // Perform 1 action:
    // 1. Destroy constructor thread.
    pCctorThread->DestroyInstance();
}
#else //TINYCLR_APPDOMAINS

bool CLR_RT_ExecutionEngine::SpawnStaticConstructorHelper( CLR_RT_Assembly* assembly, const CLR_RT_MethodDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_MethodDef_Index idxNext;

    _ASSERTE(m_cctorThread != NULL);
    _ASSERTE(m_cctorThread->CanThreadBeReused());  

    idxNext.m_data = idx.m_data;

    _ASSERTE(assembly != NULL);

    //find next method with static constructor
    if(assembly->FindNextStaticConstructor( idxNext ))
    {                                
        CLR_RT_HeapBlock_Delegate* dlg; 
        CLR_RT_HeapBlock     refDlg; refDlg.SetObjectReference( NULL );    
        CLR_RT_ProtectFromGC gc( refDlg );
        
        if(SUCCEEDED(CLR_RT_HeapBlock_Delegate::CreateInstance( refDlg, idxNext, NULL )))
        {
            dlg = refDlg.DereferenceDelegate();

            if(SUCCEEDED(m_cctorThread->PushThreadProcDelegate( dlg )))
            {                                
                m_cctorThread->m_terminationCallback = CLR_RT_ExecutionEngine::StaticConstructorTerminationCallback;

                return true;
            }
        }
    }

    assembly->m_flags |= CLR_RT_Assembly::c_StaticConstructorsExecuted;
    return false;
}

void CLR_RT_ExecutionEngine::SpawnStaticConstructor( CLR_RT_Thread *&pCctorThread )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Delegate* dlg = NULL;

    if(!EnsureSystemThread(pCctorThread, ThreadPriority::System_Highest))
        return;

    dlg = pCctorThread->m_dlg;

    if(dlg != NULL)
    {               
        CLR_RT_MethodDef_Index idx = dlg->DelegateFtn();
        CLR_RT_MethodDef_Instance inst;

        //Find next static constructor for given idx
        _ASSERTE(TINYCLR_INDEX_IS_VALID(idx));
        _SIDE_ASSERTE(inst.InitializeFromIndex( idx ));
    
        //This is ok if idx is no longer valid.  SpawnStaticConstructorHelper will call FindNextStaticConstructor
        //which will fail        
        idx.m_data++;

        if(SpawnStaticConstructorHelper( inst.m_assm, idx ))
            return;
    }

    //first, find the AppDomainAssembly to run. (what about appdomains!!!)
     TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
    {
        //Find an AppDomainAssembly that does not have it's static constructor bit set...
        if((pASSM->m_flags & CLR_RT_Assembly::c_StaticConstructorsExecuted) == 0)
        {                                
            CLR_RT_MethodDef_Index idx; idx.Set( pASSM->m_idx, 0 );
            bool fDepedenciesRun = true;

            //Check that all dependent assemblies have had static constructors run.                    
            CLR_RT_AssemblyRef_CrossReference* ar = pASSM->m_pCrossReference_AssemblyRef;
            for(int i=0; i<pASSM->m_pTablesSize[ TBL_AssemblyRef ]; i++, ar++)
            {
                if((ar->m_target->m_flags & CLR_RT_Assembly::c_StaticConstructorsExecuted) == 0)
                {
                    fDepedenciesRun = true;
                    break;
                }
            }
                    
            if(fDepedenciesRun && SpawnStaticConstructorHelper( pASSM, idx ))
                return;
        }
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    //no more static constructors needed...
    pCctorThread->DestroyInstance();
}
#endif //TINYCLR_APPDOMAINS

void CLR_RT_ExecutionEngine::FinalizerTerminationCallback(void* arg)
{
    NATIVE_PROFILE_CLR_CORE();
    g_CLR_RT_ExecutionEngine.SpawnFinalizer();
}

void CLR_RT_ExecutionEngine::SpawnFinalizer()
{    
    NATIVE_PROFILE_CLR_CORE();
        
    CLR_RT_HeapBlock_Finalizer* fin = (CLR_RT_HeapBlock_Finalizer*)m_finalizersPending.FirstNode();
    if(fin->Next() != NULL)
    {
        CLR_RT_HeapBlock     delegate; delegate.SetObjectReference( NULL );
        CLR_RT_ProtectFromGC gc( delegate );

#if defined(TINYCLR_APPDOMAINS)
        (void)SetCurrentAppDomain( fin->m_appDomain );
#endif

        if(EnsureSystemThread(m_finalizerThread, ThreadPriority::BelowNormal))
        {
            if(SUCCEEDED(CLR_RT_HeapBlock_Delegate::CreateInstance( delegate, fin->m_md, NULL )))
            {
                CLR_RT_HeapBlock_Delegate* dlg = delegate.DereferenceDelegate();

                dlg->m_object.SetObjectReference( fin->m_object );

                if(SUCCEEDED(m_finalizerThread->PushThreadProcDelegate( dlg )))
                {
                    g_CLR_RT_EventCache.Append_Node( fin );
                    m_finalizerThread->m_terminationCallback = CLR_RT_ExecutionEngine::FinalizerTerminationCallback;
                }       
            }
        }
    }
}

void CLR_RT_ExecutionEngine::AdjustExecutionCounter( CLR_RT_DblLinkedList &threadList, int iUpdateValue )

{   // Iterate over threads in increase executioin counter by iUpdateValue    
    TINYCLR_FOREACH_NODE(CLR_RT_Thread,pThread,threadList)
    {
        pThread->m_executionCounter += iUpdateValue;
        // Update m_executionCounter if thread is too behind of m_GlobalExecutionCounter
        pThread->BringExecCounterToDate( m_GlobalExecutionCounter, pThread->GetQuantumDebit() );
    }
    TINYCLR_FOREACH_NODE_END()
}

void CLR_RT_ExecutionEngine::UpdateToLowestExecutionCounter( CLR_RT_Thread *pThread ) const 
{
    // Set the m_executionCounter in thread to lowest value among ready threads.
    // Thus it will be executed last.
    pThread->m_executionCounter  = m_GlobalExecutionCounter - 1;
}

void CLR_RT_ExecutionEngine::RetrieveCurrentMethod( CLR_UINT32& assmIdx, CLR_UINT32& methodIdx )
{
    assmIdx   = 0;
    methodIdx = 0;

    if(m_currentThread != NULL)
    {
        CLR_RT_StackFrame* stack = m_currentThread->CurrentFrame();

        if(stack)
        {
            assmIdx   = stack->m_call.Assembly();
            methodIdx = stack->m_call.Method  ();
        }
    }
}

void CLR_RetrieveCurrentMethod( CLR_UINT32 & assmIdx, CLR_UINT32 & methodIdx )
{
    g_CLR_RT_ExecutionEngine.RetrieveCurrentMethod( assmIdx, methodIdx );
}

void CLR_SoftReboot()
{
    CLR_EE_DBG_SET( RebootPending );
}

void CLR_DebuggerBreak()
{
    if(g_CLR_RT_ExecutionEngine.m_currentThread != NULL)
    {
        CLR_RT_HeapBlock *obj = g_CLR_RT_ExecutionEngine.m_currentThread->m_currentException.Dereference();

        ///
        /// Only inject the exception once -- if the dereference is not null then the exception is already set on the current thread
        ///
        if(obj == NULL)
        {        
            Library_corlib_native_System_Exception::CreateInstance( g_CLR_RT_ExecutionEngine.m_currentThread->m_currentException, g_CLR_RT_WellKnownTypes.m_WatchdogException, CLR_E_WATCHDOG_TIMEOUT, g_CLR_RT_ExecutionEngine.m_currentThread->CurrentFrame() );
        }
    }
}
    
HRESULT CLR_RT_ExecutionEngine::ScheduleThreads( int maxContextSwitch )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

#if defined(TINYCLR_APPDOMAINS)                    
    CLR_RT_AppDomain* appDomainSav = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();
#endif

    // We run threads based on the m_executionCounter. 
    // The thread with highest m_executionCounter is chosen for execution.
    // The highest value of m_executionCounter of any thread is stored in m_GlobalExecutionCounter
    // We need to check that m_GlobalExecutionCounter does not underflow ( runs below - -2147483647 ) This would be very rare condition, but it may happen.
    // We put threshold at - 0x40000000
    if ( m_GlobalExecutionCounter < -EXECUTION_COUNTER_MAXIMUM )
    {   // Iterate over threads in all queues and bump the execution counter by MAX_EXECUTION_COUNTER_ADJUSTMENT 

        m_GlobalExecutionCounter += EXECUTION_COUNTER_ADJUSTMENT;

        // For each list of threads runs over it and updates execution counter. 
        // AdjustExecutionCounter gets const & to list of threads. 
        // List of threads is not modified, but m_executionCounter is bumped up in each thread.

        AdjustExecutionCounter( m_threadsReady,   EXECUTION_COUNTER_ADJUSTMENT );
        AdjustExecutionCounter( m_threadsWaiting, EXECUTION_COUNTER_ADJUSTMENT );
        AdjustExecutionCounter( m_threadsZombie,  EXECUTION_COUNTER_ADJUSTMENT );
    }
    
    while(maxContextSwitch-- > 0)
    {
        
#if defined(WIN32)
        if(HAL_Windows_IsShutdownPending())
        {
            TINYCLR_SET_AND_LEAVE(CLR_S_NO_THREADS);
        }
#endif

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(CLR_EE_DBG_IS( Stopped ))
        {
            TINYCLR_SET_AND_LEAVE(CLR_S_NO_READY_THREADS);
        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)        

        CLR_RT_Thread* th = NULL;

        //  If a static constructor thread exists, we should be running it.
        //  What about func-eval?
        if (m_cctorThread == NULL)  
        {
            // This is normal case execution. Looks for first ready thread. 
            th = (CLR_RT_Thread*)m_threadsReady.FirstNode();  
        }
        else //If a static constructor thread exists, we should be running it.
        {
            //  This is special case executed during initialization of static constructors.
            if (m_cctorThread->m_status == CLR_RT_Thread::TH_S_Ready  && !(m_cctorThread->m_flags & CLR_RT_Thread::TH_F_Suspended))
            {
                th = m_cctorThread;
            }
            else
            {
                // The m_cctorThread is exists, but not ready - means entered blocking call.
                // We do not want to preempt constructor thread, so stay idle.
                TINYCLR_SET_AND_LEAVE(CLR_S_NO_READY_THREADS); 
            }
        }

        

        
        // If th->Next() is NULL, then there are no Ready to run threads in the system.
        // In this case we spawn finalizer and make finalizer thread as ready one.
        if(th->Next() == NULL)
        {
            g_CLR_RT_ExecutionEngine.SpawnFinalizer();
            
            // Now finalizer thread might be in ready state if there are object that need call to finalizer.
            // th might point to finilizer thread.
            th = (CLR_RT_Thread*)m_threadsReady.FirstNode();

            //Thread create can cause stopping debugging event
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            if(CLR_EE_DBG_IS(Stopped))
            {
                TINYCLR_SET_AND_LEAVE(CLR_S_NO_READY_THREADS);
            }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        }

        // If there is ready thread - decrease m_executionCounter for this (th) thread. 
        if( th->Next() != NULL )
        {
            // The value to update m_executionCounter for each run. See comment for GetQuantumDebit for possible values
            int debitForEachRun = th->GetQuantumDebit();

            // If thread is way too much behind on its execution, we cutt off extra credit.
            // We garantee the thread will not be scheduled more than 4 consequitive times.
            th->BringExecCounterToDate( m_GlobalExecutionCounter, debitForEachRun );
            
            // Substruct the execution counter by debit value ( for executing thread )
            th->m_executionCounter -= debitForEachRun;

            // Keep the track of lowest execution counter. 
            if ( th->m_executionCounter < m_GlobalExecutionCounter )
            {
                m_GlobalExecutionCounter = th->m_executionCounter;
            }
        }
        else
        {
            if(m_threadsWaiting.IsEmpty())
            {
                TINYCLR_SET_AND_LEAVE(CLR_S_NO_THREADS);
            }

            TINYCLR_SET_AND_LEAVE(CLR_S_NO_READY_THREADS);
        }

        ::Watchdog_ResetCounter();

        {
            // Runs the tread until expiration of its quantum or until thread is blocked.
            hr = th->Execute();
        }

        if(FAILED(hr))
        {
            switch(hr)
            {
            case CLR_E_RESCHEDULE:
                break;

            case CLR_E_THREAD_WAITING:
                th->m_status = CLR_RT_Thread::TH_S_Waiting;
                break;

            default:
                th->m_status = CLR_RT_Thread::TH_S_Terminated;
                break;
            }
        }

        ::Watchdog_ResetCounter();

        PutInProperList( th );
        
        UpdateTime();

        (void)ProcessTimer();
    }

    TINYCLR_SET_AND_LEAVE(CLR_S_QUANTUM_EXPIRED);

    TINYCLR_CLEANUP();

#if defined(TINYCLR_APPDOMAINS)                    
                
    if(CLR_EE_IS( UnloadingAppDomain ))
    {
        if(TryToUnloadAppDomains())
        {
            //If we are successful in unloading an AppDomain, return CLR_S_QUANTUM_EXPIRED
            //to cause ScheduleThreads to be called again.  This allows the somewhat expensive operation
            //of trying to unload an AppDomain once every ScheduleThread call, rather than once every context switch            

            hr = CLR_S_QUANTUM_EXPIRED;
        }
    }

    g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );
#endif

    TINYCLR_CLEANUP_END();
}

CLR_UINT32 CLR_RT_ExecutionEngine::WaitForActivity()
{
    NATIVE_PROFILE_CLR_CORE();

    UpdateTime();

    CLR_INT64 timeoutMin = ProcessTimer();

    if(m_threadsReady.IsEmpty() == false) return 0; // Someone woke up...

    if(timeoutMin > 0LL)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,m_threadsWaiting)
        {
            if((th->m_waitForEvents & c_Event_IdleCPU) != 0 && th->m_waitForEvents_IdleTimeWorkItem < timeoutMin)
            {
                th->m_waitForEvents_IdleTimeWorkItem = TIMEOUT_ZERO;

                th->Restart( true );

                return SYSTEM_EVENT_FLAG_ALL; // Someone woke up...
            }
        }
        TINYCLR_FOREACH_NODE_END();

        return WaitForActivity( SLEEP_LEVEL__SLEEP, g_CLR_HW_Hardware.m_wakeupEvents, timeoutMin );
    }

    return 0;
}

CLR_UINT32 CLR_RT_ExecutionEngine::WaitForActivity( CLR_UINT32 powerLevel, CLR_UINT32 events, CLR_INT64 timeout_ms )
{
    NATIVE_PROFILE_CLR_CORE();
 
    if(powerLevel != CLR_HW_Hardware::PowerLevel__Active)
    {
        return WaitSystemEvents( powerLevel, events, timeout_ms );
    }
    
    return 0;
}

//--//

void CLR_RT_ExecutionEngine::PutInProperList( CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_CORE();
    switch(th->m_status)
    {
    case CLR_RT_Thread::TH_S_Ready:
        if((th->m_flags & CLR_RT_Thread::TH_F_Suspended) == 0)
        {
            InsertThreadRoundRobin( m_threadsReady, th );
            break;
        }
        //
        // Fall-through...
        //
    case CLR_RT_Thread::TH_S_Waiting:
        m_threadsWaiting.LinkAtBack( th );
        break;

    case CLR_RT_Thread::TH_S_Terminated:
        th->Passivate();
        break;

    case CLR_RT_Thread::TH_S_Unstarted:
        m_threadsZombie.LinkAtFront( th );
        break;
    }
}

void CLR_RT_ExecutionEngine::AbortAllThreads( CLR_RT_DblLinkedList& threads )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,threads)
    {
        if((th->m_flags & CLR_RT_Thread::TH_F_Aborted) == 0)
        {
            th->Abort();

            TINYCLR_FOREACH_NODE_RESTART(CLR_RT_Thread,th,threads);
        }
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_ExecutionEngine::ReleaseAllThreads( CLR_RT_DblLinkedList& threads )
{
    NATIVE_PROFILE_CLR_CORE();
    while(true)
    {
        CLR_RT_Thread* th = (CLR_RT_Thread*)threads.ExtractFirstNode(); if(!th) break;

        th->DestroyInstance();
    }
}

void CLR_RT_ExecutionEngine::InsertThreadRoundRobin( CLR_RT_DblLinkedList& threads, CLR_RT_Thread* thTarget )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_Thread* th;

    thTarget->Unlink();

    if(threads.IsEmpty())
    {
        th = (CLR_RT_Thread*)threads.Tail();
    }
    else
    {
        int priTarget = thTarget->GetExecutionCounter();

        TINYCLR_FOREACH_NODE__NODECL(CLR_RT_Thread,th,threads)
        {
            if(th->GetExecutionCounter() < priTarget) break;
        }
        TINYCLR_FOREACH_NODE_END();
    }

    thTarget->m_waitForEvents         = 0;
    thTarget->m_waitForEvents_Timeout = TIMEOUT_INFINITE;

    if(thTarget->m_waitForObject != NULL)
    {
        g_CLR_RT_EventCache.Append_Node( thTarget->m_waitForObject );
                    
        thTarget->m_waitForObject = NULL;
    }

    threads.InsertBeforeNode( th, thTarget );
}

//--//

HRESULT CLR_RT_ExecutionEngine::NewThread( CLR_RT_Thread*& thRes, CLR_RT_HeapBlock_Delegate* pDelegate, int priority, CLR_INT32 id, CLR_UINT32 flags  )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(CLR_RT_Thread::CreateInstance( id != -1 ? id : ++m_lastPid, pDelegate, priority, thRes, flags ));

    PutInProperList( thRes );

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(thRes)
        {
            thRes->DestroyInstance();

            thRes = NULL;
        }
    }

    TINYCLR_CLEANUP_END();
}
                                                              
CLR_INT32 CLR_RT_ExecutionEngine::GetNextThreadId() 
{
    return ++m_lastPid; 
}    

//--//

CLR_RT_HeapBlock* CLR_RT_ExecutionEngine::ExtractHeapBlocksForArray( CLR_RT_TypeDef_Instance& inst, CLR_UINT32 length, const CLR_RT_ReflectionDef_Index& reflex )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DataType                 dt  = (CLR_DataType)inst.m_target->dataType;
    const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ dt ];

    CLR_UINT32 totLength = (CLR_UINT32)(sizeof(CLR_RT_HeapBlock_Array) + length * dtl.m_sizeInBytes);
    CLR_UINT32 lengthHB = CONVERTFROMSIZETOHEAPBLOCKS(totLength);

    if(lengthHB > CLR_RT_HeapBlock::HB_MaxSize) return NULL;

    CLR_RT_HeapBlock_Array* pArray = (CLR_RT_HeapBlock_Array*)ExtractHeapBlocks( m_heap, DATATYPE_SZARRAY, 0, lengthHB );

    if(pArray)
    {
        pArray->ReflectionData() =  reflex;
        pArray->m_numOfElements  =  length;

        pArray->m_typeOfElement  =  dt;
        pArray->m_sizeOfElement  =  dtl.m_sizeInBytes;
        pArray->m_fReference     = (dtl.m_flags & CLR_RT_DataTypeLookup::c_Numeric) == 0;

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
        g_CLR_PRF_Profiler.TrackObjectCreation( pArray );
#endif
    }

    return pArray;
}

CLR_RT_HeapBlock* CLR_RT_ExecutionEngine::ExtractHeapBlocksForClassOrValueTypes( CLR_UINT32 dataType, CLR_UINT32 flags, const CLR_RT_TypeDef_Index& cls, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    if(length > CLR_RT_HeapBlock::HB_MaxSize) return NULL;

    _ASSERTE(dataType == DATATYPE_CLASS || dataType == DATATYPE_VALUETYPE);

    flags = flags | CLR_RT_HeapBlock::HB_InitializeToZero;
    CLR_RT_HeapBlock* hb = ExtractHeapBlocks( m_heap, dataType, flags, length );

    if(hb)
    {
        hb->SetObjectCls(cls);

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
        g_CLR_PRF_Profiler.TrackObjectCreation( hb );
#endif
    }

    return hb;
}

CLR_RT_HeapBlock* CLR_RT_ExecutionEngine::ExtractHeapBytesForObjects( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    return ExtractHeapBlocksForObjects( dataType, flags, CONVERTFROMSIZETOHEAPBLOCKS(length) );
}

CLR_RT_HeapBlock* CLR_RT_ExecutionEngine::ExtractHeapBlocksForObjects( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    if(length > CLR_RT_HeapBlock::HB_MaxSize) return NULL;

    _ASSERTE(dataType != DATATYPE_CLASS && dataType != DATATYPE_VALUETYPE && dataType != DATATYPE_SZARRAY);

    flags &= ~CLR_RT_HeapBlock::HB_Alive;

    CLR_RT_HeapBlock* hb = ExtractHeapBlocks( m_heap, dataType, flags, length );

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
    if(hb)
    {
        g_CLR_PRF_Profiler.TrackObjectCreation( hb );
    }
#endif

    return hb;
}

//--//

CLR_RT_HeapBlock_Node* CLR_RT_ExecutionEngine::ExtractHeapBlocksForEvents( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    if(length > CLR_RT_HeapBlock::HB_MaxSize) return NULL;

    flags |= CLR_RT_HeapBlock::HB_Alive | CLR_RT_HeapBlock::HB_Event;

    CLR_RT_HeapBlock_Node* hb = (CLR_RT_HeapBlock_Node*)ExtractHeapBlocks( m_heap, dataType, flags, length );

    if(hb)
    {
        hb->GenericNode_Initialize();

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
        g_CLR_PRF_Profiler.TrackObjectCreation( hb );
#endif
    }

    return hb;
}

CLR_RT_HeapBlock* CLR_RT_ExecutionEngine::ExtractHeapBlocks( CLR_RT_DblLinkedList& heap, CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
#if !defined(BUILD_RTM)
    if(m_heapState == c_HeapState_UnderGC && ((flags & CLR_RT_HeapBlock::HB_SpecialGCAllocation) == 0))
    {
        CLR_Debug::Printf( "Internal error: call to memory allocation during garbage collection!!!\r\n" );

        // Getting here during a GC is possible, since the watchdog ISR may now require
        // dynamic memory allocation for logging.  Returning NULL means the watchdog log will 
        // be lost, but without major restructuring there is not much we can do.
        return NULL;
    }
#endif

#if defined(TINYCLR_FORCE_GC_BEFORE_EVERY_ALLOCATION)
    if(m_heapState != c_HeapState_UnderGC)
    {
        g_CLR_RT_EventCache.EventCache_Cleanup();
        PerformGarbageCollection();
    }
#endif

    for(int phase=0; ; phase++)
    {
        {
            CLR_RT_HeapBlock* hb;

            if(flags & CLR_RT_HeapBlock::HB_Event)
            {
                TINYCLR_FOREACH_NODE_BACKWARD(CLR_RT_HeapCluster,hc,heap)
                {
                    hb = hc->ExtractBlocks( dataType, flags, length );
                    if(hb)
                    {
                        return hb;
                    }
                }
                TINYCLR_FOREACH_NODE_BACKWARD_END();
            }
            else
            {
                if(m_lastHcUsed != NULL)
                {
                    hb = m_lastHcUsed->ExtractBlocks( dataType, flags, length );
                    if(hb)
                    {
                        return hb;
                    }
                }

                TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc,heap)
                {
                    hb = hc->ExtractBlocks( dataType, flags, length );
                    if(hb)
                    {
#if defined(TINYCLR_GC_VERBOSE)
                        if(s_CLR_RT_fTrace_Memory >= c_CLR_RT_Trace_Info)
                        {
                            if(phase != 0)
                            {
                                CLR_Debug::Printf( "ExtractHeapBlocks succeeded at phase %d\r\n", phase );
                            }
                        }
#endif
                        m_lastHcUsed = hc;
                        return hb;
                    }
                }
                TINYCLR_FOREACH_NODE_END();
            }

            m_lastHcUsed = NULL;
        }

        if(flags & CLR_RT_HeapBlock::HB_NoGcOnFailedAllocation)
        {
            return NULL;
        }

        switch(phase)
        {
        case 0:
#if defined(TINYCLR_GC_VERBOSE)
            if(s_CLR_RT_fTrace_Memory >= c_CLR_RT_Trace_Info)
            {
                CLR_Debug::Printf( "    Memory: ExtractHeapBlocks: %d bytes needed.\r\n", length * sizeof(CLR_RT_HeapBlock) );
            }
#endif

            PerformGarbageCollection();

            break;

        default: // Total failure...
#if !defined(BUILD_RTM)
            CLR_Debug::Printf( "Failed allocation for %d blocks, %d bytes\r\n\r\n", length, length * sizeof(CLR_RT_HeapBlock) );
#endif
            if(g_CLR_RT_GarbageCollector.m_freeBytes >= (length * sizeof(CLR_RT_HeapBlock)))
            {
                
                //A compaction probably would have saved this OOM
                //Compaction will occur for Bitmaps, Arrays, etc. if this function returns NULL, so lets not
                //through an assert here

                //Throw the OOM, and schedule a compaction at a safe point
                CLR_EE_SET( Compaction_Pending );
            }

            return NULL;
        }
    }
}

   
CLR_RT_HeapBlock* CLR_RT_ExecutionEngine::AccessStaticField( const CLR_RT_FieldDef_Index& fd )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_FieldDef_Instance inst;

    if(inst.InitializeFromIndex( fd ) && inst.m_target->flags & CLR_RECORD_FIELDDEF::FD_Static)
    {
#if defined(TINYCLR_APPDOMAINS)        
        {
            CLR_RT_AppDomainAssembly* appDomainAssembly = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->FindAppDomainAssembly( inst.m_assm );

            if(appDomainAssembly)
            {
                return &appDomainAssembly->m_pStaticFields[ inst.CrossReference().m_offset ];
            }
        }
#else
        return &inst.m_assm->m_pStaticFields[ inst.CrossReference().m_offset ];
#endif
    }

    return NULL;
}

HRESULT CLR_RT_ExecutionEngine::InitializeReference( CLR_RT_HeapBlock& ref, CLR_RT_SignatureParser& parser )
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // WARNING!!!
    //
    // If you change this method, change "CLR_RT_ExecutionEngine::InitializeLocals" too.
    //

    TINYCLR_HEADER();

    CLR_RT_SignatureParser::Element res;
    CLR_DataType                    dt;

    TINYCLR_CHECK_HRESULT(parser.Advance( res ));

    dt = res.m_dt;

    if(res.m_levels > 0) // Array
    {
        dt = DATATYPE_OBJECT;
    }
    else
    {
        if(dt == DATATYPE_VALUETYPE)
        {
            CLR_RT_TypeDef_Instance inst; inst.InitializeFromIndex( res.m_cls );

            if((inst.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_Enum)
            {
                dt = (CLR_DataType)inst.m_target->dataType;
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(NewObject( ref, inst ));
            }
        }
        else
        {
            if(c_CLR_RT_DataTypeLookup[ dt ].m_flags & CLR_RT_DataTypeLookup::c_Reference)
            {
                dt = DATATYPE_OBJECT;
            }
        }
    }

    ref.SetDataId( CLR_RT_HEAPBLOCK_RAW_ID( dt, CLR_RT_HeapBlock::HB_Alive, 1 ) );
    ref.ClearData();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::InitializeReference( CLR_RT_HeapBlock& ref, const CLR_RECORD_FIELDDEF* target, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_SignatureParser parser; parser.Initialize_FieldDef( assm, target );

    TINYCLR_SET_AND_LEAVE(InitializeReference( ref, parser ));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::InitializeLocals( CLR_RT_HeapBlock* locals, CLR_RT_Assembly* assm, const CLR_RECORD_METHODDEF* md )
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // WARNING!!!
    //
    // This method is a shortcut for the following code:
    //
  
    TINYCLR_HEADER();

    CLR_PMETADATA sig     = assm->GetSignature( md->locals );
    CLR_UINT32    count   = md->numLocals;
    bool          fZeroed = false;

    while(count)
    {
        CLR_DataType         dt = DATATYPE_VOID;
        CLR_RT_TypeDef_Index cls;
        CLR_UINT32           levels = 0;
        CLR_DataType         dtModifier = DATATYPE_VOID;

        while(true)
        {
            dt = CLR_UncompressElementType( sig );

            switch(dt)
            {
            case DATATYPE_TYPE_PINNED :
                dtModifier = DATATYPE_TYPE_PINNED;
                break;
                
            // Array declared on stack        .locals init [0] int16[] foo, 
            case DATATYPE_SZARRAY:
            // Reference declared on stack - .locals init [1] int16& pinned pShort,
            case DATATYPE_BYREF:
                levels++;
                break;

            case DATATYPE_CLASS:
            case DATATYPE_VALUETYPE:
                {
                    CLR_UINT32 tk  = CLR_TkFromStream( sig );
                    CLR_UINT32 idx = CLR_DataFromTk( tk );

                    switch(CLR_TypeFromTk( tk ))
                    {
                    case TBL_TypeSpec:
                        {
                            CLR_RT_SignatureParser          sub; sub.Initialize_TypeSpec( assm, assm->GetTypeSpec( idx ) );
                            CLR_RT_SignatureParser::Element res;

                            TINYCLR_CHECK_HRESULT(sub.Advance( res ));

                            cls     = res.m_cls;
                            levels += res.m_levels;
                        }
                        break;

                    case TBL_TypeRef:
                        cls = assm->m_pCrossReference_TypeRef[ idx ].m_target;
                        break;

                    case TBL_TypeDef:
                        cls.Set( assm->m_idx, idx );
                        break;

                    default:
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                    }

                }
                goto done;

            default:
                {
                    const CLR_RT_TypeDef_Index* cls2 = c_CLR_RT_DataTypeLookup[ dt ].m_cls;

                    if(cls2 == NULL)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                    }

                    cls = *cls2;
                }
                goto done;
            }
        }

done:
        if(levels > 0) // Array or reference
        {
            locals->SetObjectReference( NULL );
            
            // If local varialb has DATATYPE_TYPE_PINNED, we mark heap block as 
            if ( dtModifier == DATATYPE_TYPE_PINNED )
            {
                locals->Pin();
            }
        }
        else
        {
            if(dt == DATATYPE_VALUETYPE)
            {
                CLR_RT_TypeDef_Instance inst; inst.InitializeFromIndex( cls );

                if(inst.m_target->dataType != DATATYPE_VALUETYPE)
                {
                    locals->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID( inst.m_target->dataType, CLR_RT_HeapBlock::HB_Alive, 1 ) );
                    locals->ClearData();
                }
                else
                {
                    //
                    // Before we allocate anything, we need to make sure the rest of the local variables are in a consistent state.
                    //
                    if(fZeroed == false)
                    {
                        fZeroed = true;

                        CLR_RT_HeapBlock* ptr    =  locals;
                        CLR_RT_HeapBlock* ptrEnd = &locals[ count ];

                        do
                        {
                            ptr->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_I4,0,1) );

                        } while(++ptr < ptrEnd);
                    }

                    TINYCLR_CHECK_HRESULT(NewObject( *locals, inst ));
                }
            }
            else
            {
                if(c_CLR_RT_DataTypeLookup[ dt ].m_flags & CLR_RT_DataTypeLookup::c_Reference)
                {
                    dt = DATATYPE_OBJECT;
                }

                locals->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(dt,CLR_RT_HeapBlock::HB_Alive,1) );
                locals->ClearData();
            }
        }

        locals++;
        count--;
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::NewObjectFromIndex( CLR_RT_HeapBlock& reference, const CLR_RT_TypeDef_Index& cls )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance inst;

    if(inst.InitializeFromIndex( cls ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    TINYCLR_SET_AND_LEAVE(NewObject( reference, inst ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::NewObject( CLR_RT_HeapBlock& reference, const CLR_RT_TypeDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    reference.SetObjectReference( NULL );

    CLR_DataType dt = (CLR_DataType)inst.m_target->dataType;

    //
    // You cannot create an array this way.
    //
    if(inst.m_data == g_CLR_RT_WellKnownTypes.m_Array.m_data)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    if((c_CLR_RT_DataTypeLookup[ dt ].m_flags & CLR_RT_DataTypeLookup::c_Reference) == 0)
    {
        reference.SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(dt,0,1) );
    }
    else
    {
        switch(dt)
        {
        case DATATYPE_STRING:
            //
            // Special case for strings.
            //
            break;

        case DATATYPE_WEAKCLASS:
            {
                CLR_RT_HeapBlock_WeakReference* weakref;

                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_WeakReference::CreateInstance( weakref ));

                if(inst.m_data == g_CLR_RT_WellKnownTypes.m_ExtendedWeakReference.m_data)
                {
                    weakref->m_identity.m_flags |= CLR_RT_HeapBlock_WeakReference::WR_ExtendedType;
                }

                reference.SetObjectReference( weakref );
            }
            break;

        case DATATYPE_CLASS:
        case DATATYPE_VALUETYPE:
            {
                int               clsFields = inst.m_target->iFields_Num;
                int               totFields = inst.CrossReference().m_totalFields + CLR_RT_HeapBlock::HB_Object_Fields_Offset;
                CLR_RT_HeapBlock* obj       = ExtractHeapBlocksForClassOrValueTypes( dt, 0, inst, totFields ); CHECK_ALLOCATION(obj);

                reference.SetObjectReference( obj );

                {
                    const CLR_RECORD_FIELDDEF* target  = NULL;
                    CLR_RT_Assembly*           assm    = NULL;
                    CLR_RT_TypeDef_Instance    instSub = inst;


                    TINYCLR_CHECK_HRESULT(obj->SetObjectCls( inst ));

                    //
                    // Initialize field types, from last to first.
                    //
                    // We do the decrement BEFORE the comparison because we want to stop short of the first field, the object descriptor (already initialized).
                    //
                    obj += totFields;
                    while(--totFields > 0)
                    {
                        while(clsFields == 0)
                        {
                            if(instSub.SwitchToParent() == false) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

                            clsFields = instSub.m_target->iFields_Num;
                            target    = NULL;
                        }

                        if(target == NULL)
                        {
                            assm   = instSub.m_assm;
                            target = assm->GetFieldDef( instSub.m_target->iFields_First + clsFields );
                        }

                        obj--; target--; clsFields--;

                        TINYCLR_CHECK_HRESULT(InitializeReference( *obj, target, assm ));
                    }
                }

                if(inst.HasFinalizer())
                {
                    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Finalizer::CreateInstance( reference.Dereference(), inst ));
                }
            }
            break;

        default:
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::NewObject( CLR_RT_HeapBlock& reference, CLR_UINT32 tk, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance res;

    if(res.ResolveToken( tk, assm ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    TINYCLR_CHECK_HRESULT(NewObjectFromIndex( reference, res ));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::CloneObject( CLR_RT_HeapBlock& reference, const CLR_RT_HeapBlock& source )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const CLR_RT_HeapBlock* obj = &source;
    CLR_DataType            dt;

    while(true)
    {
        dt = (CLR_DataType)obj->DataType();

        if(dt == DATATYPE_BYREF ||
           dt == DATATYPE_OBJECT )
        {
            obj = obj->Dereference(); FAULT_ON_NULL(obj);
        }
        else
        {
            break;
        }
    }

    switch(dt)
    {

    case DATATYPE_VALUETYPE:
    case DATATYPE_CLASS:
        {
            //
            // Save the pointer to the object to clone, in case 'reference' and 'source' point to the same block.
            //
            CLR_RT_HeapBlock     safeSource; safeSource.SetObjectReference( obj );
            CLR_RT_ProtectFromGC gc( safeSource );

            TINYCLR_CHECK_HRESULT(NewObjectFromIndex( reference              , obj->ObjectCls() ));
            TINYCLR_CHECK_HRESULT(CopyValueType     ( reference.Dereference(), obj              ));
        }
        break;

    default:
        if((c_CLR_RT_DataTypeLookup[ dt ].m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType) == 0)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
        }

        //
        // Non-reference type, straight copy.
        //
        reference.Assign( source );
        break;
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::CopyValueType( CLR_RT_HeapBlock* destination, const CLR_RT_HeapBlock* source )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(destination != source)
    {
        const CLR_RT_TypeDef_Index& cls = source->ObjectCls();
        if(cls.m_data == destination->ObjectCls().m_data)
        {
            CLR_RT_TypeDef_Instance inst; inst.InitializeFromIndex( cls );
            CLR_UINT32              totFields = inst.CrossReference().m_totalFields;

            if(source->IsBoxed()) destination->Box();

            while(true)
            {
                if(totFields-- == 0) break;

                //
                // We increment the two pointers to skip the header of the objects.
                //
                source     ++;
                destination++;

                TINYCLR_CHECK_HRESULT(destination->Reassign( *source ));
            }

            TINYCLR_SET_AND_LEAVE(S_OK);
        }

        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::NewArrayList( CLR_RT_HeapBlock& ref, int size, CLR_RT_HeapBlock_Array*& array )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const int minCapacity = CLR_RT_ArrayListHelper::c_defaultCapacity;
    int       count       = size;
    int       capacity    = size < minCapacity ? minCapacity : size;

    TINYCLR_CHECK_HRESULT(NewObjectFromIndex( ref, g_CLR_RT_WellKnownTypes.m_ArrayList ));

    TINYCLR_CHECK_HRESULT(CLR_RT_ArrayListHelper::PrepareArrayList         ( ref,        count, capacity ));
    TINYCLR_CHECK_HRESULT(CLR_RT_ArrayListHelper::ExtractArrayFromArrayList( ref, array, count, capacity ));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::FindFieldDef( CLR_RT_TypeDef_Instance& inst, LPCSTR szText, CLR_RT_FieldDef_Index& res )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance local = inst;

    do
    {
        if(local.m_assm->FindFieldDef( local.m_target, szText, NULL, 0, res )) TINYCLR_SET_AND_LEAVE(S_OK);
    }
    while(local.SwitchToParent());

    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::FindFieldDef( CLR_RT_HeapBlock& reference, LPCSTR szText, CLR_RT_FieldDef_Index& res )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       obj;
    CLR_RT_TypeDef_Instance inst;

    if(reference.DataType() != DATATYPE_OBJECT) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    obj = reference.Dereference(); FAULT_ON_NULL(obj);

    if(inst.InitializeFromIndex( obj->ObjectCls() ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_SET_AND_LEAVE(FindFieldDef( inst, szText, res ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::FindField( CLR_RT_HeapBlock& reference, LPCSTR szText, CLR_RT_HeapBlock*& field )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_FieldDef_Instance inst; 
    CLR_RT_FieldDef_Index    idx;
    CLR_RT_HeapBlock*        res;

    field = NULL;

    TINYCLR_CHECK_HRESULT(FindFieldDef( reference, szText, idx ));

    inst.InitializeFromIndex( idx );

    if(inst.m_target->flags & CLR_RECORD_FIELDDEF::FD_Static)
    {
        res = CLR_RT_ExecutionEngine::AccessStaticField( idx ); if(res == NULL) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
    else
    {
        res = reference.Dereference(); FAULT_ON_NULL(res);

        res += inst.CrossReference().m_offset;
    }

    field = res;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::SetField( CLR_RT_HeapBlock& reference, LPCSTR szText, CLR_RT_HeapBlock& value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* field;

    TINYCLR_CHECK_HRESULT(FindField( reference, szText, field ));

    field->Assign( value );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::GetField( CLR_RT_HeapBlock& reference, LPCSTR szText, CLR_RT_HeapBlock& value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* field;

    TINYCLR_CHECK_HRESULT(FindField( reference, szText, field ));

    value.Assign( *field );

    TINYCLR_NOCLEANUP();
}

//--//

CLR_RT_HeapBlock_Lock* CLR_RT_ExecutionEngine::FindLockObject( CLR_RT_DblLinkedList& threads, CLR_RT_HeapBlock& object )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,threads)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Lock,lock,th->m_locks)
        {            
            CLR_RT_HeapBlock& res = lock->m_resource;

#if defined(TINYCLR_APPDOMAINS)
            if(lock->m_appDomain != GetCurrentAppDomain()) continue;
#endif

            if(CLR_RT_HeapBlock::ObjectsEqual(res, object, true))
            {
                return lock;
            }
        }
        TINYCLR_FOREACH_NODE_END();
    }
    TINYCLR_FOREACH_NODE_END();

    return NULL;
}

CLR_RT_HeapBlock_Lock* CLR_RT_ExecutionEngine::FindLockObject( CLR_RT_HeapBlock& object )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Lock* lock;

    if(object.DataType() == DATATYPE_OBJECT)
    {
        CLR_RT_HeapBlock* ptr = object.Dereference();

        if(ptr)
        {
            switch(ptr->DataType())
            {
                case DATATYPE_VALUETYPE:
                case DATATYPE_CLASS    :
                    return ptr->ObjectLock();
            }
        }
    }

    lock = FindLockObject( m_threadsReady  , object ); if(lock) return lock;
    lock = FindLockObject( m_threadsWaiting, object );          return lock;    
}

//--//

void CLR_RT_ExecutionEngine::DeleteLockRequests( CLR_RT_Thread* thTarget, CLR_RT_SubThread* sthTarget )
{
    NATIVE_PROFILE_CLR_CORE();
    if(( thTarget &&  thTarget->m_lockRequestsCount) ||
       (sthTarget && sthTarget->m_lockRequestsCount)  )
    {
        DeleteLockRequests( thTarget, sthTarget, m_threadsReady   );
        DeleteLockRequests( thTarget, sthTarget, m_threadsWaiting );
    }
}

void CLR_RT_ExecutionEngine::DeleteLockRequests( CLR_RT_Thread* thTarget, CLR_RT_SubThread* sthTarget, CLR_RT_DblLinkedList& threads )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,threads)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Lock,lock,th->m_locks)
        {
            TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_LockRequest,req,lock->m_requests)
            {
                CLR_RT_SubThread* sth = req->m_subthreadWaiting;

                if(sth == sthTarget || sth->m_owningThread == thTarget)
                {
                    g_CLR_RT_EventCache.Append_Node( req );

                    if(sth->ChangeLockRequestCount( -1 )) return;
                }
            }
            TINYCLR_FOREACH_NODE_END();
        }
        TINYCLR_FOREACH_NODE_END();
    }
    TINYCLR_FOREACH_NODE_END();
}

//--//

void CLR_RT_ExecutionEngine::ProcessHardware()
{
    NATIVE_PROFILE_CLR_CORE();
    ::Watchdog_ResetCounter();

    g_CLR_HW_Hardware.ProcessActivity();
}

CLR_INT64 CLR_RT_ExecutionEngine::ProcessTimer()
{
    NATIVE_PROFILE_CLR_CORE();

    CLR_INT64 timeoutMin;

    ProcessHardware();

    timeoutMin = m_maximumTimeToActive; // max sleep.

    ////////////////////////////////////////////////
    // WARNING
    // 
    // The check on the 'Stopped' condition, and the 'else' 
    // condition below cause a race condition when the application is running under debugger 
    // and there are no ready threads in the app.  teh desktop side debugger in facts is prodigal
    // of Stopped commands and so is the runtime itself.  Since the commands come asynchronously 
    // though and there is no co-ordination it is possible that 
    // a) a 'Stopped' condition reset get lost or
    // b) waiting threads whose timers are expired are never moved to the ready queue
    // 
    
    if(CLR_EE_DBG_IS( Stopped ) && m_threadsWaiting.IsEmpty())
    {
        // Don't process events while the debugger is stopped and no thread was waiting.
        // if some thread was waiting we might need to transfer it to the ready queue
    }
    else
    {
        if(m_timerCache && m_timerCacheNextTimeout > m_currentMachineTime)
        {
            timeoutMin = m_timerCacheNextTimeout - m_currentMachineTime;
        }
        //else
        {
            CheckTimers( timeoutMin );
 
            CheckThreads( timeoutMin, m_threadsReady   );
            CheckThreads( timeoutMin, m_threadsWaiting );

            m_timerCacheNextTimeout = timeoutMin + m_currentMachineTime;
            m_timerCache            = (m_timerCacheNextTimeout > m_currentMachineTime);
        
        }
    }

    // if the system timer is not set as one of the wakeup events then just return the max time to active
    if(0 == (g_CLR_HW_Hardware.m_wakeupEvents & SYSTEM_EVENT_FLAG_SYSTEM_TIMER))
    {
        timeoutMin = m_maximumTimeToActive;
    } 

    return timeoutMin;
}

void CLR_RT_ExecutionEngine::ProcessTimeEvent( CLR_UINT32 event )
{
    NATIVE_PROFILE_CLR_CORE();
    SYSTEMTIME systemTime;

    UpdateTime();

    Time_ToSystemTime( m_currentLocalTime, &systemTime );

    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Timer,timer,m_timers)
    {
        if(timer->m_flags & CLR_RT_HeapBlock_Timer::c_EnabledTimer)
        {
            CLR_UINT32 val = timer->m_flags & CLR_RT_HeapBlock_Timer::c_AnyChange;

            if(val)
            {
                timer->AdjustNextFixedExpire( systemTime, true );

                if(val == event && (timer->m_flags & CLR_RT_HeapBlock_Timer::c_Recurring) == 0)
                {
                    timer->Trigger();
                }
            }
        }
    }
    TINYCLR_FOREACH_NODE_END();

    SpawnTimer();
}

void CLR_RT_ExecutionEngine::InvalidateTimerCache()
{
    NATIVE_PROFILE_CLR_CORE();
    g_CLR_RT_ExecutionEngine.m_timerCache = false;
}

//--//--//

bool CLR_RT_ExecutionEngine::IsTimeExpired( const CLR_INT64& timeExpire, CLR_INT64& timeoutMin, bool fAbsolute )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_INT64 cmp = (fAbsolute ? m_currentLocalTime : m_currentMachineTime);

    if(timeExpire <= cmp) return true;

    CLR_INT64 diff = timeExpire - cmp;

    if(diff < timeoutMin)
    {
        timeoutMin = diff;
    }

    return false;
}

bool CLR_RT_ExecutionEngine::IsThereEnoughIdleTime( CLR_UINT32 expectedMsec )
{
    NATIVE_PROFILE_CLR_CORE();
    if(::Events_MaskedRead( g_CLR_HW_Hardware.m_wakeupEvents )) return false;

    CLR_INT64 now = Time_GetMachineTime();

    if(now + expectedMsec * TIME_CONVERSION__TO_MILLISECONDS >= m_currentNextActivityTime) return false;

    return true;
}

//--//

void CLR_RT_ExecutionEngine::CheckTimers( CLR_INT64& timeoutMin )
{
    NATIVE_PROFILE_CLR_CORE();
    bool fAnyTimersExpired = false;

    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Timer,timer,m_timers)
    {
        if(timer->m_flags & CLR_RT_HeapBlock_Timer::c_EnabledTimer)
        {
            CLR_INT64 expire = timer->m_timeExpire;
            if(IsTimeExpired( expire, timeoutMin, (timer->m_flags & CLR_RT_HeapBlock_Timer::c_AbsoluteTimer) != 0 ))
            {
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
                if(CLR_EE_DBG_IS_NOT( PauseTimers ))
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
                {
                    timer->Trigger();
                    fAnyTimersExpired = true;
                }
            }
        }
    }
    TINYCLR_FOREACH_NODE_END();

    if(fAnyTimersExpired)
    {
        SpawnTimer();
    }

}

void CLR_RT_ExecutionEngine::CheckThreads( CLR_INT64& timeoutMin, CLR_RT_DblLinkedList& threads )
{
    NATIVE_PROFILE_CLR_CORE();

    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,threads)
    {
        CLR_INT64 expire;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        if(th->m_flags & CLR_RT_Thread::TH_F_Suspended)
        {
            continue;
        }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

        //
        // Check events.
        //
        expire = th->m_waitForEvents_Timeout;
        if(IsTimeExpired( expire, timeoutMin, false ))
        {
            th->m_waitForEvents_Timeout = TIMEOUT_INFINITE;

            th->Restart( false );
        }

        //
        // Check wait for object.
        //

        {
            CLR_RT_HeapBlock_WaitForObject* wait = th->m_waitForObject;

            if(wait)
            {
                if(IsTimeExpired( wait->m_timeExpire, timeoutMin, false ))
                {
                    th->m_waitForObject_Result = CLR_RT_Thread::TH_WAIT_RESULT_TIMEOUT;
        
                    th->Restart( true );
                }
            }
        }

        //
        // Check lock requests.
        //
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Lock,lock,th->m_locks)
        {
            TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_LockRequest,req,lock->m_requests)
            {
                if(IsTimeExpired( req->m_timeExpire, timeoutMin, false ))
                {
                    CLR_RT_SubThread* sth = req->m_subthreadWaiting;

                    sth->ChangeLockRequestCount( -1 );

                    g_CLR_RT_EventCache.Append_Node( req );
                }
            }
            TINYCLR_FOREACH_NODE_END();
        }
        TINYCLR_FOREACH_NODE_END();

        //
        // Check constraints.
        //
        TINYCLR_FOREACH_NODE_BACKWARD(CLR_RT_SubThread,sth,th->m_subThreads)
        {
            if(sth->m_timeConstraint != TIMEOUT_INFINITE)
            {
                if(IsTimeExpired( s_compensation.Adjust( sth->m_timeConstraint ), timeoutMin, false ))
                {
                    (void)Library_corlib_native_System_Exception::CreateInstance( th->m_currentException, g_CLR_RT_WellKnownTypes.m_ConstraintException, S_OK, th->CurrentFrame() );

                    if((sth->m_status & CLR_RT_SubThread::STATUS_Triggered) == 0)
                    {
                        sth->m_status |= CLR_RT_SubThread::STATUS_Triggered;

                        //
                        // This is the first time, give it 500msec to clean before killing it.
                        //
                        sth->m_timeConstraint += TIME_CONVERSION__TO_MILLISECONDS * 500; CLR_RT_ExecutionEngine::InvalidateTimerCache();
                    }
                    else
                    {
                        CLR_RT_SubThread::DestroyInstance( th, sth, CLR_RT_SubThread::MODE_CheckLocks );

                        //
                        // So it doesn't fire again...
                        //
                        sth->m_timeConstraint = TIMEOUT_INFINITE;
                    }

                    th->Restart( true );
                }
            }
        }
        TINYCLR_FOREACH_NODE_END();
    }
    TINYCLR_FOREACH_NODE_END();
}

//--//

HRESULT CLR_RT_ExecutionEngine::LockObject( CLR_RT_HeapBlock& reference, CLR_RT_SubThread* sth, const CLR_INT64& timeExpire, bool fForce )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Lock* lock;

    lock = FindLockObject( reference );
    
    if(lock == NULL)
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Lock::CreateInstance( lock, sth->m_owningThread, reference ));
    }

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_Lock::IncrementOwnership( lock, sth, timeExpire, fForce ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::UnlockObject( CLR_RT_HeapBlock& reference, CLR_RT_SubThread* sth )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Lock* lock;

    lock = FindLockObject( reference );

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_Lock::DecrementOwnership( lock, sth ));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::Sleep( CLR_RT_Thread* caller, const CLR_INT64& timeExpire )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    caller->m_waitForEvents_Timeout = timeExpire; CLR_RT_ExecutionEngine::InvalidateTimerCache();
    caller->m_status                = CLR_RT_Thread::TH_S_Waiting;

    TINYCLR_SET_AND_LEAVE(CLR_E_THREAD_WAITING);

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ExecutionEngine::WaitEvents( CLR_RT_Thread* caller, const CLR_INT64& timeExpire, CLR_UINT32 events, bool& fSuccess )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(m_raisedEvents & events)
    {
        m_raisedEvents &= ~events;

        fSuccess = true;
    }
    else
    {
        fSuccess = false;

        if(Time_GetMachineTime() < timeExpire)
        {
            caller->m_waitForEvents         = events;
            caller->m_waitForEvents_Timeout = timeExpire; CLR_RT_ExecutionEngine::InvalidateTimerCache();
            caller->m_status                = CLR_RT_Thread::TH_S_Waiting;

            TINYCLR_SET_AND_LEAVE(CLR_E_THREAD_WAITING);
        }
    }

    TINYCLR_NOCLEANUP();
}

void CLR_RT_ExecutionEngine::SignalEvents( CLR_RT_DblLinkedList& threads, CLR_UINT32 events )
{
    NATIVE_PROFILE_CLR_CORE();
    m_raisedEvents |= events;

    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,threads)
    {
        if((th->m_waitForEvents & events) != 0)
        {
            _ASSERTE(&threads == &m_threadsWaiting);
            _ASSERTE(th->m_status == CLR_RT_Thread::TH_S_Waiting);

            th->Restart( true );
        }
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_ExecutionEngine::SignalEvents( CLR_UINT32 events )
{
    NATIVE_PROFILE_CLR_CORE();
    //Why does the ready queue need to be checked.
    SignalEvents( m_threadsReady  , events );
    SignalEvents( m_threadsWaiting, events );
}

//--//

bool CLR_RT_ExecutionEngine::IsInstanceOf( CLR_RT_TypeDescriptor& desc, CLR_RT_TypeDescriptor& descTarget )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDef_Instance& inst       = desc      .m_handlerCls;
    CLR_RT_TypeDef_Instance& instTarget = descTarget.m_handlerCls;
    bool                     fArray     = false;

    while(desc      .m_reflex.m_levels > 0 &&
          descTarget.m_reflex.m_levels > 0  )
    {
        desc      .GetElementType( desc       );
        descTarget.GetElementType( descTarget );

        fArray = true;
    }

    if(desc.m_reflex.m_levels < descTarget.m_reflex.m_levels) return false;

    if(desc.m_reflex.m_levels > descTarget.m_reflex.m_levels)
    {
        if(descTarget.m_reflex.m_levels == 0)
        {
            //
            // Casting from <type>[] to System.Array or System.Object is always allowed.
            //
            if(inst.m_data == g_CLR_RT_WellKnownTypes.m_Array     .m_data ||
               inst.m_data == g_CLR_RT_WellKnownTypes.m_Object    .m_data ||
               inst.m_data == g_CLR_RT_WellKnownTypes.m_IList     .m_data ||
               inst.m_data == g_CLR_RT_WellKnownTypes.m_ICloneable.m_data  )
            {
                return true;
            }
        }

        if(inst.m_target->dataType != instTarget.m_target->dataType)
        {
            return false;
        }
    }

    CLR_UINT32 semantic       = (inst      .m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask);
    CLR_UINT32 semanticTarget = (instTarget.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask);

    if(fArray)
    {
        if(semantic != semanticTarget)
        {
            return false;
        }
    }

    do
    {
        if(inst.m_data == instTarget.m_data)
        {
            return true;
        }

        //
        // Scan the list of interfaces.
        //
        if(semanticTarget == CLR_RECORD_TYPEDEF::TD_Semantics_Interface && inst.m_target->interfaces != CLR_EmptyIndex)
        {
            CLR_RT_SignatureParser          parser; parser.Initialize_Interfaces( inst.m_assm, inst.m_target );
            CLR_RT_SignatureParser::Element res;

            while(parser.Available() > 0)
            {
                if(FAILED(parser.Advance( res ))) break;

                if(res.m_cls.m_data == instTarget.m_data)
                {
                    return true;
                }
            }
        }
    }
    while(inst.SwitchToParent());

    return false;
}

bool CLR_RT_ExecutionEngine::IsInstanceOf( const CLR_RT_TypeDef_Index& cls, const CLR_RT_TypeDef_Index& clsTarget )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDescriptor desc;
    CLR_RT_TypeDescriptor descTarget;

    if(FAILED(desc      .InitializeFromType( cls       ))) return false;
    if(FAILED(descTarget.InitializeFromType( clsTarget ))) return false;

    return IsInstanceOf( desc, descTarget );
}

bool CLR_RT_ExecutionEngine::IsInstanceOf( CLR_RT_HeapBlock& ref, const CLR_RT_TypeDef_Index& clsTarget )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDescriptor desc;
    CLR_RT_TypeDescriptor descTarget;

    if(FAILED(desc      .InitializeFromObject( ref       ))) return false;
    if(FAILED(descTarget.InitializeFromType  ( clsTarget ))) return false;

    return IsInstanceOf( desc, descTarget );
}

bool CLR_RT_ExecutionEngine::IsInstanceOf( CLR_RT_HeapBlock& obj, CLR_RT_Assembly* assm, CLR_UINT32 token )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDescriptor    desc;
    CLR_RT_TypeDescriptor    descTarget;
    CLR_RT_TypeDef_Instance  clsTarget;
    CLR_RT_TypeSpec_Instance defTarget;

    if(FAILED(desc.InitializeFromObject( obj ))) return false;

    if(clsTarget.ResolveToken( token, assm ))
    {
        //
        // Shortcut for identity.
        //
        if(desc.m_handlerCls.m_data == clsTarget.m_data) return true;

        if(FAILED(descTarget.InitializeFromType( clsTarget ))) return false;
    }
    else if(defTarget.ResolveToken( token, assm ))
    {
        if(FAILED(descTarget.InitializeFromTypeSpec( defTarget ))) return false;
    }
    else
    {
        return false;
    }

    return IsInstanceOf( desc, descTarget );
}

HRESULT CLR_RT_ExecutionEngine::CastToType( CLR_RT_HeapBlock& ref, CLR_UINT32 tk, CLR_RT_Assembly* assm, bool fUpdate )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(ref.DataType() == DATATYPE_OBJECT && ref.Dereference() == NULL)
    {
        ;
    }
    else if(g_CLR_RT_ExecutionEngine.IsInstanceOf( ref, assm, tk ) == true)
    {
        ;
    }
    else
    {
        if(fUpdate == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_CAST);
        }

        ref.SetObjectReference( NULL );
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////


HRESULT CLR_RT_ExecutionEngine::InitTimeout( CLR_INT64& timeExpire, const CLR_INT64& timeout )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(timeout < 0)
    {
        if(timeout != -1L)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
        }

        timeExpire = TIMEOUT_INFINITE;
    }
    else
    {
        timeExpire = timeout + Time_GetMachineTime();
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ExecutionEngine::InitTimeout( CLR_INT64& timeExpire, CLR_INT32 timeout )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(timeout < 0)
    {
        if(timeout != -1)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
        }

        timeExpire = TIMEOUT_INFINITE;
    }
    else
    {
        timeExpire  = timeout;
        timeExpire *= TIME_CONVERSION__TO_MILLISECONDS;
        timeExpire += Time_GetMachineTime();
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_ExecutionEngine::DebuggerLoop()
{
    NATIVE_PROFILE_CLR_CORE();
    ProcessHardware();

    UpdateTime();

    WaitSystemEvents( SLEEP_LEVEL__SLEEP, g_CLR_HW_Hardware.m_wakeupEvents, TIME_CONVERSION__TO_MILLISECONDS * 100 );
}


//--//


////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

void CLR_RT_ExecutionEngine::SetDebuggingInfoBreakpoints( bool fSet )
{
    NATIVE_PROFILE_CLR_CORE();
    for(size_t pos=0; pos<m_breakpointsNum; pos++)
    {
        CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = m_breakpoints[ pos ];

        if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_HARD)
        {
            CLR_RT_MethodDef_Instance inst;

            if(inst.InitializeFromIndex( def.m_md ))
            {
                inst.DebuggingInfo().SetBreakpoint( fSet );
            }
        }
    }
}

void CLR_RT_ExecutionEngine::InstallBreakpoints( CLR_DBG_Commands::Debugging_Execution_BreakpointDef* data, size_t num )
{
    NATIVE_PROFILE_CLR_CORE();
    SetDebuggingInfoBreakpoints( false );

    if(m_breakpoints)
    {
        CLR_RT_Memory::Release( m_breakpoints );

        m_breakpoints    = NULL;
        m_breakpointsNum = 0;
    }

    if(num)
    {
        size_t len = num * sizeof(CLR_DBG_Commands::Debugging_Execution_BreakpointDef);

        m_breakpoints = (CLR_DBG_Commands::Debugging_Execution_BreakpointDef*)CLR_RT_Memory::Allocate( len, CLR_RT_HeapBlock::HB_CompactOnFailure );
        if(m_breakpoints)
        {
            memcpy( m_breakpoints, data, len );

            m_breakpointsNum = num;
        }
    }

    SetDebuggingInfoBreakpoints( true );

    Breakpoint_Threads_Prepare( m_threadsReady   );
    Breakpoint_Threads_Prepare( m_threadsWaiting );
}

void CLR_RT_ExecutionEngine::StopOnBreakpoint( CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def, CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_CORE();
    if(CLR_EE_DBG_IS_NOT(BreakpointsDisabled))
    {
        if(m_breakpointsActiveNum < ARRAYSIZE(m_breakpointsActive))
        {
            CLR_DBG_Commands::Debugging_Execution_BreakpointDef& bp = m_breakpointsActive[ m_breakpointsActiveNum++ ];

            bp = def;

            CLR_EE_DBG_SET(Stopped);
            CLR_RT_EmulatorHooks::Notify_ExecutionStateChanged();
            
            if(th)
            {
                bp.m_pid                 = th->m_pid;
                th->m_timeQuantumExpired = TRUE;
            }

            if(m_breakpointsActiveNum == 1)
            {
#if defined(NETMF_TARGET_BIG_ENDIAN)
                static CLR_DBG_Commands::Debugging_Execution_BreakpointDef s_breakpoint;
                CLR_UINT8* data;

                memcpy( &s_breakpoint, &m_breakpointsActive[0], sizeof(s_breakpoint) );
                
                data = (CLR_UINT8*)&s_breakpoint.m_id;
                CLR_Messaging::SwapEndianPattern( data, sizeof(UINT16), 2 ); //m_id & m_flags
                data = (CLR_UINT8*)&s_breakpoint.m_pid;
                CLR_Messaging::SwapEndianPattern( data, sizeof(UINT32), 8 ); //all other fields

                CLR_EE_DBG_EVENT_SEND(CLR_DBG_Commands::c_Debugging_Execution_BreakpointHit,sizeof(s_breakpoint),&s_breakpoint,WP_Flags::c_NonCritical);
#else
                CLR_EE_DBG_EVENT_SEND(CLR_DBG_Commands::c_Debugging_Execution_BreakpointHit,sizeof(CLR_DBG_Commands::Debugging_Execution_BreakpointDef),&m_breakpointsActive[ 0 ],WP_Flags::c_NonCritical);
#endif
            }
        }
        else
        {
            _ASSERTE(FALSE);
        }
    }
}

void CLR_RT_ExecutionEngine::StopOnBreakpoint( CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def, CLR_RT_StackFrame* stack, CLR_PMETADATA ip )
{    
    NATIVE_PROFILE_CLR_CORE();
    if(ip == NULL) ip = stack->m_IP;
    
    def.m_depth = stack->m_depth;
    def.m_md    = stack->m_call;
    def.m_IP    = (CLR_UINT32)(ip - stack->m_IPstart);

    //Don't fail for special cases regarding messages dealing with exception handling.
    _ASSERTE(def.m_IP == 0xffffffff || ip >= stack->m_IPstart);
    //we don't actually know the end of the method.

    StopOnBreakpoint( def, stack->m_owningThread );
}

bool CLR_RT_ExecutionEngine::DequeueActiveBreakpoint( CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def )
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_breakpointsActiveNum)
    {
        def = m_breakpointsActive[ 0 ];

        if(--m_breakpointsActiveNum == 0)
        {
            def.m_flags |= CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_LAST_BREAKPOINT;
        }

        memmove( &m_breakpointsActive[ 0 ], &m_breakpointsActive[ 1 ], sizeof(m_breakpointsActive[ 0 ]) * m_breakpointsActiveNum );

        return true;
    }

    return false;
}

//--//

void CLR_RT_ExecutionEngine::Breakpoint_System_Event( CLR_DBG_Commands::Debugging_Execution_BreakpointDef& hit, CLR_UINT16 event, CLR_RT_Thread* th, CLR_RT_StackFrame* stack, CLR_PMETADATA ip )
{        
    NATIVE_PROFILE_CLR_CORE();
    for(size_t pos=0; pos<m_breakpointsNum; pos++)
    {
        CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = m_breakpoints[ pos ];

        if(stack != NULL)
        {
            _ASSERTE(FIMPLIES(th != NULL, th == stack->m_owningThread));

            th = stack->m_owningThread;
        }

        if(th == NULL || def.m_pid == th->m_pid || def.m_pid == CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_PID_ANY)
        {
            if(def.m_flags & event)
            {
                hit.m_id    = def.m_id;
                hit.m_flags = event;                
                
                if(stack != NULL)
                {
                    StopOnBreakpoint( hit, stack, ip );
                }
                else
                {
                    StopOnBreakpoint( hit, th );
                }
            }
        }
    }
}

void CLR_RT_ExecutionEngine::Breakpoint_Assemblies_Loaded()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit; TINYCLR_CLEAR(hit);

    for(size_t i = 0; i < this->m_breakpointsActiveNum; i++)
    {
        CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = this->m_breakpointsActive[ i ];

        if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_ASSEMBLIES_LOADED)
        {
            //There is already one queued AssembliesLoaded breakpoint.  No need to send another one.
            return;
        }
    }

    Breakpoint_System_Event( hit, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_ASSEMBLIES_LOADED, m_currentThread, NULL, NULL );
}

void CLR_RT_ExecutionEngine::Breakpoint_Threads_Prepare( CLR_RT_DblLinkedList& threads )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,threads)
    {
        th->m_fHasJMCStepper = false;

        for(size_t pos=0; pos<m_breakpointsNum; pos++)
        {
            CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = m_breakpoints[ pos ];

            if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_JMC)
            {
                if(def.m_pid == th->m_pid)
                {
                    th->m_fHasJMCStepper = true;
                    break;
                }
            }
        }

        TINYCLR_FOREACH_NODE(CLR_RT_StackFrame,call,th->m_stackFrames)
        {
            call->m_flags &= ~CLR_RT_StackFrame::c_HasBreakpoint;

            if(call->m_call.DebuggingInfo().HasBreakpoint())
            {
                call->m_flags |= CLR_RT_StackFrame::c_HasBreakpoint;
            }
            else
            {
                for(size_t pos=0; pos<m_breakpointsNum; pos++)
                {
                    CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = m_breakpoints[ pos ];

                    if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP)
                    {
                        if(def.m_pid == th->m_pid && def.m_depth == call->m_depth)
                        {
                            call->m_flags |= CLR_RT_StackFrame::c_HasBreakpoint;
                            break;
                        }
                    }
                }
            }
#ifndef TINYCLR_NO_IL_INLINE
            if(call->m_inlineFrame)
            {
                if(call->m_inlineFrame->m_frame.m_call.DebuggingInfo().HasBreakpoint())
                {
                    call->m_flags |= CLR_RT_StackFrame::c_HasBreakpoint;
                }
                else
                {
                    for(size_t pos=0; pos<m_breakpointsNum; pos++)
                    {
                        CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = m_breakpoints[ pos ];

                        if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP)
                        {
                            if(def.m_pid == th->m_pid && def.m_depth == (call->m_depth-1))
                            {
                                call->m_flags |= CLR_RT_StackFrame::c_HasBreakpoint;
                                break;
                            }
                        }
                    }
                }
            }
#endif
        }
        TINYCLR_FOREACH_NODE_END();
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_ExecutionEngine::Breakpoint_Thread_Terminated( CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit; TINYCLR_CLEAR(hit);
    CLR_UINT16 evt = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_THREAD_TERMINATED;

    hit.m_depth = 0xFFFFFFFF;
       
    if(th->m_scratchPad >= 0)
    {
        evt |= CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_EVAL_COMPLETE;  
    }

    Breakpoint_System_Event( hit, evt, th, NULL, NULL );        
}

void CLR_RT_ExecutionEngine::Breakpoint_Thread_Created( CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit; TINYCLR_CLEAR(hit);

    Breakpoint_System_Event( hit, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_THREAD_CREATED, th, NULL, NULL );
}

//--//

void CLR_RT_ExecutionEngine::Breakpoint_StackFrame_Push( CLR_RT_StackFrame* stack, CLR_UINT32 reason )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_Thread* th     = stack->m_owningThread;
    int            pid    = th->m_pid;
    CLR_UINT32     depthMax  = stack->m_depth - 1;
    CLR_UINT16     flags  = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_IN;
    CLR_UINT32     depthMin = depthMax;

    if(stack->m_flags & CLR_RT_StackFrame::c_PseudoStackFrameForFilter)
    {
        //If a filter frame is being pushed on (assuming the InterceptMask for filters is set), we want to intercept the frame
        //in certain special cases.
        _ASSERTE(th->m_nestedExceptionsPos);
        CLR_RT_Thread::UnwindStack& us = th->m_nestedExceptions[ th->m_nestedExceptionsPos-1 ];
        _ASSERTE(us.m_stack == stack);
        depthMin = us.m_handlerStack->m_depth;
        //If we popped off frames from AppDomain transitions that had steppers, we want to break there as well.
        depthMax = 0xffffffff;
        flags |= CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_OVER | CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_OUT;
    }

    for(size_t pos=0; pos<m_breakpointsNum; pos++)
    {
        CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = m_breakpoints[ pos ];

        if(def.m_flags & flags)
        {
            if(def.m_pid == pid)
            {
                bool fStop;

                if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_JMC)
                {
                    fStop = stack->m_call.DebuggingInfo().IsJMC();
                }
                else
                {
                    fStop = (def.m_depth >= depthMin && def.m_depth <= depthMax);
                }

                if(def.m_depth == depthMin && (def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP) == CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_OUT)
                {
                    //In the case a user did a step out in a frame, we don't want to break if a filter gets pushed from that frame. However, if there is a step in or over on that frame,
                    //we should break.
                    fStop = false;
                }

                if(fStop)
                {
                    CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit = def;

                    hit.m_flags = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_IN;
                    hit.m_depthExceptionHandler = reason;

                    StopOnBreakpoint( hit, stack, NULL );
                }
            }
        }
    }

    Breakpoint_StackFrame_Hard( stack, stack->m_IP );
}

void CLR_RT_ExecutionEngine::Breakpoint_StackFrame_Pop( CLR_RT_StackFrame* stack, bool stepEH )
{
    NATIVE_PROFILE_CLR_CORE();
    int                pid    = stack->m_owningThread->m_pid;
    CLR_UINT32         depth  = stack->m_depth;
    CLR_RT_StackFrame* caller = stack->Caller();

    if(caller->Prev() || stepEH)
    {
        for(size_t pos=0; pos<m_breakpointsNum; pos++)
        {
            CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def    = m_breakpoints[ pos ];

            if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_OUT)
            {
                if(def.m_pid == pid)
                {
                    bool fStop;

                    if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_JMC)
                    {
                        fStop = (depth <= def.m_depth) && caller->m_call.DebuggingInfo().IsJMC();
                    }
                    else
                    {
                        fStop = (depth <= def.m_depth);
                    }

                    if(fStop)
                    {
                        CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit = def;

                        hit.m_flags = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_OUT;
                        
                        if(stepEH)
                        {
                            hit.m_depthExceptionHandler = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_STEP_EXCEPTION_HANDLER;
                        }
                        else
                        {
                            hit.m_depthExceptionHandler = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_STEP_RETURN;
                        }

                        StopOnBreakpoint( hit, (stepEH)? stack : caller, NULL );
                    }
                }
            }
        }
    }
}

void CLR_RT_ExecutionEngine::Breakpoint_StackFrame_Step( CLR_RT_StackFrame* stack, CLR_PMETADATA ip )
{
    NATIVE_PROFILE_CLR_CORE();
    int        pid      = stack->m_owningThread->m_pid;
    CLR_UINT32 depth    = stack->m_depth;
    CLR_UINT32 IPoffset = (CLR_UINT32)(ip - stack->m_IPstart);

    for(size_t pos=0; pos<m_breakpointsNum; pos++)
    {
        CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = m_breakpoints[ pos ];

        if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_OVER)
        {
            if(def.m_pid == pid && def.m_depth == depth && (IPoffset < def.m_IPStart || IPoffset >= def.m_IPEnd))
            {
                if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_JMC)
                {
                    if(!stack->m_call.DebuggingInfo().IsJMC())
                    {
                        continue;
                    }
                }

                CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit = def;

                hit.m_flags = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_STEP_OVER;

                StopOnBreakpoint( hit, stack, ip );
            }
        }
    }

    Breakpoint_StackFrame_Hard( stack, ip );
}

void CLR_RT_ExecutionEngine::Breakpoint_StackFrame_Hard( CLR_RT_StackFrame* stack, CLR_PMETADATA ip )
{
    NATIVE_PROFILE_CLR_CORE();
    if(stack->Prev() != NULL && ip != NULL)
    {
        CLR_UINT32 IPoffset = (CLR_UINT32)(ip - stack->m_IPstart);

        for(size_t pos=0; pos<m_breakpointsNum; pos++)
        {
            CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def = m_breakpoints[ pos ];

            if(def.m_flags & CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_HARD)
            {
                if(def.m_pid == stack->m_owningThread->m_pid || def.m_pid == CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_PID_ANY)
                {
                    if(def.m_md.m_data == stack->m_call.m_data && def.m_IP == IPoffset)
                    {
                        CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit = def;

                        hit.m_flags = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_HARD;

                        StopOnBreakpoint( hit, stack, ip );
                    }
                }
            }
        }
    }
}

void CLR_RT_ExecutionEngine::Breakpoint_StackFrame_Break( CLR_RT_StackFrame* stack )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit; TINYCLR_CLEAR(hit);
    
    Breakpoint_System_Event( hit, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_BREAK, NULL, stack, NULL );
}

//--//

void CLR_RT_ExecutionEngine::Breakpoint_Exception( CLR_RT_StackFrame* stack, CLR_UINT32 reason, CLR_PMETADATA ip )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit; TINYCLR_CLEAR(hit);
    hit.m_depthExceptionHandler = reason;

    Breakpoint_System_Event( hit, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_EXCEPTION_THROWN, NULL, stack, ip );
}

void CLR_RT_ExecutionEngine::Breakpoint_Exception_Uncaught( CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit; TINYCLR_CLEAR(hit);

    hit.m_depth                 = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_UNCAUGHT;
    hit.m_depthExceptionHandler = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_DEPTH_UNCAUGHT;

    Breakpoint_System_Event( hit, CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_EXCEPTION_THROWN, th, NULL, NULL );
}

void CLR_RT_ExecutionEngine::Breakpoint_Exception_Intercepted( CLR_RT_StackFrame* stack )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef hit; TINYCLR_CLEAR(hit);
    CLR_UINT16 event = CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_EXCEPTION_CAUGHT
                       | CLR_DBG_Commands::Debugging_Execution_BreakpointDef::c_EXCEPTION_UNWIND;    
    
    hit.m_depthExceptionHandler = stack->m_depth;
    
    Breakpoint_System_Event( hit, event, NULL, stack, NULL );
}

#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_APPDOMAINS)

CLR_RT_AppDomain* CLR_RT_ExecutionEngine::SetCurrentAppDomain( CLR_RT_AppDomain* appDomain )
{ 
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_AppDomain* ad = m_appDomainCurrent; 

    m_appDomainCurrent = appDomain;

    return ad;
}

CLR_RT_AppDomain* CLR_RT_ExecutionEngine::GetCurrentAppDomain()
{
    NATIVE_PROFILE_CLR_CORE();
    return m_appDomainCurrent;
}

void CLR_RT_ExecutionEngine::PrepareThreadsForAppDomainUnload( CLR_RT_AppDomain* appDomain, CLR_RT_DblLinkedList& threads)
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_Thread, th, threads)
    {
        bool fFoundDoomedAppDomain = false;
        bool fInjectThreadAbort    = false;

        TINYCLR_FOREACH_NODE(CLR_RT_StackFrame, stack, th->m_stackFrames)
        {
            if(!fFoundDoomedAppDomain)
            {
                if(stack->m_appDomain == appDomain)
                {                                    
                    //The first stack frame found in a doomed AppDomain
                    fFoundDoomedAppDomain = true;
                    fInjectThreadAbort    = true;
                    stack->m_flags |= CLR_RT_StackFrame::c_AppDomainInjectException;   
                    th   ->m_flags |= CLR_RT_Thread::TH_F_ContainsDoomedAppDomain;
                }
            }
            else //fFoundDoomedAppDomain     
            {
                if(stack->m_flags & CLR_RT_StackFrame::c_AppDomainInjectException)
                {
                    //This thread is already being unwound due to an unloading AppDomain
                    stack->m_flags &= ~CLR_RT_StackFrame::c_AppDomainInjectException;
                    fInjectThreadAbort = false;
                }
            }
        }
        TINYCLR_FOREACH_NODE_END();

        if(fInjectThreadAbort)
        {            
            (void)th->Abort();
        }
    }
    TINYCLR_FOREACH_NODE_END();
}

HRESULT CLR_RT_ExecutionEngine::UnloadAppDomain( CLR_RT_AppDomain* appDomain, CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    //Check to make sure the current thread does not contain any doomed AppDomains
    TINYCLR_FOREACH_NODE(CLR_RT_StackFrame, stack, th->m_stackFrames)
    {
        if(!stack->m_appDomain->IsLoaded() || stack->m_appDomain == appDomain) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    TINYCLR_FOREACH_NODE_END();

    PrepareThreadsForAppDomainUnload( appDomain, m_threadsReady   );
    PrepareThreadsForAppDomainUnload( appDomain, m_threadsWaiting );

    appDomain->m_state = CLR_RT_AppDomain::AppDomainState_Unloading;
    CLR_EE_SET(UnloadingAppDomain);

    TINYCLR_NOCLEANUP();
}

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_ExecutionEngine::UpdateTime()
{
    NATIVE_PROFILE_CLR_CORE();
        
    m_currentMachineTime = Time_GetMachineTime();
    m_currentLocalTime = Time_GetLocalTime();

    /// Did timezone or daylight offset got adjusted? If yes make some adjustments in timers too.
    CLR_INT32 timeZoneOffset = Time_GetTimeZoneOffset();

    if(timeZoneOffset != m_lastTimeZoneOffset)
    {
        SYSTEMTIME systemTime;
    
        m_lastTimeZoneOffset = timeZoneOffset;
        Time_ToSystemTime( m_currentLocalTime, &systemTime );
    
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Timer,timer,m_timers)
        {
            if(timer->m_flags & CLR_RT_HeapBlock_Timer::c_EnabledTimer)
            {
                if(timer->m_flags & CLR_RT_HeapBlock_Timer::c_AnyChange)
                {
                    timer->AdjustNextFixedExpire( systemTime, false );
                }
            }
        }
        TINYCLR_FOREACH_NODE_END();
    }
    
}

CLR_UINT32 CLR_RT_ExecutionEngine::WaitSystemEvents( CLR_UINT32 powerLevel, CLR_UINT32 events, CLR_INT64 timeExpire )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_INT32 timeout;
    
    CLR_UINT32 res = 0;

    m_currentNextActivityTime = timeExpire + m_currentMachineTime;

    timeout = (CLR_INT32)timeExpire / TIME_CONVERSION__TO_MILLISECONDS;

    if(timeout == 0) timeout = 1;

#if defined(TINYCLR_TRACE_SYSTEMEVENTWAIT)
    CLR_INT64 start = Time_GetMachineTime();
#endif

//#define TINYCLR_STRESS_GC
#if defined(TINYCLR_STRESS_GC)
    if(timeout > 100)
    {
        CLR_INT64 startGC = Time_GetMachineTime();


        g_CLR_RT_ExecutionEngine.PerformHeapCompaction   ();

        CLR_INT64 endGC = Time_GetMachineTime();

        timeout -= (CLR_INT32)((endGC - startGC) / TIME_CONVERSION__TO_MILLISECONDS);
    }
#endif

    ::Watchdog_GetSetEnabled( FALSE, TRUE );
    res = ::Events_WaitForEvents( powerLevel, events, timeout );
    ::Watchdog_GetSetEnabled( TRUE, TRUE );


#if defined(TINYCLR_TRACE_SYSTEMEVENTWAIT)
    CLR_INT64 stop  = Time_GetMachineTime();
    CLR_INT64 stop2 = Time_GetMachineTime();

    static CLR_INT64 totalRequested = 0;
    static CLR_INT64 totalActual    = 0;
    static CLR_INT64 samples        = 0;

    totalRequested += timeout;
    totalActual    += (stop - start) - (stop2 - stop);
    samples        += 1;

    if(samples == 10000)
    {
        CLR_Debug::Printf( "Wait %lld %lld\r\n", totalRequested / samples, totalActual / samples );

        totalRequested = 0;
        totalActual    = 0;
        samples        = 0;
    }
#endif

    m_currentNextActivityTime = 0;

    return res;
}

