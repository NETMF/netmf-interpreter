////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace System.Runtime.CompilerServices
{
    [Serializable, AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface,
                     AllowMultiple=false, Inherited=false)]
    public sealed class RequiredAttributeAttribute : Attribute
    {
        private Type requiredContract;

        public RequiredAttributeAttribute (Type requiredContract)
        {
            this.requiredContract= requiredContract;
        }

        public Type RequiredContract
        {
            get { return this.requiredContract; }
        }
    }
}


