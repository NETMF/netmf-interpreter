////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core\Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#define TINYCLR_JITTER_CODECOVERAGE

#if defined(_WIN32)
#undef TINYCLR_JITTER_CODECOVERAGE
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

//--//

struct CodeCoverage
{
#undef DECL_POSTFIX
#if defined(TINYCLR_JITTER_CODECOVERAGE)
#define DECL_POSTFIX
#else
#define DECL_POSTFIX {}
#endif
    static void RegisterFunction( FLASH_WORD* dst, LPCSTR szText ) DECL_POSTFIX;

    static void Register( FLASH_WORD* dst, const CLR_RT_MethodDef_Index& md ) DECL_POSTFIX;

#undef DECL_POSTFIX
};

//--//

#if defined(TINYCLR_JITTER_CODECOVERAGE)

#define CODECOVERAGE_REGISTERTHUNK(thunk) CodeCoverage::RegisterFunction( (FLASH_WORD*)(size_t)g_thunkTable.m_address__##thunk, "Jitter#" #thunk )

void CodeCoverage::RegisterFunction( FLASH_WORD* dst, LPCSTR szText )
{
}

void CodeCoverage::Register( FLASH_WORD* dst, const CLR_RT_MethodDef_Index& md )
{
    char   rgBuffer[ 512 ];
    LPSTR  szBuffer = rgBuffer;
    size_t iBuffer  = MAXSTRLEN(rgBuffer);

    g_CLR_RT_TypeSystem.BuildMethodName( md, szBuffer, iBuffer );

    RegisterFunction( dst, rgBuffer );
}

#else

#define CODECOVERAGE_REGISTERTHUNK(thunk)

#endif

//--//

HRESULT CLR_RT_ExecutionEngine::Compile( const CLR_RT_MethodDef_Index& md, CLR_UINT32 flags )
{
    TINYCLR_HEADER();

    MethodCompiler mc;
#if !defined(BUILD_RTM)
    CLR_UINT64     stats_start;
#endif

#if defined(_WIN32)
    g_CLR_RT_ArmEmulator.InitializeExternalCalls();
#endif

    ////////////////////////////////////////////////////////////////////////////

    //
    // Create thunk table.
    //
    if(g_thunkTable.m_address__Internal_Initialize == 0)
    {
        TINYCLR_CHECK_HRESULT(mc.Initialize( NULL, (CLR_UINT32)(size_t)m_jitter_current ));

        TINYCLR_CHECK_HRESULT(mc.CreateThunks( &g_thunkTable ));

        if(s_CLR_RT_fJitter_Enabled)
        {
            size_t      len = mc.m_Arm_Opcodes.Size() * sizeof(CLR_UINT32);
            FLASH_WORD* src = (FLASH_WORD*)&mc.m_Arm_Opcodes[ 0 ];
            FLASH_WORD* dst = m_jitter_current;
            FLASH_WORD* end = CLR_RT_Persistence_Manager::Bank::IncrementPointer( dst, (CLR_UINT32)len );

            //--//

            CODECOVERAGE_REGISTERTHUNK( Internal_Initialize                       );
            CODECOVERAGE_REGISTERTHUNK( Internal_Restart                          );
            CODECOVERAGE_REGISTERTHUNK( Internal_Error                            );
            CODECOVERAGE_REGISTERTHUNK( Internal_ErrorNoFlush                     );
            CODECOVERAGE_REGISTERTHUNK( Internal_ReturnFromMethod                 );

            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__Compare_Values          );

            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__NumericMul              );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__NumericDiv              );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__NumericDivUn            );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__NumericRem              );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__NumericRemUn            );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__NumericShl              );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__NumericShr              );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__NumericShrUn            );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__InitObject              );
            CODECOVERAGE_REGISTERTHUNK( CLR_RT_HeapBlock__Convert                 );

            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__HandleBoxing       );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__HandleIsInst       );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__HandleCasting      );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__CopyValueType      );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__CloneValueType     );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__LoadFunction       );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__LoadString         );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__NewArray           );

            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__Call               );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__CallVirtual        );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__NewObject          );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__NewDelegate        );

            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__AccessStaticField  );

            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__Throw              );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__Rethrow            );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__Leave              );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__EndFinally         );

            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__LoadIndirect       );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__StoreIndirect      );

            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__LoadObject         );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__CopyObject         );
            CODECOVERAGE_REGISTERTHUNK( MethodCompilerHelpers__StoreObject        );

            //--//

            if(end < m_jitter_end)
            {
                ::Flash_ChipReadOnly( FALSE );

                while(dst < end)
                {
                    Flash_WriteToSector( Flash_FindSector(dst), dst++, sizeof(FLASH_WORD), (byte *)src );
                    src ++;
                }

                ::Flash_ChipReadOnly( TRUE );

                if(memcmp( m_jitter_current, &mc.m_Arm_Opcodes[ 0 ], len ) == 0)
                {
                    m_jitter_current = dst;
                }
                else
                {
                    CLR_Debug::Printf( "Jitter: failed to write thunk table!\r\n" );
                }
            }
            else
            {
                CLR_Debug::Printf( "Jitter: no space for thunk table!\r\n" );
            }
        }
