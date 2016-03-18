#ifndef _METADATAMODEL_H_
#define _METADATAMODEL_H_
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

#include <cstdint>
#include "EnumFlags.h"
#include "MetadataTables.h"

namespace NETMF
{
    namespace ImmutableMetadataModel
    {
        class AssemblyRef;
        class TypeRef;
        class MethodRef;
        class FieldRef;
        class Assembly;
        class TypeDef;
        class MethodDef;
        class FieldDef;
        
        class NamedReference
        {
        public:
            NamedReference( char const* name )
                : Name( name )
            { }

            char const* Name;
        };

        class AssemblyRef : public NamedReference
        {
        public:
            AssemblyRef( const char* name, Metadata::VersionInfo const& version )
                : NamedReference( name )
                , Version( version )
            { }

            AssemblyRef( Metadata::AssemblyHeader const& header, Metadata::AssemblyRefTableEntry const& entry )
                : AssemblyRef( header.LookupString( entry.Name ), entry.Version )
            {
            }

            Metadata::VersionInfo const& Version;
        };

        class TypeRef : public NamedReference
        {
        public:
            TypeRef( const char* nameSpaceName, const char* name )
                : NamedReference( name )
                , Namespace( nameSpaceName )
            { }

            TypeRef( Metadata::AssemblyHeader const& header, Metadata::TypeRefTableEntry const& entry )
                : TypeRef( header.LookupString( entry.Name ), header.LookupString( entry.NameSpace ) )
            { }

            char const* Namespace;
        };

        class FieldRef : public NamedReference
        {
        public:
            FieldRef( Metadata::AssemblyHeader const& header, Metadata::FieldRefTableEntry const& entry )
                : NamedReference( header.LookupString( entry.Name ) )
                , ContainingType( TypeRef( header, header.GetTable<Metadata::TypeRefTableEntry>()[ entry.Container ] ) )
            {
            }

            TypeRef const& ContainingType;
        };
    }
}
#endif

