////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\core\Core.h"
#include <TinyCLR_Debugging.h>
#include <MFUpdate_decl.h>

#include <crypto.h>

#if 0
#define TRACE0( msg, ...) debug_printf( msg ) 
#define TRACE( msg, ...) debug_printf( msg, __VA_ARGS__ ) 
char const* const AccessMemoryModeNames[] = {
"AccessMemory_Check",
"AccessMemory_Read", 
"AccessMemory_Write",
"AccessMemory_Erase"
};
#else
#define TRACE0(msg,...)
#define TRACE(msg,...)
#endif

//--//

BlockStorageDevice* CLR_DBG_Debugger::m_deploymentStorageDevice = NULL;

//--//

void CLR_DBG_Debugger::Debugger_WaitForCommands()
{
    NATIVE_PROFILE_CLR_DEBUGGER();

#if !defined(BUILD_RTM)
    hal_fprintf(STREAM_LCD, "\r\nWaiting for debug commands...\r\n");
    CLR_Debug::Printf( "Waiting for debug commands...\r\n" );
#endif

    while( !CLR_EE_DBG_IS(RebootPending) && !CLR_EE_DBG_IS(ExitPending) )
    {
        g_CLR_RT_ExecutionEngine.DebuggerLoop();
    }
}

void CLR_DBG_Debugger::Debugger_Discovery()
{
    NATIVE_PROFILE_CLR_DEBUGGER();

    CLR_INT32 wait_sec = 5;

    CLR_INT64 expire = Time_GetMachineTime() + (wait_sec * TIME_CONVERSION__TO_SECONDS);

    //
    // Send "presence" ping.
    //
    CLR_DBG_Commands::Monitor_Ping cmd;

    cmd.m_source = CLR_DBG_Commands::Monitor_Ping::c_Ping_Source_TinyCLR;

    while(true)
    {
        CLR_EE_DBG_EVENT_BROADCAST(CLR_DBG_Commands::c_Monitor_Ping, sizeof(cmd), &cmd, WP_Flags::c_NoCaching | WP_Flags::c_NonCritical);

        // if we support soft reboot and the debugger is not stopped then we don't need to connect the debugger
        if(!CLR_EE_DBG_IS(Stopped) && ::CPU_IsSoftRebootSupported())
        {
            break;
        }

        g_CLR_RT_ExecutionEngine.DebuggerLoop();

        if(CLR_EE_DBG_IS(Enabled))
        {
            //
            // Debugger on the other side, let's exit the discovery loop.
            //
            CLR_Debug::Printf( "Found debugger!\r\n" );
            break;
        }

        CLR_INT64 now = Time_GetMachineTime();

        if(expire < now)
        {
            //
            // No response in time...
            //
            CLR_Debug::Printf( "No debugger!\r\n" );
            break;
        }
    }

    g_CLR_RT_ExecutionEngine.WaitForDebugger();
}


////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_DBG_Debugger::CreateInstance()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_HEADER();

    int iDebugger = 0;

    g_CLR_DBG_Debuggers = (CLR_DBG_Debugger*)&g_scratchDebugger[ 0 ];

    CLR_RT_Memory::ZeroFill( g_scratchDebuggerMessaging, sizeof(CLR_Messaging) * NUM_DEBUGGERS );

    CLR_RT_Memory::ZeroFill( g_CLR_DBG_Debuggers, sizeof(CLR_DBG_Debugger) * NUM_DEBUGGERS );

    TINYCLR_FOREACH_DEBUGGER_NO_TEMP()
    {
        if(HalSystemConfig.DebuggerPorts[ iDebugger ] == HalSystemConfig.MessagingPorts[ 0 ])
        {
            g_CLR_DBG_Debuggers[ iDebugger ].m_messaging = &g_CLR_Messaging[ 0 ];
        }
        else
        {
            g_CLR_DBG_Debuggers[iDebugger].m_messaging = (CLR_Messaging*)&g_scratchDebuggerMessaging[ iDebugger ];
        }

        TINYCLR_CHECK_HRESULT(g_CLR_DBG_Debuggers[ iDebugger ].Debugger_Initialize( HalSystemConfig.DebuggerPorts[ iDebugger ] ));
        iDebugger++;
    }
    TINYCLR_FOREACH_DEBUGGER_END();

    BlockStorageStream stream;

    if (stream.Initialize( BlockUsage::DEPLOYMENT ))
    {
        m_deploymentStorageDevice = stream.Device;
    }
    else
    {
        m_deploymentStorageDevice = NULL;
    }

    MFUpdate_Initialize();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_DBG_Debugger::Debugger_Initialize( COM_HANDLE port )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_HEADER();

    m_messaging->Initialize( port, c_Debugger_Lookup_Request, c_Debugger_Lookup_Request_count, c_Debugger_Lookup_Reply, c_Debugger_Lookup_Reply_count, (void*)this );

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

HRESULT CLR_DBG_Debugger::DeleteInstance()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_HEADER();

    TINYCLR_FOREACH_DEBUGGER(dbg)
    {
        dbg.Debugger_Cleanup();
    }
    TINYCLR_FOREACH_DEBUGGER_END();

    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_DBG_Debugger::Debugger_Cleanup()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    m_messaging->Cleanup();
}

//--//

void CLR_DBG_Debugger::ProcessCommands()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    m_messaging->m_controller.AdvanceState();
}

void CLR_DBG_Debugger::PurgeCache()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    m_messaging->PurgeCache();
}

void CLR_DBG_Debugger::BroadcastEvent( UINT32 cmd, UINT32 payloadSize, UINT8* payload, UINT32 flags )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_FOREACH_DEBUGGER(dbg)
    {
        dbg.m_messaging->SendEvent( cmd, payloadSize, payload, flags );
    }
    TINYCLR_FOREACH_DEBUGGER_END();
}

//--//

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if defined(TINYCLR_APPDOMAINS)

CLR_RT_AppDomain* CLR_DBG_Debugger::GetAppDomainFromID( CLR_UINT32 id )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_FOREACH_NODE(CLR_RT_AppDomain, appDomain, g_CLR_RT_ExecutionEngine.m_appDomains)
    {
        if(appDomain->m_id == id) return appDomain;
    }
    TINYCLR_FOREACH_NODE_END();

    return NULL;
}
#endif //TINYCLR_APPDOMAINS

CLR_RT_Thread* CLR_DBG_Debugger::GetThreadFromPid( CLR_UINT32 pid )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,g_CLR_RT_ExecutionEngine.m_threadsReady)
    {
        if(th->m_pid == pid) return th;
    }
    TINYCLR_FOREACH_NODE_END();

    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,g_CLR_RT_ExecutionEngine.m_threadsWaiting)
    {
        if(th->m_pid == pid) return th;
    }
    TINYCLR_FOREACH_NODE_END();

    return NULL;
}

HRESULT CLR_DBG_Debugger::CreateListOfThreads( CLR_DBG_Commands::Debugging_Thread_List::Reply*& cmdReply, int& totLen )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_HEADER();

    CLR_UINT32* pidDst;
    int         num;

    num = g_CLR_RT_ExecutionEngine.m_threadsReady  .NumOfNodes() +
          g_CLR_RT_ExecutionEngine.m_threadsWaiting.NumOfNodes();

    totLen = sizeof(*cmdReply) + (num-1) * sizeof(CLR_UINT32);

    cmdReply = (CLR_DBG_Commands::Debugging_Thread_List::Reply*)CLR_RT_Memory::Allocate_And_Erase( totLen, true ); CHECK_ALLOCATION(cmdReply);

    cmdReply->m_num = num;

    pidDst = cmdReply->m_pids;

    TINYCLR_FOREACH_NODE(CLR_RT_Thread,thSrc,g_CLR_RT_ExecutionEngine.m_threadsReady)
    {
        *pidDst++ = thSrc->m_pid;
    }
    TINYCLR_FOREACH_NODE_END();

    TINYCLR_FOREACH_NODE(CLR_RT_Thread,thSrc,g_CLR_RT_ExecutionEngine.m_threadsWaiting)
    {
        *pidDst++ = thSrc->m_pid;
    }
    TINYCLR_FOREACH_NODE_END();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_DBG_Debugger::CreateListOfCalls( CLR_UINT32 pid, CLR_DBG_Commands::Debugging_Thread_Stack::Reply*& cmdReply, int& totLen )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_HEADER();

    CLR_RT_Thread* th = GetThreadFromPid( pid ); FAULT_ON_NULL(th);

    for(int pass=0; pass<2; pass++)
    {
        int num = 0;

        TINYCLR_FOREACH_NODE(CLR_RT_StackFrame,call,th->m_stackFrames)
        {
            if(pass == 1)
            {
                int tmp = num;
                
#ifndef TINYCLR_NO_IL_INLINE
                if(call->m_inlineFrame)
                {
                    CLR_DBG_Commands::Debugging_Thread_Stack::Reply::Call& dst = cmdReply->m_data[ tmp++ ];
                    
                    dst.m_md =              call->m_inlineFrame->m_frame.m_call;
                    dst.m_IP = (CLR_UINT32)(call->m_inlineFrame->m_frame.m_IP - call->m_inlineFrame->m_frame.m_IPStart);
#if defined(TINYCLR_APPDOMAINS)
                    dst.m_appDomainID = call->m_appDomain->m_id;
                    dst.m_flags       = call->m_flags;
#endif
                }
#endif
                CLR_DBG_Commands::Debugging_Thread_Stack::Reply::Call& dst = cmdReply->m_data[ tmp ];

                dst.m_md =              call->m_call;
                dst.m_IP = (CLR_UINT32)(call->m_IP - call->m_IPstart);

                if(dst.m_IP && call != th->CurrentFrame())
                {
                    //With the exception of when the IP is 0, for a breakpoint on Push,
                    //The call->m_IP is the next instruction to execute, not the currently executing one.
                    //For non-leaf frames, this will return the IP within the call.
                    dst.m_IP--;
                }

#if defined(TINYCLR_APPDOMAINS)
                dst.m_appDomainID = call->m_appDomain->m_id;
                dst.m_flags       = call->m_flags;
#endif
            }

#ifndef TINYCLR_NO_IL_INLINE
            if(call->m_inlineFrame)
            {
                num++;
            }
#endif

            num++;
        }
        TINYCLR_FOREACH_NODE_END();

        if(pass == 0)
        {
            totLen = sizeof(*cmdReply) + (num-1) * sizeof(CLR_DBG_Commands::Debugging_Thread_Stack::Reply::Call);

            cmdReply = (CLR_DBG_Commands::Debugging_Thread_Stack::Reply*)CLR_RT_Memory::Allocate_And_Erase( totLen, CLR_RT_HeapBlock::HB_CompactOnFailure ); CHECK_ALLOCATION(cmdReply);

            cmdReply->m_num    = num;
            cmdReply->m_status = th->m_status;
            cmdReply->m_flags  = th->m_flags;
        }
    }

    TINYCLR_NOCLEANUP();
}

#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_DBG_Debugger::Monitor_Ping( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    bool fStopOnBoot = true;

    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;

    //
    // There's someone on the other side!!
    //
    CLR_EE_DBG_SET( Enabled );

    if((msg->m_header.m_flags & WP_Flags::c_Reply      ) == 0)
    {
        CLR_DBG_Commands::Monitor_Ping::Reply cmdReply;
        CLR_DBG_Commands::Monitor_Ping       *cmd       = (CLR_DBG_Commands::Monitor_Ping*)msg->m_payload;

        // default is to stop the debugger (backwards compatibility)
        fStopOnBoot = (cmd != NULL) && (cmd->m_dbg_flags & CLR_DBG_Commands::Monitor_Ping::c_Ping_DbgFlag_Stop);

        cmdReply.m_source    = CLR_DBG_Commands::Monitor_Ping::c_Ping_Source_TinyCLR;

#if !defined(NETMF_TARGET_BIG_ENDIAN)
        cmdReply.m_dbg_flags = CLR_EE_DBG_IS(State_ProgramExited) != 0 ? CLR_DBG_Commands::Monitor_Ping::c_Ping_DbgFlag_AppExit : 0;
#else
        cmdReply.m_dbg_flags  = CLR_DBG_Commands::Monitor_Ping::c_Ping_DbgFlag_BigEndian;
        cmdReply.m_dbg_flags |= CLR_EE_DBG_IS(State_ProgramExited) != 0 ? CLR_DBG_Commands::Monitor_Ping::c_Ping_DbgFlag_AppExit : 0;
#endif

        dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );
    }
    else
    {
        CLR_DBG_Commands::Monitor_Ping::Reply *cmdReply = (CLR_DBG_Commands::Monitor_Ping::Reply*)msg->m_payload;

        // default is to stop the debugger (backwards compatibility)
        fStopOnBoot = (cmdReply != NULL) && (cmdReply->m_dbg_flags & CLR_DBG_Commands::Monitor_Ping::c_Ping_DbgFlag_Stop);
    }

    if(CLR_EE_DBG_IS_MASK(State_Initialize, State_Mask))
    {
        if(fStopOnBoot) CLR_EE_DBG_SET(Stopped);
        else            CLR_EE_DBG_CLR(Stopped);
    }

    return true;
}