#if defined(_WIN32)
        else
        {
            size_t len = mc.m_Arm_Opcodes.Size() * sizeof(CLR_UINT32);

            m_jitter_current = CLR_RT_Persistence_Manager::Bank::IncrementPointer( m_jitter_current, (CLR_UINT32)len );
        }
#endif

        mc.Release();
    }

    ////////////////////////////////////////////////////////////////////////////

#if !defined(BUILD_RTM)
    if(s_CLR_RT_fJitter_Trace_Compile >= c_CLR_RT_Trace_Info)
    {
        CLR_Debug::Printf( "\r\n\r\nJitting " ); CLR_RT_DUMP::METHOD( md ); CLR_Debug::Printf( "\r\n" );

        stats_start = Time_CurrentTicks();
    }
#endif

    TINYCLR_CHECK_HRESULT(mc.Initialize( &md, (CLR_UINT32)(size_t)m_jitter_current ));

    mc.ReferToThunks( &g_thunkTable );

    ////////////////////////////////////////////////////////////////////////////

    if(mc.m_mdInst.m_target->RVA == CLR_EmptyIndex)
    {
        //
        // Native implementation. Nothing to do.
        //
    }
    else
    {
        TINYCLR_CHECK_HRESULT(mc.ParseArguments());
        TINYCLR_CHECK_HRESULT(mc.ParseLocals   ());

        TINYCLR_CHECK_HRESULT(mc.ParseByteCode ());

        TINYCLR_CHECK_HRESULT(mc.ParseEvalStack());

        TINYCLR_CHECK_HRESULT(mc.GenerateCode());

#if !defined(BUILD_RTM)
        if(s_CLR_RT_fJitter_Trace_Compile >= c_CLR_RT_Trace_Verbose)
        {
            int milliSec = ((int)::Time_TicksToTime( Time_CurrentTicks() - stats_start ) + CLR_RT_Time::c_TickUnits - 1) / CLR_RT_Time::c_TickUnits;

            CLR_Debug::Printf( "Compile time: %dmsec\r\n", milliSec );
        }
#endif

        if(s_CLR_RT_fJitter_Enabled)
        {
            CLR_RT_Assembly* assm = mc.m_mdInst.m_assm;

            if(assm->m_jittedCode == NULL)
            {
                CLR_UINT32 size = assm->m_pTablesSize[ TBL_MethodDef ] * sizeof(CLR_RT_MethodHandler);

                assm->m_jittedCode = (CLR_RT_MethodHandler*)CLR_RT_Memory::Allocate_And_Erase( size );
            }

            if(assm->m_jittedCode)
            {
                size_t      len =               mc.m_Arm_Opcodes.Size() * sizeof(CLR_UINT32);
                FLASH_WORD* src = (FLASH_WORD*)&mc.m_Arm_Opcodes[ 0 ];
                FLASH_WORD* dst = m_jitter_current;
                FLASH_WORD* end = CLR_RT_Persistence_Manager::Bank::IncrementPointer( dst, (CLR_UINT32)len );

                CodeCoverage::Register( dst, md );

                if(end < m_jitter_end)
                {
                    ::Flash_ChipReadOnly( FALSE );

                    while(dst < end)
                    {
                        Flash_WriteToSector( Flash_FindSector(dst), dst++, sizeof(FLASH_WORD), (byte *)src );
                        src ++;                        
                    }

                    ::Flash_ChipReadOnly( TRUE );

                    if(memcmp( m_jitter_current, &mc.m_Arm_Opcodes[ 0 ], len ) == 0)
                    {
                        assm->m_jittedCode[ mc.m_mdInst.Method() ] = (CLR_RT_MethodHandler)m_jitter_current;

                        m_jitter_current = dst;
                    }
                    else
                    {
                        CLR_Debug::Printf( "Jitter: failed to write method!\r\n" );
                    }
                }
                else
                {
                    CLR_Debug::Printf( "Jitter: no space for method!\r\n" );
                }
            }
            else
            {
                CLR_Debug::Printf( "Jitter: cannot allocate method table!\r\n" );
            }
        }
#if defined(_WIN32)
        else
        {
            size_t len = mc.m_Arm_Opcodes.Size() * sizeof(CLR_UINT32);

            m_jitter_current = CLR_RT_Persistence_Manager::Bank::IncrementPointer( m_jitter_current, (CLR_UINT32)len );
        }
#endif
    }

    ////////////////////////////////////////////////////////////////////////////

    TINYCLR_CLEANUP();

    mc.Release();

    TINYCLR_CLEANUP_END();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER_ARMEMULATION)

