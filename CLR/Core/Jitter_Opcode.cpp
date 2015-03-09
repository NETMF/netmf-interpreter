////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MethodCompiler::Opcode::Initialize( MethodCompiler* mc, CLR_OFFSET offset, CLR_OFFSET* targets )
{
    TINYCLR_HEADER();

    CLR_PMETADATA ip = mc->m_ipStart + offset;

    //--//

    memset( this, 0, sizeof(*this) );

    m_op        = CLR_ReadNextOpcodeCompressed( ip );
    m_ol        = &c_CLR_RT_OpcodeLookup[ m_op ];

    m_ipOffset  = offset;

    m_stackPop  = m_ol->StackPop ();
    m_stackPush = m_ol->StackPush();

    m_token     = CLR_EmptyToken;
    m_value.SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_FIRST_INVALID,0,1) );

    //--//

    switch(m_ol->m_opParam)
    {
    case CLR_OpcodeParam_Field:
        {
            FETCH_ARG_COMPRESSED_FIELDTOKEN(arg,ip);

            m_token = arg;
        }
        break;

    case CLR_OpcodeParam_Method:
        {
            FETCH_ARG_COMPRESSED_METHODTOKEN(arg,ip);

            m_token = arg;
        }
        break;

    case CLR_OpcodeParam_Type:
        {
            FETCH_ARG_COMPRESSED_TYPETOKEN(arg,ip);

            m_token = arg;
        }
        break;

    case CLR_OpcodeParam_String:
        {
            FETCH_ARG_COMPRESSED_STRINGTOKEN(arg,ip);

            m_token = arg;
        }
        break;

    case CLR_OpcodeParam_Tok:
        {
            FETCH_ARG_TOKEN(arg,ip);

            m_token = arg;
        }
        break;

    case CLR_OpcodeParam_Sig:
        TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_UNSUPPORTED);

    case CLR_OpcodeParam_BrTarget:
        {
            FETCH_ARG_INT16(arg,ip);

            m_numTargets = 1;

            if(targets)
            {
                *targets++ = (CLR_OFFSET)(ip - mc->m_ipStart + arg);
            }
        }
        break;

    case CLR_OpcodeParam_ShortBrTarget:
        {
            FETCH_ARG_INT8(arg,ip);

            m_numTargets = 1;

            if(targets)
            {
                *targets++ = (CLR_OFFSET)(ip - mc->m_ipStart + arg);
            }
        }
        break;

    case CLR_OpcodeParam_I:
        {
            FETCH_ARG_INT32(arg,ip);

            m_value.SetInteger( arg );
        }
        break;

    case CLR_OpcodeParam_I8:
        {
            FETCH_ARG_INT64(arg,ip);

            m_value.SetInteger( arg );
        }
        break;

    case CLR_OpcodeParam_None:
        if(m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
        {
            m_value.SetInteger( (CLR_UINT32)m_ol->m_index );
        }

        if(m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
        {
            m_value.SetInteger( (CLR_INT32)m_ol->m_index );
        }
        break;

    case CLR_OpcodeParam_R:
        {
            FETCH_ARG_UINT64(arg,ip);

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
                m_value.SetDoubleFromBits( arg );
#else
                TINYCLR_CHECK_HRESULT(m_value.SetDoubleIEEE754( arg ));
#endif

            
        }
        break;

    case CLR_OpcodeParam_Switch:
        {
            FETCH_ARG_UINT8(arg,ip);

            m_numTargets = arg;

            CLR_UINT32 base = (CLR_UINT32)(ip - mc->m_ipStart + arg * sizeof(CLR_INT16));

            while(arg--)
            {
                CLR_INT32 offset; TINYCLR_READ_UNALIGNED_INT16(offset,ip);

                if(targets)
                {
                    *targets++ = (CLR_OFFSET)(base + offset);
                }
            }
        }
        break;

    case CLR_OpcodeParam_Var:
        {
            FETCH_ARG_UINT16(arg,ip);

            m_value.SetInteger( (CLR_UINT32)m_ol->m_index );
        }
        break;

    case CLR_OpcodeParam_ShortI:
        {
            FETCH_ARG_INT8(arg,ip);

            m_value.SetInteger( (CLR_INT32)arg );
        }
        break;

    case CLR_OpcodeParam_ShortR:
        {
            FETCH_ARG_UINT32(arg,ip);
           
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
                m_value.SetFloatFromBits( arg );
#else
                TINYCLR_CHECK_HRESULT(m_value.SetFloatIEEE754( arg ));
#endif


        }
        break;

    case CLR_OpcodeParam_ShortVar:
        {
            FETCH_ARG_UINT8(arg,ip);

            m_value.SetInteger( (CLR_UINT32)arg );
        }
        break;
    }

    if(m_token != CLR_EmptyToken)
    {
        CLR_RT_Assembly* assm = mc->m_mdInst.m_assm;

        switch(CLR_TypeFromTk( m_token ))
        {
        case TBL_TypeRef:
        case TBL_TypeDef:
            {
                if(m_tdInst.ResolveToken( m_token, assm ) == false)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_TOKEN__TYPE);
                }
            }
            break;

        case TBL_MethodRef:
        case TBL_MethodDef:
            {
                if(m_mdInst.ResolveToken( m_token, assm ) == false)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_TOKEN__METHOD);
                }

                if(m_ol->m_flowCtrl == CLR_FlowControl_CALL)
                {
                    m_stackPop += m_mdInst.m_target->numArgs;

                    if(m_ol->m_logicalOpcode == LO_NewObject)
                    {
                        m_stackPop -= 1;
                    }

                    if(m_mdInst.m_target->retVal != DATATYPE_VOID)
                    {
                        m_stackPush += 1;
                    }
                }
            }
            break;

        case TBL_FieldRef:
        case TBL_FieldDef:
            {
                if(m_fdInst.ResolveToken( m_token, assm ) == false)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_TOKEN__FIELD);
                }
            }
            break;

        case TBL_TypeSpec:
            {
                CLR_RT_TypeSpec_Instance sig;
                CLR_RT_TypeDescriptor    desc;

                if(sig.ResolveToken( m_token, assm ) == false)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_TOKEN__TYPE);
                }

                TINYCLR_CHECK_HRESULT(desc.InitializeFromTypeSpec( sig ));

                m_tdInst.InitializeFromIndex( desc.m_reflex.m_data.m_type );
                m_tdInstLevels =              desc.m_reflex.m_levels;
            }
            break;

        case TBL_Strings:
            break;

        default:
            TINYCLR_SET_AND_LEAVE(CLR_E_JITTER_OPCODE_INVALID_SIGNATURE);
        }
    }

    m_ipLength  = (CLR_OFFSET)(ip - mc->m_ipStart - m_ipOffset);

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif
