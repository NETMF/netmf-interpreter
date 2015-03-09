////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Reflection
{

    using System;
    [Serializable, FlagsAttribute()]
    public enum AssemblyNameFlags
    {
        None = 0x0000,
        PublicKey = 0x0001,

        [Obsolete("This will be removed before Whidbey ships.  There will be no replacement for Whidbey.")]
        LongevityUnspecified = 0x0000,     // Nothing set.

        [Obsolete("This will be removed before Whidbey ships.  There will be no replacement for Whidbey.")]
        Library = 0x0002,     // All types in the assembly are Library types.
        [Obsolete("This will be removed before Whidbey ships.  There will be no replacement for Whidbey.")]
        AppDomainPlatform = 0x0004,     // All types in the assembly are Platform types.
        [Obsolete("This will be removed before Whidbey ships.  There will be no replacement for Whidbey.")]
        ProcessPlatform = 0x0006,     // All types in the assembly are Platform types.
        [Obsolete("This will be removed before Whidbey ships.  There will be no replacement for Whidbey.")]
        SystemPlatform = 0x0008,     // All types in the assembly are Platform types.
        [Obsolete("This will be removed before Whidbey ships.  There will be no replacement for Whidbey.")]
        LongevityMask = 0x000E,     // Bits describing the platform/library property of the assembly.
        // Accessible via AssemblyName.ProcessorArchitecture
        EnableJITcompileOptimizer = 0x4000,
        EnableJITcompileTracking = 0x8000,
        Retargetable = 0x0100,

    }

    [Serializable]
    public enum ProcessorArchitecture
    {
        None = 0x0000,
        MSIL = 0x0001,
        X86 = 0x0002,
        IA64 = 0x0003,
        Amd64 = 0x0004
    }
}


