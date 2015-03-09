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
    public class VsProjectCSharp : VsProject
    {
        // The Guid {b69e3092-b931-443c-abe7-7e7b65f2a37f} needs to match ProjectGuidCSharp defined in WiXRegistryInclude.wxs
        [Guid("b69e3092-b931-443c-abe7-7e7b65f2a37f")]
        public class ProjectFactory : VsProjectFactory
        {
            public ProjectFactory(VsPackage package)
                : base(package)
            {
            }

            protected override VsProject CreateInstance()
            {
                return new VsProjectCSharp();
            }
        }

        protected override string ProjectNodeIconResource
        {
            get { return "Resources.Icons.CSharpProject.ico"; }
        }

        protected override Guid AddItemTemplatesGuid
        {
            get { return typeof(ProjectFactory).GUID; }
        }
    }
}