////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files
// except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include "MemoryMappedFile.h"
#include "MetadataTables.h"
#include "Utilities.h"
#include "CommonFormatting.h"
#include "UnalignedAccess.h"
#include "MetadataModel.h"
#include "Graphics.h"

#define SHOW_IL_OPCODES 1

using namespace NETMF;
using namespace NETMF::Metadata;
using namespace Microsoft::Win32;
using namespace Microsoft::Utilities;

// file namespace not officially included in C++ library yet
// expected to be ratified pretty much 'as-is' into C++17
using namespace std::tr2::sys;

int ParseOneAssemblyFile( std::ostream& strm, path peFilePath );
extern void DecodeIlOpcodes( std::ostream& strm, AssemblyTableRef<NETMF::Metadata::MethodDefTableEntry> ref );

int wmain( int argc, wchar_t const* argv[ ] )
{
    if( argc != 2 )
    {
        std::cerr << "USAGE: " << argv[ 0 ] << " <path to pe files>" << std::endl;
        return ERROR_INVALID_PARAMETER;
    }

    path p( argv[ 1 ] );
    if( !is_directory( p ) )
    {
        std::cout << "No such directory: " << p << std::endl;
        return ERROR_PATH_NOT_FOUND;
    }
    
    for( const auto& entry : directory_iterator( p ) )
    {
        if( entry.path( ).extension( ).compare( ".pe" ) == 0 )
        {
            std::ofstream strm;
            auto dstFile = entry.path( ).filename().concat(L".txt");
            strm.open( dstFile, std::ofstream::out );
            ParseOneAssemblyFile( strm, entry.path( ) );
        }
    }

    std::cout << "done." << std::endl;
}

// function template to dump the contents of an AssemblyHeader table to an output stream
template<typename T>
void DumpTable( std::ostream& strm, AssemblyHeader const& header, std::string title )
{
    strm << title << std::endl;
    uint16_t index = 0;
    for( auto const& entry : header.GetTable<T>( ) )
    {
        strm << AssemblyTableRef<T>( header, entry, index++ ) << std::endl;
    }
}

void OutputAssemblyTableDetails( std::ostream& strm, AssemblyHeader const& header, std::string name, TableKind kind )
{
    // "ResourcesFiles" is the longest table name (14 chars) so use that for alignment padding
    strm << std::string( 14 - name.length( ), ' ' );
    strm << "        [" << name << "] 0x";
    strm << std::setw( 8 ) << header.StartOfTables[ ( int )kind ];
    strm << " { Padding: " << ( int )header.PaddingOfTables[ ( int )kind ];
    strm << ", Length: 0x" << std::setw( 8 ) << header.SizeOfTable( kind );
    if( kind < TableKind::EndOfAssembly )
    {
        auto elementCount = header.TableElementCount( kind );
        if( elementCount >= 0 )
            strm << ", Elements: " << std::dec << elementCount << HEX;
    }
    strm << " }" << std::endl;
}

