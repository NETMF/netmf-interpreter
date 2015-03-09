////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MethodCompiler::TypeDescriptor::Parse( CLR_RT_SignatureParser& parser )
{
    TINYCLR_HEADER();

    CLR_RT_SignatureParser::Element el;

    if(parser.Available() <= 0) TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_SIGNATURE);

    TINYCLR_CHECK_HRESULT(parser.Advance( el ));

    m_flags  = el.m_fByRef ? c_ByRef : 0;
    m_levels = el.m_levels;
    m_cls    = el.m_cls   ;

    TINYCLR_NOCLEANUP();
}

void MethodCompiler::TypeDescriptor::InitializeFromIndex( CLR_RT_TypeDef_Index& idx )
{
    m_cls    = idx;
    m_levels = 0;
    m_flags  = 0;
}

void MethodCompiler::TypeDescriptor::InitializeFromDataType( CLR_DataType dt )
{
    InitializeFromIndex( *(c_CLR_RT_DataTypeLookup[ dt ].m_cls) );
}

bool MethodCompiler::TypeDescriptor::ConvertFromTypeDescriptor( const CLR_RT_TypeDescriptor& td )
{
    if(td.m_reflex.m_kind != REFLECTION_TYPE) return false;

    m_cls    = td.m_reflex.m_data.m_type;
    m_levels = td.m_reflex.m_levels;
    m_flags  = 0;

    return true;
}

bool MethodCompiler::TypeDescriptor::ConvertToTypeDescriptor( CLR_RT_TypeDescriptor& td )
{
    CLR_RT_ReflectionDef_Index reflex;

    reflex.m_kind        = REFLECTION_TYPE;
    reflex.m_levels      = m_levels;
    reflex.m_data.m_type = m_cls;

    return SUCCEEDED(td.InitializeFromReflection( reflex ));
}

bool MethodCompiler::TypeDescriptor::NeedsCloning()
{
    if(m_levels == 0 && (m_flags & (c_Boxed | c_ByRef | c_ByRefArray)) == 0)
    {
        CLR_RT_TypeDef_Instance inst; inst.InitializeFromIndex( m_cls );

        if(inst.m_target->dataType == DATATYPE_VALUETYPE)
        {
            return true;
        }
    }

    return false;
}

bool MethodCompiler::TypeDescriptor::NeedsPromotion( Promotion& pr, bool fStore )
{
    if(m_levels == 0 && (m_flags & (c_Boxed | c_ByRef | c_ByRefArray)) == 0)
    {
        CLR_RT_TypeDef_Instance      inst; inst.InitializeFromIndex( m_cls );
        CLR_DataType                 dt  = (CLR_DataType)inst.m_target->dataType;
        const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ dt ];

        switch(dt)
        {
        case DATATYPE_I4:
        case DATATYPE_I8:
        case DATATYPE_R4:
        case DATATYPE_R8:
            if(fStore)
            {
                pr.m_dt      = dt;
                pr.m_size    = dtl.m_sizeInBytes;
                pr.m_fSigned = ((dtl.m_flags & CLR_RT_DataTypeLookup::c_Signed) != 0);

                return true;
            }
            break;

        default:
            if(dtl.m_flags & CLR_RT_DataTypeLookup::c_Numeric)
            {
                pr.m_dt      = dtl.m_sizeInBytes == 8 ? DATATYPE_I8 : DATATYPE_I4;
                pr.m_size    = dtl.m_sizeInBytes;
                pr.m_fSigned = ((dtl.m_flags & CLR_RT_DataTypeLookup::c_Signed) != 0);

                return true;
            }
            break;
        }
    }

    return false;
}

//--//

bool MethodCompiler::TypeDescriptor::IsInstanceOf( TypeDescriptor* target )
{
    CLR_RT_TypeDescriptor descThis  ; this  ->ConvertToTypeDescriptor( descThis   );
    CLR_RT_TypeDescriptor descTarget; target->ConvertToTypeDescriptor( descTarget );

    return g_CLR_RT_ExecutionEngine.IsInstanceOf( descThis, descTarget );
}

