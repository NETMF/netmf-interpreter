////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System
namespace System
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [Serializable()]
    public abstract class MulticastDelegate : Delegate
    {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool operator ==(MulticastDelegate d1, MulticastDelegate d2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool operator !=(MulticastDelegate d1, MulticastDelegate d2);

    }
}


