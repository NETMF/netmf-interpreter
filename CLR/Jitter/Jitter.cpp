////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core\Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

//--//

#if defined(TINYCLR_DUMP_JITTER_INLINE)

#define DUMP_JITTERINLINE( arg ) if(m_fDump_JitterInline) { arg; }

#else

#define DUMP_JITTERINLINE( arg )

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER_STATISTICS)

MethodCompiler::Statistics MethodCompiler::s_statistics;

void MethodCompiler::Statistics::Dump( MethodCompiler& mc, Statistics& tot )
{
    size_t i;

    m_size        += (CLR_UINT32)(mc.m_Arm_Opcodes.Size() * sizeof(CLR_UINT32));

    tot.m_methods += 1;
    tot.m_size    += m_size;

    tot.m_exceptionHandlers          += m_exceptionHandlers;
    tot.m_localVariables             += m_localVariables;
    tot.m_methodsWithValueTypeLocals += m_methodsWithValueTypeLocals;

    for(i=0; i<ARRAYSIZE(m_opcodes); i++)
    {
        tot.m_opcodes[ i ] += m_opcodes[ i ];
    }

    //--//

    if(s_CLR_RT_fJitter_Trace_Statistics >= c_CLR_RT_Trace_Verbose)
    {
        CLR_RT_DUMP::METHOD( mc.m_mdInst ); CLR_Debug::Printf( " = %d bytes\r\n", m_size );

        if(s_CLR_RT_fJitter_Trace_Statistics >= c_CLR_RT_Trace_Annoying)
        {
            CLR_Debug::Printf( "EH   : %d\r\n"     , m_exceptionHandlers                            );
            CLR_Debug::Printf( "VARS : %d(%d)\r\n" , m_localVariables, m_methodsWithValueTypeLocals );

            if(s_CLR_RT_fJitter_Trace_Statistics >= c_CLR_RT_Trace_Obnoxious)
            {
                for(size_t i=0; i<ARRAYSIZE(m_opcodes); i++)
                {
                    if(m_opcodes[ i ])
                    {
                        CLR_Debug::Printf( "  %-25s %3d\r\n", c_CLR_RT_LogicalOpcodeLookup[ i ].Name(), m_opcodes[ i ] );
                    }
                }
            }
        }

        CLR_Debug::Printf( "\r\n" );
    }
}

void MethodCompiler::Statistics::Dump()
{
    if(s_CLR_RT_fJitter_Trace_Statistics >= c_CLR_RT_Trace_Info && m_methods)
    {
        CLR_Debug::Printf( "TOTAL SIZE : %d for %d methods\r\n", m_size, m_methods                    );
        CLR_Debug::Printf( "TOTAL EH   : %d\r\n"     , m_exceptionHandlers                            );
        CLR_Debug::Printf( "TOTAL VARS : %d(%d)\r\n" , m_localVariables, m_methodsWithValueTypeLocals );

        for(size_t i=0; i<ARRAYSIZE(m_opcodes); i++)
        {
            CLR_Debug::Printf( "  %-25s %6d\r\n", c_CLR_RT_LogicalOpcodeLookup[ i ].Name(), m_opcodes[ i ] );
        }

        CLR_Debug::Printf( "\r\n" );
    }
}

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

JitterThunkTable g_thunkTable;

JitterExternalCalls s_JitterExternalCalls =
{
    &CLR_RT_HeapBlock     ::Compare_Values   , // CLR_INT32         (                   *m_fpn__CLR_RT_HeapBlock__Compare_Values        )( const CLR_RT_HeapBlock& left, const CLR_RT_HeapBlock& right, bool fSigned                            );

    &CLR_RT_HeapBlock     ::NumericMul       , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericMul            )( const CLR_RT_HeapBlock& right                                                                        );
    &CLR_RT_HeapBlock     ::NumericDiv       , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericDiv            )( const CLR_RT_HeapBlock& right                                                                        );
    &CLR_RT_HeapBlock     ::NumericDivUn     , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericDivUn          )( const CLR_RT_HeapBlock& right                                                                        );
    &CLR_RT_HeapBlock     ::NumericRem       , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericRem            )( const CLR_RT_HeapBlock& right                                                                        );
    &CLR_RT_HeapBlock     ::NumericRemUn     , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericRemUn          )( const CLR_RT_HeapBlock& right                                                                        );
    &CLR_RT_HeapBlock     ::NumericShl       , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericShl            )( const CLR_RT_HeapBlock& right                                                                        );
    &CLR_RT_HeapBlock     ::NumericShr       , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericShr            )( const CLR_RT_HeapBlock& right                                                                        );
    &CLR_RT_HeapBlock     ::NumericShrUn     , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__NumericShrUn          )( const CLR_RT_HeapBlock& right                                                                        );
    &CLR_RT_HeapBlock     ::InitObject       , // bool              (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__InitObject            )(                                                                                                      );
    &CLR_RT_HeapBlock     ::Convert          , // HRESULT           (CLR_RT_HeapBlock ::*m_fpn__CLR_RT_HeapBlock__Convert               )( CLR_DataType et, bool fOverflow, bool fUnsigned                                                      );

    &MethodCompilerHelpers::HandleBoxing     , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__HandleBoxing     )( CLR_RT_HeapBlock& ref, CLR_UINT32 type, bool fBox                                                    );
    &MethodCompilerHelpers::HandleCasting    , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__HandleCasting    )( CLR_RT_HeapBlock& ref, CLR_UINT32 type, CLR_UINT32 levels, bool fIsInst                              );
    &MethodCompilerHelpers::CopyValueType    , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__CopyValueType    )( CLR_RT_HeapBlock& refDst, const CLR_RT_HeapBlock& refSrc                                             );
    &MethodCompilerHelpers::CloneValueType   , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__CloneValueType   )( CLR_RT_HeapBlock& ref                                                                                );
    &MethodCompilerHelpers::LoadFunction     , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__LoadFunction     )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock& refDlg, CLR_RT_HeapBlock* ptrInstance );
    &MethodCompilerHelpers::LoadString       , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__LoadString       )( CLR_RT_StackFrame* stack, CLR_UINT32 string, CLR_RT_HeapBlock& refStr                                );
    &MethodCompilerHelpers::NewArray         , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__NewArray         )( CLR_RT_StackFrame* stack, CLR_UINT32 type, CLR_UINT32 levels, CLR_RT_HeapBlock& ref                  );

    &MethodCompilerHelpers::Call             , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__Call             )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument                         );
    &MethodCompilerHelpers::CallVirtual      , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__CallVirtual      )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument                         );
    &MethodCompilerHelpers::NewObject        , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__NewObject        )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument                         );
    &MethodCompilerHelpers::NewDelegate      , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__NewDelegate      )( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument                         );
    &MethodCompilerHelpers::Pop              , // void              (                   *m_fpn__MethodCompilerHelpers__Pop              )( CLR_RT_StackFrame* stack                                                                             );

    &MethodCompilerHelpers::AccessStaticField, // CLR_RT_HeapBlock* (                   *m_fpn__MethodCompilerHelpers__AccessStaticField)( CLR_UINT32 field                                                                                     );

    &MethodCompilerHelpers::Throw            , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__Throw            )( CLR_RT_StackFrame* stack, CLR_RT_HeapBlock* ex                                                       );
    &MethodCompilerHelpers::Rethrow          , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__Rethrow          )( CLR_RT_StackFrame* stack                                                                             );
    &MethodCompilerHelpers::Leave            , // CLR_PMETADATA     (                   *m_fpn__MethodCompilerHelpers__Leave            )( CLR_RT_StackFrame* stack, CLR_PMETADATA from , CLR_PMETADATA to                                      );
    &MethodCompilerHelpers::EndFinally       , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__EndFinally       )( CLR_RT_StackFrame* stack                                                                             );

    &MethodCompilerHelpers::LoadIndirect     , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__LoadIndirect     )( CLR_RT_HeapBlock& ref                                                                                );
    &MethodCompilerHelpers::StoreIndirect    , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__StoreIndirect    )( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refVal, , CLR_UINT32 op                                  );

    &MethodCompilerHelpers::LoadObject       , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__LoadObject       )( CLR_RT_HeapBlock& ref                                                                                );
    &MethodCompilerHelpers::CopyObject       , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__CopyObject       )( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refSrc                                                   );
    &MethodCompilerHelpers::StoreObject      , // HRESULT           (                   *m_fpn__MethodCompilerHelpers__StoreObject      )( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refVal                                                   );

};

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MethodCompiler::Initialize( const CLR_RT_MethodDef_Index* md, CLR_UINT32 baseAddress )
{
    TINYCLR_HEADER();

    memset( this, 0, sizeof(*this) );

                                     // CLR_RT_MethodDef_Instance    m_mdInst;
                                     // CLR_RT_TypeDef_Instance      m_clsInst;
                                     //
                                     // TypeDescriptor               m_returnValue;
    m_arguments       .Initialize(); // TypedArray<TypeDescriptor>   m_arguments;
    m_locals          .Initialize(); // TypedArray<TypeDescriptor>   m_locals;
                                     //
                                     // //--//
                                     //
                                     // CLR_PMETADATA                m_ipStart;
                                     // CLR_PMETADATA                m_ipEnd;
                                     //
                                     // size_t                       m_numOpcodes;
    m_opcodeSlots     .Initialize(); // TypedArray<OpcodeSlot>       m_opcodeSlots;
                                     //
                                     // size_t                       m_numBranches;
    m_opcodeBranches  .Initialize(); // TypedArray<CLR_OFFSET>       m_opcodeBranches;
                                     //
    m_EHs             .Initialize(); // TypedArray<CLR_RECORD_EH>    m_EHs;
                                     //
                                     // //--//
                                     //
    m_stackTypes      .Initialize(); // TypedQueue<TypeDescriptor>   m_stackTypes;
    m_stackStatus     .Initialize(); // TypedQueue<TypeDescriptor>   m_stackStatus;
                                     //
                                     // //--//
                                     //
    m_indexToArmOpcode.Initialize(); // TypedArray<CLR_UINT32>       m_indexToArmOpcode;
                                     //
                                     // //--//
                                     //
                                     // JitterThunkTable*            m_Arm_Thunks;
                                     //
    m_Arm_BaseAddress = baseAddress; // CLR_UINT32                   m_Arm_BaseAddress;
                                     //
    m_Arm_Opcodes     .Initialize(); // TypedQueue<CLR_UINT32>       m_Arm_Opcodes;
    m_Arm_ROData      .Initialize(); // TypedQueue<CLR_UINT32>       m_Arm_ROData;
                                     // CLR_UINT32                   m_Arm_ROData_Base;
                                     // ArmProcessor::Opcode         m_Arm_Op;
                                     // bool                         m_Arm_shiftUseReg;
                                     // CLR_UINT32                   m_Arm_shiftType;
                                     // CLR_UINT32                   m_Arm_shiftValue;
                                     // CLR_UINT32                   m_Arm_shiftReg;
                                     // bool                         m_Arm_setCC;
                                     //
                                     // #if defined(TINYCLR_DUMP_JITTER_INLINE)
                                     // bool                         m_fDump_JitterInline;
                                     // #endif
                                     //

    //--//

    if(md)
    {
        if(m_mdInst.InitializeFromIndex( *md ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_TOKEN__METHOD);
        }

        if(m_clsInst.InitializeFromMethod( m_mdInst ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_TOKEN__METHOD);
        }
    }

    TINYCLR_NOCLEANUP();
}

