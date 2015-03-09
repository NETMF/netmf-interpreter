////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

#include <time.h>

#pragma comment(lib, "Comdlg32")
#include <Commdlg.h>

_COM_SMRT_PTR(IXMLDOMNodeList);

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

    struct OptimizeLayout
    {
        struct Rule
        {
            std::wstring m_function;
            std::wstring m_file;
            std::wstring m_target;
        };

        typedef std::map< CLR_UINT32, int >         AddressToSizeMap;
        typedef AddressToSizeMap::iterator          AddressToSizeMapIter;

        typedef std::map< CLR_UINT32, CLR_INT64 >   AddressToFrequencyMap;
        typedef AddressToFrequencyMap::iterator     AddressToFrequencyMapIter;

        typedef std::multimap< double, CLR_UINT32 > SortByUsage;
        typedef SortByUsage::iterator               SortByUsageIter;

        typedef std::map< std::wstring, Rule >      RuleMap;
        typedef RuleMap::iterator                   RuleMapIter;

        //--//

        LPCWSTR                   m_szSymbols;
        LPCWSTR                   m_szProfileData;
        LPCWSTR                   m_szRules;
        LPCWSTR                   m_szCVS;
        LPCWSTR                   m_szScatterFile;
        int                       m_limitRAM;
        bool                      m_fOptimize;

        double                    m_cost_RAM;
        double                    m_cost_FLASH;

        CLR_RT_SymbolToAddressMap m_symdef;
        CLR_RT_AddressToSymbolMap m_symdef_Inverse;
        AddressToSizeMap          m_symdef_Size;
        AddressToFrequencyMap     m_symdef_Hits;

        SortByUsage               m_sort;
        RuleMap                   m_rules;

        CLR_INT64                 m_totHits;
        CLR_UINT32                m_entryPoint;
        CLR_UINT32                m_ramCodeEnd;

        CLR_RT_StringSet          m_seen;

        //--//

        HRESULT Execute()
        {
            TINYCLR_HEADER();

            m_totHits       = 0;
            m_entryPoint    = 0xFFFFFFFF;
            m_ramCodeEnd    = 0xFFFFFFFF;
            m_fOptimize     = m_limitRAM >= 0;

            m_cost_RAM      = 1.93       ;
            m_cost_FLASH    = 1.93 + 2.25;

            //--//

            if(!m_szSymbols || m_szSymbols[0] == 0)
            {
                wprintf( L"No symbols file!\n" );
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            //--//

            TINYCLR_CHECK_HRESULT(ParseRules      ());
            TINYCLR_CHECK_HRESULT(ParseSymDef     ());
            TINYCLR_CHECK_HRESULT(ParseProfileData());

            if(m_limitRAM <= 0)
            {
                for(CLR_RT_AddressToSymbolMapIter it = m_symdef_Inverse.begin(); it != m_symdef_Inverse.end(); it++)
                {
                    CLR_UINT32 address = it->first;

                    if(address >= m_entryPoint) break;

                    m_limitRAM = (int)address;
                }
            }

            //--//

            TINYCLR_CHECK_HRESULT(GenerateProfileSummary());
            TINYCLR_CHECK_HRESULT(GenerateScatterFile   ());

            //--//

            TINYCLR_NOCLEANUP();
        }

    private:
        HRESULT ParseSymDef()
        {
            TINYCLR_HEADER();

            TINYCLR_CHECK_HRESULT(Settings::ParseSymDef( m_szSymbols, m_symdef, m_symdef_Inverse, &m_symdef_Size, true, false ));

            TINYCLR_NOCLEANUP();
        }

        HRESULT ParseProfileData()
        {
            TINYCLR_HEADER();

            CLR_RT_StringVector vec;
            bool                fHeader = true;

            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::ExtractTokensFromFile( m_szProfileData, vec, NULL, true ));

            size_t argc = vec.size();

            for(size_t i=0; i<argc; i++)
            {
                LPCWSTR    line = vec[i].c_str();
                CLR_UINT32 address;
                CLR_UINT32 hits;

                if(fHeader)
                {
                    if(swscanf_s( line, L"Entry Point  = %08x", &m_entryPoint ) == 1) continue;
                    if(swscanf_s( line, L"RAM code end = %08x", &m_ramCodeEnd ) == 1) continue;

                    if(m_entryPoint == 0xFFFFFFFF)
                    {
                        wprintf( L"Missing EntryPoint!\n" );
                        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
                    }

                    if(m_ramCodeEnd == 0xFFFFFFFF)
                    {
                        wprintf( L"Missing RamCodeEnd!\n" );
                        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
                    }

                    fHeader = false;
                }

                if(swscanf_s( line, L"%08x %08x", &address, &hits ) == 2)
                {
                    CLR_UINT32 context;

                    if(address >= m_ramCodeEnd) address += m_entryPoint - m_ramCodeEnd;

                    if(GetSymDefContext( m_symdef_Inverse, address, context ))
                    {
                        m_symdef_Hits[context] += hits;
                    }

                    m_totHits += hits;

                    continue;
                }

                wprintf( L"Invalid line format:\n'%s'\n", line );
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            TINYCLR_NOCLEANUP();
        }

        HRESULT ParseRules()
        {
            TINYCLR_HEADER();

            if(m_szRules[0])
            {
                CLR_XmlUtil        xml;
                IXMLDOMNodeListPtr xdnlList;
                IXMLDOMNodePtr     xdnChild;
                std::wstring       szValue;
                bool               fLoaded;
                bool               fFound;

                TINYCLR_CHECK_HRESULT(xml.Load( m_szRules, L"ScatterFileRules", fLoaded, &fFound ));
                if(fFound == false)
                {
                    wprintf( L"Cannot find rules configuration: %s\n", m_szRules );

                    TINYCLR_SET_AND_LEAVE(CLR_E_ENTRY_NOT_FOUND);
                }

                //--//

                TINYCLR_CHECK_HRESULT(xml.GetAttribute( NULL, L"RamCost", szValue, fFound ));
                if(fFound) m_cost_RAM = _wtof( szValue.c_str() );

                TINYCLR_CHECK_HRESULT(xml.GetAttribute( NULL, L"FlashCost", szValue, fFound ));
                if(fFound) m_cost_FLASH = _wtof( szValue.c_str() );

                //--//

                TINYCLR_CHECK_HRESULT(xml.GetNodes( L"Map", &xdnlList ));
                for(;SUCCEEDED(xdnlList->nextNode( &xdnChild )) && xdnChild != NULL; xdnChild = NULL)
                {
                    Rule rule;

                    TINYCLR_CHECK_HRESULT(xml.GetAttribute( NULL, L"Function", rule.m_function, fFound, xdnChild ));
                    if(!fFound)
                    {
                        wprintf( L"Missing function name...\n" );

                        TINYCLR_SET_AND_LEAVE(CLR_E_ENTRY_NOT_FOUND);
                    }

                    TINYCLR_CHECK_HRESULT(xml.GetAttribute( NULL, L"File"  , rule.m_file  , fFound, xdnChild ));
                    TINYCLR_CHECK_HRESULT(xml.GetAttribute( NULL, L"Target", rule.m_target, fFound, xdnChild ));

                    m_rules[ rule.m_function ] = rule;
                }
            }

            TINYCLR_NOCLEANUP();
        }

        HRESULT GenerateProfileSummary()
        {
            TINYCLR_HEADER();

            FILE* file = NULL;

            if(m_szCVS && m_szCVS[0])
            {                
                if(_wfopen_s(&file,m_szCVS, L"w" ) != 0)
                {
                    wprintf( L"Cannot create file '%s'\n", m_szCVS );
                    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
                }

                for(CLR_RT_AddressToSymbolMapIter it = m_symdef_Inverse.begin(); it != m_symdef_Inverse.end(); it++)
                {
                    CLR_UINT32 address = it->first;
                    CLR_UINT32 size    = m_symdef_Size[address];
                    CLR_INT64  hits    = m_symdef_Hits[address];

                    if(m_fOptimize == false || (size && hits != 0))
                    {
                        double ratio;

                        fwprintf( file, L"%08x\t%d\t%I64d\t%s\n", address, size, hits, it->second.c_str() );

                        if(m_fOptimize)
                        {
                            ratio = -(double)hits / (double)size;

                            if(address > m_entryPoint) ratio /= m_cost_FLASH;
                            else                       ratio /= m_cost_RAM;
                        }
                        else
                        {
                            if(address > m_entryPoint) break;

                            ratio = address;
                        }

                        m_sort.insert( SortByUsage::value_type( ratio, address ) );
                    }
                }
            }

            TINYCLR_CLEANUP();

            if(file)
            {
                fclose( file );
            }

            TINYCLR_CLEANUP_END();
        }

        HRESULT GenerateScatterFile()
        {
            TINYCLR_HEADER();

            if(m_szScatterFile && m_szScatterFile[0])
            {
                CLR_XmlUtil    xml;
                IXMLDOMNodePtr pNodeEntry;

                TINYCLR_CHECK_HRESULT(xml.New( L"ScatterFile" ));

                int             tot  = 0;
                RuleMapIter     itRule;
                SortByUsageIter itSort;

                for(itRule = m_rules.begin(); itRule != m_rules.end(); itRule++)
                {
                    Rule&   rule     = itRule->second;
                    LPCWSTR szTarget = rule.m_target.c_str();

                    if(_wcsicmp( szTarget, L"RAM" ) == 0)
                    {
                        TINYCLR_CHECK_HRESULT(EmitRule( xml, rule ));

                        CLR_RT_SymbolToAddressMapIter itSym = m_symdef.find( rule.m_function );
                        if(itSym != m_symdef.end())
                        {
                            tot += m_symdef_Size[ itSym->second ];
                        }
                    }
                }

                for(itSort = m_sort.begin(); itSort != m_sort.end(); itSort++)
                {
                    double        ratio   = itSort->first;
                    CLR_UINT32    address = itSort->second;
                    CLR_UINT32    size    = m_symdef_Size   [address];
                    CLR_INT64     hits    = m_symdef_Hits   [address];
                    std::wstring& func    = m_symdef_Inverse[address];

                    if(m_fOptimize)
                    {
                        if(tot >= m_limitRAM) continue;
                    }
                    else
                    {
                        if(address >= m_entryPoint) continue;
                    }

                    itRule = m_rules.find( func );
                    if(itRule != m_rules.end())
                    {
                        Rule&   rule     = itRule->second;
                        LPCWSTR szTarget = rule.m_target.c_str();

                        if(_wcsicmp( szTarget, L"FLASH" ) == 0) continue;

                        TINYCLR_CHECK_HRESULT(EmitRule( xml, rule ));
                    }
                    else
                    {
                        TINYCLR_CHECK_HRESULT(EmitFunction( xml, func ));
                    }

                    tot += size;
                }

                TINYCLR_CHECK_HRESULT(xml.Save( m_szScatterFile ));
            }

            TINYCLR_NOCLEANUP();
        }

        //--//

        HRESULT EmitRule( CLR_XmlUtil& xml, Rule& rule )
        {
            TINYCLR_HEADER();

            if(rule.m_file.size() > 0)
            {
                TINYCLR_CHECK_HRESULT(EmitNode( xml, rule.m_file, L"(+RO)", L"" ));
            }
            else
            {
                TINYCLR_CHECK_HRESULT(EmitFunction( xml, rule.m_function ));
            }

            TINYCLR_NOCLEANUP();
        }

        HRESULT EmitFunction( CLR_XmlUtil& xml, const std::wstring& function )
        {
            TINYCLR_HEADER();

            TINYCLR_CHECK_HRESULT(EmitNode( xml, L"*", L"(i." + function + L")", function ));

            TINYCLR_NOCLEANUP();
        }

        HRESULT EmitNode( CLR_XmlUtil& xml, const std::wstring& name, const std::wstring& options, const std::wstring& function )
        {
            TINYCLR_HEADER();

            std::wstring full = name + L"/" + options;

            if(m_seen.find( full ) == m_seen.end())
            {
                CLR_RT_SymbolToAddressMapIter it;
                IXMLDOMNodePtr                pNodeEntry;
                bool                          fFound;

                m_seen.insert( full );

                TINYCLR_CHECK_HRESULT(xml.CreateNode( L"FileMapping", &pNodeEntry ));

                TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Name"   , name   , fFound, pNodeEntry ));
                TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Options", options, fFound, pNodeEntry ));

                it = m_symdef.find( function );
                if(it != m_symdef.end())
                {
                    WCHAR      buf[1024];
                    CLR_UINT32 address = it->second;
                    CLR_UINT32 size    = m_symdef_Size[address];
                    CLR_INT64  hits    = m_symdef_Hits[address];

                    _snwprintf_s( buf, ARRAYSIZE(buf), MAXSTRLEN(buf), L"%08x %d %I64d %s", address, size, hits, function.c_str() );

                    TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Comment", buf, fFound, pNodeEntry ));
                }
            }

            TINYCLR_NOCLEANUP();
        }
    };

    //--//

    Settings()
    {
        RevertToDefaults();

        BuildOptions();
    }

    ~Settings()
    {
        RevertToDefaults();
    }

    //--//

    void RevertToDefaults()
    {
    }

    //--//

    static HRESULT ParseSymDef( LPCWSTR szFileName, CLR_RT_SymbolToAddressMap& symdef, CLR_RT_AddressToSymbolMap& symdef_Inverse, OptimizeLayout::AddressToSizeMap* symdef_Size = NULL, bool fCode = true, bool fData = true )
    {
        TINYCLR_HEADER();

        CLR_RT_StringVector vec;
        CLR_UINT32          lastAddress = 0;

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::ExtractTokensFromFile( szFileName, vec, NULL, true ));

        symdef        .clear();
        symdef_Inverse.clear();

        size_t argc = vec.size();

        for(size_t i=0; i<argc; i++)
        {
            LPCWSTR    line = vec[i].c_str();
            CLR_UINT32 address;
            WCHAR      kind;
            WCHAR      name[1024];

            if(_snwscanf_s( line, wcslen(line), L"0x%08x %c %s", &address, &kind, 1,name, ARRAYSIZE(name) ) == 3)
            {
                if(lastAddress && symdef_Size)
                {
                    (*symdef_Size)[lastAddress] = address - lastAddress;

                    lastAddress = 0;
                }

                if(kind == 'D' && fData == false) continue;
                if(kind == 'A' && fCode == false) continue;

                symdef        [name   ] = address;
                symdef_Inverse[address] = name;

                lastAddress = address;
            }
        }

        TINYCLR_NOCLEANUP();
    }

    static bool GetSymDefContext( CLR_RT_AddressToSymbolMap& symdef_Inverse, CLR_UINT32 address, CLR_UINT32& context )
    {
        CLR_RT_AddressToSymbolMapIter it = symdef_Inverse.upper_bound( address );

        if(it == symdef_Inverse.end()) return false;

        if(it->first > address)
        {
            if(it == symdef_Inverse.begin()) return false;

            it--;
        }

        context = it->first;

        return true;
    }

    //--//