bool CLR_DBG_Debugger::Monitor_FlashSectorMap( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();

    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;

    if((msg->m_header.m_flags & WP_Flags::c_Reply) == 0)
    {
        struct Flash_Sector
        {
            UINT32 Start;
            UINT32 Length;
            UINT32 Usage;

        } *pData = NULL;

        UINT32 rangeCount = 0;
        UINT32 rangeIndex = 0;

        for(int cnt = 0; cnt < 2; cnt++)
        {
            BlockStorageDevice* device = BlockStorageList::GetFirstDevice();

            if(device == NULL)
            {
                dbg->m_messaging->ReplyToCommand( msg, true, false, NULL, 0 );
                return false;
            }

            if(cnt == 1)
            {
                pData = (struct Flash_Sector*)private_malloc(rangeCount * sizeof(struct Flash_Sector));

                if(pData == NULL)
                {
                    dbg->m_messaging->ReplyToCommand( msg, true, false, NULL, 0 );
                    return false;
                }
            }

            do
            {
                const BlockDeviceInfo* deviceInfo = device->GetDeviceInfo();

                for(UINT32 i = 0; i < deviceInfo->NumRegions;  i++)
                {
                    const BlockRegionInfo* pRegion = &deviceInfo->Regions[ i ];

                    for(UINT32 j = 0; j < pRegion->NumBlockRanges; j++)
                    {

                        if(cnt == 0)
                        {
                            rangeCount++;
                        }
                        else
                        {
                            pData[ rangeIndex ].Start  = pRegion->BlockAddress(pRegion->BlockRanges[ j ].StartBlock);
                            pData[ rangeIndex ].Length = pRegion->BlockRanges[ j ].GetBlockCount() * pRegion->BytesPerBlock;
                            pData[ rangeIndex ].Usage  = pRegion->BlockRanges[ j ].RangeType & BlockRange::USAGE_MASK;
                            rangeIndex++;
                        }
                    }
                }
            }
            while(NULL != (device = BlockStorageList::GetNextDevice( *device )));
        }


        dbg->m_messaging->ReplyToCommand( msg, true, false, (void*)pData, rangeCount * sizeof (struct Flash_Sector) );

        private_free(pData);
    }

    return true;

}

//--//

static const int AccessMemory_Check = 0;
static const int AccessMemory_Read  = 1;
static const int AccessMemory_Write = 2;
static const int AccessMemory_Erase = 3;


bool CLR_DBG_Debugger::CheckPermission( ByteAddress address, int mode )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    bool   hasPermission = false;
    UINT32 regionIndex, rangeIndex;

    m_deploymentStorageDevice->FindRegionFromAddress( address, regionIndex, rangeIndex );
    const BlockRange& range = m_deploymentStorageDevice->GetDeviceInfo()->Regions[ regionIndex ].BlockRanges[ rangeIndex ];


    switch(mode)
    {
        case AccessMemory_Check:
            hasPermission = true;
            break;
        case AccessMemory_Read:
#if defined(BUILD_RTM)
            if(!DebuggerPort_IsUsingSsl(HalSystemConfig.DebuggerPorts[ 0 ]))
                break;
#endif
            switch(range.RangeType)
            {
                case BlockRange::BLOCKTYPE_CONFIG:         // fall through
                case BlockRange::BLOCKTYPE_DIRTYBIT:       // fall through
                case BlockRange::BLOCKTYPE_DEPLOYMENT:     // fall through
                case BlockRange::BLOCKTYPE_FILESYSTEM:     // fall through
                case BlockRange::BLOCKTYPE_STORAGE_A:      // fall through
                case BlockRange::BLOCKTYPE_STORAGE_B:
                case BlockRange::BLOCKTYPE_SIMPLE_A:
                case BlockRange::BLOCKTYPE_SIMPLE_B:
                case BlockRange::BLOCKTYPE_UPDATE:

                    hasPermission = true;
                    break;
            }
            break;
        case AccessMemory_Write:
#if defined(BUILD_RTM)
            if(!DebuggerPort_IsUsingSsl(HalSystemConfig.DebuggerPorts[ 0 ]))
                break;
#endif
            if(range.IsDeployment() || range.IsConfig())
            {
                hasPermission = true;
            }
            else
            {
                hasPermission = DebuggerPort_IsUsingSsl(HalSystemConfig.DebuggerPorts[ 0 ]) == TRUE;
            }
            break;
        case AccessMemory_Erase:
#if defined(BUILD_RTM)
            if(!DebuggerPort_IsUsingSsl(HalSystemConfig.DebuggerPorts[ 0 ]))
                break;
#endif
            switch(range.RangeType)
            {
                case BlockRange::BLOCKTYPE_DEPLOYMENT:
                case BlockRange::BLOCKTYPE_FILESYSTEM:
                case BlockRange::BLOCKTYPE_STORAGE_A:
                case BlockRange::BLOCKTYPE_STORAGE_B:
                case BlockRange::BLOCKTYPE_SIMPLE_A:
                case BlockRange::BLOCKTYPE_SIMPLE_B:
                case BlockRange::BLOCKTYPE_UPDATE:
                case BlockRange::BLOCKTYPE_CONFIG:
                    hasPermission = true;
                    break;
            }
            break;
        default:
            hasPermission = false;
            break;
    }

    return hasPermission;
}

bool CLR_DBG_Debugger::AccessMemory( CLR_UINT32 location, UINT32 lengthInBytes, BYTE* buf, int mode )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TRACE("AccessMemory( 0x%08X, 0x%08x, 0x%08X, %s)\n", location, lengthInBytes, buf, AccessMemoryModeNames[mode] );

    //--//
    UINT32 iRegion, iRange;

    if (m_deploymentStorageDevice->FindRegionFromAddress( location, iRegion, iRange ))
    {
        const BlockDeviceInfo* deviceInfo = m_deploymentStorageDevice->GetDeviceInfo() ;

        // start from the block where the sector sits.
        ByteAddress   accessAddress = location;

        BYTE*         bufPtr           = buf;
        BOOL          success          = TRUE;
        INT32         accessLenInBytes = lengthInBytes;
        INT32         blockOffset      = deviceInfo->Regions[ iRegion ].OffsetFromBlock( accessAddress );

        for(;iRegion < deviceInfo->NumRegions; iRegion++)
        {
            const BlockRegionInfo *pRegion = &deviceInfo->Regions[ iRegion ];

            UINT32 RangeBaseAddress = pRegion->BlockAddress( pRegion->BlockRanges[ iRange ].StartBlock );
            UINT32 blockIndex       = pRegion->BlockIndexFromAddress( accessAddress );
            UINT32 accessMaxLength  = pRegion->BytesPerBlock - blockOffset;

            blockOffset = 0;

            for(;blockIndex < pRegion->NumBlocks; blockIndex++)
            {
                //accessMaxLength =the current largest number of bytes can be read from the block from the address to its block boundary.
                UINT32 NumOfBytes = __min(accessMaxLength, (UINT32)accessLenInBytes);

                accessMaxLength = pRegion->BytesPerBlock;

                if(blockIndex > pRegion->BlockRanges[ iRange ].EndBlock)
                {
                    iRange++;

                    if(iRange >= pRegion->NumBlockRanges)
                    {
                        ASSERT(FALSE);
                        break;
                    }
                }

                // since AccessMemory_Check is always true and will not break from here, no need to check AccessMemory_Check to free memory.
                if(!CheckPermission( accessAddress, mode ))
                {
                    TRACE0("=> Permission check failed!\n");
                    return false;
                }

                switch(mode)
                {
                    case AccessMemory_Check:
                    case AccessMemory_Read:
                        if(deviceInfo->Attribute.SupportsXIP)
                        {
                            memcpy( (BYTE*)bufPtr, (const void*)accessAddress, NumOfBytes );
                            success = TRUE;
                        }
                        else
                        {
                            if (mode == AccessMemory_Check)
                            {
                                bufPtr = (BYTE*) CLR_RT_Memory::Allocate( lengthInBytes, true );

                                if(!bufPtr)
                                {
                                    TRACE0( "=> Failed to allocate data buffer\n");
                                    return false;
                                }
                            }

                            success = m_deploymentStorageDevice->Read( accessAddress , NumOfBytes, (BYTE *)bufPtr );

                            if (mode == AccessMemory_Check)
                            {
                                *(UINT32*)buf = SUPPORT_ComputeCRC( bufPtr, NumOfBytes, *(UINT32*)buf );

                                CLR_RT_Memory::Release( bufPtr );
                            }
                        }
                        break;

                    case AccessMemory_Write:
                        success = m_deploymentStorageDevice->Write( accessAddress , NumOfBytes, (BYTE *)bufPtr, FALSE );
                        break;

                    case AccessMemory_Erase:
                        if (!m_deploymentStorageDevice->IsBlockErased( accessAddress, NumOfBytes ))
                        {
                            success = m_deploymentStorageDevice->EraseBlock( accessAddress );
                        }
                        break;

                     default:
                        break;
                }


                if(!success)
                {
                    break;
                }

                accessLenInBytes -= NumOfBytes;

                if (accessLenInBytes <= 0)
                {
                    break;
                }

                bufPtr        += NumOfBytes;
                accessAddress += NumOfBytes;
            }

            blockIndex = 0;
            iRange     = 0;

           if ((accessLenInBytes <= 0) || (!success))
               break;
        }

    }
    else
    {
    //--// RAM write
        ByteAddress sectAddr = location;

#if defined(_WIN32)

        bool proceed = false;
        void * temp;
        temp = (void *) sectAddr;

        switch(mode)
        {
        case AccessMemory_Check:
            break;

        case AccessMemory_Read:
            proceed = IsBadReadPtr( temp, lengthInBytes ) == false;
            break;

        case AccessMemory_Write:
            proceed = IsBadWritePtr( temp, lengthInBytes ) == false;
            break;
        }

        if(proceed)
#else

        UINT32 sectAddrEnd     = sectAddr + lengthInBytes;
        UINT32 ramStartAddress = HalSystemConfig.RAM1.Base;
        UINT32 ramEndAddress   = ramStartAddress + HalSystemConfig.RAM1.Size ;

        if((sectAddr <ramStartAddress) || (sectAddr >=ramEndAddress) || (sectAddrEnd >ramEndAddress) )
        {
            TRACE(" Invalid address %x and range %x Ram Start %x, Ram end %x\r\n", sectAddr, lengthInBytes, ramStartAddress, ramEndAddress);
            return FALSE;
        }
        else
#endif
        {
            switch(mode)
            {
            case AccessMemory_Check:
                break;

            case AccessMemory_Read:
                memcpy( buf, (const void*)sectAddr, lengthInBytes );
                break;

            case AccessMemory_Write:
                BYTE * memPtr;
                memPtr = (BYTE*)sectAddr;
                memcpy( memPtr, buf, lengthInBytes );
                break;

            case AccessMemory_Erase:
                memPtr = (BYTE*)sectAddr;
                if (lengthInBytes !=0)
                    memset( memPtr, 0xFF, lengthInBytes );
                break;

            default:
                break;
            }
        }
    }
    TRACE0( "=> SUCCESS\n");
    return true;
}

bool CLR_DBG_Debugger::Monitor_ReadMemory( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;

    CLR_DBG_Commands::Monitor_ReadMemory* cmd = (CLR_DBG_Commands::Monitor_ReadMemory*)msg->m_payload;
    UINT8                                 buf[ 1024 ];
    UINT32                                len = cmd->m_length; if(len > sizeof(buf)) len = sizeof(buf);

    if (m_deploymentStorageDevice == NULL) return false;
    dbg->AccessMemory( cmd->m_address, len, buf, AccessMemory_Read );

    dbg->m_messaging->ReplyToCommand( msg, true, false, buf, len );

    return true;

}

bool CLR_DBG_Debugger::Monitor_WriteMemory( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    bool fRet;

    CLR_DBG_Debugger        * dbg = (CLR_DBG_Debugger*)owner;

    CLR_DBG_Commands::Monitor_WriteMemory* cmd = (CLR_DBG_Commands::Monitor_WriteMemory*)msg->m_payload;

    if (m_deploymentStorageDevice == NULL) return false;

    fRet = dbg->AccessMemory( cmd->m_address, cmd->m_length, cmd->m_data, AccessMemory_Write );

    dbg->m_messaging->ReplyToCommand( msg, fRet, false );

    return fRet;
}

bool CLR_DBG_Debugger::Monitor_CheckMemory( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;

    CLR_DBG_Commands::Monitor_CheckMemory*       cmd      = (CLR_DBG_Commands::Monitor_CheckMemory*)msg->m_payload;
    CLR_DBG_Commands::Monitor_CheckMemory::Reply cmdReply;

    dbg->AccessMemory( cmd->m_address, cmd->m_length, (UINT8*)&cmdReply.m_crc, AccessMemory_Check );

    dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

    return true;

}

bool CLR_DBG_Debugger::Monitor_EraseMemory( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    bool                fRet;

    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;

    CLR_DBG_Commands::Monitor_EraseMemory* cmd = (CLR_DBG_Commands::Monitor_EraseMemory*)msg->m_payload;

    if (m_deploymentStorageDevice == NULL) return false;

    fRet = dbg->AccessMemory( cmd->m_address, cmd->m_length, NULL, AccessMemory_Erase );

    dbg->m_messaging->ReplyToCommand( msg, fRet, false );

    return fRet;
}

