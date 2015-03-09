using System;
using System.IO;
using System.Diagnostics;
using CorDebugInterop;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Microsoft.SPOT.Debugger
{   
    internal struct TargetFramework
    {
        public uint m_version;
        public string m_directory;
        public string m_description;

        public TargetFramework(uint version, string directory, string description)
        {
            m_version = version;
            m_directory = directory;
            m_description = description;
        }
    }

    [ComVisible(true)]
    internal class NetMFMultiTargeting : IVsFrameworkMultiTargeting
    {
        TargetFrameworkAssemblies m_frameworks;
        const string c_frameIdMicro           = ".NETMicroFramework";
        const string c_frameIdDeskCompatible  = ".NETFramework,Version=v2.0";
        VsProject    m_project;


        internal NetMFMultiTargeting(VsProject project)
        {
            m_frameworks = new TargetFrameworkAssemblies(project);
            m_project = project;
        }

        internal NetMFMultiTargeting()
        {
        }

        #region IVsFrameworkMultiTargeting Members

        public int CheckFrameworkCompatibility(string pwszTargetFrameworkMonikerSource, string pwszTargetFrameworkMonikerTarget, out uint pdwCompat)
        {
            pdwCompat = 1;
            
            if(pwszTargetFrameworkMonikerTarget.StartsWith(c_frameIdMicro))
            {
                if(pwszTargetFrameworkMonikerSource.StartsWith(c_frameIdMicro) ||
                   pwszTargetFrameworkMonikerSource == c_frameIdDeskCompatible)
                {
                    pdwCompat = 0;
                }
            }
            
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetDisplayNameForTargetFx(string pwszTargetFrameworkMoniker, out string pbstrDisplayName)
        {
            pbstrDisplayName = pwszTargetFrameworkMoniker;

            Regex exp = new Regex("Version=v(\\d+.\\d+)", RegexOptions.IgnoreCase);

            Match match = exp.Match(pwszTargetFrameworkMoniker);

            if(match.Success)
            {
                pbstrDisplayName = ".NET Micro Framework " + match.Groups[1].Value;
            }

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetFrameworkAssemblies(string pwszTargetFrameworkMoniker, uint atAssemblyType, out Array prgAssemblyPaths)
        {
            prgAssemblyPaths = null;

            List<string> paths = new List<string>();

            string dbgPath = Environment.GetEnvironmentVariable("SPOCLIENT");

            if(!string.IsNullOrEmpty(dbgPath) && Directory.Exists(dbgPath))
            {
                string path = Path.Combine(dbgPath, @"buildoutput\public\debug\client\dll\");

                if(Directory.Exists(path))
                {
                    paths.Add(path);
                }
                else
                {
                    path = Path.Combine(dbgPath, @"buildoutput\public\release\client\dll\");

                    if(Directory.Exists(path))
                    {
                        paths.Add(path);
                    }
                }
            }
            else
            {
                // TODO:  GET  PATH FROM REFERENCE ASSEMBLIES OR REGISTRY
            }

            prgAssemblyPaths = paths.ToArray();

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetInstallableFrameworkForTargetFx(string pwszTargetFrameworkMoniker, out string pbstrInstallableFrameworkMoniker)
        {
            pbstrInstallableFrameworkMoniker = "http://www.netmf.com";
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetSupportedFrameworks(out Array prgSupportedFrameworks)
        {
            prgSupportedFrameworks = null;

            string frameworkBase = Environment.GetEnvironmentVariable("ProgramFiles");

            frameworkBase = Path.Combine( frameworkBase, @"Reference Assemblies\Microsoft\Framework\" + c_frameIdMicro );

            if(!Directory.Exists(frameworkBase)) return Microsoft.VisualStudio.VSConstants.E_FAIL;

            string frameworkRedist = @"RedistList\FrameworkList.xml";

            List<string> supportedVersions = new List<string>();

            foreach(string verDir in Directory.GetDirectories(frameworkBase))
            {
                System.Version ver = Utility.VersionFromPropertyString(Path.GetFileName(verDir));

                if(m_project != null && m_project is VsProjectVisualBasic && (ver.Major < 4 || (ver.Major == 4 && ver.Minor < 2)))
                {
                    continue;
                }
                
                if(File.Exists(Path.Combine(verDir, frameworkRedist)))
                {
                    supportedVersions.Add( string.Format("{0},Version={1}", c_frameIdMicro, Path.GetFileName(verDir)) );
                }
            }

            if(supportedVersions.Count == 0)
            {
                return Microsoft.VisualStudio.VSConstants.E_FAIL;
            }

            // HACK - to get the New Project Wizard to show our templates we need to support one of the
            // standard framework versions.  Since we are based off of the v2.0 framework I choose
            // .NETFramework,v2.0 
            supportedVersions.Add(c_frameIdDeskCompatible);
            
            prgSupportedFrameworks = supportedVersions.ToArray();

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetTargetFramework(string pwszAssemblyPath, string pwszTargetFrameworkIdentifier, out string pbstrTargetFrameworkMoniker)
        {
            if(pwszTargetFrameworkIdentifier != c_frameIdMicro) // || c_frameIdDeskCompatible.Contains(pwszTargetFrameworkIdentifier))
            {
                pbstrTargetFrameworkMoniker = null;
                return Microsoft.VisualStudio.VSConstants.E_FAIL;
            }

            string path = pwszAssemblyPath.ToLower();
            string ver = "v4.3";

            Regex exp = new Regex("\\(v\\d+.\\d+)\\", RegexOptions.IgnoreCase);

            Match match = exp.Match(pwszAssemblyPath);

            if(match.Success)
            {
                ver = match.Groups[1].Value;
            }


            pbstrTargetFrameworkMoniker = string.Format("{0},Version={1}", c_frameIdMicro, ver);
            
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int IsReferenceableInTargetFx(string pwszAssemblySpec, string pwszTargetFrameworkMoniker, out bool pbIsReferenceable)
        {
            if(pwszTargetFrameworkMoniker.StartsWith(c_frameIdMicro))
            {
                pbIsReferenceable = true;
            }
            else
            {
                pbIsReferenceable = false;
            }
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int ResolveAssemblyPath(string pwszAssemblySpec, string pwszTargetFrameworkMoniker, out string pbstrResolvedAssemblyPath)
        {
            string dbgPath = Environment.GetEnvironmentVariable("SPOCLIENT");
            string dllPath = "";

            if(!string.IsNullOrEmpty(dbgPath) && Directory.Exists(dbgPath))
            {
                string path = Path.Combine(dbgPath, @"buildoutput\public\debug\client\dll\");

                if(Directory.Exists(path))
                {
                    dllPath = path;
                }
                else
                {
                    path = Path.Combine(dbgPath, @"buildoutput\public\release\client\dll\");

                    if(Directory.Exists(path))
                    {
                        dllPath = path;
                    }
                }
            }
            else
            {
                // TODO: GET PATH FROM REFERENCE ASSEMBLIES OR REGISTRY
            }

            pbstrResolvedAssemblyPath = dllPath;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int ResolveAssemblyPathsInTargetFx(string pwszTargetFrameworkMoniker, Array prgAssemblySpecs, uint cAssembliesToResolve, VsResolvedAssemblyPath[] prgResolvedAssemblyPaths, out uint pcResolvedAssemblyPaths)
        {
            //var Assemblies = new List<string>(assemblySpecs.Length);

            pcResolvedAssemblyPaths = 0;

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        #endregion
    }

    /// <summary>
    /// Visual Studio service for enumerating the installed .NET frameworks
    /// See vsproject\compsvcspkg\TargetFrameworkAssemblies.{cpp,h} in the Visual Studio
    /// depot for the original implementation.
    /// 
    /// In this minimal implementation, all assemblies are treated as "user assemblies," as
    /// opposed to "system assemblies"
    /// </summary>
    [ComVisible(true)]
    internal class TargetFrameworkAssemblies : IVsTargetFrameworkAssemblies
    {
        VsProject m_project;
        List<TargetFramework> m_frameworks;
        

        public TargetFrameworkAssemblies(VsProject project)
        {
            int er;

            m_project = project;

            try
            {
                string netMfTargetsBaseDir = null;
                er = m_project.InnerBuildPropertyStorage.GetPropertyValue(@"NetMfTargetsBaseDir",
                            String.Empty,
                            (uint)_PersistStorageType.PST_PROJECT_FILE, out netMfTargetsBaseDir);

                if (er == Microsoft.VisualStudio.VSConstants.S_OK)
                {
                    m_frameworks = BasedTargetFrameworks(netMfTargetsBaseDir);
                }
                else
                {
                    VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("\"NetMfTargetsBaseDir\" property not available from build system (err = {0:X})", er));
                }

                if (m_frameworks == null)
                {
                    string msBuildExtensionsPath = null;
                    er = m_project.InnerBuildPropertyStorage.GetPropertyValue(@"MSBuildExtensionsPath",
                        String.Empty,
                        (uint)_PersistStorageType.PST_PROJECT_FILE, out msBuildExtensionsPath);

                    if (er == Microsoft.VisualStudio.VSConstants.S_OK)
                    {
                        m_frameworks = BasedTargetFrameworks(Path.Combine(msBuildExtensionsPath, @"Microsoft\.NET Micro Framework"));
                    }
                    else
                    {
                        VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("\"MSBuildExtensionsPath\" property not available from build system (err = {0:X})", er));
                    }
                }
            }
            catch (Exception ex)
            {
                VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("Error while getting list of possible target framework property (exception code was {0}).", ex.Message));
            }
        }

        private List<TargetFramework> BasedTargetFrameworks(string baseDir)
        {
            List<TargetFramework> fxList = new List<TargetFramework>();

            DirectoryInfo mfTgtFxBaseDi = new DirectoryInfo(baseDir);

            foreach (DirectoryInfo verDi in mfTgtFxBaseDi.GetDirectories("v*"))
            {
                System.Version ver = Utility.VersionFromPropertyString(verDi.Name);

                if(m_project is VsProjectVisualBasic && (ver.Major < 4 || (ver.Major == 4 && ver.Minor < 2)))
                {
                    continue;
                }
                
                FileInfo deviceTargetFi = new FileInfo(Path.Combine(verDi.FullName, @"Device.targets"));
                if (deviceTargetFi.Exists)
                    fxList.Add(new TargetFramework(Utility.VersionToUint(ver),
                                                   verDi.Name,
                                                   @".NET Micro Framework " + verDi.Name.Substring(1)));
            }

            return fxList;
        }

        internal List<TargetFramework> SupportedFrameworks
        {
            get { return m_frameworks; }
        }

        #region IVsTargetFrameworkAssemblies Members

        int IVsTargetFrameworkAssemblies.GetSupportedFrameworks(out IEnumTargetFrameworks pTargetFrameworks)
        {
            pTargetFrameworks = new EnumTargetFrameworks(m_frameworks);
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        int IVsTargetFrameworkAssemblies.GetTargetFrameworkDescription(uint targetVersion, out string pszDescription)
        {
            pszDescription = null;
            foreach (TargetFramework framework in m_frameworks)
            {
                if (framework.m_version == targetVersion)
                {
                    pszDescription = framework.m_description;
                    return Microsoft.VisualStudio.VSConstants.S_OK;
                }
            }
            return Microsoft.VisualStudio.VSConstants.E_INVALIDARG;
        }

        /// <summary>
        /// returns the list of the system assembly references for the given version of the framework
        /// </summary>
        /// <param name="targetVersion"></param>
        /// <param name="pAssemblies"></param>
        /// <returns></returns>
        int IVsTargetFrameworkAssemblies.GetSystemAssemblies(uint targetVersion, out IEnumSystemAssemblies pAssemblies)
        {
            pAssemblies = null;

            foreach (TargetFramework framework in m_frameworks)
            {
                if (framework.m_version == targetVersion)
                {
                    pAssemblies = new EnumSystemAssemblies(framework);
                    return Microsoft.VisualStudio.VSConstants.S_OK;
                }
            }
            return Microsoft.VisualStudio.VSConstants.E_INVALIDARG;
        }

        #region Stubs
        /// <summary>
        /// Get the required target framework version for the given assembly. This function will succeed
        /// only for the assemblies we explicitely know what that version is - i.e the system assemblies.
        /// For the user assemblies (that we dont know about) that will return TARGETFRAMEWORKVERSION_UNKNOWN
        /// </summary>
        /// <param name="szAssemblyFile"></param>
        /// <param name="pTargetTargetFramework"></param>
        /// <returns></returns>
        int IVsTargetFrameworkAssemblies.GetRequiredTargetFrameworkVersion(string szAssemblyFile, out uint pTargetTargetFramework)
        {
            Debug.Assert(false, "IVsTargetFrameworkAssemblies.GetRequiredTargetFrameworkVersion is not implemented.");
            VsPackage.MessageCentre.InternalErrorMsg("stub: IVsTargetFrameworkAssemblies.GetRequiredTargetFrameworkVersion");

            pTargetTargetFramework = (uint)WellKnownTargetFrameworkVersions.TargetFrameworkVersion_Unknown;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Get the target framework version for the assembly. If we dont know for a fact what that is
        /// this method will try to guess the version by following the assembly dependecy tree (recursively) and
        /// get the maximum framework version required by any knownn assembly in te reference closure.
        /// </summary>
        /// <param name="szAssemblyFile"></param>
        /// <param name="pTargetTargetFramework"></param>
        /// <returns></returns>
        int IVsTargetFrameworkAssemblies.GetRequiredTargetFrameworkVersionFromDependency(string szAssemblyFile, out uint pTargetTargetFramework)
        {
            Debug.Assert(false, "IVsTargetFrameworkAssemblies.GetRequiredTargetFrameworkVersionFromDependency is not implemented.");
            VsPackage.MessageCentre.InternalErrorMsg("stub: IVsTargetFrameworkAssemblies.GetRequiredTargetFrameworkVersionFromDependency");

            pTargetTargetFramework = (uint)WellKnownTargetFrameworkVersions.TargetFrameworkVersion_Unknown;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Check if givven assemly file is system assembly (part of the .NET distribution
        /// if yes the method also return the min fx version that cary on that assembly.
        /// </summary>
        /// <param name="szAssemblyFile"></param>
        /// <param name="pIsSystem"></param>
        /// <param name="pTargetTargetFramework"></param>
        /// <returns></returns>
        int IVsTargetFrameworkAssemblies.IsSystemAssembly(string szAssemblyFile, out int pIsSystem, out uint pTargetTargetFramework)
        {
            Debug.Assert(false, "IVsTargetFrameworkAssemblies.IsSystemAssembly is not implemented.");
            VsPackage.MessageCentre.InternalErrorMsg("stub: IVsTargetFrameworkAssemblies.IsSystemAssembly");

            pIsSystem = 0;
            pTargetTargetFramework = (uint)WellKnownTargetFrameworkVersions.TargetFrameworkVersion_Unknown;
            return VSConstants.S_OK;
        }
        #endregion
        #endregion
    }

    #region EnumTargetFrameworks
    /// <summary>
    /// Helper class for TargetFrameworkAssemblies::GetTargetFrameworks
    /// Provides an enumerator over the frameworks installed
    /// </summary>
    internal class EnumTargetFrameworks : IEnumTargetFrameworks
    {
        List<TargetFramework> m_frameworks;
        int m_currentPos;

        public EnumTargetFrameworks(List<TargetFramework> frameworks)
        {
            m_frameworks = frameworks;
            m_currentPos = 0;
        }

        #region IEnumTargetFrameworks Members

        public int Clone(out IEnumTargetFrameworks ppIEnumComponents)
        {
            ppIEnumComponents = new EnumTargetFrameworks(m_frameworks);
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int Count(out uint pCount)
        {
            pCount = (uint)m_frameworks.Count;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int Next(uint celt, uint[] rgFrameworks, out uint pceltFetched)
        {
            uint numberReturned = 0;

            for (uint i = 0; i < celt; i++)
            {
                rgFrameworks[i] = 0;
            }
            pceltFetched = 0;

            while (m_currentPos < m_frameworks.Count && celt > 0)
            {
                rgFrameworks[numberReturned] = m_frameworks[m_currentPos++].m_version;
                numberReturned++;
                celt--;
            }
            pceltFetched = numberReturned;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int Reset()
        {
            m_currentPos = 0;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            m_currentPos += (int)celt;
            if (m_currentPos > m_frameworks.Count)
            {
                m_currentPos = m_frameworks.Count;
            }
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        #endregion
    }
    #endregion

    #region EnumSystemAssemblies Stub
    /// <summary>
    /// Helper class for TargetFrameworkAssemblies::GetSystemAssemblies
    /// Provides an enumerator over the system assemblies included in a framework.
    /// Stub that is hardcoded to return 0 assemblies.
    /// </summary>
    internal class EnumSystemAssemblies : IEnumSystemAssemblies
    {
        TargetFramework m_framework;

        internal EnumSystemAssemblies(TargetFramework framework)
        {
            m_framework = framework;
        }

        #region IEnumSystemAssemblies Members

        public int Clone(out IEnumSystemAssemblies ppIEnumComponents)
        {
            ppIEnumComponents = new EnumSystemAssemblies(m_framework);
            return VSConstants.S_OK;
        }

        public int Count(out uint pCount)
        {
            Debug.Assert(false, "IEnumSystemAssemblies.Count is not implemented.");
            VsPackage.MessageCentre.InternalErrorMsg("stub: IEnumSystemAssemblies.Count");

            pCount = 0;
            return VSConstants.S_OK;
        }

        public int Next(uint celt, string[] rgAssemblies, out uint pceltFetched)
        {
            for (int i = 0; i < celt; i++)
            {
                rgAssemblies[i] = null;
            }

            Debug.Assert(false, "IEnumSystemAssemblies.Next is not implemented.");
            VsPackage.MessageCentre.InternalErrorMsg("stub: IEnumSystemAssemblies.Next");

            pceltFetched = 0;
            return VSConstants.S_OK;
        }

        public int Reset()
        {
            Debug.Assert(false, "IEnumSystemAssemblies.Reset is not implemented.");
            VsPackage.MessageCentre.InternalErrorMsg("stub: IEnumSystemAssemblies.Reset");

            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            Debug.Assert(false, "IEnumSystemAssemblies.Skip is not implemented.");
            VsPackage.MessageCentre.InternalErrorMsg("stub: IEnumSystemAssemblies.Next");

            return VSConstants.S_OK;
        }

        #endregion
    }
    #endregion
}