void MethodCompiler::Release()
{
    m_arguments       .Release();
    m_locals          .Release();

    m_opcodeSlots     .Release();
    m_opcodeBranches  .Release();
    m_EHs             .Release();

    m_stackTypes      .Release();
    m_stackStatus     .Release();

    m_indexToArmOpcode.Release();

    m_Arm_Opcodes     .Release();
    m_Arm_ROData      .Release();
}

//--//

HRESULT MethodCompiler::CreateThunks( JitterThunkTable* tbl )
{
    TINYCLR_HEADER();

#if defined(TINYCLR_DUMP_JITTER_INLINE)
    m_fDump_JitterInline = (s_CLR_RT_fJitter_Trace_Compile >= c_CLR_RT_Trace_Annoying);
#endif

    DUMP_JITTERINLINE( CLR_Debug::Printf( "\r\n%*s THUNKS\r\n", 76, " " ) );

    //--//

#define POPFIELDS                     \
    ArmProcessor::c_register_lst_r5 | \
    ArmProcessor::c_register_lst_r6 | \
    ArmProcessor::c_register_lst_r7 | \
    ArmProcessor::c_register_lst_r8 | \
    ArmProcessor::c_register_lst_pc
   

    //--//

#define DECLARE_THUNK(thunk)                                                               \
                                                                                           \
    DUMP_JITTERINLINE( CLR_Debug::Printf( "\r\n%*s Thunk for %s\r\n", 76, " ", #thunk ) ); \
                                                                                           \
    tbl->m_address__##thunk = Arm_CurrentAbsolutePC()


#define DECLARE_THUNK_LONGBRANCH(thunk,address)                                        \
                                                                                       \
    DUMP_JITTERINLINE( CLR_Debug::Printf( "%*s Thunk for %s\r\n", 76, " ", #thunk ) ); \
                                                                                       \
    tbl->m_address__##thunk = Arm_CurrentAbsolutePC();                                 \
                                                                                       \
    TINYCLR_CHECK_HRESULT(Arm_LongBranch( address, false ))


#define DECLARE_THUNK_LONGBRANCH_HRESULT(thunk,address)                                                                                                                          \
                                                                                                                                                                                 \
    DUMP_JITTERINLINE( CLR_Debug::Printf( "\r\n%*s Thunk for %s\r\n", 76, " ", #thunk ) );                                                                                       \
                                                                                                                                                                                 \
    tbl->m_address__##thunk = Arm_CurrentAbsolutePC();                                                                                                                           \
                                                                                                                                                                                 \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_STR       ( ArmProcessor::c_register_lr, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_IP) )); \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LongBranch( address, true                                                                              )); \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_TST_IMM   ( ArmProcessor::c_register_r0, 0x80000000                                                    )); \
    Arm_SetCond( ArmProcessor::c_cond_NE ); TINYCLR_CHECK_HRESULT(Arm_LDMFD_SP  ( c_poplist                                                                                  )); \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR       ( ArmProcessor::c_register_pc, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_IP) ))


    //
    // If the new StackFrame is marked as "CallerIsCompatibleForCall", there's no need to return to the caller,
    // just load the data for the new method and start executing from there.
    //

#define DECLARE_THUNK_LONGBRANCH_HRESULT2(thunk,address)                                                                                                                                              \
                                                                                                                                                                                                      \
    DUMP_JITTERINLINE( CLR_Debug::Printf( "\r\n%*s Thunk for %s\r\n", 76, " ", #thunk ) );                                                                                                            \
                                                                                                                                                                                                      \
    tbl->m_address__##thunk = Arm_CurrentAbsolutePC();                                                                                                                                                \
                                                                                                                                                                                                      \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_STR       ( ArmProcessor::c_register_lr, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_IP)                      )); \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LongBranch( address, true                                                                                                   )); \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_CMP_IMM   ( ArmProcessor::c_register_r0, 0x0                                                                                )); \
    Arm_SetCond( ArmProcessor::c_cond_NE ); TINYCLR_CHECK_HRESULT(Arm_LDMFD_SP  ( c_poplist                                                                                                       )); \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR       ( ArmProcessor::c_register_r4, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_data.nodeLink.nextBlock) )); \
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDMIA     ( ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_owningThread), POPFIELDS, ArmProcessor::c_register_r0 ))


