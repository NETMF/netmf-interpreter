////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Runtime.CompilerServices
namespace System.Runtime.CompilerServices
{
    [Serializable, AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited=false)]
    public sealed class DateTimeConstantAttribute : CustomConstantAttribute
    {
        public DateTimeConstantAttribute(long ticks)
        {
            date = new System.DateTime(ticks);
        }

        public override Object Value
        {
            get {
                return date;
            }
        }

        private System.DateTime date;
    }
}


