////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Runtime.CompilerServices
namespace System.Runtime.CompilerServices
{

    using System;

    [Serializable]
    enum CompilationRelaxations
    {
        ImpreciseException      = 0x0001,
        ImpreciseFloat          = 0x0002,
        ImpreciseAssign         = 0x0004
    };

    [Serializable, AttributeUsage(AttributeTargets.Module)]
    public class CompilationRelaxationsAttribute : Attribute
    {
        private int m_relaxations;      // The relaxations.

        public CompilationRelaxationsAttribute (
            int relaxations)
        {
            m_relaxations = relaxations;
        }

        public int CompilationRelaxations
        {
            get
            {
                return m_relaxations;
            }
        }
    }

}


