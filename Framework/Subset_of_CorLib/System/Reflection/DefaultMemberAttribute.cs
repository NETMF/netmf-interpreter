////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Reflection
namespace System.Reflection
{

    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface), Serializable]
    public sealed class DefaultMemberAttribute : Attribute
    {
        // The name of the member
        private String m_memberName;

        // You must provide the name of the member, this is required
        public DefaultMemberAttribute(String memberName)
        {
            m_memberName = memberName;
        }

        // A get accessor to return the name from the attribute.
        // NOTE: There is no setter because the name must be provided
        //  to the constructor.  The name is not optional.
        public String MemberName
        {
            get { return m_memberName; }
        }
    }
}


