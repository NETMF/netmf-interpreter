////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_HeapBlock_NativeEventDispatcher::HandlerMethod_Initialize()
{
    NATIVE_PROFILE_CLR_IOPORT();
}

void CLR_RT_HeapBlock_NativeEventDispatcher::HandlerMethod_RecoverFromGC()
{
    NATIVE_PROFILE_CLR_IOPORT();
}

void CLR_RT_HeapBlock_NativeEventDispatcher::HandlerMethod_CleanUp()
{
    NATIVE_PROFILE_CLR_IOPORT();
}

void SaveNativeEventToHALQueue( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT32 data1, UINT32 data2 )
{
    NATIVE_PROFILE_CLR_IOPORT();
}

void CleanupNativeEventsFromHALQueue( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )
{
    NATIVE_PROFILE_CLR_IOPORT();
}

HRESULT CLR_RT_HeapBlock_NativeEventDispatcher::CreateInstance( CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock& portRef )
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_HeapBlock_NativeEventDispatcher::ExtractInstance( CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_NativeEventDispatcher*& port )
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_FEATURE_STUB_RETURN();
}

void CLR_RT_HeapBlock_NativeEventDispatcher::RecoverFromGC()
{
    NATIVE_PROFILE_CLR_IOPORT();
}

bool CLR_RT_HeapBlock_NativeEventDispatcher::ReleaseWhenDeadEx()
{
    NATIVE_PROFILE_CLR_IOPORT();
    return true;
}

void CLR_RT_HeapBlock_NativeEventDispatcher::RemoveFromHALQueue()
{
    NATIVE_PROFILE_CLR_IOPORT();
}
void CLR_RT_HeapBlock_NativeEventDispatcher::SaveToHALQueue( UINT32 data1, UINT32 data2 )
{
    NATIVE_PROFILE_CLR_IOPORT();
}

HRESULT CLR_RT_HeapBlock_NativeEventDispatcher::StartDispatch( CLR_RT_ApplicationInterrupt* appInterrupt, CLR_RT_Thread* th )
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_FEATURE_STUB_RETURN();
}

void CLR_RT_HeapBlock_NativeEventDispatcher::ThreadTerminationCallback( void* arg )
{
    NATIVE_PROFILE_CLR_IOPORT();
}

HRESULT CLR_RT_HeapBlock_NativeEventDispatcher::RecoverManagedObject( CLR_RT_HeapBlock*& port )
{
    NATIVE_PROFILE_CLR_IOPORT();
    TINYCLR_FEATURE_STUB_RETURN();
}
