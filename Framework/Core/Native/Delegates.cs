////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    public static class WeakDelegate
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public System.Delegate Combine(System.Delegate a, System.Delegate b);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public System.Delegate Remove(System.Delegate a, System.Delegate b);
    }
}