#define DECLARE_THUNK_LONGBRANCH_NOLINK(thunk,address)                                                                                   \
                                                                                                                                         \
    DUMP_JITTERINLINE( CLR_Debug::Printf( "\r\n%*s Thunk for %s\r\n", 76, " ", #thunk ) );                                               \
                                                                                                                                         \
    tbl->m_address__##thunk = Arm_CurrentAbsolutePC();                                                                                   \
                                                                                                                                         \
    TINYCLR_CHECK_HRESULT(Arm_SUB_IMM   ( ArmProcessor::c_register_lr, ArmProcessor::c_register_lr, 4                                )); \
    TINYCLR_CHECK_HRESULT(Arm_STR       ( ArmProcessor::c_register_lr, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_IP) )); \
    TINYCLR_CHECK_HRESULT(Arm_LDMFD_SP  ( c_pushlist                                                                                 )); \
    TINYCLR_CHECK_HRESULT(Arm_LongBranch( address, false                                                                             ))

    //--//

    DECLARE_THUNK(Internal_Initialize);

    Arm_SetCond( ArmProcessor::c_cond_AL );

    TINYCLR_CHECK_HRESULT(Arm_STMFD_SP( c_pushlist                                                                                                      ));
    TINYCLR_CHECK_HRESULT(Arm_MOV     ( ArmProcessor::c_register_r4, ArmProcessor::c_register_r0                                                        ));
    TINYCLR_CHECK_HRESULT(Arm_LDMIA   ( ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_owningThread), POPFIELDS, ArmProcessor::c_register_r0 ));

    //--//

    DECLARE_THUNK(Internal_ReturnFromMethod);

    //
    // If the current StackFrame is marked as "CallerIsCompatibleForRet", there's no need to return to the caller,
    // just load the data for the new method and start executing from there.
    //

    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR       ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_flags)                   ));
    Arm_SetCC();                            TINYCLR_CHECK_HRESULT(Arm_AND_IMM   ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, CLR_RT_StackFrame::c_CallerIsCompatibleForRet         ));
    Arm_SetCond( ArmProcessor::c_cond_EQ ); TINYCLR_CHECK_HRESULT(Arm_LDMFD_SP  ( c_poplist                                                                                                       ));
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_MOV       ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4                                                        ));
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR       ( ArmProcessor::c_register_r4, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_data.nodeLink.prevBlock) ));
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LongBranch( JITTER_HELPER_FPN(0,MethodCompilerHelpers,Pop), true                                                            ));
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDMIA     ( ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_owningThread), POPFIELDS, ArmProcessor::c_register_r0 ));

    //--//

    DECLARE_THUNK(Internal_Restart);

    TINYCLR_CHECK_HRESULT(Arm_MOV_IMM ( ArmProcessor::c_register_r0, CLR_E_RESTART_EXECUTION                                                 ));

    DECLARE_THUNK(Internal_Error);

    TINYCLR_CHECK_HRESULT(Arm_ADD     ( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, ArmProcessor::c_register_r1                ));
    TINYCLR_CHECK_HRESULT(Arm_STR     ( ArmProcessor::c_register_r1, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_evalStackPos) ));

    DECLARE_THUNK(Internal_ErrorNoFlush);

    TINYCLR_CHECK_HRESULT(Arm_STR     ( ArmProcessor::c_register_lr, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_IP)           ));
    TINYCLR_CHECK_HRESULT(Arm_LDMFD_SP( c_poplist                                                                                            ));

    //--//

    DECLARE_THUNK_LONGBRANCH         ( CLR_RT_HeapBlock__Compare_Values        , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,Compare_Values    ) );

    DECLARE_THUNK_LONGBRANCH         ( CLR_RT_HeapBlock__NumericMul            , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,NumericMul        ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( CLR_RT_HeapBlock__NumericDiv            , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,NumericDiv        ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( CLR_RT_HeapBlock__NumericDivUn          , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,NumericDivUn      ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( CLR_RT_HeapBlock__NumericRem            , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,NumericRem        ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( CLR_RT_HeapBlock__NumericRemUn          , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,NumericRemUn      ) );
    DECLARE_THUNK_LONGBRANCH         ( CLR_RT_HeapBlock__NumericShl            , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,NumericShl        ) );
    DECLARE_THUNK_LONGBRANCH         ( CLR_RT_HeapBlock__NumericShr            , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,NumericShr        ) );
    DECLARE_THUNK_LONGBRANCH         ( CLR_RT_HeapBlock__NumericShrUn          , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,NumericShrUn      ) );
    DECLARE_THUNK_LONGBRANCH         ( CLR_RT_HeapBlock__InitObject            , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,InitObject        ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( CLR_RT_HeapBlock__Convert               , JITTER_HELPER_FPN(0,CLR_RT_HeapBlock     ,Convert           ) );

    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__HandleBoxing     , JITTER_HELPER_FPN(0,MethodCompilerHelpers,HandleBoxing      ) );
    DECLARE_THUNK_LONGBRANCH         ( MethodCompilerHelpers__HandleIsInst     , JITTER_HELPER_FPN(0,MethodCompilerHelpers,HandleCasting     ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__HandleCasting    , JITTER_HELPER_FPN(0,MethodCompilerHelpers,HandleCasting     ) );
    DECLARE_THUNK_LONGBRANCH         ( MethodCompilerHelpers__CopyValueType    , JITTER_HELPER_FPN(0,MethodCompilerHelpers,CopyValueType     ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__CloneValueType   , JITTER_HELPER_FPN(0,MethodCompilerHelpers,CloneValueType    ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__LoadFunction     , JITTER_HELPER_FPN(0,MethodCompilerHelpers,LoadFunction      ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__LoadString       , JITTER_HELPER_FPN(0,MethodCompilerHelpers,LoadString        ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__NewArray         , JITTER_HELPER_FPN(0,MethodCompilerHelpers,NewArray          ) );

    DECLARE_THUNK_LONGBRANCH_HRESULT2( MethodCompilerHelpers__Call             , JITTER_HELPER_FPN(0,MethodCompilerHelpers,Call              ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT2( MethodCompilerHelpers__CallVirtual      , JITTER_HELPER_FPN(0,MethodCompilerHelpers,CallVirtual       ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT2( MethodCompilerHelpers__NewObject        , JITTER_HELPER_FPN(0,MethodCompilerHelpers,NewObject         ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__NewDelegate      , JITTER_HELPER_FPN(0,MethodCompilerHelpers,NewDelegate       ) );

    DECLARE_THUNK_LONGBRANCH         ( MethodCompilerHelpers__AccessStaticField, JITTER_HELPER_FPN(0,MethodCompilerHelpers,AccessStaticField ) );

    DECLARE_THUNK_LONGBRANCH_NOLINK  ( MethodCompilerHelpers__Throw            , JITTER_HELPER_FPN(0,MethodCompilerHelpers,Throw             ) );
    DECLARE_THUNK_LONGBRANCH_NOLINK  ( MethodCompilerHelpers__Rethrow          , JITTER_HELPER_FPN(0,MethodCompilerHelpers,Rethrow           ) );
    DECLARE_THUNK_LONGBRANCH         ( MethodCompilerHelpers__Leave            , JITTER_HELPER_FPN(0,MethodCompilerHelpers,Leave             ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__EndFinally       , JITTER_HELPER_FPN(0,MethodCompilerHelpers,EndFinally        ) );

    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__LoadIndirect     , JITTER_HELPER_FPN(0,MethodCompilerHelpers,LoadIndirect      ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__StoreIndirect    , JITTER_HELPER_FPN(0,MethodCompilerHelpers,StoreIndirect     ) );

    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__LoadObject       , JITTER_HELPER_FPN(0,MethodCompilerHelpers,LoadObject        ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__CopyObject       , JITTER_HELPER_FPN(0,MethodCompilerHelpers,CopyObject        ) );
    DECLARE_THUNK_LONGBRANCH_HRESULT ( MethodCompilerHelpers__StoreObject      , JITTER_HELPER_FPN(0,MethodCompilerHelpers,StoreObject       ) );

    //--//

#undef DECLARE_THUNK
#undef DECLARE_THUNK_LONGBRANCH
#undef DECLARE_THUNK_LONGBRANCH_NOLINK
#undef DECLARE_THUNK_LONGBRANCH_HRESULT
#undef DECLARE_THUNK_LONGBRANCH_HRESULT2

#undef POPFIELDS

    //--//

    TINYCLR_NOCLEANUP();
}

void MethodCompiler::ReferToThunks( JitterThunkTable* tbl )
{
    m_Arm_Thunks = tbl;
}

//--//

HRESULT MethodCompiler::OffsetToIndex( CLR_OFFSET& idx, CLR_UINT32 offset )
{
    TINYCLR_HEADER();

    OpcodeSlot* osPtr = m_opcodeSlots;
    size_t      left  = 0;
    size_t      right = m_numOpcodes+1; // Include the extra opcode slot, used to find the end of a method.

    while(left < right)
    {
        size_t      center = (left + right) / 2;
        OpcodeSlot& os     = osPtr[ center ];

        if(offset < os.m_ipOffset)
        {
            right = center;
        }
        else if(offset > os.m_ipOffset)
        {
            left = center+1;
        }
        else
        {
            idx = (CLR_OFFSET)center;

            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }

    idx = CLR_EmptyIndex;

    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_OFFSET);

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::ParseByteCode()
{
    TINYCLR_HEADER();

    CLR_RT_Assembly*            assm     = m_mdInst.m_assm;
    const CLR_RECORD_METHODDEF* mdTarget = m_mdInst.m_target;
    CLR_OFFSET                  offsetStart;
    CLR_OFFSET                  offsetEnd;

    if(assm->FindMethodBoundaries( m_mdInst.Method(), offsetStart, offsetEnd ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
    else
    {
        CLR_PMETADATA        ip;
        const CLR_RECORD_EH* ptrEh       = NULL;
        CLR_UINT32           numEh       = 0;
        CLR_UINT32           numOpcodes  = 0;
        CLR_UINT32           numBranches = 0;

        m_ipStart = assm->GetByteCode( mdTarget->RVA );
        m_ipEnd   = m_ipStart + (offsetEnd - offsetStart);

        if(mdTarget->flags & CLR_RECORD_METHODDEF::MD_HasExceptionHandlers)
        {
            m_ipEnd = CLR_RECORD_EH::ExtractEhFromByteCode( m_ipEnd, ptrEh, numEh );
        }

        //
        // Count number of opcodes and branches.
        //
        for(ip = m_ipStart; ip < m_ipEnd; )
        {
            CLR_OPCODE                 op = CLR_ReadNextOpcodeCompressed( ip );
            const CLR_RT_OpcodeLookup& ol = c_CLR_RT_OpcodeLookup[ op ];

            if(op >= CEE_COUNT || ol.m_logicalOpcode == LO_Unsupported)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
            }

            switch(ol.m_opParam)
            {
            case CLR_OpcodeParam_BrTarget     :
            case CLR_OpcodeParam_ShortBrTarget:
                numBranches++;
                break;

            case CLR_OpcodeParam_Switch:
                {
                    FETCH_ARG_UINT8(arg,ip); ip--;

                    numBranches += arg;
                }
                break;
            }

            numOpcodes++;

            ip = CLR_SkipBodyOfOpcodeCompressed( ip, op );
        }

        if(ip != m_ipEnd)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_OFFSET);
        }

        m_numOpcodes  = numOpcodes;
        m_numBranches = numBranches;

        TINYCLR_CHECK_HRESULT(m_opcodeSlots   .Allocate( numOpcodes+1  ));
        TINYCLR_CHECK_HRESULT(m_opcodeBranches.Allocate( numBranches*2 ));
        TINYCLR_CHECK_HRESULT(m_EHs           .Allocate( numEh         ));

        //
        // Initialize OpcodeSlots.
        //
        {
            OpcodeSlot* osPtr     = m_opcodeSlots;
            CLR_OFFSET  ipOffset  = 0;
            CLR_OFFSET  brOffset  = 0;
            CLR_UINT32  flagsNext = 0;

            for(ip = m_ipStart; ip < m_ipEnd; )
            {
                Opcode     op;
                CLR_UINT32 flags = flagsNext; flagsNext = 0;

                TINYCLR_CHECK_HRESULT(op.Initialize( this, ipOffset, &m_opcodeBranches[ brOffset ] ));

                //
                // Too many OUT branches
                //
                if(op.m_numTargets >= 255)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_OFFSET);
                }

                //--//

                osPtr->m_op                 = op.m_op;
                osPtr->m_ipOffset           = ipOffset;

                osPtr->m_flags              = flags;

                osPtr->m_stackPop           = op.m_stackPop;
                osPtr->m_stackPush          = op.m_stackPush;
                osPtr->m_stackDepth         = -1;
                osPtr->m_stackIdx           = CLR_EmptyIndex;

                osPtr->m_branchesIn         = 0;
                osPtr->m_branchesInIdx      = CLR_EmptyIndex;

                osPtr->m_branchesOut        = op.m_numTargets;
                osPtr->m_branchesOutIdx     = op.m_numTargets ? brOffset : CLR_EmptyIndex;

                ip       += op.m_ipLength;
                ipOffset += op.m_ipLength;
                brOffset += op.m_numTargets;

                osPtr++;
            }

            if(brOffset != numBranches)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_OFFSET);
            }

            //
            // The extra slot is used to speed up the OffsetToIndex conversion.
            //
            osPtr->m_op       = CEE_COUNT;
            osPtr->m_ipOffset = ipOffset;
        }

        //
        // Convert branches from offset to index.
        //
        {
            CLR_OFFSET* ptr = m_opcodeBranches;
            size_t      num = numBranches;

            while(num--)
            {
                CLR_OFFSET& idx = *ptr++;

                TINYCLR_CHECK_HRESULT(OffsetToIndex( idx, idx ));

                OpcodeSlot& os = m_opcodeSlots[ idx ];

                //
                // Too many IN branches
                //
                if(os.m_branchesIn >= 255)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_OFFSET);
                }

                os.m_branchesIn++;
            }
        }

        //
        // Convert OUT branches into IN branches.
        //
        {
            OpcodeSlot* osDst    = m_opcodeSlots;
            CLR_OFFSET  brOffset = numBranches;
            CLR_OFFSET* dst      = &m_opcodeBranches[ brOffset ];

            for(size_t posDst = 0; posDst < m_numOpcodes; posDst++, osDst++)
            {
                if(osDst->m_branchesIn)
                {
                    osDst->m_branchesInIdx = brOffset;

                    OpcodeSlot* osSrc  = m_opcodeSlots;

                    for(size_t posSrc = 0; posSrc < m_numOpcodes; posSrc++, osSrc++)
                    {
                        size_t numSrc = osSrc->m_branchesOut;

                        if(numSrc)
                        {
                            CLR_OFFSET* src = &m_opcodeBranches[ osSrc->m_branchesOutIdx ];

                            while(numSrc--)
                            {
                                if(*src++ == posDst)
                                {
                                    *dst++ = (CLR_OFFSET)posSrc;

                                    if(posSrc < posDst)
                                    {
                                        osDst->m_flags |= OpcodeSlot::c_BranchForward;
                                    }
                                    else
                                    {
                                        osDst->m_flags |= OpcodeSlot::c_BranchBackward;
                                    }

                                    brOffset++;
                                }
                            }
                        }
                    }

                    if(brOffset - osDst->m_branchesInIdx != osDst->m_branchesIn)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_OFFSET);
                    }
                }
            }
        }

        //
        // Initialize exception handlers.
        //
        {
            CLR_RECORD_EH* ehPtr = m_EHs;

            while(numEh--)
            {
                memcpy( ehPtr, ptrEh++, sizeof(*ehPtr) );

                TINYCLR_CHECK_HRESULT(OffsetToIndex( ehPtr->handlerStart, ehPtr->handlerStart ));
                TINYCLR_CHECK_HRESULT(OffsetToIndex( ehPtr->handlerEnd  , ehPtr->handlerEnd   ));
                TINYCLR_CHECK_HRESULT(OffsetToIndex( ehPtr->tryStart    , ehPtr->tryStart     ));
                TINYCLR_CHECK_HRESULT(OffsetToIndex( ehPtr->tryEnd      , ehPtr->tryEnd       ));

                ehPtr++;
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::ParseArguments()
{
    TINYCLR_HEADER();

    CLR_UINT32 numArgs = m_mdInst.m_target->numArgs;

    if(numArgs)
    {
        TINYCLR_CHECK_HRESULT(m_arguments.Allocate( numArgs ));

        CLR_RT_SignatureParser parser; parser.Initialize_MethodSignature( m_mdInst.m_assm, m_mdInst.m_target );
        TypeDescriptor*        ptr = m_arguments;

        TINYCLR_CHECK_HRESULT(m_returnValue.Parse( parser ));

        if(parser.m_flags & PIMAGE_CEE_CS_CALLCONV_HASTHIS)
        {
            ptr->InitializeFromIndex( m_clsInst );

            if((m_clsInst.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_ValueType && (m_mdInst.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) == 0)
            {
                ptr->m_flags |= TypeDescriptor::c_Boxed;
            }

            ptr++;
            numArgs--;
        }

        while(numArgs--)
        {
            TINYCLR_CHECK_HRESULT(ptr->Parse( parser ));

            ptr++;
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::ParseLocals()
{
    TINYCLR_HEADER();

    CLR_UINT32 numLocals = m_mdInst.m_target->numLocals;

    if(numLocals)
    {
        TINYCLR_CHECK_HRESULT(m_locals.Allocate( numLocals ));

        CLR_RT_SignatureParser parser; parser.Initialize_MethodLocals( m_mdInst.m_assm, m_mdInst.m_target );
        TypeDescriptor*        ptr = m_locals;

        while(numLocals--)
        {
            TINYCLR_CHECK_HRESULT(ptr->Parse( parser ));

            ptr++;
        }
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompiler::GenerateCode()
{
    TINYCLR_HEADER();

    static const CLR_UINT32 c_pushlist = ArmProcessor::c_register_lst_r4 | ArmProcessor::c_register_lst_r5 | ArmProcessor::c_register_lst_r6 | ArmProcessor::c_register_lst_r7 | ArmProcessor::c_register_lst_r8 | ArmProcessor::c_register_lst_lr;
    static const CLR_UINT32 c_poplist  = ArmProcessor::c_register_lst_r4 | ArmProcessor::c_register_lst_r5 | ArmProcessor::c_register_lst_r6 | ArmProcessor::c_register_lst_r7 | ArmProcessor::c_register_lst_r8 | ArmProcessor::c_register_lst_pc;

    TINYCLR_JITTER_STATISTICS_EXECUTE( Statistics stats; TINYCLR_CLEAR(stats); );

    OpcodeSlot* osPtr;
    size_t      pos;

    TINYCLR_CHECK_HRESULT(m_indexToArmOpcode.Allocate( m_numOpcodes ));

#if defined(TINYCLR_JITTER_STATISTICS)
    {
        size_t num = m_locals.Length();

        stats.m_localVariables             = (CLR_UINT32)num;
        stats.m_methodsWithValueTypeLocals =             0;

        for(pos = 0; pos < num; pos++)
        {
            if(m_locals[ pos ].NeedsCloning())
            {
                stats.m_methodsWithValueTypeLocals = 1;
                break;
            }
        }

        for(pos = 0, osPtr = m_opcodeSlots; pos < m_numOpcodes; pos++, osPtr++)
        {
            stats.m_opcodes[ c_CLR_RT_OpcodeLookup[ osPtr->m_op ].m_logicalOpcode ]++;
        }

        stats.m_exceptionHandlers = (CLR_UINT32)m_EHs.Length();
    }
#endif

    for(size_t pass=0; pass<2; pass++)
    {
        m_Arm_Opcodes.Clear();

#if defined(TINYCLR_DUMP_JITTER_INLINE)
        m_fDump_JitterInline = (pass == 1) && (s_CLR_RT_fJitter_Trace_Compile >= c_CLR_RT_Trace_Annoying);
#endif

        //
        // R4 = StackFrame
        // R5 = Thread
        // R6 = EvalStack base
        // R7 = Arguments
        // R8 = Locals
        //

        //--//

#define CALL_THUNK(cls,method) TINYCLR_CHECK_HRESULT(Arm_B( ArmProcessor::c_Link, Arm_AbsoluteOffset( m_Arm_Thunks->m_address__##cls##__##method ) ))

        {
            size_t numEH = m_EHs.Length();

            if(numEH)
            {
                TINYCLR_CHECK_HRESULT(Arm_EmitData( (CLR_UINT32)numEH ));

                for(pos = 0; pos < numEH; pos++)
                {
                    CLR_RECORD_EH&          eh = m_EHs[ pos ];
                    CLR_RT_ExceptionHandler eh2;

                    if(eh2.ConvertFromEH( m_mdInst, NULL, &eh ) == false)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_TOKEN__TYPE);
                    }

                    eh2.m_tryStart     = (CLR_PMETADATA)(size_t)(m_Arm_BaseAddress + m_indexToArmOpcode[ eh.tryStart     ]);
                    eh2.m_tryEnd       = (CLR_PMETADATA)(size_t)(m_Arm_BaseAddress + m_indexToArmOpcode[ eh.tryEnd       ]);
                    eh2.m_handlerStart = (CLR_PMETADATA)(size_t)(m_Arm_BaseAddress + m_indexToArmOpcode[ eh.handlerStart ]);
                    eh2.m_handlerEnd   = (CLR_PMETADATA)(size_t)(m_Arm_BaseAddress + m_indexToArmOpcode[ eh.handlerEnd   ]);

                    {
                        CLR_UINT32* ptr = (CLR_UINT32*)&eh2;
                        size_t      len = sizeof(eh2);

                        while(len)
                        {
                            TINYCLR_CHECK_HRESULT(Arm_EmitData( *ptr++ ));

                            len -= sizeof(*ptr);
                        }
                    }
                }
            }
        }

        //
        // Implementation of individual bytecodes.
        //
        {
            for(pos = 0, osPtr = m_opcodeSlots; pos < m_numOpcodes; pos++, osPtr++)
            {
                const CLR_RT_OpcodeLookup& ol = c_CLR_RT_OpcodeLookup[ osPtr->m_op ];
                CLR_LOGICAL_OPCODE         lo = ol.m_logicalOpcode;
                TypeDescriptor*            tdStack;
                CLR_RT_TypeDescriptor      td;
                Opcode                     op;

                TINYCLR_CHECK_HRESULT(op.Initialize( this, osPtr->m_ipOffset, NULL ));

                DUMP_JITTERINLINE( CLR_Debug::Printf( "%*s", 76, " " ); DumpOpcode( pos ) );

                m_indexToArmOpcode[ pos ] = Arm_CurrentRelativePC();

                if(osPtr->m_flags & OpcodeSlot::c_BranchBackward)
                {
                    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR    ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r5, offsetof(CLR_RT_Thread,m_timeQuantumExpired) ));
                    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( ArmProcessor::c_register_r0, 0                                                                         ));
                    Arm_SetCond( ArmProcessor::c_cond_NE ); TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r1, Arm_GetEndOfEvalStack( osPtr )                                            ));
                    Arm_SetCond( ArmProcessor::c_cond_NE ); TINYCLR_CHECK_HRESULT(Arm_B      ( ArmProcessor::c_Link, Arm_AbsoluteOffset( m_Arm_Thunks->m_address__Internal_Restart )                  ));
                }

                if(osPtr->m_stackPop)
                {
                    tdStack = GetFirstOperand( osPtr );

                    tdStack->ConvertToTypeDescriptor( td );
                }
                else
                {
                    tdStack = NULL;

                    TINYCLR_CLEAR(td);
                }

                switch(lo)
                {
                case LO_Not:
                case LO_Neg:
                    //
                    // Unary operators.
                    //
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, offsetof(CLR_RT_HeapBlock,m_data.numeric) );

                        switch(td.GetDataType())
                        {
                        case DATATYPE_I4:
                        case DATATYPE_R4:
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));

                                switch(lo)
                                {
                                case LO_Not:
                                    TINYCLR_CHECK_HRESULT(Arm_MVN( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0 ));
                                    break;

                                case LO_Neg:
                                    TINYCLR_CHECK_HRESULT(Arm_RSB_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, 0 ));
                                    break;
                                }

                                TINYCLR_CHECK_HRESULT(Arm_STR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                            }
                            break;

                        case DATATYPE_I8:
                        case DATATYPE_R8:
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LDMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r12 ));

                                switch(lo)
                                {
                                case LO_Not:
                                    TINYCLR_CHECK_HRESULT(Arm_MVN( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0 ));
                                    TINYCLR_CHECK_HRESULT(Arm_MVN( ArmProcessor::c_register_r1, ArmProcessor::c_register_r1 ));
                                    break;

                                case LO_Neg:
                                    Arm_SetCC(); TINYCLR_CHECK_HRESULT(Arm_RSB_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, 0 ));
                                    /**********/ TINYCLR_CHECK_HRESULT(Arm_RSC_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r1, 0 ));
                                    break;
                                }

                                TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r12 ));
                            }
                            break;

                        default:
                            TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                        }
                    }
                    break;

                case LO_And:
                case LO_Or:
                case LO_Xor:
                case LO_Add:
                case LO_Sub:
                    //
                    // Binary operators.
                    //
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, offsetof(CLR_RT_HeapBlock,m_data.numeric) );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, offsetof(CLR_RT_HeapBlock,m_data.numeric) );

                        switch(td.GetDataType())
                        {
                        case DATATYPE_I4:
                        case DATATYPE_R4:
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                                TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));

                                switch(lo)
                                {
                                case LO_And:
                                    TINYCLR_CHECK_HRESULT(Arm_AND( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r1 ));
                                    break;

                                case LO_Or:
                                    TINYCLR_CHECK_HRESULT(Arm_ORR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r1 ));
                                    break;

                                case LO_Xor:
                                    TINYCLR_CHECK_HRESULT(Arm_EOR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r1 ));
                                    break;

                                case LO_Add:
                                    TINYCLR_CHECK_HRESULT(Arm_ADD( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r1 ));
                                    break;

                                case LO_Sub:
                                    TINYCLR_CHECK_HRESULT(Arm_SUB( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r1 ));
                                    break;
                                }

                                TINYCLR_CHECK_HRESULT(Arm_STR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                            }
                            break;

                        case DATATYPE_I8:
                        case DATATYPE_R8:
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LDMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r12 ));
                                TINYCLR_CHECK_HRESULT(Arm_LDMIA( ArmProcessor::c_register_r6, op2, ArmProcessor::c_register_lst_r2 | ArmProcessor::c_register_lst_r3, ArmProcessor::c_register_r2  ));

                                switch(lo)
                                {
                                case LO_And:
                                    TINYCLR_CHECK_HRESULT(Arm_AND( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r2 ));
                                    TINYCLR_CHECK_HRESULT(Arm_AND( ArmProcessor::c_register_r1, ArmProcessor::c_register_r1, ArmProcessor::c_register_r3 ));
                                    break;

                                case LO_Or:
                                    TINYCLR_CHECK_HRESULT(Arm_ORR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r2 ));
                                    TINYCLR_CHECK_HRESULT(Arm_ORR( ArmProcessor::c_register_r1, ArmProcessor::c_register_r1, ArmProcessor::c_register_r3 ));
                                    break;

                                case LO_Xor:
                                    TINYCLR_CHECK_HRESULT(Arm_EOR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r2 ));
                                    TINYCLR_CHECK_HRESULT(Arm_EOR( ArmProcessor::c_register_r1, ArmProcessor::c_register_r1, ArmProcessor::c_register_r3 ));
                                    break;

                                case LO_Add:
                                    Arm_SetCC(); TINYCLR_CHECK_HRESULT(Arm_ADD( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r2 ));
                                    /**********/ TINYCLR_CHECK_HRESULT(Arm_ADC( ArmProcessor::c_register_r1, ArmProcessor::c_register_r1, ArmProcessor::c_register_r3 ));
                                    break;

                                case LO_Sub:
                                    Arm_SetCC(); TINYCLR_CHECK_HRESULT(Arm_SUB( ArmProcessor::c_register_r0, ArmProcessor::c_register_r0, ArmProcessor::c_register_r2 ));
                                    /**********/ TINYCLR_CHECK_HRESULT(Arm_SBC( ArmProcessor::c_register_r1, ArmProcessor::c_register_r1, ArmProcessor::c_register_r3 ));
                                    break;
                                }

                                TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r12 ));
                            }
                            break;

                        default:
                            TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                        }
                    }
                    break;

                case LO_Shl:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));

                        CALL_THUNK(CLR_RT_HeapBlock,NumericShl);
                    }
                    break;

                case LO_Shr:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));

                        if(ol.m_flags & CLR_RT_OpcodeLookup::COND_UNSIGNED)
                        {
                            CALL_THUNK(CLR_RT_HeapBlock,NumericShrUn);
                        }
                        else
                        {
                            CALL_THUNK(CLR_RT_HeapBlock,NumericShr);
                        }
                    }
                    break;

                case LO_Mul:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));

                        CALL_THUNK(CLR_RT_HeapBlock,NumericMul);
                    }
                    break;

                case LO_Div:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));

                        if(ol.m_flags & CLR_RT_OpcodeLookup::COND_UNSIGNED)
                        {
                            CALL_THUNK(CLR_RT_HeapBlock,NumericDivUn);
                        }
                        else
                        {
                            CALL_THUNK(CLR_RT_HeapBlock,NumericDiv);
                        }
                    }
                    break;

                case LO_Rem:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));

                        if(ol.m_flags & CLR_RT_OpcodeLookup::COND_UNSIGNED)
                        {
                            CALL_THUNK(CLR_RT_HeapBlock,NumericRemUn);
                        }
                        else
                        {
                            CALL_THUNK(CLR_RT_HeapBlock,NumericRem);
                        }
                    }
                    break;


                case LO_Box  :
                case LO_Unbox:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM   ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, op.m_tdInst.m_data               ));
                        TINYCLR_CHECK_HRESULT(Arm_MOV_IMM   ( ArmProcessor::c_register_r2, (lo == LO_Box) ? 1 : 0           ));

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        CALL_THUNK(MethodCompilerHelpers,HandleBoxing);
                    }
                    break;

                case LO_Branch:
                    {
                        CLR_UINT32 cond = ol.m_flags & CLR_RT_OpcodeLookup::COND_BRANCH_MASK;

                        switch(cond)
                        {
                        case CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS:
                            {
                                // Nothing to check.
                            }
                            break;

                        case CLR_RT_OpcodeLookup::COND_BRANCH_IFTRUE:
                        case CLR_RT_OpcodeLookup::COND_BRANCH_IFFALSE:
                            {
                                CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, offsetof(CLR_RT_HeapBlock,m_data.numeric) );

                                if(tdStack->m_flags & (TypeDescriptor::c_ByRef | TypeDescriptor::c_ByRefArray))
                                {
                                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                                }
                                else if(tdStack->m_flags & TypeDescriptor::c_Boxed)
                                {
                                    if(tdStack->m_levels)
                                    {
                                        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                                    }

                                    TINYCLR_CHECK_HRESULT(Arm_LDR    ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                                    TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( ArmProcessor::c_register_r0, 0                                ));
                                }
                                else
                                {
                                    switch(td.GetDataType())
                                    {
                                    case DATATYPE_WEAKCLASS:
                                    case DATATYPE_STRING   :
                                    case DATATYPE_OBJECT   :
                                    case DATATYPE_CLASS    :
                                    case DATATYPE_I4       :
                                        {
                                            TINYCLR_CHECK_HRESULT(Arm_LDR    ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                                            TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( ArmProcessor::c_register_r0, 0                                ));
                                        }
                                        break;

                                    case DATATYPE_I8:
                                    case DATATYPE_R8:
                                        {
                                            /**********/ TINYCLR_CHECK_HRESULT(Arm_LDMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r0 ));
                                            Arm_SetCC(); TINYCLR_CHECK_HRESULT(Arm_ORR  ( ArmProcessor::c_register_r0,      ArmProcessor::c_register_r0     , ArmProcessor::c_register_r1                                  ));
                                        }
                                        break;

                                    default:
                                        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                                    }
                                }

                                Arm_SetCond( cond == CLR_RT_OpcodeLookup::COND_BRANCH_IFTRUE ? ArmProcessor::c_cond_NE : ArmProcessor::c_cond_EQ );
                            }
                            break;

                        default:
                            {
                                CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                                CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                                TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1                          ));
                                TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2                          ));
                                TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r2, (ol.m_flags & CLR_RT_OpcodeLookup::COND_UNSIGNED) ? 0 : 1 ));

                                CALL_THUNK(CLR_RT_HeapBlock,Compare_Values);

                                TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( ArmProcessor::c_register_r0, 0 ));

                                switch(cond)
                                {
                                case CLR_RT_OpcodeLookup::COND_BRANCH_IFEQUAL         : Arm_SetCond( ArmProcessor::c_cond_EQ ); break;
                                case CLR_RT_OpcodeLookup::COND_BRANCH_IFNOTEQUAL      : Arm_SetCond( ArmProcessor::c_cond_NE ); break;
                                case CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER       : Arm_SetCond( ArmProcessor::c_cond_GT ); break;
                                case CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATEROREQUAL: Arm_SetCond( ArmProcessor::c_cond_GE ); break;
                                case CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS          : Arm_SetCond( ArmProcessor::c_cond_LT ); break;
                                case CLR_RT_OpcodeLookup::COND_BRANCH_IFLESSOREQUAL   : Arm_SetCond( ArmProcessor::c_cond_LE ); break;

                                default                                               : TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                                }
                            }
                            break;
                        }

                        TINYCLR_CHECK_HRESULT(Arm_BranchForwardOrBackward( pos, m_opcodeBranches[ osPtr->m_branchesOutIdx ] ));
                    }
                    break;

                case LO_Set:
                    {
                        CLR_INT32  op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32  op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );
                        CLR_UINT32 condTRUE;
                        CLR_UINT32 condFALSE;

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1                          ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2                          ));
                        TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r2, (ol.m_flags & CLR_RT_OpcodeLookup::COND_UNSIGNED) ? 0 : 1 ));

                        CALL_THUNK(CLR_RT_HeapBlock,Compare_Values);

                        TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( ArmProcessor::c_register_r0, 0 ));

                        switch(ol.m_flags & CLR_RT_OpcodeLookup::COND_BRANCH_MASK)
                        {
                        case CLR_RT_OpcodeLookup::COND_BRANCH_IFEQUAL  : condTRUE = ArmProcessor::c_cond_EQ; condFALSE = ArmProcessor::c_cond_NE; break;
                        case CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER: condTRUE = ArmProcessor::c_cond_GT; condFALSE = ArmProcessor::c_cond_LE; break;
                        case CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS   : condTRUE = ArmProcessor::c_cond_LT; condFALSE = ArmProcessor::c_cond_GE; break;

                        default                                        : TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                        }

                        Arm_SetCond( condTRUE  ); TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r1, 1 ));
                        Arm_SetCond( condFALSE ); TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r1, 0 ));

                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_I4, 0, 1 ) ));

                        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r3 ));
                    }
                    break;

                case LO_Switch:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, offsetof(CLR_RT_HeapBlock,m_data.numeric) );

                        TINYCLR_CHECK_HRESULT(Arm_LDR    ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( ArmProcessor::c_register_r0, osPtr->m_branchesOut             ));

                        Arm_SetCond           ( ArmProcessor::c_cond_LS      );
                        Arm_SetShift_Immediate( ArmProcessor::c_shift_LSL, 2 );
                        TINYCLR_CHECK_HRESULT(Arm_ADD( ArmProcessor::c_register_pc, ArmProcessor::c_register_pc, ArmProcessor::c_register_r0 ));

                        TINYCLR_CHECK_HRESULT(Arm_B( ArmProcessor::c_NoLink, Arm_RelativeOffset( m_indexToArmOpcode[ pos+1 ] ) ));

                        for(size_t target=0; target<osPtr->m_branchesOut; target++)
                        {
                            TINYCLR_CHECK_HRESULT(Arm_BranchForwardOrBackward( pos, m_opcodeBranches[ osPtr->m_branchesOutIdx+target ] ));
                        }
                    }
                    break;

                case LO_LoadFunction:
                case LO_LoadVirtFunction:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_MOV       ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4      ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, op.m_mdInst.m_data               ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM   ( ArmProcessor::c_register_r2, ArmProcessor::c_register_r6, op1 ));

                        if(lo == LO_LoadFunction)
                        {
                            TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r3, 0 ));
                        }
                        else
                        {
                            TINYCLR_CHECK_HRESULT(Arm_MOV( ArmProcessor::c_register_r3, ArmProcessor::c_register_r2 ));
                        }

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        CALL_THUNK(MethodCompilerHelpers,LoadFunction);
                    }
                    break;

                case LO_Call     :
                case LO_CallVirt :
                case LO_NewObject:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        {
                            TypeDescriptor* td  = GetFirstOperand( osPtr );
                            CLR_UINT32      num =                  osPtr->m_stackPop;
                            CLR_UINT32      op2 =                  op1;

                            while(num)
                            {
                                if(td->NeedsCloning())
                                {
                                    TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op2 ));

                                    CALL_THUNK(MethodCompilerHelpers,CloneValueType);
                                }

                                td  += 1;
                                op2 += sizeof(CLR_RT_HeapBlock);
                                num -= 1;
                            }
                        }

                        TINYCLR_CHECK_HRESULT(Arm_MOV       ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4      ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, op.m_mdInst.m_data               ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM   ( ArmProcessor::c_register_r2, ArmProcessor::c_register_r6, op1 ));

                        switch(lo)
                        {
                        case LO_Call:
                            {
                                CALL_THUNK(MethodCompilerHelpers,Call);
                            }
                            break;

                        case LO_CallVirt:
                            {
                                CALL_THUNK(MethodCompilerHelpers,CallVirtual);
                            }
                            break;

                        case LO_NewObject:
                            {
                                CLR_RT_TypeDef_Instance cls; cls.InitializeFromMethod( op.m_mdInst ); // This is the class to create!

                                if(cls.m_target->IsDelegate())
                                {
                                    CALL_THUNK(MethodCompilerHelpers,NewDelegate);
                                }
                                else
                                {
                                    CALL_THUNK(MethodCompilerHelpers,NewObject);
                                }
                            }
                            break;
                        }
                    }
                    break;

                case LO_Ret:
                    TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                    TINYCLR_CHECK_HRESULT(Arm_B( ArmProcessor::c_NoLink, Arm_AbsoluteOffset( m_Arm_Thunks->m_address__Internal_ReturnFromMethod ) ));
                    break;

                case LO_CastClass:
                case LO_IsInst:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM   ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, op.m_tdInst.m_data               ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r2, op.m_tdInstLevels                ));
                        TINYCLR_CHECK_HRESULT(Arm_MOV_IMM   ( ArmProcessor::c_register_r3, (lo == LO_IsInst) ? 1 : 0        ));

                        if(lo == LO_CastClass)
                        {
                            CALL_THUNK(MethodCompilerHelpers,HandleCasting);
                        }
                        else
                        {
                            CALL_THUNK(MethodCompilerHelpers,HandleIsInst);
                        }
                    }
                    break;

                case LO_Dup:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_LDMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3 ));
                        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op2, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3 ));
                    }
                    break;

                case LO_Pop:
                    {
                        // Nothing to do.
                    }
                    break;


                case LO_Throw:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_MOV    ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4      ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op1 ));

                        CALL_THUNK(MethodCompilerHelpers,Throw);
                    }
                    break;

                case LO_Rethrow:
                    {
                        TINYCLR_CHECK_HRESULT(Arm_MOV( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4 ));

                        CALL_THUNK(MethodCompilerHelpers,Rethrow);
                    }
                    break;

                case LO_Leave:
                    {
                        CLR_UINT32 target = m_opcodeBranches[ osPtr->m_branchesOutIdx ];
                        CLR_UINT32 from   = m_Arm_BaseAddress + m_indexToArmOpcode[ pos    ];
                        CLR_UINT32 to     = m_Arm_BaseAddress + m_indexToArmOpcode[ target ];

                        TINYCLR_CHECK_HRESULT(Arm_MOV( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4          ));
                        TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r1, ArmProcessor::c_register_pc, 4*4 - 8 ));
                        TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r2, ArmProcessor::c_register_pc, 4*4 - 8 ));

                        CALL_THUNK(MethodCompilerHelpers,Leave);

                        TINYCLR_CHECK_HRESULT(Arm_MOV( ArmProcessor::c_register_pc, ArmProcessor::c_register_r0 ));

                        TINYCLR_CHECK_HRESULT(Arm_EmitData( from ));
                        TINYCLR_CHECK_HRESULT(Arm_EmitData( to   ));
                    }
                    break;

                case LO_EndFinally:
                    {
                        TINYCLR_CHECK_HRESULT(Arm_MOV( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4 ));

                        CALL_THUNK(MethodCompilerHelpers,EndFinally);

                        TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_pc, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_IP) ));
                    }
                    break;


                case LO_Convert:
                    {
                        CLR_INT32                    op1   = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        bool                         fSlow = false;
                        const CLR_RT_DataTypeLookup& dtl   = c_CLR_RT_DataTypeLookup[ ol.m_dt ];

                        //
                        // For now, OVERFLOW checks are not implemented.
                        //
                        //

                        switch(td.GetDataType())
                        {
                        case DATATYPE_I4:
                            {
                                switch(ol.m_dt)
                                {
                                case DATATYPE_I4:
                                case DATATYPE_U4:
                                    // Nothing to do...
                                    break;

                                case DATATYPE_I1:
                                case DATATYPE_I2:
                                case DATATYPE_BOOLEAN:
                                case DATATYPE_U1     :
                                case DATATYPE_CHAR:
                                case DATATYPE_U2  :
                                    TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 + offsetof(CLR_RT_HeapBlock,m_data.numeric), dtl.m_sizeInBytes, ((dtl.m_flags & CLR_RT_DataTypeLookup::c_Signed) != 0) ));
                                    TINYCLR_CHECK_HRESULT(Arm_STR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 + offsetof(CLR_RT_HeapBlock,m_data.numeric)                                                                            ));
                                    break;

                                case DATATYPE_I8:
                                case DATATYPE_U8:
                                case DATATYPE_R4:
                                case DATATYPE_R8:
                                    fSlow = true;
                                    break;

                                default:
                                   TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                                }
                            }
                            break;

                        case DATATYPE_I8:
                            {
                                switch(ol.m_dt)
                                {
                                case DATATYPE_I8:
                                case DATATYPE_U8:
                                    // Nothing to do...
                                    break;

                                default:
                                    fSlow = true;
                                    break;
                                }
                            }
                            break;

                        case DATATYPE_R4:
                            {
                                fSlow = true;
                            }
                            break;

                        case DATATYPE_R8:
                            {
                                fSlow = true;
                            }
                            break;

                        default:
                            TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                        }

                        if(fSlow)
                        {
                            TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1                          ));
                            TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r1,  ol.m_dt                                                  ));
                            TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r2, (ol.m_flags & CLR_RT_OpcodeLookup::COND_OVERFLOW) ? 1 : 0 ));
                            TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r3, (ol.m_flags & CLR_RT_OpcodeLookup::COND_UNSIGNED) ? 1 : 0 ));

                            CALL_THUNK(CLR_RT_HeapBlock,Convert);
                        }
                    }
                    break;


                case LO_StoreArgument:
                case LO_StoreLocal:
                case LO_StoreStaticField:
                case LO_StoreField:
                    {
                        CLR_UINT32                srcReg = ArmProcessor::c_register_r6;
                        CLR_UINT32                dstReg;
                        CLR_INT32                 srcIdx = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32                 dstIdx = op.Index() * sizeof(CLR_RT_HeapBlock);
                        TypeDescriptor            tdTmp;
                        TypeDescriptor*           tdPtr;
                        TypeDescriptor::Promotion pr;

                        switch(lo)
                        {
                        case LO_StoreArgument:
                            dstReg = ArmProcessor::c_register_r7;
                            tdPtr  = &m_arguments[ op.Index() ];
                            break;

                        case LO_StoreLocal:
                            dstReg = ArmProcessor::c_register_r8;
                            tdPtr  = &m_locals[ op.Index() ];
                            break;

                        case LO_StoreStaticField:
                        case LO_StoreField:
                            dstReg = ArmProcessor::c_register_r3;
                            dstIdx = 0;

                            if(lo == LO_StoreStaticField)
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, op.m_fdInst.m_data ));

                                CALL_THUNK(MethodCompilerHelpers,AccessStaticField);

                                TINYCLR_CHECK_HRESULT(Arm_MOV( dstReg, ArmProcessor::c_register_r0 ));
                            }
                            else
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LDR        (      dstReg, srcReg, srcIdx + offsetof(CLR_RT_HeapBlock,m_data.objectReference.ptr) ));
                                TINYCLR_CHECK_HRESULT(Arm_FaultOnNull( pos, dstReg                                                                         ));

                                switch(td.GetDataType())
                                {
                                    //
                                    // Special case: DATETIME and TIMESPAN can be passed as BOXED or BYREF.
                                    //
                                    // If BOXED, the data is at offset 1.
                                    // If BYREF, the data is at offset 0.
                                    //
                                case DATATYPE_DATETIME:
                                case DATATYPE_TIMESPAN:
                                    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR    ( ArmProcessor::c_register_r1, srcReg, srcIdx + offsetof(CLR_RT_HeapBlock,m_id.type.dataType), 1, false ));
                                    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( ArmProcessor::c_register_r1, DATATYPE_BYREF                                                           ));
                                    Arm_SetCond( ArmProcessor::c_cond_NE ); TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( dstReg, dstReg, sizeof(CLR_RT_HeapBlock)                                                              ));

                                    TINYCLR_CHECK_HRESULT(Arm_LDMIB( srcReg, srcIdx + sizeof(CLR_RT_HeapBlock), ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
                                    TINYCLR_CHECK_HRESULT(Arm_STMIB( dstReg, dstIdx                           , ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
                                    continue;


                                case DATATYPE_REFLECTION:
                                case DATATYPE_WEAKCLASS:
                                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                                }

                                srcIdx += sizeof(CLR_RT_HeapBlock);
                                dstIdx  = op.m_fdInst.CrossReference().m_offset * sizeof(CLR_RT_HeapBlock);
                            }

                            TINYCLR_CHECK_HRESULT(td.InitializeFromFieldDefinition( op.m_fdInst ));

                            if(tdTmp.ConvertFromTypeDescriptor( td ) == false)
                            {
                                TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                            }

                            tdPtr = &tdTmp;
                            break;
                        }

                        if(tdPtr->NeedsCloning())
                        {
                            TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, srcReg, srcIdx ));
                            TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, dstReg, dstIdx ));

                            CALL_THUNK(MethodCompilerHelpers,CopyValueType);
                        }
                        else if(tdPtr->NeedsPromotion( pr, true ))
                        {
                            if(pr.m_size <= 4)
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r0, srcReg, srcIdx + offsetof(CLR_RT_HeapBlock,m_data.numeric) ));
                                TINYCLR_CHECK_HRESULT(Arm_STR( ArmProcessor::c_register_r0, dstReg, dstIdx + offsetof(CLR_RT_HeapBlock,m_data.numeric) ));
                            }
                            else
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LDMIB( srcReg, srcIdx, ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
                                TINYCLR_CHECK_HRESULT(Arm_STMIB( dstReg, dstIdx, ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
                            }
                        }
                        else
                        {
                            TINYCLR_CHECK_HRESULT(Arm_LDMIA( srcReg, srcIdx, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
                            TINYCLR_CHECK_HRESULT(Arm_STMIA( dstReg, dstIdx, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
                        }
                    }
                    break;

                case LO_LoadArgument:
                case LO_LoadLocal:
                case LO_LoadStaticField:
                case LO_LoadField:
                    {
                        CLR_UINT32                srcReg;
                        CLR_UINT32                dstReg = ArmProcessor::c_register_r6;
                        CLR_INT32                 srcIdx = op.Index() * sizeof(CLR_RT_HeapBlock);
                        CLR_INT32                 dstIdx = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        TypeDescriptor            tdTmp;
                        TypeDescriptor*           tdPtr;
                        TypeDescriptor::Promotion pr;

                        switch(lo)
                        {
                        case LO_LoadArgument:
                            srcReg = ArmProcessor::c_register_r7;
                            tdPtr  = &m_arguments[ op.Index() ];
                            break;

                        case LO_LoadLocal:
                            srcReg = ArmProcessor::c_register_r8;
                            tdPtr  = &m_locals[ op.Index() ];
                            break;

                        case LO_LoadStaticField:
                        case LO_LoadField:
                            srcReg = ArmProcessor::c_register_r3;
                            srcIdx = 0;

                            if(lo == LO_LoadStaticField)
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, op.m_fdInst.m_data ));

                                CALL_THUNK(MethodCompilerHelpers,AccessStaticField);

                                TINYCLR_CHECK_HRESULT(Arm_MOV( srcReg, ArmProcessor::c_register_r0 ));
                            }
                            else
                            {
                                CLR_DataType tdPointer = td.GetDataType();
                                CLR_UINT32   ptrReg    = dstReg;
                                CLR_INT32    ptrIdx    = dstIdx;

                                TINYCLR_CHECK_HRESULT(Arm_LDR        (      srcReg, ptrReg, ptrIdx + offsetof(CLR_RT_HeapBlock,m_data.objectReference.ptr) ));
                                TINYCLR_CHECK_HRESULT(Arm_FaultOnNull( pos, srcReg                                                                         ));

                                switch(tdPointer)
                                {
                                    //
                                    // Special case: DATETIME and TIMESPAN can be passed as BOXED or BYREF.
                                    //
                                    // If BOXED, the data is at offset 1.
                                    // If BYREF, the data is at offset 0.
                                    //
                                case DATATYPE_DATETIME:
                                case DATATYPE_TIMESPAN:
                                    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR    ( ArmProcessor::c_register_r0, ptrReg, ptrIdx + offsetof(CLR_RT_HeapBlock,m_id.type.dataType), 1, false ));
                                    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( ArmProcessor::c_register_r0, DATATYPE_BYREF                                                           ));
                                    Arm_SetCond( ArmProcessor::c_cond_NE ); TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( srcReg, srcReg, sizeof(CLR_RT_HeapBlock)                                                              ));

                                    TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_I8, 0, 1 )                                                                         ));
                                    TINYCLR_CHECK_HRESULT(Arm_LDMIB     ( srcReg, srcIdx,                                   ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
                                    TINYCLR_CHECK_HRESULT(Arm_STMIA     ( dstReg, dstIdx, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
                                    continue;

                                case DATATYPE_REFLECTION:
                                case DATATYPE_WEAKCLASS:
                                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                                }

                                srcIdx = op.m_fdInst.CrossReference().m_offset * sizeof(CLR_RT_HeapBlock);
                            }

                            TINYCLR_CHECK_HRESULT(td.InitializeFromFieldDefinition( op.m_fdInst ));

                            if(tdTmp.ConvertFromTypeDescriptor( td ) == false)
                            {
                                TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                            }

                            tdPtr = &tdTmp;
                            break;
                        }

                        if(tdPtr->NeedsPromotion( pr, false ))
                        {
                            if(pr.m_size <= 4)
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_I4, 0, 1 )                                                                   ));
                                TINYCLR_CHECK_HRESULT(Arm_LDR       ( ArmProcessor::c_register_r1, srcReg, srcIdx + offsetof(CLR_RT_HeapBlock,m_data.numeric), pr.m_size, pr.m_fSigned                            ));
                                TINYCLR_CHECK_HRESULT(Arm_STMIA     (                              dstReg, dstIdx, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r3 ));
                            }
                            else
                            {
                                TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_I8, 0, 1 )                                                                         ));
                                TINYCLR_CHECK_HRESULT(Arm_LDMIB     ( srcReg, srcIdx,                                   ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
                                TINYCLR_CHECK_HRESULT(Arm_STMIA     ( dstReg, dstIdx, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
                            }
                        }
                        else
                        {
                            TINYCLR_CHECK_HRESULT(Arm_LDMIA( srcReg, srcIdx, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
                            TINYCLR_CHECK_HRESULT(Arm_STMIA( dstReg, dstIdx, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
                        }
                    }
                    break;

                case LO_LoadArgumentAddress:
                case LO_LoadLocalAddress:
                case LO_LoadStaticFieldAddress:
                case LO_LoadFieldAddress:
                    {
                        CLR_UINT32 srcReg;
                        CLR_UINT32 dstReg = ArmProcessor::c_register_r6;
                        CLR_INT32  srcIdx = op.Index() * sizeof(CLR_RT_HeapBlock);
                        CLR_INT32  dstIdx = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        switch(lo)
                        {
                        case LO_LoadArgumentAddress:
                            srcReg = ArmProcessor::c_register_r7;

                            m_arguments[ op.Index() ].ConvertToTypeDescriptor( td );
                            break;

                        case LO_LoadLocalAddress:
                            srcReg = ArmProcessor::c_register_r8;

                            m_locals[ op.Index() ].ConvertToTypeDescriptor( td );
                            break;

                        case LO_LoadStaticFieldAddress:
                            {
                                srcReg = ArmProcessor::c_register_r3;
                                srcIdx = 0;

                                TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, op.m_fdInst.m_data ));

                                CALL_THUNK(MethodCompilerHelpers,AccessStaticField);

                                TINYCLR_CHECK_HRESULT(Arm_MOV( srcReg, ArmProcessor::c_register_r0 ));

                                TINYCLR_CHECK_HRESULT(td.InitializeFromFieldDefinition( op.m_fdInst ));
                            }
                            break;

                        case LO_LoadFieldAddress:
                            {
                                CLR_UINT32 ptrReg = dstReg;
                                CLR_INT32  ptrIdx = dstIdx;

                                srcReg = ArmProcessor::c_register_r3;
                                srcIdx = op.m_fdInst.CrossReference().m_offset * sizeof(CLR_RT_HeapBlock);

                                TINYCLR_CHECK_HRESULT(Arm_LDR        (      srcReg, ptrReg, ptrIdx + offsetof(CLR_RT_HeapBlock,m_data.objectReference.ptr) ));
                                TINYCLR_CHECK_HRESULT(Arm_FaultOnNull( pos, srcReg                                                                         ));

                                switch(td.GetDataType())
                                {
                                case DATATYPE_DATETIME:
                                case DATATYPE_TIMESPAN:
                                    //
                                    // Getting the address of a field for DATETIME and TIMESPAN doesn't really work...
                                    //
                                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                                }

                                TINYCLR_CHECK_HRESULT(td.InitializeFromFieldDefinition( op.m_fdInst ));
                            }
                            break;
                        }

                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_BYREF, 0, 1 ) ));

                        if(td.GetDataType() == DATATYPE_VALUETYPE)
                        {
                            TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r1, srcReg, srcIdx + offsetof(CLR_RT_HeapBlock,m_data.objectReference.ptr) ));
                        }
                        else
                        {
                            TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, srcReg, srcIdx ));
                        }

                        TINYCLR_CHECK_HRESULT(Arm_STMIA( dstReg, dstIdx, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r3  ));
                    }
                    break;

                case LO_LoadConstant_I4:
                case LO_LoadConstant_R4:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( op.m_value.DataType(), 0, 1 ) ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, op.m_value.NumericByRef().u4                           ));

                        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r3 ));
                    }
                    break;

                case LO_LoadConstant_I8:
                case LO_LoadConstant_R8:
                    {
                        CLR_INT32   op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_UINT32* ptr = &op.m_value.NumericByRef().u4;

                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( op.m_value.DataType(), 0, 1 ) ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, ptr[ 0 ]                                               ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r2, ptr[ 1 ]                                               ));

                        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3 ));
                    }
                    break;

                case LO_LoadNull:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_OBJECT, 0, 1 ) ));
                        TINYCLR_CHECK_HRESULT(Arm_MOV_IMM   ( ArmProcessor::c_register_r1, 0                                                ));

                        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r3 ));
                    }
                    break;

                case LO_LoadString:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_MOV       ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r4      ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, (CLR_UINT16)op.m_token           ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM   ( ArmProcessor::c_register_r2, ArmProcessor::c_register_r6, op1 ));

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        CALL_THUNK(MethodCompilerHelpers,LoadString);
                    }
                    break;

                case LO_LoadToken:
                    {
                        CLR_INT32                  op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_RT_ReflectionDef_Index reflex;

                        switch(CLR_TypeFromTk( op.m_token ))
                        {
                        case TBL_TypeSpec:
                        case TBL_TypeRef:
                        case TBL_TypeDef:
                            reflex.m_kind        = REFLECTION_TYPE;
                            reflex.m_levels      = op.m_tdInstLevels;
                            reflex.m_data.m_type = op.m_tdInst;
                            break;

                        case TBL_FieldRef:
                        case TBL_FieldDef:
                            reflex.m_kind         = REFLECTION_FIELD;
                            reflex.m_levels       = 0;
                            reflex.m_data.m_field = op.m_fdInst;
                            break;

                        case TBL_MethodRef:
                        case TBL_MethodDef:
                            reflex.m_kind          = REFLECTION_METHOD;
                            reflex.m_levels        = 0;
                            reflex.m_data.m_method = op.m_mdInst;
                            break;

                        default:
                            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                            break;
                        }

                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_REFLECTION, 0, 1 ) ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, ((CLR_UINT32*)&reflex)[ 0 ]                          ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r2, ((CLR_UINT32*)&reflex)[ 1 ]                          ));

                        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3 ));
                    }
                    break;

                case LO_NewArray:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0                                         );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 0, offsetof(CLR_RT_HeapBlock,m_data.numeric) );

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM   ( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r1, op.m_tdInst.m_data               ));
                        TINYCLR_CHECK_HRESULT(Arm_MOV_IMM   ( ArmProcessor::c_register_r2, op.m_tdInstLevels                ));
                        TINYCLR_CHECK_HRESULT(Arm_LDR       ( ArmProcessor::c_register_r3, ArmProcessor::c_register_r6, op2 ));

                        CALL_THUNK(MethodCompilerHelpers,NewArray);
                    }
                    break;

                case LO_LoadLength:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_LDR        (      ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op1 + offsetof(CLR_RT_HeapBlock,m_data.objectReference.ptr) ));
                        TINYCLR_CHECK_HRESULT(Arm_FaultOnNull( pos, ArmProcessor::c_register_r1                                                                                           ));

                        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_I4, 0, 1 )                                   ));
                        TINYCLR_CHECK_HRESULT(Arm_LDR       ( ArmProcessor::c_register_r1, ArmProcessor::c_register_r1, offsetof(CLR_RT_HeapBlock_Array,m_numOfElements ) ));

                        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op1, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r3 ));
                    }
                    break;

                case LO_StoreElement:
                    {
                        const CLR_RT_DataTypeLookup* dtl;

                        TINYCLR_CHECK_HRESULT(Arm_CheckArrayAccess( pos, td, dtl ));
                        TINYCLR_CHECK_HRESULT(Arm_FindArrayElement( pos,     dtl ));
                        TINYCLR_CHECK_HRESULT(Arm_StoreElement    ( pos,     dtl ));
                    }
                    break;

                case LO_LoadElement:
                    {
                        const CLR_RT_DataTypeLookup* dtl;

                        TINYCLR_CHECK_HRESULT(Arm_CheckArrayAccess( pos, td, dtl ));
                        TINYCLR_CHECK_HRESULT(Arm_FindArrayElement( pos,     dtl ));
                        TINYCLR_CHECK_HRESULT(Arm_LoadElement     ( pos,     dtl ));
                    }
                    break;

                case LO_LoadElementAddress:
                    {
                        const CLR_RT_DataTypeLookup* dtl;

                        TINYCLR_CHECK_HRESULT(Arm_CheckArrayAccess  ( pos, td, dtl ));
                        TINYCLR_CHECK_HRESULT(Arm_LoadElementAddress( pos,     dtl ));
                    }
                    break;

                case LO_StoreIndirect:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));
                        TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r2, op.m_op                          ));

                        CALL_THUNK(MethodCompilerHelpers,StoreIndirect);
                    }
                    break;

                case LO_LoadIndirect:
                    {
                        CLR_INT32 op = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op ));

                        CALL_THUNK(MethodCompilerHelpers,LoadIndirect);
                    }
                    break;

                case LO_InitObject:
                    {
                        CLR_INT32 op = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op ));

                        CALL_THUNK(CLR_RT_HeapBlock,InitObject);
                    }
                    break;

                case LO_LoadObject:
                    {
                        CLR_INT32 op = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op ));

                        CALL_THUNK(MethodCompilerHelpers,LoadObject);
                    }
                    break;

                case LO_CopyObject:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));

                        CALL_THUNK(MethodCompilerHelpers,CopyObject);
                    }
                    break;

                case LO_StoreObject:
                    {
                        CLR_INT32 op1 = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );
                        CLR_INT32 op2 = Arm_GetOpcodeOperandOffset( osPtr, 1, 0 );

                        TINYCLR_CHECK_HRESULT(Arm_FlushEvalStackPointer( pos, ArmProcessor::c_register_lr ));

                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op1 ));
                        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op2 ));

                        CALL_THUNK(MethodCompilerHelpers,StoreObject);
                    }
                    break;

                case LO_Nop:
                    {
                        // Nothing to do.
                    }
                    break;

                default:
                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
                }
            }
        }

        //--//

        m_Arm_ROData_Base = Arm_CurrentRelativePC();

        for(pos = 0; pos < m_Arm_ROData.Size(); pos++)
        {
            TINYCLR_CHECK_HRESULT(Arm_EmitData( m_Arm_ROData[ pos ] ));
        }
    }

    TINYCLR_JITTER_STATISTICS_EXECUTE( stats.Dump( *this, s_statistics ) );

    //--//

    TINYCLR_NOCLEANUP();
}

