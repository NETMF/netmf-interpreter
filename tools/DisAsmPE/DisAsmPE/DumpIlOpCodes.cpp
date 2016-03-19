
#include "stdafx.h"
#include "MetadataTables.h"
#include "UnalignedAccess.h"
#include "CommonFormatting.h"

using namespace NETMF::Metadata;
using namespace Microsoft::Utilities;

extern void DumpSignature( std::ostream& strm, AssemblyHeader const& header, MetadataPtr& p, size_t len /*= 0*/ );

void DumpOpCodeParamInlineNone( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    // NOP
}

void DumpOpCodeParamShortInlineVar( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    strm << ' ' << ( int )*p++;
}

void DumpOpCodeParamInlineVar( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    strm << ' ' << ReadUnalignedInt16( p );
}

void DumpOpCodeParamShortInlineI( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    strm << ' ' << ( int )*p++;
}

void DumpOpCodeParamInlineI( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    strm << ' ' << ReadUnalignedInt32( p );
}

void DumpOpCodeParamInlineI8( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    strm << ' ' << ReadUnalignedInt64( p );
}

void DumpOpCodeParamShortInlineR( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    auto rawValue = ReadUnalignedUInt32( p );
    strm << ' ' << *reinterpret_cast< float const* >( &rawValue );
}

void DumpOpCodeParamInlineR( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    auto rawValue = ReadUnalignedUInt64( p );
    strm << ' ' << *reinterpret_cast< double const* >( &rawValue );
}

void DumpOpCodeParamInlineMethod( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    MethodRefOrMethodDef token;
    token.Value = ReadUnalignedUInt16( p );
    strm << ' ' << token;
}

void DumpOpCodeParamInlineSig( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    strm << ' ';
    DumpSignature( strm, header, p );
}

void DumpOpCodeParamShortInlineBrTarget( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    // +2 for the instruction size
    auto targetOffsetBase = ilOffset + 2;
    strm << " IL_" << std::setw( 4 ) << targetOffsetBase + ( int )*p++;
}

void DumpOpCodeParamInlineBrTarget( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    auto targetOffsetBase = ilOffset + 2;
    strm << " IL_" << std::setw( 4 ) << targetOffsetBase + ReadUnalignedInt16( p );
}

void DumpOpCodeParamInlineSwitch( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    // MSSPOT1 PE format only supports 8 bits for the switch table size
    int numEntries = *p++;
    // table for switch is a set of offsets from the end of the current instruction
    // +1 for the opcode itself, +1 for the table length, + size of table
    // MSSPOT1 PE format uses only 16 bit offsets
    int targetOffsetBase = ilOffset + 2 + numEntries * sizeof( uint16_t );
    strm << '[' << numEntries << "] (";
    for( int i = 0; i < numEntries; ++i )
    {
        if( i > 0 )
            strm << ", ";

        strm << "IL_" << targetOffsetBase + ReadUnalignedUInt16( p );
    }
    strm << ')';
}

void DumpOpCodeParamInlineType( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    strm << " 0x" << ReadUnalignedInt16( p );
}

void DumpOpCodeParamInlineField( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    FieldRefOrFieldDef token;
    token.Value = ReadUnalignedInt16( p );
    strm << ' ' << token;
}

void DumpOpCodeParamInlineTok( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    auto tk = TokenFromStream( p );
    strm << " [ " << TypeFromToken( tk ) << " : " << IndexFromToken( tk ) << " ]";
}

void DumpOpCodeParamInlineString( std::ostream& strm, AssemblyHeader const& header, int ilOffset, MetadataPtr& p )
{
    strm << " \"" << FormatString( header.LookupString( ReadUnalignedInt16( p ) ) ) << '\"';
}

void DecodeIlOpcodes( std::ostream& strm, AssemblyTableRef<MethodDefTableEntry> ref  )
{
    std::string indent;
    IterableByteCodeStream it( ref.Header, ref.Index );
    for( auto p = it.begin(); p < it.end(); )
    {
        auto opcode = OpcodeFromStream( p );
        MetadataOffset ilOffset = p - it.begin();
        ExceptionHandlerTableEntry const* pHandler = it.GetHandlerFor( ilOffset );
        if( pHandler != nullptr )
        {
            // If ilOffset starts a try block
            if( pHandler->IsTryStart( ilOffset ) )
            {
                strm << indent << ".try { " << std::endl;
                indent += std::string( 4, ' ' );
            }
            if( pHandler->IsHandlerStart( ilOffset ) )
            {
                switch( pHandler->Mode )
                {
                case ExceptionHandlerMode::Catch:
                    strm << indent << "catch " << pHandler->ClassToken;
                    break;
                case ExceptionHandlerMode::CatchAll:
                    strm << indent << "catch";
                    break;
                case ExceptionHandlerMode::Filter:
                    strm << indent << "filter";
                    break;
                case ExceptionHandlerMode::Finally:
                    strm << indent << "finally";
                    break;
                }
                strm << std::endl;
                strm << indent << "{" << std::endl;
                indent += std::string( 4, ' ' );
            }
        }
        strm << indent << "IL_" << std::setw( 4 ) << ilOffset << ": (0x" << std::setw( 4 ) << static_cast< uint16_t >( opcode ) << ')';
        switch( opcode )
        {
#define OPDEF( id, name, pop, push, params, kind, len, b1, b2, flow )\
        case Opcode::##id: \
            strm << ' ' << name; \
            DumpOpCodeParam##params( strm, ref.Header, ilOffset, p ); \
            strm << std::endl; \
            break;
#include <Opcode.def>
#undef OPDEF
        default:
            strm << "<unknown>";
            break;
        }
        // If ilOffset ends a try block, catch block or filter block
        if( pHandler != nullptr )
        {
            ilOffset = p - it.begin();
            if( pHandler->IsHandlerEnd( ilOffset ) || pHandler->IsTryEnd( ilOffset ) )
            {
                indent = indent.substr( 0, indent.length( ) - 4 );
                strm << indent << "} " << std::endl;
            }
        }
    }
}