std::ostream& operator<<( std::ostream& strm, AssemblyHeader const& header )
{
    char marker[ 8 ] = { '\0' };
    strncpy_s( marker, reinterpret_cast< char const* >( header.Marker ) , 8 );

    strm << "AssemblyHeader:" << HEX << std::setfill( '0' ) << std::endl;
    strm << "                Marker: " << marker << std::endl;
    strm << "             HeaderCRC: 0x" << header.HeaderCRC << ( header.VerifyHeaderCRC( ) ? " (good)" : " (bad!)" ) << std::endl;
    strm << "           AssemblyCRC: 0x" << header.AssemblyCRC << ( header.VerifyAssemblyCRC( ) ? " (good)" : " (bad!)" ) << std::endl;
    strm << "                 Flags: " << header.Flags << std::endl;
    strm << " NativeMethodsCheckSum: 0x" << header.NativeMethodsChecksum << std::endl;
    strm << "      PatchEntryOffset: 0x" << header.PatchEntryOffset << std::endl;
    strm << "               Version: " << header.Version << std::endl;
    strm << "                  Name: 0x" << header.AssemblyName << " { \"" << FormatString( header.LookupString( header.AssemblyName ) ) << "\" }" << std::endl;
    strm << "    StringTableVersion: " << header.StringTableVersion << std::endl;
    strm << "         StartOfTables:" << std::endl;
    OutputAssemblyTableDetails( strm, header, "AssemblyRef", TableKind::AssemblyRef );
    OutputAssemblyTableDetails( strm, header, "TypeRef", TableKind::TypeRef );
    OutputAssemblyTableDetails( strm, header, "FieldRef", TableKind::FieldRef );
    OutputAssemblyTableDetails( strm, header, "MethodRef", TableKind::MethodRef );
    OutputAssemblyTableDetails( strm, header, "TypeDef", TableKind::TypeDef );
    OutputAssemblyTableDetails( strm, header, "FieldDef", TableKind::FieldDef );
    OutputAssemblyTableDetails( strm, header, "MethodDef", TableKind::MethodDef );
    OutputAssemblyTableDetails( strm, header, "Attributes", TableKind::Attributes );
    OutputAssemblyTableDetails( strm, header, "TypeSpec", TableKind::TypeSpec );
    OutputAssemblyTableDetails( strm, header, "Resources", TableKind::Resources );
    OutputAssemblyTableDetails( strm, header, "ResourcesData", TableKind::ResourcesData );
    OutputAssemblyTableDetails( strm, header, "Strings", TableKind::Strings );
    OutputAssemblyTableDetails( strm, header, "Signatures", TableKind::Signatures );
    OutputAssemblyTableDetails( strm, header, "ByteCode", TableKind::ByteCode );
    OutputAssemblyTableDetails( strm, header, "ResourcesFiles", TableKind::ResourcesFiles );
    OutputAssemblyTableDetails( strm, header, "EndOfAssembly", TableKind::EndOfAssembly );

    strm << "     NumPatchedMethods: " << std::dec << header.NumPatchedMethods << HEX << std::endl;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<AssemblyRefTableEntry >const& ref )
{
    strm << "AssemblyRefTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "    Name: " << FormatString(ref.Header.LookupString( ref.Entry.Name ) )<< std::endl;
    strm << " Version: " << ref.Entry.Version << std::endl;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<TypeSpecTableEntry> const& ref )
{
    strm << "TypeSpecTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "    Signature: 0x" << ref.Entry.Sig;
    DumpSignature( strm, ref.Header, ref.Entry.Sig );
    strm << std::endl;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<TypeDefTableEntry> const& ref )
{
    strm << "TypeDefTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "                Name: " << FormatString( ref.Header.LookupString( ref.Entry.Name ) )<< std::endl;
    strm << "           Namespace: " << FormatString( ref.Header.LookupString( ref.Entry.NameSpace ) ) << std::endl;
    strm << "             Extends: 0x" << ref.Entry.Extends.Value << std::endl;
    strm << "       EnclosingType: 0x" << ref.Entry.EnclosingType << std::endl;
    strm << "          Interfaces: 0x" << ref.Entry.Interfaces << std::endl;
    strm << "         FirstMethod: 0x" << ref.Entry.FirstMethod << std::endl;
    strm << std::dec;
    strm << "  VirtualMethodCount: " << ( int )ref.Entry.VirtualMethodCount << std::endl;
    strm << " InstanceMethodCount: " << ( int )ref.Entry.InstanceMethodCount << std::endl;
    strm << "   StaticMethodCount: " << ( int )ref.Entry.StaticMethodCount << std::endl;
    strm << HEX;
    strm << "            DataType: 0x" << ( int )ref.Entry.dataType << std::endl;
    strm << "    FirstStaticField: 0x" << ref.Entry.FirstStaticField << std::endl;
    strm << "  FisrtInstanceField: 0x" << ref.Entry.FirstInstanceField << std::endl;
    strm << std::dec;
    strm << "    StaticFieldCount: " << ( int )ref.Entry.StaticFieldCount << std::endl;
    strm << "  InstanceFieldCount: " << ( int )ref.Entry.InstanceFieldCount << std::endl;
    strm << HEX;
    strm << "               Flags: " << ref.Entry.flags << std::endl;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<MethodDefTableEntry> const& ref )
{
    strm << "MethodDefTablEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "            Name: " << FormatString( ref.Header.LookupString( ref.Entry.Name ) ) << std::endl;
    strm << "             RVA: 0x" << ref.Entry.RVA << std::endl;
    strm << "           Flags: " << ref.Entry.Flags << std::endl;
    strm << std::dec;
    strm << "          RetVal: " << ( int )ref.Entry.RetVal << std::endl;
    strm << "        ArgCount: " << ( int )ref.Entry.NumArgs << std::endl;
    strm << "      LocalCount: " << ( int )ref.Entry.NumLocals << std::endl;
    strm << " LengthEvalStack: " << ( int )ref.Entry.LengthEvalStack << std::endl;
    strm << HEX;
    strm << "  LocalsSig: 0x" << ref.Entry.Locals;
    if( ref.Entry.Locals != EmptyIndex && ref.Entry.NumLocals > 0 )
    {
        auto p = ref.Header.GetTable( TableKind::Signatures ) + ref.Entry.Locals;
        strm << " [LOCALS ";
        DumpSignatureTypeSequence( strm, p, ref.Entry.NumLocals );
    }

    strm << std::endl;
    strm << "  Signature: " << ref.Entry.Sig;
    DumpSignature( strm, ref.Header, ref.Entry.Sig );
    strm << std::endl;
#if SHOW_IL_OPCODES
    DecodeIlOpcodes( strm, ref );
#endif
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<FieldDefTableEntry> const& ref )
{
    strm << "FieldDefTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "                Name: " << FormatString( ref.Header.LookupString( ref.Entry.Name ) ) << std::endl;
    strm << "    signature(index): 0x" << std::setw( 4 ) << ref.Entry.sig;
    DumpSignature( strm, ref.Header, ref.Entry.sig );
    strm << std::endl;
    strm << " DefaultValue(index): ";
    if( ref.Entry.DefaultValue == EmptyIndex )
        strm << "Empty";
    else
        strm << " 0x" << ref.Entry.DefaultValue;
    strm << std::endl;
    strm << "               Flags: " << ref.Entry.Flags << std::endl;
    return strm;
}

