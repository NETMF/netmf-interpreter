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
#include <cstdint>
#include <atomic>
#include <string>
#include "EnumFlags.h"
#include "Win32Handles.h"

namespace Microsoft
{
    namespace Win32
    {
        // file flags enumeration for file HANDLES
        enum class FileFlags : uint32_t
        {
            FirstPipeInstance = FILE_FLAG_FIRST_PIPE_INSTANCE,
            WriteThrough = FILE_FLAG_WRITE_THROUGH,
            Overlapped = FILE_FLAG_OVERLAPPED,
        };
        ENUM_FLAGS( FileFlags )
        
        // class to contain Win32 File HANDLE
        class Win32FileHandle 
            : public Win32Handle< HANDLE, StdNullInvalidHandleTraits> 
        {
        public:
            using Win32Handle< HANDLE, StdNullInvalidHandleTraits >::operator=;

            bool GetOverlappedResult( OVERLAPPED* pOverlapped, _Out_ uint32_t& numberOfBytes, bool wait = true)
            {
                return !!::GetOverlappedResult( operator HANDLE()
                                              , pOverlapped
                                              , reinterpret_cast<LPDWORD>(&numberOfBytes)
                                              , wait ? TRUE : FALSE
                                              );
            }

            bool Write( _In_reads_bytes_(bufferLen) const void* pBuffer
                      ,  uint32_t bufferLen
                      , _Out_opt_ uint32_t* pNumberOfBytesWritten = nullptr
                      , _Inout_opt_ OVERLAPPED* pOverlapped = nullptr
                      )
            {
                return !!::WriteFile( operator HANDLE()
                                    , pBuffer
                                    , bufferLen
                                    , reinterpret_cast<LPDWORD>(pNumberOfBytesWritten)
                                    , pOverlapped
                                    );
            }

            bool Read( _Out_writes_bytes_(bufferLen) void* pBuffer
                     , uint32_t bufferLen
                     , _Out_opt_ uint32_t* pNumberOfBytesRead = nullptr
                     , _Inout_opt_ OVERLAPPED* pOverlapped  = nullptr
                     )
            {
                return !!::ReadFile( operator HANDLE()
                                   , pBuffer
                                   , bufferLen
                                   , reinterpret_cast<LPDWORD>( pNumberOfBytesRead )
                                   , pOverlapped
                                   );
            }

            bool Flush()
            {
                return !!::FlushFileBuffers( operator HANDLE() );
            }
        };
    }
}
