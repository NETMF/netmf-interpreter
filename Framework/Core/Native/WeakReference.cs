////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    public class ExtendedWeakReference : System.WeakReference
    {
        public const uint c_SurviveBoot = 0x00000001;
        public const uint c_SurvivePowerdown = 0x00000002;

        public enum PriorityLevel : int
        {
            OkayToThrowAway = 1000,
            NiceToHave = 500000,
            Important = 750000,
            Critical = 1000000,
            System = 10000000,
        }

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public ExtendedWeakReference(object target, Type selector, uint id, uint flags);
        //--//
        extern public Type Selector
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public uint Id
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public uint Flags
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        //--//
        extern public int Priority
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        //--//

        static public ExtendedWeakReference RecoverOrCreate(Type selector, uint id, uint flags)
        {
            ExtendedWeakReference wr = ExtendedWeakReference.Recover(selector, id);

            return (wr != null) ? wr : new ExtendedWeakReference(null, selector, id, flags);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public ExtendedWeakReference Recover(Type selector, uint id);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void PushBackIntoRecoverList();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void FlushAll();
    }
}