bool MethodCompiler::TypeDescriptor::IsCompatible( TypeDescriptor* left, TypeDescriptor* right )
{
    //
    // The result of CEE_LDNULL can always be converted to some other type.
    //
    if(left->m_flags & c_Null)
    {
        *left = *right;
        return true;
    }

    if(right->m_flags & c_Null)
    {
        *right = *left;
        return true;
    }

    //
    // For now, byref have to match.
    //
    if((left->m_flags & c_ByRef     ) != (right->m_flags & c_ByRef     )) return false;
    if((left->m_flags & c_ByRefArray) != (right->m_flags & c_ByRefArray)) return false;

    if(left->IsInstanceOf( right ))
    {
        *left = *right;
        return true;
    }

    if(right->IsInstanceOf( left ))
    {
        *right = *left;
        return true;
    }

    return false;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MethodCompiler::ParseEvalStack()
{
    TINYCLR_HEADER();

    size_t i;

    //
    // Start from the first instruction of the method and all the exception handlers.
    //
    m_stackStatus.Clear();
    TINYCLR_CHECK_HRESULT(ProcessEvaluationStack( 0, OpcodeSlot::c_EntryPoint ));

    for(i=0; i<m_EHs.Length(); i++)
    {
        CLR_RECORD_EH& ptr = m_EHs[ i ];

        m_stackStatus.Clear();

        if(ptr.mode & CLR_RECORD_EH::EH_Finally)
        {
        }
        else
        {
            CLR_RT_TypeDef_Instance cls;
            TypeDescriptor*         td = m_stackStatus.Push(); CHECK_ALLOCATION(td);

            if(cls.ResolveToken( ptr.GetToken(), m_clsInst.m_assm ) == false)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_TOKEN__TYPE);
            }

            td->m_cls    = cls;
            td->m_flags  = 0;
            td->m_levels = 0;
        }

        TINYCLR_CHECK_HRESULT(ProcessEvaluationStack( ptr.handlerStart, OpcodeSlot::c_ExceptionHandler ));
    }

    //
    // Find dead code.
    //
    {
        OpcodeSlot* osPtr = m_opcodeSlots;
        size_t      opNum = m_numOpcodes;

        while(opNum--)
        {
            if(osPtr->m_stackDepth < 0)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
            }

            osPtr++;
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::ProcessEvaluationStack( CLR_OFFSET pos, CLR_UINT32 flags )
{
    TINYCLR_HEADER();

    TypedQueue<CLR_OFFSET> branchesQueue; branchesQueue.Initialize();
    OpcodeSlot*            osPtr = &m_opcodeSlots[ pos ];
    bool                   fSame;

    if(osPtr->m_stackDepth >= 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
    }

    osPtr->m_flags |= flags;

    TINYCLR_CHECK_HRESULT(SaveStackStatus( *osPtr, fSame ));

    if(fSame)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    while(true)
    {
        Opcode op;

        TINYCLR_CHECK_HRESULT(op.Initialize( this, osPtr->m_ipOffset, NULL ));

        //
        // Update Eval Stack
        //
        {
            CLR_INT32 posTop = osPtr->m_stackDepth - op.m_stackPop;

            if(posTop < 0)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
            }

            TypeDescriptor* tdTop = &m_stackStatus[ posTop ];
            TypeDescriptor  td; td.m_cls.Clear();

            switch(op.m_ol->m_logicalOpcode)
            {
                //
                // Unary Operators.
                //
            case LO_Not:
            ////////////
            case LO_Neg:
                {
                    td = tdTop[ 0 ];
                }
                break;

                //
                // Binary Operators.
                //
            case LO_And:
            case LO_Or :
            case LO_Xor:
            ////////////
            case LO_Add:
            case LO_Sub:
            case LO_Mul:
            case LO_Div:
            case LO_Rem:
                {
                    td = tdTop[ 1 ];
                }
                break;

            case LO_Shl:
            case LO_Shr:
                {
                    td = tdTop[ 0 ];
                }
                break;

            case LO_Box:
                {
                    td.InitializeFromIndex( op.m_tdInst );

                    td.m_flags |= TypeDescriptor::c_Boxed;
                }
                break;

            case LO_Unbox:
                {
                    td.InitializeFromIndex( op.m_tdInst );

                    td.m_flags &= ~TypeDescriptor::c_Boxed;
                }
                break;

            case LO_Branch:
                {
                    // Nothing to do.
                }
                break;

            case LO_Set:
                {
                    td.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_Int32 );
                }
                break;

            case LO_Switch:
                {
                    // Nothing to do.
                }
                break;


            case LO_LoadFunction    :
            case LO_LoadVirtFunction:
                {
                    td.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_MethodInfo );
                }
                break;


            case LO_Call    :
            case LO_CallVirt:
                {
                    if(op.m_mdInst.m_target->retVal != DATATYPE_VOID)
                    {
                        CLR_RT_SignatureParser parser; parser.Initialize_MethodSignature( op.m_mdInst.m_assm, op.m_mdInst.m_target );

                        //
                        // Return value.
                        //
                        TINYCLR_CHECK_HRESULT(td.Parse( parser ));
                    }
                }
                break;

            case LO_Ret:
                {
                    // Nothing to do.
                }
                break;

            case LO_NewObject:
                {
                    CLR_RT_TypeDef_Instance inst; inst.InitializeFromMethod( op.m_mdInst );

                    td.InitializeFromIndex( inst );
                }
                break;

            case LO_CastClass:
            case LO_IsInst   :
                {
                    td.InitializeFromIndex( op.m_tdInst );

                    td.m_levels += op.m_tdInstLevels;

                    if(td.m_levels == 0 && (op.m_tdInst.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_ValueType)
                    {
                        td.m_flags |= TypeDescriptor::c_Boxed;
                    }
                }
                break;

            case LO_Dup:
                {
                    td = tdTop[ 0 ];
                }
                break;

            case LO_Pop:
                {
                    // Nothing to do.
                }
                break;


            case LO_Throw:
            case LO_Rethrow:
            case LO_Leave:
            case LO_EndFinally:
                {
                    // Nothing to do.
                }
                break;

            case LO_Convert:
                {
                    td.InitializeFromDataType( op.m_ol->m_dt );
                }
                break;

                //--//

            case LO_StoreArgument:
                {
                    // Nothing to do.
                }
                break;

            case LO_LoadArgument       :
            case LO_LoadArgumentAddress:
                {
                    td = m_arguments[ op.Index() ];

                    if(op.m_ol->m_logicalOpcode == LO_LoadArgumentAddress)
                    {
                        td.m_flags |= TypeDescriptor::c_ByRef;
                    }
                }
                break;

                //--//

            case LO_StoreLocal:
                {
                    // Nothing to do.
                }
                break;

            case LO_LoadLocal       :
            case LO_LoadLocalAddress:
                {
                    td = m_locals[ op.Index() ];

                    if(op.m_ol->m_logicalOpcode == LO_LoadLocalAddress)
                    {
                        td.m_flags |= TypeDescriptor::c_ByRef;
                    }
                }
                break;

                //--//

            case LO_LoadConstant_I4:
            case LO_LoadConstant_I8:
            case LO_LoadConstant_R4:
            case LO_LoadConstant_R8:
                {
                    td.InitializeFromDataType( op.m_value.DataType() );
                }
                break;

            case LO_LoadNull:
                {
                    td.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_Object );

                    td.m_flags |= TypeDescriptor::c_Null;
                }
                break;

            case LO_LoadString:
                {
                    td.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_String );
                }
                break;

            case LO_LoadToken:
                {
                    switch(CLR_TypeFromTk( op.m_token ))
                    {
                    case TBL_TypeSpec:
                    case TBL_TypeRef:
                    case TBL_TypeDef:
                        td.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_Type );
                        break;

                    case TBL_FieldRef:
                    case TBL_FieldDef:
                        td.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_FieldInfo );
                        break;

                    case TBL_MethodRef:
                    case TBL_MethodDef:
                        td.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_MethodInfo );
                        break;

                    default:
                        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
                        break;
                    }
                }
                break;

                //--//

            case LO_NewArray:
                {
                    td.InitializeFromIndex( op.m_tdInst );

                    td.m_levels = op.m_tdInstLevels+1;
                }
                break;

            case LO_LoadLength:
                {
                    td.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_Int32 );
                }
                break;

            case LO_StoreElement:
                {
                    // Nothing to do.
                }
                break;

            case LO_LoadElement:
                {
                    if(op.m_ol->m_dt == DATATYPE_OBJECT)
                    {
                        td = tdTop[ 0 ];

                        td.m_levels--;
                    }
                    else
                    {
                        td.InitializeFromDataType( op.m_ol->m_dt );
                    }
                }
                break;

            case LO_LoadElementAddress:
                {
                    td.InitializeFromIndex( op.m_tdInst );

                    td.m_levels += op.m_tdInstLevels;

                    if(op.m_tdInstLevels == 0 && (op.m_tdInst.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_ValueType)
                    {
                        td.m_flags |= TypeDescriptor::c_ByRef;
                    }
                    else
                    {
                        td.m_flags |= TypeDescriptor::c_ByRefArray;
                    }
                }
                break;

                //--//

            case LO_StoreField:
                {
                    // Nothing to do.
                }
                break;

            case LO_LoadField       :
            case LO_LoadFieldAddress:
                {
                    CLR_RT_SignatureParser parser; parser.Initialize_FieldDef( op.m_fdInst.m_assm, op.m_fdInst.m_target );

                    TINYCLR_CHECK_HRESULT(td.Parse( parser ));

                    if(op.m_ol->m_logicalOpcode == LO_LoadFieldAddress)
                    {
                        td.m_flags |= TypeDescriptor::c_ByRef;
                    }
                }
                break;

            case LO_StoreStaticField:
                {
                    // Nothing to do.
                }
                break;

            case LO_LoadStaticField       :
            case LO_LoadStaticFieldAddress:
                {
                    CLR_RT_SignatureParser parser; parser.Initialize_FieldDef( op.m_fdInst.m_assm, op.m_fdInst.m_target );

                    TINYCLR_CHECK_HRESULT(td.Parse( parser ));

                    if(op.m_ol->m_logicalOpcode == LO_LoadStaticFieldAddress)
                    {
                        td.m_flags |= TypeDescriptor::c_ByRef;
                    }
                }
                break;

            case LO_StoreIndirect:
                {
                    // Nothing to do.
                }
                break;

            case LO_LoadIndirect:
                {
                    if(op.m_ol->m_dt == DATATYPE_OBJECT)
                    {
                        td = tdTop[ 0 ];

                        td.m_flags &= ~(TypeDescriptor::c_ByRef | TypeDescriptor::c_ByRefArray);
                    }
                    else
                    {
                        td.InitializeFromDataType( op.m_ol->m_dt );
                    }
                }
                break;

            case LO_InitObject:
                {
                    // Nothing to do.
                }
                break;

            case LO_LoadObject:
                {
                    td.InitializeFromIndex( op.m_tdInst );
                }
                break;

            case LO_CopyObject:
                {
                    // Nothing to do.
                }
                break;

            case LO_StoreObject:
                {
                    // Nothing to do.
                }
                break;

            case LO_Nop:
                {
                    // Nothing to do.
                }
                break;

            case LO_Unsupported:
                {
                    // Nothing to do.
                }
                break;
            }

            //--//

            {
                CLR_INT32 diff;

                diff = op.m_stackPop;
                while(diff--)
                {
                    m_stackStatus.Pop( NULL );
                }

                //--//

                diff = op.m_stackPush;
                if(TINYCLR_INDEX_IS_INVALID(td.m_cls))
                {
                    if(diff)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
                    }
                }
                else
                {
                    if(diff == 0)
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
                    }

                    //
                    // Promote things to INT32/UINT32
                    //
                    if(td.m_levels == 0 && td.m_flags == 0)
                    {
                        CLR_RT_TypeDef_Instance cls; cls.InitializeFromIndex( td.m_cls );

                        switch(cls.m_target->dataType)
                        {
                            case DATATYPE_BOOLEAN :
                            case DATATYPE_I1      :
                            case DATATYPE_U1      :
                            case DATATYPE_CHAR    :
                            case DATATYPE_I2      :
                            case DATATYPE_U2      :
                            case DATATYPE_U4      :
                                td.m_cls = g_CLR_RT_WellKnownTypes.m_Int32;
                                break;

                            case DATATYPE_U8      :
                                td.m_cls = g_CLR_RT_WellKnownTypes.m_Int64;
                                break;
                        }
                    }

                    while(diff--)
                    {
                        tdTop = m_stackStatus.Push(); CHECK_ALLOCATION(tdTop);

                        *tdTop = td;
                    }
                }
            }
        }

        //--//

        for(CLR_UINT32 j=0; j<osPtr->m_branchesOut; j++)
        {
            CLR_OFFSET  target = m_opcodeBranches[ osPtr->m_branchesOutIdx + j ];
            OpcodeSlot& es     = m_opcodeSlots   [ target                      ];

            TINYCLR_CHECK_HRESULT(SaveStackStatus( es, fSame ));

            if(fSame == false)
            {
                CLR_OFFSET* branch = branchesQueue.Push(); CHECK_ALLOCATION(branch);

                *branch = target;
            }
        }

        //--//

        if((op.m_ol->m_flags & CLR_RT_OpcodeLookup::COND_BRANCH_MASK) != CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS)
        {
            pos++;
            osPtr = &m_opcodeSlots[pos];

            TINYCLR_CHECK_HRESULT(SaveStackStatus( *osPtr, fSame ));
        }
        else
        {
            fSame = true;
        }

        if(fSame)
        {
            CLR_OFFSET branch;

            if(branchesQueue.Pop( &branch ) == false) break;

            pos   = branch;
            osPtr = &m_opcodeSlots[ pos ];

            TINYCLR_CHECK_HRESULT(RestoreStackStatus( *osPtr ));
        }
    }

    TINYCLR_CLEANUP();

    branchesQueue.Release();

    TINYCLR_CLEANUP_END();
}

