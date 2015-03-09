////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_HeapBlock_I2CXAction::HandlerMethod_Initialize()
{
    NATIVE_PROFILE_CLR_I2C();
}

void CLR_RT_HeapBlock_I2CXAction::HandlerMethod_RecoverFromGC()
{
    NATIVE_PROFILE_CLR_I2C();
}

void CLR_RT_HeapBlock_I2CXAction::HandlerMethod_CleanUp()
{
    NATIVE_PROFILE_CLR_I2C();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::CreateInstance( CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock& xActionRef )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::ExtractInstance( CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_I2CXAction*& xAction )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::AllocateXAction( CLR_UINT32 numXActionUnits )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::PrepareXAction( I2C_USER_CONFIGURATION& config, size_t numXActions )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::PrepareXActionUnit( CLR_UINT8* src, size_t length, size_t unit, bool fRead )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_FEATURE_STUB_RETURN();
}

void CLR_RT_HeapBlock_I2CXAction::CopyBuffer( CLR_UINT8* dst, size_t length, size_t unit )
{
    NATIVE_PROFILE_CLR_I2C();
}

void CLR_RT_HeapBlock_I2CXAction::ReleaseBuffers()
{
    NATIVE_PROFILE_CLR_I2C();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::Enqueue()
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_FEATURE_STUB_RETURN();
}

void CLR_RT_HeapBlock_I2CXAction::Cancel( bool signal )
{
    NATIVE_PROFILE_CLR_I2C();
}

bool CLR_RT_HeapBlock_I2CXAction::IsPending()
{
    NATIVE_PROFILE_CLR_I2C();
    return true;
}

bool CLR_RT_HeapBlock_I2CXAction::IsTerminated()
{
    NATIVE_PROFILE_CLR_I2C();
    return true;
}

bool CLR_RT_HeapBlock_I2CXAction::IsCompleted()
{
    NATIVE_PROFILE_CLR_I2C();
    return true;
}

bool CLR_RT_HeapBlock_I2CXAction::IsReadXActionUnit( size_t unit )
{
    NATIVE_PROFILE_CLR_I2C();
    return true;
}

size_t CLR_RT_HeapBlock_I2CXAction::TransactedBytes()
{
    NATIVE_PROFILE_CLR_I2C();
    return 0;
}

CLR_UINT8 CLR_RT_HeapBlock_I2CXAction::GetStatus()
{
    NATIVE_PROFILE_CLR_I2C();
    return 0;
}

void CLR_RT_HeapBlock_I2CXAction::RecoverFromGC()
{
    NATIVE_PROFILE_CLR_I2C();
}

bool CLR_RT_HeapBlock_I2CXAction::ReleaseWhenDeadEx()
{
    NATIVE_PROFILE_CLR_I2C();
    return true;
}
