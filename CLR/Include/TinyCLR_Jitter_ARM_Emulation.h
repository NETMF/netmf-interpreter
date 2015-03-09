////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_JITTER_ARMEMULATION_H_
#define _TINYCLR_JITTER_ARMEMULATION_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

#define ARMEMULATOR_MONITOR_MEMORY
#define ARMEMULATOR_MONITOR_REGISTERS
#define ARMEMULATOR_MONITOR_OPCODES
#define ARMEMULATOR_MONITOR_CALLS

#define ARMEMULATOR_PROFILE_MODE

//--//

struct ArmEmulator : public ArmProcessor::State
{
    static const CLR_UINT32 c_InteropOpcode = 0xF0000000;

    typedef bool (ArmEmulator::*ProcessCall)();

    //--//

    struct TrackInterop
    {
        CLR_UINT32  m_pc;
        CLR_UINT32  m_op;
        ProcessCall m_call;
    };

    struct TrackExecution
    {
        CLR_UINT64 m_clockTicks;
        CLR_UINT64 m_busAccess_Read;
        CLR_UINT64 m_busAccess_Write;
        CLR_UINT64 m_busAccess_WaitStates;

        //--//

        void Start( State* st )
        {
            m_clockTicks           = st->m_clockTicks          ;
            m_busAccess_Read       = st->m_busAccess_Read      ;
            m_busAccess_Write      = st->m_busAccess_Write     ;
            m_busAccess_WaitStates = st->m_busAccess_WaitStates;
        }

        void End( State* st )
        {
            m_clockTicks           = st->m_clockTicks           - m_clockTicks          ;
            m_busAccess_Read       = st->m_busAccess_Read       - m_busAccess_Read      ;
            m_busAccess_Write      = st->m_busAccess_Write      - m_busAccess_Write     ;
            m_busAccess_WaitStates = st->m_busAccess_WaitStates - m_busAccess_WaitStates;
        }

        LPCSTR ToString()
        {
            static char rgBuffer[ 256 ];

            LPSTR  szBuffer = rgBuffer;
            size_t iBuffer  = MAXSTRLEN(rgBuffer);


            CLR_SafeSprintf( szBuffer, iBuffer, "C:%I64d R:%I64d W:%I64d WS:%I64d", m_clockTicks, m_busAccess_Read, m_busAccess_Write, m_busAccess_WaitStates );

            return rgBuffer;
        }
    };

    struct TrackCall
    {
        CLR_UINT32     m_pc;
        CLR_UINT32     m_lr;
        CLR_UINT32     m_sp;
        std::wstring*  m_name;

        TrackExecution m_te;
    };

    //--//

    typedef std::map< CLR_UINT32, TrackInterop > AddressToHandlerMap;
    typedef AddressToHandlerMap::iterator        AddressToHandlerMapIter;

    //--//

    bool                       m_fMonitorMemory;
    bool                       m_fMonitorRegisters;
    bool                       m_fMonitorOpcodes;
    bool                       m_fMonitorCalls;

    bool                       m_fStopExecution;

    //--//

    AddressToHandlerMap        m_callInterop;

    //--//

    CLR_RT_SymbolToAddressMap& m_symdef;
    CLR_RT_AddressToSymbolMap& m_symdef_Inverse;
    std::vector< TrackCall >   m_callQueue;

    //--//

    ArmEmulator( CLR_RT_SymbolToAddressMap& symdef, CLR_RT_AddressToSymbolMap& symdef_Inverse, ArmProcessor::State::LoadFtn fpnLoad = NULL, ArmProcessor::State::StoreFtn fpnStore = NULL );

    HRESULT Execute( CLR_UINT64 steps );

    void ApplyInterop ( TrackInterop& ti                    );
    void RemoveInterop( CLR_UINT32    pc                    );
    void SetInterop   ( CLR_UINT32    pc  , ProcessCall ftn );
    void SetInterop   ( LPCWSTR       name, ProcessCall ftn );

    LPCWSTR GetContext();
    bool    GetContext( CLR_UINT32 address, CLR_UINT32& context );

    //--//--//

protected:
    bool MemoryLoad ( CLR_UINT32 address, CLR_UINT32& value, CLR_DataType kind ) { return (this->*m_loadFtn )( address, value, kind ); }
    bool MemoryStore( CLR_UINT32 address, CLR_UINT32  value, CLR_DataType kind ) { return (this->*m_storeFtn)( address, value, kind ); }

    bool Interop_GenericSkipCall();

private:
    bool LoadImpl ( CLR_UINT32 address, CLR_UINT32& value, CLR_DataType kind );
    bool StoreImpl( CLR_UINT32 address, CLR_UINT32  value, CLR_DataType kind );
};

