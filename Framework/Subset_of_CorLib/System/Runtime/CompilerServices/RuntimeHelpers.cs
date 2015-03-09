////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Runtime.CompilerServices
namespace System.Runtime.CompilerServices
{

    using System;
    using System.Runtime.CompilerServices;
    [Serializable]
    public static class RuntimeHelpers
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void InitializeArray(Array array, RuntimeFieldHandle fldHandle);

        /**
         * GetObjectValue is intended to allow value classes to be manipulated as 'Object'
         * but have aliasing behavior of a value class.  The intent is that you would use
         * this function just before an assignment to a variable of type 'Object'.  If the
         * value being assigned is a mutable value class, then a shallow copy is returned
         * (because value classes have copy semantics), but otherwise the object itself
         * is returned.
         *
         * Note: VB calls this method when they're about to assign to an Object
         * or pass it as a parameter.  The goal is to make sure that boxed
         * value types work identical to unboxed value types - ie, they get
         * cloned when you pass them around, and are always passed by value.
         * Of course, reference types are not cloned.  -- BrianGru  7/12/2001
         *
         * @param obj The object that is about to be assigned.
         * @return a shallow copy of 'obj' if it is a value class, 'obj' itself otherwise
         */
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Object GetObjectValue(Object obj);

        /**
         * RunClassConstructor causes the class constructor for the given type to be triggered
         * in the current domain.  After this call returns, the class constructor is guaranteed to
         * have at least been started by some thread.  In the absence of class constructor
         * deadlock conditions, the call is further guaranteed to have completed.
         *
         * This call will generate an exception if the specified class constructor threw an
         * exception when it ran.
         */

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void RunClassConstructor(RuntimeTypeHandle type);

        extern public static int OffsetToStringData
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }
    }
}


