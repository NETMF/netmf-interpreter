////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::ByteCode::LogicalOpcodeDesc::LogicalOpcodeDesc( const CLR_RT_OpcodeLookup& ol, CLR_OPCODE op, const UINT8* ip, const UINT8* ipEnd )
{
    m_ol           = &ol;                      // const CLR_RT_OpcodeLookup* m_ol;
    m_op           = op;                       // CLR_OPCODE                 m_op;
                                               //
    m_ipOffset     = 0;                        // CLR_UINT32                 m_ipOffset;
    m_ipLength     = (CLR_UINT32)(ipEnd - ip); // CLR_UINT32                 m_ipLength;
                                               //
    m_stackDepth   = 0x80000000;               // CLR_UINT32                 m_stackDepth;
    m_stackDiff    = ol.StackChanges();        // CLR_INT32                  m_stackDiff;
                                               //
    m_references   = 0;                        // CLR_UINT32                 m_references;
                                               //
    m_index        = 0;                        // CLR_UINT32                 m_index;
    m_token        = mdTokenNil;               // mdToken                    m_token;
    m_arg_I4       = 0;                        // CLR_INT32                  m_arg_I4;
    m_arg_R4       = 0;                        // CLR_INT32                  m_arg_R4;
    m_arg_I8       = 0;                        // CLR_INT64                  m_arg_I8;
    m_arg_R8       = 0;                        // CLR_INT64                  m_arg_R8;
                                               // std::vector<CLR_INT32>     m_targets;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MetaData::ByteCode::Parse_ByteCode( const MethodDef& md, COR_ILMETHOD_DECODER& il )
{
    TINYCLR_HEADER();

    const CLR_UINT8* ip      = il.Code;
    const CLR_UINT8* ipStart = ip;
    const CLR_UINT8* ipEnd   = ip + il.GetCodeSize();

    //wprintf( L"Method: %s : %d\n", m_name.c_str(), il.GetMaxStack() );

    while(ip < ipEnd)
    {
        const CLR_UINT8*           ipPre = ip;
        CLR_OPCODE                 op    = CLR_ReadNextOpcode( ip );
        const CLR_RT_OpcodeLookup& ol    = c_CLR_RT_OpcodeLookup[op];

        //printf( "  %08X %s\n", ip - ipStart, opcodeName[op] );

        if(ol.m_flags & CLR_RT_OpcodeLookup::COND_OVERFLOW)
        {
            std::wstring str;

            if(SUCCEEDED(ErrorReporting::ConstructErrorOrigin( str, md.m_method.m_holder->m_pSymReader, md.m_md, (ULONG32)(ipPre - ipStart) )))
            {
                ErrorReporting::Print( str.c_str(), NULL, FALSE, 0, L"opcode '%S' -- overflow will not throw exception", ol.m_name );
            }
        }

        if(op < CEE_COUNT && ol.m_logicalOpcode != LO_Unsupported)
        {
            const CLR_UINT8* ipEnd = CLR_SkipBodyOfOpcode( ip, op );

            LogicalOpcodeDesc& ref = *(m_opcodes.insert( m_opcodes.end(), LogicalOpcodeDesc( ol, op, ipPre, ipEnd ) ));

            switch(ol.m_opParam)
            {
            case CLR_OpcodeParam_Field:
                {
                    FETCH_ARG_UINT32(arg,ip);

                    ref.m_token = (mdToken)arg;
                }
                break;

            case CLR_OpcodeParam_Method:
                {
                    FETCH_ARG_UINT32(arg,ip);

                    ref.m_token = (mdToken)arg;
                }
                break;

            case CLR_OpcodeParam_Type:
                {
                    FETCH_ARG_UINT32(arg,ip);

                    ref.m_token = (mdToken)arg;
                }
                break;

            case CLR_OpcodeParam_String:
                {
                    FETCH_ARG_UINT32(arg,ip);

                    ref.m_token = (mdToken)arg;
                }
                break;

            case CLR_OpcodeParam_Tok:
                {
                    FETCH_ARG_UINT32(arg,ip);

                    ref.m_token = (mdToken)arg;
                }
                break;

            case CLR_OpcodeParam_Sig:
                TINYCLR_SET_AND_LEAVE(CLR_E_UNSUPPORTED_INSTRUCTION);

            case CLR_OpcodeParam_BrTarget:
                {
                    FETCH_ARG_INT32(arg,ip);

                    ref.m_targets.resize( 1 );

                    ref.m_targets[0] = ref.m_ipLength + arg;
                }
                break;

            case CLR_OpcodeParam_ShortBrTarget:
                {
                    FETCH_ARG_INT8(arg,ip);

                    ref.m_targets.resize( 1 );

                    ref.m_targets[0] = ref.m_ipLength + arg;
                }
                break;

            case CLR_OpcodeParam_I:
                {
                    FETCH_ARG_INT32(arg,ip);

                    ref.m_arg_I4 = arg;
                }
                break;

            case CLR_OpcodeParam_I8:
                {
                    FETCH_ARG_INT64(arg,ip);

                    ref.m_arg_I8 = arg;
                }
                break;

            case CLR_OpcodeParam_None:
                if(ol.m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX)
                {
                    ref.m_index = ol.m_index;
                }

                if(ol.m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_I4)
                {
                    ref.m_arg_I4 = ol.m_index;
                }
                break;

            case CLR_OpcodeParam_R:
                {
                    FETCH_ARG_UINT64(arg,ip);

                    ref.m_arg_R8 = arg;
                }
                break;

            case CLR_OpcodeParam_Switch:
                {
                    FETCH_ARG_UINT32(arg,ip);

                    ref.m_targets.resize( arg );

                    for(CLR_UINT32 i=0; i<arg; i++)
                    {
                        CLR_INT32& val = ref.m_targets[i];

                        memcpy( &val, ip, sizeof(CLR_INT32) ); ip += sizeof(CLR_INT32);

                        val = (CLR_INT32)ref.m_ipLength + val;
                    }
                }
                break;

            case CLR_OpcodeParam_Var:
                {
                    FETCH_ARG_UINT16(arg,ip);

                    ref.m_index = arg;
                }
                break;

            case CLR_OpcodeParam_ShortI:
                {
                    FETCH_ARG_INT8(arg,ip);

                    ref.m_arg_I4 = arg;
                }
                break;

            case CLR_OpcodeParam_ShortR:
                {
                    FETCH_ARG_UINT32(arg,ip);

                    ref.m_arg_R4 = arg;
                }
                break;

            case CLR_OpcodeParam_ShortVar:
                {
                    FETCH_ARG_UINT8(arg,ip);

                    ref.m_index = arg;
                }
                break;
            }
        }
        else
        {
            wprintf( L"Method: %s\n", m_name.c_str() );
            wprintf( L"  %08X %S -- Not supported\n", ipPre - ipStart, op < CEE_COUNT ? c_CLR_RT_OpcodeLookup[op].m_name : "<invalid>" );
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }

    TINYCLR_NOCLEANUP();
}