bool CLR_DBG_Debugger::Monitor_Execute( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;

    CLR_DBG_Commands::Monitor_Execute* cmd = (CLR_DBG_Commands::Monitor_Execute*)msg->m_payload;

#if defined(BUILD_RTM)
    if(!DebuggerPort_IsUsingSsl(HalSystemConfig.DebuggerPorts[ 0 ]))
        return false;
#endif

    dbg->m_messaging->ReplyToCommand( msg, true, false );

    ((void (*)())(size_t)cmd->m_address)();

    return true;
}

bool CLR_DBG_Debugger::Monitor_Reboot( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Monitor_Reboot* cmd = (CLR_DBG_Commands::Monitor_Reboot*)msg->m_payload;

#if defined(BUILD_RTM)
    if(COM_IsSock(dbg->m_messaging->m_port))
    {
        if(!DebuggerPort_IsUsingSsl(HalSystemConfig.DebuggerPorts[ 0 ]))
            return false;
    }
#endif

    if(NULL != cmd)
    {
        if(CLR_DBG_Commands::Monitor_Reboot::c_EnterBootloader == (cmd->m_flags & CLR_DBG_Commands::Monitor_Reboot::c_EnterBootloader))
        {
            dbg->m_messaging->ReplyToCommand( msg, true, false );

            Events_WaitForEvents( 0, 100 ); // give message a little time to be flushed

            HAL_EnterBooterMode();
        }

        if(::CPU_IsSoftRebootSupported ())
        {
            if((CLR_DBG_Commands::Monitor_Reboot::c_ClrRebootOnly == (cmd->m_flags & CLR_DBG_Commands::Monitor_Reboot::c_ClrRebootOnly)))
            {
                CLR_EE_REBOOT_SET(ClrOnly);
            }
            else if((CLR_DBG_Commands::Monitor_Reboot::c_ClrStopDebugger == (cmd->m_flags & CLR_DBG_Commands::Monitor_Reboot::c_ClrStopDebugger)))
            {
                CLR_EE_REBOOT_SET(ClrOnlyStopDebugger);
            }
        }
    }

    CLR_EE_DBG_SET( RebootPending );

    dbg->m_messaging->ReplyToCommand( msg, true, false );

    return true;
}

bool CLR_DBG_Debugger::Monitor_MemoryMap( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Monitor_MemoryMap::Range map[ 2 ];

    map[0].m_address = HalSystemConfig.RAM1.Base;
    map[0].m_length  = HalSystemConfig.RAM1.Size;
    map[0].m_flags   = CLR_DBG_Commands::Monitor_MemoryMap::c_RAM;

    map[1].m_address = HalSystemConfig.FLASH.Base;
    map[1].m_length  = HalSystemConfig.FLASH.Size;
    map[1].m_flags   = CLR_DBG_Commands::Monitor_MemoryMap::c_FLASH;

    dbg->m_messaging->ReplyToCommand( msg, true, false, map, sizeof(map) );

    return true;
}



bool CLR_DBG_Debugger::Monitor_DeploymentMap( WP_Message* msg, void* owner )
{
    return true;
}



//--//

bool CLR_DBG_Debugger::Debugging_Execution_BasePtr( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Execution_BasePtr::Reply cmdReply;

    cmdReply.m_EE = (CLR_UINT32)(size_t)&g_CLR_RT_ExecutionEngine;

    dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Execution_ChangeConditions( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Execution_ChangeConditions* cmd = (CLR_DBG_Commands::Debugging_Execution_ChangeConditions*)msg->m_payload;

    g_CLR_RT_ExecutionEngine.m_iDebugger_Conditions |=  cmd->m_set;
    g_CLR_RT_ExecutionEngine.m_iDebugger_Conditions &= ~cmd->m_reset;

    if((msg->m_header.m_flags & WP_Flags::c_NonCritical) == 0)
    {
        CLR_DBG_Commands::Debugging_Execution_ChangeConditions::Reply cmdReply;

        cmdReply.m_current = g_CLR_RT_ExecutionEngine.m_iDebugger_Conditions;

        dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );
    }

    CLR_RT_EmulatorHooks::Notify_ExecutionStateChanged();

    return true;
}

//--//

static void GetClrReleaseInfo(CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::ClrInfo& clrInfo)
{
#if defined(PLATFORM_SH)
#undef OEMSTR(str) 
#define OEMSTR(str) # str
MfReleaseInfo::Init( clrInfo.m_clrReleaseInfo, VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION, OEMSTR(OEMSYSTEMINFOSTRING), hal_strlen_s(OEMSTR(OEMSYSTEMINFOSTRING)) );
#undef OEMSTR(str)
#else
    MfReleaseInfo::Init( clrInfo.m_clrReleaseInfo, VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION, OEMSYSTEMINFOSTRING, hal_strlen_s(OEMSYSTEMINFOSTRING) );
#endif

    if ( g_CLR_RT_TypeSystem.m_assemblyMscorlib &&
         g_CLR_RT_TypeSystem.m_assemblyMscorlib->m_header)
    {
        const CLR_RECORD_VERSION* mscorlibVer = & (g_CLR_RT_TypeSystem.m_assemblyMscorlib->m_header->version);
        MFVersion::Init(  clrInfo.m_TargetFrameworkVersion,
                        mscorlibVer->iMajorVersion, mscorlibVer->iMinorVersion,
                        mscorlibVer->iBuildNumber, mscorlibVer->iRevisionNumber
                        );
    }
    else
    {
        MFVersion::Init( clrInfo.m_TargetFrameworkVersion, 0, 0, 0, 0 );
    }
}


void MfReleaseInfo::Init(MfReleaseInfo& mfReleaseInfo, UINT16 major, UINT16 minor, UINT16 build, UINT16 revision, const char *info, size_t infoLen)
{
    MFVersion::Init( mfReleaseInfo.version, major, minor, build, revision );
    mfReleaseInfo.infoString[ 0 ] = 0;
    if ( NULL != info && infoLen > 0 )
    {
        const size_t len = MIN(infoLen, sizeof(mfReleaseInfo.infoString)-1);
        hal_strncpy_s( (char*)&mfReleaseInfo.infoString[ 0 ], sizeof(mfReleaseInfo.infoString), info, len );
    }
}

//--//

bool CLR_DBG_Debugger::Debugging_Execution_QueryCLRCapabilities( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities* cmd = (CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities*)msg->m_payload;

    CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::ReplyUnion reply;
    reply.u_capsFlags = 0;

    CLR_UINT8* data   = NULL;
    int size          = 0;
    bool fSuccess     = true;

    memset(&reply, 0, sizeof(reply));

    switch(cmd->m_cmd)
    {
        case CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags:

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_FloatingPoint;
#endif

            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_ExceptionFilters;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_SourceLevelDebugging;
            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_IncrementalDeployment;
            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_ThreadCreateEx;
#endif

#if defined(TINYCLR_PROFILE_NEW)
            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_Profiling;
#if defined(TINYCLR_PROFILE_NEW_CALLS)
            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_Profiling_Calls;
#endif
#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_Profiling_Allocations;
#endif
#endif

#if defined(TINYCLR_APPDOMAINS)
            reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_AppDomains;
#endif

            if (::CPU_IsSoftRebootSupported ())
            {
                reply.u_capsFlags |= CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityFlags_SoftReboot;
            }

            data = (CLR_UINT8*)&reply.u_capsFlags;
            size = sizeof(reply.u_capsFlags);
            break;

        case CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityLCD:
            reply.u_LCD.m_width  = LCD_SCREEN_WIDTH;
            reply.u_LCD.m_height = LCD_SCREEN_HEIGHT;
            reply.u_LCD.m_bpp    = LCD_SCREEN_BPP;

            data = (CLR_UINT8*)&reply.u_LCD;
            size = sizeof(reply.u_LCD);
            break;

        case CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_CapabilityVersion:
#if defined(__GNUC__)
            reply.u_SoftwareVersion.m_compilerVersion = __GNUC__;
#elif defined(__ARMCC_VERSION)
            reply.u_SoftwareVersion.m_compilerVersion = __ARMCC_VERSION;
#elif defined(_ARCVER)
            reply.u_SoftwareVersion.m_compilerVersion = _ARCVER;
#elif defined(__RENESAS__)
            reply.u_SoftwareVersion.m_compilerVersion = __RENESAS_VERSION__;
#else
            reply.u_SoftwareVersion.m_compilerVersion = -1;
#endif

#if defined(__DATE__)
            hal_strncpy_s( reply.u_SoftwareVersion.m_buildDate, sizeof(reply.u_SoftwareVersion.m_buildDate), __DATE__,  hal_strlen_s(__DATE__) );
#else
            hal_strncpy_s( reply.u_SoftwareVersion.m_buildDate, sizeof(reply.u_SoftwareVersion.m_buildDate), "UNKNOWN",  hal_strlen_s("UNKNOWN") );
#endif
            data = (CLR_UINT8*)&reply.u_SoftwareVersion;
            size = sizeof(reply.u_SoftwareVersion);
            break;

        case CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_HalSystemInfo:
            if(GetHalSystemInfo( reply.u_HalSystemInfo ) == TRUE)
            {
                data = (CLR_UINT8*)&reply.u_HalSystemInfo;
                size = sizeof(reply.u_HalSystemInfo);
            }
            else
            {
                fSuccess = false;
            }
            break;

        case CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_ClrInfo:
            GetClrReleaseInfo(reply.u_ClrInfo);
            data = (CLR_UINT8*)&reply.u_ClrInfo;
            size = sizeof(reply.u_ClrInfo);
            break;

        case CLR_DBG_Commands::Debugging_Execution_QueryCLRCapabilities::c_SolutionReleaseInfo:
            if(Solution_GetReleaseInfo(reply.u_SolutionReleaseInfo) == TRUE)
            {
                data = (CLR_UINT8*)&reply.u_SolutionReleaseInfo;
                size = sizeof(reply.u_SolutionReleaseInfo);
            }
            else
            {
                fSuccess = false;
            }
            break;

        default:
            fSuccess = false;
            break;
    }

    dbg->m_messaging->ReplyToCommand( msg, fSuccess, false, data, size );

    return true;
}


