////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_PARSEOPTIONS_H_
#define _TINYCLR_PARSEOPTIONS_H_

#include <TinyCLR_Runtime.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(_WIN32)

struct CLR_RT_ParseOptions
{
    typedef std::map< std::wstring, CLR_RT_Buffer* > BufferMap;
    typedef BufferMap::iterator                      BufferMapIter;

    struct Parameter
    {
        LPCWSTR m_szName;
        LPCWSTR m_szDescription;

        Parameter( LPCWSTR szName, LPCWSTR szDescription );

        virtual bool Parse( LPCWSTR val ) = 0;
    };

    typedef std::vector< Parameter* > ParameterList;
    typedef ParameterList::iterator   ParameterListIter;

    CLR_RT_StringVector CommandLineArgs;

    //--//

    struct Parameter_Generic : Parameter
    {
        std::wstring m_data;

        Parameter_Generic( LPCWSTR szName, LPCWSTR szDescription );

        virtual bool Parse( LPCWSTR arg );
    };

    struct Parameter_String : Parameter
    {
        std::wstring  m_dataParsed;
        std::wstring* m_dataPtr;

        Parameter_String( std::wstring* data, LPCWSTR szName, LPCWSTR szDescription );

        virtual bool Parse( LPCWSTR arg );
    };

    struct Parameter_Boolean : Parameter
    {
        bool  m_dataParsed;
        bool* m_dataPtr;

        Parameter_Boolean( bool* data, LPCWSTR szName, LPCWSTR szDescription );

        virtual bool Parse( LPCWSTR arg );
    };

    struct Parameter_Integer : Parameter
    {
        int  m_dataParsed;
        int* m_dataPtr;

        Parameter_Integer( int* data, LPCWSTR szName, LPCWSTR szDescription );

        virtual bool Parse( LPCWSTR arg );
    };

    struct Parameter_Float : Parameter
    {
        float  m_dataParsed;
        float* m_dataPtr;

        Parameter_Float( float* data, LPCWSTR szName, LPCWSTR szDescription );

        virtual bool Parse( LPCWSTR arg );
    };

    //--//

    struct Command
    {
        LPCWSTR       m_szName;
        LPCWSTR       m_szDescription;
        ParameterList m_params;

        Command( LPCWSTR szName, LPCWSTR szDescription );

        virtual bool Parse( CLR_RT_StringVector& argv, size_t& pos, CLR_RT_ParseOptions& options );

        virtual HRESULT Execute();
    };

    typedef std::list< Command* > CommandList;
    typedef CommandList::iterator CommandListIter;

    //--//

    struct Command_SetFlag : Command
    {
        bool  m_dataParsed;
        bool* m_dataPtr;

        Command_SetFlag( bool* data, LPCWSTR szName, LPCWSTR szDescription );

        virtual bool Parse( CLR_RT_StringVector& argv, size_t& pos, CLR_RT_ParseOptions& options );
    };

    //--//

    bool        m_fVerbose;
    CommandList m_commands;

    //--//

    CLR_RT_ParseOptions();

    HRESULT ExtractOptionsFromFile( LPCWSTR szFileName );

    HRESULT ReprocessOptions();
    HRESULT ProcessOptions(                          CLR_RT_StringVector& vec );
    void    PushArguments ( int argc, LPWSTR argv[], CLR_RT_StringVector& vec );

    virtual void Usage();
};

#endif

#endif // _TINYCLR_PARSEOPTIONS_H_
