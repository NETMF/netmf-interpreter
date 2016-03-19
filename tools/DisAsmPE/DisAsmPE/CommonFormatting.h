#pragma once
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
#include <map>
#include <iostream>
#include "MetadataTables.h"
#include "Graphics.h"


// This module defines basic formating for enums and simple structures that otherwise
// consume vertical code space without contributing to the overall comprehension of
// the core PE data format

// Description:
//   Equivalent to using std::uppercase << std::hex
std::ios_base& HEX( std::ios_base& strm );

// Stream output for a metadata version structure
std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::VersionInfo const& version );

std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::AssemblyHeaderFlags flags );
std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::TypeDefFlags flags );
std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::MethodDefFlags flags );
std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::FieldDefFlags flags );
std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::TableKind kind );
std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::ResourceKind kind );

std::ostream& operator<<( std::ostream& strm, NETMF::Graphics::BitmapDescriptorFlags flags );
std::ostream& operator<<( std::ostream& strm, NETMF::Graphics::BitmapImageType kind );

// create an std::string with whitespace control chars replaced as standard C++ char escape style
std::string FormatString( const char* peString );

// Function template to output an individual flag if it is set
// Template args:
//    T - Enumeration (must be marked with ENUM_FLAGS() )
//
// Input:
//       strm - stream to output flag to
//      value - value to check if the flag is set in
//       flag - flag to check, if it is set then the name is output
//       name - string name of the flag
//  outputSep - flag to indicate if the separator '|' should be output before the flags name
//
// Output:
//  outputSep - update to true after the call to allow the separator for allow but the first flag
//
template< typename T>
void OutputEnumFlagIfSet( std::ostream& strm, T value, T flag, char const* name, bool& outputSep )
{
    if( !Microsoft::Utilities::EnumFlags::HasFlags( value, flag ) )
        return;

    if( outputSep )
        strm << " | ";

    strm << name;
    outputSep = true;
}

//////////////////////////////////////////////////////////////////////////
// Description:
//   Function template to format the flags of a flags enumerated value
//
// Template parameters:
//   T enumeration type, must have ENUM_FLAGS(T) applied.
// 
// Input:
//    strm - stream to output the formated data to
//   value - flags value to format
//   names - map of flag enumerations to string names
//
// Remarks:
//    Generates a string of the form "flag1 | flag2 | flag3"
//
template<typename T>
void OutputEnumFlags( std::ostream& strm, T value, std::map< T, char const*> const names )
{
    T allKnownFlags;
    for( auto const entry : names )
    {
        allKnownFlags |= entry.first;
    }

    auto unknownFlags = ( value & ~allKnownFlags );
    bool hasUnknownFlags = static_cast<T>(0) != unknownFlags;
    if( value == static_cast<T>(0) )
        strm << names.at(static_cast<T>(0));
    else
    {
        bool outputSep = false;
        for( auto const entry : names )
        {
            OutputEnumFlagIfSet<T>( strm, value, entry.first, entry.second, outputSep );
        }

        if( hasUnknownFlags )
        {
            if( outputSep )
                strm << " | ";
            strm << "<unknownflags>( 0x" << HEX << unknownFlags << " )";
        }
    }
}

template<typename T>
inline void FindAndReplace( T& source, const T& find, const T& replace )
{
    auto findLen = find.size();
    auto replaceLen = replace.size();

    auto pos = source.find( find, 0 );
    while( pos != T::npos )
    {
        source.replace( pos, findLen, replace);
        pos += replaceLen;
        pos = source.find( find, pos );
    }
}

template< NETMF::Metadata::TableKind bitSetKind, NETMF::Metadata::TableKind bitUnsetKind>
std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::BinaryToken<bitSetKind,bitUnsetKind> token )
{
    strm << "[ " << token.GetTableKind( ) << ": " << token.GetIndex( ) << " ]";
    return strm;
}

// Assembly table entries don't contain a back-link to the
// header, so this defines a simple reference for a given
// table entry to establish the link to the header necessary
// for resolving strings and other referenced tables.
template<typename T>
struct AssemblyTableRef
{
    AssemblyTableRef( NETMF::Metadata::AssemblyHeader const& header, T const& entry, uint16_t index )
        : Header( header )
        , Entry( entry )
        , Index( index )
    { }

    NETMF::Metadata::AssemblyHeader const& Header;
    T const& Entry;
    uint16_t const Index;
};

void DumpSignatureDataType( std::ostream& strm, NETMF::Metadata::MetadataPtr& p );
void DumpSignatureTypeSequence( std::ostream& strm, NETMF::Metadata::MetadataPtr& p, size_t len );
void DumpSignature( std::ostream& strm, NETMF::Metadata::AssemblyHeader const& header, NETMF::Metadata::MetadataPtr& p, size_t len = 0 );
void DumpSignature( std::ostream& strm, NETMF::Metadata::AssemblyHeader const& header, NETMF::Metadata::SigTableIndex index, size_t len = 0 );