#define PARAM_GENERIC(parm1Name,parm1Desc)     param = new CLR_RT_ParseOptions::Parameter_Generic(      parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_STRING(val,parm1Name,parm1Desc)  param = new CLR_RT_ParseOptions::Parameter_String ( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_BOOLEAN(val,parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Boolean( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_INTEGER(val,parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Integer( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_FLOAT(val,parm1Name,parm1Desc)   param = new CLR_RT_ParseOptions::Parameter_Float  ( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )

#define PARAM_EXTRACT_STRING(lst,idx)    ((CLR_RT_ParseOptions::Parameter_Generic*)(*lst)[idx])->m_data.c_str()
#define PARAM_EXTRACT_BOOLEAN(lst,idx) *(((CLR_RT_ParseOptions::Parameter_Boolean*)(*lst)[idx])->m_dataPtr)
#define PARAM_EXTRACT_INTEGER(lst,idx) *(((CLR_RT_ParseOptions::Parameter_Integer*)(*lst)[idx])->m_dataPtr)


#define OPTION_GENERIC(optName,optDesc) cmd = new CLR_RT_ParseOptions::Command        (      optName, optDesc ); m_commands.push_back( cmd )
#define OPTION_SET(val,optName,optDesc) cmd = new CLR_RT_ParseOptions::Command_SetFlag( val, optName, optDesc ); m_commands.push_back( cmd )
#define OPTION_CALL(fpn,optName,optDesc) cmd = new Command_Call( *this, &Settings::fpn, optName, optDesc ); m_commands.push_back( cmd )