bool CLR_DBG_Debugger::Debugging_Execution_Allocate( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Execution_Allocate*       cmd = (CLR_DBG_Commands::Debugging_Execution_Allocate*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Execution_Allocate::Reply reply;

    reply.m_address = (CLR_UINT32)(size_t)CLR_RT_Memory::Allocate( cmd->m_size, CLR_RT_HeapBlock::HB_CompactOnFailure );

    if(!reply.m_address) return false;

    dbg->m_messaging->ReplyToCommand( msg, true, false, &reply, sizeof(reply) );

    return true;
}

bool CLR_DBG_Debugger::Debugging_UpgradeToSsl(WP_Message* msg, void* owner )
{
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_UpgradeToSsl*       cmd = (CLR_DBG_Commands::Debugging_UpgradeToSsl*)msg->m_payload;
    CLR_DBG_Commands::Debugging_UpgradeToSsl::Reply reply;

    if(!DebuggerPort_IsSslSupported(HalSystemConfig.DebuggerPorts[0]))
    {
        return false;
    }

    reply.m_success = 1;

    dbg->m_messaging->ReplyToCommand( msg, true, true, &reply, sizeof(reply) );

    Events_WaitForEvents(0, 300);

    return TRUE == DebuggerPort_UpgradeToSsl(HalSystemConfig.DebuggerPorts[0], cmd->m_flags);
}

static CLR_UINT32 s_missingPkts[64];

bool CLR_DBG_Debugger::Debugging_MFUpdate_Start (WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_MFUpdate_Start*       cmd = (CLR_DBG_Commands::Debugging_MFUpdate_Start*)msg->m_payload;
    CLR_DBG_Commands::Debugging_MFUpdate_Start::Reply reply, *pReply;
    CLR_INT32 replySize = sizeof(reply);
    MFUpdateHeader header;

    pReply = &reply;    

    TINYCLR_CLEAR(header);

    header.Version.usMajor = cmd->m_versionMajor;
    header.Version.usMinor = cmd->m_versionMinor;
    header.UpdateID        = cmd->m_updateId;
    header.UpdateType      = cmd->m_updateType;
    header.UpdateSubType   = cmd->m_updateSubType;
    header.UpdateSize      = cmd->m_updateSize;
    header.PacketSize      = cmd->m_updatePacketSize;

    reply.m_updateHandle = MFUpdate_InitUpdate(cmd->m_provider, header);

    dbg->m_messaging->ReplyToCommand( msg, true, false, pReply, replySize );
    
    return true;
}

bool CLR_DBG_Debugger::Debugging_MFUpdate_AuthCommand( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_MFUpdate_AuthCommand*       cmd = (CLR_DBG_Commands::Debugging_MFUpdate_AuthCommand*)msg->m_payload;
    CLR_DBG_Commands::Debugging_MFUpdate_AuthCommand::Reply reply, *pReply;
    INT32 respLen = 0;
    INT32 replySize = sizeof(reply);

    TINYCLR_CLEAR(reply);

    pReply = &reply;

    if(MFUpdate_AuthCommand(cmd->m_updateHandle, cmd->m_authCommand, cmd->m_authArgs, cmd->m_authArgsSize, NULL, respLen))
    {
        if(respLen > 0)
        {
            int cmdSize = respLen + offsetof(CLR_DBG_Commands::Debugging_MFUpdate_AuthCommand::Reply, m_response);
            
            CLR_DBG_Commands::Debugging_MFUpdate_AuthCommand::Reply* pTmp = (CLR_DBG_Commands::Debugging_MFUpdate_AuthCommand::Reply*)private_malloc(cmdSize);

            if(pTmp != NULL)
            {
                if(MFUpdate_AuthCommand(cmd->m_updateHandle, cmd->m_authCommand, cmd->m_authArgs, cmd->m_authArgsSize, pTmp->m_response, respLen))
                {
                    pReply                 = pTmp;
                    replySize              = cmdSize;
                    pReply->m_responseSize = respLen;
                    pReply->m_success      = 1;            
                }
                else
                {
                    private_free(pTmp);
                }
            }
        }
    }

    dbg->m_messaging->ReplyToCommand( msg, true, false, pReply, replySize );

    if(pReply != &reply)
    {
        private_free(pReply);
    }

    return true;
}

bool CLR_DBG_Debugger::Debugging_MFUpdate_Authenticate( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger*                                        dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_MFUpdate_Authenticate*       cmd = (CLR_DBG_Commands::Debugging_MFUpdate_Authenticate*)msg->m_payload;
    CLR_DBG_Commands::Debugging_MFUpdate_Authenticate::Reply reply;
    CLR_INT32 authType = 0;
    CLR_INT32 respLen = sizeof(authType);
    
    memset(&s_missingPkts, 0xFF, sizeof(s_missingPkts));

    reply.m_success = 0;

    MFUpdate_AuthCommand(cmd->m_updateHandle, MFUPDATE_VALIDATION_COMMAND__GET_AUTH_TYPE, NULL, 0, (UINT8*)&authType, respLen);

    if(authType == MFUPDATE_AUTHENTICATION_TYPE__SSL)
    {
        reply.m_success = 1;
        
        // reply early for SSL so that the device will try to upgrad the stream at the same time.
        dbg->m_messaging->ReplyToCommand( msg, true, false, &reply, sizeof(reply) );

        Events_WaitForEvents(0, 400);
    }

    if(MFUpdate_Authenticate(cmd->m_updateHandle, cmd->m_authenticationData, cmd->m_authenticationLen))
    {
        reply.m_success = MFUpdate_Open(cmd->m_updateHandle);

        if(!reply.m_success)
        {
            reply.m_success = MFUpdate_Create(cmd->m_updateHandle);
        }
    }

    dbg->m_messaging->ReplyToCommand( msg, true, false, &reply, sizeof(reply) );

    return true;
}

bool CLR_DBG_Debugger::Debugging_MFUpdate_GetMissingPkts( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger*                                          dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_MFUpdate_GetMissingPkts*       cmd = (CLR_DBG_Commands::Debugging_MFUpdate_GetMissingPkts*)msg->m_payload;
    CLR_DBG_Commands::Debugging_MFUpdate_GetMissingPkts::Reply reply, *pReply;
    CLR_INT32 replySize = sizeof(reply);
    CLR_INT32 int32Cnt = ARRAYSIZE(s_missingPkts);
    CLR_INT32 sizeBytes = (int32Cnt << 2) + offsetof(CLR_DBG_Commands::Debugging_MFUpdate_GetMissingPkts::Reply, m_missingPkts);

    memset(&s_missingPkts, 0xFF, sizeof(s_missingPkts));

    TINYCLR_CLEAR(reply);

    pReply = &reply;

    if(MFUpdate_GetMissingPackets(cmd->m_updateHandle, &s_missingPkts[0], &int32Cnt))
    {
        pReply = (CLR_DBG_Commands::Debugging_MFUpdate_GetMissingPkts::Reply*)private_malloc(sizeBytes);

        if(pReply != NULL)
        {
            pReply->m_missingPktCount = int32Cnt;
            pReply->m_success = 1;
            
            memcpy(pReply->m_missingPkts, s_missingPkts, int32Cnt << 2);

            replySize = sizeBytes;
        }
        else
        {
            pReply = &reply;
        }
    }
    
    dbg->m_messaging->ReplyToCommand( msg, true, false, pReply, replySize );

    if(pReply != &reply)
    {
        private_free(pReply);
    }
    
    return true;    
}

bool CLR_DBG_Debugger::Debugging_MFUpdate_AddPacket(WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_MFUpdate_AddPacket*       cmd = (CLR_DBG_Commands::Debugging_MFUpdate_AddPacket*)msg->m_payload;
    CLR_DBG_Commands::Debugging_MFUpdate_AddPacket::Reply reply;

    if(cmd->m_packetIndex >> 5 < ARRAYSIZE(s_missingPkts))
    {
        if(0 != (s_missingPkts[cmd->m_packetIndex >> 5] & (1ul << (cmd->m_packetIndex % 32))))
        {
            reply.m_success = MFUpdate_AddPacket(cmd->m_updateHandle, cmd->m_packetIndex, &cmd->m_packetData[0], cmd->m_packetLength, (CLR_UINT8*)&cmd->m_packetValidation, sizeof(cmd->m_packetValidation));

            if(reply.m_success)
            {
                s_missingPkts[cmd->m_packetIndex >> 5] &= ~(1ul << (cmd->m_packetIndex % 32));
            }
        }
        else
        {
            reply.m_success = TRUE;
        }
    }
    else
    {
        reply.m_success = MFUpdate_AddPacket(cmd->m_updateHandle, cmd->m_packetIndex, &cmd->m_packetData[0], cmd->m_packetLength, (CLR_UINT8*)&cmd->m_packetValidation, sizeof(cmd->m_packetValidation));
    }

    if(reply.m_success == FALSE) return false;

    dbg->m_messaging->ReplyToCommand( msg, true, false, &reply, sizeof(reply) );

    return true;
}
bool CLR_DBG_Debugger::Debugging_MFUpdate_Install(WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_MFUpdate_Install*       cmd = (CLR_DBG_Commands::Debugging_MFUpdate_Install*)msg->m_payload;
    CLR_DBG_Commands::Debugging_MFUpdate_Install::Reply reply;

    reply.m_success = MFUpdate_Validate(cmd->m_updateHandle, &cmd->m_updateValidation[0], cmd->m_updateValidationSize);

    // reply success before install
    dbg->m_messaging->ReplyToCommand( msg, true, false, &reply, sizeof(reply) );

    if(reply.m_success)
    {
        Events_WaitForEvents(0, 200);

        reply.m_success = MFUpdate_Install(cmd->m_updateHandle, &cmd->m_updateValidation[0], cmd->m_updateValidationSize);
    }

    return (reply.m_success == TRUE);
}

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

static bool FillValues( CLR_RT_HeapBlock* ptr, CLR_DBG_Commands::Debugging_Value*& array, size_t num, CLR_RT_HeapBlock* reference, CLR_RT_TypeDef_Instance* pTD )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    if(!ptr) return true;

    if(!array || num == 0) return false;

    CLR_DBG_Commands::Debugging_Value* dst = array++; num--;
    CLR_RT_TypeDescriptor              desc;

    memset( dst, 0, sizeof(*dst) );

    dst->m_referenceID = (reference != NULL) ? reference : ptr;
    dst->m_dt          = ptr->DataType ();
    dst->m_flags       = ptr->DataFlags();
    dst->m_size        = ptr->DataSize ();

    if(pTD != NULL)
    {
        dst->m_td = *pTD;
    }
    else if(SUCCEEDED(desc.InitializeFromObject( *ptr )))
    {
        dst->m_td = desc.m_handlerCls;
    }

    switch(dst->m_dt)
    {
    case DATATYPE_BOOLEAN   :
    case DATATYPE_I1        :
    case DATATYPE_U1        :

    case DATATYPE_CHAR      :
    case DATATYPE_I2        :
    case DATATYPE_U2        :

    case DATATYPE_I4        :
    case DATATYPE_U4        :
    case DATATYPE_R4        :

    case DATATYPE_I8        :
    case DATATYPE_U8        :
    case DATATYPE_R8        :
    case DATATYPE_DATETIME  :
    case DATATYPE_TIMESPAN  :
    case DATATYPE_REFLECTION:
        //
        // Primitives or optimized value types.
        //
        
#if !defined(NETMF_TARGET_BIG_ENDIAN)
        memcpy( dst->m_builtinValue, (void*)&ptr->NumericByRefConst().u1, 8 );
#else
        {
            UINT32 i;
            UINT8 *p = (UINT8*)&ptr->NumericByRefConst().u1;

            for (i=0;i<8;i++)
            {
                dst->m_builtinValue[i] = p[-i];
            }
        }
#endif
        break;

    case DATATYPE_STRING:
        {
            LPCSTR text = ptr->StringText();

            if(text != NULL)
            {
                dst->m_charsInString =                     text;
                dst->m_bytesInString = (CLR_UINT32)hal_strlen_s( text );

                hal_strncpy_s( (char*)dst->m_builtinValue, ARRAYSIZE(dst->m_builtinValue), text, __min(dst->m_bytesInString, MAXSTRLEN(dst->m_builtinValue)) );
            }
            else
            {
                dst->m_charsInString = NULL;
                dst->m_bytesInString = 0;
                dst->m_builtinValue[0] = 0;
            }
        }
        break;


    case DATATYPE_OBJECT:
    case DATATYPE_BYREF :
        return FillValues( ptr->Dereference(), array, num, NULL, pTD );


    case DATATYPE_CLASS    :
    case DATATYPE_VALUETYPE:
        dst->m_td = ptr->ObjectCls();
        break;

    case DATATYPE_SZARRAY:
        {
            CLR_RT_HeapBlock_Array* ptr2 = (CLR_RT_HeapBlock_Array*)ptr;

            dst->m_array_numOfElements = ptr2->m_numOfElements;
            dst->m_array_depth         = ptr2->ReflectionDataConst().m_levels;
            dst->m_array_typeIndex     = ptr2->ReflectionDataConst().m_data.m_type;
        }
        break;

    ////////////////////////////////////////

    case DATATYPE_WEAKCLASS                  :
        break;

    case DATATYPE_ARRAY_BYREF                :
        dst->m_arrayref_referenceID = ptr->Array     ();
        dst->m_arrayref_index       = ptr->ArrayIndex();

        break;

    case DATATYPE_DELEGATE_HEAD              :
        break;

    case DATATYPE_DELEGATELIST_HEAD          :
        break;

    case DATATYPE_FREEBLOCK                  :
    case DATATYPE_CACHEDBLOCK                :
    case DATATYPE_ASSEMBLY                   :
    case DATATYPE_OBJECT_TO_EVENT            :
    case DATATYPE_BINARY_BLOB_HEAD           :

    case DATATYPE_THREAD                     :
    case DATATYPE_SUBTHREAD                  :
    case DATATYPE_STACK_FRAME                :
    case DATATYPE_TIMER_HEAD                 :
    case DATATYPE_LOCK_HEAD                  :
    case DATATYPE_LOCK_OWNER_HEAD            :
    case DATATYPE_LOCK_REQUEST_HEAD          :
    case DATATYPE_WAIT_FOR_OBJECT_HEAD       :
    case DATATYPE_FINALIZER_HEAD             :
    case DATATYPE_MEMORY_STREAM_HEAD         :
    case DATATYPE_MEMORY_STREAM_DATA         :

    case DATATYPE_SERIALIZER_HEAD            :
    case DATATYPE_SERIALIZER_DUPLICATE       :
    case DATATYPE_SERIALIZER_STATE           :

    case DATATYPE_ENDPOINT_HEAD              :

#if defined(TINYCLR_APPDOMAINS)
    case DATATYPE_APPDOMAIN_HEAD             :
    case DATATYPE_TRANSPARENT_PROXY          :
    case DATATYPE_APPDOMAIN_ASSEMBLY         :
#endif

        break;
    }

    return true;
}

bool CLR_DBG_Debugger::GetValue( WP_Message* msg, CLR_RT_HeapBlock* ptr, CLR_RT_HeapBlock* reference, CLR_RT_TypeDef_Instance *pTD )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Commands::Debugging_Value  reply[ 4 ];
    CLR_DBG_Commands::Debugging_Value* array = reply;

    if(FillValues( ptr, array, ARRAYSIZE(reply), reference, pTD ))
    {
        m_messaging->ReplyToCommand( msg, true, false, reply, (int)((size_t)array - (size_t)reply) );

        return true;
    }

    m_messaging->ReplyToCommand( msg, false, false );

    return false;
}

//--//

bool CLR_DBG_Debugger::Debugging_Execution_SetCurrentAppDomain( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
#if defined(TINYCLR_APPDOMAINS)
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Execution_SetCurrentAppDomain* cmd       = (CLR_DBG_Commands::Debugging_Execution_SetCurrentAppDomain*)msg->m_payload;
    CLR_RT_AppDomain*                                          appDomain = dbg->GetAppDomainFromID( cmd->m_id );

    if(appDomain)
    {
        g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomain );
    }

    dbg->m_messaging->ReplyToCommand( msg, appDomain != NULL, false );

    return true;
#else
    return false;
#endif
}

