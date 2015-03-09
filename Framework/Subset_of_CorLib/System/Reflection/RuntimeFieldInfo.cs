////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Reflection
namespace System.Reflection
{

    using System;
    using System.Runtime.CompilerServices;

    [Serializable()]
    internal sealed class RuntimeFieldInfo : FieldInfo
    {
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

        public extern override Type FieldType
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override Object GetValue(Object obj);
    }
}


