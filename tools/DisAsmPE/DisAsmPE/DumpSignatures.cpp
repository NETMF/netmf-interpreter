#include "stdafx.h"
#include "MetadataTables.h"

using namespace NETMF::Metadata;

void DumpSignatureDataType( std::ostream& strm, MetadataPtr& p )
{
    auto code = static_cast< DataType >( AssemblyHeader::UncompressData( p ) );
    switch( code )
    {
    case DataType::Void:
        strm << "VOID";
        break;
    case DataType::Boolean:
        strm << "BOOLEAN";
        break;
    case DataType::I1:
        strm << "I1";
        break;
    case DataType::U1:
        strm << "U1";
        break;
    case DataType::CHAR:
        strm << "CHAR";
        break;
    case DataType::I2:
        strm << "I2";
        break;
    case DataType::U2:
        strm << "U2";
        break;
    case DataType::I4:
        strm << "I4";
        break;
    case DataType::U4:
        strm << "U4";
        break;
    case DataType::R4:
        strm << "R4";
        break;
    case DataType::I8:
        strm << "I8";
        break;
    case DataType::U8:
        strm << "U8";
        break;
    case DataType::R8:
        strm << "R8";
        break;
    case DataType::String:
        strm << "STRING";
        break;
    case DataType::Object:
        strm << "OBJECT";
        break;
    case DataType::Class:
        strm << "CLASS 0x" << TokenFromStream( p );
        break;
    case DataType::ValueType:
        strm << "VALUETYPE 0x" << TokenFromStream( p );
        break;
    case DataType::SZArray:
        strm << "SZARRAY ";
        DumpSignatureDataType( strm, p );
        break;
    case DataType::ByRef:
        strm << "BYREF ";
        DumpSignatureDataType( strm, p );
        break;
    default:
        strm << "UNKNOWN 0x" << static_cast< int >( code );
        break;
    }
}

void DumpSignatureTypeSequence( std::ostream& strm, MetadataPtr& p, size_t len )
{
    strm << '(';
    while( len-- > 0 )
    {
        strm << ' ';
        DumpSignatureDataType( strm, p );
        strm << ( len ? ',' : ' ' );
    }
    strm << ")]";
}

void DumpSignature( std::ostream& strm, AssemblyHeader const& header, MetadataPtr& p, size_t len /*= 0*/ )
{
    CallingConvention callingConvention = static_cast< CallingConvention >( *p++ );

    switch( callingConvention & CallingConvention::Mask )
    {
    case CallingConvention::Field:
        strm << " [FIELD] ";
        DumpSignatureDataType( strm, p );
        break;
    case CallingConvention::LocalSig:
        assert( false );
        break;
    case CallingConvention::Default:
        strm << " [METHOD ";
        len = *p++;
        DumpSignatureDataType( strm, p );          // return type
        DumpSignatureTypeSequence( strm, p, len ); // arguments
        break;
    }
}

void DumpSignature( std::ostream& strm, AssemblyHeader const& header, SigTableIndex index, size_t len/* = 0*/ )
{
    if( index == EmptyIndex )
        return;

    MetadataPtr p = header.GetTable( TableKind::Signatures ) + index;
    DumpSignature( strm, header, p, len );
}