HRESULT CLR_RT_ExecutionEngine::Emulate( CLR_RT_StackFrame* stack )
{
    TINYCLR_HEADER();

#if !defined(BUILD_RTM)
    if(s_CLR_RT_fJitter_Trace_Invoke >= c_CLR_RT_Trace_Info)
    {
        CLR_Debug::Printf( "####################################################\r\n" );
        CLR_Debug::Printf( "# Emulating call to " ); CLR_RT_DUMP::METHOD( stack->m_call ); CLR_Debug::Printf( "\r\n" );
        CLR_Debug::Printf( "####################################################\r\n" );
    }
#endif

    TINYCLR_SET_AND_LEAVE(g_CLR_RT_ArmEmulator.Execute( stack ));

    TINYCLR_NOCLEANUP();
}

//--//--//--//--//--//--//--//--//--//--//--//--//--//

CLR_RT_ArmEmulator g_CLR_RT_ArmEmulator;

//--//

CLR_RT_ArmEmulator::CLR_RT_ArmEmulator() : ArmEmulator( m_lookup_symdef, m_lookup_symdef_Inverse )
{
    m_fInitialized = false;
}

void CLR_RT_ArmEmulator::InitializeExternalCalls()
{
    CLR_UINT32* orig;
    CLR_UINT32* stub;

    if(m_fInitialized) return;

    m_fMonitorMemory    = (s_CLR_RT_fJitter_Trace_Execution >= c_CLR_RT_Trace_Obnoxious);
    m_fMonitorRegisters = (s_CLR_RT_fJitter_Trace_Execution >= c_CLR_RT_Trace_Obnoxious);
    m_fMonitorOpcodes   = (s_CLR_RT_fJitter_Trace_Execution >= c_CLR_RT_Trace_Verbose  );
    m_fMonitorCalls     = (s_CLR_RT_fJitter_Trace_Execution >= c_CLR_RT_Trace_Info     );

    m_fInitialized       = true;
    m_orig_ExternalCalls = s_JitterExternalCalls;

    m_exitTrigger = 0xDEADBEEF;

    SetInterop( (CLR_UINT32)(size_t)&m_exitTrigger, (ProcessCall)&CLR_RT_ArmEmulator::Interop_ExitTrigger );

#define INTEROP_HANDLER(name)                                                           \
                                                                                        \
    orig = (CLR_UINT32*)&s_JitterExternalCalls.m_fpn__##name;                           \
    stub = (CLR_UINT32*)&m_stub_ExternalCalls .m_fpn__##name;                           \
                                                                                        \
    *orig = (CLR_UINT32)(size_t)stub; *stub = 0xDEADBEEF;                               \
                                                                                        \
    m_lookup_symdef        [ L#name                   ] = (CLR_UINT32)(size_t)stub;     \
    m_lookup_symdef_Inverse[ (CLR_UINT32)(size_t)stub ] = L#name;                       \
                                                                                        \
    SetInterop( (CLR_UINT32)(size_t)stub, (ProcessCall)&CLR_RT_ArmEmulator::Interop_##name )

    //--//

    INTEROP_HANDLER(CLR_RT_HeapBlock__Compare_Values        );

    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericMul            );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericDiv            );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericDivUn          );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericRem            );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericRemUn          );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericShl            );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericShr            );
    INTEROP_HANDLER(CLR_RT_HeapBlock__NumericShrUn          );
    INTEROP_HANDLER(CLR_RT_HeapBlock__InitObject            );
    INTEROP_HANDLER(CLR_RT_HeapBlock__Convert               );

    INTEROP_HANDLER(MethodCompilerHelpers__HandleBoxing     );
    INTEROP_HANDLER(MethodCompilerHelpers__HandleCasting    );
    INTEROP_HANDLER(MethodCompilerHelpers__CopyValueType    );
    INTEROP_HANDLER(MethodCompilerHelpers__CloneValueType   );
    INTEROP_HANDLER(MethodCompilerHelpers__LoadFunction     );
    INTEROP_HANDLER(MethodCompilerHelpers__LoadString       );
    INTEROP_HANDLER(MethodCompilerHelpers__NewArray         );

    INTEROP_HANDLER(MethodCompilerHelpers__Call             );
    INTEROP_HANDLER(MethodCompilerHelpers__CallVirtual      );
    INTEROP_HANDLER(MethodCompilerHelpers__NewObject        );
    INTEROP_HANDLER(MethodCompilerHelpers__NewDelegate      );
    INTEROP_HANDLER(MethodCompilerHelpers__Pop              );

    INTEROP_HANDLER(MethodCompilerHelpers__AccessStaticField);

    INTEROP_HANDLER(MethodCompilerHelpers__Throw            );
    INTEROP_HANDLER(MethodCompilerHelpers__Rethrow          );
    INTEROP_HANDLER(MethodCompilerHelpers__Leave            );
    INTEROP_HANDLER(MethodCompilerHelpers__EndFinally       );

    INTEROP_HANDLER(MethodCompilerHelpers__LoadIndirect     );
    INTEROP_HANDLER(MethodCompilerHelpers__StoreIndirect    );

    INTEROP_HANDLER(MethodCompilerHelpers__LoadObject       );
    INTEROP_HANDLER(MethodCompilerHelpers__CopyObject       );
    INTEROP_HANDLER(MethodCompilerHelpers__StoreObject      );

