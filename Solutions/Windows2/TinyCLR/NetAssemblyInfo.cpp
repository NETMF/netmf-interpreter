////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "EmulatorNative.h"

using namespace System;
using namespace System::Reflection;

[assembly:AssemblyDescriptionAttribute  ("The .NET Micro Framework CLR for Windows")];
[assembly:AssemblyCompanyAttribute      ("Microsoft Corporation")];
[assembly:AssemblyCopyrightAttribute    ("Copyright Microsoft Corporation. All rights reserved.")];

#ifdef _DEBUG
[assembly:AssemblyConfigurationAttribute("Debug")];
#else
[assembly:AssemblyConfigurationAttribute("Release")];
#endif

// Other attributes you might provide:
//[assembly:AssemblyProductAttribute("")];
//[assembly:AssemblyTitleAttribute("")];
//[assembly:AssemblyTrademarkAttribute("")];
//[assembly:AssemblyCultureAttribute("")];

// Version attribute synthesized from preprocessor defines provided by the build system
#define _MACRO_STRINGIZER(arg) #arg
#define MACRO_STRINGIZER(arg) _MACRO_STRINGIZER(arg)

[assembly:AssemblyVersionAttribute( MACRO_STRINGIZER(VERSION_MAJOR.VERSION_MINOR.VERSION_BUILD.VERSION_REVISION) )];

// All solutions are expected to provide an implementation of this
unsigned int Solution_GetReleaseInfo(MfReleaseInfo& releaseInfo)
{
    MfReleaseInfo::Init(releaseInfo,
                        VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION,
                        OEMSYSTEMINFOSTRING, hal_strlen_s(OEMSYSTEMINFOSTRING)
                        );
    return 1; // alternatively, return false if you didn't initialize the releaseInfo structure.
}

