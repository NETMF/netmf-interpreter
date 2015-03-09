////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_DEBUGGING_H_
#define _TINYCLR_DEBUGGING_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyCLR_Types.h>
#include <TinyCLR_Messaging.h>

#include <WireProtocol.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if NUM_DEBUGGERS > 1
    #define TINYCLR_FOREACH_DEBUGGER(ptr)                                     \
            for(int iDebuggerT = 0; iDebuggerT < NUM_DEBUGGERS; iDebuggerT++) \
            {                                                                 \
                CLR_DBG_Debugger& ptr = g_CLR_DBG_Debuggers[ iDebuggerT ];

#define TINYCLR_FOREACH_DEBUGGER_NO_TEMP()                                    \
            for(int iDebuggerT = 0; iDebuggerT < NUM_DEBUGGERS; iDebuggerT++) \
            {                       
#else
    #define TINYCLR_FOREACH_DEBUGGER(ptr)                                     \
            {                                                                 \
                CLR_DBG_Debugger& ptr = g_CLR_DBG_Debuggers[ 0 ];            
    
    #define TINYCLR_FOREACH_DEBUGGER_NO_TEMP()                                \
            {                                                                 
#endif

#define TINYCLR_FOREACH_DEBUGGER_END() \
        }

////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_DBG_Commands
{
    static const UINT32 c_Monitor_Ping               = 0x00000000; // The payload is empty, this command is used to let the other side know we are here...
    static const UINT32 c_Monitor_Message            = 0x00000001; // The payload is composed of the string characters, no zero at the end.
    static const UINT32 c_Monitor_ReadMemory         = 0x00000002;
    static const UINT32 c_Monitor_WriteMemory        = 0x00000003;
    static const UINT32 c_Monitor_CheckMemory        = 0x00000004;
    static const UINT32 c_Monitor_EraseMemory        = 0x00000005;
    static const UINT32 c_Monitor_Execute            = 0x00000006;
    static const UINT32 c_Monitor_Reboot             = 0x00000007;
    static const UINT32 c_Monitor_MemoryMap          = 0x00000008;
    static const UINT32 c_Monitor_ProgramExit        = 0x00000009; // The payload is empty, this command is used to tell the PC of a program termination
    static const UINT32 c_Monitor_CheckSignature     = 0x0000000A;
    static const UINT32 c_Monitor_DeploymentMap      = 0x0000000B;
    static const UINT32 c_Monitor_FlashSectorMap     = 0x0000000C;
    static const UINT32 c_Monitor_SignatureKeyUpdate = 0x0000000D;
    static const UINT32 c_Monitor_OemInfo            = 0x0000000E;

    //--//

    struct Monitor_Ping
    {
        static const UINT32 c_Ping_Source_TinyCLR    = 0x00000000;
        static const UINT32 c_Ping_Source_TinyBooter = 0x00000001;
        static const UINT32 c_Ping_Source_Host       = 0x00000002;

        static const UINT32 c_Ping_DbgFlag_Stop      = 0x00000001;
        static const UINT32 c_Ping_DbgFlag_BigEndian = 0x02000002;
        static const UINT32 c_Ping_DbgFlag_AppExit   = 0x00000004;

        UINT32 m_source;
        UINT32 m_dbg_flags;

        struct Reply
        {
            UINT32 m_source;
            UINT32 m_dbg_flags;
        };
    };
    
    struct Monitor_OemInfo
    {
        struct Reply
        {
            MfReleaseInfo   m_releaseInfo;
        };
    };

    struct Monitor_Reboot
    {
        static const UINT32 c_NormalReboot    = 0;
        static const UINT32 c_EnterBootloader = 1;
        static const UINT32 c_ClrRebootOnly   = 2;
        static const UINT32 c_ClrStopDebugger = 4;

        UINT32 m_flags;
    };

    struct Monitor_ReadMemory
    {
        UINT32 m_address;
        UINT32 m_length;

        struct Reply
        {
            UINT8 m_data[ 1 ];
        };
    };

    struct Monitor_WriteMemory
    {
        UINT32 m_address;
        UINT32 m_length;
        UINT8  m_data[ 1 ];
    };

    struct Monitor_Signature
    {        
        UINT32 m_keyIndex;
        UINT32 m_length;   
        UINT8  m_signature[ 1 ];
    };

    struct Monitor_CheckMemory
    {
        UINT32 m_address;
        UINT32 m_length;

        struct Reply
        {
            UINT32 m_crc;
        };
    };

    struct Monitor_EraseMemory
    {
        UINT32 m_address;
        UINT32 m_length;
    };

    struct Monitor_Execute
    {
        UINT32 m_address;
    };

    struct Monitor_MemoryMap
    {
        static const UINT32 c_RAM   = 0x00000001;
        static const UINT32 c_FLASH = 0x00000002;

        struct Range
        {
            UINT32 m_address;
            UINT32 m_length;
            UINT32 m_flags;
        };

        Range m_map[ 1 ];
    };


    struct Monitor_DeploymentMap
    {    
        static const CLR_UINT32 c_CRC_Erased_Sentinel = 0x0;

        struct FlashSector
        {
            CLR_UINT32 m_start;
            CLR_UINT32 m_length;
            
            CLR_UINT32 m_crc;
        };

        struct Reply
        {

            FlashSector m_data[ 1 ];
        };
    };


    

    struct Monitor_SignatureKeyUpdate
    {
        UINT32 m_keyIndex;
        UINT8  m_newKeySignature[ 128 ];
        UINT8  m_newKey[ 260 ];
        UINT32 m_reserveLength;
        UINT8  m_reserveData[ 1 ];
    };

    //--------------------------------------------------------------//

    static const UINT32 c_Debugging_Execution_BasePtr              = 0x00020000; // Returns the pointer for the ExecutionEngine object.
    static const UINT32 c_Debugging_Execution_ChangeConditions     = 0x00020001; // Sets/resets the state of the debugger.
    static const UINT32 c_Debugging_Execution_SecurityKey          = 0x00020002; // Sets security key.
    static const UINT32 c_Debugging_Execution_Unlock               = 0x00020003; // Unlock the low-level command, for mfg. test programs.
    static const UINT32 c_Debugging_Execution_Allocate             = 0x00020004; // Permanently allocate some memory.
    static const UINT32 c_Debugging_Execution_Breakpoints          = 0x00020005; // Sets breakpoints.
    static const UINT32 c_Debugging_Execution_BreakpointHit        = 0x00020006; // Notification that a breakpoint was hit.
    static const UINT32 c_Debugging_Execution_BreakpointStatus     = 0x00020007; // Queries last breakpoint hit.
    static const UINT32 c_Debugging_Execution_QueryCLRCapabilities = 0x00020008; // Queries capabilities of the CLR.
    static const UINT32 c_Debugging_Execution_SetCurrentAppDomain  = 0x00020009; // Sets the current AppDomain.  This is required before
                                                                                 // performing certain debugging operations, such as
                                                                                 // accessing a static field, or doing function evaluation,

    static const UINT32 c_Debugging_Thread_Create                  = 0x00020010; // OBSOLETE - Use c_Debugging_Thread_CreateEx instead.
    static const UINT32 c_Debugging_Thread_List                    = 0x00020011; // Lists the active/waiting threads.
    static const UINT32 c_Debugging_Thread_Stack                   = 0x00020012; // Lists the call stack for a thread.
    static const UINT32 c_Debugging_Thread_Kill                    = 0x00020013; // Kills a thread.
    static const UINT32 c_Debugging_Thread_Suspend                 = 0x00020014; // Suspends the execution of a thread.
    static const UINT32 c_Debugging_Thread_Resume                  = 0x00020015; // Resumes the execution of a thread.
    static const UINT32 c_Debugging_Thread_GetException            = 0x00020016; // Gets the current exception.
    static const UINT32 c_Debugging_Thread_Unwind                  = 0x00020017; // Unwinds to given stack frame.
    static const UINT32 c_Debugging_Thread_CreateEx                = 0x00020018; // Creates a new thread but Thread.CurrentThread will return the identity of the passed thread.
    static const UINT32 c_Debugging_Thread_Get                     = 0x00021000; // Gets the current thread.
                                                                 
    static const UINT32 c_Debugging_Stack_Info                     = 0x00020020; // Gets more info on a stack frame.
    static const UINT32 c_Debugging_Stack_SetIP                    = 0x00020021; // Sets the IP on a given thread.
                                                                 
    static const UINT32 c_Debugging_Value_ResizeScratchPad         = 0x00020030; // Resizes the scratchpad area.
    static const UINT32 c_Debugging_Value_GetStack                 = 0x00020031; // Reads a value from the stack frame.
    static const UINT32 c_Debugging_Value_GetField                 = 0x00020032; // Reads a value from an object's field.
    static const UINT32 c_Debugging_Value_GetArray                 = 0x00020033; // Reads a value from an array's element.
    static const UINT32 c_Debugging_Value_GetBlock                 = 0x00020034; // Reads a value from a heap block.
    static const UINT32 c_Debugging_Value_GetScratchPad            = 0x00020035; // Reads a value from the scratchpad area.
    static const UINT32 c_Debugging_Value_SetBlock                 = 0x00020036; // Writes a value to a heap block.
    static const UINT32 c_Debugging_Value_SetArray                 = 0x00020037; // Writes a value to an array's element.
    static const UINT32 c_Debugging_Value_AllocateObject           = 0x00020038; // Creates a new instance of an object.
    static const UINT32 c_Debugging_Value_AllocateString           = 0x00020039; // Creates a new instance of a string.
    static const UINT32 c_Debugging_Value_AllocateArray            = 0x0002003A; // Creates a new instance of an array.
    static const UINT32 c_Debugging_Value_Assign                   = 0x0002003B; // Assigns a value to another value.
                                                                 
    static const UINT32 c_Debugging_TypeSys_Assemblies             = 0x00020040; // Lists all the assemblies in the system.
    static const UINT32 c_Debugging_TypeSys_AppDomains             = 0x00020044; // Lists all the AppDomans loaded.
                                                                 
    static const UINT32 c_Debugging_Resolve_Assembly               = 0x00020050; // Resolves an assembly.
    static const UINT32 c_Debugging_Resolve_Type                   = 0x00020051; // Resolves a type to a string.
    static const UINT32 c_Debugging_Resolve_Field                  = 0x00020052; // Resolves a field to a string.
    static const UINT32 c_Debugging_Resolve_Method                 = 0x00020053; // Resolves a method to a string.
    static const UINT32 c_Debugging_Resolve_VirtualMethod          = 0x00020054; // Resolves a virtual method to the actual implementation.
    static const UINT32 c_Debugging_Resolve_AppDomain              = 0x00020055; // Resolves an AppDomain to it's name, and list its loaded assemblies.


    static const UINT32 c_Debugging_MFUpdate_Start                 = 0x00020056; // 
    static const UINT32 c_Debugging_MFUpdate_AddPacket             = 0x00020057; // 
    static const UINT32 c_Debugging_MFUpdate_Install               = 0x00020058; // 
    static const UINT32 c_Debugging_MFUpdate_AuthCommand           = 0x00020059; // 
    static const UINT32 c_Debugging_MFUpdate_Authenticate          = 0x00020060; // 
    static const UINT32 c_Debugging_MFUpdate_GetMissingPkts        = 0x00020061; // 

    static const UINT32 c_Debugging_UpgradeToSsl                   = 0x00020069; //

    //--//                                                       
                                                                 
    static const UINT32 c_Debugging_Lcd_NewFrame                   = 0x00020070; // Reports a new frame sent to the LCD.
    static const UINT32 c_Debugging_Lcd_NewFrameData               = 0x00020071; // Reports a new frame sent to the LCD, with its contents.
    static const UINT32 c_Debugging_Lcd_GetFrame                   = 0x00020072; // Requests the current frame.
                                                                 
    static const UINT32 c_Debugging_Button_Report                  = 0x00020080; // Reports a button press/release.
    static const UINT32 c_Debugging_Button_Inject                  = 0x00020081; // Injects a button press/release.
                                                                                                                   
    static const UINT32 c_Debugging_Deployment_Status              = 0x000200B0; // Returns entryPoint and boundary of deployment area.
                                                                 
    static const UINT32 c_Debugging_Info_SetJMC                    = 0x000200C0; // Sets code to be flagged as JMC (Just my code).

    static const UINT32 c_Profiling_Command                        = 0x00030000; // Various incoming commands regarding profiling
    static const UINT32 c_Profiling_Stream                         = 0x00030001; // Stream for MFProfiler information.

    //--//

    struct Debugging_Execution_Unlock
    {
        UINT8 m_command[ 128 ];
        UINT8 m_hash   [ 128 ];
    };

    struct Debugging_Execution_QueryCLRCapabilities
    {
        static const CLR_UINT32 c_CapabilityFlags             = 1;
        static const CLR_UINT32 c_CapabilityLCD               = 2;
        static const CLR_UINT32 c_CapabilityVersion           = 3;
        static const CLR_UINT32 c_HalSystemInfo               = 5;
        static const CLR_UINT32 c_ClrInfo                     = 6;
        static const CLR_UINT32 c_SolutionReleaseInfo         = 7;

        static const CLR_UINT32 c_CapabilityFlags_FloatingPoint         = 0x00000001;
        static const CLR_UINT32 c_CapabilityFlags_SourceLevelDebugging  = 0x00000002;
        static const CLR_UINT32 c_CapabilityFlags_AppDomains            = 0x00000004;
        static const CLR_UINT32 c_CapabilityFlags_ExceptionFilters      = 0x00000008;
        static const CLR_UINT32 c_CapabilityFlags_IncrementalDeployment = 0x00000010;
        static const CLR_UINT32 c_CapabilityFlags_SoftReboot            = 0x00000020;
        static const CLR_UINT32 c_CapabilityFlags_Profiling             = 0x00000040;
        static const CLR_UINT32 c_CapabilityFlags_Profiling_Allocations = 0x00000080;
        static const CLR_UINT32 c_CapabilityFlags_Profiling_Calls       = 0x00000100;
        static const CLR_UINT32 c_CapabilityFlags_ThreadCreateEx        = 0x00000400;

        CLR_UINT32 m_cmd;

        struct LCD
        {
            CLR_UINT32 m_width;
            CLR_UINT32 m_height;
            CLR_UINT32 m_bpp;
        };

        struct SoftwareVersion
        {
            char m_buildDate[ 20 ];
            UINT32 m_compilerVersion;
        };

        struct ClrInfo
        {
            MfReleaseInfo m_clrReleaseInfo;
            MFVersion     m_TargetFrameworkVersion;
        };
        
        union ReplyUnion
        {
            CLR_UINT32          u_capsFlags;
            LCD                 u_LCD;
            SoftwareVersion     u_SoftwareVersion;
            HalSystemInfo       u_HalSystemInfo;
            ClrInfo             u_ClrInfo;
            MfReleaseInfo       u_SolutionReleaseInfo;
        };        
    };    

    //--//

    struct Debugging_Messaging_Query
    {
        CLR_RT_HeapBlock_EndPoint::Address m_addr;

        struct Reply
        {
            CLR_UINT32                         m_found;
            CLR_RT_HeapBlock_EndPoint::Address m_addr;
        };
    };

    struct Debugging_Messaging_Send
    {
        CLR_RT_HeapBlock_EndPoint::Address m_addr;
        UINT8                              m_data[ 1 ];

        struct Reply
        {
            CLR_UINT32                         m_found;
            CLR_RT_HeapBlock_EndPoint::Address m_addr;
        };
    };

    struct Debugging_Messaging_Reply
    {
        CLR_RT_HeapBlock_EndPoint::Address m_addr;
        UINT8                              m_data[ 1 ];

        struct Reply
        {
            CLR_UINT32                         m_found;
            CLR_RT_HeapBlock_EndPoint::Address m_addr;
        };
    };

    //--//
    

    struct Debugging_Execution_BasePtr
    {
        struct Reply
        {
            CLR_UINT32 m_EE;
        };
    };

    struct Debugging_Execution_ChangeConditions
    {
        CLR_UINT32 m_set;
        CLR_UINT32 m_reset;

        struct Reply
        {
            CLR_UINT32 m_current;
        };
    };

    struct Debugging_Execution_SecurityKey
    {
        CLR_UINT8 m_key[ 32 ];
    };

    struct Debugging_Execution_Allocate
    {
        CLR_UINT32 m_size;

        struct Reply
        {
            CLR_UINT32 m_address;
        };
    };

    struct Debugging_UpgradeToSsl
    {
        CLR_UINT32 m_flags;

        struct Reply
        {
            CLR_UINT32 m_success;
        };
    };


    struct Debugging_MFUpdate_Start
    {
        char       m_provider[64];
        CLR_UINT32 m_updateId;
        CLR_UINT32 m_updateType;
        CLR_UINT32 m_updateSubType;
        CLR_UINT32 m_updateSize;
        CLR_UINT32 m_updatePacketSize;
        CLR_UINT16 m_versionMajor;
        CLR_UINT16 m_versionMinor;

        struct Reply
        {
            CLR_INT32 m_updateHandle;
        };
    };

    struct Debugging_MFUpdate_AuthCommand
    {
        CLR_UINT32 m_updateHandle;
        CLR_UINT32 m_authCommand;
        CLR_UINT32 m_authArgsSize;
        CLR_UINT8  m_authArgs[1];

        struct Reply
        {
            CLR_INT32  m_success;
            CLR_UINT32 m_responseSize;
            CLR_UINT8  m_response[1];
        };
    };

    struct Debugging_MFUpdate_Authenticate
    {
        CLR_UINT32 m_updateHandle;
        CLR_UINT32 m_authenticationLen;
        CLR_UINT8  m_authenticationData[1];

        struct Reply
        {
            CLR_INT32 m_success;
        };
    };

    struct Debugging_MFUpdate_GetMissingPkts
    {
        CLR_UINT32 m_updateHandle;
        
        struct Reply
        {
            CLR_INT32 m_success;
            CLR_INT32  m_missingPktCount;
            CLR_UINT32 m_missingPkts[1];
        };
    };

    struct Debugging_MFUpdate_AddPacket
    {
        CLR_INT32 m_updateHandle;
        CLR_UINT32 m_packetIndex;
        CLR_UINT32 m_packetValidation;
        CLR_UINT32 m_packetLength;
        CLR_UINT8 m_packetData[1];

        struct Reply
        {
            CLR_UINT32 m_success;
        };
    };

    struct Debugging_MFUpdate_Install
    {
        CLR_INT32 m_updateHandle;
        CLR_UINT32 m_updateValidationSize;
        CLR_UINT8 m_updateValidation[1];

        struct Reply
        {
            CLR_UINT32 m_success;
        };
    };
    

    struct Debugging_Execution_BreakpointDef
    {
        static const CLR_UINT16 c_STEP_IN            = 0x0001;
        static const CLR_UINT16 c_STEP_OVER          = 0x0002;
        static const CLR_UINT16 c_STEP_OUT           = 0x0004;
        static const CLR_UINT16 c_HARD               = 0x0008;
        static const CLR_UINT16 c_EXCEPTION_THROWN   = 0x0010;
        static const CLR_UINT16 c_EXCEPTION_CAUGHT   = 0x0020;
        static const CLR_UINT16 c_EXCEPTION_UNCAUGHT = 0x0040;
        static const CLR_UINT16 c_THREAD_TERMINATED  = 0x0080;
        static const CLR_UINT16 c_THREAD_CREATED     = 0x0100;
        static const CLR_UINT16 c_ASSEMBLIES_LOADED  = 0x0200;
        static const CLR_UINT16 c_LAST_BREAKPOINT    = 0x0400;
        static const CLR_UINT16 c_STEP_JMC           = 0x0800;
        static const CLR_UINT16 c_BREAK              = 0x1000;
        static const CLR_UINT16 c_EVAL_COMPLETE      = 0x2000;
        static const CLR_UINT16 c_EXCEPTION_UNWIND   = 0x4000;
        static const CLR_UINT16 c_EXCEPTION_FINALLY  = 0x8000;

        static const CLR_UINT16 c_STEP               = c_STEP_IN | c_STEP_OVER | c_STEP_OUT;

        static const CLR_UINT32 c_PID_ANY            = 0xFFFFFFFF;

        static const CLR_UINT32 c_DEPTH_EXCEPTION_FIRST_CHANCE    = 0x00000000;
        static const CLR_UINT32 c_DEPTH_EXCEPTION_USERS_CHANCE    = 0x00000001;
        static const CLR_UINT32 c_DEPTH_EXCEPTION_HANDLER_FOUND   = 0x00000002;

        static const CLR_UINT32 c_DEPTH_STEP_NORMAL               = 0x00000010;
        static const CLR_UINT32 c_DEPTH_STEP_RETURN               = 0x00000020;
        static const CLR_UINT32 c_DEPTH_STEP_CALL                 = 0x00000030;
        static const CLR_UINT32 c_DEPTH_STEP_EXCEPTION_FILTER     = 0x00000040;
        static const CLR_UINT32 c_DEPTH_STEP_EXCEPTION_HANDLER    = 0x00000050;
        static const CLR_UINT32 c_DEPTH_STEP_INTERCEPT            = 0x00000060;
        static const CLR_UINT32 c_DEPTH_STEP_EXIT                 = 0x00000070;

        static const CLR_UINT32 c_DEPTH_UNCAUGHT                  = 0xFFFFFFFF;


        CLR_UINT16             m_id;
        CLR_UINT16             m_flags;

        CLR_UINT32             m_pid;
        CLR_UINT32             m_depth;

        //
        // m_IPStart, m_IPEnd are used for optimizing stepping operations.
        // A STEP_IN | STEP_OVER breakpoint will be hit in the given stack frame
        // only if the IP is outside of the given range [m_IPStart m_IPEnd).
        //
        CLR_UINT32             m_IPStart;
        CLR_UINT32             m_IPEnd;

        CLR_RT_MethodDef_Index m_md;
        CLR_UINT32             m_IP;

        CLR_RT_TypeDef_Index   m_td;

        CLR_UINT32             m_depthExceptionHandler;
    };

    struct Debugging_Execution_Breakpoints
    {
        CLR_UINT32                        m_flags;

        Debugging_Execution_BreakpointDef m_data[ 1 ];
    };

    struct Debugging_Execution_BreakpointHit
    {
        Debugging_Execution_BreakpointDef m_hit;
    };

    struct Debugging_Execution_BreakpointStatus
    {
        struct Reply
        {
            Debugging_Execution_BreakpointDef m_lastHit;
        };
    };

    struct Debugging_Execution_SetCurrentAppDomain
    {
        CLR_UINT32 m_id;
    };

    //--//--//

    struct Debugging_Thread_CreateEx
    {
        CLR_RT_MethodDef_Index m_md;
        int                    m_scratchPad;
        CLR_UINT32             m_pid;

        struct Reply
        {
            CLR_UINT32 m_pid;
        };
    };

    //--//

    struct Debugging_Thread_List
    {
        struct Reply
        {
            CLR_UINT32 m_num;
            CLR_UINT32 m_pids[ 1 ];
        };
    };

    //--//

    struct Debugging_Thread_Stack
    {
        CLR_UINT32 m_pid;

        struct Reply
        {
            struct Call
            {
                CLR_RT_MethodDef_Index m_md;
                CLR_UINT32             m_IP;
#if defined(TINYCLR_APPDOMAINS)
                CLR_UINT32             m_appDomainID;
                CLR_UINT32             m_flags;
#endif

            };

            CLR_UINT32 m_num;
            CLR_UINT32 m_status;
            CLR_UINT32 m_flags;
            Call       m_data[ 1 ];
        };
    };

    struct Debugging_Thread_Kill
    {
        CLR_UINT32 m_pid;

        struct Reply
        {
            int m_result;
        };
    };

    struct Debugging_Thread_Suspend
    {
        CLR_UINT32 m_pid;
    };

    struct Debugging_Thread_Resume
    {
        CLR_UINT32 m_pid;
    };

    struct Debugging_Thread_Get
    {
        CLR_UINT32 m_pid;

        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Thread_GetException
    {
        CLR_UINT32 m_pid;

        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Thread_Unwind
    {
        CLR_UINT32 m_pid;
        CLR_UINT32 m_depth;
    };

    //--//

    struct Debugging_Stack_Info
    {
        CLR_UINT32 m_pid;
        CLR_UINT32 m_depth;

        struct Reply
        {
            CLR_RT_MethodDef_Index m_md;
            CLR_UINT32             m_IP;
            CLR_UINT32             m_numOfArguments;
            CLR_UINT32             m_numOfLocals;
            CLR_UINT32             m_depthOfEvalStack;
        };
    };

    struct Debugging_Stack_SetIP
    {
        CLR_UINT32 m_pid;
        CLR_UINT32 m_depth;

        CLR_UINT32 m_IP;
        CLR_UINT32 m_depthOfEvalStack;
    };

    //--//

    struct Debugging_Value
    {
        CLR_RT_HeapBlock*       m_referenceID;
        CLR_UINT32              m_dt;                // CLR_RT_HeapBlock::DataType ()
        CLR_UINT32              m_flags;             // CLR_RT_HeapBlock::DataFlags()
        CLR_UINT32              m_size;              // CLR_RT_HeapBlock::DataSize ()

        //
        // For primitive types
        //
        CLR_UINT8               m_builtinValue[ 128 ]; // Space for string preview...

        //
        // For DATATYPE_STRING
        //
        CLR_UINT32              m_bytesInString;
        LPCSTR                  m_charsInString;

        //
        // For DATATYPE_VALUETYPE or DATATYPE_CLASSTYPE
        //
        CLR_RT_TypeDef_Index    m_td;

        //
        // For DATATYPE_SZARRAY
        //
        CLR_UINT32              m_array_numOfElements;
        CLR_UINT32              m_array_depth;
        CLR_RT_TypeDef_Index    m_array_typeIndex;

        //
        // For values from an array.
        //
        CLR_RT_HeapBlock_Array* m_arrayref_referenceID;
        CLR_UINT32              m_arrayref_index;
    };

    struct Debugging_Value_ResizeScratchPad
    {
        CLR_UINT32 m_size;
    };

    struct Debugging_Value_GetStack
    {
        static const CLR_UINT32 c_Local     = 0;
        static const CLR_UINT32 c_Argument  = 1;
        static const CLR_UINT32 c_EvalStack = 2;

        CLR_UINT32 m_pid;
        CLR_UINT32 m_depth;
        CLR_UINT32 m_kind;
        CLR_UINT32 m_index;


        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Value_GetField
    {
        CLR_RT_HeapBlock*     m_heapblock;
        CLR_UINT32            m_offset;
        CLR_RT_FieldDef_Index m_fd;


        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Value_GetArray
    {
        CLR_RT_HeapBlock* m_heapblock;
        CLR_UINT32        m_index;


        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Value_GetBlock
    {
        CLR_RT_HeapBlock* m_heapblock;


        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Value_GetScratchPad
    {
        CLR_UINT32 m_idx;


        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Value_SetBlock
    {
        CLR_RT_HeapBlock* m_heapblock;
        CLR_UINT32        m_dt;                // CLR_RT_HeapBlock::DataType ()
        CLR_UINT8         m_builtinValue[ 8 ];
    };

    struct Debugging_Value_SetArray
    {
        CLR_RT_HeapBlock_Array* m_heapblock;
        CLR_UINT32              m_index;
        CLR_UINT8               m_builtinValue[ 8 ];
    };

    //--//

    struct Debugging_Value_AllocateObject
    {
        int                  m_index;
        CLR_RT_TypeDef_Index m_td;

        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Value_AllocateString
    {
        int        m_index;
        CLR_UINT32 m_size;

        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Value_AllocateArray
    {
        int                  m_index;
        CLR_RT_TypeDef_Index m_td;
        CLR_UINT32           m_depth;
        CLR_UINT32           m_numOfElements;

        //
        // The reply is an array of Debugging_Value
        //
    };

    struct Debugging_Value_Assign
    {
        CLR_RT_HeapBlock* m_heapblockSrc;
        CLR_RT_HeapBlock* m_heapblockDst;

        //
        // The reply is an array of Debugging_Value
        //
    };

    //--//

    struct Debugging_TypeSys_Assemblies
    {        
        //
        // The reply is just an array of CLR_RT_Assembly_Index.
        //
    };

    struct Debugging_TypeSys_AppDomains
    {
        //
        // The reply is just an array of AppDomainIDs
        //
    };    

    //--//

    struct Debugging_Resolve_AppDomain
    {        
        CLR_UINT32 m_id;        

        struct Reply
        {
            CLR_UINT32 m_state;
            char       m_szName[ 512 ];
            CLR_UINT32 m_assemblies[ 1 ]; //Array of CLR_RT_Assembly_Index
        };
    };

    struct Debugging_Resolve_Assembly
    {
         CLR_RT_Assembly_Index m_idx;

         struct Reply
         {
              CLR_UINT32         m_flags;
              CLR_RECORD_VERSION m_version;
              char               m_szName[ 512 ];
         };
    };

    struct Debugging_Resolve_Type
    {
        CLR_RT_TypeDef_Index m_td;

        struct Reply
        {
            char m_type[ 512 ];
        };
    };

    struct Debugging_Resolve_Field
    {
        CLR_RT_FieldDef_Index m_fd;

        struct Reply
        {
            CLR_RT_TypeDef_Index m_td;
            CLR_UINT32           m_index;
            char                 m_name[ 512 ];
        };
    };

    struct Debugging_Resolve_Method
    {
        CLR_RT_MethodDef_Index m_md;

        struct Reply
        {
            CLR_RT_TypeDef_Index m_td;
            char                 m_method[ 512 ];
        };
    };

    struct Debugging_Resolve_VirtualMethod
    {
        CLR_RT_MethodDef_Index m_md;
        CLR_RT_HeapBlock*      m_obj;

        struct Reply
        {
            CLR_RT_MethodDef_Index m_md;
        };
    };

    //--//

    struct Debugging_Deployment_Status
    {    
        static const CLR_UINT32 c_CRC_Erased_Sentinel = 0x0;

        struct FlashSector
        {
            CLR_UINT32 m_start;
            CLR_UINT32 m_length;
            
            CLR_UINT32 m_crc;
        };

        struct Reply
        {
            CLR_UINT32 m_entryPoint;
            CLR_UINT32 m_storageStart;
            CLR_UINT32 m_storageLength;
            
            CLR_UINT32  m_eraseWord;
            CLR_UINT32  m_maxSectorErase_uSec;
            CLR_UINT32  m_maxWordWrite_uSec;
            FlashSector m_data[ 1 ];
        };
    };

    //--//

    struct Debugging_Info_SetJMC
    {
        CLR_UINT32 m_fIsJMC;
        CLR_UINT32 m_kind;    // CLR_ReflectionType

        union
        {
            CLR_RT_Assembly_Index  m_assm;
            CLR_RT_TypeDef_Index   m_type;
            CLR_RT_MethodDef_Index m_method;
            CLR_UINT32             m_raw;
        } m_data;
    };

    struct Profiling_Command
    {
        static const CLR_UINT8 c_Command_ChangeConditions = 0x01;
        static const CLR_UINT8 c_Command_FlushStream      = 0x02;

        CLR_UINT8 m_command;

        struct Reply
        {
            CLR_UINT32 m_raw;
        };
    };

    struct Profiling_ChangeConditions
    {
        CLR_UINT32 m_set;
        CLR_UINT32 m_reset;
    };

    struct Profiling_Stream
    {
        CLR_UINT16 m_seqId;
        CLR_UINT16 m_bitLen;
    };
    
};

struct CLR_DBG_Debugger
{
    CLR_Messaging*              m_messaging;
    static BlockStorageDevice*  m_deploymentStorageDevice;


    //--//

    static void Debugger_Discovery();
    static void Debugger_WaitForCommands();

    static HRESULT CreateInstance();

    HRESULT Debugger_Initialize( COM_HANDLE port );

    static HRESULT DeleteInstance();

    void Debugger_Cleanup();

    static void BroadcastEvent( UINT32 cmd, UINT32 payloadSize, UINT8* payload, UINT32 flags );

    void ProcessCommands();
    void PurgeCache     ();

private:

    bool CheckPermission( ByteAddress address, int mode );

    bool AccessMemory( CLR_UINT32 location, UINT32 lengthInBytes, BYTE* buf, int mode );

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain*  GetAppDomainFromID ( CLR_UINT32 id );
#endif

    CLR_RT_StackFrame* CheckStackFrame    ( CLR_UINT32 pid, CLR_UINT32 depth, bool& isInline                                        );
    HRESULT            CreateListOfThreads(                 CLR_DBG_Commands::Debugging_Thread_List ::Reply*& cmdReply, int& totLen );
    HRESULT            CreateListOfCalls  ( CLR_UINT32 pid, CLR_DBG_Commands::Debugging_Thread_Stack::Reply*& cmdReply, int& totLen );

    CLR_RT_Assembly*   IsGoodAssembly( CLR_IDX                       idxAssm                                  );
    bool               CheckTypeDef  ( const CLR_RT_TypeDef_Index&   td     , CLR_RT_TypeDef_Instance&   inst );
    bool               CheckFieldDef ( const CLR_RT_FieldDef_Index&  fd     , CLR_RT_FieldDef_Instance&  inst );
    bool               CheckMethodDef( const CLR_RT_MethodDef_Index& md     , CLR_RT_MethodDef_Instance& inst );

    bool               GetValue( WP_Message* msg, CLR_RT_HeapBlock* ptr, CLR_RT_HeapBlock* reference, CLR_RT_TypeDef_Instance* pTD );

    bool AllocateAndQueueMessage( CLR_UINT32 cmd, UINT32 length, UINT8* data, CLR_RT_HeapBlock_EndPoint::Port port, CLR_RT_HeapBlock_EndPoint::Address addr, CLR_UINT32 found );

    bool ProcessHeader                           ( WP_Message* msg );
    bool ProcessPayload                          ( WP_Message* msg );

       
public:  
    static CLR_RT_Thread* GetThreadFromPid ( CLR_UINT32 pid );

    static bool Monitor_Ping                            ( WP_Message* msg, void* owner );
    static bool Monitor_Reboot                          ( WP_Message* msg, void* owner );
    static bool Debugging_Execution_QueryCLRCapabilities( WP_Message* msg, void* owner );

    static bool Monitor_ReadMemory                      ( WP_Message* msg, void* owner );
    static bool Monitor_WriteMemory                     ( WP_Message* msg, void* owner );
    static bool Monitor_CheckMemory                     ( WP_Message* msg, void* owner );
    static bool Monitor_EraseMemory                     ( WP_Message* msg, void* owner );
    static bool Monitor_Execute                         ( WP_Message* msg, void* owner );
    static bool Monitor_MemoryMap                       ( WP_Message* msg, void* owner );
    static bool Monitor_FlashSectorMap                  ( WP_Message* msg, void* owner );
    static bool Monitor_DeploymentMap                   ( WP_Message* msg, void* owner );

                                             
    static bool Debugging_Execution_BasePtr             ( WP_Message* msg, void* owner );
    static bool Debugging_Execution_ChangeConditions    ( WP_Message* msg, void* owner );

    static bool Debugging_Execution_Allocate            ( WP_Message* msg, void* owner );

    static bool Debugging_UpgradeToSsl                  ( WP_Message* msg, void* owner );

    static bool Debugging_MFUpdate_Start                ( WP_Message* msg, void* owner );
    static bool Debugging_MFUpdate_AuthCommand          ( WP_Message* msg, void* owner );
    static bool Debugging_MFUpdate_Authenticate         ( WP_Message* msg, void* owner );
    static bool Debugging_MFUpdate_GetMissingPkts       ( WP_Message* msg, void* owner );
    static bool Debugging_MFUpdate_AddPacket            ( WP_Message* msg, void* owner );
    static bool Debugging_MFUpdate_Install              ( WP_Message* msg, void* owner );

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    static bool Debugging_Execution_Breakpoints         ( WP_Message* msg, void* owner );
    static bool Debugging_Execution_BreakpointStatus    ( WP_Message* msg, void* owner );
    static bool Debugging_Execution_SetCurrentAppDomain ( WP_Message* msg, void* owner );

    static bool Debugging_Thread_CreateEx               ( WP_Message* msg, void* owner );
    static bool Debugging_Thread_List                   ( WP_Message* msg, void* owner );
    static bool Debugging_Thread_Stack                  ( WP_Message* msg, void* owner );
    static bool Debugging_Thread_Kill                   ( WP_Message* msg, void* owner );
    static bool Debugging_Thread_Suspend                ( WP_Message* msg, void* owner );
    static bool Debugging_Thread_Resume                 ( WP_Message* msg, void* owner );
    static bool Debugging_Thread_GetException           ( WP_Message* msg, void* owner );
    static bool Debugging_Thread_Unwind                 ( WP_Message* msg, void* owner );
    static bool Debugging_Thread_Get                    ( WP_Message* msg, void* owner );
                                             
    static bool Debugging_Stack_Info                    ( WP_Message* msg, void* owner );
    static bool Debugging_Stack_SetIP                   ( WP_Message* msg, void* owner );
                                             
    static bool Debugging_Value_ResizeScratchPad        ( WP_Message* msg, void* owner );
    static bool Debugging_Value_GetStack                ( WP_Message* msg, void* owner );
    static bool Debugging_Value_GetField                ( WP_Message* msg, void* owner );
    static bool Debugging_Value_GetArray                ( WP_Message* msg, void* owner );
    static bool Debugging_Value_GetBlock                ( WP_Message* msg, void* owner );
    static bool Debugging_Value_GetScratchPad           ( WP_Message* msg, void* owner );
    static bool Debugging_Value_SetBlock                ( WP_Message* msg, void* owner );
    static bool Debugging_Value_SetArray                ( WP_Message* msg, void* owner );
    static bool Debugging_Value_AllocateObject          ( WP_Message* msg, void* owner );
    static bool Debugging_Value_AllocateString          ( WP_Message* msg, void* owner );
    static bool Debugging_Value_AllocateArray           ( WP_Message* msg, void* owner );
    static bool Debugging_Value_Assign                  ( WP_Message* msg, void* owner );
                                             
    static bool Debugging_TypeSys_Assemblies            ( WP_Message* msg, void* owner );
    static bool Debugging_TypeSys_AppDomains            ( WP_Message* msg, void* owner );
                                             
    static bool Debugging_Resolve_AppDomain             ( WP_Message* msg, void* owner );
    static bool Debugging_Resolve_Assembly              ( WP_Message* msg, void* owner );
    static bool Debugging_Resolve_Type                  ( WP_Message* msg, void* owner );
    static bool Debugging_Resolve_Field                 ( WP_Message* msg, void* owner );
    static bool Debugging_Resolve_Method                ( WP_Message* msg, void* owner );
    static bool Debugging_Resolve_VirtualMethod         ( WP_Message* msg, void* owner );
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    static bool Debugging_Deployment_Status             ( WP_Message* msg, void* owner );
    static bool Debugging_Info_SetJMC                   ( WP_Message* msg, void* owner );
    
    bool Debugging_Info_SetJMC_Type                     ( const CLR_RT_TypeDef_Index&   idx, bool fJMC );
    bool Debugging_Info_SetJMC_Method                   ( const CLR_RT_MethodDef_Index& idx, bool fJMC );
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    static bool Profiling_Command                       ( WP_Message* msg, void* owner );
    bool Profiling_ChangeConditions                     ( WP_Message* msg );
    bool Profiling_FlushStream                          ( WP_Message* msg );
};

//--//

extern CLR_UINT32        g_scratchDebugger[];
extern CLR_UINT32        g_scratchDebuggerMessaging[];
extern CLR_DBG_Debugger *g_CLR_DBG_Debuggers;

//--//

extern const CLR_Messaging_CommandHandlerLookup c_Debugger_Lookup_Request[];
extern const CLR_Messaging_CommandHandlerLookup c_Debugger_Lookup_Reply[];
extern const CLR_UINT32 c_Debugger_Lookup_Request_count;
extern const CLR_UINT32 c_Debugger_Lookup_Reply_count;

//--//

#endif // _TINYCLR_DEBUGGING_H_

