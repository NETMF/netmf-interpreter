////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{

    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    public struct Char
    {
        //
        // Member Variables
        //
        internal char m_value;

        //
        // Public Constants
        //
        /**
         * The maximum character value.
         */
        public const char MaxValue = (char)0xFFFF;
        /**
         * The minimum character value.
         */
        public const char MinValue = (char)0x00;

        public override String ToString()
        {
            return new String(m_value, 1);
        }

        public char ToLower()
        {
            if('A' <= m_value && m_value <= 'Z')
            {
                return (char)(m_value - ('A' - 'a'));
            }

            return m_value;
        }

        public char ToUpper()
        {
            if('a' <= m_value && m_value <= 'z')
            {
                return (char)(m_value + ('A' - 'a'));
            }

            return m_value;
        }
    }
}


