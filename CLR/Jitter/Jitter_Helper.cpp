////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core\Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MethodCompilerHelpers::HandleBoxing( CLR_RT_HeapBlock& ref, CLR_UINT32 type, bool fBox )
{
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Index    td;     td.m_data = type;
    CLR_RT_TypeDef_Instance tdInst; if(tdInst.InitializeFromIndex( td ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    if(fBox)
    {
        TINYCLR_CHECK_HRESULT(ref.PerformBoxing( tdInst ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(ref.PerformUnboxing( tdInst ));

        ref.Promote();
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::HandleCasting( CLR_RT_HeapBlock& ref, CLR_UINT32 type, CLR_UINT32 levels, bool fIsInst )
{
    TINYCLR_HEADER();

    if(ref.DataType() == DATATYPE_OBJECT && ref.Dereference() == NULL)
    {
        ;
    }
    else
    {
        CLR_RT_TypeDescriptor desc;

        if(SUCCEEDED(desc.InitializeFromObject( ref )))
        {
            if(levels == 0)
            {
                //
                // Shortcut for identity.
                //
                if(desc.m_handlerCls.m_data == type)
                {
                    TINYCLR_SET_AND_LEAVE(S_OK);
                }
            }

            {
                CLR_RT_TypeDef_Index  clsTarget; clsTarget.m_data = type;
                CLR_RT_TypeDescriptor descTarget;

                if(SUCCEEDED(descTarget.InitializeFromType( clsTarget )))
                {
                    if(levels)
                    {
                        descTarget.m_reflex.m_levels = levels;

                        descTarget.ConvertToArray();
                    }

                    if(CLR_RT_ExecutionEngine::IsInstanceOf( desc, descTarget ))
                    {
                        TINYCLR_SET_AND_LEAVE(S_OK);
                    }
                }
            }
        }

        if(fIsInst == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_CAST);
        }

        ref.SetObjectReference( NULL );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::CopyValueType( CLR_RT_HeapBlock& refDst, const CLR_RT_HeapBlock& refSrc )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* dst;
    CLR_RT_HeapBlock* src;

    dst = refDst.Dereference(); FAULT_ON_NULL(dst);
    src = refSrc.Dereference(); FAULT_ON_NULL(src);

    TINYCLR_SET_AND_LEAVE(g_CLR_RT_ExecutionEngine.CopyValueType( dst, src ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::CloneValueType( CLR_RT_HeapBlock& ref )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(g_CLR_RT_ExecutionEngine.CloneObject( ref, ref ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::LoadFunction( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock& refDlg, CLR_RT_HeapBlock* ptrInstance )
{
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Index md; md.m_data = method;

    if(ptrInstance)
    {
        CLR_RT_TypeDef_Index   cls;
        CLR_RT_MethodDef_Index mdReal;

        TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( *ptrInstance, cls ));

        if(g_CLR_RT_EventCache.FindVirtualMethod( cls, md, mdReal ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }

        md = mdReal;
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Delegate::CreateInstance( refDlg, md, stack ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::LoadString( CLR_RT_StackFrame* stack, CLR_UINT32 string, CLR_RT_HeapBlock& refStr )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_String::CreateInstance( refStr, string, stack->m_call.m_assm ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::NewArray( CLR_RT_HeapBlock& ref, CLR_UINT32 type, CLR_UINT32 levels, CLR_UINT32 size )
{
    TINYCLR_HEADER();

    CLR_RT_ReflectionDef_Index reflex;

    reflex.m_kind               = REFLECTION_TYPE;
    reflex.m_levels             = levels+1;
    reflex.m_data.m_type.m_data = type;

    for(int pass=0; pass<2; pass++)
    {
        hr = CLR_RT_HeapBlock_Array::CreateInstance( ref, size, reflex ); if(SUCCEEDED(hr)) break;

        if(hr != CLR_E_OUT_OF_MEMORY) TINYCLR_SET_AND_LEAVE(hr);

        g_CLR_RT_ExecutionEngine.PerformHeapCompaction();
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::Call( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument )
{
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Index    md    ; md.m_data = method;
    CLR_RT_MethodDef_Instance mdInst; mdInst.InitializeFromIndex( md );

    TINYCLR_CHECK_HRESULT(CLR_RT_StackFrame::Push( stack->m_owningThread, mdInst, -1 ));

    if(stack->Callee()->m_flags & CLR_RT_StackFrame::c_CallerIsCompatibleForCall)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_SET_AND_LEAVE(CLR_S_RESTART_EXECUTION);

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::CallVirtual( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument )
{
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Index    md    ; md.m_data = method;
    CLR_RT_MethodDef_Instance mdInst; mdInst.InitializeFromIndex( md );

    if(mdInst.m_target->flags & CLR_RECORD_METHODDEF::MD_DelegateInvoke)
    {
        CLR_RT_HeapBlock_Delegate* dlg = firstArgument->DereferenceDelegate(); FAULT_ON_NULL(dlg);

        if(dlg->DataType() == DATATYPE_DELEGATE_HEAD)
        {
            mdInst.InitializeFromIndex( dlg->DelegateFtn() );

            if((mdInst.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) == 0)
            {
                firstArgument->Assign( dlg->m_object );
            }
            else
            {
                memmove( &firstArgument[ 0 ], &firstArgument[ 1 ], mdInst.m_target->numArgs * sizeof(CLR_RT_HeapBlock) );

                stack->m_evalStackPos--;
            }
        }
        else
        {
            //
            // The lookup for multicast delegates is done at a later stage...
            //
        }
    }
    else
    {
        CLR_RT_TypeDef_Index   cls;
        CLR_RT_MethodDef_Index mdReal;

        TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( firstArgument[ 0 ], cls ));

        if(g_CLR_RT_EventCache.FindVirtualMethod( cls, md, mdReal ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }

        mdInst.InitializeFromIndex( mdReal );
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_StackFrame::Push( stack->m_owningThread, mdInst, -1 ));

    if(stack->Callee()->m_flags & CLR_RT_StackFrame::c_CallerIsCompatibleForCall)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_SET_AND_LEAVE(CLR_S_RESTART_EXECUTION);

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::NewObject( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument )
{
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Index    md    ; md.m_data = method;
    CLR_RT_MethodDef_Instance mdInst; mdInst.InitializeFromIndex( md );

    CLR_RT_TypeDef_Instance cls; cls.InitializeFromMethod( mdInst ); // This is the class to create!

    //
    // We have to insert the 'this' pointer as argument 0, that means moving all the arguments up one slot...
    //
    memmove( &firstArgument[ 1 ], &firstArgument[ 0 ], (CLR_UINT8*)stack->m_evalStackPos - (CLR_UINT8*)firstArgument ); stack->m_evalStackPos++;

    firstArgument->SetObjectReference( NULL );

    //--//

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObject( firstArgument[ 0 ], cls ));

    //
    // This is to flag the fact that we need to copy back the 'this' pointer into our stack.
    //
    // See CLR_RT_StackFrame::Pop()
    //
    stack->m_flags |= CLR_RT_StackFrame::c_ExecutingConstructor;

    //
    // Ok, creating a ValueType then calls its constructor.
    // But the constructor will try to load the 'this' pointer and since it's a value type, it will be cloned.
    // For the length of the constructor, change the type from an object pointer to a reference.
    //
    // See CLR_RT_StackFrame::Pop()
    //
    if((cls.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_ValueType)
    {
        if(firstArgument[ 0 ].DataType() == DATATYPE_OBJECT)
        {
            firstArgument[ 0 ].ChangeDataType( DATATYPE_BYREF );
        }
        else
        {
            //
            // This is to support the optimization on DateTime and TimeSpan:
            //
            // These are passed as built-ins. But we need to pass them as a reference,
            // so push everything down and undo the "ExecutingConstructor" trick.
            //
            memmove( &firstArgument[ 1 ], &firstArgument[ 0 ], (CLR_UINT8*)stack->m_evalStackPos - (CLR_UINT8*)firstArgument ); stack->m_evalStackPos++;

            firstArgument[ 1 ].SetReference( firstArgument[ 0 ] );

            stack->m_flags &= ~CLR_RT_StackFrame::c_ExecutingConstructor;
        }
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_StackFrame::Push( stack->m_owningThread, mdInst, -1 ));

    if(stack->Callee()->m_flags & CLR_RT_StackFrame::c_CallerIsCompatibleForCall)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_SET_AND_LEAVE(CLR_S_RESTART_EXECUTION);

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::NewDelegate( CLR_RT_StackFrame* stack, CLR_UINT32 method, CLR_RT_HeapBlock* firstArgument )
{
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Index    md    ; md.m_data = method;
    CLR_RT_MethodDef_Instance mdInst; mdInst.InitializeFromIndex( md );

    CLR_RT_TypeDef_Instance cls; cls.InitializeFromMethod( mdInst ); // This is the class to create!

    if(firstArgument[ 1 ].DataType() != DATATYPE_OBJECT)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
    else
    {
        CLR_RT_HeapBlock_Delegate* dlg = firstArgument[ 1 ].DereferenceDelegate();

        if(dlg == NULL)
        {
            TINYCLR_CHECK_HRESULT(CLR_E_NULL_REFERENCE);
        }

        dlg->m_cls = cls;

        CLR_RT_MethodDef_Instance dlgInst;

        if(dlgInst.InitializeFromIndex( dlg->DelegateFtn() ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }

        if((dlgInst.m_target->flags & CLR_RECORD_METHODDEF::MD_Static) == 0)
        {
            dlg->m_object.Assign( firstArgument[ 0 ] );
        }

        firstArgument->SetObjectReference( dlg );
    }

    TINYCLR_NOCLEANUP();
}

void MethodCompilerHelpers::Pop( CLR_RT_StackFrame* stack )
{
    stack->Pop();
}

CLR_RT_HeapBlock* MethodCompilerHelpers::AccessStaticField( CLR_UINT32 field )
{
    CLR_RT_FieldDef_Index fd; fd.m_data = field;

    return CLR_RT_ExecutionEngine::AccessStaticField( fd );
}

//--//

HRESULT MethodCompilerHelpers::Throw( CLR_RT_StackFrame* stack, CLR_RT_HeapBlock* ex )
{
    TINYCLR_HEADER();

    CLR_RT_Thread* th = stack->m_owningThread;

    th->m_currentException.Assign( *ex );

    Library_corlib_native_System_Exception::SetStackTrace( th->m_currentException, stack );

    TINYCLR_CHECK_HRESULT(CLR_E_PROCESS_EXCEPTION);

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::Rethrow( CLR_RT_StackFrame* stack )
{
    TINYCLR_HEADER();

    CLR_RT_Thread* th = stack->m_owningThread;

    if(th->m_nestedExceptionsPos == 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_STACK_UNDERFLOW);
    }
    else
    {
        CLR_RT_Thread::UnwindStack& us = th->m_nestedExceptions[ --th->m_nestedExceptionsPos ];

        if(us.m_exception)
        {
            th->m_currentException.SetObjectReference( us.m_exception );

            TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_STACK_UNDERFLOW);
        }
    }

    TINYCLR_NOCLEANUP();
}

CLR_PMETADATA MethodCompilerHelpers::Leave( CLR_RT_StackFrame* stack, CLR_PMETADATA from, CLR_PMETADATA to )
{
    CLR_RT_Thread*          th = stack->m_owningThread;
    CLR_RT_ExceptionHandler eh;

    stack->ResetStack();

    th->PopEH( stack, to );

    if(th->FindEhBlock( stack, from, to, eh ))
    {
        CLR_RT_Thread::UnwindStack* us = th->PushEH();
        if(us)
        {
            us->m_stack     = stack;
            us->m_exception = NULL;
            us->m_ip        = to;
            us->m_eh        = eh;

            return us->m_eh.m_handlerStart;
        }
    }

    return to;
}

HRESULT MethodCompilerHelpers::EndFinally( CLR_RT_StackFrame* stack )
{
    TINYCLR_HEADER();

    CLR_RT_Thread* th = stack->m_owningThread;

    if(th->m_nestedExceptionsPos == 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_STACK_UNDERFLOW);
    }
    else
    {
        CLR_RT_Thread::UnwindStack& us = th->m_nestedExceptions[ --th->m_nestedExceptionsPos ];

        if(us.m_ip)
        {
            stack->m_IP = us.m_ip;
        }
        else if(us.m_exception)
        {
            th->m_currentException.SetObjectReference( us.m_exception );

            TINYCLR_SET_AND_LEAVE(CLR_E_PROCESS_EXCEPTION);
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_STACK_UNDERFLOW);
        }
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompilerHelpers::LoadIndirect( CLR_RT_HeapBlock& ref )
{
    TINYCLR_HEADER();

    //
    // Save the pointer to the object to load/copy and protect it from GC.
    //
    CLR_RT_HeapBlock     safeSource; safeSource.Assign( ref );
    CLR_RT_ProtectFromGC gc( safeSource );

    TINYCLR_CHECK_HRESULT(ref.LoadFromReference( safeSource ));

    ref.Promote();

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::StoreIndirect( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refVal, CLR_UINT32 op )
{
    TINYCLR_HEADER();

    int size;

    //
    // Reassign will make sure these are objects of the same type.
    //
    switch(op)
    {
    case CEE_STIND_I  : size = 4; break;
    case CEE_STIND_I1 : size = 1; break;
    case CEE_STIND_I2 : size = 2; break;
    case CEE_STIND_I4 : size = 4; break;
    case CEE_STIND_I8 : size = 8; break;
    case CEE_STIND_R4 : size = 4; break;
    case CEE_STIND_R8 : size = 8; break;
    case CEE_STIND_REF: size = 0; break;
    }

    refVal.Promote();

    TINYCLR_CHECK_HRESULT(refVal.StoreToReference( refDst, size ));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompilerHelpers::LoadObject( CLR_RT_HeapBlock& ref )
{
    TINYCLR_HEADER();

    //
    // Save the pointer to the object to load/copy and protect it from GC.
    //
    CLR_RT_HeapBlock     safeSource; safeSource.Assign( ref );
    CLR_RT_ProtectFromGC gc( safeSource );

    TINYCLR_CHECK_HRESULT(ref.LoadFromReference( safeSource ));

    ref.Promote();

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::CopyObject( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refSrc )
{
    TINYCLR_HEADER();

    //
    // Reassign will make sure these are objects of the same type.
    //
    TINYCLR_CHECK_HRESULT(refDst.Reassign( refSrc ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompilerHelpers::StoreObject( CLR_RT_HeapBlock& refDst, CLR_RT_HeapBlock& refVal )
{
    TINYCLR_HEADER();

    //
    // Reassign will make sure these are objects of the same type.
    //
    TINYCLR_CHECK_HRESULT(refDst.Reassign( refVal ));

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MethodCompiler::Arm_SetShift_Immediate( CLR_UINT32 shiftType, CLR_UINT32 shiftValue )
{
    TINYCLR_HEADER();

    switch(shiftType)
    {
    case ArmProcessor::c_shift_LSL: if(                   shiftValue >= 32) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER); break;
    case ArmProcessor::c_shift_LSR: if(shiftValue <  1 || shiftValue >  32) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER); break;
    case ArmProcessor::c_shift_ASR: if(shiftValue <  1 || shiftValue >  32) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER); break;
    case ArmProcessor::c_shift_ROR: if(shiftValue <  1 || shiftValue >= 32) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER); break;
    case ArmProcessor::c_shift_RRX: if(shiftValue != 1                    ) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER); break;
    default                       :                                         TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    m_Arm_shiftType   = shiftType;
    m_Arm_shiftValue  = shiftValue;
    m_Arm_shiftUseReg = false;

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_SetShift_Register( CLR_UINT32 shiftType, CLR_UINT32 shiftReg )
{
    TINYCLR_HEADER();

    if(shiftReg >= 15)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    switch(shiftType)
    {
    case ArmProcessor::c_shift_LSL: break;
    case ArmProcessor::c_shift_LSR: break;
    case ArmProcessor::c_shift_ASR: break;
    case ArmProcessor::c_shift_ROR: break;
    case ArmProcessor::c_shift_RRX: break;
    default                       : TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    m_Arm_shiftType   = shiftType;
    m_Arm_shiftReg    = shiftReg;
    m_Arm_shiftUseReg = true;

    TINYCLR_NOCLEANUP();
}

void MethodCompiler::Arm_Reset()
{
    m_Arm_Op.m_fmt            = ArmProcessor::Opcode::Undefined;
    m_Arm_Op.m_conditionCodes = ArmProcessor::c_cond_AL;
    m_Arm_shiftUseReg         = false;
    m_Arm_shiftType           = ArmProcessor::c_shift_LSL;
    m_Arm_shiftValue          = 0;
    m_Arm_shiftReg            = 0;
    m_Arm_setCC               = false;
}

HRESULT MethodCompiler::Arm_Emit( CLR_UINT32 data )
{
    TINYCLR_HEADER();

    CLR_UINT32* ptr = m_Arm_Opcodes.Push(); CHECK_ALLOCATION(ptr);

    *ptr = data;

    Arm_Reset();

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_EmitData( CLR_UINT32 data )
{
    TINYCLR_HEADER();

#if defined(TINYCLR_DUMP_JITTER_INLINE)
    if(m_fDump_JitterInline)
    {
        CLR_Debug::Printf( "0x%08x:  %08x    ", (CLR_UINT32)(Arm_CurrentAbsolutePC()), data );

        for(size_t pos=0; pos<4; pos++)
        {
            CLR_UINT32 c = (data >> (8*pos)) & 0xFF;

            CLR_Debug::Printf( "%c", (c <= 0x1F || c >= 0x7F) ? '.' : c );
        }

        CLR_Debug::Printf( "    DCD      0x%08X\r\n", data );
    }
#endif

    TINYCLR_SET_AND_LEAVE(Arm_Emit( data ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_EmitOpcode()
{
    TINYCLR_HEADER();

#if defined(TINYCLR_DUMP_JITTER_INLINE)
    if(m_fDump_JitterInline)
    {
        ArmProcessor::Opcode::Print( Arm_CurrentAbsolutePC(), m_Arm_Op.Encode() );
    }
#endif

    TINYCLR_SET_AND_LEAVE(Arm_Emit( m_Arm_Op.Encode() ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_AddData( CLR_UINT32 data, CLR_UINT32& address )
{
    TINYCLR_HEADER();

    CLR_UINT32* ptr;
    size_t      tot = m_Arm_ROData.Size();

    for(size_t pos=0; pos<tot; pos++)
    {
        if(m_Arm_ROData[ pos ] == data)
        {
            address = (CLR_UINT32)(m_Arm_ROData_Base + pos * sizeof(CLR_UINT32));

            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }

    ptr = m_Arm_ROData.Push(); CHECK_ALLOCATION(ptr);

    *ptr = data;

    address = (CLR_UINT32)(m_Arm_ROData_Base + tot * sizeof(CLR_UINT32));

    TINYCLR_NOCLEANUP();
}

//--//--//

CLR_INT32 MethodCompiler::Arm_GetOpcodeOperandOffset( OpcodeSlot* osPtr, CLR_UINT32 idx, CLR_INT32 offset )
{
    return offset + (osPtr->m_stackDepth - osPtr->m_stackPop + idx) * sizeof(CLR_RT_HeapBlock);
}

HRESULT MethodCompiler::Arm_GetOpcodeOperand( OpcodeSlot* osPtr, CLR_UINT32 idx, CLR_UINT32 Rdst, CLR_INT32 offset )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(Arm_ADD_IMM( Rdst, ArmProcessor::c_register_r6, Arm_GetOpcodeOperandOffset( osPtr, idx, offset ) ));

    TINYCLR_NOCLEANUP();
}

//--//--//

HRESULT MethodCompiler::Arm_Alu( CLR_UINT32 alu, CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_UINT32 Rop2 )
{
    TINYCLR_HEADER();

    if(m_Arm_shiftUseReg)
    {
        if(m_Arm_Op.Prepare_DataProcessing_3( alu, m_Arm_setCC, Rdst, Rop1, Rop2, m_Arm_shiftType, m_Arm_shiftReg ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }
    else
    {
        if(m_Arm_Op.Prepare_DataProcessing_2( alu, m_Arm_setCC, Rdst, Rop1, Rop2, m_Arm_shiftType, m_Arm_shiftValue ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_Alu_IMM( CLR_UINT32 alu, CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_DataProcessing_1( alu, m_Arm_setCC, Rdst, Rop1, (CLR_UINT32)Vop2 ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

//--//--//

HRESULT MethodCompiler::Arm_MOV_IMM( CLR_UINT32 Rdst, CLR_INT32 Vsrc )
{
    TINYCLR_HEADER();

    CLR_UINT32 imm;
    CLR_UINT32 rot;

    if(ArmProcessor::Opcode::check_DataProcessing_ImmediateValue( (CLR_UINT32)Vsrc, imm, rot ))
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_MOV, Rdst, 0, Vsrc ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_MVN, Rdst, 0, -Vsrc ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_ADD_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 )
{
    TINYCLR_HEADER();

    if(Vop2 < 0)
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_SUB, Rdst, Rop1, -Vop2 ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_ADD, Rdst, Rop1, Vop2 ));
    }

    TINYCLR_NOCLEANUP();
}


HRESULT MethodCompiler::Arm_SUB_IMM( CLR_UINT32 Rdst, CLR_UINT32 Rop1, CLR_INT32 Vop2 )
{
    TINYCLR_HEADER();

    if(Vop2 < 0)
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_ADD, Rdst, Rop1, -Vop2 ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_SUB, Rdst, Rop1, Vop2 ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_CMP_IMM( CLR_UINT32 Rop1, CLR_INT32 Vop2 )
{
    TINYCLR_HEADER();

    m_Arm_setCC = true;

    if(Vop2 < 0)
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_CMN, 0, Rop1, -Vop2 ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_CMP, 0, Rop1, Vop2 ));
    }

    TINYCLR_NOCLEANUP();
}

//--//--//--//

HRESULT MethodCompiler::Arm_STMFD_SP( CLR_UINT32 lst )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_BlockDataTransfer( ArmProcessor::c_register_sp, ArmProcessor::c_Store, ArmProcessor::c_PreIndex, ArmProcessor::c_Down, ArmProcessor::c_WriteBack, lst ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LDMFD_SP( CLR_UINT32 lst )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_BlockDataTransfer( ArmProcessor::c_register_sp, ArmProcessor::c_Load, ArmProcessor::c_PostIndex, ArmProcessor::c_Up, ArmProcessor::c_WriteBack, lst ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompiler::Arm_LDMIA( CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 lst, CLR_UINT32 Rtmp )
{
    TINYCLR_HEADER();

    if(offset == 4)
    {
        TINYCLR_CHECK_HRESULT(Arm_LDMIB( Raddress, lst ));
    }
    else if(offset == 0)
    {
        TINYCLR_CHECK_HRESULT(Arm_LDMIA( Raddress, lst ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( Rtmp, Raddress, offset ));
        TINYCLR_CHECK_HRESULT(Arm_LDMIA  ( Rtmp, lst              ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LDMIA( CLR_UINT32 Raddress, CLR_UINT32 lst )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_BlockDataTransfer( Raddress, ArmProcessor::c_Load, ArmProcessor::c_PostIndex, ArmProcessor::c_Up, ArmProcessor::c_NoWriteBack, lst ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_STMIA( CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 lst, CLR_UINT32 Rtmp )
{
    TINYCLR_HEADER();

    if(offset == 4)
    {
        TINYCLR_CHECK_HRESULT(Arm_STMIB( Raddress, lst ));
    }
    else if(offset == 0)
    {
        TINYCLR_CHECK_HRESULT(Arm_STMIA( Raddress, lst ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( Rtmp, Raddress, offset ));
        TINYCLR_CHECK_HRESULT(Arm_STMIA  ( Rtmp, lst              ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_STMIA( CLR_UINT32 Raddress, CLR_UINT32 lst )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_BlockDataTransfer( Raddress, ArmProcessor::c_Store, ArmProcessor::c_PostIndex, ArmProcessor::c_Up, ArmProcessor::c_NoWriteBack, lst ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompiler::Arm_LDMIB( CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 lst, CLR_UINT32 Rtmp )
{
    TINYCLR_HEADER();

    if(offset == -4)
    {
        TINYCLR_CHECK_HRESULT(Arm_LDMIA( Raddress, lst ));
    }
    else if(offset == 0)
    {
        TINYCLR_CHECK_HRESULT(Arm_LDMIB( Raddress, lst ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( Rtmp, Raddress, offset ));
        TINYCLR_CHECK_HRESULT(Arm_LDMIB  ( Rtmp, lst              ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LDMIB( CLR_UINT32 Raddress, CLR_UINT32 lst )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_BlockDataTransfer( Raddress, ArmProcessor::c_Load, ArmProcessor::c_PreIndex, ArmProcessor::c_Up, ArmProcessor::c_NoWriteBack, lst ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_STMIB( CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 lst, CLR_UINT32 Rtmp )
{
    TINYCLR_HEADER();

    if(offset == -4)
    {
        TINYCLR_CHECK_HRESULT(Arm_STMIA( Raddress, lst ));
    }
    else if(offset == 0)
    {
        TINYCLR_CHECK_HRESULT(Arm_STMIB( Raddress, lst ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( Rtmp, Raddress, offset ));
        TINYCLR_CHECK_HRESULT(Arm_STMIB  ( Rtmp, lst              ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_STMIB( CLR_UINT32 Raddress, CLR_UINT32 lst )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_BlockDataTransfer( Raddress, ArmProcessor::c_Store, ArmProcessor::c_PreIndex, ArmProcessor::c_Up, ArmProcessor::c_NoWriteBack, lst ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompiler::Arm_LDR_REG( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_UINT32 Rindex )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_SingleDataTransfer_2( Raddress, ArmProcessor::c_Load, ArmProcessor::c_PreIndex, ArmProcessor::c_Up, ArmProcessor::c_NoWriteBack, Rvalue, ArmProcessor::c_Word, Rindex, m_Arm_shiftType, m_Arm_shiftValue ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LDR( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_INT32 offset )
{
    TINYCLR_HEADER();

    bool       fUp;
    CLR_UINT32 value;

    if(offset < 0)
    {
        fUp   =             ArmProcessor::c_Down;
        value = (CLR_UINT32)-offset;
    }
    else
    {
        fUp   =             ArmProcessor::c_Up;
        value = (CLR_UINT32)offset;
    }

    if(value > 0xFFF)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    if(m_Arm_Op.Prepare_SingleDataTransfer_1( Raddress, ArmProcessor::c_Load, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, ArmProcessor::c_Word, value ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LDR( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 size, bool fSigned )
{
    TINYCLR_HEADER();

    bool       fUp;
    CLR_UINT32 value;

    if(offset < 0)
    {
        fUp   =             ArmProcessor::c_Down;
        value = (CLR_UINT32)-offset;
    }
    else
    {
        fUp   =             ArmProcessor::c_Up;
        value = (CLR_UINT32)offset;
    }

    switch(size)
    {
    case 1:
        if(!fSigned)
        {
            if(value > 0xFFF)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            if(m_Arm_Op.Prepare_SingleDataTransfer_1( Raddress, ArmProcessor::c_Load, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, ArmProcessor::c_Byte, value ) == false)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }
        else
        {
            if(value > 0xFF)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            if(m_Arm_Op.Prepare_HalfwordDataTransfer_2( Raddress, ArmProcessor::c_Load, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, ArmProcessor::c_halfwordkind_I1, value ) == false)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }
        break;

    case 2:
        if(value > 0xFF)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        if(m_Arm_Op.Prepare_HalfwordDataTransfer_2( Raddress, ArmProcessor::c_Load, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, fSigned ? ArmProcessor::c_halfwordkind_I2 : ArmProcessor::c_halfwordkind_U2, value ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
        break;

    case 4:
        if(value > 0xFFF)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        if(m_Arm_Op.Prepare_SingleDataTransfer_1( Raddress, ArmProcessor::c_Load, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, ArmProcessor::c_Word, value ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LDR_PC( CLR_UINT32 Rvalue, CLR_UINT32 offset )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(Arm_LDR( Rvalue, ArmProcessor::c_register_pc, offset - Arm_CurrentRelativePC() - 8));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompiler::Arm_STR( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_INT32 offset )
{
    TINYCLR_HEADER();

    bool       fUp;
    CLR_UINT32 value;

    if(offset < 0)
    {
        fUp   =             ArmProcessor::c_Down;
        value = (CLR_UINT32)-offset;
    }
    else
    {
        fUp   =             ArmProcessor::c_Up;
        value = (CLR_UINT32)offset;
    }

    if(value > 0xFFF)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    if(m_Arm_Op.Prepare_SingleDataTransfer_1( Raddress, ArmProcessor::c_Store, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, ArmProcessor::c_Word, value ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_STR( CLR_UINT32 Rvalue, CLR_UINT32 Raddress, CLR_INT32 offset, CLR_UINT32 size, bool fSigned )
{
    TINYCLR_HEADER();

    bool       fUp;
    CLR_UINT32 value;

    if(offset < 0)
    {
        fUp   =             ArmProcessor::c_Down;
        value = (CLR_UINT32)-offset;
    }
    else
    {
        fUp   =             ArmProcessor::c_Up;
        value = (CLR_UINT32)offset;
    }

    switch(size)
    {
    case 1:
        if(value > 0xFFF)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        if(m_Arm_Op.Prepare_SingleDataTransfer_1( Raddress, ArmProcessor::c_Store, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, ArmProcessor::c_Byte, value ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
        break;

    case 2:
        if(value > 0xFF)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        if(m_Arm_Op.Prepare_HalfwordDataTransfer_2( Raddress, ArmProcessor::c_Store, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, fSigned ? ArmProcessor::c_halfwordkind_I2 : ArmProcessor::c_halfwordkind_U2, value ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
        break;

    case 4:
        if(value > 0xFFF)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        if(m_Arm_Op.Prepare_SingleDataTransfer_1( Raddress, ArmProcessor::c_Store, ArmProcessor::c_PreIndex, fUp, ArmProcessor::c_NoWriteBack, Rvalue, ArmProcessor::c_Word, value ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompiler::Arm_MUL3( CLR_UINT32 Rdst, CLR_UINT32 Rop )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_DataProcessing_2( ArmProcessor::c_operation_ADD, ArmProcessor::c_IgnoreCC, Rdst, Rop, Rop, ArmProcessor::c_shift_LSL, 1 ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_B( bool fLink, CLR_INT32 offset )
{
    TINYCLR_HEADER();

    if(m_Arm_Op.Prepare_Branch( offset, fLink ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_SET_AND_LEAVE(Arm_EmitOpcode());

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompiler::Arm_LongMovIMM( CLR_UINT32 Rdst, CLR_UINT32 value )
{
    TINYCLR_HEADER();

    CLR_UINT32 rot    = 0;
    bool       fFirst = true;

    while(value)
    {
        if(value & 3)
        {
            if(fFirst)
            {
                TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_MOV, Rdst, 0, (value & 0xFF) << rot ));

                fFirst = false;
            }
            else
            {
                TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_ORR, Rdst, Rdst, (value & 0xFF) << rot ));
            }

            value >>= 8;
            rot    += 8;
        }
        else
        {
            value >>= 2;
            rot    += 2;
        }
    }

    if(fFirst)
    {
        TINYCLR_CHECK_HRESULT(Arm_Alu_IMM( ArmProcessor::c_operation_MOV, Rdst, 0, 0 ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LongBranch( CLR_UINT32 address, bool fLink )
{
    TINYCLR_HEADER();

    if(fLink)
    {
        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_lr, ArmProcessor::c_register_pc, 4 ));
    }

    TINYCLR_CHECK_HRESULT(Arm_LDR     ( ArmProcessor::c_register_pc, ArmProcessor::c_register_pc, -4 ));
    TINYCLR_CHECK_HRESULT(Arm_EmitData( address                                                      ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_BranchForwardOrBackward( size_t posFrom, size_t posTo )
{
    TINYCLR_HEADER();

    CLR_UINT32 dst = m_indexToArmOpcode[ posTo ];

    if(posFrom < posTo)
    {
        if(m_opcodeSlots[ posTo ].m_flags & OpcodeSlot::c_BranchBackward)
        {
            dst += c_sizeOfTimeQuantumCode;
        }
    }

    TINYCLR_CHECK_HRESULT(Arm_B( ArmProcessor::c_NoLink, Arm_RelativeOffset( dst ) ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_FaultOnNull( size_t pos, CLR_UINT32 Rd )
{
    TINYCLR_HEADER();

    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_CMP_IMM( Rd, 0                                             ));
    Arm_SetCond( ArmProcessor::c_cond_EQ ); TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r0, CLR_E_NULL_REFERENCE ));
    Arm_SetCond( ArmProcessor::c_cond_EQ ); TINYCLR_CHECK_HRESULT(Arm_Ret    ( pos, false                                        ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_CheckHResult( size_t pos )
{
    TINYCLR_HEADER();

    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_TST_IMM( ArmProcessor::c_register_r0, 0x80000000 ));
    Arm_SetCond( ArmProcessor::c_cond_NE ); TINYCLR_CHECK_HRESULT(Arm_Ret    ( pos, true                               ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_Ret( size_t pos, bool fFlush )
{
    TINYCLR_HEADER();

    if(fFlush)
    {
        CLR_UINT32  cond   = Arm_GetCond();
        OpcodeSlot* osPtr  = &m_opcodeSlots[ pos ];
        CLR_UINT32  offset = Arm_GetEndOfEvalStack( osPtr );

        Arm_SetCond( cond ); TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r1, offset                                                 ));
        Arm_SetCond( cond ); TINYCLR_CHECK_HRESULT(Arm_B      ( ArmProcessor::c_Link, Arm_AbsoluteOffset( m_Arm_Thunks->m_address__Internal_Error ) ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_B( ArmProcessor::c_Link, Arm_AbsoluteOffset( m_Arm_Thunks->m_address__Internal_ErrorNoFlush ) ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_FlushEvalStackPointer( size_t pos, CLR_UINT32 Rtmp )
{
    TINYCLR_HEADER();

    OpcodeSlot* osPtr  = &m_opcodeSlots[ pos ];
    CLR_UINT32  offset = Arm_GetEndOfEvalStack( osPtr );

    if(offset)
    {
        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( Rtmp, ArmProcessor::c_register_r6, offset                                     ));
        TINYCLR_CHECK_HRESULT(Arm_STR    ( Rtmp, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_evalStackPos) ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_STR( ArmProcessor::c_register_r6, ArmProcessor::c_register_r4, offsetof(CLR_RT_StackFrame,m_evalStackPos) ));
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MethodCompiler::Arm_CheckArrayAccess( size_t pos, CLR_RT_TypeDescriptor& td, const CLR_RT_DataTypeLookup*& dtlRes )
{
    TINYCLR_HEADER();

    OpcodeSlot*           osPtr = &m_opcodeSlots[ pos ];
    CLR_INT32             offsetArray = Arm_GetOpcodeOperandOffset( osPtr, 0, offsetof(CLR_RT_HeapBlock,m_data.objectReference.ptr) );
    CLR_INT32             offsetIndex = Arm_GetOpcodeOperandOffset( osPtr, 1, offsetof(CLR_RT_HeapBlock,m_data.numeric            ) );
    CLR_RT_TypeDescriptor tdSub;

    if(td.GetElementType( tdSub ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    dtlRes = &c_CLR_RT_DataTypeLookup[ tdSub.GetDataType() ];

    TINYCLR_CHECK_HRESULT(Arm_LDR        (      ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, offsetArray ));
    TINYCLR_CHECK_HRESULT(Arm_FaultOnNull( pos, ArmProcessor::c_register_r1                                           ));

    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR    ( ArmProcessor::c_register_r2, ArmProcessor::c_register_r6, offsetIndex                                       ));
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_LDR    ( ArmProcessor::c_register_r3, ArmProcessor::c_register_r1, offsetof(CLR_RT_HeapBlock_Array,m_numOfElements ) ));
    /*************************************/ TINYCLR_CHECK_HRESULT(Arm_CMP    ( ArmProcessor::c_register_r3, ArmProcessor::c_register_r2                                                    ));
    Arm_SetCond( ArmProcessor::c_cond_LS ); TINYCLR_CHECK_HRESULT(Arm_MOV_IMM( ArmProcessor::c_register_r0, CLR_E_OUT_OF_RANGE                                                             ));
    Arm_SetCond( ArmProcessor::c_cond_LS ); TINYCLR_CHECK_HRESULT(Arm_Ret    ( pos, false                                                                                                  ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_FindArrayElement( size_t pos, const CLR_RT_DataTypeLookup* dtl )
{
    TINYCLR_HEADER();

    CLR_UINT32 idxReg = ArmProcessor::c_register_r2;

    switch(dtl->m_sizeInBytes)
    {
    case 1:
        break;

    case 2:
        Arm_SetShift_Immediate( ArmProcessor::c_shift_LSL, 1 );
        break;

    case 4:
        Arm_SetShift_Immediate( ArmProcessor::c_shift_LSL, 2 );
        break;

    case 8:
        Arm_SetShift_Immediate( ArmProcessor::c_shift_LSL, 3 );
        break;

    case 12:
        idxReg = ArmProcessor::c_register_r3;

        TINYCLR_CHECK_HRESULT(Arm_MUL3( ArmProcessor::c_register_r3, ArmProcessor::c_register_r2 ));

        Arm_SetShift_Immediate( ArmProcessor::c_shift_LSL, 2 );
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);
    }

    TINYCLR_CHECK_HRESULT(Arm_ADD    ( ArmProcessor::c_register_r3, ArmProcessor::c_register_r1, idxReg                         ));
    TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r3, ArmProcessor::c_register_r3, sizeof(CLR_RT_HeapBlock_Array) ));

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LoadElementAddress( size_t pos, const CLR_RT_DataTypeLookup* dtl )
{
    TINYCLR_HEADER();

    OpcodeSlot* osPtr = &m_opcodeSlots[ pos ];
    CLR_INT32   op    = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

    if((dtl->m_flags & CLR_RT_DataTypeLookup::c_SemanticMask) == CLR_RT_DataTypeLookup::c_ValueType)
    {
        TINYCLR_CHECK_HRESULT(Arm_FindArrayElement( pos, dtl ));

        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_BYREF, 0, 1 )                                                                                    ));

        if(dtl->m_flags & CLR_RT_DataTypeLookup::c_OptimizedValueType)
        {
            TINYCLR_CHECK_HRESULT(Arm_MOV( ArmProcessor::c_register_r1, ArmProcessor::c_register_r3 ));
        }
        else
        {
            TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r1, ArmProcessor::c_register_r3, offsetof(CLR_RT_HeapBlock,m_data.objectReference.ptr) ));
        }

        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r3 ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_ARRAY_BYREF, 0, 1 )                                                                                ));
        TINYCLR_CHECK_HRESULT(Arm_STMIA     ( ArmProcessor::c_register_r6, op, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3 ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_LoadElement( size_t pos, const CLR_RT_DataTypeLookup* dtl )
{
    TINYCLR_HEADER();

    OpcodeSlot* osPtr = &m_opcodeSlots[ pos ];
    CLR_INT32   op    = Arm_GetOpcodeOperandOffset( osPtr, 0, 0 );

    if(dtl->m_flags & CLR_RT_DataTypeLookup::c_Numeric)
    {
        if(dtl->m_sizeInBytes <= 4)
        {
            TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_I4, 0, 1 )                                                                                    ));
            TINYCLR_CHECK_HRESULT(Arm_LDR       ( ArmProcessor::c_register_r1, ArmProcessor::c_register_r3, 0 , dtl->m_sizeInBytes, ((dtl->m_flags & CLR_RT_DataTypeLookup::c_Signed) != 0)                    ));
            TINYCLR_CHECK_HRESULT(Arm_STMIA     (                              ArmProcessor::c_register_r6, op, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1, ArmProcessor::c_register_r3 ));
        }
        else
        {
            TINYCLR_CHECK_HRESULT(Arm_LongMovIMM( ArmProcessor::c_register_r0, CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_I8, 0, 1 )                                                                                          ));
            TINYCLR_CHECK_HRESULT(Arm_LDMIA     ( ArmProcessor::c_register_r3, 0 ,                                   ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
            TINYCLR_CHECK_HRESULT(Arm_STMIA     ( ArmProcessor::c_register_r6, op, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
        }
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_LDMIA( ArmProcessor::c_register_r3, 0 , ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r6, op, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::Arm_StoreElement( size_t pos, const CLR_RT_DataTypeLookup* dtl )
{
    TINYCLR_HEADER();

    OpcodeSlot* osPtr = &m_opcodeSlots[ pos ];
    CLR_INT32   op    = Arm_GetOpcodeOperandOffset( osPtr, 2, 0 );

    if((dtl->m_flags & CLR_RT_DataTypeLookup::c_SemanticMask) == CLR_RT_DataTypeLookup::c_ValueType)
    {
        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r1, ArmProcessor::c_register_r6, op ));
        TINYCLR_CHECK_HRESULT(Arm_ADD_IMM( ArmProcessor::c_register_r0, ArmProcessor::c_register_r3, 0  ));

        TINYCLR_CHECK_HRESULT(Arm_B( ArmProcessor::c_Link, Arm_AbsoluteOffset( m_Arm_Thunks->m_address__MethodCompilerHelpers__CopyValueType ) ));
    }
    else if(dtl->m_flags & CLR_RT_DataTypeLookup::c_Numeric)
    {
        if(dtl->m_sizeInBytes <= 4)
        {
            TINYCLR_CHECK_HRESULT(Arm_LDR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r6, op + offsetof(CLR_RT_HeapBlock,m_data.numeric)                                 ));
            TINYCLR_CHECK_HRESULT(Arm_STR( ArmProcessor::c_register_r0, ArmProcessor::c_register_r3, 0, dtl->m_sizeInBytes, ((dtl->m_flags & CLR_RT_DataTypeLookup::c_Signed) != 0) ));
        }
        else
        {
            TINYCLR_CHECK_HRESULT(Arm_LDMIB( ArmProcessor::c_register_r6, op, ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
            TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r3, 0 , ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
        }
    }
    else
    {
        TINYCLR_CHECK_HRESULT(Arm_LDMIA( ArmProcessor::c_register_r6, op, ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r12 ));
        TINYCLR_CHECK_HRESULT(Arm_STMIA( ArmProcessor::c_register_r3, 0 , ArmProcessor::c_register_lst_r0 | ArmProcessor::c_register_lst_r1 | ArmProcessor::c_register_lst_r2, ArmProcessor::c_register_r3  ));
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif
