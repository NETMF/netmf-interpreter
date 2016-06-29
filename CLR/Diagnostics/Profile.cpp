////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Diagnostics.h"

//--//

#if defined(TINYCLR_PROFILE_NEW_CALLS)

void* CLR_PROF_CounterCallChain::Prepare( CLR_PROF_Handler* handler )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    if(m_owningHandler)
    {
        CLR_UINT64 t = handler->GetFrozenTime() - m_owningHandler->m_time_start;

        Complete( t, m_owningHandler );

        handler->m_target_Mode = CLR_PROF_Handler::c_Mode_Ignore;

        return NULL;
    }
    else
    {
        m_owningHandler = handler;

        handler->m_target_Mode = CLR_PROF_Handler::c_Mode_CallChain;

        return this;
    }
}

void CLR_PROF_CounterCallChain::Complete( CLR_UINT64& t, CLR_PROF_Handler* handler )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    if(m_owningHandler)
    {
        m_time_exclusive              += t;

        m_owningHandler->m_target = NULL;
        m_owningHandler           = NULL;
    }
}

void CLR_PROF_CounterCallChain::Enter( CLR_RT_StackFrame* stack )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_PROF_Handler::SuspendTime();

    TINYCLR_CLEAR(*this);

    m_owningStackFrame = stack;

    CLR_PROF_Handler::ResumeTime();
}

void CLR_PROF_CounterCallChain::Leave()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_PROF_Handler::SuspendTime();
    CLR_PROF_Handler::ResumeTime();
}

#endif

//--//

#if defined(TINYCLR_PROFILE_HANDLER)

         bool              CLR_PROF_Handler::s_initialized;
         CLR_PROF_Handler* CLR_PROF_Handler::s_current;
volatile CLR_UINT64        CLR_PROF_Handler::s_time_overhead;
volatile CLR_UINT64        CLR_PROF_Handler::s_time_freeze;
volatile CLR_UINT64        CLR_PROF_Handler::s_time_adjusted;

//--//


// TODO: roll this entire function into the HAL so that
// hardware with a HighRes counter can be used, this
// implementation is easy enough to move to the HAL as
// a support function for systems with only a lowres
// counter
static CLR_UINT64 GetPerformanceCounter()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
#if defined(_WIN32)
    return HAL_Windows_GetPerformanceTicks();
#else
    static CLR_UINT32 rollover  = 0;
    static CLR_UINT32 lastValue = 0;

    CLR_UINT32 value = ::Time_PerformanceCounter();

    if(lastValue > value) rollover++;

    lastValue = value;

    return ((CLR_UINT64)rollover << 32) | (CLR_UINT64)value;
#endif
}

//--//

void CLR_PROF_Handler::Constructor()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    SuspendTime();

    m_target_Mode = c_Mode_Ignore;

    Init( NULL );
}

#if defined(TINYCLR_PROFILE_NEW_CALLS)
void CLR_PROF_Handler::Constructor( CLR_PROF_CounterCallChain& target )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    SuspendTime();

    Init( target.Prepare( this ) );
}
#endif

void CLR_PROF_Handler::Destructor()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    SuspendTime();

    CLR_UINT64 t = GetFrozenTime() - m_time_start;

    if(m_target_Mode == c_Mode_Ignore)
    {
        s_time_adjusted += t;
    }
    else
    {
        if(m_containing)
        {
            m_containing->m_time_correction += t;
        }

        if(m_target)
        {
            switch(m_target_Mode)
            {
#if defined(TINYCLR_PROFILE_NEW_CALLS)
            case c_Mode_CallChain: ((CLR_PROF_CounterCallChain*)m_target)->Complete( t, this ); break;
#endif
            }
        }
    }

    s_current = m_containing;

    ResumeTime();
}


void CLR_PROF_Handler::Init( void* target )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    m_target     = target;
    m_containing = s_current; s_current = this;

    if(m_target)
    {
        if(m_containing && m_containing->m_target == NULL)
        {
            m_target = NULL;
        }
    }

    if(m_target == NULL)
    {
        m_target_Mode = c_Mode_Ignore;
    }

    m_time_correction = 0;
    m_time_start      = ResumeTime();
}

static void TestCalibrate( CLR_PROF_CounterCallChain& cnt )
{
    CLR_PROF_Handler c( cnt );
}

void CLR_PROF_Handler::Calibrate()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
#if defined(PLATFORM_ARM)
    const int c_training = 10;
#else
    const int c_training = 1024;
#endif

    if(s_initialized) return;

    s_current       = NULL;
    s_time_overhead = 0;
    s_time_freeze   = 0;
    s_time_adjusted = 0;

#if defined(PLATFORM_ARM)
    ::Time_PerformanceCounter_Initialize();
#endif

    int              i;
    CLR_PROF_CounterCallChain tmp;

    for(i=0; i<c_training; i++)
    {
        TINYCLR_CLEAR(tmp);
        TestCalibrate( tmp );

        CLR_INT64 diff = tmp.m_time_exclusive;

        if(diff == 0) break;

        if(diff > 100 && i > c_training/2)
        {
            i--;
            continue;
        }

        s_time_overhead += diff;
    }

    s_time_adjusted = GetPerformanceCounter();

    s_initialized = true;
}

void CLR_PROF_Handler::SuspendTime()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    s_time_freeze = GetPerformanceCounter();
}

CLR_UINT64 CLR_PROF_Handler::GetFrozenTime()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    return s_time_freeze - s_time_adjusted;
}

CLR_UINT64 CLR_PROF_Handler::ResumeTime()
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    CLR_UINT64 res = GetFrozenTime();

    s_time_adjusted = (GetPerformanceCounter() - res) + s_time_overhead;

    return res;
}

CLR_UINT64 CLR_PROF_Handler::ResumeTime( CLR_INT64 t )
{
    NATIVE_PROFILE_CLR_DIAGNOSTICS();
    s_time_adjusted += t;

    return ResumeTime();
}
#endif //defined(TINYCLR_PROFILE_HANDLER)

//--//

