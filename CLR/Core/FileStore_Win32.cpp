////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"



////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_FileStore::LoadFile( LPCWSTR szFile, CLR_RT_Buffer& vec )
{
    TINYCLR_HEADER();

    FILE* stream;
#if defined(PLATFORM_WINCE)
    if((stream=_wfopen(szFile, L"rb"))==0)
#else
    if(_wfopen_s(&stream, szFile, L"rb" ) != 0)
#endif
    {
        wprintf( L"Cannot open '%s'!\n", szFile );
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    /*********/ fseek( stream, 0, SEEK_END );
    long size = ftell( stream              );
    /*********/ fseek( stream, 0, SEEK_SET );

    vec.resize( size );

    if(vec.size() && fread( &vec[ 0 ], vec.size(), 1, stream ) != 1)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }


    TINYCLR_CLEANUP();

    if(stream)
    {
        fclose( stream );
    }

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_FileStore::SaveFile( LPCWSTR szFile, const CLR_RT_Buffer& vec )
{
    TINYCLR_HEADER();
        
    const CLR_UINT8* buf = NULL;
    size_t size = vec.size();

    if(size > 0)
    {
        buf = &vec[ 0 ];
    }

    TINYCLR_SET_AND_LEAVE(SaveFile( szFile, buf, size ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_FileStore::SaveFile( LPCWSTR szFile, const CLR_UINT8* buf, size_t size )
{
    TINYCLR_HEADER();
    FILE* stream;

#if defined(PLATFORM_WINCE)
    if((stream=_wfopen(szFile, L"wb"))==0)
#else
    if(_wfopen_s(&stream, szFile, L"wb" ) != 0)
#endif
    {
        wprintf( L"Cannot open '%s' for writing!\n", szFile );
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    if(buf != NULL && size != 0)
    {
        if(fwrite( buf, size, 1, stream ) != 1)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }

    TINYCLR_CLEANUP();

    if(stream)
    {
        fclose( stream );
    }

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT CLR_RT_FileStore::ExtractTokensFromFile( LPCWSTR szFileName, std::vector< std::wstring >& vec, LPCWSTR separators, bool fNoComments )
{
    TINYCLR_HEADER();

    CLR_RT_Buffer buf;

    TINYCLR_CHECK_HRESULT(LoadFile( szFileName, buf ));

    ExtractTokens( buf, vec, separators, fNoComments );

    TINYCLR_NOCLEANUP();
}

void CLR_RT_FileStore::ExtractTokens( const CLR_RT_Buffer& buf, CLR_RT_StringVector& vec, LPCWSTR separators, bool fNoComments )
{
    std::wstring tmp;
    LPSTR        szBufA = NULL;
    LPWSTR       szBufW = NULL;
    LPCWSTR      src;
    size_t       len;

    if (buf.size() == 0) { return; }
    else if(buf.size() >= 2 && buf[ 0 ] == 0xFF && buf[ 1 ] == 0xFE)
    {
        len = (buf.size() - 2) / sizeof(WCHAR);

        src = (LPCWSTR)&buf[ 2 ];
    }
    else
    {
        len = buf.size() / sizeof(CHAR);

        szBufA = new CHAR[ len+1 ]; memcpy( szBufA, &buf[ 0 ], len ); szBufA[ len ] = 0;
        CLR_RT_UnicodeHelper::ConvertFromUTF8( szBufA, tmp );

        src = tmp.c_str();
        len = tmp.size();
    }

    //--//

    szBufW = new WCHAR[ len+1 ]; memcpy( szBufW, src, len * sizeof(WCHAR) ); szBufW[ len ] = 0;

    vec.reserve( len / 60 );

    ExtractTokensFromBuffer( szBufW, vec, separators, fNoComments );

    //--//

    delete[] szBufW;
    delete[] szBufA;
}

void CLR_RT_FileStore::ExtractTokensFromBuffer( LPWSTR szLine, CLR_RT_StringVector& vec, LPCWSTR separators, bool fNoComments )
{
    while(*szLine)
    {
        LPWSTR szNextLine = szLine;

        while(*szNextLine)
        {
            if(*szNextLine == '\r' || *szNextLine == '\n')
            {
                while(*szNextLine == '\r' || *szNextLine == '\n')
                {
                    *szNextLine++ = 0;
                }

                break;
            }

            szNextLine++;
        }

        if(fNoComments == false || szLine[ 0 ] != '#')
        {
            CLR_RT_FileStore::ExtractTokensFromString( szLine, vec, separators );
        }

        szLine = szNextLine;
    }
}

void CLR_RT_FileStore::ExtractTokensFromString( LPCWSTR szLine, CLR_RT_StringVector& vec, LPCWSTR separators )
{
    if(separators)
    {
        while(szLine[ 0 ] && wcschr( separators, szLine[0] )) szLine++;

        while(szLine[ 0 ])
        {
            WCHAR  token        [ 2048 ];
#if !defined(PLATFORM_WINCE)
            WCHAR  tokenExpanded[ 2048 ];
#endif
            bool   fQuote = false;
            size_t pos;

            for(pos=0; pos<MAXSTRLEN(token); )
            {
                WCHAR c = *szLine++;

                if(c == 0)
                {
                    szLine--;
                    break;
                }

                if(fQuote)
                {
#if !defined(PLATFORM_WINCE)
                    if(c == '\\')
                    {
                        c = *szLine++;;
                    }
                    else
#endif
                        if(c == '"')
                    {
                        fQuote = false;
                        continue;
                    }
                }
                else if(c == '"')
                {
                    fQuote = true;
                    continue;
                }
                else
                {
                    if(wcschr( separators, c )) break;
                }

                token[ pos++ ] = c;
            }
            token[ pos ] = 0;

#if !defined(PLATFORM_WINCE)
            ::ExpandEnvironmentStringsW( token, tokenExpanded, MAXSTRLEN(tokenExpanded) );
            vec.push_back( tokenExpanded );
#else
            vec.push_back( token );
#endif


            while(szLine[ 0 ] && wcschr( separators, szLine[ 0 ] )) szLine++;
        }
    }
    else
    {
        vec.push_back( szLine );
    }
}
