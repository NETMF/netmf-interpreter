////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#pragma once

#define _WIN32_WINNT 0x0501

#define WIN32_LEAN_AND_MEAN     // Exclude rarely-used stuff from Windows headers
#include <stdio.h>
#include <tchar.h>

#include <windows.h>

#include <process.h>

#include <vector>
#include <list>

#include <cor.h>
#include <corhdr.h>
#include <corhlpr.h>

#include <AssemblyParser.h>
#include <WatchAssemblyBuilder.h>

#include <TinyCLR_Runtime.h>
#include <TinyCLR_Checks.h>
#include <TinyCLR_Diagnostics.h>
#include <TinyCLR_Graphics.h>
#include <TinyCLR_Hardware.h>

#include <TinyCLR_Debugging.h>

#include <TinyCLR_ParseOptions.h>

#include <HAL_Windows.h>