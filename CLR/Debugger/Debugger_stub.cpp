////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\core\Core.h"


////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_DBG_Debugger::Debugger_WaitForCommands()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
}

void CLR_DBG_Debugger::Debugger_Discovery()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
}

void CLR_DBG_Debugger::ProcessCommands()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
}

void CLR_DBG_Debugger::PurgeCache()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
}

HRESULT CLR_DBG_Debugger::CreateInstance()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    g_CLR_DBG_Debuggers = (CLR_DBG_Debugger*)&g_scratchDebugger[0];
    TINYCLR_SYSTEM_STUB_RETURN();
}

//--//

HRESULT CLR_DBG_Debugger::DeleteInstance()
{
    NATIVE_PROFILE_CLR_DEBUGGER();
    TINYCLR_SYSTEM_STUB_RETURN();
}

void CLR_DBG_Debugger::BroadcastEvent( UINT32 cmd, UINT32 payloadSize, UINT8* payload, UINT32 flags )
{
    NATIVE_PROFILE_CLR_DEBUGGER();
}

void MfReleaseInfo::Init( MfReleaseInfo& mfReleaseInfo, UINT16 major, UINT16 minor, UINT16 build, UINT16 revision, const char *info, size_t infoLen )
{
    MFVersion::Init( mfReleaseInfo.version, major, minor, build, revision );
    mfReleaseInfo.infoString[ 0 ] = 0;
    if ( NULL != info && infoLen > 0 )
    {
        const size_t len = MIN(infoLen, sizeof(mfReleaseInfo.infoString)-1);
        hal_strncpy_s( (char*)&mfReleaseInfo.infoString[0], sizeof(mfReleaseInfo.infoString), info, len );
    }
}

