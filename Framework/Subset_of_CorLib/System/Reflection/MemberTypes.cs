////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Reflection
namespace System.Reflection
{

    using System;
    /**
     * This Enum matchs the CorTypeAttr defined in CorHdr.h
     */
    [Serializable()]
    public enum MemberTypes
    {
        // The following are the known classes which extend MemberInfo
        Constructor = 0x01,
        Event = 0x02,
        Field = 0x04,
        Method = 0x08,
        Property = 0x10,
        TypeInfo = 0x20,
        Custom = 0x40,
        NestedType = 0x80,
        All = Constructor | Event | Field | Method | Property | TypeInfo | NestedType,
    }
}


