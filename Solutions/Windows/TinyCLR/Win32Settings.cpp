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

#include "stdafx.h"
#include "Win32Settings.h"
#include <Shellapi.h>
#include <TinyCLR_Jitter.h>
#include <TinyCLR_PlatformDef.h>

#include "MemoryMappedFile.h"
#include "WinPCAPDeviceList.h"
#include "WinPcap_Eth_lwIP_Adapter.h"
#include "OutputDebugStream.h"

extern WINPCAP_ETH_LWIP_DEVICE_CONFIG g_WINPCAP_ETH_LWIP_Config;

using namespace Microsoft::Win32;

extern MemoryMappedFile FlashMemoryFile;

Win32Settings::Win32Settings( )
    : m_idxDatBufferIndex( 0 )
    , m_fWaitForDebugger( false )
    , m_fPerformGarbageCollection( false )
    , m_fPerformHeapCompaction( false )
    , m_fNoExecuteIL( false )
    , m_fWaitOnExit( false )
    , m_fNoNetwork( false )
{
    //  BuildOptions loads all of the known commands which will be parsed
    //  from the command line.
    BuildOptions( );
}

Win32Settings::~Win32Settings( )
{
    for( CLR_RT_ParseOptions::BufferMapIter it = m_assemblies.begin( ); it != m_assemblies.end( ); it++ )
    {
        delete it->second;
    }
}

HRESULT Win32Settings::ExtractOptionsFromFile( LPCWSTR szFileName )
{
    HRESULT hr;

    CLR_RT_StringVector vec;
    hr = CLR_RT_FileStore::ExtractTokensFromFile( szFileName, vec );
    if( FAILED( hr ) )
        return TINYCLR_DEBUG_PROCESS_EXCEPTION( hr, NULL, NULL, 0 );


    hr = ProcessOptions( vec );
    if( FAILED( hr ) )
        hr = TINYCLR_DEBUG_PROCESS_EXCEPTION( hr, NULL, NULL, 0 );

    return hr;
}

HRESULT Win32Settings::CheckAssemblyFormat( CLR_RECORD_ASSEMBLY* header, LPCWSTR src )
{
    if( header->GoodAssembly( ) )
        return S_OK;
        
    wprintf( L"Invalid assembly format for '%s': ", src );
    for( int i = 0; i < sizeof( header->marker ); i++ )
    {
        wprintf( L"%02x", header->marker[ i ] );
    }
    wprintf( L"\n" );

    return E_FAIL;
}

#define PARAM_GENERIC(parm1Name,parm1Desc)     param = new CLR_RT_ParseOptions::Parameter_Generic(      parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_STRING(val,parm1Name,parm1Desc)  param = new CLR_RT_ParseOptions::Parameter_String ( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_BOOLEAN(val,parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Boolean( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_INTEGER(val,parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Integer( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_FLOAT(val,parm1Name,parm1Desc)   param = new CLR_RT_ParseOptions::Parameter_Float  ( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )

#define PARAM_EXTRACT_STD_STRING(lst,idx)    ((CLR_RT_ParseOptions::Parameter_Generic*)(*lst)[idx])->m_data

#define PARAM_EXTRACT_STRING(lst,idx)    ((CLR_RT_ParseOptions::Parameter_Generic*)(*lst)[idx])->m_data.c_str()
#define PARAM_EXTRACT_BOOLEAN(lst,idx) *(((CLR_RT_ParseOptions::Parameter_Boolean*)(*lst)[idx])->m_dataPtr)
#define PARAM_EXTRACT_INTEGER(lst,idx) *(((CLR_RT_ParseOptions::Parameter_Integer*)(*lst)[idx])->m_dataPtr)

#define OPTION_GENERIC(optName,optDesc)  cmd = new CLR_RT_ParseOptions::Command        (      optName, optDesc ); m_commands.push_back( cmd )
#define OPTION_SET(val,optName,optDesc)  cmd = new CLR_RT_ParseOptions::Command_SetFlag( val, optName, optDesc ); m_commands.push_back( cmd )
#define OPTION_CALL(fpn,optName,optDesc) cmd = new Command_Call( [this](CLR_RT_ParseOptions::ParameterList* params){ return this->fpn(params); }, optName, optDesc ); m_commands.push_back( cmd )

#define OPTION_STRING(val,optName,optDesc,parm1Name,parm1Desc)  OPTION_GENERIC(optName,optDesc); PARAM_STRING(val,parm1Name,parm1Desc)
#define OPTION_BOOLEAN(val,optName,optDesc,parm1Name,parm1Desc) OPTION_GENERIC(optName,optDesc); PARAM_BOOLEAN(val,parm1Name,parm1Desc)
#define OPTION_INTEGER(val,optName,optDesc,parm1Name,parm1Desc) OPTION_GENERIC(optName,optDesc); PARAM_INTEGER(val,parm1Name,parm1Desc)
#define OPTION_FLOAT(val,optName,optDesc,parm1Name,parm1Desc)   OPTION_GENERIC(optName,optDesc); PARAM_FLOAT(val,parm1Name,parm1Desc)

