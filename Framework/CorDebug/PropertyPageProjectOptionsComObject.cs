using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

using Microsoft.VisualStudio.Editors.PropertyPages;
using System.Windows.Forms;
using System.Drawing;

namespace Microsoft.SPOT.Debugger
{
    // The Guid {AFBAC3D0-7526-4d80-9F42-2A975120BB5F} needs to match PropertyPageCLSID defined in WiXRegistryInclude.wxs
    [ComVisible(true), Guid("AFBAC3D0-7526-4d80-9F42-2A975120BB5F")]
    public class PropertyPageComObject : PropPageBase
    {
        public PropertyPageComObject()
        {
            //By default, the base designer class finds the default size by
            //looking for $this.size and $this.ClientSize in managed resources
            //associated with the WinForm control ControlType
            //In order to avoid (at least temporarily) the need for managed resources
            //for this assembly, we are just setting the default size explicitly.

            this.DefaultSize = new Size( 614, 364 );
        }

        protected override Type ControlType
        {
            get { return typeof(PropertyPageProjectOptions); }
        }

        protected override string Title
        {
            get { return ".NET Micro Framework"; }
        }

        protected override Control CreateControl()
        {            
            return new PropertyPageProjectOptions();
        }
    }    
}