bool CLR_DBG_Debugger::Debugging_Execution_Breakpoints( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Execution_Breakpoints* cmd = (CLR_DBG_Commands::Debugging_Execution_Breakpoints*)msg->m_payload;

    g_CLR_RT_ExecutionEngine.InstallBreakpoints( cmd->m_data, (msg->m_header.m_size - sizeof(cmd->m_flags)) / sizeof(CLR_DBG_Commands::Debugging_Execution_BreakpointDef) );

    dbg->m_messaging->ReplyToCommand( msg, true, false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Execution_BreakpointStatus( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Execution_BreakpointStatus::Reply reply;

    if(g_CLR_RT_ExecutionEngine.DequeueActiveBreakpoint( reply.m_lastHit ) == false)
    {
        memset( &reply.m_lastHit, 0, sizeof(reply.m_lastHit) );
    }

    dbg->m_messaging->ReplyToCommand( msg, true, false, &reply, sizeof(reply) );

    return true;
}

//--//

CLR_RT_Assembly* CLR_DBG_Debugger::IsGoodAssembly( CLR_IDX idxAssm )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
    {
        if(pASSM->m_idx == idxAssm) return pASSM;
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    return NULL;
}

bool CLR_DBG_Debugger::CheckTypeDef( const CLR_RT_TypeDef_Index& td, CLR_RT_TypeDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_RT_Assembly* assm = IsGoodAssembly( td.Assembly() );

    if(assm && td.Type() < assm->m_pTablesSize[ TBL_TypeDef ])
    {
        return inst.InitializeFromIndex( td );
    }

    return false;
}

bool CLR_DBG_Debugger::CheckFieldDef( const CLR_RT_FieldDef_Index& fd, CLR_RT_FieldDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_RT_Assembly* assm = IsGoodAssembly( fd.Assembly() );

    if(assm && fd.Field() < assm->m_pTablesSize[ TBL_FieldDef ])
    {
        return inst.InitializeFromIndex( fd );
    }

    return false;
}

bool CLR_DBG_Debugger::CheckMethodDef( const CLR_RT_MethodDef_Index& md, CLR_RT_MethodDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_RT_Assembly* assm = IsGoodAssembly( md.Assembly() );

    if(assm && md.Method() < assm->m_pTablesSize[ TBL_MethodDef ])
    {
        return inst.InitializeFromIndex( md );
    }

    return false;
}

CLR_RT_StackFrame* CLR_DBG_Debugger::CheckStackFrame( CLR_UINT32 pid, CLR_UINT32 depth, bool& isInline )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_RT_Thread* th = GetThreadFromPid( pid );

    isInline = false;

    if(th)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_StackFrame,call,th->m_stackFrames)
        {
#ifndef TINYCLR_NO_IL_INLINE
            if(call->m_inlineFrame)
            {
                if(depth-- == 0) 
                {
                    isInline = true;
                    return call;
                }
            }
#endif

            if(depth-- == 0) return call;
        }
        TINYCLR_FOREACH_NODE_END();
    }

    return NULL;
}

//--//

static HRESULT Debugging_Thread_Create_Helper( CLR_RT_MethodDef_Index& md, CLR_RT_Thread*& th, CLR_UINT32 pid )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock     ref; ref.SetObjectReference( NULL );
    CLR_RT_ProtectFromGC gc( ref );
    CLR_RT_Thread*       realThread = (pid != 0) ? CLR_DBG_Debugger::GetThreadFromPid( pid ) : NULL;

    th = NULL;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Delegate::CreateInstance( ref, md, NULL ));

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewThread( th, ref.DereferenceDelegate(), ThreadPriority::Highest, -1 ));
    

    if (realThread)
    {
        th->m_realThread = realThread;
    }

    {
        CLR_RT_StackFrame*          stack   = th->CurrentFrame();
        const CLR_RECORD_METHODDEF* target  = stack->m_call.m_target;
        CLR_UINT8                   numArgs = target->numArgs;

        if(numArgs)
        {
            CLR_RT_SignatureParser          parser; parser.Initialize_MethodSignature( stack->m_call.m_assm, target );
            CLR_RT_SignatureParser::Element res;
            CLR_RT_HeapBlock*               args = stack->m_arguments;

            if(parser.m_flags & PIMAGE_CEE_CS_CALLCONV_HASTHIS)
            {
                args->SetObjectReference( NULL );

                numArgs--;
                args++;
            }

            //
            // Skip return value.
            //
            TINYCLR_CHECK_HRESULT(parser.Advance( res ));

            //
            // None of the arguments can be ByRef.
            //
            {
                CLR_RT_SignatureParser parser2 = parser;

                for(;parser2.Available() > 0;)
                {
                    TINYCLR_CHECK_HRESULT(parser2.Advance( res ));

                    if(res.m_fByRef)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                    }
                }
            }

            for(CLR_UINT8 i=0; i<numArgs; i++, args++)
            {
                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.InitializeReference( *args, parser ));
            }
        }
    }

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(th)
        {
            th->Terminate();
            th = NULL;
        }
    }

    TINYCLR_CLEANUP_END();
}