HRESULT MethodCompiler::SaveStackStatus( OpcodeSlot& os, bool& fSame )
{
    TINYCLR_HEADER();

    size_t lenSrc = m_stackStatus.Size();

    if(lenSrc > OpcodeSlot::c_MaxDepth)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
    }

    if(os.m_stackDepth < 0)
    {
        os.m_stackDepth = (CLR_INT8)lenSrc;
        fSame           = false;

        //--//

        if(lenSrc == 0)
        {
            os.m_stackIdx = CLR_EmptyIndex;
        }
        else
        {
            TypeDescriptor* ptrSrc = &m_stackStatus[ 0 ];
            TypeDescriptor* ptrDst = &m_stackTypes [ 0 ];
            size_t          lenDst =  m_stackTypes .Size();
            size_t          pos;

            if(lenDst > lenSrc)
            {
                size_t size = sizeof(*ptrSrc) * lenSrc;

                for(pos=0; pos < lenDst - lenSrc; pos++)
                {
                    if(ptrSrc->m_cls.m_data == ptrDst->m_cls.m_data)
                    {
                        if(memcmp( ptrSrc, ptrDst, size ) == 0)
                        {
                            os.m_stackIdx = (CLR_OFFSET)pos;

                            TINYCLR_SET_AND_LEAVE(S_OK);
                        }
                    }

                    ptrDst++;
                }
            }

            CHECK_ALLOCATION(m_stackTypes.Reserve( lenDst + lenSrc ));

            os.m_stackIdx = (CLR_OFFSET)lenDst;

            for(pos=0; pos < lenSrc; pos++)
            {
                ptrDst = m_stackTypes.Push(); CHECK_ALLOCATION(ptrDst);

                *ptrDst = *ptrSrc++;
            }
        }
    }
    else if(os.m_stackDepth != lenSrc)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
    }
    else
    {
        TypeDescriptor* ptrSrc = &m_stackStatus[ 0             ];
        TypeDescriptor* ptrDst = &m_stackTypes [ os.m_stackIdx ];

        fSame = memcmp( ptrSrc, ptrDst, lenSrc * sizeof(*ptrSrc) ) == 0;

        if(fSame == false)
        {
            for(size_t i=0; i<lenSrc; i++)
            {
                if(memcmp( ptrSrc, ptrDst, sizeof(*ptrSrc) ))
                {
                    if(TypeDescriptor::IsCompatible( ptrSrc, ptrDst ) == false)
                    {
                        CLR_INT32 levels;

                        CLR_Debug::Printf( "  Eval Stack mismatch at %d: ", i );

                        CLR_RT_DUMP::TYPE( ptrSrc->m_cls );
                        levels = ptrSrc->m_levels;
                        while(levels-- > 0)
                        {
                            CLR_Debug::Printf( "[]" );
                        }
                        CLR_Debug::Printf( " " );

                        CLR_RT_DUMP::TYPE( ptrDst->m_cls );
                        levels = ptrDst->m_levels;
                        while(levels-- > 0)
                        {
                            CLR_Debug::Printf( "[]" );
                        }
                        CLR_Debug::Printf( "\n" );

                        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
                    }
                }

                ptrSrc++; ptrDst++;
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MethodCompiler::RestoreStackStatus( OpcodeSlot& os )
{
    TINYCLR_HEADER();

    m_stackStatus.Clear();

    if(os.m_stackIdx == CLR_EmptyIndex)
    {
        if(os.m_stackDepth != 0)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_EVALUATION_STACK);
        }
    }
    else
    {
        TypeDescriptor* ptrSrc = &m_stackTypes[ os.m_stackIdx  ];
        size_t          len    =                os.m_stackDepth;

        CHECK_ALLOCATION(m_stackStatus.Reserve( len ));

        while(len--)
        {
            TypeDescriptor* ptrDst = m_stackStatus.Push(); CHECK_ALLOCATION(ptrDst);

            *ptrDst = *ptrSrc++;
        }
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif
