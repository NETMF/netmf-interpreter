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
#include "stdafx.h"
#include <iostream>
#include "MetadataTables.h"
#include "CommonFormatting.h"

using namespace NETMF::Metadata;
using namespace NETMF::Graphics;

std::ios_base& HEX( std::ios_base& strm )
{
    return std::uppercase( std::hex( strm ) );
}

std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::VersionInfo const& version )
{
    strm << version.MajorVersion << '.' << version.MinorVersion << '.' << version.BuildNumber << '.' << version.RevisionNumber;
    return strm;
}

// create an std::string with whitespace control chars replaced as standard C++ char escape style
// That is, a newline, carriage return or tab will appear as a two character sequence (i.e. "\n" )
// in the resulting string
std::string FormatString( const char* peString )
{
    std::string retVal( peString );
    // not the most efficient implementation, but this is an illustrative app, not a production one...
    FindAndReplace( retVal, std::string("\n"), std::string("\\n") );
    FindAndReplace( retVal, std::string("\r"), std::string("\\r") );
    FindAndReplace( retVal, std::string("\t"), std::string("\\t") );
    return retVal;
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

std::ostream& operator<<( std::ostream& strm, AssemblyHeaderFlags flags )
{
    static std::map< AssemblyHeaderFlags, char const*> const flagNamesMap =
    { 
        { AssemblyHeaderFlags::None      , "None" },
        { AssemblyHeaderFlags::NeedReboot, "NeedReboot" },
        { AssemblyHeaderFlags::Patch     , "Patch" },
        { AssemblyHeaderFlags::NeedReboot, "NeedReboot" }
    };

    OutputEnumFlags( strm, flags, flagNamesMap );
    return strm;
}

std::ostream& operator<<( std::ostream& strm, TypeDefFlags flags )
{
    static std::map< TypeDefFlags, char const*> const flagNamesMap =
    {
        { TypeDefFlags::None                ,"None" },
        { TypeDefFlags::ScopeMask           ,"ScopeMask" },
        { TypeDefFlags::NotPublic           ,"NotPublic" },
        { TypeDefFlags::Public              ,"Public" },
        { TypeDefFlags::NestedPublic        ,"NestedPublic" },
        { TypeDefFlags::NestedPrivate       ,"NestedPrivate" },
        { TypeDefFlags::NestedFamily        ,"NestedFamily" },
        { TypeDefFlags::NestedAssembly      ,"NestedAssembly" },
        { TypeDefFlags::NestedFamANDAssem   ,"NestedFamANDAssem" },
        { TypeDefFlags::NestedFamORAssem    ,"NestedFamORAssem" },
        { TypeDefFlags::Serializable        ,"Serializable" },
        { TypeDefFlags::SemanticsMask       ,"SemanticsMask" },
        { TypeDefFlags::Class               ,"Class" },
        { TypeDefFlags::ValueType           ,"ValueType" },
        { TypeDefFlags::Interface           ,"Interface" },
        { TypeDefFlags::Enum                ,"Enum" },
        { TypeDefFlags::Abstract            ,"Abstract" },
        { TypeDefFlags::Sealed              ,"Sealed" },
        { TypeDefFlags::SpecialName         ,"SpecialName" },
        { TypeDefFlags::Delegate            ,"Delegate" },
        { TypeDefFlags::MulticastDelegate   ,"MulticastDelegate" },
        { TypeDefFlags::Patched             ,"Patched" },
        { TypeDefFlags::BeforeFieldInit     ,"BeforeFieldInit" },
        { TypeDefFlags::HasSecurity         ,"HasSecurity" },
        { TypeDefFlags::HasFinalizer        ,"HasFinalizer" },
        { TypeDefFlags::HasAttributes       ,"HasAttributes" },
    };

    OutputEnumFlags( strm, flags, flagNamesMap );
    return strm;
}

std::ostream& operator<<( std::ostream& strm, MethodDefFlags flags )
{
    static std::map< MethodDefFlags, char const*> const flagNamesMap =
    {
        { MethodDefFlags::ScopeMask            ,"ScopeMask" },
        { MethodDefFlags::PrivateScope         ,"PrivateScope" },
        { MethodDefFlags::Private              ,"Private" },
        { MethodDefFlags::FamANDAssem          ,"FamANDAssem" },
        { MethodDefFlags::Assem                ,"Assem" },
        { MethodDefFlags::Family               ,"Family" },
        { MethodDefFlags::FamORAssem           ,"FamORAssem" },
        { MethodDefFlags::Public               ,"Public" },
        { MethodDefFlags::Static               ,"Static" },
        { MethodDefFlags::Final                ,"Final" },
        { MethodDefFlags::Virtual              ,"Virtual" },
        { MethodDefFlags::HideBySig            ,"HideBySig" },
        { MethodDefFlags::VtableLayoutMask     ,"VtableLayoutMask" },
        { MethodDefFlags::ReuseSlot            ,"ReuseSlot" },
        { MethodDefFlags::NewSlot              ,"NewSlot" },
        { MethodDefFlags::Abstract             ,"Abstract" },
        { MethodDefFlags::SpecialName          ,"SpecialName" },
        { MethodDefFlags::NativeProfiled       ,"NativeProfiled" },
        { MethodDefFlags::Constructor          ,"Constructor" },
        { MethodDefFlags::StaticConstructor    ,"StaticConstructor" },
        { MethodDefFlags::Finalizer            ,"Finalizer" },
        { MethodDefFlags::DelegateConstructor  ,"DelegateConstructor" },
        { MethodDefFlags::DelegateInvoke       ,"DelegateInvoke" },
        { MethodDefFlags::DelegateBeginInvoke  ,"DelegateBeginInvoke" },
        { MethodDefFlags::DelegateEndInvoke    ,"DelegateEndInvoke" },
        { MethodDefFlags::Synchronized         ,"Synchronized" },
        { MethodDefFlags::GloballySynchronized ,"GloballySynchronized" },
        { MethodDefFlags::Patched              ,"Patched" },
        { MethodDefFlags::EntryPoint           ,"EntryPoint" },
        { MethodDefFlags::RequireSecObject     ,"RequireSecObject" },
        { MethodDefFlags::HasSecurity          ,"HasSecurity" },
        { MethodDefFlags::HasExceptionHandlers ,"HasExceptionHandlers" },
        { MethodDefFlags::HasAttributes        ,"HasAttributes" }
    };
    
    OutputEnumFlags( strm, flags, flagNamesMap );
    return strm;
}

std::ostream& operator<<( std::ostream& strm, FieldDefFlags flags )
{
    std::map<FieldDefFlags, char const*> const flagNamesMap =
    {
       { FieldDefFlags::None          , "None" },
       { FieldDefFlags::ScopeMask     , "ScopeMask" },
       { FieldDefFlags::Private       , "Private" },
       { FieldDefFlags::FamANDAssem   , "FamANDAssem" },
       { FieldDefFlags::Assembly      , "Assembly" },
       { FieldDefFlags::Family        , "Family" },
       { FieldDefFlags::FamORAssem    , "FamORAssem" },
       { FieldDefFlags::Public        , "Public" },
       { FieldDefFlags::NotSerialized , "NotSerialized" },
       { FieldDefFlags::Static        , "Static" },
       { FieldDefFlags::InitOnly      , "InitOnly" },
       { FieldDefFlags::Literal       , "Literal" },
       { FieldDefFlags::SpecialName   , "SpecialName" },
       { FieldDefFlags::HasDefault    , "HasDefault" },
       { FieldDefFlags::HasFieldRVA   , "HasFieldRVA" },
       { FieldDefFlags::NoReflection  , "NoReflection" },
       { FieldDefFlags::HasAttributes , "HasAttributes" }
    };

    OutputEnumFlags( strm, flags, flagNamesMap );
    return strm;
}

std::ostream& operator<<( std::ostream& strm, NETMF::Graphics::BitmapDescriptorFlags flags )
{
    static std::map< BitmapDescriptorFlags, char const*> flagNamesMap = 
    {
       { BitmapDescriptorFlags::None      , "None" },
       { BitmapDescriptorFlags::ReadOnly  , "ReadOnly" },
       { BitmapDescriptorFlags::Compressed, "Compressed" }
    };

    OutputEnumFlags( strm, flags, flagNamesMap );
    return strm;
}

std::ostream& operator<<( std::ostream& strm, NETMF::Graphics::BitmapImageType kind )
{
    switch( kind )
    {
    case NETMF::Graphics::BitmapImageType::TinyCLRBitmap:
        strm << "TinyCLRBitmap";
        break;
    case NETMF::Graphics::BitmapImageType::Gif:
        strm << "Gif";
        break;
    case NETMF::Graphics::BitmapImageType::JPeg:
        strm << "JPeg";
        break;
    case NETMF::Graphics::BitmapImageType::Bmp:
        strm << "Bmp";
        break;
    default:
        break;
    }
    return strm;
}

