////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System
namespace System
{

    using System;
    using System.Reflection;
    using System.Threading;
    using System.Runtime.CompilerServices;
    [Serializable()]
    public abstract class Delegate
    {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public override extern bool Equals(Object obj);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Delegate Combine(Delegate a, Delegate b);

        extern public MethodInfo Method
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public Object Target
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Delegate Remove(Delegate source, Delegate value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool operator ==(Delegate d1, Delegate d2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool operator !=(Delegate d1, Delegate d2);

    }
}