//--//

        void ResetCoverage()
        {
        }

        bool GetCoverage( CLR_UINT32& address, CLR_UINT32& size, CLR_UINT32*& counters )
        {
            return false;
        }

        //--//

        static CLR_UINT32 Read( void* target, CLR_DataType kind )
        {
            switch(kind)
            {
                case DATATYPE_U1: return (CLR_UINT32)*(CLR_UINT8 *)target;
                case DATATYPE_U2: return (CLR_UINT32)*(CLR_UINT16*)target;
                case DATATYPE_U4: return (CLR_UINT32)*(CLR_UINT32*)target;
                case DATATYPE_I1: return (CLR_INT32 )*(CLR_INT8  *)target;
                case DATATYPE_I2: return (CLR_INT32 )*(CLR_INT16 *)target;
            }

            return 0xBAADF00D;
        }

        static void Write( void* target, CLR_UINT32 value, CLR_DataType kind )
        {
            switch(kind)
            {
                case DATATYPE_U1: *(CLR_UINT8 *)target = (CLR_UINT8 )value; return;
                case DATATYPE_U2: *(CLR_UINT16*)target = (CLR_UINT16)value; return;
                case DATATYPE_U4: *(CLR_UINT32*)target = (CLR_UINT32)value; return;
                case DATATYPE_I1: *(CLR_INT8  *)target = (CLR_INT8  )value; return;
                case DATATYPE_I2: *(CLR_INT16 *)target = (CLR_INT16 )value; return;
            }
        }
    };

    //--//

    typedef std::map< CLR_UINT32, MemoryRange > AddressToMemoryRangeMap;
    typedef AddressToMemoryRangeMap::iterator   AddressToMemoryRangeMapIter;

    //--//

    static const CLR_UINT32 c_LCD_base = 0xa0000000;
    static const CLR_UINT32 c_LCD_size = 0x00020000;
    static const CLR_UINT32 c_ROM_base = 0x0c000000;
    static const CLR_UINT32 c_ROM_size = 512*1024;

    //--//

    bool                    m_fMonitorSerialPrint;
    bool                    m_fNoSleep;
    CLR_UINT64              m_sleepTicks;

    MemoryRange             m_memories_RAM;
    MemoryRange             m_memories_FLASH;
    AddressToMemoryRangeMap m_memories;
    CLR_UINT32              m_latency_RAM;
    CLR_UINT32              m_latency_FLASH;
    CLR_UINT32              m_latency_LCD;

    CLR_UINT64              m_lastRamWrite;

    //--//

    int                     m_LCD_page;
    int                     m_LCD_column;
    UINT32                  m_LCD_buffer[ LCD_SCREEN_SIZE_IN_WORDS_MAX ];

    //--//

    CLR_UINT64              m_TimeQuantumExpire;
    CLR_UINT32              m_TimeQuantumPtr;

    //--//

    MemoryRange& GetRAM  () { return m_memories_RAM  ; }
    MemoryRange& GetFLASH() { return m_memories_FLASH; }

    //--//

    void ResetCodeCoverage() {}
    void DumpCodeCoverage () {}

    //--//

    void AddMemoryRange( CLR_UINT32 base, CLR_UINT32 length, void* target, LoadHandlerFtn load, StoreHandlerFtn store );

    template <typename T> void AddMemoryRange( CLR_UINT32 base, T& target, LoadHandlerFtn load, StoreHandlerFtn store )
    {
        AddMemoryRange( base, (CLR_UINT32)sizeof(T), &target, load, store );
    }

    //--//--//

private:
    bool LoadImpl ( CLR_UINT32 address, CLR_UINT32& value, CLR_DataType kind );
    bool StoreImpl( CLR_UINT32 address, CLR_UINT32  value, CLR_DataType kind );

    //--//

private:

#define INTEROP_HANDLER(xxx) bool Interop_##xxx()

    INTEROP_HANDLER( ApplicationEntryPoint );

    INTEROP_HANDLER( VTE_USART_Write                     );
    INTEROP_HANDLER( SUPPORT_ComputeCRC                  );
    INTEROP_HANDLER( SUPPORT_StubForARMEmulatorInterface );

    INTEROP_HANDLER( VTE_I28F320W18_32_ChipInitialize );
    INTEROP_HANDLER( VTE_I28F320W18_32_ChipReadOnly   );
    INTEROP_HANDLER( VTE_I28F320W18_32_EraseBlock     );
    INTEROP_HANDLER( VTE_I28F320W18_32_EraseSector    );
    INTEROP_HANDLER( VTE_I28F320W18_32_ReadProductID  );
    INTEROP_HANDLER( VTE_I28F320W18_32_WriteWord      );

    INTEROP_HANDLER( WaitForInterrupts );

    INTEROP_HANDLER( VTE_GPIO_BUTTON_GetNextStateChange );

    INTEROP_HANDLER( VTE_CHARGER_DUALSTATUS_Status );

    INTEROP_HANDLER( Events_Set          );
    INTEROP_HANDLER( Events_Get          );
    INTEROP_HANDLER( Events_Clear        );
    INTEROP_HANDLER( Events_MaskedRead   );
    INTEROP_HANDLER( Events_SetBoolTimer );

    INTEROP_HANDLER( Time_PerformanceCounter );

    INTEROP_HANDLER( RegisterFunction__12CodeCoverageSFPUiPCc );

    INTEROP_HANDLER( TraceModeFull   );
    INTEROP_HANDLER( TraceModeRemove );