HRESULT Win32Settings::Cmd_Cfg( CLR_RT_ParseOptions::ParameterList* params )
{
    return ExtractOptionsFromFile( PARAM_EXTRACT_STRING( params, 0 ) );
}

HRESULT Win32Settings::Cmd_Load( CLR_RT_ParseOptions::ParameterList* params )
{
    AssemblyPaths.push_back( PARAM_EXTRACT_STD_STRING( params, 0 ) );
    return S_OK;
}

HRESULT Win32Settings::Cmd_LoadDatabase( CLR_RT_ParseOptions::ParameterList* params )
{
    DatPaths.push_back( PARAM_EXTRACT_STD_STRING( params, 0 ) );
    return S_OK;
}

HRESULT Win32Settings::Cmd_Resolve( CLR_RT_ParseOptions::ParameterList* params )
{
    return S_OK;
}

HRESULT Win32Settings::Cmd_Execute( CLR_RT_ParseOptions::ParameterList* params )
{
    return S_OK;
}

HRESULT Win32Settings::Cmd_CommandLineArgs( CLR_RT_ParseOptions::ParameterList* params )
{
    m_szCommandLineArgs = PARAM_EXTRACT_STRING( params, 0 );
    return S_OK;
}

HRESULT Win32Settings::Cmd_ListNICs( CLR_RT_ParseOptions::ParameterList* params )
{
    printf("Avalable NICs:\n");
    WinPCAP::DeviceList deviceList;
    for( auto netif : deviceList )
    {
        printf("  %s\n", netif->description == nullptr ? "" : netif->description );
        printf("    %s\n", netif->name );
    }
    return S_OK;
}

HRESULT Win32Settings::Set_NicGuid( CLR_RT_ParseOptions::ParameterList* params )
{
    auto guidStr= PARAM_EXTRACT_STD_STRING( params, 0 );

    // verify the format of the GUID is legit
    UUID id;
    auto hr = ::IIDFromString( guidStr.c_str(), &id);
    if( FAILED( hr ) )
        return hr;

    NicGuid = std::move( guidStr );
    
    cdbg << "Parsed option NicGuid: " << NicGuid << std::endl;

    PostParseProcessCommands.push_back( [this]()
    {
        std::wstring_convert< std::codecvt_utf8_utf16<wchar_t> > converter;

    
        HAL_CONFIG_BLOCK::ApplyConfig( WINPCAP_ETH_LWIP_DEVICE_CONFIG::GetDriverName()
                                     , &g_WINPCAP_ETH_LWIP_Config
                                     , sizeof(g_WINPCAP_ETH_LWIP_Config)
                                     );

        strcpy_s( g_WINPCAP_ETH_LWIP_Config.DeviceConfigs[0].adapterGuid
                , converter.to_bytes( NicGuid ).c_str()
                );
    
        
        cdbg << "Updating config block for LWIP ethernet with NicGuid: " << NicGuid << std::endl;

        auto stat = HAL_CONFIG_BLOCK::UpdateBlockWithName( WINPCAP_ETH_LWIP_DEVICE_CONFIG::GetDriverName()
                                                         , &g_WINPCAP_ETH_LWIP_Config
                                                         , sizeof(g_WINPCAP_ETH_LWIP_Config)
                                                         , FALSE
                                                         );
        return !!stat;
    });

    return S_OK;
}

// xx:xx:xx:xx:xx:xx
const wchar_t MacRegExColonDelimPattern[] = LR"(([[:xdigit:]]{2})\:([[:xdigit:]]{2})\:([[:xdigit:]]{2})\:([[:xdigit:]]{2})\:([[:xdigit:]]{2})\:([[:xdigit:]]{2}))";

// xx-xx-xx-xx-xx-xx
const wchar_t MacRegExDashDelimPattern[] = LR"(([[:xdigit:]]{2})-([[:xdigit:]]{2})-([[:xdigit:]]{2})-([[:xdigit:]]{2})-([[:xdigit:]]{2})-([[:xdigit:]]{2}))";

// xxxxxxxxxxxx
const wchar_t MacRegExNoDelimPattern[] = LR"(([[:xdigit:]]{2})([[:xdigit:]]{2})([[:xdigit:]]{2})([[:xdigit:]]{2})([[:xdigit:]]{2})([[:xdigit:]]{2}))";

static std::list< std::wregex> MacRegExMatchers
    = { std::wregex( MacRegExColonDelimPattern )
      , std::wregex( MacRegExDashDelimPattern )
      , std::wregex( MacRegExNoDelimPattern )
      };

