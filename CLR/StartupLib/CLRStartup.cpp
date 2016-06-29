////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CLRStartup.h"

#if defined(_WIN32)

#include<TinyCLR_ParseOptions.h>

#endif

////////////////////////////////////////////////////////////////////////////////

struct Settings
#if defined(_WIN32)
: CLR_RT_ParseOptions
#endif
{
    CLR_SETTINGS m_clrOptions;
#if defined(_WIN32)
    CLR_RT_ParseOptions::BufferMap m_assemblies;
#endif
    bool m_fInitialized;

    //--//
    
    HRESULT Initialize(CLR_SETTINGS params)
    {
        TINYCLR_HEADER();

        m_clrOptions = params;

#if defined(PLATFORM_WINDOWS_EMULATOR)
        g_CLR_RT_ExecutionEngine.m_fPerformGarbageCollection = params.PerformGarbageCollection;
        g_CLR_RT_ExecutionEngine.m_fPerformHeapCompaction    = params.PerformHeapCompaction;

        CLR_UINT32 clockFrequencyBaseline = 27000000;
        CLR_UINT32 clockFrequency         = CPU_SystemClock();
        double     clockFrequencyRatio    = 1;

        if(clockFrequency > 0)
        {
            clockFrequencyRatio = (double)clockFrequencyBaseline / (double)clockFrequency;
        }

        g_HAL_Configuration_Windows.ProductType              = HAL_Configuration_Windows::Product_Aux;
        g_HAL_Configuration_Windows.SlowClockPerSecond       = 32768;
        g_HAL_Configuration_Windows.TicksPerMethodCall       = (CLR_UINT64)(45.0*clockFrequencyRatio);
        g_HAL_Configuration_Windows.TicksPerOpcode           = (CLR_UINT64)( 5.0*clockFrequencyRatio);
        g_HAL_Configuration_Windows.GraphHeapEnabled         = false;
#endif

        TINYCLR_CHECK_HRESULT(CLR_RT_ExecutionEngine::CreateInstance());
#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "Created EE.\r\n" );
#endif

#if !defined(BUILD_RTM)
        if(params.WaitForDebugger)
        {
#if defined(_WIN32)
            CLR_EE_DBG_SET( Enabled );
#endif
            CLR_EE_DBG_SET( Stopped );
        }
#endif

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.StartHardware());
#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "Started Hardware.\r\n" );
#endif

        CLR_DBG_Debugger::Debugger_Discovery();

        m_fInitialized = true;


        TINYCLR_NOCLEANUP();
    }

    
    HRESULT LoadAssembly( const CLR_RECORD_ASSEMBLY* header, CLR_RT_Assembly*& assm )
    {   
        TINYCLR_HEADER();
        
        const CLR_RT_NativeAssemblyData *pNativeAssmData;
        
        TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( header, assm ));
        
        // Get handlers for native functions in assembly
        pNativeAssmData = GetAssemblyNativeData( assm->m_szName );
        
        // If pNativeAssmData not NULL- means this assembly has native calls and there is pointer to table with native calls.
        if ( pNativeAssmData != NULL )
        {   
            // First verify that check sum in assembly object matches hardcoded check sum. 
            if ( assm->m_header->nativeMethodsChecksum != pNativeAssmData->m_checkSum )
            {
                CLR_Debug::Printf("***********************************************************************\r\n");
                CLR_Debug::Printf("*                                                                     *\r\n");
                CLR_Debug::Printf("* ERROR!!!!  Firmware version does not match managed code version!!!! *\r\n");
                CLR_Debug::Printf("*                                                                     *\r\n");
                CLR_Debug::Printf("*                                                                     *\r\n");
                CLR_Debug::Printf("* Invalid native checksum: %s 0x%08X!=0x%08X *\r\n",
                                    assm->m_szName,
                                    assm->m_header->nativeMethodsChecksum,
                                    pNativeAssmData->m_checkSum
                                 );
                CLR_Debug::Printf("*                                                                     *\r\n");
                CLR_Debug::Printf("***********************************************************************\r\n");

                TINYCLR_SET_AND_LEAVE(CLR_E_ASSM_WRONG_CHECKSUM);
            }

            // Assembly has valid pointer to table with native methods. Save it.
            assm->m_nativeCode = (const CLR_RT_MethodHandler *)pNativeAssmData->m_pNativeMethods;
        }
        g_CLR_RT_TypeSystem.Link( assm );
        TINYCLR_NOCLEANUP();
    }


    HRESULT Load()
    {
        TINYCLR_HEADER();

#if defined(_WIN32)
        CLR_RT_StringVector vec;

        WCHAR* pContext = NULL;
        WCHAR* pch = wcstok_s(this->m_clrOptions.EmulatorArgs, L" ", &pContext);
        
        while (pch != NULL)
        {
            //printf ("%s\n",pch);
            vec.push_back(pch);
            
            pch = wcstok_s(NULL, L" ", &pContext);
        }
        
        ProcessOptions(vec);

        for(CLR_RT_ParseOptions::BufferMapIter it = m_assemblies.begin(); it != m_assemblies.end(); it++)
        {
            CLR_RT_Assembly* assm;
            const CLR_RT_Buffer* buffer = (const CLR_RT_Buffer*)it->second;
            const CLR_RECORD_ASSEMBLY* header = (CLR_RECORD_ASSEMBLY*)&(*buffer)[0];

            // Creates instance of assembly, sets pointer to native functions, links to g_CLR_RT_TypeSystem 
            TINYCLR_CHECK_HRESULT( LoadAssembly( header, assm ) );
        }
#else     

#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "Create TS.\r\n" );
#endif

        TINYCLR_CHECK_HRESULT(LoadKnownAssemblies( TinyClr_Dat_Start, TinyClr_Dat_End ));

#endif // defined(PLATFORM_WINDOWS_EMULATOR) || defined(PLATFORM_WINCE)

#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "Loading Deployment Assemblies.\r\n" );
#endif

        LoadDeploymentAssemblies( BlockUsage::DEPLOYMENT );

        //--//

#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "Resolving.\r\n" );
#endif
        TINYCLR_CHECK_HRESULT(g_CLR_RT_TypeSystem.ResolveAll());

        g_CLR_RT_Persistence_Manager.Initialize();

        TINYCLR_CHECK_HRESULT(g_CLR_RT_TypeSystem.PrepareForExecution());

#if defined(TINYCLR_PROFILE_HANDLER)
        CLR_PROF_Handler::Calibrate();
#endif

        TINYCLR_CLEANUP();

#if !defined(BUILD_RTM)
        if(FAILED(hr)) CLR_Debug::Printf( "Error: %08x\r\n", hr );
#endif

        TINYCLR_CLEANUP_END();
    }

    HRESULT CheckKnownAssembliesForNonXIP( char** start, char** end )
    {
        //--//
        TINYCLR_HEADER();

        BlockStorageDevice *device;
        ByteAddress datByteAddress;
        UINT32 datSize = ROUNDTOMULTIPLE((UINT32)(*end)- (UINT32)(*start), CLR_UINT32);

        if (BlockStorageList::FindDeviceForPhysicalAddress( &device, (UINT32)(*start), datByteAddress ) && device != NULL)
        {    
            const BlockDeviceInfo * deviceInfo=device->GetDeviceInfo();

            if (!deviceInfo->Attribute.SupportsXIP)
            {
                BYTE * datAssembliesBuffer = (BYTE*)CLR_RT_Memory::Allocate_And_Erase( datSize, CLR_RT_HeapBlock ::HB_Unmovable );  CHECK_ALLOCATION(datAssembliesBuffer);

                if ( !device->Read( datByteAddress, datSize, datAssembliesBuffer ))
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
                }
                *start = (char *)datAssembliesBuffer;
                *end = (char *)((UINT32) datAssembliesBuffer + (UINT32)datSize);

            }
        }

        // else data in RAM
        TINYCLR_NOCLEANUP();
    }


    HRESULT LoadKnownAssemblies( char* start, char* end )
    {
        //--//
        TINYCLR_HEADER();
        char *assStart = start;
        char *assEnd = end;
        const CLR_RECORD_ASSEMBLY* header;

        TINYCLR_CHECK_HRESULT(CheckKnownAssembliesForNonXIP( &assStart, &assEnd ));
#if !defined(BUILD_RTM)
        CLR_Debug::Printf(" Loading start at %x, end %x\r\n", (UINT32)assStart, (UINT32)assEnd);
#endif 

        g_buildCRC = SUPPORT_ComputeCRC( assStart, (UINT32)assEnd -(UINT32) assStart, 0 );


        header = (const CLR_RECORD_ASSEMBLY*)assStart;

        while((char*)header + sizeof(CLR_RECORD_ASSEMBLY) < assEnd && header->GoodAssembly())
        {
            CLR_RT_Assembly* assm;

            // Creates instance of assembly, sets pointer to native functions, links to g_CLR_RT_TypeSystem 
            TINYCLR_CHECK_HRESULT(LoadAssembly( header, assm ));
            
            header = (const CLR_RECORD_ASSEMBLY*)ROUNDTOMULTIPLE((size_t)header + header->TotalSize(), CLR_UINT32);
        }
        
        TINYCLR_NOCLEANUP();
    }


    HRESULT ContiguousBlockAssemblies( BlockStorageStream stream, BOOL isXIP ) 
    {
        TINYCLR_HEADER();

        const CLR_RECORD_ASSEMBLY* header;
        BYTE * assembliesBuffer ;
        INT32  headerInBytes = sizeof(CLR_RECORD_ASSEMBLY);
        BYTE * headerBuffer  = NULL;

        if(!isXIP)
        {
            headerBuffer = (BYTE*)CLR_RT_Memory::Allocate( headerInBytes, true );  CHECK_ALLOCATION(headerBuffer);
            CLR_RT_Memory::ZeroFill( headerBuffer, headerInBytes );
        }

        while(TRUE)
        {
            if(!stream.Read( &headerBuffer, headerInBytes )) break;

            header = (const CLR_RECORD_ASSEMBLY*)headerBuffer;

            // check header first before read
            if(!header->GoodHeader())
            {
                break;
            }

            UINT32 AssemblySizeInByte = ROUNDTOMULTIPLE(header->TotalSize(), CLR_UINT32);

            if(!isXIP)
            {
                // read the assemblies
                assembliesBuffer = (BYTE*)CLR_RT_Memory::Allocate_And_Erase( AssemblySizeInByte, CLR_RT_HeapBlock ::HB_Unmovable );
                
                if (!assembliesBuffer) 
                {
                    // release the headerbuffer which has being used and leave
                    CLR_RT_Memory::Release( headerBuffer );
                    
                    TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_MEMORY);
                }
            }

            stream.Seek( -headerInBytes );

            if(!stream.Read( &assembliesBuffer, AssemblySizeInByte )) break;

            header = (const CLR_RECORD_ASSEMBLY*)assembliesBuffer;

            if(!header->GoodAssembly())
            {
                if(!isXIP) CLR_RT_Memory::Release( assembliesBuffer );
                break;
            }
                
            // we have good Assembly 

            CLR_RT_Assembly* assm;

            CLR_Debug::Printf( "Attaching deployed file.\r\n" );

            // Creates instance of assembly, sets pointer to native functions, links to g_CLR_RT_TypeSystem 
            if (FAILED(LoadAssembly( header, assm ) ))
            {   
                if(!isXIP) CLR_RT_Memory::Release( assembliesBuffer );
                break;
            }
            assm->m_flags |= CLR_RT_Assembly::c_Deployed;
        }
        if(!isXIP) CLR_RT_Memory::Release( headerBuffer );
        
        TINYCLR_NOCLEANUP();
    }


    HRESULT LoadDeploymentAssemblies( UINT32 memoryUsage )
    {
        TINYCLR_HEADER();

        BlockStorageStream      stream;
        const BlockDeviceInfo* deviceInfo;

        // find the block            
        if (!stream.Initialize( memoryUsage ))
        {
#if !defined(BUILD_RTM)
            CLR_Debug::Printf( "ERROR: Could not find device for DEPLOYMENT usage\r\n" );
#endif            
            TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
        }

        do
        {
            deviceInfo = stream.Device->GetDeviceInfo();
            
            ContiguousBlockAssemblies( stream, deviceInfo->Attribute.SupportsXIP );
        }
        while(stream.NextStream());
        
        TINYCLR_NOCLEANUP();
    }

    void Cleanup()
    {
        g_CLR_RT_Persistence_Manager.Uninitialize();

        CLR_RT_ExecutionEngine::DeleteInstance();

#if defined(_WIN32)
        memset( &g_CLR_RT_Persistence_Manager, 0, sizeof(g_CLR_RT_Persistence_Manager) );
        memset( &g_CLR_RT_ExecutionEngine, 0, sizeof(g_CLR_RT_ExecutionEngine));
        memset( &g_CLR_RT_WellKnownTypes, 0, sizeof(g_CLR_RT_WellKnownTypes));

        memset( &g_CLR_RT_WellKnownMethods, 0, sizeof(g_CLR_RT_WellKnownMethods));
        memset( &g_CLR_RT_TypeSystem, 0, sizeof(g_CLR_RT_TypeSystem));
        memset( &g_CLR_RT_EventCache, 0, sizeof(g_CLR_RT_EventCache));
        memset( &g_CLR_RT_GarbageCollector, 0, sizeof(g_CLR_RT_GarbageCollector));
        memset( &g_CLR_HW_Hardware, 0, sizeof(g_CLR_HW_Hardware));
#endif

        m_fInitialized = false;
    }

    Settings()
    {
        m_fInitialized = false;
#if defined(_WIN32)
        BuildOptions();
#endif
    }

