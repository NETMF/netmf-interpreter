using System.Diagnostics;

namespace Microsoft.SPOT.Debugger
{
    using System;
    using System.Diagnostics.Tracing;
    using WireProtocol;
    using System.Collections.Generic;

    [EventSource( Name="Microsoft-NETMF-Debugger")]
    internal class DebuggerEventSource : EventSource
    {
        public static DebuggerEventSource Log { get { return Log_.Value;  } }
        private static readonly Lazy<DebuggerEventSource> Log_ = new Lazy<DebuggerEventSource>( ()=>new DebuggerEventSource() );

#if TRACE
        [Flags]
        enum PacketFlags
        {
            None = 0,
            NonCritical = 0x0001, // This doesn't need an acknowledge.
            Reply = 0x0002, // This is the result of a command.
            BadHeader = 0x0004,
            BadPayload = 0x0008,
            Spare0010 = 0x0010,
            Spare0020 = 0x0020,
            Spare0040 = 0x0040,
            Spare0080 = 0x0080,
            Spare0100 = 0x0100,
            Spare0200 = 0x0200,
            Spare0400 = 0x0400,
            Spare0800 = 0x0800,
            Spare1000 = 0x1000,
            NoCaching = 0x2000,
            NACK = 0x4000,
            ACK = 0x8000,
        }

