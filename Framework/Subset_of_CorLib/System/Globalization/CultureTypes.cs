////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#define ENABLE_CROSS_APPDOMAIN
#define ENABLE_CROSS_APPDOMAIN

namespace System.Globalization
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Reflection;
    using System.Resources;
    public enum CultureTypes
    {
        AllCultures,
        FrameworkCultures,
        InstalledWin32Cultures,
        NeutralCultures,
        ReplacementCultures,
        SpecificCultures,
        UserCustomCulture,
        WindowsOnlyCultures
    }
}


