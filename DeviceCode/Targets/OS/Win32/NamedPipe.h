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
#include <string>
#include "EnumFlags.h"
#include "Win32FileIo.h"

namespace Microsoft
{
    namespace Win32
    {
        enum class PipeAccessMode : uint32_t
        {
            Duplex = PIPE_ACCESS_DUPLEX,
            Inbound = PIPE_ACCESS_INBOUND,
            Outbound = PIPE_ACCESS_OUTBOUND,
            FirstPipeInstance = FILE_FLAG_FIRST_PIPE_INSTANCE,
            WriteThrough = FILE_FLAG_WRITE_THROUGH,
            Overlapped = FILE_FLAG_OVERLAPPED,
            WriteDAC = WRITE_DAC,
            WriteOwner = WRITE_OWNER,
            AccessSystemSecurity = ACCESS_SYSTEM_SECURITY
        };
        ENUM_FLAGS( PipeAccessMode )

        enum class PipeMode : uint32_t
        {
            WriteStream = PIPE_TYPE_BYTE,
            WriteMessage = PIPE_TYPE_MESSAGE,
            ReadStream = PIPE_READMODE_BYTE,
            ReadMessage = PIPE_READMODE_MESSAGE,
            Wait = PIPE_WAIT,
            NoWait = PIPE_NOWAIT,
            AcceptRemoteClients = PIPE_ACCEPT_REMOTE_CLIENTS,
            RejectRemoteClients = PIPE_REJECT_REMOTE_CLIENTS
        };
        ENUM_FLAGS( PipeMode )

        class NamedPipe : public Win32FileHandle
        {
        public:
            NamedPipe()
            {
            }

            bool Create( wchar_t const* name 
                       , PipeAccessMode accessMode = PipeAccessMode::Duplex | PipeAccessMode::FirstPipeInstance
                       , PipeMode pipeMode = PipeMode::ReadStream | PipeMode::WriteStream | PipeMode::RejectRemoteClients | PipeMode::Wait
                       , uint32_t maxInstances = 1
                       , uint32_t outbufferSize = 4096
                       , uint32_t inbufferSize  = 4096
                       , uint32_t defaultTimeout = 0
                       , SECURITY_ATTRIBUTES* pSecAttrib = nullptr
                       )
            {
                Name = name;
                auto hPipe = ::CreateNamedPipe( Name.c_str()
                                              , (uint32_t)accessMode
                                              , (uint32_t)pipeMode
                                              , maxInstances
                                              , outbufferSize
                                              , inbufferSize
                                              , defaultTimeout
                                              , pSecAttrib
                                              );
                InternalAssign( hPipe );
                return IsValid();
            }

            bool Connect(OVERLAPPED* pOverlapped = nullptr )
            {
                return !!::ConnectNamedPipe( operator HANDLE(), pOverlapped);
            }

            bool Open( wchar_t const* name, uint32_t timeout = NMPWAIT_WAIT_FOREVER  )
            {
                Name = name;
                if( !::WaitNamedPipe( Name.c_str(), timeout ) )
                    return false;

                auto hPipe = ::CreateFile( Name.c_str()
                                         , GENERIC_READ | GENERIC_WRITE
                                         , 0 // no sharing
                                         , nullptr
                                         , OPEN_EXISTING
                                         , FILE_ATTRIBUTE_NORMAL
                                         , nullptr
                                         );
                InternalAssign( hPipe );
                return IsValid();
            }

           static const std::wstring LocalPipeNamePrefix;

        private:
            std::wstring Name;
        };

        const std::wstring NamedPipe::LocalPipeNamePrefix = LR"(\\.\pipe\)";
    }
}