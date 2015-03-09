////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading
{
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System;
    public abstract class WaitHandle : MarshalByRefObject
    {
        public const int WaitTimeout = 0x102;
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual bool WaitOne(int millisecondsTimeout, bool exitContext);
        public virtual bool WaitOne()
        {
            return WaitOne(Timeout.Infinite, false);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int WaitMultiple(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext, bool WaitAll);

        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
        {
            return (WaitMultiple(waitHandles, millisecondsTimeout, exitContext, true /* waitall*/ ) != WaitTimeout);
        }

        public static bool WaitAll(WaitHandle[] waitHandles)
        {
            return WaitAll(waitHandles, Timeout.Infinite, true);
        }

        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
        {
            return WaitMultiple(waitHandles, millisecondsTimeout, exitContext, false /* waitany*/ );
        }

        public static int WaitAny(WaitHandle[] waitHandles)
        {
            return WaitAny(waitHandles, Timeout.Infinite, true);
        }
    }
}