bool CLR_DBG_Debugger::Debugging_Thread_CreateEx( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_CreateEx*       cmd = (CLR_DBG_Commands::Debugging_Thread_CreateEx*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Thread_CreateEx::Reply cmdReply;
    CLR_RT_Thread*                                     th;

    if(SUCCEEDED(Debugging_Thread_Create_Helper( cmd->m_md, th, cmd->m_pid )))
    {
        th->m_scratchPad = cmd->m_scratchPad;

        cmdReply.m_pid = th->m_pid;
    }
    else
    {
        cmdReply.m_pid = (CLR_UINT32)-1;
    }

    dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Thread_List( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_List::Reply* cmdReply = NULL;
    int                                             len      = 0;

    if(FAILED(dbg->CreateListOfThreads( cmdReply, len )))
    {
        dbg->m_messaging->ReplyToCommand( msg, false, false );
    }
    else
    {
        dbg->m_messaging->ReplyToCommand( msg, true, false, cmdReply, len );
    }

    CLR_RT_Memory::Release( cmdReply );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Thread_Stack( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_Stack*        cmd      = (CLR_DBG_Commands::Debugging_Thread_Stack*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Thread_Stack::Reply* cmdReply = NULL;
    int                                              len      = 0;

    if(FAILED(dbg->CreateListOfCalls( cmd->m_pid, cmdReply, len )))
    {
        dbg->m_messaging->ReplyToCommand( msg, false, false );
    }
    else
    {
        dbg->m_messaging->ReplyToCommand( msg, true, false, cmdReply, len );
    }

    CLR_RT_Memory::Release( cmdReply );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Thread_Kill( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_Kill*       cmd = (CLR_DBG_Commands::Debugging_Thread_Kill*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Thread_Kill::Reply cmdReply;
    CLR_RT_Thread*                                 th  = dbg->GetThreadFromPid( cmd->m_pid );

    if(th)
    {
        th->Terminate();

        cmdReply.m_result = 1;
    }
    else
    {
        cmdReply.m_result = 0;
    }

    dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Thread_Suspend( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_Suspend* cmd = (CLR_DBG_Commands::Debugging_Thread_Suspend*)msg->m_payload;
    CLR_RT_Thread*                              th  = dbg->GetThreadFromPid( cmd->m_pid );

    if(th)
    {
        th->Suspend();
    }

    dbg->m_messaging->ReplyToCommand( msg, th != NULL, false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Thread_Resume( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_Resume* cmd = (CLR_DBG_Commands::Debugging_Thread_Resume*)msg->m_payload;
    CLR_RT_Thread*                             th  = dbg->GetThreadFromPid( cmd->m_pid );

    if(th)
    {
        th->Resume();
    }

    dbg->m_messaging->ReplyToCommand( msg, th != NULL, false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Thread_Get( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger*                       dbg  = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_Get* cmd  = (CLR_DBG_Commands::Debugging_Thread_Get*)msg->m_payload;
    CLR_RT_Thread*                          th   = dbg->GetThreadFromPid( cmd->m_pid );
    CLR_RT_HeapBlock*                       pThread;
    bool fFound = false;

    if(th == NULL) return false;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    //If we are a thread spawned by the debugger to perform evaluations,
    //return the thread object that correspond to thread that has focus in debugger.
    th = th->m_realThread;
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    //Find an existing managed thread, if it exists
    //making sure to only return the managed object association with the current appdomain
    //to prevent leaking of managed Thread objects across AD boundaries.

    TINYCLR_FOREACH_NODE(CLR_RT_ObjectToEvent_Source,src,th->m_references)
    {
        CLR_RT_HeapBlock* pManagedThread = src->m_objectPtr;
        _ASSERTE(pManagedThread != NULL);
        
#if defined(TINYCLR_APPDOMAINS)
        {
            CLR_RT_ObjectToEvent_Source* appDomainSrc = CLR_RT_ObjectToEvent_Source::ExtractInstance( pManagedThread[ Library_corlib_native_System_Threading_Thread::FIELD__m_AppDomain ] );

            if(appDomainSrc == NULL) break;
            
            fFound = (appDomainSrc->m_eventPtr == g_CLR_RT_ExecutionEngine.GetCurrentAppDomain());
        }
#else
        fFound = true;
#endif

        if(fFound)
        {
            pThread = pManagedThread;
            
            break;
        }
    }
    TINYCLR_FOREACH_NODE_END();    

    if(!fFound)
    {
        pThread = (CLR_RT_HeapBlock*)private_malloc(sizeof(CLR_RT_HeapBlock));
        
        //Create the managed thread.
        //This implies that there is no state in the managed object.  This is not exactly true, as the managed thread 
        //contains the priority as well as the delegate to start.  However, that state is really just used as a placeholder for
        //the data before the thread is started.  Once the thread is started, they are copied over to the unmanaged thread object
        //and no longer used.  The managed object is then used simply as a wrapper for the unmanaged thread.  Therefore, it is safe 
        //to simply make another managed thread here.
        if(SUCCEEDED(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *pThread, g_CLR_RT_WellKnownTypes.m_Thread )))
        {
            CLR_RT_HeapBlock* pRes = pThread->Dereference();

            int pri = th->GetThreadPriority();
            
            pRes[Library_corlib_native_System_Threading_Thread::FIELD__m_Priority].NumericByRef().s4 = pri;

            if(SUCCEEDED(CLR_RT_ObjectToEvent_Source::CreateInstance( th, *pRes, pRes[ Library_corlib_native_System_Threading_Thread::FIELD__m_Thread ] )))
            {
#if defined(TINYCLR_APPDOMAINS)
                CLR_RT_ObjectToEvent_Source::CreateInstance( g_CLR_RT_ExecutionEngine.GetCurrentAppDomain(), *pRes, pRes[ Library_corlib_native_System_Threading_Thread::FIELD__m_AppDomain ] );
#endif
                fFound = true;
            }
            
        }
    }

    if(!fFound) return false;
    
    return dbg->GetValue( msg, pThread, NULL, NULL );
}

bool CLR_DBG_Debugger::Debugging_Thread_GetException( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_GetException* cmd = (CLR_DBG_Commands::Debugging_Thread_GetException*)msg->m_payload;
    CLR_RT_Thread*                                   th  = dbg->GetThreadFromPid( cmd->m_pid );
    CLR_RT_HeapBlock*                                blk = NULL;

    if(th)
    {
        blk = &th->m_currentException;
    }

    return dbg->GetValue( msg, blk, NULL, NULL );
}

bool CLR_DBG_Debugger::Debugging_Thread_Unwind( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Thread_Unwind* cmd = (CLR_DBG_Commands::Debugging_Thread_Unwind*)msg->m_payload;
    CLR_RT_StackFrame*                         call;
    CLR_RT_Thread*                             th;
    bool                                       isInline = false;

    if((call = dbg->CheckStackFrame( cmd->m_pid, cmd->m_depth, isInline )) != NULL)
    {
        _ASSERTE((call->m_flags & CLR_RT_StackFrame::c_MethodKind_Native) == 0);

        th = call->m_owningThread;
        _ASSERTE(th->m_nestedExceptionsPos);

        CLR_RT_Thread::UnwindStack& us = th->m_nestedExceptions[ th->m_nestedExceptionsPos - 1 ];
        _ASSERTE(th->m_currentException.Dereference() == us.m_exception);
        _ASSERTE(us.m_flags & CLR_RT_Thread::UnwindStack::c_ContinueExceptionHandler);

        us.m_handlerStack  = call;
        us.m_flags        |= CLR_RT_Thread::UnwindStack::c_MagicCatchForInteceptedException;

        us.SetPhase(CLR_RT_Thread::UnwindStack::p_2_RunningFinallys_0);
    }

    return true;
}

//--//

bool CLR_DBG_Debugger::Debugging_Stack_Info( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Stack_Info*       cmd = (CLR_DBG_Commands::Debugging_Stack_Info*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Stack_Info::Reply cmdReply;
    CLR_RT_StackFrame*                            call;
    bool                                          isInline = false;

    if((call = dbg->CheckStackFrame( cmd->m_pid, cmd->m_depth, isInline )) != NULL)
    {
#ifndef TINYCLR_NO_IL_INLINE
        if(isInline)
        {
            cmdReply.m_md               =              call->m_inlineFrame->m_frame.m_call;
            cmdReply.m_IP               = (CLR_UINT32)(call->m_inlineFrame->m_frame.m_IP - call->m_inlineFrame->m_frame.m_IPStart);
            cmdReply.m_numOfArguments   =              call->m_inlineFrame->m_frame.m_call.m_target->numArgs;
            cmdReply.m_numOfLocals      =              call->m_inlineFrame->m_frame.m_call.m_target->numLocals;
            cmdReply.m_depthOfEvalStack = (CLR_UINT32)(call->m_evalStack - call->m_inlineFrame->m_frame.m_evalStack);
        }
        else
#endif
        {
            cmdReply.m_md               =              call->m_call;
            cmdReply.m_IP               = (CLR_UINT32)(call->m_IP - call->m_IPstart);
            cmdReply.m_numOfArguments   =              call->m_call.m_target->numArgs;
            cmdReply.m_numOfLocals      =              call->m_call.m_target->numLocals;
            cmdReply.m_depthOfEvalStack = (CLR_UINT32) call->TopValuePosition();
        }

        dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

        return true;
    }

    dbg->m_messaging->ReplyToCommand( msg, false, false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Stack_SetIP( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Stack_SetIP* cmd = (CLR_DBG_Commands::Debugging_Stack_SetIP*)msg->m_payload;
    CLR_RT_StackFrame*                       call;
    bool                                     isInline = false;

    if((call = dbg->CheckStackFrame( cmd->m_pid, cmd->m_depth, isInline )) != NULL)
    {
#ifndef TINYCLR_NO_IL_INLINE
        if(isInline)
        {
            dbg->m_messaging->ReplyToCommand( msg, false, false );

            return true;
        }
        else
#endif            
        {
            call->m_IP            = call->m_IPstart   + cmd->m_IP;
            call->m_evalStackPos  = call->m_evalStack + cmd->m_depthOfEvalStack;
        }
        
        call->m_flags &= ~CLR_RT_StackFrame::c_InvalidIP;

        dbg->m_messaging->ReplyToCommand( msg, true, false );

        return true;
    }

    dbg->m_messaging->ReplyToCommand( msg, false, false );

    return true;
}

//--//

static bool IsBlockEnumMaybe( CLR_RT_HeapBlock* blk )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    const CLR_UINT32 c_MaskForPrimitive = CLR_RT_DataTypeLookup::c_Integer | CLR_RT_DataTypeLookup::c_Numeric;

    CLR_RT_TypeDescriptor desc;

    if(FAILED(desc.InitializeFromObject( *blk ))) return false;

    const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ desc.m_handlerCls.m_target->dataType ];

    return (dtl.m_flags & c_MaskForPrimitive) == c_MaskForPrimitive;
}

static bool SetBlockHelper( CLR_RT_HeapBlock* blk, CLR_DataType dt, CLR_UINT8* builtinValue )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    bool fCanAssign = false;

    if(blk)
    {
        CLR_DataType                 dtDst;
        CLR_RT_HeapBlock             src;

        dtDst = blk->DataType();

        src.SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(dt, 0, 1) );
        memcpy( (void*)&src.NumericByRef().u1, builtinValue, sizeof(CLR_UINT64) );

        if(dtDst == dt)
        {
            fCanAssign = true;
        }
        else
        {
            if(dt == DATATYPE_REFLECTION)
            {
                fCanAssign = (dtDst == DATATYPE_OBJECT && blk->Dereference() == NULL);
            }
            else if(dt == DATATYPE_OBJECT)
            {
                fCanAssign = (src .Dereference() == NULL && dtDst == DATATYPE_REFLECTION);
            }
            else
            {
                _ASSERTE(c_CLR_RT_DataTypeLookup[ dtDst ].m_flags & CLR_RT_DataTypeLookup::c_Numeric);

                if(c_CLR_RT_DataTypeLookup[ dtDst ].m_sizeInBytes == sizeof(CLR_INT32) &&
                   c_CLR_RT_DataTypeLookup[ dt    ].m_sizeInBytes <  sizeof(CLR_INT32))
                {
                    dt = dtDst;
                    fCanAssign = true;
                }
            }
        }

        if(fCanAssign)
        {
            blk->ChangeDataType( dt );
            memcpy( (void*)&blk->NumericByRef().u1, builtinValue, sizeof(CLR_UINT64) );
        }
    }

    return fCanAssign;
}

static CLR_RT_HeapBlock* GetScratchPad_Helper( int idx )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_RT_HeapBlock_Array* array = g_CLR_RT_ExecutionEngine.m_scratchPadArray;
    CLR_RT_HeapBlock        tmp;
    CLR_RT_HeapBlock        ref;

    tmp.SetObjectReference( array );

    if(SUCCEEDED(ref.InitializeArrayReference( tmp, idx )))
    {
        return (CLR_RT_HeapBlock*)array->GetElement( idx );
    }

    return NULL;
}

//--//

bool CLR_DBG_Debugger::Debugging_Value_ResizeScratchPad( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_ResizeScratchPad* cmd = (CLR_DBG_Commands::Debugging_Value_ResizeScratchPad*)msg->m_payload;
    CLR_RT_HeapBlock                                    ref;
    bool                                                fRes = true;

    if(cmd->m_size == 0)
    {
        g_CLR_RT_ExecutionEngine.m_scratchPadArray = NULL;
    }
    else
    {
        if(SUCCEEDED(CLR_RT_HeapBlock_Array::CreateInstance( ref, cmd->m_size, g_CLR_RT_WellKnownTypes.m_Object )))
        {
            CLR_RT_HeapBlock_Array* pOld = g_CLR_RT_ExecutionEngine.m_scratchPadArray;
            CLR_RT_HeapBlock_Array* pNew = ref.DereferenceArray();

            if(pOld)
            {
                memcpy( pNew->GetFirstElement(), pOld->GetFirstElement(), sizeof(CLR_RT_HeapBlock) * __min( pNew->m_numOfElements, pOld->m_numOfElements ) );
            }

            g_CLR_RT_ExecutionEngine.m_scratchPadArray = pNew;
        }
        else
        {
            fRes = false;
        }
    }

    dbg->m_messaging->ReplyToCommand( msg, fRes, false );

    return false;
}

bool CLR_DBG_Debugger::Debugging_Value_GetStack( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_GetStack* cmd = (CLR_DBG_Commands::Debugging_Value_GetStack*)msg->m_payload;
    CLR_RT_StackFrame*                          call;
    bool                                        isInline = false;

    if((call = dbg->CheckStackFrame( cmd->m_pid, cmd->m_depth, isInline )) != NULL)
    {
        CLR_RT_HeapBlock* array;
        CLR_UINT32        num;
#ifndef TINYCLR_NO_IL_INLINE
        CLR_RT_MethodDef_Instance& md = isInline ? call->m_inlineFrame->m_frame.m_call : call->m_call;
#else
        CLR_RT_MethodDef_Instance& md = call->m_call;
#endif

        switch(cmd->m_kind)
        {
        case CLR_DBG_Commands::Debugging_Value_GetStack::c_Argument:
#ifndef TINYCLR_NO_IL_INLINE
            array = isInline ? call->m_inlineFrame->m_frame.m_args : call->m_arguments;
            num   = isInline ? md.m_target->numArgs                : md.m_target->numArgs;
#else
            array = call->m_arguments;
            num   = call->m_call.m_target->numArgs;
#endif
            break;

        case CLR_DBG_Commands::Debugging_Value_GetStack::c_Local:
#ifndef TINYCLR_NO_IL_INLINE
            array = isInline ? call->m_inlineFrame->m_frame.m_locals : call->m_locals;
            num   = isInline ? md.m_target->numLocals                : md.m_target->numLocals;
#else
            array = call->m_locals;
            num   = call->m_call.m_target->numLocals;
#endif
            break;

        case CLR_DBG_Commands::Debugging_Value_GetStack::c_EvalStack:
#ifndef TINYCLR_NO_IL_INLINE
            array = isInline ? call->m_inlineFrame->m_frame.m_evalStack                                   : call->m_evalStack;
            num   = isInline ? (CLR_UINT32)(call->m_evalStack - call->m_inlineFrame->m_frame.m_evalStack) : (CLR_UINT32)call->TopValuePosition();
#else
            array =             call->m_evalStack;
            num   = (CLR_UINT32)call->TopValuePosition();
#endif
            break;

        default:
            return false;
        }

        if(cmd->m_index >= num) return false;

        CLR_RT_HeapBlock*        blk       = &array[ cmd->m_index ];
        CLR_RT_HeapBlock*        reference = NULL;
        CLR_RT_HeapBlock         tmp;
        CLR_RT_TypeDef_Instance* pTD       = NULL;
        CLR_RT_TypeDef_Instance  td;

        if(cmd->m_kind != CLR_DBG_Commands::Debugging_Value_GetStack::c_EvalStack && IsBlockEnumMaybe( blk ))
        {
            CLR_UINT32                      iElement = cmd->m_index;
            CLR_RT_SignatureParser          parser;
            CLR_RT_SignatureParser::Element res;
            CLR_RT_TypeDescriptor           desc;

            if(cmd->m_kind == CLR_DBG_Commands::Debugging_Value_GetStack::c_Argument)
            {
                parser.Initialize_MethodSignature( md.m_assm, md.m_target );

                iElement++; // Skip the return value, always at the head of the signature.

                if(parser.m_flags & PIMAGE_CEE_CS_CALLCONV_HASTHIS)
                {
                    if(iElement == 0) return false; // The requested argument is the "this" argument, it can never be a primitive.

                    iElement--;
                }
            }
            else
            {
                parser.Initialize_MethodLocals( md.m_assm, md.m_target );
            }

            do
            {
                parser.Advance( res );
            }
            while(iElement--);

            //
            // Arguments to a methods come from the eval stack and we don't fix up the eval stack for each call.
            // So some arguments have the wrong datatype, since an eval stack push always promotes to 32 bits.
            //
            if(c_CLR_RT_DataTypeLookup[ blk->DataType() ].m_sizeInBytes == sizeof(CLR_INT32) &&
               c_CLR_RT_DataTypeLookup[ res.m_dt        ].m_sizeInBytes <  sizeof(CLR_INT32)  )
            {
                tmp.Assign        ( *blk      );
                tmp.ChangeDataType(  res.m_dt );

                reference = blk; blk = &tmp;
            }

            //
            // Check for enum.
            //
            desc.InitializeFromType( res.m_cls );

            if(desc.m_handlerCls.m_target->IsEnum())
            {
                td = desc.m_handlerCls; pTD = &td;
            }
        }

        return dbg->GetValue( msg, blk, reference, pTD );
    }

    return false;
}

bool CLR_DBG_Debugger::Debugging_Value_GetField( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_GetField* cmd       = (CLR_DBG_Commands::Debugging_Value_GetField*)msg->m_payload;
    CLR_RT_HeapBlock*                           blk       = cmd->m_heapblock;
    CLR_RT_HeapBlock*                           reference = NULL;
    CLR_RT_HeapBlock                            tmp;
    CLR_RT_TypeDescriptor                       desc;
    CLR_RT_TypeDef_Instance                     td;
    CLR_RT_TypeDef_Instance*                    pTD       = NULL;
    CLR_RT_FieldDef_Instance                    inst;
    CLR_UINT32                                  offset;

    if(blk != NULL && cmd->m_offset > 0)
    {
        if(FAILED(desc.InitializeFromObject( *blk ))) return false;

        td     = desc.m_handlerCls;
        offset = cmd->m_offset - 1;

        while(true)
        {
            CLR_UINT32 iFields     = td.m_target->iFields_Num;
            CLR_UINT32 totalFields = td.CrossReference().m_totalFields;
            CLR_UINT32 dFields     = totalFields - iFields;

            if(offset >= dFields)
            {
                offset -= dFields;
                break;
            }

            if(!td.SwitchToParent()) return false;
        }

        cmd->m_fd.Set( td.Assembly(), td.m_target->iFields_First + offset );
    }

    if(!dbg->CheckFieldDef( cmd->m_fd, inst )) return false;

    if(blk == NULL)
    {
        blk = CLR_RT_ExecutionEngine::AccessStaticField( cmd->m_fd );
    }
    else
    {
        if(cmd->m_offset == 0)
        {
            cmd->m_offset = inst.CrossReference().m_offset;
        }

        if(cmd->m_offset == 0) return false;

        switch(blk->DataType())
        {
            case DATATYPE_CLASS    :
            case DATATYPE_VALUETYPE:
                break;

            default:
                if(FAILED(blk->EnsureObjectReference( blk ))) return false;
                break;
        }

        switch(blk->DataType())
        {
            case DATATYPE_DATETIME: // Special case.
            case DATATYPE_TIMESPAN: // Special case.
                tmp.SetInteger( (CLR_INT64)blk->NumericByRefConst().s8 );
                reference = blk; blk = &tmp;
                break;

            default:
                blk = &blk[ cmd->m_offset ];
                break;
        }
    }

    if(IsBlockEnumMaybe( blk ))
    {
        if(SUCCEEDED(desc.InitializeFromFieldDefinition( inst )))
        {
            if(desc.m_handlerCls.m_target->IsEnum())
            {
                pTD = &desc.m_handlerCls;
            }
        }
    }

    return dbg->GetValue( msg, blk, reference, pTD );
}

bool CLR_DBG_Debugger::Debugging_Value_GetArray( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_GetArray* cmd       = (CLR_DBG_Commands::Debugging_Value_GetArray*)msg->m_payload;
    CLR_RT_HeapBlock*                           blk       = NULL;
    CLR_RT_HeapBlock*                           reference = NULL;
    CLR_RT_HeapBlock                            tmp;
    CLR_RT_HeapBlock                            ref;
    CLR_RT_TypeDef_Instance*                    pTD       = NULL;
    CLR_RT_TypeDef_Instance                     td;

    tmp.SetObjectReference( cmd->m_heapblock );

    if(SUCCEEDED(ref.InitializeArrayReference( tmp, cmd->m_index )))
    {
        CLR_RT_HeapBlock_Array* array = ref.Array();

        if(array->m_fReference)
        {
            blk = (CLR_RT_HeapBlock*)array->GetElement( cmd->m_index );
        }
        else
        {
            if(FAILED(tmp.LoadFromReference( ref ))) return false;

            blk = &tmp; reference = (CLR_RT_HeapBlock*)-1;
        }

        if(IsBlockEnumMaybe( blk ))
        {
            if(td.InitializeFromIndex( array->ReflectionDataConst().m_data.m_type ))
            {
                if(td.m_target->IsEnum())
                {
                    pTD = &td;
                }
            }
        }
    }

    return dbg->GetValue( msg, blk, reference, pTD );
}

bool CLR_DBG_Debugger::Debugging_Value_GetBlock( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_GetBlock* cmd = (CLR_DBG_Commands::Debugging_Value_GetBlock*)msg->m_payload;
    CLR_RT_HeapBlock*                           blk = cmd->m_heapblock;

    return dbg->GetValue( msg, blk, NULL, NULL );
}

bool CLR_DBG_Debugger::Debugging_Value_GetScratchPad( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_GetScratchPad* cmd = (CLR_DBG_Commands::Debugging_Value_GetScratchPad*)msg->m_payload;
    CLR_RT_HeapBlock*                                blk = GetScratchPad_Helper( cmd->m_idx );

    return dbg->GetValue( msg, blk, NULL, NULL );
}

bool CLR_DBG_Debugger::Debugging_Value_SetBlock( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_SetBlock* cmd = (CLR_DBG_Commands::Debugging_Value_SetBlock*)msg->m_payload;
    CLR_RT_HeapBlock*                           blk = cmd->m_heapblock;

    dbg->m_messaging->ReplyToCommand( msg, SetBlockHelper( blk, (CLR_DataType)cmd->m_dt, cmd->m_builtinValue ), false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Value_SetArray( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_SetArray* cmd      = (CLR_DBG_Commands::Debugging_Value_SetArray*)msg->m_payload;
    CLR_RT_HeapBlock_Array*                     array    = cmd->m_heapblock;
    CLR_RT_HeapBlock                            tmp;
    bool                                        fSuccess = false;

    tmp.SetObjectReference( cmd->m_heapblock );

    //
    // We can only set values in arrays of primitive types.
    //
    if(array != NULL && !array->m_fReference)
    {
        CLR_RT_HeapBlock ref;

        if(SUCCEEDED(ref.InitializeArrayReference( tmp, cmd->m_index )))
        {
            if(SUCCEEDED(tmp.LoadFromReference( ref )))
            {
                if(SetBlockHelper( &tmp, tmp.DataType(), cmd->m_builtinValue ))
                {
                    if(SUCCEEDED(tmp.StoreToReference( ref, 0 )))
                    {
                        fSuccess = true;
                    }
                }
            }
        }
    }

    dbg->m_messaging->ReplyToCommand( msg, fSuccess, false );

    return true;
}

//--//

bool CLR_DBG_Debugger::Debugging_Value_AllocateObject( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_AllocateObject* cmd = (CLR_DBG_Commands::Debugging_Value_AllocateObject*)msg->m_payload;
    CLR_RT_HeapBlock*                                 blk = NULL;
    CLR_RT_HeapBlock*                                 ptr = GetScratchPad_Helper( cmd->m_index );

    if(ptr)
    {
        if(SUCCEEDED(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *ptr, cmd->m_td )))
        {
            blk = ptr;
        }
    }

    return dbg->GetValue( msg, blk, NULL, NULL );
}

bool CLR_DBG_Debugger::Debugging_Value_AllocateString( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_AllocateString* cmd = (CLR_DBG_Commands::Debugging_Value_AllocateString*)msg->m_payload;
    CLR_RT_HeapBlock*                                 blk = NULL;
    CLR_RT_HeapBlock*                                 ptr = GetScratchPad_Helper( cmd->m_index );

    if(ptr)
    {
        CLR_RT_HeapBlock_String* str = CLR_RT_HeapBlock_String::CreateInstance( *ptr, cmd->m_size );

        if(str)
        {
            LPSTR dst = (LPSTR)str->StringText();

            //
            // Fill the string with spaces, it will be set at a later stage.
            //
            memset( dst, ' ', cmd->m_size ); dst[ cmd->m_size ] = 0;

            blk = ptr;
        }
    }

    return dbg->GetValue( msg, blk, NULL, NULL );
}

bool CLR_DBG_Debugger::Debugging_Value_AllocateArray( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_AllocateArray* cmd = (CLR_DBG_Commands::Debugging_Value_AllocateArray*)msg->m_payload;
    CLR_RT_HeapBlock*                                blk = NULL;
    CLR_RT_HeapBlock*                                ptr = GetScratchPad_Helper( cmd->m_index );

    if(ptr)
    {
        CLR_RT_ReflectionDef_Index reflex;

        reflex.m_kind        = REFLECTION_TYPE;
        reflex.m_levels      = cmd->m_depth;
        reflex.m_data.m_type = cmd->m_td;

        if(SUCCEEDED(CLR_RT_HeapBlock_Array::CreateInstance( *ptr, cmd->m_numOfElements, reflex )))
        {
            blk = ptr;
        }
    }

    return dbg->GetValue( msg, blk, NULL, NULL );
}

#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if defined(TINYCLR_PROFILE_NEW)
bool CLR_DBG_Debugger::Profiling_Command( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger*                    dbg     = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Profiling_Command* cmd     = (CLR_DBG_Commands::Profiling_Command*)msg->m_payload;
    CLR_UINT8                            command = cmd->m_command;

    switch(command)
    {
        case CLR_DBG_Commands::Profiling_Command::c_Command_ChangeConditions:
            return dbg->Profiling_ChangeConditions( msg );

        case CLR_DBG_Commands::Profiling_Command::c_Command_FlushStream:
            return dbg->Profiling_FlushStream( msg );

        default:
            return false;
    }
}

bool CLR_DBG_Debugger::Profiling_ChangeConditions( WP_Message* msg )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Commands::Profiling_Command* parent_cmd = (CLR_DBG_Commands::Profiling_Command*)msg->m_payload;
    CLR_DBG_Commands::Profiling_ChangeConditions* cmd = (CLR_DBG_Commands::Profiling_ChangeConditions*)&parent_cmd[1];

    g_CLR_RT_ExecutionEngine.m_iProfiling_Conditions |=  cmd->m_set;
    g_CLR_RT_ExecutionEngine.m_iProfiling_Conditions &= ~cmd->m_reset;

    CLR_DBG_Commands::Profiling_Command::Reply cmdReply;

    cmdReply.m_raw = g_CLR_RT_ExecutionEngine.m_iProfiling_Conditions;

    m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

    _ASSERTE(FIMPLIES(CLR_EE_PRF_IS(Allocations),  CLR_EE_PRF_IS(Enabled)));
    _ASSERTE(FIMPLIES(CLR_EE_PRF_IS(Calls)      ,  CLR_EE_PRF_IS(Enabled)));

    if((cmd->m_set & CLR_RT_ExecutionEngine::c_fProfiling_Enabled) != 0)
    {
        g_CLR_PRF_Profiler.SendMemoryLayout();
    }

    return true;
}

bool CLR_DBG_Debugger::Profiling_FlushStream( WP_Message* msg )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Commands::Profiling_Command::Reply cmdReply;

    g_CLR_PRF_Profiler.Stream_Flush();

    cmdReply.m_raw = 0;

    m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

    return true;
}

#endif //#if defined(TINYCLR_PROFILE_NEW)

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

//--//

struct AnalyzeObject
{
    CLR_RT_HeapBlock*     m_ptr;
    bool                  m_fNull;
    bool                  m_fBoxed;
    bool                  m_fCanBeNull;
    CLR_RT_TypeDescriptor m_desc;
    CLR_RT_HeapBlock      m_value;
};

static HRESULT AnalyzeObject_Helper( CLR_RT_HeapBlock* ptr, AnalyzeObject& ao )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_HEADER();

    if(ptr && ptr->DataType() == DATATYPE_BYREF) ptr = ptr->Dereference();

    ao.m_ptr = ptr;

    if(ptr == NULL || (ptr->DataType() == DATATYPE_OBJECT && ptr->Dereference() == NULL))
    {
        ao.m_fNull      = true;
        ao.m_fBoxed     = false;
        ao.m_fCanBeNull = true;
    }
    else
    {
        TINYCLR_CHECK_HRESULT(ao.m_desc.InitializeFromObject( *ptr ));

        ao.m_fNull  = false;
        ao.m_fBoxed = (ptr->DataType() == DATATYPE_OBJECT && ptr->Dereference()->IsBoxed());

        switch(ao.m_desc.m_flags & CLR_RT_DataTypeLookup::c_SemanticMask2 & ~CLR_RT_DataTypeLookup::c_SemanticMask)
        {
        case CLR_RT_DataTypeLookup::c_Array:
        case CLR_RT_DataTypeLookup::c_ArrayList:
            ao.m_fCanBeNull = true;
            break;
        default:
            {
                switch(ao.m_desc.m_flags & CLR_RT_DataTypeLookup::c_SemanticMask)
                {
                case CLR_RT_DataTypeLookup::c_Primitive:
                case CLR_RT_DataTypeLookup::c_ValueType:
                case CLR_RT_DataTypeLookup::c_Enum:
                    ao.m_fCanBeNull = ao.m_fBoxed || (ao.m_desc.m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_String.m_data);
                    break;

                default:
                    ao.m_fCanBeNull = true;
                    break;
                }

            break;
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

static HRESULT Assign_Helper( CLR_RT_HeapBlock* blkDst, CLR_RT_HeapBlock* blkSrc )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_HEADER();

    AnalyzeObject        aoDst;
    AnalyzeObject        aoSrc;
    CLR_RT_HeapBlock     srcVal; srcVal.SetObjectReference( NULL );
    CLR_RT_ProtectFromGC gc( srcVal );

    TINYCLR_CHECK_HRESULT(AnalyzeObject_Helper( blkDst, aoDst ));
    TINYCLR_CHECK_HRESULT(AnalyzeObject_Helper( blkSrc, aoSrc ));

    if(aoSrc.m_fNull)
    {
        if(aoDst.m_fCanBeNull == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }

        TINYCLR_CHECK_HRESULT(srcVal.StoreToReference( *blkDst, 0 ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(srcVal.LoadFromReference( *blkSrc ));

        if(aoDst.m_fNull)
        {
            if(aoSrc.m_fCanBeNull == false)
            {
                TINYCLR_CHECK_HRESULT(srcVal.PerformBoxing( aoSrc.m_desc.m_handlerCls ));
            }

            blkDst->Assign( srcVal );
        }
        else
        {
            if(srcVal.IsAValueType())
            {
                if(blkDst->IsAValueType() == false)
                {
                    TINYCLR_CHECK_HRESULT(srcVal.PerformBoxing( aoSrc.m_desc.m_handlerCls ));
                }
            }
            else
            {
                if(blkDst->IsAValueType() == true)
                {
                    TINYCLR_CHECK_HRESULT(srcVal.PerformUnboxing( aoSrc.m_desc.m_handlerCls ));
                }
            }

            TINYCLR_CHECK_HRESULT(blkDst->Reassign( srcVal ));
        }
    }

    TINYCLR_NOCLEANUP();
}


bool CLR_DBG_Debugger::Debugging_Value_Assign( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Value_Assign* cmd    = (CLR_DBG_Commands::Debugging_Value_Assign*)msg->m_payload;
    CLR_RT_HeapBlock*                         blkDst = cmd->m_heapblockDst;
    CLR_RT_HeapBlock*                         blkSrc = cmd->m_heapblockSrc;

    if(blkDst && FAILED(Assign_Helper( blkDst, blkSrc )))
    {
        blkDst = NULL;
    }

    return dbg->GetValue( msg, blkDst, NULL, NULL );
}

//--//

bool CLR_DBG_Debugger::Debugging_TypeSys_Assemblies( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_RT_Assembly_Index assemblies[ CLR_RT_TypeSystem::c_MaxAssemblies ];
    int                   num = 0;

    TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
    {
        assemblies[ num++ ].Set( pASSM->m_idx );
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    dbg->m_messaging->ReplyToCommand( msg, true, false, assemblies, sizeof(CLR_RT_Assembly_Index) * num );

    return true;
}

bool CLR_DBG_Debugger::Debugging_TypeSys_AppDomains( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
#if defined(TINYCLR_APPDOMAINS)
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    int        num                = 0;
    CLR_UINT32 appDomainIDs[ 256 ];

    TINYCLR_FOREACH_NODE(CLR_RT_AppDomain, appDomain, g_CLR_RT_ExecutionEngine.m_appDomains)
    {
        appDomainIDs[ num++ ] = appDomain->m_id;

        if(num >= ARRAYSIZE(appDomainIDs)) break;
    }
    TINYCLR_FOREACH_NODE_END();

    dbg->m_messaging->ReplyToCommand( msg, true, false, appDomainIDs, sizeof(CLR_UINT32) * num );

    return true;
#else
    return false;
#endif
}

//--//

bool CLR_DBG_Debugger::Debugging_Resolve_AppDomain( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
#if defined(TINYCLR_APPDOMAINS)
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Resolve_AppDomain*        cmd           = (CLR_DBG_Commands::Debugging_Resolve_AppDomain*)msg->m_payload;
    CLR_RT_AppDomain*                                     appDomain     = dbg->GetAppDomainFromID( cmd->m_id );
    CLR_UINT32                                            numAssemblies = 0;
    CLR_DBG_Commands::Debugging_Resolve_AppDomain::Reply* cmdReply;
    CLR_UINT8                                             buf[ sizeof(CLR_DBG_Commands::Debugging_Resolve_AppDomain::Reply) + sizeof(CLR_RT_Assembly_Index)*CLR_RT_TypeSystem::c_MaxAssemblies ];
    size_t                                                count;
    LPCSTR                                                name;
    CLR_RT_Assembly_Index*                                pAssemblyIndex;

    if(appDomain)
    {
        cmdReply = (CLR_DBG_Commands::Debugging_Resolve_AppDomain::Reply*)&buf;

        cmdReply->m_state = appDomain->m_state;

        name  = appDomain->m_strName->StringText();
        count = __min( hal_strlen_s( name ) + 1, sizeof(cmdReply->m_szName) - 1 );

        hal_strncpy_s( cmdReply->m_szName, ARRAYSIZE(cmdReply->m_szName), name, count );

        pAssemblyIndex = (CLR_RT_Assembly_Index*)(&cmdReply->m_assemblies);

        TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN(appDomain)
        {
            pAssemblyIndex->Set( pASSM->m_idx );
            pAssemblyIndex++;
            numAssemblies++;
        }
        TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN_END();

        dbg->m_messaging->ReplyToCommand( msg, true, false, cmdReply, sizeof(*cmdReply) + sizeof(CLR_RT_Assembly_Index) * (numAssemblies - 1) );
    }
    else
    {
        dbg->m_messaging->ReplyToCommand( msg, false, false );
    }

    return true;
#else
    return false;
#endif
}

bool CLR_DBG_Debugger::Debugging_Resolve_Assembly( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Resolve_Assembly*       cmd = (CLR_DBG_Commands::Debugging_Resolve_Assembly*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Resolve_Assembly::Reply cmdReply;
    CLR_RT_Assembly*                                    assm = dbg->IsGoodAssembly( cmd->m_idx.Assembly() );

    if(assm)
    {
#if defined(_WIN32)
        //append path
        if(assm->m_strPath != NULL)
        {
            sprintf_s( cmdReply.m_szName, ARRAYSIZE(cmdReply.m_szName), "%s,%s", assm->m_szName, assm->m_strPath->c_str() );
        }
        else
#endif
        {
            hal_strncpy_s( cmdReply.m_szName, ARRAYSIZE(cmdReply.m_szName), assm->m_szName, MAXSTRLEN(cmdReply.m_szName) );
        }

        cmdReply.m_flags   = assm->m_flags;
        cmdReply.m_version = assm->m_header->version;

        dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

        return true;
    }

    dbg->m_messaging->ReplyToCommand( msg, false, false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Resolve_Type( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Resolve_Type*       cmd = (CLR_DBG_Commands::Debugging_Resolve_Type*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Resolve_Type::Reply cmdReply;
    CLR_RT_TypeDef_Instance                         inst;

    if(dbg->CheckTypeDef( cmd->m_td, inst ))
    {
        LPSTR  szBuffer =           cmdReply.m_type;
        size_t iBuffer  = MAXSTRLEN(cmdReply.m_type);

        if(SUCCEEDED(g_CLR_RT_TypeSystem.BuildTypeName( inst, szBuffer, iBuffer )))
        {
            dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

            return true;
        }
    }

    dbg->m_messaging->ReplyToCommand( msg, false, false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Resolve_Field( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Resolve_Field*       cmd = (CLR_DBG_Commands::Debugging_Resolve_Field*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Resolve_Field::Reply cmdReply;
    CLR_RT_FieldDef_Instance                         inst;

    if(dbg->CheckFieldDef( cmd->m_fd, inst ))
    {
        LPSTR  szBuffer =           cmdReply.m_name;
        size_t iBuffer  = MAXSTRLEN(cmdReply.m_name);

        if(SUCCEEDED(g_CLR_RT_TypeSystem.BuildFieldName( inst, szBuffer, iBuffer )))
        {
            CLR_RT_TypeDef_Instance instClass; instClass.InitializeFromField( inst );

            cmdReply.m_td    = instClass;
            cmdReply.m_index = inst.CrossReference().m_offset;

            dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

            return true;
        }
    }

    dbg->m_messaging->ReplyToCommand( msg, false, false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Resolve_Method( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Resolve_Method*       cmd = (CLR_DBG_Commands::Debugging_Resolve_Method*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Resolve_Method::Reply cmdReply;
    CLR_RT_MethodDef_Instance                         inst;
    CLR_RT_TypeDef_Instance                           instOwner;

    if(dbg->CheckMethodDef( cmd->m_md, inst ) && instOwner.InitializeFromMethod( inst ))
    {
        LPSTR  szBuffer =           cmdReply.m_method;
        size_t iBuffer  = MAXSTRLEN(cmdReply.m_method);

        cmdReply.m_td = instOwner;

        CLR_SafeSprintf( szBuffer, iBuffer, "%s", inst.m_assm->GetString( inst.m_target->name ) );

        dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

        return true;
    }

    dbg->m_messaging->ReplyToCommand( msg, false, false );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Resolve_VirtualMethod( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Resolve_VirtualMethod*       cmd = (CLR_DBG_Commands::Debugging_Resolve_VirtualMethod*)msg->m_payload;
    CLR_DBG_Commands::Debugging_Resolve_VirtualMethod::Reply cmdReply;
    CLR_RT_TypeDef_Index                                     cls;
    CLR_RT_MethodDef_Index                                   md;

    cmdReply.m_md.Clear();

    if(SUCCEEDED(CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( *cmd->m_obj, cls )))
    {
        if(g_CLR_RT_EventCache.FindVirtualMethod( cls, cmd->m_md, md ))
        {
            cmdReply.m_md = md;
        }
    }

    dbg->m_messaging->ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

    return true;
}


#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

//--//

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

bool CLR_DBG_Debugger::Debugging_Deployment_Status( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Deployment_Status::Reply* cmdReply;
    CLR_UINT32                                            totLength;
    CLR_UINT32                                            deploySectorsNum  = 0;
    CLR_UINT32                                            deploySectorStart = 0;
    CLR_UINT32                                            deployLength      = 0;

    const BlockDeviceInfo*                                deviceInfo;

    // find the first device in list with DEPLOYMENT blocks
    if (m_deploymentStorageDevice != NULL)
    {
        BlockStorageStream stream;

        if(stream.Initialize( BlockUsage::DEPLOYMENT, m_deploymentStorageDevice ))
        {
            do
            {
                if(deploySectorsNum == 0)
                {
                    deploySectorStart = stream.CurrentAddress();
                }
                deployLength     += stream.Length;
                deploySectorsNum ++;
            }
            while(stream.NextStream() && stream.BaseAddress == (deploySectorStart + deployLength));
        }

        deviceInfo = m_deploymentStorageDevice->GetDeviceInfo();

        totLength = sizeof(CLR_DBG_Commands::Debugging_Deployment_Status::Reply) + (deploySectorsNum) * sizeof(CLR_DBG_Commands::Debugging_Deployment_Status::FlashSector);

        cmdReply = (CLR_DBG_Commands::Debugging_Deployment_Status::Reply*)CLR_RT_Memory::Allocate( totLength, true );

        if(!cmdReply) return false;

        CLR_RT_Memory::ZeroFill( cmdReply, totLength );

        cmdReply->m_entryPoint          = g_CLR_RT_TypeSystem.m_entryPoint.m_data;
        cmdReply->m_storageStart        = deploySectorStart;
        cmdReply->m_storageLength       = deployLength;
        cmdReply->m_eraseWord           = 0xffffffff; //Is this true for all current devices?
        cmdReply->m_maxSectorErase_uSec = m_deploymentStorageDevice->MaxBlockErase_uSec();
        cmdReply->m_maxWordWrite_uSec   = m_deploymentStorageDevice->MaxSectorWrite_uSec();

        int index = 0;

        bool fDone = false;

        if(stream.Initialize( BlockUsage::DEPLOYMENT, m_deploymentStorageDevice ))
        {
            do
            {
                FLASH_WORD  * dataBuf = NULL;
                CLR_UINT32 crc=0;

                if (!(deviceInfo->Attribute.SupportsXIP))
                {
                    // length for each block can be different, so should malloc and free at each block
                    dataBuf = (FLASH_WORD* )CLR_RT_Memory::Allocate( stream.BlockLength, true );  if(!dataBuf) return false;
                }

                //or should the PC have to calculate this??
                // need to read the data to a buffer first.
                if (m_deploymentStorageDevice->IsBlockErased( stream.CurrentAddress(), stream.Length ))
                {
                     crc = CLR_DBG_Commands::Monitor_DeploymentMap::c_CRC_Erased_Sentinel;
                }
                else
                {
                    int len = stream.Length;
                    while(len > 0)
                    {
                        stream.Read( (BYTE **)&dataBuf, stream.BlockLength );
                        
                        crc = SUPPORT_ComputeCRC( dataBuf, stream.BlockLength, crc );

                        len -= stream.BlockLength;
                    }
                }

                if (!(deviceInfo->Attribute.SupportsXIP))
                {
                    CLR_RT_Memory::Release( dataBuf );
                }

                // need real address
                cmdReply->m_data[ index ].m_start  = stream.BaseAddress;
                cmdReply->m_data[ index ].m_length = stream.Length;
                cmdReply->m_data[ index ].m_crc    = crc;
                index ++;

                if(index >= (INT32)deploySectorsNum)
                {
                    fDone = true;
                    break;
                }

            }
            while(stream.NextStream());
        }

        dbg->m_messaging->ReplyToCommand( msg, true, false, cmdReply, totLength );

        CLR_RT_Memory::Release( cmdReply );

        return true;
    }
    else
    {
        dbg->m_messaging->ReplyToCommand( msg, false, false, NULL, 0 );
        return false;
    }
}

bool CLR_DBG_Debugger::Debugging_Info_SetJMC_Method( const CLR_RT_MethodDef_Index& idx, bool fJMC )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_RT_MethodDef_Instance inst;

    if(!CheckMethodDef( idx, inst )        ) return false;
    if(inst.m_target->RVA == CLR_EmptyIndex) return false;

    inst.DebuggingInfo().SetJMC( fJMC );

    return true;
}

bool CLR_DBG_Debugger::Debugging_Info_SetJMC_Type( const CLR_RT_TypeDef_Index& idx, bool fJMC )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    const CLR_RECORD_TYPEDEF*     td;
          CLR_RT_TypeDef_Instance inst;
          int                     totMethods;
          CLR_RT_MethodDef_Index  md;

    if(!CheckTypeDef( idx, inst )) return false;

    td         = inst.m_target;
    totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;

    for(int i=0; i<totMethods; i++)
    {
        md.Set( idx.Assembly(), td->methods_First + i );

        Debugging_Info_SetJMC_Method( md, fJMC );
    }

    return true;
}

bool CLR_DBG_Debugger::Debugging_Info_SetJMC( WP_Message* msg, void* owner )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    CLR_DBG_Debugger* dbg = (CLR_DBG_Debugger*)owner;
    CLR_DBG_Commands::Debugging_Info_SetJMC* cmd  = (CLR_DBG_Commands::Debugging_Info_SetJMC*)msg->m_payload;
    bool                                     fJMC = (cmd->m_fIsJMC != 0);

    switch(cmd->m_kind)
    {
    case REFLECTION_ASSEMBLY:
        {
            CLR_RT_Assembly* assm = dbg->IsGoodAssembly( cmd->m_data.m_assm.Assembly() );

            if(!assm) return false;

            for(int i=0; i<assm->m_pTablesSize[ TBL_TypeDef ]; i++)
            {
                CLR_RT_TypeDef_Index idx;

                idx.Set( cmd->m_data.m_assm.Assembly(), i );

                dbg->Debugging_Info_SetJMC_Type( idx, fJMC );
            }

            return true;
        }

    case REFLECTION_TYPE:
        return dbg->Debugging_Info_SetJMC_Type( cmd->m_data.m_type, fJMC );

    case REFLECTION_METHOD:
        return dbg->Debugging_Info_SetJMC_Method( cmd->m_data.m_method, fJMC );

    default:
        return false;
    }
}

#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

