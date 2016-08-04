////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <Windows.h>
#include "Win32FileIo.h"

namespace Microsoft
{
    namespace Win32
    {
        class MemoryMappedFile
        {
        public:
            MemoryMappedFile()
                : pData(nullptr)
                , Size(0)
            {
            }

            ~MemoryMappedFile()
            {
                Close();
            }

            MemoryMappedFile( const MemoryMappedFile& ) = delete;
            MemoryMappedFile& operator=( const MemoryMappedFile& ) = delete;
    
            bool Open( const wchar_t* pName, UINT32 maxSize)
            {
                Close();

                hFile = ::CreateFile( pName, FILE_GENERIC_READ | FILE_GENERIC_WRITE, 0, nullptr, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr );
                if( !hFile.IsValid() )
                    return false;

                hMapping = ::CreateFileMapping( (HANDLE)hFile, nullptr, PAGE_READWRITE, 0, maxSize, nullptr );
                if( !hMapping.IsValid() )
                    return false;

                pData = ::MapViewOfFile( (HANDLE)hMapping, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, maxSize );
                if( pData == nullptr )
                    return false;

                if( ::GetFileSize( (HANDLE)hFile, nullptr ) == 0 )
                    memset( pData, 0xFFFFFFFF, maxSize );
        
                Size = maxSize;
                return true;
            }

            bool Close()
            {
                Size = 0;
                bool retVal = true;
                if( pData != nullptr )
                {
                    retVal &= !!::UnmapViewOfFile( pData );
                    pData = nullptr;    
                }

                retVal &= hMapping.Close();
                retVal &= hFile.Close();
                return retVal;
            }

            UINT8* GetDataPointer()
            {
                return reinterpret_cast<UINT8*>( pData );
            }

            UINT32 Length()
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