////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Reflection
namespace System.Reflection
{
    using System.Runtime.CompilerServices;
    ////////////////////////////////////////////////////////////////////////////////
    //   Method is the class which represents a Method. These are accessed from
    //   Class through getMethods() or getMethod(). This class contains information
    //   about each method and also allows the method to be dynamically invoked
    //   on an instance.
    ////////////////////////////////////////////////////////////////////////////////
    using System;
    [Serializable()]
    public abstract class MethodBase : MemberInfo
    {
        public extern bool IsPublic
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsStatic
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsFinal
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsVirtual
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsAbstract
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern Object Invoke(Object obj, Object[] parameters);
        public extern override String Name
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern override Type DeclaringType
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }
    }
}


