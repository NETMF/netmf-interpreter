////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyHal.h>
#include <MicroBooter_decl.h>

struct SREC_Handler
{
    int    m_Pos;
    char   m_LineBuffer[512];
    BOOL   m_Failures;
    UINT32 m_StartAddress;
    UINT32 m_ImageStart;
    UINT32 m_ImageCRC;
    UINT32 m_ImageLength;
    UINT32 m_BootMarkerAddress;
    BOOL   m_isRamBuild;
    

    const HAL_SYSTEM_MEMORY_CONFIG*  m_pMemCfg;
    BlockStorageStream m_Stream;

    void Initialize();
    BOOL Process( char c );
    void SignalFailure();
    void SignalSuccess();

    BOOL ParseLine( const char* SRECLine, BOOL readModWrite );

    //--//
    
private:
    static const char* htoi( const char* hexstring, UINT32 length, UINT32& value );
};
