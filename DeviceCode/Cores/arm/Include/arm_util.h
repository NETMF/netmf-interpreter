////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _ARM_UTIL_H_
#define _ARM_UTIL_H_ 1

//--//

// same for all arm/thumb

#if defined(FIQ_SAMPLING_PROFILER)
extern "C"
{
void Profiler_FIQ_Initialize();
}
extern void FIQ_Profiler_Dump();
#endif

#define NOP() {__asm{ NOP }}

#if defined(PLATFORM_ARM)  && defined(COMPILE_ARM)

#include "smartptr_FIQ_arm.h"

#else

// TODO - use FIQ stibs for now in thumb more as well
// Cortex - thumb2 doesn't have FIQ

// stubs
class SmartPtr_FIQ
{
    UINT32 m_state;

public:

    SmartPtr_FIQ()  { m_state = 1; }
    ~SmartPtr_FIQ() { m_state = 0; }

    BOOL WasDisabled()
    {
        return FALSE;
    }

    void Acquire()
    {
    }

    void Release()
    {
    }

    void Probe()
    {
    }

    static BOOL GetState()
    {
        return FALSE;
    }

    static BOOL ForceDisabled()
    {
        return FALSE;
    }

    static BOOL ForceEnabled()
    {
        return TRUE;
    }
};

#endif

//--//

#if defined(FIQ_SAMPLING_PROFILER)

void FIQ_Profiler_Init();

void FIQ_Profiler_Dump();

void Profiler_FIQ_Initialize();

#endif  // defined(FIQ_SAMPLING_PROFILER)

#if !defined(_WIN32) && !defined(FIQ_SAMPLING_PROFILER) && defined(PROFILE_BUILD)

void Native_Profiler_Init();

void Native_Profiler_Dump();

void Native_Profiler_WriteToCOM(void *buffer, UINT32 size);

UINT64 Native_Profiler_TimeInMicroseconds();

void Native_Profiler_Start();

void Native_Profiler_Stop();

#endif                                

#if defined(DEBUG) 
#define JTAG_DEBUGGING 
#else
#undef JTAG_DEBUGGING
#endif

// keeping the clock always on help the JTAG, but wastes battery quickly!
#if defined(JTAG_DEBUGGING)
    #define CPU_SPIN_NOT_SLEEP 1
#endif

// the FIQ sampling profiler needs the CPU to always spin and never sleep
#if defined(FIQ_SAMPLING_PROFILER)
    #define CPU_SPIN_NOT_SLEEP 1
#endif
// the FIQ latency profiler needs the ARM timers to always run

/********************************************************************************/

#endif  // _ARM_UTIL_H_

