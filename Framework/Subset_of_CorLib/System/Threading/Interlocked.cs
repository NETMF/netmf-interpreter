////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Threading
namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    // After much discussion, we decided the Interlocked class doesn't need
    // any HPA's for synchronization or external threading.  They hurt C#'s
    // codegen for the yield keyword, and arguably they didn't protect much.
    // Instead, they penalized people (and compilers) for writing threadsafe
    // code.
    public static class Interlocked
    {
        /******************************
         * Increment
         *   Implemented: int
         *                        long
         *****************************/

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int Increment(ref int location);
        /******************************
         * Decrement
         *   Implemented: int
         *                        long
         *****************************/

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int Decrement(ref int location);
        //public static extern long Decrement(ref long location);

        /******************************
         * Exchange
         *   Implemented: int
         *                        long
         *                        float
         *                        double
         *                        Object
         *                        IntPtr
         *****************************/

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int Exchange(ref int location1, int value);
        /******************************
         * CompareExchange
         *    Implemented: int
         *                         long
         *                         float
         *                         double
         *                         Object
         *                         IntPtr
         *****************************/

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int CompareExchange(ref int location1, int value, int comparand);
    }
}