#undef INTEROP_HANDLER

    //--//

#define MEMORYRANGE_HANDLER(xxx) \
    bool Handler_##xxx##_Load ( void* target, CLR_UINT32& value, CLR_DataType kind );\
    bool Handler_##xxx##_Store( void* target, CLR_UINT32  value, CLR_DataType kind )

    MEMORYRANGE_HANDLER(LCD);

    MEMORYRANGE_HANDLER(BUSWATCHER  );
    MEMORYRANGE_HANDLER(EBIU        );
    MEMORYRANGE_HANDLER(DMAC        );
    MEMORYRANGE_HANDLER(VITERBI     );
    MEMORYRANGE_HANDLER(FILTERARCTAN);
    MEMORYRANGE_HANDLER(INTC        );
    MEMORYRANGE_HANDLER(REMAP_PAUSE );
    MEMORYRANGE_HANDLER(ARMTIMER0   );
    MEMORYRANGE_HANDLER(ARMTIMER1   );
    MEMORYRANGE_HANDLER(VTU32       );
    MEMORYRANGE_HANDLER(USART0      );
    MEMORYRANGE_HANDLER(USART1      );
    MEMORYRANGE_HANDLER(USB         );
    MEMORYRANGE_HANDLER(GPIO        );
    MEMORYRANGE_HANDLER(SECURITYKEY );
    MEMORYRANGE_HANDLER(MWSPI       );
    MEMORYRANGE_HANDLER(CMU         );
    MEMORYRANGE_HANDLER(RTC         );
    MEMORYRANGE_HANDLER(EDMAIF      );
    MEMORYRANGE_HANDLER(PCU         );

#undef MEMORYRANGE_HANDLER
};

////////////////////////////////////////////////////////////////////////////////////////////////////

class CLR_RT_ArmEmulator : public ArmEmulator
{
    bool                      m_fInitialized;

    CLR_RT_SymbolToAddressMap m_lookup_symdef;
    CLR_RT_AddressToSymbolMap m_lookup_symdef_Inverse;

    CLR_UINT32                m_exitTrigger;

    JitterExternalCalls m_orig_ExternalCalls;
    JitterExternalCalls m_stub_ExternalCalls;

    //--//--//

public:
    CLR_RT_ArmEmulator();

    HRESULT Execute( CLR_RT_StackFrame* stack );

    void InitializeExternalCalls();

    //--//--//

private:

#define INTEROP_HANDLER(xxx) bool Interop_##xxx()

    INTEROP_HANDLER(ExitTrigger);

    //--//

    INTEROP_HANDLER(CLR_RT_HeapBlock__Compare_Values         );

    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericMul             );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericDiv             );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericDivUn           );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericRem             );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericRemUn           );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericShl             );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericShr             );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericShrUn           );
    INTEROP_HANDLER(CLR_RT_HeapBlock__InitObject             );
    INTEROP_HANDLER(CLR_RT_HeapBlock__Convert                );

    INTEROP_HANDLER(MethodCompilerHelpers__HandleBoxing      );
    INTEROP_HANDLER(MethodCompilerHelpers__HandleCasting     );
    INTEROP_HANDLER(MethodCompilerHelpers__CopyValueType     );
    INTEROP_HANDLER(MethodCompilerHelpers__CloneValueType    );
    INTEROP_HANDLER(MethodCompilerHelpers__LoadFunction      );
    INTEROP_HANDLER(MethodCompilerHelpers__LoadString        );
    INTEROP_HANDLER(MethodCompilerHelpers__NewArray          );

    INTEROP_HANDLER(MethodCompilerHelpers__Call              );
    INTEROP_HANDLER(MethodCompilerHelpers__CallVirtual       );
    INTEROP_HANDLER(MethodCompilerHelpers__NewObject         );
    INTEROP_HANDLER(MethodCompilerHelpers__NewDelegate       );
    INTEROP_HANDLER(MethodCompilerHelpers__Pop               );

    INTEROP_HANDLER(MethodCompilerHelpers__AccessStaticField );

    INTEROP_HANDLER(MethodCompilerHelpers__Throw             );
    INTEROP_HANDLER(MethodCompilerHelpers__Rethrow           );
    INTEROP_HANDLER(MethodCompilerHelpers__Leave             );
    INTEROP_HANDLER(MethodCompilerHelpers__EndFinally        );

    INTEROP_HANDLER(MethodCompilerHelpers__LoadIndirect      );
    INTEROP_HANDLER(MethodCompilerHelpers__StoreIndirect     );

    INTEROP_HANDLER(MethodCompilerHelpers__LoadObject        );
    INTEROP_HANDLER(MethodCompilerHelpers__CopyObject        );
    INTEROP_HANDLER(MethodCompilerHelpers__StoreObject       );

#undef INTEROP_HANDLER
};

extern CLR_RT_ArmEmulator g_CLR_RT_ArmEmulator;

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif //  _TINYCLR_JITTER_ARMEMULATION_H_
