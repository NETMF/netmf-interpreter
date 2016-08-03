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

#pragma once
#include <functional>
#include <regex>

namespace Microsoft
{
    namespace Win32
    {
        struct Command_Call
            : CLR_RT_ParseOptions::Command
        {
            typedef std::function< HRESULT( CLR_RT_ParseOptions::ParameterList* params )> HandlerFunc_t;

            Command_Call( HandlerFunc_t handler, LPCWSTR szName, LPCWSTR szDescription )
                : CLR_RT_ParseOptions::Command( szName, szDescription )
                , Handler( handler )
            {
            }

            virtual HRESULT Execute( ) override
            {
                return Handler( &m_params );
            }

            HandlerFunc_t Handler;
        };

        struct Win32Settings
            : CLR_RT_ParseOptions
        {
            CLR_RT_ParseOptions::BufferMap  m_assemblies;

            int                             m_idxDatBufferIndex;
            bool                            m_fWaitForDebugger;
            bool                            m_fPerformGarbageCollection;
            bool                            m_fPerformHeapCompaction;
            bool                            m_fNoExecuteIL;
            bool                            m_fWaitOnExit;
            bool                            m_fNoNetwork;

            std::wstring                    m_szCommandLineArgs;

            Win32Settings( );
            ~Win32Settings( );

        public:
            HRESULT ParseCmdLineOptions( int argc, wchar_t const* argv[ ] );
            HRESULT ProcessCommandLineSettings( );

        private:
            HRESULT LoadAssemblies( );
            HRESULT ExtractOptionsFromFile( LPCWSTR szFileName );

            HRESULT CheckAssemblyFormat( CLR_RECORD_ASSEMBLY* header, LPCWSTR src );

            HRESULT Execute( );

            HRESULT Cmd_Cfg( CLR_RT_ParseOptions::ParameterList* params = NULL );

            HRESULT Cmd_Load( CLR_RT_ParseOptions::ParameterList* params = NULL );

            HRESULT Cmd_LoadDatabase( CLR_RT_ParseOptions::ParameterList* params = NULL );

            HRESULT Cmd_Resolve( CLR_RT_ParseOptions::ParameterList* params = NULL );

            HRESULT Cmd_Execute( CLR_RT_ParseOptions::ParameterList* params = NULL );

            HRESULT Cmd_CommandLineArgs( CLR_RT_ParseOptions::ParameterList* params = NULL );

            HRESULT Cmd_ListNICs( CLR_RT_ParseOptions::ParameterList* params = NULL );
            HRESULT Set_NicGuid( CLR_RT_ParseOptions::ParameterList* params = NULL );
            HRESULT Set_NicMAC( CLR_RT_ParseOptions::ParameterList* params = NULL );
            HRESULT Cmd_EraseFlash( CLR_RT_ParseOptions::ParameterList* params = NULL );

            void Usage( );

            void BuildOptions( );

            HRESULT LoadDataBase( std::wstring path );
            HRESULT LoadAssembly( std::wstring path );

            static HRESULT LoadAssembly( CLR_RECORD_ASSEMBLY* header, CLR_RT_Assembly*& assm, LPCWSTR szName );

            std::wstring get_NicGuid( )
            {
                return NicGuid;
            }

        private:
            std::list<std::wstring> AssemblyPaths;
            std::list<std::wstring> DatPaths;
            std::wstring NicGuid;
            bool EraseFlash;
            std::list<std::function<bool( void )>> PostParseProcessCommands;
            std::wstring MacAddress;
            std::wsmatch MacAddressParseResults;
        };
    }
}