#if defined(_WIN32)
    ~Settings()
    {
        for(CLR_RT_ParseOptions::BufferMapIter it = m_assemblies.begin(); it != m_assemblies.end(); it++)
        {
            delete it->second;
        }

        m_assemblies.clear();                            // CLR_RT_ParseOptions::BufferMap m_assemblies;
    }

    struct Command_Call : CLR_RT_ParseOptions::Command
    {
        typedef HRESULT (Settings::*FPN)( CLR_RT_ParseOptions::ParameterList* params );

        Settings& m_parent;
        FPN       m_call;

        Command_Call( Settings& parent, FPN call, LPCWSTR szName, LPCWSTR szDescription ) 
            : CLR_RT_ParseOptions::Command( szName, szDescription ), m_parent(parent), m_call(call)
        {
        }

        virtual HRESULT Execute()
        {
            return (m_parent.*m_call)( &m_params );
        }
    };

#define PARAM_GENERIC(parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Generic( parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define OPTION_CALL(fpn,optName,optDesc)   cmd   = new Command_Call( *this, &Settings::fpn, optName, optDesc ); m_commands.push_back( cmd )
#define PARAM_EXTRACT_STRING(lst,idx)    ((CLR_RT_ParseOptions::Parameter_Generic*)(*lst)[idx])->m_data.c_str()

    void BuildOptions()
    {
        CLR_RT_ParseOptions::Command*   cmd;
        CLR_RT_ParseOptions::Parameter* param;

        OPTION_CALL( Cmd_Load, L"-load", L"Loads an assembly formatted for TinyCLR" );
        PARAM_GENERIC( L"<file>", L"File to load"                                   );
        
        OPTION_CALL( Cmd_LoadDatabase, L"-loadDatabase", L"Loads a set of assemblies" );
        PARAM_GENERIC( L"<file>", L"Image to load"                                    );

        OPTION_CALL( Cmd_Resolve, L"-resolve", L"Tries to resolve cross-assembly references" );
    }    

    HRESULT CheckAssemblyFormat( CLR_RECORD_ASSEMBLY* header, LPCWSTR src )
    {
        TINYCLR_HEADER();

        if(header->GoodAssembly() == false)
        {
            wprintf( L"Invalid assembly format for '%s': ", src );
            for(int i=0; i<sizeof(header->marker); i++)
            {
                wprintf( L"%02x", header->marker[i] );
            }
            wprintf( L"\n" );

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_Load( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR              szName = PARAM_EXTRACT_STRING( params, 0 );
        CLR_RT_Buffer*       buffer = new CLR_RT_Buffer(); 
        CLR_RECORD_ASSEMBLY* header;

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szName, *buffer ));

        header = (CLR_RECORD_ASSEMBLY*)&(*buffer)[0]; TINYCLR_CHECK_HRESULT(CheckAssemblyFormat( header, szName ));

        m_assemblies[szName] = buffer;

        TINYCLR_CLEANUP();

        if(FAILED(hr))
        {
            delete buffer;
        }

        TINYCLR_CLEANUP_END();
    }


    HRESULT Cmd_LoadDatabase( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(!m_fInitialized)
        {
            CLR_RT_ExecutionEngine::CreateInstance();
        }

        {
            LPCWSTR              szFile = PARAM_EXTRACT_STRING( params, 0 );
            CLR_RT_Buffer        buffer;
            CLR_RECORD_ASSEMBLY* header;
            CLR_RECORD_ASSEMBLY* headerEnd;
            std::wstring         strName;
            
            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szFile, buffer ));

            header    = (CLR_RECORD_ASSEMBLY*)&buffer[0              ];
            headerEnd = (CLR_RECORD_ASSEMBLY*)&buffer[buffer.size()-1];
            
            while(header + 1 <= headerEnd && header->GoodAssembly())
            {
                CLR_RT_Buffer*       bufferSub = new CLR_RT_Buffer(); 
                CLR_RECORD_ASSEMBLY* headerSub;
                CLR_RT_Assembly*     assm;
            
                bufferSub->resize( header->TotalSize() );
            
                headerSub = (CLR_RECORD_ASSEMBLY*)&(*bufferSub)[0]; 
            
                if((CLR_UINT8*)header + header->TotalSize() > (CLR_UINT8*)headerEnd)
                {
                    //checksum passed, but not enough data in assembly
                    _ASSERTE(FALSE);
                    delete bufferSub;
                    break;
                }
                memcpy( headerSub, header, header->TotalSize() );
            
                m_assemblies[strName] = bufferSub;
            
                if(FAILED(hr = CLR_RT_Assembly::CreateInstance( headerSub, assm )))
                {
                    delete bufferSub;
                    break;
                }
            
                CLR_RT_UnicodeHelper::ConvertFromUTF8( assm->m_szName, strName ); m_assemblies[strName] = bufferSub;
            
                assm->DestroyInstance();
            
                header = (CLR_RECORD_ASSEMBLY*)ROUNDTOMULTIPLE( (size_t)header + header->TotalSize(), CLR_UINT32 );
            }
        }

        TINYCLR_CLEANUP();

        if(!m_fInitialized)
        {
            CLR_RT_ExecutionEngine::DeleteInstance();
        }

        TINYCLR_CLEANUP_END();
    }

    HRESULT Cmd_Resolve( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        bool fError = false;

        TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
        {
            const CLR_RECORD_ASSEMBLYREF* src = (const CLR_RECORD_ASSEMBLYREF*)pASSM->GetTable( TBL_AssemblyRef );
            for(int i=0; i<pASSM->m_pTablesSize[TBL_AssemblyRef]; i++, src++)
            {
                LPCSTR szName = pASSM->GetString( src->name );

                if(g_CLR_RT_TypeSystem.FindAssembly( szName, &src->version, true ) == NULL)
                {
                    printf( "Missing assembly: %s (%d.%d.%d.%d)\n", szName, src->version.iMajorVersion, src->version.iMinorVersion, src->version.iBuildNumber, src->version.iRevisionNumber );

                    fError = true;
                }
            }
        }
        TINYCLR_FOREACH_ASSEMBLY_END();

        if(fError) TINYCLR_SET_AND_LEAVE(CLR_E_ENTRY_NOT_FOUND);

        TINYCLR_CHECK_HRESULT(g_CLR_RT_TypeSystem.ResolveAll());

        TINYCLR_NOCLEANUP();
    }
