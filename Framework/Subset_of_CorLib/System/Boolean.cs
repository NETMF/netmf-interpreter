////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System;
    using System.Runtime.CompilerServices;

    /**
     * A place holder class for boolean.
     * @author Jay Roxe (jroxe)
     * @version
     */
    [Serializable]
    public struct Boolean
    {
        public static readonly string FalseString = "False";
        public static readonly string TrueString = "True";

        private bool m_value;

        public override String ToString()
        {
            return (m_value) ? TrueString : FalseString;
        }

    }
}


