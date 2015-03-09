////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#define ENABLE_CROSS_APPDOMAIN
#define ENABLE_CROSS_APPDOMAIN
namespace System.Globalization
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Reflection;
    using System.Resources;
    public class CultureInfo /*: ICloneable , IFormatProvider*/ {
        internal NumberFormatInfo numInfo = null;
        internal DateTimeFormatInfo dateTimeInfo = null;
        internal string m_name = null;
        internal ResourceManager m_rm;
        [NonSerialized]
        private CultureInfo m_parent;
        const string c_ResourceBase = "System.Globalization.Resources.CultureInfo";
        internal string EnsureStringResource(ref string str, System.Globalization.Resources.CultureInfo.StringResources id)
        {
            if (str == null)
            {
                str = (string)ResourceManager.GetObject(m_rm, id);
            }

            return str;
        }

        internal string[] EnsureStringArrayResource(ref string[] strArray, System.Globalization.Resources.CultureInfo.StringResources id)
        {
            if (strArray == null)
            {
                string str = (string)ResourceManager.GetObject(m_rm, id);
                strArray = str.Split('|');
            }

            return (string[])strArray.Clone();
        }

        public CultureInfo(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            m_rm = new ResourceManager(c_ResourceBase, typeof(CultureInfo).Assembly, name, true);
            m_name = m_rm.m_cultureName;
        }

        internal CultureInfo(ResourceManager resourceManager)
        {
            m_rm = resourceManager;
            m_name = resourceManager.m_cultureName;
        }

        public static CultureInfo CurrentUICulture
        {
            get
            {
                //only one system-wide culture.  We do not currently support per-thread cultures
                CultureInfo culture = CurrentUICultureInternal;
                if (culture == null)
                {
                    culture = new CultureInfo("");
                    CurrentUICultureInternal = culture;
                }

                return culture;
            }
        }

        private extern static CultureInfo CurrentUICultureInternal
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        public virtual CultureInfo Parent
        {
            get
            {
                if (m_parent == null)
                {
                    if (m_name == "") //Invariant culture
                    {
                        m_parent = this;
                    }
                    else
                    {
                        string parentName = m_name;
                        int iDash = m_name.LastIndexOf('-');
                        if (iDash >= 0)
                        {
                            parentName = parentName.Substring(0, iDash);
                        }
                        else
                        {
                            parentName = "";
                        }

                        m_parent = new CultureInfo(parentName);
                    }
                }

                return m_parent;
            }
        }

        public static CultureInfo[] GetCultures(CultureTypes types)
        {
            ArrayList listCultures = new ArrayList();
            //Look for all assemblies/satellite assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
            {
                Assembly assembly = assemblies[iAssembly];
                string mscorlib = "mscorlib";
                string fullName = assembly.FullName;
                // consider adding startswith ?
                if ((mscorlib.Length <= fullName.Length) && (fullName.Substring(0, mscorlib.Length) == mscorlib))
                {
                    string[] resources = assembly.GetManifestResourceNames();
                    for (int iResource = 0; iResource < resources.Length; iResource++)
                    {
                        string resource = resources[iResource];
                        string ciResource = c_ResourceBase;
                        if (ciResource.Length < resource.Length && resource.Substring(0, ciResource.Length) == ciResource)
                        {
                            //System.Globalization.Resources.CultureInfo.<culture>.tinyresources
                            string cultureName = resource.Substring(ciResource.Length, resource.Length - ciResource.Length - System.Resources.ResourceManager.s_fileExtension.Length);
                            // remove the leading "."
                            if (cultureName != "")
                            {
                                cultureName = cultureName.Substring(1, cultureName.Length - 1);
                            }

                            // if GetManifestResourceNames() changes, we need to change this code to ensure the index is the same.
                            listCultures.Add(new CultureInfo(new ResourceManager(c_ResourceBase, cultureName, iResource, typeof(CultureInfo).Assembly, assembly)));
                        }
                    }
                }
            }

            return (CultureInfo[])listCultures.ToArray(typeof(CultureInfo));
        }

        public virtual String Name
        {
            get
            {
                return m_name;
            }
        }

        public override String ToString()
        {
            return m_name;
        }

//        public virtual Object GetFormat(Type formatType) {
//            if (formatType == typeof(NumberFormatInfo)) {
//                return (NumberFormat);
//            }
//            if (formatType == typeof(DateTimeFormatInfo)) {
//                return (DateTimeFormat);
//            }
//            return (null);
//        }

//        internal static void CheckNeutral(CultureInfo culture) {
//            if (culture.IsNeutralCulture) {
//                    BCLDebug.Assert(culture.m_name != null, "[CultureInfo.CheckNeutral]Always expect m_name to be set");
//                    throw new NotSupportedException(
//                                    Environment.GetResourceString("Argument_CultureInvalidFormat",
//                                    culture.m_name));
//            }
//        }

//        [System.Runtime.InteropServices.ComVisible(false)]
//        public CultureTypes CultureTypes
//        {
//            get
//            {
//                CultureTypes types = 0;

//                if (m_cultureTableRecord.IsNeutralCulture)
//                    types |= CultureTypes.NeutralCultures;
//                else 
//                    types |= CultureTypes.SpecificCultures;

//                if (m_cultureTableRecord.IsSynthetic)
//                    types |= CultureTypes.WindowsOnlyCultures | CultureTypes.InstalledWin32Cultures; // Synthetic is installed culture too.
//                else
//                {
//                    // Not Synthetic
//                    if (CultureTable.IsInstalledLCID(cultureID)) 
//                        types |= CultureTypes.InstalledWin32Cultures;
                        
//                    if (!m_cultureTableRecord.IsCustomCulture || m_cultureTableRecord.IsReplacementCulture)
//                        types |= CultureTypes.FrameworkCultures;
//                }

//                if (m_cultureTableRecord.IsCustomCulture)
//                {
//                    types |= CultureTypes.UserCustomCulture;

//                    if (m_cultureTableRecord.IsReplacementCulture)
//                        types |= CultureTypes.ReplacementCultures;
//                }


//                return types;
//            }
//        }

        public virtual NumberFormatInfo NumberFormat {
            get {

                if(numInfo == null)
                {
                    numInfo = new NumberFormatInfo(this);
                }

                return numInfo;
            }
        }

        public virtual DateTimeFormatInfo DateTimeFormat
        {
            get
            {
                if (dateTimeInfo == null)
                {
                    dateTimeInfo = new DateTimeFormatInfo(this);
                }

                return dateTimeInfo;
            }
        }
    }
}


