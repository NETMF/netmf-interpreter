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
#include "MetadataTables.h"

using namespace std;
using namespace NETMF::Metadata;
using namespace Microsoft::Utilities;

bool AssemblyHeader::FindMethodBoundaries( MethodDefTableEntry const& methodDef
                                         , MethodDefTableIndex i
                                         , MetadataOffset& start
                                         , MetadataOffset& end
                                         ) const
{
    MethodDefTableEntry const* p = &methodDef;
    if( p->RVA == EmptyIndex )
        return false;

    start = p->RVA;

    while(true)
    {
        p++;
        i++;

        if( i == TableElementCount( TableKind::MethodDef ) )
        {
            end = SizeOfTable( TableKind::ByteCode );
            break;
        }

        // methods with RVA ==  EmptyIndex are extern and considered InternalCall
        if( p->RVA != EmptyIndex )
        {
            end = p->RVA;
            break;
        }
    }

    return true;
}

int32_t AssemblyHeader::TableElementCount( TableKind kind ) const
{
    auto sizeInBytes = SizeOfTable( kind );
    switch( kind )
    {
    case TableKind::AssemblyRef:
        return sizeInBytes / sizeof( AssemblyRefTableEntry );

    case TableKind::TypeRef:
        return sizeInBytes / sizeof( TypeRefTableEntry );

    case TableKind::FieldRef:
        return sizeInBytes / sizeof( FieldRefTableEntry );

    case TableKind::MethodRef:
        return sizeInBytes / sizeof( MethodRefTableEntry );

    case TableKind::TypeDef:
        return sizeInBytes / sizeof( TypeDefTableEntry );

    case TableKind::FieldDef:
        return sizeInBytes / sizeof( FieldDefTableEntry );

    case TableKind::MethodDef:
        return sizeInBytes / sizeof( MethodDefTableEntry );

    case TableKind::Attributes:
        return sizeInBytes / sizeof( AttributeTableEntry );

    case TableKind::TypeSpec:
        return sizeInBytes / sizeof( TypeSpecTableEntry );

    case TableKind::Resources:
        return sizeInBytes / sizeof( ResourcesTableEntry );

    case TableKind::ResourcesFiles:
        return sizeInBytes / sizeof( ResourcesFilesTableEntry );

    // these are blob tables and don't have fixed records
    case TableKind::ResourcesData:
    case TableKind::Strings:
    case TableKind::Signatures:
    case TableKind::ByteCode:
        return -1;
        break;

    case TableKind::EndOfAssembly:
    case TableKind::Max:
    default:
        assert( false );
        return -1;
    }
}

char const* AssemblyHeader::LookupString( StringTableIndex i ) const
{
    static const StringTableIndex iMax = static_cast<StringTableIndex const>( 0xFFFF - StringTableIndexToBlobOffsetMapSize );

    if( i >= iMax )
    {
        return &WellKnownStringTableBlob[ StringTableIndexToBlobOffsetMap[ static_cast<StringTableIndex>( 0xFFFF - i ) ] ];
    }

    auto pTable = ( LPCSTR )GetTable( TableKind::Strings );
    return &( ( pTable )[ i ] );
}

// for details on compressed integers in the signature tables see ECMA-335 II.23.2
uint32_t AssemblyHeader::UncompressData( MetadataPtr& p )
{
    auto ptr = p;
    uint32_t val = *ptr++;
    // Handle smallest data inline.
    if((val & 0x80) == 0x00)          // 0xxx xxxx
    {
    }
    else
    {
        if( ( val & 0xC0 ) == 0x80 )  // 10xx xxxx
        {
            val = ( val & 0x3F ) << 8;
            val |= static_cast< uint32_t >( *ptr++ );
        }
        else                          // 110x xxxx
        {
            val = ( val & 0x1F ) << 24;
            val |= static_cast< uint32_t >( *ptr++ << 16 );
            val |= static_cast< uint32_t >( *ptr++ << 8 );
            val |= static_cast< uint32_t >( *ptr++ << 0 );
        }
    }
    p = ptr;

    return val;
}

void IterableExceptionHandlers::InitFromByteCode( MetadataPtr& pIlEnd )
{
    NumHandlers = *(--pIlEnd);
    if( NumHandlers == 0 )
        return;

    pIlEnd -= sizeof(ExceptionHandlerTableEntry) * NumHandlers;
    auto pHandlerStart = reinterpret_cast<ExceptionHandlerTableEntry const*>(pIlEnd);
    // deal with potential misalignment by making a copy
    pHandlers = new ExceptionHandlerTableEntry[ NumHandlers ];
    memcpy( const_cast<ExceptionHandlerTableEntry*>( pHandlers )
            , pHandlerStart
            , NumHandlers * sizeof( ExceptionHandlerTableEntry )
            );
}

IterableByteCodeStream::IterableByteCodeStream( AssemblyHeader const& header, MethodDefTableIndex i )
    : Header( header )
    , MethodDef( header.GetTable<MethodDefTableEntry>( )[ i ] )
{
    MetadataOffset startOffset, endOffset;
    auto pByteCode = Header.GetTable( TableKind::ByteCode );
    header.FindMethodBoundaries( i, startOffset, endOffset );
    IlStart = pByteCode + startOffset;
    IlEnd = pByteCode + endOffset;
    if( EnumFlags::HasFlags( MethodDef.Flags, MethodDefFlags::HasExceptionHandlers ) )
    {
        EhHandlers.InitFromByteCode( IlEnd );
    }
}

