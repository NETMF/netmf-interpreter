////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    //This class only static members and doesn't require the serializable keyword.

    using System;
    using System.Runtime.CompilerServices;

    public static class GC
    {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool AnyPendingFinalizers();

        public static void WaitForPendingFinalizers()
        {
            while (AnyPendingFinalizers()) System.Threading.Thread.Sleep(10);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void SuppressFinalize(Object obj);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void ReRegisterForFinalize(Object obj);

    }
}