#define OPTION_STRING(val,optName,optDesc,parm1Name,parm1Desc)  OPTION_GENERIC(optName,optDesc); PARAM_STRING(val,parm1Name,parm1Desc)
#define OPTION_BOOLEAN(val,optName,optDesc,parm1Name,parm1Desc) OPTION_GENERIC(optName,optDesc); PARAM_BOOLEAN(val,parm1Name,parm1Desc)
#define OPTION_INTEGER(val,optName,optDesc,parm1Name,parm1Desc) OPTION_GENERIC(optName,optDesc); PARAM_INTEGER(val,parm1Name,parm1Desc)
#define OPTION_FLOAT(val,optName,optDesc,parm1Name,parm1Desc)   OPTION_GENERIC(optName,optDesc); PARAM_FLOAT(val,parm1Name,parm1Desc)

    //--//

    HRESULT Cmd_Cfg( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        TINYCLR_CHECK_HRESULT(ExtractOptionsFromFile( PARAM_EXTRACT_STRING( params, 0 ) ));

        TINYCLR_NOCLEANUP();
    }

    //--//

    HRESULT Cmd_SplitFlash( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR       szImage = PARAM_EXTRACT_STRING( params, 0 );
        LPCWSTR       szLow   = PARAM_EXTRACT_STRING( params, 1 );
        LPCWSTR       szHigh  = PARAM_EXTRACT_STRING( params, 2 );
        CLR_RT_Buffer buffer;
        CLR_RT_Buffer bufferLow;
        CLR_RT_Buffer bufferHigh;

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szImage, buffer ));

        size_t len = ROUNDTOMULTIPLE(buffer.size(),FLASH_WORD);

        len = ROUNDTOMULTIPLE(buffer.size(),FLASH_WORD);

        bufferLow .resize( len / 2 );
        bufferHigh.resize( len / 2 );

        for(size_t posIn=0, posOut=0; posIn<len; posIn+=4, posOut+=2)
        {
            bufferLow [posOut+0] = buffer[posIn+0];
            bufferLow [posOut+1] = buffer[posIn+1];
            bufferHigh[posOut+0] = buffer[posIn+2];
            bufferHigh[posOut+1] = buffer[posIn+3];
        }

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( szLow , bufferLow  ));
        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( szHigh, bufferHigh ));

        TINYCLR_NOCLEANUP();
    }

    //--//

    HRESULT Cmd_AddFontToProcess( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(::AddFontResourceExW( PARAM_EXTRACT_STRING( params, 0 ), FR_PRIVATE, NULL ) == 0) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_FontDialog( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR        szInit = PARAM_EXTRACT_STRING( params, 0 );
        ENUMLOGFONTEXW elf;
        LOGFONTW       lf;
        std::wstring   buffer;
        bool           fInit = false;

        if(szInit[0])
        {
            TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::DecodeDefinition( elf, szInit ));

            fInit = true;
        }

        TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::SelectFont      ( elf, fInit  ));
        TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::EncodeDefinition( elf, buffer ));

        lf = elf.elfLogFont;

        wprintf( L"Font definition:\n\n" );

        if(lf .lfFaceName[0]        ) wprintf( L"# FaceName       = %s\n",      lf .lfFaceName       );
        if(lf .lfHeight         != 0) wprintf( L"# Height         = %d\n", (int)lf .lfHeight         );
        if(lf .lfWidth          != 0) wprintf( L"# Width          = %d\n", (int)lf .lfWidth          );
        if(lf .lfEscapement     != 0) wprintf( L"# Escapement     = %d\n", (int)lf .lfEscapement     );
        if(lf .lfOrientation    != 0) wprintf( L"# Orientation    = %d\n", (int)lf .lfOrientation    );
        if(lf .lfWeight         != 0) wprintf( L"# Weight         = %d\n", (int)lf .lfWeight         );
        if(lf .lfItalic         != 0) wprintf( L"# Italic         = %d\n", (int)lf .lfItalic         );
        if(lf .lfUnderline      != 0) wprintf( L"# Underline      = %d\n", (int)lf .lfUnderline      );
        if(lf .lfStrikeOut      != 0) wprintf( L"# StrikeOut      = %d\n", (int)lf .lfStrikeOut      );
        if(lf .lfCharSet        != 0) wprintf( L"# CharSet        = %d\n", (int)lf .lfCharSet        );
        if(lf .lfOutPrecision   != 3) wprintf( L"# OutPrecision   = %d\n", (int)lf .lfOutPrecision   );
        if(lf .lfClipPrecision  != 2) wprintf( L"# ClipPrecision  = %d\n", (int)lf .lfClipPrecision  );
        if(lf .lfQuality        != 1) wprintf( L"# Quality        = %d\n", (int)lf .lfQuality        );
        if(lf .lfPitchAndFamily != 0) wprintf( L"# PitchAndFamily = %d\n", (int)lf .lfPitchAndFamily );
        if(elf.elfFullName[0]       ) wprintf( L"# FullName       = %s\n",      elf.elfFullName      );
        if(elf.elfScript[0]         ) wprintf( L"# Script         = %s\n",      elf.elfScript        );
        if(elf.elfStyle[0]          ) wprintf( L"# Style          = %s\n",      elf.elfStyle         );

        wprintf( L"\"%s\"\n\n", buffer.c_str() );

        TINYCLR_NOCLEANUP();
    }

    //--//

    HRESULT Cmd_ParseSymDef( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR szFileName       = PARAM_EXTRACT_STRING( params, 0 );
        m_szEntryPoint           = PARAM_EXTRACT_STRING( params, 1 );

        TINYCLR_CHECK_HRESULT(ParseSymDef( szFileName, m_symdef, m_symdef_Inverse ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_HashBuild( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR       szBuild  = PARAM_EXTRACT_STRING( params, 0 );
        LPCWSTR       szSymdef = PARAM_EXTRACT_STRING( params, 1 );
        CLR_RT_Buffer buffer;
        FILE*         stream = NULL;

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szBuild, buffer ));
        
        if(_wfopen_s(&stream, szSymdef, L"a+" ) == 0)
        {
            fprintf( stream, "0x%X D LOAD_IMAGE_CRC\r\n", SUPPORT_ComputeCRC( &buffer[0], (int)buffer.size(), 0 ) );

            fclose( stream );
        }
        else
        {
            wprintf( L"Cannot open '%s'!\n", szSymdef );
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        TINYCLR_NOCLEANUP();
    }

    //--//

    HRESULT Cmd_Compress( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR szInput  = PARAM_EXTRACT_STRING( params, 0 );
        LPCWSTR szOutput = PARAM_EXTRACT_STRING( params, 1 );
        UINT32  address;

        if(m_szEntryPoint == NULL)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        if(m_symdef.find( m_szEntryPoint ) == m_symdef.end())
        {
            if(swscanf_s( m_szEntryPoint, L"%x", &address ) == 0)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }
        else
        {
            address = m_symdef[m_szEntryPoint];
        }

        wprintf( L"Compressing image at address '0x%08x'!\n", address );

        if(LZ77_Compress( szInput, szOutput, (UINT8*)&address, sizeof(address) ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_UpdateFntdef( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR       szFile = PARAM_EXTRACT_STRING( params, 0 );
        CLR_RT_Buffer buffer;

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szFile, buffer ));

        TINYCLR_CHECK_HRESULT(CLR_GFX_FontBuilder::UpdateScript( buffer ))

        TINYCLR_NOCLEANUP();
    }

    //--//

    HRESULT Cmd_OptimizeLayout( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        OptimizeLayout ol;

        //--/

        ol.m_szSymbols     = PARAM_EXTRACT_STRING ( params, 0 );
        ol.m_szProfileData = PARAM_EXTRACT_STRING ( params, 1 );
        ol.m_szRules       = PARAM_EXTRACT_STRING ( params, 2 );
        ol.m_szCVS         = PARAM_EXTRACT_STRING ( params, 3 );
        ol.m_szScatterFile = PARAM_EXTRACT_STRING ( params, 4 );
        ol.m_limitRAM      = PARAM_EXTRACT_INTEGER( params, 5 );

        TINYCLR_CHECK_HRESULT(ol.Execute());

        TINYCLR_NOCLEANUP();
    }

    //--//

    //Ok, this is getting silly already. This function is in 3 separate libs already
    //where does common functionality belong?
    static bool StringEndsWithSuffix( LPCWSTR str1, LPCWSTR str2 )
    {
        size_t len1 = wcslen( str1 );
        size_t len2 = wcslen( str2 );

        return (len1 >= len2 && _wcsicmp( &str1[len1-len2], str2 ) == 0);
    }

    //--//

    void Usage()
    {
        wprintf( L"\nBuildHelper.exe\n"                  );
        wprintf( L"Available command line switches:\n\n" );

        CLR_RT_ParseOptions::Usage();
    }

    void BuildOptions()
    {
        CLR_RT_ParseOptions::Command*   cmd;
        CLR_RT_ParseOptions::Parameter* param;

        OPTION_SET( &m_fVerbose, L"-verbose", L"Outputs each command before executing it" );

        OPTION_CALL( Cmd_Cfg, L"-cfg", L"Loads configuration from a file" );
        PARAM_GENERIC( L"<file>", L"Config file to load"                  );

        //--//

        OPTION_CALL( Cmd_SplitFlash, L"-split_flash", L"Split ER_FLASH for pre-programming of image" );
        PARAM_GENERIC( L"<image>"   , L"Full image input file"                                       );
        PARAM_GENERIC( L"<lowword>" , L"Low Half image output file"                                  );
        PARAM_GENERIC( L"<highword>", L"High Half image output file"                                 );

        //--//

        OPTION_CALL( Cmd_AddFontToProcess, L"-add_font_to_process", L"Adds a font to the current process" );
        PARAM_GENERIC( L"<font>", L"Font to add to the process"                                           );

        OPTION_CALL( Cmd_FontDialog, L"-font_dialog", L"Displays the font dialog and shows the encoding to use in an .fntdef file" );
        PARAM_GENERIC( L"<definition>", L"Optional font definition"                                                                );

        //--//

        OPTION_CALL( Cmd_ParseSymDef, L"-symdef", L"Loads symdef output from ARM build" );
        PARAM_GENERIC( L"<file>",       L"SymDef file"                                  );
        PARAM_GENERIC( L"<entrypoint>", L"Entrypoint function"                          );

        //--//

        OPTION_CALL( Cmd_HashBuild, L"-hashBuild", L"Appends build hash to a symdef file" );
        PARAM_GENERIC( L"<.bin file>"    , L"Build to hash"                               );
        PARAM_GENERIC( L"<.symdefs file>", L"Symdef file to update"                       );

        //--//

        OPTION_CALL( Cmd_Compress, L"-compress", L"Prepares a compressed blob for loader" );
        PARAM_GENERIC( L"<input file>"  , L"Input file, with header"                      );
        PARAM_GENERIC( L"<output file>" , L"Output file"                                  );

        //--//

        OPTION_CALL( Cmd_UpdateFntdef, L"-update_fntdef", L"Updates a .fntdef file with more complete information" );
        PARAM_GENERIC( L"<file>", L".fntdef file to analyze"                                                       );

        //--//

        OPTION_CALL( Cmd_OptimizeLayout, L"-optimizeLayout", L"Generate scatter file to optimize RAM and Battery usage" );
        PARAM_GENERIC(       L"<symdef file>" , L"Symbols file"                                                         );
        PARAM_GENERIC(       L"<profile file>", L"Data from the profiler"                                               );
        PARAM_GENERIC(       L"<rules file>"  , L"Rules file for assignment constraints"                                );
        PARAM_GENERIC(       L"<cvs file>"    , L"Usage file"                                                           );
        PARAM_GENERIC(       L"<scatter file>", L"Scatter file"                                                         );
        PARAM_INTEGER( NULL, L"<RAM limit>"   , L"Maximum amount of RAM to reserve for code"                            );
    }
};

//--//

int _tmain(int argc, _TCHAR* argv[])
{
    TINYCLR_HEADER();

    CLR_RT_Assembly::InitString();

    CLR_RT_StringVector vec;
    Settings            st;

    ::CoInitialize( 0 );

    TINYCLR_CHECK_HRESULT(HAL_Windows::Memory_Resize( 64 * 1024 * 1024 ));

    HAL_Init_Custom_Heap();

    CLR_RT_Memory::Reset         ();    

    st.PushArguments( argc-1, argv+1, vec );

    TINYCLR_CHECK_HRESULT(st.ProcessOptions( vec ));


    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        wprintf( L"FAILURE: %S\n", CLR_RT_DUMP::GETERRORMESSAGE( hr ) ); fflush( stdout );
    }

    ::CoUninitialize();

    return FAILED(hr) ? 10 : 0;
}