//--//

void MethodCompiler::DumpOpcode( size_t pos )
{
    OpcodeSlot*                osPtr = &m_opcodeSlots[ pos ];
    const CLR_RT_OpcodeLookup& ol    =  c_CLR_RT_OpcodeLookup[ osPtr->m_op ];

    CLR_Debug::Printf( " IL_%04x(%3d) %c%c : " , pos, osPtr->m_stackDepth, osPtr->m_flags & OpcodeSlot::c_BranchForward ? 'F' : ' ', osPtr->m_flags & OpcodeSlot::c_BranchBackward ? 'B' : ' ' );

    CLR_Debug::Printf( "%-20s %-12s", c_CLR_RT_LogicalOpcodeLookup[ ol.m_logicalOpcode ].Name(), ol.Name() );

    if(osPtr->m_branchesIn)
    {
        CLR_Debug::Printf( " [IN:" );
        for(size_t i=0; i<osPtr->m_branchesIn; i++)
        {
            CLR_Debug::Printf( " IL_%04x", m_opcodeBranches[ osPtr->m_branchesInIdx+i ] );
        }
        CLR_Debug::Printf( "]" );
    }

    if(osPtr->m_branchesOut)
    {
        CLR_Debug::Printf( " [OUT:" );
        for(size_t i=0; i<osPtr->m_branchesOut; i++)
        {
            CLR_Debug::Printf( " IL_%04x", m_opcodeBranches[ osPtr->m_branchesOutIdx+i ] );
        }
        CLR_Debug::Printf( "]" );
    }

    if(osPtr->m_stackIdx != CLR_EmptyIndex)
    {
        TypeDescriptor* ptr    = GetFirstStackElement( osPtr );
        TypeDescriptor* ptrEnd = GetLastStackElement(  osPtr );

        {
            int offset = (osPtr->m_branchesIn ? 6 + osPtr->m_branchesIn * 8 : 0) + (osPtr->m_branchesOut ? 7 + osPtr->m_branchesOut * 8 : 0);

            if(offset < 20) offset = 20 - offset;
            else            offset = 0;

            CLR_Debug::Printf( "%*s  Stack: ", offset, "" );
        }

        while(ptr <= ptrEnd)
        {
            CLR_INT32 levels = ptr->m_levels;

            CLR_Debug::Printf( " " );

            if(ptr->m_flags & TypeDescriptor::c_ByRef     ) CLR_Debug::Printf( "ByRef "      );
            if(ptr->m_flags & TypeDescriptor::c_ByRefArray) CLR_Debug::Printf( "ByRefArray " );
            if(ptr->m_flags & TypeDescriptor::c_Boxed     ) CLR_Debug::Printf( "Boxed "      );
            if(ptr->m_flags & TypeDescriptor::c_Null      ) CLR_Debug::Printf( "Null "       );

            CLR_RT_DUMP::TYPE( ptr->m_cls ); while(levels-- > 0) CLR_Debug::Printf( "[]" );

            if(ptr < ptrEnd) CLR_Debug::Printf( "," );

            ptr++;
        }
    }

    CLR_Debug::Printf( "\r\n" );
}

void MethodCompiler::DumpOpcodes()
{
    for(size_t pos = 0; pos < m_numOpcodes; pos++)
    {
        DumpOpcode( pos );
    }
}

void MethodCompiler::DumpJitterOutput()
{
    CLR_UINT32 address = m_Arm_BaseAddress;

    for(size_t pos = 0; pos < m_Arm_Opcodes.Size(); pos++)
    {
        ArmProcessor::Opcode::Print( address, m_Arm_Opcodes[ pos ] ); address += sizeof(CLR_UINT32);
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif
