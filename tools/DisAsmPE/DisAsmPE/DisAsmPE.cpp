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

#define SHOW_IL_OPCODES 1

using namespace NETMF;
using namespace NETMF::Metadata;
using namespace Microsoft::Win32;
using namespace Microsoft::Utilities;
using namespace std::tr2::sys;

// Assembly table entries don't contain a back-link to the
// header, so this defines a simple reference for a given
// table entry to establish the link to the header necessary
// for resolving strings and other referenced tables.
template<typename T>
struct AssemblyTableRef
{
    AssemblyTableRef( AssemblyHeader const& header, T const& entry, uint16_t index )
        : Header( header )
        , Entry( entry )
        , Index( index )
    { }

    AssemblyHeader const& Header;
    T const& Entry;
    uint16_t const Index;
};

int ParseOneAssemblyFile( std::ostream& strm, path peFilePath );

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

// create an std::string with whitespace control chars replaced as standard C++ char escape style
std::string FormatString( const char* peString )
{
    std::string retVal( peString );
    // not the most efficient implementation, but this is an illustrative app, not a production one...
    FindAndReplace( retVal, std::string("\n"), std::string("\\n") );
    FindAndReplace( retVal, std::string("\r"), std::string("\\r") );
    FindAndReplace( retVal, std::string("\t"), std::string("\\t") );
    return retVal;
}

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