        private static Dictionary<uint, string> CommandNameMap = new Dictionary<uint, string>
        {
            [ Commands.c_Monitor_Ping ] = "Ping",
            [ Commands.c_Monitor_Message ] = "Message",
            [ Commands.c_Monitor_ReadMemory ] = "ReadMemory",
            [ Commands.c_Monitor_WriteMemory ] = "WriteMemory",
            [ Commands.c_Monitor_CheckMemory ] = "CheckMemory",
            [ Commands.c_Monitor_EraseMemory ] = "EraseMemory",
            [ Commands.c_Monitor_Execute ] = "Execute",
            [ Commands.c_Monitor_Reboot ] = "Reboot",
            [ Commands.c_Monitor_MemoryMap ] = "MemoryMap",
            [ Commands.c_Monitor_ProgramExit ] = "ProgramExit",
            [ Commands.c_Monitor_CheckSignature ] = "CheckSignature",
            [ Commands.c_Monitor_DeploymentMap ] = "DeploymentMap",
            [ Commands.c_Monitor_FlashSectorMap ] = "FlashSectorMap",
            [ Commands.c_Monitor_SignatureKeyUpdate ] = "SignatureKeyUpdate",
            [ Commands.c_Monitor_OemInfo ] = "OemInfo",
            [ Commands.c_Debugging_Execution_BasePtr ] = "Execution_BasePtr",
            [ Commands.c_Debugging_Execution_ChangeConditions ] = "Execution_ChangeConditions",
            [ Commands.c_Debugging_Execution_SecurityKey ] = "Execution_SecurityKey",
            [ Commands.c_Debugging_Execution_Unlock ] = "Execution_Unlock",
            [ Commands.c_Debugging_Execution_Allocate ] = "Execution_Allocate",
            [ Commands.c_Debugging_Execution_Breakpoints ] = "Execution_Breakpoints",
            [ Commands.c_Debugging_Execution_BreakpointHit ] = "Execution_BreakpointHit",
            [ Commands.c_Debugging_Execution_BreakpointStatus ] = "Execution_BreakpointStatus",
            [ Commands.c_Debugging_Execution_QueryCLRCapabilities ] = "Execution_QueryCLRCapabilities",
            [ Commands.c_Debugging_Execution_SetCurrentAppDomain ] = "Execution_SetCurrentAppDomain",
            [ Commands.c_Debugging_Thread_Create ] = "Thread_Create",
            [ Commands.c_Debugging_Thread_List ] = "Thread_List",
            [ Commands.c_Debugging_Thread_Stack ] = "Thread_Stack",
            [ Commands.c_Debugging_Thread_Kill ] = "Thread_Kill",
            [ Commands.c_Debugging_Thread_Suspend ] = "Thread_Suspend",
            [ Commands.c_Debugging_Thread_Resume ] = "Thread_Resume",
            [ Commands.c_Debugging_Thread_GetException ] = "Thread_GetException",
            [ Commands.c_Debugging_Thread_Unwind ] = "Thread_Unwind",
            [ Commands.c_Debugging_Thread_CreateEx ] = "Thread_CreateEx",
            [ Commands.c_Debugging_Thread_Get ] = "Thread_Get",
            [ Commands.c_Debugging_Stack_Info ] = "Stack_Info",
            [ Commands.c_Debugging_Stack_SetIP ] = "Stack_SetIP",
            [ Commands.c_Debugging_Value_ResizeScratchPad ] = "Value_ResizeScratchPad",
            [ Commands.c_Debugging_Value_GetStack ] = "Value_GetStack",
            [ Commands.c_Debugging_Value_GetField ] = "Value_GetField",
            [ Commands.c_Debugging_Value_GetArray ] = "Value_GetArray",
            [ Commands.c_Debugging_Value_GetBlock ] = "Value_GetBlock",
            [ Commands.c_Debugging_Value_GetScratchPad ] = "Value_GetScratchPad",
            [ Commands.c_Debugging_Value_SetBlock ] = "Value_SetBlock",
            [ Commands.c_Debugging_Value_SetArray ] = "Value_SetArray",
            [ Commands.c_Debugging_Value_AllocateObject ] = "Value_AllocateObject",
            [ Commands.c_Debugging_Value_AllocateString ] = "Value_AllocateString",
            [ Commands.c_Debugging_Value_AllocateArray ] = "Value_AllocateArray",
            [ Commands.c_Debugging_Value_Assign ] = "Value_Assign",
            [ Commands.c_Debugging_TypeSys_Assemblies ] = "TypeSys_Assemblies",
            [ Commands.c_Debugging_TypeSys_AppDomains ] = "TypeSys_AppDomains",
            [ Commands.c_Debugging_Resolve_Assembly ] = "Resolve_Assembly",
            [ Commands.c_Debugging_Resolve_Type ] = "Resolve_Type",
            [ Commands.c_Debugging_Resolve_Field ] = "Resolve_Field",
            [ Commands.c_Debugging_Resolve_Method ] = "Resolve_Method",
            [ Commands.c_Debugging_Resolve_VirtualMethod ] = "Resolve_VirtualMethod",
            [ Commands.c_Debugging_Resolve_AppDomain ] = "Resolve_AppDomain",
            [ Commands.c_Debugging_MFUpdate_Start ] = "MFUpdate_Start",
            [ Commands.c_Debugging_MFUpdate_AddPacket ] = "MFUpdate_AddPacket",
            [ Commands.c_Debugging_MFUpdate_Install ] = "MFUpdate_Install",
            [ Commands.c_Debugging_MFUpdate_AuthCmd ] = "MFUpdate_AuthCmd",
            [ Commands.c_Debugging_MFUpdate_Authenticate ] = "MFUpdate_Authenticate",
            [ Commands.c_Debugging_MFUpdate_GetMissingPkts ] = "MFUpdate_GetMissingPkts",
            [ Commands.c_Debugging_UpgradeToSsl ] = "UpgradeToSsl",
            [ Commands.c_Debugging_Lcd_NewFrame ] = "Lcd_NewFrame",
            [ Commands.c_Debugging_Lcd_NewFrameData ] = "Lcd_NewFrameData",
            [ Commands.c_Debugging_Lcd_GetFrame ] = "Lcd_GetFrame",
            [ Commands.c_Debugging_Button_Report ] = "Button_Report",
            [ Commands.c_Debugging_Button_Inject ] = "Button_Inject",
            [ Commands.c_Debugging_Messaging_Query ] = "Messaging_Query",
            [ Commands.c_Debugging_Messaging_Send ] = "Messaging_Send",
            [ Commands.c_Debugging_Messaging_Reply ] = "Messaging_Reply",
            [ Commands.c_Debugging_Logging_GetNumberOfRecords ] = "Logging_GetNumberOfRecords",
            [ Commands.c_Debugging_Logging_GetRecord ] = "Logging_GetRecord",
            [ Commands.c_Debugging_Logging_Erase ] = "Logging_Erase",
            [ Commands.c_Debugging_Logging_GetRecords ] = "Logging_GetRecords",
            [ Commands.c_Debugging_Deployment_Status ] = "Deployment_Status",
            [ Commands.c_Debugging_Info_SetJMC ] = "Info_SetJMC",
            [ Commands.c_Profiling_Command ] = "Profiling_Command",
            [ Commands.c_Profiling_Stream ] = "Profiling_Stream"
        };

