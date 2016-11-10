////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <CPU_WATCHDOG_decl.h>

#ifndef NATIVE_PROFILER_CPP
#define NATIVE_PROFILER_CPP


//--//
#if !defined(_WIN32) && !defined(FIQ_SAMPLING_PROFILER) && !defined(HAL_REDUCESIZE) && defined(PROFILE_BUILD)

#include "..\Native_Profiler.h"


__weak Native_Profiler::Native_Profiler()
{
}


__weak Native_Profiler::~Native_Profiler()
{
}


__weak void Native_Profiler_Init()
{
}


__weak void Native_Profiler_Dump()
{
}


__weak void Native_Profiler_WriteToCOM(void *buffer, UINT32 size)
{
}


__weak UINT64 Native_Profiler_TimeInMicroseconds()
{
    return 0;
}

__weak void Native_Profiler_Start()
{
}

__weak void Native_Profiler_Stop()
{
}

#endif  // !defined(_WIN32) && !defined(FIQ_SAMPLING_PROFILER) && !defined(HAL_REDUCESIZE) && defined(PROFILE_BUILD)
#endif  // defined(NATIVE_PROFILER_CPP)
