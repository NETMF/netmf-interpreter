////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

#include <time.h>

#pragma comment(lib, "Comdlg32")
#include <Commdlg.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

struct Settings : CLR_RT_ParseOptions
{
    CLR_RT_SymbolToAddressMap m_symdef;
    CLR_RT_AddressToSymbolMap m_symdef_Inverse;
    LPCWSTR                   m_szEntryPoint;


    //--//

    struct Command_Call : CLR_RT_ParseOptions::Command
    {
        typedef HRESULT (Settings::*FPN)( CLR_RT_ParseOptions::ParameterList* params );

        Settings& m_parent;
        FPN       m_call;

        Command_Call( Settings& parent, FPN call, LPCWSTR szName, LPCWSTR szDescription ) : CLR_RT_ParseOptions::Command( szName, szDescription ), m_parent(parent), m_call(call)
        {
        }

        virtual HRESULT Execute()
        {
            return (m_parent.*m_call)( &m_params );
        }
    };

    //--//

    Settings()
    {
        BuildOptions();
    }

    //--//

#define PARAM_GENERIC(parm1Name,parm1Desc)     param = new CLR_RT_ParseOptions::Parameter_Generic(      parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_STRING(val,parm1Name,parm1Desc)  param = new CLR_RT_ParseOptions::Parameter_String ( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_BOOLEAN(val,parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Boolean( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_INTEGER(val,parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Integer( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_FLOAT(val,parm1Name,parm1Desc)   param = new CLR_RT_ParseOptions::Parameter_Float  ( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )

#define PARAM_EXTRACT_STRING(lst,idx)    ((CLR_RT_ParseOptions::Parameter_Generic*)(*lst)[ idx ])->m_data.c_str()
#define PARAM_EXTRACT_BOOLEAN(lst,idx) *(((CLR_RT_ParseOptions::Parameter_Boolean*)(*lst)[ idx ])->m_dataPtr)
#define PARAM_EXTRACT_INTEGER(lst,idx) *(((CLR_RT_ParseOptions::Parameter_Integer*)(*lst)[ idx ])->m_dataPtr)


#define OPTION_GENERIC(optName,optDesc)  cmd = new CLR_RT_ParseOptions::Command        (      optName, optDesc ); m_commands.push_back( cmd )
#define OPTION_SET(val,optName,optDesc)  cmd = new CLR_RT_ParseOptions::Command_SetFlag( val, optName, optDesc ); m_commands.push_back( cmd )
#define OPTION_CALL(fpn,optName,optDesc) cmd = new Command_Call( *this, &Settings::fpn, optName, optDesc )      ; m_commands.push_back( cmd )

#define OPTION_STRING(val,optName,optDesc,parm1Name,parm1Desc)  OPTION_GENERIC(optName,optDesc); PARAM_STRING(val,parm1Name,parm1Desc)
#define OPTION_BOOLEAN(val,optName,optDesc,parm1Name,parm1Desc) OPTION_GENERIC(optName,optDesc); PARAM_BOOLEAN(val,parm1Name,parm1Desc)
#define OPTION_INTEGER(val,optName,optDesc,parm1Name,parm1Desc) OPTION_GENERIC(optName,optDesc); PARAM_INTEGER(val,parm1Name,parm1Desc)
#define OPTION_FLOAT(val,optName,optDesc,parm1Name,parm1Desc)   OPTION_GENERIC(optName,optDesc); PARAM_FLOAT(val,parm1Name,parm1Desc)

    //--//

    static bool StringEndsWithSuffix( LPCWSTR str1, LPCWSTR str2 )
    {
        size_t len1 = wcslen( str1 );
        size_t len2 = wcslen( str2 );

        return (len1 >= len2 && _wcsicmp( &str1[ len1-len2 ], str2 ) == 0);
    }

    HRESULT Cmd_ConvertFont( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR szFileIn  = PARAM_EXTRACT_STRING(params, 0);
        LPCWSTR szFileOut = PARAM_EXTRACT_STRING(params, 1);

        CLR_GFX_FontBuilder         fb;
        CLR_GFX_Font*               font = NULL;
        CLR_RT_Buffer               buf;
        CLR_RT_Buffer               bufT;
        CLR_UINT8*                  ptr;
        CLR_UINT32                  size;
        TinyResourcesFileHeader     fileHeader;
        TinyResourcesResourceHeader resource;

        TINYCLR_CHECK_HRESULT(fb.Initialize());

        buf.clear();

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szFileIn, buf ));