        string GetCommandName( uint cmd )
        {
            string retVal;
            if( !CommandNameMap.TryGetValue( cmd, out retVal ) )
                retVal = $"0x{cmd:X08}";

            return retVal;
        }
#endif

        [Event( 1, Opcode = EventOpcode.Send )]
        public void WireProtocolTxHeader( uint crcHeader, uint crcData, uint cmd, uint flags, ushort seq, ushort seqReply, uint length )
        {
#if TRACE
            Trace.TraceInformation( "TX: {0} flags=[{1}] hCRC: 0x{2:X08} pCRC: 0x{3:X08} seq: 0x{4:X04} replySeq: 0x{5:X04} len={6}"
                                      , GetCommandName( cmd )
                                      , ( PacketFlags )flags
                                      , crcHeader
                                      , crcData
                                      , seq
                                      , seqReply
                                      , length
                                      );
#endif
            WriteCustomEvent( 1, crcHeader, crcData, cmd, flags, seq, seqReply, length );
        }

        [Event( 2, Opcode = EventOpcode.Receive )]
        public void WireProtocolRxHeader( uint crcHeader, uint crcData, uint cmd, uint flags, ushort seq, ushort seqReply, uint length )
        {
#if TRACE
            Trace.TraceInformation( "RX: {0} flags=[{1}] hCRC: 0x{2:X08} pCRC: 0x{3:X08} seq: 0x{4:X04} replySeq: 0x{5:X04} len={6}"
                                  , GetCommandName( cmd )
                                  , ( PacketFlags )flags
                                  , crcHeader
                                  , crcData
                                  , seq
                                  , seqReply
                                  , length
                                  );
#endif
            WriteCustomEvent( 2, crcHeader, crcData, cmd, flags, seq, seqReply, length );
        }

        [Event( 3 )]
        public void WireProtocolReceiveState( MessageReassembler.ReceiveState state )
        {
            WriteEvent( 3, ( int )state );
        }

        [Event(4)]
        public void EngineEraseMemory( uint address, uint length )
        {
            Trace.TraceInformation( "EraseMemory: @0x{0:X08}; LEN=0x{1:X08}", address, length );
            WriteEvent( 4, ( int )address, ( int )length );
        }

        [Event(5)]
        public void EngineWriteMemory( uint address, int length )
        {
            Trace.TraceInformation( "WriteMemory: @0x{0:X08}; LEN=0x{1:X08}", address, length );
            WriteEvent( 5, ( int )address, length );
        }

        private DebuggerEventSource()
        {
        }

        [NonEvent]
        unsafe void WriteCustomEvent( int eventId, uint crcHeader, uint crcData, uint cmd, uint flags, ushort seq, ushort seqReply, uint length )
        {
            EventData* pDataDesc = stackalloc EventData[ 7 ];
            pDataDesc[ 0 ].DataPointer = ( IntPtr )( &crcHeader );
            pDataDesc[ 0 ].Size = sizeof( uint );

            pDataDesc[ 1 ].DataPointer = ( IntPtr )( &crcData );
            pDataDesc[ 1 ].Size = sizeof( uint );

            pDataDesc[ 2 ].DataPointer = (IntPtr)( &cmd );
            pDataDesc[ 2 ].Size = sizeof( uint );

            pDataDesc[ 3 ].DataPointer = ( IntPtr )( &flags );
            pDataDesc[ 3 ].Size = sizeof( uint );

            pDataDesc[ 4 ].DataPointer = ( IntPtr )( &seq );
            pDataDesc[ 4 ].Size = sizeof( ushort );

            pDataDesc[ 5 ].DataPointer = ( IntPtr )( &seqReply );
            pDataDesc[ 5 ].Size = sizeof( ushort );

            pDataDesc[ 6 ].DataPointer = ( IntPtr )( &length );
            pDataDesc[ 6 ].Size = sizeof( uint );

            WriteEventCore( eventId, 7, pDataDesc );
        }
    }
}
