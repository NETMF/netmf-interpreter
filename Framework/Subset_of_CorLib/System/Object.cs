////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System;
    using System.Runtime.CompilerServices;
    /**
     * The <i>Object</i> is the root class for all object in the CLR System. <i>Object</i>
     * is the super class for all other CLR objects and provide a set of methods and low level
     * services to subclasses.  These services include object synchronization and support for clone
     * operations.
     *
     * @see System.ICloneable
     */
    //This class contains no data and does not need to be serializable
    [Serializable()]
    public class Object
    {
        [Diagnostics.DebuggerHidden]
        public Object()
        {
        }

        public virtual String ToString()
        {
            return GetType().FullName;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual bool Equals(Object obj);

        public static bool Equals(Object objA, Object objB)
        {
            if (objA == objB)
            {
                return true;
            }

            if (objA == null || objB == null)
            {
                return false;
            }

            return objA.Equals(objB);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool ReferenceEquals(Object objA, Object objB);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual int GetHashCode();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern Type GetType();

        [Diagnostics.DebuggerHidden]
        ~Object()
        {
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        protected extern Object MemberwiseClone();

    }
}