#if FNT_SUPPORT
        if(StringEndsWithSuffix( szFileIn, L".fntdef" ))
        {
            TINYCLR_CHECK_HRESULT(fb.LoadFromScript( buf ));
        }
        else
        {
            TINYCLR_CHECK_HRESULT(fb.LoadFromFNT( buf, 0, 255, 0 ));
        }
#else
        TINYCLR_CHECK_HRESULT(fb.LoadFromScript( buf ));
#endif

        TINYCLR_CHECK_HRESULT(fb.GenerateFont( font ));

        TINYCLR_CHECK_HRESULT(font->SaveToBuffer( bufT ));

        /*.tinyresources format */

        fileHeader.magicNumber          = TinyResourcesFileHeader::MAGIC_NUMBER;
        fileHeader.version              = CLR_RECORD_RESOURCE_FILE::CURRENT_VERSION;
        fileHeader.sizeOfHeader         = sizeof(fileHeader);
        fileHeader.sizeOfResourceHeader = sizeof(resource);
        fileHeader.numberOfResources    = 1;

        resource.id = 0;
        resource.kind = CLR_RECORD_RESOURCE::RESOURCE_Font;
        resource.pad = 0;
        resource.size = (CLR_UINT32)bufT.size();

        buf.clear();

        size = fileHeader.sizeOfHeader;
        size += fileHeader.numberOfResources*fileHeader.sizeOfResourceHeader;
        size += (CLR_UINT32)bufT.size();

        buf.resize(size);
        ptr = &buf[ 0 ];
        //write file header
        memcpy(ptr, &fileHeader, fileHeader.sizeOfHeader        ); ptr += fileHeader.sizeOfHeader;
        //write resource header
        memcpy(ptr, &resource  , fileHeader.sizeOfResourceHeader); ptr += fileHeader.sizeOfResourceHeader;
        //write resource data
        memcpy(ptr, &bufT[0]   , bufT.size()                    ); ptr += bufT.size();

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( szFileOut, buf ));

        TINYCLR_CLEANUP();

        if(font)
        {
            free(font);
        }

        TINYCLR_CLEANUP_END();
    }
    //--//

    void Usage()
    {
        wprintf( L"TFConvert - .TTF to .TinyFNT conversion tool\n\n");
        wprintf( L"Converts a TrueType font into .tinyfnt file for the .NET Micro Framework.\n\n" );
        wprintf( L"Syntax:\n\n");

        CLR_RT_ParseOptions::Usage();
    }

    void BuildOptions()
    {
        CLR_RT_ParseOptions::Command*   cmd;
        CLR_RT_ParseOptions::Parameter* param;

        //--//
        OPTION_CALL( Cmd_ConvertFont, L"-convertFont", L"TFConvert <input file> <output file>" );
        PARAM_GENERIC(       L"<input file>" , L"Font definition file (.fntdef)"               );
        PARAM_GENERIC(       L"<output file>", L"Font output file (.tinyfnt)"                  );
    }
};

//--//

int _tmain(int argc, _TCHAR* argv[])
{
    TINYCLR_HEADER();

    CLR_RT_Assembly::InitString();

    CLR_RT_StringVector vec;
    Settings            st;
    LPWSTR              convertFont[] = { L"-convertFont" };

    TINYCLR_CHECK_HRESULT(::CoInitialize( 0 ));

    TINYCLR_CHECK_HRESULT(HAL_Windows::Memory_Resize( 16 * 1024 * 1024 ));
    HAL_Init_Custom_Heap();

    CLR_RT_Memory::Reset         ();    

    st.PushArguments( 1, convertFont, vec );
    st.PushArguments( argc-1, argv+1, vec );

    TINYCLR_CHECK_HRESULT(st.ProcessOptions( vec ));

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        fflush( stdout );
    }

    ::CoUninitialize();

    return FAILED(hr) ? 10 : 0;
}

