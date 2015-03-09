using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Flavor;

namespace Microsoft.SPOT.Debugger
{
    public abstract class VsProjectFactory : Microsoft.VisualStudio.Shell.Flavor.FlavoredProjectFactoryBase
    {
        private VsPackage m_package;

        public VsProjectFactory(VsPackage package)
        {
            m_package = package;
        }

        protected abstract VsProject CreateInstance();

        protected override object PreCreateForOuter(IntPtr outerProject )
        {   
            VsProject project = CreateInstance();

            project.Package = m_package;

            return project;
        }
    }    
}
