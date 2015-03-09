////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////


void CLR_RT_HeapBlock_EndPoint::HandlerMethod_Initialize()
{
}

void CLR_RT_HeapBlock_EndPoint::HandlerMethod_RecoverFromGC()
{
}

void CLR_RT_HeapBlock_EndPoint::HandlerMethod_CleanUp()
{
}

CLR_RT_HeapBlock_EndPoint* CLR_RT_HeapBlock_EndPoint::FindEndPoint( const CLR_RT_HeapBlock_EndPoint::Port& port )
{
    return NULL;
}

bool CLR_RT_HeapBlock_EndPoint::Port::Compare( const CLR_RT_HeapBlock_EndPoint::Port& port )
{
    return false;
}

//--//

HRESULT CLR_RT_HeapBlock_EndPoint::CreateInstance( const CLR_RT_HeapBlock_EndPoint::Port& port, CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock& epRef )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_HeapBlock_EndPoint::ExtractInstance( CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_EndPoint*& endPoint )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

bool CLR_RT_HeapBlock_EndPoint::ReleaseWhenDeadEx()
{
    return true;
}

//--//

void CLR_RT_HeapBlock_EndPoint::RecoverFromGC()
{
}

//--//

CLR_RT_HeapBlock_EndPoint::Message* CLR_RT_HeapBlock_EndPoint::FindMessage( CLR_UINT32 cmd, const CLR_UINT32* seq )
{
    return NULL;
}