#endif //#if defined(_WIN32)

};

static Settings s_ClrSettings;

//--//

void ClrExit()
{
    NATIVE_PROFILE_CLR_STARTUP();
    CLR_EE_DBG_SET(ExitPending);
}

void ClrReboot()
{
    NATIVE_PROFILE_CLR_STARTUP();
    CLR_EE_REBOOT_SET(ClrOnly);
    CLR_EE_DBG_SET(RebootPending);
}


#if defined(_WIN32)
HRESULT ClrLoadPE( LPCWSTR szPeFilePath )
{
    CLR_RT_StringVector vec;

    vec.push_back(L"-load");
        
    vec.push_back(szPeFilePath);
    
    return s_ClrSettings.ProcessOptions(vec);
}

HRESULT ClrLoadDAT( LPCWSTR szDatFilePath )
{
    CLR_RT_StringVector vec;

    vec.push_back(L"-loadDatabase");
        
    vec.push_back(szDatFilePath);
    
    return s_ClrSettings.ProcessOptions(vec);
}
#endif


void ClrStartup( CLR_SETTINGS params )
{
    NATIVE_PROFILE_CLR_STARTUP();
    //Settings settings;
    ASSERT(sizeof(CLR_RT_HeapBlock_Raw) == sizeof(CLR_RT_HeapBlock));
    bool softReboot;

    do
    {
        softReboot = false;

        CLR_RT_Assembly::InitString();

#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "\r\nTinyCLR (Build %d.%d.%d.%d)\r\n\r\n", VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION );
#endif

        CLR_RT_Memory::Reset         ();
        
#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "Starting...\r\n" );
#endif
        if(SUCCEEDED(s_ClrSettings.Initialize(params)))
        {
            if(SUCCEEDED(s_ClrSettings.Load()))
            {
#if !defined(BUILD_RTM)
                CLR_Debug::Printf( "Ready.\r\n" );
#endif

#if defined(_WIN32)
                (void)g_CLR_RT_ExecutionEngine.Execute( params.EmulatorArgs, params.MaxContextSwitches );
#else
                (void)g_CLR_RT_ExecutionEngine.Execute( NULL, params.MaxContextSwitches );
#endif

#if !defined(BUILD_RTM)
                CLR_Debug::Printf( "Done.\r\n" );
#endif
            }
        }

        if( CLR_EE_DBG_IS_NOT( RebootPending ))
        {
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            CLR_EE_DBG_SET_MASK(State_ProgramExited, State_Mask);
            CLR_EE_DBG_EVENT_BROADCAST(CLR_DBG_Commands::c_Monitor_ProgramExit, 0, NULL, WP_Flags::c_NonCritical);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

            if(params.EnterDebuggerLoopAfterExit)
            {
                CLR_DBG_Debugger::Debugger_WaitForCommands();
            }
        }

        // DO NOT USE 'ELSE IF' here because the state can change in Debugger_WaitForCommands() call
        
        if( CLR_EE_DBG_IS( RebootPending ))
        {
            if(CLR_EE_REBOOT_IS( ClrOnly ))
            {
                softReboot = true;

                params.WaitForDebugger = CLR_EE_REBOOT_IS(ClrOnlyStopDebugger);
                
                s_ClrSettings.Cleanup();

                HAL_Uninitialize();

                SmartPtr_IRQ::ForceDisabled();

                //re-init the hal for the reboot (initially it is called in bootentry)
                HAL_Initialize();

                // make sure interrupts are back on
                SmartPtr_IRQ::ForceEnabled();
            }
            else
            {
                CPU_Reset();
            }
        }
    } while( softReboot );
}

