////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Runtime.InteropServices
namespace System.Runtime.InteropServices
{

    using System;
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
    public sealed class ComVisibleAttribute : Attribute
    {
        internal bool _val;
        public ComVisibleAttribute(bool visibility)
        {
            _val = visibility;
        }

        public bool Value { get { return _val; } }
    }

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Delegate, Inherited = false)]
    public sealed class GuidAttribute : Attribute
    {
        internal String _val;
        public GuidAttribute(String guid)
        {
            _val = guid;
        }

        public String Value { get { return _val; } }
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class OutAttribute : Attribute
    {
        public OutAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class StructLayoutAttribute : Attribute
    {
        internal LayoutKind _val;
        public StructLayoutAttribute(LayoutKind layoutKind)
        {
            _val = layoutKind;
        }

        public StructLayoutAttribute(short layoutKind)
        {
            _val = (LayoutKind)layoutKind;
        }

        public LayoutKind Value { get { return _val; } }
        public int Pack;
        public int Size;
        public CharSet CharSet;
    }
}


