////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

CLR_UINT32                 g_buildCRC;

CLR_RT_ExecutionEngine     g_CLR_RT_ExecutionEngine;
CLR_RT_WellKnownTypes      g_CLR_RT_WellKnownTypes;
CLR_RT_WellKnownMethods    g_CLR_RT_WellKnownMethods;
CLR_RT_TypeSystem          g_CLR_RT_TypeSystem;
CLR_RT_EventCache          g_CLR_RT_EventCache;
CLR_RT_GarbageCollector    g_CLR_RT_GarbageCollector;
CLR_HW_Hardware            g_CLR_HW_Hardware;

const CLR_RADIAN c_CLR_radians[] =
{
    { 1000, -0  },
    { 995,  105 },
    { 978,  208 },
    { 951,  309 },
    { 914,  407 },
    { 866,  500 },
    { 809,  588 },
    { 743,  669 },
    { 669,  743 },
    { 588,  809 },
    { 500,  866 },
    { 407,  914 },
    { 309,  951 },
    { 208,  978 },
    { 105,  995 },
    { 0,   1000 },
    { -105, 995 },
    { -208, 978 },
    { -309, 951 },
    { -407, 914 },
    { -500, 866 },
    { -588, 809 },
    { -669, 743 },
    { -743, 669 },
    { -809, 588 },
    { -866, 500 },
    { -914, 407 },
    { -951, 309 },
    { -978, 208 },
    { -995, 105 },
    { -1000, 0  },
    { -995, -105 },
    { -978, -208 },
    { -951, -309 },
    { -914, -407 },
    { -866, -500 },
    { -809, -588 },
    { -743, -669 },
    { -669, -743 },
    { -588, -809 },
    { -500, -866 },
    { -407, -914 },
    { -309, -951 },
    { -208, -978 },
    { -105, -995 },
    { 0,   -1000 },
    { 105, -995 },
    { 208, -978 },
    { 309, -951 },
    { 407, -914 },
    { 500, -866 },
    { 588, -809 },
    { 669, -743 },
    { 743, -669 },
    { 809, -588 },
    { 866, -500 },
    { 914, -407 },
    { 951, -309 },
    { 978, -208 },
    { 995, -105 },
};

// the Arm 3.0 compiler drags in a bunch of ABI methods (for initialization) if struct arrays are not initialized
CLR_UINT32        g_scratchDebugger         [ sizeof(CLR_DBG_Debugger) * NUM_DEBUGGERS / sizeof(UINT32) + 1 ];
CLR_UINT32        g_scratchDebuggerMessaging[ sizeof(CLR_Messaging)    * NUM_DEBUGGERS / sizeof(UINT32) + 1 ];
CLR_DBG_Debugger *g_CLR_DBG_Debuggers;

CLR_RT_Persistence_Manager g_CLR_RT_Persistence_Manager;

//--//

