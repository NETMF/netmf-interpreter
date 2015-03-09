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


Native_Profiler::Native_Profiler()
{
}


Native_Profiler::~Native_Profiler()
{
}


void Native_Profiler_Init()
{
}


void Native_Profiler_Dump()
{
}


void Native_Profiler_WriteToCOM(void *buffer, UINT32 size)
{
}


UINT64 Native_Profiler_TimeInMicroseconds()
{
    return 0;
}

void Native_Profiler_Start()
{
}

void Native_Profiler_Stop()
{
}

#endif  // !defined(_WIN32) && !defined(FIQ_SAMPLING_PROFILER) && !defined(HAL_REDUCESIZE) && defined(PROFILE_BUILD)
#endif  // defined(NATIVE_PROFILER_CPP)
