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
#include <iostream>
#include "MetadataTables.h"

std::ios_base& HEX( std::ios_base& strm )
{
    return std::uppercase( std::hex( strm ) );
}

std::ostream& operator<<( std::ostream& strm, NETMF::Metadata::VersionInfo const& version )
{
    strm << version.MajorVersion << '.' << version.MinorVersion << '.' << version.BuildNumber << '.' << version.RevisionNumber;
    return strm;
}

template< typename T>
void OutputEnumFlagIfSet( std::ostream& strm, T value, T flag, char const* name, bool& outputSep )
{
    if( !EnumFlags::HasFlags( value, flag ) )
        return;

    if( outputSep )
        strm << " | ";

    strm << name;
    outputSep = true;
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