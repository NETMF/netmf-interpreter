////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Reflection
{

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyCopyrightAttribute : Attribute
    {
        private String m_copyright;

        public AssemblyCopyrightAttribute(String copyright)
        {
            m_copyright = copyright;
        }

        public String Copyright
        {
            get { return m_copyright; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyTrademarkAttribute : Attribute
    {
        private String m_trademark;

        public AssemblyTrademarkAttribute(String trademark)
        {
            m_trademark = trademark;
        }

        public String Trademark
        {
            get { return m_trademark; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyProductAttribute : Attribute
    {
        private String m_product;

        public AssemblyProductAttribute(String product)
        {
            m_product = product;
        }

        public String Product
        {
            get { return m_product; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyCompanyAttribute : Attribute
    {
        private String m_company;

        public AssemblyCompanyAttribute(String company)
        {
            m_company = company;
        }

        public String Company
        {
            get { return m_company; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyDescriptionAttribute : Attribute
    {
        private String m_description;

        public AssemblyDescriptionAttribute(String description)
        {
            m_description = description;
        }

        public String Description
        {
            get { return m_description; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyTitleAttribute : Attribute
    {
        private String m_title;

        public AssemblyTitleAttribute(String title)
        {
            m_title = title;
        }

        public String Title
        {
            get { return m_title; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyConfigurationAttribute : Attribute
    {
        private String m_configuration;

        public AssemblyConfigurationAttribute(String configuration)
        {
            m_configuration = configuration;
        }

        public String Configuration
        {
            get { return m_configuration; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyDefaultAliasAttribute : Attribute
    {
        private String m_defaultAlias;

        public AssemblyDefaultAliasAttribute(String defaultAlias)
        {
            m_defaultAlias = defaultAlias;
        }

        public String DefaultAlias
        {
            get { return m_defaultAlias; }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyInformationalVersionAttribute : Attribute
    {
        private String m_informationalVersion;

        public AssemblyInformationalVersionAttribute(String informationalVersion)
        {
            m_informationalVersion = informationalVersion;
        }

        public String InformationalVersion
        {
            get { return m_informationalVersion; }
        }
    }
}


