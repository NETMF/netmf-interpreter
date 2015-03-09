////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    public struct HeapBlock
    {
        public uint id;
        public uint lowPart;
        public uint highPart;
    }

    public class VersionStruct
    {
        public ushort major;
        public ushort minor;
        public ushort build;
        public ushort revision;

        public Version Version
        {
            get { return new Version(major, minor, build, revision); }
        }
    }

    public class Flags
    {
        public const ushort c_NonCritical = 0x0001; // This doesn't need an acknowledge.
        public const ushort c_Reply = 0x0002; // This is the result of a command.
        public const ushort c_BadHeader = 0x0004;
        public const ushort c_BadPayload = 0x0008;
        public const ushort c_Spare0010 = 0x0010;
        public const ushort c_Spare0020 = 0x0020;
        public const ushort c_Spare0040 = 0x0040;
        public const ushort c_Spare0080 = 0x0080;
        public const ushort c_Spare0100 = 0x0100;
        public const ushort c_Spare0200 = 0x0200;
        public const ushort c_Spare0400 = 0x0400;
        public const ushort c_Spare0800 = 0x0800;
        public const ushort c_Spare1000 = 0x1000;
        public const ushort c_NoCaching = 0x2000;
        public const ushort c_NACK = 0x4000;
        public const ushort c_ACK = 0x8000;
    }

    [Serializable]
    public class Packet
    {
        public static string MARKER_DEBUGGER_V1 = "MSdbgV1\0"; // Used to identify the debugger at boot time.
        public static string MARKER_PACKET_V1 = "MSpktV1\0"; // Used to identify the start of a packet.
        public const int SIZE_OF_SIGNATURE = 8;

        public byte[] m_signature = new byte[SIZE_OF_SIGNATURE];
        public uint m_crcHeader = 0;
        public uint m_crcData = 0;

        public uint m_cmd = 0;
        public ushort m_seq = 0;
        public ushort m_seqReply = 0;
        public uint m_flags = 0;
        public uint m_size = 0;
    }

    public class Commands
    {
        public const uint c_Monitor_Ping = 0x00000000; // The payload is empty, this command is used to let the other side know we are here...
        public const uint c_Monitor_Message = 0x00000001; // The payload is composed of the string characters, no zero at the end.
        public const uint c_Monitor_ReadMemory = 0x00000002;
        public const uint c_Monitor_WriteMemory = 0x00000003;
        public const uint c_Monitor_CheckMemory = 0x00000004;
        public const uint c_Monitor_EraseMemory = 0x00000005;
        public const uint c_Monitor_Execute = 0x00000006;
        public const uint c_Monitor_Reboot = 0x00000007;
        public const uint c_Monitor_MemoryMap = 0x00000008;
        public const uint c_Monitor_ProgramExit = 0x00000009; // The payload is empty, this command is used to tell the PC of a program termination        
        public const uint c_Monitor_CheckSignature = 0x0000000A;
        public const uint c_Monitor_DeploymentMap = 0x0000000B;
        public const uint c_Monitor_FlashSectorMap = 0x0000000C;
        public const uint c_Monitor_SignatureKeyUpdate = 0x0000000D;
        public const uint c_Monitor_OemInfo = 0x0000000E;

        public class Monitor_Message : IConverter
        {
            public byte[] m_data = null;

            public void PrepareForDeserialize(int size, byte[] data, Converter converter)
            {
                m_data = new byte[size];
            }

            public override string ToString()
            {
                return Encoding.UTF8.GetString(m_data);
            }
        }

        public class Monitor_FlashSectorMap
        {
            public const uint c_MEMORY_USAGE_BOOTSTRAP = 0x00000010;
            public const uint c_MEMORY_USAGE_CODE = 0x00000020;
            public const uint c_MEMORY_USAGE_CONFIG = 0x00000030;
            public const uint c_MEMORY_USAGE_FS = 0x00000040;
            public const uint c_MEMORY_USAGE_DEPLOYMENT = 0x00000050;
            public const uint c_MEMORY_USAGE_UPDATE = 0x0060;
            public const uint c_MEMORY_USAGE_SIMPLE_A = 0x00000090;
            public const uint c_MEMORY_USAGE_SIMPLE_B = 0x000000A0;
            public const uint c_MEMORY_USAGE_STORAGE_A = 0x000000E0;
            public const uint c_MEMORY_USAGE_STORAGE_B = 0x000000F0;
            public const uint c_MEMORY_USAGE_MASK = 0x000000F0;

            public struct FlashSectorData
            {
                public uint m_address;
                public uint m_size;
                public uint m_flags;
            }

            public class Reply : IConverter
            {
                public FlashSectorData[] m_map = null;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    int num = size / (3 * 4);  // size divided by size of FlashSectorData struct (3*sizeof(uint))

                    m_map = new FlashSectorData[num];
                }
            }
        }

        public class Monitor_Ping
        {
            public const uint c_Ping_Source_TinyCLR = 0x00000000;
            public const uint c_Ping_Source_TinyBooter = 0x00000001;
            public const uint c_Ping_Source_Host = 0x00000002;

            public const uint c_Ping_DbgFlag_Stop       = 0x00000001;
            public const uint c_Ping_DbgFlag_BigEndian  = 0x02000002;
            public const uint c_Ping_DbgFlag_AppExit    = 0x00000004;

            public uint m_source;
            public uint m_dbg_flags;


            public class Reply
            {
                public uint m_source;
                public uint m_dbg_flags;
            }
        }

        public class Monitor_OemInfo
        {
            public class Reply : IConverter
            {
                public ReleaseInfo m_releaseInfo;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_releaseInfo = new ReleaseInfo();
                }
            }
        }

        public class Monitor_ReadMemory
        {
            public uint m_address = 0;
            public uint m_length = 0;

            public class Reply : IConverter
            {
                public byte[] m_data = null;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_data = new byte[size];
                }
            }
        }

        public class Monitor_WriteMemory
        {
            public uint m_address = 0;
            public uint m_length = 0;
            public byte[] m_data = null;

            public void PrepareForSend(uint address, byte[] data, int offset, int length)
            {
                m_address = address;
                m_length = (uint)length;
                m_data = new byte[length];

                Array.Copy(data, offset, m_data, 0, length);
            }
        }

        public class Monitor_CheckMemory
        {
            public uint m_address = 0;
            public uint m_length = 0;

            public class Reply
            {
                public uint m_crc = 0;
            }
        }

        public class Monitor_EraseMemory
        {
            public uint m_address = 0;
            public uint m_length = 0;
        }

        public class Monitor_Execute
        {
            public uint m_address = 0;
        }

        public class Monitor_MemoryMap
        {
            public const uint c_RAM = 0x00000001;
            public const uint c_FLASH = 0x00000002;

            public struct Range
            {
                public uint m_address;
                public uint m_length;
                public uint m_flags;
            }

            public class Reply : IConverter
            {
                public Range[] m_map = null;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    int num = size / (3 * 4);

                    m_map = new Range[num];

                    for (int i = 0; i < num; i++)
                    {
                        m_map[i] = new Range();
                    }
                }
            }
        }

        public class Monitor_Signature
        {
            public uint m_keyIndex;
            public uint m_length = 0;
            public byte[] m_signature = null;

            public void PrepareForSend(byte[] signature, uint keyIndex)
            {
                uint length = (uint)signature.Length;

                m_keyIndex = keyIndex;
                m_length = length;
                m_signature = new byte[length];

                Array.Copy(signature, 0, m_signature, 0, length);
            }
        }

        public class Monitor_Reboot
        {
            public const uint c_NormalReboot = 0;
            public const uint c_EnterBootloader = 1;
            public const uint c_ClrRebootOnly = 2;
            public const uint c_ClrWaitForDbg = 4;

            public uint m_flags = 0;
        }

        public class Monitor_SignatureKeyUpdate
        {
            public const uint c_SignatureSize = 128;
            public const uint c_PublicKeySize = 260;

            public uint m_keyIndex;
            public byte[] m_newPublicKeySignature = new byte[128];
            public byte[] m_newPublicKey = new byte[260];
            public uint m_reserveLength = 0;
            public byte[] m_reserveData;

            public bool PrepareForSend(uint keyIndex, byte[] newPublicKeySig, byte[] newPublicKey, byte[] reserveData)
            {
                m_keyIndex = keyIndex;

                if (newPublicKey.Length != m_newPublicKey.Length) return false;

                if (newPublicKeySig != null)
                {
                    if (newPublicKeySig.Length != m_newPublicKeySignature.Length) return false;

                    Array.Copy(newPublicKeySig, 0, m_newPublicKeySignature, 0, m_newPublicKeySignature.Length);
                }

                Array.Copy(newPublicKey, 0, m_newPublicKey, 0, m_newPublicKey.Length);

                if (reserveData == null)
                {
                    m_reserveLength = 0;
                    m_reserveData = new byte[0];
                }
                else
                {
                    m_reserveLength = (uint)reserveData.Length;
                    m_reserveData = new byte[m_reserveLength];

                    if (m_reserveLength > 0)
                    {
                        Array.Copy(reserveData, 0, m_reserveData, 0, m_reserveLength);
                    }
                }

                return true;
            }
        }

        public class Monitor_DeploymentMap
        {
            public struct DeploymentData
            {
                public uint m_address;
                public uint m_size;
                public uint m_CRC;
            }

            public class Reply : IConverter
            {
                public DeploymentData[] m_map = null;
                public int m_count = 0;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    int num = (size - 4) / (3 * 4);  // size - sizof(m_count) divided by size of deplpoymentdata struct (3*sizeof(uint))

                    m_map = new DeploymentData[num];
                }
            }
        }

        public const uint c_Debugging_Execution_BasePtr = 0x00020000; // Returns the pointer for the ExecutionEngine object.
        public const uint c_Debugging_Execution_ChangeConditions = 0x00020001; // Sets/resets the state of the debugger.
        public const uint c_Debugging_Execution_SecurityKey = 0x00020002; // Sets security key.
        public const uint c_Debugging_Execution_Unlock = 0x00020003; // Unlocks the low-level command, for mfg. test programs.
        public const uint c_Debugging_Execution_Allocate = 0x00020004; // Permanently allocates some memory.
        public const uint c_Debugging_Execution_Breakpoints = 0x00020005; // Sets breakpoints.
        public const uint c_Debugging_Execution_BreakpointHit = 0x00020006; // Notification that a breakpoint was hit.
        public const uint c_Debugging_Execution_BreakpointStatus = 0x00020007; // Queries last breakpoint hit.
        public const uint c_Debugging_Execution_QueryCLRCapabilities = 0x00020008; // Queries capabilities of the CLR.
        public const uint c_Debugging_Execution_SetCurrentAppDomain = 0x00020009; // Sets current AppDomain for subsequent debugging operations

        public const uint c_Debugging_Thread_Create = 0x00020010; // Creates a new thread, based on a static method.
        public const uint c_Debugging_Thread_List = 0x00020011; // Lists the active/waiting threads.
        public const uint c_Debugging_Thread_Stack = 0x00020012; // Lists the call stack for a thread.
        public const uint c_Debugging_Thread_Kill = 0x00020013; // Kills a thread.
        public const uint c_Debugging_Thread_Suspend = 0x00020014; // Suspends the execution of a thread.
        public const uint c_Debugging_Thread_Resume = 0x00020015; // Resumes the execution of a thread.
        public const uint c_Debugging_Thread_GetException = 0x00020016; // Gets the current exception.
        public const uint c_Debugging_Thread_Unwind = 0x00020017; // Unwinds to given stack frame.
        public const uint c_Debugging_Thread_CreateEx = 0x00020018; // Creates a new thread as Debugging_Thread_Create that borrows the identity of another thread.
        public const uint c_Debugging_Thread_Get      = 0x00021000; // Gets the current thread.

        public const uint c_Debugging_Stack_Info = 0x00020020; // Gets more info on a stack frame.
        public const uint c_Debugging_Stack_SetIP = 0x00020021; // Sets the IP on a given stack frame.

        public const uint c_Debugging_Value_ResizeScratchPad = 0x00020030; // Resizes the scratchpad area.
        public const uint c_Debugging_Value_GetStack = 0x00020031; // Reads a value from the stack frame.
        public const uint c_Debugging_Value_GetField = 0x00020032; // Reads a value from an object's field.
        public const uint c_Debugging_Value_GetArray = 0x00020033; // Reads a value from an array's element.
        public const uint c_Debugging_Value_GetBlock = 0x00020034; // Reads a value from a heap block.
        public const uint c_Debugging_Value_GetScratchPad = 0x00020035; // Reads a value from the scratchpad area.
        public const uint c_Debugging_Value_SetBlock = 0x00020036; // Writes a value to a heap block.
        public const uint c_Debugging_Value_SetArray = 0x00020037; // Writes a value to an array's element.
        public const uint c_Debugging_Value_AllocateObject = 0x00020038; // Creates a new instance of an object.
        public const uint c_Debugging_Value_AllocateString = 0x00020039; // Creates a new instance of a string.
        public const uint c_Debugging_Value_AllocateArray = 0x0002003A; // Creates a new instance of an array.
        public const uint c_Debugging_Value_Assign = 0x0002003B; // Assigns a value to another value.

        public const uint c_Debugging_TypeSys_Assemblies = 0x00020040; // Lists all the assemblies in the system.
        public const uint c_Debugging_TypeSys_AppDomains = 0x00020044; // Lists all the AppDomans loaded.

        public const uint c_Debugging_Resolve_Assembly = 0x00020050; // Resolves an assembly.
        public const uint c_Debugging_Resolve_Type = 0x00020051; // Resolves a type to a string.
        public const uint c_Debugging_Resolve_Field = 0x00020052; // Resolves a field to a string.
        public const uint c_Debugging_Resolve_Method = 0x00020053; // Resolves a method to a string.
        public const uint c_Debugging_Resolve_VirtualMethod = 0x00020054; // Resolves a virtual method to the actual implementation.
        public const uint c_Debugging_Resolve_AppDomain = 0x00020055; // Resolves an AppDomain to it's name, and list its loaded assemblies.
        
        public const uint c_Debugging_MFUpdate_Start = 0x00020056; // 
        public const uint c_Debugging_MFUpdate_AddPacket = 0x00020057; // 
        public const uint c_Debugging_MFUpdate_Install = 0x00020058; // 
        public const uint c_Debugging_MFUpdate_AuthCmd = 0x00020059; // 
        public const uint c_Debugging_MFUpdate_Authenticate = 0x00020060; // 
        public const uint c_Debugging_MFUpdate_GetMissingPkts = 0x00020061; // 

        public const uint c_Debugging_UpgradeToSsl = 0x00020069; // 

        public const uint c_Debugging_Lcd_NewFrame = 0x00020070; // Reports a new frame sent to the LCD.
        public const uint c_Debugging_Lcd_NewFrameData = 0x00020071; // Reports a new frame sent to the LCD, with its contents.
        public const uint c_Debugging_Lcd_GetFrame = 0x00020072; // Requests the current frame.

        public const uint c_Debugging_Button_Report = 0x00020080; // Reports a button press/release.
        public const uint c_Debugging_Button_Inject = 0x00020081; // Injects a button press/release.

        public const uint c_Debugging_Messaging_Query = 0x00020090; // Checks the presence of an EndPoint.
        public const uint c_Debugging_Messaging_Send = 0x00020091; // Sends a message to an EndPoint.
        public const uint c_Debugging_Messaging_Reply = 0x00020092; // Response from an EndPoint.

        public const uint c_Debugging_Logging_GetNumberOfRecords = 0x000200A0; // Returns the number of records in the log.
        public const uint c_Debugging_Logging_GetRecord = 0x000200A1; // Returns the n-th record in the log.
        public const uint c_Debugging_Logging_Erase = 0x000200A2; // Erases the logs.
        public const uint c_Debugging_Logging_GetRecords = 0x000200A3; // Returns multiple records, starting from the n-th record.

        public const uint c_Debugging_Deployment_Status = 0x000200B0; // Returns entryPoint and boundary of deployment area.

        public const uint c_Debugging_Info_SetJMC = 0x000200C0; // Sets code to be flagged as JMC (Just my code).

        public const uint c_Profiling_Command = 0x00030000; // Controls various aspects of profiling.
        public const uint c_Profiling_Stream = 0x00030001; // Stream for MFProfiler information.

        public class Debugging_Execution_BasePtr
        {
            public class Reply
            {
                public uint m_EE = 0;
            }
        }

        public class Debugging_Execution_ChangeConditions
        {
            public const uint c_Unused00000001 = 0x00000001;
            public const uint c_Unused00000002 = 0x00000002;
            public const uint c_Unused00000004 = 0x00000004;
            public const uint c_LcdSendFrame = 0x00000100;
            public const uint c_LcdSendFrameNotification = 0x00000200;
            public const uint c_State_Initialize = 0x00000000;
            public const uint c_State_ProgramRunning = 0x00000400;
            public const uint c_State_ProgramExited = 0x00000800;
            public const uint c_State_Mask = 0x00000c00;
            public const uint c_BreakpointsDisabled = 0x00001000;
            public const int c_fDebugger_Quiet = 0x00010000; // Do not spew debug text to the debugger
            public const uint c_PauseTimers = 0x04000000; // Threads associated with timers are created in "suspended" mode.
            public const uint c_NoCompaction = 0x08000000; // Don't perform compaction during execution.
            public const uint c_SourceLevelDebugging = 0x10000000;
            public const uint c_RebootPending = 0x20000000;
            public const uint c_Enabled = 0x40000000;
            public const uint c_Stopped = 0x80000000;

            public uint m_set = 0;
            public uint m_reset = 0;

            public class Reply
            {
                public uint m_current = 0;
            }
        }

        public class Debugging_Execution_SecurityKey
        {
            public byte[] m_key = new byte[32];
        };

        public class Debugging_Execution_Unlock
        {
            public byte[] m_command = new byte[128];
            public byte[] m_hash = new byte[128];
        };

        public class Debugging_Execution_Allocate
        {
            public uint m_size;

            public class Reply
            {
                public uint m_address = 0;
            }
        };

        public class Debugging_UpgradeToSsl
        {
            public uint m_flags;

            public class Reply
            {
                public int m_success;
            }
        }

        public class Debugging_MFUpdate_Start
        {
            public const int c_UpdateProviderSize = 64;

            public byte[] m_updateProvider = new byte[c_UpdateProviderSize];
            public uint m_updateId;
            public uint m_updateType;
            public uint m_updateSubType;
            public uint m_updateSize;
            public uint m_updatePacketSize;
            public ushort m_updateVerMajor;
            public ushort m_updateVerMinor;
        
            public class Reply
            {
                public int m_updateHandle;
            };
        };

        
        public class Debugging_MFUpdate_AuthCommand
        {
            public int    m_updateHandle;
            public uint   m_authCommand;
            public uint   m_authArgsSize;
            public byte[] m_authArgs;

            public bool PrepareForSend(byte[] authArgs)
            {
                m_authArgsSize = (uint)authArgs.Length;
                m_authArgs     = new byte[m_authArgsSize];

                Array.Copy(authArgs, 0, m_authArgs, 0, m_authArgsSize);
                
                return true;
            }
        
            public class Reply : IConverter
            {
                public int    m_success;
                public uint   m_responseSize;
                public byte[] m_response;

                public void  PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_response = new byte[(size - 8)]; // subtract sizeof(m_success) and sizeof(m_responseSize)
                }
            };
        };
        
        public class Debugging_MFUpdate_Authenticate
        {
            public int    m_updateHandle;
            public uint   m_authenticationSize;
            public byte[] m_authenticationData;

            public bool PrepareForSend(byte[] authenticationData)
            {
                m_authenticationSize = authenticationData == null ? 0 : (uint)authenticationData.Length;
                m_authenticationData = new byte[m_authenticationSize];

                if (m_authenticationSize > 0)
                {
                    Array.Copy(authenticationData, 0, m_authenticationData, 0, m_authenticationSize);
                }
                
                return true;
            }
            
            public class Reply
            {
                public int m_success;
            };
        };

        public class Debugging_MFUpdate_GetMissingPkts
        {
            public int m_updateHandle;

            public class Reply : IConverter
            {
                public int m_success;
                public int m_missingPktCount;
                public uint[] m_missingPkts;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_missingPkts = new uint[(size - 8) / 4]; // subtract sizeof(m_success) and sizeof(m_missingPktCount)
                }
            };
        };

        public class Debugging_MFUpdate_AddPacket
        {
            public int  m_updateHandle;
            public uint m_packetIndex;
            public uint m_packetValidation;
            public uint m_packetLength = 0;
            public byte[] m_packetData;

            public void PrepareForSend(byte[] packetData)
            {
                m_packetLength = (uint)packetData.Length;
                m_packetData = new byte[m_packetLength];

                Array.Copy(packetData, 0, m_packetData, 0, m_packetLength);
            }
        
            public class Reply
            {
                public uint m_success;
            };
        };
        
        public class Debugging_MFUpdate_Install
        {
            public int m_updateHandle;
            public uint m_updateValidationSize;
            public byte[] m_updateValidation;

            public void PrepareForSend(byte[] packetValidation)
            {
                m_updateValidationSize = (uint)packetValidation.Length;
                m_updateValidation = new byte[m_updateValidationSize];

                Array.Copy(packetValidation, 0, m_updateValidation, 0, m_updateValidationSize);
            }
        
            public class Reply
            {
                public uint m_success;
            };
        };
        
        

        public class Debugging_Execution_BreakpointDef
        {
            public const ushort c_STEP_IN = 0x0001;
            public const ushort c_STEP_OVER = 0x0002;
            public const ushort c_STEP_OUT = 0x0004;
            public const ushort c_HARD = 0x0008;
            public const ushort c_EXCEPTION_THROWN = 0x0010;
            public const ushort c_EXCEPTION_CAUGHT = 0x0020;
            public const ushort c_EXCEPTION_UNCAUGHT = 0x0040;
            public const ushort c_THREAD_TERMINATED = 0x0080;
            public const ushort c_THREAD_CREATED = 0x0100;
            public const ushort c_ASSEMBLIES_LOADED = 0x0200;
            public const ushort c_LAST_BREAKPOINT = 0x0400;
            public const ushort c_STEP_JMC = 0x0800;
            public const ushort c_BREAK = 0x1000;
            public const ushort c_EVAL_COMPLETE = 0x2000;
            public const ushort c_EXCEPTION_UNWIND = 0x4000;
            public const ushort c_EXCEPTION_FINALLY = 0x8000;

            public const ushort c_STEP = c_STEP_IN | c_STEP_OUT | c_STEP_OVER;

            public const uint c_PID_ANY = 0xFFFFFFFF;

            public const uint c_DEPTH_EXCEPTION_FIRST_CHANCE = 0x00000000;
            public const uint c_DEPTH_EXCEPTION_USERS_CHANCE = 0x00000001;
            public const uint c_DEPTH_EXCEPTION_HANDLER_FOUND = 0x00000002;

            public const uint c_DEPTH_STEP_NORMAL = 0x00000010;
            public const uint c_DEPTH_STEP_RETURN = 0x00000020;
            public const uint c_DEPTH_STEP_CALL = 0x00000030;
            public const uint c_DEPTH_STEP_EXCEPTION_FILTER = 0x00000040;
            public const uint c_DEPTH_STEP_EXCEPTION_HANDLER = 0x00000050;
            public const uint c_DEPTH_STEP_INTERCEPT = 0x00000060;
            public const uint c_DEPTH_STEP_EXIT = 0x00000070;

            public const uint c_DEPTH_UNCAUGHT = 0xFFFFFFFF;

            public ushort m_id;
            public ushort m_flags;

            public uint m_pid;
            public uint m_depth;

            //m_IPStart, m_IPEnd are used for optimizing stepping operations.  a STEP_IN | STEP_OVER breakpoint will be
            //hit in the given stack frame only if the IP is outside of the given range [m_IPStart m_IPEnd)  
            public uint m_IPStart;
            public uint m_IPEnd;

            public uint m_md;
            public uint m_IP;

            public uint m_td;

            public uint m_depthExceptionHandler;
        }

        public class Debugging_Execution_Breakpoints
        {
            public uint m_flags;

            public Debugging_Execution_BreakpointDef[] m_data;
        }

        public class Debugging_Execution_BreakpointHit
        {
            public Debugging_Execution_BreakpointDef m_hit;
        }

        public class Debugging_Execution_BreakpointStatus
        {
            public class Reply
            {
                public Debugging_Execution_BreakpointDef m_lastHit;
            }
        }

        public class Debugging_Execution_QueryCLRCapabilities
        {
            public const uint c_CapabilityFlags = 1;
            public const uint c_CapabilityLCD = 2;
            public const uint c_CapabilitySoftwareVersion = 3;

            public const uint c_CapabilityHalSystemInfo = 5;
            public const uint c_CapabilityClrInfo = 6;
            public const uint c_CapabilitySolutionReleaseInfo = 7;

            public const uint c_CapabilityFlags_FloatingPort = 0x00000001;
            public const uint c_CapabilityFlags_SourceLevelDebugging = 0x00000002;
            public const uint c_CapabilityFlags_AppDomains = 0x00000004;
            public const uint c_CapabilityFlags_ExceptionFilters = 0x00000008;
            public const uint c_CapabilityFlags_IncrementalDeployment = 0x00000010;
            public const uint c_CapabilityFlags_SoftReboot = 0x00000020;
            public const uint c_CapabilityFlags_Profiling = 0x00000040;
            public const uint c_CapabilityFlags_Profiling_Allocations = 0x00000080;
            public const uint c_CapabilityFlags_Profiling_Calls = 0x00000100;

            public uint m_caps;

            public class Reply : IConverter
            {
                public byte[] m_data = null;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_data = new byte[size];
                }
            }

            public class LCD
            {
                public uint m_width;
                public uint m_height;
                public uint m_bpp;
            }
            public class SoftwareVersion
            {
                public byte[] m_buildDate = new byte[20];
                public uint m_compilerVersion;
            }

            public class OEM_MODEL_SKU
            {
                public byte OEM;
                public byte Model;
                public ushort SKU;
            }

            public class OEM_SERIAL_NUMBERS : IConverter
            {
                public byte[] module_serial_number;
                public byte[] system_serial_number;

                public OEM_SERIAL_NUMBERS()
                {
                    module_serial_number = new byte[32];
                    system_serial_number = new byte[16];
                }

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    module_serial_number = new byte[32];
                    system_serial_number = new byte[16];
                }
            }

            public class HalSystemInfo : IConverter
            {
                public ReleaseInfo m_releaseInfo;
                public OEM_MODEL_SKU m_OemModelInfo;
                public OEM_SERIAL_NUMBERS m_OemSerialNumbers;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_releaseInfo = new ReleaseInfo();
                    m_OemModelInfo = new OEM_MODEL_SKU();
                    m_OemSerialNumbers = new OEM_SERIAL_NUMBERS();
                }
            }

            public class ClrInfo : IConverter
            {
                public ReleaseInfo m_clrReleaseInfo;
                public VersionStruct m_TargetFrameworkVersion;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_clrReleaseInfo = new ReleaseInfo();
                    m_TargetFrameworkVersion = new VersionStruct();
                }
            }
        }

        public class Debugging_Execution_SetCurrentAppDomain
        {
            public uint m_id;
        }

        public class Debugging_Thread_Create
        {
            public uint m_md = 0;            
            public int m_scratchPad = 0;

            public class Reply
            {
                public uint m_pid = 0;
            }
        }

        public class Debugging_Thread_CreateEx
        {
            public uint m_md = 0;           
            public int m_scratchPad = 0;
            public uint m_pid = 0;

            public class Reply
            {
                public uint m_pid = 0;
            }
        }

        public class Debugging_Thread_List
        {
            public class Reply : IConverter
            {
                public uint m_num = 0;
                public uint[] m_pids = null;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_pids = new uint[(size - 4) / 4];
                }
            }
        }

        public class Debugging_Thread_Stack
        {
            public uint m_pid = 0;

            public class Reply : IConverter
            {
                #region Thread flags

                public const uint TH_S_Ready = 0x00000000;
                public const uint TH_S_Waiting = 0x00000001;
                public const uint TH_S_Terminated = 0x00000002;

                public const uint TH_F_Suspended = 0x00000001;
                public const uint TH_F_Aborted = 0x00000002;
                public const uint TH_F_Finalizer = 0x00000004;
                public const uint TH_F_ContainsDoomedAppDomain = 0x00000008;

                #endregion

                #region Stack Flags

                public const uint c_MethodKind_Native = 0x00000000;
                public const uint c_MethodKind_Interpreted = 0x00000001;
                public const uint c_MethodKind_Jitted = 0x00000002;
                public const uint c_MethodKind_Mask = 0x00000003;

                public const uint c_UNUSED_00000004 = 0x00000004;
                public const uint c_UNUSED_00000008 = 0x00000008;

                public const uint c_ExecutingConstructor = 0x00000010;
                public const uint c_CompactAndRestartOnOutOfMemory = 0x00000020;
                public const uint c_CallOnPop = 0x00000040;
                public const uint c_CalledOnPop = 0x00000080;

                public const uint c_NeedToSynchronize = 0x00000100;
                public const uint c_PendingSynchronize = 0x00000200;
                public const uint c_Synchronized = 0x00000400;
                public const uint c_UNUSED_00000800 = 0x00000800;

                public const uint c_NeedToSynchronizeGlobally = 0x00001000;
                public const uint c_PendingSynchronizeGlobally = 0x00002000;
                public const uint c_SynchronizedGlobally = 0x00004000;
                public const uint c_PseudoStackFrameForFilter = 0x00080000;

                public const uint c_ExecutingIL = 0x00010000;
                public const uint c_CallerIsCompatibleForCall = 0x00020000;
                public const uint c_CallerIsCompatibleForRet = 0x00040000;
                public const uint c_UNUSED_00080000 = 0x00080000;

                public const uint c_UNUSED_00100000 = 0x00100000;
                public const uint c_UNUSED_00200000 = 0x00200000;
                public const uint c_UNUSED_00400000 = 0x00400000;
                public const uint c_UNUSED_00800000 = 0x00800000;

                public const uint c_UNUSED_01000000 = 0x01000000;
                public const uint c_UNUSED_02000000 = 0x02000000;

                public const uint c_AppDomainMethodInvoke = 0x04000000;
                public const uint c_AppDomainInjectException = 0x08000000;
                public const uint c_AppDomainTransition = 0x10000000;
                public const uint c_InvalidIP = 0x20000000;
                public const uint c_ShouldInterceptException = 0x40000000;
                public const uint c_HasBreakpoint = 0x80000000;

                #endregion

                public class Call
                {
                    public uint m_md;
                    public uint m_IP;
                }

                public class CallEx : Call
                {
                    public uint m_appDomainID;
                    public uint m_flags;
                }

                public uint m_num = 0;
                public uint m_status = 0;
                public uint m_flags = 0;
                public Call[] m_data = null;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    if (converter.Capabilities.AppDomains)
                    {
                        m_data = new CallEx[(size - 12) / 16];
                    }
                    else
                    {
                        m_data = new Call[(size - 12) / 8];
                    }
                }
            }
        }

        public class Debugging_Thread_Kill
        {
            public uint m_pid = 0;

            public class Reply
            {
                public int m_result = 0;
            }
        }

        public class Debugging_Thread_Suspend
        {
            public uint m_pid = 0;
        }

        public class Debugging_Thread_Resume
        {
            public uint m_pid = 0;
        }

        public class Debugging_Thread_Get
        {
            public uint m_pid = 0;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Thread_GetException
        {
            public uint m_pid = 0;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Thread_Unwind
        {
            public uint m_pid = 0;
            public uint m_depth = 0;
        }

        public class Debugging_Stack_Info
        {
            public uint m_pid;
            public uint m_depth;

            public class Reply
            {
                public uint m_md;
                public uint m_IP;
                public uint m_numOfArguments;
                public uint m_numOfLocals;
                public uint m_depthOfEvalStack;
            }
        }

        public class Debugging_Stack_SetIP
        {
            public uint m_pid;
            public uint m_depth;

            public uint m_IP;
            public uint m_depthOfEvalStack;
        }

        public class Debugging_Value_Reply : IConverter
        {
            public Debugging_Value[] m_values;

            public void PrepareForDeserialize(int size, byte[] data, Converter converter)
            {
                m_values = Debugging_Value.Allocate(size, data);
            }
        }

        public class Debugging_Value
        {
            public const uint HB_Alive = 0x01;
            public const uint HB_KeepAlive = 0x02;
            public const uint HB_Event = 0x04;
            public const uint HB_Pinned = 0x08;
            public const uint HB_Boxed = 0x10;
            public const uint HB_NeedFinalizer = 0x20;
            public const uint HB_Signaled = 0x40;
            public const uint HB_SignalAutoReset = 0x80;


            public uint m_referenceID;
            public uint m_dt;
            public uint m_flags;
            public uint m_size;

            public byte[] m_builtinValue = new byte[128];

            // For DATATYPE_STRING

            public uint m_bytesInString;
            public uint m_charsInString;

            // For DATATYPE_VALUETYPE or DATATYPE_CLASSTYPE

            public uint m_td;

            // For DATATYPE_SZARRAY

            public uint m_array_numOfElements;
            public uint m_array_depth;
            public uint m_array_typeIndex;

            // For values from an array.

            public uint m_arrayref_referenceID;
            public uint m_arrayref_index;


            static internal Debugging_Value[] Allocate(int size, byte[] data)
            {
                int num = size / (12 * 4 + 128);

                Debugging_Value[] res = new Debugging_Value[num];

                for (int i = 0; i < num; i++)
                {
                    res[i] = new Debugging_Value();
                }

                return res;
            }
        }

        public class Debugging_Value_ResizeScratchPad
        {
            public int m_size;
        }

        public class Debugging_Value_GetStack
        {
            public const uint c_Local = 0;
            public const uint c_Argument = 1;
            public const uint c_EvalStack = 2;

            public uint m_pid;
            public uint m_depth;
            public uint m_kind;
            public uint m_index;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Value_GetField
        {
            public uint m_heapblock;
            public uint m_offset;
            public uint m_fd;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Value_GetArray
        {
            public uint m_heapblock;
            public uint m_index;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Value_GetBlock
        {
            public uint m_heapblock;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Value_GetScratchPad
        {
            public int m_index;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Value_SetBlock
        {
            public uint m_heapblock;
            public uint m_dt;
            public byte[] m_value = new byte[8];
        }

        public class Debugging_Value_SetArray
        {
            public uint m_heapblock;
            public uint m_index;
            public byte[] m_value = new byte[8];   // Only primitive support for now
        }

        public class Debugging_Value_AllocateObject
        {
            public int m_index;
            public uint m_td;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Value_AllocateString
        {
            public int m_index;
            public uint m_size;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Value_AllocateArray
        {
            public int m_index;
            public uint m_td;
            public uint m_depth;
            public uint m_numOfElements;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Value_Assign
        {
            public uint m_heapblockSrc;
            public uint m_heapblockDst;

            // Reply is Debugging_Value_Reply
        }

        public class Debugging_Reply_Uint_Array : IConverter
        {
            public uint[] m_data = null;

            public void PrepareForDeserialize(int size, byte[] data, Converter converter)
            {
                m_data = new uint[(size) / 4];
            }
        }

        public class Debugging_TypeSys_Assemblies
        {
            public class Reply : Debugging_Reply_Uint_Array
            {
            }
        }

        public class Debugging_TypeSys_AppDomains
        {
            public class Reply : Debugging_Reply_Uint_Array
            {
            }
        }

        public class Debugging_Resolve_Type
        {
            public uint m_td = 0;

            public class Reply
            {
                public byte[] m_type = new byte[512];
            }

            public class Result
            {
                public string m_name;
            }
        }

        public class Debugging_Resolve_Field
        {
            public uint m_fd = 0;

            public class Reply
            {
                public uint m_td = 0;
                public uint m_offset = 0;
                public byte[] m_name = new byte[512];
            }

            public class Result
            {
                public uint m_td;
                public uint m_offset;
                public string m_name;
            }
        }

        public class Debugging_Resolve_Method
        {
            public uint m_md = 0;

            public class Reply
            {
                public uint m_td;
                public byte[] m_method = new byte[512];
            }

            public class Result
            {
                public uint m_td;
                public string m_name;
            }
        }

        public class Debugging_Resolve_Assembly
        {
            public uint m_idx = 0;
            [NonSerialized]
            public Reply m_reply;

            public struct Version
            {
                public ushort iMajorVersion;
                public ushort iMinorVersion;
                public ushort iBuildNumber;
                public ushort iRevisionNumber;

                public override string ToString()
                {
                    return string.Format("{0}.{1}.{2}.{3}", iMajorVersion, iMinorVersion, iBuildNumber, iRevisionNumber);
                }
            }

            public class Reply
            {
                public const uint c_Resolved = 0x00000001;
                public const uint c_Patched = 0x00000002;
                public const uint c_PreparedForExecution = 0x00000004;
                public const uint c_Deployed = 0x00000008;
                public const uint c_PreparingForExecution = 0x00000010;

                public uint m_flags;
                public Version m_version;
                public byte[] m_nameBuffer = new byte[512]; // char

                [NonSerialized]
                private string m_name;
                [NonSerialized]
                private string m_path;

                private void EnsureName()
                {
                    if (m_name == null)
                    {
                        string name = Commands.GetZeroTerminatedString(m_nameBuffer, false);
                        string path = null;

                        int iComma = name.IndexOf(',');

                        if (iComma >= 0)
                        {
                            path = name.Substring(iComma + 1);
                            name = name.Substring(0, iComma);
                        }

                        m_name = name;
                        m_path = path;
                    }
                }

                public string Name
                {
                    get
                    {
                        EnsureName();

                        return m_name;
                    }
                }

                public string Path
                {
                    get
                    {
                        EnsureName();

                        return m_path;
                    }
                }
            }
        }

        public class Debugging_Resolve_VirtualMethod
        {
            public uint m_md;
            public uint m_obj;

            public class Reply
            {
                public uint m_md;
            }
        }

        public class Debugging_Resolve_AppDomain
        {
            public static uint AppDomainState_Loaded = 0;
            public static uint AppDomainState_Unloading = 1;
            public static uint AppDomainState_Unloaded = 2;

            public uint m_id;

            public class Reply : IConverter
            {
                public uint m_state;
                public byte[] m_szName = new byte[512];
                public uint[] m_data = null;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_data = new uint[(size - 512 - 4) / 4];
                }

                public string Name
                {
                    get
                    {
                        return Commands.GetZeroTerminatedString(m_szName, false);
                    }
                }
            }
        }

        public class Debugging_Lcd_FrameData : IConverter
        {
            public class Header
            {
                public ushort m_widthInWords;
                public ushort m_heightInPixels;
            }

            public Header m_header = null;
            public uint[] m_data = null;

            public void PrepareForDeserialize(int size, byte[] data, Converter converter)
            {
                m_header = new Header();

                converter.Deserialize(m_header, data);

                int sizeData = m_header.m_heightInPixels * m_header.m_widthInWords;

                m_data = new uint[sizeData];
            }
        }

        public class Debugging_Lcd_NewFrame
        {
        }

        public class Debugging_Lcd_NewFrameData : Debugging_Lcd_FrameData
        {
        }

        public class Debugging_Lcd_GetFrame
        {
            public class Reply : Debugging_Lcd_FrameData
            {
            }
        }

        public class Debugging_Button_Report
        {
            public uint m_pressed;
            public uint m_released;
        }

        public class Debugging_Button_Inject
        {
            public uint m_pressed;
            public uint m_released;
        }

        public class Debugging_Messaging_Address
        {
            public const int c_size = 5 * 4;

            public uint m_seq;

            public uint m_from_Type;
            public uint m_from_Id;

            public uint m_to_Type;
            public uint m_to_Id;
        }

        public class Debugging_Messaging_Query
        {
            public Debugging_Messaging_Address m_addr = new Debugging_Messaging_Address();

            public class Reply
            {
                public uint m_found = 0;
                public Debugging_Messaging_Address m_addr = new Debugging_Messaging_Address();
            }
        }

        public class Debugging_Messaging_Send : IConverter
        {
            public Debugging_Messaging_Address m_addr = new Debugging_Messaging_Address();
            public byte[] m_data = null;

            public void PrepareForDeserialize(int size, byte[] data, Converter converter)
            {
                m_data = new byte[size - Debugging_Messaging_Address.c_size];
            }

            public class Reply
            {
                public uint m_found = 0;
                public Debugging_Messaging_Address m_addr = new Debugging_Messaging_Address();
            }
        }

        public class Debugging_Messaging_Reply : IConverter
        {
            public Debugging_Messaging_Address m_addr = new Debugging_Messaging_Address();
            public byte[] m_data = null;

            public void PrepareForDeserialize(int size, byte[] data, Converter converter)
            {
                m_data = new byte[size - Debugging_Messaging_Address.c_size];
            }

            public class Reply
            {
                public uint m_found = 0;
                public Debugging_Messaging_Address m_addr = new Debugging_Messaging_Address();
            }
        }

        public class Debugging_Deployment_Status
        {
            public const int c_CRC_Erased_Sentinel = 0x0;

            public struct FlashSector
            {
                public uint m_start;
                public uint m_length;
                public uint m_crc;
            };

            public class Reply
            {
                public uint m_entryPoint;
                public uint m_storageStart;
                public uint m_storageLength;
            }

            public class ReplyEx : Reply, IConverter
            {
                //Optional, for incremental deployment
                public uint m_eraseWord;
                public uint m_maxSectorErase_uSec;
                public uint m_maxWordWrite_uSec;

                public FlashSector[] m_data = null;

                public void PrepareForDeserialize(int size, byte[] data, Converter converter)
                {
                    m_data = new FlashSector[(size - 6 * 4) / (3 * 4)];
                }
            }
        }

        public class Debugging_Info_SetJMC
        {
            public uint m_fIsJMC;
            public uint m_kind;

            public uint m_raw;
        }

        public class Profiling_Command
        {
            public const byte c_Command_ChangeConditions = 0x01;
            public const byte c_Command_FlushStream = 0x02;

            public class ChangeConditionsFlags
            {
                public const uint c_Enabled = 0x00000001;
                public const uint c_Allocations = 0x00000002;
                public const uint c_Calls = 0x00000004;
                public const uint c_TinyCLRTypes = 0x00000008;
            }

            public byte m_command;
            public uint m_parm1;
            public uint m_parm2;


            public class Reply
            {
                public uint m_raw = 0;
            }
        }

        public class Profiling_Stream : IConverter
        {
            public ushort seqId;
            public ushort bitLen;
            public byte[] payload;

            public void PrepareForDeserialize(int size, byte[] data, Converter converter)
            {
                payload = new byte[size - 4];
            }
        }

        internal static string GetZeroTerminatedString(byte[] buf, bool fUTF8)
        {
            if (buf == null) return null;

            int len = 0;
            int num = buf.Length;

            while (len < num && buf[len] != 0) len++;

            if (fUTF8) return Encoding.UTF8.GetString(buf, 0, len);
            else return Encoding.ASCII.GetString(buf, 0, len);
        }

        public static object ResolveCommandToPayload(uint cmd, bool fReply, CLRCapabilities capabilities)
        {
            if (fReply)
            {
                switch (cmd)
                {
                    case c_Monitor_Ping: return new Monitor_Ping.Reply();
                    case c_Monitor_OemInfo: return new Monitor_OemInfo.Reply();
                    case c_Monitor_ReadMemory: return new Monitor_ReadMemory.Reply();
                    case c_Monitor_CheckMemory: return new Monitor_CheckMemory.Reply();
                    case c_Monitor_MemoryMap: return new Monitor_MemoryMap.Reply();
                    case c_Monitor_DeploymentMap: return new Monitor_DeploymentMap.Reply();
                    case c_Monitor_FlashSectorMap: return new Monitor_FlashSectorMap.Reply();

                    case c_Debugging_Execution_BasePtr: return new Debugging_Execution_BasePtr.Reply();
                    case c_Debugging_Execution_ChangeConditions: return new Debugging_Execution_ChangeConditions.Reply();
                    case c_Debugging_Execution_Allocate: return new Debugging_Execution_Allocate.Reply();
                    case c_Debugging_Execution_BreakpointStatus: return new Debugging_Execution_BreakpointStatus.Reply();
                    case c_Debugging_Execution_QueryCLRCapabilities: return new Debugging_Execution_QueryCLRCapabilities.Reply();

                    case c_Debugging_MFUpdate_Start: return new Debugging_MFUpdate_Start.Reply();
                    case c_Debugging_MFUpdate_AuthCmd: return new Debugging_MFUpdate_AuthCommand.Reply();
                    case c_Debugging_MFUpdate_Authenticate: return new Debugging_MFUpdate_Authenticate.Reply();
                    case c_Debugging_MFUpdate_GetMissingPkts: return new Debugging_MFUpdate_GetMissingPkts.Reply();
                    case c_Debugging_MFUpdate_AddPacket: return new Debugging_MFUpdate_AddPacket.Reply();
                    case c_Debugging_MFUpdate_Install: return new Debugging_MFUpdate_Install.Reply();

                    case c_Debugging_UpgradeToSsl: return new Debugging_UpgradeToSsl.Reply();

                    case c_Debugging_Thread_Create: return new Debugging_Thread_Create.Reply();
                    case c_Debugging_Thread_CreateEx: return new Debugging_Thread_CreateEx.Reply();
                    case c_Debugging_Thread_List: return new Debugging_Thread_List.Reply();
                    case c_Debugging_Thread_Stack: return new Debugging_Thread_Stack.Reply();
                    case c_Debugging_Thread_Kill: return new Debugging_Thread_Kill.Reply();
                    case c_Debugging_Thread_GetException: return new Debugging_Value_Reply();
                    case c_Debugging_Thread_Get: return new Debugging_Value_Reply();

                    case c_Debugging_Stack_Info: return new Debugging_Stack_Info.Reply();

                    case c_Debugging_Value_GetStack: return new Debugging_Value_Reply();
                    case c_Debugging_Value_GetField: return new Debugging_Value_Reply();
                    case c_Debugging_Value_GetArray: return new Debugging_Value_Reply();
                    case c_Debugging_Value_GetBlock: return new Debugging_Value_Reply();
                    case c_Debugging_Value_GetScratchPad: return new Debugging_Value_Reply();
                    case c_Debugging_Value_AllocateObject: return new Debugging_Value_Reply();
                    case c_Debugging_Value_AllocateString: return new Debugging_Value_Reply();
                    case c_Debugging_Value_AllocateArray: return new Debugging_Value_Reply();
                    case c_Debugging_Value_Assign: return new Debugging_Value_Reply();

                    case c_Debugging_TypeSys_Assemblies: return new Debugging_TypeSys_Assemblies.Reply();
                    case c_Debugging_TypeSys_AppDomains: return new Debugging_TypeSys_AppDomains.Reply();

                    case c_Debugging_Resolve_Type: return new Debugging_Resolve_Type.Reply();
                    case c_Debugging_Resolve_Field: return new Debugging_Resolve_Field.Reply();
                    case c_Debugging_Resolve_Method: return new Debugging_Resolve_Method.Reply();
                    case c_Debugging_Resolve_Assembly: return new Debugging_Resolve_Assembly.Reply();
                    case c_Debugging_Resolve_VirtualMethod: return new Debugging_Resolve_VirtualMethod.Reply();
                    case c_Debugging_Resolve_AppDomain: return new Debugging_Resolve_AppDomain.Reply();

                    case c_Debugging_Lcd_GetFrame: return new Debugging_Lcd_GetFrame.Reply();

                    case c_Debugging_Messaging_Query: return new Debugging_Messaging_Query.Reply();
                    case c_Debugging_Messaging_Send: return new Debugging_Messaging_Send.Reply();
                    case c_Debugging_Messaging_Reply: return new Debugging_Messaging_Reply.Reply();

                    case c_Debugging_Deployment_Status:
                        if (capabilities.IncrementalDeployment) return new Debugging_Deployment_Status.ReplyEx();
                        else return new Debugging_Deployment_Status.Reply();

                    case c_Profiling_Command: return new Profiling_Command.Reply();
                }
            }
            else
            {
                switch (cmd)
                {
                    case c_Monitor_Ping: return new Monitor_Ping();
                    case c_Monitor_Message: return new Monitor_Message();
                    case c_Monitor_ReadMemory: return new Monitor_ReadMemory();
                    case c_Monitor_WriteMemory: return new Monitor_WriteMemory();
                    case c_Monitor_CheckMemory: return new Monitor_CheckMemory();
                    case c_Monitor_EraseMemory: return new Monitor_EraseMemory();
                    case c_Monitor_Execute: return new Monitor_Execute();
                    case c_Monitor_MemoryMap: return new Monitor_MemoryMap();
                    case c_Monitor_Reboot: return new Monitor_Reboot();
                    case c_Monitor_DeploymentMap: return new Monitor_DeploymentMap();
                    case c_Monitor_FlashSectorMap: return new Monitor_FlashSectorMap();
                    case c_Monitor_SignatureKeyUpdate: return new Monitor_SignatureKeyUpdate();

                    case c_Debugging_Execution_BasePtr: return new Debugging_Execution_BasePtr();
                    case c_Debugging_Execution_ChangeConditions: return new Debugging_Execution_ChangeConditions();
                    case c_Debugging_Execution_SecurityKey: return new Debugging_Execution_SecurityKey();
                    case c_Debugging_Execution_Unlock: return new Debugging_Execution_Unlock();
                    case c_Debugging_Execution_Allocate: return new Debugging_Execution_Allocate();
                    case c_Debugging_Execution_BreakpointHit: return new Debugging_Execution_BreakpointHit();
                    case c_Debugging_Execution_BreakpointStatus: return new Debugging_Execution_BreakpointStatus();
                    case c_Debugging_Execution_QueryCLRCapabilities: return new Debugging_Execution_QueryCLRCapabilities();
                    case c_Debugging_Execution_SetCurrentAppDomain: return new Debugging_Execution_SetCurrentAppDomain();

                    case c_Debugging_MFUpdate_Start: return new Debugging_MFUpdate_Start();
                    case c_Debugging_MFUpdate_AuthCmd: return new Debugging_MFUpdate_AuthCommand();
                    case c_Debugging_MFUpdate_Authenticate: return new Debugging_MFUpdate_Authenticate();
                    case c_Debugging_MFUpdate_GetMissingPkts: return new Debugging_MFUpdate_GetMissingPkts();
                    case c_Debugging_MFUpdate_AddPacket: return new Debugging_MFUpdate_AddPacket();
                    case c_Debugging_MFUpdate_Install: return new Debugging_MFUpdate_Install();

                    case c_Debugging_UpgradeToSsl: return new Debugging_UpgradeToSsl();

                    case c_Debugging_Thread_Create: return new Debugging_Thread_Create();
                    case c_Debugging_Thread_CreateEx: return new Debugging_Thread_CreateEx();
                    case c_Debugging_Thread_List: return new Debugging_Thread_List();
                    case c_Debugging_Thread_Stack: return new Debugging_Thread_Stack();
                    case c_Debugging_Thread_Kill: return new Debugging_Thread_Kill();
                    case c_Debugging_Thread_Suspend: return new Debugging_Thread_Suspend();
                    case c_Debugging_Thread_Resume: return new Debugging_Thread_Resume();
                    case c_Debugging_Thread_GetException: return new Debugging_Thread_GetException();
                    case c_Debugging_Thread_Unwind: return new Debugging_Thread_Unwind();
                    case c_Debugging_Thread_Get: return new Debugging_Thread_Get();

                    case c_Debugging_Stack_Info: return new Debugging_Stack_Info();
                    case c_Debugging_Stack_SetIP: return new Debugging_Stack_SetIP();

                    case c_Debugging_Value_ResizeScratchPad: return new Debugging_Value_ResizeScratchPad();
                    case c_Debugging_Value_GetStack: return new Debugging_Value_GetStack();
                    case c_Debugging_Value_GetField: return new Debugging_Value_GetField();
                    case c_Debugging_Value_GetArray: return new Debugging_Value_GetArray();
                    case c_Debugging_Value_GetBlock: return new Debugging_Value_GetBlock();
                    case c_Debugging_Value_GetScratchPad: return new Debugging_Value_GetScratchPad();
                    case c_Debugging_Value_SetBlock: return new Debugging_Value_SetBlock();
                    case c_Debugging_Value_SetArray: return new Debugging_Value_SetArray();
                    case c_Debugging_Value_AllocateObject: return new Debugging_Value_AllocateObject();
                    case c_Debugging_Value_AllocateString: return new Debugging_Value_AllocateString();
                    case c_Debugging_Value_AllocateArray: return new Debugging_Value_AllocateArray();
                    case c_Debugging_Value_Assign: return new Debugging_Value_Assign();

                    case c_Debugging_TypeSys_Assemblies: return new Debugging_TypeSys_Assemblies();
                    case c_Debugging_TypeSys_AppDomains: return new Debugging_TypeSys_AppDomains();

                    case c_Debugging_Resolve_Type: return new Debugging_Resolve_Type();
                    case c_Debugging_Resolve_Field: return new Debugging_Resolve_Field();
                    case c_Debugging_Resolve_Method: return new Debugging_Resolve_Method();
                    case c_Debugging_Resolve_Assembly: return new Debugging_Resolve_Assembly();
                    case c_Debugging_Resolve_VirtualMethod: return new Debugging_Resolve_VirtualMethod();
                    case c_Debugging_Resolve_AppDomain: return new Debugging_Resolve_AppDomain();

                    case c_Debugging_Lcd_NewFrame: return new Debugging_Lcd_NewFrame();
                    case c_Debugging_Lcd_NewFrameData: return new Debugging_Lcd_NewFrameData();
                    case c_Debugging_Lcd_GetFrame: return new Debugging_Lcd_GetFrame();

                    case c_Debugging_Button_Report: return new Debugging_Button_Report();
                    case c_Debugging_Button_Inject: return new Debugging_Button_Inject();

                    case c_Debugging_Messaging_Query: return new Debugging_Messaging_Query();
                    case c_Debugging_Messaging_Send: return new Debugging_Messaging_Send();
                    case c_Debugging_Messaging_Reply: return new Debugging_Messaging_Reply();

                    case c_Debugging_Deployment_Status: return new Debugging_Deployment_Status();

                    case c_Debugging_Info_SetJMC: return new Debugging_Info_SetJMC();

                    case c_Profiling_Stream: return new Profiling_Stream();
                }
            }

            return null;
        }
    }
}