void DumpSignature( std::ostream& strm, AssemblyHeader const& header, MetadataPtr& p, size_t len = 0 )
{
    CallingConvention callingConvention = static_cast< CallingConvention >( *p++ );

    switch( callingConvention & CallingConvention::Mask )
    {
    case CallingConvention::Field:
        strm << " [FIELD]";
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

void DumpSignature( std::ostream& strm, AssemblyHeader const& header, SigTableIndex index, size_t len = 0 )
{
    if( index == EmptyIndex )
        return;

    MetadataPtr p = header.GetTable( TableKind::Signatures ) + index;
    DumpSignature( strm, header, p, len );
}

std::ostream& operator<<( std::ostream& strm, AssemblyHeaderFlags flags )
{
    auto allKnownFlags = AssemblyHeaderFlags::None
                       | AssemblyHeaderFlags::NeedReboot
                       | AssemblyHeaderFlags::Patch
                       | AssemblyHeaderFlags::NeedReboot;

    auto unknownFlags = ( flags & ~allKnownFlags );
    bool hasUnknownFlags = AssemblyHeaderFlags::None != unknownFlags;
    bool outputSep = false;
    if( flags == AssemblyHeaderFlags::None )
        strm << "None";
    else
    {
        OutputEnumFlagIfSet( strm, flags, AssemblyHeaderFlags::NeedReboot, "NeedReboot", outputSep );
        OutputEnumFlagIfSet( strm, flags, AssemblyHeaderFlags::Patch, "Patch", outputSep );
        OutputEnumFlagIfSet( strm, flags, AssemblyHeaderFlags::BigEndian, "BigEndian", outputSep );
        if( hasUnknownFlags )
        {
            if( outputSep )
                strm << " | ";
            strm << "<unknownflags>( 0x" << HEX << unknownFlags << " )";
        }
    }
    return strm;
}

std::ostream& operator<<( std::ostream& strm, TypeDefFlags flags )
{
    auto allKnownFlags = TypeDefFlags::None
                       | TypeDefFlags::ScopeMask
                       | TypeDefFlags::NotPublic
                       | TypeDefFlags::Public
                       | TypeDefFlags::NestedPublic
                       | TypeDefFlags::NestedPrivate
                       | TypeDefFlags::NestedFamily
                       | TypeDefFlags::NestedAssembly
                       | TypeDefFlags::NestedFamANDAssem
                       | TypeDefFlags::NestedFamORAssem
                       | TypeDefFlags::Serializable
                       | TypeDefFlags::SemanticsMask
                       | TypeDefFlags::Class
                       | TypeDefFlags::ValueType
                       | TypeDefFlags::Interface
                       | TypeDefFlags::Enum
                       | TypeDefFlags::Abstract
                       | TypeDefFlags::Sealed
                       | TypeDefFlags::SpecialName
                       | TypeDefFlags::Delegate
                       | TypeDefFlags::MulticastDelegate
                       | TypeDefFlags::Patched
                       | TypeDefFlags::BeforeFieldInit
                       | TypeDefFlags::HasSecurity
                       | TypeDefFlags::HasFinalizer
                       | TypeDefFlags::HasAttributes;

    auto unknownFlags = ( flags & ~allKnownFlags );
    bool hasUnknownFlags = TypeDefFlags::None != unknownFlags;
    bool outputSep = false;
    if( flags == TypeDefFlags::None )
        strm << "None";
    else
    {
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::ScopeMask, "ScopeMask", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::NotPublic, "NotPublic", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Public, "Public", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::NestedPublic, "NestedPublic", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::NestedPrivate, "NestedPrivate", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::NestedFamily, "NestedFamily", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::NestedAssembly, "NestedAssembly", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::NestedFamANDAssem, "NestedFamANDAssem", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::NestedFamORAssem, "NestedFamORAssem", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Serializable, "Serializable", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::SemanticsMask, "SemanticsMask", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Class, "Class", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::ValueType, "ValueType", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Interface, "Interface", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Enum, "Enum", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Abstract, "Abstract", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Sealed, "Sealed", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::SpecialName, "SpecialName", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Delegate, "Delegate", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::MulticastDelegate, "MulticastDelegate", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::Patched, "Patched", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::BeforeFieldInit, "BeforeFieldInit", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::HasSecurity, "HasSecurity", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::HasFinalizer, "HasFinalizer", outputSep );
        OutputEnumFlagIfSet( strm, flags, TypeDefFlags::HasAttributes, "HasAttributes", outputSep );

        if( hasUnknownFlags )
        {
            if( outputSep )
                strm << " | ";
            strm << "<unknownflags>( 0x" << HEX << unknownFlags << " )";
        }
    }
    return strm;
}

std::ostream& operator<<( std::ostream& strm, MethodDefFlags flags )
{
    auto allKnownFlags = MethodDefFlags::ScopeMask
                       | MethodDefFlags::PrivateScope
                       | MethodDefFlags::Private
                       | MethodDefFlags::FamANDAssem
                       | MethodDefFlags::Assem
                       | MethodDefFlags::Family
                       | MethodDefFlags::FamORAssem
                       | MethodDefFlags::Public
                       | MethodDefFlags::Static
                       | MethodDefFlags::Final
                       | MethodDefFlags::Virtual
                       | MethodDefFlags::HideBySig
                       | MethodDefFlags::VtableLayoutMask
                       | MethodDefFlags::ReuseSlot
                       | MethodDefFlags::NewSlot
                       | MethodDefFlags::Abstract
                       | MethodDefFlags::SpecialName
                       | MethodDefFlags::NativeProfiled
                       | MethodDefFlags::Constructor
                       | MethodDefFlags::StaticConstructor
                       | MethodDefFlags::Finalizer
                       | MethodDefFlags::DelegateConstructor
                       | MethodDefFlags::DelegateInvoke
                       | MethodDefFlags::DelegateBeginInvoke
                       | MethodDefFlags::DelegateEndInvoke
                       | MethodDefFlags::Synchronized
                       | MethodDefFlags::GloballySynchronized
                       | MethodDefFlags::Patched
                       | MethodDefFlags::EntryPoint
                       | MethodDefFlags::RequireSecObject
                       | MethodDefFlags::HasSecurity
                       | MethodDefFlags::HasExceptionHandlers
                       | MethodDefFlags::HasAttributes;

    auto unknownFlags = ( flags & ~allKnownFlags );
    bool hasUnknownFlags = MethodDefFlags::None != unknownFlags;
    bool outputSep = false;
    if( flags == MethodDefFlags::None )
        strm << "None";
    else
    {
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::ScopeMask, "ScopeMask", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::PrivateScope, "PrivateScope", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Private, "Private", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::FamANDAssem, "FamANDAssem", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Assem, "Assem", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Family, "Family", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::FamORAssem, "FamORAssem", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Public, "Public", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Static, "Static", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Final, "Final", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Virtual, "Virtual", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::HideBySig, "HideBySig", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::VtableLayoutMask, "VtableLayoutMask", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::ReuseSlot, "ReuseSlot", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::NewSlot, "NewSlot", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Abstract, "Abstract", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::SpecialName, "SpecialName", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::NativeProfiled, "NativeProfiled", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Constructor, "Constructor", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::StaticConstructor, "StaticConstructor", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Finalizer, "Finalizer", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::DelegateConstructor, "DelegateConstructor", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::DelegateInvoke, "DelegateInvoke", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::DelegateBeginInvoke, "DelegateBeginInvoke", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::DelegateEndInvoke, "DelegateEndInvoke", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Synchronized, "Synchronized", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::GloballySynchronized, "GloballySynchronized", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::Patched, "Patched", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::EntryPoint, "EntryPoint", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::RequireSecObject, "RequireSecObject", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::HasSecurity, "HasSecurity", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::HasExceptionHandlers, "HasExceptionHandlers", outputSep );
        OutputEnumFlagIfSet( strm, flags, MethodDefFlags::HasAttributes, "HasAttributes", outputSep );

        if( hasUnknownFlags )
        {
            if( outputSep )
                strm << " | ";
            strm << "<unknownflags>( 0x" << HEX << unknownFlags << " )";
        }
    }
    return strm;
}

