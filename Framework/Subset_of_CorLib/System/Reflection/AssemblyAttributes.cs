////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Reflection
{

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyCultureAttribute : Attribute
    {
        private String m_culture;

        public AssemblyCultureAttribute(String culture)
        {
            m_culture = culture;
        }

        public String Culture
        {
            get { return m_culture; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyVersionAttribute : Attribute
    {
        private String m_version;

        public AssemblyVersionAttribute(String version)
        {
            m_version = version;
        }

    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyKeyFileAttribute : Attribute
    {
        private String m_keyFile;

        public AssemblyKeyFileAttribute(String keyFile)
        {
            m_keyFile = keyFile;
        }

        public String KeyFile
        {
            get { return m_keyFile; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyKeyNameAttribute : Attribute
    {
        private String m_keyName;

        public AssemblyKeyNameAttribute(String keyName)
        {
            m_keyName = keyName;
        }

        public String KeyName
        {
            get { return m_keyName; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyDelaySignAttribute : Attribute
    {
        private bool m_delaySign;

        public AssemblyDelaySignAttribute(bool delaySign)
        {
            m_delaySign = delaySign;
        }

        public bool DelaySign
        {
            get
            { return m_delaySign; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyFlagsAttribute : Attribute
    {
        private AssemblyNameFlags m_flags;

        [CLSCompliant(false)]
        public AssemblyFlagsAttribute(uint flags)
        {
            m_flags = (AssemblyNameFlags)flags;
        }

        [CLSCompliant(false)]
        public uint Flags
        {
            get { return (uint)m_flags; }
        }

        public AssemblyFlagsAttribute(AssemblyNameFlags assemblyFlags)
        {
            m_flags = assemblyFlags;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class AssemblyFileVersionAttribute : Attribute
    {
        private String _version;

        public AssemblyFileVersionAttribute(String version)
        {
            if (version == null)
                throw new ArgumentNullException("version");
            _version = version;
        }

        public String Version
        {
            get { return _version; }
        }
    }
}


