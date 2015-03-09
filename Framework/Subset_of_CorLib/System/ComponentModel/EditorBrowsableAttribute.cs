////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.ComponentModel
namespace System.ComponentModel
{

    using System;

    public enum EditorBrowsableState
    {
        Always,
        Never,
        Advanced
    }

    /** Custom attribute to indicate that a specified object
     * should be hidden from the editor. (i.e Intellisence filtering)
     */
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    public sealed class EditorBrowsableAttribute : Attribute
    {

        private EditorBrowsableState browsableState;

        public EditorBrowsableAttribute() : this(EditorBrowsableState.Always) { }

        public EditorBrowsableAttribute(EditorBrowsableState state)
        {
            this.browsableState = state;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            EditorBrowsableAttribute attribute1 = obj as EditorBrowsableAttribute;
            if (attribute1 != null)
            {
                return (attribute1.browsableState == this.browsableState);
            }

            return false;
        }

        public EditorBrowsableState State
        {
            get
            {
                return this.browsableState;
            }
        }

    }
}