std::ostream& operator<<( std::ostream& strm, FieldDefFlags flags )
{
    auto allKnownFlags = FieldDefFlags::None
                       | FieldDefFlags::ScopeMask
                       | FieldDefFlags::Private
                       | FieldDefFlags::FamANDAssem
                       | FieldDefFlags::Assembly
                       | FieldDefFlags::Family
                       | FieldDefFlags::FamORAssem
                       | FieldDefFlags::Public
                       | FieldDefFlags::NotSerialized
                       | FieldDefFlags::Static
                       | FieldDefFlags::InitOnly
                       | FieldDefFlags::Literal
                       | FieldDefFlags::SpecialName
                       | FieldDefFlags::HasDefault
                       | FieldDefFlags::HasFieldRVA
                       | FieldDefFlags::NoReflection
                       | FieldDefFlags::HasAttributes;

    auto unknownFlags = ( flags & ~allKnownFlags );
    bool hasUnknownFlags = FieldDefFlags::None != unknownFlags;
    bool outputSep = false;
    if( flags == FieldDefFlags::None )
        strm << "None";
    else
    {
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::ScopeMask, "ScopeMask", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::Private, "Private", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::FamANDAssem, "FamANDAssem", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::Assembly, "Assembly", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::Family, "Family", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::FamORAssem, "FamORAssem", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::Public, "Public", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::NotSerialized, "NotSerialized", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::Static, "Static", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::InitOnly, "InitOnly", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::Literal, "Literal", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::SpecialName, "SpecialName", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::HasDefault, "HasDefault", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::HasFieldRVA, "HasFieldRVA", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::NoReflection, "NoReflection", outputSep );
        OutputEnumFlagIfSet( strm, flags, FieldDefFlags::HasAttributes, "HasAttributes", outputSep );

        if( hasUnknownFlags )
        {
            if( outputSep )
                strm << " | ";
            strm << "<unknownflags>( 0x" << HEX << unknownFlags << " )";
        }
    }
    return strm;
}

void OutputAssemblyTableDetails( std::ostream& strm, AssemblyHeader const& header, std::string name, TableKind kind )
{
    // "ResourcesFiles" is the longest name (14 chars) so use that for alignment padding
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

std::ostream& operator<<( std::ostream& strm, TableKind kind )
{
    switch( kind )
    {
    case NETMF::Metadata::TableKind::AssemblyRef:
        strm << "AssemblyRef";
        break;
    case NETMF::Metadata::TableKind::TypeRef:
        strm << "TypeRef";
        break;
    case NETMF::Metadata::TableKind::FieldRef:
        strm << "FieldRef";
        break;
    case NETMF::Metadata::TableKind::MethodRef:
        strm << "MethodRef";
        break;
    case NETMF::Metadata::TableKind::TypeDef:
        strm << "TypeDef";
        break;
    case NETMF::Metadata::TableKind::FieldDef:
        strm << "FieldDef";
        break;
    case NETMF::Metadata::TableKind::MethodDef:
        strm << "MethodDef";
        break;
    case NETMF::Metadata::TableKind::Attributes:
        strm << "Attributes";
        break;
    case NETMF::Metadata::TableKind::TypeSpec:
        strm << "TypeSpec";
        break;
    case NETMF::Metadata::TableKind::Resources:
        strm << "Resources";
        break;
    case NETMF::Metadata::TableKind::ResourcesData:
        strm << "ResourcesData";
        break;
    case NETMF::Metadata::TableKind::Strings:
        strm << "Strings";
        break;
    case NETMF::Metadata::TableKind::Signatures:
        strm << "Signatures";
        break;
    case NETMF::Metadata::TableKind::ByteCode:
        strm << "ByteCode";
        break;
    case NETMF::Metadata::TableKind::ResourcesFiles:
        strm << "ResourcesFiles";
        break;
    case NETMF::Metadata::TableKind::EndOfAssembly:
        strm << "EndOfAssembly";
        break;
    default:
        strm << "<invalid>";
        break;
    }
    return strm;
}

template< TableKind bitSetKind, TableKind bitUnsetKind>
std::ostream& operator<<( std::ostream& strm, BinaryToken<bitSetKind,bitUnsetKind> token )
{
    strm << "[ " << token.GetTableKind( ) << ": " << token.GetIndex( ) << " ]";
    return strm;
}

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

std::ostream& operator<<( std::ostream& strm, ResourceKind kind )
{
    switch( kind )
    {
    case NETMF::Metadata::ResourceKind::Invalid:
        strm << "Invalid";
        break;
    case NETMF::Metadata::ResourceKind::Bitmap:
        strm << "Bitmap";
        break;
    case NETMF::Metadata::ResourceKind::Font:
        strm << "Font";
        break;
    case NETMF::Metadata::ResourceKind::String:
        strm << "String";
        break;
    case NETMF::Metadata::ResourceKind::Binary:
        strm << "Binary";
        break;
    default:
        strm << "Unknown ResourceKind: " << static_cast< uint16_t >( kind );
        break;
    }
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
    case ResourceKind::Font:
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

std::ostream& operator<<( std::ostream& strm, MethodRefOrMethodDef token )
{
    strm << "[ " << token.GetTableKind( ) << " : " << token.GetIndex( ) << " ]";
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
