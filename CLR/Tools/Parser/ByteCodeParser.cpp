////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MetaData::ByteCode::Parse( const TypeDef& td, const MethodDef& md, COR_ILMETHOD_DECODER& il )
{
    TINYCLR_HEADER();

    if(il.Code && il.GetCodeSize() > 0)
    {
        m_name.append( td.m_name );
        m_name.append( L"::"     );
        m_name.append( md.m_name );

        TINYCLR_CHECK_HRESULT(Parse_ByteCode( md, il ));

        LogicalOpcodeDesc* dst;
        OffsetToIndex      mapOffsetToIndex_Start;
        OffsetToIndex      mapOffsetToIndex_End;
        size_t             len    = m_opcodes.size();
        CLR_INT32          offset = 0;

        for(size_t i=0; i<len; i++)
        {
            LogicalOpcodeDesc& ref = m_opcodes[i];

            mapOffsetToIndex_Start[offset] = (CLR_INT32)i;

            ref.m_ipOffset = offset; offset += ref.m_ipLength;

            mapOffsetToIndex_End[offset] = (CLR_INT32)i;
        }

        //
        // Mark the first opcode as a branch destination.
        //
        m_opcodes[0].m_references++;

        for(size_t i=0; i<len; i++)
        {
            LogicalOpcodeDesc& ref = m_opcodes[i];

            if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
            {
                for(CLR_UINT32 j=0; j<ref.m_targets.size(); j++)
                {
                    CLR_INT32& target = ref.m_targets[j];

                    dst = FindTarget( mapOffsetToIndex_Start, ref.m_ipOffset + target , target );
                    if(dst == NULL)
                    {
                        wprintf( L"Method: %s\n", m_name.c_str() );
                        wprintf( L"Cannot find target opcode: %d -> %d:%d\n", i, ref.m_ipOffset, j );
                        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
                    }
                }
            }
        }

        //--//

        if(il.EH)
        {
            int ehCount = il.EH->EHCount();
            if(ehCount)
            {
                IMAGE_COR_ILMETHOD_SECT_EH_CLAUSE_FAT ehBuff;

                for(int j = 0; j < ehCount; j++)
                {
                    const IMAGE_COR_ILMETHOD_SECT_EH_CLAUSE_FAT* ehInfo = il.EH->EHClause( j, &ehBuff );
                    if(ehInfo)
                    {
                        LogicalExceptionBlock leb;

                        leb.m_Flags         = ehInfo->Flags;
                        leb.m_TryOffset     = ehInfo->TryOffset;
                        leb.m_TryLength     = ehInfo->TryLength;
                        leb.m_HandlerOffset = ehInfo->HandlerOffset;
                        leb.m_HandlerLength = ehInfo->HandlerLength;

                        if(leb.m_Flags == COR_ILEXCEPTION_CLAUSE_FILTER)
                        {
                            leb.m_FilterOffset    = ehInfo->FilterOffset;
                            dst = FindTarget( mapOffsetToIndex_Start, leb.m_FilterOffset, leb.m_FilterIndex);
                            if(dst == NULL) { wprintf( L"Bad FilterOffset: %d %d\n", j, leb.m_FilterOffset); TINYCLR_SET_AND_LEAVE(CLR_E_FAIL); }
                        }
                        else
                        {
                            leb.m_ClassToken    = ehInfo->ClassToken;
                        }

                        dst = FindTarget( mapOffsetToIndex_Start, leb.m_TryOffset                          , leb.m_TryIndex        ); if(dst == NULL) { wprintf( L"Bad TryOffset: %d %d\n"    , j, leb.m_TryOffset     ); TINYCLR_SET_AND_LEAVE(CLR_E_FAIL); }
                        dst = FindTarget( mapOffsetToIndex_End  , leb.m_TryOffset     + leb.m_TryLength    , leb.m_TryIndexEnd     ); if(dst == NULL) { wprintf( L"Bad TryLength: %d %d\n"    , j, leb.m_TryLength     ); TINYCLR_SET_AND_LEAVE(CLR_E_FAIL); }
                        dst = FindTarget( mapOffsetToIndex_Start, leb.m_HandlerOffset                      , leb.m_HandlerIndex    ); if(dst == NULL) { wprintf( L"Bad HandlerOffset: %d %d\n", j, leb.m_HandlerOffset ); TINYCLR_SET_AND_LEAVE(CLR_E_FAIL); }
                        dst = FindTarget( mapOffsetToIndex_End  , leb.m_HandlerOffset + leb.m_HandlerLength, leb.m_HandlerIndexEnd ); if(dst == NULL) { wprintf( L"Bad HandlerLength: %d %d\n", j, leb.m_HandlerLength ); TINYCLR_SET_AND_LEAVE(CLR_E_FAIL); }

                        m_exceptions.push_back( leb );
                    }
                }
            }
        }

        //--//

//        offset = 0;
//
//        for(size_t i=0; i<len; i++)
//        {
//            LogicalOpcodeDesc& ref = m_opcodes[i];
//
//            if(ref.m_references) offset++;
//        }
//
//        wprintf( L"Method: %s  %d / %d\n", m_name.c_str(), offset, len );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::ByteCode::VerifyConsistency( const TypeDef& td, const MethodDef& md, Parser* pr )
{
    TINYCLR_HEADER();

    size_t len = m_opcodes.size();

    for(size_t i=0; i<len; i++)
    {
        LogicalOpcodeDesc& ref = m_opcodes[i];

        if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN)
        {
            if(pr->m_holder->IsMethodToken( pr, ref.m_token ))
            {
                Parser*    prDst;
                MethodDef* mdDst;

                TINYCLR_CHECK_HRESULT(pr->m_holder->ResolveMethodDef( pr, ref.m_token, prDst, mdDst ));

                switch(ref.m_ol->m_logicalOpcode)
                {
                case LO_Call    :
                case LO_CallVirt:
                    ref.m_stackDiff -= (CLR_INT32)mdDst->m_method.m_lstParams.size();

                    if((mdDst->m_flags & mdStatic) == 0)
                    {
                        ref.m_stackDiff -= 1;
                    }

                    if(mdDst->m_method.m_retValue.m_opt != ELEMENT_TYPE_VOID)
                    {
                        ref.m_stackDiff += 1;
                    }
                    break;

                case LO_NewObject:
                    ref.m_stackDiff -= (CLR_INT32)mdDst->m_method.m_lstParams.size();

                    break;
                }
            }
        }
    }

    TINYCLR_CHECK_HRESULT(UpdateStackDepth());

    TINYCLR_CLEANUP();

    //DumpStats();

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT MetaData::ByteCode::UpdateStackDepth()
{
    TINYCLR_HEADER();

    size_t i;

    TINYCLR_CHECK_HRESULT(ComputeStackDepth( 0, 0 ));

    for(i=0; i<m_exceptions.size(); i++)
    {
        LogicalExceptionBlock& leb = m_exceptions[i];

        TINYCLR_CHECK_HRESULT(ComputeStackDepth( leb.m_HandlerIndex, (leb.m_Flags & COR_ILEXCEPTION_CLAUSE_FINALLY) ? 0 : 1 ));
        if(leb.m_Flags & COR_ILEXCEPTION_CLAUSE_FILTER)
            TINYCLR_CHECK_HRESULT(ComputeStackDepth( leb.m_FilterIndex, 1 ));
    }

    //--//

    for(i=0; i<m_opcodes.size(); i++)
    {
        LogicalOpcodeDesc& ref = m_opcodes[i];

        if(ref.m_stackDepth == 0x80000000)
        {
            std::wstring str;

            wprintf( L"Method: %s\n", m_name.c_str() );
            wprintf( L"Unreachable opcode: %d:%04x\n", i, ref.m_ipOffset );

            ref.m_stackDepth = 0;

            //FIXME: How do I get MethodDef information so this warning actually gets printed?
            /*if(SUCCEEDED(ErrorReporting::ConstructErrorOrigin( str, m_pSymReader, md , ref.m_ipOffset )))
            {
                ErrorReporting::Print( str.c_str(), NULL, FALSE, 0, L"Unreachable opcode: %d:%04x", i, ref.m_ipOffset );
            }*/

            //DumpStats();
            //TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::ByteCode::ComputeStackDepth( size_t pos, CLR_UINT32 depth )
{
    TINYCLR_HEADER();

    while(true)
    {
        LogicalOpcodeDesc& ref = m_opcodes[pos];

        if(ref.m_stackDepth != 0x80000000)
        {
            if(ref.m_stackDepth != depth)
            {
                wprintf( L"%s:\n", m_name.c_str() );
                wprintf( L"Stack mismatch at %d: %d <> %d\n", pos, ref.m_stackDepth, depth );

                DumpStats();
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            break;
        }

        ref.m_stackDepth = depth; 
        
        if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::STACK_RESET)
        {
            depth = 0;
        }
        else
        {
            depth += ref.m_stackDiff;
        }
        
        if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
        {
            for(CLR_UINT32 j=0; j<ref.m_targets.size(); j++)
            {
                TINYCLR_CHECK_HRESULT(ComputeStackDepth( ref.m_targets[j], depth ));
            }
        }

        if((ref.m_ol->m_flags & CLR_RT_OpcodeLookup::COND_BRANCH_MASK) == CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS) break;

        pos++;
    }

    TINYCLR_NOCLEANUP();
}

CLR_UINT32 MetaData::ByteCode::MaxStackDepth()
{
    CLR_UINT32 maxStackDepth = 0;

    for(size_t i=0; i<m_opcodes.size(); i++)
    {
        LogicalOpcodeDesc& ref = m_opcodes[i];
        CLR_UINT32         stackDepth = ref.m_stackDepth;

        if(maxStackDepth < stackDepth) maxStackDepth = stackDepth;
    }

    return maxStackDepth;
}

//--//

MetaData::ByteCode::LogicalOpcodeDesc* MetaData::ByteCode::FindTarget( OffsetToIndex& map, CLR_INT32 offset, CLR_INT32& index )
{
    OffsetToIndexConstIter it = map.find( offset );

    if(it != map.end())
    {
        index = it->second;

        LogicalOpcodeDesc& ref = m_opcodes[ index ];

        ref.m_references++;

        return &ref;
    }

    return NULL;
}

void MetaData::ByteCode::DumpStats()
{
    std::map<CLR_OPCODE        ,int> countOpcode;
    std::map<CLR_LOGICAL_OPCODE,int> countLogicalOpcode;

    static std::map<CLR_OPCODE        ,int> g_countOpcode;
    static std::map<CLR_LOGICAL_OPCODE,int> g_countLogicalOpcode;

    wprintf( L"%s:\n", m_name.c_str() );

    for(size_t i=0; i<m_opcodes.size(); i++)
    {
        LogicalOpcodeDesc& ref = m_opcodes[i];

        countOpcode       [ref.m_op                ]++;
        countLogicalOpcode[ref.m_ol->m_logicalOpcode]++;

        g_countOpcode       [ref.m_op                ]++;
        g_countLogicalOpcode[ref.m_ol->m_logicalOpcode]++;

        DumpOpcode( i, ref );
    }

    wprintf( L"Opcodes: %d op, %d lo (%d op, %d lo)\n\n", countOpcode.size(), countLogicalOpcode.size(), g_countOpcode.size(), g_countLogicalOpcode.size() );
}

//--//

MetaData::ByteCode::Distribution MetaData::ByteCode::s_numOfOpcodes;
MetaData::ByteCode::Distribution MetaData::ByteCode::s_numOfEHs;
MetaData::ByteCode::Distribution MetaData::ByteCode::s_sizeOfMethod;

void MetaData::ByteCode::DumpDistributionStats()
{
    DistributionIter it;

    for(it=s_numOfOpcodes.begin(); it!=s_numOfOpcodes.end(); it++)
    {
        wprintf( L"Distribution: Opcode : %d %d\n", it->first, it->second );
    }

    for(it=s_numOfEHs.begin(); it!=s_numOfEHs.end(); it++)
    {
        wprintf( L"Distribution: EH : %d %d\n", it->first, it->second );
    }

    for(it=s_sizeOfMethod.begin(); it!=s_sizeOfMethod.end(); it++)
    {
        wprintf( L"Distribution: Size : %d %d\n", it->first, it->second );
    }
}

void MetaData::ByteCode::DumpOpcode( size_t index, LogicalOpcodeDesc& ref )
{
    wprintf( L"  %4d:%04x %c%3d %-20S", index, ref.m_ipOffset, ref.m_references ? '*' : ' ', ref.m_stackDepth, c_CLR_RT_LogicalOpcodeLookup[ref.m_ol->m_logicalOpcode].m_name );

    switch((ref.m_ol->m_flags & CLR_RT_OpcodeLookup::COND_BRANCH_MASK))
    {
    case CLR_RT_OpcodeLookup::COND_BRANCH_NEVER           : break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_ALWAYS          : break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_IFTRUE          : wprintf( L" IfTrue"           ); break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_IFFALSE         : wprintf( L" IfFalse"          ); break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_IFEQUAL         : wprintf( L" IfEqual"          ); break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_IFNOTEQUAL      : wprintf( L" IfNotEqual"       ); break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATER       : wprintf( L" IfGreater"        ); break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_IFGREATEROREQUAL: wprintf( L" IfGreaterOrEqual" ); break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_IFLESS          : wprintf( L" IfLess"           ); break;
    case CLR_RT_OpcodeLookup::COND_BRANCH_IFLESSOREQUAL   : wprintf( L" IfLessOrEqual"    ); break;
    }

    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::COND_UNSIGNED) wprintf( L" Unsigned" );
    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::COND_OVERFLOW) wprintf( L" Overflow" );

    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_DT   ) wprintf( L" DataType=%d", ref.m_ol->m_dt );
    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_INDEX) wprintf( L" %d"         , ref.m_index   );
    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_TOKEN) wprintf( L" %08x"       , ref.m_token   );
    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_I4   ) wprintf( L" %d"         , ref.m_arg_I4  );
    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_I8   ) wprintf( L" %I64d"      , ref.m_arg_I8  );
    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_R4   ) wprintf( L" %f"         , (float)ref.m_arg_R4  );
    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_R8   ) wprintf( L" %g"         , (double)ref.m_arg_R8  );

    if(ref.m_ol->m_flags & CLR_RT_OpcodeLookup::ATTRIB_HAS_TARGET)
    {
        for(CLR_UINT32 j=0; j<ref.m_targets.size(); j++)
        {
            CLR_UINT32 offset = ref.m_ipOffset + ref.m_ipLength + ref.m_targets[j];

            for(size_t k=0; k<m_opcodes.size(); k++)
            {
                LogicalOpcodeDesc& refTarget = m_opcodes[k];

                if(refTarget.m_ipOffset == offset)
                {
                    wprintf( L" Index_%d", k );
                    break;
                }
            }
        }
    }

    for(size_t i=0; i<m_exceptions.size(); i++)
    {
        LogicalExceptionBlock& leb = m_exceptions[i];

        if(index == leb.m_TryIndex        ) wprintf( L" TRY {"           );
        if(index == leb.m_TryIndexEnd     ) wprintf( L" }"               );
        if(index == leb.m_HandlerIndex    ) wprintf( L" CATCH/FINALLY {" );
        if(index == leb.m_HandlerIndexEnd ) wprintf( L" }"               );
    }

    wprintf( L"\n" );
}