extern NETWORK_CONFIG g_NetworkConfig;

HRESULT Win32Settings::Set_NicMAC( CLR_RT_ParseOptions::ParameterList* params )
{
    MacAddress = PARAM_EXTRACT_STD_STRING( params, 0 );

    // verify the format of the MAC is legit
    HRESULT retVal = E_INVALIDARG;
    
    cdbg << "Parsing mac option: " << MacAddress << std::endl;

    // walk through each expression format supported to validate at least one
    // of them is supported.
    for( auto&& expression : MacRegExMatchers )
    {
        if(std::regex_match( MacAddress, MacAddressParseResults, expression ) )
        {
            retVal = S_OK;
            break;
        }
    }
    if( FAILED( retVal ) )
    {
        cdbg << "Failed to parse mac option!" << std::endl;
        return retVal;
    }

    PostParseProcessCommands.push_back( [this]()
    {   
        HAL_CONFIG_BLOCK::ApplyConfig( g_NetworkConfig.GetDriverName(), &g_NetworkConfig, sizeof(g_NetworkConfig) );
        auto& itf = g_NetworkConfig.NetworkInterfaces[ 0 ];
        auto macBuffer = &itf.macAddressBuffer[ 0 ];
    
        // convert captured hex digit groups into integral values
        for(int i = 1; i <= 6; ++i )
            macBuffer[i - 1] = std::stoi( MacAddressParseResults[ i ].str(), nullptr, 16 );
        
        itf.macAddressLen = 6;
    
        cdbg << "Updating mac config data: ";
        cdbg << std::setfill(L'0') << std::uppercase;
        cdbg << std::setw( 2 ) << std::hex << (uint8_t)( macBuffer[ 0 ] ) << ":" ;
        cdbg << std::setw( 2 ) << std::hex << (uint8_t)( macBuffer[ 1 ] ) << ":" ;
        cdbg << std::setw( 2 ) << std::hex << (uint8_t)( macBuffer[ 2 ] ) << ":" ;
        cdbg << std::setw( 2 ) << std::hex << (uint8_t)( macBuffer[ 3 ] ) << ":" ;
        cdbg << std::setw( 2 ) << std::hex << (uint8_t)( macBuffer[ 4 ] ) << ":" ;
        cdbg << std::setw( 2 ) << std::hex << (uint8_t)( macBuffer[ 5 ] );
        cdbg << std::setw( 2 ) << std::endl;

        return !!HAL_CONFIG_BLOCK::UpdateBlockWithName(g_NetworkConfig.GetDriverName(), &g_NetworkConfig, sizeof(g_NetworkConfig), TRUE);
    });

    return S_OK;
}

HRESULT Win32Settings::Cmd_EraseFlash( CLR_RT_ParseOptions::ParameterList* params )
{
    EraseFlash = true;
    PostParseProcessCommands.push_back( []()
    {
        cdbg << "erasing flash data" << std::endl;

        memset( FlashMemoryFile.GetDataPointer(), 0xFF, FlashMemoryFile.Length() );
        BlockStorageStream stream;
        if( !stream.Initialize( BlockUsage::CONFIG ) )
            return false;
        
        return !!stream.Write( (UINT8*)&g_ConfigurationSector, sizeof( g_ConfigurationSector ) );
    });
    return S_OK;
}

void Win32Settings::Usage( )
{
    std::cout << "Available command line switches:\n\n";

    CLR_RT_ParseOptions::Usage( );
}

//----------------------------------------------------//
/*  Loads the possible command line switches into the
    CLR_RT_ParseOptions.m_commands array....
*/
void Win32Settings::BuildOptions( )
{
    CLR_RT_ParseOptions::Command*   cmd;
    CLR_RT_ParseOptions::Parameter* param;

    OPTION_SET( &m_fVerbose, L"-verbose", L"Outputs each command before executing it" );
    OPTION_SET( &m_fNoExecuteIL, L"-noexecute", L"Do not Execute IL after processing commands (default will execute IL)");
    OPTION_SET( &m_fWaitOnExit, L"-waitonexit", L"Wait for keypress on exit (useful during debugging of this app)");
    OPTION_SET( &m_fNoNetwork, L"-nonetwork", L"Don't initialize the WINPCAP network adapter interface");

    OPTION_CALL( Cmd_Load, L"-load", L"Loads an assembly" );
    PARAM_GENERIC( L"<file>", L"File to load" );

    OPTION_CALL( Cmd_LoadDatabase, L"-loadDatabase", L"Loads a set of assemblies" );
    PARAM_GENERIC( L"<file>", L"Image to load" );

    OPTION_CALL( Cmd_ListNICs, L"-listnics", L"Lists the available NIC adapters for use by this application" );

    OPTION_CALL( Set_NicGuid, L"-nicid", L"Provide the GUID of the network adapter to use. (see -listnics to find the id for an available NIC)");
    PARAM_GENERIC(L"<guid>", L"GUID of the interface to use for networking");

    OPTION_CALL( Set_NicMAC, L"-mac", L"Provide the MAC address for the network adapter");
    PARAM_GENERIC(L"<xx:xx:xx:xx:xx:xx>|<xx-xx-xx-xx-xx-xx>", L"MAC address for the interface to use for networking");

    OPTION_CALL(Cmd_EraseFlash, L"-erase", L"Erases entire flash");

    OPTION_CALL( Cmd_Cfg, L"-cfg", L"Loads configuration from a file" );
    PARAM_GENERIC( L"<file>", L"Config file to load. Syntax is that of the command line options with one option per line.");
}

