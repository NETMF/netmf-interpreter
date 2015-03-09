using System;
using System.IO;
using System.Diagnostics;
using CorDebugInterop;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.Win32;
using System.Collections.Specialized;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.SPOT.Debugger
{
    [ComVisible(false)]
    public class VsProjectVisualBasic : VsProject, IVbCompilerHost
    {
        // The Guid {bb063c12-22d6-4e50-a55f-f678d783e61d} needs to match ProjectGuidVisualBasic defined in WiXRegistryInclude.wxs
        [Guid("bb063c12-22d6-4e50-a55f-f678d783e61d")]    
        public class ProjectFactory : VsProjectFactory
        {
            public ProjectFactory(VsPackage package) : base(package)
            {
            }

            protected override VsProject CreateInstance()
            {
                return new VsProjectVisualBasic();
            }
        }

        protected override string ProjectNodeIconResource
        {
            get { return "Resources.Icons.VisualBasicProject.ico"; }
        }

        protected override Guid AddItemTemplatesGuid
        {
            get { return typeof(ProjectFactory).GUID; }
        }

        #region IVbCompilerHost Members

        int IVbCompilerHost.OutputString(string @string)
        {
            this.Package.GetOutputPane(Utility.VsConstants.VisualStudioShell.BuildOutput, null).OutputStringThreadSafe(@string);

            return Utility.COM_HResults.S_OK;   
        }

        int IVbCompilerHost.GetSdkPath(out string pSdkPath)
        {
            pSdkPath = null;

            //cannot be swapped after load?
            //cannot be determined from active config, at load??
            string frameworkVersion;
            m_innerIVsBuildPropertyStorage.GetPropertyValue(MSBuildProperties.TargetFrameworkVersion, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out frameworkVersion);

            PlatformInfo pi = new PlatformInfo(frameworkVersion);
            pSdkPath = pi.FrameworkAssembliesPath;

            return Utility.COM_HResults.S_OK;   
        }

        int IVbCompilerHost.GetTargetLibraryType(out __MIDL___MIDL_itf_CorDebugInterop_1054_0004 pTargetLibraryType)
        {
            //We need to set /netcf on the compile flag.  This is how to set it for the hosted compiler.
            pTargetLibraryType = __MIDL___MIDL_itf_CorDebugInterop_1054_0004.TLB_Starlite;
            return Utility.COM_HResults.S_OK;            
        }

        #endregion
    }
}