////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT.Platform.Test;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_EmulateHardware
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual void EnableInterrupt();

        public static void Main()
        {
            // TODO: Add your other test classes to args.
            string[] args = { "TestPower", "TestWatchdog", "EmulateHardware", "TestPorts" };
            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}