HRESULT Win32Settings::ParseCmdLineOptions( int argc, wchar_t const* argv[ ] )
{
    CLR_RT_StringVector vec;

    for( int i = 1; i < argc; ++i )
    {
        auto arg = std::wstring( argv[ i ] );
        if( arg[ 0 ] == L'"' )
            arg = arg.substr( 1, arg.length( ) - 2 );

        vec.push_back( argv[ i ] );
    }

    auto retVal = ProcessOptions( vec );
    // Always add standard handler for loading assemblies and DAT files
    PostParseProcessCommands.push_back( [this]()
    {
        return !!SUCCEEDED( LoadAssemblies() );
    });

    return retVal;
}

HRESULT Win32Settings::LoadDataBase(std::wstring path)
{
    CLR_RT_Buffer        buffer;
    CLR_UINT8*           pDatBuffer = ( CLR_UINT8* )FlashMemoryFile.GetDataPointer( );

    auto hr = CLR_RT_FileStore::LoadFile( path.c_str(), buffer );
    if( FAILED( hr ) )
        return hr;

    auto header = ( CLR_RECORD_ASSEMBLY* )&buffer[ 0 ];
    auto headerEnd = ( CLR_RECORD_ASSEMBLY* )&buffer[ buffer.size( ) - 1 ];

    if( FlashMemoryFile.Length( ) < ( m_idxDatBufferIndex + buffer.size( ) ) )
        return CLR_E_OUT_OF_MEMORY;

    while( header < headerEnd && header->GoodAssembly( ) )
    {
        memcpy( &pDatBuffer[ m_idxDatBufferIndex ], header, header->TotalSize( ) );

        m_idxDatBufferIndex += header->TotalSize( );

        m_idxDatBufferIndex = ROUNDTOMULTIPLE( m_idxDatBufferIndex, CLR_UINT32 );

        header = ( CLR_RECORD_ASSEMBLY* )ROUNDTOMULTIPLE( ( size_t )header + header->TotalSize( ), CLR_UINT32 );
    }

    return S_OK;
}

HRESULT Win32Settings::LoadAssembly( std::wstring path )
{
    CLR_RT_Buffer* buffer = new CLR_RT_Buffer( );
    CLR_UINT8* pDatBuffer = ( CLR_UINT8* )FlashMemoryFile.GetDataPointer( );
    m_assemblies[ path.c_str() ] = buffer;
    std::wcout << L"Loading PE File: " << path << std::endl;

    HRESULT hr = CLR_RT_FileStore::LoadFile( path.c_str(), *buffer );
    if( FAILED( hr ) )
    {
        std::wcerr << L"Error loading PE File (0x" << std::hex << hr << L" ): " << path << std::endl;
        return hr;
    }

    auto header = ( CLR_RECORD_ASSEMBLY* )&( *buffer )[ 0 ];
    hr = CheckAssemblyFormat( header, path.c_str() );
    if( FAILED( hr ) )
        return hr;

    if( FlashMemoryFile.Length( ) < ( m_idxDatBufferIndex + buffer->size( ) ) )
        return CLR_E_OUT_OF_MEMORY;

    memcpy( &pDatBuffer[ m_idxDatBufferIndex ], &( *buffer )[ 0 ], buffer->size( ) );

    m_idxDatBufferIndex += ( int )buffer->size( );
    m_idxDatBufferIndex = ROUNDTOMULTIPLE( m_idxDatBufferIndex, CLR_UINT32 );

    return S_OK;
}

HRESULT Win32Settings::LoadAssemblies()
{
    for( auto&& path : AssemblyPaths )
    {
        auto hr = LoadAssembly( path );
        if( FAILED( hr ) )
            return hr;
    }

    for( auto&& path : DatPaths )
    {
        auto hr = LoadDataBase( path );
        if( FAILED( hr ) )
            return hr;
    }
    return S_OK;
}

HRESULT Win32Settings::ProcessCommandLineSettings()
{
    for( auto&& func : PostParseProcessCommands )
        func();

    return S_OK;
}

