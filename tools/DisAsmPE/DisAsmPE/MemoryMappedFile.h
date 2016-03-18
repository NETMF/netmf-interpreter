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

#include <Windows.h>
#include "Win32FileIo.h"
#include "ScopeGuard.h"

namespace Microsoft
{
    namespace Win32
    {
        class MemoryMappedFile
        {
        public:
            MemoryMappedFile( )
                : pData( nullptr )
                , Size( 0 )
            { }

            ~MemoryMappedFile( )
            {
                Close( );
            }

            // disallow copy and assignment
            MemoryMappedFile( const MemoryMappedFile& ) = delete;
            MemoryMappedFile& operator=( const MemoryMappedFile& ) = delete;

            bool Open( const wchar_t* pName
                     , DWORD flags = FILE_GENERIC_READ
                     , DWORD pageFlags = PAGE_READONLY
                     , DWORD mapFlags = FILE_MAP_READ
                     , UINT32 maxSize = 0
                     )
            {
                Close( );

                hFile = ::CreateFile( pName, flags, 0, nullptr, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr );
                if( !hFile.IsValid( ) )
                    return false;

                hMapping = ::CreateFileMapping( ( HANDLE )hFile, nullptr, pageFlags, 0, maxSize, nullptr );
                if( !hMapping.IsValid( ) )
                    return false;

                pData = ::MapViewOfFile( ( HANDLE )hMapping, mapFlags, 0, 0, maxSize );
                if( pData == nullptr )
                    return false;

                Size = ::GetFileSize( ( HANDLE )hFile, nullptr );
                return true;
            }

            bool Close( )
            {
                Size = 0;
                bool retVal = true;
                if( pData != nullptr )
                {
                    retVal &= !!::UnmapViewOfFile( pData );
                    pData = nullptr;
                }

                retVal &= hMapping.Close( );
                retVal &= hFile.Close( );
                return retVal;
            }

            template<typename T>
            T const* GetConstDataPointer( )
            {
                return reinterpret_cast<T const *>( pData );
            }

            template<typename T>
            T* GetDataPointer( )
            {
                return reinterpret_cast<T*>( pData );
            }

            UINT32 Length( )
            {
                return Size;
            }

        private:
            Win32FileHandle hFile;
            Win32FileHandle hMapping;
            void* pData;
            UINT32 Size;
        };
    }
}