// While the bitmap and font formats are not technically part of the PE format
// they are NETMF defined standard resource types so this app dumps the basic
// header structures to verify the PE resource data is accessible.

std::ostream& operator<<( std::ostream& strm, NETMF::Graphics::BitmapDescriptor const& descriptor )
{
    strm << std::dec;
    strm << "    BitmapDescriptor:" << std::endl;
    strm << "        Width: " << descriptor.Width << std::endl;
    strm << "       Height: " << descriptor.Height << std::endl;
    strm << "        Flags: " << descriptor.Flags << std::endl;
    strm << "  BitPerPixel: " << descriptor.BitsPerPixel << std::endl;
    strm << "    ImageType: " << descriptor.ImageType << std::endl;
    strm << HEX;
    return strm;
}

void DumpFontMetrics( std::ostream& strm, NETMF::Graphics::FontMetrics const& metrics, std::string indent )
{
    strm << std::dec;
    strm << indent << "FontMetrics:" << std::endl;
    strm << indent << "          Height: " << metrics.Height << std::endl;
    strm << indent << "          Offset: " << metrics.Offset << std::endl;
    strm << indent << "          Ascent: " << metrics.Ascent << std::endl;
    strm << indent << "         Descent: " << metrics.Descent << std::endl;
    strm << indent << " InternalLeading: " << metrics.InternalLeading << std::endl;
    strm << indent << " ExternalLeading: " << metrics.ExternalLeading << std::endl;
    strm << indent << "    AvgCharWidth: " << metrics.AvgCharWidth << std::endl;
    strm << indent << "    MaxCharWidth: " << metrics.MaxCharWidth << std::endl;
    strm << HEX;
}