#undef INTEROP_HANDLER
}

HRESULT CLR_RT_ArmEmulator::Execute( CLR_RT_StackFrame* stack )
{
    TINYCLR_HEADER();

    CLR_UINT32 rgBuffer[ 512 ];

    m_pc                                       = (CLR_UINT32)(size_t)stack->m_nativeMethod;
    m_registers[ ArmProcessor::c_register_r0 ] = (CLR_UINT32)(size_t)stack;
    m_registers[ ArmProcessor::c_register_lr ] = (CLR_UINT32)(size_t)&m_exitTrigger;
    m_registers[ ArmProcessor::c_register_sp ] = (CLR_UINT32)(size_t)&rgBuffer[ ARRAYSIZE(rgBuffer) ];

    TINYCLR_CHECK_HRESULT(ArmEmulator::Execute( 0xFFFFFFFF ));

    TINYCLR_SET_AND_LEAVE(m_registers[ ArmProcessor::c_register_r0 ]);

    TINYCLR_NOCLEANUP();
}

//--//--//--//--//--//--//--//--//--//--//--//--//--//

bool CLR_RT_ArmEmulator::Interop_ExitTrigger()
{
    m_fStopExecution = true;

    return false;
}

//--//--//--//--//

#define INTEROP_BOOL(name,pos)    bool name =             (m_registers[ pos ] != 0)
#define INTEROP_VAL(cls,name,pos) cls  name = (cls)        m_registers[ pos ]
#define INTEROP_PTR(cls,name,pos) cls  name = (cls)(size_t)m_registers[ pos ]

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__Compare_Values()
{
    INTEROP_PTR (CLR_RT_HeapBlock*,left   ,0);
    INTEROP_PTR (CLR_RT_HeapBlock*,right  ,1);
    INTEROP_BOOL(                  fSigned,2);

    m_registers[ 0 ] = CLR_RT_HeapBlock::Compare_Values( *left, *right, fSigned );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__NumericMul()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,right,1);

    m_registers[ 0 ] = pThis->NumericMul( *right );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__NumericDiv()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,right,1);

    m_registers[ 0 ] = pThis->NumericDiv( *right );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__NumericDivUn()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,right,1);

    m_registers[ 0 ] = pThis->NumericDivUn( *right );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__NumericRem()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,right,1);

    m_registers[ 0 ] = pThis->NumericRem( *right );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__NumericRemUn()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,right,1);

    m_registers[ 0 ] = pThis->NumericRemUn( *right );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__NumericShl()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,right,1);

    m_registers[ 0 ] = pThis->NumericShl( *right );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__NumericShr()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,right,1);

    m_registers[ 0 ] = pThis->NumericShr( *right );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__NumericShrUn()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,right,1);

    m_registers[ 0 ] = pThis->NumericShrUn( *right );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__InitObject()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,pThis,0);

    m_registers[ 0 ] = pThis->InitObject();

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_CLR_RT_HeapBlock__Convert()
{
    INTEROP_PTR (CLR_RT_HeapBlock*,ref      ,0);
    INTEROP_VAL (CLR_DataType     ,et       ,1);
    INTEROP_BOOL(                  fOverflow,2);
    INTEROP_BOOL(                  fUnsigned,3);

    m_registers[ 0 ] = ref->Convert( et, fOverflow, fUnsigned );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__HandleBoxing()
{
    INTEROP_PTR (CLR_RT_HeapBlock*,ref ,0);
    INTEROP_VAL (CLR_UINT32       ,type,1);
    INTEROP_BOOL(                  fBox,2);

    m_registers[ 0 ] = MethodCompilerHelpers::HandleBoxing( *ref, type, fBox );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__HandleCasting()
{
    INTEROP_PTR (CLR_RT_HeapBlock*,ref    ,0);
    INTEROP_VAL (CLR_UINT32       ,type   ,1);
    INTEROP_VAL (CLR_UINT32       ,levels ,2);
    INTEROP_BOOL(                  fIsInst,3);

    m_registers[ 0 ] = MethodCompilerHelpers::HandleCasting( *ref, type, levels, fIsInst );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__CopyValueType()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,refDst,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,refSrc,1);

    m_registers[ 0 ] = MethodCompilerHelpers::CopyValueType( *refDst, *refSrc );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__CloneValueType()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,ref,0);

    m_registers[ 0 ] = MethodCompilerHelpers::CloneValueType( *ref );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__LoadFunction()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack      ,0);
    INTEROP_VAL(CLR_UINT32        ,method     ,1);
    INTEROP_PTR(CLR_RT_HeapBlock* ,refDlg     ,2);
    INTEROP_PTR(CLR_RT_HeapBlock* ,ptrInstance,3);

    m_registers[ 0 ] = MethodCompilerHelpers::LoadFunction( stack, method, *refDlg, ptrInstance );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__LoadString()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack ,0);
    INTEROP_VAL(CLR_UINT32        ,string,1);
    INTEROP_PTR(CLR_RT_HeapBlock* ,refStr,2);

    m_registers[ 0 ] = MethodCompilerHelpers::LoadString( stack, string, *refStr );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__NewArray()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,ref   ,0);
    INTEROP_VAL(CLR_UINT32       ,type  ,1);
    INTEROP_VAL(CLR_UINT32       ,levels,2);
    INTEROP_VAL(CLR_UINT32       ,size  ,3);

    m_registers[ 0 ] = MethodCompilerHelpers::NewArray( *ref, type, levels, size );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__Call()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack        ,0);
    INTEROP_VAL(CLR_UINT32        ,method       ,1);
    INTEROP_PTR(CLR_RT_HeapBlock* ,firstArgument,2);

    m_registers[ 0 ] = MethodCompilerHelpers::Call( stack, method, firstArgument );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__CallVirtual()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack        ,0);
    INTEROP_VAL(CLR_UINT32        ,method       ,1);
    INTEROP_PTR(CLR_RT_HeapBlock* ,firstArgument,2);

    m_registers[ 0 ] = MethodCompilerHelpers::CallVirtual( stack, method, firstArgument );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__NewObject()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack        ,0);
    INTEROP_VAL(CLR_UINT32        ,method       ,1);
    INTEROP_PTR(CLR_RT_HeapBlock* ,firstArgument,2);

    m_registers[ 0 ] = MethodCompilerHelpers::NewObject( stack, method, firstArgument );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__NewDelegate()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack        ,0);
    INTEROP_VAL(CLR_UINT32        ,method       ,1);
    INTEROP_PTR(CLR_RT_HeapBlock* ,firstArgument,2);

    m_registers[ 0 ] = MethodCompilerHelpers::NewDelegate( stack, method, firstArgument );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__Pop()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack,0);

    MethodCompilerHelpers::Pop( stack );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__AccessStaticField()
{
    INTEROP_VAL(CLR_UINT32,field,0);

    m_registers[ 0 ] = (CLR_UINT32)(size_t)MethodCompilerHelpers::AccessStaticField( field );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__Throw()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack,0);
    INTEROP_PTR(CLR_RT_HeapBlock* ,ex   ,1);

    m_registers[ 0 ] = MethodCompilerHelpers::Throw( stack, ex );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__Rethrow()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack,0);

    m_registers[ 0 ] = MethodCompilerHelpers::Rethrow( stack );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__Leave()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack,0);
    INTEROP_PTR(CLR_PMETADATA     ,from ,1);
    INTEROP_PTR(CLR_PMETADATA     ,to   ,2);

    m_registers[ 0 ] = (CLR_UINT32)(size_t)MethodCompilerHelpers::Leave( stack, from, to );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__EndFinally()
{
    INTEROP_PTR(CLR_RT_StackFrame*,stack,0);

    m_registers[ 0 ] = MethodCompilerHelpers::EndFinally( stack );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__LoadIndirect()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,ref,0);

    m_registers[ 0 ] = MethodCompilerHelpers::LoadIndirect( *ref );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__StoreIndirect()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,refDst,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,refVal,1);
    INTEROP_VAL(CLR_UINT32       ,op    ,2);

    m_registers[ 0 ] = MethodCompilerHelpers::StoreIndirect( *refDst, *refVal, op );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__LoadObject()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,ref,0);

    m_registers[ 0 ] = MethodCompilerHelpers::LoadObject( *ref );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__CopyObject()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,refDst,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,refSrc,1);

    m_registers[ 0 ] = MethodCompilerHelpers::CopyObject( *refDst, *refSrc );

    return Interop_GenericSkipCall();
}

//--//

bool CLR_RT_ArmEmulator::Interop_MethodCompilerHelpers__StoreObject()
{
    INTEROP_PTR(CLR_RT_HeapBlock*,refDst,0);
    INTEROP_PTR(CLR_RT_HeapBlock*,refVal,1);

    m_registers[ 0 ] = MethodCompilerHelpers::StoreObject( *refDst, *refVal );

    return Interop_GenericSkipCall();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // defined(TINYCLR_JITTER_ARMEMULATION)

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // defined(TINYCLR_JITTER)