std::ostream& operator<<( std::ostream& strm, NETMF::Graphics::FontDescriptor const& descriptor )
{
    strm << "    FontDescriptor:" << std::endl;
    DumpFontMetrics( strm, descriptor.Metrics, "        ");
    strm << std::dec;
    strm << "        Ranges: " << descriptor.Ranges << std::endl;
    strm << "    Characters: " << descriptor.Characters << std::endl;
    strm << "         Flags: " << descriptor.Flags << std::endl;
    strm << HEX;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<ResourcesTableEntry> const& ref )
{
    strm << "ResourcesTableEntry[0x" << ref.Index << "]:" << std::endl;
    auto pTable = reinterpret_cast< ResourcesTableEntry const* >( ref.Header.GetTable( table_kind<ResourcesTableEntry>::value ) );
    auto& next = pTable[ ref.Index + 1 ];
    auto size = ref.Entry.SizeOfData( next );

    strm << "     id: 0x" << std::setw( 8 ) << ref.Entry.Id << std::endl;
    strm << "   kind: " << ref.Entry.Kind << std::endl;
    strm << "  flags: 0x" << std::setw( 8 ) << ref.Entry.Flags << std::endl;
    strm << " offset: 0x" << std::setw( 8 ) << ref.Entry.Offset << std::endl;
    strm << "   size: 0x" << std::setw( 8 ) << size << std::endl;

    MetadataPtr pData = ref.Entry.Offset + ref.Header.GetTable( TableKind::ResourcesData );
    switch( ref.Entry.Kind )
    {
    case ResourceKind::Invalid:
        break;
    case ResourceKind::String:
        strm << '\"' << reinterpret_cast< char const* >( pData ) << '\"' << std::endl;
        break;
    case ResourceKind::Bitmap:
        strm << *reinterpret_cast< NETMF::Graphics::BitmapDescriptor const* >( pData ) << std::endl;
        break;
    case ResourceKind::Font:
        strm << *reinterpret_cast< NETMF::Graphics::FontDescriptor const* >( pData ) << std::endl;
        break;
    case ResourceKind::Binary:
        strm << " byte[ " << std::dec << size << HEX << " ] " << std::endl;
        break;
    default:
        break;
    }
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<ResourcesFilesTableEntry> const& ref )
{
    strm << "ResourcesFileTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << std::dec;
    strm << "              version: " << ref.Entry.Version << ( ref.Entry.Version == ResourcesFilesTableEntry::CurrentVersion ? " [Current]" : " [Unknown]" ) << std::endl;
    strm << "         sizeofHeader: " << ref.Entry.SizeOfHeader << ( ref.Entry.SizeOfHeader == sizeof( ResourcesFilesTableEntry ) ? " [Current]" : " [Unknown]" ) << std::endl;
    strm << " sizeOfResourceHeader: " << ref.Entry.SizeOfResourceHeader << ( ref.Entry.SizeOfResourceHeader == sizeof( ResourcesTableEntry ) ? " [Current]" : " [Unknown]" ) << std::endl;
    strm << "    numberOfResources: " << ref.Entry.NumberOfResources << std::endl;
    strm << "                 Name: " << FormatString( ref.Header.LookupString( ref.Entry.Name ) ) << std::endl;
    strm << HEX;
    strm << "               offset: 0x" << ref.Entry.Offset << std::endl;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<AttributeTableEntry> const& ref )
{
    strm << "AttributeTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "   ownerKind: " << ref.Entry.OwnerType << std::endl;
    strm << std::dec;
    strm << "  ownerIndex: " << ref.Entry.OwnerIdx << std::endl;
    strm << " constructor: " << ref.Entry.Constructor << std::endl;
    strm << HEX;
    strm << "        data: " << ref.Entry.Data << std::endl;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<TypeRefTableEntry> const& ref )
{
    strm << "TypeRefTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "      Name: " << FormatString(ref.Header.LookupString( ref.Entry.Name ) ) << std::endl;
    strm << " Namespace: " << FormatString(ref.Header.LookupString( ref.Entry.NameSpace ) ) << std::endl;
    strm << "     scope: [ " << TypeFromToken( ref.Entry.Scope ) << ':' << std::setw( 4 ) << IndexFromToken( ref.Entry.Scope ) << "]" << std::endl;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<FieldRefTableEntry> const& ref )
{
    strm << "FieldRefTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "      Name: " << FormatString( ref.Header.LookupString( ref.Entry.Name ) ) << std::endl;
    strm << " Container: 0x" << std::setw( 4 ) << ref.Entry.Container << std::endl;
    strm << " Signature: 0x" << std::setw( 4 ) << ref.Entry.Sig;
    DumpSignature( strm, ref.Header, ref.Entry.Sig );
    strm << std::endl;
    return strm;
}

std::ostream& operator<<( std::ostream& strm, AssemblyTableRef<MethodRefTableEntry> const& ref )
{
    strm << "MethodRefTableEntry[0x" << ref.Index << "]:" << std::endl;
    strm << "      Name: " << FormatString( ref.Header.LookupString( ref.Entry.Name ) ) << std::endl;
    strm << " Container: 0x" << std::setw( 4 ) << ref.Entry.Container << std::endl;
    strm << " Signature: 0x" << std::setw( 4 ) << ref.Entry.Sig;
    DumpSignature( strm, ref.Header, ref.Entry.Sig );
    strm << std::endl;
    return strm;
}

int ParseOneAssemblyFile( std::ostream& strm, path peFilePath )
{
    MemoryMappedFile peFile;
    if( !peFile.Open( peFilePath.c_str( ) ) )
    {
        auto err = GetLastError( );
        std::cerr << "ERROR: " << err << std::endl;
        return err;
    }

    AssemblyHeader const& header = *peFile.GetConstDataPointer<AssemblyHeader>( );
    if( !header.VerifyCRCs( ) )
    {
        std::cerr << "Assembly CRCs Invalid '" << peFilePath << "'" << std::endl;
        return -1;
    }

    strm << header << std::endl;
    DumpTable<AssemblyRefTableEntry>( strm, header, "AssemblyRefTable:" );
    DumpTable<TypeRefTableEntry>( strm, header, "TypeRefTable:" );
    DumpTable<FieldRefTableEntry>( strm, header, "FieldRefTable:" );
    DumpTable<MethodRefTableEntry>( strm, header, "MethodRefTable:" );
    DumpTable<TypeDefTableEntry>( strm, header, "TypeDefTable:" );
    DumpTable<FieldDefTableEntry>( strm, header, "FieldDefTable:" );
    DumpTable<MethodDefTableEntry>( strm, header, "MethodDefTable:" );
    DumpTable<AttributeTableEntry>( strm, header, "AttributeTable:" );
    DumpTable<TypeSpecTableEntry>( strm, header, "TypeSpecTable:" );
    DumpTable<ResourcesTableEntry>( strm, header, "ResourcesTable:" );
    DumpTable<ResourcesFilesTableEntry>( strm, header, "ResourcesFilesTable:" );
    // The rest of the tables are all BLOB tables
    return 0;
}
